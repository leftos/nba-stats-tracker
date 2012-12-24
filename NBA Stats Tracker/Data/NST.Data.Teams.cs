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
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Windows;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Data
{
    /// <summary>
    /// Contains all the information for the teams' performances in a game.
    /// </summary>
    public class TeamBoxScore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBoxScore" /> class.
        /// </summary>
        public TeamBoxScore()
        {
            id = -1;
            bshistid = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBoxScore" /> class.
        /// </summary>
        /// <param name="r">The SQLite query result row which contains the required information.</param>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamBoxScore" /> class.
        /// </summary>
        /// <param name="ds">The Basketball-Reference.com dataset resulting from the parsing.</param>
        /// <param name="dateParts">The parts of the date string.</param>
        public TeamBoxScore(DataSet ds, string[] dateParts)
        {
            DataTable away = ds.Tables[0];
            DataTable home = ds.Tables[1];

            int done = 0;
            foreach (var team in MainWindow.TeamOrder)
            {
                if (dateParts[0].Contains(team.Key))
                {
                    Team1 = team.Key;
                    done++;
                }
                if (dateParts[1].Contains(team.Key))
                {
                    Team2 = team.Key;
                    done++;
                }
                if (done == 2)
                    break;
            }
            if (done != 2)
            {
                Team1 = "$$Invalid";
                Team2 = "$$Invalid";
                return;
            }
            string date = dateParts[2].Trim() + ", " + dateParts[3].Trim();
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
        public float FGp { get; set; }
        public float TPp { get; set; }
        public float FTp { get; set; }
        public ushort DisplayREB { get; set; }
        public ushort DisplayOREB { get; set; }
        public ushort DisplayAST { get; set; }
        public ushort DisplayTO { get; set; }
        public ushort DisplayBLK { get; set; }
        public ushort DisplaySTL { get; set; }
        public ushort DisplayFOUL { get; set; }
        public double DisplayGmSc { get; set; }

        /// <summary>
        /// Prepares the presentation fields of the class.
        /// </summary>
        /// <param name="team">The team.</param>
        public void PrepareForDisplay(string team)
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
                FGp = (float) FGM1/FGA1;
                TPp = (float) TPM1/TPA1;
                FTp = (float) FTM1/FTA1;
                DisplayREB = REB1;
                DisplayOREB = OREB1;
                DisplayAST = AST1;
                DisplayTO = TO1;
                DisplayBLK = BLK1;
                DisplaySTL = STL1;
                DisplayFOUL = FOUL1;

                var temp = new TeamStats();
                var tempopp = new TeamStats();
                TeamStats.AddTeamStatsFromBoxScore(this, ref temp, ref tempopp);
                temp.CalcMetrics(tempopp);

                DisplayGmSc = temp.metrics["GmSc"];
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
                FGp = (float) FGM2/FGA2;
                TPp = (float) TPM2/TPA2;
                FTp = (float) FTM2/FTA2;
                DisplayREB = REB2;
                DisplayOREB = OREB2;
                DisplayAST = AST2;
                DisplayTO = TO2;
                DisplayBLK = BLK2;
                DisplaySTL = STL2;
                DisplayFOUL = FOUL2;

                var temp = new TeamStats();
                var tempopp = new TeamStats();
                TeamStats.AddTeamStatsFromBoxScore(this, ref tempopp, ref temp);
                temp.CalcMetrics(tempopp);

                DisplayGmSc = temp.metrics["GmSc"];
            }
            DisplayResult += PTS1 + "-" + PTS2;
        }
    }

    /// <summary>
    /// A list of constant pseudonyms for specific entries in the teams' stats arrays.
    /// </summary>
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

    /// <summary>
    /// A container for all of a team's information, stats, averages and metrics handled by the program.
    /// </summary>
    [Serializable]
    public class TeamStats
    {
        public int ID;
        private int _division;

        /// <summary>
        /// Averages for each team.
        /// 0: PPG, 1: PAPG, 2: FG%, 3: FGEff, 4: 3P%, 5: 3PEff, 6: FT%, 7:FTEff,
        /// 8: RPG, 9: ORPG, 10: DRPG, 11: SPG, 12: BPG, 13: TPG, 14: APG, 15: FPG, 16: W%,
        /// 17: Weff, 18: PD
        /// </summary>
        public float[] averages = new float[19];

        public int conference;

        public string displayName;
        public bool isHidden;
        public Dictionary<string, double> metrics = new Dictionary<string, double>();

        public string name;
        public int offset;

        public float[] pl_averages = new float[19];
        public Dictionary<string, double> pl_metrics = new Dictionary<string, double>();
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamStats" /> class.
        /// </summary>
        public TeamStats()
        {
            prepareEmpty();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamStats" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TeamStats(string name) : this()
        {
            this.name = name;
            displayName = name;
        }

        public int division
        {
            get { return _division; }
            set
            {
                _division = value;
                try
                {
                    conference = MainWindow.Divisions.Find(division1 => division1.ID == value).ConferenceID;
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Prepares an empty TeamStats instance.
        /// </summary>
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
            division = 0;
            conference = 0;
        }

        /// <summary>
        /// Calculates the averages of a team's stats.
        /// </summary>
        public void CalcAvg()
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

        /// <summary>
        /// Calculates the league averages.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="statRange">The stat range.</param>
        /// <returns></returns>
        public static TeamStats CalculateLeagueAverages(Dictionary<int, TeamStats> tst, Span statRange)
        {
            var ls = new TeamStats("League");
            uint teamCount = CountTeams(tst, statRange);
            for (int i = 0; i < tst.Count; i++)
            {
                ls.AddTeamStats(tst[i], statRange);
            }
            ls.CalcMetrics(ls, (statRange == Span.Playoffs));

            ls.winloss[0] /= teamCount;
            ls.winloss[1] /= teamCount;
            ls.pl_winloss[0] /= teamCount;
            ls.pl_winloss[1] /= teamCount;
            ls.averages[t.Weff] /= teamCount;
            ls.pl_averages[t.Weff] /= teamCount;

            return ls;
        }

        /// <summary>
        /// Calculates the team metrics for all the teams.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstopp">The opposing team stats dictionary.</param>
        /// <param name="playoffs">if set to <c>true</c>, the metric stats will be calculated for the playoff performances of the teams.</param>
        public static void CalculateAllMetrics(ref Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstopp, bool playoffs = false)
        {
            for (int i = 0; i < tst.Count; i++)
            {
                tst[i].CalcMetrics(tstopp[i], playoffs);
            }
        }

        /// <summary>
        /// Counts the teams having more than one game in a specific time-span of the league's calendar.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="statRange">The stat range.</param>
        /// <returns></returns>
        private static uint CountTeams(Dictionary<int, TeamStats> tst, Span statRange)
        {
            uint teamCount = 0;

            if (statRange != Span.Playoffs)
            {
                for (int i = 0; i < tst.Count; i++)
                {
                    if (tst[i].getGames() > 0)
                        teamCount++;
                }
            }
            else
            {
                for (int i = 0; i < tst.Count; i++)
                {
                    if (tst[i].getPlayoffGames() > 0)
                        teamCount++;
                }
            }
            return (teamCount != 0) ? teamCount : 1;
        }

        /// <summary>
        /// Calculates the metric stats for this team.
        /// </summary>
        /// <param name="tsopp">The opposing team stats.</param>
        /// <param name="playoffs">if set to <c>true</c>, the metrics will be calculated based on the team's playoff performances.</param>
        public void CalcMetrics(TeamStats tsopp, bool playoffs = false)
        {
            var temp_metrics = new Dictionary<string, double>();

            var tstats = new double[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                if (!playoffs)
                    tstats[i] = stats[i];
                else
                    tstats[i] = pl_stats[i];
            }

            var toppstats = new double[tsopp.stats.Length];
            for (int i = 0; i < tsopp.stats.Length; i++)
            {
                if (!playoffs)
                    toppstats[i] = tsopp.stats[i];
                else
                    toppstats[i] = tsopp.pl_stats[i];
            }

            uint games = (!playoffs) ? getGames() : getPlayoffGames();

            double Poss = CalcPossMetric(tstats, toppstats);
            temp_metrics.Add("Poss", Poss);
            temp_metrics.Add("PossPG", Poss/games);

            Poss = CalcPossMetric(toppstats, tstats);

            Dictionary<string, double> toppmetrics = (!playoffs) ? tsopp.metrics : tsopp.pl_metrics;
            try
            {
                toppmetrics.Add("Poss", Poss);
            }
            catch
            {
            }

            double Pace = MainWindow.gameLength*((temp_metrics["Poss"] + toppmetrics["Poss"])/(2*(tstats[t.MINS])));
            temp_metrics.Add("Pace", Pace);

            double ORTG = (tstats[t.PF]/temp_metrics["Poss"])*100;
            temp_metrics.Add("ORTG", ORTG);

            double DRTG = (tstats[t.PA]/temp_metrics["Poss"])*100;
            temp_metrics.Add("DRTG", DRTG);

            double ASTp = 100*(tstats[t.AST])/(tstats[t.FGA] + tstats[t.FTA]*0.44 + tstats[t.AST] + tstats[t.TO]);
            temp_metrics.Add("AST%", ASTp);

            double DREBp = 100*tstats[t.DREB]/(tstats[t.DREB] + toppstats[t.OREB]);
            temp_metrics.Add("DREB%", DREBp);

            double EFGp = (tstats[t.FGM] + tstats[t.TPM]*0.5)/tstats[t.FGA];
            temp_metrics.Add("EFG%", EFGp);

            double EFFd = ORTG - DRTG;
            temp_metrics.Add("EFFd", EFFd);

            double TOR = tstats[t.TO]/(tstats[t.FGA] + 0.44*tstats[t.FTA] + tstats[t.TO]);
            temp_metrics.Add("TOR", TOR);

            double OREBp = 100*tstats[t.OREB]/(tstats[t.OREB] + toppstats[t.DREB]);
            temp_metrics.Add("OREB%", OREBp);

            double FTR = tstats[t.FTM]/tstats[t.FGA];
            temp_metrics.Add("FTR", FTR);

            float[] taverages = (!playoffs) ? averages : pl_averages;

            double PWp = (((taverages[t.PPG] - taverages[t.PAPG])*2.7) +
                         ((double) MainWindow.seasonLength/2))/MainWindow.seasonLength;
            temp_metrics.Add("PW%", PWp);

            double TSp = tstats[t.PF]/(2*(tstats[t.FGA] + 0.44*tstats[t.FTA]));
            temp_metrics.Add("TS%", TSp);

            double TPR = tstats[t.TPA]/tstats[t.FGA];
            temp_metrics.Add("3PR", TPR);

            double PythW = MainWindow.seasonLength*(Math.Pow(tstats[t.PF], 16.5))/(Math.Pow(tstats[t.PF], 16.5) + Math.Pow(tstats[t.PA], 16.5));
            temp_metrics.Add("PythW", PythW);

            double PythL = MainWindow.seasonLength - PythW;
            temp_metrics.Add("PythL", PythL);

            double GmSc = tstats[t.PF] + 0.4*tstats[t.FGM] - 0.7*tstats[t.FGA] - 0.4*(tstats[t.FTA] - tstats[t.FTM]) + 0.7*tstats[t.OREB] +
                          0.3*tstats[t.DREB] + tstats[t.STL] + 0.7*tstats[t.AST] + 0.7*tstats[t.BLK] - 0.4*tstats[t.FOUL] - tstats[t.TO];
            temp_metrics.Add("GmSc", GmSc/games);


            if (!playoffs)
                metrics = new Dictionary<string, double>(temp_metrics);
            else
                pl_metrics = new Dictionary<string, double>(temp_metrics);
        }

        /// <summary>
        /// Calculates the Possessions metric.
        /// </summary>
        /// <param name="tstats">The team stats.</param>
        /// <param name="toppstats">The opposing team stats.</param>
        /// <returns></returns>
        private static double CalcPossMetric(double[] tstats, double[] toppstats)
        {
            double Poss = 0.5*
                          ((tstats[t.FGA] + 0.4*tstats[t.FTA] -
                            1.07*(tstats[t.OREB]/(tstats[t.OREB] + toppstats[t.DREB]))*(tstats[t.FGA] - tstats[t.FGM]) + tstats[t.TO]) +
                           (toppstats[t.FGA] + 0.4*toppstats[t.FTA] -
                            1.07*(toppstats[t.OREB]/(toppstats[t.OREB] + tstats[t.DREB]))*(toppstats[t.FGA] - toppstats[t.FGM]) +
                            toppstats[t.TO]));
            return Poss;
        }

        /// <summary>
        /// Gets the amount of games played by the team.
        /// </summary>
        /// <returns></returns>
        internal uint getGames()
        {
            uint games = winloss[0] + winloss[1];
            return games;
        }

        /// <summary>
        /// Gets the amount of playoff games played by the team.
        /// </summary>
        /// <returns></returns>
        internal uint getPlayoffGames()
        {
            uint pl_games = pl_winloss[0] + pl_winloss[1];
            return pl_games;
        }

        /// <summary>
        /// Adds the team stats from a TeamStats instance to the current stats.
        /// </summary>
        /// <param name="ts">The team stats to add.</param>
        /// <param name="mode">The time-span.</param>
        /// <exception cref="System.Exception">Team Add Stats called with invalid parameter.</exception>
        public void AddTeamStats(TeamStats ts, Span mode)
        {
            switch (mode)
            {
                case Span.Season:
                {
                    winloss[0] += ts.winloss[0];
                    winloss[1] += ts.winloss[1];

                    for (int i = 0; i < stats.Length; i++)
                    {
                        stats[i] += ts.stats[i];
                    }

                    CalcAvg();
                    break;
                }
                case Span.Playoffs:
                {
                    pl_winloss[0] += ts.pl_winloss[0];
                    pl_winloss[1] += ts.pl_winloss[1];

                    for (int i = 0; i < pl_stats.Length; i++)
                    {
                        pl_stats[i] += ts.pl_stats[i];
                    }

                    CalcAvg();
                    break;
                }
                case Span.SeasonAndPlayoffs:
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

                    CalcAvg();
                    break;
                }
                default:
                {
                    throw new Exception("Team Add Stats called with invalid parameter: " + mode);
                }
            }
        }

        /// <summary>
        /// Resets the stats.
        /// </summary>
        /// <param name="mode">The time-span.</param>
        /// <exception cref="System.Exception">Team Reset Stats called with invalid parameter.</exception>
        public void ResetStats(Span mode)
        {
            switch (mode)
            {
                case Span.Season:
                {
                    winloss[0] = 0;
                    winloss[1] = 0;

                    for (int i = 0; i < stats.Length; i++)
                    {
                        stats[i] = 0;
                    }

                    CalcAvg();
                    break;
                }
                case Span.Playoffs:
                {
                    pl_winloss[0] = 0;
                    pl_winloss[1] = 0;

                    for (int i = 0; i < pl_stats.Length; i++)
                    {
                        pl_stats[i] = 0;
                    }

                    CalcAvg();
                    break;
                }
                case Span.SeasonAndPlayoffs:
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

                    CalcAvg();
                    break;
                }
                default:
                {
                    throw new Exception("Team Reset Stats called with invalid parameter: " + mode);
                }
            }
        }

        /// <summary>
        /// Calculates the team rankings for each stat.
        /// </summary>
        /// <param name="_teamStats">The team stats dictionary.</param>
        /// <param name="playoffs">if set to <c>true</c>, the rankings will be calculated based on the teams' playoff performances.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Presents the team's averages and rankings in a well-formatted multi-line string.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="TeamOrder">The team order.</param>
        /// <returns></returns>
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
                    "Win %: {32:F3} ({33})\nWin eff: {34:F2} ({35})\n\nPPG: {0:F1} ({16})\nPAPG: {1:F1} ({17})\n\nFG%: {2:F3} ({18})\nFGeff: {3:F2} ({19})\n3P%: {4:F3} ({20})\n3Peff: {5:F2} ({21})\n" +
                    "FT%: {6:F3} ({22})\nFTeff: {7:F2} ({23})\n\nRPG: {8:F1} ({24})\nORPG: {9:F1} ({25})\nDRPG: {10:F1} ({26})\n\nSPG: {11:F1} ({27})\nBPG: {12:F1} ({28})\n" +
                    "TPG: {13:F1} ({29})\nAPG: {14:F1} ({30})\nFPG: {15:F1} ({31})", tst[id].averages[t.PPG], tst[id].averages[t.PAPG],
                    tst[id].averages[t.FGp], tst[id].averages[t.FGeff], tst[id].averages[t.TPp], tst[id].averages[t.TPeff],
                    tst[id].averages[t.FTp], tst[id].averages[t.FTeff], tst[id].averages[t.RPG], tst[id].averages[t.ORPG],
                    tst[id].averages[t.DRPG], tst[id].averages[t.SPG], tst[id].averages[t.BPG], tst[id].averages[t.TPG],
                    tst[id].averages[t.APG], tst[id].averages[t.FPG], rating[id][0], tst.Count + 1 - rating[id][1], rating[id][2],
                    rating[id][3], rating[id][4], rating[id][5], rating[id][6], rating[id][7], rating[id][8], rating[id][9], rating[id][10],
                    rating[id][11], rating[id][12], tst.Count + 1 - rating[id][13], rating[id][14], tst.Count + 1 - rating[id][15],
                    tst[id].averages[t.Wp], rating[id][16], tst[id].averages[t.Weff], rating[id][t.Weff]);
            return text;
        }

        /// <summary>
        /// Gets the winning percentage.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <returns></returns>
        public float getWinningPercentage(Span span)
        {
            if (span == Span.Season)
            {
                return winloss[0]/getGames();
            }
            else if (span == Span.Playoffs)
            {
                return pl_winloss[0]/getGames();
            }
            else
            {
                return (winloss[0] + pl_winloss[0])/(getGames() + getPlayoffGames());
            }
        }

        /// <summary>
        /// Returns a well-formatted multi-line string presenting a scouting report for the team in natural language.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="psrList"> </param>
        /// <returns></returns>
        public string ScoutingReport(Dictionary<int, TeamStats> tst, ObservableCollection<PlayerStatsRow> psrList)
        {
            var pgList = psrList.Where(ps => ps.Position1 == Position.PG).ToList();
            pgList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps1.GmSc));
            pgList.Reverse();
            var sgList = psrList.Where(ps => ps.Position1 == Position.SG).ToList();
            sgList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            sgList.Reverse();
            var sfList = psrList.Where(ps => ps.Position1 == Position.SF).ToList();
            sfList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            sfList.Reverse();
            var pfList = psrList.Where(ps => ps.Position1 == Position.PF).ToList();
            pfList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            pfList.Reverse();
            var cList = psrList.Where(ps => ps.Position1 == Position.C).ToList();
            cList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            cList.Reverse();

            var roster = "Team Roster\n";
            roster += "\nPG: ";
            pgList.ForEach(ps => roster += ps.FirstName + " " + ps.LastName + ", ");
            roster = roster.Remove(roster.Length - 2);
            roster += "\nSG: ";
            sgList.ForEach(ps => roster += ps.FirstName + " " + ps.LastName + ", ");
            roster = roster.Remove(roster.Length - 2);
            roster += "\nSF: ";
            sfList.ForEach(ps => roster += ps.FirstName + " " + ps.LastName + ", ");
            roster = roster.Remove(roster.Length - 2);
            roster += "\nPF: ";
            pfList.ForEach(ps => roster += ps.FirstName + " " + ps.LastName + ", ");
            roster = roster.Remove(roster.Length - 2);
            roster += "\nC: ";
            cList.ForEach(ps => roster += ps.FirstName + " " + ps.LastName + ", ");
            roster = roster.Remove(roster.Length - 2);

            int[][] rating = CalculateTeamRankings(tst);
            int teamCount = tst.Count;
            int divpos = 0;
            int confpos = 0;

            var divTeams = tst.Where(pair => pair.Value.division == division).ToList();
            divTeams.Sort((t1, t2) => t1.Value.getWinningPercentage(Span.Season).CompareTo(t2.Value.getWinningPercentage(Span.Season)));
            divTeams.Reverse();
            for (int i = 0; i < divTeams.Count; i++)
            {
                if (divTeams[i].Value.ID == ID)
                {
                    divpos = i + 1;
                    break;
                }
            }
            var confTeams = tst.Where(pair => pair.Value.conference == conference).ToList();
            confTeams.Sort((t1, t2) => t1.Value.getWinningPercentage(Span.Season).CompareTo(t2.Value.getWinningPercentage(Span.Season)));
            confTeams.Reverse();
            for (int i = 0; i < confTeams.Count; i++)
            {
                if (confTeams[i].Value.ID == ID)
                {
                    confpos = i + 1;
                    break;
                }
            }

            string msg = roster + "\n\n===================================================\n\n";
            msg += String.Format("{0}, the {1}", displayName, rating[ID][17]);
            switch (rating[ID][17])
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

            int topThird = teamCount/3;
            int secondThird = teamCount/3*2;
            int topHalf = teamCount/2;

            msg += " strongest team in the league right now, after having played " + rating[ID][19].ToString() + " games. Their record is " +
                   "currently at " + winloss[0] + "-" + winloss[1];

            if (MainWindow.Divisions.Count > 1) 
                msg += ", putting them at #" + divpos + " in their division and at #" + confpos + " in their conference";

            msg += ".\n\n";

            if ((rating[ID][3] <= 5) && (rating[ID][5] <= 5))
            {
                if (rating[ID][7] <= 5)
                {
                    msg += "This team just can't be beaten offensively. One of the strongest in the league in all aspects.";
                }
                else
                {
                    msg += "Great team offensively. Even when they don't get to the line, they know how to raise the bar with " +
                           "efficiency in both 2 and 3 pointers.";
                }
            }
            else if ((rating[ID][3] <= topThird) && (rating[ID][5] <= topThird))
            {
                if (rating[ID][7] <= topThird)
                {
                    msg += "Top third of the league in everything offense, and they're one to worry about.";
                }
                else
                {
                    msg += "Although their free throwing is not on par with their other offensive qualities, you can't relax " +
                           "when playing against them. Top third of the league in field goals and 3 pointers.";
                }
            }
            else if ((rating[ID][3] <= secondThird) && (rating[ID][5] <= secondThird))
            {
                if (rating[ID][7] <= topThird)
                {
                    msg += "Although an average offensive team (they can't seem to remain consistent from both inside and " +
                           "outside the arc), they can get back at you with their efficiency from the line.";
                }
                else
                {
                    msg += "Average offensive team. Not really efficient in anything they do when they bring the ball down " + "the court.";
                }
            }
            else
            {
                if (rating[ID][7] <= topThird)
                {
                    msg += "They aren't consistent from the floor, but still manage to get to the line enough times and " +
                           "be good enough to make a difference.";
                }
                else
                {
                    msg += "One of the most inconsistent teams at the offensive end, and they aren't efficient enough from " +
                           "the line to make up for it.";
                }
            }
            msg += "\n\n";

            if (rating[ID][3] <= 5)
                msg += "Top scoring team, one of the top 5 in field goal efficiency.";
            else if (rating[ID][3] <= topThird)
                msg += "You'll have to worry about their scoring efficiency, as they're in the top third of the league.";
            else if (rating[ID][3] <= secondThird)
                msg += "Scoring is not their virtue, but they're not that bad either.";
            else if (rating[ID][3] <= teamCount)
                msg += "You won't have to worry about their scoring, one of the least 10 efficient in the league.";

            int comp = rating[ID][t.FGeff] - rating[ID][t.FGp];
            if (comp < -topHalf)
                msg += "\nThey score more baskets than their FG% would have you guess, but they need to work on getting more consistent.";
            else if (comp > topHalf)
                msg +=
                    "\nThey can be dangerous whenever they shoot the ball. Their offense just doesn't get them enough chances to shoot it, though.";

            msg += String.Format(" (#{0} in FG%: {1:F3} - #{2} in FGeff: {3:F2})", rating[ID][t.FGp], averages[t.FGp], rating[ID][t.FGeff],
                                 averages[t.FGeff]);
            msg += "\n";

            if (rating[ID][5] <= 5)
                msg += "You'll need to always have an eye on the perimeter. They can turn a game around with their 3 pointers. " +
                       "They score well, they score a lot.";
            else if (rating[ID][5] <= topThird)
                msg += "Their 3pt shooting is bad news. They're in the top third of the league, and you can't relax playing against them.";
            else if (rating[ID][5] <= secondThird)
                msg += "Not much to say about their 3pt shooting. Average, but it is there.";
            else if (rating[ID][5] <= teamCount)
                msg += "Definitely not a threat from 3pt land, one of the worst in the league. They waste too many shots from there.";

            comp = rating[ID][t.TPeff] - rating[ID][t.TPp];
            if (comp < -topHalf)
                msg += "\nThey'll get enough 3 pointers to go down each night, but not on a good enough percentage for that amount.";
            else if (comp > topHalf)
                msg += "\nWith their accuracy from the 3PT line, you'd think they'd shoot more of those.";

            msg += String.Format(" (#{0} in 3P%: {1:F3} - #{2} in 3Peff: {3:F2})", rating[ID][t.TPp], averages[t.TPp], rating[ID][t.TPeff],
                                 averages[t.TPeff]);
            msg += "\n";

            if (rating[ID][7] <= 5)
                msg += "They tend to attack the lanes hard, getting to the line and making the most of it. They're one of the best " +
                       "teams in the league at it.";
            else if (rating[ID][7] <= topThird)
                msg +=
                    "One of the best teams in the league at getting to the line. They get enough free throws to punish the opposing team every night. Top third of the league.";
            else if (rating[ID][7] <= secondThird)
                msg +=
                    "Average free throw efficiency, you don't have to worry about sending them to the line; at least as much as other aspects of their game.";
            else if (rating[ID][7] <= teamCount)
                if (rating[ID][t.FTp] < topHalf)
                    msg +=
                        "A team that you'll enjoy playing hard and aggressively against on defense. They don't know how to get to the line.";
                else
                    msg +=
                        "A team that doesn't know how to get to the line, or how to score from there. You don't have to worry about freebies against them.";

            msg += String.Format(" (#{0} in FT%: {1:F3} - #{2} in FTeff: {3:F2})", rating[ID][t.FTp], averages[t.FTp], rating[ID][t.FTeff],
                                 averages[t.FTeff]);
            comp = rating[ID][t.FTeff] - rating[ID][t.FTp];
            if (comp < -topHalf)
                msg +=
                    "\nAlthough they get to the line a lot and make some free throws, they have to put up a lot to actually get that amount each night.";
            else if (comp > topHalf)
                msg += "\nThey're lethal when shooting free throws, but they need to play harder and get there more often.";

            msg += "\n";

            if (rating[ID][14] <= topHalf)
                msg +=
                    "They know how to find the open man, and they get their offense going by getting it around the perimeter until a clean shot is there.";
            else if ((rating[ID][14] > topHalf) && (rating[ID][3] < topThird))
                msg +=
                    "A team that prefers to run its offense through its core players in isolation. Not very good in assists, but they know how to get the job " +
                    "done more times than not.";
            else
                msg +=
                    "A team that seems to have some selfish players around, nobody really that efficient to carry the team into high percentages.";
            
            msg += String.Format(" (#{0} in APG: {1:F1})", rating[ID][t.APG], averages[t.APG]);
            msg += "\n\n";

            if (31 - rating[ID][t.PAPG] <= 5)
                msg += "Don't expect to get your score high against them. An elite defensive team, top 5 in points against them each night.";
            else if (31 - rating[ID][t.PAPG] <= topThird)
                msg += "One of the better defensive teams out there, limiting their opponents to low scores night in, night out.";
            else if (31 - rating[ID][t.PAPG] <= secondThird)
                msg += "Average defensively, not much to show for it, but they're no blow-outs.";
            else if (31 - rating[ID][t.PAPG] <= teamCount)
                msg += "This team has just forgotten what defense is. They're one of the 10 easiest teams to score against.";

            msg += String.Format(" (#{0} in PAPG: {1:F1})", tst.Count + 1 - rating[ID][t.PAPG], averages[t.PAPG]);
            msg += "\n\n";

            if ((rating[ID][9] <= topThird) && (rating[ID][11] <= topThird) && (rating[ID][12] <= topThird))
                msg +=
                    "Hustle is their middle name. They attack the offensive glass, they block, they steal. Don't even dare to blink or get complacent.\n\n";
            else if ((rating[ID][9] >= secondThird) && (rating[ID][11] >= secondThird) && (rating[ID][12] >= secondThird))
                msg += "This team just doesn't know what hustle means. You'll be doing circles around them if you're careful.\n\n";

            if (rating[ID][8] <= 5)
                msg += "Sensational rebounding team, everybody jumps for the ball, no missed shot is left loose.";
            else if (rating[ID][8] <= topThird)
                msg += "You can't ignore their rebounding ability, they work together and are in the top third of the league in rebounding.";
            else if (rating[ID][8] <= secondThird)
                msg += "They crash the boards as much as the next guy, but they won't give up any freebies.";
            else if (rating[ID][8] <= teamCount)
                msg +=
                    "Second chance points? One of their biggest fears. Low low LOW rebounding numbers; just jump for the ball and you'll keep your score high.";

            msg += " ";

            if ((rating[ID][9] <= topThird) && (rating[ID][10] <= topThird))
                msg +=
                    "The work they put on rebounding on both sides of the court is commendable. Both offensive and defensive rebounds, their bread and butter.";

            msg += String.Format(" (#{0} in RPG: {1:F1}, #{2} in ORPG: {3:F1}, #{4} in DRPG: {5:F1})", rating[ID][t.RPG], averages[t.RPG], rating[ID][t.ORPG], 
                averages[t.ORPG], rating[ID][t.DRPG], averages[t.DRPG]);
            msg += "\n\n";

            if ((rating[ID][11] <= topThird) && (rating[ID][12] <= topThird))
                msg +=
                    "A team that knows how to play defense. They're one of the best in steals and blocks, and they make you work hard on offense.";
            else if (rating[ID][11] <= topThird)
                msg +=
                    "Be careful dribbling and passing. They won't be much trouble once you shoot the ball, but the trouble is getting there. Great in steals.";
            else if (rating[ID][12] <= topThird)
                msg +=
                    "Get that thing outta here! Great blocking team, they turn the lights off on any mismatched jumper or drive; sometimes even when you least expect it.";
            else
                msg +=
                    "Nothing too significant as far as blocks and steals go.";
            msg += String.Format(" (#{0} in SPG: {1:F1}, #{2} in BPG: {3:F1})\n", rating[ID][t.SPG], averages[t.SPG], rating[ID][t.BPG], averages[t.BPG]);

            if ((rating[ID][13] <= topThird) && (rating[ID][15] <= topThird))
                msg +=
                    "Clumsy team to say the least. They're not careful with the ball, and they foul too much. Keep your eyes open and play hard.";
            else if (rating[ID][13] < topThird)
                msg +=
                    "Not good ball handlers, and that's being polite. Bottom 10 in turnovers, they have work to do until they get their offense going.";
            else if (rating[ID][15] < topThird)
                msg += "A team that's prone to fouling. You better drive the lanes as hard as you can, you'll get to the line a lot.";
            else
                msg +=
                    "This team is careful with and without the ball. They're good at keeping their turnovers down, and don't foul too much.\nDon't throw " +
                    "your players into steals or fouls against them, because they play smart, and you're probably going to see the opposite call than the " +
                    "one you expected.";
            msg += String.Format(" (#{0} in TPG: {1:F1}, #{2} in FPG: {3:F1})", tst.Count + 1 - rating[ID][t.TPG], averages[t.TPG], tst.Count + 1 - rating[ID][t.FPG], averages[t.FPG]);

            msg += "\n\n";

            msg += "In summary, their best areas are ";
            var dict = new Dictionary<int, int>();
            for (int k = 0; k < rating[ID].Length;k++)
            {
                dict.Add(k, rating[ID][k]);
            }
            dict[t.FPG] = tst.Count + 1 - dict[t.FPG];
            dict[t.TPG] = tst.Count + 1 - dict[t.TPG];
            dict[t.PAPG] = tst.Count + 1 - dict[t.PAPG];
            var strengths = (from entry in dict orderby entry.Value ascending select entry.Key).ToList();
            int m = 0;
            int j = 5;
            while (true)
            {
                if (m == j)
                    break;
                switch (strengths[m])
                {
                    case t.APG:
                        msg += String.Format("assists (#{0}, {1:F1}), ", rating[ID][t.APG], averages[t.APG]);
                        break;
                    case t.BPG:
                        msg += String.Format("blocks (#{0}, {1:F1}), ", rating[ID][t.BPG], averages[t.BPG]);
                        break;
                    case t.DRPG:
                        msg += String.Format("defensive rebounds (#{0}, {1:F1}), ", rating[ID][t.DRPG], averages[t.DRPG]);
                        break;
                    case t.FGeff:
                        msg += String.Format("field goals (#{0}, {1:F1} per game on {2:F3}), ", rating[ID][t.FGeff], (double)stats[t.FGM]/getGames(), averages[t.FGp]);
                        break;
                    case t.FPG:
                        msg += String.Format("fouls (#{0}, {1:F1}), ", tst.Count + 1 - rating[ID][t.FPG], averages[t.FPG]);
                        break;
                    case t.FTeff:
                        msg += String.Format("free throws (#{0}, {1:F1} per game on {2:F3}), ", rating[ID][t.FTeff], (double)stats[t.FTM] / getGames(), averages[t.FTp]);
                        break;
                    case t.ORPG:
                        msg += String.Format("offensive rebounds (#{0}, {1:F1}), ", rating[ID][t.ORPG], averages[t.ORPG]);
                        break;
                    case t.PAPG:
                        msg += String.Format("points allowed per game (#{0}, {1:F1}), ", tst.Count + 1 - rating[ID][t.PAPG], averages[t.PAPG]);
                        break;
                    case t.PPG:
                        msg += String.Format("scoring (#{0}, {1:F1}), ", rating[ID][t.PPG], averages[t.PPG]);
                        break;
                    case t.RPG:
                        msg += String.Format("rebounds (#{0}, {1:F1}), ", rating[ID][t.RPG], averages[t.RPG]);
                        break;
                    case t.SPG:
                        msg += String.Format("steals (#{0}, {1:F1}), ", rating[ID][t.SPG], averages[t.SPG]);
                        break;
                    case t.TPG:
                        msg += String.Format("turnovers (#{0}, {1:F1}), ", tst.Count + 1 - rating[ID][t.TPG], averages[t.TPG]);
                        break;
                    case t.TPeff:
                        msg += String.Format("three-pointers (#{0}, {1:F1} per game on {2:F3}), ", rating[ID][t.TPeff], (double)stats[t.TPM] / getGames(), averages[t.TPp]);
                        break;
                    default:
                        j++;
                        break;
                }
                m++;
            }
            msg = msg.TrimEnd(new char[] {' ', ','});
            msg += ".";
            return msg;
        }

        /// <summary>
        /// Determines whether the team is hidden for the current season.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="name">The name of the team.</param>
        /// <param name="season">The season ID.</param>
        /// <returns>
        ///   <c>true</c> if the team is hidden; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTeamHiddenInSeason(string file, string name, int season)
        {
            var db = new SQLiteDatabase(file);
            int maxSeason = SQLiteIO.getMaxSeason(file);
            string teamsT = "Teams";
            if (season != maxSeason)
                teamsT += "S" + season;

            string q = "select isHidden from " + teamsT + " where Name LIKE \"" + name + "\"";
            bool isHidden = Tools.getBoolean(db.GetDataTable(q).Rows[0], "isHidden");

            return isHidden;
        }

        /// <summary>
        /// Adds the team stats from a box score.
        /// </summary>
        /// <param name="bsToAdd">The box score to add.</param>
        /// <param name="ts1">The first team's team stats.</param>
        /// <param name="ts2">The second team's team stats.</param>
        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref TeamStats ts1, ref TeamStats ts2, bool ignorePlayoffFlag = false)
        {
            var _tst = new Dictionary<int, TeamStats> {{1, ts1}, {2, ts2}};
            var _tstopp = new Dictionary<int, TeamStats> {{1, new TeamStats()}, {2, new TeamStats()}};
            AddTeamStatsFromBoxScore(bsToAdd, ref _tst, ref _tstopp, 1, 2, ignorePlayoffFlag);
            ts1 = _tst[1];
            ts2 = _tst[2];
        }

        /// <summary>
        /// Adds the team stats from a box score.
        /// </summary>
        /// <param name="bsToAdd">The box score to add.</param>
        /// <param name="_tst">The team stats dictionary.</param>
        /// <param name="_tstopp">The opposing team stats dictionary.</param>
        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref Dictionary<int, TeamStats> _tst,
                                                    ref Dictionary<int, TeamStats> _tstopp)
        {
            int id1 = MainWindow.TeamOrder[bsToAdd.Team1];
            int id2 = MainWindow.TeamOrder[bsToAdd.Team2];

            AddTeamStatsFromBoxScore(bsToAdd, ref _tst, ref _tstopp, id1, id2);
        }

        /// <summary>
        /// Adds the team stats from a box score.
        /// </summary>
        /// <param name="bsToAdd">The box score to add.</param>
        /// <param name="_tst">The team stats dictionary.</param>
        /// <param name="_tstopp">The opposing team stats dictionary.</param>
        /// <param name="id1">The away team's ID.</param>
        /// <param name="id2">The home team's ID.</param>
        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref Dictionary<int, TeamStats> _tst,
                                                    ref Dictionary<int, TeamStats> _tstopp, int id1, int id2, bool ignorePlayoffFlag = false)
        {
            TeamStats ts1 = _tst[id1];
            TeamStats ts2 = _tst[id2];
            TeamStats tsopp1 = _tstopp[id1];
            TeamStats tsopp2 = _tstopp[id2];
            if (!bsToAdd.isPlayoff || ignorePlayoffFlag)
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

            ts1.CalcAvg();
            ts2.CalcAvg();
            tsopp1.CalcAvg();
            tsopp2.CalcAvg();

            _tst[id1] = ts1;
            _tst[id2] = ts2;
            _tstopp[id1] = tsopp1;
            _tstopp[id2] = tsopp2;
        }

        /// <summary>
        /// Checks for teams in divisions that don't exist anymore, and reassings them to the first available division.
        /// </summary>
        public static void CheckForInvalidDivisions()
        {
            var db = new SQLiteDatabase(MainWindow.currentDB);
            var usedIDs = new List<int>();
            db.GetDataTable("SELECT ID FROM Divisions").Rows.Cast<DataRow>().ToList().ForEach(row => usedIDs.Add(Tools.getInt(row, "ID")));

            var teamsChanged = new List<string>();

            int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
            for (int i = maxSeason; i >= 1; i--)
            {
                string teamsT = "Teams";
                string pl_teamsT = "PlayoffTeams";
                string oppT = "Opponents";
                string pl_oppT = "PlayoffOpponents";
                if (i != maxSeason)
                {
                    string toAdd = "S" + i;
                    teamsT += toAdd;
                    pl_teamsT += toAdd;
                    oppT += toAdd;
                    pl_oppT += toAdd;
                }

                var tables = new List<string> {teamsT, pl_teamsT, oppT, pl_oppT};
                foreach (string table in tables)
                {
                    string q = "SELECT ID, Name, Division FROM " + table;
                    DataTable res = db.GetDataTable(q);

                    foreach (DataRow r in res.Rows)
                    {
                        if (usedIDs.Contains(Tools.getInt(r, "Division")) == false)
                        {
                            db.Update(table, new Dictionary<string, string> {{"Division", MainWindow.Divisions.First().ID.ToString()}},
                                      "ID = " + Tools.getString(r, "ID"));
                            int teamid = MainWindow.TeamOrder[Tools.getString(r, "Name")];
                            MainWindow.tst[teamid].division = MainWindow.Divisions.First().ID;
                            if (teamsChanged.Contains(MainWindow.tst[teamid].displayName) == false)
                                teamsChanged.Add(MainWindow.tst[teamid].displayName);
                        }
                    }
                }
            }

            if (teamsChanged.Count > 0)
            {
                teamsChanged.Sort();
                string s = "Some teams were in divisions that were deleted and have been reset to the " + MainWindow.Divisions.First().Name +
                           " division.\n\n";
                teamsChanged.ForEach(s1 => s += s1 + "\n");
                s = s.TrimEnd(new[] {'\n'});
                SQLiteIO.saveSeasonToDatabase();
                MessageBox.Show(s);
            }
        }

        /// <summary>
        /// Adds one or more box scores resulting from an SQLite query to a TeamStats instance.
        /// </summary>
        /// <param name="res">The result of the query containing the box score records.</param>
        /// <param name="ts">The TeamStats instance to be modified.</param>
        /// <param name="tsopp">The opposing TeamStats instance to be modified..</param>
        public static void AddToTeamStatsFromSQLBoxScores(DataTable res, ref TeamStats ts, ref TeamStats tsopp)
        {
            foreach (DataRow r in res.Rows)
            {
                AddToTeamStatsFromSQLBoxScore(r, ref ts, ref tsopp);
            }
        }

        /// <summary>
        /// Adds a box score resulting from an SQLite query to a TeamStats instance.
        /// </summary>
        /// <param name="r">The result of the query containing the box score record.</param>
        /// <param name="ts">The TeamStats instance to be modified.</param>
        /// <param name="tsopp">The opposing TeamStats instance to be modified.</param>
        public static void AddToTeamStatsFromSQLBoxScore(DataRow r, ref TeamStats ts, ref TeamStats tsopp)
        {
            bool playoffs = Tools.getBoolean(r, "isPlayoff");
            if (!playoffs)
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.name))
                {
                    if (t1pts > t2pts)
                        ts.winloss[0]++;
                    else
                        ts.winloss[1]++;
                    tsopp.stats[t.MINS] = ts.stats[t.MINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.stats[t.PA] = ts.stats[t.PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.stats[t.PF] = ts.stats[t.PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    ts.stats[t.OREB] += T1oreb;

                    ts.stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    ts.stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    ts.stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");

                    tsopp.stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    tsopp.stats[t.OREB] += T2oreb;

                    tsopp.stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    tsopp.stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    tsopp.stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts)
                        ts.winloss[0]++;
                    else
                        ts.winloss[1]++;
                    tsopp.stats[t.MINS] = ts.stats[t.MINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.stats[t.PA] = ts.stats[t.PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.stats[t.PF] = ts.stats[t.PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    ts.stats[t.OREB] += T2oreb;

                    ts.stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    ts.stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    ts.stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");

                    tsopp.stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    tsopp.stats[t.OREB] += T1oreb;

                    tsopp.stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    tsopp.stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    tsopp.stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");
                }

                tsopp.winloss[1] = ts.winloss[0];
                tsopp.winloss[0] = ts.winloss[1];
            }
            else
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.name))
                {
                    if (t1pts > t2pts)
                        ts.pl_winloss[0]++;
                    else
                        ts.pl_winloss[1]++;
                    tsopp.pl_stats[t.MINS] = ts.pl_stats[t.MINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.pl_stats[t.PA] = ts.pl_stats[t.PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.pl_stats[t.PF] = ts.pl_stats[t.PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.pl_stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.pl_stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.pl_stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.pl_stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.pl_stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.pl_stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.pl_stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    ts.pl_stats[t.OREB] += T1oreb;

                    ts.pl_stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.pl_stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.pl_stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    ts.pl_stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    ts.pl_stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");

                    tsopp.pl_stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.pl_stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.pl_stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.pl_stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.pl_stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.pl_stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.pl_stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    tsopp.pl_stats[t.OREB] += T2oreb;

                    tsopp.pl_stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.pl_stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.pl_stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    tsopp.pl_stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    tsopp.pl_stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts)
                        ts.pl_winloss[0]++;
                    else
                        ts.pl_winloss[1]++;
                    tsopp.pl_stats[t.MINS] = ts.pl_stats[t.MINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.pl_stats[t.PA] = ts.pl_stats[t.PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.pl_stats[t.PF] = ts.pl_stats[t.PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.pl_stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.pl_stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.pl_stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.pl_stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.pl_stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.pl_stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.pl_stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    ts.pl_stats[t.OREB] += T2oreb;

                    ts.pl_stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.pl_stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.pl_stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    ts.pl_stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    ts.pl_stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");

                    tsopp.pl_stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.pl_stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.pl_stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.pl_stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.pl_stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.pl_stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.pl_stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    tsopp.pl_stats[t.OREB] += T1oreb;

                    tsopp.pl_stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.pl_stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.pl_stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    tsopp.pl_stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    tsopp.pl_stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");
                }

                tsopp.pl_winloss[1] = ts.pl_winloss[0];
                tsopp.pl_winloss[0] = ts.pl_winloss[1];
            }

            ts.CalcAvg();
            tsopp.CalcAvg();
        }
    }

    /// <summary>
    /// Provides a presentation interface for a team's metric stats.
    /// </summary>
    public class TeamMetricStatsRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamMetricStatsRow" /> class.
        /// </summary>
        /// <param name="ts">The team's stats.</param>
        /// <param name="playoffs">if set to <c>true</c>, the row will present playoffs metric stats.</param>
        public TeamMetricStatsRow(TeamStats ts, bool playoffs = false)
        {
            Name = ts.displayName;
            if (!playoffs)
            {
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
                TSp = ts.metrics["TS%"];
                TPR = ts.metrics["3PR"];
                PythW = ts.metrics["PythW"];
                PythL = ts.metrics["PythL"];
            }
            else
            {
                Poss = ts.pl_metrics["PossPG"];
                Pace = ts.pl_metrics["Pace"];
                ORTG = ts.pl_metrics["ORTG"];
                DRTG = ts.pl_metrics["DRTG"];
                ASTp = ts.pl_metrics["AST%"];
                DREBp = ts.pl_metrics["DREB%"];
                EFGp = ts.pl_metrics["EFG%"];
                EFFd = ts.pl_metrics["EFFd"];
                TOR = ts.pl_metrics["TOR"];
                OREBp = ts.pl_metrics["OREB%"];
                FTR = ts.pl_metrics["FTR"];
                PWp = ts.pl_metrics["PW%"];
                TSp = ts.pl_metrics["TS%"];
                TPR = ts.pl_metrics["3PR"];
                PythW = ts.pl_metrics["PythW"];
                PythL = ts.pl_metrics["PythL"];
            }
        }

        public string Name { get; set; }
        public double ORTG { get; set; }
        public double DRTG { get; set; }
        public double EFFd { get; set; }
        public double PWp { get; set; }
        public double PythW { get; set; }
        public double PythL { get; set; }
        public double TSp { get; set; }
        public double EFGp { get; set; }
        public double TPR { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double ASTp { get; set; }
        public double TOR { get; set; }
        public double FTR { get; set; }
        public double Poss { get; set; }
        public double Pace { get; set; }
    }

    /// <summary>
    /// Used to determine the team ranking for each stat.
    /// </summary>
    public class TeamRankings
    {
        public int[][] rankings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRankings" /> class.
        /// </summary>
        /// <param name="_tst">The team stats dictionary containing all team stats.</param>
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