#define MyAppSetupName 'NBA Stats Tracker'
#define MyAppVersion ''
#define MyAppVerInfo ''

[Setup]
AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion} {#MyAppVerInfo}
AppCopyright=Copyright © Lefteris Aslanoglou 2011-2012
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany=Lefteris Aslanoglou
AppPublisher=Lefteris Aslanoglou
;AppPublisherURL=http://...
AppSupportURL=http://forums.nba-live.com/viewtopic.php?f=143&t=83896
;AppUpdatesURL=http://...
OutputBaseFilename=[Leftos] {#MyAppSetupName} {#MyAppVersion} {#MyAppVerInfo}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={pf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\NBA Stats Tracker.exe
OutputDir=..
SourceDir=.
AllowNoIcons=yes
;SetupIconFile=MyProgramIcon
SolidCompression=yes
;MinVersion default value: "0,5.0 (Windows 2000+) if Unicode Inno Setup, else 4.0,4.0 (Windows 95+)"
;MinVersion=0,5.0
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64 ia64
ShowLanguageDialog=no
AlwaysShowGroupOnReadyPage=True
AlwaysShowDirOnReadyPage=True

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\LeftosCommonLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\LeftosCommonLibrary.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Readme.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SQLite.Interop.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SQLiteDatabase.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SQLiteDatabase.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\System.Data.SQLite.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\76ers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Bobcats.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Bucks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Bulls.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Cavaliers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Celtics.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Clippers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\down.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Grizzlies.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Hawks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Heat.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Hornets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Jazz.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Kings.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Knicks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Lakers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Magic.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Mavericks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Nets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Nuggets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Pacers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Pistons.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Raptors.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Rockets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Spurs.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Suns.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Thunder.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Timberwolves.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Trail Blazers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\up.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Warriors.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\Wizards.gif"; DestDir: "{app}\Images"; Flags: ignoreversion

[Icons]
Name: "{group}\NBA Stats Tracker"; Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NBA Stats Tracker.exe"
Name: "{group}\{cm:UninstallProgram,NBA Stats Tracker}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\NBA Stats Tracker"; Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NBA Stats Tracker.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; Flags: nowait postinstall runascurrentuser skipifsilent; Description: "{cm:LaunchProgram,NBA Stats Tracker}"

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2


[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Registry]
Root: "HKCU"; Subkey: "Software\NBA Stats Tracker"; Flags: uninsdeletekey

[Dirs]
Name: "{app}\Images"

