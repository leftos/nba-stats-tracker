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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LeftosCommonLibrary;
using LeftosCommonLibrary.BeTimvwFramework;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Misc;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.ListExtensions;
using NBA_Stats_Tracker.Helper.Misc;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Used to display and edit team and player box scores, as well as player metric stats and the best performers for that game.
    /// </summary>
    public partial class BoxScoreWindow
    {
        #region Mode enum

        /// <summary>
        /// Used to determine the function for which the window has been opened. 
        /// Update is for entering a new box score.
        /// View is for viewing and editing pre-existing box score.
        /// ViewAndIgnore is for viewing a pre-existing box score in read-only mode.
        /// </summary>
        public enum Mode
        {
            Update,
            View,
            ViewAndIgnore
        };

        #endregion

        private static Mode _curMode = Mode.Update;

        private static TeamBoxScore _curTeamBoxScore;
        private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        private readonly bool onImport;
        private List<string> Teams;
        private int curSeason;
        private Brush defaultBackground;
        private bool loading;
        private bool minsUpdating;
        private string playersT;
        private List<PlayerStatsRow> pmsrListAway, pmsrListHome;
        private Dictionary<int, PlayerStats> pst;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="curMode">The Mode enum instance which determines the function for which the window is opened.</param>
        public BoxScoreWindow(Mode curMode = Mode.Update)
        {
            InitializeComponent();

            if (MainWindow.tf.isBetween)
            {
                MainWindow.tf = new Timeframe(MainWindow.tf.SeasonNum);
                MainWindow.UpdateAllData();
            }

            /*
            tst = MainWindow.tst;
            pst = MainWindow.pst;
            tstopp = MainWindow.tstopp;
            */

            cbHistory.Visibility = Visibility.Hidden;

            _curMode = curMode;
            prepareWindow(curMode);

            MainWindow.bs = new TeamBoxScore();

            if (curMode == Mode.Update)
            {
                _curTeamBoxScore = new TeamBoxScore();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="curMode">The Mode enum instance which determines the function for which the window is opened.</param>
        /// <param name="id">The ID of the box score to be viewed.</param>
        public BoxScoreWindow(Mode curMode, int id) : this(curMode)
        {
            pst = MainWindow.pst;

            LoadBoxScore(id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="bse">The Box Score Entry from which to load the box score to be viewed.</param>
        /// <param name="onImport">if set to <c>true</c>, a box score is being imported into the database, and the window is prepared accordingly.</param>
        public BoxScoreWindow(BoxScoreEntry bse, bool onImport = false) : this()
        {
            LoadBoxScore(bse);
            this.onImport = onImport;

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
        /// Initializes a new instance of the <see cref="BoxScoreWindow" /> class.
        /// </summary>
        /// <param name="bse">The Box Score Entry from which to load the box score to be viewed.</param>
        /// <param name="pst">The player stats dictionary to use for this instance.</param>
        /// <param name="onImport">if set to <c>true</c>, a box score is being imported into the database, and the window is prepared accordingly.</param>
        public BoxScoreWindow(BoxScoreEntry bse, Dictionary<int, PlayerStats> pst, bool onImport) : this()
        {
            this.pst = pst;

            LoadBoxScore(bse);
            this.onImport = onImport;

            if (onImport)
            {
                MainWindow.bs = bse.bs;
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
        private ObservableCollection<KeyValuePair<int, string>> PlayersListAway { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> PlayersListHome { get; set; }

        /// <summary>
        /// Finds the requested box score and loads it.
        /// </summary>
        /// <param name="id">The ID of the box score.</param>
        private void LoadBoxScore(int id)
        {
            int bshistid = -1;

            for (int i = 0; i < MainWindow.bshist.Count; i++)
            {
                if (MainWindow.bshist[i].bs.id == id)
                {
                    bshistid = i;
                    break;
                }
            }

            BoxScoreEntry bse = MainWindow.bshist[bshistid];
            _curTeamBoxScore = MainWindow.bshist[bshistid].bs;
            _curTeamBoxScore.bshistid = bshistid;
            LoadBoxScore(bse);
        }

        /// <summary>
        /// Loads the given box score.
        /// </summary>
        /// <param name="bse">The BoxScoreEntry to load.</param>
        private void LoadBoxScore(BoxScoreEntry bse)
        {
            TeamBoxScore bs = bse.bs;
            MainWindow.bs = bse.bs;
            txtPTS1.Text = bs.PTS1.ToString();
            txtREB1.Text = bs.REB1.ToString();
            txtAST1.Text = bs.AST1.ToString();
            txtSTL1.Text = bs.STL1.ToString();
            txtBLK1.Text = bs.BLK1.ToString();
            txtTO1.Text = bs.TO1.ToString();
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
            txtTO2.Text = bs.TO2.ToString();
            txtFGM2.Text = bs.FGM2.ToString();
            txtFGA2.Text = bs.FGA2.ToString();
            txt3PM2.Text = bs.TPM2.ToString();
            txt3PA2.Text = bs.TPA2.ToString();
            txtFTM2.Text = bs.FTM2.ToString();
            txtFTA2.Text = bs.FTA2.ToString();
            txtOREB2.Text = bs.OREB2.ToString();
            txtFOUL2.Text = bs.FOUL2.ToString();
            txtMINS2.Text = bs.MINS2.ToString();

            dtpGameDate.SelectedDate = bs.gamedate;
            curSeason = bs.SeasonNum;
            //LinkInternalsToMainWindow();
            chkIsPlayoff.IsChecked = bs.isPlayoff;

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
            loading = true;
            foreach (PlayerBoxScore pbs in bse.pbsList)
            {
                if (pbs.Team == bs.Team1)
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
                cmbTeam1.SelectedItem = Misc.GetDisplayNameFromTeam(MainWindow.tst, bs.Team1);
                cmbTeam2.SelectedItem = Misc.GetDisplayNameFromTeam(MainWindow.tst, bs.Team2);
            }
            catch
            {
                MessageBox.Show("One of the teams requested is disabled for this season. This box score is not available.\n" +
                                "To be able to see this box score, enable the teams included in it.");
                Close();
            }
            PopulateSeasonCombo();


            loading = false;
        }

        /*
        private void LinkInternalsToMainWindow()
        {
            tst = MainWindow.tst;
            tstopp = MainWindow.tstopp;
            pst = MainWindow.pst;
            TeamOrder = MainWindow.TeamOrder;
        }
         */

        protected SortedDictionary<string, int> TeamOrder;

        /// <summary>
        /// Updates the player box score data grid for the specified team.
        /// </summary>
        /// <param name="team">1 for the away team, anything else for the home team.</param>
        private void UpdateDataGrid(int team)
        {
            SortableBindingList<PlayerBoxScore> pbsList;
            string TeamName;
            if (team == 1)
            {
                try
                {
                    TeamName = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam1.SelectedItem.ToString());
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
                    TeamName = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam2.SelectedItem.ToString());
                    pbsList = pbsHomeList;
                }
                catch (Exception)
                {
                    return;
                }
            }

            ObservableCollection<KeyValuePair<int, string>> PlayersList;
            EventHandlers.UpdateBoxScoreDataGrid(TeamName, out PlayersList, ref pbsList, playersT, loading);

            if (team == 1)
            {
                colPlayerAway.ItemsSource = PlayersList;
                PlayersListAway = PlayersList;
                pbsAwayList = pbsList;
                dgvPlayersAway.ItemsSource = pbsAwayList;
                dgvPlayersAway.CanUserAddRows = false;
            }
            else
            {
                colPlayerHome.ItemsSource = PlayersList;
                PlayersListHome = PlayersList;
                pbsHomeList = pbsList;
                dgvPlayersHome.ItemsSource = pbsHomeList;
                dgvPlayersHome.CanUserAddRows = false;
            }
        }

        /// <summary>
        /// Prepares the window based on the mode of function it was opened for.
        /// </summary>
        /// <param name="curMode">The Mode enum instance which determines the function for which the window is opened.</param>
        private void prepareWindow(Mode curMode)
        {
            curSeason = MainWindow.curSeason;

            PopulateSeasonCombo();

            PopulateTeamsCombo();

            defaultBackground = cmbTeam1.Background;

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

            MainWindow.bs.done = false;

            dtpGameDate.SelectedDate = DateTime.Today;

            _curMode = curMode;

            calculateScoreAway();
            calculateScoreHome();

            if (_curMode == Mode.View || _curMode == Mode.ViewAndIgnore)
            {
                label1.Content = "";
                chkDoNotUpdate.Visibility = Visibility.Hidden;
                txbDoNotUpdate.Visibility = Visibility.Hidden;
                Title = "View & Edit Box Score";
            }

            if (_curMode == Mode.ViewAndIgnore)
            {
                label1.Content = "Any changes made to the box score will be ignored.";
                label1.FontWeight = FontWeights.Bold;
                btnOK.Visibility = Visibility.Hidden;
                btnCalculateTeams.Visibility = Visibility.Hidden;
                btnCancel.Content = "Close";
                Title = "View Box Score";
            }
        }

        /// <summary>
        /// Populates the teams combo-box.
        /// </summary>
        private void PopulateTeamsCombo()
        {
            Teams = new List<string>();
            foreach (var kvp in MainWindow.TeamOrder)
            {
                if (!MainWindow.tst[kvp.Value].isHidden)
                    Teams.Add(MainWindow.tst[kvp.Value].displayName);
            }

            Teams.Sort();

            cmbTeam1.ItemsSource = Teams;
            cmbTeam2.ItemsSource = Teams;
        }

        /// <summary>
        /// Populates the season combo-box.
        /// </summary>
        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.Items.Clear();

            for (int i = maxSeason; i > 0; i--)
            {
                bool addIt = true;
                if (cmbTeam1.SelectedItem != null)
                {
                    if (TeamStats.IsTeamHiddenInSeason(MainWindow.currentDB,
                                                       Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam1.SelectedItem.ToString()), i))
                        addIt = false;
                }
                if (cmbTeam2.SelectedItem != null)
                {
                    if (TeamStats.IsTeamHiddenInSeason(MainWindow.currentDB,
                                                       Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam2.SelectedItem.ToString()), i))
                        addIt = false;
                }
                if (addIt)
                    cmbSeasonNum.Items.Add(i.ToString());
            }

            cmbSeasonNum.SelectedIndex = -1;
            cmbSeasonNum.SelectedItem = curSeason.ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_curMode == Mode.Update)
            {
                tryParseBS();
                if (MainWindow.bs.done == false)
                    return;
            }
            else
            {
                if (_curMode == Mode.View)
                {
                    if (_curTeamBoxScore.bshistid != -1)
                    {
                        MessageBoxResult r = MessageBox.Show("Do you want to save any changes to this Box Score?", "NBA Stats Tracker",
                                                             MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                        if (r == MessageBoxResult.Cancel)
                            return;

                        if (r == MessageBoxResult.Yes)
                        {
                            tryParseBS();
                            if (MainWindow.bs.done == false)
                                return;

                            MainWindow.UpdateBoxScore();
                            MessageBox.Show("It is recommended to save the database for changes to take effect.");
                        }
                        else
                        {
                            MainWindow.bs.done = false;
                        }
                    }
                    else
                    {
                        MainWindow.bs.done = false;
                    }
                }
            }
            Close();
        }

        /// <summary>
        /// Tries to the parse the current team & player box scores, and check for any errors.
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        private void tryParseBS()
        {
            if (cmbTeam1.SelectedItem.ToString() == cmbTeam2.SelectedItem.ToString())
            {
                MessageBox.Show("You can't have the same team in both Home & Away.");
                return;
            }
            if ((txtPTS1.Text == "") || (txtPTS1.Text == "N/A") || (txtPTS2.Text == "") || (txtPTS2.Text == "N/A"))
            {
                //MessageBox.Show("The Box Score is incomplete. Make sure you input all stats.");
                return;
            }
            if (cmbSeasonNum.SelectedIndex == -1)
            {
                MessageBox.Show("You have to choose a season.");
                return;
            }
            try
            {
                try
                {
                    MainWindow.bs.id = _curTeamBoxScore.id;
                    MainWindow.bs.bshistid = _curTeamBoxScore.bshistid;
                }
                catch
                {
                    MainWindow.bs.id = -1;
                    MainWindow.bs.bshistid = -1;
                }
                MainWindow.bs.isPlayoff = chkIsPlayoff.IsChecked.GetValueOrDefault();
                MainWindow.bs.gamedate = dtpGameDate.SelectedDate.GetValueOrDefault();
                MainWindow.bs.SeasonNum = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
                MainWindow.bs.Team1 = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam1.SelectedItem.ToString());
                MainWindow.bs.Team2 = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam2.SelectedItem.ToString());
                MainWindow.bs.MINS2 = MainWindow.bs.MINS1 = Convert.ToUInt16(txtMINS1.Text);

                if (MainWindow.bs.MINS1 <= 0)
                {
                    throwErrorWithMessage(
                        "You have to enter the game's minutes. Usually 48 for 4 quarters, 53 for 1 overtime, 58 for 2 overtimes.");
                }

                MainWindow.bs.PTS1 = Convert.ToUInt16(txtPTS1.Text);
                MainWindow.bs.REB1 = Convert.ToUInt16(txtREB1.Text);
                MainWindow.bs.AST1 = Convert.ToUInt16(txtAST1.Text);
                MainWindow.bs.STL1 = Convert.ToUInt16(txtSTL1.Text);
                MainWindow.bs.BLK1 = Convert.ToUInt16(txtBLK1.Text);
                MainWindow.bs.TO1 = Convert.ToUInt16(txtTO1.Text);
                MainWindow.bs.FGM1 = Convert.ToUInt16(txtFGM1.Text);
                MainWindow.bs.FGA1 = Convert.ToUInt16(txtFGA1.Text);
                MainWindow.bs.TPM1 = Convert.ToUInt16(txt3PM1.Text);
                MainWindow.bs.TPA1 = Convert.ToUInt16(txt3PA1.Text);

                if (MainWindow.bs.FGA1 < MainWindow.bs.FGM1)
                {
                    throwErrorWithMessage("The FGM stat can't be higher than the FGA stat.");
                }
                if (MainWindow.bs.TPA1 < MainWindow.bs.TPM1)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the 3PA stat.");
                }
                if (MainWindow.bs.FGM1 < MainWindow.bs.TPM1)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the FGM stat.");
                }

                MainWindow.bs.FTM1 = Convert.ToUInt16(txtFTM1.Text);
                MainWindow.bs.FTA1 = Convert.ToUInt16(txtFTA1.Text);
                if (MainWindow.bs.FTA1 < MainWindow.bs.FTM1)
                {
                    throwErrorWithMessage("The FTM stat can't be higher than the FTA stat.");
                }

                MainWindow.bs.OREB1 = Convert.ToUInt16(txtOREB1.Text);
                if (MainWindow.bs.OREB1 > MainWindow.bs.REB1)
                {
                    throwErrorWithMessage("The OFF stat can't be higher than the REB stat.");
                }
                if (MainWindow.bs.FGA1 < MainWindow.bs.TPA1)
                {
                    throwErrorWithMessage("The 3PA stat can't be higher than the FGA stat.");
                }

                MainWindow.bs.FOUL1 = Convert.ToUInt16(txtFOUL1.Text);
                MainWindow.bs.PTS2 = Convert.ToUInt16(txtPTS2.Text);
                MainWindow.bs.REB2 = Convert.ToUInt16(txtREB2.Text);
                MainWindow.bs.AST2 = Convert.ToUInt16(txtAST2.Text);
                MainWindow.bs.STL2 = Convert.ToUInt16(txtSTL2.Text);
                MainWindow.bs.BLK2 = Convert.ToUInt16(txtBLK2.Text);
                MainWindow.bs.TO2 = Convert.ToUInt16(txtTO2.Text);
                MainWindow.bs.FGM2 = Convert.ToUInt16(txtFGM2.Text);
                MainWindow.bs.FGA2 = Convert.ToUInt16(txtFGA2.Text);
                MainWindow.bs.TPM2 = Convert.ToUInt16(txt3PM2.Text);
                MainWindow.bs.TPA2 = Convert.ToUInt16(txt3PA2.Text);

                if (MainWindow.bs.FGA2 < MainWindow.bs.FGM2)
                {
                    throwErrorWithMessage("The FGM stat can't be higher than the FGA stat.");
                }
                if (MainWindow.bs.TPA2 < MainWindow.bs.TPM2)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the 3PA stat.");
                }
                if (MainWindow.bs.FGM2 < MainWindow.bs.TPM2)
                {
                    throwErrorWithMessage("The 3PM stat can't be higher than the FGM stat.");
                }
                if (MainWindow.bs.FGA2 < MainWindow.bs.TPA2)
                {
                    throwErrorWithMessage("The 3PA stat can't be higher than the FGA stat.");
                }

                MainWindow.bs.FTM2 = Convert.ToUInt16(txtFTM2.Text);
                MainWindow.bs.FTA2 = Convert.ToUInt16(txtFTA2.Text);
                if (MainWindow.bs.FTA2 < MainWindow.bs.FTM2)
                {
                    throwErrorWithMessage("The FTM stat can't be higher than the FTA stat.");
                }

                MainWindow.bs.OREB2 = Convert.ToUInt16(txtOREB2.Text);

                if (MainWindow.bs.OREB2 > MainWindow.bs.REB2)
                {
                    throwErrorWithMessage("The OFF stat can't be higher than the REB stat.");
                }

                MainWindow.bs.FOUL2 = Convert.ToUInt16(txtFOUL2.Text);

                #region Additional Box Score Checks

                if (MainWindow.bs.AST1 > MainWindow.bs.FGM1 || MainWindow.bs.AST2 > MainWindow.bs.FGM2)
                {
                    throwErrorWithMessage("The AST stat can't be higher than the FGM stat.");
                }

                if (MainWindow.bs.BLK1 > MainWindow.bs.FGA2 - MainWindow.bs.FGM2 ||
                    MainWindow.bs.BLK2 > MainWindow.bs.FGA1 - MainWindow.bs.FGM1)
                {
                    throwErrorWithMessage("The BLK stat for one team can't be higher than the other team's missed FGA (i.e. FGA - FGM).");
                }

                if (MainWindow.bs.REB1 - MainWindow.bs.OREB1 > MainWindow.bs.FGA2 - MainWindow.bs.FGM2 ||
                    MainWindow.bs.REB2 - MainWindow.bs.OREB2 > MainWindow.bs.FGA1 - MainWindow.bs.FGM1)
                {
                    throwErrorWithMessage(
                        "The DREB (i.e. REB - OREB) stat for one team can't be higher than the other team's missed FGA (i.e. FGA - FGM).");
                }

                if (MainWindow.bs.OREB1 > MainWindow.bs.FGA1 - MainWindow.bs.FGM1 ||
                    MainWindow.bs.OREB2 > MainWindow.bs.FGA2 - MainWindow.bs.FGM2)
                {
                    throwErrorWithMessage("The OREB stat cant' be higher than the missed FGA (i.e. FGA - FGM).");
                }

                if (MainWindow.bs.STL1 > MainWindow.bs.TO2 || MainWindow.bs.STL2 > MainWindow.bs.TO1)
                {
                    throwErrorWithMessage("The STL stat for one team can't be higher than the other team's TO.");
                }

                // TODO: Need to handle the possibility of technical fouls somehow.
                /*
                if (MainWindow.bs.FGA1 > MainWindow.bs.FOUL2 * 3 || MainWindow.bs.FGA2 > MainWindow.bs.FOUL1 * 3)
                {
                    throwErrorWithMessage("The FTA stat for one team can't be more than 3 times the other team's FOUL stat.");
                }
                */

                #endregion

                MainWindow.bs.doNotUpdate = chkDoNotUpdate.IsChecked.GetValueOrDefault();

                #region Player Box Scores Check

                string Team1 = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam1.SelectedItem.ToString());
                string Team2 = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam2.SelectedItem.ToString());

                foreach (PlayerBoxScore pbs in pbsAwayList)
                    pbs.Team = Team1;

                foreach (PlayerBoxScore pbs in pbsHomeList)
                    pbs.Team = Team2;

                int starters = 0;
                var pbsLists = new List<SortableBindingList<PlayerBoxScore>>(2) {pbsAwayList, pbsHomeList};
                Dictionary<int, string> allPlayers = PlayersListAway.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                foreach (var kvp in PlayersListHome)
                    allPlayers.Add(kvp.Key, kvp.Value);
                foreach (var pbsList in pbsLists)
                {
                    starters = 0;
                    foreach (PlayerBoxScore pbs in pbsList)
                    {
                        //pbs.PlayerID = 
                        if (pbs.PlayerID == -1)
                            continue;

                        if (pbs.isOut)
                        {
                            pbs.ResetStats();
                            continue;
                        }

                        if (pbs.isStarter)
                        {
                            starters++;
                            if (starters > 5)
                            {
                                string s = "There can't be more than 5 starters in each team.";
                                s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                                MessageBox.Show(s);
                                throw (new Exception());
                            }
                        }

                        if (pbs.FGM > pbs.FGA)
                        {
                            string s = "The FGM stat can't be higher than the FGA stat.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.TPM > pbs.TPA)
                        {
                            string s = "The 3PM stat can't be higher than the 3PA stat.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.FGM < pbs.TPM)
                        {
                            string s = "The 3PM stat can't be higher than the FGM stat.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.FGA < pbs.TPA)
                        {
                            string s = "The TPA stat can't be higher than the FGA stat.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.FTM > pbs.FTA)
                        {
                            string s = "The FTM stat can't be higher than the FTA stat.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.OREB > pbs.REB)
                        {
                            string s = "The OREB stat can't be higher than the REB stat.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        if (pbs.isStarter && pbs.MINS == 0)
                        {
                            string s = "A player can't be a starter but not have any minutes played.";
                            s += "\n\nTeam: " + pbs.Team + "\nPlayer: " + allPlayers[pbs.PlayerID];
                            MessageBox.Show(s);
                            throw (new Exception());
                        }

                        pbs.DREB = (UInt16) (pbs.REB - pbs.OREB);
                    }
                }
                MainWindow.pbsLists = pbsLists;

                #endregion

                MainWindow.bs.done = true;
            }
            catch
            {
                MessageBox.Show("The Box Score seems to be invalid. Check that there's no stats missing.");
                MainWindow.bs.done = false;
            }
        }

        /// <summary>
        /// Throws an exception after showing a friendly error message to the user.
        /// </summary>
        /// <param name="msg">The message displayed to the user and used for the exception.</param>
        /// <exception cref="System.Exception"></exception>
        private void throwErrorWithMessage(string msg)
        {
            MessageBox.Show(msg, "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
            throw (new Exception(msg));
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbTeam1 control. Updates the tab headers and corresponding data grid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam1.Header = cmbTeam1.SelectedItem;
            tabAwayMetric.Header = cmbTeam1.SelectedItem + " Metric Stats";
            grpAway.Header = cmbTeam1.SelectedItem;
            UpdateDataGrid(1);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbTeam2 control. Updates the tab headers and corresponding data grid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam2.Header = cmbTeam2.SelectedItem;
            tabHomeMetric.Header = cmbTeam2.SelectedItem + " Metric Stats";
            grpHome.Header = cmbTeam2.SelectedItem;
            UpdateDataGrid(2);
        }

        /// <summary>
        /// Checks if the same team is selected as home and away, and changes the combo-box color to reflect that.
        /// </summary>
        private void checkIfSameTeams()
        {
            string Team1, Team2;
            try
            {
                Team1 = cmbTeam1.SelectedItem.ToString();
                Team2 = cmbTeam2.SelectedItem.ToString();
            }
            catch (Exception)
            {
                return;
            }


            if (Team1 == Team2)
            {
                cmbTeam1.Background = Brushes.Red;
                cmbTeam2.Background = Brushes.Red;
                return;
            }

            cmbTeam1.Background = defaultBackground;
            cmbTeam2.Background = defaultBackground;
        }

        /// <summary>
        /// Tries to calculate the score and averages for the Away team.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void calculateScoreAway(object sender = null, TextChangedEventArgs e = null)
        {
            int PTS1;
            string T1Avg;
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
                int fga = Convert.ToInt32(txtFGA1.Text);
                int tpa = Convert.ToInt32(txt3PA1.Text);
                int fta = Convert.ToInt32(txtFTA1.Text);

                EventHandlers.calculateScore(fgm, fga, tpm, tpa, ftm, fta, out PTS1, out T1Avg);
            }
            catch
            {
                EventHandlers.calculateScore(fgm, null, tpm, null, ftm, null, out PTS1, out T1Avg);
            }
            txtPTS1.Text = PTS1.ToString();
            txbT1Avg.Text = T1Avg;
        }

        /// <summary>
        /// Tries to calculate the score and averages for the Home team.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void calculateScoreHome(object sender = null, TextChangedEventArgs e = null)
        {
            int PTS2;
            string T2Avg;
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
                int fga = Convert.ToInt32(txtFGA2.Text);
                int tpa = Convert.ToInt32(txt3PA2.Text);
                int fta = Convert.ToInt32(txtFTA2.Text);

                EventHandlers.calculateScore(fgm, fga, tpm, tpa, ftm, fta, out PTS2, out T2Avg);
            }
            catch
            {
                EventHandlers.calculateScore(fgm, null, tpm, null, ftm, null, out PTS2, out T2Avg);
            }
            txtPTS2.Text = PTS2.ToString();
            txbT2Avg.Text = T2Avg;
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control. Copies the current team and player box scores as tab-separated values to the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            tryParseBS();
            if (MainWindow.bs.done)
            {
                string data1 =
                    String.Format(
                        "{0}\t\t\t\t{19}\t{1}\t{2}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:F3}\t{12}\t{13}\t{14:F3}\t{15}\t{16}\t{17:F3}\t{3}\t{18}",
                        cmbTeam1.SelectedItem, MainWindow.bs.PTS1, MainWindow.bs.REB1, MainWindow.bs.OREB1,
                        MainWindow.bs.REB1 - MainWindow.bs.OREB1, MainWindow.bs.AST1, MainWindow.bs.STL1, MainWindow.bs.BLK1,
                        MainWindow.bs.TO1, MainWindow.bs.FGM1, MainWindow.bs.FGA1, MainWindow.bs.FGM1/(float) MainWindow.bs.FGA1,
                        MainWindow.bs.TPM1, MainWindow.bs.TPA1, MainWindow.bs.TPM1/(float) MainWindow.bs.TPA1, MainWindow.bs.FTM1,
                        MainWindow.bs.FTA1, MainWindow.bs.FTM1/(float) MainWindow.bs.FTA1, MainWindow.bs.FOUL1, MainWindow.bs.MINS1);

                string data2 =
                    String.Format(
                        "{0}\t\t\t\t{19}\t{1}\t{2}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:F3}\t{12}\t{13}\t{14:F3}\t{15}\t{16}\t{17:F3}\t{3}\t{18}",
                        cmbTeam2.SelectedItem, MainWindow.bs.PTS2, MainWindow.bs.REB2, MainWindow.bs.OREB2,
                        MainWindow.bs.REB2 - MainWindow.bs.OREB2, MainWindow.bs.AST2, MainWindow.bs.STL2, MainWindow.bs.BLK2,
                        MainWindow.bs.TO2, MainWindow.bs.FGM2, MainWindow.bs.FGA2, MainWindow.bs.FGM2/(float) MainWindow.bs.FGA2,
                        MainWindow.bs.TPM2, MainWindow.bs.TPA2, MainWindow.bs.TPM2/(float) MainWindow.bs.TPA2, MainWindow.bs.FTM2,
                        MainWindow.bs.FTA2, MainWindow.bs.FTM2/(float) MainWindow.bs.FTA2, MainWindow.bs.FOUL2, MainWindow.bs.MINS2);

                dgvPlayersAway.SelectAllCells();
                ApplicationCommands.Copy.Execute(null, dgvPlayersAway);
                dgvPlayersAway.UnselectAllCells();
                var result1 = (string) Clipboard.GetData(DataFormats.Text);
                dgvPlayersHome.SelectAllCells();
                ApplicationCommands.Copy.Execute(null, dgvPlayersHome);
                dgvPlayersHome.UnselectAllCells();
                var result2 = (string) Clipboard.GetData(DataFormats.Text);

                string result = result1 + data1 + "\n\n\n" + result2 + data2;
                Clipboard.SetText(result);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.bs.done = false;
            Close();
        }

        /// <summary>
        /// Handles the GotFocus event of the _bsAnyTextbox control. Any time a textbox with this event handler gets focus, 
        /// its text gets selected so that it can be easily replaced.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void _bsAnyTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.SelectAll();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbSeasonNum control. 
        /// Loads the new season's team and player stats dictionaries and other information, and repopulates the combo-boxes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1)
                return;

            curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
            //MainWindow.ChangeSeason(curSeason, MainWindow.getMaxSeason(MainWindow.currentDB));

            if (!onImport)
            {
                SQLiteIO.LoadSeason(MainWindow.currentDB, curSeason, doNotLoadBoxScores: true);

                playersT = "Players";

                if (curSeason != maxSeason)
                {
                    playersT += "S" + curSeason;
                }
            }
            PopulateTeamsCombo();
        }

        /// <summary>
        /// Handles the Click event of the btnCalculateTeams control.
        /// Calculates the stats for each team from the totals of the corresponding player stats.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnCalculateTeams_Click(object sender, RoutedEventArgs e)
        {
            TeamBoxScore bs = new TeamBoxScore();
            CalculateTeamsFromPlayers(ref bs, pbsAwayList, pbsHomeList);

            txtREB1.Text = bs.REB1.ToString();
            txtAST1.Text = bs.AST1.ToString();
            txtSTL1.Text = bs.STL1.ToString();
            txtBLK1.Text = bs.BLK1.ToString();
            txtTO1.Text = bs.TO1.ToString();
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
            txtTO2.Text = bs.TO2.ToString();
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

        public static void CalculateTeamsFromPlayers(ref TeamBoxScore bs, IEnumerable<PlayerBoxScore> awayPBS,
                                                     IEnumerable<PlayerBoxScore> homePBS)
        {
            ushort REB = 0, AST = 0, STL = 0, TOS = 0, BLK = 0, FGM = 0, FGA = 0, TPM = 0, TPA = 0, FTM = 0, FTA = 0, OREB = 0, FOUL = 0;

            foreach (PlayerBoxScore pbs in awayPBS)
            {
                REB += pbs.REB;
                AST += pbs.AST;
                STL += pbs.STL;
                TOS += pbs.TOS;
                BLK += pbs.BLK;
                FGM += pbs.FGM;
                FGA += pbs.FGA;
                TPM += pbs.TPM;
                TPA += pbs.TPA;
                FTM += pbs.FTM;
                FTA += pbs.FTA;
                OREB += pbs.OREB;
                FOUL += pbs.FOUL;
            }

            bs.REB1 = REB;
            bs.AST1 = AST;
            bs.STL1 = STL;
            bs.BLK1 = BLK;
            bs.TO1 = TOS;
            bs.FGM1 = FGM;
            bs.FGA1 = FGA;
            bs.TPM1 = TPM;
            bs.TPA1 = TPA;
            bs.FTM1 = FTM;
            bs.FTA1 = FTA;
            bs.OREB1 = OREB;
            bs.FOUL1 = FOUL;

            bs.PTS1 = (ushort) (bs.FGM1*2 + bs.TPM1 + bs.FTM1);

            REB = 0;
            AST = 0;
            STL = 0;
            TOS = 0;
            BLK = 0;
            FGM = 0;
            FGA = 0;
            TPM = 0;
            TPA = 0;
            FTM = 0;
            FTA = 0;
            OREB = 0;
            FOUL = 0;

            foreach (PlayerBoxScore pbs in homePBS)
            {
                REB += pbs.REB;
                AST += pbs.AST;
                STL += pbs.STL;
                TOS += pbs.TOS;
                BLK += pbs.BLK;
                FGM += pbs.FGM;
                FGA += pbs.FGA;
                TPM += pbs.TPM;
                TPA += pbs.TPA;
                FTM += pbs.FTM;
                FTA += pbs.FTA;
                OREB += pbs.OREB;
                FOUL += pbs.FOUL;
            }

            bs.REB2 = REB;
            bs.AST2 = AST;
            bs.STL2 = STL;
            bs.BLK2 = BLK;
            bs.TO2 = TOS;
            bs.FGM2 = FGM;
            bs.FGA2 = FGA;
            bs.TPM2 = TPM;
            bs.TPA2 = TPA;
            bs.FTM2 = FTM;
            bs.FTA2 = FTA;
            bs.OREB2 = OREB;
            bs.FOUL2 = FOUL;

            bs.PTS2 = (ushort) (bs.FGM2*2 + bs.TPM2 + bs.FTM2);
        }

        /// <summary>
        /// Handles the CopyingCellClipboardContent event of the colPlayerAway control.
        /// Uses a custom CopyingCellCpiboardContent event handler that replaces the player ID with the player name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        private void colPlayerAway_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, PlayersListAway);
        }

        /// <summary>
        /// Handles the CopyingCellClipboardContent event of the colPlayerHome control.
        /// Uses a custom CopyingCellCpiboardContent event handler that replaces the player ID with the player name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        private void colPlayerHome_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, PlayersListHome);
        }

        /// <summary>
        /// Handles the CopyingCellClipboardContent event of the PercentageColumn control.
        /// Uses a custom CopyingCellClipboardContent event handler that formats the percentage as a string before copying it to the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        private void PercentageColumn_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PercentageColumn_CopyingCellClipboardContent(e);
        }

        /// <summary>
        /// Handles the TextChanged event of the txtMINS1 control.
        /// Makes sure both teams have the same minutes played for the game.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void txtMINS1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!minsUpdating)
            {
                minsUpdating = true;
                txtMINS2.Text = txtMINS1.Text;
                minsUpdating = false;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the txtMINS2 control.
        /// Makes sure both teams have the same minutes played for the game.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs" /> instance containing the event data.</param>
        private void txtMINS2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!minsUpdating)
            {
                minsUpdating = true;
                txtMINS1.Text = txtMINS2.Text;
                minsUpdating = false;
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the tabControl1 control.
        /// Updates the metric stats for each team's players and calculates the best performers on-demand.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is TabControl)
            {
                if (tabControl1.SelectedItem == tabAwayMetric)
                {
                    UpdateMetric(1);
                }
                else if (tabControl1.SelectedItem == tabHomeMetric)
                {
                    UpdateMetric(2);
                }
                else if (tabControl1.SelectedItem == tabBest)
                {
                    UpdateMetric(1);
                    UpdateMetric(2);
                    UpdateBest();
                }
            }
        }

        /// <summary>
        /// Calculates the best performers from each team, and their most significant stats.
        /// </summary>
        private void UpdateBest()
        {
            try
            {
                if (pmsrListAway.Count == 0 && pmsrListHome.Count == 0)
                    return;

                pmsrListAway.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                pmsrListAway.Reverse();

                pmsrListHome.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                pmsrListHome.Reverse();
            }
            catch (Exception)
            {
                return;
            }

            string TeamBest;
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

            bool skipaway = pmsrListAway.Count == 0;
            bool skiphome = pmsrListHome.Count == 0;

            //if (skiphome || (!skipaway && pmsrListAway[0].GmSc > pmsrListHome[0].GmSc))
            if (skiphome || (!skipaway && Convert.ToInt32(txtPTS1.Text) > Convert.ToInt32(txtPTS2.Text)))
            {
                int bestID = pmsrListAway[0].ID;
                foreach (PlayerBoxScore pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == bestID)
                    {
                        pbsBest = pbs;
                    }
                }
                TeamBest = cmbTeam1.SelectedItem.ToString();
                awayid = 1;
                homeid = 0;
            }
            else
            {
                int bestID = pmsrListHome[0].ID;
                foreach (PlayerBoxScore pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == bestID)
                    {
                        pbsBest = pbs;
                    }
                }
                TeamBest = cmbTeam2.SelectedItem.ToString();
                awayid = 0;
                homeid = 1;
            }

            PlayerStats ps = pst[pbsBest.PlayerID];
            string text = pbsBest.GetBestStats(5, ps.Position1);
            txbMVP.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")";
            txbMVPStats.Text = TeamBest + "\n\n" + text;

            if (pmsrListAway.Count > awayid)
            {
                int id2 = pmsrListAway[awayid++].ID;
                foreach (PlayerBoxScore pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == id2)
                    {
                        pbsAway1 = pbs;
                    }
                }

                ps = pst[pbsAway1.PlayerID];
                text = pbsAway1.GetBestStats(5, ps.Position1);
                txbAway1.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (pmsrListAway.Count > awayid)
            {
                int id3 = pmsrListAway[awayid++].ID;
                foreach (PlayerBoxScore pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == id3)
                    {
                        pbsAway2 = pbs;
                    }
                }

                ps = pst[pbsAway2.PlayerID];
                text = pbsAway2.GetBestStats(5, ps.Position1);
                txbAway2.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (pmsrListAway.Count > awayid)
            {
                int id4 = pmsrListAway[awayid++].ID;
                foreach (PlayerBoxScore pbs in pbsAwayList)
                {
                    if (pbs.PlayerID == id4)
                    {
                        pbsAway3 = pbs;
                    }
                }

                ps = pst[pbsAway3.PlayerID];
                text = pbsAway3.GetBestStats(5, ps.Position1);
                txbAway3.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (pmsrListHome.Count > homeid)
            {
                int id2 = pmsrListHome[homeid++].ID;
                foreach (PlayerBoxScore pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == id2)
                    {
                        pbsHome1 = pbs;
                    }
                }

                ps = pst[pbsHome1.PlayerID];
                text = pbsHome1.GetBestStats(5, ps.Position1);
                txbHome1.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (pmsrListHome.Count > homeid)
            {
                int id3 = pmsrListHome[homeid++].ID;
                foreach (PlayerBoxScore pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == id3)
                    {
                        pbsHome2 = pbs;
                    }
                }

                ps = pst[pbsHome2.PlayerID];
                text = pbsHome2.GetBestStats(5, ps.Position1);
                txbHome2.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }

            if (pmsrListHome.Count > homeid)
            {
                int id4 = pmsrListHome[homeid++].ID;
                foreach (PlayerBoxScore pbs in pbsHomeList)
                {
                    if (pbs.PlayerID == id4)
                    {
                        pbsHome3 = pbs;
                    }
                }

                ps = pst[pbsHome3.PlayerID];
                text = pbsHome3.GetBestStats(5, ps.Position1);
                txbHome3.Text = ps.FirstName + " " + ps.LastName + " (" + ps.Position1 + ")\n\n" + text;
            }
        }

        /// <summary>
        /// Updates the metric stats for the specified team's players.
        /// </summary>
        /// <param name="team">1 if the away team's players' metric stats should be updated; anything else for the home team.</param>
        private void UpdateMetric(int team)
        {
            var ts = new TeamStats(cmbTeam1.SelectedItem.ToString());
            var tsopp = new TeamStats(cmbTeam2.SelectedItem.ToString());

            tryParseBS();
            if (!MainWindow.bs.done)
                return;

            TeamBoxScore bs = MainWindow.bs;

            if (team == 1)
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            else
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);

            ts.CalcMetrics(tsopp);

            var pmsrList = new List<PlayerStatsRow>();

            SortableBindingList<PlayerBoxScore> pbsList = team == 1 ? pbsAwayList : pbsHomeList;

            foreach (PlayerBoxScore pbs in pbsList)
            {
                if (pbs.PlayerID == -1)
                    continue;

                PlayerStats ps = pst[pbs.PlayerID].Clone();
                ps.ResetStats();
                ps.AddBoxScore(pbs, bs.isPlayoff);
                ps.CalcMetrics(ts, tsopp, new TeamStats("$$Empty"));
                pmsrList.Add(new PlayerStatsRow(ps));
            }

            pmsrList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            pmsrList.Reverse();

            if (team == 1)
            {
                pmsrListAway = new List<PlayerStatsRow>(pmsrList);
                dgvMetricAway.ItemsSource = pmsrListAway;
            }
            else
            {
                pmsrListHome = new List<PlayerStatsRow>(pmsrList);
                dgvMetricHome.ItemsSource = pmsrListHome;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPaste control.
        /// Imports the team and player box scores, if any, from the tab-separated values in the clipboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = Tools.SplitLinesToArray(Clipboard.GetText());
            int found = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string team = "";
                if (line.StartsWith("\t") && !(lines[i + 1].StartsWith("\t")))
                {
                    team = lines[i + 1].Split('\t')[0];
                }
                else
                    continue;

                if (found == 0)
                    cmbTeam1.SelectedItem = team;
                else
                    cmbTeam2.SelectedItem = team;

                found++;
                i += 4;

                if (found == 2)
                    break;
            }
            List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSV(lines);

            int status = 0;
            for (int j = 0; j < dictList.Count; j++)
            {
                Dictionary<string, string> dict = dictList[j];
                string name;
                try
                {
                    name = dict["Player"];
                }
                catch (Exception)
                {
                    MessageBox.Show("Couldn't detect a player's name in the pasted data. " +
                                    "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there.");
                    return;
                }

                if (name == "" &&
                    (dictList[j + 1]["Player"] == cmbTeam1.SelectedItem.ToString() ||
                     dictList[j + 1]["Player"] == cmbTeam2.SelectedItem.ToString()))
                {
                    status++;
                    continue;
                }

                if (status == 0)
                {
                    for (int i = 0; i < PlayersListAway.Count; i++)
                    {
                        if (PlayersListAway[i].Value == name)
                        {
                            try
                            {
                                pbsAwayList.Remove(pbsAwayList.Single(delegate(PlayerBoxScore pbs)
                                                                      {
                                                                          if (PlayersListAway[i].Key == pbs.PlayerID)
                                                                              return true;
                                                                          return false;
                                                                      }));
                            }
                            catch (Exception)
                            {
                            }

                            pbsAwayList.Add(new PlayerBoxScore(dict, PlayersListAway[i].Key, cmbTeam1.SelectedItem.ToString()));
                            break;
                        }
                    }
                }
                else if (status == 1)
                {
                    txtMINS1.Text = txtMINS1.Text.TrySetValue(dict, "MINS", typeof (UInt16));
                    txtPTS1.Text = txtPTS1.Text.TrySetValue(dict, "PTS", typeof (UInt16));
                    txtREB1.Text = txtREB1.Text.TrySetValue(dict, "REB", typeof (UInt16));
                    txtAST1.Text = txtAST1.Text.TrySetValue(dict, "AST", typeof (UInt16));
                    txtSTL1.Text = txtSTL1.Text.TrySetValue(dict, "STL", typeof (UInt16));
                    txtBLK1.Text = txtBLK1.Text.TrySetValue(dict, "BLK", typeof (UInt16));
                    txtTO1.Text = txtTO1.Text.TrySetValue(dict, "TO", typeof (UInt16));
                    txtFGM1.Text = txtFGM1.Text.TrySetValue(dict, "FGM", typeof (UInt16));
                    txtFGA1.Text = txtFGA1.Text.TrySetValue(dict, "FGA", typeof (UInt16));
                    txt3PM1.Text = txt3PM1.Text.TrySetValue(dict, "3PM", typeof (UInt16));
                    txt3PA1.Text = txt3PA1.Text.TrySetValue(dict, "3PA", typeof (UInt16));
                    txtFTM1.Text = txtFTM1.Text.TrySetValue(dict, "FTM", typeof (UInt16));
                    txtFTA1.Text = txtFTA1.Text.TrySetValue(dict, "FTA", typeof (UInt16));
                    txtOREB1.Text = txtOREB1.Text.TrySetValue(dict, "OREB", typeof (UInt16));
                    txtFOUL1.Text = txtFOUL1.Text.TrySetValue(dict, "FOUL", typeof (UInt16));
                    status++;
                }
                else if (status == 2)
                {
                    for (int i = 0; i < PlayersListHome.Count; i++)
                    {
                        if (PlayersListHome[i].Value == name)
                        {
                            try
                            {
                                pbsHomeList.Remove(pbsHomeList.Single(delegate(PlayerBoxScore pbs)
                                                                      {
                                                                          if (PlayersListHome[i].Key == pbs.PlayerID)
                                                                              return true;
                                                                          return false;
                                                                      }));
                            }
                            catch (Exception)
                            {
                            }

                            pbsHomeList.Add(new PlayerBoxScore(dict, PlayersListHome[i].Key, cmbTeam1.SelectedItem.ToString()));
                            break;
                        }
                    }
                }
                else if (status == 3)
                {
                    txtMINS2.Text = txtMINS2.Text.TrySetValue(dict, "MINS", typeof (UInt16));
                    txtPTS2.Text = txtPTS2.Text.TrySetValue(dict, "PTS", typeof (UInt16));
                    txtREB2.Text = txtREB2.Text.TrySetValue(dict, "REB", typeof (UInt16));
                    txtAST2.Text = txtAST2.Text.TrySetValue(dict, "AST", typeof (UInt16));
                    txtSTL2.Text = txtSTL2.Text.TrySetValue(dict, "STL", typeof (UInt16));
                    txtBLK2.Text = txtBLK2.Text.TrySetValue(dict, "BLK", typeof (UInt16));
                    txtTO2.Text = txtTO2.Text.TrySetValue(dict, "TO", typeof (UInt16));
                    txtFGM2.Text = txtFGM2.Text.TrySetValue(dict, "FGM", typeof (UInt16));
                    txtFGA2.Text = txtFGA2.Text.TrySetValue(dict, "FGA", typeof (UInt16));
                    txt3PM2.Text = txt3PM2.Text.TrySetValue(dict, "3PM", typeof (UInt16));
                    txt3PA2.Text = txt3PA2.Text.TrySetValue(dict, "3PA", typeof (UInt16));
                    txtFTM2.Text = txtFTM2.Text.TrySetValue(dict, "FTM", typeof (UInt16));
                    txtFTA2.Text = txtFTA2.Text.TrySetValue(dict, "FTA", typeof (UInt16));
                    txtOREB2.Text = txtOREB2.Text.TrySetValue(dict, "OREB", typeof (UInt16));
                    txtFOUL2.Text = txtFOUL2.Text.TrySetValue(dict, "FOUL", typeof (UInt16));
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event of the btnTools control.
        /// Opens the Tools context-menu on left click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void btnTools_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cxmTools.PlacementTarget = this;
            cxmTools.IsOpen = true;
        }

        /// <summary>
        /// Handles the Sorting event of any WPF DataGrid stat column.
        /// Uses a custom Sorting event handler to sort a stat in descending order if it hasn't been sorted yet.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridSortingEventArgs" /> instance containing the event data.</param>
        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }

        /// <summary>
        /// Handles the ShowToolTip event of the Any control.
        /// Uses a custom ShowToolTip event handler that shows the tooltip below the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private void Any_ShowToolTip(object sender, DependencyPropertyChangedEventArgs e)
        {
            GenericEventHandlers.Any_ShowToolTip(sender, e);
        }

        /// <summary>
        /// Handles the Click event of the btnCompareTeamAndPlayerStats control.
        /// Compares each team's stats to the total of its players' stats, to see if the stats match.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnCompareTeamAndPlayerStats_Click(object sender, RoutedEventArgs e)
        {
            int REB = 0, AST = 0, STL = 0, TOS = 0, BLK = 0, FGM = 0, FGA = 0, TPM = 0, TPA = 0, FTM = 0, FTA = 0, OREB = 0, FOUL = 0;

            foreach (PlayerBoxScore pbs in pbsAwayList)
            {
                REB += pbs.REB;
                AST += pbs.AST;
                STL += pbs.STL;
                TOS += pbs.TOS;
                BLK += pbs.BLK;
                FGM += pbs.FGM;
                FGA += pbs.FGA;
                TPM += pbs.TPM;
                TPA += pbs.TPA;
                FTM += pbs.FTM;
                FTA += pbs.FTA;
                OREB += pbs.OREB;
                FOUL += pbs.FOUL;
            }

            string team = cmbTeam1.SelectedItem.ToString();

            if (txtREB1.Text != REB.ToString())
            {
                comparisonError("REB", team);
                return;
            }
            if (txtAST1.Text != AST.ToString())
            {
                comparisonError("AST", team);
                return;
            }

            if (txtSTL1.Text != STL.ToString())
            {
                comparisonError("STL", team);
                return;
            }

            if (txtBLK1.Text != BLK.ToString())
            {
                comparisonError("BLK", team);
                return;
            }

            if (txtTO1.Text != TOS.ToString())
            {
                comparisonError("TO", team);
                return;
            }

            if (txtFGM1.Text != FGM.ToString())
            {
                comparisonError("FGM", team);
                return;
            }

            if (txtFGA1.Text != FGA.ToString())
            {
                comparisonError("FGA", team);
                return;
            }

            if (txt3PM1.Text != TPM.ToString())
            {
                comparisonError("3PM", team);
                return;
            }

            if (txt3PA1.Text != TPA.ToString())
            {
                comparisonError("3PA", team);
                return;
            }

            if (txtFTM1.Text != FTM.ToString())
            {
                comparisonError("FTM", team);
                return;
            }

            if (txtFTA1.Text != FTA.ToString())
            {
                comparisonError("FTA", team);
                return;
            }

            if (txtOREB1.Text != OREB.ToString())
            {
                comparisonError("OREB", team);
                return;
            }

            if (txtFOUL1.Text != FOUL.ToString())
            {
                comparisonError("FOUL", team);
                return;
            }

            REB = 0;
            AST = 0;
            STL = 0;
            TOS = 0;
            BLK = 0;
            FGM = 0;
            FGA = 0;
            TPM = 0;
            TPA = 0;
            FTM = 0;
            FTA = 0;
            OREB = 0;
            FOUL = 0;

            foreach (PlayerBoxScore pbs in pbsHomeList)
            {
                REB += pbs.REB;
                AST += pbs.AST;
                STL += pbs.STL;
                TOS += pbs.TOS;
                BLK += pbs.BLK;
                FGM += pbs.FGM;
                FGA += pbs.FGA;
                TPM += pbs.TPM;
                TPA += pbs.TPA;
                FTM += pbs.FTM;
                FTA += pbs.FTA;
                OREB += pbs.OREB;
                FOUL += pbs.FOUL;
            }

            team = cmbTeam2.SelectedItem.ToString();

            if (txtREB2.Text != REB.ToString())
            {
                comparisonError("REB", team);
                return;
            }
            if (txtAST2.Text != AST.ToString())
            {
                comparisonError("AST", team);
                return;
            }

            if (txtSTL2.Text != STL.ToString())
            {
                comparisonError("STL", team);
                return;
            }

            if (txtBLK2.Text != BLK.ToString())
            {
                comparisonError("BLK", team);
                return;
            }

            if (txtTO2.Text != TOS.ToString())
            {
                comparisonError("TO", team);
                return;
            }

            if (txtFGM2.Text != FGM.ToString())
            {
                comparisonError("FGM", team);
                return;
            }

            if (txtFGA2.Text != FGA.ToString())
            {
                comparisonError("FGA", team);
                return;
            }

            if (txt3PM2.Text != TPM.ToString())
            {
                comparisonError("3PM", team);
                return;
            }

            if (txt3PA2.Text != TPA.ToString())
            {
                comparisonError("3PA", team);
                return;
            }

            if (txtFTM2.Text != FTM.ToString())
            {
                comparisonError("FTM", team);
                return;
            }

            if (txtFTA2.Text != FTA.ToString())
            {
                comparisonError("FTA", team);
                return;
            }

            if (txtOREB2.Text != OREB.ToString())
            {
                comparisonError("OREB", team);
                return;
            }

            if (txtFOUL2.Text != FOUL.ToString())
            {
                comparisonError("FOUL", team);
                return;
            }

            MessageBox.Show("All team and player stats add up!");
        }

        /// <summary>
        /// Shows a message explaining which comparison between player and team stats failed.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="team">The team.</param>
        private void comparisonError(string stat, string team)
        {
            MessageBox.Show("The accumulated " + stat + " stat for the " + team + "'s players doesn't match the stat entered for the team.");
        }
    }
}