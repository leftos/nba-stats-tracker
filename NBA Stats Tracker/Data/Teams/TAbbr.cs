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
    ///     A list of constant pseudonyms for specific entries in the teams' stats arrays.
    /// </summary>
    public static class TAbbr
    {
        public const int MINS = 0,
                         PF = 1,
                         PA = 2,
                         FGM = 4,
                         FGA = 5,
                         TPM = 6,
                         TPA = 7,
                         FTM = 8,
                         FTA = 9,
                         OREB = 10,
                         DREB = 11,
                         STL = 12,
                         TOS = 13,
                         BLK = 14,
                         AST = 15,
                         FOUL = 16;

        public const int PPG = 0,
                         PAPG = 1,
                         FGp = 2,
                         FGeff = 3,
                         TPp = 4,
                         TPeff = 5,
                         FTp = 6,
                         FTeff = 7,
                         RPG = 8,
                         ORPG = 9,
                         DRPG = 10,
                         SPG = 11,
                         BPG = 12,
                         TPG = 13,
                         APG = 14,
                         FPG = 15,
                         Wp = 16,
                         Weff = 17,
                         PD = 18,
                         MPG = 19;

        public static readonly Dictionary<int, string> Totals = new Dictionary<int, string>
            {
                {0, "MINS"},
                {1, "PF"},
                {2, "PA"},
                {4, "FGM"},
                {5, "FGA"},
                {6, "3PM"},
                {7, "3PA"},
                {8, "FTM"},
                {9, "FTA"},
                {10, "OREB"},
                {11, "DREB"},
                {12, "STL"},
                {13, "TO"},
                {14, "BLK"},
                {15, "AST"},
                {16, "FOUL"}
            };

        public static readonly Dictionary<int, string> PerGame = new Dictionary<int, string>
            {
                {0, "PPG"},
                {1, "PAPG"},
                {2, "FG%"},
                {3, "FGeff"},
                {4, "3P%"},
                {5, "3Peff"},
                {6, "FT%"},
                {7, "FTeff"},
                {8, "RPG"},
                {9, "ORPG"},
                {10, "DRPG"},
                {11, "SPG"},
                {12, "BPG"},
                {13, "TPG"},
                {14, "APG"},
                {15, "FPG"},
                {16, "WP"},
                {17, "Weff"},
                {18, "PD"},
                {19, "MPG"}
            };

        public static readonly List<string> ExtendedTotals = new List<string>
            {
                "PF",
                "PA",
                "FGM",
                "FGA",
                "3PM",
                "3PA",
                "FTM",
                "FTA",
                "REB",
                "OREB",
                "DREB",
                "AST",
                "STL",
                "BLK",
                "TO",
                "FOUL",
                "MINS"
            };

        public static readonly List<string> ExtendedPerGame = new List<string>
            {
                "PPG",
                "PAPG",
                "FGMPG",
                "FGAPG",
                "FG%",
                "FGeff",
                "3PMPG",
                "3PAPG",
                "3P%",
                "3Peff",
                "FTMPG",
                "FTAPG",
                "FT%",
                "FTeff",
                "RPG",
                "ORPG",
                "DRPG",
                "APG",
                "SPG",
                "BPG",
                "TPG",
                "FPG",
                "MPG"
            };

        public static readonly List<string> MetricsNames = new List<string>
            {
                "ORTG",
                "DRTG",
                "EFFd",
                "PW%",
                "PythW",
                "PythL",
                "TS%",
                "EFG%",
                "3PR",
                "DREB%",
                "OREB%",
                "AST%",
                "TOR",
                "FTR",
                "Poss",
                "PossPG",
                "Pace"
            };
    }
}