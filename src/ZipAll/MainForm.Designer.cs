namespace ZipAll;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private TabControl tabControl;
    private TabPage tabCompress;
    private TabPage tabAbout;

    private GroupBox grpSource;
    private Label lblSourceDirectory;
    private TextBox txtSourceDirectory;
    private Button btnBrowseSource;

    private GroupBox grpDestination;
    private Label lblArchiveName;
    private TextBox txtArchiveName;
    private Label lblDestinationFolder;
    private TextBox txtDestinationFolder;
    private Button btnBrowseDestination;

    private GroupBox grpCompression;
    private RadioButton rbDeflate;
    private RadioButton rbStored;
    private CheckBox chkPasswordProtect;
    private TextBox txtPassword;

    private GroupBox grpExclusions;
    private ListBox lstExclusions;
    private Button btnAddFileExclusion;
    private Button btnAddFolderExclusion;
    private Button btnRemoveExclusion;

    private ProgressBar progressBar;
    private Label lblStatus;
    private Button btnStart;
    private Button btnCancel;

    private PictureBox picAboutIcon;
    private Label lblAboutName;
    private Label lblAboutVersion;
    private Label lblAboutCopyright;
    private LinkLabel lnkAboutEmail;
    private LinkLabel lnkAboutWebsite;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _appIcon?.Dispose();
            picAboutIcon.Image?.Dispose();
            _cts?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        tabControl = new TabControl();
        tabCompress = new TabPage();
        tabAbout = new TabPage();

        grpSource = new GroupBox();
        lblSourceDirectory = new Label();
        txtSourceDirectory = new TextBox();
        btnBrowseSource = new Button();

        grpDestination = new GroupBox();
        lblArchiveName = new Label();
        txtArchiveName = new TextBox();
        lblDestinationFolder = new Label();
        txtDestinationFolder = new TextBox();
        btnBrowseDestination = new Button();

        grpCompression = new GroupBox();
        rbDeflate = new RadioButton();
        rbStored = new RadioButton();
        chkPasswordProtect = new CheckBox();
        txtPassword = new TextBox();

        grpExclusions = new GroupBox();
        lstExclusions = new ListBox();
        btnAddFileExclusion = new Button();
        btnAddFolderExclusion = new Button();
        btnRemoveExclusion = new Button();

        progressBar = new ProgressBar();
        lblStatus = new Label();
        btnStart = new Button();
        btnCancel = new Button();

        picAboutIcon = new PictureBox();
        lblAboutName = new Label();
        lblAboutVersion = new Label();
        lblAboutCopyright = new Label();
        lnkAboutEmail = new LinkLabel();
        lnkAboutWebsite = new LinkLabel();

        tabControl.SuspendLayout();
        tabCompress.SuspendLayout();
        tabAbout.SuspendLayout();
        grpSource.SuspendLayout();
        grpDestination.SuspendLayout();
        grpCompression.SuspendLayout();
        grpExclusions.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picAboutIcon).BeginInit();
        SuspendLayout();

        grpSource.BackColor = Color.White;
        grpSource.Location = new Point(12, 12);
        grpSource.Size = new Size(600, 60);
        grpSource.Text = "Source";
        grpSource.Controls.Add(lblSourceDirectory);
        grpSource.Controls.Add(txtSourceDirectory);
        grpSource.Controls.Add(btnBrowseSource);

        lblSourceDirectory.AutoSize = true;
        lblSourceDirectory.Location = new Point(10, 27);
        lblSourceDirectory.Text = "Source directory:";

        txtSourceDirectory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSourceDirectory.Location = new Point(140, 24);
        txtSourceDirectory.ReadOnly = true;
        txtSourceDirectory.Size = new Size(370, 23);

        btnBrowseSource.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseSource.Location = new Point(516, 23);
        btnBrowseSource.Size = new Size(75, 25);
        btnBrowseSource.Text = "Browse...";
        btnBrowseSource.UseVisualStyleBackColor = true;
        btnBrowseSource.Click += btnBrowseSource_Click;

        grpDestination.BackColor = Color.White;
        grpDestination.Location = new Point(12, 80);
        grpDestination.Size = new Size(600, 100);
        grpDestination.Text = "Destination";
        grpDestination.Controls.Add(lblArchiveName);
        grpDestination.Controls.Add(txtArchiveName);
        grpDestination.Controls.Add(lblDestinationFolder);
        grpDestination.Controls.Add(txtDestinationFolder);
        grpDestination.Controls.Add(btnBrowseDestination);

        lblArchiveName.AutoSize = true;
        lblArchiveName.Location = new Point(10, 27);
        lblArchiveName.Text = "Archive name:";

        txtArchiveName.Location = new Point(140, 24);
        txtArchiveName.Size = new Size(200, 23);
        txtArchiveName.Text = "archive";

        lblDestinationFolder.AutoSize = true;
        lblDestinationFolder.Location = new Point(10, 63);
        lblDestinationFolder.Text = "Destination folder:";

        txtDestinationFolder.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtDestinationFolder.Location = new Point(140, 60);
        txtDestinationFolder.ReadOnly = true;
        txtDestinationFolder.Size = new Size(370, 23);

        btnBrowseDestination.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnBrowseDestination.Location = new Point(516, 59);
        btnBrowseDestination.Size = new Size(75, 25);
        btnBrowseDestination.Text = "Browse...";
        btnBrowseDestination.UseVisualStyleBackColor = true;
        btnBrowseDestination.Click += btnBrowseDestination_Click;

        grpCompression.BackColor = Color.White;
        grpCompression.Location = new Point(12, 188);
        grpCompression.Size = new Size(600, 80);
        grpCompression.Text = "Compression";
        grpCompression.Controls.Add(rbDeflate);
        grpCompression.Controls.Add(rbStored);
        grpCompression.Controls.Add(chkPasswordProtect);
        grpCompression.Controls.Add(txtPassword);

        rbDeflate.AutoSize = true;
        rbDeflate.Checked = true;
        rbDeflate.Location = new Point(10, 22);
        rbDeflate.Size = new Size(280, 20);
        rbDeflate.TabStop = true;
        rbDeflate.Text = "Deflate (compress, with automatic Stored fallback)";
        rbDeflate.UseVisualStyleBackColor = true;

        rbStored.AutoSize = true;
        rbStored.Location = new Point(320, 22);
        rbStored.Size = new Size(200, 20);
        rbStored.TabStop = true;
        rbStored.Text = "Stored (no compression)";
        rbStored.UseVisualStyleBackColor = true;

        chkPasswordProtect.AutoSize = true;
        chkPasswordProtect.Location = new Point(10, 52);
        chkPasswordProtect.Size = new Size(130, 20);
        chkPasswordProtect.Text = "Password-protect:";
        chkPasswordProtect.UseVisualStyleBackColor = true;
        chkPasswordProtect.CheckedChanged += chkPasswordProtect_CheckedChanged;

        txtPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtPassword.Enabled = false;
        txtPassword.Location = new Point(150, 50);
        txtPassword.PasswordChar = '●';
        txtPassword.Size = new Size(300, 23);

        grpExclusions.BackColor = Color.White;
        grpExclusions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        grpExclusions.Location = new Point(12, 276);
        grpExclusions.Size = new Size(600, 160);
        grpExclusions.Text = "Exclusions";
        grpExclusions.Controls.Add(lstExclusions);
        grpExclusions.Controls.Add(btnAddFileExclusion);
        grpExclusions.Controls.Add(btnAddFolderExclusion);
        grpExclusions.Controls.Add(btnRemoveExclusion);

        lstExclusions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        lstExclusions.Location = new Point(10, 22);
        lstExclusions.Size = new Size(470, 158);

        btnAddFileExclusion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAddFileExclusion.Location = new Point(490, 22);
        btnAddFileExclusion.Size = new Size(100, 25);
        btnAddFileExclusion.Text = "Add File...";
        btnAddFileExclusion.UseVisualStyleBackColor = true;
        btnAddFileExclusion.Click += btnAddFileExclusion_Click;

        btnAddFolderExclusion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnAddFolderExclusion.Location = new Point(490, 53);
        btnAddFolderExclusion.Size = new Size(100, 25);
        btnAddFolderExclusion.Text = "Add Folder...";
        btnAddFolderExclusion.UseVisualStyleBackColor = true;
        btnAddFolderExclusion.Click += btnAddFolderExclusion_Click;

        btnRemoveExclusion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRemoveExclusion.Location = new Point(490, 84);
        btnRemoveExclusion.Size = new Size(100, 25);
        btnRemoveExclusion.Text = "Remove";
        btnRemoveExclusion.UseVisualStyleBackColor = true;
        btnRemoveExclusion.Click += btnRemoveExclusion_Click;

        progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(12, 446);
        progressBar.Size = new Size(600, 20);

        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStatus.AutoEllipsis = true;
        lblStatus.Location = new Point(12, 470);
        lblStatus.Size = new Size(600, 20);
        lblStatus.Text = "Ready.";

        btnStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnStart.Location = new Point(432, 498);
        btnStart.Size = new Size(85, 30);
        btnStart.Text = "Start";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;

        btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        btnCancel.Enabled = false;
        btnCancel.Location = new Point(527, 498);
        btnCancel.Size = new Size(85, 30);
        btnCancel.Text = "Cancel";
        btnCancel.UseVisualStyleBackColor = true;
        btnCancel.Click += btnCancel_Click;

        tabCompress.BackColor = Color.White;
        tabCompress.Text = "Compress";
        tabCompress.UseVisualStyleBackColor = true;
        tabCompress.Controls.Add(grpSource);
        tabCompress.Controls.Add(grpDestination);
        tabCompress.Controls.Add(grpCompression);
        tabCompress.Controls.Add(grpExclusions);
        tabCompress.Controls.Add(progressBar);
        tabCompress.Controls.Add(lblStatus);
        tabCompress.Controls.Add(btnStart);
        tabCompress.Controls.Add(btnCancel);

        picAboutIcon.Location = new Point(24, 24);
        picAboutIcon.Size = new Size(48, 48);
        picAboutIcon.SizeMode = PictureBoxSizeMode.Zoom;

        lblAboutName.AutoSize = true;
        lblAboutName.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblAboutName.Location = new Point(84, 24);
        lblAboutName.Text = "ZipAll";

        lblAboutVersion.AutoSize = true;
        lblAboutVersion.Location = new Point(84, 58);
        lblAboutVersion.Text = "Version";

        lblAboutCopyright.AutoSize = true;
        lblAboutCopyright.Location = new Point(24, 96);
        lblAboutCopyright.Text = "Copyright \u00A9 2026 Patrick JAILLET";

        lnkAboutEmail.AutoSize = true;
        lnkAboutEmail.Location = new Point(24, 122);
        lnkAboutEmail.Text = "contact.shaderstudio@gmail.com";
        lnkAboutEmail.LinkClicked += lnkAboutEmail_LinkClicked;

        lnkAboutWebsite.AutoSize = true;
        lnkAboutWebsite.Location = new Point(24, 148);
        lnkAboutWebsite.Text = "https://patrickjaillet.github.io/sandefjord-software";
        lnkAboutWebsite.LinkClicked += lnkAboutWebsite_LinkClicked;

        tabAbout.BackColor = Color.White;
        tabAbout.Text = "About";
        tabAbout.UseVisualStyleBackColor = true;
        tabAbout.Controls.Add(picAboutIcon);
        tabAbout.Controls.Add(lblAboutName);
        tabAbout.Controls.Add(lblAboutVersion);
        tabAbout.Controls.Add(lblAboutCopyright);
        tabAbout.Controls.Add(lnkAboutEmail);
        tabAbout.Controls.Add(lnkAboutWebsite);

        tabControl.Dock = DockStyle.Fill;
        tabControl.Controls.Add(tabCompress);
        tabControl.Controls.Add(tabAbout);

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(632, 568);
        MinimumSize = new Size(560, 460);
        Controls.Add(tabControl);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ZipAll";

        grpSource.ResumeLayout(false);
        grpSource.PerformLayout();
        grpDestination.ResumeLayout(false);
        grpDestination.PerformLayout();
        grpCompression.ResumeLayout(false);
        grpCompression.PerformLayout();
        grpExclusions.ResumeLayout(false);
        tabCompress.ResumeLayout(false);
        tabAbout.ResumeLayout(false);
        tabAbout.PerformLayout();
        tabControl.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picAboutIcon).EndInit();
        ResumeLayout(false);
    }
}
