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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Misc;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.Miscellaneous;
using SQLite_Database;
using Swordfish.WPF.Charts;

#endregion

namespace NBA_Stats_Tracker.Windows
{
    /// <summary>
    ///     Shows player information and stats.
    /// </summary>
    public partial class PlayerOverviewWindow
    {
        public static string askedTeam;
        private readonly SQLiteDatabase db = new SQLiteDatabase(MainWindow.currentDB);
        private readonly int maxSeason = SQLiteIO.getMaxSeason(MainWindow.currentDB);
        private int SelectedPlayerID = -1;
        private List<string> Teams;

        private ObservableCollection<KeyValuePair<int, string>> _playersList = new ObservableCollection<KeyValuePair<int, string>>();

        private string _selectedPlayer;
        private bool changingTimeframe;
        private PlayerRankings cumPlayoffsRankingsActive, cumPlayoffsRankingsPosition, cumPlayoffsRankingsTeam;
        private PlayerRankings cumSeasonRankingsActive, cumSeasonRankingsPosition, cumSeasonRankingsTeam;
        private int curSeason = MainWindow.curSeason;
        private DataTable dt_ov;
        private List<PlayerBoxScore> hthAllPBS;
        private List<PlayerBoxScore> hthOppPBS;
        private List<PlayerBoxScore> hthOwnPBS;

        private ObservableCollection<KeyValuePair<int, string>> oppPlayersList = new ObservableCollection<KeyValuePair<int, string>>();

        private ObservableCollection<PlayerBoxScore> pbsList;
        private string pl_playersT = MainWindow.pl_playersT;
        private PlayerStatsRow pl_psr;
        private PlayerRankings pl_rankingsActive;
        private PlayerRankings pl_rankingsPosition;
        private PlayerRankings pl_rankingsTeam;

        private Dictionary<int, PlayerStats> playersActive;
        private Dictionary<int, PlayerStats> playersSamePosition;
        private Dictionary<int, PlayerStats> playersSameTeam;
        private string playersT = MainWindow.playersT;
        private PlayerStatsRow psr;
        private PlayerRankings rankingsActive;
        private PlayerRankings rankingsPosition;
        private PlayerRankings rankingsTeam;
        private ObservableCollection<PlayerStatsRow> splitPSRs;
        private SortedDictionary<string, int> teamOrder = MainWindow.TeamOrder;
        private ObservableCollection<PlayerHighsRow> recordsList { get; set; } 

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerOverviewWindow" /> class.
        /// </summary>
        public PlayerOverviewWindow()
        {
            InitializeComponent();

            Height = Misc.GetRegistrySetting("PlayerOvHeight", (int) Height);
            Width = Misc.GetRegistrySetting("PlayerOvWidth", (int) Width);
            Top = Misc.GetRegistrySetting("PlayerOvY", (int) Top);
            Left = Misc.GetRegistrySetting("PlayerOvX", (int) Left);

            prepareWindow();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerOverviewWindow" /> class.
        ///     Automatically switches to view a specific player.
        /// </summary>
        /// <param name="team">The player's team name.</param>
        /// <param name="playerID">The player ID.</param>
        public PlayerOverviewWindow(string team, int playerID) : this()
        {
            if (!String.IsNullOrWhiteSpace(team))
            {
                cmbTeam.SelectedItem = GetDisplayNameFromTeam(team);
            }
            else
            {
                cmbTeam.SelectedItem = "- Inactive -";
            }
            cmbPlayer.SelectedValue = playerID.ToString();
        }

        private ObservableCollection<KeyValuePair<int, string>> PlayersList
        {
            get { return _playersList; }
            set
            {
                _playersList = value;
                OnPropertyChanged("PlayersList");
            }
        }

        public string SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                _selectedPlayer = value;
                OnPropertyChanged("SelectedPlayer");
            }
        }

        private int SelectedOppPlayerID { get; set; }

        /// <summary>
        ///     Finds a team's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Requested team that is hidden.</exception>
        private string GetCurTeamFromDisplayName(string displayName)
        {
            if (displayName == "- Inactive -")
                return displayName;
            foreach (int kvp in MainWindow.tst.Keys)
            {
                if (MainWindow.tst[kvp].displayName == displayName)
                {
                    if (MainWindow.tst[kvp].isHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.tst[kvp].name);

                    return MainWindow.tst[kvp].name;
                }
            }
            throw new Exception("Team not found: " + displayName);
        }

        /// <summary>
        ///     Finds a team's displayName by its name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Requested team that is hidden.</exception>
        private string GetDisplayNameFromTeam(string name)
        {
            if (name == "- Inactive -")
                return name;
            foreach (int kvp in MainWindow.tst.Keys)
            {
                if (MainWindow.tst[kvp].name == name)
                {
                    if (MainWindow.tst[kvp].isHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.tst[kvp].name);

                    return MainWindow.tst[kvp].displayName;
                }
            }
            throw new Exception("Team not found: " + name);
        }

        /// <summary>
        ///     Populates the teams combo.
        /// </summary>
        private void PopulateTeamsCombo()
        {
            Teams = new List<string>();
            foreach (var kvp in teamOrder)
            {
                if (!MainWindow.tst[kvp.Value].isHidden)
                    Teams.Add(MainWindow.tst[kvp.Value].displayName);
            }

            Teams.Sort();

            Teams.Add("- Inactive -");

            cmbTeam.ItemsSource = Teams;
            cmbOppTeam.ItemsSource = Teams;
        }

        /// <summary>
        ///     Prepares the window: populates data tables, sets DataGrid properties, populates combos and calculates metrics.
        /// </summary>
        private void prepareWindow()
        {
            DataContext = this;

            PopulateSeasonCombo();

            var Positions = new List<string> {" ", "PG", "SG", "SF", "PF", "C"};
            var Positions2 = new List<string> {" ", "PG", "SG", "SF", "PF", "C"};
            cmbPosition1.ItemsSource = Positions;
            cmbPosition2.ItemsSource = Positions2;

            PopulateTeamsCombo();

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

            changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.tf.EndDate;
            dtpStart.SelectedDate = MainWindow.tf.StartDate;
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

            dgvBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTH.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvHTHBoxScores.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvOverviewStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvSplitStats.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            dgvYearly.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

            grpPlayoffsFacts.Visibility = Visibility.Collapsed;
            grpPlayoffsLeadersFacts.Visibility = Visibility.Collapsed;
            grpPlayoffsScoutingReport.Visibility = Visibility.Collapsed;
            grpSeasonFacts.Visibility = Visibility.Collapsed;
            grpSeasonLeadersFacts.Visibility = Visibility.Collapsed;
            grpSeasonScoutingReport.Visibility = Visibility.Collapsed;

            GetActivePlayers();
            PopulateGraphStatCombo();
        }

        /// <summary>
        ///     Gets a player stats dictionary of only the active players, and calculates their rankingsPerGame.
        /// </summary>
        private void GetActivePlayers()
        {
            playersActive = new Dictionary<int, PlayerStats>();

            string q = "select * from " + playersT + " where isActive LIKE \"True\"";
            q += " AND isHidden LIKE \"False\"";
            DataTable res = db.GetDataTable(q);
            foreach (DataRow r in res.Rows)
            {
                string q2 = "select * from " + pl_playersT + " where ID = " + Tools.getInt(r, "ID");
                DataTable pl_res = db.GetDataTable(q2);

                var ps = new PlayerStats(r);
                playersActive.Add(ps.ID, ps);
                playersActive[ps.ID].UpdatePlayoffStats(pl_res.Rows[0]);
            }

            rankingsActive = new PlayerRankings(playersActive);
            pl_rankingsActive = new PlayerRankings(playersActive, true);
        }

