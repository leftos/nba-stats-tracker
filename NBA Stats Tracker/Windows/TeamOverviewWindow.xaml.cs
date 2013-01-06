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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Data.Misc;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.ListExtensions;
using NBA_Stats_Tracker.Helper.Misc;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Shows team information and stats.
    /// </summary>
    public partial class TeamOverviewWindow
    {
        private readonly string teamToLoad;
        private List<TeamBoxScore> bsrList = new List<TeamBoxScore>();
        private bool changingOppRange;
        private bool changingOppTeam;
        private bool changingTimeframe;

        private int curSeason = MainWindow.curSeason;
        private string curTeam;
        private TeamStats curts;
        private TeamStats curtsopp;
        private SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private DataTable dt_bs;
        private DataTable dt_bs_res;
        private DataTable dt_hth;
        private DataTable dt_ov;
        private DataTable dt_ss;
        private DataTable dt_yea;
        private DataView dv_hth;
        private int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        private ObservableCollection<PlayerStatsRow> pl_psrList;
        private int[][] pl_rankings;
        private ObservableCollection<PlayerStatsRow> psrList;
        private Dictionary<int, PlayerStats> pst;
        private int[][] rankings;
        private Dictionary<int, TeamStats> tst;
        private Dictionary<int, TeamStats> tstopp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamOverviewWindow" /> class.
        /// </summary>
        public TeamOverviewWindow()
        {
            InitializeComponent();

            Height = Misc.GetRegistrySetting("TeamOvHeight", (int) Height);
            Width = Misc.GetRegistrySetting("TeamOvWidth", (int) Width);
            Top = Misc.GetRegistrySetting("TeamOvY", (int) Top);
            Left = Misc.GetRegistrySetting("TeamOvX", (int) Left);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamOverviewWindow" /> class.
        /// </summary>
        /// <param name="team">The team to switch to when the window finishes loading.</param>
        public TeamOverviewWindow(string team) : this()
        {
            teamToLoad = team;
        }

        /// <summary>
        /// Populates the teams combo.
        /// </summary>
        private void PopulateTeamsCombo()
        {
            List<string> teams =
                (from kvp in MainWindow.TeamOrder where !tst[kvp.Value].isHidden select tst[kvp.Value].displayName).ToList();

            teams.Sort();

            cmbTeam.ItemsSource = teams;
            cmbOppTeam.ItemsSource = teams;
            cmbOppTeamBest.ItemsSource = teams;
        }

        /// <summary>
        /// Populates the season combo.
        /// </summary>
        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            //cmbSeasonNum.SelectedValue = MainWindow.tf.SeasonNum;
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// Switches to the previous team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == 0)
                cmbTeam.SelectedIndex = cmbTeam.Items.Count - 1;
            else
                cmbTeam.SelectedIndex--;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// Switches to the next team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count - 1)
                cmbTeam.SelectedIndex = 0;
            else
                cmbTeam.SelectedIndex++;
        }

        /// <summary>
        /// Updates the Overview tab and loads the appropriate box scores depending on the timeframe.
        /// </summary>
        private void UpdateOverviewAndBoxScores()
        {
            int i = MainWindow.TeamOrder[curTeam];

            curts = tst[i];
            curtsopp = tstopp[i];

            bsrList = new List<TeamBoxScore>();

            #region Prepare Team Overview

            var boxScoreEntries = MainWindow.bshist.Where(bse => bse.bs.Team1 == curTeam || bse.bs.Team2 == curTeam);

            foreach (var r in boxScoreEntries)
            {
                var bsr = r.bs.DeepClone();
                bsr.PrepareForDisplay(curTeam);
                bsrList.Add(bsr);
            }

            #region Regular Season

            DataRow dr = dt_ov.NewRow();

            dr["Type"] = "Stats";
            dr["Games"] = curts.getGames();
            dr["Wins (W%)"] = curts.winloss[0].ToString();
            dr["Losses (Weff)"] = curts.winloss[1].ToString();
            dr["PF"] = curts.stats[t.PF].ToString();
            dr["PA"] = curts.stats[t.PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = curts.stats[t.FGM].ToString() + "-" + curts.stats[t.FGA].ToString();
            dr["3PT"] = curts.stats[t.TPM].ToString() + "-" + curts.stats[t.TPA].ToString();
            dr["FT"] = curts.stats[t.FTM].ToString() + "-" + curts.stats[t.FTA].ToString();
            dr["REB"] = (curts.stats[t.DREB] + curts.stats[t.OREB]).ToString();
            dr["OREB"] = curts.stats[t.OREB].ToString();
            dr["DREB"] = curts.stats[t.DREB].ToString();
            dr["AST"] = curts.stats[t.AST].ToString();
            dr["TO"] = curts.stats[t.TO].ToString();
            dr["STL"] = curts.stats[t.STL].ToString();
            dr["BLK"] = curts.stats[t.BLK].ToString();
            dr["FOUL"] = curts.stats[t.FOUL].ToString();
            dr["MINS"] = curts.stats[t.MINS].ToString();

            dt_ov.Rows.Add(dr);

            dr = dt_ov.NewRow();

            curts.CalcAvg(); // Just to be sure...

            dr["Type"] = "Averages";
            //dr["Games"] = curts.getGames();
            dr["Wins (W%)"] = String.Format("{0:F3}", curts.averages[t.Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", curts.averages[t.Weff]);
            dr["PF"] = String.Format("{0:F1}", curts.averages[t.PPG]);
            dr["PA"] = String.Format("{0:F1}", curts.averages[t.PAPG]);
            dr["PD"] = String.Format("{0:F1}", curts.averages[t.PD]);
            dr["FG"] = String.Format("{0:F3}", curts.averages[t.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", curts.averages[t.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", curts.averages[t.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", curts.averages[t.TPeff]);
            dr["FT"] = String.Format("{0:F3}", curts.averages[t.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", curts.averages[t.FTeff]);
            dr["REB"] = String.Format("{0:F1}", curts.averages[t.RPG]);
            dr["OREB"] = String.Format("{0:F1}", curts.averages[t.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", curts.averages[t.DRPG]);
            dr["AST"] = String.Format("{0:F1}", curts.averages[t.APG]);
            dr["TO"] = String.Format("{0:F1}", curts.averages[t.TPG]);
            dr["STL"] = String.Format("{0:F1}", curts.averages[t.SPG]);
            dr["BLK"] = String.Format("{0:F1}", curts.averages[t.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", curts.averages[t.FPG]);

            dt_ov.Rows.Add(dr);

            // Rankings can only be shown based on total stats
            // ...for now
            DataRow dr2;
            dr2 = dt_ov.NewRow();

            dr2["Type"] = "Rankings";
            dr2["Wins (W%)"] = rankings[i][t.Wp];
            dr2["Losses (Weff)"] = rankings[i][t.Weff];
            dr2["PF"] = rankings[i][t.PPG];
            dr2["PA"] = cmbTeam.Items.Count + 1 - rankings[i][t.PAPG];
            dr2["PD"] = rankings[i][t.PD];
            dr2["FG"] = rankings[i][t.FGp];
            dr2["FGeff"] = rankings[i][t.FGeff];
            dr2["3PT"] = rankings[i][t.TPp];
            dr2["3Peff"] = rankings[i][t.TPeff];
            dr2["FT"] = rankings[i][t.FTp];
            dr2["FTeff"] = rankings[i][t.FTeff];
            dr2["REB"] = rankings[i][t.RPG];
            dr2["OREB"] = rankings[i][t.ORPG];
            dr2["DREB"] = rankings[i][t.DRPG];
            dr2["AST"] = rankings[i][t.APG];
            dr2["TO"] = cmbTeam.Items.Count + 1 - rankings[i][t.TPG];
            dr2["STL"] = rankings[i][t.SPG];
            dr2["BLK"] = rankings[i][t.BPG];
            dr2["FOUL"] = cmbTeam.Items.Count + 1 - rankings[i][t.FPG];

            dt_ov.Rows.Add(dr2);

            dr2 = dt_ov.NewRow();

            dr2["Type"] = "Opp Stats";
            dr2["Games"] = curtsopp.getGames();
            dr2["Wins (W%)"] = curtsopp.winloss[0].ToString();
            dr2["Losses (Weff)"] = curtsopp.winloss[1].ToString();
            dr2["PF"] = curtsopp.stats[t.PF].ToString();
            dr2["PA"] = curtsopp.stats[t.PA].ToString();
            dr2["PD"] = " ";
            dr2["FG"] = curtsopp.stats[t.FGM].ToString() + "-" + curtsopp.stats[t.FGA].ToString();
            dr2["3PT"] = curtsopp.stats[t.TPM].ToString() + "-" + curtsopp.stats[t.TPA].ToString();
            dr2["FT"] = curtsopp.stats[t.FTM].ToString() + "-" + curtsopp.stats[t.FTA].ToString();
            dr2["REB"] = (curtsopp.stats[t.DREB] + curtsopp.stats[t.OREB]).ToString();
            dr2["OREB"] = curtsopp.stats[t.OREB].ToString();
            dr2["DREB"] = curtsopp.stats[t.DREB].ToString();
            dr2["AST"] = curtsopp.stats[t.AST].ToString();
            dr2["TO"] = curtsopp.stats[t.TO].ToString();
            dr2["STL"] = curtsopp.stats[t.STL].ToString();
            dr2["BLK"] = curtsopp.stats[t.BLK].ToString();
            dr2["FOUL"] = curtsopp.stats[t.FOUL].ToString();
            dr2["MINS"] = curtsopp.stats[t.MINS].ToString();

            dt_ov.Rows.Add(dr2);

            dr2 = dt_ov.NewRow();

            dr2["Type"] = "Opp Avg";
            dr2["Wins (W%)"] = String.Format("{0:F3}", curtsopp.averages[t.Wp]);
            dr2["Losses (Weff)"] = String.Format("{0:F2}", curtsopp.averages[t.Weff]);
            dr2["PF"] = String.Format("{0:F1}", curtsopp.averages[t.PPG]);
            dr2["PA"] = String.Format("{0:F1}", curtsopp.averages[t.PAPG]);
            dr2["PD"] = String.Format("{0:F1}", curtsopp.averages[t.PD]);
            dr2["FG"] = String.Format("{0:F3}", curtsopp.averages[t.FGp]);
            dr2["FGeff"] = String.Format("{0:F2}", curtsopp.averages[t.FGeff]);
            dr2["3PT"] = String.Format("{0:F3}", curtsopp.averages[t.TPp]);
            dr2["3Peff"] = String.Format("{0:F2}", curtsopp.averages[t.TPeff]);
            dr2["FT"] = String.Format("{0:F3}", curtsopp.averages[t.FTp]);
            dr2["FTeff"] = String.Format("{0:F2}", curtsopp.averages[t.FTeff]);
            dr2["REB"] = String.Format("{0:F1}", curtsopp.averages[t.RPG]);
            dr2["OREB"] = String.Format("{0:F1}", curtsopp.averages[t.ORPG]);
            dr2["DREB"] = String.Format("{0:F1}", curtsopp.averages[t.DRPG]);
            dr2["AST"] = String.Format("{0:F1}", curtsopp.averages[t.APG]);
            dr2["TO"] = String.Format("{0:F1}", curtsopp.averages[t.TPG]);
            dr2["STL"] = String.Format("{0:F1}", curtsopp.averages[t.SPG]);
            dr2["BLK"] = String.Format("{0:F1}", curtsopp.averages[t.BPG]);
            dr2["FOUL"] = String.Format("{0:F1}", curtsopp.averages[t.FPG]);

            dt_ov.Rows.Add(dr2);

            #endregion

            #region Playoffs

            dt_ov.Rows.Add(dt_ov.NewRow());

            dr = dt_ov.NewRow();

            dr["Type"] = "Playoffs";
            dr["Games"] = curts.getPlayoffGames();
            dr["Wins (W%)"] = curts.pl_winloss[0].ToString();
            dr["Losses (Weff)"] = curts.pl_winloss[1].ToString();
            dr["PF"] = curts.pl_stats[t.PF].ToString();
            dr["PA"] = curts.pl_stats[t.PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = curts.pl_stats[t.FGM].ToString() + "-" + curts.pl_stats[t.FGA].ToString();
            dr["3PT"] = curts.pl_stats[t.TPM].ToString() + "-" + curts.pl_stats[t.TPA].ToString();
            dr["FT"] = curts.pl_stats[t.FTM].ToString() + "-" + curts.pl_stats[t.FTA].ToString();
            dr["REB"] = (curts.pl_stats[t.DREB] + curts.pl_stats[t.OREB]).ToString();
            dr["OREB"] = curts.pl_stats[t.OREB].ToString();
            dr["DREB"] = curts.pl_stats[t.DREB].ToString();
            dr["AST"] = curts.pl_stats[t.AST].ToString();
            dr["TO"] = curts.pl_stats[t.TO].ToString();
            dr["STL"] = curts.pl_stats[t.STL].ToString();
            dr["BLK"] = curts.pl_stats[t.BLK].ToString();
            dr["FOUL"] = curts.pl_stats[t.FOUL].ToString();
            dr["MINS"] = curts.pl_stats[t.MINS].ToString();

            dt_ov.Rows.Add(dr);

            dr = dt_ov.NewRow();

            dr["Type"] = "Pl Avg";
            dr["Wins (W%)"] = String.Format("{0:F3}", curts.pl_averages[t.Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", curts.pl_averages[t.Weff]);
            dr["PF"] = String.Format("{0:F1}", curts.pl_averages[t.PPG]);
            dr["PA"] = String.Format("{0:F1}", curts.pl_averages[t.PAPG]);
            dr["PD"] = String.Format("{0:F1}", curts.pl_averages[t.PD]);
            dr["FG"] = String.Format("{0:F3}", curts.pl_averages[t.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", curts.pl_averages[t.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", curts.pl_averages[t.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", curts.pl_averages[t.TPeff]);
            dr["FT"] = String.Format("{0:F3}", curts.pl_averages[t.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", curts.pl_averages[t.FTeff]);
            dr["REB"] = String.Format("{0:F1}", curts.pl_averages[t.RPG]);
            dr["OREB"] = String.Format("{0:F1}", curts.pl_averages[t.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", curts.pl_averages[t.DRPG]);
            dr["AST"] = String.Format("{0:F1}", curts.pl_averages[t.APG]);
            dr["TO"] = String.Format("{0:F1}", curts.pl_averages[t.TPG]);
            dr["STL"] = String.Format("{0:F1}", curts.pl_averages[t.SPG]);
            dr["BLK"] = String.Format("{0:F1}", curts.pl_averages[t.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", curts.pl_averages[t.FPG]);

            dt_ov.Rows.Add(dr);

            dr2 = dt_ov.NewRow();

            int count = tst.Count(z => z.Value.getPlayoffGames() > 0);

            dr2["Type"] = "Pl Rank";
            dr2["Wins (W%)"] = pl_rankings[i][t.Wp];
            dr2["Losses (Weff)"] = pl_rankings[i][t.Weff];
            dr2["PF"] = pl_rankings[i][t.PPG];
            dr2["PA"] = count + 1 - pl_rankings[i][t.PAPG];
            dr2["PD"] = pl_rankings[i][t.PD];
            dr2["FG"] = pl_rankings[i][t.FGp];
            dr2["FGeff"] = pl_rankings[i][t.FGeff];
            dr2["3PT"] = pl_rankings[i][t.TPp];
            dr2["3Peff"] = pl_rankings[i][t.TPeff];
            dr2["FT"] = pl_rankings[i][t.FTp];
            dr2["FTeff"] = pl_rankings[i][t.FTeff];
            dr2["REB"] = pl_rankings[i][t.RPG];
            dr2["OREB"] = pl_rankings[i][t.ORPG];
            dr2["DREB"] = pl_rankings[i][t.DRPG];
            dr2["AST"] = pl_rankings[i][t.APG];
            dr2["TO"] = count + 1 - pl_rankings[i][t.TPG];
            dr2["STL"] = pl_rankings[i][t.SPG];
            dr2["BLK"] = pl_rankings[i][t.BPG];
            dr2["FOUL"] = count + 1 - pl_rankings[i][t.FPG];

            dt_ov.Rows.Add(dr2);

            dr2 = dt_ov.NewRow();

            dr2["Type"] = "Opp Pl Stats";
            dr2["Games"] = curtsopp.getPlayoffGames();
            dr2["Wins (W%)"] = curtsopp.pl_winloss[0].ToString();
            dr2["Losses (Weff)"] = curtsopp.pl_winloss[1].ToString();
            dr2["PF"] = curtsopp.pl_stats[t.PF].ToString();
            dr2["PA"] = curtsopp.pl_stats[t.PA].ToString();
            dr2["PD"] = " ";
            dr2["FG"] = curtsopp.pl_stats[t.FGM].ToString() + "-" + curtsopp.pl_stats[t.FGA].ToString();
            dr2["3PT"] = curtsopp.pl_stats[t.TPM].ToString() + "-" + curtsopp.pl_stats[t.TPA].ToString();
            dr2["FT"] = curtsopp.pl_stats[t.FTM].ToString() + "-" + curtsopp.pl_stats[t.FTA].ToString();
            dr2["REB"] = (curtsopp.pl_stats[t.DREB] + curtsopp.pl_stats[t.OREB]).ToString();
            dr2["OREB"] = curtsopp.pl_stats[t.OREB].ToString();
            dr2["DREB"] = curtsopp.pl_stats[t.DREB].ToString();
            dr2["AST"] = curtsopp.pl_stats[t.AST].ToString();
            dr2["TO"] = curtsopp.pl_stats[t.TO].ToString();
            dr2["STL"] = curtsopp.pl_stats[t.STL].ToString();
            dr2["BLK"] = curtsopp.pl_stats[t.BLK].ToString();
            dr2["FOUL"] = curtsopp.pl_stats[t.FOUL].ToString();
            dr2["MINS"] = curtsopp.pl_stats[t.MINS].ToString();

            dt_ov.Rows.Add(dr2);

            dr2 = dt_ov.NewRow();

            dr2["Type"] = "Opp Pl Avg";
            dr2["Wins (W%)"] = String.Format("{0:F3}", curtsopp.pl_averages[t.Wp]);
            dr2["Losses (Weff)"] = String.Format("{0:F2}", curtsopp.pl_averages[t.Weff]);
            dr2["PF"] = String.Format("{0:F1}", curtsopp.pl_averages[t.PPG]);
            dr2["PA"] = String.Format("{0:F1}", curtsopp.pl_averages[t.PAPG]);
            dr2["PD"] = String.Format("{0:F1}", curtsopp.pl_averages[t.PD]);
            dr2["FG"] = String.Format("{0:F3}", curtsopp.pl_averages[t.FGp]);
            dr2["FGeff"] = String.Format("{0:F2}", curtsopp.pl_averages[t.FGeff]);
            dr2["3PT"] = String.Format("{0:F3}", curtsopp.pl_averages[t.TPp]);
            dr2["3Peff"] = String.Format("{0:F2}", curtsopp.pl_averages[t.TPeff]);
            dr2["FT"] = String.Format("{0:F3}", curtsopp.pl_averages[t.FTp]);
            dr2["FTeff"] = String.Format("{0:F2}", curtsopp.pl_averages[t.FTeff]);
            dr2["REB"] = String.Format("{0:F1}", curtsopp.pl_averages[t.RPG]);
            dr2["OREB"] = String.Format("{0:F1}", curtsopp.pl_averages[t.ORPG]);
            dr2["DREB"] = String.Format("{0:F1}", curtsopp.pl_averages[t.DRPG]);
            dr2["AST"] = String.Format("{0:F1}", curtsopp.pl_averages[t.APG]);
            dr2["TO"] = String.Format("{0:F1}", curtsopp.pl_averages[t.TPG]);
            dr2["STL"] = String.Format("{0:F1}", curtsopp.pl_averages[t.SPG]);
            dr2["BLK"] = String.Format("{0:F1}", curtsopp.pl_averages[t.BPG]);
            dr2["FOUL"] = String.Format("{0:F1}", curtsopp.pl_averages[t.FPG]);

            dt_ov.Rows.Add(dr2);

            #endregion

            CreateViewAndUpdateOverview();

            dgvBoxScores.ItemsSource = bsrList;

            #endregion
        }

        /// <summary>
        /// Creates a DataView based on the current overview DataTable and refreshes the DataGrid.
        /// </summary>
        private void CreateViewAndUpdateOverview()
        {
            var dv_ov = new DataView(dt_ov) {AllowNew = false, AllowDelete = false};
            dgvTeamStats.DataContext = dv_ov;
        }

        /// <summary>
        /// Calculates the split stats and updates the split stats tab.
        /// </summary>
        private void UpdateSplitStats()
        {
            var splitTeamStats = MainWindow.splitTeamStats;
            var TeamOrder = MainWindow.TeamOrder;
            var ID = TeamOrder[curTeam];

            DataRow dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[ID]["Home"], ref dr, "Home");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[ID]["Away"], ref dr, "Away");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[ID]["Wins"], ref dr, "Wins");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[ID]["Losses"], ref dr, "Losses");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[ID]["Season"], ref dr, "Season");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(splitTeamStats[ID]["Playoffs"], ref dr, "Playoffs");
            dt_ss.Rows.Add(dr);

            #region Per Opponent

            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);

            foreach (var oppTeam in MainWindow.TeamOrder.Keys)
            {
                if (oppTeam == curTeam)
                    continue;

                dr = dt_ss.NewRow();
                CreateDataRowFromTeamStats(splitTeamStats[ID]["vs " + MainWindow.DisplayNames[oppTeam]], ref dr,
                                           "vs " + MainWindow.DisplayNames[oppTeam]);
                dt_ss.Rows.Add(dr);
            }

            #endregion

            #region Monthly split stats

            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);

            foreach (var sspair in splitTeamStats[ID].Where(pair => pair.Key.StartsWith("M ")))
            {
                dr = dt_ss.NewRow();
                DateTime label = new DateTime(Convert.ToInt32(sspair.Key.Substring(2, 4)), Convert.ToInt32(sspair.Key.Substring(7, 2)), 1);
                CreateDataRowFromTeamStats(sspair.Value, ref dr, label.Year.ToString() + " " + String.Format("{0:MMMM}", label));
                dt_ss.Rows.Add(dr);
            }

            #endregion

            // DataTable is done, create DataView and load into DataGrid
            var dv_ss = new DataView(dt_ss) {AllowEdit = false, AllowNew = false};

            dgvSplit.DataContext = dv_ss;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbTeam control.
        /// Loads the information for the newly selected team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                dgvBoxScores.DataContext = null;
                dgvTeamStats.DataContext = null;
                dgvHTHStats.DataContext = null;
                dgvHTHBoxScores.DataContext = null;
                dgvSplit.DataContext = null;
                dgvPlayerStats.ItemsSource = null;
                dgvMetricStats.ItemsSource = null;
                dgvTeamRoster.ItemsSource = null;

                if (cmbTeam.SelectedIndex == -1)
                    return;
                if (cmbSeasonNum.SelectedIndex == -1)
                    return;
            }
            catch
            {
                return;
            }

            dt_bs_res = new DataTable();

            //DataRow dr;

            dt_bs.Clear();
            dt_ov.Clear();
            dt_hth.Clear();
            dt_ss.Clear();
            dt_yea.Clear();

            curTeam = GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString());

            UpdateOverviewAndBoxScores();

            UpdateSplitStats();

            TeamStats ts = tst[MainWindow.TeamOrder[curTeam]];
            Title = cmbTeam.SelectedItem + " Team Overview - " + (ts.getGames() + ts.getPlayoffGames()) + " games played";

            UpdateHeadToHead();

            UpdateYearlyStats();

            UpdatePlayerAndMetricStats();

            UpdateBest();
        }

        /// <summary>
        /// Finds the tam's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private string GetCurTeamFromDisplayName(string displayName)
        {
            return Misc.GetCurTeamFromDisplayName(tst, displayName);
        }

        /// <summary>
        /// Finds the team's displayName by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private string GetDisplayNameFromTeam(string name)
        {
            return Misc.GetDisplayNameFromTeam(tst, name);
        }

        /// <summary>
        /// Determines the team's best players and their most significant stats and updates the corresponding tab.
        /// </summary>
        private void UpdateBest()
        {
            txbPlayer1.Text = "";
            txbPlayer2.Text = "";
            txbPlayer3.Text = "";
            txbPlayer4.Text = "";
            txbPlayer5.Text = "";
            txbPlayer6.Text = "";

            PlayerStatsRow psr1;
            string text;
            try
            {
                List<PlayerStatsRow> templist = psrList.ToList();
                /*
                if (double.IsNaN(templist[0].PER))
                {
                    templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                }
                else
                {
                    templist.Sort((pmsr1, pmsr2) => pmsr1.PER.CompareTo(pmsr2.PER));
                }
                */
                templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                templist.Reverse();

                psr1 = templist[0];
                text = psr1.GetBestStats(5);
                txbPlayer1.Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")" +
                                  (psr1.isInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(5);
                txbPlayer2.Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + ")" +
                                  (psr2.isInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(5);
                txbPlayer3.Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + ")" +
                                  (psr3.isInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(5);
                txbPlayer4.Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + ")" +
                                  (psr4.isInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(5);
                txbPlayer5.Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + ")" +
                                  (psr5.isInjured ? " (Injured)" : "") + "\n\n" + text;

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(5);
                txbPlayer6.Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + ")" +
                                  (psr6.isInjured ? " (Injured)" : "") + "\n\n" + text;
            }
            catch (Exception)
            {
            }

            CalculateStarting5();
        }

        /// <summary>
        /// Determines the team's best starting five and their most significant stats.
        /// </summary>
        private void CalculateStarting5()
        {
            txbStartingPG.Text = "";
            txbStartingSG.Text = "";
            txbStartingSF.Text = "";
            txbStartingPF.Text = "";
            txbStartingC.Text = "";

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            List<PlayerStatsRow> PGList =
                psrList.Where(row => (row.Position1.ToString() == "PG" || row.Position2.ToString() == "PG") && row.isInjured == false)
                       .ToList();
            PGList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            PGList.Reverse();
            List<PlayerStatsRow> SGList =
                psrList.Where(row => (row.Position1.ToString() == "SG" || row.Position2.ToString() == "SG") && row.isInjured == false)
                       .ToList();
            SGList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            SGList.Reverse();
            List<PlayerStatsRow> SFList =
                psrList.Where(row => (row.Position1.ToString() == "SF" || row.Position2.ToString() == "SF") && row.isInjured == false)
                       .ToList();
            SFList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            SFList.Reverse();
            List<PlayerStatsRow> PFList =
                psrList.Where(row => (row.Position1.ToString() == "PF" || row.Position2.ToString() == "PF") && row.isInjured == false)
                       .ToList();
            PFList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            PFList.Reverse();
            List<PlayerStatsRow> CList =
                psrList.Where(row => (row.Position1.ToString() == "C" || row.Position2.ToString() == "C") && row.isInjured == false)
                       .ToList();
            CList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            CList.Reverse();
            var permutations = new List<StartingFivePermutation>();

            double max = Double.MinValue;
            for (int i1 = 0; i1 < PGList.Count; i1++)
                for (int i2 = 0; i2 < SGList.Count; i2++)
                    for (int i3 = 0; i3 < SFList.Count; i3++)
                        for (int i4 = 0; i4 < PFList.Count; i4++)
                            for (int i5 = 0; i5 < CList.Count; i5++)
                            {
                                double _sum = 0;
                                int _pInP = 0;
                                var perm = new List<int>();
                                perm.Add(PGList[i1].ID);
                                _sum += PGList[i1].GmSc;
                                if (PGList[i1].Position1.ToString() == "PG")
                                    _pInP++;
                                if (perm.Contains(SGList[i2].ID))
                                {
                                    continue;
                                }
                                perm.Add(SGList[i2].ID);
                                _sum += SGList[i2].GmSc;
                                if (SGList[i2].Position1.ToString() == "SG")
                                    _pInP++;
                                if (perm.Contains(SFList[i3].ID))
                                {
                                    continue;
                                }
                                perm.Add(SFList[i3].ID);
                                _sum += SFList[i3].GmSc;
                                if (SFList[i3].Position1.ToString() == "SF")
                                    _pInP++;
                                if (perm.Contains(PFList[i4].ID))
                                {
                                    continue;
                                }
                                perm.Add(PFList[i4].ID);
                                _sum += PFList[i4].GmSc;
                                if (PFList[i4].Position1.ToString() == "PF")
                                    _pInP++;
                                if (perm.Contains(CList[i5].ID))
                                {
                                    continue;
                                }
                                perm.Add(CList[i5].ID);
                                _sum += CList[i5].GmSc;
                                if (CList[i5].Position1.ToString() == "C")
                                    _pInP++;

                                if (_sum > max)
                                    max = _sum;

                                permutations.Add(new StartingFivePermutation {idList = perm, PlayersInPrimaryPosition = _pInP, Sum = _sum});
                            }

            try
            {
                StartingFivePermutation bestPerm =
                    permutations.Where(perm1 => perm1.Sum.Equals(max)).OrderByDescending(perm2 => perm2.PlayersInPrimaryPosition).First();
                bestPerm.idList.ForEach(i1 => tempList.Add(psrList.Single(row => row.ID == i1)));
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
        }

        /// <summary>
        /// Calculates the player and metric stats and updates the corresponding tabs.
        /// </summary>
        private void UpdatePlayerAndMetricStats()
        {
            psrList = new ObservableCollection<PlayerStatsRow>();
            pl_psrList = new ObservableCollection<PlayerStatsRow>();

            var players = pst.Where(pair => pair.Value.TeamF == curTeam && !pair.Value.isHidden);
            foreach (var pl in players)
            {
                psrList.Add(new PlayerStatsRow(pl.Value, false));
                pl_psrList.Add(new PlayerStatsRow(pl.Value, true));
            }

            psrList.Sort(delegate(PlayerStatsRow row1, PlayerStatsRow row2) { return String.Compare(row1.LastName, row2.LastName); });
            pl_psrList.Sort(delegate(PlayerStatsRow row1, PlayerStatsRow row2) { return String.Compare(row1.LastName, row2.LastName); });

            dgvPlayerStats.ItemsSource = psrList;
            dgvMetricStats.ItemsSource = psrList;
            dgvTeamRoster.ItemsSource = psrList;
            dgvTeamRoster.CanUserAddRows = false;
            dgvPlayerPlayoffStats.ItemsSource = pl_psrList;
            dgvPlayoffMetricStats.ItemsSource = pl_psrList;
        }

        /// <summary>
        /// Updates the head to head tab.
        /// </summary>
        private void UpdateHeadToHead()
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        /// <summary>
        /// Calculates the yearly stats and updates the yearly stats tab.
        /// </summary>
        private void UpdateYearlyStats()
        {
            dt_yea.Clear();

            string currentDB = MainWindow.currentDB;
            curSeason = MainWindow.curSeason;
            maxSeason = SQLiteIO.getMaxSeason(currentDB);

            TeamStats ts = tst[MainWindow.TeamOrder[curTeam]];
            TeamStats tsopp;
            var tsAllSeasons = new TeamStats("All Seasons");
            var tsAllPlayoffs = new TeamStats("All Playoffs");
            var tsAll = new TeamStats("All Games");
            tsAllSeasons.AddTeamStats(ts, Span.Season);
            tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
            tsAll.AddTeamStats(ts, Span.SeasonAndPlayoffs);

            DataRow drcur = dt_yea.NewRow();
            DataRow drcur_pl = dt_yea.NewRow();
            CreateDataRowFromTeamStats(ts, ref drcur, "Season " + curSeason.ToString());

            bool playedInPlayoffs = false;
            if (ts.pl_winloss[0] + ts.pl_winloss[1] > 0)
            {
                CreateDataRowFromTeamStats(ts, ref drcur_pl, "Playoffs " + curSeason.ToString(), true);
                playedInPlayoffs = true;
            }

            //
            string qr = string.Format(@"SELECT * FROM PastTeamStats WHERE TeamID = {0} ORDER BY ""SOrder""", ts.ID);
            DataTable dt = db.GetDataTable(qr);
            foreach (DataRow dr in dt.Rows)
            {
                DataRow dr4 = dt_yea.NewRow();
                ts = new TeamStats();
                if (Tools.getBoolean(dr, "isPlayoff"))
                {
                    SQLiteIO.GetTeamStatsFromDataRow(ref ts, dr, true);
                    CreateDataRowFromTeamStats(ts, ref dr4, "Playoffs " + Tools.getString(dr, "SeasonName"), true);
                    tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
                    tsAll.AddTeamStats(ts, Span.Playoffs);
                }
                else
                {
                    SQLiteIO.GetTeamStatsFromDataRow(ref ts, dr, false);
                    CreateDataRowFromTeamStats(ts, ref dr4, "Season " + Tools.getString(dr, "SeasonName"), false);
                    tsAllSeasons.AddTeamStats(ts, Span.Season);
                    tsAll.AddTeamStats(ts, Span.Season);
                }
                dt_yea.Rows.Add(dr4);
            }
            //

            for (int j = 1; j <= maxSeason; j++)
            {
                if (j != curSeason)
                {
                    SQLiteIO.GetTeamStatsFromDatabase(MainWindow.currentDB, curTeam, j, out ts, out tsopp);
                    DataRow dr3 = dt_yea.NewRow();
                    DataRow dr3_pl = dt_yea.NewRow();
                    CreateDataRowFromTeamStats(ts, ref dr3, "Season " + j.ToString());

                    dt_yea.Rows.Add(dr3);
                    if (ts.pl_winloss[0] + ts.pl_winloss[1] > 0)
                    {
                        CreateDataRowFromTeamStats(ts, ref dr3_pl, "Playoffs " + j.ToString(), true);
                        dt_yea.Rows.Add(dr3_pl);
                    }

                    tsAllSeasons.AddTeamStats(ts, Span.Season);
                    tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
                    tsAll.AddTeamStats(ts, Span.SeasonAndPlayoffs);
                }
                else
                {
                    dt_yea.Rows.Add(drcur);
                    if (playedInPlayoffs)
                        dt_yea.Rows.Add(drcur_pl);
                }
            }

            dt_yea.Rows.Add(dt_yea.NewRow());

            drcur = dt_yea.NewRow();
            CreateDataRowFromTeamStats(tsAllSeasons, ref drcur, "All Seasons");
            dt_yea.Rows.Add(drcur);
            drcur = dt_yea.NewRow();
            CreateDataRowFromTeamStats(tsAllPlayoffs, ref drcur, "All Playoffs");
            dt_yea.Rows.Add(drcur);

            dt_yea.Rows.Add(dt_yea.NewRow());

            drcur = dt_yea.NewRow();
            CreateDataRowFromTeamStats(tsAll, ref drcur, "All Games");
            dt_yea.Rows.Add(drcur);

            var dv_yea = new DataView(dt_yea) {AllowNew = false, AllowEdit = false};

            dgvYearly.DataContext = dv_yea;
        }

        /// <summary>
        /// Handles the Click event of the btnShowAvg control.
        /// Shows the old "Correct Team Stats" styled averages and rankings window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string msg = TeamStats.TeamAveragesAndRankings(GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()), tst,
                                                               MainWindow.TeamOrder);
                if (msg != "")
                {
                    var cw = new CopyableMessageWindow(msg, GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()), TextAlignment.Center);
                    cw.ShowDialog();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No team selected.");
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveCustomTeam control.
        /// Saves the team's stats into the database.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores" +
                    ") or edit the box-scores themselves.", "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int id;
            try
            {
                id = MainWindow.TeamOrder[GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString())];
            }
            catch (Exception)
            {
                return;
            }
            tst[id].winloss[0] = Convert.ToByte(myCell(0, 2));
            tst[id].winloss[1] = Convert.ToByte(myCell(0, 3));
            tst[id].stats[t.PF] = Convert.ToUInt16(myCell(0, 4));
            tst[id].stats[t.PA] = Convert.ToUInt16(myCell(0, 5));

            string[] parts = myCell(0, 7).Split('-');
            tst[id].stats[t.FGM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[t.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 9).Split('-');
            tst[id].stats[t.TPM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[t.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 11).Split('-');
            tst[id].stats[t.FTM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[t.FTA] = Convert.ToUInt16(parts[1]);

            tst[id].stats[t.OREB] = Convert.ToUInt16(myCell(0, 14));
            tst[id].stats[t.DREB] = Convert.ToUInt16(myCell(0, 15));

            tst[id].stats[t.AST] = Convert.ToUInt16(myCell(0, 16));
            tst[id].stats[t.TO] = Convert.ToUInt16(myCell(0, 17));
            tst[id].stats[t.STL] = Convert.ToUInt16(myCell(0, 18));
            tst[id].stats[t.BLK] = Convert.ToUInt16(myCell(0, 19));
            tst[id].stats[t.FOUL] = Convert.ToUInt16(myCell(0, 20));
            tst[id].stats[t.MINS] = Convert.ToUInt16(myCell(0, 21));

            tst[id].pl_winloss[0] = Convert.ToByte(myCell(6, 2));
            tst[id].pl_winloss[1] = Convert.ToByte(myCell(6, 3));
            tst[id].pl_stats[t.PF] = Convert.ToUInt16(myCell(6, 4));
            tst[id].pl_stats[t.PA] = Convert.ToUInt16(myCell(6, 5));

            parts = myCell(6, 7).Split('-');
            tst[id].pl_stats[t.FGM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[t.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(6, 9).Split('-');
            tst[id].pl_stats[t.TPM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[t.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(6, 11).Split('-');
            tst[id].pl_stats[t.FTM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[t.FTA] = Convert.ToUInt16(parts[1]);

            tst[id].pl_stats[t.OREB] = Convert.ToUInt16(myCell(6, 14));
            tst[id].pl_stats[t.DREB] = Convert.ToUInt16(myCell(6, 15));

            tst[id].pl_stats[t.AST] = Convert.ToUInt16(myCell(6, 16));
            tst[id].pl_stats[t.TO] = Convert.ToUInt16(myCell(6, 17));
            tst[id].pl_stats[t.STL] = Convert.ToUInt16(myCell(6, 18));
            tst[id].pl_stats[t.BLK] = Convert.ToUInt16(myCell(6, 19));
            tst[id].pl_stats[t.FOUL] = Convert.ToUInt16(myCell(6, 20));
            tst[id].pl_stats[t.MINS] = Convert.ToUInt16(myCell(6, 21));

            tst[id].CalcAvg();


            // Opponents
            tstopp[id].winloss[0] = Convert.ToByte(myCell(3, 2));
            tstopp[id].winloss[1] = Convert.ToByte(myCell(3, 3));
            tstopp[id].stats[t.PF] = Convert.ToUInt16(myCell(3, 4));
            tstopp[id].stats[t.PA] = Convert.ToUInt16(myCell(3, 5));

            parts = myCell(3, 7).Split('-');
            tstopp[id].stats[t.FGM] = Convert.ToUInt16(parts[0]);
            tstopp[id].stats[t.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 9).Split('-');
            tstopp[id].stats[t.TPM] = Convert.ToUInt16(parts[0]);
            tstopp[id].stats[t.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 11).Split('-');
            tstopp[id].stats[t.FTM] = Convert.ToUInt16(parts[0]);
            tstopp[id].stats[t.FTA] = Convert.ToUInt16(parts[1]);

            tstopp[id].stats[t.OREB] = Convert.ToUInt16(myCell(3, 14));
            tstopp[id].stats[t.DREB] = Convert.ToUInt16(myCell(3, 15));

            tstopp[id].stats[t.AST] = Convert.ToUInt16(myCell(3, 16));
            tstopp[id].stats[t.TO] = Convert.ToUInt16(myCell(3, 17));
            tstopp[id].stats[t.STL] = Convert.ToUInt16(myCell(3, 18));
            tstopp[id].stats[t.BLK] = Convert.ToUInt16(myCell(3, 19));
            tstopp[id].stats[t.FOUL] = Convert.ToUInt16(myCell(3, 20));
            tstopp[id].stats[t.MINS] = Convert.ToUInt16(myCell(3, 21));

            tstopp[id].pl_winloss[0] = Convert.ToByte(myCell(9, 2));
            tstopp[id].pl_winloss[1] = Convert.ToByte(myCell(9, 3));
            tstopp[id].pl_stats[t.PF] = Convert.ToUInt16(myCell(9, 4));
            tstopp[id].pl_stats[t.PA] = Convert.ToUInt16(myCell(9, 5));

            parts = myCell(9, 7).Split('-');
            tstopp[id].pl_stats[t.FGM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[t.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(9, 9).Split('-');
            tstopp[id].pl_stats[t.TPM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[t.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(9, 11).Split('-');
            tstopp[id].pl_stats[t.FTM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[t.FTA] = Convert.ToUInt16(parts[1]);

            tstopp[id].pl_stats[t.OREB] = Convert.ToUInt16(myCell(9, 14));
            tstopp[id].pl_stats[t.DREB] = Convert.ToUInt16(myCell(9, 15));

            tstopp[id].pl_stats[t.AST] = Convert.ToUInt16(myCell(9, 16));
            tstopp[id].pl_stats[t.TO] = Convert.ToUInt16(myCell(9, 17));
            tstopp[id].pl_stats[t.STL] = Convert.ToUInt16(myCell(9, 18));
            tstopp[id].pl_stats[t.BLK] = Convert.ToUInt16(myCell(9, 19));
            tstopp[id].pl_stats[t.FOUL] = Convert.ToUInt16(myCell(9, 20));
            tstopp[id].pl_stats[t.MINS] = Convert.ToUInt16(myCell(9, 21));

            tstopp[id].CalcAvg();

            Dictionary<int, PlayerStats> playersToUpdate = psrList.Select(cur => new PlayerStats(cur)).ToDictionary(ps => ps.ID);
            List<int> playerIDs = playersToUpdate.Keys.ToList();
            foreach (var playerID in playerIDs)
            {
                playersToUpdate[playerID].UpdatePlayoffStats(pl_psrList.Single(pl_psr => pl_psr.ID == playerID));
            }

            SQLiteIO.saveSeasonToDatabase(MainWindow.currentDB, tst, tstopp, playersToUpdate, curSeason, maxSeason, partialUpdate: true);
            SQLiteIO.LoadSeason(MainWindow.currentDB, curSeason, doNotLoadBoxScores: true);

            int temp = cmbTeam.SelectedIndex;
            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = temp;
        }

        /// <summary>
        /// Gets the value of the specified cell from the dgvTeamStats DataGrid.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private string myCell(int row, int col)
        {
            return GetCellValue(dgvTeamStats, row, col);
        }

        /// <summary>
        /// Gets the value of the specified cell from the specified DataGrid.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private string GetCellValue(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
                return dataRowView.Row.ItemArray[col].ToString();

            return null;
        }

        /// <summary>
        /// Handles the Click event of the btnScoutingReport control.
        /// Displays a well-formatted scouting report in natural language containing comments on the team's performance, strong and weak points.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnScoutingReport_Click(object sender, RoutedEventArgs e)
        {
            int id;
            try
            {
                id = MainWindow.TeamOrder[GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString())];
            }
            catch
            {
                return;
            }

            var temptst = new Dictionary<int, TeamStats>();
            foreach (var kvp in tst)
            {
                int i = kvp.Key;
                temptst.Add(i, tst[i].DeepClone());
                temptst[i].ResetStats(Span.SeasonAndPlayoffs);
                temptst[i].AddTeamStats(tst[i], Span.SeasonAndPlayoffs);
            }

            if (temptst.Count > 1)
            {
                string msg = temptst[id].ScoutingReport(temptst, psrList);
                var cw = new CopyableMessageWindow(msg, "Scouting Report", TextAlignment.Left);
                cw.ShowDialog();
            }
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the dtpEnd control.
        /// Makes sure the starting date isn't after the ending date, and updates the team's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
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
            LinkInternalsToMainWindow();
            cmbTeam_SelectionChanged(sender, null);
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the dtpStart control.
        /// Makes sure the starting date isn't after the ending date, and updates the team's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
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
            LinkInternalsToMainWindow();
            cmbTeam_SelectionChanged(sender, null);
        }

        /// <summary>
        /// Handles the Checked event of the rbStatsAllTime control.
        /// Allows the user to display stats from the whole season.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.tf = new Timeframe(curSeason);
            if (!changingTimeframe)
            {
                MainWindow.UpdateAllData();
                LinkInternalsToMainWindow();
                cmbTeam_SelectionChanged(sender, null);
            }
        }

        /// <summary>
        /// Handles the Checked event of the rbStatsBetween control.
        /// Allows the user to display stats between the specified timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            if (!changingTimeframe)
            {
                MainWindow.UpdateAllData();
                LinkInternalsToMainWindow();
                cmbTeam_SelectionChanged(sender, null);
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the dgvBoxScores control.
        /// Allows the user to view a specific box score in the Box Score window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (TeamBoxScore) dgvBoxScores.SelectedItems[0];

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, row.id);
                try
                {
                    bsw.ShowDialog();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPrevOpp control.
        /// Switches to the previous opposing team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnPrevOpp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == 0)
                cmbOppTeam.SelectedIndex = cmbOppTeam.Items.Count - 1;
            else
                cmbOppTeam.SelectedIndex--;
        }

        /// <summary>
        /// Handles the Click event of the btnNextOpp control.
        /// Switches to the next opposing team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnNextOpp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == cmbOppTeam.Items.Count - 1)
                cmbOppTeam.SelectedIndex = 0;
            else
                cmbOppTeam.SelectedIndex++;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbOppTeam control.
        /// Synchronizes the two opposing team combos, loads the stats of the selected opposing team, and updates the appropriate tabs.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbOppTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingOppTeam)
                return;

            if (sender == cmbOppTeam)
            {
                try
                {
                    changingOppTeam = true;
                    cmbOppTeamBest.SelectedIndex = cmbOppTeam.SelectedIndex;
                }
                catch
                {
                    changingOppTeam = false;
                    return;
                }
            }
            else if (sender == cmbOppTeamBest)
            {
                try
                {
                    changingOppTeam = true;
                    cmbOppTeam.SelectedIndex = cmbOppTeamBest.SelectedIndex;
                }
                catch
                {
                    changingOppTeam = false;
                    return;
                }
            }
            else
            {
                try
                {
                    changingOppTeam = true;
                    cmbOppTeamBest.SelectedIndex = cmbOppTeam.SelectedIndex;
                }
                catch (Exception)
                {
                    changingOppTeam = false;
                    return;
                }
            }

            if (cmbOppTeam.SelectedIndex == -1)
            {
                changingOppTeam = false;
                return;
            }

            dgvHTHBoxScores.DataContext = null;
            dgvHTHStats.DataContext = null;

            try
            {
                txbTeam1.Text = ""; //Fires exception on InitializeComponent()
            }
            catch (Exception)
            {
                changingOppTeam = false;
                return;
            }
            txbTeam2.Text = "";
            txbTeam3.Text = "";
            txbOpp1.Text = "";
            txbOpp2.Text = "";
            txbOpp3.Text = "";

            if (cmbOppTeam.SelectedIndex == cmbTeam.SelectedIndex)
            {
                changingOppTeam = false;
                return;
            }

            curTeam = GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString());
            string curOpp = GetCurTeamFromDisplayName(cmbOppTeam.SelectedItem.ToString());

            int iown = MainWindow.TeamOrder[curTeam];
            int iopp = MainWindow.TeamOrder[curOpp];

            var dt_hth_bs = new DataTable();
            dt_hth_bs.Columns.Add("Date");
            dt_hth_bs.Columns.Add("Home-Away");
            dt_hth_bs.Columns.Add("Result");
            dt_hth_bs.Columns.Add("Score");
            dt_hth_bs.Columns.Add("GameID");

            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats(curOpp);

            db = new SQLiteDatabase(MainWindow.currentDB);

            if (dt_hth.Rows.Count > 1)
                dt_hth.Rows.RemoveAt(dt_hth.Rows.Count - 1);

            var bshist = MainWindow.bshist;

            var BSEs =
                bshist.Where(
                    bse => (bse.bs.Team1 == curTeam && bse.bs.Team2 == curOpp) || (bse.bs.Team1 == curOpp && bse.bs.Team2 == curTeam));

            if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
            {
                ts = tst[iown];
                ts.CalcAvg();

                tsopp = tst[iopp];
                tsopp.CalcAvg();
            }
            else
            {
                foreach (var bse in BSEs)
                {
                    TeamStats.AddTeamStatsFromBoxScore(bse.bs, ref ts, ref tsopp, true);
                }
            }

            //ts.CalcMetrics(tsopp);
            //tsopp.CalcMetrics(ts);
            var ls = new TeamStats();
            ls.AddTeamStats(ts, Span.SeasonAndPlayoffs);
            ls.AddTeamStats(tsopp, Span.SeasonAndPlayoffs);
            List<int> keys = pst.Keys.ToList();
            List<PlayerStatsRow> teamPMSRList = new List<PlayerStatsRow>(), oppPMSRList = new List<PlayerStatsRow>();
            foreach (int key in keys)
            {
                if (pst[key].TeamF == ts.name)
                {
                    teamPMSRList.Add(new PlayerStatsRow(pst[key]));
                }
                else if (pst[key].TeamF == tsopp.name)
                {
                    oppPMSRList.Add(new PlayerStatsRow(pst[key]));
                }
            }

            foreach (var bse in BSEs)
            {
                int t1pts = bse.bs.PTS1;
                int t2pts = bse.bs.PTS2;
                DataRow bsr = dt_hth_bs.NewRow();
                bsr["Date"] = bse.bs.gamedate.ToString().Split(' ')[0];
                if (bse.bs.Team1.Equals(curTeam))
                {
                    bsr["Home-Away"] = "Away";

                    if (t1pts > t2pts)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }
                }
                else
                {
                    bsr["Home-Away"] = "Home";

                    if (t2pts > t1pts)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }
                }

                bsr["Score"] = bse.bs.PTS1 + "-" + bse.bs.PTS2;
                bsr["GameID"] = bse.bs.id.ToString();

                dt_hth_bs.Rows.Add(bsr);
            }

            dt_hth.Clear();

            DataRow dr = dt_hth.NewRow();

            CreateDataRowFromTeamStats(ts, ref dr, "Averages");

            dt_hth.Rows.Add(dr);

            dr = dt_hth.NewRow();

            CreateDataRowFromTeamStats(tsopp, ref dr, "Opp Avg");

            dt_hth.Rows.Add(dr);

            dr = dt_hth.NewRow();

            CreateDataRowFromTeamStats(ts, ref dr, "Playoffs", true);

            dt_hth.Rows.Add(dr);

            dr = dt_hth.NewRow();

            CreateDataRowFromTeamStats(tsopp, ref dr, "Opp Pl Avg", true);

            dt_hth.Rows.Add(dr);

            dv_hth = new DataView(dt_hth) {AllowNew = false, AllowEdit = false};

            dgvHTHStats.DataContext = dv_hth;

            var dv_hth_bs = new DataView(dt_hth_bs) {AllowNew = false, AllowEdit = false};

            dgvHTHBoxScores.DataContext = dv_hth_bs;

            List<PlayerStatsRow> guards = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                             {
                                                                 if (psr.Position1.ToString().EndsWith("G"))
                                                                 {
                                                                     if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                                         return true;

                                                                     return (!psr.isInjured);
                                                                 }
                                                                 return false;
                                                             }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            List<PlayerStatsRow> fors = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                           {
                                                               if (psr.Position1.ToString().EndsWith("F"))
                                                               {
                                                                   if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                                       return true;

                                                                   return (!psr.isInjured);
                                                               }
                                                               return false;
                                                           }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            List<PlayerStatsRow> centers = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                              {
                                                                  if (psr.Position1.ToString().EndsWith("C"))
                                                                  {
                                                                      if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                                          return true;

                                                                      return (!psr.isInjured);
                                                                  }
                                                                  return false;
                                                              }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                string text = guards[0].GetBestStats(5);
                txbTeam1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + (guards[0].isInjured ? " (Injured)" : "") + "\n\n" +
                                text;

                text = fors[0].GetBestStats(5);
                txbTeam2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + (fors[0].isInjured ? " (Injured)" : "") + "\n\n" + text;

                text = centers[0].GetBestStats(5);
                txbTeam3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + (centers[0].isInjured ? " (Injured)" : "") +
                                "\n\n" + text;
            }
            catch
            {
            }

            guards = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                       {
                                           if (psr.Position1.ToString().EndsWith("G"))
                                           {
                                               if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                   return true;

                                               return (!psr.isInjured);
                                           }
                                           return false;
                                       }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            fors = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                     {
                                         if (psr.Position1.ToString().EndsWith("F"))
                                         {
                                             if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                 return true;

                                             return (!psr.isInjured);
                                         }
                                         return false;
                                     }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            centers = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                        {
                                            if (psr.Position1.ToString().EndsWith("C"))
                                            {
                                                if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                                                    return true;

                                                return (!psr.isInjured);
                                            }
                                            return false;
                                        }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                string text = guards[0].GetBestStats(5);
                txbOpp1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + (guards[0].isInjured ? " (Injured)" : "") + "\n\n" +
                               text;

                text = fors[0].GetBestStats(5);
                txbOpp2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + (fors[0].isInjured ? " (Injured)" : "") + "\n\n" + text;

                text = centers[0].GetBestStats(5);
                txbOpp3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + (centers[0].isInjured ? " (Injured)" : "") +
                               "\n\n" + text;
            }
            catch
            {
            }

            grpHTHBestOpp.Header = cmbOppTeamBest.SelectedItem;
            grpHTHBestTeam.Header = cmbTeam.SelectedItem;

            changingOppTeam = false;
        }

        /// <summary>
        /// Creates a data row from a TeamStats instance.
        /// </summary>
        /// <param name="ts">The TeamStats instance.</param>
        /// <param name="dr">The data row to be edited.</param>
        /// <param name="title">The title for the row's Type or Name column.</param>
        /// <param name="playoffs">if set to <c>true</c>, the row will present the team's playoff stats; otherwise, the regular season's.</param>
        public static void CreateDataRowFromTeamStats(TeamStats ts, ref DataRow dr, string title, bool playoffs = false)
        {
            try
            {
                dr["Type"] = title;
            }
            catch
            {
                dr["Name"] = title;
            }
            if (!playoffs)
            {
                dr["Games"] = ts.getGames();
                dr["Wins"] = ts.winloss[0].ToString();
                dr["Losses"] = ts.winloss[1].ToString();
                dr["W%"] = String.Format("{0:F3}", ts.averages[t.Wp]);
                dr["Weff"] = String.Format("{0:F2}", ts.averages[t.Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.averages[t.PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.averages[t.PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.averages[t.PD]);
                dr["FG"] = String.Format("{0:F3}", ts.averages[t.FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.averages[t.FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.averages[t.TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.averages[t.TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.averages[t.FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.averages[t.FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.averages[t.RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.averages[t.ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.averages[t.DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.averages[t.APG]);
                dr["TO"] = String.Format("{0:F1}", ts.averages[t.TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.averages[t.SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.averages[t.BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.averages[t.FPG]);
                dr["MINS"] = String.Format("{0:F1}", ts.averages[t.MINS]);
            }
            else
            {
                dr["Games"] = ts.getPlayoffGames();
                dr["Wins"] = ts.pl_winloss[0].ToString();
                dr["Losses"] = ts.pl_winloss[1].ToString();
                dr["W%"] = String.Format("{0:F3}", ts.pl_averages[t.Wp]);
                dr["Weff"] = String.Format("{0:F2}", ts.pl_averages[t.Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.pl_averages[t.PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.pl_averages[t.PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.pl_averages[t.PD]);
                dr["FG"] = String.Format("{0:F3}", ts.pl_averages[t.FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.pl_averages[t.FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.pl_averages[t.TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.pl_averages[t.TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.pl_averages[t.FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.pl_averages[t.FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.pl_averages[t.RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.pl_averages[t.ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.pl_averages[t.DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.pl_averages[t.APG]);
                dr["TO"] = String.Format("{0:F1}", ts.pl_averages[t.TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.pl_averages[t.SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.pl_averages[t.BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.pl_averages[t.FPG]);
                dr["MINS"] = String.Format("{0:F1}", ts.pl_averages[t.MINS]);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Window control.
        /// Connects the team and player stats dictionaries to the Main window's, calculates team rankings, 
        /// prepares the data tables and sets DataGrid parameters.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Prepare Data Tables

            dt_ov = new DataTable();

            dt_ov.Columns.Add("Type");
            dt_ov.Columns.Add("Games");
            dt_ov.Columns.Add("Wins (W%)");
            dt_ov.Columns.Add("Losses (Weff)");
            dt_ov.Columns.Add("PF");
            dt_ov.Columns.Add("PA");
            dt_ov.Columns.Add("PD");
            dt_ov.Columns.Add("FG");
            dt_ov.Columns.Add("FGeff");
            dt_ov.Columns.Add("3PT");
            dt_ov.Columns.Add("3Peff");
            dt_ov.Columns.Add("FT");
            dt_ov.Columns.Add("FTeff");
            dt_ov.Columns.Add("REB");
            dt_ov.Columns.Add("OREB");
            dt_ov.Columns.Add("DREB");
            dt_ov.Columns.Add("AST");
            dt_ov.Columns.Add("TO");
            dt_ov.Columns.Add("STL");
            dt_ov.Columns.Add("BLK");
            dt_ov.Columns.Add("FOUL");
            dt_ov.Columns.Add("MINS");

            dt_hth = new DataTable();

            dt_hth.Columns.Add("Type");
            dt_hth.Columns.Add("Games");
            dt_hth.Columns.Add("Wins");
            dt_hth.Columns.Add("Losses");
            dt_hth.Columns.Add("W%");
            dt_hth.Columns.Add("Weff");
            dt_hth.Columns.Add("PF");
            dt_hth.Columns.Add("PA");
            dt_hth.Columns.Add("PD");
            dt_hth.Columns.Add("FG");
            dt_hth.Columns.Add("FGeff");
            dt_hth.Columns.Add("3PT");
            dt_hth.Columns.Add("3Peff");
            dt_hth.Columns.Add("FT");
            dt_hth.Columns.Add("FTeff");
            dt_hth.Columns.Add("REB");
            dt_hth.Columns.Add("OREB");
            dt_hth.Columns.Add("DREB");
            dt_hth.Columns.Add("AST");
            dt_hth.Columns.Add("TO");
            dt_hth.Columns.Add("STL");
            dt_hth.Columns.Add("BLK");
            dt_hth.Columns.Add("FOUL");

            dt_ss = new DataTable();

            dt_ss.Columns.Add("Type");
            dt_ss.Columns.Add("Games");
            dt_ss.Columns.Add("Wins");
            dt_ss.Columns.Add("Losses");
            dt_ss.Columns.Add("W%");
            dt_ss.Columns.Add("Weff");
            dt_ss.Columns.Add("PF");
            dt_ss.Columns.Add("PA");
            dt_ss.Columns.Add("PD");
            dt_ss.Columns.Add("FG");
            dt_ss.Columns.Add("FGeff");
            dt_ss.Columns.Add("3PT");
            dt_ss.Columns.Add("3Peff");
            dt_ss.Columns.Add("FT");
            dt_ss.Columns.Add("FTeff");
            dt_ss.Columns.Add("REB");
            dt_ss.Columns.Add("OREB");
            dt_ss.Columns.Add("DREB");
            dt_ss.Columns.Add("AST");
            dt_ss.Columns.Add("TO");
            dt_ss.Columns.Add("STL");
            dt_ss.Columns.Add("BLK");
            dt_ss.Columns.Add("FOUL");

            dt_yea = new DataTable();

            dt_yea.Columns.Add("Type");
            dt_yea.Columns.Add("Games");
            dt_yea.Columns.Add("Wins");
            dt_yea.Columns.Add("Losses");
            dt_yea.Columns.Add("W%");
            dt_yea.Columns.Add("Weff");
            dt_yea.Columns.Add("PF");
            dt_yea.Columns.Add("PA");
            dt_yea.Columns.Add("PD");
            dt_yea.Columns.Add("FG");
            dt_yea.Columns.Add("FGeff");
            dt_yea.Columns.Add("3PT");
            dt_yea.Columns.Add("3Peff");
            dt_yea.Columns.Add("FT");
            dt_yea.Columns.Add("FTeff");
            dt_yea.Columns.Add("REB");
            dt_yea.Columns.Add("OREB");
            dt_yea.Columns.Add("DREB");
            dt_yea.Columns.Add("AST");
            dt_yea.Columns.Add("TO");
            dt_yea.Columns.Add("STL");
            dt_yea.Columns.Add("BLK");
            dt_yea.Columns.Add("FOUL");

            dt_bs = new DataTable();
            dt_bs.Columns.Add("Date", typeof (DateTime));
            dt_bs.Columns.Add("Opponent");
            dt_bs.Columns.Add("Home-Away");
            dt_bs.Columns.Add("Result");
            dt_bs.Columns.Add("Score");
            dt_bs.Columns.Add("GameID");

            #endregion

            LinkInternalsToMainWindow();

            PopulateTeamsCombo();

            changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.tf.EndDate;
            dtpStart.SelectedDate = MainWindow.tf.StartDate;
            PopulateSeasonCombo();
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.tf.SeasonNum);
            if (MainWindow.tf.isBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            changingTimeframe = false;

            //cmbSeasonNum.SelectedIndex = MainWindow.mwInstance.cmbSeasonNum.SelectedIndex;

            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamRoster.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvSplit.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvYearly.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            cmbTeam.SelectedIndex = -1;
            if (!String.IsNullOrWhiteSpace(teamToLoad))
                cmbTeam.SelectedItem = teamToLoad;

            cmbOppTeam.SelectedIndex = -1;
            //Following line commented out to allow for faster loading of Team Overview
            //cmbOppTeam.SelectedIndex = 1;

            /*
            try
            {
                imgLogo.Source = Misc.LoadImage(MainWindow.imageDict[GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString())]);
            }
            catch (Exception)
            { }
            */
        }

        private void LinkInternalsToMainWindow()
        {
            tst = MainWindow.tst;
            tstopp = MainWindow.tstopp;
            pst = MainWindow.pst;

            rankings = MainWindow.TeamRankings;
            pl_rankings = MainWindow.PlayoffTeamRankings;
        }

        /// <summary>
        /// Handles the Checked event of the rbHTHStatsAnyone control.
        /// Used to include all the teams' games in the stat calculations, no matter the opponent.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            if (changingOppRange)
                return;

            if (sender == rbHTHStatsAnyone)
            {
                try
                {
                    changingOppRange = true;
                    rbHTHStatsAnyoneBest.IsChecked = rbHTHStatsAnyone.IsChecked;
                }
                catch
                {
                    changingOppRange = false;
                    return;
                }
            }
            else if (sender == rbHTHStatsAnyoneBest)
            {
                try
                {
                    changingOppRange = true;
                    rbHTHStatsAnyone.IsChecked = rbHTHStatsAnyoneBest.IsChecked;
                }
                catch
                {
                    changingOppRange = false;
                    return;
                }
            }
            else
            {
                try
                {
                    changingOppRange = true;
                    rbHTHStatsAnyoneBest.IsChecked = rbHTHStatsAnyone.IsChecked;
                }
                catch (Exception)
                {
                    changingOppRange = false;
                    return;
                }
            }
            cmbOppTeam_SelectionChanged(sender, null);
            changingOppRange = false;
        }

        /// <summary>
        /// Handles the Checked event of the rbHTHStatsEachOther control.
        /// Used to include only stats from the games these two teams have played against each other.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            if (changingOppRange)
                return;

            if (sender == rbHTHStatsEachOther)
            {
                try
                {
                    changingOppRange = true;
                    rbHTHStatsEachOtherBest.IsChecked = rbHTHStatsEachOther.IsChecked;
                }
                catch
                {
                    changingOppRange = false;
                    return;
                }
            }
            else if (sender == rbHTHStatsEachOtherBest)
            {
                try
                {
                    changingOppRange = true;
                    rbHTHStatsEachOther.IsChecked = rbHTHStatsEachOtherBest.IsChecked;
                }
                catch
                {
                    changingOppRange = false;
                    return;
                }
            }
            else
            {
                try
                {
                    changingOppRange = true;
                    rbHTHStatsEachOtherBest.IsChecked = rbHTHStatsEachOther.IsChecked;
                }
                catch (Exception)
                {
                    changingOppRange = false;
                    return;
                }
            }
            cmbOppTeam_SelectionChanged(sender, null);
            changingOppRange = false;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cmbSeasonNum control.
        /// Loads the team and player stats and information for the new season, repopulates the teams combo and tries to switch to the same team again.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs" /> instance containing the event data.</param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                rbStatsAllTime.IsChecked = true;
                if (cmbSeasonNum.SelectedIndex == -1)
                {
                    changingTimeframe = false;
                    return;
                }

                curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;
                if (curSeason == MainWindow.curSeason && !MainWindow.tf.isBetween)
                {
                    changingTimeframe = false;
                    return;
                }

                MainWindow.tf = new Timeframe(curSeason);
                MainWindow.ChangeSeason(curSeason);
                SQLiteIO.LoadSeason(curSeason);
                PopulateTeamsCombo();

                LinkInternalsToMainWindow();

                try
                {
                    cmbTeam.SelectedIndex = -1;
                    cmbTeam.SelectedItem = GetDisplayNameFromTeam(curTeam);
                }
                catch
                {
                    cmbTeam.SelectedIndex = -1;
                }
                changingTimeframe = false;
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the AnyPlayerDataGrid control.
        /// Views the selected player in the Player Overview window, and reloads their team's stats aftewrards.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void AnyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EventHandlers.AnyPlayerDataGrid_MouseDoubleClick(sender, e))
            {
                int curIndex = cmbTeam.SelectedIndex;
                cmbTeam.SelectedIndex = -1;
                cmbTeam.SelectedIndex = curIndex;
            }
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the dgvHTHBoxScores control.
        /// Views the selected box score in the Box Score window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void dgvHTHBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvHTHBoxScores.SelectedItems[0];
                int gameid = Convert.ToInt32(row["GameID"].ToString());

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameid);
                try
                {
                    bsw.ShowDialog();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// Updates the Main window's team & player stats dictionaries to match the ones in the Team Overview window before closing.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs" /> instance containing the event data.</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MainWindow.tst = tst;
            MainWindow.tstopp = tstopp;
            MainWindow.pst = pst;

            Misc.SetRegistrySetting("TeamOvHeight", Height);
            Misc.SetRegistrySetting("TeamOvWidth", Width);
            Misc.SetRegistrySetting("TeamOvX", Left);
            Misc.SetRegistrySetting("TeamOvY", Top);
        }

        /// <summary>
        /// Handles the Click event of the btnChangeName control.
        /// Allows the user to update the team's displayName for the current season.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnChangeName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ibw = new InputBoxWindow("Please enter the new name for the team", tst[MainWindow.TeamOrder[curTeam]].displayName);
                ibw.ShowDialog();
            }
            catch
            {
                return;
            }

            string newname = MainWindow.input;
            var dict = new Dictionary<string, string> {{"DisplayName", newname}};
            db.Update(MainWindow.teamsT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(MainWindow.pl_teamsT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(MainWindow.oppT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(MainWindow.pl_oppT, dict, "Name LIKE \"" + curTeam + "\"");

            int teamid = MainWindow.TeamOrder[curTeam];
            tst[teamid].displayName = newname;
            tstopp[teamid].displayName = newname;

            MainWindow.tst = tst;
            MainWindow.tstopp = tstopp;

            PopulateTeamsCombo();

            cmbTeam.SelectedItem = newname;
        }

        /// <summary>
        /// Handles the Sorting event of the StatColumn control.
        /// Uses a custom Sorting event handler that sorts a stat in descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridSortingEventArgs" /> instance containing the event data.</param>
        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the dgvTeamStats control.
        /// Allows the user to paste and import tab-separated values formatted team stats into the current team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void dgvTeamStats_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                string[] lines = Tools.SplitLinesToArray(Clipboard.GetText());
                List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSV(lines);

                foreach (var dict in dictList)
                {
                    string type = "Stats";
                    try
                    {
                        type = dict["Type"];
                    }
                    catch (Exception)
                    {
                    }
                    switch (type)
                    {
                        case "Stats":
                            TryChangeRow(0, dict);
                            break;

                        case "Playoffs":
                            TryChangeRow(6, dict);
                            break;

                        case "Opp Stats":
                            TryChangeRow(3, dict);
                            break;

                        case "Opp Pl Stats":
                            TryChangeRow(9, dict);
                            break;
                    }
                }

                CreateViewAndUpdateOverview();

                //btnSaveCustomTeam_Click(null, null);
            }
        }

        /// <summary>
        /// Tries to parse the data in the dictionary and change the values of the specified Overview row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="dict">The dict.</param>
        private void TryChangeRow(int row, Dictionary<string, string> dict)
        {
            dt_ov.Rows[row].TryChangeValue(dict, "Games", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "Wins (W%)", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "Losses (Weff)", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "PF", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "PA", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "FG", typeof (UInt16), "-");
            dt_ov.Rows[row].TryChangeValue(dict, "3PT", typeof (UInt16), "-");
            dt_ov.Rows[row].TryChangeValue(dict, "FT", typeof (UInt16), "-");
            dt_ov.Rows[row].TryChangeValue(dict, "REB", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "OREB", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "DREB", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "AST", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "TO", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "STL", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "BLK", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "FOUL", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "MINS", typeof (UInt16));
        }

        /// <summary>
        /// Allows the user to paste and import multiple player stats into the team's players.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        private void AnyPlayerDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                string[] lines = Tools.SplitLinesToArray(Clipboard.GetText());
                List<Dictionary<string, string>> dictList = CSV.DictionaryListFromTSV(lines);

                var list = sender == dgvPlayerStats ? psrList : pl_psrList;
                for (int j = 0; j < dictList.Count; j++)
                {
                    var dict = dictList[j];
                    int ID;
                    try
                    {
                        ID = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ID =
                                pst.Values.Single(
                                    ps => ps.TeamF == curTeam && ps.LastName == dict["Last Name"] && ps.FirstName == dict["First Name"]).ID;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Player in row " + (j+1) +
                                            " couldn't be determined either by ID or Full Name. Make sure the pasted data has the proper headers. " +
                                            "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        var psr = list.Single(ps => ps.ID == ID);
                        PlayerStatsRow.TryChangePSR(ref psr, dict);
                        PlayerStatsRow.Refresh(ref psr);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                ((DataGrid) sender).ItemsSource = null;
                ((DataGrid) sender).ItemsSource = list;

                MessageBox.Show(
                    "Data pasted successfully! Remember to save!\n\nNote that metric and other stats may appear incorrect until you save.",
                    "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handles the Click event of the chkHTHHideInjured control.
        /// Used to ignore injured players while doing Head-To-Head Best Performers analysis.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void chkHTHHideInjured_Click(object sender, RoutedEventArgs e)
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        /// <summary>
        /// Handles the Click event of the btnChangeDivision control.
        /// Allows the user to change the team's division.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void btnChangeDivision_Click(object sender, RoutedEventArgs e)
        {
            int teamid = MainWindow.TeamOrder[curTeam];
            int i = 0;
            foreach (Division div in MainWindow.Divisions)
            {
                if (tst[teamid].division == div.ID)
                {
                    break;
                }
                i++;
            }
            var ccw = new ComboChoiceWindow(ComboChoiceWindow.Mode.Division, i);
            ccw.ShowDialog();

            string[] parts = MainWindow.input.Split(new[] {": "}, 2, StringSplitOptions.None);
            Division myDiv = MainWindow.Divisions.Find(division => division.Name == parts[1]);

            tst[teamid].division = myDiv.ID;
            tstopp[teamid].division = myDiv.ID;

            var dict = new Dictionary<string, string>
                       {
                           {"Division", tst[teamid].division.ToString()},
                           {"Conference", tst[teamid].conference.ToString()}
                       };
            db.Update(MainWindow.teamsT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(MainWindow.pl_teamsT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(MainWindow.oppT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(MainWindow.pl_oppT, dict, "Name LIKE \"" + curTeam + "\"");

            MainWindow.tst = tst;
            MainWindow.tstopp = tstopp;
        }

        /// <summary>
        /// Handles the Sorting event of the dgvBoxScores control.
        /// Uses a custom Sorting event handler that sorts dates or a stat in descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridSortingEventArgs" /> instance containing the event data.</param>
        private void dgvBoxScores_Sorting(object sender, DataGridSortingEventArgs e)
        {
            StatColumn_Sorting(sender, e);
        }

        private void btnAddPastStats_Click(object sender, RoutedEventArgs e)
        {
            AddStatsWindow adw = new AddStatsWindow(true, MainWindow.TeamOrder[curTeam]);
            if (adw.ShowDialog() == true)
            {
                UpdateYearlyStats();
            }
        }
    }
}