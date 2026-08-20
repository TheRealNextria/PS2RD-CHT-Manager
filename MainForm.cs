using System.ComponentModel;
using System.Text;

namespace PS2RDCHTManager;

public partial class MainForm : Form
{
    private AnalysisReport? _report;
    private readonly BindingSource _cheatSource = new();
    private bool _updatingEditor;
    private string _gameTitle = string.Empty;

    public MainForm()
    {
		InitializeComponent();
		try
    {
		this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
    }
		catch
    {
        // Ignore if icon is unavailable.
    }

		this.ShowIcon = true;
		this.ShowInTaskbar = true;
	
        _cheatSource.DataSource = new BindingList<CheatEntry>();
        lstCheats.DisplayMember = nameof(CheatEntry.Name);
        lstCheats.DataSource = _cheatSource;

        // Start with one editable cheat so the user can type immediately.
        var firstCheat = new CheatEntry { Name = "Cheat 1" };
        Cheats.Add(firstCheat);
        _cheatSource.ResetBindings(false);
        lstCheats.SelectedItem = firstCheat;
        LoadSelectedCheat();

        UpdateCheatButtons();
        UpdateValidation();
    }

    private BindingList<CheatEntry> Cheats => (BindingList<CheatEntry>)_cheatSource.DataSource!;

