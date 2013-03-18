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

namespace NBA_Stats_Tracker.Data.Teams
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Data.SQLiteIO;
    using NBA_Stats_Tracker.Windows.MainInterface;

    #endregion

    /// <summary>Contains all the information for the teams' performances in a game.</summary>
    public class TeamBoxScore
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamBoxScore" /> class.
        /// </summary>
        public TeamBoxScore()
        {
            ID = -1;
            BSHistID = -1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamBoxScore" /> class.
        /// </summary>
        /// <param name="r">The SQLite query result row which contains the required information.</param>
        public TeamBoxScore(DataRow r, Dictionary<int, TeamStats> tst)
        {
            ID = Convert.ToInt32(r["GameID"].ToString());
            try
            {
                Team1ID = Convert.ToInt32(r["Team1ID"].ToString());
                Team2ID = Convert.ToInt32(r["Team2ID"].ToString());
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is KeyNotFoundException)
                {
                    Team1ID = tst.Single(ts => ts.Value.Name == ParseCell.GetString(r, "T1Name")).Value.ID;
                    Team2ID = tst.Single(ts => ts.Value.Name == ParseCell.GetString(r, "T2Name")).Value.ID;
                }
                else
                {
                    throw;
                }
            }
            GameDate = Convert.ToDateTime(r["Date"].ToString());
            SeasonNum = Convert.ToInt32(r["SeasonNum"].ToString());
            IsPlayoff = Convert.ToBoolean(r["IsPlayoff"].ToString());
            PTS1 = Convert.ToUInt16(r["T1PTS"].ToString());
            REB1 = Convert.ToUInt16(r["T1REB"].ToString());
            AST1 = Convert.ToUInt16(r["T1AST"].ToString());
            STL1 = Convert.ToUInt16(r["T1STL"].ToString());
            BLK1 = Convert.ToUInt16(r["T1BLK"].ToString());
            TOS1 = Convert.ToUInt16(r["T1TOS"].ToString());
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
            TOS2 = Convert.ToUInt16(r["T2TOS"].ToString());
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeamBoxScore" /> class.
        /// </summary>
        /// <param name="ds">The Basketball-Reference.com dataset resulting from the parsing.</param>
        /// <param name="dateParts">The parts of the date string.</param>
        public TeamBoxScore(DataSet ds, string[] dateParts)
        {
            var away = ds.Tables[0];
            var home = ds.Tables[1];

            var segmentsDone = 0;
            foreach (var team in MainWindow.TST)
            {
                if (dateParts[0].Contains(team.Value.Name))
                {
                    Team1ID = team.Key;
                    segmentsDone++;
                }
                if (dateParts[1].Contains(team.Value.Name))
                {
                    Team2ID = team.Key;
                    segmentsDone++;
                }
                if (segmentsDone == 2)
                {
                    break;
                }
            }
            if (segmentsDone != 2)
            {
                Team1ID = -2;
                Team2ID = -2;
                return;
            }
            var date = dateParts[2].Trim() + ", " + dateParts[3].Trim();
            GameDate = Convert.ToDateTime(date);

            ID = SQLiteIO.GetFreeID(MainWindow.CurrentDB, "GameResults", "GameID");
            SeasonNum = MainWindow.CurSeason;

            var rt = away.Rows[away.Rows.Count - 1];
            PTS1 = ParseCell.GetUInt16(rt, "PTS");
            REB1 = Convert.ToUInt16(rt["TRB"].ToString());
            AST1 = Convert.ToUInt16(rt["AST"].ToString());
            STL1 = Convert.ToUInt16(rt["STL"].ToString());
            BLK1 = Convert.ToUInt16(rt["BLK"].ToString());
            TOS1 = Convert.ToUInt16(rt["TOV"].ToString());
            FGM1 = Convert.ToUInt16(rt["FG"].ToString());
            FGA1 = Convert.ToUInt16(rt["FGA"].ToString());
            TPM1 = Convert.ToUInt16(rt["3P"].ToString());
            TPA1 = Convert.ToUInt16(rt["3PA"].ToString());
            FTM1 = Convert.ToUInt16(rt["FT"].ToString());
            FTA1 = Convert.ToUInt16(rt["FTA"].ToString());
            OREB1 = Convert.ToUInt16(rt["ORB"].ToString());
            FOUL1 = Convert.ToUInt16(rt["PF"].ToString());
            MINS1 = (ushort) (Convert.ToUInt16(rt["MP"].ToString()) / 5);

            rt = home.Rows[home.Rows.Count - 1];
            PTS2 = ParseCell.GetUInt16(rt, "PTS");
            REB2 = Convert.ToUInt16(rt["TRB"].ToString());
            AST2 = Convert.ToUInt16(rt["AST"].ToString());
            STL2 = Convert.ToUInt16(rt["STL"].ToString());
            BLK2 = Convert.ToUInt16(rt["BLK"].ToString());
            TOS2 = Convert.ToUInt16(rt["TOV"].ToString());
            FGM2 = Convert.ToUInt16(rt["FG"].ToString());
            FGA2 = Convert.ToUInt16(rt["FGA"].ToString());
            TPM2 = Convert.ToUInt16(rt["3P"].ToString());
            TPA2 = Convert.ToUInt16(rt["3PA"].ToString());
            FTM2 = Convert.ToUInt16(rt["FT"].ToString());
            FTA2 = Convert.ToUInt16(rt["FTA"].ToString());
            OREB2 = Convert.ToUInt16(rt["ORB"].ToString());
            FOUL2 = Convert.ToUInt16(rt["PF"].ToString());
            MINS2 = (ushort) (Convert.ToUInt16(rt["MP"].ToString()) / 5);
        }

        public UInt16 AST1 { get; set; }
        public UInt16 AST2 { get; set; }
        public UInt16 BLK1 { get; set; }
        public UInt16 BLK2 { get; set; }
        public UInt16 FGA1 { get; set; }
        public UInt16 FGA2 { get; set; }
        public UInt16 FGM1 { get; set; }
        public UInt16 FGM2 { get; set; }
        public UInt16 FTA1 { get; set; }
        public UInt16 FTA2 { get; set; }
        public UInt16 FTM1 { get; set; }
        public UInt16 FTM2 { get; set; }
        public UInt16 MINS1 { get; set; }
        public UInt16 MINS2 { get; set; }
        public UInt16 OREB1 { get; set; }
        public UInt16 OREB2 { get; set; }
        public UInt16 FOUL1 { get; set; }
        public UInt16 FOUL2 { get; set; }
        public UInt16 PTS1 { get; set; }
        public UInt16 PTS2 { get; set; }
        public UInt16 REB1 { get; set; }
        public UInt16 REB2 { get; set; }
        public UInt16 STL1 { get; set; }
        public UInt16 STL2 { get; set; }
        public int SeasonNum { get; set; }
        public UInt16 TOS1 { get; set; }
        public UInt16 TOS2 { get; set; }
        public UInt16 TPA1 { get; set; }
        public UInt16 TPA2 { get; set; }
        public UInt16 TPM1 { get; set; }
        public UInt16 TPM2 { get; set; }
        public int Team1ID { get; set; }
        public int Team2ID { get; set; }
        public int BSHistID { get; set; }
        public bool DoNotUpdate { get; set; }
        public bool Done { get; set; }
        public DateTime GameDate { get; set; }
        public int ID { get; set; }
        public bool IsPlayoff { get; set; }

        public string DisplayTeam { get; set; }
        public string DisplayOpponent { get; set; }
        public string DisplayResult { get; set; }
        public string DisplayLocation { get; set; }
        public float FGp { get; set; }
        public float TPp { get; set; }
        public float FTp { get; set; }
        public ushort DisplayREB { get; set; }
        public ushort DisplayOREB { get; set; }
        public ushort DisplayAST { get; set; }
        public ushort DisplayTO { get; set; }
        public ushort DisplayBLK { get; set; }
        public ushort DisplaySTL { get; set; }
        public ushort DisplayFOUL { get; set; }
        public double DisplayGmSc { get; set; }

        /// <summary>Prepares the presentation fields of the class.</summary>
        /// <param name="teamID">The team.</param>
        public void PrepareForDisplay(Dictionary<int, TeamStats> tst)
        {
            DisplayTeam = tst[Team2ID].DisplayName;
            DisplayOpponent = tst[Team1ID].DisplayName;
            DisplayResult = PTS1 + "-" + PTS2;
        }

        /// <summary>Prepares the presentation fields of the class.</summary>
        /// <param name="teamID">The team.</param>
        public void PrepareForDisplay(Dictionary<int, TeamStats> tst, int teamID)
        {
            if (teamID == Team1ID)
            {
                DisplayTeam = tst[Team1ID].DisplayName;
                DisplayOpponent = tst[Team2ID].DisplayName;
                DisplayLocation = "Away";
                DisplayResult = PTS1 > PTS2 ? "W " : "L ";
                FGp = (float) FGM1 / FGA1;
                TPp = (float) TPM1 / TPA1;
                FTp = (float) FTM1 / FTA1;
                DisplayREB = REB1;
                DisplayOREB = OREB1;
                DisplayAST = AST1;
                DisplayTO = TOS1;
                DisplayBLK = BLK1;
                DisplaySTL = STL1;
                DisplayFOUL = FOUL1;

                var temp = new TeamStats();
                var tempopp = new TeamStats();
                TeamStats.AddTeamStatsFromBoxScore(this, ref temp, ref tempopp);
                temp.CalcMetrics(tempopp);

                DisplayGmSc = temp.Metrics["GmSc"];
            }
            else
            {
                DisplayTeam = tst[Team2ID].DisplayName;
                DisplayOpponent = tst[Team1ID].DisplayName;
                DisplayLocation = "Home";
                DisplayResult = PTS1 < PTS2 ? "W " : "L ";
                FGp = (float) FGM2 / FGA2;
                TPp = (float) TPM2 / TPA2;
                FTp = (float) FTM2 / FTA2;
                DisplayREB = REB2;
                DisplayOREB = OREB2;
                DisplayAST = AST2;
                DisplayTO = TOS2;
                DisplayBLK = BLK2;
                DisplaySTL = STL2;
                DisplayFOUL = FOUL2;

                var temp = new TeamStats();
                var tempopp = new TeamStats();
                TeamStats.AddTeamStatsFromBoxScore(this, ref tempopp, ref temp);
                temp.CalcMetrics(tempopp);

                DisplayGmSc = temp.Metrics["GmSc"];
            }
            DisplayResult += PTS1 + "-" + PTS2;
        }
    }
}