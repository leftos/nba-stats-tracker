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
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// Interaction logic for LeagueOverviewWindow.xaml
    /// </summary>
    public partial class LeagueOverviewWindow
    {
        private static Dictionary<int, PlayerStats> _pst;
        private static Dictionary<int, TeamStats> _tst, partialTST;
        private static Dictionary<int, TeamStats> _tstopp, partialOppTST;
        private static int lastShownPlayerSeason;
        private static int lastShownLeadersSeason;
        private static int lastShownTeamSeason;
        private static int lastShownPlayoffSeason;
        private static int lastShownBoxSeason;
        private static string message;
        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly DataTable dt_bs;
        private readonly DataTable dt_lpts;
        private readonly DataTable dt_lts;
        private readonly DataTable dt_pts;
        private readonly DataTable dt_ts;
        private bool changingTimeframe;
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

        private enum TeamFilter
        {
            League,
            Conference,
            Division
        }

        private TeamFilter filterType;
        private string filterDescription;

        public LeagueOverviewWindow(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstopp, Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

            #region Prepare DataTables

            dt_ts = new DataTable();

            PrepareTeamDataTable(ref dt_ts);

            dt_lts = new DataTable();

            PrepareTeamDataTable(ref dt_lts);

            dt_pts = new DataTable();

            PreparePlayoffDataTable(ref dt_pts);

            dt_lpts = new DataTable();

            PreparePlayoffDataTable(ref dt_lpts);

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
            PopulateDivisionCombo();

            changingTimeframe = true;
            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1).AddDays(1);
            rbStatsAllTime.IsChecked = true;
            cmbDivConf.SelectedIndex = 0;
            changingTimeframe = false;

            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaders.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeagueTeamMetricStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvLeaguePlayoffStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            sem = new Semaphore(1, 1);
        }

        private void PopulateDivisionCombo()
        {
            List<ComboBoxItemWithEnabled> list = new List<ComboBoxItemWithEnabled>();
            list.Add(new ComboBoxItemWithEnabled("Whole League"));
            list.Add(new ComboBoxItemWithEnabled("-- Conferences --", false));
            foreach (var conf in MainWindow.Conferences)
            {
                list.Add(new ComboBoxItemWithEnabled(conf.Name));
            }
            list.Add(new ComboBoxItemWithEnabled("-- Divisions --", false));
            foreach (var div in MainWindow.Divisions)
            {
                var conf = MainWindow.Conferences.Find(conference => conference.ID == div.ConferenceID);
                list.Add(new ComboBoxItemWithEnabled(String.Format("{0}: {1}", conf.Name, div.Name)));
            }
            cmbDivConf.DisplayMemberPath = "Item";
            //cmbDivConf.SelectedValuePath = "Item";
            cmbDivConf.ItemsSource = list;
        }

        private void PreparePlayoffDataTable(ref DataTable dt_pts)
        {
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
        }

        private void PrepareTeamDataTable(ref DataTable dt_ts)
        {
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
        }

        private string GetCurTeamFromDisplayName(string p)
        {
            return Misc.GetCurTeamFromDisplayName(_tst, p);
        }

        private string GetDisplayNameFromTeam(string p)
        {
            return Misc.GetDisplayNameFromTeam(_tst, p);
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                }
                rbStatsBetween.IsChecked = true;
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
                changingTimeframe = false;
            }
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                }
                rbStatsBetween.IsChecked = true;
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
                changingTimeframe = false;
            }
        }

        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        private void tbcLeagueOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            if (reload || e.OriginalSource is TabControl)
            {
                if (tbcLeagueOverview.SelectedItem == tabTeamStats || tbcLeagueOverview.SelectedItem == tabTeamMetricStats)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownTeamSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PrepareTeamStats();
                        lastShownTeamSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabPlayoffStats)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownPlayoffSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PreparePlayoffStats();
                        lastShownPlayoffSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabLeaders)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownLeadersSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PreparePlayerAndMetricStats();
                        PrepareLeagueLeaders();
                        lastShownLeadersSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabPlayerStats || tbcLeagueOverview.SelectedItem == tabMetricStats ||
                         tbcLeagueOverview.SelectedItem == tabBest || tbcLeagueOverview.SelectedItem == tabStartingFive)
                {
                    cmbDivConf.IsEnabled = true;
                    bool doIt = false;
                    if (lastShownPlayerSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PreparePlayerAndMetricStats();
                        lastShownPlayerSeason = curSeason;
                    }
                }
                else if (tbcLeagueOverview.SelectedItem == tabBoxScores)
                {
                    cmbDivConf.IsEnabled = false;
                    bool doIt = false;
                    if (lastShownBoxSeason != curSeason)
                        doIt = true;
                    else if (reload)
                        doIt = true;

                    if (doIt)
                    {
                        PrepareBoxScores();
                        lastShownBoxSeason = curSeason;
                    }
                }
                reload = false;
            }
            //}
            //catch (Exception ex)
            //{
            //    //throw ex;
            //}
        }

        private void PrepareLeagueLeaders()
        {
            int i = 0;
            var leadersList = new List<PlayerStatsRow>();
            bool allTime = rbStatsAllTime.IsChecked.GetValueOrDefault();

            tbcLeagueOverview.Visibility = Visibility.Hidden;
            txbStatus.FontWeight = FontWeights.Bold;
            txbStatus.Text = "Please wait while players are judged for their league leading ability...";

            var worker1 = new BackgroundWorker {WorkerReportsProgress = true};

            worker1.DoWork += delegate
                                  {
                                      if (allTime)
                                      {
                                          foreach (PlayerStatsRow psr in psrList)
                                          {
                                              if (psr.isActive)
                                                  leadersList.Add(ConvertToLeagueLeader(psr, _tst));
                                              worker1.ReportProgress(1);
                                          }
                                      }
                                      else
                                      {
                                          foreach (PlayerStatsRow psr in psrList)
                                          {
                                              if (psr.isActive)
                                                  leadersList.Add(ConvertToLeagueLeader(psr, partialTST));
                                              worker1.ReportProgress(1);
                                          }
                                      }
                                  };

            worker1.ProgressChanged +=
                delegate
                    {
                        txbStatus.Text = "Please wait while players are judged for their league leading ability (" + ++i + "/" + psrList.Count +
                                         " completed)...";
                    };

            worker1.RunWorkerCompleted += delegate
                                              {
                                                  leadersList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                                  leadersList.Reverse();

                                                  dgvLeaders.ItemsSource = leadersList;
                                                  tbcLeagueOverview.Visibility = Visibility.Visible;
                                                  txbStatus.FontWeight = FontWeights.Normal;
                                                  txbStatus.Text = message;
                                              };

            worker1.RunWorkerAsync();
        }

        private static Semaphore sem;

        private void PreparePlayerAndMetricStats()
        {
            List<PlayerStatsRow> lpsr;
            List<PlayerStatsRow> pmsrList;
            List<PlayerStatsRow> lpmsr;
            psrList = new List<PlayerStatsRow>();
            lpsr = new List<PlayerStatsRow>();
            pmsrList = new List<PlayerStatsRow>();
            lpmsr = new List<PlayerStatsRow>();

            var worker1 = new BackgroundWorker {WorkerReportsProgress = true};

            var alltime = rbStatsAllTime.IsChecked.GetValueOrDefault();
            var startDate = dtpStart.SelectedDate.GetValueOrDefault();
            var endDate = dtpEnd.SelectedDate.GetValueOrDefault();
            txbStatus.FontWeight = FontWeights.Bold;
            txbStatus.Text = "Please wait while player averages and metric stats are being calculated...";

            int i = 0;

            var playerCount = -1;

            worker1.DoWork += delegate
                                  {
                                      sem.WaitOne();
                                      psrList = new List<PlayerStatsRow>();
                                      lpsr = new List<PlayerStatsRow>();
                                      pmsrList = new List<PlayerStatsRow>();
                                      lpmsr = new List<PlayerStatsRow>();
                                      if (alltime)
                                      {
                                          playerCount = _pst.Count;
                                          foreach (var kvp in _pst)
                                          {
                                              if (kvp.Value.isHidden)
                                                  continue;
                                              var psr = new PlayerStatsRow(kvp.Value);

                                              if (psr.isActive)
                                              {
                                                  if (!InCurrentFilter(_tst[MainWindow.TeamOrder[psr.TeamF]]))
                                                      continue;
                                                  psr.TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName;
                                                  var pmsr = new PlayerStatsRow(kvp.Value) {TeamFDisplay = psr.TeamFDisplay};
                                                  pmsrList.Add(pmsr);
                                              }
                                              else
                                              {
                                                  if (filterType != TeamFilter.League)
                                                      continue;

                                                  psr.TeamFDisplay = "- Inactive -";
                                              }
                                              psrList.Add(psr);
                                              worker1.ReportProgress(1);
                                          }
                                          PlayerStats leagueAverages = PlayerStats.CalculateLeagueAverages(_pst, _tst);
                                          lpsr.Add(new PlayerStatsRow(leagueAverages));
                                          lpmsr.Add(new PlayerStatsRow(leagueAverages));
                                      }
                                      else
                                      {
                                          partialTST = new Dictionary<int, TeamStats>();
                                          partialOppTST = new Dictionary<int, TeamStats>();
                                          // Prepare Teams
                                          foreach (var kvp in MainWindow.TeamOrder)
                                          {
                                              string curTeam = kvp.Key;
                                              int teamID = MainWindow.TeamOrder[curTeam];

                                              q = "select * from GameResults where ((T1Name LIKE \"" + curTeam + "\") OR (T2Name LIKE \"" + curTeam +
                                                  "\")) AND ((Date >= \"" + SQLiteDatabase.ConvertDateTimeToSQLite(startDate) + "\") AND (Date <= \"" +
                                                  SQLiteDatabase.ConvertDateTimeToSQLite(endDate) + "\"))" + " ORDER BY Date DESC";
                                              res = db.GetDataTable(q);

                                              var ts2 = new TeamStats(curTeam);
                                              var tsopp2 = new TeamStats(curTeam);
                                              foreach (DataRow r in res.Rows)
                                              {
                                                  TeamOverviewWindow.AddToTeamStatsFromSQLBoxScore(r, ref ts2, ref tsopp2);
                                              }
                                              partialTST[teamID] = ts2;
                                              partialOppTST[teamID] = tsopp2;
                                          }

                                          // Prepare Players
                                          q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID)";
                                          q = SQLiteDatabase.AddDateRangeToSQLQuery(q, startDate, endDate);
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

                                          playerCount = pstBetween.Count;
                                          foreach (var kvp in pstBetween)
                                          {
                                              var psr = new PlayerStatsRow(kvp.Value);
                                              if (psr.isActive)
                                              {
                                                  if (!InCurrentFilter(_tst[MainWindow.TeamOrder[psr.TeamF]]))
                                                      continue;

                                                  psr.TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName;

                                                  var pmsr = new PlayerStatsRow(kvp.Value)
                                                                 {TeamFDisplay = _tst[MainWindow.TeamOrder[psr.TeamF]].displayName};
                                                  pmsrList.Add(pmsr);
                                              }
                                              else
                                              {
                                                  if (filterType != TeamFilter.League)
                                                      continue;

                                                  psr.TeamFDisplay = "- Inactive -";
                                              }
                                              psrList.Add(psr);
                                              worker1.ReportProgress(1);
                                          }
                                          List<int> psrIDs = new List<int>();
                                          psrList.ForEach(row => psrIDs.Add(row.ID));
                                          PlayerStats leagueAverages = PlayerStats.CalculateLeagueAverages(pstBetween, partialTST);
                                          lpsr.Add(new PlayerStatsRow(leagueAverages));
                                          lpmsr.Add(new PlayerStatsRow(leagueAverages));
                                      }

                                      psrList.Sort((psr1, psr2) => psr1.PPG.CompareTo(psr2.PPG));
                                      psrList.Reverse();

                                      pmsrList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                                      pmsrList.Reverse();
                                  };

            worker1.ProgressChanged +=
                delegate
                    {
                        if (++i < playerCount)
                        {
                            txbStatus.Text = "Please wait while player averages and metric stats are being calculated (" + i + "/" + playerCount +
                                             " completed)...";
                        }
                        else
                        {
                            txbStatus.Text = "Please wait as best performers and best starting 5 are being calculated...";
                        }
                    };

            worker1.RunWorkerCompleted += delegate
                                              {

                                                  dgvPlayerStats.ItemsSource = psrList;
                                                  dgvLeaguePlayerStats.ItemsSource = lpsr;
                                                  dgvMetricStats.ItemsSource = pmsrList;
                                                  dgvLeagueMetricStats.ItemsSource = lpmsr;

                                                  txbPlayer1.Text = "";
                                                  txbPlayer2.Text = "";
                                                  txbPlayer3.Text = "";
                                                  txbPlayer4.Text = "";
                                                  txbPlayer5.Text = "";
                                                  txbPlayer6.Text = "";

                                                  List<PlayerStatsRow> templist = new List<PlayerStatsRow>();
                                                  try
                                                  {
                                                      templist = pmsrList.ToList();
                                                      templist.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
                                                      templist.Reverse();

                                                      PlayerStatsRow psr1 = templist[0];
                                                      string text = psr1.GetBestStats(5);
                                                      txbPlayer1.Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " +
                                                                        psr1.TeamFDisplay + ")\n\n" + text;

                                                      PlayerStatsRow psr2 = templist[1];
                                                      text = psr2.GetBestStats(5);
                                                      txbPlayer2.Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + " - " +
                                                                        psr2.TeamFDisplay + ")\n\n" + text;

                                                      PlayerStatsRow psr3 = templist[2];
                                                      text = psr3.GetBestStats(5);
                                                      txbPlayer3.Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + " - " +
                                                                        psr3.TeamFDisplay + ")\n\n" + text;

                                                      PlayerStatsRow psr4 = templist[3];
                                                      text = psr4.GetBestStats(5);
                                                      txbPlayer4.Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + " - " +
                                                                        psr4.TeamFDisplay + ")\n\n" + text;

                                                      PlayerStatsRow psr5 = templist[4];
                                                      text = psr5.GetBestStats(5);
                                                      txbPlayer5.Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + " - " +
                                                                        psr5.TeamFDisplay + ")\n\n" + text;

                                                      PlayerStatsRow psr6 = templist[5];
                                                      text = psr6.GetBestStats(5);
                                                      txbPlayer6.Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + " - " +
                                                                        psr6.TeamFDisplay + ")\n\n" + text;
                                                  }
                                                  catch (Exception)
                                                  {
                                                  }
                                                  CalculateStarting5(templist);

                                                  tbcLeagueOverview.Visibility = Visibility.Visible;
                                                  txbStatus.FontWeight = FontWeights.Normal;
                                                  txbStatus.Text = message;
                                                  sem.Release();
                                              };

            worker1.RunWorkerAsync();
        }

        private void CalculateStarting5(List<PlayerStatsRow> sortedPSRList)
        {
            txbStartingPG.Text = "";
            txbStartingSG.Text = "";
            txbStartingSF.Text = "";
            txbStartingPF.Text = "";
            txbStartingC.Text = "";
            txbSubs.Text = "";

            string text;
            PlayerStatsRow psr1;
            List<PlayerStatsRow> tempList = new List<PlayerStatsRow>();

            var PGList = psrList.Where(row => (row.Position1 == "PG" || row.Position2 == "PG") && row.isInjured == false).Take(10).ToList();
            PGList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            PGList.Reverse();
            var SGList = psrList.Where(row => (row.Position1 == "SG" || row.Position2 == "SG") && row.isInjured == false).Take(10).ToList();
            SGList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            SGList.Reverse();
            var SFList = psrList.Where(row => (row.Position1 == "SF" || row.Position2 == "SF") && row.isInjured == false).Take(10).ToList();
            SFList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            SFList.Reverse();
            var PFList = psrList.Where(row => (row.Position1 == "PF" || row.Position2 == "PF") && row.isInjured == false).Take(10).ToList();
            PFList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            PFList.Reverse();
            var CList = psrList.Where(row => (row.Position1 == "C" || row.Position2 == "C") && row.isInjured == false).Take(10).ToList();
            CList.Sort((pmsr1, pmsr2) => pmsr1.GmSc.CompareTo(pmsr2.GmSc));
            CList.Reverse();
            List<StartingFivePermutation> permutations = new List<StartingFivePermutation>();

            double max = Double.MinValue;
            for (int i1 = 0; i1 <PGList.Count; i1++)
                for (int i2 = 0; i2 < SGList.Count; i2++)
                    for (int i3 = 0; i3 < SFList.Count; i3++)
                        for (int i4 = 0; i4 < PFList.Count; i4++)
                            for (int i5 = 0; i5 < CList.Count; i5++)
                            {
                                double _sum = 0;
                                var _pInP = 0;
                                var perm = new List<int>();
                                perm.Add(PGList[i1].ID);
                                _sum += PGList[i1].GmSc;
                                if (PGList[i1].Position1 == "PG")
                                    _pInP++;
                                if (perm.Contains(SGList[i2].ID))
                                {
                                    continue;
                                }
                                perm.Add(SGList[i2].ID);
                                _sum += SGList[i2].GmSc;
                                if (SGList[i2].Position1 == "SG")
                                    _pInP++;
                                if (perm.Contains(SFList[i3].ID))
                                {
                                    continue;
                                }
                                perm.Add(SFList[i3].ID);
                                _sum += SFList[i3].GmSc;
                                if (SFList[i3].Position1 == "SF")
                                    _pInP++;
                                if (perm.Contains(PFList[i4].ID))
                                {
                                    continue;
                                }
                                perm.Add(PFList[i4].ID);
                                _sum += PFList[i4].GmSc;
                                if (PFList[i4].Position1 == "PF")
                                    _pInP++;
                                if (perm.Contains(CList[i5].ID))
                                {
                                    continue;
                                }
                                perm.Add(CList[i5].ID);
                                _sum += CList[i5].GmSc;
                                if (CList[i5].Position1 == "C")
                                    _pInP++;

                                if (_sum > max)
                                    max = _sum;

                                permutations.Add(new StartingFivePermutation {idList = perm, pInP = _pInP, sum = _sum});
                            }

            StartingFivePermutation bestPerm;
            try
            {
                bestPerm = permutations.Where(perm1 => perm1.sum.Equals(max)).OrderByDescending(perm2 => perm2.pInP).First();
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
                txbStartingPG.Text = "PG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" + text;
            }
            catch (Exception)
            {}

            try
            {
                psr1 = tempList[1];
                text = psr1.GetBestStats(5);
                txbStartingSG.Text = "SG: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" + text;
            }
            catch (Exception)
            {}

            try
            {
                psr1 = tempList[2];
                text = psr1.GetBestStats(5);
                txbStartingSF.Text = "SF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" + text;
            }
            catch (Exception)
            {}

            try
            {
                psr1 = tempList[3];
                text = psr1.GetBestStats(5);
                txbStartingPF.Text = "PF: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" +
                                     text;
            }
            catch (Exception)
            {}

            try
            {
                psr1 = tempList[4];
                text = psr1.GetBestStats(5);
                txbStartingC.Text = "C: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + ")\n\n" + text;
            }
            catch (Exception)
            {}

            // Subs
            txbSubs.Text = "Subs: ";
            List<int> usedIDs = new List<int>(bestPerm.idList);
            int i = 0;
            try
            {
                while (usedIDs.Contains(PGList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(PGList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(SGList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(SGList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(SFList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(SFList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(PFList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(PFList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                i = 0;
                while (usedIDs.Contains(CList[i].ID))
                {
                    i++;
                }
                usedIDs.Add(CList[i].ID);
            }
            catch (Exception)
            {
            }

            try
            {
                int count = usedIDs.Count;
                for (int j = count + 1; j <= 12; j++)
                {
                    i = 0;
                    while (usedIDs.Contains(sortedPSRList[i].ID))
                    {
                        i++;
                    }
                    usedIDs.Add(sortedPSRList[i].ID);
                }
            }
            catch (Exception)
            {
            }

            usedIDs.Skip(5).ToList().ForEach(id => tempList.Add(psrList.Single(row => row.ID == id)));
            for (i = 5; i < usedIDs.Count; i++)
            {
                psr1 = tempList[i];
                //text = psr1.GetBestStats(5);
                txbSubs.Text += psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + " - " + psr1.TeamFDisplay + "), ";
            }
            txbSubs.Text = txbSubs.Text.TrimEnd(new char[] {' ', ','});
        }

        private void PrepareBoxScores()
        {
            dt_bs.Clear();

            q = "select * from GameResults";

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                q += " where SeasonNum = " + cmbSeasonNum.SelectedValue;
            }
            else
            {
                q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault(), true);
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
            dt_lpts.Clear();

            var ls = new TeamStats("League");

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out _tst, out _tstopp, out MainWindow.TeamOrder);

                ls = TeamStats.CalculateLeagueAverages(_tst, "Playoffs");

                foreach (int key in _tst.Keys)
                {
                    if (_tst[key].isHidden)
                        continue;
                    if (_tst[key].getPlayoffGames() == 0)
                        continue;
                    if (!InCurrentFilter(_tst[key]))
                        continue;

                    DataRow r = dt_pts.NewRow();

                    TeamOverviewWindow.CreateDataRowFromTeamStats(_tst[key], ref r, GetDisplayNameFromTeam(_tst[key].name), true);

                    dt_pts.Rows.Add(r);
                }
            }
            else
            {
                partialTST = new Dictionary<int, TeamStats>();
                partialOppTST = new Dictionary<int, TeamStats>();

                int i = 0;
                foreach (var kvp in MainWindow.TeamOrder)
                {
                    if (!InCurrentFilter(kvp.Key))
                        continue;

                    q = String.Format("select * from GameResults where ((T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") AND IsPlayoff LIKE \"True\");",
                                      kvp.Key);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());

                    res = db.GetDataTable(q);

                    DataRow r = dt_pts.NewRow();

                    ts = new TeamStats(kvp.Key);
                    tsopp = new TeamStats();
                    TeamOverviewWindow.AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                    TeamOverviewWindow.CreateDataRowFromTeamStats(ts, ref r, GetDisplayNameFromTeam(kvp.Key), true);
                    partialTST[i] = ts;
                    partialOppTST[i] = tsopp;

                    dt_pts.Rows.Add(r);
                    i++;
                }

                ls = TeamStats.CalculateLeagueAverages(partialTST, "Playoffs");
            }

            DataRow r2 = dt_lpts.NewRow();

            TeamOverviewWindow.CreateDataRowFromTeamStats(ls, ref r2, "League", true);

            dt_lpts.Rows.Add(r2);

            // DataTable's ready, set DataView and fill DataGrid
            var dv_pts = new DataView(dt_pts) {AllowNew = false, AllowEdit = false, Sort = "Weff DESC"};
            var dv_lpts = new DataView(dt_lpts) {AllowNew = false, AllowEdit = false};

            dgvPlayoffStats.DataContext = dv_pts;
            dgvLeaguePlayoffStats.DataContext = dv_lpts;
        }

        private void PrepareTeamStats()
        {
            var tmsrList = new List<TeamMetricStatsRow>();
            var lssr = new List<TeamMetricStatsRow>();

            var ls = new TeamStats("League");

            dt_lts.Clear();
            dt_ts.Clear();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                SQLiteIO.GetAllTeamStatsFromDatabase(MainWindow.currentDB, curSeason, out _tst, out _tstopp, out MainWindow.TeamOrder);

                TeamStats.CalculateAllMetrics(ref _tst, _tstopp);

                foreach (int key in _tst.Keys)
                {
                    if (_tst[key].isHidden)
                        continue;

                    if (!InCurrentFilter(_tst[key]))
                        continue;

                    DataRow r = dt_ts.NewRow();

                    TeamOverviewWindow.CreateDataRowFromTeamStats(_tst[key], ref r, GetDisplayNameFromTeam(_tst[key].name));

                    dt_ts.Rows.Add(r);

                    tmsrList.Add(new TeamMetricStatsRow(_tst[key]));
                }

                ls = TeamStats.CalculateLeagueAverages(_tst, "Season");
            }
            else
            {
                var partialTST = new Dictionary<int, TeamStats>();
                var partialOppTST = new Dictionary<int, TeamStats>();

                int i = 0;
                foreach (var kvp in MainWindow.TeamOrder)
                {
                    if (!InCurrentFilter(kvp.Key))
                        continue;

                    q = String.Format("select * from GameResults where ((T1Name LIKE \"{0}\" OR T2Name LIKE \"{0}\") AND IsPlayoff LIKE \"False\");",
                                      kvp.Key);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());

                    res = db.GetDataTable(q);

                    DataRow r = dt_ts.NewRow();

                    ts = new TeamStats(kvp.Key);
                    tsopp = new TeamStats();
                    TeamOverviewWindow.AddToTeamStatsFromSQLBoxScores(res, ref ts, ref tsopp);
                    TeamOverviewWindow.CreateDataRowFromTeamStats(ts, ref r, GetDisplayNameFromTeam(kvp.Key));
                    ts.CalcMetrics(tsopp);
                    partialTST[i] = ts;
                    partialOppTST[i] = tsopp;

                    tmsrList.Add(new TeamMetricStatsRow(ts));

                    dt_ts.Rows.Add(r);

                    i++;
                }
                TeamStats.CalculateAllMetrics(ref partialTST, partialOppTST);

                ls = TeamStats.CalculateLeagueAverages(partialTST, "Season");
            }

            DataRow r2 = dt_lts.NewRow();

            TeamOverviewWindow.CreateDataRowFromTeamStats(ls, ref r2, "League");

            dt_lts.Rows.Add(r2);

            lssr.Add(new TeamMetricStatsRow(ls));

            // DataTable's ready, set DataView and fill DataGrid
            var dv_ts = new DataView(dt_ts) {AllowNew = false, AllowEdit = false, Sort = "Weff DESC"};
            var dv_lts = new DataView(dt_lts) {AllowNew = false, AllowEdit = false};

            dgvTeamStats.DataContext = dv_ts;
            dgvLeagueTeamStats.DataContext = dv_lts;

            tmsrList.Sort((tmsr1, tmsr2) => tmsr1.EFFd.CompareTo(tmsr2.EFFd));
            tmsrList.Reverse();
            dgvTeamMetricStats.ItemsSource = tmsrList;
            dgvLeagueTeamMetricStats.ItemsSource = lssr;
        }

        private bool InCurrentFilter(TeamStats ts)
        {
            if (filterType == TeamFilter.League)
                return true;

            if (filterType == TeamFilter.Conference)
            {
                int confID = -1;
                foreach (var conf in MainWindow.Conferences)
                {
                    if (conf.Name == filterDescription)
                    {
                        confID = conf.ID;
                        break;
                    }
                }
                var div = MainWindow.Divisions.Find(division => division.ID == ts.division);
                if (div.ConferenceID == confID)
                    return true;
                else
                    return false;
            }
            else
            {
                var div = MainWindow.Divisions.Find(division => division.ID == ts.division);
                if (div.Name == filterDescription)
                    return true;
                else
                    return false;
            }
        }

        private bool InCurrentFilter(string teamName)
        {
            if (filterType == TeamFilter.League)
                return true;

            var res = db.GetDataTable("SELECT Division FROM Teams WHERE Name LIKE \"" + teamName + "\"");
            var divID = Tools.getInt(res.Rows[0], "Division");

            if (filterType == TeamFilter.Conference)
            {
                int confID = -1;
                foreach (var conf in MainWindow.Conferences)
                {
                    if (conf.Name == filterDescription)
                    {
                        confID = conf.ID;
                        break;
                    }
                }
                var div = MainWindow.Divisions.Find(division => division.ID == divID);
                if (div.ConferenceID == confID)
                    return true;
                else
                    return false;
            }
            else
            {
                var div = MainWindow.Divisions.Find(division => division.ID == divID);
                if (div.Name == filterDescription)
                    return true;
                else
                    return false;
            }
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                reload = true;
                lastShownTeamSeason = 0;
                lastShownPlayerSeason = 0;
                lastShownPlayoffSeason = 0;
                lastShownLeadersSeason = 0;
                lastShownBoxSeason = 0;
                tbcLeagueOverview_SelectionChanged(null, null);
            }
        }

        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSeasonNum.SelectedIndex == -1)
                return;

            curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;

            SQLiteIO.LoadSeason(MainWindow.currentDB, out _tst, out _tstopp, out _pst, out MainWindow.TeamOrder, ref MainWindow.pt,
                                ref MainWindow.bshist, _curSeason: curSeason);
            MainWindow.CopySeasonToMainWindow(_tst, _tstopp, _pst);

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

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameid);
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

                var tow = new TeamOverviewWindow(team);
                tow.ShowDialog();
            }
        }

        private void dgvPlayoffStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayoffStats.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvPlayoffStats.SelectedItems[0];
                string team = row["Name"].ToString();

                var tow = new TeamOverviewWindow(team);
                tow.ShowDialog();
            }
        }

        private PlayerStatsRow ConvertToLeagueLeader(PlayerStatsRow psr, Dictionary<int, TeamStats> teamStats)
        {
            string team = psr.TeamF;
            ts = teamStats[MainWindow.TeamOrder[team]];
            uint gamesTeam = ts.getGames();
            uint gamesPlayer = psr.GP;
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

            if (psr.FGM < fgmRequired)
                newpsr.FGp = float.NaN;
            if (psr.TPM < tpmRequired)
                newpsr.TPp = float.NaN;
            if (psr.FTM < ftmRequired)
                newpsr.FTp = float.NaN;

            if (gamesPlayer >= gamesRequired)
            {
                return newpsr;
            }

            if (psr.PTS < ptsRequired)
                newpsr.PPG = float.NaN;
            if (psr.REB < rebRequired)
                newpsr.RPG = float.NaN;
            if (psr.AST < astRequired)
                newpsr.APG = float.NaN;
            if (psr.STL < stlRequired)
                newpsr.SPG = float.NaN;
            if (psr.BLK < blkRequired)
                newpsr.BPG = float.NaN;
            if (psr.MINS < minRequired)
                newpsr.MPG = float.NaN;
            return newpsr;
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayerStats.SelectedCells.Count > 0)
            {
                var psr = (PlayerStatsRow) dgvPlayerStats.SelectedItems[0];

                var pow = new PlayerOverviewWindow(psr.TeamF, psr.ID);
                pow.ShowDialog();
            }
        }

        private void dgvLeaders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvLeaders.SelectedCells.Count > 0)
            {
                var psr = (PlayerStatsRow) dgvLeaders.SelectedItems[0];

                var pow = new PlayerOverviewWindow(psr.TeamF, psr.ID);
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

        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }

        private void dgvTeamMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamMetricStats.Columns.Count && i < dgvLeagueTeamMetricStats.Columns.Count; ++i)
                dgvLeagueTeamMetricStats.Columns[i].Width = dgvTeamMetricStats.Columns[i].ActualWidth;
        }

        private void dgvTeamStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvTeamStats.Columns.Count && i < dgvLeagueTeamStats.Columns.Count; ++i)
                dgvLeagueTeamStats.Columns[i].Width = dgvTeamStats.Columns[i].ActualWidth;
        }

        private void dgvPlayoffStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvPlayoffStats.Columns.Count && i < dgvLeaguePlayoffStats.Columns.Count; ++i)
                dgvLeaguePlayoffStats.Columns[i].Width = dgvPlayoffStats.Columns[i].ActualWidth;
        }

        private void dgvPlayerStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvPlayerStats.Columns.Count && i < dgvLeaguePlayerStats.Columns.Count; ++i)
                dgvLeaguePlayerStats.Columns[i].Width = dgvPlayerStats.Columns[i].ActualWidth;
        }

        private void dgvMetricStats_LayoutUpdated(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvMetricStats.Columns.Count && i < dgvLeagueMetricStats.Columns.Count; ++i)
                dgvLeagueMetricStats.Columns[i].Width = dgvMetricStats.Columns[i].ActualWidth;
        }

        private void dgLeague_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = "L";
        }

        private void cmbDivConf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changingTimeframe)
                return;

            if (cmbDivConf.SelectedIndex == -1)
                return;

            ComboBoxItemWithEnabled cur = (ComboBoxItemWithEnabled) cmbDivConf.SelectedItem;
            var name = cur.Item;
            var parts = name.Split(new string[] {": "}, 2, StringSplitOptions.None);
            if (parts.Length == 1)
            {
                if (parts[0] == "Whole League")
                {
                    filterType = TeamFilter.League;
                    filterDescription = "";
                }
                else
                {
                    filterType = TeamFilter.Conference;
                    filterDescription = parts[0];
                }
            }
            else
            {
                filterType = TeamFilter.Division;
                filterDescription = parts[1];
            }

            reload = true;
			lastShownTeamSeason = 0;
			lastShownPlayerSeason = 0;
			lastShownPlayoffSeason = 0;
			lastShownLeadersSeason = 0;
			lastShownBoxSeason = 0;
            tbcLeagueOverview_SelectionChanged(null, null);
        }
    }
}