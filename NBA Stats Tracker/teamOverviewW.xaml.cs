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
        public const int tMINS = 0, tPF = 1, tPA = 2, tFGM = 4, tFGA = 5, tTPM = 6, tTPA = 7, tFTM = 8, tFTA = 9, tOREB = 10, tDREB = 11, tSTL = 12, tTO = 13, tBLK = 14, tAST = 15, tFOUL = 16;

        public const int tPPG = 0, tPAPG = 1, tFGp = 2, tFGeff = 3, tTPp = 4, tTPeff = 5, tFTp = 6, tFTeff = 7, tRPG = 8, tORPG = 9, tDRPG = 10, tSPG = 11, tBPG = 12, tTPG = 13, tAPG = 14, tFPG = 15, tWp = 16, tWeff = 17, tPD = 18;

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
        private ObservableCollection<PlayerStatsRow> psrList;
        private Dictionary<int, PlayerStats> pst;
        private TeamStats[] tst;
        private TeamStats[] tstopp;
        private int curSeason = MainWindow.curSeason;
        private int maxSeason = MainWindow.getMaxSeason(MainWindow.currentDB);
        private TeamStats curts;
        private TeamStats curtsopp;

        public teamOverviewW()
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
            dt_bs.Columns.Add("Date");
            dt_bs.Columns.Add("Opponent");
            dt_bs.Columns.Add("Home-Away");
            dt_bs.Columns.Add("Result");
            dt_bs.Columns.Add("Score");
            dt_bs.Columns.Add("GameID");

            #endregion

            tst = MainWindow.tst;
            pst = MainWindow.pst;
            tstopp = MainWindow.tstopp;

            foreach (var kvp in MainWindow.TeamOrder)
            {
                cmbTeam.Items.Add(kvp.Key);
                cmbOppTeam.Items.Add(kvp.Key);
            }

            rankings = NSTHelper.calculateRankings(tst);
            pl_rankings = NSTHelper.calculateRankings(tst, playoffs: true);

            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1).AddDays(1);

            PopulateSeasonCombo();

            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvPlayerStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvSplit.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvYearly.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvTeamStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = 0;
        }

        public teamOverviewW(string team) : this()
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
            curts = new TeamStats(curTeam);
            curtsopp = new TeamStats("Opponents");

            int i = MainWindow.TeamOrder[curTeam];

            #region Prepare Team Overview & Box Scores

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                curts = tst[i];
                curtsopp = tstopp[i];

                DataTable res;
                String q = "select * from GameResults where ((T1Name LIKE '" + curTeam + "') OR (T2Name LIKE '"
                           + curTeam + "')) AND SeasonNum = " + curSeason + " ORDER BY Date DESC";
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
                               SQLiteDatabase.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()) + "'))" +
                               " ORDER BY Date DESC";
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
                        curtsopp.winloss[1] = curts.winloss[0];
                        curtsopp.winloss[0] = curts.winloss[1];
                    }
                    AddToTeamStatsFromSQLBoxScore(res, ref curts, ref curtsopp);
                    curts.CalcMetrics(curtsopp);
                }
            }

            #region Regular Season

            DataRow dr = dt_ov.NewRow();

            dr["Type"] = "Stats";
            dr["Games"] = curts.getGames();
            dr["Wins (W%)"] = curts.winloss[0].ToString();
            dr["Losses (Weff)"] = curts.winloss[1].ToString();
            dr["PF"] = curts.stats[tPF].ToString();
            dr["PA"] = curts.stats[tPA].ToString();
            dr["PD"] = " ";
            dr["FG"] = curts.stats[tFGM].ToString() + "-" + curts.stats[tFGA].ToString();
            dr["3PT"] = curts.stats[tTPM].ToString() + "-" + curts.stats[tTPA].ToString();
            dr["FT"] = curts.stats[tFTM].ToString() + "-" + curts.stats[tFTA].ToString();
            dr["REB"] = (curts.stats[tDREB] + curts.stats[tOREB]).ToString();
            dr["OREB"] = curts.stats[tOREB].ToString();
            dr["DREB"] = curts.stats[tDREB].ToString();
            dr["AST"] = curts.stats[tAST].ToString();
            dr["TO"] = curts.stats[tTO].ToString();
            dr["STL"] = curts.stats[tSTL].ToString();
            dr["BLK"] = curts.stats[tBLK].ToString();
            dr["FOUL"] = curts.stats[tFOUL].ToString();
            dr["MINS"] = curts.stats[tMINS].ToString();

            dt_ov.Rows.Add(dr);

            dr = dt_ov.NewRow();

            curts.calcAvg(); // Just to be sure...

            dr["Type"] = "Averages";
            //dr["Games"] = curts.getGames();
            dr["Wins (W%)"] = String.Format("{0:F3}", curts.averages[tWp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", curts.averages[tWeff]);
            dr["PF"] = String.Format("{0:F1}", curts.averages[tPPG]);
            dr["PA"] = String.Format("{0:F1}", curts.averages[tPAPG]);
            dr["PD"] = String.Format("{0:F1}", curts.averages[tPD]);
            dr["FG"] = String.Format("{0:F3}", curts.averages[tFGp]);
            dr["FGeff"] = String.Format("{0:F2}", curts.averages[tFGeff]);
            dr["3PT"] = String.Format("{0:F3}", curts.averages[tTPp]);
            dr["3Peff"] = String.Format("{0:F2}", curts.averages[tTPeff]);
            dr["FT"] = String.Format("{0:F3}", curts.averages[tFTp]);
            dr["FTeff"] = String.Format("{0:F2}", curts.averages[tFTeff]);
            dr["REB"] = String.Format("{0:F1}", curts.averages[tRPG]);
            dr["OREB"] = String.Format("{0:F1}", curts.averages[tORPG]);
            dr["DREB"] = String.Format("{0:F1}", curts.averages[tDRPG]);
            dr["AST"] = String.Format("{0:F1}", curts.averages[tAPG]);
            dr["TO"] = String.Format("{0:F1}", curts.averages[tTPG]);
            dr["STL"] = String.Format("{0:F1}", curts.averages[tSPG]);
            dr["BLK"] = String.Format("{0:F1}", curts.averages[tBPG]);
            dr["FOUL"] = String.Format("{0:F1}", curts.averages[tFPG]);

            dt_ov.Rows.Add(dr);

            // Rankings can only be shown based on total stats
            // ...for now
            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                DataRow dr2 = dt_ov.NewRow();

                dr2["Type"] = "Rankings";
                dr2["Wins (W%)"] = rankings[i][tWp];
                dr2["Losses (Weff)"] = rankings[i][tWeff];
                dr2["PF"] = rankings[i][tPPG];
                dr2["PA"] = cmbTeam.Items.Count + 1 - rankings[i][tPAPG];
                dr2["PD"] = rankings[i][tPD];
                dr2["FG"] = rankings[i][tFGp];
                dr2["FGeff"] = rankings[i][tFGeff];
                dr2["3PT"] = rankings[i][tTPp];
                dr2["3Peff"] = rankings[i][tTPeff];
                dr2["FT"] = rankings[i][tFTp];
                dr2["FTeff"] = rankings[i][tFTeff];
                dr2["REB"] = rankings[i][tRPG];
                dr2["OREB"] = rankings[i][tORPG];
                dr2["DREB"] = rankings[i][tDRPG];
                dr2["AST"] = rankings[i][tAST];
                dr2["TO"] = cmbTeam.Items.Count + 1 - rankings[i][tTO];
                dr2["STL"] = rankings[i][tSTL];
                dr2["BLK"] = rankings[i][tBLK];
                dr2["FOUL"] = cmbTeam.Items.Count + 1 - rankings[i][tFOUL];

                dt_ov.Rows.Add(dr2);

                dr = dt_ov.NewRow();

                dr["Type"] = "Opp Stats";
                dr["Games"] = curtsopp.getGames();
                dr["Wins (W%)"] = curtsopp.winloss[0].ToString();
                dr["Losses (Weff)"] = curtsopp.winloss[1].ToString();
                dr["PF"] = curtsopp.stats[tPF].ToString();
                dr["PA"] = curtsopp.stats[tPA].ToString();
                dr["PD"] = " ";
                dr["FG"] = curtsopp.stats[tFGM].ToString() + "-" + curtsopp.stats[tFGA].ToString();
                dr["3PT"] = curtsopp.stats[tTPM].ToString() + "-" + curtsopp.stats[tTPA].ToString();
                dr["FT"] = curtsopp.stats[tFTM].ToString() + "-" + curtsopp.stats[tFTA].ToString();
                dr["REB"] = (curtsopp.stats[tDREB] + curtsopp.stats[tOREB]).ToString();
                dr["OREB"] = curtsopp.stats[tOREB].ToString();
                dr["DREB"] = curtsopp.stats[tDREB].ToString();
                dr["AST"] = curtsopp.stats[tAST].ToString();
                dr["TO"] = curtsopp.stats[tTO].ToString();
                dr["STL"] = curtsopp.stats[tSTL].ToString();
                dr["BLK"] = curtsopp.stats[tBLK].ToString();
                dr["FOUL"] = curtsopp.stats[tFOUL].ToString();
                dr["MINS"] = curtsopp.stats[tMINS].ToString();

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                curtsopp.calcAvg(); // Just to be sure...

                dr["Type"] = "Opp Avg";
                //dr["Games"] = curtsopp.getGames();
                dr["Wins (W%)"] = String.Format("{0:F3}", curtsopp.averages[tWp]);
                dr["Losses (Weff)"] = String.Format("{0:F2}", curtsopp.averages[tWeff]);
                dr["PF"] = String.Format("{0:F1}", curtsopp.averages[tPPG]);
                dr["PA"] = String.Format("{0:F1}", curtsopp.averages[tPAPG]);
                dr["PD"] = String.Format("{0:F1}", curtsopp.averages[tPD]);
                dr["FG"] = String.Format("{0:F3}", curtsopp.averages[tFGp]);
                dr["FGeff"] = String.Format("{0:F2}", curtsopp.averages[tFGeff]);
                dr["3PT"] = String.Format("{0:F3}", curtsopp.averages[tTPp]);
                dr["3Peff"] = String.Format("{0:F2}", curtsopp.averages[tTPeff]);
                dr["FT"] = String.Format("{0:F3}", curtsopp.averages[tFTp]);
                dr["FTeff"] = String.Format("{0:F2}", curtsopp.averages[tFTeff]);
                dr["REB"] = String.Format("{0:F1}", curtsopp.averages[tRPG]);
                dr["OREB"] = String.Format("{0:F1}", curtsopp.averages[tORPG]);
                dr["DREB"] = String.Format("{0:F1}", curtsopp.averages[tDRPG]);
                dr["AST"] = String.Format("{0:F1}", curtsopp.averages[tAPG]);
                dr["TO"] = String.Format("{0:F1}", curtsopp.averages[tTPG]);
                dr["STL"] = String.Format("{0:F1}", curtsopp.averages[tSPG]);
                dr["BLK"] = String.Format("{0:F1}", curtsopp.averages[tBPG]);
                dr["FOUL"] = String.Format("{0:F1}", curtsopp.averages[tFPG]);

                dt_ov.Rows.Add(dr);
            }
            else
            {
                DataRow dr2 = dt_ov.NewRow();

                dr2["Type"] = "Opp Stats";
                dr2["Games"] = curtsopp.getGames();
                dr2["Wins (W%)"] = curtsopp.winloss[0].ToString();
                dr2["Losses (Weff)"] = curtsopp.winloss[1].ToString();
                dr2["PF"] = curtsopp.stats[tPF].ToString();
                dr2["PA"] = curtsopp.stats[tPA].ToString();
                dr2["PD"] = " ";
                dr2["FG"] = curtsopp.stats[tFGM].ToString() + "-" + curtsopp.stats[tFGA].ToString();
                dr2["3PT"] = curtsopp.stats[tTPM].ToString() + "-" + curtsopp.stats[tTPA].ToString();
                dr2["FT"] = curtsopp.stats[tFTM].ToString() + "-" + curtsopp.stats[tFTA].ToString();
                dr2["REB"] = (curtsopp.stats[tDREB] + curtsopp.stats[tOREB]).ToString();
                dr2["OREB"] = curtsopp.stats[tOREB].ToString();
                dr2["DREB"] = curtsopp.stats[tDREB].ToString();
                dr2["AST"] = curtsopp.stats[tAST].ToString();
                dr2["TO"] = curtsopp.stats[tTO].ToString();
                dr2["STL"] = curtsopp.stats[tSTL].ToString();
                dr2["BLK"] = curtsopp.stats[tBLK].ToString();
                dr2["FOUL"] = curtsopp.stats[tFOUL].ToString();
                dr2["MINS"] = curtsopp.stats[tMINS].ToString();

                dt_ov.Rows.Add(dr2);

                dr2 = dt_ov.NewRow();

                curtsopp.calcAvg(); // Just to be sure...

                dr2["Type"] = "Opp Avg";
                dr2["Wins (W%)"] = String.Format("{0:F3}", curtsopp.averages[tWp]);
                dr2["Losses (Weff)"] = String.Format("{0:F2}", curtsopp.averages[tWeff]);
                dr2["PF"] = String.Format("{0:F1}", curtsopp.averages[tPPG]);
                dr2["PA"] = String.Format("{0:F1}", curtsopp.averages[tPAPG]);
                dr2["PD"] = String.Format("{0:F1}", curtsopp.averages[tPD]);
                dr2["FG"] = String.Format("{0:F3}", curtsopp.averages[tFGp]);
                dr2["FGeff"] = String.Format("{0:F2}", curtsopp.averages[tFGeff]);
                dr2["3PT"] = String.Format("{0:F3}", curtsopp.averages[tTPp]);
                dr2["3Peff"] = String.Format("{0:F2}", curtsopp.averages[tTPeff]);
                dr2["FT"] = String.Format("{0:F3}", curtsopp.averages[tFTp]);
                dr2["FTeff"] = String.Format("{0:F2}", curtsopp.averages[tFTeff]);
                dr2["REB"] = String.Format("{0:F1}", curtsopp.averages[tRPG]);
                dr2["OREB"] = String.Format("{0:F1}", curtsopp.averages[tORPG]);
                dr2["DREB"] = String.Format("{0:F1}", curtsopp.averages[tDRPG]);
                dr2["AST"] = String.Format("{0:F1}", curtsopp.averages[tAST]);
                dr2["TO"] = String.Format("{0:F1}", curtsopp.averages[tTO]);
                dr2["STL"] = String.Format("{0:F1}", curtsopp.averages[tSTL]);
                dr2["BLK"] = String.Format("{0:F1}", curtsopp.averages[tBLK]);
                dr2["FOUL"] = String.Format("{0:F1}", curtsopp.averages[tFOUL]);

                dt_ov.Rows.Add(dr2);

                curtsopp.calcAvg();
            }

            #endregion

            #region Playoffs

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                dr = dt_ov.NewRow();

                dr["Type"] = "Playoffs";
                dr["Games"] = curts.getPlayoffGames();
                dr["Wins (W%)"] = curts.pl_winloss[0].ToString();
                dr["Losses (Weff)"] = curts.pl_winloss[1].ToString();
                dr["PF"] = curts.pl_stats[tPF].ToString();
                dr["PA"] = curts.pl_stats[tPA].ToString();
                dr["PD"] = " ";
                dr["FG"] = curts.pl_stats[tFGM].ToString() + "-" + curts.pl_stats[tFGA].ToString();
                dr["3PT"] = curts.pl_stats[tTPM].ToString() + "-" + curts.pl_stats[tTPA].ToString();
                dr["FT"] = curts.pl_stats[tFTM].ToString() + "-" + curts.pl_stats[tFTA].ToString();
                dr["REB"] = (curts.pl_stats[tDREB] + curts.pl_stats[tOREB]).ToString();
                dr["OREB"] = curts.pl_stats[tOREB].ToString();
                dr["DREB"] = curts.pl_stats[tDREB].ToString();
                dr["AST"] = curts.pl_stats[tAST].ToString();
                dr["TO"] = curts.pl_stats[tTO].ToString();
                dr["STL"] = curts.pl_stats[tSTL].ToString();
                dr["BLK"] = curts.pl_stats[tBLK].ToString();
                dr["FOUL"] = curts.pl_stats[tFOUL].ToString();
                dr["MINS"] = curts.pl_stats[tMINS].ToString();

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Pl Avg";
                dr["Wins (W%)"] = String.Format("{0:F3}", curts.pl_averages[tWp]);
                dr["Losses (Weff)"] = String.Format("{0:F2}", curts.pl_averages[tWeff]);
                dr["PF"] = String.Format("{0:F1}", curts.pl_averages[tPPG]);
                dr["PA"] = String.Format("{0:F1}", curts.pl_averages[tPAPG]);
                dr["PD"] = String.Format("{0:F1}", curts.pl_averages[tPD]);
                dr["FG"] = String.Format("{0:F3}", curts.pl_averages[tFGp]);
                dr["FGeff"] = String.Format("{0:F2}", curts.pl_averages[tFGeff]);
                dr["3PT"] = String.Format("{0:F3}", curts.pl_averages[tTPp]);
                dr["3Peff"] = String.Format("{0:F2}", curts.pl_averages[tTPeff]);
                dr["FT"] = String.Format("{0:F3}", curts.pl_averages[tFTp]);
                dr["FTeff"] = String.Format("{0:F2}", curts.pl_averages[tFTeff]);
                dr["REB"] = String.Format("{0:F1}", curts.pl_averages[tRPG]);
                dr["OREB"] = String.Format("{0:F1}", curts.pl_averages[tORPG]);
                dr["DREB"] = String.Format("{0:F1}", curts.pl_averages[tDRPG]);
                dr["AST"] = String.Format("{0:F1}", curts.pl_averages[tAPG]);
                dr["TO"] = String.Format("{0:F1}", curts.pl_averages[tTPG]);
                dr["STL"] = String.Format("{0:F1}", curts.pl_averages[tSPG]);
                dr["BLK"] = String.Format("{0:F1}", curts.pl_averages[tBPG]);
                dr["FOUL"] = String.Format("{0:F1}", curts.pl_averages[tFPG]);

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
                    dr2["Wins (W%)"] = pl_rankings[i][tWp];
                    dr2["Losses (Weff)"] = pl_rankings[i][tWeff];
                    dr2["PF"] = pl_rankings[i][tPPG];
                    dr2["PA"] = count + 1 - pl_rankings[i][tPAPG];
                    dr2["PD"] = pl_rankings[i][tPD];
                    dr2["FG"] = pl_rankings[i][tFGp];
                    dr2["FGeff"] = pl_rankings[i][tFGeff];
                    dr2["3PT"] = pl_rankings[i][tTPp];
                    dr2["3Peff"] = pl_rankings[i][tTPeff];
                    dr2["FT"] = pl_rankings[i][tFTp];
                    dr2["FTeff"] = pl_rankings[i][tFTeff];
                    dr2["REB"] = pl_rankings[i][tRPG];
                    dr2["OREB"] = pl_rankings[i][tORPG];
                    dr2["DREB"] = pl_rankings[i][tDRPG];
                    dr2["AST"] = pl_rankings[i][tAST];
                    dr2["TO"] = count + 1 - pl_rankings[i][tTO];
                    dr2["STL"] = pl_rankings[i][tSTL];
                    dr2["BLK"] = pl_rankings[i][tBLK];
                    dr2["FOUL"] = count + 1 - pl_rankings[i][tFOUL];

                    dt_ov.Rows.Add(dr2);

                    dr = dt_ov.NewRow();

                    dr["Type"] = "Opp Pl Stats";
                    dr["Games"] = curtsopp.getPlayoffGames();
                    dr["Wins (W%)"] = curtsopp.winloss[0].ToString();
                    dr["Losses (Weff)"] = curtsopp.winloss[1].ToString();
                    dr["PF"] = curtsopp.pl_stats[tPF].ToString();
                    dr["PA"] = curtsopp.pl_stats[tPA].ToString();
                    dr["PD"] = " ";
                    dr["FG"] = curtsopp.pl_stats[tFGM].ToString() + "-" + curtsopp.pl_stats[tFGA].ToString();
                    dr["3PT"] = curtsopp.pl_stats[tTPM].ToString() + "-" + curtsopp.pl_stats[tTPA].ToString();
                    dr["FT"] = curtsopp.pl_stats[tFTM].ToString() + "-" + curtsopp.pl_stats[tFTA].ToString();
                    dr["REB"] = (curtsopp.pl_stats[tDREB] + curtsopp.pl_stats[tOREB]).ToString();
                    dr["OREB"] = curtsopp.pl_stats[tOREB].ToString();
                    dr["DREB"] = curtsopp.pl_stats[tDREB].ToString();
                    dr["AST"] = curtsopp.pl_stats[tAST].ToString();
                    dr["TO"] = curtsopp.pl_stats[tTO].ToString();
                    dr["STL"] = curtsopp.pl_stats[tSTL].ToString();
                    dr["BLK"] = curtsopp.pl_stats[tBLK].ToString();
                    dr["FOUL"] = curtsopp.pl_stats[tFOUL].ToString();
                    dr["MINS"] = curtsopp.pl_stats[tMINS].ToString();

                    dt_ov.Rows.Add(dr);

                    dr = dt_ov.NewRow();

                    dr["Type"] = "Opp Pl Avg";
                    //dr["Games"] = curtsopp.getGames();
                    dr["Wins (W%)"] = String.Format("{0:F3}", curtsopp.pl_averages[tWp]);
                    dr["Losses (Weff)"] = String.Format("{0:F2}", curtsopp.pl_averages[tWeff]);
                    dr["PF"] = String.Format("{0:F1}", curtsopp.pl_averages[tPPG]);
                    dr["PA"] = String.Format("{0:F1}", curtsopp.pl_averages[tPAPG]);
                    dr["PD"] = String.Format("{0:F1}", curtsopp.pl_averages[tPD]);
                    dr["FG"] = String.Format("{0:F3}", curtsopp.pl_averages[tFGp]);
                    dr["FGeff"] = String.Format("{0:F2}", curtsopp.pl_averages[tFGeff]);
                    dr["3PT"] = String.Format("{0:F3}", curtsopp.pl_averages[tTPp]);
                    dr["3Peff"] = String.Format("{0:F2}", curtsopp.pl_averages[tTPeff]);
                    dr["FT"] = String.Format("{0:F3}", curtsopp.pl_averages[tFTp]);
                    dr["FTeff"] = String.Format("{0:F2}", curtsopp.pl_averages[tFTeff]);
                    dr["REB"] = String.Format("{0:F1}", curtsopp.pl_averages[tRPG]);
                    dr["OREB"] = String.Format("{0:F1}", curtsopp.pl_averages[tORPG]);
                    dr["DREB"] = String.Format("{0:F1}", curtsopp.pl_averages[tDRPG]);
                    dr["AST"] = String.Format("{0:F1}", curtsopp.pl_averages[tAPG]);
                    dr["TO"] = String.Format("{0:F1}", curtsopp.pl_averages[tTPG]);
                    dr["STL"] = String.Format("{0:F1}", curtsopp.pl_averages[tSPG]);
                    dr["BLK"] = String.Format("{0:F1}", curtsopp.pl_averages[tBPG]);
                    dr["FOUL"] = String.Format("{0:F1}", curtsopp.pl_averages[tFPG]);

                    dt_ov.Rows.Add(dr);
                }
                else
                {
                    DataRow dr2 = dt_ov.NewRow();

                    dr2["Type"] = "Opp Pl Stats";
                    dr2["Games"] = curtsopp.getPlayoffGames();
                    dr2["Wins (W%)"] = curtsopp.pl_winloss[0].ToString();
                    dr2["Losses (Weff)"] = curtsopp.pl_winloss[1].ToString();
                    dr2["PF"] = curtsopp.pl_stats[tPF].ToString();
                    dr2["PA"] = curtsopp.pl_stats[tPA].ToString();
                    dr2["PD"] = " ";
                    dr2["FG"] = curtsopp.pl_stats[tFGM].ToString() + "-" + curtsopp.pl_stats[tFGA].ToString();
                    dr2["3PT"] = curtsopp.pl_stats[tTPM].ToString() + "-" + curtsopp.pl_stats[tTPA].ToString();
                    dr2["FT"] = curtsopp.pl_stats[tFTM].ToString() + "-" + curtsopp.pl_stats[tFTA].ToString();
                    dr2["REB"] = (curtsopp.pl_stats[tDREB] + curtsopp.pl_stats[tOREB]).ToString();
                    dr2["OREB"] = curtsopp.pl_stats[tOREB].ToString();
                    dr2["DREB"] = curtsopp.pl_stats[tDREB].ToString();
                    dr2["AST"] = curtsopp.pl_stats[tAST].ToString();
                    dr2["TO"] = curtsopp.pl_stats[tTO].ToString();
                    dr2["STL"] = curtsopp.pl_stats[tSTL].ToString();
                    dr2["BLK"] = curtsopp.pl_stats[tBLK].ToString();
                    dr2["FOUL"] = curtsopp.pl_stats[tFOUL].ToString();

                    dt_ov.Rows.Add(dr2);

                    dr2 = dt_ov.NewRow();

                    curtsopp.calcAvg(); // Just to be sure...

                    dr2["Type"] = "Opp Pl Avg";
                    dr2["Wins (W%)"] = String.Format("{0:F3}", curtsopp.pl_averages[tWp]);
                    dr2["Losses (Weff)"] = String.Format("{0:F2}", curtsopp.pl_averages[tWeff]);
                    dr2["PF"] = String.Format("{0:F1}", curtsopp.pl_averages[tPPG]);
                    dr2["PA"] = String.Format("{0:F1}", curtsopp.pl_averages[tPAPG]);
                    dr2["PD"] = String.Format("{0:F1}", curtsopp.pl_averages[tPD]);
                    dr2["FG"] = String.Format("{0:F3}", curtsopp.pl_averages[tFGp]);
                    dr2["FGeff"] = String.Format("{0:F2}", curtsopp.pl_averages[tFGeff]);
                    dr2["3PT"] = String.Format("{0:F3}", curtsopp.pl_averages[tTPp]);
                    dr2["3Peff"] = String.Format("{0:F2}", curtsopp.pl_averages[tTPeff]);
                    dr2["FT"] = String.Format("{0:F3}", curtsopp.pl_averages[tFTp]);
                    dr2["FTeff"] = String.Format("{0:F2}", curtsopp.pl_averages[tFTeff]);
                    dr2["REB"] = String.Format("{0:F1}", curtsopp.pl_averages[tRPG]);
                    dr2["OREB"] = String.Format("{0:F1}", curtsopp.pl_averages[tORPG]);
                    dr2["DREB"] = String.Format("{0:F1}", curtsopp.pl_averages[tDRPG]);
                    dr2["AST"] = String.Format("{0:F1}", curtsopp.pl_averages[tAST]);
                    dr2["TO"] = String.Format("{0:F1}", curtsopp.pl_averages[tTO]);
                    dr2["STL"] = String.Format("{0:F1}", curtsopp.pl_averages[tSTL]);
                    dr2["BLK"] = String.Format("{0:F1}", curtsopp.pl_averages[tBLK]);
                    dr2["FOUL"] = String.Format("{0:F1}", curtsopp.pl_averages[tFOUL]);

                    dt_ov.Rows.Add(dr2);

                    curtsopp.calcAvg();
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
                string s = " AND SeasonNum = " + cmbSeasonNum.SelectedItem;
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
                    if (new DateTime(dCur.Year, dCur.Month, 1) == new DateTime(dEnd.Year, dEnd.Month, 1))
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

            curTeam = cmbTeam.SelectedItem.ToString();
            var ts = new TeamStats(curTeam);
            var tsopp = new TeamStats("Opponents");

            UpdateOverviewAndBoxScores();

            UpdateSplitStats();

            ts = tst[MainWindow.TeamOrder[curTeam]];
            Title = cmbTeam.SelectedItem + " Team Overview - " + ts.getGames() + " games played";

            UpdateHeadToHead();

            UpdateYearlyStats();

            UpdatePlayerAndMetricStats();
        }

        private void UpdatePlayerAndMetricStats()
        {
            string playersT = "Players";
            if (curSeason != maxSeason) playersT += "S" + curSeason;

            string q;
            DataTable res;
            q = "select * from " + playersT + " where TeamFin LIKE '" + curTeam + "'";
            res = db.GetDataTable(q);

            psrList = new ObservableCollection<PlayerStatsRow>();
            ObservableCollection<PlayerMetricStatsRow> pmsrList = new ObservableCollection<PlayerMetricStatsRow>();

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                foreach (DataRow r in res.Rows)
                {
                    PlayerStats ps = pst[Convert.ToInt32(r["ID"].ToString())];
                    psrList.Add(new PlayerStatsRow(ps));
                    pmsrList.Add(new PlayerMetricStatsRow(ps));
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
                    TeamStats curTSAll = new TeamStats(curTeam);
                    curTSAll.AddTeamStats(curts, "All");
                    TeamStats curTSOppAll = new TeamStats(curTeam);
                    curTSOppAll.AddTeamStats(curtsopp, "All");
                    curTSAll.CalcMetrics(curTSOppAll);
                    psBetween.CalcMetrics(curTSAll, curTSOppAll, new TeamStats("$$Empty"));

                    psrList.Add(new PlayerStatsRow(psBetween));
                    pmsrList.Add(new PlayerMetricStatsRow(psBetween));

                    dgvMetricStatsPERColumn.Visibility = Visibility.Collapsed;
                    dgvMetricStatsEFFColumn.Visibility = Visibility.Collapsed;
                    dgvMetricStatsPPRColumn.Visibility = Visibility.Collapsed;
                }
            }

            dgvPlayerStats.ItemsSource = psrList;
            dgvMetricStats.ItemsSource = pmsrList;
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
            TeamStats tsopp = tstopp[MainWindow.TeamOrder[curTeam]];
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
                    MainWindow.GetTeamStatsFromDatabase(MainWindow.currentDB, curTeam, j, ref ts, ref tsopp);
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

            var dv_yea = new DataView(dt_yea);
            dv_yea.AllowNew = false;
            dv_yea.AllowEdit = false;

            dgvYearly.DataContext = dv_yea;

            #endregion
        }

        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            string msg = NSTHelper.averagesAndRankings(cmbTeam.SelectedItem.ToString(), tst, MainWindow.TeamOrder);
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
            tst[id].stats[tPF] = Convert.ToUInt16(myCell(0, 4));
            tst[id].stats[tPA] = Convert.ToUInt16(myCell(0, 5));

            string[] parts = myCell(0, 7).Split('-');
            tst[id].stats[tFGM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[tFGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 9).Split('-');
            tst[id].stats[tTPM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[tTPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 11).Split('-');
            tst[id].stats[tFTM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[tFTA] = Convert.ToUInt16(parts[1]);

            tst[id].stats[tOREB] = Convert.ToUInt16(myCell(0, 14));
            tst[id].stats[tDREB] = Convert.ToUInt16(myCell(0, 15));

            tst[id].stats[tAST] = Convert.ToUInt16(myCell(0, 16));
            tst[id].stats[tTO] = Convert.ToUInt16(myCell(0, 17));
            tst[id].stats[tSTL] = Convert.ToUInt16(myCell(0, 18));
            tst[id].stats[tBLK] = Convert.ToUInt16(myCell(0, 19));
            tst[id].stats[tFOUL] = Convert.ToUInt16(myCell(0, 20));
            tst[id].stats[tMINS] = Convert.ToUInt16(myCell(0, 21));

            tst[id].pl_winloss[0] = Convert.ToByte(myCell(5, 2));
            tst[id].pl_winloss[1] = Convert.ToByte(myCell(5, 3));
            tst[id].pl_stats[tPF] = Convert.ToUInt16(myCell(5, 4));
            tst[id].pl_stats[tPA] = Convert.ToUInt16(myCell(5, 5));

            parts = myCell(5, 7).Split('-');
            tst[id].pl_stats[tFGM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[tFGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 9).Split('-');
            tst[id].pl_stats[tTPM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[tTPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 11).Split('-');
            tst[id].pl_stats[tFTM] = Convert.ToUInt16(parts[0]);
            tst[id].pl_stats[tFTA] = Convert.ToUInt16(parts[1]);

            tst[id].pl_stats[tOREB] = Convert.ToUInt16(myCell(5, 14));
            tst[id].pl_stats[tDREB] = Convert.ToUInt16(myCell(5, 15));

            tst[id].pl_stats[tAST] = Convert.ToUInt16(myCell(5, 16));
            tst[id].pl_stats[tTO] = Convert.ToUInt16(myCell(5, 17));
            tst[id].pl_stats[tSTL] = Convert.ToUInt16(myCell(5, 18));
            tst[id].pl_stats[tBLK] = Convert.ToUInt16(myCell(5, 19));
            tst[id].pl_stats[tFOUL] = Convert.ToUInt16(myCell(5, 20));
            tst[id].pl_stats[tMINS] = Convert.ToUInt16(myCell(5, 21));

            tst[id].calcAvg();


            // Opponents
            tstopp[id].winloss[0] = Convert.ToByte(myCell(3, 2));
            tstopp[id].winloss[1] = Convert.ToByte(myCell(3, 3));
            tstopp[id].stats[tPF] = Convert.ToUInt16(myCell(3, 4));
            tstopp[id].stats[tPA] = Convert.ToUInt16(myCell(3, 5));

            parts = myCell(3, 7).Split('-');
            tstopp[id].stats[tFGM] = Convert.ToUInt16(parts[0]);
            tstopp[id].stats[tFGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 9).Split('-');
            tstopp[id].stats[tTPM] = Convert.ToUInt16(parts[0]);
            tstopp[id].stats[tTPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 11).Split('-');
            tstopp[id].stats[tFTM] = Convert.ToUInt16(parts[0]);
            tstopp[id].stats[tFTA] = Convert.ToUInt16(parts[1]);

            tstopp[id].stats[tOREB] = Convert.ToUInt16(myCell(3, 14));
            tstopp[id].stats[tDREB] = Convert.ToUInt16(myCell(3, 15));

            tstopp[id].stats[tAST] = Convert.ToUInt16(myCell(3, 16));
            tstopp[id].stats[tTO] = Convert.ToUInt16(myCell(3, 17));
            tstopp[id].stats[tSTL] = Convert.ToUInt16(myCell(3, 18));
            tstopp[id].stats[tBLK] = Convert.ToUInt16(myCell(3, 19));
            tstopp[id].stats[tFOUL] = Convert.ToUInt16(myCell(3, 20));
            tstopp[id].stats[tMINS] = Convert.ToUInt16(myCell(3, 21));

            tstopp[id].pl_winloss[0] = Convert.ToByte(myCell(5, 2));
            tstopp[id].pl_winloss[1] = Convert.ToByte(myCell(5, 3));
            tstopp[id].pl_stats[tPF] = Convert.ToUInt16(myCell(5, 4));
            tstopp[id].pl_stats[tPA] = Convert.ToUInt16(myCell(5, 5));

            parts = myCell(5, 7).Split('-');
            tstopp[id].pl_stats[tFGM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[tFGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 9).Split('-');
            tstopp[id].pl_stats[tTPM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[tTPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(5, 11).Split('-');
            tstopp[id].pl_stats[tFTM] = Convert.ToUInt16(parts[0]);
            tstopp[id].pl_stats[tFTA] = Convert.ToUInt16(parts[1]);

            tstopp[id].pl_stats[tOREB] = Convert.ToUInt16(myCell(5, 14));
            tstopp[id].pl_stats[tDREB] = Convert.ToUInt16(myCell(5, 15));

            tstopp[id].pl_stats[tAST] = Convert.ToUInt16(myCell(5, 16));
            tstopp[id].pl_stats[tTO] = Convert.ToUInt16(myCell(5, 17));
            tstopp[id].pl_stats[tSTL] = Convert.ToUInt16(myCell(5, 18));
            tstopp[id].pl_stats[tBLK] = Convert.ToUInt16(myCell(5, 19));
            tstopp[id].pl_stats[tFOUL] = Convert.ToUInt16(myCell(5, 20));
            tstopp[id].pl_stats[tMINS] = Convert.ToUInt16(myCell(5, 21));

            tstopp[id].calcAvg();

            var playersToUpdate = new Dictionary<int, PlayerStats>();

            foreach (PlayerStatsRow cur in psrList)
            {
                var ps = new PlayerStats(cur);
                playersToUpdate.Add(ps.ID, ps);
            }

            MainWindow.saveSeasonToDatabase(MainWindow.currentDB, tst, tstopp, playersToUpdate,
                                            curSeason, maxSeason);
            MainWindow.LoadDatabase(MainWindow.currentDB, ref tst, ref tstopp, ref pst, ref MainWindow.TeamOrder,
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

            int[][] rating = NSTHelper.calculateRankings(tst);
            if (rating.Length != 1)
            {
                string msg = NSTHelper.scoutReport(rating, id, cmbTeam.SelectedItem.ToString());
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
                                         curSeason);

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
                                      curSeason);

                    DataTable res2 = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);

                    q = String.Format("select * from GameResults " +
                                      "where (((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                                      "OR " +
                                      "((T1Name LIKE '{1}') AND (T2Name LIKE '{0}')) AND IsPlayoff LIKE 'True' AND SeasonNum = {2});",
                                      cmbTeam.SelectedItem,
                                      cmbOppTeam.SelectedItem,
                                      curSeason);

                    res2 = db.GetDataTable(q);
                    AddToTeamStatsFromSQLBoxScore(res2, ref ts, ref tsopp);
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
                dr["W%"] = String.Format("{0:F3}", ts.averages[tWp]);
                dr["Weff"] = String.Format("{0:F2}", ts.averages[tWeff]);
                dr["PF"] = String.Format("{0:F1}", ts.averages[tPPG]);
                dr["PA"] = String.Format("{0:F1}", ts.averages[tPAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.averages[tPD]);
                dr["FG"] = String.Format("{0:F3}", ts.averages[tFGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.averages[tFGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.averages[tTPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.averages[tTPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.averages[tFTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.averages[tFTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.averages[tRPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.averages[tORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.averages[tDRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.averages[tAPG]);
                dr["TO"] = String.Format("{0:F1}", ts.averages[tTPG]);
                dr["STL"] = String.Format("{0:F1}", ts.averages[tSPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.averages[tBPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.averages[tFPG]);
            }
            else
            {
                dr["Games"] = ts.getPlayoffGames();
                dr["Wins"] = ts.pl_winloss[0].ToString();
                dr["Losses"] = ts.pl_winloss[1].ToString();
                dr["W%"] = String.Format("{0:F3}", ts.pl_averages[tWp]);
                dr["Weff"] = String.Format("{0:F2}", ts.pl_averages[tWeff]);
                dr["PF"] = String.Format("{0:F1}", ts.pl_averages[tPPG]);
                dr["PA"] = String.Format("{0:F1}", ts.pl_averages[tPAPG]);
                dr["PD"] = String.Format("{0:F1}", ts.pl_averages[tPD]);
                dr["FG"] = String.Format("{0:F3}", ts.pl_averages[tFGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.pl_averages[tFGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.pl_averages[tTPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.pl_averages[tTPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.pl_averages[tFTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.pl_averages[tFTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.pl_averages[tRPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.pl_averages[tORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.pl_averages[tDRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.pl_averages[tAPG]);
                dr["TO"] = String.Format("{0:F1}", ts.pl_averages[tTPG]);
                dr["STL"] = String.Format("{0:F1}", ts.pl_averages[tSPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.pl_averages[tBPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.pl_averages[tFPG]);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbOppTeam.SelectedIndex = -1;
            cmbOppTeam.SelectedIndex = 1;
        }

        public static void AddToTeamStatsFromSQLBoxScore(DataTable res, ref TeamStats ts, ref TeamStats tsopp)
        {
            foreach (DataRow r in res.Rows)
            {
                AddToTeamStatsFromSQLBoxScore(r, ref ts, ref tsopp);
            }
        }

        public static void AddToTeamStatsFromSQLBoxScore(DataRow r, ref TeamStats ts, ref TeamStats tsopp)
        {
            bool playoffs = NSTHelper.getBoolean(r, "isPlayoff");
            if (!playoffs)
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(ts.name))
                {
                    if (t1pts > t2pts) ts.winloss[0]++;
                    else ts.winloss[1]++;
                    tsopp.stats[tMINS] = ts.stats[tMINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.stats[tPA] = ts.stats[tPF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.stats[tPF] = ts.stats[tPA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.stats[tFGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.stats[tFGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.stats[tTPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.stats[tTPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.stats[tFTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.stats[tFTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.stats[tDREB] += (ushort) (T1reb - T1oreb);
                    ts.stats[tOREB] += T1oreb;

                    ts.stats[tSTL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.stats[tTO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.stats[tBLK] += NSTHelper.getUShort(r, "T1BLK");
                    ts.stats[tAST] += NSTHelper.getUShort(r, "T1AST");
                    ts.stats[tFOUL] += NSTHelper.getUShort(r, "T1FOUL");

                    tsopp.stats[tFGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.stats[tFGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.stats[tTPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.stats[tTPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.stats[tFTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.stats[tFTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.stats[tDREB] += (ushort) (T2reb - T2oreb);
                    tsopp.stats[tOREB] += T2oreb;

                    tsopp.stats[tSTL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.stats[tTO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.stats[tBLK] += NSTHelper.getUShort(r, "T2BLK");
                    tsopp.stats[tAST] += NSTHelper.getUShort(r, "T2AST");
                    tsopp.stats[tFOUL] += NSTHelper.getUShort(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts) ts.winloss[0]++;
                    else ts.winloss[1]++;
                    tsopp.stats[tMINS] = ts.stats[tMINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.stats[tPA] = ts.stats[tPF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.stats[tPF] = ts.stats[tPA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.stats[tFGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.stats[tFGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.stats[tTPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.stats[tTPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.stats[tFTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.stats[tFTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.stats[tDREB] += (ushort) (T2reb - T2oreb);
                    ts.stats[tOREB] += T2oreb;

                    ts.stats[tSTL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.stats[tTO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.stats[tBLK] += NSTHelper.getUShort(r, "T2BLK");
                    ts.stats[tAST] += NSTHelper.getUShort(r, "T2AST");
                    ts.stats[tFOUL] += NSTHelper.getUShort(r, "T2FOUL");

                    tsopp.stats[tFGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.stats[tFGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.stats[tTPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.stats[tTPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.stats[tFTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.stats[tFTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.stats[tDREB] += (ushort) (T1reb - T1oreb);
                    tsopp.stats[tOREB] += T1oreb;

                    tsopp.stats[tSTL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.stats[tTO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.stats[tBLK] += NSTHelper.getUShort(r, "T1BLK");
                    tsopp.stats[tAST] += NSTHelper.getUShort(r, "T1AST");
                    tsopp.stats[tFOUL] += NSTHelper.getUShort(r, "T1FOUL");
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
                    tsopp.pl_stats[tMINS] = ts.pl_stats[tMINS] += Convert.ToUInt16(r["T1MINS"].ToString());
                    tsopp.pl_stats[tPA] = ts.pl_stats[tPF] += Convert.ToUInt16(r["T1PTS"].ToString());
                    tsopp.pl_stats[tPF] = ts.pl_stats[tPA] += Convert.ToUInt16(r["T2PTS"].ToString());

                    ts.pl_stats[tFGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    ts.pl_stats[tFGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    ts.pl_stats[tTPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    ts.pl_stats[tTPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    ts.pl_stats[tFTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    ts.pl_stats[tFTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    ts.pl_stats[tDREB] += (ushort) (T1reb - T1oreb);
                    ts.pl_stats[tOREB] += T1oreb;

                    ts.pl_stats[tSTL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.pl_stats[tTO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.pl_stats[tBLK] += NSTHelper.getUShort(r, "T1BLK");
                    ts.pl_stats[tAST] += NSTHelper.getUShort(r, "T1AST");
                    ts.pl_stats[tFOUL] += NSTHelper.getUShort(r, "T1FOUL");

                    tsopp.pl_stats[tFGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.pl_stats[tFGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.pl_stats[tTPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.pl_stats[tTPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.pl_stats[tFTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.pl_stats[tFTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.pl_stats[tDREB] += (ushort) (T2reb - T2oreb);
                    tsopp.pl_stats[tOREB] += T2oreb;

                    tsopp.pl_stats[tSTL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.pl_stats[tTO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.pl_stats[tBLK] += NSTHelper.getUShort(r, "T2BLK");
                    tsopp.pl_stats[tAST] += NSTHelper.getUShort(r, "T2AST");
                    tsopp.pl_stats[tFOUL] += NSTHelper.getUShort(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts) ts.pl_winloss[0]++;
                    else ts.pl_winloss[1]++;
                    tsopp.pl_stats[tMINS] = ts.pl_stats[tMINS] += Convert.ToUInt16(r["T2MINS"].ToString());
                    tsopp.pl_stats[tPA] = ts.pl_stats[tPF] += Convert.ToUInt16(r["T2PTS"].ToString());
                    tsopp.pl_stats[tPF] = ts.pl_stats[tPA] += Convert.ToUInt16(r["T1PTS"].ToString());

                    ts.pl_stats[tFGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    ts.pl_stats[tFGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    ts.pl_stats[tTPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    ts.pl_stats[tTPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    ts.pl_stats[tFTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    ts.pl_stats[tFTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    ts.pl_stats[tDREB] += (ushort) (T2reb - T2oreb);
                    ts.pl_stats[tOREB] += T2oreb;

                    ts.pl_stats[tSTL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.pl_stats[tTO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.pl_stats[tBLK] += NSTHelper.getUShort(r, "T2BLK");
                    ts.pl_stats[tAST] += NSTHelper.getUShort(r, "T2AST");
                    ts.pl_stats[tFOUL] += NSTHelper.getUShort(r, "T2FOUL");

                    tsopp.pl_stats[tFGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.pl_stats[tFGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.pl_stats[tTPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.pl_stats[tTPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.pl_stats[tFTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.pl_stats[tFTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.pl_stats[tDREB] += (ushort) (T1reb - T1oreb);
                    tsopp.pl_stats[tOREB] += T1oreb;

                    tsopp.pl_stats[tSTL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.pl_stats[tTO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.pl_stats[tBLK] += NSTHelper.getUShort(r, "T1BLK");
                    tsopp.pl_stats[tAST] += NSTHelper.getUShort(r, "T1AST");
                    tsopp.pl_stats[tFOUL] += NSTHelper.getUShort(r, "T1FOUL");
                }

                tsopp.pl_winloss[1] = ts.pl_winloss[0];
                tsopp.pl_winloss[0] = ts.pl_winloss[1];
            }

            ts.calcAvg();
            tsopp.calcAvg();
        }

        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppTeam_SelectionChanged(sender, null);
        }

        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppTeam_SelectionChanged(sender, null);
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
            curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
            MainWindow.LoadDatabase(MainWindow.currentDB, ref tst, ref tstopp, ref pst, ref MainWindow.TeamOrder,
                                          ref MainWindow.pt, ref MainWindow.bshist, _curSeason: curSeason);
            cmbTeam_SelectionChanged(null, null);
        }

        private void dgvPlayerStats_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvPlayerStats.SelectedCells.Count > 0)
            {
                var row = (PlayerStatsRow) dgvPlayerStats.SelectedItems[0];
                int playerID = row.ID;

                var pow = new playerOverviewW(curTeam, playerID);
                pow.ShowDialog();

                UpdatePlayerAndMetricStats();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.tst = tst;
            MainWindow.tstopp = tstopp;
            MainWindow.pst = pst;
        }
    }
}