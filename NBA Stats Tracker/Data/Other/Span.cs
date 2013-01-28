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

namespace NBA_Stats_Tracker.Data.Other
{
    /// <summary>
    ///     Used to differentiate between available time-spans for stats.
    /// </summary>
    public enum Span
    {
        Season,
        Playoffs,
        SeasonAndPlayoffsToSeason,
        SeasonAndPlayoffs
    }
}