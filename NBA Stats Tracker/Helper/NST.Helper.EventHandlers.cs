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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using LeftosCommonLibrary.BeTimvwFramework;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Windows;
using SQLite_Database;

namespace NBA_Stats_Tracker.Helper
{
    /// <summary>
    /// Implements Event Handlers used by multiple controls from all over NBA Stats Tracker.
    /// </summary>
    public static class EventHandlers
    {
        /// <summary>
        /// Handles the MouseDoubleClick event of any WPF DataGrid control containing PlayerStatsRow entries.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        /// <returns></returns>
        public static bool AnyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid s = sender as DataGrid;
            if (s.SelectedCells.Count > 0)
            {
                var psr = (PlayerStatsRow)s.SelectedItems[0];

                var pow = new PlayerOverviewWindow(psr.TeamF, psr.ID);
                pow.ShowDialog();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of any WPF DataGrid control containing team information.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        public static void AnyTeamDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid s = sender as DataGrid;
            if (s.SelectedCells.Count > 0)
            {
                var row = (DataRowView) s.SelectedItems[0];
                string team = row["Name"].ToString();

                var tow = new TeamOverviewWindow(team);
                tow.ShowDialog();
            }
        }

        /// <summary>
        /// Formats the data being copied to the clipboard. Used for columns containing percentage data.
        /// </summary>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        public static void PercentageColumn_CopyingCellClipboardContent(DataGridCellClipboardEventArgs e)
        {
            try
            {
                e.Content = String.Format("{0:F3}", e.Content);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Substitutes the Player ID value with the Player's name before copying the data to the clipboard. 
        /// Used for data-bound columns containing player selection combo-boxes.
        /// </summary>
        /// <param name="e">The <see cref="DataGridCellClipboardEventArgs" /> instance containing the event data.</param>
        /// <param name="PlayersList">The players list.</param>
        public static void PlayerColumn_CopyingCellClipboardContent(DataGridCellClipboardEventArgs e,
                                                                    IEnumerable<KeyValuePair<int, string>> PlayersList)
        {
            try
            {
                foreach (var p in PlayersList)
                {
                    if (Convert.ToInt32(e.Content) == p.Key)
                    {
                        e.Content = p.Value;
                        break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Sorts the column in descending order first, if it's not already sorted. Used for columns containing stats.
        /// </summary>
        /// <param name="e">The <see cref="DataGridSortingEventArgs" /> instance containing the event data.</param>
        public static void StatColumn_Sorting(DataGridSortingEventArgs e)
        {
            var namesNotToSortDescendingFirst = new List<string> {"Player", "Last Name", "First Name", "Team"};
            if (e.Column.SortDirection == null)
            {
                if (namesNotToSortDescendingFirst.Contains(e.Column.Header) == false)
                    e.Column.SortDirection = ListSortDirection.Ascending;
            }
        }

        /// <summary>
        /// Updates the box score data grid.
        /// </summary>
        /// <param name="TeamName">Name of the team.</param>
        /// <param name="PlayersList">The players list.</param>
        /// <param name="pbsList">The player box score list.</param>
        /// <param name="playersT">The players' SQLite table name.</param>
        /// <param name="loading">if set to <c>true</c>, it is assumed that a pre-existing box score is being loaded.</param>
        public static void UpdateBoxScoreDataGrid(string TeamName, out ObservableCollection<KeyValuePair<int, string>> PlayersList,
                                                  ref SortableBindingList<PlayerBoxScore> pbsList, string playersT, bool loading)
        {
            var db = new SQLiteDatabase(MainWindow.currentDB);
            string q = "select * from " + playersT + " where TeamFin LIKE \"" + TeamName + "\"";
            q += " ORDER BY LastName ASC";
            DataTable res = db.GetDataTable(q);

            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            if (!loading)
                pbsList = new SortableBindingList<PlayerBoxScore>();

            foreach (DataRow r in res.Rows)
            {
                var ps = new PlayerStats(r);
                PlayersList.Add(new KeyValuePair<int, string>(ps.ID, ps.LastName + ", " + ps.FirstName));
            }

            for (int i = 0; i < pbsList.Count; i++)
            {
                PlayerBoxScore cur = pbsList[i];
                string name = MainWindow.pst[cur.PlayerID].LastName + ", " + MainWindow.pst[cur.PlayerID].FirstName;
                var player = new KeyValuePair<int, string>(cur.PlayerID, name);
                cur.Name = name;
                if (!PlayersList.Contains(player))
                {
                    PlayersList.Add(player);
                }
                pbsList[i] = cur;
            }
            PlayersList = new ObservableCollection<KeyValuePair<int, string>>(PlayersList.OrderBy(item => item.Value));
        }

        /// <summary>
        /// Updates the live box score data grid.
        /// </summary>
        /// <param name="TeamName">Name of the team.</param>
        /// <param name="PlayersList">The players list.</param>
        /// <param name="pbsList">The player box score list.</param>
        /// <param name="playersT">The players' SQLite table name.</param>
        /// <param name="loading">if set to <c>true</c>, it is assumed that a pre-existing box score is being loaded.</param>
        public static void UpdateBoxScoreDataGrid(string TeamName, out ObservableCollection<KeyValuePair<int, string>> PlayersList,
                                                  ref SortableBindingList<LivePlayerBoxScore> pbsList, string playersT, bool loading)
        {
            var db = new SQLiteDatabase(MainWindow.currentDB);
            string q = "select * from " + playersT + " where TeamFin LIKE \"" + TeamName + "\"";
            q += " ORDER BY LastName ASC";
            DataTable res = db.GetDataTable(q);

            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            if (!loading)
                pbsList = new SortableBindingList<LivePlayerBoxScore>();

            foreach (DataRow r in res.Rows)
            {
                var ps = new PlayerStats(r);
                PlayersList.Add(new KeyValuePair<int, string>(ps.ID, ps.LastName + ", " + ps.FirstName));
            }

            for (int i = 0; i < pbsList.Count; i++)
            {
                LivePlayerBoxScore cur = pbsList[i];
                string name = MainWindow.pst[cur.PlayerID].LastName + ", " + MainWindow.pst[cur.PlayerID].FirstName;
                var player = new KeyValuePair<int, string>(cur.PlayerID, name);
                cur.Name = name;
                if (!PlayersList.Contains(player))
                {
                    PlayersList.Add(player);
                }
                pbsList[i] = cur;
            }
            PlayersList = new ObservableCollection<KeyValuePair<int, string>>(PlayersList.OrderBy(item => item.Value));

            if (!loading)
            {
                foreach (var p in PlayersList)
                {
                    pbsList.Add(new LivePlayerBoxScore {PlayerID = p.Key});
                }
            }
        }

        /// <summary>
        /// Calculates the score.
        /// </summary>
        /// <param name="fgm">The FGM.</param>
        /// <param name="fga">The FGA.</param>
        /// <param name="tpm">The 3PM.</param>
        /// <param name="tpa">The 3PA.</param>
        /// <param name="ftm">The FTM.</param>
        /// <param name="fta">The FTA.</param>
        /// <param name="PTS">The PTS.</param>
        /// <param name="percentages">The percentages.</param>
        public static void calculateScore(int fgm, int? fga, int tpm, int? tpa, int ftm, int? fta, out int PTS, out string percentages)
        {
            try
            {
                PTS = ((fgm - tpm)*2 + tpm*3 + ftm);
            }
            catch
            {
                PTS = 0;
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