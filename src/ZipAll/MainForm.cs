using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using ZipAll.Core;

namespace ZipAll;

public partial class MainForm : Form
{
    private readonly List<ExclusionItem> _exclusionItems = new();
    private CancellationTokenSource? _cts;
    private Icon? _appIcon;

    public MainForm()
    {
        InitializeComponent();
        lblAboutVersion.Text = "Version " + GetDisplayVersion();

        _appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        if (_appIcon is not null)
        {
            this.Icon = _appIcon;
            picAboutIcon.Image = _appIcon.ToBitmap();
        }

        grpSource.AllowDrop = true;
        grpSource.DragEnter += grpSource_DragEnter;
        grpSource.DragDrop += grpSource_DragDrop;
        txtSourceDirectory.AllowDrop = true;
        txtSourceDirectory.DragEnter += grpSource_DragEnter;
        txtSourceDirectory.DragDrop += grpSource_DragDrop;
    }

    public void SetSourceDirectory(string directory)
    {
        txtSourceDirectory.Text = directory;

        if (string.IsNullOrWhiteSpace(txtArchiveName.Text) || txtArchiveName.Text == "archive")
        {
            var folderName = new DirectoryInfo(directory).Name;

            if (!string.IsNullOrEmpty(folderName) && folderName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
            {
                txtArchiveName.Text = folderName;
            }
        }
    }

    private static void grpSource_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = TryGetDroppedDirectory(e.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private void grpSource_DragDrop(object? sender, DragEventArgs e)
    {
        if (TryGetDroppedDirectory(e.Data, out var directory))
        {
            SetSourceDirectory(directory);
        }
    }

    private static bool TryGetDroppedDirectory(IDataObject? data, out string directory)
    {
        directory = string.Empty;

        if (data is null || !data.GetDataPresent(DataFormats.FileDrop))
        {
            return false;
        }

        if (data.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length == 0)
        {
            return false;
        }

        var candidate = paths[0];

        if (!Directory.Exists(candidate))
        {
            return false;
        }

        directory = candidate;
        return true;
    }

    private void btnBrowseSource_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Source Directory",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtSourceDirectory.Text = dialog.SelectedPath;
        }
    }

