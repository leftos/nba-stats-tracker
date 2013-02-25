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

#endregion

#region Using Directives

using System.Collections.Generic;

#endregion

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     A list of constant pseudonyms for specific entries in the players' stats arrays.
    /// </summary>
    public static class PAbbr
    {
        public const int GP = 0,
                         GS = 1,
                         MINS = 2,
                         PTS = 3,
                         DREB = 4,
                         OREB = 5,
                         AST = 6,
                         STL = 7,
                         BLK = 8,
                         TOS = 9,
                         FOUL = 10,
                         FGM = 11,
                         FGA = 12,
                         TPM = 13,
                         TPA = 14,
                         FTM = 15,
                         FTA = 16;

        /// <summary>
        ///     Only to be used with CareerHighs
        /// </summary>
        public const int REB = 17; // REB used only for CareerHighs

        public const int MPG = 0,
                         PPG = 1,
                         DRPG = 2,
                         ORPG = 3,
                         APG = 4,
                         SPG = 5,
                         BPG = 6,
                         TPG = 7,
                         FPG = 8,
                         FGp = 9,
                         FGeff = 10,
                         TPp = 11,
                         TPeff = 12,
                         FTp = 13,
                         FTeff = 14,
                         RPG = 15;

        public static readonly Dictionary<int, string> Totals = new Dictionary<int, string>
                                                                {
                                                                    {0, "GP"},
                                                                    {1, "GS"},
                                                                    {2, "MINS"},
                                                                    {3, "PTS"},
                                                                    {4, "DREB"},
                                                                    {5, "OREB"},
                                                                    {6, "AST"},
                                                                    {7, "STL"},
                                                                    {8, "BLK"},
                                                                    {9, "TOS"},
                                                                    {10, "FOUL"},
                                                                    {11, "FGM"},
                                                                    {12, "FGA"},
                                                                    {13, "3PM"},
                                                                    {14, "3PA"},
                                                                    {15, "FTM"},
                                                                    {16, "FTA"}
                                                                };

        public static readonly Dictionary<int, string> PerGame = new Dictionary<int, string>
                                                                 {
                                                                     {0, "MPG"},
                                                                     {1, "PPG"},
                                                                     {2, "DRPG"},
                                                                     {3, "ORPG"},
                                                                     {4, "APG"},
                                                                     {5, "SPG"},
                                                                     {6, "BPG"},
                                                                     {7, "TPG"},
                                                                     {8, "FPG"},
                                                                     {9, "FG%"},
                                                                     {10, "FGeff"},
                                                                     {11, "3P%"},
                                                                     {12, "3Peff"},
                                                                     {13, "FT%"},
                                                                     {14, "FTeff"},
                                                                     {15, "RPG"}
                                                                 };

        public static readonly List<string> ExtendedTotals = new List<string>
                                                             {
                                                                 "GP",
                                                                 "GS",
                                                                 "PTS",
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
                                                               "GmSc",
                                                               "GmScE",
                                                               "AST%",
                                                               "EFG%",
                                                               "STL%",
                                                               "TO%",
                                                               "TS%",
                                                               "USG%",
                                                               "EFF",
                                                               "aPER",
                                                               "BLK%",
                                                               "DREB%",
                                                               "OREB%",
                                                               "REB%",
                                                               "PPR",
                                                               "PTSR",
                                                               "REBR",
                                                               "OREBR",
                                                               "ASTR",
                                                               "BLKR",
                                                               "STLR",
                                                               "TOR",
                                                               "FTR",
                                                               "FTAR",
                                                               "PER"
                                                           };

        public static readonly Dictionary<string, double> MetricsDict = new Dictionary<string, double>(MetricsNames.Count);
    }

    // Unlike TeamStats which was designed before REDitor implemented such stats,
    // PlayerStats were made according to REDitor's standards, to make life 
    // easier when importing/exporting from REDitor's CSV
}