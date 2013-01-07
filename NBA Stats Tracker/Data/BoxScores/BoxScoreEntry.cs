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
using NBA_Stats_Tracker.Data.Players;
using NBA_Stats_Tracker.Data.Teams;

#endregion

namespace NBA_Stats_Tracker.Data.BoxScores
{
    /// <summary>
    ///     A container for a TeamBoxScore and a list of PlayerBoxScores, along with other helpful information.
    /// </summary>
    public class BoxScoreEntry
    {
        public DateTime date;
        public bool mustUpdate;
        public List<PlayerBoxScore> pbsList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreEntry" /> class.
        /// </summary>
        /// <param name="bs">The TeamBoxScore to initialize with.</param>
        public BoxScoreEntry(TeamBoxScore bs)
        {
            this.bs = bs;
            date = DateTime.Now;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreEntry" /> class.
        /// </summary>
        /// <param name="bs">The TeamBoxScore to initialize with.</param>
        /// <param name="date">The date of the game.</param>
        /// <param name="pbsList">The PlayerBoxScore list.</param>
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