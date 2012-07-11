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
using System.ComponentModel;
using System.Data;
using System.Linq;
using LeftosCommonLibrary;

#endregion

namespace NBA_Stats_Tracker.Data
{
    public static class p
    {
        public const int GP = 0,
                         GS = 1,
                         MINS = 2,
                         PTS = 3,
                         DREB = 4,
                         OREB = 5,
                         AST = 6,
                         STL = 7,
                         BLK = 8,
                         TO = 9,
                         FOUL = 10,
                         FGM = 11,
                         FGA = 12,
                         TPM = 13,
                         TPA = 14,
                         FTM = 15,
                         FTA = 16;

        public const int MPG = 0,
                         PPG = 1,
                         DRPG = 2,
                         ORPG = 3,
                         APG = 4,
                         SPG = 5,
                         BPG = 6,
                         TPG = 7,
                         FPG = 8,
                         FGp = 9,
                         FGeff = 10,
                         TPp = 11,
                         TPeff = 12,
                         FTp = 13,
                         FTeff = 14,
                         RPG = 15;
    }

    // Unlike TeamStats which was designed before REditor implemented such stats,
    // PlayerStats were made according to REditor's standards, to make life 
    // easier when importing/exporting from REditor's CSV
    public class PlayerStats
    {
        // TODO: Metric Stats here

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
        public Dictionary<string, double> metrics = new Dictionary<string, double>();
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
            ID = Tools.getInt(dataRow, "ID");
            LastName = Tools.getString(dataRow, "LastName");
            FirstName = Tools.getString(dataRow, "FirstName");
            Position1 = Tools.getString(dataRow, "Position1");
            if (String.IsNullOrEmpty(Position1)) Position1 = " ";
            Position2 = Tools.getString(dataRow, "Position2");
            if (String.IsNullOrEmpty(Position2)) Position2 = " ";
            TeamF = Tools.getString(dataRow, "TeamFin");
            TeamS = Tools.getString(dataRow, "TeamSta");
            isActive = Tools.getBoolean(dataRow, "isActive");
            isInjured = Tools.getBoolean(dataRow, "isInjured");
            isAllStar = Tools.getBoolean(dataRow, "isAllStar");
            isNBAChampion = Tools.getBoolean(dataRow, "isNBAChampion");

            stats[p.GP] = Tools.getUInt16(dataRow, "GP");
            stats[p.GS] = Tools.getUInt16(dataRow, "GS");
            stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
            stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");
            stats[p.FGM] = Tools.getUInt16(dataRow, "FGM");
            stats[p.FGA] = Tools.getUInt16(dataRow, "FGA");
            stats[p.TPM] = Tools.getUInt16(dataRow, "TPM");
            stats[p.TPA] = Tools.getUInt16(dataRow, "TPA");
            stats[p.FTM] = Tools.getUInt16(dataRow, "FTM");
            stats[p.FTA] = Tools.getUInt16(dataRow, "FTA");
            stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
            stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
            stats[p.STL] = Tools.getUInt16(dataRow, "STL");
            stats[p.TO] = Tools.getUInt16(dataRow, "TOS");
            stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
            stats[p.AST] = Tools.getUInt16(dataRow, "AST");
            stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");

            CalcAvg();
        }

        public PlayerStats(int ID, string LastName, string FirstName, string Position1, string Position2, string TeamF,
                           string TeamS,
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

            stats[p.GP] = Tools.getUInt16(dataRow, "GP");
            stats[p.GS] = Tools.getUInt16(dataRow, "GS");
            stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
            stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");

            string[] parts = Tools.getString(dataRow, "FG").Split('-');

            stats[p.FGM] = Convert.ToUInt16(parts[0]);
            stats[p.FGA] = Convert.ToUInt16(parts[1]);

            parts = Tools.getString(dataRow, "3PT").Split('-');

            stats[p.TPM] = Convert.ToUInt16(parts[0]);
            stats[p.TPA] = Convert.ToUInt16(parts[1]);

            parts = Tools.getString(dataRow, "FT").Split('-');

            stats[p.FTM] = Convert.ToUInt16(parts[0]);
            stats[p.FTA] = Convert.ToUInt16(parts[1]);

            stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
            stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
            stats[p.STL] = Tools.getUInt16(dataRow, "STL");
            stats[p.TO] = Tools.getUInt16(dataRow, "TO");
            stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
            stats[p.AST] = Tools.getUInt16(dataRow, "AST");
            stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");

            CalcAvg();
        }

