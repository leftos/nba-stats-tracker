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

    using System;

    #endregion

    public class PlayerHighsRow
    {
        public PlayerHighsRow(int playerID, string type, UInt16[] highs)
        {
            PlayerID = playerID;
            Type = type;
            MINS = highs[PAbbrT.MINS];
            PTS = highs[PAbbrT.PTS];
            FGM = highs[PAbbrT.FGM];
            FGA = highs[PAbbrT.FGA];
            TPM = highs[PAbbrT.TPM];
            TPA = highs[PAbbrT.TPA];
            FTM = highs[PAbbrT.FTM];
            FTA = highs[PAbbrT.FTA];
            REB = highs[PAbbrT.REB];
            OREB = highs[PAbbrT.OREB];
            DREB = highs[PAbbrT.DREB];
            STL = highs[PAbbrT.STL];
            TOS = highs[PAbbrT.TOS];
            BLK = highs[PAbbrT.BLK];
            AST = highs[PAbbrT.AST];
            FOUL = highs[PAbbrT.FOUL];
        }

        public int PlayerID { get; set; }
        public string Type { get; set; }
        public UInt16 MINS { get; set; }
        public UInt16 PTS { get; set; }
        public UInt16 FGM { get; set; }
        public UInt16 FGA { get; set; }
        public UInt16 TPM { get; set; }
        public UInt16 TPA { get; set; }
        public UInt16 FTM { get; set; }
        public UInt16 FTA { get; set; }
        public UInt16 REB { get; set; }
        public UInt16 OREB { get; set; }
        public UInt16 DREB { get; set; }
        public UInt16 STL { get; set; }
        public UInt16 TOS { get; set; }
        public UInt16 BLK { get; set; }
        public UInt16 AST { get; set; }
        public UInt16 FOUL { get; set; }
    }
}