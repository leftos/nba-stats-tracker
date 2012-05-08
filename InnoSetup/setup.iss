;#define use_iis
;#define use_kb835732

;#define use_msi20
#define use_msi31
;#define use_msi45

;#define use_ie6

;#define use_dotnetfx11
;#define use_dotnetfx11lp

;#define use_dotnetfx20
;#define use_dotnetfx20lp

;#define use_dotnetfx35
;#define use_dotnetfx35lp

#define use_dotnetfx40client
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
Name: "{group}\Get Real NBA Stats"; Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NBA Stats Tracker.exe"; Parameters: "-realnbaonly"; Comment: "Starts the tool only to download the Real NBA Stats, saves them, and exits."

[Run]
Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; Flags: nowait postinstall runascurrentuser skipifsilent; Description: "{cm:LaunchProgram,NBA Stats Tracker}"

#include "scripts\products.iss"

#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
#include "scripts\products\dotnetfxversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif

#ifdef use_msi20
#include "scripts\products\msi20.iss"
#endif
#ifdef use_msi31
#include "scripts\products\msi31.iss"
#endif
#ifdef use_msi45
#include "scripts\products\msi45.iss"
#endif

#ifdef use_ie6
#include "scripts\products\ie6.iss"
#endif

#ifdef use_dotnetfx11
#include "scripts\products\dotnetfx11.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#ifdef use_dotnetfx11lp
#include "scripts\products\dotnetfx11lp.iss"
#endif
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#include "scripts\products\dotnetfx20sp1.iss"
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#include "scripts\products\dotnetfx20sp1lp.iss"
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40client
#include "scripts\products\dotnetfx40client.iss"
#endif

#ifdef use_dotnetfx40full
#include "scripts\products\dotnetfx40full.iss"
#endif

#ifdef use_wic
#include "scripts\products\wic.iss"
#endif

#ifdef use_vc2010
#include "scripts\products\vcredist2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif

#ifdef use_sqlcompact35sp2
#include "scripts\products\sqlcompact35sp2.iss"
#endif

#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif

[CustomMessages]
win_sp_title=Windows %1 Service Pack %2


[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Registry]
Root: "HKCU"; Subkey: "Software\NBA 2K12 Keep My Mod"; Flags: uninsdeletekey

[Dirs]
Name: "{app}\Images"

[Code]
function InitializeSetup(): boolean;
begin
	//init windows version
	initwinversion();

#ifdef use_iis
	if (not iis()) then exit;
#endif

#ifdef use_msi20
	msi20('2.0');
#endif
#ifdef use_msi31
	msi31('3.1');
#endif
#ifdef use_msi45
	msi45('4.5');
#endif
#ifdef use_ie6
	ie6('5.0.2919');
#endif

#ifdef use_dotnetfx11
	dotnetfx11();
#ifdef use_dotnetfx11lp
	dotnetfx11lp();
#endif
	dotnetfx11sp1();
#endif

	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
	//check if .netfx 2.0 can be installed on this OS
	if not minwinspversion(5, 0, 3) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['2000', '3'])]), mberror, mb_ok);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		msgbox(fmtmessage(custommessage('depinstall_missing'), [fmtmessage(custommessage('win_sp_title'), ['XP', '2'])]), mberror, mb_ok);
		exit;
	end;

	if minwinversion(5, 1) then begin
		dotnetfx20sp2();
#ifdef use_dotnetfx20lp
		dotnetfx20sp2lp();
#endif
	end else begin
		if minwinversion(5, 0) and minwinspversion(5, 0, 4) then begin
#ifdef use_kb835732
			kb835732();
#endif
			dotnetfx20sp1();
#ifdef use_dotnetfx20lp
			dotnetfx20sp1lp();
#endif
		end else begin
			dotnetfx20();
#ifdef use_dotnetfx20lp
			dotnetfx20lp();
#endif
		end;
	end;
#endif

#ifdef use_dotnetfx35
	//dotnetfx35();
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	//dotnetfx35lp();
	dotnetfx35sp1lp();
#endif
#endif

	// if no .netfx 4.0 is found, install the client (smallest)
#ifdef use_dotnetfx40client
	if (not netfxinstalled(NetFx40Client, '')) then
		dotnetfx40client();
#endif

#ifdef use_dotnetfx40full
  if (not netfxinstalled(NetFx40Full, '')) then
    dotnetfx40full();
#endif

#ifdef use_wic
	wic();
#endif

#ifdef use_vc2010
	vcredist2010();
#endif

#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif

#ifdef use_sqlcompact35sp2
	sqlcompact35sp2();
#endif

#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif

	Result := true;
end;