        public PlayerStats(PlayerStatsRow playerStatsRow)
        {
            LastName = playerStatsRow.LastName;
            FirstName = playerStatsRow.FirstName;

            stats[p.GP] = playerStatsRow.GP;
            stats[p.GS] = playerStatsRow.GS;
            stats[p.MINS] = playerStatsRow.MINS;
            stats[p.PTS] = playerStatsRow.PTS;
            stats[p.FGM] = playerStatsRow.FGM;
            stats[p.FGA] = playerStatsRow.FGA;
            stats[p.TPM] = playerStatsRow.TPM;
            stats[p.TPA] = playerStatsRow.TPA;
            stats[p.FTM] = playerStatsRow.FTM;
            stats[p.FTA] = playerStatsRow.FTA;
            stats[p.OREB] = playerStatsRow.OREB;
            stats[p.DREB] = playerStatsRow.DREB;
            stats[p.STL] = playerStatsRow.STL;
            stats[p.TO] = playerStatsRow.TOS;
            stats[p.BLK] = playerStatsRow.BLK;
            stats[p.AST] = playerStatsRow.AST;
            stats[p.FOUL] = playerStatsRow.FOUL;

            ID = playerStatsRow.ID;
            Position1 = playerStatsRow.Position1;
            Position2 = playerStatsRow.Position2;
            TeamF = playerStatsRow.TeamF;
            TeamS = playerStatsRow.TeamS;
            isActive = playerStatsRow.isActive;
            isAllStar = playerStatsRow.isAllStar;
            isInjured = playerStatsRow.isInjured;
            isNBAChampion = playerStatsRow.isNBAChampion;

            CalcAvg();
        }

        public void CalcAvg()
        {
            int games = stats[p.GP];
            averages[p.MPG] = (float) stats[p.MINS]/games;
            averages[p.PPG] = (float) stats[p.PTS]/games;
            averages[p.FGp] = (float) stats[p.FGM]/stats[p.FGA];
            averages[p.FGeff] = averages[p.FGp]*((float) stats[p.FGM]/games);
            averages[p.TPp] = (float) stats[p.TPM]/stats[p.TPA];
            averages[p.TPeff] = averages[p.TPp]*((float) stats[p.TPM]/games);
            averages[p.FTp] = (float) stats[p.FTM]/stats[p.FTA];
            averages[p.FTeff] = averages[p.FTp]*((float) stats[p.FTM]/games);
            averages[p.RPG] = (float) (stats[p.OREB] + stats[p.DREB])/games;
            averages[p.ORPG] = (float) stats[p.OREB]/games;
            averages[p.DRPG] = (float) stats[p.DREB]/games;
            averages[p.SPG] = (float) stats[p.STL]/games;
            averages[p.BPG] = (float) stats[p.BLK]/games;
            averages[p.TPG] = (float) stats[p.TO]/games;
            averages[p.APG] = (float) stats[p.AST]/games;
            averages[p.FPG] = (float) stats[p.FOUL]/games;
        }

