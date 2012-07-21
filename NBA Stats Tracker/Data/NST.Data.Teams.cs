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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Windows;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Data
{
    public class TeamBoxScore
    {
        public TeamBoxScore()
        {
            id = -1;
            bshistid = -1;
        }

        public TeamBoxScore(DataRow r)
        {
            id = Convert.ToInt32(r["GameID"].ToString());
            Team1 = r["T1Name"].ToString();
            Team2 = r["T2Name"].ToString();
            gamedate = Convert.ToDateTime(r["Date"].ToString());
            SeasonNum = Convert.ToInt32(r["SeasonNum"].ToString());
            isPlayoff = Convert.ToBoolean(r["IsPlayoff"].ToString());
            PTS1 = Convert.ToUInt16(r["T1PTS"].ToString());
            REB1 = Convert.ToUInt16(r["T1REB"].ToString());
            AST1 = Convert.ToUInt16(r["T1AST"].ToString());
            STL1 = Convert.ToUInt16(r["T1STL"].ToString());
            BLK1 = Convert.ToUInt16(r["T1BLK"].ToString());
            TO1 = Convert.ToUInt16(r["T1TOS"].ToString());
            FGM1 = Convert.ToUInt16(r["T1FGM"].ToString());
            FGA1 = Convert.ToUInt16(r["T1FGA"].ToString());
            TPM1 = Convert.ToUInt16(r["T13PM"].ToString());
            TPA1 = Convert.ToUInt16(r["T13PA"].ToString());
            FTM1 = Convert.ToUInt16(r["T1FTM"].ToString());
            FTA1 = Convert.ToUInt16(r["T1FTA"].ToString());
            OREB1 = Convert.ToUInt16(r["T1OREB"].ToString());
            FOUL1 = Convert.ToUInt16(r["T1FOUL"].ToString());
            MINS1 = Convert.ToUInt16(r["T1MINS"].ToString());

            PTS2 = Convert.ToUInt16(r["T2PTS"].ToString());
            REB2 = Convert.ToUInt16(r["T2REB"].ToString());
            AST2 = Convert.ToUInt16(r["T2AST"].ToString());
            STL2 = Convert.ToUInt16(r["T2STL"].ToString());
            BLK2 = Convert.ToUInt16(r["T2BLK"].ToString());
            TO2 = Convert.ToUInt16(r["T2TOS"].ToString());
            FGM2 = Convert.ToUInt16(r["T2FGM"].ToString());
            FGA2 = Convert.ToUInt16(r["T2FGA"].ToString());
            TPM2 = Convert.ToUInt16(r["T23PM"].ToString());
            TPA2 = Convert.ToUInt16(r["T23PA"].ToString());
            FTM2 = Convert.ToUInt16(r["T2FTM"].ToString());
            FTA2 = Convert.ToUInt16(r["T2FTA"].ToString());
            OREB2 = Convert.ToUInt16(r["T2OREB"].ToString());
            FOUL2 = Convert.ToUInt16(r["T2FOUL"].ToString());
            MINS2 = Convert.ToUInt16(r["T2MINS"].ToString());
        }

        public TeamBoxScore(DataSet ds, string[] parts)
        {
            DataTable away = ds.Tables[0];
            DataTable home = ds.Tables[1];

            int done = 0;
            foreach (var team in MainWindow.TeamOrder)
            {
                if (parts[0].Contains(team.Key))
                {
                    Team1 = team.Key;
                    done++;
                }
                if (parts[1].Contains(team.Key))
                {
                    Team2 = team.Key;
                    done++;
                }
                if (done == 2) break;
            }
            if (done != 2)
            {
                Team1 = "$$Invalid";
                Team2 = "$$Invalid";
                return;
            }
            string date = parts[2].Trim() + ", " + parts[3].Trim();
            gamedate = Convert.ToDateTime(date);

            id = SQLiteIO.GetFreeID(MainWindow.currentDB, "GameResults", "GameID");
            SeasonNum = MainWindow.curSeason;

            DataRow rt = away.Rows[away.Rows.Count - 1];
            PTS1 = Tools.getUInt16(rt, "PTS");
            REB1 = Convert.ToUInt16(rt["TRB"].ToString());
            AST1 = Convert.ToUInt16(rt["AST"].ToString());
            STL1 = Convert.ToUInt16(rt["STL"].ToString());
            BLK1 = Convert.ToUInt16(rt["BLK"].ToString());
            TO1 = Convert.ToUInt16(rt["TOV"].ToString());
            FGM1 = Convert.ToUInt16(rt["FG"].ToString());
            FGA1 = Convert.ToUInt16(rt["FGA"].ToString());
            TPM1 = Convert.ToUInt16(rt["3P"].ToString());
            TPA1 = Convert.ToUInt16(rt["3PA"].ToString());
            FTM1 = Convert.ToUInt16(rt["FT"].ToString());
            FTA1 = Convert.ToUInt16(rt["FTA"].ToString());
            OREB1 = Convert.ToUInt16(rt["ORB"].ToString());
            FOUL1 = Convert.ToUInt16(rt["PF"].ToString());
            MINS1 = (ushort) (Convert.ToUInt16(rt["MP"].ToString())/5);

            rt = home.Rows[home.Rows.Count - 1];
            PTS2 = Tools.getUInt16(rt, "PTS");
            REB2 = Convert.ToUInt16(rt["TRB"].ToString());
            AST2 = Convert.ToUInt16(rt["AST"].ToString());
            STL2 = Convert.ToUInt16(rt["STL"].ToString());
            BLK2 = Convert.ToUInt16(rt["BLK"].ToString());
            TO2 = Convert.ToUInt16(rt["TOV"].ToString());
            FGM2 = Convert.ToUInt16(rt["FG"].ToString());
            FGA2 = Convert.ToUInt16(rt["FGA"].ToString());
            TPM2 = Convert.ToUInt16(rt["3P"].ToString());
            TPA2 = Convert.ToUInt16(rt["3PA"].ToString());
            FTM2 = Convert.ToUInt16(rt["FT"].ToString());
            FTA2 = Convert.ToUInt16(rt["FTA"].ToString());
            OREB2 = Convert.ToUInt16(rt["ORB"].ToString());
            FOUL2 = Convert.ToUInt16(rt["PF"].ToString());
            MINS2 = (ushort) (Convert.ToUInt16(rt["MP"].ToString())/5);
        }

        public UInt16 AST1 { get; set; }
        public UInt16 AST2 { get; set; }
        public UInt16 BLK1 { get; set; }
        public UInt16 BLK2 { get; set; }
        public UInt16 FGA1 { get; set; }
        public UInt16 FGA2 { get; set; }
        public UInt16 FGM1 { get; set; }
        public UInt16 FGM2 { get; set; }
        public UInt16 FTA1 { get; set; }
        public UInt16 FTA2 { get; set; }
        public UInt16 FTM1 { get; set; }
        public UInt16 FTM2 { get; set; }
        public UInt16 MINS1 { get; set; }
        public UInt16 MINS2 { get; set; }
        public UInt16 OREB1 { get; set; }
        public UInt16 OREB2 { get; set; }
        public UInt16 FOUL1 { get; set; }
        public UInt16 FOUL2 { get; set; }
        public UInt16 PTS1 { get; set; }
        public UInt16 PTS2 { get; set; }
        public UInt16 REB1 { get; set; }
        public UInt16 REB2 { get; set; }
        public UInt16 STL1 { get; set; }
        public UInt16 STL2 { get; set; }
        public int SeasonNum { get; set; }
        public UInt16 TO1 { get; set; }
        public UInt16 TO2 { get; set; }
        public UInt16 TPA1 { get; set; }
        public UInt16 TPA2 { get; set; }
        public UInt16 TPM1 { get; set; }
        public UInt16 TPM2 { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public int bshistid { get; set; }
        public bool doNotUpdate { get; set; }
        public bool done { get; set; }
        public DateTime gamedate { get; set; }
        public int id { get; set; }
        public bool isPlayoff { get; set; }

        public string DisplayTeam { get; set; }
        public string DisplayOpponent { get; set; }
        public string DisplayResult { get; set; }
        public string DisplayLocation { get; set; }

        public void Prepare(string team)
        {
            if (team == Team1)
            {
                DisplayTeam = Team1;
                DisplayOpponent = Team2;
                DisplayLocation = "Away";
                if (PTS1 > PTS2)
                {
                    DisplayResult = "W ";
                }
                else
                {
                    DisplayResult = "L ";
                }
            }
            else
            {
                DisplayTeam = Team2;
                DisplayOpponent = Team1;
                DisplayLocation = "Home";
                if (PTS1 < PTS2)
                {
                    DisplayResult = "W ";
                }
                else
                {
                    DisplayResult = "L ";
                }
            }
            DisplayResult += PTS1 + "-" + PTS2;
        }
    }

    public static class t
    {
        public const int MINS = 0,
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
    }

    public class TeamStats
    {
        public int ID;

        /// <summary>
        /// Averages for each team.
        /// 0: PPG, 1: PAPG, 2: FG%, 3: FGEff, 4: 3P%, 5: 3PEff, 6: FT%, 7:FTEff,
        /// 8: RPG, 9: ORPG, 10: DRPG, 11: SPG, 12: BPG, 13: TPG, 14: APG, 15: FPG, 16: W%,
        /// 17: Weff, 18: PD
        /// </summary>
        public float[] averages = new float[19];

        public string displayName;
        public bool isHidden;
        public Dictionary<string, double> metrics = new Dictionary<string, double>();

        public string name;
        public int offset;

        public float[] pl_averages = new float[19];
        public int pl_offset;
        public uint[] pl_stats = new uint[18];
        public uint[] pl_winloss = new uint[2];

        /// <summary>
        /// Stats for each team.
        /// 0: M, 1: PF, 2: PA, 3: 0x0000, 4: FGM, 5: FGA, 6: 3PM, 7: 3PA, 8: FTM, 9: FTA,
        /// 10: OREB, 11: DREB, 12: STL, 13: TO, 14: BLK, 15: AST,
        /// 16: FOUL
        /// </summary>
        public uint[] stats = new uint[18];
        public uint[] winloss = new uint[2];

        public TeamStats()
        {
            prepareEmpty();
        }

        public TeamStats(string name) : this()
        {
            this.name = name;
            displayName = name;
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
            isHidden = false;
        }

        public void calcAvg()
        {
            uint games = winloss[0] + winloss[1];
            uint pl_games = pl_winloss[0] + pl_winloss[1];

            averages[t.Wp] = (float) winloss[0]/games;
            averages[t.Weff] = averages[t.Wp]*winloss[0];
            averages[t.PPG] = (float) stats[t.PF]/games;
            averages[t.PAPG] = (float) stats[t.PA]/games;
            averages[t.FGp] = (float) stats[t.FGM]/stats[t.FGA];
            averages[t.FGeff] = averages[t.FGp]*((float) stats[t.FGM]/games);
            averages[t.TPp] = (float) stats[t.TPM]/stats[t.TPA];
            averages[t.TPeff] = averages[t.TPp]*((float) stats[t.TPM]/games);
            averages[t.FTp] = (float) stats[t.FTM]/stats[t.FTA];
            averages[t.FTeff] = averages[t.FTp]*((float) stats[t.FTM]/games);
            averages[t.RPG] = (float) (stats[t.OREB] + stats[t.DREB])/games;
            averages[t.ORPG] = (float) stats[t.OREB]/games;
            averages[t.DRPG] = (float) stats[t.DREB]/games;
            averages[t.SPG] = (float) stats[t.STL]/games;
            averages[t.BPG] = (float) stats[t.BLK]/games;
            averages[t.TPG] = (float) stats[t.TO]/games;
            averages[t.APG] = (float) stats[t.AST]/games;
            averages[t.FPG] = (float) stats[t.FOUL]/games;
            averages[t.PD] = averages[t.PPG] - averages[t.PAPG];

            pl_averages[t.Wp] = (float) pl_winloss[0]/pl_games;
            pl_averages[t.Weff] = pl_averages[t.Wp]*pl_winloss[0];
            pl_averages[t.PPG] = (float) pl_stats[t.PF]/pl_games;
            pl_averages[t.PAPG] = (float) pl_stats[t.PA]/pl_games;
            pl_averages[t.FGp] = (float) pl_stats[t.FGM]/pl_stats[t.FGA];
            pl_averages[t.FGeff] = pl_averages[t.FGp]*((float) pl_stats[t.FGM]/pl_games);
            pl_averages[t.TPp] = (float) pl_stats[t.TPM]/pl_stats[t.TPA];
            pl_averages[t.TPeff] = pl_averages[t.TPp]*((float) pl_stats[t.TPM]/pl_games);
            pl_averages[t.FTp] = (float) pl_stats[t.FTM]/pl_stats[t.FTA];
            pl_averages[t.FTeff] = pl_averages[t.FTp]*((float) pl_stats[t.FTM]/pl_games);
            pl_averages[t.RPG] = (float) (pl_stats[t.OREB] + pl_stats[t.DREB])/pl_games;
            pl_averages[t.ORPG] = (float) pl_stats[t.OREB]/pl_games;
            pl_averages[t.DRPG] = (float) pl_stats[t.DREB]/pl_games;
            pl_averages[t.SPG] = (float) pl_stats[t.STL]/pl_games;
            pl_averages[t.BPG] = (float) pl_stats[t.BLK]/pl_games;
            pl_averages[t.TPG] = (float) pl_stats[t.TO]/pl_games;
            pl_averages[t.APG] = (float) pl_stats[t.AST]/pl_games;
            pl_averages[t.FPG] = (float) pl_stats[t.FOUL]/pl_games;
            pl_averages[t.PD] = pl_averages[t.PPG] - pl_averages[t.PAPG];
        }

        public static TeamStats CalculateLeagueAverages(Dictionary<int, TeamStats> tst, string statRange)
        {
            TeamStats ls = new TeamStats("League");
            uint teamCount = CountTeams(tst, statRange);
            for (int i = 0; i < tst.Count; i++)
            {
                ls.AddTeamStats(tst[i], statRange);
            }
            ls.CalcMetrics(ls);

            ls.winloss[0] /= teamCount;
            ls.winloss[1] /= teamCount;
            ls.pl_winloss[0] /= teamCount;
            ls.pl_winloss[1] /= teamCount;
            ls.averages[t.Weff] /= teamCount;
            ls.pl_averages[t.Weff] /= teamCount;

            return ls;
        }

        public static void CalculateAllMetrics(ref Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstopp)
        {
            var temp = new Dictionary<int, TeamStats>();
            var tempopp= new Dictionary<int, TeamStats>();
            var teamCount = CountTeams(tst, "All");

            for (int i = 0; i < tst.Count; i++)
            {
                temp[i] = new TeamStats();
                temp[i].AddTeamStats(tst[i], "All");
                tempopp[i] = new TeamStats();
                tempopp[i].AddTeamStats(tstopp[i], "All");

                temp[i].CalcMetrics(tempopp[i]);
                tst[i].metrics = new Dictionary<string, double>(temp[i].metrics);
            }
        }

        private static uint CountTeams(Dictionary<int, TeamStats> tst, string statRange)
        {
            uint teamCount = 0;

            if (statRange != "Playoffs")
            {
                for (int i = 0; i < tst.Count; i++)
                {
                    if (tst[i].getGames() > 0) teamCount++;
                }
            }
            else
            {
                for (int i = 0; i < tst.Count; i++)
                {
                    if (tst[i].getPlayoffGames() > 0) teamCount++;
                }
            }
            return (teamCount!=0) ? teamCount : 1;
        }

        public void CalcMetrics(TeamStats tsopp)
        {
            metrics = new Dictionary<string, double>();

            var tstats = new double[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                tstats[i] = stats[i];
            }

            var toppstats = new double[tsopp.stats.Length];
            for (int i = 0; i < tsopp.stats.Length; i++)
            {
                toppstats[i] = stats[i];
            }

            double Poss = GetPossMetric(tstats, toppstats);
            metrics.Add("Poss", Poss);
            metrics.Add("PossPG", Poss/getGames());
            
            Poss = GetPossMetric(toppstats, tstats);
            try { tsopp.metrics.Add("Poss", Poss); }
            catch
            {
            }
            
            double Pace = 48*((metrics["Poss"] + tsopp.metrics["Poss"])/(2*(tstats[t.MINS])));
            metrics.Add("Pace", Pace);

            double ORTG = (tstats[t.PF]/Poss) * 100;
            metrics.Add("ORTG", ORTG);

            double DRTG = (tstats[t.PA]/Poss)*100;
            metrics.Add("DRTG", DRTG);

            double ASTp = 100*(tstats[t.AST])/(tstats[t.FGA] + tstats[t.FTA]*0.44 + tstats[t.AST] + tstats[t.TO]);
            metrics.Add("AST%", ASTp);

            double DREBp = 100*tstats[t.DREB]/(tstats[t.DREB] + toppstats[t.OREB]);
            metrics.Add("DREB%", DREBp);

            double EFGp = 100*(tstats[t.FGM] + tstats[t.TPM]*0.5)/tstats[t.FGA];
            metrics.Add("EFG%", EFGp);

            double EFFd = ORTG - DRTG;
            metrics.Add("EFFd", EFFd);

            double TOR = tstats[t.TO]/(tstats[t.FGA] + 0.44*tstats[t.FTA] + tstats[t.TO]);
            metrics.Add("TOR", TOR);

            double OREBp = 100*tstats[t.OREB]/(tstats[t.OREB] + toppstats[t.DREB]);
            metrics.Add("OREB%", OREBp);

            double FTR = tstats[t.FTM]/tstats[t.FGA];
            metrics.Add("FTR", FTR);

            double PWp = (((averages[t.PPG] - averages[t.PAPG])*2.7) + 41)/82;
            metrics.Add("PW%", PWp);
        }

        private static double GetPossMetric(double[] tstats, double[] toppstats)
        {
            double Poss = 0.5*
                          ((tstats[t.FGA] + 0.4*tstats[t.FTA] -
                            1.07*(tstats[t.OREB]/(tstats[t.OREB] + toppstats[t.DREB]))*
                            (tstats[t.FGA] - tstats[t.FGM]) + tstats[t.TO]) +
                           (toppstats[t.FGA] + 0.4*toppstats[t.FTA] -
                            1.07*(toppstats[t.OREB]/(toppstats[t.OREB] + tstats[t.DREB]))*
                            (toppstats[t.FGA] - toppstats[t.FGM]) + toppstats[t.TO]));
            return Poss;
        }

        internal uint getGames()
        {
            uint games = winloss[0] + winloss[1];
            return games;
        }

        internal uint getPlayoffGames()
        {
            uint pl_games = pl_winloss[0] + pl_winloss[1];
            return pl_games;
        }

        public void AddTeamStats(TeamStats ts, string mode)
        {
            switch (mode)
            {
                case "Season":
                    {
                        winloss[0] += ts.winloss[0];
                        winloss[1] += ts.winloss[1];

                        for (int i = 0; i < stats.Length; i++)
                        {
                            stats[i] += ts.stats[i];
                        }

                        calcAvg();
                        break;
                    }
                case "Playoffs":
                    {
                        pl_winloss[0] += ts.pl_winloss[0];
                        pl_winloss[1] += ts.pl_winloss[1];

                        for (int i = 0; i < pl_stats.Length; i++)
                        {
                            pl_stats[i] += ts.pl_stats[i];
                        }

                        calcAvg();
                        break;
                    }
                case "All":
                    {
                        winloss[0] += ts.winloss[0];
                        winloss[1] += ts.winloss[1];

                        for (int i = 0; i < stats.Length; i++)
                        {
                            stats[i] += ts.stats[i];
                        }

                        winloss[0] += ts.pl_winloss[0];
                        winloss[1] += ts.pl_winloss[1];

                        for (int i = 0; i < pl_stats.Length; i++)
                        {
                            stats[i] += ts.pl_stats[i];
                        }

                        calcAvg();
                        break;
                    }
                default:
                    {
                        throw new Exception("Team Add Stats called with invalid parameter: " + mode);
                    }
            }
        }

        public void ResetStats(string mode)
        {
            switch (mode)
            {
                case "Season":
                    {
                        winloss[0] = 0;
                        winloss[1] = 0;

                        for (int i = 0; i < stats.Length; i++)
                        {
                            stats[i] = 0;
                        }

                        calcAvg();
                        break;
                    }
                case "Playoffs":
                    {
                        pl_winloss[0] = 0;
                        pl_winloss[1] = 0;

                        for (int i = 0; i < pl_stats.Length; i++)
                        {
                            pl_stats[i] = 0;
                        }

                        calcAvg();
                        break;
                    }
                case "All":
                    {
                        winloss[0] = 0;
                        winloss[1] = 0;

                        for (int i = 0; i < stats.Length; i++)
                        {
                            stats[i] = 0;
                        }

                        pl_winloss[0] = 0;
                        pl_winloss[1] = 0;

                        for (int i = 0; i < pl_stats.Length; i++)
                        {
                            pl_stats[i] = 0;
                        }

                        calcAvg();
                        break;
                    }
                default:
                    {
                        throw new Exception("Team Reset Stats called with invalid parameter: " + mode);
                    }
            }
        }

        public static int[][] CalculateTeamRankings(Dictionary<int, TeamStats> _teamStats, bool playoffs = false)
        {
            int len = _teamStats.Count;
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
                rating[k][19] = (int) _teamStats[k].getGames();
            }
            return rating;
        }

        public static string TeamAveragesAndRankings(string teamName, Dictionary<int, TeamStats> tst,
                                                     SortedDictionary<string, int> TeamOrder)
        {
            int id;
            try
            {
                id = TeamOrder[teamName];
            }
            catch
            {
                return "";
            }
            int[][] rating = CalculateTeamRankings(tst);
            string text =
                String.Format(
                    "Win %: {32:F3} ({33})\nWin eff: {34:F2} ({35})\n\nPPG: {0:F1} ({16})\nPAPG: {1:F1} ({17})\n\nFG%: {2:F3} ({18})\nFGeff: {3:F2} ({19})\n3P%: {4:F3} ({20})\n3Peff: {5:F2} ({21})\n"
                    +
                    "FT%: {6:F3} ({22})\nFTeff: {7:F2} ({23})\n\nRPG: {8:F1} ({24})\nORPG: {9:F1} ({25})\nDRPG: {10:F1} ({26})\n\nSPG: {11:F1} ({27})\nBPG: {12:F1} ({28})\n"
                    + "TPG: {13:F1} ({29})\nAPG: {14:F1} ({30})\nFPG: {15:F1} ({31})",
                    tst[id].averages[t.PPG], tst[id].averages[t.PAPG], tst[id].averages[t.FGp],
                    tst[id].averages[t.FGeff], tst[id].averages[t.TPp], tst[id].averages[t.TPeff],
                    tst[id].averages[t.FTp], tst[id].averages[t.FTeff], tst[id].averages[t.RPG],
                    tst[id].averages[t.ORPG],
                    tst[id].averages[t.DRPG], tst[id].averages[t.SPG],
                    tst[id].averages[t.BPG], tst[id].averages[t.TPG], tst[id].averages[t.APG], tst[id].averages[t.FPG],
                    rating[id][0], tst.Count + 1 - rating[id][1], rating[id][2], rating[id][3], rating[id][4],
                    rating[id][5], rating[id][6], rating[id][7], rating[id][8], rating[id][9],
                    rating[id][10], rating[id][11], rating[id][12], tst.Count + 1 - rating[id][13],
                    rating[id][14], tst.Count + 1 - rating[id][15], tst[id].averages[t.Wp], rating[id][16],
                    tst[id].averages[t.Weff], rating[id][t.Weff]);
            return text;
        }

        public static string TeamScoutingReport(int[][] rating, int teamID, string teamName)
        {
            //public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            //FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            //TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17;
            string msg = String.Format("{0}, the {1}", teamName, rating[teamID][17]);
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

            int comp = rating[teamID][t.FGeff] - rating[teamID][t.FGp];
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

            comp = rating[teamID][t.TPeff] - rating[teamID][t.TPp];
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
                if (rating[teamID][t.FTp] < 15)
                    msg +=
                        "A team that you'll enjoy playing hard and aggressively against on defense. They don't know how to get to the line.";
                else
                    msg +=
                        "A team that doesn't know how to get to the line, or how to score from there. You don't have to worry about freebies against them.";

            comp = rating[teamID][t.FTeff] - rating[teamID][t.FTp];
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

            if (31 - rating[teamID][t.PAPG] <= 5)
                msg +=
                    "Don't expect to get your score high against them. An elite defensive team, top 5 in points against them each night.";
            else if (31 - rating[teamID][t.PAPG] <= 10)
                msg +=
                    "One of the better defensive teams out there, limiting their opponents to low scores night in, night out.";
            else if (31 - rating[teamID][t.PAPG] <= 20)
                msg += "Average defensively, not much to show for it, but they're no blow-outs.";
            else if (31 - rating[teamID][t.PAPG] <= 30)
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

        public static bool IsTeamHiddenInSeason(string file, string name, int season)
        {
            var db = new SQLiteDatabase(file);
            int maxSeason = SQLiteIO.getMaxSeason(file);
            string teamsT = "Teams";
            if (season != maxSeason) teamsT += "S" + season;

            string q = "select isHidden from " + teamsT + " where Name LIKE \"" + name + "\"";
            bool isHidden = Tools.getBoolean(db.GetDataTable(q).Rows[0], "isHidden");

            return isHidden;
        }

        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref TeamStats ts1, ref TeamStats ts2)
        {
            var _tst = new Dictionary<int, TeamStats> {{1, ts1}, {2, ts2}};
            var _tstopp = new Dictionary<int, TeamStats> {{1, new TeamStats()}, {2, new TeamStats()}};
            AddTeamStatsFromBoxScore(bsToAdd, ref _tst, ref _tstopp, 1, 2);
            ts1 = _tst[1];
            ts2 = _tst[2];
        }

        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref Dictionary<int, TeamStats> _tst,
                                                    ref Dictionary<int, TeamStats> _tstopp, int id1, int id2)
        {
            TeamStats ts1 = _tst[id1];
            TeamStats ts2 = _tst[id2];
            TeamStats tsopp1 = _tstopp[id1];
            TeamStats tsopp2 = _tstopp[id2];
            if (!bsToAdd.isPlayoff)
            {
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    ts1.winloss[0]++;
                    ts2.winloss[1]++;
                }
                else
                {
                    ts1.winloss[1]++;
                    ts2.winloss[0]++;
                }
                // Add minutes played
                ts1.stats[t.MINS] += bsToAdd.MINS1;
                ts2.stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                ts1.stats[t.PF] += bsToAdd.PTS1;
                ts2.stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                ts1.stats[t.PA] += bsToAdd.PTS2;
                ts2.stats[t.PA] += bsToAdd.PTS1;

                //
                ts1.stats[t.FGM] += bsToAdd.FGM1;
                ts2.stats[t.FGM] += bsToAdd.FGM2;

                ts1.stats[t.FGA] += bsToAdd.FGA1;
                ts2.stats[t.FGA] += bsToAdd.FGA2;

                //
                ts1.stats[t.TPM] += bsToAdd.TPM1;
                ts2.stats[t.TPM] += bsToAdd.TPM2;

                //
                ts1.stats[t.TPA] += bsToAdd.TPA1;
                ts2.stats[t.TPA] += bsToAdd.TPA2;

                //
                ts1.stats[t.FTM] += bsToAdd.FTM1;
                ts2.stats[t.FTM] += bsToAdd.FTM2;

                //
                ts1.stats[t.FTA] += bsToAdd.FTA1;
                ts2.stats[t.FTA] += bsToAdd.FTA2;

                //
                ts1.stats[t.OREB] += bsToAdd.OREB1;
                ts2.stats[t.OREB] += bsToAdd.OREB2;

                //
                ts1.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                ts2.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                ts1.stats[t.STL] += bsToAdd.STL1;
                ts2.stats[t.STL] += bsToAdd.STL2;

                //
                ts1.stats[t.TO] += bsToAdd.TO1;
                ts2.stats[t.TO] += bsToAdd.TO2;

                //
                ts1.stats[t.BLK] += bsToAdd.BLK1;
                ts2.stats[t.BLK] += bsToAdd.BLK2;

                //
                ts1.stats[t.AST] += bsToAdd.AST1;
                ts2.stats[t.AST] += bsToAdd.AST2;

                //
                ts1.stats[t.FOUL] += bsToAdd.FOUL1;
                ts2.stats[t.FOUL] += bsToAdd.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    tsopp2.winloss[0]++;
                    tsopp1.winloss[1]++;
                }
                else
                {
                    tsopp2.winloss[1]++;
                    tsopp1.winloss[0]++;
                }
                // Add minutes played
                tsopp2.stats[t.MINS] += bsToAdd.MINS1;
                tsopp1.stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                tsopp2.stats[t.PF] += bsToAdd.PTS1;
                tsopp1.stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                tsopp2.stats[t.PA] += bsToAdd.PTS2;
                tsopp1.stats[t.PA] += bsToAdd.PTS1;

                //
                tsopp2.stats[t.FGM] += bsToAdd.FGM1;
                tsopp1.stats[t.FGM] += bsToAdd.FGM2;

                tsopp2.stats[t.FGA] += bsToAdd.FGA1;
                tsopp1.stats[t.FGA] += bsToAdd.FGA2;

                //
                tsopp2.stats[t.TPM] += bsToAdd.TPM1;
                tsopp1.stats[t.TPM] += bsToAdd.TPM2;

                //
                tsopp2.stats[t.TPA] += bsToAdd.TPA1;
                tsopp1.stats[t.TPA] += bsToAdd.TPA2;

                //
                tsopp2.stats[t.FTM] += bsToAdd.FTM1;
                tsopp1.stats[t.FTM] += bsToAdd.FTM2;

                //
                tsopp2.stats[t.FTA] += bsToAdd.FTA1;
                tsopp1.stats[t.FTA] += bsToAdd.FTA2;

                //
                tsopp2.stats[t.OREB] += bsToAdd.OREB1;
                tsopp1.stats[t.OREB] += bsToAdd.OREB2;

                //
                tsopp2.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                tsopp1.stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                tsopp2.stats[t.STL] += bsToAdd.STL1;
                tsopp1.stats[t.STL] += bsToAdd.STL2;

                //
                tsopp2.stats[t.TO] += bsToAdd.TO1;
                tsopp1.stats[t.TO] += bsToAdd.TO2;

                //
                tsopp2.stats[t.BLK] += bsToAdd.BLK1;
                tsopp1.stats[t.BLK] += bsToAdd.BLK2;

                //
                tsopp2.stats[t.AST] += bsToAdd.AST1;
                tsopp1.stats[t.AST] += bsToAdd.AST2;

                //
                tsopp2.stats[t.FOUL] += bsToAdd.FOUL1;
                tsopp1.stats[t.FOUL] += bsToAdd.FOUL2;
            }
            else
            {
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    ts1.pl_winloss[0]++;
                    ts2.pl_winloss[1]++;
                }
                else
                {
                    ts1.pl_winloss[1]++;
                    ts2.pl_winloss[0]++;
                }
                // Add minutes played
                ts1.pl_stats[t.MINS] += bsToAdd.MINS1;
                ts2.pl_stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                ts1.pl_stats[t.PF] += bsToAdd.PTS1;
                ts2.pl_stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                ts1.pl_stats[t.PA] += bsToAdd.PTS2;
                ts2.pl_stats[t.PA] += bsToAdd.PTS1;

                //
                ts1.pl_stats[t.FGM] += bsToAdd.FGM1;
                ts2.pl_stats[t.FGM] += bsToAdd.FGM2;

                ts1.pl_stats[t.FGA] += bsToAdd.FGA1;
                ts2.pl_stats[t.FGA] += bsToAdd.FGA2;

                //
                ts1.pl_stats[t.TPM] += bsToAdd.TPM1;
                ts2.pl_stats[t.TPM] += bsToAdd.TPM2;

                //
                ts1.pl_stats[t.TPA] += bsToAdd.TPA1;
                ts2.pl_stats[t.TPA] += bsToAdd.TPA2;

                //
                ts1.pl_stats[t.FTM] += bsToAdd.FTM1;
                ts2.pl_stats[t.FTM] += bsToAdd.FTM2;

                //
                ts1.pl_stats[t.FTA] += bsToAdd.FTA1;
                ts2.pl_stats[t.FTA] += bsToAdd.FTA2;

                //
                ts1.pl_stats[t.OREB] += bsToAdd.OREB1;
                ts2.pl_stats[t.OREB] += bsToAdd.OREB2;

                //
                ts1.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                ts2.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                ts1.pl_stats[t.STL] += bsToAdd.STL1;
                ts2.pl_stats[t.STL] += bsToAdd.STL2;

                //
                ts1.pl_stats[t.TO] += bsToAdd.TO1;
                ts2.pl_stats[t.TO] += bsToAdd.TO2;

                //
                ts1.pl_stats[t.BLK] += bsToAdd.BLK1;
                ts2.pl_stats[t.BLK] += bsToAdd.BLK2;

                //
                ts1.pl_stats[t.AST] += bsToAdd.AST1;
                ts2.pl_stats[t.AST] += bsToAdd.AST2;

                //
                ts1.pl_stats[t.FOUL] += bsToAdd.FOUL1;
                ts2.pl_stats[t.FOUL] += bsToAdd.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    tsopp2.pl_winloss[0]++;
                    tsopp1.pl_winloss[1]++;
                }
                else
                {
                    tsopp2.pl_winloss[1]++;
                    tsopp1.pl_winloss[0]++;
                }
                // Add minutes played
                tsopp2.pl_stats[t.MINS] += bsToAdd.MINS1;
                tsopp1.pl_stats[t.MINS] += bsToAdd.MINS2;

                // Add Points For
                tsopp2.pl_stats[t.PF] += bsToAdd.PTS1;
                tsopp1.pl_stats[t.PF] += bsToAdd.PTS2;

                // Add Points Against
                tsopp2.pl_stats[t.PA] += bsToAdd.PTS2;
                tsopp1.pl_stats[t.PA] += bsToAdd.PTS1;

                //
                tsopp2.pl_stats[t.FGM] += bsToAdd.FGM1;
                tsopp1.pl_stats[t.FGM] += bsToAdd.FGM2;

                tsopp2.pl_stats[t.FGA] += bsToAdd.FGA1;
                tsopp1.pl_stats[t.FGA] += bsToAdd.FGA2;

                //
                tsopp2.pl_stats[t.TPM] += bsToAdd.TPM1;
                tsopp1.pl_stats[t.TPM] += bsToAdd.TPM2;

                //
                tsopp2.pl_stats[t.TPA] += bsToAdd.TPA1;
                tsopp1.pl_stats[t.TPA] += bsToAdd.TPA2;

                //
                tsopp2.pl_stats[t.FTM] += bsToAdd.FTM1;
                tsopp1.pl_stats[t.FTM] += bsToAdd.FTM2;

                //
                tsopp2.pl_stats[t.FTA] += bsToAdd.FTA1;
                tsopp1.pl_stats[t.FTA] += bsToAdd.FTA2;

                //
                tsopp2.pl_stats[t.OREB] += bsToAdd.OREB1;
                tsopp1.pl_stats[t.OREB] += bsToAdd.OREB2;

                //
                tsopp2.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                tsopp1.pl_stats[t.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                tsopp2.pl_stats[t.STL] += bsToAdd.STL1;
                tsopp1.pl_stats[t.STL] += bsToAdd.STL2;

                //
                tsopp2.pl_stats[t.TO] += bsToAdd.TO1;
                tsopp1.pl_stats[t.TO] += bsToAdd.TO2;

                //
                tsopp2.pl_stats[t.BLK] += bsToAdd.BLK1;
                tsopp1.pl_stats[t.BLK] += bsToAdd.BLK2;

                //
                tsopp2.pl_stats[t.AST] += bsToAdd.AST1;
                tsopp1.pl_stats[t.AST] += bsToAdd.AST2;

                //
                tsopp2.pl_stats[t.FOUL] += bsToAdd.FOUL1;
                tsopp1.pl_stats[t.FOUL] += bsToAdd.FOUL2;
            }

            ts1.calcAvg();
            ts2.calcAvg();
            tsopp1.calcAvg();
            tsopp2.calcAvg();

            _tst[id1] = ts1;
            _tst[id2] = ts2;
            _tstopp[id1] = tsopp1;
            _tstopp[id2] = tsopp2;
        }
    }

    public class TeamMetricStatsRow
    {
        public string Name { get; set; }
        public double ORTG { get; set; }
        public double DRTG { get; set; }
        public double EFFd { get; set; }
        public double PWp { get; set; }
        public double EFGp { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double ASTp { get; set; }
        public double TOR { get; set; }
        public double FTR { get; set; }
        public double Poss { get; set; }
        public double Pace { get; set; }

        public TeamMetricStatsRow(TeamStats ts)
        {
            Name = ts.displayName;
            Poss = ts.metrics["PossPG"];
            Pace = ts.metrics["Pace"];
            ORTG = ts.metrics["ORTG"];
            DRTG = ts.metrics["DRTG"];
            ASTp = ts.metrics["AST%"];
            DREBp = ts.metrics["DREB%"];
            EFGp = ts.metrics["EFG%"];
            EFFd = ts.metrics["EFFd"];
            TOR = ts.metrics["TOR"];
            OREBp = ts.metrics["OREB%"];
            FTR = ts.metrics["FTR"];
            PWp = ts.metrics["PW%"];
        }
    }

    public class TeamRankings
    {

        public int[][] rankings;

        public TeamRankings(Dictionary<int, TeamStats> _tst)
        {
            rankings = new int[_tst.Count][];
            for (int i = 0; i < _tst.Count; i++)
            {
                rankings[i] = new int[_tst[i].averages.Length];
            }
            for (int j = 0; j < _tst[0].averages.Length; j++)
            {
                var averages = new Dictionary<int, float>();
                for (int i = 0; i < _tst.Count; i++)
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
}