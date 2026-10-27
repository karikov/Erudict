; Per-user installer. No admin rights. Files land in %LOCALAPPDATA%\ERuDict.

[Setup]
AppId={{A7C3E1B4-6D28-4F5A-9C10-E8B2D4F6A191}
AppName=ERuDict
AppVersion=1.2.0
AppPublisher=ERuDict
DefaultDirName={localappdata}\ERuDict
PrivilegesRequired=lowest
SetupIconFile=..\assets\app.ico
UninstallDisplayIcon={app}\ERuDict.exe
OutputDir=..\bin\Release
OutputBaseFilename=ERuDict-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no

[Files]
Source: "..\bin\Release\publish-aot\ERuDict.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\publish-aot\dict\*"; DestDir: "{app}\dict"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{userprograms}\ERuDict"; Filename: "{app}\ERuDict.exe"
Name: "{userstartup}\ERuDict"; Filename: "{app}\ERuDict.exe"; Tasks: startup

[Tasks]
Name: startup; Description: "Запускать ERuDict вместе с Windows"
