#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
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
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NBA_Stats_Tracker.Data;
using SQLite_Database;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    /// Interaction logic for leagueOverviewW.xaml
    /// </summary>
    public partial class leagueOverviewW
    {
        private static Dictionary<int, PlayerStats> _pst;
        private static TeamStats[] _tst, partialTST;
        private static TeamStats[] _tstopp, partialOppTST;
        private static int lastShownPlayerSeason;
        private static int lastShownLeadersSeason;
        private static int lastShownTeamSeason;
        private static int lastShownPlayoffSeason;
        private static int lastShownBoxSeason;
        private static string message;
        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly DataTable dt_bs;
        private readonly DataTable dt_pts;
        private readonly DataTable dt_ts;
/*
        private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
*/
        private int curSeason = MainWindow.curSeason;
        private List<PlayerStatsRow> psrList;
        private string q;
        private bool reload;
        private DataTable res;
        private TeamStats ts;
        private TeamStats tsopp;

        public leagueOverviewW(TeamStats[] tst, TeamStats[] tstopp, Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

            #region Prepare DataTables

            dt_ts = new DataTable();

            dt_ts.Columns.Add("Name");
            dt_ts.Columns.Add("Games", typeof (int));
            dt_ts.Columns.Add("Wins", typeof (int));
            dt_ts.Columns.Add("Losses", typeof (int));
            dt_ts.Columns.Add("W%", typeof (float));
            dt_ts.Columns.Add("Weff", typeof (float));
            dt_ts.Columns.Add("PF", typeof (float));
            dt_ts.Columns.Add("PA", typeof (float));
            dt_ts.Columns.Add("PD", typeof (float));
            dt_ts.Columns.Add("FG", typeof (float));
            dt_ts.Columns.Add("FGeff", typeof (float));
            dt_ts.Columns.Add("3PT", typeof (float));
            dt_ts.Columns.Add("3Peff", typeof (float));
            dt_ts.Columns.Add("FT", typeof (float));
            dt_ts.Columns.Add("FTeff", typeof (float));
            dt_ts.Columns.Add("REB", typeof (float));
            dt_ts.Columns.Add("OREB", typeof (float));
            dt_ts.Columns.Add("DREB", typeof (float));
            dt_ts.Columns.Add("AST", typeof (float));
            dt_ts.Columns.Add("TO", typeof (float));
            dt_ts.Columns.Add("STL", typeof (float));
            dt_ts.Columns.Add("BLK", typeof (float));
            dt_ts.Columns.Add("FOUL", typeof (float));

            dt_pts = new DataTable();

            dt_pts.Columns.Add("Name");
            dt_pts.Columns.Add("Games", typeof (int));
            dt_pts.Columns.Add("Wins", typeof (int));
            dt_pts.Columns.Add("Losses", typeof (int));
            dt_pts.Columns.Add("W%", typeof (float));
            dt_pts.Columns.Add("Weff", typeof (float));
            dt_pts.Columns.Add("PF", typeof (float));
            dt_pts.Columns.Add("PA", typeof (float));
            dt_pts.Columns.Add("PD", typeof (float));
            dt_pts.Columns.Add("FG", typeof (float));
            dt_pts.Columns.Add("FGeff", typeof (float));
            dt_pts.Columns.Add("3PT", typeof (float));
            dt_pts.Columns.Add("3Peff", typeof (float));
            dt_pts.Columns.Add("FT", typeof (float));
            dt_pts.Columns.Add("FTeff", typeof (float));
            dt_pts.Columns.Add("REB", typeof (float));
            dt_pts.Columns.Add("OREB", typeof (float));
            dt_pts.Columns.Add("DREB", typeof (float));
            dt_pts.Columns.Add("AST", typeof (float));
            dt_pts.Columns.Add("TO", typeof (float));
            dt_pts.Columns.Add("STL", typeof (float));
            dt_pts.Columns.Add("BLK", typeof (float));
            dt_pts.Columns.Add("FOUL", typeof (float));


            dt_bs = new DataTable();

            dt_bs.Columns.Add("Date");
            dt_bs.Columns.Add("Away");
            dt_bs.Columns.Add("AS", typeof (int));
            dt_bs.Columns.Add("Home");
            dt_bs.Columns.Add("HS", typeof (int));
            dt_bs.Columns.Add("GameID");

            #endregion

            _tst = tst;
            _tstopp = tstopp;
            _pst = new Dictionary<int, PlayerStats>(pst);

            PopulateSeasonCombo();

            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1).AddDays(1);

            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaders.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            foreach (TeamStats t in _tst)
            {
                if (t.displayName == p)
                {
                    if (t.isHidden)
                        throw new Exception("Requested team that is hidden: " + t.name);

                    return t.name;
                }
            }
            throw new Exception("Team not found: " + p);
        }

        private string GetDisplayNameFromTeam(string p)
        {
            foreach (TeamStats t in _tst)
            {
                if (t.name == p)
                {
                    if (t.isHidden)
                        throw new Exception("Requested team that is hidden: " + t.name);

                    return t.displayName;
                }
            }
            throw new Exception("Team not found: " + p);
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            reload = true;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            reload = true;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        private void PopulateSeasonCombo()
        {
            for (int i = SQLiteIO.getMaxSeason(MainWindow.currentDB); i > 0; i--)
            {
                cmbSeasonNum.Items.Add(i.ToString());
            }

            cmbSeasonNum.SelectedItem = MainWindow.curSeason.ToString();
        }

        private void tbcLeagueOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (reload || e.OriginalSource is TabControl)
                {
                    if (tbcLeagueOverview.SelectedItem == tabTeamStats)
                    {
                        bool doIt = false;
                        if (lastShownTeamSeason != curSeason) doIt = true;
                        else if (reload) doIt = true;

                        if (doIt)
                        {
                            PrepareTeamStats();
                            lastShownTeamSeason = curSeason;
                        }
                    }
                    else if (tbcLeagueOverview.SelectedItem == tabPlayoffStats)
                    {
                        bool doIt = false;
                        if (lastShownPlayoffSeason != curSeason) doIt = true;
                        else if (reload) doIt = true;

                        if (doIt)
                        {
                            PreparePlayoffStats();
                            lastShownPlayoffSeason = curSeason;
                        }
                    }
                    else if (tbcLeagueOverview.SelectedItem == tabLeaders)
                    {
                        bool doIt = false;
                        if (lastShownLeadersSeason != curSeason) doIt = true;
                        else if (reload) doIt = true;

                        if (doIt)
                        {
                            PreparePlayerAndMetricStats();
                            PrepareLeagueLeaders();
                            lastShownLeadersSeason = curSeason;
                        }
                    }
                    else if (tbcLeagueOverview.SelectedItem == tabPlayerStats ||
                             tbcLeagueOverview.SelectedItem == tabMetricStats)
                    {
                        bool doIt = false;
                        if (lastShownPlayerSeason != curSeason) doIt = true;
                        else if (reload) doIt = true;

                        if (doIt)
                        {
                            PreparePlayerAndMetricStats();
                            lastShownPlayerSeason = curSeason;
                        }
                    }
                    else if (tbcLeagueOverview.SelectedItem == tabBoxScores)
                    {
                        bool doIt = false;
                        if (lastShownBoxSeason != curSeason) doIt = true;
                        else if (reload) doIt = true;

                        if (doIt)
                        {
                            PrepareBoxScores();
                            lastShownBoxSeason = curSeason;
                        }
                    }
                    reload = false;
                }
            }
            catch (Exception)
            {
            }
        }

        private void PrepareLeagueLeaders()
        {
            int i = 0;
            var leadersList = new List<PlayerStatsRow>();
            bool allTime = rbStatsAllTime.IsChecked.GetValueOrDefault();

            tbcLeagueOverview.Visibility = Visibility.Hidden;

            var worker1 = new BackgroundWorker {WorkerReportsProgress = true};

            worker1.DoWork += delegate
                                  {
                                      if (allTime)
                                      {
                                          foreach (PlayerStatsRow psr in psrList)
                                          {
                                              leadersList.Add(ConvertToLeagueLeader(psr, _tst));
                                              worker1.ReportProgress(1);
                                          }
                                      }
                                      else
                                      {
                                          foreach (PlayerStatsRow psr in psrList)
                                          {
                                              leadersList.Add(ConvertToLeagueLeader(psr, partialTST));
                                              worker1.ReportProgress(1);
                                          }
                                      }
                                  };

            worker1.ProgressChanged += delegate
                                           {
                                               txbStatus.Text =
                                                   "Please wait while players are judged for their league leading ability (" +
                                                   ++i + "/" + psrList.Count + " completed)...";
                                           };

            worker1.RunWorkerCompleted += delegate
                                              {
                                                  leadersList.Sort(
                                                      (psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                                  leadersList.Reverse();

                                                  dgvLeaders.ItemsSource = leadersList;
                                                  tbcLeagueOverview.Visibility = Visibility.Visible;
                                                  txbStatus.Text = message;
                                              };

            worker1.RunWorkerAsync();
        }

        private void PreparePlayerAndMetricStats()
        {
            psrList = new List<PlayerStatsRow>();
            var pmsrList = new List<PlayerMetricStatsRow>();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                foreach (KeyValuePair<int, PlayerStats> kvp in _pst)
                {
                    var psr = new PlayerStatsRow(kvp.Value);
                    psr.TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName;
                    psrList.Add(psr);
                    var pmsr = new PlayerMetricStatsRow(kvp.Value)
                                   {TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName};
                    pmsrList.Add(pmsr);
                }
            }
            else
            {
                partialTST = new TeamStats[MainWindow.TeamOrder.Count];
                partialOppTST = new TeamStats[MainWindow.TeamOrder.Count];
                // Prepare Teams
                foreach (KeyValuePair<string, int> kvp in MainWindow.TeamOrder)
                {
                    string curTeam = kvp.Key;
                    int teamID = MainWindow.TeamOrder[curTeam];

                    q = "select * from GameResults where ((T1Name LIKE '" + curTeam + "') OR (T2Name LIKE '"
                        + curTeam + "')) AND ((Date >= '" +
                        SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault())
                        + "') AND (Date <= '" +
                        SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()) + "'))" +
                        " ORDER BY Date DESC";
                    res = db.GetDataTable(q);

                    partialTST[teamID] = new TeamStats(curTeam);
                    partialOppTST[teamID] = new TeamStats(curTeam);
                    foreach (DataRow r in res.Rows)
                    {
                        teamOverviewW.AddToTeamStatsFromSQLBoxScore(r, ref partialTST[teamID],
                                                                    ref partialOppTST[teamID]);
                    }
                }

                // Prepare Players
                q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID)";
                q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                          dtpEnd.SelectedDate.GetValueOrDefault());
                res = db.GetDataTable(q);

                var pstBetween = new Dictionary<int, PlayerStats>();

                foreach (DataRow r in res.Rows)
                {
                    var pbs = new PlayerBoxScore(r);
                    if (pstBetween.ContainsKey(pbs.PlayerID))
                    {
                        pstBetween[pbs.PlayerID].AddBoxScore(pbs);
                    }
                    else
                    {
                        string q2 = "select * from Players where ID = " + pbs.PlayerID;
                        DataTable res2 = db.GetDataTable(q2);

                        var p = new Player(res2.Rows[0]);

                        var ps = new PlayerStats(p);
                        ps.AddBoxScore(pbs);
                        pstBetween.Add(pbs.PlayerID, ps);
                    }
                }

                PlayerStats.CalculateAllMetrics(ref pstBetween, partialTST, partialOppTST, MainWindow.TeamOrder, true);

                foreach (KeyValuePair<int, PlayerStats> kvp in pstBetween)
                {
                    var psr = new PlayerStatsRow(kvp.Value);
                    psr.TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName;
                    psrList.Add(psr);
                    var pmsr = new PlayerMetricStatsRow(kvp.Value)
                                   {TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName};
                    pmsrList.Add(pmsr);
                }
            }

            psrList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
            psrList.Reverse();

            pmsrList.Sort(
                (pmsr1, pmsr2) => pmsr1.PER.CompareTo(pmsr2.PER));
            pmsrList.Reverse();

            dgvPlayerStats.ItemsSource = psrList;
            dgvMetricStats.ItemsSource = pmsrList;
        }

        private void PrepareBoxScores()
        {
            dt_bs.Clear();

            q = "select * from GameResults";

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                q += " where SeasonNum = " + cmbSeasonNum.SelectedItem;
            }
            else
            {
                q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                          dtpEnd.SelectedDate.GetValueOrDefault(), true);
            }
            q += " ORDER BY Date DESC";

            res = db.GetDataTable(q);

            foreach (DataRow dr in res.Rows)
            {
                DataRow r = dt_bs.NewRow();

                try
                {
                    r["Date"] = dr["Date"].ToString().Split(' ')[0];
                    r["Away"] = GetDisplayNameFromTeam(dr["T1Name"].ToString());
                    r["AS"] = Convert.ToInt32(dr["T1PTS"].ToString());
                    r["Home"] = GetDisplayNameFromTeam(dr["T2Name"].ToString());
                    r["HS"] = Convert.ToInt32(dr["T2PTS"].ToString());
                    r["GameID"] = dr["GameID"].ToString();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    continue;
                }

                dt_bs.Rows.Add(r);
            }

            var dv_bs = new DataView(dt_bs) {AllowNew = false, AllowEdit = false};

            dgvBoxScores.DataContext = dv_bs;
        }

        private void PreparePlayoffStats()
        {
            dt_pts.Clear();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                SQLiteIO.LoadSeason(MainWindow.currentDB, out _tst, out _tstopp, out _pst, out MainWindow.TeamOrder,
                                      ref MainWindow.pt, ref MainWindow.bshist,
                                      _curSeason: Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString()));

                foreach (TeamStats cur in _tst)
                {
                    if (cur.isHidden) continue;
                    if (cur.getPlayoffGames() == 0) continue;

                    DataRow r = dt_pts.NewRow();

                    teamOverviewW.CreateDataRowFromTeamStats(cur, ref r, GetDisplayNameFromTeam(cur.name), true);

                    dt_pts.Rows.Add(r);
                }
            }
            else
            {
                foreach (KeyValuePair<string, int> kvp in MainWindow.TeamOrder)
                {
                    q =
                        String.Format(
                            "select * from GameResults where ((T1Name LIKE '{0}' OR T2Name LIKE '{0}') AND IsPlayoff LIKE 'True');",
                            kvp.Key);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());

                    res = db.GetDataTable(q);

                    DataRow r = dt_pts.NewRow();

                    ts = new TeamStats(kvp.Key);
                    tsopp = new TeamStats();
                    teamOverviewW.AddToTeamStatsFromSQLBoxScore(res, ref ts, ref tsopp);
                    teamOverviewW.CreateDataRowFromTeamStats(ts, ref r, GetDisplayNameFromTeam(kvp.Key), true);

                    dt_pts.Rows.Add(r);
                }
            }

            // DataTable's ready, set DataView and fill DataGrid
            var dv_pts = new DataView(dt_pts) {AllowNew = false, AllowEdit = false, Sort = "Name ASC"};

            dgvPlayoffStats.DataContext = dv_pts;
        }

        private void PrepareTeamStats()
        {
            dt_ts.Clear();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out _tst, out _tstopp,
                                                       out MainWindow.TeamOrder);

                foreach (TeamStats cur in _tst)
                {
                    if (cur.isHidden) continue;

                    DataRow r = dt_ts.NewRow();

                    teamOverviewW.CreateDataRowFromTeamStats(cur, ref r, GetDisplayNameFromTeam(cur.name));

                    dt_ts.Rows.Add(r);
                }
            }
            else
            {
                foreach (KeyValuePair<string, int> kvp in MainWindow.TeamOrder)
                {
                    q =
                        String.Format(
                            "select * from GameResults where ((T1Name LIKE '{0}' OR T2Name LIKE '{0}') AND IsPlayoff LIKE 'False');",
                            kvp.Key);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());

                    res = db.GetDataTable(q);

                    DataRow r = dt_ts.NewRow();

                    ts = new TeamStats(kvp.Key);
                    tsopp = new TeamStats();
                    teamOverviewW.AddToTeamStatsFromSQLBoxScore(res, ref ts, ref tsopp);
                    teamOverviewW.CreateDataRowFromTeamStats(ts, ref r, GetDisplayNameFromTeam(kvp.Key));

                    dt_ts.Rows.Add(r);
                }
            }

            // DataTable's ready, set DataView and fill DataGrid
            var dv_ts = new DataView(dt_ts) {AllowNew = false, AllowEdit = false, Sort = "Weff DESC"};

            dgvTeamStats.DataContext = dv_ts;
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpEnd.IsEnabled = false;
                dtpStart.IsEnabled = false;
                cmbSeasonNum.IsEnabled = true;
            }
            catch
            {
            }
            reload = true;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpEnd.IsEnabled = true;
                dtpStart.IsEnabled = true;
                cmbSeasonNum.IsEnabled = false;
            }
            catch
            {
            }
            reload = true;
            tbcLeagueOverview_SelectionChanged(null, null);
        }

        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem);
            SQLiteIO.LoadSeason(MainWindow.currentDB, out _tst, out _tstopp, out _pst, out MainWindow.TeamOrder,
                                  ref MainWindow.pt, ref MainWindow.bshist, _curSeason: curSeason);
            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                reload = true;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvBoxScores.SelectedItems[0];
                int gameid = Convert.ToInt32(row["GameID"].ToString());

                var bsw = new boxScoreW(boxScoreW.Mode.View, gameid);
                try
                {
                    bsw.ShowDialog();

                    MainWindow.UpdateBoxScore();
                }
                catch
                {
                }
            }
        }

        private void dgvTeamStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvTeamStats.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvTeamStats.SelectedItems[0];
                string team = row["Name"].ToString();

                var tow = new teamOverviewW(team);
                tow.ShowDialog();
            }
        }

        private void dgvPlayoffStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayoffStats.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvPlayoffStats.SelectedItems[0];
                string team = row["Name"].ToString();

                var tow = new teamOverviewW(team);
                tow.ShowDialog();
            }
        }

        private PlayerStatsRow ConvertToLeagueLeader(PlayerStatsRow psr, TeamStats[] teamStats)
        {
            string team = psr.TeamF;
            ts = teamStats[MainWindow.TeamOrder[team]];
            int gamesTeam = ts.getGames();
            int gamesPlayer = psr.GP;
            var newpsr = new PlayerStatsRow(new PlayerStats(psr));

            // Below functions found using Eureqa II
            var gamesRequired = (int) Math.Ceiling(0.8522*gamesTeam); // Maximum error of 0
            var fgmRequired = (int) Math.Ceiling(3.65*gamesTeam); // Max error of 0
            var ftmRequired = (int) Math.Ceiling(1.52*gamesTeam);
            var tpmRequired = (int) Math.Ceiling(0.666671427752402*gamesTeam);
            var ptsRequired = (int) Math.Ceiling(17.07*gamesTeam);
            var rebRequired = (int) Math.Ceiling(9.74720677727814*gamesTeam);
            var astRequired = (int) Math.Ceiling(4.87*gamesTeam);
            var stlRequired = (int) Math.Ceiling(1.51957078555763*gamesTeam);
            var blkRequired = (int) Math.Ceiling(1.21*gamesTeam);
            var minRequired = (int) Math.Ceiling(24.39*gamesTeam);

            if (psr.FGM < fgmRequired) newpsr.FGp = float.NaN;
            if (psr.TPM < tpmRequired) newpsr.TPp = float.NaN;
            if (psr.FTM < ftmRequired) newpsr.FTp = float.NaN;

            if (gamesPlayer >= gamesRequired)
            {
                return newpsr;
            }

            if (psr.PTS < ptsRequired) newpsr.PPG = float.NaN;
            if (psr.REB < rebRequired) newpsr.RPG = float.NaN;
            if (psr.AST < astRequired) newpsr.APG = float.NaN;
            if (psr.STL < stlRequired) newpsr.SPG = float.NaN;
            if (psr.BLK < blkRequired) newpsr.BPG = float.NaN;
            if (psr.MINS < minRequired) newpsr.MPG = float.NaN;
            return newpsr;
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayerStats.SelectedCells.Count > 0)
            {
                var psr = (PlayerStatsRow) dgvPlayerStats.SelectedItems[0];

                var pow = new playerOverviewW(psr.TeamF, psr.ID);
                pow.ShowDialog();
            }
        }

        private void dgvLeaders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvLeaders.SelectedCells.Count > 0)
            {
                var psr = (PlayerStatsRow) dgvLeaders.SelectedItems[0];

                var pow = new playerOverviewW(psr.TeamF, psr.ID);
                pow.ShowDialog();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayerStats.CalculateAllMetrics(ref _pst, _tst, _tstopp, MainWindow.TeamOrder, true);
            lastShownPlayerSeason = 0;
            lastShownLeadersSeason = 0;
            lastShownTeamSeason = curSeason;
            lastShownPlayoffSeason = 0;
            lastShownBoxSeason = 0;
            message = txbStatus.Text;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            lastShownTeamSeason = 0;
            lastShownPlayerSeason = 0;
            lastShownPlayoffSeason = 0;
            lastShownLeadersSeason = 0;
            lastShownBoxSeason = 0;
        }
    }
}