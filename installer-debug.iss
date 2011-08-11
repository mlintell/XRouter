; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "XRouter"
#define MyAppVersion "1.0"
#define MyAppPublisher "The XRouter Team"
#define MyAppURL "http://www.assembla.com/spaces/xrouter"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8DBC166C-E839-446C-B607-8EE13B1BFF7A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
LicenseFile=LICENSE
InfoBeforeFile=README
OutputDir=installer
OutputBaseFilename=xrouter-setup-debug
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Debug\DaemonNT.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\ObjectConfigurator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\SchemaTron.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Broker.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Common.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.ComponentHosting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Gateway.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Processor.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Data.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\DaemonNT.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Adapters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Manager.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\SimpleDiagrammer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Debug\XRouter.Gui.exe"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

