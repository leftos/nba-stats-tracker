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

namespace NBA_Stats_Tracker.Data.BoxScores
{
    #region Using Directives

    using System;
    using System.Collections.Generic;

    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.Teams;

    #endregion

    /// <summary>A container for a TeamBoxScore and a list of PlayerBoxScores, along with other helpful information.</summary>
    [Serializable]
    public class BoxScoreEntry
    {
        public bool MustUpdate { get; set; }
        public List<PlayerBoxScore> PBSList { get; set; }
        public TeamBoxScore BS { get; set; }
        public string Team1Display { get; set; }
        public string Team2Display { get; set; }
        public List<PlayByPlayEntry> PBPEList { get; set; }
        public List<PlayByPlayEntry> FilteredPBPEList { get; set; } 

        public BoxScoreEntry()
        {
            PBSList = new List<PlayerBoxScore>();
            PBPEList = new List<PlayByPlayEntry>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreEntry" /> class.
        /// </summary>
        /// <param name="bs">The TeamBoxScore to initialize with.</param>
        public BoxScoreEntry(TeamBoxScore bs) : this()
        {
            BS = bs;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BoxScoreEntry" /> class.
        /// </summary>
        /// <param name="bs">The TeamBoxScore to initialize with.</param>
        /// <param name="pbsList">The PlayerBoxScore list.</param>
        /// <param name="pbpeList"></param>
        public BoxScoreEntry(TeamBoxScore bs, List<PlayerBoxScore> pbsList, List<PlayByPlayEntry> pbpeList)
            : this(bs)
        {
            PBSList = pbsList;
            PBPEList = pbpeList;
        }
    }
}