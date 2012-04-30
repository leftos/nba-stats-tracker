using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using LeftosCommonLibrary;
using Microsoft.Win32;

namespace NBA_2K12_Correct_Team_Stats
{
    internal class StatsTracker
    {
        public const int M = 0,
                         PF = 1,
                         PA = 2,
                         FGM = 4,
                         FGA = 5,
                         TPM = 6,
                         TPA = 7,
                         FTM = 8,
                         FTA = 9,
                         OREB = 10,
                         DREB = 11,
                         STL = 12,
                         TO = 13,
                         BLK = 14,
                         AST = 15,
                         FOUL = 16;

        public const int PPG = 0,
                         PAPG = 1,
                         FGp = 2,
                         FGeff = 3,
                         TPp = 4,
                         TPeff = 5,
                         FTp = 6,
                         FTeff = 7,
                         RPG = 8,
                         ORPG = 9,
                         DRPG = 10,
                         SPG = 11,
                         BPG = 12,
                         TPG = 13,
                         APG = 14,
                         FPG = 15,
                         Wp = 16,
                         Weff = 17;

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
            foreach (var kvp in TeamOrder)
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

        public static int[][] calculateRankings(TeamStats[] _teamStats, bool playoffs = false)
        {
            int len = _teamStats.GetLength(0);
            var rating = new int[len][];
            for (int i = 0; i < len; i++)
            {
                rating[i] = new int[20];
            }
            for (int k = 0; k < len; k++)
            {
                for (int i = 0; i < 19; i++)
                {
                    rating[k][i] = 1;
                    for (int j = 0; j < len; j++)
                    {
                        if (j != k)
                        {
                            if (!playoffs)
                            {
                                if (_teamStats[j].averages[i] > _teamStats[k].averages[i])
                                {
                                    rating[k][i]++;
                                }
                            }
                            else
                            {
                                if (_teamStats[j].pl_averages[i] > _teamStats[k].pl_averages[i])
                                {
                                    rating[k][i]++;
                                }
                            }
                        }
                    }
                }
                rating[k][19] = _teamStats[k].getGames();
            }
            return rating;
        }

        public static TeamStats[] GetStats(string fn, ref SortedDictionary<string, int> TeamOrder, ref PlayoffTree pt,
                                           bool havePT = false)
        {
            var _teamStats = new TeamStats[30];
            for (int i = 0; i < 30; i++)
            {
                _teamStats[i] = new TeamStats();
            }
            if (!havePT) pt = null;

            string ext = Tools.getExtension(fn);

            if (ext.ToUpperInvariant() == "PMG")
            {
                if (!havePT)
                {
                    pt = new PlayoffTree();
                    MessageBoxResult r =
                        MessageBox.Show("Do you have a saved Playoff Tree you want to load for this save file?",
                                        "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                    {
                        var ptw = new playoffTreeW();
                        ptw.ShowDialog();
                        if (!pt.done) return new TeamStats[1];

                        var spt = new SaveFileDialog();
                        spt.Title = "Please select a file to save the Playoff Tree to...";
                        spt.InitialDirectory = AppDocsPath;
                        spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        spt.ShowDialog();

                        if (spt.FileName == "") return new TeamStats[1];

                        try
                        {
                            FileStream stream = File.Open(spt.FileName, FileMode.Create);
                            var bf = new BinaryFormatter();
                            bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                            bf.Serialize(stream, pt);
                            stream.Close();
                        }
                        catch (Exception ex)
                        {
                            App.errorReport(ex, "Trying to save playoff tree");
                        }
                    }
                    else if (r == MessageBoxResult.Yes)
                    {
                        var ofd = new OpenFileDialog();
                        ofd.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                        ofd.InitialDirectory = AppDocsPath;
                        ofd.Title = "Please select the file you saved the Playoff Tree to for " +
                                    Tools.getSafeFilename(fn) + "...";
                        ofd.ShowDialog();

                        if (ofd.FileName == "") return new TeamStats[1];

                        FileStream stream = File.Open(ofd.FileName, FileMode.Open);
                        var bf = new BinaryFormatter();
                        bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                        pt = (PlayoffTree) bf.Deserialize(stream);
                        stream.Close();
                    }
                    else return new TeamStats[1];
                }
            }
            prepareOffsets(fn, _teamStats, ref TeamOrder, ref pt);

            var br = new BinaryReader(File.OpenRead(fn));
            var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();
            var buf = new byte[2];

            foreach (var kvp in TeamOrder)
            {
                if (kvp.Key != "")
                {
                    _teamStats[kvp.Value].name = kvp.Key;
                }
            }
            for (int i = 0; i < 30; i++)
            {
                ms.Seek(_teamStats[i].offset, SeekOrigin.Begin);
                ms.Read(buf, 0, 2);
                _teamStats[i].winloss[0] = buf[0];
                _teamStats[i].winloss[1] = buf[1];
                for (int j = 0; j < 18; j++)
                {
                    ms.Read(buf, 0, 2);
                    _teamStats[i].stats[j] = BitConverter.ToUInt16(buf, 0);
                }
            }

            if (pt != null && pt.teams[0] != "Invalid")
            {
                for (int i = 0; i < 16; i++)
                {
                    int id = TeamOrder[pt.teams[i]];
                    ms.Seek(_teamStats[id].pl_offset, SeekOrigin.Begin);
                    ms.Read(buf, 0, 2);
                    _teamStats[id].name = pt.teams[i];
                    _teamStats[id].pl_winloss[0] = buf[0];
                    _teamStats[id].pl_winloss[1] = buf[1];
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Read(buf, 0, 2);
                        _teamStats[id].pl_stats[j] = BitConverter.ToUInt16(buf, 0);
                    }
                }
            }

            for (int i = 0; i < _teamStats.Length; i++)
            {
                _teamStats[i].calcAvg();
            }

            return _teamStats;
        }

        public static void prepareOffsets(string fn, TeamStats[] _teamStats, ref SortedDictionary<string, int> TeamOrder,
                                          ref PlayoffTree pt)
        {
            // Stage 1
            string ext = Tools.getExtension(fn);
            if (ext.ToUpperInvariant() == "FXG" || ext.ToUpperInvariant() == "RFG")
            {
                _teamStats[0].offset = 3240532;
            }
            else if (ext.ToUpperInvariant() == "CMG")
            {
                _teamStats[0].offset = 5722996;
            }
            else if (ext.ToUpperInvariant() == "PMG")
            {
                _teamStats[TeamOrder[pt.teams[0]]].offset = 1813028;
            }

            // Stage 2
            if (ext.ToUpperInvariant() != "PMG")
            {
                for (int i = 1; i < 30; i++)
                {
                    _teamStats[i].offset = _teamStats[i - 1].offset + 40;
                }
                int inPlayoffs = checkIfIntoPlayoffs(fn, _teamStats, ref TeamOrder, ref pt);
                if (inPlayoffs == 1)
                {
                    _teamStats[TeamOrder[pt.teams[0]]].pl_offset = _teamStats[0].offset - 1440;
                    for (int i = 1; i < 16; i++)
                    {
                        _teamStats[TeamOrder[pt.teams[i]]].pl_offset =
                            _teamStats[TeamOrder[pt.teams[i - 1]]].pl_offset + 40;
                    }
                }
                else if (inPlayoffs == -1) return;
            }
            else
            {
                for (int i = 1; i < 16; i++)
                {
                    _teamStats[TeamOrder[pt.teams[i]]].pl_offset = _teamStats[TeamOrder[pt.teams[i - 1]]].pl_offset + 40;
                }
            }
        }

