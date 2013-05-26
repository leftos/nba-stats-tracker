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

namespace NBA_Stats_Tracker.Data.Players
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.Players.Contracts;
    using NBA_Stats_Tracker.Data.Players.Injuries;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Windows.MainInterface;

    #endregion

    /// <summary>Implements an easily bindable interface to a player's stats.</summary>
    public class PlayerStatsRow : INotifyPropertyChanged
    {
        private PlayerInjury _injury;

        public PlayerStatsRow()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the interface provided will show playoff stats.
        /// </param>
        /// <param name="calcRatings">Whether to calculate the player's NBA 2K ratings.</param>
        /// <param name="teamName">A custom team name.</param>
        public PlayerStatsRow(PlayerStats ps, bool playoffs = false, bool calcRatings = true, string teamName = null)
            : this()
        {
            LastName = ps.LastName;
            FirstName = ps.FirstName;

            ID = ps.ID;
            Position1 = ps.Position1;
            Position2 = ps.Position2;
            TeamF = ps.TeamF;
            TeamFDisplay = teamName ?? MainWindow.DisplayNames[TeamF];
            TeamS = ps.TeamS;
            if (TeamS == -1)
            {
                TeamSDisplay = "";
            }
            else
            {
                try
                {
                    TeamSDisplay = MainWindow.DisplayNames[TeamS];
                }
                catch (KeyNotFoundException)
                {
                    TeamSDisplay = "Unknown";
                }
            }
            IsActive = ps.IsActive;
            IsHidden = ps.IsHidden;
            IsAllStar = ps.IsAllStar;
            Injury = ps.Injury.DeepClone(null);
            IsNBAChampion = ps.IsNBAChampion;
            YearOfBirth = ps.YearOfBirth;
            YearsPro = ps.YearsPro;

            ContractOption = ps.Contract.Option;
            for (var i = 1; i <= 7; i++)
            {
                typeof(PlayerStatsRow).GetProperty("ContractY" + i).SetValue(this, ps.Contract.TryGetSalary(i), null);
            }
            ContractYears = ps.Contract.GetYears();
            ContractYearsMinusOption = ps.Contract.GetYearsMinusOption();

            Custom = new List<double>();

            Height = ps.Height;
            Weight = ps.Weight;

            if (!playoffs)
            {
                GP = ps.Totals[PAbbr.GP];
                GS = ps.Totals[PAbbr.GS];
                MINS = ps.Totals[PAbbr.MINS];
                PTS = ps.Totals[PAbbr.PTS];
                FGM = ps.Totals[PAbbr.FGM];
                FGMPG = ((float) FGM / GP);
                FGA = ps.Totals[PAbbr.FGA];
                FGAPG = ((float) FGA / GP);
                TPM = ps.Totals[PAbbr.TPM];
                TPMPG = ((float) TPM / GP);
                TPA = ps.Totals[PAbbr.TPA];
                TPAPG = ((float) TPA / GP);
                FTM = ps.Totals[PAbbr.FTM];
                FTMPG = ((float) FTM / GP);
                FTA = ps.Totals[PAbbr.FTA];
                FTAPG = ((float) FTA / GP);
                OREB = ps.Totals[PAbbr.OREB];
                DREB = ps.Totals[PAbbr.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ps.Totals[PAbbr.STL];
                TOS = ps.Totals[PAbbr.TOS];
                BLK = ps.Totals[PAbbr.BLK];
                AST = ps.Totals[PAbbr.AST];
                FOUL = ps.Totals[PAbbr.FOUL];

                MPG = ps.PerGame[PAbbr.MPG];
                PPG = ps.PerGame[PAbbr.PPG];
                FGp = ps.PerGame[PAbbr.FGp];
                FGeff = ps.PerGame[PAbbr.FGeff];
                TPp = ps.PerGame[PAbbr.TPp];
                TPeff = ps.PerGame[PAbbr.TPeff];
                FTp = ps.PerGame[PAbbr.FTp];
                FTeff = ps.PerGame[PAbbr.FTeff];
                RPG = ps.PerGame[PAbbr.RPG];
                ORPG = ps.PerGame[PAbbr.ORPG];
                DRPG = ps.PerGame[PAbbr.DRPG];
                SPG = ps.PerGame[PAbbr.SPG];
                TPG = ps.PerGame[PAbbr.TPG];
                BPG = ps.PerGame[PAbbr.BPG];
                APG = ps.PerGame[PAbbr.APG];
                FPG = ps.PerGame[PAbbr.FPG];

                try
                {
                    PTSR = ps.Metrics["PTSR"];
                    REBR = ps.Metrics["REBR"];
                    OREBR = ps.Metrics["OREBR"];
                    ASTR = ps.Metrics["ASTR"];
                    BLKR = ps.Metrics["BLKR"];
                    STLR = ps.Metrics["STLR"];
                    TOR = ps.Metrics["TOR"];
                    FTR = ps.Metrics["FTR"];
                    FTAR = ps.Metrics["FTAR"];
                    GmSc = ps.Metrics["GmSc"];
                    GmScE = ps.Metrics["GmScE"];
                    EFF = ps.Metrics["EFF"];
                    EFGp = ps.Metrics["EFG%"];
                    TSp = ps.Metrics["TS%"];
                    ASTp = ps.Metrics["AST%"];
                    STLp = ps.Metrics["STL%"];
                    TOp = ps.Metrics["TO%"];
                    USGp = ps.Metrics["USG%"];

                    try
                    {
                        PER = ps.Metrics["PER"];
                    }
                    catch (Exception)
                    {
                        PER = Double.NaN;
                    }

                    BLKp = ps.Metrics["BLK%"];
                    DREBp = ps.Metrics["DREB%"];
                    OREBp = ps.Metrics["OREB%"];
                    REBp = ps.Metrics["REB%"];
                    PPR = ps.Metrics["PPR"];
                }
                catch (KeyNotFoundException)
                {
                }
            }
            else
            {
                GP = ps.PlTotals[PAbbr.GP];
                GS = ps.PlTotals[PAbbr.GS];
                MINS = ps.PlTotals[PAbbr.MINS];
                PTS = ps.PlTotals[PAbbr.PTS];
                FGM = ps.PlTotals[PAbbr.FGM];
                FGMPG = ((float) FGM / GP);
                FGA = ps.PlTotals[PAbbr.FGA];
                FGAPG = ((float) FGA / GP);
                TPM = ps.PlTotals[PAbbr.TPM];
                TPMPG = ((float) TPM / GP);
                TPA = ps.PlTotals[PAbbr.TPA];
                TPAPG = (uint) ((double) TPA / GP);
                FTM = ps.PlTotals[PAbbr.FTM];
                FTMPG = ((float) FTM / GP);
                FTA = ps.PlTotals[PAbbr.FTA];
                FTAPG = ((float) FTA / GP);
                OREB = ps.PlTotals[PAbbr.OREB];
                DREB = ps.PlTotals[PAbbr.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ps.PlTotals[PAbbr.STL];
                TOS = ps.PlTotals[PAbbr.TOS];
                BLK = ps.PlTotals[PAbbr.BLK];
                AST = ps.PlTotals[PAbbr.AST];
                FOUL = ps.PlTotals[PAbbr.FOUL];

                MPG = ps.PlPerGame[PAbbr.MPG];
                PPG = ps.PlPerGame[PAbbr.PPG];
                FGp = ps.PlPerGame[PAbbr.FGp];
                FGeff = ps.PlPerGame[PAbbr.FGeff];
                TPp = ps.PlPerGame[PAbbr.TPp];
                TPeff = ps.PlPerGame[PAbbr.TPeff];
                FTp = ps.PlPerGame[PAbbr.FTp];
                FTeff = ps.PlPerGame[PAbbr.FTeff];
                RPG = ps.PlPerGame[PAbbr.RPG];
                ORPG = ps.PlPerGame[PAbbr.ORPG];
                DRPG = ps.PlPerGame[PAbbr.DRPG];
                SPG = ps.PlPerGame[PAbbr.SPG];
                TPG = ps.PlPerGame[PAbbr.TPG];
                BPG = ps.PlPerGame[PAbbr.BPG];
                APG = ps.PlPerGame[PAbbr.APG];
                FPG = ps.PlPerGame[PAbbr.FPG];

                try
                {
                    PTSR = ps.PlMetrics["PTSR"];
                    REBR = ps.PlMetrics["REBR"];
                    OREBR = ps.PlMetrics["OREBR"];
                    ASTR = ps.PlMetrics["ASTR"];
                    BLKR = ps.PlMetrics["BLKR"];
                    STLR = ps.PlMetrics["STLR"];
                    TOR = ps.PlMetrics["TOR"];
                    FTR = ps.PlMetrics["FTR"];
                    FTAR = ps.PlMetrics["FTAR"];
                    GmSc = ps.PlMetrics["GmSc"];
                    GmScE = ps.PlMetrics["GmScE"];
                    EFF = ps.PlMetrics["EFF"];
                    EFGp = ps.PlMetrics["EFG%"];
                    TSp = ps.PlMetrics["TS%"];
                    ASTp = ps.PlMetrics["AST%"];
                    STLp = ps.PlMetrics["STL%"];
                    TOp = ps.PlMetrics["TO%"];
                    USGp = ps.PlMetrics["USG%"];

                    try
                    {
                        PER = ps.PlMetrics["PER"];
                    }
                    catch (Exception)
                    {
                        PER = Double.NaN;
                    }

                    BLKp = ps.PlMetrics["BLK%"];
                    DREBp = ps.PlMetrics["DREB%"];
                    OREBp = ps.PlMetrics["OREB%"];
                    REBp = ps.PlMetrics["REB%"];
                    PPR = ps.PlMetrics["PPR"];
                }
                catch (KeyNotFoundException)
                {
                }
            }
            if (calcRatings)
            {
                calculate2KRatings(playoffs);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="type">The type.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the interface provided will show playoff stats.
        /// </param>
        public PlayerStatsRow(PlayerStats ps, string type, bool playoffs = false)
            : this(ps, playoffs)
        {
            Type = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="type">The type.</param>
        /// <param name="group">The group.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the interface provided will show playoff stats.
        /// </param>
        public PlayerStatsRow(PlayerStats ps, string type, string group, bool playoffs = false)
            : this(ps, type, playoffs)
        {
            Type = type;
            Group = group;
        }

        public int ContractYears { get; set; }
        public int ContractYearsMinusOption { get; set; }

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

        public float MPG { get; set; }
        public float PPG { get; set; }
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

        public double EFF { get; set; }
        public double GmSc { get; set; }
        public double GmScE { get; set; }
        public double EFGp { get; set; }
        public double TSp { get; set; }
        public double ASTp { get; set; }
        public double STLp { get; set; }
        public double TOp { get; set; }
        public double USGp { get; set; }
        public double PTSR { get; set; }
        public double REBR { get; set; }
        public double OREBR { get; set; }
        public double ASTR { get; set; }
        public double BLKR { get; set; }
        public double STLR { get; set; }
        public double TOR { get; set; }
        public double FTR { get; set; }
        public double FTAR { get; set; }

        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public Position Position1 { get; set; }
        public Position Position2 { get; set; }

        public bool Highlight { get; set; }

        public string Position1S
        {
            get { return PlayerStats.PositionToString(Position1); }
        }

        public string Position2S
        {
            get { return PlayerStats.PositionToString(Position2); }
        }

        public int TeamF { get; set; }
        public string TeamFDisplay { get; set; }
        public int TeamS { get; set; }
        public bool IsHidden { get; set; }
        public bool IsAllStar { get; set; }

        public PlayerInjury Injury
        {
            get { return _injury; }
            set
            {
                _injury = value;
                OnPropertyChanged("Injury");
                OnPropertyChanged("InjuryName");
                OnPropertyChanged("InjuryApproxDaysLeft");
                OnPropertyChanged("InjuryDaysLeft");
                OnPropertyChanged("InjuryStatus");
                OnPropertyChanged("IsInjured");
            }
        }

        public string InjuryName
        {
            get { return Injury.InjuryName; }
        }

        public string InjuryApproxDaysLeft
        {
            get { return Injury.ApproximateDays; }
        }

        public bool IsInjured
        {
            get { return Injury.IsInjured; }
        }

        public int InjuryDaysLeft
        {
            get { return Injury.InjuryDaysLeft; }
        }

        public string InjuryStatus
        {
            get { return Injury.Status; }
        }

        public bool IsNBAChampion { get; set; }

        public int YearOfBirth { get; set; }
        public int YearsPro { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }

        public string Type { get; set; }
        public string Group { get; set; }

        public int reRFT { get; set; }
        public int reRPass { get; set; }
        public int reRBlock { get; set; }
        public int reRSteal { get; set; }
        public int reROffRbd { get; set; }
        public int reRDefRbd { get; set; }
        public int reTShotTnd { get; set; }
        public int reTDrawFoul { get; set; }
        public int reTTouch { get; set; }
        public int reTCommitFl { get; set; }

        public int ContractY1 { get; set; }
        public int ContractY2 { get; set; }
        public int ContractY3 { get; set; }
        public int ContractY4 { get; set; }
        public int ContractY5 { get; set; }
        public int ContractY6 { get; set; }
        public int ContractY7 { get; set; }
        public PlayerContractOption ContractOption { get; set; }

        public string DisplayHeight
        {
            get
            {
                if (!MainWindow.IsImperial)
                {
                    return Height.ToString("0");
                }
                else
                {
                    var allInches = Height * 0.393701;
                    var feet = Convert.ToInt32(Math.Floor(allInches / 12));
                    var inches = Convert.ToInt32(allInches) % 12;
                    return String.Format("{0}\'{1}\"", feet, inches);
                }
            }
            set
            {
                if (!MainWindow.IsImperial)
                {
                    try
                    {
                        Height = Convert.ToDouble(value);
                    }
                    catch
                    {
                        MessageBox.Show(value + " is not a proper value for metric height.");
                    }
                }
                else
                {
                    try
                    {
                        Height = ConvertImperialHeightToMetric(value);
                    }
                    catch
                    {
                        MessageBox.Show(value + " is not a proper value for imperial height.");
                    }
                }
                OnPropertyChanged("DisplayHeight");
                OnPropertyChanged("Height");
            }
        }

        public string DisplayWeight
        {
            get
            {
                if (MainWindow.IsImperial)
                {
                    return Weight.ToString("F2");
                }
                else
                {
                    return (Weight * 0.453592).ToString("F2");
                }
            }
            set
            {
                if (MainWindow.IsImperial)
                {
                    try
                    {
                        Weight = Convert.ToDouble(value);
                    }
                    catch
                    {
                        MessageBox.Show(value + " is not a proper value for imperial weight.");
                    }
                }
                else
                {
                    try
                    {
                        Weight = ConvertMetricWeightToImperial(value);
                    }
                    catch
                    {
                        MessageBox.Show(value + " is not a proper value for metric weight.");
                    }
                }
                OnPropertyChanged("DisplayWeight");
                OnPropertyChanged("Weight");
            }
        }

        public bool IsActive { get; set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public static double ConvertImperialHeightToMetric(string value)
        {
            try
            {
                var parts = value.Split('\'');
                if (parts.Length != 2)
                {
                    throw new Exception("Tried to split imperial height string, got " + parts.Length + " parts instead of 2.");
                }
                parts[1] = parts[1].Replace("\"", "");
                var allInches = Convert.ToInt32(parts[0]) * 12 + Convert.ToInt32(parts[1]);
                return (allInches) / 0.393701;
            }
            catch
            {
                throw new Exception(value + " is not a proper value for imperial height.");
            }
        }

        public static double ConvertMetricWeightToImperial(string value)
        {
            try
            {
                return ConvertMetricWeightToImperial(Convert.ToDouble(value));
            }
            catch (FormatException)
            {
                throw new FormatException(value + " is not a proper value for metric weight.");
            }
        }

        public static double ConvertMetricWeightToImperial(double value)
        {
            return value / 0.453592;
        }

        private void calculate2KRatings(bool playoffs = false)
        {
            var gpPctSetting = MainWindow.RatingsGPPctSetting;
            var gpPCTreq = MainWindow.RatingsGPPctRequired;
            var mpgSetting = MainWindow.RatingsMPGSetting;
            var MPGreq = MainWindow.RatingsMPGRequired;

            var pGP = GP;
            var team = new TeamStats();
            uint tGP = 0;
            try
            {
                team = MainWindow.TST.Single(ts => ts.Value.ID == TeamF).Value;
                tGP = playoffs ? team.GetPlayoffGames() : team.GetGames();
            }
            catch (InvalidOperationException)
            {
                gpPctSetting = "-1";
            }

            if ((gpPctSetting != "-1" && (double) (pGP * 100) / tGP < gpPCTreq) || (mpgSetting != "-1" && MPG < MPGreq))
            {
                reRFT = -1;
                reRPass = -1;
                reRBlock = -1;
                reRSteal = -1;
                reROffRbd = -1;
                reRDefRbd = -1;
                reTShotTnd = -1;
                reTDrawFoul = -1;
                reTTouch = -1;
                reTCommitFl = -1;
                return;
            }

            try
            {
                reRFT = Convert.ToInt32(100 * FTp);
                if (reRFT > 99)
                {
                    reRFT = 99;
                }
            }
            catch
            {
                reRFT = -1;
            }

            try
            {
                var ASTp100 = ASTp * 100;
                reRPass =
                    Convert.ToInt32(31.1901795687457 + 1.36501096444891 * ASTp100 + 4.34894327991171 / (-0.702541953738967 - ASTp100));
                if (reRPass > 99)
                {
                    reRPass = 99;
                }
            }
            catch
            {
                reRPass = -1;
            }

            try
            {
                var BLKp100 = BLKp * 100;
                reRBlock =
                    Convert.ToInt32(
                        25.76 + 17.03 * BLKp100 + 0.8376 * Math.Pow(BLKp100, 3) - 3.195 * Math.Pow(BLKp100, 2)
                        - 0.07319 * Math.Pow(BLKp100, 4));
                if (reRBlock > 99)
                {
                    reRBlock = 99;
                }
            }
            catch
            {
                reRBlock = -1;
            }

            try
            {
                var STLp100 = STLp * 100;
                reRSteal = Convert.ToInt32(29.92 + 14.57 * STLp100 - 0.1509 * Math.Pow(STLp100, 2));
                if (reRSteal > 99)
                {
                    reRSteal = 99;
                }
            }
            catch
            {
                reRSteal = -1;
            }

            try
            {
                var OREBp100 = OREBp * 100;
                reROffRbd =
                    Convert.ToInt32(
                        24.67 + 3.864 * OREBp100 + 0.3523 * Math.Pow(OREBp100, 2) + 0.0007358 * Math.Pow(OREBp100, 4)
                        - 0.02796 * Math.Pow(OREBp100, 3));
                if (reROffRbd > 99)
                {
                    reROffRbd = 99;
                }
            }
            catch
            {
                reROffRbd = -1;
            }

            try
            {
                var DREBp100 = DREBp * 100;
                reRDefRbd = Convert.ToInt32(25 + 2.5 * DREBp100);
                if (reRDefRbd > 99)
                {
                    reRDefRbd = 99;
                }
            }
            catch
            {
                reRDefRbd = -1;
            }

            try
            {
                reTShotTnd = Convert.ToInt32(2 + 4 * FGAPG);
                if (reTShotTnd > 90)
                {
                    reTShotTnd = 90;
                }
            }
            catch
            {
                reTShotTnd = -1;
            }

            try
            {
                reTDrawFoul = Convert.ToInt32(FTAR * 10);
                if (reTDrawFoul > 99)
                {
                    reTDrawFoul = 99;
                }
            }
            catch
            {
                reTDrawFoul = -1;
            }

            try
            {
                var FGAR = (double) FGA / MINS * 36;
                var touchTotal = Convert.ToInt32(FGAR + FTAR + TOR + ASTR);
                reTTouch = Convert.ToInt32(3.141 * Math.Pow(touchTotal, 2) / (1.178 + touchTotal));
                if (reTTouch > 99)
                {
                    reTTouch = 99;
                }
            }
            catch
            {
                reTTouch = -1;
            }

            try
            {
                reTCommitFl = Convert.ToInt32((double) FOUL / MINS * 36 * 10);
                if (reTCommitFl > 99)
                {
                    reTCommitFl = 99;
                }
            }
            catch
            {
                reTCommitFl = -1;
            }
        }

        /// <summary>Gets the best stats.</summary>
        /// <param name="count">The count of stats to return.</param>
        /// <returns>A well-formatted multi-line string presenting the best stats.</returns>
        public string GetBestStats(int count)
        {
            if (GP == 0)
            {
                return "";
            }

            var position = Position1;
            double fgn = 0, tpn = 0, ftn = 0, ftrn = 0;
            var statsn = new Dictionary<string, double>();

            double fgfactor, tpfactor, ftfactor, orebfactor, rebfactor, astfactor, stlfactor, blkfactor, ptsfactor, ftrfactor;

            PlayerBoxScore.GetFactors(
                position,
                out fgfactor,
                out tpfactor,
                out ftfactor,
                out orebfactor,
                out rebfactor,
                out astfactor,
                out stlfactor,
                out blkfactor,
                out ptsfactor,
                out ftrfactor);

            if (FGM / GP > 4)
            {
                fgn = FGp / fgfactor;
            }
            statsn.Add("fgn", fgn);

            if (TPM / GP > 2)
            {
                tpn = TPp / tpfactor;
            }
            statsn.Add("tpn", tpn);

            if (FTM / GP > 3)
            {
                ftn = FTp / ftfactor;
            }
            statsn.Add("ftn", ftn);

            var orebn = ORPG / orebfactor;
            statsn.Add("orebn", orebn);

            /*
            drebn = (REB-OREB)/6.348;
            statsn.Add("drebn", drebn);
            */

            var rebn = RPG / rebfactor;
            statsn.Add("rebn", rebn);

            var astn = APG / astfactor;
            statsn.Add("astn", astn);

            var stln = SPG / stlfactor;
            statsn.Add("stln", stln);

            var blkn = BPG / blkfactor;
            statsn.Add("blkn", blkn);

            var ptsn = PPG / ptsfactor;
            statsn.Add("ptsn", ptsn);

            if (FTM / GP > 3)
            {
                ftrn = ((double) FTM / FGA) / ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            var items = from k in statsn.Keys orderby statsn[k] descending select k;

            var s = "";
            var i = 1;
            s += String.Format("PPG: {0:F1}\n", PPG);
            foreach (var item in items)
            {
                if (i == count)
                {
                    break;
                }

                switch (item)
                {
                    case "fgn":
                        s += String.Format("FG: {0:F1}-{1:F1} ({2:F3})\n", (double) FGM / GP, (double) FGA / GP, FGp);
                        break;

                    case "tpn":
                        s += String.Format("3P: {0:F1}-{1:F1} ({2:F3})\n", (double) TPM / GP, (double) TPA / GP, TPp);
                        break;

                    case "ftn":
                        s += String.Format("FT: {0:F1}-{1:F1} ({2:F3})\n", (double) FTM / GP, (double) FTA / GP, FTp);
                        break;

                    case "orebn":
                        s += String.Format("ORPG: {0:F1}\n", ORPG);
                        break;

                        /*
                case "drebn":
                    s += String.Format("DREB: {0}\n", REB - OREB);
                    break;
                */

                    case "rebn":
                        s += String.Format("RPG: {0:F1}\n", RPG);
                        break;

                    case "astn":
                        s += String.Format("APG: {0:F1}\n", APG);
                        break;

                    case "stln":
                        s += String.Format("SPG: {0:F1}\n", SPG);
                        break;

                    case "blkn":
                        s += String.Format("BPG: {0:F1}\n", BPG);
                        break;

                    case "ptsn":
                        continue;

                    case "ftrn":
                        s += String.Format(
                            "FTM/FGA: {0:F1}-{1:F1} ({2:F3})\n", (double) FTM / GP, (double) FGA / GP, (double) FTM / FGA);
                        break;
                }

                i++;
            }
            return s;
        }

        /// <summary>Gets a list (dictionary) of the best stats.</summary>
        /// <param name="count">The count of stats to return.</param>
        /// <returns>A list (dictionary) of the best stats' names and values</returns>
        public Dictionary<string, string> GetBestStatsList(int count)
        {
            if (GP == 0)
            {
                return new Dictionary<string, string>();
            }

            var statList = new Dictionary<string, string>();
            var s = GetBestStats(count);
            var lines = s.Split('\n');
            for (var i = 1; i < count; i++)
            {
                var parts = lines[i].Split(new[] { ": " }, StringSplitOptions.None);
                statList.Add(parts[0], parts[1]);
            }
            return statList;
        }

        /// <summary>Shows a scouting report for the player in natural language.</summary>
        /// <param name="rankingsActive">The rankingsPerGame of currently active players.</param>
        /// <param name="rankingsTeam">The rankingsPerGame of the players in the same team.</param>
        /// <param name="rankingsPosition">The rankingsPerGame of the players in the same position.</param>
        /// <param name="pbsIList">The list of the player's available box scores.</param>
        /// <param name="bestGame">The well-formatted string from the player's best game.</param>
        public string ScoutingReport(
            PlayerRankings rankingsActive,
            PlayerRankings rankingsTeam,
            PlayerRankings rankingsPosition,
            IEnumerable<PlayerBoxScore> pbsIList,
            string bestGame)
        {
            var pbsList = pbsIList.ToList();
            var s = "";
            s += String.Format(
                "{0} {1}, born in {3} ({6} years old today), is a {4}{5} tall {2} ",
                FirstName,
                LastName,
                Position1,
                YearOfBirth,
                DisplayHeight,
                MainWindow.IsImperial ? "" : "cm.",
                DateTime.Today.Year - YearOfBirth);
            if (Position2 != Position.None)
            {
                s += String.Format("(alternatively {0})", Position2);
            }
            s += ", ";

            if (IsActive)
            {
                s += String.Format("who currently plays for the {0}.", TeamFDisplay);
            }
            else
            {
                s += String.Format("who is currently a Free Agent.");
            }

            s += String.Format(" He's been a pro for {0} year", YearsPro);
            if (YearsPro != 1)
            {
                s += "s";
            }
            s += ".";

            s += "\n\n";

            s += String.Format("He averages {0:F1} PPG on {1:F1} MPG, making for {2:F1} points per 36 minutes. ", PPG, MPG, PTSR);

            if (rankingsTeam.RankingsPerGame[ID][PAbbr.PPG] <= 3)
            {
                s += String.Format(
                    "One of the best scorers in the team, #{0} among his teammates. ", rankingsTeam.RankingsPerGame[ID][PAbbr.PPG]);
            }
            if (rankingsPosition.RankingsPerGame[ID][PAbbr.PPG] <= 10)
            {
                s +=
                    String.Format(
                        "His performance has got him to become one of the best at his position in scoring, #{0} among {1}'s. ",
                        rankingsPosition.RankingsPerGame[ID][PAbbr.PPG],
                        Position1);
            }
            if (rankingsActive.RankingsPerGame[ID][PAbbr.PPG] <= 20)
            {
                s += String.Format(
                    "He's actually one of the best in the league in scoring, rated #{0} overall. ",
                    rankingsActive.RankingsPerGame[ID][PAbbr.PPG]);
            }

            var statList = GetBestStatsList(5);

            s += "\n\n";

            foreach (var stat in statList)
            {
                switch (stat.Key)
                {
                    case "FG":
                        s +=
                            String.Format(
                                "Shooting, one of his main strengths. He's averaging {0} as far as field goals go. Percentage-wise, his performance "
                                + "ranks him at #{1} overall. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.FGp]);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.FGp] <= 3)
                        {
                            s += String.Format(
                                "Top from the floor in his team, ranks at #{0} ", rankingsTeam.RankingsPerGame[ID][PAbbr.FGp]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.FGp] <= 10)
                        {
                            s += String.Format(
                                "Definitely dominating among {0}'s on scoring percentage, ranked at #{1}. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.FGp]);
                        }
                        break;
                    case "3P":
                        s +=
                            String.Format(
                                "His 3-point shooting is another area of focus. His three-point shooting PerGame {0}. #{1} in the league in 3P%. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.TPp]);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.TPp] <= 3)
                        {
                            s += String.Format(
                                "One of the best guys from the arc in his team, ranks at #{0} ",
                                rankingsTeam.RankingsPerGame[ID][PAbbr.TPp]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.TPp] <= 10)
                        {
                            s += String.Format(
                                "Not many {0}'s do better than him, as he's ranked at #{1}. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.TPp]);
                        }
                        break;
                    case "FT":
                        s +=
                            String.Format(
                                "Take a look at his free throw stats: He's averaging {0} from the line, which "
                                + "ranks him at #{1} overall. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.FTp]);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.FTp] <= 3)
                        {
                            s +=
                                String.Format(
                                    "Coach might prefer him to get all the fouls late in the game, as he ranks #{0} in his team. ",
                                    rankingsTeam.RankingsPerGame[ID][PAbbr.FTp]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.FTp] <= 10)
                        {
                            s += String.Format(
                                "Most {0}'s in the league struggle to keep up with him, he's ranked at #{1}. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.FTp]);
                        }
                        break;
                    case "ORPG":
                        s +=
                            String.Format(
                                "Crashing the offensive glass, one of his main strengths. His average offensive boards per game are at {0}, which "
                                + "ranks him at #{1} overall. He grabs {2:F1} offensive rebounds every 36 minutes. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.ORPG],
                                OREBR);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.ORPG] <= 3)
                        {
                            s += String.Format(
                                "One of the main guys to worry about below your basket, #{0} in his team. ",
                                rankingsTeam.RankingsPerGame[ID][PAbbr.ORPG]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.ORPG] <= 10)
                        {
                            s += String.Format(
                                "He's ranked at #{1} among {0}'s in grabbing those second chance opportunities. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.ORPG]);
                        }
                        break;
                    case "RPG":
                        s +=
                            String.Format(
                                "He makes a point of crashing the boards. His RPG are at {0} ({2:F1} per 36 minutes), which "
                                + "ranks him at #{1} overall. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.RPG],
                                REBR);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.RPG] <= 3)
                        {
                            s += String.Format(
                                "One of the top rebounders in his team, #{0} actually. ", rankingsTeam.RankingsPerGame[ID][PAbbr.RPG]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.RPG] <= 10)
                        {
                            s += String.Format(
                                "He's ranked at #{1} among {0}'s in crashing the boards. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.RPG]);
                        }
                        break;
                    case "BPG":
                        s +=
                            String.Format(
                                "Keep him in mind when he's in your face. His BPG are at {0} ({2:F1} per 36 minutes), which "
                                + "ranks him at #{1} overall. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.BPG],
                                BLKR);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.BPG] <= 3)
                        {
                            s += String.Format(
                                "Among the top blockers in the team, ranked at #{0}. ", rankingsTeam.RankingsPerGame[ID][PAbbr.BPG]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.BPG] <= 10)
                        {
                            s += String.Format(
                                "One of the best {0}'s (#{1}) at blocking shots. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.BPG]);
                        }
                        break;
                    case "APG":
                        s +=
                            String.Format(
                                "Assisting the ball, an important aspect of his game. He does {0} APG ({2:F1} per 36 minutes), ranking him at #{1} overall. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.APG],
                                ASTR);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.APG] <= 3)
                        {
                            s += String.Format(
                                "#{0} as far as playmakers in the team go. ", rankingsTeam.RankingsPerGame[ID][PAbbr.APG]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.APG] <= 10)
                        {
                            s += String.Format(
                                "One of the league's best {0}'s (#{1}) at setting up teammates for a shot. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.APG]);
                        }
                        break;
                    case "SPG":
                        s +=
                            String.Format(
                                "Tries to keep his hands active; keep in mind his {0} SPG ({2:F1} per 36 minutes). His performance in taking the ball away has "
                                + "ranked him at #{1} in the league. ",
                                stat.Value,
                                rankingsActive.RankingsPerGame[ID][PAbbr.SPG],
                                STLR);
                        if (rankingsTeam.RankingsPerGame[ID][PAbbr.SPG] <= 3)
                        {
                            s += String.Format(
                                "#{0} in taking the ball away among his teammates. ", rankingsTeam.RankingsPerGame[ID][PAbbr.SPG]);
                        }
                        if (rankingsPosition.RankingsPerGame[ID][PAbbr.SPG] <= 10)
                        {
                            s += String.Format(
                                "One of the league's best {0}'s (#{1}) in this aspect. ",
                                Position1,
                                rankingsPosition.RankingsPerGame[ID][PAbbr.SPG]);
                        }
                        break;
                    case "FTM/FGA":
                        s += String.Format("He fights through contact to get to the line. His FTM/FGA rate is at {0}. ", stat.Value);
                        break;
                }
                s += "\n";
            }

            s +=
                String.Format(
                    "His foul rate is at {0:F1} per 36 minutes, while his turnover rate is at {1:F1} per the same duration.\n\n",
                    (double) FOUL / MINS * 36,
                    TOR);

            pbsList.Sort((pbs1, pbs2) => pbs1.RealDate.CompareTo(pbs2.RealDate));
            pbsList.Reverse();

            if (!String.IsNullOrWhiteSpace(bestGame))
            {
                var parts = bestGame.Split(new[] { ": ", " vs ", " (", "\n" }, StringSplitOptions.None);
                s += String.Format(
                    "His best game was at {0} against the {1}, with a Game Score of {2:F2} ",
                    parts[1],
                    parts[2],
                    pbsList.Find(pbs => pbs.RealDate == Convert.ToDateTime(parts[1])).GmSc);
                s += "(";
                for (var i = 5; i < parts.Length; i++)
                {
                    if (String.IsNullOrWhiteSpace(parts[i]))
                    {
                        break;
                    }

                    s += String.Format("{0} {1}", parts[i + 1], parts[i]);
                    if (parts[i + 2].Contains(")"))
                    {
                        s += String.Format(" ({0}, ", parts[i + 2]);
                        i += 2;
                    }
                    else
                    {
                        s += ", ";
                        i += 1;
                    }
                }
                s = s.TrimEnd(new[] { ',', ' ' });
                s += "). ";
            }

            if (pbsList.Count > 5)
            {
                double sum = 0;
                for (var i = 0; i < 5; i++)
                {
                    sum += pbsList[i].GmSc;
                }
                var average = sum / 5;
                s += String.Format("He's been averaging a Game Score of {0:F2} in his last 5 games, ", average);
                if (average > GmSc)
                {
                    s += String.Format("which can be considered an improvement compared to his season average of {0:F2}. ", GmSc);
                }
                else
                {
                    s += String.Format("which is lower than his season average of {0:F2}. ", GmSc);
                }
            }
            else if (pbsList.Count > 3)
            {
                double sum = 0;
                for (var i = 0; i < 3; i++)
                {
                    sum += pbsList[i].GmSc;
                }
                var average = sum / 3;
                s += String.Format("He's been averaging a Game Score of {0:F2} in his last 3 games, ", average);
                if (average > GmSc)
                {
                    s += String.Format("which can be considered an improvement compared to his season average of {0:F2}. ", GmSc);
                }
                else
                {
                    s += String.Format("which is lower than his season average of {0:F2}. ", GmSc);
                }
            }
            else
            {
                s += String.Format("He's been averaging a Game Score of {0:F2}. ", GmSc);
            }

            s += "\n\nAccording to his rankings in the league, his best areas are ";
            var dict = new Dictionary<int, int>();
            for (var k = 0; k < rankingsActive.RankingsPerGame[ID].Length; k++)
            {
                dict.Add(k, rankingsActive.RankingsPerGame[ID][k]);
            }
            var strengths = (from entry in dict orderby entry.Value ascending select entry.Key).ToList();
            var m = 0;
            var j = 3;
            while (true)
            {
                if (m == j)
                {
                    break;
                }
                switch (strengths[m])
                {
                    case PAbbr.APG:
                        s += String.Format("assists (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.APG], APG);
                        break;
                    case PAbbr.BPG:
                        s += String.Format("blocks (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.BPG], BPG);
                        break;
                    case PAbbr.DRPG:
                        s += String.Format("defensive rebounds (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.DRPG], DRPG);
                        break;
                    case PAbbr.FGeff:
                        s += String.Format(
                            "field goals (#{0}, {1:F1} per game on {2:F3}), ",
                            rankingsActive.RankingsPerGame[ID][PAbbr.FGeff],
                            (double) FGM / GP,
                            FGp);
                        break;
                    case PAbbr.FPG:
                        s += String.Format("fouls (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.FPG], FPG);
                        break;
                    case PAbbr.FTeff:
                        s += String.Format(
                            "free throws (#{0}, {1:F1} per game on {2:F3}), ",
                            rankingsActive.RankingsPerGame[ID][PAbbr.FTeff],
                            (double) FTM / GP,
                            FTp);
                        break;
                    case PAbbr.ORPG:
                        s += String.Format("offensive rebounds (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.ORPG], ORPG);
                        break;
                    case PAbbr.PPG:
                        s += String.Format("scoring (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.PPG], PPG);
                        break;
                    case PAbbr.RPG:
                        s += String.Format("rebounds (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.RPG], RPG);
                        break;
                    case PAbbr.SPG:
                        s += String.Format("steals (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.SPG], SPG);
                        break;
                    case PAbbr.TPG:
                        s += String.Format("turnovers (#{0}, {1:F1}), ", rankingsActive.RankingsPerGame[ID][PAbbr.TPG], TPG);
                        break;
                    case PAbbr.TPeff:
                        s += String.Format(
                            "three-pointers (#{0}, {1:F1} per game on {2:F3}), ",
                            rankingsActive.RankingsPerGame[ID][PAbbr.TPeff],
                            (double) TPM / GP,
                            TPp);
                        break;
                    default:
                        j++;
                        break;
                }
                m++;
            }
            s = s.TrimEnd(new[] { ' ', ',' });
            s += ".";

            return s;
        }

        /// <summary>Tries to parse the specified dictionary and update the specified PlayerStatsRow instance.</summary>
        /// <param name="psr">The PSR.</param>
        /// <param name="dict">The dict.</param>
        public static void TryChangePSR(ref PlayerStatsRow psr, Dictionary<string, string> dict)
        {
            psr.GP = psr.GP.TrySetValue(dict, "GP", typeof(UInt16));
            psr.GS = psr.GS.TrySetValue(dict, "GS", typeof(UInt16));
            psr.MINS = psr.MINS.TrySetValue(dict, "MINS", typeof(UInt16));
            psr.PTS = psr.PTS.TrySetValue(dict, "PTS", typeof(UInt16));
            psr.FGM = psr.FGM.TrySetValue(dict, "FGM", typeof(UInt16));
            psr.FGA = psr.FGA.TrySetValue(dict, "FGA", typeof(UInt16));
            psr.TPM = psr.TPM.TrySetValue(dict, "3PM", typeof(UInt16));
            psr.TPA = psr.TPA.TrySetValue(dict, "3PA", typeof(UInt16));
            psr.FTM = psr.FTM.TrySetValue(dict, "FTM", typeof(UInt16));
            psr.FTA = psr.FTA.TrySetValue(dict, "FTA", typeof(UInt16));
            psr.REB = psr.REB.TrySetValue(dict, "REB", typeof(UInt16));
            psr.OREB = psr.OREB.TrySetValue(dict, "OREB", typeof(UInt16));
            psr.DREB = psr.DREB.TrySetValue(dict, "DREB", typeof(UInt16));
            psr.AST = psr.AST.TrySetValue(dict, "AST", typeof(UInt16));
            psr.TOS = psr.TOS.TrySetValue(dict, "TO", typeof(UInt16));
            psr.STL = psr.STL.TrySetValue(dict, "STL", typeof(UInt16));
            psr.BLK = psr.BLK.TrySetValue(dict, "BLK", typeof(UInt16));
            psr.FOUL = psr.FOUL.TrySetValue(dict, "FOUL", typeof(UInt16));
        }

        public static void Refresh(ref PlayerStatsRow psr)
        {
            psr = new PlayerStatsRow(new PlayerStats(psr));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PlayerStatsRow ConvertToMyLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            var ts = teamStats[TeamF];
            var gamesTeam = (!playoffs) ? ts.GetGames() : ts.GetPlayoffGames();
            var gamesPlayer = GP;
            var newpsr = this.DeepClone();

            var gpPctSetting = MainWindow.MyLeadersGPPctSetting;
            var gpPctRequired = MainWindow.MyLeadersGPPctRequired;
            var mpgSetting = MainWindow.MyLeadersMPGSetting;
            var mpgRequired = MainWindow.MyLeadersMPGRequired;

            if ((gpPctSetting != "-1" && (double) gamesPlayer * 100 / gamesTeam < gpPctRequired)
                || (mpgSetting != "-1" && MPG < mpgRequired))
            {
                newpsr.PTS = 0;
                newpsr.FGM = 0;
                newpsr.FGA = 0;
                newpsr.TPM = 0;
                newpsr.TPA = 0;
                newpsr.FTM = 0;
                newpsr.FTA = 0;
                newpsr.REB = 0;
                newpsr.OREB = 0;
                newpsr.DREB = 0;
                newpsr.BLK = 0;
                newpsr.AST = 0;
                newpsr.TOS = uint.MaxValue;
                newpsr.STL = 0;
                newpsr.FOUL = uint.MaxValue;

                newpsr.FGp = float.NaN;
                newpsr.FGeff = float.NaN;
                newpsr.TPp = float.NaN;
                newpsr.TPeff = float.NaN;
                newpsr.FTp = float.NaN;
                newpsr.FTeff = float.NaN;
                newpsr.PPG = float.NaN;
                newpsr.RPG = float.NaN;
                newpsr.DRPG = float.NaN;
                newpsr.ORPG = float.NaN;
                newpsr.APG = float.NaN;
                newpsr.SPG = float.NaN;
                newpsr.BPG = float.NaN;

                newpsr.GmSc = double.NaN;
                newpsr.PTSR = double.NaN;
                newpsr.REBR = double.NaN;
                newpsr.OREBR = double.NaN;
                newpsr.ASTR = double.NaN;
                newpsr.BLKR = double.NaN;
                newpsr.STLR = double.NaN;
                newpsr.TOR = double.NaN;
                newpsr.FTR = double.NaN;
                newpsr.FTAR = double.NaN;
                newpsr.GmScE = double.NaN;
                newpsr.EFF = double.NaN;
                newpsr.EFGp = double.NaN;
                newpsr.TSp = double.NaN;
                newpsr.ASTp = double.NaN;
                newpsr.STLp = double.NaN;
                newpsr.TOp = double.NaN;
                newpsr.USGp = double.NaN;
                newpsr.PER = double.NaN;
                newpsr.BLKp = double.NaN;
                newpsr.DREBp = double.NaN;
                newpsr.OREBp = double.NaN;
                newpsr.REBp = double.NaN;
                newpsr.PPR = double.NaN;
            }

            return newpsr;
        }

        /// <summary>Edits a player's stats row to adjust for the rules and requirements of the NBA's League Leaders standings.</summary>
        /// <param name="teamStats">The player's team stats.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the playoff stats will be edited; otherwise, the regular season's.
        /// </param>
        /// <returns></returns>
        public PlayerStatsRow ConvertToLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            var ts = teamStats[TeamF];
            var gamesTeam = (!playoffs) ? ts.GetGames() : ts.GetPlayoffGames();
            var gamesPlayer = GP;
            var newpsr = this.DeepClone();

            // Below functions found using Eureqa II
            var gamesRequired = (int) Math.Ceiling(0.8522 * gamesTeam); // Maximum error of 0
            var fgmRequired = (int) Math.Ceiling(3.65 * gamesTeam); // Max error of 0
            var ftmRequired = (int) Math.Ceiling(1.52 * gamesTeam);
            var tpmRequired = (int) Math.Ceiling(0.666671427752402 * gamesTeam);
            var ptsRequired = (int) Math.Ceiling(17.07 * gamesTeam);
            var rebRequired = (int) Math.Ceiling(9.74720677727814 * gamesTeam);
            var astRequired = (int) Math.Ceiling(4.87 * gamesTeam);
            var stlRequired = (int) Math.Ceiling(1.51957078555763 * gamesTeam);
            var blkRequired = (int) Math.Ceiling(1.21 * gamesTeam);
            var minRequired = (int) Math.Ceiling(24.39 * gamesTeam);

            if (FGM < fgmRequired)
            {
                //newpsr.PTS = 0;
                newpsr.FGM = 0;
                newpsr.FGA = 0;
                newpsr.TSp = double.NaN;
                newpsr.EFGp = double.NaN;
                newpsr.GmSc = double.NaN;
                newpsr.GmScE = double.NaN;
                newpsr.PTSR = double.NaN;
                newpsr.EFF = double.NaN;
                newpsr.FTR = double.NaN;
                newpsr.FTAR = double.NaN;
                newpsr.USGp = double.NaN;
                newpsr.PER = double.NaN;
                newpsr.PPR = double.NaN;

                newpsr.FGp = float.NaN;
                newpsr.FGeff = float.NaN;
            }
            if (TPM < tpmRequired)
            {
                //newpsr.PTS = 0;
                newpsr.TPM = 0;
                newpsr.TPA = 0;
                newpsr.TSp = double.NaN;
                newpsr.EFGp = double.NaN;
                newpsr.GmSc = double.NaN;
                newpsr.GmScE = double.NaN;
                newpsr.PTSR = double.NaN;
                newpsr.EFF = double.NaN;
                newpsr.FTR = double.NaN;
                newpsr.FTAR = double.NaN;
                newpsr.USGp = double.NaN;
                newpsr.PER = double.NaN;
                newpsr.PPR = double.NaN;

                newpsr.TPp = float.NaN;
                newpsr.TPeff = float.NaN;
            }
            if (FTM < ftmRequired)
            {
                //newpsr.PTS = 0;
                newpsr.FTM = 0;
                newpsr.FTA = 0;
                newpsr.TSp = double.NaN;
                newpsr.GmSc = double.NaN;
                newpsr.GmScE = double.NaN;
                newpsr.PTSR = double.NaN;
                newpsr.EFF = double.NaN;
                newpsr.FTR = double.NaN;
                newpsr.FTAR = double.NaN;
                newpsr.USGp = double.NaN;
                newpsr.PER = double.NaN;
                newpsr.PPR = double.NaN;

                newpsr.FTp = float.NaN;
                newpsr.FTeff = float.NaN;
            }

            if (gamesPlayer >= gamesRequired)
            {
                return newpsr;
            }
            else
            {
                newpsr.GmSc = double.NaN;
                newpsr.GmScE = double.NaN;
                newpsr.EFF = double.NaN;
                newpsr.PER = double.NaN;
                newpsr.TOS = uint.MaxValue;
                newpsr.FOUL = uint.MaxValue;
                newpsr.TOR = double.NaN;
            }

            if (PTS < ptsRequired)
            {
                newpsr.PPG = float.NaN;
                newpsr.PTS = 0;
                newpsr.TSp = double.NaN;
                newpsr.EFGp = double.NaN;
                newpsr.PTSR = double.NaN;
                newpsr.USGp = double.NaN;
                newpsr.PPR = double.NaN;
            }
            if (REB < rebRequired)
            {
                newpsr.RPG = float.NaN;
                newpsr.DRPG = float.NaN;
                newpsr.ORPG = float.NaN;

                newpsr.REB = 0;
                newpsr.OREB = 0;
                newpsr.DREB = 0;
                newpsr.REBR = double.NaN;
                newpsr.OREBR = double.NaN;
                newpsr.DREBp = double.NaN;
                newpsr.OREBp = double.NaN;
                newpsr.REBp = double.NaN;
            }
            if (AST < astRequired)
            {
                newpsr.APG = float.NaN;
                newpsr.AST = 0;
                newpsr.ASTp = double.NaN;
                newpsr.ASTR = double.NaN;
                newpsr.USGp = double.NaN;
                newpsr.PPR = double.NaN;
            }
            if (STL < stlRequired)
            {
                newpsr.SPG = float.NaN;
                newpsr.STL = 0;
                newpsr.STLp = double.NaN;
                newpsr.STLR = double.NaN;
            }
            if (BLK < blkRequired)
            {
                newpsr.BPG = float.NaN;
                newpsr.BLK = 0;
                newpsr.BLKp = double.NaN;
                newpsr.BLKR = double.NaN;
            }
            if (MINS < minRequired)
            {
                newpsr.MPG = float.NaN;
            }
            return newpsr;
        }

        #region Metrics that require opponents' stats

        public double PER { get; set; }
        public double BLKp { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double REBp { get; set; }
        public double PPR { get; set; }

        public string TeamSDisplay { get; set; }

        public List<double> Custom { get; set; }

        public string FullNameGivenFirst
        {
            get
            {
                if (String.IsNullOrWhiteSpace(FirstName))
                {
                    return LastName;
                }

                if (String.IsNullOrWhiteSpace(LastName))
                {
                    return FirstName;
                }

                return FirstName + " " + LastName;
            }
        }

        public string FullName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(FirstName))
                {
                    return LastName;
                }

                if (String.IsNullOrWhiteSpace(LastName))
                {
                    return FirstName;
                }

                return LastName + ", " + FirstName;
            }
        }

        #endregion
    }
}