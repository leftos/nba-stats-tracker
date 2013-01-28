#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System.Collections.Generic;
using System.Linq;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     Used to determine the player ranking for each stat.
    /// </summary>
    public class PlayerRankings
    {
        public int avgcount = (new PlayerStats(new Player(-1, -1, "", "", Position.None, Position.None))).averages.Length;
        public Dictionary<int, Dictionary<string, int>> rankingsMetrics = new Dictionary<int, Dictionary<string, int>>();

        public Dictionary<int, int[]> rankingsPerGame = new Dictionary<int, int[]>();
        public Dictionary<int, int[]> rankingsTotal = new Dictionary<int, int[]>();

        public PlayerRankings()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerRankings" /> class, and calculates the rankingsPerGame.
        /// </summary>
        /// <param name="pst">The PlayerStats dictionary, containing all player information.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the rankingsPerGame will take only playoff performances into account.
        /// </param>
        public PlayerRankings(Dictionary<int, PlayerStats> pst, bool playoffs = false)
        {
            Dictionary<int, PlayerStats> validPlayers = pst.Where(ps => ps.Value.stats[p.GP] > 0).ToDictionary(a => a.Key, a => a.Value);

            var dummyPS = new PlayerStats();
            //int firstPlayerID = validPlayers.Keys.ToList()[0];
            int totalsCount = dummyPS.stats.Length;
            int metricsCount = dummyPS.metrics.Count;

            foreach (var kvp in validPlayers)
            {
                rankingsPerGame.Add(kvp.Key, new int[avgcount]);
                rankingsTotal.Add(kvp.Key, new int[totalsCount]);
                rankingsMetrics.Add(kvp.Key, new Dictionary<string, int>());
            }

            for (int j = 0; j < avgcount; j++)
            {
                Dictionary<int, float> averages;
                if (!playoffs)
                    averages = validPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.averages[j]);
                else
                    averages = validPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pl_averages[j]);

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                if (j != p.FPG && j != p.TPG)
                    tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankingsPerGame[kvp.Key][j] = k;
                    k++;
                }
            }
            int plCount = pst.Count;
            foreach (var kvp in pst.Where(ps => ps.Value.stats[p.GP] == 0))
            {
                rankingsPerGame.Add(kvp.Key, new int[avgcount]);
                for (int i = 0; i < avgcount; i++)
                {
                    rankingsPerGame[kvp.Key][i] = plCount;
                }
            }

            for (int j = 0; j < totalsCount; j++)
            {
                Dictionary<int, uint> totals;
                if (!playoffs)
                    totals = validPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.stats[j]);
                else
                    totals = validPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pl_stats[j]);

                var tempList = new List<KeyValuePair<int, uint>>(totals);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                if (j != p.FOUL && j != p.TOS)
                    tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankingsTotal[kvp.Key][j] = k;
                    k++;
                }
            }
            foreach (var kvp in pst.Where(ps => ps.Value.stats[p.GP] == 0))
            {
                rankingsTotal.Add(kvp.Key, new int[totalsCount]);
                for (int i = 0; i < totalsCount; i++)
                {
                    rankingsTotal[kvp.Key][i] = plCount;
                }
            }

            var badMetrics = new List<string> {"TO%", "TOR"};
            List<string> metricsNames = p.metricsNames;
            for (int j = 0; j < metricsCount; j++)
            {
                Dictionary<int, double> metrics;
                if (!playoffs)
                    metrics = validPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.metrics[metricsNames[j]]);
                else
                    metrics = validPlayers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.pl_metrics[metricsNames[j]]);

                var tempList = new List<KeyValuePair<int, double>>(metrics);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                if (!badMetrics.Contains(metricsNames[j]))
                    tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankingsMetrics[kvp.Key][metricsNames[j]] = k;
                    k++;
                }
            }
            foreach (var kvp in pst.Where(ps => ps.Value.stats[p.GP] == 0))
            {
                rankingsMetrics.Add(kvp.Key, new Dictionary<string, int>());
                for (int i = 0; i < metricsCount; i++)
                {
                    rankingsMetrics[kvp.Key][metricsNames[i]] = plCount;
                }
            }
        }

        public static PlayerRankings CalculateActiveRankings(bool playoffs = false)
        {
            var cumRankingsActive = new PlayerRankings(
                MainWindow.pst.Where(ps => ps.Value.isActive).ToDictionary(r => r.Key, r => r.Value), playoffs);
            return cumRankingsActive;
        }

        public static PlayerRankings CalculateLeadersRankings(out Dictionary<int, PlayerStats> pstLeaders, bool playoffs = false)
        {
            Dictionary<int, PlayerStats> pstActive = MainWindow.pst.Where(ps => ps.Value.isActive)
                                                               .ToDictionary(ps => ps.Key, ps => ps.Value);
            List<int> listOfKeys = pstActive.Keys.ToList();
            foreach (var key in listOfKeys)
            {
                pstActive[key] = MainWindow.LeadersPrefSetting == "NBA"
                                     ? pstActive[key].ConvertToLeagueLeader(MainWindow.tst, playoffs)
                                     : pstActive[key].ConvertToMyLeagueLeader(MainWindow.tst, playoffs);
            }
            var cumRankingsActive = new PlayerRankings(pstActive, playoffs);
            pstLeaders = pstActive;
            return cumRankingsActive;
        }
    }
}