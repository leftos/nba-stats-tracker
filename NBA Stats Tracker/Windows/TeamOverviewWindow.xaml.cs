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
using NBA_Stats_Tracker.Helper;
using SQLite_Database;
using EventHandlers = NBA_Stats_Tracker.Helper.EventHandlers;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for TeamOverviewWindow.xaml
    /// </summary>
    public partial class TeamOverviewWindow
    {
        private static string teamsT, pl_teamsT, oppT, pl_oppT;
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
        private int[][] pl_rankings;
        private ObservableCollection<PlayerStatsRow> psrList;
        private Dictionary<int, PlayerStats> pst;
        private int[][] rankings;
        private Dictionary<int, TeamStats> tst;
        private Dictionary<int, TeamStats> tstopp;

        public TeamOverviewWindow()
        {
            InitializeComponent();
        }

        public TeamOverviewWindow(string team)
            : this()
        {
            teamToLoad = team;
        }

        private void PopulateTeamsCombo()
        {
            List<string> teams =
                (from kvp in MainWindow.TeamOrder where !tst[kvp.Value].isHidden select tst[kvp.Value].displayName).
                    ToList();

            teams.Sort();

            cmbTeam.ItemsSource = teams;
            cmbOppTeam.ItemsSource = teams;
            cmbOppTeamBest.ItemsSource = teams;
        }

        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == 0) cmbTeam.SelectedIndex = cmbTeam.Items.Count - 1;
            else cmbTeam.SelectedIndex--;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count - 1) cmbTeam.SelectedIndex = 0;
            else cmbTeam.SelectedIndex++;
        }

        private void UpdateOverviewAndBoxScores()
        {
            curts = new TeamStats(curTeam);
            curtsopp = new TeamStats("Opponents");

            bsrList = new List<TeamBoxScore>();

            int i = MainWindow.TeamOrder[curTeam];

            #region Prepare Team Overview & Box Scores

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                curts = tst[i];
                curtsopp = tstopp[i];

                String q = "select * from GameResults where ((T1Name LIKE \"" + curTeam + "\") OR (T2Name LIKE \""
                           + curTeam + "\")) AND SeasonNum = " + curSeason + " ORDER BY Date DESC";
                DataTable res = db.GetDataTable(q);
                dt_bs_res = res;

                foreach (DataRow r in res.Rows)
                {
                    var bsr = new TeamBoxScore(r);
                    bsr.Prepare(curTeam);
                    bsrList.Add(bsr);
                }
            }
            else
            {
                if ((dtpStart.SelectedDate.HasValue) && (dtpEnd.SelectedDate.HasValue))
                {
                    String q = "select * from GameResults where ((T1Name LIKE \"" + curTeam + "\") OR (T2Name LIKE \""
                               + curTeam + "\")) AND ((Date >= \"" +
                               SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault())
                               + "\") AND (Date <= \"" +
                               SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()) + "\"))" +
                               " ORDER BY Date DESC";
                    DataTable res = db.GetDataTable(q);
                    dt_bs_res = res;

                    foreach (DataRow r in res.Rows)
                    {
                        var bsr = new TeamBoxScore(r);
                        bsr.Prepare(curTeam);
                        bsrList.Add(bsr);
                    }
                    AddToTeamStatsFromSQLBoxScores(res, ref curts, ref curtsopp);
                    curts.CalcMetrics(curtsopp);
                }
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

            curts.calcAvg(); // Just to be sure...

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
            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
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
            }

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

            curtsopp.calcAvg(); // Just to be sure...

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

            curtsopp.calcAvg();

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

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
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
            }

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

            dt_ov.Rows.Add(dr2);

            dr2 = dt_ov.NewRow();

            curtsopp.calcAvg(); // Just to be sure...

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

            curtsopp.calcAvg();

            #endregion

            var dv_ov = new DataView(dt_ov) {AllowNew = false};

            dgvTeamStats.DataContext = dv_ov;

            dgvBoxScores.ItemsSource = bsrList;

            #endregion
        }

        private void UpdateSplitStats()
        {
            // Prepare Queries
            string qr_home = String.Format("select * from GameResults where (T2Name LIKE \"{0}\")", curTeam);
            string qr_away = String.Format("select * from GameResults where (T1Name LIKE \"{0}\")", curTeam);
            string qr_wins = String.Format("select * from GameResults where "
                                           + "((T1Name LIKE \"{0}\" AND T1PTS > T2PTS) "
                                           + "OR (T2Name LIKE \"{0}\" AND T2PTS > T1PTS))",
                                           curTeam);
            string qr_losses = String.Format("select * from GameResults where "
                                             + "((T1Name LIKE \"{0}\" AND T1PTS < T2PTS) "
                                             + "OR (T2Name LIKE \"{0}\" AND T2PTS < T1PTS))",
                                             curTeam);
            string qr_season = String.Format("select * from GameResults where "
                                             + "(T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") "
                                             + "AND IsPlayoff LIKE \"False\"",
                                             curTeam);
            string qr_playoffs = String.Format("select * from GameResults where "
                                               + "(T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") "
                                               + "AND IsPlayoff LIKE \"True\"",
                                               curTeam);

            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                qr_home = SQLiteDatabase.AddDateRangeToSQLQuery(qr_home, dtpStart.SelectedDate.GetValueOrDefault(),
                                                                dtpEnd.SelectedDate.GetValueOrDefault());
                qr_away = SQLiteDatabase.AddDateRangeToSQLQuery(qr_away, dtpStart.SelectedDate.GetValueOrDefault(),
                                                                dtpEnd.SelectedDate.GetValueOrDefault());
                qr_wins = SQLiteDatabase.AddDateRangeToSQLQuery(qr_wins, dtpStart.SelectedDate.GetValueOrDefault(),
                                                                dtpEnd.SelectedDate.GetValueOrDefault());
                qr_losses = SQLiteDatabase.AddDateRangeToSQLQuery(qr_losses, dtpStart.SelectedDate.GetValueOrDefault(),
                                                                  dtpEnd.SelectedDate.GetValueOrDefault());
                qr_season = SQLiteDatabase.AddDateRangeToSQLQuery(qr_season, dtpStart.SelectedDate.GetValueOrDefault(),
                                                                  dtpEnd.SelectedDate.GetValueOrDefault());
                qr_playoffs = SQLiteDatabase.AddDateRangeToSQLQuery(qr_playoffs,
                                                                    dtpStart.SelectedDate.GetValueOrDefault(),
                                                                    dtpEnd.SelectedDate.GetValueOrDefault());
            }
            else
            {
                string s = " AND SeasonNum = " + cmbSeasonNum.SelectedValue;
                qr_home += s;
                qr_away += s;
                qr_wins += s;
                qr_losses += s;
                qr_season += s;
                qr_playoffs += s;
            }

            /*
            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);
            */

            // Clear Team Stats
            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats("Opponents");

            DataTable res2 = db.GetDataTable(qr_home);
            AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
            DataRow dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Home");
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_away);
            AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Away");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_wins);
            AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Wins");
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_losses);
            AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Losses");
            dt_ss.Rows.Add(dr);

            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_season);
            AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Season");
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_playoffs);
            AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Playoffs");
            dt_ss.Rows.Add(dr);

            #region Monthly split stats

            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                DateTime dStart = dtpStart.SelectedDate.GetValueOrDefault();
                DateTime dEnd = dtpEnd.SelectedDate.GetValueOrDefault();

                DateTime dCur = dStart;
                var qrm = new List<string>();

                while (true)
                {
                    if (new DateTime(dCur.Year, dCur.Month, 1) == new DateTime(dEnd.Year, dEnd.Month, 1))
                    {
                        string s = String.Format("select * from GameResults where "
                                                 + "(T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") "
                                                 + "AND (Date >= \"{1}\" AND Date <=\"{2}\");",
                                                 curTeam,
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dCur),
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dEnd));

                        qrm.Add(s);
                        break;
                    }
                    else
                    {
                        string s = String.Format("select * from GameResults where "
                                                 + "(T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") "
                                                 + "AND (Date >= \"{1}\" AND Date <=\"{2}\");",
                                                 curTeam,
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dCur),
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(
                                                     new DateTime(dCur.Year, dCur.Month, 1).AddMonths(1).AddDays(-1)));

                        qrm.Add(s);

                        dCur = new DateTime(dCur.Year, dCur.Month, 1).AddMonths(1);
                    }
                }

                dr = dt_ss.NewRow();
                dr["Type"] = " ";
                dt_ss.Rows.Add(dr);

                int i = 0;
                foreach (string q in qrm)
                {
                    ts = new TeamStats(curTeam);
                    tsopp = new TeamStats("Opponents");

                    res2 = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScores(res2, ref ts, ref tsopp);
                    dr = dt_ss.NewRow();
                    DateTime label = new DateTime(dStart.Year, dStart.Month, 1).AddMonths(i);
                    CreateDataRowFromTeamStats(ts, ref dr,
                                               label.Year.ToString() + " " + String.Format("{0:MMMM}", label));
                    dt_ss.Rows.Add(dr);
                    i++;
                }
            }

            #endregion

            // DataTable is done, create DataView and load into DataGrid
            var dv_ss = new DataView(dt_ss) {AllowEdit = false, AllowNew = false};

            dgvSplit.DataContext = dv_ss;
        }

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

                if (cmbTeam.SelectedIndex == -1) return;
                if (cmbSeasonNum.SelectedIndex == -1) return;
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
            Title = cmbTeam.SelectedItem + " Team Overview - " + (ts.getGames() + ts.getPlayoffGames()) +
                    " games played";

            UpdateHeadToHead();

            UpdateYearlyStats();

            UpdatePlayerAndMetricStats();

            UpdateBest();
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            return Misc.GetCurTeamFromDisplayName(tst, p);
        }

        private string GetDisplayNameFromTeam(string p)
        {
            return Misc.GetDisplayNameFromTeam(tst, p);
        }

        private void UpdateBest()
        {
            txbPlayer1.Text = "";
            txbPlayer2.Text = "";
            txbPlayer3.Text = "";
            txbPlayer4.Text = "";
            txbPlayer5.Text = "";
            txbPlayer6.Text = "";

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

                PlayerStatsRow psr1 = templist[0];
                string text = psr1.GetBestStats(4);
                txbPlayer1.Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")\n\n" + text;

                PlayerStatsRow psr2 = templist[1];
                text = psr2.GetBestStats(4);
                txbPlayer2.Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + ")\n\n" + text;

                PlayerStatsRow psr3 = templist[2];
                text = psr3.GetBestStats(4);
                txbPlayer3.Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + ")\n\n" + text;

                PlayerStatsRow psr4 = templist[3];
                text = psr4.GetBestStats(4);
                txbPlayer4.Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + ")\n\n" + text;

                PlayerStatsRow psr5 = templist[4];
                text = psr5.GetBestStats(4);
                txbPlayer5.Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + ")\n\n" + text;

                PlayerStatsRow psr6 = templist[5];
                text = psr6.GetBestStats(4);
                txbPlayer6.Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + ")\n\n" + text;
            }
            catch (Exception)
            {
            }
        }

        private void UpdatePlayerAndMetricStats()
        {
            string playersT = "Players";
            if (curSeason != maxSeason) playersT += "S" + curSeason;

            string q = "select * from " + playersT + " where TeamFin LIKE \"" + curTeam + "\"";
            DataTable res = db.GetDataTable(q);

            psrList = new ObservableCollection<PlayerStatsRow>();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                foreach (DataRow r in res.Rows)
                {
                    PlayerStats ps = pst[Convert.ToInt32(r["ID"].ToString())];
                    psrList.Add(new PlayerStatsRow(ps));
                }

                dgvMetricStatsPERColumn.Visibility = Visibility.Visible;
                dgvMetricStatsEFFColumn.Visibility = Visibility.Visible;
                dgvMetricStatsPPRColumn.Visibility = Visibility.Visible;
            }
            else
            {
                foreach (DataRow r in res.Rows)
                {
                    var psBetween = new PlayerStats(new Player(r));

                    q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID) "
                        + "where PlayerID = " + r["ID"];
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += " ORDER BY Date DESC";
                    DataTable res2 = db.GetDataTable(q);

                    foreach (DataRow r2 in res2.Rows)
                    {
                        var pbs = new PlayerBoxScore(r2);
                        psBetween.AddBoxScore(pbs);
                    }
                    var curTSAll = new TeamStats(curTeam);
                    curTSAll.AddTeamStats(curts, "All");
                    var curTSOppAll = new TeamStats(curTeam);
                    curTSOppAll.AddTeamStats(curtsopp, "All");
                    curTSAll.CalcMetrics(curTSOppAll);
                    psBetween.CalcMetrics(curTSAll, curTSOppAll, new TeamStats("$$Empty"));

                    psrList.Add(new PlayerStatsRow(psBetween));

                    dgvMetricStatsPERColumn.Visibility = Visibility.Collapsed;
                    dgvMetricStatsEFFColumn.Visibility = Visibility.Collapsed;
                    dgvMetricStatsPPRColumn.Visibility = Visibility.Collapsed;
                }
            }

            dgvPlayerStats.ItemsSource = psrList;
            dgvMetricStats.ItemsSource = psrList;
        }

        private void UpdateHeadToHead()
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        private void UpdateYearlyStats()
        {
            #region Prepare Yearly Stats

            string currentDB = MainWindow.currentDB;
            curSeason = MainWindow.curSeason;
            maxSeason = SQLiteIO.getMaxSeason(currentDB);

            TeamStats ts = tst[MainWindow.TeamOrder[curTeam]];
            TeamStats tsopp;
            var tsAllSeasons = new TeamStats("All Seasons");
            var tsAllPlayoffs = new TeamStats("All Playoffs");
            var tsAll = new TeamStats("All Games");
            tsAllSeasons.AddTeamStats(ts, "Season");
            tsAllPlayoffs.AddTeamStats(ts, "Playoffs");
            tsAll.AddTeamStats(ts, "All");

            DataRow drcur = dt_yea.NewRow();
            DataRow drcur_pl = dt_yea.NewRow();
            CreateDataRowFromTeamStats(ts, ref drcur, "Season " + curSeason.ToString());

            bool playedInPlayoffs = false;
            if (ts.pl_winloss[0] + ts.pl_winloss[1] > 0)
            {
                CreateDataRowFromTeamStats(ts, ref drcur_pl, "Playoffs " + curSeason.ToString(), true);
                playedInPlayoffs = true;
            }

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
                        CreateDataRowFromTeamStats(ts, ref dr3_pl,
                                                   "Playoffs " + j.ToString(), true);
                        dt_yea.Rows.Add(dr3_pl);
                    }

                    tsAllSeasons.AddTeamStats(ts, "Season");
                    tsAllPlayoffs.AddTeamStats(ts, "Playoffs");
                    tsAll.AddTeamStats(ts, "All");
                }
                else
                {
                    dt_yea.Rows.Add(drcur);
                    if (playedInPlayoffs) dt_yea.Rows.Add(drcur_pl);
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

            #endregion
        }

        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string msg =
                    TeamStats.TeamAveragesAndRankings(GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()),
                                                      tst, MainWindow.TeamOrder);
                if (msg != "")
                {
                    var cw = new CopyableMessageWindow(msg, GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()),
                                                       TextAlignment.Center);
                    cw.ShowDialog();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No team selected.");
            }
        }

        private void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores"
                    + ") or edit the box-scores themselves.", "NBA Stats Tracker", MessageBoxButton.OK,
                    MessageBoxImage.Information);
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

            tst[id].pl_winloss[0] = Convert.ToByte(myCell(5, 2));
            tst[id].pl_winloss[1] = Convert.ToByte(myCell(5, 3));
            tst[id].pl_stats[t.PF] = Convert.ToUInt16(myCell(5, 4));
            tst[id].pl_stats[t.PA] = Convert.ToUInt16(myCell(5, 5));

            parts = myCell(5, 7).Split('-');
            tst[id].pl_stats[t.FGM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[t.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 9).Split('-');
            tst[id].pl_stats[t.TPM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[t.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 11).Split('-');
            tst[id].pl_stats[t.FTM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[t.FTA] = Convert.ToUInt16(parts[1]);

            tst[id].pl_stats[t.OREB] = Convert.ToUInt16(myCell(5, 14));
            tst[id].pl_stats[t.DREB] = Convert.ToUInt16(myCell(5, 15));

            tst[id].pl_stats[t.AST] = Convert.ToUInt16(myCell(5, 16));
            tst[id].pl_stats[t.TO] = Convert.ToUInt16(myCell(5, 17));
            tst[id].pl_stats[t.STL] = Convert.ToUInt16(myCell(5, 18));
            tst[id].pl_stats[t.BLK] = Convert.ToUInt16(myCell(5, 19));
            tst[id].pl_stats[t.FOUL] = Convert.ToUInt16(myCell(5, 20));
            tst[id].pl_stats[t.MINS] = Convert.ToUInt16(myCell(5, 21));

            tst[id].calcAvg();


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

            tstopp[id].pl_winloss[0] = Convert.ToByte(myCell(5, 2));
            tstopp[id].pl_winloss[1] = Convert.ToByte(myCell(5, 3));
            tstopp[id].pl_stats[t.PF] = Convert.ToUInt16(myCell(5, 4));
            tstopp[id].pl_stats[t.PA] = Convert.ToUInt16(myCell(5, 5));

            parts = myCell(5, 7).Split('-');
            tstopp[id].pl_stats[t.FGM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[t.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 9).Split('-');
            tstopp[id].pl_stats[t.TPM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[t.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 11).Split('-');
            tstopp[id].pl_stats[t.FTM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[t.FTA] = Convert.ToUInt16(parts[1]);

            tstopp[id].pl_stats[t.OREB] = Convert.ToUInt16(myCell(5, 14));
            tstopp[id].pl_stats[t.DREB] = Convert.ToUInt16(myCell(5, 15));

            tstopp[id].pl_stats[t.AST] = Convert.ToUInt16(myCell(5, 16));
            tstopp[id].pl_stats[t.TO] = Convert.ToUInt16(myCell(5, 17));
            tstopp[id].pl_stats[t.STL] = Convert.ToUInt16(myCell(5, 18));
            tstopp[id].pl_stats[t.BLK] = Convert.ToUInt16(myCell(5, 19));
            tstopp[id].pl_stats[t.FOUL] = Convert.ToUInt16(myCell(5, 20));
            tstopp[id].pl_stats[t.MINS] = Convert.ToUInt16(myCell(5, 21));

            tstopp[id].calcAvg();

            Dictionary<int, PlayerStats> playersToUpdate =
                psrList.Select(cur => new PlayerStats(cur)).ToDictionary(ps => ps.ID);

            SQLiteIO.saveSeasonToDatabase(MainWindow.currentDB, tst, tstopp, playersToUpdate,
                                          curSeason, maxSeason, partialUpdate: true);
            SQLiteIO.LoadSeason(MainWindow.currentDB, out tst, out tstopp, out pst, out MainWindow.TeamOrder,
                                ref MainWindow.pt, ref MainWindow.bshist, _curSeason: curSeason,
                                doNotLoadBoxScores: true);

            int temp = cmbTeam.SelectedIndex;
            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = temp;
        }

        private string myCell(int row, int col)
        {
            return GetCellValue(dgvTeamStats, row, col);
        }

        private string GetCellValue(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
                return dataRowView.Row.ItemArray[col].ToString();

            return null;
        }

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

            int[][] rating = TeamStats.CalculateTeamRankings(tst);
            if (rating.Length != 1)
            {
                string msg = TeamStats.TeamScoutingReport(rating, id,
                                                          GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()));
                var cw = new CopyableMessageWindow(msg, "Scouting Report", TextAlignment.Left);
                cw.ShowDialog();
            }
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            changingTimeframe = true;
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            rbStatsBetween.IsChecked = true;
            changingTimeframe = false;
            cmbTeam_SelectionChanged(sender, null);
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            rbStatsBetween.IsChecked = true;
            cmbTeam_SelectionChanged(sender, null);
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe) cmbTeam_SelectionChanged(sender, null);
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe) cmbTeam_SelectionChanged(sender, null);
        }

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

        private void btnPrevOpp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == 0) cmbOppTeam.SelectedIndex = cmbOppTeam.Items.Count - 1;
            else cmbOppTeam.SelectedIndex--;
        }

        private void btnNextOpp_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == cmbOppTeam.Items.Count - 1) cmbOppTeam.SelectedIndex = 0;
            else cmbOppTeam.SelectedIndex++;
        }

        private void cmbOppTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingOppTeam) return;

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

            var partialPST = new Dictionary<int, PlayerStats>();

            if (cmbOppTeam.SelectedIndex == cmbTeam.SelectedIndex)
            {
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

            DataTable res;

            if (dt_hth.Rows.Count > 1) dt_hth.Rows.RemoveAt(dt_hth.Rows.Count - 1);

            bool doPartial = true;

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                string q = String.Format("select * from GameResults " +
                                         "where (((T1Name LIKE \"{0}\") AND (T2Name LIKE \"{1}\")) " +
                                         "OR " +
                                         "((T1Name LIKE \"{1}\") AND (T2Name LIKE \"{0}\"))) AND SeasonNum = {2} ORDER BY Date DESC",
                                         cmbTeam.SelectedItem,
                                         cmbOppTeam.SelectedItem,
                                         curSeason);

                res = db.GetDataTable(q);

                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    doPartial = false;

                    ts = tst[iown];
                    ts.calcAvg();

                    tsopp = tst[iopp];
                    tsopp.calcAvg();

                    /*
                    i = -1;
                    foreach (DataColumn col in dt_hth.Columns)
                    {
                        i++;
                        if (i == 0) continue;
                    
                        float val1 = Convert.ToSingle(GetCellValue(dgvHTHStats, 0, i));
                        float val2 = Convert.ToSingle(GetCellValue(dgvHTHStats, 1, i));
                        if (val1 > val2)
                        {
                            //
                        }
                    }
                    */
                }
                else
                {
                    q = String.Format("select * from GameResults " +
                                      "where ((((T1Name LIKE \"{0}\") AND (T2Name LIKE \"{1}\")) " +
                                      "OR " +
                                      "((T1Name LIKE \"{1}\") AND (T2Name LIKE \"{0}\"))) AND SeasonNum = {2}) ORDER BY Date DESC;",
                                      cmbTeam.SelectedItem,
                                      cmbOppTeam.SelectedItem,
                                      curSeason);

                    res = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                }
            }
            else
            {
                string q =
                    String.Format(
                        "select * from GameResults where ((((T1Name LIKE \"{0}\") AND (T2Name LIKE \"{1}\")) " +
                        "OR ((T1Name LIKE \"{1}\") AND (T2Name LIKE \"{0}\"))) AND ((Date >= \"{2}\") AND (Date <= \"{3}\"))) ORDER BY Date DESC",
                        cmbTeam.SelectedItem,
                        cmbOppTeam.SelectedItem,
                        SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault()),
                        SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()));
                res = db.GetDataTable(q);

                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    string q2 =
                        String.Format(
                            "select * from GameResults where (((T1Name LIKE \"{0}\") OR (T2Name LIKE \"{1}\") " +
                            "OR (T1Name LIKE \"{1}\") OR (T2Name LIKE \"{0}\")) AND ((Date >= \"{2}\") AND (Date <= \"{3}\"))) ORDER BY Date DESC",
                            cmbTeam.SelectedItem,
                            cmbOppTeam.SelectedItem,
                            SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault()),
                            SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()));
                    res = db.GetDataTable(q2);
                    AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                }
                else
                {
                    AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                }
            }

            if (doPartial)
            {
                foreach (DataRow dr2 in res.Rows)
                {
                    string q2 =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE GameResults.GameID={0}", dr2["GameID"]);

                    DataTable res2 = db.GetDataTable(q2);
                    foreach (DataRow dr3 in res2.Rows)
                    {
                        int pID = Tools.getInt(dr3, "PlayerID");
                        int season = Tools.getInt(dr3, "SeasonNum");
                        string playersT = "Players";
                        if (season != maxSeason) playersT += "S" + season;
                        string q3 = String.Format("SELECT * FROM {0} WHERE ID={1}", playersT, pID);
                        DataTable res3 = db.GetDataTable(q3);

                        PlayerStats ps;
                        if (partialPST.ContainsKey(pID))
                        {
                            ps = partialPST[pID];
                        }
                        else
                        {
                            ps = new PlayerStats(new Player(res3.Rows[0]));
                        }
                        ps.AddBoxScore(new PlayerBoxScore(dr3));
                        partialPST[pID] = ps;
                    }
                }
            }
            else
            {
                partialPST = pst;
            }

            //ts.CalcMetrics(tsopp);
            //tsopp.CalcMetrics(ts);
            var ls = new TeamStats();
            ls.AddTeamStats(ts, "All");
            ls.AddTeamStats(tsopp, "All");
            List<int> keys = partialPST.Keys.ToList();
            List<PlayerStatsRow> teamPMSRList = new List<PlayerStatsRow>(), oppPMSRList = new List<PlayerStatsRow>();
            foreach (int key in keys)
            {
                if (partialPST[key].TeamF == ts.name)
                {
                    partialPST[key].CalcMetrics(ts, tsopp, ls, GmScOnly: true);
                    teamPMSRList.Add(new PlayerStatsRow(partialPST[key]));
                }
                else if (partialPST[key].TeamF == tsopp.name)
                {
                    partialPST[key].CalcMetrics(tsopp, ts, ls, GmScOnly: true);
                    oppPMSRList.Add(new PlayerStatsRow(partialPST[key]));
                }
            }

            foreach (DataRow r in res.Rows)
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(curTeam))
                {
                    DataRow bsr = dt_hth_bs.NewRow();
                    bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                    bsr["Home-Away"] = "Away";

                    if (t1pts > t2pts)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }

                    bsr["Score"] = r["T1PTS"] + "-" + r["T2PTS"];
                    bsr["GameID"] = r["GameID"].ToString();

                    dt_hth_bs.Rows.Add(bsr);
                }
                else
                {
                    DataRow bsr = dt_hth_bs.NewRow();
                    bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                    bsr["Home-Away"] = "Home";

                    if (t2pts > t1pts)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }

                    bsr["Score"] = r["T2PTS"] + "-" + r["T1PTS"];
                    bsr["GameID"] = r["GameID"].ToString();

                    dt_hth_bs.Rows.Add(bsr);
                }
            }

            dt_hth.Clear();

            DataRow dr = dt_hth.NewRow();

            CreateDataRowFromTeamStats(ts, ref dr, "Averages");

            dt_hth.Rows.Add(dr);

            dr = dt_hth.NewRow();

            CreateDataRowFromTeamStats(tsopp, ref dr, "Opp Avg");

            dt_hth.Rows.Add(dr);

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                dr = dt_hth.NewRow();

                CreateDataRowFromTeamStats(ts, ref dr, "Playoffs", true);

                dt_hth.Rows.Add(dr);

                dr = dt_hth.NewRow();

                CreateDataRowFromTeamStats(tsopp, ref dr, "Opp Pl Avg", true);

                dt_hth.Rows.Add(dr);
            }

            dv_hth = new DataView(dt_hth) {AllowNew = false, AllowEdit = false};

            dgvHTHStats.DataContext = dv_hth;

            var dv_hth_bs = new DataView(dt_hth_bs) {AllowNew = false, AllowEdit = false};

            dgvHTHBoxScores.DataContext = dv_hth_bs;

            List<PlayerStatsRow> guards = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                                 {
                                                                     if (psr.Position1.EndsWith("G")) return true;
                                                                     return false;
                                                                 }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            List<PlayerStatsRow> fors = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                               {
                                                                   if (psr.Position1.EndsWith("F")) return true;
                                                                   return false;
                                                               }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            List<PlayerStatsRow> centers = teamPMSRList.Where(delegate(PlayerStatsRow psr)
                                                                  {
                                                                      if (psr.Position1.EndsWith("C")) return true;
                                                                      return false;
                                                                  }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                string text = guards[0].GetBestStats(4);
                txbTeam1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + "\n\n" + text;

                text = fors[0].GetBestStats(4);
                txbTeam2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + "\n\n" + text;

                text = centers[0].GetBestStats(4);
                txbTeam3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + "\n\n" + text;
            }
            catch
            {
            }

            guards = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                           {
                                               if (psr.Position1.EndsWith("G")) return true;
                                               return false;
                                           }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            fors = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                         {
                                             if (psr.Position1.EndsWith("F")) return true;
                                             return false;
                                         }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            centers = oppPMSRList.Where(delegate(PlayerStatsRow psr)
                                            {
                                                if (psr.Position1.EndsWith("C")) return true;
                                                return false;
                                            }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                string text = guards[0].GetBestStats(4);
                txbOpp1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + "\n\n" + text;

                text = fors[0].GetBestStats(4);
                txbOpp2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + "\n\n" + text;

                text = centers[0].GetBestStats(4);
                txbOpp3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + "\n\n" + text;
            }
            catch
            {
            }

            changingOppTeam = false;
        }

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
            }
        }

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

            tst = MainWindow.tst;
            pst = MainWindow.pst;
            tstopp = MainWindow.tstopp;

            PopulateTeamsCombo();

            rankings = TeamStats.CalculateTeamRankings(tst);
            pl_rankings = TeamStats.CalculateTeamRankings(tst, playoffs: true);

            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1).AddDays(1);

            PopulateSeasonCombo();

            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvSplit.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvYearly.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            cmbTeam.SelectedIndex = -1;
            if (!String.IsNullOrWhiteSpace(teamToLoad)) cmbTeam.SelectedItem = teamToLoad;

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

        public static void AddToTeamStatsFromSQLBoxScores(DataTable res, ref TeamStats ts, ref TeamStats tsopp)
        {
            foreach (DataRow r in res.Rows)
            {
                AddToTeamStatsFromSQLBoxScore(r, ref ts, ref tsopp);
            }
        }

        public static void AddToTeamStatsFromSQLBoxScore(DataRow r, ref TeamStats ts, ref TeamStats tsopp)
        {
            bool playoffs = Tools.getBoolean(r, "isPlayoff");
            if (!playoffs)
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.name))
                {
                    if (t1pts > t2pts) ts.winloss[0]++;
                    else ts.winloss[1]++;
                    tsopp.stats[t.MINS] = ts.stats[t.MINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.stats[t.PA] = ts.stats[t.PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.stats[t.PF] = ts.stats[t.PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    ts.stats[t.OREB] += T1oreb;

                    ts.stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    ts.stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    ts.stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");

                    tsopp.stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    tsopp.stats[t.OREB] += T2oreb;

                    tsopp.stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    tsopp.stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    tsopp.stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts) ts.winloss[0]++;
                    else ts.winloss[1]++;
                    tsopp.stats[t.MINS] = ts.stats[t.MINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.stats[t.PA] = ts.stats[t.PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.stats[t.PF] = ts.stats[t.PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    ts.stats[t.OREB] += T2oreb;

                    ts.stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    ts.stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    ts.stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");

                    tsopp.stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    tsopp.stats[t.OREB] += T1oreb;

                    tsopp.stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    tsopp.stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    tsopp.stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");
                }

                tsopp.winloss[1] = ts.winloss[0];
                tsopp.winloss[0] = ts.winloss[1];
            }
            else
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.name))
                {
                    if (t1pts > t2pts) ts.pl_winloss[0]++;
                    else ts.pl_winloss[1]++;
                    tsopp.pl_stats[t.MINS] = ts.pl_stats[t.MINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.pl_stats[t.PA] = ts.pl_stats[t.PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.pl_stats[t.PF] = ts.pl_stats[t.PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.pl_stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.pl_stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.pl_stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.pl_stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.pl_stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.pl_stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.pl_stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    ts.pl_stats[t.OREB] += T1oreb;

                    ts.pl_stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.pl_stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.pl_stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    ts.pl_stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    ts.pl_stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");

                    tsopp.pl_stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.pl_stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.pl_stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.pl_stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.pl_stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.pl_stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.pl_stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    tsopp.pl_stats[t.OREB] += T2oreb;

                    tsopp.pl_stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.pl_stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.pl_stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    tsopp.pl_stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    tsopp.pl_stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts) ts.pl_winloss[0]++;
                    else ts.pl_winloss[1]++;
                    tsopp.pl_stats[t.MINS] = ts.pl_stats[t.MINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.pl_stats[t.PA] = ts.pl_stats[t.PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.pl_stats[t.PF] = ts.pl_stats[t.PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.pl_stats[t.FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.pl_stats[t.FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.pl_stats[t.TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.pl_stats[t.TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.pl_stats[t.FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.pl_stats[t.FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.pl_stats[t.DREB] += (ushort) (T2reb - T2oreb);
                    ts.pl_stats[t.OREB] += T2oreb;

                    ts.pl_stats[t.STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.pl_stats[t.TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.pl_stats[t.BLK] += Tools.getUInt16(r, "T2BLK");
                    ts.pl_stats[t.AST] += Tools.getUInt16(r, "T2AST");
                    ts.pl_stats[t.FOUL] += Tools.getUInt16(r, "T2FOUL");

                    tsopp.pl_stats[t.FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.pl_stats[t.FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.pl_stats[t.TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.pl_stats[t.TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.pl_stats[t.FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.pl_stats[t.FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.pl_stats[t.DREB] += (ushort) (T1reb - T1oreb);
                    tsopp.pl_stats[t.OREB] += T1oreb;

                    tsopp.pl_stats[t.STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.pl_stats[t.TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.pl_stats[t.BLK] += Tools.getUInt16(r, "T1BLK");
                    tsopp.pl_stats[t.AST] += Tools.getUInt16(r, "T1AST");
                    tsopp.pl_stats[t.FOUL] += Tools.getUInt16(r, "T1FOUL");
                }

                tsopp.pl_winloss[1] = ts.pl_winloss[0];
                tsopp.pl_winloss[0] = ts.pl_winloss[1];
            }

            ts.calcAvg();
            tsopp.calcAvg();
        }

        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            if (changingOppRange) return;

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

        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            if (changingOppRange) return;

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

        private void rbBSDetailed_Checked(object sender, RoutedEventArgs e)
        {
            cmbTeam_SelectionChanged(null, null);
        }

        private void rbBSSimple_Checked(object sender, RoutedEventArgs e)
        {
            cmbTeam_SelectionChanged(null, null);
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                rbStatsAllTime.IsChecked = true;
                if (cmbSeasonNum.SelectedIndex == -1) return;

                curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

                teamsT = "Teams";
                pl_teamsT = "PlayoffTeams";
                oppT = "Opponents";
                pl_oppT = "PlayoffOpponents";
                if (curSeason != maxSeason)
                {
                    string s = "S" + curSeason;
                    teamsT += s;
                    pl_teamsT += s;
                    oppT += s;
                    pl_oppT += s;
                }

                SQLiteIO.LoadSeason(MainWindow.currentDB, out tst, out tstopp, out pst, out MainWindow.TeamOrder,
                                    ref MainWindow.pt, ref MainWindow.bshist, _curSeason: curSeason);
                PopulateTeamsCombo();
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

            //MainWindow.ChangeSeason(curSeason, Convert.ToInt32(cmbSeasonNum.Items[cmbSeasonNum.Items.Count-1].ToString()));
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayerStats.SelectedCells.Count > 0)
            {
                var row = (PlayerStatsRow) dgvPlayerStats.SelectedItems[0];
                int playerID = row.ID;

                var pow = new PlayerOverviewWindow(curTeam, playerID);
                pow.ShowDialog();

                UpdatePlayerAndMetricStats();
            }
        }

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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MainWindow.tst = tst;
            MainWindow.tstopp = tstopp;
            MainWindow.pst = pst;
        }

        private void btnChangeName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ibw = new InputBoxWindow("Please enter the new name for the team",
                                             tst[MainWindow.TeamOrder[curTeam]].displayName);
                ibw.ShowDialog();
            }
            catch
            {
                return;
            }

            string newname = MainWindow.input;
            var dict = new Dictionary<string, string> {{"DisplayName", newname}};
            db.Update(teamsT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(pl_teamsT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(oppT, dict, "Name LIKE \"" + curTeam + "\"");
            db.Update(pl_oppT, dict, "Name LIKE \"" + curTeam + "\"");

            int teamid = MainWindow.TeamOrder[curTeam];
            tst[teamid].displayName = newname;
            tstopp[teamid].displayName = newname;

            MainWindow.tst = tst;
            MainWindow.tstopp = tstopp;

            PopulateTeamsCombo();

            cmbTeam.SelectedItem = newname;
        }

        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }
    }
}