using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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

namespace NBA_Stats_Tracker
{
    /// <summary>
    /// Interaction logic for playerOverviewW.xaml
    /// </summary>
    public partial class playerOverviewW : Window
    {
        private SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);

        private ObservableCollection<KeyValuePair<int, string>> _playersList =
            new ObservableCollection<KeyValuePair<int, string>>();

        private ObservableCollection<KeyValuePair<int, string>> oppPlayersList =
            new ObservableCollection<KeyValuePair<int, string>>();
 
        private ObservableCollection<PlayerBoxScore> curPBS;
        private List<PlayerBoxScore> hthOwnPBS;
        private List<PlayerBoxScore> hthOppPBS;
        private List<PlayerBoxScore> hthAllPBS; 
        private ObservableCollection<PlayerStatsRow> splitPSRs;
        private PlayerStats psBetween;
        private PlayerStatsRow psr;
        private DataTable dt_ov;
        private int curSeason = MainWindow.curSeason;
        private int maxSeason = MainWindow.getMaxSeason(MainWindow.currentDB);
        private string playersT = "Players";
        private Dictionary<int, PlayerStats> playersActive;
        private Dictionary<int, PlayerStats> playersSamePosition;
        private Dictionary<int, PlayerStats> playersSameTeam;
        private List<string> Teams;
        private PlayerRankings rankingsActive;
        private PlayerRankings rankingsTeam;
        private PlayerRankings rankingsPosition;
        private int SelectedPlayerID = -1;
        public static string askedTeam;
        private SortedDictionary<string, int> teamOrder = MainWindow.TeamOrder; 

        public const int pGP = 0,
                         pGS = 1,
                         pMINS = 2,
                         pPTS = 3,
                         pDREB = 4,
                         pOREB = 5,
                         pAST = 6,
                         pSTL = 7,
                         pBLK = 8,
                         pTO = 9,
                         pFOUL = 10,
                         pFGM = 11,
                         pFGA = 12,
                         pTPM = 13,
                         pTPA = 14,
                         pFTM = 15,
                         pFTA = 16;

        public const int pMPG = 0,
                         pPPG = 1,
                         pDRPG = 2,
                         pORPG = 3,
                         pAPG = 4,
                         pSPG = 5,
                         pBPG = 6,
                         pTPG = 7,
                         pFPG = 8,
                         pFGp = 9,
                         pFGeff = 10,
                         pTPp = 11,
                         pTPeff = 12,
                         pFTp = 13,
                         pFTeff = 14,
                         pRPG = 15;

        public ObservableCollection<KeyValuePair<int,string>> PlayersList
        {
            get { return _playersList; }
            set 
            { 
                _playersList = value;
                OnPropertyChanged("PlayersList");
            }
        }

        private string _selectedPlayer;
        public string SelectedPlayer
        {
            get { return _selectedPlayer; }
            set { _selectedPlayer = value;
                OnPropertyChanged("SelectedPlayer");
            }
        }

        public playerOverviewW()
        {
            InitializeComponent();

            prepareWindow();
        }

        public playerOverviewW(string team, int playerID) : this()
        {
            cmbTeam.SelectedItem = team;
            cmbPlayer.SelectedValue = playerID.ToString();
        }

        private void prepareWindow()
        {
            DataContext = this;

            PopulateSeasonCombo();

            var Positions = new List<string>() {"PG", "SG", "SF", "PF", "C"};
            var Positions2 = new List<string>() { " ", "PG", "SG", "SF", "PF", "C" };
            cmbPosition1.ItemsSource = Positions;
            cmbPosition2.ItemsSource = Positions2;

            Teams = new List<string>();
            foreach (var kvp in teamOrder)
            {
                Teams.Add(kvp.Key);
            }
            Teams.Add("- Inactive -");

            cmbTeam.ItemsSource = Teams;
            cmbOppTeam.ItemsSource = Teams;

            dt_ov = new DataTable();
            dt_ov.Columns.Add("Type");
            dt_ov.Columns.Add("GP");
            dt_ov.Columns.Add("GS");
            dt_ov.Columns.Add("MINS");
            dt_ov.Columns.Add("PTS");
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

            dtpStart.SelectedDate = DateTime.Now.AddMonths(-1).AddDays(1);
            dtpEnd.SelectedDate = DateTime.Now;

            GetActivePlayers();
        }

        private void GetActivePlayers()
        {
            playersActive = new Dictionary<int, PlayerStats>();

            string q = "select * from " + playersT + " where isActive LIKE 'True'";
            DataTable res = db.GetDataTable(q);
            foreach (DataRow r in res.Rows)
            {
                PlayerStats ps = new PlayerStats(r);
                playersActive.Add(ps.ID, ps);
            }

            rankingsActive = new PlayerRankings(playersActive);
        }

