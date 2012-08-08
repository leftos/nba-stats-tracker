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
		Recommended Starters
			Find out which starting 5, with which players in which position, would maximize your team's performance.
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
		Best Performances
            A quick look at the player's best performances during the season or a timeframe of your choice.
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
		Team Metric Stats
            Advanced team statistics that take into account multiple factors of the team's performance, as well as of its opponents and the league.
		League Leaders
			The league's leading players in each average, taking into account the NBA's rule for league leaders eligibility.
		Player Stats
			All the players, all stats and averages, no restrictions applied.
		Metric Stats
			Advanced player statistics that take into account multiple factors of the player, team and league performance.
		Best Performers
			The best players of the whole league with a summary of their most significant stats.
		Ultimate Team
            The best combination of players from all over the league for a full 12-player team.
		Box Scores
			Every box score saved in the specific timeframe.
	Box Scores
		Box Scores include Team Stats, Player Stats (simple & metric) as well as a Best Performers tab, which shows you the best players of the game from both teams.
	Live Box Scores
		Besides being able to input the box scores after the fact, you can easily keep track of a game that you're watching using Live Box Score. Every important stat has up and down arrows which allow you to easily add a 3PT to a specific player, an offensive rebound, or whatever else is happening.
			
NBA 2K12 Features
	This tool was originally made to offer a temporary workaround to the Association/ Season/My Player/Create A Legend team stats bug. Any game you entered and played would have the team stats of the teams that played in it all wrong afterwards. For example 85BPG, 92APG, 70RPG, etc. This really ruined the immersion the Association offers for those that like to play or even watch the games, instead of just simulating them from the Calendar. The Team Stats screen was wrong, the in-game overlays about the team were wrong. A mess. Even after the first console patches, 2K still hasn't fixed this issue. 
	
	Don't worry though.
	
	If you have the patience to follow the tutorial below for each game you play in your career, you should have your team stats automatically corrected, and also have full box-scores (with Team & Player Stats) of your games in NBA Stats Tracker for you to check out.
	
	Just follow these steps:
	REditor
		1. From in-game, select Play Game (even if you're going to simulate it), and let it save your career on the jersey selection screen.
		2. Alt-Tab out of the game and start REditor.
		3. Open your save in REditor, export everything to CSV (File > Export to CSV).
		4. Start NBA Stats Tracker.
		5. Open your database, or create a new one if you haven't previously done so.
		6. Click on Import from 2K12 Save, select the folder you saved the CSVs into in Step 3.
		7. Save the database.
		8. Alt-Tab into the game and play it or watch it, then after it's done let it save your career again. If you're simulating games, simulate just this game and then save immediately. Do not let any other games simulate since your last save.
		9. Alt-Tab out of the game and start REditor.
		10. Open your save in REditor, export everything to CSV (File > Export to CSV).
		11. Go back to the tool, open the database you saved before the game.
		12. Click on Import from 2K12 Save, select the folder you saved the CSVs into in Step 10.
		13. The tool should detect the game you played and ask you which team is the home team.
		14. The tool will now show you the full box score of the game so that you can verify everything's okay. Click on OK.
		15. Save the database.
		16. Click on Export to 2K12 Save, select the folder you saved the CSVs into in Step 3.
		17. Go to REditor, open your career again, import everything from CSV (File > Import from CSV).
		18. Save your career in REditor.
		19. You're done! Once you go back to NBA 2K12, make sure to quit your career and reload it, or your changes could be lost.

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

		Specific tables in the tool also support having TSV-formatted tables pasted into them:
			* Team Overview: Overview, Player Stats
			* Player Overview: Overview
			* Box Scores, via Tools > Paste
	    To make sure the data you're pasting is compatible, use the tab's table as a base by copying it and pasting it into a spreadsheet, and then making the changes in that. Any data/stats that can't be parsed will be ignored. Remember that whatever you paste isn't saved automatically, you'll have to save via the Overview window before switching team or player.

	Real NBA Stats
		This feature allows you to automatically grab the real league's team stats, player stats and box scores, and import them into the tool. All that with just a single-click! From there on, you can use all the tool's features, including averages, rankings, scouting reports, comparisons, CSV exports, and anything else that finds its way into the tool's features, onto the real NBA team stats. 	
		

Known Issues
	- Opponent stats seem to not be updated in some rare occurences. This bug hasn't been tracked down yet, but a temporary workaround exists as the "Recalculate Opponent Team Stats" feature in the Miscellaneous menu. If you find a certain procedure that reproduces the bug, please contact me using the link at the bottom of this Readme file.


Disclaimer
	The tool is still in beta. I've tested it in my environment, on my Association files and it seems to work perfectly. If you encounter any problems, you'll find a backup of your Association in the Saves folder. Keep one thing in mind however... 
	
					ALWAYS KEEP BACKUPS OF YOUR SAVES!
					
	I won't take any responsibility if this tool messes up your Saves. You've been warned.
	
	
Special thanks
	- JaoSming, for his roster editing tutorial, especially the part regarding the CRC checks
	- Onisak, for his help with debugging
	- Vl@d Zola Jr, for his invaluable help in making NST compatible with all NBA 2K12 saves
	- albidnis, for his idea to export to CSV
	- jrlocke, for being the first donator, a repeated and generous one
	- zizoux, for his idea to inject real stats, which ended up being the Custom Leagues & Real NBA Stats features
	- AreaOfEffect, for his help with debugging
	- Tinifu Tuitama, for his donation
	- koberulz, for his extensive suggestions and help with debugging
	- nbagnome, for his live box score idea and his donation
	- Lagoa, for his suggestions and help with debugging
	- intruda, for his donation
	- WBT99, for his extensive suggestions
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
		Uses an Object DeepCloning Code Example by Felix K. (http://stackoverflow.com/a/8026574/427338)
		Uses an Object DeepCloning Code Example by Rahul Dantkale of Indigo Architects (http://www.codeproject.com/Articles/23983/Object-Cloning-at-its-simplest)
		Uses a Sortable Binding List implementation by Tim Van Wassenhove (http://www.timvw.be/2008/08/02/presenting-the-sortablebindinglistt-take-two/)
	
	
Discussion/Support Thread
	http://forums.nba-live.com/viewtopic.php?f=143&t=84106
	
Donations Page
    http://students.ceid.upatras.gr/~aslanoglou/donate.html
