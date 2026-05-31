; Kaldforge Inno Setup Script
; Creates a Windows installer for the Kaldforge application.

#define MyAppName "Kaldforge"
#define MyAppVersion "1.0.0.0"
#define MyAppPublisher "Kaldforge Studio"
#define MyAppURL "https://kaldforge.com"
#define MyAppExeName "Kaldforge.exe"

; Paths relative to this script file's directory
#define SourceDir "..\ProgramStarter.App\bin\Release\net10.0-windows\win-x64\publish"
#define IconPath "..\ProgramStarter.App\Assets\AppIcon\kaldforge.ico"

[Setup]
; Basic metadata
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation defaults
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}

; Output
OutputDir=Output
OutputBaseFilename=KaldforgeSetup

; Icon for the installer and uninstaller entries
SetupIconFile={#IconPath}
UninstallDisplayIcon={#IconPath}

; Compression
Compression=lzma2/max
SolidCompression=yes

; Windows compatibility - supports Windows 10 and later
MinVersion=10.0

; Privileges: lowest means admin is only prompted when the chosen
; install path (e.g. Program Files) requires it.
PrivilegesRequired=lowest

; Standard uninstall behavior
Uninstallable=yes

; Modern wizard style
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
; The main application executable and all its dependencies
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Start Menu shortcut
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"

; Optional Desktop shortcut
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Optionally launch the app after installation (unchecked by default)
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Kaldforge"; Flags: nowait postinstall skipifsilent unchecked

[UninstallRun]
; Clean up any files the app may have created (optional)

[Code]
// Ensure the uninstall entry appears in Windows Apps & Features list.
// Inno Setup handles this automatically via the [Setup] section settings.
