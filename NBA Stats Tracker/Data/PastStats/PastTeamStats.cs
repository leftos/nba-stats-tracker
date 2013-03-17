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

namespace NBA_Stats_Tracker.Data.PastStats
{
    using System.Data;

    using LeftosCommonLibrary;

    public class PastTeamStats
    {
        public PastTeamStats()
        {
        }

        public PastTeamStats(DataRow dr)
        {
            Wins = ParseCell.GetUInt32(dr, "WIN");
            Losses = ParseCell.GetUInt32(dr, "LOSS");
            TeamID = ParseCell.GetInt32(dr, "TeamID");

            MINS = ParseCell.GetUInt32(dr, "MINS");
            PF = ParseCell.GetUInt32(dr, "PF");
            PA = ParseCell.GetUInt32(dr, "PA");
            FGM = ParseCell.GetUInt32(dr, "FGM");
            FGA = ParseCell.GetUInt32(dr, "FGA");
            TPM = ParseCell.GetUInt32(dr, "TPM");
            TPA = ParseCell.GetUInt32(dr, "TPA");
            FTM = ParseCell.GetUInt32(dr, "FTM");
            FTA = ParseCell.GetUInt32(dr, "FTA");
            OREB = ParseCell.GetUInt32(dr, "OREB");
            DREB = ParseCell.GetUInt32(dr, "DREB");
            REB = OREB + DREB;
            STL = ParseCell.GetUInt32(dr, "STL");
            TOS = ParseCell.GetUInt32(dr, "TOS");
            BLK = ParseCell.GetUInt32(dr, "BLK");
            AST = ParseCell.GetUInt32(dr, "AST");
            FOUL = ParseCell.GetUInt32(dr, "FOUL");

            SeasonName = ParseCell.GetString(dr, "SeasonName");
            Order = ParseCell.GetInt32(dr, "SOrder");
            IsPlayoff = ParseCell.GetBoolean(dr, "isPlayoff");
            ID = ParseCell.GetInt32(dr, "ID");
        }

        public int TeamID { get; set; }
        public int ID { get; set; }
        public string SeasonName { get; set; }
        public int Order { get; set; }
        public bool IsPlayoff { get; set; }
        public uint Wins { get; set; }
        public uint Losses { get; set; }
        public uint MINS { get; set; }
        public uint PF { get; set; }
        public uint PA { get; set; }
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

        public void EndEdit()
        {
            REB = OREB + DREB;
            PF = (FGM - TPM) * 2 + TPM * 3 + FTM;
        }
    }
}