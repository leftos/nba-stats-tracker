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
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Helper.Miscellaneous;
using NBA_Stats_Tracker.Windows.MainInterface;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Data.Teams
{
    /// <summary>
    ///     A container for all of a team's information, stats, PerGame and metrics handled by the program.
    /// </summary>
    [Serializable]
    public class TeamStats
    {
        public int Conference;
        public string CurStreak;

        public string DisplayName;
        public int ID;
        public bool IsHidden;
        public Dictionary<string, double> Metrics = new Dictionary<string, double>();

        public string Name;
        public int Offset;

        /// <summary>
        ///     Averages for each team.
        ///     0: PPG, 1: PAPG, 2: FG%, 3: FGEff, 4: 3P%, 5: 3PEff, 6: FT%, 7:FTEff,
        ///     8: RPG, 9: ORPG, 10: DRPG, 11: SPG, 12: BPG, 13: TPG, 14: APG, 15: FPG, 16: W%,
        ///     17: Weff, 18: PD
        /// </summary>
        public float[] PerGame = new float[20];

        public Dictionary<string, double> PlMetrics = new Dictionary<string, double>();
        public int PlOffset;
        public float[] PlPerGame = new float[20];
        public uint[] PlRecord = new uint[2];
        public uint[] PlTotals = new uint[17];

        public uint[] Record = new uint[2];

        /// <summary>
        ///     Stats for each team.
        ///     0: M, 1: PF, 2: PA, 3: 0x0000, 4: FGM, 5: FGA, 6: 3PM, 7: 3PA, 8: FTM, 9: FTA,
        ///     10: OREB, 11: DREB, 12: STL, 13: TO, 14: BLK, 15: AST,
        ///     16: FOUL
        /// </summary>
        public uint[] Totals = new uint[17];

        private int _division;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamStats" /> class.
        /// </summary>
        public TeamStats()
        {
            ID = -1;
            prepareEmpty();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamStats" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TeamStats(int id) : this()
        {
            ID = id;
        }

        public TeamStats(int id, string name) : this()
        {
            ID = id;
            Name = name;
            DisplayName = name;
        }

        public TeamStats(TeamStatsRow tsr, bool playoffs = false)
        {
            Name = tsr.Name;
            DisplayName = tsr.DisplayName;

            if (!playoffs)
            {
                Record[0] = tsr.Wins;
                Record[1] = tsr.Losses;

                Totals[TAbbr.MINS] = tsr.MINS;
                Totals[TAbbr.PF] = tsr.PF;
                Totals[TAbbr.PA] = tsr.PA;
                Totals[TAbbr.FGM] = tsr.FGM;
                Totals[TAbbr.FGA] = tsr.FGA;
                Totals[TAbbr.TPM] = tsr.TPM;
                Totals[TAbbr.TPA] = tsr.TPA;
                Totals[TAbbr.FTM] = tsr.FTM;
                Totals[TAbbr.FTA] = tsr.FTA;
                Totals[TAbbr.OREB] = tsr.OREB;
                Totals[TAbbr.DREB] = tsr.DREB;
                Totals[TAbbr.STL] = tsr.STL;
                Totals[TAbbr.TOS] = tsr.TOS;
                Totals[TAbbr.BLK] = tsr.BLK;
                Totals[TAbbr.AST] = tsr.AST;
                Totals[TAbbr.FOUL] = tsr.FOUL;

                Metrics["PossPG"] = tsr.Poss;
                Metrics["Pace"] = tsr.Pace;
                Metrics["ORTG"] = tsr.ORTG;
                Metrics["DRTG"] = tsr.DRTG;
                Metrics["AST%"] = tsr.ASTp;
                Metrics["DREB%"] = tsr.DREBp;
                Metrics["EFG%"] = tsr.EFGp;
                Metrics["EFFd"] = tsr.EFFd;
                Metrics["TOR"] = tsr.TOR;
                Metrics["OREB%"] = tsr.OREBp;
                Metrics["FTR"] = tsr.FTR;
                Metrics["PW%"] = tsr.PWp;
                Metrics["TS%"] = tsr.TSp;
                Metrics["3PR"] = tsr.TPR;
                Metrics["PythW"] = tsr.PythW;
                Metrics["PythL"] = tsr.PythL;
            }
            else
            {
                PlRecord[0] = tsr.Wins;
                PlRecord[1] = tsr.Losses;

                PlTotals[TAbbr.MINS] = tsr.MINS;
                PlTotals[TAbbr.PF] = tsr.PF;
                PlTotals[TAbbr.PA] = tsr.PA;
                PlTotals[TAbbr.FGM] = tsr.FGM;
                PlTotals[TAbbr.FGA] = tsr.FGA;
                PlTotals[TAbbr.TPM] = tsr.TPM;
                PlTotals[TAbbr.TPA] = tsr.TPA;
                PlTotals[TAbbr.FTM] = tsr.FTM;
                PlTotals[TAbbr.FTA] = tsr.FTA;
                PlTotals[TAbbr.OREB] = tsr.OREB;
                PlTotals[TAbbr.DREB] = tsr.DREB;
                PlTotals[TAbbr.STL] = tsr.STL;
                PlTotals[TAbbr.TOS] = tsr.TOS;
                PlTotals[TAbbr.BLK] = tsr.BLK;
                PlTotals[TAbbr.AST] = tsr.AST;
                PlTotals[TAbbr.FOUL] = tsr.FOUL;

                PlMetrics["PossPG"] = tsr.Poss;
                PlMetrics["Pace"] = tsr.Pace;
                PlMetrics["ORTG"] = tsr.ORTG;
                PlMetrics["DRTG"] = tsr.DRTG;
                PlMetrics["AST%"] = tsr.ASTp;
                PlMetrics["DREB%"] = tsr.DREBp;
                PlMetrics["EFG%"] = tsr.EFGp;
                PlMetrics["EFFd"] = tsr.EFFd;
                PlMetrics["TOR"] = tsr.TOR;
                PlMetrics["OREB%"] = tsr.OREBp;
                PlMetrics["FTR"] = tsr.FTR;
                PlMetrics["PW%"] = tsr.PWp;
                PlMetrics["TS%"] = tsr.TSp;
                PlMetrics["3PR"] = tsr.TPR;
                PlMetrics["PythW"] = tsr.PythW;
                PlMetrics["PythL"] = tsr.PythL;
            }

            ID = tsr.ID;
            IsHidden = tsr.IsHidden;

            CalcAvg();
        }

        public int Division
        {
            get { return _division; }
            set
            {
                _division = value;
                try
                {
                    Conference = MainWindow.Divisions.Find(division1 => division1.ID == value).ConferenceID;
                }
                catch
                {
                    Console.WriteLine("Tried to set conference for team " + ID + " but couldn't detect division " + _division +
                                      "'s conference.");
                }
            }
        }

        /// <summary>
        ///     Prepares an empty TeamStats instance.
        /// </summary>
        private void prepareEmpty()
        {
            Record[0] = Convert.ToByte(0);
            Record[1] = Convert.ToByte(0);
            PlRecord[0] = Convert.ToByte(0);
            PlRecord[1] = Convert.ToByte(0);
            for (int i = 0; i < Totals.Length; i++)
            {
                Totals[i] = 0;
                PlTotals[i] = 0;
            }
            for (int i = 0; i < PerGame.Length; i++)
            {
                PerGame[i] = 0;
                PlPerGame[i] = 0;
            }
            IsHidden = false;
            Division = 0;
            Conference = 0;

            TAbbr.MetricsNames.ForEach(metricName =>
                {
                    Metrics.Add(metricName, double.NaN);
                    PlMetrics.Add(metricName, double.NaN);
                });
        }

        /// <summary>
        ///     Calculates the PerGame of a team's stats.
        /// </summary>
        public void CalcAvg()
        {
            uint games = Record[0] + Record[1];
            uint plGames = PlRecord[0] + PlRecord[1];

            PerGame[TAbbr.Wp] = (float) Record[0]/games;
            PerGame[TAbbr.Weff] = PerGame[TAbbr.Wp]*Record[0];
            PerGame[TAbbr.PPG] = (float) Totals[TAbbr.PF]/games;
            PerGame[TAbbr.PAPG] = (float) Totals[TAbbr.PA]/games;
            PerGame[TAbbr.FGp] = (float) Totals[TAbbr.FGM]/Totals[TAbbr.FGA];
            PerGame[TAbbr.FGeff] = PerGame[TAbbr.FGp]*((float) Totals[TAbbr.FGM]/games);
            PerGame[TAbbr.TPp] = (float) Totals[TAbbr.TPM]/Totals[TAbbr.TPA];
            PerGame[TAbbr.TPeff] = PerGame[TAbbr.TPp]*((float) Totals[TAbbr.TPM]/games);
            PerGame[TAbbr.FTp] = (float) Totals[TAbbr.FTM]/Totals[TAbbr.FTA];
            PerGame[TAbbr.FTeff] = PerGame[TAbbr.FTp]*((float) Totals[TAbbr.FTM]/games);
            PerGame[TAbbr.RPG] = (float) (Totals[TAbbr.OREB] + Totals[TAbbr.DREB])/games;
            PerGame[TAbbr.ORPG] = (float) Totals[TAbbr.OREB]/games;
            PerGame[TAbbr.DRPG] = (float) Totals[TAbbr.DREB]/games;
            PerGame[TAbbr.SPG] = (float) Totals[TAbbr.STL]/games;
            PerGame[TAbbr.BPG] = (float) Totals[TAbbr.BLK]/games;
            PerGame[TAbbr.TPG] = (float) Totals[TAbbr.TOS]/games;
            PerGame[TAbbr.APG] = (float) Totals[TAbbr.AST]/games;
            PerGame[TAbbr.FPG] = (float) Totals[TAbbr.FOUL]/games;
            PerGame[TAbbr.PD] = PerGame[TAbbr.PPG] - PerGame[TAbbr.PAPG];
            PerGame[TAbbr.MPG] = (float) Totals[TAbbr.MINS]/games;

            PlPerGame[TAbbr.Wp] = (float) PlRecord[0]/plGames;
            PlPerGame[TAbbr.Weff] = PlPerGame[TAbbr.Wp]*PlRecord[0];
            PlPerGame[TAbbr.PPG] = (float) PlTotals[TAbbr.PF]/plGames;
            PlPerGame[TAbbr.PAPG] = (float) PlTotals[TAbbr.PA]/plGames;
            PlPerGame[TAbbr.FGp] = (float) PlTotals[TAbbr.FGM]/PlTotals[TAbbr.FGA];
            PlPerGame[TAbbr.FGeff] = PlPerGame[TAbbr.FGp]*((float) PlTotals[TAbbr.FGM]/plGames);
            PlPerGame[TAbbr.TPp] = (float) PlTotals[TAbbr.TPM]/PlTotals[TAbbr.TPA];
            PlPerGame[TAbbr.TPeff] = PlPerGame[TAbbr.TPp]*((float) PlTotals[TAbbr.TPM]/plGames);
            PlPerGame[TAbbr.FTp] = (float) PlTotals[TAbbr.FTM]/PlTotals[TAbbr.FTA];
            PlPerGame[TAbbr.FTeff] = PlPerGame[TAbbr.FTp]*((float) PlTotals[TAbbr.FTM]/plGames);
            PlPerGame[TAbbr.RPG] = (float) (PlTotals[TAbbr.OREB] + PlTotals[TAbbr.DREB])/plGames;
            PlPerGame[TAbbr.ORPG] = (float) PlTotals[TAbbr.OREB]/plGames;
            PlPerGame[TAbbr.DRPG] = (float) PlTotals[TAbbr.DREB]/plGames;
            PlPerGame[TAbbr.SPG] = (float) PlTotals[TAbbr.STL]/plGames;
            PlPerGame[TAbbr.BPG] = (float) PlTotals[TAbbr.BLK]/plGames;
            PlPerGame[TAbbr.TPG] = (float) PlTotals[TAbbr.TOS]/plGames;
            PlPerGame[TAbbr.APG] = (float) PlTotals[TAbbr.AST]/plGames;
            PlPerGame[TAbbr.FPG] = (float) PlTotals[TAbbr.FOUL]/plGames;
            PlPerGame[TAbbr.PD] = PlPerGame[TAbbr.PPG] - PlPerGame[TAbbr.PAPG];
            PlPerGame[TAbbr.MPG] = (float) PlTotals[TAbbr.MINS]/plGames;
        }

        /// <summary>
        ///     Calculates the league PerGame.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="statRange">The stat range.</param>
        /// <returns></returns>
        public static TeamStats CalculateLeagueAverages(Dictionary<int, TeamStats> tst, Span statRange)
        {
            var ls = new TeamStats(-1, "League");
            uint teamCount = countTeams(tst, statRange);
            for (int i = 0; i < tst.Count; i++)
            {
                ls.AddTeamStats(tst[i], statRange);
            }
            ls.CalcMetrics(ls, (statRange == Span.Playoffs));

            ls.Record[0] /= teamCount;
            ls.Record[1] /= teamCount;
            ls.PlRecord[0] /= teamCount;
            ls.PlRecord[1] /= teamCount;
            ls.PerGame[TAbbr.Weff] /= teamCount;
            ls.PlPerGame[TAbbr.Weff] /= teamCount;

            return ls;
        }

        /// <summary>
        ///     Calculates the team metrics for all the teams.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the metric stats will be calculated for the playoff performances of the teams.
        /// </param>
        public static void CalculateAllMetrics(ref Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstOpp,
                                               bool playoffs = false)
        {
            List<int> tstKeys = tst.Keys.ToList();
            for (int i = 0; i < tst.Keys.Count; i++)
            {
                int key = tstKeys[i];
                tst[key].CalcMetrics(tstOpp[key], playoffs);
            }
        }

        /// <summary>
        ///     Counts the teams having more than one game in a specific time-span of the league's calendar.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="statRange">The stat range.</param>
        /// <returns></returns>
        private static uint countTeams(Dictionary<int, TeamStats> tst, Span statRange)
        {
            uint teamCount = 0;

            if (statRange != Span.Playoffs)
            {
                for (int i = 0; i < tst.Count; i++)
                {
                    if (tst[i].GetGames() > 0)
                    {
                        teamCount++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < tst.Count; i++)
                {
                    if (tst[i].GetPlayoffGames() > 0)
                    {
                        teamCount++;
                    }
                }
            }
            return (teamCount != 0) ? teamCount : 1;
        }

        /// <summary>
        ///     Calculates the metric stats for this team.
        /// </summary>
        /// <param name="tsopp">The opposing team stats.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the metrics will be calculated based on the team's playoff performances.
        /// </param>
        public void CalcMetrics(TeamStats tsopp, bool playoffs = false)
        {
            var tempMetrics = new Dictionary<string, double>();

            var tempTotals = new double[Totals.Length];
            for (int i = 0; i < Totals.Length; i++)
            {
                if (!playoffs)
                {
                    tempTotals[i] = Totals[i];
                }
                else
                {
                    tempTotals[i] = PlTotals[i];
                }
            }

            var toppstats = new double[tsopp.Totals.Length];
            for (int i = 0; i < tsopp.Totals.Length; i++)
            {
                if (!playoffs)
                {
                    toppstats[i] = tsopp.Totals[i];
                }
                else
                {
                    toppstats[i] = tsopp.PlTotals[i];
                }
            }

            uint games = (!playoffs) ? GetGames() : GetPlayoffGames();

            double poss = calcPossMetric(tempTotals, toppstats);
            tempMetrics.Add("Poss", poss);
            tempMetrics.Add("PossPG", poss/games);

            poss = calcPossMetric(toppstats, tempTotals);

            Dictionary<string, double> toppmetrics = (!playoffs) ? tsopp.Metrics : tsopp.PlMetrics;
            try
            {
                toppmetrics.Add("Poss", poss);
            }
            catch
            {
                Console.WriteLine("Possessions metric couldn't be calculated for team " + ID);
            }

            double pace = MainWindow.GameLength*((tempMetrics["Poss"] + toppmetrics["Poss"])/(2*(tempTotals[TAbbr.MINS])));
            tempMetrics.Add("Pace", pace);

            double ortg = (tempTotals[TAbbr.PF]/tempMetrics["Poss"])*100;
            tempMetrics.Add("ORTG", ortg);

            double drtg = (tempTotals[TAbbr.PA]/tempMetrics["Poss"])*100;
            tempMetrics.Add("DRTG", drtg);

            double astP = (tempTotals[TAbbr.AST])/
                          (tempTotals[TAbbr.FGA] + tempTotals[TAbbr.FTA]*0.44 + tempTotals[TAbbr.AST] + tempTotals[TAbbr.TOS]);
            tempMetrics.Add("AST%", astP);

            double drebP = tempTotals[TAbbr.DREB]/(tempTotals[TAbbr.DREB] + toppstats[TAbbr.OREB]);
            tempMetrics.Add("DREB%", drebP);

            double efgP = (tempTotals[TAbbr.FGM] + tempTotals[TAbbr.TPM]*0.5)/tempTotals[TAbbr.FGA];
            tempMetrics.Add("EFG%", efgP);

            double effD = ortg - drtg;
            tempMetrics.Add("EFFd", effD);

            double tor = tempTotals[TAbbr.TOS]/(tempTotals[TAbbr.FGA] + 0.44*tempTotals[TAbbr.FTA] + tempTotals[TAbbr.TOS]);
            tempMetrics.Add("TOR", tor);

            double orebP = tempTotals[TAbbr.OREB]/(tempTotals[TAbbr.OREB] + toppstats[TAbbr.DREB]);
            tempMetrics.Add("OREB%", orebP);

            double ftr = tempTotals[TAbbr.FTM]/tempTotals[TAbbr.FGA];
            tempMetrics.Add("FTR", ftr);

            float[] tempPerGame = (!playoffs) ? PerGame : PlPerGame;

            double pwP = (((tempPerGame[TAbbr.PPG] - tempPerGame[TAbbr.PAPG])*2.7) + ((double) MainWindow.SeasonLength/2))/
                         MainWindow.SeasonLength;
            tempMetrics.Add("PW%", pwP);

            double tsP = tempTotals[TAbbr.PF]/(2*(tempTotals[TAbbr.FGA] + 0.44*tempTotals[TAbbr.FTA]));
            tempMetrics.Add("TS%", tsP);

            double tpr = tempTotals[TAbbr.TPA]/tempTotals[TAbbr.FGA];
            tempMetrics.Add("3PR", tpr);

            double pythW = MainWindow.SeasonLength*(Math.Pow(tempTotals[TAbbr.PF], 16.5))/
                           (Math.Pow(tempTotals[TAbbr.PF], 16.5) + Math.Pow(tempTotals[TAbbr.PA], 16.5));
            tempMetrics.Add("PythW", pythW);

            double pythL = MainWindow.SeasonLength - pythW;
            tempMetrics.Add("PythL", pythL);

            double gmsc = tempTotals[TAbbr.PF] + 0.4*tempTotals[TAbbr.FGM] - 0.7*tempTotals[TAbbr.FGA] -
                          0.4*(tempTotals[TAbbr.FTA] - tempTotals[TAbbr.FTM]) + 0.7*tempTotals[TAbbr.OREB] + 0.3*tempTotals[TAbbr.DREB] +
                          tempTotals[TAbbr.STL] + 0.7*tempTotals[TAbbr.AST] + 0.7*tempTotals[TAbbr.BLK] - 0.4*tempTotals[TAbbr.FOUL] -
                          tempTotals[TAbbr.TOS];
            tempMetrics.Add("GmSc", gmsc/games);


            if (!playoffs)
            {
                Metrics = new Dictionary<string, double>(tempMetrics);
            }
            else
            {
                PlMetrics = new Dictionary<string, double>(tempMetrics);
            }
        }

        /// <summary>
        ///     Calculates the Possessions metric.
        /// </summary>
        /// <param name="tstats">The team stats.</param>
        /// <param name="toppstats">The opposing team stats.</param>
        /// <returns></returns>
        private static double calcPossMetric(double[] tstats, double[] toppstats)
        {
            double poss = 0.5*
                          ((tstats[TAbbr.FGA] + 0.4*tstats[TAbbr.FTA] -
                            1.07*(tstats[TAbbr.OREB]/(tstats[TAbbr.OREB] + toppstats[TAbbr.DREB]))*
                            (tstats[TAbbr.FGA] - tstats[TAbbr.FGM]) + tstats[TAbbr.TOS]) +
                           (toppstats[TAbbr.FGA] + 0.4*toppstats[TAbbr.FTA] -
                            1.07*(toppstats[TAbbr.OREB]/(toppstats[TAbbr.OREB] + tstats[TAbbr.DREB]))*
                            (toppstats[TAbbr.FGA] - toppstats[TAbbr.FGM]) + toppstats[TAbbr.TOS]));
            return poss;
        }

        /// <summary>
        ///     Gets the amount of games played by the team.
        /// </summary>
        /// <returns></returns>
        public uint GetGames()
        {
            uint games = Record[0] + Record[1];
            return games;
        }

        /// <summary>
        ///     Gets the amount of playoff games played by the team.
        /// </summary>
        /// <returns></returns>
        public uint GetPlayoffGames()
        {
            uint plGames = PlRecord[0] + PlRecord[1];
            return plGames;
        }

        /// <summary>
        ///     Adds the team stats from a TeamStats instance to the current stats.
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
                        Record[0] += ts.Record[0];
                        Record[1] += ts.Record[1];

                        for (int i = 0; i < Totals.Length; i++)
                        {
                            Totals[i] += ts.Totals[i];
                        }

                        CalcAvg();
                        break;
                    }
                case Span.Playoffs:
                    {
                        PlRecord[0] += ts.PlRecord[0];
                        PlRecord[1] += ts.PlRecord[1];

                        for (int i = 0; i < PlTotals.Length; i++)
                        {
                            PlTotals[i] += ts.PlTotals[i];
                        }

                        CalcAvg();
                        break;
                    }
                case Span.SeasonAndPlayoffs:
                    {
                        Record[0] += ts.Record[0];
                        Record[1] += ts.Record[1];

                        for (int i = 0; i < Totals.Length; i++)
                        {
                            Totals[i] += ts.Totals[i];
                        }

                        Record[0] += ts.PlRecord[0];
                        Record[1] += ts.PlRecord[1];

                        for (int i = 0; i < PlTotals.Length; i++)
                        {
                            Totals[i] += ts.PlTotals[i];
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
        ///     Resets the stats.
        /// </summary>
        /// <param name="mode">The time-span.</param>
        /// <exception cref="System.Exception">Team Reset Stats called with invalid parameter.</exception>
        public void ResetStats(Span mode)
        {
            switch (mode)
            {
                case Span.Season:
                    {
                        Record[0] = 0;
                        Record[1] = 0;

                        for (int i = 0; i < Totals.Length; i++)
                        {
                            Totals[i] = 0;
                        }

                        CalcAvg();
                        break;
                    }
                case Span.Playoffs:
                    {
                        PlRecord[0] = 0;
                        PlRecord[1] = 0;

                        for (int i = 0; i < PlTotals.Length; i++)
                        {
                            PlTotals[i] = 0;
                        }

                        CalcAvg();
                        break;
                    }
                case Span.SeasonAndPlayoffs:
                    {
                        Record[0] = 0;
                        Record[1] = 0;

                        for (int i = 0; i < Totals.Length; i++)
                        {
                            Totals[i] = 0;
                        }

                        PlRecord[0] = 0;
                        PlRecord[1] = 0;

                        for (int i = 0; i < PlTotals.Length; i++)
                        {
                            PlTotals[i] = 0;
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
        ///     Gets the winning percentage.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <returns></returns>
        public double GetWinningPercentage(Span span)
        {
            if (span == Span.Season)
            {
                try
                {
                    return ((double) Record[0])/GetGames();
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
            else if (span == Span.Playoffs)
            {
                try
                {
                    return ((double) PlRecord[0])/GetGames();
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
            else
            {
                try
                {
                    return ((double) (Record[0] + PlRecord[0]))/(GetGames() + GetPlayoffGames());
                }
                catch (DivideByZeroException)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        ///     Returns a well-formatted multi-line string presenting a scouting report for the team in natural language.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="psrList"> </param>
        /// <returns></returns>
        public string ScoutingReport(Dictionary<int, TeamStats> tst, ObservableCollection<PlayerStatsRow> psrList,
                                     TeamRankings teamRankings, bool playoffs = false)
        {
            uint[] tempRecord = playoffs ? PlRecord : Record;
            uint[] tempTotals = playoffs ? PlTotals : Totals;
            float[] tempPerGame = playoffs ? PlPerGame : PerGame;

            List<PlayerStatsRow> pgList = psrList.Where(ps => ps.Position1 == Position.PG).ToList();
            pgList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps1.GmSc));
            pgList.Reverse();
            List<PlayerStatsRow> sgList = psrList.Where(ps => ps.Position1 == Position.SG).ToList();
            sgList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            sgList.Reverse();
            List<PlayerStatsRow> sfList = psrList.Where(ps => ps.Position1 == Position.SF).ToList();
            sfList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            sfList.Reverse();
            List<PlayerStatsRow> pfList = psrList.Where(ps => ps.Position1 == Position.PF).ToList();
            pfList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            pfList.Reverse();
            List<PlayerStatsRow> cList = psrList.Where(ps => ps.Position1 == Position.C).ToList();
            cList.Sort((ps1, ps2) => ps1.GmSc.CompareTo(ps2.GmSc));
            cList.Reverse();

            string roster = "Team Roster\n";
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

            int[][] rating = teamRankings.RankingsPerGame;
            int teamCount = tst.Count;
            int divpos = 0;
            int confpos = 0;

            List<KeyValuePair<int, TeamStats>> divTeams = tst.Where(pair => pair.Value.Division == Division).ToList();
            divTeams.Sort((t1, t2) => t1.Value.GetWinningPercentage(Span.Season).CompareTo(t2.Value.GetWinningPercentage(Span.Season)));
            divTeams.Reverse();
            for (int i = 0; i < divTeams.Count; i++)
            {
                if (divTeams[i].Value.ID == ID)
                {
                    divpos = i + 1;
                    break;
                }
            }
            List<KeyValuePair<int, TeamStats>> confTeams = tst.Where(pair => pair.Value.Conference == Conference).ToList();
            confTeams.Sort((t1, t2) => t1.Value.GetWinningPercentage(Span.Season).CompareTo(t2.Value.GetWinningPercentage(Span.Season)));
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
            msg += String.Format("{0}, the {1}{2}", DisplayName, rating[ID][17], Misc.GetRankingSuffix(rating[ID][17]));

            int topThird = teamCount/3;
            int secondThird = teamCount/3*2;
            int topHalf = teamCount/2;

            msg +=
                string.Format(
                    " strongest team in the league right now, after having played {0} games. Their record is currently at {1}-{2}",
                    (tempRecord[0] + tempRecord[1]), tempRecord[0], tempRecord[1]);

            if (!playoffs && MainWindow.Divisions.Count > 1)
            {
                msg += ", putting them at #" + divpos + " in their division and at #" + confpos + " in their conference";
            }

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
                    msg += "Average offensive team. Not really efficient in anything they do when they bring the ball down " +
                           "the court.";
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
            {
                msg += "Top scoring team, one of the top 5 in field goal efficiency.";
            }
            else if (rating[ID][3] <= topThird)
            {
                msg += "You'll have to worry about their scoring efficiency, as they're in the top third of the league.";
            }
            else if (rating[ID][3] <= secondThird)
            {
                msg += "Scoring is not their virtue, but they're not that bad either.";
            }
            else if (rating[ID][3] <= teamCount)
            {
                msg += "You won't have to worry about their scoring, one of the least 10 efficient in the league.";
            }

            int comp = rating[ID][TAbbr.FGeff] - rating[ID][TAbbr.FGp];
            if (comp < -topHalf)
            {
                msg +=
                    "\nThey score more baskets than their FG% would have you guess, but they need to work on getting more consistent.";
            }
            else if (comp > topHalf)
            {
                msg +=
                    "\nThey can be dangerous whenever they shoot the ball. Their offense just doesn't get them enough chances to shoot it, though.";
            }

            msg += String.Format(" (#{0} in FG%: {1:F3} - #{2} in FGeff: {3:F2})", rating[ID][TAbbr.FGp], tempPerGame[TAbbr.FGp],
                                 rating[ID][TAbbr.FGeff], tempPerGame[TAbbr.FGeff]);
            msg += "\n";

            if (rating[ID][5] <= 5)
            {
                msg += "You'll need to always have an eye on the perimeter. They can turn a game around with their 3 pointers. " +
                       "They score well, they score a lot.";
            }
            else if (rating[ID][5] <= topThird)
            {
                msg +=
                    "Their 3pt shooting is bad news. They're in the top third of the league, and you can't relax playing against them.";
            }
            else if (rating[ID][5] <= secondThird)
            {
                msg += "Not much to say about their 3pt shooting. Average, but it is there.";
            }
            else if (rating[ID][5] <= teamCount)
            {
                msg += "Definitely not a threat from 3pt land, one of the worst in the league. They waste too many shots from there.";
            }

            comp = rating[ID][TAbbr.TPeff] - rating[ID][TAbbr.TPp];
            if (comp < -topHalf)
            {
                msg += "\nThey'll get enough 3 pointers to go down each night, but not on a good enough percentage for that amount.";
            }
            else if (comp > topHalf)
            {
                msg += "\nWith their accuracy from the 3PT line, you'd think they'd shoot more of those.";
            }

            msg += String.Format(" (#{0} in 3P%: {1:F3} - #{2} in 3Peff: {3:F2})", rating[ID][TAbbr.TPp], tempPerGame[TAbbr.TPp],
                                 rating[ID][TAbbr.TPeff], tempPerGame[TAbbr.TPeff]);
            msg += "\n";

            if (rating[ID][7] <= 5)
            {
                msg += "They tend to attack the lanes hard, getting to the line and making the most of it. They're one of the best " +
                       "teams in the league at it.";
            }
            else if (rating[ID][7] <= topThird)
            {
                msg +=
                    "One of the best teams in the league at getting to the line. They get enough free throws to punish the opposing team every night. Top third of the league.";
            }
            else if (rating[ID][7] <= secondThird)
            {
                msg +=
                    "Average free throw efficiency, you don't have to worry about sending them to the line; at least as much as other aspects of their game.";
            }
            else if (rating[ID][7] <= teamCount)
            {
                if (rating[ID][TAbbr.FTp] < topHalf)
                {
                    msg +=
                        "A team that you'll enjoy playing hard and aggressively against on defense. They don't know how to get to the line.";
                }
                else
                {
                    msg +=
                        "A team that doesn't know how to get to the line, or how to score from there. You don't have to worry about freebies against them.";
                }
            }

            msg += String.Format(" (#{0} in FT%: {1:F3} - #{2} in FTeff: {3:F2})", rating[ID][TAbbr.FTp], tempPerGame[TAbbr.FTp],
                                 rating[ID][TAbbr.FTeff], tempPerGame[TAbbr.FTeff]);
            comp = rating[ID][TAbbr.FTeff] - rating[ID][TAbbr.FTp];
            if (comp < -topHalf)
            {
                msg +=
                    "\nAlthough they get to the line a lot and make some free throws, they have to put up a lot to actually get that amount each night.";
            }
            else if (comp > topHalf)
            {
                msg += "\nThey're lethal when shooting free throws, but they need to play harder and get there more often.";
            }

            msg += "\n";

            if (rating[ID][14] <= topHalf)
            {
                msg +=
                    "They know how to find the open man, and they get their offense going by getting it around the perimeter until a clean shot is there.";
            }
            else if ((rating[ID][14] > topHalf) && (rating[ID][3] < topThird))
            {
                msg +=
                    "A team that prefers to run its offense through its core players in isolation. Not very good in assists, but they know how to get the job " +
                    "done more times than not.";
            }
            else
            {
                msg +=
                    "A team that seems to have some selfish players around, nobody really that efficient to carry the team into high percentages.";
            }

            msg += String.Format(" (#{0} in APG: {1:F1})", rating[ID][TAbbr.APG], tempPerGame[TAbbr.APG]);
            msg += "\n\n";

            if (31 - rating[ID][TAbbr.PAPG] <= 5)
            {
                msg +=
                    "Don't expect to get your score high against them. An elite defensive team, top 5 in points against them each night.";
            }
            else if (31 - rating[ID][TAbbr.PAPG] <= topThird)
            {
                msg += "One of the better defensive teams out there, limiting their opponents to low scores night in, night out.";
            }
            else if (31 - rating[ID][TAbbr.PAPG] <= secondThird)
            {
                msg += "Average defensively, not much to show for it, but they're no blow-outs.";
            }
            else if (31 - rating[ID][TAbbr.PAPG] <= teamCount)
            {
                msg += "This team has just forgotten what defense is. They're one of the 10 easiest teams to score against.";
            }

            msg += String.Format(" (#{0} in PAPG: {1:F1})", tst.Count + 1 - rating[ID][TAbbr.PAPG], tempPerGame[TAbbr.PAPG]);
            msg += "\n\n";

            if ((rating[ID][9] <= topThird) && (rating[ID][11] <= topThird) && (rating[ID][12] <= topThird))
            {
                msg +=
                    "Hustle is their middle name. They attack the offensive glass, they block, they steal. Don't even dare to blink or get complacent.\n\n";
            }
            else if ((rating[ID][9] >= secondThird) && (rating[ID][11] >= secondThird) && (rating[ID][12] >= secondThird))
            {
                msg += "This team just doesn't know what hustle means. You'll be doing circles around them if you're careful.\n\n";
            }

            if (rating[ID][8] <= 5)
            {
                msg += "Sensational rebounding team, everybody jumps for the ball, no missed shot is left loose.";
            }
            else if (rating[ID][8] <= topThird)
            {
                msg +=
                    "You can't ignore their rebounding ability, they work together and are in the top third of the league in rebounding.";
            }
            else if (rating[ID][8] <= secondThird)
            {
                msg += "They crash the boards as much as the next guy, but they won't give up any freebies.";
            }
            else if (rating[ID][8] <= teamCount)
            {
                msg +=
                    "Second chance points? One of their biggest fears. Low low LOW rebounding numbers; just jump for the ball and you'll keep your score high.";
            }

            msg += " ";

            if ((rating[ID][9] <= topThird) && (rating[ID][10] <= topThird))
            {
                msg +=
                    "The work they put on rebounding on both sides of the court is commendable. Both offensive and defensive rebounds, their bread and butter.";
            }

            msg += String.Format(" (#{0} in RPG: {1:F1}, #{2} in ORPG: {3:F1}, #{4} in DRPG: {5:F1})", rating[ID][TAbbr.RPG],
                                 tempPerGame[TAbbr.RPG], rating[ID][TAbbr.ORPG], tempPerGame[TAbbr.ORPG], rating[ID][TAbbr.DRPG],
                                 tempPerGame[TAbbr.DRPG]);
            msg += "\n\n";

            if ((rating[ID][11] <= topThird) && (rating[ID][12] <= topThird))
            {
                msg +=
                    "A team that knows how to play defense. They're one of the best in steals and blocks, and they make you work hard on offense.";
            }
            else if (rating[ID][11] <= topThird)
            {
                msg +=
                    "Be careful dribbling and passing. They won't be much trouble once you shoot the ball, but the trouble is getting there. Great in steals.";
            }
            else if (rating[ID][12] <= topThird)
            {
                msg +=
                    "Get that thing outta here! Great blocking team, they turn the lights off on any mismatched jumper or drive; sometimes even when you least expect it.";
            }
            else
            {
                msg += "Nothing too significant as far as blocks and steals go.";
            }
            msg += String.Format(" (#{0} in SPG: {1:F1}, #{2} in BPG: {3:F1})\n", rating[ID][TAbbr.SPG], tempPerGame[TAbbr.SPG],
                                 rating[ID][TAbbr.BPG], tempPerGame[TAbbr.BPG]);

            if ((rating[ID][13] <= topThird) && (rating[ID][15] <= topThird))
            {
                msg +=
                    "Clumsy team to say the least. They're not careful with the ball, and they foul too much. Keep your eyes open and play hard.";
            }
            else if (rating[ID][13] < topThird)
            {
                msg +=
                    "Not good ball handlers, and that's being polite. Bottom 10 in turnovers, they have work to do until they get their offense going.";
            }
            else if (rating[ID][15] < topThird)
            {
                msg += "A team that's prone to fouling. You better drive the lanes as hard as you can, you'll get to the line a lot.";
            }
            else
            {
                msg +=
                    "This team is careful with and without the ball. They're good at keeping their turnovers down, and don't foul too much.\nDon't throw " +
                    "your players into steals or fouls against them, because they play smart, and you're probably going to see the opposite call than the " +
                    "one you expected.";
            }
            msg += String.Format(" (#{0} in TPG: {1:F1}, #{2} in FPG: {3:F1})", tst.Count + 1 - rating[ID][TAbbr.TPG],
                                 tempPerGame[TAbbr.TPG], tst.Count + 1 - rating[ID][TAbbr.FPG], tempPerGame[TAbbr.FPG]);

            msg += "\n\n";

            msg += "In summary, their best areas are ";
            var dict = new Dictionary<int, int>();
            for (int k = 0; k < rating[ID].Length; k++)
            {
                dict.Add(k, rating[ID][k]);
            }
            dict[TAbbr.FPG] = tst.Count + 1 - dict[TAbbr.FPG];
            dict[TAbbr.TPG] = tst.Count + 1 - dict[TAbbr.TPG];
            dict[TAbbr.PAPG] = tst.Count + 1 - dict[TAbbr.PAPG];
            List<int> strengths = (from entry in dict
                                   orderby entry.Value ascending
                                   select entry.Key).ToList();
            int m = 0;
            int j = 5;
            while (true)
            {
                if (m == j)
                {
                    break;
                }
                switch (strengths[m])
                {
                    case TAbbr.APG:
                        msg += String.Format("assists (#{0}, {1:F1}), ", rating[ID][TAbbr.APG], tempPerGame[TAbbr.APG]);
                        break;
                    case TAbbr.BPG:
                        msg += String.Format("blocks (#{0}, {1:F1}), ", rating[ID][TAbbr.BPG], tempPerGame[TAbbr.BPG]);
                        break;
                    case TAbbr.DRPG:
                        msg += String.Format("defensive rebounds (#{0}, {1:F1}), ", rating[ID][TAbbr.DRPG], tempPerGame[TAbbr.DRPG]);
                        break;
                    case TAbbr.FGeff:
                        msg += String.Format("field goals (#{0}, {1:F1} per game on {2:F3}), ", rating[ID][TAbbr.FGeff],
                                             (double) tempTotals[TAbbr.FGM]/GetGames(), tempPerGame[TAbbr.FGp]);
                        break;
                    case TAbbr.FPG:
                        msg += String.Format("fouls (#{0}, {1:F1}), ", tst.Count + 1 - rating[ID][TAbbr.FPG], tempPerGame[TAbbr.FPG]);
                        break;
                    case TAbbr.FTeff:
                        msg += String.Format("free throws (#{0}, {1:F1} per game on {2:F3}), ", rating[ID][TAbbr.FTeff],
                                             (double) tempTotals[TAbbr.FTM]/GetGames(), tempPerGame[TAbbr.FTp]);
                        break;
                    case TAbbr.ORPG:
                        msg += String.Format("offensive rebounds (#{0}, {1:F1}), ", rating[ID][TAbbr.ORPG], tempPerGame[TAbbr.ORPG]);
                        break;
                    case TAbbr.PAPG:
                        msg += String.Format("points allowed per game (#{0}, {1:F1}), ", tst.Count + 1 - rating[ID][TAbbr.PAPG],
                                             tempPerGame[TAbbr.PAPG]);
                        break;
                    case TAbbr.PPG:
                        msg += String.Format("scoring (#{0}, {1:F1}), ", rating[ID][TAbbr.PPG], tempPerGame[TAbbr.PPG]);
                        break;
                    case TAbbr.RPG:
                        msg += String.Format("rebounds (#{0}, {1:F1}), ", rating[ID][TAbbr.RPG], tempPerGame[TAbbr.RPG]);
                        break;
                    case TAbbr.SPG:
                        msg += String.Format("steals (#{0}, {1:F1}), ", rating[ID][TAbbr.SPG], tempPerGame[TAbbr.SPG]);
                        break;
                    case TAbbr.TPG:
                        msg += String.Format("turnovers (#{0}, {1:F1}), ", tst.Count + 1 - rating[ID][TAbbr.TPG],
                                             tempPerGame[TAbbr.TPG]);
                        break;
                    case TAbbr.TPeff:
                        msg += String.Format("three-pointers (#{0}, {1:F1} per game on {2:F3}), ", rating[ID][TAbbr.TPeff],
                                             (double) tempTotals[TAbbr.TPM]/GetGames(), tempPerGame[TAbbr.TPp]);
                        break;
                    default:
                        j++;
                        break;
                }
                m++;
            }
            msg = msg.TrimEnd(new[] {' ', ','});
            msg += ".";
            return msg;
        }

        /// <summary>
        ///     Determines whether the team is hidden for the current season.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="id">The name of the team.</param>
        /// <param name="season">The season ID.</param>
        /// <returns>
        ///     <c>true</c> if the team is hidden; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTeamHiddenInSeason(string file, int id, int season)
        {
            var db = new SQLiteDatabase(file);
            int maxSeason = SQLiteIO.SQLiteIO.GetMaxSeason(file);
            string teamsT = "Teams";
            if (season != maxSeason)
            {
                teamsT += "S" + season;
            }

            string q = "select isHidden from " + teamsT + " where ID = " + id + "";
            bool isHidden = ParseCell.GetBoolean(db.GetDataTable(q).Rows[0], "isHidden");

            return isHidden;
        }

        /// <summary>
        ///     Adds the team stats from a box score.
        /// </summary>
        /// <param name="bsToAdd">The box score to add.</param>
        /// <param name="ts1">The first team's team stats.</param>
        /// <param name="ts2">The second team's team stats.</param>
        /// <param name="ignorePlayoffFlag">Whether to ignore the playoff flag in the box score and add the stats to the season totals.</param>
        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref TeamStats ts1, ref TeamStats ts2,
                                                    bool ignorePlayoffFlag = false)
        {
            var tst = new Dictionary<int, TeamStats> {{1, ts1}, {2, ts2}};
            var tstOpp = new Dictionary<int, TeamStats> {{1, new TeamStats()}, {2, new TeamStats()}};
            AddTeamStatsFromBoxScore(bsToAdd, ref tst, ref tstOpp, 1, 2, ignorePlayoffFlag);
            ts1 = tst[1];
            ts2 = tst[2];
        }

        /// <summary>
        ///     Adds the team stats from a box score.
        /// </summary>
        /// <param name="bsToAdd">The box score to add.</param>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref Dictionary<int, TeamStats> tst,
                                                    ref Dictionary<int, TeamStats> tstOpp)
        {
            AddTeamStatsFromBoxScore(bsToAdd, ref tst, ref tstOpp, bsToAdd.Team1ID, bsToAdd.Team2ID);
        }

        /// <summary>
        ///     Adds the team stats from a box score.
        /// </summary>
        /// <param name="bsToAdd">The box score to add.</param>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstOpp">The opposing team stats dictionary.</param>
        /// <param name="id1">The away team's ID.</param>
        /// <param name="id2">The home team's ID.</param>
        public static void AddTeamStatsFromBoxScore(TeamBoxScore bsToAdd, ref Dictionary<int, TeamStats> tst,
                                                    ref Dictionary<int, TeamStats> tstOpp, int id1, int id2,
                                                    bool ignorePlayoffFlag = false)
        {
            TeamStats ts1 = tst[id1];
            TeamStats ts2 = tst[id2];
            TeamStats tsopp1 = tstOpp[id1];
            TeamStats tsopp2 = tstOpp[id2];
            if (!bsToAdd.IsPlayoff || ignorePlayoffFlag)
            {
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    ts1.Record[0]++;
                    ts2.Record[1]++;
                }
                else
                {
                    ts1.Record[1]++;
                    ts2.Record[0]++;
                }
                // Add minutes played
                ts1.Totals[TAbbr.MINS] += bsToAdd.MINS1;
                ts2.Totals[TAbbr.MINS] += bsToAdd.MINS2;

                // Add Points For
                ts1.Totals[TAbbr.PF] += bsToAdd.PTS1;
                ts2.Totals[TAbbr.PF] += bsToAdd.PTS2;

                // Add Points Against
                ts1.Totals[TAbbr.PA] += bsToAdd.PTS2;
                ts2.Totals[TAbbr.PA] += bsToAdd.PTS1;

                //
                ts1.Totals[TAbbr.FGM] += bsToAdd.FGM1;
                ts2.Totals[TAbbr.FGM] += bsToAdd.FGM2;

                ts1.Totals[TAbbr.FGA] += bsToAdd.FGA1;
                ts2.Totals[TAbbr.FGA] += bsToAdd.FGA2;

                //
                ts1.Totals[TAbbr.TPM] += bsToAdd.TPM1;
                ts2.Totals[TAbbr.TPM] += bsToAdd.TPM2;

                //
                ts1.Totals[TAbbr.TPA] += bsToAdd.TPA1;
                ts2.Totals[TAbbr.TPA] += bsToAdd.TPA2;

                //
                ts1.Totals[TAbbr.FTM] += bsToAdd.FTM1;
                ts2.Totals[TAbbr.FTM] += bsToAdd.FTM2;

                //
                ts1.Totals[TAbbr.FTA] += bsToAdd.FTA1;
                ts2.Totals[TAbbr.FTA] += bsToAdd.FTA2;

                //
                ts1.Totals[TAbbr.OREB] += bsToAdd.OREB1;
                ts2.Totals[TAbbr.OREB] += bsToAdd.OREB2;

                //
                ts1.Totals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                ts2.Totals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                ts1.Totals[TAbbr.STL] += bsToAdd.STL1;
                ts2.Totals[TAbbr.STL] += bsToAdd.STL2;

                //
                ts1.Totals[TAbbr.TOS] += bsToAdd.TOS1;
                ts2.Totals[TAbbr.TOS] += bsToAdd.TOS2;

                //
                ts1.Totals[TAbbr.BLK] += bsToAdd.BLK1;
                ts2.Totals[TAbbr.BLK] += bsToAdd.BLK2;

                //
                ts1.Totals[TAbbr.AST] += bsToAdd.AST1;
                ts2.Totals[TAbbr.AST] += bsToAdd.AST2;

                //
                ts1.Totals[TAbbr.FOUL] += bsToAdd.FOUL1;
                ts2.Totals[TAbbr.FOUL] += bsToAdd.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    tsopp2.Record[0]++;
                    tsopp1.Record[1]++;
                }
                else
                {
                    tsopp2.Record[1]++;
                    tsopp1.Record[0]++;
                }
                // Add minutes played
                tsopp2.Totals[TAbbr.MINS] += bsToAdd.MINS1;
                tsopp1.Totals[TAbbr.MINS] += bsToAdd.MINS2;

                // Add Points For
                tsopp2.Totals[TAbbr.PF] += bsToAdd.PTS1;
                tsopp1.Totals[TAbbr.PF] += bsToAdd.PTS2;

                // Add Points Against
                tsopp2.Totals[TAbbr.PA] += bsToAdd.PTS2;
                tsopp1.Totals[TAbbr.PA] += bsToAdd.PTS1;

                //
                tsopp2.Totals[TAbbr.FGM] += bsToAdd.FGM1;
                tsopp1.Totals[TAbbr.FGM] += bsToAdd.FGM2;

                tsopp2.Totals[TAbbr.FGA] += bsToAdd.FGA1;
                tsopp1.Totals[TAbbr.FGA] += bsToAdd.FGA2;

                //
                tsopp2.Totals[TAbbr.TPM] += bsToAdd.TPM1;
                tsopp1.Totals[TAbbr.TPM] += bsToAdd.TPM2;

                //
                tsopp2.Totals[TAbbr.TPA] += bsToAdd.TPA1;
                tsopp1.Totals[TAbbr.TPA] += bsToAdd.TPA2;

                //
                tsopp2.Totals[TAbbr.FTM] += bsToAdd.FTM1;
                tsopp1.Totals[TAbbr.FTM] += bsToAdd.FTM2;

                //
                tsopp2.Totals[TAbbr.FTA] += bsToAdd.FTA1;
                tsopp1.Totals[TAbbr.FTA] += bsToAdd.FTA2;

                //
                tsopp2.Totals[TAbbr.OREB] += bsToAdd.OREB1;
                tsopp1.Totals[TAbbr.OREB] += bsToAdd.OREB2;

                //
                tsopp2.Totals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                tsopp1.Totals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                tsopp2.Totals[TAbbr.STL] += bsToAdd.STL1;
                tsopp1.Totals[TAbbr.STL] += bsToAdd.STL2;

                //
                tsopp2.Totals[TAbbr.TOS] += bsToAdd.TOS1;
                tsopp1.Totals[TAbbr.TOS] += bsToAdd.TOS2;

                //
                tsopp2.Totals[TAbbr.BLK] += bsToAdd.BLK1;
                tsopp1.Totals[TAbbr.BLK] += bsToAdd.BLK2;

                //
                tsopp2.Totals[TAbbr.AST] += bsToAdd.AST1;
                tsopp1.Totals[TAbbr.AST] += bsToAdd.AST2;

                //
                tsopp2.Totals[TAbbr.FOUL] += bsToAdd.FOUL1;
                tsopp1.Totals[TAbbr.FOUL] += bsToAdd.FOUL2;
            }
            else
            {
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    ts1.PlRecord[0]++;
                    ts2.PlRecord[1]++;
                }
                else
                {
                    ts1.PlRecord[1]++;
                    ts2.PlRecord[0]++;
                }
                // Add minutes played
                ts1.PlTotals[TAbbr.MINS] += bsToAdd.MINS1;
                ts2.PlTotals[TAbbr.MINS] += bsToAdd.MINS2;

                // Add Points For
                ts1.PlTotals[TAbbr.PF] += bsToAdd.PTS1;
                ts2.PlTotals[TAbbr.PF] += bsToAdd.PTS2;

                // Add Points Against
                ts1.PlTotals[TAbbr.PA] += bsToAdd.PTS2;
                ts2.PlTotals[TAbbr.PA] += bsToAdd.PTS1;

                //
                ts1.PlTotals[TAbbr.FGM] += bsToAdd.FGM1;
                ts2.PlTotals[TAbbr.FGM] += bsToAdd.FGM2;

                ts1.PlTotals[TAbbr.FGA] += bsToAdd.FGA1;
                ts2.PlTotals[TAbbr.FGA] += bsToAdd.FGA2;

                //
                ts1.PlTotals[TAbbr.TPM] += bsToAdd.TPM1;
                ts2.PlTotals[TAbbr.TPM] += bsToAdd.TPM2;

                //
                ts1.PlTotals[TAbbr.TPA] += bsToAdd.TPA1;
                ts2.PlTotals[TAbbr.TPA] += bsToAdd.TPA2;

                //
                ts1.PlTotals[TAbbr.FTM] += bsToAdd.FTM1;
                ts2.PlTotals[TAbbr.FTM] += bsToAdd.FTM2;

                //
                ts1.PlTotals[TAbbr.FTA] += bsToAdd.FTA1;
                ts2.PlTotals[TAbbr.FTA] += bsToAdd.FTA2;

                //
                ts1.PlTotals[TAbbr.OREB] += bsToAdd.OREB1;
                ts2.PlTotals[TAbbr.OREB] += bsToAdd.OREB2;

                //
                ts1.PlTotals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                ts2.PlTotals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                ts1.PlTotals[TAbbr.STL] += bsToAdd.STL1;
                ts2.PlTotals[TAbbr.STL] += bsToAdd.STL2;

                //
                ts1.PlTotals[TAbbr.TOS] += bsToAdd.TOS1;
                ts2.PlTotals[TAbbr.TOS] += bsToAdd.TOS2;

                //
                ts1.PlTotals[TAbbr.BLK] += bsToAdd.BLK1;
                ts2.PlTotals[TAbbr.BLK] += bsToAdd.BLK2;

                //
                ts1.PlTotals[TAbbr.AST] += bsToAdd.AST1;
                ts2.PlTotals[TAbbr.AST] += bsToAdd.AST2;

                //
                ts1.PlTotals[TAbbr.FOUL] += bsToAdd.FOUL1;
                ts2.PlTotals[TAbbr.FOUL] += bsToAdd.FOUL2;


                // Opponents Team Stats
                // Add win & loss
                if (bsToAdd.PTS1 > bsToAdd.PTS2)
                {
                    tsopp2.PlRecord[0]++;
                    tsopp1.PlRecord[1]++;
                }
                else
                {
                    tsopp2.PlRecord[1]++;
                    tsopp1.PlRecord[0]++;
                }
                // Add minutes played
                tsopp2.PlTotals[TAbbr.MINS] += bsToAdd.MINS1;
                tsopp1.PlTotals[TAbbr.MINS] += bsToAdd.MINS2;

                // Add Points For
                tsopp2.PlTotals[TAbbr.PF] += bsToAdd.PTS1;
                tsopp1.PlTotals[TAbbr.PF] += bsToAdd.PTS2;

                // Add Points Against
                tsopp2.PlTotals[TAbbr.PA] += bsToAdd.PTS2;
                tsopp1.PlTotals[TAbbr.PA] += bsToAdd.PTS1;

                //
                tsopp2.PlTotals[TAbbr.FGM] += bsToAdd.FGM1;
                tsopp1.PlTotals[TAbbr.FGM] += bsToAdd.FGM2;

                tsopp2.PlTotals[TAbbr.FGA] += bsToAdd.FGA1;
                tsopp1.PlTotals[TAbbr.FGA] += bsToAdd.FGA2;

                //
                tsopp2.PlTotals[TAbbr.TPM] += bsToAdd.TPM1;
                tsopp1.PlTotals[TAbbr.TPM] += bsToAdd.TPM2;

                //
                tsopp2.PlTotals[TAbbr.TPA] += bsToAdd.TPA1;
                tsopp1.PlTotals[TAbbr.TPA] += bsToAdd.TPA2;

                //
                tsopp2.PlTotals[TAbbr.FTM] += bsToAdd.FTM1;
                tsopp1.PlTotals[TAbbr.FTM] += bsToAdd.FTM2;

                //
                tsopp2.PlTotals[TAbbr.FTA] += bsToAdd.FTA1;
                tsopp1.PlTotals[TAbbr.FTA] += bsToAdd.FTA2;

                //
                tsopp2.PlTotals[TAbbr.OREB] += bsToAdd.OREB1;
                tsopp1.PlTotals[TAbbr.OREB] += bsToAdd.OREB2;

                //
                tsopp2.PlTotals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB1 - bsToAdd.OREB1);
                tsopp1.PlTotals[TAbbr.DREB] += Convert.ToUInt16(bsToAdd.REB2 - bsToAdd.OREB2);

                //
                tsopp2.PlTotals[TAbbr.STL] += bsToAdd.STL1;
                tsopp1.PlTotals[TAbbr.STL] += bsToAdd.STL2;

                //
                tsopp2.PlTotals[TAbbr.TOS] += bsToAdd.TOS1;
                tsopp1.PlTotals[TAbbr.TOS] += bsToAdd.TOS2;

                //
                tsopp2.PlTotals[TAbbr.BLK] += bsToAdd.BLK1;
                tsopp1.PlTotals[TAbbr.BLK] += bsToAdd.BLK2;

                //
                tsopp2.PlTotals[TAbbr.AST] += bsToAdd.AST1;
                tsopp1.PlTotals[TAbbr.AST] += bsToAdd.AST2;

                //
                tsopp2.PlTotals[TAbbr.FOUL] += bsToAdd.FOUL1;
                tsopp1.PlTotals[TAbbr.FOUL] += bsToAdd.FOUL2;
            }

            ts1.CalcAvg();
            ts2.CalcAvg();
            tsopp1.CalcAvg();
            tsopp2.CalcAvg();

            tst[id1] = ts1;
            tst[id2] = ts2;
            tstOpp[id1] = tsopp1;
            tstOpp[id2] = tsopp2;
        }

        /// <summary>
        ///     Checks for teams in divisions that don't exist anymore, and reassings them to the first available division.
        /// </summary>
        public static void CheckForInvalidDivisions()
        {
            var db = new SQLiteDatabase(MainWindow.CurrentDB);
            var usedIDs = new List<int>();
            db.GetDataTable("SELECT ID FROM Divisions")
              .Rows.Cast<DataRow>()
              .ToList()
              .ForEach(row => usedIDs.Add(ParseCell.GetInt32(row, "ID")));

            var teamsChanged = new List<string>();

            int maxSeason = SQLiteIO.SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);
            for (int i = maxSeason; i >= 1; i--)
            {
                string teamsT = "Teams";
                string plTeamsT = "PlayoffTeams";
                string oppT = "Opponents";
                string plOppT = "PlayoffOpponents";
                if (i != maxSeason)
                {
                    string toAdd = "S" + i;
                    teamsT += toAdd;
                    plTeamsT += toAdd;
                    oppT += toAdd;
                    plOppT += toAdd;
                }

                var tables = new List<string> {teamsT, plTeamsT, oppT, plOppT};
                foreach (string table in tables)
                {
                    string q = "SELECT ID, Name, Division FROM " + table;
                    DataTable res = db.GetDataTable(q);

                    foreach (DataRow r in res.Rows)
                    {
                        if (usedIDs.Contains(ParseCell.GetInt32(r, "Division")) == false)
                        {
                            db.Update(table, new Dictionary<string, string> {{"Division", MainWindow.Divisions.First().ID.ToString()}},
                                      "ID = " + ParseCell.GetString(r, "ID"));
                            int teamid = MainWindow.TST.Values.Single(ts => ts.Name == ParseCell.GetString(r, "Name")).ID;
                            MainWindow.TST[teamid].Division = MainWindow.Divisions.First().ID;
                            if (teamsChanged.Contains(MainWindow.TST[teamid].DisplayName) == false)
                            {
                                teamsChanged.Add(MainWindow.TST[teamid].DisplayName);
                            }
                        }
                    }
                }
            }

            if (teamsChanged.Count > 0)
            {
                teamsChanged.Sort();
                string s = "Some teams were in divisions that were deleted and have been reset to the " +
                           MainWindow.Divisions.First().Name + " division.\n\n";
                teamsChanged.ForEach(s1 => s += s1 + "\n");
                s = s.TrimEnd(new[] {'\n'});
                SQLiteIO.SQLiteIO.SaveSeasonToDatabase();
                MessageBox.Show(s);
            }
        }

        /// <summary>
        ///     Adds one or more box scores resulting from an SQLite query to a TeamStats instance.
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
        ///     Adds a box score resulting from an SQLite query to a TeamStats instance.
        /// </summary>
        /// <param name="r">The result of the query containing the box score record.</param>
        /// <param name="ts">The TeamStats instance to be modified.</param>
        /// <param name="tsopp">The opposing TeamStats instance to be modified.</param>
        public static void AddToTeamStatsFromSQLBoxScore(DataRow r, ref TeamStats ts, ref TeamStats tsopp)
        {
            bool playoffs = ParseCell.GetBoolean(r, "isPlayoff");
            if (!playoffs)
            {
                int t1PTS = Convert.ToInt32(r["T1PTS"].ToString());
                int t2PTS = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.Name))
                {
                    if (t1PTS > t2PTS)
                    {
                        ts.Record[0]++;
                    }
                    else
                    {
                        ts.Record[1]++;
                    }
                    tsopp.Totals[TAbbr.MINS] = ts.Totals[TAbbr.MINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.Totals[TAbbr.PA] = ts.Totals[TAbbr.PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.Totals[TAbbr.PF] = ts.Totals[TAbbr.PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.Totals[TAbbr.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.Totals[TAbbr.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.Totals[TAbbr.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.Totals[TAbbr.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.Totals[TAbbr.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.Totals[TAbbr.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    ushort t1REB = Convert.ToUInt16(r["T1REB"].ToString());
                    ushort t1OREB = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.Totals[TAbbr.DREB] += (ushort) (t1REB - t1OREB);
                    ts.Totals[TAbbr.OREB] += t1OREB;

                    ts.Totals[TAbbr.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.Totals[TAbbr.TOS] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.Totals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T1BLK");
                    ts.Totals[TAbbr.AST] += ParseCell.GetUInt16(r, "T1AST");
                    ts.Totals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T1FOUL");

                    tsopp.Totals[TAbbr.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.Totals[TAbbr.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.Totals[TAbbr.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.Totals[TAbbr.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.Totals[TAbbr.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.Totals[TAbbr.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    ushort t2REB = Convert.ToUInt16(r["T2REB"].ToString());
                    ushort t2OREB = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.Totals[TAbbr.DREB] += (ushort) (t2REB - t2OREB);
                    tsopp.Totals[TAbbr.OREB] += t2OREB;

                    tsopp.Totals[TAbbr.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.Totals[TAbbr.TOS] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.Totals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T2BLK");
                    tsopp.Totals[TAbbr.AST] += ParseCell.GetUInt16(r, "T2AST");
                    tsopp.Totals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T2FOUL");
                }
                else
                {
                    if (t2PTS > t1PTS)
                    {
                        ts.Record[0]++;
                    }
                    else
                    {
                        ts.Record[1]++;
                    }
                    tsopp.Totals[TAbbr.MINS] = ts.Totals[TAbbr.MINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.Totals[TAbbr.PA] = ts.Totals[TAbbr.PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.Totals[TAbbr.PF] = ts.Totals[TAbbr.PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.Totals[TAbbr.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.Totals[TAbbr.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.Totals[TAbbr.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.Totals[TAbbr.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.Totals[TAbbr.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.Totals[TAbbr.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    ushort t2REB = Convert.ToUInt16(r["T2REB"].ToString());
                    ushort t2OREB = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.Totals[TAbbr.DREB] += (ushort) (t2REB - t2OREB);
                    ts.Totals[TAbbr.OREB] += t2OREB;

                    ts.Totals[TAbbr.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.Totals[TAbbr.TOS] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.Totals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T2BLK");
                    ts.Totals[TAbbr.AST] += ParseCell.GetUInt16(r, "T2AST");
                    ts.Totals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T2FOUL");

                    tsopp.Totals[TAbbr.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.Totals[TAbbr.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.Totals[TAbbr.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.Totals[TAbbr.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.Totals[TAbbr.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.Totals[TAbbr.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    ushort t1REB = Convert.ToUInt16(r["T1REB"].ToString());
                    ushort t1OREB = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.Totals[TAbbr.DREB] += (ushort) (t1REB - t1OREB);
                    tsopp.Totals[TAbbr.OREB] += t1OREB;

                    tsopp.Totals[TAbbr.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.Totals[TAbbr.TOS] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.Totals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T1BLK");
                    tsopp.Totals[TAbbr.AST] += ParseCell.GetUInt16(r, "T1AST");
                    tsopp.Totals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T1FOUL");
                }

                tsopp.Record[1] = ts.Record[0];
                tsopp.Record[0] = ts.Record[1];
            }
            else
            {
                int t1PTS = Convert.ToInt32(r["T1PTS"].ToString());
                int t2PTS = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.Name))
                {
                    if (t1PTS > t2PTS)
                    {
                        ts.PlRecord[0]++;
                    }
                    else
                    {
                        ts.PlRecord[1]++;
                    }
                    tsopp.PlTotals[TAbbr.MINS] = ts.PlTotals[TAbbr.MINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.PlTotals[TAbbr.PA] = ts.PlTotals[TAbbr.PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.PlTotals[TAbbr.PF] = ts.PlTotals[TAbbr.PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.PlTotals[TAbbr.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.PlTotals[TAbbr.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.PlTotals[TAbbr.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.PlTotals[TAbbr.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.PlTotals[TAbbr.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.PlTotals[TAbbr.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    ushort t1REB = Convert.ToUInt16(r["T1REB"].ToString());
                    ushort t1OREB = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.PlTotals[TAbbr.DREB] += (ushort) (t1REB - t1OREB);
                    ts.PlTotals[TAbbr.OREB] += t1OREB;

                    ts.PlTotals[TAbbr.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.PlTotals[TAbbr.TOS] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.PlTotals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T1BLK");
                    ts.PlTotals[TAbbr.AST] += ParseCell.GetUInt16(r, "T1AST");
                    ts.PlTotals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T1FOUL");

                    tsopp.PlTotals[TAbbr.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.PlTotals[TAbbr.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.PlTotals[TAbbr.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.PlTotals[TAbbr.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.PlTotals[TAbbr.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.PlTotals[TAbbr.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    ushort t2REB = Convert.ToUInt16(r["T2REB"].ToString());
                    ushort t2OREB = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.PlTotals[TAbbr.DREB] += (ushort) (t2REB - t2OREB);
                    tsopp.PlTotals[TAbbr.OREB] += t2OREB;

                    tsopp.PlTotals[TAbbr.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.PlTotals[TAbbr.TOS] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.PlTotals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T2BLK");
                    tsopp.PlTotals[TAbbr.AST] += ParseCell.GetUInt16(r, "T2AST");
                    tsopp.PlTotals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T2FOUL");
                }
                else
                {
                    if (t2PTS > t1PTS)
                    {
                        ts.PlRecord[0]++;
                    }
                    else
                    {
                        ts.PlRecord[1]++;
                    }
                    tsopp.PlTotals[TAbbr.MINS] = ts.PlTotals[TAbbr.MINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.PlTotals[TAbbr.PA] = ts.PlTotals[TAbbr.PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.PlTotals[TAbbr.PF] = ts.PlTotals[TAbbr.PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.PlTotals[TAbbr.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.PlTotals[TAbbr.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.PlTotals[TAbbr.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.PlTotals[TAbbr.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.PlTotals[TAbbr.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.PlTotals[TAbbr.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    ushort t2REB = Convert.ToUInt16(r["T2REB"].ToString());
                    ushort t2OREB = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.PlTotals[TAbbr.DREB] += (ushort) (t2REB - t2OREB);
                    ts.PlTotals[TAbbr.OREB] += t2OREB;

                    ts.PlTotals[TAbbr.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.PlTotals[TAbbr.TOS] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.PlTotals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T2BLK");
                    ts.PlTotals[TAbbr.AST] += ParseCell.GetUInt16(r, "T2AST");
                    ts.PlTotals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T2FOUL");

                    tsopp.PlTotals[TAbbr.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.PlTotals[TAbbr.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.PlTotals[TAbbr.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.PlTotals[TAbbr.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.PlTotals[TAbbr.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.PlTotals[TAbbr.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    ushort t1REB = Convert.ToUInt16(r["T1REB"].ToString());
                    ushort t1OREB = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.PlTotals[TAbbr.DREB] += (ushort) (t1REB - t1OREB);
                    tsopp.PlTotals[TAbbr.OREB] += t1OREB;

                    tsopp.PlTotals[TAbbr.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.PlTotals[TAbbr.TOS] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.PlTotals[TAbbr.BLK] += ParseCell.GetUInt16(r, "T1BLK");
                    tsopp.PlTotals[TAbbr.AST] += ParseCell.GetUInt16(r, "T1AST");
                    tsopp.PlTotals[TAbbr.FOUL] += ParseCell.GetUInt16(r, "T1FOUL");
                }

                tsopp.PlRecord[1] = ts.PlRecord[0];
                tsopp.PlRecord[0] = ts.PlRecord[1];
            }

            ts.CalcAvg();
            tsopp.CalcAvg();
        }
    }
}