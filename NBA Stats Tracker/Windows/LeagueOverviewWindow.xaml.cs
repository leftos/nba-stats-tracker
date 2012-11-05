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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Helper;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Provides an overview of the whole league's stats. Allows filtering by division and conference.
    /// </summary>
    public partial class LeagueOverviewWindow
    {
        private static Dictionary<int, PlayerStats> _pst;
        private static Dictionary<int, TeamStats> _tst, partialTST;
        private static Dictionary<int, TeamStats> _tstopp, partialOppTST;
        private static int lastShownPlayerSeason;
        private static int lastShownLeadersSeason;
        private static int lastShownTeamSeason;
        private static int lastShownPlayoffSeason;
        private static int lastShownBoxSeason;
        private static string message;
        private static Semaphore sem;
        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly DataTable dt_bs;
        private readonly DataTable dt_lpts;
        private readonly DataTable dt_lts;
        private readonly DataTable dt_pts;
        private readonly DataTable dt_ts;
        private bool changingTimeframe;
        /*
                private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        */
        private int curSeason = MainWindow.curSeason;
        private string filterDescription;
        private TeamFilter filterType;
        private List<PlayerStatsRow> pl_psrList;
        private List<PlayerStatsRow> psrList;
        private string q;
        private bool reload;
        private DataTable res;
        private TeamStats ts;
        private TeamStats tsopp;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeagueOverviewWindow" /> class.
        /// </summary>
        /// <param name="tst">The team stats dictionary.</param>
        /// <param name="tstopp">The opposing team stats dictionary.</param>
        /// <param name="pst">The player stats dictionary.</param>
        public LeagueOverviewWindow(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstopp, Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

            #region Prepare DataTables

            dt_ts = new DataTable();

            PrepareTeamDataTable(ref dt_ts);

            dt_lts = new DataTable();

            PrepareTeamDataTable(ref dt_lts);

            dt_pts = new DataTable();

            PreparePlayoffDataTable(ref dt_pts);

            dt_lpts = new DataTable();

            PreparePlayoffDataTable(ref dt_lpts);

            dt_bs = new DataTable();

            dt_bs.Columns.Add("Date");
            dt_bs.Columns.Add("Away");
            dt_bs.Columns.Add("AS", typeof (int));
            dt_bs.Columns.Add("Home");
            dt_bs.Columns.Add("HS", typeof (int));
            dt_bs.Columns.Add("GameID");

            #endregion

            _tst = tst;
            _tstopp = tstopp;
            _pst = new Dictionary<int, PlayerStats>(pst);

            PopulateSeasonCombo();
            PopulateDivisionCombo();

            changingTimeframe = true;
            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1).AddDays(1);
            rbStatsAllTime.IsChecked = true;
            cmbDivConf.SelectedIndex = 0;
            changingTimeframe = false;

            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaders.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaguePlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            sem = new Semaphore(1, 1);
        }

        /// <summary>
        /// Populates the division combo.
        /// </summary>
        private void PopulateDivisionCombo()
        {
            var list = new List<ComboBoxItemWithIsEnabled>();
            list.Add(new ComboBoxItemWithIsEnabled("Whole League"));
            list.Add(new ComboBoxItemWithIsEnabled("-- Conferences --", false));
            foreach (Conference conf in MainWindow.Conferences)
            {
                list.Add(new ComboBoxItemWithIsEnabled(conf.Name));
            }
            list.Add(new ComboBoxItemWithIsEnabled("-- Divisions --", false));
            foreach (Division div in MainWindow.Divisions)
            {
                Conference conf = MainWindow.Conferences.Find(conference => conference.ID == div.ConferenceID);
                list.Add(new ComboBoxItemWithIsEnabled(String.Format("{0}: {1}", conf.Name, div.Name)));
            }
            cmbDivConf.DisplayMemberPath = "Item";
            //cmbDivConf.SelectedValuePath = "Item";
            cmbDivConf.ItemsSource = list;
        }

        /// <summary>
        /// Adds the required columns to the playoff data table.
        /// </summary>
        /// <param name="dt_pts">The playoff data table to be prepared.</param>
        private void PreparePlayoffDataTable(ref DataTable dt_pts)
        {
            dt_pts.Columns.Add("Name");
            dt_pts.Columns.Add("Games", typeof (int));
            dt_pts.Columns.Add("Wins", typeof (int));
            dt_pts.Columns.Add("Losses", typeof (int));
            dt_pts.Columns.Add("W%", typeof (float));
            dt_pts.Columns.Add("Weff", typeof (float));
            dt_pts.Columns.Add("PF", typeof (float));
            dt_pts.Columns.Add("PA", typeof (float));
            dt_pts.Columns.Add("PD", typeof (float));
            dt_pts.Columns.Add("FG", typeof (float));
            dt_pts.Columns.Add("FGeff", typeof (float));
            dt_pts.Columns.Add("3PT", typeof (float));
            dt_pts.Columns.Add("3Peff", typeof (float));
            dt_pts.Columns.Add("FT", typeof (float));
            dt_pts.Columns.Add("FTeff", typeof (float));
            dt_pts.Columns.Add("REB", typeof (float));
            dt_pts.Columns.Add("OREB", typeof (float));
            dt_pts.Columns.Add("DREB", typeof (float));
            dt_pts.Columns.Add("AST", typeof (float));
            dt_pts.Columns.Add("TO", typeof (float));
            dt_pts.Columns.Add("STL", typeof (float));
            dt_pts.Columns.Add("BLK", typeof (float));
            dt_pts.Columns.Add("FOUL", typeof (float));
        }

        /// <summary>
        /// Adds the required columns to the team data table.
        /// </summary>
        /// <param name="dt_ts">The DT_TS.</param>
        private void PrepareTeamDataTable(ref DataTable dt_ts)
        {
            dt_ts.Columns.Add("Name");
            dt_ts.Columns.Add("Games", typeof (int));
            dt_ts.Columns.Add("Wins", typeof (int));
            dt_ts.Columns.Add("Losses", typeof (int));
            dt_ts.Columns.Add("W%", typeof (float));
            dt_ts.Columns.Add("Weff", typeof (float));
            dt_ts.Columns.Add("PF", typeof (float));
            dt_ts.Columns.Add("PA", typeof (float));
            dt_ts.Columns.Add("PD", typeof (float));
            dt_ts.Columns.Add("FG", typeof (float));
            dt_ts.Columns.Add("FGeff", typeof (float));
            dt_ts.Columns.Add("3PT", typeof (float));
            dt_ts.Columns.Add("3Peff", typeof (float));
            dt_ts.Columns.Add("FT", typeof (float));
            dt_ts.Columns.Add("FTeff", typeof (float));
            dt_ts.Columns.Add("REB", typeof (float));
            dt_ts.Columns.Add("OREB", typeof (float));
            dt_ts.Columns.Add("DREB", typeof (float));
            dt_ts.Columns.Add("AST", typeof (float));
            dt_ts.Columns.Add("TO", typeof (float));
            dt_ts.Columns.Add("STL", typeof (float));
            dt_ts.Columns.Add("BLK", typeof (float));
            dt_ts.Columns.Add("FOUL", typeof (float));
        }

        /// <summary>
        /// Finds the team's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private string GetCurTeamFromDisplayName(string displayName)
        {
            return Misc.GetCurTeamFromDisplayName(_tst, displayName);
        }

        /// <summary>
        /// Finds the team's displayName by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private string GetDisplayNameFromTeam(string name)
        {
            return Misc.GetDisplayNameFromTeam(_tst, name);
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the dtpStart control.
        /// Makes sure that the starting date isn't before the ending date, and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                }
                rbStatsBetween.IsChecked = true;
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
                changingTimeframe = false;
            }
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the dtpEnd control.
        /// Makes sure that the starting date isn't before the ending date, and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                }
                rbStatsBetween.IsChecked = true;
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
                changingTimeframe = false;
            }
        }

        /// <summary>
        /// Populates the season combo.
        /// </summary>
        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the tbcLeagueOverview control.
        /// Handles tab changes, and refreshes the data if required (e.g. on season/time-range changes).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void tbcLeagueOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            if (reload || e.OriginalSource is TabControl)
            {
                if (tbcLeagueOverview.SelectedItem == tabTeamStats || tbcLeagueOverview.SelectedItem == tabTeamMetricStats)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownTeamSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PrepareTeamStats();
                        lastShownTeamSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabPlayoffStats || tbcLeagueOverview.SelectedItem == tabTeamPlayoffMetricStats)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownPlayoffSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PreparePlayoffStats();
                        lastShownPlayoffSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabLeaders || tbcLeagueOverview.SelectedItem == tabPlayoffLeaders)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownLeadersSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PreparePlayerStats(leaders: true);
                        lastShownLeadersSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabPlayerStats || tbcLeagueOverview.SelectedItem == tabMetricStats ||
                         tbcLeagueOverview.SelectedItem == tabBest || tbcLeagueOverview.SelectedItem == tabStartingFive ||
                         tbcLeagueOverview.SelectedItem == tabPlayerPlayoffStats || tbcLeagueOverview.SelectedItem == tabPlayoffMetricStats ||
                         tbcLeagueOverview.SelectedItem == tabPlayoffBest || tbcLeagueOverview.SelectedItem == tabPlayoffStartingFive)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownPlayerSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PreparePlayerStats();
                        lastShownPlayerSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabBoxScores)
                {
                    //cmbDivConf.IsEnabled = false;
                    bool doIt = false;
                    if (lastShownBoxSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PrepareBoxScores();
                        lastShownBoxSeason = curSeason;
                    }
                }
                reload = false;
            }
            //}
            //catch (Exception ex)
            //{
            //    //throw ex;
            //}
        }

        /// <summary>
        /// Prepares and presents the player stats.
        /// </summary>
        /// <param name="leaders">if set to <c>true</c>, the stats are calculated based on the NBA rules for League Leaders standings.</param>
        private void PreparePlayerStats(bool leaders = false)
        {
            List<PlayerStatsRow> lpsr;
            List<PlayerStatsRow> pmsrList;
            List<PlayerStatsRow> lpmsr;
            psrList = new List<PlayerStatsRow>();
            lpsr = new List<PlayerStatsRow>();
            pmsrList = new List<PlayerStatsRow>();
            lpmsr = new List<PlayerStatsRow>();

            List<PlayerStatsRow> pl_lpsr;
            List<PlayerStatsRow> pl_pmsrList;
            List<PlayerStatsRow> pl_lpmsr;
            pl_psrList = new List<PlayerStatsRow>();
            pl_lpsr = new List<PlayerStatsRow>();
            pl_pmsrList = new List<PlayerStatsRow>();
            pl_lpmsr = new List<PlayerStatsRow>();

            var leadersList = new List<PlayerStatsRow>();
            var pl_leadersList = new List<PlayerStatsRow>();

            var worker1 = new BackgroundWorker {WorkerReportsProgress = true};

            bool allTime = rbStatsAllTime.IsChecked.GetValueOrDefault();
            bool alltime = allTime;
            DateTime startDate = dtpStart.SelectedDate.GetValueOrDefault();
            DateTime endDate = dtpEnd.SelectedDate.GetValueOrDefault();
            txbStatus.FontWeight = FontWeights.Bold;
            txbStatus.Text = "Please wait while player averages and metric stats are being calculated...";

            int i = 0;

            int playerCount = -1;

            worker1.DoWork += delegate
                              {
                                  sem.WaitOne();
                                  psrList = new List<PlayerStatsRow>();
                                  lpsr = new List<PlayerStatsRow>();
                                  pmsrList = new List<PlayerStatsRow>();
                                  lpmsr = new List<PlayerStatsRow>();

                                  pl_psrList = new List<PlayerStatsRow>();
                                  pl_lpsr = new List<PlayerStatsRow>();
                                  pl_pmsrList = new List<PlayerStatsRow>();
                                  pl_lpmsr = new List<PlayerStatsRow>();

                                  if (alltime)
                                  {
                                      playerCount = _pst.Count;
                                      foreach (var kvp in _pst)
                                      {
                                          if (kvp.Value.isHidden)
                                              continue;
                                          var psr = new PlayerStatsRow(kvp.Value);
                                          var pl_psr = new PlayerStatsRow(kvp.Value, true);

                                          if (psr.isActive)
                                          {
                                              if (!InCurrentFilter(_tst[MainWindow.TeamOrder[psr.TeamF]]))
                                                  continue;
                                              psr.TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName;
                                              pl_psr.TeamFDisplay = psr.TeamFDisplay;

                                              var pmsr = new PlayerStatsRow(kvp.Value) {TeamFDisplay = psr.TeamFDisplay};
                                              var pl_pmsr = new PlayerStatsRow(kvp.Value, true) {TeamFDisplay = psr.TeamFDisplay};
                                              pmsrList.Add(pmsr);
                                              pl_pmsrList.Add(pl_pmsr);
                                          }
                                          else
                                          {
                                              if (filterType != TeamFilter.League)
                                                  continue;

                                              psr.TeamFDisplay = "- Inactive -";
                                              pl_psr.TeamFDisplay = psr.TeamFDisplay;
                                          }
                                          psrList.Add(psr);
                                          pl_psrList.Add(pl_psr);
                                          worker1.ReportProgress(1);
                                      }
                                      PlayerStats leagueAverages = PlayerStats.CalculateLeagueAverages(_pst, _tst);
                                      lpsr.Add(new PlayerStatsRow(leagueAverages));
                                      lpmsr.Add(new PlayerStatsRow(leagueAverages));
                                      pl_lpsr.Add(new PlayerStatsRow(leagueAverages, true));
                                      pl_lpmsr.Add(new PlayerStatsRow(leagueAverages, true));
                                  }
                                  else
                                  {
                                      partialTST = new Dictionary<int, TeamStats>();
                                      partialOppTST = new Dictionary<int, TeamStats>();
                                      // Prepare Teams
                                      foreach (var kvp in MainWindow.TeamOrder)
                                      {
                                          string curTeam = kvp.Key;
                                          int teamID = MainWindow.TeamOrder[curTeam];

                                          q = "select * from GameResults where ((T1Name LIKE \"" + curTeam + "\") OR (T2Name LIKE \"" +
                                              curTeam + "\")) AND ((Date >= \"" + SQLiteDatabase.ConvertDateTimeToSQLite(startDate) +
                                              "\") AND (Date <= \"" + SQLiteDatabase.ConvertDateTimeToSQLite(endDate) +
                                              "\")) ORDER BY Date DESC";
                                          res = db.GetDataTable(q);

                                          var ts2 = new TeamStats(curTeam);
                                          var tsopp2 = new TeamStats(curTeam);
                                          foreach (DataRow r in res.Rows)
                                          {
                                              TeamOverviewWindow.AddToTeamStatsFromSQLBoxScore(r, ref ts2, ref tsopp2);
                                          }
                                          partialTST[teamID] = ts2;
                                          partialOppTST[teamID] = tsopp2;
                                      }

                                      // Prepare Players
                                      q =
                                          "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID)";
                                      q = SQLiteDatabase.AddDateRangeToSQLQuery(q, startDate, endDate);
                                      res = db.GetDataTable(q);

                                      var pstBetween = new Dictionary<int, PlayerStats>();

                                      foreach (DataRow r in res.Rows)
                                      {
                                          bool isPlayoff = Tools.getBoolean(r, "isPlayoff");
                                          var pbs = new PlayerBoxScore(r);
                                          if (pstBetween.ContainsKey(pbs.PlayerID))
                                          {
                                              pstBetween[pbs.PlayerID].AddBoxScore(pbs, isPlayoff);
                                          }
                                          else
                                          {
                                              string q2 = "select * from Players where ID = " + pbs.PlayerID;
                                              DataTable res2 = db.GetDataTable(q2);

                                              var p = new Player(res2.Rows[0]);

                                              var ps = new PlayerStats(p);
                                              ps.AddBoxScore(pbs, isPlayoff);
                                              pstBetween.Add(pbs.PlayerID, ps);
                                          }
                                      }

                                      PlayerStats.CalculateAllMetrics(ref pstBetween, partialTST, partialOppTST, MainWindow.TeamOrder, true);
                                      PlayerStats.CalculateAllMetrics(ref pstBetween, partialTST, partialOppTST, MainWindow.TeamOrder, true,
                                                                      true);

                                      playerCount = pstBetween.Count;
                                      foreach (var kvp in pstBetween)
                                      {
                                          var psr = new PlayerStatsRow(kvp.Value);
                                          var pl_psr = new PlayerStatsRow(kvp.Value, true);
                                          if (psr.isActive)
                                          {
                                              if (!InCurrentFilter(_tst[MainWindow.TeamOrder[psr.TeamF]]))
                                                  continue;

                                              psr.TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName;
                                              pl_psr.TeamFDisplay = psr.TeamFDisplay;

                                              var pmsr = new PlayerStatsRow(kvp.Value)
                                                         {TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName};
                                              var pl_pmsr = new PlayerStatsRow(kvp.Value, true) {TeamFDisplay = pmsr.TeamFDisplay};
                                              pmsrList.Add(pmsr);
                                              pl_pmsrList.Add(pl_pmsr);
                                          }
                                          else
                                          {
                                              if (filterType != TeamFilter.League)
                                                  continue;

                                              psr.TeamFDisplay = "- Inactive -";
                                              pl_psr.TeamFDisplay = psr.TeamFDisplay;
                                          }
                                          psrList.Add(psr);
                                          pl_psrList.Add(psr);
                                          worker1.ReportProgress(1);
                                      }
                                      var psrIDs = new List<int>();
                                      psrList.ForEach(row => psrIDs.Add(row.ID));
                                      PlayerStats leagueAverages = PlayerStats.CalculateLeagueAverages(pstBetween, partialTST);
                                      lpsr.Add(new PlayerStatsRow(leagueAverages));
                                      lpmsr.Add(new PlayerStatsRow(leagueAverages));
                                      pl_lpsr.Add(new PlayerStatsRow(leagueAverages));
                                      pl_lpmsr.Add(new PlayerStatsRow(leagueAverages));
                                  }

                                  psrList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                  psrList.Reverse();

                                  pmsrList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                                  pmsrList.Reverse();

                                  pl_psrList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                  pl_psrList.Reverse();

                                  pl_pmsrList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                                  pl_pmsrList.Reverse();

                                  if (leaders)
                                  {
                                      if (allTime)
                                      {
                                          foreach (PlayerStatsRow psr in psrList)
                                          {
                                              if (psr.isActive)
                                                  leadersList.Add(ConvertToLeagueLeader(psr, _tst));
                                          }
                                          foreach (PlayerStatsRow psr in pl_psrList)
                                          {
                                              if (psr.isActive)
                                                  pl_leadersList.Add(ConvertToLeagueLeader(psr, _tst, true));
                                          }
                                      }
                                      else
                                      {
                                          foreach (PlayerStatsRow psr in psrList)
                                          {
                                              if (psr.isActive)
                                                  leadersList.Add(ConvertToLeagueLeader(psr, partialTST));
                                          }
                                          foreach (PlayerStatsRow psr in pl_psrList)
                                          {
                                              if (psr.isActive)
                                                  pl_leadersList.Add(ConvertToLeagueLeader(psr, partialTST, true));
                                          }
                                      }
                                  }
                              };

            worker1.ProgressChanged += delegate
                                       {
                                           if (++i < playerCount)
                                           {
                                               txbStatus.Text =
                                                   "Please wait while player averages and metric stats are being calculated (" + i + "/" +
                                                   playerCount + " completed)...";
                                           }
                                           else
                                           {
                                               txbStatus.Text = "Please wait as best performers and best starting 5 are being calculated...";
                                           }
                                       };

            worker1.RunWorkerCompleted += delegate
                                          {
                                              dgvPlayerStats.ItemsSource = psrList;
                                              dgvLeaguePlayerStats.ItemsSource = lpsr;
                                              dgvMetricStats.ItemsSource = pmsrList;
                                              dgvLeagueMetricStats.ItemsSource = lpmsr;

                                              dgvPlayerPlayoffStats.ItemsSource = pl_psrList;
                                              dgvLeaguePlayerPlayoffStats.ItemsSource = pl_lpsr;
                                              dgvPlayoffMetricStats.ItemsSource = pl_pmsrList;
                                              dgvLeaguePlayoffMetricStats.ItemsSource = pl_lpmsr;

                                              PrepareBestPerformers(pmsrList, pl_pmsrList);

                                              if (leaders)
                                              {
                                                  leadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                                  leadersList.Reverse();

                                                  pl_leadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                                  pl_leadersList.Reverse();

                                                  dgvLeaders.ItemsSource = leadersList;
                                                  dgvPlayoffLeaders.ItemsSource = pl_leadersList;
                                              }

                                              tbcLeagueOverview.Visibility = Visibility.Visible;
                                              txbStatus.FontWeight = FontWeights.Normal;
                                              txbStatus.Text = message;
                                              sem.Release();
                                          };

            worker1.RunWorkerAsync();
        }

        /// <summary>
        /// Prepares and presents the best performers' stats.
        /// </summary>
        /// <param name="pmsrList">The list of currently loaded PlayerMetricStatsRow instances.</param>
        /// <param name="pl_pmsrList">The list of currently loaded playoff PlayerMetricStatsRow instances.</param>
        private void PrepareBestPerformers(List<PlayerStatsRow> pmsrList, List<PlayerStatsRow> pl_pmsrList)
        {
            txbPlayer1.Text = "";
            txbPlayer2.Text = "";
            txbPlayer3.Text = "";
            txbPlayer4.Text = "";
            txbPlayer5.Text = "";
            txbPlayer6.Text = "";

            var templist = new List<PlayerStatsRow>();
            try
            {
                templist = pmsrList.ToList();
                templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                templist.Reverse();

                PlayerStatsRow psr1 = templist[0];
                string text = psr1.GetBestStats(5);
                txbPlayer1.Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                                  text;

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(5);
                txbPlayer2.Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + " - " + psr2.TeamFDisplay + ")\n\n" +
                                  text;

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(5);
                txbPlayer3.Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + " - " + psr3.TeamFDisplay + ")\n\n" +
                                  text;

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(5);
                txbPlayer4.Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + " - " + psr4.TeamFDisplay + ")\n\n" +
                                  text;

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(5);
                txbPlayer5.Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + " - " + psr5.TeamFDisplay + ")\n\n" +
                                  text;

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(5);
                txbPlayer6.Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + " - " + psr6.TeamFDisplay + ")\n\n" +
                                  text;
            }
            catch (Exception)
            {
            }
            CalculateStarting5(templist);


            txbPlPlayer1.Text = "";
            txbPlPlayer2.Text = "";
            txbPlPlayer3.Text = "";
            txbPlPlayer4.Text = "";
            txbPlPlayer5.Text = "";
            txbPlPlayer6.Text = "";

            templist = new List<PlayerStatsRow>();
            try
            {
                templist = pl_pmsrList.ToList();
                templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                templist.Reverse();

                PlayerStatsRow psr1 = templist[0];
                string text = psr1.GetBestStats(5);
                txbPlPlayer1.Text = string.Format("1: {0} {1} ({2} - {3})\n\n{4}", psr1.FirstName, psr1.LastName, psr1.Position1, psr1.TeamFDisplay, text);

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(5);
                txbPlPlayer2.Text = string.Format("2: {0} {1} ({2} - {3})\n\n{4}", psr2.FirstName, psr2.LastName, psr2.Position1, psr2.TeamFDisplay, text);

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(5);
                txbPlPlayer3.Text = string.Format("3: {0} {1} ({2} - {3})\n\n{4}", psr3.FirstName, psr3.LastName, psr3.Position1, psr3.TeamFDisplay, text);

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(5);
                txbPlPlayer4.Text = string.Format("4: {0} {1} ({2} - {3})\n\n{4}", psr4.FirstName, psr4.LastName, psr4.Position1, psr4.TeamFDisplay, text);

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(5);
                txbPlPlayer5.Text = string.Format("5: {0} {1} ({2} - {3})\n\n{4}", psr5.FirstName, psr5.LastName, psr5.Position1, psr5.TeamFDisplay, text);

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(5);
                txbPlPlayer6.Text = string.Format("6: {0} {1} ({2} - {3})\n\n{4}", psr6.FirstName, psr6.LastName, psr6.Position1, psr6.TeamFDisplay, text);
            }
            catch (Exception)
            {
            }
            CalculateStarting5(templist, true);
        }

        /// <summary>
        /// Calculates the best starting five for the current scope.
        /// </summary>
        /// <param name="sortedPSRList">The list of currently loaded PlayerStatsRow instances, sorted by GmSc in descending order.</param>
        /// <param name="playoffs">if set to <c>true</c>, the starting five will be determined based on their playoff performances.</param>
        private void CalculateStarting5(List<PlayerStatsRow> sortedPSRList, bool playoffs = false)
        {
            if (!playoffs)
            {
                txbStartingPG.Text = "";
                txbStartingSG.Text = "";
                txbStartingSF.Text = "";
                txbStartingPF.Text = "";
                txbStartingC.Text = "";
                txbSubs.Text = "";
            }
            else
            {
                txbPlStartingPG.Text = "";
                txbPlStartingSG.Text = "";
                txbPlStartingSF.Text = "";
                txbPlStartingPF.Text = "";
                txbPlStartingC.Text = "";
                txbPlSubs.Text = "";
            }

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            List<PlayerStatsRow> PGList =
                sortedPSRList.Where(row => (row.Position1 == Position.PG || row.Position2 == Position.PG) && row.isInjured == false).Take(10).ToList();
            PGList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            PGList.Reverse();
            List<PlayerStatsRow> SGList =
                sortedPSRList.Where(row => (row.Position1 == Position.SG || row.Position2 == Position.SG) && row.isInjured == false).Take(10).ToList();
            SGList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            SGList.Reverse();
            List<PlayerStatsRow> SFList =
                sortedPSRList.Where(row => (row.Position1 == Position.SF || row.Position2 == Position.SF) && row.isInjured == false).Take(10).ToList();
            SFList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            SFList.Reverse();
            List<PlayerStatsRow> PFList =
                sortedPSRList.Where(row => (row.Position1 == Position.PF || row.Position2 == Position.PF) && row.isInjured == false).Take(10).ToList();
            PFList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            PFList.Reverse();
            List<PlayerStatsRow> CList =
                sortedPSRList.Where(row => (row.Position1 == Position.C || row.Position2 == Position.C) && row.isInjured == false).Take(10).ToList();
            CList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            CList.Reverse();
            var permutations = new List<StartingFivePermutation>();

            double max = Double.MinValue;
            for (int i1 = 0; i1 < PGList.Count; i1++)
                for (int i2 = 0; i2 < SGList.Count; i2++)
                    for (int i3 = 0; i3 < SFList.Count; i3++)
                        for (int i4 = 0; i4 < PFList.Count; i4++)
                            for (int i5 = 0; i5 < CList.Count; i5++)
                            {
                                double _sum = 0;
                                int _pInP = 0;
                                var perm = new List<int>();
                                perm.Add(PGList[i1].ID);
                                _sum += PGList[i1].GmSc;
                                if (PGList[i1].Position1.ToString() == "PG")
                                    _pInP++;
                                if (perm.Contains(SGList[i2].ID))
                                {
                                    continue;
                                }
                                perm.Add(SGList[i2].ID);
                                _sum += SGList[i2].GmSc;
                                if (SGList[i2].Position1.ToString() == "SG")
                                    _pInP++;
                                if (perm.Contains(SFList[i3].ID))
                                {
                                    continue;
                                }
                                perm.Add(SFList[i3].ID);
                                _sum += SFList[i3].GmSc;
                                if (SFList[i3].Position1.ToString() == "SF")
                                    _pInP++;
                                if (perm.Contains(PFList[i4].ID))
                                {
                                    continue;
                                }
                                perm.Add(PFList[i4].ID);
                                _sum += PFList[i4].GmSc;
                                if (PFList[i4].Position1.ToString() == "PF")
                                    _pInP++;
                                if (perm.Contains(CList[i5].ID))
                                {
                                    continue;
                                }
                                perm.Add(CList[i5].ID);
                                _sum += CList[i5].GmSc;
                                if (CList[i5].Position1.ToString() == "C")
                                    _pInP++;

                                if (_sum > max)
                                    max = _sum;

                                permutations.Add(new StartingFivePermutation {idList = perm, PlayersInPrimaryPosition = _pInP, Sum = _sum});
                            }

            StartingFivePermutation bestPerm;
            try
            {
                bestPerm = permutations.Where(perm1 => perm1.Sum.Equals(max)).OrderByDescending(perm2 => perm2.PlayersInPrimaryPosition).First();
                bestPerm.idList.ForEach(i1 => tempList.Add(sortedPSRList.Single(row => row.ID == i1)));
            }
            catch (Exception)
            {
                return;
            }

            string displayText;
            try
            {
                psr1 = tempList[0];
                text = psr1.GetBestStats(5);
                displayText = "PG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                              text;
                if (!playoffs)
                    txbStartingPG.Text = displayText;
                else
                    txbPlStartingPG.Text = displayText;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[1];
                text = psr1.GetBestStats(5);
                displayText = "SG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                              text;
                if (!playoffs)
                    txbStartingSG.Text = displayText;
                else
                    txbPlStartingSG.Text = displayText;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[2];
                text = psr1.GetBestStats(5);
                displayText = "SF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                              text;
                if (!playoffs)
                    txbStartingSF.Text = displayText;
                else
                    txbPlStartingSF.Text = displayText;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[3];
                text = psr1.GetBestStats(5);
                displayText = "PF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                              text;
                if (!playoffs)
                    txbStartingPF.Text = displayText;
                else
                    txbPlStartingPF.Text = displayText;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[4];
                text = psr1.GetBestStats(5);
                displayText = "C: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                              text;
                if (!playoffs)
                    txbStartingC.Text = displayText;
                else
                    txbPlStartingC.Text = displayText;
            }
            catch (Exception)
            {
            }

            // Subs
            var usedIDs = new List<int>(bestPerm.idList);
            int i = 0;
            try
            {
                while (usedIDs.Contains(PGList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(PGList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(SGList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(SGList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(SFList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(SFList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(PFList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(PFList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(CList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(CList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                int count = usedIDs.Count;
                for (int j = count + 1; j <= 12; j++)
                {
                    i = 0;
                    while (usedIDs.Contains(sortedPSRList[i].ID))
                    {
                        i++;
                    }
                    usedIDs.Add(sortedPSRList[i].ID);
                }
            }
            catch (Exception)
            {
            }

            usedIDs.Skip(5).ToList().ForEach(id => tempList.Add(sortedPSRList.Single(row => row.ID == id)));
            displayText = "Subs: ";
            for (i = 5; i < usedIDs.Count; i++)
            {
                psr1 = tempList[i];
                //text = psr1.GetBestStats(5);
                displayText += psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + "), ";
            }
            displayText = displayText.TrimEnd(new[] {' ', ','});

            if (!playoffs)
                txbSubs.Text = displayText;
            else
                txbPlSubs.Text = displayText;
        }

        /// <summary>
        /// Prepares and presents the available box scores.
        /// </summary>
        private void PrepareBoxScores()
        {
            dt_bs.Clear();

            q = "select * from GameResults";

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                q += " where SeasonNum = " + cmbSeasonNum.SelectedValue;
            }
            else
            {
                q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                          dtpEnd.SelectedDate.GetValueOrDefault(), true);
            }
            q += " ORDER BY Date DESC";

            res = db.GetDataTable(q);

            foreach (DataRow dr in res.Rows)
            {
                if (!InCurrentFilter(dr["T1Name"].ToString()) && !InCurrentFilter(dr["T2Name"].ToString()))
                {
                    continue;
                }

                DataRow r = dt_bs.NewRow();

                try
                {
                    r["Date"] = dr["Date"].ToString().Split(' ')[0];
                    r["Away"] = GetDisplayNameFromTeam(dr["T1Name"].ToString());
                    r["AS"] = Convert.ToInt32(dr["T1PTS"].ToString());
                    r["Home"] = GetDisplayNameFromTeam(dr["T2Name"].ToString());
                    r["HS"] = Convert.ToInt32(dr["T2PTS"].ToString());
                    r["GameID"] = dr["GameID"].ToString();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    continue;
                }

                dt_bs.Rows.Add(r);
            }

            var dv_bs = new DataView(dt_bs) {AllowNew = false, AllowEdit = false};

            dgvBoxScores.DataContext = dv_bs;
        }

        /// <summary>
        /// Prepares and presents the playoff stats.
        /// </summary>
        private void PreparePlayoffStats()
        {
            var tmsrList = new List<TeamMetricStatsRow>();
            var lssr = new List<TeamMetricStatsRow>();

            dt_pts.Clear();
            dt_lpts.Clear();

            var ls = new TeamStats("League");

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out _tst, out _tstopp, out MainWindow.TeamOrder);

                TeamStats.CalculateAllMetrics(ref _tst, _tstopp, true);

                foreach (int key in _tst.Keys)
                {
                    if (_tst[key].isHidden)
                        continue;
                    if (_tst[key].getPlayoffGames() == 0)
                        continue;
                    if (!InCurrentFilter(_tst[key]))
                        continue;

                    DataRow r = dt_pts.NewRow();

                    TeamOverviewWindow.CreateDataRowFromTeamStats(_tst[key], ref r, GetDisplayNameFromTeam(_tst[key].name), true);

                    dt_pts.Rows.Add(r);

                    tmsrList.Add(new TeamMetricStatsRow(_tst[key], true));
                }

                ls = TeamStats.CalculateLeagueAverages(_tst, Span.Playoffs);
            }
            else
            {
                partialTST = new Dictionary<int, TeamStats>();
                partialOppTST = new Dictionary<int, TeamStats>();

                int i = 0;
                foreach (var kvp in MainWindow.TeamOrder)
                {
                    if (!InCurrentFilter(kvp.Key))
                        continue;

                    q =
                        String.Format(
                            "select * from GameResults where ((T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") AND IsPlayoff LIKE \"True\");",
                            kvp.Key);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());

                    res = db.GetDataTable(q);

                    DataRow r = dt_pts.NewRow();

                    ts = new TeamStats(kvp.Key);
                    tsopp = new TeamStats();
                    TeamOverviewWindow.AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                    TeamOverviewWindow.CreateDataRowFromTeamStats(ts, ref r, GetDisplayNameFromTeam(kvp.Key), true);
                    ts.CalcMetrics(tsopp, true);
                    partialTST[i] = ts;
                    partialOppTST[i] = tsopp;

                    tmsrList.Add(new TeamMetricStatsRow(ts, true));

                    dt_pts.Rows.Add(r);
                    i++;
                }

                ls = TeamStats.CalculateLeagueAverages(partialTST, Span.Playoffs);
            }

            DataRow r2 = dt_lpts.NewRow();

            TeamOverviewWindow.CreateDataRowFromTeamStats(ls, ref r2, "League", true);

            dt_lpts.Rows.Add(r2);

            // DataTable's ready, set DataView and fill DataGrid
            var dv_pts = new DataView(dt_pts) {AllowNew = false, AllowEdit = false, Sort = "Weff DESC"};
            var dv_lpts = new DataView(dt_lpts) {AllowNew = false, AllowEdit = false};

            lssr.Add(new TeamMetricStatsRow(ls, true));

            dgvPlayoffStats.DataContext = dv_pts;
            dgvLeaguePlayoffStats.DataContext = dv_lpts;

            tmsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            tmsrList.Reverse();
            dgvTeamPlayoffMetricStats.ItemsSource = tmsrList;
            dgvLeagueTeamPlayoffMetricStats.ItemsSource = lssr;
        }

        /// <summary>
        /// Prepares and presents the team stats.
        /// </summary>
        private void PrepareTeamStats()
        {
            var tmsrList = new List<TeamMetricStatsRow>();
            var lssr = new List<TeamMetricStatsRow>();

            var ls = new TeamStats("League");

            dt_lts.Clear();
            dt_ts.Clear();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out _tst, out _tstopp, out MainWindow.TeamOrder);

                TeamStats.CalculateAllMetrics(ref _tst, _tstopp);

                foreach (int key in _tst.Keys)
                {
                    if (_tst[key].isHidden)
                        continue;

                    if (!InCurrentFilter(_tst[key]))
                        continue;

                    DataRow r = dt_ts.NewRow();

                    TeamOverviewWindow.CreateDataRowFromTeamStats(_tst[key], ref r, GetDisplayNameFromTeam(_tst[key].name));

                    dt_ts.Rows.Add(r);

                    tmsrList.Add(new TeamMetricStatsRow(_tst[key]));
                }

                ls = TeamStats.CalculateLeagueAverages(_tst, Span.Season);
            }
            else
            {
                var partialTST = new Dictionary<int, TeamStats>();
                var partialOppTST = new Dictionary<int, TeamStats>();

                int i = 0;
                foreach (var kvp in MainWindow.TeamOrder)
                {
                    if (!InCurrentFilter(kvp.Key))
                        continue;

                    q =
                        String.Format(
                            "select * from GameResults where ((T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") AND IsPlayoff LIKE \"False\");",
                            kvp.Key);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());

                    res = db.GetDataTable(q);

                    DataRow r = dt_ts.NewRow();

                    ts = new TeamStats(kvp.Key);
                    tsopp = new TeamStats();
                    TeamOverviewWindow.AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                    TeamOverviewWindow.CreateDataRowFromTeamStats(ts, ref r, GetDisplayNameFromTeam(kvp.Key));
                    ts.CalcMetrics(tsopp);
                    partialTST[i] = ts;
                    partialOppTST[i] = tsopp;

                    tmsrList.Add(new TeamMetricStatsRow(ts));

                    dt_ts.Rows.Add(r);

                    i++;
                }
                TeamStats.CalculateAllMetrics(ref partialTST, partialOppTST);

                ls = TeamStats.CalculateLeagueAverages(partialTST, Span.Season);
            }

            DataRow r2 = dt_lts.NewRow();

            TeamOverviewWindow.CreateDataRowFromTeamStats(ls, ref r2, "League");

            dt_lts.Rows.Add(r2);

            lssr.Add(new TeamMetricStatsRow(ls));

            // DataTable's ready, set DataView and fill DataGrid
            var dv_ts = new DataView(dt_ts) {AllowNew = false, AllowEdit = false, Sort = "Weff DESC"};
            var dv_lts = new DataView(dt_lts) {AllowNew = false, AllowEdit = false};

            dgvTeamStats.DataContext = dv_ts;
            dgvLeagueTeamStats.DataContext = dv_lts;

            tmsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            tmsrList.Reverse();
            dgvTeamMetricStats.ItemsSource = tmsrList;
            dgvLeagueTeamMetricStats.ItemsSource = lssr;
        }

        /// <summary>
        /// Determines whether a specific team should be shown or not, based on the current filter.
        /// </summary>
        /// <param name="ts">The team's TeamStats instance.</param>
        /// <returns>true if it should be shown; otherwise, false</returns>
        private bool InCurrentFilter(TeamStats ts)
        {
            if (filterType == TeamFilter.League)
                return true;

            if (filterType == TeamFilter.Conference)
            {
                int confID = -1;
                foreach (Conference conf in MainWindow.Conferences)
                {
                    if (conf.Name == filterDescription)
                    {
                        confID = conf.ID;
                        break;
                    }
                }
                Division div = MainWindow.Divisions.Find(division => division.ID == ts.division);
                if (div.ConferenceID == confID)
                    return true;
                else
                    return false;
            }
            else
            {
                Division div = MainWindow.Divisions.Find(division => division.ID == ts.division);
                if (div.Name == filterDescription)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Determines whether a specific team should be shown or not, based on the current filter.
        /// </summary>
        /// <param name="teamName">Name of the team.</param>
        /// <returns>
        /// true if it should be shown; otherwise, false
        /// </returns>
        private bool InCurrentFilter(string teamName)
        {
            if (filterType == TeamFilter.League)
                return true;

            DataTable res = db.GetDataTable("SELECT Division FROM Teams WHERE Name LIKE \"" + teamName + "\"");
            int divID = Tools.getInt(res.Rows[0], "Division");

            if (filterType == TeamFilter.Conference)
            {
                int confID = -1;
                foreach (Conference conf in MainWindow.Conferences)
                {
                    if (conf.Name == filterDescription)
                    {
                        confID = conf.ID;
                        break;
                    }
                }
                Division div = MainWindow.Divisions.Find(division => division.ID == divID);
                if (div.ConferenceID == confID)
                    return true;
                else
                    return false;
            }
            else
            {
                Division div = MainWindow.Divisions.Find(division => division.ID == divID);
                if (div.Name == filterDescription)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Handles the Checked event of the rbStatsAllTime control.
        /// Changes the timeframe to the whole season, forces all tabs to be reloaded on first request, and reloads the current one.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        /// <summary>
        /// Handles the Checked event of the rbStatsBetween control.
        /// Changes the timeframe to be between the specified dates, forces all tabs to be reloaded on first request, and reloads the current one.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        /// <summary>
        /// Handles the LoadingRow event of the dg control. 
        /// Adds a ranking number to the row's header.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridRowEventArgs" /> instance containing the event data.</param>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbSeasonNum control.
        /// Loads all the required information for the new season and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1)
                return;

            curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

            SQLiteIO.LoadSeason(MainWindow.currentDB, out _tst, out _tstopp, out _pst, out MainWindow.TeamOrder,
                                ref MainWindow.bshist, _curSeason: curSeason);
            MainWindow.CopySeasonToMainWindow(_tst, _tstopp, _pst);

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                reload = true;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the dgvBoxScores control. 
        /// Views the selected box score in the Box Score Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvBoxScores.SelectedItems[0];
                int gameid = Convert.ToInt32(row["GameID"].ToString());

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameid);
                try
                {
                    bsw.ShowDialog();

                    MainWindow.UpdateBoxScore();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the AnyTeamDataGrid control.
        /// Views the selected team in the Team Overview Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void AnyTeamDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventHandlers.AnyTeamDataGrid_MouseDoubleClick(sender, e);
        }

        /// <summary>
        /// Edits a player's stats row to adjust for the rules and requirements of the NBA's League Leaders standings.
        /// </summary>
        /// <param name="psr">The player stats row.</param>
        /// <param name="teamStats">The player's team stats.</param>
        /// <param name="playoffs">if set to <c>true</c>, the playoff stats will be edited; otherwise, the regular season's.</param>
        /// <returns></returns>
        private PlayerStatsRow ConvertToLeagueLeader(PlayerStatsRow psr, Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            string team = psr.TeamF;
            ts = teamStats[MainWindow.TeamOrder[team]];
            uint gamesTeam = (!playoffs) ? ts.getGames() : ts.getPlayoffGames();
            uint gamesPlayer = psr.GP;
            var newpsr = new PlayerStatsRow(new PlayerStats(psr));

            // Below functions found using Eureqa II
            var gamesRequired = (int) Math.Ceiling(0.8522*gamesTeam); // Maximum error of 0
            var fgmRequired = (int) Math.Ceiling(3.65*gamesTeam); // Max error of 0
            var ftmRequired = (int) Math.Ceiling(1.52*gamesTeam);
            var tpmRequired = (int) Math.Ceiling(0.666671427752402*gamesTeam);
            var ptsRequired = (int) Math.Ceiling(17.07*gamesTeam);
            var rebRequired = (int) Math.Ceiling(9.74720677727814*gamesTeam);
            var astRequired = (int) Math.Ceiling(4.87*gamesTeam);
            var stlRequired = (int) Math.Ceiling(1.51957078555763*gamesTeam);
            var blkRequired = (int) Math.Ceiling(1.21*gamesTeam);
            var minRequired = (int) Math.Ceiling(24.39*gamesTeam);

            if (psr.FGM < fgmRequired)
                newpsr.FGp = float.NaN;
            if (psr.TPM < tpmRequired)
                newpsr.TPp = float.NaN;
            if (psr.FTM < ftmRequired)
                newpsr.FTp = float.NaN;

            if (gamesPlayer >= gamesRequired)
            {
                return newpsr;
            }

            if (psr.PTS < ptsRequired)
                newpsr.PPG = float.NaN;
            if (psr.REB < rebRequired)
                newpsr.RPG = float.NaN;
            if (psr.AST < astRequired)
                newpsr.APG = float.NaN;
            if (psr.STL < stlRequired)
                newpsr.SPG = float.NaN;
            if (psr.BLK < blkRequired)
                newpsr.BPG = float.NaN;
            if (psr.MINS < minRequired)
                newpsr.MPG = float.NaN;
            return newpsr;
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the AnyPlayerDataGrid control.
        /// Views the selected player in the Player Overview Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void AnyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventHandlers.AnyPlayerDataGrid_MouseDoubleClick(sender, e);
        }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// Forces all tabs to be reloaded on first request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstopp, MainWindow.TeamOrder, true);
            lastShownPlayerSeason = 0;
            lastShownLeadersSeason = 0;
            lastShownTeamSeason = curSeason;
            lastShownPlayoffSeason = 0;
            lastShownBoxSeason = 0;
            message = txbStatus.Text;
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// Forces all tabs to be reloaded on first request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs" /> instance containing the event data.</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            lastShownTeamSeason = 0;
            lastShownPlayerSeason = 0;
            lastShownPlayoffSeason = 0;
            lastShownLeadersSeason = 0;
            lastShownBoxSeason = 0;
        }

        /// <summary>
        /// Handles the Sorting event of the StatColumn control.
        /// Uses a custom Sorting event handler that sorts a stat column in descending order, if it's not already sorted.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridSortingEventArgs" /> instance containing the event data.</param>
        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvTeamMetricStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvTeamMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamMetricStats.Columns.Count && i < dgvLeagueTeamMetricStats.Columns.Count; ++i)
                dgvLeagueTeamMetricStats.Columns[i].Width = dgvTeamMetricStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvTeamPlayoffMetricStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvTeamPlayoffMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamPlayoffMetricStats.Columns.Count && i < dgvLeagueTeamPlayoffMetricStats.Columns.Count; ++i)
                dgvLeagueTeamPlayoffMetricStats.Columns[i].Width = dgvTeamPlayoffMetricStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvTeamStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvTeamStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamStats.Columns.Count && i < dgvLeagueTeamStats.Columns.Count; ++i)
                dgvLeagueTeamStats.Columns[i].Width = dgvTeamStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvPlayoffStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvPlayoffStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvPlayoffStats.Columns.Count && i < dgvLeaguePlayoffStats.Columns.Count; ++i)
                dgvLeaguePlayoffStats.Columns[i].Width = dgvPlayoffStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvPlayerStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvPlayerStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvPlayerStats.Columns.Count && i < dgvLeaguePlayerStats.Columns.Count; ++i)
                dgvLeaguePlayerStats.Columns[i].Width = dgvPlayerStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvPlayerPlayoffStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvPlayerPlayoffStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvPlayerPlayoffStats.Columns.Count && i < dgvLeaguePlayerPlayoffStats.Columns.Count; ++i)
                dgvLeaguePlayerPlayoffStats.Columns[i].Width = dgvPlayerPlayoffStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LayoutUpdated event of the dgvMetricStats control.
        /// Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dgvMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvMetricStats.Columns.Count && i < dgvLeagueMetricStats.Columns.Count; ++i)
                dgvLeagueMetricStats.Columns[i].Width = dgvMetricStats.Columns[i].ActualWidth;
        }

        /// <summary>
        /// Handles the LoadingRow event of the dgLeague control.
        /// Adds an "L" to the row header for the league average row.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridRowEventArgs" /> instance containing the event data.</param>
        private void dgLeague_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = "L";
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbDivConf control.
        /// Applies the new filter, forces all tabs to reload on first request and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbDivConf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingTimeframe)
                return;

            if (cmbDivConf.SelectedIndex == -1)
                return;

            var cur = (ComboBoxItemWithIsEnabled) cmbDivConf.SelectedItem;
            string name = cur.Item;
            string[] parts = name.Split(new[] {": "}, 2, StringSplitOptions.None);
            if (parts.Length == 1)
            {
                if (parts[0] == "Whole League")
                {
                    filterType = TeamFilter.League;
                    filterDescription = "";
                }
                else
                {
                    filterType = TeamFilter.Conference;
                    filterDescription = parts[0];
                }
            }
            else
            {
                filterType = TeamFilter.Division;
                filterDescription = parts[1];
            }

            reload = true;
            lastShownTeamSeason = 0;
            lastShownPlayerSeason = 0;
            lastShownPlayoffSeason = 0;
            lastShownLeadersSeason = 0;
            lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        /// <summary>
        /// Handles the Checked event of the rbSeason control.
        /// Switches the visibility of the Season tabs to visible, and of the Playoff tabs to hidden.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbSeason_Checked(object sender, RoutedEventArgs e)
        {
            tabTeamStats.Visibility = Visibility.Visible;
            tabTeamMetricStats.Visibility = Visibility.Visible;
            tabPlayerStats.Visibility = Visibility.Visible;
            tabMetricStats.Visibility = Visibility.Visible;
            tabStartingFive.Visibility = Visibility.Visible;
            tabBest.Visibility = Visibility.Visible;
            tabLeaders.Visibility = Visibility.Visible;

            if (tbcLeagueOverview.SelectedItem != null)
            {
                var curTab = tbcLeagueOverview.SelectedItem as TabItem;
                switch (curTab.Name)
                {
                    case "tabPlayoffStats":
                        tbcLeagueOverview.SelectedItem = tabTeamStats;
                        break;

                    case "tabTeamPlayoffMetricStats":
                        tbcLeagueOverview.SelectedItem = tabTeamMetricStats;
                        break;

                    case "tabPlayerPlayoffStats":
                        tbcLeagueOverview.SelectedItem = tabPlayerStats;
                        break;

                    case "tabPlayoffMetricStats":
                        tbcLeagueOverview.SelectedItem = tabMetricStats;
                        break;

                    case "tabPlayoffBest":
                        tbcLeagueOverview.SelectedItem = tabBest;
                        break;

                    case "tabPlayoffStartingFive":
                        tbcLeagueOverview.SelectedItem = tabStartingFive;
                        break;

                    case "tabPlayoffLeaders":
                        tbcLeagueOverview.SelectedItem = tabLeaders;
                        break;
                }
            }

            tabPlayoffStats.Visibility = Visibility.Collapsed;
            tabTeamPlayoffMetricStats.Visibility = Visibility.Collapsed;
            tabPlayerPlayoffStats.Visibility = Visibility.Collapsed;
            tabPlayoffMetricStats.Visibility = Visibility.Collapsed;
            tabPlayoffBest.Visibility = Visibility.Collapsed;
            tabPlayoffStartingFive.Visibility = Visibility.Collapsed;
            tabPlayoffLeaders.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the Checked event of the rbPlayoffs control.
        /// Switches the visibility of the Season tabs to hidden, and of the Playoff tabs to visible.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbPlayoffs_Checked(object sender, RoutedEventArgs e)
        {
            tabPlayoffStats.Visibility = Visibility.Visible;
            tabTeamPlayoffMetricStats.Visibility = Visibility.Visible;
            tabPlayerPlayoffStats.Visibility = Visibility.Visible;
            tabPlayoffMetricStats.Visibility = Visibility.Visible;
            tabPlayoffBest.Visibility = Visibility.Visible;
            tabPlayoffStartingFive.Visibility = Visibility.Visible;
            tabPlayoffLeaders.Visibility = Visibility.Visible;

            if (tbcLeagueOverview.SelectedItem != null)
            {
                var curTab = tbcLeagueOverview.SelectedItem as TabItem;
                switch (curTab.Name)
                {
                    case "tabTeamStats":
                        tbcLeagueOverview.SelectedItem = tabPlayoffStats;
                        break;

                    case "tabTeamMetricStats":
                        tbcLeagueOverview.SelectedItem = tabTeamPlayoffMetricStats;
                        break;

                    case "tabPlayerStats":
                        tbcLeagueOverview.SelectedItem = tabPlayerPlayoffStats;
                        break;

                    case "tabMetricStats":
                        tbcLeagueOverview.SelectedItem = tabPlayoffMetricStats;
                        break;

                    case "tabBest":
                        tbcLeagueOverview.SelectedItem = tabPlayoffBest;
                        break;

                    case "tabStartingFive":
                        tbcLeagueOverview.SelectedItem = tabPlayoffStartingFive;
                        break;

                    case "tabLeaders":
                        tbcLeagueOverview.SelectedItem = tabPlayoffLeaders;
                        break;
                }
            }

            tabTeamStats.Visibility = Visibility.Collapsed;
            tabTeamMetricStats.Visibility = Visibility.Collapsed;
            tabPlayerStats.Visibility = Visibility.Collapsed;
            tabMetricStats.Visibility = Visibility.Collapsed;
            tabStartingFive.Visibility = Visibility.Collapsed;
            tabBest.Visibility = Visibility.Collapsed;
            tabLeaders.Visibility = Visibility.Collapsed;
        }

        #region Nested type: TeamFilter

        /// <summary>
        /// Used to determine the filter that should be applied to which teams and players are included in the calculations
        /// and shown in the DataGrids.
        /// </summary>
        private enum TeamFilter
        {
            League,
            Conference,
            Division
        }

        #endregion

    }
}