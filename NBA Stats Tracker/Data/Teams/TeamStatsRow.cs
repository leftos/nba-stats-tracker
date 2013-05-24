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

    using NBA_Stats_Tracker.Data.Players;

    #endregion

    public class TeamStatsRow
    {
        public TeamStatsRow(TeamStats ts, bool playoffs = false)
        {
            ID = ts.ID;
            Name = ts.Name;
            DisplayName = ts.DisplayName;
            IsHidden = ts.IsHidden;

            if (!playoffs)
            {
                Games = ts.GetGames();
                Wins = ts.Record[0];
                Losses = ts.Record[1];
                MINS = ts.Totals[TAbbr.MINS];
                PF = ts.Totals[TAbbr.PF];
                PA = ts.Totals[TAbbr.PA];
                FGM = ts.Totals[TAbbr.FGM];
                FGMPG = ((float) FGM / Games);
                FGA = ts.Totals[TAbbr.FGA];
                FGAPG = ((float) FGA / Games);
                TPM = ts.Totals[TAbbr.TPM];
                TPMPG = ((float) TPM / Games);
                TPA = ts.Totals[TAbbr.TPA];
                TPAPG = ((float) TPA / Games);
                FTM = ts.Totals[TAbbr.FTM];
                FTMPG = ((float) FTM / Games);
                FTA = ts.Totals[TAbbr.FTA];
                FTAPG = ((float) FTA / Games);
                OREB = ts.Totals[TAbbr.OREB];
                DREB = ts.Totals[TAbbr.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ts.Totals[TAbbr.STL];
                TOS = ts.Totals[TAbbr.TOS];
                BLK = ts.Totals[TAbbr.BLK];
                AST = ts.Totals[TAbbr.AST];
                FOUL = ts.Totals[TAbbr.FOUL];

                Wp = ts.PerGame[TAbbr.Wp];
                Weff = ts.PerGame[TAbbr.Weff];
                MPG = ts.PerGame[TAbbr.MPG];
                PPG = ts.PerGame[TAbbr.PPG];
                PAPG = ts.PerGame[TAbbr.PAPG];
                FGp = ts.PerGame[TAbbr.FGp];
                FGeff = ts.PerGame[TAbbr.FGeff];
                TPp = ts.PerGame[TAbbr.TPp];
                TPeff = ts.PerGame[TAbbr.TPeff];
                FTp = ts.PerGame[TAbbr.FTp];
                FTeff = ts.PerGame[TAbbr.FTeff];
                RPG = ts.PerGame[TAbbr.RPG];
                ORPG = ts.PerGame[TAbbr.ORPG];
                DRPG = ts.PerGame[TAbbr.DRPG];
                SPG = ts.PerGame[TAbbr.SPG];
                TPG = ts.PerGame[TAbbr.TPG];
                BPG = ts.PerGame[TAbbr.BPG];
                APG = ts.PerGame[TAbbr.APG];
                FPG = ts.PerGame[TAbbr.FPG];

                Poss = ts.Metrics["PossPG"];
                Pace = ts.Metrics["Pace"];
                ORTG = ts.Metrics["ORTG"];
                DRTG = ts.Metrics["DRTG"];
                ASTp = ts.Metrics["AST%"];
                DREBp = ts.Metrics["DREB%"];
                EFGp = ts.Metrics["EFG%"];
                EFFd = ts.Metrics["EFFd"];
                TOR = ts.Metrics["TOR"];
                OREBp = ts.Metrics["OREB%"];
                FTR = ts.Metrics["FTR"];
                PWp = ts.Metrics["PW%"];
                TSp = ts.Metrics["TS%"];
                TPR = ts.Metrics["3PR"];
                PythW = ts.Metrics["PythW"];
                PythL = ts.Metrics["PythL"];
            }
            else
            {
                Games = ts.GetPlayoffGames();
                Wins = ts.PlRecord[0];
                Losses = ts.PlRecord[1];
                MINS = ts.PlTotals[TAbbr.MINS];
                PF = ts.PlTotals[TAbbr.PF];
                PA = ts.PlTotals[TAbbr.PA];
                FGM = ts.PlTotals[TAbbr.FGM];
                FGMPG = ((float) FGM / Games);
                FGA = ts.PlTotals[TAbbr.FGA];
                FGAPG = ((float) FGA / Games);
                TPM = ts.PlTotals[TAbbr.TPM];
                TPMPG = ((float) TPM / Games);
                TPA = ts.PlTotals[TAbbr.TPA];
                TPAPG = ((float) TPA / Games);
                FTM = ts.PlTotals[TAbbr.FTM];
                FTMPG = ((float) FTM / Games);
                FTA = ts.PlTotals[TAbbr.FTA];
                FTAPG = ((float) FTA / Games);
                OREB = ts.PlTotals[TAbbr.OREB];
                DREB = ts.PlTotals[TAbbr.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ts.PlTotals[TAbbr.STL];
                TOS = ts.PlTotals[TAbbr.TOS];
                BLK = ts.PlTotals[TAbbr.BLK];
                AST = ts.PlTotals[TAbbr.AST];
                FOUL = ts.PlTotals[TAbbr.FOUL];

                Wp = ts.PlPerGame[TAbbr.Wp];
                Weff = ts.PlPerGame[TAbbr.Weff];
                PPG = ts.PlPerGame[TAbbr.PPG];
                PAPG = ts.PlPerGame[TAbbr.PAPG];
                FGp = ts.PlPerGame[TAbbr.FGp];
                FGeff = ts.PlPerGame[TAbbr.FGeff];
                TPp = ts.PlPerGame[TAbbr.TPp];
                TPeff = ts.PlPerGame[TAbbr.TPeff];
                FTp = ts.PlPerGame[TAbbr.FTp];
                FTeff = ts.PlPerGame[TAbbr.FTeff];
                RPG = ts.PlPerGame[TAbbr.RPG];
                ORPG = ts.PlPerGame[TAbbr.ORPG];
                DRPG = ts.PlPerGame[TAbbr.DRPG];
                SPG = ts.PlPerGame[TAbbr.SPG];
                TPG = ts.PlPerGame[TAbbr.TPG];
                BPG = ts.PlPerGame[TAbbr.BPG];
                APG = ts.PlPerGame[TAbbr.APG];
                FPG = ts.PlPerGame[TAbbr.FPG];

                Poss = ts.PlMetrics["PossPG"];
                Pace = ts.PlMetrics["Pace"];
                ORTG = ts.PlMetrics["ORTG"];
                DRTG = ts.PlMetrics["DRTG"];
                ASTp = ts.PlMetrics["AST%"];
                DREBp = ts.PlMetrics["DREB%"];
                EFGp = ts.PlMetrics["EFG%"];
                EFFd = ts.PlMetrics["EFFd"];
                TOR = ts.PlMetrics["TOR"];
                OREBp = ts.PlMetrics["OREB%"];
                FTR = ts.PlMetrics["FTR"];
                PWp = ts.PlMetrics["PW%"];
                TSp = ts.PlMetrics["TS%"];
                TPR = ts.PlMetrics["3PR"];
                PythW = ts.PlMetrics["PythW"];
                PythL = ts.PlMetrics["PythL"];
            }

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
    }
}