NBA Stats Tracker
	by Lefteris "Leftos" Aslanoglou


Prologue
	What started as a workaround to the Team Stats bug in NBA 2K12, grew up to become a full stats tracker & analyzer for any basketball league. 'NBA 2K12 Correct Team Stats' is now 'NBA Stats Tracker', it's my thesis for my Computer Engineering degree, it's my work and passion.
	
	Its goal is to offer a cheap and easy solution to real life coaches and fans who want to keep track of their favorite teams and leagues and do all kinds of analysis on the data. When done, it should also offer scouting reports in natural language for nearly every window and table of data. It will also feature a mobile app with syncing to the cloud, to allow you to carry the stats and scouting reports everywhere with you.

Quick notes
	NBA Stats Tracker is a basketball analysis tool. It allows you to add teams and players to a database and gather stats either by entering the box scores of each game, or by manually editing the stats yourself.
	
	What the tool offers is deep analysis and comparison for every team and player, as well as league-wide overviews. It also features scouting reports in natural language which can help you prepare against an opponent you're unfamiliar with, get to know their strengths and weaknesses, as well as what trends come up out of their recent performances.
	
	All analysis features can be applied to a season's stats, or any specific timeframe starting from days up to years.
	
	Whenever you find yourself in a window that has a table with teams, players or box-scores in it, double-click on any of them to get to the overview screen of it.
	
	Each database can keep data from multiple seasons. You can compare yearly averages, as well as compare results against a particular opponent over the years, for example.
	
Installation Notes
	NBA Stats Tracker requires the .NET Framework 4 Client Profile. The installer should download and install it automatically.

Features in a glance
	Team Overview
		Overview
			The Overview Tab offers you just that, a quick overview over each team's stats, averages, and rankings during the season and playoffs. You can edit and save each team's stats from this tab.
		Split Stats
			Analysis on how the team is doing when playing at home and away, through wins and losses, during the seasons and the playoffs, as well as a monthly breakdown.
		Player Stats
			A list of each team's players with their averages and total stats. You can quickly edit the player stats from this tab.
		Metric Stats
			Advanced player statistics that take into account multiple factors of the player, team and league performance.
		Best Performers
			The best players of each team with a summary of their most significant stats.
		Box Scores
			A list of each team's games, with easy access to the Box Score window, which allows you to view the full team and player stats, as well as edit them.
		Head-To-Head
			Compare each team's averages to another team's; either against the league or each other; view the box scores of the games between the two teams.
		Head-To-Head Best Performers
			Compare the best guard, forward and center of two teams; get a quick overview of the important matchups.
		Yearly Report
			See the averages of the team each season, as well as its total averages for all seasons.
	Player Overview
		Overview
			View a player's stats, averages and rank of each average against the league, teammates and players of the same position. Change their name, position, team, injury status, as well as whether their in the season's all-stars, or the championship team.
		Split Stats
			Similar to the team split stats, with the addition of stats split between the teams the player has played for in the timeframe; if the player was traded, you'll see stats before and after each trade.
		Box Scores
			A list of games the player has been recorded in, with their stats and shooting percentages.
		Head-To-Head
			Compare any two players stats and averages, either in games against the league, or each other's teams.
		Yearly Report
			See a player's progress over each season, as well as their career averages.
	Advanced Player Search
		Search among all the players in any season by setting whatever criteria you want, such as position, total stats, average stats, team, PER, etc.
		When doing an advanced search, the players are sorted by each filter you use, by taking Metrics filters into account first, then Averages, then Totals.
	League Overview
		Team Stats
			All the teams, all the averages, all of the regular season.
		Playoff Stats
			Similar to the team stats, but with playoff averages.
		League Leaders
			The league's leading players in each average, taking into account the NBA's rule for league leaders eligibility.
		Player Stats
			All the players, all stats and averages, no restrictions applied.
		Metric Stats
			Advanced player statistics that take into account multiple factors of the player, team and league performance.
		Best Performers
			The best players of the whole league with a summary of their most significant stats.
		Box Scores
			Every box score saved in the specific timeframe.
	Box Scores
		Box Scores include Team Stats, Player Stats (simple & metric) as well as a Best Performers tab, which shows you the best players of the game from both teams.
	Live Box Scores
		Besides being able to input the box scores after the fact, you can easily keep track of a game that you're watching using Live Box Score. Every important stat has up and down arrows which allow you to easily add a 3PT to a specific player, an offensive rebound, or whatever else is happening.
			
