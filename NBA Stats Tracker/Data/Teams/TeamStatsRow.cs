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
using System.Collections.Generic;
using System.Linq;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Players;

#endregion

namespace NBA_Stats_Tracker.Data.Teams
{
    public class TeamStatsRow
    {
        public TeamStatsRow(TeamStats ts, bool playoffs = false)
        {
            ID = ts.ID;
            Name = ts.name;
            DisplayName = ts.displayName;
            IsHidden = ts.isHidden;

            if (!playoffs)
            {
                Games = ts.getGames();
                Wins = ts.winloss[0];
                Losses = ts.winloss[1];
                MINS = ts.stats[t.MINS];
                PF = ts.stats[t.PF];
                PA = ts.stats[t.PA];
                FGM = ts.stats[t.FGM];
                FGMPG = ((float) FGM/Games);
                FGA = ts.stats[t.FGA];
                FGAPG = ((float) FGA/Games);
                TPM = ts.stats[t.TPM];
                TPMPG = ((float) TPM/Games);
                TPA = ts.stats[t.TPA];
                TPAPG = ((float) TPA/Games);
                FTM = ts.stats[t.FTM];
                FTMPG = ((float) FTM/Games);
                FTA = ts.stats[t.FTA];
                FTAPG = ((float) FTA/Games);
                OREB = ts.stats[t.OREB];
                DREB = ts.stats[t.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ts.stats[t.STL];
                TOS = ts.stats[t.TOS];
                BLK = ts.stats[t.BLK];
                AST = ts.stats[t.AST];
                FOUL = ts.stats[t.FOUL];

                Wp = ts.averages[t.Wp];
                Weff = ts.averages[t.Weff];
                MPG = ts.averages[t.MPG];
                PPG = ts.averages[t.PPG];
                PAPG = ts.averages[t.PAPG];
                FGp = ts.averages[t.FGp];
                FGeff = ts.averages[t.FGeff];
                TPp = ts.averages[t.TPp];
                TPeff = ts.averages[t.TPeff];
                FTp = ts.averages[t.FTp];
                FTeff = ts.averages[t.FTeff];
                RPG = ts.averages[t.RPG];
                ORPG = ts.averages[t.ORPG];
                DRPG = ts.averages[t.DRPG];
                SPG = ts.averages[t.SPG];
                TPG = ts.averages[t.TPG];
                BPG = ts.averages[t.BPG];
                APG = ts.averages[t.APG];
                FPG = ts.averages[t.FPG];

                Poss = ts.metrics["PossPG"];
                Pace = ts.metrics["Pace"];
                ORTG = ts.metrics["ORTG"];
                DRTG = ts.metrics["DRTG"];
                ASTp = ts.metrics["AST%"];
                DREBp = ts.metrics["DREB%"];
                EFGp = ts.metrics["EFG%"];
                EFFd = ts.metrics["EFFd"];
                TOR = ts.metrics["TOR"];
                OREBp = ts.metrics["OREB%"];
                FTR = ts.metrics["FTR"];
                PWp = ts.metrics["PW%"];
                TSp = ts.metrics["TS%"];
                TPR = ts.metrics["3PR"];
                PythW = ts.metrics["PythW"];
                PythL = ts.metrics["PythL"];
            }
            else
            {
                Games = ts.getPlayoffGames();
                Wins = ts.pl_winloss[0];
                Losses = ts.pl_winloss[1];
                MINS = ts.pl_stats[t.MINS];
                PF = ts.pl_stats[t.PF];
                PA = ts.pl_stats[t.PA];
                FGM = ts.pl_stats[t.FGM];
                FGMPG = ((float) FGM/Games);
                FGA = ts.pl_stats[t.FGA];
                FGAPG = ((float) FGA/Games);
                TPM = ts.pl_stats[t.TPM];
                TPMPG = ((float) TPM/Games);
                TPA = ts.pl_stats[t.TPA];
                TPAPG = ((float) TPA/Games);
                FTM = ts.pl_stats[t.FTM];
                FTMPG = ((float) FTM/Games);
                FTA = ts.pl_stats[t.FTA];
                FTAPG = ((float) FTA/Games);
                OREB = ts.pl_stats[t.OREB];
                DREB = ts.pl_stats[t.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ts.pl_stats[t.STL];
                TOS = ts.pl_stats[t.TOS];
                BLK = ts.pl_stats[t.BLK];
                AST = ts.pl_stats[t.AST];
                FOUL = ts.pl_stats[t.FOUL];

                Wp = ts.pl_averages[t.Wp];
                Weff = ts.pl_averages[t.Weff];
                PPG = ts.pl_averages[t.PPG];
                PAPG = ts.pl_averages[t.PAPG];
                FGp = ts.pl_averages[t.FGp];
                FGeff = ts.pl_averages[t.FGeff];
                TPp = ts.pl_averages[t.TPp];
                TPeff = ts.pl_averages[t.TPeff];
                FTp = ts.pl_averages[t.FTp];
                FTeff = ts.pl_averages[t.FTeff];
                RPG = ts.pl_averages[t.RPG];
                ORPG = ts.pl_averages[t.ORPG];
                DRPG = ts.pl_averages[t.DRPG];
                SPG = ts.pl_averages[t.SPG];
                TPG = ts.pl_averages[t.TPG];
                BPG = ts.pl_averages[t.BPG];
                APG = ts.pl_averages[t.APG];
                FPG = ts.pl_averages[t.FPG];

                Poss = ts.pl_metrics["PossPG"];
                Pace = ts.pl_metrics["Pace"];
                ORTG = ts.pl_metrics["ORTG"];
                DRTG = ts.pl_metrics["DRTG"];
                ASTp = ts.pl_metrics["AST%"];
                DREBp = ts.pl_metrics["DREB%"];
                EFGp = ts.pl_metrics["EFG%"];
                EFFd = ts.pl_metrics["EFFd"];
                TOR = ts.pl_metrics["TOR"];
                OREBp = ts.pl_metrics["OREB%"];
                FTR = ts.pl_metrics["FTR"];
                PWp = ts.pl_metrics["PW%"];
                TSp = ts.pl_metrics["TS%"];
                TPR = ts.pl_metrics["3PR"];
                PythW = ts.pl_metrics["PythW"];
                PythL = ts.pl_metrics["PythL"];
            }

            CurStreak = ts.CurStreak;
        }

        public TeamStatsRow(TeamStats ts, Dictionary<int, PlayerStats> pst, bool playoffs = false) : this(ts, playoffs)
        {
            CalculateTotalContracts(pst);
            CalculatePlayerCounts(pst);
        }

        public TeamStatsRow(TeamStats ts, Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats,
                            bool playoffs = false) : this(ts, playoffs)
        {
            DivW = splitTeamStats[ID]["Division"].winloss[0];
            DivL = splitTeamStats[ID]["Division"].winloss[1];
            ConfW = splitTeamStats[ID]["Conference"].winloss[0];
            ConfL = splitTeamStats[ID]["Conference"].winloss[1];
            L10W = splitTeamStats[ID]["Last 10"].winloss[0];
            L10L = splitTeamStats[ID]["Last 10"].winloss[1];
        }

        public TeamStatsRow(TeamStats ts, Dictionary<int, PlayerStats> pst, Dictionary<int, Dictionary<string, TeamStats>> splitTeamStats,
                            bool playoffs = false) : this(ts, splitTeamStats, playoffs)
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
        public string DivRecord { get { return String.Format("{0}-{1}", DivW, DivL); } }
        public uint ConfW { get; set; }
        public uint ConfL { get; set; }
        public string ConfRecord { get { return String.Format("{0}-{1}", ConfW, ConfL); } }
        public uint L10W { get; set; }
        public uint L10L { get; set; }
        public string L10Record { get { return String.Format("{0}-{1}", L10W, L10L); } }
        public string CurStreak { get; set; }

        public bool IsHidden { get; set; }

        public bool Highlight { get; set; }

        public static void TryChangeTSR(ref TeamStatsRow tsr, Dictionary<string, string> dict)
        {
            tsr.Wins = tsr.Wins.TrySetValue(dict, "Wins", typeof (UInt16));
            tsr.Losses = tsr.Losses.TrySetValue(dict, "Losses", typeof (UInt16));
            tsr.MINS = tsr.MINS.TrySetValue(dict, "MINS", typeof (UInt16));
            tsr.PF = tsr.PF.TrySetValue(dict, "PF", typeof (UInt16));
            tsr.PA = tsr.PF.TrySetValue(dict, "PA", typeof (UInt16));
            tsr.FGM = tsr.FGM.TrySetValue(dict, "FGM", typeof (UInt16));
            tsr.FGA = tsr.FGA.TrySetValue(dict, "FGA", typeof (UInt16));
            tsr.TPM = tsr.TPM.TrySetValue(dict, "3PM", typeof (UInt16));
            tsr.TPA = tsr.TPA.TrySetValue(dict, "3PA", typeof (UInt16));
            tsr.FTM = tsr.FTM.TrySetValue(dict, "FTM", typeof (UInt16));
            tsr.FTA = tsr.FTA.TrySetValue(dict, "FTA", typeof (UInt16));
            tsr.REB = tsr.REB.TrySetValue(dict, "REB", typeof (UInt16));
            tsr.OREB = tsr.OREB.TrySetValue(dict, "OREB", typeof (UInt16));
            tsr.DREB = tsr.DREB.TrySetValue(dict, "DREB", typeof (UInt16));
            tsr.AST = tsr.AST.TrySetValue(dict, "AST", typeof (UInt16));
            tsr.TOS = tsr.TOS.TrySetValue(dict, "TO", typeof (UInt16));
            tsr.STL = tsr.STL.TrySetValue(dict, "STL", typeof (UInt16));
            tsr.BLK = tsr.BLK.TrySetValue(dict, "BLK", typeof (UInt16));
            tsr.FOUL = tsr.FOUL.TrySetValue(dict, "FOUL", typeof (UInt16));
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

        public int InjuredCount { get; set; }
    }
}