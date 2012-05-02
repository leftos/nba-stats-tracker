using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for teamOverviewW.xaml
    /// </summary>
    public partial class teamOverviewW : Window
    {
        public const int M = 0,
                         PF = 1,
                         PA = 2,
                         FGM = 4,
                         FGA = 5,
                         TPM = 6,
                         TPA = 7,
                         FTM = 8,
                         FTA = 9,
                         OREB = 10,
                         DREB = 11,
                         STL = 12,
                         TO = 13,
                         BLK = 14,
                         AST = 15,
                         FOUL = 16;

        public const int PPG = 0,
                         PAPG = 1,
                         FGp = 2,
                         FGeff = 3,
                         TPp = 4,
                         TPeff = 5,
                         FTp = 6,
                         FTeff = 7,
                         RPG = 8,
                         ORPG = 9,
                         DRPG = 10,
                         SPG = 11,
                         BPG = 12,
                         TPG = 13,
                         APG = 14,
                         FPG = 15,
                         Wp = 16,
                         Weff = 17,
                         PD = 18;

        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly DataTable dt_bs;
        private readonly DataTable dt_hth;
        private readonly DataTable dt_ov;
        private readonly DataTable dt_ss;
        private readonly DataTable dt_yea;
        private readonly int[][] pl_rankings;
        private readonly int[][] rankings;
        private string curTeam;
        private DataTable dt_bs_res;
        private DataView dv_hth;
        private ObservableCollection<PlayerStatsRow> psr;
        private Dictionary<int, PlayerStats> pst;
        private string showSeason;
        private TeamStats[] tst;

        public teamOverviewW(TeamStats[] tst, Dictionary<int, PlayerStats> pst)
        {
            InitializeComponent();

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
            dt_bs.Columns.Add("Date");
            dt_bs.Columns.Add("Opponent");
            dt_bs.Columns.Add("Home-Away");
            dt_bs.Columns.Add("Result");
            dt_bs.Columns.Add("Score");
            dt_bs.Columns.Add("GameID");

            #endregion

            this.tst = tst;
            this.pst = pst;

            foreach (var kvp in MainWindow.TeamOrder)
            {
                cmbTeam.Items.Add(kvp.Key);
                cmbOppTeam.Items.Add(kvp.Key);
            }

            rankings = StatsTracker.calculateRankings(tst);
            pl_rankings = StatsTracker.calculateRankings(tst, playoffs: true);

            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1).AddDays(1);

            PopulateSeasonCombo();

            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = 0;
        }

        public teamOverviewW(TeamStats[] tst, Dictionary<int, PlayerStats> pst, string team) : this(tst, pst)
        {
            cmbTeam.SelectedItem = team;
        }

        private void PopulateSeasonCombo()
        {
            for (int i = MainWindow.getMaxSeason(MainWindow.currentDB); i > 0; i--)
            {
                cmbSeasonNum.Items.Add(i.ToString());
            }

            cmbSeasonNum.SelectedItem = MainWindow.curSeason.ToString();
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
            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats("Opponents");

            int i = MainWindow.TeamOrder[curTeam];

            #region Prepare Team Overview & Box Scores

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                tst = MainWindow.LoadDatabase(MainWindow.currentDB, ref pst, ref MainWindow.TeamOrder,
                                                ref MainWindow.pt, ref MainWindow.bshist,
                                                _curSeason: Convert.ToInt32(showSeason));

                ts = tst[i];

                DataTable res;
                String q = "select * from GameResults where ((T1Name LIKE '" + curTeam + "') OR (T2Name LIKE '"
                           + curTeam + "')) AND SeasonNum = " + showSeason + " ORDER BY Date DESC";
                res = db.GetDataTable(q);
                dt_bs_res = res;

                foreach (DataRow r in res.Rows)
                {
                    int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                    int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                    if (r["T1Name"].ToString().Equals(curTeam))
                    {
                        DataRow bsr = dt_bs.NewRow();
                        bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                        bsr["Opponent"] = r["T2Name"].ToString();
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

                        dt_bs.Rows.Add(bsr);
                    }
                    else
                    {
                        DataRow bsr = dt_bs.NewRow();
                        bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                        bsr["Opponent"] = r["T1Name"].ToString();
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

                        dt_bs.Rows.Add(bsr);
                    }
                }
            }
            else
            {
                if ((dtpStart.SelectedDate.HasValue) && (dtpEnd.SelectedDate.HasValue))
                {
                    DataTable res;
                    String q = "select * from GameResults where ((T1Name LIKE '" + curTeam + "') OR (T2Name LIKE '"
                               + curTeam + "')) AND ((Date >= '" +
                               SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault())
                               + "') AND (Date <= '" +
                               SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()) + "'))" + " ORDER BY Date DESC";
                    res = db.GetDataTable(q);
                    dt_bs_res = res;

                    foreach (DataRow r in res.Rows)
                    {
                        int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                        int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                        if (r["T1Name"].ToString().Equals(curTeam))
                        {
                            DataRow bsr = dt_bs.NewRow();
                            bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                            bsr["Opponent"] = r["T2Name"].ToString();
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

                            dt_bs.Rows.Add(bsr);
                        }
                        else
                        {
                            DataRow bsr = dt_bs.NewRow();
                            bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                            bsr["Opponent"] = r["T1Name"].ToString();
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

                            dt_bs.Rows.Add(bsr);
                        }
                        tsopp.winloss[1] = ts.winloss[0];
                        tsopp.winloss[0] = ts.winloss[1];
                    }
                    AddToTeamStatsFromSQLBoxScore(res, ref ts, ref tsopp);
                }
            }

            #region Regular Season

            DataRow dr = dt_ov.NewRow();

            dr["Type"] = "Stats";
            dr["Games"] = ts.getGames();
            dr["Wins (W%)"] = ts.winloss[0].ToString();
            dr["Losses (Weff)"] = ts.winloss[1].ToString();
            dr["PF"] = ts.stats[PF].ToString();
            dr["PA"] = ts.stats[PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = ts.stats[FGM].ToString() + "-" + ts.stats[FGA].ToString();
            dr["3PT"] = ts.stats[TPM].ToString() + "-" + ts.stats[TPA].ToString();
            dr["FT"] = ts.stats[FTM].ToString() + "-" + ts.stats[FTA].ToString();
            dr["REB"] = (ts.stats[DREB] + ts.stats[OREB]).ToString();
            dr["OREB"] = ts.stats[OREB].ToString();
            dr["DREB"] = ts.stats[DREB].ToString();
            dr["AST"] = ts.stats[AST].ToString();
            dr["TO"] = ts.stats[TO].ToString();
            dr["STL"] = ts.stats[STL].ToString();
            dr["BLK"] = ts.stats[BLK].ToString();
            dr["FOUL"] = ts.stats[FOUL].ToString();

            dt_ov.Rows.Add(dr);

            dr = dt_ov.NewRow();

            ts.calcAvg(); // Just to be sure...

            dr["Type"] = "Averages";
            //dr["Games"] = ts.getGames();
            dr["Wins (W%)"] = String.Format("{0:F3}", ts.averages[Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", ts.averages[Weff]);
            dr["PF"] = String.Format("{0:F1}", ts.averages[PPG]);
            dr["PA"] = String.Format("{0:F1}", ts.averages[PAPG]);
            dr["PD"] = String.Format("{0:F1}", ts.averages[PD]);
            dr["FG"] = String.Format("{0:F3}", ts.averages[FGp]);
            dr["FGeff"] = String.Format("{0:F2}", ts.averages[FGeff]);
            dr["3PT"] = String.Format("{0:F3}", ts.averages[TPp]);
            dr["3Peff"] = String.Format("{0:F2}", ts.averages[TPeff]);
            dr["FT"] = String.Format("{0:F3}", ts.averages[FTp]);
            dr["FTeff"] = String.Format("{0:F2}", ts.averages[FTeff]);
            dr["REB"] = String.Format("{0:F1}", ts.averages[RPG]);
            dr["OREB"] = String.Format("{0:F1}", ts.averages[ORPG]);
            dr["DREB"] = String.Format("{0:F1}", ts.averages[DRPG]);
            dr["AST"] = String.Format("{0:F1}", ts.averages[APG]);
            dr["TO"] = String.Format("{0:F1}", ts.averages[TPG]);
            dr["STL"] = String.Format("{0:F1}", ts.averages[SPG]);
            dr["BLK"] = String.Format("{0:F1}", ts.averages[BPG]);
            dr["FOUL"] = String.Format("{0:F1}", ts.averages[FPG]);

            dt_ov.Rows.Add(dr);

            // Rankings can only be shown based on total stats
            // ...for now
            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                DataRow dr2 = dt_ov.NewRow();

                dr2["Type"] = "Rankings";
                dr2["Wins (W%)"] = rankings[i][Wp];
                dr2["Losses (Weff)"] = rankings[i][Weff];
                dr2["PF"] = rankings[i][PPG];
                dr2["PA"] = cmbTeam.Items.Count + 1 - rankings[i][PAPG];
                dr2["PD"] = rankings[i][PD];
                dr2["FG"] = rankings[i][FGp];
                dr2["FGeff"] = rankings[i][FGeff];
                dr2["3PT"] = rankings[i][TPp];
                dr2["3Peff"] = rankings[i][TPeff];
                dr2["FT"] = rankings[i][FTp];
                dr2["FTeff"] = rankings[i][FTeff];
                dr2["REB"] = rankings[i][RPG];
                dr2["OREB"] = rankings[i][ORPG];
                dr2["DREB"] = rankings[i][DRPG];
                dr2["AST"] = rankings[i][AST];
                dr2["TO"] = cmbTeam.Items.Count + 1 - rankings[i][TO];
                dr2["STL"] = rankings[i][STL];
                dr2["BLK"] = rankings[i][BLK];
                dr2["FOUL"] = cmbTeam.Items.Count + 1 - rankings[i][FOUL];

                dt_ov.Rows.Add(dr2);
            }
            else
            {
                DataRow dr2 = dt_ov.NewRow();

                dr2["Type"] = "Opp Stats";
                dr2["Games"] = tsopp.getGames();
                dr2["Wins (W%)"] = tsopp.winloss[0].ToString();
                dr2["Losses (Weff)"] = tsopp.winloss[1].ToString();
                dr2["PF"] = tsopp.stats[PF].ToString();
                dr2["PA"] = tsopp.stats[PA].ToString();
                dr2["PD"] = " ";
                dr2["FG"] = tsopp.stats[FGM].ToString() + "-" + tsopp.stats[FGA].ToString();
                dr2["3PT"] = tsopp.stats[TPM].ToString() + "-" + tsopp.stats[TPA].ToString();
                dr2["FT"] = tsopp.stats[FTM].ToString() + "-" + tsopp.stats[FTA].ToString();
                dr2["REB"] = (tsopp.stats[DREB] + tsopp.stats[OREB]).ToString();
                dr2["OREB"] = tsopp.stats[OREB].ToString();
                dr2["DREB"] = tsopp.stats[DREB].ToString();
                dr2["AST"] = tsopp.stats[AST].ToString();
                dr2["TO"] = tsopp.stats[TO].ToString();
                dr2["STL"] = tsopp.stats[STL].ToString();
                dr2["BLK"] = tsopp.stats[BLK].ToString();
                dr2["FOUL"] = tsopp.stats[FOUL].ToString();

                dt_ov.Rows.Add(dr2);

                dr2 = dt_ov.NewRow();

                tsopp.calcAvg(); // Just to be sure...

                dr2["Type"] = "Opp Avg";
                dr2["Wins (W%)"] = String.Format("{0:F3}", tsopp.averages[Wp]);
                dr2["Losses (Weff)"] = String.Format("{0:F2}", tsopp.averages[Weff]);
                dr2["PF"] = String.Format("{0:F1}", tsopp.averages[PPG]);
                dr2["PA"] = String.Format("{0:F1}", tsopp.averages[PAPG]);
                dr2["PD"] = String.Format("{0:F1}", tsopp.averages[PD]);
                dr2["FG"] = String.Format("{0:F3}", tsopp.averages[FGp]);
                dr2["FGeff"] = String.Format("{0:F2}", tsopp.averages[FGeff]);
                dr2["3PT"] = String.Format("{0:F3}", tsopp.averages[TPp]);
                dr2["3Peff"] = String.Format("{0:F2}", tsopp.averages[TPeff]);
                dr2["FT"] = String.Format("{0:F3}", tsopp.averages[FTp]);
                dr2["FTeff"] = String.Format("{0:F2}", tsopp.averages[FTeff]);
                dr2["REB"] = String.Format("{0:F1}", tsopp.averages[RPG]);
                dr2["OREB"] = String.Format("{0:F1}", tsopp.averages[ORPG]);
                dr2["DREB"] = String.Format("{0:F1}", tsopp.averages[DRPG]);
                dr2["AST"] = String.Format("{0:F1}", tsopp.averages[AST]);
                dr2["TO"] = String.Format("{0:F1}", tsopp.averages[TO]);
                dr2["STL"] = String.Format("{0:F1}", tsopp.averages[STL]);
                dr2["BLK"] = String.Format("{0:F1}", tsopp.averages[BLK]);
                dr2["FOUL"] = String.Format("{0:F1}", tsopp.averages[FOUL]);

                dt_ov.Rows.Add(dr2);

                tsopp.calcAvg();
            }

            #endregion

            #region Playoffs

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                dr = dt_ov.NewRow();

                dr["Type"] = "Playoffs";
                dr["Games"] = ts.getPlayoffGames();
                dr["Wins (W%)"] = ts.pl_winloss[0].ToString();
                dr["Losses (Weff)"] = ts.pl_winloss[1].ToString();
                dr["PF"] = ts.pl_stats[PF].ToString();
                dr["PA"] = ts.pl_stats[PA].ToString();
                dr["PD"] = " ";
                dr["FG"] = ts.pl_stats[FGM].ToString() + "-" + ts.pl_stats[FGA].ToString();
                dr["3PT"] = ts.pl_stats[TPM].ToString() + "-" + ts.pl_stats[TPA].ToString();
                dr["FT"] = ts.pl_stats[FTM].ToString() + "-" + ts.pl_stats[FTA].ToString();
                dr["REB"] = (ts.pl_stats[DREB] + ts.pl_stats[OREB]).ToString();
                dr["OREB"] = ts.pl_stats[OREB].ToString();
                dr["DREB"] = ts.pl_stats[DREB].ToString();
                dr["AST"] = ts.pl_stats[AST].ToString();
                dr["TO"] = ts.pl_stats[TO].ToString();
                dr["STL"] = ts.pl_stats[STL].ToString();
                dr["BLK"] = ts.pl_stats[BLK].ToString();
                dr["FOUL"] = ts.pl_stats[FOUL].ToString();

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Pl Avg";
                dr["Wins (W%)"] = String.Format("{0:F3}", ts.pl_averages[Wp]);
                dr["Losses (Weff)"] = String.Format("{0:F2}", ts.pl_averages[Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.pl_averages[PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.pl_averages[PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.pl_averages[PD]);
                dr["FG"] = String.Format("{0:F3}", ts.pl_averages[FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.pl_averages[FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.pl_averages[TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.pl_averages[TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.pl_averages[FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.pl_averages[FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.pl_averages[RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.pl_averages[ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.pl_averages[DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.pl_averages[APG]);
                dr["TO"] = String.Format("{0:F1}", ts.pl_averages[TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.pl_averages[SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.pl_averages[BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.pl_averages[FPG]);

                dt_ov.Rows.Add(dr);

                // Rankings can only be shown based on total pl_stats
                // ...for now
                if (rbStatsAllTime.IsChecked.GetValueOrDefault())
                {
                    DataRow dr2 = dt_ov.NewRow();

                    int count = 0;
                    foreach (TeamStats z in tst)
                    {
                        if (z.getPlayoffGames() > 0) count++;
                    }

                    dr2["Type"] = "Pl Rank";
                    dr2["Wins (W%)"] = pl_rankings[i][Wp];
                    dr2["Losses (Weff)"] = pl_rankings[i][Weff];
                    dr2["PF"] = pl_rankings[i][PPG];
                    dr2["PA"] = count + 1 - pl_rankings[i][PAPG];
                    dr2["PD"] = pl_rankings[i][PD];
                    dr2["FG"] = pl_rankings[i][FGp];
                    dr2["FGeff"] = pl_rankings[i][FGeff];
                    dr2["3PT"] = pl_rankings[i][TPp];
                    dr2["3Peff"] = pl_rankings[i][TPeff];
                    dr2["FT"] = pl_rankings[i][FTp];
                    dr2["FTeff"] = pl_rankings[i][FTeff];
                    dr2["REB"] = pl_rankings[i][RPG];
                    dr2["OREB"] = pl_rankings[i][ORPG];
                    dr2["DREB"] = pl_rankings[i][DRPG];
                    dr2["AST"] = pl_rankings[i][AST];
                    dr2["TO"] = count + 1 - pl_rankings[i][TO];
                    dr2["STL"] = pl_rankings[i][STL];
                    dr2["BLK"] = pl_rankings[i][BLK];
                    dr2["FOUL"] = count + 1 - pl_rankings[i][FOUL];

                    dt_ov.Rows.Add(dr2);
                }
                else
                {
                    DataRow dr2 = dt_ov.NewRow();

                    dr2["Type"] = "Opp Pl Stats";
                    dr2["Games"] = tsopp.getPlayoffGames();
                    dr2["Wins (W%)"] = tsopp.pl_winloss[0].ToString();
                    dr2["Losses (Weff)"] = tsopp.pl_winloss[1].ToString();
                    dr2["PF"] = tsopp.pl_stats[PF].ToString();
                    dr2["PA"] = tsopp.pl_stats[PA].ToString();
                    dr2["PD"] = " ";
                    dr2["FG"] = tsopp.pl_stats[FGM].ToString() + "-" + tsopp.pl_stats[FGA].ToString();
                    dr2["3PT"] = tsopp.pl_stats[TPM].ToString() + "-" + tsopp.pl_stats[TPA].ToString();
                    dr2["FT"] = tsopp.pl_stats[FTM].ToString() + "-" + tsopp.pl_stats[FTA].ToString();
                    dr2["REB"] = (tsopp.pl_stats[DREB] + tsopp.pl_stats[OREB]).ToString();
                    dr2["OREB"] = tsopp.pl_stats[OREB].ToString();
                    dr2["DREB"] = tsopp.pl_stats[DREB].ToString();
                    dr2["AST"] = tsopp.pl_stats[AST].ToString();
                    dr2["TO"] = tsopp.pl_stats[TO].ToString();
                    dr2["STL"] = tsopp.pl_stats[STL].ToString();
                    dr2["BLK"] = tsopp.pl_stats[BLK].ToString();
                    dr2["FOUL"] = tsopp.pl_stats[FOUL].ToString();

                    dt_ov.Rows.Add(dr2);

                    dr2 = dt_ov.NewRow();

                    tsopp.calcAvg(); // Just to be sure...

                    dr2["Type"] = "Opp Pl Avg";
                    dr2["Wins (W%)"] = String.Format("{0:F3}", tsopp.pl_averages[Wp]);
                    dr2["Losses (Weff)"] = String.Format("{0:F2}", tsopp.pl_averages[Weff]);
                    dr2["PF"] = String.Format("{0:F1}", tsopp.pl_averages[PPG]);
                    dr2["PA"] = String.Format("{0:F1}", tsopp.pl_averages[PAPG]);
                    dr2["PD"] = String.Format("{0:F1}", tsopp.pl_averages[PD]);
                    dr2["FG"] = String.Format("{0:F3}", tsopp.pl_averages[FGp]);
                    dr2["FGeff"] = String.Format("{0:F2}", tsopp.pl_averages[FGeff]);
                    dr2["3PT"] = String.Format("{0:F3}", tsopp.pl_averages[TPp]);
                    dr2["3Peff"] = String.Format("{0:F2}", tsopp.pl_averages[TPeff]);
                    dr2["FT"] = String.Format("{0:F3}", tsopp.pl_averages[FTp]);
                    dr2["FTeff"] = String.Format("{0:F2}", tsopp.pl_averages[FTeff]);
                    dr2["REB"] = String.Format("{0:F1}", tsopp.pl_averages[RPG]);
                    dr2["OREB"] = String.Format("{0:F1}", tsopp.pl_averages[ORPG]);
                    dr2["DREB"] = String.Format("{0:F1}", tsopp.pl_averages[DRPG]);
                    dr2["AST"] = String.Format("{0:F1}", tsopp.pl_averages[AST]);
                    dr2["TO"] = String.Format("{0:F1}", tsopp.pl_averages[TO]);
                    dr2["STL"] = String.Format("{0:F1}", tsopp.pl_averages[STL]);
                    dr2["BLK"] = String.Format("{0:F1}", tsopp.pl_averages[BLK]);
                    dr2["FOUL"] = String.Format("{0:F1}", tsopp.pl_averages[FOUL]);

                    dt_ov.Rows.Add(dr2);

                    tsopp.calcAvg();
                }
            }

            #endregion

            var dv_ov = new DataView(dt_ov);
            dv_ov.AllowNew = false;

            dgvTeamStats.DataContext = dv_ov;

            DataView dv_bs;

            if (rbBSSimple.IsChecked.GetValueOrDefault())
            {
                dv_bs = new DataView(dt_bs);
            }
            else
            {
                dv_bs = new DataView(dt_bs_res);
            }
            dv_bs.AllowEdit = false;
            dv_bs.AllowNew = false;

            dgvBoxScores.DataContext = dv_bs;

            #endregion
        }

        private void UpdateSplitStats()
        {
            #region Prepare Split Stats

            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats("Opponents");

            DataRow dr;

            // Prepare Queries
            string qr_home = String.Format("select * from GameResults where (T2Name LIKE '{0}')", curTeam);
            string qr_away = String.Format("select * from GameResults where (T1Name LIKE '{0}')", curTeam);
            string qr_wins = String.Format("select * from GameResults where "
                                           + "((T1Name LIKE '{0}' AND T1PTS > T2PTS) "
                                           + "OR (T2Name LIKE '{0}' AND T2PTS > T1PTS))",
                                           curTeam);
            string qr_losses = String.Format("select * from GameResults where "
                                             + "((T1Name LIKE '{0}' AND T1PTS < T2PTS) "
                                             + "OR (T2Name LIKE '{0}' AND T2PTS < T1PTS))",
                                             curTeam);
            string qr_season = String.Format("select * from GameResults where "
                                             + "(T1Name LIKE '{0}' OR T2Name LIKE '{0}') "
                                             + "AND IsPlayoff LIKE 'False'",
                                             curTeam);
            string qr_playoffs = String.Format("select * from GameResults where "
                                               + "(T1Name LIKE '{0}' OR T2Name LIKE '{0}') "
                                               + "AND IsPlayoff LIKE 'True'",
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
                string s = " AND SeasonNum = " + cmbSeasonNum.SelectedItem.ToString();
                qr_home += s;
                qr_away += s;
                qr_wins += s;
                qr_losses += s;
                qr_season += s;
                qr_playoffs += s;
            }

            DataTable res2;

            /*
            dr = dt_ss.NewRow();
            dr["Type"] = " ";
            dt_ss.Rows.Add(dr);
            */

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_home);
            AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Home");
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_away);
            AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
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
            AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Wins");
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_losses);
            AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
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
            AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
            dr = dt_ss.NewRow();
            CreateDataRowFromTeamStats(ts, ref dr, "Season");
            dt_ss.Rows.Add(dr);

            // Clear Team Stats
            ts = new TeamStats(curTeam);
            tsopp = new TeamStats("Opponents");

            res2 = db.GetDataTable(qr_playoffs);
            AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
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
                    if (dCur.AddMonths(1) > dEnd)
                    {
                        string s = String.Format("select * from GameResults where "
                                                 + "(T1Name LIKE '{0}' OR T2Name LIKE '{0}') "
                                                 + "AND (Date >= '{1}' AND Date <='{2}');",
                                                 curTeam,
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dCur),
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dEnd));

                        qrm.Add(s);
                        break;
                    }
                    else
                    {
                        string s = String.Format("select * from GameResults where "
                                                 + "(T1Name LIKE '{0}' OR T2Name LIKE '{0}') "
                                                 + "AND (Date >= '{1}' AND Date <='{2}');",
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
                    AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
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
            var dv_ss = new DataView(dt_ss);
            dv_ss.AllowEdit = false;
            dv_ss.AllowNew = false;

            dgvSplit.DataContext = dv_ss;

            #endregion
        }

        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTeam.SelectedIndex == -1) return;
                if (cmbSeasonNum.SelectedIndex == -1) return;
            }
            catch
            {
                return;
            }

            var dt_bs_res = new DataTable();

            //DataRow dr;

            dgvBoxScores.DataContext = null;
            dgvTeamStats.DataContext = null;
            dgvHTHStats.DataContext = null;
            dgvHTHBoxScores.DataContext = null;
            dgvSplit.DataContext = null;

            dt_bs.Clear();
            dt_ov.Clear();
            dt_hth.Clear();
            dt_ss.Clear();
            dt_yea.Clear();

            int i = MainWindow.TeamOrder[cmbTeam.SelectedItem.ToString()];
            showSeason = cmbSeasonNum.SelectedItem.ToString();

            curTeam = cmbTeam.SelectedItem.ToString();
            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats("Opponents");

            UpdateOverviewAndBoxScores();

            UpdateSplitStats();

            ts = tst[MainWindow.TeamOrder[curTeam]];
            Title = cmbTeam.SelectedItem + " Team Overview - " + ts.getGames() + " games played";

            UpdateHeadToHead();

            UpdateYearlyStats();

            UpdatePlayerStats();
        }

        private void UpdatePlayerStats()
        {
            int curSeason = MainWindow.curSeason;
            int maxSeason = MainWindow.getMaxSeason(MainWindow.currentDB);

            string playersT = "Players";
            if (curSeason != maxSeason) playersT += "S" + curSeason;

            string q;
            DataTable res;
            q = "select * from " + playersT + " where TeamFin LIKE '" + curTeam + "'";
            res = db.GetDataTable(q);

            psr = new ObservableCollection<PlayerStatsRow>();

            foreach (DataRow r in res.Rows)
            {
                psr.Add(new PlayerStatsRow(new PlayerStats(r)));
            }

            dgvPlayerStats.ItemsSource = psr;
        }

        private void UpdateHeadToHead()
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        private void UpdateYearlyStats()
        {
            #region Prepare Yearly Stats

            string currentDB = MainWindow.currentDB;
            int curSeason = MainWindow.curSeason;
            int maxSeason = MainWindow.getMaxSeason(currentDB);

            TeamStats ts = tst[MainWindow.TeamOrder[curTeam]];

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
                    ts = MainWindow.GetTeamStatsFromDatabase(MainWindow.currentDB, curTeam, j);
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
                }
                else
                {
                    dt_yea.Rows.Add(drcur);
                    if (playedInPlayoffs) dt_yea.Rows.Add(drcur_pl);
                }
            }

            var dv_yea = new DataView(dt_yea);
            dv_yea.AllowNew = false;
            dv_yea.AllowEdit = false;

            dgvYearly.DataContext = dv_yea;

            #endregion
        }

        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            string msg = StatsTracker.averagesAndRankings(cmbTeam.SelectedItem.ToString(), tst, MainWindow.TeamOrder);
            if (msg != "")
            {
                var cw = new copyableW(msg, cmbTeam.SelectedItem.ToString(), TextAlignment.Center);
                cw.ShowDialog();
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

            int id = MainWindow.TeamOrder[cmbTeam.SelectedItem.ToString()];
            tst[id].winloss[0] = Convert.ToByte(myCell(0, 2));
            tst[id].winloss[1] = Convert.ToByte(myCell(0, 3));
            tst[id].stats[PF] = Convert.ToUInt16(myCell(0, 4));
            tst[id].stats[PA] = Convert.ToUInt16(myCell(0, 5));

            string[] parts = myCell(0, 7).Split('-');
            tst[id].stats[FGM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 9).Split('-');
            tst[id].stats[TPM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 11).Split('-');
            tst[id].stats[FTM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[FTA] = Convert.ToUInt16(parts[1]);

            tst[id].stats[OREB] = Convert.ToUInt16(myCell(0, 14));
            tst[id].stats[DREB] = Convert.ToUInt16(myCell(0, 15));

            tst[id].stats[AST] = Convert.ToUInt16(myCell(0, 16));
            tst[id].stats[TO] = Convert.ToUInt16(myCell(0, 17));
            tst[id].stats[STL] = Convert.ToUInt16(myCell(0, 18));
            tst[id].stats[BLK] = Convert.ToUInt16(myCell(0, 19));
            tst[id].stats[FOUL] = Convert.ToUInt16(myCell(0, 20));

            tst[id].calcAvg();

            var playersToUpdate = new Dictionary<int, PlayerStats>();

            foreach (var cur in psr)
            {
                PlayerStats ps = new PlayerStats(cur);
                playersToUpdate.Add(ps.ID, ps);
            }

            MainWindow.saveSeasonToDatabase(MainWindow.currentDB, tst, playersToUpdate,
                                            Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString()),
                                            MainWindow.getMaxSeason(MainWindow.currentDB));

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
            return (dataGrid.Items[row] as DataRowView).Row.ItemArray[col].ToString();
        }

        private void btnScoutingReport_Click(object sender, RoutedEventArgs e)
        {
            int id = -1;
            try
            {
                id = MainWindow.TeamOrder[cmbTeam.SelectedItem.ToString()];
            }
            catch
            {
                return;
            }

            int[][] rating = StatsTracker.calculateRankings(tst);
            if (rating.Length != 1)
            {
                string msg = StatsTracker.scoutReport(rating, id, cmbTeam.SelectedItem.ToString());
                var cw = new copyableW(msg, "Scouting Report", TextAlignment.Left);
                cw.ShowDialog();
            }
        }

        private void btnReloadStats_Click(object sender, RoutedEventArgs e)
        {
            int temp = cmbTeam.SelectedIndex;
            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = temp;
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if (tbcTeamOverview.SelectedItem == tabSplitStats)
            {
                if (rbStatsBetween.IsChecked.GetValueOrDefault())
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddYears(-1).AddDays(1);
                }
            }
             */
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            cmbTeam_SelectionChanged(sender, null);
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if (tbcTeamOverview.SelectedItem == tabSplitStats)
            {
                if (rbStatsBetween.IsChecked.GetValueOrDefault())
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddYears(1).AddDays(-1);
                }
            }
            */

            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            cmbTeam_SelectionChanged(sender, null);
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
            cmbTeam_SelectionChanged(sender, null);
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            FixTimeFrameForSplitStats();
            try
            {
                dtpEnd.IsEnabled = true;
                dtpStart.IsEnabled = true;
                cmbSeasonNum.IsEnabled = false;
            }
            catch
            {
            }
            cmbTeam_SelectionChanged(sender, null);
        }

        private void FixTimeFrameForSplitStats()
        {
            /*
            if (tbcTeamOverview.SelectedItem == tabSplitStats)
            {
                if (rbStatsBetween.IsChecked.GetValueOrDefault())
                {
                    try
                    {
                        if (dtpEnd.SelectedDate.GetValueOrDefault().AddYears(-1).AddDays(1) != dtpStart.SelectedDate.GetValueOrDefault())
                        {
                            dtpStart.SelectedDate = dtpEnd.SelectedDate.Value.AddYears(-1).AddDays(1);
                        }
                    }
                    catch
                    {
                        dtpEnd.SelectedDate = DateTime.Today;
                        dtpStart.SelectedDate = DateTime.Today.AddYears(-1).AddDays(1);
                    }
                }
            }
             */
        }

        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvBoxScores.SelectedItems[0];
                int gameid = Convert.ToInt32(row["GameID"].ToString());

                int i = 0;

                foreach (BoxScoreEntry bse in MainWindow.bshist)
                {
                    if (bse.bs.id == gameid)
                    {
                        MainWindow.bs = new BoxScore();

                        var bsw = new boxScoreW(boxScoreW.Mode.View, i);
                        bsw.ShowDialog();

                        MainWindow.UpdateBoxScore();
                        break;
                    }
                    i++;
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
            try
            {
                if (cmbOppTeam.SelectedIndex == -1) return;
            }
            catch
            {
                return;
            }

            dgvHTHBoxScores.DataContext = null;
            dgvHTHStats.DataContext = null;

            if (cmbOppTeam.SelectedIndex == cmbTeam.SelectedIndex)
            {
                return;
            }

            int iown = MainWindow.TeamOrder[cmbTeam.SelectedItem.ToString()];
            int iopp = MainWindow.TeamOrder[cmbOppTeam.SelectedItem.ToString()];

            string curTeam = cmbTeam.SelectedItem.ToString();
            string curOpp = cmbOppTeam.SelectedItem.ToString();

            var dt_hth_bs = new DataTable();
            dt_hth_bs.Columns.Add("Date");
            dt_hth_bs.Columns.Add("Home-Away");
            dt_hth_bs.Columns.Add("Result");
            dt_hth_bs.Columns.Add("Score");
            dt_hth_bs.Columns.Add("GameID");

            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats(curOpp);

            var db = new SQLiteDatabase(MainWindow.currentDB);

            var res = new DataTable();

            if (dt_hth.Rows.Count > 1) dt_hth.Rows.RemoveAt(dt_hth.Rows.Count - 1);

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                string q = String.Format("select * from GameResults " +
                                         "where (((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                                         "OR " +
                                         "((T1Name LIKE '{1}') AND (T2Name LIKE '{0}'))) AND SeasonNum = {2}",
                                         cmbTeam.SelectedItem,
                                         cmbOppTeam.SelectedItem,
                                         showSeason);

                res = db.GetDataTable(q);

                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
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
                                      "where ((((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                                      "OR " +
                                      "((T1Name LIKE '{1}') AND (T2Name LIKE '{0}'))) AND IsPlayoff LIKE 'False' AND SeasonNum = {2});",
                                      cmbTeam.SelectedItem,
                                      cmbOppTeam.SelectedItem,
                                      showSeason);

                    DataTable res2 = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp, false);

                    q = String.Format("select * from GameResults " +
                                      "where (((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                                      "OR " +
                                      "((T1Name LIKE '{1}') AND (T2Name LIKE '{0}')) AND IsPlayoff LIKE 'True' AND SeasonNum = {2});",
                                      cmbTeam.SelectedItem,
                                      cmbOppTeam.SelectedItem,
                                      showSeason);

                    res2 = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp, true);
                }
            }
            else
            {
                string q =
                    String.Format("select * from GameResults where ((((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                                  "OR ((T1Name LIKE '{1}') AND (T2Name LIKE '{0}'))) AND ((Date >= '{2}') AND (Date <= '{3}')))",
                                  cmbTeam.SelectedItem,
                                  cmbOppTeam.SelectedItem,
                                  SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault()),
                                  SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()));
                res = db.GetDataTable(q);

                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    string q2 =
                        String.Format("select * from GameResults where (((T1Name LIKE '{0}') OR (T2Name LIKE '{1}') " +
                                      "OR (T1Name LIKE '{1}') OR (T2Name LIKE '{0}')) AND ((Date >= '{2}') AND (Date <= '{3}')))",
                                      cmbTeam.SelectedItem,
                                      cmbOppTeam.SelectedItem,
                                      SQLiteDatabase.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault()),
                                      SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()));
                    DataTable res2 = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
                }
                else
                {
                    AddToTeamStatsFromSQLBoxScore(res, ref ts, ref tsopp);
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

            dv_hth = new DataView(dt_hth);
            dv_hth.AllowNew = false;
            dv_hth.AllowEdit = false;

            dgvHTHStats.DataContext = dv_hth;

            var dv_hth_bs = new DataView(dt_hth_bs);
            dv_hth_bs.AllowNew = false;
            dv_hth_bs.AllowEdit = false;
            dv_hth_bs.Sort = "Date DESC";

            dgvHTHBoxScores.DataContext = dv_hth_bs;
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
                dr["W%"] = String.Format("{0:F3}", ts.averages[Wp]);
                dr["Weff"] = String.Format("{0:F2}", ts.averages[Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.averages[PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.averages[PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.averages[PD]);
                dr["FG"] = String.Format("{0:F3}", ts.averages[FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.averages[FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.averages[TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.averages[TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.averages[FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.averages[FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.averages[RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.averages[ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.averages[DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.averages[APG]);
                dr["TO"] = String.Format("{0:F1}", ts.averages[TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.averages[SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.averages[BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.averages[FPG]);
            }
            else
            {
                dr["Games"] = ts.getPlayoffGames();
                dr["Wins"] = ts.pl_winloss[0].ToString();
                dr["Losses"] = ts.pl_winloss[1].ToString();
                dr["W%"] = String.Format("{0:F3}", ts.pl_averages[Wp]);
                dr["Weff"] = String.Format("{0:F2}", ts.pl_averages[Weff]);
                dr["PF"] = String.Format("{0:F1}", ts.pl_averages[PPG]);
                dr["PA"] = String.Format("{0:F1}", ts.pl_averages[PAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.pl_averages[PD]);
                dr["FG"] = String.Format("{0:F3}", ts.pl_averages[FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.pl_averages[FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.pl_averages[TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.pl_averages[TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.pl_averages[FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.pl_averages[FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.pl_averages[RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.pl_averages[ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.pl_averages[DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.pl_averages[APG]);
                dr["TO"] = String.Format("{0:F1}", ts.pl_averages[TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.pl_averages[SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.pl_averages[BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.pl_averages[FPG]);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbOppTeam.SelectedIndex = -1;
            cmbOppTeam.SelectedIndex = 1;
        }

        public static void AddToTeamStatsFromSQLBoxScore(DataTable res, ref TeamStats ts, ref TeamStats tsopp,
                                                        bool playoffs = false)
        {
            foreach (DataRow r in res.Rows)
            {
                if (!playoffs)
                {
                    int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                    int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                    if (r["T1Name"].ToString().Equals(ts.name))
                    {
                        if (t1pts > t2pts) ts.winloss[0]++;
                        else ts.winloss[1]++;
                        tsopp.stats[PA] = ts.stats[PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                        tsopp.stats[PF] = ts.stats[PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                        ts.stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                        ts.stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                        ts.stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                        ts.stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                        ts.stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                        ts.stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                        UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                        UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                        ts.stats[DREB] += (ushort) (T1reb - T1oreb);
                        ts.stats[OREB] += T1oreb;

                        ts.stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                        ts.stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                        ts.stats[BLK] += StatsTracker.getUShort(r, "T1BLK");
                        ts.stats[AST] += StatsTracker.getUShort(r, "T1AST");
                        ts.stats[FOUL] += StatsTracker.getUShort(r, "T1FOUL");

                        tsopp.stats[FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                        tsopp.stats[FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                        tsopp.stats[TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                        tsopp.stats[TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                        tsopp.stats[FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                        tsopp.stats[FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                        UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                        UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                        tsopp.stats[DREB] += (ushort) (T2reb - T2oreb);
                        tsopp.stats[OREB] += T2oreb;

                        tsopp.stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                        tsopp.stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                        tsopp.stats[BLK] += StatsTracker.getUShort(r, "T2BLK");
                        tsopp.stats[AST] += StatsTracker.getUShort(r, "T2AST");
                        tsopp.stats[FOUL] += StatsTracker.getUShort(r, "T2FOUL");
                    }
                    else
                    {
                        if (t2pts > t1pts) ts.winloss[0]++;
                        else ts.winloss[1]++;
                        tsopp.stats[PA] = ts.stats[PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                        tsopp.stats[PF] = ts.stats[PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                        ts.stats[FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                        ts.stats[FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                        ts.stats[TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                        ts.stats[TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                        ts.stats[FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                        ts.stats[FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                        UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                        UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                        ts.stats[DREB] += (ushort) (T2reb - T2oreb);
                        ts.stats[OREB] += T2oreb;

                        ts.stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                        ts.stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                        ts.stats[BLK] += StatsTracker.getUShort(r, "T2BLK");
                        ts.stats[AST] += StatsTracker.getUShort(r, "T2AST");
                        ts.stats[FOUL] += StatsTracker.getUShort(r, "T2FOUL");

                        tsopp.stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                        tsopp.stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                        tsopp.stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                        tsopp.stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                        tsopp.stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                        tsopp.stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                        UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                        UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                        tsopp.stats[DREB] += (ushort) (T1reb - T1oreb);
                        tsopp.stats[OREB] += T1oreb;

                        tsopp.stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                        tsopp.stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                        tsopp.stats[BLK] += StatsTracker.getUShort(r, "T1BLK");
                        tsopp.stats[AST] += StatsTracker.getUShort(r, "T1AST");
                        tsopp.stats[FOUL] += StatsTracker.getUShort(r, "T1FOUL");
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
                        tsopp.pl_stats[PA] = ts.pl_stats[PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                        tsopp.pl_stats[PF] = ts.pl_stats[PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                        ts.pl_stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                        ts.pl_stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                        ts.pl_stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                        ts.pl_stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                        ts.pl_stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                        ts.pl_stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                        UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                        UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                        ts.pl_stats[DREB] += (ushort) (T1reb - T1oreb);
                        ts.pl_stats[OREB] += T1oreb;

                        ts.pl_stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                        ts.pl_stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                        ts.pl_stats[BLK] += StatsTracker.getUShort(r, "T1BLK");
                        ts.pl_stats[AST] += StatsTracker.getUShort(r, "T1AST");
                        ts.pl_stats[FOUL] += StatsTracker.getUShort(r, "T1FOUL");

                        tsopp.pl_stats[FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                        tsopp.pl_stats[FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                        tsopp.pl_stats[TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                        tsopp.pl_stats[TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                        tsopp.pl_stats[FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                        tsopp.pl_stats[FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                        UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                        UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                        tsopp.pl_stats[DREB] += (ushort) (T2reb - T2oreb);
                        tsopp.pl_stats[OREB] += T2oreb;

                        tsopp.pl_stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                        tsopp.pl_stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                        tsopp.pl_stats[BLK] += StatsTracker.getUShort(r, "T2BLK");
                        tsopp.pl_stats[AST] += StatsTracker.getUShort(r, "T2AST");
                        tsopp.pl_stats[FOUL] += StatsTracker.getUShort(r, "T2FOUL");
                    }
                    else
                    {
                        if (t2pts > t1pts) ts.pl_winloss[0]++;
                        else ts.pl_winloss[1]++;
                        tsopp.pl_stats[PA] = ts.pl_stats[PF] += Convert.ToUInt16(r["T2PTS"].ToString());
                        tsopp.pl_stats[PF] = ts.pl_stats[PA] += Convert.ToUInt16(r["T1PTS"].ToString());

                        ts.pl_stats[FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                        ts.pl_stats[FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                        ts.pl_stats[TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                        ts.pl_stats[TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                        ts.pl_stats[FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                        ts.pl_stats[FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                        UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                        UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                        ts.pl_stats[DREB] += (ushort) (T2reb - T2oreb);
                        ts.pl_stats[OREB] += T2oreb;

                        ts.pl_stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                        ts.pl_stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                        ts.pl_stats[BLK] += StatsTracker.getUShort(r, "T2BLK");
                        ts.pl_stats[AST] += StatsTracker.getUShort(r, "T2AST");
                        ts.pl_stats[FOUL] += StatsTracker.getUShort(r, "T2FOUL");

                        tsopp.pl_stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                        tsopp.pl_stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                        tsopp.pl_stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                        tsopp.pl_stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                        tsopp.pl_stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                        tsopp.pl_stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                        UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                        UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                        tsopp.pl_stats[DREB] += (ushort) (T1reb - T1oreb);
                        tsopp.pl_stats[OREB] += T1oreb;

                        tsopp.pl_stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                        tsopp.pl_stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                        tsopp.pl_stats[BLK] += StatsTracker.getUShort(r, "T1BLK");
                        tsopp.pl_stats[AST] += StatsTracker.getUShort(r, "T1AST");
                        tsopp.pl_stats[FOUL] += StatsTracker.getUShort(r, "T1FOUL");
                    }

                    tsopp.pl_winloss[1] = ts.pl_winloss[0];
                    tsopp.pl_winloss[0] = ts.pl_winloss[1];
                }

                ts.calcAvg();
                tsopp.calcAvg();
            }
        }

        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppTeam_SelectionChanged(sender, null);
        }

        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppTeam_SelectionChanged(sender, null);
        }

        private void tbcTeamOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if ((tbcTeamOverview.SelectedItem == tabSplitStats) || (tbcTeamOverview.SelectedItem == tabHTH) || (tbcTeamOverview.SelectedItem == tabYearly))
            {
                cmbTeam_SelectionChanged(null, null);
            }
            FixTimeFrameForSplitStats();
            */
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
            cmbTeam_SelectionChanged(null, null);
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayerStats.SelectedCells.Count > 0)
            {
                PlayerStatsRow row = (PlayerStatsRow) dgvPlayerStats.SelectedItems[0];
                int playerID = row.ID;

                playerOverviewW pow = new playerOverviewW(curTeam, playerID);
                pow.ShowDialog();

                UpdatePlayerStats();
            }
        }

        private void dgvHTHBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                DataRowView row;
                try
                {
                    row = (DataRowView) dgvHTHBoxScores.SelectedItems[0];
                }
                catch
                {
                    return;
                }
                int gameid = Convert.ToInt32(row["GameID"].ToString());
            
                int i = 0;
            
                foreach (BoxScoreEntry bse in MainWindow.bshist)
                {
                    if (bse.bs.id == gameid)
                    {
                        MainWindow.bs = new BoxScore();
            
                        var bsw = new boxScoreW(boxScoreW.Mode.View, i);
                        bsw.ShowDialog();

                        MainWindow.UpdateBoxScore();
                        break;
                    }
                    i++;
                }
            }
        }
    }
}