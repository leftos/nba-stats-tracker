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
using System.Globalization;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker.Data
{
    /// <summary>
    /// A list of constant pseudonyms for specific entries in the players' stats arrays.
    /// </summary>
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
    /// <summary>
    /// A container for all of a player's information, stats, averages and metrics handled by the program.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        public string FirstName;
        public int ID;
        public string LastName;
        public string Position1;
        public string Position2;
        public int Age;
        public int YearsPro;
        public string TeamF;
        public string TeamS = "";
        public float[] averages = new float[16];
        public bool isActive;
        public bool isAllStar;
        public bool isHidden;
        public bool isInjured;
        public bool isNBAChampion;
        public Dictionary<string, double> metrics = new Dictionary<string, double>();
        public float[] pl_averages = new float[16];
        public Dictionary<string, double> pl_metrics = new Dictionary<string, double>();
        public uint[] pl_stats = new uint[17];
        public uint[] stats = new uint[17];

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        public PlayerStats() : this(new Player())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="player">A Player instance containing the information to initialize with.</param>
        public PlayerStats(Player player)
        {
            ID = player.ID;
            LastName = player.LastName;
            FirstName = player.FirstName;
            Position1 = player.Position;
            Position2 = player.Position2;
            Age = 0;
            YearsPro = 0;
            TeamF = player.Team;
            isActive = true;
            isHidden = false;
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

            for (int i = 0; i < pl_stats.Length; i++)
            {
                pl_stats[i] = 0;
            }

            for (int i = 0; i < averages.Length; i++)
            {
                pl_averages[i] = 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="dataRow">A row of an SQLite query result containing player information.</param>
        /// <param name="playoffs">if set to <c>true</c>, the row is assumed to contain playoff stats.</param>
        public PlayerStats(DataRow dataRow, bool playoffs = false)
        {
            ID = Tools.getInt(dataRow, "ID");

            if (!playoffs)
            {
                LastName = Tools.getString(dataRow, "LastName");
                FirstName = Tools.getString(dataRow, "FirstName");
                Position1 = Tools.getString(dataRow, "Position1");
                if (String.IsNullOrEmpty(Position1))
                    Position1 = " ";
                Position2 = Tools.getString(dataRow, "Position2");
                if (String.IsNullOrEmpty(Position2))
                    Position2 = " ";
                TeamF = Tools.getString(dataRow, "TeamFin");
                TeamS = Tools.getString(dataRow, "TeamSta");
                isActive = Tools.getBoolean(dataRow, "isActive");

                // Backwards compatibility with databases that didn't have the field
                try
                {
                    isHidden = Tools.getBoolean(dataRow, "isHidden");
                }
                catch
                {
                    isHidden = false;
                }

                // Backwards compatibility with databases that didn't have the fields
                try
                {
                    Age = Tools.getInt(dataRow, "Age");
                    YearsPro = Tools.getInt(dataRow, "YearsPro");
                }
                catch
                {
                    Age = 0;
                    YearsPro = 0;
                }

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
            }
            else
            {
                pl_stats[p.GP] = Tools.getUInt16(dataRow, "GP");
                pl_stats[p.GS] = Tools.getUInt16(dataRow, "GS");
                pl_stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
                pl_stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");
                pl_stats[p.FGM] = Tools.getUInt16(dataRow, "FGM");
                pl_stats[p.FGA] = Tools.getUInt16(dataRow, "FGA");
                pl_stats[p.TPM] = Tools.getUInt16(dataRow, "TPM");
                pl_stats[p.TPA] = Tools.getUInt16(dataRow, "TPA");
                pl_stats[p.FTM] = Tools.getUInt16(dataRow, "FTM");
                pl_stats[p.FTA] = Tools.getUInt16(dataRow, "FTA");
                pl_stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
                pl_stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
                pl_stats[p.STL] = Tools.getUInt16(dataRow, "STL");
                pl_stats[p.TO] = Tools.getUInt16(dataRow, "TOS");
                pl_stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
                pl_stats[p.AST] = Tools.getUInt16(dataRow, "AST");
                pl_stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");
            }

            CalcAvg();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="Position1">The primary position.</param>
        /// <param name="Position2">The secondary position.</param>
        /// <param name="TeamF">The team the player is currently with.</param>
        /// <param name="TeamS">The team the player started the season with.</param>
        /// <param name="isActive">if set to <c>true</c> the player is currently active (i.e. signed with a team).</param>
        /// <param name="isHidden">if set to <c>true</c> the player is hidden for this season.</param>
        /// <param name="isInjured">if set to <c>true</c> the player is injured.</param>
        /// <param name="isAllStar">if set to <c>true</c> is an All-Star this season.</param>
        /// <param name="isNBAChampion">if set to <c>true</c> is a champion this season.</param>
        /// <param name="dataRow">A row of an SQLite query result containing player information.</param>
        /// <param name="playoffs">if set to <c>true</c> the row is assumed to contain playoff stats.</param>
        public PlayerStats(int ID, string LastName, string FirstName, string Position1, string Position2, int Age, int YearsPro, string TeamF, string TeamS,
                           bool isActive, bool isHidden, bool isInjured, bool isAllStar, bool isNBAChampion, DataRow dataRow,
                           bool playoffs = false)
        {
            this.ID = ID;
            this.LastName = LastName;
            this.FirstName = FirstName;
            this.Position1 = Position1;
            this.Position2 = Position2;
            this.TeamF = TeamF;
            this.TeamS = TeamS;
            this.Age = Age;
            this.YearsPro = YearsPro;
            this.isActive = isActive;
            this.isHidden = isHidden;
            this.isAllStar = isAllStar;
            this.isInjured = isInjured;
            this.isNBAChampion = isNBAChampion;

            try
            {
                if (!playoffs)
                {
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
                }
                else
                {
                    pl_stats[p.GP] = Tools.getUInt16(dataRow, "GP");
                    pl_stats[p.GS] = Tools.getUInt16(dataRow, "GS");
                    pl_stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
                    pl_stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");

                    string[] parts = Tools.getString(dataRow, "FG").Split('-');

                    pl_stats[p.FGM] = Convert.ToUInt16(parts[0]);
                    pl_stats[p.FGA] = Convert.ToUInt16(parts[1]);

                    parts = Tools.getString(dataRow, "3PT").Split('-');

                    pl_stats[p.TPM] = Convert.ToUInt16(parts[0]);
                    pl_stats[p.TPA] = Convert.ToUInt16(parts[1]);

                    parts = Tools.getString(dataRow, "FT").Split('-');

                    pl_stats[p.FTM] = Convert.ToUInt16(parts[0]);
                    pl_stats[p.FTA] = Convert.ToUInt16(parts[1]);

                    pl_stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
                    pl_stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
                    pl_stats[p.STL] = Tools.getUInt16(dataRow, "STL");
                    pl_stats[p.TO] = Tools.getUInt16(dataRow, "TO");
                    pl_stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
                    pl_stats[p.AST] = Tools.getUInt16(dataRow, "AST");
                    pl_stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("{0} {1} ({2}) has some invalid data.\n\nError: {3}", FirstName, LastName,
                                              TeamF, ex.Message));
            }

            CalcAvg();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="playerStatsRow">The player stats row.</param>
        /// <param name="playoffs">if set to <c>true</c> the row is assumed to contain playoff stats.</param>
        public PlayerStats(PlayerStatsRow playerStatsRow, bool playoffs = false)
        {
            LastName = playerStatsRow.LastName;
            FirstName = playerStatsRow.FirstName;

            if (!playoffs)
            {
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

                metrics["GmSc"] = playerStatsRow.GmSc;
                metrics["GmScE"] = playerStatsRow.GmScE;
                metrics["EFF"] = playerStatsRow.EFF;
                metrics["EFG%"] = playerStatsRow.EFGp;
                metrics["TS%"] = playerStatsRow.TSp;
                metrics["AST%"] = playerStatsRow.ASTp;
                metrics["STL%"] = playerStatsRow.STLp;
                metrics["TO%"] = playerStatsRow.TOp;
                metrics["USG%"] = playerStatsRow.USGp;
                metrics["PTSR"] = playerStatsRow.PTSR;
                metrics["REBR"] = playerStatsRow.REBR;
                metrics["OREBR"] = playerStatsRow.OREBR;
                metrics["ASTR"] = playerStatsRow.ASTR;
                metrics["BLKR"] = playerStatsRow.BLKR;
                metrics["STLR"] = playerStatsRow.STLR;
                metrics["TOR"] = playerStatsRow.TOR;
                metrics["FTR"] = playerStatsRow.FTR;
                metrics["PER"] = playerStatsRow.PER;
                metrics["BLK%"] = playerStatsRow.BLKp;
                metrics["DREB%"] = playerStatsRow.DREBp;
                metrics["OREB%"] = playerStatsRow.OREBp;
                metrics["REB%"] = playerStatsRow.REBp;
                metrics["PPR"] = playerStatsRow.PPR;
            }
            else
            {
                pl_stats[p.GP] = playerStatsRow.GP;
                pl_stats[p.GS] = playerStatsRow.GS;
                pl_stats[p.MINS] = playerStatsRow.MINS;
                pl_stats[p.PTS] = playerStatsRow.PTS;
                pl_stats[p.FGM] = playerStatsRow.FGM;
                pl_stats[p.FGA] = playerStatsRow.FGA;
                pl_stats[p.TPM] = playerStatsRow.TPM;
                pl_stats[p.TPA] = playerStatsRow.TPA;
                pl_stats[p.FTM] = playerStatsRow.FTM;
                pl_stats[p.FTA] = playerStatsRow.FTA;
                pl_stats[p.OREB] = playerStatsRow.OREB;
                pl_stats[p.DREB] = playerStatsRow.DREB;
                pl_stats[p.STL] = playerStatsRow.STL;
                pl_stats[p.TO] = playerStatsRow.TOS;
                pl_stats[p.BLK] = playerStatsRow.BLK;
                pl_stats[p.AST] = playerStatsRow.AST;
                pl_stats[p.FOUL] = playerStatsRow.FOUL;

                pl_metrics["GmSc"] = playerStatsRow.GmSc;
                pl_metrics["GmScE"] = playerStatsRow.GmScE;
                pl_metrics["EFF"] = playerStatsRow.EFF;
                pl_metrics["EFG%"] = playerStatsRow.EFGp;
                pl_metrics["TS%"] = playerStatsRow.TSp;
                pl_metrics["AST%"] = playerStatsRow.ASTp;
                pl_metrics["STL%"] = playerStatsRow.STLp;
                pl_metrics["TO%"] = playerStatsRow.TOp;
                pl_metrics["USG%"] = playerStatsRow.USGp;
                pl_metrics["PTSR"] = playerStatsRow.PTSR;
                pl_metrics["REBR"] = playerStatsRow.REBR;
                pl_metrics["OREBR"] = playerStatsRow.OREBR;
                pl_metrics["ASTR"] = playerStatsRow.ASTR;
                pl_metrics["BLKR"] = playerStatsRow.BLKR;
                pl_metrics["STLR"] = playerStatsRow.STLR;
                pl_metrics["TOR"] = playerStatsRow.TOR;
                pl_metrics["FTR"] = playerStatsRow.FTR;
                pl_metrics["PER"] = playerStatsRow.PER;
                pl_metrics["BLK%"] = playerStatsRow.BLKp;
                pl_metrics["DREB%"] = playerStatsRow.DREBp;
                pl_metrics["OREB%"] = playerStatsRow.OREBp;
                pl_metrics["REB%"] = playerStatsRow.REBp;
                pl_metrics["PPR"] = playerStatsRow.PPR;
            }

            ID = playerStatsRow.ID;
            Position1 = playerStatsRow.Position1;
            Position2 = playerStatsRow.Position2;
            TeamF = playerStatsRow.TeamF;
            TeamS = playerStatsRow.TeamS;
            Age = playerStatsRow.Age;
            YearsPro = playerStatsRow.YearsPro;
            isActive = playerStatsRow.isActive;
            isHidden = playerStatsRow.isHidden;
            isAllStar = playerStatsRow.isAllStar;
            isInjured = playerStatsRow.isInjured;
            isNBAChampion = playerStatsRow.isNBAChampion;

            CalcAvg();
        }

        /// <summary>
        /// Updates the playoff stats.
        /// </summary>
        /// <param name="dataRow">The data row containing the playoff stats.</param>
        public void UpdatePlayoffStats(DataRow dataRow)
        {
            pl_stats[p.GP] = Tools.getUInt16(dataRow, "GP");
            pl_stats[p.GS] = Tools.getUInt16(dataRow, "GS");
            pl_stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
            pl_stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");
            pl_stats[p.FGM] = Tools.getUInt16(dataRow, "FGM");
            pl_stats[p.FGA] = Tools.getUInt16(dataRow, "FGA");
            pl_stats[p.TPM] = Tools.getUInt16(dataRow, "TPM");
            pl_stats[p.TPA] = Tools.getUInt16(dataRow, "TPA");
            pl_stats[p.FTM] = Tools.getUInt16(dataRow, "FTM");
            pl_stats[p.FTA] = Tools.getUInt16(dataRow, "FTA");
            pl_stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
            pl_stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
            pl_stats[p.STL] = Tools.getUInt16(dataRow, "STL");
            pl_stats[p.TO] = Tools.getUInt16(dataRow, "TOS");
            pl_stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
            pl_stats[p.AST] = Tools.getUInt16(dataRow, "AST");
            pl_stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");

            CalcAvg(true);
        }

        /// <summary>
        /// Calculates the averages of a player's stats.
        /// </summary>
        /// <param name="playoffsOnly">if set to <c>true</c>, only the playoff averages will be calculated.</param>
        public void CalcAvg(bool playoffsOnly = false)
        {
            if (!playoffsOnly)
            {
                uint games = stats[p.GP];
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

            uint pl_games = pl_stats[p.GP];
            pl_averages[p.MPG] = (float) pl_stats[p.MINS]/pl_games;
            pl_averages[p.PPG] = (float) pl_stats[p.PTS]/pl_games;
            pl_averages[p.FGp] = (float) pl_stats[p.FGM]/pl_stats[p.FGA];
            pl_averages[p.FGeff] = pl_averages[p.FGp]*((float) pl_stats[p.FGM]/pl_games);
            pl_averages[p.TPp] = (float) pl_stats[p.TPM]/pl_stats[p.TPA];
            pl_averages[p.TPeff] = pl_averages[p.TPp]*((float) pl_stats[p.TPM]/pl_games);
            pl_averages[p.FTp] = (float) pl_stats[p.FTM]/pl_stats[p.FTA];
            pl_averages[p.FTeff] = pl_averages[p.FTp]*((float) pl_stats[p.FTM]/pl_games);
            pl_averages[p.RPG] = (float) (pl_stats[p.OREB] + pl_stats[p.DREB])/pl_games;
            pl_averages[p.ORPG] = (float) pl_stats[p.OREB]/pl_games;
            pl_averages[p.DRPG] = (float) pl_stats[p.DREB]/pl_games;
            pl_averages[p.SPG] = (float) pl_stats[p.STL]/pl_games;
            pl_averages[p.BPG] = (float) pl_stats[p.BLK]/pl_games;
            pl_averages[p.TPG] = (float) pl_stats[p.TO]/pl_games;
            pl_averages[p.APG] = (float) pl_stats[p.AST]/pl_games;
            pl_averages[p.FPG] = (float) pl_stats[p.FOUL]/pl_games;
        }

        /// <summary>
        /// Calculates the Metric Stats for this Player
        /// </summary>
        /// <param name="ts">The player's team's stats</param>
        /// <param name="tsopp">The player's team's opponents' stats</param>
        /// <param name="ls">The total league stats</param>
        /// <param name="leagueOv">Whether CalcMetrics is being called from the League Overview screen</param>
        public void CalcMetrics(TeamStats ts, TeamStats tsopp, TeamStats ls, bool leagueOv = false, bool GmScOnly = false,
                                bool playoffs = false)
        {
            var pstats = new double[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                if (!playoffs)
                    pstats[i] = stats[i];
                else
                    pstats[i] = pl_stats[i];
            }

            var tstats = new double[ts.stats.Length];
            for (int i = 0; i < ts.stats.Length; i++)
            {
                if (!playoffs)
                    tstats[i] = ts.stats[i];
                else
                    tstats[i] = ts.pl_stats[i];
            }

            var toppstats = new double[tsopp.stats.Length];
            for (int i = 0; i < tsopp.stats.Length; i++)
            {
                if (!playoffs)
                    toppstats[i] = tsopp.stats[i];
                else
                    toppstats[i] = tsopp.pl_stats[i];
            }

            var lstats = new double[ls.stats.Length];
            for (int i = 0; i < ls.stats.Length; i++)
            {
                if (!playoffs)
                    lstats[i] = ls.stats[i];
                else
                    lstats[i] = ls.pl_stats[i];
            }


            double pREB = pstats[p.OREB] + pstats[p.DREB];
            double tREB = tstats[t.OREB] + tstats[t.DREB];

            var temp_metrics = new Dictionary<string, double>();

            double GmSc = pstats[p.PTS] + 0.4*pstats[p.FGM] - 0.7*pstats[p.FGA] - 0.4*(pstats[p.FTA] - pstats[p.FTM]) + 0.7*pstats[p.OREB] +
                          0.3*pstats[p.DREB] + pstats[p.STL] + 0.7*pstats[p.AST] + 0.7*pstats[p.BLK] - 0.4*pstats[p.FOUL] - pstats[p.TO];
            temp_metrics.Add("GmSc", GmSc/pstats[p.GP]);

            double GmScE = 36*(1/pstats[p.MINS])*GmSc;
            temp_metrics.Add("GmScE", GmScE);

            if (!GmScOnly)
            {
                #region temp_metrics that do not require Opponent Stats

                double ASTp = 100*pstats[p.AST]/(((pstats[p.MINS]/(tstats[t.MINS]))*tstats[t.FGM]) - pstats[p.FGM]);
                temp_metrics.Add("AST%", ASTp);

                double EFGp = (pstats[p.FGM] + 0.5*pstats[p.TPM])/pstats[p.FGA];
                temp_metrics.Add("EFG%", EFGp);

                Dictionary<string, double> toppmetrics;
                if (!playoffs)
                    toppmetrics = tsopp.metrics;
                else
                    toppmetrics = tsopp.pl_metrics;

                double STLp = 100*(pstats[p.STL]*(tstats[t.MINS]))/(pstats[p.MINS]*toppmetrics["Poss"]);
                temp_metrics.Add("STL%", STLp);

                double TOp = 100*pstats[p.TO]/(pstats[p.FGA] + 0.44*pstats[p.FTA] + pstats[p.TO]);
                temp_metrics.Add("TO%", TOp);

                double TSp = pstats[p.PTS]/(2*(pstats[p.FGA] + 0.44*pstats[p.FTA]));
                temp_metrics.Add("TS%", TSp);

                double USGp = 100*((pstats[p.FGA] + 0.44*pstats[p.FTA] + pstats[p.TO])*(tstats[t.MINS]))/
                              (pstats[p.MINS]*(tstats[t.FGA] + 0.44*tstats[t.FTA] + tstats[t.TO]));
                temp_metrics.Add("USG%", USGp);

                // Rates, stat per 36 minutes played
                double PTSR = (pstats[p.PTS]/pstats[p.MINS])*36;
                temp_metrics.Add("PTSR", PTSR);

                double REBR = (pREB/pstats[p.MINS])*36;
                temp_metrics.Add("REBR", REBR);

                double OREBR = (pstats[p.OREB]/pstats[p.MINS])*36;
                temp_metrics.Add("OREBR", OREBR);

                double ASTR = (pstats[p.AST]/pstats[p.MINS])*36;
                temp_metrics.Add("ASTR", ASTR);

                double BLKR = (pstats[p.BLK]/pstats[p.MINS])*36;
                temp_metrics.Add("BLKR", BLKR);

                double STLR = (pstats[p.STL]/pstats[p.MINS])*36;
                temp_metrics.Add("STLR", STLR);

                double TOR = (pstats[p.TO]/pstats[p.MINS])*36;
                temp_metrics.Add("TOR", TOR);

                double FTR = (pstats[p.FTM]/pstats[p.FGA]);
                temp_metrics.Add("FTR", FTR);
                //
                // PER preparations
                double lREB = lstats[t.OREB] + lstats[t.DREB];
                double factor = (2/3) - (0.5*(lstats[t.AST]/lstats[t.FGM]))/(2*(lstats[t.FGM]/lstats[t.FTM]));
                double VOP = lstats[t.PF]/(lstats[t.FGA] - lstats[t.OREB] + lstats[t.TO] + 0.44*lstats[t.FTA]);
                double lDRBp = lstats[t.DREB]/lREB;

                double uPER = (1/pstats[p.MINS])*
                              (pstats[p.TPM] + (2/3)*pstats[p.AST] + (2 - factor*(tstats[t.AST]/tstats[t.FGM]))*pstats[p.FGM] +
                               (pstats[p.FTM]*0.5*(1 + (1 - (tstats[t.AST]/tstats[t.FGM])) + (2/3)*(tstats[t.AST]/tstats[t.FGM]))) -
                               VOP*pstats[p.TO] - VOP*lDRBp*(pstats[p.FGA] - pstats[p.FGM]) -
                               VOP*0.44*(0.44 + (0.56*lDRBp))*(pstats[p.FTA] - pstats[p.FTM]) + VOP*(1 - lDRBp)*(pREB - pstats[p.OREB]) +
                               VOP*lDRBp*pstats[p.OREB] + VOP*pstats[p.STL] + VOP*lDRBp*pstats[p.BLK] -
                               pstats[p.FOUL]*((lstats[t.FTM]/lstats[t.FOUL]) - 0.44*(lstats[t.FTA]/lstats[t.FOUL])*VOP));
                temp_metrics.Add("EFF", uPER*100);

                #endregion

                #region temp_metrics that require Opponents stats

                if (ts.getGames() == tsopp.getGames())
                {
                    double BLKp = 100*(pstats[p.BLK]*(tstats[t.MINS]))/(pstats[p.MINS]*(toppstats[t.FGA] - toppstats[t.TPA]));

                    double DRBp = 100*(pstats[p.DREB]*(tstats[t.MINS]))/(pstats[p.MINS]*(tstats[t.DREB] + toppstats[t.OREB]));

                    double ORBp = 100*(pstats[p.OREB]*(tstats[t.MINS]))/(pstats[p.MINS]*(tstats[t.OREB] + toppstats[t.DREB]));

                    double toppREB = toppstats[t.OREB] + toppstats[t.DREB];

                    double REBp = 100*(pREB*(tstats[t.MINS]))/(pstats[p.MINS]*(tREB + toppREB));

                    #region temp_metrics that require league stats

                    double aPER;
                    double PPR;

                    if (ls.name != "$$Empty")
                    {
                        //double paceAdj = ls.temp_metrics["Pace"]/ts.temp_metrics["Pace"];
                        double estPaceAdj;
                        if (!playoffs)
                            estPaceAdj = 2*ls.averages[t.PPG]/(ts.averages[t.PPG] + tsopp.averages[t.PPG]);
                        else
                            estPaceAdj = 2*ls.pl_averages[t.PPG]/(ts.pl_averages[t.PPG] + tsopp.pl_averages[t.PPG]);

                        aPER = estPaceAdj*uPER;

                        PPR = 100*estPaceAdj*(((pstats[p.AST]*2/3) - pstats[p.TO])/pstats[p.MINS]);
                    }
                    else
                    {
                        aPER = Double.NaN;
                        PPR = Double.NaN;
                    }

                    #endregion

                    temp_metrics.Add("aPER", aPER);
                    temp_metrics.Add("BLK%", BLKp);
                    temp_metrics.Add("DREB%", DRBp);
                    temp_metrics.Add("OREB%", ORBp);
                    temp_metrics.Add("REB%", REBp);
                    temp_metrics.Add("PPR", PPR);
                }
                else
                {
                    temp_metrics.Add("aPER", Double.NaN);
                    temp_metrics.Add("BLK%", Double.NaN);
                    temp_metrics.Add("DREB%", Double.NaN);
                    temp_metrics.Add("OREB%", Double.NaN);
                    temp_metrics.Add("REB%", Double.NaN);
                    temp_metrics.Add("PPR", Double.NaN);
                }

                #endregion
            }

            uint games = (!playoffs) ? ts.getGames() : ts.getPlayoffGames();

            var gamesRequired = (int) Math.Ceiling(0.8522*games);
            if (leagueOv)
            {
                if (pstats[p.GP] < gamesRequired)
                {
                    foreach (string name in temp_metrics.Keys.ToList())
                        temp_metrics[name] = Double.NaN;
                }
            }

            if (!playoffs)
                metrics = new Dictionary<string, double>(temp_metrics);
            else
                pl_metrics = new Dictionary<string, double>(temp_metrics);
        }

        /// <summary>
        /// Calculates the PER.
        /// </summary>
        /// <param name="lg_aPER">The league average PER.</param>
        /// <param name="playoffs">if set to <c>true</c>, the PER is calculated for the player's playoff stats.</param>
        public void CalcPER(double lg_aPER, bool playoffs = false)
        {
            try
            {
                if (!playoffs)
                    metrics.Add("PER", metrics["aPER"]*(15/lg_aPER));
                else
                    pl_metrics.Add("PER", pl_metrics["aPER"]*(15/lg_aPER));
            }
            catch (Exception)
            {
                if (!playoffs)
                    metrics.Add("PER", double.NaN);
                else
                    pl_metrics.Add("PER", double.NaN);
            }
        }

        /// <summary>
        /// Adds a player's box score to their stats.
        /// </summary>
        /// <param name="pbs">The Player Box Score.</param>
        /// <param name="isPlayoff">if set to <c>true</c>, the stats are added to the playoff stats.</param>
        /// <exception cref="System.Exception">Occurs when the player IDs from the stats and box score do not match.</exception>
        public void AddBoxScore(PlayerBoxScore pbs, bool isPlayoff = false)
        {
            if (ID != pbs.PlayerID)
                throw new Exception("Tried to update PlayerStats " + ID + " with PlayerBoxScore " + pbs.PlayerID);

            if (!isPlayoff)
            {
                if (pbs.isStarter)
                    stats[p.GS]++;
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
            }
            else
            {
                if (pbs.isStarter)
                    pl_stats[p.GS]++;
                if (pbs.MINS > 0)
                {
                    pl_stats[p.GP]++;
                    pl_stats[p.MINS] += pbs.MINS;
                }
                pl_stats[p.PTS] += pbs.PTS;
                pl_stats[p.FGM] += pbs.FGM;
                pl_stats[p.FGA] += pbs.FGA;
                pl_stats[p.TPM] += pbs.TPM;
                pl_stats[p.TPA] += pbs.TPA;
                pl_stats[p.FTM] += pbs.FTM;
                pl_stats[p.FTA] += pbs.FTA;
                pl_stats[p.OREB] += pbs.OREB;
                pl_stats[p.DREB] += pbs.DREB;
                pl_stats[p.STL] += pbs.STL;
                pl_stats[p.TO] += pbs.TOS;
                pl_stats[p.BLK] += pbs.BLK;
                pl_stats[p.AST] += pbs.AST;
                pl_stats[p.FOUL] += pbs.FOUL;
            }

            CalcAvg();
        }

        /// <summary>
        /// Adds the player stats from a PlayerStats instance to the current stats.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="addBothToSeasonStats">if set to <c>true</c>, both season and playoff stats will be added to the season stats.</param>
        public void AddPlayerStats(PlayerStats ps, bool addBothToSeasonStats = false)
        {
            if (!addBothToSeasonStats)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    stats[i] += ps.stats[i];
                }

                for (int i = 0; i < pl_stats.Length; i++)
                {
                    pl_stats[i] += ps.pl_stats[i];
                }
            }
            else
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    stats[i] += ps.stats[i];
                }

                for (int i = 0; i < pl_stats.Length; i++)
                {
                    stats[i] += ps.pl_stats[i];
                }
            }

            CalcAvg();
        }

        /// <summary>
        /// Resets the stats.
        /// </summary>
        public void ResetStats()
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = 0;
            }

            for (int i = 0; i < pl_stats.Length; i++)
            {
                pl_stats[i] = 0;
            }

            CalcAvg();
        }

        /// <summary>
        /// Calculates the league averages.
        /// </summary>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="teamStats">The team stats.</param>
        /// <returns></returns>
        public static PlayerStats CalculateLeagueAverages(Dictionary<int, PlayerStats> playerStats, Dictionary<int, TeamStats> teamStats)
        {
            var lps = new PlayerStats(new Player(-1, "", "League", "Averages", " ", " "));
            foreach (int key in playerStats.Keys)
            {
                lps.AddPlayerStats(playerStats[key]);
            }

            var ls = new TeamStats("League");
            for (int i = 0; i < teamStats.Count; i++)
            {
                ls.AddTeamStats(teamStats[i], Span.Season);
                ls.AddTeamStats(teamStats[i], Span.Playoffs);
            }
            ls.CalcMetrics(ls);
            ls.CalcMetrics(ls, true);
            lps.CalcMetrics(ls, ls, ls, true);
            lps.CalcMetrics(ls, ls, ls, true, playoffs: true);

            var playerCount = (uint) playerStats.Count;
            for (int i = 0; i < lps.stats.Length; i++)
            {
                lps.stats[i] /= playerCount;
                lps.pl_stats[i] /= playerCount;
            }
            //ps.CalcAvg();
            return lps;
        }

        /// <summary>
        /// Calculates all metrics.
        /// </summary>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="teamStats">The team stats.</param>
        /// <param name="oppStats">The opposing team stats.</param>
        /// <param name="TeamOrder">The team order.</param>
        /// <param name="leagueOv">set to <c>true</c> if calling from the LeagueOverview window.</param>
        /// <param name="playoffs">if set to <c>true</c>, the metrics will be calculated for the playoff stats.</param>
        public static void CalculateAllMetrics(ref Dictionary<int, PlayerStats> playerStats, Dictionary<int, TeamStats> teamStats,
                                               Dictionary<int, TeamStats> oppStats, SortedDictionary<string, int> TeamOrder,
                                               bool leagueOv = false, bool playoffs = false)
        {
            int tCount = teamStats.Count;

            var ls = new TeamStats();
            for (int i = 0; i < tCount; i++)
            {
                if (!playoffs)
                {
                    ls.AddTeamStats(teamStats[i], Span.Season);
                    teamStats[i].CalcMetrics(oppStats[i]);
                }
                else
                {
                    ls.AddTeamStats(teamStats[i], Span.Playoffs);
                    teamStats[i].CalcMetrics(oppStats[i], true);
                }
            }
            ls.CalcMetrics(ls, playoffs);

            double lg_aPER = 0;
            double pl_lg_aPER = 0;
            double totalMins = 0;
            double pl_totalMins = 0;

            foreach (int playerid in playerStats.Keys.ToList())
            {
                if (String.IsNullOrEmpty(playerStats[playerid].TeamF))
                    continue;

                int teamid = TeamOrder[playerStats[playerid].TeamF];
                TeamStats ts = teamStats[teamid];
                TeamStats tsopp = oppStats[teamid];

                playerStats[playerid].CalcMetrics(ts, tsopp, ls, leagueOv, playoffs: playoffs);
                if (!playoffs)
                {
                    if (!(Double.IsNaN(playerStats[playerid].metrics["aPER"])))
                    {
                        lg_aPER += playerStats[playerid].metrics["aPER"]*playerStats[playerid].stats[p.MINS];
                        totalMins += playerStats[playerid].stats[p.MINS];
                    }
                }
                else
                {
                    if (!(Double.IsNaN(playerStats[playerid].pl_metrics["aPER"])))
                    {
                        pl_lg_aPER += playerStats[playerid].pl_metrics["aPER"]*playerStats[playerid].pl_stats[p.MINS];
                        pl_totalMins += playerStats[playerid].pl_stats[p.MINS];
                    }
                }
            }
            if (!playoffs)
                lg_aPER /= totalMins;
            else
                pl_lg_aPER /= pl_totalMins;

            foreach (int playerid in playerStats.Keys.ToList())
            {
                if (String.IsNullOrEmpty(playerStats[playerid].TeamF))
                    continue;

                if (!playoffs)
                    playerStats[playerid].CalcPER(lg_aPER);
                else
                    playerStats[playerid].CalcPER(pl_lg_aPER, true);
            }
        }
    }

    /// <summary>
    /// Contains all the information of a player's performance in a game.
    /// </summary>
    [Serializable]
    public class PlayerBoxScore : INotifyPropertyChanged
    {
        private UInt16 _FGA;
        private UInt16 _FGM;
        private UInt16 _FTA;
        private UInt16 _FTM;
        private UInt16 _TPA;
        protected UInt16 _TPM;
        //public ObservableCollection<KeyValuePair<int, string>> PlayersList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        public PlayerBoxScore()
        {
            PlayerID = -1;
            Team = "";
            isStarter = false;
            playedInjured = false;
            isOut = false;
            ResetStats();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="r">The DataRow containing the player's box score.</param>
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
                RealDate = Convert.ToDateTime(Date);

                CalcMetrics(GameID, r);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="brRow">The Basketball-Reference.com row containing the player's box score.</param>
        /// <param name="team">The team.</param>
        /// <param name="gameID">The game ID.</param>
        /// <param name="starter">if set to <c>true</c>, the player is a starter.</param>
        /// <param name="playerStats">The player stats.</param>
        public PlayerBoxScore(DataRow brRow, string team, int gameID, bool starter, Dictionary<int, PlayerStats> playerStats)
        {
            string[] nameParts = brRow[0].ToString().Split(new[] {' '}, 2);
            try
            {
                PlayerID = playerStats.Single(delegate(KeyValuePair<int, PlayerStats> kvp)
                                              {
                                                  if (kvp.Value.LastName == nameParts[1] && kvp.Value.FirstName == nameParts[0] &&
                                                      kvp.Value.TeamF == team)
                                                      return true;
                                                  return false;
                                              }).Value.ID;
            }
            catch (Exception)
            {
                try
                {
                    PlayerID = playerStats.Single(delegate(KeyValuePair<int, PlayerStats> kvp)
                                                  {
                                                      if (kvp.Value.LastName == nameParts[1] && kvp.Value.FirstName == nameParts[0])
                                                          return true;
                                                      return false;
                                                  }).Value.ID;
                }
                catch (Exception)
                {
                    //MessageBox.Show("No player with the name " + nameParts[0] + " " + nameParts[1] + " was found in the database.");
                    PlayerID = -1;
                    return;
                }
            }

            GameID = gameID;
            Team = team;
            isStarter = starter;
            playedInjured = false;
            isOut = false;
            PTS = Tools.getUInt16(brRow, "PTS");
            REB = Convert.ToUInt16(brRow["TRB"].ToString());
            AST = Convert.ToUInt16(brRow["AST"].ToString());
            STL = Convert.ToUInt16(brRow["STL"].ToString());
            BLK = Convert.ToUInt16(brRow["BLK"].ToString());
            TOS = Convert.ToUInt16(brRow["TOV"].ToString());
            FGM = Convert.ToUInt16(brRow["FG"].ToString());
            FGA = Convert.ToUInt16(brRow["FGA"].ToString());
            TPM = Convert.ToUInt16(brRow["3P"].ToString());
            TPA = Convert.ToUInt16(brRow["3PA"].ToString());
            FTM = Convert.ToUInt16(brRow["FT"].ToString());
            FTA = Convert.ToUInt16(brRow["FTA"].ToString());
            OREB = Convert.ToUInt16(brRow["ORB"].ToString());
            FOUL = Convert.ToUInt16(brRow["PF"].ToString());
            MINS = Convert.ToUInt16(brRow["MP"].ToString().Split(':')[0]);
            if (Convert.ToUInt16(brRow["MP"].ToString().Split(':')[1]) >= 30)
                MINS++;
            DREB = (UInt16) (REB - OREB);
            FGp = (float) FGM/FGA;
            TPp = (float) TPM/TPA;
            FTp = (float) FTM/FTA;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="dict">The dictionary containing the player box score.</param>
        /// <param name="playerID">The player ID.</param>
        /// <param name="team">The team.</param>
        public PlayerBoxScore(Dictionary<string, string> dict, int playerID, string team)
        {
            PlayerID = playerID;
            Team = team;
            isStarter = isStarter.TrySetValue(dict, "Starter", typeof (bool));
            playedInjured = playedInjured.TrySetValue(dict, "Injured", typeof (bool));
            isOut = isOut.TrySetValue(dict, "Out", typeof (bool));
            MINS = MINS.TrySetValue(dict, "MINS", typeof (UInt16));
            PTS = PTS.TrySetValue(dict, "PTS", typeof (UInt16));
            REB = REB.TrySetValue(dict, "REB", typeof (UInt16));
            AST = AST.TrySetValue(dict, "AST", typeof (UInt16));
            STL = STL.TrySetValue(dict, "STL", typeof (UInt16));
            BLK = BLK.TrySetValue(dict, "BLK", typeof (UInt16));
            TOS = TOS.TrySetValue(dict, "TO", typeof (UInt16));
            FGM = FGM.TrySetValue(dict, "FGM", typeof (UInt16));
            FGA = FGA.TrySetValue(dict, "FGA", typeof (UInt16));
            TPM = TPM.TrySetValue(dict, "3PM", typeof (UInt16));
            TPA = TPA.TrySetValue(dict, "3PA", typeof (UInt16));
            FTM = FTM.TrySetValue(dict, "FTM", typeof (UInt16));
            FTA = FTA.TrySetValue(dict, "FTA", typeof (UInt16));
            OREB = OREB.TrySetValue(dict, "OREB", typeof (UInt16));
            FOUL = FOUL.TrySetValue(dict, "FOUL", typeof (UInt16));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class. 
        /// Used to cast a LivePlayerBoxScore to a PlayerBoxScore which can be saved to the database.
        /// </summary>
        /// <param name="lpbs">The LivePlayerBoxScore instance containing the player's box score.</param>
        public PlayerBoxScore(LivePlayerBoxScore lpbs)
        {
            PlayerID = lpbs.PlayerID;
            Name = lpbs.Name;
            Team = lpbs.Team;
            TeamPTS = lpbs.TeamPTS;
            OppTeam = lpbs.OppTeam;
            OppTeamPTS = lpbs.OppTeamPTS;
            isStarter = lpbs.isStarter;
            playedInjured = lpbs.playedInjured;
            isOut = lpbs.isOut;
            GmSc = lpbs.GmSc;
            GmScE = lpbs.GmScE;
            MINS = lpbs.MINS;
            PTS = lpbs.PTS;
            FGM = lpbs.FGM;
            FGA = lpbs.FGA;
            FGp = lpbs.FGp;
            TPM = lpbs.TPM;
            TPA = lpbs.TPA;
            TPp = lpbs.TPp;
            FTM = lpbs.FTM;
            FTA = lpbs.FTA;
            FTp = lpbs.FTp;
            REB = lpbs.REB;
            OREB = lpbs.OREB;
            DREB = lpbs.DREB;
            STL = lpbs.STL;
            TOS = lpbs.TOS;
            BLK = lpbs.BLK;
            AST = lpbs.AST;
            FOUL = lpbs.FOUL;

            Result = lpbs.Result;
            Date = lpbs.Date;
            GameID = lpbs.GameID;
        }

        public DateTime RealDate { get; set; }

        public int PlayerID { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public int TeamPTS { get; set; }
        public string OppTeam { get; set; }
        public int OppTeamPTS { get; set; }
        public bool isStarter { get; set; }
        public bool playedInjured { get; set; }
        public bool isOut { get; set; }
        public double GmSc { get; set; }
        public double GmScE { get; set; }
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Calculates the metrics of a player's performance.
        /// </summary>
        /// <param name="gameID">The game ID.</param>
        /// <param name="r">The SQLite DataRow containing the player's box score. Should be the result of an INNER JOIN'ed query between PlayerResults and GameResults.</param>
        private void CalcMetrics(int gameID, DataRow r)
        {
            var bs = new TeamBoxScore(r);

            var ts = new TeamStats(Team);
            var tsopp = new TeamStats(OppTeam);

            string Team1 = Tools.getString(r, "T1Name");
            string Team2 = Tools.getString(r, "T2Name");

            if (Team == Team1)
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            else
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);

            var ps = new PlayerStats();
            ps.ID = PlayerID;
            ps.AddBoxScore(this, bs.isPlayoff);
            ps.CalcMetrics(ts, tsopp, new TeamStats("$$Empty"), GmScOnly: true);

            GmSc = ps.metrics["GmSc"];
            GmScE = ps.metrics["GmScE"];
        }

        /// <summary>
        /// Calculates the points scored.
        /// </summary>
        protected void CalculatePoints()
        {
            PTS = (ushort) ((_FGM - _TPM)*2 + _TPM*3 + _FTM);
        }

        /// <summary>
        /// Gets the best stats of a player's performance.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <param name="position">The player's primary position.</param>
        /// <returns></returns>
        public string GetBestStats(int count, string position)
        {
            double fgn = 0, tpn = 0, ftn = 0, ftrn = 0;
            var statsn = new Dictionary<string, double>();

            double fgfactor, tpfactor, ftfactor, orebfactor, rebfactor, astfactor, stlfactor, blkfactor, ptsfactor, ftrfactor;

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
                ftrn = ((double) FTM/FGA)/ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            IOrderedEnumerable<string> items = from k in statsn.Keys orderby statsn[k] descending select k;

            string s = "";
            s += String.Format("PTS: {0}\n", PTS);
            int i = 1;
            foreach (string item in items)
            {
                if (i == count)
                    break;

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
                        continue;

                    case "ftrn":
                        s += String.Format("FTM/FGA: {0}-{1} ({2:F3})\n", FTM, FGA, (double) FTM/FGA);
                        break;
                }

                i++;
            }
            return s;
        }

        /// <summary>
        /// Resets the stats.
        /// </summary>
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

        protected void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    /// <summary>
    /// Used to keep live track of a player's performance. Extends PlayerBoxScore.
    /// </summary>
    [Serializable]
    public class LivePlayerBoxScore : PlayerBoxScore
    {
        private ushort _OREB;
        private ushort _TwoPM;

        public UInt16 TwoPM
        {
            get { return _TwoPM; }
            set
            {
                _TwoPM = value;
                FGM = (ushort) (TPM + _TwoPM);
                CalculatePoints();
                NotifyPropertyChanged("FGM");
                NotifyPropertyChanged("PTS");
            }
        }

        public new UInt16 TPM
        {
            get { return _TPM; }
            set
            {
                _TPM = value;
                FGM = (ushort) (TPM + _TwoPM);
                CalculatePoints();
                NotifyPropertyChanged("FGM");
                NotifyPropertyChanged("PTS");
            }
        }

        public new UInt16 OREB
        {
            get { return _OREB; }
            set
            {
                if (_OREB < value)
                    REB++;
                else if (_OREB > value)
                    REB--;
                _OREB = value;
                NotifyPropertyChanged("REB");
            }
        }
    }

    /// <summary>
    /// Used to determine the player ranking for each stat.
    /// </summary>
    public class PlayerRankings
    {
        public int avgcount = (new PlayerStats(new Player(-1, "", "", "", "", ""))).averages.Length;

        public Dictionary<int, int[]> list = new Dictionary<int, int[]>();
        public Dictionary<int, int[]> rankings = new Dictionary<int, int[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerRankings" /> class, and calculates the rankings.
        /// </summary>
        /// <param name="pst">The PlayerStats dictionary, containing all player information.</param>
        /// <param name="playoffs">if set to <c>true</c>, the rankings will take only playoff performances into account.</param>
        public PlayerRankings(Dictionary<int, PlayerStats> pst, bool playoffs = false)
        {
            foreach (var kvp in pst)
            {
                rankings.Add(kvp.Key, new int[avgcount]);
            }
            for (int j = 0; j < avgcount; j++)
            {
                Dictionary<int, float> averages;
                if (!playoffs)
                    averages = pst.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.averages[j]);
                else
                    averages = pst.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pl_averages[j]);

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

    /// <summary>
    /// Contains the basic information required to initially create a player.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        public Player()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="Team">The team.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="Position1">The primary position.</param>
        /// <param name="Position2">The secondary position.</param>
        public Player(int ID, string Team, string LastName, string FirstName, string Position1, string Position2)
        {
            this.ID = ID;
            this.Team = Team;
            this.LastName = LastName;
            this.FirstName = FirstName;
            Position = Position1;
            this.Position2 = Position2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Player" /> class.
        /// </summary>
        /// <param name="dataRow">The data row containing the player information.</param>
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

    /// <summary>
    /// Implements an easily bindable interface to a player's stats.
    /// </summary>
    public class PlayerStatsRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="playoffs">if set to <c>true</c>, the interface provided will show playoff stats.</param>
        public PlayerStatsRow(PlayerStats ps, bool playoffs = false)
        {
            LastName = ps.LastName;
            FirstName = ps.FirstName;

            ID = ps.ID;
            Position1 = ps.Position1;
            Position2 = ps.Position2;
            TeamF = ps.TeamF;
            TeamS = ps.TeamS;
            isActive = ps.isActive;
            isHidden = ps.isHidden;
            isAllStar = ps.isAllStar;
            isInjured = ps.isInjured;
            isNBAChampion = ps.isNBAChampion;
            Age = ps.Age;
            YearsPro = ps.YearsPro;

            if (!playoffs)
            {
                GP = ps.stats[p.GP];
                GS = ps.stats[p.GS];
                MINS = ps.stats[p.MINS];
                PTS = ps.stats[p.PTS];
                FGM = ps.stats[p.FGM];
                FGMPG = ((float) FGM/GP);
                FGA = ps.stats[p.FGA];
                FGAPG = ((float) FGA/GP);
                TPM = ps.stats[p.TPM];
                TPMPG = ((float) TPM/GP);
                TPA = ps.stats[p.TPA];
                TPAPG = (uint) ((double) TPA/GP);
                FTM = ps.stats[p.FTM];
                FTMPG = ((float) FTM/GP);
                FTA = ps.stats[p.FTA];
                FTAPG = ((float) FTA/GP);
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

                try
                {
                    GmSc = ps.metrics["GmSc"];
                    GmScE = ps.metrics["GmScE"];
                    EFF = ps.metrics["EFF"];
                    EFGp = ps.metrics["EFG%"];
                    TSp = ps.metrics["TS%"];
                    ASTp = ps.metrics["AST%"];
                    STLp = ps.metrics["STL%"];
                    TOp = ps.metrics["TO%"];
                    USGp = ps.metrics["USG%"];
                    PTSR = ps.metrics["PTSR"];
                    REBR = ps.metrics["REBR"];
                    OREBR = ps.metrics["OREBR"];
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
                catch (KeyNotFoundException)
                {
                }
            }
            else
            {
                GP = ps.pl_stats[p.GP];
                GS = ps.pl_stats[p.GS];
                MINS = ps.pl_stats[p.MINS];
                PTS = ps.pl_stats[p.PTS];
                FGM = ps.pl_stats[p.FGM];
                FGMPG = ((float) FGM/GP);
                FGA = ps.pl_stats[p.FGA];
                FGAPG = ((float) FGA/GP);
                TPM = ps.pl_stats[p.TPM];
                TPMPG = ((float) TPM/GP);
                TPA = ps.pl_stats[p.TPA];
                TPAPG = (uint) ((double) TPA/GP);
                FTM = ps.pl_stats[p.FTM];
                FTMPG = ((float) FTM/GP);
                FTA = ps.pl_stats[p.FTA];
                FTAPG = ((float) FTA/GP);
                OREB = ps.pl_stats[p.OREB];
                DREB = ps.pl_stats[p.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ps.pl_stats[p.STL];
                TOS = ps.pl_stats[p.TO];
                BLK = ps.pl_stats[p.BLK];
                AST = ps.pl_stats[p.AST];
                FOUL = ps.pl_stats[p.FOUL];

                MPG = ps.pl_averages[p.MPG];
                PPG = ps.pl_averages[p.PPG];
                FGp = ps.pl_averages[p.FGp];
                FGeff = ps.pl_averages[p.FGeff];
                TPp = ps.pl_averages[p.TPp];
                TPeff = ps.pl_averages[p.TPeff];
                FTp = ps.pl_averages[p.FTp];
                FTeff = ps.pl_averages[p.FTeff];
                RPG = ps.pl_averages[p.RPG];
                ORPG = ps.pl_averages[p.ORPG];
                DRPG = ps.pl_averages[p.DRPG];
                SPG = ps.pl_averages[p.SPG];
                TPG = ps.pl_averages[p.TPG];
                BPG = ps.pl_averages[p.BPG];
                APG = ps.pl_averages[p.APG];
                FPG = ps.pl_averages[p.FPG];

                try
                {
                    GmSc = ps.pl_metrics["GmSc"];
                    GmScE = ps.pl_metrics["GmScE"];
                    EFF = ps.pl_metrics["EFF"];
                    EFGp = ps.pl_metrics["EFG%"];
                    TSp = ps.pl_metrics["TS%"];
                    ASTp = ps.pl_metrics["AST%"];
                    STLp = ps.pl_metrics["STL%"];
                    TOp = ps.pl_metrics["TO%"];
                    USGp = ps.pl_metrics["USG%"];
                    PTSR = ps.pl_metrics["PTSR"];
                    REBR = ps.pl_metrics["REBR"];
                    OREBR = ps.pl_metrics["OREBR"];
                    ASTR = ps.pl_metrics["ASTR"];
                    BLKR = ps.pl_metrics["BLKR"];
                    STLR = ps.pl_metrics["STLR"];
                    TOR = ps.pl_metrics["TOR"];
                    FTR = ps.pl_metrics["FTR"];

                    try
                    {
                        PER = ps.pl_metrics["PER"];
                    }
                    catch (Exception)
                    {
                        PER = double.NaN;
                    }

                    BLKp = ps.pl_metrics["BLK%"];
                    DREBp = ps.pl_metrics["DREB%"];
                    OREBp = ps.pl_metrics["OREB%"];
                    REBp = ps.pl_metrics["REB%"];
                    PPR = ps.pl_metrics["PPR"];
                }
                catch (KeyNotFoundException)
                {
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="type">The type.</param>
        /// <param name="playoffs">if set to <c>true</c>, the interface provided will show playoff stats.</param>
        public PlayerStatsRow(PlayerStats ps, string type, bool playoffs = false) : this(ps, playoffs)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="type">The type.</param>
        /// <param name="group">The group.</param>
        /// <param name="playoffs">if set to <c>true</c>, the interface provided will show playoff stats.</param>
        public PlayerStatsRow(PlayerStats ps, string type, string group, bool playoffs = false) : this(ps, type, playoffs)
        {
            Type = type;
            Group = group;
        }

        public uint GP { get; set; }
        public uint GS { get; set; }

        public uint MINS { get; set; }
        public uint PTS { get; set; }
        public uint FGM { get; set; }
        public uint FGA { get; set; }
        public uint TPM { get; set; }
        public uint TPA { get; set; }
        public uint FTM { get; set; }
        public uint FTA { get; set; }
        public uint REB { get; set; }
        public uint OREB { get; set; }
        public uint DREB { get; set; }
        public uint STL { get; set; }
        public uint TOS { get; set; }
        public uint BLK { get; set; }
        public uint AST { get; set; }
        public uint FOUL { get; set; }

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

        public float FGMPG { get; set; }
        public float FGAPG { get; set; }
        public float TPMPG { get; set; }
        public float TPAPG { get; set; }
        public float FTMPG { get; set; }
        public float FTAPG { get; set; }

        public double EFF { get; set; }
        public double GmSc { get; set; }
        public double GmScE { get; set; }
        public double EFGp { get; set; }
        public double TSp { get; set; }
        public double ASTp { get; set; }
        public double STLp { get; set; }
        public double TOp { get; set; }
        public double USGp { get; set; }
        public double PTSR { get; set; }
        public double REBR { get; set; }
        public double OREBR { get; set; }
        public double ASTR { get; set; }
        public double BLKR { get; set; }
        public double STLR { get; set; }
        public double TOR { get; set; }
        public double FTR { get; set; }

        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Position1 { get; set; }
        public string Position2 { get; set; }
        public string TeamF { get; set; }
        public string TeamFDisplay { get; set; }
        public string TeamS { get; set; }
        public bool isActive { get; set; }
        public bool isHidden { get; set; }
        public bool isAllStar { get; set; }
        public bool isInjured { get; set; }
        public bool isNBAChampion { get; set; }

        public int Age { get; set; }
        public int YearsPro { get; set; }

        public string Type { get; set; }
        public string Group { get; set; }

        #region Metrics that require opponents' stats

        public double PER { get; set; }
        public double BLKp { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double REBp { get; set; }
        public double PPR { get; set; }

        #endregion

        /// <summary>
        /// Gets the best stats.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <returns>A well-formatted multi-line string presenting the best stats.</returns>
        public string GetBestStats(int count)
        {
            string position = Position1;
            double fgn = 0, tpn = 0, ftn = 0, ftrn = 0;
            var statsn = new Dictionary<string, double>();

            double fgfactor, tpfactor, ftfactor, orebfactor, rebfactor, astfactor, stlfactor, blkfactor, ptsfactor, ftrfactor;

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

            if (FGM/GP > 4)
            {
                fgn = FGp/fgfactor;
            }
            statsn.Add("fgn", fgn);

            if (TPM/GP > 2)
            {
                tpn = TPp/tpfactor;
            }
            statsn.Add("tpn", tpn);

            if (FTM/GP > 4)
            {
                ftn = FTp/ftfactor;
            }
            statsn.Add("ftn", ftn);

            double orebn = ORPG/orebfactor;
            statsn.Add("orebn", orebn);

            /*
            drebn = (REB-OREB)/6.348;
            statsn.Add("drebn", drebn);
            */

            double rebn = RPG/rebfactor;
            statsn.Add("rebn", rebn);

            double astn = APG/astfactor;
            statsn.Add("astn", astn);

            double stln = SPG/stlfactor;
            statsn.Add("stln", stln);

            double blkn = BPG/blkfactor;
            statsn.Add("blkn", blkn);

            double ptsn = PPG/ptsfactor;
            statsn.Add("ptsn", ptsn);

            if (FTM/GP > 3)
            {
                ftrn = ((double) FTM/FGA)/ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            IOrderedEnumerable<string> items = from k in statsn.Keys orderby statsn[k] descending select k;

            string s = "";
            int i = 1;
            s += String.Format("PPG: {0:F1}\n", PPG);
            foreach (string item in items)
            {
                if (i == count)
                    break;

                switch (item)
                {
                    case "fgn":
                        s += String.Format("FG: {0:F1}-{1:F1} ({2:F3})\n", (double) FGM/GP, (double) FGA/GP, FGp);
                        break;

                    case "tpn":
                        s += String.Format("3P: {0:F1}-{1:F1} ({2:F3})\n", (double) TPM/GP, (double) TPA/GP, TPp);
                        break;

                    case "ftn":
                        s += String.Format("FT: {0:F1}-{1:F1} ({2:F3})\n", (double) FTM/GP, (double) FTA/GP, FTp);
                        break;

                    case "orebn":
                        s += String.Format("ORPG: {0:F1}\n", ORPG);
                        break;

                        /*
                case "drebn":
                    s += String.Format("DREB: {0}\n", REB - OREB);
                    break;
                */

                    case "rebn":
                        s += String.Format("RPG: {0:F1}\n", RPG);
                        break;

                    case "astn":
                        s += String.Format("APG: {0:F1}\n", APG);
                        break;

                    case "stln":
                        s += String.Format("SPG: {0:F1}\n", SPG);
                        break;

                    case "blkn":
                        s += String.Format("BPG: {0:F1}\n", BPG);
                        break;

                    case "ptsn":
                        continue;

                    case "ftrn":
                        s += String.Format("FTM/FGA: {0:F1}-{1:F1} ({2:F3})\n", (double) FTM/GP, (double) FGA/GP, (double) FTM/FGA);
                        break;
                }

                i++;
            }
            return s;
        }

        /// <summary>
        /// Gets a list (dictionary) of the best stats.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <returns>A list (dictionary) of the best stats' names and values</returns>
        public Dictionary<string, string> GetBestStatsList(int count)
        {
            var statList = new Dictionary<string, string>();
            string s = GetBestStats(count);
            string[] lines = s.Split('\n');
            for (int i = 1;i<count;i++)
            {
                string[] parts = lines[i].Split(new []{": "}, StringSplitOptions.None);
                statList.Add(parts[0], parts[1]);
            }
            return statList;
        }

        /// <summary>
        /// Shows a scouting report for the player in natural language.
        /// </summary>
        /// <param name="pst">The PlayerStats dictionary containing all the player information.</param>
        /// <param name="rankingsActive">The rankings of currently active players.</param>
        /// <param name="rankingsTeam">The rankings of the players in the same team.</param>
        /// <param name="rankingsPosition">The rankings of the players in the same position.</param>
        /// <param name="pbsList">The list of the player's available box scores.</param>
        /// <param name="bestGame">The well-formatted string from the player's best game.</param>
        public void ScoutingReport(Dictionary<int, PlayerStats> pst, PlayerRankings rankingsActive, PlayerRankings rankingsTeam, PlayerRankings rankingsPosition,
            List<PlayerBoxScore> pbsList, string bestGame)
        {
            string s = "";
            s += String.Format("{0} {1} is a {3} years old {2} ", FirstName, LastName, Position1, Age);
            if (Position2 != " ")
                s += String.Format("(alternatively {0})", Position2);
            s += ", ";

            if (isActive)
                s += String.Format("who currently plays for the {0}.", TeamF);
            else
                s += String.Format("who is currently a Free Agent.");

            s += String.Format(" He's been a pro for {0} year", YearsPro);
            if (YearsPro > 1)
                s += "s";
            s += ".";

            s += "\n\n";

            s += String.Format("He averages {0:F1} PPG on {1:F1} MPG, making for {2:F1} points per 36 minutes. ", PPG, MPG, PTSR);

            if (rankingsTeam.list[ID][p.PPG] <= 3)
                s += String.Format("One of the best scorers in the team, #{0} among his teammates. ", rankingsTeam.list[ID][p.PPG]);
            if (rankingsPosition.list[ID][p.PPG] <= 10)
                s += String.Format("His performance has got him to become one of the best at his position in scoring, #{0} among {1}'s. ",
                                   rankingsPosition.list[ID][p.PPG], Position1);
            if (rankingsActive.list[ID][p.PPG] <= 10)
                s += String.Format("He's actually one of the best in the league in scoring, rated #{0} overall. ",
                                   rankingsActive.list[ID][p.PPG]);

            var statList = GetBestStatsList(5);

            s += "\n\n";

            foreach (var stat in statList)
            {
                switch (stat.Key)
                {
                    case "FG":
                        s +=
                            String.Format(
                                "Shooting, one of his main strengths. He's averaging {0} as far as field goals go. Percentage-wise, his performance " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.list[ID][p.FGp]);
                        if (rankingsTeam.list[ID][p.FGp] <= 3)
                            s += String.Format("Top from the floor in his team, ranks at #{0} ",
                                               rankingsTeam.list[ID][p.FGp]);
                        if (rankingsPosition.list[ID][p.FGp] <= 10)
                            s +=
                                String.Format(
                                    "Definitely dominating among {0}'s on scoring percentage, ranked at #{1}. ",
                                    Position1, rankingsPosition.list[ID][p.FGp]);
                        break;
                    case "3P":
                        s +=
                            String.Format(
                                "His 3-point shooting is another area of focus. His three-point shooting averages {0}. #{1} in the league in 3P%. ", stat.Value, rankingsActive.list[ID][p.TPp]);
                        if (rankingsTeam.list[ID][p.TPp] <= 3)
                            s += String.Format("One of the best guys from the arc in his team, ranks at #{0} ",
                                               rankingsTeam.list[ID][p.TPp]);
                        if (rankingsPosition.list[ID][p.TPp] <= 10)
                            s +=
                                String.Format(
                                    "Not many {0}'s do better than him, as he's ranked at #{1}. ",
                                    Position1, rankingsPosition.list[ID][p.TPp]);
                        break;
                    case "FT":
                        s +=
                            String.Format(
                                "Take a look at his free throw stats: He's averaging {0} from the line, which " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.list[ID][p.FTp]);
                        if (rankingsTeam.list[ID][p.FTp] <= 3)
                            s += String.Format("Coach might prefer him to get all the fouls late in the game, as he ranks #{0} in his team. ",
                                               rankingsTeam.list[ID][p.FTp]);
                        if (rankingsPosition.list[ID][p.FTp] <= 10)
                            s +=
                                String.Format(
                                    "Most {0}'s in the league struggle to keep up with him, he's ranked at #{1}. ",
                                    Position1, rankingsPosition.list[ID][p.FTp]);
                        break;
                    case "ORPG":
                        s +=
                            String.Format(
                                "Crashing the offensive glass, one of his main strengths. His average offensive boards per game are at {0}, which " +
                                "ranks him at #{1} overall. He grabs {2:F1} offensive rebounds every 36 minutes. ", stat.Value,
                                rankingsActive.list[ID][p.ORPG], OREBR);
                        if (rankingsTeam.list[ID][p.ORPG] <= 3)
                            s += String.Format("One of the main guys to worry about below your basket, #{0} in his team. ",
                                               rankingsTeam.list[ID][p.ORPG]);
                        if (rankingsPosition.list[ID][p.ORPG] <= 10)
                            s +=
                                String.Format(
                                    "He's ranked at #{1} among {0}'s in grabbing those second chance opportunities. ",
                                    Position1, rankingsPosition.list[ID][p.ORPG]);
                        break;
                    case "RPG":
                        s +=
                            String.Format(
                                "He makes a point of crashing the boards. His RPG are at {0} ({2:F1} per 36 minutes), which " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.list[ID][p.RPG], REBR);
                        if (rankingsTeam.list[ID][p.RPG] <= 3)
                            s += String.Format("One of the top rebounders in his team, #{0} actually. ",
                                               rankingsTeam.list[ID][p.RPG]);
                        if (rankingsPosition.list[ID][p.RPG] <= 10)
                            s +=
                                String.Format(
                                    "He's ranked at #{1} among {0}'s in crashing the boards. ",
                                    Position1, rankingsPosition.list[ID][p.RPG]);
                        break;
                    case "BPG":
                        s +=
                            String.Format(
                                "Keep him in mind when he's in your face. His BPG are at {0} ({2:F1} per 36 minutes), which " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.list[ID][p.BPG], BLKR);
                        if (rankingsTeam.list[ID][p.BPG] <= 3)
                            s += String.Format("Among the top blockers in the team, ranked at #{0}. ",
                                               rankingsTeam.list[ID][p.BPG]);
                        if (rankingsPosition.list[ID][p.BPG] <= 10)
                            s +=
                                String.Format(
                                    "One of the best {0}'s (#{1}) at blocking shots. ",
                                    Position1, rankingsPosition.list[ID][p.BPG]);
                        break;
                    case "APG":
                        s +=
                            String.Format(
                                "Assisting the ball, an important aspect of his game. He does {0} APG ({2:F1} per 36 minutes), ranking him at #{1} overall. ",
                                stat.Value, rankingsActive.list[ID][p.APG], ASTR);
                        if (rankingsTeam.list[ID][p.APG] <= 3)
                            s += String.Format("#{0} as far as playmakers in the team go. ",
                                               rankingsTeam.list[ID][p.APG]);
                        if (rankingsPosition.list[ID][p.APG] <= 10)
                            s +=
                                String.Format(
                                    "One of the league's best {0}'s (#{1}) at setting up teammates for a shot. ",
                                    Position1, rankingsPosition.list[ID][p.APG]);
                        break;
                    case "SPG":
                        s +=
                            String.Format(
                                "Tries to keep his hands active; keep in mind his {0} SPG ({2:F1} per 36 minutes). His performance in taking the ball away has " +
                                "ranked him at #{1} in the league. ", stat.Value, rankingsActive.list[ID][p.SPG], STLR);
                        if (rankingsTeam.list[ID][p.SPG] <= 3)
                            s += String.Format("#{0} in taking the ball away among his teammates. ",
                                               rankingsTeam.list[ID][p.SPG]);
                        if (rankingsPosition.list[ID][p.SPG] <= 10)
                            s +=
                                String.Format(
                                    "One of the league's best {0}'s (#{1}) in this aspect. ",
                                    Position1, rankingsPosition.list[ID][p.SPG]);
                        break;
                    case "FTM/FGA":
                        s +=
                            String.Format(
                                "He fights through contact to get to the line. His FTM/FGA rate is at {0}. ", stat.Value);
                        break;
                }
                s += "\n";
            }

            s += String.Format("His foul rate is at {0:F1} per 36 minutes, while his turnover rate is at {1:F1} per the same duration.\n\n",
                               (double)FOUL / MINS * 36, TOR);
            
            pbsList.Sort((pbs1, pbs2) => pbs1.RealDate.CompareTo(pbs2.RealDate));
            pbsList.Reverse();

            if (!String.IsNullOrWhiteSpace(bestGame))
            {
                string[] parts = bestGame.Split(new string[] {": ", " vs ", " (", "\n"}, StringSplitOptions.None);
                s += String.Format("His best game was at {0} against the {1}, with a Game Score of {2:F2} ", parts[1], parts[2],
                                   pbsList.Find(pbs => pbs.RealDate == Convert.ToDateTime(parts[1])).GmSc);
                s += "(";
                for (int i = 5; i < parts.Length; i++)
                {
                    if (String.IsNullOrWhiteSpace(parts[i]))
                        break;

                    s += String.Format("{0} {1}", parts[i + 1], parts[i]);
                    if (parts[i+2].Contains(")"))
                    {
                        s += String.Format(" ({0}, ", parts[i+2]);
                        i += 2;
                    }
                    else
                    {
                        s += ", ";
                        i += 1;
                    }
                }
                s = s.TrimEnd(new char[] {',', ' '});
                s += "). ";
            }

            if (pbsList.Count > 5)
            {
                double sum = 0;
                for (int i = 0; i<5;i++)
                {
                    sum += pbsList[i].GmSc;
                }
                double average = sum/5;
                s += String.Format("He's been averaging a Game Score of {0:F2} in his last 5 games, ", average);
                if (average > GmSc)
                {
                    s += String.Format("which can be considered an improvement compared to his season average of {0:F2}. ", GmSc);
                }
                else
                {
                    s += String.Format("which is lower than his season average of {0:F2}. ", GmSc);
                }
            }
            else if (pbsList.Count > 3)
            {
                double sum = 0;
                for (int i = 0; i < 5; i++)
                {
                    sum += pbsList[i].GmSc;
                }
                double average = sum / 3;
                s += String.Format("He's been averaging a Game Score of {0:F2} in his last 3 games, ", average);
                if (average > GmSc)
                {
                    s += String.Format("which can be considered an improvement compared to his season average of {0:F2}. ", GmSc);
                }
                else
                {
                    s += String.Format("which is lower than his season average of {0:F2}. ", GmSc);
                }
            }
            else
            {
                s += String.Format("He's been averaging a Game Score of {0:F2}. ", GmSc);
            }

            var cmw = new CopyableMessageWindow(s, String.Format("{0} {1}'s Scouting Report", FirstName, LastName), TextAlignment.Left);
            cmw.ShowDialog();
        }
    }
}