    private void btnOpenElf_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open PS2 ELF",
            Filter = "PS2 ELF / executable (*.*)|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        ResetCheatsForNewGame();
        _gameTitle = string.Empty;
        txtSerial.Clear();
        AnalyzeElf(dialog.FileName, null, dialog.FileName);
    }

    private void btnOpenIso_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open PlayStation 2 ISO",
            Filter = "PlayStation 2 ISO (*.iso)|*.iso|All files (*.*)|*.*",
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        ResetCheatsForNewGame();

        try
        {
            UseWaitCursor = true;
            _gameTitle = Path.GetFileNameWithoutExtension(dialog.FileName).Trim();
            lblStatus.Text = "Reading SYSTEM.CNF and boot ELF from ISO...";
            lblStatus.Update();
            Ps2IsoInfo iso = Ps2IsoReader.Read(dialog.FileName);
            txtSerial.Text = iso.Serial;
            EnsureHeaderEntry();
            AnalyzeElf($"{dialog.FileName}  ::  {iso.ElfPath}", iso.ElfData, dialog.FileName);
        }
        catch (Exception ex)
        {
            ClearAnalysis();
            txtSourcePath.Text = dialog.FileName;
            lblStatus.Text = "Could not read the PS2 ISO.";
            MessageBox.Show(this, ex.Message, "PS2RD Cheat Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    private void ResetCheatsForNewGame()
    {
        _updatingEditor = true;
        try
        {
            Cheats.Clear();

            var firstCheat = new CheatEntry { Name = "Cheat 1" };
            Cheats.Add(firstCheat);
            _cheatSource.ResetBindings(false);
            lstCheats.SelectedItem = firstCheat;
        }
        finally
        {
            _updatingEditor = false;
        }

        LoadSelectedCheat();
        UpdateCheatButtons();
        UpdateValidation();
        UpdateSaveButton();
    }

    private void AnalyzeElf(string displaySource, byte[]? elfData, string sourcePath)
    {
        try
        {
            UseWaitCursor = true;
            lblStatus.Text = "Analyzing ELF...";
            lblStatus.Update();
            _report = elfData is null ? Ps2ElfAnalyzer.Analyze(sourcePath) : Ps2ElfAnalyzer.Analyze(elfData);
            txtSourcePath.Text = displaySource;
            txtCrc.Text = _report.Crc.ToString("X8");
            listCandidates.Items.Clear();

            var found = _report.Results.Where(r => r.TargetAddress != 0).ToList();
            foreach (var result in found)
            {
                var item = new ListViewItem(result.Type);
                item.SubItems.Add($"9{result.TargetAddress:X7}");
                item.SubItems.Add($"{result.TargetData:X8}");
                item.Tag = result;
                listCandidates.Items.Add(item);
            }

            var preferred = found.FirstOrDefault(r => r.Type.Equals("sceSifSendCmd", StringComparison.OrdinalIgnoreCase)) ?? found.FirstOrDefault();
            if (preferred is not null)
            {
                SelectResult(preferred);
                foreach (ListViewItem item in listCandidates.Items)
                {
                    if (ReferenceEquals(item.Tag, preferred))
                    {
                        item.Selected = true; item.Focused = true; item.EnsureVisible(); break;
                    }
                }
                lblStatus.Text = preferred.Type.Equals("sceSifSendCmd", StringComparison.OrdinalIgnoreCase)
                    ? "Recommended PS2RD mastercode found (sceSifSendCmd)."
                    : "Mastercode found. sceSifSendCmd was not found; first candidate selected.";
            }
            else
            {
                txtSelected.Clear(); txtSource.Clear(); btnCopy.Enabled = false;
                lblStatus.Text = "No mastercode candidates found.";
            }
            UpdateSaveButton();
        }
        catch (Exception ex)
        {
            ClearAnalysis(keepSerial: true);
            txtSourcePath.Text = displaySource;
            lblStatus.Text = "Error while analyzing ELF.";
            MessageBox.Show(this, ex.Message, "PS2RD Cheat Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    private void ClearAnalysis(bool keepSerial = false)
    {
        _report = null; listCandidates.Items.Clear(); txtCrc.Clear(); txtSelected.Clear(); txtSource.Clear();
        if (!keepSerial) txtSerial.Clear();
        btnCopy.Enabled = false; UpdateSaveButton();
    }

    private void listCandidates_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listCandidates.SelectedItems.Count == 1 && listCandidates.SelectedItems[0].Tag is AnalysisResult result)
            SelectResult(result);
    }

    private void SelectResult(AnalysisResult result)
    {
        txtSelected.Text = $"9{result.TargetAddress:X7} {result.TargetData:X8}";
        txtSource.Text = result.Type;
        btnCopy.Enabled = true;
        EnsureMasterCodeEntry(txtSelected.Text);
        UpdateSaveButton();
    }

    private void btnCopy_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtSelected.Text))
        {
            Clipboard.SetText(txtSelected.Text);
            lblStatus.Text = "Mastercode copied to clipboard.";
        }
    }

    private void EnsureHeaderEntry()
    {
        if (string.IsNullOrWhiteSpace(_gameTitle) || string.IsNullOrWhiteSpace(txtSerial.Text))
            return;

        string headerText = $"\"{_gameTitle.Trim()} /ID {txtSerial.Text.Trim()}\"";

        CheatEntry? header = Cheats.FirstOrDefault(c => c.IsHeader);
        if (header is null)
        {
            header = new CheatEntry
            {
                Name = headerText,
                Codes = string.Empty,
                IsHeader = true
            };
            Cheats.Insert(0, header);
        }
        else
        {
            header.Name = headerText;
            header.Codes = string.Empty;

            int currentIndex = Cheats.IndexOf(header);
            if (currentIndex != 0)
            {
                Cheats.RemoveAt(currentIndex);
                Cheats.Insert(0, header);
            }
        }

        _cheatSource.ResetBindings(false);
        lstCheats.Invalidate();
        UpdateCheatButtons();
    }

    private void EnsureMasterCodeEntry(string masterCode)
    {
        if (string.IsNullOrWhiteSpace(masterCode)) return;

        CheatEntry? master = Cheats.FirstOrDefault(c => c.IsMasterCode);
        if (master is null)
        {
            master = new CheatEntry
            {
                Name = "Mastercode",
                Codes = masterCode.ToLowerInvariant(),
                IsMasterCode = true
            };
            int insertIndex = Cheats.Count > 0 && Cheats[0].IsHeader ? 1 : 0;
            Cheats.Insert(insertIndex, master);
        }
        else
        {
            master.Name = "Mastercode";
            master.Codes = masterCode.ToLowerInvariant();

            int targetIndex = Cheats.Count > 0 && Cheats[0].IsHeader ? 1 : 0;
            int currentIndex = Cheats.IndexOf(master);
            if (currentIndex != targetIndex)
            {
                Cheats.RemoveAt(currentIndex);
                Cheats.Insert(Math.Min(targetIndex, Cheats.Count), master);
            }
        }

        _cheatSource.ResetBindings(false);
        lstCheats.Invalidate();
        UpdateCheatButtons();
    }

    private static string BuildChtText(string gameTitle, string serial, string masterCode, IEnumerable<CheatEntry> cheats)
    {
        var sb = new StringBuilder();

        string title = string.IsNullOrWhiteSpace(gameTitle) ? serial : gameTitle.Trim();
        sb.Append('"').Append(title).Append(" /ID ").Append(serial.Trim()).AppendLine("\"");
        sb.AppendLine("Mastercode");
        sb.AppendLine(masterCode.Trim().ToLowerInvariant());

        foreach (CheatEntry cheat in cheats.Where(c => !c.IsMasterCode && !c.IsHeader))
        {
            string name = cheat.Name.Trim();
            string codes = cheat.Codes.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(codes))
                continue;

            sb.AppendLine();
            sb.AppendLine(name);

            if (!string.IsNullOrWhiteSpace(cheat.Description))
                sb.AppendLine("//" + cheat.Description.Trim());

            sb.AppendLine(codes);
        }

        return sb.ToString();
    }

    private void btnAddCheat_Click(object sender, EventArgs e)
    {
        SaveCurrentCheat();
        int normalCheatCount = Cheats.Count(c => !c.IsMasterCode && !c.IsHeader);
        var cheat = new CheatEntry { Name = $"Cheat {normalCheatCount + 1}" };
        Cheats.Add(cheat);
        _cheatSource.ResetBindings(false);
        lstCheats.SelectedItem = cheat;
        txtCheatName.Focus(); txtCheatName.SelectAll();
        UpdateCheatButtons();
    }

    private void btnRemoveCheat_Click(object sender, EventArgs e)
    {
        if (lstCheats.SelectedItem is not CheatEntry cheat) return;
        if (cheat.IsHeader)
        {
            MessageBox.Show(this, "The game header cannot be deleted.", "Delete Cheat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (cheat.IsMasterCode)
        {
            MessageBox.Show(this, "The automatically generated mastercode cannot be deleted.", "Delete Cheat", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int index = lstCheats.SelectedIndex;

        // Removing the selected item can make the ListBox visually select the
        // next item without raising SelectedIndexChanged again when the numeric
        // index stays the same. Suppress intermediate selection events and then
        // explicitly reload the final selected cheat so the editor can never
        // contain stale data from the deleted entry.
        _updatingEditor = true;
        try
        {
            Cheats.Remove(cheat);
            _cheatSource.ResetBindings(false);

            if (Cheats.Count > 0)
                lstCheats.SelectedIndex = Math.Min(index, Cheats.Count - 1);
            else
                lstCheats.SelectedIndex = -1;
        }
        finally
        {
            _updatingEditor = false;
        }

        if (Cheats.Count > 0)
            LoadSelectedCheat();
        else
            ClearCheatEditor();

        UpdateCheatButtons();
        UpdateSaveButton();
    }

    private void btnMoveUp_Click(object sender, EventArgs e)
    {
        if (lstCheats.SelectedItem is not CheatEntry cheat || cheat.IsHeader || cheat.IsMasterCode)
            return;

        SaveCurrentCheat();

        int index = Cheats.IndexOf(cheat);
        if (index <= 0)
            return;

        CheatEntry previous = Cheats[index - 1];

        // Header and mastercode are fixed at the top and can never be crossed.
        if (previous.IsHeader || previous.IsMasterCode)
            return;

        Cheats.RemoveAt(index);
        Cheats.Insert(index - 1, cheat);

        _cheatSource.ResetBindings(false);
        lstCheats.SelectedItem = cheat;
        lstCheats.Invalidate();

        UpdateCheatButtons();
        UpdateSaveButton();
    }

    private void btnMoveDown_Click(object sender, EventArgs e)
    {
        if (lstCheats.SelectedItem is not CheatEntry cheat || cheat.IsHeader || cheat.IsMasterCode)
            return;

        SaveCurrentCheat();

        int index = Cheats.IndexOf(cheat);
        if (index < 0 || index >= Cheats.Count - 1)
            return;

        CheatEntry next = Cheats[index + 1];

        // Fixed entries normally only exist at the top, but never cross them.
        if (next.IsHeader || next.IsMasterCode)
            return;

        Cheats.RemoveAt(index);
        Cheats.Insert(index + 1, cheat);

        _cheatSource.ResetBindings(false);
        lstCheats.SelectedItem = cheat;
        lstCheats.Invalidate();

        UpdateCheatButtons();
        UpdateSaveButton();
    }

    private void lstCheats_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_updatingEditor) return;
        LoadSelectedCheat();
    }

    private void LoadSelectedCheat()
    {
        _updatingEditor = true;
        try
        {
            if (lstCheats.SelectedItem is CheatEntry cheat)
            {
                txtCheatName.Text = cheat.Name;
                txtDescription.Text = cheat.Description;
                txtCodes.Text = cheat.Codes;
                bool editable = !cheat.IsMasterCode && !cheat.IsHeader;
                txtCheatName.Enabled = editable;
                txtDescription.Enabled = editable;
                txtCodes.Enabled = editable;
            }
            else ClearCheatEditor();
        }
        finally { _updatingEditor = false; }
        UpdateValidation(); UpdateCheatButtons();
    }

    private void ClearCheatEditor()
    {
        _updatingEditor = true;
        txtCheatName.Clear(); txtDescription.Clear(); txtCodes.Clear();
        txtCheatName.Enabled = false; txtDescription.Enabled = false; txtCodes.Enabled = false;
        _updatingEditor = false;
        UpdateValidation();
    }

    private void txtCheatName_TextChanged(object sender, EventArgs e)
    {
        if (_updatingEditor || lstCheats.SelectedItem is not CheatEntry cheat || cheat.IsMasterCode || cheat.IsHeader) return;
        cheat.Name = txtCheatName.Text;

        // Redraw only. Do not reset/rebind the BindingSource here, because that
        // raises selection events and can steal focus after every keystroke.
        lstCheats.Invalidate();
        UpdateSaveButton();
    }

    private void txtDescription_TextChanged(object sender, EventArgs e)
    {
        if (_updatingEditor || lstCheats.SelectedItem is not CheatEntry cheat || cheat.IsMasterCode || cheat.IsHeader) return;

        cheat.Description = txtDescription.Text;

        // Redraw only; do not reset/rebind the BindingSource while typing.
        lstCheats.Invalidate();
        UpdateSaveButton();
    }

    private void lstCheats_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        e.DrawBackground();

        if (lstCheats.Items[e.Index] is CheatEntry cheat)
        {
            if (!string.IsNullOrWhiteSpace(cheat.Description) && !cheat.IsHeader && !cheat.IsMasterCode)
            {
                Rectangle nameBounds = new(
                    e.Bounds.X + 2,
                    e.Bounds.Y + 1,
                    e.Bounds.Width - 4,
                    17);

                Rectangle descriptionBounds = new(
                    e.Bounds.X + 10,
                    e.Bounds.Y + 18,
                    e.Bounds.Width - 12,
                    Math.Max(15, e.Bounds.Height - 19));

                TextRenderer.DrawText(
                    e.Graphics,
                    cheat.Name,
                    e.Font ?? lstCheats.Font,
                    nameBounds,
                    e.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                string description = cheat.Description
                    .Replace("\r", " ")
                    .Replace("\n", " ");

                TextRenderer.DrawText(
                    e.Graphics,
                    description,
                    e.Font ?? lstCheats.Font,
                    descriptionBounds,
                    SystemColors.GrayText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
            else
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    cheat.Name,
                    e.Font ?? lstCheats.Font,
                    e.Bounds,
                    e.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        e.DrawFocusRectangle();
    }

    private void txtCodes_TextChanged(object sender, EventArgs e)
    {
        if (_updatingEditor || lstCheats.SelectedItem is not CheatEntry cheat || cheat.IsMasterCode || cheat.IsHeader) return;
        cheat.Codes = txtCodes.Text;
        UpdateValidation(); UpdateSaveButton();
    }

    private void SaveCurrentCheat()
    {
        if (lstCheats.SelectedItem is not CheatEntry cheat || cheat.IsMasterCode || cheat.IsHeader) return;
        cheat.Name = txtCheatName.Text;
        cheat.Description = txtDescription.Text;
        cheat.Codes = txtCodes.Text;
    }

    private void UpdateValidation()
    {
        lvValidation.Items.Clear();
        if (lstCheats.SelectedItem is not CheatEntry selected)
        {
            lblValidation.Text = "No cheat selected.";
            return;
        }

        if (selected.IsHeader)
        {
            lblValidation.Text = "Game header.";
            return;
        }

        var lines = Ps2RdCheat.Validate(txtCodes.Text);
        int bad = lines.Count(x => !x.IsValid);
        int good = lines.Count(x => x.IsValid);
        foreach (var v in lines)
        {
            var item = new ListViewItem(v.LineNumber.ToString());
            item.SubItems.Add(v.IsValid ? "OK" : "ERROR");
            item.SubItems.Add(v.Normalized ?? v.Original.Trim());
            item.SubItems.Add(v.Message);
            lvValidation.Items.Add(item);
        }
        lblValidation.Text = bad == 0
            ? (good == 0 ? "Enter one or more RAW codes." : $"✓ {good} valid RAW code line(s).")
            : $"⚠ {bad} invalid line(s), {good} valid line(s).";
    }

    private static string NormalizeCodesForDuplicateCheck(string codes)
    {
        string[] lines = codes
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return string.Join(
            "\n",
            lines.Select(line => string.Join(
                " ",
                line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                .ToUpperInvariant()));
    }

    private void btnImportPnach_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Import PCSX2 PNACH",
            Filter = "PCSX2 patch files (*.pnach)|*.pnach|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            PnachImportResult result = PnachImporter.ImportFile(dialog.FileName);

            HashSet<string> seenCodeSets = Cheats
                .Where(c => !c.IsHeader && !c.IsMasterCode && !string.IsNullOrWhiteSpace(c.Codes))
                .Select(c => NormalizeCodesForDuplicateCheck(c.Codes))
                .Where(c => c.Length > 0)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            List<PnachCheat> available = new();
            List<PnachCheat> duplicates = new();
            List<PnachCheat> unsupported = new();

            foreach (PnachCheat cheat in result.Cheats.Where(c => c.Codes.Count > 0))
            {
                string rawText = string.Join(Environment.NewLine, cheat.Codes);
                var validation = Ps2RdCheat.Validate(rawText);

                if (validation.Any(v => !v.IsValid))
                {
                    unsupported.Add(cheat);
                    continue;
                }

                string normalized = NormalizeCodesForDuplicateCheck(rawText);

                if (normalized.Length > 0 && !seenCodeSets.Add(normalized))
                {
                    duplicates.Add(cheat);
                    continue;
                }

                available.Add(cheat);
            }

            int duplicateCount = duplicates.Count;
            int unsupportedCount = unsupported.Count;

            if (available.Count == 0 && duplicates.Count == 0 && unsupported.Count == 0)
            {
                MessageBox.Show(
                    this,
                    "No convertible EE cheats were found in this PNACH file.",
                    "Import PNACH",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var selectForm = new PnachImportForm(available, duplicates, unsupported);
            if (selectForm.ShowDialog(this) != DialogResult.OK)
                return;

            List<PnachCheat> selected = selectForm.SelectedCheats.ToList();
            if (selected.Count == 0)
                return;

            SaveCurrentCheat();

            // Remove the automatic empty "Cheat 1" placeholder before a real import.
            CheatEntry? emptyPlaceholder = Cheats.FirstOrDefault(c =>
                !c.IsHeader &&
                !c.IsMasterCode &&
                c.Name.Equals("Cheat 1", StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(c.Codes));

            if (emptyPlaceholder is not null)
                Cheats.Remove(emptyPlaceholder);

            CheatEntry? lastImported = null;

            foreach (PnachCheat imported in selected)
            {
                var cheat = new CheatEntry
                {
                    Name = imported.Name,
                    Description = imported.Description,
                    Codes = string.Join(Environment.NewLine, imported.Codes)
                };

                Cheats.Add(cheat);
                lastImported = cheat;
            }

            _cheatSource.ResetBindings(false);

            if (lastImported is not null)
                lstCheats.SelectedItem = lastImported;

            UpdateCheatButtons();
            UpdateValidation();
            UpdateSaveButton();

            string extra = string.Empty;
            if (result.SkippedIopPatches > 0)
                extra += $" {result.SkippedIopPatches} IOP patch(es) skipped.";
            if (result.UnparsedPatchLines > 0)
                extra += $" {result.UnparsedPatchLines} patch line(s) could not be parsed.";

            if (duplicateCount > 0)
                extra += $" {duplicateCount} duplicate(s) detected.";
            if (unsupportedCount > 0)
                extra += $" {unsupportedCount} unsupported/non-RAW cheat(s) detected.";

            lblStatus.Text = $"Imported {selected.Count} PNACH cheat(s) from {Path.GetFileName(dialog.FileName)}.{extra}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                ex.Message,
                "Import PNACH",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void btnLoadCht_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Title = "Open PS2RD .CHT", Filter = "OPL cheat files (*.cht)|*.cht|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            string chtText = File.ReadAllText(dialog.FileName);
            var imported = Ps2RdCheat.ParseCht(chtText, out string? importedMaster);

            string firstLine = chtText.Replace("\r\n", "\n").Replace('\r', '\n')
                                      .Split('\n', StringSplitOptions.None)
                                      .FirstOrDefault()?.Trim() ?? string.Empty;
            if (firstLine.Length >= 2 && firstLine.StartsWith('"') && firstLine.EndsWith('"'))
            {
                string header = firstLine[1..^1];
                int idPos = header.LastIndexOf(" /ID ", StringComparison.OrdinalIgnoreCase);
                _gameTitle = idPos >= 0 ? header[..idPos].Trim() : header.Trim();
            }

            Cheats.Clear();
            txtSelected.Clear();

            if (firstLine.Length >= 2 && firstLine.StartsWith('"') && firstLine.EndsWith('"'))
            {
                string header = firstLine[1..^1];
                int idPos = header.LastIndexOf(" /ID ", StringComparison.OrdinalIgnoreCase);
                if (idPos >= 0)
                {
                    _gameTitle = header[..idPos].Trim();
                    txtSerial.Text = header[(idPos + 5)..].Trim();
                }

                Cheats.Add(new CheatEntry
                {
                    Name = firstLine,
                    Codes = string.Empty,
                    IsHeader = true
                });
            }

            if (!string.IsNullOrWhiteSpace(importedMaster))
            {
                txtSelected.Text = importedMaster.Trim();

                Cheats.Add(new CheatEntry
                {
                    Name = "Mastercode",
                    Codes = importedMaster.ToLowerInvariant(),
                    IsMasterCode = true
                });
            }

            foreach (var cheat in imported)
            {
                if (cheat.Name.TrimStart().StartsWith('"') &&
                    cheat.Name.Contains(" /ID ", StringComparison.OrdinalIgnoreCase))
                    continue;

                cheat.IsMasterCode = false;
                cheat.IsHeader = false;
                Cheats.Add(cheat);
            }
            _cheatSource.ResetBindings(false);
            if (Cheats.Count > 0) lstCheats.SelectedIndex = 0; else ClearCheatEditor();
            string note = importedMaster is null ? "" : $" Imported mastercode: {importedMaster}.";
            lblStatus.Text = $"Loaded {Cheats.Count} cheat(s) from {Path.GetFileName(dialog.FileName)}.{note}";
            UpdateCheatButtons(); UpdateSaveButton();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Load CHT", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnSaveCht_Click(object sender, EventArgs e)
    {
        SaveCurrentCheat();
        if (string.IsNullOrWhiteSpace(txtSerial.Text))
        {
            MessageBox.Show(this, "Open a PS2 ISO first so the game serial is known.", "Save CHT", MessageBoxButtons.OK, MessageBoxIcon.Information); return;
        }
        if (string.IsNullOrWhiteSpace(txtSelected.Text) || !txtSelected.Text.StartsWith('9'))
        {
            MessageBox.Show(this, "A type-9 PS2RD mastercode is required.", "Save CHT", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }

        var invalid = Cheats.Where(c => !c.IsMasterCode && !c.IsHeader).SelectMany(c => Ps2RdCheat.Validate(c.Codes).Select(v => (c, v))).Where(x => !x.v.IsValid).ToList();
        if (invalid.Count > 0)
        {
            MessageBox.Show(this, $"There are {invalid.Count} invalid code line(s). Fix them before saving.", "Save CHT", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        string fileName = txtSerial.Text.Trim() + ".cht";
        using var dialog = new SaveFileDialog { Title = "Save OPL PS2RD cheat file", Filter = "OPL cheat files (*.cht)|*.cht", FileName = fileName, AddExtension = true, DefaultExt = "cht" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialog.FileName, BuildChtText(_gameTitle, txtSerial.Text, txtSelected.Text, Cheats));
        lblStatus.Text = $"Saved {Path.GetFileName(dialog.FileName)}.";
    }

    private void UpdateCheatButtons()
    {
        bool editable = lstCheats.SelectedItem is CheatEntry selected &&
                        !selected.IsMasterCode &&
                        !selected.IsHeader;

        btnRemoveCheat.Enabled = editable;
        txtCheatName.Enabled = editable;
        txtDescription.Enabled = editable;
        txtCodes.Enabled = editable;

        bool canMoveUp = false;
        bool canMoveDown = false;

        if (editable && lstCheats.SelectedItem is CheatEntry cheat)
        {
            int index = Cheats.IndexOf(cheat);

            canMoveUp =
                index > 0 &&
                !Cheats[index - 1].IsHeader &&
                !Cheats[index - 1].IsMasterCode;

            canMoveDown =
                index >= 0 &&
                index < Cheats.Count - 1 &&
                !Cheats[index + 1].IsHeader &&
                !Cheats[index + 1].IsMasterCode;
        }

        btnMoveUp.Enabled = canMoveUp;
        btnMoveDown.Enabled = canMoveDown;
    }

    private void UpdateSaveButton()
    {
        btnSaveCht.Enabled = !string.IsNullOrWhiteSpace(txtSerial.Text) && !string.IsNullOrWhiteSpace(txtSelected.Text);
    }
}
