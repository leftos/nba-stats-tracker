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

using System;
using System.Collections.Generic;
using System.Windows;

#endregion

namespace NBA_Stats_Tracker.Data
{
    public class BoxScoreEntry
    {
        public DateTime date;
        public bool mustUpdate;
        public List<PlayerBoxScore> pbsList;

        public BoxScoreEntry(TeamBoxScore bs)
        {
            this.bs = bs;
            date = DateTime.Now;
        }

        public BoxScoreEntry(TeamBoxScore bs, DateTime date, List<PlayerBoxScore> pbsList)
        {
            this.bs = bs;
            this.date = date;
            this.pbsList = pbsList;
        }

        public TeamBoxScore bs { get; set; }
        public string Team1Display { get; set; }
        public string Team2Display { get; set; }
    }
}