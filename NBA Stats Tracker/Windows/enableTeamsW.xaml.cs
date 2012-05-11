#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System.Collections.Generic;
using System.Data;
using System.Windows;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for enableTeamsW.xaml
    /// </summary>
    public partial class enableTeamsW : Window
    {
        private readonly int _curSeason;
        private readonly string _currentDB;
        private readonly int _maxSeason;

        public enableTeamsW(string currentDB, int curSeason, int maxSeason)
        {
            InitializeComponent();
            _currentDB = currentDB;
            _curSeason = curSeason;
            _maxSeason = maxSeason;

            lblCurSeason.Content = "Current Season: " + _curSeason + "/" + _maxSeason;

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


            string q = "select DisplayName, isHidden from " + teamsT + " ORDER BY DisplayName ASC";

            var db = new SQLiteDatabase(_currentDB);
            DataTable res = db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
                if (!Helper.getBoolean(r, "isHidden"))
                {
                    lstEnabled.Items.Add(Helper.getString(r, "DisplayName"));
                }
                else
                {
                    lstDisabled.Items.Add(Helper.getString(r, "DisplayName"));
                }
            }
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
                var dict = new Dictionary<string, string>();
                dict.Add("isHidden", "False");
                db.Update(teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_teamsT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(oppT, dict, "DisplayName LIKE '" + name + "'");
                db.Update(pl_oppT, dict, "DisplayName LIKE '" + name + "'");
            }

            foreach (string name in lstDisabled.Items)
            {
                var dict = new Dictionary<string, string>();
                dict.Add("isHidden", "True");
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