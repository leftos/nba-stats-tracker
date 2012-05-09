;#define use_iis
;#define use_kb835732

;#define use_msi20
;#define use_msi31
;#define use_msi45

;#define use_ie6

;#define use_dotnetfx11
;#define use_dotnetfx11lp

;#define use_dotnetfx20
;#define use_dotnetfx20lp

;#define use_dotnetfx35
;#define use_dotnetfx35lp

;#define use_dotnetfx40client
;#define use_wic

;#define use_vc2010

;#define use_mdac28
;#define use_jet4sp8

;#define use_sqlcompact35sp2

;#define use_sql2005express
;#define use_sql2008express

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
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Readme.txt"; DestDir: "{app}"; Flags: ignoreversion isreadme
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\76ers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Bobcats.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Bucks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Bulls.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Cavaliers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Celtics.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Clippers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\down.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Grizzlies.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Hawks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Heat.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Hornets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Jazz.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Kings.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Knicks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Lakers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Magic.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Mavericks.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Nets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Nuggets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Pacers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Pistons.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Raptors.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Rockets.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Spurs.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Suns.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Thunder.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Timberwolves.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Trail Blazers.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\up.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Warriors.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\Images\Wizards.gif"; DestDir: "{app}\Images"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SQLite.Interop.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\System.Data.SQLite.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.pdb"; DestDir: "{app}"; Flags: ignoreversion

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

