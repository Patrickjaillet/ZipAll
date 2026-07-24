# Manual Regression Checklist

Run this checklist against a freshly built installer (`dist/ZipAllSetup-<version>.exe`) before every release, on real Windows hardware or a VM. Automated coverage (`dotnet test`, `ExclusionHarness`, `ManualHarness`) should also pass before starting this list.

## Install / uninstall
- [ ] Installer runs and completes without errors on a clean machine (no prior ZipAll install).
- [ ] Start Menu shortcut is created and launches the app.
- [ ] Optional desktop shortcut is created when the task is selected during install.
- [ ] Optional "Send to" shortcut is created when the task is selected during install (`%APPDATA%\Microsoft\Windows\SendTo\ZipAll.lnk`).
- [ ] Uninstaller removes the application directory, Start Menu shortcuts, desktop shortcut, and Send To shortcut, leaving no residual files.
- [ ] Reinstalling over an existing installation succeeds (upgrade path).

## Core compression flow
- [ ] Selecting a source directory populates the field via `FolderBrowserDialog`.
- [ ] Selecting a destination folder populates the field via `FolderBrowserDialog`.
- [ ] Default archive name is editable and validated (rejects empty/invalid names with a `MessageBox`).
- [ ] Compressing a small folder with Deflate produces a valid `.zip` openable in Windows Explorer / another archive tool.
- [ ] Compressing the same folder with Stored produces a valid, larger (uncompressed) `.zip`.
- [ ] Progress bar and status label update during compression.
- [ ] Cancel button stops an in-progress compression and leaves no partial `.zip` locked or corrupting the destination folder.

## Exclusions
- [ ] Adding a file exclusion (name/wildcard) via the UI removes matching files from the resulting archive.
- [ ] Adding a folder exclusion via the UI removes the entire subtree from the resulting archive.
- [ ] Removing an exclusion from the list restores the corresponding files/folders on the next run.

## Drag & drop
- [ ] Dragging a folder from Windows Explorer onto the Source group or its text field sets it as the source directory and, if the archive name is still the default, prefills the archive name from the folder name.
- [ ] Dragging a single file (not a folder) is rejected (no drop effect / no change).
- [ ] Drag & drop is disabled while a compression is in progress.

## Password protection
- [ ] Checking "Password-protect" enables the password field; unchecking it disables and ignores it.
- [ ] Starting compression with the box checked and an empty password shows a validation `MessageBox` instead of proceeding.
- [ ] A password-protected archive opens in a real archive tool (7-Zip, Windows Explorer) only after entering the correct password.
- [ ] Entering the wrong password when opening the archive in a real archive tool fails to extract.

## Command-line mode
- [ ] `ZipAll.exe --help` (or `-h`) prints usage and exits without opening the GUI, when run from a console.
- [ ] `ZipAll.exe --source <dir> --dest <dir>` compresses headlessly and exits 0 on success, with progress lines printed to the console.
- [ ] `--exclude-file` / `--exclude-dir` (repeatable) exclude matching entries, matching GUI behavior.
- [ ] `--password <pwd>` produces a password-protected archive, verified internally before exit.
- [ ] An invalid/missing required argument (e.g. missing `--dest`) prints an error and usage, and exits with a non-zero code.
- [ ] Running `ZipAll.exe` with no arguments still opens the GUI as before.

## Explorer "Send to" integration
- [ ] Right-click a folder in Explorer → Send To → ZipAll opens the GUI with that folder pre-filled as the source and the archive name defaulted to the folder name.

## Edge cases
- [ ] Source folder containing an empty subfolder compresses without error (folder itself is not required to appear as a zip entry).
- [ ] Source folder containing Unicode/special-character file and folder names round-trips correctly.
- [ ] Source folder containing a very large number of files (several hundred+) completes without UI freeze (progress reporting stays responsive).
- [ ] Source path longer than 260 characters compresses successfully (long path support).
- [ ] A locked/in-use file in the source folder is skipped gracefully, with the skip reported to the user, instead of aborting the whole archive.
- [ ] Read-only, hidden, and system source files are archived and their attributes are preserved in the resulting entries.

## About tab / branding
- [ ] About tab shows the correct application name and version (matches the installer's `AppVersion`).
- [ ] Copyright, email, and website are correct and the website link opens the default browser.
- [ ] Application icon appears correctly in the title bar, taskbar, and Start Menu shortcut.

## Packaging
- [ ] The installed app runs without requiring a separately installed .NET runtime (self-contained deployment).
- [ ] No debug symbols (`.pdb`) are present in the installed directory.
