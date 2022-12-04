[Setup]
AppName=EasyCaster Alarm 
AppVersion=2.0
WizardStyle=modern
DefaultDirName={autopf}\EasyCaster
DefaultGroupName=EasyCaster
UninstallDisplayIcon={app}\EasyCaster.Alarm.exe
Compression=lzma2
SolidCompression=yes
OutputDir=win-x86
OutputBaseFilename=EasyCasterAlarmSetup

[Languages]
Name: uk; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Messages]
uk.BeveledLabel=Ukrainian

[Files]
Source: "win-x86\EasyCaster.Alarm.exe"; DestDir: "{app}"

[Icons]
Name: "{group}\EasyCaster Alarm"; Filename: "{app}\EasyCaster.Alarm.exe"

