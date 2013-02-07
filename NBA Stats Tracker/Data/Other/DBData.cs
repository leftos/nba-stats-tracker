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