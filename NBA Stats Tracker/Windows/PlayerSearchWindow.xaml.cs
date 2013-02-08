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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ciloci.Flee;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.Miscellaneous;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Allows the user to search for players fulfilling any combination of user-specified criteria.
    /// </summary>
    public partial class PlayerSearchWindow
    {
        private readonly List<string> _contractOptions = new List<string> {"Any", "None", "Team", "Player", "Team2Yr"};
        private readonly string _folder = App.AppDocsPath + @"\Search Filters";

        private readonly List<string> _metrics = new List<string>
                                                 {
                                                     "PER",
                                                     "EFF",
                                                     "GmSc",
                                                     "TS%",
                                                     "PPR",
                                                     "OREB%",
                                                     "DREB%",
                                                     "AST%",
                                                     "STL%",
                                                     "BLK%",
                                                     "TO%",
                                                     "USG%",
                                                     "PTSR",
                                                     "REBR",
                                                     "OREBR",
                                                     "ASTR",
                                                     "BLKR",
                                                     "STLR",
                                                     "TOR",
                                                     "FTR"
                                                 };

        private readonly List<string> _numericOptions = new List<string> {"<", "<=", "=", ">=", ">"};

        private readonly List<string> _perGame = new List<string>
                                                 {
                                                     "PPG",
                                                     "FG%",
                                                     "FGeff",
                                                     "3P%",
                                                     "3Peff",
                                                     "FT%",
                                                     "FTeff",
                                                     "RPG",
                                                     "ORPG",
                                                     "APG",
                                                     "SPG",
                                                     "BPG",
                                                     "TPG",
                                                     "FPG",
                                                     "MPG"
                                                 };

        private readonly List<string> _positions = new List<string> {"Any", "None", "PG", "SG", "SF", "PF", "C"};
        private readonly List<string> _stringOptions = new List<string> {"Contains", "Is"};

        private readonly List<string> _totals = new List<string>
                                                {
                                                    "GP",
                                                    "GS",
                                                    "PTS",
                                                    "FGM",
                                                    "FGA",
                                                    "3PM",
                                                    "3PA",
                                                    "FTM",
                                                    "FTA",
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerSearchWindow" /> class.
        ///     Prepares the window by populating all the combo-boxes.
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

            cmbYOBOp.ItemsSource = _numericOptions;
            cmbYOBOp.SelectedIndex = 3;

            cmbYearsProOp.ItemsSource = _numericOptions;
            cmbYearsProOp.SelectedIndex = 3;

            _curSeason = MainWindow.CurSeason;
            populateSeasonCombo();
            SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);

            cmbTotalsPar.ItemsSource = _totals;
            cmbTotalsOp.ItemsSource = _numericOptions;
            cmbTotalsOp.SelectedIndex = 3;

            cmbAvgPar.ItemsSource = _perGame;
            cmbAvgOp.ItemsSource = _numericOptions;
            cmbAvgOp.SelectedIndex = 3;

            cmbMetricsPar.ItemsSource = _metrics;
            cmbMetricsOp.ItemsSource = _numericOptions;
            cmbMetricsOp.SelectedIndex = 3;

            for (int i = 1; i <= 7; i++)
            {
                cmbContractPar.Items.Add("Year " + i);
            }
            cmbContractOp.ItemsSource = _numericOptions;
            cmbContractOp.SelectedIndex = 1;

            cmbHeightOp.ItemsSource = _numericOptions;
            cmbHeightOp.SelectedIndex = 3;

            cmbWeightOp.ItemsSource = _numericOptions;
            cmbWeightOp.SelectedIndex = 3;

            cmbContractYLeftOp.ItemsSource = _numericOptions;
            cmbContractYLeftOp.SelectedIndex = 1;

            cmbContractOpt.ItemsSource = _contractOptions;
            cmbContractOpt.SelectedIndex = 0;

            //chkIsActive.IsChecked = null;
            //cmbTeam.SelectedItem = "- Any -";

            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
        }

        /// <summary>
        ///     Finds a team's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private int GetTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(MainWindow.TST, displayName);
        }

        /// <summary>
        ///     Populates the season combo.
        /// </summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;
            cmbTFSeason.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = _curSeason;
            cmbTFSeason.SelectedValue = _curSeason;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control.
        ///     Loads all the team and player information for the newly selected season, and refreshes the teams combo-box.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
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

                List<string> teams = (from kvp in MainWindow.TeamOrder
                                      where !MainWindow.TST[kvp.Value].IsHidden
                                      select MainWindow.TST[kvp.Value].DisplayName).ToList();

                teams.Sort();
                teams.Insert(0, "- Any -");

                cmbTeam.ItemsSource = teams;
                cmbTeam_SelectionChanged(sender, null);
                _changingTimeframe = false;
            }
        }

        /// <summary>
        ///     Handles the Click event of the btnSearch control.
        ///     Implements the searching algorithm by accumulating the criteria and filtering the available players based on those criteria.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgvPlayerStats.ItemsSource = null;
            Dictionary<int, PlayerStats> pst = MainWindow.PST;
            IEnumerable<KeyValuePair<int, PlayerStats>> filteredPST = pst.AsEnumerable();

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
                filteredPST = filteredPST.Where(pair => pair.Value.Position1.ToString() == cmbPosition1.SelectedItem.ToString());
            }

            if (cmbPosition2.SelectedIndex != -1 && cmbPosition2.SelectedItem.ToString() != "Any")
            {
                filteredPST = filteredPST.Where(pair => pair.Value.Position2.ToString() == cmbPosition2.SelectedItem.ToString());
            }

            if (cmbContractOpt.SelectedIndex != -1 && cmbContractOpt.SelectedItem.ToString() != "Any")
            {
                filteredPST = filteredPST.Where(pair => pair.Value.Contract.Option.ToString() == cmbContractOpt.SelectedItem.ToString());
            }

            if (chkIsActive.IsChecked.GetValueOrDefault())
            {
                filteredPST = filteredPST.Where(pair => pair.Value.IsActive);
            }
            else if (chkIsActive.IsChecked != null)
            {
                filteredPST = filteredPST.Where(pair => !pair.Value.IsActive);
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

            if (cmbTeam.SelectedItem != null && !String.IsNullOrEmpty(cmbTeam.SelectedItem.ToString()) &&
                chkIsActive.IsChecked.GetValueOrDefault() && cmbTeam.SelectedItem.ToString() != "- Any -")
            {
                filteredPST = filteredPST.Where(pair => pair.Value.TeamF == GetTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString()));
            }

            var psrList = new List<PlayerStatsRow>();
            var plPSRList = new List<PlayerStatsRow>();
            foreach (var ps in filteredPST.ToDictionary(ps => ps.Value.ID, ps => ps.Value).Values)
            {
                psrList.Add(new PlayerStatsRow(ps));
                plPSRList.Add(new PlayerStatsRow(ps, true));
            }

            ICollectionView psrView = CollectionViewSource.GetDefaultView(psrList);
            psrView.Filter = filter;

            ICollectionView plPSRView = CollectionViewSource.GetDefaultView(plPSRList);
            plPSRView.Filter = filter;

            dgvPlayerStats.ItemsSource = psrView;
            dgvPlayoffStats.ItemsSource = plPSRView;

            string sortColumn;
            foreach (var item in lstMetrics.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("%", "p");
                psrView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
                plPSRView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }
            foreach (var item in lstAvg.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("3P", "TP");
                sortColumn = sortColumn.Replace("%", "p");
                psrView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
                plPSRView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }
            foreach (var item in lstTotals.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("3P", "TP");
                sortColumn = sortColumn.Replace("TO", "TOS");
                psrView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
                plPSRView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }

            if (psrView.SortDescriptions.Count == 0)
            {
                psrView.SortDescriptions.Add(new SortDescription("GmSc", ListSortDirection.Descending));
                plPSRView.SortDescriptions.Add(new SortDescription("GmSc", ListSortDirection.Descending));
            }

            tbcPlayerSearch.SelectedItem = tabResults;
        }

        /// <summary>
        ///     Used to filter the PlayerStatsRow results based on any user-specified metric stats criteria.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns></returns>
        private bool filter(object o)
        {
            var psr = (PlayerStatsRow) o;
            var ps = new PlayerStats(psr);
            bool keep = true;
            var context = new ExpressionContext();
            IGenericExpression<bool> ige;
            if (!String.IsNullOrWhiteSpace(txtYearsProVal.Text))
            {
                ige = context.CompileGeneric<bool>(psr.YearsPro.ToString() + cmbYearsProOp.SelectedItem + txtYearsProVal.Text);
                if (ige.Evaluate() == false)
                    return false;
            }
            if (!String.IsNullOrWhiteSpace(txtYOBVal.Text))
            {
                context = new ExpressionContext();
                ige = context.CompileGeneric<bool>(psr.YearOfBirth.ToString() + cmbYOBOp.SelectedItem + txtYOBVal.Text);
                if (ige.Evaluate() == false)
                    return false;
            }
            if (!String.IsNullOrWhiteSpace(txtHeightVal.Text))
            {
                double metricHeight = MainWindow.IsImperial
                                          ? PlayerStatsRow.ConvertImperialHeightToMetric(txtHeightVal.Text)
                                          : Convert.ToDouble(txtHeightVal.Text);
                context = new ExpressionContext();
                ige = context.CompileGeneric<bool>(psr.Height.ToString() + cmbHeightOp.SelectedItem + metricHeight.ToString());
                if (ige.Evaluate() == false)
                    return false;
            }
            if (!String.IsNullOrWhiteSpace(txtWeightVal.Text))
            {
                double imperialWeight = MainWindow.IsImperial
                                            ? Convert.ToDouble(txtWeightVal.Text)
                                            : PlayerStatsRow.ConvertMetricWeightToImperial(txtWeightVal.Text);
                context = new ExpressionContext();
                ige = context.CompileGeneric<bool>(psr.Weight.ToString() + cmbWeightOp.SelectedItem + imperialWeight.ToString());
                if (ige.Evaluate() == false)
                    return false;
            }
            if (!String.IsNullOrWhiteSpace(txtContractYLeftVal.Text))
            {
                context = new ExpressionContext();
                ige =
                    context.CompileGeneric<bool>((chkExcludeOption.IsChecked.GetValueOrDefault()
                                                      ? psr.ContractYearsMinusOption.ToString()
                                                      : psr.ContractYears.ToString()) + cmbContractYLeftOp.SelectedItem +
                                                 txtContractYLeftVal.Text);
                if (ige.Evaluate() == false)
                    return false;
            }

            foreach (var contractYear in lstContract.Items.Cast<string>())
            {
                string[] parts = contractYear.Split(' ');
                ige =
                    context.CompileGeneric<bool>(psr.GetType().GetProperty("ContractY" + parts[1]).GetValue(psr, null) + parts[2] + parts[3]);
                keep = ige.Evaluate();
                if (!keep)
                    return keep;
            }

            Parallel.ForEach(lstTotals.Items.Cast<string>(), (item, loopState) =>
                                                             {
                                                                 string[] parts = item.Split(' ');
                                                                 parts[0] = parts[0].Replace("3P", "TP");
                                                                 parts[0] = parts[0].Replace("TO", "TOS");
                                                                 context = new ExpressionContext();
                                                                 ige =
                                                                     context.CompileGeneric<bool>(
                                                                         psr.GetType().GetProperty(parts[0]).GetValue(psr, null) + parts[1] +
                                                                         parts[2]);
                                                                 keep = ige.Evaluate();
                                                                 if (!keep)
                                                                     loopState.Stop();
                                                             });

            if (!keep)
                return keep;

            Parallel.ForEach(lstAvg.Items.Cast<string>(), (item, loopState) =>
                                                          {
                                                              string[] parts = item.Split(' ');
                                                              parts[0] = parts[0].Replace("3P", "TP");
                                                              parts[0] = parts[0].Replace("%", "p");
                                                              context = new ExpressionContext();
                                                              object value = psr.GetType().GetProperty(parts[0]).GetValue(psr, null);
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
                                                                  loopState.Stop();
                                                          });

            if (!keep)
                return keep;

            Parallel.ForEach(lstMetrics.Items.Cast<string>(), (item, loopState) =>
                                                              {
                                                                  string[] parts = item.Split(' ');
                                                                  parts[0] = parts[0].Replace("%", "p");
                                                                  //double val = Convert.ToDouble(parts[2]);
                                                                  context = new ExpressionContext();
                                                                  if (!double.IsNaN(ps.Metrics[parts[0]]))
                                                                  {
                                                                      ige =
                                                                          context.CompileGeneric<bool>(ps.Metrics[parts[0]] + parts[1] +
                                                                                                       parts[2]);
                                                                      keep = ige.Evaluate();
                                                                  }
                                                                  else
                                                                  {
                                                                      keep = false;
                                                                  }
                                                                  if (!keep)
                                                                      loopState.Stop();
                                                              });
            return keep;
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

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbTeam control.
        ///     Switches the IsActive criterion to true if a team is selected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTeam.SelectedIndex != -1)
                chkIsActive.IsChecked = true;
        }

        /// <summary>
        ///     Handles the Click event of the chkIsActive control.
        ///     Switches the currently selected team to the "Any" option if the IsActive criterion is set to true; otherwise, resets the teams combo-box to no selection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault())
                cmbTeam.SelectedIndex = 0;
            else
                cmbTeam.SelectedIndex = -1;
        }

        /// <summary>
        ///     Handles the Click event of the btnTotalsAdd control.
        ///     Adds a total stats filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnTotalsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTotalsPar.SelectedIndex == -1 || cmbTotalsOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtTotalsVal.Text))
                return;

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

        /// <summary>
        ///     Handles the Click event of the btnTotalsDel control.
        ///     Deletes a total stats filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnTotalsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.SelectedIndex == -1)
                return;

            if (lstTotals.SelectedItems.Count == 1)
            {
                string item = lstTotals.SelectedItem.ToString();
                lstTotals.Items.Remove(item);
                string[] parts = item.Split(' ');
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

        /// <summary>
        ///     Handles the Click event of the btnAvgAdd control.
        ///     Adds an average stats filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnAvgAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbAvgPar.SelectedIndex == -1 || cmbAvgOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtAvgVal.Text))
                return;

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

        /// <summary>
        ///     Handles the Click event of the btnAvgDel control.
        ///     Deletes an average stats filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnAvgDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvg.SelectedIndex == -1)
                return;

            if (lstAvg.SelectedItems.Count == 1)
            {
                string item = lstAvg.SelectedItem.ToString();
                lstAvg.Items.Remove(item);
                string[] parts = item.Split(' ');
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

        /// <summary>
        ///     Handles the Click event of the btnMetricsAdd control.
        ///     Adds a metric stats filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnMetricsAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMetricsPar.SelectedIndex == -1 || cmbMetricsOp.SelectedIndex == -1 || String.IsNullOrWhiteSpace(txtMetricsVal.Text))
                return;

            try
            {
                Convert.ToSingle(txtMetricsVal.Text);
            }
            catch
            {
                return;
            }

            lstMetrics.Items.Add(cmbMetricsPar.SelectedItem + " " + cmbMetricsOp.SelectedItem + " " + txtMetricsVal.Text);
            cmbMetricsPar.SelectedIndex = -1;
            txtMetricsVal.Text = "";
        }

        /// <summary>
        ///     Handles the Click event of the btnMetricsDel control.
        ///     Deletes a metric stats filter.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnMetricsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstMetrics.SelectedIndex == -1)
                return;

            if (lstMetrics.SelectedItems.Count == 1)
            {
                string item = lstMetrics.SelectedItem.ToString();
                lstMetrics.Items.Remove(item);
                string[] parts = item.Split(' ');
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
        ///     Handles the Sorting event of the dgvPlayerStats control.
        ///     Uses a custom Sorting event handler that sorts a stat in descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvPlayerStats_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
        }

        /// <summary>
        ///     Handles the Click event of the btnLoadFilters control.
        ///     Loads the filters from a previously saved filters file.
        /// </summary>
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
                return;

            int filterCount = lstTotals.Items.Count + lstAvg.Items.Count + lstMetrics.Items.Count;
            if (filterCount > 0)
            {
                MessageBoxResult mbr = MessageBox.Show("Do you want to clear the current stat filters before loading the new ones?",
                                                       "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel)
                    return;
                if (mbr == MessageBoxResult.Yes)
                {
                    lstTotals.Items.Clear();
                    lstAvg.Items.Clear();
                    lstMetrics.Items.Clear();
                    lstContract.Items.Clear();
                }
            }

            string[] s = File.ReadAllLines(sfd.FileName);
            for (int i = 0; i < s.Length; i++)
            {
                string[] parts = s[i].Split('\t');
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
                        txtYearsProVal.Text = parts[2];
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

                    case "Season":
                        cmbSeasonNum.SelectedItem = MainWindow.GetSeasonName(Convert.ToInt32(parts[1]));
                        break;

                    case "Totals":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("TotalsEND"))
                                break;

                            lstTotals.Items.Add(line);
                        }
                        break;

                    case "Avg":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("AvgEND"))
                                break;

                            lstAvg.Items.Add(line);
                        }
                        break;

                    case "Metrics":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("MetricsEND"))
                                break;

                            lstMetrics.Items.Add(line);
                        }
                        break;

                    case "Contract":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("ContractEND"))
                                break;

                            lstMetrics.Items.Add(line);
                        }
                        break;

                    case "TF":
                        if (parts[1].ToLowerInvariant() == "true")
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
        }

        /// <summary>
        ///     Handles the Click event of the btnSaveFilters control.
        ///     Saves the current filters to a file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            string s = "";
            s += String.Format("LastName\t{0}\t{1}\n", cmbLastNameSetting.SelectedItem, txtLastName.Text);
            s += String.Format("FirstName\t{0}\t{1}\n", cmbFirstNameSetting.SelectedItem, txtFirstName.Text);
            s += String.Format("Position\t{0}\t{1}\n", cmbPosition1.SelectedItem, cmbPosition2.SelectedItem);
            s += String.Format("YearOfBirth\t{0}\t{1}\n", cmbYOBOp.SelectedItem, txtYOBVal.Text);
            s += String.Format("YearsPro\t{0}\t{1}\n", cmbYearsProOp.SelectedItem, txtYearsProVal.Text);
            s += String.Format("Height\t{0}\t{1}\n", cmbHeightOp.SelectedItem, txtHeightVal.Text);
            s += String.Format("Weight\t{0}\t{1}\n", cmbWeightOp.SelectedItem, txtWeightVal.Text);
            s += String.Format("ContractYLeft\t{0}\t{1}\n", cmbContractYLeftOp.SelectedItem, txtContractYLeftVal.Text);
            s += String.Format("ContractOpt\t{0}\n", cmbContractOpt.SelectedItem);
            s += String.Format("Active\t{0}\n", chkIsActive.IsChecked.ToString());
            s += String.Format("Injured\t{0}\n", chkIsInjured.IsChecked.ToString());
            s += String.Format("AllStar\t{0}\n", chkIsAllStar.IsChecked.ToString());
            s += String.Format("Champion\t{0}\n", chkIsChampion.IsChecked.ToString());
            if (cmbTeam.SelectedItem != null)
                s += String.Format("Team\t{0}\n", GetTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString()));
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
            bool isBetween = rbStatsBetween.IsChecked.GetValueOrDefault();
            s += String.Format("TF\t{0}\t{1}\t{2}", isBetween.ToString(),
                               isBetween ? dtpStart.SelectedDate.GetValueOrDefault().ToString() : cmbTFSeason.SelectedItem.ToString(),
                               isBetween ? dtpEnd.SelectedDate.GetValueOrDefault().ToString() : "");

            if (!Directory.Exists(_folder))
                Directory.CreateDirectory(_folder);

            var sfd = new SaveFileDialog
                      {
                          InitialDirectory = Path.GetFullPath(_folder),
                          Filter = "NST Search Filters (*.nsf)|*.nsf",
                          DefaultExt = "nsf"
                      };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName))
                return;

            try
            {
                File.WriteAllText(sfd.FileName, s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Couldn't save filters file.\n\n", ex.Message);
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

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            if (!_changingTimeframe)
            {
                MainWindow.UpdateAllData();
                populateTeamsCombo();
            }
        }

        private void populateTeamsCombo()
        {
            List<string> teams = (from kvp in MainWindow.TeamOrder
                                  where !MainWindow.TST[kvp.Value].IsHidden
                                  select MainWindow.TST[kvp.Value].DisplayName).ToList();

            teams.Sort();
            teams.Insert(0, "- Any -");

            cmbTeam.ItemsSource = teams;
            cmbTeam_SelectionChanged(null, null);
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

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
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
        }
    }
}