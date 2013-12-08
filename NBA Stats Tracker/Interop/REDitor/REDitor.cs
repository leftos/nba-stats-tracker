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

namespace NBA_Stats_Tracker.Interop.REDitor
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
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

    /// <summary>
    ///     Implements import and export methods for interoperability with REDitor. This is the safest and most complete way to import
    ///     and export stats from NBA 2K save files.
    /// </summary>
    public static class REDitor
    {
        /// <summary>Implements the Positions enum used by 2K12 save files.</summary>
        private static readonly Dictionary<string, string> Positions = new Dictionary<string, string>
            {
                { "0", "PG" },
                { "1", "SG" },
                { "2", "SF" },
                { "3", "PF" },
                { "4", "C" },
                { "5", "None" }
            };

        public static List<int> TeamsThatPlayedAGame;
        public static List<int> PickedTeams;
        public static DateTime SelectedDate;
        private static List<string> legalTTypes;

        /// <summary>Creates a settings file. Settings files include teams participating in the save, as well as the default import/export folder.</summary>
        /// <param name="activeTeams">The active teams.</param>
        /// <param name="folder">The default import/export folder.</param>
        private static void createSettingsFile(IEnumerable<Dictionary<string, string>> activeTeams, string folder)
        {
            var s1 = "Folder$$" + folder + "\n";
            var s2 = activeTeams.Aggregate("Active$$", (current, team) => current + (team["ID"] + "$%"));
            s2 = s2.Substring(0, s2.Length - 2);
            s2 += "\n";

            var stg = s1 + s2;

            SaveFileDialog sfd = null;
            Tools.AppInvoke(
                () =>
                    {
                        sfd = new SaveFileDialog
                            {
                                Title = "Save Active Teams List",
                                Filter = "Active Teams List (*.red)|*.red",
                                DefaultExt = "red",
                                InitialDirectory = App.AppDocsPath
                            };
                        sfd.ShowDialog();
                    });

            if (String.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            var sw = new StreamWriter(sfd.FileName);
            sw.Write(stg);
            sw.Close();
        }

        public static async Task ImportOldPlayerCareerStats(
            Dictionary<int, PlayerStats> pst, Dictionary<int, TeamStats> tst, string folder)
        {
            if (tst.Count != 30)
            {
                MessageBox.Show(
                    "Can't import previous player stats to a database that doesn't have 30 teams. Please import "
                    + "your NBA 2K save once to this database to populate it properly before trying to import previous "
                    + "player stats.");
                MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-2);
                return;
            }

            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            NBA2KVersion nba2KVersion;
            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats, out nba2KVersion) == -1)
            {
                MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-1);
                return;
            }

            var list = new List<string> { "1 season ago" };
            for (var i = 2; i <= 20; i++)
            {
                list.Add(i + " seasons ago");
            }

            var ccw = new ComboChoiceWindow("Add player stats starting from...", list);
            if (ccw.ShowDialog() != true)
            {
                MainWindow.MWInstance.mainGrid.Visibility = Visibility.Visible;
                return;
            }

            var startAt = Convert.ToInt32(ComboChoiceWindow.UserChoice.Split(' ')[0]);

            var seasonNames = new Dictionary<int, string>();
            if (nba2KVersion == NBA2KVersion.NBA2K12)
            {
                while (true)
                {
                    var ibw =
                        new InputBoxWindow(
                            "Enter the season that describes the first season in this database (e.g. 2011-2012 by default in NBA 2K12, 2010-2011 if you "
                            + "imported last year's stats first, 2012 for a season taking place only in that year, etc.):",
                            "2011-2012");
                    if (ibw.ShowDialog() != true)
                    {
                        MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(-2);
                        return;
                    }

                    int year;
                    var twoPartSeasonDesc = InputBoxWindow.UserInput.Contains("-");
                    try
                    {
                        year = Convert.ToInt32(twoPartSeasonDesc ? InputBoxWindow.UserInput.Split('-')[0] : InputBoxWindow.UserInput);
                    }
                    catch
                    {
                        MessageBox.Show(
                            "The year you entered (" + InputBoxWindow.UserInput
                            + ") was not in a valid format.\nValid formats are:\n\t2012\n\t2011-2012");
                        continue;
                    }

                    for (var i = startAt; i <= 20; i++)
                    {
                        seasonNames.Add(
                            i, twoPartSeasonDesc ? string.Format("{0}-{1}", year - i, (year - i + 1)) : (year - i).ToString());
                    }
                    break;
                }
            }

            initializeLegalTeamTypes(nba2KVersion);

            var validTeams = teams.FindAll(
                delegate(Dictionary<string, string> team)
                    {
                        if (legalTTypes.IndexOf(team["TType"]) != -1)
                        {
                            return true;
                        }
                        return false;
                    });

            var activeTeams = validTeams.FindAll(
                delegate(Dictionary<string, string> team)
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
                    createSettingsFile(activeTeams, folder);
                }
            }

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            foreach (var team in activeTeams)
            {
                var id = -1;
                var name = team["Name"];
                if (tst.All(pair => pair.Value.Name != name))
                {
                    for (var i = 0; i < 30; i++)
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
                for (var i = 6; i <= 12; i++)
                {
                    var cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
                for (var i = 13; i <= 20; i++)
                {
                    var cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
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

            var validPlayers = players.FindAll(player => isValidPlayer(player, nba2KVersion));

            var endAt = nba2KVersion >= NBA2KVersion.NBA2K13 ? 19 : 20;

            var pw = new ProgressWindow("Please wait as player career stats are being imported...");
            pw.Show();

            await Task.Run(
                () =>
                    {
                        var count = validPlayers.Count;
                        var ppsList = new List<PastPlayerStats>();
                        for (var i = 0; i < count; i++)
                        {
                            var percentProgress = i * 100 / count;
                            if (percentProgress % 5 == 0)
                            {
                                Tools.AppInvoke(() => pw.SetProgressBarValue(percentProgress));
                            }
                            var player = validPlayers[i];
                            var playerID = Convert.ToInt32(player["ID"]);

                            var lastName = player["Last_Name"];
                            var firstName = player["First_Name"];

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
                                if (pst.ContainsKey(playerID) && pst[playerID].LastName == lastName
                                    && pst[playerID].FirstName == firstName)
                                {
                                    pst[playerID].IsSigned = false;
                                    pst[playerID].TeamF = -1;
                                    pst[playerID].IsHidden = true;
                                }
                                continue;
                            }

                            #region Match Player

                            if (pst.ContainsKey(playerID)
                                && (pst[playerID].LastName != lastName || pst[playerID].FirstName != firstName))
                            {
                                var candidates =
                                    pst.Where(
                                        pair =>
                                        pair.Value.LastName == lastName && pair.Value.FirstName == firstName
                                        && pair.Value.IsHidden == false).ToList();
                                if (candidates.Count > 0)
                                {
                                    var found = false;
                                    var c2 = candidates.Where(pair => tst.ContainsKey(pair.Value.TeamF)).ToList();
                                    if (c2.Count() == 1)
                                    {
                                        playerID = c2.First().Value.ID;
                                        found = true;
                                    }
                                    else
                                    {
                                        var c4 =
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
                                                var c3 = candidates.Where(pair => pair.Value.TeamF == curTeam.ID).ToList();
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
                                            var choice = String.Format(
                                                "{0}: {1} {2} (Born {3}",
                                                pair.Value.ID,
                                                pair.Value.FirstName,
                                                pair.Value.LastName,
                                                pair.Value.YearOfBirth);
                                            if (pair.Value.IsSigned)
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
                                        var message = String.Format(
                                            "{0}: {1} {2} (Born {3}",
                                            player["ID"],
                                            player["First_Name"],
                                            player["Last_Name"],
                                            player["BirthYear"]);
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

                            #endregion Match Player

                            var qr = "SELECT * FROM PastPlayerStats WHERE PlayerID = " + playerID + " ORDER BY \"SOrder\"";
                            var dt = MainWindow.DB.GetDataTable(qr);
                            dt.Rows.Cast<DataRow>().ToList().ForEach(dr => ppsList.Add(new PastPlayerStats(dr)));
                            for (var j = startAt; j < endAt; j++)
                            {
                                var statEntryID = player["StatY" + j];
                                if (statEntryID == "-1")
                                {
                                    continue;
                                }
                                var stats = playerStats.Single(d => d["ID"] == statEntryID);
                                var prevStats = new PastPlayerStats();
                                var teamID2 = stats["TeamID2"];
                                var teamID1 = stats["TeamID1"];
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
                                prevStats.GP = Convert.ToUInt16(stats["GamesP"]);
                                prevStats.GS = Convert.ToUInt16(stats["GamesS"]);
                                prevStats.MINS = Convert.ToUInt16(stats["Minutes"]);
                                prevStats.PTS = Convert.ToUInt16(stats["Points"]);
                                prevStats.DREB = Convert.ToUInt16(stats["DRebs"]);
                                prevStats.OREB = Convert.ToUInt16(stats["ORebs"]);
                                prevStats.AST = Convert.ToUInt16(stats["Assists"]);
                                prevStats.STL = Convert.ToUInt16(stats["Steals"]);
                                prevStats.BLK = Convert.ToUInt16(stats["Blocks"]);
                                prevStats.TOS = Convert.ToUInt16(stats["TOs"]);
                                prevStats.FOUL = Convert.ToUInt16(stats["Fouls"]);
                                prevStats.FGM = Convert.ToUInt16(stats["FGMade"]);
                                prevStats.FGA = Convert.ToUInt16(stats["FGAtt"]);
                                try
                                {
                                    prevStats.TPM = Convert.ToUInt16(stats["3PTMade"]);
                                    prevStats.TPA = Convert.ToUInt16(stats["3PTAtt"]);
                                }
                                catch (KeyNotFoundException)
                                {
                                    prevStats.TPM = Convert.ToUInt16(stats["TPTMade"]);
                                    prevStats.TPA = Convert.ToUInt16(stats["TPTAtt"]);
                                }
                                prevStats.FTM = Convert.ToUInt16(stats["FTMade"]);
                                prevStats.FTA = Convert.ToUInt16(stats["FTAtt"]);
                                prevStats.PlayerID = playerID;
                                var yearF = 0;
                                if (nba2KVersion != NBA2KVersion.NBA2K12)
                                {
                                    yearF = Convert.ToInt32(stats["Year"]);
                                }
                                prevStats.SeasonName = nba2KVersion == NBA2KVersion.NBA2K12
                                                           ? seasonNames[j]
                                                           : String.Format("{0}-{1}", yearF - 1, yearF);
                                prevStats.IsPlayoff = false;
                                prevStats.Order = nba2KVersion == NBA2KVersion.NBA2K12 ? 20 - j : yearF;
                                prevStats.EndEdit();
                                ppsList.Add(prevStats);
                            }
                        }
                        Tools.AppInvoke(
                            () =>
                                {
                                    pw.SetProgressBarValue(99);
                                    pw.SetMessage("Please wait while the player career stats are being saved...");
                                });
                        SQLiteIO.SavePastPlayerStatsToDatabase(MainWindow.DB, ppsList);
                    });

            pw.CanClose = true;
            pw.Close();
            MainWindow.MWInstance.OnImportOldPlayerStatsCompleted(0);
        }

        private static bool isValidPlayer(Dictionary<string, string> player, NBA2KVersion nba2KVersion)
        {
            if ((player["IsFA"] == "0" && player["TeamID1"] != "-1") || (player["IsFA"] == "1"))
            {
                if (nba2KVersion == NBA2KVersion.NBA2K12)
                {
                    if (player["PlType"] == "4" || player["PlType"] == "5" || player["PlType"] == "6")
                    {
                        return true;
                    }
                }
                else if (nba2KVersion == NBA2KVersion.NBA2K13)
                {
                    if (player["IsRegNBA"] == "1" || player["IsSpecial"] == "1")
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Assert(nba2KVersion == NBA2KVersion.NBA2K14);
                    if (player["IsRegular"] == "1")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>Imports all team (and optionally) player stats from an REDitor-exported set of CSV files.</summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="folder">The folder containing the exported CSV files.</param>
        /// <param name="teamsOnly">
        ///     if set to <c>true</c>, only team stats will be imported.
        /// </param>
        /// <returns></returns>
        public static int ImportCurrentYear(
            ref Dictionary<int, TeamStats> tst,
            ref Dictionary<int, TeamStats> tstOpp,
            ref Dictionary<int, PlayerStats> pst,
            string folder,
            bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            NBA2KVersion nba2KVersion;
            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats, out nba2KVersion) == -1)
            {
                return -1;
            }

            initializeLegalTeamTypes(nba2KVersion);

            var importMessages = new List<string>();
            var tradesList = new List<string>();
            var faSigningsList = new List<string>();
            var reSigningsList = new List<string>();
            var waiversList = new List<string>();
            var injuredList = new List<string>();
            var reInjuredList = new List<string>();
            var recoveredList = new List<string>();
            var newRecordsList = new List<string>();

            #region Import Teams & Team Stats

            var validTeams = teams.FindAll(
                delegate(Dictionary<string, string> team)
                    {
                        if (legalTTypes.IndexOf(team["TType"]) != -1)
                        {
                            return true;
                        }
                        return false;
                    });

            var activeTeams = validTeams.FindAll(
                delegate(Dictionary<string, string> team)
                    {
                        if (team["StatCurS"] != "-1")
                        {
                            return true;
                        }
                        return false;
                    }).ToList();
            if (activeTeams.Count < 30)
            {
                bool? dialogResult = null;
                Tools.AppInvoke(
                    () =>
                        {
                            var dlw = new DualListWindow(validTeams, activeTeams);
                            dialogResult = dlw.ShowDialog();
                        });
                if (dialogResult != true)
                {
                    return -1;
                }
                activeTeams = new List<Dictionary<string, string>>(MainWindow.SelectedTeams);

                if (MainWindow.SelectedTeamsChanged)
                {
                    createSettingsFile(activeTeams, folder);
                }
            }

            var madeNew = false;

            if (tst.Count != activeTeams.Count)
            {
                tst = new Dictionary<int, TeamStats>();
                tstOpp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }

            var oldTST = tst.ToDictionary(ts => ts.Key, ts => ts.Value.BinarySerializationClone());
            var oldtstOpp = tstOpp.ToDictionary(ts => ts.Key, ts => ts.Value.BinarySerializationClone());
            var oldPST = pst.ToDictionary(ps => ps.Key, ps => ps.Value.BinarySerializationClone());

            CreateDivisions();

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            activeTeams.Sort((p1, p2) => Convert.ToInt32(p1["ID"]).CompareTo(Convert.ToInt32(p2["ID"])));
            foreach (var team in activeTeams)
            {
                var name = team["Name"];
                if (nba2KVersion != NBA2KVersion.NBA2K12)
                {
                    name += (team["Year"] == "0" ? "" : team["Year"].PadLeft(2, '0'));
                }
                var redID = Convert.ToInt32(team["ID"]);
                if (tst.Values.All(ts => ts.Name != name))
                {
                    if (tst.Keys.Contains(redID))
                    {
                        var oldName = tst[redID].Name;
                        tst[redID].Name = name;
                        tstOpp[redID].Name = name;
                        if (oldName == tst[redID].DisplayName)
                        {
                            tst[redID].DisplayName = name;
                            tstOpp[redID].DisplayName = name;
                        }
                    }
                    else
                    {
                        tst.Add(redID, new TeamStats(redID, name));
                        tstOpp.Add(redID, new TeamStats(redID, name));
                    }
                }
                var id = tst.Values.Single(ts => ts.Name == name).ID;
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                if (madeNew)
                {
                    tst[id] = new TeamStats(id, name);
                    tstOpp[id] = new TeamStats(id, name);
                }

                var sStatsID = Convert.ToInt32(team["StatCurS"]);
                var pStatsID = Convert.ToInt32(team["StatCurP"]);

                var sStats = teamStats.Find(
                    delegate(Dictionary<string, string> s)
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
                    tst[id].Totals[TAbbrT.MINS] = Convert.ToUInt16(sStats["Mins"]);
                    tst[id].Totals[TAbbrT.PF] = Convert.ToUInt16(sStats["PtsFor"]);
                    tst[id].Totals[TAbbrT.PA] = Convert.ToUInt16(sStats["PtsAg"]);
                    tst[id].Totals[TAbbrT.FGM] = Convert.ToUInt16(sStats["FGMade"]);
                    tst[id].Totals[TAbbrT.FGA] = Convert.ToUInt16(sStats["FGAtt"]);
                    try
                    {
                        tst[id].Totals[TAbbrT.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                        tst[id].Totals[TAbbrT.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                    }
                    catch (KeyNotFoundException)
                    {
                        tst[id].Totals[TAbbrT.TPM] = Convert.ToUInt16(sStats["TPTMade"]);
                        tst[id].Totals[TAbbrT.TPA] = Convert.ToUInt16(sStats["TPTAtt"]);
                    }
                    tst[id].Totals[TAbbrT.FTM] = Convert.ToUInt16(sStats["FTMade"]);
                    tst[id].Totals[TAbbrT.FTA] = Convert.ToUInt16(sStats["FTAtt"]);
                    tst[id].Totals[TAbbrT.DREB] = Convert.ToUInt16(sStats["DRebs"]);
                    tst[id].Totals[TAbbrT.OREB] = Convert.ToUInt16(sStats["ORebs"]);
                    tst[id].Totals[TAbbrT.STL] = Convert.ToUInt16(sStats["Steals"]);
                    tst[id].Totals[TAbbrT.BLK] = Convert.ToUInt16(sStats["Blocks"]);
                    tst[id].Totals[TAbbrT.AST] = Convert.ToUInt16(sStats["Assists"]);
                    tst[id].Totals[TAbbrT.FOUL] = Convert.ToUInt16(sStats["Fouls"]);
                    tst[id].Totals[TAbbrT.TOS] = Convert.ToUInt16(sStats["TOs"]);

                    if (pStatsID != -1)
                    {
                        var pStats = teamStats.Single(s => s["ID"] == pStatsID.ToString());
                        tst[id].PlRecord[0] = Convert.ToByte(pStats["Wins"]);
                        tst[id].PlRecord[1] = Convert.ToByte(pStats["Losses"]);
                        tst[id].PlTotals[TAbbrT.MINS] = Convert.ToUInt16(pStats["Mins"]);
                        tst[id].PlTotals[TAbbrT.PF] = Convert.ToUInt16(pStats["PtsFor"]);
                        tst[id].PlTotals[TAbbrT.PA] = Convert.ToUInt16(pStats["PtsAg"]);
                        tst[id].PlTotals[TAbbrT.FGM] = Convert.ToUInt16(pStats["FGMade"]);
                        tst[id].PlTotals[TAbbrT.FGA] = Convert.ToUInt16(pStats["FGAtt"]);
                        try
                        {
                            tst[id].PlTotals[TAbbrT.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                            tst[id].PlTotals[TAbbrT.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                        }
                        catch (KeyNotFoundException)
                        {
                            tst[id].PlTotals[TAbbrT.TPM] = Convert.ToUInt16(sStats["TPTMade"]);
                            tst[id].PlTotals[TAbbrT.TPA] = Convert.ToUInt16(sStats["TPTAtt"]);
                        }
                        tst[id].PlTotals[TAbbrT.FTM] = Convert.ToUInt16(pStats["FTMade"]);
                        tst[id].PlTotals[TAbbrT.FTA] = Convert.ToUInt16(pStats["FTAtt"]);
                        tst[id].PlTotals[TAbbrT.DREB] = Convert.ToUInt16(pStats["DRebs"]);
                        tst[id].PlTotals[TAbbrT.OREB] = Convert.ToUInt16(pStats["ORebs"]);
                        tst[id].PlTotals[TAbbrT.STL] = Convert.ToUInt16(pStats["Steals"]);
                        tst[id].PlTotals[TAbbrT.BLK] = Convert.ToUInt16(pStats["Blocks"]);
                        tst[id].PlTotals[TAbbrT.AST] = Convert.ToUInt16(pStats["Assists"]);
                        tst[id].PlTotals[TAbbrT.FOUL] = Convert.ToUInt16(pStats["Fouls"]);
                        tst[id].PlTotals[TAbbrT.TOS] = Convert.ToUInt16(pStats["TOs"]);
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
                for (var i = 6; i <= 12; i++)
                {
                    var cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
                for (var i = 13; i <= 20; i++)
                {
                    var cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
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
                var validPlayers = players.FindAll(player => isValidPlayer(player, nba2KVersion));

                foreach (var player in validPlayers)
                {
                    var playerID = Convert.ToInt32(player["ID"]);

                    var lastName = player["Last_Name"];
                    var firstName = player["First_Name"];

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
                            pst[playerID].IsSigned = false;
                            pst[playerID].TeamF = -1;
                            pst[playerID].IsHidden = true;
                        }
                        continue;
                    }

                    var playerStatsID = Convert.ToInt32(player["StatY0"]);

                    Dictionary<string, string> playerSeasonStats;
                    try
                    {
                        playerSeasonStats = playerStats.Single(s => s["ID"] == playerStatsID.ToString());
                    }
                    catch
                    {
                        playerSeasonStats = null;
                    }

                    Dictionary<string, string> playerPlayoffStats = null;
                    if (nba2KVersion >= NBA2KVersion.NBA2K13)
                    {
                        var playerPlayoffStatsID = player["StatPOs"];
                        try
                        {
                            playerPlayoffStats = playerStats.Find(s => s["ID"] == playerPlayoffStatsID);
                        }
                        catch
                        {
                            playerPlayoffStats = null;
                        }
                    }

                    #region Match Player

                    if (pst.ContainsKey(playerID) && (pst[playerID].LastName != lastName || pst[playerID].FirstName != firstName))
                    {
                        var candidates =
                            pst.Where(
                                pair =>
                                pair.Value.LastName == lastName && pair.Value.FirstName == firstName && pair.Value.IsHidden == false)
                               .ToList();
                        if (candidates.Count > 0)
                        {
                            var found = false;
                            var temptst = tst;
                            var c2 = candidates.Where(pair => temptst.ContainsKey(pair.Value.TeamF)).ToList();
                            if (c2.Count() == 1)
                            {
                                playerID = c2.First().Value.ID;
                                found = true;
                            }
                            else
                            {
                                var c4 = candidates.Where(pair => pair.Value.YearOfBirth.ToString() == player["BirthYear"]).ToList();
                                if (c4.Count == 1)
                                {
                                    playerID = c4.First().Value.ID;
                                    found = true;
                                }
                                else
                                {
                                    if (pTeam != -1)
                                    {
                                        var c3 = candidates.Where(pair => pair.Value.TeamF == curTeam.ID).ToList();
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
                                    var choice = String.Format(
                                        "{0}: {1} {2} (Born {3}",
                                        pair.Value.ID,
                                        pair.Value.FirstName,
                                        pair.Value.LastName,
                                        pair.Value.YearOfBirth);
                                    if (pair.Value.IsSigned)
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
                                var message = String.Format(
                                    "{0}: {1} {2} (Born {3}",
                                    player["ID"],
                                    player["First_Name"],
                                    player["Last_Name"],
                                    player["BirthYear"]);
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

                    #endregion Match Player

                    var curPlayer = pst[playerID];
                    var oldPlayer = curPlayer.BinarySerializationClone();

                    curPlayer.Position1 = (Position) Enum.Parse(typeof(Position), player["Pos"]);
                    curPlayer.Position2 = (Position) Enum.Parse(typeof(Position), player["SecondPos"]);
                    curPlayer.IsHidden = false;
                    curPlayer.YearsPro = Convert.ToInt32(player["YearsPro"]);
                    curPlayer.YearOfBirth = Convert.ToInt32(player["BirthYear"]);
                    curPlayer.Contract.Option = (PlayerContractOption) Enum.Parse(typeof(PlayerContractOption), player["COption"]);
                    curPlayer.Contract.ContractSalaryPerYear.Clear();
                    for (var i = 1; i < 7; i++)
                    {
                        var salary = Convert.ToInt32(player["CYear" + i]);
                        if (salary == 0)
                        {
                            break;
                        }

                        curPlayer.Contract.ContractSalaryPerYear.Add(salary);
                    }
                    curPlayer.Height = Convert.ToDouble(player["Height"]);
                    curPlayer.Weight = Convert.ToDouble(player["Weight"]);
                    curPlayer.Injury = new PlayerInjury(Convert.ToInt32(player["InjType"]), Convert.ToInt32(player["InjDaysLeft"]));

                    if (playerSeasonStats != null)
                    {
                        var teamReal = pTeam.ToString();
                        var team2 = playerSeasonStats["TeamID1"];
                        //bool hasBeenTraded = (team1 != "-1");

                        curPlayer.TeamF = pTeam;
                        curPlayer.TeamS = Convert.ToInt32(team2);

                        curPlayer.IsSigned = (player["IsFA"] != "1" && teamReal != "-1");

                        importPlayerStats(curPlayer, playerSeasonStats, nba2KVersion, false);

                        curPlayer.CalcAvg();

                        pst[playerID] = curPlayer;
                    }
                    else
                    {
                        curPlayer.TeamF = pTeam;

                        curPlayer.IsSigned = player["IsFA"] != "1";

                        curPlayer.CalcAvg();
                    }

                    if (playerPlayoffStats != null)
                    {
                        importPlayerStats(curPlayer, playerPlayoffStats, nba2KVersion, true);

                        curPlayer.CalcAvg();
                    }

                    if (nba2KVersion >= NBA2KVersion.NBA2K13)
                    {
                        importPlayerCareerHighs(curPlayer, player);
                    }

                    #region Import Messsages

                    var name = String.Format("{0} {1}", curPlayer.FirstName, curPlayer.LastName);
                    if (oldPlayer.TeamF != curPlayer.TeamF)
                    {
                        string msg;
                        if (curPlayer.IsSigned && oldPlayer.IsSigned)
                        {
                            msg = String.Format(
                                "{0} was traded from the {1} to the {2}.",
                                name,
                                tst[oldPlayer.TeamF].DisplayName,
                                tst[curPlayer.TeamF].DisplayName);
                            tradesList.Add(msg);
                        }
                        else if (oldPlayer.IsSigned)
                        {
                            msg = String.Format("{0} was released from the {1}.", name, tst[oldPlayer.TeamF].DisplayName);
                            waiversList.Add(msg);
                        }
                    }

                    if (oldPlayer.Contract.GetYears() < curPlayer.Contract.GetYears() && curPlayer.IsSigned)
                    {
                        var msg = name;
                        bool reSigned;
                        if (!oldPlayer.IsSigned && curPlayer.IsSigned)
                        {
                            reSigned = false;
                            msg += " signed ";
                        }
                        else
                        {
                            reSigned = true;
                            msg += " re-signed ";
                        }
                        msg += String.Format(
                            "with the {0} on a {1}/{2:C0} ({3:C0} per year) contract.",
                            tst[curPlayer.TeamF].DisplayName,
                            curPlayer.Contract.GetYearsDesc(),
                            curPlayer.Contract.GetTotal(),
                            curPlayer.Contract.GetAverage());
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
                            injuredList.Add(
                                string.Format(
                                    "{0} ({1}) got injured. Status: {2}",
                                    name,
                                    curPlayer.TeamF != -1 ? tst[curPlayer.TeamF].DisplayName : "Free Agent",
                                    curPlayer.Injury.Status));
                        }
                        else if (!curPlayer.Injury.IsInjured)
                        {
                            recoveredList.Add(
                                string.Format(
                                    "{1} ({0}) is no longer injured.",
                                    curPlayer.TeamF != -1 ? tst[curPlayer.TeamF].DisplayName : "Free Agent",
                                    name));
                        }
                        else
                        {
                            reInjuredList.Add(
                                string.Format(
                                    "{0} ({3}) was being reported as having {1}, is now reported as: {2}",
                                    name,
                                    oldPlayer.Injury.InjuryName,
                                    curPlayer.Injury.Status,
                                    curPlayer.TeamF != -1 ? tst[curPlayer.TeamF].DisplayName : "Free Agent"));
                        }
                    }

                    var count = 0;
                    var recMsg = "";
                    for (var i = 0; i < curPlayer.CareerHighs.Count(); i++)
                    {
                        if (i == PAbbrT.MINS || i == PAbbrT.TOS || i == PAbbrT.FOUL)
                        {
                            continue;
                        }
                        if (oldPlayer.CareerHighs[i] != 0 && oldPlayer.CareerHighs[i] < curPlayer.CareerHighs[i])
                        {
                            if (count == 0)
                            {
                                recMsg = string.Format(
                                    "{0} has a new career-high in {1} ({2})",
                                    curPlayer.FullInfo(tst, true, false),
                                    PlayerStatsHelper.CHTotals[i],
                                    curPlayer.CareerHighs[i]);
                            }
                            else
                            {
                                recMsg += string.Format(", {0} ({1})", PlayerStatsHelper.CHTotals[i], curPlayer.CareerHighs[i]);
                            }
                            count++;
                        }
                    }
                    recMsg += ".";
                    if (count > 0)
                    {
                        newRecordsList.Add(recMsg);
                    }

                    #endregion
                }
            }

            if (duplicatePlayers.Count > 0)
            {
                var msg =
                    "The following names belong to two or more players in the database and the tool couldn't determine who to import to:\n\n";
                duplicatePlayers.ForEach(item => msg += item + ", ");
                msg = msg.TrimEnd(new[] { ' ', ',' });
                msg += "\n\nImport will continue, but there will be some stats missing." + "\n\nTo avoid this problem, either\n"
                       + "1) disable the duplicate occurences via (Miscellaneous > Enable/Disable Players For This Season...), or\n"
                       + "2) transfer the correct instance of the player to their current team.";
                Tools.AppInvoke(() => MessageBox.Show(msg));
            }

            #endregion

            #region Check for box-scores we can calculate

            if (oldTST.Count == 30)
            {
                TeamsThatPlayedAGame = new List<int>();
                foreach (var team in tst)
                {
                    var newTeam = team.Value;
                    var teamID = team.Key;
                    var oldTeam = oldTST[teamID];

                    if (oldTeam.GetGames() + 1 == newTeam.GetGames() || oldTeam.GetPlayoffGames() + 1 == newTeam.GetPlayoffGames())
                    {
                        TeamsThatPlayedAGame.Add(team.Key);
                    }
                }

                if (TeamsThatPlayedAGame.Count >= 2)
                {
                    bool? result = null;

                    PickedTeams = new List<int>();
                    Tools.AppInvoke(
                        () =>
                            {
                                var dlw = new PickGamesWindow(TeamsThatPlayedAGame);
                                result = dlw.ShowDialog();
                            });

                    if (result == true)
                    {
                        for (var i = 0; i <= PickedTeams.Count - 2; i += 2)
                        {
                            var t1 = PickedTeams[i];
                            var t2 = PickedTeams[i + 1];

                            var bse = prepareBoxScore(tst, oldTST, pst, oldPST, t1, t2);

                            var teamBoxScore = bse.BS;
                            BoxScoreWindow.CalculateTeamsFromPlayers(
                                ref teamBoxScore,
                                bse.PBSList.Where(pbs => pbs.TeamID == bse.BS.Team1ID),
                                bse.PBSList.Where(pbs => pbs.TeamID == bse.BS.Team2ID));

                            if (teamBoxScore.PTS1 != getDiff(tst, oldTST, t1, TAbbrT.PF, teamBoxScore.IsPlayoff)
                                || teamBoxScore.PTS2 != getDiff(tst, oldTST, t2, TAbbrT.PF, teamBoxScore.IsPlayoff))
                            {
                                var localtst = tst;
                                Tools.AppInvoke(
                                    () =>
                                    MessageBox.Show(
                                        String.Format(
                                            "{0} @ {1} won't have its box-score imported because it couldn't be properly calculated. A possible reason for this is that one or more players participating in that game has been since traded away from the teams.",
                                            localtst[t1].DisplayName,
                                            localtst[t2].DisplayName),
                                        "NBA Stats Tracker",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Exclamation));
                                continue;
                            }

                            bse.BS.GameDate = SelectedDate;
                            TeamStats.AddTeamStatsFromBoxScore(bse.BS, ref oldTST, ref oldtstOpp, t1, t2);
                            MainWindow.BSHist.Add(bse);
                            tst[t1] = oldTST[t1].BinarySerializationClone();
                            tst[t2] = oldTST[t2].BinarySerializationClone();
                            tstOpp[t1] = oldtstOpp[t1].BinarySerializationClone();
                            tstOpp[t2] = oldtstOpp[t2].BinarySerializationClone();
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
                    importMessages.Add("Players whose injury status changed");
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
                importMessages.Add("");
                importMessages.Add("");
            }
            if (newRecordsList.Count > 0)
            {
                importMessages.Add("New Career Highs");
                importMessages.Add("========================================");
                importMessages.Add("");
                importMessages.AddRange(newRecordsList);
                importMessages.Add("");
            }

            if (importMessages.Count > 0)
            {
                importMessages.Add("");
                Tools.AppInvoke(
                    () =>
                        {
                            var cmw = new CopyableMessageWindow(importMessages.Aggregate((m1, m2) => m1 + "\n" + m2), "League News");
                            cmw.ShowDialog();
                        });
            }

            return 0;
        }

        private static void initializeLegalTeamTypes(NBA2KVersion nba2KVersion)
        {
            switch (nba2KVersion)
            {
                case NBA2KVersion.NBA2K12:
                    legalTTypes = new List<string> { "0", "4" };
                    break;
                case NBA2KVersion.NBA2K13:
                    legalTTypes = new List<string> { "0", "21" };
                    break;
                case NBA2KVersion.NBA2K14:
                    legalTTypes = new List<string> { "0", "22" };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int ImportLastYear(
            ref Dictionary<int, TeamStats> tst,
            ref Dictionary<int, TeamStats> tstOpp,
            ref Dictionary<int, PlayerStats> pst,
            string folder,
            bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            NBA2KVersion nba2KVersion;
            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats, out nba2KVersion) == -1)
            {
                return -1;
            }

            #region Import Teams & Team Stats

            initializeLegalTeamTypes(nba2KVersion);

            var validTeams = teams.FindAll(
                delegate(Dictionary<string, string> team)
                    {
                        if (legalTTypes.IndexOf(team["TType"]) != -1)
                        {
                            return true;
                        }
                        return false;
                    });

            var activeTeams = validTeams.FindAll(
                delegate(Dictionary<string, string> team)
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
                    createSettingsFile(activeTeams, folder);
                }
            }

            var madeNew = false;

            if (tst.Count != activeTeams.Count)
            {
                tst = new Dictionary<int, TeamStats>();
                tstOpp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }

            CreateDivisions();

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            foreach (var team in activeTeams)
            {
                var name = team["Name"];
                if (nba2KVersion != NBA2KVersion.NBA2K12)
                {
                    name += (team["Year"] == "0" ? "" : team["Year"].PadLeft(2, '0'));
                }
                var redID = Convert.ToInt32(team["ID"]);
                if (tst.Values.All(ts => ts.Name != name))
                {
                    if (tst.Keys.Contains(redID))
                    {
                        var oldName = tst[redID].Name;
                        tst[redID].Name = name;
                        tstOpp[redID].Name = name;
                        if (oldName == tst[redID].DisplayName)
                        {
                            tst[redID].DisplayName = name;
                            tstOpp[redID].DisplayName = name;
                        }
                    }
                    else
                    {
                        tst.Add(redID, new TeamStats(redID, name));
                        tstOpp.Add(redID, new TeamStats(redID, name));
                    }
                }
                var id = tst.Values.Single(ts => ts.Name == name).ID;
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                if (madeNew)
                {
                    tst[id] = new TeamStats(id, name);
                    tstOpp[id] = new TeamStats(id, name);
                }

                var sStatsID = Convert.ToInt32(team["StatPrevS"]);
                var pStatsID = Convert.ToInt32(team["StatPrevP"]);

                var sStats = teamStats.Find(
                    delegate(Dictionary<string, string> s)
                        {
                            if (s["ID"] == sStatsID.ToString())
                            {
                                return true;
                            }
                            return false;
                        });

                var curTeam = tst[id];
                curTeam.ID = Convert.ToInt32(team["ID"]);
                curTeam.Division = Convert.ToInt32(team["Division"]);
                tstOpp[id].Division = Convert.ToInt32(team["Division"]);

                if (sStats != null)
                {
                    curTeam.Record[0] = Convert.ToByte(sStats["Wins"]);
                    curTeam.Record[1] = Convert.ToByte(sStats["Losses"]);
                    curTeam.Totals[TAbbrT.MINS] = Convert.ToUInt16(sStats["Mins"]);
                    curTeam.Totals[TAbbrT.PF] = Convert.ToUInt16(sStats["PtsFor"]);
                    curTeam.Totals[TAbbrT.PA] = Convert.ToUInt16(sStats["PtsAg"]);
                    curTeam.Totals[TAbbrT.FGM] = Convert.ToUInt16(sStats["FGMade"]);
                    curTeam.Totals[TAbbrT.FGA] = Convert.ToUInt16(sStats["FGAtt"]);
                    try
                    {
                        curTeam.Totals[TAbbrT.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                        curTeam.Totals[TAbbrT.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                    }
                    catch (KeyNotFoundException)
                    {
                        curTeam.Totals[TAbbrT.TPM] = Convert.ToUInt16(sStats["TPTMade"]);
                        curTeam.Totals[TAbbrT.TPA] = Convert.ToUInt16(sStats["TPTAtt"]);
                    }
                    curTeam.Totals[TAbbrT.FTM] = Convert.ToUInt16(sStats["FTMade"]);
                    curTeam.Totals[TAbbrT.FTA] = Convert.ToUInt16(sStats["FTAtt"]);
                    curTeam.Totals[TAbbrT.DREB] = Convert.ToUInt16(sStats["DRebs"]);
                    curTeam.Totals[TAbbrT.OREB] = Convert.ToUInt16(sStats["ORebs"]);
                    curTeam.Totals[TAbbrT.STL] = Convert.ToUInt16(sStats["Steals"]);
                    curTeam.Totals[TAbbrT.BLK] = Convert.ToUInt16(sStats["Blocks"]);
                    curTeam.Totals[TAbbrT.AST] = Convert.ToUInt16(sStats["Assists"]);
                    curTeam.Totals[TAbbrT.FOUL] = Convert.ToUInt16(sStats["Fouls"]);
                    curTeam.Totals[TAbbrT.TOS] = Convert.ToUInt16(sStats["TOs"]);
                    //tstOpp[id].stats[t.TO] = Convert.ToUInt16(sStats["TOsAg"]);

                    if (pStatsID != -1)
                    {
                        var pStats = teamStats.Find(
                            delegate(Dictionary<string, string> s)
                                {
                                    if (s["ID"] == pStatsID.ToString())
                                    {
                                        return true;
                                    }
                                    return false;
                                });
                        curTeam.PlRecord[0] = Convert.ToByte(pStats["Wins"]);
                        curTeam.PlRecord[1] = Convert.ToByte(pStats["Losses"]);
                        curTeam.PlTotals[TAbbrT.MINS] = Convert.ToUInt16(pStats["Mins"]);
                        curTeam.PlTotals[TAbbrT.PF] = Convert.ToUInt16(pStats["PtsFor"]);
                        curTeam.PlTotals[TAbbrT.PA] = Convert.ToUInt16(pStats["PtsAg"]);
                        curTeam.PlTotals[TAbbrT.FGM] = Convert.ToUInt16(pStats["FGMade"]);
                        curTeam.PlTotals[TAbbrT.FGA] = Convert.ToUInt16(pStats["FGAtt"]);
                        try
                        {
                            curTeam.PlTotals[TAbbrT.TPM] = Convert.ToUInt16(pStats["3PTMade"]);
                            curTeam.PlTotals[TAbbrT.TPA] = Convert.ToUInt16(pStats["3PTAtt"]);
                        }
                        catch (KeyNotFoundException)
                        {
                            curTeam.PlTotals[TAbbrT.TPM] = Convert.ToUInt16(pStats["TPTMade"]);
                            curTeam.PlTotals[TAbbrT.TPA] = Convert.ToUInt16(pStats["TPTAtt"]);
                        }
                        curTeam.PlTotals[TAbbrT.FTM] = Convert.ToUInt16(pStats["FTMade"]);
                        curTeam.PlTotals[TAbbrT.FTA] = Convert.ToUInt16(pStats["FTAtt"]);
                        curTeam.PlTotals[TAbbrT.DREB] = Convert.ToUInt16(pStats["DRebs"]);
                        curTeam.PlTotals[TAbbrT.OREB] = Convert.ToUInt16(pStats["ORebs"]);
                        curTeam.PlTotals[TAbbrT.STL] = Convert.ToUInt16(pStats["Steals"]);
                        curTeam.PlTotals[TAbbrT.BLK] = Convert.ToUInt16(pStats["Blocks"]);
                        curTeam.PlTotals[TAbbrT.AST] = Convert.ToUInt16(pStats["Assists"]);
                        curTeam.PlTotals[TAbbrT.FOUL] = Convert.ToUInt16(pStats["Fouls"]);
                        curTeam.PlTotals[TAbbrT.TOS] = Convert.ToUInt16(pStats["TOs"]);
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
                for (var i = 6; i <= 12; i++)
                {
                    var cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1)
                    {
                        rosters[id].Add(cur);
                    }
                    else
                    {
                        break;
                    }
                }
                for (var i = 13; i <= 20; i++)
                {
                    var cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
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
                var validPlayers = players.FindAll(
                    delegate(Dictionary<string, string> player)
                        {
                            if (isValidPlayer(player, nba2KVersion) && player["YearsPro"] != "1")
                            {
                                return true;
                            }
                            return false;
                        });

                foreach (var player in validPlayers)
                {
                    var playerID = Convert.ToInt32(player["ID"]);

                    var lastName = player["Last_Name"];
                    var firstName = player["First_Name"];

#if DEBUG
                    //if (lastName == "Battie") System.Diagnostics.Debugger.Break();
#endif

                    var playerStatsID = player["StatY1"];
                    Dictionary<string, string> playerSeasonStats;
                    try
                    {
                        playerSeasonStats = playerStats.Single(s => s["ID"] == playerStatsID);
                    }
                    catch
                    {
                        playerSeasonStats = null;
                    }

                    string team1;
                    string team2;
                    if (playerSeasonStats != null)
                    {
                        team1 = playerSeasonStats["TeamID2"];
                        team2 = playerSeasonStats["TeamID1"];
                        var hasBeenTraded = (team1 != "-1");

                        if (!hasBeenTraded)
                        {
                            team1 = team2;
                        }
                    }
                    else
                    {
                        team1 = "-1";
                        team2 = "-1";
                    }
                    if (pst.ContainsKey(playerID) && (pst[playerID].LastName != lastName || pst[playerID].FirstName != firstName))
                    {
                        var candidates =
                            pst.Where(
                                pair =>
                                pair.Value.LastName == lastName && pair.Value.FirstName == firstName && pair.Value.IsHidden == false)
                               .ToList();
                        if (candidates.Any())
                        {
                            var temptst = tst;
                            var c2 = candidates.Where(pair => temptst.ContainsKey(pair.Value.TeamF)).ToList();
                            if (c2.Count == 1)
                            {
                                playerID = c2.First().Value.ID;
                            }
                            else
                            {
                                if (team1 != "-1" && activeTeamsIDs.Contains(team1.ToInt32()))
                                {
                                    var curTeam =
                                        tst.Single(team => team.Value.Name == activeTeams.Find(ateam => ateam["ID"] == team1)["Name"]);

                                    var c3 = candidates.Where(pair => pair.Value.TeamF == curTeam.Value.ID).ToList();
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
                                else
                                {
                                    duplicatePlayers.Add(firstName + " " + lastName);
                                    continue;
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

                    var ps = pst[playerID];
                    ps.IsHidden = false;
                    ps.YearsPro = Convert.ToInt32(player["YearsPro"]) - 1;
                    ps.YearOfBirth = Convert.ToInt32(player["BirthYear"]);
                    ps.Height = Convert.ToDouble(player["Height"]);
                    ps.Weight = Convert.ToDouble(player["Weight"]);

                    if (playerSeasonStats != null)
                    {
                        ps.TeamF = Convert.ToInt32(team1);
                        ps.TeamS = Convert.ToInt32(team2);

                        ps.IsSigned = team1 != "-1";

                        importPlayerStats(ps, playerSeasonStats, nba2KVersion, false);

                        ps.CalcAvg();
                    }
                    else
                    {
                        ps.TeamF = -1;

                        ps.IsSigned = false;

                        ps.CalcAvg();
                    }

                    if (nba2KVersion >= NBA2KVersion.NBA2K13)
                    {
                        importPlayerCareerHighs(ps, player);
                    }
                }
            }

            if (duplicatePlayers.Count > 0)
            {
                var msg =
                    "The following names belong to two or more players in the database and the tool couldn't determine who to import to:\n\n";
                duplicatePlayers.ForEach(item => msg += item + ", ");
                msg = msg.TrimEnd(new[] { ' ', ',' });
                msg += "\n\nImport will continue, but there will be some stats missing." + "\n\nTo avoid this problem, either\n"
                       + "1) disable the duplicate occurences via (Miscellaneous > Enable/Disable Players For This Season...), or\n"
                       + "2) transfer the correct instance of the player to their current team.";
                MessageBox.Show(msg);
            }

            #endregion

            return 0;
        }

        private static void importPlayerStats(
            PlayerStats ps, Dictionary<string, string> redPlayerStats, NBA2KVersion nba2KVersion, bool playoffs)
        {
            var arr = !playoffs ? ps.Totals : ps.PlTotals;
            arr[PAbbrT.GP] = Convert.ToUInt16(redPlayerStats["GamesP"]);
            arr[PAbbrT.GS] = Convert.ToUInt16(redPlayerStats["GamesS"]);
            arr[PAbbrT.MINS] = Convert.ToUInt16(redPlayerStats["Minutes"]);
            arr[PAbbrT.PTS] = Convert.ToUInt16(redPlayerStats["Points"]);
            arr[PAbbrT.DREB] = Convert.ToUInt16(redPlayerStats["DRebs"]);
            arr[PAbbrT.OREB] = Convert.ToUInt16(redPlayerStats["ORebs"]);
            arr[PAbbrT.AST] = Convert.ToUInt16(redPlayerStats["Assists"]);
            arr[PAbbrT.STL] = Convert.ToUInt16(redPlayerStats["Steals"]);
            arr[PAbbrT.BLK] = Convert.ToUInt16(redPlayerStats["Blocks"]);
            arr[PAbbrT.TOS] = Convert.ToUInt16(redPlayerStats["TOs"]);
            arr[PAbbrT.FOUL] = Convert.ToUInt16(redPlayerStats["Fouls"]);
            arr[PAbbrT.FGM] = Convert.ToUInt16(redPlayerStats["FGMade"]);
            arr[PAbbrT.FGA] = Convert.ToUInt16(redPlayerStats["FGAtt"]);
            try
            {
                arr[PAbbrT.TPM] = Convert.ToUInt16(redPlayerStats["3PTMade"]);
                arr[PAbbrT.TPA] = Convert.ToUInt16(redPlayerStats["3PTAtt"]);
            }
            catch (KeyNotFoundException)
            {
                arr[PAbbrT.TPM] = Convert.ToUInt16(redPlayerStats["TPTMade"]);
                arr[PAbbrT.TPA] = Convert.ToUInt16(redPlayerStats["TPTAtt"]);
            }
            arr[PAbbrT.FTM] = Convert.ToUInt16(redPlayerStats["FTMade"]);
            arr[PAbbrT.FTA] = Convert.ToUInt16(redPlayerStats["FTAtt"]);

            if (nba2KVersion == NBA2KVersion.NBA2K12)
            {
                ps.IsAllStar = Convert.ToBoolean(Convert.ToInt32(redPlayerStats["IsAStar"]));
                ps.IsNBAChampion = Convert.ToBoolean(Convert.ToInt32(redPlayerStats["IsChamp"]));
            }
            else if (nba2KVersion >= NBA2KVersion.NBA2K13)
            {
                ps.IsAllStar = Convert.ToBoolean(Convert.ToInt32(redPlayerStats["IsAllStar"]));
                ps.IsNBAChampion = Convert.ToBoolean(Convert.ToInt32(redPlayerStats["IsNBAChamp"]));
            }
        }

        private static void importPlayerCareerHighs(PlayerStats ps, Dictionary<string, string> redPlayer)
        {
            ps.CareerHighs[PAbbrT.PTS] = Convert.ToUInt16(redPlayer["GH_CarPts"]);
            ps.CareerHighs[PAbbrT.FGM] = Convert.ToUInt16(redPlayer["GH_CarFGM"]);
            ps.CareerHighs[PAbbrT.FGA] = Convert.ToUInt16(redPlayer["GH_CarFGA"]);
            ps.CareerHighs[PAbbrT.TPM] = Convert.ToUInt16(redPlayer["GH_Car3PM"]);
            ps.CareerHighs[PAbbrT.TPA] = Convert.ToUInt16(redPlayer["GH_Car3PA"]);
            ps.CareerHighs[PAbbrT.FTM] = Convert.ToUInt16(redPlayer["GH_CarFTM"]);
            ps.CareerHighs[PAbbrT.FTA] = Convert.ToUInt16(redPlayer["GH_CarFTA"]);
            ps.CareerHighs[PAbbrT.OREB] = Convert.ToUInt16(redPlayer["GH_CarOReb"]);
            ps.CareerHighs[PAbbrT.DREB] = Convert.ToUInt16(redPlayer["GH_CarDReb"]);
            ps.CareerHighs[PAbbrT.REB] = Convert.ToUInt16(redPlayer["GH_CarRebs"]);
            ps.CareerHighs[PAbbrT.AST] = Convert.ToUInt16(redPlayer["GH_CarAst"]);
            ps.CareerHighs[PAbbrT.STL] = Convert.ToUInt16(redPlayer["GH_CarStl"]);
            ps.CareerHighs[PAbbrT.BLK] = Convert.ToUInt16(redPlayer["GH_CarBlk"]);
        }

        /// <summary>Creates the NBA divisions and conferences.</summary>
        public static void CreateDivisions()
        {
            MainWindow.Conferences.Clear();
            MainWindow.Conferences.Add(new Conference { ID = 0, Name = "East" });
            MainWindow.Conferences.Add(new Conference { ID = 1, Name = "West" });

            MainWindow.Divisions.Clear();
            MainWindow.Divisions.Add(new Division { ID = 0, Name = "Atlantic", ConferenceID = 0 });
            MainWindow.Divisions.Add(new Division { ID = 1, Name = "Central", ConferenceID = 0 });
            MainWindow.Divisions.Add(new Division { ID = 2, Name = "Southeast", ConferenceID = 0 });
            MainWindow.Divisions.Add(new Division { ID = 3, Name = "Southwest", ConferenceID = 1 });
            MainWindow.Divisions.Add(new Division { ID = 4, Name = "Northwest", ConferenceID = 1 });
            MainWindow.Divisions.Add(new Division { ID = 5, Name = "Pacific", ConferenceID = 1 });
        }

        /// <summary>Creates a new player and adds them to the player stats dictionary.</summary>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="player">The dictionary containing the player information.</param>
        /// <param name="preferredID">The preferred ID.</param>
        /// <returns></returns>
        private static int createNewPlayer(
            ref Dictionary<int, PlayerStats> pst, Dictionary<string, string> player, int preferredID = -1)
        {
            int playerID;
            if (preferredID == -1)
            {
                playerID = SQLiteIO.GetFreeID(
                    MainWindow.CurrentDB,
                    "Players" + (MainWindow.CurSeason != SQLiteIO.GetMaxSeason(MainWindow.CurrentDB) ? "S" + MainWindow.CurSeason : ""),
                    "ID",
                    new List<int>());
            }
            else
            {
                playerID = preferredID;
            }
            while (pst.ContainsKey(playerID))
            {
                playerID++;
            }
            pst.Add(
                playerID,
                new PlayerStats(
                    new Player
                        {
                            ID = playerID,
                            FirstName = player["First_Name"],
                            LastName = player["Last_Name"],
                            Position1 = (Position) Enum.Parse(typeof(Position), Positions[player["Pos"]]),
                            Position2 = (Position) Enum.Parse(typeof(Position), Positions[player["SecondPos"]])
                        }));
            return playerID;
        }

        /// <summary>Calculates the box score by comparing the participating team's current and previous team and player stats.</summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="oldTST">The old team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="oldPST">The old player stats dictionary.</param>
        /// <param name="t1">The away team ID.</param>
        /// <param name="t2">The home team ID.</param>
        /// <returns></returns>
        private static BoxScoreEntry prepareBoxScore(
            Dictionary<int, TeamStats> tst,
            Dictionary<int, TeamStats> oldTST,
            Dictionary<int, PlayerStats> pst,
            Dictionary<int, PlayerStats> oldPST,
            int t1,
            int t2)
        {
            var isPlayoff = (tst[t1].GetPlayoffGames() > 0);
            var bs = new TeamBoxScore
                {
                    IsPlayoff = isPlayoff,
                    Team1ID = t1,
                    MINS1 = getDiff(tst, oldTST, t1, TAbbrT.MINS, isPlayoff),
                    Team2ID = t2,
                    MINS2 = getDiff(tst, oldTST, t2, TAbbrT.MINS, isPlayoff)
                };

            var bse = new BoxScoreEntry(bs) { PBSList = new List<PlayerBoxScore>() };

            /*
                        var team1Players = pst.Where(pair => pair.Value.TeamF == bs.Team1);
                        var team2Players = pst.Where(pair => pair.Value.TeamF == bs.Team2);
                        */

            var bothTeamsPlayers = pst.Where(pair => pair.Value.TeamF == bs.Team1ID || pair.Value.TeamF == bs.Team2ID);
            foreach (var playerKVP in bothTeamsPlayers)
            {
                var oldplayerKVP = oldPST.Single(pair => pair.Value.ID == playerKVP.Value.ID);

                var newPlayer = playerKVP.Value;
                var oldPlayer = oldplayerKVP.Value;

                PlayerBoxScore pbs;
                if (getDiff(newPlayer, oldPlayer, PAbbrT.GP) == 1)
                {
                    pbs = new PlayerBoxScore
                        {
                            PlayerID = newPlayer.ID,
                            TeamID = newPlayer.TeamF,
                            IsStarter = (getDiff(newPlayer, oldPlayer, PAbbrT.GS) == 1),
                            PlayedInjured = newPlayer.Injury.IsInjured,
                            MINS = getDiff(newPlayer, oldPlayer, PAbbrT.MINS),
                            PTS = getDiff(newPlayer, oldPlayer, PAbbrT.PTS),
                            OREB = getDiff(newPlayer, oldPlayer, PAbbrT.OREB),
                            DREB = getDiff(newPlayer, oldPlayer, PAbbrT.DREB),
                            AST = getDiff(newPlayer, oldPlayer, PAbbrT.AST),
                            STL = getDiff(newPlayer, oldPlayer, PAbbrT.STL),
                            BLK = getDiff(newPlayer, oldPlayer, PAbbrT.BLK),
                            TOS = getDiff(newPlayer, oldPlayer, PAbbrT.TOS),
                            FGM = getDiff(newPlayer, oldPlayer, PAbbrT.FGM),
                            FGA = getDiff(newPlayer, oldPlayer, PAbbrT.FGA),
                            TPM = getDiff(newPlayer, oldPlayer, PAbbrT.TPM),
                            TPA = getDiff(newPlayer, oldPlayer, PAbbrT.TPA),
                            FTM = getDiff(newPlayer, oldPlayer, PAbbrT.FTM),
                            FTA = getDiff(newPlayer, oldPlayer, PAbbrT.FTA),
                            FOUL = getDiff(newPlayer, oldPlayer, PAbbrT.FOUL)
                        };
                    pbs.REB = (ushort) (pbs.OREB + pbs.DREB);
                    pbs.FGp = (float) pbs.FGM / pbs.FGA;
                    pbs.TPp = (float) pbs.TPM / pbs.TPA;
                    pbs.FTp = (float) pbs.FTM / pbs.FTA;
                }
                else
                {
                    pbs = new PlayerBoxScore { PlayerID = newPlayer.ID, TeamID = newPlayer.TeamF, IsOut = true };
                }

                bse.PBSList.Add(pbs);
            }
            bse.BS.GameDate = DateTime.Today;
            bse.BS.SeasonNum = MainWindow.CurSeason;

            return bse;
        }

        /// <summary>Gets the difference of a team's stat's value between the current and previous stats.</summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="oldTST">The old team stats dictionary.</param>
        /// <param name="teamID">The team ID.</param>
        /// <param name="stat">The stat.</param>
        /// <param name="isPlayoff">
        ///     if set to <c>true</c>, the difference will be calculated based on the playoff stats.
        /// </param>
        /// <returns></returns>
        private static ushort getDiff(
            Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> oldTST, int teamID, int stat, bool isPlayoff = false)
        {
            return !isPlayoff
                       ? (ushort) (tst[teamID].Totals[stat] - oldTST[teamID].Totals[stat])
                       : (ushort) (tst[teamID].PlTotals[stat] - oldTST[teamID].PlTotals[stat]);
        }

        /// <summary>Gets the difference of a player's stat's value between the current and previous stats.</summary>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="oldPST">The old player stats dictionary.</param>
        /// <param name="id">The player's ID.</param>
        /// <param name="stat">The stat.</param>
        /// <param name="isPlayoff">Whether to compare the playoff stats, instead of the regular season ones.</param>
        /// <returns>The difference between the stats.</returns>
        private static ushort getDiff(
            Dictionary<int, PlayerStats> pst, Dictionary<int, PlayerStats> oldPST, int id, int stat, bool isPlayoff = false)
        {
            return getDiff(pst[id], oldPST[id], stat, isPlayoff);
        }

        /// <summary>Gets the difference of a player's stat's value between the current and previous stats.</summary>
        /// <param name="newPS">The new player stats instance.</param>
        /// <param name="oldPS">The old player stats instance.</param>
        /// <param name="stat">The stat.</param>
        /// <param name="isPlayoff">Whether to compare the playoff stats, instead of the regular season ones.</param>
        /// <returns>The difference between the stats.</returns>
        private static ushort getDiff(PlayerStats newPS, PlayerStats oldPS, int stat, bool isPlayoff = false)
        {
            return !isPlayoff
                       ? (ushort) (newPS.Totals[stat] - oldPS.Totals[stat])
                       : (ushort) (newPS.PlTotals[stat] - oldPS.PlTotals[stat]);
        }

        /// <summary>
        ///     Exports all the team (and optionally player) stats and information to a set of CSV files, which can then be imported into
        ///     REDitor.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="teamsOnly">
        ///     if set to <c>true</c>, only the teams' stats will be exported.
        /// </param>
        /// <returns></returns>
        public static int ExportCurrentYear(
            Dictionary<int, TeamStats> tst, Dictionary<int, PlayerStats> pst, string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;
            NBA2KVersion nba2KVersion;
            if (populateREDitorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats, out nba2KVersion) == -1)
            {
                return -1;
            }

            foreach (var key in tst.Keys)
            {
                var ts = tst[key];

                var id = ts.ID;

                var tindex = teams.FindIndex(
                    delegate(Dictionary<string, string> s)
                        {
                            if (s["ID"] == id.ToString())
                            {
                                return true;
                            }
                            return false;
                        });

                var team = teams[tindex];

                var sStatsID = Convert.ToInt32(team["StatCurS"]);
                var pStatsID = Convert.ToInt32(team["StatCurP"]);

                var sStatsIndex = teamStats.FindIndex(
                    delegate(Dictionary<string, string> s)
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
                    teamStats[sStatsIndex]["Mins"] = ts.Totals[TAbbrT.MINS].ToString();
                    teamStats[sStatsIndex]["PtsFor"] = ts.Totals[TAbbrT.PF].ToString();
                    teamStats[sStatsIndex]["PtsAg"] = ts.Totals[TAbbrT.PA].ToString();
                    teamStats[sStatsIndex]["FGMade"] = ts.Totals[TAbbrT.FGM].ToString();
                    teamStats[sStatsIndex]["FGAtt"] = ts.Totals[TAbbrT.FGA].ToString();
                    try
                    {
                        teamStats[sStatsIndex]["3PTMade"] = ts.Totals[TAbbrT.TPM].ToString();
                        teamStats[sStatsIndex]["3PTAtt"] = ts.Totals[TAbbrT.TPA].ToString();
                    }
                    catch (KeyNotFoundException)
                    {
                        teamStats[sStatsIndex]["TPTMade"] = ts.Totals[TAbbrT.TPM].ToString();
                        teamStats[sStatsIndex]["TPTAtt"] = ts.Totals[TAbbrT.TPA].ToString();
                    }
                    teamStats[sStatsIndex]["FTMade"] = ts.Totals[TAbbrT.FTM].ToString();
                    teamStats[sStatsIndex]["FTAtt"] = ts.Totals[TAbbrT.FTA].ToString();
                    teamStats[sStatsIndex]["DRebs"] = ts.Totals[TAbbrT.DREB].ToString();
                    teamStats[sStatsIndex]["ORebs"] = ts.Totals[TAbbrT.OREB].ToString();
                    teamStats[sStatsIndex]["Steals"] = ts.Totals[TAbbrT.STL].ToString();
                    teamStats[sStatsIndex]["Blocks"] = ts.Totals[TAbbrT.BLK].ToString();
                    teamStats[sStatsIndex]["Assists"] = ts.Totals[TAbbrT.AST].ToString();
                    teamStats[sStatsIndex]["Fouls"] = ts.Totals[TAbbrT.FOUL].ToString();
                    teamStats[sStatsIndex]["TOs"] = ts.Totals[TAbbrT.TOS].ToString();
                }

                if (pStatsID != -1)
                {
                    var pStatsIndex = teamStats.FindIndex(
                        delegate(Dictionary<string, string> s)
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
                        teamStats[pStatsIndex]["Mins"] = ts.PlTotals[TAbbrT.MINS].ToString();
                        teamStats[pStatsIndex]["PtsFor"] = ts.PlTotals[TAbbrT.PF].ToString();
                        teamStats[pStatsIndex]["PtsAg"] = ts.PlTotals[TAbbrT.PA].ToString();
                        teamStats[pStatsIndex]["FGMade"] = ts.PlTotals[TAbbrT.FGM].ToString();
                        teamStats[pStatsIndex]["FGAtt"] = ts.PlTotals[TAbbrT.FGA].ToString();
                        try
                        {
                            teamStats[pStatsIndex]["3PTMade"] = ts.PlTotals[TAbbrT.TPM].ToString();
                            teamStats[pStatsIndex]["3PTAtt"] = ts.PlTotals[TAbbrT.TPA].ToString();
                        }
                        catch (KeyNotFoundException)
                        {
                            teamStats[pStatsIndex]["TPTMade"] = ts.PlTotals[TAbbrT.TPM].ToString();
                            teamStats[pStatsIndex]["TPTAtt"] = ts.PlTotals[TAbbrT.TPA].ToString();
                        }
                        teamStats[pStatsIndex]["FTMade"] = ts.PlTotals[TAbbrT.FTM].ToString();
                        teamStats[pStatsIndex]["FTAtt"] = ts.PlTotals[TAbbrT.FTA].ToString();
                        teamStats[pStatsIndex]["DRebs"] = ts.PlTotals[TAbbrT.DREB].ToString();
                        teamStats[pStatsIndex]["ORebs"] = ts.PlTotals[TAbbrT.OREB].ToString();
                        teamStats[pStatsIndex]["Steals"] = ts.PlTotals[TAbbrT.STL].ToString();
                        teamStats[pStatsIndex]["Blocks"] = ts.PlTotals[TAbbrT.BLK].ToString();
                        teamStats[pStatsIndex]["Assists"] = ts.PlTotals[TAbbrT.AST].ToString();
                        teamStats[pStatsIndex]["Fouls"] = ts.PlTotals[TAbbrT.FOUL].ToString();
                        teamStats[pStatsIndex]["TOs"] = ts.PlTotals[TAbbrT.TOS].ToString();
                    }
                }
            }

            var unmatchedPlayers = new List<string>();
            if (!teamsOnly)
            {
                foreach (var key in pst.Keys)
                {
                    var ps = pst[key];

                    var id = ps.ID;

                    Dictionary<string, string> player;
                    var candidates =
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
                            var message = String.Format("{0}: {1} {2} (Born {3}", ps.ID, ps.FirstName, ps.LastName, ps.YearOfBirth);
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

                    var playerSeasonStatsID = player["StatY0"];
                    var playerSeasonStatsIndex = playerStats.FindIndex(s => s["ID"] == playerSeasonStatsID);
                    var playerPlayoffStatsIndex = -1;
                    if (nba2KVersion >= NBA2KVersion.NBA2K13)
                    {
                        var playerPlayoffStatsID = player["StatPOs"];
                        playerPlayoffStatsIndex = playerStats.FindIndex(s => s["ID"] == playerPlayoffStatsID);
                    }

                    if (playerSeasonStatsIndex != -1)
                    {
                        playerStats[playerSeasonStatsIndex]["GamesP"] = ps.Totals[PAbbrT.GP].ToString();
                        playerStats[playerSeasonStatsIndex]["GamesS"] = ps.Totals[PAbbrT.GS].ToString();
                        playerStats[playerSeasonStatsIndex]["Minutes"] = ps.Totals[PAbbrT.MINS].ToString();
                        playerStats[playerSeasonStatsIndex]["Points"] = ps.Totals[PAbbrT.PTS].ToString();
                        playerStats[playerSeasonStatsIndex]["DRebs"] = ps.Totals[PAbbrT.DREB].ToString();
                        playerStats[playerSeasonStatsIndex]["ORebs"] = ps.Totals[PAbbrT.OREB].ToString();
                        playerStats[playerSeasonStatsIndex]["Assists"] = ps.Totals[PAbbrT.AST].ToString();
                        playerStats[playerSeasonStatsIndex]["Steals"] = ps.Totals[PAbbrT.STL].ToString();
                        playerStats[playerSeasonStatsIndex]["Blocks"] = ps.Totals[PAbbrT.BLK].ToString();
                        playerStats[playerSeasonStatsIndex]["TOs"] = ps.Totals[PAbbrT.TOS].ToString();
                        playerStats[playerSeasonStatsIndex]["Fouls"] = ps.Totals[PAbbrT.FOUL].ToString();
                        playerStats[playerSeasonStatsIndex]["FGMade"] = ps.Totals[PAbbrT.FGM].ToString();
                        playerStats[playerSeasonStatsIndex]["FGAtt"] = ps.Totals[PAbbrT.FGA].ToString();
                        try
                        {
                            playerStats[playerSeasonStatsIndex]["3PTMade"] = ps.Totals[PAbbrT.TPM].ToString();
                            playerStats[playerSeasonStatsIndex]["3PTAtt"] = ps.Totals[PAbbrT.TPA].ToString();
                        }
                        catch (KeyNotFoundException)
                        {
                            playerStats[playerSeasonStatsIndex]["TPTMade"] = ps.Totals[PAbbrT.TPM].ToString();
                            playerStats[playerSeasonStatsIndex]["TPTAtt"] = ps.Totals[PAbbrT.TPA].ToString();
                        }
                        playerStats[playerSeasonStatsIndex]["FTMade"] = ps.Totals[PAbbrT.FTM].ToString();
                        playerStats[playerSeasonStatsIndex]["FTAtt"] = ps.Totals[PAbbrT.FTA].ToString();
                        if (nba2KVersion == NBA2KVersion.NBA2K12)
                        {
                            playerStats[playerSeasonStatsIndex]["IsAStar"] = (ps.IsAllStar ? 1 : 0).ToString();
                            playerStats[playerSeasonStatsIndex]["IsChamp"] = (ps.IsNBAChampion ? 1 : 0).ToString();
                        }
                        else if (nba2KVersion >= NBA2KVersion.NBA2K13)
                        {
                            playerStats[playerSeasonStatsIndex]["IsAllStar"] = (ps.IsAllStar ? 1 : 0).ToString();
                            playerStats[playerSeasonStatsIndex]["IsNBAChamp"] = (ps.IsNBAChampion ? 1 : 0).ToString();
                        }
                    }

                    if (playerPlayoffStatsIndex != -1)
                    {
                        playerStats[playerPlayoffStatsIndex]["GamesP"] = ps.PlTotals[PAbbrT.GP].ToString();
                        playerStats[playerPlayoffStatsIndex]["GamesS"] = ps.PlTotals[PAbbrT.GS].ToString();
                        playerStats[playerPlayoffStatsIndex]["Minutes"] = ps.PlTotals[PAbbrT.MINS].ToString();
                        playerStats[playerPlayoffStatsIndex]["Points"] = ps.PlTotals[PAbbrT.PTS].ToString();
                        playerStats[playerPlayoffStatsIndex]["DRebs"] = ps.PlTotals[PAbbrT.DREB].ToString();
                        playerStats[playerPlayoffStatsIndex]["ORebs"] = ps.PlTotals[PAbbrT.OREB].ToString();
                        playerStats[playerPlayoffStatsIndex]["Assists"] = ps.PlTotals[PAbbrT.AST].ToString();
                        playerStats[playerPlayoffStatsIndex]["Steals"] = ps.PlTotals[PAbbrT.STL].ToString();
                        playerStats[playerPlayoffStatsIndex]["Blocks"] = ps.PlTotals[PAbbrT.BLK].ToString();
                        playerStats[playerPlayoffStatsIndex]["TOs"] = ps.PlTotals[PAbbrT.TOS].ToString();
                        playerStats[playerPlayoffStatsIndex]["Fouls"] = ps.PlTotals[PAbbrT.FOUL].ToString();
                        playerStats[playerPlayoffStatsIndex]["FGMade"] = ps.PlTotals[PAbbrT.FGM].ToString();
                        playerStats[playerPlayoffStatsIndex]["FGAtt"] = ps.PlTotals[PAbbrT.FGA].ToString();
                        try
                        {
                            playerStats[playerPlayoffStatsIndex]["3PTMade"] = ps.PlTotals[PAbbrT.TPM].ToString();
                            playerStats[playerPlayoffStatsIndex]["3PTAtt"] = ps.PlTotals[PAbbrT.TPA].ToString();
                        }
                        catch (KeyNotFoundException)
                        {
                            playerStats[playerPlayoffStatsIndex]["TPTMade"] = ps.PlTotals[PAbbrT.TPM].ToString();
                            playerStats[playerPlayoffStatsIndex]["TPTAtt"] = ps.PlTotals[PAbbrT.TPA].ToString();
                        }
                        playerStats[playerPlayoffStatsIndex]["FTMade"] = ps.PlTotals[PAbbrT.FTM].ToString();
                        playerStats[playerPlayoffStatsIndex]["FTAtt"] = ps.PlTotals[PAbbrT.FTA].ToString();
                        if (nba2KVersion == NBA2KVersion.NBA2K12)
                        {
                            playerStats[playerPlayoffStatsIndex]["IsAStar"] = (ps.IsAllStar ? 1 : 0).ToString();
                            playerStats[playerPlayoffStatsIndex]["IsChamp"] = (ps.IsNBAChampion ? 1 : 0).ToString();
                        }
                        else if (nba2KVersion >= NBA2KVersion.NBA2K13)
                        {
                            playerStats[playerPlayoffStatsIndex]["IsAllStar"] = (ps.IsAllStar ? 1 : 0).ToString();
                            playerStats[playerPlayoffStatsIndex]["IsNBAChamp"] = (ps.IsNBAChampion ? 1 : 0).ToString();
                        }
                    }
                }
            }

            var path = folder + @"\Team_Stats.csv";
            CSV.CSVFromDictionaryList(teamStats, path);
            if (!teamsOnly)
            {
                path = folder + @"\Player_Stats.csv";
                CSV.CSVFromDictionaryList(playerStats, path);
                if (unmatchedPlayers.Count > 0)
                {
                    unmatchedPlayers.Add("");
                    MessageBox.Show(
                        unmatchedPlayers.Aggregate((s1, s2) => s1 + "\n" + s2) + "\n"
                        + "The above players have multiple matching ones in the NBA 2K save and can't be exported.\n"
                        + "Please send your save and database to the developer.\n\nEverything else was exported successfully.");
                }
            }

            return 0;
        }

        /// <summary>
        ///     Populates the REDitor dictionary lists by importing the CSV data into them. Each dictionary has Setting-Value pairs, where
        ///     Setting is the column header, and Value is the corresponding value of that particular record.
        /// </summary>
        /// <param name="folder">The folder containing the REDitor-exported CSV files.</param>
        /// <param name="teams">The resulting teams information dictionary list.</param>
        /// <param name="players">The resulting players information dictionary list.</param>
        /// <param name="teamStats">The resulting team stats dictionary list.</param>
        /// <param name="playerStats">The resulting player stats dictionary list.</param>
        /// <param name="nba2KVersion">Returns the detected NBA 2K version.</param>
        /// <returns>0 if the dictionaries were populated correctly, -1 if an error occurred.</returns>
        private static int populateREDitorDictionaryLists(
            string folder,
            out List<Dictionary<string, string>> teams,
            out List<Dictionary<string, string>> players,
            out List<Dictionary<string, string>> teamStats,
            out List<Dictionary<string, string>> playerStats,
            out NBA2KVersion nba2KVersion)
        {
            try
            {
                teams = CSV.DictionaryListFromCSVFile(folder + @"\Teams.csv");
                players = CSV.DictionaryListFromCSVFile(folder + @"\Players.csv");
                teamStats = CSV.DictionaryListFromCSVFile(folder + @"\Team_Stats.csv");
                playerStats = CSV.DictionaryListFromCSVFile(folder + @"\Player_Stats.csv");
                if (players[0].ContainsKey("PlType"))
                {
                    nba2KVersion = NBA2KVersion.NBA2K12;
                }
                else if (players[0].ContainsKey("IsRegNBA"))
                {
                    nba2KVersion = NBA2KVersion.NBA2K13;
                }
                else if (players[0].ContainsKey("IsRegular"))
                {
                    nba2KVersion = NBA2KVersion.NBA2K14;
                }
                else
                {
                    throw new Exception("This is an unknown format");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                teams = null;
                players = null;
                teamStats = null;
                playerStats = null;
                nba2KVersion = NBA2KVersion.NBA2K12;
                return -1;
            }
            return 0;
        }
    }
}