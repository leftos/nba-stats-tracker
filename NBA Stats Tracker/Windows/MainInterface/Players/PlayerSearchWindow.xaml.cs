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

namespace NBA_Stats_Tracker.Windows.MainInterface.Players
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using Ciloci.Flee;

    using LeftosCommonLibrary;
    using LeftosCommonLibrary.CommonDialogs;

    using Microsoft.Win32;

    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.EventHandlers;
    using NBA_Stats_Tracker.Helper.Miscellaneous;

    #endregion

    /// <summary>Allows the user to search for players fulfilling any combination of user-specified criteria.</summary>
    public partial class PlayerSearchWindow
    {
        private readonly List<string> _contractOptions = new List<string> { "Any", "None", "Team", "Player", "Team2Yr" };
        private readonly List<string> _customExpressions = new List<string>();
        private readonly List<string> _customMetricNames;
        private readonly string _folder = App.AppDocsPath + @"\Search Filters";
        private readonly List<string> _metrics = PlayerStatsHelper.MetricsNames;

        private readonly List<string> _numericComparisons = new List<string> { "<", "<=", "=", ">=", ">" };
        private readonly List<string> _numericOperators = new List<string> { "+", "-", "*", "/", "(", ")" };
        private readonly List<string> _numericPSRPropertyNames = new List<string>();
        private readonly List<string> _numericTSRPropertyNames = new List<string>();
        private readonly List<string> _perGame = PlayerStatsHelper.ExtendedPerGame;

        private readonly List<string> _positions = new List<string> { "Any", "None", "PG", "SG", "SF", "PF", "C" };
        private readonly List<string> _splitOn = new List<string>();
        private readonly List<string> _stringOptions = new List<string> { "Contains", "Is" };

        private readonly List<string> _teamMetrics = TeamStatsHelper.MetricsNames;
        private readonly List<string> _teamPerGame = TeamStatsHelper.ExtendedPerGame;
        private readonly List<string> _teamTotals = TeamStatsHelper.ExtendedTotals;
        private readonly List<string> _totals = PlayerStatsHelper.ExtendedTotals;

        private List<PlayerStatsRow> _cList;
        private bool _changingTimeframe;
        private IEnumerable<string> _contractYears;
        private string _contractYearsLeftOp;
        private string _contractYearsLeftVal;
        private int _curSeason;
        private bool _fixingPageValue;
        private string _heightOp;
        private string _heightVal;
        private string _lastPropertyUsed;
        private IEnumerable<string> _metricFilters;
        private List<StartingFivePermutation> _permutations;
        private List<PlayerStatsRow> _pfList;
        private IEnumerable<string> _pgFilters;
        private List<PlayerStatsRow> _pgList;
        private List<PlayerStatsRow> _psrListPl;
        private List<PlayerStatsRow> _psrListSea;
        private ICollectionView _psrViewPl;
        private ICollectionView _psrViewSea;
        private List<PlayerStatsRow> _sfList;
        private List<PlayerStatsRow> _sgList;
        private IEnumerable<string> _totalsFilters;
        private string _weightOp;
        private string _weightVal;
        private string _yearsProOp;
        private string _yearsProVal;
        private string _yobOp;
        private string _yobVal;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerSearchWindow" /> class. Prepares the window by populating all the combo-boxes.
        /// </summary>
        public PlayerSearchWindow()
        {
            InitializeComponent();

            cmbFirstNameSetting.ItemsSource = _stringOptions;
            cmbFirstNameSetting.SelectedIndex = 0;
            cmbLastNameSetting.ItemsSource = _stringOptions;
            cmbLastNameSetting.SelectedIndex = 0;

            cmbPosition1.ItemsSource = _positions;
            cmbPosition1.SelectedIndex = 0;
            cmbPosition2.ItemsSource = _positions;
            cmbPosition2.SelectedIndex = 0;

            cmbYOBOp.ItemsSource = _numericComparisons;
            cmbYOBOp.SelectedIndex = 3;

            cmbYearsProOp.ItemsSource = _numericComparisons;
            cmbYearsProOp.SelectedIndex = 3;

            _curSeason = MainWindow.CurSeason;
            populateSeasonCombo();
            SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);

            cmbTotalsPar.ItemsSource = _totals;
            cmbTotalsOp.ItemsSource = _numericComparisons;
            cmbTotalsOp.SelectedIndex = 3;

            cmbAvgPar.ItemsSource = _perGame;
            cmbAvgOp.ItemsSource = _numericComparisons;
            cmbAvgOp.SelectedIndex = 3;

            cmbMetricsPar.ItemsSource = _metrics;
            cmbMetricsOp.ItemsSource = _numericComparisons;
            cmbMetricsOp.SelectedIndex = 3;

            for (var i = 1; i <= 7; i++)
            {
                cmbContractPar.Items.Add("Year " + i);
            }
            cmbContractOp.ItemsSource = _numericComparisons;
            cmbContractOp.SelectedIndex = 1;

            cmbHeightOp.ItemsSource = _numericComparisons;
            cmbHeightOp.SelectedIndex = 3;

            cmbWeightOp.ItemsSource = _numericComparisons;
            cmbWeightOp.SelectedIndex = 3;

            cmbContractYLeftOp.ItemsSource = _numericComparisons;
            cmbContractYLeftOp.SelectedIndex = 1;

            cmbContractOpt.ItemsSource = _contractOptions;
            cmbContractOpt.SelectedIndex = 0;

            populateCustomCombos();

            _splitOn = _numericOperators.ToList();
            _splitOn.Add(" ");
            cmbCustomOp.ItemsSource = _numericOperators;

            var comparisons = _numericComparisons.ToList();
            comparisons.Insert(0, "All");
            cmbCustomComp.ItemsSource = comparisons;
            cmbCustomComp.SelectedIndex = 0;

            _customMetricNames = new List<string>();

            //chkIsActive.IsChecked = null;
            //cmbTeam.SelectedItem = "- Any -";

            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            populateTeamsCombo();

            Height = Tools.GetRegistrySetting("APSHeight", (int) Height);
            Width = Tools.GetRegistrySetting("APSWidth", (int) Width);
            Top = Tools.GetRegistrySetting("APSY", (int) Top);
            Left = Tools.GetRegistrySetting("APSX", (int) Left);
        }

        private void populateCustomCombos()
        {
            // Players
            var hiddenProperties = new List<string> { "ID", "TeamF", "TeamS" };
            var psrProperties = (typeof(PlayerStatsRow)).GetProperties();
            foreach (var property in psrProperties)
            {
                if (!Tools.IsNumericType(property.PropertyType) || typeof(Enum).IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                var name = property.Name;
                name = name.Replace("p", "%");
                if (name != "TPG")
                {
                    name = name.Replace("TP", "3P");
                }
                name = name.Replace("TOS", "TO");
                if (!hiddenProperties.Contains(name))
                {
                    _numericPSRPropertyNames.Add(name);
                }
            }
            _numericPSRPropertyNames.Sort();
            var otherPropertyNames =
                _numericPSRPropertyNames.Where(
                    name =>
                    !_totals.Contains(name) && !_perGame.Contains(name) && !_metrics.Contains(name) && !hiddenProperties.Contains(name))
                                        .ToList();
            cmbCustomOther.ItemsSource = otherPropertyNames;
            cmbCustomTotals.ItemsSource = _totals;
            cmbCustomPerGame.ItemsSource = _perGame;
            cmbCustomMetrics.ItemsSource = _metrics;

            // Teams
            var hiddenTeamProperties = new List<string> { "ID" };
            var tsrProperties = (typeof(TeamStatsRow)).GetProperties();
            foreach (var property in tsrProperties)
            {
                if (!Tools.IsNumericType(property.PropertyType) || typeof(Enum).IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                var name = property.Name;
                name = name.Replace("p", "%");
                if (name != "TPG")
                {
                    name = name.Replace("TP", "3P");
                }
                name = name.Replace("TOS", "TO");
                if (!hiddenProperties.Contains(name))
                {
                    _numericTSRPropertyNames.Add(name);
                }
            }
            _numericTSRPropertyNames.Sort();
            var otherTeamPropertyNames =
                _numericTSRPropertyNames.Where(
                    name =>
                    !_teamTotals.Contains(name) && !_teamPerGame.Contains(name) && !_teamMetrics.Contains(name)
                    && !hiddenTeamProperties.Contains(name)).ToList();
            cmbCustomTeamOther.ItemsSource = otherTeamPropertyNames;
            cmbCustomTeamTotals.ItemsSource = _teamTotals;
            cmbCustomTeamPerGame.ItemsSource = _teamPerGame;
            cmbCustomTeamMetrics.ItemsSource = _teamMetrics;
        }

        /// <summary>Finds a team's name by its displayName.</summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private int GetTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(MainWindow.TST, displayName);
        }

        /// <summary>Populates the season combo.</summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;
            cmbTFSeason.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = _curSeason;
            cmbTFSeason.SelectedValue = _curSeason;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control. Loads all the team and player information for the newly
        ///     selected season, and refreshes the teams combo-box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private async void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                _changingTimeframe = false;

                if (MainWindow.Tf.SeasonNum != _curSeason || MainWindow.Tf.IsBetween)
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    MainWindow.ChangeSeason(_curSeason);
                    await updateData();
                }
            }
        }

        /// <summary>
        ///     Handles the Click event of the btnSearch control. Implements the searching algorithm by accumulating the criteria and
        ///     filtering the available players based on those criteria.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgvPlayerStats.ItemsSource = null;
            var pst = MainWindow.PST;
            var filteredPST = pst.AsEnumerable();

            if (!String.IsNullOrWhiteSpace(txtLastName.Text))
            {
                filteredPST = cmbLastNameSetting.SelectedItem.ToString() == "Contains"
                                  ? filteredPST.Where(pair => pair.Value.LastName.Contains(txtLastName.Text))
                                  : filteredPST.Where(pair => pair.Value.LastName == txtLastName.Text);
            }

            if (!String.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                filteredPST = cmbFirstNameSetting.SelectedItem.ToString() == "Contains"
                                  ? filteredPST.Where(pair => pair.Value.FirstName.Contains(txtFirstName.Text))
                                  : filteredPST.Where(pair => pair.Value.FirstName == txtFirstName.Text);
            }

            if (cmbPosition1.SelectedIndex != -1 && cmbPosition1.SelectedItem.ToString() != "Any")
            {
                filteredPST = filteredPST.Where(pair => pair.Value.Position1S == cmbPosition1.SelectedItem.ToString());
            }

            if (cmbPosition2.SelectedIndex != -1 && cmbPosition2.SelectedItem.ToString() != "Any")
            {
                filteredPST = filteredPST.Where(pair => pair.Value.Position2S == cmbPosition2.SelectedItem.ToString());
            }

            if (cmbContractOpt.SelectedIndex != -1 && cmbContractOpt.SelectedItem.ToString() != "Any")
            {
                filteredPST = filteredPST.Where(
                    pair => pair.Value.Contract.Option.ToString() == cmbContractOpt.SelectedItem.ToString());
            }

            if (chkIsActive.IsChecked.GetValueOrDefault())
            {
                filteredPST = filteredPST.Where(pair => pair.Value.IsSigned);
            }
            else if (chkIsActive.IsChecked != null)
            {
                filteredPST = filteredPST.Where(pair => !pair.Value.IsSigned);
            }

            if (chkIsInjured.IsChecked.GetValueOrDefault())
            {
                filteredPST = filteredPST.Where(pair => pair.Value.Injury.IsInjured);
            }
            else if (chkIsInjured.IsChecked != null)
            {
                filteredPST = filteredPST.Where(pair => !pair.Value.Injury.IsInjured);
            }

            if (chkIsAllStar.IsChecked.GetValueOrDefault())
            {
                filteredPST = filteredPST.Where(pair => pair.Value.IsAllStar);
            }
            else if (chkIsAllStar.IsChecked != null)
            {
                filteredPST = filteredPST.Where(pair => !pair.Value.IsAllStar);
            }

            if (chkIsChampion.IsChecked.GetValueOrDefault())
            {
                filteredPST = filteredPST.Where(pair => pair.Value.IsNBAChampion);
            }
            else if (chkIsChampion.IsChecked != null)
            {
                filteredPST = filteredPST.Where(pair => !pair.Value.IsNBAChampion);
            }

            if (cmbTeam.SelectedItem != null && !String.IsNullOrEmpty(cmbTeam.SelectedItem.ToString())
                && chkIsActive.IsChecked.GetValueOrDefault() && cmbTeam.SelectedItem.ToString() != "- Any -")
            {
                filteredPST = filteredPST.Where(pair => pair.Value.TeamF == GetTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString()));
            }

            var pstDict = filteredPST.ToDictionary(ps => ps.Value.ID, ps => ps.Value);
            _psrListSea = pstDict.Values.AsParallel().Select(ps => new PlayerStatsRow(ps)).ToList();
            _psrListPl = pstDict.Values.AsParallel().Select(ps => new PlayerStatsRow(ps, true)).ToList();
            var tsrListSea = MainWindow.TST.Values.Select(ts => new TeamStatsRow(ts)).ToList();
            var tsrListPl = MainWindow.TST.Values.Select(ts => new TeamStatsRow(ts, true)).ToList();
            var tsrOppListSea = MainWindow.TSTOpp.Values.Select(ts => new TeamStatsRow(ts)).ToList();
            var tsrOppListPl = MainWindow.TSTOpp.Values.Select(ts => new TeamStatsRow(ts, true)).ToList();

            #region Custom Expressions parsing

            _customMetricNames.Clear();

            dgvPlayerStats.Columns.SkipWhile(col => col.Header.ToString() != "Custom")
                          .Skip(1)
                          .ToList()
                          .ForEach(col => dgvPlayerStats.Columns.Remove(col));

            dgvPlayoffStats.Columns.SkipWhile(col => col.Header.ToString() != "Custom")
                           .Skip(1)
                           .ToList()
                           .ForEach(col => dgvPlayoffStats.Columns.Remove(col));

            var includedIDs = pstDict.Keys.ToList();
            var context = new ExpressionContext();
            var j = 0;
            var message = "";
            foreach (var itemS in _customExpressions)
            {
                var expByPlayer = includedIDs.ToDictionary(id => id, id => "");
                var plExpByPlayer = includedIDs.ToDictionary(id => id, id => "");
                var itemSParts = itemS.Split(new[] { ": " }, StringSplitOptions.None);
                var name = itemSParts[0];
                var item = itemSParts[1];
                var decimalPoints = itemSParts[3];
                var parts = item.Split(_splitOn.ToArray(), StringSplitOptions.RemoveEmptyEntries);
                var k = 0;
                PlayerStatsRow curPSR;
                PlayerStatsRow curPSRPl;

                var ignoredIDs = new List<int>();
                var ignoredPlIDs = new List<int>();
                for (var i = 0; i < item.Length; i++)
                {
                    var c1 = item[i];
                    if (_numericOperators.Contains(c1.ToString()))
                    {
                        foreach (var id in includedIDs)
                        {
                            if (!ignoredIDs.Contains(id))
                            {
                                expByPlayer[id] += c1;
                            }
                            if (!ignoredPlIDs.Contains(id))
                            {
                                plExpByPlayer[id] += c1;
                            }
                        }
                    }
                    else if (c1 == '$')
                    {
                        try
                        {
                            char c2;
                            try
                            {
                                c2 = item[i + 1];
                            }
                            catch (IndexOutOfRangeException)
                            {
                                MessageBox.Show("Encountered '$' at end of custom expression.");
                                return;
                            }
                            bool isPlayer;
                            bool? isOwnTeam = null;
                            string part;
                            if (c2 != '$')
                            {
                                part = parts[k++].Substring(1);
                                isPlayer = true;
                            }
                            else
                            {
                                part = parts[k++].Substring(3);
                                isPlayer = false;
                                if (item[i + 2] == 't')
                                {
                                    isOwnTeam = true;
                                }
                                else if (item[i + 2] == 'o')
                                {
                                    isOwnTeam = false;
                                }
                                else
                                {
                                    MessageBox.Show("Encountered unknown/malformed parameter: " + parts[k - 1]);
                                    return;
                                }
                            }
                            part = part.Replace("3P", "TP");
                            part = part.Replace("TO", "TOS");
                            part = part.Replace("%", "p");
                            foreach (var id in includedIDs)
                            {
                                if (!ignoredIDs.Contains(id))
                                {
                                    string val;
                                    curPSR = _psrListSea.Single(psr => psr.ID == id);
                                    if (isPlayer)
                                    {
                                        val =
                                            Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(part).GetValue(curPSR, null))
                                                   .ToString(".0###############");
                                    }
                                    else
                                    {
                                        if (curPSR.TeamF == -1)
                                        {
                                            val = "NaN";
                                        }
                                        else
                                        {
                                            var curTSR = isOwnTeam == true
                                                             ? tsrListSea.Single(tsr => tsr.ID == curPSR.TeamF)
                                                             : tsrOppListSea.Single(tsr => tsr.ID == curPSR.TeamF);
                                            val =
                                                Convert.ToDouble(typeof(TeamStatsRow).GetProperty(part).GetValue(curTSR, null))
                                                       .ToString(".0###############");
                                        }
                                    }
                                    if (val != "NaN")
                                    {
                                        expByPlayer[id] += val;
                                    }
                                    else
                                    {
                                        ignoredIDs.Add(id);
                                        expByPlayer[id] = "$$INVALID";
                                    }
                                }
                                if (!ignoredPlIDs.Contains(id))
                                {
                                    string plVal;
                                    curPSRPl = _psrListPl.Single(psr => psr.ID == id);
                                    if (isPlayer)
                                    {
                                        plVal =
                                            Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(part).GetValue(curPSRPl, null))
                                                   .ToString(".0###############");
                                    }
                                    else
                                    {
                                        if (curPSRPl.TeamF == -1)
                                        {
                                            plVal = "NaN";
                                        }
                                        else
                                        {
                                            var curTSRPl = isOwnTeam == true
                                                               ? tsrListPl.Single(tsr => tsr.ID == curPSRPl.TeamF)
                                                               : tsrOppListPl.Single(tsr => tsr.ID == curPSRPl.TeamF);
                                            plVal =
                                                Convert.ToDouble(typeof(TeamStatsRow).GetProperty(part).GetValue(curTSRPl, null))
                                                       .ToString(".0###############");
                                        }
                                    }
                                    if (plVal != "NaN")
                                    {
                                        plExpByPlayer[id] += plVal;
                                    }
                                    else
                                    {
                                        ignoredPlIDs.Add(id);
                                        plExpByPlayer[id] = "$$INVALID";
                                    }
                                }
                            }
                            if (isPlayer)
                            {
                                i += part.Length; // should be Length-1 but we're using Substring(1) in part's initialization
                            }
                            else
                            {
                                i += part.Length + 2; // we're using Substring(3)
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error encountered while trying to evaluate custom expression parameter.\n\n" + ex.Message);
                            return;
                        }
                    }
                    else if (Tools.IsNumeric(parts[k]))
                    {
                        var part = parts[k++];
                        foreach (var id in includedIDs)
                        {
                            if (!ignoredIDs.Contains(id))
                            {
                                expByPlayer[id] += part;
                            }
                            if (!ignoredPlIDs.Contains(id))
                            {
                                plExpByPlayer[id] += part;
                            }
                        }
                        i += part.Length - 1;
                    }
                }
                foreach (var id in includedIDs)
                {
                    curPSR = _psrListSea.Single(psr => psr.ID == id);
                    curPSRPl = _psrListPl.Single(psr => psr.ID == id);
                    if (!ignoredIDs.Contains(id))
                    {
                        try
                        {
                            var compiled = context.CompileGeneric<double>(expByPlayer[id]);
                            curPSR.Custom.Add(compiled.Evaluate());
                        }
                        catch (Exception ex)
                        {
                            message = string.Format(
                                "Expression {0}:\n{1}\nevaluated to\n{2}\n\nError: {3}", (j + 1), item, expByPlayer[id], ex.Message);
                            curPSR.Custom.Add(double.NaN);
                        }
                    }
                    else
                    {
                        curPSR.Custom.Add(double.NaN);
                    }
                    if (!ignoredPlIDs.Contains(id))
                    {
                        try
                        {
                            var compiled = context.CompileGeneric<double>(plExpByPlayer[id]);
                            curPSRPl.Custom.Add(compiled.Evaluate());
                        }
                        catch (Exception ex)
                        {
                            message = string.Format(
                                "Expression {0}:\n{1}\nevaluated to\n{2}\n\nError: {3}", (j + 1), item, plExpByPlayer[id], ex.Message);
                            curPSRPl.Custom.Add(double.NaN);
                        }
                    }
                    else
                    {
                        curPSRPl.Custom.Add(double.NaN);
                    }
                }
                dgvPlayerStats.Columns.Add(
                    new DataGridTextColumn
                        {
                            Header = name,
                            Binding =
                                new Binding
                                    {
                                        Path = new PropertyPath(string.Format("Custom[{0}]", j)),
                                        Mode = BindingMode.OneWay,
                                        StringFormat = "{0:F" + decimalPoints + "}"
                                    }
                        });
                dgvPlayoffStats.Columns.Add(
                    new DataGridTextColumn
                        {
                            Header = name,
                            Binding =
                                new Binding
                                    {
                                        Path = new PropertyPath(string.Format("Custom[{0}]", j)),
                                        Mode = BindingMode.OneWay,
                                        StringFormat = "{0:F" + decimalPoints + "}"
                                    }
                        });
                _customMetricNames.Add(name);
                j++;
            }

            populateSituationalsCombo();

            #endregion Custom Expressions parsing

            IsEnabled = false;
            var pw = new ProgressWindow("Please wait while players are filtered...", false, false);
            pw.Show();

            _yearsProVal = txtYearsProVal.Text;
            _yearsProOp = cmbYearsProOp.SelectedItem.ToString();
            _yobVal = txtYOBVal.Text;
            _yobOp = cmbYOBOp.SelectedItem.ToString();
            _heightVal = txtHeightVal.Text;
            _heightOp = cmbHeightOp.SelectedItem.ToString();
            _weightVal = txtWeightVal.Text;
            _weightOp = cmbWeightOp.SelectedItem.ToString();
            _contractYearsLeftVal = txtContractYLeftVal.Text;
            _contractYearsLeftOp = cmbContractYLeftOp.SelectedItem.ToString();
            _contractYears = lstContract.Items.Cast<string>();
            _totalsFilters = lstTotals.Items.Cast<string>();
            _pgFilters = lstAvg.Items.Cast<string>();
            _metricFilters = lstMetrics.Items.Cast<string>();

            _psrViewSea = CollectionViewSource.GetDefaultView(_psrListSea);
            await Task.Run(() => _psrViewSea.Filter = filter);

            _psrViewPl = CollectionViewSource.GetDefaultView(_psrListPl);
            await Task.Run(() => _psrViewPl.Filter = filter);

            dgvPlayerStats.ItemsSource = _psrViewSea;
            dgvPlayoffStats.ItemsSource = _psrViewPl;

            string sortColumn;
            foreach (var item in lstMetrics.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("%", "p");
                _psrViewSea.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
                _psrViewPl.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }
            foreach (var item in lstAvg.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("3P", "TP");
                sortColumn = sortColumn.Replace("%", "p");
                _psrViewSea.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
                _psrViewPl.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }
            foreach (var item in lstTotals.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("3P", "TP");
                sortColumn = sortColumn.Replace("TO", "TOS");
                _psrViewSea.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
                _psrViewPl.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }

            if (_psrViewSea.SortDescriptions.Count == 0)
            {
                dgvPlayerStats.Columns.Single(col => col.Header.ToString() == "GmSc").SortDirection = ListSortDirection.Descending;
                dgvPlayoffStats.Columns.Single(col => col.Header.ToString() == "GmSc").SortDirection = ListSortDirection.Descending;
                _psrViewSea.SortDescriptions.Add(new SortDescription("GmSc", ListSortDirection.Descending));
                _psrViewPl.SortDescriptions.Add(new SortDescription("GmSc", ListSortDirection.Descending));
            }

            tbcPlayerSearch.SelectedItem = tabResults;

            _lastPropertyUsed = "";

            await prepareSituationalSeason();
            await prepareSituationalPlayoffs();

            IsEnabled = true;
            pw.CanClose = true;
            pw.Close();

            if (!String.IsNullOrWhiteSpace(message))
            {
                const string warning = "An error occurred while trying to evaluate one or more of the custom column expressions:\n\n";
                const string contact =
                    "\n\nIf you believe your expression is correct and that you shouldn't be getting this error, contact the developer "
                    + "and give them this information.";
                var cmw = new CopyableMessageWindow(warning + message + contact, "Error while evaluating expression", beep: true);
                cmw.ShowDialog();
            }
        }

        /// <summary>Used to filter the PlayerStatsRow results based on any user-specified metric stats criteria.</summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        private bool filter(object o)
        {
            var psr = (PlayerStatsRow) o;
            var ps = new PlayerStats(psr);
            var keep = true;
            var context = new ExpressionContext();
            IGenericExpression<bool> ige;

            if (!String.IsNullOrWhiteSpace(_yearsProVal))
            {
                ige = context.CompileGeneric<bool>(psr.YearsPro.ToString() + _yearsProOp + _yearsProVal);
                if (ige.Evaluate() == false)
                {
                    return false;
                }
            }
            if (!String.IsNullOrWhiteSpace(_yobVal))
            {
                context = new ExpressionContext();
                ige = context.CompileGeneric<bool>(psr.YearOfBirth.ToString() + _yobOp + _yobVal);
                if (ige.Evaluate() == false)
                {
                    return false;
                }
            }
            if (!String.IsNullOrWhiteSpace(_heightVal))
            {
                var metricHeight = MainWindow.IsImperial
                                       ? PlayerStatsRow.ConvertImperialHeightToMetric(_heightVal)
                                       : Convert.ToDouble(_heightVal);
                context = new ExpressionContext();
                ige = context.CompileGeneric<bool>(psr.Height.ToString() + _heightOp + metricHeight.ToString());
                if (ige.Evaluate() == false)
                {
                    return false;
                }
            }
            if (!String.IsNullOrWhiteSpace(_weightVal))
            {
                var imperialWeight = MainWindow.IsImperial
                                         ? Convert.ToDouble(_weightVal)
                                         : PlayerStatsRow.ConvertMetricWeightToImperial(_weightVal);
                context = new ExpressionContext();
                ige = context.CompileGeneric<bool>(psr.Weight.ToString() + _weightOp + imperialWeight.ToString());
                if (ige.Evaluate() == false)
                {
                    return false;
                }
            }
            if (!String.IsNullOrWhiteSpace(_contractYearsLeftVal))
            {
                context = new ExpressionContext();
                ige =
                    context.CompileGeneric<bool>(
                        (chkExcludeOption.IsChecked.GetValueOrDefault()
                             ? psr.ContractYearsMinusOption.ToString()
                             : psr.ContractYears.ToString()) + _contractYearsLeftOp + _contractYearsLeftVal);
                if (ige.Evaluate() == false)
                {
                    return false;
                }
            }

            foreach (var contractYear in _contractYears)
            {
                var parts = contractYear.Split(' ');
                ige =
                    context.CompileGeneric<bool>(
                        psr.GetType().GetProperty("ContractY" + parts[1]).GetValue(psr, null) + parts[2] + parts[3]);
                keep = ige.Evaluate();
                if (!keep)
                {
                    return false;
                }
            }

            var stopped = false;

            Parallel.ForEach(
                _totalsFilters,
                (item, loopState) =>
                    {
                        var parts = item.Split(' ');
                        parts[0] = parts[0].Replace("3P", "TP");
                        parts[0] = parts[0].Replace("TO", "TOS");
                        context = new ExpressionContext();
                        ige =
                            context.CompileGeneric<bool>(psr.GetType().GetProperty(parts[0]).GetValue(psr, null) + parts[1] + parts[2]);
                        keep = ige.Evaluate();
                        if (!keep)
                        {
                            stopped = true;
                            loopState.Stop();
                        }
                    });

            if (!keep || stopped)
            {
                return false;
            }

            Parallel.ForEach(
                _pgFilters,
                (item, loopState) =>
                    {
                        var parts = item.Split(' ');
                        parts[0] = parts[0].Replace("3P", "TP");
                        parts[0] = parts[0].Replace("%", "p");
                        context = new ExpressionContext();
                        var value = psr.GetType().GetProperty(parts[0]).GetValue(psr, null);
                        if (!Double.IsNaN(Convert.ToDouble(value)))
                        {
                            ige = context.CompileGeneric<bool>(value + parts[1] + parts[2]);
                            keep = ige.Evaluate();
                        }
                        else
                        {
                            keep = false;
                        }
                        if (!keep)
                        {
                            stopped = true;
                            loopState.Stop();
                        }
                    });

            if (!keep || stopped)
            {
                return false;
            }

            Parallel.ForEach(
                _metricFilters,
                (item, loopState) =>
                    {
                        var parts = item.Split(' ');
                        //parts[0] = parts[0].Replace("%", "p");
                        //double val = Convert.ToDouble(parts[2]);
                        context = new ExpressionContext();
                        var actualValue = ps.Metrics[parts[0]];
                        var filterType = parts[1];
                        if (!double.IsNaN(actualValue))
                        {
                            if (double.IsPositiveInfinity(actualValue))
                            {
                                switch (filterType)
                                {
                                    case ">":
                                    case ">=":
                                        keep = true;
                                        break;
                                    default:
                                        keep = false;
                                        break;
                                }
                            }
                            else if (double.IsNegativeInfinity(actualValue))
                            {
                                switch (filterType)
                                {
                                    case "<":
                                    case "<=":
                                        keep = true;
                                        break;
                                    default:
                                        keep = false;
                                        break;
                                }
                            }
                            else
                            {
                                ige = context.CompileGeneric<bool>(actualValue + filterType + parts[2]);
                                keep = ige.Evaluate();
                            }
                        }
                        else
                        {
                            keep = false;
                        }
                        if (!keep)
                        {
                            stopped = true;
                            loopState.Stop();
                        }
                    });

            if (!keep || stopped)
            {
                return false;
            }

            var expCount = _customExpressions.Count;
            Parallel.For(
                0,
                expCount,
                (i, loopState) =>
                    {
                        var index = i;
                        var exp = _customExpressions[index];
                        var itemSParts = exp.Split(new[] { ": " }, StringSplitOptions.None);
                        var filterParts = itemSParts[2].Split(new[] { ' ' }, 2);
                        var filterType = filterParts[0];
                        var filterVal = filterParts.Length == 2 ? filterParts[1] : "";

                        if (filterType == "All")
                        {
                            return;
                        }

                        var actualValue = psr.Custom[index];
                        if (!double.IsNaN(actualValue))
                        {
                            if (double.IsPositiveInfinity(actualValue))
                            {
                                switch (filterType)
                                {
                                    case ">":
                                    case ">=":
                                        keep = true;
                                        break;
                                    default:
                                        keep = false;
                                        break;
                                }
                            }
                            else if (double.IsNegativeInfinity(actualValue))
                            {
                                switch (filterType)
                                {
                                    case "<":
                                    case "<=":
                                        keep = true;
                                        break;
                                    default:
                                        keep = false;
                                        break;
                                }
                            }
                            else
                            {
                                ige = new ExpressionContext().CompileGeneric<bool>(actualValue + filterType + filterVal);
                                keep = ige.Evaluate();
                            }
                        }
                        else
                        {
                            keep = false;
                        }
                        if (!keep)
                        {
                            stopped = true;
                            loopState.Stop();
                        }
                    });

            if (!keep || stopped)
            {
                return false;
            }
            return keep;
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

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>Handles the SelectionChanged event of the cmbTeam control. Switches the IsSigned criterion to true if a team is selected.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTeam.SelectedIndex != -1)
            {
                chkIsActive.IsChecked = true;
            }
        }

        /// <summary>
        ///     Handles the Click event of the chkIsActive control. Switches the currently selected team to the "Any" option if the IsSigned
        ///     criterion is set to true; otherwise, resets the teams combo-box to no selection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault())
            {
                cmbTeam.SelectedIndex = 0;
            }
            else
            {
                cmbTeam.SelectedIndex = -1;
            }
        }

        /// <summary>Handles the Click event of the btnTotalsAdd control. Adds a total stats filter.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnTotalsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTotalsPar.SelectedIndex == -1 || cmbTotalsOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtTotalsVal.Text))
            {
                return;
            }

            try
            {
                Convert.ToSingle(txtTotalsVal.Text);
            }
            catch
            {
                return;
            }

            lstTotals.Items.Add(cmbTotalsPar.SelectedItem + " " + cmbTotalsOp.SelectedItem + " " + txtTotalsVal.Text);
            cmbTotalsPar.SelectedIndex = -1;
            txtTotalsVal.Text = "";
        }

        /// <summary>Handles the Click event of the btnTotalsDel control. Deletes a total stats filter.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnTotalsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.SelectedIndex == -1)
            {
                return;
            }

            if (lstTotals.SelectedItems.Count == 1)
            {
                var item = lstTotals.SelectedItem.ToString();
                lstTotals.Items.Remove(item);
                var parts = item.Split(' ');
                cmbTotalsPar.SelectedItem = parts[0];
                cmbTotalsOp.SelectedItem = parts[1];
                txtTotalsVal.Text = parts[2];
            }
            else
            {
                foreach (var item in new List<string>(lstTotals.SelectedItems.Cast<string>()))
                {
                    lstTotals.Items.Remove(item);
                }
            }
        }

        /// <summary>Handles the Click event of the btnAvgAdd control. Adds an average stats filter.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnAvgAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAvgPar.SelectedIndex == -1 || cmbAvgOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtAvgVal.Text))
            {
                return;
            }

            try
            {
                Convert.ToSingle(txtAvgVal.Text);
            }
            catch
            {
                return;
            }

            lstAvg.Items.Add(cmbAvgPar.SelectedItem + " " + cmbAvgOp.SelectedItem + " " + txtAvgVal.Text);
            cmbAvgPar.SelectedIndex = -1;
            txtAvgVal.Text = "";
        }

        /// <summary>Handles the Click event of the btnAvgDel control. Deletes an average stats filter.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnAvgDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvg.SelectedIndex == -1)
            {
                return;
            }

            if (lstAvg.SelectedItems.Count == 1)
            {
                var item = lstAvg.SelectedItem.ToString();
                lstAvg.Items.Remove(item);
                var parts = item.Split(' ');
                cmbAvgPar.SelectedItem = parts[0];
                cmbAvgOp.SelectedItem = parts[1];
                txtAvgVal.Text = parts[2];
            }
            else
            {
                foreach (var item in new List<string>(lstAvg.SelectedItems.Cast<string>()))
                {
                    lstAvg.Items.Remove(item);
                }
            }
        }

        /// <summary>Handles the Click event of the btnMetricsAdd control. Adds a metric stats filter.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnMetricsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMetricsPar.SelectedIndex == -1 || cmbMetricsOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtMetricsVal.Text))
            {
                return;
            }

            try
            {
                Convert.ToDouble(txtMetricsVal.Text);
            }
            catch
            {
                return;
            }

            lstMetrics.Items.Add(cmbMetricsPar.SelectedItem + " " + cmbMetricsOp.SelectedItem + " " + txtMetricsVal.Text);
            cmbMetricsPar.SelectedIndex = -1;
            txtMetricsVal.Text = "";
        }

        /// <summary>Handles the Click event of the btnMetricsDel control. Deletes a metric stats filter.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnMetricsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstMetrics.SelectedIndex == -1)
            {
                return;
            }

            if (lstMetrics.SelectedItems.Count == 1)
            {
                var item = lstMetrics.SelectedItem.ToString();
                lstMetrics.Items.Remove(item);
                var parts = item.Split(' ');
                cmbMetricsPar.SelectedItem = parts[0];
                cmbMetricsOp.SelectedItem = parts[1];
                txtMetricsVal.Text = parts[2];
            }
            else
            {
                foreach (var item in new List<string>(lstMetrics.SelectedItems.Cast<string>()))
                {
                    lstMetrics.Items.Remove(item);
                }
            }
        }

        /// <summary>
        ///     Handles the Sorting event of the dgvPlayerStats control. Uses a custom Sorting event handler that sorts a stat in descending
        ///     order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvPlayerStats_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        /// <summary>Handles the Click event of the btnLoadFilters control. Loads the filters from a previously saved filters file.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnLoadFilters_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new OpenFileDialog
                {
                    InitialDirectory = Path.GetFullPath(_folder),
                    Filter = "NST Search Filters (*.nsf)|*.nsf",
                    DefaultExt = "nsf"
                };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            var filterCount = lstTotals.Items.Count + lstAvg.Items.Count + lstMetrics.Items.Count + lstContract.Items.Count
                              + _customExpressions.Count;
            if (filterCount > 0)
            {
                var mbr =
                    MessageBox.Show(
                        "Do you want to clear the current stat filters and custom columns before loading the new ones?",
                        "NBA Stats Tracker",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel)
                {
                    return;
                }
                if (mbr == MessageBoxResult.Yes)
                {
                    lstTotals.Items.Clear();
                    lstAvg.Items.Clear();
                    lstMetrics.Items.Clear();
                    lstContract.Items.Clear();
                    _customExpressions.Clear();
                }
            }

            var s = File.ReadAllLines(sfd.FileName);
            for (var i = 0; i < s.Length; i++)
            {
                var parts = s[i].Split('\t');
                switch (parts[0])
                {
                    case "LastName":
                        cmbLastNameSetting.SelectedItem = parts[1];
                        txtLastName.Text = parts[2];
                        break;

                    case "FirstName":
                        cmbFirstNameSetting.SelectedItem = parts[1];
                        txtFirstName.Text = parts[2];
                        break;

                    case "Position":
                        cmbPosition1.SelectedItem = parts[1];
                        cmbPosition2.SelectedItem = parts[2];
                        break;

                    case "YearOfBirth":
                        cmbYOBOp.SelectedItem = parts[1];
                        txtYOBVal.Text = parts[2];
                        break;

                    case "YearsPro":
                        cmbYearsProOp.SelectedItem = parts[1];
                        _yearsProVal = parts[2];
                        break;

                    case "Height":
                        cmbHeightOp.SelectedItem = parts[1];
                        txtHeightVal.Text = parts[2];
                        break;

                    case "Weight":
                        cmbWeightOp.SelectedItem = parts[1];
                        txtWeightVal.Text = parts[2];
                        break;

                    case "ContractYLeft":
                        cmbContractYLeftOp.SelectedItem = parts[1];
                        txtContractYLeftVal.Text = parts[2];
                        break;

                    case "ContractOpt":
                        cmbContractOpt.SelectedItem = parts[1];
                        break;

                    case "Active":
                        try
                        {
                            chkIsActive.IsChecked = bool.Parse(parts[1]);
                        }
                        catch
                        {
                            chkIsActive.IsChecked = null;
                        }
                        break;

                    case "Injured":
                        try
                        {
                            chkIsInjured.IsChecked = bool.Parse(parts[1]);
                        }
                        catch
                        {
                            chkIsInjured.IsChecked = null;
                        }
                        break;

                    case "AllStar":
                        try
                        {
                            chkIsAllStar.IsChecked = bool.Parse(parts[1]);
                        }
                        catch
                        {
                            chkIsAllStar.IsChecked = null;
                        }
                        break;

                    case "Champion":
                        try
                        {
                            chkIsChampion.IsChecked = bool.Parse(parts[1]);
                        }
                        catch
                        {
                            chkIsChampion.IsChecked = null;
                        }
                        break;

                    case "Team":
                        cmbTeam.SelectedItem = MainWindow.TST[Convert.ToInt32(parts[1])].DisplayName;
                        break;

                    case "Totals":
                        while (true)
                        {
                            var line = s[++i];
                            if (line.StartsWith("TotalsEND"))
                            {
                                break;
                            }

                            lstTotals.Items.Add(line);
                        }
                        break;

                    case "Avg":
                        while (true)
                        {
                            var line = s[++i];
                            if (line.StartsWith("AvgEND"))
                            {
                                break;
                            }

                            lstAvg.Items.Add(line);
                        }
                        break;

                    case "Metrics":
                        while (true)
                        {
                            var line = s[++i];
                            if (line.StartsWith("MetricsEND"))
                            {
                                break;
                            }

                            lstMetrics.Items.Add(line);
                        }
                        break;

                    case "Contract":
                        while (true)
                        {
                            var line = s[++i];
                            if (line.StartsWith("ContractEND"))
                            {
                                break;
                            }

                            lstMetrics.Items.Add(line);
                        }
                        break;

                    case "Custom":
                        while (true)
                        {
                            var line = s[++i];
                            if (line.StartsWith("CustomEND"))
                            {
                                break;
                            }

                            _customExpressions.Add(line);
                        }
                        break;

                    case "TF":
                        if (parts[1].ToUpperInvariant() == "true")
                        {
                            dtpStart.SelectedDate = Convert.ToDateTime(parts[2]);
                            dtpEnd.SelectedDate = Convert.ToDateTime(parts[3]);
                            rbStatsBetween.IsChecked = true;
                        }
                        else
                        {
                            try
                            {
                                cmbTFSeason.SelectedItem = parts[2];
                            }
                            catch
                            {
                                Console.WriteLine("Season could not be selected while loading search filter.");
                            }
                            rbStatsAllTime.IsChecked = true;
                        }
                        break;
                }
            }
            refreshCustomExpressionList();
        }

        /// <summary>Handles the Click event of the btnSaveFilters control. Saves the current filters to a file.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            var s = "";
            s += String.Format("LastName\t{0}\t{1}\n", cmbLastNameSetting.SelectedItem, txtLastName.Text);
            s += String.Format("FirstName\t{0}\t{1}\n", cmbFirstNameSetting.SelectedItem, txtFirstName.Text);
            s += String.Format("Position\t{0}\t{1}\n", cmbPosition1.SelectedItem, cmbPosition2.SelectedItem);
            s += String.Format("YearOfBirth\t{0}\t{1}\n", cmbYOBOp.SelectedItem, txtYOBVal.Text);
            s += String.Format("YearsPro\t{0}\t{1}\n", cmbYearsProOp.SelectedItem, _yearsProVal);
            s += String.Format("Height\t{0}\t{1}\n", cmbHeightOp.SelectedItem, txtHeightVal.Text);
            s += String.Format("Weight\t{0}\t{1}\n", cmbWeightOp.SelectedItem, txtWeightVal.Text);
            s += String.Format("ContractYLeft\t{0}\t{1}\n", cmbContractYLeftOp.SelectedItem, txtContractYLeftVal.Text);
            s += String.Format("ContractOpt\t{0}\n", cmbContractOpt.SelectedItem);
            s += String.Format("Active\t{0}\n", chkIsActive.IsChecked.ToString());
            s += String.Format("Injured\t{0}\n", chkIsInjured.IsChecked.ToString());
            s += String.Format("AllStar\t{0}\n", chkIsAllStar.IsChecked.ToString());
            s += String.Format("Champion\t{0}\n", chkIsChampion.IsChecked.ToString());
            if (cmbTeam.SelectedItem != null)
            {
                s += String.Format("Team\t{0}\n", GetTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString()));
            }
            s += String.Format("Season\t{0}\n", _curSeason);
            s += String.Format("Totals\n");
            s = lstTotals.Items.Cast<string>().Aggregate(s, (current, item) => current + (item + "\n"));
            s += "TotalsEND\n";
            s += String.Format("Avg\n");
            s = lstAvg.Items.Cast<string>().Aggregate(s, (current, item) => current + (item + "\n"));
            s += "AvgEND\n";
            s += String.Format("Metrics\n");
            s = lstMetrics.Items.Cast<string>().Aggregate(s, (current, item) => current + (item + "\n"));
            s += "MetricsEND\n";
            s += String.Format("Contract\n");
            s = lstContract.Items.Cast<string>().Aggregate(s, (current, item) => current + (item + "\n"));
            s += "ContractEND\n";
            s += String.Format("Custom\n");
            s = lstCustom.Items.Cast<string>().Aggregate(s, (current, item) => current + (item + "\n"));
            s += "CustomEND\n";
            var isBetween = rbStatsBetween.IsChecked.GetValueOrDefault();
            s += String.Format(
                "TF\t{0}\t{1}\t{2}",
                isBetween.ToString(),
                isBetween ? dtpStart.SelectedDate.GetValueOrDefault().ToString() : cmbTFSeason.SelectedItem.ToString(),
                isBetween ? dtpEnd.SelectedDate.GetValueOrDefault().ToString() : "");

            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }

            var sfd = new SaveFileDialog
                {
                    InitialDirectory = Path.GetFullPath(_folder),
                    Filter = "NST Search Filters (*.nsf)|*.nsf",
                    DefaultExt = "nsf"
                };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
            {
                return;
            }

            try
            {
                File.WriteAllText(sfd.FileName, s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't save filters file.\n\n", ex.Message);
            }
        }

        private async Task updateData()
        {
            IsEnabled = false;
            await MainWindow.UpdateAllData(true);
            populateTeamsCombo();
            MainWindow.MWInstance.StopProgressWatchTimer();
            IsEnabled = true;
        }

        private async void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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
            _changingTimeframe = false;
            await updateData();
        }

        private async void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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
            _changingTimeframe = false;
            await updateData();
        }

        private async void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                await updateData();
            }
        }

        private void populateTeamsCombo()
        {
            var teams = (MainWindow.TST.Where(kvp => !kvp.Value.IsHidden).Select(kvp => kvp.Value.DisplayName)).ToList();
            teams.Sort();
            teams.Insert(0, "- Any -");

            cmbTeam.ItemsSource = teams;
            cmbTeam_SelectionChanged(null, null);
        }

        private async void cmbTFSeason_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

                _changingTimeframe = false;

                if (MainWindow.Tf.SeasonNum != _curSeason || MainWindow.Tf.IsBetween)
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    MainWindow.ChangeSeason(_curSeason);
                    await updateData();
                }
            }
        }

        private async void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                MainWindow.Tf = new Timeframe(_curSeason);
                await updateData();
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            rbCustomTeamOwn.IsChecked = true;

            _changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.Tf.EndDate;
            dtpStart.SelectedDate = MainWindow.Tf.StartDate;
            populateSeasonCombo();
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.Tf.SeasonNum);
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

            populateSituationalsCombo();
        }

        private void anyCustomTotalsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox) sender;

            if (combo.SelectedIndex == -1)
            {
                return;
            }

            cmbCustomPerGame.SelectedIndex = -1;
            cmbCustomMetrics.SelectedIndex = -1;
            cmbCustomOther.SelectedIndex = -1;
            cmbCustomTeamPerGame.SelectedIndex = -1;
            cmbCustomTeamMetrics.SelectedIndex = -1;
            cmbCustomTeamOther.SelectedIndex = -1;

            if (!combo.Name.Contains("Team"))
            {
                cmbCustomTeamTotals.SelectedIndex = -1;
            }
            else
            {
                cmbCustomTotals.SelectedIndex = -1;
            }
            cmbCustomOp.SelectedIndex = -1;
        }

        private void anyCustomPerGameCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox) sender;

            if (combo.SelectedIndex == -1)
            {
                return;
            }

            cmbCustomTotals.SelectedIndex = -1;
            cmbCustomMetrics.SelectedIndex = -1;
            cmbCustomOther.SelectedIndex = -1;
            cmbCustomTeamTotals.SelectedIndex = -1;
            cmbCustomTeamMetrics.SelectedIndex = -1;
            cmbCustomTeamOther.SelectedIndex = -1;

            if (!combo.Name.Contains("Team"))
            {
                cmbCustomTeamPerGame.SelectedIndex = -1;
            }
            else
            {
                cmbCustomPerGame.SelectedIndex = -1;
            }
            cmbCustomOp.SelectedIndex = -1;
        }

        private void anyCustomMetricsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox) sender;

            if (combo.SelectedIndex == -1)
            {
                return;
            }

            cmbCustomTotals.SelectedIndex = -1;
            cmbCustomPerGame.SelectedIndex = -1;
            cmbCustomOther.SelectedIndex = -1;
            cmbCustomTeamTotals.SelectedIndex = -1;
            cmbCustomTeamPerGame.SelectedIndex = -1;
            cmbCustomTeamOther.SelectedIndex = -1;

            if (!combo.Name.Contains("Team"))
            {
                cmbCustomTeamMetrics.SelectedIndex = -1;
            }
            else
            {
                cmbCustomMetrics.SelectedIndex = -1;
            }
            cmbCustomOp.SelectedIndex = -1;
        }

        private void anyCustomOtherComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox) sender;

            if (combo.SelectedIndex == -1)
            {
                return;
            }

            cmbCustomTotals.SelectedIndex = -1;
            cmbCustomPerGame.SelectedIndex = -1;
            cmbCustomMetrics.SelectedIndex = -1;
            cmbCustomTeamTotals.SelectedIndex = -1;
            cmbCustomTeamPerGame.SelectedIndex = -1;
            cmbCustomTeamMetrics.SelectedIndex = -1;

            if (!combo.Name.Contains("Team"))
            {
                cmbCustomTeamOther.SelectedIndex = -1;
            }
            else
            {
                cmbCustomOther.SelectedIndex = -1;
            }
            cmbCustomOp.SelectedIndex = -1;
        }

        private void cmbCustomOp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCustomOp.SelectedIndex == -1)
            {
                return;
            }

            cmbCustomTotals.SelectedIndex = -1;
            cmbCustomPerGame.SelectedIndex = -1;
            cmbCustomMetrics.SelectedIndex = -1;
            cmbCustomOther.SelectedIndex = -1;
            cmbCustomTeamTotals.SelectedIndex = -1;
            cmbCustomTeamPerGame.SelectedIndex = -1;
            cmbCustomTeamMetrics.SelectedIndex = -1;
            cmbCustomTeamOther.SelectedIndex = -1;
        }

        private void btnCustomExpAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomComp.SelectedIndex == -1)
            {
                cmbCustomComp.SelectedItem = "All";
            }

            var name = txtCustomName.Text;
            var decimalPoints = txtCustomDP.Text;
            var exp = txtCustomExp.Text;
            var filterType = cmbCustomComp.SelectedItem.ToString();
            var filterValue = txtCustomVal.Text;
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(exp) || String.IsNullOrWhiteSpace(decimalPoints))
            {
                return;
            }

            if (!Tools.CheckForBalancedBracketing(exp))
            {
                MessageBox.Show("The parentheses aren't balanced.");
                return;
            }

            uint temp;
            if (!UInt32.TryParse(decimalPoints, out temp))
            {
                MessageBox.Show("Decimal points must be a non-negative integer.");
                return;
            }

            if (!Tools.IsNumeric(filterValue) && filterType != "All")
            {
                MessageBox.Show("Filter value must be a numeric.");
                return;
            }

            var parts = exp.Split(_splitOn.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var known = false;
                if (part.StartsWith("$$"))
                {
                    if (_numericTSRPropertyNames.Contains(part.Substring(3)))
                    {
                        known = true;
                    }
                }
                else if (part.StartsWith("$"))
                {
                    if (_numericPSRPropertyNames.Contains(part.Substring(1)))
                    {
                        known = true;
                    }
                }
                else if (Tools.IsNumeric(part))
                {
                    known = true;
                }

                if (!known)
                {
                    MessageBox.Show("Unknown property: " + part);
                    return;
                }
            }

            _customExpressions.Add(name + ": " + exp + ": " + filterType + " " + filterValue + ": " + decimalPoints);

            refreshCustomExpressionList();

            txtCustomName.Text = "";
            txtCustomExp.Text = "";
            txtCustomDP.Text = "";
            txtCustomVal.Text = "";
        }

        private void refreshCustomExpressionList()
        {
            lstCustom.ItemsSource = null;
            lstCustom.ItemsSource = _customExpressions;
        }

        private void btnCustomAdd_Click(object sender, RoutedEventArgs e)
        {
            var playerCombos = new List<ComboBox> { cmbCustomTotals, cmbCustomPerGame, cmbCustomMetrics, cmbCustomOther };
            var teamCombos = new List<ComboBox> { cmbCustomTeamTotals, cmbCustomTeamPerGame, cmbCustomTeamMetrics, cmbCustomTeamOther };
            var combosToCheck = new List<ComboBox> { cmbCustomOp };
            combosToCheck.AddRange(playerCombos);
            combosToCheck.AddRange(teamCombos);

            if (combosToCheck.All(cmb => cmb.SelectedIndex == -1))
            {
                return;
            }

            if (cmbCustomOp.SelectedIndex != -1)
            {
                txtCustomExp.Text += cmbCustomOp.SelectedItem;
            }
            else if (playerCombos.Any(cmb => cmb.SelectedIndex != -1))
            {
                txtCustomExp.Text += "$";
                txtCustomExp.Text += playerCombos.Single(cmb => cmb.SelectedIndex != -1).SelectedItem;
            }
            else if (teamCombos.Any(cmb => cmb.SelectedIndex != -1))
            {
                txtCustomExp.Text += rbCustomTeamOwn.IsChecked.GetValueOrDefault() ? "$$t" : "$$o";
                txtCustomExp.Text += teamCombos.Single(cmb => cmb.SelectedIndex != -1).SelectedItem;
            }
        }

        private void btnCustomExpDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstCustom.SelectedItems.Count == 1)
            {
                var parts = lstCustom.SelectedItem.ToString().Split(new[] { ": " }, StringSplitOptions.None);
                txtCustomName.Text = parts[0];
                txtCustomExp.Text = parts[1];
                var filterParts = parts[2].Split(new[] { ' ' }, 2);
                cmbCustomComp.SelectedItem = filterParts[0];
                txtCustomVal.Text = filterParts.Length == 2 ? filterParts[1] : "";
                txtCustomDP.Text = parts[3];
            }

            foreach (var item in lstCustom.SelectedItems.Cast<string>().ToList())
            {
                _customExpressions.Remove(item);
            }

            refreshCustomExpressionList();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Tools.SetRegistrySetting("APSHeight", Height);
            Tools.SetRegistrySetting("APSWidth", Width);
            Tools.SetRegistrySetting("APSX", Left);
            Tools.SetRegistrySetting("APSY", Top);
        }

        private async void cmbSituationalSea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await prepareSituationalSeason();
        }

        private async Task prepareSituationalSeason()
        {
            txbStartingPGSea.Text = "";
            txbStartingSGSea.Text = "";
            txbStartingSFSea.Text = "";
            txbStartingPFSea.Text = "";
            txbStartingCSea.Text = "";
            txbSubsSea.Text = "";

            if (cmbSituationalSea.SelectedIndex == -1 || _psrViewSea == null || _psrViewSea.IsEmpty)
            {
                return;
            }

            txbStartingPGSea.Text = "Please wait...";
            await calculateSituational(cmbSituationalSea.SelectedItem.ToString(), false);
        }

        private async void cmbSituationalPl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await prepareSituationalPlayoffs();
        }

        private async Task prepareSituationalPlayoffs()
        {
            txbStartingPGPl.Text = "";
            txbStartingSGPl.Text = "";
            txbStartingSFPl.Text = "";
            txbStartingPFPl.Text = "";
            txbStartingCPl.Text = "";
            txbSubsPl.Text = "";

            if (cmbSituationalPl.SelectedIndex == -1 || _psrViewPl == null || _psrViewPl.IsEmpty)
            {
                return;
            }

            txbStartingPGPl.Text = "Please wait...";
            await calculateSituational(cmbSituationalPl.SelectedItem.ToString(), true);
        }

        private async Task calculateSituational(string property, bool playoffs)
        {
            var nudSituational = !playoffs ? nudSituationalSea : nudSituationalPl;
            var psrList = !playoffs ? _psrViewSea.Cast<PlayerStatsRow>().ToList() : _psrViewPl.Cast<PlayerStatsRow>().ToList();

            _fixingPageValue = true;
            if (nudSituational.Value == null || nudSituational.Value < 1)
            {
                nudSituational.Value = 1;
            }
            if (((nudSituational.Value - 1) * 5) + 1 > psrList.Count)
            {
                nudSituational.Value = ((psrList.Count - 1) / 5) + 1;
            }
            _fixingPageValue = false;

            var txbStartingPG = !playoffs ? txbStartingPGSea : txbStartingPGPl;
            var txbStartingSG = !playoffs ? txbStartingSGSea : txbStartingSGPl;
            var txbStartingSF = !playoffs ? txbStartingSFSea : txbStartingSFPl;
            var txbStartingPF = !playoffs ? txbStartingPFSea : txbStartingPFPl;
            var txbStartingC = !playoffs ? txbStartingCSea : txbStartingCPl;
            var txbSubs = !playoffs ? txbSubsSea : txbSubsPl;

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            if (_customMetricNames.Contains(property))
            {
                property = "Custom[" + _customMetricNames.IndexOf(property) + "]";
            }
            else
            {
                property = property.Replace("%", "p").Replace("3", "T");
            }

            if (property != _lastPropertyUsed || _lastPropertyUsed == "")
            {
                await Task.Run(() => recalculatePermutations(property, psrList));
                _lastPropertyUsed = property;
            }

            var permutations = _permutations.ToList();

            var usedIDs = new List<int>();
            var permToGet = nudSituational.Value;
            int i;
            try
            {
                for (i = 0; i < permToGet; i++)
                {
                    permutations.RemoveAll(p => usedIDs.Any(id => p.IDList.Contains(id)));
                    StartingFivePermutation curPerm;
                    try
                    {
                        curPerm =
                            permutations.OrderByDescending(perm => perm.Sum)
                                        .ThenByDescending(perm => perm.PlayersInPrimaryPosition)
                                        .First();
                    }
                    catch (InvalidOperationException)
                    {
                        if (permToGet > 1)
                        {
                            _fixingPageValue = true;
                            nudSituational.Value = --permToGet;
                            _fixingPageValue = false;
                            i = -1;
                            permutations = _permutations.ToList();
                            usedIDs.Clear();
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    usedIDs.AddRange(curPerm.IDList);
                    tempList.Clear();
                    tempList.AddRange(curPerm.IDList.Select(id => psrList.Single(row => row.ID == id)).ToList());
                }
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                psr1 = tempList[0];
                text = psr1.GetBestStats(5);
                txbStartingPG.Text = "PG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[1];
                text = psr1.GetBestStats(5);
                txbStartingSG.Text = "SG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[2];
                text = psr1.GetBestStats(5);
                txbStartingSF.Text = "SF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[3];
                text = psr1.GetBestStats(5);
                txbStartingPF.Text = "PF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            try
            {
                psr1 = tempList[4];
                text = psr1.GetBestStats(5);
                txbStartingC.Text = "C: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }

            var idCountToSkip = usedIDs.Count;
            i = 0;
            try
            {
                while (usedIDs.Contains(_pgList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(_pgList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute PG: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(_sgList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(_sgList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute SG: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(_sfList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(_sfList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute SF: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(_pfList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(_pfList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute PF: " + ex.Message);
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(_cList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(_cList[i].ID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's substitute C: " + ex.Message);
            }

            try
            {
                var count = usedIDs.Count - idCountToSkip + 5;
                for (var j = count + 1; j <= 12; j++)
                {
                    i = 0;
                    while (usedIDs.Contains(psrList[i].ID))
                    {
                        i++;
                    }
                    usedIDs.Add(psrList[i].ID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown while trying to calculate ultimate team's other substitutes: " + ex.Message);
            }

            usedIDs.Skip(idCountToSkip).ToList().ForEach(id => tempList.Add(psrList.Single(row => row.ID == id)));
            var displayText = "Subs: ";
            for (i = 5; i < usedIDs.Count - idCountToSkip + 5; i++)
            {
                psr1 = tempList[i];
                //text = psr1.GetBestStats(5);
                displayText += psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + "), ";
            }
            displayText = displayText.TrimEnd(new[] { ' ', ',' });
            txbSubs.Text = displayText;
        }

        private void recalculatePermutations(string property, List<PlayerStatsRow> psrList)
        {
            _pgList = psrList.Where(row => (row.Position1S == "PG" || row.Position2S == "PG"))
                // && row.IsInjured == false)
                             .Take(10).ToList();
            _pgList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            _pgList.Reverse();
            _sgList = psrList.Where(row => (row.Position1S == "SG" || row.Position2S == "SG"))
                // && row.IsInjured == false)
                             .Take(10).ToList();
            _sgList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            _sgList.Reverse();
            _sfList = psrList.Where(row => (row.Position1S == "SF" || row.Position2S == "SF"))
                // && row.IsInjured == false)
                             .Take(10).ToList();
            _sfList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            _sfList.Reverse();
            _pfList = psrList.Where(row => (row.Position1S == "PF" || row.Position2S == "PF"))
                // && row.IsInjured == false)
                             .Take(10).ToList();
            _pfList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            _pfList.Reverse();
            _cList = psrList.Where(row => (row.Position1S == "C" || row.Position2S == "C"))
                // && row.IsInjured == false)
                            .Take(10).ToList();
            _cList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            _cList.Reverse();
            _permutations = new List<StartingFivePermutation>();

            var max = Double.MinValue;
            foreach (var pg in _pgList)
            {
                foreach (var sg in _sgList)
                {
                    foreach (var sf in _sfList)
                    {
                        foreach (var pf in _pfList)
                        {
                            foreach (var c in _cList)
                            {
                                double sum = 0;
                                var pInP = 0;
                                var perm = new List<int>(5) { pg.ID };
                                sum += pg.GetValue<double>(property);
                                if (pg.Position1S == "PG")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(sg.ID))
                                {
                                    continue;
                                }
                                perm.Add(sg.ID);
                                sum += sg.GetValue<double>(property);
                                if (sg.Position1S == "SG")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(sf.ID))
                                {
                                    continue;
                                }
                                perm.Add(sf.ID);
                                sum += sf.GetValue<double>(property);
                                if (sf.Position1S == "SF")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(pf.ID))
                                {
                                    continue;
                                }
                                perm.Add(pf.ID);
                                sum += pf.GetValue<double>(property);
                                if (pf.Position1S == "PF")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(c.ID))
                                {
                                    continue;
                                }
                                perm.Add(c.ID);
                                sum += c.GetValue<double>(property);
                                if (c.Position1S == "C")
                                {
                                    pInP++;
                                }

                                if (sum > max)
                                {
                                    max = sum;
                                }

                                _permutations.Add(
                                    new StartingFivePermutation { IDList = perm, PlayersInPrimaryPosition = pInP, Sum = sum });
                            }
                        }
                    }
                }
            }
        }

        private void populateSituationalsCombo()
        {
            var situationals = new List<string>();
            var temp = new List<string>();
            temp.AddRange(PlayerStatsHelper.MetricsNames);
            temp.AddRange(PlayerStatsHelper.PerGame.Values);
            temp.AddRange(PlayerStatsHelper.Totals.Values);
            var psrProps = typeof(PlayerStatsRow).GetProperties().Select(prop => prop.Name).ToList();
            situationals.AddRange(
                from t in temp let realName = t.Replace("%", "p").Replace("3", "T") where psrProps.Contains(realName) select t);
            situationals.InsertRange(0, _customMetricNames);
            cmbSituationalSea.ItemsSource = situationals;
            cmbSituationalPl.ItemsSource = cmbSituationalSea.ItemsSource;
            cmbSituationalSea.SelectedIndex = 0;
            cmbSituationalPl.SelectedIndex = 0;
        }

        private void nudSituationalSea_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_fixingPageValue)
            {
                return;
            }

            cmbSituationalSea_SelectionChanged(null, null);
        }

        private void nudSituationalPl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_fixingPageValue)
            {
                return;
            }

            cmbSituationalPl_SelectionChanged(null, null);
        }
    }
}