        /// <summary>
        /// Calculates the Metric Stats for this Player
        /// </summary>
        /// <param name="ts">The player's team's stats</param>
        /// <param name="tsopp">The player's team's opponents' stats</param>
        /// <param name="ls">The total league stats</param>
        /// <param name="leagueOv">Whether CalcMetrics is being called from the League Overview screen</param>
        public void CalcMetrics(TeamStats ts, TeamStats tsopp, TeamStats ls, bool leagueOv = false)
        {
            var pstats = new double[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                pstats[i] = stats[i];
            }

            var tstats = new double[ts.stats.Length];
            for (int i = 0; i < ts.stats.Length; i++)
            {
                tstats[i] = ts.stats[i];
            }

            var toppstats = new double[tsopp.stats.Length];
            for (int i = 0; i < tsopp.stats.Length; i++)
            {
                toppstats[i] = tsopp.stats[i];
            }

            var lstats = new double[ls.stats.Length];
            for (int i = 0; i < ls.stats.Length; i++)
            {
                lstats[i] = ls.stats[i];
            }

            double pREB = pstats[p.OREB] + pstats[p.DREB];
            double tREB = tstats[t.OREB] + tstats[t.DREB];

            metrics = new Dictionary<string, double>();

            #region Metrics that do not require Opponent Stats

            double ASTp = 100*pstats[p.AST]/(((pstats[p.MINS]/(tstats[t.MINS]))*tstats[t.FGM]) - pstats[p.FGM]);
            metrics.Add("AST%", ASTp);

            double EFGp = (pstats[p.FGM] + 0.5*pstats[p.TPM])/pstats[p.FGA];
            metrics.Add("EFG%", EFGp);

            double GmSc = pstats[p.PTS] + 0.4*pstats[p.FGM] - 0.7*pstats[p.FGA] - 0.4*(pstats[p.FTA] - pstats[p.FTM]) +
                          0.7*pstats[p.OREB] + 0.3*pstats[p.DREB] + pstats[p.STL] + 0.7*pstats[p.AST] +
                          0.7*pstats[p.BLK] -
                          0.4*pstats[p.FOUL] - pstats[p.TO];
            metrics.Add("GmSc", GmSc/pstats[p.GP]);

            double STLp = 100*(pstats[p.STL]*(tstats[t.MINS]))/(pstats[p.MINS]*tsopp.metrics["Poss"]);
            metrics.Add("STL%", STLp);

            double TOp = 100*pstats[p.TO]/(pstats[p.FGA] + 0.44*pstats[p.FTA] + pstats[p.TO]);
            metrics.Add("TO%", TOp);

            double TSp = pstats[p.PTS]/(2*(pstats[p.FGA] + 0.44*pstats[p.FTA]));
            metrics.Add("TS%", TSp);

            double USGp = 100*
                          ((pstats[p.FGA] + 0.44*pstats[p.FTA] + pstats[p.TO])*
                           (tstats[t.MINS]))/(pstats[p.MINS]*(tstats[t.FGA] + 0.44*tstats[t.FTA] + tstats[t.TO]));
            metrics.Add("USG%", USGp);

            // Rates, stat per 49 minutes played
            double PTSR = (pstats[p.PTS]/pstats[p.MINS])*48;
            metrics.Add("PTSR", PTSR);

            double REBR = (pREB/pstats[p.MINS])*48;
            metrics.Add("REBR", REBR);

            double ASTR = (pstats[p.AST]/pstats[p.MINS])*48;
            metrics.Add("ASTR", ASTR);

            double BLKR = (pstats[p.BLK]/pstats[p.MINS])*48;
            metrics.Add("BLKR", BLKR);

            double STLR = (pstats[p.STL]/pstats[p.MINS])*48;
            metrics.Add("STLR", STLR);

            double TOR = (pstats[p.TO]/pstats[p.MINS])*48;
            metrics.Add("TOR", TOR);

            double FTR = (pstats[p.FTM]/pstats[p.FGA]);
            metrics.Add("FTR", FTR);
            //
            // PER preparations
            double lREB = lstats[t.OREB] + lstats[t.DREB];
            double factor = (2/3) - (0.5*(lstats[t.AST]/lstats[t.FGM]))/(2*(lstats[t.FGM]/lstats[t.FTM]));
            double VOP = lstats[t.PF]/(lstats[t.FGA] - lstats[t.OREB] + lstats[t.TO] + 0.44*lstats[t.FTA]);
            double lDRBp = lstats[t.DREB]/lREB;

            double uPER = (1/pstats[p.MINS])*
                          (pstats[p.TPM]
                           + (2/3)*pstats[p.AST]
                           + (2 - factor*(tstats[t.AST]/tstats[t.FGM]))*pstats[p.FGM]
                           +
                           (pstats[p.FTM]*0.5*
                            (1 + (1 - (tstats[t.AST]/tstats[t.FGM])) + (2/3)*(tstats[t.AST]/tstats[t.FGM])))
                           - VOP*pstats[p.TO]
                           - VOP*lDRBp*(pstats[p.FGA] - pstats[p.FGM])
                           - VOP*0.44*(0.44 + (0.56*lDRBp))*(pstats[p.FTA] - pstats[p.FTM])
                           + VOP*(1 - lDRBp)*(pREB - pstats[p.OREB])
                           + VOP*lDRBp*pstats[p.OREB]
                           + VOP*pstats[p.STL]
                           + VOP*lDRBp*pstats[p.BLK]
                           - pstats[p.FOUL]*((lstats[t.FTM]/lstats[t.FOUL]) - 0.44*(lstats[t.FTA]/lstats[t.FOUL])*VOP));
            metrics.Add("EFF", uPER*100);

            #endregion

            #region Metrics that require Opponents stats

            if (ts.getGames() == tsopp.getGames())
            {
                double BLKp = 100*(pstats[p.BLK]*(tstats[t.MINS]))/
                              (pstats[p.MINS]*(toppstats[t.FGA] - toppstats[t.TPA]));

                double DRBp = 100*(pstats[p.DREB]*(tstats[t.MINS]))/
                              (pstats[p.MINS]*(tstats[t.DREB] + toppstats[t.OREB]));

                double ORBp = 100*(pstats[p.OREB]*(tstats[t.MINS]))/
                              (pstats[p.MINS]*(tstats[t.OREB] + toppstats[t.DREB]));

                double toppREB = toppstats[t.OREB] + toppstats[t.DREB];

                double REBp = 100*(pREB*(tstats[t.MINS]))/(pstats[p.MINS]*(tREB + toppREB));

                #region Metrics that require league stats

                double aPER;
                double PPR;

                if (ls.name != "$$Empty")
                {
                    //double paceAdj = ls.metrics["Pace"]/ts.metrics["Pace"];
                    double estPaceAdj = 2*ls.averages[t.PPG]/(ts.averages[t.PPG] + tsopp.averages[t.PPG]);

                    aPER = estPaceAdj*uPER;

                    PPR = 100*estPaceAdj*(((pstats[p.AST]*2/3) - pstats[p.TO])/pstats[p.MINS]);
                }
                else
                {
                    aPER = Double.NaN;
                    PPR = Double.NaN;
                }

                #endregion

                metrics.Add("aPER", aPER);
                metrics.Add("BLK%", BLKp);
                metrics.Add("DREB%", DRBp);
                metrics.Add("OREB%", ORBp);
                metrics.Add("REB%", REBp);
                metrics.Add("PPR", PPR);
            }
            else
            {
                metrics.Add("aPER", Double.NaN);
                metrics.Add("BLK%", Double.NaN);
                metrics.Add("DREB%", Double.NaN);
                metrics.Add("OREB%", Double.NaN);
                metrics.Add("REB%", Double.NaN);
                metrics.Add("PPR", Double.NaN);
            }

            #endregion

            var gamesRequired = (int) Math.Ceiling(0.8522*ts.getGames());
            if (leagueOv)
            {
                if (stats[p.GP] < gamesRequired)
                {
                    foreach (string name in metrics.Keys.ToList())
                        metrics[name] = Double.NaN;
                }
            }
        }

