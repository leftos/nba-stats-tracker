using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace NBA_2K12_Correct_Team_Stats
{
    /// <summary>
    /// Interaction logic for teamOverviewW.xaml
    /// </summary>
    public partial class teamOverviewW : Window
    {
        private TeamStats[] tst;
        private int[][] rankings;

        DataTable dt_hth;
        DataView dv_hth;

        public const int M = 0, PF = 1, PA = 2, FGM = 4, FGA = 5, TPM = 6, TPA = 7,
            FTM = 8, FTA = 9, OREB = 10, DREB = 11, STL = 12, TO = 13, BLK = 14, AST = 15,
            FOUL = 16;
        public const int PPG = 0, PAPG = 1, FGp = 2, FGeff = 3, TPp = 4, TPeff = 5,
            FTp = 6, FTeff = 7, RPG = 8, ORPG = 9, DRPG = 10, SPG = 11, BPG = 12,
            TPG = 13, APG = 14, FPG = 15, Wp = 16, Weff = 17, PD = 18;

        public teamOverviewW(TeamStats[] tst)
        {
            InitializeComponent();

            this.tst = tst;

            foreach (KeyValuePair<string,int> kvp in MainWindow.TeamOrder)
            {
                cmbTeam.Items.Add(kvp.Key);
                cmbOppTeam.Items.Add(kvp.Key);
            }

            rankings = StatsTracker.calculateRankings(tst);

            dtpEnd.SelectedDate = DateTime.Today;
            dtpStart.SelectedDate = DateTime.Today.AddMonths(-1);

            btnReloadStats.Visibility = Visibility.Hidden;

            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedIndex = 0;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == 0) cmbTeam.SelectedIndex = cmbTeam.Items.Count -1;
            else cmbTeam.SelectedIndex--;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count-1) cmbTeam.SelectedIndex = 0;
            else cmbTeam.SelectedIndex++;
        }

        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTeam.SelectedIndex == -1) return;
            }
            catch
            {
                return;
            }

            dgvBoxScores.DataContext = null;
            dgvTeamStats.DataContext = null;
            dgvHTHStats.DataContext = null;
            dgvHTHBoxScores.DataContext = null;

            int i = MainWindow.TeamOrder[cmbTeam.SelectedItem.ToString()];

            string curTeam = cmbTeam.SelectedItem.ToString();
            TeamStats ts = new TeamStats(curTeam);
            TeamStats tsopp = new TeamStats("Opponents");

            SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
            
            DataTable dt_ov = new DataTable();

            dt_ov.Columns.Add("Type");
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

            DataTable dt_bs = new DataTable();
            dt_bs.Columns.Add("Date");
            dt_bs.Columns.Add("Opponent");
            dt_bs.Columns.Add("Home-Away");
            dt_bs.Columns.Add("Result");
            dt_bs.Columns.Add("Score");
            dt_bs.Columns.Add("GameID");

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                ts = tst[i];

                DataTable res;
                String q = "select * from GameResults where ((T1Name LIKE '" + curTeam + "') OR (T2Name LIKE '"
                    + curTeam + "'));";
                res = db.GetDataTable(q);

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
                            ts.winloss[0]++;
                            bsr["Result"] = "W";
                        }
                        else
                        {
                            ts.winloss[1]++;
                            bsr["Result"] = "L";
                        }

                        bsr["Score"] = r["T1PTS"].ToString() + "-" + r["T2PTS"].ToString();
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
                            ts.winloss[0]++;
                            bsr["Result"] = "W";
                        }
                        else
                        {
                            ts.winloss[1]++;
                            bsr["Result"] = "L";
                        }

                        bsr["Score"] = r["T2PTS"].ToString() + "-" + r["T1PTS"].ToString();
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
                        + curTeam + "')) AND ((Date >= '" + db.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault())
                        + "') AND (Date <= '" + db.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()) + "'));";
                    res = db.GetDataTable(q);

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
                                ts.winloss[0]++;
                                bsr["Result"] = "W";
                            }
                            else
                            {
                                ts.winloss[1]++;
                                bsr["Result"] = "L";
                            }

                            bsr["Score"] = r["T1PTS"].ToString() + "-" + r["T2PTS"].ToString();
                            bsr["GameID"] = r["GameID"].ToString();

                            dt_bs.Rows.Add(bsr);

                            tsopp.stats[PA] = ts.stats[PF] += Convert.ToUInt16(r["T1PTS"].ToString());
                            tsopp.stats[PF] = ts.stats[PA] += Convert.ToUInt16(r["T2PTS"].ToString());

                            ts.stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                            ts.stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                            ts.stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                            ts.stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                            ts.stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                            ts.stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                            UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                            UInt16 T1oreb =  Convert.ToUInt16(r["T1OREB"].ToString());
                            ts.stats[DREB] += (ushort)((int)T1reb - (int)T1oreb);
                            ts.stats[OREB] += T1oreb;

                            ts.stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                            ts.stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                            ts.stats[BLK] += getUshort(r, "T1BLK");
                            ts.stats[AST] += getUshort(r, "T1AST");
                            ts.stats[FOUL] += getUshort(r, "T1FOUL");

                            tsopp.stats[FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                            tsopp.stats[FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                            tsopp.stats[TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                            tsopp.stats[TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                            tsopp.stats[FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                            tsopp.stats[FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                            UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                            UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                            tsopp.stats[DREB] += (ushort)((int)T2reb - (int)T2oreb);
                            tsopp.stats[OREB] += T2oreb;

                            tsopp.stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                            tsopp.stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                            tsopp.stats[BLK] += getUshort(r, "T2BLK");
                            tsopp.stats[AST] += getUshort(r, "T2AST");
                            tsopp.stats[FOUL] += getUshort(r, "T2FOUL");
                        }
                        else
                        {
                            DataRow bsr = dt_bs.NewRow();
                            bsr["Date"] = r["Date"].ToString().Split(' ')[0];
                            bsr["Opponent"] = r["T1Name"].ToString();
                            bsr["Home-Away"] = "Home";

                            if (t2pts > t1pts)
                            {
                                ts.winloss[0]++;
                                bsr["Result"] = "W";
                            }
                            else
                            {
                                ts.winloss[1]++;
                                bsr["Result"] = "L";
                            }

                            bsr["Score"] = r["T2PTS"].ToString() + "-" + r["T1PTS"].ToString();
                            bsr["GameID"] = r["GameID"].ToString();

                            dt_bs.Rows.Add(bsr);

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
                            ts.stats[DREB] += (ushort)((int)T2reb - (int)T2oreb);
                            ts.stats[OREB] += T2oreb;

                            ts.stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                            ts.stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                            ts.stats[BLK] += getUshort(r, "T2BLK");
                            ts.stats[AST] += getUshort(r, "T2AST");
                            ts.stats[FOUL] += getUshort(r, "T2FOUL");

                            tsopp.stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                            tsopp.stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                            tsopp.stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                            tsopp.stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                            tsopp.stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                            tsopp.stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                            UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                            UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                            tsopp.stats[DREB] += (ushort)((int)T1reb - (int)T1oreb);
                            tsopp.stats[OREB] += T1oreb;

                            tsopp.stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                            tsopp.stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                            tsopp.stats[BLK] += getUshort(r, "T1BLK");
                            tsopp.stats[AST] += getUshort(r, "T1AST");
                            tsopp.stats[FOUL] += getUshort(r, "T1FOUL");
                        }
                        tsopp.winloss[1] = ts.winloss[0];
                        tsopp.winloss[0] = ts.winloss[1];
                    }
                }
            }            

            DataRow dr = dt_ov.NewRow();

            dr["Type"] = "Stats";
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
            dr["AST"] = String.Format("{0:F1}", ts.averages[AST]);
            dr["TO"] = String.Format("{0:F1}", ts.averages[TO]);
            dr["STL"] = String.Format("{0:F1}", ts.averages[STL]);
            dr["BLK"] = String.Format("{0:F1}", ts.averages[BLK]);
            dr["FOUL"] = String.Format("{0:F1}", ts.averages[FOUL]);

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

            DataView dv_ov = new DataView(dt_ov);
            dv_ov.AllowNew = false;

            dgvTeamStats.DataContext = dv_ov;

            DataView dv_bs = new DataView(dt_bs);
            dv_bs.AllowEdit = false;
            dv_bs.AllowNew = false;

            dgvBoxScores.DataContext = dv_bs;

            dv_hth = new DataView(dt_hth);
            dv_hth.AllowNew = false;
            dv_hth.AllowEdit = false;

            dgvHTHStats.DataContext = dv_hth;

            Title = cmbTeam.SelectedItem.ToString() + " Team Overview - " + ts.getGames() + " games played";

            cmbOppTeam_SelectionChanged(sender, e);
        }

        UInt16 getUshort(DataRow r, string ColumnName)
        {
            return Convert.ToUInt16(r[ColumnName].ToString());
        }

        private void btnShowAvg_Click(object sender, RoutedEventArgs e)
        {
            string msg = StatsTracker.averagesAndRankings(cmbTeam.SelectedItem.ToString(), tst, MainWindow.TeamOrder);
            if (msg != "")
            {
                copyableW cw = new copyableW(msg, cmbTeam.SelectedItem.ToString(), TextAlignment.Center);
                cw.ShowDialog();
            }
        }

        private void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show("You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores"
                    + ") or edit the box-scores themselves.", "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int id = MainWindow.TeamOrder[cmbTeam.SelectedItem.ToString()];
            tst[id].winloss[0] = Convert.ToByte(myCell(0, 1));
            tst[id].winloss[1] = Convert.ToByte(myCell(0, 2));
            tst[id].stats[PF] = Convert.ToUInt16(myCell(0, 3));
            tst[id].stats[PA] = Convert.ToUInt16(myCell(0, 4));

            string[] parts = myCell(0, 6).Split('-');
            tst[id].stats[FGM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 8).Split('-');
            tst[id].stats[TPM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 10).Split('-');
            tst[id].stats[FTM] = Convert.ToUInt16(parts[0]);
            tst[id].stats[FTA] = Convert.ToUInt16(parts[1]);

            tst[id].stats[OREB] = Convert.ToUInt16(myCell(0, 13));
            tst[id].stats[DREB] = Convert.ToUInt16(myCell(0, 14));

            tst[id].stats[AST] = Convert.ToUInt16(myCell(0, 15));
            tst[id].stats[TO] = Convert.ToUInt16(myCell(0, 16));
            tst[id].stats[STL] = Convert.ToUInt16(myCell(0, 17));
            tst[id].stats[BLK] = Convert.ToUInt16(myCell(0, 18));
            tst[id].stats[FOUL] = Convert.ToUInt16(myCell(0, 19));

            tst[id].calcAvg();
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
                copyableW cw = new copyableW(msg, "Scouting Report", TextAlignment.Left);
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
            cmbTeam_SelectionChanged(sender, null);
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbTeam_SelectionChanged(sender, null);
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            cmbTeam_SelectionChanged(sender, null);
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            cmbTeam_SelectionChanged(sender, null);
        }

        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                DataRowView row = (DataRowView)dgvBoxScores.SelectedItems[0];
                int gameid = Convert.ToInt32(row["GameID"].ToString());

                int i = 0;

                foreach (BoxScoreEntry bse in MainWindow.bshist)
                {
                    if (bse.bs.id == gameid)
                    {
                        MainWindow.bs = new BoxScore();

                        boxScoreW bsw = new boxScoreW(boxScoreW.Mode.View, i);
                        bsw.ShowDialog();

                        if (MainWindow.bs.bshistid != -1)
                        {
                            if (MainWindow.bs.done)
                            {
                                MainWindow.bshist[MainWindow.bs.bshistid].bs = MainWindow.bs;
                                MainWindow.bshist[MainWindow.bs.bshistid].mustUpdate = true;

                                MessageBox.Show("It is recommended to save and reload the Team Stats file for changes to take effect.");
                                //MainWindow.updateStatus("One or more Box Scores have been updated. Save the Team Stats file before continuing.");
                            }
                        }
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

            DataTable dt_hth_bs = new DataTable();
            dt_hth_bs.Columns.Add("Date");
            dt_hth_bs.Columns.Add("Home-Away");
            dt_hth_bs.Columns.Add("Result");
            dt_hth_bs.Columns.Add("Score");
            dt_hth_bs.Columns.Add("GameID");

            TeamStats ts = new TeamStats(curTeam);
            TeamStats tsopp = new TeamStats(curOpp);

            SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);

            DataTable res = new DataTable();

            if (dt_hth.Rows.Count > 1) dt_hth.Rows.RemoveAt(dt_hth.Rows.Count - 1);

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                string q = String.Format("select * from GameResults where (((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                    "OR ((T1Name LIKE '{1}') AND (T2Name LIKE '{0}')))",
                    cmbTeam.SelectedItem.ToString(),
                    cmbOppTeam.SelectedItem.ToString());

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
                    CalculateTeamStatsFromSQLDataTable(res, ref ts, ref tsopp);
                }
            }
            else
            {
                string q = String.Format("select * from GameResults where ((((T1Name LIKE '{0}') AND (T2Name LIKE '{1}')) " +
                        "OR ((T1Name LIKE '{1}') AND (T2Name LIKE '{0}'))) AND ((Date >= '{2}') AND (Date <= '{3}')))",
                        cmbTeam.SelectedItem.ToString(),
                        cmbOppTeam.SelectedItem.ToString(),
                        db.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault()),
                        db.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()));
                res = db.GetDataTable(q);

                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    string q2 = String.Format("select * from GameResults where (((T1Name LIKE '{0}') OR (T2Name LIKE '{1}') " +
                        "OR (T1Name LIKE '{1}') OR (T2Name LIKE '{0}')) AND ((Date >= '{2}') AND (Date <= '{3}')))",
                        cmbTeam.SelectedItem.ToString(),
                        cmbOppTeam.SelectedItem.ToString(),
                        db.ConvertDateTimeToSQLite(dtpStart.SelectedDate.GetValueOrDefault()),
                        db.ConvertDateTimeToSQLite(dtpEnd.SelectedDate.GetValueOrDefault()));
                    DataTable res2 = db.GetDataTable(q);
                    CalculateTeamStatsFromSQLDataTable(res2, ref ts, ref tsopp);
                }
                else
                {                    
                    CalculateTeamStatsFromSQLDataTable(res, ref ts, ref tsopp);
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

                    bsr["Score"] = r["T1PTS"].ToString() + "-" + r["T2PTS"].ToString();
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

                    bsr["Score"] = r["T2PTS"].ToString() + "-" + r["T1PTS"].ToString();
                    bsr["GameID"] = r["GameID"].ToString();

                    dt_hth_bs.Rows.Add(bsr);
                }
            }

            dt_hth.Clear();

            DataRow dr = dt_hth.NewRow();

            dr = dt_hth.NewRow();

            dr["Type"] = "Averages";
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
            dr["AST"] = String.Format("{0:F1}", ts.averages[AST]);
            dr["TO"] = String.Format("{0:F1}", ts.averages[TO]);
            dr["STL"] = String.Format("{0:F1}", ts.averages[STL]);
            dr["BLK"] = String.Format("{0:F1}", ts.averages[BLK]);
            dr["FOUL"] = String.Format("{0:F1}", ts.averages[FOUL]);

            dt_hth.Rows.Add(dr);

            dr = dt_hth.NewRow();

            dr["Type"] = "Opp Avg";
            dr["Wins"] = tsopp.winloss[0].ToString();
            dr["Losses"] = tsopp.winloss[1].ToString();
            dr["W%"] = String.Format("{0:F3}", tsopp.averages[Wp]);
            dr["Weff"] = String.Format("{0:F2}", tsopp.averages[Weff]);
            dr["PF"] = String.Format("{0:F1}", tsopp.averages[PPG]);
            dr["PA"] = String.Format("{0:F1}", tsopp.averages[PAPG]);
            dr["PD"] = String.Format("{0:F1}", tsopp.averages[PD]);
            dr["FG"] = String.Format("{0:F3}", tsopp.averages[FGp]);
            dr["FGeff"] = String.Format("{0:F2}", tsopp.averages[FGeff]);
            dr["3PT"] = String.Format("{0:F3}", tsopp.averages[TPp]);
            dr["3Peff"] = String.Format("{0:F2}", tsopp.averages[TPeff]);
            dr["FT"] = String.Format("{0:F3}", tsopp.averages[FTp]);
            dr["FTeff"] = String.Format("{0:F2}", tsopp.averages[FTeff]);
            dr["REB"] = String.Format("{0:F1}", tsopp.averages[RPG]);
            dr["OREB"] = String.Format("{0:F1}", tsopp.averages[ORPG]);
            dr["DREB"] = String.Format("{0:F1}", tsopp.averages[DRPG]);
            dr["AST"] = String.Format("{0:F1}", tsopp.averages[AST]);
            dr["TO"] = String.Format("{0:F1}", tsopp.averages[TO]);
            dr["STL"] = String.Format("{0:F1}", tsopp.averages[STL]);
            dr["BLK"] = String.Format("{0:F1}", tsopp.averages[BLK]);
            dr["FOUL"] = String.Format("{0:F1}", tsopp.averages[FOUL]);

            dt_hth.Rows.Add(dr);

            dv_hth = new DataView(dt_hth);
            dv_hth.AllowNew = false;
            dv_hth.AllowEdit = false;

            dgvHTHStats.DataContext = dv_hth;

            DataView dv_hth_bs = new DataView(dt_hth_bs);
            dv_hth_bs.AllowNew = false;
            dv_hth_bs.AllowEdit = false;

            dgvHTHBoxScores.DataContext = dv_hth_bs;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbOppTeam.SelectedIndex = -1;
            cmbOppTeam.SelectedIndex = 1;
        }

        private void CalculateTeamStatsFromSQLDataTable(DataTable res, ref TeamStats ts, ref TeamStats tsopp)
        {
            foreach (DataRow r in res.Rows)
            {
                int t1pts = Convert.ToInt32(r["T1PTS"].ToString());
                int t2pts = Convert.ToInt32(r["T2PTS"].ToString());
                if (r["T1Name"].ToString().Equals(cmbTeam.SelectedItem.ToString()))
                {
                    if (t1pts > t2pts) ts.winloss[0]++; else ts.winloss[1]++;
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
                    ts.stats[DREB] += (ushort)((int)T1reb - (int)T1oreb);
                    ts.stats[OREB] += T1oreb;

                    ts.stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    ts.stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    ts.stats[BLK] += getUshort(r, "T1BLK");
                    ts.stats[AST] += getUshort(r, "T1AST");
                    ts.stats[FOUL] += getUshort(r, "T1FOUL");

                    tsopp.stats[FGM] += Convert.ToUInt16(r["T2FGM"].ToString());
                    tsopp.stats[FGA] += Convert.ToUInt16(r["T2FGA"].ToString());
                    tsopp.stats[TPM] += Convert.ToUInt16(r["T23PM"].ToString());
                    tsopp.stats[TPA] += Convert.ToUInt16(r["T23PA"].ToString());
                    tsopp.stats[FTM] += Convert.ToUInt16(r["T2FTM"].ToString());
                    tsopp.stats[FTA] += Convert.ToUInt16(r["T2FTA"].ToString());

                    UInt16 T2reb = Convert.ToUInt16(r["T2REB"].ToString());
                    UInt16 T2oreb = Convert.ToUInt16(r["T2OREB"].ToString());
                    tsopp.stats[DREB] += (ushort)((int)T2reb - (int)T2oreb);
                    tsopp.stats[OREB] += T2oreb;

                    tsopp.stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    tsopp.stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    tsopp.stats[BLK] += getUshort(r, "T2BLK");
                    tsopp.stats[AST] += getUshort(r, "T2AST");
                    tsopp.stats[FOUL] += getUshort(r, "T2FOUL");
                }
                else
                {
                    if (t2pts > t1pts) ts.winloss[0]++; else ts.winloss[1]++;
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
                    ts.stats[DREB] += (ushort)((int)T2reb - (int)T2oreb);
                    ts.stats[OREB] += T2oreb;

                    ts.stats[STL] += Convert.ToUInt16(r["T2STL"].ToString());
                    ts.stats[TO] += Convert.ToUInt16(r["T2TOS"].ToString());
                    ts.stats[BLK] += getUshort(r, "T2BLK");
                    ts.stats[AST] += getUshort(r, "T2AST");
                    ts.stats[FOUL] += getUshort(r, "T2FOUL");

                    tsopp.stats[FGM] += Convert.ToUInt16(r["T1FGM"].ToString());
                    tsopp.stats[FGA] += Convert.ToUInt16(r["T1FGA"].ToString());
                    tsopp.stats[TPM] += Convert.ToUInt16(r["T13PM"].ToString());
                    tsopp.stats[TPA] += Convert.ToUInt16(r["T13PA"].ToString());
                    tsopp.stats[FTM] += Convert.ToUInt16(r["T1FTM"].ToString());
                    tsopp.stats[FTA] += Convert.ToUInt16(r["T1FTA"].ToString());

                    UInt16 T1reb = Convert.ToUInt16(r["T1REB"].ToString());
                    UInt16 T1oreb = Convert.ToUInt16(r["T1OREB"].ToString());
                    tsopp.stats[DREB] += (ushort)((int)T1reb - (int)T1oreb);
                    tsopp.stats[OREB] += T1oreb;

                    tsopp.stats[STL] += Convert.ToUInt16(r["T1STL"].ToString());
                    tsopp.stats[TO] += Convert.ToUInt16(r["T1TOS"].ToString());
                    tsopp.stats[BLK] += getUshort(r, "T1BLK");
                    tsopp.stats[AST] += getUshort(r, "T1AST");
                    tsopp.stats[FOUL] += getUshort(r, "T1FOUL");
                }
                tsopp.winloss[1] = ts.winloss[0];
                tsopp.winloss[0] = ts.winloss[1];

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

        private void dgvHTHBoxScores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                DataRowView row;
                try
                {
                   row = (DataRowView)dgvHTHBoxScores.SelectedItems[0];
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

                        boxScoreW bsw = new boxScoreW(boxScoreW.Mode.View, i);
                        bsw.ShowDialog();

                        if (MainWindow.bs.bshistid != -1)
                        {
                            if (MainWindow.bs.done)
                            {
                                MainWindow.bshist[MainWindow.bs.bshistid].bs = MainWindow.bs;
                                MainWindow.bshist[MainWindow.bs.bshistid].mustUpdate = true;

                                MessageBox.Show("It is recommended to save and reload the Team Stats file for changes to take effect.");
                                //MainWindow.updateStatus("One or more Box Scores have been updated. Save the Team Stats file before continuing.");
                            }
                        }
                        break;
                    }
                    i++;
                }
            }
        }
    }
}