        private void PopulateSeasonCombo()
        {
            for (int i = maxSeason; i > 0; i--)
            {
                cmbSeasonNum.Items.Add(i.ToString());
            }

            cmbSeasonNum.SelectedItem = curSeason.ToString();
        }

        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgvOverviewStats.DataContext = null;
            grdOverview.DataContext = null;
            cmbPosition1.SelectedIndex = -1;
            cmbPosition2.SelectedIndex = -1;

            if (cmbTeam.SelectedIndex == -1) return;

            cmbPlayer.ItemsSource = null;

            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            string q;
            if (cmbTeam.SelectedItem.ToString() != "- Inactive -")
            {
                q = "select * from " + playersT + " where TeamFin LIKE '" + cmbTeam.SelectedItem.ToString() + "' AND isActive LIKE 'True'";
            }
            else
            {
                q = "select * from " + playersT + " where isActive LIKE 'False'";
            }
            var res = db.GetDataTable(q);

            playersSameTeam = new Dictionary<int, PlayerStats>();

            foreach (DataRow r in res.Rows)
            {
                PlayersList.Add(new KeyValuePair<int, string>(StatsTracker.getInt(r, "ID"),
                                StatsTracker.getString(r, "FirstName") + " " + StatsTracker.getString(r, "LastName") + 
                                " (" + StatsTracker.getString(r, "Position1") + ")"));
                PlayerStats ps = new PlayerStats(r);
                playersSameTeam.Add(ps.ID, ps);
            }
            rankingsTeam = new PlayerRankings(playersSameTeam);

