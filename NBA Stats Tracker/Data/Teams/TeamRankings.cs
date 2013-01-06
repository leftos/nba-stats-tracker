using System.Collections.Generic;

namespace NBA_Stats_Tracker.Data.Teams
{
    /// <summary>
    /// Used to determine the team ranking for each stat.
    /// </summary>
    public class TeamRankings
    {
        public int[][] rankings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRankings" /> class.
        /// </summary>
        /// <param name="_tst">The team stats dictionary containing all team stats.</param>
        public TeamRankings(Dictionary<int, TeamStats> _tst)
        {
            rankings = new int[_tst.Count][];
            for (int i = 0; i < _tst.Count; i++)
            {
                rankings[i] = new int[_tst[i].averages.Length];
            }
            for (int j = 0; j < _tst[0].averages.Length; j++)
            {
                var averages = new Dictionary<int, float>();
                for (int i = 0; i < _tst.Count; i++)
                {
                    averages.Add(i, _tst[i].averages[j]);
                }

                var tempList = new List<KeyValuePair<int, float>>(averages);
                tempList.Sort((x, y) => x.Value.CompareTo(y.Value));
                tempList.Reverse();

                int k = 1;
                foreach (var kvp in tempList)
                {
                    rankings[kvp.Key][j] = k;
                    k++;
                }
            }
        }
    }
}