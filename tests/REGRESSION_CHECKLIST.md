# Manual Regression Checklist

Run this checklist against a freshly built installer (`dist/ZipAllSetup-<version>.exe`) before every release, on real Windows hardware or a VM. Automated coverage (`dotnet test`, `ExclusionHarness`, `ManualHarness`) should also pass before starting this list.

## Install / uninstall
- [ ] Installer runs and completes without errors on a clean machine (no prior ZipAll install).
- [ ] Start Menu shortcut is created and launches the app.
- [ ] Optional desktop shortcut is created when the task is selected during install.
- [ ] Uninstaller removes the application directory, Start Menu shortcuts, and desktop shortcut, leaving no residual files.
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