NBA 2K12 Features
	This tool was originally made to offer a temporary workaround to the Association/ Season/My Player/Create A Legend team stats bug. Any game you entered and played would have the team stats of the teams that played in it all wrong afterwards. For example 85BPG, 92APG, 70RPG, etc. This really ruined the immersion the Association offers for those that like to play or even watch the games, instead of just simulating them from the Calendar. The Team Stats screen was wrong, the in-game overlays about the team were wrong. A mess. Even after the first console patches, 2K still hasn't fixed this issue. 
	
	Don't worry though.
	
	If you have the patience to just enter the Box Score (just the team stats, the players' ones aren't required) in the tool after each game you play, it'll make sure that your Team Stats (and the other team's that you played against) will be correctly updated. 
	
	Just follow these steps:
	REditor
		1. From in-game, select Play Game, and let it save your career.
		2. Alt-Tab out of the game and start REditor.
		3. Open your save in REditor, export everything to TSV (File > Export to TSV).
		4. Start NBA Stats Tracker.
		5. Open your database, or create a new one if you haven't previously done so.
		6. Click on Import from 2K12 Save, select the folder you saved the TSVs into in Step 3.
		7. Save the database.
		8. Alt-Tab into the game and play it or watch it, then after it's done let it save your career again.
		9. Go back to the tool, open the database you saved before the game.
		10. Click on Update with Box Score, enter the Box Score and click OK.
		11. Click on Export to 2K12 Save, select the folder you saved the TSVs into in Step 3.
		12. Go to REditor, open your career again, import everything from TSV (File > Import from TSV).
		13. Save your career in REditor.
		14. You're done!

		Q: I don't even know what REditor is! Care to throw me a bone?
		A: In short, REditor is the ultimate NBA 2K12 tool. It allows you to edit everything in any roster or career file. You can find out more by visiting
		http://www.red-mods.com/

	Old Season 1 Workaround
		1. From in-game, select Play Game, and let it save your career.
		2. Alt-Tab out of the game and into the tool.
		3. Open your database, or create a new one if you haven't previously done so.
		4. Import the NBA 2K12 stats from your save right before the game.
		5. Save the database.
		6. Alt-Tab into the game and play it or watch it, then after it's done let it save your career again.
		7. Go back to the tool, open the database you saved before the game.
		8. Click on Update with Box Score, enter the Box Score and click OK.
		9. Click on Export to 2K12 Save, choose the Career file in the dialog.
		10. You're done!

		
Additional Features
	Stat Averages, Ranking and Scout Reports
		See how well your team is doing, and scout your next opponent for their strengths and weaknesses in a quick glance! 

		You can view the averages of each team (such as PPG, RPG, FG%), etc, as well as the ranking of each stat in the league, by clicking on "Show Averages",	after loading your save and selecting a team, of course.

		The "eff" averages are efficiency averages, and they take into account both the success percentage, as well as the absolute amount. What that means is though both a 3-0 and a 6-0 team are on a 1.0 winning %, the second team can obviously keep the 1.0 longer, and has more chances of winning the division/conference/league. Another example are 3 pointers. Two teams that shoot 40% from beyond the arc are dangerous. But if you attempt 5 3-pointers in the whole game, 40% means you made two. That's not really dangerous now, is it? However, a team that has a 40% success while attempting 15 three pointers a game, makes 6 every game! Now that's more like it. Thus, a team that has the same percentage with another, but has that on more wins/shots, will have a better "eff" average. 

		The "Win eff" ranking is actually a Power Ranking of sorts, as it shows you the relative winning strength of a team in the league, taking account not only the winning percentage, but also how many wins they've got so far and how close they are to the end of the season. 

		You can also view a Scouting Report of any team, in natural language, commenting on their pros and cons based on their stat rankings in the league, preparing you for a game against them. 

	Export Tables
		All the tables in the tool (Team/Player/League Overview & Box Scores) support copying to the clipboard; when you do, the table's column names (headers) are included as well.

		Box Scores in particular, have a special Copy button in their window, which copies both Player (if any) & Team Stats to the clipboard.

		The resulting text is in Tab-Separated Values format, supported by Excel and many other spreadsheet editors for pasting into.

	Real NBA Stats
		This feature allows you to automatically grab the real league's team stats, player stats and box scores, and import them into the tool. All that with just a single-click! From there on, you can use all the tool's features, including averages, rankings, scouting reports, comparisons, CSV exports, and anything else that finds its way into the tool's features, onto the real NBA team stats. 	
		

