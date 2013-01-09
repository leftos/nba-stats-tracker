#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

#endregion

using System.Collections.Generic;

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     A list of constant pseudonyms for specific entries in the players' stats arrays.
    /// </summary>
    public static class p
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
        /// Only to be used with CareerHighs
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

        public static Dictionary<int, string> totals = new Dictionary<int, string>
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

        public static Dictionary<int, string> averages = new Dictionary<int, string>
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
    }

    // Unlike TeamStats which was designed before REditor implemented such stats,
    // PlayerStats were made according to REditor's standards, to make life 
    // easier when importing/exporting from REditor's CSV
}