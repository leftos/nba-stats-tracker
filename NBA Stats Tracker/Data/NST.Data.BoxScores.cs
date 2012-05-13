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
using System.Data;

#endregion

namespace NBA_Stats_Tracker.Data
{
    public class BoxScore
    {
        public UInt16 AST1 { get; set; }
        public UInt16 AST2 { get; set; }
        public UInt16 BLK1{ get; set; }
        public UInt16 BLK2{ get; set; }
        public UInt16 FGA1{ get; set; }
        public UInt16 FGA2{ get; set; }
        public UInt16 FGM1{ get; set; }
        public UInt16 FGM2{ get; set; }
        public UInt16 FTA1{ get; set; }
        public UInt16 FTA2{ get; set; }
        public UInt16 FTM1{ get; set; }
        public UInt16 FTM2{ get; set; }
        public UInt16 MINS1{ get; set; }
        public UInt16 MINS2{ get; set; }
        public UInt16 OREB1{ get; set; }
        public UInt16 OREB2{ get; set; }
        public UInt16 FOUL1{ get; set; }
        public UInt16 FOUL2{ get; set; }
        public UInt16 PTS1{ get; set; }
        public UInt16 PTS2{ get; set; }
        public UInt16 REB1{ get; set; }
        public UInt16 REB2{ get; set; }
        public UInt16 STL1{ get; set; }
        public UInt16 STL2{ get; set; }
        public int SeasonNum{ get; set; }
        public UInt16 TO1{ get; set; }
        public UInt16 TO2{ get; set; }
        public UInt16 TPA1{ get; set; }
        public UInt16 TPA2{ get; set; }
        public UInt16 TPM1{ get; set; }
        public UInt16 TPM2{ get; set; }
        public string Team1{ get; set; }
        public string Team2{ get; set; }
        public int bshistid { get; set; }
        public bool doNotUpdate{ get; set; }
        public bool done{ get; set; }
        public DateTime gamedate{ get; set; }
        public int id{ get; set; }
        public bool isPlayoff{ get; set; }

        public BoxScore()
        {
            id = -1;
            bshistid = -1;
        }

        public BoxScore(DataRow r)
        {
            id = Convert.ToInt32(r["GameID"].ToString());
            Team1 = r["T1Name"].ToString();
            Team2 = r["T2Name"].ToString();
            gamedate = Convert.ToDateTime(r["Date"].ToString());
            SeasonNum = Convert.ToInt32(r["SeasonNum"].ToString());
            isPlayoff = Convert.ToBoolean(r["IsPlayoff"].ToString());
            PTS1 = Convert.ToUInt16(r["T1PTS"].ToString());
            REB1 = Convert.ToUInt16(r["T1REB"].ToString());
            AST1 = Convert.ToUInt16(r["T1AST"].ToString());
            STL1 = Convert.ToUInt16(r["T1STL"].ToString());
            BLK1 = Convert.ToUInt16(r["T1BLK"].ToString());
            TO1 = Convert.ToUInt16(r["T1TOS"].ToString());
            FGM1 = Convert.ToUInt16(r["T1FGM"].ToString());
            FGA1 = Convert.ToUInt16(r["T1FGA"].ToString());
            TPM1 = Convert.ToUInt16(r["T13PM"].ToString());
            TPA1 = Convert.ToUInt16(r["T13PA"].ToString());
            FTM1 = Convert.ToUInt16(r["T1FTM"].ToString());
            FTA1 = Convert.ToUInt16(r["T1FTA"].ToString());
            OREB1 = Convert.ToUInt16(r["T1OREB"].ToString());
            FOUL1 = Convert.ToUInt16(r["T1FOUL"].ToString());
            MINS1 = Convert.ToUInt16(r["T1MINS"].ToString());

            PTS2 = Convert.ToUInt16(r["T2PTS"].ToString());
            REB2 = Convert.ToUInt16(r["T2REB"].ToString());
            AST2 = Convert.ToUInt16(r["T2AST"].ToString());
            STL2 = Convert.ToUInt16(r["T2STL"].ToString());
            BLK2 = Convert.ToUInt16(r["T2BLK"].ToString());
            TO2 = Convert.ToUInt16(r["T2TOS"].ToString());
            FGM2 = Convert.ToUInt16(r["T2FGM"].ToString());
            FGA2 = Convert.ToUInt16(r["T2FGA"].ToString());
            TPM2 = Convert.ToUInt16(r["T23PM"].ToString());
            TPA2 = Convert.ToUInt16(r["T23PA"].ToString());
            FTM2 = Convert.ToUInt16(r["T2FTM"].ToString());
            FTA2 = Convert.ToUInt16(r["T2FTA"].ToString());
            OREB2 = Convert.ToUInt16(r["T2OREB"].ToString());
            FOUL2 = Convert.ToUInt16(r["T2FOUL"].ToString());
            MINS2 = Convert.ToUInt16(r["T2MINS"].ToString());
        }
    }

    public class BoxScoreEntry
    {
        public BoxScore bs { get; set; }
        public DateTime date;
        public bool mustUpdate;
        public List<PlayerBoxScore> pbsList;

        public string Team1Display { get; set; }
        public string Team2Display { get; set; }

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