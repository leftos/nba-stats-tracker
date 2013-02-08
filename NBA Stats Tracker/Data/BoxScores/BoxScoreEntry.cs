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
        public DateTime Date;
        public bool MustUpdate;
        public List<PlayerBoxScore> PBSList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreEntry" /> class.
        /// </summary>
        /// <param name="bs">The TeamBoxScore to initialize with.</param>
        public BoxScoreEntry(TeamBoxScore bs)
        {
            BS = bs;
            Date = DateTime.Now;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreEntry" /> class.
        /// </summary>
        /// <param name="bs">The TeamBoxScore to initialize with.</param>
        /// <param name="date">The date of the game.</param>
        /// <param name="pbsList">The PlayerBoxScore list.</param>
        public BoxScoreEntry(TeamBoxScore bs, DateTime date, List<PlayerBoxScore> pbsList)
        {
            BS = bs;
            Date = date;
            PBSList = pbsList;
        }

        public TeamBoxScore BS { get; set; }
        public string Team1Display { get; set; }
        public string Team2Display { get; set; }
    }
}