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
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Helper;
using SQLite_Database;
using EventHandlers = NBA_Stats_Tracker.Helper.EventHandlers;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class BoxScoreWindow
    {
        #region Mode enum

        public enum Mode
        {
            Update,
            View,
            ViewAndIgnore
        };

        #endregion

        private static Mode curmode = Mode.Update;

        private static TeamBoxScore _curTeamBoxScore;
        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        private List<string> Teams;
        private int curSeason;
        private Brush defaultBackground;
        private bool loading;
        private bool minsUpdating;
        private string playersT;
        private List<PlayerStatsRow> pmsrListAway, pmsrListHome;
        private Dictionary<int, PlayerStats> pst = new Dictionary<int, PlayerStats>();
        private Dictionary<int, TeamStats> tst = new Dictionary<int, TeamStats>();
        private Dictionary<int, TeamStats> tstopp = new Dictionary<int, TeamStats>();
        private bool onImport;

        public BoxScoreWindow(Mode _curmode = Mode.Update)
        {
            InitializeComponent();

            cbHistory.Visibility = Visibility.Hidden;

            prepareWindow(_curmode);

            MainWindow.bs = new TeamBoxScore();

            if (_curmode == Mode.Update)
            {
                _curTeamBoxScore = new TeamBoxScore();
            }
        }

        public BoxScoreWindow(Mode _curmode, int id) : this(_curmode)
        {
            LoadBoxScore(id);
        }

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

        public BoxScoreWindow(BoxScoreEntry bse, Dictionary<int, PlayerStats> pst, bool onImport) : this(bse, onImport)
        {
            this.pst = pst;

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

        private SortableBindingList<PlayerBoxScore> pbsAwayList { get; set; }
        private SortableBindingList<PlayerBoxScore> pbsHomeList { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> PlayersListAway { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> PlayersListHome { get; set; }

        private void LoadBoxScore(int id)
        {
            int bshistid = -1;

            for (int i = 0; i < MainWindow.bshist.Count; i++)
            {
                if (MainWindow.bshist[i].bs.id == id)
                    bshistid = i;
            }

            BoxScoreEntry bse = MainWindow.bshist[bshistid];
            _curTeamBoxScore = MainWindow.bshist[bshistid].bs;
            _curTeamBoxScore.bshistid = bshistid;
            LoadBoxScore(bse);
        }

        private void LoadBoxScore(BoxScoreEntry bse)
        {
            TeamBoxScore bs = bse.bs;
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
            txtOFF1.Text = bs.OREB1.ToString();
            txtPF1.Text = bs.FOUL1.ToString();
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
            txtOFF2.Text = bs.OREB2.ToString();
            txtPF2.Text = bs.FOUL2.ToString();

            dtpGameDate.SelectedDate = bs.gamedate;
            curSeason = bs.SeasonNum;
            SortedDictionary<string, int> TeamOrder;
            SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out tst, out tstopp, out TeamOrder);
            chkIsPlayoff.IsChecked = bs.isPlayoff;

            calculateScore1();
            calculateScore2();

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
                cmbTeam1.SelectedItem = Misc.GetDisplayNameFromTeam(tst, bs.Team1);
                cmbTeam2.SelectedItem = Misc.GetDisplayNameFromTeam(tst, bs.Team2);
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

        private void UpdateDataGrid(int team)
        {
            SortableBindingList<PlayerBoxScore> pbsList;
            string TeamName;
            if (team == 1)
            {
                try
                {
                    TeamName = Misc.GetCurTeamFromDisplayName(tst, cmbTeam1.SelectedItem.ToString());
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
                    TeamName = Misc.GetCurTeamFromDisplayName(tst, cmbTeam2.SelectedItem.ToString());
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
            }
            else
            {
                colPlayerHome.ItemsSource = PlayersList;
                PlayersListHome = PlayersList;
                pbsHomeList = pbsList;
                dgvPlayersHome.ItemsSource = pbsHomeList;
            }
        }

        private void prepareWindow(Mode _curmode)
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

            dgvPlayersAway.RowEditEnding += LeftosCommonLibrary.EventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvPlayersAway.PreviewKeyDown += LeftosCommonLibrary.EventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvPlayersAway.PreviewKeyUp += LeftosCommonLibrary.EventHandlers.Any_PreviewKeyUp_CheckTab;
            dgvPlayersHome.RowEditEnding += LeftosCommonLibrary.EventHandlers.WPFDataGrid_RowEditEnding_GoToNewRowOnTab;
            dgvPlayersHome.PreviewKeyDown += LeftosCommonLibrary.EventHandlers.Any_PreviewKeyDown_CheckTab;
            dgvPlayersHome.PreviewKeyUp += LeftosCommonLibrary.EventHandlers.Any_PreviewKeyUp_CheckTab;

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

            PopulateBoxScoreCombo();

            curmode = _curmode;

            MainWindow.bs.done = false;

            calculateScore1();
            calculateScore2();

            if (curmode == Mode.View || curmode == Mode.ViewAndIgnore)
            {
                label1.Content = "";
                chkDoNotUpdate.Visibility = Visibility.Hidden;
                txbDoNotUpdate.Visibility = Visibility.Hidden;
                Title = "View & Edit Box Score";
            }

            if (curmode == Mode.ViewAndIgnore)
            {
                label1.Content = "Any changes made to the box score will be ignored.";
                label1.FontWeight = FontWeights.Bold;
                btnOK.Visibility = Visibility.Hidden;
                btnCalculateTeams.Visibility = Visibility.Hidden;
                btnCancel.Content = "Close";
                Title = "View Box Score";
            }
        }

        private void PopulateBoxScoreCombo()
        {
            /*
            cbHistory.Items.Clear();
            foreach (BoxScoreEntry cur in MainWindow.bshist)
            {
                //if (cur.bs.SeasonNum == curSeason)
                //{
                cbHistory.Items.Add(cur.date.ToShortDateString() + " - " + cur.bs.Team1 + " @ " + cur.bs.Team2);
                //}
            }
            */
        }

        private void PopulateTeamsCombo()
        {
            Teams = new List<string>();
            foreach (var kvp in MainWindow.TeamOrder)
            {
                if (!tst[kvp.Value].isHidden)
                    Teams.Add(tst[kvp.Value].displayName);
            }

            Teams.Sort();

            cmbTeam1.ItemsSource = Teams;
            cmbTeam2.ItemsSource = Teams;
        }

        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.Items.Clear();

            for (int i = maxSeason; i > 0; i--)
            {
                bool addIt = true;
                if (cmbTeam1.SelectedItem != null)
                {
                    if (TeamStats.IsTeamHiddenInSeason(MainWindow.currentDB, Misc.GetCurTeamFromDisplayName(tst, cmbTeam1.SelectedItem.ToString()), i))
                        addIt = false;
                }
                if (cmbTeam2.SelectedItem != null)
                {
                    if (TeamStats.IsTeamHiddenInSeason(MainWindow.currentDB, Misc.GetCurTeamFromDisplayName(tst, cmbTeam2.SelectedItem.ToString()), i))
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
            if (curmode == Mode.Update)
            {
                tryParseBS();
                if (MainWindow.bs.done == false)
                    return;
            }
            else
            {
                if (curmode == Mode.View)
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
                MainWindow.bs.Team1 = Misc.GetCurTeamFromDisplayName(tst, cmbTeam1.SelectedItem.ToString());
                MainWindow.bs.Team2 = Misc.GetCurTeamFromDisplayName(tst, cmbTeam2.SelectedItem.ToString());
                MainWindow.bs.MINS2 = MainWindow.bs.MINS1 = Convert.ToUInt16(txtMINS1.Text);

                if (MainWindow.bs.MINS1 <= 0)
                {
                    MessageBox.Show("You have to enter the game's minutes. Usually 48 for 4 quarters, 53 for 1 overtime, 58 for 2 overtimes.");
                    throw (new Exception());
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
                    MessageBox.Show("The FGM stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.TPA1 < MainWindow.bs.TPM1)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the 3PA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGM1 < MainWindow.bs.TPM1)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the FGM stat.");
                    throw (new Exception());
                }

                MainWindow.bs.FTM1 = Convert.ToUInt16(txtFTM1.Text);
                MainWindow.bs.FTA1 = Convert.ToUInt16(txtFTA1.Text);
                if (MainWindow.bs.FTA1 < MainWindow.bs.FTM1)
                {
                    MessageBox.Show("The FTM stat can't be higher than the FTA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.OREB1 = Convert.ToUInt16(txtOFF1.Text);
                if (MainWindow.bs.OREB1 > MainWindow.bs.REB1)
                {
                    MessageBox.Show("The OFF stat can't be higher than the REB stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGA1 < MainWindow.bs.TPA1)
                {
                    MessageBox.Show("The 3PA stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.FOUL1 = Convert.ToUInt16(txtPF1.Text);
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
                    MessageBox.Show("The FGM stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.TPA2 < MainWindow.bs.TPM2)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the 3PA stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGM2 < MainWindow.bs.TPM2)
                {
                    MessageBox.Show("The 3PM stat can't be higher than the FGM stat.");
                    throw (new Exception());
                }
                if (MainWindow.bs.FGA2 < MainWindow.bs.TPA2)
                {
                    MessageBox.Show("The 3PA stat can't be higher than the FGA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.FTM2 = Convert.ToUInt16(txtFTM2.Text);
                MainWindow.bs.FTA2 = Convert.ToUInt16(txtFTA2.Text);
                if (MainWindow.bs.FTA2 < MainWindow.bs.FTM2)
                {
                    MessageBox.Show("The FTM stat can't be higher than the FTA stat.");
                    throw (new Exception());
                }

                MainWindow.bs.OREB2 = Convert.ToUInt16(txtOFF2.Text);

                if (MainWindow.bs.OREB2 > MainWindow.bs.REB2)
                {
                    MessageBox.Show("The OFF stat can't be higher than the REB stat.");
                    throw (new Exception());
                }

                MainWindow.bs.FOUL2 = Convert.ToUInt16(txtPF2.Text);

                MainWindow.bs.doNotUpdate = chkDoNotUpdate.IsChecked.GetValueOrDefault();

                #region Player Box Scores Check

                string Team1 = Misc.GetCurTeamFromDisplayName(tst, cmbTeam1.SelectedItem.ToString());
                string Team2 = Misc.GetCurTeamFromDisplayName(tst, cmbTeam2.SelectedItem.ToString());

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

        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam1.Header = cmbTeam1.SelectedItem;
            tabAwayMetric.Header = cmbTeam1.SelectedItem + " Metric Stats";
            grpAway.Header = cmbTeam1.SelectedItem;
            UpdateDataGrid(1);
        }

        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            tabTeam2.Header = cmbTeam2.SelectedItem;
            tabHomeMetric.Header = cmbTeam2.SelectedItem + " Metric Stats";
            grpHome.Header = cmbTeam2.SelectedItem;
            UpdateDataGrid(2);
        }

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

        private void calculateScore1(object sender = null, TextChangedEventArgs e = null)
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

        private void calculateScore2(object sender = null, TextChangedEventArgs e = null)
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

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            tryParseBS();
            if (MainWindow.bs.done)
            {
                string data1 =
                    String.Format(
                        "{0}\t\t\t\t{19}\t{1}\t{2}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:F3}\t{12}\t{13}\t{14:F3}\t{15}\t{16}\t{17:F3}\t{3}\t{18}",
                        cmbTeam1.SelectedItem, MainWindow.bs.PTS1, MainWindow.bs.REB1, MainWindow.bs.OREB1, MainWindow.bs.REB1 - MainWindow.bs.OREB1,
                        MainWindow.bs.AST1, MainWindow.bs.STL1, MainWindow.bs.BLK1, MainWindow.bs.TO1, MainWindow.bs.FGM1, MainWindow.bs.FGA1,
                        MainWindow.bs.FGM1/(float) MainWindow.bs.FGA1, MainWindow.bs.TPM1, MainWindow.bs.TPA1,
                        MainWindow.bs.TPM1/(float) MainWindow.bs.TPA1, MainWindow.bs.FTM1, MainWindow.bs.FTA1,
                        MainWindow.bs.FTM1/(float) MainWindow.bs.FTA1, MainWindow.bs.FOUL1, MainWindow.bs.MINS1);

                string data2 =
                    String.Format(
                        "{0}\t\t\t\t{19}\t{1}\t{2}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:F3}\t{12}\t{13}\t{14:F3}\t{15}\t{16}\t{17:F3}\t{3}\t{18}",
                        cmbTeam2.SelectedItem, MainWindow.bs.PTS2, MainWindow.bs.REB2, MainWindow.bs.OREB2, MainWindow.bs.REB2 - MainWindow.bs.OREB2,
                        MainWindow.bs.AST2, MainWindow.bs.STL2, MainWindow.bs.BLK2, MainWindow.bs.TO2, MainWindow.bs.FGM2, MainWindow.bs.FGA2,
                        MainWindow.bs.FGM2/(float) MainWindow.bs.FGA2, MainWindow.bs.TPM2, MainWindow.bs.TPA2,
                        MainWindow.bs.TPM2/(float) MainWindow.bs.TPA2, MainWindow.bs.FTM2, MainWindow.bs.FTA2,
                        MainWindow.bs.FTM2/(float) MainWindow.bs.FTA2, MainWindow.bs.FOUL2, MainWindow.bs.MINS2);

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

        private void _bsAnyTextbox_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox) sender;
            tb.SelectAll();
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1)
                return;

            curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
            //MainWindow.ChangeSeason(curSeason, MainWindow.getMaxSeason(MainWindow.currentDB));

            if (!onImport)
            {
                SQLiteIO.LoadSeason(MainWindow.currentDB, out tst, out tstopp, out pst, out MainWindow.TeamOrder, ref MainWindow.pt,
                                    ref MainWindow.bshist, curSeason, doNotLoadBoxScores: true);
                MainWindow.CopySeasonToMainWindow(tst, tstopp, pst);

                playersT = "Players";

                if (curSeason != maxSeason)
                {
                    playersT += "S" + curSeason;
                }

                PopulateBoxScoreCombo();
            }
            PopulateTeamsCombo();
        }

        private void btnCalculateTeams_Click(object sender, RoutedEventArgs e)
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

            txtREB1.Text = REB.ToString();
            txtAST1.Text = AST.ToString();
            txtSTL1.Text = STL.ToString();
            txtBLK1.Text = BLK.ToString();
            txtTO1.Text = TOS.ToString();
            txtFGM1.Text = FGM.ToString();
            txtFGA1.Text = FGA.ToString();
            txt3PM1.Text = TPM.ToString();
            txt3PA1.Text = TPA.ToString();
            txtFTM1.Text = FTM.ToString();
            txtFTA1.Text = FTA.ToString();
            txtOFF1.Text = OREB.ToString();
            txtPF1.Text = FOUL.ToString();

            calculateScore1();

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

            txtREB2.Text = REB.ToString();
            txtAST2.Text = AST.ToString();
            txtSTL2.Text = STL.ToString();
            txtBLK2.Text = BLK.ToString();
            txtTO2.Text = TOS.ToString();
            txtFGM2.Text = FGM.ToString();
            txtFGA2.Text = FGA.ToString();
            txt3PM2.Text = TPM.ToString();
            txt3PA2.Text = TPA.ToString();
            txtFTM2.Text = FTM.ToString();
            txtFTA2.Text = FTA.ToString();
            txtOFF2.Text = OREB.ToString();
            txtPF2.Text = FOUL.ToString();

            calculateScore2();
        }

        private void colPlayerAway_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, PlayersListAway);
        }

        private void colPlayerHome_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, PlayersListHome);
        }

        private void PercentageColumn_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PercentageColumn_CopyingCellClipboardContent(e);
        }

        private void txtMINS1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!minsUpdating)
            {
                minsUpdating = true;
                txtMINS2.Text = txtMINS1.Text;
                minsUpdating = false;
            }
        }

        private void txtMINS2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!minsUpdating)
            {
                minsUpdating = true;
                txtMINS1.Text = txtMINS2.Text;
                minsUpdating = false;
            }
        }

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

            var pbsList = team == 1 ? pbsAwayList : pbsHomeList;

            foreach (PlayerBoxScore pbs in pbsList)
            {
                if (pbs.PlayerID == -1)
                    continue;

                PlayerStats ps = pst[pbs.PlayerID].Clone();
                ps.ResetStats();
                ps.AddBoxScore(pbs);
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

        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = Tools.SplitLinesToArray(Clipboard.GetText());
            int found = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
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
            var dictList = CSV.DictionaryListFromTSV(lines);

            int status = 0;
            for (int j = 0; j < dictList.Count; j++)
            {
                var dict = dictList[j];
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
                    (dictList[j + 1]["Player"] == cmbTeam1.SelectedItem.ToString() || dictList[j + 1]["Player"] == cmbTeam2.SelectedItem.ToString()))
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
                    txtOFF1.Text = txtOFF1.Text.TrySetValue(dict, "OREB", typeof (UInt16));
                    txtPF1.Text = txtPF1.Text.TrySetValue(dict, "FOUL", typeof (UInt16));
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
                    txtOFF2.Text = txtOFF2.Text.TrySetValue(dict, "OREB", typeof (UInt16));
                    txtPF2.Text = txtPF2.Text.TrySetValue(dict, "FOUL", typeof (UInt16));
                    break;
                }
            }
        }

        private void btnTools_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            cxmTools.PlacementTarget = this;
            cxmTools.IsOpen = true;
        }

        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }

        private void Any_ShowToolTip(object sender, DependencyPropertyChangedEventArgs e)
        {
            LeftosCommonLibrary.EventHandlers.Any_ShowToolTip(sender, e);
        }
    }
}