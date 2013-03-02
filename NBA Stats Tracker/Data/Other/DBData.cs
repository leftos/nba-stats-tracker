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
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Teams;

#endregion

namespace NBA_Stats_Tracker.Data.Other
{
    public class DBData
    {
        public List<BoxScoreEntry> BSHist;
        public Dictionary<int, string> DisplayNames;
        public Dictionary<int, PlayerStats> PST;
        public PlayerRankings PlayoffPlayerRankings;
        public TeamRankings PlayoffTeamRankings;
        public PlayerRankings SeasonPlayerRankings;
        public TeamRankings SeasonTeamRankings;
        public Dictionary<int, Dictionary<string, PlayerStats>> SplitPlayerStats;
        public Dictionary<int, Dictionary<string, TeamStats>> SplitTeamStats;
        public Dictionary<int, TeamStats> TST;
        public Dictionary<int, TeamStats> TSTOpp;

        public DBData(Dictionary<int, TeamStats> tst, Dictionary<int, TeamStats> tstOpp,
                      Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats, TeamRankings seasonTeamRankings,
                      TeamRankings playoffTeamRankings, Dictionary<int, PlayerStats> pst,
                      Dictionary<int, Dictionary<string, PlayerStats>> splitPlayerStats, PlayerRankings seasonPlayerRankings,
                      PlayerRankings playoffPlayerRankings, List<BoxScoreEntry> bsHist, Dictionary<int, string> displayNames)
        {
            BSHist = bsHist;
            DisplayNames = displayNames;
            PST = pst;
            SeasonPlayerRankings = seasonPlayerRankings;
            PlayoffPlayerRankings = playoffPlayerRankings;
            PlayoffTeamRankings = playoffTeamRankings;
            SplitPlayerStats = splitPlayerStats;
            SplitTeamStats = splitTeamStats;
            TST = tst;
            TSTOpp = tstOpp;
            SeasonTeamRankings = seasonTeamRankings;
        }
    }
}