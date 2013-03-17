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

using System.Collections.Generic;

#endregion

namespace NBA_Stats_Tracker.Data.Teams
{
    /// <summary>
    ///     Used to determine the team ranking for each stat.
    /// </summary>
    public class TeamRankings
    {
        public Dictionary<int, Dictionary<string, int>> RankingsMetrics;
        public int[][] RankingsPerGame;
        public int[][] RankingsTotal;

        public TeamRankings()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamRankings" /> class.
        /// </summary>
        /// <param name="tst">The team stats dictionary containing all team stats.</param>
        public TeamRankings(Dictionary<int, TeamStats> tst, bool playoffs = false)
        {
            RankingsPerGame = new int[tst.Count][];
            for (int i = 0; i < tst.Count; i++)
            {
                RankingsPerGame[i] = new int[tst[i].PerGame.Length];
            }
            for (int j = 0; j < (new TeamStats()).PerGame.Length; j++)
            {
                var averages = new Dictionary<int, float>();
                for (int i = 0; i < tst.Count; i++)
                {
                    averages.Add(i, playoffs ? tst[i].PlPerGame[j] : tst[i].PerGame[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                if (j != TAbbr.FPG && j != TAbbr.TPG && j != TAbbr.PAPG)
                {
                    tempList.Reverse();
                }

                int k = 1;
                foreach (var kvp in tempList)
                {
                    RankingsPerGame[kvp.Key][j] = k;
                    k++;
                }
            }

            RankingsTotal = new int[tst.Count][];
            for (int i = 0; i < tst.Count; i++)
            {
                RankingsTotal[i] = new int[tst[i].Totals.Length];
            }
            for (int j = 0; j < (new TeamStats()).Totals.Length; j++)
            {
                var totals = new Dictionary<int, float>();
                for (int i = 0; i < tst.Count; i++)
                {
                    totals.Add(i, playoffs ? tst[i].PlTotals[j] : tst[i].Totals[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(totals);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                if (j != TAbbr.FOUL && j != TAbbr.TOS && j != TAbbr.PA)
                {
                    tempList.Reverse();
                }

                int k = 1;
                foreach (var kvp in tempList)
                {
                    RankingsTotal[kvp.Key][j] = k;
                    k++;
                }
            }

            var badMetrics = new List<string> {"DRTG", "TOR", "PythL"};
            RankingsMetrics = new Dictionary<int, Dictionary<string, int>>();
            List<string> metricsNames = TAbbr.MetricsNames;
            for (int i = 0; i < tst.Count; i++)
            {
                RankingsMetrics[i] = new Dictionary<string, int>();
            }
            foreach (string metricName in metricsNames)
            {
                var metricStats = new Dictionary<int, double>();
                for (int i = 0; i < tst.Count; i++)
                {
                    metricStats.Add(i, playoffs ? tst[i].PlMetrics[metricName] : tst[i].Metrics[metricName]);
                }

                var tempList = new List<KeyValuePair<int, double>>(metricStats);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                if (!badMetrics.Contains(metricName))
                {
                    tempList.Reverse();
                }

                int k = 1;
                foreach (var kvp in tempList)
                {
                    RankingsMetrics[kvp.Key][metricName] = k;
                    k++;
                }
            }
        }
    }
}