        /// <summary>
        ///     Populates the season combo.
        /// </summary>
        private void PopulateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = curSeason;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbTeam control.
        ///     Populates the player combo, resets all relevant DataGrid DataContext and ItemsSource properties, and calculates the in-team player rankingsPerGame.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgvOverviewStats.DataContext = null;
            grdOverview.DataContext = null;
            cmbPosition1.SelectedIndex = -1;
            cmbPosition2.SelectedIndex = -1;
            dgvBoxScores.ItemsSource = null;
            dgvHTH.ItemsSource = null;
            dgvHTHBoxScores.ItemsSource = null;
            dgvSplitStats.ItemsSource = null;
            dgvYearly.ItemsSource = null;
            grpPlayoffsFacts.Visibility = Visibility.Collapsed;
            grpPlayoffsLeadersFacts.Visibility = Visibility.Collapsed;
            grpPlayoffsScoutingReport.Visibility = Visibility.Collapsed;
            grpSeasonFacts.Visibility = Visibility.Collapsed;
            grpSeasonLeadersFacts.Visibility = Visibility.Collapsed;
            grpSeasonScoutingReport.Visibility = Visibility.Collapsed;
            
            if (cmbTeam.SelectedIndex == -1)
                return;

            cmbPlayer.ItemsSource = null;

            PlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            playersSameTeam = new Dictionary<int, PlayerStats>();
            if (cmbTeam.SelectedItem.ToString() != "- Inactive -")
            {
                List<PlayerStats> list =
                    MainWindow.pst.Values.Where(
                        ps => ps.TeamF == GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString()) && !ps.isHidden && ps.isActive)
                              .ToList();
                list.Sort((ps1, ps2) => ps1.LastName.CompareTo(ps2.LastName));
                list.ForEach(delegate(PlayerStats ps)
                             {
                                 PlayersList.Add(new KeyValuePair<int, string>(ps.ID,
                                                                               String.Format("{0}, {1} ({2})", ps.LastName, ps.FirstName,
                                                                                             ps.Position1.ToString())));
                                 playersSameTeam.Add(ps.ID, ps);
                             });
            }
            else
            {
                List<PlayerStats> list = MainWindow.pst.Values.Where(ps => !ps.isHidden && !ps.isActive).ToList();
                list.Sort((ps1, ps2) => ps1.LastName.CompareTo(ps2.LastName));
                list.ForEach(delegate(PlayerStats ps)
                             {
                                 PlayersList.Add(new KeyValuePair<int, string>(ps.ID,
                                                                               String.Format("{0}, {1} ({2})", ps.LastName, ps.FirstName,
                                                                                             ps.Position1.ToString())));
                                 playersSameTeam.Add(ps.ID, ps);
                             });
            }
            rankingsTeam = new PlayerRankings(playersSameTeam);
            pl_rankingsTeam = new PlayerRankings(playersSameTeam, true);

            cmbPlayer.ItemsSource = PlayersList;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbPlayer control.
        ///     Updates the PlayerStatsRow instance and all DataGrid controls with this player's information and stats.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == -1)
                return;

            SelectedPlayerID = ((KeyValuePair<int, string>) (((cmbPlayer)).SelectedItem)).Key;

            pbsList = new ObservableCollection<PlayerBoxScore>();

            string q = "select * from " + playersT + " where ID = " + SelectedPlayerID.ToString();
            DataTable res = db.GetDataTable(q);

            if (res.Rows.Count == 0) // Player not found in this year's database
            {
                cmbTeam_SelectionChanged(null, null); // Reload this team's players
                return;
            }
            /*
            string q2 = "select * from " + pl_playersT + " where ID = " + SelectedPlayerID.ToString();
            DataTable pl_res = db.GetDataTable(q2);

            psr = new PlayerStatsRow(new PlayerStats(res.Rows[0]));
            pl_psr = new PlayerStatsRow(new PlayerStats(pl_res.Rows[0], true), true);
            */
            psr = new PlayerStatsRow(MainWindow.pst[SelectedPlayerID]);
            pl_psr = new PlayerStatsRow(MainWindow.pst[SelectedPlayerID], true);

            UpdateOverviewAndBoxScores();

            UpdateSplitStats();

            UpdateYearlyReport();

            cumSeasonRankingsActive = PlayerRankings.CalculateActiveRankings();
            cumSeasonRankingsPosition =
                new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.Position1 == psr.Position1).ToDictionary(r => r.Key, r => r.Value));
            cumSeasonRankingsTeam =
                new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.TeamF == psr.TeamF).ToDictionary(r => r.Key, r => r.Value));

            cumPlayoffsRankingsActive = PlayerRankings.CalculateActiveRankings(true);
            cumPlayoffsRankingsPosition =
                new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.Position1 == psr.Position1).ToDictionary(r => r.Key, r => r.Value),
                                   true);
            cumPlayoffsRankingsTeam =
                new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.TeamF == psr.TeamF).ToDictionary(r => r.Key, r => r.Value), true);

            UpdateScoutingReport();

            UpdateRecords();

            cmbOppPlayer_SelectionChanged(null, null);

            if (cmbGraphStat.SelectedIndex == -1)
                cmbGraphStat.SelectedIndex = 0;
        }

        private List<string> GetFacts(int id, PlayerRankings rankings, bool onlyLeaders = false)
        {
            var leadersStats = new List<int> {p.PPG, p.FGp, p.TPp, p.FTp, p.RPG, p.SPG, p.APG, p.BPG};
            int count = 0;
            var facts = new List<string>();
            for (int i = 0; i < rankings.rankingsPerGame[id].Length; i++)
            {
                if (onlyLeaders)
                {
                    if (!leadersStats.Contains(i))
                        continue;
                }
                int rank = rankings.rankingsPerGame[id][i];
                if (rank <= 20)
                {
                    string fact = String.Format("{0}{1} in {2}: ", rank, Misc.getRankingSuffix(rank), p.averages[i]);
                    if (p.averages[i].EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", MainWindow.pst[id].averages[i]);
                    }
                    else if (p.averages[i].EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2} ", MainWindow.pst[id].averages[i]);
                        switch (p.averages[i].Substring(0, 2))
                        {
                            case "FG":
                                fact += String.Format("({0:F1} FGM/G on {1:F3})",
                                                      ((double) MainWindow.pst[id].stats[p.FGM])/MainWindow.pst[id].stats[p.GP],
                                                      MainWindow.pst[id].averages[p.FGp]);
                                break;
                            case "3P":
                                fact += String.Format("({0:F1} 3PM/G on {1:F3})",
                                                      ((double)MainWindow.pst[id].stats[p.TPM]) / MainWindow.pst[id].stats[p.GP],
                                                      MainWindow.pst[id].averages[p.TPp]);
                                break;
                            case "FT":
                                fact += String.Format("({0:F1} FTM/G on {1:F3})",
                                                      ((double) MainWindow.pst[id].stats[p.FTM])/MainWindow.pst[id].stats[p.GP],
                                                      MainWindow.pst[id].averages[p.FTp]);
                                break;
                        }
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", MainWindow.pst[id].averages[i]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            if (!onlyLeaders)
            {
                for (int i = 0; i < rankings.rankingsTotal[id].Length; i++)
                {
                    int rank = rankings.rankingsTotal[id][i];
                    if (rank <= 20)
                    {
                        string fact = String.Format("{0}{1} in {2}: ", rank, Misc.getRankingSuffix(rank), p.totals[i]);
                        fact += String.Format("{0}", MainWindow.pst[id].stats[i]);
                        facts.Add(fact);
                        count++;
                    }
                }
                var metricsToSkip = new List<string> {"aPER", "uPER"};
                for (int i = 0; i < rankings.rankingsMetrics[id].Keys.Count; i++)
                {
                    string metricName = rankings.rankingsMetrics[id].Keys.ToList()[i];
                    if (metricsToSkip.Contains(metricName))
                        continue;

                    int rank = rankings.rankingsMetrics[id][metricName];
                    if (rank <= 20)
                    {
                        string fact = String.Format("{0}{1} in {2}: ", rank, Misc.getRankingSuffix(rank), metricName.Replace("p", "%"));
                        if (metricName.EndsWith("p") || metricName.EndsWith("%"))
                        {
                            fact += String.Format("{0:F3}", MainWindow.pst[id].metrics[metricName]);
                        }
                        else if (metricName.EndsWith("eff"))
                        {
                            fact += String.Format("{0:F2}", MainWindow.pst[id].metrics[metricName]);
                        }
                        else
                        {
                            fact += String.Format("{0:F1}", MainWindow.pst[id].metrics[metricName]);
                        }
                        facts.Add(fact);
                        count++;
                    }
                }
            }
            facts.Sort(
                (f1, f2) =>
                Convert.ToInt32(f1.Substring(0, f1.IndexOfAny(new[] {'s', 'n', 'r', 't'})))
                       .CompareTo(Convert.ToInt32(f2.Substring(0, f2.IndexOfAny(new[] {'s', 'n', 'r', 't'})))));
            return facts;
        }

        private void UpdateScoutingReport()
        {
            int id = SelectedPlayerID;

            if (MainWindow.pst[id].stats[p.GP] > 0)
            {
                grpSeasonScoutingReport.Visibility = Visibility.Visible;
                grpSeasonFacts.Visibility = Visibility.Visible;
                grpSeasonLeadersFacts.Visibility = Visibility.Visible;

                string msg = new PlayerStatsRow(MainWindow.pst[id], false, false).ScoutingReport(MainWindow.pst, cumSeasonRankingsActive,
                                                                                                 cumSeasonRankingsTeam,
                                                                                                 cumSeasonRankingsPosition, pbsList,
                                                                                                 txbGame1.Text);
                txbSeasonScoutingReport.Text = msg;

                List<string> facts = GetFacts(id, cumSeasonRankingsActive);
                txbSeasonFacts.Text = AggregateFacts(facts);
                if (facts.Count == 0)
                    grpSeasonFacts.Visibility = Visibility.Collapsed;

                facts = GetFacts(id, MainWindow.SeasonLeadersRankings, true);
                txbSeasonLeadersFacts.Text = AggregateFacts(facts);
                if (facts.Count == 0)
                    grpSeasonLeadersFacts.Visibility = Visibility.Collapsed;
            }
            else
            {
                grpSeasonScoutingReport.Visibility = Visibility.Collapsed;
                grpSeasonFacts.Visibility = Visibility.Collapsed;
                grpSeasonLeadersFacts.Visibility = Visibility.Collapsed;
            }

            if (MainWindow.pst[id].pl_stats[p.GP] > 0)
            {
                grpPlayoffsScoutingReport.Visibility = Visibility.Visible;
                grpPlayoffsFacts.Visibility = Visibility.Visible;
                grpPlayoffsLeadersFacts.Visibility = Visibility.Visible;

                string msg = new PlayerStatsRow(MainWindow.pst[id], true, false).ScoutingReport(MainWindow.pst, cumPlayoffsRankingsActive, cumPlayoffsRankingsTeam, cumPlayoffsRankingsPosition, pbsList, txbGame1.Text);
                txbPlayoffsScoutingReport.Text = msg;

                List<string> facts = GetFacts(id, cumPlayoffsRankingsActive);
                txbPlayoffsFacts.Text = AggregateFacts(facts);
                if (facts.Count == 0)
                    grpPlayoffsFacts.Visibility = Visibility.Collapsed;

                facts = GetFacts(id, MainWindow.PlayoffsLeadersRankings, true);
                txbPlayoffsLeadersFacts.Text = AggregateFacts(facts);
                if (facts.Count == 0)
                    grpPlayoffsLeadersFacts.Visibility = Visibility.Collapsed;
            }
            else
            {
                grpPlayoffsScoutingReport.Visibility = Visibility.Collapsed;
                grpPlayoffsFacts.Visibility = Visibility.Collapsed;
                grpPlayoffsLeadersFacts.Visibility = Visibility.Collapsed;
            }

            svScoutingReport.ScrollToTop();
        }

        private static string AggregateFacts(List<string> facts)
        {
            switch (facts.Count)
            {
                case 0:
                    return "";
                case 1:
                    return facts[0];
                default:
                    return facts.Aggregate((s1, s2) => s1 + "\n" + s2);
            }
        }

        private void UpdateRecords()
        {
            //MainWindow.pst[SelectedPlayerID].CalculateSeasonHighs();

            recordsList = new ObservableCollection<PlayerHighsRow>();
            var shList = MainWindow.seasonHighs.Single(sh => sh.Key == SelectedPlayerID).Value;
            foreach (var shRec in shList)
            {
                recordsList.Add(new PlayerHighsRow(SelectedPlayerID, "Season " + MainWindow.GetSeasonName(shRec.Key), shRec.Value));
            }
            recordsList.Add(new PlayerHighsRow(SelectedPlayerID, "Career", MainWindow.pst[SelectedPlayerID].careerHighs));

            dgvHighs.ItemsSource = null;
            dgvHighs.ItemsSource = recordsList;

            dgvContract.ItemsSource = new ObservableCollection<PlayerStatsRow>{psr};
        }

        /// <summary>
        ///     Updates the tab viewing the year-by-year overview of the player's stats.
        /// </summary>
        private void UpdateYearlyReport()
        {
            var psrList = new List<PlayerStatsRow>();
            var psCareer = new PlayerStats(new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1, psr.Position2));

            string qr = "SELECT * FROM PastPlayerStats WHERE PlayerID = " + psr.ID + " ORDER BY \"SOrder\"";
            DataTable dt = db.GetDataTable(qr);
            foreach (DataRow dr in dt.Rows)
            {
                var ps = new PlayerStats();
                bool isPlayoff = Tools.getBoolean(dr, "isPlayoff");
                ps.GetStatsFromDataRow(dr, isPlayoff);
                ps.TeamF = Tools.getString(dr, "TeamFin");
                ps.TeamS = Tools.getString(dr, "TeamSta");
                var tempMetrics = isPlayoff ? ps.pl_metrics : ps.metrics;
                PlayerStats.CalculateRates(isPlayoff ? ps.pl_stats : ps.stats, ref tempMetrics);
                var curPSR = new PlayerStatsRow(ps, isPlayoff ? "Playoffs " + Tools.getString(dr, "SeasonName") : "Season " + Tools.getString(dr, "SeasonName"), isPlayoff);
                
                psrList.Add(curPSR);
                
                psCareer.AddPlayerStats(ps);
            }

            for (int i = 1; i <= maxSeason; i++)
            {
                string pT = "Players";
                if (i != maxSeason)
                    pT += "S" + i;

                string q = "select * from " + pT + " where ID = " + SelectedPlayerID;
                DataTable res = db.GetDataTable(q);
                if (res.Rows.Count == 1)
                {
                    var ps = new PlayerStats(res.Rows[0]);
                    PlayerStats.CalculateRates(ps.stats, ref ps.metrics);
                    var psr2 = new PlayerStatsRow(ps, "Season " + i);
                    psrList.Add(psr2);
                    psCareer.AddPlayerStats(ps);
                }

                pT = "PlayoffPlayers";
                if (i != maxSeason)
                    pT += "S" + i;

                q = "select * from " + pT + " where ID = " + SelectedPlayerID;
                res = db.GetDataTable(q);
                if (res.Rows.Count == 1)
                {
                    var ps = new PlayerStats(res.Rows[0], true);
                    if (ps.pl_stats[p.GP] > 0)
                    {
                        PlayerStats.CalculateRates(ps.pl_stats, ref ps.pl_metrics);
                        var psr2 = new PlayerStatsRow(ps, "Playoffs " + i, true);
                        psrList.Add(psr2);
                        psCareer.AddPlayerStats(ps, true);
                    }
                }
            }

            PlayerStats.CalculateRates(psCareer.stats, ref psCareer.metrics);
            psrList.Add(new PlayerStatsRow(psCareer, "Career", "Career"));

            var psrListCollection = new ListCollectionView(psrList);
            Debug.Assert(psrListCollection.GroupDescriptions != null, "psrListCollection.GroupDescriptions != null");
            psrListCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            dgvYearly.ItemsSource = psrListCollection;
        }

        /// <summary>
        ///     Updates the overview tab and prepares the available box scores for the current timeframe.
        /// </summary>
        private void UpdateOverviewAndBoxScores()
        {
            TeamStats ts = psr.isActive ? MainWindow.tst[MainWindow.TeamOrder[psr.TeamF]] : new TeamStats();
            var tsopp = new TeamStats("Opponents");

            grdOverview.DataContext = psr;

            playersSamePosition =
                MainWindow.pst.Where(ps => ps.Value.Position1 == psr.Position1 && ps.Value.isActive)
                          .ToDictionary(ps => ps.Value.ID, ps => ps.Value);

            rankingsPosition = new PlayerRankings(playersSamePosition);
            pl_rankingsPosition = new PlayerRankings(playersSamePosition, true);

            foreach (
                BoxScoreEntry bse in MainWindow.bshist.Where(bse => bse.pbsList.Any(pbs => pbs.PlayerID == psr.ID && !pbs.isOut)).ToList())
            {
                var pbs = new PlayerBoxScore();
                pbs = bse.pbsList.Single(pbs1 => pbs1.PlayerID == psr.ID);
                pbs.AddInfoFromTeamBoxScore(bse.bs);
                pbs.CalcMetrics(bse.bs);
                pbsList.Add(pbs);
            }

            cmbPosition1.SelectedItem = psr.Position1.ToString();
            cmbPosition2.SelectedItem = psr.Position2.ToString();

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

            if (psr.isActive)
            {
                int id = SelectedPlayerID;

                dr = dt_ov.NewRow();

                dr["Type"] = "Rankings";
                dr["MINS"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.MPG]);
                dr["PTS"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.PPG]);
                dr["FG"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.FGp]);
                dr["FGeff"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.FGeff]);
                dr["3PT"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.TPp]);
                dr["3Peff"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.TPeff]);
                dr["FT"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.FTp]);
                dr["FTeff"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.FTeff]);
                dr["REB"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.RPG]);
                dr["OREB"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.ORPG]);
                dr["DREB"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.DRPG]);
                dr["AST"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][t.PAPG]);
                dr["TO"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.TPG]);
                dr["STL"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.SPG]);
                dr["BLK"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.BPG]);
                dr["FOUL"] = String.Format("{0}", rankingsActive.rankingsPerGame[id][p.FPG]);

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "In-team Rankings";
                dr["MINS"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.MPG]);
                dr["PTS"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.PPG]);
                dr["FG"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.FGp]);
                dr["FGeff"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.FGeff]);
                dr["3PT"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.TPp]);
                dr["3Peff"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.TPeff]);
                dr["FT"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.FTp]);
                dr["FTeff"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.FTeff]);
                dr["REB"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.RPG]);
                dr["OREB"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.ORPG]);
                dr["DREB"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.DRPG]);
                dr["AST"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][t.PAPG]);
                dr["TO"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.TPG]);
                dr["STL"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.SPG]);
                dr["BLK"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.BPG]);
                dr["FOUL"] = String.Format("{0}", rankingsTeam.rankingsPerGame[id][p.FPG]);

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Position Rankings";
                dr["MINS"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.MPG]);
                dr["PTS"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.PPG]);
                dr["FG"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.FGp]);
                dr["FGeff"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.FGeff]);
                dr["3PT"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.TPp]);
                dr["3Peff"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.TPeff]);
                dr["FT"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.FTp]);
                dr["FTeff"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.FTeff]);
                dr["REB"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.RPG]);
                dr["OREB"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.ORPG]);
                dr["DREB"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.DRPG]);
                dr["AST"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][t.PAPG]);
                dr["TO"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.TPG]);
                dr["STL"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.SPG]);
                dr["BLK"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.BPG]);
                dr["FOUL"] = String.Format("{0}", rankingsPosition.rankingsPerGame[id][p.FPG]);

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Team Avg";
                dr["PTS"] = String.Format("{0:F1}", ts.averages[t.PPG]);
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

                dt_ov.Rows.Add(dr);
            }

            #endregion

            dr = dt_ov.NewRow();

            dr["Type"] = " ";

            dt_ov.Rows.Add(dr);

            #region Playoffs

            dr = dt_ov.NewRow();

            dr["Type"] = "Pl Stats";
            dr["GP"] = pl_psr.GP.ToString();
            dr["GS"] = pl_psr.GS.ToString();
            dr["MINS"] = pl_psr.MINS.ToString();
            dr["PTS"] = pl_psr.PTS.ToString();
            dr["FG"] = pl_psr.FGM.ToString() + "-" + pl_psr.FGA.ToString();
            dr["3PT"] = pl_psr.TPM.ToString() + "-" + pl_psr.TPA.ToString();
            dr["FT"] = pl_psr.FTM.ToString() + "-" + pl_psr.FTA.ToString();
            dr["REB"] = (pl_psr.DREB + pl_psr.OREB).ToString();
            dr["OREB"] = pl_psr.OREB.ToString();
            dr["DREB"] = pl_psr.DREB.ToString();
            dr["AST"] = pl_psr.AST.ToString();
            dr["TO"] = pl_psr.TOS.ToString();
            dr["STL"] = pl_psr.STL.ToString();
            dr["BLK"] = pl_psr.BLK.ToString();
            dr["FOUL"] = pl_psr.FOUL.ToString();

            dt_ov.Rows.Add(dr);

            dr = dt_ov.NewRow();

            dr["Type"] = "Pl Avg";
            dr["MINS"] = String.Format("{0:F1}", pl_psr.MPG);
            dr["PTS"] = String.Format("{0:F1}", pl_psr.PPG);
            dr["FG"] = String.Format("{0:F3}", pl_psr.FGp);
            dr["FGeff"] = String.Format("{0:F2}", pl_psr.FGeff);
            dr["3PT"] = String.Format("{0:F3}", pl_psr.TPp);
            dr["3Peff"] = String.Format("{0:F2}", pl_psr.TPeff);
            dr["FT"] = String.Format("{0:F3}", pl_psr.FTp);
            dr["FTeff"] = String.Format("{0:F2}", pl_psr.FTeff);
            dr["REB"] = String.Format("{0:F1}", pl_psr.RPG);
            dr["OREB"] = String.Format("{0:F1}", pl_psr.ORPG);
            dr["DREB"] = String.Format("{0:F1}", pl_psr.DRPG);
            dr["AST"] = String.Format("{0:F1}", pl_psr.APG);
            dr["TO"] = String.Format("{0:F1}", pl_psr.TPG);
            dr["STL"] = String.Format("{0:F1}", pl_psr.SPG);
            dr["BLK"] = String.Format("{0:F1}", pl_psr.BPG);
            dr["FOUL"] = String.Format("{0:F1}", pl_psr.FPG);

            dt_ov.Rows.Add(dr);

            #region Rankings

            if (psr.isActive)
            {
                int id = Convert.ToInt32(SelectedPlayerID);

                dr = dt_ov.NewRow();

                dr["Type"] = "Pl Rank";
                dr["MINS"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.MPG]);
                dr["PTS"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.PPG]);
                dr["FG"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.FGp]);
                dr["FGeff"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.FGeff]);
                dr["3PT"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.TPp]);
                dr["3Peff"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.TPeff]);
                dr["FT"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.FTp]);
                dr["FTeff"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.FTeff]);
                dr["REB"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.RPG]);
                dr["OREB"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.ORPG]);
                dr["DREB"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.DRPG]);
                dr["AST"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][t.PAPG]);
                dr["TO"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.TPG]);
                dr["STL"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.SPG]);
                dr["BLK"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.BPG]);
                dr["FOUL"] = String.Format("{0}", pl_rankingsActive.rankingsPerGame[id][p.FPG]);

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Pl In-Team";
                dr["MINS"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.MPG]);
                dr["PTS"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.PPG]);
                dr["FG"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.FGp]);
                dr["FGeff"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.FGeff]);
                dr["3PT"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.TPp]);
                dr["3Peff"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.TPeff]);
                dr["FT"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.FTp]);
                dr["FTeff"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.FTeff]);
                dr["REB"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.RPG]);
                dr["OREB"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.ORPG]);
                dr["DREB"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.DRPG]);
                dr["AST"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][t.PAPG]);
                dr["TO"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.TPG]);
                dr["STL"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.SPG]);
                dr["BLK"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.BPG]);
                dr["FOUL"] = String.Format("{0}", pl_rankingsTeam.rankingsPerGame[id][p.FPG]);

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Pl Position";
                dr["MINS"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.MPG]);
                dr["PTS"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.PPG]);
                dr["FG"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.FGp]);
                dr["FGeff"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.FGeff]);
                dr["3PT"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.TPp]);
                dr["3Peff"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.TPeff]);
                dr["FT"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.FTp]);
                dr["FTeff"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.FTeff]);
                dr["REB"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.RPG]);
                dr["OREB"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.ORPG]);
                dr["DREB"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.DRPG]);
                dr["AST"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][t.PAPG]);
                dr["TO"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.TPG]);
                dr["STL"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.SPG]);
                dr["BLK"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.BPG]);
                dr["FOUL"] = String.Format("{0}", pl_rankingsPosition.rankingsPerGame[id][p.FPG]);

                dt_ov.Rows.Add(dr);

                dr = dt_ov.NewRow();

                dr["Type"] = "Pl Team Avg";
                dr["PTS"] = String.Format("{0:F1}", ts.pl_averages[t.PPG]);
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

                dt_ov.Rows.Add(dr);
            }

            #endregion

            #endregion

            var dv_ov = new DataView(dt_ov) {AllowNew = false};

            dgvOverviewStats.DataContext = dv_ov;

            #region Prepare Box Scores

            dgvBoxScores.ItemsSource = pbsList;
            UpdateBest();
            cmbGraphStat_SelectionChanged(null, null);

            #endregion
        }

        /// <summary>
        ///     Updates the best performances tab with the player's best performances and the most significant stats of each one for the current timeframe.
        /// </summary>
        private void UpdateBest()
        {
            txbGame1.Text = "";
            txbGame2.Text = "";
            txbGame3.Text = "";
            txbGame4.Text = "";
            txbGame5.Text = "";
            txbGame6.Text = "";

            try
            {
                List<PlayerBoxScore> templist = pbsList.ToList();
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

                PlayerBoxScore psr1 = templist[0];
                string text = psr1.GetBestStats(5, psr.Position1);
                txbGame1.Text = "1: " + psr1.Date + " vs " + psr1.OppTeam + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[1];
                text = psr1.GetBestStats(5, psr.Position1);
                txbGame2.Text = "2: " + psr1.Date + " vs " + psr1.OppTeam + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[2];
                text = psr1.GetBestStats(5, psr.Position1);
                txbGame3.Text = "3: " + psr1.Date + " vs " + psr1.OppTeam + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[3];
                text = psr1.GetBestStats(5, psr.Position1);
                txbGame4.Text = "4: " + psr1.Date + " vs " + psr1.OppTeam + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[4];
                text = psr1.GetBestStats(5, psr.Position1);
                txbGame5.Text = "5: " + psr1.Date + " vs " + psr1.OppTeam + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[5];
                text = psr1.GetBestStats(5, psr.Position1);
                txbGame6.Text = "6: " + psr1.Date + " vs " + psr1.OppTeam + " (" + psr1.Result + ")\n\n" + text;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Updates the split stats tab for the current timeframe.
        /// </summary>
        private void UpdateSplitStats()
        {
            splitPSRs = new ObservableCollection<PlayerStatsRow>();
            Dictionary<int, Dictionary<string, PlayerStats>> split = MainWindow.splitPlayerStats;
            //Home
            splitPSRs.Add(new PlayerStatsRow(split[psr.ID]["Home"], "Home"));

            //Away
            splitPSRs.Add(new PlayerStatsRow(split[psr.ID]["Away"], "Away"));

            //Wins
            splitPSRs.Add(new PlayerStatsRow(split[psr.ID]["Wins"], "Wins", "Result"));

            //Losses
            splitPSRs.Add(new PlayerStatsRow(split[psr.ID]["Losses"], "Losses", "Result"));

            //Season
            splitPSRs.Add(new PlayerStatsRow(split[psr.ID]["Season"], "Season", "Part of Season"));

            //Playoffs
            splitPSRs.Add(new PlayerStatsRow(split[psr.ID]["Playoffs"], "Playoffs", "Part of Season"));

            #region Each Team Played In Stats

            foreach (var ss in split[psr.ID].Where(pair => pair.Key.StartsWith("with ")))
            {
                splitPSRs.Add(new PlayerStatsRow(split[psr.ID][ss.Key], ss.Key, "Team Played For"));
            }

            #endregion

            #region Opponents

            foreach (var ss in split[psr.ID].Where(pair => pair.Key.StartsWith("vs ")))
            {
                splitPSRs.Add(new PlayerStatsRow(split[psr.ID][ss.Key], ss.Key, "Team Played Against"));
            }

            #endregion

            #region Monthly Split Stats

            foreach (var ss in split[psr.ID].Where(pair => pair.Key.StartsWith("M ")))
            {
                splitPSRs.Add(new PlayerStatsRow(split[psr.ID][ss.Key], ss.Key.Substring(2), "Monthly"));
            }

            #endregion

            var splitPSRsCollection = new ListCollectionView(splitPSRs);
            Debug.Assert(splitPSRsCollection.GroupDescriptions != null, "splitPSRsCollection.GroupDescriptions != null");
            splitPSRsCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            dgvSplitStats.ItemsSource = splitPSRsCollection;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control.
        ///     Loads the specified season's team and player stats and tries to automatically switch to the same player again, if he exists in the specified season and isn't hidden.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                rbStatsAllTime.IsChecked = true;
                changingTimeframe = false;

                try
                {
                    curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;
                }
                catch (Exception)
                {
                    return;
                }

                if (!(MainWindow.tf.SeasonNum == curSeason && !MainWindow.tf.isBetween))
                {
                    MainWindow.tf = new Timeframe(curSeason);
                    MainWindow.UpdateAllData();
                }

                MainWindow.ChangeSeason(curSeason);

                playersT = MainWindow.playersT;
                pl_playersT = MainWindow.pl_playersT;

                if (cmbPlayer.SelectedIndex != -1)
                {
                    PlayerStats ps = CreatePlayerStatsFromCurrent();

                    teamOrder = MainWindow.TeamOrder;

                    GetActivePlayers();

                    PopulateTeamsCombo();

                    string q = "select * from " + playersT + " where ID = " + ps.ID;
                    q += " AND isHidden LIKE \"False\"";
                    DataTable res = db.GetDataTable(q);

                    if (res.Rows.Count > 0)
                    {
                        bool nowActive = Tools.getBoolean(res.Rows[0], "isActive");
                        string newTeam = nowActive ? res.Rows[0]["TeamFin"].ToString() : " - Inactive -";
                        cmbTeam.SelectedIndex = -1;
                        if (nowActive)
                        {
                            if (newTeam != "")
                            {
                                try
                                {
                                    cmbTeam.SelectedItem = GetDisplayNameFromTeam(newTeam);
                                }
                                catch (Exception)
                                {
                                    cmbTeam.SelectedIndex = -1;
                                    cmbPlayer.SelectedIndex = -1;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            cmbTeam.SelectedItem = "- Inactive -";
                        }
                        cmbPlayer.SelectedIndex = -1;
                        cmbPlayer.SelectedValue = ps.ID;

                        if (cmbOppPlayer.SelectedIndex != -1)
                        {
                            SelectedOppPlayerID = ((KeyValuePair<int, string>) (((cmbOppPlayer)).SelectedItem)).Key;

                            q = "select * from " + playersT + " where ID = " + SelectedOppPlayerID;
                            q += " AND isHidden LIKE \"False\"";
                            res = db.GetDataTable(q);

                            if (res.Rows.Count > 0)
                            {
                                nowActive = Tools.getBoolean(res.Rows[0], "isActive");
                                newTeam = nowActive ? res.Rows[0]["TeamFin"].ToString() : " - Inactive -";
                                cmbOppTeam.SelectedIndex = -1;
                                if (nowActive)
                                {
                                    if (newTeam != "")
                                    {
                                        try
                                        {
                                            cmbOppTeam.SelectedItem = GetDisplayNameFromTeam(newTeam);
                                        }
                                        catch (Exception)
                                        {
                                            cmbOppTeam.SelectedIndex = -1;
                                            cmbOppPlayer.SelectedIndex = -1;
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    cmbOppTeam.SelectedItem = "- Inactive -";
                                }
                                cmbOppPlayer.SelectedIndex = -1;
                                cmbOppPlayer.SelectedValue = SelectedOppPlayerID;
                            }
                            else
                            {
                                cmbOppTeam.SelectedIndex = -1;
                                cmbOppPlayer.SelectedIndex = -1;
                            }
                        }
                    }
                    else
                    {
                        cmbTeam.SelectedIndex = -1;
                        cmbPlayer.SelectedIndex = -1;
                        cmbOppTeam.SelectedIndex = -1;
                        cmbOppPlayer.SelectedIndex = -1;
                    }
                }
                else
                {
                    PopulateTeamsCombo();
                }
            }
        }

        /// <summary>
        ///     Handles the Click event of the btnScoutingReport control.
        ///     Displays a quick overview of the player's performance in a natural language scouting report.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnScoutingReport_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == -1)
                return;

            var temppst = new Dictionary<int, PlayerStats>();
            foreach (var kvp in MainWindow.pst)
            {
                int i = kvp.Key;
                temppst.Add(i, kvp.Value.DeepClone());
                temppst[i].ResetStats();
                temppst[i].AddPlayerStats(MainWindow.pst[i], true);
            }

            PlayerRankings cumRankingsActive = PlayerRankings.CalculateActiveRankings();
            var cumRankingsPosition =
                new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.Position1 == psr.Position1).ToDictionary(r => r.Key, r => r.Value));
            var cumRankingsTeam =
                new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.TeamF == psr.TeamF).ToDictionary(r => r.Key, r => r.Value));

            new PlayerStatsRow(temppst[psr.ID]).ScoutingReport(MainWindow.pst, cumRankingsActive, cumRankingsTeam, cumRankingsPosition,
                                                               pbsList.ToList(), txbGame1.Text);
        }

        /// <summary>
        ///     Handles the Click event of the btnSavePlayer control.
        ///     Saves the current player's stats to the database.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnSavePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == -1)
                return;

            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores" +
                    ") or edit the box-scores themselves.", "NBA Stats Tracker", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PlayerStats ps = CreatePlayerStatsFromCurrent();

            var pslist = new Dictionary<int, PlayerStats> {{ps.ID, ps}};

            SQLiteIO.savePlayersToDatabase(MainWindow.currentDB, pslist, curSeason, maxSeason, true);

            MainWindow.pst = SQLiteIO.GetPlayersFromDatabase(MainWindow.currentDB, MainWindow.tst, MainWindow.tstopp, MainWindow.TeamOrder,
                                                             curSeason, maxSeason);

            GetActivePlayers();
            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedItem = ps.isActive ? GetDisplayNameFromTeam(ps.TeamF) : "- Inactive -";
            cmbPlayer.SelectedIndex = -1;
            cmbPlayer.SelectedValue = ps.ID;
            //cmbPlayer.SelectedValue = ps.LastName + " " + ps.FirstName + " (" + ps.Position1 + ")";
        }

        /// <summary>
        ///     Creates a PlayerStats instance from the currently displayed information and stats.
        /// </summary>
        /// <returns></returns>
        private PlayerStats CreatePlayerStatsFromCurrent()
        {
            if (cmbPosition2.SelectedItem == null)
                cmbPosition2.SelectedItem = " ";

            string TeamF;
            if (chkIsActive.IsChecked.GetValueOrDefault() == false)
            {
                TeamF = "";
            }
            else
            {
                TeamF = GetCurTeamFromDisplayName(cmbTeam.SelectedItem.ToString());
                if (TeamF == "- Inactive -")
                {
                    askedTeam = "";
                    var atw = new ComboChoiceWindow(Teams);
                    atw.ShowDialog();
                    TeamF = askedTeam;
                }
            }

            var ps = new PlayerStats(psr.ID, txtLastName.Text, txtFirstName.Text,
                                     (Position) Enum.Parse(typeof (Position), cmbPosition1.SelectedItem.ToString()),
                                     (Position) Enum.Parse(typeof (Position), cmbPosition2.SelectedItem.ToString()),
                                     Convert.ToInt32(txtYearOfBirth.Text), Convert.ToInt32(txtYearsPro.Text), TeamF, psr.TeamS,
                                     chkIsActive.IsChecked.GetValueOrDefault(), false, chkIsInjured.IsChecked.GetValueOrDefault(),
                                     chkIsAllStar.IsChecked.GetValueOrDefault(), chkIsNBAChampion.IsChecked.GetValueOrDefault(),
                                     dt_ov.Rows[0]);
            ps.height = psr.Height;
            ps.weight = psr.Weight;
            ps.UpdateCareerHighs(recordsList.Single(r => r.Type == "Career"));
            ps.UpdateContract(dgvContract.ItemsSource.Cast<PlayerStatsRow>().First());
            return ps;
        }

        /// <summary>
        ///     Handles the Click event of the btnNext control.
        ///     Switches to the next team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count - 1)
                cmbTeam.SelectedIndex = 0;
            else
                cmbTeam.SelectedIndex++;
        }

        /// <summary>
        ///     Handles the Click event of the btnPrev control.
        ///     Switches to the previous team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex <= 0)
                cmbTeam.SelectedIndex = cmbTeam.Items.Count - 1;
            else
                cmbTeam.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Click event of the btnNextPlayer control.
        ///     Switches to the next player.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex == cmbPlayer.Items.Count - 1)
                cmbPlayer.SelectedIndex = 0;
            else
                cmbPlayer.SelectedIndex++;
        }

        /// <summary>
        ///     Handles the Click event of the btnPrevPlayer control.
        ///     Switches to the previous player.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlayer.SelectedIndex <= 0)
                cmbPlayer.SelectedIndex = cmbPlayer.Items.Count - 1;
            else
                cmbPlayer.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsAllTime control.
        ///     Allows the user to display stats from the whole season.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                MainWindow.tf = new Timeframe(curSeason);
                MainWindow.UpdateAllData();
                cmbSeasonNum_SelectionChanged(null, null);
            }
        }

        /// <summary>
        ///     Handles the Checked event of the rbStatsBetween control.
        ///     Allows the user to display stats between the specified timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!changingTimeframe)
            {
                MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                MainWindow.UpdateAllData();
                cmbPlayer_SelectionChanged(null, null);
            }
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpStart control.
        ///     Makes sure the starting date isn't after the ending date, and updates the player's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                }
                MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                MainWindow.UpdateAllData();
                rbStatsBetween.IsChecked = true;
                changingTimeframe = false;

                cmbPlayer_SelectionChanged(null, null);
            }
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpEnd control.
        ///     Makes sure the starting date isn't after the ending date, and updates the player's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!changingTimeframe)
            {
                changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                }
                MainWindow.tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                MainWindow.UpdateAllData();
                rbStatsBetween.IsChecked = true;
                changingTimeframe = false;

                cmbPlayer_SelectionChanged(null, null);
            }
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbOppTeam control.
        ///     Allows the user to change the opposing team, the players of which can be compared to.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbOppTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == -1)
                return;

            dgvHTH.ItemsSource = null;
            cmbOppPlayer.ItemsSource = null;

            oppPlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            string q;
            if (cmbOppTeam.SelectedItem.ToString() != "- Inactive -")
            {
                q = "select * from " + playersT + " where TeamFin LIKE \"" + cmbOppTeam.SelectedItem + "\" AND isActive LIKE \"True\"";
            }
            else
            {
                q = "select * from " + playersT + " where isActive LIKE \"False\"";
            }
            q += " AND isHidden LIKE \"False\"";
            q += " ORDER BY LastName ASC";
            DataTable res = db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
                oppPlayersList.Add(new KeyValuePair<int, string>(Tools.getInt(r, "ID"),
                                                                 Tools.getString(r, "LastName") + ", " + Tools.getString(r, "FirstName") +
                                                                 " (" + Tools.getString(r, "Position1") + ")"));
            }

            cmbOppPlayer.ItemsSource = oppPlayersList;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbOppPlayer control.
        ///     Allows the user to change the opposing player, to whose stats the current player's stats will be compared.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbOppPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbTeam.SelectedIndex == -1 || cmbOppTeam.SelectedIndex == -1 || cmbPlayer.SelectedIndex == -1 ||
                    cmbOppPlayer.SelectedIndex == -1)
                {
                    dgvHTH.ItemsSource = null;
                    dgvHTHBoxScores.ItemsSource = null;
                    return;
                }
            }
            catch
            {
                return;
            }

            dgvHTH.ItemsSource = null;

            SelectedOppPlayerID = ((KeyValuePair<int, string>) (cmbOppPlayer.SelectedItem)).Key;

            var psrList = new ObservableCollection<PlayerStatsRow>();

            hthAllPBS = new List<PlayerBoxScore>();

            string q;
            DataTable res;

            if (SelectedPlayerID == SelectedOppPlayerID)
                return;

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    psr.Type = psr.FirstName + " " + psr.LastName;
                    psrList.Add(psr);

                    hthOwnPBS = new List<PlayerBoxScore>(pbsList);

                    q = "SELECT * FROM " + playersT + " WHERE ID = " + SelectedOppPlayerID;
                    res = db.GetDataTable(q);

                    var ps = new PlayerStats(res.Rows[0]);
                    var oppPSR = new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName);

                    oppPSR.Type = oppPSR.FirstName + " " + oppPSR.LastName;
                    psrList.Add(oppPSR);

                    q = "select * from PlayerResults INNER JOIN GameResults ON (PlayerResults.GameID = GameResults.GameID) " +
                        "where PlayerID = " + SelectedOppPlayerID + " AND SeasonNum = " + curSeason + " AND isOut = \"False\"";
                    res = db.GetDataTable(q);

                    hthOppPBS = new List<PlayerBoxScore>();
                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r);
                        hthOppPBS.Add(pbs);
                    }
                    var gameIDs = new List<int>();
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
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\"" + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\"" +
                            "AND SeasonNum = {2}) ORDER BY Date DESC", SelectedPlayerID, SelectedOppPlayerID, curSeason);
                    res = db.GetDataTable(q);

                    var p = new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1, psr.Position2);
                    var ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r);
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
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\" " + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\" " +
                            "AND SeasonNum = {2}) ORDER BY Date DESC", SelectedOppPlayerID, SelectedPlayerID, curSeason);
                    res = db.GetDataTable(q);

                    ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r);
                        ps.AddBoxScore(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));
                }
            }
            else
            {
                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    psr.Type = psr.FirstName + " " + psr.LastName;
                    psrList.Add(psr);

                    var gameIDs = new List<int>();
                    foreach (PlayerBoxScore cur in pbsList)
                    {
                        hthAllPBS.Add(cur);
                        gameIDs.Add(cur.GameID);
                    }

                    IEnumerable<PlayerBoxScore> oppPBSList =
                        MainWindow.bshist.Where(bse => bse.pbsList.Any(pbs => pbs.PlayerID == SelectedOppPlayerID))
                                  .Select(bse => bse.pbsList.Single(pbs => pbs.PlayerID == SelectedOppPlayerID));
                    foreach (PlayerBoxScore oppPBS in oppPBSList)
                    {
                        if (!gameIDs.Contains(oppPBS.GameID))
                        {
                            hthAllPBS.Add(oppPBS);
                        }
                    }

                    PlayerStats oppPS = MainWindow.pst[SelectedOppPlayerID];
                    psrList.Add(new PlayerStatsRow(oppPS, oppPS.FirstName + " " + oppPS.LastName));
                }
                else
                {
                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\"" + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\"", SelectedPlayerID,
                            SelectedOppPlayerID);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += ") ORDER BY Date DESC";
                    res = db.GetDataTable(q);

                    var p = new Player(psr.ID, psr.TeamF, psr.LastName, psr.FirstName, psr.Position1, psr.Position2);
                    var ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r);
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
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\" " + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\" ", SelectedOppPlayerID,
                            SelectedPlayerID);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += ") ORDER BY Date DESC";
                    res = db.GetDataTable(q);

                    ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r);
                        ps.AddBoxScore(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));
                }
            }

            hthAllPBS.Sort((pbs1, pbs2) => pbs1.RealDate.CompareTo(pbs2.RealDate));
            hthAllPBS.Reverse();

            dgvHTH.ItemsSource = psrList;
            dgvHTHBoxScores.ItemsSource = hthAllPBS;
            //dgvHTHBoxScores.ItemsSource = new ObservableCollection<PlayerBoxScore>(hthAllPBS);
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvBoxScores control.
        ///     Allows the user to view a specific box score in the Box Score window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (PlayerBoxScore) dgvBoxScores.SelectedItems[0];
                int gameID = row.GameID;

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameID);
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

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvHTHBoxScores control.
        ///     Allows the user to view a specific box score in the Box Score window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvHTHBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                var row = (PlayerBoxScore) dgvHTHBoxScores.SelectedItems[0];
                int gameID = row.GameID;

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameID);
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

        /// <summary>
        ///     Handles the Checked event of the rbHTHStatsAnyone control.
        ///     Used to include all the players' games in the stat calculations, no matter the opponent.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppPlayer_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Checked event of the rbHTHStatsEachOther control.
        ///     Used to include only stats from the games these two players have played against each other.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            cmbOppPlayer_SelectionChanged(null, null);
        }

        /// <summary>
        ///     Handles the Sorting event of the StatColumn control.
        ///     Uses a custom Sorting event handler that sorts a stat in descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void StatColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting(e);
        }

        /// <summary>
        ///     Handles the PreviewKeyDown event of the dgvOverviewStats control.
        ///     Allows the user to paste and import tab-separated values formatted player stats into the current player.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvOverviewStats_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && rbStatsAllTime.IsChecked.GetValueOrDefault())
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
                    }
                }

                CreateViewAndUpdateOverview();

                //btnSavePlayer_Click(null, null);
            }
        }

        /// <summary>
        ///     Tries to change the specified row of the Overview data table using the specified dictionary.
        ///     Used when pasting TSV data from the clipboard.
        /// </summary>
        /// <param name="row">The row of dt_ov to try and change.</param>
        /// <param name="dict">The dictionary containing stat-value pairs.</param>
        private void TryChangeRow(int row, Dictionary<string, string> dict)
        {
            dt_ov.Rows[row].TryChangeValue(dict, "GP", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "GS", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "MINS", typeof (UInt16));
            dt_ov.Rows[row].TryChangeValue(dict, "PTS", typeof (UInt16));
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
        }

        /// <summary>
        ///     Creates a DataView instance based on the dt_ov Overview data table and updates the dgvOverviewStats data context.
        /// </summary>
        private void CreateViewAndUpdateOverview()
        {
            var dv_ov = new DataView(dt_ov) {AllowNew = false, AllowDelete = false};
            dgvOverviewStats.DataContext = dv_ov;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbGraphStat control.
        ///     Calculates and displays the player's performance graph for the newly selected stat.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbGraphStat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbGraphStat.SelectedIndex == -1 || cmbTeam.SelectedIndex == -1 || cmbPlayer.SelectedIndex == -1 || pbsList.Count < 1)
                return;

            var cp = new ChartPrimitive();
            double i = 0;

            string propToGet = cmbGraphStat.SelectedItem.ToString();
            propToGet = propToGet.Replace('3', 'T');
            propToGet = propToGet.Replace('%', 'p');

            double sum = 0;
            double games = 0;

            foreach (PlayerBoxScore pbs in pbsList)
            {
                i++;
                double value = Convert.ToDouble(typeof (PlayerBoxScore).GetProperty(propToGet).GetValue(pbs, null));
                if (!double.IsNaN(value))
                {
                    if (propToGet.Contains("p"))
                        value = Convert.ToDouble(Convert.ToInt32(value*1000))/1000;
                    cp.AddPoint(i, value);
                    games++;
                    sum += value;
                }
            }
            cp.Label = cmbGraphStat.SelectedItem.ToString();
            cp.ShowInLegend = false;
            chart.Primitives.Clear();
            if (cp.Points.Count > 0)
            {
                double average = sum/games;
                var cpavg = new ChartPrimitive();
                for (int j = 1; j <= i; j++)
                {
                    cpavg.AddPoint(j, average);
                }
                cpavg.Color = Color.FromRgb(0, 0, 100);
                cpavg.Dashed = true;
                cpavg.ShowInLegend = false;
                chart.Primitives.Add(cpavg);
                chart.Primitives.Add(cp);
            }
            chart.RedrawPlotLines();
            var cp2 = new ChartPrimitive();
            cp2.AddPoint(1, 0);
            cp2.AddPoint(i, 1);
            chart.Primitives.Add(cp2);
            chart.ResetPanAndZoom();
        }

        /// <summary>
        ///     Populates the graph stat combo.
        /// </summary>
        private void PopulateGraphStatCombo()
        {
            var stats = new List<string>
                        {
                            "GmSc",
                            "GmScE",
                            "PTS",
                            "FGM",
                            "FGA",
                            "FG%",
                            "3PM",
                            "3PA",
                            "3P%",
                            "FTM",
                            "FTA",
                            "FT%",
                            "REB",
                            "OREB",
                            "DREB",
                            "AST",
                            "BLK",
                            "STL",
                            "TO",
                            "FOUL"
                        };

            stats.ForEach(s => cmbGraphStat.Items.Add(s));
        }

        /// <summary>
        ///     Handles the Click event of the btnPrevStat control.
        ///     Switches to the previous graph stat.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevStat_Click(object sender, RoutedEventArgs e)
        {
            if (cmbGraphStat.SelectedIndex == 0)
                cmbGraphStat.SelectedIndex = cmbGraphStat.Items.Count - 1;
            else
                cmbGraphStat.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Click event of the btnNextStat control.
        ///     Switches to the next graph stat.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextStat_Click(object sender, RoutedEventArgs e)
        {
            if (cmbGraphStat.SelectedIndex == cmbGraphStat.Items.Count - 1)
                cmbGraphStat.SelectedIndex = 0;
            else
                cmbGraphStat.SelectedIndex++;
        }

        /// <summary>
        ///     Handles the Click event of the btnPrevOppTeam control.
        ///     Switches to the previous opposing team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevOppTeam_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex <= 0)
                cmbOppTeam.SelectedIndex = cmbOppTeam.Items.Count - 1;
            else
                cmbOppTeam.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Click event of the btnNextOppTeam control.
        ///     Switches to the next opposing team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextOppTeam_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppTeam.SelectedIndex == cmbOppTeam.Items.Count - 1)
                cmbOppTeam.SelectedIndex = 0;
            else
                cmbOppTeam.SelectedIndex++;
        }

        /// <summary>
        ///     Handles the Click event of the btnPrevOppPlayer control.
        ///     Switches to the previous opposing player.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevOppPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppPlayer.SelectedIndex <= 0)
                cmbOppPlayer.SelectedIndex = cmbOppPlayer.Items.Count - 1;
            else
                cmbOppPlayer.SelectedIndex--;
        }

        /// <summary>
        ///     Handles the Click event of the btnNextOppPlayer control.
        ///     Switches to the next opposing player.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextOppPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOppPlayer.SelectedIndex == cmbOppPlayer.Items.Count - 1)
                cmbOppPlayer.SelectedIndex = 0;
            else
                cmbOppPlayer.SelectedIndex++;
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            Misc.SetRegistrySetting("PlayerOvHeight", Height);
            Misc.SetRegistrySetting("PlayerOvWidth", Width);
            Misc.SetRegistrySetting("PlayerOvX", Left);
            Misc.SetRegistrySetting("PlayerOvY", Top);
        }

        private void btnAddPastStats_Click(object sender, RoutedEventArgs e)
        {
            var adw = new AddStatsWindow(false, psr.ID);
            if (adw.ShowDialog() == true)
            {
                UpdateYearlyReport();
            }
        }

        private void btnCopySeasonScouting_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonScoutingReport.Text);
        }

        private void btnCopySeasonFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonFacts.Text);
        }

        private void btnCopyPlayoffsScouting_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsScoutingReport.Text);
        }

        private void btnCopyPlayoffsFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsFacts.Text);
        }

        private void btnCopySeasonLeadersFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonLeadersFacts.Text);
        }

        private void btnCopyPlayoffsLeadersFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsLeadersFacts.Text);
        }
    }
}