            cmbPlayer.ItemsSource = PlayersList;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void cmbPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == -1) return;

            SelectedPlayerID =
                ((System.Collections.Generic.KeyValuePair<int, string>)
                 (((System.Windows.Controls.Primitives.Selector) (cmbPlayer)).SelectedItem)).Key;

            string q = "select * from " + playersT + " where ID = " + SelectedPlayerID.ToString();

            var res = db.GetDataTable(q);

            if (res.Rows.Count == 0) // Player not found in this year's database
            {
                cmbTeam_SelectionChanged(null, null); // Reload this team's players
                return;
            }

            psr = new PlayerStatsRow(new PlayerStats(res.Rows[0]));

            UpdateOverviewAndBoxScores();

            UpdateSplitStats();

            UpdateYearlyReport();

            if (tbcPlayerOverview.SelectedItem == tabHTH)
            {
                cmbOppPlayer_SelectionChanged(null, null);
            }
        }

        private void UpdateYearlyReport()
        {
            for (int i = 1; i <= maxSeason; i++)
            {
                string pT = "Players";
                if (i != maxSeason) pT += "S" + i;

                string q = "select * from " + pT + " where ID = " + SelectedPlayerID;
                DataTable res = db.GetDataTable(q);

                List<PlayerStatsRow> psrList = new List<PlayerStatsRow>();
                if (res.Rows.Count == 1)
                {
                    PlayerStatsRow psr = new PlayerStatsRow(new PlayerStats(res.Rows[0]), "Season " + i);
                    psrList.Add(psr);
                }

                dgvYearly.ItemsSource = psrList;
            }
        }

        private void UpdateOverviewAndBoxScores()
        {
            TeamStats ts = new TeamStats("Team"),
                      tsopp = new TeamStats("Opponents");

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                grdOverview.DataContext = psr;

                string q = "select * from " + playersT + " where Position1 LIKE '" + psr.Position1 +
                    "' AND isActive LIKE 'True'";
                var res = db.GetDataTable(q);

                playersSamePosition = new Dictionary<int, PlayerStats>();

                foreach (DataRow r in res.Rows)
                {
                    PlayerStats ps = new PlayerStats(r);
                    playersSamePosition.Add(ps.ID, ps);
                }
                rankingsPosition = new PlayerRankings(playersSamePosition);

                q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID) "
                    + "where PlayerID = " + SelectedPlayerID.ToString() +
                    " AND SeasonNum = " + curSeason
                    + " ORDER BY Date DESC";
                res = db.GetDataTable(q);

                curPBS = new ObservableCollection<PlayerBoxScore>();
                foreach (DataRow r in res.Rows)
                {
                    PlayerBoxScore pbs = new PlayerBoxScore(r);
                    curPBS.Add(pbs);
                }
            }
            else
            {
                string q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID) "
                + "where PlayerID = " + SelectedPlayerID.ToString();
                q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                          dtpEnd.SelectedDate.GetValueOrDefault());
                q += " ORDER BY Date DESC";
                var res = db.GetDataTable(q);

                psBetween =
                    new PlayerStats(new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1,
                                               psr.Position2));

                curPBS = new ObservableCollection<PlayerBoxScore>();

                teamOverviewW.AddToTeamStatsFromSQLBoxScore(res, ref ts, ref tsopp);
                
                foreach (DataRow r in res.Rows)
                {
                    PlayerBoxScore pbs = new PlayerBoxScore(r);
                    curPBS.Add(pbs);

                    psBetween.AddBoxScore(pbs);
                }

                psr = new PlayerStatsRow(psBetween);
            }

            cmbPosition1.SelectedItem = psr.Position1;
            cmbPosition2.SelectedItem = psr.Position2;

            dt_ov.Clear();

            DataRow dr = dt_ov.NewRow();

            dr["Type"] = "Stats";
            dr["GP"] = psr.GP.ToString();
            dr["GS"] = psr.GS.ToString();
            dr["MINS"] = psr.MINS.ToString();
            dr["PTS"] = psr.PTS.ToString();
            dr["FG"] = psr.FGM.ToString() + "-" + psr.FGA.ToString();
            dr["3PT"] = psr.TPM.ToString() + "-" + psr.TPA.ToString();
            dr["FT"] = psr.FTM.ToString() + "-" + psr.FTA.ToString();
            dr["REB"] = (psr.DREB + psr.OREB).ToString();
            dr["OREB"] = psr.OREB.ToString();
            dr["DREB"] = psr.DREB.ToString();
            dr["AST"] = psr.AST.ToString();
            dr["TO"] = psr.TOS.ToString();
            dr["STL"] = psr.STL.ToString();
            dr["BLK"] = psr.BLK.ToString();
            dr["FOUL"] = psr.FOUL.ToString();

            dt_ov.Rows.Add(dr);

            dr = dt_ov.NewRow();

            dr["Type"] = "Averages";
            dr["MINS"] = String.Format("{0:F1}", psr.MPG);
            dr["PTS"] = String.Format("{0:F1}", psr.PPG);
            dr["FG"] = String.Format("{0:F3}", psr.FGp);
            dr["FGeff"] = String.Format("{0:F2}", psr.FGeff);
            dr["3PT"] = String.Format("{0:F3}", psr.TPp);
            dr["3Peff"] = String.Format("{0:F2}", psr.TPeff);
            dr["FT"] = String.Format("{0:F3}", psr.FTp);
            dr["FTeff"] = String.Format("{0:F2}", psr.FTeff);
            dr["REB"] = String.Format("{0:F1}", psr.RPG);
            dr["OREB"] = String.Format("{0:F1}", psr.ORPG);
            dr["DREB"] = String.Format("{0:F1}", psr.DRPG);
            dr["AST"] = String.Format("{0:F1}", psr.APG);
            dr["TO"] = String.Format("{0:F1}", psr.TPG);
            dr["STL"] = String.Format("{0:F1}", psr.SPG);
            dr["BLK"] = String.Format("{0:F1}", psr.BPG);
            dr["FOUL"] = String.Format("{0:F1}", psr.FPG);

            dt_ov.Rows.Add(dr);

            #region Rankings

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                if (psr.isActive)
                {
                    int id = Convert.ToInt32(SelectedPlayerID);

                    dr = dt_ov.NewRow();

                    dr["Type"] = "Rankings";
                    dr["MINS"] = String.Format("{0}", rankingsActive.list[id][pMPG]);
                    dr["PTS"] = String.Format("{0}", rankingsActive.list[id][pPPG]);
                    dr["FG"] = String.Format("{0}", rankingsActive.list[id][pFGp]);
                    dr["FGeff"] = String.Format("{0}", rankingsActive.list[id][pFGeff]);
                    dr["3PT"] = String.Format("{0}", rankingsActive.list[id][pTPp]);
                    dr["3Peff"] = String.Format("{0}", rankingsActive.list[id][pTPeff]);
                    dr["FT"] = String.Format("{0}", rankingsActive.list[id][pFTp]);
                    dr["FTeff"] = String.Format("{0}", rankingsActive.list[id][pFTeff]);
                    dr["REB"] = String.Format("{0}", rankingsActive.list[id][pRPG]);
                    dr["OREB"] = String.Format("{0}", rankingsActive.list[id][pORPG]);
                    dr["DREB"] = String.Format("{0}", rankingsActive.list[id][pDRPG]);
                    dr["AST"] = String.Format("{0}", rankingsActive.list[id][pAPG]);
                    dr["TO"] = String.Format("{0}", rankingsActive.list[id][pTPG]);
                    dr["STL"] = String.Format("{0}", rankingsActive.list[id][pSPG]);
                    dr["BLK"] = String.Format("{0}", rankingsActive.list[id][pBPG]);
                    dr["FOUL"] = String.Format("{0}", rankingsActive.list[id][pFPG]);

                    dt_ov.Rows.Add(dr);

                    dr = dt_ov.NewRow();

                    dr["Type"] = "In-team Rankings";
                    dr["MINS"] = String.Format("{0}", rankingsTeam.list[id][pMPG]);
                    dr["PTS"] = String.Format("{0}", rankingsTeam.list[id][pPPG]);
                    dr["FG"] = String.Format("{0}", rankingsTeam.list[id][pFGp]);
                    dr["FGeff"] = String.Format("{0}", rankingsTeam.list[id][pFGeff]);
                    dr["3PT"] = String.Format("{0}", rankingsTeam.list[id][pTPp]);
                    dr["3Peff"] = String.Format("{0}", rankingsTeam.list[id][pTPeff]);
                    dr["FT"] = String.Format("{0}", rankingsTeam.list[id][pFTp]);
                    dr["FTeff"] = String.Format("{0}", rankingsTeam.list[id][pFTeff]);
                    dr["REB"] = String.Format("{0}", rankingsTeam.list[id][pRPG]);
                    dr["OREB"] = String.Format("{0}", rankingsTeam.list[id][pORPG]);
                    dr["DREB"] = String.Format("{0}", rankingsTeam.list[id][pDRPG]);
                    dr["AST"] = String.Format("{0}", rankingsTeam.list[id][pAPG]);
                    dr["TO"] = String.Format("{0}", rankingsTeam.list[id][pTPG]);
                    dr["STL"] = String.Format("{0}", rankingsTeam.list[id][pSPG]);
                    dr["BLK"] = String.Format("{0}", rankingsTeam.list[id][pBPG]);
                    dr["FOUL"] = String.Format("{0}", rankingsTeam.list[id][pFPG]);

                    dt_ov.Rows.Add(dr);

                    dr = dt_ov.NewRow();

                    dr["Type"] = "Position Rankings";
                    dr["MINS"] = String.Format("{0}", rankingsPosition.list[id][pMPG]);
                    dr["PTS"] = String.Format("{0}", rankingsPosition.list[id][pPPG]);
                    dr["FG"] = String.Format("{0}", rankingsPosition.list[id][pFGp]);
                    dr["FGeff"] = String.Format("{0}", rankingsPosition.list[id][pFGeff]);
                    dr["3PT"] = String.Format("{0}", rankingsPosition.list[id][pTPp]);
                    dr["3Peff"] = String.Format("{0}", rankingsPosition.list[id][pTPeff]);
                    dr["FT"] = String.Format("{0}", rankingsPosition.list[id][pFTp]);
                    dr["FTeff"] = String.Format("{0}", rankingsPosition.list[id][pFTeff]);
                    dr["REB"] = String.Format("{0}", rankingsPosition.list[id][pRPG]);
                    dr["OREB"] = String.Format("{0}", rankingsPosition.list[id][pORPG]);
                    dr["DREB"] = String.Format("{0}", rankingsPosition.list[id][pDRPG]);
                    dr["AST"] = String.Format("{0}", rankingsPosition.list[id][pAPG]);
                    dr["TO"] = String.Format("{0}", rankingsPosition.list[id][pTPG]);
                    dr["STL"] = String.Format("{0}", rankingsPosition.list[id][pSPG]);
                    dr["BLK"] = String.Format("{0}", rankingsPosition.list[id][pBPG]);
                    dr["FOUL"] = String.Format("{0}", rankingsPosition.list[id][pFPG]);

                    dt_ov.Rows.Add(dr);
                }
            }
            else
            {
                dr = dt_ov.NewRow();

                dr["Type"] = "Team Avg";
                dr["PTS"] = String.Format("{0:F1}", ts.averages[PPG]);
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
            }

            #endregion

            DataView dv_ov = new DataView(dt_ov);
            dv_ov.AllowNew = false;

            dgvOverviewStats.DataContext = dv_ov;

            
            #region Prepare Box Scores

            dgvBoxScores.ItemsSource = curPBS;

            #endregion
        }

        private void UpdateSplitStats()
        {
            string qr_home = String.Format("select * from PlayerResults INNER JOIN GameResults ON "
                                           + "(PlayerResults.GameID = GameResults.GameID "
                                           + "AND Team = T2Name) "
                                           + "WHERE PlayerID = {0}", psr.ID);
            string qr_away = String.Format("select * from PlayerResults INNER JOIN GameResults ON "
                                           + "(PlayerResults.GameID = GameResults.GameID "
                                           + "AND Team = T1Name) "
                                           + "WHERE PlayerID = {0}", psr.ID);
            string qr_wins = String.Format("select * from PlayerResults INNER JOIN GameResults ON "
                                           + "(PlayerResults.GameID = GameResults.GameID) "
                                           + "WHERE PlayerID = {0} "
                                           + "AND (((Team = T1Name) AND (T1PTS > T2PTS)) OR ((Team = T2Name) AND (T2PTS > T1PTS)))", 
                                           psr.ID);
            string qr_losses = String.Format("select * from PlayerResults INNER JOIN GameResults ON "
                                           + "(PlayerResults.GameID = GameResults.GameID) "
                                           + "WHERE PlayerID = {0} "
                                           + "AND (((Team = T1Name) AND (T1PTS < T2PTS)) OR ((Team = T2Name) AND (T2PTS < T1PTS)))",
                                           psr.ID);
            string qr_season = String.Format("select * from PlayerResults INNER JOIN GameResults ON "
                                           + "(PlayerResults.GameID = GameResults.GameID) "
                                           + "WHERE PlayerID = {0} AND IsPlayoff LIKE 'False'", psr.ID);
            string qr_playoffs = String.Format("select * from PlayerResults INNER JOIN GameResults ON "
                                           + "(PlayerResults.GameID = GameResults.GameID) "
                                           + "WHERE PlayerID = {0} AND IsPlayoff LIKE 'True'", psr.ID);
            string qr_teams = String.Format("select Team from PlayerResults INNER JOIN GameResults ON " +
                                            "(PlayerResults.GameID = GameResults.GameID) " +
                                            " WHERE PlayerID = {0}", psr.ID);

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
                qr_teams = SQLiteDatabase.AddDateRangeToSQLQuery(qr_teams,
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
                qr_teams += s;
            }

            qr_teams += " GROUP BY Team";

            DataTable res;

            splitPSRs = new ObservableCollection<PlayerStatsRow>();

            //Home
            res = db.GetDataTable(qr_home);
            PlayerStats ps =
                new PlayerStats(new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1, psr.Position2));

            foreach (DataRow r in res.Rows)
            {
                ps.AddBoxScore(new PlayerBoxScore(r));
            }
            splitPSRs.Add(new PlayerStatsRow(ps, "Home"));

            //Away
            res = db.GetDataTable(qr_away);
            ps.ResetStats();

            foreach (DataRow r in res.Rows)
            {
                ps.AddBoxScore(new PlayerBoxScore(r));
            }
            splitPSRs.Add(new PlayerStatsRow(ps, "Away"));

            //Wins
            res = db.GetDataTable(qr_wins);
            ps.ResetStats();

            foreach (DataRow r in res.Rows)
            {
                ps.AddBoxScore(new PlayerBoxScore(r));
            }
            splitPSRs.Add(new PlayerStatsRow(ps, "Wins"));

            //Losses
            res = db.GetDataTable(qr_losses);
            ps.ResetStats();

            foreach (DataRow r in res.Rows)
            {
                ps.AddBoxScore(new PlayerBoxScore(r));
            }
            splitPSRs.Add(new PlayerStatsRow(ps, "Losses"));

            //Season
            res = db.GetDataTable(qr_season);
            ps.ResetStats();

            foreach (DataRow r in res.Rows)
            {
                ps.AddBoxScore(new PlayerBoxScore(r));
            }
            splitPSRs.Add(new PlayerStatsRow(ps, "Season"));

            //Playoffs
            res = db.GetDataTable(qr_playoffs);
            ps.ResetStats();

            foreach (DataRow r in res.Rows)
            {
                ps.AddBoxScore(new PlayerBoxScore(r));
            }
            splitPSRs.Add(new PlayerStatsRow(ps, "Playoffs"));

            #region Each Team Played In Stats

            res = db.GetDataTable(qr_teams);
            
            if (res.Rows.Count > 1)
            {
                List<string> teams = new List<string>(res.Rows.Count);
                foreach (DataRow r in res.Rows)
                    teams.Add(r["Team"].ToString());

                foreach (string team in teams)
                {
                    string q = String.Format("select * from PlayerResults INNER JOIN GameResults" +
                                             " ON (PlayerResults.GameID = GameResults.GameID)" +
                                             " WHERE PlayerID = {0} AND Team = '{1}'",
                                             psr.ID, team);
                    if (rbStatsBetween.IsChecked.GetValueOrDefault())
                    {
                        q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                                        dtpEnd.SelectedDate.GetValueOrDefault());}
                    else
                    {
                        string s = " AND SeasonNum = " + cmbSeasonNum.SelectedItem.ToString();
                        q += s;
                    }
                    res = db.GetDataTable(q);

                    ps.ResetStats();

                    foreach (DataRow r in res.Rows)
                    {
                        ps.AddBoxScore(new PlayerBoxScore(r));
                    }
                    splitPSRs.Add(new PlayerStatsRow(ps, team));
                }
            }
            #endregion

            #region Monthly Split Stats
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
                        string s = String.Format("select * from PlayerResults " +
                                                 "INNER JOIN GameResults " +
                                                 "ON (PlayerResults.GameID = GameResults.GameID) " +
                                                 "WHERE (Date >= '{0}' AND Date <= '{1}') AND PlayerID = {2}",
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dCur),
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dEnd),
                                                 psr.ID);

                        qrm.Add(s);
                        break;
                    }
                    else
                    {
                        string s = String.Format("select * from PlayerResults " +
                                                 "INNER JOIN GameResults " +
                                                 "ON (PlayerResults.GameID = GameResults.GameID) " +
                                                 "WHERE (Date >= '{0}' AND Date <= '{1}') AND PlayerID = {2}",
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(dCur),
                                                 SQLiteDatabase.ConvertDateTimeToSQLite(
                                                     new DateTime(dCur.Year, dCur.Month, 1).AddMonths(1).AddDays(-1)),
                                                 psr.ID);

                        qrm.Add(s);
                        dCur = dCur.AddMonths(1);
                    }
                }

                int i = 0;
                foreach (string q in qrm)
                {
                    ps.ResetStats();
                    res = db.GetDataTable(q);

                    foreach (DataRow r in res.Rows)
                    {
                        ps.AddBoxScore(new PlayerBoxScore(r));
                    }

                    DateTime label = new DateTime(dStart.Year, dStart.Month, 1).AddMonths(i);
                    splitPSRs.Add(new PlayerStatsRow(ps, label.Year.ToString() + " " + String.Format("{0:MMMM}", label)));
                    i++;
                }
            }
            #endregion

            dgvSplitStats.ItemsSource = splitPSRs;
        }

        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                try
                {
                    curSeason = Convert.ToInt32(cmbSeasonNum.SelectedItem.ToString());
                }
                catch (Exception)
                {
                    return;
                }
                MainWindow.curSeason = curSeason;
                playersT = "Players";

                if (curSeason != maxSeason)
                {
                    playersT += "S" + curSeason;
                }

                MainWindow.pst = MainWindow.GetPlayersFromDatabase(MainWindow.currentDB, curSeason, maxSeason);

                GetActivePlayers();

                if (cmbPlayer.SelectedIndex != -1)
                {
                    PlayerStats ps = CreatePlayerStatsFromCurrent();

                    string q = "select * from " + playersT + " where ID = " + ps.ID;
                    DataTable res = db.GetDataTable(q);

                    string newTeam;
                    bool nowActive;
                    if (res.Rows.Count > 0)
                    {
                        nowActive = StatsTracker.getBoolean(res.Rows[0], "isActive");
                        if (nowActive)
                        {
                            newTeam = res.Rows[0]["TeamFin"].ToString();
                        }
                        else
                        {
                            newTeam = " - Inactive -";
                        }
                        cmbTeam.SelectedIndex = -1;
                        if (nowActive)
                        {
                            if (newTeam != "")
                            {
                                cmbTeam.SelectedItem = newTeam;
                            }
                        }
                        else
                        {
                            cmbTeam.SelectedItem = "- Inactive -";
                        }
                        cmbPlayer.SelectedIndex = -1;
                        cmbPlayer.SelectedValue = ps.ID;
                    }
                    else
                    {
                        cmbTeam.SelectedIndex = -1;
                        cmbPlayer.SelectedIndex = -1;
                    }

                }
            }
        }

        private void btnScoutingReport_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Player Scouting Reports coming soon!");
        }

        private void btnSavePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == -1) return;

            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores"
                    + ") or edit the box-scores themselves.", "NBA Stats Tracker", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var ps = CreatePlayerStatsFromCurrent();

            Dictionary<int, PlayerStats> pslist = new Dictionary<int, PlayerStats>();
            pslist.Add(ps.ID, ps);

            MainWindow.savePlayersToDatabase(MainWindow.currentDB, pslist, curSeason, maxSeason);

            MainWindow.pst = MainWindow.GetPlayersFromDatabase(MainWindow.currentDB, curSeason, maxSeason);

            GetActivePlayers();
            cmbTeam.SelectedIndex = -1;
            if (ps.isActive)
            {
                cmbTeam.SelectedItem = ps.TeamF;
            }
            else
            {
                cmbTeam.SelectedItem = "- Inactive -";
            }
            cmbPlayer.SelectedIndex = -1;
            cmbPlayer.SelectedValue = ps.ID;
            //cmbPlayer.SelectedValue = ps.LastName + " " + ps.FirstName + " (" + ps.Position1 + ")";
        }

        private PlayerStats CreatePlayerStatsFromCurrent()
        {
            if (cmbPosition2.SelectedItem == null) cmbPosition2.SelectedItem = " ";

            string TeamF;
            if (chkIsActive.IsChecked.GetValueOrDefault() == false)
            {
                TeamF = "";
            }
            else
            {
                TeamF = cmbTeam.SelectedItem.ToString();
                if (TeamF == "- Inactive -")
                {
                    askedTeam = "";
                    askTeamW atw = new askTeamW(Teams);
                    atw.ShowDialog();
                    TeamF = askedTeam;
                }
            }

            PlayerStats ps = new PlayerStats(
                psr.ID, txtLastName.Text, txtFirstName.Text, cmbPosition1.SelectedItem.ToString(),
                cmbPosition2.SelectedItem.ToString(), TeamF, psr.TeamS,
                chkIsActive.IsChecked.GetValueOrDefault(), chkIsInjured.IsChecked.GetValueOrDefault(),
                chkIsAllStar.IsChecked.GetValueOrDefault(), chkIsNBAChampion.IsChecked.GetValueOrDefault(),
                dt_ov.Rows[0]);
            return ps;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count - 1) cmbTeam.SelectedIndex = 0;
            else cmbTeam.SelectedIndex++;
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == 0) cmbTeam.SelectedIndex = cmbTeam.Items.Count - 1;
            else cmbTeam.SelectedIndex--;
        }

        private void btnNextPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == cmbPlayer.Items.Count - 1) cmbPlayer.SelectedIndex = 0;
            else cmbPlayer.SelectedIndex++;
        }

        private void btnPrevPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == 0) cmbPlayer.SelectedIndex = cmbPlayer.Items.Count - 1;
            else cmbPlayer.SelectedIndex--;
        }

        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            cmbSeasonNum_SelectionChanged(null, null);
        }

        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            cmbPlayer_SelectionChanged(null, null);
        }

        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
            }
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                cmbPlayer_SelectionChanged(null, null);
            }
        }

        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
            }
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                cmbPlayer_SelectionChanged(null, null);
            }
        }

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

        private void cmbOppTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == -1) return;

            dgvHTH.ItemsSource = null;
            cmbOppPlayer.ItemsSource = null;

            oppPlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            string q;
            if (cmbOppTeam.SelectedItem.ToString() != "- Inactive -")
            {
                q = "select * from " + playersT + " where TeamFin LIKE '" + cmbOppTeam.SelectedItem + "' AND isActive LIKE 'True'";
            }
            else
            {
                q = "select * from " + playersT + " where isActive LIKE 'False'";
            }
            var res = db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
                oppPlayersList.Add(new KeyValuePair<int, string>(StatsTracker.getInt(r, "ID"),
                                StatsTracker.getString(r, "FirstName") + " " + StatsTracker.getString(r, "LastName") +
                                " (" + StatsTracker.getString(r, "Position1") + ")"));
            }

            cmbOppPlayer.ItemsSource = oppPlayersList;
        }

        private void cmbOppPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTeam.SelectedIndex == -1
                    || cmbOppTeam.SelectedIndex == -1
                    || cmbPlayer.SelectedIndex == -1
                    || cmbOppPlayer.SelectedIndex == -1)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            dgvHTH.ItemsSource = null;

            SelectedOppPlayerID =((KeyValuePair<int, string>) (cmbOppPlayer.SelectedItem)).Key;

            ObservableCollection<PlayerStatsRow> psrList = new ObservableCollection<PlayerStatsRow>();
            
            hthAllPBS = new List<PlayerBoxScore>();

            string q;
            DataTable res;

            if (SelectedPlayerID == SelectedOppPlayerID) return;

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    /*
                    q = "SELECT * FROM " + playersT + " WHERE ID = " + SelectedPlayerID;
                    res = db.GetDataTable(q);

                    PlayerStats ps = new PlayerStats(res.Rows[0]);
                    PlayerStatsRow ownPSR = new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName);
                    */
                    psr.Type = psr.FirstName + " " + psr.LastName;
                    psrList.Add(psr);

                    hthOwnPBS = new List<PlayerBoxScore>(curPBS);

                    q = "SELECT * FROM " + playersT + " WHERE ID = " + SelectedOppPlayerID;
                    res = db.GetDataTable(q);

                    PlayerStats ps = new PlayerStats(res.Rows[0]);
                    PlayerStatsRow oppPSR = new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName);

                    oppPSR.Type = oppPSR.FirstName + " " + oppPSR.LastName;
                    psrList.Add(oppPSR);

                    q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID) "
                    + "where PlayerID = " + SelectedOppPlayerID +
                    " AND SeasonNum = " + curSeason;
                    res = db.GetDataTable(q);

                    hthOppPBS = new List<PlayerBoxScore>();
                    foreach (DataRow r in res.Rows)
                    {
                        PlayerBoxScore pbs = new PlayerBoxScore(r);
                        hthOppPBS.Add(pbs);
                    }
                    List<int> gameIDs = new List<int>();
                    foreach (PlayerBoxScore bs in hthOwnPBS)
                    {
                        hthAllPBS.Add(bs);
                        gameIDs.Add(bs.GameID);
                    }
                    foreach (PlayerBoxScore bs in hthOppPBS)
                    {
                        if (!gameIDs.Contains(bs.GameID))
                        {
                            hthAllPBS.Add(bs);
                        }
                    }
                }
                else
                {
                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " +
                            "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} " +
                            "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " +
                            "WHERE PlayerID = {1} " +
                            "AND SeasonNum = {2}) ORDER BY Date DESC",
                            SelectedPlayerID, SelectedOppPlayerID, curSeason);
                    res = db.GetDataTable(q);

                    Player p = new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1, psr.Position2);
                    PlayerStats ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        PlayerBoxScore pbs = new PlayerBoxScore(r);
                        ps.AddBoxScore(pbs);
                        hthAllPBS.Add(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));


                    // Opponent
                    q = "SELECT * FROM " + playersT + " WHERE ID = " + SelectedOppPlayerID;
                    res = db.GetDataTable(q);

                    p = new Player(res.Rows[0]);

                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " +
                            "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} " +
                            "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " +
                            "WHERE PlayerID = {1} " +
                            "AND SeasonNum = {2}) ORDER BY Date DESC",
                            SelectedOppPlayerID, SelectedPlayerID, curSeason);
                    res = db.GetDataTable(q);

                    ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        PlayerBoxScore pbs = new PlayerBoxScore(r);
                        ps.AddBoxScore(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));
                }
            }
            else
            {
                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    psrList.Add(new PlayerStatsRow(psBetween, psBetween.FirstName + " " + psBetween.LastName));

                    List<int> gameIDs = new List<int>();
                    foreach (PlayerBoxScore cur in curPBS)
                    {
                        hthAllPBS.Add(cur);
                        gameIDs.Add(cur.GameID);
                    }

                    q = "SELECT * FROM " + playersT + " WHERE ID = " + SelectedOppPlayerID;
                    res = db.GetDataTable(q);

                    Player p = new Player(res.Rows[0]);

                    q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID) "
                        + "where PlayerID = " + SelectedOppPlayerID.ToString();
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    res = db.GetDataTable(q);

                    PlayerStats psOppBetween = new PlayerStats(p);
                    foreach (DataRow r in res.Rows)
                    {
                        PlayerBoxScore pbs = new PlayerBoxScore(r);
                        psOppBetween.AddBoxScore(pbs);

                        if (!gameIDs.Contains(pbs.GameID))
                        {
                            hthAllPBS.Add(pbs);
                        }
                    }

                    psrList.Add(new PlayerStatsRow(psOppBetween, psOppBetween.FirstName + " " + psOppBetween.LastName));
                }
                else
                {
                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " +
                            "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} " +
                            "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " +
                            "WHERE PlayerID = {1} ",
                            SelectedPlayerID, SelectedOppPlayerID);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q,
                                                              dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += ") ORDER BY Date DESC";
                    res = db.GetDataTable(q);

                    Player p = new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1, psr.Position2);
                    PlayerStats ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        PlayerBoxScore pbs = new PlayerBoxScore(r);
                        ps.AddBoxScore(pbs);
                        hthAllPBS.Add(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));


                    // Opponent
                    q = "SELECT * FROM " + playersT + " WHERE ID = " + SelectedOppPlayerID;
                    res = db.GetDataTable(q);

                    p = new Player(res.Rows[0]);

                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " +
                            "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} " +
                            "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " +
                            "WHERE PlayerID = {1} ",
                            SelectedOppPlayerID, SelectedPlayerID);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q,
                                                              dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += ") ORDER BY Date DESC";
                    res = db.GetDataTable(q);

                    ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        PlayerBoxScore pbs = new PlayerBoxScore(r);
                        ps.AddBoxScore(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));
                }
            }

            hthAllPBS.Sort(delegate(PlayerBoxScore pbs1, PlayerBoxScore pbs2) { return pbs1.Date.CompareTo(pbs2.Date); });
            hthAllPBS.Reverse();

            dgvHTH.ItemsSource = psrList;
            dgvHTHBoxScores.ItemsSource = hthAllPBS;
            //dgvHTHBoxScores.ItemsSource = new ObservableCollection<PlayerBoxScore>(hthAllPBS);
        }

        private int SelectedOppPlayerID { get; set; }

        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                PlayerBoxScore row = (PlayerBoxScore) dgvBoxScores.SelectedItems[0];
                int gameID = row.GameID;

                int i = 0;

                foreach (BoxScoreEntry bse in MainWindow.bshist)
                {
                    if (bse.bs.id == gameID)
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

        private void dgvHTHBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                PlayerBoxScore row = (PlayerBoxScore)dgvHTHBoxScores.SelectedItems[0];
                int gameID = row.GameID;

                int i = 0;

                foreach (BoxScoreEntry bse in MainWindow.bshist)
                {
                    if (bse.bs.id == gameID)
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

        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppPlayer_SelectionChanged(null, null);
        }

        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppPlayer_SelectionChanged(null, null);
        }
    }
}
