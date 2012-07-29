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
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Helper;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for enableTeamsW.xaml
    /// </summary>
    public partial class DualListWindow
    {
        private readonly List<Dictionary<string, string>> _activeTeams;
        private readonly int _curSeason;
        private readonly string _currentDB;
        private readonly int _maxSeason;
        private readonly List<Dictionary<string, string>> _validTeams;
        private BindingList<KeyValuePair<int, string>> shownPlayers = new BindingList<KeyValuePair<int, string>>();
        private BindingList<KeyValuePair<int, string>> hiddenPlayers = new BindingList<KeyValuePair<int, string>>();

        private readonly Mode mode;
        private bool changed = true;
        private string _playersT;

        private DualListWindow()
        {
            InitializeComponent();
        }

        public DualListWindow(List<Dictionary<string, string>> validTeams, List<Dictionary<string, string>> activeTeams) : this()
        {
            _validTeams = validTeams;
            _activeTeams = activeTeams;
            mode = Mode.REditor;

            lblCurSeason.Content = "NST couldn't determine all the teams in your Association. Please enable them.";

            foreach (var team in validTeams)
            {
                string s = String.Format("{0} (ID: {1})", team["Name"], team["ID"]);
                if (!activeTeams.Contains(team))
                {
                    lstDisabled.Items.Add(s);
                }
                else
                {
                    lstEnabled.Items.Add(s);
                }
            }

            btnLoadList.Visibility = Visibility.Visible;
        }

        public DualListWindow(string currentDB, int curSeason, int maxSeason, Mode mode) : this()
        {
            this.mode = mode;

            _currentDB = currentDB;
            _curSeason = curSeason;
            _maxSeason = maxSeason;

            lblCurSeason.Content = "Current Season: " + _curSeason + "/" + _maxSeason;

            var db = new SQLiteDatabase(_currentDB);

            string teamsT = "Teams";
            _playersT = "Players";
            if (_curSeason != _maxSeason)
            {
                string s = "S" + _curSeason;
                teamsT += s;
                _playersT += s;
            }

            if (mode == Mode.HiddenTeams)
            {
                string q = "select DisplayName, isHidden from " + teamsT + " ORDER BY DisplayName ASC";

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
                btnLoadList.Visibility = Visibility.Hidden;
            }
            else if (mode == Mode.HiddenPlayers)
            {
                string q = "SELECT (LastName || ', ' || FirstName || ' (' || TeamFin || ')') AS Name, ID, isHidden FROM " + _playersT +
                           " ORDER BY LastName";

                DataTable res = db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    string s = Tools.getString(r, "Name");
                    if (!Tools.getBoolean(r, "isHidden"))
                    {
                        shownPlayers.Add(new KeyValuePair<int, string>(Tools.getInt(r, "ID"), s));
                    }
                    else
                    {
                        hiddenPlayers.Add(new KeyValuePair<int, string>(Tools.getInt(r, "ID"), s));
                    }
                }

                shownPlayers.RaiseListChangedEvents = true;
                lstEnabled.DisplayMemberPath = "Value";
                lstEnabled.SelectedValuePath = "Key";
                lstEnabled.ItemsSource = shownPlayers;

                hiddenPlayers.RaiseListChangedEvents = true;
                lstDisabled.DisplayMemberPath = "Value";
                lstDisabled.SelectedValuePath = "Key";
                lstDisabled.ItemsSource = hiddenPlayers;

                btnLoadList.Visibility = Visibility.Hidden;
            }
        }

        public DualListWindow(Mode mode)
        {
            InitializeComponent();

            this.mode = mode;

            if (mode == Mode.PickBoxScore)
            {
                btnLoadList.Visibility = Visibility.Hidden;
                var candidates = Interop.InteropREditor.teamsThatPlayedAGame;
                lblCurSeason.Content = "Select the two teams that you want to extract the box score for";

                if (candidates.Count > 2)
                {
                    foreach (var team in candidates)
                    {
                        lstDisabled.Items.Add(MainWindow.tst[team].displayName);
                    }
                }
                else if (candidates.Count == 2)
                {
                    foreach (var team in candidates)
                    {
                        lstEnabled.Items.Add(MainWindow.tst[team].displayName);
                    }
                }
            }
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            foreach (int key in MainWindow.tst.Keys)
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
            foreach (int key in MainWindow.tst.Keys)
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
            if (mode != Mode.HiddenPlayers)
            {
                var names = new string[lstDisabled.SelectedItems.Count];
                lstDisabled.SelectedItems.CopyTo(names, 0);

                foreach (string name in names)
                {
                    lstEnabled.Items.Add(name);
                    lstDisabled.Items.Remove(name);
                }
                List<string> items = new List<string>();
                foreach (var item in lstEnabled.Items)
                {
                    items.Add(item.ToString());
                }
                items.Sort();
                lstEnabled.Items.Clear();
                items.ForEach(item => lstEnabled.Items.Add(item));
                changed = true;
            }
            else
            {
                List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
                for (int i = 0; i < lstDisabled.SelectedItems.Count; i++)
                {
                    list.Add((KeyValuePair<int, string>) lstDisabled.SelectedItems[i]);
                }
                foreach (var item in list)
                {
                    shownPlayers.Add(item);
                    hiddenPlayers.Remove(item);
                }
                shownPlayers.Sort(ListExtensions.KVPStringComparison);
            }
        }

        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            if (mode != Mode.HiddenPlayers)
            {
                var names = new string[lstEnabled.SelectedItems.Count];
                lstEnabled.SelectedItems.CopyTo(names, 0);

                foreach (string name in names)
                {
                    lstDisabled.Items.Add(name);
                    lstEnabled.Items.Remove(name);
                }
                List<string> items = new List<string>();
                foreach (var item in lstDisabled.Items)
                {
                    items.Add(item.ToString());
                }
                items.Sort();
                lstDisabled.Items.Clear();
                items.ForEach(item => lstDisabled.Items.Add(item));

                changed = true;
            }
            else
            {
                List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
                for (int i = 0; i < lstEnabled.SelectedItems.Count; i++)
                {
                    list.Add((KeyValuePair<int, string>) lstEnabled.SelectedItems[i]);
                }
                foreach (var item in list)
                {
                    hiddenPlayers.Add(item);
                    shownPlayers.Remove(item);
                }
                hiddenPlayers.Sort(ListExtensions.KVPStringComparison);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.HiddenTeams)
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
                    db.Update(teamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(pl_teamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(oppT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(pl_oppT, dict, "DisplayName LIKE \"" + name + "\"");
                }

                foreach (string name in lstDisabled.Items)
                {
                    string q = "select * from GameResults where SeasonNum = " + _curSeason + " AND (T1Name LIKE \"" + GetCurTeamFromDisplayName(name) +
                               "\" OR T2Name LIKE \"" + GetCurTeamFromDisplayName(name) + "\")";
                    DataTable res = db.GetDataTable(q);

                    if (res.Rows.Count > 0)
                    {
                        MessageBoxResult r = MessageBox.Show(name + " have box scores this season. Are you sure you want to disable this team?",
                                                             "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (r == MessageBoxResult.No)
                            continue;
                    }

                    var dict = new Dictionary<string, string> {{"isHidden", "True"}};
                    db.Update(teamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(pl_teamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(oppT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(pl_oppT, dict, "DisplayName LIKE \"" + name + "\"");
                }

                MainWindow.addInfo = "$$TEAMSENABLED";
                Close();
            }
            else if (mode == Mode.HiddenPlayers)
            {
                var db = new SQLiteDatabase(_currentDB);

                List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();
                List<string> whereList = new List<string>();

                foreach (KeyValuePair<int, string> item in lstEnabled.Items)
                {
                    var dict = new Dictionary<string, string> {{"isHidden", "False"}};
                    dataList.Add(dict);
                    whereList.Add("ID = " + item.Key);
                }
                db.UpdateManyTransaction(_playersT, dataList, whereList);

                dataList = new List<Dictionary<string, string>>();
                whereList = new List<string>();
                foreach (KeyValuePair<int, string> item in lstDisabled.Items)
                {
                    string q = "select * from PlayerResults INNER JOIN GameResults ON GameResults.GameID = PlayerResults.GameID where SeasonNum = " +
                               _curSeason + " AND PlayerID = " + item.Key;
                    DataTable res = db.GetDataTable(q);

                    if (res.Rows.Count > 0)
                    {
                        MessageBoxResult r = MessageBox.Show(item.Value + " has box scores this season. Are you sure you want to disable them?",
                                                             "NBA Stats Tracker", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (r == MessageBoxResult.No)
                            continue;
                    }

                    var dict = new Dictionary<string, string> {{"isHidden", "True"}, {"isActive", "False"}, {"TeamFin", ""}};
                    dataList.Add(dict);
                    whereList.Add("ID = " + item.Key);
                }
                db.UpdateManyTransaction(_playersT, dataList, whereList);

                MainWindow.addInfo = "$$PLAYERSENABLED";
                Close();
            }
            else if (mode == Mode.REditor)
            {
                if (lstEnabled.Items.Count != 30)
                {
                    MessageBox.Show("You can't have less or more than 30 teams enabled. You currently have " + lstEnabled.Items.Count + ".");
                    return;
                }

                //MainWindow.selectedTeams = new List<Dictionary<string, string>>(_activeTeams);
                MainWindow.selectedTeams = new List<Dictionary<string, string>>();
                foreach (string team in lstEnabled.Items)
                {
                    string teamName = team.Split(new[] {" (ID: "}, StringSplitOptions.None)[0];
                    MainWindow.selectedTeams.Add(_validTeams.Find(delegate(Dictionary<string, string> t)
                                                                      {
                                                                          if (t["Name"] == teamName)
                                                                              return true;
                                                                          return false;
                                                                      }));
                }
                MainWindow.selectedTeamsChanged = changed;
                DialogResult = true;
                Close();
            }
            else if (mode == Mode.PickBoxScore)
            {
                if (lstEnabled.Items.Count != 2)
                    return;

                MessageBoxResult r = MessageBox.Show("Is " + lstEnabled.Items[0] + " the Home Team?", "NBA Stats Tracker",
                                                     MessageBoxButton.YesNoCancel);
                if (r == MessageBoxResult.Cancel)
                    return;

                Interop.InteropREditor.pickedTeams = new List<int>();

                if (r == MessageBoxResult.Yes)
                {
                    Interop.InteropREditor.pickedTeams.Add(MainWindow.TeamOrder[GetCurTeamFromDisplayName(lstEnabled.Items[1].ToString())]);
                    Interop.InteropREditor.pickedTeams.Add(MainWindow.TeamOrder[GetCurTeamFromDisplayName(lstEnabled.Items[0].ToString())]);
                }
                else
                {
                    Interop.InteropREditor.pickedTeams.Add(MainWindow.TeamOrder[GetCurTeamFromDisplayName(lstEnabled.Items[0].ToString())]);
                    Interop.InteropREditor.pickedTeams.Add(MainWindow.TeamOrder[GetCurTeamFromDisplayName(lstEnabled.Items[1].ToString())]);
                }

                DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.addInfo = "";
            DialogResult = false;
            Close();
        }

        private void btnLoadList_Click(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.REditor)
            {
                var ofd = new OpenFileDialog
                              {Title = "Load Active Teams List", InitialDirectory = App.AppDocsPath, Filter = "Active Teams List (*.red)|*.red"};
                ofd.ShowDialog();

                if (String.IsNullOrWhiteSpace(ofd.FileName))
                    return;

                string stg = File.ReadAllText(ofd.FileName);
                string[] lines = stg.Split(new[] {'\n'});
                foreach (string line in lines)
                {
                    if (line.StartsWith("Active$$"))
                    {
                        var enabledTeams = new List<string>(line.Substring(8).Split(new[] {"$%"}, StringSplitOptions.None));
                        foreach (string team in enabledTeams)
                        {
                            bool found = false;
                            foreach (string dteam in lstDisabled.Items)
                            {
                                if (dteam.Contains("(ID: " + team + ")"))
                                {
                                    lstDisabled.Items.Remove(dteam);
                                    lstEnabled.Items.Add(dteam);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                foreach (string eteam in lstEnabled.Items)
                                {
                                    if (eteam.Contains("(ID: " + team + ")"))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                    continue;

                                MessageBox.Show("The active teams list you loaded is incompatible with the save you're trying to import.");
                                return;
                            }
                        }
                    }
                }

                var items = new List<string>();
                foreach (var item in lstEnabled.Items)
                {
                    items.Add(item.ToString());
                }
                items.Sort();
                lstEnabled.Items.Clear();
                items.ForEach(item => lstEnabled.Items.Add(item));

                changed = false;
            }
        }

        #region Nested type: Mode

        public enum Mode
        {
            REditor,
            HiddenTeams,
            HiddenPlayers,
            PickBoxScore
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                                                                                  {
                                                                                      if (mode == Mode.PickBoxScore && lstEnabled.Items.Count == 2)
                                                                                      {
                                                                                          btnOK_Click(null, null);
                                                                                      }
                                                                                  }));
        }
    }
}