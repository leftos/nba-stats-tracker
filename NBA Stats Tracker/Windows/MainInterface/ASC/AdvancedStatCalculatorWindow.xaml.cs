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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ciloci.Flee;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.Miscellaneous;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface.ASC
{
    /// <summary>
    ///     Interaction logic for AdvancedStatCalculatorWindow.xaml
    /// </summary>
    public partial class AdvancedStatCalculatorWindow : Window
    {
        #region SelectionType enum

        public enum SelectionType
        {
            Team,
            Player
        };

        #endregion

        private readonly Dictionary<Selection, List<Filter>> _filters = new Dictionary<Selection, List<Filter>>();
        private readonly string _folder = App.AppDocsPath + @"\Advanced Stats Filters";

        private readonly List<string> _numericOptions = new List<string> {"<", "<=", "=", ">=", ">"};
        private readonly List<string> _positions = new List<string> {"Any", "None", "PG", "SG", "SF", "PF", "C"};

        private readonly List<string> _totals = new List<string>
                                                {
                                                    " ",
                                                    "PTS (PF)",
                                                    "PA",
                                                    "FGM",
                                                    "FGA",
                                                    "FG%",
                                                    "3PM",
                                                    "3PA",
                                                    "3P%",
                                                    "FTM",
                                                    "FTA",
                                                    "FT%",
                                                    "REB",
                                                    "OREB",
                                                    "AST",
                                                    "STL",
                                                    "BLK",
                                                    "TO",
                                                    "FOUL",
                                                    "MINS"
                                                };

        private bool _changingTimeframe;
        private int _curSeason;
        private bool _loading;
        private List<int> _playersToHighlight = new List<int>();
        private List<int> _teamsToHighlight = new List<int>();

        public AdvancedStatCalculatorWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<KeyValuePair<int, string>> playersList { get; set; }
        private ObservableCollection<PlayerStatsRow> psrList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrList_Not { get; set; }
        private ObservableCollection<PlayerStatsRow> psrList_Not { get; set; }
        private ObservableCollection<TeamStatsRow> tsrOppList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrOppList_Not { get; set; }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            _changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.Tf.EndDate;
            dtpStart.SelectedDate = MainWindow.Tf.StartDate;
            cmbSeasonNum.ItemsSource = MainWindow.MWInstance.cmbSeasonNum.ItemsSource;
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.Tf.SeasonNum);
            _curSeason = MainWindow.Tf.SeasonNum;
            cmbTFSeason.ItemsSource = MainWindow.MWInstance.cmbSeasonNum.ItemsSource;
            cmbTFSeason.SelectedItem = cmbSeasonNum.SelectedItem;
            if (MainWindow.Tf.IsBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            _changingTimeframe = false;

            _loading = true;
            cmbPosition1Filter.ItemsSource = _positions;
            cmbPosition2Filter.ItemsSource = _positions;
            cmbPosition1Filter.SelectedIndex = 0;
            cmbPosition2Filter.SelectedIndex = 0;
            formatTotalsForTeams();
            cmbTotalsOp.ItemsSource = _numericOptions;
            cmbTotalsPar.SelectedIndex = 0;
            cmbTotalsOp.SelectedIndex = 3;
            cmbTotalsPar2.SelectedIndex = 0;
            _loading = false;

            populateTeamsCombo();
        }

        private void formatTotalsForTeams()
        {
            List<string> newTotals = _totals.ToList();
            for (int i = 0; i < newTotals.Count; i++)
            {
                if (newTotals[i] == "PTS (PF)")
                {
                    newTotals[i] = "PF";
                    break;
                }
            }
            _totals.Skip(3).ToList().ForEach(item => newTotals.Add("Opp" + item));
            cmbTotalsPar.ItemsSource = newTotals;
            cmbTotalsPar2.ItemsSource = newTotals;
        }

        private void formatTotalsForPlayers()
        {
            List<string> newTotals = _totals.ToList();
            for (int i = 0; i < newTotals.Count; i++)
            {
                if (newTotals[i] == "PTS (PF)")
                {
                    newTotals[i] = "PTS";
                    break;
                }
            }
            cmbTotalsPar.ItemsSource = newTotals;
            cmbTotalsPar2.ItemsSource = newTotals;
        }

        private void populateTeamsCombo()
        {
            cmbTeamFilter.Items.Clear();
            cmbSelectedTeam.Items.Clear();
            var teams = new List<string>();
            MainWindow.TST.Values.ToList().ForEach(ts => teams.Add(ts.DisplayName));
            teams.Sort();
            cmbTeamFilter.Items.Add("- Any -");
            teams.ForEach(delegate(string i)
                          {
                              cmbTeamFilter.Items.Add(i);
                              cmbSelectedTeam.Items.Add(i);
                          });

            cmbTeamFilter.SelectedIndex = -1;
            cmbTeamFilter.SelectedIndex = 0;
            cmbPosition1Filter.SelectedIndex = 0;
            cmbPosition2Filter.SelectedIndex = 0;
        }

        private void cmbTeamFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            populatePlayersCombo();
        }

        private void populatePlayersCombo()
        {
            playersList = new ObservableCollection<KeyValuePair<int, string>>();
            List<PlayerStats> list = MainWindow.PST.Values.ToList();
            if (cmbPosition1Filter.SelectedItem != null && cmbPosition1Filter.SelectedItem.ToString() != "Any")
            {
                list = list.Where(ps => ps.Position1.ToString() == cmbPosition1Filter.SelectedItem.ToString()).ToList();
            }
            if (cmbPosition2Filter.SelectedItem != null && cmbPosition2Filter.SelectedItem.ToString() != "Any")
            {
                list = list.Where(ps => ps.Position2.ToString() == cmbPosition2Filter.SelectedItem.ToString()).ToList();
            }
            list = list.OrderBy(ps => ps.LastName).ThenBy(ps => ps.FirstName).ToList();
            if (chkIsActive.IsChecked.GetValueOrDefault() && cmbTeamFilter.SelectedItem.ToString() != "- Any -")
            {
                list =
                    list.Where(
                        ps =>
                        ps.TeamF == Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbTeamFilter.SelectedItem.ToString()) && ps.IsActive)
                        .ToList();
            }
            else if (chkIsActive.IsChecked == false)
            {
                list = list.Where(ps => !ps.IsActive).ToList();
            }
            list.ForEach(
                ps =>
                playersList.Add(new KeyValuePair<int, string>(ps.ID,
                                                              String.Format("{0}, {1} ({2})", ps.LastName, ps.FirstName,
                                                                            ps.Position1.ToString()))));

            cmbSelectedPlayer.ItemsSource = playersList;
        }

        private void cmbPosition1Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading)
                populatePlayersCombo();
        }

        private void cmbPosition2Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading)
                populatePlayersCombo();
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (cmbSeasonNum.SelectedIndex == -1)
                    return;

                cmbTFSeason.SelectedItem = cmbSeasonNum.SelectedItem;

                _curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

                if (MainWindow.Tf.SeasonNum != _curSeason || MainWindow.Tf.IsBetween)
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    MainWindow.ChangeSeason(_curSeason);
                    SQLiteIO.LoadSeason();
                }

                populateTeamsCombo();
                _changingTimeframe = false;
            }
        }

        private void cmbTFSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (cmbTFSeason.SelectedIndex == -1)
                    return;

                cmbSeasonNum.SelectedItem = cmbTFSeason.SelectedItem;
                rbStatsAllTime.IsChecked = true;

                _curSeason = ((KeyValuePair<int, string>) (((cmbTFSeason)).SelectedItem)).Key;

                if (MainWindow.Tf.SeasonNum != _curSeason || MainWindow.Tf.IsBetween)
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    MainWindow.ChangeSeason(_curSeason);
                    SQLiteIO.LoadSeason();
                }

                populateTeamsCombo();

                _changingTimeframe = false;
            }
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Tf = new Timeframe(_curSeason);
            if (!_changingTimeframe)
            {
                MainWindow.UpdateAllData();
                cmbSeasonNum_SelectionChanged(null, null);
            }
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            if (!_changingTimeframe)
            {
                MainWindow.UpdateAllData();
                populateTeamsCombo();
            }
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
                return;
            _changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            _changingTimeframe = false;
            MainWindow.UpdateAllData();
            populateTeamsCombo();
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
                return;
            _changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            _changingTimeframe = false;
            MainWindow.UpdateAllData();
            populateTeamsCombo();
        }

        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault())
                cmbTeamFilter.SelectedIndex = 0;
            else
                cmbTeamFilter.SelectedIndex = -1;
            populatePlayersCombo();
        }

        private void btnTotalsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSelectedPlayer.SelectedIndex == -1)
            {
                if (cmbSelectedTeam.SelectedIndex == -1)
                {
                    return;
                }
                else
                {
                    if (cmbTotalsPar.SelectedIndex < 1 || cmbTotalsOp.SelectedIndex == -1)
                        return;

                    if (!String.IsNullOrWhiteSpace(cmbTotalsPar2.SelectedItem.ToString()))
                    {
                        txtTotalsVal.Text = "";
                    }

                    int teamID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbSelectedTeam.SelectedItem.ToString());
                    KeyValuePair<Selection, List<Filter>> filter;
                    try
                    {
                        filter = _filters.Single(fil => fil.Key.SelectionType == SelectionType.Team && fil.Key.ID == teamID);
                    }
                    catch
                    {
                        _filters.Add(new Selection(SelectionType.Team, teamID), new List<Filter>());
                        filter = _filters.Single(fil => fil.Key.SelectionType == SelectionType.Team && fil.Key.ID == teamID);
                    }
                    try
                    {
                        filter.Value.Remove(
                            filter.Value.Single(
                                o =>
                                o.Parameter == cmbTotalsPar.SelectedItem.ToString() && o.Operator == cmbTotalsOp.SelectedItem.ToString()));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the properties of the one being inserted.");
                    }
                    finally
                    {
                        filter.Value.Add(new Filter(cmbTotalsPar.SelectedItem.ToString(), cmbTotalsOp.SelectedItem.ToString(),
                                                    cmbTotalsPar2.SelectedItem.ToString(), txtTotalsVal.Text));
                    }
                }
            }
            else
            {
                if (cmbTotalsPar.SelectedIndex == -1 || cmbTotalsOp.SelectedIndex == -1)
                    return;

                int playerID = ((KeyValuePair<int, string>) (((cmbSelectedPlayer)).SelectedItem)).Key;
                KeyValuePair<Selection, List<Filter>> filter;
                try
                {
                    filter = _filters.Single(fil => fil.Key.SelectionType == SelectionType.Player && fil.Key.ID == playerID);
                }
                catch
                {
                    _filters.Add(new Selection(SelectionType.Player, playerID), new List<Filter>());
                    filter = _filters.Single(fil => fil.Key.SelectionType == SelectionType.Player && fil.Key.ID == playerID);
                }
                if (cmbTotalsPar.SelectedIndex > 0)
                {
                    if (cmbTotalsPar.SelectedItem.ToString() == "PA")
                        return;

                    try
                    {
                        filter.Value.Remove(
                            filter.Value.Single(
                                o =>
                                o.Parameter == cmbTotalsPar.SelectedItem.ToString() && o.Operator == cmbTotalsOp.SelectedItem.ToString()));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the properties of the one being inserted.");
                    }
                    finally
                    {
                        filter.Value.Add(new Filter(cmbTotalsPar.SelectedItem.ToString(), cmbTotalsOp.SelectedItem.ToString(),
                                                    cmbTotalsPar2.SelectedItem.ToString(), txtTotalsVal.Text));
                    }
                }
                else
                {
                    try
                    {
                        filter.Value.Remove(filter.Value.Single(f => f.Parameter == "isStarter"));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the property isStarter.");
                    }
                    finally
                    {
                        string value;
                        if (chkIsStarter.IsChecked == null)
                        {
                            value = "Any";
                        }
                        else if (chkIsStarter.IsChecked == true)
                        {
                            value = "True";
                        }
                        else
                        {
                            value = "False";
                        }
                        filter.Value.Add(new Filter("isStarter", "is", "", value));
                    }

                    try
                    {
                        filter.Value.Remove(filter.Value.Single(f => f.Parameter == "isInjured"));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the property isInjured.");
                    }
                    finally
                    {
                        string value;
                        if (chkIsInjured.IsChecked == null)
                        {
                            value = "Any";
                        }
                        else if (chkIsInjured.IsChecked == true)
                        {
                            value = "True";
                        }
                        else
                        {
                            value = "False";
                        }
                        filter.Value.Add(new Filter("isInjured", "is", "", value));
                    }

                    try
                    {
                        filter.Value.Remove(filter.Value.Single(f => f.Parameter == "isOut"));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the property isOut.");
                    }
                    finally
                    {
                        string value;
                        if (chkIsOut.IsChecked == null)
                        {
                            value = "Any";
                        }
                        else if (chkIsOut.IsChecked == true)
                        {
                            value = "True";
                        }
                        else
                        {
                            value = "False";
                        }
                        filter.Value.Add(new Filter("isOut", "is", "", value));
                    }
                }
            }
            populateTotalsList();
        }

        private void populateTotalsList()
        {
            lstTotals.Items.Clear();
            foreach (var filter in _filters)
            {
                string s;
                if (filter.Key.SelectionType == SelectionType.Team)
                {
                    s = string.Format("(#T{0}) {1}: ", filter.Key.ID, MainWindow.TST.Values.Single(ts => ts.ID == filter.Key.ID).DisplayName);
                }
                else
                {
                    PlayerStats player = MainWindow.PST.Values.Single(ps => ps.ID == filter.Key.ID);
                    string teamName;
                    try
                    {
                        teamName = MainWindow.TST[player.TeamF].DisplayName;
                    }
                    catch (Exception)
                    {
                        teamName = "Inactive";
                    }
                    s = String.Format("(#P{4}) {0}, {1} ({2} - {3}): ", player.LastName, player.FirstName, player.Position1, teamName,
                                      player.ID);
                }
                foreach (var option in filter.Value)
                {
                    string s2 = s +
                                String.Format("{0} {1} {2}", option.Parameter, option.Operator,
                                              String.IsNullOrWhiteSpace(option.Parameter2) ? option.Value : option.Parameter2);
                    lstTotals.Items.Add(s2);
                }
            }
        }

        private void cmbSelectedPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectedPlayer.SelectedIndex == -1)
                return;

            cmbSelectedTeam.SelectedIndex = -1;
            formatTotalsForPlayers();
        }

        private void cmbSelectedTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectedTeam.SelectedIndex == -1)
                return;

            cmbSelectedPlayer.SelectedIndex = -1;
            formatTotalsForTeams();
        }

        private void btnTotalsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.SelectedIndex == -1)
                return;

            foreach (string item in lstTotals.SelectedItems)
            {
                int id = Convert.ToInt32(item.Substring(3).Split(')')[0]);
                string[] criterion = item.Split(':')[1].Trim().Split(' ');
                string parameter = criterion[0];
                string op = criterion[1];
                KeyValuePair<Selection, List<Filter>> filter = item.Substring(2, 1) == "T"
                                                                   ? _filters.Single(
                                                                       f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id)
                                                                   : _filters.Single(
                                                                       f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                filter.Value.Remove(filter.Value.Single(o => o.Parameter == parameter && o.Operator == op));
            }

            populateTotalsList();
        }

        /// <summary>
        ///     Handles the LoadingRow event of the dg control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridRowEventArgs" /> instance containing the event data.
        /// </param>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void dgvTeamStats_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        private void dgvPlayerStats_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _playersToHighlight.Clear();
            _teamsToHighlight.Clear();

            var bsToCalculate = new List<BoxScoreEntry>();
            var notBsToCalculate = new List<BoxScoreEntry>();
            foreach (var bse in MainWindow.BSHist)
            {
                bool keep = true;
                foreach (var filter in _filters)
                {
                    if (filter.Key.SelectionType == SelectionType.Team)
                    {
                        if (!_teamsToHighlight.Contains(filter.Key.ID))
                        {
                            _teamsToHighlight.Add(filter.Key.ID);
                        }
                        //string teamName = MainWindow.TeamOrder.Single(pair => pair.Value == filter.Key.ID).Key;
                        if (bse.BS.Team1ID != filter.Key.ID && bse.BS.Team2ID != filter.Key.ID)
                        {
                            keep = false;
                            break;
                        }
                        string p = bse.BS.Team1ID == filter.Key.ID ? "1" : "2";
                        string oppP = p == "1" ? "2" : "1";

                        foreach (var option in filter.Value)
                        {
                            string parameter;
                            if (!option.Parameter.StartsWith("Opp"))
                            {
                                switch (option.Parameter)
                                {
                                    case "PF":
                                        parameter = "PTS" + p;
                                        break;
                                    case "PA":
                                        parameter = "PTS" + oppP;
                                        break;
                                    default:
                                        parameter = option.Parameter + p;
                                        break;
                                }
                            }
                            else
                            {
                                parameter = option.Parameter.Substring(3) + oppP;
                            }
                            parameter = parameter.Replace("3P", "TP");
                            parameter = parameter.Replace("TO", "TOS");

                            string parameter2;
                            if (!option.Parameter2.StartsWith("Opp"))
                            {
                                switch (option.Parameter2)
                                {
                                    case "PF":
                                        parameter2 = "PTS" + p;
                                        break;
                                    case "PA":
                                        parameter2 = "PTS" + oppP;
                                        break;
                                    default:
                                        parameter2 = option.Parameter2 + p;
                                        break;
                                }
                            }
                            else
                            {
                                parameter2 = option.Parameter2.Substring(3) + oppP;
                            }
                            parameter2 = parameter2.Replace("3P", "TP");
                            parameter2 = parameter2.Replace("TO", "TOS");
                            var context = new ExpressionContext();

                            IGenericExpression<bool> ige;
                            if (String.IsNullOrWhiteSpace(parameter2))
                            {
                                if (!parameter.Contains("%"))
                                {
                                    ige =
                                        context.CompileGeneric<bool>(string.Format("{0} {1} {2}",
                                                                                   bse.BS.GetType()
                                                                                      .GetProperty(parameter)
                                                                                      .GetValue(bse.BS, null), option.Operator, option.Value));
                                }
                                else
                                {
                                    string par1 = parameter.Replace("%", "M");
                                    string par2 = parameter.Replace("%", "A");
                                    ige =
                                        context.CompileGeneric<bool>(
                                            string.Format("(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                          bse.BS.GetType().GetProperty(par1).GetValue(bse.BS, null),
                                                          bse.BS.GetType().GetProperty(par2).GetValue(bse.BS, null), option.Operator,
                                                          option.Value));
                                }
                            }
                            else
                            {
                                if (!parameter.Contains("%"))
                                {
                                    if (!parameter2.Contains("%"))
                                    {
                                        ige =
                                            context.CompileGeneric<bool>(string.Format("{0} {1} {2}", getValue(bse, parameter),
                                                                                       option.Operator, getValue(bse, parameter2)));
                                    }
                                    else
                                    {
                                        string par2Part1 = parameter2.Replace("%", "M");
                                        string par2Part2 = parameter2.Replace("%", "A");
                                        ige =
                                            context.CompileGeneric<bool>(
                                                string.Format("Cast({0}, double) {1} (Cast({2}, double) / Cast({3}, double))",
                                                              getValue(bse, parameter), option.Operator, getValue(bse, par2Part1),
                                                              getValue(bse, par2Part2)));
                                    }
                                }
                                else
                                {
                                    if (!parameter2.Contains("%"))
                                    {
                                        string par1Part1 = parameter.Replace("%", "M");
                                        string par1Part2 = parameter.Replace("%", "A");
                                        ige =
                                            context.CompileGeneric<bool>(
                                                string.Format("(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                              getValue(bse, par1Part1), getValue(bse, par1Part2), option.Operator,
                                                              getValue(bse, parameter2)));
                                    }
                                    else
                                    {
                                        string par1Part1 = parameter.Replace("%", "M");
                                        string par1Part2 = parameter.Replace("%", "A");
                                        string par2Part1 = parameter2.Replace("%", "M");
                                        string par2Part2 = parameter2.Replace("%", "A");
                                        ige =
                                            context.CompileGeneric<bool>(
                                                string.Format(
                                                    "(Cast({0}, double) / Cast({1}, double)) {2} (Cast({3}, double) / Cast({4}, double))",
                                                    getValue(bse, par1Part1), getValue(bse, par1Part2), option.Operator,
                                                    getValue(bse, par2Part1), getValue(bse, par2Part2)));
                                    }
                                }
                            }
                            if (ige.Evaluate() == false)
                            {
                                keep = false;
                                notBsToCalculate.Add(bse);
                                break;
                            }
                        }
                        if (!keep)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (!_playersToHighlight.Contains(filter.Key.ID))
                        {
                            _playersToHighlight.Add(filter.Key.ID);
                        }
                        PlayerBoxScore pbs;
                        try
                        {
                            pbs = bse.PBSList.Single(pbs1 => pbs1.PlayerID == filter.Key.ID);
                        }
                        catch (InvalidOperationException)
                        {
                            keep = false;
                            break;
                        }

                        foreach (var option in filter.Value)
                        {
                            string parameter = option.Parameter;
                            parameter = parameter.Replace("3P", "TP");
                            parameter = parameter.Replace("TO", "TOS");
                            var context = new ExpressionContext();

                            IGenericExpression<bool> ige;
                            if (!parameter.Contains("%"))
                            {
                                ige =
                                    context.CompileGeneric<bool>(string.Format("{0} {1} {2}",
                                                                               pbs.GetType().GetProperty(parameter).GetValue(pbs, null),
                                                                               option.Operator, option.Value));
                            }
                            else
                            {
                                string par1 = parameter.Replace("%", "M");
                                string par2 = parameter.Replace("%", "A");
                                ige =
                                    context.CompileGeneric<bool>(
                                        string.Format("(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                      pbs.GetType().GetProperty(par1).GetValue(pbs, null),
                                                      pbs.GetType().GetProperty(par2).GetValue(pbs, null), option.Operator, option.Value));
                            }
                            if (ige.Evaluate() == false)
                            {
                                keep = false;
                                notBsToCalculate.Add(bse);
                                break;
                            }
                        }
                        if (!keep)
                        {
                            break;
                        }
                    }
                }
                if (keep)
                {
                    bsToCalculate.Add(bse);
                }
            }

            calculateAdvancedStats(bsToCalculate, notBsToCalculate);

            tbcAdv.SelectedItem = tabPlayerStats;
        }

        private static object getValue(BoxScoreEntry bse, string parameter)
        {
            return bse.BS.GetType().GetProperty(parameter).GetValue(bse.BS, null);
        }

        private void calculateAdvancedStats(List<BoxScoreEntry> bsToCalculate, List<BoxScoreEntry> notBsToCalculate)
        {
            var advtst = new Dictionary<int, TeamStats>();
            var advtstOpp = new Dictionary<int, TeamStats>();
            var advpst = new Dictionary<int, PlayerStats>();
            var advPPtst = new Dictionary<int, TeamStats>();
            var advPPtstOpp = new Dictionary<int, TeamStats>();

            var advtstNot = new Dictionary<int, TeamStats>();
            var advtstOppNot = new Dictionary<int, TeamStats>();
            var advpstNot = new Dictionary<int, PlayerStats>();
            var advPPtstNot = new Dictionary<int, TeamStats>();
            var advPPtstOppNot = new Dictionary<int, TeamStats>();

            foreach (var bse in bsToCalculate)
            {
                int team1ID = bse.BS.Team1ID;
                int team2ID = bse.BS.Team2ID;
                TeamBoxScore bs = bse.BS;
                if (!advtst.ContainsKey(team1ID))
                {
                    advtst.Add(team1ID,
                               new TeamStats
                               {
                                   ID = team1ID,
                                   Name = MainWindow.TST[team1ID].Name,
                                   DisplayName = MainWindow.TST[team1ID].DisplayName
                               });
                    advtstOpp.Add(team1ID,
                                  new TeamStats
                                  {
                                      ID = team1ID,
                                      Name = MainWindow.TST[team1ID].Name,
                                      DisplayName = MainWindow.TST[team1ID].DisplayName
                                  });
                }
                TeamStats ts1 = advtst[team1ID];
                TeamStats ts1Opp = advtstOpp[team1ID];
                if (!advtst.ContainsKey(team2ID))
                {
                    //advTeamOrder.Add(MainWindow.tst[team2ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                    advtst.Add(team2ID,
                               new TeamStats
                               {
                                   ID = team2ID,
                                   Name = MainWindow.TST[team2ID].Name,
                                   DisplayName = MainWindow.TST[team2ID].DisplayName
                               });
                    advtstOpp.Add(team2ID,
                                  new TeamStats
                                  {
                                      ID = team2ID,
                                      Name = MainWindow.TST[team2ID].Name,
                                      DisplayName = MainWindow.TST[team2ID].DisplayName
                                  });
                }
                TeamStats ts2 = advtst[team2ID];
                TeamStats ts2Opp = advtstOpp[team2ID];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts2Opp, ref ts1Opp, true);
                foreach (var pbs in bse.PBSList)
                {
                    if (advpst.All(pair => pair.Key != pbs.PlayerID))
                    {
                        advpst.Add(pbs.PlayerID, MainWindow.PST[pbs.PlayerID].DeepClone());
                        advpst[pbs.PlayerID].ResetStats();
                    }
                    advpst[pbs.PlayerID].AddBoxScore(pbs, false);

                    int teamID = pbs.TeamID;
                    if (!advPPtst.ContainsKey(pbs.PlayerID))
                    {
                        advPPtst.Add(pbs.PlayerID,
                                     new TeamStats
                                     {
                                         ID = teamID,
                                         Name = MainWindow.TST[teamID].Name,
                                         DisplayName = MainWindow.TST[teamID].DisplayName
                                     });
                    }
                    if (!advPPtstOpp.ContainsKey(pbs.PlayerID))
                    {
                        advPPtstOpp.Add(pbs.PlayerID,
                                        new TeamStats
                                        {
                                            ID = teamID,
                                            Name = MainWindow.TST[teamID].Name,
                                            DisplayName = MainWindow.TST[teamID].DisplayName
                                        });
                    }
                    TeamStats ts = advPPtst[pbs.PlayerID];
                    TeamStats tsopp = advPPtstOpp[pbs.PlayerID];
                    if (team1ID == pbs.TeamID)
                    {
                        TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp, true);
                    }
                    else
                    {
                        TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts, true);
                    }
                }
            }

            foreach (var bse in notBsToCalculate)
            {
                int team1ID = bse.BS.Team1ID;
                int team2ID = bse.BS.Team2ID;
                TeamBoxScore bs = bse.BS;
                if (!advtstNot.ContainsKey(team1ID))
                {
                    //advTeamOrder.Add(MainWindow.tst[team1ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                    advtstNot.Add(team1ID,
                                  new TeamStats
                                  {
                                      ID = team1ID,
                                      Name = MainWindow.TST[team1ID].Name,
                                      DisplayName = MainWindow.TST[team1ID].DisplayName
                                  });
                    advtstOppNot.Add(team1ID,
                                     new TeamStats
                                     {
                                         ID = team1ID,
                                         Name = MainWindow.TST[team1ID].Name,
                                         DisplayName = MainWindow.TST[team1ID].DisplayName
                                     });
                }
                TeamStats ts1 = advtstNot[team1ID];
                TeamStats ts1Opp = advtstOppNot[team1ID];
                if (!advtstNot.ContainsKey(team2ID))
                {
                    //advTeamOrder.Add(MainWindow.tst[team2ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                    advtstNot.Add(team2ID,
                                  new TeamStats
                                  {
                                      ID = team2ID,
                                      Name = MainWindow.TST[team2ID].Name,
                                      DisplayName = MainWindow.TST[team2ID].DisplayName
                                  });
                    advtstOppNot.Add(team2ID,
                                     new TeamStats
                                     {
                                         ID = team2ID,
                                         Name = MainWindow.TST[team2ID].Name,
                                         DisplayName = MainWindow.TST[team2ID].DisplayName
                                     });
                }
                TeamStats ts2 = advtstNot[team2ID];
                TeamStats ts2Opp = advtstOppNot[team2ID];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts2Opp, ref ts1Opp, true);
                foreach (var pbs in bse.PBSList)
                {
                    if (pbs.IsOut)
                        continue;

                    if (advpstNot.All(pair => pair.Key != pbs.PlayerID))
                    {
                        advpstNot.Add(pbs.PlayerID, MainWindow.PST[pbs.PlayerID].DeepClone());
                        advpstNot[pbs.PlayerID].ResetStats();
                    }
                    advpstNot[pbs.PlayerID].AddBoxScore(pbs, false);

                    int teamID = pbs.TeamID;
                    if (!advPPtstNot.ContainsKey(pbs.PlayerID))
                    {
                        advPPtstNot.Add(pbs.PlayerID,
                                        new TeamStats
                                        {
                                            ID = teamID,
                                            Name = MainWindow.TST[teamID].Name,
                                            DisplayName = MainWindow.TST[teamID].DisplayName
                                        });
                    }
                    if (!advPPtstOppNot.ContainsKey(pbs.PlayerID))
                    {
                        advPPtstOppNot.Add(pbs.PlayerID,
                                           new TeamStats
                                           {
                                               ID = teamID,
                                               Name = MainWindow.TST[teamID].Name,
                                               DisplayName = MainWindow.TST[teamID].DisplayName
                                           });
                    }
                    TeamStats ts = advPPtstNot[pbs.PlayerID];
                    TeamStats tsopp = advPPtstOppNot[pbs.PlayerID];
                    if (team1ID == pbs.TeamID)
                    {
                        TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp, true);
                    }
                    else
                    {
                        TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts, true);
                    }
                }
            }

            TeamStats.CalculateAllMetrics(ref advtst, advtstOpp, false);
            TeamStats.CalculateAllMetrics(ref advtstOpp, advtst, false);
            PlayerStats.CalculateAllMetrics(ref advpst, advPPtst, advPPtstOpp, teamsPerPlayer: true);

            psrList = new ObservableCollection<PlayerStatsRow>();
            ObservableCollection<PlayerStatsRow> playerStatsRows = psrList;
            advpst.ToList().ForEach(item => highlightAndAddPlayers(item, ref playerStatsRows));
            tsrList = new ObservableCollection<TeamStatsRow>();
            ObservableCollection<TeamStatsRow> teamStatsRows = tsrList;
            advtst.ToList().ForEach(item => highlightAndAddTeams(item, ref teamStatsRows));
            tsrOppList = new ObservableCollection<TeamStatsRow>();
            ObservableCollection<TeamStatsRow> oppStatsRows = tsrOppList;
            advtstOpp.ToList().ForEach(item => highlightAndAddTeams(item, ref oppStatsRows));

            dgvTeamStats.ItemsSource = tsrList;
            dgvOppStats.ItemsSource = tsrOppList;
            dgvPlayerStats.ItemsSource = psrList;

            TeamStats.CalculateAllMetrics(ref advtstNot, advtstOppNot, false);
            TeamStats.CalculateAllMetrics(ref advtstOppNot, advtstNot, false);
            PlayerStats.CalculateAllMetrics(ref advpstNot, advPPtstNot, advPPtstOppNot, teamsPerPlayer: true);

            psrList_Not = new ObservableCollection<PlayerStatsRow>();
            ObservableCollection<PlayerStatsRow> playerStatsRowsNot = psrList_Not;
            advpstNot.ToList().ForEach(item => highlightAndAddPlayers(item, ref playerStatsRowsNot));
            tsrList_Not = new ObservableCollection<TeamStatsRow>();
            ObservableCollection<TeamStatsRow> teamStatsRowsNot = tsrList_Not;
            advtstNot.ToList().ForEach(item => highlightAndAddTeams(item, ref teamStatsRowsNot));
            tsrOppList_Not = new ObservableCollection<TeamStatsRow>();
            ObservableCollection<TeamStatsRow> oppStatsRowsNot = tsrOppList_Not;
            advtstOppNot.ToList().ForEach(item => highlightAndAddTeams(item, ref oppStatsRowsNot));

            dgvTeamStatsUnmet.ItemsSource = tsrList_Not;
            dgvOppStatsUnmet.ItemsSource = tsrOppList_Not;
            dgvPlayerStatsUnmet.ItemsSource = psrList_Not;

            bsToCalculate.ForEach(bse => bse.BS.PrepareForDisplay(advtst));
            notBsToCalculate.ForEach(bse => bse.BS.PrepareForDisplay(advtstNot));
            dgvBoxScores.ItemsSource = bsToCalculate.Select(bse => bse.BS);
            dgvBoxScoresUnmet.ItemsSource = notBsToCalculate.Select(bse => bse.BS);
        }

        private void highlightAndAddTeams(KeyValuePair<int, TeamStats> pair, ref ObservableCollection<TeamStatsRow> tsrObservable)
        {
            var tsr = new TeamStatsRow(pair.Value);
            if (_teamsToHighlight.Contains(tsr.ID))
            {
                tsr.Highlight = true;
            }
            tsrObservable.Add(tsr);
        }

        private void highlightAndAddPlayers(KeyValuePair<int, PlayerStats> pair, ref ObservableCollection<PlayerStatsRow> psrObservable)
        {
            var psr = new PlayerStatsRow(pair.Value);
            if (_playersToHighlight.Contains(psr.ID))
            {
                psr.Highlight = true;
            }
            psrObservable.Add(psr);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            _playersToHighlight.Clear();
            _teamsToHighlight.Clear();

            calculateAdvancedStats(MainWindow.BSHist, new List<BoxScoreEntry>());

            tbcAdv.SelectedItem = tabPlayerStats;
        }

        private void dgvBoxScores_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        private void btnLoadFilters_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                      {
                          InitialDirectory = Path.GetFullPath(_folder),
                          Title = "Select the filters file you want to load...",
                          Filter = "NST Advanced Stats Filters (*.naf)|*.naf",
                          DefaultExt = "naf"
                      };

            ofd.ShowDialog();

            if (String.IsNullOrWhiteSpace(ofd.FileName))
                return;

            if (lstTotals.Items.Count > 0)
            {
                MessageBoxResult mbr = MessageBox.Show("Do you want to clear the current stat filters before loading the new ones?",
                                                       "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel)
                    return;

                if (mbr == MessageBoxResult.Yes)
                {
                    _filters.Clear();
                    lstTotals.Items.Clear();
                }
            }

            string[] lines = File.ReadAllLines(ofd.FileName);
            foreach (var item in lines)
            {
                int id = Convert.ToInt32(item.Substring(3).Split(')')[0]);
                string[] criterion = item.Split(':')[1].Trim().Split(' ');
                string parameter = criterion[0];
                string op = criterion[1];
                string par2;
                string val;
                try
                {
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
                    Convert.ToDouble(criterion[2]);
// ReSharper restore ReturnValueOfPureMethodIsNotUsed
                    par2 = "";
                    val = criterion[2];
                }
                catch
                {
                    par2 = criterion[2];
                    val = "";
                }

                KeyValuePair<Selection, List<Filter>> filter;
                if (item.Substring(2, 1) == "T")
                {
                    try
                    {
                        filter = _filters.Single(f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id);
                    }
                    catch (InvalidOperationException)
                    {
                        _filters.Add(new Selection(SelectionType.Team, id), new List<Filter>());
                        filter = _filters.Single(f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id);
                    }
                }
                else
                {
                    try
                    {
                        filter = _filters.Single(f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                    }
                    catch (InvalidOperationException)
                    {
                        _filters.Add(new Selection(SelectionType.Player, id), new List<Filter>());
                        filter = _filters.Single(f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                    }
                }
                filter.Value.Add(new Filter(parameter, op, par2, val));
            }

            populateTotalsList();
        }

        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.Items.Count == 0)
                return;

            var sfd = new SaveFileDialog
                      {
                          InitialDirectory = Path.GetFullPath(_folder),
                          Title = "Save Filters As",
                          Filter = "NST Advanced Stats Filters (*.naf)|*.naf",
                          DefaultExt = "naf"
                      };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
                return;

            try
            {
                File.WriteAllLines(sfd.FileName, lstTotals.Items.Cast<string>().ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't save filters file.\n\n" + ex.Message);
            }
        }

        private void cmbTotalsPar2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTotalsPar2.SelectedIndex == -1)
                return;

            if (String.IsNullOrWhiteSpace(cmbTotalsPar2.SelectedItem.ToString()))
            {
                txtTotalsVal.IsEnabled = true;
            }
            else
            {
                txtTotalsVal.Text = "";
                txtTotalsVal.IsEnabled = false;
            }
        }

        #region Nested type: Filter

        public struct Filter
        {
            public string Operator;
            public string Parameter;
            public string Parameter2;
            public string Value;

            public Filter(string parameter, string oper, string parameter2, string value)
            {
                Parameter = parameter;
                Operator = oper;
                Parameter2 = parameter2;
                Value = value;
            }
        }

        #endregion

        #region Nested type: Selection

        public struct Selection
        {
            public int ID;
            public SelectionType SelectionType;

            public Selection(SelectionType selectionType, int id)
            {
                SelectionType = selectionType;
                ID = id;
            }
        };

        #endregion
    }
}