        public void CalcPER(double lg_aPER)
        {
            try
            {
                metrics.Add("PER", metrics["aPER"] * (15 / lg_aPER));
            }
            catch (Exception)
            {
                metrics.Add("PER", double.NaN);
            }
        }

        public void AddBoxScore(PlayerBoxScore pbs)
        {
            if (ID != pbs.PlayerID)
                throw new Exception("Tried to update PlayerStats " + ID + " with PlayerBoxScore " + pbs.PlayerID);

            if (pbs.isStarter) stats[p.GS]++;
            if (pbs.MINS > 0)
            {
                stats[p.GP]++;
                stats[p.MINS] += pbs.MINS;
            }
            stats[p.PTS] += pbs.PTS;
            stats[p.FGM] += pbs.FGM;
            stats[p.FGA] += pbs.FGA;
            stats[p.TPM] += pbs.TPM;
            stats[p.TPA] += pbs.TPA;
            stats[p.FTM] += pbs.FTM;
            stats[p.FTA] += pbs.FTA;
            stats[p.OREB] += pbs.OREB;
            stats[p.DREB] += pbs.DREB;
            stats[p.STL] += pbs.STL;
            stats[p.TO] += pbs.TOS;
            stats[p.BLK] += pbs.BLK;
            stats[p.AST] += pbs.AST;
            stats[p.FOUL] += pbs.FOUL;

            CalcAvg();
        }

