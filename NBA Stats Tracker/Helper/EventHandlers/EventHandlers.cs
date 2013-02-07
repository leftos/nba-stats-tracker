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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Players.Injuries;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker.Helper.EventHandlers
{
    /// <summary>
    ///     Implements Event Handlers used by multiple controls from all over NBA Stats Tracker.
    /// </summary>
    public static class EventHandlers
    {
        /// <summary>
        ///     Handles the MouseDoubleClick event of any WPF DataGrid control containing PlayerStatsRow entries.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        /// <returns></returns>
        public static bool AnyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var s = sender as DataGrid;
            if (s != null && s.SelectedCells.Count > 0)
            {
                var psr = (PlayerStatsRow) s.SelectedItems[0];

                var pow = new PlayerOverviewWindow(psr.TeamF, psr.ID);
                pow.ShowDialog();

                return true;
            }
            return false;
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of any WPF DataGrid control containing team information.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        public static void AnyTeamDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var s = sender as DataGrid;
            if (s != null && s.SelectedCells.Count > 0)
            {
                var row = (DataRowView) s.SelectedItems[0];
                string team = row["Name"].ToString();

                var tow = new TeamOverviewWindow(team);
                tow.ShowDialog();
            }
        }

        /// <summary>
        ///     Formats the data being copied to the clipboard. Used for columns containing percentage data.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.
        /// </param>
        public static void PercentageColumn_CopyingCellClipboardContent(DataGridCellClipboardEventArgs e)
        {
            try
            {
                e.Content = String.Format("{0:F3}", e.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tried to format percentage column on copy: " + ex.Message);
            }
        }

        /// <summary>
        ///     Substitutes the Player ID value with the Player's name before copying the data to the clipboard.
        ///     Used for data-bound columns containing player selection combo-boxes.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.
        /// </param>
        /// <param name="playersList">The players list.</param>
        public static void PlayerColumn_CopyingCellClipboardContent(DataGridCellClipboardEventArgs e,
                                                                    IEnumerable<KeyValuePair<int, string>> playersList)
        {
            try
            {
                foreach (var p in playersList)
                {
                    if (Convert.ToInt32(e.Content) == p.Key)
                    {
                        e.Content = p.Value;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Tried to format player column on copy: " + ex.Message);
            }
        }

        /// <summary>
        ///     Sorts the column in descending order first, if it's not already sorted. Used for columns containing stats.
        /// </summary>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        public static void StatColumn_Sorting(DataGrid sender, DataGridSortingEventArgs e)
        {
            var namesNotToSortDescendingFirst = new List<string> {"Player", "Last Name", "First Name", "Team", "Returns", "Injury"};
            if (e.Column.Header.ToString() == "Returns")
            {
                e.Handled = true;

                e.Column.SortDirection = (e.Column.SortDirection != ListSortDirection.Ascending)
                                             ? ListSortDirection.Ascending
                                             : ListSortDirection.Descending;

                var lcv = (ListCollectionView) CollectionViewSource.GetDefaultView((sender).ItemsSource);
                lcv.CustomSort = e.Column.SortDirection == ListSortDirection.Ascending
                                     ? new PlayerInjuryDaysComparerAsc()
                                     : new PlayerInjuryDaysComparerDesc();
            }
            else if (e.Column.Header.ToString() == "Injury")
            {
                e.Handled = true;

                e.Column.SortDirection = (e.Column.SortDirection != ListSortDirection.Ascending)
                                             ? ListSortDirection.Ascending
                                             : ListSortDirection.Descending;

                var lcv = (ListCollectionView) CollectionViewSource.GetDefaultView((sender).ItemsSource);
                lcv.CustomSort = e.Column.SortDirection == ListSortDirection.Ascending
                                     ? new PlayerInjuryNameComparerAsc()
                                     : new PlayerInjuryNameComparerDesc();
            }
            else if (e.Column.Header.ToString() == "Team")
            {
                e.Handled = true;

                e.Column.SortDirection = (e.Column.SortDirection != ListSortDirection.Ascending)
                                             ? ListSortDirection.Ascending
                                             : ListSortDirection.Descending;

                var lcv = (ListCollectionView) CollectionViewSource.GetDefaultView((sender).ItemsSource);
                lcv.CustomSort = e.Column.SortDirection == ListSortDirection.Ascending
                                     ? new TeamNameComparerAsc()
                                     : new TeamNameComparerDesc();
            }
            else if (e.Column.SortDirection == null && e.Column.Header.ToString().Contains("Position") == false)
            {
                if (namesNotToSortDescendingFirst.Contains(e.Column.Header) == false)
                    e.Column.SortDirection = ListSortDirection.Ascending;
            }
        }

        /// <summary>
        ///     Calculates the score.
        /// </summary>
        /// <param name="fgm">The FGM.</param>
        /// <param name="fga">The FGA.</param>
        /// <param name="tpm">The 3PM.</param>
        /// <param name="tpa">The 3PA.</param>
        /// <param name="ftm">The FTM.</param>
        /// <param name="fta">The FTA.</param>
        /// <param name="pts">The PTS.</param>
        /// <param name="percentages">The percentages.</param>
        public static void CalculateScore(int fgm, int? fga, int tpm, int? tpa, int ftm, int? fta, out int pts, out string percentages)
        {
            try
            {
                pts = ((fgm - tpm)*2 + tpm*3 + ftm);
            }
            catch
            {
                pts = 0;
                percentages = "";
                return;
            }
            try
            {
                percentages = String.Format("FG%: {0:F3}\t3P%: {1:F3}\tFT%: {2:F3}", (float) fgm/fga, (float) tpm/tpa, (float) ftm/fta);
            }
            catch
            {
                percentages = "";
            }
        }
    }
}