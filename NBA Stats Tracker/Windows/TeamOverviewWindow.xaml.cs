#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.ListExtensions;
using NBA_Stats_Tracker.Helper.Miscellaneous;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Shows team information and stats.
    /// </summary>
    public partial class TeamOverviewWindow
    {
        private readonly string _teamToLoad;
        private List<TeamBoxScore> _bsrList = new List<TeamBoxScore>();
        private bool _changingOppRange;
        private bool _changingOppTeam;
        private bool _changingTimeframe;
        private int _curOpp;

        private int _curSeason = MainWindow.CurSeason;
        private int _curTeam;
        private TeamStats _curts;
        private TeamStats _curtsopp;
        private SQLiteDatabase _db = new SQLiteDatabase(MainWindow.CurrentDB);
        private DataTable _dtBS;
        private DataTable _dtHTH;
        private DataTable _dtOv;
        private DataTable _dtSs;
        private DataTable _dtYea;
        private DataView _dvHTH;
        private int _maxSeason = SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);
        private string _oppBestC = "";
        private string _oppBestF = "";
        private string _oppBestG = "";
        private ObservableCollection<PlayerStatsRow> _plPSRList;
        private TeamRankings _playoffRankings;
        private ObservableCollection<PlayerStatsRow> _psrList;
        private Dictionary<int, PlayerStats> _pst;
        private TeamRankings _seasonRankings;
        private string _teamBestC = "";
        private string _teamBestF = "";
        private string _teamBestG = "";
        private Dictionary<int, TeamStats> _tst;
        private Dictionary<int, TeamStats> _tstOpp;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamOverviewWindow" /> class.
        /// </summary>
        public TeamOverviewWindow()
        {
            InitializeComponent();

            Height = Misc.GetRegistrySetting("TeamOvHeight", (int) Height);
            Width = Misc.GetRegistrySetting("TeamOvWidth", (int) Width);
            Top = Misc.GetRegistrySetting("TeamOvY", (int) Top);
            Left = Misc.GetRegistrySetting("TeamOvX", (int) Left);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamOverviewWindow" /> class.
        /// </summary>
        /// <param name="team">The team to switch to when the window finishes loading.</param>
        public TeamOverviewWindow(string team) : this()
        {
            _teamToLoad = team;
        }

        protected ObservableCollection<PlayerHighsRow> recordsList { get; set; }

        /// <summary>
        ///     Populates the teams combo.
        /// </summary>
        private void populateTeamsCombo()
        {
            List<string> teams = (from kvp in MainWindow.TeamOrder
                                  where !_tst[kvp.Value].IsHidden
                                  select _tst[kvp.Value].DisplayName).ToList();

            teams.Sort();

            cmbTeam.ItemsSource = teams;
            cmbOppTeam.ItemsSource = teams;
            cmbOppTeamBest.ItemsSource = teams;
            cmbMPOppTeam.ItemsSource = teams;
        }

        /// <summary>
        ///     Populates the season combo.
        /// </summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            //cmbSeasonNum.SelectedValue = MainWindow.tf.SeasonNum;
        }

        /// <summary>
        ///     Handles the Click event of the btnPrev control.
        ///     Switches to the previous team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex <= 0)
                cmbTeam.SelectedIndex = cmbTeam.Items.Count - 1;
            else
                cmbTeam.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Click event of the btnNext control.
        ///     Switches to the next team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count - 1)
                cmbTeam.SelectedIndex = 0;
            else
                cmbTeam.SelectedIndex++;
        }

        private void updateScoutingReport()
        {
            int id;
            try
            {
                id = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
            }
            catch
            {
                return;
            }

            string msg = _tst[id].ScoutingReport(_tst, _psrList, MainWindow.SeasonTeamRankings);
            txbSeasonScoutingReport.Text = msg;

            List<string> facts = getFacts(id, MainWindow.SeasonTeamRankings);
            txbSeasonFacts.Text = facts.Aggregate("", (s1, s2) => s1 + "\n" + s2);

            if (_tst[id].GetPlayoffGames() > 0)
            {
                msg = _tst[id].ScoutingReport(_tst, _psrList, MainWindow.PlayoffTeamRankings, true);
                txbPlayoffsScoutingReport.Text = msg;

                facts = getFacts(id, MainWindow.PlayoffTeamRankings);
                txbPlayoffsFacts.Text = facts.Aggregate("", (s1, s2) => s1 + "\n" + s2);

                grpPlayoffsScoutingReport.Visibility = Visibility.Visible;
                grpPlayoffsFacts.Visibility = Visibility.Visible;
            }
            else
            {
                grpPlayoffsScoutingReport.Visibility = Visibility.Collapsed;
                grpPlayoffsFacts.Visibility = Visibility.Collapsed;
            }

            svScoutingReport.ScrollToTop();
        }

        private List<string> getFacts(int id, TeamRankings rankings)
        {
            int count = 0;
            var facts = new List<string>();
            int topThird = MainWindow.TST.Count/3;
            for (int i = 0; i < rankings.RankingsTotal[id].Length; i++)
            {
                if (i == 3)
                    continue;

                int rank = rankings.RankingsTotal[id][i];
                if (rank <= topThird)
                {
                    string fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), TAbbr.Totals[i]);
                    fact += String.Format("{0}", _tst[id].Totals[i]);
                    facts.Add(fact);
                    count++;
                }
            }
            for (int i = 0; i < rankings.RankingsPerGame[id].Length; i++)
            {
                int rank = rankings.RankingsPerGame[id][i];
                if (rank <= topThird)
                {
                    string fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), TAbbr.PerGame[i]);
                    if (TAbbr.PerGame[i].EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", _tst[id].PerGame[i]);
                    }
                    else if (TAbbr.PerGame[i].EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2}", _tst[id].PerGame[i]);
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", _tst[id].PerGame[i]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            for (int i = 0; i < rankings.RankingsMetrics[id].Keys.Count; i++)
            {
                string metricName = rankings.RankingsMetrics[id].Keys.ToList()[i];
                int rank = rankings.RankingsMetrics[id][metricName];
                if (rank <= topThird)
                {
                    string fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), metricName.Replace("p", "%"));
                    if (metricName.EndsWith("p") || metricName.EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", _tst[id].Metrics[metricName]);
                    }
                    else if (metricName.EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2}", _tst[id].Metrics[metricName]);
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", _tst[id].Metrics[metricName]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            facts.Sort(
                (f1, f2) =>
                Convert.ToInt32(f1.Substring(0, f1.IndexOfAny(new[] {'s', 'n', 'r', 't'})))
                       .CompareTo(Convert.ToInt32(f2.Substring(0, f2.IndexOfAny(new[] {'s', 'n', 'r', 't'})))));
            return facts;
        }

        /// <summary>
        ///     Updates the Overview tab and loads the appropriate box scores depending on the timeframe.
        /// </summary>
        private void updateOverviewAndBoxScores()
        {
            int id = _curTeam;

            _curts = _tst[id];
            _curtsopp = _tstOpp[id];

            _bsrList = new List<TeamBoxScore>();

            #region Prepare Team Overview

            IEnumerable<BoxScoreEntry> boxScoreEntries =
                MainWindow.BSHist.Where(bse => bse.BS.Team1ID == _curTeam || bse.BS.Team2ID == _curTeam);

            foreach (var r in boxScoreEntries)
            {
                TeamBoxScore bsr = r.BS.DeepClone();
                bsr.PrepareForDisplay(_tst, _curTeam);
                _bsrList.Add(bsr);
            }

            #region Regular Season

            DataRow dr = _dtOv.NewRow();

            dr["Type"] = "Stats";
            dr["Games"] = _curts.GetGames();
            dr["Wins (W%)"] = _curts.Record[0].ToString();
            dr["Losses (Weff)"] = _curts.Record[1].ToString();
            dr["PF"] = _curts.Totals[TAbbr.PF].ToString();
            dr["PA"] = _curts.Totals[TAbbr.PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = _curts.Totals[TAbbr.FGM].ToString() + "-" + _curts.Totals[TAbbr.FGA].ToString();
            dr["3PT"] = _curts.Totals[TAbbr.TPM].ToString() + "-" + _curts.Totals[TAbbr.TPA].ToString();
            dr["FT"] = _curts.Totals[TAbbr.FTM].ToString() + "-" + _curts.Totals[TAbbr.FTA].ToString();
            dr["REB"] = (_curts.Totals[TAbbr.DREB] + _curts.Totals[TAbbr.OREB]).ToString();
            dr["OREB"] = _curts.Totals[TAbbr.OREB].ToString();
            dr["DREB"] = _curts.Totals[TAbbr.DREB].ToString();
            dr["AST"] = _curts.Totals[TAbbr.AST].ToString();
            dr["TO"] = _curts.Totals[TAbbr.TOS].ToString();
            dr["STL"] = _curts.Totals[TAbbr.STL].ToString();
            dr["BLK"] = _curts.Totals[TAbbr.BLK].ToString();
            dr["FOUL"] = _curts.Totals[TAbbr.FOUL].ToString();
            dr["MINS"] = _curts.Totals[TAbbr.MINS].ToString();

            _dtOv.Rows.Add(dr);

            dr = _dtOv.NewRow();

            _curts.CalcAvg(); // Just to be sure...

            dr["Type"] = "Averages";
            //dr["Games"] = curts.getGames();
            dr["Wins (W%)"] = String.Format("{0:F3}", _curts.PerGame[TAbbr.Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", _curts.PerGame[TAbbr.Weff]);
            dr["PF"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.PPG]);
            dr["PA"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.PAPG]);
            dr["PD"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.PD]);
            dr["FG"] = String.Format("{0:F3}", _curts.PerGame[TAbbr.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", _curts.PerGame[TAbbr.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", _curts.PerGame[TAbbr.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", _curts.PerGame[TAbbr.TPeff]);
            dr["FT"] = String.Format("{0:F3}", _curts.PerGame[TAbbr.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", _curts.PerGame[TAbbr.FTeff]);
            dr["REB"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.RPG]);
            dr["OREB"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.DRPG]);
            dr["AST"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.APG]);
            dr["TO"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.TPG]);
            dr["STL"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.SPG]);
            dr["BLK"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", _curts.PerGame[TAbbr.FPG]);

            _dtOv.Rows.Add(dr);

            // Rankings can only be shown based on total stats
            // ...for now
            DataRow dr2;
            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Rankings";
            dr2["Wins (W%)"] = _seasonRankings.RankingsPerGame[id][TAbbr.Wp];
            dr2["Losses (Weff)"] = _seasonRankings.RankingsPerGame[id][TAbbr.Weff];
            dr2["PF"] = _seasonRankings.RankingsPerGame[id][TAbbr.PPG];
            dr2["PA"] = cmbTeam.Items.Count + 1 - _seasonRankings.RankingsPerGame[id][TAbbr.PAPG];
            dr2["PD"] = _seasonRankings.RankingsPerGame[id][TAbbr.PD];
            dr2["FG"] = _seasonRankings.RankingsPerGame[id][TAbbr.FGp];
            dr2["FGeff"] = _seasonRankings.RankingsPerGame[id][TAbbr.FGeff];
            dr2["3PT"] = _seasonRankings.RankingsPerGame[id][TAbbr.TPp];
            dr2["3Peff"] = _seasonRankings.RankingsPerGame[id][TAbbr.TPeff];
            dr2["FT"] = _seasonRankings.RankingsPerGame[id][TAbbr.FTp];
            dr2["FTeff"] = _seasonRankings.RankingsPerGame[id][TAbbr.FTeff];
            dr2["REB"] = _seasonRankings.RankingsPerGame[id][TAbbr.RPG];
            dr2["OREB"] = _seasonRankings.RankingsPerGame[id][TAbbr.ORPG];
            dr2["DREB"] = _seasonRankings.RankingsPerGame[id][TAbbr.DRPG];
            dr2["AST"] = _seasonRankings.RankingsPerGame[id][TAbbr.APG];
            dr2["TO"] = cmbTeam.Items.Count + 1 - _seasonRankings.RankingsPerGame[id][TAbbr.TPG];
            dr2["STL"] = _seasonRankings.RankingsPerGame[id][TAbbr.SPG];
            dr2["BLK"] = _seasonRankings.RankingsPerGame[id][TAbbr.BPG];
            dr2["FOUL"] = cmbTeam.Items.Count + 1 - _seasonRankings.RankingsPerGame[id][TAbbr.FPG];

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Stats";
            dr2["Games"] = _curtsopp.GetGames();
            dr2["Wins (W%)"] = _curtsopp.Record[0].ToString();
            dr2["Losses (Weff)"] = _curtsopp.Record[1].ToString();
            dr2["PF"] = _curtsopp.Totals[TAbbr.PF].ToString();
            dr2["PA"] = _curtsopp.Totals[TAbbr.PA].ToString();
            dr2["PD"] = " ";
            dr2["FG"] = _curtsopp.Totals[TAbbr.FGM].ToString() + "-" + _curtsopp.Totals[TAbbr.FGA].ToString();
            dr2["3PT"] = _curtsopp.Totals[TAbbr.TPM].ToString() + "-" + _curtsopp.Totals[TAbbr.TPA].ToString();
            dr2["FT"] = _curtsopp.Totals[TAbbr.FTM].ToString() + "-" + _curtsopp.Totals[TAbbr.FTA].ToString();
            dr2["REB"] = (_curtsopp.Totals[TAbbr.DREB] + _curtsopp.Totals[TAbbr.OREB]).ToString();
            dr2["OREB"] = _curtsopp.Totals[TAbbr.OREB].ToString();
            dr2["DREB"] = _curtsopp.Totals[TAbbr.DREB].ToString();
            dr2["AST"] = _curtsopp.Totals[TAbbr.AST].ToString();
            dr2["TO"] = _curtsopp.Totals[TAbbr.TOS].ToString();
            dr2["STL"] = _curtsopp.Totals[TAbbr.STL].ToString();
            dr2["BLK"] = _curtsopp.Totals[TAbbr.BLK].ToString();
            dr2["FOUL"] = _curtsopp.Totals[TAbbr.FOUL].ToString();
            dr2["MINS"] = _curtsopp.Totals[TAbbr.MINS].ToString();

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Avg";
            dr2["Wins (W%)"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbr.Wp]);
            dr2["Losses (Weff)"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbr.Weff]);
            dr2["PF"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.PPG]);
            dr2["PA"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.PAPG]);
            dr2["PD"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.PD]);
            dr2["FG"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbr.FGp]);
            dr2["FGeff"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbr.FGeff]);
            dr2["3PT"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbr.TPp]);
            dr2["3Peff"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbr.TPeff]);
            dr2["FT"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbr.FTp]);
            dr2["FTeff"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbr.FTeff]);
            dr2["REB"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.RPG]);
            dr2["OREB"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.ORPG]);
            dr2["DREB"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.DRPG]);
            dr2["AST"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.APG]);
            dr2["TO"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.TPG]);
            dr2["STL"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.SPG]);
            dr2["BLK"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.BPG]);
            dr2["FOUL"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbr.FPG]);

            _dtOv.Rows.Add(dr2);

            #endregion

            #region Playoffs

            _dtOv.Rows.Add(_dtOv.NewRow());

            dr = _dtOv.NewRow();

            dr["Type"] = "Playoffs";
            dr["Games"] = _curts.GetPlayoffGames();
            dr["Wins (W%)"] = _curts.PlRecord[0].ToString();
            dr["Losses (Weff)"] = _curts.PlRecord[1].ToString();
            dr["PF"] = _curts.PlTotals[TAbbr.PF].ToString();
            dr["PA"] = _curts.PlTotals[TAbbr.PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = _curts.PlTotals[TAbbr.FGM].ToString() + "-" + _curts.PlTotals[TAbbr.FGA].ToString();
            dr["3PT"] = _curts.PlTotals[TAbbr.TPM].ToString() + "-" + _curts.PlTotals[TAbbr.TPA].ToString();
            dr["FT"] = _curts.PlTotals[TAbbr.FTM].ToString() + "-" + _curts.PlTotals[TAbbr.FTA].ToString();
            dr["REB"] = (_curts.PlTotals[TAbbr.DREB] + _curts.PlTotals[TAbbr.OREB]).ToString();
            dr["OREB"] = _curts.PlTotals[TAbbr.OREB].ToString();
            dr["DREB"] = _curts.PlTotals[TAbbr.DREB].ToString();
            dr["AST"] = _curts.PlTotals[TAbbr.AST].ToString();
            dr["TO"] = _curts.PlTotals[TAbbr.TOS].ToString();
            dr["STL"] = _curts.PlTotals[TAbbr.STL].ToString();
            dr["BLK"] = _curts.PlTotals[TAbbr.BLK].ToString();
            dr["FOUL"] = _curts.PlTotals[TAbbr.FOUL].ToString();
            dr["MINS"] = _curts.PlTotals[TAbbr.MINS].ToString();

            _dtOv.Rows.Add(dr);

            dr = _dtOv.NewRow();

            dr["Type"] = "Pl Avg";
            dr["Wins (W%)"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbr.Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbr.Weff]);
            dr["PF"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.PPG]);
            dr["PA"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.PAPG]);
            dr["PD"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.PD]);
            dr["FG"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbr.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbr.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbr.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbr.TPeff]);
            dr["FT"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbr.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbr.FTeff]);
            dr["REB"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.RPG]);
            dr["OREB"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.DRPG]);
            dr["AST"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.APG]);
            dr["TO"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.TPG]);
            dr["STL"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.SPG]);
            dr["BLK"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbr.FPG]);

            _dtOv.Rows.Add(dr);

            dr2 = _dtOv.NewRow();

            int count = _tst.Count(z => z.Value.GetPlayoffGames() > 0);

            dr2["Type"] = "Pl Rank";
            dr2["Wins (W%)"] = _playoffRankings.RankingsPerGame[id][TAbbr.Wp];
            dr2["Losses (Weff)"] = _playoffRankings.RankingsPerGame[id][TAbbr.Weff];
            dr2["PF"] = _playoffRankings.RankingsPerGame[id][TAbbr.PPG];
            dr2["PA"] = count + 1 - _playoffRankings.RankingsPerGame[id][TAbbr.PAPG];
            dr2["PD"] = _playoffRankings.RankingsPerGame[id][TAbbr.PD];
            dr2["FG"] = _playoffRankings.RankingsPerGame[id][TAbbr.FGp];
            dr2["FGeff"] = _playoffRankings.RankingsPerGame[id][TAbbr.FGeff];
            dr2["3PT"] = _playoffRankings.RankingsPerGame[id][TAbbr.TPp];
            dr2["3Peff"] = _playoffRankings.RankingsPerGame[id][TAbbr.TPeff];
            dr2["FT"] = _playoffRankings.RankingsPerGame[id][TAbbr.FTp];
            dr2["FTeff"] = _playoffRankings.RankingsPerGame[id][TAbbr.FTeff];
            dr2["REB"] = _playoffRankings.RankingsPerGame[id][TAbbr.RPG];
            dr2["OREB"] = _playoffRankings.RankingsPerGame[id][TAbbr.ORPG];
            dr2["DREB"] = _playoffRankings.RankingsPerGame[id][TAbbr.DRPG];
            dr2["AST"] = _playoffRankings.RankingsPerGame[id][TAbbr.APG];
            dr2["TO"] = count + 1 - _playoffRankings.RankingsPerGame[id][TAbbr.TPG];
            dr2["STL"] = _playoffRankings.RankingsPerGame[id][TAbbr.SPG];
            dr2["BLK"] = _playoffRankings.RankingsPerGame[id][TAbbr.BPG];
            dr2["FOUL"] = count + 1 - _playoffRankings.RankingsPerGame[id][TAbbr.FPG];

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Pl Stats";
            dr2["Games"] = _curtsopp.GetPlayoffGames();
            dr2["Wins (W%)"] = _curtsopp.PlRecord[0].ToString();
            dr2["Losses (Weff)"] = _curtsopp.PlRecord[1].ToString();
            dr2["PF"] = _curtsopp.PlTotals[TAbbr.PF].ToString();
            dr2["PA"] = _curtsopp.PlTotals[TAbbr.PA].ToString();
            dr2["PD"] = " ";
            dr2["FG"] = _curtsopp.PlTotals[TAbbr.FGM].ToString() + "-" + _curtsopp.PlTotals[TAbbr.FGA].ToString();
            dr2["3PT"] = _curtsopp.PlTotals[TAbbr.TPM].ToString() + "-" + _curtsopp.PlTotals[TAbbr.TPA].ToString();
            dr2["FT"] = _curtsopp.PlTotals[TAbbr.FTM].ToString() + "-" + _curtsopp.PlTotals[TAbbr.FTA].ToString();
            dr2["REB"] = (_curtsopp.PlTotals[TAbbr.DREB] + _curtsopp.PlTotals[TAbbr.OREB]).ToString();
            dr2["OREB"] = _curtsopp.PlTotals[TAbbr.OREB].ToString();
            dr2["DREB"] = _curtsopp.PlTotals[TAbbr.DREB].ToString();
            dr2["AST"] = _curtsopp.PlTotals[TAbbr.AST].ToString();
            dr2["TO"] = _curtsopp.PlTotals[TAbbr.TOS].ToString();
            dr2["STL"] = _curtsopp.PlTotals[TAbbr.STL].ToString();
            dr2["BLK"] = _curtsopp.PlTotals[TAbbr.BLK].ToString();
            dr2["FOUL"] = _curtsopp.PlTotals[TAbbr.FOUL].ToString();
            dr2["MINS"] = _curtsopp.PlTotals[TAbbr.MINS].ToString();

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Pl Avg";
            dr2["Wins (W%)"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbr.Wp]);
            dr2["Losses (Weff)"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbr.Weff]);
            dr2["PF"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.PPG]);
            dr2["PA"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.PAPG]);
            dr2["PD"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.PD]);
            dr2["FG"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbr.FGp]);
            dr2["FGeff"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbr.FGeff]);
            dr2["3PT"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbr.TPp]);
            dr2["3Peff"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbr.TPeff]);
            dr2["FT"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbr.FTp]);
            dr2["FTeff"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbr.FTeff]);
            dr2["REB"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.RPG]);
            dr2["OREB"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.ORPG]);
            dr2["DREB"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.DRPG]);
            dr2["AST"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.APG]);
            dr2["TO"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.TPG]);
            dr2["STL"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.SPG]);
            dr2["BLK"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.BPG]);
            dr2["FOUL"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbr.FPG]);

            _dtOv.Rows.Add(dr2);

            #endregion

            createViewAndUpdateOverview();

            dgvBoxScores.ItemsSource = _bsrList;

            #endregion
        }

        /// <summary>
        ///     Creates a DataView based on the current overview DataTable and refreshes the DataGrid.
        /// </summary>
        private void createViewAndUpdateOverview()
        {
            var dvOv = new DataView(_dtOv) {AllowNew = false, AllowDelete = false};
            dgvTeamStats.DataContext = dvOv;
        }

        /// <summary>
        ///     Calculates the split stats and updates the split stats tab.
        /// </summary>
        private void updateSplitStats()
        {
            Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats = MainWindow.SplitTeamStats;
            int id = _curTeam;

            DataRow dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Home"], ref dr, "Home");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Away"], ref dr, "Away");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Wins"], ref dr, "Wins");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Losses"], ref dr, "Losses");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Season"], ref dr, "Season");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Playoffs"], ref dr, "Playoffs");
            _dtSs.Rows.Add(dr);

            #region Per Opponent

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            foreach (var oppTeam in MainWindow.TeamOrder.Values)
            {
                if (oppTeam == _curTeam)
                    continue;

                dr = _dtSs.NewRow();
                CreateDataRowFromTeamStats(splitTeamStats[id]["vs " + MainWindow.DisplayNames[oppTeam]], ref dr,
                                           "vs " + MainWindow.DisplayNames[oppTeam]);
                _dtSs.Rows.Add(dr);
            }

            #endregion

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["vs >= .500"], ref dr, "vs >= .500");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["vs < .500"], ref dr, "vs < .500");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Last 10"], ref dr, "Last 10");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[id]["Before"], ref dr, "Before");
            _dtSs.Rows.Add(dr);

            #region Monthly split stats

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            foreach (var sspair in splitTeamStats[id].Where(pair => pair.Key.StartsWith("M ")))
            {
                dr = _dtSs.NewRow();
                var label = new DateTime(Convert.ToInt32(sspair.Key.Substring(2, 4)), Convert.ToInt32(sspair.Key.Substring(7, 2)), 1);
                CreateDataRowFromTeamStats(sspair.Value, ref dr, label.Year.ToString() + " " + String.Format("{0:MMMM}", label));
                _dtSs.Rows.Add(dr);
            }

            #endregion

            // DataTable is done, create DataView and load into DataGrid
            var dvSs = new DataView(_dtSs) {AllowEdit = false, AllowNew = false};

            dgvSplit.DataContext = dvSs;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbTeam control.
        ///     Loads the information for the newly selected team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dgvBoxScores.DataContext = null;
                dgvTeamStats.DataContext = null;
                dgvHTHStats.DataContext = null;
                dgvHTHBoxScores.DataContext = null;
                dgvSplit.DataContext = null;
                dgvPlayerStats.ItemsSource = null;
                dgvMetricStats.ItemsSource = null;
                dgvTeamRoster.ItemsSource = null;

                if (cmbTeam.SelectedIndex == -1)
                    return;
                if (cmbSeasonNum.SelectedIndex == -1)
                    return;
            }
            catch
            {
                return;
            }

            new DataTable();

            //DataRow dr;

            _dtBS.Clear();
            _dtOv.Clear();
            _dtHTH.Clear();
            _dtSs.Clear();
            _dtYea.Clear();

            _curTeam = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());

            updateOverviewAndBoxScores();

            updateSplitStats();

            TeamStats ts = _tst[_curTeam];
            Title = cmbTeam.SelectedItem + " Team Overview - " + (ts.GetGames() + ts.GetPlayoffGames()) + " games played";

            updateHeadToHead();

            updateYearlyStats();

            updatePlayerAndMetricStats();

            updateBest();

            updateScoutingReport();

            updateRecords();
        }

        private void updateRecords()
        {
            recordsList = new ObservableCollection<PlayerHighsRow>();
            foreach (var psr in _psrList)
            {
                recordsList.Add(new PlayerHighsRow(psr.ID, psr.FirstName + " " + psr.LastName, MainWindow.PST[psr.ID].CareerHighs));
            }

            dgvHighs.ItemsSource = null;
            dgvHighs.ItemsSource = recordsList;
        }

        /// <summary>
        ///     Finds the tam's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private int getTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(_tst, displayName);
        }

        /// <summary>
        ///     Determines the team's best players and their most significant stats and updates the corresponding tab.
        /// </summary>
        private void updateBest()
        {
            txbPlayer1.Text = "";
            txbPlayer2.Text = "";
            txbPlayer3.Text = "";
            txbPlayer4.Text = "";
            txbPlayer5.Text = "";
            txbPlayer6.Text = "";

            PlayerStatsRow psr1;
            string text;
            try
            {
                List<PlayerStatsRow> templist = _psrList.ToList();
                /*
                if (double.IsNaN(templist[0].PER))
                {
                    templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                }
                else
                {
                    templist.Sort((pmsr1, pmsr2) => pmsr1.PER.CompareTo(pmsr2.PER));
                }
                */
                templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                templist.Reverse();

                psr1 = templist[0];
                text = psr1.GetBestStats(5);
                txbPlayer1.Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")" +
                                  (psr1.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(5);
                txbPlayer2.Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + ")" +
                                  (psr2.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(5);
                txbPlayer3.Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + ")" +
                                  (psr3.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(5);
                txbPlayer4.Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + ")" +
                                  (psr4.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(5);
                txbPlayer5.Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + ")" +
                                  (psr5.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(5);
                txbPlayer6.Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + ")" +
                                  (psr6.IsInjured ? " (Injured)" : "") + "\n\n" + text;
            }
            catch (Exception)
            {
            }

            calculateSituational(cmbSituational.SelectedItem.ToString());
        }

        /// <summary>
        ///     Determines the team's best starting five and their most significant stats.
        /// </summary>
        private void calculateSituational(string property)
        {
            txbStartingPG.Text = "";
            txbStartingSG.Text = "";
            txbStartingSF.Text = "";
            txbStartingPF.Text = "";
            txbStartingC.Text = "";

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            bool doBench = false;
            if (property == "Starters")
            {
                property = "GmSc";
            }
            else if (property == "Bench")
            {
                property = "GmSc";
                doBench = true;
            }
            else
            {
                property = property.Replace("%", "p").Replace("3", "T");
            }

            List<PlayerStatsRow> pgList =
                _psrList.Where(row => (row.Position1.ToString() == "PG" || row.Position2.ToString() == "PG") && row.IsInjured == false)
                        .ToList();
            pgList.Sort((pmsr1, pmsr2) => comparePSRs(property, pmsr1, pmsr2));
            pgList.Reverse();
            List<PlayerStatsRow> sgList =
                _psrList.Where(row => (row.Position1.ToString() == "SG" || row.Position2.ToString() == "SG") && row.IsInjured == false)
                        .ToList();
            sgList.Sort((pmsr1, pmsr2) => comparePSRs(property, pmsr1, pmsr2));
            sgList.Reverse();
            List<PlayerStatsRow> sfList =
                _psrList.Where(row => (row.Position1.ToString() == "SF" || row.Position2.ToString() == "SF") && row.IsInjured == false)
                        .ToList();
            sfList.Sort((pmsr1, pmsr2) => comparePSRs(property, pmsr1, pmsr2));
            sfList.Reverse();
            List<PlayerStatsRow> pfList =
                _psrList.Where(row => (row.Position1.ToString() == "PF" || row.Position2.ToString() == "PF") && row.IsInjured == false)
                        .ToList();
            pfList.Sort((pmsr1, pmsr2) => comparePSRs(property, pmsr1, pmsr2));
            pfList.Reverse();
            List<PlayerStatsRow> cList =
                _psrList.Where(row => (row.Position1.ToString() == "C" || row.Position2.ToString() == "C") && row.IsInjured == false)
                        .ToList();
            cList.Sort((pmsr1, pmsr2) => comparePSRs(property, pmsr1, pmsr2));
            cList.Reverse();
            var permutations = new List<StartingFivePermutation>();

            double max = Double.MinValue;
            foreach (var pg in pgList)
                foreach (var sg in sgList)
                    foreach (var sf in sfList)
                        foreach (var pf in pfList)
                            foreach (var c in cList)
                            {
                                double sum = 0;
                                int pInP = 0;
                                var perm = new List<int> {pg.ID};
                                sum += Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(pg, null));
                                if (pg.Position1.ToString() == "PG")
                                    pInP++;
                                if (perm.Contains(sg.ID))
                                {
                                    continue;
                                }
                                perm.Add(sg.ID);
                                sum += Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(sg, null));
                                if (sg.Position1.ToString() == "SG")
                                    pInP++;
                                if (perm.Contains(sf.ID))
                                {
                                    continue;
                                }
                                perm.Add(sf.ID);
                                sum += Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(sf, null));
                                if (sf.Position1.ToString() == "SF")
                                    pInP++;
                                if (perm.Contains(pf.ID))
                                {
                                    continue;
                                }
                                perm.Add(pf.ID);
                                sum += Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(pf, null));
                                if (pf.Position1.ToString() == "PF")
                                    pInP++;
                                if (perm.Contains(c.ID))
                                {
                                    continue;
                                }
                                perm.Add(c.ID);
                                sum += Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(c, null));
                                if (c.Position1.ToString() == "C")
                                    pInP++;

                                if (sum > max)
                                    max = sum;

                                permutations.Add(new StartingFivePermutation {IDList = perm, PlayersInPrimaryPosition = pInP, Sum = sum});
                            }

            try
            {
                StartingFivePermutation bestPerm =
                    permutations.Where(perm => perm.Sum.Equals(max)).OrderByDescending(perm => perm.PlayersInPrimaryPosition).First();
                if (!doBench)
                {
                    bestPerm.IDList.ForEach(i1 => tempList.Add(_psrList.Single(row => row.ID == i1)));
                }
                else
                {
                    List<StartingFivePermutation> benchPerms =
                        permutations.Where(perm => !(perm.IDList.Any(id => bestPerm.IDList.Contains(id)))).ToList();
                    if (benchPerms.Count == 0)
                    {
                        foreach (var perm in permutations)
                        {
                            foreach (var id in perm.IDList)
                            {
                                if (bestPerm.IDList.Contains(id))
                                {
                                    perm.BestPermCount++;
                                }
                            }
                        }
                        benchPerms = permutations;
                    }
                    StartingFivePermutation benchPerm =
                        benchPerms.OrderBy(perm => perm.BestPermCount)
                                  .ThenByDescending(perm => perm.Sum)
                                  .ThenByDescending(perm => perm.PlayersInPrimaryPosition)
                                  .First();
                    benchPerm.IDList.ForEach(i1 => tempList.Add(_psrList.Single(row => row.ID == i1)));
                }
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                psr1 = tempList[0];
                text = psr1.GetBestStats(5);
                txbStartingPG.Text = "PG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[1];
                text = psr1.GetBestStats(5);
                txbStartingSG.Text = "SG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[2];
                text = psr1.GetBestStats(5);
                txbStartingSF.Text = "SF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[3];
                text = psr1.GetBestStats(5);
                txbStartingPF.Text = "PF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[4];
                text = psr1.GetBestStats(5);
                txbStartingC.Text = "C: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }
        }

        private static int comparePSRs(string property, PlayerStatsRow pmsr1, PlayerStatsRow pmsr2)
        {
            return
                Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(pmsr1, null))
                       .CompareTo(Convert.ToDouble(typeof (PlayerStatsRow).GetProperty(property).GetValue(pmsr2, null)));
        }

        /// <summary>
        ///     Calculates the player and metric stats and updates the corresponding tabs.
        /// </summary>
        private void updatePlayerAndMetricStats()
        {
            _psrList = new ObservableCollection<PlayerStatsRow>();
            _plPSRList = new ObservableCollection<PlayerStatsRow>();

            IEnumerable<KeyValuePair<int, PlayerStats>> players = _pst.Where(pair => pair.Value.TeamF == _curTeam && !pair.Value.IsHidden);
            foreach (var pl in players)
            {
                _psrList.Add(new PlayerStatsRow(pl.Value, false));
                _plPSRList.Add(new PlayerStatsRow(pl.Value, true));
            }

            _psrList.Sort((row1, row2) => String.CompareOrdinal(row1.LastName, row2.LastName));
            _plPSRList.Sort((row1, row2) => String.CompareOrdinal(row1.LastName, row2.LastName));

            dgvPlayerStats.ItemsSource = _psrList;
            dgvMetricStats.ItemsSource = _psrList;
            dgvTeamRoster.ItemsSource = _psrList;
            dgvTeamRoster.CanUserAddRows = false;
            dgvPlayerPlayoffStats.ItemsSource = _plPSRList;
            dgvPlayoffMetricStats.ItemsSource = _plPSRList;
        }

        /// <summary>
        ///     Updates the head to head tab.
        /// </summary>
        private void updateHeadToHead()
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Calculates the yearly stats and updates the yearly stats tab.
        /// </summary>
        private void updateYearlyStats()
        {
            _dtYea.Clear();

            string currentDB = MainWindow.CurrentDB;
            _curSeason = MainWindow.CurSeason;
            _maxSeason = SQLiteIO.GetMaxSeason(currentDB);

            TeamStats ts = _tst[_curTeam];
            TeamStats tsopp;
            var tsAllSeasons = new TeamStats(-1, "All Seasons");
            var tsAllPlayoffs = new TeamStats(-1, "All Playoffs");
            var tsAll = new TeamStats(-1, "All Games");
            tsAllSeasons.AddTeamStats(ts, Span.Season);
            tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
            tsAll.AddTeamStats(ts, Span.SeasonAndPlayoffs);

            DataRow drcur = _dtYea.NewRow();
            DataRow drcurPl = _dtYea.NewRow();
            CreateDataRowFromTeamStats(ts, ref drcur, "Season " + MainWindow.GetSeasonName(_curSeason));

            bool playedInPlayoffs = false;
            if (ts.PlRecord[0] + ts.PlRecord[1] > 0)
            {
                CreateDataRowFromTeamStats(ts, ref drcurPl, "Playoffs " + MainWindow.GetSeasonName(_curSeason), true);
                playedInPlayoffs = true;
            }

            //
            string qr = string.Format(@"SELECT * FROM PastTeamStats WHERE TeamID = {0} ORDER BY ""SOrder""", ts.ID);
            DataTable dt = _db.GetDataTable(qr);
            foreach (DataRow dr in dt.Rows)
            {
                DataRow dr4 = _dtYea.NewRow();
                ts = new TeamStats();
                if (DataRowCellParsers.GetBoolean(dr, "isPlayoff"))
                {
                    SQLiteIO.GetTeamStatsFromDataRow(ref ts, dr, true);
                    CreateDataRowFromTeamStats(ts, ref dr4, "Playoffs " + DataRowCellParsers.GetString(dr, "SeasonName"), true);
                    tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
                    tsAll.AddTeamStats(ts, Span.Playoffs);
                }
                else
                {
                    SQLiteIO.GetTeamStatsFromDataRow(ref ts, dr, false);
                    CreateDataRowFromTeamStats(ts, ref dr4, "Season " + DataRowCellParsers.GetString(dr, "SeasonName"), false);
                    tsAllSeasons.AddTeamStats(ts, Span.Season);
                    tsAll.AddTeamStats(ts, Span.Season);
                }
                _dtYea.Rows.Add(dr4);
            }
            //

            for (int j = 1; j <= _maxSeason; j++)
            {
                if (j != _curSeason)
                {
                    SQLiteIO.GetTeamStatsFromDatabase(MainWindow.CurrentDB, _curTeam, j, out ts, out tsopp);
                    DataRow dr3 = _dtYea.NewRow();
                    DataRow dr3Pl = _dtYea.NewRow();
                    CreateDataRowFromTeamStats(ts, ref dr3, "Season " + MainWindow.GetSeasonName(j));

                    _dtYea.Rows.Add(dr3);
                    if (ts.PlRecord[0] + ts.PlRecord[1] > 0)
                    {
                        CreateDataRowFromTeamStats(ts, ref dr3Pl, "Playoffs " + MainWindow.GetSeasonName(j), true);
                        _dtYea.Rows.Add(dr3Pl);
                    }

                    tsAllSeasons.AddTeamStats(ts, Span.Season);
                    tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
                    tsAll.AddTeamStats(ts, Span.SeasonAndPlayoffs);
                }
                else
                {
                    _dtYea.Rows.Add(drcur);
                    if (playedInPlayoffs)
                        _dtYea.Rows.Add(drcurPl);
                }
            }

            _dtYea.Rows.Add(_dtYea.NewRow());

            drcur = _dtYea.NewRow();
            CreateDataRowFromTeamStats(tsAllSeasons, ref drcur, "All Seasons");
            _dtYea.Rows.Add(drcur);
            drcur = _dtYea.NewRow();
            CreateDataRowFromTeamStats(tsAllPlayoffs, ref drcur, "All Playoffs");
            _dtYea.Rows.Add(drcur);

            _dtYea.Rows.Add(_dtYea.NewRow());

            drcur = _dtYea.NewRow();
            CreateDataRowFromTeamStats(tsAll, ref drcur, "All Games");
            _dtYea.Rows.Add(drcur);

            var dvYea = new DataView(_dtYea) {AllowNew = false, AllowEdit = false};

            dgvYearly.DataContext = dvYea;
        }

        /// <summary>
        ///     Handles the Click event of the btnSaveCustomTeam control.
        ///     Saves the team's stats into the database.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores" +
                    ") or edit the box-scores themselves.", "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int id;
            try
            {
                id = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
            }
            catch (Exception)
            {
                return;
            }
            _tst[id].Record[0] = Convert.ToByte(myCell(0, 2));
            _tst[id].Record[1] = Convert.ToByte(myCell(0, 3));
            _tst[id].Totals[TAbbr.PF] = Convert.ToUInt16(myCell(0, 4));
            _tst[id].Totals[TAbbr.PA] = Convert.ToUInt16(myCell(0, 5));

            string[] parts = myCell(0, 7).Split('-');
            _tst[id].Totals[TAbbr.FGM] = Convert.ToUInt16(parts[0]);
            _tst[id].Totals[TAbbr.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 9).Split('-');
            _tst[id].Totals[TAbbr.TPM] = Convert.ToUInt16(parts[0]);
            _tst[id].Totals[TAbbr.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 11).Split('-');
            _tst[id].Totals[TAbbr.FTM] = Convert.ToUInt16(parts[0]);
            _tst[id].Totals[TAbbr.FTA] = Convert.ToUInt16(parts[1]);

            _tst[id].Totals[TAbbr.OREB] = Convert.ToUInt16(myCell(0, 14));
            _tst[id].Totals[TAbbr.DREB] = Convert.ToUInt16(myCell(0, 15));

            _tst[id].Totals[TAbbr.AST] = Convert.ToUInt16(myCell(0, 16));
            _tst[id].Totals[TAbbr.TOS] = Convert.ToUInt16(myCell(0, 17));
            _tst[id].Totals[TAbbr.STL] = Convert.ToUInt16(myCell(0, 18));
            _tst[id].Totals[TAbbr.BLK] = Convert.ToUInt16(myCell(0, 19));
            _tst[id].Totals[TAbbr.FOUL] = Convert.ToUInt16(myCell(0, 20));
            _tst[id].Totals[TAbbr.MINS] = Convert.ToUInt16(myCell(0, 21));

            _tst[id].PlRecord[0] = Convert.ToByte(myCell(6, 2));
            _tst[id].PlRecord[1] = Convert.ToByte(myCell(6, 3));
            _tst[id].PlTotals[TAbbr.PF] = Convert.ToUInt16(myCell(6, 4));
            _tst[id].PlTotals[TAbbr.PA] = Convert.ToUInt16(myCell(6, 5));

            parts = myCell(6, 7).Split('-');
            _tst[id].PlTotals[TAbbr.FGM] = Convert.ToUInt16(parts[0]);
            _tst[id].PlTotals[TAbbr.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(6, 9).Split('-');
            _tst[id].PlTotals[TAbbr.TPM] = Convert.ToUInt16(parts[0]);
            _tst[id].PlTotals[TAbbr.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(6, 11).Split('-');
            _tst[id].PlTotals[TAbbr.FTM] = Convert.ToUInt16(parts[0]);
            _tst[id].PlTotals[TAbbr.FTA] = Convert.ToUInt16(parts[1]);

            _tst[id].PlTotals[TAbbr.OREB] = Convert.ToUInt16(myCell(6, 14));
            _tst[id].PlTotals[TAbbr.DREB] = Convert.ToUInt16(myCell(6, 15));

            _tst[id].PlTotals[TAbbr.AST] = Convert.ToUInt16(myCell(6, 16));
            _tst[id].PlTotals[TAbbr.TOS] = Convert.ToUInt16(myCell(6, 17));
            _tst[id].PlTotals[TAbbr.STL] = Convert.ToUInt16(myCell(6, 18));
            _tst[id].PlTotals[TAbbr.BLK] = Convert.ToUInt16(myCell(6, 19));
            _tst[id].PlTotals[TAbbr.FOUL] = Convert.ToUInt16(myCell(6, 20));
            _tst[id].PlTotals[TAbbr.MINS] = Convert.ToUInt16(myCell(6, 21));

            _tst[id].CalcAvg();


            // Opponents
            _tstOpp[id].Record[0] = Convert.ToByte(myCell(3, 2));
            _tstOpp[id].Record[1] = Convert.ToByte(myCell(3, 3));
            _tstOpp[id].Totals[TAbbr.PF] = Convert.ToUInt16(myCell(3, 4));
            _tstOpp[id].Totals[TAbbr.PA] = Convert.ToUInt16(myCell(3, 5));

            parts = myCell(3, 7).Split('-');
            _tstOpp[id].Totals[TAbbr.FGM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].Totals[TAbbr.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 9).Split('-');
            _tstOpp[id].Totals[TAbbr.TPM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].Totals[TAbbr.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 11).Split('-');
            _tstOpp[id].Totals[TAbbr.FTM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].Totals[TAbbr.FTA] = Convert.ToUInt16(parts[1]);

            _tstOpp[id].Totals[TAbbr.OREB] = Convert.ToUInt16(myCell(3, 14));
            _tstOpp[id].Totals[TAbbr.DREB] = Convert.ToUInt16(myCell(3, 15));

            _tstOpp[id].Totals[TAbbr.AST] = Convert.ToUInt16(myCell(3, 16));
            _tstOpp[id].Totals[TAbbr.TOS] = Convert.ToUInt16(myCell(3, 17));
            _tstOpp[id].Totals[TAbbr.STL] = Convert.ToUInt16(myCell(3, 18));
            _tstOpp[id].Totals[TAbbr.BLK] = Convert.ToUInt16(myCell(3, 19));
            _tstOpp[id].Totals[TAbbr.FOUL] = Convert.ToUInt16(myCell(3, 20));
            _tstOpp[id].Totals[TAbbr.MINS] = Convert.ToUInt16(myCell(3, 21));

            _tstOpp[id].PlRecord[0] = Convert.ToByte(myCell(9, 2));
            _tstOpp[id].PlRecord[1] = Convert.ToByte(myCell(9, 3));
            _tstOpp[id].PlTotals[TAbbr.PF] = Convert.ToUInt16(myCell(9, 4));
            _tstOpp[id].PlTotals[TAbbr.PA] = Convert.ToUInt16(myCell(9, 5));

            parts = myCell(9, 7).Split('-');
            _tstOpp[id].PlTotals[TAbbr.FGM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].PlTotals[TAbbr.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(9, 9).Split('-');
            _tstOpp[id].PlTotals[TAbbr.TPM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].PlTotals[TAbbr.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(9, 11).Split('-');
            _tstOpp[id].PlTotals[TAbbr.FTM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].PlTotals[TAbbr.FTA] = Convert.ToUInt16(parts[1]);

            _tstOpp[id].PlTotals[TAbbr.OREB] = Convert.ToUInt16(myCell(9, 14));
            _tstOpp[id].PlTotals[TAbbr.DREB] = Convert.ToUInt16(myCell(9, 15));

            _tstOpp[id].PlTotals[TAbbr.AST] = Convert.ToUInt16(myCell(9, 16));
            _tstOpp[id].PlTotals[TAbbr.TOS] = Convert.ToUInt16(myCell(9, 17));
            _tstOpp[id].PlTotals[TAbbr.STL] = Convert.ToUInt16(myCell(9, 18));
            _tstOpp[id].PlTotals[TAbbr.BLK] = Convert.ToUInt16(myCell(9, 19));
            _tstOpp[id].PlTotals[TAbbr.FOUL] = Convert.ToUInt16(myCell(9, 20));
            _tstOpp[id].PlTotals[TAbbr.MINS] = Convert.ToUInt16(myCell(9, 21));

            _tstOpp[id].CalcAvg();

            Dictionary<int, PlayerStats> playersToUpdate = _psrList.Select(cur => new PlayerStats(cur)).ToDictionary(ps => ps.ID);
            List<int> playerIDs = playersToUpdate.Keys.ToList();
            foreach (var playerID in playerIDs)
            {
                playersToUpdate[playerID].UpdatePlayoffStats(_plPSRList.Single(plPSR => plPSR.ID == playerID));
                playersToUpdate[playerID].UpdateCareerHighs(recordsList.Single(r => r.PlayerID == playerID));
            }

            SQLiteIO.SaveSeasonToDatabase(MainWindow.CurrentDB, _tst, _tstOpp, playersToUpdate, _curSeason, _maxSeason, partialUpdate: true);
            SQLiteIO.LoadSeason(MainWindow.CurrentDB, _curSeason, doNotLoadBoxScores: true);
            linkInternalsToMainWindow();
            MainWindow.UpdateNotables();

            int temp = cmbTeam.SelectedIndex;
            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = temp;
        }

        /// <summary>
        ///     Gets the value of the specified cell from the dgvTeamStats DataGrid.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private string myCell(int row, int col)
        {
            return GetCellValue(dgvTeamStats, row, col);
        }

        /// <summary>
        ///     Gets the value of the specified cell from the specified DataGrid.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private string GetCellValue(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
                return dataRowView.Row.ItemArray[col].ToString();

            return null;
        }

        /// <summary>
        ///     Handles the Click event of the btnScoutingReport control.
        ///     Displays a well-formatted scouting report in natural language containing comments on the team's performance, strong and weak points.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnScoutingReport_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpEnd control.
        ///     Makes sure the starting date isn't after the ending date, and updates the team's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
                return;
            _changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            _changingTimeframe = false;
            MainWindow.UpdateAllData();
            linkInternalsToMainWindow();
            cmbTeam_SelectionChanged(sender, null);
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpStart control.
        ///     Makes sure the starting date isn't after the ending date, and updates the team's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
                return;
            _changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            _changingTimeframe = false;
            MainWindow.UpdateAllData();
            linkInternalsToMainWindow();
            cmbTeam_SelectionChanged(sender, null);
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsAllTime control.
        ///     Allows the user to display stats from the whole season.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Tf = new Timeframe(_curSeason);
            if (!_changingTimeframe)
            {
                MainWindow.UpdateAllData();
                linkInternalsToMainWindow();
                cmbTeam_SelectionChanged(sender, null);
            }
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsBetween control.
        ///     Allows the user to display stats between the specified timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            if (!_changingTimeframe)
            {
                MainWindow.UpdateAllData();
                linkInternalsToMainWindow();
                cmbTeam_SelectionChanged(sender, null);
            }
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvBoxScores control.
        ///     Allows the user to view a specific box score in the Box Score window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (TeamBoxScore) dgvBoxScores.SelectedItems[0];

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, row.ID);
                try
                {
                    bsw.ShowDialog();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Handles the Click event of the btnPrevOpp control.
        ///     Switches to the previous opposing team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevOpp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex <= 0)
                cmbOppTeam.SelectedIndex = cmbOppTeam.Items.Count - 1;
            else
                cmbOppTeam.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Click event of the btnNextOpp control.
        ///     Switches to the next opposing team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextOpp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == cmbOppTeam.Items.Count - 1)
                cmbOppTeam.SelectedIndex = 0;
            else
                cmbOppTeam.SelectedIndex++;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbOppTeam control.
        ///     Synchronizes the two opposing team combos, loads the stats of the selected opposing team, and updates the appropriate tabs.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbOppTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingOppTeam)
                return;

            if (Equals(sender, cmbOppTeam))
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeamBest.SelectedIndex = cmbOppTeam.SelectedIndex;
                    cmbMPOppTeam.SelectedIndex = cmbOppTeam.SelectedIndex;
                }
                catch
                {
                    _changingOppTeam = false;
                    return;
                }
            }
            else if (Equals(sender, cmbOppTeamBest))
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeam.SelectedIndex = cmbOppTeamBest.SelectedIndex;
                    cmbMPOppTeam.SelectedIndex = cmbOppTeamBest.SelectedIndex;
                }
                catch
                {
                    _changingOppTeam = false;
                    return;
                }
            }
            else if (Equals(sender, cmbMPOppTeam))
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeam.SelectedIndex = cmbMPOppTeam.SelectedIndex;
                    cmbOppTeamBest.SelectedIndex = cmbMPOppTeam.SelectedIndex;
                }
                catch
                {
                    _changingOppTeam = false;
                    return;
                }
            }
            else
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeamBest.SelectedIndex = cmbOppTeam.SelectedIndex;
                    cmbMPOppTeam.SelectedIndex = cmbOppTeam.SelectedIndex;
                }
                catch (Exception)
                {
                    _changingOppTeam = false;
                    return;
                }
            }

            if (cmbOppTeam.SelectedIndex == -1)
            {
                _changingOppTeam = false;
                return;
            }

            dgvHTHBoxScores.DataContext = null;
            dgvHTHStats.DataContext = null;

            try
            {
                txbTeam1.Text = ""; //Fires exception on InitializeComponent()
            }
            catch (Exception)
            {
                _changingOppTeam = false;
                return;
            }
            txbTeam2.Text = "";
            txbTeam3.Text = "";
            txbOpp1.Text = "";
            txbOpp2.Text = "";
            txbOpp3.Text = "";
            txbMPDesc.Text = "";
            txbMPDescHeader.Text = "";
            txbMPOpp.Text = "";
            txbMPOppHeader.Text = "";
            txbMPTeam.Text = "";
            txbMPTeamHeader.Text = "";

            if (cmbOppTeam.SelectedIndex == cmbTeam.SelectedIndex)
            {
                _changingOppTeam = false;
                return;
            }

            _curTeam = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
            _curOpp = getTeamIDFromDisplayName(cmbOppTeam.SelectedItem.ToString());

            List<PlayerStatsRow> teamPMSRList;
            List<PlayerStatsRow> oppPMSRList;
            prepareHeadToHeadTab(out teamPMSRList, out oppPMSRList);
            prepareHTHBestPerformersTab(teamPMSRList, oppPMSRList);
            prepareMatchupPreview(teamPMSRList, oppPMSRList);

            _changingOppTeam = false;
        }

        private void prepareMatchupPreview(List<PlayerStatsRow> teamPMSRList, List<PlayerStatsRow> oppPMSRList)
        {
            int iTeam = _curTeam;
            int iOpp = _curOpp;
            txbMPDescHeader.Text = "vs\n";
            txbMPTeamHeader.Text = string.Format("{0}\n{1}-{2}", _tst[_curTeam].DisplayName, _tst[iTeam].Record[0], _tst[iTeam].Record[1]);
            txbMPOppHeader.Text = string.Format("{0}\n{1}-{2}", _tst[_curOpp].DisplayName, _tst[iOpp].Record[0], _tst[iOpp].Record[1]);

            var tsr = new TeamStatsRow(_tst[iTeam]);
            var tsropp = new TeamStatsRow(_tst[iOpp]);
            string msgDesc = "";
            string msgTeam = "";
            string msgOpp = "";

            var used = new List<int>();
            var dict = new Dictionary<int, int>();
            for (int k = 0; k < _seasonRankings.RankingsPerGame[iTeam].Length; k++)
            {
                dict.Add(k, _seasonRankings.RankingsPerGame[iTeam][k]);
            }
            dict[TAbbr.FPG] = _tst.Count + 1 - dict[TAbbr.FPG];
            dict[TAbbr.TPG] = _tst.Count + 1 - dict[TAbbr.TPG];
            dict[TAbbr.PAPG] = _tst.Count + 1 - dict[TAbbr.PAPG];
            List<int> strengths = (from entry in dict
                                   orderby entry.Value ascending
                                   select entry.Key).ToList();
            int m = 0;
            int j = 2;
            while (true)
            {
                if (m == j)
                    break;
                bool def = false;
                switch (strengths[m])
                {
                    case TAbbr.APG:
                        msgDesc += "Assists";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.APG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.APG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.APG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.APG]);
                        break;
                    case TAbbr.BPG:
                        msgDesc += "Blocks";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.BPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.BPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.BPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.BPG]);
                        break;
                    case TAbbr.DRPG:
                        msgDesc += "Def. Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.DRPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.DRPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.DRPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.DRPG]);
                        break;
                    case TAbbr.FPG:
                        msgDesc += "Fouls";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.FPG, _tst.Count - _seasonRankings.RankingsPerGame[iTeam][TAbbr.FPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.FPG, _tst.Count - _seasonRankings.RankingsPerGame[iOpp][TAbbr.FPG]);
                        break;
                    case TAbbr.ORPG:
                        msgDesc += "Off. Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.ORPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.ORPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.ORPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.ORPG]);
                        break;
                    case TAbbr.PAPG:
                        msgDesc += "Points Against";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.PAPG, _tst.Count - _seasonRankings.RankingsPerGame[iTeam][TAbbr.PAPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.PAPG, _tst.Count - _seasonRankings.RankingsPerGame[iOpp][TAbbr.PAPG]);
                        break;
                    case TAbbr.PPG:
                        msgDesc += "Points For";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.PPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.PPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.PPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.PPG]);
                        break;
                    case TAbbr.RPG:
                        msgDesc += "Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.RPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.RPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.RPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.RPG]);
                        break;
                    case TAbbr.SPG:
                        msgDesc += "Steals";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.SPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.SPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.SPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.SPG]);
                        break;
                    case TAbbr.TPG:
                        msgDesc += "Turnovers";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.TPG, _tst.Count - _seasonRankings.RankingsPerGame[iTeam][TAbbr.TPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.TPG, _tst.Count - _seasonRankings.RankingsPerGame[iOpp][TAbbr.TPG]);
                        break;
                    case TAbbr.FGeff:
                        msgDesc += "Field Goals";
                        msgTeam += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsr.FGMPG, tsr.FGAPG, tsr.FGp,
                                                 _seasonRankings.RankingsPerGame[iTeam][TAbbr.FGeff]);
                        msgOpp += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsropp.FGMPG, tsropp.FGAPG, tsropp.FGp,
                                                _seasonRankings.RankingsPerGame[iOpp][TAbbr.FGeff]);
                        break;
                    case TAbbr.TPeff:
                        msgDesc += "3 Pointers";
                        msgTeam += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsr.TPMPG, tsr.TPAPG, tsr.TPp,
                                                 _seasonRankings.RankingsPerGame[iTeam][TAbbr.TPeff]);
                        msgOpp += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsropp.TPMPG, tsropp.TPAPG, tsropp.TPp,
                                                _seasonRankings.RankingsPerGame[iOpp][TAbbr.TPeff]);
                        break;
                    case TAbbr.FTeff:
                        msgDesc += "Free Throws";
                        msgTeam += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsr.FTMPG, tsr.FTAPG, tsr.FTp,
                                                 _seasonRankings.RankingsPerGame[iTeam][TAbbr.FTeff]);
                        msgOpp += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsropp.FTMPG, tsropp.FTAPG, tsropp.FTp,
                                                _seasonRankings.RankingsPerGame[iOpp][TAbbr.FTeff]);
                        break;
                    default:
                        j++;
                        def = true;
                        break;
                }
                if (!def)
                {
                    used.Add(strengths[m]);
                    msgDesc += "\n";
                    msgTeam += "\n";
                    msgOpp += "\n";
                }
                m++;
            }

            dict = new Dictionary<int, int>();
            for (int k = 0; k < _seasonRankings.RankingsPerGame[iOpp].Length; k++)
            {
                dict.Add(k, _seasonRankings.RankingsPerGame[iOpp][k]);
            }
            dict[TAbbr.FPG] = _tst.Count + 1 - dict[TAbbr.FPG];
            dict[TAbbr.TPG] = _tst.Count + 1 - dict[TAbbr.TPG];
            dict[TAbbr.PAPG] = _tst.Count + 1 - dict[TAbbr.PAPG];
            strengths = (from entry in dict
                         orderby entry.Value ascending
                         select entry.Key).ToList();
            m = 0;
            j = 2;
            while (true)
            {
                if (m == j)
                    break;
                if (used.Contains(strengths[m]))
                {
                    m++;
                    j++;
                    continue;
                }
                bool def = false;
                switch (strengths[m])
                {
                    case TAbbr.APG:
                        msgDesc += "Assists";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.APG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.APG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.APG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.APG]);
                        break;
                    case TAbbr.BPG:
                        msgDesc += "Blocks";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.BPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.BPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.BPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.BPG]);
                        break;
                    case TAbbr.DRPG:
                        msgDesc += "Def. Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.DRPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.DRPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.DRPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.DRPG]);
                        break;
                    case TAbbr.FPG:
                        msgDesc += "Fouls";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.FPG, _tst.Count - _seasonRankings.RankingsPerGame[iTeam][TAbbr.FPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.FPG, _tst.Count - _seasonRankings.RankingsPerGame[iOpp][TAbbr.FPG]);
                        break;
                    case TAbbr.ORPG:
                        msgDesc += "Off. Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.ORPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.ORPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.ORPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.ORPG]);
                        break;
                    case TAbbr.PAPG:
                        msgDesc += "Points Against";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.PAPG, _tst.Count - _seasonRankings.RankingsPerGame[iTeam][TAbbr.PAPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.PAPG, _tst.Count - _seasonRankings.RankingsPerGame[iOpp][TAbbr.PAPG]);
                        break;
                    case TAbbr.PPG:
                        msgDesc += "Points For";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.PPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.PPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.PPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.PPG]);
                        break;
                    case TAbbr.RPG:
                        msgDesc += "Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.RPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.RPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.RPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.RPG]);
                        break;
                    case TAbbr.SPG:
                        msgDesc += "Steals";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.SPG, _seasonRankings.RankingsPerGame[iTeam][TAbbr.SPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.SPG, _seasonRankings.RankingsPerGame[iOpp][TAbbr.SPG]);
                        break;
                    case TAbbr.TPG:
                        msgDesc += "Turnovers";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.TPG, _tst.Count - _seasonRankings.RankingsPerGame[iTeam][TAbbr.TPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.TPG, _tst.Count - _seasonRankings.RankingsPerGame[iOpp][TAbbr.TPG]);
                        break;
                    case TAbbr.FGeff:
                        msgDesc += "Field Goals";
                        msgTeam += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsr.FGMPG, tsr.FGAPG, tsr.FGp,
                                                 _seasonRankings.RankingsPerGame[iTeam][TAbbr.FGeff]);
                        msgOpp += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsropp.FGMPG, tsropp.FGAPG, tsropp.FGp,
                                                _seasonRankings.RankingsPerGame[iOpp][TAbbr.FGeff]);
                        break;
                    case TAbbr.TPeff:
                        msgDesc += "3 Pointers";
                        msgTeam += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsr.TPMPG, tsr.TPAPG, tsr.TPp,
                                                 _seasonRankings.RankingsPerGame[iTeam][TAbbr.TPeff]);
                        msgOpp += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsropp.TPMPG, tsropp.TPAPG, tsropp.TPp,
                                                _seasonRankings.RankingsPerGame[iOpp][TAbbr.TPeff]);
                        break;
                    case TAbbr.FTeff:
                        msgDesc += "Free Throws";
                        msgTeam += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsr.FTMPG, tsr.FTAPG, tsr.FTp,
                                                 _seasonRankings.RankingsPerGame[iTeam][TAbbr.FTeff]);
                        msgOpp += string.Format("{0:F1}-{1:F1} ({2:F3}) ({3})", tsropp.FTMPG, tsropp.FTAPG, tsropp.FTp,
                                                _seasonRankings.RankingsPerGame[iOpp][TAbbr.FTeff]);
                        break;
                    default:
                        j++;
                        def = true;
                        break;
                }
                if (!def)
                {
                    used.Add(strengths[m]);
                    msgDesc += "\n";
                    msgTeam += "\n";
                    msgOpp += "\n";
                }
                m++;
            }
            string[] descParts = msgDesc.Split('\n');
            string[] teamParts = msgTeam.Split('\n');
            string[] oppParts = msgOpp.Split('\n');
            string s0 = teamParts[0] + "\t" + descParts[0] + "\t" + oppParts[0];
            string s1 = teamParts[1] + "\t" + descParts[1] + "\t" + oppParts[1];
            string s2 = teamParts[2] + "\t" + descParts[2] + "\t" + oppParts[2];
            string s3 = teamParts[3] + "\t" + descParts[3] + "\t" + oppParts[3];
            var list = new List<string> {s0, s1, s2, s3};
            list.Shuffle();
            list.ForEach(item =>
                         {
                             string[] parts = item.Split('\t');
                             txbMPTeam.Text += parts[0] + "\n";
                             txbMPDesc.Text += parts[1] + "\n";
                             txbMPOpp.Text += parts[2] + "\n";
                         });

            txbMPDesc.Text += "\nG\n\n\nF\n\n\nC\n\n";
            txbMPTeam.Text += "\n" + _teamBestG + "\n" + _teamBestF + "\n" + _teamBestC;
            txbMPOpp.Text += "\n" + _oppBestG + "\n" + _oppBestF + "\n" + _oppBestC;
        }

        private void prepareHTHBestPerformersTab(List<PlayerStatsRow> teamPMSRList, List<PlayerStatsRow> oppPMSRList)
        {
            List<PlayerStatsRow> guards = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                             {
                                                                 if (psr.Position1.ToString().EndsWith("G"))
                                                                 {
                                                                     if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                                         return true;

                                                                     return (!psr.IsInjured);
                                                                 }
                                                                 return false;
                                                             }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            List<PlayerStatsRow> fors = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                           {
                                                               if (psr.Position1.ToString().EndsWith("F"))
                                                               {
                                                                   if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                                       return true;

                                                                   return (!psr.IsInjured);
                                                               }
                                                               return false;
                                                           }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            List<PlayerStatsRow> centers = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                              {
                                                                  if (psr.Position1.ToString().EndsWith("C"))
                                                                  {
                                                                      if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                                          return true;

                                                                      return (!psr.IsInjured);
                                                                  }
                                                                  return false;
                                                              }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                string text = guards[0].GetBestStats(5);
                txbTeam1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + (guards[0].IsInjured ? " (Injured)" : "") + "\n\n" +
                                text;
                string[] lines = text.Split('\n');
                _teamBestG = string.Format("{0} {1}\n({2}, {3}, {4})\n", guards[0].FirstName, guards[0].LastName, lines[0], lines[1],
                                           lines[2]);

                text = fors[0].GetBestStats(5);
                txbTeam2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + (fors[0].IsInjured ? " (Injured)" : "") + "\n\n" + text;
                lines = text.Split('\n');
                _teamBestF = string.Format("{0} {1}\n({2}, {3}, {4})\n", fors[0].FirstName, fors[0].LastName, lines[0], lines[1], lines[2]);

                text = centers[0].GetBestStats(5);
                txbTeam3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + (centers[0].IsInjured ? " (Injured)" : "") +
                                "\n\n" + text;
                lines = text.Split('\n');
                _teamBestC = string.Format("{0} {1}\n({2}, {3}, {4})\n", centers[0].FirstName, centers[0].LastName, lines[0], lines[1],
                                           lines[2]);
            }
            catch
            {
            }

            guards = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                       {
                                           if (psr.Position1.ToString().EndsWith("G"))
                                           {
                                               if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                   return true;

                                               return (!psr.IsInjured);
                                           }
                                           return false;
                                       }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            fors = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                     {
                                         if (psr.Position1.ToString().EndsWith("F"))
                                         {
                                             if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                 return true;

                                             return (!psr.IsInjured);
                                         }
                                         return false;
                                     }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            centers = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                        {
                                            if (psr.Position1.ToString().EndsWith("C"))
                                            {
                                                if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                    return true;

                                                return (!psr.IsInjured);
                                            }
                                            return false;
                                        }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                string text = guards[0].GetBestStats(5);
                txbTeam1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + (guards[0].IsInjured ? " (Injured)" : "") + "\n\n" +
                                text;
                string[] lines = text.Split('\n');
                _oppBestG = string.Format("{0} {1}\n({2}, {3}, {4})\n", guards[0].FirstName, guards[0].LastName, lines[0], lines[1],
                                          lines[2]);

                text = fors[0].GetBestStats(5);
                txbTeam2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + (fors[0].IsInjured ? " (Injured)" : "") + "\n\n" + text;
                lines = text.Split('\n');
                _oppBestF = string.Format("{0} {1}\n({2}, {3}, {4})\n", fors[0].FirstName, fors[0].LastName, lines[0], lines[1], lines[2]);

                text = centers[0].GetBestStats(5);
                txbTeam3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + (centers[0].IsInjured ? " (Injured)" : "") +
                                "\n\n" + text;
                lines = text.Split('\n');
                _oppBestC = string.Format("{0} {1}\n({2}, {3}, {4})\n", centers[0].FirstName, centers[0].LastName, lines[0], lines[1],
                                          lines[2]);
            }
            catch
            {
            }

            grpHTHBestOpp.Header = cmbOppTeamBest.SelectedItem;
            grpHTHBestTeam.Header = cmbTeam.SelectedItem;
        }

        private void prepareHeadToHeadTab(out List<PlayerStatsRow> teamPMSRList, out List<PlayerStatsRow> oppPMSRList)
        {
            int iown = _curTeam;
            int iopp = _curOpp;

            var dtHTHBS = new DataTable();
            dtHTHBS.Columns.Add("Date");
            dtHTHBS.Columns.Add("Home-Away");
            dtHTHBS.Columns.Add("Result");
            dtHTHBS.Columns.Add("Score");
            dtHTHBS.Columns.Add("GameID");

            var ts = new TeamStats(_curTeam);
            var tsopp = new TeamStats(_curOpp);

            _db = new SQLiteDatabase(MainWindow.CurrentDB);

            if (_dtHTH.Rows.Count > 1)
                _dtHTH.Rows.RemoveAt(_dtHTH.Rows.Count - 1);

            List<BoxScoreEntry> bsHist = MainWindow.BSHist;

            List<BoxScoreEntry> bseList =
                bsHist.Where(
                    bse =>
                    (bse.BS.Team1ID == _curTeam && bse.BS.Team2ID == _curOpp) || (bse.BS.Team1ID == _curOpp && bse.BS.Team2ID == _curTeam))
                      .ToList();

            if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
            {
                ts = _tst[iown];
                ts.CalcAvg();

                tsopp = _tst[iopp];
                tsopp.CalcAvg();
            }
            else
            {
                foreach (var bse in bseList)
                {
                    TeamStats.AddTeamStatsFromBoxScore(bse.BS, ref ts, ref tsopp, true);
                }
            }

            //ts.CalcMetrics(tsopp);
            //tsopp.CalcMetrics(ts);
            var ls = new TeamStats();
            ls.AddTeamStats(ts, Span.SeasonAndPlayoffs);
            ls.AddTeamStats(tsopp, Span.SeasonAndPlayoffs);
            List<int> keys = _pst.Keys.ToList();
            teamPMSRList = new List<PlayerStatsRow>();
            oppPMSRList = new List<PlayerStatsRow>();
            foreach (var key in keys)
            {
                if (_pst[key].TeamF == ts.ID)
                {
                    teamPMSRList.Add(new PlayerStatsRow(_pst[key]));
                }
                else if (_pst[key].TeamF == tsopp.ID)
                {
                    oppPMSRList.Add(new PlayerStatsRow(_pst[key]));
                }
            }

            foreach (var bse in bseList)
            {
                int t1PTS = bse.BS.PTS1;
                int t2PTS = bse.BS.PTS2;
                DataRow bsr = dtHTHBS.NewRow();
                bsr["Date"] = bse.BS.GameDate.ToString().Split(' ')[0];
                if (bse.BS.Team1ID.Equals(_curTeam))
                {
                    bsr["Home-Away"] = "Away";

                    if (t1PTS > t2PTS)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }
                }
                else
                {
                    bsr["Home-Away"] = "Home";

                    if (t2PTS > t1PTS)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }
                }

                bsr["Score"] = bse.BS.PTS1 + "-" + bse.BS.PTS2;
                bsr["GameID"] = bse.BS.ID.ToString();

                dtHTHBS.Rows.Add(bsr);
            }

            _dtHTH.Clear();

            DataRow dr = _dtHTH.NewRow();

            CreateDataRowFromTeamStats(ts, ref dr, "Averages");

            _dtHTH.Rows.Add(dr);

            dr = _dtHTH.NewRow();

            CreateDataRowFromTeamStats(tsopp, ref dr, "Opp Avg");

            _dtHTH.Rows.Add(dr);

            dr = _dtHTH.NewRow();

            CreateDataRowFromTeamStats(ts, ref dr, "Playoffs", true);

            _dtHTH.Rows.Add(dr);

            dr = _dtHTH.NewRow();

            CreateDataRowFromTeamStats(tsopp, ref dr, "Opp Pl Avg", true);

            _dtHTH.Rows.Add(dr);

            _dvHTH = new DataView(_dtHTH) {AllowNew = false, AllowEdit = false};

            dgvHTHStats.DataContext = _dvHTH;

            var dvHTHBS = new DataView(dtHTHBS) {AllowNew = false, AllowEdit = false};

            dgvHTHBoxScores.DataContext = dvHTHBS;
        }

        /// <summary>
        ///     Creates a data row from a TeamStats instance.
        /// </summary>
        /// <param name="ts">The TeamStats instance.</param>
        /// <param name="dr">The data row to be edited.</param>
        /// <param name="title">The title for the row's Type or Name column.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the row will present the team's playoff stats; otherwise, the regular season's.
        /// </param>
        public static void CreateDataRowFromTeamStats(TeamStats ts, ref DataRow dr, string title, bool playoffs = false)
        {
            try
            {
                dr["Type"] = title;
            }
            catch
            {
                dr["Name"] = title;
            }
            if (!playoffs)
            {
                dr["Games"] = ts.GetGames();
                dr["Wins"] = ts.Record[0].ToString();
                dr["Losses"] = ts.Record[1].ToString();
                dr["W%"] = String.Format("{0:F3}", ts.PerGame[TAbbr.Wp]);
                dr["Weff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.PerGame[TAbbr.PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.PerGame[TAbbr.PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.PerGame[TAbbr.PD]);
                dr["FG"] = String.Format("{0:F3}", ts.PerGame[TAbbr.FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.PerGame[TAbbr.TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.PerGame[TAbbr.FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.PerGame[TAbbr.RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.PerGame[TAbbr.ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.PerGame[TAbbr.DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.PerGame[TAbbr.APG]);
                dr["TO"] = String.Format("{0:F1}", ts.PerGame[TAbbr.TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.PerGame[TAbbr.SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.PerGame[TAbbr.BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.PerGame[TAbbr.FPG]);
                dr["MINS"] = String.Format("{0:F1}", ts.PerGame[TAbbr.MINS]);
            }
            else
            {
                dr["Games"] = ts.GetPlayoffGames();
                dr["Wins"] = ts.PlRecord[0].ToString();
                dr["Losses"] = ts.PlRecord[1].ToString();
                dr["W%"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.Wp]);
                dr["Weff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.PD]);
                dr["FG"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.APG]);
                dr["TO"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.FPG]);
                dr["MINS"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.MINS]);
            }
        }

        /// <summary>
        ///     Handles the Loaded event of the Window control.
        ///     Connects the team and player stats dictionaries to the Main window's, calculates team rankingsPerGame,
        ///     prepares the data tables and sets DataGrid parameters.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Prepare Data Tables

            _dtOv = new DataTable();

            _dtOv.Columns.Add("Type");
            _dtOv.Columns.Add("Games");
            _dtOv.Columns.Add("Wins (W%)");
            _dtOv.Columns.Add("Losses (Weff)");
            _dtOv.Columns.Add("PF");
            _dtOv.Columns.Add("PA");
            _dtOv.Columns.Add("PD");
            _dtOv.Columns.Add("FG");
            _dtOv.Columns.Add("FGeff");
            _dtOv.Columns.Add("3PT");
            _dtOv.Columns.Add("3Peff");
            _dtOv.Columns.Add("FT");
            _dtOv.Columns.Add("FTeff");
            _dtOv.Columns.Add("REB");
            _dtOv.Columns.Add("OREB");
            _dtOv.Columns.Add("DREB");
            _dtOv.Columns.Add("AST");
            _dtOv.Columns.Add("TO");
            _dtOv.Columns.Add("STL");
            _dtOv.Columns.Add("BLK");
            _dtOv.Columns.Add("FOUL");
            _dtOv.Columns.Add("MINS");

            _dtHTH = new DataTable();

            _dtHTH.Columns.Add("Type");
            _dtHTH.Columns.Add("Games");
            _dtHTH.Columns.Add("Wins");
            _dtHTH.Columns.Add("Losses");
            _dtHTH.Columns.Add("W%");
            _dtHTH.Columns.Add("Weff");
            _dtHTH.Columns.Add("PF");
            _dtHTH.Columns.Add("PA");
            _dtHTH.Columns.Add("PD");
            _dtHTH.Columns.Add("FG");
            _dtHTH.Columns.Add("FGeff");
            _dtHTH.Columns.Add("3PT");
            _dtHTH.Columns.Add("3Peff");
            _dtHTH.Columns.Add("FT");
            _dtHTH.Columns.Add("FTeff");
            _dtHTH.Columns.Add("REB");
            _dtHTH.Columns.Add("OREB");
            _dtHTH.Columns.Add("DREB");
            _dtHTH.Columns.Add("AST");
            _dtHTH.Columns.Add("TO");
            _dtHTH.Columns.Add("STL");
            _dtHTH.Columns.Add("BLK");
            _dtHTH.Columns.Add("FOUL");
            _dtHTH.Columns.Add("MINS");

            _dtSs = new DataTable();

            _dtSs.Columns.Add("Type");
            _dtSs.Columns.Add("Games");
            _dtSs.Columns.Add("Wins");
            _dtSs.Columns.Add("Losses");
            _dtSs.Columns.Add("W%");
            _dtSs.Columns.Add("Weff");
            _dtSs.Columns.Add("PF");
            _dtSs.Columns.Add("PA");
            _dtSs.Columns.Add("PD");
            _dtSs.Columns.Add("FG");
            _dtSs.Columns.Add("FGeff");
            _dtSs.Columns.Add("3PT");
            _dtSs.Columns.Add("3Peff");
            _dtSs.Columns.Add("FT");
            _dtSs.Columns.Add("FTeff");
            _dtSs.Columns.Add("REB");
            _dtSs.Columns.Add("OREB");
            _dtSs.Columns.Add("DREB");
            _dtSs.Columns.Add("AST");
            _dtSs.Columns.Add("TO");
            _dtSs.Columns.Add("STL");
            _dtSs.Columns.Add("BLK");
            _dtSs.Columns.Add("FOUL");
            _dtSs.Columns.Add("MINS");

            _dtYea = new DataTable();

            _dtYea.Columns.Add("Type");
            _dtYea.Columns.Add("Games");
            _dtYea.Columns.Add("Wins");
            _dtYea.Columns.Add("Losses");
            _dtYea.Columns.Add("W%");
            _dtYea.Columns.Add("Weff");
            _dtYea.Columns.Add("PF");
            _dtYea.Columns.Add("PA");
            _dtYea.Columns.Add("PD");
            _dtYea.Columns.Add("FG");
            _dtYea.Columns.Add("FGeff");
            _dtYea.Columns.Add("3PT");
            _dtYea.Columns.Add("3Peff");
            _dtYea.Columns.Add("FT");
            _dtYea.Columns.Add("FTeff");
            _dtYea.Columns.Add("REB");
            _dtYea.Columns.Add("OREB");
            _dtYea.Columns.Add("DREB");
            _dtYea.Columns.Add("AST");
            _dtYea.Columns.Add("TO");
            _dtYea.Columns.Add("STL");
            _dtYea.Columns.Add("BLK");
            _dtYea.Columns.Add("FOUL");
            _dtYea.Columns.Add("MINS");

            _dtBS = new DataTable();
            _dtBS.Columns.Add("Date", typeof (DateTime));
            _dtBS.Columns.Add("Opponent");
            _dtBS.Columns.Add("Home-Away");
            _dtBS.Columns.Add("Result");
            _dtBS.Columns.Add("Score");
            _dtBS.Columns.Add("GameID");

            #endregion

            linkInternalsToMainWindow();

            populateTeamsCombo();

            _changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.Tf.EndDate;
            dtpStart.SelectedDate = MainWindow.Tf.StartDate;
            populateSeasonCombo();
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.Tf.SeasonNum);
            if (MainWindow.Tf.IsBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            _changingTimeframe = false;

            populateSituationalsCombo();

            //cmbSeasonNum.SelectedIndex = MainWindow.mwInstance.cmbSeasonNum.SelectedIndex;

            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamRoster.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvSplit.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvYearly.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            cmbTeam.SelectedIndex = -1;
            if (!String.IsNullOrWhiteSpace(_teamToLoad))
                cmbTeam.SelectedItem = _teamToLoad;

            cmbOppTeam.SelectedIndex = -1;
            //Following line commented out to allow for faster loading of Team Overview
            //cmbOppTeam.SelectedIndex = 1;

            /*
            try
            {
                imgLogo.Source = Misc.LoadImage(MainWindow.imageDict[GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString())]);
            }
            catch (Exception)
            { }
            */
        }

        private void populateSituationalsCombo()
        {
            var situationals = new List<string> {"Starters", "Bench"};
            var temp = new List<string>();
            temp.AddRange(PAbbr.MetricsNames);
            temp.AddRange(PAbbr.PerGame.Values);
            temp.AddRange(PAbbr.Totals.Values);
            List<string> psrProps = typeof (PlayerStatsRow).GetProperties().Select(prop => prop.Name).ToList();
            situationals.AddRange(from t in temp
                                  let realName = t.Replace("%", "p").Replace("3", "T")
                                  where psrProps.Contains(realName)
                                  select t);
            cmbSituational.ItemsSource = situationals;
            cmbSituational.SelectedIndex = 0;
        }

        private void linkInternalsToMainWindow()
        {
            _tst = MainWindow.TST;
            _tstOpp = MainWindow.TSTOpp;
            _pst = MainWindow.PST;

            _seasonRankings = MainWindow.SeasonTeamRankings;
            _playoffRankings = MainWindow.PlayoffTeamRankings;
        }

        /// <summary>
        ///     Handles the Checked event of the rbHTHStatsAnyone control.
        ///     Used to include all the teams' games in the stat calculations, no matter the opponent.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            if (_changingOppRange)
                return;

            if (Equals(sender, rbHTHStatsAnyone))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsAnyoneBest.IsChecked = rbHTHStatsAnyone.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else if (Equals(sender, rbHTHStatsAnyoneBest))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsAnyone.IsChecked = rbHTHStatsAnyoneBest.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsAnyoneBest.IsChecked = rbHTHStatsAnyone.IsChecked;
                }
                catch (Exception)
                {
                    _changingOppRange = false;
                    return;
                }
            }
            cmbOppTeam_SelectionChanged(sender, null);
            _changingOppRange = false;
        }

        /// <summary>
        ///     Handles the Checked event of the rbHTHStatsEachOther control.
        ///     Used to include only stats from the games these two teams have played against each other.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            if (_changingOppRange)
                return;

            if (Equals(sender, rbHTHStatsEachOther))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsEachOtherBest.IsChecked = rbHTHStatsEachOther.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else if (Equals(sender, rbHTHStatsEachOtherBest))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsEachOther.IsChecked = rbHTHStatsEachOtherBest.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsEachOtherBest.IsChecked = rbHTHStatsEachOther.IsChecked;
                }
                catch (Exception)
                {
                    _changingOppRange = false;
                    return;
                }
            }
            cmbOppTeam_SelectionChanged(sender, null);
            _changingOppRange = false;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control.
        ///     Loads the team and player stats and information for the new season, repopulates the teams combo and tries to switch to the same team again.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                rbStatsAllTime.IsChecked = true;
                if (cmbSeasonNum.SelectedIndex == -1)
                {
                    _changingTimeframe = false;
                    return;
                }

                _curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;
                if (_curSeason == MainWindow.CurSeason && !MainWindow.Tf.IsBetween)
                {
                    _changingTimeframe = false;
                    return;
                }

                MainWindow.Tf = new Timeframe(_curSeason);
                MainWindow.ChangeSeason(_curSeason);
                SQLiteIO.LoadSeason(_curSeason);

                linkInternalsToMainWindow();
                populateTeamsCombo();

                try
                {
                    cmbTeam.SelectedIndex = -1;
                    cmbTeam.SelectedItem = _tst[_curTeam].DisplayName;
                }
                catch
                {
                    cmbTeam.SelectedIndex = -1;
                }
                _changingTimeframe = false;
            }
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the AnyPlayerDataGrid control.
        ///     Views the selected player in the Player Overview window, and reloads their team's stats aftewrards.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void anyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EventHandlers.AnyPlayerDataGrid_MouseDoubleClick(sender, e))
            {
                int curIndex = cmbTeam.SelectedIndex;
                cmbTeam.SelectedIndex = -1;
                cmbTeam.SelectedIndex = curIndex;
            }
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvHTHBoxScores control.
        ///     Views the selected box score in the Box Score window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvHTHBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvHTHBoxScores.SelectedItems[0];
                int gameid = Convert.ToInt32(row["GameID"].ToString());

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameid);
                try
                {
                    bsw.ShowDialog();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Handles the Closing event of the Window control.
        ///     Updates the Main window's team & player stats dictionaries to match the ones in the Team Overview window before closing.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="CancelEventArgs" /> instance containing the event data.
        /// </param>
        private void window_Closing(object sender, CancelEventArgs e)
        {
            MainWindow.TST = _tst;
            MainWindow.TSTOpp = _tstOpp;
            MainWindow.PST = _pst;

            Misc.SetRegistrySetting("TeamOvHeight", Height);
            Misc.SetRegistrySetting("TeamOvWidth", Width);
            Misc.SetRegistrySetting("TeamOvX", Left);
            Misc.SetRegistrySetting("TeamOvY", Top);
        }

        /// <summary>
        ///     Handles the Click event of the btnChangeName control.
        ///     Allows the user to update the team's displayName for the current season.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnChangeName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ibw = new InputBoxWindow("Please enter the new name for the team", _tst[_curTeam].DisplayName);
                ibw.ShowDialog();
            }
            catch
            {
                return;
            }

            string newname = MainWindow.Input;
            var dict = new Dictionary<string, string> {{"DisplayName", newname}};
            _db.Update(MainWindow.TeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlTeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.OppT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlOppT, dict, "Name LIKE \"" + _curTeam + "\"");

            int teamid = _curTeam;
            _tst[teamid].DisplayName = newname;
            _tstOpp[teamid].DisplayName = newname;

            MainWindow.TST = _tst;
            MainWindow.TSTOpp = _tstOpp;

            populateTeamsCombo();

            cmbTeam.SelectedItem = newname;
        }

        /// <summary>
        ///     Handles the Sorting event of the StatColumn control.
        ///     Uses a custom Sorting event handler that sorts a stat in descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void statColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        /// <summary>
        ///     Handles the PreviewKeyDown event of the dgvTeamStats control.
        ///     Allows the user to paste and import tab-separated values formatted team stats into the current team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvTeamStats_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                foreach (var dict in dictList)
                {
                    string type = "Stats";
                    try
                    {
                        type = dict["Type"];
                    }
                    catch (Exception)
                    {
                    }
                    switch (type)
                    {
                        case "Stats":
                            tryChangeRow(0, dict);
                            break;

                        case "Playoffs":
                            tryChangeRow(6, dict);
                            break;

                        case "Opp Stats":
                            tryChangeRow(3, dict);
                            break;

                        case "Opp Pl Stats":
                            tryChangeRow(9, dict);
                            break;
                    }
                }

                createViewAndUpdateOverview();

                //btnSaveCustomTeam_Click(null, null);
            }
        }

        /// <summary>
        ///     Tries to parse the data in the dictionary and change the values of the specified Overview row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="dict">The dict.</param>
        private void tryChangeRow(int row, Dictionary<string, string> dict)
        {
            _dtOv.Rows[row].TryChangeValue(dict, "Games", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "Wins (W%)", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "Losses (Weff)", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "PF", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "PA", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "FG", typeof (UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "3PT", typeof (UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "FT", typeof (UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "REB", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "OREB", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "DREB", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "AST", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "TO", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "STL", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "BLK", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "FOUL", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "MINS", typeof (UInt16));
        }

        /// <summary>
        ///     Allows the user to paste and import multiple player stats into the team's players.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void anyPlayerDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                ObservableCollection<PlayerStatsRow> list = Equals(sender, dgvPlayerStats) ? _psrList : _plPSRList;
                for (int j = 0; j < dictList.Count; j++)
                {
                    Dictionary<string, string> dict = dictList[j];
                    int id;
                    try
                    {
                        id = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            id =
                                _pst.Values.Single(
                                    ps => ps.TeamF == _curTeam && ps.LastName == dict["Last Name"] && ps.FirstName == dict["First Name"]).ID;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Player in row " + (j + 1) +
                                            " couldn't be determined either by ID or Full Name. Make sure the pasted data has the proper headers. " +
                                            "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        PlayerStatsRow psr = list.Single(ps => ps.ID == id);
                        PlayerStatsRow.TryChangePSR(ref psr, dict);
                        PlayerStatsRow.Refresh(ref psr);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                ((DataGrid) sender).ItemsSource = null;
                ((DataGrid) sender).ItemsSource = list;

                MessageBox.Show(
                    "Data pasted successfully! Remember to save!\n\nNote that metric and other stats may appear incorrect until you save.",
                    "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Handles the Click event of the chkHTHHideInjured control.
        ///     Used to ignore injured players while doing Head-To-Head Best Performers analysis.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void chkHTHHideInjured_Click(object sender, RoutedEventArgs e)
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Click event of the btnChangeDivision control.
        ///     Allows the user to change the team's division.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnChangeDivision_Click(object sender, RoutedEventArgs e)
        {
            int teamid = _curTeam;
            int i = MainWindow.Divisions.TakeWhile(div => _tst[teamid].Division != div.ID).Count();
            var ccw = new ComboChoiceWindow(ComboChoiceWindow.Mode.Division, i);
            ccw.ShowDialog();

            string[] parts = MainWindow.Input.Split(new[] {": "}, 2, StringSplitOptions.None);
            Division myDiv = MainWindow.Divisions.Find(division => division.Name == parts[1]);

            _tst[teamid].Division = myDiv.ID;
            _tstOpp[teamid].Division = myDiv.ID;

            var dict = new Dictionary<string, string>
                       {
                           {"Division", _tst[teamid].Division.ToString()},
                           {"Conference", _tst[teamid].Conference.ToString()}
                       };
            _db.Update(MainWindow.TeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlTeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.OppT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlOppT, dict, "Name LIKE \"" + _curTeam + "\"");

            MainWindow.TST = _tst;
            MainWindow.TSTOpp = _tstOpp;
        }

        /// <summary>
        ///     Handles the Sorting event of the dgvBoxScores control.
        ///     Uses a custom Sorting event handler that sorts dates or a stat in descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvBoxScores_Sorting(object sender, DataGridSortingEventArgs e)
        {
            statColumn_Sorting(sender, e);
        }

        private void btnAddPastStats_Click(object sender, RoutedEventArgs e)
        {
            var adw = new AddStatsWindow(true, _curTeam);
            if (adw.ShowDialog() == true)
            {
                updateYearlyStats();
            }
        }

        private void btnCopySeasonScouting_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonScoutingReport.Text);
        }

        private void btnCopyPlayoffsScouting_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsScoutingReport.Text);
        }

        private void btnCopySeasonFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonFacts.Text);
        }

        private void btnCopyPlayoffsFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsFacts.Text);
        }

        private void cmbSituational_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == -1 || cmbSituational.SelectedIndex == -1)
                return;

            calculateSituational(cmbSituational.SelectedItem.ToString());
        }

        private void btnCopyMPToClipboard_Click(object sender, RoutedEventArgs e)
        {
            string all = "";
            string[] descHeader = txbMPDescHeader.Text.Split('\n');
            string[] teamHeader = txbMPTeamHeader.Text.Split('\n');
            string[] oppHeader = txbMPOppHeader.Text.Split('\n');
            for (int i = 0; i < descHeader.Length; i++)
            {
                all += String.Format("{0}\t{1}\t{2}\n", teamHeader[i], descHeader[i], oppHeader[i]);
            }
            all += "\n";
            string[] descMsg = txbMPDesc.Text.Split('\n');
            string[] teamMsg = txbMPTeam.Text.Split('\n');
            string[] oppMsg = txbMPOpp.Text.Split('\n');
            for (int i = 0; i < descMsg.Length; i++)
            {
                all += String.Format("{0}\t{1}\t{2}\n", teamMsg[i], descMsg[i], oppMsg[i]);
            }
            Clipboard.SetText(all);
        }
    }
}