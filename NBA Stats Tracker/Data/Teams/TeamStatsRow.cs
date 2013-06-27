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
    using System.Linq;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.Players;

    #endregion

    public class TeamStatsRow
    {
        public TeamStatsRow()
        {
            PBPSList = new List<PlayerPBPStats>();
        }

        public TeamStatsRow(TeamStats ts, bool playoffs = false)
            : this()
        {
            ID = ts.ID;
            Name = ts.Name;
            DisplayName = ts.DisplayName;
            IsHidden = ts.IsHidden;

            Games = !playoffs ? ts.GetGames() : ts.GetPlayoffGames();
            Wins = !playoffs ? ts.Record[0] : ts.PlRecord[0];
            Losses = !playoffs ? ts.Record[1] : ts.PlRecord[1];

            var totals = !playoffs ? ts.Totals : ts.PlTotals;
            var perGame = !playoffs ? ts.PerGame : ts.PlPerGame;
            var metrics = !playoffs ? ts.Metrics : ts.PlMetrics;

            MINS = totals[TAbbrT.MINS];
            PF = totals[TAbbrT.PF];
            PA = totals[TAbbrT.PA];
            FGM = totals[TAbbrT.FGM];
            FGMPG = ((float) FGM / Games);
            FGA = totals[TAbbrT.FGA];
            FGAPG = ((float) FGA / Games);
            TPM = totals[TAbbrT.TPM];
            TPMPG = ((float) TPM / Games);
            TPA = totals[TAbbrT.TPA];
            TPAPG = ((float) TPA / Games);
            FTM = totals[TAbbrT.FTM];
            FTMPG = ((float) FTM / Games);
            FTA = totals[TAbbrT.FTA];
            FTAPG = ((float) FTA / Games);
            OREB = totals[TAbbrT.OREB];
            DREB = totals[TAbbrT.DREB];
            REB = (UInt16) (OREB + DREB);
            STL = totals[TAbbrT.STL];
            TOS = totals[TAbbrT.TOS];
            BLK = totals[TAbbrT.BLK];
            AST = totals[TAbbrT.AST];
            FOUL = totals[TAbbrT.FOUL];

            Wp = perGame[TAbbrPG.Wp];
            Weff = perGame[TAbbrPG.Weff];
            MPG = perGame[TAbbrPG.MPG];
            PPG = perGame[TAbbrPG.PPG];
            PAPG = perGame[TAbbrPG.PAPG];
            PD = PPG - PAPG;
            FGp = perGame[TAbbrPG.FGp];
            FGeff = perGame[TAbbrPG.FGeff];
            TPp = perGame[TAbbrPG.TPp];
            TPeff = perGame[TAbbrPG.TPeff];
            FTp = perGame[TAbbrPG.FTp];
            FTeff = perGame[TAbbrPG.FTeff];
            RPG = perGame[TAbbrPG.RPG];
            ORPG = perGame[TAbbrPG.ORPG];
            DRPG = perGame[TAbbrPG.DRPG];
            SPG = perGame[TAbbrPG.SPG];
            TPG = perGame[TAbbrPG.TPG];
            BPG = perGame[TAbbrPG.BPG];
            APG = perGame[TAbbrPG.APG];
            FPG = perGame[TAbbrPG.FPG];

            Poss = metrics["PossPG"];
            Pace = metrics["Pace"];
            ORTG = metrics["ORTG"];
            DRTG = metrics["DRTG"];
            ASTp = metrics["AST%"];
            DREBp = metrics["DREB%"];
            EFGp = metrics["EFG%"];
            EFFd = metrics["EFFd"];
            TOR = metrics["TOR"];
            OREBp = metrics["OREB%"];
            FTR = metrics["FTR"];
            PWp = metrics["PW%"];
            TSp = metrics["TS%"];
            TPR = metrics["3PR"];
            PythW = metrics["PythW"];
            PythL = metrics["PythL"];
            GmSc = metrics["GmSc"];

            CurStreak = ts.CurStreak;
        }

        public TeamStatsRow(TeamStats ts, Dictionary<int, PlayerStats> pst, bool playoffs = false)
            : this(ts, playoffs)
        {
            CalculateTotalContracts(pst);
            CalculatePlayerCounts(pst);
        }

        public TeamStatsRow(TeamStats ts, Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats, bool playoffs = false)
            : this(ts, playoffs)
        {
            DivW = splitTeamStats[ID]["Division"].Record[0];
            DivL = splitTeamStats[ID]["Division"].Record[1];
            ConfW = splitTeamStats[ID]["Conference"].Record[0];
            ConfL = splitTeamStats[ID]["Conference"].Record[1];
            L10W = splitTeamStats[ID]["Last 10"].Record[0];
            L10L = splitTeamStats[ID]["Last 10"].Record[1];
        }

        public TeamStatsRow(
            TeamStats ts,
            Dictionary<int, PlayerStats> pst,
            Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats,
            bool playoffs = false)
            : this(ts, splitTeamStats, playoffs)
        {
            CalculateTotalContracts(pst);
            CalculatePlayerCounts(pst);
        }

        public double GmSc { get; set; }

        public int ID { get; set; }
        public uint Games { get; set; }
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

        public float Wp { get; set; }
        public float Weff { get; set; }
        public float MPG { get; set; }
        public float PPG { get; set; }
        public float PAPG { get; set; }
        public float PD { get; set; }
        public float FGp { get; set; }
        public float FGeff { get; set; }
        public float TPp { get; set; }
        public float TPeff { get; set; }
        public float FTp { get; set; }
        public float FTeff { get; set; }
        public float RPG { get; set; }
        public float ORPG { get; set; }
        public float DRPG { get; set; }
        public float SPG { get; set; }
        public float TPG { get; set; }
        public float BPG { get; set; }
        public float APG { get; set; }
        public float FPG { get; set; }

        public float FGMPG { get; set; }
        public float FGAPG { get; set; }
        public float TPMPG { get; set; }
        public float TPAPG { get; set; }
        public float FTMPG { get; set; }
        public float FTAPG { get; set; }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public double ORTG { get; set; }
        public double DRTG { get; set; }
        public double EFFd { get; set; }
        public double PWp { get; set; }
        public double PythW { get; set; }
        public double PythL { get; set; }
        public double TSp { get; set; }
        public double EFGp { get; set; }
        public double TPR { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double ASTp { get; set; }
        public double TOR { get; set; }
        public double FTR { get; set; }
        public double Poss { get; set; }
        public double Pace { get; set; }

        public int ContractsY1 { get; set; }
        public int ContractsY2 { get; set; }
        public int ContractsY3 { get; set; }
        public int ContractsY4 { get; set; }
        public int ContractsY5 { get; set; }
        public int ContractsY6 { get; set; }
        public int ContractsY7 { get; set; }

        public int PGCount { get; set; }
        public int SGCount { get; set; }
        public int SFCount { get; set; }
        public int PFCount { get; set; }
        public int CCount { get; set; }
        public int PlCount { get; set; }

        public uint DivW { get; set; }
        public uint DivL { get; set; }

        public string DivRecord
        {
            get { return String.Format("{0}-{1}", DivW, DivL); }
        }

        public uint ConfW { get; set; }
        public uint ConfL { get; set; }

        public string ConfRecord
        {
            get { return String.Format("{0}-{1}", ConfW, ConfL); }
        }

        public uint L10W { get; set; }
        public uint L10L { get; set; }

        public string L10Record
        {
            get { return String.Format("{0}-{1}", L10W, L10L); }
        }

        public string CurStreak { get; set; }

        public bool IsHidden { get; set; }

        public bool Highlight { get; set; }
        public int InjuredCount { get; set; }
        public List<PlayerPBPStats> PBPSList { get; set; }

        public static void TryChangeTSR(ref TeamStatsRow tsr, Dictionary<string, string> dict)
        {
            tsr.Wins = tsr.Wins.TrySetValue(dict, "Wins", typeof(UInt16));
            tsr.Losses = tsr.Losses.TrySetValue(dict, "Losses", typeof(UInt16));
            tsr.MINS = tsr.MINS.TrySetValue(dict, "MINS", typeof(UInt16));
            tsr.PF = tsr.PF.TrySetValue(dict, "PF", typeof(UInt16));
            tsr.PA = tsr.PF.TrySetValue(dict, "PA", typeof(UInt16));
            tsr.FGM = tsr.FGM.TrySetValue(dict, "FGM", typeof(UInt16));
            tsr.FGA = tsr.FGA.TrySetValue(dict, "FGA", typeof(UInt16));
            tsr.TPM = tsr.TPM.TrySetValue(dict, "3PM", typeof(UInt16));
            tsr.TPA = tsr.TPA.TrySetValue(dict, "3PA", typeof(UInt16));
            tsr.FTM = tsr.FTM.TrySetValue(dict, "FTM", typeof(UInt16));
            tsr.FTA = tsr.FTA.TrySetValue(dict, "FTA", typeof(UInt16));
            tsr.REB = tsr.REB.TrySetValue(dict, "REB", typeof(UInt16));
            tsr.OREB = tsr.OREB.TrySetValue(dict, "OREB", typeof(UInt16));
            tsr.DREB = tsr.DREB.TrySetValue(dict, "DREB", typeof(UInt16));
            tsr.AST = tsr.AST.TrySetValue(dict, "AST", typeof(UInt16));
            tsr.TOS = tsr.TOS.TrySetValue(dict, "TO", typeof(UInt16));
            tsr.STL = tsr.STL.TrySetValue(dict, "STL", typeof(UInt16));
            tsr.BLK = tsr.BLK.TrySetValue(dict, "BLK", typeof(UInt16));
            tsr.FOUL = tsr.FOUL.TrySetValue(dict, "FOUL", typeof(UInt16));
        }

        public static void Refresh(ref TeamStatsRow tsr)
        {
            tsr = new TeamStatsRow(new TeamStats(tsr));
        }

        public void CalculateTotalContracts(Dictionary<int, PlayerStats> pst)
        {
            var teamPlayers = pst.Values.Where(ps => ps.TeamF == ID).ToList();
            ContractsY1 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(1)).Sum();
            ContractsY2 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(2)).Sum();
            ContractsY3 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(3)).Sum();
            ContractsY4 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(4)).Sum();
            ContractsY5 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(5)).Sum();
            ContractsY6 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(6)).Sum();
            ContractsY7 = teamPlayers.Select(ps => ps.Contract.TryGetSalary(7)).Sum();
        }

        public void CalculatePlayerCounts(Dictionary<int, PlayerStats> pst)
        {
            var teamPlayers = pst.Values.Where(ps => ps.TeamF == ID).ToList();
            PlCount = teamPlayers.Count;
            PGCount = teamPlayers.Count(ps => ps.Position1 == Position.PG);
            SGCount = teamPlayers.Count(ps => ps.Position1 == Position.SG);
            SFCount = teamPlayers.Count(ps => ps.Position1 == Position.SF);
            PFCount = teamPlayers.Count(ps => ps.Position1 == Position.PF);
            CCount = teamPlayers.Count(ps => ps.Position1 == Position.C);
            InjuredCount = teamPlayers.Count(ps => ps.Injury.IsInjured);
        }

        public void PopulatePBPSList(IEnumerable<BoxScoreEntry> bseList)
        {
            PBPSList.Clear();
            var teamBSEList = bseList.Where(bse => bse.BS.Team1ID == ID || bse.BS.Team2ID == ID).ToList();
            for (var i = 0; i < 7; i++)
            {
                PBPSList.Add(new PlayerPBPStats());
            }
            foreach (var bse in teamBSEList)
            {
                var pbpeList = bse.PBPEList;
                var list = PBPSList;
                var teamPlayerIDs = bse.PBSList.Where(pbs => pbs.TeamID == ID).Select(pbs => pbs.PlayerID).ToList();
                PlayerPBPStats.AddShotsToList(ref list, teamPlayerIDs, pbpeList);
                PBPSList[6].AddOtherStats(teamPlayerIDs, pbpeList, false);
            }
        }

        public TResult GetValue<TResult>(string prop)
        {
            return this.GetValue<TeamStatsRow, TResult>(prop);
        }
    }
}