// Define stuff we need to download/update/install

//#define use_iis
//#define use_kb835732
//#define use_kb886903
//#define use_kb928366

//#define use_msi20
//#define use_msi31
#define use_msi45
//#define use_ie6

//#define use_dotnetfx11
// German languagepack?
//#define use_dotnetfx11lp

//#define use_dotnetfx20
// German languagepack?
//#define use_dotnetfx20lp

//#define use_dotnetfx35
// German languagepack?
//#define use_dotnetfx35lp

// dotnet_Passive enabled shows the .NET/VC2010 installation progress, as it can take quite some time
#define dotnet_Passive
#define use_dotnetfx40
//#define use_vc2010

//#define use_mdac28
//#define use_jet4sp8
// SQL 3.5 Compact Edition
//#define use_scceruntime
// SQL Express
//#define use_sql2005express
//#define use_sql2008express

// Enable the required define(s) below if a local event function (prepended with Local) is used
//#define haveLocalPrepareToInstall
//#define haveLocalNeedRestart
//#define haveLocalNextButtonClick

// End

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
;AppSupportURL=http://...
;AppUpdatesURL=http://...
OutputBaseFilename=[Leftos] {#MyAppSetupName} {#MyAppVersion} {#MyAppVerInfo}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={pf}\{#MyAppSetupName}
UninstallDisplayIcon={app}\NBA Stats Tracker.exe
UninstallDisplayName={#MyAppSetupName}
Uninstallable=true
DirExistsWarning=no
CreateAppDir=true
OutputDir=..
SourceDir=.
AllowNoIcons=yes
UsePreviousGroup=yes
UsePreviousAppDir=yes
LanguageDetectionMethod=uilanguage
InternalCompressLevel=ultra
SolidCompression=yes
Compression=lzma/ultra
MinVersion=4.1,5.01
PrivilegesRequired=admin
ArchitecturesAllowed=x86 x64 ia64
ArchitecturesInstallIn64BitMode=x64 ia64
AppSupportURL=http://forums.nba-live.com/viewtopic.php?f=143&t=84110
AppUpdatesURL=http://forums.nba-live.com/viewtopic.php?f=143&t=84110

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Ciloci.Flee.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\HtmlAgilityPack.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\LeftosCommonLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\LeftosCommonLibrary.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\LumenWorks.Framework.IO.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\NBA Stats Tracker.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Readme.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SoftwareArchitects.Windows.Controls.ScrollSynchronizer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SQLiteDatabase.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\SQLiteDatabase.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\System.Data.SQLite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\WPFToolkit.Extended.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Leftos\Documents\Visual Studio 2010\Projects\NBA Stats Tracker\NBA Stats Tracker\bin\Release\Images\*"; DestDir: "{app}\Images"; Flags: createallsubdirs recursesubdirs

[Icons]
Name: "{group}\NBA Stats Tracker"; Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NBA Stats Tracker.exe"
Name: "{group}\{cm:UninstallProgram,NBA Stats Tracker}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\NBA Stats Tracker"; Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NBA Stats Tracker.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\NBA Stats Tracker"; Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\NBA Stats Tracker.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\NBA Stats Tracker.exe"; WorkingDir: "{app}"; Flags: nowait postinstall runascurrentuser skipifsilent; Description: "{cm:LaunchProgram,NBA Stats Tracker}"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Registry]
Root: "HKCU"; Subkey: "Software\Lefteris Aslanoglou\NBA Stats Tracker"; Flags: uninsdeletekey

[Dirs]
Name: "{app}\Images"

#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"

#ifdef use_iis
#include "scripts\products\iis.iss"
#endif

#ifdef use_kb835732
#include "scripts\products\kb835732.iss"
#endif
#ifdef use_kb886903
#include "scripts\products\kb886903.iss"
#endif
#ifdef use_kb928366
#include "scripts\products\kb928366.iss"
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
#include "scripts\products\dotnetfx11lp.iss"
#include "scripts\products\dotnetfx11sp1.iss"
#endif

#ifdef use_dotnetfx20
#include "scripts\products\dotnetfx20.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20lp.iss"
#endif
#include "scripts\products\dotnetfx20sp1.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20sp1lp.iss"
#endif
#include "scripts\products\dotnetfx20sp2.iss"
#ifdef use_dotnetfx20lp
#include "scripts\products\dotnetfx20sp2lp.iss"
#endif
#endif

#ifdef use_dotnetfx35
#include "scripts\products\dotnetfx35.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35lp.iss"
#endif
#include "scripts\products\dotnetfx35sp1.iss"
#ifdef use_dotnetfx35lp
#include "scripts\products\dotnetfx35sp1lp.iss"
#endif
#endif

#ifdef use_dotnetfx40
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\dotnetfx40full.iss"
#endif
#ifdef use_vc2010
#include "scripts\products\vc2010.iss"
#endif

#ifdef use_mdac28
#include "scripts\products\mdac28.iss"
#endif
#ifdef use_jet4sp8
#include "scripts\products\jet4sp8.iss"
#endif
// SQL 3.5 Compact Edition
#ifdef use_scceruntime
#include "scripts\products\scceruntime.iss"
#endif
// SQL Express
#ifdef use_sql2005express
#include "scripts\products\sql2005express.iss"
#endif
#ifdef use_sql2008express
#include "scripts\products\sql2008express.iss"
#endif

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3

#expr SaveToFile(AddBackslash(SourcePath) + "Preprocessed"+MyAppSetupname+".iss")

[Code]
function InitializeSetup(): Boolean;
begin
	//init windows version
	initwinversion();
	
	//check if dotnetfx20 can be installed on this OS
	//if not minwinspversion(5, 0, 3) then begin
	//	MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
	//	exit;
	//end;
	if not minwinspversion(5, 1, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
		exit;
	end;
	
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
#ifdef use_kb886903
	kb886903(); //better use windows update
#endif
#ifdef use_kb928366
	kb928366(); //better use windows update
#endif
	
	//install .netfx 2.0 sp2 if possible; if not sp1 if possible; if not .netfx 2.0
#ifdef use_dotnetfx20
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
	dotnetfx35();
#ifdef use_dotnetfx35lp
	dotnetfx35lp();
#endif
	dotnetfx35sp1();
#ifdef use_dotnetfx35lp
	dotnetfx35sp1lp();
#endif
#endif
	
	// If no .NET 4.0 framework found, install the smallest
#ifdef use_dotnetfx40
	if not dotnetfx40client(true) then
	    if not dotnetfx40full(true) then
	        dotnetfx40client(false);
	// Alternatively:
	// dotnetfx40full();
#endif

	// Visual C++ 2010 Redistributable
#ifdef use_vc2010
	vc2010();
#endif
	
#ifdef use_mdac28
	mdac28('2.7');
#endif
#ifdef use_jet4sp8
	jet4sp8('4.0.8015');
#endif
	// SQL 3.5 CE
#ifdef use_ssceruntime
	ssceruntime();
#endif
	// SQL Express
#ifdef use_sql2005express
	sql2005express();
#endif
#ifdef use_sql2008express
	sql2008express();
#endif
	
	Result := true;
end;
