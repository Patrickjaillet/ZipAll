# Installer

This folder contains the Inno Setup script that packages ZipAll for end users.

- **`ZipAll.iss`** — Inno Setup 6 script. Installs the self-contained
  `dotnet publish` output from `../publish/` into `Program Files`, creates a
  Start Menu shortcut (and an optional desktop shortcut), registers an
  uninstaller, and uses `../res/icons/installer.ico` as the setup wizard icon.
  Because the published app is self-contained, the target machine needs no
  separately installed .NET runtime.

## Building the installer

Requires [Inno Setup 6](https://jrsoftware.org/isinfo.php) (`ISCC.exe` on
`PATH`) on Windows, or `iscc` under Wine on Linux/macOS.

```powershell
# From the repository root
.\build.ps1 -Installer
```

```bash
# From the repository root
./build.sh --installer
```

Both commands:
1. Run a fresh self-contained, single-file `dotnet publish` (`win-x64`,
   Release) into `../publish/`.
2. Read back the version stamped into the published executable
   (`Directory.Build.props` → `MAJOR.MINOR.BUILD`).
3. Invoke `ISCC` on `ZipAll.iss` with that version, writing
   `ZipAllSetup-<version>.exe` into `../dist/`.

You can also invoke Inno Setup directly for a one-off build with a specific
version:

```
ISCC.exe /DMyAppVersion=0.8.0.0 ZipAll.iss
```

## Icons

- `../res/icons/app.ico` — multi-resolution (16 to 256 px) application icon,
  embedded in `ZipAll.exe` via `ApplicationIcon` in `ZipAll.csproj`.
- `../res/icons/installer.ico` — multi-resolution (16 to 256 px) icon used
  for the Setup wizard and uninstall entry (`SetupIconFile` in `ZipAll.iss`).
- `../res/icons/src/*.png` — 256×256 source renders the two `.ico` files were
  generated from, kept for reference if the icons ever need to be redrawn or
  re-exported at additional sizes.

## Testing

The compiled installer should be smoke-tested on a clean Windows 10/11 VM
before every release: fresh install, shortcuts launch the app, uninstall
removes all files with nothing left behind in `Program Files` or the Start
Menu.
