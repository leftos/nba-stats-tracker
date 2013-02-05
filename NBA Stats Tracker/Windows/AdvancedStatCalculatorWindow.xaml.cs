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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

namespace NBA_Stats_Tracker.Windows
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

        private readonly List<string> NumericOptions = new List<string> {"<", "<=", "=", ">=", ">"};
        private readonly List<string> Positions = new List<string> {"Any", "None", "PG", "SG", "SF", "PF", "C"};

        private readonly List<string> Totals = new List<string>
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

        private readonly Dictionary<Selection, List<Filter>> filters = new Dictionary<Selection, List<Filter>>();
        private ObservableCollection<KeyValuePair<int, string>> _playersList;

        private bool changingTimeframe;
        private int curSeason;
        private bool loading;

        public AdvancedStatCalculatorWindow()
        {
            InitializeComponent();
        }

        private ObservableCollection<KeyValuePair<int, string>> PlayersList
        {
            get { return _playersList; }
            set
            {
                _playersList = value;
                OnPropertyChanged("PlayersList");
            }
        }

        private ObservableCollection<PlayerStatsRow> psrList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrList { get; set; }
        protected ObservableCollection<TeamStatsRow> tsrList_not { get; set; }

        protected ObservableCollection<PlayerStatsRow> psrList_not { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.tf.EndDate;
            dtpStart.SelectedDate = MainWindow.tf.StartDate;
            cmbSeasonNum.ItemsSource = MainWindow.mwInstance.cmbSeasonNum.ItemsSource;
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.tf.SeasonNum);
            curSeason = MainWindow.tf.SeasonNum;
            cmbTFSeason.ItemsSource = MainWindow.mwInstance.cmbSeasonNum.ItemsSource;
            cmbTFSeason.SelectedItem = cmbSeasonNum.SelectedItem;
            if (MainWindow.tf.isBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            changingTimeframe = false;

            loading = true;
            cmbPosition1Filter.ItemsSource = Positions;
            cmbPosition2Filter.ItemsSource = Positions;
            cmbPosition1Filter.SelectedIndex = 0;
            cmbPosition2Filter.SelectedIndex = 0;
            formatTotalsForTeams();
            cmbTotalsOp.ItemsSource = NumericOptions;
            cmbTotalsPar.SelectedIndex = 0;
            cmbTotalsOp.SelectedIndex = 3;
            cmbTotalsPar2.SelectedIndex = 0;
            loading = false;

            PopulateTeamsCombo();
        }

        private void formatTotalsForTeams()
        {
            var newTotals = Totals.ToList();
            for (int i = 0; i < newTotals.Count; i++)
            {
                if (newTotals[i] == "PTS (PF)")
                {
                    newTotals[i] = "PF";
                    break;
                }
            }
            Totals.Skip(3).ToList().ForEach(item => newTotals.Add("Opp" + item));
            cmbTotalsPar.ItemsSource = newTotals;
            cmbTotalsPar2.ItemsSource = newTotals;
        }

        private void formatTotalsForPlayers()
        {
            var newTotals = Totals.ToList(); for (int i = 0; i < newTotals.Count; i++)
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

        private void PopulateTeamsCombo()
        {
            cmbTeamFilter.Items.Clear();
            cmbSelectedTeam.Items.Clear();
            var teams = new List<string>();
            MainWindow.tst.Values.ToList().ForEach(ts => teams.Add(ts.displayName));
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
            PopulatePlayersCombo();
        }

        private void PopulatePlayersCombo()
        {
            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            List<PlayerStats> list = MainWindow.pst.Values.ToList();
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
                        ps.TeamF == Misc.GetTeamIDFromDisplayName(MainWindow.tst, cmbTeamFilter.SelectedItem.ToString()) && ps.isActive)
                        .ToList();
            }
            else if (chkIsActive.IsChecked == false)
            {
                list = list.Where(ps => !ps.isActive).ToList();
            }
            list.ForEach(
                ps =>
                PlayersList.Add(new KeyValuePair<int, string>(ps.ID,
                                                              String.Format("{0}, {1} ({2})", ps.LastName, ps.FirstName,
                                                                            ps.Position1.ToString()))));

            cmbSelectedPlayer.ItemsSource = PlayersList;
        }

        private void cmbPosition1Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loading)
                PopulatePlayersCombo();
        }

        private void cmbPosition2Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!loading)
                PopulatePlayersCombo();
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (cmbSeasonNum.SelectedIndex == -1)
                    return;

                cmbTFSeason.SelectedItem = cmbSeasonNum.SelectedItem;

                curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

                if (MainWindow.tf.SeasonNum != curSeason || MainWindow.tf.isBetween)
                {
                    MainWindow.tf = new Timeframe(curSeason);
                    MainWindow.ChangeSeason(curSeason);
                    SQLiteIO.LoadSeason();
                }

                PopulateTeamsCombo();
                changingTimeframe = false;
            }
        }

        private void cmbTFSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (cmbTFSeason.SelectedIndex == -1)
                    return;

                cmbSeasonNum.SelectedItem = cmbTFSeason.SelectedItem;
                rbStatsAllTime.IsChecked = true;

                curSeason = ((KeyValuePair<int, string>) (((cmbTFSeason)).SelectedItem)).Key;

                if (MainWindow.tf.SeasonNum != curSeason || MainWindow.tf.isBetween)
                {
                    MainWindow.tf = new Timeframe(curSeason);
                    MainWindow.ChangeSeason(curSeason);
                    SQLiteIO.LoadSeason();
                }

                PopulateTeamsCombo();

                changingTimeframe = false;
            }
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.tf = new Timeframe(curSeason);
            if (!changingTimeframe)
            {
                MainWindow.UpdateAllData();
                cmbSeasonNum_SelectionChanged(null, null);
            }
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            if (!changingTimeframe)
            {
                MainWindow.UpdateAllData();
                PopulateTeamsCombo();
            }
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingTimeframe)
                return;
            changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            changingTimeframe = false;
            MainWindow.UpdateAllData();
            PopulateTeamsCombo();
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingTimeframe)
                return;
            changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            changingTimeframe = false;
            MainWindow.UpdateAllData();
            PopulateTeamsCombo();
        }

        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault())
                cmbTeamFilter.SelectedIndex = 0;
            else
                cmbTeamFilter.SelectedIndex = -1;
            PopulatePlayersCombo();
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

                    int teamID = Misc.GetTeamIDFromDisplayName(MainWindow.tst, cmbSelectedTeam.SelectedItem.ToString());
                    KeyValuePair<Selection, List<Filter>> filter;
                    try
                    {
                        filter = filters.Single(fil => fil.Key.SelectionType == SelectionType.Team && fil.Key.ID == teamID);
                    }
                    catch
                    {
                        filters.Add(new Selection(SelectionType.Team, teamID), new List<Filter>());
                        filter = filters.Single(fil => fil.Key.SelectionType == SelectionType.Team && fil.Key.ID == teamID);
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
                    filter = filters.Single(fil => fil.Key.SelectionType == SelectionType.Player && fil.Key.ID == playerID);
                }
                catch
                {
                    filters.Add(new Selection(SelectionType.Player, playerID), new List<Filter>());
                    filter = filters.Single(fil => fil.Key.SelectionType == SelectionType.Player && fil.Key.ID == playerID);
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
            PopulateTotalsList();
        }

        private void PopulateTotalsList()
        {
            lstTotals.Items.Clear();
            foreach (var filter in filters)
            {
                string s;
                if (filter.Key.SelectionType == SelectionType.Team)
                {
                    s = string.Format("(#T{0}) {1}: ", filter.Key.ID, MainWindow.tst.Values.Single(ts => ts.ID == filter.Key.ID).displayName);
                }
                else
                {
                    PlayerStats player = MainWindow.pst.Values.Single(ps => ps.ID == filter.Key.ID);
                    string teamName;
                    try
                    {
                        teamName = MainWindow.tst[player.TeamF].displayName;
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
                var criterion = item.Split(':')[1].Trim().Split(' ');
                string parameter = criterion[0];
                string op = criterion[1];
                KeyValuePair<Selection, List<Filter>> filter;
                if (item.Substring(2, 1) == "T")
                {
                    filter = filters.Single(f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id);
                }
                else
                {
                    filter = filters.Single(f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                }
                filter.Value.Remove(filter.Value.Single(o => o.Parameter == parameter && o.Operator == op));
            }

            PopulateTotalsList();
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

        private List<int> playersToHighlight = new List<int>(); 
        private List<int> teamsToHighlight = new List<int>();
        private readonly string folder = App.AppDocsPath + @"\Advanced Stats Filters";

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            playersToHighlight.Clear();
            teamsToHighlight.Clear();

            var bsToCalculate = new List<BoxScoreEntry>();
            var notBsToCalculate = new List<BoxScoreEntry>();
            foreach (var bse in MainWindow.bshist)
            {
                bool keep = true;
                foreach (var filter in filters)
                {
                    if (filter.Key.SelectionType == SelectionType.Team)
                    {
                        if (!teamsToHighlight.Contains(filter.Key.ID))
                        {
                            teamsToHighlight.Add(filter.Key.ID);
                        }
                        //string teamName = MainWindow.TeamOrder.Single(pair => pair.Value == filter.Key.ID).Key;
                        if (bse.bs.Team1ID != filter.Key.ID && bse.bs.Team2ID != filter.Key.ID)
                        {
                            keep = false;
                            break;
                        }
                        string p = bse.bs.Team1ID == filter.Key.ID ? "1" : "2";
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
                                                                                   bse.bs.GetType()
                                                                                      .GetProperty(parameter)
                                                                                      .GetValue(bse.bs, null), option.Operator, option.Value));
                                }
                                else
                                {
                                    var par1 = parameter.Replace("%", "M");
                                    var par2 = parameter.Replace("%", "A");
                                    ige =
                                        context.CompileGeneric<bool>(
                                            string.Format("(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                          bse.bs.GetType().GetProperty(par1).GetValue(bse.bs, null),
                                                          bse.bs.GetType().GetProperty(par2).GetValue(bse.bs, null), option.Operator,
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
                                            context.CompileGeneric<bool>(string.Format("{0} {1} {2}",
                                                                                       getValue(bse, parameter), option.Operator,
                                                                                       getValue(bse, parameter2)));
                                    }
                                    else
                                    {
                                        var par2Part1 = parameter2.Replace("%", "M");
                                        var par2Part2 = parameter2.Replace("%", "A");
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
                                        var par1Part1 = parameter.Replace("%", "M");
                                        var par1Part2 = parameter.Replace("%", "A");
                                        ige =
                                            context.CompileGeneric<bool>(
                                                string.Format("(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                              getValue(bse, par1Part1), getValue(bse, par1Part2), option.Operator,
                                                              getValue(bse, parameter2)));
                                    }
                                    else
                                    {
                                        var par1Part1 = parameter.Replace("%", "M");
                                        var par1Part2 = parameter.Replace("%", "A"); 
                                        var par2Part1 = parameter2.Replace("%", "M");
                                        var par2Part2 = parameter2.Replace("%", "A");
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
                        if (!playersToHighlight.Contains(filter.Key.ID))
                        {
                            playersToHighlight.Add(filter.Key.ID);
                        }
                        PlayerBoxScore pbs;
                        try
                        {
                            pbs = bse.pbsList.Single(pbs1 => pbs1.PlayerID == filter.Key.ID);
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
                                                                               pbs.GetType()
                                                                                  .GetProperty(parameter)
                                                                                  .GetValue(pbs, null), option.Operator, option.Value));
                            }
                            else
                            {
                                var par1 = parameter.Replace("%", "M");
                                var par2 = parameter.Replace("%", "A");
                                ige =
                                    context.CompileGeneric<bool>(
                                        string.Format("(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                      pbs.GetType().GetProperty(par1).GetValue(pbs, null),
                                                      pbs.GetType().GetProperty(par2).GetValue(pbs, null), option.Operator,
                                                      option.Value));
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

            CalculateAdvancedStats(bsToCalculate, notBsToCalculate);

            tbcAdv.SelectedItem = tabPlayerStats;
        }

        private static object getValue(BoxScoreEntry bse, string parameter)
        {
            return bse.bs.GetType()
                      .GetProperty(parameter)
                      .GetValue(bse.bs, null);
        }

        private void CalculateAdvancedStats(List<BoxScoreEntry> bsToCalculate, List<BoxScoreEntry> notBsToCalculate)
        {
            var advtst = new Dictionary<int, TeamStats>();
            var advtstopp = new Dictionary<int, TeamStats>();
            var advpst = new Dictionary<int, PlayerStats>();
            var advPPtst = new Dictionary<int, TeamStats>();
            var advPPtstopp = new Dictionary<int, TeamStats>();

            var advtst_not = new Dictionary<int, TeamStats>();
            var advtstopp_not = new Dictionary<int, TeamStats>();
            var advpst_not = new Dictionary<int, PlayerStats>();
            var advPPtst_not = new Dictionary<int, TeamStats>();
            var advPPtstopp_not = new Dictionary<int, TeamStats>();

            foreach (var bse in bsToCalculate)
            {
                int team1ID = bse.bs.Team1ID;
                int team2ID = bse.bs.Team2ID;
                TeamBoxScore bs = bse.bs;
                if (!advtst.ContainsKey(team1ID))
                {
                    advtst.Add(team1ID,
                               new TeamStats {ID = team1ID, name = MainWindow.tst[team1ID].name, displayName = MainWindow.tst[team1ID].displayName});
                    advtstopp.Add(team1ID,
                                  new TeamStats
                                  {
                                      ID = team1ID,
                                      name = MainWindow.tst[team1ID].name,
                                      displayName = MainWindow.tst[team1ID].displayName
                                  });
                }
                TeamStats ts1 = advtst[team1ID];
                TeamStats ts1opp = advtstopp[team1ID];
                if (!advtst.ContainsKey(team2ID))
                {
                    //advTeamOrder.Add(MainWindow.tst[team2ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                    advtst.Add(team2ID,
                               new TeamStats {ID = team2ID, name = MainWindow.tst[team2ID].name, displayName = MainWindow.tst[team2ID].displayName});
                    advtstopp.Add(team2ID,
                                  new TeamStats
                                  {
                                      ID = team2ID,
                                      name = MainWindow.tst[team2ID].name,
                                      displayName = MainWindow.tst[team2ID].displayName
                                  });
                }
                TeamStats ts2 = advtst[team2ID];
                TeamStats ts2opp = advtstopp[team2ID];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts2opp, ref ts1opp, true);
                foreach (var pbs in bse.pbsList)
                {
                    if (advpst.All(pair => pair.Key != pbs.PlayerID))
                    {
                        advpst.Add(pbs.PlayerID, MainWindow.pst[pbs.PlayerID].DeepClone());
                        advpst[pbs.PlayerID].ResetStats();
                    }
                    advpst[pbs.PlayerID].AddBoxScore(pbs, false);

                    var teamID = pbs.TeamID;
                    if (!advPPtst.ContainsKey(pbs.PlayerID))
                    {
                        advPPtst.Add(pbs.PlayerID,
                                     new TeamStats
                                     {
                                         ID = teamID,
                                         name = MainWindow.tst[teamID].name,
                                         displayName = MainWindow.tst[teamID].displayName
                                     });
                    }
                    if (!advPPtstopp.ContainsKey(pbs.PlayerID))
                    {
                        advPPtstopp.Add(pbs.PlayerID,
                                        new TeamStats
                                        {
                                            ID = teamID,
                                            name = MainWindow.tst[teamID].name,
                                            displayName = MainWindow.tst[teamID].displayName
                                        });
                    }
                    TeamStats ts = advPPtst[pbs.PlayerID];
                    TeamStats tsopp = advPPtstopp[pbs.PlayerID];
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
                int team1ID = bse.bs.Team1ID;
                int team2ID = bse.bs.Team2ID;
                TeamBoxScore bs = bse.bs;
                if (!advtst_not.ContainsKey(team1ID))
                {
                    //advTeamOrder.Add(MainWindow.tst[team1ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                    advtst_not.Add(team1ID,
                                   new TeamStats
                                   {
                                       ID = team1ID,
                                       name = MainWindow.tst[team1ID].name,
                                       displayName = MainWindow.tst[team1ID].displayName
                                   });
                    advtstopp_not.Add(team1ID,
                                      new TeamStats
                                      {
                                          ID = team1ID,
                                          name = MainWindow.tst[team1ID].name,
                                          displayName = MainWindow.tst[team1ID].displayName
                                      });
                }
                TeamStats ts1 = advtst_not[team1ID];
                TeamStats ts1opp = advtstopp_not[team1ID];
                if (!advtst_not.ContainsKey(team2ID))
                {
                    //advTeamOrder.Add(MainWindow.tst[team2ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                    advtst_not.Add(team2ID,
                                   new TeamStats
                                   {
                                       ID = team2ID,
                                       name = MainWindow.tst[team2ID].name,
                                       displayName = MainWindow.tst[team2ID].displayName
                                   });
                    advtstopp_not.Add(team2ID,
                                      new TeamStats
                                      {
                                          ID = team2ID,
                                          name = MainWindow.tst[team2ID].name,
                                          displayName = MainWindow.tst[team2ID].displayName
                                      });
                }
                TeamStats ts2 = advtst_not[team2ID];
                TeamStats ts2opp = advtstopp_not[team2ID];
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts2opp, ref ts1opp, true);
                foreach (var pbs in bse.pbsList)
                {
                    if (pbs.isOut)
                        continue;

                    if (advpst_not.All(pair => pair.Key != pbs.PlayerID))
                    {
                        advpst_not.Add(pbs.PlayerID, MainWindow.pst[pbs.PlayerID].DeepClone());
                        advpst_not[pbs.PlayerID].ResetStats();
                    }
                    advpst_not[pbs.PlayerID].AddBoxScore(pbs, false);

                    var teamID = pbs.TeamID;
                    if (!advPPtst_not.ContainsKey(pbs.PlayerID))
                    {
                        advPPtst_not.Add(pbs.PlayerID,
                                         new TeamStats
                                         {
                                             ID = teamID,
                                             name = MainWindow.tst[teamID].name,
                                             displayName = MainWindow.tst[teamID].displayName
                                         });
                    }
                    if (!advPPtstopp_not.ContainsKey(pbs.PlayerID))
                    {
                        advPPtstopp_not.Add(pbs.PlayerID,
                                            new TeamStats
                                            {
                                                ID = teamID,
                                                name = MainWindow.tst[teamID].name,
                                                displayName = MainWindow.tst[teamID].displayName
                                            });
                    }
                    TeamStats ts = advPPtst_not[pbs.PlayerID];
                    TeamStats tsopp = advPPtstopp_not[pbs.PlayerID];
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

            TeamStats.CalculateAllMetrics(ref advtst, advtstopp, false);
            TeamStats.CalculateAllMetrics(ref advtstopp, advtst, false);
            PlayerStats.CalculateAllMetrics(ref advpst, advPPtst, advPPtstopp, teamsPerPlayer: true);

            psrList = new ObservableCollection<PlayerStatsRow>();
            var playerStatsRows = psrList;
            advpst.ToList().ForEach(item => highlightAndAddPlayers(item, ref playerStatsRows));
            tsrList = new ObservableCollection<TeamStatsRow>();
            var teamStatsRows = tsrList;
            advtst.ToList().ForEach(item => highlightAndAddTeams(item, ref teamStatsRows));
            tsrOppList = new ObservableCollection<TeamStatsRow>();
            var oppStatsRows = tsrOppList;
            advtstopp.ToList().ForEach(item => highlightAndAddTeams(item, ref oppStatsRows));

            dgvTeamStats.ItemsSource = tsrList;
            dgvOppStats.ItemsSource = tsrOppList;
            dgvPlayerStats.ItemsSource = psrList;

            TeamStats.CalculateAllMetrics(ref advtst_not, advtstopp_not, false);
            TeamStats.CalculateAllMetrics(ref advtstopp_not, advtst_not, false);
            PlayerStats.CalculateAllMetrics(ref advpst_not, advPPtst_not, advPPtstopp_not, teamsPerPlayer: true);

            psrList_not = new ObservableCollection<PlayerStatsRow>();
            var playerStatsRows_not = psrList_not;
            advpst_not.ToList().ForEach(item => highlightAndAddPlayers(item, ref playerStatsRows_not));
            tsrList_not = new ObservableCollection<TeamStatsRow>();
            var teamStatsRows_not = tsrList_not;
            advtst_not.ToList().ForEach(item => highlightAndAddTeams(item, ref teamStatsRows_not));
            tsrOppList_not = new ObservableCollection<TeamStatsRow>();
            var oppStatsRows_not = tsrOppList_not;
            advtstopp_not.ToList().ForEach(item => highlightAndAddTeams(item, ref oppStatsRows_not));

            dgvTeamStatsUnmet.ItemsSource = tsrList_not;
            dgvOppStatsUnmet.ItemsSource = tsrOppList_not;
            dgvPlayerStatsUnmet.ItemsSource = psrList_not;

            bsToCalculate.ForEach(bse => bse.bs.PrepareForDisplay(advtst));
            notBsToCalculate.ForEach(bse => bse.bs.PrepareForDisplay(advtst_not));
            dgvBoxScores.ItemsSource = bsToCalculate.Select(bse => bse.bs);
            dgvBoxScoresUnmet.ItemsSource = notBsToCalculate.Select(bse => bse.bs);
        }

        protected ObservableCollection<TeamStatsRow> tsrOppList { get; set; }
        protected ObservableCollection<TeamStatsRow> tsrOppList_not { get; set; }

        private void highlightAndAddTeams(KeyValuePair<int, TeamStats> pair, ref ObservableCollection<TeamStatsRow> tsrObservable)
        {
            var tsr = new TeamStatsRow(pair.Value);
            if (teamsToHighlight.Contains(tsr.ID))
            {
                tsr.Highlight = true;
            }
            tsrObservable.Add(tsr);
        }

        private void highlightAndAddPlayers(KeyValuePair<int, PlayerStats> pair, ref ObservableCollection<PlayerStatsRow> psrObservable)
        {
            var psr = new PlayerStatsRow(pair.Value);
            if (playersToHighlight.Contains(psr.ID))
            {
                psr.Highlight = true;
            }
            psrObservable.Add(psr);
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

            public Selection(SelectionType selectionType, int ID)
            {
                SelectionType = selectionType;
                this.ID = ID;
            }
        };

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            playersToHighlight.Clear();
            teamsToHighlight.Clear();

            CalculateAdvancedStats(MainWindow.bshist, new List<BoxScoreEntry>());

            tbcAdv.SelectedItem = tabPlayerStats;
        }

        private void dgvBoxScores_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        private void btnLoadFilters_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
                                 {
                                     InitialDirectory = Path.GetFullPath(folder),
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
                    filters.Clear();
                    lstTotals.Items.Clear();
                }
            }

            string[] lines = File.ReadAllLines(ofd.FileName);
            foreach (string item in lines)
            {
                int id = Convert.ToInt32(item.Substring(3).Split(')')[0]);
                var criterion = item.Split(':')[1].Trim().Split(' ');
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
                        filter = filters.Single(f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id);
                    }
                    catch (InvalidOperationException)
                    {
                        filters.Add(new Selection(SelectionType.Team, id), new List<Filter>());
                        filter = filters.Single(f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id);
                    }
                }
                else
                {
                    try
                    {
                        filter = filters.Single(f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                    }
                    catch (InvalidOperationException)
                    {
                        filters.Add(new Selection(SelectionType.Player, id), new List<Filter>());
                        filter = filters.Single(f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                    }
                }
                filter.Value.Add(new Filter(parameter, op, par2, val));
            }

            PopulateTotalsList();
        }

        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.Items.Count == 0)
                return;

            SaveFileDialog sfd = new SaveFileDialog
                                 {
                                     InitialDirectory = Path.GetFullPath(folder),
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
    }
}