#nullable disable

namespace PS2RDCHTManager;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Button btnOpenIso, btnOpenElf, btnCopy, btnAddCheat, btnRemoveCheat, btnMoveUp, btnMoveDown, btnImportPnach, btnLoadCht, btnSaveCht;
    private TextBox txtSourcePath, txtSerial, txtCrc, txtSelected, txtSource, txtCheatName, txtDescription, txtCodes;
    private ListView listCandidates, lvValidation;
    private ColumnHeader colFunction, colAddress, colData, colValLine, colValState, colValCode, colValInfo;
    private ListBox lstCheats;
    private Label lblStatus, lblSourcePath, lblSerial, lblCrc, lblCandidates, lblSelected, lblSource, lblCheats, lblCheatName, lblDescription, lblCodes, lblValidation;
    private GroupBox grpMaster, grpCheats;
    private SplitContainer splitCheats;

    protected override void Dispose(bool disposing) { if (disposing && components != null) components.Dispose(); base.Dispose(disposing); }

    private void InitializeComponent()
    {
        btnOpenIso = new Button();
        btnOpenElf = new Button();
        btnCopy = new Button();
        btnAddCheat = new Button();
        btnRemoveCheat = new Button();
        btnMoveUp = new Button();
        btnMoveDown = new Button();
        btnImportPnach = new Button();
        btnLoadCht = new Button();
        btnSaveCht = new Button();
        txtSourcePath = new TextBox();
        txtSerial = new TextBox();
        txtCrc = new TextBox();
        txtSelected = new TextBox();
        txtSource = new TextBox();
        txtCheatName = new TextBox();
        txtDescription = new TextBox();
        txtCodes = new TextBox();
        listCandidates = new ListView();
        colFunction = new ColumnHeader();
        colAddress = new ColumnHeader();
        colData = new ColumnHeader();
        lvValidation = new ListView();
        colValLine = new ColumnHeader();
        colValState = new ColumnHeader();
        colValCode = new ColumnHeader();
        colValInfo = new ColumnHeader();
        lstCheats = new ListBox();
        lblStatus = new Label();
        lblSourcePath = new Label();
        lblSerial = new Label();
        lblCrc = new Label();
        lblCandidates = new Label();
        lblSelected = new Label();
        lblSource = new Label();
        lblCheats = new Label();
        lblCheatName = new Label();
        lblDescription = new Label();
        lblCodes = new Label();
        lblValidation = new Label();
        grpMaster = new GroupBox();
        grpCheats = new GroupBox();
        splitCheats = new SplitContainer();
        grpMaster.SuspendLayout();
        grpCheats.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitCheats).BeginInit();
        splitCheats.Panel1.SuspendLayout();
        splitCheats.Panel2.SuspendLayout();
        splitCheats.SuspendLayout();
        SuspendLayout();
        // 
        // btnOpenIso
        // 
        btnOpenIso.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpenIso.Location = new Point(1461, 15);
        btnOpenIso.Name = "btnOpenIso";
        btnOpenIso.Size = new Size(105, 27);
        btnOpenIso.TabIndex = 2;
        btnOpenIso.Text = "Open ISO...";
        btnOpenIso.Click += btnOpenIso_Click;
        // 
        // btnOpenElf
        // 
        btnOpenElf.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnOpenElf.Location = new Point(1572, 15);
        btnOpenElf.Name = "btnOpenElf";
        btnOpenElf.Size = new Size(105, 27);
        btnOpenElf.TabIndex = 3;
        btnOpenElf.Text = "Open ELF...";
        btnOpenElf.Click += btnOpenElf_Click;
        // 
        // btnCopy
        // 
        btnCopy.Enabled = false;
        btnCopy.Location = new Point(610, 159);
        btnCopy.Name = "btnCopy";
        btnCopy.Size = new Size(140, 27);
        btnCopy.TabIndex = 6;
        btnCopy.Text = "Copy Mastercode";
        btnCopy.Click += btnCopy_Click;
        // 
        // btnAddCheat
        // 
        btnAddCheat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnAddCheat.Location = new Point(8, 29);
        btnAddCheat.Name = "btnAddCheat";
        btnAddCheat.Size = new Size(1081, 27);
        btnAddCheat.TabIndex = 2;
        btnAddCheat.Text = "Add Cheat";
        btnAddCheat.Click += btnAddCheat_Click;
        // 
        // btnRemoveCheat
        // 
        btnRemoveCheat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnRemoveCheat.Location = new Point(8, 62);
        btnRemoveCheat.Name = "btnRemoveCheat";
        btnRemoveCheat.Size = new Size(1081, 27);
        btnRemoveCheat.TabIndex = 3;
        btnRemoveCheat.Text = "Delete Cheat";
        btnRemoveCheat.Click += btnRemoveCheat_Click;
        // 
        // btnMoveUp
        // 
        btnMoveUp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        btnMoveUp.Enabled = false;
        btnMoveUp.Location = new Point(8, 95);
        btnMoveUp.Name = "btnMoveUp";
        btnMoveUp.Size = new Size(537, 27);
        btnMoveUp.TabIndex = 4;
        btnMoveUp.Text = "↑ Move Up";
        btnMoveUp.Click += btnMoveUp_Click;
        // 
        // btnMoveDown
        // 
        btnMoveDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMoveDown.Enabled = false;
        btnMoveDown.Location = new Point(551, 95);
        btnMoveDown.Name = "btnMoveDown";
        btnMoveDown.Size = new Size(538, 27);
        btnMoveDown.TabIndex = 5;
        btnMoveDown.Text = "↓ Move Down";
        btnMoveDown.Click += btnMoveDown_Click;
        // 
        // btnImportPnach
        // 
        btnImportPnach.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnImportPnach.Location = new Point(1344, 49);
        btnImportPnach.Name = "btnImportPnach";
        btnImportPnach.Size = new Size(111, 27);
        btnImportPnach.TabIndex = 8;
        btnImportPnach.Text = "Import PNACH...";
        btnImportPnach.Click += btnImportPnach_Click;
        // 
        // 
        // btnLoadCht
        // 
        btnLoadCht.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnLoadCht.Location = new Point(1461, 49);
        btnLoadCht.Name = "btnLoadCht";
        btnLoadCht.Size = new Size(105, 27);
        btnLoadCht.TabIndex = 8;
        btnLoadCht.Text = "Load .CHT...";
        btnLoadCht.Click += btnLoadCht_Click;
        // 
        // btnSaveCht
        // 
        btnSaveCht.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSaveCht.Enabled = false;
        btnSaveCht.Location = new Point(1572, 49);
        btnSaveCht.Name = "btnSaveCht";
        btnSaveCht.Size = new Size(105, 27);
        btnSaveCht.TabIndex = 9;
        btnSaveCht.Text = "Save .CHT...";
        btnSaveCht.Click += btnSaveCht_Click;
        // 
        // txtSourcePath
        // 
        txtSourcePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSourcePath.Location = new Point(78, 17);
        txtSourcePath.Name = "txtSourcePath";
        txtSourcePath.ReadOnly = true;
        txtSourcePath.Size = new Size(1371, 23);
        txtSourcePath.TabIndex = 1;
        // 
        // txtSerial
        // 
        txtSerial.Location = new Point(78, 51);
        txtSerial.Name = "txtSerial";
        txtSerial.ReadOnly = true;
        txtSerial.Size = new Size(150, 23);
        txtSerial.TabIndex = 5;
        // 
        // txtCrc
        // 
        txtCrc.Location = new Point(310, 51);
        txtCrc.Name = "txtCrc";
        txtCrc.ReadOnly = true;
        txtCrc.Size = new Size(120, 23);
        txtCrc.TabIndex = 7;
        // 
        // txtSelected
        // 
        txtSelected.Font = new Font("Consolas", 10F, FontStyle.Bold);
        txtSelected.Location = new Point(77, 161);
        txtSelected.Name = "txtSelected";
        txtSelected.ReadOnly = true;
        txtSelected.Size = new Size(245, 23);
        txtSelected.TabIndex = 3;
        // 
        // txtSource
        // 
        txtSource.Location = new Point(404, 161);
        txtSource.Name = "txtSource";
        txtSource.ReadOnly = true;
        txtSource.Size = new Size(190, 23);
        txtSource.TabIndex = 5;
        // 
        // txtCheatName
        // 
        txtCheatName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtCheatName.Location = new Point(91, 6);
        txtCheatName.Name = "txtCheatName";
        txtCheatName.Size = new Size(469, 23);
        txtCheatName.TabIndex = 1;
        txtCheatName.TextChanged += txtCheatName_TextChanged;
        // 
        // txtDescription
        // 
        txtDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDescription.Location = new Point(12, 62);
        txtDescription.Multiline = true;
        txtDescription.Name = "txtDescription";
        txtDescription.ScrollBars = ScrollBars.Vertical;
        txtDescription.Size = new Size(548, 48);
        txtDescription.TabIndex = 3;
        txtDescription.TextChanged += txtDescription_TextChanged;
        // 
        // txtCodes
        // 
        txtCodes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtCodes.Font = new Font("Consolas", 10F);
        txtCodes.Location = new Point(12, 135);
        txtCodes.Multiline = true;
        txtCodes.Name = "txtCodes";
        txtCodes.ScrollBars = ScrollBars.Both;
        txtCodes.Size = new Size(548, 70);
        txtCodes.TabIndex = 5;
        txtCodes.WordWrap = false;
        txtCodes.TextChanged += txtCodes_TextChanged;
        // 
        // listCandidates
        // 
        listCandidates.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        listCandidates.Columns.AddRange(new ColumnHeader[] { colFunction, colAddress, colData });
        listCandidates.FullRowSelect = true;
        listCandidates.GridLines = true;
        listCandidates.Location = new Point(12, 44);
        listCandidates.MultiSelect = false;
        listCandidates.Name = "listCandidates";
        listCandidates.Size = new Size(1641, 104);
        listCandidates.TabIndex = 1;
        listCandidates.UseCompatibleStateImageBehavior = false;
        listCandidates.View = View.Details;
        listCandidates.SelectedIndexChanged += listCandidates_SelectedIndexChanged;
        // 
        // colFunction
        // 
        colFunction.Text = "Function";
        colFunction.Width = 260;
        // 
        // colAddress
        // 
        colAddress.Text = "PS2RD address";
        colAddress.Width = 190;
        // 
        // colData
        // 
        colData.Text = "Original instruction";
        colData.Width = 210;
        // 
        // lvValidation
        // 
        lvValidation.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lvValidation.Columns.AddRange(new ColumnHeader[] { colValLine, colValState, colValCode, colValInfo });
        lvValidation.FullRowSelect = true;
        lvValidation.GridLines = true;
        lvValidation.Location = new Point(12, 236);
        lvValidation.Name = "lvValidation";
        lvValidation.Size = new Size(548, 334);
        lvValidation.TabIndex = 7;
        lvValidation.UseCompatibleStateImageBehavior = false;
        lvValidation.View = View.Details;
        // 
        // colValLine
        // 
        colValLine.Text = "Line";
        colValLine.Width = 45;
        // 
        // colValState
        // 
        colValState.Text = "Status";
        colValState.Width = 70;
        // 
        // colValCode
        // 
        colValCode.Text = "Code";
        colValCode.Width = 190;
        // 
        // colValInfo
        // 
        colValInfo.Text = "Info";
        colValInfo.Width = 380;
        // 
        // lstCheats
        // 
        lstCheats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lstCheats.DrawMode = DrawMode.OwnerDrawFixed;
        lstCheats.ItemHeight = 36;
        lstCheats.Location = new Point(8, 130);
        lstCheats.Name = "lstCheats";
        lstCheats.Size = new Size(1215, 656);
        lstCheats.TabIndex = 1;
        lstCheats.DrawItem += lstCheats_DrawItem;
        lstCheats.SelectedIndexChanged += lstCheats_SelectedIndexChanged;
        // 
        // lblStatus
        // 
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.BorderStyle = BorderStyle.Fixed3D;
        lblStatus.Location = new Point(0, 922);
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new Padding(8, 0, 0, 0);
        lblStatus.Size = new Size(1689, 27);
        lblStatus.TabIndex = 12;
        lblStatus.Text = "Open a PS2 ISO or ELF to begin.";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblSourcePath
        // 
        lblSourcePath.AutoSize = true;
        lblSourcePath.Location = new Point(16, 20);
        lblSourcePath.Name = "lblSourcePath";
        lblSourcePath.Size = new Size(46, 15);
        lblSourcePath.TabIndex = 0;
        lblSourcePath.Text = "Source:";
        // 
        // lblSerial
        // 
        lblSerial.AutoSize = true;
        lblSerial.Location = new Point(16, 54);
        lblSerial.Name = "lblSerial";
        lblSerial.Size = new Size(38, 15);
        lblSerial.TabIndex = 4;
        lblSerial.Text = "Serial:";
        // 
        // lblCrc
        // 
        lblCrc.AutoSize = true;
        lblCrc.Location = new Point(249, 54);
        lblCrc.Name = "lblCrc";
        lblCrc.Size = new Size(54, 15);
        lblCrc.TabIndex = 6;
        lblCrc.Text = "ELF CRC:";
        // 
        // lblCandidates
        // 
        lblCandidates.AutoSize = true;
        lblCandidates.Location = new Point(12, 25);
        lblCandidates.Name = "lblCandidates";
        lblCandidates.Size = new Size(66, 15);
        lblCandidates.TabIndex = 0;
        lblCandidates.Text = "Candidates";
        // 
        // lblSelected
        // 
        lblSelected.AutoSize = true;
        lblSelected.Location = new Point(12, 165);
        lblSelected.Name = "lblSelected";
        lblSelected.Size = new Size(54, 15);
        lblSelected.TabIndex = 2;
        lblSelected.Text = "Selected:";
        // 
        // lblSource
        // 
        lblSource.AutoSize = true;
        lblSource.Location = new Point(342, 165);
        lblSource.Name = "lblSource";
        lblSource.Size = new Size(57, 15);
        lblSource.TabIndex = 4;
        lblSource.Text = "Function:";
        // 
        // lblCheats
        // 
        lblCheats.AutoSize = true;
        lblCheats.Location = new Point(8, 9);
        lblCheats.Name = "lblCheats";
        lblCheats.Size = new Size(43, 15);
        lblCheats.TabIndex = 0;
        lblCheats.Text = "Cheats";
        // 
        // lblCheatName
        // 
        lblCheatName.AutoSize = true;
        lblCheatName.Location = new Point(12, 9);
        lblCheatName.Name = "lblCheatName";
        lblCheatName.Size = new Size(74, 15);
        lblCheatName.TabIndex = 0;
        lblCheatName.Text = "Cheat name:";
        // 
        // lblDescription
        // 
        lblDescription.AutoSize = true;
        lblDescription.Location = new Point(12, 43);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new Size(70, 15);
        lblDescription.TabIndex = 2;
        lblDescription.Text = "Description:";
        // 
        // lblCodes
        // 
        lblCodes.AutoSize = true;
        lblCodes.Location = new Point(12, 116);
        lblCodes.Name = "lblCodes";
        lblCodes.Size = new Size(69, 15);
        lblCodes.TabIndex = 4;
        lblCodes.Text = "RAW codes:";
        // 
        // lblValidation
        // 
        lblValidation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblValidation.Location = new Point(12, 212);
        lblValidation.Name = "lblValidation";
        lblValidation.Size = new Size(1273, 22);
        lblValidation.TabIndex = 6;
        lblValidation.Text = "No cheat selected.";
        // 
        // grpMaster
        // 
        grpMaster.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        grpMaster.Controls.Add(lblCandidates);
        grpMaster.Controls.Add(listCandidates);
        grpMaster.Controls.Add(lblSelected);
        grpMaster.Controls.Add(txtSelected);
        grpMaster.Controls.Add(lblSource);
        grpMaster.Controls.Add(txtSource);
        grpMaster.Controls.Add(btnCopy);
        grpMaster.Location = new Point(12, 88);
        grpMaster.Name = "grpMaster";
        grpMaster.Size = new Size(1665, 219);
        grpMaster.TabIndex = 10;
        grpMaster.TabStop = false;
        grpMaster.Text = "Mastercode";
        // 
        // grpCheats
        // 
        grpCheats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpCheats.Controls.Add(splitCheats);
        grpCheats.Location = new Point(12, 316);
        grpCheats.Name = "grpCheats";
        grpCheats.Size = new Size(1665, 595);
        grpCheats.TabIndex = 11;
        grpCheats.TabStop = false;
        grpCheats.Text = "PS2RD Cheats";
        // 
        // splitCheats
        // 
        splitCheats.Dock = DockStyle.Fill;
        splitCheats.Location = new Point(3, 19);
        splitCheats.Name = "splitCheats";
        // 
        // splitCheats.Panel1
        // 
        splitCheats.Panel1.Controls.Add(lblCheats);
        splitCheats.Panel1.Controls.Add(lstCheats);
        splitCheats.Panel1.Controls.Add(btnAddCheat);
        splitCheats.Panel1.Controls.Add(btnRemoveCheat);
        splitCheats.Panel1.Controls.Add(btnMoveUp);
        splitCheats.Panel1.Controls.Add(btnMoveDown);
        // 
        // splitCheats.Panel2
        // 
        splitCheats.Panel2.Controls.Add(lblCheatName);
        splitCheats.Panel2.Controls.Add(txtCheatName);
        splitCheats.Panel2.Controls.Add(lblDescription);
        splitCheats.Panel2.Controls.Add(txtDescription);
        splitCheats.Panel2.Controls.Add(lblCodes);
        splitCheats.Panel2.Controls.Add(txtCodes);
        splitCheats.Panel2.Controls.Add(lblValidation);
        splitCheats.Panel2.Controls.Add(lvValidation);
        splitCheats.Size = new Size(1659, 573);
        splitCheats.SplitterDistance = 1092;
        splitCheats.TabIndex = 0;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1689, 949);
        Controls.Add(lblSourcePath);
        Controls.Add(txtSourcePath);
        Controls.Add(btnOpenIso);
        Controls.Add(btnOpenElf);
        Controls.Add(lblSerial);
        Controls.Add(txtSerial);
        Controls.Add(lblCrc);
        Controls.Add(txtCrc);
        Controls.Add(btnImportPnach);
        Controls.Add(btnLoadCht);
        Controls.Add(btnSaveCht);
        Controls.Add(grpMaster);
        Controls.Add(grpCheats);
        Controls.Add(lblStatus);
        MinimumSize = new Size(900, 650);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "PS2RD CHT Manager";
        grpMaster.ResumeLayout(false);
        grpMaster.PerformLayout();
        grpCheats.ResumeLayout(false);
        splitCheats.Panel1.ResumeLayout(false);
        splitCheats.Panel1.PerformLayout();
        splitCheats.Panel2.ResumeLayout(false);
        splitCheats.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitCheats).EndInit();
        splitCheats.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
