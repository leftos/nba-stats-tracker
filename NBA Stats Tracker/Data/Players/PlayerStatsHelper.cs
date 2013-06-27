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

namespace NBA_Stats_Tracker.Data.Players
{
    #region Using Directives

    using System.Collections.Generic;
    using System.Linq;

    #endregion

    /// <summary>Lists and dictionaries to help access and create lists of player stats.</summary>
    public static class PlayerStatsHelper
    {
        public static readonly Dictionary<int, string> Totals = new Dictionary<int, string>
            {
                { 0, "GP" },
                { 1, "GS" },
                { 2, "MINS" },
                { 3, "PTS" },
                { 4, "DREB" },
                { 5, "OREB" },
                { 6, "AST" },
                { 7, "STL" },
                { 8, "BLK" },
                { 9, "TOS" },
                { 10, "FOUL" },
                { 11, "FGM" },
                { 12, "FGA" },
                { 13, "3PM" },
                { 14, "3PA" },
                { 15, "FTM" },
                { 16, "FTA" }
            };

        public static readonly Dictionary<int, string> CHTotals = new Dictionary<int, string>
            {
                { 0, "GP" },
                { 1, "GS" },
                { 2, "MINS" },
                { 3, "PTS" },
                { 4, "DREB" },
                { 5, "OREB" },
                { 6, "AST" },
                { 7, "STL" },
                { 8, "BLK" },
                { 9, "TOS" },
                { 10, "FOUL" },
                { 11, "FGM" },
                { 12, "FGA" },
                { 13, "3PM" },
                { 14, "3PA" },
                { 15, "FTM" },
                { 16, "FTA" },
                { 17, "REB" }
            };

        public static readonly Dictionary<int, string> PerGame = new Dictionary<int, string>
            {
                { 0, "MPG" },
                { 1, "PPG" },
                { 2, "DRPG" },
                { 3, "ORPG" },
                { 4, "APG" },
                { 5, "SPG" },
                { 6, "BPG" },
                { 7, "TPG" },
                { 8, "FPG" },
                { 9, "FG%" },
                { 10, "FGeff" },
                { 11, "3P%" },
                { 12, "3Peff" },
                { 13, "FT%" },
                { 14, "FTeff" },
                { 15, "RPG" }
            };

        public static readonly Dictionary<string, string> TotalsToPerGame = new Dictionary<string, string>
            {
                { "PTS", "PPG" },
                { "FGM", "FGMPG" },
                { "FGA", "FGAPG" },
                { "TPM", "TPMPG" },
                { "TPA", "TPAPG" },
                { "FTM", "FTMPG" },
                { "FTA", "FTAPG" },
                { "REB", "RPG" },
                { "OREB", "ORPG" },
                { "DREB", "DRPG" },
                { "STL", "SPG" },
                { "BLK", "BPG" },
                { "TOS", "TPG" },
                { "AST", "APG" },
                { "FOUL", "FPG" },
                { "MINS", "MPG" }
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

        public static readonly List<string> MetricsNames = new List<string> {
                "PER",
                "EFF",
                "ORTG",
                "DRTG",
                "RTGd",
                "GmSc",
                "GmScE",
                "TS%",
                "EFG%",
                "Floor%",
                "AST%",
                "PPR",
                "STL%",
                "TO%",
                "USG%",
                "BLK%",
                "DREB%",
                "OREB%",
                "REB%",
                "PTSR",
                "REBR",
                "OREBR",
                "ASTR",
                "BLKR",
                "STLR",
                "TOR",
                "FTR",
                "FTAR",
                "aPER"
            };

        public static readonly Dictionary<string, double> MetricsDict =
            MetricsNames.Select(name => new KeyValuePair<string, double>(name, double.NaN))
                        .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    // Unlike TeamStats which was designed before REDitor implemented such stats,
    // PlayerStats were made according to REDitor's standards, to make life 
    // easier when importing/exporting from REDitor's CSV
}