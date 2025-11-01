; ==========================================
; Kiosk Suite Installer - Dual App Version
; ==========================================

[Setup]
AppName=Kiosk Suite
AppVersion=2.0.0
DefaultDirName={autopf}\KioskSuite
DefaultGroupName=Kiosk Suite
OutputDir=.\Output
OutputBaseFilename=KioskSuite_Installer
PrivilegesRequired=admin
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\KioskBrowser.exe

[Files]
; Include all EXEs and dependencies
Source: "..\AdminUI\bin\x64\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\src\KioskBrowser\bin\x64\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Dirs]
Name: "{app}\Assets\Data"; Permissions: users-full
[Icons]
; Start Menu shortcuts
Name: "{autoprograms}\Kiosk Suite\Kiosk Browser"; Filename: "{app}\KioskBrowser.exe"
Name: "{autoprograms}\Kiosk Suite\Admin Panel"; Filename: "{app}\AdminUI.exe"

; Desktop shortcuts
Name: "{autodesktop}\Kiosk Browser"; Filename: "{app}\KioskBrowser.exe"; Tasks: desktopicon
Name: "{autodesktop}\Kiosk Admin Panel"; Filename: "{app}\AdminUI.exe"; Tasks: desktopicon

[Registry]
; (Optional) Store install paths for your admin panel
Root: HKLM; Subkey: "SOFTWARE\KioskSuite"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"
Root: HKLM; Subkey: "SOFTWARE\KioskSuite"; ValueType: string; ValueName: "KioskBrowserPath"; ValueData: "{app}\KioskBrowser.exe"
Root: HKLM; Subkey: "SOFTWARE\KioskSuite"; ValueType: string; ValueName: "AdminUIPath"; ValueData: "{app}\AdminUI.exe"

[Tasks]
Name: "desktopicon"; Description: "Create desktop shortcuts"; GroupDescription: "Additional Icons:"; Flags: unchecked

[Run]
; Optional: Auto-launch Admin Panel after installation
Filename: "{app}\AdminUI.exe"; Description: "Launch Admin Panel"; Flags: nowait postinstall skipifsilent
