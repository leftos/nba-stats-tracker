using System;
using NBA_Stats_Tracker.Windows;

namespace NBA_Stats_Tracker.Data.Players
{
    public class PlayerHighsRow
    {
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

        public PlayerHighsRow(int playerID, string type, UInt16[] highs)
        {
            PlayerID = playerID;
            Type = type;
            MINS = highs[p.MINS];
            PTS = highs[p.PTS];
            FGM = highs[p.FGM];
            FGA = highs[p.FGA];
            TPM = highs[p.TPM];
            TPA = highs[p.TPA];
            FTM = highs[p.FTM];
            FTA = highs[p.FTA];
            REB = highs[p.REB];
            OREB = highs[p.OREB];
            DREB = highs[p.DREB];
            STL = highs[p.STL];
            TOS = highs[p.TOS];
            BLK = highs[p.BLK];
            AST = highs[p.AST];
            FOUL = highs[p.FOUL];
        }
    }
}