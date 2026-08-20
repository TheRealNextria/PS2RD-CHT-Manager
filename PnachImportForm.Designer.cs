#nullable disable

namespace PS2RDCHTManager;

partial class PnachImportForm
{
    private System.ComponentModel.IContainer components = null;
    private Label lblSearch;
    private TextBox txtSearch;
    private Button btnSelectAll;
    private Button btnDeselectAll;
    private CheckBox chkShowDuplicates;
    private CheckBox chkShowUnsupported;
    private CheckedListBox checkedCheats;
    private Label lblCodesPreview;
    private TextBox txtCodesPreview;
    private Label lblDescription;
    private TextBox txtDescription;
    private Label lblCount;
    private Button btnImport;
    private Button btnCancel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblSearch = new Label();
        txtSearch = new TextBox();
        btnSelectAll = new Button();
        btnDeselectAll = new Button();
        chkShowDuplicates = new CheckBox();
        chkShowUnsupported = new CheckBox();
        checkedCheats = new CheckedListBox();
        lblCodesPreview = new Label();
        txtCodesPreview = new TextBox();
        lblDescription = new Label();
        txtDescription = new TextBox();
        lblCount = new Label();
        btnImport = new Button();
        btnCancel = new Button();
        SuspendLayout();
        // 
        // lblSearch
        // 
        lblSearch.AutoSize = true;
        lblSearch.Location = new Point(12, 17);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new Size(45, 15);
        lblSearch.TabIndex = 0;
        lblSearch.Text = "Search:";
        // 
        // txtSearch
        // 
        txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSearch.Location = new Point(63, 14);
        txtSearch.Name = "txtSearch";
        txtSearch.Size = new Size(573, 23);
        txtSearch.TabIndex = 1;
        txtSearch.TextChanged += txtSearch_TextChanged;
        // 
        // btnSelectAll
        // 
        btnSelectAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnSelectAll.Location = new Point(642, 12);
        btnSelectAll.Name = "btnSelectAll";
        btnSelectAll.Size = new Size(130, 27);
        btnSelectAll.TabIndex = 2;
        btnSelectAll.Text = "Select All";
        btnSelectAll.Click += btnSelectAll_Click;
        // 
        // btnDeselectAll
        // 
        btnDeselectAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDeselectAll.Location = new Point(778, 12);
        btnDeselectAll.Name = "btnDeselectAll";
        btnDeselectAll.Size = new Size(130, 27);
        btnDeselectAll.TabIndex = 3;
        btnDeselectAll.Text = "Deselect All";
        btnDeselectAll.Click += btnDeselectAll_Click;
        // 
        // chkShowDuplicates
        // 
        chkShowDuplicates.AutoSize = true;
        chkShowDuplicates.Location = new Point(12, 51);
        chkShowDuplicates.Name = "chkShowDuplicates";
        chkShowDuplicates.Size = new Size(112, 19);
        chkShowDuplicates.TabIndex = 4;
        chkShowDuplicates.Text = "Show duplicates";
        chkShowDuplicates.UseVisualStyleBackColor = true;
        chkShowDuplicates.CheckedChanged += chkShowDuplicates_CheckedChanged;
        // 
        // chkShowUnsupported
        // 
        chkShowUnsupported.AutoSize = true;
        chkShowUnsupported.Location = new Point(140, 51);
        chkShowUnsupported.Name = "chkShowUnsupported";
        chkShowUnsupported.Size = new Size(188, 19);
        chkShowUnsupported.TabIndex = 5;
        chkShowUnsupported.Text = "Show unsupported / non-RAW";
        chkShowUnsupported.UseVisualStyleBackColor = true;
        chkShowUnsupported.CheckedChanged += chkShowUnsupported_CheckedChanged;
        // 
        // 
        // checkedCheats
        // 
        checkedCheats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        checkedCheats.CheckOnClick = true;
        checkedCheats.FormattingEnabled = true;
        checkedCheats.HorizontalScrollbar = true;
        checkedCheats.IntegralHeight = false;
        checkedCheats.Location = new Point(12, 76);
        checkedCheats.Name = "checkedCheats";
        checkedCheats.Size = new Size(896, 329);
        checkedCheats.TabIndex = 6;
        checkedCheats.ItemCheck += checkedCheats_ItemCheck;
        checkedCheats.SelectedIndexChanged += checkedCheats_SelectedIndexChanged;
        // 
        // lblCodesPreview
        // 
        lblCodesPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblCodesPreview.AutoSize = true;
        lblCodesPreview.Location = new Point(12, 416);
        lblCodesPreview.Name = "lblCodesPreview";
        lblCodesPreview.Size = new Size(67, 15);
        lblCodesPreview.TabIndex = 7;
        lblCodesPreview.Text = "RAW codes:";
        // 
        // txtCodesPreview
        // 
        txtCodesPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtCodesPreview.Font = new Font("Consolas", 9F);
        txtCodesPreview.Location = new Point(12, 435);
        txtCodesPreview.Multiline = true;
        txtCodesPreview.Name = "txtCodesPreview";
        txtCodesPreview.ReadOnly = true;
        txtCodesPreview.ScrollBars = ScrollBars.Both;
        txtCodesPreview.Size = new Size(896, 90);
        txtCodesPreview.TabIndex = 8;
        txtCodesPreview.WordWrap = false;
        // 
        // 
        // lblDescription
        // 
        lblDescription.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblDescription.AutoSize = true;
        lblDescription.Location = new Point(12, 536);
        lblDescription.Name = "lblDescription";
        lblDescription.Size = new Size(70, 15);
        lblDescription.TabIndex = 9;
        lblDescription.Text = "Description:";
        // 
        // txtDescription
        // 
        txtDescription.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtDescription.Location = new Point(12, 555);
        txtDescription.Multiline = true;
        txtDescription.Name = "txtDescription";
        txtDescription.ReadOnly = true;
        txtDescription.ScrollBars = ScrollBars.Vertical;
        txtDescription.Size = new Size(896, 65);
        txtDescription.TabIndex = 10;
        // 
        // lblCount
        // 
        lblCount.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblCount.Location = new Point(12, 632);
        lblCount.Name = "lblCount";
        lblCount.Size = new Size(600, 23);
        lblCount.TabIndex = 11;
        lblCount.Text = "Selected: 0 / 0";
        lblCount.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnImport
        // 
        btnImport.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnImport.Enabled = false;
        btnImport.Location = new Point(642, 630);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(130, 27);
        btnImport.TabIndex = 12;
        btnImport.Text = "Import Selected";
        btnImport.Click += btnImport_Click;
        // 
        // btnCancel
        // 
        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(778, 630);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(130, 27);
        btnCancel.TabIndex = 13;
        btnCancel.Text = "Cancel";
        // 
        // PnachImportForm
        // 
        AcceptButton = btnImport;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(920, 669);
        Controls.Add(lblSearch);
        Controls.Add(txtSearch);
        Controls.Add(btnSelectAll);
        Controls.Add(btnDeselectAll);
        Controls.Add(chkShowDuplicates);
        Controls.Add(chkShowUnsupported);
        Controls.Add(checkedCheats);
        Controls.Add(lblCodesPreview);
        Controls.Add(txtCodesPreview);
        Controls.Add(lblDescription);
        Controls.Add(txtDescription);
        Controls.Add(lblCount);
        Controls.Add(btnImport);
        Controls.Add(btnCancel);
        MinimumSize = new Size(700, 500);
        Name = "PnachImportForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Select PNACH Cheats";
        ResumeLayout(false);
        PerformLayout();
    }
}
