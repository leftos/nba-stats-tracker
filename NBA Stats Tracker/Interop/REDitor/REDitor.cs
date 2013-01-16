#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Misc;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;
using MessageBox = System.Windows.MessageBox;

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

        public static List<int> teamsThatPlayedAGame;
        public static List<int> pickedTeams;
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
                return;

            var sw = new StreamWriter(sfd.FileName);
            sw.Write(stg);
            sw.Close();
        }

        /// <summary>
        ///     Imports all team (and optionally) player stats from an REDitor-exported set of CSV files.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstopp">The opposing team stats dictionary.</param>
        /// <param name="TeamOrder">The team order.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="folder">The folder containing the exported CSV files.</param>
        /// <param name="teamsOnly">
        ///     if set to <c>true</c>, only team stats will be imported.
        /// </param>
        /// <returns></returns>
        public static int ImportAll(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstopp,
                                    ref SortedDictionary<string, int> TeamOrder, ref Dictionary<int, PlayerStats> pst, string folder,
                                    bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            if (PopulateREditorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
                return -1;

            List<string> importMessages = new List<string>();

            #region Import Teams & Team Stats

            var legalTTypes = new List<string> {"0", "4"};

            List<Dictionary<string, string>> validTeams = teams.FindAll(delegate(Dictionary<string, string> team)
                                                                        {
                                                                            if (legalTTypes.IndexOf(team["TType"]) != -1)
                                                                                return true;
                                                                            return false;
                                                                        });

            List<Dictionary<string, string>> activeTeams = validTeams.FindAll(delegate(Dictionary<string, string> team)
                                                                              {
                                                                                  if (team["StatCurS"] != "-1")
                                                                                      return true;
                                                                                  return false;
                                                                              });
            if (activeTeams.Count < 30)
            {
                var dlw = new DualListWindow(validTeams, activeTeams);
                if (dlw.ShowDialog() == false)
                {
                    return -1;
                }

                activeTeams = new List<Dictionary<string, string>>(MainWindow.selectedTeams);

                if (MainWindow.selectedTeamsChanged)
                {
                    CreateSettingsFile(activeTeams, folder);
                }
            }

            bool madeNew = false;

            if (tst.Count != activeTeams.Count)
            {
                tst = new Dictionary<int, TeamStats>();
                tstopp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }

            var oldTST = new Dictionary<int, TeamStats>();
            foreach (var ts in tst)
            {
                oldTST.Add(ts.Key, ts.Value.Clone());
            }
            var oldTSTOpp = new Dictionary<int, TeamStats>();
            foreach (var ts in tstopp)
            {
                oldTSTOpp.Add(ts.Key, ts.Value.Clone());
            }
            var oldPST = new Dictionary<int, PlayerStats>();
            foreach (var ps in pst)
            {
                oldPST.Add(ps.Key, ps.Value.Clone());
            }

            CreateDivisions();

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            foreach (var team in activeTeams)
            {
                int id = -1;
                string name = team["Name"];
                if (!TeamOrder.ContainsKey(name))
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (!TeamOrder.ContainsValue(i))
                        {
                            id = i;
                            break;
                        }
                    }
                    TeamOrder.Add(name, id);
                }
                id = TeamOrder[name];
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                if (madeNew)
                {
                    tst[id] = new TeamStats(name);
                    tstopp[id] = new TeamStats(name);
                }

                int sStatsID = Convert.ToInt32(team["StatCurS"]);
                int pStatsID = Convert.ToInt32(team["StatCurP"]);

                Dictionary<string, string> sStats = teamStats.Find(delegate(Dictionary<string, string> s)
                                                                   {
                                                                       if (s["ID"] == sStatsID.ToString())
                                                                           return true;
                                                                       return false;
                                                                   });

                tst[id].ID = Convert.ToInt32(team["ID"]);
                tst[id].division = Convert.ToInt32(team["Division"]);
                tstopp[id].division = Convert.ToInt32(team["Division"]);


                if (sStats != null)
                {
                    tst[id].winloss[0] = Convert.ToByte(sStats["Wins"]);
                    tst[id].winloss[1] = Convert.ToByte(sStats["Losses"]);
                    tst[id].stats[t.MINS] = Convert.ToUInt16(sStats["Mins"]);
                    tst[id].stats[t.PF] = Convert.ToUInt16(sStats["PtsFor"]);
                    tst[id].stats[t.PA] = Convert.ToUInt16(sStats["PtsAg"]);
                    tst[id].stats[t.FGM] = Convert.ToUInt16(sStats["FGMade"]);
                    tst[id].stats[t.FGA] = Convert.ToUInt16(sStats["FGAtt"]);
                    tst[id].stats[t.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                    tst[id].stats[t.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                    tst[id].stats[t.FTM] = Convert.ToUInt16(sStats["FTMade"]);
                    tst[id].stats[t.FTA] = Convert.ToUInt16(sStats["FTAtt"]);
                    tst[id].stats[t.DREB] = Convert.ToUInt16(sStats["DRebs"]);
                    tst[id].stats[t.OREB] = Convert.ToUInt16(sStats["ORebs"]);
                    tst[id].stats[t.STL] = Convert.ToUInt16(sStats["Steals"]);
                    tst[id].stats[t.BLK] = Convert.ToUInt16(sStats["Blocks"]);
                    tst[id].stats[t.AST] = Convert.ToUInt16(sStats["Assists"]);
                    tst[id].stats[t.FOUL] = Convert.ToUInt16(sStats["Fouls"]);
                    tst[id].stats[t.TOS] = Convert.ToUInt16(sStats["TOs"]);
                    //tstopp[id].stats[t.TO] = Convert.ToUInt16(sStats["TOsAg"]);

                    if (pStatsID != -1)
                    {
                        Dictionary<string, string> pStats = teamStats.Find(delegate(Dictionary<string, string> s)
                                                                           {
                                                                               if (s["ID"] == pStatsID.ToString())
                                                                                   return true;
                                                                               return false;
                                                                           });
                        tst[id].pl_winloss[0] = Convert.ToByte(pStats["Wins"]);
                        tst[id].pl_winloss[1] = Convert.ToByte(pStats["Losses"]);
                        tst[id].pl_stats[t.MINS] = Convert.ToUInt16(pStats["Mins"]);
                        tst[id].pl_stats[t.PF] = Convert.ToUInt16(pStats["PtsFor"]);
                        tst[id].pl_stats[t.PA] = Convert.ToUInt16(pStats["PtsAg"]);
                        tst[id].pl_stats[t.FGM] = Convert.ToUInt16(pStats["FGMade"]);
                        tst[id].pl_stats[t.FGA] = Convert.ToUInt16(pStats["FGAtt"]);
                        tst[id].pl_stats[t.TPM] = Convert.ToUInt16(pStats["3PTMade"]);
                        tst[id].pl_stats[t.TPA] = Convert.ToUInt16(pStats["3PTAtt"]);
                        tst[id].pl_stats[t.FTM] = Convert.ToUInt16(pStats["FTMade"]);
                        tst[id].pl_stats[t.FTA] = Convert.ToUInt16(pStats["FTAtt"]);
                        tst[id].pl_stats[t.DREB] = Convert.ToUInt16(pStats["DRebs"]);
                        tst[id].pl_stats[t.OREB] = Convert.ToUInt16(pStats["ORebs"]);
                        tst[id].pl_stats[t.STL] = Convert.ToUInt16(pStats["Steals"]);
                        tst[id].pl_stats[t.BLK] = Convert.ToUInt16(pStats["Blocks"]);
                        tst[id].pl_stats[t.AST] = Convert.ToUInt16(pStats["Assists"]);
                        tst[id].pl_stats[t.FOUL] = Convert.ToUInt16(pStats["Fouls"]);
                        tst[id].pl_stats[t.TOS] = Convert.ToUInt16(pStats["TOs"]);
                        //tstopp[id].pl_stats[t.TO] = Convert.ToUInt16(pStats["TOsAg"]);
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
                        rosters[id].Add(cur);
                    else
                        break;
                }
                for (int i = 13; i <= 20; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
                    if (cur != -1)
                        rosters[id].Add(cur);
                    else
                        break;
                }
            }

            #endregion

            #region Import Players & Player Stats

            var duplicatePlayers = new List<string>();
            if (!teamsOnly)
            {
                List<Dictionary<string, string>> validPlayers = players.FindAll(delegate(Dictionary<string, string> player)
                                                                                {
                                                                                    if (player["PlType"] == "4" || player["PlType"] == "5" ||
                                                                                        player["PlType"] == "6")
                                                                                    {
                                                                                        if ((player["IsFA"] == "0" &&
                                                                                             player["TeamID1"] != "-1") ||
                                                                                            (player["IsFA"] == "1"))
                                                                                        {
                                                                                            return true;
                                                                                        }
                                                                                    }
                                                                                    return false;
                                                                                });

                foreach (var player in validPlayers)
                {
                    int playerID = Convert.ToInt32(player["ID"]);

                    string LastName = player["Last_Name"];
                    string FirstName = player["First_Name"];

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
                    
                    if (!activeTeamsIDs.Contains(pTeam) && player["IsFA"] != "1")
                    {
                        if (pst.ContainsKey(playerID) && pst[playerID].LastName == LastName && pst[playerID].FirstName == FirstName)
                        {
                            pst[playerID].isActive = false;
                            pst[playerID].TeamF = "";
                            pst[playerID].isHidden = true;
                        }
                        continue;
                    }

                    int playerStatsID = Convert.ToInt32(player["StatY0"]);

                    Dictionary<string, string> plStats = playerStats.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == playerStatsID.ToString())
                                                                                  return true;
                                                                              return false;
                                                                          });

                    if (pst.ContainsKey(playerID) && (pst[playerID].LastName != LastName || pst[playerID].FirstName != FirstName))
                    {
                        List<KeyValuePair<int, PlayerStats>> candidates =
                            pst.Where(
                                pair => pair.Value.LastName == LastName && pair.Value.FirstName == FirstName && pair.Value.isHidden == false)
                               .ToList();
                        if (candidates.Count > 0)
                        {
                            bool found = false;
                            SortedDictionary<string, int> order = TeamOrder;
                            List<KeyValuePair<int, PlayerStats>> c2 = candidates.Where(pair => order.ContainsKey(pair.Value.TeamF)).ToList();
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
                                        KeyValuePair<int, TeamStats> curTeam =
                                            tst.Single(
                                                team => team.Value.name == activeTeams.Find(ateam => ateam["ID"] == pTeam.ToString())["Name"]);

                                        List<KeyValuePair<int, PlayerStats>> c3 =
                                            candidates.Where(pair => pair.Value.TeamF == curTeam.Value.name).ToList();
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
                                duplicatePlayers.Add(FirstName + " " + LastName);
                                continue;
                            }
                        }
                        else
                        {
                            playerID = CreateNewPlayer(ref pst, player);
                        }
                    }
                    else if (!pst.ContainsKey(playerID))
                    {
                        playerID = CreateNewPlayer(ref pst, player, playerID);
                    }
                    var curPlayer = pst[playerID];
                    var oldPlayer = curPlayer.Clone();

                    curPlayer.Position1 = (Position) Enum.Parse(typeof (Position), player["Pos"]);
                    curPlayer.Position2 = (Position) Enum.Parse(typeof (Position), player["SecondPos"]);
                    curPlayer.isHidden = false;
                    curPlayer.YearsPro = Convert.ToInt32(player["YearsPro"]);
                    curPlayer.YearOfBirth = Convert.ToInt32(player["BirthYear"]);
                    curPlayer.contract.Option = (PlayerContractOption) Enum.Parse(typeof (PlayerContractOption), player["COption"]);
                    curPlayer.contract.ContractSalaryPerYear.Clear();
                    for (int i = 1; i < 7; i++)
                    {
                        var salary = Convert.ToInt32(player["CYear" + i]);
                        if (salary == 0)
                            break;

                        curPlayer.contract.ContractSalaryPerYear.Add(salary);
                    }
                    curPlayer.height = Convert.ToDouble(player["Height"]);
                    curPlayer.weight = Convert.ToDouble(player["Weight"]);

                    if (plStats != null)
                    {
                        string TeamFName = "";
                        string TeamSName = "";
                        string teamReal = pTeam.ToString();
                        string team1 = plStats["TeamID2"];
                        string team2 = plStats["TeamID1"];
                        bool hasBeenTraded = (team1 != "-1");

                        if (!hasBeenTraded)
                        {
                            team1 = team2;
                        }
                        if (teamReal != "-1" && player["IsFA"] != "1")
                        {
                            Dictionary<string, string> TeamF = teams.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == teamReal)
                                                                                  return true;
                                                                              return false;
                                                                          });
                            TeamFName = TeamF["Name"];
                        }

                        if (hasBeenTraded)
                        {
                            Dictionary<string, string> TeamS = teams.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == team2)
                                                                                  return true;
                                                                              return false;
                                                                          });
                            TeamSName = TeamS["Name"];
                        }

                        PlayerStats ps = curPlayer;
                        ps.TeamF = TeamFName;
                        ps.TeamS = TeamSName;

                        ps.isActive = (player["IsFA"] != "1" && teamReal != "-1");

                        ps.stats[p.GP] = Convert.ToUInt16(plStats["GamesP"]);
                        ps.stats[p.GS] = Convert.ToUInt16(plStats["GamesS"]);
                        ps.stats[p.MINS] = Convert.ToUInt16(plStats["Minutes"]);
                        ps.stats[p.PTS] = Convert.ToUInt16(plStats["Points"]);
                        ps.stats[p.DREB] = Convert.ToUInt16(plStats["DRebs"]);
                        ps.stats[p.OREB] = Convert.ToUInt16(plStats["ORebs"]);
                        ps.stats[p.AST] = Convert.ToUInt16(plStats["Assists"]);
                        ps.stats[p.STL] = Convert.ToUInt16(plStats["Steals"]);
                        ps.stats[p.BLK] = Convert.ToUInt16(plStats["Blocks"]);
                        ps.stats[p.TOS] = Convert.ToUInt16(plStats["TOs"]);
                        ps.stats[p.FOUL] = Convert.ToUInt16(plStats["Fouls"]);
                        ps.stats[p.FGM] = Convert.ToUInt16(plStats["FGMade"]);
                        ps.stats[p.FGA] = Convert.ToUInt16(plStats["FGAtt"]);
                        ps.stats[p.TPM] = Convert.ToUInt16(plStats["3PTMade"]);
                        ps.stats[p.TPA] = Convert.ToUInt16(plStats["3PTAtt"]);
                        ps.stats[p.FTM] = Convert.ToUInt16(plStats["FTMade"]);
                        ps.stats[p.FTA] = Convert.ToUInt16(plStats["FTAtt"]);

                        ps.isAllStar = Convert.ToBoolean(Convert.ToInt32(plStats["IsAStar"]));
                        ps.isNBAChampion = Convert.ToBoolean(Convert.ToInt32(plStats["IsChamp"]));

                        ps.isInjured = player["InjType"] != "0";

                        ps.CalcAvg();

                        pst[playerID] = ps;
                    }
                    else
                    {
                        PlayerStats ps = curPlayer;

                        string TeamFName = "";
                        string team1 = pTeam.ToString();
                        if (team1 != "-1" && player["IsFA"] != "1")
                        {
                            Dictionary<string, string> TeamF = teams.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == team1)
                                                                                  return true;
                                                                              return false;
                                                                          });
                            TeamFName = TeamF["Name"];
                        }
                        ps.TeamF = TeamFName;

                        ps.isActive = player["IsFA"] != "1";
                        ps.isInjured = player["InjType"] != "0";

                        ps.CalcAvg();

                        pst[playerID] = ps;
                    }

                    string name = String.Format("{0} {1}", curPlayer.FirstName, curPlayer.LastName);
                    if (oldPlayer.TeamF != curPlayer.TeamF)
                    {
                        string msg;
                        if (curPlayer.isActive && oldPlayer.isActive)
                        {
                            msg = String.Format("{0} was traded from the {1} to the {2}.", name,
                                                Helper.Miscellaneous.Misc.GetDisplayNameFromTeam(tst, oldPlayer.TeamF),
                                                Helper.Miscellaneous.Misc.GetDisplayNameFromTeam(tst, curPlayer.TeamF));
                            importMessages.Add(msg);
                        }
                        else if (oldPlayer.isActive)
                        {
                            msg = String.Format("{0} was released from the {1}.", name,
                                                Helper.Miscellaneous.Misc.GetDisplayNameFromTeam(tst, oldPlayer.TeamF));
                            importMessages.Add(msg);
                        }
                    }
                    
                    if (oldPlayer.contract.GetYears() < curPlayer.contract.GetYears() && curPlayer.isActive)
                    {
                        string msg = name;
                        if (!oldPlayer.isActive && curPlayer.isActive)
                        {
                            msg += " signed ";
                        }
                        else
                        {
                            msg += " re-signed ";
                        }
                        msg += String.Format("with the {0} on a {1}yr/{2:C0} contract.",
                                                 Helper.Miscellaneous.Misc.GetDisplayNameFromTeam(tst, curPlayer.TeamF),
                                                 curPlayer.contract.GetYears(), curPlayer.contract.GetTotal());
                        importMessages.Add(msg);
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

            #region Check for box-scores we can calculate

            if (oldTST.Count == 30)
            {
                teamsThatPlayedAGame = new List<int>();
                foreach (var team in tst)
                {
                    TeamStats newTeam = team.Value;
                    int teamID = team.Key;
                    TeamStats oldTeam = oldTST[teamID];

                    if (oldTeam.getGames() + 1 == newTeam.getGames() || oldTeam.getPlayoffGames() + 1 == newTeam.getPlayoffGames())
                    {
                        teamsThatPlayedAGame.Add(team.Key);
                    }
                }

                if (teamsThatPlayedAGame.Count >= 2)
                {
                    pickedTeams = new List<int>();
                    var dlw = new PickGamesWindow(teamsThatPlayedAGame);

                    if (dlw.ShowDialog() == true)
                    {
                        for (int i = 0; i <= pickedTeams.Count - 2; i += 2)
                        {
                            int t1 = pickedTeams[i];
                            int t2 = pickedTeams[i + 1];

                            BoxScoreEntry bse = PrepareBoxScore(tst, oldTST, pst, oldPST, t1, t2);

                            TeamBoxScore teamBoxScore = bse.bs;
                            BoxScoreWindow.CalculateTeamsFromPlayers(ref teamBoxScore, bse.pbsList.Where(pbs => pbs.Team == bse.bs.Team1),
                                                                     bse.pbsList.Where(pbs => pbs.Team == bse.bs.Team2));

                            if (teamBoxScore.PTS1 != getDiff(tst, oldTST, t1, t.PF, teamBoxScore.isPlayoff) ||
                                teamBoxScore.PTS2 != getDiff(tst, oldTST, t2, t.PF, teamBoxScore.isPlayoff))
                            {
                                MessageBox.Show(
                                    String.Format(
                                        "{0} @ {1} won't have its box-score imported because it couldn't be properly calculated. A possible reason for this is that one or more players participating in that game has been since traded away from the teams.",
                                        tst[t1].displayName, tst[t2].displayName), "NBA Stats Tracker", MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                                continue;
                            }

                            bse.bs.gamedate = SelectedDate;
                            bse.date = bse.bs.gamedate;
                            TeamStats.AddTeamStatsFromBoxScore(bse.bs, ref oldTST, ref oldTSTOpp, t1, t2);
                            MainWindow.bshist.Add(bse);
                            tst[t1] = oldTST[t1].Clone();
                            tst[t2] = oldTST[t2].Clone();
                            tstopp[t1] = oldTSTOpp[t1].Clone();
                            tstopp[t2] = oldTSTOpp[t2].Clone();
                        }
                    }
                }
            }

            #endregion

            if (importMessages.Count > 0)
            {
                importMessages.Add("");
                CopyableMessageWindow cmw = new CopyableMessageWindow(importMessages.Aggregate((m1, m2) => m1 + "\n" + m2),
                                                                      "League Transactions", TextAlignment.Left);
                cmw.ShowDialog();
            }

            return 0;
        }

        public static int ImportPrevious(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstopp,
                                         ref SortedDictionary<string, int> TeamOrder, ref Dictionary<int, PlayerStats> pst, string folder,
                                         bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;

            if (PopulateREditorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
                return -1;

            #region Import Teams & Team Stats

            var legalTTypes = new List<string> {"0", "4"};

            List<Dictionary<string, string>> validTeams = teams.FindAll(delegate(Dictionary<string, string> team)
                                                                        {
                                                                            if (legalTTypes.IndexOf(team["TType"]) != -1)
                                                                                return true;
                                                                            return false;
                                                                        });

            List<Dictionary<string, string>> activeTeams = validTeams.FindAll(delegate(Dictionary<string, string> team)
                                                                              {
                                                                                  if (team["StatCurS"] != "-1")
                                                                                      return true;
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

                activeTeams = new List<Dictionary<string, string>>(MainWindow.selectedTeams);

                if (MainWindow.selectedTeamsChanged)
                {
                    CreateSettingsFile(activeTeams, folder);
                }
            }

            bool madeNew = false;

            if (tst.Count != activeTeams.Count)
            {
                tst = new Dictionary<int, TeamStats>();
                tstopp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }

            var oldTST = new Dictionary<int, TeamStats>();
            foreach (var ts in tst)
            {
                oldTST.Add(ts.Key, ts.Value.Clone());
            }
            var oldTSTOpp = new Dictionary<int, TeamStats>();
            foreach (var ts in tstopp)
            {
                oldTSTOpp.Add(ts.Key, ts.Value.Clone());
            }
            var oldPST = new Dictionary<int, PlayerStats>();
            foreach (var ps in pst)
            {
                oldPST.Add(ps.Key, ps.Value.Clone());
            }

            CreateDivisions();

            var activeTeamsIDs = new List<int>();
            var rosters = new Dictionary<int, List<int>>();
            foreach (var team in activeTeams)
            {
                int id = -1;
                string name = team["Name"];
                if (!TeamOrder.ContainsKey(name))
                {
                    for (int i = 0; i < 30; i++)
                    {
                        if (!TeamOrder.ContainsValue(i))
                        {
                            id = i;
                            break;
                        }
                    }
                    TeamOrder.Add(name, id);
                }
                id = TeamOrder[name];
                activeTeamsIDs.Add(Convert.ToInt32(team["ID"]));

                if (madeNew)
                {
                    tst[id] = new TeamStats(name);
                    tstopp[id] = new TeamStats(name);
                }

                int sStatsID = Convert.ToInt32(team["StatPrevS"]);
                int pStatsID = Convert.ToInt32(team["StatPrevP"]);

                Dictionary<string, string> sStats = teamStats.Find(delegate(Dictionary<string, string> s)
                                                                   {
                                                                       if (s["ID"] == sStatsID.ToString())
                                                                           return true;
                                                                       return false;
                                                                   });

                var curTeam = tst[id];
                curTeam.ID = Convert.ToInt32(team["ID"]);
                curTeam.division = Convert.ToInt32(team["Division"]);
                tstopp[id].division = Convert.ToInt32(team["Division"]);


                if (sStats != null)
                {
                    curTeam.winloss[0] = Convert.ToByte(sStats["Wins"]);
                    curTeam.winloss[1] = Convert.ToByte(sStats["Losses"]);
                    curTeam.stats[t.MINS] = Convert.ToUInt16(sStats["Mins"]);
                    curTeam.stats[t.PF] = Convert.ToUInt16(sStats["PtsFor"]);
                    curTeam.stats[t.PA] = Convert.ToUInt16(sStats["PtsAg"]);
                    curTeam.stats[t.FGM] = Convert.ToUInt16(sStats["FGMade"]);
                    curTeam.stats[t.FGA] = Convert.ToUInt16(sStats["FGAtt"]);
                    curTeam.stats[t.TPM] = Convert.ToUInt16(sStats["3PTMade"]);
                    curTeam.stats[t.TPA] = Convert.ToUInt16(sStats["3PTAtt"]);
                    curTeam.stats[t.FTM] = Convert.ToUInt16(sStats["FTMade"]);
                    curTeam.stats[t.FTA] = Convert.ToUInt16(sStats["FTAtt"]);
                    curTeam.stats[t.DREB] = Convert.ToUInt16(sStats["DRebs"]);
                    curTeam.stats[t.OREB] = Convert.ToUInt16(sStats["ORebs"]);
                    curTeam.stats[t.STL] = Convert.ToUInt16(sStats["Steals"]);
                    curTeam.stats[t.BLK] = Convert.ToUInt16(sStats["Blocks"]);
                    curTeam.stats[t.AST] = Convert.ToUInt16(sStats["Assists"]);
                    curTeam.stats[t.FOUL] = Convert.ToUInt16(sStats["Fouls"]);
                    curTeam.stats[t.TOS] = Convert.ToUInt16(sStats["TOs"]);
                    //tstopp[id].stats[t.TO] = Convert.ToUInt16(sStats["TOsAg"]);

                    if (pStatsID != -1)
                    {
                        Dictionary<string, string> pStats = teamStats.Find(delegate(Dictionary<string, string> s)
                                                                           {
                                                                               if (s["ID"] == pStatsID.ToString())
                                                                                   return true;
                                                                               return false;
                                                                           });
                        curTeam.pl_winloss[0] = Convert.ToByte(pStats["Wins"]);
                        curTeam.pl_winloss[1] = Convert.ToByte(pStats["Losses"]);
                        curTeam.pl_stats[t.MINS] = Convert.ToUInt16(pStats["Mins"]);
                        curTeam.pl_stats[t.PF] = Convert.ToUInt16(pStats["PtsFor"]);
                        curTeam.pl_stats[t.PA] = Convert.ToUInt16(pStats["PtsAg"]);
                        curTeam.pl_stats[t.FGM] = Convert.ToUInt16(pStats["FGMade"]);
                        curTeam.pl_stats[t.FGA] = Convert.ToUInt16(pStats["FGAtt"]);
                        curTeam.pl_stats[t.TPM] = Convert.ToUInt16(pStats["3PTMade"]);
                        curTeam.pl_stats[t.TPA] = Convert.ToUInt16(pStats["3PTAtt"]);
                        curTeam.pl_stats[t.FTM] = Convert.ToUInt16(pStats["FTMade"]);
                        curTeam.pl_stats[t.FTA] = Convert.ToUInt16(pStats["FTAtt"]);
                        curTeam.pl_stats[t.DREB] = Convert.ToUInt16(pStats["DRebs"]);
                        curTeam.pl_stats[t.OREB] = Convert.ToUInt16(pStats["ORebs"]);
                        curTeam.pl_stats[t.STL] = Convert.ToUInt16(pStats["Steals"]);
                        curTeam.pl_stats[t.BLK] = Convert.ToUInt16(pStats["Blocks"]);
                        curTeam.pl_stats[t.AST] = Convert.ToUInt16(pStats["Assists"]);
                        curTeam.pl_stats[t.FOUL] = Convert.ToUInt16(pStats["Fouls"]);
                        curTeam.pl_stats[t.TOS] = Convert.ToUInt16(pStats["TOs"]);
                        //tstopp[id].pl_stats[t.TO] = Convert.ToUInt16(pStats["TOsAg"]);
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
                        rosters[id].Add(cur);
                    else
                        break;
                }
                for (int i = 13; i <= 20; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_R" + i.ToString()]);
                    if (cur != -1)
                        rosters[id].Add(cur);
                    else
                        break;
                }
            }

            #endregion

            #region Import Players & Player Stats

            var duplicatePlayers = new List<string>();
            if (!teamsOnly)
            {
                List<Dictionary<string, string>> validPlayers = players.FindAll(delegate(Dictionary<string, string> player)
                                                                                {
                                                                                    if (player["PlType"] == "4" || player["PlType"] == "5" ||
                                                                                        player["PlType"] == "6")
                                                                                    {
                                                                                        if (((player["IsFA"] == "0" &&
                                                                                              player["TeamID1"] != "-1") ||
                                                                                             (player["IsFA"] == "1")) &&
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

                    string LastName = player["Last_Name"];
                    string FirstName = player["First_Name"];

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

                    var curPlayer = pst[playerID];
                    if (!activeTeamsIDs.Contains(pTeam) && player["IsFA"] != "1")
                    {
                        if (pst.ContainsKey(playerID) && (curPlayer.LastName == LastName) && curPlayer.FirstName == FirstName)
                        {
                            curPlayer.isActive = false;
                            curPlayer.TeamF = "";
                            curPlayer.isHidden = true;
                        }
                        continue;
                    }

                    int playerStatsID = Convert.ToInt32(player["StatY1"]);

                    Dictionary<string, string> plStats = playerStats.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == playerStatsID.ToString())
                                                                                  return true;
                                                                              return false;
                                                                          });

                    if (pst.ContainsKey(playerID) && (curPlayer.LastName != LastName || curPlayer.FirstName != FirstName))
                    {
                        List<KeyValuePair<int, PlayerStats>> candidates =
                            pst.Where(
                                pair => pair.Value.LastName == LastName && pair.Value.FirstName == FirstName && pair.Value.isHidden == false)
                               .ToList();
                        if (candidates.Count() > 0)
                        {
                            SortedDictionary<string, int> order = TeamOrder;
                            List<KeyValuePair<int, PlayerStats>> c2 = candidates.Where(pair => order.ContainsKey(pair.Value.TeamF)).ToList();
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
                                            team => team.Value.name == activeTeams.Find(ateam => ateam["ID"] == pTeam.ToString())["Name"]);

                                    List<KeyValuePair<int, PlayerStats>> c3 =
                                        candidates.Where(pair => pair.Value.TeamF == curTeam.Value.name).ToList();
                                    if (c3.Count == 1)
                                    {
                                        playerID = c3.First().Value.ID;
                                    }
                                    else
                                    {
                                        duplicatePlayers.Add(FirstName + " " + LastName);
                                        continue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            playerID = CreateNewPlayer(ref pst, player);
                        }
                    }
                    else if (!pst.ContainsKey(playerID))
                    {
                        playerID = CreateNewPlayer(ref pst, player, playerID);
                    }

                    curPlayer.isHidden = false;
                    curPlayer.YearsPro = Convert.ToInt32(player["YearsPro"]);
                    curPlayer.YearOfBirth = Convert.ToInt32(player["BirthYear"]);
                    curPlayer.height = Convert.ToDouble(player["Height"]);
                    curPlayer.weight = Convert.ToDouble(player["Weight"]);


                    if (plStats != null)
                    {
                        string TeamFName = "";
                        string TeamSName = "";
                        //string teamReal = pTeam.ToString();
                        string team1 = plStats["TeamID2"];
                        string team2 = plStats["TeamID1"];
                        bool hasBeenTraded = (team1 != "-1");

                        if (!hasBeenTraded)
                        {
                            team1 = team2;
                        }
                        if (team1 != "-1" && player["IsFA"] != "1")
                        {
                            Dictionary<string, string> TeamF = teams.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == team1)
                                                                                  return true;
                                                                              return false;
                                                                          });
                            TeamFName = TeamF["Name"];
                        }

                        if (hasBeenTraded)
                        {
                            Dictionary<string, string> TeamS = teams.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == team2)
                                                                                  return true;
                                                                              return false;
                                                                          });
                            TeamSName = TeamS["Name"];
                        }

                        PlayerStats ps = curPlayer;
                        ps.TeamF = TeamFName;
                        ps.TeamS = TeamSName;

                        ps.isActive = (player["IsFA"] != "1" && team1 != "-1");

                        ps.stats[p.GP] = Convert.ToUInt16(plStats["GamesP"]);
                        ps.stats[p.GS] = Convert.ToUInt16(plStats["GamesS"]);
                        ps.stats[p.MINS] = Convert.ToUInt16(plStats["Minutes"]);
                        ps.stats[p.PTS] = Convert.ToUInt16(plStats["Points"]);
                        ps.stats[p.DREB] = Convert.ToUInt16(plStats["DRebs"]);
                        ps.stats[p.OREB] = Convert.ToUInt16(plStats["ORebs"]);
                        ps.stats[p.AST] = Convert.ToUInt16(plStats["Assists"]);
                        ps.stats[p.STL] = Convert.ToUInt16(plStats["Steals"]);
                        ps.stats[p.BLK] = Convert.ToUInt16(plStats["Blocks"]);
                        ps.stats[p.TOS] = Convert.ToUInt16(plStats["TOs"]);
                        ps.stats[p.FOUL] = Convert.ToUInt16(plStats["Fouls"]);
                        ps.stats[p.FGM] = Convert.ToUInt16(plStats["FGMade"]);
                        ps.stats[p.FGA] = Convert.ToUInt16(plStats["FGAtt"]);
                        ps.stats[p.TPM] = Convert.ToUInt16(plStats["3PTMade"]);
                        ps.stats[p.TPA] = Convert.ToUInt16(plStats["3PTAtt"]);
                        ps.stats[p.FTM] = Convert.ToUInt16(plStats["FTMade"]);
                        ps.stats[p.FTA] = Convert.ToUInt16(plStats["FTAtt"]);

                        ps.isAllStar = Convert.ToBoolean(Convert.ToInt32(plStats["IsAStar"]));
                        ps.isNBAChampion = Convert.ToBoolean(Convert.ToInt32(plStats["IsChamp"]));

                        ps.isInjured = player["InjType"] != "0";

                        ps.CalcAvg();

                        pst[playerID] = ps;
                    }
                    else
                    {
                        PlayerStats ps = curPlayer;

                        string TeamFName = "";
                        string team1 = pTeam.ToString();
                        if (team1 != "-1" && player["IsFA"] != "1")
                        {
                            Dictionary<string, string> TeamF = teams.Find(delegate(Dictionary<string, string> s)
                                                                          {
                                                                              if (s["ID"] == team1)
                                                                                  return true;
                                                                              return false;
                                                                          });
                            TeamFName = TeamF["Name"];
                        }
                        ps.TeamF = TeamFName;

                        ps.isActive = player["IsFA"] != "1";
                        ps.isInjured = player["InjType"] != "0";

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
        private static int CreateNewPlayer(ref Dictionary<int, PlayerStats> pst, Dictionary<string, string> player, int preferredID = -1)
        {
            int playerID;
            if (preferredID == -1)
            {
                playerID = SQLiteIO.GetFreeID(MainWindow.currentDB,
                                              "Players" +
                                              (MainWindow.curSeason != SQLiteIO.getMaxSeason(MainWindow.currentDB)
                                                   ? "S" + MainWindow.curSeason
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
                                        ID = Convert.ToInt32(player["ID"]),
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
        private static BoxScoreEntry PrepareBoxScore(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> oldTST,
                                                     Dictionary<int, PlayerStats> pst, Dictionary<int, PlayerStats> oldPST, int t1, int t2)
        {
            bool isPlayoff = (tst[t1].getPlayoffGames() > 0);
            var bs = new TeamBoxScore
                     {
                         isPlayoff = isPlayoff,
                         Team1 = tst[t1].name,
                         MINS1 = getDiff(tst, oldTST, t1, t.MINS, isPlayoff),
                         Team2 = tst[t2].name,
                         MINS2 = getDiff(tst, oldTST, t2, t.MINS, isPlayoff)
                     };


            var bse = new BoxScoreEntry(bs);
            bse.pbsList = new List<PlayerBoxScore>();

            /*
                        var team1Players = pst.Where(pair => pair.Value.TeamF == bs.Team1);
                        var team2Players = pst.Where(pair => pair.Value.TeamF == bs.Team2);
                        */

            IEnumerable<KeyValuePair<int, PlayerStats>> bothTeamsPlayers =
                pst.Where(pair => pair.Value.TeamF == bs.Team1 || pair.Value.TeamF == bs.Team2);
            foreach (var playerKVP in bothTeamsPlayers)
            {
                KeyValuePair<int, PlayerStats> oldplayerKVP = oldPST.Single(pair => pair.Value.ID == playerKVP.Value.ID);

                PlayerStats newPlayer = playerKVP.Value;
                PlayerStats oldPlayer = oldplayerKVP.Value;

                PlayerBoxScore pbs;
                if (getDiff(newPlayer, oldPlayer, p.GP) == 1)
                {
                    pbs = new PlayerBoxScore
                          {
                              PlayerID = newPlayer.ID,
                              Team = newPlayer.TeamF,
                              isStarter = (getDiff(newPlayer, oldPlayer, p.GS) == 1),
                              playedInjured = newPlayer.isInjured,
                              MINS = getDiff(newPlayer, oldPlayer, p.MINS),
                              PTS = getDiff(newPlayer, oldPlayer, p.PTS),
                              OREB = getDiff(newPlayer, oldPlayer, p.OREB),
                              DREB = getDiff(newPlayer, oldPlayer, p.DREB),
                              AST = getDiff(newPlayer, oldPlayer, p.AST),
                              STL = getDiff(newPlayer, oldPlayer, p.STL),
                              BLK = getDiff(newPlayer, oldPlayer, p.BLK),
                              TOS = getDiff(newPlayer, oldPlayer, p.TOS),
                              FGM = getDiff(newPlayer, oldPlayer, p.FGM),
                              FGA = getDiff(newPlayer, oldPlayer, p.FGA),
                              TPM = getDiff(newPlayer, oldPlayer, p.TPM),
                              TPA = getDiff(newPlayer, oldPlayer, p.TPA),
                              FTM = getDiff(newPlayer, oldPlayer, p.FTM),
                              FTA = getDiff(newPlayer, oldPlayer, p.FTA),
                              FOUL = getDiff(newPlayer, oldPlayer, p.FOUL)
                          };
                    pbs.REB = (ushort) (pbs.OREB + pbs.DREB);
                    pbs.FGp = (float) pbs.FGM/pbs.FGA;
                    pbs.TPp = (float) pbs.TPM/pbs.TPA;
                    pbs.FTp = (float) pbs.FTM/pbs.FTA;
                }
                else
                {
                    pbs = new PlayerBoxScore {PlayerID = newPlayer.ID, Team = newPlayer.TeamF, isOut = true};
                }

                bse.pbsList.Add(pbs);
            }
            bse.date = DateTime.Today;
            bse.bs.gamedate = bse.date;
            bse.bs.SeasonNum = MainWindow.curSeason;

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
                       ? (ushort) (tst[teamID].stats[stat] - oldTST[teamID].stats[stat])
                       : (ushort) (tst[teamID].pl_stats[stat] - oldTST[teamID].pl_stats[stat]);
        }

        /// <summary>
        ///     Gets the difference of a player's stat's value between the current and previous stats.
        /// </summary>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="oldPST">The old player stats dictionary.</param>
        /// <param name="ID">The player's ID.</param>
        /// <param name="stat">The stat.</param>
        /// <returns></returns>
        private static ushort getDiff(Dictionary<int, PlayerStats> pst, Dictionary<int, PlayerStats> oldPST, int ID, int stat,
                                      bool isPlayoff = false)
        {
            return getDiff(pst[ID], oldPST[ID], stat, isPlayoff);
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
            return !isPlayoff ? (ushort) (newPS.stats[stat] - oldPS.stats[stat]) : (ushort) (newPS.pl_stats[stat] - oldPS.pl_stats[stat]);
        }

        /// <summary>
        ///     Exports all the team (and optionally player) stats and information to a set of CSV files, which can then be imported into REDitor.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstopp">The opposing team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="teamsOnly">
        ///     if set to <c>true</c>, only the teams' stats will be exported.
        /// </param>
        /// <returns></returns>
        public static int ExportAll(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstopp, Dictionary<int, PlayerStats> pst,
                                    string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;
            if (PopulateREditorDictionaryLists(folder, out teams, out players, out teamStats, out playerStats) == -1)
                return -1;

            foreach (int key in tst.Keys)
            {
                TeamStats ts = tst[key];
                TeamStats tsopp = tstopp[key];

                int id = ts.ID;

                int tindex = teams.FindIndex(delegate(Dictionary<string, string> s)
                                             {
                                                 if (s["ID"] == id.ToString())
                                                     return true;
                                                 return false;
                                             });

                Dictionary<string, string> team = teams[tindex];

                int sStatsID = Convert.ToInt32(team["StatCurS"]);
                int pStatsID = Convert.ToInt32(team["StatCurP"]);

                int sStatsIndex = teamStats.FindIndex(delegate(Dictionary<string, string> s)
                                                      {
                                                          if (s["ID"] == sStatsID.ToString())
                                                              return true;
                                                          return false;
                                                      });

                if (sStatsIndex != -1)
                {
                    teamStats[sStatsIndex]["Wins"] = ts.winloss[0].ToString();
                    teamStats[sStatsIndex]["Losses"] = ts.winloss[1].ToString();
                    teamStats[sStatsIndex]["Mins"] = ts.stats[t.MINS].ToString();
                    teamStats[sStatsIndex]["PtsFor"] = ts.stats[t.PF].ToString();
                    teamStats[sStatsIndex]["PtsAg"] = ts.stats[t.PA].ToString();
                    teamStats[sStatsIndex]["FGMade"] = ts.stats[t.FGM].ToString();
                    teamStats[sStatsIndex]["FGAtt"] = ts.stats[t.FGA].ToString();
                    teamStats[sStatsIndex]["3PTMade"] = ts.stats[t.TPM].ToString();
                    teamStats[sStatsIndex]["3PTAtt"] = ts.stats[t.TPA].ToString();
                    teamStats[sStatsIndex]["FTMade"] = ts.stats[t.FTM].ToString();
                    teamStats[sStatsIndex]["FTAtt"] = ts.stats[t.FTA].ToString();
                    teamStats[sStatsIndex]["DRebs"] = ts.stats[t.DREB].ToString();
                    teamStats[sStatsIndex]["ORebs"] = ts.stats[t.OREB].ToString();
                    teamStats[sStatsIndex]["Steals"] = ts.stats[t.STL].ToString();
                    teamStats[sStatsIndex]["Blocks"] = ts.stats[t.BLK].ToString();
                    teamStats[sStatsIndex]["Assists"] = ts.stats[t.AST].ToString();
                    teamStats[sStatsIndex]["Fouls"] = ts.stats[t.FOUL].ToString();
                    teamStats[sStatsIndex]["TOs"] = ts.stats[t.TOS].ToString();
                    //teamStats[sStatsIndex]["TOsAg"] = tsopp.stats[t.TO].ToString();
                }

                if (pStatsID != -1)
                {
                    int pStatsIndex = teamStats.FindIndex(delegate(Dictionary<string, string> s)
                                                          {
                                                              if (s["ID"] == pStatsID.ToString())
                                                                  return true;
                                                              return false;
                                                          });

                    if (pStatsIndex != -1)
                    {
                        teamStats[pStatsIndex]["Wins"] = ts.pl_winloss[0].ToString();
                        teamStats[pStatsIndex]["Losses"] = ts.pl_winloss[1].ToString();
                        teamStats[pStatsIndex]["Mins"] = ts.pl_stats[t.MINS].ToString();
                        teamStats[pStatsIndex]["PtsFor"] = ts.pl_stats[t.PF].ToString();
                        teamStats[pStatsIndex]["PtsAg"] = ts.pl_stats[t.PA].ToString();
                        teamStats[pStatsIndex]["FGMade"] = ts.pl_stats[t.FGM].ToString();
                        teamStats[pStatsIndex]["FGAtt"] = ts.pl_stats[t.FGA].ToString();
                        teamStats[pStatsIndex]["3PTMade"] = ts.pl_stats[t.TPM].ToString();
                        teamStats[pStatsIndex]["3PTAtt"] = ts.pl_stats[t.TPA].ToString();
                        teamStats[pStatsIndex]["FTMade"] = ts.pl_stats[t.FTM].ToString();
                        teamStats[pStatsIndex]["FTAtt"] = ts.pl_stats[t.FTA].ToString();
                        teamStats[pStatsIndex]["DRebs"] = ts.pl_stats[t.DREB].ToString();
                        teamStats[pStatsIndex]["ORebs"] = ts.pl_stats[t.OREB].ToString();
                        teamStats[pStatsIndex]["Steals"] = ts.pl_stats[t.STL].ToString();
                        teamStats[pStatsIndex]["Blocks"] = ts.pl_stats[t.BLK].ToString();
                        teamStats[pStatsIndex]["Assists"] = ts.pl_stats[t.AST].ToString();
                        teamStats[pStatsIndex]["Fouls"] = ts.pl_stats[t.FOUL].ToString();
                        teamStats[pStatsIndex]["TOs"] = ts.pl_stats[t.TOS].ToString();
                        //teamStats[pStatsIndex]["TOsAg"] = tsopp.stats[t.TO].ToString();
                    }
                }
            }

            if (!teamsOnly)
            {
                foreach (int key in pst.Keys)
                {
                    PlayerStats ps = pst[key];

                    int id = ps.ID;

                    int pindex = players.FindIndex(delegate(Dictionary<string, string> s)
                                                   {
                                                       if (s["ID"] == id.ToString())
                                                           return true;
                                                       return false;
                                                   });

                    Dictionary<string, string> player = players[pindex];

                    /*for (int i = 16; i >= 0; i--)
                    {
                        string cur = player["StatY" + i.ToString()];
                        if (cur != "-1") playerStatsID = Convert.ToInt32(cur);
                    }*/
                    int playerStatsID = Convert.ToInt32(player["StatY0"]);

                    int playerStatsIndex = playerStats.FindIndex(delegate(Dictionary<string, string> s)
                                                                 {
                                                                     if (s["ID"] == playerStatsID.ToString())
                                                                         return true;
                                                                     return false;
                                                                 });

                    if (playerStatsIndex != -1)
                    {
                        playerStats[playerStatsIndex]["GamesP"] = ps.stats[p.GP].ToString();
                        playerStats[playerStatsIndex]["GamesS"] = ps.stats[p.GS].ToString();
                        playerStats[playerStatsIndex]["Minutes"] = ps.stats[p.MINS].ToString();
                        playerStats[playerStatsIndex]["Points"] = ps.stats[p.PTS].ToString();
                        playerStats[playerStatsIndex]["DRebs"] = ps.stats[p.DREB].ToString();
                        playerStats[playerStatsIndex]["ORebs"] = ps.stats[p.OREB].ToString();
                        playerStats[playerStatsIndex]["Assists"] = ps.stats[p.AST].ToString();
                        playerStats[playerStatsIndex]["Steals"] = ps.stats[p.STL].ToString();
                        playerStats[playerStatsIndex]["Blocks"] = ps.stats[p.BLK].ToString();
                        playerStats[playerStatsIndex]["TOs"] = ps.stats[p.TOS].ToString();
                        playerStats[playerStatsIndex]["Fouls"] = ps.stats[p.FOUL].ToString();
                        playerStats[playerStatsIndex]["FGMade"] = ps.stats[p.FGM].ToString();
                        playerStats[playerStatsIndex]["FGAtt"] = ps.stats[p.FGA].ToString();
                        playerStats[playerStatsIndex]["3PTMade"] = ps.stats[p.TPM].ToString();
                        playerStats[playerStatsIndex]["3PTAtt"] = ps.stats[p.TPA].ToString();
                        playerStats[playerStatsIndex]["FTMade"] = ps.stats[p.FTM].ToString();
                        playerStats[playerStatsIndex]["FTAtt"] = ps.stats[p.FTA].ToString();
                        playerStats[playerStatsIndex]["IsAStar"] = (ps.isAllStar ? 1 : 0).ToString();
                        playerStats[playerStatsIndex]["IsChamp"] = (ps.isNBAChampion ? 1 : 0).ToString();
                    }
                }
            }

            string path = folder + @"\Team_Stats.csv";
            CSV.CSVFromDictionaryList(teamStats, path);
            if (!teamsOnly)
            {
                path = folder + @"\Player_Stats.csv";
                CSV.CSVFromDictionaryList(playerStats, path);
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
        private static int PopulateREditorDictionaryLists(string folder, out List<Dictionary<string, string>> teams,
                                                          out List<Dictionary<string, string>> players,
                                                          out List<Dictionary<string, string>> teamStats,
                                                          out List<Dictionary<string, string>> playerStats)
        {
            try
            {
                teams = CSV.DictionaryListFromCSV(folder + @"\Teams.csv");
                players = CSV.DictionaryListFromCSV(folder + @"\Players.csv");
                teamStats = CSV.DictionaryListFromCSV(folder + @"\Team_Stats.csv");
                playerStats = CSV.DictionaryListFromCSV(folder + @"\Player_Stats.csv");
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