        public void AddPlayerStats(PlayerStats ps)
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] += ps.stats[i];
            }

            CalcAvg();
        }

        public void ResetStats()
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = 0;
            }

            CalcAvg();
        }

        public static void CalculateAllMetrics(ref Dictionary<int, PlayerStats> playerStats, Dictionary<int, TeamStats> teamStats,
                                               Dictionary<int, TeamStats> oppStats, SortedDictionary<string, int> TeamOrder,
                                               bool leagueOv = false)
        {
            int tCount = teamStats.Count;

            var ls = new TeamStats();
            var lsopp = new TeamStats();
            var tst = new TeamStats[tCount];
            var tstopp = new TeamStats[tCount];
            for (int i = 0; i < tCount; i++)
            {
                ls.AddTeamStats(teamStats[i], "All");
                tst[i] = new TeamStats();
                tst[i].AddTeamStats(teamStats[i], "All");
                lsopp.AddTeamStats(oppStats[i], "All");
                tstopp[i] = new TeamStats();
                tstopp[i].AddTeamStats(oppStats[i], "All");
                tst[i].CalcMetrics(tstopp[i]);
            }
            ls.CalcMetrics(lsopp);

            double lg_aPER = 0;
            double totalMins = 0;

            foreach (int playerid in playerStats.Keys.ToList())
            {
                if (String.IsNullOrEmpty(playerStats[playerid].TeamF)) continue;

                int teamid = TeamOrder[playerStats[playerid].TeamF];
                TeamStats ts = tst[teamid];
                TeamStats tsopp = tstopp[teamid];

                playerStats[playerid].CalcMetrics(ts, tsopp, ls, leagueOv);
                if (!(Double.IsNaN(playerStats[playerid].metrics["aPER"])))
                {
                    lg_aPER += playerStats[playerid].metrics["aPER"]*playerStats[playerid].stats[p.MINS];
                    totalMins += playerStats[playerid].stats[p.MINS];
                }
            }
            lg_aPER /= totalMins;

            foreach (int playerid in playerStats.Keys.ToList())
            {
                if (String.IsNullOrEmpty(playerStats[playerid].TeamF)) continue;

                playerStats[playerid].CalcPER(lg_aPER);
            }
        }
    }

    public class PlayerBoxScore : INotifyPropertyChanged
    {
        private UInt16 _FGA;
        private UInt16 _FGM;
        private UInt16 _FTA;
        private UInt16 _FTM;
        private UInt16 _TPA;
        private UInt16 _TPM;
        //public ObservableCollection<KeyValuePair<int, string>> PlayersList { get; set; }
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
            PlayerID = Tools.getInt(r, "PlayerID");
            GameID = Tools.getInt(r, "GameID");
            Team = r["Team"].ToString();
            isStarter = Tools.getBoolean(r, "isStarter");
            playedInjured = Tools.getBoolean(r, "playedInjured");
            isOut = Tools.getBoolean(r, "isOut");
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
            DREB = (UInt16) (REB - OREB);
            FGp = (float) FGM/FGA;
            TPp = (float) TPM/TPA;
            FTp = (float) FTM/FTA;

            // Let's try to get the result and date of the game
            // Only works for INNER JOIN'ed rows
            try
            {
                int T1PTS = Tools.getInt(r, "T1PTS");
                int T2PTS = Tools.getInt(r, "T2PTS");

                string Team1 = Tools.getString(r, "T1Name");
                string Team2 = Tools.getString(r, "T2Name");

                if (Team == Team1)
                {
                    if (T1PTS > T2PTS)
                        Result = "W " + T1PTS.ToString() + "-" + T2PTS.ToString();
                    else
                        Result = "L " + T1PTS.ToString() + "-" + T2PTS.ToString();

                    TeamPTS = T1PTS;
                    OppTeam = Team2;
                    OppTeamPTS = T2PTS;
                }
                else
                {
                    if (T2PTS > T1PTS)
                        Result = "W " + T2PTS.ToString() + "-" + T1PTS.ToString();
                    else
                        Result = "L " + T2PTS.ToString() + "-" + T1PTS.ToString();

                    TeamPTS = T2PTS;
                    OppTeam = Team1;
                    OppTeamPTS = T1PTS;
                }

                Date = Tools.getString(r, "Date").Split(' ')[0];
            }
            catch (Exception)
            {
            }
        }

        public int PlayerID { get; set; }
        public string Team { get; set; }
        public int TeamPTS { get; set; }
        public string OppTeam { get; set; }
        public int OppTeamPTS { get; set; }
        public bool isStarter { get; set; }
        public bool playedInjured { get; set; }
        public bool isOut { get; set; }
        public UInt16 MINS { get; set; }
        public UInt16 PTS { get; set; }

        public UInt16 FGM
        {
            get { return _FGM; }
            set
            {
                _FGM = value;
                FGp = (float) _FGM/_FGA;
                CalculatePoints();
                NotifyPropertyChanged("FGp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 FGA
        {
            get { return _FGA; }
            set
            {
                _FGA = value;
                FGp = (float) _FGM/_FGA;
                CalculatePoints();
                NotifyPropertyChanged("FGp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float FGp { get; set; }

        public UInt16 TPM
        {
            get { return _TPM; }
            set
            {
                _TPM = value;
                TPp = (float) _TPM/_TPA;
                CalculatePoints();
                NotifyPropertyChanged("TPp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 TPA
        {
            get { return _TPA; }
            set
            {
                _TPA = value;
                TPp = (float) _TPM/_TPA;
                CalculatePoints();
                NotifyPropertyChanged("TPp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float TPp { get; set; }

        public UInt16 FTM
        {
            get { return _FTM; }
            set
            {
                _FTM = value;
                FTp = (float) FTM/FTA;
                CalculatePoints();
                NotifyPropertyChanged("FTp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 FTA
        {
            get { return _FTA; }
            set
            {
                _FTA = value;
                FTp = (float) FTM/FTA;
                CalculatePoints();
                NotifyPropertyChanged("FTp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float FTp { get; set; }
        public UInt16 REB { get; set; }
        public UInt16 OREB { get; set; }
        public UInt16 DREB { get; set; }
        public UInt16 STL { get; set; }
        public UInt16 TOS { get; set; }
        public UInt16 BLK { get; set; }
        public UInt16 AST { get; set; }
        public UInt16 FOUL { get; set; }

        public string Result { get; set; }
        public string Date { get; set; }
        public int GameID { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void CalculatePoints()
        {
            PTS = (ushort) ((_FGM - _TPM)*2 + _TPM*3 + _FTM); //(fgm - tpm)*2 + tpm*3 + ftm
        }

        public string GetBestStats(int count, string position)
        {
            double fgn = 0, tpn = 0, ftn = 0, ftrn = 0;
            var statsn = new Dictionary<string, double>();

            double fgfactor,
                   tpfactor,
                   ftfactor,
                   orebfactor,
                   rebfactor,
                   astfactor,
                   stlfactor,
                   blkfactor,
                   ptsfactor,
                   ftrfactor;

            if (position.EndsWith("G"))
            {
                fgfactor = 0.4871;
                tpfactor = 0.39302;
                ftfactor = 0.86278;
                orebfactor = 1.242;
                rebfactor = 4.153;
                astfactor = 6.324;
                stlfactor = 1.619;
                blkfactor = 0.424;
                ptsfactor = 17.16;
                ftrfactor = 0.271417;
            }
            else if (position.EndsWith("F"))
            {
                fgfactor = 0.52792;
                tpfactor = 0.38034;
                ftfactor = 0.82656;
                orebfactor = 2.671;
                rebfactor = 8.145;
                astfactor = 3.037;
                stlfactor = 1.209;
                blkfactor = 1.24;
                ptsfactor = 17.731;
                ftrfactor = 0.307167;
            }
            else if (position.EndsWith("C"))
            {
                fgfactor = 0.52862;
                tpfactor = 0.23014;
                ftfactor = 0.75321;
                orebfactor = 2.328;
                rebfactor = 7.431;
                astfactor = 1.688;
                stlfactor = 0.68;
                blkfactor = 1.536;
                ptsfactor = 11.616;
                ftrfactor = 0.302868;
            }
            else
            {
                fgfactor = 0.51454;
                tpfactor = 0.3345;
                ftfactor = 0.81418;
                orebfactor = 2.0803;
                rebfactor = 6.5763;
                astfactor = 3.683;
                stlfactor = 1.1693;
                blkfactor = 1.0667;
                ptsfactor = 15.5023;
                ftrfactor = 0.385722;
            }

            if (FGM > 4)
            {
                fgn = FGp/fgfactor;
            }
            statsn.Add("fgn", fgn);

            if (TPM > 2)
            {
                tpn = TPp/tpfactor;
            }
            statsn.Add("tpn", tpn);

            if (FTM > 4)
            {
                ftn = FTp/ftfactor;
            }
            statsn.Add("ftn", ftn);

            double orebn = OREB/orebfactor;
            statsn.Add("orebn", orebn);

            /*
            drebn = (REB-OREB)/6.348;
            statsn.Add("drebn", drebn);
            */

            double rebn = REB/rebfactor;
            statsn.Add("rebn", rebn);

            double astn = AST/astfactor;
            statsn.Add("astn", astn);

            double stln = STL/stlfactor;
            statsn.Add("stln", stln);

            double blkn = BLK/blkfactor;
            statsn.Add("blkn", blkn);

            double ptsn = PTS/ptsfactor;
            statsn.Add("ptsn", ptsn);

            if (FTM > 3)
            {
                ftrn = ((double)FTM/FGA)/ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            IOrderedEnumerable<string> items = from k in statsn.Keys
                                               orderby statsn[k] descending
                                               select k;

            string s = "";
            int i = 0;
            foreach (string item in items)
            {
                if (i == count) break;

                switch (item)
                {
                    case "fgn":
                        s += String.Format("FG: {0}-{1} ({2:F3})\n", FGM, FGA, FGp);
                        break;

                    case "tpn":
                        s += String.Format("3P: {0}-{1} ({2:F3})\n", TPM, TPA, TPp);
                        break;

                    case "ftn":
                        s += String.Format("FT: {0}-{1} ({2:F3})\n", FTM, FTA, FTp);
                        break;

                    case "orebn":
                        s += String.Format("OREB: {0}\n", OREB);
                        break;

                        /*
                    case "drebn":
                        s += String.Format("DREB: {0}\n", REB - OREB);
                        break;
                    */

                    case "rebn":
                        s += String.Format("REB: {0}\n", REB);
                        break;

                    case "astn":
                        s += String.Format("AST: {0}\n", AST);
                        break;

                    case "stln":
                        s += String.Format("STL: {0}\n", STL);
                        break;

                    case "blkn":
                        s += String.Format("BLK: {0}\n", BLK);
                        break;

                    case "ptsn":
                        s += String.Format("PTS: {0}\n", PTS);
                        break;

                    case "ftrn":
                        s += String.Format("FTM/FGA: {0}-{1} ({2:F3})\n", FTM, FGA, (double)FTM/FGA);
                        break;
                }

                i++;
            }
            return s;
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
            FGp = 0;
            FTp = 0;
            TPp = 0;
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
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

        private readonly int avgcount = (new PlayerStats(new Player(-1, "", "", "", "", ""))).averages.Length;

        private readonly Dictionary<int, int[]> rankings = new Dictionary<int, int[]>();
        public Dictionary<int, int[]> list = new Dictionary<int, int[]>();

        public PlayerRankings(Dictionary<int, PlayerStats> pst)
        {
            foreach (KeyValuePair<int, PlayerStats> kvp in pst)
            {
                rankings.Add(kvp.Key, new int[avgcount]);
            }
            for (int j = 0; j < avgcount; j++)
            {
                var averages = pst.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.averages[j]);

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (KeyValuePair<int, float> kvp in tempList)
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

        public Player(DataRow dataRow)
        {
            ID = Tools.getInt(dataRow, "ID");
            Team = Tools.getString(dataRow, "TeamFin");
            LastName = Tools.getString(dataRow, "LastName");
            FirstName = Tools.getString(dataRow, "FirstName");
            Position = Tools.getString(dataRow, "Position1");
            Position2 = Tools.getString(dataRow, "Position2");
        }

        public int ID { get; set; }
        public string Team { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Position { get; set; }
        public string Position2 { get; set; }
        public bool AddToAll { get; set; }
    }

    public class PlayerStatsRow
    {
        public PlayerStatsRow(PlayerStats ps)
        {
            LastName = ps.LastName;
            FirstName = ps.FirstName;

            GP = ps.stats[p.GP];
            GS = ps.stats[p.GS];
            MINS = ps.stats[p.MINS];
            PTS = ps.stats[p.PTS];
            FGM = ps.stats[p.FGM];
            FGA = ps.stats[p.FGA];
            TPM = ps.stats[p.TPM];
            TPA = ps.stats[p.TPA];
            FTM = ps.stats[p.FTM];
            FTA = ps.stats[p.FTA];
            OREB = ps.stats[p.OREB];
            DREB = ps.stats[p.DREB];
            REB = (UInt16) (OREB + DREB);
            STL = ps.stats[p.STL];
            TOS = ps.stats[p.TO];
            BLK = ps.stats[p.BLK];
            AST = ps.stats[p.AST];
            FOUL = ps.stats[p.FOUL];

            MPG = ps.averages[p.MPG];
            PPG = ps.averages[p.PPG];
            FGp = ps.averages[p.FGp];
            FGeff = ps.averages[p.FGeff];
            TPp = ps.averages[p.TPp];
            TPeff = ps.averages[p.TPeff];
            FTp = ps.averages[p.FTp];
            FTeff = ps.averages[p.FTeff];
            RPG = ps.averages[p.RPG];
            ORPG = ps.averages[p.ORPG];
            DRPG = ps.averages[p.DRPG];
            SPG = ps.averages[p.SPG];
            TPG = ps.averages[p.TPG];
            BPG = ps.averages[p.BPG];
            APG = ps.averages[p.APG];
            FPG = ps.averages[p.FPG];

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

        public PlayerStatsRow(PlayerStats ps, string type)
            : this(ps)
        {
            Type = type;
        }

        public PlayerStatsRow(PlayerStats ps, string type, string group)
            : this(ps, type)
        {
            Type = type;
            Group = group;
        }

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
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Position1 { get; set; }
        public string Position2 { get; set; }
        public string TeamF { get; set; }
        public string TeamFDisplay { get; set; }
        public string TeamS { get; set; }
        public bool isActive { get; set; }
        public bool isAllStar { get; set; }
        public bool isInjured { get; set; }
        public bool isNBAChampion { get; set; }

        public string Type { get; set; }
        public string Group { get; set; }
    }

    public class PlayerMetricStatsRow
    {
        public PlayerMetricStatsRow(PlayerStats ps)
        {
            //ps.CalcMetrics();

            ID = ps.ID;
            LastName = ps.LastName;
            FirstName = ps.FirstName;
            TeamF = ps.TeamF;

            EFF = ps.metrics["EFF"];
            GmSc = ps.metrics["GmSc"];
            EFGp = ps.metrics["EFG%"];
            TSp = ps.metrics["TS%"];
            ASTp = ps.metrics["AST%"];
            STLp = ps.metrics["STL%"];
            TOp = ps.metrics["TO%"];
            USGp = ps.metrics["USG%"];
            PTSR = ps.metrics["PTSR"];
            REBR = ps.metrics["REBR"];
            ASTR = ps.metrics["ASTR"];
            BLKR = ps.metrics["BLKR"];
            STLR = ps.metrics["STLR"];
            TOR = ps.metrics["TOR"];
            FTR = ps.metrics["FTR"];

            try
            {
                PER = ps.metrics["PER"];
            }
            catch (Exception)
            {
                PER = double.NaN;
            }

            BLKp = ps.metrics["BLK%"];
            DREBp = ps.metrics["DREB%"];
            OREBp = ps.metrics["OREB%"];
            REBp = ps.metrics["REB%"];
            PPR = ps.metrics["PPR"];
        }

        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string TeamF { get; set; }
        public string TeamFDisplay { get; set; }

        public double EFF { get; set; }
        public double GmSc { get; set; }
        public double EFGp { get; set; }
        public double TSp { get; set; }
        public double ASTp { get; set; }
        public double STLp { get; set; }
        public double TOp { get; set; }
        public double USGp { get; set; }
        public double PTSR { get; set; }
        public double REBR { get; set; }
        public double ASTR { get; set; }
        public double BLKR { get; set; }
        public double STLR { get; set; }
        public double TOR { get; set; }
        public double FTR { get; set; }

        #region Metrics that require opponents' stats

        public double PER { get; set; }
        public double BLKp { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double REBp { get; set; }
        public double PPR { get; set; }

        #endregion
    }
}