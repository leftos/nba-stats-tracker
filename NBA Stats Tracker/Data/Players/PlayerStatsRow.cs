using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Annotations;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     Implements an easily bindable interface to a player's stats.
    /// </summary>
    public class PlayerStatsRow : INotifyPropertyChanged
    {
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
        public PlayerStatsRow(PlayerStats ps, bool playoffs = false, bool calcRatings = true)
        {
            LastName = ps.LastName;
            FirstName = ps.FirstName;

            ID = ps.ID;
            Position1 = ps.Position1;
            Position2 = ps.Position2;
            TeamF = ps.TeamF;
            TeamS = ps.TeamS;
            isActive = ps.isActive;
            isHidden = ps.isHidden;
            isAllStar = ps.isAllStar;
            isInjured = ps.isInjured;
            isNBAChampion = ps.isNBAChampion;
            YearOfBirth = ps.YearOfBirth;
            YearsPro = ps.YearsPro;

            ContractOption = ps.contract.Option;
            for (int i = 1; i <= 7; i++)
            {
                typeof(PlayerStatsRow).GetProperty("ContractY" + i).SetValue(this, ps.contract.TryGetSalary(i), null);
            }

            Height = ps.height;
            Weight = ps.weight;
            
            if (!playoffs)
            {
                GP = ps.stats[p.GP];
                GS = ps.stats[p.GS];
                MINS = ps.stats[p.MINS];
                PTS = ps.stats[p.PTS];
                FGM = ps.stats[p.FGM];
                FGMPG = ((float) FGM/GP);
                FGA = ps.stats[p.FGA];
                FGAPG = ((float) FGA/GP);
                TPM = ps.stats[p.TPM];
                TPMPG = ((float) TPM/GP);
                TPA = ps.stats[p.TPA];
                TPAPG = ((float) TPA/GP);
                FTM = ps.stats[p.FTM];
                FTMPG = ((float) FTM/GP);
                FTA = ps.stats[p.FTA];
                FTAPG = ((float) FTA/GP);
                OREB = ps.stats[p.OREB];
                DREB = ps.stats[p.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ps.stats[p.STL];
                TOS = ps.stats[p.TOS];
                BLK = ps.stats[p.BLK];
                AST = ps.stats[p.AST];
                FOUL = ps.stats[p.FOUL];

                MPG = ps.averages[p.MPG];
                PPG = ps.averages[p.PPG];
                FGp = ps.averages[p.FGp];
                FGeff = ps.averages[p.FGeff];
                TPp = ps.averages[p.TPp];
                TPeff = ps.averages[p.TPeff];
                FTp = ps.averages[p.FTp];
                FTeff = ps.averages[p.FTeff];
                RPG = ps.averages[p.RPG];
                ORPG = ps.averages[p.ORPG];
                DRPG = ps.averages[p.DRPG];
                SPG = ps.averages[p.SPG];
                TPG = ps.averages[p.TPG];
                BPG = ps.averages[p.BPG];
                APG = ps.averages[p.APG];
                FPG = ps.averages[p.FPG];

                try
                {
                    PTSR = ps.metrics["PTSR"];
                    REBR = ps.metrics["REBR"];
                    OREBR = ps.metrics["OREBR"];
                    ASTR = ps.metrics["ASTR"];
                    BLKR = ps.metrics["BLKR"];
                    STLR = ps.metrics["STLR"];
                    TOR = ps.metrics["TOR"];
                    FTR = ps.metrics["FTR"];
                    FTAR = ps.metrics["FTAR"];
                    GmSc = ps.metrics["GmSc"];
                    GmScE = ps.metrics["GmScE"];
                    EFF = ps.metrics["EFF"];
                    EFGp = ps.metrics["EFG%"];
                    TSp = ps.metrics["TS%"];
                    ASTp = ps.metrics["AST%"];
                    STLp = ps.metrics["STL%"];
                    TOp = ps.metrics["TO%"];
                    USGp = ps.metrics["USG%"];

                    try
                    {
                        PER = ps.metrics["PER"];
                    }
                    catch (Exception)
                    {
                        PER = Double.NaN;
                    }

                    BLKp = ps.metrics["BLK%"];
                    DREBp = ps.metrics["DREB%"];
                    OREBp = ps.metrics["OREB%"];
                    REBp = ps.metrics["REB%"];
                    PPR = ps.metrics["PPR"];
                }
                catch (KeyNotFoundException)
                {
                }
            }
            else
            {
                GP = ps.pl_stats[p.GP];
                GS = ps.pl_stats[p.GS];
                MINS = ps.pl_stats[p.MINS];
                PTS = ps.pl_stats[p.PTS];
                FGM = ps.pl_stats[p.FGM];
                FGMPG = ((float) FGM/GP);
                FGA = ps.pl_stats[p.FGA];
                FGAPG = ((float) FGA/GP);
                TPM = ps.pl_stats[p.TPM];
                TPMPG = ((float) TPM/GP);
                TPA = ps.pl_stats[p.TPA];
                TPAPG = (uint) ((double) TPA/GP);
                FTM = ps.pl_stats[p.FTM];
                FTMPG = ((float) FTM/GP);
                FTA = ps.pl_stats[p.FTA];
                FTAPG = ((float) FTA/GP);
                OREB = ps.pl_stats[p.OREB];
                DREB = ps.pl_stats[p.DREB];
                REB = (UInt16) (OREB + DREB);
                STL = ps.pl_stats[p.STL];
                TOS = ps.pl_stats[p.TOS];
                BLK = ps.pl_stats[p.BLK];
                AST = ps.pl_stats[p.AST];
                FOUL = ps.pl_stats[p.FOUL];

                MPG = ps.pl_averages[p.MPG];
                PPG = ps.pl_averages[p.PPG];
                FGp = ps.pl_averages[p.FGp];
                FGeff = ps.pl_averages[p.FGeff];
                TPp = ps.pl_averages[p.TPp];
                TPeff = ps.pl_averages[p.TPeff];
                FTp = ps.pl_averages[p.FTp];
                FTeff = ps.pl_averages[p.FTeff];
                RPG = ps.pl_averages[p.RPG];
                ORPG = ps.pl_averages[p.ORPG];
                DRPG = ps.pl_averages[p.DRPG];
                SPG = ps.pl_averages[p.SPG];
                TPG = ps.pl_averages[p.TPG];
                BPG = ps.pl_averages[p.BPG];
                APG = ps.pl_averages[p.APG];
                FPG = ps.pl_averages[p.FPG];

                try
                {
                    PTSR = ps.pl_metrics["PTSR"];
                    REBR = ps.pl_metrics["REBR"];
                    OREBR = ps.pl_metrics["OREBR"];
                    ASTR = ps.pl_metrics["ASTR"];
                    BLKR = ps.pl_metrics["BLKR"];
                    STLR = ps.pl_metrics["STLR"];
                    TOR = ps.pl_metrics["TOR"];
                    FTR = ps.pl_metrics["FTR"];
                    FTAR = ps.pl_metrics["FTAR"];
                    GmSc = ps.pl_metrics["GmSc"];
                    GmScE = ps.pl_metrics["GmScE"];
                    EFF = ps.pl_metrics["EFF"];
                    EFGp = ps.pl_metrics["EFG%"];
                    TSp = ps.pl_metrics["TS%"];
                    ASTp = ps.pl_metrics["AST%"];
                    STLp = ps.pl_metrics["STL%"];
                    TOp = ps.pl_metrics["TO%"];
                    USGp = ps.pl_metrics["USG%"];

                    try
                    {
                        PER = ps.pl_metrics["PER"];
                    }
                    catch (Exception)
                    {
                        PER = Double.NaN;
                    }

                    BLKp = ps.pl_metrics["BLK%"];
                    DREBp = ps.pl_metrics["DREB%"];
                    OREBp = ps.pl_metrics["OREB%"];
                    REBp = ps.pl_metrics["REB%"];
                    PPR = ps.pl_metrics["PPR"];
                }
                catch (KeyNotFoundException)
                {
                }
            }
            if (calcRatings)
                Calculate2KRatings(playoffs);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStatsRow" /> class.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="type">The type.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the interface provided will show playoff stats.
        /// </param>
        public PlayerStatsRow(PlayerStats ps, string type, bool playoffs = false) : this(ps, playoffs)
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
        public PlayerStatsRow(PlayerStats ps, string type, string group, bool playoffs = false) : this(ps, type, playoffs)
        {
            Type = type;
            Group = group;
        }

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
        public string TeamF { get; set; }
        public string TeamFDisplay { get; set; }
        public string TeamS { get; set; }
        public bool isActive { get; set; }
        public bool isHidden { get; set; }
        public bool isAllStar { get; set; }
        public bool isInjured { get; set; }
        public bool isNBAChampion { get; set; }

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
                    var allInches = Height*0.393701;
                    int feet = Convert.ToInt32(Math.Floor(allInches/12));
                    int inches = Convert.ToInt32(allInches)%12;
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
                        var parts = value.Split('\'');
                        parts[1] = parts[1].Replace("\"", "");
                        var allInches = Convert.ToInt32(parts[0])*12 + Convert.ToInt32(parts[1]);
                        Height = ((double) allInches)/0.393701;
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
                    return (Weight*0.453592).ToString("F2");
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
                        Weight = Convert.ToDouble(value) / 0.453592;
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
        
        #region Metrics that require opponents' stats

        public double PER { get; set; }
        public double BLKp { get; set; }
        public double DREBp { get; set; }
        public double OREBp { get; set; }
        public double REBp { get; set; }
        public double PPR { get; set; }

        #endregion

        private void Calculate2KRatings(bool playoffs = false)
        {
            var gpPctSetting = MainWindow.RatingsGPPctSetting;
            var gpPCTreq = MainWindow.RatingsGPPctRequired;
            var mpgSetting = MainWindow.RatingsMPGSetting;
            var MPGreq = MainWindow.RatingsMPGRequired;

            var pGP = GP;
            TeamStats team = new TeamStats();
            uint tGP = 0;
            try
            {
                team = MainWindow.tst.Single(ts => ts.Value.name == TeamF).Value;
                tGP = playoffs ? team.getPlayoffGames() : team.getGames();
            }
            catch (InvalidOperationException)
            {
                gpPctSetting = "-1";
            }

            if ((gpPctSetting != "-1" && (double) (pGP*100)/tGP < gpPCTreq) || (mpgSetting != "-1" && MPG < MPGreq))
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
                reRFT = Convert.ToInt32(100*FTp);
                if (reRFT > 99)
                    reRFT = 99;
            }
            catch
            {
                reRFT = -1;
            }

            try
            {
                var ASTp100 = ASTp*100;
                reRPass = Convert.ToInt32(31.1901795687457 + 1.36501096444891*ASTp100 + 4.34894327991171/(-0.702541953738967 - ASTp100));
                if (reRPass > 99)
                    reRPass = 99;
            }
            catch
            {
                reRPass = -1;
            }

            try
            {
                var BLKp100 = BLKp*100;
                reRBlock =
                    Convert.ToInt32(25.76 + 17.03*BLKp100 + 0.8376*Math.Pow(BLKp100, 3) - 3.195*Math.Pow(BLKp100, 2) - 0.07319*Math.Pow(BLKp100, 4));
                if (reRBlock > 99)
                    reRBlock = 99;
            }
            catch
            {
                reRBlock = -1;
            }

            try
            {
                var STLp100 = STLp*100;
                reRSteal = Convert.ToInt32(29.92 + 14.57 * STLp100 - 0.1509 * Math.Pow(STLp100, 2));
                if (reRSteal > 99)
                    reRSteal = 99;
            }
            catch
            {
                reRSteal = -1;
            }

            try
            {
                var OREBp100 = OREBp*100;
                reROffRbd =
                    Convert.ToInt32(24.67 + 3.864*OREBp100 + 0.3523*Math.Pow(OREBp100, 2) + 0.0007358*Math.Pow(OREBp100, 4) -
                                    0.02796*Math.Pow(OREBp100, 3));
                if (reROffRbd > 99)
                    reROffRbd = 99;
            }
            catch
            {
                reROffRbd = -1;
            }

            try
            {
                var DREBp100 = DREBp*100;
                reRDefRbd = Convert.ToInt32(25 + 2.5*DREBp100);
                if (reRDefRbd > 99)
                    reRDefRbd = 99;
            }
            catch
            {
                reRDefRbd = -1;
            }

            try
            {
                reTShotTnd = Convert.ToInt32(2 + 4*FGAPG);
                if (reTShotTnd > 90)
                    reTShotTnd = 90;
            }
            catch
            {
                reTShotTnd = -1;
            }

            try
            {
                reTDrawFoul = Convert.ToInt32(FTAR*10);
                if (reTDrawFoul > 99)
                    reTDrawFoul = 99;
            }
            catch
            {
                reTDrawFoul = -1;
            }

            try
            {
                double FGAR = (double) FGA/MINS*36;
                int touchTotal = Convert.ToInt32(FGAR + FTAR + TOR + ASTR);
                reTTouch = Convert.ToInt32(3.141*Math.Pow(touchTotal, 2)/(1.178 + touchTotal));
                if (reTTouch > 99)
                    reTTouch = 99;
            }
            catch
            {
                reTTouch = -1;
            }

            try
            {
                reTCommitFl = Convert.ToInt32((double) FOUL/MINS*36*10);
                if (reTCommitFl > 99)
                    reTCommitFl = 99;
            }
            catch
            {
                reTCommitFl = -1;
            }
        }

        /// <summary>
        ///     Gets the best stats.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <returns>A well-formatted multi-line string presenting the best stats.</returns>
        public string GetBestStats(int count)
        {
            if (GP == 0) return "";

            Position position = Position1;
            double fgn = 0, tpn = 0, ftn = 0, ftrn = 0;
            var statsn = new Dictionary<string, double>();

            double fgfactor, tpfactor, ftfactor, orebfactor, rebfactor, astfactor, stlfactor, blkfactor, ptsfactor, ftrfactor;

            if (position.ToString().EndsWith("G"))
            {
                fgfactor = 0.4871;
                tpfactor = 0.39302;
                ftfactor = 0.86278;
                orebfactor = 1.242;
                rebfactor = 4.153;
                astfactor = 6.324;
                stlfactor = 1.619;
                blkfactor = 0.424;
                ptsfactor = 17.16;
                ftrfactor = 0.271417;
            }
            else if (position.ToString().EndsWith("F"))
            {
                fgfactor = 0.52792;
                tpfactor = 0.38034;
                ftfactor = 0.82656;
                orebfactor = 2.671;
                rebfactor = 8.145;
                astfactor = 3.037;
                stlfactor = 1.209;
                blkfactor = 1.24;
                ptsfactor = 17.731;
                ftrfactor = 0.307167;
            }
            else if (position.ToString().EndsWith("C"))
            {
                fgfactor = 0.52862;
                tpfactor = 0.23014;
                ftfactor = 0.75321;
                orebfactor = 2.328;
                rebfactor = 7.431;
                astfactor = 1.688;
                stlfactor = 0.68;
                blkfactor = 1.536;
                ptsfactor = 11.616;
                ftrfactor = 0.302868;
            }
            else
            {
                fgfactor = 0.51454;
                tpfactor = 0.3345;
                ftfactor = 0.81418;
                orebfactor = 2.0803;
                rebfactor = 6.5763;
                astfactor = 3.683;
                stlfactor = 1.1693;
                blkfactor = 1.0667;
                ptsfactor = 15.5023;
                ftrfactor = 0.385722;
            }

            if (FGM/GP > 4)
            {
                fgn = FGp/fgfactor;
            }
            statsn.Add("fgn", fgn);

            if (TPM/GP > 2)
            {
                tpn = TPp/tpfactor;
            }
            statsn.Add("tpn", tpn);

            if (FTM/GP > 4)
            {
                ftn = FTp/ftfactor;
            }
            statsn.Add("ftn", ftn);

            double orebn = ORPG/orebfactor;
            statsn.Add("orebn", orebn);

            /*
            drebn = (REB-OREB)/6.348;
            statsn.Add("drebn", drebn);
            */

            double rebn = RPG/rebfactor;
            statsn.Add("rebn", rebn);

            double astn = APG/astfactor;
            statsn.Add("astn", astn);

            double stln = SPG/stlfactor;
            statsn.Add("stln", stln);

            double blkn = BPG/blkfactor;
            statsn.Add("blkn", blkn);

            double ptsn = PPG/ptsfactor;
            statsn.Add("ptsn", ptsn);

            if (FTM/GP > 3)
            {
                ftrn = ((double) FTM/FGA)/ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            IOrderedEnumerable<string> items = from k in statsn.Keys orderby statsn[k] descending select k;

            string s = "";
            int i = 1;
            s += String.Format("PPG: {0:F1}\n", PPG);
            foreach (string item in items)
            {
                if (i == count)
                    break;

                switch (item)
                {
                    case "fgn":
                        s += String.Format("FG: {0:F1}-{1:F1} ({2:F3})\n", (double) FGM/GP, (double) FGA/GP, FGp);
                        break;

                    case "tpn":
                        s += String.Format("3P: {0:F1}-{1:F1} ({2:F3})\n", (double) TPM/GP, (double) TPA/GP, TPp);
                        break;

                    case "ftn":
                        s += String.Format("FT: {0:F1}-{1:F1} ({2:F3})\n", (double) FTM/GP, (double) FTA/GP, FTp);
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
                        s += String.Format("FTM/FGA: {0:F1}-{1:F1} ({2:F3})\n", (double) FTM/GP, (double) FGA/GP, (double) FTM/FGA);
                        break;
                }

                i++;
            }
            return s;
        }

        /// <summary>
        ///     Gets a list (dictionary) of the best stats.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <returns>A list (dictionary) of the best stats' names and values</returns>
        public Dictionary<string, string> GetBestStatsList(int count)
        {
            if (GP == 0)
                return new Dictionary<string, string>();

            var statList = new Dictionary<string, string>();
            string s = GetBestStats(count);
            string[] lines = s.Split('\n');
            for (int i = 1; i < count; i++)
            {
                string[] parts = lines[i].Split(new[] {": "}, StringSplitOptions.None);
                statList.Add(parts[0], parts[1]);
            }
            return statList;
        }

        /// <summary>
        ///     Shows a scouting report for the player in natural language.
        /// </summary>
        /// <param name="pst">The PlayerStats dictionary containing all the player information.</param>
        /// <param name="rankingsActive">The rankingsPerGame of currently active players.</param>
        /// <param name="rankingsTeam">The rankingsPerGame of the players in the same team.</param>
        /// <param name="rankingsPosition">The rankingsPerGame of the players in the same position.</param>
        /// <param name="pbsList">The list of the player's available box scores.</param>
        /// <param name="bestGame">The well-formatted string from the player's best game.</param>
        public string ScoutingReport(Dictionary<int, PlayerStats> pst, PlayerRankings rankingsActive, PlayerRankings rankingsTeam,
                                     PlayerRankings rankingsPosition, IList<PlayerBoxScore> pbsIList, string bestGame, bool playoffs = false)
        {
            List<PlayerBoxScore> pbsList = pbsIList.ToList();
            string s = "";
            s += String.Format("{0} {1}, born in {3} ({6} years old today), is a {4}{5} tall {2} ", FirstName, LastName, Position1, YearOfBirth, DisplayHeight, MainWindow.IsImperial ? "" : "cm.", DateTime.Today.Year - YearOfBirth);
            if (Position2 != Position.None)
                s += String.Format("(alternatively {0})", Position2);
            s += ", ";

            if (isActive)
                s += String.Format("who currently plays for the {0}.", TeamF);
            else
                s += String.Format("who is currently a Free Agent.");

            s += String.Format(" He's been a pro for {0} year", YearsPro);
            if (YearsPro != 1)
                s += "s";
            s += ".";

            s += "\n\n";

            s += String.Format("He averages {0:F1} PPG on {1:F1} MPG, making for {2:F1} points per 36 minutes. ", PPG, MPG, PTSR);

            if (rankingsTeam.rankingsPerGame[ID][p.PPG] <= 3)
                s += String.Format("One of the best scorers in the team, #{0} among his teammates. ",
                                   rankingsTeam.rankingsPerGame[ID][p.PPG]);
            if (rankingsPosition.rankingsPerGame[ID][p.PPG] <= 10)
                s += String.Format("His performance has got him to become one of the best at his position in scoring, #{0} among {1}'s. ",
                                   rankingsPosition.rankingsPerGame[ID][p.PPG], Position1);
            if (rankingsActive.rankingsPerGame[ID][p.PPG] <= 20)
                s += String.Format("He's actually one of the best in the league in scoring, rated #{0} overall. ",
                                   rankingsActive.rankingsPerGame[ID][p.PPG]);

            Dictionary<string, string> statList = GetBestStatsList(5);

            s += "\n\n";

            foreach (var stat in statList)
            {
                switch (stat.Key)
                {
                    case "FG":
                        s +=
                            String.Format(
                                "Shooting, one of his main strengths. He's averaging {0} as far as field goals go. Percentage-wise, his performance " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.rankingsPerGame[ID][p.FGp]);
                        if (rankingsTeam.rankingsPerGame[ID][p.FGp] <= 3)
                            s += String.Format("Top from the floor in his team, ranks at #{0} ", rankingsTeam.rankingsPerGame[ID][p.FGp]);
                        if (rankingsPosition.rankingsPerGame[ID][p.FGp] <= 10)
                            s += String.Format("Definitely dominating among {0}'s on scoring percentage, ranked at #{1}. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.FGp]);
                        break;
                    case "3P":
                        s +=
                            String.Format(
                                "His 3-point shooting is another area of focus. His three-point shooting averages {0}. #{1} in the league in 3P%. ",
                                stat.Value, rankingsActive.rankingsPerGame[ID][p.TPp]);
                        if (rankingsTeam.rankingsPerGame[ID][p.TPp] <= 3)
                            s += String.Format("One of the best guys from the arc in his team, ranks at #{0} ",
                                               rankingsTeam.rankingsPerGame[ID][p.TPp]);
                        if (rankingsPosition.rankingsPerGame[ID][p.TPp] <= 10)
                            s += String.Format("Not many {0}'s do better than him, as he's ranked at #{1}. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.TPp]);
                        break;
                    case "FT":
                        s +=
                            String.Format(
                                "Take a look at his free throw stats: He's averaging {0} from the line, which " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.rankingsPerGame[ID][p.FTp]);
                        if (rankingsTeam.rankingsPerGame[ID][p.FTp] <= 3)
                            s +=
                                String.Format(
                                    "Coach might prefer him to get all the fouls late in the game, as he ranks #{0} in his team. ",
                                    rankingsTeam.rankingsPerGame[ID][p.FTp]);
                        if (rankingsPosition.rankingsPerGame[ID][p.FTp] <= 10)
                            s += String.Format("Most {0}'s in the league struggle to keep up with him, he's ranked at #{1}. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.FTp]);
                        break;
                    case "ORPG":
                        s +=
                            String.Format(
                                "Crashing the offensive glass, one of his main strengths. His average offensive boards per game are at {0}, which " +
                                "ranks him at #{1} overall. He grabs {2:F1} offensive rebounds every 36 minutes. ", stat.Value,
                                rankingsActive.rankingsPerGame[ID][p.ORPG], OREBR);
                        if (rankingsTeam.rankingsPerGame[ID][p.ORPG] <= 3)
                            s += String.Format("One of the main guys to worry about below your basket, #{0} in his team. ",
                                               rankingsTeam.rankingsPerGame[ID][p.ORPG]);
                        if (rankingsPosition.rankingsPerGame[ID][p.ORPG] <= 10)
                            s += String.Format("He's ranked at #{1} among {0}'s in grabbing those second chance opportunities. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.ORPG]);
                        break;
                    case "RPG":
                        s +=
                            String.Format(
                                "He makes a point of crashing the boards. His RPG are at {0} ({2:F1} per 36 minutes), which " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.rankingsPerGame[ID][p.RPG], REBR);
                        if (rankingsTeam.rankingsPerGame[ID][p.RPG] <= 3)
                            s += String.Format("One of the top rebounders in his team, #{0} actually. ",
                                               rankingsTeam.rankingsPerGame[ID][p.RPG]);
                        if (rankingsPosition.rankingsPerGame[ID][p.RPG] <= 10)
                            s += String.Format("He's ranked at #{1} among {0}'s in crashing the boards. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.RPG]);
                        break;
                    case "BPG":
                        s +=
                            String.Format(
                                "Keep him in mind when he's in your face. His BPG are at {0} ({2:F1} per 36 minutes), which " +
                                "ranks him at #{1} overall. ", stat.Value, rankingsActive.rankingsPerGame[ID][p.BPG], BLKR);
                        if (rankingsTeam.rankingsPerGame[ID][p.BPG] <= 3)
                            s += String.Format("Among the top blockers in the team, ranked at #{0}. ",
                                               rankingsTeam.rankingsPerGame[ID][p.BPG]);
                        if (rankingsPosition.rankingsPerGame[ID][p.BPG] <= 10)
                            s += String.Format("One of the best {0}'s (#{1}) at blocking shots. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.BPG]);
                        break;
                    case "APG":
                        s +=
                            String.Format(
                                "Assisting the ball, an important aspect of his game. He does {0} APG ({2:F1} per 36 minutes), ranking him at #{1} overall. ",
                                stat.Value, rankingsActive.rankingsPerGame[ID][p.APG], ASTR);
                        if (rankingsTeam.rankingsPerGame[ID][p.APG] <= 3)
                            s += String.Format("#{0} as far as playmakers in the team go. ", rankingsTeam.rankingsPerGame[ID][p.APG]);
                        if (rankingsPosition.rankingsPerGame[ID][p.APG] <= 10)
                            s += String.Format("One of the league's best {0}'s (#{1}) at setting up teammates for a shot. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.APG]);
                        break;
                    case "SPG":
                        s +=
                            String.Format(
                                "Tries to keep his hands active; keep in mind his {0} SPG ({2:F1} per 36 minutes). His performance in taking the ball away has " +
                                "ranked him at #{1} in the league. ", stat.Value, rankingsActive.rankingsPerGame[ID][p.SPG], STLR);
                        if (rankingsTeam.rankingsPerGame[ID][p.SPG] <= 3)
                            s += String.Format("#{0} in taking the ball away among his teammates. ", rankingsTeam.rankingsPerGame[ID][p.SPG]);
                        if (rankingsPosition.rankingsPerGame[ID][p.SPG] <= 10)
                            s += String.Format("One of the league's best {0}'s (#{1}) in this aspect. ", Position1,
                                               rankingsPosition.rankingsPerGame[ID][p.SPG]);
                        break;
                    case "FTM/FGA":
                        s += String.Format("He fights through contact to get to the line. His FTM/FGA rate is at {0}. ", stat.Value);
                        break;
                }
                s += "\n";
            }

            s += String.Format(
                "His foul rate is at {0:F1} per 36 minutes, while his turnover rate is at {1:F1} per the same duration.\n\n",
                (double) FOUL/MINS*36, TOR);

            pbsList.Sort((pbs1, pbs2) => pbs1.RealDate.CompareTo(pbs2.RealDate));
            pbsList.Reverse();

            if (!String.IsNullOrWhiteSpace(bestGame))
            {
                string[] parts = bestGame.Split(new[] {": ", " vs ", " (", "\n"}, StringSplitOptions.None);
                s += String.Format("His best game was at {0} against the {1}, with a Game Score of {2:F2} ", parts[1], parts[2],
                                   pbsList.Find(pbs => pbs.RealDate == Convert.ToDateTime(parts[1])).GmSc);
                s += "(";
                for (int i = 5; i < parts.Length; i++)
                {
                    if (String.IsNullOrWhiteSpace(parts[i]))
                        break;

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
                s = s.TrimEnd(new[] {',', ' '});
                s += "). ";
            }

            if (pbsList.Count > 5)
            {
                double sum = 0;
                for (int i = 0; i < 5; i++)
                {
                    sum += pbsList[i].GmSc;
                }
                double average = sum/5;
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
                for (int i = 0; i < 3; i++)
                {
                    sum += pbsList[i].GmSc;
                }
                double average = sum/3;
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
            for (int k = 0; k < rankingsActive.rankingsPerGame[ID].Length; k++)
            {
                dict.Add(k, rankingsActive.rankingsPerGame[ID][k]);
            }
            dict[t.FPG] = pst.Count + 1 - dict[t.FPG];
            dict[t.TPG] = pst.Count + 1 - dict[t.TPG];
            dict[t.PAPG] = pst.Count + 1 - dict[t.PAPG];
            List<int> strengths = (from entry in dict orderby entry.Value ascending select entry.Key).ToList();
            int m = 0;
            int j = 3;
            while (true)
            {
                if (m == j)
                    break;
                switch (strengths[m])
                {
                    case p.APG:
                        s += String.Format("assists (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.APG], APG);
                        break;
                    case p.BPG:
                        s += String.Format("blocks (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.BPG], BPG);
                        break;
                    case p.DRPG:
                        s += String.Format("defensive rebounds (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.DRPG], DRPG);
                        break;
                    case p.FGeff:
                        s += String.Format("field goals (#{0}, {1:F1} per game on {2:F3}), ", rankingsActive.rankingsPerGame[ID][p.FGeff],
                                           (double) FGM/GP, FGp);
                        break;
                    case p.FPG:
                        s += String.Format("fouls (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.FPG], FPG);
                        break;
                    case p.FTeff:
                        s += String.Format("free throws (#{0}, {1:F1} per game on {2:F3}), ", rankingsActive.rankingsPerGame[ID][p.FTeff],
                                           (double) FTM/GP, FTp);
                        break;
                    case p.ORPG:
                        s += String.Format("offensive rebounds (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.ORPG], ORPG);
                        break;
                    case p.PPG:
                        s += String.Format("scoring (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.PPG], PPG);
                        break;
                    case p.RPG:
                        s += String.Format("rebounds (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.RPG], RPG);
                        break;
                    case p.SPG:
                        s += String.Format("steals (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.SPG], SPG);
                        break;
                    case p.TPG:
                        s += String.Format("turnovers (#{0}, {1:F1}), ", rankingsActive.rankingsPerGame[ID][p.TPG], TPG);
                        break;
                    case p.TPeff:
                        s += String.Format("three-pointers (#{0}, {1:F1} per game on {2:F3}), ", rankingsActive.rankingsPerGame[ID][p.TPeff],
                                           (double) TPM/GP, TPp);
                        break;
                    default:
                        j++;
                        break;
                }
                m++;
            }
            s = s.TrimEnd(new[] {' ', ','});
            s += ".";

            return s;
        }

        /// <summary>
        ///     Tries to parse the specified dictionary and update the specified PlayerStatsRow instance.
        /// </summary>
        /// <param name="psr">The PSR.</param>
        /// <param name="dict">The dict.</param>
        public static void TryChangePSR(ref PlayerStatsRow psr, Dictionary<string, string> dict)
        {
            psr.GP = psr.GP.TrySetValue(dict, "GP", typeof (UInt16));
            psr.GS = psr.GS.TrySetValue(dict, "GS", typeof (UInt16));
            psr.MINS = psr.MINS.TrySetValue(dict, "MINS", typeof (UInt16));
            psr.PTS = psr.PTS.TrySetValue(dict, "PTS", typeof (UInt16));
            psr.FGM = psr.FGM.TrySetValue(dict, "FGM", typeof (UInt16));
            psr.FGA = psr.FGA.TrySetValue(dict, "FGA", typeof (UInt16));
            psr.TPM = psr.TPM.TrySetValue(dict, "3PM", typeof (UInt16));
            psr.TPA = psr.TPA.TrySetValue(dict, "3PA", typeof (UInt16));
            psr.FTM = psr.FTM.TrySetValue(dict, "FTM", typeof (UInt16));
            psr.FTA = psr.FTA.TrySetValue(dict, "FTA", typeof (UInt16));
            psr.REB = psr.REB.TrySetValue(dict, "REB", typeof (UInt16));
            psr.OREB = psr.OREB.TrySetValue(dict, "OREB", typeof (UInt16));
            psr.DREB = psr.DREB.TrySetValue(dict, "DREB", typeof (UInt16));
            psr.AST = psr.AST.TrySetValue(dict, "AST", typeof (UInt16));
            psr.TOS = psr.TOS.TrySetValue(dict, "TO", typeof (UInt16));
            psr.STL = psr.STL.TrySetValue(dict, "STL", typeof (UInt16));
            psr.BLK = psr.BLK.TrySetValue(dict, "BLK", typeof (UInt16));
            psr.FOUL = psr.FOUL.TrySetValue(dict, "FOUL", typeof (UInt16));
        }

        public static void Refresh(ref PlayerStatsRow psr)
        {
            psr = new PlayerStatsRow(new PlayerStats(psr));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public PlayerStatsRow ConvertToMyLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            string team = TeamF;
            TeamStats ts = teamStats[MainWindow.TeamOrder[team]];
            uint gamesTeam = (!playoffs) ? ts.getGames() : ts.getPlayoffGames();
            uint gamesPlayer = GP;
            PlayerStatsRow newpsr = this.DeepClone();

            var gpPctSetting = MainWindow.MyLeadersGPPctSetting;
            var gpPctRequired = MainWindow.MyLeadersGPPctRequired;
            var mpgSetting = MainWindow.MyLeadersMPGSetting;
            var mpgRequired = MainWindow.MyLeadersMPGRequired;

            if ((gpPctSetting != "-1" && (double) gamesPlayer*100/gamesTeam < gpPctRequired) || (mpgSetting != "-1" && MPG < mpgRequired))
            {
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
                newpsr.MPG = float.NaN;
            }

            return newpsr;
        }

        /// <summary>
        ///     Edits a player's stats row to adjust for the rules and requirements of the NBA's League Leaders standings.
        /// </summary>
        /// <param name="psr">The player stats row.</param>
        /// <param name="teamStats">The player's team stats.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the playoff stats will be edited; otherwise, the regular season's.
        /// </param>
        /// <returns></returns>
        public PlayerStatsRow ConvertToLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            string team = TeamF;
            TeamStats ts = teamStats[MainWindow.TeamOrder[team]];
            uint gamesTeam = (!playoffs) ? ts.getGames() : ts.getPlayoffGames();
            uint gamesPlayer = GP;
            PlayerStatsRow newpsr = this.DeepClone();

            // Below functions found using Eureqa II
            var gamesRequired = (int) Math.Ceiling(0.8522*gamesTeam); // Maximum error of 0
            var fgmRequired = (int) Math.Ceiling(3.65*gamesTeam); // Max error of 0
            var ftmRequired = (int) Math.Ceiling(1.52*gamesTeam);
            var tpmRequired = (int) Math.Ceiling(0.666671427752402*gamesTeam);
            var ptsRequired = (int) Math.Ceiling(17.07*gamesTeam);
            var rebRequired = (int) Math.Ceiling(9.74720677727814*gamesTeam);
            var astRequired = (int) Math.Ceiling(4.87*gamesTeam);
            var stlRequired = (int) Math.Ceiling(1.51957078555763*gamesTeam);
            var blkRequired = (int) Math.Ceiling(1.21*gamesTeam);
            var minRequired = (int) Math.Ceiling(24.39*gamesTeam);

            if (FGM < fgmRequired)
            {
                newpsr.FGp = float.NaN;
                newpsr.FGeff = float.NaN;
            }
            if (TPM < tpmRequired)
            {
                newpsr.TPp = float.NaN;
                newpsr.TPeff = float.NaN;
            }
            if (FTM < ftmRequired)
            {
                newpsr.FTp = float.NaN;
                newpsr.FTeff = float.NaN;
            }

            if (gamesPlayer >= gamesRequired)
            {
                return newpsr;
            }

            if (PTS < ptsRequired)
                newpsr.PPG = float.NaN;
            if (REB < rebRequired)
            {
                newpsr.RPG = float.NaN;
                newpsr.DRPG = float.NaN;
                newpsr.ORPG = float.NaN;
            }
            if (AST < astRequired)
                newpsr.APG = float.NaN;
            if (STL < stlRequired)
                newpsr.SPG = float.NaN;
            if (BLK < blkRequired)
                newpsr.BPG = float.NaN;
            if (MINS < minRequired)
                newpsr.MPG = float.NaN;
            return newpsr;
        }
    }
}