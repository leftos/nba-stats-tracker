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

namespace NBA_Stats_Tracker.Windows.MainInterface.League
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using LeftosCommonLibrary;
    using LeftosCommonLibrary.CommonDialogs;

    using Microsoft.Win32;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.EventHandlers;
    using NBA_Stats_Tracker.Helper.Miscellaneous;
    using NBA_Stats_Tracker.Windows.MainInterface.BoxScores;

    using SQLite_Database;

    #endregion

    /// <summary>Provides an overview of the whole league's stats. Allows filtering by division and conference.</summary>
    public partial class LeagueOverviewWindow
    {
        private static Dictionary<int, PlayerStats> _pst;
        private static Dictionary<int, TeamStats> _tst;
        private static List<BoxScoreEntry> _bsHist;
        private static int _lastShownPlayerSeason;
        private static int _lastShownTeamSeason;
        private static int _lastShownBoxSeason;
        private static string _message;
        private static Semaphore _sem;

        private readonly Dictionary<string, string> REtoREDitor = new Dictionary<string, string>
            {
                { "RFT", "SShtFT" },
                { "RPass", "SPass" },
                { "RBlock", "SBlock" },
                { "RSteal", "SSteal" },
                { "ROffRbd", "SOReb" },
                { "RDefRbd", "SDReb" },
                { "TShotTnd", "TShtTend" },
                { "TDrawFoul", "TDrawFoul" },
                { "TTouch", "TTouches" },
                { "TCommitFl", "TCommFoul" }
            };

        private readonly SQLiteDatabase _db = new SQLiteDatabase(MainWindow.CurrentDB);
        private readonly DataTable _dtBS;

        private readonly List<string> _uTeamCriteria = new List<string>
            {
                "All Players (GmSc)",
                "All Players (PER)",
                "League Leaders (GmSc)",
                "League Leaders (PER)",
                "My League Leaders (GmSc)",
                "My League Leaders (PER)"
            };

        private readonly List<string> _uTeamOptions = new List<string>
            {
                "All-League 1st Team",
                "All-League 2nd Team",
                "All-League 3rd Team",
                "All-Rookies 1st Team",
                "All-Rookies 2nd Team"
            };

        private string _best1Text;
        private string _best2Text;
        private string _best3Text;
        private string _best4Text;
        private string _best5Text;
        private string _best6Text;
        private bool _changingTimeframe;
        /*
                private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        */
        private int _curSeason = MainWindow.CurSeason;
        private string _filterDescription;
        private TeamFilter _filterType;
        private List<PlayerStatsRow> _lPSR;
        private List<PlayerStatsRow> _leadersList;
        private List<PlayerStatsRow> _myLeadersList;
        private string _plBest1Text, _plBest2Text, _plBest3Text, _plBest4Text, _plBest5Text, _plBest6Text;
        private string _plCText;
        private List<PlayerStatsRow> _plLeadersList;
        private List<PlayerStatsRow> _plLpsr;
        private List<PlayerStatsRow> _plMyLeadersList;
        private string _plPFText;
        private string _plPGText;
        private List<PlayerStatsRow> _plPSRList;
        private string _plSFText;
        private string _plSGText;
        private string _plSubsText;
        private List<PlayerStatsRow> _psrList;
        private bool _reload;
        private string _sCText;
        private string _sPFText;
        private string _sPGText;
        private string _sSFText;
        private string _sSGText;
        private string _sSubsText;

        private Dictionary<int, Dictionary<string, TeamStats>> _splitTeamStats;
        private Dictionary<int, TeamStats> _tstOpp;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LeagueOverviewWindow" /> class.
        /// </summary>
        public LeagueOverviewWindow()
        {
            InitializeComponent();

            Height = Tools.GetRegistrySetting("LeagueOvHeight", (int) Height);
            Width = Tools.GetRegistrySetting("LeagueOvWidth", (int) Width);
            Top = Tools.GetRegistrySetting("LeagueOvY", (int) Top);
            Left = Tools.GetRegistrySetting("LeagueOvX", (int) Left);

            _dtBS = new DataTable();

            _dtBS.Columns.Add("Date");
            _dtBS.Columns.Add("Away");
            _dtBS.Columns.Add("AS", typeof(int));
            _dtBS.Columns.Add("Home");
            _dtBS.Columns.Add("HS", typeof(int));
            _dtBS.Columns.Add("GameID");

            linkInternalsToMainWindow();

            populateSeasonCombo();
            populateDivisionCombo();

            _changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.Tf.EndDate;
            dtpStart.SelectedDate = MainWindow.Tf.StartDate;
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.Tf.SeasonNum);
            if (MainWindow.Tf.IsBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            cmbDivConf.SelectedIndex = 0;
            rbSeason.IsChecked = true;
            _changingTimeframe = false;

            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaders.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvRatings.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            _sem = new Semaphore(1, 1);
        }

        private List<TeamStatsRow> pl_Lssr { get; set; }
        private List<TeamStatsRow> pl_TSRList { get; set; }
        private List<TeamStatsRow> pl_OppTSRList { get; set; }

        private List<TeamStatsRow> oppTSRList { get; set; }
        private List<TeamStatsRow> lssr { get; set; }
        private List<TeamStatsRow> TSRList { get; set; }

        private void populateSituationalsCombo()
        {
            var prevSitValue = cmbSituational.SelectedIndex;
            var prevCriValue = cmbUTCriteria.SelectedIndex;

            cmbSituational.SelectedIndex = -1;
            cmbUTCriteria.SelectedIndex = -1;

            cmbSituational.ItemsSource = _uTeamOptions;
            cmbSituational.SelectedIndex = prevSitValue == -1 ? 0 : prevSitValue;

            cmbUTCriteria.ItemsSource = _uTeamCriteria;
            cmbUTCriteria.SelectedIndex = prevCriValue == -1 ? 0 : prevCriValue;
        }

        /// <summary>Populates the division combo.</summary>
        private void populateDivisionCombo()
        {
            var list = new List<ComboBoxItemWithIsEnabled>
                {
                    new ComboBoxItemWithIsEnabled("Whole League"),
                    new ComboBoxItemWithIsEnabled("-- Conferences --", false)
                };
            list.AddRange(MainWindow.Conferences.Select(conf => new ComboBoxItemWithIsEnabled(conf.Name)));
            list.Add(new ComboBoxItemWithIsEnabled("-- Divisions --", false));
            list.AddRange(
                from div in MainWindow.Divisions
                let conf = MainWindow.Conferences.Find(conference => conference.ID == div.ConferenceID)
                select new ComboBoxItemWithIsEnabled(String.Format("{0}: {1}", conf.Name, div.Name)));
            cmbDivConf.DisplayMemberPath = "Item";
            //cmbDivConf.SelectedValuePath = "Item";
            cmbDivConf.ItemsSource = list;
        }

        private void tryChangeRow(ref DataTable dt, int row, Dictionary<string, string> dict)
        {
            dt.Rows[row].TryChangeValue(dict, "Games", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "Wins", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "Losses", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "PF", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "PA", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "FGM", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "FGA", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "3PM", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "3PA", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "FTM", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "FTA", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "REB", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "OREB", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "DREB", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "AST", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "TO", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "STL", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "BLK", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "FOUL", typeof(UInt16));
            dt.Rows[row].TryChangeValue(dict, "MINS", typeof(UInt16));
        }

        /// <summary>Finds the team's name by its displayName.</summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private int getTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(_tst, displayName);
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpStart control. Makes sure that the starting date isn't before the ending
        ///     date, and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                }
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());

                updateData();
            }
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpEnd control. Makes sure that the starting date isn't before the ending date,
        ///     and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                }
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());

                updateData();
            }
        }

        private void updateData()
        {
            IsEnabled = false;
            Task.Factory.StartNew(() => MainWindow.UpdateAllData(true))
                .FailFastOnException(MainWindow.MWInstance.UIScheduler)
                .ContinueWith(t => linkInternalsToMainWindow())
                .FailFastOnException(MainWindow.MWInstance.UIScheduler)
                .ContinueWith(t => refresh(rbStatsBetween.IsChecked.GetValueOrDefault()), MainWindow.MWInstance.UIScheduler);
        }

        /// <summary>Populates the season combo.</summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = _curSeason;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the tbcLeagueOverview control. Handles tab changes, and refreshes the data if required
        ///     (e.g. on season/time-range changes).
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void tbcLeagueOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            if (!_reload && !(e.OriginalSource is TabControl))
            {
                return;
            }

            var currentTab = tbcLeagueOverview.SelectedItem;
            if (Equals(currentTab, tabTeamStats) || Equals(currentTab, tabTeamMetricStats))
            {
                cmbDivConf.IsEnabled = true;
                var doIt = false;
                if (_lastShownTeamSeason != _curSeason)
                {
                    doIt = true;
                }
                else if (_reload)
                {
                    doIt = true;
                }

                if (doIt)
                {
                    prepareTeamStats();
                    _lastShownTeamSeason = _curSeason;
                }
            }
            else if (Equals(currentTab, tabPlayerStats) || Equals(currentTab, tabMetricStats) || Equals(currentTab, tabBest)
                     || Equals(currentTab, tabStartingFive) || Equals(currentTab, tabRatings) || Equals(currentTab, tabContracts)
                     || Equals(currentTab, tabLeaders) || Equals(currentTab, tabMyLeaders))
            {
                cmbDivConf.IsEnabled = true;
                var doIt = false;
                if (_lastShownPlayerSeason != _curSeason)
                {
                    doIt = true;
                }
                else if (_reload)
                {
                    doIt = true;
                }

                if (doIt)
                {
                    preparePlayerStats();
                    _lastShownPlayerSeason = _curSeason;
                }
            }
            else if (Equals(currentTab, tabBoxScores))
            {
                //cmbDivConf.IsEnabled = false;
                var doIt = false;
                if (_lastShownBoxSeason != _curSeason)
                {
                    doIt = true;
                }
                else if (_reload)
                {
                    doIt = true;
                }

                if (doIt)
                {
                    prepareBoxScores();
                    _lastShownBoxSeason = _curSeason;
                }
            }
            _reload = false;
        }

        /// <summary>Prepares and presents the player stats.</summary>
        private void preparePlayerStats()
        {
            _psrList = new List<PlayerStatsRow>();
            _lPSR = new List<PlayerStatsRow>();

            _plPSRList = new List<PlayerStatsRow>();
            _plLpsr = new List<PlayerStatsRow>();

            _leadersList = new List<PlayerStatsRow>();
            _plLeadersList = new List<PlayerStatsRow>();

            _myLeadersList = new List<PlayerStatsRow>();
            _plMyLeadersList = new List<PlayerStatsRow>();

            var worker1 = new BackgroundWorker { WorkerReportsProgress = true };

            txbStatus.FontWeight = FontWeights.Bold;
            txbStatus.Text = "Please wait while player PerGame and metric stats are being calculated...";

            var i = 0;

            var playerCount = -1;

            worker1.DoWork += delegate
                {
                    _sem.WaitOne();
                    _psrList = new List<PlayerStatsRow>();
                    _lPSR = new List<PlayerStatsRow>();

                    _plPSRList = new List<PlayerStatsRow>();
                    _plLpsr = new List<PlayerStatsRow>();

                    playerCount = _pst.Count;
                    foreach (var kvp in _pst)
                    {
                        if (kvp.Value.IsHidden)
                        {
                            continue;
                        }
                        var psr = new PlayerStatsRow(kvp.Value);
                        var plPSR = new PlayerStatsRow(kvp.Value, true);

                        if (psr.IsActive)
                        {
                            if (!inCurrentFilter(_tst[psr.TeamF]))
                            {
                                continue;
                            }
                            psr.TeamFDisplay = _tst[psr.TeamF].DisplayName;
                            plPSR.TeamFDisplay = psr.TeamFDisplay;
                        }
                        else
                        {
                            if (_filterType != TeamFilter.League)
                            {
                                continue;
                            }

                            psr.TeamFDisplay = "- Inactive -";
                            plPSR.TeamFDisplay = psr.TeamFDisplay;
                        }
                        _psrList.Add(psr);
                        _plPSRList.Add(plPSR);
                        worker1.ReportProgress(1);
                    }
                    var leagueAverages = PlayerStats.CalculateLeagueAverages(_pst, _tst);
                    _lPSR.Add(new PlayerStatsRow(leagueAverages));
                    _plLpsr.Add(new PlayerStatsRow(leagueAverages, true));

                    _psrList.Sort((psr1, psr2) => psr1.GmSc.CompareTo(psr2.GmSc));
                    _psrList.Reverse();

                    _plPSRList.Sort((psr1, psr2) => psr1.GmSc.CompareTo(psr2.GmSc));
                    _plPSRList.Reverse();

                    foreach (var psr in _psrList)
                    {
                        if (psr.IsActive)
                        {
                            _leadersList.Add(psr.ConvertToLeagueLeader(_tst));
                            _myLeadersList.Add(psr.ConvertToMyLeagueLeader(_tst));
                        }
                    }
                    foreach (var psr in _plPSRList)
                    {
                        if (psr.IsActive)
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
                        txbStatus.Text = "Please wait while player PerGame and metric stats are being calculated (" + i + "/"
                                         + playerCount + " completed)...";
                    }
                    else
                    {
                        txbStatus.Text = "Please wait as best performers and best starting 5 are being calculated...";
                    }
                };

            worker1.RunWorkerCompleted += delegate
                {
                    var isSeason = rbSeason.IsChecked.GetValueOrDefault();
                    dgvPlayerStats.ItemsSource = isSeason ? _psrList : _plPSRList;
                    dgvLeaguePlayerStats.ItemsSource = isSeason ? _lPSR : _plLpsr;
                    dgvMetricStats.ItemsSource = isSeason ? _psrList : _plPSRList;
                    dgvLeagueMetricStats.ItemsSource = isSeason ? _lPSR : _plLpsr;
                    dgvRatings.ItemsSource = isSeason ? _psrList : _plPSRList;
                    dgvContracts.ItemsSource = isSeason ? _psrList : _plPSRList;
                    dgvLeaders.ItemsSource = isSeason ? _leadersList : _plLeadersList;
                    dgvMyLeaders.ItemsSource = isSeason ? _myLeadersList : _plMyLeadersList;

                    populateSituationalsCombo();

                    updateUltimateTeamTextboxes(isSeason);
                    updateBestPerformersTextboxes(isSeason);

                    tbcLeagueOverview.Visibility = Visibility.Visible;
                    txbStatus.FontWeight = FontWeights.Normal;
                    txbStatus.Text = _message;
                    _sem.Release();
                };

            tbcLeagueOverview.Visibility = Visibility.Hidden;
            worker1.RunWorkerAsync();
        }

        private void updateBestPerformersTextboxes(bool isSeason)
        {
            txbPlayer1.Text = isSeason ? _best1Text : _plBest1Text;
            txbPlayer2.Text = isSeason ? _best2Text : _plBest2Text;
            txbPlayer3.Text = isSeason ? _best3Text : _plBest3Text;
            txbPlayer4.Text = isSeason ? _best4Text : _plBest4Text;
            txbPlayer5.Text = isSeason ? _best5Text : _plBest5Text;
            txbPlayer6.Text = isSeason ? _best6Text : _plBest6Text;
        }

        private void updateUltimateTeamTextboxes(bool isSeason)
        {
            txbStartingPG.Text = isSeason ? _sPGText : _plPGText;
            txbStartingSG.Text = isSeason ? _sSGText : _plSGText;
            txbStartingSF.Text = isSeason ? _sSFText : _plSFText;
            txbStartingPF.Text = isSeason ? _sPFText : _plPFText;
            txbStartingC.Text = isSeason ? _sCText : _plCText;
            txbSubs.Text = isSeason ? _sSubsText : _plSubsText;
        }

        /// <summary>Prepares and presents the best performers' stats.</summary>
        /// <param name="psrList">The list of currently loaded PlayerMetricStatsRow instances.</param>
        /// <param name="plPSRList">The list of currently loaded playoff PlayerMetricStatsRow instances.</param>
        private void calculateBestPerformers(IEnumerable<PlayerStatsRow> psrList, IEnumerable<PlayerStatsRow> plPSRList)
        {
            _best1Text = "";
            _best2Text = "";
            _best3Text = "";
            _best4Text = "";
            _best5Text = "";
            _best6Text = "";

            var useGmSc = cmbUTCriteria.SelectedItem.ToString().Contains("GmSc");

            var templist = new List<PlayerStatsRow>();
            try
            {
                templist = psrList.ToList();
                templist.Sort((pmsr1, pmsr2) => useGmSc ? pmsr1.GmSc.CompareTo(pmsr2.GmSc) : pmsr1.PER.CompareTo(pmsr2.PER));
                templist.Reverse();
                var startingIndex = (Convert.ToInt32(nudBestPage.Value) - 1) * 6;
                templist = templist.Skip(startingIndex).ToList();

                var psr1 = templist[0];
                var text = psr1.GetBestStats(5);
                _best1Text = (startingIndex + 1) + ": " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - "
                             + psr1.TeamFDisplay + ")\n\n" + text;

                var psr2 = templist[1];
                text = psr2.GetBestStats(5);
                _best2Text = (startingIndex + 2) + ": " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + " - "
                             + psr2.TeamFDisplay + ")\n\n" + text;

                var psr3 = templist[2];
                text = psr3.GetBestStats(5);
                _best3Text = (startingIndex + 3) + ": " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + " - "
                             + psr3.TeamFDisplay + ")\n\n" + text;

                var psr4 = templist[3];
                text = psr4.GetBestStats(5);
                _best4Text = (startingIndex + 4) + ": " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + " - "
                             + psr4.TeamFDisplay + ")\n\n" + text;

                var psr5 = templist[4];
                text = psr5.GetBestStats(5);
                _best5Text = (startingIndex + 5) + ": " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + " - "
                             + psr5.TeamFDisplay + ")\n\n" + text;

                var psr6 = templist[5];
                text = psr6.GetBestStats(5);
                _best6Text = (startingIndex + 6) + ": " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + " - "
                             + psr6.TeamFDisplay + ")\n\n" + text;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate season best performers: " + ex.Message);
            }

            _plBest1Text = "";
            _plBest2Text = "";
            _plBest3Text = "";
            _plBest4Text = "";
            _plBest5Text = "";
            _plBest6Text = "";

            try
            {
                templist = plPSRList.ToList();
                templist.Sort((pmsr1, pmsr2) => useGmSc ? pmsr1.GmSc.CompareTo(pmsr2.GmSc) : pmsr1.PER.CompareTo(pmsr2.PER));
                templist.Reverse();
                var startingIndex = (Convert.ToInt32(nudBestPage.Value) - 1) * 6;
                templist = templist.Skip(startingIndex).ToList();

                var psr1 = templist[0];
                var text = psr1.GetBestStats(5);
                _plBest1Text = string.Format(
                    "{0}: {1} {2} ({3} - {4})\n\n{5}",
                    (startingIndex + 1),
                    psr1.FirstName,
                    psr1.LastName,
                    psr1.Position1,
                    psr1.TeamFDisplay,
                    text);

                var psr2 = templist[1];
                text = psr2.GetBestStats(5);
                _plBest2Text = string.Format(
                    "{0}: {1} {2} ({3} - {4})\n\n{5}",
                    (startingIndex + 2),
                    psr2.FirstName,
                    psr2.LastName,
                    psr2.Position1,
                    psr2.TeamFDisplay,
                    text);

                var psr3 = templist[2];
                text = psr3.GetBestStats(5);
                _plBest3Text = string.Format(
                    "{0}: {1} {2} ({3} - {4})\n\n{5}",
                    (startingIndex + 3),
                    psr3.FirstName,
                    psr3.LastName,
                    psr3.Position1,
                    psr3.TeamFDisplay,
                    text);

                var psr4 = templist[3];
                text = psr4.GetBestStats(5);
                _plBest4Text = string.Format(
                    "{0}: {1} {2} ({3} - {4})\n\n{5}",
                    (startingIndex + 4),
                    psr4.FirstName,
                    psr4.LastName,
                    psr4.Position1,
                    psr4.TeamFDisplay,
                    text);

                var psr5 = templist[4];
                text = psr5.GetBestStats(5);
                _plBest5Text = string.Format(
                    "{0}: {1} {2} ({3} - {4})\n\n{5}",
                    (startingIndex + 5),
                    psr5.FirstName,
                    psr5.LastName,
                    psr5.Position1,
                    psr5.TeamFDisplay,
                    text);

                var psr6 = templist[5];
                text = psr6.GetBestStats(5);
                _plBest6Text = string.Format(
                    "{0}: {1} {2} ({3} - {4})\n\n{5}",
                    (startingIndex + 6),
                    psr6.FirstName,
                    psr6.LastName,
                    psr6.Position1,
                    psr6.TeamFDisplay,
                    text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate playoff best performers: " + ex.Message);
            }
        }

        /// <summary>Calculates the best starting five for the current scope.</summary>
        /// <param name="psrList">The list of currently loaded PlayerStatsRow instances, sorted by GmSc in descending order.</param>
        /// <param name="type"></param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the starting five will be determined based on their playoff performances.
        /// </param>
        private void calculateUltimateTeam(List<PlayerStatsRow> psrList, string type, bool playoffs = false)
        {
            if (!playoffs)
            {
                _sPGText = "";
                _sSGText = "";
                _sSFText = "";
                _sPFText = "";
                _sCText = "";
                _sSubsText = "";
            }
            else
            {
                _plPGText = "";
                _plSGText = "";
                _plSFText = "";
                _plPFText = "";
                _plCText = "";
                _plSubsText = "";
            }

            bool useGmSc = cmbUTCriteria.SelectedItem.ToString().Contains("GmSc");

            if (type.StartsWith("All-Rookie"))
            {
                psrList = psrList.Where(ps => ps.YearsPro == 1).ToList();
            }
            psrList = psrList.OrderByDescending(ps => useGmSc ? ps.GmSc : ps.PER).ToList();

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            var pgList =
                psrList.Where(row => (row.Position1 == Position.PG || row.Position2 == Position.PG)) // && row.isInjured == false)
                       .Take(10).ToList();
            pgList = pgList.OrderByDescending(ps => useGmSc ? ps.GmSc : ps.PER).ToList();
            var sgList =
                psrList.Where(row => (row.Position1 == Position.SG || row.Position2 == Position.SG)) // && row.isInjured == false)
                       .Take(10).ToList();
            sgList = sgList.OrderByDescending(ps => useGmSc ? ps.GmSc : ps.PER).ToList();
            var sfList =
                psrList.Where(row => (row.Position1 == Position.SF || row.Position2 == Position.SF)) // && row.isInjured == false)
                       .Take(10).ToList();
            sfList = sfList.OrderByDescending(ps => useGmSc ? ps.GmSc : ps.PER).ToList();
            var pfList =
                psrList.Where(row => (row.Position1 == Position.PF || row.Position2 == Position.PF)) // && row.isInjured == false)
                       .Take(10).ToList();
            pfList = pfList.OrderByDescending(ps => useGmSc ? ps.GmSc : ps.PER).ToList();
            var cList =
                psrList.Where(row => (row.Position1 == Position.C || row.Position2 == Position.C)) // && row.isInjured == false)
                       .Take(10).ToList();
            cList = cList.OrderByDescending(ps => useGmSc ? ps.GmSc : ps.PER).ToList();
            var permutations = new List<StartingFivePermutation>();

            var max = Double.MinValue;
            foreach (var pg in pgList)
            {
                foreach (var sg in sgList)
                {
                    foreach (var sf in sfList)
                    {
                        foreach (var pf in pfList)
                        {
                            foreach (var c in cList)
                            {
                                double sum = 0;
                                var pInP = 0;
                                var perm = new List<int>(5) { pg.ID };
                                sum += useGmSc ? pg.GmSc : pg.PER;
                                if (pg.Position1S == "PG")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(sg.ID))
                                {
                                    continue;
                                }
                                perm.Add(sg.ID);
                                sum += useGmSc ? sg.GmSc : sg.PER;
                                if (sg.Position1S == "SG")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(sf.ID))
                                {
                                    continue;
                                }
                                perm.Add(sf.ID);
                                sum += useGmSc ? sf.GmSc : sf.PER;
                                if (sf.Position1S == "SF")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(pf.ID))
                                {
                                    continue;
                                }
                                perm.Add(pf.ID);
                                sum += useGmSc ? pf.GmSc : pf.PER;
                                if (pf.Position1S == "PF")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(c.ID))
                                {
                                    continue;
                                }
                                perm.Add(c.ID);
                                sum += useGmSc ? c.GmSc : c.PER;
                                if (c.Position1S == "C")
                                {
                                    pInP++;
                                }

                                if (sum > max)
                                {
                                    max = sum;
                                }

                                permutations.Add(
                                    new StartingFivePermutation { IDList = perm, PlayersInPrimaryPosition = pInP, Sum = sum });
                            }
                        }
                    }
                }
            }

            var bestPerm = new StartingFivePermutation();
            var benchPerm = new StartingFivePermutation();
            var thirdPerm = new StartingFivePermutation();
            try
            {
                bestPerm =
                    permutations.Where(perm1 => perm1.Sum.Equals(max))
                                .OrderByDescending(perm2 => perm2.PlayersInPrimaryPosition)
                                .First();
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
                            permutations.Where(
                                p => !(p.IDList.Any(id => bestPerm.IDList.Contains(id) || benchPerm.IDList.Contains(id))))
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
                displayText = "PG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay
                              + ")\n\n" + text;
                if (!playoffs)
                {
                    _sPGText = displayText;
                }
                else
                {
                    _plPGText = displayText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's PG: " + ex.Message);
            }

            try
            {
                psr1 = tempList[1];
                text = psr1.GetBestStats(5);
                displayText = "SG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay
                              + ")\n\n" + text;
                if (!playoffs)
                {
                    _sSGText = displayText;
                }
                else
                {
                    _plSGText = displayText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's SG: " + ex.Message);
            }

            try
            {
                psr1 = tempList[2];
                text = psr1.GetBestStats(5);
                displayText = "SF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay
                              + ")\n\n" + text;
                if (!playoffs)
                {
                    _sSFText = displayText;
                }
                else
                {
                    _plSFText = displayText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's SF: " + ex.Message);
            }

            try
            {
                psr1 = tempList[3];
                text = psr1.GetBestStats(5);
                displayText = "PF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay
                              + ")\n\n" + text;
                if (!playoffs)
                {
                    _sPFText = displayText;
                }
                else
                {
                    _plPFText = displayText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's PF: " + ex.Message);
            }

            try
            {
                psr1 = tempList[4];
                text = psr1.GetBestStats(5);
                displayText = "C: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay
                              + ")\n\n" + text;
                if (!playoffs)
                {
                    _sCText = displayText;
                }
                else
                {
                    _plCText = displayText;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's C: " + ex.Message);
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
            var idCountToSkip = usedIDs.Count;
            var i = 0;
            try
            {
                while (usedIDs.Contains(pgList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(pgList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute PG: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(sgList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(sgList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute SG: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(sfList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(sfList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute SF: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(pfList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(pfList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute PF: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(cList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(cList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute C: " + ex.Message);
            }

            try
            {
                var count = usedIDs.Count - idCountToSkip + 5;
                for (var j = count + 1; j <= 12; j++)
                {
                    i = 0;
                    while (usedIDs.Contains(psrList[i].ID))
                    {
                        i++;
                    }
                    usedIDs.Add(psrList[i].ID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's other substitutes: " + ex.Message);
            }

            usedIDs.Skip(idCountToSkip).ToList().ForEach(id => tempList.Add(psrList.Single(row => row.ID == id)));
            displayText = "Subs: ";
            for (i = 5; i < usedIDs.Count - idCountToSkip + 5; i++)
            {
                psr1 = tempList[i];
                //text = psr1.GetBestStats(5);
                displayText += psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + "), ";
            }
            displayText = displayText.TrimEnd(new[] { ' ', ',' });

            if (!playoffs)
            {
                _sSubsText = displayText;
            }
            else
            {
                _plSubsText = displayText;
            }
        }

        /// <summary>Prepares and presents the available box scores.</summary>
        private void prepareBoxScores()
        {
            _dtBS.Clear();

            foreach (var bse in _bsHist)
            {
                if (!inCurrentFilter(bse.BS.Team1ID) && !inCurrentFilter(bse.BS.Team2ID))
                {
                    continue;
                }
                if (rbSeason.IsChecked.GetValueOrDefault())
                {
                    if (bse.BS.IsPlayoff)
                    {
                        continue;
                    }
                }
                else
                {
                    if (!bse.BS.IsPlayoff)
                    {
                        continue;
                    }
                }

                var r = _dtBS.NewRow();

                try
                {
                    r["Date"] = bse.BS.GameDate.ToString().Split(' ')[0];
                    r["Away"] = MainWindow.TST[bse.BS.Team1ID].DisplayName;
                    r["AS"] = Convert.ToInt32(bse.BS.PTS1);
                    r["Home"] = MainWindow.TST[bse.BS.Team2ID].DisplayName;
                    r["HS"] = Convert.ToInt32(bse.BS.PTS2);
                    r["GameID"] = bse.BS.ID;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    continue;
                }

                _dtBS.Rows.Add(r);
            }

            var dvBS = new DataView(_dtBS) { AllowNew = false, AllowEdit = false };

            dgvBoxScores.DataContext = dvBS;
        }

        /// <summary>Prepares and presents the team stats.</summary>
        private void prepareTeamStats()
        {
            TSRList = new List<TeamStatsRow>();
            lssr = new List<TeamStatsRow>();
            oppTSRList = new List<TeamStatsRow>();

            var ls = new TeamStats(-1, "League");

            foreach (var key in _tst.Keys)
            {
                if (_tst[key].IsHidden)
                {
                    continue;
                }

                if (!inCurrentFilter(_tst[key]))
                {
                    continue;
                }

                TSRList.Add(new TeamStatsRow(_tst[key], _pst, _splitTeamStats));
                oppTSRList.Add(new TeamStatsRow(_tstOpp[key]));
            }

            ls = TeamStats.CalculateLeagueAverages(_tst, Span.Season);

            lssr.Add(new TeamStatsRow(ls));

            TSRList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            TSRList.Reverse();
            oppTSRList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            //oppTsrList.Reverse();

            pl_TSRList = new List<TeamStatsRow>();
            pl_Lssr = new List<TeamStatsRow>();
            pl_OppTSRList = new List<TeamStatsRow>();

            var ls1 = new TeamStats(-1, "League");

            foreach (var key1 in _tst.Keys)
            {
                if (_tst[key1].IsHidden)
                {
                    continue;
                }
                if (_tst[key1].GetPlayoffGames() == 0)
                {
                    continue;
                }
                if (!inCurrentFilter(_tst[key1]))
                {
                    continue;
                }

                pl_TSRList.Add(new TeamStatsRow(_tst[key1], true));
                pl_OppTSRList.Add(new TeamStatsRow(_tstOpp[key1], true));
            }

            ls1 = TeamStats.CalculateLeagueAverages(_tst, Span.Playoffs);

            pl_Lssr.Add(new TeamStatsRow(ls1, true));

            pl_TSRList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            pl_TSRList.Reverse();
            pl_OppTSRList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            //pl_oppTsrList.Reverse();

            var isSeason = rbSeason.IsChecked.GetValueOrDefault();

            dgvTeamStats.ItemsSource = isSeason ? TSRList : pl_TSRList;
            dgvLeagueTeamStats.ItemsSource = isSeason ? lssr : pl_Lssr;

            dgvTeamMetricStats.ItemsSource = isSeason ? TSRList : pl_TSRList;
            dgvLeagueTeamMetricStats.ItemsSource = isSeason ? lssr : pl_Lssr;

            dgvOpponentStats.ItemsSource = isSeason ? oppTSRList : pl_OppTSRList;
            dgvLeagueOpponentStats.ItemsSource = isSeason ? lssr : pl_Lssr;

            dgvOpponentMetricStats.ItemsSource = isSeason ? oppTSRList : pl_OppTSRList;
            dgvLeagueOpponentMetricStats.ItemsSource = isSeason ? lssr : pl_Lssr;

            dgvTeamInfo.ItemsSource = TSRList;
        }

        /// <summary>Determines whether a specific team should be shown or not, based on the current filter.</summary>
        /// <param name="ts">The team's TeamStats instance.</param>
        /// <returns>true if it should be shown; otherwise, false</returns>
        private bool inCurrentFilter(TeamStats ts)
        {
            if (_filterType == TeamFilter.League)
            {
                return true;
            }

            if (_filterType == TeamFilter.Conference)
            {
                var confID = -1;
                foreach (var conf in MainWindow.Conferences)
                {
                    if (conf.Name == _filterDescription)
                    {
                        confID = conf.ID;
                        break;
                    }
                }
                var div = MainWindow.Divisions.Find(division => division.ID == ts.Division);
                if (div.ConferenceID == confID)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var div = MainWindow.Divisions.Find(division => division.ID == ts.Division);
                if (div.Name == _filterDescription)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>Determines whether a specific team should be shown or not, based on the current filter.</summary>
        /// <param name="teamID">Name of the team.</param>
        /// <returns>true if it should be shown; otherwise, false</returns>
        private bool inCurrentFilter(int teamID)
        {
            if (_filterType == TeamFilter.League)
            {
                return true;
            }

            var res = _db.GetDataTable("SELECT Division FROM Teams WHERE ID = " + teamID);
            var divID = ParseCell.GetInt32(res.Rows[0], "Division");

            if (_filterType == TeamFilter.Conference)
            {
                var confID = -1;
                foreach (var conf in MainWindow.Conferences)
                {
                    if (conf.Name == _filterDescription)
                    {
                        confID = conf.ID;
                        break;
                    }
                }
                var div = MainWindow.Divisions.Find(division => division.ID == divID);
                if (div.ConferenceID == confID)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var div = MainWindow.Divisions.Find(division => division.ID == divID);
                if (div.Name == _filterDescription)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsAllTime control. Changes the timeframe to the whole season, forces all tabs to be
        ///     reloaded on first request, and reloads the current one.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                _reload = true;
                MainWindow.Tf = new Timeframe(_curSeason);

                updateData();
            }
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsBetween control. Changes the timeframe to be between the specified dates, forces all
        ///     tabs to be reloaded on first request, and reloads the current one.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                _reload = true;
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());

                updateData();
            }
        }

        /// <summary>Handles the LoadingRow event of the dg control. Adds a ranking number to the row's header.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridRowEventArgs" /> instance containing the event data.
        /// </param>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control. Loads all the required information for the new season and
        ///     reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1)
            {
                return;
            }

            _curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

            if (_curSeason == MainWindow.Tf.SeasonNum && !MainWindow.Tf.IsBetween)
            {
                return;
            }

            MainWindow.Tf = new Timeframe(_curSeason);

            updateData();
        }

        private void refresh(bool between)
        {
            if (!between)
            {
                _reload = true;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
            else
            {
                _lastShownTeamSeason = 0;
                _lastShownPlayerSeason = 0;
                _lastShownBoxSeason = 0;
                rbStatsBetween.IsChecked = true;
                _reload = true;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
            _changingTimeframe = false;

            MainWindow.MWInstance.StopProgressWatchTimer();
            IsEnabled = true;
        }

        private void linkInternalsToMainWindow()
        {
            _tst = MainWindow.TST;
            _tstOpp = MainWindow.TSTOpp;
            _pst = MainWindow.PST;
            _bsHist = MainWindow.BSHist;
            _splitTeamStats = MainWindow.SplitTeamStats;
        }

        /// <summary>Handles the MouseDoubleClick event of the dgvBoxScores control. Views the selected box score in the Box Score Window.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvBoxScores.SelectedItems[0];
                var gameid = Convert.ToInt32(row["GameID"].ToString());

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameid);
                try
                {
                    if (bsw.ShowDialog() == true)
                    {
                        _reload = true;
                        updateData();
                        tbcLeagueOverview_SelectionChanged(null, null);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>Handles the MouseDoubleClick event of the AnyTeamDataGrid control. Views the selected team in the Team Overview Window.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void anyTeamDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventHandlers.AnyTeamDataGrid_MouseDoubleClick(sender, e);
        }

        /// <summary>Handles the MouseDoubleClick event of the AnyPlayerDataGrid control. Views the selected player in the Player Overview Window.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void anyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EventHandlers.AnyPlayerDataGrid_MouseDoubleClick(sender, e);
        }

        /// <summary>Handles the Loaded event of the Window control. Forces all tabs to be reloaded on first request.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            //PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstOpp, MainWindow.TeamOrder, true);
            _lastShownPlayerSeason = 0;
            _lastShownTeamSeason = _curSeason;
            _lastShownBoxSeason = 0;
            _message = txbStatus.Text;
        }

        /// <summary>Handles the Closing event of the Window control. Forces all tabs to be reloaded on first request.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="CancelEventArgs" /> instance containing the event data.
        /// </param>
        private void window_Closing(object sender, CancelEventArgs e)
        {
            _lastShownTeamSeason = 0;
            _lastShownPlayerSeason = 0;
            _lastShownBoxSeason = 0;

            Tools.SetRegistrySetting("LeagueOvHeight", Height);
            Tools.SetRegistrySetting("LeagueOvWidth", Width);
            Tools.SetRegistrySetting("LeagueOvX", Left);
            Tools.SetRegistrySetting("LeagueOvY", Top);
        }

        /// <summary>
        ///     Handles the Sorting event of the StatColumn control. Uses a custom Sorting event handler that sorts a stat column in
        ///     descending order, if it's not already sorted.
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
        ///     Handles the LayoutUpdated event of the dgvTeamMetricStats control. Used to synchronize the column width between the
        ///     teams/players DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvTeamMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (var i = 0; i < dgvTeamMetricStats.Columns.Count && i < dgvLeagueTeamMetricStats.Columns.Count; ++i)
            {
                dgvLeagueTeamMetricStats.Columns[i].Width = dgvTeamMetricStats.Columns[i].ActualWidth;
            }
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvTeamStats control. Used to synchronize the column width between the teams/players
        ///     DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvTeamStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (var i = 0; i < dgvTeamStats.Columns.Count && i < dgvLeagueTeamStats.Columns.Count; ++i)
            {
                dgvLeagueTeamStats.Columns[i].Width = dgvTeamStats.Columns[i].ActualWidth;
            }
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvPlayerStats control. Used to synchronize the column width between the teams/players
        ///     DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvPlayerStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (var i = 0; i < dgvPlayerStats.Columns.Count && i < dgvLeaguePlayerStats.Columns.Count; ++i)
            {
                dgvLeaguePlayerStats.Columns[i].Width = dgvPlayerStats.Columns[i].ActualWidth;
            }
        }

        /// <summary>
        ///     Handles the LayoutUpdated event of the dgvMetricStats control. Used to synchronize the column width between the teams/players
        ///     DataGrid and the league average DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dgvMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (var i = 0; i < dgvMetricStats.Columns.Count && i < dgvLeagueMetricStats.Columns.Count; ++i)
            {
                dgvLeagueMetricStats.Columns[i].Width = dgvMetricStats.Columns[i].ActualWidth;
            }
        }

        /// <summary>Handles the LoadingRow event of the dgLeague control. Adds an "L" to the row header for the league average row.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridRowEventArgs" /> instance containing the event data.
        /// </param>
        private void dgLeague_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = "L";
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbDivConf control. Applies the new filter, forces all tabs to reload on first
        ///     request and reloads the current tab.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbDivConf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
            {
                return;
            }

            if (cmbDivConf.SelectedIndex == -1)
            {
                return;
            }

            var cur = (ComboBoxItemWithIsEnabled) cmbDivConf.SelectedItem;
            var name = cur.Item;
            var parts = name.Split(new[] { ": " }, 2, StringSplitOptions.None);
            if (parts.Length == 1)
            {
                if (parts[0] == "Whole League")
                {
                    _filterType = TeamFilter.League;
                    _filterDescription = "";
                }
                else
                {
                    _filterType = TeamFilter.Conference;
                    _filterDescription = parts[0];
                }
            }
            else
            {
                _filterType = TeamFilter.Division;
                _filterDescription = parts[1];
            }

            _reload = true;
            _lastShownTeamSeason = 0;
            _lastShownPlayerSeason = 0;
            _lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Checked event of the rbSeason control. Switches the visibility of the Season tabs to visible, and of the Playoff
        ///     tabs to hidden.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbSeason_Checked(object sender, RoutedEventArgs e)
        {
            if (_changingTimeframe)
            {
                return;
            }

            _reload = true;
            _lastShownTeamSeason = 0;
            _lastShownPlayerSeason = 0;
            _lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Checked event of the rbPlayoffs control. Switches the visibility of the Season tabs to hidden, and of the Playoff
        ///     tabs to visible.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbPlayoffs_Checked(object sender, RoutedEventArgs e)
        {
            if (_changingTimeframe)
            {
                return;
            }

            _reload = true;
            _lastShownTeamSeason = 0;
            _lastShownPlayerSeason = 0;
            _lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        private void btnExportLRERatings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Title = "Select the TSV file of your roster" };
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
            {
                return;
            }

            var file = ofd.FileName;

            var dictList = CSV.DictionaryListFromTSVFile(file);

            var plist = rbSeason.IsChecked.GetValueOrDefault() ? _psrList : _plPSRList;

            var ratingNames =
                dgvRatings.Columns.ToList()
                          .SkipWhile(c => c.Header.ToString() != "RatingsStart")
                          .Skip(1)
                          .Select(c => c.Header.ToString())
                          .ToList();

            foreach (var ps in plist)
            {
                var pInsts =
                    dictList.FindAll(dict => dict["Name"].ToUpperInvariant() == (ps.FirstName + " " + ps.LastName).ToUpperInvariant())
                            .ToList();
                foreach (var pInst in pInsts)
                {
                    foreach (var ratingName in ratingNames)
                    {
                        var rating = Convert.ToInt32(typeof(PlayerStatsRow).GetProperty("re" + ratingName).GetValue(ps, null));
                        if (rating != -1)
                        {
                            pInst[ratingName] = rating.ToString();
                        }
                    }
                }
            }

            CSV.TSVFromDictionaryList(dictList, file);

            MessageBox.Show(
                "Successfully updated Roster TSV with calculated ratings.",
                "NBA Stats Tracker",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void dgvPlayerStats_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;

                var dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                var isSeason = rbSeason.IsChecked.GetValueOrDefault();
                var list = isSeason ? _psrList : _plPSRList;
                for (var j = 0; j < dictList.Count; j++)
                {
                    var dict = dictList[j];
                    int id;
                    try
                    {
                        id = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var matching = new List<PlayerStats>();
                            if (dict.ContainsKey("Last Name"))
                            {
                                matching =
                                    MainWindow.PST.Values.Where(
                                        ps => ps.LastName == dict["Last Name"] && ps.FirstName == dict["First Name"]).ToList();
                            }
                            else if (dict.ContainsKey("Name"))
                            {
                                if (dict["Name"].Contains(", "))
                                {
                                    var parts = dict["Name"].Split(',');
                                    matching =
                                        MainWindow.PST.Values.Where(ps => ps.LastName == parts[0] && ps.FirstName == parts[1]).ToList();
                                }
                                else
                                {
                                    var parts = dict["Name"].Split(new[] { ' ' }, 2);
                                    matching =
                                        MainWindow.PST.Values.Where(ps => ps.LastName == parts[1] && ps.FirstName == parts[0]).ToList();
                                }
                            }
                            if (matching.Count == 0)
                            {
                                throw new Exception();
                            }
                            if (matching.Count > 1)
                            {
                                try
                                {
                                    matching = matching.Where(ps => MainWindow.TST[ps.TeamF].DisplayName == dict["Team"]).ToList();
                                }
                                catch
                                {
                                }
                            }
                            if (matching.Count > 1)
                            {
                                throw new Exception();
                            }
                            else
                            {
                                id = matching[0].ID;
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(
                                "Player in row " + j
                                + " couldn't be determined either by ID or Full Name. Make sure the pasted data has the proper headers. "
                                + "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        var psr = list.Single(ps => ps.ID == id);
                        PlayerStatsRow.TryChangePSR(ref psr, dict);
                        PlayerStatsRow.Refresh(ref psr);
                        list[list.FindIndex(ts => ts.ID == id)] = psr;
                        MainWindow.PST[id] = new PlayerStats(psr, !isSeason);
                    }
                    catch
                    {
                        continue;
                    }
                }

                ((DataGrid) sender).ItemsSource = null;
                ((DataGrid) sender).ItemsSource = list;

                MessageBox.Show(
                    "Data pasted successfully! Remember to save!\n\nNote that metric and other stats may appear incorrect until you save.",
                    "NBA Stats Tracker",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void anyTeamDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;

                var dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());
                var dg = (DataGrid) sender;
                var isSeason = rbSeason.IsChecked.GetValueOrDefault();
                List<TeamStatsRow> list;
                if (isSeason)
                {
                    list = !dg.Name.Contains("Opp") ? TSRList : oppTSRList;
                }
                else
                {
                    list = !dg.Name.Contains("Opp") ? pl_TSRList : pl_OppTSRList;
                }

                for (var j = 0; j < dictList.Count; j++)
                {
                    var dict = dictList[j];
                    int id;
                    try
                    {
                        id = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            id = MainWindow.TST.Values.Single(ts => ts.DisplayName == dict["Team"]).ID;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(
                                "Team in row " + (j + 1)
                                + " couldn't be determined either by ID or Name. Make sure the pasted data has the proper headers. "
                                + "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        var tsr = list.Single(ts => ts.ID == id);
                        TeamStatsRow.TryChangeTSR(ref tsr, dict);
                        TeamStatsRow.Refresh(ref tsr);
                        list[list.FindIndex(ts => ts.ID == id)] = tsr;
                        if (!dg.Name.Contains("Opp"))
                        {
                            MainWindow.TST[tsr.ID] = new TeamStats(tsr, !isSeason);
                        }
                        else
                        {
                            MainWindow.TSTOpp[tsr.ID] = new TeamStats(tsr, !isSeason);
                        }
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
                    "NBA Stats Tracker",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void dgvOpponentStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (var i = 0; i < dgvOpponentStats.Columns.Count && i < dgvLeagueOpponentStats.Columns.Count; ++i)
            {
                dgvLeagueOpponentStats.Columns[i].Width = dgvOpponentStats.Columns[i].ActualWidth;
            }
        }

        private void dgvOpponentMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (var i = 0; i < dgvOpponentMetricStats.Columns.Count && i < dgvLeagueOpponentMetricStats.Columns.Count; ++i)
            {
                dgvLeagueOpponentMetricStats.Columns[i].Width = dgvOpponentMetricStats.Columns[i].ActualWidth;
            }
        }

        private void btnSetRatingsCriteria_Click(object sender, RoutedEventArgs e)
        {
            var ibw =
                new InputBoxWindow(
                    "Enter percentage (0-100) of team's games played the player must have "
                    + "participated in (-1 not to use this criterion)",
                    SQLiteIO.GetSetting("RatingsGPPct", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("RatingsGPPct", InputBoxWindow.UserInput);
            }

            ibw = new InputBoxWindow(
                "Enter minimum amount of minutes per game played by the player (-1 not to use this criterion)",
                SQLiteIO.GetSetting("RatingsMPG", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("RatingsMPG", InputBoxWindow.UserInput);
            }

            MainWindow.LoadRatingsCriteria();

            preparePlayerStats();
        }

        private void btnExportLREDitorRatings_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                {
                    Title = "Select the Players.csv file of your REDitor-exported save",
                    Filter = "Comma-separated Values Files (*.csv)|*.csv"
                };
            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
            {
                return;
            }

            var file = ofd.FileName;

            var dictList = CSV.DictionaryListFromCSVFile(file);

            var plist = rbSeason.IsChecked.GetValueOrDefault() ? _psrList : _plPSRList;

            var ratingNames =
                dgvRatings.Columns.ToList()
                          .SkipWhile(c => c.Header.ToString() != "RatingsStart")
                          .Skip(1)
                          .Select(c => c.Header.ToString())
                          .ToList();

            foreach (var ps in plist)
            {
                var pInsts =
                    dictList.FindAll(
                        dict =>
                        dict["First_Name"].ToUpperInvariant() == ps.FirstName.ToUpperInvariant()
                        && dict["Last_Name"].ToUpperInvariant() == ps.LastName.ToUpperInvariant()).ToList();
                foreach (var pInst in pInsts)
                {
                    foreach (var ratingName in ratingNames)
                    {
                        var rating = Convert.ToInt32(typeof(PlayerStatsRow).GetProperty("re" + ratingName).GetValue(ps, null));
                        if (rating != -1)
                        {
                            pInst[REtoREDitor[ratingName]] = rating.ToString();
                        }
                    }
                }
            }

            CSV.CSVFromDictionaryList(dictList, file);

            MessageBox.Show(
                "Successfully updated Players CSV with calculated ratings.",
                "NBA Stats Tracker",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void btnSetMyLeadersCriteria_Click(object sender, RoutedEventArgs e)
        {
            var ibw =
                new InputBoxWindow(
                    "Enter percentage (0-100) of team's games played the player must have "
                    + "participated in (-1 not to use this criterion)",
                    SQLiteIO.GetSetting("MyLeadersGPPct", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("MyLeadersGPPct", InputBoxWindow.UserInput);
            }

            ibw = new InputBoxWindow(
                "Enter minimum amount of minutes per game played by the player (-1 not to use this criterion)",
                SQLiteIO.GetSetting("MyLeadersMPG", "-1"));
            if (ibw.ShowDialog() == true)
            {
                SQLiteIO.SetSetting("MyLeadersMPG", InputBoxWindow.UserInput);
            }

            MainWindow.LoadMyLeadersCriteria();

            preparePlayerStats();
        }

        private void cmbSituational_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSituational.SelectedIndex == -1 || cmbUTCriteria.SelectedIndex == -1)
            {
                return;
            }

            prepareUltimateTeam();
        }

        private void prepareUltimateTeam()
        {
            switch (cmbUTCriteria.SelectedItem.ToString().Split('(')[0].Trim())
            {
                case "All Players":
                    calculateUltimateTeam(_psrList, cmbSituational.SelectedItem.ToString());
                    calculateUltimateTeam(_plPSRList, cmbSituational.SelectedItem.ToString(), true);
                    break;
                case "League Leaders":
                    calculateUltimateTeam(_leadersList, cmbSituational.SelectedItem.ToString());
                    calculateUltimateTeam(_plLeadersList, cmbSituational.SelectedItem.ToString(), true);
                    break;
                case "My League Leaders":
                    calculateUltimateTeam(_myLeadersList, cmbSituational.SelectedItem.ToString());
                    calculateUltimateTeam(_plMyLeadersList, cmbSituational.SelectedItem.ToString(), true);
                    break;
            }

            updateUltimateTeamTextboxes(rbSeason.IsChecked.GetValueOrDefault());
        }

        private void cmbUTCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUTCriteria.SelectedIndex == -1 || cmbSituational.SelectedIndex == -1)
            {
                return;
            }

            prepareUltimateTeam();
            prepareBestPerformers();

            if (cmbUTCriteria.SelectedItem.ToString().Split('(')[0].Trim() == "League Leaders")
            {
                txbStartingPG.Text =
                    "Ultimate Team can't be calculated with League Leaders filtering. Other features (e.g. \"Best Performers\" "
                    + "use this filtering though. Use All Players or My League Leaders for Ultimate Team.";
            }
        }

        private void prepareBestPerformers()
        {
            if (nudBestPage.Value == null || nudBestPage.Value < 1)
            {
                nudBestPage.Value = 1;
            }

            switch (cmbUTCriteria.SelectedItem.ToString().Split('(')[0].Trim())
            {
                case "All Players":
                    calculateBestPerformers(_psrList, _plPSRList);
                    break;
                case "League Leaders":
                    calculateBestPerformers(_leadersList, _plLeadersList);
                    break;
                case "My League Leaders":
                    calculateBestPerformers(_myLeadersList, _plMyLeadersList);
                    break;
            }

            updateBestPerformersTextboxes(rbSeason.IsChecked.GetValueOrDefault());
        }

        private void chkOnlyInjured_Click(object sender, RoutedEventArgs e)
        {
            if (chkOnlyInjured.IsChecked.GetValueOrDefault())
            {
                var pView = CollectionViewSource.GetDefaultView(_psrList);
                pView.Filter = o => ((PlayerStatsRow) o).IsInjured;
                dgvContracts.ItemsSource = pView;
            }
            else
            {
                var pView = CollectionViewSource.GetDefaultView(_psrList);
                pView.Filter = null;
                dgvContracts.ItemsSource = null;
                dgvContracts.ItemsSource = pView;
            }
        }

        private void nudBestPage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            prepareBestPerformers();
        }

        #region Nested type: TeamFilter

        /// <summary>
        ///     Used to determine the filter that should be applied to which teams and players are included in the calculations and shown in
        ///     the DataGrids.
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