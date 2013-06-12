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

namespace NBA_Stats_Tracker.Windows.MiscDialogs
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using LeftosCommonLibrary;

    using Microsoft.Win32;

    using NBA_Stats_Tracker.Helper.ListExtensions;
    using NBA_Stats_Tracker.Helper.Miscellaneous;
    using NBA_Stats_Tracker.Windows.MainInterface;

    using SQLite_Database;

    #endregion

    /// <summary>Provides a multi-purpose dual-list window interface (e.g. used to enable/disable (show/hide) teams and players).</summary>
    public partial class DualListWindow
    {
        #region Mode enum

        /// <summary>Provides the different modes of function for this window.</summary>
        public enum Mode
        {
            REDitor,
            HiddenTeams,
            HiddenPlayers,
            TradePlayers
        }

        #endregion

        private readonly int _curSeason;
        private readonly string _currentDB;

        private readonly int _maxSeason;

        private readonly Mode _mode;
        private readonly string _playersT;
        private readonly Dictionary<int, List<int>> _rosters;

        private readonly List<Dictionary<string, string>> _validTeams;

        private bool _changed = true;
        private BindingList<KeyValuePair<int, string>> _hiddenPlayers = new BindingList<KeyValuePair<int, string>>();
        private BindingList<KeyValuePair<int, string>> _shownPlayers = new BindingList<KeyValuePair<int, string>>();

        private DualListWindow()
        {
            InitializeComponent();

            cmbTeam1.Visibility = Visibility.Collapsed;
            cmbTeam2.Visibility = Visibility.Collapsed;
        }

        public DualListWindow(int team1, int team2)
            : this()
        {
            _mode = Mode.TradePlayers;

            cmbTeam1.Visibility = Visibility.Visible;
            cmbTeam2.Visibility = Visibility.Visible;

            lblEnabled.Visibility = Visibility.Collapsed;
            lblDisabled.Visibility = Visibility.Collapsed;
            btnLoadList.Visibility = Visibility.Collapsed;

            var teamsList =
                MainWindow.TST.Values.ToList()
                          .OrderBy(ts => ts.DisplayName)
                          .Select(ts => new KeyValuePair<int, string>(ts.ID, ts.DisplayName))
                          .ToList();

            _rosters = teamsList.ToDictionary(
                team => team.Key, team => MainWindow.PST.Where(ps => ps.Value.TeamF == team.Key).Select(ps => ps.Key).ToList());

            teamsList.Add(new KeyValuePair<int, string>(-1, "- Free Agency -"));
            _rosters.Add(-1, MainWindow.PST.Where(ps => !ps.Value.IsHidden && !ps.Value.IsSigned).Select(ps => ps.Key).ToList());

            cmbTeam1.ItemsSource = cmbTeam2.ItemsSource = teamsList;
            cmbTeam1.SelectedValuePath = cmbTeam2.SelectedValuePath = "Key";
            cmbTeam1.DisplayMemberPath = cmbTeam2.DisplayMemberPath = "Value";
            lstDisabled.SelectedValuePath = lstDisabled.SelectedValuePath = "Key";
            lstEnabled.DisplayMemberPath = lstDisabled.DisplayMemberPath = "Value";
            btnEnable.Content = "<-";
            btnDisable.Content = "->";

            cmbTeam1.SelectedValue = team1;
            cmbTeam2.SelectedValue = team2;

            txbDescription.Text = "Trade players between any teams";
            Title = "Trade Players";
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DualListWindow" /> class. Used to determine the active teams in an NBA 2K save file.
        /// </summary>
        /// <param name="validTeams">The valid teams.</param>
        /// <param name="activeTeams">The active teams.</param>
        public DualListWindow(List<Dictionary<string, string>> validTeams, List<Dictionary<string, string>> activeTeams)
            : this()
        {
            _validTeams = validTeams;
            _mode = Mode.REDitor;

            txbDescription.Text = "NST couldn't determine all the teams in your save. Please enable them.";

            foreach (var team in validTeams)
            {
                var s = String.Format("{0}", team["Name"]);
                if (team.ContainsKey("Year") && team["Year"] != "0")
                {
                    s += " '" + team["Year"].PadLeft(2, '0');
                }
                s += " (ID: " + team["ID"] + ")";
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="DualListWindow" /> class. Used to enable/disable players/teams for the season.
        /// </summary>
        /// <param name="currentDB">The current DB.</param>
        /// <param name="curSeason">The cur season.</param>
        /// <param name="maxSeason">The max season.</param>
        /// <param name="mode">The mode.</param>
        public DualListWindow(string currentDB, int curSeason, int maxSeason, Mode mode)
            : this()
        {
            _mode = mode;

            _currentDB = currentDB;
            _curSeason = curSeason;
            _maxSeason = maxSeason;

            txbDescription.Text = "Current Season: " + _curSeason + "/" + _maxSeason;

            var db = new SQLiteDatabase(_currentDB);

            var teamsT = "Teams";
            _playersT = "Players";
            if (_curSeason != _maxSeason)
            {
                var s = "S" + _curSeason;
                teamsT += s;
                _playersT += s;
            }

            if (mode == Mode.HiddenTeams)
            {
                var q = "select DisplayName, isHidden from " + teamsT + " ORDER BY DisplayName ASC";

                var res = db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    if (!ParseCell.GetBoolean(r, "isHidden"))
                    {
                        lstEnabled.Items.Add(ParseCell.GetString(r, "DisplayName"));
                    }
                    else
                    {
                        lstDisabled.Items.Add(ParseCell.GetString(r, "DisplayName"));
                    }
                }
                btnLoadList.Visibility = Visibility.Hidden;
            }
            else if (mode == Mode.HiddenPlayers)
            {
                Title = "Enable/Disable Players for Season";
                lblEnabled.Content = "Enabled Players";
                lblDisabled.Content = "Disabled Players";

                var q = "SELECT (LastName || ', ' || FirstName || ' (' || TeamFin || ')') AS Name, ID, isHidden FROM " + _playersT
                        + " ORDER BY LastName";

                var res = db.GetDataTable(q);

                foreach (DataRow r in res.Rows)
                {
                    var s = ParseCell.GetString(r, "Name");
                    if (!ParseCell.GetBoolean(r, "isHidden"))
                    {
                        _shownPlayers.Add(new KeyValuePair<int, string>(ParseCell.GetInt32(r, "ID"), s));
                    }
                    else
                    {
                        _hiddenPlayers.Add(new KeyValuePair<int, string>(ParseCell.GetInt32(r, "ID"), s));
                    }
                }

                _shownPlayers.RaiseListChangedEvents = true;
                lstEnabled.DisplayMemberPath = "Value";
                lstEnabled.SelectedValuePath = "Key";
                lstEnabled.ItemsSource = _shownPlayers;

                _hiddenPlayers.RaiseListChangedEvents = true;
                lstDisabled.DisplayMemberPath = "Value";
                lstDisabled.SelectedValuePath = "Key";
                lstDisabled.ItemsSource = _hiddenPlayers;

                btnLoadList.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>Finds the specified team's name by its displayName.</summary>
        /// <param name="displayName">The team's name.</param>
        /// <returns></returns>
        private static int getTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(MainWindow.TST, displayName);
        }

        /// <summary>Handles the Click event of the btnEnable control. Adds one or more disabled items to the enabled list, and sorts.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnEnable_Click(object sender, RoutedEventArgs e)
        {
            if (_mode != Mode.HiddenPlayers && _mode != Mode.TradePlayers)
            {
                var names = new string[lstDisabled.SelectedItems.Count];
                lstDisabled.SelectedItems.CopyTo(names, 0);

                foreach (var name in names)
                {
                    lstEnabled.Items.Add(name);
                    lstDisabled.Items.Remove(name);
                }
                var items = (from object item in lstEnabled.Items select item.ToString()).ToList();
                items.Sort();
                lstEnabled.Items.Clear();
                items.ForEach(item => lstEnabled.Items.Add(item));
                _changed = true;
            }
            else
            {
                var list = lstDisabled.SelectedItems.Cast<KeyValuePair<int, string>>().ToList();
                foreach (var item in list)
                {
                    _shownPlayers.Add(item);
                    _hiddenPlayers.Remove(item);
                    if (_mode == Mode.TradePlayers)
                    {
                        var teamID1 = ((KeyValuePair<int, string>) cmbTeam1.SelectedItem).Key;
                        var teamID2 = ((KeyValuePair<int, string>) cmbTeam2.SelectedItem).Key;
                        _rosters[teamID1].Add(item.Key);
                        _rosters[teamID2].Remove(item.Key);
                    }
                }
                _shownPlayers.Sort(ListExtensions.KVPStringComparison);
            }
        }

        /// <summary>Handles the Click event of the btnDisable control. Adds one or more enabled items to the disabled list and sorts.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnDisable_Click(object sender, RoutedEventArgs e)
        {
            if (_mode != Mode.HiddenPlayers && _mode != Mode.TradePlayers)
            {
                var names = new string[lstEnabled.SelectedItems.Count];
                lstEnabled.SelectedItems.CopyTo(names, 0);

                foreach (var name in names)
                {
                    lstDisabled.Items.Add(name);
                    lstEnabled.Items.Remove(name);
                }
                var items = (from object item in lstDisabled.Items select item.ToString()).ToList();
                items.Sort();
                lstDisabled.Items.Clear();
                items.ForEach(item => lstDisabled.Items.Add(item));

                _changed = true;
            }
            else
            {
                var list = lstEnabled.SelectedItems.Cast<KeyValuePair<int, string>>().ToList();
                foreach (var item in list)
                {
                    _hiddenPlayers.Add(item);
                    _shownPlayers.Remove(item);
                    if (_mode == Mode.TradePlayers)
                    {
                        var teamID1 = ((KeyValuePair<int, string>) cmbTeam1.SelectedItem).Key;
                        var teamID2 = ((KeyValuePair<int, string>) cmbTeam2.SelectedItem).Key;
                        _rosters[teamID2].Add(item.Key);
                        _rosters[teamID1].Remove(item.Key);
                    }
                }
                _hiddenPlayers.Sort(ListExtensions.KVPStringComparison);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == Mode.HiddenTeams)
            {
                var db = new SQLiteDatabase(_currentDB);

                var teamsT = "Teams";
                var plTeamsT = "PlayoffTeams";
                var oppT = "Opponents";
                var plOppT = "PlayoffOpponents";
                if (_curSeason != _maxSeason)
                {
                    var s = "S" + _curSeason;
                    teamsT += s;
                    plTeamsT += s;
                    oppT += s;
                    plOppT += s;
                }

                foreach (string name in lstEnabled.Items)
                {
                    var dict = new Dictionary<string, string> { { "isHidden", "False" } };
                    db.Update(teamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(plTeamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(oppT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(plOppT, dict, "DisplayName LIKE \"" + name + "\"");
                }

                foreach (string name in lstDisabled.Items)
                {
                    var q = "select * from GameResults where SeasonNum = " + _curSeason + " AND (Team1ID = "
                            + getTeamIDFromDisplayName(name) + " OR Team2ID = " + getTeamIDFromDisplayName(name) + ")";
                    var res = db.GetDataTable(q);

                    if (res.Rows.Count > 0)
                    {
                        var r = MessageBox.Show(
                            name + " have box scores this season. Are you sure you want to disable this team?",
                            "NBA Stats Tracker",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (r == MessageBoxResult.No)
                        {
                            continue;
                        }
                    }

                    var dict = new Dictionary<string, string> { { "isHidden", "True" } };
                    db.Update(teamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(plTeamsT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(oppT, dict, "DisplayName LIKE \"" + name + "\"");
                    db.Update(plOppT, dict, "DisplayName LIKE \"" + name + "\"");
                }

                MainWindow.AddInfo = "$$TEAMSENABLED";
                Close();
            }
            else if (_mode == Mode.HiddenPlayers)
            {
                var db = new SQLiteDatabase(_currentDB);

                var dataList = new List<Dictionary<string, string>>();
                var whereList = new List<string>();

                foreach (KeyValuePair<int, string> item in lstEnabled.Items)
                {
                    var dict = new Dictionary<string, string> { { "isHidden", "False" } };
                    dataList.Add(dict);
                    whereList.Add("ID = " + item.Key);
                }
                db.UpdateManyTransaction(_playersT, dataList, whereList);

                dataList = new List<Dictionary<string, string>>();
                whereList = new List<string>();
                foreach (KeyValuePair<int, string> item in lstDisabled.Items)
                {
                    var q =
                        "select * from PlayerResults INNER JOIN GameResults ON GameResults.GameID = PlayerResults.GameID where SeasonNum = "
                        + _curSeason + " AND PlayerID = " + item.Key;
                    var res = db.GetDataTable(q, true);

                    if (res.Rows.Count > 0)
                    {
                        var r = MessageBox.Show(
                            item.Value + " has box scores this season. Are you sure you want to disable them?",
                            "NBA Stats Tracker",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (r == MessageBoxResult.No)
                        {
                            continue;
                        }
                    }

                    var dict = new Dictionary<string, string> { { "isHidden", "True" }, { "isActive", "False" }, { "TeamFin", "" } };
                    dataList.Add(dict);
                    whereList.Add("ID = " + item.Key);
                }
                db.UpdateManyTransaction(_playersT, dataList, whereList);

                MainWindow.AddInfo = "$$PLAYERSENABLED";
                Close();
            }
            else if (_mode == Mode.REDitor)
            {
                if (lstEnabled.Items.Count != 30)
                {
                    MessageBox.Show(
                        "You can't have less or more than 30 teams enabled. You currently have " + lstEnabled.Items.Count + ".");
                    return;
                }

                //MainWindow.selectedTeams = new List<Dictionary<string, string>>(_activeTeams);
                MainWindow.SelectedTeams = new List<Dictionary<string, string>>();
                foreach (string team in lstEnabled.Items)
                {
                    var teamName = team.Split(new[] { " (ID: " }, StringSplitOptions.None)[0];
                    MainWindow.SelectedTeams.Add(
                        _validTeams.Find(
                            delegate(Dictionary<string, string> t)
                                {
                                    if (t["Name"] == teamName)
                                    {
                                        return true;
                                    }
                                    return false;
                                }));
                }
                MainWindow.SelectedTeamsChanged = _changed;
                DialogResult = true;
                Close();
            }
            else if (_mode == Mode.TradePlayers)
            {
                foreach (var pair in _rosters)
                {
                    var newTeam = pair.Key;
                    foreach (var pID in pair.Value)
                    {
                        var oldTeam = MainWindow.PST[pID].TeamF;
                        var oldTeamS = MainWindow.PST[pID].TeamS;
                        if (newTeam != oldTeam)
                        {
                            if (oldTeamS == -1)
                            {
                                MainWindow.PST[pID].TeamS = MainWindow.PST[pID].TeamF;
                            }
                            MainWindow.PST[pID].TeamF = newTeam;
                            MainWindow.PST[pID].IsSigned = newTeam != -1;
                        }
                    }
                }

                DialogResult = true;
                Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.AddInfo = "";
            DialogResult = false;
            Close();
        }

        /// <summary>Handles the Click event of the btnLoadList control. Used to load a previously saved active teams list.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnLoadList_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == Mode.REDitor)
            {
                var ofd = new OpenFileDialog
                    {
                        Title = "Load Active Teams List",
                        InitialDirectory = App.AppDocsPath,
                        Filter = "Active Teams List (*.red)|*.red"
                    };
                ofd.ShowDialog();

                if (String.IsNullOrWhiteSpace(ofd.FileName))
                {
                    return;
                }

                var stg = File.ReadAllText(ofd.FileName);
                var lines = stg.Split(new[] { '\n' });
                foreach (var line in lines)
                {
                    if (line.StartsWith("Active$$"))
                    {
                        var enabledTeams = new List<string>(line.Substring(8).Split(new[] { "$%" }, StringSplitOptions.None));
                        foreach (var team in enabledTeams)
                        {
                            var found = false;
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
                                if (lstEnabled.Items.Cast<string>().Any(eteam => eteam.Contains("(ID: " + team + ")")))
                                {
                                    found = true;
                                }
                                if (found)
                                {
                                    continue;
                                }

                                MessageBox.Show(
                                    "The active teams list you loaded is incompatible with the save you're trying to import.");
                                return;
                            }
                        }
                    }
                }

                var items = (from object item in lstEnabled.Items select item.ToString()).ToList();
                items.Sort();
                lstEnabled.Items.Clear();
                items.ForEach(item => lstEnabled.Items.Add(item));

                _changed = false;
            }
        }

        private void cmbTeam1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshPlayerLists();
        }

        private void refreshPlayerLists()
        {
            if (cmbTeam1.SelectedIndex != -1)
            {
                var id = ((KeyValuePair<int, string>) cmbTeam1.SelectedItem).Key;
                var players =
                    MainWindow.PST.Where(ps => _rosters[id].Contains(ps.Key))
                              .OrderBy(ps => ps.Value.FullName)
                              .Select(ps => new KeyValuePair<int, string>(ps.Key, ps.Value.FullName))
                              .ToList();
                _shownPlayers = new BindingList<KeyValuePair<int, string>>(players);
                lstEnabled.ItemsSource = _shownPlayers;
            }

            if (cmbTeam2.SelectedIndex != -1)
            {
                var id = ((KeyValuePair<int, string>) cmbTeam2.SelectedItem).Key;
                var players =
                    MainWindow.PST.Where(ps => _rosters[id].Contains(ps.Key))
                              .OrderBy(ps => ps.Value.FullName)
                              .Select(ps => new KeyValuePair<int, string>(ps.Key, ps.Value.FullName))
                              .ToList();
                _hiddenPlayers = new BindingList<KeyValuePair<int, string>>(players);
                lstDisabled.ItemsSource = _hiddenPlayers;
            }
        }

        private void cmbTeam2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshPlayerLists();
        }
    }
}