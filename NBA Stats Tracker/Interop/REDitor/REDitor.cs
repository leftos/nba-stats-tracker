#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using LeftosCommonLibrary;
using LeftosCommonLibrary.CommonDialogs;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.PastStats;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Players.Contracts;
using NBA_Stats_Tracker.Data.Players.Injuries;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows.MainInterface;
using NBA_Stats_Tracker.Windows.MainInterface.BoxScores;
using NBA_Stats_Tracker.Windows.MiscDialogs;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace NBA_Stats_Tracker.Interop.REDitor
{
    /// <summary>
    ///     Implements import and export methods for interoperability with REDitor.
    ///     This is the safest and most complete way to import and export stats from NBA 2K save files.
    /// </summary>
    public static class REDitor
    {
        /// <summary>
        ///     Implements the Positions enum used by 2K12 save files.
        /// </summary>
        private static readonly Dictionary<string, string> Positions = new Dictionary<string, string>
            {
                {"0", "PG"},
                {"1", "SG"},
                {"2", "SF"},
                {"3", "PF"},
                {"4", "C"},
                {"5", "None"}
            };

        public static List<int> TeamsThatPlayedAGame;
        public static List<int> PickedTeams;
        public static DateTime SelectedDate;

        /// <summary>
        ///     Creates a settings file. Settings files include teams participating in the save, as well as the default import/export folder.
        /// </summary>
        /// <param name="activeTeams">The active teams.</param>
        /// <param name="folder">The default import/export folder.</param>
        public static void CreateSettingsFile(List<Dictionary<string, string>> activeTeams, string folder)
        {
            string s1 = "Folder$$" + folder + "\n";
            string s2 = activeTeams.Aggregate("Active$$", (current, team) => current + (team["ID"] + "$%"));
            s2 = s2.Substring(0, s2.Length - 2);
            s2 += "\n";

            string stg = s1 + s2;

            var sfd = new SaveFileDialog
                {
                    Title = "Save Active Teams List",
                    Filter = "Active Teams List (*.red)|*.red",
                    DefaultExt = "red",
                    InitialDirectory = App.AppDocsPath
                };
            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            var sw = new StreamWriter(sfd.FileName);
            sw.Write(stg);
            sw.Close();
        }

        public static void ImportOld(Dictionary<int, PlayerStats> pst, Dictionary<int, TeamStats> tst, string folder)
        {
            if (tst.Count != 30)
            {
                MessageBox.Show("Can't import previous player stats to a database that doesn't have 30 teams. Please import " +
                                "your NBA 2K save once to this database to populate it properly before trying to import previous " +
                                "player stats.");
                MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-2);
                return;
            }

            var list = new List<string> {"1 season ago"};
            for (int i = 2; i <= 20; i++)
            {
                list.Add(i + " seasons ago");
            }

            int startAt;
            var ccw = new ComboChoiceWindow("Add player stats staring from...", list);
            if (ccw.ShowDialog() != true)
            {
                MainWindow.MWInstance.mainGrid.Visibility = Visibility.Visible;
                return;
            }

            startAt = Convert.ToInt32(ComboChoiceWindow.UserChoice.Split(' ')[0]);

            var ibw =
                new InputBoxWindow(
                    "Enter the current season (e.g. 2011-2012 by default in NBA 2K12, 2012 for a season" +
                    " taking place only in that year, etc.):", "2011-2012");
            if (ibw.ShowDialog() != true)
            {
                MainWindow.MWInstance.mainGrid.Visibility = Visibility.Visible;
                return;
            }

            int year;
            bool twoPartSeasonDesc = InputBoxWindow.UserInput.Contains("-");
            try
            {
                year = Convert.ToInt32(twoPartSeasonDesc ? InputBoxWindow.UserInput.Split('-')[0] : InputBoxWindow.UserInput);
            }
            catch
            {
                MessageBox.Show("The year you entered (" + InputBoxWindow.UserInput +
                                ") was not in a valid format.\nValid formats are:\n\t2012\n\t2011-2012");
                MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-2);
                return;
            }

            var seasonNames = new Dictionary<int, string>();
            for (int i = startAt; i <= 20; i++)
            {
                seasonNames.Add(i, twoPartSeasonDesc ? string.Format("{0}-{1}", year - i, (year - i + 1)) : (year - i).ToString());
            }

            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
            {
                MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-1);
                return;
            }

            var legalTTypes = new List<string> {"0", "4"};

            List<Dictionary<string, string>> validTeams = teams.FindAll(delegate(Dictionary<string, string> team)
                {
                    if (legalTTypes.IndexOf(team["TType"]) != -1)
                    {
                        return true;
                    }
                    return false;
                });

            List<Dictionary<string, string>> activeTeams = validTeams.FindAll(delegate(Dictionary<string, string> team)
                {
                    if (team["StatCurS"] != "-1")
                    {
                        return true;
                    }
                    return false;
                });
            if (activeTeams.Count < 30)
            {
                var dlw = new DualListWindow(validTeams, activeTeams);
                if (dlw.ShowDialog() == false)
                {
                    MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-1);
                    return;
                }

                activeTeams = new List<Dictionary<string, string>>(MainWindow.SelectedTeams);

                if (MainWindow.SelectedTeamsChanged)
                {
                    CreateSettingsFile(activeTeams, folder);
                }
            }

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            foreach (var team in activeTeams)
            {
                int id = -1;
                string name = team["Name"];
                if (tst.All(pair => pair.Value.Name != name))
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (!tst.ContainsKey(i))
                        {
                            id = i;
                            break;
                        }
                    }
                }
                else
                {
                    id = tst.Single(pair => pair.Value.Name == name).Key;
                }
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                rosters[id] = new List<int>
                    {
                        Convert.ToInt32(team["Ros_PG"]),
                        Convert.ToInt32(team["Ros_SG"]),
                        Convert.ToInt32(team["Ros_SF"]),
                        Convert.ToInt32(team["Ros_PF"]),
                        Convert.ToInt32(team["Ros_C"])
                    };
                for (int i = 6; i <= 12; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 13; i <= 20; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
            }


            List<Dictionary<string, string>> validPlayers = players.FindAll(delegate(Dictionary<string, string> player)
                {
                    if (player["PlType"] == "4" || player["PlType"] == "5" || player["PlType"] == "6")
                    {
                        if ((player["IsFA"] == "0" && player["TeamID1"] != "-1") || (player["IsFA"] == "1"))
                        {
                            return true;
                        }
                    }
                    return false;
                });

            var pw = new ProgressWindow("Please wait as player career stats are being imported...");
            pw.Show();
            var bw = new BackgroundWorker {WorkerReportsProgress = true};
            bw.DoWork += delegate
                {
                    int count = validPlayers.Count;
                    var ppsList = new List<PastPlayerStats>();
                    for (int i = 0; i < count; i++)
                    {
                        int percentProgress = i*100/count;
                        if (percentProgress%5 == 0)
                        {
                            bw.ReportProgress(percentProgress);
                        }
                        Dictionary<string, string> player = validPlayers[i];
                        int playerID = Convert.ToInt32(player["ID"]);

                        string lastName = player["Last_Name"];
                        string firstName = player["First_Name"];

                        int pTeam;
                        var curTeam = new TeamStats();
                        try
                        {
                            pTeam = rosters.Single(r => r.Value.Contains(playerID)).Key;
                            curTeam = tst[pTeam];
                        }
                        catch (InvalidOperationException)
                        {
                            pTeam = -1;
                        }

                        if (!activeTeamsIDs.Contains(pTeam) && player["IsFA"] != "1")
                        {
                            if (pst.ContainsKey(playerID) && pst[playerID].LastName == lastName && pst[playerID].FirstName == firstName)
                            {
                                pst[playerID].IsActive = false;
                                pst[playerID].TeamF = -1;
                                pst[playerID].IsHidden = true;
                            }
                            continue;
                        }

                        if (pst.ContainsKey(playerID) && (pst[playerID].LastName != lastName || pst[playerID].FirstName != firstName))
                        {
                            List<KeyValuePair<int, PlayerStats>> candidates =
                                pst.Where(
                                    pair =>
                                    pair.Value.LastName == lastName && pair.Value.FirstName == firstName &&
                                    pair.Value.IsHidden == false).ToList();
                            if (candidates.Count > 0)
                            {
                                bool found = false;
                                List<KeyValuePair<int, PlayerStats>> c2 =
                                    candidates.Where(pair => tst.ContainsKey(pair.Value.TeamF)).ToList();
                                if (c2.Count() == 1)
                                {
                                    playerID = c2.First().Value.ID;
                                    found = true;
                                }
                                else
                                {
                                    List<KeyValuePair<int, PlayerStats>> c4 =
                                        candidates.Where(pair => pair.Value.YearOfBirth.ToString() == player["BirthYear"]).ToList();
                                    if (c4.Count == 1)
                                    {
                                        playerID = c4.First().Value.ID;
                                        found = true;
                                    }
                                    else
                                    {
                                        if (pTeam != -1)
                                        {
                                            List<KeyValuePair<int, PlayerStats>> c3 =
                                                candidates.Where(pair => pair.Value.TeamF == curTeam.ID).ToList();
                                            if (c3.Count == 1)
                                            {
                                                playerID = c3.First().Value.ID;
                                                found = true;
                                            }
                                        }
                                    }
                                }
                                if (!found)
                                {
                                    var choices = new List<string>();
                                    foreach (var pair in candidates)
                                    {
                                        string choice = String.Format("{0}: {1} {2} (Born {3}", pair.Value.ID, pair.Value.FirstName,
                                                                      pair.Value.LastName, pair.Value.YearOfBirth);
                                        if (pair.Value.IsActive)
                                        {
                                            choice += String.Format(", plays in {0}", pair.Value.TeamF);
                                        }
                                        else
                                        {
                                            choice += ", free agent";
                                        }
                                        choice += ")";
                                        choices.Add(choice);
                                    }
                                    string message = String.Format("{0}: {1} {2} (Born {3}", player["ID"], player["First_Name"],
                                                                   player["Last_Name"], player["BirthYear"]);
                                    if (pTeam != -1)
                                    {
                                        message += String.Format(", plays in {0}", curTeam.DisplayName);
                                    }
                                    else
                                    {
                                        message += ", free agent";
                                    }
                                    message += ")";
                                    ccw = new ComboChoiceWindow(message, choices);
                                    if (ccw.ShowDialog() != true)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        playerID = Convert.ToInt32(ComboChoiceWindow.UserChoice.Split(':')[0]);
                                    }
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (!pst.ContainsKey(playerID))
                        {
                            continue;
                        }
                        PlayerStats curPlayer = pst[playerID];

                        string qr = "SELECT * FROM PastPlayerStats WHERE PlayerID = " + playerID + " ORDER BY \"SOrder\"";
                        DataTable dt = MainWindow.DB.GetDataTable(qr);
                        dt.Rows.Cast<DataRow>().ToList().ForEach(dr => ppsList.Add(new PastPlayerStats(dr)));
                        for (int j = startAt; j <= 20; j++)
                        {
                            string statEntryID = player["StatY" + j];
                            if (statEntryID == "-1")
                            {
                                break;
                            }
                            Dictionary<string, string> plStats = playerStats.Single(d => d["ID"] == statEntryID);
                            var prevStats = new PastPlayerStats();
                            string teamID2 = plStats["TeamID2"];
                            string teamID1 = plStats["TeamID1"];
                            if (teamID2 == "-1")
                            {
                                if (teamID1 != "-1")
                                {
                                    prevStats.TeamFName = teams.Single(team => team["ID"] == teamID1)["Name"];
                                }
                            }
                            else
                            {
                                prevStats.TeamFName = teams.Single(team => team["ID"] == teamID2)["Name"];
                                if (teamID1 != "-1")
                                {
                                    prevStats.TeamSName = teams.Single(team => team["ID"] == teamID1)["Name"];
                                }
                            }
                            prevStats.GP = Convert.ToUInt16(plStats["GamesP"]);
                            prevStats.GS = Convert.ToUInt16(plStats["GamesS"]);
                            prevStats.MINS = Convert.ToUInt16(plStats["Minutes"]);
                            prevStats.PTS = Convert.ToUInt16(plStats["Points"]);
                            prevStats.DREB = Convert.ToUInt16(plStats["DRebs"]);
                            prevStats.OREB = Convert.ToUInt16(plStats["ORebs"]);
                            prevStats.AST = Convert.ToUInt16(plStats["Assists"]);
                            prevStats.STL = Convert.ToUInt16(plStats["Steals"]);
                            prevStats.BLK = Convert.ToUInt16(plStats["Blocks"]);
                            prevStats.TOS = Convert.ToUInt16(plStats["TOs"]);
                            prevStats.FOUL = Convert.ToUInt16(plStats["Fouls"]);
                            prevStats.FGM = Convert.ToUInt16(plStats["FGMade"]);
                            prevStats.FGA = Convert.ToUInt16(plStats["FGAtt"]);
                            prevStats.TPM = Convert.ToUInt16(plStats["3PTMade"]);
                            prevStats.TPA = Convert.ToUInt16(plStats["3PTAtt"]);
                            prevStats.FTM = Convert.ToUInt16(plStats["FTMade"]);
                            prevStats.FTA = Convert.ToUInt16(plStats["FTAtt"]);
                            prevStats.PlayerID = playerID;
                            prevStats.SeasonName = seasonNames[j];
                            prevStats.IsPlayoff = false;
                            prevStats.Order = 20 - j;
                            prevStats.EndEdit();
                            ppsList.Add(prevStats);
                        }
                    }
                    bw.ReportProgress(99, "Please wait while the player career stats are being saved...");
                    SQLiteIO.SavePastPlayerStatsToDatabase(MainWindow.DB, ppsList);
                };

            bw.RunWorkerCompleted += delegate
                {
                    pw.CanClose = true;
                    pw.Close();
                    MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(0);
                };

            bw.ProgressChanged += delegate(object sender, ProgressChangedEventArgs args)
                {
                    pw.SetProgressBarValue(args.ProgressPercentage);
                    if (args.UserState != null)
                    {
                        pw.SetMessage(args.UserState.ToString());
                    }
                };

            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     Imports all team (and optionally) player stats from an REDitor-exported set of CSV files.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="folder">The folder containing the exported CSV files.</param>
        /// <param name="teamsOnly">
        ///     if set to <c>true</c>, only team stats will be imported.
        /// </param>
        /// <returns></returns>
        public static int ImportAll(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstOpp,
                                    ref Dictionary<int, PlayerStats> pst, string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
            {
                return -1;
            }

            var importMessages = new List<string>();
            var tradesList = new List<string>();
            var faSigningsList = new List<string>();
            var reSigningsList = new List<string>();
            var waiversList = new List<string>();
            var injuredList = new List<string>();
            var reInjuredList = new List<string>();
            var recoveredList = new List<string>();

            #region Import Teams & Team Stats

            var legalTTypes = new List<string> {"0", "4"};

            List<Dictionary<string, string>> validTeams = teams.FindAll(delegate(Dictionary<string, string> team)
                {
                    if (legalTTypes.IndexOf(team["TType"]) != -1)
                    {
                        return true;
                    }
                    return false;
                });

            List<Dictionary<string, string>> activeTeams = validTeams.FindAll(delegate(Dictionary<string, string> team)
                {
                    if (team["StatCurS"] != "-1")
                    {
                        return true;
                    }
                    return false;
                });
            if (activeTeams.Count < 30)
            {
                var dlw = new DualListWindow(validTeams, activeTeams);
                if (dlw.ShowDialog() == false)
                {
                    return -1;
                }

                activeTeams = new List<Dictionary<string, string>>(MainWindow.SelectedTeams);

                if (MainWindow.SelectedTeamsChanged)
                {
                    CreateSettingsFile(activeTeams, folder);
                }
            }

            bool madeNew = false;

            if (tst.Count != activeTeams.Count)
            {
                tst = new Dictionary<int, TeamStats>();
                tstOpp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }

            Dictionary<int, TeamStats> oldTST = tst.ToDictionary(ts => ts.Key, ts => ts.Value.Clone());
            Dictionary<int, TeamStats> oldtstOpp = tstOpp.ToDictionary(ts => ts.Key, ts => ts.Value.Clone());
            Dictionary<int, PlayerStats> oldPST = pst.ToDictionary(ps => ps.Key, ps => ps.Value.Clone());

            CreateDivisions();

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            activeTeams.Sort((p1, p2) => Convert.ToInt32(p1["ID"]).CompareTo(Convert.ToInt32(p2["ID"])));
            foreach (var team in activeTeams)
            {
                string name = team["Name"];
                int redID = Convert.ToInt32(team["ID"]);
                if (tst.Values.All(ts => ts.Name != name))
                {
                    if (tst.Keys.Contains(redID))
                    {
                        string oldName = tst[redID].Name;
                        tst[redID].Name = name;
                        tstOpp[redID].Name = name;
                        if (oldName == tst[redID].DisplayName)
                        {
                            tst[redID].DisplayName = name;
                            tstOpp[redID].DisplayName = name;
                        }
                    }
                }
                int id = tst.Values.Single(ts => ts.Name == name).ID;
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                if (madeNew)
                {
                    tst[id] = new TeamStats(id, name);
                    tstOpp[id] = new TeamStats(id, name);
                }

                int sStatsID = Convert.ToInt32(team["StatCurS"]);
                int pStatsID = Convert.ToInt32(team["StatCurP"]);

                Dictionary<string, string> sStats = teamStats.Find(delegate(Dictionary<string, string> s)
                    {
                        if (s["ID"] == sStatsID.ToString())
                        {
                            return true;
                        }
                        return false;
                    });

                tst[id].ID = id;
                tstOpp[id].ID = id;
                tst[id].Division = Convert.ToInt32(team["Division"]);
                tstOpp[id].Division = Convert.ToInt32(team["Division"]);

                if (sStats != null)
                {
                    tst[id].Record[0] = Convert.ToByte(sStats["Wins"]);
                    tst[id].Record[1] = Convert.ToByte(sStats["Losses"]);
                    tst[id].Totals[TAbbr.MINS] = Convert.ToUInt16(sStats["Mins"]);
                    tst[id].Totals[TAbbr.PF] = Convert.ToUInt16(sStats["PtsFor"]);
                    tst[id].Totals[TAbbr.PA] = Convert.ToUInt16(sStats["PtsAg"]);
                    tst[id].Totals[TAbbr.FGM] = Convert.ToUInt16(sStats["FGMade"]);
                    tst[id].Totals[TAbbr.FGA] = Convert.ToUInt16(sStats["FGAtt"]);
                    tst[id].Totals[TAbbr.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                    tst[id].Totals[TAbbr.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                    tst[id].Totals[TAbbr.FTM] = Convert.ToUInt16(sStats["FTMade"]);
                    tst[id].Totals[TAbbr.FTA] = Convert.ToUInt16(sStats["FTAtt"]);
                    tst[id].Totals[TAbbr.DREB] = Convert.ToUInt16(sStats["DRebs"]);
                    tst[id].Totals[TAbbr.OREB] = Convert.ToUInt16(sStats["ORebs"]);
                    tst[id].Totals[TAbbr.STL] = Convert.ToUInt16(sStats["Steals"]);
                    tst[id].Totals[TAbbr.BLK] = Convert.ToUInt16(sStats["Blocks"]);
                    tst[id].Totals[TAbbr.AST] = Convert.ToUInt16(sStats["Assists"]);
                    tst[id].Totals[TAbbr.FOUL] = Convert.ToUInt16(sStats["Fouls"]);
                    tst[id].Totals[TAbbr.TOS] = Convert.ToUInt16(sStats["TOs"]);

                    if (pStatsID != -1)
                    {
                        Dictionary<string, string> pStats = teamStats.Find(delegate(Dictionary<string, string> s)
                            {
                                if (s["ID"] == pStatsID.ToString())
                                {
                                    return true;
                                }
                                return false;
                            });
                        tst[id].PlRecord[0] = Convert.ToByte(pStats["Wins"]);
                        tst[id].PlRecord[1] = Convert.ToByte(pStats["Losses"]);
                        tst[id].PlTotals[TAbbr.MINS] = Convert.ToUInt16(pStats["Mins"]);
                        tst[id].PlTotals[TAbbr.PF] = Convert.ToUInt16(pStats["PtsFor"]);
                        tst[id].PlTotals[TAbbr.PA] = Convert.ToUInt16(pStats["PtsAg"]);
                        tst[id].PlTotals[TAbbr.FGM] = Convert.ToUInt16(pStats["FGMade"]);
                        tst[id].PlTotals[TAbbr.FGA] = Convert.ToUInt16(pStats["FGAtt"]);
                        tst[id].PlTotals[TAbbr.TPM] = Convert.ToUInt16(pStats["3PTMade"]);
                        tst[id].PlTotals[TAbbr.TPA] = Convert.ToUInt16(pStats["3PTAtt"]);
                        tst[id].PlTotals[TAbbr.FTM] = Convert.ToUInt16(pStats["FTMade"]);
                        tst[id].PlTotals[TAbbr.FTA] = Convert.ToUInt16(pStats["FTAtt"]);
                        tst[id].PlTotals[TAbbr.DREB] = Convert.ToUInt16(pStats["DRebs"]);
                        tst[id].PlTotals[TAbbr.OREB] = Convert.ToUInt16(pStats["ORebs"]);
                        tst[id].PlTotals[TAbbr.STL] = Convert.ToUInt16(pStats["Steals"]);
                        tst[id].PlTotals[TAbbr.BLK] = Convert.ToUInt16(pStats["Blocks"]);
                        tst[id].PlTotals[TAbbr.AST] = Convert.ToUInt16(pStats["Assists"]);
                        tst[id].PlTotals[TAbbr.FOUL] = Convert.ToUInt16(pStats["Fouls"]);
                        tst[id].PlTotals[TAbbr.TOS] = Convert.ToUInt16(pStats["TOs"]);
                    }
                }

                tst[id].CalcAvg();

                rosters[id] = new List<int>
                    {
                        Convert.ToInt32(team["Ros_PG"]),
                        Convert.ToInt32(team["Ros_SG"]),
                        Convert.ToInt32(team["Ros_SF"]),
                        Convert.ToInt32(team["Ros_PF"]),
                        Convert.ToInt32(team["Ros_C"])
                    };
                for (int i = 6; i <= 12; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 13; i <= 20; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            #endregion

            #region Import Players & Player Stats

            var duplicatePlayers = new List<string>();
            if (!teamsOnly)
            {
                List<Dictionary<string, string>> validPlayers = players.FindAll(delegate(Dictionary<string, string> player)
                    {
                        if (player["PlType"] == "4" || player["PlType"] == "5" || player["PlType"] == "6")
                        {
                            if ((player["IsFA"] == "0" && player["TeamID1"] != "-1") || (player["IsFA"] == "1"))
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                foreach (var player in validPlayers)
                {
                    int playerID = Convert.ToInt32(player["ID"]);

                    string lastName = player["Last_Name"];
                    string firstName = player["First_Name"];

#if DEBUG
                    //if (LastName == "Felton") System.Diagnostics.Debugger.Break();
#endif

                    int pTeam;
                    var curTeam = new TeamStats();
                    try
                    {
                        pTeam = rosters.Single(r => r.Value.Contains(playerID)).Key;
                        curTeam = tst[pTeam];
                    }
                    catch (InvalidOperationException)
                    {
                        pTeam = -1;
                    }

                    if (pTeam == -1 && player["IsFA"] != "1")
                    {
                        if (pst.ContainsKey(playerID) && pst[playerID].LastName == lastName && pst[playerID].FirstName == firstName)
                        {
                            pst[playerID].IsActive = false;
                            pst[playerID].TeamF = -1;
                            pst[playerID].IsHidden = true;
                        }
                        continue;
                    }

                    int playerStatsID = Convert.ToInt32(player["StatY0"]);

                    Dictionary<string, string> plStats = playerStats.Find(delegate(Dictionary<string, string> s)
                        {
                            if (s["ID"] == playerStatsID.ToString())
                            {
                                return true;
                            }
                            return false;
                        });

                    if (pst.ContainsKey(playerID) && (pst[playerID].LastName != lastName || pst[playerID].FirstName != firstName))
                    {
                        List<KeyValuePair<int, PlayerStats>> candidates =
                            pst.Where(
                                pair =>
                                pair.Value.LastName == lastName && pair.Value.FirstName == firstName && pair.Value.IsHidden == false)
                               .ToList();
                        if (candidates.Count > 0)
                        {
                            bool found = false;
                            Dictionary<int, TeamStats> temptst = tst;
                            List<KeyValuePair<int, PlayerStats>> c2 =
                                candidates.Where(pair => temptst.ContainsKey(pair.Value.TeamF)).ToList();
                            if (c2.Count() == 1)
                            {
                                playerID = c2.First().Value.ID;
                                found = true;
                            }
                            else
                            {
                                List<KeyValuePair<int, PlayerStats>> c4 =
                                    candidates.Where(pair => pair.Value.YearOfBirth.ToString() == player["BirthYear"]).ToList();
                                if (c4.Count == 1)
                                {
                                    playerID = c4.First().Value.ID;
                                    found = true;
                                }
                                else
                                {
                                    if (pTeam != -1)
                                    {
                                        List<KeyValuePair<int, PlayerStats>> c3 =
                                            candidates.Where(pair => pair.Value.TeamF == curTeam.ID).ToList();
                                        if (c3.Count == 1)
                                        {
                                            playerID = c3.First().Value.ID;
                                            found = true;
                                        }
                                    }
                                }
                            }
                            if (!found)
                            {
                                var choices = new List<string>();
                                foreach (var pair in candidates)
                                {
                                    string choice = String.Format("{0}: {1} {2} (Born {3}", pair.Value.ID, pair.Value.FirstName,
                                                                  pair.Value.LastName, pair.Value.YearOfBirth);
                                    if (pair.Value.IsActive)
                                    {
                                        choice += String.Format(", plays in {0}", tst[pair.Value.TeamF].DisplayName);
                                    }
                                    else
                                    {
                                        choice += ", free agent";
                                    }
                                    choice += ")";
                                    choices.Add(choice);
                                }
                                string message = String.Format("{0}: {1} {2} (Born {3}", player["ID"], player["First_Name"],
                                                               player["Last_Name"], player["BirthYear"]);
                                if (pTeam != -1)
                                {
                                    message += String.Format(", plays in {0}", curTeam.DisplayName);
                                }
                                else
                                {
                                    message += ", free agent";
                                }
                                message += ")";
                                var ccw = new ComboChoiceWindow(message, choices);
                                if (ccw.ShowDialog() != true)
                                {
                                    duplicatePlayers.Add(firstName + " " + lastName);
                                    continue;
                                }
                                else
                                {
                                    playerID = Convert.ToInt32(ComboChoiceWindow.UserChoice.Split(':')[0]);
                                }
                            }
                        }
                        else
                        {
                            playerID = createNewPlayer(ref pst, player);
                        }
                    }
                    else if (!pst.ContainsKey(playerID))
                    {
                        playerID = createNewPlayer(ref pst, player, playerID);
                    }
                    PlayerStats curPlayer = pst[playerID];
                    PlayerStats oldPlayer = curPlayer.Clone();

                    curPlayer.Position1 = (Position) Enum.Parse(typeof (Position), player["Pos"]);
                    curPlayer.Position2 = (Position) Enum.Parse(typeof (Position), player["SecondPos"]);
                    curPlayer.IsHidden = false;
                    curPlayer.YearsPro = Convert.ToInt32(player["YearsPro"]);
                    curPlayer.YearOfBirth = Convert.ToInt32(player["BirthYear"]);
                    curPlayer.Contract.Option = (PlayerContractOption) Enum.Parse(typeof (PlayerContractOption), player["COption"]);
                    curPlayer.Contract.ContractSalaryPerYear.Clear();
                    for (int i = 1; i < 7; i++)
                    {
                        int salary = Convert.ToInt32(player["CYear" + i]);
                        if (salary == 0)
                        {
                            break;
                        }

                        curPlayer.Contract.ContractSalaryPerYear.Add(salary);
                    }
                    curPlayer.Height = Convert.ToDouble(player["Height"]);
                    curPlayer.Weight = Convert.ToDouble(player["Weight"]);
                    curPlayer.Injury = new PlayerInjury(Convert.ToInt32(player["InjType"]), Convert.ToInt32(player["InjDaysLeft"]));

                    if (plStats != null)
                    {
                        string teamReal = pTeam.ToString();
                        string team1 = plStats["TeamID2"];
                        string team2 = plStats["TeamID1"];
                        bool hasBeenTraded = (team1 != "-1");

                        if (teamReal != "-1" && player["IsFA"] != "1")
                        {
                            TeamStats ts = tst[pTeam];
                        }
                        if (hasBeenTraded)
                        {
                            Dictionary<string, string> teamS = teams.Find(delegate(Dictionary<string, string> s)
                                {
                                    if (s["ID"] == team2)
                                    {
                                        return true;
                                    }
                                    return false;
                                });
                        }

                        PlayerStats ps = curPlayer;
                        ps.TeamF = pTeam;
                        ps.TeamS = Convert.ToInt32(team2);

                        ps.IsActive = (player["IsFA"] != "1" && teamReal != "-1");

                        ps.Totals[PAbbr.GP] = Convert.ToUInt16(plStats["GamesP"]);
                        ps.Totals[PAbbr.GS] = Convert.ToUInt16(plStats["GamesS"]);
                        ps.Totals[PAbbr.MINS] = Convert.ToUInt16(plStats["Minutes"]);
                        ps.Totals[PAbbr.PTS] = Convert.ToUInt16(plStats["Points"]);
                        ps.Totals[PAbbr.DREB] = Convert.ToUInt16(plStats["DRebs"]);
                        ps.Totals[PAbbr.OREB] = Convert.ToUInt16(plStats["ORebs"]);
                        ps.Totals[PAbbr.AST] = Convert.ToUInt16(plStats["Assists"]);
                        ps.Totals[PAbbr.STL] = Convert.ToUInt16(plStats["Steals"]);
                        ps.Totals[PAbbr.BLK] = Convert.ToUInt16(plStats["Blocks"]);
                        ps.Totals[PAbbr.TOS] = Convert.ToUInt16(plStats["TOs"]);
                        ps.Totals[PAbbr.FOUL] = Convert.ToUInt16(plStats["Fouls"]);
                        ps.Totals[PAbbr.FGM] = Convert.ToUInt16(plStats["FGMade"]);
                        ps.Totals[PAbbr.FGA] = Convert.ToUInt16(plStats["FGAtt"]);
                        ps.Totals[PAbbr.TPM] = Convert.ToUInt16(plStats["3PTMade"]);
                        ps.Totals[PAbbr.TPA] = Convert.ToUInt16(plStats["3PTAtt"]);
                        ps.Totals[PAbbr.FTM] = Convert.ToUInt16(plStats["FTMade"]);
                        ps.Totals[PAbbr.FTA] = Convert.ToUInt16(plStats["FTAtt"]);

                        ps.IsAllStar = Convert.ToBoolean(Convert.ToInt32(plStats["IsAStar"]));
                        ps.IsNBAChampion = Convert.ToBoolean(Convert.ToInt32(plStats["IsChamp"]));

                        ps.Injury = new PlayerInjury(Convert.ToInt32(player["InjType"]), Convert.ToInt32(player["InjDaysLeft"]));

                        ps.CalcAvg();

                        pst[playerID] = ps;
                    }
                    else
                    {
                        PlayerStats ps = curPlayer;

                        ps.TeamF = pTeam;

                        ps.IsActive = player["IsFA"] != "1";
                        ps.Injury = new PlayerInjury(Convert.ToInt32(player["InjType"]), Convert.ToInt32(player["InjDaysLeft"]));

                        ps.CalcAvg();

                        pst[playerID] = ps;
                    }

                    #region Import Messsages

                    string name = String.Format("{0} {1}", curPlayer.FirstName, curPlayer.LastName);
                    if (oldPlayer.TeamF != curPlayer.TeamF)
                    {
                        string msg;
                        if (curPlayer.IsActive && oldPlayer.IsActive)
                        {
                            msg = String.Format("{0} was traded from the {1} to the {2}.", name, tst[oldPlayer.TeamF].DisplayName,
                                                tst[curPlayer.TeamF].DisplayName);
                            tradesList.Add(msg);
                        }
                        else if (oldPlayer.IsActive)
                        {
                            msg = String.Format("{0} was released from the {1}.", name, tst[oldPlayer.TeamF].DisplayName);
                            waiversList.Add(msg);
                        }
                    }

                    if (oldPlayer.Contract.GetYears() < curPlayer.Contract.GetYears() && curPlayer.IsActive)
                    {
                        string msg = name;
                        bool reSigned;
                        if (!oldPlayer.IsActive && curPlayer.IsActive)
                        {
                            reSigned = false;
                            msg += " signed ";
                        }
                        else
                        {
                            reSigned = true;
                            msg += " re-signed ";
                        }
                        msg += String.Format("with the {0} on a {1}/{2:C0} ({3:C0} per year) contract.",
                                             tst[curPlayer.TeamF].DisplayName, curPlayer.Contract.GetYearsDesc(),
                                             curPlayer.Contract.GetTotal(), curPlayer.Contract.GetAverage());
                        if (reSigned)
                        {
                            reSigningsList.Add(msg);
                        }
                        else
                        {
                            faSigningsList.Add(msg);
                        }
                    }

                    if (oldPlayer.Injury.InjuryName != curPlayer.Injury.InjuryName)
                    {
                        if (!oldPlayer.Injury.IsInjured)
                        {
                            injuredList.Add(name + " got injured. Status: " + curPlayer.Injury.Status);
                        }
                        else if (!curPlayer.Injury.IsInjured)
                        {
                            recoveredList.Add(name + " is no longer injured.");
                        }
                        else
                        {
                            reInjuredList.Add(name + " was injured with " + oldPlayer.Injury.InjuryName +
                                              ", is now injured again. Status: " + curPlayer.Injury.Status);
                        }
                    }

                    #endregion
                }
            }

            if (duplicatePlayers.Count > 0)
            {
                string msg =
                    "The following names belong to two or more players in the database and the tool couldn't determine who to import to:\n\n";
                duplicatePlayers.ForEach(item => msg += item + ", ");
                msg = msg.TrimEnd(new[] {' ', ','});
                msg += "\n\nImport will continue, but there will be some stats missing." + "\n\nTo avoid this problem, either\n" +
                       "1) disable the duplicate occurences via (Miscellaneous > Enable/Disable Players For This Season...), or\n" +
                       "2) transfer the correct instance of the player to their current team.";
                MessageBox.Show(msg);
            }

            #endregion

            #region Check for box-scores we can calculate

            if (oldTST.Count == 30)
            {
                TeamsThatPlayedAGame = new List<int>();
                foreach (var team in tst)
                {
                    TeamStats newTeam = team.Value;
                    int teamID = team.Key;
                    TeamStats oldTeam = oldTST[teamID];

                    if (oldTeam.GetGames() + 1 == newTeam.GetGames() || oldTeam.GetPlayoffGames() + 1 == newTeam.GetPlayoffGames())
                    {
                        TeamsThatPlayedAGame.Add(team.Key);
                    }
                }

                if (TeamsThatPlayedAGame.Count >= 2)
                {
                    PickedTeams = new List<int>();
                    var dlw = new PickGamesWindow(TeamsThatPlayedAGame);

                    if (dlw.ShowDialog() == true)
                    {
                        for (int i = 0; i <= PickedTeams.Count - 2; i += 2)
                        {
                            int t1 = PickedTeams[i];
                            int t2 = PickedTeams[i + 1];

                            BoxScoreEntry bse = prepareBoxScore(tst, oldTST, pst, oldPST, t1, t2);

                            TeamBoxScore teamBoxScore = bse.BS;
                            BoxScoreWindow.CalculateTeamsFromPlayers(ref teamBoxScore,
                                                                     bse.PBSList.Where(pbs => pbs.TeamID == bse.BS.Team1ID),
                                                                     bse.PBSList.Where(pbs => pbs.TeamID == bse.BS.Team2ID));

                            if (teamBoxScore.PTS1 != getDiff(tst, oldTST, t1, TAbbr.PF, teamBoxScore.IsPlayoff) ||
                                teamBoxScore.PTS2 != getDiff(tst, oldTST, t2, TAbbr.PF, teamBoxScore.IsPlayoff))
                            {
                                MessageBox.Show(
                                    String.Format(
                                        "{0} @ {1} won't have its box-score imported because it couldn't be properly calculated. A possible reason for this is that one or more players participating in that game has been since traded away from the teams.",
                                        tst[t1].DisplayName, tst[t2].DisplayName), "NBA Stats Tracker", MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                                continue;
                            }

                            bse.BS.GameDate = SelectedDate;
                            bse.Date = bse.BS.GameDate;
                            TeamStats.AddTeamStatsFromBoxScore(bse.BS, ref oldTST, ref oldtstOpp, t1, t2);
                            MainWindow.BSHist.Add(bse);
                            tst[t1] = oldTST[t1].Clone();
                            tst[t2] = oldTST[t2].Clone();
                            tstOpp[t1] = oldtstOpp[t1].Clone();
                            tstOpp[t2] = oldtstOpp[t2].Clone();
                        }
                    }
                }
            }

            #endregion

            if (tradesList.Count + faSigningsList.Count + reSigningsList.Count + waiversList.Count > 0)
            {
                importMessages.Add("League Transactions");
                importMessages.Add("========================================");
                importMessages.Add("");
                if (tradesList.Count > 0)
                {
                    importMessages.Add("Players traded");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(tradesList);
                    importMessages.Add("");
                }
                if (faSigningsList.Count > 0)
                {
                    importMessages.Add("Players signed from free-agency");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(faSigningsList);
                    importMessages.Add("");
                }
                if (reSigningsList.Count > 0)
                {
                    importMessages.Add("Players that signed an extension");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(reSigningsList);
                    importMessages.Add("");
                }
                if (waiversList.Count > 0)
                {
                    importMessages.Add("Players waived");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(waiversList);
                    importMessages.Add("");
                }
                importMessages.Add("");
                importMessages.Add("");
            }
            if (injuredList.Count + reInjuredList.Count + recoveredList.Count > 0)
            {
                importMessages.Add("Injury Updates");
                importMessages.Add("========================================");
                importMessages.Add("");
                if (injuredList.Count > 0)
                {
                    importMessages.Add("Players injured");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(injuredList);
                    importMessages.Add("");
                }
                if (reInjuredList.Count > 0)
                {
                    importMessages.Add("Players that got injured again after recovery");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(reInjuredList);
                    importMessages.Add("");
                }
                if (recoveredList.Count > 0)
                {
                    importMessages.Add("Players recovered");
                    importMessages.Add("=========================");
                    importMessages.Add("");
                    importMessages.AddRange(recoveredList);
                    importMessages.Add("");
                }
            }

            if (importMessages.Count > 0)
            {
                importMessages.Add("");
                var cmw = new CopyableMessageWindow(importMessages.Aggregate((m1, m2) => m1 + "\n" + m2), "League News",
                                                    TextAlignment.Left);
                cmw.ShowDialog();
            }

            return 0;
        }

        public static int ImportPrevious(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstOpp,
                                         ref Dictionary<int, PlayerStats> pst, string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
            {
                return -1;
            }

            #region Import Teams & Team Stats

            var legalTTypes = new List<string> {"0", "4"};

            List<Dictionary<string, string>> validTeams = teams.FindAll(delegate(Dictionary<string, string> team)
                {
                    if (legalTTypes.IndexOf(team["TType"]) != -1)
                    {
                        return true;
                    }
                    return false;
                });

            List<Dictionary<string, string>> activeTeams = validTeams.FindAll(delegate(Dictionary<string, string> team)
                {
                    if (team["StatCurS"] != "-1")
                    {
                        return true;
                    }
                    return false;
                });
            /*
            if (activeTeams.Count == 0)
            {
                MessageBox.Show("No Team Stats found in save.");
                return -1;
            }
            */
            if (activeTeams.Count < 30)
            {
                var dlw = new DualListWindow(validTeams, activeTeams);
                if (dlw.ShowDialog() == false)
                {
                    return -1;
                }

                activeTeams = new List<Dictionary<string, string>>(MainWindow.SelectedTeams);

                if (MainWindow.SelectedTeamsChanged)
                {
                    CreateSettingsFile(activeTeams, folder);
                }
            }

            bool madeNew = false;

            if (tst.Count != activeTeams.Count)
            {
                tst = new Dictionary<int, TeamStats>();
                tstOpp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }

            Dictionary<int, TeamStats> oldTST = tst.ToDictionary(ts => ts.Key, ts => ts.Value.Clone());
            Dictionary<int, TeamStats> oldtstOpp = tstOpp.ToDictionary(ts => ts.Key, ts => ts.Value.Clone());
            Dictionary<int, PlayerStats> oldPST = pst.ToDictionary(ps => ps.Key, ps => ps.Value.Clone());

            CreateDivisions();

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            foreach (var team in activeTeams)
            {
                int id = -1;
                string name = team["Name"];
                if (oldTST.All(pair => pair.Value.Name != name))
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (!tst.ContainsKey(i))
                        {
                            id = i;
                            break;
                        }
                    }
                }
                else
                {
                    id = oldTST.Single(pair => pair.Value.Name == name).Key;
                }
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                if (madeNew)
                {
                    tst[id] = new TeamStats(id, name);
                    tstOpp[id] = new TeamStats(id, name);
                }

                int sStatsID = Convert.ToInt32(team["StatPrevS"]);
                int pStatsID = Convert.ToInt32(team["StatPrevP"]);

                Dictionary<string, string> sStats = teamStats.Find(delegate(Dictionary<string, string> s)
                    {
                        if (s["ID"] == sStatsID.ToString())
                        {
                            return true;
                        }
                        return false;
                    });

                TeamStats curTeam = tst[id];
                curTeam.ID = Convert.ToInt32(team["ID"]);
                curTeam.Division = Convert.ToInt32(team["Division"]);
                tstOpp[id].Division = Convert.ToInt32(team["Division"]);


                if (sStats != null)
                {
                    curTeam.Record[0] = Convert.ToByte(sStats["Wins"]);
                    curTeam.Record[1] = Convert.ToByte(sStats["Losses"]);
                    curTeam.Totals[TAbbr.MINS] = Convert.ToUInt16(sStats["Mins"]);
                    curTeam.Totals[TAbbr.PF] = Convert.ToUInt16(sStats["PtsFor"]);
                    curTeam.Totals[TAbbr.PA] = Convert.ToUInt16(sStats["PtsAg"]);
                    curTeam.Totals[TAbbr.FGM] = Convert.ToUInt16(sStats["FGMade"]);
                    curTeam.Totals[TAbbr.FGA] = Convert.ToUInt16(sStats["FGAtt"]);
                    curTeam.Totals[TAbbr.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                    curTeam.Totals[TAbbr.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                    curTeam.Totals[TAbbr.FTM] = Convert.ToUInt16(sStats["FTMade"]);
                    curTeam.Totals[TAbbr.FTA] = Convert.ToUInt16(sStats["FTAtt"]);
                    curTeam.Totals[TAbbr.DREB] = Convert.ToUInt16(sStats["DRebs"]);
                    curTeam.Totals[TAbbr.OREB] = Convert.ToUInt16(sStats["ORebs"]);
                    curTeam.Totals[TAbbr.STL] = Convert.ToUInt16(sStats["Steals"]);
                    curTeam.Totals[TAbbr.BLK] = Convert.ToUInt16(sStats["Blocks"]);
                    curTeam.Totals[TAbbr.AST] = Convert.ToUInt16(sStats["Assists"]);
                    curTeam.Totals[TAbbr.FOUL] = Convert.ToUInt16(sStats["Fouls"]);
                    curTeam.Totals[TAbbr.TOS] = Convert.ToUInt16(sStats["TOs"]);
                    //tstOpp[id].stats[t.TO] = Convert.ToUInt16(sStats["TOsAg"]);

                    if (pStatsID != -1)
                    {
                        Dictionary<string, string> pStats = teamStats.Find(delegate(Dictionary<string, string> s)
                            {
                                if (s["ID"] == pStatsID.ToString())
                                {
                                    return true;
                                }
                                return false;
                            });
                        curTeam.PlRecord[0] = Convert.ToByte(pStats["Wins"]);
                        curTeam.PlRecord[1] = Convert.ToByte(pStats["Losses"]);
                        curTeam.PlTotals[TAbbr.MINS] = Convert.ToUInt16(pStats["Mins"]);
                        curTeam.PlTotals[TAbbr.PF] = Convert.ToUInt16(pStats["PtsFor"]);
                        curTeam.PlTotals[TAbbr.PA] = Convert.ToUInt16(pStats["PtsAg"]);
                        curTeam.PlTotals[TAbbr.FGM] = Convert.ToUInt16(pStats["FGMade"]);
                        curTeam.PlTotals[TAbbr.FGA] = Convert.ToUInt16(pStats["FGAtt"]);
                        curTeam.PlTotals[TAbbr.TPM] = Convert.ToUInt16(pStats["3PTMade"]);
                        curTeam.PlTotals[TAbbr.TPA] = Convert.ToUInt16(pStats["3PTAtt"]);
                        curTeam.PlTotals[TAbbr.FTM] = Convert.ToUInt16(pStats["FTMade"]);
                        curTeam.PlTotals[TAbbr.FTA] = Convert.ToUInt16(pStats["FTAtt"]);
                        curTeam.PlTotals[TAbbr.DREB] = Convert.ToUInt16(pStats["DRebs"]);
                        curTeam.PlTotals[TAbbr.OREB] = Convert.ToUInt16(pStats["ORebs"]);
                        curTeam.PlTotals[TAbbr.STL] = Convert.ToUInt16(pStats["Steals"]);
                        curTeam.PlTotals[TAbbr.BLK] = Convert.ToUInt16(pStats["Blocks"]);
                        curTeam.PlTotals[TAbbr.AST] = Convert.ToUInt16(pStats["Assists"]);
                        curTeam.PlTotals[TAbbr.FOUL] = Convert.ToUInt16(pStats["Fouls"]);
                        curTeam.PlTotals[TAbbr.TOS] = Convert.ToUInt16(pStats["TOs"]);
                        //tstOpp[id].pl_stats[t.TO] = Convert.ToUInt16(pStats["TOsAg"]);
                    }
                }

                curTeam.CalcAvg();

                rosters[id] = new List<int>
                    {
                        Convert.ToInt32(team["Ros_PG"]),
                        Convert.ToInt32(team["Ros_SG"]),
                        Convert.ToInt32(team["Ros_SF"]),
                        Convert.ToInt32(team["Ros_PF"]),
                        Convert.ToInt32(team["Ros_C"])
                    };
                for (int i = 6; i <= 12; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 13; i <= 20; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            #endregion

            #region Import Players & Player Stats

            var duplicatePlayers = new List<string>();
            if (!teamsOnly)
            {
                List<Dictionary<string, string>> validPlayers = players.FindAll(delegate(Dictionary<string, string> player)
                    {
                        if (player["PlType"] == "4" || player["PlType"] == "5" || player["PlType"] == "6")
                        {
                            if (((player["IsFA"] == "0" && player["TeamID1"] != "-1") || (player["IsFA"] == "1")) &&
                                player["YearsPro"] != "1")
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                foreach (var player in validPlayers)
                {
                    int playerID = Convert.ToInt32(player["ID"]);

                    string lastName = player["Last_Name"];
                    string firstName = player["First_Name"];

                    //if (LastName == "Kemp") System.Diagnostics.Debugger.Break();

                    int pTeam;
                    try
                    {
                        pTeam = rosters.Single(r => r.Value.Contains(playerID)).Key;
                    }
                    catch (InvalidOperationException)
                    {
                        pTeam = -1;
                    }

                    PlayerStats curPlayer = pst[playerID];
                    if (!activeTeamsIDs.Contains(pTeam) && player["IsFA"] != "1")
                    {
                        if (pst.ContainsKey(playerID) && (curPlayer.LastName == lastName) && curPlayer.FirstName == firstName)
                        {
                            curPlayer.IsActive = false;
                            curPlayer.TeamF = -1;
                            curPlayer.IsHidden = true;
                        }
                        continue;
                    }

                    int playerStatsID = Convert.ToInt32(player["StatY1"]);

                    Dictionary<string, string> plStats = playerStats.Find(delegate(Dictionary<string, string> s)
                        {
                            if (s["ID"] == playerStatsID.ToString())
                            {
                                return true;
                            }
                            return false;
                        });

                    if (pst.ContainsKey(playerID) && (curPlayer.LastName != lastName || curPlayer.FirstName != firstName))
                    {
                        List<KeyValuePair<int, PlayerStats>> candidates =
                            pst.Where(
                                pair =>
                                pair.Value.LastName == lastName && pair.Value.FirstName == firstName && pair.Value.IsHidden == false)
                               .ToList();
                        if (candidates.Any())
                        {
                            Dictionary<int, TeamStats> temptst = tst;
                            List<KeyValuePair<int, PlayerStats>> c2 =
                                candidates.Where(pair => temptst.ContainsKey(pair.Value.TeamF)).ToList();
                            if (c2.Count() == 1)
                            {
                                playerID = c2.First().Value.ID;
                            }
                            else
                            {
                                if (pTeam != -1)
                                {
                                    KeyValuePair<int, TeamStats> curTeam =
                                        tst.Single(
                                            team =>
                                            team.Value.Name == activeTeams.Find(ateam => ateam["ID"] == pTeam.ToString())["Name"]);

                                    List<KeyValuePair<int, PlayerStats>> c3 =
                                        candidates.Where(pair => pair.Value.TeamF == curTeam.Value.ID).ToList();
                                    if (c3.Count == 1)
                                    {
                                        playerID = c3.First().Value.ID;
                                    }
                                    else
                                    {
                                        duplicatePlayers.Add(firstName + " " + lastName);
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            playerID = createNewPlayer(ref pst, player);
                        }
                    }
                    else if (!pst.ContainsKey(playerID))
                    {
                        playerID = createNewPlayer(ref pst, player, playerID);
                    }

                    curPlayer.IsHidden = false;
                    curPlayer.YearsPro = Convert.ToInt32(player["YearsPro"]);
                    curPlayer.YearOfBirth = Convert.ToInt32(player["BirthYear"]);
                    curPlayer.Height = Convert.ToDouble(player["Height"]);
                    curPlayer.Weight = Convert.ToDouble(player["Weight"]);

                    if (plStats != null)
                    {
                        string team1 = plStats["TeamID2"];
                        string team2 = plStats["TeamID1"];
                        bool hasBeenTraded = (team1 != "-1");

                        if (!hasBeenTraded)
                        {
                            team1 = team2;
                        }

                        PlayerStats ps = curPlayer;
                        ps.TeamF = Convert.ToInt32(team1);
                        ps.TeamS = Convert.ToInt32(team2);

                        ps.IsActive = (player["IsFA"] != "1" && team1 != "-1");

                        ps.Totals[PAbbr.GP] = Convert.ToUInt16(plStats["GamesP"]);
                        ps.Totals[PAbbr.GS] = Convert.ToUInt16(plStats["GamesS"]);
                        ps.Totals[PAbbr.MINS] = Convert.ToUInt16(plStats["Minutes"]);
                        ps.Totals[PAbbr.PTS] = Convert.ToUInt16(plStats["Points"]);
                        ps.Totals[PAbbr.DREB] = Convert.ToUInt16(plStats["DRebs"]);
                        ps.Totals[PAbbr.OREB] = Convert.ToUInt16(plStats["ORebs"]);
                        ps.Totals[PAbbr.AST] = Convert.ToUInt16(plStats["Assists"]);
                        ps.Totals[PAbbr.STL] = Convert.ToUInt16(plStats["Steals"]);
                        ps.Totals[PAbbr.BLK] = Convert.ToUInt16(plStats["Blocks"]);
                        ps.Totals[PAbbr.TOS] = Convert.ToUInt16(plStats["TOs"]);
                        ps.Totals[PAbbr.FOUL] = Convert.ToUInt16(plStats["Fouls"]);
                        ps.Totals[PAbbr.FGM] = Convert.ToUInt16(plStats["FGMade"]);
                        ps.Totals[PAbbr.FGA] = Convert.ToUInt16(plStats["FGAtt"]);
                        ps.Totals[PAbbr.TPM] = Convert.ToUInt16(plStats["3PTMade"]);
                        ps.Totals[PAbbr.TPA] = Convert.ToUInt16(plStats["3PTAtt"]);
                        ps.Totals[PAbbr.FTM] = Convert.ToUInt16(plStats["FTMade"]);
                        ps.Totals[PAbbr.FTA] = Convert.ToUInt16(plStats["FTAtt"]);

                        ps.IsAllStar = Convert.ToBoolean(Convert.ToInt32(plStats["IsAStar"]));
                        ps.IsNBAChampion = Convert.ToBoolean(Convert.ToInt32(plStats["IsChamp"]));

                        ps.Injury = new PlayerInjury(Convert.ToInt32(player["InjType"]), Convert.ToInt32(player["InjDaysLeft"]));

                        ps.CalcAvg();

                        pst[playerID] = ps;
                    }
                }
            }

            if (duplicatePlayers.Count > 0)
            {
                string msg =
                    "The following names belong to two or more players in the database and the tool couldn't determine who to import to:\n\n";
                duplicatePlayers.ForEach(item => msg += item + ", ");
                msg = msg.TrimEnd(new[] {' ', ','});
                msg += "\n\nImport will continue, but there will be some stats missing." + "\n\nTo avoid this problem, either\n" +
                       "1) disable the duplicate occurences via (Miscellaneous > Enable/Disable Players For This Season...), or\n" +
                       "2) transfer the correct instance of the player to their current team.";
                MessageBox.Show(msg);
            }

            #endregion

            return 0;
        }

        /// <summary>
        ///     Creates the NBA divisions and conferences.
        /// </summary>
        public static void CreateDivisions()
        {
            MainWindow.Conferences.Clear();
            MainWindow.Conferences.Add(new Conference {ID = 0, Name = "East"});
            MainWindow.Conferences.Add(new Conference {ID = 1, Name = "West"});

            MainWindow.Divisions.Clear();
            MainWindow.Divisions.Add(new Division {ID = 0, Name = "Atlantic", ConferenceID = 0});
            MainWindow.Divisions.Add(new Division {ID = 1, Name = "Central", ConferenceID = 0});
            MainWindow.Divisions.Add(new Division {ID = 2, Name = "Southeast", ConferenceID = 0});
            MainWindow.Divisions.Add(new Division {ID = 3, Name = "Southwest", ConferenceID = 1});
            MainWindow.Divisions.Add(new Division {ID = 4, Name = "Northwest", ConferenceID = 1});
            MainWindow.Divisions.Add(new Division {ID = 5, Name = "Pacific", ConferenceID = 1});
        }

        /// <summary>
        ///     Creates a new player and adds them to the player stats dictionary.
        /// </summary>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="player">The dictionary containing the player information.</param>
        /// <param name="preferredID">The preferred ID.</param>
        /// <returns></returns>
        private static int createNewPlayer(ref Dictionary<int, PlayerStats> pst, Dictionary<string, string> player,
                                           int preferredID = -1)
        {
            int playerID;
            if (preferredID == -1)
            {
                playerID = SQLiteIO.GetFreeID(MainWindow.CurrentDB,
                                              "Players" +
                                              (MainWindow.CurSeason != SQLiteIO.GetMaxSeason(MainWindow.CurrentDB)
                                                   ? "S" + MainWindow.CurSeason
                                                   : ""));
            }
            else
            {
                playerID = preferredID;
            }
            while (pst.ContainsKey(playerID))
            {
                playerID++;
            }
            pst.Add(playerID,
                    new PlayerStats(new Player
                        {
                            ID = playerID,
                            FirstName = player["First_Name"],
                            LastName = player["Last_Name"],
                            Position1 = (Position) Enum.Parse(typeof (Position), Positions[player["Pos"]]),
                            Position2 = (Position) Enum.Parse(typeof (Position), Positions[player["SecondPos"]])
                        }));
            return playerID;
        }

        /// <summary>
        ///     Calculates the box score by comparing the participating team's current and previous team and player stats.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="oldTST">The old team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="oldPST">The old player stats dictionary.</param>
        /// <param name="t1">The away team ID.</param>
        /// <param name="t2">The home team ID.</param>
        /// <returns></returns>
        private static BoxScoreEntry prepareBoxScore(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> oldTST,
                                                     Dictionary<int, PlayerStats> pst, Dictionary<int, PlayerStats> oldPST, int t1,
                                                     int t2)
        {
            bool isPlayoff = (tst[t1].GetPlayoffGames() > 0);
            var bs = new TeamBoxScore
                {
                    IsPlayoff = isPlayoff,
                    Team1ID = t1,
                    MINS1 = getDiff(tst, oldTST, t1, TAbbr.MINS, isPlayoff),
                    Team2ID = t2,
                    MINS2 = getDiff(tst, oldTST, t2, TAbbr.MINS, isPlayoff)
                };


            var bse = new BoxScoreEntry(bs) {PBSList = new List<PlayerBoxScore>()};

            /*
                        var team1Players = pst.Where(pair => pair.Value.TeamF == bs.Team1);
                        var team2Players = pst.Where(pair => pair.Value.TeamF == bs.Team2);
                        */

            IEnumerable<KeyValuePair<int, PlayerStats>> bothTeamsPlayers =
                pst.Where(pair => pair.Value.TeamF == bs.Team1ID || pair.Value.TeamF == bs.Team2ID);
            foreach (var playerKVP in bothTeamsPlayers)
            {
                KeyValuePair<int, PlayerStats> oldplayerKVP = oldPST.Single(pair => pair.Value.ID == playerKVP.Value.ID);

                PlayerStats newPlayer = playerKVP.Value;
                PlayerStats oldPlayer = oldplayerKVP.Value;

                PlayerBoxScore pbs;
                if (getDiff(newPlayer, oldPlayer, PAbbr.GP) == 1)
                {
                    pbs = new PlayerBoxScore
                        {
                            PlayerID = newPlayer.ID,
                            TeamID = newPlayer.TeamF,
                            IsStarter = (getDiff(newPlayer, oldPlayer, PAbbr.GS) == 1),
                            PlayedInjured = newPlayer.Injury.IsInjured,
                            MINS = getDiff(newPlayer, oldPlayer, PAbbr.MINS),
                            PTS = getDiff(newPlayer, oldPlayer, PAbbr.PTS),
                            OREB = getDiff(newPlayer, oldPlayer, PAbbr.OREB),
                            DREB = getDiff(newPlayer, oldPlayer, PAbbr.DREB),
                            AST = getDiff(newPlayer, oldPlayer, PAbbr.AST),
                            STL = getDiff(newPlayer, oldPlayer, PAbbr.STL),
                            BLK = getDiff(newPlayer, oldPlayer, PAbbr.BLK),
                            TOS = getDiff(newPlayer, oldPlayer, PAbbr.TOS),
                            FGM = getDiff(newPlayer, oldPlayer, PAbbr.FGM),
                            FGA = getDiff(newPlayer, oldPlayer, PAbbr.FGA),
                            TPM = getDiff(newPlayer, oldPlayer, PAbbr.TPM),
                            TPA = getDiff(newPlayer, oldPlayer, PAbbr.TPA),
                            FTM = getDiff(newPlayer, oldPlayer, PAbbr.FTM),
                            FTA = getDiff(newPlayer, oldPlayer, PAbbr.FTA),
                            FOUL = getDiff(newPlayer, oldPlayer, PAbbr.FOUL)
                        };
                    pbs.REB = (ushort) (pbs.OREB + pbs.DREB);
                    pbs.FGp = (float) pbs.FGM/pbs.FGA;
                    pbs.TPp = (float) pbs.TPM/pbs.TPA;
                    pbs.FTp = (float) pbs.FTM/pbs.FTA;
                }
                else
                {
                    pbs = new PlayerBoxScore {PlayerID = newPlayer.ID, TeamID = newPlayer.TeamF, IsOut = true};
                }

                bse.PBSList.Add(pbs);
            }
            bse.Date = DateTime.Today;
            bse.BS.GameDate = bse.Date;
            bse.BS.SeasonNum = MainWindow.CurSeason;

            return bse;
        }

        /// <summary>
        ///     Gets the difference of a team's stat's value between the current and previous stats.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="oldTST">The old team stats dictionary.</param>
        /// <param name="teamID">The team ID.</param>
        /// <param name="stat">The stat.</param>
        /// <param name="isPlayoff">
        ///     if set to <c>true</c>, the difference will be calculated based on the playoff stats.
        /// </param>
        /// <returns></returns>
        private static ushort getDiff(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> oldTST, int teamID, int stat,
                                      bool isPlayoff = false)
        {
            return !isPlayoff
                       ? (ushort) (tst[teamID].Totals[stat] - oldTST[teamID].Totals[stat])
                       : (ushort) (tst[teamID].PlTotals[stat] - oldTST[teamID].PlTotals[stat]);
        }

        /// <summary>
        ///     Gets the difference of a player's stat's value between the current and previous stats.
        /// </summary>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="oldPST">The old player stats dictionary.</param>
        /// <param name="id">The player's ID.</param>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        private static ushort getDiff(Dictionary<int, PlayerStats> pst, Dictionary<int, PlayerStats> oldPST, int id, int stat,
                                      bool isPlayoff = false)
        {
            return getDiff(pst[id], oldPST[id], stat, isPlayoff);
        }

        /// <summary>
        ///     Gets the difference of a player's stat's value between the current and previous stats.
        /// </summary>
        /// <param name="newPS">The new player stats instance.</param>
        /// <param name="oldPS">The old player stats instance.</param>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        private static ushort getDiff(PlayerStats newPS, PlayerStats oldPS, int stat, bool isPlayoff = false)
        {
            return !isPlayoff
                       ? (ushort) (newPS.Totals[stat] - oldPS.Totals[stat])
                       : (ushort) (newPS.PlTotals[stat] - oldPS.PlTotals[stat]);
        }

        /// <summary>
        ///     Exports all the team (and optionally player) stats and information to a set of CSV files, which can then be imported into REDitor.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="teamsOnly">
        ///     if set to <c>true</c>, only the teams' stats will be exported.
        /// </param>
        /// <returns></returns>
        public static int ExportAll(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstOpp, Dictionary<int, PlayerStats> pst,
                                    string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;
            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
            {
                return -1;
            }

            foreach (int key in tst.Keys)
            {
                TeamStats ts = tst[key];

                int id = ts.ID;

                int tindex = teams.FindIndex(delegate(Dictionary<string, string> s)
                    {
                        if (s["ID"] == id.ToString())
                        {
                            return true;
                        }
                        return false;
                    });

                Dictionary<string, string> team = teams[tindex];

                int sStatsID = Convert.ToInt32(team["StatCurS"]);
                int pStatsID = Convert.ToInt32(team["StatCurP"]);

                int sStatsIndex = teamStats.FindIndex(delegate(Dictionary<string, string> s)
                    {
                        if (s["ID"] == sStatsID.ToString())
                        {
                            return true;
                        }
                        return false;
                    });

                if (sStatsIndex != -1)
                {
                    teamStats[sStatsIndex]["Wins"] = ts.Record[0].ToString();
                    teamStats[sStatsIndex]["Losses"] = ts.Record[1].ToString();
                    teamStats[sStatsIndex]["Mins"] = ts.Totals[TAbbr.MINS].ToString();
                    teamStats[sStatsIndex]["PtsFor"] = ts.Totals[TAbbr.PF].ToString();
                    teamStats[sStatsIndex]["PtsAg"] = ts.Totals[TAbbr.PA].ToString();
                    teamStats[sStatsIndex]["FGMade"] = ts.Totals[TAbbr.FGM].ToString();
                    teamStats[sStatsIndex]["FGAtt"] = ts.Totals[TAbbr.FGA].ToString();
                    teamStats[sStatsIndex]["3PTMade"] = ts.Totals[TAbbr.TPM].ToString();
                    teamStats[sStatsIndex]["3PTAtt"] = ts.Totals[TAbbr.TPA].ToString();
                    teamStats[sStatsIndex]["FTMade"] = ts.Totals[TAbbr.FTM].ToString();
                    teamStats[sStatsIndex]["FTAtt"] = ts.Totals[TAbbr.FTA].ToString();
                    teamStats[sStatsIndex]["DRebs"] = ts.Totals[TAbbr.DREB].ToString();
                    teamStats[sStatsIndex]["ORebs"] = ts.Totals[TAbbr.OREB].ToString();
                    teamStats[sStatsIndex]["Steals"] = ts.Totals[TAbbr.STL].ToString();
                    teamStats[sStatsIndex]["Blocks"] = ts.Totals[TAbbr.BLK].ToString();
                    teamStats[sStatsIndex]["Assists"] = ts.Totals[TAbbr.AST].ToString();
                    teamStats[sStatsIndex]["Fouls"] = ts.Totals[TAbbr.FOUL].ToString();
                    teamStats[sStatsIndex]["TOs"] = ts.Totals[TAbbr.TOS].ToString();
                }

                if (pStatsID != -1)
                {
                    int pStatsIndex = teamStats.FindIndex(delegate(Dictionary<string, string> s)
                        {
                            if (s["ID"] == pStatsID.ToString())
                            {
                                return true;
                            }
                            return false;
                        });

                    if (pStatsIndex != -1)
                    {
                        teamStats[pStatsIndex]["Wins"] = ts.PlRecord[0].ToString();
                        teamStats[pStatsIndex]["Losses"] = ts.PlRecord[1].ToString();
                        teamStats[pStatsIndex]["Mins"] = ts.PlTotals[TAbbr.MINS].ToString();
                        teamStats[pStatsIndex]["PtsFor"] = ts.PlTotals[TAbbr.PF].ToString();
                        teamStats[pStatsIndex]["PtsAg"] = ts.PlTotals[TAbbr.PA].ToString();
                        teamStats[pStatsIndex]["FGMade"] = ts.PlTotals[TAbbr.FGM].ToString();
                        teamStats[pStatsIndex]["FGAtt"] = ts.PlTotals[TAbbr.FGA].ToString();
                        teamStats[pStatsIndex]["3PTMade"] = ts.PlTotals[TAbbr.TPM].ToString();
                        teamStats[pStatsIndex]["3PTAtt"] = ts.PlTotals[TAbbr.TPA].ToString();
                        teamStats[pStatsIndex]["FTMade"] = ts.PlTotals[TAbbr.FTM].ToString();
                        teamStats[pStatsIndex]["FTAtt"] = ts.PlTotals[TAbbr.FTA].ToString();
                        teamStats[pStatsIndex]["DRebs"] = ts.PlTotals[TAbbr.DREB].ToString();
                        teamStats[pStatsIndex]["ORebs"] = ts.PlTotals[TAbbr.OREB].ToString();
                        teamStats[pStatsIndex]["Steals"] = ts.PlTotals[TAbbr.STL].ToString();
                        teamStats[pStatsIndex]["Blocks"] = ts.PlTotals[TAbbr.BLK].ToString();
                        teamStats[pStatsIndex]["Assists"] = ts.PlTotals[TAbbr.AST].ToString();
                        teamStats[pStatsIndex]["Fouls"] = ts.PlTotals[TAbbr.FOUL].ToString();
                        teamStats[pStatsIndex]["TOs"] = ts.PlTotals[TAbbr.TOS].ToString();
                    }
                }
            }

            var unmatchedPlayers = new List<string>();
            if (!teamsOnly)
            {
                foreach (int key in pst.Keys)
                {
                    PlayerStats ps = pst[key];

                    int id = ps.ID;

                    Dictionary<string, string> player;
                    List<Dictionary<string, string>> candidates =
                        players.Where(dict => dict["Last_Name"] == ps.LastName && dict["First_Name"] == ps.FirstName).ToList();
                    try
                    {
                        player = candidates.Single(dict => dict["ID"] == id.ToString());
                    }
                    catch (InvalidOperationException)
                    {
                        try
                        {
                            player = candidates.Single(dict => dict["TeamID1"] == ps.TeamF.ToString());
                        }
                        catch (InvalidOperationException)
                        {
                            string message = String.Format("{0}: {1} {2} (Born {3}", ps.ID, ps.FirstName, ps.LastName, ps.YearOfBirth);
                            if (ps.TeamF != -1)
                            {
                                message += String.Format(", plays in {0}", tst[ps.TeamF].DisplayName);
                            }
                            else
                            {
                                message += ", free agent";
                            }
                            message += ")";
                            unmatchedPlayers.Add(message);
                            continue;
                        }
                    }

                    int playerStatsID = Convert.ToInt32(player["StatY0"]);

                    int playerStatsIndex = playerStats.FindIndex(delegate(Dictionary<string, string> s)
                        {
                            if (s["ID"] == playerStatsID.ToString())
                            {
                                return true;
                            }
                            return false;
                        });

                    if (playerStatsIndex != -1)
                    {
                        playerStats[playerStatsIndex]["GamesP"] = ps.Totals[PAbbr.GP].ToString();
                        playerStats[playerStatsIndex]["GamesS"] = ps.Totals[PAbbr.GS].ToString();
                        playerStats[playerStatsIndex]["Minutes"] = ps.Totals[PAbbr.MINS].ToString();
                        playerStats[playerStatsIndex]["Points"] = ps.Totals[PAbbr.PTS].ToString();
                        playerStats[playerStatsIndex]["DRebs"] = ps.Totals[PAbbr.DREB].ToString();
                        playerStats[playerStatsIndex]["ORebs"] = ps.Totals[PAbbr.OREB].ToString();
                        playerStats[playerStatsIndex]["Assists"] = ps.Totals[PAbbr.AST].ToString();
                        playerStats[playerStatsIndex]["Steals"] = ps.Totals[PAbbr.STL].ToString();
                        playerStats[playerStatsIndex]["Blocks"] = ps.Totals[PAbbr.BLK].ToString();
                        playerStats[playerStatsIndex]["TOs"] = ps.Totals[PAbbr.TOS].ToString();
                        playerStats[playerStatsIndex]["Fouls"] = ps.Totals[PAbbr.FOUL].ToString();
                        playerStats[playerStatsIndex]["FGMade"] = ps.Totals[PAbbr.FGM].ToString();
                        playerStats[playerStatsIndex]["FGAtt"] = ps.Totals[PAbbr.FGA].ToString();
                        playerStats[playerStatsIndex]["3PTMade"] = ps.Totals[PAbbr.TPM].ToString();
                        playerStats[playerStatsIndex]["3PTAtt"] = ps.Totals[PAbbr.TPA].ToString();
                        playerStats[playerStatsIndex]["FTMade"] = ps.Totals[PAbbr.FTM].ToString();
                        playerStats[playerStatsIndex]["FTAtt"] = ps.Totals[PAbbr.FTA].ToString();
                        playerStats[playerStatsIndex]["IsAStar"] = (ps.IsAllStar ? 1 : 0).ToString();
                        playerStats[playerStatsIndex]["IsChamp"] = (ps.IsNBAChampion ? 1 : 0).ToString();
                    }
                }
            }

            string path = folder + @"\Team_Stats.csv";
            CSV.CSVFromDictionaryList(teamStats, path);
            if (!teamsOnly)
            {
                path = folder + @"\Player_Stats.csv";
                CSV.CSVFromDictionaryList(playerStats, path);
                if (unmatchedPlayers.Count > 0)
                {
                    unmatchedPlayers.Add("");
                    MessageBox.Show(unmatchedPlayers.Aggregate((s1, s2) => s1 + "\n" + s2) + "\n" +
                                    "The above players have multiple matching ones in the NBA 2K save and can't be exported.\n" +
                                    "Please send your save and database to the developer.\n\nEverything else was exported successfully.");
                }
            }

            return 0;
        }

        /// <summary>
        ///     Populates the REDitor dictionary lists by importing the CSV data into them.
        ///     Each dictionary has Setting-Value pairs, where Setting is the column header, and Value is the corresponding value of that particular record.
        /// </summary>
        /// <param name="folder">The folder containing the REDitor-exported CSV files.</param>
        /// <param name="teams">The resulting teams information dictionary list.</param>
        /// <param name="players">The resulting players information dictionary list.</param>
        /// <param name="teamStats">The resulting team stats dictionary list.</param>
        /// <param name="playerStats">The resulting player stats dictionary list.</param>
        /// <returns></returns>
        private static int populateREDitorDictionaryLists(string folder, out List<Dictionary<string, string>> teams,
                                                          out List<Dictionary<string, string>> players,
                                                          out List<Dictionary<string, string>> teamStats,
                                                          out List<Dictionary<string, string>> playerStats)
        {
            try
            {
                teams = CSV.DictionaryListFromCSVFile(folder + @"\Teams.csv");
                players = CSV.DictionaryListFromCSVFile(folder + @"\Players.csv");
                teamStats = CSV.DictionaryListFromCSVFile(folder + @"\Team_Stats.csv");
                playerStats = CSV.DictionaryListFromCSVFile(folder + @"\Player_Stats.csv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                teams = null;
                players = null;
                teamStats = null;
                playerStats = null;
                return -1;
            }
            return 0;
        }
    }
}