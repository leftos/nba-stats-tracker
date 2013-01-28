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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.Miscellaneous;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Provides an overview of the whole league's stats. Allows filtering by division and conference.
    /// </summary>
    public partial class LeagueOverviewWindow
    {
        private static Dictionary<int, PlayerStats> _pst;
        private static Dictionary<int, TeamStats> _tst;
        private static List<BoxScoreEntry> _bshist;
        private static int lastShownPlayerSeason;
        private static int lastShownTeamSeason;
        private static int lastShownBoxSeason;
        private static string message;
        private static Semaphore sem;
        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly DataTable dt_bs;

        private Dictionary<string, string> REtoREDitor = new Dictionary<string, string>
                                                         {
                                                             {"RFT", "SShtFT"},
                                                             {"RPass", "SPass"},
                                                             {"RBlock", "SBlock"},
                                                             {"RSteal", "SSteal"},
                                                             {"ROffRbd", "SOReb"},
                                                             {"RDefRbd", "SDReb"},
                                                             {"TShotTnd", "TShtTend"},
                                                             {"TDrawFoul", "TDrawFoul"},
                                                             {"TTouch", "TTouches"},
                                                             {"TCommitFl", "TCommFoul"}
                                                         };

        private List<PlayerStatsRow> _leadersList;
        private List<PlayerStatsRow> _myLeadersList;
        private List<PlayerStatsRow> _plLeadersList;
        private List<PlayerStatsRow> _plMyLeadersList;
        private List<PlayerStatsRow> _plPsrList;
        private List<PlayerStatsRow> _psrList;
        private Dictionary<int, TeamStats> _tstopp;
        private string best1Text;
        private string best2Text;
        private string best3Text;
        private string best4Text;
        private string best5Text;
        private string best6Text;
        private bool changingTimeframe;
        /*
                private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        */
        private int curSeason = MainWindow.curSeason;
        private string filterDescription;
        private TeamFilter filterType;
        private string pl_best1Text, pl_best2Text, pl_best3Text, pl_best4Text, pl_best5Text, pl_best6Text;
        private string pl_sCText;
        private string pl_sPFText;
        private string pl_sPGText;
        private string pl_sSFText;
        private string pl_sSGText;
        private string pl_sSubsText;
        private bool reload;
        private string sCText;
        private string sPFText;
        private string sPGText;
        private string sSFText;
        private string sSGText;
        private string sSubsText;

        private List<string> uTeamCriteria = new List<string> {"All Players", "League Leaders", "My League Leaders"};

        private List<string> uTeamOptions = new List<string>
                                            {
                                                "All-League 1st Team",
                                                "All-League 2nd Team",
                                                "All-League 3rd Team",
                                                "All-Rookies 1st Team",
                                                "All-Rookies 2nd Team"
                                            };

        /// <summary>
        ///     Initializes a new instance of the <see cref="LeagueOverviewWindow" /> class.
        /// </summary>
        public LeagueOverviewWindow()
        {
            InitializeComponent();

            Height = Misc.GetRegistrySetting("LeagueOvHeight", (int) Height);
            Width = Misc.GetRegistrySetting("LeagueOvWidth", (int) Width);
            Top = Misc.GetRegistrySetting("LeagueOvY", (int) Top);
            Left = Misc.GetRegistrySetting("LeagueOvX", (int) Left);

            dt_bs = new DataTable();

            dt_bs.Columns.Add("Date");
            dt_bs.Columns.Add("Away");
            dt_bs.Columns.Add("AS", typeof (int));
            dt_bs.Columns.Add("Home");
            dt_bs.Columns.Add("HS", typeof (int));
            dt_bs.Columns.Add("GameID");

            LinkInternalsToMainWindow();

            PopulateSeasonCombo();
            PopulateDivisionCombo();

            changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.tf.EndDate;
            dtpStart.SelectedDate = MainWindow.tf.StartDate;
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.tf.SeasonNum);
            if (MainWindow.tf.isBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            cmbDivConf.SelectedIndex = 0;
            rbSeason.IsChecked = true;
            changingTimeframe = false;

            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaders.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            sem = new Semaphore(1, 1);
        }

        private DataView dv_ts { get; set; }
        private DataView dv_lts { get; set; }
        protected List<TeamStatsRow> pl_lssr { get; set; }
        protected List<TeamStatsRow> pl_tsrList { get; set; }
        protected List<TeamStatsRow> pl_oppTsrList { get; set; }

        protected List<TeamStatsRow> oppTsrList { get; set; }

        protected List<TeamStatsRow> lssr { get; set; }

        protected List<TeamStatsRow> tsrList { get; set; }

        private void PopulateSituationalsCombo()
        {
            int prevSitValue = cmbSituational.SelectedIndex;
            int prevCriValue = cmbUTCriteria.SelectedIndex;

            cmbSituational.SelectedIndex = -1;
            cmbUTCriteria.SelectedIndex = -1;

            cmbSituational.ItemsSource = uTeamOptions;
            cmbSituational.SelectedIndex = prevSitValue == -1 ? 0 : prevSitValue;

            cmbUTCriteria.ItemsSource = uTeamCriteria;
            cmbUTCriteria.SelectedIndex = prevCriValue == -1 ? 0 : prevCriValue;
        }

        /// <summary>
        ///     Populates the division combo.
        /// </summary>
        private void PopulateDivisionCombo()
        {
            var list = new List<ComboBoxItemWithIsEnabled>();
            list.Add(new ComboBoxItemWithIsEnabled("Whole League"));
            list.Add(new ComboBoxItemWithIsEnabled("-- Conferences --", false));
            foreach (var conf in MainWindow.Conferences)
            {
                list.Add(new ComboBoxItemWithIsEnabled(conf.Name));
            }
            list.Add(new ComboBoxItemWithIsEnabled("-- Divisions --", false));
            foreach (var div in MainWindow.Divisions)
            {
                Conference conf = MainWindow.Conferences.Find(conference => conference.ID == div.ConferenceID);
                list.Add(new ComboBoxItemWithIsEnabled(String.Format("{0}: {1}", conf.Name, div.Name)));
            }
            cmbDivConf.DisplayMemberPath = "Item";
            //cmbDivConf.SelectedValuePath = "Item";
            cmbDivConf.ItemsSource = list;
        }

        private void TryChangeRow(ref DataTable dt, int row, Dictionary<string, string> dict)
        {
            dt.Rows[row].TryChangeValue(dict, "Games", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "Wins", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "Losses", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "PF", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "PA", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "FGM", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "FGA", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "3PM", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "3PA", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "FTM", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "FTA", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "REB", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "OREB", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "DREB", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "AST", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "TO", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "STL", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "BLK", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "FOUL", typeof (UInt16));
            dt.Rows[row].TryChangeValue(dict, "MINS", typeof (UInt16));
        }

        /// <summary>
        ///     Finds the team's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private int GetTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(_tst, displayName);
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpStart control.
        ///     Makes sure that the starting date isn't before the ending date, and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                }
                MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                MainWindow.UpdateAllData();
                LinkInternalsToMainWindow();
                rbStatsBetween.IsChecked = true;
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
                changingTimeframe = false;
            }
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpEnd control.
        ///     Makes sure that the starting date isn't before the ending date, and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                }
                MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                MainWindow.UpdateAllData();
                LinkInternalsToMainWindow();
                rbStatsBetween.IsChecked = true;
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
                changingTimeframe = false;
            }
        }

        /// <summary>
        ///     Populates the season combo.
        /// </summary>
        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the tbcLeagueOverview control.
        ///     Handles tab changes, and refreshes the data if required (e.g. on season/time-range changes).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void tbcLeagueOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            if (reload || e.OriginalSource is TabControl)
            {
                object currentTab = tbcLeagueOverview.SelectedItem;
                if (currentTab == tabTeamStats || currentTab == tabTeamMetricStats)
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
                else if (currentTab == tabPlayerStats || currentTab == tabMetricStats || currentTab == tabBest ||
                         currentTab == tabStartingFive || currentTab == tabRatings || currentTab == tabContracts || currentTab == tabLeaders ||
                         currentTab == tabMyLeaders)
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
                else if (currentTab == tabBoxScores)
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
        ///     Prepares and presents the player stats.
        /// </summary>
        /// <param name="leaders">
        ///     if set to <c>true</c>, the stats are calculated based on the NBA rules for League Leaders standings.
        /// </param>
        private void PreparePlayerStats()
        {
            List<PlayerStatsRow> lpsr;
            _psrList = new List<PlayerStatsRow>();
            lpsr = new List<PlayerStatsRow>();

            List<PlayerStatsRow> pl_lpsr;
            _plPsrList = new List<PlayerStatsRow>();
            pl_lpsr = new List<PlayerStatsRow>();

            _leadersList = new List<PlayerStatsRow>();
            _plLeadersList = new List<PlayerStatsRow>();

            _myLeadersList = new List<PlayerStatsRow>();
            _plMyLeadersList = new List<PlayerStatsRow>();

            var worker1 = new BackgroundWorker {WorkerReportsProgress = true};

            txbStatus.FontWeight = FontWeights.Bold;
            txbStatus.Text = "Please wait while player averages and metric stats are being calculated...";

            int i = 0;

            int playerCount = -1;

            worker1.DoWork += delegate
                              {
                                  sem.WaitOne();
                                  _psrList = new List<PlayerStatsRow>();
                                  lpsr = new List<PlayerStatsRow>();

                                  _plPsrList = new List<PlayerStatsRow>();
                                  pl_lpsr = new List<PlayerStatsRow>();

                                  playerCount = _pst.Count;
                                  foreach (var kvp in _pst)
                                  {
                                      if (kvp.Value.isHidden)
                                          continue;
                                      var psr = new PlayerStatsRow(kvp.Value);
                                      var pl_psr = new PlayerStatsRow(kvp.Value, true);

                                      if (psr.isActive)
                                      {
                                          if (!InCurrentFilter(_tst[psr.TeamF]))
                                              continue;
                                          psr.TeamFDisplay = _tst[psr.TeamF].displayName;
                                          pl_psr.TeamFDisplay = psr.TeamFDisplay;
                                      }
                                      else
                                      {
                                          if (filterType != TeamFilter.League)
                                              continue;

                                          psr.TeamFDisplay = "- Inactive -";
                                          pl_psr.TeamFDisplay = psr.TeamFDisplay;
                                      }
                                      _psrList.Add(psr);
                                      _plPsrList.Add(pl_psr);
                                      worker1.ReportProgress(1);
                                  }
                                  PlayerStats leagueAverages = PlayerStats.CalculateLeagueAverages(_pst, _tst);
                                  lpsr.Add(new PlayerStatsRow(leagueAverages));
                                  pl_lpsr.Add(new PlayerStatsRow(leagueAverages, true));

                                  _psrList.Sort((psr1, psr2) => psr1.GmSc.CompareTo(psr2.GmSc));
                                  _psrList.Reverse();

                                  _plPsrList.Sort((psr1, psr2) => psr1.GmSc.CompareTo(psr2.GmSc));
                                  _plPsrList.Reverse();

                                  foreach (var psr in _psrList)
                                  {
                                      if (psr.isActive)
                                      {
                                          _leadersList.Add(psr.ConvertToLeagueLeader(_tst));
                                          _myLeadersList.Add(psr.ConvertToMyLeagueLeader(_tst));
                                      }
                                  }
                                  foreach (var psr in _plPsrList)
                                  {
                                      if (psr.isActive)
                                      {
                                          _plLeadersList.Add(psr.ConvertToLeagueLeader(_tst, true));
                                          _plMyLeadersList.Add(psr.ConvertToMyLeagueLeader(_tst, true));
                                      }
                                  }

                                  _leadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                  _leadersList.Reverse();

                                  _plLeadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                  _plLeadersList.Reverse();

                                  _myLeadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                  _myLeadersList.Reverse();

                                  _plMyLeadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                  _plMyLeadersList.Reverse();
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
                                              bool isSeason = rbSeason.IsChecked.GetValueOrDefault();
                                              dgvPlayerStats.ItemsSource = isSeason ? _psrList : _plPsrList;
                                              dgvLeaguePlayerStats.ItemsSource = isSeason ? lpsr : pl_lpsr;
                                              dgvMetricStats.ItemsSource = isSeason ? _psrList : _plPsrList;
                                              dgvLeagueMetricStats.ItemsSource = isSeason ? lpsr : pl_lpsr;
                                              dgvRatings.ItemsSource = isSeason ? _psrList : _plPsrList;
                                              dgvContracts.ItemsSource = isSeason ? _psrList : _plPsrList;
                                              dgvLeaders.ItemsSource = isSeason ? _leadersList : _plLeadersList;
                                              dgvMyLeaders.ItemsSource = isSeason ? _myLeadersList : _plMyLeadersList;

                                              PopulateSituationalsCombo();

                                              UpdateUltimateTeamTextboxes(isSeason);
                                              UpdateBestPerformersTextboxes(isSeason);

                                              tbcLeagueOverview.Visibility = Visibility.Visible;
                                              txbStatus.FontWeight = FontWeights.Normal;
                                              txbStatus.Text = message;
                                              sem.Release();
                                          };

            tbcLeagueOverview.Visibility = Visibility.Hidden;
            worker1.RunWorkerAsync();
        }

        private void UpdateBestPerformersTextboxes(bool isSeason)
        {
            txbPlayer1.Text = isSeason ? best1Text : pl_best1Text;
            txbPlayer2.Text = isSeason ? best2Text : pl_best2Text;
            txbPlayer3.Text = isSeason ? best3Text : pl_best3Text;
            txbPlayer4.Text = isSeason ? best4Text : pl_best4Text;
            txbPlayer5.Text = isSeason ? best5Text : pl_best5Text;
            txbPlayer6.Text = isSeason ? best6Text : pl_best6Text;
        }

        private void UpdateUltimateTeamTextboxes(bool isSeason)
        {
            txbStartingPG.Text = isSeason ? sPGText : pl_sPGText;
            txbStartingSG.Text = isSeason ? sSGText : pl_sSGText;
            txbStartingSF.Text = isSeason ? sSFText : pl_sSFText;
            txbStartingPF.Text = isSeason ? sPFText : pl_sPFText;
            txbStartingC.Text = isSeason ? sCText : pl_sCText;
            txbSubs.Text = isSeason ? sSubsText : pl_sSubsText;
        }

        /// <summary>
        ///     Prepares and presents the best performers' stats.
        /// </summary>
        /// <param name="pmsrList">The list of currently loaded PlayerMetricStatsRow instances.</param>
        /// <param name="pl_pmsrList">The list of currently loaded playoff PlayerMetricStatsRow instances.</param>
        private void CalculateBestPerformers(List<PlayerStatsRow> pmsrList, List<PlayerStatsRow> pl_pmsrList)
        {
            best1Text = "";
            best2Text = "";
            best3Text = "";
            best4Text = "";
            best5Text = "";
            best6Text = "";

            var templist = new List<PlayerStatsRow>();
            try
            {
                templist = pmsrList.ToList();
                templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                templist.Reverse();

                PlayerStatsRow psr1 = templist[0];
                string text = psr1.GetBestStats(5);
                best1Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                            text;

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(5);
                best2Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + " - " + psr2.TeamFDisplay + ")\n\n" +
                            text;

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(5);
                best3Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + " - " + psr3.TeamFDisplay + ")\n\n" +
                            text;

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(5);
                best4Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + " - " + psr4.TeamFDisplay + ")\n\n" +
                            text;

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(5);
                best5Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + " - " + psr5.TeamFDisplay + ")\n\n" +
                            text;

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(5);
                best6Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + " - " + psr6.TeamFDisplay + ")\n\n" +
                            text;
            }
            catch (Exception)
            {
            }


            pl_best1Text = "";
            pl_best2Text = "";
            pl_best3Text = "";
            pl_best4Text = "";
            pl_best5Text = "";
            pl_best6Text = "";

            templist = new List<PlayerStatsRow>();
            try
            {
                templist = pl_pmsrList.ToList();
                templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                templist.Reverse();

                PlayerStatsRow psr1 = templist[0];
                string text = psr1.GetBestStats(5);
                pl_best1Text = string.Format("1: {0} {1} ({2} - {3})\n\n{4}", psr1.FirstName, psr1.LastName, psr1.Position1,
                                             psr1.TeamFDisplay, text);

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(5);
                pl_best2Text = string.Format("2: {0} {1} ({2} - {3})\n\n{4}", psr2.FirstName, psr2.LastName, psr2.Position1,
                                             psr2.TeamFDisplay, text);

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(5);
                pl_best3Text = string.Format("3: {0} {1} ({2} - {3})\n\n{4}", psr3.FirstName, psr3.LastName, psr3.Position1,
                                             psr3.TeamFDisplay, text);

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(5);
                pl_best4Text = string.Format("4: {0} {1} ({2} - {3})\n\n{4}", psr4.FirstName, psr4.LastName, psr4.Position1,
                                             psr4.TeamFDisplay, text);

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(5);
                pl_best5Text = string.Format("5: {0} {1} ({2} - {3})\n\n{4}", psr5.FirstName, psr5.LastName, psr5.Position1,
                                             psr5.TeamFDisplay, text);

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(5);
                pl_best6Text = string.Format("6: {0} {1} ({2} - {3})\n\n{4}", psr6.FirstName, psr6.LastName, psr6.Position1,
                                             psr6.TeamFDisplay, text);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Calculates the best starting five for the current scope.
        /// </summary>
        /// <param name="psrList">The list of currently loaded PlayerStatsRow instances, sorted by GmSc in descending order.</param>
        /// <param name="type"></param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the starting five will be determined based on their playoff performances.
        /// </param>
        private void CalculateUltimateTeam(List<PlayerStatsRow> psrList, string type, bool playoffs = false)
        {
            if (!playoffs)
            {
                sPGText = "";
                sSGText = "";
                sSFText = "";
                sPFText = "";
                sCText = "";
                sSubsText = "";
            }
            else
            {
                pl_sPGText = "";
                pl_sSGText = "";
                pl_sSFText = "";
                pl_sPFText = "";
                pl_sCText = "";
                pl_sSubsText = "";
            }

            if (type.StartsWith("All-Rookie"))
            {
                psrList = psrList.Where(ps => ps.YearsPro == 1).ToList();
            }
            psrList = psrList.OrderByDescending(ps => ps.GmSc).ToList();

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            List<PlayerStatsRow> PGList =
                psrList.Where(row => (row.Position1 == Position.PG || row.Position2 == Position.PG)) // && row.isInjured == false)
                       .Take(10).ToList();
            PGList = PGList.OrderByDescending(ps => ps.GmSc).ToList();
            List<PlayerStatsRow> SGList =
                psrList.Where(row => (row.Position1 == Position.SG || row.Position2 == Position.SG)) // && row.isInjured == false)
                       .Take(10).ToList();
            SGList = SGList.OrderByDescending(ps => ps.GmSc).ToList();
            List<PlayerStatsRow> SFList =
                psrList.Where(row => (row.Position1 == Position.SF || row.Position2 == Position.SF)) // && row.isInjured == false)
                       .Take(10).ToList();
            SFList = SFList.OrderByDescending(ps => ps.GmSc).ToList();
            List<PlayerStatsRow> PFList =
                psrList.Where(row => (row.Position1 == Position.PF || row.Position2 == Position.PF)) // && row.isInjured == false)
                       .Take(10).ToList();
            PFList = PFList.OrderByDescending(ps => ps.GmSc).ToList();
            List<PlayerStatsRow> CList =
                psrList.Where(row => (row.Position1 == Position.C || row.Position2 == Position.C)) // && row.isInjured == false)
                       .Take(10).ToList();
            CList = CList.OrderByDescending(ps => ps.GmSc).ToList();
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
                                if (PGList[i1].Position1S == "PG")
                                    _pInP++;
                                if (perm.Contains(SGList[i2].ID))
                                {
                                    continue;
                                }
                                perm.Add(SGList[i2].ID);
                                _sum += SGList[i2].GmSc;
                                if (SGList[i2].Position1S == "SG")
                                    _pInP++;
                                if (perm.Contains(SFList[i3].ID))
                                {
                                    continue;
                                }
                                perm.Add(SFList[i3].ID);
                                _sum += SFList[i3].GmSc;
                                if (SFList[i3].Position1S == "SF")
                                    _pInP++;
                                if (perm.Contains(PFList[i4].ID))
                                {
                                    continue;
                                }
                                perm.Add(PFList[i4].ID);
                                _sum += PFList[i4].GmSc;
                                if (PFList[i4].Position1S == "PF")
                                    _pInP++;
                                if (perm.Contains(CList[i5].ID))
                                {
                                    continue;
                                }
                                perm.Add(CList[i5].ID);
                                _sum += CList[i5].GmSc;
                                if (CList[i5].Position1S == "C")
                                    _pInP++;

                                if (_sum > max)
                                    max = _sum;

                                permutations.Add(new StartingFivePermutation {IDList = perm, PlayersInPrimaryPosition = _pInP, Sum = _sum});
                            }

            var bestPerm = new StartingFivePermutation();
            var benchPerm = new StartingFivePermutation();
            var thirdPerm = new StartingFivePermutation();
            try
            {
                bestPerm =
                    permutations.Where(perm1 => perm1.Sum.Equals(max)).OrderByDescending(perm2 => perm2.PlayersInPrimaryPosition).First();
                if (type.Contains("1st"))
                {
                    bestPerm.IDList.ForEach(i1 => tempList.Add(psrList.Single(row => row.ID == i1)));
                }
                else
                {
                    benchPerm =
                        permutations.Where(p => !(p.IDList.Any(id => bestPerm.IDList.Contains(id))))
                                    .OrderByDescending(p => p.Sum)
                                    .ThenByDescending(p => p.PlayersInPrimaryPosition)
                                    .First();
                    if (type.Contains("2nd"))
                    {
                        benchPerm.IDList.ForEach(i1 => tempList.Add(psrList.Single(row => row.ID == i1)));
                    }
                    else
                    {
                        thirdPerm =
                            permutations.Where(p => !(p.IDList.Any(id => bestPerm.IDList.Contains(id) || benchPerm.IDList.Contains(id))))
                                        .OrderByDescending(p => p.Sum)
                                        .ThenByDescending(p => p.PlayersInPrimaryPosition)
                                        .First();
                        thirdPerm.IDList.ForEach(i1 => tempList.Add(psrList.Single(row => row.ID == i1)));
                    }
                }
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
                    sPGText = displayText;
                else
                    pl_sPGText = displayText;
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
                    sSGText = displayText;
                else
                    pl_sSGText = displayText;
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
                    sSFText = displayText;
                else
                    pl_sSFText = displayText;
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
                    sPFText = displayText;
                else
                    pl_sPFText = displayText;
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
                    sCText = displayText;
                else
                    pl_sCText = displayText;
            }
            catch (Exception)
            {
            }

            // Subs
            List<int> usedIDs;
            if (type.Contains("1st"))
            {
                usedIDs = new List<int>();
                usedIDs.AddRange(bestPerm.IDList);
            }
            else if (type.Contains("2nd"))
            {
                usedIDs = new List<int>();
                usedIDs.AddRange(bestPerm.IDList);
                usedIDs.AddRange(benchPerm.IDList);
            }
            else
            {
                usedIDs = new List<int>();
                usedIDs.AddRange(bestPerm.IDList);
                usedIDs.AddRange(benchPerm.IDList);
                usedIDs.AddRange(thirdPerm.IDList);
            }
            int idCountToSkip = usedIDs.Count;
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
                int count = usedIDs.Count - idCountToSkip + 5;
                for (int j = count + 1; j <= 12; j++)
                {
                    i = 0;
                    while (usedIDs.Contains(psrList[i].ID))
                    {
                        i++;
                    }
                    usedIDs.Add(psrList[i].ID);
                }
            }
            catch (Exception)
            {
            }

            usedIDs.Skip(idCountToSkip).ToList().ForEach(id => tempList.Add(psrList.Single(row => row.ID == id)));
            displayText = "Subs: ";
            for (i = 5; i < usedIDs.Count - idCountToSkip + 5; i++)
            {
                psr1 = tempList[i];
                //text = psr1.GetBestStats(5);
                displayText += psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + "), ";
            }
            displayText = displayText.TrimEnd(new[] {' ', ','});

            if (!playoffs)
                sSubsText = displayText;
            else
                pl_sSubsText = displayText;
        }

        /// <summary>
        ///     Prepares and presents the available box scores.
        /// </summary>
        private void PrepareBoxScores()
        {
            dt_bs.Clear();

            foreach (var bse in _bshist)
            {
                if (!InCurrentFilter(bse.bs.Team1ID) && !InCurrentFilter(bse.bs.Team2ID))
                {
                    continue;
                }
                if (rbSeason.IsChecked.GetValueOrDefault())
                {
                    if (bse.bs.isPlayoff)
                        continue;
                }
                else
                {
                    if (!bse.bs.isPlayoff)
                        continue;
                }

                DataRow r = dt_bs.NewRow();

                try
                {
                    r["Date"] = bse.bs.gamedate.ToString().Split(' ')[0];
                    r["Away"] = MainWindow.tst[bse.bs.Team1ID].displayName;
                    r["AS"] = Convert.ToInt32(bse.bs.PTS1);
                    r["Home"] = MainWindow.tst[bse.bs.Team2ID].displayName;
                    r["HS"] = Convert.ToInt32(bse.bs.PTS2);
                    r["GameID"] = bse.bs.id;
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
        ///     Prepares and presents the team stats.
        /// </summary>
        private void PrepareTeamStats()
        {
            tsrList = new List<TeamStatsRow>();
            lssr = new List<TeamStatsRow>();
            oppTsrList = new List<TeamStatsRow>();

            var ls = new TeamStats(-1, "League");

            foreach (var key in _tst.Keys)
            {
                if (_tst[key].isHidden)
                    continue;

                if (!InCurrentFilter(_tst[key]))
                    continue;

                tsrList.Add(new TeamStatsRow(_tst[key]));
                oppTsrList.Add(new TeamStatsRow(_tstopp[key]));
            }

            ls = TeamStats.CalculateLeagueAverages(_tst, Span.Season);

            lssr.Add(new TeamStatsRow(ls));

            tsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            tsrList.Reverse();
            oppTsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            //oppTsrList.Reverse();

            pl_tsrList = new List<TeamStatsRow>();
            pl_lssr = new List<TeamStatsRow>();
            pl_oppTsrList = new List<TeamStatsRow>();

            var ls1 = new TeamStats(-1, "League");

            foreach (var key1 in _tst.Keys)
            {
                if (_tst[key1].isHidden)
                    continue;
                if (_tst[key1].getPlayoffGames() == 0)
                    continue;
                if (!InCurrentFilter(_tst[key1]))
                    continue;

                pl_tsrList.Add(new TeamStatsRow(_tst[key1], true));
                pl_oppTsrList.Add(new TeamStatsRow(_tstopp[key1], true));
            }

            ls1 = TeamStats.CalculateLeagueAverages(_tst, Span.Playoffs);

            pl_lssr.Add(new TeamStatsRow(ls1, true));

            pl_tsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            pl_tsrList.Reverse();
            pl_oppTsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            //pl_oppTsrList.Reverse();

            bool isSeason = rbSeason.IsChecked.GetValueOrDefault();

            dgvTeamStats.ItemsSource = isSeason ? tsrList : pl_tsrList;
            dgvLeagueTeamStats.ItemsSource = isSeason ? lssr : pl_lssr;

            dgvTeamMetricStats.ItemsSource = isSeason ? tsrList : pl_tsrList;
            dgvLeagueTeamMetricStats.ItemsSource = isSeason ? lssr : pl_lssr;

            dgvOpponentStats.ItemsSource = isSeason ? oppTsrList : pl_oppTsrList;
            dgvLeagueOpponentStats.ItemsSource = isSeason ? lssr : pl_lssr;

            dgvOpponentMetricStats.ItemsSource = isSeason ? oppTsrList : pl_oppTsrList;
            dgvLeagueOpponentMetricStats.ItemsSource = isSeason ? lssr : pl_lssr;
        }

        /// <summary>
        ///     Determines whether a specific team should be shown or not, based on the current filter.
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
                foreach (var conf in MainWindow.Conferences)
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
        ///     Determines whether a specific team should be shown or not, based on the current filter.
        /// </summary>
        /// <param name="teamID">Name of the team.</param>
        /// <returns>
        ///     true if it should be shown; otherwise, false
        /// </returns>
        private bool InCurrentFilter(int teamID)
        {
            if (filterType == TeamFilter.League)
                return true;

            DataTable res = db.GetDataTable("SELECT Division FROM Teams WHERE ID = " + teamID);
            int divID = Tools.getInt(res.Rows[0], "Division");

            if (filterType == TeamFilter.Conference)
            {
                int confID = -1;
                foreach (var conf in MainWindow.Conferences)
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
        ///     Handles the Checked event of the rbStatsAllTime control.
        ///     Changes the timeframe to the whole season, forces all tabs to be reloaded on first request, and reloads the current one.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                reload = true;
                MainWindow.tf = new Timeframe(curSeason);
                MainWindow.UpdateAllData();
                LinkInternalsToMainWindow();
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsBetween control.
        ///     Changes the timeframe to be between the specified dates, forces all tabs to be reloaded on first request, and reloads the current one.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                reload = true;
                MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                MainWindow.UpdateAllData();
                LinkInternalsToMainWindow();
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        /// <summary>
        ///     Handles the LoadingRow event of the dg control.
        ///     Adds a ranking number to the row's header.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridRowEventArgs" /> instance containing the event data.
        /// </param>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control.
        ///     Loads all the required information for the new season and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1)
                return;

            curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

            if (curSeason == MainWindow.tf.SeasonNum && !MainWindow.tf.isBetween)
                return;

            SQLiteIO.LoadSeason(MainWindow.currentDB, curSeason);
            LinkInternalsToMainWindow();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                reload = true;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        private void LinkInternalsToMainWindow()
        {
            _tst = MainWindow.tst;
            _tstopp = MainWindow.tstopp;
            _pst = MainWindow.pst;
            _bshist = MainWindow.bshist;
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvBoxScores control.
        ///     Views the selected box score in the Box Score Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
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
        ///     Handles the MouseDoubleClick event of the AnyTeamDataGrid control.
        ///     Views the selected team in the Team Overview Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void AnyTeamDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventHandlers.AnyTeamDataGrid_MouseDoubleClick(sender, e);
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the AnyPlayerDataGrid control.
        ///     Views the selected player in the Player Overview Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void AnyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventHandlers.AnyPlayerDataGrid_MouseDoubleClick(sender, e);
        }

        /// <summary>
        ///     Handles the Loaded event of the Window control.
        ///     Forces all tabs to be reloaded on first request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstopp, MainWindow.TeamOrder, true);
            lastShownPlayerSeason = 0;
            lastShownTeamSeason = curSeason;
            lastShownBoxSeason = 0;
            message = txbStatus.Text;
        }

        /// <summary>
        ///     Handles the Closing event of the Window control.
        ///     Forces all tabs to be reloaded on first request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="CancelEventArgs" /> instance containing the event data.
        /// </param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            lastShownTeamSeason = 0;
            lastShownPlayerSeason = 0;
            lastShownBoxSeason = 0;

            Misc.SetRegistrySetting("LeagueOvHeight", Height);
            Misc.SetRegistrySetting("LeagueOvWidth", Width);
            Misc.SetRegistrySetting("LeagueOvX", Left);
            Misc.SetRegistrySetting("LeagueOvY", Top);
        }

        /// <summary>
        ///     Handles the Sorting event of the StatColumn control.
        ///     Uses a custom Sorting event handler that sorts a stat column in descending order, if it's not already sorted.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvTeamMetricStats control.
        ///     Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvTeamMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamMetricStats.Columns.Count && i < dgvLeagueTeamMetricStats.Columns.Count; ++i)
                dgvLeagueTeamMetricStats.Columns[i].Width = dgvTeamMetricStats.Columns[i].ActualWidth;
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvTeamStats control.
        ///     Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvTeamStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamStats.Columns.Count && i < dgvLeagueTeamStats.Columns.Count; ++i)
                dgvLeagueTeamStats.Columns[i].Width = dgvTeamStats.Columns[i].ActualWidth;
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvPlayerStats control.
        ///     Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvPlayerStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvPlayerStats.Columns.Count && i < dgvLeaguePlayerStats.Columns.Count; ++i)
                dgvLeaguePlayerStats.Columns[i].Width = dgvPlayerStats.Columns[i].ActualWidth;
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvMetricStats control.
        ///     Used to synchronize the column width between the teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvMetricStats.Columns.Count && i < dgvLeagueMetricStats.Columns.Count; ++i)
                dgvLeagueMetricStats.Columns[i].Width = dgvMetricStats.Columns[i].ActualWidth;
        }

        /// <summary>
        ///     Handles the LoadingRow event of the dgLeague control.
        ///     Adds an "L" to the row header for the league average row.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridRowEventArgs" /> instance containing the event data.
        /// </param>
        private void dgLeague_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = "L";
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbDivConf control.
        ///     Applies the new filter, forces all tabs to reload on first request and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
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
            lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Checked event of the rbSeason control.
        ///     Switches the visibility of the Season tabs to visible, and of the Playoff tabs to hidden.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbSeason_Checked(object sender, RoutedEventArgs e)
        {
            if (changingTimeframe)
                return;

            reload = true;
            lastShownTeamSeason = 0;
            lastShownPlayerSeason = 0;
            lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Checked event of the rbPlayoffs control.
        ///     Switches the visibility of the Season tabs to hidden, and of the Playoff tabs to visible.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbPlayoffs_Checked(object sender, RoutedEventArgs e)
        {
            if (changingTimeframe)
                return;

            reload = true;
            lastShownTeamSeason = 0;
            lastShownPlayerSeason = 0;
            lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        private void btnExportLRERatings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select the TSV file of your roster";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            string file = ofd.FileName;

            List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSVFile(file);

            List<PlayerStatsRow> plist = rbSeason.IsChecked.GetValueOrDefault() ? _psrList : _plPsrList;

            List<string> ratingNames =
                dgvRatings.Columns.ToList()
                          .SkipWhile(c => c.Header.ToString() != "RatingsStart")
                          .Skip(1)
                          .Select(c => c.Header.ToString())
                          .ToList();

            foreach (var ps in plist)
            {
                List<Dictionary<string, string>> pInsts =
                    dictList.FindAll(dict => dict["Name"].ToLowerInvariant() == (ps.FirstName + " " + ps.LastName).ToLowerInvariant())
                            .ToList();
                foreach (var pInst in pInsts)
                {
                    foreach (var ratingName in ratingNames)
                    {
                        int rating = Convert.ToInt32(typeof (PlayerStatsRow).GetProperty("re" + ratingName).GetValue(ps, null));
                        if (rating != -1)
                        {
                            pInst[ratingName] = rating.ToString();
                        }
                    }
                }
            }

            CSV.TSVFromDictionaryList(dictList, file);

            MessageBox.Show("Successfully updated Roster TSV with calculated ratings.", "NBA Stats Tracker", MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void dgvPlayerStats_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                bool isSeason = rbSeason.IsChecked.GetValueOrDefault();
                List<PlayerStatsRow> list = isSeason ? _psrList : _plPsrList;
                for (int j = 0; j < dictList.Count; j++)
                {
                    Dictionary<string, string> dict = dictList[j];
                    int ID;
                    try
                    {
                        ID = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ID =
                                MainWindow.pst.Values.Single(ps => ps.LastName == dict["Last Name"] && ps.FirstName == dict["First Name"])
                                          .ID;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Player in row " + j +
                                            " couldn't be determined either by ID or Full Name. Make sure the pasted data has the proper headers. " +
                                            "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        PlayerStatsRow psr = list.Single(ps => ps.ID == ID);
                        PlayerStatsRow.TryChangePSR(ref psr, dict);
                        PlayerStatsRow.Refresh(ref psr);
                        list[list.FindIndex(ts => ts.ID == ID)] = psr;
                        MainWindow.pst[ID] = new PlayerStats(psr, !isSeason);
                    }
                    catch
                    {
                        continue;
                    }
                }

                ((DataGrid) sender).ItemsSource = null;
                ((DataGrid) sender).ItemsSource = list;

                MessageBox.Show("Data pasted successfully! Remember to save!", "NBA Stats Tracker", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void AnyTeamDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                bool isSeason = rbSeason.IsChecked.GetValueOrDefault();
                List<TeamStatsRow> list = isSeason ? tsrList : pl_tsrList;

                for (int j = 0; j < dictList.Count; j++)
                {
                    Dictionary<string, string> dict = dictList[j];
                    int ID;
                    try
                    {
                        ID = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ID = MainWindow.tst.Values.Single(ts => ts.displayName == dict["Team"]).ID;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Team in row " + (j + 1) +
                                            " couldn't be determined either by ID or Name. Make sure the pasted data has the proper headers. " +
                                            "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        TeamStatsRow tsr = list.Single(ts => ts.ID == ID);
                        TeamStatsRow.TryChangeTSR(ref tsr, dict);
                        TeamStatsRow.Refresh(ref tsr);
                        list[list.FindIndex(ts => ts.ID == ID)] = tsr;
                        MainWindow.tst[tsr.ID] = new TeamStats(tsr, !isSeason);
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

        private void dgvOpponentStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvOpponentStats.Columns.Count && i < dgvLeagueOpponentStats.Columns.Count; ++i)
                dgvLeagueOpponentStats.Columns[i].Width = dgvOpponentStats.Columns[i].ActualWidth;
        }

        private void dgvOpponentMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvOpponentMetricStats.Columns.Count && i < dgvLeagueOpponentMetricStats.Columns.Count; ++i)
                dgvLeagueOpponentMetricStats.Columns[i].Width = dgvOpponentMetricStats.Columns[i].ActualWidth;
        }

        private void btnSetRatingsCriteria_Click(object sender, RoutedEventArgs e)
        {
            var ibw =
                new InputBoxWindow(
                    "Enter percentage (0-100) of team's games played the player must have " +
                    "participated in (-1 not to use this criterion)", SQLiteIO.GetSetting("RatingsGPPct", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("RatingsGPPct", MainWindow.input);
            }

            ibw = new InputBoxWindow("Enter minimum amount of minutes per game played by the player (-1 not to use this criterion)",
                                     SQLiteIO.GetSetting("RatingsMPG", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("RatingsMPG", MainWindow.input);
            }

            MainWindow.LoadRatingsCriteria();

            PreparePlayerStats();
        }

        private void btnExportLREDitorRatings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select the Players.csv file of your REDitor-exported save";
            ofd.Filter = "Comma-separated Values Files (*.csv)|*.csv";
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            string file = ofd.FileName;

            List<Dictionary<string, string>> dictList = CSV.DictionaryListFromCSVFile(file);

            List<PlayerStatsRow> plist = rbSeason.IsChecked.GetValueOrDefault() ? _psrList : _plPsrList;

            List<string> ratingNames =
                dgvRatings.Columns.ToList()
                          .SkipWhile(c => c.Header.ToString() != "RatingsStart")
                          .Skip(1)
                          .Select(c => c.Header.ToString())
                          .ToList();

            foreach (var ps in plist)
            {
                List<Dictionary<string, string>> pInsts =
                    dictList.FindAll(
                        dict =>
                        dict["First_Name"].ToLowerInvariant() == ps.FirstName.ToLowerInvariant() &&
                        dict["Last_Name"].ToLowerInvariant() == ps.LastName.ToLowerInvariant()).ToList();
                foreach (var pInst in pInsts)
                {
                    foreach (var ratingName in ratingNames)
                    {
                        int rating = Convert.ToInt32(typeof (PlayerStatsRow).GetProperty("re" + ratingName).GetValue(ps, null));
                        if (rating != -1)
                        {
                            pInst[REtoREDitor[ratingName]] = rating.ToString();
                        }
                    }
                }
            }

            CSV.CSVFromDictionaryList(dictList, file);

            MessageBox.Show("Successfully updated Players CSV with calculated ratings.", "NBA Stats Tracker", MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void btnSetMyLeadersCriteria_Click(object sender, RoutedEventArgs e)
        {
            var ibw =
                new InputBoxWindow(
                    "Enter percentage (0-100) of team's games played the player must have " +
                    "participated in (-1 not to use this criterion)", SQLiteIO.GetSetting("MyLeadersGPPct", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("MyLeadersGPPct", MainWindow.input);
            }

            ibw = new InputBoxWindow("Enter minimum amount of minutes per game played by the player (-1 not to use this criterion)",
                                     SQLiteIO.GetSetting("MyLeadersMPG", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("MyLeadersMPG", MainWindow.input);
            }

            MainWindow.LoadMyLeadersCriteria();

            PreparePlayerStats();
        }

        private void cmbSituational_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSituational.SelectedIndex == -1 || cmbUTCriteria.SelectedIndex == -1)
                return;

            PrepareUltimateTeam();
        }

        private void PrepareUltimateTeam()
        {
            switch (cmbUTCriteria.SelectedItem.ToString())
            {
                case "All Players":
                    CalculateUltimateTeam(_psrList, cmbSituational.SelectedItem.ToString());
                    CalculateUltimateTeam(_plPsrList, cmbSituational.SelectedItem.ToString(), true);
                    break;
                case "League Leaders":
                    CalculateUltimateTeam(_leadersList, cmbSituational.SelectedItem.ToString());
                    CalculateUltimateTeam(_plLeadersList, cmbSituational.SelectedItem.ToString(), true);
                    break;
                case "My League Leaders":
                    CalculateUltimateTeam(_myLeadersList, cmbSituational.SelectedItem.ToString());
                    CalculateUltimateTeam(_plMyLeadersList, cmbSituational.SelectedItem.ToString(), true);
                    break;
            }

            UpdateUltimateTeamTextboxes(rbSeason.IsChecked.GetValueOrDefault());
        }

        private void cmbUTCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUTCriteria.SelectedIndex == -1 || cmbSituational.SelectedIndex == -1)
                return;

            PrepareUltimateTeam();
            PrepareBestPerformers();
        }

        private void PrepareBestPerformers()
        {
            switch (cmbUTCriteria.SelectedItem.ToString())
            {
                case "All Players":
                    CalculateBestPerformers(_psrList, _plPsrList);
                    break;
                case "League Leaders":
                    CalculateBestPerformers(_leadersList, _plLeadersList);
                    break;
                case "My League Leaders":
                    CalculateBestPerformers(_myLeadersList, _plMyLeadersList);
                    break;
            }

            UpdateBestPerformersTextboxes(rbSeason.IsChecked.GetValueOrDefault());
        }

        #region Nested type: TeamFilter

        /// <summary>
        ///     Used to determine the filter that should be applied to which teams and players are included in the calculations
        ///     and shown in the DataGrids.
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