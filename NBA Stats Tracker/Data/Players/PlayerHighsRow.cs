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

namespace NBA_Stats_Tracker.Data.Players
{
    public class PlayerHighsRow
    {
        public PlayerHighsRow(int playerID, string type, UInt16[] highs)
        {
            PlayerID = playerID;
            Type = type;
            MINS = highs[PAbbr.MINS];
            PTS = highs[PAbbr.PTS];
            FGM = highs[PAbbr.FGM];
            FGA = highs[PAbbr.FGA];
            TPM = highs[PAbbr.TPM];
            TPA = highs[PAbbr.TPA];
            FTM = highs[PAbbr.FTM];
            FTA = highs[PAbbr.FTA];
            REB = highs[PAbbr.REB];
            OREB = highs[PAbbr.OREB];
            DREB = highs[PAbbr.DREB];
            STL = highs[PAbbr.STL];
            TOS = highs[PAbbr.TOS];
            BLK = highs[PAbbr.BLK];
            AST = highs[PAbbr.AST];
            FOUL = highs[PAbbr.FOUL];
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