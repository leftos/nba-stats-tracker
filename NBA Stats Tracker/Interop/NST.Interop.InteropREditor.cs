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

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using NBA_Stats_Tracker.Data;

namespace NBA_Stats_Tracker.Interop
{
    public class InteropREditor
    {
        private static Dictionary<string, string> Positions = new Dictionary<string, string>
                                                               {
                                                                   {"0", "PG"},
                                                                   {"1", "SG"},
                                                                   {"2", "SF"},
                                                                   {"3", "PF"},
                                                                   {"4", "C"},
                                                                   {"5", " "}
                                                               }; 

        private static List<Dictionary<string, string>> ConvertTSVtoDictionary(string[] TSV)
        {
            List<Dictionary<string, string>> dictList = new List<Dictionary<string, string>> {new Dictionary<string, string>()};
            string[] headers = TSV[0].Split('\t');
            for (int i = 1; i < TSV.Length; i++)
            {
                string[] values = TSV[i].Split('\t');
                dictList.Add(new Dictionary<string, string>());
                for (int index = 0; index < headers.Length; index++)
                {
                    dictList[i-1][headers[index]] = values[index];
                }
            }

            return dictList;
        }

        private static List<Dictionary<string, string>> CreateDictionaryList(string path)
        {
            string[] TSV = File.ReadAllLines(path);
            return ConvertTSVtoDictionary(TSV);
        }

