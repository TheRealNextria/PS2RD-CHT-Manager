namespace PS2RDCHTManager;

public partial class PnachImportForm : Form
{
    private readonly List<PnachCheat> _uniqueCheats;
    private readonly List<PnachCheat> _duplicateCheats;
    private readonly List<PnachCheat> _unsupportedCheats;
    private readonly HashSet<PnachCheat> _selected = new();
    private bool _updatingChecks;

    public IReadOnlyList<PnachCheat> SelectedCheats =>
        _uniqueCheats
            .Concat(chkShowDuplicates.Checked ? _duplicateCheats : Enumerable.Empty<PnachCheat>())
            .Concat(chkShowUnsupported.Checked ? _unsupportedCheats : Enumerable.Empty<PnachCheat>())
            .Where(c => _selected.Contains(c))
            .ToList();

    public PnachImportForm(
        IEnumerable<PnachCheat> uniqueCheats,
        IEnumerable<PnachCheat> duplicateCheats,
        IEnumerable<PnachCheat> unsupportedCheats)
    {
        InitializeComponent();

        _uniqueCheats = uniqueCheats.ToList();
        _duplicateCheats = duplicateCheats.ToList();
        _unsupportedCheats = unsupportedCheats.ToList();

        ApplyFilter();
        UpdateCounts();
    }

    private void txtSearch_TextChanged(object sender, EventArgs e)
    {
        ApplyFilter();
    }

    private void chkShowDuplicates_CheckedChanged(object sender, EventArgs e)
    {
        ApplyFilter();
    }

    private void chkShowUnsupported_CheckedChanged(object sender, EventArgs e)
    {
        ApplyFilter();
    }

    private void checkedCheats_ItemCheck(object sender, ItemCheckEventArgs e)
    {
        if (_updatingChecks || e.Index < 0 || e.Index >= checkedCheats.Items.Count)
            return;

        if (checkedCheats.Items[e.Index] is not PnachCheatDisplayItem item)
            return;

        if (e.NewValue == CheckState.Checked)
            _selected.Add(item.Cheat);
        else
            _selected.Remove(item.Cheat);

        BeginInvoke(UpdateCounts);
    }

    private void checkedCheats_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (checkedCheats.SelectedItem is PnachCheatDisplayItem item)
        {
            txtCodesPreview.Text = string.Join(Environment.NewLine, item.Cheat.Codes);
            txtDescription.Text = item.Cheat.Description;
        }
        else
        {
            txtCodesPreview.Clear();
            txtDescription.Clear();
        }
    }

    private void btnSelectAll_Click(object sender, EventArgs e)
    {
        foreach (PnachCheat cheat in GetCurrentlyVisibleCheats())
            _selected.Add(cheat);

        ApplyFilter();
        UpdateCounts();
    }

    private void btnDeselectAll_Click(object sender, EventArgs e)
    {
        foreach (PnachCheat cheat in GetCurrentlyVisibleCheats())
            _selected.Remove(cheat);

        ApplyFilter();
        UpdateCounts();
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
        if (_selected.Count == 0)
        {
            MessageBox.Show(
                this,
                "Select at least one cheat to import.",
                "Import PNACH",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private IEnumerable<PnachCheat> GetCurrentlyVisibleCheats()
    {
        string filter = txtSearch.Text.Trim();

        IEnumerable<PnachCheat> source = _uniqueCheats;

        if (chkShowDuplicates.Checked)
            source = source.Concat(_duplicateCheats);

        if (chkShowUnsupported.Checked)
            source = source.Concat(_unsupportedCheats);

        if (filter.Length > 0)
        {
            source = source.Where(c =>
                c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return source;
    }

    private void ApplyFilter()
    {
        string filter = txtSearch.Text.Trim();

        IEnumerable<PnachCheatDisplayItem> visible =
            _uniqueCheats.Select(c => new PnachCheatDisplayItem(c, PnachCheatDisplayKind.Unique));

        if (chkShowDuplicates.Checked)
        {
            visible = visible.Concat(
                _duplicateCheats.Select(c => new PnachCheatDisplayItem(c, PnachCheatDisplayKind.Duplicate)));
        }

        if (chkShowUnsupported.Checked)
        {
            visible = visible.Concat(
                _unsupportedCheats.Select(c => new PnachCheatDisplayItem(c, PnachCheatDisplayKind.Unsupported)));
        }

        if (filter.Length > 0)
        {
            visible = visible.Where(item =>
                item.Cheat.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                item.Cheat.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        _updatingChecks = true;
        checkedCheats.BeginUpdate();

        try
        {
            PnachCheat? previous = (checkedCheats.SelectedItem as PnachCheatDisplayItem)?.Cheat;
            checkedCheats.Items.Clear();

            foreach (PnachCheatDisplayItem item in visible)
            {
                int index = checkedCheats.Items.Add(item);
                checkedCheats.SetItemChecked(index, _selected.Contains(item.Cheat));
            }

            if (previous is not null)
            {
                for (int i = 0; i < checkedCheats.Items.Count; i++)
                {
                    if (checkedCheats.Items[i] is PnachCheatDisplayItem item &&
                        ReferenceEquals(item.Cheat, previous))
                    {
                        checkedCheats.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
        finally
        {
            checkedCheats.EndUpdate();
            _updatingChecks = false;
        }

        UpdateCounts();
    }

    private void UpdateCounts()
    {
        int total = _uniqueCheats.Count + _duplicateCheats.Count + _unsupportedCheats.Count;

        lblCount.Text =
            $"Selected: {_selected.Count}    " +
            $"Unique: {_uniqueCheats.Count}    " +
            $"Duplicates: {_duplicateCheats.Count}    " +
            $"Unsupported: {_unsupportedCheats.Count}    " +
            $"Showing: {checkedCheats.Items.Count} / {total}";

        btnImport.Enabled = _selected.Count > 0;
    }

    private enum PnachCheatDisplayKind
    {
        Unique,
        Duplicate,
        Unsupported
    }

    private sealed class PnachCheatDisplayItem
    {
        public PnachCheat Cheat { get; }
        public PnachCheatDisplayKind Kind { get; }

        public PnachCheatDisplayItem(PnachCheat cheat, PnachCheatDisplayKind kind)
        {
            Cheat = cheat;
            Kind = kind;
        }

        public override string ToString() =>
            Kind switch
            {
                PnachCheatDisplayKind.Duplicate => $"{Cheat.Name}  (duplicate)",
                PnachCheatDisplayKind.Unsupported => $"{Cheat.Name}  (not PS2RD RAW)",
                _ => Cheat.Name
            };
    }
}