        public static int checkIfIntoPlayoffs(string fn, TeamStats[] _teamStats,
                                              ref SortedDictionary<string, int> TeamOrder, ref PlayoffTree pt)
        {
            int gamesInSeason = -1;
            string ptFile = "";
            string safefn = Tools.getSafeFilename(fn);
            string SettingsFile = AppDocsPath + safefn + ".cfg";
            string team = "";

            if (File.Exists(SettingsFile))
            {
                var sr = new StreamReader(SettingsFile);
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    string[] parts = line.Split('\t');
                    if (parts[0] == fn)
                    {
                        try
                        {
                            gamesInSeason = Convert.ToInt32(parts[1]);
                            ptFile = parts[2];
                            team = parts[3];

                            TeamOrder = setTeamOrder(team);
                        }
                        catch
                        {
                            gamesInSeason = -1;
                        }
                        break;
                    }
                }
                sr.Close();
            }
            if (gamesInSeason == -1)
            {
                gamesInSeason = askGamesInSeason(gamesInSeason);

                mode = askMode();

                TeamOrder = setTeamOrder(mode);

                saveSettingsForFile(fn, gamesInSeason, "", team, SettingsFile);
            }

            var br = new BinaryReader(File.OpenRead(fn));
            var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);
            br.Close();

            bool done = true;

            if (ptFile == "")
            {
                for (int i = 0; i < 30; i++)
                {
                    ms.Seek(_teamStats[i].offset, SeekOrigin.Begin);
                    var w = (byte) ms.ReadByte();
                    var l = (byte) ms.ReadByte();
                    uint total = Convert.ToUInt32(w + l);
                    if (total < gamesInSeason)
                    {
                        done = false;
                        break;
                    }
                }
            }

            if (done)
            {
                if (ptFile == "")
                {
                    pt = null;
                    pt = new PlayoffTree();
                    tempPT = new PlayoffTree();
                    var ptW = new playoffTreeW();
                    ptW.ShowDialog();
                    pt = tempPT;

                    if (!pt.done) return -1;

                    var spt = new SaveFileDialog();
                    spt.Title = "Please select a file to save the Playoff Tree to...";
                    spt.InitialDirectory = AppDocsPath;
                    spt.Filter = "Playoff Tree files (*.ptr)|*.ptr";
                    spt.ShowDialog();

                    if (spt.FileName == "") return -1;

                    ptFile = spt.FileName;

                    try
                    {
                        FileStream stream = File.Open(spt.FileName, FileMode.Create);
                        var bf = new BinaryFormatter();
                        bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                        bf.Serialize(stream, pt);
                        stream.Close();
                    }
                    catch (Exception ex)
                    {
                        App.errorReport(ex, "Trying to save playoff tree");
                    }
                }
                else
                {
                    FileStream stream = File.Open(ptFile, FileMode.Open);
                    var bf = new BinaryFormatter();
                    bf.AssemblyFormat = FormatterAssemblyStyle.Simple;

                    pt = (PlayoffTree) bf.Deserialize(stream);
                    stream.Close();
                }
            }

            saveSettingsForFile(fn, gamesInSeason, ptFile, team, SettingsFile);

            if (done) return 1;
            else return 0;
        }

        private static string askMode()
        {
            var at = new askTeamW(false);
            at.ShowDialog();
            return mode;
        }

        public static void saveSettingsForFile(string fn, int gamesInSeason, string ptFile, string team,
                                               string SettingsFile)
        {
            var sw2 = new StreamWriter(SettingsFile, false);
            sw2.WriteLine("{0}\t{1}\t{2}\t{3}", fn, gamesInSeason, ptFile, team);
            sw2.Close();
        }

        public static void updateSavegame(string fn, TeamStats[] tst, SortedDictionary<string, int> TeamOrder,
                                          PlayoffTree pt)
        {
            var br = new BinaryReader(File.OpenRead(fn));
            var ms = new MemoryStream(br.ReadBytes(Convert.ToInt32(br.BaseStream.Length)), true);

            if ((pt != null) && (pt.teams[0] != "Invalid"))
            {
                for (int i = 0; i < 16; i++)
                {
                    ms.Seek(tst[TeamOrder[pt.teams[i]]].pl_offset, SeekOrigin.Begin);
                    ms.Write(tst[TeamOrder[pt.teams[i]]].pl_winloss, 0, 2);
                    for (int j = 0; j < 18; j++)
                    {
                        ms.Write(BitConverter.GetBytes(tst[TeamOrder[pt.teams[i]]].pl_stats[j]), 0, 2);
                    }
                }
            }

            for (int i = 0; i < 30; i++)
            {
                ms.Seek(tst[i].offset, SeekOrigin.Begin);
                ms.Write(tst[i].winloss, 0, 2);
                for (int j = 0; j < 18; j++)
                {
                    ms.Write(BitConverter.GetBytes(tst[i].stats[j]), 0, 2);
                }
            }

            var bw = new BinaryWriter(File.OpenWrite(AppTempPath + Tools.getSafeFilename(fn)));
            ms.Position = 4;
            var t = new byte[1048576];
            int count;
            do
            {
                count = ms.Read(t, 0, 1048576);
                bw.Write(t, 0, count);
            } while (count > 0);

            br.Close();
            bw.Close();

            byte[] crc =
                Tools.ReverseByteOrder(Tools.StringToByteArray(Tools.getCRC(AppTempPath + Tools.getSafeFilename(fn))), 4);

            try
            {
                File.Delete(fn + ".bak");
            }
            catch
            {
            }
            File.Move(fn, fn + ".bak");
            var br2 = new BinaryReader(File.OpenRead(AppTempPath + Tools.getSafeFilename(fn)));
            var bw2 = new BinaryWriter(File.OpenWrite(fn));
            bw2.Write(crc);
            do
            {
                t = br2.ReadBytes(1048576);
                bw2.Write(t);
            } while (t.Length > 0);
            br2.Close();
            bw2.Close();

            File.Delete(AppTempPath + Tools.getSafeFilename(fn));
        }

        public static string averagesAndRankings(string teamName, TeamStats[] tst,
                                                 SortedDictionary<string, int> TeamOrder)
        {
            int id = -1;
            try
            {
                id = TeamOrder[teamName];
            }
            catch
            {
                return "";
            }
            int[][] rating = calculateRankings(tst);
            string text =
                String.Format(
                    "Win %: {32:F3} ({33})\nWin eff: {34:F2} ({35})\n\nPPG: {0:F1} ({16})\nPAPG: {1:F1} ({17})\n\nFG%: {2:F3} ({18})\nFGeff: {3:F2} ({19})\n3P%: {4:F3} ({20})\n3Peff: {5:F2} ({21})\n"
                    +
                    "FT%: {6:F3} ({22})\nFTeff: {7:F2} ({23})\n\nRPG: {8:F1} ({24})\nORPG: {9:F1} ({25})\nDRPG: {10:F1} ({26})\n\nSPG: {11:F1} ({27})\nBPG: {12:F1} ({28})\n"
                    + "TPG: {13:F1} ({29})\nAPG: {14:F1} ({30})\nFPG: {15:F1} ({31})",
                    tst[id].averages[PPG], tst[id].averages[PAPG], tst[id].averages[FGp],
                    tst[id].averages[FGeff], tst[id].averages[TPp], tst[id].averages[TPeff],
                    tst[id].averages[FTp], tst[id].averages[FTeff], tst[id].averages[RPG], tst[id].averages[ORPG],
                    tst[id].averages[DRPG], tst[id].averages[SPG],
                    tst[id].averages[BPG], tst[id].averages[TPG], tst[id].averages[APG], tst[id].averages[FPG],
                    rating[id][0], tst.GetLength(0) + 1 - rating[id][1], rating[id][2], rating[id][3], rating[id][4],
                    rating[id][5], rating[id][6], rating[id][7], rating[id][8], rating[id][9],
                    rating[id][10], rating[id][11], rating[id][12], tst.GetLength(0) + 1 - rating[id][13],
                    rating[id][14], tst.GetLength(0) + 1 - rating[id][15], tst[id].averages[Wp], rating[id][16],
                    tst[id].averages[Weff], rating[id][Weff]);
            return text;
        }

        public static string scoutReport(int[][] rating, int teamID, string teamName)
        {
            //public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            //FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            //TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17;
            string msg;
            msg = String.Format("{0}, the {1}", teamName, rating[teamID][17]);
            switch (rating[teamID][17])
            {
                case 1:
                case 21:
                    msg += "st";
                    break;
                case 2:
                case 22:
                    msg += "nd";
                    break;
                case 3:
                case 23:
                    msg += "rd";
                    break;
                default:
                    msg += "th";
                    break;
            }
            msg += " strongest team in the league right now, after having played " + rating[teamID][19].ToString() +
                   " games.\n\n";

            if ((rating[teamID][3] <= 5) && (rating[teamID][5] <= 5))
            {
                if (rating[teamID][7] <= 5)
                {
                    msg +=
                        "This team just can't be beaten offensively. One of the strongest in the league in all aspects.";
                }
                else
                {
                    msg += "Great team offensively. Even when they don't get to the line, they know how to raise the bar with "
                           + "efficiency in both 2 and 3 pointers.";
                }
            }
            else if ((rating[teamID][3] <= 10) && (rating[teamID][5] <= 10))
            {
                if (rating[teamID][7] <= 10)
                {
                    msg += "Top 10 in the league in everything offense, and they're one to worry about.";
                }
                else
                {
                    msg += "Although their free throwing is not on par with their other offensive qualities, you can't relax "
                           + "when playing against them. Top 10 in field goals and 3 pointers.";
                }
            }
            else if ((rating[teamID][3] <= 20) && (rating[teamID][5] <= 20))
            {
                if (rating[teamID][7] <= 10)
                {
                    msg += "Although an average offensive team (they can't seem to remain consistent from both inside and "
                           + "outside the arc), they can get back at you with their efficiency from the line.";
                }
                else
                {
                    msg += "Average offensive team. Not really efficient in anything they do when they bring the ball down "
                           + "the court.";
                }
            }
            else
            {
                if (rating[teamID][7] <= 10)
                {
                    msg += "They aren't consistent from the floor, but still manage to get to the line enough times and "
                           + "be good enough to make a difference.";
                }
                else
                {
                    msg += "One of the most inconsistent teams at the offensive end, and they aren't efficient enough from "
                           + "the line to make up for it.";
                }
            }
            msg += "\n\n";

            if (rating[teamID][3] <= 5)
                msg += "Top scoring team, one of the top 5 in field goal efficiency.";
            else if (rating[teamID][3] <= 10)
                msg +=
                    "You'll have to worry about their scoring efficiency, as they're one of the Top 10 in the league.";
            else if (rating[teamID][3] <= 20)
                msg += "Scoring is not their virtue, but they're not that bad either.";
            else if (rating[teamID][3] <= 30)
                msg += "You won't have to worry about their scoring, one of the least 10 efficient in the league.";

            int comp = rating[teamID][FGeff] - rating[teamID][FGp];
            if (comp < -15)
                msg +=
                    "\nThey score more baskets than their FG% would have you guess, but they need to work on getting more consistent.";
            else if (comp > 15)
                msg +=
                    "\nThey can be dangerous whenever they shoot the ball. Their offense just doesn't get them enough chances to shoot it, though.";

            msg += "\n";

            if (rating[teamID][5] <= 5)
                msg += "You'll need to always have an eye on the perimeter. They can turn a game around with their 3 pointers. "
                       + "They score well, they score a lot.";
            else if (rating[teamID][5] <= 10)
                msg +=
                    "Their 3pt shooting is bad news. They're in the top 10, and you can't relax playing against them.";
            else if (rating[teamID][5] <= 20)
                msg += "Not much to say about their 3pt shooting. Average, but it is there.";
            else if (rating[teamID][5] <= 30)
                msg +=
                    "Definitely not a threat from 3pt land, one of the worst in the league. They waste too many shots from there.";

            comp = rating[teamID][TPeff] - rating[teamID][TPp];
            if (comp < -15)
                msg +=
                    "\nThey'll get enough 3 pointers to go down each night, but not on a good enough percentage for that amount.";
            else if (comp > 15)
                msg += "\nWith their accuracy from the 3PT line, you'd think they'd shoot more of those.";

            msg += "\n";

            if (rating[teamID][7] <= 5)
                msg += "They tend to attack the lanes hard, getting to the line and making the most of it. They're one of the best "
                       + "teams in the league at it.";
            else if (rating[teamID][7] <= 10)
                msg +=
                    "One of the best teams in the league at getting to the line. They get enough free throws to punish the opposing team every night. Top 10.";
            else if (rating[teamID][7] <= 20)
                msg +=
                    "Average free throw efficiency, you don't have to worry about sending them to the line; at least as much as other aspects of their game.";
            else if (rating[teamID][7] <= 30)
                if (rating[teamID][FTp] < 15)
                    msg +=
                        "A team that you'll enjoy playing hard and aggressively against on defense. They don't know how to get to the line.";
                else
                    msg +=
                        "A team that doesn't know how to get to the line, or how to score from there. You don't have to worry about freebies against them.";

            comp = rating[teamID][FTeff] - rating[teamID][FTp];
            if (comp < -15)
                msg +=
                    "\nAlthough they get to the line a lot and make some free throws, they have to put up a lot to actually get that amount each night.";
            else if (comp > 15)
                msg +=
                    "\nThey're lethal when shooting free throws, but they need to play harder and get there more often.";

            msg += "\n";

            if (rating[teamID][14] <= 15)
                msg +=
                    "They know how to find the open man, and they get their offense going by getting it around the perimeter until a clean shot is there.";
            else if ((rating[teamID][14] > 15) && (rating[teamID][3] < 10))
                msg += "A team that prefers to run its offense through its core players in isolation. Not very good in assists, but they know how to get the job "
                       + "done more times than not.";
            else
                msg +=
                    "A team that seems to have some selfish players around, nobody really that efficient to carry the team into high percentages.";

            msg += "\n\n";

            if (31 - rating[teamID][PAPG] <= 5)
                msg +=
                    "Don't expect to get your score high against them. An elite defensive team, top 5 in points against them each night.";
            else if (31 - rating[teamID][PAPG] <= 10)
                msg +=
                    "One of the better defensive teams out there, limiting their opponents to low scores night in, night out.";
            else if (31 - rating[teamID][PAPG] <= 20)
                msg += "Average defensively, not much to show for it, but they're no blow-outs.";
            else if (31 - rating[teamID][PAPG] <= 30)
                msg +=
                    "This team has just forgotten what defense is. They're one of the 10 easiest teams to score against.";

            msg += "\n\n";

            if ((rating[teamID][9] <= 10) && (rating[teamID][11] <= 10) && (rating[teamID][12] <= 10))
                msg +=
                    "Hustle is their middle name. They attack the offensive glass, they block, they steal. Don't even dare to blink or get complacent.\n\n";
            else if ((rating[teamID][9] >= 20) && (rating[teamID][11] >= 20) && (rating[teamID][12] >= 20))
                msg +=
                    "This team just doesn't know what hustle means. You'll be doing circles around them if you're careful.\n\n";

            if (rating[teamID][8] <= 5)
                msg += "Sensational rebounding team, everybody jumps for the ball, no missed shot is left loose.";
            else if (rating[teamID][8] <= 10)
                msg +=
                    "You can't ignore their rebounding ability, they work together and are in the top 10 in rebounding.";
            else if (rating[teamID][8] <= 20)
                msg += "They crash the boards as much as the next guy, but they won't give up any freebies.";
            else if (rating[teamID][8] <= 30)
                msg +=
                    "Second chance points? One of their biggest fears. Low low LOW rebounding numbers; just jump for the ball and you'll keep your score high.";

            msg += " ";

            if ((rating[teamID][9] <= 10) && (rating[teamID][10] <= 10))
                msg +=
                    "The work they put on rebounding on both sides of the court is commendable. Both offensive and defensive rebounds, their bread and butter.";

            msg += "\n\n";

            if ((rating[teamID][11] <= 10) && (rating[teamID][12] <= 10))
                msg +=
                    "A team that knows how to play defense. They're one of the best in steals and blocks, and they make you work hard on offense.\n";
            else if (rating[teamID][11] <= 10)
                msg +=
                    "Be careful dribbling and passing. They won't be much trouble once you shoot the ball, but the trouble is getting there. Great in steals.\n";
            else if (rating[teamID][12] <= 10)
                msg +=
                    "Get that thing outta here! Great blocking team, they turn the lights off on any mismatched jumper or drive; sometimes even when you least expect it.\n";

            if ((rating[teamID][13] <= 10) && (rating[teamID][15] <= 10))
                msg +=
                    "Clumsy team to say the least. They're not careful with the ball, and they foul too much. Keep your eyes open and play hard.";
            else if (rating[teamID][13] < 10)
                msg +=
                    "Not good ball handlers, and that's being polite. Bottom 10 in turnovers, they have work to do until they get their offense going.";
            else if (rating[teamID][15] < 10)
                msg +=
                    "A team that's prone to fouling. You better drive the lanes as hard as you can, you'll get to the line a lot.";
            else
                msg += "This team is careful with and without the ball. They're good at keeping their turnovers down, and don't foul too much.\nDon't throw "
                       +
                       "your players into steals or fouls against them, because they play smart, and you're probably going to see the opposite call than the "
                       + "one you expected.";

            return msg;
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
            ts.stats[M] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[FGM] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[FGA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip FG%
            ts.stats[TPM] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[TPA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip 3G%
            ts.stats[FTM] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[FTA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip FT%
            ts.stats[OREB] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[DREB] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            grs_GetNextStat(ref sr); // Skip Total Rebounds
            ts.stats[AST] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[STL] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[BLK] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[TO] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[FOUL] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            ts.stats[PF] = Convert.ToUInt16(grs_GetNextStat(ref sr));

            do
            {
                line = sr.ReadLine();
            } while (line.Contains("<td align=\"left\" >Opponent</td>") == false);

            for (int i = 0; i < 19; i++)
                line = sr.ReadLine();

            ts.stats[PA] = Convert.ToUInt16(grs_GetNextStat(ref sr));
            sr.Close();
        }

        private static string grs_GetNextStat(ref StreamReader sr)
        {
            string line = sr.ReadLine();
            string[] parts1 = line.Split('>');
            string[] parts2 = parts1[1].Split('<');
            return parts2[0];
        }

        public static UInt16 getUShort(DataRow r, string ColumnName)
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

    // Unlike TeamStats which was designed before REditor implemented such stats,
    // PlayerStats were made according to REditor's standards, to make life 
    // easier when importing/exporting from REditor's CSV
    public class PlayerStats
    {
        // TODO: Metric Stats here

        public const int pGP = 0,
                         pGS = 1,
                         pMINS = 2,
                         pPTS = 3,
                         pDREB = 4,
                         pOREB = 5,
                         pAST = 6,
                         pSTL = 7,
                         pBLK = 8,
                         pTO = 9,
                         pFOUL = 10,
                         pFGM = 11,
                         pFGA = 12,
                         pTPM = 13,
                         pTPA = 14,
                         pFTM = 15,
                         pFTA = 16;

        public const int pMPG = 0,
                         pPPG = 1,
                         pDRPG = 2,
                         pORPG = 3,
                         pAPG = 4,
                         pSPG = 5,
                         pBPG = 6,
                         pTPG = 7,
                         pFPG = 8,
                         pFGp = 9,
                         pFGeff = 10,
                         pTPp = 11,
                         pTPeff = 12,
                         pFTp = 13,
                         pFTeff = 14,
                         pRPG = 15;

        public string FirstName;
        public int ID;
        public string LastName;
        public string Position1;
        public string Position2;
        public string TeamF;
        public string TeamS = "";
        public float[] averages = new float[16];
        public bool isActive;
        public bool isAllStar;
        public bool isInjured;
        public bool isNBAChampion;
        public UInt16[] stats = new UInt16[17];

        public PlayerStats(Player player)
        {
            ID = player.ID;
            LastName = player.LastName;
            FirstName = player.FirstName;
            Position1 = player.Position;
            Position2 = player.Position2;
            TeamF = player.Team;
            isActive = true;
            isInjured = false;
            isAllStar = false;
            isNBAChampion = false;

            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = 0;
            }

            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = 0;
            }
        }

        public PlayerStats(DataRow dataRow)
        {
            ID = StatsTracker.getInt(dataRow, "ID");
            LastName = StatsTracker.getString(dataRow, "LastName");
            FirstName = StatsTracker.getString(dataRow, "FirstName");
            Position1 = StatsTracker.getString(dataRow, "Position1");
            Position2 = StatsTracker.getString(dataRow, "Position2");
            TeamF = StatsTracker.getString(dataRow, "TeamFin");
            TeamS = StatsTracker.getString(dataRow, "TeamSta");
            isActive = StatsTracker.getBoolean(dataRow, "isActive");
            isInjured = StatsTracker.getBoolean(dataRow, "isInjured");
            isAllStar = StatsTracker.getBoolean(dataRow, "isAllStar");
            isNBAChampion = StatsTracker.getBoolean(dataRow, "isNBAChampion");

            stats[pGP] = StatsTracker.getUShort(dataRow, "GP");
            stats[pGS] = StatsTracker.getUShort(dataRow, "GS");
            stats[pMINS] = StatsTracker.getUShort(dataRow, "MINS");
            stats[pPTS] = StatsTracker.getUShort(dataRow, "PTS");
            stats[pFGM] = StatsTracker.getUShort(dataRow, "FGM");
            stats[pFGA] = StatsTracker.getUShort(dataRow, "FGA");
            stats[pTPM] = StatsTracker.getUShort(dataRow, "TPM");
            stats[pTPA] = StatsTracker.getUShort(dataRow, "TPA");
            stats[pFTM] = StatsTracker.getUShort(dataRow, "FTM");
            stats[pFTA] = StatsTracker.getUShort(dataRow, "FTA");
            stats[pOREB] = StatsTracker.getUShort(dataRow, "OREB");
            stats[pDREB] = StatsTracker.getUShort(dataRow, "DREB");
            stats[pSTL] = StatsTracker.getUShort(dataRow, "STL");
            stats[pTO] = StatsTracker.getUShort(dataRow, "TOS");
            stats[pBLK] = StatsTracker.getUShort(dataRow, "BLK");
            stats[pAST] = StatsTracker.getUShort(dataRow, "AST");
            stats[pFOUL] = StatsTracker.getUShort(dataRow, "FOUL");

            calcAvg();
        }

        public PlayerStats(int ID, string LastName, string FirstName, string Position1, string Position2, string TeamF, string TeamS,
            bool isActive, bool isInjured, bool isAllStar, bool isNBAChampion, DataRow dataRow)
        {
            this.ID = ID;
            this.LastName = LastName;
            this.FirstName = FirstName;
            this.Position1 = Position1;
            this.Position2 = Position2;
            this.TeamF = TeamF;
            this.TeamS = TeamS;
            this.isActive = isActive;
            this.isAllStar = isAllStar;
            this.isInjured = isInjured;
            this.isNBAChampion = isNBAChampion;

            stats[pGP] = StatsTracker.getUShort(dataRow, "GP");
            stats[pGS] = StatsTracker.getUShort(dataRow, "GS");
            stats[pMINS] = StatsTracker.getUShort(dataRow, "MINS");
            stats[pPTS] = StatsTracker.getUShort(dataRow, "PTS");

            string[] parts = StatsTracker.getString(dataRow, "FG").Split('-');

            stats[pFGM] = Convert.ToUInt16(parts[0]);
            stats[pFGA] = Convert.ToUInt16(parts[1]);

            parts = StatsTracker.getString(dataRow, "3PT").Split('-');

            stats[pTPM] = Convert.ToUInt16(parts[0]);
            stats[pTPA] = Convert.ToUInt16(parts[1]);

            parts = StatsTracker.getString(dataRow, "FT").Split('-');

            stats[pFTM] = Convert.ToUInt16(parts[0]);
            stats[pFTA] = Convert.ToUInt16(parts[1]);

            stats[pOREB] = StatsTracker.getUShort(dataRow, "OREB");
            stats[pDREB] = StatsTracker.getUShort(dataRow, "DREB");
            stats[pSTL] = StatsTracker.getUShort(dataRow, "STL");
            stats[pTO] = StatsTracker.getUShort(dataRow, "TO");
            stats[pBLK] = StatsTracker.getUShort(dataRow, "BLK");
            stats[pAST] = StatsTracker.getUShort(dataRow, "AST");
            stats[pFOUL] = StatsTracker.getUShort(dataRow, "FOUL");

            calcAvg();
        }

        public PlayerStats(PlayerStatsRow playerStatsRow)
        {
            LastName = playerStatsRow.LastName;
            FirstName = playerStatsRow.FirstName;

            stats[pGP] = playerStatsRow.GP;
            stats[pGS] = playerStatsRow.GS;
            stats[pMINS] = playerStatsRow.MINS;
            stats[pPTS] = playerStatsRow.PTS;
            stats[pFGM] = playerStatsRow.FGM;
            stats[pFGA] = playerStatsRow.FGA;
            stats[pTPM] = playerStatsRow.TPM;
            stats[pTPA] = playerStatsRow.TPA;
            stats[pFTM] = playerStatsRow.FTM;
            stats[pFTA] = playerStatsRow.FTA;
            stats[pOREB] = playerStatsRow.OREB;
            stats[pDREB] = playerStatsRow.DREB;
            stats[pSTL] = playerStatsRow.STL;
            stats[pTO] = playerStatsRow.TOS;
            stats[pBLK] = playerStatsRow.BLK;
            stats[pAST] = playerStatsRow.AST;
            stats[pFOUL] = playerStatsRow.FOUL;

            ID = playerStatsRow.ID;
            Position1 = playerStatsRow.Position1;
            Position2 = playerStatsRow.Position2;
            TeamF = playerStatsRow.TeamF;
            TeamS = playerStatsRow.TeamS;
            isActive = playerStatsRow.isActive;
            isAllStar = playerStatsRow.isAllStar;
            isInjured = playerStatsRow.isInjured;
            isNBAChampion = playerStatsRow.isNBAChampion;
        }

        public int calcAvg()
        {
            int games = stats[pGP];
            if (stats[pGP] == 0) games = -1;
            averages[pMPG] = (float) stats[pMINS]/games;
            averages[pPPG] = (float) stats[pPTS]/games;
            averages[pFGp] = (float) stats[pFGM]/stats[pFGA];
            averages[pFGeff] = averages[pFGp]*((float) stats[pFGM]/games);
            averages[pTPp] = (float) stats[pTPM]/stats[pTPA];
            averages[pTPeff] = averages[pTPp]*((float) stats[pTPM]/games);
            averages[pFTp] = (float) stats[pFTM]/stats[pFTA];
            averages[pFTeff] = averages[pFTp]*((float) stats[pFTM]/games);
            averages[pRPG] = (float) (stats[pOREB] + stats[pDREB])/games;
            averages[pORPG] = (float) stats[pOREB]/games;
            averages[pDRPG] = (float) stats[pDREB]/games;
            averages[pSPG] = (float) stats[pSTL]/games;
            averages[pBPG] = (float) stats[pBLK]/games;
            averages[pTPG] = (float) stats[pTO]/games;
            averages[pAPG] = (float) stats[pAST]/games;
            averages[pFPG] = (float) stats[pFOUL]/games;

            return games;
        }

        public void addBoxScore(PlayerBoxScore pbs)
        {
            if (ID != pbs.PlayerID) throw new Exception("Tried to update PlayerStats " + ID + " with PlayerBoxScore " + pbs.PlayerID);

            if (pbs.isStarter) stats[pGS]++;
            if (pbs.MINS > 0)
            {
                stats[pGP]++;
                stats[pMINS] += pbs.MINS;
            }
            stats[pPTS] += pbs.PTS;
            stats[pFGM] += pbs.FGM;
            stats[pFGA] += pbs.FGA;
            stats[pTPM] += pbs.TPM;
            stats[pTPA] += pbs.TPA;
            stats[pFTM] += pbs.FTM;
            stats[pFTA] += pbs.FTA;
            stats[pOREB] += pbs.OREB;
            stats[pDREB] += pbs.DREB;
            stats[pSTL] += pbs.STL;
            stats[pTO] += pbs.TOS;
            stats[pBLK] += pbs.BLK;
            stats[pAST] += pbs.AST;
            stats[pFOUL] += pbs.FOUL;

            calcAvg();
        }
    }

    public class PlayerBoxScore
    {
        //public ObservableCollection<KeyValuePair<int, string>> PlayersList { get; set; }
        public int PlayerID { get; set; }
        public string Team { get; set; }
        public bool isStarter { get; set; }
        public bool playedInjured { get; set; }
        public bool isOut { get; set; }
        public UInt16 MINS { get; set; }
        public UInt16 PTS { get; set; }
        public UInt16 FGM { get; set; }
        public UInt16 FGA { get; set; }
        public UInt16 TPM { get; set; }
        public UInt16 TPA { get; set; }
        public UInt16 FTM { get; set; }
        public UInt16 FTA { get; set; }
        public UInt16 REB { get; set; }
        public UInt16 OREB { get; set; }
        public UInt16 DREB { get; set; }
        public UInt16 STL { get; set; }
        public UInt16 TOS { get; set; }
        public UInt16 BLK { get; set; }
        public UInt16 AST { get; set; }
        public UInt16 FOUL { get; set; }

        public PlayerBoxScore()
        {
            PlayerID = -1;
            Team = "";
            isStarter = false;
            playedInjured = false;
            isOut = false;
            ResetStats();
        }

        public PlayerBoxScore(DataRow r)
        {
            PlayerID = StatsTracker.getInt(r, "PlayerID");
            Team = r["Team"].ToString();
            isStarter = StatsTracker.getBoolean(r, "isStarter");
            playedInjured = StatsTracker.getBoolean(r, "playedInjured");
            isOut = StatsTracker.getBoolean(r, "isOut");
            MINS = Convert.ToUInt16(r["MINS"].ToString());
            PTS = Convert.ToUInt16(r["PTS"].ToString());
            REB = Convert.ToUInt16(r["REB"].ToString());
            AST = Convert.ToUInt16(r["AST"].ToString());
            STL = Convert.ToUInt16(r["STL"].ToString());
            BLK = Convert.ToUInt16(r["BLK"].ToString());
            TOS = Convert.ToUInt16(r["TOS"].ToString());
            FGM = Convert.ToUInt16(r["FGM"].ToString());
            FGA = Convert.ToUInt16(r["FGA"].ToString());
            TPM = Convert.ToUInt16(r["TPM"].ToString());
            TPA = Convert.ToUInt16(r["TPA"].ToString());
            FTM = Convert.ToUInt16(r["FTM"].ToString());
            FTA = Convert.ToUInt16(r["FTA"].ToString());
            OREB = Convert.ToUInt16(r["OREB"].ToString());
            FOUL = Convert.ToUInt16(r["FOUL"].ToString());
            DREB = (UInt16)(REB - OREB);
        }

        public void ResetStats()
        {
            MINS = 0;
            PTS = 0;
            FGM = 0;
            FGA = 0;
            TPM = 0;
            TPA = 0;
            FTM = 0;
            FTA = 0;
            REB = 0;
            OREB = 0;
            DREB = 0;
            STL = 0;
            TOS = 0;
            BLK = 0;
            AST = 0;
            FOUL = 0;
        }
    }

    public class PlayerRankings
    {
        public const int pMPG = 0,
                         pPPG = 1,
                         pDRPG = 2,
                         pORPG = 3,
                         pAPG = 4,
                         pSPG = 5,
                         pBPG = 6,
                         pTPG = 7,
                         pFPG = 8,
                         pFGp = 9,
                         pFGeff = 10,
                         pTPp = 11,
                         pTPeff = 12,
                         pFTp = 13,
                         pFTeff = 14,
                         pRPG = 15;

        private Dictionary<int, int[]> rankings = new Dictionary<int, int[]>();
        public Dictionary<int, int[]> list = new Dictionary<int, int[]>();
        private int avgcount = (new PlayerStats(new Player(-1, "", "", "", "", ""))).averages.Length;

        public PlayerRankings(Dictionary<int, PlayerStats> pst)
        {
            foreach (var kvp in pst)
            {
                rankings.Add(kvp.Key, new int[avgcount]);
            }
            for (int j = 0; j < avgcount; j++)
            {
                var averages = new Dictionary<int, float>();
                foreach (var kvp in pst)
                {
                    averages.Add(kvp.Key, kvp.Value.averages[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankings[kvp.Key][j] = k;
                    k++;
                }
            }

            /*
            list = new Dictionary<int, int[]>();
            for (int i = 0; i<pst.Count; i++)
                list.Add(pst[i].ID, rankings[i]);
            */
            list = rankings;
        }
    }

    public class TeamStats
    {
        public const int M = 0,
                         PF = 1,
                         PA = 2,
                         FGM = 4,
                         FGA = 5,
                         TPM = 6,
                         TPA = 7,
                         FTM = 8,
                         FTA = 9,
                         OREB = 10,
                         DREB = 11,
                         STL = 12,
                         TO = 13,
                         BLK = 14,
                         AST = 15,
                         FOUL = 16;

        public const int PPG = 0,
                         PAPG = 1,
                         FGp = 2,
                         FGeff = 3,
                         TPp = 4,
                         TPeff = 5,
                         FTp = 6,
                         FTeff = 7,
                         RPG = 8,
                         ORPG = 9,
                         DRPG = 10,
                         SPG = 11,
                         BPG = 12,
                         TPG = 13,
                         APG = 14,
                         FPG = 15,
                         Wp = 16,
                         Weff = 17,
                         PD = 18;

        /// <summary>
        /// Averages for each team.
        /// 0: PPG, 1: PAPG, 2: FG%, 3: FGEff, 4: 3P%, 5: 3PEff, 6: FT%, 7:FTEff,
        /// 8: RPG, 9: ORPG, 10: DRPG, 11: SPG, 12: BPG, 13: TPG, 14: APG, 15: FPG, 16: W%,
        /// 17: Weff, 18: PD
        /// </summary>
        public float[] averages = new float[19];

        public string name;
        public Int32 offset;

        public float[] pl_averages = new float[19];
        public Int32 pl_offset;
        public UInt16[] pl_stats = new UInt16[18];
        public byte[] pl_winloss = new byte[2];

        /// <summary>
        /// Stats for each team.
        /// 0: M, 1: PF, 2: PA, 3: 0x0000, 4: FGM, 5: FGA, 6: 3PM, 7: 3PA, 8: FTM, 9: FTA,
        /// 10: OREB, 11: DREB, 12: STL, 13: TO, 14: BLK, 15: AST,
        /// 16: FOUL
        /// </summary>
        public UInt16[] stats = new UInt16[18];

        public byte[] winloss = new byte[2];

        public TeamStats()
        {
            prepareEmpty();
        }

        public TeamStats(string name)
        {
            this.name = name;
            prepareEmpty();
        }

        private void prepareEmpty()
        {
            winloss[0] = Convert.ToByte(0);
            winloss[1] = Convert.ToByte(0);
            pl_winloss[0] = Convert.ToByte(0);
            pl_winloss[1] = Convert.ToByte(0);
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = 0;
                pl_stats[i] = 0;
            }
            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = 0;
                pl_averages[i] = 0;
            }
        }

        public int calcAvg()
        {
            int games = winloss[0] + winloss[1];
            int pl_games = pl_winloss[0] + pl_winloss[1];

            if (games == 0) games = -1;
            averages[Wp] = (float) winloss[0]/games;
            averages[Weff] = averages[Wp]*winloss[0];
            averages[PPG] = (float) stats[PF]/games;
            averages[PAPG] = (float) stats[PA]/games;
            averages[FGp] = (float) stats[FGM]/stats[FGA];
            averages[FGeff] = averages[FGp]*((float) stats[FGM]/games);
            averages[TPp] = (float) stats[TPM]/stats[TPA];
            averages[TPeff] = averages[TPp]*((float) stats[TPM]/games);
            averages[FTp] = (float) stats[FTM]/stats[FTA];
            averages[FTeff] = averages[FTp]*((float) stats[FTM]/games);
            averages[RPG] = (float) (stats[OREB] + stats[DREB])/games;
            averages[ORPG] = (float) stats[OREB]/games;
            averages[DRPG] = (float) stats[DREB]/games;
            averages[SPG] = (float) stats[STL]/games;
            averages[BPG] = (float) stats[BLK]/games;
            averages[TPG] = (float) stats[TO]/games;
            averages[APG] = (float) stats[AST]/games;
            averages[FPG] = (float) stats[FOUL]/games;
            averages[PD] = averages[PPG] - averages[PAPG];

            pl_averages[Wp] = (float) pl_winloss[0]/pl_games;
            pl_averages[Weff] = pl_averages[Wp]*pl_winloss[0];
            pl_averages[PPG] = (float) pl_stats[PF]/pl_games;
            pl_averages[PAPG] = (float) pl_stats[PA]/pl_games;
            pl_averages[FGp] = (float) pl_stats[FGM]/pl_stats[FGA];
            pl_averages[FGeff] = pl_averages[FGp]*((float) pl_stats[FGM]/pl_games);
            pl_averages[TPp] = (float) pl_stats[TPM]/pl_stats[TPA];
            pl_averages[TPeff] = pl_averages[TPp]*((float) pl_stats[TPM]/pl_games);
            pl_averages[FTp] = (float) pl_stats[FTM]/pl_stats[FTA];
            pl_averages[FTeff] = pl_averages[FTp]*((float) pl_stats[FTM]/pl_games);
            pl_averages[RPG] = (float) (pl_stats[OREB] + pl_stats[DREB])/pl_games;
            pl_averages[ORPG] = (float) pl_stats[OREB]/pl_games;
            pl_averages[DRPG] = (float) pl_stats[DREB]/pl_games;
            pl_averages[SPG] = (float) pl_stats[STL]/pl_games;
            pl_averages[BPG] = (float) pl_stats[BLK]/pl_games;
            pl_averages[TPG] = (float) pl_stats[TO]/pl_games;
            pl_averages[APG] = (float) pl_stats[AST]/pl_games;
            pl_averages[FPG] = (float) pl_stats[FOUL]/pl_games;
            pl_averages[PD] = pl_averages[PPG] - pl_averages[PAPG];

            return games;
        }

        internal int getGames()
        {
            int games = winloss[0] + winloss[1];
            return games;
        }

        internal int getPlayoffGames()
        {
            int pl_games = pl_winloss[0] + pl_winloss[1];
            return pl_games;
        }
    }

    public class Rankings
    {
        public const int PPG = 0,
                         PAPG = 1,
                         FGp = 2,
                         FGeff = 3,
                         TPp = 4,
                         TPeff = 5,
                         FTp = 6,
                         FTeff = 7,
                         RPG = 8,
                         ORPG = 9,
                         DRPG = 10,
                         SPG = 11,
                         BPG = 12,
                         TPG = 13,
                         APG = 14,
                         FPG = 15,
                         Wp = 16,
                         Weff = 17,
                         PD = 18;

        public int[][] rankings;

        public Rankings(TeamStats[] _tst)
        {
            rankings = new int[_tst.Length][];
            for (int i = 0; i < _tst.Length; i++)
            {
                rankings[i] = new int[_tst[i].averages.Length];
            }
            for (int j = 0; j < _tst[0].averages.Length; j++)
            {
                var averages = new Dictionary<int, float>();
                for (int i = 0; i < _tst.Length; i++)
                {
                    averages.Add(i, _tst[i].averages[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankings[kvp.Key][j] = k;
                    k++;
                }
            }
        }
    }

    public class BoxScore
    {
        public UInt16 AST1;
        public UInt16 AST2;
        public UInt16 BLK1;
        public UInt16 BLK2;
        public UInt16 FGA1;
        public UInt16 FGA2;
        public UInt16 FGM1;
        public UInt16 FGM2;
        public UInt16 FTA1;
        public UInt16 FTA2;
        public UInt16 FTM1;
        public UInt16 FTM2;
        public UInt16 OFF1;
        public UInt16 OFF2;
        public UInt16 PF1;
        public UInt16 PF2;
        public UInt16 PTS1;
        public UInt16 PTS2;
        public UInt16 REB1;
        public UInt16 REB2;
        public UInt16 STL1;
        public UInt16 STL2;
        public int SeasonNum;
        public UInt16 TO1;
        public UInt16 TO2;
        public UInt16 TPA1;
        public UInt16 TPA2;
        public UInt16 TPM1;
        public UInt16 TPM2;
        public string Team1;
        public string Team2;
        public int bshistid = -1;
        public bool doNotUpdate;
        public bool done;
        public DateTime gamedate;
        public int id = -1;
        public bool isPlayoff;
    }

    public class BoxScoreEntry
    {
        public BoxScore bs;
        public DateTime date;
        public bool mustUpdate;
        public List<PlayerBoxScore> pbsList; 

        public BoxScoreEntry(BoxScore bs)
        {
            this.bs = bs;
            date = DateTime.Now;
        }

        public BoxScoreEntry(BoxScore bs, DateTime date, List<PlayerBoxScore> pbsList)
        {
            this.bs = bs;
            this.date = date;
            this.pbsList = pbsList;
        }
    }

    [Serializable]
    public class PlayoffTree : ISerializable
    {
        public bool done;
        public string[] teams = new string[16];

        public PlayoffTree()
        {
            teams[0] = "Invalid";
        }

        public PlayoffTree(SerializationInfo info, StreamingContext ctxt)
        {
            teams = (string[]) info.GetValue("teams", typeof (string[]));
            done = (bool) info.GetValue("done", typeof (bool));
        }

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("teams", teams);
            info.AddValue("done", done);
        }

        #endregion
    }

    public class Player
    {
        public Player()
        {
        }

        public Player(int ID, string Team, string LastName, string FirstName, string Position1, string Position2)
        {
            this.ID = ID;
            this.Team = Team;
            this.LastName = LastName;
            this.FirstName = FirstName;
            Position = Position1;
            this.Position2 = Position2;
        }

        public int ID { get; set; }
        public string Team { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Position { get; set; }
        public string Position2 { get; set; }
    }

    public class PlayerStatsRow
    {
        public const int pGP = 0,
                         pGS = 1,
                         pMINS = 2,
                         pPTS = 3,
                         pDREB = 4,
                         pOREB = 5,
                         pAST = 6,
                         pSTL = 7,
                         pBLK = 8,
                         pTO = 9,
                         pFOUL = 10,
                         pFGM = 11,
                         pFGA = 12,
                         pTPM = 13,
                         pTPA = 14,
                         pFTM = 15,
                         pFTA = 16;

        public const int pMPG = 0,
                         pPPG = 1,
                         pDRPG = 2,
                         pORPG = 3,
                         pAPG = 4,
                         pSPG = 5,
                         pBPG = 6,
                         pTPG = 7,
                         pFPG = 8,
                         pFGp = 9,
                         pFGeff = 10,
                         pTPp = 11,
                         pTPeff = 12,
                         pFTp = 13,
                         pFTeff = 14,
                         pRPG = 15;

        public PlayerStatsRow(PlayerStats ps)
        {
            LastName = ps.LastName;
            FirstName = ps.FirstName;

            GP = ps.stats[pGP];
            GS = ps.stats[pGS];
            MINS = ps.stats[pMINS];
            PTS = ps.stats[pPTS];
            FGM = ps.stats[pFGM];
            FGA = ps.stats[pFGA];
            TPM = ps.stats[pTPM];
            TPA = ps.stats[pTPA];
            FTM = ps.stats[pFTM];
            FTA = ps.stats[pFTA];
            OREB = ps.stats[pOREB];
            DREB = ps.stats[pDREB];
            REB = (UInt16) (OREB + DREB);
            STL = ps.stats[pSTL];
            TOS = ps.stats[pTO];
            BLK = ps.stats[pBLK];
            AST = ps.stats[pAST];
            FOUL = ps.stats[pFOUL];

            MPG = ps.averages[pMPG];
            PPG = ps.averages[pPPG];
            FGp = ps.averages[pFGp];
            FGeff = ps.averages[pFGeff];
            TPp = ps.averages[pTPp];
            TPeff = ps.averages[pTPeff];
            FTp = ps.averages[pFTp];
            FTeff = ps.averages[pFTeff];
            RPG = ps.averages[pRPG];
            ORPG = ps.averages[pORPG];
            DRPG = ps.averages[pDRPG];
            SPG = ps.averages[pSPG];
            TPG = ps.averages[pTPG];
            BPG = ps.averages[pBPG];
            APG = ps.averages[pAPG];
            FPG = ps.averages[pFPG];

            ID = ps.ID;
            Position1 = ps.Position1;
            Position2 = ps.Position2;
            TeamF = ps.TeamF;
            TeamS = ps.TeamS;
            isActive = ps.isActive;
            isAllStar = ps.isAllStar;
            isInjured = ps.isInjured;
            isNBAChampion = ps.isNBAChampion;
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }

        public UInt16 GP { get; set; }
        public UInt16 GS { get; set; }

        public UInt16 MINS { get; set; }
        public UInt16 PTS { get; set; }
        public UInt16 FGM { get; set; }
        public UInt16 FGA { get; set; }
        public UInt16 TPM { get; set; }
        public UInt16 TPA { get; set; }
        public UInt16 FTM { get; set; }
        public UInt16 FTA { get; set; }
        public UInt16 REB { get; set; }
        public UInt16 OREB { get; set; }
        public UInt16 DREB { get; set; }
        public UInt16 STL { get; set; }
        public UInt16 TOS { get; set; }
        public UInt16 BLK { get; set; }
        public UInt16 AST { get; set; }
        public UInt16 FOUL { get; set; }

        public float MPG { get; set; }
        public float PPG { get; set; }
        public float FGp { get; set; }
        public float FGeff { get; set; }
        public float TPp { get; set; }
        public float TPeff { get; set; }
        public float FTp { get; set; }
        public float FTeff { get; set; }
        public float RPG { get; set; }
        public float ORPG { get; set; }
        public float DRPG { get; set; }
        public float SPG { get; set; }
        public float TPG { get; set; }
        public float BPG { get; set; }
        public float APG { get; set; }
        public float FPG { get; set; }

        public int ID { get; set; }
        public string Position1 { get; set; }
        public string Position2 { get; set; }
        public string TeamF { get; set; }
        public string TeamS { get; set; }
        public bool isActive { get; set; }
        public bool isAllStar { get; set; }
        public bool isInjured { get; set; }
        public bool isNBAChampion { get; set; }

        // Not to be shown in DataGrid
    }
}