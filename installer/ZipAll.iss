; ZipAll installer script (Inno Setup 6.x)
;
; Packages the self-contained, single-file `dotnet publish` output from
; ..\publish\ (see ..\build.ps1 -Publish / ..\build.sh --publish) so the
; target machine needs no separately installed .NET runtime.
;
; Build with:
;   ISCC.exe ZipAll.iss
;   ISCC.exe /DMyAppVersion=0.8.123.0 ZipAll.iss     (override version, e.g. from CI)

#ifndef MyAppVersion
  #define MyAppVersion "0.8.0.0"
#endif

#define MyAppName "ZipAll"
#define MyAppPublisher "Patrick JAILLET"
#define MyAppURL "https://patrickjaillet.github.io/sandefjord-software"
#define MyAppExeName "ZipAll.exe"
#define MyAppId "{925D1032-1AFC-48B1-930C-43AB325955E0}"
#define PublishDir "..\publish"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright=Copyright (C) 2026 {#MyAppPublisher}
VersionInfoVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=..\dist
OutputBaseFilename=ZipAllSetup-{#MyAppVersion}
SetupIconFile=..\res\icons\installer.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
DisableDirPage=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main executable first, so it is easy to spot in the compiled script.
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; Everything else the self-contained publish produced (native runtime bits
; not embedded in the single file, if any). Debug symbols are left out of
; the end-user installer on purpose.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "{#MyAppExeName},*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