Disclaimer
	The tool is still in beta. I've tested it in my environment, on my Association files and it seems to work perfectly. If you encounter any problems, you'll find a backup of your Association in the Saves folder. Keep one thing in mind however... 
	
					ALWAYS KEEP BACKUPS OF YOUR SAVES!
					
	I won't take any responsibility if this tool messes up your Saves. You've been warned.
	
	
Special thanks
	- JaoSming, for his roster editing tutorial, especially the part regarding the CRC checks
	- Onisak, for his help with debugging
	- Vl@d Zola Jr, for his invaluable help in making NST compatible with all NBA 2K12 saves
	- albidnis, for his idea to export to CSV
	- jrlocke, for being the first donator, and a generous one
	- zizoux, for his idea to inject real stats, which ended up being the Custom Leagues & Real NBA Stats features
	- AreaOfEffect, for his help with debugging
	- Tinifu Tuitama, for his donation
	- koberulz, for his extensive suggestions and help with debugging
	- nbagnome, for his live box score idea and his donation
	- Everyone at the NLSC community, for their continued support


Development Credits
	All development for NBA Stats Tracker was done by myself, Lefteris Aslanoglou, as the implementation of my thesis "Application Development for Basketball Statistical Analysis in Natural Language" under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou.

	This software uses, with permission (whether implicit or explicit), the following binary implementations, class libraries and code examples:
		Uses the SQLite database engine (http://www.sqlite.org/)
		Uses the System.Data.SQLite .NET wrapper for SQLite (http://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki)
		Uses the HTMLAgilityPack HTML parser (http://htmlagilitypack.codeplex.com/)
		Uses an SQLite Class Library 
			originally based on a tutorial by Mike Duncan,
				(http://www.mikeduncan.com/sqlite-on-dotnet-in-3-mins/)
			implemented by brennydoogles,		
				(http://www.dreamincode.net/forums/topic/157830-using-sqlite-with-c%23/)
			and improved upon by myself.
		Uses a CRC32 Class Library by Damien Guard (http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net)
		Uses a FolderBrowseDialog WPF usage example (http://stackoverflow.com/questions/315164/how-to-use-a-folderbrowserdialog-from-a-wpf-application)
		Uses the Extended WPF Toolkit (http://wpftoolkit.codeplex.com/)
		Uses the LumenWorks Framework (http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader)
		Uses a personal edit of the SoftwareArchitects ScrollSynchronizer (http://www.codeproject.com/Articles/39244/Scroll-Synchronization)
		Uses the Ciloci Fast Lightweight Expression Evaluator (http://flee.codeplex.com)
	
	
Version History
	v0.11.5.2
		- Fix: Fixed bug that would crash NST when using Live Box Score
		- Fix: Fixed bug that would crash NST when loading a database with multiple seasons
		- Fix: Fixed grouping of Player Split Stats

	v0.11.5
		- Improvement: Advanced Player Search has been overhauled, to allow easy addition and removal of filters, as well as automatically sort the results by each filter
		- Addition: Advanced Player Search filters can now be saved and loaded at a later time
		- Change: Changed rate metrics to be calculated per 36 minutes of play, as in Basketball-Reference.com
		- Addition: Added Offensive Rebound Rate (OREBR) as a metric stat

	v0.11.4
		- Addition: NBA Box Scores can now be downloaded from Basketball-Reference.com

	v0.11.3
		- Addition: 6 Best Performers of each team are now shown in Team Overview, with a summary of their most significant stats
		- Addition: 6 Best League-Wide Performers  are now shown in League Overview, with a summary of their most significant stats
		- Addition: Head-To-Head Best Performers tab added to Team Overview; presents a comparison of the best Guard, Forward and Center of each team along with their most significant stats
		- Improvement: Team Overview should now open faster
		- Change: PTS and PPG will always be shown as the first significant stat of any player in the Best Performers tabs
		- Fix: Fixed stat presentation in Overview tab of Team Overview regarding playoffs and limited timeframes
		- Improvement: Minor other interface improvements

	v0.11.2.1
		- Fix: Fixed bug that made the PER column blank wherever PER was supposed to be shown
		- Fix: Fixed bug that would save the team's stats as the opponents' stats as well
		- Fix: Fixed bug that would erase all other teams' but the current's players when saving changes in the Team Overview window

	v0.11.2
		- Addition: League Overview now offers league-wide averages in all tabs (except League Leaders & Box Scores)
		- Addition: Team Metric Stats added to League Overview

	v0.11.1
		- Improvement: User is asked which teams are active in their Career if they can't be detected from the REditor import
		- Improvement: Improvements in REditor import handling

	v0.11.0.1
		- Improvement: Tool will now inform you of a successful REditor export
		- Fix: Tool would ignore imported players with PlType 6 (Existing CAT Player)

	v0.11
		- Improvement: Fully compatible with any NBA 2K12 save, any season, if used along with REditor (requires proper REditor license)
		- Addition: Added Live Box Score feature, which allows you to easily keep track of a live game, and then save it as a box score in your database
		- Improvement: Seasons can now have names, so that for example, Season "1" can now be Season "1967-1968"
		- Fix: Advanced Player Search wouldn't take GP or GS into account
		- Improvement: When you choose to sort a stat-table by a specific stat, the tool now will sort in descending order by default

	v0.10.7.9
		- Fix: Adjusted Download NBA Stats to changes in Basketball-Reference.com's format

	v0.10.7.8
		- Improvement: Parallelized some code to improve performance
		- Fix: Fixed various bugs in League Overview regarding inactive players and some tabs not updating correctly
		- Fix: Adjusted Download NBA Stats to changes in Basketball-Reference.com's format

	v0.10.7.7
		- Addition: Check For Updates on start can now be disabled (Options Menu)
		- Addition: You can manually check for updates after the tool starts (About window)
		- Improvement: Players in the list-boxes in the Player Overview & Box Score windows are now sorted by last name for easier discovery

	v0.10.7.6
		- Fix: Tool would crash when reactivating an inactive player
		- Fix: FTM/FGA percentage wouldn't show correctly in Best Performers tab in the Box Score window
		- Addition: Now you can reset all player/team stats for a season (Miscellaneous menu)

	v0.10.7.5
		- Fix: If after saving an edited box score you tried to update the database with a new one, the new one would overwrite the just edited box score instead of creating a new entry for itself
		- Improvement: Improved saving speed related to player box scores
		- Fix: Some box scores would not get imported correctly when using the Import Box Scores feature

	v0.10.7.4
		- Fix: Metric Stats tables in Team & League Overview wouldn't copy to clipboard correctly
		- Fix: Fixed a crash when the tool tried to calculate the metrics of a player that is currently inactive

	v0.10.7.3
		- Fix: Fixed some wrong averages and rankings in the Team Overview screen
		- Fix: Fixed wrong team name for the Player of the Game if they were from the home team
		- Fix: Fixed recent bug that broke editing & saving a Box Score you had previously inserted

	v0.10.7.2
		- Fix: Advanced Player Search options Active, Injured, All Star and Champion now have three settings: true, false and none; setting any of them to none will search for both players that are for example injured and not injured
		- Fix: Fixed bugs regarding inactive player handling

	v0.10.7.1
		- Fix: Fixed recent bug that made all displayed team names blank when importing stats from a 2K12 save

	v0.10.7
		- Addition: Added Advanced Player Search

	v0.10.6.5
		- Fix: Fixed a recent bug that messed up 3P stats

	v0.10.6.4
		- Addition: Current Season can now be changed from the Main Window as well
		- Addition: Box Scores can now be deleted (Miscellaneous > Delete Box Scores...)
		- Fix: Fixed bug that in some cases didn't allow users to view box scores of teams disabled in some seasons but enabled in others
		- Fix: The tool would crash whenever you tried to view a box score that included a team that's disabled for that season; now it shows a message informing you of the fact
		- Improvement: When disabling teams, you're warned if you're about to disable a team that has box scores for that season
		- Fix: Various minor fixes around the program

	v0.10.6.3
		- Improvement: Database is saved immediately after adding teams
		- Improvement: Player Stats, Metric Stats and League Leaders in League Overview now feature row headers that have the ranking of each row
		- Fix: Many fixes around the League Overview and Box Score windows regarding teams being renamed and/or disabled
		- Removal: Since switching between different box scores inside the Box Score window caused too many problems, the functionality is removed; it has been more than replaced by the Box Scores tabs in Team, Player & League Overview; double-click on any box score in each list to view it in full detail

	v0.10.6.2
		- Improvement: Players are allowed to have no position set; that allows you to keep stats for leagues and years for which no such info is available

	v0.10.6.1
		- Fix: File > Save Database As wouldn't work when the destination file was the same as the currently open
		- Improvement: Big performance increase in saving operations; save, save as and start new season should now be much faster
		- Fix: League Overview displayed Team Stats in the Playoff Stats tab in some cases
		- Fix: When adding players to separate seasons, they could end up having the same IDs and sharing stats between seasons; now each player added, no matter the season, gets their own unique ID

	v0.10.6 - May 9
		- Addition: Teams can be enabled and disabled per season (Main Window > Miscellaneous > Enable/Disable Teams For This Season); this can be used to relegate a team for a season and then have them back in another season with their other years' stats intact
		- Addition: Tool is now able to handle teams that are in one season but not another; either weren't created in it or are disabled
		- Addition: Teams can be renamed (Team Overview > Change Team Name)

	v0.10.5.1 - May 8
		- Fix: Fixed a recent bug that broke the Add Players function
		- Improvement: Database is saved immediately after adding players

	v0.10.5 - May 8
		- Improvement: Download NBA Stats now downloads both team and player stats (season & playoffs)
		- Fix: Fixed a recent bug which caused Team Overview to show incorrect playoff stats for all teams
		- Improvement: A lot of performance improvements to the League Overview window

	v0.10.4.2 - May 7
		- Improvement: Best Peformers calculations now take player position into account; should be able to pick the most significant stats better
		- Fix: Players with apostrophes in their name (such as O'Neal) caused SQlite errors and weren't being added to the database
		- Fix: Tool wouldn't make any checks in the Add Players window, meaning you could add players with no names, no position, no team.
		- Fix: When in a table that automatically inserts a new row after the current one, pressing Tab while in the last column of the last row would make the table lose focus; it now correctly moves to the first column of the newly inserted row
		- Removal: Removed Defensive Rebounds from the Best Performers screen as a possibly significant stat; Offensive and Total Rebounds remain
		- Change: Best Performers tab in Box Scores window now shows Player of the Game plus 3 best players from each team
		- Fix: Fixed bug (probably since v0.10.3) that broke the Export to 2K12 Save feature

	v0.10.4.1 - May 7
		- Addition: Best Performers tab Addition to Box Score window, showing you the key stats from the key players of the game
		- Addition: FTR (Free Throw Rate) metric Addition to Metric Stats
		- Addition: New edition of NBA Stats Tracker gets its own release channel, with update notifications

	v0.10.3 - May 6
		- Fix: Tool wouldn't save the 2K12 Compatibility Mode correctly, leading to mixed Team Stats in many occasions
		- Addition: Metric Stats have been implemented; there's a table for them in Team Overview, League Overview & Box Scores
		- Addition: Each team now has "Opponents" stats as well, which can either be directly edited or accumulated via box score updating
		- Addition: You can now import Box Scores from other database files
		- Improvement: Various performance improvements
		- Fix: Nearly all BPG and TPG columns were reversed; that is, BPG were in the TPG column, and TPG in the BPG one
		- Fix: Various other minor bug fixes

	v0.10.2 - May 5
		- Addition: Minutes Played have been Addition as a stat kept for Teams

	v0.10.1.2 - May 4
		- Addition: All the tables in the tool support copying to the clipboard; when you do, the table's column names (headers) are included as well
		- Improvement: The Box Score window's Copy feature now copies both teams' player and team stats into the Clipboard in a TSV table with headers
		- Fix: Fixed various minor issues

	v0.10 - May 3
		- Change: The program now uses SQLite 3 databases instead of the custom binary format
		- Change: Massive UI overhaul, more details below
		- Addition: Team Overview screen; all statistics can be limited to a specific timeframe if the required box scores are available
			* Overview: View and edit your team's stats; limit them to a timeframe and see your opponents' stats against you during that time
			* Split Stats: See how a team's performing when winning or losing, at home or away, during the regular season or the playoffs, as well as its monthly progress
			* Player Stats: See an overview of the stats of all the team's players
			* Box Scores: View any team's box scores in an easy to search table
			* Head-To-Head: Compare your team to any other team; compare stats against the league, as well as each other
			* Yearly Report: Compare the team's averages over multiple seasons
		- Addition: Player Overview screen
			* Overview: View and edit a player's stats and other info
			* Split Stats: Player performance on wins and losses, home and away, season and playoffs, monthly progress, as well as on each team they played (before/after trade)
			* Box Scores: View all box scores that this player is in
			* Head-To-Head: Compare any two players in their games against the league, as well as against each other
			* Yearly Report: Compare any player's averages over multiple seasons
		- Addition: League Overview screen
			* Team Stats: Similar to the 2K Team Stats table, a full overview of the league team averages in a sortable table
			* Playoff Stats: As above, but limited to teams in the playoffs
			* League Leaders: See the leading players in the league and their averages (uses NBA rules on whether a player is eligible for inclusion)
			* Player Stats: All players, all stats, no rules
			* Box Scores: An overview of all box scores saved around the league
		- Addition: Box Scores can now be edited after the initial creation
		- Addition: Box Scores now require a Season number as well as the date the game took place; you can also differentiate between regular season and playoff games for the split stats
		- Addition: Double-clicking on a team, player or box-score in any table in the tool takes you to the respective screen for further analysis and edits
		- Addition: Tool now keeps track of stats over multiple seasons; Team Stats, Playoff Stats, Player Stats and Box Scores are all preserved between seasons, and you can switch to any season's stats easily

	v0.8 - Mar 21
		- Added keeping history of Box Score updates
		- Changed Team Stats file structure to allow for further expansion and changes; this means that Saved Team Stats from versions previous to this one aren't compatible
		- Other minor fixes and improvements
		
	v0.7.3 - Jan 23
		- Added preview of Trends feature
		- Uncluttered interface; many buttons moved to menus, keyboard shortcuts added
		
	v0.7 - Jan 21
		- Real NBA Stats feature added; the tool can now grab all the real league's team stats and import them so you can use all its features on them
		- Added feature to compare your team to its real life counterpart in a head-to-head comparison
		- Added feature to compare the same team from two different Team Stats files
		   
	v0.6 - Jan 15
		- Tool can now create and edit custom leagues, offering features such as updating with Box Scores, team averages and rankings and CSV exporting to other leagues too, besides NBA 2K12 Saves (more info above); when the tool's in custom league mode, all stats are directly editable
		- Tool can now inject the directly editable custom league stats into an NBA 2K12 save; so there is a way to directly edit the stats of your save if you so choose, it's explained above
		- Tool can now export manually inputted Box Scores to CSV
		- Versus window implemented, comparing any two teams' averages
		   
	v0.5 - Jan 9
		- Tool can now export any team's stats, averages and rankings to CSV
		- Tool can now export league-wide team stats and averages to CSV
		- Tool now checks for saved team stats and Association file compatibility when the user uses "Load & Update Team Stats"; if updating the team stats with the info the user provided in the Box Score would cause a different Wins/Losses stat for any team than the game has saved, the tool shows a warning

	v0.4 - Jan 9
		- Scouting Reports implemented, translating the stat rankings of each team into natural comments about their play, combining different stats to try and make insightful comments; more depth will be added to scouting in future development
		   
	v0.3 - Jan 9
		- "Show Averages" feature added, which shows a team's averages such as PPG, FG%, RPG, etc., as well as show the team's ranking in the league for each stat
		   
	v0.2 - Jan 8
		- Added functionality to keep the tool working during the playoffs
		- Now compatible with Season & Playoffs careers

	v0.1 - Jan 8
		- Initial Release
		
		
Discussion/Support Thread: http://forums.nba-live.com/viewtopic.php?f=143&t=84106
