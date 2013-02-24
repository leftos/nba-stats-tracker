#region Copyright Notice

//    Copyright 2011-2013 Eleftherios Aslanoglou
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Other;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Players.Injuries;
using NBA_Stats_Tracker.Data.SQLiteIO;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Helper.EventHandlers;
using NBA_Stats_Tracker.Helper.Miscellaneous;
using NBA_Stats_Tracker.Windows.MainInterface.BoxScores;
using NBA_Stats_Tracker.Windows.MainInterface.ToolWindows;
using NBA_Stats_Tracker.Windows.MiscTools;
using SQLite_Database;
using Swordfish.WPF.Charts;

#endregion

namespace NBA_Stats_Tracker.Windows.MainInterface.Players
{
    /// <summary>
    ///     Shows player information and stats.
    /// </summary>
    public partial class PlayerOverviewWindow
    {
        public static string AskedTeam;
        private readonly SQLiteDatabase _db = new SQLiteDatabase(MainWindow.CurrentDB);
        private readonly int _maxSeason = SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);

        private bool _changingTimeframe;
        private PlayerRankings _cumPlayoffsRankingsActive, _cumPlayoffsRankingsPosition, _cumPlayoffsRankingsTeam;
        private PlayerRankings _cumSeasonRankingsActive, _cumSeasonRankingsPosition, _cumSeasonRankingsTeam;
        private int _curSeason = MainWindow.CurSeason;
        private DataTable _dtOv;
        private List<PlayerBoxScore> _hthAllPBS;
        private List<PlayerBoxScore> _hthOppPBS;
        private List<PlayerBoxScore> _hthOwnPBS;

        private ObservableCollection<KeyValuePair<int, string>> _oppPlayersList = new ObservableCollection<KeyValuePair<int, string>>();

        private ObservableCollection<PlayerBoxScore> _pbsList;
        private PlayerStatsRow _plPSR;
        private string _plPlayersT = MainWindow.PlPlayersT;
        private PlayerRankings _plRankingsActive;
        private PlayerRankings _plRankingsPosition;
        private PlayerRankings _plRankingsTeam;

        private Dictionary<int, PlayerStats> _playersActive;
        private Dictionary<int, PlayerStats> _playersSamePosition;
        private Dictionary<int, PlayerStats> _playersSameTeam;
        private string _playersT = MainWindow.PlayersT;
        private PlayerStatsRow _psr;
        private PlayerRankings _rankingsActive;
        private PlayerRankings _rankingsPosition;
        private PlayerRankings _rankingsTeam;
        private int _selectedPlayerID = -1;
        private ObservableCollection<PlayerStatsRow> _splitPSRs;
        private SortedDictionary<string, int> _teamOrder = MainWindow.TeamOrder;
        private List<string> _teams;

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
        public PlayerOverviewWindow(int team, int playerID) : this()
        {
            cmbTeam.SelectedItem = team != -1 ? MainWindow.TST[team].DisplayName : "- Inactive -";
            cmbPlayer.SelectedValue = playerID.ToString();
        }

        private ObservableCollection<PlayerHighsRow> recordsList { get; set; }

        private ObservableCollection<KeyValuePair<int, string>> playersList { get; set; }

        private int selectedOppPlayerID { get; set; }

