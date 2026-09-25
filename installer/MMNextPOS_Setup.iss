; =====================================================================
; MMNextPOS Installer Setup Script (Inno Setup 6)
; =====================================================================
; Features:
; - Self-contained win-x64 publish output (no .NET runtime required)
; - Start Menu + optional Desktop shortcut
; - Pyidaungsu Myanmar font registration (Regular / Bold / Numbers)
; - Optional bundled portable MySQL (compile with ISCC /DBundleMySQL)
; - Clean uninstaller registration
;
; Build via: scripts\Build-Installer.ps1
; =====================================================================

#ifndef AppVersion
  #define AppVersion "2.0.0"
#endif
#ifndef AppYear
  #define AppYear GetDateTimeString('yyyy', '', '');
#endif

#define MyAppName "MMNext POS"
#define MyAppPublisher "MMNext-Org"
#define MyAppURL "https://github.com/MMNext-Org/MMNext-POS"
#define MyAppExeName "MMNextPOS.WinForms.exe"

; Staging root produced by scripts\Build-Installer.ps1 (relative to this file)
#ifndef StageDir
  #define StageDir "..\artifacts\installer"
#endif

[Setup]
AppId={{1EEDA6ED-5C2C-485F-84E7-C7EF329ADFF8}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir={#StageDir}
OutputBaseFilename=MMNextPOS_Setup_v{#AppVersion}
SetupIconFile=..\src\MMNextPOS.WinForms\app.ico
UninstallDisplayIcon={app}\bin\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; 1. Application binaries (self-contained publish output)
Source: "{#StageDir}\app\*"; DestDir: "{app}\bin"; Flags: ignoreversion recursesubdirs createallsubdirs

; 2. Pyidaungsu Myanmar fonts (system-wide registration, only if missing)
Source: "{#StageDir}\fonts\Pyidaungsu-2.5.3_Regular.ttf"; DestDir: "{autofonts}"; FontInstall: "Pyidaungsu"; Flags: ignoreversion uninsneveruninstall; Check: FontNotInstalled('Pyidaungsu')
Source: "{#StageDir}\fonts\Pyidaungsu-2.5.3_Bold.ttf"; DestDir: "{autofonts}"; FontInstall: "Pyidaungsu Bold"; Flags: ignoreversion uninsneveruninstall; Check: FontNotInstalled('Pyidaungsu Bold')
Source: "{#StageDir}\fonts\Pyidaungsu-2.5.3_Numbers.ttf"; DestDir: "{autofonts}"; FontInstall: "Pyidaungsu Numbers"; Flags: ignoreversion uninsneveruninstall; Check: FontNotInstalled('Pyidaungsu Numbers')

#ifdef BundleMySQL
; 3. Optional portable MySQL engine (1-click offline install)
Source: "{#StageDir}\database\*"; DestDir: "{app}\database"; Flags: ignoreversion recursesubdirs createallsubdirs
#endif

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Tasks: desktopicon

[Run]
#ifdef BundleMySQL
; Install & start the portable MySQL service in the background
Filename: "powershell.exe"; Parameters: "-ExecutionPolicy Bypass -File ""{app}\database\setup_service.ps1"""; Flags: runhidden
#endif
; Launch application after setup
Filename: "{app}\bin\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

#ifdef BundleMySQL
[UninstallRun]
; Stop and uninstall the MySQL service on uninstall
Filename: "sc.exe"; Parameters: "stop MMNextPOS"; Flags: runhidden; RunOnceId: "StopMySQLService"
Filename: "sc.exe"; Parameters: "delete MMNextPOS"; Flags: runhidden; RunOnceId: "RemoveMySQLService"
#endif

[Code]
// Returns True when the given font display name is not registered on the system.
function FontNotInstalled(FontName: String): Boolean;
begin
  Result := not FontExists(FontName);
end;
