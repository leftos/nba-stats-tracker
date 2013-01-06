using System.Collections.Generic;
using System.Linq;
using NBA_Stats_Tracker.Windows;

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    /// Used to determine the player ranking for each stat.
    /// </summary>
    public class PlayerRankings
    {
        public int avgcount = (new PlayerStats(new Player(-1, "", "", "", Position.None, Position.None))).averages.Length;

        public Dictionary<int, int[]> list = new Dictionary<int, int[]>();
        public Dictionary<int, int[]> rankings = new Dictionary<int, int[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerRankings" /> class, and calculates the rankings.
        /// </summary>
        /// <param name="pst">The PlayerStats dictionary, containing all player information.</param>
        /// <param name="playoffs">if set to <c>true</c>, the rankings will take only playoff performances into account.</param>
        public PlayerRankings(Dictionary<int, PlayerStats> pst, bool playoffs = false)
        {
            var validPlayers = pst.Where(ps => ps.Value.stats[p.GP] > 0);
            foreach (var kvp in validPlayers)
            {
                rankings.Add(kvp.Key, new int[avgcount]);
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
                    rankings[kvp.Key][j] = k;
                    k++;
                }
            }
            int plCount = pst.Count;
            foreach (var kvp in pst.Where(ps => ps.Value.stats[p.GP] == 0))
            {
                rankings.Add(kvp.Key, new int[avgcount]);
                for (int i = 0; i < avgcount; i++)
                {
                    rankings[kvp.Key][i] = plCount;
                }
            }

            /*
            list = new Dictionary<int, int[]>();
            for (int i = 0; i<pst.Count; i++)
                list.Add(pst[i].ID, rankings[i]);
            */
            list = rankings;
        }

        public static PlayerRankings CalculateActiveRankings()
        {
            var cumRankingsActive = new PlayerRankings(MainWindow.pst.Where(ps => ps.Value.isActive).ToDictionary(r => r.Key, r => r.Value));
            return cumRankingsActive;
        }

        public PlayerRankings CalculateLeadersRankings(out Dictionary<int, PlayerStats> pstLeaders)
        {
            var pstActive = MainWindow.pst.Where(ps => ps.Value.isActive).ToDictionary(ps => ps.Key, ps => ps.Value);
            var listOfKeys = pstActive.Keys.ToList();
            foreach (var key in listOfKeys)
            {
                pstActive[key] = LeagueOverviewWindow.ConvertToLeagueLeader(pstActive[key], MainWindow.tst);
            }
            var cumRankingsActive = new PlayerRankings(pstActive);
            pstLeaders = pstActive;
            return cumRankingsActive;
        }
    }
}