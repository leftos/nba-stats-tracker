using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LeftosCommonLibrary.BeTimvwFramework;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.Misc;

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Allows the user to keep track of the box scores of a live game.
    /// </summary>
    public partial class LiveBoxScoreWindow
    {
        private readonly Brush defaultBackground;
        private readonly string playersT;
        private List<string> Teams;
        private SortableBindingList<LivePlayerBoxScore> pbsAwayList = new SortableBindingList<LivePlayerBoxScore>();
        private SortableBindingList<LivePlayerBoxScore> pbsHomeList = new SortableBindingList<LivePlayerBoxScore>();
        private DataRowView rowBeingEdited;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveBoxScoreWindow" /> class.
        /// </summary>
        public LiveBoxScoreWindow()
        {
            InitializeComponent();

            defaultBackground = cmbTeam1.Background;

            playersT = "Players";

            if (MainWindow.curSeason != SQLiteIO.getMaxSeason(MainWindow.currentDB))
            {
                playersT += "S" + MainWindow.curSeason;
            }

            PopulateTeamsCombo();
        }

        private ObservableCollection<KeyValuePair<int, string>> PlayersListAway { get; set; }
        private ObservableCollection<KeyValuePair<int, string>> PlayersListHome { get; set; }

        /// <summary>
        /// Handles the CopyingCellClipboardContent event of the colPlayerAway control.
        /// Uses a custom CopyingCellClipboardContent event handler that replaces the Player ID with the player's name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        private void colPlayerAway_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, PlayersListAway);
        }

        /// <summary>
        /// Handles the CopyingCellClipboardContent event of the colPlayerHome control.
        /// Uses a custom CopyingCellClipboardContent event handler that replaces the Player ID with the player's name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        private void colPlayerHome_CopyingCellClipboardContent(object sender, DataGridCellClipboardEventArgs e)
        {
            EventHandlers.PlayerColumn_CopyingCellClipboardContent(e, PlayersListHome);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbTeam1 control.
        /// Checks if the same team is selected for both home and away, and updates the corresponding DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            UpdateDataGrid(1);
        }

        /// <summary>
        /// Checks if the same team is selected for both home and away, and changes the combo-box background accordingly.
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
        /// Updates the data grid for the specified team, filling it with all the team's players.
        /// </summary>
        /// <param name="team">1 for the away team; anything else for the home team.</param>
        private void UpdateDataGrid(int team)
        {
            SortableBindingList<LivePlayerBoxScore> pbsList;
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
            EventHandlers.UpdateBoxScoreDataGrid(TeamName, out PlayersList, ref pbsList, playersT, false);

            if (team == 1)
            {
                colPlayerAway.ItemsSource = PlayersList;
                pbsAwayList = pbsList;
                dgvPlayersAway.ItemsSource = pbsAwayList;
            }
            else
            {
                colPlayerHome.ItemsSource = PlayersList;
                pbsHomeList = pbsList;
                dgvPlayersHome.ItemsSource = pbsHomeList;
            }
        }

        /// <summary>
        /// Populates the teams combo.
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
        /// Handles the CellEditEnding event of the dataGrid control.
        /// Used to force the immediate update of other data-bound data in the row.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridCellEditEndingEventArgs" /> instance containing the event data.</param>
        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var rowView = e.Row.Item as DataRowView;
            rowBeingEdited = rowView;
        }

        /// <summary>
        /// Handles the CurrentCellChanged event of the dataGrid control.
        /// Used to force the immediate update of other data-bound data in the row.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void dataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (rowBeingEdited != null)
            {
                rowBeingEdited.EndEdit();
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbTeam2 control.
        /// Checks if the same team is selected for both home and away, and updates the corresponding DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            checkIfSameTeams();
            UpdateDataGrid(2);
        }

        /// <summary>
        /// Calculates the away team's stats by accumulating the player box scores.
        /// </summary>
        private void calculateAwayTeam()
        {
            txbAwayStats.Text = calculateTeam(pbsAwayList);
            compareScores();
        }

        /// <summary>
        /// Compares the two team's scores, makes the leading team's stats bold.
        /// </summary>
        private void compareScores()
        {
            int awayScore;
            int homeScore;
            try
            {
                awayScore = Convert.ToInt32(txbAwayStats.Text.Split(new[] {' '}, 2)[0]);
                homeScore = Convert.ToInt32(txbHomeStats.Text.Split(new[] {' '}, 2)[0]);
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

        /// <summary>
        /// Calculates the home team's stats by accumulating the player box scores.
        /// </summary>
        private void calculateHomeTeam()
        {
            txbHomeStats.Text = calculateTeam(pbsHomeList);
            compareScores();
        }

        /// <summary>
        /// Calculates the team's stats by accumulating the player box scores.
        /// </summary>
        /// <param name="pbsList">The team's LivePlayerBoxScore instances list.</param>
        /// <returns>A well-formatted string displaying all the calculated stats for the team.</returns>
        private string calculateTeam(IEnumerable<LivePlayerBoxScore> pbsList)
        {
            int REB = 0, AST = 0, STL = 0, TOS = 0, BLK = 0, FGM = 0, TPM = 0, FTM = 0, OREB = 0, FOUL = 0, PTS = 0;

            foreach (LivePlayerBoxScore pbs in pbsList)
            {
                PTS += pbs.PTS;
                REB += pbs.REB;
                AST += pbs.AST;
                STL += pbs.STL;
                TOS += pbs.TOS;
                BLK += pbs.BLK;
                FGM += pbs.FGM;
                TPM += pbs.TPM;
                FTM += pbs.FTM;
                OREB += pbs.OREB;
                FOUL += pbs.FOUL;
            }

            string resp = String.Format("{0} PTS - {1} REBS ({2} OREBS) - {3} ASTS - {4} BLKS - {5} STLS - {6} TOS - {7} FOUL", PTS, REB,
                                        OREB, AST, BLK, STL, TOS, FOUL);

            return resp;
        }

        /// <summary>
        /// Calculates the box score entry in order to transfer the stats to the Box Score Window for further editing and saving.
        /// </summary>
        /// <returns></returns>
        private BoxScoreEntry calculateBoxScoreEntry()
        {
            var bs = new TeamBoxScore
                     {
                         REB1 = 0,
                         AST1 = 0,
                         STL1 = 0,
                         TO1 = 0,
                         BLK1 = 0,
                         FGM1 = 0,
                         TPM1 = 0,
                         FTM1 = 0,
                         OREB1 = 0,
                         FOUL1 = 0,
                         PTS1 = 0,
                         MINS1 = (ushort) MainWindow.gameLength,
                         REB2 = 0,
                         AST2 = 0,
                         STL2 = 0,
                         TO2 = 0,
                         BLK2 = 0,
                         FGM2 = 0,
                         TPM2 = 0,
                         FTM2 = 0,
                         OREB2 = 0,
                         FOUL2 = 0,
                         PTS2 = 0,
                         MINS2 = (ushort) MainWindow.gameLength
                     };

            foreach (LivePlayerBoxScore pbs in pbsAwayList)
            {
                bs.PTS1 += pbs.PTS;
                bs.REB1 += pbs.REB;
                bs.AST1 += pbs.AST;
                bs.STL1 += pbs.STL;
                bs.TO1 += pbs.TOS;
                bs.BLK1 += pbs.BLK;
                bs.FGM1 += pbs.FGM;
                bs.TPM1 += pbs.TPM;
                bs.FTM1 += pbs.FTM;
                bs.OREB1 += pbs.OREB;
                bs.FOUL1 += pbs.FOUL;
            }

            foreach (LivePlayerBoxScore pbs in pbsHomeList)
            {
                bs.PTS2 += pbs.PTS;
                bs.REB2 += pbs.REB;
                bs.AST2 += pbs.AST;
                bs.STL2 += pbs.STL;
                bs.TO2 += pbs.TOS;
                bs.BLK2 += pbs.BLK;
                bs.FGM2 += pbs.FGM;
                bs.TPM2 += pbs.TPM;
                bs.FTM2 += pbs.FTM;
                bs.OREB2 += pbs.OREB;
                bs.FOUL2 += pbs.FOUL;
            }

            bs.Team1 = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam1.SelectedItem.ToString());
            bs.Team2 = Misc.GetCurTeamFromDisplayName(MainWindow.tst, cmbTeam2.SelectedItem.ToString());

            bs.gamedate = DateTime.Today;
            bs.SeasonNum = MainWindow.curSeason;
            bs.done = false;

            var bse = new BoxScoreEntry(bs) {pbsList = new List<PlayerBoxScore>()};
            foreach (LivePlayerBoxScore lpbs in pbsAwayList)
            {
                var pbs = new PlayerBoxScore(lpbs);
                pbs.Team = bs.Team1;
                bse.pbsList.Add(pbs);
            }
            foreach (LivePlayerBoxScore lpbs in pbsHomeList)
            {
                var pbs = new PlayerBoxScore(lpbs);
                pbs.Team = bs.Team2;
                bse.pbsList.Add(pbs);
            }

            bse.Team1Display = cmbTeam1.SelectedItem.ToString();
            bse.Team2Display = cmbTeam2.SelectedItem.ToString();

            return bse;
        }

        /// <summary>
        /// Handles the SourceUpdated event of the IntegerUpDown control.
        /// Recalculates both teams' stats each time a stat is updated.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataTransferEventArgs" /> instance containing the event data.</param>
        private void IntegerUpDown_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            calculateAwayTeam();
            calculateHomeTeam();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control. 
        /// Copies all the data into a box score entry and views it in the Box Score Window for further editing and saving.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam1.SelectedIndex == -1 || cmbTeam2.SelectedIndex == -1 || cmbTeam1.SelectedIndex == cmbTeam2.SelectedIndex)
                return;

            BoxScoreEntry bse = calculateBoxScoreEntry();
            DialogResult = true;
            MainWindow.tempbse = bse;
            Close();
        }

        #region Drag and Drop

        private LivePlayerBoxScore _targetPerson;

        /// <summary>
        /// Handles the MouseMove event of the DataGrid control.
        /// Implements row rearranging functionality via drag-and-drop. Handles the drag event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void DataGrid_MouseMove(object sender, MouseEventArgs e)
        {
            BindingList<LivePlayerBoxScore> pbsList = sender == dgvPlayersAway ? pbsAwayList : pbsHomeList;
            // This is what we're using as a cue to start a drag, but this can be 
            // customized as needed for an application. 
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Find the row and only drag it if it is already selected. 
                var row = FindVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);
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
        /// Handles the CheckDropTarget event of the DataGrid control.
        /// Isn't currently used since this window only has DataGrid controls containing LivePlayerBoxScore objects.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        private void DataGrid_CheckDropTarget(object sender, DragEventArgs e)
        {
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
            if ((row == null) || !(row.Item is LivePlayerBoxScore))
            {
                // Not over a DataGridRow that contains a LivePlayerBoxScore object 
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles the Drop event of the DataGrid control.
        /// Implements row rearranging functionality via drag-and-drop. Handles the drop event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
        private void DataGrid_Drop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Verify that this is a valid drop and then store the drop target 
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
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
        /// Finds the visual parent of the specified UI element. 
        /// Used to determine the DataGrid that was the destination of the drop event during drag-and-drop.
        /// </summary>
        /// <typeparam name="UIE">The type of the UI element.</typeparam>
        /// <param name="element">The UI element.</param>
        /// <returns></returns>
        private static UIE FindVisualParent<UIE>(UIElement element) where UIE : UIElement
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