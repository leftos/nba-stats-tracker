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

using System.Collections.Generic;
using System.Data;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for enableTeamsW.xaml
    /// </summary>
    public partial class DualListWindow
    {
        private readonly int _curSeason;
        private readonly string _currentDB;
        private readonly int _maxSeason;

        public DualListWindow(string currentDB, int curSeason, int maxSeason)
        {
            InitializeComponent();
            _currentDB = currentDB;
            _curSeason = curSeason;
            _maxSeason = maxSeason;

            lblCurSeason.Content = "Current Season: " + _curSeason + "/" + _maxSeason;

            string teamsT = "Teams";
            //string pl_teamsT = "PlayoffTeams";
            //string oppT = "Opponents";
            //string pl_oppT = "PlayoffOpponents";
            if (_curSeason != _maxSeason)
            {
                string s = "S" + _curSeason;
                teamsT += s;
                /*
                pl_teamsT += s;
                oppT += s;
                pl_oppT += s;
                 */
            }


            string q = "select DisplayName, isHidden from " + teamsT + " ORDER BY DisplayName ASC";

            var db = new SQLiteDatabase(_currentDB);
            DataTable res = db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
                if (!Tools.getBoolean(r, "isHidden"))
                {
                    lstEnabled.Items.Add(Tools.getString(r, "DisplayName"));
                }
                else
                {
                    lstDisabled.Items.Add(Tools.getString(r, "DisplayName"));
                }
            }
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            foreach (var key in MainWindow.tst.Keys)
            {
                if (MainWindow.tst[key].displayName == p)
                {
                    return MainWindow.tst[key].name;
                }
            }
            return "$$TEAMNOTFOUND: " + p;
        }

        private string GetDisplayNameFromTeam(string p)
        {
            foreach (var key in MainWindow.tst.Keys)
            {
                if (MainWindow.tst[key].name == p)
                {
                    return MainWindow.tst[key].displayName;
                }
            }
            return "$$TEAMNOTFOUND: " + p;
        }

        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            var names = new string[lstDisabled.SelectedItems.Count];
            lstDisabled.SelectedItems.CopyTo(names, 0);

            foreach (string name in names)
            {
                lstEnabled.Items.Add(name);
                lstDisabled.Items.Remove(name);
            }
        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            var names = new string[lstEnabled.SelectedItems.Count];
            lstEnabled.SelectedItems.CopyTo(names, 0);

            foreach (string name in names)
            {
                lstDisabled.Items.Add(name);
                lstEnabled.Items.Remove(name);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var db = new SQLiteDatabase(_currentDB);

            string teamsT = "Teams";
            string pl_teamsT = "PlayoffTeams";
            string oppT = "Opponents";
            string pl_oppT = "PlayoffOpponents";
            if (_curSeason != _maxSeason)
            {
                string s = "S" + _curSeason;
                teamsT += s;
                pl_teamsT += s;
                oppT += s;
                pl_oppT += s;
            }

            foreach (string name in lstEnabled.Items)
            {
                var dict = new Dictionary<string, string> {{"isHidden", "False"}};
                db.Update(teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(oppT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_oppT, dict, "DisplayName LIKE '" + name + "'");
            }

            foreach (string name in lstDisabled.Items)
            {
                string q = "select * from GameResults where SeasonNum = " + _curSeason + " AND (T1Name LIKE '" + GetCurTeamFromDisplayName(name) +
                           "' OR T2Name LIKE '" + GetCurTeamFromDisplayName(name) + "')";
                DataTable res = db.GetDataTable(q);

                if (res.Rows.Count > 0)
                {
                    MessageBoxResult r =
                        MessageBox.Show(
                            name + " has box scores this season. Are you sure you want to disable this team?",
                            "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (r == MessageBoxResult.No) continue;
                }

                var dict = new Dictionary<string, string> {{"isHidden", "True"}};
                db.Update(teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(oppT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_oppT, dict, "DisplayName LIKE '" + name + "'");
            }

            MainWindow.addInfo = "$$TEAMSENABLED";
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.addInfo = "";
            Close();
        }
    }
}