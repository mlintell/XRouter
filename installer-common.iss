  ; XRouter installer - template

; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "XRouter"
#define MyAppVersion "1.0"
#define MyAppPublisher "The XRouter Team"
#define MyAppURL "http://www.assembla.com/spaces/xrouter"

#define XRouterServiceName "xrouter"
#define XRouterManagerServiceName "xroutermanager"

[Setup]
; NOTE: AppId must be defined in the file which uses this template
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
OutputBaseFilename=xrouter-setup{#OutputBaseFilenamePostfix}
Compression=lzma
SolidCompression=yes
SetupIconFile=XRouter\Gui\Resources\xrouter.ico
WizardImageFile=installer\xrouter-logo-1.0-innosetup-164-314.bmp
WizardSmallImageFile=installer\xrouter-logo-1.0-innosetup-55x55.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: "sw"; Description: "Program files"; Types: full custom
Name: "sw\xRouter"; Description: "XRouter"; Types: full
Name: "sw\xRouter\Service"; Description: "XRouter Service"; Types: full compact
Name: "sw\xRouter\Manager"; Description: "XRouter Manager"; Types: full compact
Name: "sw\xRouter\Database"; Description: "XRouter Database image"; Types: full compact
Name: "sw\xRouter\Gui"; Description: "XRouter GUI"; Types: full
Name: "sw\daemonNt"; Description: "DaemonNT"; Types: full
Name: "sw\daemonNt\Console"; Description: "DaemonNT Console"; Types: full compact
Name: "sw\daemonNt\Gui"; Description: "DaemonNT GUI"; Types: full
Name: "sw\schemaTron"; Description: "SchemaTron"; Types: full
Name: "sw\schemaTron\console"; Description: "SchemaTron Console"; Types: full

Name: "docs"; Description: "Documentation"; Types: full
Name: "examples"; Description: "Examples"; Types: full
Name: "examples\restaurant"; Description: "Restaurant"; Types: full
Name: "examples\soasupport"; Description: "SOA Support"; Types: full

[Tasks]
Name: generateRestaurantData; Description: "Generate random input data for the Restaurant demo"; Components: examples\restaurant
Name: copyExampleConfigs; Description: "Copy config files from example ones"; Components: sw\xRouter

[Dirs]
Name: "{app}\Logs"; Components: sw\xRouter
Name: "{app}\AdapterPlugins"; Components: sw\xRouter
Name: "{app}\ActionPlugins"; Components: sw\xRouter
Name: "C:\XRouter\DemoRestaurant"; Components: examples\restaurant
Name: "C:\XRouter\DemoRestaurant\Pokladna"; Components: examples\restaurant
Name: "C:\XRouter\DemoRestaurant\Tisk"; Components: examples\restaurant
Name: "C:\XRouter\DemoRestaurant\InputData"; Components: examples\restaurant

[Files]
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "bin\{#BuildType}\DaemonNT.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\daemonNt\Console
Source: "bin\{#BuildType}\DaemonNT.GUI.ConfigEditor.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\daemonNt\Gui
Source: "bin\{#BuildType}\DaemonNT.xml"; DestDir: "{app}"; DestName: "DaemonNT.example.xml"; Flags: ignoreversion; Components: sw\xRouter\Service sw\xRouter\Manager
Source: "bin\{#BuildType}\ICSharpCode.AvalonEdit.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter\Gui
Source: "bin\{#BuildType}\ObjectConfigurator.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\RibbonControlsLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter\Gui
Source: "bin\{#BuildType}\SchemaTron.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\schemaTron sw\xRouter sw\daemonNt
Source: "bin\{#BuildType}\SchemaTron.Console.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\schemaTron\console
Source: "bin\{#BuildType}\SimpleDiagrammer.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\AdapterPlugins\XRouter.Adapters.dll"; DestDir: "{app}\AdapterPlugins\"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.Broker.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.Common.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.ComponentHosting.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.Data.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.Gateway.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.Gui.exe"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter\Gui
Source: "bin\{#BuildType}\XRouter.Gui.exe.config"; DestDir: "{app}"; DestName: "XRouter.Gui.exe.example.config"; Flags: ignoreversion; Components: sw\xRouter\Gui
Source: "bin\{#BuildType}\XRouter.Manager.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter
Source: "bin\{#BuildType}\XRouter.Processor.dll"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter

Source: "database\*"; DestDir: "{app}\database"; Flags: ignoreversion; Components: sw\xRouter\Database

Source: "README"; DestName: "README.txt"; DestDir: "{app}\Documentation"; Flags: isreadme; Components: sw
Source: "LICENSE"; DestName: "LICENSE.txt"; DestDir: "{app}\Documentation"; Components: sw
Source: "AUTHORS"; DestName: "AUTHORS.txt"; DestDir: "{app}\Documentation"; Components: sw
Source: "docs\XRouter\*"; Excludes: "dummy.txt,*.chw"; DestDir: "{app}\Documentation\XRouter"; Components: docs
Source: "docs\DaemonNT\*"; Excludes: "dummy.txt,*.chw"; DestDir: "{app}\Documentation\DaemonNT"; Components: docs
Source: "docs\SchemaTron\*"; Excludes: "dummy.txt,*.chw"; DestDir: "{app}\Documentation\SchemaTron"; Components: docs

Source: "copy-example-configs.bat"; DestDir: "{app}"; Flags: ignoreversion; Components: sw\xRouter

Source: "XRouterExamples\Restaurant\bin\{#BuildType}\*.exe"; Excludes: "*vshost*"; DestDir: "{app}\Examples\Restaurant"; Flags: ignoreversion; Components: examples\restaurant
;Source: "XRouterExamples\Restaurant\bin\{#BuildType}\*.dll"; DestDir: "{app}\Examples\Restaurant"; Flags: ignoreversion; Components: examples\restaurant
Source: "XRouterExamples\Restaurant\bin\{#BuildType}\*.exe.config"; Excludes: "*vshost*"; DestDir: "{app}\Examples\Restaurant"; Flags: ignoreversion; Components: examples\restaurant
Source: "XRouterExamples\Restaurant\Data\*"; DestDir: "{app}\Examples\Restaurant\Data"; Flags: ignoreversion recursesubdirs; Components: examples\restaurant
Source: "XRouterExamples\Restaurant\generate-demo-data.bat"; DestDir: "{app}\Examples\Restaurant"; Flags: ignoreversion; Components: examples\restaurant

Source: "XRouterExamples\SOASupport\bin\{#BuildType}\*.exe"; Excludes: "*vshost*"; DestDir: "{app}\Examples\SOASupport"; Flags: ignoreversion; Components: examples\soasupport

[Run]
Filename: "{app}\Examples\Restaurant\generate-demo-data.bat"; Description: "Generate random input data for the demo."; Flags: postinstall shellexec waituntilterminated; Tasks: generateRestaurantData
Filename: "{app}\copy-example-configs.bat"; Description: "Copy config files from example ones (DaemonNT.xml, GUI config)"; Flags: postinstall shellexec waituntilterminated runascurrentuser unchecked; Tasks: copyExampleConfigs

[Icons]
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

Name: "{group}\XRouter Configuration Manager GUI"; Filename: "{app}\XRouter.Gui.exe"; WorkingDir: "{app}"
Name: "{group}\DaemonNT Configuration Editor GUI"; Filename: "{app}\DaemonNT.GUI.ConfigEditor.exe"; WorkingDir: "{app}"

Name: "{group}\XRouter\Install XRouter as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w install {#XRouterServiceName}"
Name: "{group}\XRouter\Uninstall XRouter as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w uninstall {#XRouterServiceName}"
Name: "{group}\XRouter\Start XRouter as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w start {#XRouterServiceName}"
Name: "{group}\XRouter\Stop XRouter as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w stop {#XRouterServiceName}"

Name: "{group}\XRouter Manager\Install XRouter Manager as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w install {#XRouterManagerServiceName}"
Name: "{group}\XRouter Manager\Uninstall XRouter Manager as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w uninstall {#XRouterManagerServiceName}"
Name: "{group}\XRouter Manager\Start XRouter Manager as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w start {#XRouterManagerServiceName}"
Name: "{group}\XRouter Manager\Stop XRouter Manager as Windows Service"; Filename: "{app}\DaemonNT.exe"; WorkingDir: "{app}"; Parameters: "-w stop {#XRouterManagerServiceName}"

Name: "{app}\Application Links"; Filename: "{group}"
