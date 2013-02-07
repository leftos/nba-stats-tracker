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

using System;

#endregion

namespace NBA_Stats_Tracker.Data.Other
{
    /// <summary>
    ///     Used to pass on all the required information for a specific timeframe.
    /// </summary>
    public class Timeframe
    {
        public Timeframe(int seasonNum)
        {
            IsBetween = false;
            SeasonNum = seasonNum;
        }

        public Timeframe(DateTime startDate, DateTime endDate)
        {
            IsBetween = true;
            StartDate = startDate;
            EndDate = endDate;
            SeasonNum = 1;
        }

        public bool IsBetween { get; set; }
        public int SeasonNum { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}