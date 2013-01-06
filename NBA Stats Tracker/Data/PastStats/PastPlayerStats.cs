using System.Data;
using LeftosCommonLibrary;

namespace NBA_Stats_Tracker.Data.PastStats
{
    public class PastPlayerStats
    {
        public int PlayerID { get; set; }
        public int ID { get; set; }
        public string SeasonName { get; set; }
        public int Order { get; set; }
        public bool isPlayoff { get; set; }
        public string TeamF { get; set; }
        public string TeamS { get; set; }
        public uint GP { get; set; }
        public uint GS { get; set; }
        public uint MINS { get; set; }
        public uint PTS { get; set; }
        public uint FGM { get; set; }
        public uint FGA { get; set; }
        public uint TPM { get; set; }
        public uint TPA { get; set; }
        public uint FTM { get; set; }
        public uint FTA { get; set; }
        public uint REB { get; set; }
        public uint OREB { get; set; }
        public uint DREB { get; set; }
        public uint STL { get; set; }
        public uint TOS { get; set; }
        public uint BLK { get; set; }
        public uint AST { get; set; }
        public uint FOUL { get; set; }

        public PastPlayerStats()
        {
        }

        public PastPlayerStats(DataRow dr)
        {
            GP = Tools.getUInt32(dr, "GP");
            GS = Tools.getUInt32(dr, "GS");
            PlayerID = Tools.getInt(dr, "PlayerID");

            MINS = Tools.getUInt32(dr, "MINS");
            PTS = Tools.getUInt32(dr, "PTS");
            FGM = Tools.getUInt32(dr, "FGM");
            FGA = Tools.getUInt32(dr, "FGA");
            TPM = Tools.getUInt32(dr, "TPM");
            TPA = Tools.getUInt32(dr, "TPA");
            FTM = Tools.getUInt32(dr, "FTM");
            FTA = Tools.getUInt32(dr, "FTA");
            OREB = Tools.getUInt32(dr, "OREB");
            DREB = Tools.getUInt32(dr, "DREB");
            REB = OREB + DREB;
            STL = Tools.getUInt32(dr, "STL");
            TOS = Tools.getUInt32(dr, "TOS");
            BLK = Tools.getUInt32(dr, "BLK");
            AST = Tools.getUInt32(dr, "AST");
            FOUL = Tools.getUInt32(dr, "FOUL");

            SeasonName = Tools.getString(dr, "SeasonName");
            Order = Tools.getInt(dr, "SOrder");
            isPlayoff = Tools.getBoolean(dr, "isPlayoff");
            TeamF = Tools.getString(dr, "TeamFin");
            TeamS = Tools.getString(dr, "TeamSta");
            ID = Tools.getInt(dr, "ID");
        }

        public void EndEdit()
        {
            REB = OREB + DREB;
            PTS = (FGM - TPM)*2 + TPM*3 + FTM;
        }
    }
}