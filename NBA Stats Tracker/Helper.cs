#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Windows;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Interop;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker
{
    internal class Helper
    {
        public static string AppDocsPath = MainWindow.AppDocsPath;
        public static string SavesPath = MainWindow.SavesPath;
        public static string AppTempPath = MainWindow.AppTempPath;
        public static string mode = "Mode 0";
        public static bool errorRealStats;

        public static PlayoffTree tempPT;

        public static SortedDictionary<string, int> setTeamOrder(string mode)
        {
            SortedDictionary<string, int> TeamOrder;

            switch (mode)
            {
                case "Mode 0":
                default:
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 9},
                                        {"Bulls", 28},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 7},
                                        {"Grizzlies", 6},
                                        {"Hawks", 16},
                                        {"Heat", 4},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 13},
                                        {"Knicks", 5},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 2},
                                        {"Pistons", 3},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 10},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 8},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 1":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 2},
                                        {"Bulls", 28},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 7},
                                        {"Grizzlies", 6},
                                        {"Hawks", 16},
                                        {"Heat", 4},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 13},
                                        {"Knicks", 5},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 9},
                                        {"Pistons", 10},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 3},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 8},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 6":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 8},
                                        {"Bulls", 28},
                                        {"Cavaliers", 12},
                                        {"Celtics", 13},
                                        {"Clippers", 6},
                                        {"Grizzlies", 5},
                                        {"Hawks", 16},
                                        {"Heat", 3},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 14},
                                        {"Knicks", 4},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 10},
                                        {"Pistons", 11},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 9},
                                        {"Suns", 2},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 7},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 2":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 8},
                                        {"Bulls", 28},
                                        {"Cavaliers", 12},
                                        {"Celtics", 13},
                                        {"Clippers", 6},
                                        {"Grizzlies", 5},
                                        {"Hawks", 16},
                                        {"Heat", 3},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 2},
                                        {"Knicks", 4},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 10},
                                        {"Pistons", 11},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 9},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 7},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 3":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 7},
                                        {"Bulls", 28},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 5},
                                        {"Grizzlies", 4},
                                        {"Hawks", 16},
                                        {"Heat", 2},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 13},
                                        {"Knicks", 3},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 9},
                                        {"Pistons", 10},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 8},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 6},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 4":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 7},
                                        {"Bulls", 24},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 5},
                                        {"Grizzlies", 4},
                                        {"Hawks", 16},
                                        {"Heat", 2},
                                        {"Hornets", 15},
                                        {"Jazz", 29},
                                        {"Kings", 13},
                                        {"Knicks", 3},
                                        {"Lakers", 27},
                                        {"Magic", 23},
                                        {"Mavericks", 25},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 9},
                                        {"Pistons", 10},
                                        {"Raptors", 21},
                                        {"Rockets", 28},
                                        {"Spurs", 8},
                                        {"Suns", 14},
                                        {"Thunder", 26},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 6},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 5":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 13},
                                        {"Bobcats", 10},
                                        {"Bucks", 0},
                                        {"Bulls", 4},
                                        {"Cavaliers", 20},
                                        {"Celtics", 14},
                                        {"Clippers", 5},
                                        {"Grizzlies", 16},
                                        {"Hawks", 22},
                                        {"Heat", 1},
                                        {"Hornets", 9},
                                        {"Jazz", 11},
                                        {"Kings", 29},
                                        {"Knicks", 17},
                                        {"Lakers", 28},
                                        {"Magic", 8},
                                        {"Mavericks", 26},
                                        {"Nets", 3},
                                        {"Nuggets", 27},
                                        {"Pacers", 19},
                                        {"Pistons", 25},
                                        {"Raptors", 21},
                                        {"Rockets", 24},
                                        {"Spurs", 12},
                                        {"Suns", 23},
                                        {"Thunder", 7},
                                        {"Timberwolves", 18},
                                        {"Trail Blazers", 2},
                                        {"Warriors", 6},
                                        {"Wizards", 15}
                                    };
                    break;
            }

            var checklist = new List<int>();
            foreach (KeyValuePair<string, int> kvp in TeamOrder)
            {
                if (checklist.Contains(kvp.Value) == false)
                {
                    checklist.Add(kvp.Value);
                }
                else
                {
                    MessageBox.Show("Conflict for " + mode + " TeamOrder on ID " + kvp.Value);
                    Environment.Exit(-1);
                }
            }

            return TeamOrder;
        }

        public static int askGamesInSeason(int gamesInSeason)
        {
            MessageBoxResult r =
                MessageBox.Show(
                    "How many games does each season have in this save?\n\n82 Games: Yes\n58 Games: No\n29 Games: Cancel",
                    "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes) gamesInSeason = 82;
            else if (r == MessageBoxResult.No) gamesInSeason = 58;
            else if (r == MessageBoxResult.Cancel) gamesInSeason = 28;
            return gamesInSeason;
        }


        public static TeamStats getRealStats(string team, bool useLocal = false)
        {
            var ts = new TeamStats();
            var web = new WebClient();
            string file = AppDocsPath + team + ".rst";

            var TeamNamesShort = new Dictionary<string, string>
                                     {
                                         {"76ers", "PHI"},
                                         {"Bobcats", "CHA"},
                                         {"Bucks", "MIL"},
                                         {"Bulls", "CHI"},
                                         {"Cavaliers", "CLE"},
                                         {"Celtics", "BOS"},
                                         {"Clippers", "LAC"},
                                         {"Grizzlies", "MEM"},
                                         {"Hawks", "ATL"},
                                         {"Heat", "MIA"},
                                         {"Hornets", "NOH"},
                                         {"Jazz", "UTA"},
                                         {"Kings", "SAC"},
                                         {"Knicks", "NYK"},
                                         {"Lakers", "LAL"},
                                         {"Magic", "ORL"},
                                         {"Mavericks", "DAL"},
                                         {"Nets", "NJN"},
                                         {"Nuggets", "DEN"},
                                         {"Pacers", "IND"},
                                         {"Pistons", "DET"},
                                         {"Raptors", "TOR"},
                                         {"Rockets", "HOU"},
                                         {"Spurs", "SAS"},
                                         {"Suns", "PHO"},
                                         {"Thunder", "OKC"},
                                         {"Timberwolves", "MIN"},
                                         {"Trail Blazers", "POR"},
                                         {"Warriors", "GSW"},
                                         {"Wizards", "WAS"}
                                     };

            ts.name = team;
            string tns = TeamNamesShort[team];
            if (!useLocal)
            {
                web.DownloadFile("http://www.basketball-reference.com/teams/" + tns + "/2012.html", file);
            }
            if (File.Exists(file))
            {
                grs_getStats(ref ts, file);

                if (errorRealStats)
                {
                    web.DownloadFile("http://www.basketball-reference.com/teams/" + tns + "/2012.html", file);
                    grs_getStats(ref ts, file);
                }

                ts.calcAvg();
            }
            else
            {
                ts.name = "Error";
            }
            return ts;
        }

        private static void grs_getStats(ref TeamStats ts, string file)
        {
            errorRealStats = false;
            var sr = new StreamReader(file);
            string line;
            try
            {
                do
                {
                    line = sr.ReadLine();
                } while (line.Contains("Team Splits") == false);
            }
            catch
            {
                errorRealStats = true;
                sr.Close();
                return;
            }

            for (int i = 0; i < 3; i++)
                line = sr.ReadLine();

            // <p><strong>3-10
            string[] parts1 = line.Split('>');
            string[] parts2 = parts1[2].Split('<');
            string[] _winloss = parts2[0].Split('-');
            ts.winloss[0] = Convert.ToByte(_winloss[0]);
            ts.winloss[1] = Convert.ToByte(_winloss[1]);

            do
            {
                line = sr.ReadLine();
            } while (line.Contains("<div class=\"table_container\" id=\"div_team\">") == false);
            do
            {
                line = sr.ReadLine();
            } while (line.Contains("<td align=\"left\" >Team</td>") == false);

            grs_GetNextStat(ref sr); // Skip games played
            ts.stats[t.MINS] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.FGM] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.FGA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip FG%
            ts.stats[t.TPM] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.TPA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip 3G%
            ts.stats[t.FTM] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.FTA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip FT%
            ts.stats[t.OREB] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.DREB] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip Total Rebounds
            ts.stats[t.AST] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.STL] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.BLK] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.TO] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.FOUL] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[t.PF] = Convert.ToUInt16(grs_GetNextStat(ref sr));

            do
            {
                line = sr.ReadLine();
            } while (line.Contains("<td align=\"left\" >Opponent</td>") == false);

            for (int i = 0; i < 19; i++)
                line = sr.ReadLine();

            ts.stats[t.PA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            sr.Close();
        }

        private static string grs_GetNextStat(ref StreamReader sr)
        {
            string line = sr.ReadLine();
            string[] parts1 = line.Split('>');
            string[] parts2 = parts1[1].Split('<');
            return parts2[0];
        }

        public static UInt16 getUInt16(DataRow r, string ColumnName)
        {
            return Convert.ToUInt16(r[ColumnName].ToString());
        }

        public static int getInt(DataRow r, string ColumnName)
        {
            return Convert.ToInt32(r[ColumnName].ToString());
        }

        public static Boolean getBoolean(DataRow r, string ColumnName)
        {
            string s = r[ColumnName].ToString();
            s = s.ToLower();
            return Convert.ToBoolean(s);
        }

        public static string getString(DataRow r, string ColumnName)
        {
            return r[ColumnName].ToString();
        }
    }
}