        public static int ImportAll(ref Dictionary<int, TeamStats> tst, ref Dictionary<int, TeamStats> tstopp, ref SortedDictionary<string, int> TeamOrder, ref Dictionary<int, PlayerStats> pst, string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;
            try
            {
                teams = CreateDictionaryList(folder + @"\Teams.tsv");
                players = CreateDictionaryList(folder + @"\Players.tsv");
                teamStats = CreateDictionaryList(folder + @"\Team_Stats.tsv");
                playerStats = CreateDictionaryList(folder + @"\Player_Stats.tsv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return -1;
            }

            #region Import Teams & Team Stats
            var activeTeams = teams.FindAll(delegate(Dictionary<string, string> team)
            {
                if (team["StatCurS"] != "-1") return true;
                return false;
            });

            if (activeTeams.Count == 0)
            {
                MessageBox.Show("No Team Stats found in save.");
                return -1;
            }

            bool madeNew = false;

            if (tst.Count != 30)
            {
                tst = new Dictionary<int, TeamStats>();
                tstopp = new Dictionary<int, TeamStats>();
                madeNew = true;
            }
            List<int> activeTeamsIDs = new List<int>();
            Dictionary<int, List<int>> rosters = new Dictionary<int, List<int>>();
            foreach (Dictionary<string, string> team in activeTeams)
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

                var sStats = teamStats.Find(delegate (Dictionary<string,string> s)
                                                {
                                                    if (s["ID"] == sStatsID.ToString()) return true;
                                                    return false;
                                                });
                tst[id].ID = Convert.ToInt32(team["ID"]);
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
                tst[id].stats[t.TO] = Convert.ToUInt16(sStats["TOs"]);
                //tstopp[id].stats[t.TO] = Convert.ToUInt16(sStats["TOsAg"]);

                if (pStatsID != -1)
                {
                    var pStats = teamStats.Find(delegate (Dictionary<string,string> s)
                                                {
                                                    if (s["ID"] == pStatsID.ToString()) return true;
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
                    tst[id].pl_stats[t.TO] = Convert.ToUInt16(pStats["TOs"]);
                    //tstopp[id].pl_stats[t.TO] = Convert.ToUInt16(pStats["TOsAg"]);
                }
                tst[id].calcAvg();

                rosters[id].Add(Convert.ToInt32(team["Ros_PG"]));
                rosters[id].Add(Convert.ToInt32(team["Ros_SG"]));
                rosters[id].Add(Convert.ToInt32(team["Ros_SF"]));
                rosters[id].Add(Convert.ToInt32(team["Ros_PG"]));
                rosters[id].Add(Convert.ToInt32(team["Ros_PG"]));
                for (int i = 6; i <= 20; i++)
                {
                    int cur = Convert.ToInt32(team["Ros_S" + i.ToString()]);
                    if (cur != -1) rosters[id].Add(cur);
                    else break;
                }
            }

            #endregion

            #region Import Players & Player Stats
            if (!teamsOnly)
            {
                var activePlayers = players.FindAll(delegate(Dictionary<string, string> player)
                                                        {
                                                            if (player["PlType"] == "4")
                                                            {
                                                                if ((player["IsFA"] == "0" && player["TeamID1"] != "-1") ||
                                                                    (player["IsFA"] == "1"))
                                                                {
                                                                    return true;
                                                                }
                                                            }
                                                            return false;
                                                        });

                foreach (var player in activePlayers)
                {
                    int playerStatsID = -1;
                    /*
                    for (int i = 16; i >= 0; i--)
                    {
                        string cur = player["StatY" + i.ToString()];
                        if (cur != "-1") playerStatsID = Convert.ToInt32(cur);
                    }
                    */
                    playerStatsID = Convert.ToInt32(player["StatY0"]);

                    //TODO: Handle this a bit more gracefully
                    if (playerStatsID == -1) continue;

                    var plStats = playerStats.Find(delegate(Dictionary<string, string> s)
                                                       {
                                                           if (s["ID"] == playerStatsID.ToString()) return true;
                                                           return false;
                                                       });

                    int playerID = Convert.ToInt32(player["ID"]);

                    if (!pst.ContainsKey(playerID))
                    {
                        pst.Add(playerID, new PlayerStats(new Player
                                                              {
                                                                  ID = Convert.ToInt32(player["ID"]),
                                                                  FirstName = player["First_Name"],
                                                                  LastName = player["Last_Name"],
                                                                  Position = Positions[player["Pos"]],
                                                                  Position2 = Positions[player["SecondPos"]]
                                                              }));
                    }

                    string TeamFName = "";
                    string team1 = plStats["TeamID1"];
                    if (team1 != "-1")
                    {
                        var TeamF = teams.Find(delegate(Dictionary<string, string> s)
                                                   {
                                                       if (s["ID"] == team1) return true;
                                                       return false;
                                                   });
                        TeamFName = TeamF["Name"];
                    }

                    string TeamSName = "";
                    string team2 = plStats["TeamID2"];
                    if (team2 != "-1")
                    {
                        var TeamS = teams.Find(delegate(Dictionary<string, string> s)
                                                   {
                                                       if (s["ID"] == team2) return true;
                                                       return false;
                                                   });
                        TeamSName = TeamS["Name"];
                    }

                    PlayerStats ps = pst[playerID];
                    ps.TeamF = TeamFName;
                    ps.TeamS = TeamSName;

                    if (team1 == "-1")
                    {
                        ps.isActive = false;
                    }
                    else
                    {
                        ps.isActive = true;
                    }

                    ps.stats[p.GP] = Convert.ToUInt16(plStats["GamesP"]);
                    ps.stats[p.GS] = Convert.ToUInt16(plStats["GamesS"]);
                    ps.stats[p.MINS] = Convert.ToUInt16(plStats["Minutes"]);
                    ps.stats[p.PTS] = Convert.ToUInt16(plStats["Points"]);
                    ps.stats[p.DREB] = Convert.ToUInt16(plStats["DRebs"]);
                    ps.stats[p.OREB] = Convert.ToUInt16(plStats["ORebs"]);
                    ps.stats[p.AST] = Convert.ToUInt16(plStats["Assists"]);
                    ps.stats[p.STL] = Convert.ToUInt16(plStats["Steals"]);
                    ps.stats[p.BLK] = Convert.ToUInt16(plStats["Blocks"]);
                    ps.stats[p.TO] = Convert.ToUInt16(plStats["TOs"]);
                    ps.stats[p.FOUL] = Convert.ToUInt16(plStats["Fouls"]);
                    ps.stats[p.FGM] = Convert.ToUInt16(plStats["FGMade"]);
                    ps.stats[p.FGA] = Convert.ToUInt16(plStats["FGAtt"]);
                    ps.stats[p.TPM] = Convert.ToUInt16(plStats["3PTMade"]);
                    ps.stats[p.TPA] = Convert.ToUInt16(plStats["3PTAtt"]);
                    ps.stats[p.FTM] = Convert.ToUInt16(plStats["FTMade"]);
                    ps.stats[p.FTA] = Convert.ToUInt16(plStats["FTAtt"]);

                    ps.isAllStar = Convert.ToBoolean(plStats["isAStar"]);
                    ps.isNBAChampion = Convert.ToBoolean(plStats["isChamp"]);

                    if (player["InjType"] != "0") ps.isInjured = true;
                    else ps.isInjured = false;

                    ps.CalcAvg();

                    pst[playerID] = ps;
                }
            }

            #endregion

            return 0;
        }

        public static int ExportAll(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstopp, Dictionary<int, PlayerStats> pst, string folder, bool teamsOnly = false)
        {
            List<Dictionary<string, string>> teams;
            List<Dictionary<string, string>> players;
            List<Dictionary<string, string>> teamStats;
            List<Dictionary<string, string>> playerStats;
            try
            {
                teams = CreateDictionaryList(folder + @"\Teams.tsv");
                players = CreateDictionaryList(folder + @"\Players.tsv");
                teamStats = CreateDictionaryList(folder + @"\Team_Stats.tsv");
                playerStats = CreateDictionaryList(folder + @"\Player_Stats.tsv");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return -1;
            }

            foreach (var key in tst.Keys)
            {
                TeamStats ts = tst[key];
                TeamStats tsopp = tstopp[key];

                int id = ts.ID;

                int tindex = teams.FindIndex(delegate(Dictionary<string, string> s)
                                                 {
                                                     if (s["ID"] == id.ToString()) return true;
                                                     return false;
                                                 });

                var team = teams[tindex];

                int sStatsID = Convert.ToInt32(team["StatCurS"]);
                int pStatsID = Convert.ToInt32(team["StatCurP"]);

                var sStatsIndex = teamStats.FindIndex(delegate(Dictionary<string, string> s)
                {
                    if (s["ID"] == sStatsID.ToString()) return true;
                    return false;
                });

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
                teamStats[sStatsIndex]["TOs"] = ts.stats[t.TO].ToString();
                //teamStats[sStatsIndex]["TOsAg"] = tsopp.stats[t.TO].ToString();

                if (pStatsID != -1)
                {
                    var pStatsIndex = teamStats.FindIndex(delegate(Dictionary<string, string> s)
                    {
                        if (s["ID"] == pStatsID.ToString()) return true;
                        return false;
                    });

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
                    teamStats[pStatsIndex]["TOs"] = ts.pl_stats[t.TO].ToString();
                    //teamStats[pStatsIndex]["TOsAg"] = tsopp.stats[t.TO].ToString();
                }
            }

            if (!teamsOnly)
            {
                foreach (var key in pst.Keys)
                {
                    PlayerStats ps = pst[key];

                    int id = ps.ID;

                    int pindex = players.FindIndex(delegate(Dictionary<string, string> s)
                                                       {
                                                           if (s["ID"] == id.ToString()) return true;
                                                           return false;
                                                       });

                    var player = players[pindex];

                    int playerStatsID = -1;
                    /*for (int i = 16; i >= 0; i--)
                    {
                        string cur = player["StatY" + i.ToString()];
                        if (cur != "-1") playerStatsID = Convert.ToInt32(cur);
                    }*/
                    playerStatsID = Convert.ToInt32(player["StatsY0"]);

                    int playerStatsIndex = playerStats.FindIndex(delegate(Dictionary<string, string> s)
                                                                     {
                                                                         if (s["ID"] == playerStatsID.ToString())
                                                                             return true;
                                                                         return false;
                                                                     });

                    playerStats[playerStatsIndex]["GamesP"] = ps.stats[p.GP].ToString();
                    playerStats[playerStatsIndex]["GamesS"] = ps.stats[p.GS].ToString();
                    playerStats[playerStatsIndex]["Minutes"] = ps.stats[p.MINS].ToString();
                    playerStats[playerStatsIndex]["Points"] = ps.stats[p.PTS].ToString();
                    playerStats[playerStatsIndex]["DRebs"] = ps.stats[p.DREB].ToString();
                    playerStats[playerStatsIndex]["ORebs"] = ps.stats[p.OREB].ToString();
                    playerStats[playerStatsIndex]["Assists"] = ps.stats[p.AST].ToString();
                    playerStats[playerStatsIndex]["Steals"] = ps.stats[p.STL].ToString();
                    playerStats[playerStatsIndex]["Blocks"] = ps.stats[p.BLK].ToString();
                    playerStats[playerStatsIndex]["TOs"] = ps.stats[p.TO].ToString();
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

            return 0;
        }
    }
}