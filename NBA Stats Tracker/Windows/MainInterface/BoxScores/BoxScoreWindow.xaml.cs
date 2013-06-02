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

namespace NBA_Stats_Tracker.Windows.MainInterface.BoxScores
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    using LeftosCommonLibrary;
    using LeftosCommonLibrary.BeTimvwFramework;
    using LeftosCommonLibrary.CommonDialogs;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;
    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.EventHandlers;
    using NBA_Stats_Tracker.Helper.ListExtensions;
    using NBA_Stats_Tracker.Helper.Miscellaneous;

    #endregion

    /// <summary>Used to display and edit team and player box scores, as well as player metric stats and the best performers for that game.</summary>
    public partial class BoxScoreWindow
    {
        #region Mode enum

        /// <summary>
        ///     Used to determine the function for which the window has been opened. Update is for entering a new box score. View is for
        ///     viewing and editing pre-existing box score. ViewAndIgnore is for viewing a pre-existing box score in read-only mode.
        /// </summary>
        public enum Mode
        {
            Update,
            View,
            ViewAndIgnore
        };

        #endregion

        private static Mode curMode = Mode.Update;

        private static TeamBoxScore curTeamBoxScore;
        private readonly int _maxSeason = SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);
        private readonly bool _onImport;
        private bool _clickedOK;
        private int _curSeason;
        private Brush _defaultBackground;
        private bool _isManualEditCommit;
        private bool _loading;
        private bool _minsUpdating;
        private List<PlayerStatsRow> _pmsrListAway, _pmsrListHome;
        private Dictionary<int, PlayerStats> _pst;
        private List<string> _teams;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="curMode">The Mode enum instance which determines the function for which the window is opened.</param>
        public BoxScoreWindow(Mode curMode = Mode.Update)
        {
            InitializeComponent();
            _clickedOK = false;

            if (MainWindow.Tf.IsBetween)
            {
                MainWindow.Tf = new Timeframe(MainWindow.Tf.SeasonNum);
                IsEnabled = false;
                Task.Factory.StartNew(() => MainWindow.UpdateAllData(true))
                    .FailFastOnException(MainWindow.MWInstance.UIScheduler)
                    .ContinueWith(t => finishInitialization(curMode), MainWindow.MWInstance.UIScheduler)
                    .FailFastOnException(MainWindow.MWInstance.UIScheduler);
            }
            else
            {
                finishInitialization(curMode);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="curMode">The Mode enum instance which determines the function for which the window is opened.</param>
        /// <param name="id">The ID of the box score to be viewed.</param>
        public BoxScoreWindow(Mode curMode, int id)
            : this(curMode)
        {
            loadBoxScore(id);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="bse">The Box Score Entry from which to load the box score to be viewed.</param>
        /// <param name="onImport">
        ///     if set to <c>true</c>, a box score is being imported into the database, and the window is prepared accordingly.
        /// </param>
        public BoxScoreWindow(BoxScoreEntry bse, bool onImport = false)
            : this()
        {
            loadBoxScore(bse);
            _onImport = onImport;

            if (onImport)
            {
                chkDoNotUpdate.IsEnabled = false;
                cmbSeasonNum.IsEnabled = false;
                cmbTeam1.IsEnabled = false;
                cmbTeam2.IsEnabled = false;
                btnCalculateTeams_Click(null, null);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="bse">The Box Score Entry from which to load the box score to be viewed.</param>
        /// <param name="pst">The player stats dictionary to use for this instance.</param>
        /// <param name="onImport">
        ///     if set to <c>true</c>, a box score is being imported into the database, and the window is prepared accordingly.
        /// </param>
        public BoxScoreWindow(BoxScoreEntry bse, Dictionary<int, PlayerStats> pst, bool onImport)
            : this()
        {
            _pst = pst;

            loadBoxScore(bse);
            _onImport = onImport;

            if (onImport)
            {
                MainWindow.TempBSE_BS = bse.BS;
                chkDoNotUpdate.IsEnabled = false;
                cmbSeasonNum.IsEnabled = false;
                cmbTeam1.IsEnabled = false;
                cmbTeam2.IsEnabled = false;
                btnCalculateTeams_Click(null, null);
            }

            btnOK_Click(null, null);
        }

        private SortableBindingList<PlayerBoxScore> pbsAwayList { get; set; }
        private SortableBindingList<PlayerBoxScore> pbsHomeList { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> playersListAway { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> playersListHome { get; set; }
        private ObservableCollection<PlayByPlayEntry> pbpeList { get; set; }

        private void finishInitialization(Mode mode)
        {
            _pst = MainWindow.PST;

            curMode = mode;
            prepareWindow(mode);

            MainWindow.TempBSE_BS = new TeamBoxScore();

            if (mode == Mode.Update)
            {
                curTeamBoxScore = new TeamBoxScore();
                FillTeamBoxScore(curTeamBoxScore);
                txtMINS1.Text = MainWindow.GameLength.ToString();
            }

            try
            {
                ProgressWindow.PwInstance.CanClose = true;
                ProgressWindow.PwInstance.Close();
            }
            catch
            {
                Console.WriteLine("ProgressWindow couldn't be closed; maybe it wasn't open.");
            }

            dgvPlayersAway.PreviewKeyDown += GenericEventHandlers.WPFDataGrid_PreviewKeyDown_GoToNextColumnOnEnter;
            dgvPlayersHome.PreviewKeyDown += GenericEventHandlers.WPFDataGrid_PreviewKeyDown_GoToNextColumnOnEnter;
            IsEnabled = true;
        }

        /// <summary>Finds the requested box score and loads it.</summary>
        /// <param name="id">The ID of the box score.</param>
        private void loadBoxScore(int id)
        {
            var bse = MainWindow.BSHist.Single(entry => entry.BS.ID == id);
            curTeamBoxScore = bse.BS;
            loadBoxScore(bse);
        }

        /// <summary>Loads the given box score.</summary>
        /// <param name="bse">The BoxScoreEntry to load.</param>
        private void loadBoxScore(BoxScoreEntry bse)
        {
            var bs = bse.BS;
            MainWindow.TempBSE_BS = bse.BS;
            FillTeamBoxScore(bs);

            dtpGameDate.SelectedDate = bs.GameDate;
            _curSeason = bs.SeasonNum;
            //LinkInternalsToMainWindow();
            chkIsPlayoff.IsChecked = bs.IsPlayoff;

            calculateScoreAway();
            calculateScoreHome();

            pbsAwayList = new SortableBindingList<PlayerBoxScore>();
            pbsHomeList = new SortableBindingList<PlayerBoxScore>();

            pbsAwayList.AllowNew = true;
            pbsAwayList.AllowEdit = true;
            pbsAwayList.AllowRemove = true;
            pbsAwayList.RaiseListChangedEvents = true;

            pbsHomeList.AllowNew = true;
            pbsHomeList.AllowEdit = true;
            pbsHomeList.AllowRemove = true;
            pbsHomeList.RaiseListChangedEvents = true;

            dgvPlayersAway.ItemsSource = pbsAwayList;
            dgvPlayersHome.ItemsSource = pbsHomeList;
            _loading = true;
            foreach (var pbs in bse.PBSList)
            {
                if (pbs.TeamID == bs.Team1ID)
                {
                    pbsAwayList.Add(pbs);
                }
                else
                {
                    pbsHomeList.Add(pbs);
                }
            }

            pbsAwayList.Sort((pbs1, pbs2) => (pbs2.MINS - pbs1.MINS));
            pbsHomeList.Sort((pbs1, pbs2) => (pbs2.MINS - pbs1.MINS));

            try
            {
                cmbTeam1.SelectedItem = MainWindow.TST[bs.Team1ID].DisplayName;
                cmbTeam2.SelectedItem = MainWindow.TST[bs.Team2ID].DisplayName;
            }
            catch
            {
                MessageBox.Show(
                    "One of the teams requested is disabled for this season. This box score is not available.\n"
                    + "To be able to see this box score, enable the teams included in it.");
                Close();
            }
            populateSeasonCombo();

            pbpeList = new ObservableCollection<PlayByPlayEntry>(bse.PBPEList);
            pbpeList.Sort(new PlayByPlayEntryComparerAsc());
            lstPlayByPlay.ItemsSource = pbpeList;

            _loading = false;
        }

        private void FillTeamBoxScore(TeamBoxScore bs)
        {
            txtPTS1.Text = bs.PTS1.ToString();
            txtREB1.Text = bs.REB1.ToString();
            txtAST1.Text = bs.AST1.ToString();
            txtSTL1.Text = bs.STL1.ToString();
            txtBLK1.Text = bs.BLK1.ToString();
            txtTO1.Text = bs.TOS1.ToString();
            txtFGM1.Text = bs.FGM1.ToString();
            txtFGA1.Text = bs.FGA1.ToString();
            txt3PM1.Text = bs.TPM1.ToString();
            txt3PA1.Text = bs.TPA1.ToString();
            txtFTM1.Text = bs.FTM1.ToString();
            txtFTA1.Text = bs.FTA1.ToString();
            txtOREB1.Text = bs.OREB1.ToString();
            txtFOUL1.Text = bs.FOUL1.ToString();
            txtMINS1.Text = bs.MINS1.ToString();
            txtPTS2.Text = bs.PTS2.ToString();
            txtREB2.Text = bs.REB2.ToString();
            txtAST2.Text = bs.AST2.ToString();
            txtSTL2.Text = bs.STL2.ToString();
            txtBLK2.Text = bs.BLK2.ToString();
            txtTO2.Text = bs.TOS2.ToString();
            txtFGM2.Text = bs.FGM2.ToString();
            txtFGA2.Text = bs.FGA2.ToString();
            txt3PM2.Text = bs.TPM2.ToString();
            txt3PA2.Text = bs.TPA2.ToString();
            txtFTM2.Text = bs.FTM2.ToString();
            txtFTA2.Text = bs.FTA2.ToString();
            txtOREB2.Text = bs.OREB2.ToString();
            txtFOUL2.Text = bs.FOUL2.ToString();
            txtMINS2.Text = bs.MINS2.ToString();
        }

        /// <summary>Updates the player box score data grid for the specified team.</summary>
        /// <param name="team">1 for the away team, anything else for the home team.</param>
        private void updateDataGrid(int team)
        {
            SortableBindingList<PlayerBoxScore> pbsList;
            int teamID;
            if (team == 1)
            {
                try
                {
                    teamID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString());
                    pbsList = pbsAwayList;
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                try
                {
                    teamID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam2.SelectedItem.ToString());
                    pbsList = pbsHomeList;
                }
                catch (Exception)
                {
                    return;
                }
            }

            ObservableCollection<KeyValuePair<int, string>> playersList;
            updateBoxScoreDataGrid(teamID, out playersList, ref pbsList, _loading);

            if (team == 1)
            {
                colPlayerAway.ItemsSource = playersList;
                playersListAway = playersList;
                pbsAwayList = pbsList;
                dgvPlayersAway.ItemsSource = pbsAwayList;
                //dgvPlayersAway.CanUserAddRows = false;
            }
            else
            {
                colPlayerHome.ItemsSource = playersList;
                playersListHome = playersList;
                pbsHomeList = pbsList;
                dgvPlayersHome.ItemsSource = pbsHomeList;
                //dgvPlayersHome.CanUserAddRows = false;
            }
        }

        /// <summary>Prepares the window based on the mode of function it was opened for.</summary>
        /// <param name="mode">The Mode enum instance which determines the function for which the window is opened.</param>
        private void prepareWindow(Mode mode)
        {
            _curSeason = MainWindow.CurSeason;

            populateSeasonCombo();

            populateTeamsCombo();

            _defaultBackground = cmbTeam1.Background;

            pbsAwayList = new SortableBindingList<PlayerBoxScore>();
            pbsHomeList = new SortableBindingList<PlayerBoxScore>();

            pbsAwayList.AllowNew = true;
            pbsAwayList.AllowEdit = true;
            pbsAwayList.AllowRemove = true;
            pbsAwayList.RaiseListChangedEvents = true;

            pbsHomeList.AllowNew = true;
            pbsHomeList.AllowEdit = true;
            pbsHomeList.AllowRemove = true;
            pbsHomeList.RaiseListChangedEvents = true;

            dgvPlayersAway.ItemsSource = pbsAwayList;
            dgvPlayersHome.ItemsSource = pbsHomeList;

            dgvPlayersAway.RowEditEnding += GenericEventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvPlayersAway.PreviewKeyDown += GenericEventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvPlayersAway.PreviewKeyUp += GenericEventHandlers.Any_PreviewKeyUp_CheckTab;
            dgvPlayersHome.RowEditEnding += GenericEventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvPlayersHome.PreviewKeyDown += GenericEventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvPlayersHome.PreviewKeyUp += GenericEventHandlers.Any_PreviewKeyUp_CheckTab;

            dgvPlayersAway.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayersHome.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvMetricAway.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvMetricHome.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            dgvMetricAwayEFFColumn.Visibility = Visibility.Collapsed;
            dgvMetricAwayPERColumn.Visibility = Visibility.Collapsed;
            dgvMetricAwayPPRColumn.Visibility = Visibility.Collapsed;

            dgvMetricHomeEFFColumn.Visibility = Visibility.Collapsed;
            dgvMetricHomePERColumn.Visibility = Visibility.Collapsed;
            dgvMetricHomePPRColumn.Visibility = Visibility.Collapsed;

            cmbTeam1.SelectedIndex = -1;
            cmbTeam2.SelectedIndex = -1;
            cmbTeam1.SelectedIndex = 0;
            cmbTeam2.SelectedIndex = 1;

            MainWindow.TempBSE_BS.Done = false;

            dtpGameDate.SelectedDate = DateTime.Today;

            pbpeList = new ObservableCollection<PlayByPlayEntry>();

            curMode = mode;

            calculateScoreAway();
            calculateScoreHome();

            if (curMode == Mode.View || curMode == Mode.ViewAndIgnore)
            {
                label1.Content = "";
                chkDoNotUpdate.Visibility = Visibility.Hidden;
                txbDoNotUpdate.Visibility = Visibility.Hidden;
                Title = "View & Edit Box Score";
            }

            if (curMode == Mode.ViewAndIgnore)
            {
                label1.Content = "Any changes made to the box score will be ignored.";
                label1.FontWeight = FontWeights.Bold;
                btnOK.Visibility = Visibility.Hidden;
                btnCalculateTeams.Visibility = Visibility.Hidden;
                btnCancel.Content = "Close";
                Title = "View Box Score";
            }
        }

        /// <summary>Populates the teams combo-box.</summary>
        private void populateTeamsCombo()
        {
            _teams = new List<string>();
            foreach (var kvp in MainWindow.TST)
            {
                if (!kvp.Value.IsHidden)
                {
                    _teams.Add(kvp.Value.DisplayName);
                }
            }

            _teams.Sort();

            cmbTeam1.ItemsSource = _teams;
            cmbTeam2.ItemsSource = _teams;
        }

        /// <summary>Populates the season combo-box.</summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.Items.Clear();

            for (var i = _maxSeason; i > 0; i--)
            {
                var addIt = true;
                if (cmbTeam1.SelectedItem != null)
                {
                    if (TeamStats.IsTeamHiddenInSeason(
                        MainWindow.CurrentDB, Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString()), i))
                    {
                        addIt = false;
                    }
                }
                if (cmbTeam2.SelectedItem != null)
                {
                    if (TeamStats.IsTeamHiddenInSeason(
                        MainWindow.CurrentDB, Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam2.SelectedItem.ToString()), i))
                    {
                        addIt = false;
                    }
                }
                if (addIt)
                {
                    cmbSeasonNum.Items.Add(i.ToString());
                }
            }

            cmbSeasonNum.SelectedIndex = -1;
            cmbSeasonNum.SelectedItem = _curSeason.ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (curMode == Mode.Update)
            {
                tryParseBS();
                if (MainWindow.TempBSE_BS.Done == false)
                {
                    return;
                }
            }
            else
            {
                if (curMode == Mode.View)
                {
                    var r = MessageBox.Show(
                        "Do you want to save any changes to this Box Score?",
                        "NBA Stats Tracker",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);
                    if (r == MessageBoxResult.Cancel)
                    {
                        return;
                    }

                    if (r == MessageBoxResult.Yes)
                    {
                        tryParseBS();
                        if (MainWindow.TempBSE_BS.Done == false)
                        {
                            return;
                        }

                        MainWindow.UpdateBoxScore();
                        DialogResult = true;
                    }
                    else
                    {
                        MainWindow.TempBSE_BS.Done = false;
                        DialogResult = false;
                    }
                }
            }
            _clickedOK = true;
            Close();
        }

        /// <summary>Tries to the parse the current team & player box scores, and check for any errors.</summary>
        /// <exception cref="System.Exception"></exception>
        private void tryParseBS(bool ignoreScores = false)
        {
            try
            {
                if (cmbTeam1.SelectedItem.ToString() == cmbTeam2.SelectedItem.ToString())
                {
                    throwErrorWithMessage("You can't have the same team in both Home & Away.");
                }
                if (String.IsNullOrWhiteSpace(txtPTS1.Text) || txtPTS1.Text == "N/A" || String.IsNullOrWhiteSpace(txtPTS2.Text)
                    || txtPTS2.Text == "N/A")
                {
                    throwErrorWithMessage("The Box Score is incomplete. Make sure you input all stats.");
                }
                if (cmbSeasonNum.SelectedIndex == -1)
                {
                    throwErrorWithMessage("You have to choose a season.");
                }
                try
                {
                    MainWindow.TempBSE_BS.ID = curTeamBoxScore.ID;
                }
                catch
                {
                    MainWindow.TempBSE_BS.ID = -1;
                }
                MainWindow.TempBSE_BS.IsPlayoff = chkIsPlayoff.IsChecked.GetValueOrDefault();
                MainWindow.TempBSE_BS.GameDate = dtpGameDate.SelectedDate.GetValueOrDefault();
                MainWindow.TempBSE_BS.SeasonNum = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
                MainWindow.TempBSE_BS.Team1ID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString());
                MainWindow.TempBSE_BS.Team2ID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam2.SelectedItem.ToString());
                MainWindow.TempBSE_BS.MINS2 = MainWindow.TempBSE_BS.MINS1 = Convert.ToUInt16(txtMINS1.Text);

                var teamName = cmbTeam1.SelectedItem.ToString();
                if (MainWindow.TempBSE_BS.MINS1 <= 0)
                {
                    throwErrorWithMessage(
                        "You have to enter the game's minutes. Usually 48 for 4 quarters, 53 for 1 overtime, 58 for 2 overtimes.",
                        teamName);
                }

                MainWindow.TempBSE_BS.PTS1 = Convert.ToUInt16(txtPTS1.Text);
                MainWindow.TempBSE_BS.REB1 = Convert.ToUInt16(txtREB1.Text);
                MainWindow.TempBSE_BS.AST1 = Convert.ToUInt16(txtAST1.Text);
                MainWindow.TempBSE_BS.STL1 = Convert.ToUInt16(txtSTL1.Text);
                MainWindow.TempBSE_BS.BLK1 = Convert.ToUInt16(txtBLK1.Text);
                MainWindow.TempBSE_BS.TOS1 = Convert.ToUInt16(txtTO1.Text);
                MainWindow.TempBSE_BS.FGM1 = Convert.ToUInt16(txtFGM1.Text);
                MainWindow.TempBSE_BS.FGA1 = Convert.ToUInt16(txtFGA1.Text);
                MainWindow.TempBSE_BS.TPM1 = Convert.ToUInt16(txt3PM1.Text);
                MainWindow.TempBSE_BS.TPA1 = Convert.ToUInt16(txt3PA1.Text);

                if (MainWindow.TempBSE_BS.FGA1 < MainWindow.TempBSE_BS.FGM1)
                {
                    throwErrorWithMessage("The FGM stat can't be higher than the FGA stat.", teamName);
                }
                if (MainWindow.TempBSE_BS.TPA1 < MainWindow.TempBSE_BS.TPM1)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the 3PA stat.", teamName);
                }
                if (MainWindow.TempBSE_BS.FGM1 < MainWindow.TempBSE_BS.TPM1)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the FGM stat.", teamName);
                }

                MainWindow.TempBSE_BS.FTM1 = Convert.ToUInt16(txtFTM1.Text);
                MainWindow.TempBSE_BS.FTA1 = Convert.ToUInt16(txtFTA1.Text);
                if (MainWindow.TempBSE_BS.FTA1 < MainWindow.TempBSE_BS.FTM1)
                {
                    throwErrorWithMessage("The FTM stat can't be higher than the FTA stat.", teamName);
                }

                MainWindow.TempBSE_BS.OREB1 = Convert.ToUInt16(txtOREB1.Text);
                if (MainWindow.TempBSE_BS.OREB1 > MainWindow.TempBSE_BS.REB1)
                {
                    throwErrorWithMessage("The OFF stat can't be higher than the REB stat.", teamName);
                }
                if (MainWindow.TempBSE_BS.FGA1 < MainWindow.TempBSE_BS.TPA1)
                {
                    throwErrorWithMessage("The 3PA stat can't be higher than the FGA stat.", teamName);
                }

                MainWindow.TempBSE_BS.FOUL1 = Convert.ToUInt16(txtFOUL1.Text);

                teamName = cmbTeam2.SelectedItem.ToString();
                MainWindow.TempBSE_BS.PTS2 = Convert.ToUInt16(txtPTS2.Text);
                if (!ignoreScores && MainWindow.TempBSE_BS.PTS1 == MainWindow.TempBSE_BS.PTS2)
                {
                    throwErrorWithMessage(
                        "The points can't be the same for both teams. NBA Stats Tracker doesn't support games that end in draw.");
                }
                MainWindow.TempBSE_BS.REB2 = Convert.ToUInt16(txtREB2.Text);
                MainWindow.TempBSE_BS.AST2 = Convert.ToUInt16(txtAST2.Text);
                MainWindow.TempBSE_BS.STL2 = Convert.ToUInt16(txtSTL2.Text);
                MainWindow.TempBSE_BS.BLK2 = Convert.ToUInt16(txtBLK2.Text);
                MainWindow.TempBSE_BS.TOS2 = Convert.ToUInt16(txtTO2.Text);
                MainWindow.TempBSE_BS.FGM2 = Convert.ToUInt16(txtFGM2.Text);
                MainWindow.TempBSE_BS.FGA2 = Convert.ToUInt16(txtFGA2.Text);
                MainWindow.TempBSE_BS.TPM2 = Convert.ToUInt16(txt3PM2.Text);
                MainWindow.TempBSE_BS.TPA2 = Convert.ToUInt16(txt3PA2.Text);

                if (MainWindow.TempBSE_BS.FGA2 < MainWindow.TempBSE_BS.FGM2)
                {
                    throwErrorWithMessage("The FGM stat can't be higher than the FGA stat.", teamName);
                }
                if (MainWindow.TempBSE_BS.TPA2 < MainWindow.TempBSE_BS.TPM2)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the 3PA stat.", teamName);
                }
                if (MainWindow.TempBSE_BS.FGM2 < MainWindow.TempBSE_BS.TPM2)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the FGM stat.", teamName);
                }
                if (MainWindow.TempBSE_BS.FGA2 < MainWindow.TempBSE_BS.TPA2)
                {
                    throwErrorWithMessage("The 3PA stat can't be higher than the FGA stat.", teamName);
                }

                MainWindow.TempBSE_BS.FTM2 = Convert.ToUInt16(txtFTM2.Text);
                MainWindow.TempBSE_BS.FTA2 = Convert.ToUInt16(txtFTA2.Text);
                if (MainWindow.TempBSE_BS.FTA2 < MainWindow.TempBSE_BS.FTM2)
                {
                    throwErrorWithMessage("The FTM stat can't be higher than the FTA stat.", teamName);
                }

                MainWindow.TempBSE_BS.OREB2 = Convert.ToUInt16(txtOREB2.Text);

                if (MainWindow.TempBSE_BS.OREB2 > MainWindow.TempBSE_BS.REB2)
                {
                    throwErrorWithMessage("The OFF stat can't be higher than the REB stat.", teamName);
                }

                MainWindow.TempBSE_BS.FOUL2 = Convert.ToUInt16(txtFOUL2.Text);

                #region Additional Box Score Checks

                if (MainWindow.TempBSE_BS.AST1 > MainWindow.TempBSE_BS.FGM1 || MainWindow.TempBSE_BS.AST2 > MainWindow.TempBSE_BS.FGM2)
                {
                    throwErrorWithMessage("The AST stat can't be higher than the FGM stat.");
                }

                if (MainWindow.TempBSE_BS.BLK1 > MainWindow.TempBSE_BS.FGA2 - MainWindow.TempBSE_BS.FGM2
                    || MainWindow.TempBSE_BS.BLK2 > MainWindow.TempBSE_BS.FGA1 - MainWindow.TempBSE_BS.FGM1)
                {
                    throwErrorWithMessage(
                        "The BLK stat for one team can't be higher than the other team's missed FGA (i.e. FGA - FGM).");
                }

                if (MainWindow.TempBSE_BS.REB1 - MainWindow.TempBSE_BS.OREB1 > MainWindow.TempBSE_BS.FGA2 - MainWindow.TempBSE_BS.FGM2
                    || MainWindow.TempBSE_BS.REB2 - MainWindow.TempBSE_BS.OREB2
                    > MainWindow.TempBSE_BS.FGA1 - MainWindow.TempBSE_BS.FGM1)
                {
                    throwErrorWithMessage(
                        "The DREB (i.e. REB - OREB) stat for one team can't be higher than the other team's missed FGA (i.e. FGA - FGM).");
                }

                if (MainWindow.TempBSE_BS.OREB1 > MainWindow.TempBSE_BS.FGA1 - MainWindow.TempBSE_BS.FGM1
                    || MainWindow.TempBSE_BS.OREB2 > MainWindow.TempBSE_BS.FGA2 - MainWindow.TempBSE_BS.FGM2)
                {
                    throwErrorWithMessage("The OREB stat cant' be higher than the missed FGA (i.e. FGA - FGM).");
                }

                if (MainWindow.TempBSE_BS.STL1 > MainWindow.TempBSE_BS.TOS2 || MainWindow.TempBSE_BS.STL2 > MainWindow.TempBSE_BS.TOS1)
                {
                    throwErrorWithMessage("The STL stat for one team can't be higher than the other team's TO.");
                }

                // TODO: Need to handle the possibility of technical fouls somehow.
                /*
                if (MainWindow.TempBSE_BS.FGA1 > MainWindow.TempBSE_BS.FOUL2 * 3 || MainWindow.TempBSE_BS.FGA2 > MainWindow.TempBSE_BS.FOUL1 * 3)
                {
                    throwErrorWithMessage("The FTA stat for one team can't be more than 3 times the other team's FOUL stat.");
                }
                */

                #endregion

                MainWindow.TempBSE_BS.DoNotUpdate = chkDoNotUpdate.IsChecked.GetValueOrDefault();

                #region Player Box Scores Check

                var team1 = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString());
                var team2 = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam2.SelectedItem.ToString());

                foreach (var pbs in pbsAwayList)
                {
                    pbs.TeamID = team1;
                }

                foreach (var pbs in pbsHomeList)
                {
                    pbs.TeamID = team2;
                }

                var pbsLists = new List<SortableBindingList<PlayerBoxScore>>(2) { pbsAwayList, pbsHomeList };
                var allPlayers = playersListAway.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                foreach (var kvp in playersListHome)
                {
                    allPlayers.Add(kvp.Key, kvp.Value);
                }
                var teamIter = 0;
                foreach (var pbsList in pbsLists)
                {
                    teamIter++;
                    teamName = teamIter == 1 ? cmbTeam1.SelectedItem.ToString() : cmbTeam2.SelectedItem.ToString();
                    var starters = 0;
                    foreach (var pbs in pbsList)
                    {
                        //pbs.PlayerID = 
                        if (pbs.PlayerID == -1)
                        {
                            continue;
                        }

                        var isOut = pbs.MINS == 0;

                        if (isOut)
                        {
                            if (pbs.FGM > 0 || pbs.FGA > 0 || pbs.TPM > 0 || pbs.TPA > 0 || pbs.FTM > 0 || pbs.FTA > 0 || pbs.REB > 0
                                || pbs.DREB > 0 || pbs.OREB > 0 || pbs.BLK > 0 || pbs.STL > 0 || pbs.TOS > 0 || pbs.AST > 0
                                || pbs.FOUL > 0)
                            {
                                var s = "The player can't have both 0 minutes and more than 0 in any of his stats. If he played "
                                        + "at all, his minutes should be at least 1.";
                                s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                                MessageBox.Show(s);
                                throw (new Exception(s));
                            }
                        }

                        pbs.IsOut = isOut;

                        if (pbs.IsOut)
                        {
                            pbs.ResetStats();
                            continue;
                        }

                        if (pbs.IsStarter)
                        {
                            starters++;
                            if (starters > 5)
                            {
                                var s = "There can't be more than 5 starters in each team.";
                                s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                                MessageBox.Show(s);
                                throw (new Exception(s));
                            }
                        }

                        if (pbs.FGM > pbs.FGA)
                        {
                            var s = "The FGM stat can't be higher than the FGA stat.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception(s));
                        }

                        if (pbs.TPM > pbs.TPA)
                        {
                            var s = "The 3PM stat can't be higher than the 3PA stat.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception(s));
                        }

                        if (pbs.FGM < pbs.TPM)
                        {
                            var s = "The 3PM stat can't be higher than the FGM stat.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception(s));
                        }

                        if (pbs.FGA < pbs.TPA)
                        {
                            var s = "The TPA stat can't be higher than the FGA stat.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.FTM > pbs.FTA)
                        {
                            var s = "The FTM stat can't be higher than the FTA stat.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception(s));
                        }

                        if (pbs.OREB > pbs.REB)
                        {
                            var s = "The OREB stat can't be higher than the REB stat.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception(s));
                        }

                        if (pbs.IsStarter && pbs.MINS == 0)
                        {
                            var s = "A player can't be a starter but not have any minutes played.";
                            s += "\n\nTeam: " + teamName + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception(s));
                        }

                        pbs.DREB = (UInt16) (pbs.REB - pbs.OREB);
                    }
                }
                MainWindow.TempBSE_PBSLists = pbsLists;

                MainWindow.TempBSE_PBPEList = new List<PlayByPlayEntry>(pbpeList);

                #endregion

                MainWindow.TempBSE_BS.Done = true;
            }
            catch
            {
                //MessageBox.Show("The Box Score seems to be invalid. Check that there's no stats missing.");
                MainWindow.TempBSE_BS.Done = false;
            }
        }

        /// <summary>Throws an exception after showing a friendly error message to the user.</summary>
        /// <param name="msg">The message displayed to the user and used for the exception.</param>
        /// <param name="teamName">The name of the team to be included in the message as having an issue.</param>
        /// <exception cref="System.Exception"></exception>
        private void throwErrorWithMessage(string msg, string teamName = "")
        {
            msg += String.IsNullOrWhiteSpace(teamName) ? "" : "\nTeam: " + teamName;
            MessageBox.Show(msg, "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
            throw (new Exception(msg));
        }

        /// <summary>Handles the SelectionChanged event of the cmbTeam1 control. Updates the tab headers and corresponding data grid.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam1.Header = cmbTeam1.SelectedItem;
            tabAwayMetric.Header = cmbTeam1.SelectedItem + " Metric Stats";
            grpAway.Header = cmbTeam1.SelectedItem;
            updateDataGrid(1);
        }

        /// <summary>Handles the SelectionChanged event of the cmbTeam2 control. Updates the tab headers and corresponding data grid.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam2.Header = cmbTeam2.SelectedItem;
            tabHomeMetric.Header = cmbTeam2.SelectedItem + " Metric Stats";
            grpHome.Header = cmbTeam2.SelectedItem;
            updateDataGrid(2);
        }

        /// <summary>Checks if the same team is selected as home and away, and changes the combo-box color to reflect that.</summary>
        private void checkIfSameTeams()
        {
            string team1, team2;
            try
            {
                team1 = cmbTeam1.SelectedItem.ToString();
                team2 = cmbTeam2.SelectedItem.ToString();
            }
            catch (Exception)
            {
                return;
            }

            if (team1 == team2)
            {
                cmbTeam1.Background = Brushes.Red;
                cmbTeam2.Background = Brushes.Red;
                return;
            }

            cmbTeam1.Background = _defaultBackground;
            cmbTeam2.Background = _defaultBackground;
        }

        /// <summary>Tries to calculate the score and PerGame for the Away team.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="TextChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void calculateScoreAway(object sender = null, TextChangedEventArgs e = null)
        {
            int pts1;
            string t1Avg;
            int ftm;
            int tpm;
            int fgm;
            try
            {
                fgm = Convert.ToInt32(txtFGM1.Text);
                tpm = Convert.ToInt32(txt3PM1.Text);
                ftm = Convert.ToInt32(txtFTM1.Text);
            }
            catch
            {
                return;
            }

            try
            {
                var fga = Convert.ToInt32(txtFGA1.Text);
                var tpa = Convert.ToInt32(txt3PA1.Text);
                var fta = Convert.ToInt32(txtFTA1.Text);

                EventHandlers.CalculateScore(fgm, fga, tpm, tpa, ftm, fta, out pts1, out t1Avg);
            }
            catch
            {
                EventHandlers.CalculateScore(fgm, null, tpm, null, ftm, null, out pts1, out t1Avg);
            }
            txtPTS1.Text = pts1.ToString();
            txbT1Avg.Text = t1Avg;
        }

        /// <summary>Tries to calculate the score and PerGame for the Home team.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">
        ///     The <see cref="TextChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void calculateScoreHome(object sender = null, TextChangedEventArgs e = null)
        {
            int pts2;
            string t2Avg;
            int ftm;
            int tpm;
            int fgm;
            try
            {
                fgm = Convert.ToInt32(txtFGM2.Text);
                tpm = Convert.ToInt32(txt3PM2.Text);
                ftm = Convert.ToInt32(txtFTM2.Text);
            }
            catch
            {
                return;
            }

            try
            {
                var fga = Convert.ToInt32(txtFGA2.Text);
                var tpa = Convert.ToInt32(txt3PA2.Text);
                var fta = Convert.ToInt32(txtFTA2.Text);

                EventHandlers.CalculateScore(fgm, fga, tpm, tpa, ftm, fta, out pts2, out t2Avg);
            }
            catch
            {
                EventHandlers.CalculateScore(fgm, null, tpm, null, ftm, null, out pts2, out t2Avg);
            }
            txtPTS2.Text = pts2.ToString();
            txbT2Avg.Text = t2Avg;
        }

        /// <summary>
        ///     Handles the Click event of the btnCopy control. Copies the current team and player box scores as tab-separated values to the
        ///     clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            tryParseBS();
            if (MainWindow.TempBSE_BS.Done)
            {
                var data1 =
                    String.Format(
                        "{0}\t\t\t\t{18}\t{1}\t{2}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10:F3}\t{11}\t{12}\t{13:F3}\t{14}\t{15}\t{16:F3}\t{3}\t{17}",
                        cmbTeam1.SelectedItem,
                        MainWindow.TempBSE_BS.PTS1,
                        MainWindow.TempBSE_BS.REB1,
                        MainWindow.TempBSE_BS.OREB1,
                        MainWindow.TempBSE_BS.AST1,
                        MainWindow.TempBSE_BS.STL1,
                        MainWindow.TempBSE_BS.BLK1,
                        MainWindow.TempBSE_BS.TOS1,
                        MainWindow.TempBSE_BS.FGM1,
                        MainWindow.TempBSE_BS.FGA1,
                        MainWindow.TempBSE_BS.FGM1 / (float) MainWindow.TempBSE_BS.FGA1,
                        MainWindow.TempBSE_BS.TPM1,
                        MainWindow.TempBSE_BS.TPA1,
                        MainWindow.TempBSE_BS.TPM1 / (float) MainWindow.TempBSE_BS.TPA1,
                        MainWindow.TempBSE_BS.FTM1,
                        MainWindow.TempBSE_BS.FTA1,
                        MainWindow.TempBSE_BS.FTM1 / (float) MainWindow.TempBSE_BS.FTA1,
                        MainWindow.TempBSE_BS.FOUL1,
                        MainWindow.TempBSE_BS.MINS1);

                var data2 =
                    String.Format(
                        "{0}\t\t\t\t{18}\t{1}\t{2}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10:F3}\t{11}\t{12}\t{13:F3}\t{14}\t{15}\t{16:F3}\t{3}\t{17}",
                        cmbTeam2.SelectedItem,
                        MainWindow.TempBSE_BS.PTS2,
                        MainWindow.TempBSE_BS.REB2,
                        MainWindow.TempBSE_BS.OREB2,
                        MainWindow.TempBSE_BS.AST2,
                        MainWindow.TempBSE_BS.STL2,
                        MainWindow.TempBSE_BS.BLK2,
                        MainWindow.TempBSE_BS.TOS2,
                        MainWindow.TempBSE_BS.FGM2,
                        MainWindow.TempBSE_BS.FGA2,
                        MainWindow.TempBSE_BS.FGM2 / (float) MainWindow.TempBSE_BS.FGA2,
                        MainWindow.TempBSE_BS.TPM2,
                        MainWindow.TempBSE_BS.TPA2,
                        MainWindow.TempBSE_BS.TPM2 / (float) MainWindow.TempBSE_BS.TPA2,
                        MainWindow.TempBSE_BS.FTM2,
                        MainWindow.TempBSE_BS.FTA2,
                        MainWindow.TempBSE_BS.FTM2 / (float) MainWindow.TempBSE_BS.FTA2,
                        MainWindow.TempBSE_BS.FOUL2,
                        MainWindow.TempBSE_BS.MINS2);

                dgvPlayersAway.SelectAllCells();
                ApplicationCommands.Copy.Execute(null, dgvPlayersAway);
                dgvPlayersAway.UnselectAllCells();
                var result1 = (string) Clipboard.GetData(DataFormats.Text);
                dgvPlayersHome.SelectAllCells();
                ApplicationCommands.Copy.Execute(null, dgvPlayersHome);
                dgvPlayersHome.UnselectAllCells();
                var result2 = (string) Clipboard.GetData(DataFormats.Text);

                var result = result1 + "\n" + data1 + "\n\n\n" + result2 + "\n" + data2;
                Clipboard.SetText(result);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.TempBSE_BS.Done = false;
            Close();
        }

        /// <summary>
        ///     Handles the GotFocus event of the _bsAnyTextbox control. Any time a textbox with this event handler gets focus, its text gets
        ///     selected so that it can be easily replaced.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void bsAnyTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.SelectAll();
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control. Loads the new season's team and player stats dictionaries and
        ///     other information, and repopulates the combo-boxes.
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

            _curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
            //MainWindow.ChangeSeason(curSeason, MainWindow.getMaxSeason(MainWindow.currentDB));

            if (!_onImport)
            {
                if (_curSeason != MainWindow.Tf.SeasonNum)
                {
                    SQLiteIO.LoadSeason(MainWindow.CurrentDB, _curSeason, doNotLoadBoxScores: true);
                }

                if (_curSeason != _maxSeason)
                {
                }
            }
            populateTeamsCombo();
        }

        /// <summary>
        ///     Handles the Click event of the btnCalculateTeams control. Calculates the stats for each team from the totals of the
        ///     corresponding player stats.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnCalculateTeams_Click(object sender, RoutedEventArgs e)
        {
            var bs = new TeamBoxScore();
            CalculateTeamsFromPlayers(ref bs, pbsAwayList, pbsHomeList);

            txtREB1.Text = bs.REB1.ToString();
            txtAST1.Text = bs.AST1.ToString();
            txtSTL1.Text = bs.STL1.ToString();
            txtBLK1.Text = bs.BLK1.ToString();
            txtTO1.Text = bs.TOS1.ToString();
            txtFGM1.Text = bs.FGM1.ToString();
            txtFGA1.Text = bs.FGA1.ToString();
            txt3PM1.Text = bs.TPM1.ToString();
            txt3PA1.Text = bs.TPA1.ToString();
            txtFTM1.Text = bs.FTM1.ToString();
            txtFTA1.Text = bs.FTA1.ToString();
            txtOREB1.Text = bs.OREB1.ToString();
            txtFOUL1.Text = bs.FOUL1.ToString();

            txtREB2.Text = bs.REB2.ToString();
            txtAST2.Text = bs.AST2.ToString();
            txtSTL2.Text = bs.STL2.ToString();
            txtBLK2.Text = bs.BLK2.ToString();
            txtTO2.Text = bs.TOS2.ToString();
            txtFGM2.Text = bs.FGM2.ToString();
            txtFGA2.Text = bs.FGA2.ToString();
            txt3PM2.Text = bs.TPM2.ToString();
            txt3PA2.Text = bs.TPA2.ToString();
            txtFTM2.Text = bs.FTM2.ToString();
            txtFTA2.Text = bs.FTA2.ToString();
            txtOREB2.Text = bs.OREB2.ToString();
            txtFOUL2.Text = bs.FOUL2.ToString();

            calculateScoreAway();
            calculateScoreHome();
        }

        public static void CalculateTeamsFromPlayers(
            ref TeamBoxScore bs, IEnumerable<PlayerBoxScore> awayPBS, IEnumerable<PlayerBoxScore> homePBS)
        {
            ushort reb = 0,
                   ast = 0,
                   stl = 0,
                   tos = 0,
                   blk = 0,
                   fgm = 0,
                   fga = 0,
                   tpm = 0,
                   tpa = 0,
                   ftm = 0,
                   fta = 0,
                   oreb = 0,
                   foul = 0;

            foreach (var pbs in awayPBS)
            {
                reb += pbs.REB;
                ast += pbs.AST;
                stl += pbs.STL;
                tos += pbs.TOS;
                blk += pbs.BLK;
                fgm += pbs.FGM;
                fga += pbs.FGA;
                tpm += pbs.TPM;
                tpa += pbs.TPA;
                ftm += pbs.FTM;
                fta += pbs.FTA;
                oreb += pbs.OREB;
                foul += pbs.FOUL;
            }

            bs.REB1 = reb;
            bs.AST1 = ast;
            bs.STL1 = stl;
            bs.BLK1 = blk;
            bs.TOS1 = tos;
            bs.FGM1 = fgm;
            bs.FGA1 = fga;
            bs.TPM1 = tpm;
            bs.TPA1 = tpa;
            bs.FTM1 = ftm;
            bs.FTA1 = fta;
            bs.OREB1 = oreb;
            bs.FOUL1 = foul;

            bs.PTS1 = (ushort) (bs.FGM1 * 2 + bs.TPM1 + bs.FTM1);

            reb = 0;
            ast = 0;
            stl = 0;
            tos = 0;
            blk = 0;
            fgm = 0;
            fga = 0;
            tpm = 0;
            tpa = 0;
            ftm = 0;
            fta = 0;
            oreb = 0;
            foul = 0;

            foreach (var pbs in homePBS)
            {
                reb += pbs.REB;
                ast += pbs.AST;
                stl += pbs.STL;
                tos += pbs.TOS;
                blk += pbs.BLK;
                fgm += pbs.FGM;
                fga += pbs.FGA;
                tpm += pbs.TPM;
                tpa += pbs.TPA;
                ftm += pbs.FTM;
                fta += pbs.FTA;
                oreb += pbs.OREB;
                foul += pbs.FOUL;
            }

            bs.REB2 = reb;
            bs.AST2 = ast;
            bs.STL2 = stl;
            bs.BLK2 = blk;
            bs.TOS2 = tos;
            bs.FGM2 = fgm;
            bs.FGA2 = fga;
            bs.TPM2 = tpm;
            bs.TPA2 = tpa;
            bs.FTM2 = ftm;
            bs.FTA2 = fta;
            bs.OREB2 = oreb;
            bs.FOUL2 = foul;

            bs.PTS2 = (ushort) (bs.FGM2 * 2 + bs.TPM2 + bs.FTM2);
        }

        /// <summary>
        ///     Handles the CopyingCellClipboardContent event of the colPlayerAway control. Uses a custom CopyingCellCpiboardContent event
        ///     handler that replaces the player ID with the player name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.
        /// </param>
        private void colPlayerAway_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, playersListAway);
        }

        /// <summary>
        ///     Handles the CopyingCellClipboardContent event of the colPlayerHome control. Uses a custom CopyingCellCpiboardContent event
        ///     handler that replaces the player ID with the player name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.
        /// </param>
        private void colPlayerHome_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, playersListHome);
        }

        /// <summary>
        ///     Handles the CopyingCellClipboardContent event of the PercentageColumn control. Uses a custom CopyingCellClipboardContent
        ///     event handler that formats the percentage as a string before copying it to the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.
        /// </param>
        private void percentageColumn_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PercentageColumn_CopyingCellClipboardContent(e);
        }

        /// <summary>Handles the TextChanged event of the txtMINS1 control. Makes sure both teams have the same minutes played for the game.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="TextChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void txtMINS1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_minsUpdating)
            {
                _minsUpdating = true;
                txtMINS2.Text = txtMINS1.Text;
                _minsUpdating = false;
            }
        }

        /// <summary>Handles the TextChanged event of the txtMINS2 control. Makes sure both teams have the same minutes played for the game.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="TextChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void txtMINS2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_minsUpdating)
            {
                _minsUpdating = true;
                txtMINS1.Text = txtMINS2.Text;
                _minsUpdating = false;
            }
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the tabControl1 control. Updates the metric stats for each team's players and
        ///     calculates the best performers on-demand.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is TabControl)
            {
                if (Equals(tabControl1.SelectedItem, tabAwayMetric))
                {
                    updateMetric(1);
                }
                else if (Equals(tabControl1.SelectedItem, tabHomeMetric))
                {
                    updateMetric(2);
                }
                else if (Equals(tabControl1.SelectedItem, tabBest))
                {
                    updateMetric(1);
                    updateMetric(2);
                    updateBest();
                }
            }
        }

        /// <summary>Calculates the best performers from each team, and their most significant stats.</summary>
        private void updateBest()
        {
            try
            {
                if (_pmsrListAway.Count == 0 && _pmsrListHome.Count == 0)
                {
                    return;
                }

                _pmsrListAway.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                _pmsrListAway.Reverse();

                _pmsrListHome.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                _pmsrListHome.Reverse();
            }
            catch (Exception)
            {
                return;
            }

            string teamBest;
            int awayid, homeid;
            var pbsBest = new PlayerBoxScore();
            PlayerBoxScore pbsAway1 = new PlayerBoxScore(),
                           pbsAway2 = new PlayerBoxScore(),
                           pbsAway3 = new PlayerBoxScore(),
                           pbsHome1 = new PlayerBoxScore(),
                           pbsHome2 = new PlayerBoxScore(),
                           pbsHome3 = new PlayerBoxScore();

            txbMVP.Text = "";
            txbMVPStats.Text = "";
            txbAway1.Text = "";
            txbAway2.Text = "";
            txbAway3.Text = "";
            txbHome1.Text = "";
            txbHome2.Text = "";
            txbHome3.Text = "";

            var skipaway = _pmsrListAway.Count == 0;
            var skiphome = _pmsrListHome.Count == 0;

            //if (skiphome || (!skipaway && pmsrListAway[0].GmSc > pmsrListHome[0].GmSc))
            if (skiphome || (!skipaway && Convert.ToInt32(txtPTS1.Text) > Convert.ToInt32(txtPTS2.Text)))
            {
                var bestID = _pmsrListAway[0].ID;
                foreach (var pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == bestID)
                    {
                        pbsBest = pbs;
                    }
                }
                teamBest = cmbTeam1.SelectedItem.ToString();
                awayid = 1;
                homeid = 0;
            }
            else
            {
                var bestID = _pmsrListHome[0].ID;
                foreach (var pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == bestID)
                    {
                        pbsBest = pbs;
                    }
                }
                teamBest = cmbTeam2.SelectedItem.ToString();
                awayid = 0;
                homeid = 1;
            }

            var ps = _pst[pbsBest.PlayerID];
            var text = pbsBest.GetBestStats(5, ps.Position1);
            txbMVP.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")";
            txbMVPStats.Text = teamBest + "\n\n" + text;

            if (_pmsrListAway.Count > awayid)
            {
                var id2 = _pmsrListAway[awayid++].ID;
                foreach (var pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == id2)
                    {
                        pbsAway1 = pbs;
                    }
                }

                ps = _pst[pbsAway1.PlayerID];
                text = pbsAway1.GetBestStats(5, ps.Position1);
                txbAway1.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (_pmsrListAway.Count > awayid)
            {
                var id3 = _pmsrListAway[awayid++].ID;
                foreach (var pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == id3)
                    {
                        pbsAway2 = pbs;
                    }
                }

                ps = _pst[pbsAway2.PlayerID];
                text = pbsAway2.GetBestStats(5, ps.Position1);
                txbAway2.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (_pmsrListAway.Count > awayid)
            {
                var id4 = _pmsrListAway[awayid++].ID;
                foreach (var pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == id4)
                    {
                        pbsAway3 = pbs;
                    }
                }

                ps = _pst[pbsAway3.PlayerID];
                text = pbsAway3.GetBestStats(5, ps.Position1);
                txbAway3.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (_pmsrListHome.Count > homeid)
            {
                var id2 = _pmsrListHome[homeid++].ID;
                foreach (var pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == id2)
                    {
                        pbsHome1 = pbs;
                    }
                }

                ps = _pst[pbsHome1.PlayerID];
                text = pbsHome1.GetBestStats(5, ps.Position1);
                txbHome1.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (_pmsrListHome.Count > homeid)
            {
                var id3 = _pmsrListHome[homeid++].ID;
                foreach (var pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == id3)
                    {
                        pbsHome2 = pbs;
                    }
                }

                ps = _pst[pbsHome2.PlayerID];
                text = pbsHome2.GetBestStats(5, ps.Position1);
                txbHome2.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (_pmsrListHome.Count > homeid)
            {
                var id4 = _pmsrListHome[homeid++].ID;
                foreach (var pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == id4)
                    {
                        pbsHome3 = pbs;
                    }
                }

                ps = _pst[pbsHome3.PlayerID];
                text = pbsHome3.GetBestStats(5, ps.Position1);
                txbHome3.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }
        }

        /// <summary>Updates the metric stats for the specified team's players.</summary>
        /// <param name="team">1 if the away team's players' metric stats should be updated; anything else for the home team.</param>
        private void updateMetric(int team)
        {
            var ts = new TeamStats(-1, cmbTeam1.SelectedItem.ToString());
            var tsopp = new TeamStats(-1, cmbTeam2.SelectedItem.ToString());

            tryParseBS();
            if (!MainWindow.TempBSE_BS.Done)
            {
                return;
            }

            var bs = MainWindow.TempBSE_BS;

            if (team == 1)
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            }
            else
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);
            }

            ts.CalcMetrics(tsopp);

            var pmsrList = new List<PlayerStatsRow>();

            var pbsList = team == 1 ? pbsAwayList : pbsHomeList;

            foreach (var pbs in pbsList)
            {
                if (pbs.PlayerID == -1)
                {
                    continue;
                }

                var ps = _pst[pbs.PlayerID].Clone();
                ps.ResetStats();
                ps.AddBoxScore(pbs, bs.IsPlayoff);
                ps.CalcMetrics(ts, tsopp, new TeamStats(-1));
                pmsrList.Add(new PlayerStatsRow(ps));
            }

            pmsrList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            pmsrList.Reverse();

            if (team == 1)
            {
                _pmsrListAway = new List<PlayerStatsRow>(pmsrList);
                dgvMetricAway.ItemsSource = _pmsrListAway;
            }
            else
            {
                _pmsrListHome = new List<PlayerStatsRow>(pmsrList);
                dgvMetricHome.ItemsSource = _pmsrListHome;
            }
        }

        /// <summary>
        ///     Handles the Click event of the btnPaste control. Imports the team and player box scores, if any, from the tab-separated
        ///     values in the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            var text = Clipboard.GetText();
            var lines = Tools.SplitLinesToArray(text);
            var found = 0;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                string team;
                if (line.StartsWith("\t") && !(lines[i + 1].StartsWith("\t")))
                {
                    team = lines[i + 1].Split('\t')[0];
                }
                else
                {
                    continue;
                }

                if (found == 0)
                {
                    cmbTeam1.SelectedItem = team;
                }
                else
                {
                    cmbTeam2.SelectedItem = team;
                }

                found++;
                i += 4;

                if (found == 2)
                {
                    break;
                }
            }
            var dictList = CSV.DictionaryListFromTSVString(text);

            var status = 0;
            for (var j = 0; j < dictList.Count; j++)
            {
                var dict = dictList[j];
                string name;
                try
                {
                    name = dict["Player"];
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "Couldn't detect a player's name in the pasted data. "
                        + "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there.");
                    return;
                }

                if (name == ""
                    && (dictList[j + 1]["Player"] == cmbTeam1.SelectedItem.ToString()
                        || dictList[j + 1]["Player"] == cmbTeam2.SelectedItem.ToString()))
                {
                    status++;
                    continue;
                }

                if (status == 0)
                {
                    foreach (var pair in playersListAway)
                    {
                        if (pair.Value == name)
                        {
                            try
                            {
                                pbsAwayList.Remove(pbsAwayList.Single(pbs => pair.Key == pbs.PlayerID));
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("A player being pasted wasn't previously in the box-score.");
                            }

                            pbsAwayList.Add(
                                new PlayerBoxScore(
                                    dict,
                                    pair.Key,
                                    Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString()),
                                    pair.Value));
                            break;
                        }
                    }
                }
                else if (status == 1)
                {
                    txtMINS1.Text = txtMINS1.Text.TrySetValue(dict, "MINS", typeof(UInt16));
                    txtPTS1.Text = txtPTS1.Text.TrySetValue(dict, "PTS", typeof(UInt16));
                    txtREB1.Text = txtREB1.Text.TrySetValue(dict, "REB", typeof(UInt16));
                    txtAST1.Text = txtAST1.Text.TrySetValue(dict, "AST", typeof(UInt16));
                    txtSTL1.Text = txtSTL1.Text.TrySetValue(dict, "STL", typeof(UInt16));
                    txtBLK1.Text = txtBLK1.Text.TrySetValue(dict, "BLK", typeof(UInt16));
                    txtTO1.Text = txtTO1.Text.TrySetValue(dict, "TO", typeof(UInt16));
                    txtFGM1.Text = txtFGM1.Text.TrySetValue(dict, "FGM", typeof(UInt16));
                    txtFGA1.Text = txtFGA1.Text.TrySetValue(dict, "FGA", typeof(UInt16));
                    txt3PM1.Text = txt3PM1.Text.TrySetValue(dict, "3PM", typeof(UInt16));
                    txt3PA1.Text = txt3PA1.Text.TrySetValue(dict, "3PA", typeof(UInt16));
                    txtFTM1.Text = txtFTM1.Text.TrySetValue(dict, "FTM", typeof(UInt16));
                    txtFTA1.Text = txtFTA1.Text.TrySetValue(dict, "FTA", typeof(UInt16));
                    txtOREB1.Text = txtOREB1.Text.TrySetValue(dict, "OREB", typeof(UInt16));
                    txtFOUL1.Text = txtFOUL1.Text.TrySetValue(dict, "FOUL", typeof(UInt16));
                    status++;
                }
                else if (status == 2)
                {
                    foreach (var pair in playersListHome)
                    {
                        if (pair.Value == name)
                        {
                            try
                            {
                                pbsHomeList.Remove(pbsHomeList.Single(pbs => pair.Key == pbs.PlayerID));
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("A player being pasted wasn't previously in the box-score.");
                            }

                            pbsHomeList.Add(
                                new PlayerBoxScore(
                                    dict,
                                    pair.Key,
                                    Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam2.SelectedItem.ToString()),
                                    pair.Value));
                            break;
                        }
                    }
                }
                else if (status == 3)
                {
                    txtMINS2.Text = txtMINS2.Text.TrySetValue(dict, "MINS", typeof(UInt16));
                    txtPTS2.Text = txtPTS2.Text.TrySetValue(dict, "PTS", typeof(UInt16));
                    txtREB2.Text = txtREB2.Text.TrySetValue(dict, "REB", typeof(UInt16));
                    txtAST2.Text = txtAST2.Text.TrySetValue(dict, "AST", typeof(UInt16));
                    txtSTL2.Text = txtSTL2.Text.TrySetValue(dict, "STL", typeof(UInt16));
                    txtBLK2.Text = txtBLK2.Text.TrySetValue(dict, "BLK", typeof(UInt16));
                    txtTO2.Text = txtTO2.Text.TrySetValue(dict, "TO", typeof(UInt16));
                    txtFGM2.Text = txtFGM2.Text.TrySetValue(dict, "FGM", typeof(UInt16));
                    txtFGA2.Text = txtFGA2.Text.TrySetValue(dict, "FGA", typeof(UInt16));
                    txt3PM2.Text = txt3PM2.Text.TrySetValue(dict, "3PM", typeof(UInt16));
                    txt3PA2.Text = txt3PA2.Text.TrySetValue(dict, "3PA", typeof(UInt16));
                    txtFTM2.Text = txtFTM2.Text.TrySetValue(dict, "FTM", typeof(UInt16));
                    txtFTA2.Text = txtFTA2.Text.TrySetValue(dict, "FTA", typeof(UInt16));
                    txtOREB2.Text = txtOREB2.Text.TrySetValue(dict, "OREB", typeof(UInt16));
                    txtFOUL2.Text = txtFOUL2.Text.TrySetValue(dict, "FOUL", typeof(UInt16));
                    break;
                }
            }
        }

        /// <summary>Handles the PreviewMouseLeftButtonDown event of the btnTools control. Opens the Tools context-menu on left click.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void btnTools_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cxmTools.PlacementTarget = this;
            cxmTools.IsOpen = true;
        }

        /// <summary>
        ///     Handles the Sorting event of any WPF DataGrid stat column. Uses a custom Sorting event handler to sort a stat in descending
        ///     order if it hasn't been sorted yet.
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
        ///     Handles the ShowToolTip event of the Any control. Uses a custom ShowToolTip event handler that shows the tooltip below the
        ///     control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void any_ShowToolTip(object sender, DependencyPropertyChangedEventArgs e)
        {
            GenericEventHandlers.Any_ShowToolTip(sender, e);
        }

        /// <summary>
        ///     Handles the Click event of the btnCompareTeamAndPlayerStats control. Compares each team's stats to the total of its players'
        ///     stats, to see if the stats match.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnCompareTeamAndPlayerStats_Click(object sender, RoutedEventArgs e)
        {
            int reb = 0, ast = 0, stl = 0, tos = 0, blk = 0, fgm = 0, fga = 0, tpm = 0, tpa = 0, ftm = 0, fta = 0, oreb = 0, foul = 0;

            foreach (var pbs in pbsAwayList)
            {
                reb += pbs.REB;
                ast += pbs.AST;
                stl += pbs.STL;
                tos += pbs.TOS;
                blk += pbs.BLK;
                fgm += pbs.FGM;
                fga += pbs.FGA;
                tpm += pbs.TPM;
                tpa += pbs.TPA;
                ftm += pbs.FTM;
                fta += pbs.FTA;
                oreb += pbs.OREB;
                foul += pbs.FOUL;
            }

            var team = cmbTeam1.SelectedItem.ToString();

            if (txtREB1.Text != reb.ToString())
            {
                comparisonError("REB", team);
                return;
            }
            if (txtAST1.Text != ast.ToString())
            {
                comparisonError("AST", team);
                return;
            }

            if (txtSTL1.Text != stl.ToString())
            {
                comparisonError("STL", team);
                return;
            }

            if (txtBLK1.Text != blk.ToString())
            {
                comparisonError("BLK", team);
                return;
            }

            if (txtTO1.Text != tos.ToString())
            {
                comparisonError("TO", team);
                return;
            }

            if (txtFGM1.Text != fgm.ToString())
            {
                comparisonError("FGM", team);
                return;
            }

            if (txtFGA1.Text != fga.ToString())
            {
                comparisonError("FGA", team);
                return;
            }

            if (txt3PM1.Text != tpm.ToString())
            {
                comparisonError("3PM", team);
                return;
            }

            if (txt3PA1.Text != tpa.ToString())
            {
                comparisonError("3PA", team);
                return;
            }

            if (txtFTM1.Text != ftm.ToString())
            {
                comparisonError("FTM", team);
                return;
            }

            if (txtFTA1.Text != fta.ToString())
            {
                comparisonError("FTA", team);
                return;
            }

            if (txtOREB1.Text != oreb.ToString())
            {
                comparisonError("OREB", team);
                return;
            }

            if (txtFOUL1.Text != foul.ToString())
            {
                comparisonError("FOUL", team);
                return;
            }

            reb = 0;
            ast = 0;
            stl = 0;
            tos = 0;
            blk = 0;
            fgm = 0;
            fga = 0;
            tpm = 0;
            tpa = 0;
            ftm = 0;
            fta = 0;
            oreb = 0;
            foul = 0;

            foreach (var pbs in pbsHomeList)
            {
                reb += pbs.REB;
                ast += pbs.AST;
                stl += pbs.STL;
                tos += pbs.TOS;
                blk += pbs.BLK;
                fgm += pbs.FGM;
                fga += pbs.FGA;
                tpm += pbs.TPM;
                tpa += pbs.TPA;
                ftm += pbs.FTM;
                fta += pbs.FTA;
                oreb += pbs.OREB;
                foul += pbs.FOUL;
            }

            team = cmbTeam2.SelectedItem.ToString();

            if (txtREB2.Text != reb.ToString())
            {
                comparisonError("REB", team);
                return;
            }
            if (txtAST2.Text != ast.ToString())
            {
                comparisonError("AST", team);
                return;
            }

            if (txtSTL2.Text != stl.ToString())
            {
                comparisonError("STL", team);
                return;
            }

            if (txtBLK2.Text != blk.ToString())
            {
                comparisonError("BLK", team);
                return;
            }

            if (txtTO2.Text != tos.ToString())
            {
                comparisonError("TO", team);
                return;
            }

            if (txtFGM2.Text != fgm.ToString())
            {
                comparisonError("FGM", team);
                return;
            }

            if (txtFGA2.Text != fga.ToString())
            {
                comparisonError("FGA", team);
                return;
            }

            if (txt3PM2.Text != tpm.ToString())
            {
                comparisonError("3PM", team);
                return;
            }

            if (txt3PA2.Text != tpa.ToString())
            {
                comparisonError("3PA", team);
                return;
            }

            if (txtFTM2.Text != ftm.ToString())
            {
                comparisonError("FTM", team);
                return;
            }

            if (txtFTA2.Text != fta.ToString())
            {
                comparisonError("FTA", team);
                return;
            }

            if (txtOREB2.Text != oreb.ToString())
            {
                comparisonError("OREB", team);
                return;
            }

            if (txtFOUL2.Text != foul.ToString())
            {
                comparisonError("FOUL", team);
                return;
            }

            MessageBox.Show("All team and player stats add up!");
        }

        /// <summary>Shows a message explaining which comparison between player and team stats failed.</summary>
        /// <param name="stat">The stat.</param>
        /// <param name="team">The team.</param>
        private void comparisonError(string stat, string team)
        {
            MessageBox.Show(
                "The accumulated " + stat + " stat for the " + team + "'s players doesn't match the stat entered for the team.");
        }

        private void btnCopyBestToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(
                String.Format(
                    "Player of the Game\n\n{0}\n\n\n{1} Best Performers\n\n{2}\n\n{3}\n\n{4}\n\n\n"
                    + "{5} Best Performers\n\n{6}\n\n{7}\n\n{8}",
                    txbMVP.Text + "\n\n" + txbMVPStats.Text,
                    tabTeam1.Header,
                    txbAway1.Text,
                    txbAway2.Text,
                    txbAway3.Text,
                    tabTeam2.Header,
                    txbHome1.Text,
                    txbHome2.Text,
                    txbHome3.Text));
        }

        /// <summary>Updates the box score data grid.</summary>
        /// <param name="teamID">Name of the team.</param>
        /// <param name="playersList">The players list.</param>
        /// <param name="pbsList">The player box score list.</param>
        /// <param name="loading">
        ///     if set to <c>true</c>, it is assumed that a pre-existing box score is being loaded.
        /// </param>
        private static void updateBoxScoreDataGrid(
            int teamID,
            out ObservableCollection<KeyValuePair<int, string>> playersList,
            ref SortableBindingList<PlayerBoxScore> pbsList,
            bool loading)
        {
            var list = MainWindow.PST.Where(ps => ps.Value.TeamF == teamID).ToDictionary(ps => ps.Key, ps => ps.Value);

            playersList = new ObservableCollection<KeyValuePair<int, string>>();
            if (!loading)
            {
                pbsList = new SortableBindingList<PlayerBoxScore>();
            }

            foreach (var ps in list.Values)
            {
                playersList.Add(new KeyValuePair<int, string>(ps.ID, ps.FullName));
                if (!loading)
                {
                    var pbs = new PlayerBoxScore { PlayerID = ps.ID, TeamID = teamID };
                    pbsList.Add(pbs);
                }
            }

            for (var i = 0; i < pbsList.Count; i++)
            {
                var cur = pbsList[i];
                var name = MainWindow.PST[cur.PlayerID].FullName;
                var player = new KeyValuePair<int, string>(cur.PlayerID, name);
                cur.Name = name;
                if (!playersList.Contains(player))
                {
                    playersList.Add(player);
                }
                pbsList[i] = cur;
            }
            playersList = new ObservableCollection<KeyValuePair<int, string>>(playersList.OrderBy(item => item.Value));
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            if (!_clickedOK)
            {
                MainWindow.TempBSE_BS.Done = false;
            }
        }

        private void btnEditPlayByPlay_Click(object sender, RoutedEventArgs e)
        {
            tryParseBS(true);
            if (!MainWindow.TempBSE_BS.Done)
            {
                return;
            }

            var pbpw = new PlayByPlayWindow(
                MainWindow.TST,
                MainWindow.PST,
                new BoxScoreEntry(
                    MainWindow.TempBSE_BS, MainWindow.TempBSE_PBSLists.SelectMany(list => list).ToList(), MainWindow.TempBSE_PBPEList),
                MainWindow.TempBSE_BS.Team1ID,
                MainWindow.TempBSE_BS.Team2ID);
            if (pbpw.ShowDialog() != true)
            {
                return;
            }

            pbpeList = new ObservableCollection<PlayByPlayEntry>(PlayByPlayWindow.SavedPlays);
            pbpeList.Sort(new PlayByPlayEntryComparerAsc());
            lstPlayByPlay.ItemsSource = pbpeList;

            MainWindow.TempBSE_PBPEList = new List<PlayByPlayEntry>(PlayByPlayWindow.SavedPlays);
        }

        private void anyPBSDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var pbs = (e.Row.Item as PlayerBoxScore);
                if (pbs == null)
                {
                    return;
                }
                var id = pbs.PlayerID;
                Dispatcher.BeginInvoke(new Action(() => pbs.Name = MainWindow.PST[id].FullName), DispatcherPriority.Background);
            }
        }

        private void anyPBSDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!_isManualEditCommit && e.EditAction == DataGridEditAction.Commit)
            {
                _isManualEditCommit = true;
                var grid = (DataGrid) sender;
                if (grid != null)
                {
                    grid.CommitEdit(DataGridEditingUnit.Row, true);
                }
                _isManualEditCommit = false;
            }
        }

        private void btnCalculateFromPBP_Click(object sender, RoutedEventArgs e)
        {
            foreach (var pbs in pbsAwayList)
            {
                pbs.CalculateFromPBPEList(pbpeList);
            }

            foreach (var pbs in pbsHomeList)
            {
                pbs.CalculateFromPBPEList(pbpeList);
            }
        }
    }
}