    private void btnBrowseDestination_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Destination Folder",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtDestinationFolder.Text = dialog.SelectedPath;
        }
    }

    private void btnAddFileExclusion_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select File to Exclude",
            CheckFileExists = true,
            Multiselect = true,
        };

        if (Directory.Exists(txtSourceDirectory.Text))
        {
            dialog.InitialDirectory = txtSourceDirectory.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            foreach (var fileName in dialog.FileNames)
            {
                AddExclusion(fileName, isDirectory: false);
            }
        }
    }

    private void btnAddFolderExclusion_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select Folder to Exclude",
        };

        if (Directory.Exists(txtSourceDirectory.Text))
        {
            dialog.SelectedPath = txtSourceDirectory.Text;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddExclusion(dialog.SelectedPath, isDirectory: true);
        }
    }

    private void btnRemoveExclusion_Click(object? sender, EventArgs e)
    {
        var index = lstExclusions.SelectedIndex;

        if (index < 0)
        {
            return;
        }

        _exclusionItems.RemoveAt(index);
        lstExclusions.Items.RemoveAt(index);
    }

    private async void btnStart_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs(out var sourceDirectory, out var destinationZipPath))
        {
            return;
        }

        var password = chkPasswordProtect.Checked ? txtPassword.Text : null;
        var exclusions = BuildExclusionEngine();

        SetBusyState(true);
        _cts = new CancellationTokenSource();

        try
        {
            lblStatus.Text = "Scanning source directory...";

            var totalFiles = await Task.Run(
                () => DirectoryWalker.EnumerateFiles(sourceDirectory, exclusions).Count(),
                _cts.Token);

            progressBar.Maximum = Math.Max(totalFiles, 1);
            progressBar.Value = 0;

            var compressionMode = rbStored.Checked ? ZipCompressionMode.Stored : ZipCompressionMode.Deflate;

            var processed = 0;
            var progress = new Progress<ArchiveFileEntry>(entry =>
            {
                processed++;
                progressBar.Value = Math.Min(processed, progressBar.Maximum);
                lblStatus.Text = $"Compressing {processed} of {totalFiles}: {entry.RelativePath}";
            });

            var result = string.IsNullOrEmpty(password)
                ? await ArchiveWriter.CreateArchiveAsync(
                    sourceDirectory,
                    destinationZipPath,
                    exclusions,
                    compressionMode,
                    progress,
                    _cts.Token)
                : await EncryptedArchiveWriter.CreateArchiveAsync(
                    sourceDirectory,
                    destinationZipPath,
                    password,
                    exclusions,
                    compressionMode,
                    progress,
                    _cts.Token);

            var verification = string.IsNullOrEmpty(password)
                ? ArchiveVerifier.Verify(destinationZipPath, result.EntryCount)
                : EncryptedArchiveVerifier.Verify(destinationZipPath, password, result.EntryCount);

            if (verification.Success)
            {
                lblStatus.Text = result.SkippedEntries.Count == 0
                    ? $"Done: {result.EntryCount} files, {result.TotalCompressedBytes:N0} bytes ({result.CompressionRatio:P1} smaller), {result.Elapsed.TotalSeconds:F1}s."
                    : $"Done with {result.SkippedEntries.Count} file(s) skipped: {result.EntryCount} files, {result.TotalCompressedBytes:N0} bytes ({result.CompressionRatio:P1} smaller), {result.Elapsed.TotalSeconds:F1}s.";

                var summary =
                    "Archive created successfully." +
                    $"\n\nFiles: {result.EntryCount} ({result.DeflatedEntryCount} compressed, {result.StoredEntryCount} stored)" +
                    $"\nOriginal size: {result.TotalBytesWritten:N0} bytes" +
                    $"\nArchive size: {result.TotalCompressedBytes:N0} bytes" +
                    $"\nCompression ratio: {result.CompressionRatio:P1}" +
                    $"\nElapsed: {result.Elapsed.TotalSeconds:F1} s" +
                    $"\nDestination: {destinationZipPath}";

                if (result.SkippedEntries.Count > 0)
                {
                    var skippedList = string.Join(
                        Environment.NewLine,
                        result.SkippedEntries.Take(10).Select(s => $"  {s.Path} ({s.Reason})"));

                    summary += $"\n\n{result.SkippedEntries.Count} file(s) could not be read (locked or inaccessible) and were skipped:\n{skippedList}";

                    if (result.SkippedEntries.Count > 10)
                    {
                        summary += $"\n  ... and {result.SkippedEntries.Count - 10} more.";
                    }
                }

                MessageBox.Show(
                    this,
                    summary,
                    "ZipAll",
                    MessageBoxButtons.OK,
                    result.SkippedEntries.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            else
            {
                lblStatus.Text = "Verification failed.";
                MessageBox.Show(
                    this,
                    verification.FailureReason ?? "Archive verification failed.",
                    "ZipAll",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (OperationCanceledException)
        {
            lblStatus.Text = "Cancelled.";
            progressBar.Value = 0;
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Error.";
            MessageBox.Show(this, ex.Message, "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetBusyState(false);
        }
    }

    private void chkPasswordProtect_CheckedChanged(object? sender, EventArgs e)
    {
        txtPassword.Enabled = chkPasswordProtect.Checked;
    }

    private void btnCancel_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
        lblStatus.Text = "Cancelling...";
    }

    private void lnkAboutEmail_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLink("mailto:contact.shaderstudio@gmail.com");
    }

    private void lnkAboutWebsite_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
    {
        OpenLink("https://patrickjaillet.github.io/sandefjord-software");
    }

    private void OpenLink(string url)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateInputs(out string sourceDirectory, out string destinationZipPath)
    {
        sourceDirectory = txtSourceDirectory.Text.Trim();
        destinationZipPath = string.Empty;

        if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
        {
            MessageBox.Show(this, "Please select a valid source directory.", "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var archiveName = txtArchiveName.Text.Trim();

        if (string.IsNullOrEmpty(archiveName))
        {
            MessageBox.Show(this, "Please enter an archive name.", "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (archiveName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            MessageBox.Show(this, "The archive name contains invalid characters.", "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!archiveName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            archiveName += ".zip";
        }

        var destinationFolder = txtDestinationFolder.Text.Trim();

        if (string.IsNullOrEmpty(destinationFolder) || !Directory.Exists(destinationFolder))
        {
            MessageBox.Show(this, "Please select a valid destination folder.", "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (chkPasswordProtect.Checked && string.IsNullOrEmpty(txtPassword.Text))
        {
            MessageBox.Show(this, "Please enter a password, or uncheck password protection.", "ZipAll", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        destinationZipPath = Path.Combine(destinationFolder, archiveName);
        return true;
    }

    private ExclusionEngine BuildExclusionEngine()
    {
        var directoryPatterns = _exclusionItems.Where(item => item.IsDirectory).Select(item => item.Pattern);
        var filePatterns = _exclusionItems.Where(item => !item.IsDirectory).Select(item => item.Pattern);
        return new ExclusionEngine(directoryPatterns, filePatterns);
    }

    private void AddExclusion(string path, bool isDirectory)
    {
        if (_exclusionItems.Any(item => item.IsDirectory == isDirectory && string.Equals(item.Pattern, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _exclusionItems.Add(new ExclusionItem(path, isDirectory));
        lstExclusions.Items.Add(FormatExclusion(path, isDirectory));
    }

    private void SetBusyState(bool busy)
    {
        btnBrowseSource.Enabled = !busy;
        btnBrowseDestination.Enabled = !busy;
        btnAddFileExclusion.Enabled = !busy;
        btnAddFolderExclusion.Enabled = !busy;
        btnRemoveExclusion.Enabled = !busy;
        txtArchiveName.Enabled = !busy;
        rbDeflate.Enabled = !busy;
        rbStored.Enabled = !busy;
        chkPasswordProtect.Enabled = !busy;
        txtPassword.Enabled = !busy && chkPasswordProtect.Checked;
        lstExclusions.Enabled = !busy;
        btnStart.Enabled = !busy;
        btnCancel.Enabled = busy;
        grpSource.AllowDrop = !busy;
        txtSourceDirectory.AllowDrop = !busy;
    }

    private static string FormatExclusion(string path, bool isDirectory) =>
        isDirectory ? $"[Folder] {path}" : $"[File] {path}";

    private static string GetDisplayVersion()
    {
        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        return informationalVersion ?? Application.ProductVersion;
    }

    private readonly record struct ExclusionItem(string Pattern, bool IsDirectory);
}
