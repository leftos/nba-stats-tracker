#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;

#endregion

namespace NBA_Stats_Tracker.Data
{
    public class BoxScore
    {
        public UInt16 AST1;
        public UInt16 AST2;
        public UInt16 BLK1;
        public UInt16 BLK2;
        public UInt16 FGA1;
        public UInt16 FGA2;
        public UInt16 FGM1;
        public UInt16 FGM2;
        public UInt16 FTA1;
        public UInt16 FTA2;
        public UInt16 FTM1;
        public UInt16 FTM2;
        public UInt16 MINS1;
        public UInt16 MINS2;
        public UInt16 OFF1;
        public UInt16 OFF2;
        public UInt16 PF1;
        public UInt16 PF2;
        public UInt16 PTS1;
        public UInt16 PTS2;
        public UInt16 REB1;
        public UInt16 REB2;
        public UInt16 STL1;
        public UInt16 STL2;
        public int SeasonNum;
        public UInt16 TO1;
        public UInt16 TO2;
        public UInt16 TPA1;
        public UInt16 TPA2;
        public UInt16 TPM1;
        public UInt16 TPM2;
        public string Team1;
        public string Team2;
        public int bshistid = -1;
        public bool doNotUpdate;
        public bool done;
        public DateTime gamedate;
        public int id = -1;
        public bool isPlayoff;
    }

    public class BoxScoreEntry
    {
        public BoxScore bs;
        public DateTime date;
        public bool mustUpdate;
        public List<PlayerBoxScore> pbsList;

        public BoxScoreEntry(BoxScore bs)
        {
            this.bs = bs;
            date = DateTime.Now;
        }

        public BoxScoreEntry(BoxScore bs, DateTime date, List<PlayerBoxScore> pbsList)
        {
            this.bs = bs;
            this.date = date;
            this.pbsList = pbsList;
        }
    }
}