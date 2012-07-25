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
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Windows;
using SQLite_Database;

namespace NBA_Stats_Tracker.Helper
{
    public static class EventHandlers
    {
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

        public static void StatColumn_Sorting(DataGridSortingEventArgs e)
        {
            if (e.Column.SortDirection == null) e.Column.SortDirection = ListSortDirection.Ascending;
        }

        public static void UpdateBoxScoreDataGrid(string TeamName,
                                                  out ObservableCollection<KeyValuePair<int, string>> PlayersList,
                                                  ref BindingList<PlayerBoxScore> pbsList, string playersT, bool loading)
        {
            var db = new SQLiteDatabase(MainWindow.currentDB);
            string q = "select * from " + playersT + " where TeamFin LIKE \"" + TeamName + "\"";
            q += " ORDER BY LastName ASC";
            DataTable res = db.GetDataTable(q);

            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            if (!loading) pbsList = new BindingList<PlayerBoxScore>();

            foreach (DataRow r in res.Rows)
            {
                var ps = new PlayerStats(r);
                PlayersList.Add(new KeyValuePair<int, string>(ps.ID, ps.LastName + ", " + ps.FirstName));
            }

            foreach (PlayerBoxScore cur in pbsList)
            {
                var player = new KeyValuePair<int, string>(cur.PlayerID,
                                                           MainWindow.pst[cur.PlayerID].LastName + ", " +
                                                           MainWindow.pst[cur.PlayerID].FirstName);
                if (!PlayersList.Contains(player))
                {
                    PlayersList.Add(player);
                }
            }
            PlayersList = new ObservableCollection<KeyValuePair<int, string>>(PlayersList.OrderBy(item => item.Value));
        }

        public static void UpdateBoxScoreDataGrid(string TeamName,
                                                  out ObservableCollection<KeyValuePair<int, string>> PlayersList,
                                                  ref BindingList<LivePlayerBoxScore> pbsList, string playersT,
                                                  bool loading)
        {
            var db = new SQLiteDatabase(MainWindow.currentDB);
            string q = "select * from " + playersT + " where TeamFin LIKE \"" + TeamName + "\"";
            q += " ORDER BY LastName ASC";
            DataTable res = db.GetDataTable(q);

            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            if (!loading) pbsList = new BindingList<LivePlayerBoxScore>();

            foreach (DataRow r in res.Rows)
            {
                var ps = new PlayerStats(r);
                PlayersList.Add(new KeyValuePair<int, string>(ps.ID, ps.LastName + ", " + ps.FirstName));
            }

            foreach (LivePlayerBoxScore cur in pbsList)
            {
                var player = new KeyValuePair<int, string>(cur.PlayerID,
                                                           MainWindow.pst[cur.PlayerID].LastName + ", " +
                                                           MainWindow.pst[cur.PlayerID].FirstName);
                if (!PlayersList.Contains(player))
                {
                    PlayersList.Add(player);
                }
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

        public static void calculateScore(int fgm, int? fga, int tpm, int? tpa, int ftm, int? fta, out int PTS,
                                          out string percentages)
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
                percentages = String.Format("FG%: {0:F3}\t3P%: {1:F3}\tFT%: {2:F3}", (float) fgm/fga, (float) tpm/tpa,
                                            (float) ftm/fta);
            }
            catch
            {
                percentages = "";
            }
        }
    }
}