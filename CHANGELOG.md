# Changelog

All notable changes to ZipAll will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to a `MAJOR.MINOR.BUILD` versioning scheme
(`MINOR` increments with each completed development phase, `BUILD` auto-increments
on every build, `MAJOR` moves to `1` at the v1.0.0 release).

## [Unreleased]

## [1.0.0] - 2026-07-24
### Added
- `CONTRIBUTING.md`: contribution ground rules (C#/.NET/WinForms-only, English-only, no source comments, White theme only, Windows-only, BCL-only archive format, self-contained-only dependencies), PR checklist, and bug report template.
- `tests/ZipAll.Core.Tests`: xUnit unit test project (39 tests) covering `WildcardMatcher`, `ExclusionEngine`, `DirectoryWalker`, and `ArchiveWriter`/`ArchiveVerifier`, including empty folders, deeply nested trees, Unicode/special-character file names, and large flat trees; added to `ZipAll.sln`.
- `tests/REGRESSION_CHECKLIST.md`: manual regression checklist to run against a built installer before every release.
- `docs/screenshot.png`: real screenshot of the main window's Compress tab, referenced from `README.md`.
### Fixed
- `installer/ZipAll.iss`: `AppId` GUID was mis-escaped in the `#define`, causing the Inno Setup preprocessor to mistake it for an unknown `{constant}` and abort compilation; fixed by doubling the opening brace (`"{{...}"`).
- `installer/ZipAll.iss`: the `[Files]` wildcard entry excluded every file produced by the self-contained single-file publish (the `.exe` and its `.pdb`), leaving zero matches and aborting the build with "No files found matching"; fixed by adding the `skipifsourcedoesntexist` flag.
- `ArchiveWriter.CreateArchiveAsync` read `ZipArchiveEntry.Length`/`CompressedLength` after writing each entry to accumulate size statistics, but those properties unconditionally throw `InvalidOperationException` in `ZipArchiveMode.Create` — every archive containing at least one file was broken. Fixed by deriving the raw size from the already-known file length and the compressed size from the underlying zip stream's position delta (minus local file header overhead), found while writing the new unit test suite.
### Changed
- `README.md` fully rewritten from the Phase 0 stub: feature list, installation instructions, step-by-step usage guide, build instructions (debug/publish/installer for both `build.ps1` and `build.sh`), project layout, and a real screenshot.
- Local Git repository pushed to the newly created public GitHub repository at `https://github.com/Patrickjaillet/ZipAll`.
- Installer build and install/uninstall cycle verified end-to-end on a Windows 10 machine with the .NET SDK and Inno Setup 6 installed: `build.ps1 -Installer` now produces `dist/ZipAllSetup-<version>.exe`, which installs silently, launches correctly, and uninstalls cleanly with no residual files.
- Self-contained `win-x86` build published and smoke-tested (launches under WOW64 on a 64-bit Windows 10 host). Windows 11 and ARM64 remain untested — no such machine was available in this environment.
- Version stamp moved to `1.0.0` (`MAJOR` now `1`, marking the first stable release).

## [0.8.0] - 2026-07-24
### Added
- Multi-resolution application icon (`res/icons/app.ico`: 16, 24, 32, 48, 64, 128, 256 px), embedded in `ZipAll.exe` via `ApplicationIcon`, replacing the Phase 0 16×16 placeholder.
- Multi-resolution installer icon (`res/icons/installer.ico`, same size set), used as the Inno Setup wizard/uninstall icon; 256×256 source renders kept under `res/icons/src/` for future re-exports.
- Inno Setup script (`installer/ZipAll.iss`): installs the self-contained `dotnet publish` output to `Program Files`, creates a Start Menu shortcut and an optional desktop shortcut, registers a full uninstaller, and requires no separate .NET runtime on the target machine.
- `-Installer` / `--installer` build option in `build.ps1` / `build.sh`: publishes a fresh self-contained build, reads back its stamped version, and invokes `ISCC` to produce `dist/ZipAllSetup-<version>.exe`.
### Changed
- `installer/README.md` documents the installer build workflow, icon provenance, and the pre-release clean-VM smoke test checklist.
- `.gitignore` now excludes the installer output folder (`dist/`).

## [0.7.0] - 2026-07-24
### Added
- Long path support (source/destination paths over 260 characters) confirmed and hardened via the `longPathAware` setting in the application manifest, native to .NET on modern Windows.
- Read-only, hidden, and system source files are now always enumerated (directory traversal no longer skips any `FileAttributes`) and archived correctly, with their attributes preserved in the resulting `.zip` entry (`ZipArchiveEntry.ExternalAttributes`).
- Locked or access-denied files and directories are now skipped gracefully instead of aborting the whole archive: `DirectoryWalker` and `ArchiveWriter` catch `IOException`/`UnauthorizedAccessException` per entry, and `ArchiveResult.SkippedEntries` / the post-compression summary dialog report exactly what was skipped and why.
- Application manifest audited end-to-end: `asInvoker` execution level (no unnecessary elevation), Per-Monitor-V2 DPI awareness with a legacy fallback, and long-path awareness all confirmed present and correctly wired into the self-contained publish output.
### Changed
- `ArchiveFileEntry` now carries the source file's `FileAttributes` alongside its path, threaded from `DirectoryWalker` into `ArchiveWriter`.
- `ArchiveResult.EntryCount` now reflects files actually written to the archive (excluding skipped entries); a new `SkippedEntries` list exposes what was skipped.
- Resource management audit across the WinForms app: the extracted application icon, the About-tab bitmap, the compression `CancellationTokenSource`, and the `Process` handle used to open links are all disposed deterministically; no behavior change, no more handle/stream leaks.

## [0.6.0] - 2026-07-24
### Added
- Deflate compression via `System.IO.Compression.CompressionLevel.Optimal`, selectable per archive alongside the existing Stored method through a new "Compression" section in the GUI.
- Automatic per-file fallback to Stored: files up to 64 MB are test-compressed in memory first, and only written as Deflate when the compressed size is actually smaller than the raw size; larger files stream directly with Deflate to avoid excessive memory use.
- `ArchiveResult` now reports compression benchmarking data: total compressed bytes, compression ratio, stored/deflated entry counts, and elapsed time, all surfaced in the post-compression summary dialog and status bar.
### Changed
- `ArchiveWriter.CreateArchiveAsync` takes a new `ZipCompressionMode` parameter (`Stored` or `Deflate`, defaulting to `Deflate`) in addition to the existing exclusion and progress-reporting parameters.

## [0.5.0] - 2026-07-24
### Added
- "About" tab fully branded: application icon (extracted from the embedded `.exe` resource), application name, version read from assembly metadata, copyright notice, and clickable email/website links opening via `Process.Start` (default mail client / default browser).
- Application icon now also set as the main window's title-bar icon, sourced from the same embedded `.ico` used for the `.exe` resources (`ApplicationIcon` in the project file).
### Changed
- Confirmed White theme styling is applied consistently across every form, tab, and dialog in the application.

## [0.4.0] - 2026-07-24
### Added
- Graphical user interface (WinForms, White theme): main window with a "Compress" tab and an "About" tab (`TabControl`).
- Source directory selection and destination folder selection via `FolderBrowserDialog`.
- Editable archive name field (`.zip` extension appended automatically if omitted).
- Exclusion list UI: add individual files or whole folders to the exclusion list, remove entries, backed by the existing `ExclusionEngine`.
- Progress bar and status label updated live during compression via `IProgress<T>` and `async`/`await`.
- Start / Cancel controls, with cancellation propagated through a `CancellationToken` into the archiving pipeline.
- Input validation with native `MessageBox.Show` dialogs for invalid source/destination/archive name.
- Post-compression integrity check via `ArchiveVerifier`, with a summary dialog reporting file count and total size.

## [0.3.0] - 2026-07-24
### Added
- `WildcardMatcher`: hand-rolled `*`/`?` wildcard-to-Regex matcher (case-insensitive, no third-party dependency).
- `ExclusionEngine`: holds separate directory and file exclusion pattern lists; each pattern can be an absolute path, a path relative to the source root, or a wildcard matched against either the relative path or the leaf name.
- Directory walker now prunes excluded directories before descending into them, so large excluded trees (e.g. `node_modules`) are never enumerated.
- `tests/ExclusionHarness`: builds a 120-level-deep nested tree plus `node_modules`/`bin`/`obj` folders and `*.log`/`*.tmp`/`Thumbs.db` files, applies exclusion rules (name-based, relative-path-based, and one absolute-path rule), and asserts nothing excluded leaks into the walker output or the resulting archive.

## [0.2.0] - 2026-07-24
### Added
- Core ZIP engine (`ZipAll.Core`): writes archives with `System.IO.Compression.ZipArchive` using the Stored method (no compression yet — that arrives in Phase 5).
- Directory tree walker that recursively enumerates files and produces archive-relative paths.
- Integrity verification pass that reads every entry back and relies on `ZipArchive`'s built-in CRC32 check, comparing the entry count against the source folder.
- Async, cancellable file I/O for both reading source files and writing archive entries.
- Full Unicode path support end-to-end (native to .NET strings), verified with round-tripped Unicode filenames.
- Manual test harness project (`tests/ManualHarness`) that builds a sample nested folder tree, archives it, and verifies the round trip; independently re-verified with Python's `zipfile` module as a second, unrelated archive reader.

## [0.1.0] - 2026-07-24
### Added
- Initial project scaffolding: `src/`, `res/`, `docs/`, `installer/`, `tests/` folder structure.
- Git repository initialized with `.gitignore`, MIT `LICENSE`, `README.md` and `CHANGELOG.md` stubs.
- Build script for `dotnet build` / self-contained single-file `dotnet publish`.
- Automatic build/version stamping via `Directory.Build.props`.
- Minimal WinForms application shell (empty main form) as proof of concept.
- Placeholder icons for the application and the installer.
