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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ciloci.Flee;
using LeftosCommonLibrary;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for PlayerSearchWindow.xaml
    /// </summary>
    public partial class PlayerSearchWindow
    {
        private readonly List<string> NumericOptions = new List<string> {"<", "<=", "=", ">=", ">"};
        private readonly List<string> Positions = new List<string> {"Any", " ", "PG", "SG", "SF", "PF", "C"};
        private readonly List<string> StringOptions = new List<string> {"Contains", "Is"};

        private readonly List<string> Totals = new List<string>
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
                                                       "FOUL"
                                                   };

        private readonly List<string> Averages = new List<string>
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
                                                         "FPG"
                                                     }; 

        private readonly List<string> Metrics = new List<string>
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

        private readonly int maxSeason;
        private int curSeason;
        private string folder = App.AppDocsPath + @"\Search Filters";

        public PlayerSearchWindow()
        {
            InitializeComponent();

            cmbFirstNameSetting.ItemsSource = StringOptions;
            cmbFirstNameSetting.SelectedIndex = 0;
            cmbLastNameSetting.ItemsSource = StringOptions;
            cmbLastNameSetting.SelectedIndex = 0;

            cmbPosition1.ItemsSource = Positions;
            cmbPosition1.SelectedIndex = 0;
            cmbPosition2.ItemsSource = Positions;
            cmbPosition2.SelectedIndex = 0;

            curSeason = MainWindow.curSeason;
            PopulateSeasonCombo();
            maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);

            cmbTotalsPar.ItemsSource = Totals;
            cmbTotalsOp.ItemsSource = NumericOptions;
            cmbTotalsOp.SelectedIndex = 3;

            cmbAvgPar.ItemsSource = Averages;
            cmbAvgOp.ItemsSource = NumericOptions;
            cmbAvgOp.SelectedIndex = 3;

            cmbMetricsPar.ItemsSource = Metrics;
            cmbMetricsOp.ItemsSource = NumericOptions;
            cmbMetricsOp.SelectedIndex = 3;

            //chkIsActive.IsChecked = null;
            //cmbTeam.SelectedItem = "- Any -";

            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            foreach (int kvp in MainWindow.tst.Keys)
            {
                if (MainWindow.tst[kvp].displayName == p)
                {
                    return MainWindow.tst[kvp].name;
                }
            }
            return "$$TEAMNOTFOUND: " + p;
        }

        private string GetDisplayNameFromTeam(string p)
        {
            foreach (int kvp in MainWindow.tst.Keys)
            {
                if (MainWindow.tst[kvp].name == p)
                {
                    return MainWindow.tst[kvp].displayName;
                }
            }
            return "$$TEAMNOTFOUND: " + p;
        }

        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1) return;

            curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

            MainWindow.curSeason = curSeason;
            SQLiteIO.LoadSeason();

            List<string> teams =
                (from kvp in MainWindow.TeamOrder where !MainWindow.tst[kvp.Value].isHidden select MainWindow.tst[kvp.Value].displayName).ToList();

            teams.Sort();
            teams.Insert(0, "- Any -");

            cmbTeam.ItemsSource = teams;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            dgvPlayerStats.ItemsSource = null;

            string playersT = "Players";
            if (curSeason != maxSeason) playersT += "S" + curSeason;

            string q = "select * from " + playersT;

            string where = " WHERE ";
            if (!String.IsNullOrWhiteSpace(txtLastName.Text))
            {
                if (cmbLastNameSetting.SelectedItem.ToString() == "Contains")
                {
                    where += "LastName LIKE \"%" + txtLastName.Text + "%\" AND ";
                }
                else
                {
                    where += "LastName LIKE \"" + txtLastName.Text + "\" AND ";
                }
            }

            if (!String.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                if (cmbFirstNameSetting.SelectedItem.ToString() == "Contains")
                {
                    where += "FirstName LIKE \"%" + txtFirstName.Text + "%\" AND ";
                }
                else
                {
                    where += "FirstName LIKE \"" + txtFirstName.Text + "\" AND ";
                }
            }

            if (cmbPosition1.SelectedIndex != -1 && cmbPosition1.SelectedItem.ToString() != "Any")
            {
                where += "Position1 LIKE \"" + cmbPosition1.SelectedItem + "\" AND ";
            }

            if (cmbPosition2.SelectedIndex != -1 && cmbPosition2.SelectedItem.ToString() != "Any")
            {
                where += "Position2 LIKE \"" + cmbPosition2.SelectedItem + "\" AND ";
            }

            if (chkIsActive.IsChecked.GetValueOrDefault())
            {
                where += "isActive LIKE \"True\" AND ";
            }
            else if (chkIsActive.IsChecked != null)
            {
                where += "isActive LIKE \"False\" AND ";
            }

            if (chkIsInjured.IsChecked.GetValueOrDefault())
            {
                where += "isInjured LIKE \"True\" AND ";
            }
            else if (chkIsInjured.IsChecked != null)
            {
                where += "isInjured LIKE \"False\" AND ";
            }

            if (chkIsAllStar.IsChecked.GetValueOrDefault())
            {
                where += "isAllStar LIKE \"True\" AND ";
            }
            else if (chkIsAllStar.IsChecked != null)
            {
                where += "isAllStar LIKE \"False\" AND ";
            }

            if (chkIsChampion.IsChecked.GetValueOrDefault())
            {
                where += "isNBAChampion LIKE \"True\" AND ";
            }
            else if (chkIsChampion.IsChecked != null)
            {
                where += "isNBAChampion LIKE \"False\" AND ";
            }

            if (cmbTeam.SelectedItem != null && !String.IsNullOrEmpty(cmbTeam.SelectedItem.ToString()) &&
                chkIsActive.IsChecked.GetValueOrDefault() && cmbTeam.SelectedItem.ToString() != "- Any -")
            {
                where += "TeamFin LIKE \"" + GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()) + "\" AND ";
            }

            foreach (var item in lstTotals.Items.Cast<string>())
            {
                string filter = item;
                filter = filter.Replace(" REB ", " (OREB+DREB) ");
                filter = filter.Replace("3P", "TP");
                filter = filter.Replace(" TO ", " TOS ");

                where += filter + " AND ";
            }

            foreach (var item in lstAvg.Items.Cast<string>())
            {
                string filter = item;
                string[] parts = filter.Split(' ');
                switch(parts[0])
                {
                    case "PPG":
                        filter = filter.Replace("PPG", "cast(PTS as REAL)/GP");
                        break;

                    case "FG%":
                        filter = filter.Replace("FG%", "cast(FGM as REAL)/FGA");
                        break;

                    case "FGeff":
                        filter = filter.Replace("FGeff", "(cast(FGM as REAL)/FGA)*FGM/GP");
                        break;

                    case "3P%":
                        filter = filter.Replace("3P%", "cast(TPM as REAL)/TPA");
                        break;

                    case "3Peff":
                        filter = filter.Replace("3Peff", "(cast(TPM as REAL)/TPA)*TPM/GP");
                        break;

                    case "FT%":
                        filter = filter.Replace("FT%", "cast(FTM as REAL)/FTA");
                        break;

                    case "FTeff":
                        filter = filter.Replace("FTeff", "(cast(FTM as REAL)/FTA)*FTM/GP");
                        break;

                    case "ORPG":
                        filter = filter.Replace("ORPG", "cast(OREB as REAL)/GP");
                        break;

                    case "RPG":
                        filter = filter.Replace("RPG", "cast((OREB+DREB) as REAL)/GP");
                        break;

                    case "BPG":
                        filter = filter.Replace("BPG", "cast(BLK as REAL)/GP");
                        break;

                    case "APG":
                        filter = filter.Replace("APG", "cast(AST as REAL)/GP");
                        break;

                    case "SPG":
                        filter = filter.Replace("SPG", "cast(STL as REAL)/GP");
                        break;

                    case "TPG":
                        filter = filter.Replace("TPG", "cast(TOS as REAL)/GP");
                        break;

                    case "FPG":
                        filter = filter.Replace("FPG", "cast(FOUL as REAL)/GP");
                        break;
                }

                where += filter + " AND ";
            }

            where = where.Remove(where.Length - 4);

            var db = new SQLiteDatabase(MainWindow.currentDB);
            DataTable res;
            try
            {
                res = db.GetDataTable(q + where);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid query.\n\n" + ex.Message);
                return;
            }

            var psr = new List<PlayerStatsRow>();
            foreach (DataRow dr in res.Rows)
            {
                int id = Tools.getInt(dr, "ID");
                psr.Add(new PlayerStatsRow(MainWindow.pst[id]));
            }

            var psrView = CollectionViewSource.GetDefaultView(psr);
            psrView.Filter = Filter;

            dgvPlayerStats.ItemsSource = psrView;

            string sortColumn;
            foreach (var item in lstMetrics.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("%", "p");
                psrView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }
            foreach (var item in lstAvg.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("3P", "TP");
                sortColumn = sortColumn.Replace("%", "p");
                psrView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }
            foreach (var item in lstTotals.Items.Cast<string>())
            {
                sortColumn = item.Split(' ')[0];
                sortColumn = sortColumn.Replace("3P", "TP");
                sortColumn = sortColumn.Replace("TO", "TOS");
                psrView.SortDescriptions.Add(new SortDescription(sortColumn, ListSortDirection.Descending));
            }

            if (psrView.SortDescriptions.Count == 0)
                psrView.SortDescriptions.Add(new SortDescription("GmSc", ListSortDirection.Descending));

            tbcPlayerSearch.SelectedItem = tabResults;
        }

        private bool Filter(object o)
        {
            var psr = (PlayerStatsRow) o;
            var ps = new PlayerStats(psr);
            bool keep = true;
            Parallel.ForEach(lstMetrics.Items.Cast<string>(), (item, loopState) =>
                                                                  {
                                                                      string[] parts = item.Split(' ');
                                                                      //double val = Convert.ToDouble(parts[2]);
                                                                      ExpressionContext context = new ExpressionContext();
                                                                      var ige = context.CompileGeneric<bool>(ps.metrics[parts[0]] + parts[1] + parts[2]);
                                                                      keep = ige.Evaluate();
                                                                      if (!keep) loopState.Stop();
                                                                  });
            return keep;
        }

        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
        }

        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTeam.SelectedIndex != -1) chkIsActive.IsChecked = true;
        }

        private void chkIsActive_Click(object sender, RoutedEventArgs e)
        {
            if (chkIsActive.IsChecked.GetValueOrDefault()) cmbTeam.SelectedIndex = 0;
            else cmbTeam.SelectedIndex = -1;
        }

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

        private void btnTotalsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstTotals.SelectedIndex == -1) return;

            if (lstTotals.SelectedItems.Count == 1)
            {
                var item = lstTotals.SelectedItem.ToString();
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

        private void btnAvgDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstAvg.SelectedIndex == -1) return;

            if (lstAvg.SelectedItems.Count == 1)
            {
                var item = lstAvg.SelectedItem.ToString();
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

        private void btnMetricsDel_Click(object sender, RoutedEventArgs e)
        {
            if (lstMetrics.SelectedIndex == -1) return;

            if (lstMetrics.SelectedItems.Count == 1)
            {
                var item = lstMetrics.SelectedItem.ToString();
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

        private void dgvPlayerStats_Sorting(object sender, DataGridSortingEventArgs e)
        {
            Helper.EventHandlers.StatColumn_Sorting(e);
        }

        private void btnLoadFilters_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog sfd = new OpenFileDialog
                                     {
                InitialDirectory = Path.GetFullPath(folder),
                Filter = "NST Search Filters (*.nsf)|*.nsf",
                DefaultExt = "nsf"
            };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName)) return;

            int filterCount = lstTotals.Items.Count + lstAvg.Items.Count + lstMetrics.Items.Count;
            if (filterCount > 0)
            {
                MessageBoxResult mbr = MessageBox.Show("Do you want to clear the current stat filters before loading the new ones?",
                                "NBA Stats Tracker", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel) return;
                if (mbr == MessageBoxResult.Yes)
                {
                    lstTotals.Items.Clear();
                    lstAvg.Items.Clear();
                    lstMetrics.Items.Clear();
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
                        cmbTeam.SelectedItem = GetDisplayNameFromTeam(parts[1]);
                        break;

                    case "Season":
                        cmbSeasonNum.SelectedItem = MainWindow.GetSeasonName(Convert.ToInt32(parts[1]));
                        break;

                    case "Totals":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("TotalsEND")) break;

                            lstTotals.Items.Add(line);
                        }
                        break;

                    case "Avg":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("AvgEND")) break;

                            lstAvg.Items.Add(line);
                        }
                        break;

                    case "Metrics":
                        while (true)
                        {
                            string line = s[++i];
                            if (line.StartsWith("MetricsEND")) break;

                            lstMetrics.Items.Add(line);
                        }
                        break;
                }
            }
        }

        private void btnSaveFilters_Click(object sender, RoutedEventArgs e)
        {
            string s = "";
            s += String.Format("LastName\t{0}\t{1}\n", cmbLastNameSetting.SelectedItem, txtLastName.Text);
            s += String.Format("FirstName\t{0}\t{1}\n", cmbFirstNameSetting.SelectedItem, txtFirstName.Text);
            s += String.Format("Position\t{0}\t{1}\n", cmbPosition1.SelectedItem, cmbPosition2.SelectedItem);
            s += String.Format("Active\t{0}\n", chkIsActive.IsChecked.ToString());
            s += String.Format("Injured\t{0}\n", chkIsInjured.IsChecked.ToString());
            s += String.Format("AllStar\t{0}\n", chkIsAllStar.IsChecked.ToString());
            s += String.Format("Champion\t{0}\n", chkIsChampion.IsChecked.ToString());
            if (cmbTeam.SelectedItem != null)
                s += String.Format("Team\t{0}\n", GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()));
            s += String.Format("Season\t{0}\n", curSeason);
            s += String.Format("Totals\n");
            foreach (var item in lstTotals.Items.Cast<string>())
            {
                s += item + "\n";
            }
            s += "TotalsEND\n";
            s += String.Format("Avg\n");
            foreach (var item in lstAvg.Items.Cast<string>())
            {
                s += item + "\n";
            }
            s += "AvgEND\n";
            s += String.Format("Metrics\n");
            foreach (var item in lstMetrics.Items.Cast<string>())
            {
                s += item + "\n";
            }
            s += "MetricsEND\n";

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            SaveFileDialog sfd = new SaveFileDialog
                                     {
                                         InitialDirectory = Path.GetFullPath(folder),
                                         Filter = "NST Search Filters (*.nsf)|*.nsf",
                                         DefaultExt = "nsf"
                                     };

            sfd.ShowDialog();

            if (String.IsNullOrWhiteSpace(sfd.FileName)) return;

            File.WriteAllText(sfd.FileName, s);
        }
    }
}