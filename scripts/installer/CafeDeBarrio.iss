#define AppVersion GetEnv("AppVersion")
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

[Setup]
AppName=Café de Barrio POS
AppVersion={#AppVersion}
AppPublisher=Pablo Joel Castillo Flores
DefaultDirName={autopf}\CafeDeBarrio
DefaultGroupName=Café de Barrio
OutputDir=..\..\release
OutputBaseFilename=CafeDeBarrio-POS-v{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
UninstallDisplayIcon={app}\CafeBarrio.POS.exe

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "..\..\release\pos\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "..\..\config\App.config.example"; DestDir: "{app}"; DestName: "App.config"; Flags: onlyifdoesntexist
Source: "..\..\docs\CONFIGURACION.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Café de Barrio POS";    Filename: "{app}\CafeBarrio.POS.exe"
Name: "{group}\Desinstalar";           Filename: "{uninstallexe}"
Name: "{commondesktop}\Café de Barrio POS"; Filename: "{app}\CafeBarrio.POS.exe"

[Run]
Filename: "notepad.exe"; Parameters: "{app}\CONFIGURACION.md"; \
  Description: "Ver guía de configuración"; Flags: postinstall skipifsilent
