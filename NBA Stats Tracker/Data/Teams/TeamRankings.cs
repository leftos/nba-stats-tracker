using System.Collections.Generic;

namespace NBA_Stats_Tracker.Data.Teams
{
    /// <summary>
    ///     Used to determine the team ranking for each stat.
    /// </summary>
    public class TeamRankings
    {
        public Dictionary<int, Dictionary<string, int>> rankingsMetrics;
        public int[][] rankingsPerGame;
        public int[][] rankingsTotal;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamRankings" /> class.
        /// </summary>
        /// <param name="tst">The team stats dictionary containing all team stats.</param>
        public TeamRankings(Dictionary<int, TeamStats> tst, bool playoffs = false)
        {
            rankingsPerGame = new int[tst.Count][];
            for (int i = 0; i < tst.Count; i++)
            {
                rankingsPerGame[i] = new int[tst[i].averages.Length];
            }
            for (int j = 0; j < tst[0].averages.Length; j++)
            {
                var averages = new Dictionary<int, float>();
                for (int i = 0; i < tst.Count; i++)
                {
                    averages.Add(i, playoffs ? tst[i].pl_averages[j] : tst[i].averages[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankingsPerGame[kvp.Key][j] = k;
                    k++;
                }
            }

            rankingsTotal = new int[tst.Count][];
            for (int i = 0; i < tst.Count; i++)
            {
                rankingsTotal[i] = new int[tst[i].stats.Length];
            }
            for (int j = 0; j < tst[0].stats.Length; j++)
            {
                var totals = new Dictionary<int, float>();
                for (int i = 0; i < tst.Count; i++)
                {
                    totals.Add(i, playoffs ? tst[i].pl_stats[j] : tst[i].stats[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(totals);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankingsTotal[kvp.Key][j] = k;
                    k++;
                }
            }

            rankingsMetrics = new Dictionary<int, Dictionary<string, int>>();
            var metricsNames = new List<string>(tst[0].metrics.Keys);
            for (int i = 0; i < tst.Count; i++)
            {
                rankingsMetrics[i] = new Dictionary<string, int>();
            }
            foreach (string metricName in metricsNames)
            {
                var metricStats = new Dictionary<int, double>();
                for (int i = 0; i < tst.Count; i++)
                {
                    metricStats.Add(i, playoffs ? tst[i].pl_metrics[metricName] : tst[i].metrics[metricName]);
                }

                var tempList = new List<KeyValuePair<int, double>>(metricStats);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankingsMetrics[kvp.Key][metricName] = k;
                    k++;
                }
            }
        }
    }
}