        /// <summary>
        ///     Finds a team's name by its displayName.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Requested team that is hidden.</exception>
        private int GetTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(MainWindow.TST, displayName);
        }

        /// <summary>
        ///     Populates the teams combo.
        /// </summary>
        private void populateTeamsCombo()
        {
            _teams = new List<string>();
            foreach (var kvp in _teamOrder)
            {
                if (!MainWindow.TST[kvp.Value].IsHidden)
                    _teams.Add(MainWindow.TST[kvp.Value].DisplayName);
            }

            _teams.Sort();

            _teams.Add("- Inactive -");

            cmbTeam.ItemsSource = _teams;
            cmbOppTeam.ItemsSource = _teams;
        }

        /// <summary>
        ///     Prepares the window: populates data tables, sets DataGrid properties, populates combos and calculates metrics.
        /// </summary>
        private void prepareWindow()
        {
            DataContext = this;

            populateSeasonCombo();

            var positions = Enum.GetNames(typeof (Position));
            cmbPosition1.ItemsSource = positions;
            cmbPosition2.ItemsSource = positions;

            populateTeamsCombo();

            _dtOv = new DataTable();
            _dtOv.Columns.Add("Type");
            _dtOv.Columns.Add("GP");
            _dtOv.Columns.Add("GS");
            _dtOv.Columns.Add("MINS");
            _dtOv.Columns.Add("PTS");
            _dtOv.Columns.Add("FG");
            _dtOv.Columns.Add("FGeff");
            _dtOv.Columns.Add("3PT");
            _dtOv.Columns.Add("3Peff");
            _dtOv.Columns.Add("FT");
            _dtOv.Columns.Add("FTeff");
            _dtOv.Columns.Add("REB");
            _dtOv.Columns.Add("OREB");
            _dtOv.Columns.Add("DREB");
            _dtOv.Columns.Add("AST");
            _dtOv.Columns.Add("TO");
            _dtOv.Columns.Add("STL");
            _dtOv.Columns.Add("BLK");
            _dtOv.Columns.Add("FOUL");

            _changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.Tf.EndDate;
            dtpStart.SelectedDate = MainWindow.Tf.StartDate;
            cmbSeasonNum.SelectedItem = MainWindow.SeasonList.Single(pair => pair.Key == MainWindow.Tf.SeasonNum);
            if (MainWindow.Tf.IsBetween)
            {
                rbStatsBetween.IsChecked = true;
            }
            else
            {
                rbStatsAllTime.IsChecked = true;
            }
            _changingTimeframe = false;

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

            chkIsActive.SetBinding(ToggleButton.IsCheckedProperty, new Binding("IsActive") {Source = _psr});

            getActivePlayers();
            populateGraphStatCombo();
        }

        /// <summary>
        ///     Gets a player stats dictionary of only the active players, and calculates their rankingsPerGame.
        /// </summary>
        private void getActivePlayers()
        {
            _playersActive = new Dictionary<int, PlayerStats>();

            var q = "select * from " + _playersT + " where isActive LIKE \"True\"";
            q += " AND isHidden LIKE \"False\"";
            var res = _db.GetDataTable(q);
            foreach (DataRow r in res.Rows)
            {
                var q2 = "select * from " + _plPlayersT + " where ID = " + ParseCell.GetInt32(r, "ID");
                var plRes = _db.GetDataTable(q2);

                var ps = new PlayerStats(r, MainWindow.TST);
                _playersActive.Add(ps.ID, ps);
                _playersActive[ps.ID].UpdatePlayoffStats(plRes.Rows[0]);
            }

            _rankingsActive = new PlayerRankings(_playersActive);
            _plRankingsActive = new PlayerRankings(_playersActive, true);
        }

        /// <summary>
        ///     Populates the season combo.
        /// </summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            cmbSeasonNum.SelectedValue = _curSeason;
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

            playersList = new ObservableCollection<KeyValuePair<int, string>>();
            _playersSameTeam = new Dictionary<int, PlayerStats>();
            if (cmbTeam.SelectedItem.ToString() != "- Inactive -")
            {
                var list =
                    MainWindow.PST.Values.Where(
                        ps => ps.TeamF == GetTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString()) && !ps.IsHidden && ps.IsActive).ToList();
                list.Sort((ps1, ps2) => ps1.LastName.CompareTo(ps2.LastName));
                list.ForEach(delegate(PlayerStats ps)
                             {
                                 playersList.Add(new KeyValuePair<int, string>(ps.ID,
                                                                               String.Format("{0}, {1} ({2})", ps.LastName, ps.FirstName,
                                                                                             ps.Position1.ToString())));
                                 _playersSameTeam.Add(ps.ID, ps);
                             });
            }
            else
            {
                var list = MainWindow.PST.Values.Where(ps => !ps.IsHidden && !ps.IsActive).ToList();
                list.Sort((ps1, ps2) => ps1.LastName.CompareTo(ps2.LastName));
                list.ForEach(delegate(PlayerStats ps)
                             {
                                 playersList.Add(new KeyValuePair<int, string>(ps.ID,
                                                                               String.Format("{0}, {1} ({2})", ps.LastName, ps.FirstName,
                                                                                             ps.Position1.ToString())));
                                 _playersSameTeam.Add(ps.ID, ps);
                             });
            }
            _rankingsTeam = new PlayerRankings(_playersSameTeam);
            _plRankingsTeam = new PlayerRankings(_playersSameTeam, true);

            cmbPlayer.ItemsSource = playersList;
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

            _selectedPlayerID = ((KeyValuePair<int, string>) (((cmbPlayer)).SelectedItem)).Key;

            _pbsList = new ObservableCollection<PlayerBoxScore>();

            var q = "select * from " + _playersT + " where ID = " + _selectedPlayerID.ToString();
            var res = _db.GetDataTable(q);

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
            _psr = new PlayerStatsRow(MainWindow.PST[_selectedPlayerID]);
            _plPSR = new PlayerStatsRow(MainWindow.PST[_selectedPlayerID], true);

            updateOverviewAndBoxScores();

            updateSplitStats();

            updateYearlyReport();

            _cumSeasonRankingsActive = PlayerRankings.CalculateActiveRankings();
            _cumSeasonRankingsPosition =
                new PlayerRankings(MainWindow.PST.Where(ps => ps.Value.Position1 == _psr.Position1).ToDictionary(r => r.Key, r => r.Value));
            _cumSeasonRankingsTeam =
                new PlayerRankings(MainWindow.PST.Where(ps => ps.Value.TeamF == _psr.TeamF).ToDictionary(r => r.Key, r => r.Value));

            _cumPlayoffsRankingsActive = PlayerRankings.CalculateActiveRankings(true);
            _cumPlayoffsRankingsPosition =
                new PlayerRankings(MainWindow.PST.Where(ps => ps.Value.Position1 == _psr.Position1).ToDictionary(r => r.Key, r => r.Value),
                                   true);
            _cumPlayoffsRankingsTeam =
                new PlayerRankings(MainWindow.PST.Where(ps => ps.Value.TeamF == _psr.TeamF).ToDictionary(r => r.Key, r => r.Value), true);

            updateScoutingReport();

            updateRecords();

            cmbOppPlayer_SelectionChanged(null, null);

            if (cmbGraphStat.SelectedIndex == -1)
                cmbGraphStat.SelectedIndex = 0;
        }

        private List<string> GetFacts(int id, PlayerRankings rankings, bool onlyLeaders = false)
        {
            var leadersStats = new List<int> {PAbbr.PPG, PAbbr.FGp, PAbbr.TPp, PAbbr.FTp, PAbbr.RPG, PAbbr.SPG, PAbbr.APG, PAbbr.BPG};
            var count = 0;
            var facts = new List<string>();
            for (var i = 0; i < rankings.RankingsPerGame[id].Length; i++)
            {
                if (onlyLeaders)
                {
                    if (!leadersStats.Contains(i))
                        continue;
                }
                var rank = rankings.RankingsPerGame[id][i];
                if (rank <= 20)
                {
                    var fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), PAbbr.PerGame[i]);
                    if (PAbbr.PerGame[i].EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", MainWindow.PST[id].PerGame[i]);
                    }
                    else if (PAbbr.PerGame[i].EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2} ", MainWindow.PST[id].PerGame[i]);
                        switch (PAbbr.PerGame[i].Substring(0, 2))
                        {
                            case "FG":
                                fact += String.Format("({0:F1} FGM/G on {1:F3})",
                                                      ((double) MainWindow.PST[id].Totals[PAbbr.FGM])/MainWindow.PST[id].Totals[PAbbr.GP],
                                                      MainWindow.PST[id].PerGame[PAbbr.FGp]);
                                break;
                            case "3P":
                                fact += String.Format("({0:F1} 3PM/G on {1:F3})",
                                                      ((double) MainWindow.PST[id].Totals[PAbbr.TPM])/MainWindow.PST[id].Totals[PAbbr.GP],
                                                      MainWindow.PST[id].PerGame[PAbbr.TPp]);
                                break;
                            case "FT":
                                fact += String.Format("({0:F1} FTM/G on {1:F3})",
                                                      ((double) MainWindow.PST[id].Totals[PAbbr.FTM])/MainWindow.PST[id].Totals[PAbbr.GP],
                                                      MainWindow.PST[id].PerGame[PAbbr.FTp]);
                                break;
                        }
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", MainWindow.PST[id].PerGame[i]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            var metricsToSkip = new List<string> {"aPER", "uPER"};
            for (var i = 0; i < rankings.RankingsMetrics[id].Keys.Count; i++)
            {
                var metricName = rankings.RankingsMetrics[id].Keys.ToList()[i];
                if (metricsToSkip.Contains(metricName))
                    continue;

                var rank = rankings.RankingsMetrics[id][metricName];
                if (rank <= 20)
                {
                    var fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), metricName.Replace("p", "%"));
                    if (metricName.EndsWith("p") || metricName.EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", MainWindow.PST[id].Metrics[metricName]);
                    }
                    else if (metricName.EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2}", MainWindow.PST[id].Metrics[metricName]);
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", MainWindow.PST[id].Metrics[metricName]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            if (!onlyLeaders)
            {
                for (var i = 0; i < rankings.RankingsTotal[id].Length; i++)
                {
                    var rank = rankings.RankingsTotal[id][i];
                    if (rank <= 20)
                    {
                        var fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), PAbbr.Totals[i]);
                        fact += String.Format("{0}", MainWindow.PST[id].Totals[i]);
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

        private void updateScoutingReport()
        {
            var id = _selectedPlayerID;

            if (MainWindow.PST[id].Totals[PAbbr.GP] > 0)
            {
                grpSeasonScoutingReport.Visibility = Visibility.Visible;
                grpSeasonFacts.Visibility = Visibility.Visible;
                grpSeasonLeadersFacts.Visibility = Visibility.Visible;

                var msg = new PlayerStatsRow(MainWindow.PST[id], false, false).ScoutingReport(MainWindow.PST, _cumSeasonRankingsActive,
                                                                                              _cumSeasonRankingsTeam,
                                                                                              _cumSeasonRankingsPosition, _pbsList,
                                                                                              txbGame1.Text);
                txbSeasonScoutingReport.Text = msg;

                var facts = GetFacts(id, _cumSeasonRankingsActive);
                txbSeasonFacts.Text = aggregateFacts(facts);
                if (facts.Count == 0)
                    grpSeasonFacts.Visibility = Visibility.Collapsed;

                facts = GetFacts(id, MainWindow.SeasonLeadersRankings, true);
                txbSeasonLeadersFacts.Text = aggregateFacts(facts);
                if (facts.Count == 0)
                    grpSeasonLeadersFacts.Visibility = Visibility.Collapsed;
            }
            else
            {
                grpSeasonScoutingReport.Visibility = Visibility.Collapsed;
                grpSeasonFacts.Visibility = Visibility.Collapsed;
                grpSeasonLeadersFacts.Visibility = Visibility.Collapsed;
            }

            if (MainWindow.PST[id].PlTotals[PAbbr.GP] > 0)
            {
                grpPlayoffsScoutingReport.Visibility = Visibility.Visible;
                grpPlayoffsFacts.Visibility = Visibility.Visible;
                grpPlayoffsLeadersFacts.Visibility = Visibility.Visible;

                var msg = new PlayerStatsRow(MainWindow.PST[id], true, false).ScoutingReport(MainWindow.PST, _cumPlayoffsRankingsActive,
                                                                                             _cumPlayoffsRankingsTeam,
                                                                                             _cumPlayoffsRankingsPosition, _pbsList,
                                                                                             txbGame1.Text);
                txbPlayoffsScoutingReport.Text = msg;

                var facts = GetFacts(id, _cumPlayoffsRankingsActive);
                txbPlayoffsFacts.Text = aggregateFacts(facts);
                if (facts.Count == 0)
                    grpPlayoffsFacts.Visibility = Visibility.Collapsed;

                facts = GetFacts(id, MainWindow.PlayoffsLeadersRankings, true);
                txbPlayoffsLeadersFacts.Text = aggregateFacts(facts);
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

        private static string aggregateFacts(List<string> facts)
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

        private void updateRecords()
        {
            //MainWindow.pst[SelectedPlayerID].CalculateSeasonHighs();

            recordsList = new ObservableCollection<PlayerHighsRow>();
            var shList = MainWindow.SeasonHighs.Single(sh => sh.Key == _selectedPlayerID).Value;
            foreach (var shRec in shList)
            {
                recordsList.Add(new PlayerHighsRow(_selectedPlayerID, "Season " + MainWindow.GetSeasonName(shRec.Key), shRec.Value));
            }
            recordsList.Add(new PlayerHighsRow(_selectedPlayerID, "Career", MainWindow.PST[_selectedPlayerID].CareerHighs));

            dgvHighs.ItemsSource = null;
            dgvHighs.ItemsSource = recordsList;

            dgvContract.ItemsSource = new ObservableCollection<PlayerStatsRow> {_psr};
        }

        /// <summary>
        ///     Updates the tab viewing the year-by-year overview of the player's stats.
        /// </summary>
        private void updateYearlyReport()
        {
            var psrList = new List<PlayerStatsRow>();
            var psCareer = new PlayerStats(new Player(_psr.ID, _psr.TeamF, _psr.LastName, _psr.FirstName, _psr.Position1, _psr.Position2));

            var qr = "SELECT * FROM PastPlayerStats WHERE PlayerID = " + _psr.ID + " ORDER BY CAST(\"SOrder\" AS INTEGER)";
            var dt = _db.GetDataTable(qr);
            foreach (DataRow dr in dt.Rows)
            {
                var ps = new PlayerStats();
                var isPlayoff = ParseCell.GetBoolean(dr, "isPlayoff");
                ps.GetStatsFromDataRow(dr, isPlayoff);
                var tempMetrics = isPlayoff ? ps.PlMetrics : ps.Metrics;
                PlayerStats.CalculateRates(isPlayoff ? ps.PlTotals : ps.Totals, ref tempMetrics);
                var type = isPlayoff
                               ? "Playoffs " + ParseCell.GetString(dr, "SeasonName")
                               : "Season " + ParseCell.GetString(dr, "SeasonName");
                var curPSR = new PlayerStatsRow(ps, type, isPlayoff)
                             {
                                 TeamFDisplay = ParseCell.GetString(dr, "TeamFin"),
                                 TeamSDisplay = ParseCell.GetString(dr, "TeamSta")
                             };

                psrList.Add(curPSR);

                psCareer.AddPlayerStats(ps);
            }

            for (var i = 1; i <= _maxSeason; i++)
            {
                var displayNames = new Dictionary<int, string>();
                SQLiteIO.GetSeasonDisplayNames(MainWindow.CurrentDB, i, ref displayNames);
                var pT = "Players";
                if (i != _maxSeason)
                    pT += "S" + i;

                var q = "select * from " + pT + " where ID = " + _selectedPlayerID;
                var res = _db.GetDataTable(q);
                if (res.Rows.Count == 1)
                {
                    var ps = new PlayerStats(res.Rows[0], MainWindow.TST);
                    PlayerStats.CalculateRates(ps.Totals, ref ps.Metrics);
                    var psr2 = new PlayerStatsRow(ps, "Season " + MainWindow.GetSeasonName(i));
                    psr2.TeamFDisplay = Misc.GetDisplayName(displayNames, psr2.TeamF);
                    psr2.TeamSDisplay = Misc.GetDisplayName(displayNames, psr2.TeamS);
                    psrList.Add(psr2);
                    psCareer.AddPlayerStats(ps);
                }

                pT = "PlayoffPlayers";
                if (i != _maxSeason)
                    pT += "S" + i;

                q = "select * from " + pT + " where ID = " + _selectedPlayerID;
                res = _db.GetDataTable(q);
                if (res.Rows.Count == 1)
                {
                    var ps = new PlayerStats(res.Rows[0], MainWindow.TST, true);
                    if (ps.PlTotals[PAbbr.GP] > 0)
                    {
                        PlayerStats.CalculateRates(ps.PlTotals, ref ps.PlMetrics);
                        var psr2 = new PlayerStatsRow(ps, "Playoffs " + MainWindow.GetSeasonName(i), true);
                        psr2.TeamFDisplay = Misc.GetDisplayName(displayNames, psr2.TeamF);
                        psr2.TeamSDisplay = Misc.GetDisplayName(displayNames, psr2.TeamS);
                        psrList.Add(psr2);
                        psCareer.AddPlayerStats(ps, true);
                    }
                }
            }

            PlayerStats.CalculateRates(psCareer.Totals, ref psCareer.Metrics);
            psrList.Add(new PlayerStatsRow(psCareer, "Career", "Career"));

            var psrListCollection = new ListCollectionView(psrList);
            Debug.Assert(psrListCollection.GroupDescriptions != null, "psrListCollection.GroupDescriptions != null");
            psrListCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            dgvYearly.ItemsSource = psrListCollection;
        }

        /// <summary>
        ///     Updates the overview tab and prepares the available box scores for the current timeframe.
        /// </summary>
        private void updateOverviewAndBoxScores()
        {
            var ts = _psr.IsActive ? MainWindow.TST[_psr.TeamF] : new TeamStats();
            var tsopp = new TeamStats(ts.ID, "Opponents");

            grdOverview.DataContext = _psr;

            _playersSamePosition =
                MainWindow.PST.Where(ps => ps.Value.Position1 == _psr.Position1 && ps.Value.IsActive)
                          .ToDictionary(ps => ps.Value.ID, ps => ps.Value);

            _rankingsPosition = new PlayerRankings(_playersSamePosition);
            _plRankingsPosition = new PlayerRankings(_playersSamePosition, true);

            foreach (var bse in MainWindow.BSHist.Where(bse => bse.PBSList.Any(pbs => pbs.PlayerID == _psr.ID && !pbs.IsOut)).ToList())
            {
                var pbs = new PlayerBoxScore();
                pbs = bse.PBSList.Single(pbs1 => pbs1.PlayerID == _psr.ID);
                pbs.AddInfoFromTeamBoxScore(MainWindow.TST, bse.BS);
                pbs.CalcMetrics(bse.BS);
                _pbsList.Add(pbs);
            }

            cmbPosition1.SelectedItem = _psr.Position1.ToString();
            cmbPosition2.SelectedItem = _psr.Position2.ToString();

            _dtOv.Clear();

            var dr = _dtOv.NewRow();

            dr["Type"] = "Stats";
            dr["GP"] = _psr.GP.ToString();
            dr["GS"] = _psr.GS.ToString();
            dr["MINS"] = _psr.MINS.ToString();
            dr["PTS"] = _psr.PTS.ToString();
            dr["FG"] = _psr.FGM.ToString() + "-" + _psr.FGA.ToString();
            dr["3PT"] = _psr.TPM.ToString() + "-" + _psr.TPA.ToString();
            dr["FT"] = _psr.FTM.ToString() + "-" + _psr.FTA.ToString();
            dr["REB"] = (_psr.DREB + _psr.OREB).ToString();
            dr["OREB"] = _psr.OREB.ToString();
            dr["DREB"] = _psr.DREB.ToString();
            dr["AST"] = _psr.AST.ToString();
            dr["TO"] = _psr.TOS.ToString();
            dr["STL"] = _psr.STL.ToString();
            dr["BLK"] = _psr.BLK.ToString();
            dr["FOUL"] = _psr.FOUL.ToString();

            _dtOv.Rows.Add(dr);

            dr = _dtOv.NewRow();

            dr["Type"] = "Averages";
            dr["MINS"] = String.Format("{0:F1}", _psr.MPG);
            dr["PTS"] = String.Format("{0:F1}", _psr.PPG);
            dr["FG"] = String.Format("{0:F3}", _psr.FGp);
            dr["FGeff"] = String.Format("{0:F2}", _psr.FGeff);
            dr["3PT"] = String.Format("{0:F3}", _psr.TPp);
            dr["3Peff"] = String.Format("{0:F2}", _psr.TPeff);
            dr["FT"] = String.Format("{0:F3}", _psr.FTp);
            dr["FTeff"] = String.Format("{0:F2}", _psr.FTeff);
            dr["REB"] = String.Format("{0:F1}", _psr.RPG);
            dr["OREB"] = String.Format("{0:F1}", _psr.ORPG);
            dr["DREB"] = String.Format("{0:F1}", _psr.DRPG);
            dr["AST"] = String.Format("{0:F1}", _psr.APG);
            dr["TO"] = String.Format("{0:F1}", _psr.TPG);
            dr["STL"] = String.Format("{0:F1}", _psr.SPG);
            dr["BLK"] = String.Format("{0:F1}", _psr.BPG);
            dr["FOUL"] = String.Format("{0:F1}", _psr.FPG);

            _dtOv.Rows.Add(dr);

            #region Rankings

            if (_psr.IsActive)
            {
                var id = _selectedPlayerID;

                dr = _dtOv.NewRow();

                dr["Type"] = "Rankings";
                dr["MINS"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.MPG]);
                dr["PTS"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.PPG]);
                dr["FG"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.FGp]);
                dr["FGeff"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.FGeff]);
                dr["3PT"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.TPp]);
                dr["3Peff"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.TPeff]);
                dr["FT"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.FTp]);
                dr["FTeff"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.FTeff]);
                dr["REB"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.RPG]);
                dr["OREB"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.ORPG]);
                dr["DREB"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.DRPG]);
                dr["AST"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][TAbbr.PAPG]);
                dr["TO"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.TPG]);
                dr["STL"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.SPG]);
                dr["BLK"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.BPG]);
                dr["FOUL"] = String.Format("{0}", _rankingsActive.RankingsPerGame[id][PAbbr.FPG]);

                _dtOv.Rows.Add(dr);

                dr = _dtOv.NewRow();

                dr["Type"] = "In-team Rankings";
                dr["MINS"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.MPG]);
                dr["PTS"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.PPG]);
                dr["FG"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.FGp]);
                dr["FGeff"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.FGeff]);
                dr["3PT"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.TPp]);
                dr["3Peff"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.TPeff]);
                dr["FT"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.FTp]);
                dr["FTeff"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.FTeff]);
                dr["REB"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.RPG]);
                dr["OREB"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.ORPG]);
                dr["DREB"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.DRPG]);
                dr["AST"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][TAbbr.PAPG]);
                dr["TO"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.TPG]);
                dr["STL"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.SPG]);
                dr["BLK"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.BPG]);
                dr["FOUL"] = String.Format("{0}", _rankingsTeam.RankingsPerGame[id][PAbbr.FPG]);

                _dtOv.Rows.Add(dr);

                dr = _dtOv.NewRow();

                dr["Type"] = "Position Rankings";
                dr["MINS"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.MPG]);
                dr["PTS"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.PPG]);
                dr["FG"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.FGp]);
                dr["FGeff"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.FGeff]);
                dr["3PT"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.TPp]);
                dr["3Peff"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.TPeff]);
                dr["FT"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.FTp]);
                dr["FTeff"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.FTeff]);
                dr["REB"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.RPG]);
                dr["OREB"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.ORPG]);
                dr["DREB"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.DRPG]);
                dr["AST"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][TAbbr.PAPG]);
                dr["TO"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.TPG]);
                dr["STL"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.SPG]);
                dr["BLK"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.BPG]);
                dr["FOUL"] = String.Format("{0}", _rankingsPosition.RankingsPerGame[id][PAbbr.FPG]);

                _dtOv.Rows.Add(dr);

                dr = _dtOv.NewRow();

                dr["Type"] = "Team Avg";
                dr["PTS"] = String.Format("{0:F1}", ts.PerGame[TAbbr.PPG]);
                dr["FG"] = String.Format("{0:F3}", ts.PerGame[TAbbr.FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.PerGame[TAbbr.TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.PerGame[TAbbr.FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.PerGame[TAbbr.FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.PerGame[TAbbr.RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.PerGame[TAbbr.ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.PerGame[TAbbr.DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.PerGame[TAbbr.APG]);
                dr["TO"] = String.Format("{0:F1}", ts.PerGame[TAbbr.TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.PerGame[TAbbr.SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.PerGame[TAbbr.BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.PerGame[TAbbr.FPG]);

                _dtOv.Rows.Add(dr);
            }

            #endregion

            dr = _dtOv.NewRow();

            dr["Type"] = " ";

            _dtOv.Rows.Add(dr);

            #region Playoffs

            dr = _dtOv.NewRow();

            dr["Type"] = "Pl Stats";
            dr["GP"] = _plPSR.GP.ToString();
            dr["GS"] = _plPSR.GS.ToString();
            dr["MINS"] = _plPSR.MINS.ToString();
            dr["PTS"] = _plPSR.PTS.ToString();
            dr["FG"] = _plPSR.FGM.ToString() + "-" + _plPSR.FGA.ToString();
            dr["3PT"] = _plPSR.TPM.ToString() + "-" + _plPSR.TPA.ToString();
            dr["FT"] = _plPSR.FTM.ToString() + "-" + _plPSR.FTA.ToString();
            dr["REB"] = (_plPSR.DREB + _plPSR.OREB).ToString();
            dr["OREB"] = _plPSR.OREB.ToString();
            dr["DREB"] = _plPSR.DREB.ToString();
            dr["AST"] = _plPSR.AST.ToString();
            dr["TO"] = _plPSR.TOS.ToString();
            dr["STL"] = _plPSR.STL.ToString();
            dr["BLK"] = _plPSR.BLK.ToString();
            dr["FOUL"] = _plPSR.FOUL.ToString();

            _dtOv.Rows.Add(dr);

            dr = _dtOv.NewRow();

            dr["Type"] = "Pl Avg";
            dr["MINS"] = String.Format("{0:F1}", _plPSR.MPG);
            dr["PTS"] = String.Format("{0:F1}", _plPSR.PPG);
            dr["FG"] = String.Format("{0:F3}", _plPSR.FGp);
            dr["FGeff"] = String.Format("{0:F2}", _plPSR.FGeff);
            dr["3PT"] = String.Format("{0:F3}", _plPSR.TPp);
            dr["3Peff"] = String.Format("{0:F2}", _plPSR.TPeff);
            dr["FT"] = String.Format("{0:F3}", _plPSR.FTp);
            dr["FTeff"] = String.Format("{0:F2}", _plPSR.FTeff);
            dr["REB"] = String.Format("{0:F1}", _plPSR.RPG);
            dr["OREB"] = String.Format("{0:F1}", _plPSR.ORPG);
            dr["DREB"] = String.Format("{0:F1}", _plPSR.DRPG);
            dr["AST"] = String.Format("{0:F1}", _plPSR.APG);
            dr["TO"] = String.Format("{0:F1}", _plPSR.TPG);
            dr["STL"] = String.Format("{0:F1}", _plPSR.SPG);
            dr["BLK"] = String.Format("{0:F1}", _plPSR.BPG);
            dr["FOUL"] = String.Format("{0:F1}", _plPSR.FPG);

            _dtOv.Rows.Add(dr);

            #region Rankings

            if (_psr.IsActive)
            {
                var id = Convert.ToInt32(_selectedPlayerID);

                dr = _dtOv.NewRow();

                dr["Type"] = "Pl Rank";
                dr["MINS"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.MPG]);
                dr["PTS"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.PPG]);
                dr["FG"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.FGp]);
                dr["FGeff"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.FGeff]);
                dr["3PT"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.TPp]);
                dr["3Peff"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.TPeff]);
                dr["FT"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.FTp]);
                dr["FTeff"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.FTeff]);
                dr["REB"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.RPG]);
                dr["OREB"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.ORPG]);
                dr["DREB"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.DRPG]);
                dr["AST"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][TAbbr.PAPG]);
                dr["TO"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.TPG]);
                dr["STL"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.SPG]);
                dr["BLK"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.BPG]);
                dr["FOUL"] = String.Format("{0}", _plRankingsActive.RankingsPerGame[id][PAbbr.FPG]);

                _dtOv.Rows.Add(dr);

                dr = _dtOv.NewRow();

                dr["Type"] = "Pl In-Team";
                dr["MINS"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.MPG]);
                dr["PTS"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.PPG]);
                dr["FG"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.FGp]);
                dr["FGeff"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.FGeff]);
                dr["3PT"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.TPp]);
                dr["3Peff"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.TPeff]);
                dr["FT"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.FTp]);
                dr["FTeff"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.FTeff]);
                dr["REB"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.RPG]);
                dr["OREB"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.ORPG]);
                dr["DREB"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.DRPG]);
                dr["AST"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][TAbbr.PAPG]);
                dr["TO"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.TPG]);
                dr["STL"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.SPG]);
                dr["BLK"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.BPG]);
                dr["FOUL"] = String.Format("{0}", _plRankingsTeam.RankingsPerGame[id][PAbbr.FPG]);

                _dtOv.Rows.Add(dr);

                dr = _dtOv.NewRow();

                dr["Type"] = "Pl Position";
                dr["MINS"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.MPG]);
                dr["PTS"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.PPG]);
                dr["FG"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.FGp]);
                dr["FGeff"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.FGeff]);
                dr["3PT"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.TPp]);
                dr["3Peff"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.TPeff]);
                dr["FT"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.FTp]);
                dr["FTeff"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.FTeff]);
                dr["REB"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.RPG]);
                dr["OREB"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.ORPG]);
                dr["DREB"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.DRPG]);
                dr["AST"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][TAbbr.PAPG]);
                dr["TO"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.TPG]);
                dr["STL"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.SPG]);
                dr["BLK"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.BPG]);
                dr["FOUL"] = String.Format("{0}", _plRankingsPosition.RankingsPerGame[id][PAbbr.FPG]);

                _dtOv.Rows.Add(dr);

                dr = _dtOv.NewRow();

                dr["Type"] = "Pl Team Avg";
                dr["PTS"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.PPG]);
                dr["FG"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.FGp]);
                dr["FGeff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.FGeff]);
                dr["3PT"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.TPp]);
                dr["3Peff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.TPeff]);
                dr["FT"] = String.Format("{0:F3}", ts.PlPerGame[TAbbr.FTp]);
                dr["FTeff"] = String.Format("{0:F2}", ts.PlPerGame[TAbbr.FTeff]);
                dr["REB"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.RPG]);
                dr["OREB"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.ORPG]);
                dr["DREB"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.DRPG]);
                dr["AST"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.APG]);
                dr["TO"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.TPG]);
                dr["STL"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.SPG]);
                dr["BLK"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.BPG]);
                dr["FOUL"] = String.Format("{0:F1}", ts.PlPerGame[TAbbr.FPG]);

                _dtOv.Rows.Add(dr);
            }

            #endregion

            #endregion

            var dvOv = new DataView(_dtOv) {AllowNew = false};

            dgvOverviewStats.DataContext = dvOv;

            #region Prepare Box Scores

            dgvBoxScores.ItemsSource = _pbsList;
            updateBest();
            cmbGraphStat_SelectionChanged(null, null);

            #endregion
        }

        /// <summary>
        ///     Updates the best performances tab with the player's best performances and the most significant stats of each one for the current timeframe.
        /// </summary>
        private void updateBest()
        {
            txbGame1.Text = "";
            txbGame2.Text = "";
            txbGame3.Text = "";
            txbGame4.Text = "";
            txbGame5.Text = "";
            txbGame6.Text = "";

            try
            {
                var templist = _pbsList.ToList();
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

                var psr1 = templist[0];
                var text = psr1.GetBestStats(5, _psr.Position1);
                txbGame1.Text = "1: " + psr1.Date + " vs " + MainWindow.DisplayNames[psr1.OppTeamID] + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[1];
                text = psr1.GetBestStats(5, _psr.Position1);
                txbGame2.Text = "2: " + psr1.Date + " vs " + MainWindow.DisplayNames[psr1.OppTeamID] + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[2];
                text = psr1.GetBestStats(5, _psr.Position1);
                txbGame3.Text = "3: " + psr1.Date + " vs " + MainWindow.DisplayNames[psr1.OppTeamID] + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[3];
                text = psr1.GetBestStats(5, _psr.Position1);
                txbGame4.Text = "4: " + psr1.Date + " vs " + MainWindow.DisplayNames[psr1.OppTeamID] + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[4];
                text = psr1.GetBestStats(5, _psr.Position1);
                txbGame5.Text = "5: " + psr1.Date + " vs " + MainWindow.DisplayNames[psr1.OppTeamID] + " (" + psr1.Result + ")\n\n" + text;

                psr1 = templist[5];
                text = psr1.GetBestStats(5, _psr.Position1);
                txbGame6.Text = "6: " + psr1.Date + " vs " + MainWindow.DisplayNames[psr1.OppTeamID] + " (" + psr1.Result + ")\n\n" + text;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Updates the split stats tab for the current timeframe.
        /// </summary>
        private void updateSplitStats()
        {
            _splitPSRs = new ObservableCollection<PlayerStatsRow>();
            var split = MainWindow.SplitPlayerStats;
            //Home
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Home"], "Home"));

            //Away
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Away"], "Away"));

            //Wins
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Wins"], "Wins", "Result"));

            //Losses
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Losses"], "Losses", "Result"));

            //Season
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Season"], "Season", "Part of Season"));

            //Playoffs
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Playoffs"], "Playoffs", "Part of Season"));

            #region Each Team Played In Stats

            foreach (var ss in split[_psr.ID].Where(pair => pair.Key.StartsWith("with ")))
            {
                _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID][ss.Key], ss.Key, "Team Played For"));
            }

            #endregion

            #region Opponents

            foreach (var ss in
                split[_psr.ID].Where(
                    pair => pair.Key.StartsWith("vs ") && pair.Key.Substring(3, 3) != ">= " && pair.Key.Substring(3, 2) != "< "))
            {
                _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID][ss.Key], ss.Key, "Team Played Against"));
            }

            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["vs >= .500"], "vs >= .500", "Opp Win %"));
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["vs < .500"], "vs < .500", "Opp Win %"));

            #endregion

            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Last 10"], "Last 10", "IsLast10"));
            _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID]["Before"], "Before", "IsLast10"));

            #region Monthly Split Stats

            foreach (var ss in split[_psr.ID].Where(pair => pair.Key.StartsWith("M ")))
            {
                _splitPSRs.Add(new PlayerStatsRow(split[_psr.ID][ss.Key], ss.Key.Substring(2), "Monthly"));
            }

            #endregion

            var splitPSRsCollection = new ListCollectionView(_splitPSRs);
            Debug.Assert(splitPSRsCollection.GroupDescriptions != null, "splitPSRsCollection.GroupDescriptions != null");
            splitPSRsCollection.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            dgvSplitStats.ItemsSource = splitPSRsCollection;
        }

        private void updateData()
        {
            IsEnabled = false;
            MainWindow.UpdateAllData(true).ContinueWith(t => refresh(), MainWindow.MWInstance.UIScheduler);
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
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                rbStatsAllTime.IsChecked = true;
                _changingTimeframe = false;

                try
                {
                    _curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;
                }
                catch (Exception)
                {
                    return;
                }

                if (!(MainWindow.Tf.SeasonNum == _curSeason && !MainWindow.Tf.IsBetween))
                {
                    MainWindow.Tf = new Timeframe(_curSeason);
                    updateData();
                }
            }
        }

        private void refresh()
        {
            PlayerStats ps;

            MainWindow.ChangeSeason(_curSeason);

            _playersT = MainWindow.PlayersT;
            _plPlayersT = MainWindow.PlPlayersT;

            populateTeamsCombo();

            if (cmbPlayer.SelectedIndex != -1)
            {
                ps = createPlayerStatsFromCurrent();

                var oldOwn = ps.ID;
                var oldOpp = selectedOppPlayerID;
                cmbTeam.SelectedIndex = -1;
                cmbPlayer.SelectedIndex = -1;
                cmbOppTeam.SelectedIndex = -1;
                cmbOppPlayer.SelectedIndex = -1;

                try
                {
                    _teamOrder = MainWindow.TeamOrder;

                    getActivePlayers();

                    var newps = MainWindow.PST[oldOwn];
                    if (newps.IsActive)
                    {
                        try
                        {
                            cmbTeam.SelectedIndex = -1;
                            cmbTeam.SelectedItem = MainWindow.TST[newps.TeamF].DisplayName;
                        }
                        catch (Exception)
                        {
                            cmbTeam.SelectedIndex = -1;
                            cmbPlayer.SelectedIndex = -1;
                            cmbOppTeam.SelectedIndex = -1;
                            cmbOppPlayer.SelectedIndex = -1;
                            throw;
                        }
                    }
                    else
                    {
                        cmbTeam.SelectedItem = "- Inactive -";
                    }
                    cmbPlayer.SelectedIndex = -1;
                    cmbPlayer.SelectedValue = newps.ID;

                    if (oldOpp != -1)
                    {
                        var newOpp = MainWindow.PST[oldOpp];
                        if (newOpp.IsActive)
                        {
                            try
                            {
                                cmbOppTeam.SelectedIndex = -1;
                                cmbOppTeam.SelectedItem = MainWindow.TST[newOpp.TeamF].DisplayName;
                            }
                            catch (Exception)
                            {
                                cmbOppTeam.SelectedIndex = -1;
                                cmbOppPlayer.SelectedIndex = -1;
                                throw;
                            }
                        }
                        else
                        {
                            cmbOppTeam.SelectedItem = "- Inactive -";
                        }
                        cmbOppPlayer.SelectedIndex = -1;
                        cmbOppPlayer.SelectedValue = newOpp.ID;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to find player again: " + ex.Message);
                }
            }
            else
            {
                cmbTeam.SelectedIndex = -1;
                cmbPlayer.SelectedIndex = -1;
                cmbOppTeam.SelectedIndex = -1;
                cmbOppPlayer.SelectedIndex = -1;
            }

            try
            {
                ProgressWindow.PwInstance.CanClose = true;
                ProgressWindow.PwInstance.Close();
            }
            catch
            {
                Console.WriteLine("ProgressWindow couldn't be closed; maybe it wasn't open.");
            }
            IsEnabled = true;
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
            foreach (var kvp in MainWindow.PST)
            {
                var i = kvp.Key;
                temppst.Add(i, kvp.Value.DeepClone());
                temppst[i].ResetStats();
                temppst[i].AddPlayerStats(MainWindow.PST[i], true);
            }

            var cumRankingsActive = PlayerRankings.CalculateActiveRankings();
            var cumRankingsPosition =
                new PlayerRankings(MainWindow.PST.Where(ps => ps.Value.Position1 == _psr.Position1).ToDictionary(r => r.Key, r => r.Value));
            var cumRankingsTeam =
                new PlayerRankings(MainWindow.PST.Where(ps => ps.Value.TeamF == _psr.TeamF).ToDictionary(r => r.Key, r => r.Value));

            new PlayerStatsRow(temppst[_psr.ID]).ScoutingReport(MainWindow.PST, cumRankingsActive, cumRankingsTeam, cumRankingsPosition,
                                                                _pbsList.ToList(), txbGame1.Text);
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

            var ps = createPlayerStatsFromCurrent();

            var pslist = new Dictionary<int, PlayerStats> {{ps.ID, ps}};

            SQLiteIO.SavePlayersToDatabase(MainWindow.CurrentDB, pslist, _curSeason, _maxSeason, true);

            MainWindow.PST = SQLiteIO.GetPlayersFromDatabase(MainWindow.CurrentDB, MainWindow.TST, MainWindow.TSTOpp, _curSeason, _maxSeason);

            getActivePlayers();
            cmbTeam.SelectedIndex = -1;
            cmbTeam.SelectedItem = ps.IsActive ? MainWindow.TST[ps.TeamF].DisplayName : "- Inactive -";
            cmbPlayer.SelectedIndex = -1;
            cmbPlayer.SelectedValue = ps.ID;
            //cmbPlayer.SelectedValue = ps.LastName + " " + ps.FirstName + " (" + ps.Position1 + ")";
        }

        /// <summary>
        ///     Creates a PlayerStats instance from the currently displayed information and stats.
        /// </summary>
        /// <returns></returns>
        private PlayerStats createPlayerStatsFromCurrent()
        {
            if (cmbPosition2.SelectedItem == null)
                cmbPosition2.SelectedItem = " ";

            int teamF;
            if (chkIsActive.IsChecked.GetValueOrDefault() == false)
            {
                teamF = -1;
            }
            else
            {
                teamF = GetTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
                if (teamF == -1)
                {
                    AskedTeam = "";
                    var atw = new ComboChoiceWindow("Select the team to which to sign the player", _teams, ComboChoiceWindow.Mode.OneTeam);
                    if (atw.ShowDialog() == true)
                    {
                        teamF = Misc.GetTeamIDFromDisplayName(MainWindow.TST, AskedTeam);
                    }
                    else
                    {
                        teamF = -1;
                        chkIsActive.IsChecked = false;
                    }
                }
            }

            var ps = new PlayerStats(_psr.ID, txtLastName.Text, txtFirstName.Text,
                                     (Position) Enum.Parse(typeof (Position), cmbPosition1.SelectedItem.ToString()),
                                     (Position) Enum.Parse(typeof (Position), cmbPosition2.SelectedItem.ToString()),
                                     Convert.ToInt32(txtYearOfBirth.Text), Convert.ToInt32(txtYearsPro.Text), teamF, _psr.TeamS,
                                     chkIsActive.IsChecked.GetValueOrDefault(), false, _psr.Injury,
                                     chkIsAllStar.IsChecked.GetValueOrDefault(), chkIsNBAChampion.IsChecked.GetValueOrDefault(),
                                     _dtOv.Rows[0]) {Height = _psr.Height, Weight = _psr.Weight};
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
            if (!_changingTimeframe)
            {
                MainWindow.Tf = new Timeframe(_curSeason);
                updateData();
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
            if (!_changingTimeframe)
            {
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                updateData();
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
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                }
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                updateData();
                rbStatsBetween.IsChecked = true;
                _changingTimeframe = false;
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
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
                {
                    dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                }
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                updateData();
                rbStatsBetween.IsChecked = true;
                _changingTimeframe = false;
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

            _oppPlayersList = new ObservableCollection<KeyValuePair<int, string>>();
            string q;
            if (cmbOppTeam.SelectedItem.ToString() != "- Inactive -")
            {
                q = "select * from " + _playersT + " where TeamFin = " +
                    Misc.GetTeamIDFromDisplayName(MainWindow.TST, cmbOppTeam.SelectedItem.ToString()) + " AND isActive LIKE \"True\"";
            }
            else
            {
                q = "select * from " + _playersT + " where isActive LIKE \"False\"";
            }
            q += " AND isHidden LIKE \"False\"";
            q += " ORDER BY LastName ASC";
            var res = _db.GetDataTable(q);

            foreach (DataRow r in res.Rows)
            {
                _oppPlayersList.Add(new KeyValuePair<int, string>(ParseCell.GetInt32(r, "ID"),
                                                                  ParseCell.GetString(r, "LastName") + ", " +
                                                                  ParseCell.GetString(r, "FirstName") + " (" +
                                                                  ParseCell.GetString(r, "Position1") + ")"));
            }

            cmbOppPlayer.ItemsSource = _oppPlayersList;
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

            selectedOppPlayerID = ((KeyValuePair<int, string>) (cmbOppPlayer.SelectedItem)).Key;

            var psrList = new ObservableCollection<PlayerStatsRow>();

            _hthAllPBS = new List<PlayerBoxScore>();

            string q;
            DataTable res;

            if (_selectedPlayerID == selectedOppPlayerID)
                return;

            if (rbStatsAllTime.IsChecked.GetValueOrDefault())
            {
                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    _psr.Type = _psr.FirstName + " " + _psr.LastName;
                    psrList.Add(_psr);

                    _hthOwnPBS = new List<PlayerBoxScore>(_pbsList);

                    q = "SELECT * FROM " + _playersT + " WHERE ID = " + selectedOppPlayerID;
                    res = _db.GetDataTable(q);

                    var ps = new PlayerStats(res.Rows[0], MainWindow.TST);
                    var oppPSR = new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName);

                    oppPSR.Type = oppPSR.FirstName + " " + oppPSR.LastName;
                    psrList.Add(oppPSR);

                    q = "select * " + "FROM PlayerResults INNER JOIN GameResults " + "ON (PlayerResults.GameID = GameResults.GameID) " +
                        "WHERE PlayerID = " + selectedOppPlayerID + " AND SeasonNum = " + _curSeason + " AND isOut = \"False\"";
                    res = _db.GetDataTable(q, true);

                    _hthOppPBS = new List<PlayerBoxScore>();
                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r, MainWindow.TST);
                        _hthOppPBS.Add(pbs);
                    }
                    var gameIDs = new List<int>();
                    foreach (var bs in _hthOwnPBS)
                    {
                        _hthAllPBS.Add(bs);
                        gameIDs.Add(bs.GameID);
                    }
                    foreach (var bs in _hthOppPBS)
                    {
                        if (!gameIDs.Contains(bs.GameID))
                        {
                            _hthAllPBS.Add(bs);
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
                            "AND SeasonNum = {2}) ORDER BY Date DESC", _selectedPlayerID, selectedOppPlayerID, _curSeason);
                    res = _db.GetDataTable(q, true);

                    var p = new Player(_psr.ID, _psr.TeamF, _psr.LastName, _psr.FirstName, _psr.Position1, _psr.Position2);
                    var ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r, MainWindow.TST);
                        ps.AddBoxScore(pbs);
                        _hthAllPBS.Add(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));


                    // Opponent
                    q = "SELECT * FROM " + _playersT + " WHERE ID = " + selectedOppPlayerID;
                    res = _db.GetDataTable(q);

                    p = new Player(res.Rows[0]);

                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\" " + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\" " +
                            "AND SeasonNum = {2}) ORDER BY Date DESC", selectedOppPlayerID, _selectedPlayerID, _curSeason);
                    res = _db.GetDataTable(q, true);

                    ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r, MainWindow.TST);
                        ps.AddBoxScore(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));
                }
            }
            else
            {
                if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
                {
                    _psr.Type = _psr.FirstName + " " + _psr.LastName;
                    psrList.Add(_psr);

                    var gameIDs = new List<int>();
                    foreach (var cur in _pbsList)
                    {
                        _hthAllPBS.Add(cur);
                        gameIDs.Add(cur.GameID);
                    }

                    var oppPBSList =
                        MainWindow.BSHist.Where(bse => bse.PBSList.Any(pbs => pbs.PlayerID == selectedOppPlayerID))
                                  .Select(bse => bse.PBSList.Single(pbs => pbs.PlayerID == selectedOppPlayerID));
                    foreach (var oppPBS in oppPBSList)
                    {
                        if (!gameIDs.Contains(oppPBS.GameID))
                        {
                            _hthAllPBS.Add(oppPBS);
                        }
                    }

                    var oppPS = MainWindow.PST[selectedOppPlayerID];
                    psrList.Add(new PlayerStatsRow(oppPS, oppPS.FirstName + " " + oppPS.LastName));
                }
                else
                {
                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\"" + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\"", _selectedPlayerID,
                            selectedOppPlayerID);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += ") ORDER BY Date DESC";
                    res = _db.GetDataTable(q, true);

                    var p = new Player(_psr.ID, _psr.TeamF, _psr.LastName, _psr.FirstName, _psr.Position1, _psr.Position2);
                    var ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r, MainWindow.TST);
                        ps.AddBoxScore(pbs);
                        _hthAllPBS.Add(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));


                    // Opponent
                    q = "SELECT * FROM " + _playersT + " WHERE ID = " + selectedOppPlayerID;
                    res = _db.GetDataTable(q);

                    p = new Player(res.Rows[0]);

                    q =
                        String.Format(
                            "SELECT * FROM PlayerResults INNER JOIN GameResults " + "ON GameResults.GameID = PlayerResults.GameID " +
                            "WHERE PlayerID = {0} AND isOut = \"False\" " + "AND PlayerResults.GameID IN " +
                            "(SELECT GameID FROM PlayerResults " + "WHERE PlayerID = {1} AND isOut = \"False\" ", selectedOppPlayerID,
                            _selectedPlayerID);
                    q = SQLiteDatabase.AddDateRangeToSQLQuery(q, dtpStart.SelectedDate.GetValueOrDefault(),
                                                              dtpEnd.SelectedDate.GetValueOrDefault());
                    q += ") ORDER BY Date DESC";
                    res = _db.GetDataTable(q, true);

                    ps = new PlayerStats(p);

                    foreach (DataRow r in res.Rows)
                    {
                        var pbs = new PlayerBoxScore(r, MainWindow.TST);
                        ps.AddBoxScore(pbs);
                    }

                    psrList.Add(new PlayerStatsRow(ps, ps.FirstName + " " + ps.LastName));
                }
            }

            _hthAllPBS.Sort((pbs1, pbs2) => pbs1.RealDate.CompareTo(pbs2.RealDate));
            _hthAllPBS.Reverse();

            dgvHTH.ItemsSource = psrList;
            dgvHTHBoxScores.ItemsSource = _hthAllPBS;
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
                var gameID = row.GameID;

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
                var gameID = row.GameID;

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
        private void statColumn_Sorting(object sender, DataGridSortingEventArgs e)
        {
            EventHandlers.StatColumn_Sorting((DataGrid) sender, e);
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
                var dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                foreach (var dict in dictList)
                {
                    var type = "Stats";
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
                            tryChangeRow(0, dict);
                            break;
                    }
                }

                createViewAndUpdateOverview();

                //btnSavePlayer_Click(null, null);
            }
        }

        /// <summary>
        ///     Tries to change the specified row of the Overview data table using the specified dictionary.
        ///     Used when pasting TSV data from the clipboard.
        /// </summary>
        /// <param name="row">The row of dt_ov to try and change.</param>
        /// <param name="dict">The dictionary containing stat-value pairs.</param>
        private void tryChangeRow(int row, Dictionary<string, string> dict)
        {
            _dtOv.Rows[row].TryChangeValue(dict, "GP", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "GS", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "MINS", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "PTS", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "FG", typeof (UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "3PT", typeof (UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "FT", typeof (UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "REB", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "OREB", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "DREB", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "AST", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "TO", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "STL", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "BLK", typeof (UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "FOUL", typeof (UInt16));
        }

        /// <summary>
        ///     Creates a DataView instance based on the dt_ov Overview data table and updates the dgvOverviewStats data context.
        /// </summary>
        private void createViewAndUpdateOverview()
        {
            var dvOv = new DataView(_dtOv) {AllowNew = false, AllowDelete = false};
            dgvOverviewStats.DataContext = dvOv;
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
            if (cmbGraphStat.SelectedIndex == -1 || cmbTeam.SelectedIndex == -1 || cmbPlayer.SelectedIndex == -1 || _pbsList.Count < 1)
                return;

            var cp = new ChartPrimitive();
            double i = 0;

            var propToGet = cmbGraphStat.SelectedItem.ToString();
            propToGet = propToGet.Replace('3', 'T');
            propToGet = propToGet.Replace('%', 'p');

            double sum = 0;
            double games = 0;

            foreach (var pbs in _pbsList)
            {
                i++;
                var value = Convert.ToDouble(typeof (PlayerBoxScore).GetProperty(propToGet).GetValue(pbs, null));
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
                var average = sum/games;
                var cpavg = new ChartPrimitive();
                for (var j = 1; j <= i; j++)
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
        private void populateGraphStatCombo()
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

        private void window_Closing(object sender, CancelEventArgs e)
        {
            Misc.SetRegistrySetting("PlayerOvHeight", Height);
            Misc.SetRegistrySetting("PlayerOvWidth", Width);
            Misc.SetRegistrySetting("PlayerOvX", Left);
            Misc.SetRegistrySetting("PlayerOvY", Top);
        }

        private void btnAddPastStats_Click(object sender, RoutedEventArgs e)
        {
            var adw = new AddStatsWindow(false, _psr.ID);
            if (adw.ShowDialog() == true)
            {
                updateYearlyReport();
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

        private void chkIsInjured_Click(object sender, RoutedEventArgs e)
        {
            chkIsInjured.IsChecked = _psr.IsInjured;
            var piw = new PlayerInjuryWindow(_psr.Injury);
            if (piw.ShowDialog() == true)
            {
                _psr.Injury = PlayerInjuryWindow.InjuryType != -1
                                  ? new PlayerInjury(PlayerInjuryWindow.InjuryType, PlayerInjuryWindow.InjuryDaysLeft)
                                  : new PlayerInjury(PlayerInjuryWindow.CustomInjuryName, PlayerInjuryWindow.InjuryDaysLeft);
            }
            chkIsInjured.IsChecked = _psr.IsInjured;
        }
    }
}