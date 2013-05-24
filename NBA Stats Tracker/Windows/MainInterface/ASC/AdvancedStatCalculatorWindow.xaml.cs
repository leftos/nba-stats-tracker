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

namespace NBA_Stats_Tracker.Windows.MainInterface.ASC
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    using Ciloci.Flee;

    using LeftosCommonLibrary;

    using Microsoft.Win32;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;
    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.EventHandlers;
    using NBA_Stats_Tracker.Helper.Miscellaneous;
    using NBA_Stats_Tracker.Windows.MainInterface.BoxScores;

    #endregion

    /// <summary>Interaction logic for AdvancedStatCalculatorWindow.xaml</summary>
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

        private readonly List<string> _numericOptions = new List<string> { "<", "<=", "=", ">=", ">" };
        private readonly List<int> _playersToHighlight = new List<int>();
        private readonly List<string> _positions = new List<string> { "Any", "None", "PG", "SG", "SF", "PF", "C" };
        private readonly List<int> _teamsToHighlight = new List<int>();

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
        private double _defaultPBPHeight;
        private bool _loading;

        public AdvancedStatCalculatorWindow()
        {
            InitializeComponent();
            PlayersOnTheFloor = new List<int>();
        }

        private ObservableCollection<KeyValuePair<int, string>> playersList { get; set; }
        private ObservableCollection<PlayerStatsRow> psrList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrList_Not { get; set; }
        private ObservableCollection<PlayerStatsRow> psrList_Not { get; set; }
        private ObservableCollection<TeamStatsRow> tsrOppList { get; set; }
        private ObservableCollection<TeamStatsRow> tsrOppList_Not { get; set; }

        public static List<int> PlayersOnTheFloor { get; set; }

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

            var eventTypes = PlayByPlayEntry.EventTypes.Values.ToList();
            eventTypes.Insert(0, "Any");
            cmbEventType.ItemsSource = eventTypes;
            cmbEventType.SelectedIndex = 0;

            var shotOrigins = ShotEntry.ShotOrigins.Values.ToList();
            shotOrigins.Insert(0, "Any");
            cmbShotOrigin.ItemsSource = shotOrigins;
            cmbShotOrigin.SelectedIndex = 0;

            var shotTypes = ShotEntry.ShotTypes.Values.ToList();
            shotTypes.Insert(0, "Any");
            cmbShotType.ItemsSource = shotTypes;
            cmbShotType.SelectedIndex = 0;

            var pbpNumericOptions = _numericOptions.ToList();
            pbpNumericOptions.Insert(0, "Any");
            cmbPeriodOp.ItemsSource = pbpNumericOptions;
            cmbPeriodOp.SelectedIndex = 0;
            cmbTimeLeftOp.ItemsSource = pbpNumericOptions;
            cmbTimeLeftOp.SelectedIndex = 0;
            cmbShotClockOp.ItemsSource = pbpNumericOptions;
            cmbShotClockOp.SelectedIndex = 0;

            chkShotIsMade.IsChecked = null;
            chkShotIsAssisted.IsChecked = null;
            _loading = false;

            _defaultPBPHeight = gdrPlayByPlay.Height.Value;

            populateTeamsCombo();

            Height = Tools.GetRegistrySetting("ASCHeight", (int) Height);
            Width = Tools.GetRegistrySetting("ASCWidth", (int) Width);
            Top = Tools.GetRegistrySetting("ASCY", (int) Top);
            Left = Tools.GetRegistrySetting("ASCX", (int) Left);
        }

        private void formatTotalsForTeams()
        {
            var newTotals = _totals.ToList();
            for (var i = 0; i < newTotals.Count; i++)
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
            var newTotals = _totals.ToList();
            for (var i = 0; i < newTotals.Count; i++)
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
            cmbTeamFilter.Items.Add("Any");
            teams.ForEach(
                delegate(string i)
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
            if (!_loading)
            {
                if (cmbTeamFilter.SelectedIndex != -1 && cmbTeamFilter.SelectedItem.ToString() != "Any")
                {
                    chkIsActive.IsChecked = true;
                }
                populatePlayersCombo();
            }
        }

        private void populatePlayersCombo()
        {
            playersList = new ObservableCollection<KeyValuePair<int, string>>();
            var list = MainWindow.PST.Values.ToList();
            if (cmbPosition1Filter.SelectedItem != null && cmbPosition1Filter.SelectedItem.ToString() != "Any")
            {
                list = list.Where(ps => ps.Position1.ToString() == cmbPosition1Filter.SelectedItem.ToString()).ToList();
            }
            if (cmbPosition2Filter.SelectedItem != null && cmbPosition2Filter.SelectedItem.ToString() != "Any")
            {
                list = list.Where(ps => ps.Position2.ToString() == cmbPosition2Filter.SelectedItem.ToString()).ToList();
            }
            list = list.OrderBy(ps => ps.LastName).ThenBy(ps => ps.FirstName).ToList();
            if (chkIsActive.IsChecked.GetValueOrDefault() && cmbTeamFilter.SelectedItem.ToString() != "Any")
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
                playersList.Add(
                    new KeyValuePair<int, string>(
                    ps.ID,
                    String.Format(
                        "{0}, {1} ({2} - {3})",
                        ps.LastName,
                        ps.FirstName,
                        ps.Position1.ToString(),
                        ps.IsActive ? MainWindow.TST[ps.TeamF].DisplayName : "Free Agent"))));

            cmbSelectedPlayer.ItemsSource = playersList;
            var playersListPBP = playersList.ToList();
            playersListPBP.Insert(0, new KeyValuePair<int, string>(-1, "Any"));
            cmbPlayer1.ItemsSource = playersListPBP;
            cmbPlayer2.ItemsSource = playersListPBP;
            cmbPlayer1.SelectedIndex = 0;
            cmbPlayer2.SelectedIndex = 0;
        }

        private void cmbPosition1Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading)
            {
                populatePlayersCombo();
            }
        }

        private void cmbPosition2Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loading)
            {
                populatePlayersCombo();
            }
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (cmbSeasonNum.SelectedIndex == -1)
                {
                    return;
                }

                cmbTFSeason.SelectedItem = cmbSeasonNum.SelectedItem;

                _curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

                if (MainWindow.Tf.SeasonNum != _curSeason || MainWindow.Tf.IsBetween)
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    MainWindow.ChangeSeason(_curSeason);
                    updateData();
                }
            }
        }

        private void updateData()
        {
            IsEnabled = false;
            Task.Factory.StartNew(() => MainWindow.UpdateAllData(true))
                .FailFastOnException(MainWindow.MWInstance.UIScheduler)
                .ContinueWith(t => refresh(), MainWindow.MWInstance.UIScheduler)
                .FailFastOnException(MainWindow.MWInstance.UIScheduler);
        }

        private void refresh()
        {
            populateTeamsCombo();
            _changingTimeframe = false;

            MainWindow.MWInstance.StopProgressWatchTimer();
            IsEnabled = true;
        }

        private void cmbTFSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (cmbTFSeason.SelectedIndex == -1)
                {
                    return;
                }

                cmbSeasonNum.SelectedItem = cmbTFSeason.SelectedItem;
                rbStatsAllTime.IsChecked = true;

                _curSeason = ((KeyValuePair<int, string>) (((cmbTFSeason)).SelectedItem)).Key;

                if (MainWindow.Tf.SeasonNum != _curSeason || MainWindow.Tf.IsBetween)
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    MainWindow.ChangeSeason(_curSeason);
                    updateData();
                }

                _changingTimeframe = false;
            }
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                cmbSeasonNum_SelectionChanged(null, null);
            }
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            if (!_changingTimeframe)
            {
                updateData();
            }
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
            {
                return;
            }
            _changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            updateData();
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingTimeframe)
            {
                return;
            }
            _changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            updateData();
        }

        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault())
            {
                cmbTeamFilter.SelectedIndex = 0;
            }
            else
            {
                cmbTeamFilter.SelectedIndex = -1;
            }
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
                    {
                        return;
                    }

                    if (!String.IsNullOrWhiteSpace(cmbTotalsPar2.SelectedItem.ToString()))
                    {
                        txtTotalsVal.Text = "";
                    }
                    else
                    {
                        if (String.IsNullOrWhiteSpace(txtTotalsVal.Text))
                        {
                            return;
                        }
                    }

                    var teamID = Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbSelectedTeam.SelectedItem.ToString());
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
                                o.Parameter1 == cmbTotalsPar.SelectedItem.ToString()
                                && o.Operator == cmbTotalsOp.SelectedItem.ToString()));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the properties of the one being inserted.");
                    }
                    finally
                    {
                        filter.Value.Add(
                            new Filter(
                                cmbTotalsPar.SelectedItem.ToString(),
                                cmbTotalsOp.SelectedItem.ToString(),
                                cmbTotalsPar2.SelectedItem.ToString(),
                                txtTotalsVal.Text));
                    }
                }
            }
            else
            {
                if (cmbTotalsPar.SelectedIndex <= 0)
                {
                    if (chkIsStarter.IsChecked == null && chkIsInjured.IsChecked == null && chkIsOut.IsChecked == null)
                    {
                        return;
                    }
                }
                else
                {
                    if (cmbTotalsOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtTotalsVal.Text))
                    {
                        return;
                    }
                }

                var playerID = ((KeyValuePair<int, string>) (((cmbSelectedPlayer)).SelectedItem)).Key;
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
                    {
                        return;
                    }

                    try
                    {
                        filter.Value.Remove(
                            filter.Value.Single(
                                o =>
                                o.Parameter1 == cmbTotalsPar.SelectedItem.ToString()
                                && o.Operator == cmbTotalsOp.SelectedItem.ToString()));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the properties of the one being inserted.");
                    }
                    finally
                    {
                        filter.Value.Add(
                            new Filter(
                                cmbTotalsPar.SelectedItem.ToString(),
                                cmbTotalsOp.SelectedItem.ToString(),
                                cmbTotalsPar2.SelectedItem.ToString(),
                                txtTotalsVal.Text));
                    }
                }
                else
                {
                    try
                    {
                        filter.Value.Remove(filter.Value.Single(f => f.Parameter1 == "IsStarter"));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the property isStarter.");
                    }
                    finally
                    {
                        if (chkIsStarter.IsChecked != null)
                        {
                            var value = chkIsStarter.IsChecked == true ? "True" : "False";
                            filter.Value.Add(new Filter("IsStarter", "=", "", value));
                        }
                    }

                    try
                    {
                        filter.Value.Remove(filter.Value.Single(f => f.Parameter1 == "PlayedInjured"));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the property isInjured.");
                    }
                    finally
                    {
                        if (chkIsInjured.IsChecked != null)
                        {
                            var value = chkIsInjured.IsChecked == true ? "True" : "False";
                            filter.Value.Add(new Filter("PlayedInjured", "=", "", value));
                        }
                    }

                    try
                    {
                        filter.Value.Remove(filter.Value.Single(f => f.Parameter1 == "IsOut"));
                    }
                    catch
                    {
                        Console.WriteLine("Didn't find a previous filter matching the property isOut.");
                    }
                    finally
                    {
                        if (chkIsOut.IsChecked != null)
                        {
                            var value = chkIsOut.IsChecked == true ? "True" : "False";
                            filter.Value.Add(new Filter("IsOut", "=", "", value));
                        }
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
                    s = string.Format(
                        "(#T{0}) {1}: ", filter.Key.ID, MainWindow.TST.Values.Single(ts => ts.ID == filter.Key.ID).DisplayName);
                }
                else
                {
                    var player = MainWindow.PST.Values.Single(ps => ps.ID == filter.Key.ID);
                    string teamName;
                    try
                    {
                        teamName = MainWindow.TST[player.TeamF].DisplayName;
                    }
                    catch (Exception)
                    {
                        teamName = "Inactive";
                    }
                    s = String.Format(
                        "(#P{4}) {0}, {1} ({2} - {3}): ", player.LastName, player.FirstName, player.Position1, teamName, player.ID);
                }
                foreach (var option in filter.Value)
                {
                    var s2 = s
                             + String.Format(
                                 "{0} {1} {2}",
                                 option.Parameter1,
                                 option.Operator,
                                 String.IsNullOrWhiteSpace(option.Parameter2) ? option.Value : option.Parameter2);
                    lstTotals.Items.Add(s2);
                }
            }
        }

        private void cmbSelectedPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectedPlayer.SelectedIndex == -1)
            {
                return;
            }

            cmbSelectedTeam.SelectedIndex = -1;
            formatTotalsForPlayers();
        }

        private void cmbSelectedTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSelectedTeam.SelectedIndex == -1)
            {
                return;
            }

            cmbSelectedPlayer.SelectedIndex = -1;
            formatTotalsForTeams();
        }

        private void btnTotalsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.SelectedIndex == -1)
            {
                return;
            }

            foreach (string item in lstTotals.SelectedItems)
            {
                var id = Convert.ToInt32(item.Substring(3).Split(')')[0]);
                var criterion = item.Split(':')[1].Trim().Split(' ');
                var parameter = criterion[0];
                var op = criterion[1];
                var filter = item.Substring(2, 1) == "T"
                                 ? _filters.Single(f => f.Key.SelectionType == SelectionType.Team && f.Key.ID == id)
                                 : _filters.Single(f => f.Key.SelectionType == SelectionType.Player && f.Key.ID == id);
                filter.Value.Remove(filter.Value.Single(o => o.Parameter1 == parameter && o.Operator == op));
                if (filter.Value.Count == 0)
                {
                    _filters.Remove(filter.Key);
                }
            }

            populateTotalsList();
        }

        /// <summary>Handles the LoadingRow event of the dg control.</summary>
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
            var doPlays = chkUsePBP.IsChecked == true;

            #region Initialize PBP Filters

            int curEventKey;
            if (cmbEventType.SelectedIndex <= 0)
            {
                curEventKey = -2;
            }
            else
            {
                curEventKey = PlayByPlayEntry.EventTypes.Single(pair => pair.Value == cmbEventType.SelectedItem.ToString()).Key;
            }

            int curPlayer1ID;
            if (cmbPlayer1.SelectedIndex <= 0)
            {
                curPlayer1ID = -1;
            }
            else
            {
                curPlayer1ID = ((KeyValuePair<int, string>) (((cmbPlayer1)).SelectedItem)).Key;
                _playersToHighlight.Add(curPlayer1ID);
            }

            int curPlayer2ID;
            if (cmbPlayer2.SelectedIndex <= 0)
            {
                curPlayer2ID = -1;
            }
            else
            {
                curPlayer2ID = ((KeyValuePair<int, string>) (((cmbPlayer2)).SelectedItem)).Key;
                _playersToHighlight.Add(curPlayer2ID);
            }

            int curLocation;
            if (cmbLocationShotDistance.SelectedIndex <= 0)
            {
                curLocation = -2;
            }
            else
            {
                curLocation = curEventKey == 2
                                  ? ShotEntry.ShotDistances.Single(
                                      dist => dist.Value == cmbLocationShotDistance.SelectedItem.ToString()).Key
                                  : PlayByPlayEntry.EventLocations.Single(
                                      loc => loc.Value == cmbLocationShotDistance.SelectedItem.ToString()).Key;
            }

            int shotOrigin;
            if (cmbShotOrigin.SelectedIndex <= 0 || cmbShotOrigin.IsEnabled == false)
            {
                shotOrigin = -2;
            }
            else
            {
                shotOrigin = ShotEntry.ShotOrigins.Single(orig => orig.Value == cmbShotOrigin.SelectedItem.ToString()).Key;
            }

            int shotType;
            if (cmbShotType.SelectedIndex <= 0 || cmbShotType.IsEnabled == false)
            {
                shotType = -2;
            }
            else
            {
                shotType = ShotEntry.ShotTypes.Single(orig => orig.Value == cmbShotType.SelectedItem.ToString()).Key;
            }

            var shotIsMade = chkShotIsMade.IsEnabled ? chkShotIsMade.IsChecked : null;

            var shotIsAssisted = chkShotIsAssisted.IsEnabled ? chkShotIsAssisted.IsChecked : null;

            double timeLeft;
            if (cmbTimeLeftOp.SelectedIndex > 0)
            {
                try
                {
                    timeLeft = PlayByPlayWindow.ConvertTimeStringToDouble(txtTimeLeftPar.Value as string);
                }
                catch
                {
                    timeLeft = -1;
                }
            }
            else
            {
                timeLeft = -1;
            }

            double shotClock;
            if (cmbShotClockOp.SelectedIndex > 0)
            {
                try
                {
                    shotClock = PlayByPlayWindow.ConvertTimeStringToDouble(txtShotClockPar.Value as string);
                }
                catch
                {
                    shotClock = -1;
                }
            }
            else
            {
                shotClock = -1;
            }

            int period;
            if (cmbPeriodOp.SelectedIndex > 0)
            {
                try
                {
                    period = (txtPeriodPar.Value as string).ToInt32();
                }
                catch
                {
                    period = Int32.MinValue;
                }
            }
            else
            {
                period = Int32.MinValue;
            }

            #endregion

            foreach (var bse in MainWindow.BSHist)
            {
                var keep = true;

                #region Apply Box Score Filters

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
                        var p = bse.BS.Team1ID == filter.Key.ID ? "1" : "2";
                        var oppP = p == "1" ? "2" : "1";

                        foreach (var option in filter.Value)
                        {
                            string parameter;
                            if (!option.Parameter1.StartsWith("Opp"))
                            {
                                switch (option.Parameter1)
                                {
                                    case "PF":
                                        parameter = "PTS" + p;
                                        break;
                                    case "PA":
                                        parameter = "PTS" + oppP;
                                        break;
                                    default:
                                        parameter = option.Parameter1 + p;
                                        break;
                                }
                            }
                            else
                            {
                                parameter = option.Parameter1.Substring(3) + oppP;
                            }
                            parameter = parameter.Replace("3P", "TP");
                            parameter = parameter.Replace("TO", "TOS");

                            var parameter2 = "";
                            if (!String.IsNullOrWhiteSpace(option.Parameter2))
                            {
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
                            }
                            var context = new ExpressionContext();

                            IGenericExpression<bool> ige;
                            if (String.IsNullOrWhiteSpace(parameter2))
                            {
                                if (!parameter.Contains("%"))
                                {
                                    ige =
                                        context.CompileGeneric<bool>(
                                            string.Format(
                                                "{0} {1} {2}",
                                                bse.BS.GetType().GetProperty(parameter).GetValue(bse.BS, null),
                                                option.Operator,
                                                option.Value));
                                }
                                else
                                {
                                    var par1 = parameter.Replace("%", "M");
                                    var par2 = parameter.Replace("%", "A");
                                    ige =
                                        context.CompileGeneric<bool>(
                                            string.Format(
                                                "(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                bse.BS.GetType().GetProperty(par1).GetValue(bse.BS, null),
                                                bse.BS.GetType().GetProperty(par2).GetValue(bse.BS, null),
                                                option.Operator,
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
                                            context.CompileGeneric<bool>(
                                                string.Format(
                                                    "{0} {1} {2}", getValue(bse, parameter), option.Operator, getValue(bse, parameter2)));
                                    }
                                    else
                                    {
                                        var par2Part1 = parameter2.Replace("%", "M");
                                        var par2Part2 = parameter2.Replace("%", "A");
                                        ige =
                                            context.CompileGeneric<bool>(
                                                string.Format(
                                                    "Cast({0}, double) {1} (Cast({2}, double) / Cast({3}, double))",
                                                    getValue(bse, parameter),
                                                    option.Operator,
                                                    getValue(bse, par2Part1),
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
                                                string.Format(
                                                    "(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                                    getValue(bse, par1Part1),
                                                    getValue(bse, par1Part2),
                                                    option.Operator,
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
                                                    getValue(bse, par1Part1),
                                                    getValue(bse, par1Part2),
                                                    option.Operator,
                                                    getValue(bse, par2Part1),
                                                    getValue(bse, par2Part2)));
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
                            var parameter = option.Parameter1;
                            parameter = parameter.Replace("3P", "TP");
                            parameter = parameter.Replace("TO", "TOS");
                            var context = new ExpressionContext();

                            IGenericExpression<bool> ige;
                            if (!parameter.Contains("%"))
                            {
                                ige =
                                    context.CompileGeneric<bool>(
                                        string.Format(
                                            "{0} {1} {2}",
                                            pbs.GetType().GetProperty(parameter).GetValue(pbs, null),
                                            option.Operator,
                                            option.Value));
                            }
                            else
                            {
                                var par1 = parameter.Replace("%", "M");
                                var par2 = parameter.Replace("%", "A");
                                ige =
                                    context.CompileGeneric<bool>(
                                        string.Format(
                                            "(Cast({0}, double) / Cast({1}, double)) {2} Cast({3}, double)",
                                            pbs.GetType().GetProperty(par1).GetValue(pbs, null),
                                            pbs.GetType().GetProperty(par2).GetValue(pbs, null),
                                            option.Operator,
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

                #endregion

                if (doPlays)
                {
                    var pbpeList = bse.PBPEList.ToList();

                    // Event Type
                    if (curEventKey > -2)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.EventType == curEventKey).ToList();
                        if (curEventKey == -1)
                        {
                            pbpeList = pbpeList.Where(pbpe => pbpe.EventDesc == txtEventDesc.Text).ToList();
                        }
                    }

                    // Player 1
                    if (curPlayer1ID > -1)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.Player1ID == curPlayer1ID).ToList();
                    }

                    // Player 2
                    if (curPlayer2ID > -1)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.Player2ID == curPlayer2ID).ToList();
                    }

                    // Location
                    if (curLocation > -2)
                    {
                        pbpeList = curEventKey == 2
                                       ? pbpeList.Where(pbpe => pbpe.ShotEntry.Distance == curLocation).ToList()
                                       : pbpeList.Where(pbpe => pbpe.Location == curLocation).ToList();
                        if (curLocation == -1)
                        {
                            pbpeList = pbpeList.Where(pbpe => pbpe.LocationDesc == txtLocationDesc.Text).ToList();
                        }
                    }

                    // Shot Origin
                    if (shotOrigin > -2)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.ShotEntry.Origin == shotOrigin).ToList();
                    }

                    // Shot Type
                    if (shotType > -2)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.ShotEntry.Type == shotType).ToList();
                    }

                    // Shot Is Made
                    if (shotIsMade != null)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.ShotEntry.IsMade == (shotIsMade == true)).ToList();
                    }

                    // Shot Is Assisted
                    if (shotIsAssisted != null)
                    {
                        pbpeList = pbpeList.Where(pbpe => pbpe.ShotEntry.IsAssisted == (shotIsAssisted == true)).ToList();
                    }

                    // Time Left
                    if (timeLeft > -0.5)
                    {
                        pbpeList =
                            pbpeList.Where(
                                pbpe =>
                                new ExpressionContext().CompileGeneric<bool>(
                                    string.Format("{0} {1} {2}", pbpe.TimeLeft, cmbTimeLeftOp.SelectedItem.ToString(), timeLeft))
                                                       .Evaluate()).ToList();
                    }

                    // Shot Clock
                    if (shotClock > -0.5)
                    {
                        pbpeList =
                            pbpeList.Where(
                                pbpe =>
                                new ExpressionContext().CompileGeneric<bool>(
                                    string.Format("{0} {1} {2}", pbpe.ShotClockLeft, cmbShotClockOp.SelectedItem.ToString(), shotClock))
                                                       .Evaluate()).ToList();
                    }

                    // Period
                    if (period > Int32.MinValue)
                    {
                        pbpeList =
                            pbpeList.Where(
                                pbpe =>
                                new ExpressionContext().CompileGeneric<bool>(
                                    string.Format("{0} {1} {2}", pbpe.Quarter, cmbPeriodOp.SelectedItem.ToString(), period)).Evaluate())
                                    .ToList();
                    }

                    // Players on the floor
                    pbpeList = PlayersOnTheFloor.Aggregate(
                        pbpeList,
                        (current, pid) =>
                        current.Where(pbpe => pbpe.Team1PlayerIDs.Contains(pid) || pbpe.Team2PlayerIDs.Contains(pid)).ToList());

                    // Check if we should include this box score
                    if (pbpeList.Count == 0)
                    {
                        keep = false;
                        notBsToCalculate.Add(bse);
                    }

                    bse.FilteredPBPEList = pbpeList;
                }
                if (keep)
                {
                    bsToCalculate.Add(bse);
                }
            }

            calculateAdvancedStats(bsToCalculate, notBsToCalculate, doPlays);

            tbcAdv.SelectedItem = tabPlayerStats;
        }

        private static object getValue(BoxScoreEntry bse, string parameter)
        {
            return bse.BS.GetType().GetProperty(parameter).GetValue(bse.BS, null);
        }

        private void calculateAdvancedStats(
            List<BoxScoreEntry> bsToCalculate, List<BoxScoreEntry> notBsToCalculate, bool doPlays = false)
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
                extractFromBoxScoreEntry(bse, ref advtst, ref advtstOpp, ref advpst, ref advPPtst, ref advPPtstOpp, doPlays);
            }

            foreach (var bse in notBsToCalculate)
            {
                extractFromBoxScoreEntry(
                    bse, ref advtstNot, ref advtstOppNot, ref advpstNot, ref advPPtstNot, ref advPPtstOppNot, doPlays);
            }

            TeamStats.CalculateAllMetrics(ref advtst, advtstOpp, false);
            TeamStats.CalculateAllMetrics(ref advtstOpp, advtst, false);
            PlayerStats.CalculateAllMetrics(ref advpst, advPPtst, advPPtstOpp, teamsPerPlayer: true);

            psrList = new ObservableCollection<PlayerStatsRow>();
            var playerStatsRows = psrList;
            advpst.ToList().ForEach(item => highlightAndAddPlayers(item, ref playerStatsRows));
            tsrList = new ObservableCollection<TeamStatsRow>();
            var teamStatsRows = tsrList;
            advtst.ToList().ForEach(item => highlightAndAddTeams(item, ref teamStatsRows));
            tsrOppList = new ObservableCollection<TeamStatsRow>();
            var oppStatsRows = tsrOppList;
            advtstOpp.ToList().ForEach(item => highlightAndAddTeams(item, ref oppStatsRows));

            dgvTeamStats.ItemsSource = tsrList;
            dgvOppStats.ItemsSource = tsrOppList;
            dgvPlayerStats.ItemsSource = psrList;

            TeamStats.CalculateAllMetrics(ref advtstNot, advtstOppNot, false);
            TeamStats.CalculateAllMetrics(ref advtstOppNot, advtstNot, false);
            PlayerStats.CalculateAllMetrics(ref advpstNot, advPPtstNot, advPPtstOppNot, teamsPerPlayer: true);

            psrList_Not = new ObservableCollection<PlayerStatsRow>();
            var playerStatsRowsNot = psrList_Not;
            advpstNot.ToList().ForEach(item => highlightAndAddPlayers(item, ref playerStatsRowsNot));
            tsrList_Not = new ObservableCollection<TeamStatsRow>();
            var teamStatsRowsNot = tsrList_Not;
            advtstNot.ToList().ForEach(item => highlightAndAddTeams(item, ref teamStatsRowsNot));
            tsrOppList_Not = new ObservableCollection<TeamStatsRow>();
            var oppStatsRowsNot = tsrOppList_Not;
            advtstOppNot.ToList().ForEach(item => highlightAndAddTeams(item, ref oppStatsRowsNot));

            dgvTeamStatsUnmet.ItemsSource = tsrList_Not;
            dgvOppStatsUnmet.ItemsSource = tsrOppList_Not;
            dgvPlayerStatsUnmet.ItemsSource = psrList_Not;

            bsToCalculate.ForEach(bse => bse.BS.PrepareForDisplay(advtst));
            notBsToCalculate.ForEach(bse => bse.BS.PrepareForDisplay(advtstNot));
            dgvBoxScores.ItemsSource = bsToCalculate.Select(bse => bse.BS);
            dgvBoxScoresUnmet.ItemsSource = notBsToCalculate.Select(bse => bse.BS);
        }

        private void extractFromBoxScoreEntry(
            BoxScoreEntry bse,
            ref Dictionary<int, TeamStats> advtst,
            ref Dictionary<int, TeamStats> advtstOpp,
            ref Dictionary<int, PlayerStats> advpst,
            ref Dictionary<int, TeamStats> advPPtst,
            ref Dictionary<int, TeamStats> advPPtstOpp,
            bool doPlays)
        {
            var team1ID = bse.BS.Team1ID;
            var team2ID = bse.BS.Team2ID;
            var bs = bse.BS;
            if (!advtst.ContainsKey(team1ID))
            {
                advtst.Add(
                    team1ID,
                    new TeamStats
                        {
                            ID = team1ID,
                            Name = MainWindow.TST[team1ID].Name,
                            DisplayName = MainWindow.TST[team1ID].DisplayName
                        });
                advtstOpp.Add(
                    team1ID,
                    new TeamStats
                        {
                            ID = team1ID,
                            Name = MainWindow.TST[team1ID].Name,
                            DisplayName = MainWindow.TST[team1ID].DisplayName
                        });
            }
            var ts1 = advtst[team1ID];
            var ts1Opp = advtstOpp[team1ID];
            if (!advtst.ContainsKey(team2ID))
            {
                //advTeamOrder.Add(MainWindow.tst[team2ID].name, advTeamOrder.Any() ? advTeamOrder.Values.Max() + 1 : 0);
                advtst.Add(
                    team2ID,
                    new TeamStats
                        {
                            ID = team2ID,
                            Name = MainWindow.TST[team2ID].Name,
                            DisplayName = MainWindow.TST[team2ID].DisplayName
                        });
                advtstOpp.Add(
                    team2ID,
                    new TeamStats
                        {
                            ID = team2ID,
                            Name = MainWindow.TST[team2ID].Name,
                            DisplayName = MainWindow.TST[team2ID].DisplayName
                        });
            }
            var ts2 = advtst[team2ID];
            var ts2Opp = advtstOpp[team2ID];
            if (!doPlays)
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts1, ref ts2, true);
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts2Opp, ref ts1Opp, true);
            }
            foreach (var pbs in bse.PBSList)
            {
                if (advpst.All(pair => pair.Key != pbs.PlayerID))
                {
                    advpst.Add(pbs.PlayerID, MainWindow.PST[pbs.PlayerID].DeepClone());
                    advpst[pbs.PlayerID].ResetStats();
                }
                if (!doPlays)
                {
                    advpst[pbs.PlayerID].AddBoxScore(pbs, false);
                }

                var teamID = pbs.TeamID;
                if (!advPPtst.ContainsKey(pbs.PlayerID))
                {
                    advPPtst.Add(
                        pbs.PlayerID,
                        new TeamStats
                            {
                                ID = teamID,
                                Name = MainWindow.TST[teamID].Name,
                                DisplayName = MainWindow.TST[teamID].DisplayName
                            });
                }
                if (!advPPtstOpp.ContainsKey(pbs.PlayerID))
                {
                    advPPtstOpp.Add(
                        pbs.PlayerID,
                        new TeamStats
                            {
                                ID = teamID,
                                Name = MainWindow.TST[teamID].Name,
                                DisplayName = MainWindow.TST[teamID].DisplayName
                            });
                }
                if (!doPlays)
                {
                    var ts = advPPtst[pbs.PlayerID];
                    var tsopp = advPPtstOpp[pbs.PlayerID];
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

            if (doPlays)
            {
                /*
                if (cmbPlayer1.SelectedIndex > 0)
                {
                    var curPlayer1ID = ((KeyValuePair<int, string>) ((cmbPlayer1).SelectedItem)).Key;
                    if (advpst.All(pair => pair.Key != curPlayer1ID))
                    {
                        advpst.Add(curPlayer1ID, MainWindow.PST[curPlayer1ID].DeepClone());
                        advpst[curPlayer1ID].ResetStats();
                    }
                }
                if (cmbPlayer2.SelectedIndex > 0)
                {
                    var curPlayer2ID = ((KeyValuePair<int, string>) ((cmbPlayer2).SelectedItem)).Key;
                    if (advpst.All(pair => pair.Key != curPlayer2ID))
                    {
                        advpst.Add(curPlayer2ID, MainWindow.PST[curPlayer2ID].DeepClone());
                        advpst[curPlayer2ID].ResetStats();
                    }
                }
                */
                var tempbse = bse.Clone();

                // Calculate
                for (var i = 0; i < tempbse.PBSList.Count; i++)
                {
                    var pbs = tempbse.PBSList[i];
                    if (advpst.ContainsKey(pbs.PlayerID) == false)
                    {
                        continue;
                    }
                    pbs.CalculateFromPBPEList(tempbse.FilteredPBPEList);
                    advpst[pbs.PlayerID].AddBoxScore(pbs, false);
                }
                var teamBoxScore = tempbse.BS;
                BoxScoreWindow.CalculateTeamsFromPlayers(
                    ref teamBoxScore,
                    tempbse.PBSList.Where(pbs => pbs.TeamID == team1ID).ToList(),
                    tempbse.PBSList.Where(pbs => pbs.TeamID == team2ID).ToList());
                TeamStats.AddTeamStatsFromBoxScore(tempbse.BS, ref ts1, ref ts2, true);
                TeamStats.AddTeamStatsFromBoxScore(tempbse.BS, ref ts2Opp, ref ts1Opp, true);
                foreach (var pbs in tempbse.PBSList)
                {
                    var ts = advPPtst[pbs.PlayerID];
                    var tsopp = advPPtstOpp[pbs.PlayerID];
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

        private void highlightAndAddPlayers(
            KeyValuePair<int, PlayerStats> pair, ref ObservableCollection<PlayerStatsRow> psrObservable)
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
            {
                return;
            }

            if (lstTotals.Items.Count > 0)
            {
                var mbr = MessageBox.Show(
                    "Do you want to clear the current stat filters before loading the new ones?",
                    "NBA Stats Tracker",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (mbr == MessageBoxResult.Yes)
                {
                    _filters.Clear();
                    lstTotals.Items.Clear();
                }
            }

            cmbPosition1Filter.SelectedIndex = 0;
            cmbPosition2Filter.SelectedIndex = 0;
            chkIsActive.IsChecked = null;
            cmbTeamFilter.SelectedIndex = 0;

            var lines = File.ReadAllLines(ofd.FileName);
            foreach (var item in lines)
            {
                if (!item.StartsWith("$$"))
                {
                    var id = Convert.ToInt32(item.Substring(3).Split(')')[0]);
                    var criterion = item.Split(':')[1].Trim().Split(' ');
                    var parameter = criterion[0];
                    var op = criterion[1];
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
                else
                {
                    var realItem = item.Substring(2);
                    var parts = realItem.Split('$');
                    var value = parts[1];
                    switch (parts[0])
                    {
                        case "ET":
                            cmbEventType.SelectedItem = value;
                            break;
                        case "ETDesc":
                            txtEventDesc.Text = value;
                            break;
                        case "P1":
                            try
                            {
                                cmbPlayer1.SelectedItem = playersList.Single(p => p.Key == value.ToInt32());
                            }
                            catch
                            {
                                cmbPlayer1.SelectedIndex = 0;
                            }
                            break;
                        case "P2":
                            try
                            {
                                cmbPlayer2.SelectedItem = playersList.Single(p => p.Key == value.ToInt32());
                            }
                            catch
                            {
                                cmbPlayer2.SelectedIndex = 0;
                            }
                            break;
                        case "LOC":
                            cmbLocationShotDistance.SelectedItem = value;
                            break;
                        case "LOCDesc":
                            txtLocationDesc.Text = value;
                            break;
                        case "ORIG":
                            cmbShotOrigin.SelectedItem = value;
                            break;
                        case "ST":
                            cmbShotType.SelectedItem = value;
                            break;
                        case "SIM":
                            chkShotIsMade.IsChecked = value == "Null" ? null : (bool?) Convert.ToBoolean(value);
                            break;
                        case "SIA":
                            chkShotIsMade.IsChecked = value == "Null" ? null : (bool?) Convert.ToBoolean(value);
                            break;
                        case "TL":
                            cmbTimeLeftOp.SelectedItem = value;
                            txtTimeLeftPar.Text = parts[2];
                            break;
                        case "SCL":
                            cmbShotClockOp.SelectedItem = value;
                            txtShotClockPar.Text = parts[2];
                            break;
                        case "PER":
                            cmbPeriodOp.SelectedItem = value;
                            txtPeriodPar.Text = parts[2];
                            break;
                        case "POTF":
                            var players = realItem.Split(new[] { '$' }, 2)[1].Split(
                                new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                            PlayersOnTheFloor.Clear();
                            foreach (var player in players)
                            {
                                PlayersOnTheFloor.Add(player.ToInt32());
                            }
                            break;
                        case "UsePBP":
                            chkUsePBP.IsChecked = Convert.ToBoolean(value);
                            break;
                    }
                }
            }

            populateTotalsList();
            updatePOTFToolTip();
        }

        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
                {
                    InitialDirectory = Path.GetFullPath(_folder),
                    Title = "Save Filters As",
                    Filter = "NST Advanced Stats Filters (*.naf)|*.naf",
                    DefaultExt = "naf"
                };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            try
            {
                var list = lstTotals.Items.Cast<string>().ToList();
                list.Add("$$ET$" + cmbEventType.SelectedItem);
                list.Add("$$ETDesc$" + txtEventDesc.Text);
                if (cmbPlayer1.SelectedIndex != -1)
                {
                    list.Add("$$P1$" + ((KeyValuePair<int, string>) (cmbPlayer1.SelectedItem)).Key);
                }
                if (cmbPlayer2.SelectedIndex != -1)
                {
                    list.Add("$$P2$" + ((KeyValuePair<int, string>) (cmbPlayer2.SelectedItem)).Key);
                }
                list.Add("$$LOC$" + cmbLocationShotDistance.SelectedItem);
                list.Add("$$LOCDesc$" + txtLocationDesc.Text);
                list.Add("$$ORIG$" + cmbShotOrigin.SelectedItem);
                list.Add("$$ST$" + cmbShotType.SelectedItem);
                list.Add("$$SIM$" + (chkShotIsMade.IsChecked.HasValue ? chkShotIsMade.IsChecked.ToString() : "Null"));
                list.Add("$$SIA$" + (chkShotIsAssisted.IsChecked.HasValue ? chkShotIsAssisted.IsChecked.ToString() : "Null"));
                list.Add("$$TL$" + cmbTimeLeftOp.SelectedItem + "$" + txtTimeLeftPar.Value);
                list.Add("$$SCL$" + cmbShotClockOp.SelectedItem + "$" + txtShotClockPar.Value);
                list.Add("$$PER$" + cmbPeriodOp.SelectedItem + "$" + txtPeriodPar.Value);
                list.Add(PlayersOnTheFloor.Aggregate("$$POTF$", (current, pid) => current + (pid + "$")));
                list.Add("$$UsePBP$" + (chkUsePBP.IsChecked == true));
                File.WriteAllLines(sfd.FileName, list);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't save filters file.\n\n" + ex.Message);
            }
        }

        private void cmbTotalsPar2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTotalsPar2.SelectedIndex == -1)
            {
                return;
            }

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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Tools.SetRegistrySetting("ASCHeight", Height);
            Tools.SetRegistrySetting("ASCWidth", Width);
            Tools.SetRegistrySetting("ASCX", Left);
            Tools.SetRegistrySetting("ASCY", Top);
        }

        private void cmbEventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbEventType.SelectedIndex == -1)
            {
                return;
            }

            int curEventKey;
            if (cmbEventType.SelectedIndex == 0)
            {
                curEventKey = -2;
            }
            else
            {
                curEventKey = PlayByPlayEntry.EventTypes.Single(pair => pair.Value == cmbEventType.SelectedItem.ToString()).Key;
            }

            txtEventDesc.IsEnabled = cmbEventType.SelectedItem.ToString() == "Other";

            grdShotEvent.IsEnabled = curEventKey == 1;
            txbLocationLabel.Text = curEventKey == 1 ? "Shot Distance" : "Location";
            txtLocationDesc.IsEnabled = false;
            var shotDistances = ShotEntry.ShotDistances.Values.ToList();
            shotDistances.Insert(0, "Any");

            var eventLocations = PlayByPlayEntry.EventLocations.Values.ToList();
            eventLocations.Insert(0, "Any");

            cmbLocationShotDistance.ItemsSource = curEventKey == 1 ? shotDistances : eventLocations;
            cmbLocationShotDistance.SelectedIndex = 0;
            if (curEventKey == 3 || curEventKey == 4)
            {
                cmbLocationShotDistance.IsEnabled = false;
            }
            else
            {
                cmbLocationShotDistance.IsEnabled = true;
            }

            try
            {
                var definition = PlayByPlayEntry.Player2Definition[curEventKey];
                txbPlayer2Label.Text = definition;
                cmbPlayer2.IsEnabled = true;
            }
            catch (KeyNotFoundException)
            {
                txbPlayer2Label.Text = "Not Applicable";
                cmbPlayer2.IsEnabled = false;
            }
        }

        private void cmbLocationShotDistance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLocationShotDistance.SelectedIndex <= 0)
            {
                txtLocationDesc.IsEnabled = false;
                return;
            }

            var curEventKey = PlayByPlayEntry.EventTypes.Single(pair => pair.Value == cmbEventType.SelectedItem.ToString()).Key;
            if (curEventKey != 1 && cmbLocationShotDistance.SelectedIndex != -1)
            {
                var curDistanceKey =
                    PlayByPlayEntry.EventLocations.Single(pair => pair.Value == cmbLocationShotDistance.SelectedItem.ToString()).Key;
                txtLocationDesc.IsEnabled = (curDistanceKey == -1 && curEventKey != 3 && curEventKey != 4);
            }
            else
            {
                txtLocationDesc.IsEnabled = false;
            }
        }

        private void btnHidePBP_Click(object sender, RoutedEventArgs e)
        {
            if (gdrPlayByPlay.Height.Value > 0)
            {
                gdrPlayByPlay.Height = new GridLength(0);
                Height -= _defaultPBPHeight;
                btnHidePBP.Content = "Show Play-By-Play Panel";
            }
            else
            {
                Height += _defaultPBPHeight;
                gdrPlayByPlay.Height = new GridLength(_defaultPBPHeight);
                btnHidePBP.Content = "Hide Play-By-Play Panel";
            }
        }

        private void chkUsePBP_Click(object sender, RoutedEventArgs e)
        {
            grdPBP1.IsEnabled = stpPBP2.IsEnabled = chkUsePBP.IsChecked == true;
        }

        private void btnSetPBPPlayers_Click(object sender, RoutedEventArgs e)
        {
            var ascsp = new ASCSelectPlayers(playersList, PlayersOnTheFloor);
            ascsp.ShowDialog();

            updatePOTFToolTip();
        }

        private void updatePOTFToolTip()
        {
            var tt = new ToolTip();
            var text = "";
            foreach (var id in PlayersOnTheFloor)
            {
                var info = MainWindow.PST[id].FullName + " (";
                var p1s = MainWindow.PST[id].Position1S;
                if (!String.IsNullOrWhiteSpace(p1s))
                {
                    info += p1s + " - ";
                }
                info += MainWindow.TST[MainWindow.PST[id].TeamF].DisplayName + ")";
                text += info + "\n";
            }
            text = text.TrimEnd(new[] { '\n' });
            tt.Content = text;
            btnSetPBPPlayers.ToolTip = tt;
        }

        #region Nested type: Filter

        private struct Filter
        {
            public readonly string Operator;
            public readonly string Parameter1;
            public readonly string Parameter2;
            public readonly string Value;

            public Filter(string parameter1, string oper, string parameter2, string value)
            {
                Parameter1 = parameter1;
                Operator = oper;
                Parameter2 = parameter2;
                Value = value;
            }
        }

        #endregion

        #region Nested type: Selection

        private struct Selection
        {
            public readonly int ID;
            public readonly SelectionType SelectionType;

            public Selection(SelectionType selectionType, int id)
            {
                SelectionType = selectionType;
                ID = id;
            }
        };

        #endregion
    }
}