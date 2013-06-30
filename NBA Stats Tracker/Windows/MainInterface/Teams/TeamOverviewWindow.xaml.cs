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

namespace NBA_Stats_Tracker.Windows.MainInterface.Teams
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using LeftosCommonLibrary;
    using LeftosCommonLibrary.CommonDialogs;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;
    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Helper.EventHandlers;
    using NBA_Stats_Tracker.Helper.ListExtensions;
    using NBA_Stats_Tracker.Helper.Miscellaneous;
    using NBA_Stats_Tracker.Windows.MainInterface.BoxScores;
    using NBA_Stats_Tracker.Windows.MainInterface.ToolWindows;
    using NBA_Stats_Tracker.Windows.MiscDialogs;

    using SQLite_Database;

    using Swordfish.WPF.Charts;

    #endregion

    /// <summary>Shows team information and stats.</summary>
    public partial class TeamOverviewWindow
    {
        private readonly int _teamIDToLoad = -1;
        private readonly string _teamToLoad = "";
        private List<BoxScoreEntry> _bseList;
        private List<BoxScoreEntry> _bseListPl;
        private List<BoxScoreEntry> _bseListSea;
        private bool _changingOppRange;
        private bool _changingOppTeam;
        private bool _changingTimeframe;
        private int _curOpp;

        private int _curSeason = MainWindow.CurSeason;
        private int _curTeam;
        private TeamStats _curts;
        private TeamStats _curtsopp;
        private SQLiteDatabase _db = new SQLiteDatabase(MainWindow.CurrentDB);
        private DataTable _dtBS;
        private DataTable _dtHTH;
        private DataTable _dtOv;
        private DataTable _dtSs;
        private DataTable _dtYea;
        private DataView _dvHTH;
        private DateTime _lastEndDate = DateTime.MinValue;
        private DateTime _lastStartDate = DateTime.MinValue;
        private int _maxSeason = SQLiteIO.GetMaxSeason(MainWindow.CurrentDB);
        private string _oppBestC = "";
        private string _oppBestF = "";
        private string _oppBestG = "";
        private ObservableCollection<PlayerStatsRow> _plPSRList;
        private TeamRankings _playoffRankings;
        private ObservableCollection<PlayerStatsRow> _psrList;
        private Dictionary<int, PlayerStats> _pst;
        private TeamRankings _seasonRankings;
        private List<TeamBoxScore> _tbsList = new List<TeamBoxScore>();
        private string _teamBestC = "";
        private string _teamBestF = "";
        private string _teamBestG = "";
        private Dictionary<int, TeamStats> _tst;
        private Dictionary<int, TeamStats> _tstOpp;
        private TeamStatsRow _curTSR;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamOverviewWindow" /> class.
        /// </summary>
        public TeamOverviewWindow()
        {
            InitializeComponent();

            Height = Tools.GetRegistrySetting("TeamOvHeight", (int) Height);
            Width = Tools.GetRegistrySetting("TeamOvWidth", (int) Width);
            Top = Tools.GetRegistrySetting("TeamOvY", (int) Top);
            Left = Tools.GetRegistrySetting("TeamOvX", (int) Left);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamOverviewWindow" /> class.
        /// </summary>
        /// <param name="team">The team to switch to when the window finishes loading.</param>
        public TeamOverviewWindow(string team)
            : this()
        {
            _teamToLoad = team;
        }

        public TeamOverviewWindow(int id)
            : this()
        {
            _teamIDToLoad = id;
        }

        private ObservableCollection<PlayerHighsRow> recordsList { get; set; }

        /// <summary>Populates the teams combo.</summary>
        private void populateTeamsCombo()
        {
            var teams = (MainWindow.TST.Where(kvp => !kvp.Value.IsHidden).Select(kvp => kvp.Value.DisplayName)).ToList();

            teams.Sort();

            cmbTeam.ItemsSource = teams;
            cmbOppTeam.ItemsSource = teams;
            cmbOppTeamBest.ItemsSource = teams;
            cmbMPOppTeam.ItemsSource = teams;
        }

        /// <summary>Populates the season combo.</summary>
        private void populateSeasonCombo()
        {
            cmbSeasonNum.ItemsSource = MainWindow.SeasonList;

            //cmbSeasonNum.SelectedValue = MainWindow.tf.SeasonNum;
        }

        /// <summary>Handles the Click event of the btnPrev control. Switches to the previous team.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex <= 0)
            {
                cmbTeam.SelectedIndex = cmbTeam.Items.Count - 1;
            }
            else
            {
                cmbTeam.SelectedIndex--;
            }
        }

        /// <summary>Handles the Click event of the btnNext control. Switches to the next team.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == cmbTeam.Items.Count - 1)
            {
                cmbTeam.SelectedIndex = 0;
            }
            else
            {
                cmbTeam.SelectedIndex++;
            }
        }

        private void updateScoutingReport()
        {
            int id;
            try
            {
                id = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
            }
            catch
            {
                return;
            }

            var msg = _tst[id].ScoutingReport(_tst, _psrList, MainWindow.SeasonTeamRankings);
            txbSeasonScoutingReport.Text = msg;

            var facts = getFacts(id, MainWindow.SeasonTeamRankings);
            txbSeasonFacts.Text = facts.Aggregate("", (s1, s2) => s1 + "\n" + s2);

            if (_tst[id].GetPlayoffGames() > 0)
            {
                msg = _tst[id].ScoutingReport(_tst, _psrList, MainWindow.PlayoffTeamRankings, true);
                txbPlayoffsScoutingReport.Text = msg;

                facts = getFacts(id, MainWindow.PlayoffTeamRankings, true);
                txbPlayoffsFacts.Text = facts.Aggregate("", (s1, s2) => s1 + "\n" + s2);

                grpPlayoffsScoutingReport.Visibility = Visibility.Visible;
                grpPlayoffsFacts.Visibility = Visibility.Visible;
            }
            else
            {
                grpPlayoffsScoutingReport.Visibility = Visibility.Collapsed;
                grpPlayoffsFacts.Visibility = Visibility.Collapsed;
            }

            svScoutingReport.ScrollToTop();
        }

        private List<string> getFacts(int id, TeamRankings rankings, bool playoffs = false)
        {
            var count = 0;
            var facts = new List<string>();
            var topThird = MainWindow.TST.Count / 3;
            for (var i = 0; i < rankings.RankingsTotal[id].Length; i++)
            {
                if (i == 3)
                {
                    continue;
                }

                var rank = rankings.RankingsTotal[id][i];
                if (rank <= topThird)
                {
                    var fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), TeamStatsHelper.Totals[i]);
                    fact += String.Format("{0}", !playoffs ? _tst[id].Totals[i] : _tst[id].PlTotals[i]);
                    facts.Add(fact);
                    count++;
                }
            }
            for (var i = 0; i < rankings.RankingsPerGame[id].Length; i++)
            {
                if (double.IsNaN(!playoffs ? _tst[id].PerGame[i] : _tst[id].PlPerGame[i]))
                {
                    continue;
                }
                var rank = rankings.RankingsPerGame[id][i];
                if (rank <= topThird)
                {
                    var fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), TeamStatsHelper.PerGame[i]);
                    if (TeamStatsHelper.PerGame[i].EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", !playoffs ? _tst[id].PerGame[i] : _tst[id].PlPerGame[i]);
                    }
                    else if (TeamStatsHelper.PerGame[i].EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2}", !playoffs ? _tst[id].PerGame[i] : _tst[id].PlPerGame[i]);
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", !playoffs ? _tst[id].PerGame[i] : _tst[id].PlPerGame[i]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            for (var i = 0; i < rankings.RankingsMetrics[id].Keys.Count; i++)
            {
                var metricName = rankings.RankingsMetrics[id].Keys.ToList()[i];
                if (double.IsNaN(!playoffs ? _tst[id].Metrics[metricName] : _tst[id].PlMetrics[metricName]))
                {
                    continue;
                }
                var rank = rankings.RankingsMetrics[id][metricName];
                if (rank <= topThird)
                {
                    var fact = String.Format("{0}{1} in {2}: ", rank, Misc.GetRankingSuffix(rank), metricName.Replace("p", "%"));
                    if (metricName.EndsWith("p") || metricName.EndsWith("%"))
                    {
                        fact += String.Format("{0:F3}", !playoffs ? _tst[id].Metrics[metricName] : _tst[id].PlMetrics[metricName]);
                    }
                    else if (metricName.EndsWith("eff"))
                    {
                        fact += String.Format("{0:F2}", !playoffs ? _tst[id].Metrics[metricName] : _tst[id].PlMetrics[metricName]);
                    }
                    else
                    {
                        fact += String.Format("{0:F1}", !playoffs ? _tst[id].Metrics[metricName] : _tst[id].PlMetrics[metricName]);
                    }
                    facts.Add(fact);
                    count++;
                }
            }
            facts.Sort(
                (f1, f2) =>
                Convert.ToInt32(f1.Substring(0, f1.IndexOfAny(new[] { 's', 'n', 'r', 't' })))
                       .CompareTo(Convert.ToInt32(f2.Substring(0, f2.IndexOfAny(new[] { 's', 'n', 'r', 't' })))));
            return facts;
        }

        /// <summary>Updates the Overview tab and loads the appropriate box scores depending on the timeframe.</summary>
        private void updateOverviewAndBoxScores()
        {
            var id = _curTeam;

            _curts = _tst[id];
            _curtsopp = _tstOpp[id];

            _tbsList = new List<TeamBoxScore>();

            #region Prepare Team Overview

            _bseList = MainWindow.BSHist.Where(bse => bse.BS.Team1ID == _curTeam || bse.BS.Team2ID == _curTeam).ToList();
            _bseListSea = _bseList.Where(bse => bse.BS.IsPlayoff == false).ToList();
            _bseListPl = _bseList.Where(bse => bse.BS.IsPlayoff).ToList();

            foreach (var r in _bseList)
            {
                var bsr = r.BS.CustomClone();
                bsr.PrepareForDisplay(_tst, _curTeam);
                _tbsList.Add(bsr);
            }

            #region Regular Season

            var dr = _dtOv.NewRow();

            dr["Type"] = "Stats";
            dr["Games"] = _curts.GetGames();
            dr["Wins (W%)"] = _curts.Record[0].ToString();
            dr["Losses (Weff)"] = _curts.Record[1].ToString();
            dr["PF"] = _curts.Totals[TAbbrT.PF].ToString();
            dr["PA"] = _curts.Totals[TAbbrT.PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = _curts.Totals[TAbbrT.FGM].ToString() + "-" + _curts.Totals[TAbbrT.FGA].ToString();
            dr["3PT"] = _curts.Totals[TAbbrT.TPM].ToString() + "-" + _curts.Totals[TAbbrT.TPA].ToString();
            dr["FT"] = _curts.Totals[TAbbrT.FTM].ToString() + "-" + _curts.Totals[TAbbrT.FTA].ToString();
            dr["REB"] = (_curts.Totals[TAbbrT.DREB] + _curts.Totals[TAbbrT.OREB]).ToString();
            dr["OREB"] = _curts.Totals[TAbbrT.OREB].ToString();
            dr["DREB"] = _curts.Totals[TAbbrT.DREB].ToString();
            dr["AST"] = _curts.Totals[TAbbrT.AST].ToString();
            dr["TO"] = _curts.Totals[TAbbrT.TOS].ToString();
            dr["STL"] = _curts.Totals[TAbbrT.STL].ToString();
            dr["BLK"] = _curts.Totals[TAbbrT.BLK].ToString();
            dr["FOUL"] = _curts.Totals[TAbbrT.FOUL].ToString();
            dr["MINS"] = _curts.Totals[TAbbrT.MINS].ToString();

            _dtOv.Rows.Add(dr);

            dr = _dtOv.NewRow();

            _curts.CalcAvg(); // Just to be sure...

            dr["Type"] = "Averages";
            //dr["Games"] = curts.getGames();
            dr["Wins (W%)"] = String.Format("{0:F3}", _curts.PerGame[TAbbrPG.Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", _curts.PerGame[TAbbrPG.Weff]);
            dr["PF"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.PPG]);
            dr["PA"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.PAPG]);
            dr["PD"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.PD]);
            dr["FG"] = String.Format("{0:F3}", _curts.PerGame[TAbbrPG.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", _curts.PerGame[TAbbrPG.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", _curts.PerGame[TAbbrPG.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", _curts.PerGame[TAbbrPG.TPeff]);
            dr["FT"] = String.Format("{0:F3}", _curts.PerGame[TAbbrPG.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", _curts.PerGame[TAbbrPG.FTeff]);
            dr["REB"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.RPG]);
            dr["OREB"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.DRPG]);
            dr["AST"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.APG]);
            dr["TO"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.TPG]);
            dr["STL"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.SPG]);
            dr["BLK"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", _curts.PerGame[TAbbrPG.FPG]);

            _dtOv.Rows.Add(dr);

            // Rankings can only be shown based on total stats
            // ...for now
            var dr2 = _dtOv.NewRow();

            dr2["Type"] = "Rankings";
            dr2["Wins (W%)"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.Wp];
            dr2["Losses (Weff)"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.Weff];
            dr2["PF"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.PPG];
            dr2["PA"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.PAPG];
            dr2["PD"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.PD];
            dr2["FG"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.FGp];
            dr2["FGeff"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.FGeff];
            dr2["3PT"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.TPp];
            dr2["3Peff"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.TPeff];
            dr2["FT"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.FTp];
            dr2["FTeff"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.FTeff];
            dr2["REB"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.RPG];
            dr2["OREB"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.ORPG];
            dr2["DREB"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.DRPG];
            dr2["AST"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.APG];
            dr2["TO"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.TPG];
            dr2["STL"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.SPG];
            dr2["BLK"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.BPG];
            dr2["FOUL"] = _seasonRankings.RankingsPerGame[id][TAbbrPG.FPG];

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Stats";
            dr2["Games"] = _curtsopp.GetGames();
            dr2["Wins (W%)"] = _curtsopp.Record[0].ToString();
            dr2["Losses (Weff)"] = _curtsopp.Record[1].ToString();
            dr2["PF"] = _curtsopp.Totals[TAbbrT.PF].ToString();
            dr2["PA"] = _curtsopp.Totals[TAbbrT.PA].ToString();
            dr2["PD"] = " ";
            dr2["FG"] = _curtsopp.Totals[TAbbrT.FGM].ToString() + "-" + _curtsopp.Totals[TAbbrT.FGA].ToString();
            dr2["3PT"] = _curtsopp.Totals[TAbbrT.TPM].ToString() + "-" + _curtsopp.Totals[TAbbrT.TPA].ToString();
            dr2["FT"] = _curtsopp.Totals[TAbbrT.FTM].ToString() + "-" + _curtsopp.Totals[TAbbrT.FTA].ToString();
            dr2["REB"] = (_curtsopp.Totals[TAbbrT.DREB] + _curtsopp.Totals[TAbbrT.OREB]).ToString();
            dr2["OREB"] = _curtsopp.Totals[TAbbrT.OREB].ToString();
            dr2["DREB"] = _curtsopp.Totals[TAbbrT.DREB].ToString();
            dr2["AST"] = _curtsopp.Totals[TAbbrT.AST].ToString();
            dr2["TO"] = _curtsopp.Totals[TAbbrT.TOS].ToString();
            dr2["STL"] = _curtsopp.Totals[TAbbrT.STL].ToString();
            dr2["BLK"] = _curtsopp.Totals[TAbbrT.BLK].ToString();
            dr2["FOUL"] = _curtsopp.Totals[TAbbrT.FOUL].ToString();
            dr2["MINS"] = _curtsopp.Totals[TAbbrT.MINS].ToString();

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Avg";
            dr2["Wins (W%)"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbrPG.Wp]);
            dr2["Losses (Weff)"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbrPG.Weff]);
            dr2["PF"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.PPG]);
            dr2["PA"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.PAPG]);
            dr2["PD"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.PD]);
            dr2["FG"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbrPG.FGp]);
            dr2["FGeff"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbrPG.FGeff]);
            dr2["3PT"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbrPG.TPp]);
            dr2["3Peff"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbrPG.TPeff]);
            dr2["FT"] = String.Format("{0:F3}", _curtsopp.PerGame[TAbbrPG.FTp]);
            dr2["FTeff"] = String.Format("{0:F2}", _curtsopp.PerGame[TAbbrPG.FTeff]);
            dr2["REB"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.RPG]);
            dr2["OREB"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.ORPG]);
            dr2["DREB"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.DRPG]);
            dr2["AST"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.APG]);
            dr2["TO"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.TPG]);
            dr2["STL"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.SPG]);
            dr2["BLK"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.BPG]);
            dr2["FOUL"] = String.Format("{0:F1}", _curtsopp.PerGame[TAbbrPG.FPG]);

            _dtOv.Rows.Add(dr2);

            #endregion

            #region Playoffs

            _dtOv.Rows.Add(_dtOv.NewRow());

            dr = _dtOv.NewRow();

            dr["Type"] = "Playoffs";
            dr["Games"] = _curts.GetPlayoffGames();
            dr["Wins (W%)"] = _curts.PlRecord[0].ToString();
            dr["Losses (Weff)"] = _curts.PlRecord[1].ToString();
            dr["PF"] = _curts.PlTotals[TAbbrT.PF].ToString();
            dr["PA"] = _curts.PlTotals[TAbbrT.PA].ToString();
            dr["PD"] = " ";
            dr["FG"] = _curts.PlTotals[TAbbrT.FGM].ToString() + "-" + _curts.PlTotals[TAbbrT.FGA].ToString();
            dr["3PT"] = _curts.PlTotals[TAbbrT.TPM].ToString() + "-" + _curts.PlTotals[TAbbrT.TPA].ToString();
            dr["FT"] = _curts.PlTotals[TAbbrT.FTM].ToString() + "-" + _curts.PlTotals[TAbbrT.FTA].ToString();
            dr["REB"] = (_curts.PlTotals[TAbbrT.DREB] + _curts.PlTotals[TAbbrT.OREB]).ToString();
            dr["OREB"] = _curts.PlTotals[TAbbrT.OREB].ToString();
            dr["DREB"] = _curts.PlTotals[TAbbrT.DREB].ToString();
            dr["AST"] = _curts.PlTotals[TAbbrT.AST].ToString();
            dr["TO"] = _curts.PlTotals[TAbbrT.TOS].ToString();
            dr["STL"] = _curts.PlTotals[TAbbrT.STL].ToString();
            dr["BLK"] = _curts.PlTotals[TAbbrT.BLK].ToString();
            dr["FOUL"] = _curts.PlTotals[TAbbrT.FOUL].ToString();
            dr["MINS"] = _curts.PlTotals[TAbbrT.MINS].ToString();

            _dtOv.Rows.Add(dr);

            dr = _dtOv.NewRow();

            dr["Type"] = "Pl Avg";
            dr["Wins (W%)"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbrPG.Wp]);
            dr["Losses (Weff)"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbrPG.Weff]);
            dr["PF"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.PPG]);
            dr["PA"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.PAPG]);
            dr["PD"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.PD]);
            dr["FG"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbrPG.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbrPG.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbrPG.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbrPG.TPeff]);
            dr["FT"] = String.Format("{0:F3}", _curts.PlPerGame[TAbbrPG.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", _curts.PlPerGame[TAbbrPG.FTeff]);
            dr["REB"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.RPG]);
            dr["OREB"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.DRPG]);
            dr["AST"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.APG]);
            dr["TO"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.TPG]);
            dr["STL"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.SPG]);
            dr["BLK"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", _curts.PlPerGame[TAbbrPG.FPG]);

            _dtOv.Rows.Add(dr);

            dr2 = _dtOv.NewRow();

            var count = _tst.Count(z => z.Value.GetPlayoffGames() > 0);

            dr2["Type"] = "Pl Rank";
            dr2["Wins (W%)"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.Wp];
            dr2["Losses (Weff)"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.Weff];
            dr2["PF"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.PPG];
            dr2["PA"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.PAPG];
            dr2["PD"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.PD];
            dr2["FG"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.FGp];
            dr2["FGeff"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.FGeff];
            dr2["3PT"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.TPp];
            dr2["3Peff"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.TPeff];
            dr2["FT"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.FTp];
            dr2["FTeff"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.FTeff];
            dr2["REB"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.RPG];
            dr2["OREB"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.ORPG];
            dr2["DREB"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.DRPG];
            dr2["AST"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.APG];
            dr2["TO"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.TPG];
            dr2["STL"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.SPG];
            dr2["BLK"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.BPG];
            dr2["FOUL"] = _playoffRankings.RankingsPerGame[id][TAbbrPG.FPG];

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Pl Stats";
            dr2["Games"] = _curtsopp.GetPlayoffGames();
            dr2["Wins (W%)"] = _curtsopp.PlRecord[0].ToString();
            dr2["Losses (Weff)"] = _curtsopp.PlRecord[1].ToString();
            dr2["PF"] = _curtsopp.PlTotals[TAbbrT.PF].ToString();
            dr2["PA"] = _curtsopp.PlTotals[TAbbrT.PA].ToString();
            dr2["PD"] = " ";
            dr2["FG"] = _curtsopp.PlTotals[TAbbrT.FGM].ToString() + "-" + _curtsopp.PlTotals[TAbbrT.FGA].ToString();
            dr2["3PT"] = _curtsopp.PlTotals[TAbbrT.TPM].ToString() + "-" + _curtsopp.PlTotals[TAbbrT.TPA].ToString();
            dr2["FT"] = _curtsopp.PlTotals[TAbbrT.FTM].ToString() + "-" + _curtsopp.PlTotals[TAbbrT.FTA].ToString();
            dr2["REB"] = (_curtsopp.PlTotals[TAbbrT.DREB] + _curtsopp.PlTotals[TAbbrT.OREB]).ToString();
            dr2["OREB"] = _curtsopp.PlTotals[TAbbrT.OREB].ToString();
            dr2["DREB"] = _curtsopp.PlTotals[TAbbrT.DREB].ToString();
            dr2["AST"] = _curtsopp.PlTotals[TAbbrT.AST].ToString();
            dr2["TO"] = _curtsopp.PlTotals[TAbbrT.TOS].ToString();
            dr2["STL"] = _curtsopp.PlTotals[TAbbrT.STL].ToString();
            dr2["BLK"] = _curtsopp.PlTotals[TAbbrT.BLK].ToString();
            dr2["FOUL"] = _curtsopp.PlTotals[TAbbrT.FOUL].ToString();
            dr2["MINS"] = _curtsopp.PlTotals[TAbbrT.MINS].ToString();

            _dtOv.Rows.Add(dr2);

            dr2 = _dtOv.NewRow();

            dr2["Type"] = "Opp Pl Avg";
            dr2["Wins (W%)"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbrPG.Wp]);
            dr2["Losses (Weff)"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbrPG.Weff]);
            dr2["PF"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.PPG]);
            dr2["PA"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.PAPG]);
            dr2["PD"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.PD]);
            dr2["FG"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbrPG.FGp]);
            dr2["FGeff"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbrPG.FGeff]);
            dr2["3PT"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbrPG.TPp]);
            dr2["3Peff"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbrPG.TPeff]);
            dr2["FT"] = String.Format("{0:F3}", _curtsopp.PlPerGame[TAbbrPG.FTp]);
            dr2["FTeff"] = String.Format("{0:F2}", _curtsopp.PlPerGame[TAbbrPG.FTeff]);
            dr2["REB"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.RPG]);
            dr2["OREB"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.ORPG]);
            dr2["DREB"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.DRPG]);
            dr2["AST"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.APG]);
            dr2["TO"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.TPG]);
            dr2["STL"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.SPG]);
            dr2["BLK"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.BPG]);
            dr2["FOUL"] = String.Format("{0:F1}", _curtsopp.PlPerGame[TAbbrPG.FPG]);

            _dtOv.Rows.Add(dr2);

            #endregion

            createViewAndUpdateOverview();

            dgvBoxScores.ItemsSource = _tbsList;

            #endregion

            _curTSR = new TeamStatsRow(_curts);
            dgMetrics.ItemsSource = new List<TeamStatsRow> { _curTSR };

            updatePBPStats();
        }

        /// <summary>Creates a DataView based on the current overview DataTable and refreshes the DataGrid.</summary>
        private void createViewAndUpdateOverview()
        {
            var dvOv = new DataView(_dtOv) { AllowNew = false, AllowDelete = false };
            dgvTeamStats.DataContext = dvOv;
        }

        /// <summary>Calculates the split stats and updates the split stats tab.</summary>
        private void updateSplitStats()
        {
            var splitTeamStats = MainWindow.SplitTeamStats;
            var id = _curTeam;

            var dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Home"], ref dr, "Home");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Away"], ref dr, "Away");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Wins"], ref dr, "Wins");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Losses"], ref dr, "Losses");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Season"], ref dr, "Season");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Playoffs"], ref dr, "Playoffs");
            _dtSs.Rows.Add(dr);

            #region Per Opponent

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            var inverseDict = MainWindow.DisplayNames.ToDictionary(pair => pair.Value, pair => pair.Key);
            var names = inverseDict.Keys.ToList();
            names.Remove("");
            names.Sort();

            foreach (var name in names)
            {
                if (inverseDict[name] == _curTeam)
                {
                    continue;
                }

                dr = _dtSs.NewRow();
                createDataRowFromTeamStats(splitTeamStats[id]["vs " + name], ref dr, "vs " + name);
                _dtSs.Rows.Add(dr);
            }

            #endregion

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["vs >= .500"], ref dr, "vs >= .500");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["vs < .500"], ref dr, "vs < .500");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Last 10"], ref dr, "Last 10");
            _dtSs.Rows.Add(dr);

            dr = _dtSs.NewRow();
            createDataRowFromTeamStats(splitTeamStats[id]["Before"], ref dr, "Before");
            _dtSs.Rows.Add(dr);

            #region Monthly split stats

            dr = _dtSs.NewRow();
            dr["Type"] = " ";
            _dtSs.Rows.Add(dr);

            foreach (var sspair in splitTeamStats[id].Where(pair => pair.Key.StartsWith("M ")))
            {
                dr = _dtSs.NewRow();
                var labeldt = new DateTime(Convert.ToInt32(sspair.Key.Substring(2, 4)), Convert.ToInt32(sspair.Key.Substring(7, 2)), 1);
                createDataRowFromTeamStats(sspair.Value, ref dr, labeldt.Year.ToString() + " " + String.Format("{0:MMMM}", labeldt));
                _dtSs.Rows.Add(dr);
            }

            #endregion

            // DataTable is done, create DataView and load into DataGrid
            var dvSs = new DataView(_dtSs) { AllowEdit = false, AllowNew = false };

            dgvSplit.DataContext = dvSs;
        }

        /// <summary>Handles the SelectionChanged event of the cmbTeam control. Loads the information for the newly selected team.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
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
                dgMetrics.ItemsSource = null;
                dgOther.ItemsSource = null;
                dgShooting.ItemsSource = null;
                clearGraph();

                if (cmbTeam.SelectedIndex == -1)
                {
                    return;
                }
                if (cmbSeasonNum.SelectedIndex == -1)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            //DataRow dr;

            _dtBS.Clear();
            _dtOv.Clear();
            _dtHTH.Clear();
            _dtSs.Clear();
            _dtYea.Clear();

            _curTeam = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());

            updateOverviewAndBoxScores();

            updateSplitStats();

            var ts = _tst[_curTeam];
            Title = cmbTeam.SelectedItem + " Team Overview - " + (ts.GetGames() + ts.GetPlayoffGames()) + " games played";

            updateHeadToHead();

            updateYearlyStats();

            updatePlayerAndMetricStats();

            updateBest();

            updateScoutingReport();

            updateRecords();

            tbcTeamOverview_SelectionChanged(null, null);

            cmbGraphStat_SelectionChanged(null, null);
        }

        private void updateRecords()
        {
            recordsList = new ObservableCollection<PlayerHighsRow>();
            foreach (var psr in _psrList)
            {
                recordsList.Add(new PlayerHighsRow(psr.ID, psr.FirstName + " " + psr.LastName, MainWindow.PST[psr.ID].CareerHighs));
            }

            dgvHighs.ItemsSource = null;
            dgvHighs.ItemsSource = recordsList;
        }

        /// <summary>Finds the tam's name by its displayName.</summary>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        private int getTeamIDFromDisplayName(string displayName)
        {
            return Misc.GetTeamIDFromDisplayName(_tst, displayName);
        }

        /// <summary>Determines the team's best players and their most significant stats and updates the corresponding tab.</summary>
        private void updateBest()
        {
            txbPlayer1.Text = "";
            txbPlayer2.Text = "";
            txbPlayer3.Text = "";
            txbPlayer4.Text = "";
            txbPlayer5.Text = "";
            txbPlayer6.Text = "";

            try
            {
                var templist = _psrList.ToList();
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
                var text = psr1.GetBestStats(5);
                txbPlayer1.Text = "1: " + psr1.FirstName + " " + psr1.LastName + " (" + psr1.Position1 + ")"
                                  + (psr1.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                var psr2 = templist[1];
                text = psr2.GetBestStats(5);
                txbPlayer2.Text = "2: " + psr2.FirstName + " " + psr2.LastName + " (" + psr2.Position1 + ")"
                                  + (psr2.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                var psr3 = templist[2];
                text = psr3.GetBestStats(5);
                txbPlayer3.Text = "3: " + psr3.FirstName + " " + psr3.LastName + " (" + psr3.Position1 + ")"
                                  + (psr3.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                var psr4 = templist[3];
                text = psr4.GetBestStats(5);
                txbPlayer4.Text = "4: " + psr4.FirstName + " " + psr4.LastName + " (" + psr4.Position1 + ")"
                                  + (psr4.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                var psr5 = templist[4];
                text = psr5.GetBestStats(5);
                txbPlayer5.Text = "5: " + psr5.FirstName + " " + psr5.LastName + " (" + psr5.Position1 + ")"
                                  + (psr5.IsInjured ? " (Injured)" : "") + "\n\n" + text;

                var psr6 = templist[5];
                text = psr6.GetBestStats(5);
                txbPlayer6.Text = "6: " + psr6.FirstName + " " + psr6.LastName + " (" + psr6.Position1 + ")"
                                  + (psr6.IsInjured ? " (Injured)" : "") + "\n\n" + text;
            }
            catch (Exception)
            {
            }

            calculateSituational(cmbSituational.SelectedItem.ToString());
        }

        /// <summary>Determines the team's best starting five and their most significant stats.</summary>
        private void calculateSituational(string property)
        {
            txbStartingPG.Text = "";
            txbStartingSG.Text = "";
            txbStartingSF.Text = "";
            txbStartingPF.Text = "";
            txbStartingC.Text = "";

            string text;
            PlayerStatsRow psr1;
            var tempList = new List<PlayerStatsRow>();

            var doBench = false;
            if (property == "Starters")
            {
                property = "GmSc";
            }
            else if (property == "Bench")
            {
                property = "GmSc";
                doBench = true;
            }
            else
            {
                property = property.Replace("%", "p").Replace("3", "T");
            }

            var pgList =
                _psrList.Where(row => (row.Position1.ToString() == "PG" || row.Position2.ToString() == "PG") && row.IsInjured == false)
                        .ToList();
            pgList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            pgList.Reverse();
            var sgList =
                _psrList.Where(row => (row.Position1.ToString() == "SG" || row.Position2.ToString() == "SG") && row.IsInjured == false)
                        .ToList();
            sgList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            sgList.Reverse();
            var sfList =
                _psrList.Where(row => (row.Position1.ToString() == "SF" || row.Position2.ToString() == "SF") && row.IsInjured == false)
                        .ToList();
            sfList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            sfList.Reverse();
            var pfList =
                _psrList.Where(row => (row.Position1.ToString() == "PF" || row.Position2.ToString() == "PF") && row.IsInjured == false)
                        .ToList();
            pfList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            pfList.Reverse();
            var cList =
                _psrList.Where(row => (row.Position1.ToString() == "C" || row.Position2.ToString() == "C") && row.IsInjured == false)
                        .ToList();
            cList.Sort((pmsr1, pmsr2) => GenericExtensions.Compare(pmsr1, pmsr2, property));
            cList.Reverse();
            var permutations = new List<StartingFivePermutation>();

            var max = Double.MinValue;
            foreach (var pg in pgList)
            {
                foreach (var sg in sgList)
                {
                    foreach (var sf in sfList)
                    {
                        foreach (var pf in pfList)
                        {
                            foreach (var c in cList)
                            {
                                double sum = 0;
                                var pInP = 0;
                                var perm = new List<int>(5) { pg.ID };
                                sum += Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(property).GetValue(pg, null));
                                if (pg.Position1.ToString() == "PG")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(sg.ID))
                                {
                                    continue;
                                }
                                perm.Add(sg.ID);
                                sum += Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(property).GetValue(sg, null));
                                if (sg.Position1.ToString() == "SG")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(sf.ID))
                                {
                                    continue;
                                }
                                perm.Add(sf.ID);
                                sum += Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(property).GetValue(sf, null));
                                if (sf.Position1.ToString() == "SF")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(pf.ID))
                                {
                                    continue;
                                }
                                perm.Add(pf.ID);
                                sum += Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(property).GetValue(pf, null));
                                if (pf.Position1.ToString() == "PF")
                                {
                                    pInP++;
                                }
                                if (perm.Contains(c.ID))
                                {
                                    continue;
                                }
                                perm.Add(c.ID);
                                sum += Convert.ToDouble(typeof(PlayerStatsRow).GetProperty(property).GetValue(c, null));
                                if (c.Position1.ToString() == "C")
                                {
                                    pInP++;
                                }

                                if (sum > max)
                                {
                                    max = sum;
                                }

                                permutations.Add(
                                    new StartingFivePermutation { IDList = perm, PlayersInPrimaryPosition = pInP, Sum = sum });
                            }
                        }
                    }
                }
            }

            try
            {
                var bestPerm =
                    permutations.Where(perm => perm.Sum.Equals(max)).OrderByDescending(perm => perm.PlayersInPrimaryPosition).First();
                if (!doBench)
                {
                    bestPerm.IDList.ForEach(i1 => tempList.Add(_psrList.Single(row => row.ID == i1)));
                }
                else
                {
                    var benchPerms = permutations.Where(perm => !(perm.IDList.Any(id => bestPerm.IDList.Contains(id)))).ToList();
                    if (benchPerms.Count == 0)
                    {
                        foreach (var perm in permutations)
                        {
                            foreach (var id in perm.IDList)
                            {
                                if (bestPerm.IDList.Contains(id))
                                {
                                    perm.BestPermCount++;
                                }
                            }
                        }
                        benchPerms = permutations;
                    }
                    var benchPerm =
                        benchPerms.OrderBy(perm => perm.BestPermCount)
                                  .ThenByDescending(perm => perm.Sum)
                                  .ThenByDescending(perm => perm.PlayersInPrimaryPosition)
                                  .First();
                    benchPerm.IDList.ForEach(i1 => tempList.Add(_psrList.Single(row => row.ID == i1)));
                }
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

        /// <summary>Calculates the player and metric stats and updates the corresponding tabs.</summary>
        private void updatePlayerAndMetricStats()
        {
            _psrList = new ObservableCollection<PlayerStatsRow>();
            _plPSRList = new ObservableCollection<PlayerStatsRow>();

            var players = _pst.Where(pair => pair.Value.TeamF == _curTeam && !pair.Value.IsHidden);
            foreach (var pl in players)
            {
                _psrList.Add(new PlayerStatsRow(pl.Value, false));
                _plPSRList.Add(new PlayerStatsRow(pl.Value, true));
            }

            _psrList.Sort((row1, row2) => String.CompareOrdinal(row1.LastName, row2.LastName));
            _plPSRList.Sort((row1, row2) => String.CompareOrdinal(row1.LastName, row2.LastName));

            dgvPlayerStats.ItemsSource = rbPlayerStatsSeason.IsChecked == true ? _psrList : _plPSRList;
            dgvMetricStats.ItemsSource = dgvPlayerStats.ItemsSource;
            dgvTeamRoster.ItemsSource = _psrList;
            dgvTeamRoster.CanUserAddRows = false;
        }

        /// <summary>Updates the head to head tab.</summary>
        private void updateHeadToHead()
        {
            cmbOppTeam_SelectionChanged(null, null);
        }

        /// <summary>Calculates the yearly stats and updates the yearly stats tab.</summary>
        private void updateYearlyStats()
        {
            _dtYea.Clear();

            var currentDB = MainWindow.CurrentDB;
            _curSeason = MainWindow.CurSeason;
            _maxSeason = SQLiteIO.GetMaxSeason(currentDB);

            var ts = _tst[_curTeam];
            var tsAllSeasons = new TeamStats(-1, "All Seasons");
            var tsAllPlayoffs = new TeamStats(-1, "All Playoffs");
            var tsAll = new TeamStats(-1, "All Games");
            tsAllSeasons.AddTeamStats(ts, Span.Season);
            tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
            tsAll.AddTeamStats(ts, Span.SeasonAndPlayoffsToSeason);

            var drcur = _dtYea.NewRow();
            var drcurPl = _dtYea.NewRow();
            createDataRowFromTeamStats(ts, ref drcur, "Season " + MainWindow.GetSeasonName(_curSeason));

            var playedInPlayoffs = false;
            if (ts.PlRecord[0] + ts.PlRecord[1] > 0)
            {
                createDataRowFromTeamStats(ts, ref drcurPl, "Playoffs " + MainWindow.GetSeasonName(_curSeason), true);
                playedInPlayoffs = true;
            }

            //
            var qr = string.Format(@"SELECT * FROM PastTeamStats WHERE TeamID = {0} ORDER BY ""SOrder""", ts.ID);
            var dt = _db.GetDataTable(qr);
            foreach (DataRow dr in dt.Rows)
            {
                var dr4 = _dtYea.NewRow();
                ts = new TeamStats();
                if (ParseCell.GetBoolean(dr, "isPlayoff"))
                {
                    SQLiteIO.GetTeamStatsFromDataRow(ref ts, dr, true);
                    createDataRowFromTeamStats(ts, ref dr4, "Playoffs " + ParseCell.GetString(dr, "SeasonName"), true);
                    tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
                    tsAll.AddTeamStats(ts, Span.Playoffs);
                }
                else
                {
                    SQLiteIO.GetTeamStatsFromDataRow(ref ts, dr, false);
                    createDataRowFromTeamStats(ts, ref dr4, "Season " + ParseCell.GetString(dr, "SeasonName"), false);
                    tsAllSeasons.AddTeamStats(ts, Span.Season);
                    tsAll.AddTeamStats(ts, Span.Season);
                }
                _dtYea.Rows.Add(dr4);
            }
            //

            for (var j = 1; j <= _maxSeason; j++)
            {
                if (j != _curSeason)
                {
                    TeamStats tsopp;
                    SQLiteIO.GetTeamStatsFromDatabase(MainWindow.CurrentDB, _curTeam, j, out ts, out tsopp);
                    var dr3 = _dtYea.NewRow();
                    var dr3Pl = _dtYea.NewRow();
                    createDataRowFromTeamStats(ts, ref dr3, "Season " + MainWindow.GetSeasonName(j));

                    _dtYea.Rows.Add(dr3);
                    if (ts.PlRecord[0] + ts.PlRecord[1] > 0)
                    {
                        createDataRowFromTeamStats(ts, ref dr3Pl, "Playoffs " + MainWindow.GetSeasonName(j), true);
                        _dtYea.Rows.Add(dr3Pl);
                    }

                    tsAllSeasons.AddTeamStats(ts, Span.Season);
                    tsAllPlayoffs.AddTeamStats(ts, Span.Playoffs);
                    tsAll.AddTeamStats(ts, Span.SeasonAndPlayoffsToSeason);
                }
                else
                {
                    _dtYea.Rows.Add(drcur);
                    if (playedInPlayoffs)
                    {
                        _dtYea.Rows.Add(drcurPl);
                    }
                }
            }

            _dtYea.Rows.Add(_dtYea.NewRow());

            drcur = _dtYea.NewRow();
            createDataRowFromTeamStats(tsAllSeasons, ref drcur, "All Seasons");
            _dtYea.Rows.Add(drcur);
            drcur = _dtYea.NewRow();
            createDataRowFromTeamStats(tsAllPlayoffs, ref drcur, "All Playoffs");
            _dtYea.Rows.Add(drcur);

            _dtYea.Rows.Add(_dtYea.NewRow());

            drcur = _dtYea.NewRow();
            createDataRowFromTeamStats(tsAll, ref drcur, "All Games");
            _dtYea.Rows.Add(drcur);

            var dvYea = new DataView(_dtYea) { AllowNew = false, AllowEdit = false };

            dgvYearly.DataContext = dvYea;
        }

        /// <summary>Handles the Click event of the btnSaveCustomTeam control. Saves the team's stats into the database.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private async void btnSaveCustomTeam_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == -1)
            {
                return;
            }
            if (rbStatsBetween.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You can't edit partial stats. You can either edit the total stats (which are kept separately from box-scores"
                    + ") or edit the box-scores themselves.",
                    "NBA Stats Tracker",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            int id;
            try
            {
                id = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
            }
            catch (Exception)
            {
                return;
            }
            _tst[id].Record[0] = Convert.ToByte(myCell(0, 2));
            _tst[id].Record[1] = Convert.ToByte(myCell(0, 3));
            _tst[id].Totals[TAbbrT.PF] = Convert.ToUInt16(myCell(0, 4));
            _tst[id].Totals[TAbbrT.PA] = Convert.ToUInt16(myCell(0, 5));

            var parts = myCell(0, 7).Split('-');
            _tst[id].Totals[TAbbrT.FGM] = Convert.ToUInt16(parts[0]);
            _tst[id].Totals[TAbbrT.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 9).Split('-');
            _tst[id].Totals[TAbbrT.TPM] = Convert.ToUInt16(parts[0]);
            _tst[id].Totals[TAbbrT.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(0, 11).Split('-');
            _tst[id].Totals[TAbbrT.FTM] = Convert.ToUInt16(parts[0]);
            _tst[id].Totals[TAbbrT.FTA] = Convert.ToUInt16(parts[1]);

            _tst[id].Totals[TAbbrT.OREB] = Convert.ToUInt16(myCell(0, 14));
            _tst[id].Totals[TAbbrT.DREB] = Convert.ToUInt16(myCell(0, 15));

            _tst[id].Totals[TAbbrT.AST] = Convert.ToUInt16(myCell(0, 16));
            _tst[id].Totals[TAbbrT.TOS] = Convert.ToUInt16(myCell(0, 17));
            _tst[id].Totals[TAbbrT.STL] = Convert.ToUInt16(myCell(0, 18));
            _tst[id].Totals[TAbbrT.BLK] = Convert.ToUInt16(myCell(0, 19));
            _tst[id].Totals[TAbbrT.FOUL] = Convert.ToUInt16(myCell(0, 20));
            _tst[id].Totals[TAbbrT.MINS] = Convert.ToUInt16(myCell(0, 21));

            _tst[id].PlRecord[0] = Convert.ToByte(myCell(6, 2));
            _tst[id].PlRecord[1] = Convert.ToByte(myCell(6, 3));
            _tst[id].PlTotals[TAbbrT.PF] = Convert.ToUInt16(myCell(6, 4));
            _tst[id].PlTotals[TAbbrT.PA] = Convert.ToUInt16(myCell(6, 5));

            parts = myCell(6, 7).Split('-');
            _tst[id].PlTotals[TAbbrT.FGM] = Convert.ToUInt16(parts[0]);
            _tst[id].PlTotals[TAbbrT.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(6, 9).Split('-');
            _tst[id].PlTotals[TAbbrT.TPM] = Convert.ToUInt16(parts[0]);
            _tst[id].PlTotals[TAbbrT.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(6, 11).Split('-');
            _tst[id].PlTotals[TAbbrT.FTM] = Convert.ToUInt16(parts[0]);
            _tst[id].PlTotals[TAbbrT.FTA] = Convert.ToUInt16(parts[1]);

            _tst[id].PlTotals[TAbbrT.OREB] = Convert.ToUInt16(myCell(6, 14));
            _tst[id].PlTotals[TAbbrT.DREB] = Convert.ToUInt16(myCell(6, 15));

            _tst[id].PlTotals[TAbbrT.AST] = Convert.ToUInt16(myCell(6, 16));
            _tst[id].PlTotals[TAbbrT.TOS] = Convert.ToUInt16(myCell(6, 17));
            _tst[id].PlTotals[TAbbrT.STL] = Convert.ToUInt16(myCell(6, 18));
            _tst[id].PlTotals[TAbbrT.BLK] = Convert.ToUInt16(myCell(6, 19));
            _tst[id].PlTotals[TAbbrT.FOUL] = Convert.ToUInt16(myCell(6, 20));
            _tst[id].PlTotals[TAbbrT.MINS] = Convert.ToUInt16(myCell(6, 21));

            _tst[id].CalcAvg();

            // Opponents
            _tstOpp[id].Record[0] = Convert.ToByte(myCell(3, 2));
            _tstOpp[id].Record[1] = Convert.ToByte(myCell(3, 3));
            _tstOpp[id].Totals[TAbbrT.PF] = Convert.ToUInt16(myCell(3, 4));
            _tstOpp[id].Totals[TAbbrT.PA] = Convert.ToUInt16(myCell(3, 5));

            parts = myCell(3, 7).Split('-');
            _tstOpp[id].Totals[TAbbrT.FGM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].Totals[TAbbrT.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 9).Split('-');
            _tstOpp[id].Totals[TAbbrT.TPM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].Totals[TAbbrT.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(3, 11).Split('-');
            _tstOpp[id].Totals[TAbbrT.FTM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].Totals[TAbbrT.FTA] = Convert.ToUInt16(parts[1]);

            _tstOpp[id].Totals[TAbbrT.OREB] = Convert.ToUInt16(myCell(3, 14));
            _tstOpp[id].Totals[TAbbrT.DREB] = Convert.ToUInt16(myCell(3, 15));

            _tstOpp[id].Totals[TAbbrT.AST] = Convert.ToUInt16(myCell(3, 16));
            _tstOpp[id].Totals[TAbbrT.TOS] = Convert.ToUInt16(myCell(3, 17));
            _tstOpp[id].Totals[TAbbrT.STL] = Convert.ToUInt16(myCell(3, 18));
            _tstOpp[id].Totals[TAbbrT.BLK] = Convert.ToUInt16(myCell(3, 19));
            _tstOpp[id].Totals[TAbbrT.FOUL] = Convert.ToUInt16(myCell(3, 20));
            _tstOpp[id].Totals[TAbbrT.MINS] = Convert.ToUInt16(myCell(3, 21));

            _tstOpp[id].PlRecord[0] = Convert.ToByte(myCell(9, 2));
            _tstOpp[id].PlRecord[1] = Convert.ToByte(myCell(9, 3));
            _tstOpp[id].PlTotals[TAbbrT.PF] = Convert.ToUInt16(myCell(9, 4));
            _tstOpp[id].PlTotals[TAbbrT.PA] = Convert.ToUInt16(myCell(9, 5));

            parts = myCell(9, 7).Split('-');
            _tstOpp[id].PlTotals[TAbbrT.FGM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].PlTotals[TAbbrT.FGA] = Convert.ToUInt16(parts[1]);

            parts = myCell(9, 9).Split('-');
            _tstOpp[id].PlTotals[TAbbrT.TPM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].PlTotals[TAbbrT.TPA] = Convert.ToUInt16(parts[1]);

            parts = myCell(9, 11).Split('-');
            _tstOpp[id].PlTotals[TAbbrT.FTM] = Convert.ToUInt16(parts[0]);
            _tstOpp[id].PlTotals[TAbbrT.FTA] = Convert.ToUInt16(parts[1]);

            _tstOpp[id].PlTotals[TAbbrT.OREB] = Convert.ToUInt16(myCell(9, 14));
            _tstOpp[id].PlTotals[TAbbrT.DREB] = Convert.ToUInt16(myCell(9, 15));

            _tstOpp[id].PlTotals[TAbbrT.AST] = Convert.ToUInt16(myCell(9, 16));
            _tstOpp[id].PlTotals[TAbbrT.TOS] = Convert.ToUInt16(myCell(9, 17));
            _tstOpp[id].PlTotals[TAbbrT.STL] = Convert.ToUInt16(myCell(9, 18));
            _tstOpp[id].PlTotals[TAbbrT.BLK] = Convert.ToUInt16(myCell(9, 19));
            _tstOpp[id].PlTotals[TAbbrT.FOUL] = Convert.ToUInt16(myCell(9, 20));
            _tstOpp[id].PlTotals[TAbbrT.MINS] = Convert.ToUInt16(myCell(9, 21));

            _tstOpp[id].CalcAvg();

            var playersToUpdate = _psrList.Select(cur => new PlayerStats(cur)).ToDictionary(ps => ps.ID);
            var playerIDs = playersToUpdate.Keys.ToList();
            foreach (var playerID in playerIDs)
            {
                playersToUpdate[playerID].UpdatePlayoffStats(_plPSRList.Single(plPSR => plPSR.ID == playerID));
                playersToUpdate[playerID].UpdateCareerHighs(recordsList.Single(r => r.PlayerID == playerID));
            }

            SQLiteIO.SaveSeasonToDatabase(
                MainWindow.CurrentDB, _tst, _tstOpp, playersToUpdate, _curSeason, _maxSeason, partialUpdate: true);
            await updateData();
        }

        /// <summary>Gets the value of the specified cell from the dgvTeamStats DataGrid.</summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private string myCell(int row, int col)
        {
            return GetCellValue(dgvTeamStats, row, col);
        }

        /// <summary>Gets the value of the specified cell from the specified DataGrid.</summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        private string GetCellValue(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
            {
                return dataRowView.Row.ItemArray[col].ToString();
            }

            return null;
        }

        /// <summary>
        ///     Handles the Click event of the btnScoutingReport control. Displays a well-formatted scouting report in natural language
        ///     containing comments on the team's performance, strong and weak points.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnScoutingReport_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpEnd control. Makes sure the starting date isn't after the ending date, and
        ///     updates the team's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private async void dtpEnd_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate.GetValueOrDefault() == _lastEndDate)
            {
                return;
            }
            if (_changingTimeframe)
            {
                return;
            }
            _changingTimeframe = true;
            _lastEndDate = dtpEnd.SelectedDate.GetValueOrDefault();
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpStart.SelectedDate = dtpEnd.SelectedDate.GetValueOrDefault().AddMonths(-1).AddDays(1);
                _lastStartDate = dtpStart.SelectedDate.GetValueOrDefault();
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            await updateData();
            _changingTimeframe = false;
        }

        private async Task updateData()
        {
            IsEnabled = false;
            await MainWindow.UpdateAllData(true);
            linkInternalsToMainWindow();
            var curTeam = cmbTeam.SelectedIndex == -1 ? -1 : _curTeam;
            populateTeamsCombo();
            try
            {
                cmbTeam.SelectedIndex = -1;
                if (curTeam != -1)
                {
                    cmbTeam.SelectedItem = _tst[curTeam].DisplayName;
                }
            }
            catch
            {
                cmbTeam.SelectedIndex = -1;
            }
            MainWindow.MWInstance.StopProgressWatchTimer();
            IsEnabled = true;
        }

        /// <summary>
        ///     Handles the SelectedDateChanged event of the dtpStart control. Makes sure the starting date isn't after the ending date, and
        ///     updates the team's stats based on the new timeframe.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private async void dtpStart_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpEnd.SelectedDate.GetValueOrDefault() == _lastEndDate)
            {
                return;
            }
            if (_changingTimeframe)
            {
                return;
            }
            _changingTimeframe = true;
            _lastStartDate = dtpStart.SelectedDate.GetValueOrDefault();
            if (dtpEnd.SelectedDate < dtpStart.SelectedDate)
            {
                dtpEnd.SelectedDate = dtpStart.SelectedDate.GetValueOrDefault().AddMonths(1).AddDays(-1);
                _lastEndDate = dtpEnd.SelectedDate.GetValueOrDefault();
            }
            MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
            rbStatsBetween.IsChecked = true;
            await updateData();
            _changingTimeframe = false;
        }

        /// <summary>Handles the Checked event of the rbStatsAllTime control. Allows the user to display stats from the whole season.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private async void rbStatsAllTime_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                _lastEndDate = DateTime.MinValue;
                _lastStartDate = DateTime.MinValue;
                MainWindow.Tf = new Timeframe(_curSeason);
                await updateData();
                _changingTimeframe = false;
            }
        }

        /// <summary>Handles the Checked event of the rbStatsBetween control. Allows the user to display stats between the specified timeframe.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private async void rbStatsBetween_Checked(object sender, RoutedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                _lastEndDate = DateTime.MinValue;
                _lastStartDate = DateTime.MinValue;
                MainWindow.Tf = new Timeframe(dtpStart.SelectedDate.GetValueOrDefault(), dtpEnd.SelectedDate.GetValueOrDefault());
                await updateData();
                _changingTimeframe = false;
            }
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the dgvBoxScores control. Allows the user to view a specific box score in the Box Score
        ///     window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private async void dgvBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvBoxScores.SelectedCells.Count > 0)
            {
                var row = (TeamBoxScore) dgvBoxScores.SelectedItems[0];

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, row.ID);
                try
                {
                    if (bsw.ShowDialog() == true)
                    {
                        await updateData();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>Handles the Click event of the btnPrevOpp control. Switches to the previous opposing team.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevOpp_Click(object sender, RoutedEventArgs e)
        {
            ComboBox control;
            if (Equals(sender, btnPrevOpp))
            {
                control = cmbOppTeam;
            }
            else if (Equals(sender, btnPrevOppBest))
            {
                control = cmbOppTeamBest;
            }
            else if (Equals(sender, btnMPPrevOpp))
            {
                control = cmbMPOppTeam;
            }
            else
            {
                throw new Exception("TeamOverview.btnPrevOpp_Click called from unexpected sender: " + sender);
            }
            if (control.SelectedIndex <= 0)
            {
                control.SelectedIndex = cmbOppTeam.Items.Count - 1;
            }
            else
            {
                control.SelectedIndex--;
            }
        }

        /// <summary>Handles the Click event of the btnNextOpp control. Switches to the next opposing team.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextOpp_Click(object sender, RoutedEventArgs e)
        {
            ComboBox control;
            if (Equals(sender, btnNextOpp))
            {
                control = cmbOppTeam;
            }
            else if (Equals(sender, btnNextOppBest))
            {
                control = cmbOppTeamBest;
            }
            else if (Equals(sender, btnMPNextOpp))
            {
                control = cmbMPOppTeam;
            }
            else
            {
                throw new Exception("TeamOverview.btnNextOpp_Click called from unexpected sender: " + sender);
            }

            if (control.SelectedIndex == cmbOppTeam.Items.Count - 1)
            {
                control.SelectedIndex = 0;
            }
            else
            {
                control.SelectedIndex++;
            }
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbOppTeam control. Synchronizes the two opposing team combos, loads the stats of
        ///     the selected opposing team, and updates the appropriate tabs.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private void cmbOppTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_changingOppTeam)
            {
                return;
            }

            if (Equals(sender, cmbOppTeam))
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeamBest.SelectedIndex = cmbOppTeam.SelectedIndex;
                    cmbMPOppTeam.SelectedIndex = cmbOppTeam.SelectedIndex;
                }
                catch
                {
                    _changingOppTeam = false;
                    return;
                }
            }
            else if (Equals(sender, cmbOppTeamBest))
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeam.SelectedIndex = cmbOppTeamBest.SelectedIndex;
                    cmbMPOppTeam.SelectedIndex = cmbOppTeamBest.SelectedIndex;
                }
                catch
                {
                    _changingOppTeam = false;
                    return;
                }
            }
            else if (Equals(sender, cmbMPOppTeam))
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeam.SelectedIndex = cmbMPOppTeam.SelectedIndex;
                    cmbOppTeamBest.SelectedIndex = cmbMPOppTeam.SelectedIndex;
                }
                catch
                {
                    _changingOppTeam = false;
                    return;
                }
            }
            else
            {
                try
                {
                    _changingOppTeam = true;
                    cmbOppTeamBest.SelectedIndex = cmbOppTeam.SelectedIndex;
                    cmbMPOppTeam.SelectedIndex = cmbOppTeam.SelectedIndex;
                }
                catch (Exception)
                {
                    _changingOppTeam = false;
                    return;
                }
            }

            if (cmbOppTeam.SelectedIndex == -1)
            {
                _changingOppTeam = false;
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
                _changingOppTeam = false;
                return;
            }
            txbTeam2.Text = "";
            txbTeam3.Text = "";
            txbOpp1.Text = "";
            txbOpp2.Text = "";
            txbOpp3.Text = "";
            txbMPDesc.Text = "";
            txbMPDescHeader.Text = "";
            txbMPOpp.Text = "";
            txbMPOppHeader.Text = "";
            txbMPTeam.Text = "";
            txbMPTeamHeader.Text = "";

            if (cmbOppTeam.SelectedIndex == cmbTeam.SelectedIndex)
            {
                _changingOppTeam = false;
                return;
            }

            _curTeam = getTeamIDFromDisplayName(cmbTeam.SelectedItem.ToString());
            _curOpp = getTeamIDFromDisplayName(cmbOppTeam.SelectedItem.ToString());

            List<PlayerStatsRow> teamPMSRList;
            List<PlayerStatsRow> oppPMSRList;
            prepareHeadToHeadTab(out teamPMSRList, out oppPMSRList);
            prepareHTHBestPerformersTab(teamPMSRList, oppPMSRList);
            prepareMatchupPreview(teamPMSRList, oppPMSRList);

            _changingOppTeam = false;
        }

        private void prepareMatchupPreview(List<PlayerStatsRow> teamPMSRList, List<PlayerStatsRow> oppPMSRList)
        {
            var iTeam = _curTeam;
            var iOpp = _curOpp;
            txbMPDescHeader.Text = "vs\n";
            txbMPTeamHeader.Text = string.Format(
                "{0}\n{1}-{2}", _tst[_curTeam].DisplayName, _tst[iTeam].Record[0], _tst[iTeam].Record[1]);
            txbMPOppHeader.Text = string.Format("{0}\n{1}-{2}", _tst[_curOpp].DisplayName, _tst[iOpp].Record[0], _tst[iOpp].Record[1]);

            var tsr = new TeamStatsRow(_tst[iTeam]);
            var tsropp = new TeamStatsRow(_tst[iOpp]);
            var msgDesc = "";
            var msgTeam = "";
            var msgOpp = "";

            var used = new List<int>();
            var dict = new Dictionary<int, int>();
            for (var k = 0; k < _seasonRankings.RankingsPerGame[iTeam].Length; k++)
            {
                dict.Add(k, _seasonRankings.RankingsPerGame[iTeam][k]);
            }
            prepareMatchupPreviewStrengths(dict, ref msgDesc, tsr, iTeam, tsropp, iOpp, used, ref msgTeam, ref msgOpp);

            dict = new Dictionary<int, int>();
            for (var k = 0; k < _seasonRankings.RankingsPerGame[iOpp].Length; k++)
            {
                dict.Add(k, _seasonRankings.RankingsPerGame[iOpp][k]);
            }
            prepareMatchupPreviewStrengths(dict, ref msgDesc, tsr, iTeam, tsropp, iOpp, used, ref msgTeam, ref msgOpp);

            var descParts = msgDesc.Split('\n');
            var teamParts = msgTeam.Split('\n');
            var oppParts = msgOpp.Split('\n');
            var s0 = teamParts[0] + "\t" + descParts[0] + "\t" + oppParts[0];
            var s1 = teamParts[1] + "\t" + descParts[1] + "\t" + oppParts[1];
            var s2 = teamParts[2] + "\t" + descParts[2] + "\t" + oppParts[2];
            var s3 = teamParts[3] + "\t" + descParts[3] + "\t" + oppParts[3];
            var list = new List<string> { s0, s1, s2, s3 };
            list.Shuffle();
            list.ForEach(
                item =>
                    {
                        var parts = item.Split('\t');
                        txbMPTeam.Text += parts[0] + "\n";
                        txbMPDesc.Text += parts[1] + "\n";
                        txbMPOpp.Text += parts[2] + "\n";
                    });

            txbMPDesc.Text += "\nG\n\n\nF\n\n\nC\n\n";
            txbMPTeam.Text += "\n" + _teamBestG + "\n" + _teamBestF + "\n" + _teamBestC;
            txbMPOpp.Text += "\n" + _oppBestG + "\n" + _oppBestF + "\n" + _oppBestC;
        }

        private void prepareMatchupPreviewStrengths(
            Dictionary<int, int> dict,
            ref string msgDesc,
            TeamStatsRow tsr,
            int iTeam,
            TeamStatsRow tsropp,
            int iOpp,
            List<int> used,
            ref string msgTeam,
            ref string msgOpp)
        {
            var strengths = (from entry in dict orderby entry.Value ascending select entry.Key).ToList();
            var m = 0;
            var j = 2;
            while (true)
            {
                if (m == j)
                {
                    break;
                }
                if (used.Contains(strengths[m]))
                {
                    m++;
                    j++;
                    continue;
                }
                var def = false;
                switch (strengths[m])
                {
                    case TAbbrPG.APG:
                        msgDesc += "Assists";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.APG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.APG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.APG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.APG]);
                        break;
                    case TAbbrPG.BPG:
                        msgDesc += "Blocks";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.BPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.BPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.BPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.BPG]);
                        break;
                    case TAbbrPG.DRPG:
                        msgDesc += "Def. Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.DRPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.DRPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.DRPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.DRPG]);
                        break;
                    case TAbbrPG.FPG:
                        msgDesc += "Fouls";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.FPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.FPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.FPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.FPG]);
                        break;
                    case TAbbrPG.ORPG:
                        msgDesc += "Off. Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.ORPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.ORPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.ORPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.ORPG]);
                        break;
                    case TAbbrPG.PAPG:
                        msgDesc += "Points Against";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.PAPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.PAPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.PAPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.PAPG]);
                        break;
                    case TAbbrPG.PPG:
                        msgDesc += "Points For";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.PPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.PPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.PPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.PPG]);
                        break;
                    case TAbbrPG.RPG:
                        msgDesc += "Rebounds";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.RPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.RPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.RPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.RPG]);
                        break;
                    case TAbbrPG.SPG:
                        msgDesc += "Steals";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.SPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.SPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.SPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.SPG]);
                        break;
                    case TAbbrPG.TPG:
                        msgDesc += "Turnovers";
                        msgTeam += string.Format("{0:F1} ({1})", tsr.TPG, _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.TPG]);
                        msgOpp += string.Format("{0:F1} ({1})", tsropp.TPG, _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.TPG]);
                        break;
                    case TAbbrPG.FGeff:
                        msgDesc += "Field Goals";
                        msgTeam += string.Format(
                            "{0:F1}-{1:F1} ({2:F3}) ({3})",
                            tsr.FGMPG,
                            tsr.FGAPG,
                            tsr.FGp,
                            _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.FGeff]);
                        msgOpp += string.Format(
                            "{0:F1}-{1:F1} ({2:F3}) ({3})",
                            tsropp.FGMPG,
                            tsropp.FGAPG,
                            tsropp.FGp,
                            _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.FGeff]);
                        break;
                    case TAbbrPG.TPeff:
                        msgDesc += "3 Pointers";
                        msgTeam += string.Format(
                            "{0:F1}-{1:F1} ({2:F3}) ({3})",
                            tsr.TPMPG,
                            tsr.TPAPG,
                            tsr.TPp,
                            _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.TPeff]);
                        msgOpp += string.Format(
                            "{0:F1}-{1:F1} ({2:F3}) ({3})",
                            tsropp.TPMPG,
                            tsropp.TPAPG,
                            tsropp.TPp,
                            _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.TPeff]);
                        break;
                    case TAbbrPG.FTeff:
                        msgDesc += "Free Throws";
                        msgTeam += string.Format(
                            "{0:F1}-{1:F1} ({2:F3}) ({3})",
                            tsr.FTMPG,
                            tsr.FTAPG,
                            tsr.FTp,
                            _seasonRankings.RankingsPerGame[iTeam][TAbbrPG.FTeff]);
                        msgOpp += string.Format(
                            "{0:F1}-{1:F1} ({2:F3}) ({3})",
                            tsropp.FTMPG,
                            tsropp.FTAPG,
                            tsropp.FTp,
                            _seasonRankings.RankingsPerGame[iOpp][TAbbrPG.FTeff]);
                        break;
                    default:
                        j++;
                        def = true;
                        break;
                }
                if (!def)
                {
                    used.Add(strengths[m]);
                    msgDesc += "\n";
                    msgTeam += "\n";
                    msgOpp += "\n";
                }
                m++;
            }
        }

        private void prepareHTHBestPerformersTab(List<PlayerStatsRow> teamPMSRList, List<PlayerStatsRow> oppPMSRList)
        {
            var guards = teamPMSRList.Where(
                delegate(PlayerStatsRow psr)
                    {
                        if (psr.Position1.ToString().EndsWith("G"))
                        {
                            if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                            {
                                return true;
                            }

                            return (!psr.IsInjured);
                        }
                        return false;
                    }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            var fors = teamPMSRList.Where(
                delegate(PlayerStatsRow psr)
                    {
                        if (psr.Position1.ToString().EndsWith("F"))
                        {
                            if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                            {
                                return true;
                            }

                            return (!psr.IsInjured);
                        }
                        return false;
                    }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            var centers = teamPMSRList.Where(
                delegate(PlayerStatsRow psr)
                    {
                        if (psr.Position1.ToString().EndsWith("C"))
                        {
                            if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                            {
                                return true;
                            }

                            return (!psr.IsInjured);
                        }
                        return false;
                    }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                var text = guards[0].GetBestStats(5);
                txbTeam1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + (guards[0].IsInjured ? " (Injured)" : "")
                                + "\n\n" + text;
                var lines = text.Split('\n');
                _teamBestG = string.Format(
                    "{0} {1}\n({2}, {3}, {4})\n", guards[0].FirstName, guards[0].LastName, lines[0], lines[1], lines[2]);

                text = fors[0].GetBestStats(5);
                txbTeam2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + (fors[0].IsInjured ? " (Injured)" : "") + "\n\n"
                                + text;
                lines = text.Split('\n');
                _teamBestF = string.Format(
                    "{0} {1}\n({2}, {3}, {4})\n", fors[0].FirstName, fors[0].LastName, lines[0], lines[1], lines[2]);

                text = centers[0].GetBestStats(5);
                txbTeam3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + (centers[0].IsInjured ? " (Injured)" : "")
                                + "\n\n" + text;
                lines = text.Split('\n');
                _teamBestC = string.Format(
                    "{0} {1}\n({2}, {3}, {4})\n", centers[0].FirstName, centers[0].LastName, lines[0], lines[1], lines[2]);
            }
            catch
            {
            }

            guards = oppPMSRList.Where(
                delegate(PlayerStatsRow psr)
                    {
                        if (psr.Position1.ToString().EndsWith("G"))
                        {
                            if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                            {
                                return true;
                            }

                            return (!psr.IsInjured);
                        }
                        return false;
                    }).ToList();
            guards.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            guards.Reverse();

            fors = oppPMSRList.Where(
                delegate(PlayerStatsRow psr)
                    {
                        if (psr.Position1.ToString().EndsWith("F"))
                        {
                            if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                            {
                                return true;
                            }

                            return (!psr.IsInjured);
                        }
                        return false;
                    }).ToList();
            fors.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            fors.Reverse();

            centers = oppPMSRList.Where(
                delegate(PlayerStatsRow psr)
                    {
                        if (psr.Position1.ToString().EndsWith("C"))
                        {
                            if (chkHTHHideInjured.IsChecked.GetValueOrDefault() == false)
                            {
                                return true;
                            }

                            return (!psr.IsInjured);
                        }
                        return false;
                    }).ToList();
            centers.Sort((pmsr1, pmsr2) => (pmsr1.GmSc.CompareTo(pmsr2.GmSc)));
            centers.Reverse();

            try
            {
                var text = guards[0].GetBestStats(5);
                txbOpp1.Text = "G: " + guards[0].FirstName + " " + guards[0].LastName + (guards[0].IsInjured ? " (Injured)" : "")
                               + "\n\n" + text;
                var lines = text.Split('\n');
                _oppBestG = string.Format(
                    "{0} {1}\n({2}, {3}, {4})\n", guards[0].FirstName, guards[0].LastName, lines[0], lines[1], lines[2]);

                text = fors[0].GetBestStats(5);
                txbOpp2.Text = "F: " + fors[0].FirstName + " " + fors[0].LastName + (fors[0].IsInjured ? " (Injured)" : "") + "\n\n"
                               + text;
                lines = text.Split('\n');
                _oppBestF = string.Format(
                    "{0} {1}\n({2}, {3}, {4})\n", fors[0].FirstName, fors[0].LastName, lines[0], lines[1], lines[2]);

                text = centers[0].GetBestStats(5);
                txbOpp3.Text = "C: " + centers[0].FirstName + " " + centers[0].LastName + (centers[0].IsInjured ? " (Injured)" : "")
                               + "\n\n" + text;
                lines = text.Split('\n');
                _oppBestC = string.Format(
                    "{0} {1}\n({2}, {3}, {4})\n", centers[0].FirstName, centers[0].LastName, lines[0], lines[1], lines[2]);
            }
            catch
            {
            }

            grpHTHBestOpp.Header = cmbOppTeamBest.SelectedItem;
            grpHTHBestTeam.Header = cmbTeam.SelectedItem;
        }

        private void prepareHeadToHeadTab(out List<PlayerStatsRow> teamPMSRList, out List<PlayerStatsRow> oppPMSRList)
        {
            var iown = _curTeam;
            var iopp = _curOpp;

            var dtHTHBS = new DataTable();
            dtHTHBS.Columns.Add("Date");
            dtHTHBS.Columns.Add("Home-Away");
            dtHTHBS.Columns.Add("Result");
            dtHTHBS.Columns.Add("Score");
            dtHTHBS.Columns.Add("GameID");

            var ts = new TeamStats(_curTeam);
            var tsopp = new TeamStats(_curOpp);

            _db = new SQLiteDatabase(MainWindow.CurrentDB);

            if (_dtHTH.Rows.Count > 1)
            {
                _dtHTH.Rows.RemoveAt(_dtHTH.Rows.Count - 1);
            }

            var bsHist = MainWindow.BSHist;

            var bseList =
                bsHist.Where(
                    bse =>
                    (bse.BS.Team1ID == _curTeam && bse.BS.Team2ID == _curOpp)
                    || (bse.BS.Team1ID == _curOpp && bse.BS.Team2ID == _curTeam)).ToList();

            if (rbHTHStatsAnyone.IsChecked.GetValueOrDefault())
            {
                ts = _tst[iown];
                ts.CalcAvg();

                tsopp = _tst[iopp];
                tsopp.CalcAvg();
            }
            else
            {
                foreach (var bse in bseList)
                {
                    TeamStats.AddTeamStatsFromBoxScore(bse.BS, ref ts, ref tsopp, true);
                }
            }

            //ts.CalcMetrics(tsopp);
            //tsopp.CalcMetrics(ts);
            var ls = new TeamStats();
            ls.AddTeamStats(ts, Span.SeasonAndPlayoffsToSeason);
            ls.AddTeamStats(tsopp, Span.SeasonAndPlayoffsToSeason);
            var keys = _pst.Keys.ToList();
            teamPMSRList = new List<PlayerStatsRow>();
            oppPMSRList = new List<PlayerStatsRow>();
            foreach (var key in keys)
            {
                if (_pst[key].TeamF == ts.ID)
                {
                    teamPMSRList.Add(new PlayerStatsRow(_pst[key]));
                }
                else if (_pst[key].TeamF == tsopp.ID)
                {
                    oppPMSRList.Add(new PlayerStatsRow(_pst[key]));
                }
            }

            foreach (var bse in bseList)
            {
                int t1PTS = bse.BS.PTS1;
                int t2PTS = bse.BS.PTS2;
                var bsr = dtHTHBS.NewRow();
                bsr["Date"] = bse.BS.GameDate.ToString().Split(' ')[0];
                if (bse.BS.Team1ID.Equals(_curTeam))
                {
                    bsr["Home-Away"] = "Away";

                    if (t1PTS > t2PTS)
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

                    if (t2PTS > t1PTS)
                    {
                        bsr["Result"] = "W";
                    }
                    else
                    {
                        bsr["Result"] = "L";
                    }
                }

                bsr["Score"] = bse.BS.PTS1 + "-" + bse.BS.PTS2;
                bsr["GameID"] = bse.BS.ID.ToString();

                dtHTHBS.Rows.Add(bsr);
            }

            _dtHTH.Clear();

            var dr = _dtHTH.NewRow();

            createDataRowFromTeamStats(ts, ref dr, "Averages");

            _dtHTH.Rows.Add(dr);

            dr = _dtHTH.NewRow();

            createDataRowFromTeamStats(tsopp, ref dr, "Opp Avg");

            _dtHTH.Rows.Add(dr);

            dr = _dtHTH.NewRow();

            createDataRowFromTeamStats(ts, ref dr, "Playoffs", true);

            _dtHTH.Rows.Add(dr);

            dr = _dtHTH.NewRow();

            createDataRowFromTeamStats(tsopp, ref dr, "Opp Pl Avg", true);

            _dtHTH.Rows.Add(dr);

            _dvHTH = new DataView(_dtHTH) { AllowNew = false, AllowEdit = false };

            dgvHTHStats.DataContext = _dvHTH;

            var dvHTHBS = new DataView(dtHTHBS) { AllowNew = false, AllowEdit = false };

            dgvHTHBoxScores.DataContext = dvHTHBS;
        }

        /// <summary>Creates a data row from a TeamStats instance.</summary>
        /// <param name="ts">The TeamStats instance.</param>
        /// <param name="dr">The data row to be edited.</param>
        /// <param name="title">The title for the row's Type or Name column.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the row will present the team's playoff stats; otherwise, the regular season's.
        /// </param>
        private static void createDataRowFromTeamStats(TeamStats ts, ref DataRow dr, string title, bool playoffs = false)
        {
            try
            {
                dr["Type"] = title;
            }
            catch
            {
                dr["Name"] = title;
            }
            float[] PerGame;
            if (!playoffs)
            {
                PerGame = ts.PerGame;
                dr["Games"] = ts.GetGames();
                dr["Wins"] = ts.Record[0].ToString();
                dr["Losses"] = ts.Record[1].ToString();
            }
            else
            {
                PerGame = ts.PlPerGame;
                dr["Games"] = ts.GetPlayoffGames();
                dr["Wins"] = ts.PlRecord[0].ToString();
                dr["Losses"] = ts.PlRecord[1].ToString();
            }
            dr["W%"] = String.Format("{0:F3}", PerGame[TAbbrPG.Wp]);
            dr["Weff"] = String.Format("{0:F2}", PerGame[TAbbrPG.Weff]);
            dr["PF"] = String.Format("{0:F1}", PerGame[TAbbrPG.PPG]);
            dr["PA"] = String.Format("{0:F1}", PerGame[TAbbrPG.PAPG]);
            dr["PD"] = String.Format("{0:F1}", PerGame[TAbbrPG.PD]);
            dr["FG"] = String.Format("{0:F3}", PerGame[TAbbrPG.FGp]);
            dr["FGeff"] = String.Format("{0:F2}", PerGame[TAbbrPG.FGeff]);
            dr["3PT"] = String.Format("{0:F3}", PerGame[TAbbrPG.TPp]);
            dr["3Peff"] = String.Format("{0:F2}", PerGame[TAbbrPG.TPeff]);
            dr["FT"] = String.Format("{0:F3}", PerGame[TAbbrPG.FTp]);
            dr["FTeff"] = String.Format("{0:F2}", PerGame[TAbbrPG.FTeff]);
            dr["REB"] = String.Format("{0:F1}", PerGame[TAbbrPG.RPG]);
            dr["OREB"] = String.Format("{0:F1}", PerGame[TAbbrPG.ORPG]);
            dr["DREB"] = String.Format("{0:F1}", PerGame[TAbbrPG.DRPG]);
            dr["AST"] = String.Format("{0:F1}", PerGame[TAbbrPG.APG]);
            dr["TO"] = String.Format("{0:F1}", PerGame[TAbbrPG.TPG]);
            dr["STL"] = String.Format("{0:F1}", PerGame[TAbbrPG.SPG]);
            dr["BLK"] = String.Format("{0:F1}", PerGame[TAbbrPG.BPG]);
            dr["FOUL"] = String.Format("{0:F1}", PerGame[TAbbrPG.FPG]);
            dr["MINS"] = String.Format("{0:F1}", PerGame[TAbbrPG.MPG]);
        }

        private static void createTeamStatsFromDataRow(ref TeamStats ts, DataRow dr, bool playoffs = false)
        {
            var Totals = !playoffs ? ts.Totals : ts.PlTotals;
            var PerGame = !playoffs ? ts.PerGame : ts.PlPerGame;
            var Record = !playoffs ? ts.Record : ts.PlRecord;
            Record[0] = ParseCell.GetUInt16(dr, "Wins");
            Record[1] = ParseCell.GetUInt16(dr, "Losses");
            PerGame[TAbbrPG.Wp] = ParseCell.GetFloat(dr, "W%");
            PerGame[TAbbrPG.Weff] = ParseCell.GetFloat(dr, "Weff");
            PerGame[TAbbrPG.PPG] = ParseCell.GetFloat(dr, "PF");
            PerGame[TAbbrPG.PAPG] = ParseCell.GetFloat(dr, "PA");
            PerGame[TAbbrPG.PD] = ParseCell.GetFloat(dr, "PD");
            PerGame[TAbbrPG.FGp] = ParseCell.GetFloat(dr, "FG");
            PerGame[TAbbrPG.FGeff] = ParseCell.GetFloat(dr, "FGeff");
            PerGame[TAbbrPG.TPp] = ParseCell.GetFloat(dr, "3PT");
            PerGame[TAbbrPG.TPeff] = ParseCell.GetFloat(dr, "3Peff");
            PerGame[TAbbrPG.FTp] = ParseCell.GetFloat(dr, "FT");
            PerGame[TAbbrPG.FTeff] = ParseCell.GetFloat(dr, "FTeff");
            PerGame[TAbbrPG.RPG] = ParseCell.GetFloat(dr, "REB");
            PerGame[TAbbrPG.ORPG] = ParseCell.GetFloat(dr, "OREB");
            PerGame[TAbbrPG.DRPG] = ParseCell.GetFloat(dr, "DREB");
            PerGame[TAbbrPG.APG] = ParseCell.GetFloat(dr, "AST");
            PerGame[TAbbrPG.TPG] = ParseCell.GetFloat(dr, "TO");
            PerGame[TAbbrPG.SPG] = ParseCell.GetFloat(dr, "STL");
            PerGame[TAbbrPG.BPG] = ParseCell.GetFloat(dr, "BLK");
            PerGame[TAbbrPG.FPG] = ParseCell.GetFloat(dr, "FOUL");
            PerGame[TAbbrPG.MPG] = ParseCell.GetFloat(dr, "MINS");

            var games = Record[0] + Record[1];

            Totals[TAbbrT.PF] = Convert.ToUInt32(PerGame[TAbbrPG.PPG] * games);
            Totals[TAbbrT.PA] = Convert.ToUInt32(PerGame[TAbbrPG.PAPG] * games);

            Totals[TAbbrT.FGM] = Convert.ToUInt32((PerGame[TAbbrPG.FGeff] / PerGame[TAbbrPG.FGp]) * games);
            Totals[TAbbrT.FGA] = Convert.ToUInt32(Totals[TAbbrT.FGM] / PerGame[TAbbrPG.FGp]);

            Totals[TAbbrT.TPM] = Convert.ToUInt32((PerGame[TAbbrPG.TPeff] / PerGame[TAbbrPG.TPp]) * games);
            Totals[TAbbrT.TPA] = Convert.ToUInt32(Totals[TAbbrT.TPM] / PerGame[TAbbrPG.TPp]);

            Totals[TAbbrT.FTM] = Convert.ToUInt32((PerGame[TAbbrPG.FTeff] / PerGame[TAbbrPG.FTp]) * games);
            Totals[TAbbrT.FTA] = Convert.ToUInt32(Totals[TAbbrT.FTM] / PerGame[TAbbrPG.FTp]);

            Totals[TAbbrT.DREB] = Convert.ToUInt32(PerGame[TAbbrPG.DRPG] * games);
            Totals[TAbbrT.OREB] = Convert.ToUInt32(PerGame[TAbbrPG.ORPG] * games);
            Totals[TAbbrT.AST] = Convert.ToUInt32(PerGame[TAbbrPG.APG] * games);
            Totals[TAbbrT.TOS] = Convert.ToUInt32(PerGame[TAbbrPG.TPG] * games);
            Totals[TAbbrT.STL] = Convert.ToUInt32(PerGame[TAbbrPG.SPG] * games);
            Totals[TAbbrT.BLK] = Convert.ToUInt32(PerGame[TAbbrPG.BPG] * games);
            Totals[TAbbrT.FOUL] = Convert.ToUInt32(PerGame[TAbbrPG.FPG] * games);
            Totals[TAbbrT.MINS] = Convert.ToUInt32(PerGame[TAbbrPG.MPG] * games);
        }

        /// <summary>
        ///     Handles the Loaded event of the Window control. Connects the team and player stats dictionaries to the Main window's,
        ///     calculates team rankingsPerGame, prepares the data tables and sets DataGrid parameters.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Prepare Data Tables

            _dtOv = new DataTable();

            _dtOv.Columns.Add("Type");
            _dtOv.Columns.Add("Games");
            _dtOv.Columns.Add("Wins (W%)");
            _dtOv.Columns.Add("Losses (Weff)");
            _dtOv.Columns.Add("PF");
            _dtOv.Columns.Add("PA");
            _dtOv.Columns.Add("PD");
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
            _dtOv.Columns.Add("MINS");

            _dtHTH = new DataTable();

            _dtHTH.Columns.Add("Type");
            _dtHTH.Columns.Add("Games");
            _dtHTH.Columns.Add("Wins");
            _dtHTH.Columns.Add("Losses");
            _dtHTH.Columns.Add("W%");
            _dtHTH.Columns.Add("Weff");
            _dtHTH.Columns.Add("PF");
            _dtHTH.Columns.Add("PA");
            _dtHTH.Columns.Add("PD");
            _dtHTH.Columns.Add("FG");
            _dtHTH.Columns.Add("FGeff");
            _dtHTH.Columns.Add("3PT");
            _dtHTH.Columns.Add("3Peff");
            _dtHTH.Columns.Add("FT");
            _dtHTH.Columns.Add("FTeff");
            _dtHTH.Columns.Add("REB");
            _dtHTH.Columns.Add("OREB");
            _dtHTH.Columns.Add("DREB");
            _dtHTH.Columns.Add("AST");
            _dtHTH.Columns.Add("TO");
            _dtHTH.Columns.Add("STL");
            _dtHTH.Columns.Add("BLK");
            _dtHTH.Columns.Add("FOUL");
            _dtHTH.Columns.Add("MINS");

            _dtSs = new DataTable();

            _dtSs.Columns.Add("Type");
            _dtSs.Columns.Add("Games");
            _dtSs.Columns.Add("Wins");
            _dtSs.Columns.Add("Losses");
            _dtSs.Columns.Add("W%");
            _dtSs.Columns.Add("Weff");
            _dtSs.Columns.Add("PF");
            _dtSs.Columns.Add("PA");
            _dtSs.Columns.Add("PD");
            _dtSs.Columns.Add("FG");
            _dtSs.Columns.Add("FGeff");
            _dtSs.Columns.Add("3PT");
            _dtSs.Columns.Add("3Peff");
            _dtSs.Columns.Add("FT");
            _dtSs.Columns.Add("FTeff");
            _dtSs.Columns.Add("REB");
            _dtSs.Columns.Add("OREB");
            _dtSs.Columns.Add("DREB");
            _dtSs.Columns.Add("AST");
            _dtSs.Columns.Add("TO");
            _dtSs.Columns.Add("STL");
            _dtSs.Columns.Add("BLK");
            _dtSs.Columns.Add("FOUL");
            _dtSs.Columns.Add("MINS");

            _dtYea = new DataTable();

            _dtYea.Columns.Add("Type");
            _dtYea.Columns.Add("Games");
            _dtYea.Columns.Add("Wins");
            _dtYea.Columns.Add("Losses");
            _dtYea.Columns.Add("W%");
            _dtYea.Columns.Add("Weff");
            _dtYea.Columns.Add("PF");
            _dtYea.Columns.Add("PA");
            _dtYea.Columns.Add("PD");
            _dtYea.Columns.Add("FG");
            _dtYea.Columns.Add("FGeff");
            _dtYea.Columns.Add("3PT");
            _dtYea.Columns.Add("3Peff");
            _dtYea.Columns.Add("FT");
            _dtYea.Columns.Add("FTeff");
            _dtYea.Columns.Add("REB");
            _dtYea.Columns.Add("OREB");
            _dtYea.Columns.Add("DREB");
            _dtYea.Columns.Add("AST");
            _dtYea.Columns.Add("TO");
            _dtYea.Columns.Add("STL");
            _dtYea.Columns.Add("BLK");
            _dtYea.Columns.Add("FOUL");
            _dtYea.Columns.Add("MINS");

            _dtBS = new DataTable();
            _dtBS.Columns.Add("Date", typeof(DateTime));
            _dtBS.Columns.Add("Opponent");
            _dtBS.Columns.Add("Home-Away");
            _dtBS.Columns.Add("Result");
            _dtBS.Columns.Add("Score");
            _dtBS.Columns.Add("GameID");

            #endregion

            linkInternalsToMainWindow();

            populateTeamsCombo();

            _changingTimeframe = true;
            dtpEnd.SelectedDate = MainWindow.Tf.EndDate;
            dtpStart.SelectedDate = MainWindow.Tf.StartDate;
            populateSeasonCombo();
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

            populateSituationalsCombo();

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

            var shotOrigins = ShotEntry.ShotOrigins.Values.ToList();
            shotOrigins.Insert(0, "Any");
            cmbShotOrigin.ItemsSource = shotOrigins;
            cmbShotOrigin.SelectedIndex = 0;

            var shotTypes = ShotEntry.ShotTypes.Values.ToList();
            shotTypes.Insert(0, "Any");
            cmbShotType.ItemsSource = shotTypes;
            cmbShotType.SelectedIndex = 0;

            _bseListSea = new List<BoxScoreEntry>();
            _bseListPl = new List<BoxScoreEntry>();

            cmbTeam.SelectedIndex = -1;
            if (_teamIDToLoad != -1)
            {
                cmbTeam.SelectedItem = _tst[_teamIDToLoad].DisplayName;
            }
            else if (!String.IsNullOrWhiteSpace(_teamToLoad))
            {
                cmbTeam.SelectedItem = _teamToLoad;
            }

            cmbOppTeam.SelectedIndex = -1;

            chkHTHHideInjured.IsChecked = SQLiteIO.GetSetting("HTHHideInjured", true);
            chkMatchupHideInjured.IsChecked = chkHTHHideInjured.IsChecked;

            populateGraphStatCombo();
            cmbGraphStat.SelectedIndex = 0;
            cmbGraphInterval.SelectedIndex = 0;
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

        private void populateSituationalsCombo()
        {
            var situationals = new List<string> { "Starters", "Bench" };
            var temp = new List<string>();
            temp.AddRange(PlayerStatsHelper.MetricsNames);
            temp.AddRange(PlayerStatsHelper.PerGame.Values);
            temp.AddRange(PlayerStatsHelper.Totals.Values);
            var psrProps = typeof(PlayerStatsRow).GetProperties().Select(prop => prop.Name).ToList();
            situationals.AddRange(
                from t in temp let realName = t.Replace("%", "p").Replace("3", "T") where psrProps.Contains(realName) select t);
            cmbSituational.ItemsSource = situationals;
            cmbSituational.SelectedIndex = 0;
        }

        private void linkInternalsToMainWindow(Task task = null)
        {
            _tst = MainWindow.TST;
            _tstOpp = MainWindow.TSTOpp;
            _pst = MainWindow.PST;

            _seasonRankings = MainWindow.SeasonTeamRankings;
            _playoffRankings = MainWindow.PlayoffTeamRankings;
        }

        /// <summary>
        ///     Handles the Checked event of the rbHTHStatsAnyone control. Used to include all the teams' games in the stat calculations, no
        ///     matter the opponent.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbHTHStatsAnyone_Checked(object sender, RoutedEventArgs e)
        {
            if (_changingOppRange)
            {
                return;
            }

            if (Equals(sender, rbHTHStatsAnyone))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsAnyoneBest.IsChecked = rbHTHStatsAnyone.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else if (Equals(sender, rbHTHStatsAnyoneBest))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsAnyone.IsChecked = rbHTHStatsAnyoneBest.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsAnyoneBest.IsChecked = rbHTHStatsAnyone.IsChecked;
                }
                catch (Exception)
                {
                    _changingOppRange = false;
                    return;
                }
            }
            cmbOppTeam_SelectionChanged(sender, null);
            _changingOppRange = false;
        }

        /// <summary>
        ///     Handles the Checked event of the rbHTHStatsEachOther control. Used to include only stats from the games these two teams have
        ///     played against each other.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void rbHTHStatsEachOther_Checked(object sender, RoutedEventArgs e)
        {
            if (_changingOppRange)
            {
                return;
            }

            if (Equals(sender, rbHTHStatsEachOther))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsEachOtherBest.IsChecked = rbHTHStatsEachOther.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else if (Equals(sender, rbHTHStatsEachOtherBest))
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsEachOther.IsChecked = rbHTHStatsEachOtherBest.IsChecked;
                }
                catch
                {
                    _changingOppRange = false;
                    return;
                }
            }
            else
            {
                try
                {
                    _changingOppRange = true;
                    rbHTHStatsEachOtherBest.IsChecked = rbHTHStatsEachOther.IsChecked;
                }
                catch (Exception)
                {
                    _changingOppRange = false;
                    return;
                }
            }
            cmbOppTeam_SelectionChanged(sender, null);
            _changingOppRange = false;
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the cmbSeasonNum control. Loads the team and player stats and information for the new
        ///     season, repopulates the teams combo and tries to switch to the same team again.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="SelectionChangedEventArgs" /> instance containing the event data.
        /// </param>
        private async void cmbSeasonNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_changingTimeframe)
            {
                _changingTimeframe = true;
                _lastEndDate = DateTime.MinValue;
                _lastStartDate = DateTime.MinValue;
                rbStatsAllTime.IsChecked = true;
                if (cmbSeasonNum.SelectedIndex == -1)
                {
                    _changingTimeframe = false;
                    return;
                }

                _curSeason = ((KeyValuePair<int, string>) (((cmbSeasonNum)).SelectedItem)).Key;
                if (_curSeason == MainWindow.CurSeason && !MainWindow.Tf.IsBetween)
                {
                    _changingTimeframe = false;
                    return;
                }

                MainWindow.Tf = new Timeframe(_curSeason);
                MainWindow.ChangeSeason(_curSeason);
                await updateData();
                /*
                SQLiteIO.LoadSeason(_curSeason);

                linkInternalsToMainWindow();
                */
                _changingTimeframe = false;
            }
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the AnyPlayerDataGrid control. Views the selected player in the Player Overview window,
        ///     and reloads their team's stats aftewrards.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private void anyPlayerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EventHandlers.AnyPlayerDataGrid_MouseDoubleClick(sender, e))
            {
                var curIndex = cmbTeam.SelectedIndex;
                cmbTeam.SelectedIndex = -1;
                cmbTeam.SelectedIndex = curIndex;
            }
        }

        /// <summary>Handles the MouseDoubleClick event of the dgvHTHBoxScores control. Views the selected box score in the Box Score window.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="MouseButtonEventArgs" /> instance containing the event data.
        /// </param>
        private async void dgvHTHBoxScores_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvHTHBoxScores.SelectedCells.Count > 0)
            {
                var row = (DataRowView) dgvHTHBoxScores.SelectedItems[0];
                var gameid = Convert.ToInt32(row["GameID"].ToString());

                var bsw = new BoxScoreWindow(BoxScoreWindow.Mode.View, gameid);
                try
                {
                    if (bsw.ShowDialog() == true)
                    {
                        await updateData();
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Handles the Closing event of the Window control. Updates the Main window's team & player stats dictionaries to match the ones
        ///     in the Team Overview window before closing.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="CancelEventArgs" /> instance containing the event data.
        /// </param>
        private void window_Closing(object sender, CancelEventArgs e)
        {
            MainWindow.TST = _tst;
            MainWindow.TSTOpp = _tstOpp;
            MainWindow.PST = _pst;

            Tools.SetRegistrySetting("TeamOvHeight", Height);
            Tools.SetRegistrySetting("TeamOvWidth", Width);
            Tools.SetRegistrySetting("TeamOvX", Left);
            Tools.SetRegistrySetting("TeamOvY", Top);
        }

        /// <summary>
        ///     Handles the Click event of the btnChangeName control. Allows the user to update the team's displayName for the current
        ///     season.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnChangeName_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == -1)
            {
                return;
            }
            if (MainWindow.Tf.IsBetween)
            {
                MessageBox.Show(
                    "Please switch to a season timeframe before trying to change the name of a team.",
                    App.AppName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            var newname = "";
            try
            {
                while (true)
                {
                    var ibw = new InputBoxWindow("Please enter the new name for the team", _tst[_curTeam].DisplayName);
                    if (ibw.ShowDialog() != true)
                    {
                        return;
                    }
                    newname = InputBoxWindow.UserInput;
                    if (_tst.Any(pair => pair.Value.DisplayName == newname))
                    {
                        MessageBox.Show("A team already exists with that name.");
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch
            {
                return;
            }
            var dict = new Dictionary<string, string> { { "DisplayName", newname } };
            _db.Update(MainWindow.TeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlTeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.OppT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlOppT, dict, "Name LIKE \"" + _curTeam + "\"");

            var teamid = _curTeam;
            _tst[teamid].DisplayName = newname;
            _tstOpp[teamid].DisplayName = newname;

            MainWindow.TST = _tst;
            MainWindow.TSTOpp = _tstOpp;

            populateTeamsCombo();

            cmbTeam.SelectedItem = newname;
        }

        /// <summary>
        ///     Handles the Sorting event of the StatColumn control. Uses a custom Sorting event handler that sorts a stat in descending
        ///     order, if it's not sorted already.
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
        ///     Handles the PreviewKeyDown event of the dgvTeamStats control. Allows the user to paste and import tab-separated values
        ///     formatted team stats into the current team.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvTeamStats_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;

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

                        case "Playoffs":
                            tryChangeRow(6, dict);
                            break;

                        case "Opp Stats":
                            tryChangeRow(3, dict);
                            break;

                        case "Opp Pl Stats":
                            tryChangeRow(9, dict);
                            break;
                    }
                }

                createViewAndUpdateOverview();

                //btnSaveCustomTeam_Click(null, null);
            }
        }

        /// <summary>Tries to parse the data in the dictionary and change the values of the specified Overview row.</summary>
        /// <param name="row">The row.</param>
        /// <param name="dict">The dict.</param>
        private void tryChangeRow(int row, Dictionary<string, string> dict)
        {
            _dtOv.Rows[row].TryChangeValue(dict, "Games", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "Wins (W%)", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "Losses (Weff)", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "PF", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "PA", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "FG", typeof(UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "3PT", typeof(UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "FT", typeof(UInt16), "-");
            _dtOv.Rows[row].TryChangeValue(dict, "REB", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "OREB", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "DREB", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "AST", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "TO", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "STL", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "BLK", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "FOUL", typeof(UInt16));
            _dtOv.Rows[row].TryChangeValue(dict, "MINS", typeof(UInt16));
        }

        /// <summary>Allows the user to paste and import multiple player stats into the team's players.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="KeyEventArgs" /> instance containing the event data.
        /// </param>
        private void anyPlayerDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;

                var dictList = CSV.DictionaryListFromTSVString(Clipboard.GetText());

                var list = Equals(sender, dgvPlayerStats) ? _psrList : _plPSRList;
                for (var j = 0; j < dictList.Count; j++)
                {
                    var dict = dictList[j];
                    int id;
                    try
                    {
                        id = Convert.ToInt32(dict["ID"]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var matching = new List<PlayerStats>();
                            if (dict.ContainsKey("Last Name"))
                            {
                                matching =
                                    MainWindow.PST.Values.Where(
                                        ps => ps.LastName == dict["Last Name"] && ps.FirstName == dict["First Name"]).ToList();
                            }
                            else if (dict.ContainsKey("Name"))
                            {
                                if (dict["Name"].Contains(", "))
                                {
                                    var parts = dict["Name"].Split(',');
                                    matching =
                                        MainWindow.PST.Values.Where(ps => ps.LastName == parts[0] && ps.FirstName == parts[1]).ToList();
                                }
                                else
                                {
                                    var parts = dict["Name"].Split(new[] { ' ' }, 2);
                                    matching =
                                        MainWindow.PST.Values.Where(ps => ps.LastName == parts[1] && ps.FirstName == parts[0]).ToList();
                                }
                            }
                            if (matching.Count == 0)
                            {
                                throw new Exception();
                            }
                            if (matching.Count > 1)
                            {
                                try
                                {
                                    matching = matching.Where(ps => ps.TeamF == _curTeam).ToList();
                                }
                                catch
                                {
                                }
                            }
                            if (matching.Count > 1)
                            {
                                throw new Exception();
                            }
                            else
                            {
                                id = matching[0].ID;
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(
                                "Player in row " + (j + 1)
                                + " couldn't be determined either by ID or Full Name. Make sure the pasted data has the proper headers. "
                                + "\nUse a copy of this table as a base by copying it and pasting it into a spreadsheet and making changes there, if needed.");
                            return;
                        }
                    }
                    try
                    {
                        var psr = list.Single(ps => ps.ID == id);
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
                    "NBA Stats Tracker",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        ///     Handles the Click event of the chkHTHHideInjured control. Used to ignore injured players while doing Head-To-Head Best
        ///     Performers analysis.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void chkHTHHideInjured_Click(object sender, RoutedEventArgs e)
        {
            chkMatchupHideInjured.IsChecked = chkHTHHideInjured.IsChecked;
            SQLiteIO.SetSetting("HTHHideInjured", chkHTHHideInjured.IsChecked == true);

            cmbOppTeam_SelectionChanged(null, null);
        }

        /// <summary>Handles the Click event of the btnChangeDivision control. Allows the user to change the team's division.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnChangeDivision_Click(object sender, RoutedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == -1)
            {
                return;
            }
            if (MainWindow.Tf.IsBetween)
            {
                MessageBox.Show(
                    "Please switch to a season timeframe before trying to change the division of a team.",
                    App.AppName,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            var teamid = _curTeam;
            var i = MainWindow.Divisions.TakeWhile(div => _tst[teamid].Division != div.ID).Count();
            var ccw = new ComboChoiceWindow(ComboChoiceWindow.Mode.Division, i);
            ccw.ShowDialog();

            var parts = InputBoxWindow.UserInput.Split(new[] { ": " }, 2, StringSplitOptions.None);
            var myDiv = MainWindow.Divisions.Find(division => division.Name == parts[1]);

            _tst[teamid].Division = myDiv.ID;
            _tstOpp[teamid].Division = myDiv.ID;

            var dict = new Dictionary<string, string>
                {
                    { "Division", _tst[teamid].Division.ToString() },
                    { "Conference", _tst[teamid].Conference.ToString() }
                };
            _db.Update(MainWindow.TeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlTeamsT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.OppT, dict, "Name LIKE \"" + _curTeam + "\"");
            _db.Update(MainWindow.PlOppT, dict, "Name LIKE \"" + _curTeam + "\"");

            MainWindow.TST = _tst;
            MainWindow.TSTOpp = _tstOpp;
        }

        /// <summary>
        ///     Handles the Sorting event of the dgvBoxScores control. Uses a custom Sorting event handler that sorts dates or a stat in
        ///     descending order, if it's not sorted already.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="DataGridSortingEventArgs" /> instance containing the event data.
        /// </param>
        private void dgvBoxScores_Sorting(object sender, DataGridSortingEventArgs e)
        {
            statColumn_Sorting(sender, e);
        }

        private void btnAddPastStats_Click(object sender, RoutedEventArgs e)
        {
            var adw = new AddStatsWindow(true, _curTeam);
            if (adw.ShowDialog() == true)
            {
                updateYearlyStats();
            }
        }

        private void btnCopySeasonScouting_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonScoutingReport.Text);
        }

        private void btnCopyPlayoffsScouting_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsScoutingReport.Text);
        }

        private void btnCopySeasonFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbSeasonFacts.Text);
        }

        private void btnCopyPlayoffsFacts_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txbPlayoffsFacts.Text);
        }

        private void cmbSituational_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTeam.SelectedIndex == -1 || cmbSituational.SelectedIndex == -1)
            {
                return;
            }

            calculateSituational(cmbSituational.SelectedItem.ToString());
        }

        private void btnCopyMPToClipboard_Click(object sender, RoutedEventArgs e)
        {
            var all = "";
            var descHeader = txbMPDescHeader.Text.Split('\n');
            var teamHeader = txbMPTeamHeader.Text.Split('\n');
            var oppHeader = txbMPOppHeader.Text.Split('\n');
            for (var i = 0; i < descHeader.Length; i++)
            {
                all += String.Format("{0}\t{1}\t{2}\n", teamHeader[i], descHeader[i], oppHeader[i]);
            }
            all += "\n";
            var descMsg = txbMPDesc.Text.Split('\n');
            var teamMsg = txbMPTeam.Text.Split('\n');
            var oppMsg = txbMPOpp.Text.Split('\n');
            for (var i = 0; i < descMsg.Length; i++)
            {
                all += String.Format("{0}\t{1}\t{2}\n", teamMsg[i], descMsg[i], oppMsg[i]);
            }
            Clipboard.SetText(all);
        }

        private void btnTrade_Click(object sender, RoutedEventArgs e)
        {
            var w = new DualListWindow(_curTeam, _curTeam == 0 ? 1 : 0);
            var res = w.ShowDialog();

            if (res == true)
            {
                MessageBox.Show("Players traded successfully. The database will be saved now. This may take a few moments.");
                MainWindow.MWInstance.btnSaveCurrentSeason_Click(null, null);
                linkInternalsToMainWindow();
                cmbTeam_SelectionChanged(null, null);
            }
        }

        private void cmbShotOrigin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShotOrigin.SelectedIndex == -1)
            {
                return;
            }

            updatePBPStats();
        }

        private void updatePBPStats()
        {
            dgShooting.ItemsSource = null;
            dgOther.ItemsSource = null;

            if (_curts == null || _curts.ID == -1)
            {
                return;
            }

            int origin;
            if (cmbShotOrigin.SelectedIndex <= 0)
            {
                origin = -1;
            }
            else
            {
                origin = ShotEntry.ShotOrigins.Single(o => o.Value == cmbShotOrigin.SelectedItem.ToString()).Key;
            }
            int type;
            if (cmbShotType.SelectedIndex <= 0)
            {
                type = -1;
            }
            else
            {
                type = ShotEntry.ShotTypes.Single(o => o.Value == cmbShotType.SelectedItem.ToString()).Key;
            }
            var shstList = ShotEntry.ShotDistances.Values.Select(distance => new PlayerPBPStats { Description = distance }).ToList();
            shstList.Add(new PlayerPBPStats { Description = "Total" });
            var lastIndex = shstList.Count - 1;

            foreach (var bse in _bseList)
            {
                var teamPlayerIDs = bse.PBSList.Where(o => o.TeamID == _curts.ID).Select(o => o.PlayerID).ToList();
                var teamPBPEList =
                    bse.PBPEList.Where(o => teamPlayerIDs.Contains(o.Player1ID) || teamPlayerIDs.Contains(o.Player2ID)).ToList();
                PlayerPBPStats.AddShotsToList(ref shstList, teamPlayerIDs, teamPBPEList, origin, type);
                shstList[lastIndex].AddOtherStats(teamPlayerIDs, teamPBPEList, false);
            }

            dgShooting.ItemsSource = shstList;
            dgOther.ItemsSource = new List<PlayerPBPStats> { shstList[lastIndex] };
        }

        private void cmbShotType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShotType.SelectedIndex == -1)
            {
                return;
            }

            updatePBPStats();
        }

        private void btnShotChart_Click(object sender, RoutedEventArgs e)
        {
            if (_curts == null || _curts.ID == -1 || _bseList == null)
            {
                return;
            }

            var dict = new Dictionary<int, PlayerPBPStats>();
            for (var i = 1; i <= 20; i++)
            {
                dict.Add(i, new PlayerPBPStats());
            }

            foreach (var bse in _bseList)
            {
                var teamPlayerIDs = bse.PBSList.Where(o => o.TeamID == _curts.ID).Select(o => o.PlayerID).ToList();
                var list = bse.PBPEList.Where(o => teamPlayerIDs.Contains(o.Player1ID) || teamPlayerIDs.Contains(o.Player2ID)).ToList();
                PlayerPBPStats.AddShotsToDictionary(ref dict, teamPlayerIDs, list);
            }

            var w = new ShotChartWindow(dict, Equals(sender, btnShotChartOff));
            w.ShowDialog();
        }

        private void rbPlayerStatsSeason_Click(object sender, RoutedEventArgs e)
        {
            rbMetricStatsSeason.IsChecked = true;
            rbShootingStatsSeason.IsChecked = true;
            rbOtherStatsSeason.IsChecked = true;
            updatePlayerAndMetricStats();
        }

        private void rbPlayerStatsPlayoff_Click(object sender, RoutedEventArgs e)
        {
            rbMetricStatsPlayoff.IsChecked = true;
            rbShootingStatsPlayoff.IsChecked = true;
            rbOtherStatsPlayoff.IsChecked = true;
            updatePlayerAndMetricStats();
        }

        private void rbMetricStatsSeason_Click(object sender, RoutedEventArgs e)
        {
            rbPlayerStatsSeason.IsChecked = true;
            rbShootingStatsSeason.IsChecked = true;
            rbOtherStatsSeason.IsChecked = true;
            updatePlayerAndMetricStats();
        }

        private void rbMetricStatsPlayoff_Click(object sender, RoutedEventArgs e)
        {
            rbPlayerStatsPlayoff.IsChecked = true;
            rbShootingStatsPlayoff.IsChecked = true;
            rbOtherStatsPlayoff.IsChecked = true;
            updatePlayerAndMetricStats();
        }

        private void tbcTeamOverview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Equals(tbcTeamOverview.SelectedItem, tabShootingStats) || Equals(tbcTeamOverview.SelectedItem, tabOtherStats))
            {
                updatePlayerShootingStats();
            }
        }

        private void updatePlayerShootingStats()
        {
            dgvPlayerShootingStats.ItemsSource = null;
            dgvPlayerOtherStats.ItemsSource = null;
            if (_curts == null || _curts.ID == -1 || _psrList == null)
            {
                return;
            }

            var psrList = rbShootingStatsSeason.IsChecked == true ? _psrList : _plPSRList;
            var bseList = rbShootingStatsSeason.IsChecked == true ? _bseListSea : _bseListPl;
            foreach (var psr in psrList)
            {
                psr.PopulatePBPSList(bseList);
            }
            dgvPlayerShootingStats.ItemsSource = psrList;
            dgvPlayerOtherStats.ItemsSource = dgvPlayerShootingStats.ItemsSource;
        }

        private void rbShootingStatsSeason_Click(object sender, RoutedEventArgs e)
        {
            rbPlayerStatsSeason.IsChecked = true;
            rbMetricStatsSeason.IsChecked = true;
            rbOtherStatsSeason.IsChecked = true;
            updatePlayerAndMetricStats();
            updatePlayerShootingStats();
        }

        private void rbShootingStatsPlayoff_Click(object sender, RoutedEventArgs e)
        {
            rbPlayerStatsPlayoff.IsChecked = true;
            rbMetricStatsPlayoff.IsChecked = true;
            rbOtherStatsPlayoff.IsChecked = true;
            updatePlayerAndMetricStats();
            updatePlayerShootingStats();
        }

        private void rbOtherStatsSeason_Click(object sender, RoutedEventArgs e)
        {
            rbPlayerStatsSeason.IsChecked = true;
            rbMetricStatsSeason.IsChecked = true;
            rbShootingStatsSeason.IsChecked = true;
            updatePlayerAndMetricStats();
            updatePlayerShootingStats();
        }

        private void rbOtherStatsPlayoff_Click(object sender, RoutedEventArgs e)
        {
            rbPlayerStatsPlayoff.IsChecked = true;
            rbMetricStatsPlayoff.IsChecked = true;
            rbShootingStatsPlayoff.IsChecked = true;
            updatePlayerAndMetricStats();
            updatePlayerShootingStats();
        }

        private void chkMatchupHideInjured_Click(object sender, RoutedEventArgs e)
        {
            chkHTHHideInjured.IsChecked = chkMatchupHideInjured.IsChecked;
            SQLiteIO.SetSetting("HTHHideInjured", chkHTHHideInjured.IsChecked == true);

            cmbOppTeam_SelectionChanged(null, null);
        }

        private void cmbGraphStat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            updateGraph();
        }

        private void updateGraph()
        {
            if (cmbGraphStat.SelectedIndex == -1 || cmbTeam.SelectedIndex == -1 || cmbGraphInterval.SelectedIndex == -1)
            {
                clearGraph();
                return;
            }
            var intervalItem = cmbGraphInterval.SelectedItem.ToString();
            var yearlyRows = _dtYea.Rows.Cast<DataRow>().Where(dr => dr[0].ToString().StartsWith("Season")).ToList();
            var monthlyStats = MainWindow.SplitTeamStats[_curts.ID].Where(pair => pair.Key.StartsWith("M ")).ToList();
            var orderedBSEList = _bseList.OrderBy(bse => bse.BS.GameDate).ToList();
            Intervals interval;
            int count;
            switch (intervalItem)
            {
                case "Every Game":
                    interval = Intervals.EveryGame;
                    count = orderedBSEList.Count;
                    break;
                case "Monthly":
                    interval = Intervals.Monthly;
                    count = monthlyStats.Count;
                    break;
                case "Yearly":
                    interval = Intervals.Yearly;
                    count = yearlyRows.Count;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (count < 2)
            {
                clearGraph();
                return;
            }

            var propToGet = cmbGraphStat.SelectedItem.ToString();
            propToGet = propToGet.Replace('3', 'T');
            propToGet = propToGet.Replace('%', 'p');
            propToGet = propToGet.Replace("TO", "TOS");

            double sum = 0;
            double games = 0;

            chart.Primitives.Clear();
            var cp = new ChartPrimitive { Label = cmbGraphStat.SelectedItem.ToString(), ShowInLegend = false };

            switch (interval)
            {
                case Intervals.EveryGame:
                    for (var i = 0; i < count; i++)
                    {
                        var bse = orderedBSEList[i];
                        bse.BS.PrepareForDisplay(_tst, _curts.ID);

                        var isTeamAway = bse.BS.Team1ID == _curts.ID;
                        var propToGetFinal = propToGet;
                        if (propToGet == "PF")
                        {
                            propToGetFinal = isTeamAway ? "PTS1" : "PTS2";
                        }
                        else if (propToGet == "PA")
                        {
                            propToGetFinal = isTeamAway ? "PTS2" : "PTS1";
                        }
                        else
                        {
                            propToGetFinal += (isTeamAway ? 1 : 2).ToString();
                        }
                        var value = bse.BS.GetValue<TeamBoxScore, double>(propToGetFinal);
                        if (double.IsNaN(value))
                        {
                            continue;
                        }
                        if (propToGet.Contains("p"))
                        {
                            value = Convert.ToDouble(Convert.ToInt32(value * 1000)) / 1000;
                        }
                        cp.AddPoint(i + 1, value);
                        games++;
                        sum += value;
                    }
                    break;
                case Intervals.Monthly:
                    monthlyStats = monthlyStats.OrderBy(ms => ms.Key).ToList();
                    if (TeamStatsHelper.TotalsToPerGame.ContainsKey(propToGet))
                    {
                        propToGet = TeamStatsHelper.TotalsToPerGame[propToGet];
                    }
                    for (var i = 0; i < count; i++)
                    {
                        var ts = monthlyStats[i].Value;
                        ts.CalcMetrics(new TeamStats());
                        var tsr = new TeamStatsRow(ts);
                        var value = tsr.GetValue<double>(propToGet);
                        if (double.IsNaN(value))
                        {
                            continue;
                        }
                        if (propToGet.Contains("p"))
                        {
                            value = Convert.ToDouble(Convert.ToInt32(value * 1000)) / 1000;
                        }
                        cp.AddPoint(i + 1, value);
                        games++;
                        sum += value;
                    }
                    break;
                case Intervals.Yearly:
                    if (TeamStatsHelper.TotalsToPerGame.ContainsKey(propToGet))
                    {
                        propToGet = TeamStatsHelper.TotalsToPerGame[propToGet];
                    }
                    for (var i = 0; i < count; i++)
                    {
                        var ts = new TeamStats();
                        createTeamStatsFromDataRow(ref ts, _dtYea.Rows.Cast<DataRow>().ToList()[i]);
                        ts.CalcMetrics(new TeamStats());
                        var tsr = new TeamStatsRow(ts);
                        var value = tsr.GetValue<TeamStatsRow, double>(propToGet);
                        if (double.IsNaN(value))
                        {
                            continue;
                        }
                        if (propToGet.Contains("p"))
                        {
                            value = Convert.ToDouble(Convert.ToInt32(value * 1000)) / 1000;
                        }
                        cp.AddPoint(i + 1, value);
                        games++;
                        sum += value;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (cp.Points.Count > 0)
            {
                chart.Primitives.Add(cp);
            }
            if (chart.Primitives.Count > 0 && chart.Primitives.Sum(p => p.Points.Count) > 1)
            {
                //var average = sum / games;
                if (TeamStatsHelper.TotalsToPerGame.ContainsKey(propToGet))
                {
                    propToGet = TeamStatsHelper.TotalsToPerGame[propToGet];
                }
                double average;
                switch (interval)
                {
                    case Intervals.EveryGame:
                    case Intervals.Monthly:
                        average = _curTSR.GetValue<double>(propToGet);
                        break;
                    case Intervals.Yearly:
                        var ts = new TeamStats();
                        createTeamStatsFromDataRow(ref ts, _dtYea.Rows.Cast<DataRow>().ToList().Last());
                        ts.CalcMetrics(new TeamStats());
                        var tsr = new TeamStatsRow(ts);
                        average = tsr.GetValue<double>(propToGet);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var cpavg = new ChartPrimitive();
                for (var i = 0; i < count; i++)
                {
                    cpavg.AddPoint(i + 1, average);
                }
                cpavg.Color = Color.FromRgb(0, 0, 100);
                cpavg.Dashed = true;
                cpavg.ShowInLegend = false;
                var cp2 = new ChartPrimitive();
                cp2.AddPoint(chart.Primitives.First().Points.First().X, 0);
                cp2.AddPoint(chart.Primitives.Last().Points.Last().X, 1);

                chart.Primitives.Add(cpavg);
                chart.RedrawPlotLines();
                chart.Primitives.Add(cp2);
            }
            else
            {
                chart.RedrawPlotLines();
                var cp2 = new ChartPrimitive();
                cp2.AddPoint(1, 0);
                cp2.AddPoint(2, 1);
                chart.Primitives.Add(cp2);
            }
            chart.ResetPanAndZoom();
        }

        private void clearGraph()
        {
            chart.Primitives.Clear();
            chart.RedrawPlotLines();
            chart.ResetPanAndZoom();
        }

        /// <summary>Populates the graph stat combo.</summary>
        private void populateGraphStatCombo()
        {
            var stats = new List<string>
                {
                    "GmSc",
                    "PF",
                    "PA",
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

            cmbGraphStat.ItemsSource = stats;

            var intervals = new List<string> { "Every Game", "Monthly", "Yearly" };

            cmbGraphInterval.ItemsSource = intervals;
        }

        /// <summary>Handles the Click event of the btnPrevStat control. Switches to the previous graph stat.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnPrevStat_Click(object sender, RoutedEventArgs e)
        {
            if (cmbGraphStat.SelectedIndex == 0)
            {
                cmbGraphStat.SelectedIndex = cmbGraphStat.Items.Count - 1;
            }
            else
            {
                cmbGraphStat.SelectedIndex--;
            }
        }

        /// <summary>Handles the Click event of the btnNextStat control. Switches to the next graph stat.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">
        ///     The <see cref="RoutedEventArgs" /> instance containing the event data.
        /// </param>
        private void btnNextStat_Click(object sender, RoutedEventArgs e)
        {
            if (cmbGraphStat.SelectedIndex == cmbGraphStat.Items.Count - 1)
            {
                cmbGraphStat.SelectedIndex = 0;
            }
            else
            {
                cmbGraphStat.SelectedIndex++;
            }
        }

        private void cmbGraphInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbGraphStat_SelectionChanged(null, null);
        }

        #region Nested type: Intervals

        private enum Intervals
        {
            EveryGame,
            Monthly,
            Yearly
        };

        #endregion
    }
}