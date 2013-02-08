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
    public struct DBData
    {
        public List<BoxScoreEntry> BSHist;
        public Dictionary<string, string> DisplayNames;
        public Dictionary<int, PlayerStats> PST;
        public PlayerRankings PlayerRankings;
        public PlayerRankings PlayoffPlayerRankings;
        public TeamRankings PlayoffTeamRankings;
        public Dictionary<int, Dictionary<string, PlayerStats>> SplitPlayerStats;
        public Dictionary<int, Dictionary<string, TeamStats>> SplitTeamStats;
        public Dictionary<int, TeamStats> TST;
        public Dictionary<int, TeamStats> TSTOpp;
        public SortedDictionary<string, int> TeamOrder;
        public TeamRankings TeamRankings;
    }
}