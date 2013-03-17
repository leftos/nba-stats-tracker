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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;

    using LeftosCommonLibrary.BeTimvwFramework;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.EventHandlers;
    using NBA_Stats_Tracker.Helper.Miscellaneous;

    using SQLite_Database;

    /// <summary>Allows the user to keep track of the box scores of a live game.</summary>
    public partial class LiveBoxScoreWindow
    {
        private readonly Brush _defaultBackground;
        private readonly string _playersT;
        private SortableBindingList<LivePlayerBoxScore> _lpbsAwayList = new SortableBindingList<LivePlayerBoxScore>();
        private SortableBindingList<LivePlayerBoxScore> _lpbsHomeList = new SortableBindingList<LivePlayerBoxScore>();
        private DataRowView _rowBeingEdited;
        private List<string> _teams;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LiveBoxScoreWindow" /> class.
        /// </summary>
        public LiveBoxScoreWindow()
        {
            InitializeComponent();

            _defaultBackground = cmbTeam1.Background;

            _playersT = "Players";

            if (MainWindow.CurSeason != SQLiteIO.GetMaxSeason(MainWindow.CurrentDB))
            {
                _playersT += "S" + MainWindow.CurSeason;
            }

            populateTeamsCombo();
        }

        private ObservableCollection<KeyValuePair<int, string>> playersListAway { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> playersListHome { get; set; }

        /// <summary>
        ///     Handles the CopyingCellClipboardContent event of the colPlayerAway control. Uses a custom CopyingCellClipboardContent event
        ///     handler that replaces the Player ID with the player's name.
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
        ///     Handles the CopyingCellClipboardContent event of the colPlayerHome control. Uses a custom CopyingCellClipboardContent event
        ///     handler that replaces the Player ID with the player's name.
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
        ///     Handles the SelectionChanged event of the cmbTeam1 control. Checks if the same team is selected for both home and away, and
        ///     updates the corresponding DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            updateDataGrid(1);
        }

        /// <summary>Checks if the same team is selected for both home and away, and changes the combo-box background accordingly.</summary>
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

        /// <summary>Updates the data grid for the specified team, filling it with all the team's players.</summary>
        /// <param name="team">1 for the away team; anything else for the home team.</param>
        private void updateDataGrid(int team)
        {
            SortableBindingList<LivePlayerBoxScore> pbsList;
            int teamID;
            if (team == 1)
            {
                try
                {
                    teamID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString());
                    pbsList = _lpbsAwayList;
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
                    pbsList = _lpbsHomeList;
                }
                catch (Exception)
                {
                    return;
                }
            }

            ObservableCollection<KeyValuePair<int, string>> playersList;
            updateLiveBoxScoreDataGrid(teamID, out playersList, ref pbsList, _playersT, false);

            if (team == 1)
            {
                colPlayerAway.ItemsSource = playersList;
                _lpbsAwayList = pbsList;
                dgvPlayersAway.ItemsSource = _lpbsAwayList;
            }
            else
            {
                colPlayerHome.ItemsSource = playersList;
                _lpbsHomeList = pbsList;
                dgvPlayersHome.ItemsSource = _lpbsHomeList;
            }
        }

        /// <summary>Populates the teams combo.</summary>
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

        /// <summary>
        ///     Handles the CellEditEnding event of the dataGrid control. Used to force the immediate update of other data-bound data in the
        ///     row.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridCellEditEndingEventArgs" /> instance containing the event data.
        /// </param>
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var rowView = e.Row.Item as DataRowView;
            _rowBeingEdited = rowView;
        }

        /// <summary>
        ///     Handles the CurrentCellChanged event of the dataGrid control. Used to force the immediate update of other data-bound data in
        ///     the row.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="EventArgs" /> instance containing the event data.
        /// </param>
        private void dataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (_rowBeingEdited != null)
            {
                _rowBeingEdited.EndEdit();
            }
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbTeam2 control. Checks if the same team is selected for both home and away, and
        ///     updates the corresponding DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            updateDataGrid(2);
        }

        /// <summary>Calculates the away team's stats by accumulating the player box scores.</summary>
        private void calculateAwayTeam()
        {
            txbAwayStats.Text = calculateTeam(_lpbsAwayList);
            compareScores();
        }

        /// <summary>Compares the two team's scores, makes the leading team's stats bold.</summary>
        private void compareScores()
        {
            int awayScore;
            int homeScore;
            try
            {
                awayScore = Convert.ToInt32(txbAwayStats.Text.Split(new[] { ' ' }, 2)[0]);
                homeScore = Convert.ToInt32(txbHomeStats.Text.Split(new[] { ' ' }, 2)[0]);
            }
            catch
            {
                return;
            }
            if (awayScore > homeScore)
            {
                txbAwayStats.FontWeight = FontWeights.Bold;
                txbHomeStats.FontWeight = FontWeights.Normal;
            }
            else if (homeScore > awayScore)
            {
                txbAwayStats.FontWeight = FontWeights.Normal;
                txbHomeStats.FontWeight = FontWeights.Bold;
            }
            else
            {
                txbAwayStats.FontWeight = FontWeights.Normal;
                txbHomeStats.FontWeight = FontWeights.Normal;
            }
        }

        /// <summary>Calculates the home team's stats by accumulating the player box scores.</summary>
        private void calculateHomeTeam()
        {
            txbHomeStats.Text = calculateTeam(_lpbsHomeList);
            compareScores();
        }

        /// <summary>Calculates the team's stats by accumulating the player box scores.</summary>
        /// <param name="pbsList">The team's LivePlayerBoxScore instances list.</param>
        /// <returns>A well-formatted string displaying all the calculated stats for the team.</returns>
        private string calculateTeam(IEnumerable<LivePlayerBoxScore> pbsList)
        {
            int reb = 0, ast = 0, stl = 0, tos = 0, blk = 0, oreb = 0, foul = 0, pts = 0;

            foreach (LivePlayerBoxScore pbs in pbsList)
            {
                pts += pbs.PTS;
                reb += pbs.REB;
                ast += pbs.AST;
                stl += pbs.STL;
                tos += pbs.TOS;
                blk += pbs.BLK;
                oreb += pbs.OREB;
                foul += pbs.FOUL;
            }

            string resp = String.Format(
                "{0} PTS - {1} REBS ({2} OREBS) - {3} ASTS - {4} BLKS - {5} STLS - {6} TOS - {7} FOUL",
                pts,
                reb,
                oreb,
                ast,
                blk,
                stl,
                tos,
                foul);

            return resp;
        }

        /// <summary>Calculates the box score entry in order to transfer the stats to the Box Score Window for further editing and saving.</summary>
        /// <returns></returns>
        private BoxScoreEntry calculateBoxScoreEntry()
        {
            var bs = new TeamBoxScore
                {
                    REB1 = 0,
                    AST1 = 0,
                    STL1 = 0,
                    TOS1 = 0,
                    BLK1 = 0,
                    FGM1 = 0,
                    TPM1 = 0,
                    FTM1 = 0,
                    OREB1 = 0,
                    FOUL1 = 0,
                    PTS1 = 0,
                    MINS1 = (ushort) MainWindow.GameLength,
                    REB2 = 0,
                    AST2 = 0,
                    STL2 = 0,
                    TOS2 = 0,
                    BLK2 = 0,
                    FGM2 = 0,
                    TPM2 = 0,
                    FTM2 = 0,
                    OREB2 = 0,
                    FOUL2 = 0,
                    PTS2 = 0,
                    MINS2 = (ushort) MainWindow.GameLength
                };

            foreach (LivePlayerBoxScore pbs in _lpbsAwayList)
            {
                bs.PTS1 += pbs.PTS;
                bs.REB1 += pbs.REB;
                bs.AST1 += pbs.AST;
                bs.STL1 += pbs.STL;
                bs.TOS1 += pbs.TOS;
                bs.BLK1 += pbs.BLK;
                bs.FGM1 += pbs.FGM;
                bs.TPM1 += pbs.TPM;
                bs.FTM1 += pbs.FTM;
                bs.OREB1 += pbs.OREB;
                bs.FOUL1 += pbs.FOUL;
            }

            foreach (LivePlayerBoxScore pbs in _lpbsHomeList)
            {
                bs.PTS2 += pbs.PTS;
                bs.REB2 += pbs.REB;
                bs.AST2 += pbs.AST;
                bs.STL2 += pbs.STL;
                bs.TOS2 += pbs.TOS;
                bs.BLK2 += pbs.BLK;
                bs.FGM2 += pbs.FGM;
                bs.TPM2 += pbs.TPM;
                bs.FTM2 += pbs.FTM;
                bs.OREB2 += pbs.OREB;
                bs.FOUL2 += pbs.FOUL;
            }

            bs.Team1ID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam1.SelectedItem.ToString());
            bs.Team2ID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeam2.SelectedItem.ToString());

            bs.GameDate = DateTime.Today;
            bs.SeasonNum = MainWindow.CurSeason;
            bs.Done = false;

            var bse = new BoxScoreEntry(bs) { PBSList = new List<PlayerBoxScore>() };
            foreach (LivePlayerBoxScore lpbs in _lpbsAwayList)
            {
                lpbs.TeamID = bs.Team1ID;
                bse.PBSList.Add(lpbs);
            }
            foreach (LivePlayerBoxScore lpbs in _lpbsHomeList)
            {
                lpbs.TeamID = bs.Team2ID;
                bse.PBSList.Add(lpbs);
            }

            /*
            
            foreach (LivePlayerBoxScore lpbs in _lpbsAwayList)
            {
                var pbs = new PlayerBoxScore(lpbs) {TeamID = bs.Team1ID};
                bse.PBSList.Add(pbs);
            }
            foreach (LivePlayerBoxScore lpbs in _lpbsHomeList)
            {
                var pbs = new PlayerBoxScore(lpbs) {TeamID = bs.Team2ID};
                bse.PBSList.Add(pbs);
            }
            */
            bse.Team1Display = cmbTeam1.SelectedItem.ToString();
            bse.Team2Display = cmbTeam2.SelectedItem.ToString();

            return bse;
        }

        /// <summary>Handles the SourceUpdated event of the IntegerUpDown control. Recalculates both teams' stats each time a stat is updated.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataTransferEventArgs" /> instance containing the event data.
        /// </param>
        private void integerUpDown_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            calculateAwayTeam();
            calculateHomeTeam();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to close without saving this Live Box Score?",
                App.AppName,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.No)
            {
                return;
            }
            DialogResult = false;
            Close();
        }

        /// <summary>
        ///     Handles the Click event of the btnCopy control. Copies all the data into a box score entry and views it in the Box Score
        ///     Window for further editing and saving.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam1.SelectedIndex == -1 || cmbTeam2.SelectedIndex == -1 || cmbTeam1.SelectedIndex == cmbTeam2.SelectedIndex)
            {
                return;
            }

            BoxScoreEntry bse = calculateBoxScoreEntry();
            DialogResult = true;
            MainWindow.TempBSE = bse;
            Close();
        }

        /// <summary>Updates the live box score data grid.</summary>
        /// <param name="teamID">Name of the team.</param>
        /// <param name="playersList">The players list.</param>
        /// <param name="pbsList">The player box score list.</param>
        /// <param name="playersT">The players' SQLite table name.</param>
        /// <param name="loading">
        ///     if set to <c>true</c>, it is assumed that a pre-existing box score is being loaded.
        /// </param>
        private static void updateLiveBoxScoreDataGrid(
            int teamID,
            out ObservableCollection<KeyValuePair<int, string>> playersList,
            ref SortableBindingList<LivePlayerBoxScore> pbsList,
            string playersT,
            bool loading)
        {
            var db = new SQLiteDatabase(MainWindow.CurrentDB);
            string q = "select * from " + playersT + " where TeamFin = \"" + teamID + "\"";
            q += " ORDER BY LastName ASC";
            DataTable res = db.GetDataTable(q);

            playersList = new ObservableCollection<KeyValuePair<int, string>>();
            if (!loading)
            {
                pbsList = new SortableBindingList<LivePlayerBoxScore>();
            }

            foreach (DataRow r in res.Rows)
            {
                var ps = new PlayerStats(r, MainWindow.TST);
                playersList.Add(new KeyValuePair<int, string>(ps.ID, ps.LastName + ", " + ps.FirstName));
            }

            for (int i = 0; i < pbsList.Count; i++)
            {
                LivePlayerBoxScore cur = pbsList[i];
                string name = MainWindow.PST[cur.PlayerID].LastName + ", " + MainWindow.PST[cur.PlayerID].FirstName;
                var player = new KeyValuePair<int, string>(cur.PlayerID, name);
                cur.Name = name;
                if (!playersList.Contains(player))
                {
                    playersList.Add(player);
                }
                pbsList[i] = cur;
            }
            playersList = new ObservableCollection<KeyValuePair<int, string>>(playersList.OrderBy(item => item.Value));

            if (!loading)
            {
                foreach (var p in playersList)
                {
                    pbsList.Add(new LivePlayerBoxScore { PlayerID = p.Key });
                }
            }
        }

        #region Drag and Drop

        private LivePlayerBoxScore _targetPerson;

        /// <summary>
        ///     Handles the MouseMove event of the DataGrid control. Implements row rearranging functionality via drag-and-drop. Handles the
        ///     drag event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseEventArgs" /> instance containing the event data.
        /// </param>
        private void dataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            BindingList<LivePlayerBoxScore> pbsList = Equals(sender, dgvPlayersAway) ? _lpbsAwayList : _lpbsHomeList;
            // This is what we're using as a cue to start a drag, but this can be 
            // customized as needed for an application. 
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Find the row and only drag it if it is already selected. 
                var row = findVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);
                if ((row != null) && row.IsSelected)
                {
                    // Perform the drag operation 
                    var selectedPerson = (LivePlayerBoxScore) row.Item;
                    DragDropEffects finalDropEffect = DragDrop.DoDragDrop(row, selectedPerson, DragDropEffects.Move);
                    if ((finalDropEffect == DragDropEffects.Move) && (_targetPerson != null))
                    {
                        // A Move drop was accepted 

                        // Determine the index of the item being dragged and the drop 
                        // location. If they are difference, then move the selected 
                        // item to the new location. 
                        int oldIndex = pbsList.IndexOf(selectedPerson);
                        int newIndex = pbsList.IndexOf(_targetPerson);
                        if (oldIndex != newIndex)
                        {
                            pbsList.Insert(newIndex + 1, selectedPerson);
                            pbsList.RemoveAt(oldIndex);
                        }

                        _targetPerson = null;
                    }
                }
            }
        }

        /// <summary>
        ///     Handles the CheckDropTarget event of the DataGrid control. Isn't currently used since this window only has DataGrid controls
        ///     containing LivePlayerBoxScore objects.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DragEventArgs" /> instance containing the event data.
        /// </param>
        private void dataGrid_CheckDropTarget(object sender, DragEventArgs e)
        {
            var row = findVisualParent<DataGridRow>(e.OriginalSource as UIElement);
            if ((row == null) || !(row.Item is LivePlayerBoxScore))
            {
                // Not over a DataGridRow that contains a LivePlayerBoxScore object 
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        ///     Handles the Drop event of the DataGrid control. Implements row rearranging functionality via drag-and-drop. Handles the drop
        ///     event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DragEventArgs" /> instance containing the event data.
        /// </param>
        private void dataGrid_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Verify that this is a valid drop and then store the drop target 
            var row = findVisualParent<DataGridRow>(e.OriginalSource as UIElement);
            if (row != null)
            {
                _targetPerson = row.Item as LivePlayerBoxScore;
                if (_targetPerson != null)
                {
                    e.Effects = DragDropEffects.Move;
                }
            }
        }

        /// <summary>
        ///     Finds the visual parent of the specified UI element. Used to determine the DataGrid that was the destination of the drop
        ///     event during drag-and-drop.
        /// </summary>
        /// <typeparam name="UIE">The type of the UI element.</typeparam>
        /// <param name="element">The UI element.</param>
        /// <returns></returns>
        private static UIE findVisualParent<UIE>(UIElement element) where UIE : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as UIE;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        #endregion
    }
}