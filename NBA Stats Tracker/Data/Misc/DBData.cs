using System.Collections.Generic;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Teams;

namespace NBA_Stats_Tracker.Data.Misc
{
    public struct DBData
    {
        public Dictionary<int, TeamStats> tst;
        public Dictionary<int, TeamStats> tstopp;
        public SortedDictionary<string, int> TeamOrder;
        public Dictionary<int, PlayerStats> pst;
        public Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats;
        public Dictionary<int, Dictionary<string, PlayerStats>> splitPlayerStats;
        public List<BoxScoreEntry> bshist;
        public TeamRankings teamRankings;
        public PlayerRankings playerRankings;
        public TeamRankings playoffTeamRankings;
        public PlayerRankings playoffPlayerRankings;
        public Dictionary<string, string> DisplayNames;
    }
}