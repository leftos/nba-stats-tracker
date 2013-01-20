using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Annotations;
using NBA_Stats_Tracker.Data.BoxScores;
using NBA_Stats_Tracker.Data.Misc;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     A container for all of a player's information, stats, averages and metrics handled by the program.
    /// </summary>
    [Serializable]
    public class PlayerStats : INotifyPropertyChanged
    {
        public PlayerContract Contract;
        public string FirstName;
        public int ID;
        public string LastName;
        public Position Position1;
        public Position Position2;
        public string TeamF;
        public string TeamS = "";
        public int YearOfBirth;
        public int YearsPro;
        public float[] averages = new float[16];
        public ushort[] careerHighs = new ushort[18];
        public double height;
        public bool isActive;
        public bool isAllStar;
        public bool isHidden;
        public bool isInjured;
        public bool isNBAChampion;
        public Dictionary<string, double> metrics = new Dictionary<string, double>();
        public float[] pl_averages = new float[16];
        public Dictionary<string, double> pl_metrics = new Dictionary<string, double>();
        public uint[] pl_stats = new uint[17];
        public uint[] stats = new uint[17];
        public double weight;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        public PlayerStats()
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = 0;
            }

            for (int i = 0; i < averages.Length; i++)
            {
                averages[i] = 0;
            }

            for (int i = 0; i < pl_stats.Length; i++)
            {
                pl_stats[i] = 0;
            }

            for (int i = 0; i < pl_averages.Length; i++)
            {
                pl_averages[i] = 0;
            }

            for (int i = 0; i < careerHighs.Length; i++)
            {
                careerHighs[i] = 0;
            }

            Contract = new PlayerContract();
            isActive = false;
            isHidden = false;
            isInjured = false;
            isAllStar = false;
            isNBAChampion = false;
            YearOfBirth = 0;
            YearsPro = 0;
            height = 0;
            weight = 0;
            TeamF = "";
            TeamS = "";

            p.metricsNames.ForEach(name =>
                                   {
                                       metrics.Add(name, double.NaN);
                                       pl_metrics.Add(name, double.NaN);
                                   });
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="player">A Player instance containing the information to initialize with.</param>
        public PlayerStats(Player player) : this()
        {
            ID = player.ID;
            LastName = player.LastName;
            FirstName = player.FirstName;
            Position1 = player.Position1;
            Position2 = player.Position2;
            TeamF = player.Team;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="dataRow">A row of an SQLite query result containing player information.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the row is assumed to contain playoff stats.
        /// </param>
        public PlayerStats(DataRow dataRow, bool playoffs = false) : this()
        {
            ID = Tools.getInt(dataRow, "ID");

            if (!playoffs)
            {
                LastName = Tools.getString(dataRow, "LastName");
                FirstName = Tools.getString(dataRow, "FirstName");
                string p1 = Tools.getString(dataRow, "Position1");
                if (String.IsNullOrWhiteSpace(p1))
                    Position1 = Position.None;
                else
                    Position1 = (Position) Enum.Parse(typeof (Position), p1);
                string p2 = Tools.getString(dataRow, "Position2");
                if (String.IsNullOrWhiteSpace(p2))
                    Position2 = Position.None;
                else
                    Position2 = (Position) Enum.Parse(typeof (Position), p2);
                TeamF = Tools.getString(dataRow, "TeamFin");
                TeamS = Tools.getString(dataRow, "TeamSta");
                isActive = Tools.getBoolean(dataRow, "isActive");

                // Backwards compatibility with databases that didn't have the field
                try
                {
                    isHidden = Tools.getBoolean(dataRow, "isHidden");
                }
                catch
                {
                    isHidden = false;
                }

                try
                {
                    YearOfBirth = Tools.getInt(dataRow, "YearOfBirth");
                }
                catch
                {
                    try
                    {
                        YearOfBirth = Convert.ToInt32(MainWindow.input) - Tools.getInt(dataRow, "Age");
                    }
                    catch
                    {
                        YearOfBirth = 0;
                    }
                }

                try
                {
                    YearsPro = Tools.getInt(dataRow, "YearsPro");
                }
                catch (Exception)
                {
                    YearsPro = 0;
                }
                //

                isInjured = Tools.getBoolean(dataRow, "isInjured");
                isAllStar = Tools.getBoolean(dataRow, "isAllStar");
                isNBAChampion = Tools.getBoolean(dataRow, "isNBAChampion");
                Contract = new PlayerContract();
                for (int i = 1; i <= 7; i++)
                {
                    int salary;
                    try
                    {
                        salary = Tools.getInt(dataRow, "ContractY" + i);
                    }
                    catch (ArgumentException)
                    {
                        break;
                    }
                    if (salary == 0)
                        break;

                    Contract.ContractSalaryPerYear.Add(salary);
                }
                try
                {
                    Contract.Option =
                        (PlayerContractOption) Enum.Parse(typeof (PlayerContractOption), Tools.getString(dataRow, "ContractOption"));
                }
                catch (ArgumentException)
                {
                    Contract.Option = PlayerContractOption.None;
                }
                try
                {
                    height = Tools.getReal(dataRow, "Height");
                    weight = Tools.getReal(dataRow, "Weight");
                }
                catch (ArgumentException)
                {
                    height = 0;
                    weight = 0;
                }
            }

            GetStatsFromDataRow(dataRow, playoffs);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <param name="LastName">The last name.</param>
        /// <param name="FirstName">The first name.</param>
        /// <param name="Position1">The primary position.</param>
        /// <param name="Position2">The secondary position.</param>
        /// <param name="TeamF">The team the player is currently with.</param>
        /// <param name="TeamS">The team the player started the season with.</param>
        /// <param name="isActive">
        ///     if set to <c>true</c> the player is currently active (i.e. signed with a team).
        /// </param>
        /// <param name="isHidden">
        ///     if set to <c>true</c> the player is hidden for this season.
        /// </param>
        /// <param name="isInjured">
        ///     if set to <c>true</c> the player is injured.
        /// </param>
        /// <param name="isAllStar">
        ///     if set to <c>true</c> is an All-Star this season.
        /// </param>
        /// <param name="isNBAChampion">
        ///     if set to <c>true</c> is a champion this season.
        /// </param>
        /// <param name="dataRow">A row of an SQLite query result containing player information.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c> the row is assumed to contain playoff stats.
        /// </param>
        public PlayerStats(int ID, string LastName, string FirstName, Position Position1, Position Position2, int YearOfBirth, int YearsPro,
                           string TeamF, string TeamS, bool isActive, bool isHidden, bool isInjured, bool isAllStar, bool isNBAChampion,
                           DataRow dataRow, bool playoffs = false) : this()
        {
            this.ID = ID;
            this.LastName = LastName;
            this.FirstName = FirstName;
            this.Position1 = Position1;
            this.Position2 = Position2;
            this.TeamF = TeamF;
            this.TeamS = TeamS;
            this.YearOfBirth = YearOfBirth;
            this.YearsPro = YearsPro;
            this.isActive = isActive;
            this.isHidden = isHidden;
            this.isAllStar = isAllStar;
            this.isInjured = isInjured;
            this.isNBAChampion = isNBAChampion;

            try
            {
                if (!playoffs)
                {
                    stats[p.GP] = Tools.getUInt16(dataRow, "GP");
                    stats[p.GS] = Tools.getUInt16(dataRow, "GS");
                    stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
                    stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");

                    string[] parts = Tools.getString(dataRow, "FG").Split('-');

                    stats[p.FGM] = Convert.ToUInt16(parts[0]);
                    stats[p.FGA] = Convert.ToUInt16(parts[1]);

                    parts = Tools.getString(dataRow, "3PT").Split('-');

                    stats[p.TPM] = Convert.ToUInt16(parts[0]);
                    stats[p.TPA] = Convert.ToUInt16(parts[1]);

                    parts = Tools.getString(dataRow, "FT").Split('-');

                    stats[p.FTM] = Convert.ToUInt16(parts[0]);
                    stats[p.FTA] = Convert.ToUInt16(parts[1]);

                    stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
                    stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
                    stats[p.STL] = Tools.getUInt16(dataRow, "STL");
                    stats[p.TOS] = Tools.getUInt16(dataRow, "TO");
                    stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
                    stats[p.AST] = Tools.getUInt16(dataRow, "AST");
                    stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");
                }
                else
                {
                    pl_stats[p.GP] = Tools.getUInt16(dataRow, "GP");
                    pl_stats[p.GS] = Tools.getUInt16(dataRow, "GS");
                    pl_stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
                    pl_stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");

                    string[] parts = Tools.getString(dataRow, "FG").Split('-');

                    pl_stats[p.FGM] = Convert.ToUInt16(parts[0]);
                    pl_stats[p.FGA] = Convert.ToUInt16(parts[1]);

                    parts = Tools.getString(dataRow, "3PT").Split('-');

                    pl_stats[p.TPM] = Convert.ToUInt16(parts[0]);
                    pl_stats[p.TPA] = Convert.ToUInt16(parts[1]);

                    parts = Tools.getString(dataRow, "FT").Split('-');

                    pl_stats[p.FTM] = Convert.ToUInt16(parts[0]);
                    pl_stats[p.FTA] = Convert.ToUInt16(parts[1]);

                    pl_stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
                    pl_stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
                    pl_stats[p.STL] = Tools.getUInt16(dataRow, "STL");
                    pl_stats[p.TOS] = Tools.getUInt16(dataRow, "TO");
                    pl_stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
                    pl_stats[p.AST] = Tools.getUInt16(dataRow, "AST");
                    pl_stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("{0} {1} ({2}) has some invalid data.\n\nError: {3}", FirstName, LastName, TeamF, ex.Message));
            }

            CalcAvg();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="playerStatsRow">The player stats row.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c> the row is assumed to contain playoff stats.
        /// </param>
        public PlayerStats(PlayerStatsRow playerStatsRow, bool playoffs = false) : this()
        {
            LastName = playerStatsRow.LastName;
            FirstName = playerStatsRow.FirstName;

            if (!playoffs)
            {
                stats[p.GP] = playerStatsRow.GP;
                stats[p.GS] = playerStatsRow.GS;
                stats[p.MINS] = playerStatsRow.MINS;
                stats[p.PTS] = playerStatsRow.PTS;
                stats[p.FGM] = playerStatsRow.FGM;
                stats[p.FGA] = playerStatsRow.FGA;
                stats[p.TPM] = playerStatsRow.TPM;
                stats[p.TPA] = playerStatsRow.TPA;
                stats[p.FTM] = playerStatsRow.FTM;
                stats[p.FTA] = playerStatsRow.FTA;
                stats[p.OREB] = playerStatsRow.OREB;
                stats[p.DREB] = playerStatsRow.DREB;
                stats[p.STL] = playerStatsRow.STL;
                stats[p.TOS] = playerStatsRow.TOS;
                stats[p.BLK] = playerStatsRow.BLK;
                stats[p.AST] = playerStatsRow.AST;
                stats[p.FOUL] = playerStatsRow.FOUL;

                metrics["GmSc"] = playerStatsRow.GmSc;
                metrics["GmScE"] = playerStatsRow.GmScE;
                metrics["EFF"] = playerStatsRow.EFF;
                metrics["EFG%"] = playerStatsRow.EFGp;
                metrics["TS%"] = playerStatsRow.TSp;
                metrics["AST%"] = playerStatsRow.ASTp;
                metrics["STL%"] = playerStatsRow.STLp;
                metrics["TO%"] = playerStatsRow.TOp;
                metrics["USG%"] = playerStatsRow.USGp;
                metrics["PTSR"] = playerStatsRow.PTSR;
                metrics["REBR"] = playerStatsRow.REBR;
                metrics["OREBR"] = playerStatsRow.OREBR;
                metrics["ASTR"] = playerStatsRow.ASTR;
                metrics["BLKR"] = playerStatsRow.BLKR;
                metrics["STLR"] = playerStatsRow.STLR;
                metrics["TOR"] = playerStatsRow.TOR;
                metrics["FTR"] = playerStatsRow.FTR;
                metrics["PER"] = playerStatsRow.PER;
                metrics["BLK%"] = playerStatsRow.BLKp;
                metrics["DREB%"] = playerStatsRow.DREBp;
                metrics["OREB%"] = playerStatsRow.OREBp;
                metrics["REB%"] = playerStatsRow.REBp;
                metrics["PPR"] = playerStatsRow.PPR;
            }
            else
            {
                pl_stats[p.GP] = playerStatsRow.GP;
                pl_stats[p.GS] = playerStatsRow.GS;
                pl_stats[p.MINS] = playerStatsRow.MINS;
                pl_stats[p.PTS] = playerStatsRow.PTS;
                pl_stats[p.FGM] = playerStatsRow.FGM;
                pl_stats[p.FGA] = playerStatsRow.FGA;
                pl_stats[p.TPM] = playerStatsRow.TPM;
                pl_stats[p.TPA] = playerStatsRow.TPA;
                pl_stats[p.FTM] = playerStatsRow.FTM;
                pl_stats[p.FTA] = playerStatsRow.FTA;
                pl_stats[p.OREB] = playerStatsRow.OREB;
                pl_stats[p.DREB] = playerStatsRow.DREB;
                pl_stats[p.STL] = playerStatsRow.STL;
                pl_stats[p.TOS] = playerStatsRow.TOS;
                pl_stats[p.BLK] = playerStatsRow.BLK;
                pl_stats[p.AST] = playerStatsRow.AST;
                pl_stats[p.FOUL] = playerStatsRow.FOUL;

                pl_metrics["GmSc"] = playerStatsRow.GmSc;
                pl_metrics["GmScE"] = playerStatsRow.GmScE;
                pl_metrics["EFF"] = playerStatsRow.EFF;
                pl_metrics["EFG%"] = playerStatsRow.EFGp;
                pl_metrics["TS%"] = playerStatsRow.TSp;
                pl_metrics["AST%"] = playerStatsRow.ASTp;
                pl_metrics["STL%"] = playerStatsRow.STLp;
                pl_metrics["TO%"] = playerStatsRow.TOp;
                pl_metrics["USG%"] = playerStatsRow.USGp;
                pl_metrics["PTSR"] = playerStatsRow.PTSR;
                pl_metrics["REBR"] = playerStatsRow.REBR;
                pl_metrics["OREBR"] = playerStatsRow.OREBR;
                pl_metrics["ASTR"] = playerStatsRow.ASTR;
                pl_metrics["BLKR"] = playerStatsRow.BLKR;
                pl_metrics["STLR"] = playerStatsRow.STLR;
                pl_metrics["TOR"] = playerStatsRow.TOR;
                pl_metrics["FTR"] = playerStatsRow.FTR;
                pl_metrics["PER"] = playerStatsRow.PER;
                pl_metrics["BLK%"] = playerStatsRow.BLKp;
                pl_metrics["DREB%"] = playerStatsRow.DREBp;
                pl_metrics["OREB%"] = playerStatsRow.OREBp;
                pl_metrics["REB%"] = playerStatsRow.REBp;
                pl_metrics["PPR"] = playerStatsRow.PPR;
            }

            ID = playerStatsRow.ID;
            Position1 = playerStatsRow.Position1;
            Position2 = playerStatsRow.Position2;
            TeamF = playerStatsRow.TeamF;
            TeamS = playerStatsRow.TeamS;
            YearOfBirth = playerStatsRow.YearOfBirth;
            YearsPro = playerStatsRow.YearsPro;
            isActive = playerStatsRow.isActive;
            isHidden = playerStatsRow.isHidden;
            isAllStar = playerStatsRow.isAllStar;
            isInjured = playerStatsRow.isInjured;
            isNBAChampion = playerStatsRow.isNBAChampion;

            Contract.Option = playerStatsRow.ContractOption;
            Contract.ContractSalaryPerYear.Clear();
            for (int i = 1; i <= 7; i++)
            {
                int salary = Convert.ToInt32(typeof (PlayerStatsRow).GetProperty("ContractY" + i).GetValue(playerStatsRow, null));
                if (salary == 0)
                {
                    break;
                }

                Contract.ContractSalaryPerYear.Add(salary);
            }

            height = playerStatsRow.Height;
            weight = playerStatsRow.Weight;

            CalcAvg();
        }

        public string Position1S
        {
            get { return PositionToString(Position1); }
        }

        public string Position2S
        {
            get { return PositionToString(Position2); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static string PositionToString(Position position)
        {
            switch (position)
            {
                case Position.C:
                    return "C";
                case Position.PF:
                    return "PF";
                case Position.SF:
                    return "SF";
                case Position.SG:
                    return "SG";
                case Position.PG:
                    return "PG";
                default:
                    return "";
            }
        }

        public void UpdateCareerHighs(DataRow r)
        {
            int importedID = Tools.getInt(r, "PlayerID");
            if (importedID != ID)
            {
                throw new Exception("Tried to update Career Highs of Player with ID " + ID + " with career highs of player with ID " +
                                    importedID);
            }
            careerHighs[p.MINS] = Convert.ToUInt16(r["MINS"].ToString());
            careerHighs[p.PTS] = Convert.ToUInt16(r["PTS"].ToString());
            careerHighs[p.REB] = Convert.ToUInt16(r["REB"].ToString());
            careerHighs[p.AST] = Convert.ToUInt16(r["AST"].ToString());
            careerHighs[p.STL] = Convert.ToUInt16(r["STL"].ToString());
            careerHighs[p.BLK] = Convert.ToUInt16(r["BLK"].ToString());
            careerHighs[p.TOS] = Convert.ToUInt16(r["TOS"].ToString());
            careerHighs[p.FGM] = Convert.ToUInt16(r["FGM"].ToString());
            careerHighs[p.FGA] = Convert.ToUInt16(r["FGA"].ToString());
            careerHighs[p.TPM] = Convert.ToUInt16(r["TPM"].ToString());
            careerHighs[p.TPA] = Convert.ToUInt16(r["TPA"].ToString());
            careerHighs[p.FTM] = Convert.ToUInt16(r["FTM"].ToString());
            careerHighs[p.FTA] = Convert.ToUInt16(r["FTA"].ToString());
            careerHighs[p.OREB] = Convert.ToUInt16(r["OREB"].ToString());
            careerHighs[p.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            careerHighs[p.DREB] = Convert.ToUInt16(r["DREB"].ToString());
        }

        public void UpdateCareerHighs(PlayerHighsRow phr)
        {
            if (phr.PlayerID != ID)
            {
                throw new Exception("Tried to update Career Highs of Player with ID " + ID + " with career highs of player with ID " +
                                    phr.PlayerID);
            }
            careerHighs[p.MINS] = phr.MINS;
            careerHighs[p.PTS] = phr.PTS;
            careerHighs[p.FGM] = phr.FGM;
            careerHighs[p.FGA] = phr.FGA;
            careerHighs[p.TPM] = phr.TPM;
            careerHighs[p.TPA] = phr.TPA;
            careerHighs[p.FTM] = phr.FTM;
            careerHighs[p.FTA] = phr.FTA;
            careerHighs[p.REB] = phr.REB;
            careerHighs[p.OREB] = phr.OREB;
            careerHighs[p.DREB] = phr.DREB;
            careerHighs[p.STL] = phr.STL;
            careerHighs[p.TOS] = phr.TOS;
            careerHighs[p.BLK] = phr.BLK;
            careerHighs[p.AST] = phr.AST;
            careerHighs[p.FOUL] = phr.FOUL;
        }

        public void CalculateSeasonHighs(List<BoxScoreEntry> bsList)
        {
            /*
            var allTimePBSList = new List<PlayerBoxScore>();
            var db = new SQLite_Database.SQLiteDatabase(MainWindow.currentDB);
            string q = "select * from PlayerResults where PlayerID = " + ID;
            var res = db.GetDataTable(q);
            foreach (DataRow dr in res.Rows)
            {
                allTimePBSList.Add(new PlayerBoxScore(dr));
                q = "select SeasonNum from GameResults where GameID = " + Tools.getInt(dr, "GameID");
                int seasonNum = Convert.ToInt32(db.ExecuteScalar(q));
                allTimePBSList.Last().SeasonNum = seasonNum;
            }
            var seasons = allTimePBSList.GroupBy(pbs => pbs.SeasonNum).Select(g => g.Key).ToList();
            */

            List<BoxScoreEntry> bsListWithPlayer = bsList.Where(bse => bse.pbsList.Any(pbs => pbs.PlayerID == ID)).ToList();
            List<int> seasonsList = bsListWithPlayer.GroupBy(bse => bse.bs.SeasonNum).Select(pair => pair.Key).ToList();
            List<PlayerBoxScore> allTimePBSList = bsListWithPlayer.Select(bse => bse.pbsList.Single(pbs => pbs.PlayerID == ID)).ToList();
            allTimePBSList.ForEach(pbs => pbs.SeasonNum = bsListWithPlayer.Single(bse => bse.bs.id == pbs.GameID).bs.SeasonNum);

            if (MainWindow.seasonHighs.ContainsKey(ID))
            {
                MainWindow.seasonHighs.Remove(ID);
            }
            MainWindow.seasonHighs.Add(ID, new Dictionary<int, ushort[]>());
            foreach (int season in seasonsList)
            {
                List<PlayerBoxScore> seasonPBSList = allTimePBSList.Where(pbs => pbs.SeasonNum == season).ToList();
                MainWindow.seasonHighs[ID].Add(season, new ushort[18]);
                ushort[] sh = MainWindow.seasonHighs[ID][season];
                for (int i = 0; i < sh.Length; i++)
                {
                    sh[i] = 0;
                }
                sh[p.AST] = seasonPBSList.Select(pbs => pbs.AST).Max();
                sh[p.BLK] = seasonPBSList.Select(pbs => pbs.BLK).Max();
                sh[p.DREB] = seasonPBSList.Select(pbs => pbs.DREB).Max();
                sh[p.OREB] = seasonPBSList.Select(pbs => pbs.OREB).Max();
                sh[p.REB] = seasonPBSList.Select(pbs => pbs.REB).Max();
                sh[p.STL] = seasonPBSList.Select(pbs => pbs.STL).Max();
                sh[p.TOS] = seasonPBSList.Select(pbs => pbs.TOS).Max();
                sh[p.FOUL] = seasonPBSList.Select(pbs => pbs.FOUL).Max();
                sh[p.FGM] = seasonPBSList.Select(pbs => pbs.FGM).Max();
                sh[p.FGA] = seasonPBSList.Select(pbs => pbs.FGA).Max();
                sh[p.TPM] = seasonPBSList.Select(pbs => pbs.TPM).Max();
                sh[p.TPA] = seasonPBSList.Select(pbs => pbs.TPA).Max();
                sh[p.FTM] = seasonPBSList.Select(pbs => pbs.FTM).Max();
                sh[p.FTA] = seasonPBSList.Select(pbs => pbs.FTA).Max();
                sh[p.PTS] = seasonPBSList.Select(pbs => pbs.PTS).Max();
                sh[p.MINS] = seasonPBSList.Select(pbs => pbs.MINS).Max();

                for (int i = 0; i < sh.Length; i++)
                {
                    if (careerHighs[i] < sh[i])
                    {
                        careerHighs[i] = sh[i];
                    }
                }
            }
        }

        public void GetStatsFromDataRow(DataRow dataRow, bool isPlayoff)
        {
            if (!isPlayoff)
            {
                stats[p.GP] = Tools.getUInt16(dataRow, "GP");
                stats[p.GS] = Tools.getUInt16(dataRow, "GS");
                stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
                stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");
                stats[p.FGM] = Tools.getUInt16(dataRow, "FGM");
                stats[p.FGA] = Tools.getUInt16(dataRow, "FGA");
                stats[p.TPM] = Tools.getUInt16(dataRow, "TPM");
                stats[p.TPA] = Tools.getUInt16(dataRow, "TPA");
                stats[p.FTM] = Tools.getUInt16(dataRow, "FTM");
                stats[p.FTA] = Tools.getUInt16(dataRow, "FTA");
                stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
                stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
                stats[p.STL] = Tools.getUInt16(dataRow, "STL");
                stats[p.TOS] = Tools.getUInt16(dataRow, "TOS");
                stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
                stats[p.AST] = Tools.getUInt16(dataRow, "AST");
                stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");
            }
            else
            {
                pl_stats[p.GP] = Tools.getUInt16(dataRow, "GP");
                pl_stats[p.GS] = Tools.getUInt16(dataRow, "GS");
                pl_stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
                pl_stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");
                pl_stats[p.FGM] = Tools.getUInt16(dataRow, "FGM");
                pl_stats[p.FGA] = Tools.getUInt16(dataRow, "FGA");
                pl_stats[p.TPM] = Tools.getUInt16(dataRow, "TPM");
                pl_stats[p.TPA] = Tools.getUInt16(dataRow, "TPA");
                pl_stats[p.FTM] = Tools.getUInt16(dataRow, "FTM");
                pl_stats[p.FTA] = Tools.getUInt16(dataRow, "FTA");
                pl_stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
                pl_stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
                pl_stats[p.STL] = Tools.getUInt16(dataRow, "STL");
                pl_stats[p.TOS] = Tools.getUInt16(dataRow, "TOS");
                pl_stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
                pl_stats[p.AST] = Tools.getUInt16(dataRow, "AST");
                pl_stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");
            }

            CalcAvg();
        }

        /// <summary>
        ///     Updates the playoff stats.
        /// </summary>
        /// <param name="dataRow">The data row containing the playoff stats.</param>
        public void UpdatePlayoffStats(DataRow dataRow)
        {
            pl_stats[p.GP] = Tools.getUInt16(dataRow, "GP");
            pl_stats[p.GS] = Tools.getUInt16(dataRow, "GS");
            pl_stats[p.MINS] = Tools.getUInt16(dataRow, "MINS");
            pl_stats[p.PTS] = Tools.getUInt16(dataRow, "PTS");
            pl_stats[p.FGM] = Tools.getUInt16(dataRow, "FGM");
            pl_stats[p.FGA] = Tools.getUInt16(dataRow, "FGA");
            pl_stats[p.TPM] = Tools.getUInt16(dataRow, "TPM");
            pl_stats[p.TPA] = Tools.getUInt16(dataRow, "TPA");
            pl_stats[p.FTM] = Tools.getUInt16(dataRow, "FTM");
            pl_stats[p.FTA] = Tools.getUInt16(dataRow, "FTA");
            pl_stats[p.OREB] = Tools.getUInt16(dataRow, "OREB");
            pl_stats[p.DREB] = Tools.getUInt16(dataRow, "DREB");
            pl_stats[p.STL] = Tools.getUInt16(dataRow, "STL");
            pl_stats[p.TOS] = Tools.getUInt16(dataRow, "TOS");
            pl_stats[p.BLK] = Tools.getUInt16(dataRow, "BLK");
            pl_stats[p.AST] = Tools.getUInt16(dataRow, "AST");
            pl_stats[p.FOUL] = Tools.getUInt16(dataRow, "FOUL");

            CalcAvg(true);
        }

        /// <summary>
        ///     Updates the playoff stats.
        /// </summary>
        /// <param name="dataRow">The data row containing the playoff stats.</param>
        public void UpdatePlayoffStats(PlayerStatsRow pl_psr)
        {
            pl_stats[p.GP] = pl_psr.GP;
            pl_stats[p.GS] = pl_psr.GS;
            pl_stats[p.MINS] = pl_psr.MINS;
            pl_stats[p.PTS] = pl_psr.PTS;
            pl_stats[p.FGM] = pl_psr.FGM;
            pl_stats[p.FGA] = pl_psr.FGA;
            pl_stats[p.TPM] = pl_psr.TPM;
            pl_stats[p.TPA] = pl_psr.TPA;
            pl_stats[p.FTM] = pl_psr.FTM;
            pl_stats[p.FTA] = pl_psr.FTA;
            pl_stats[p.OREB] = pl_psr.OREB;
            pl_stats[p.DREB] = pl_psr.DREB;
            pl_stats[p.STL] = pl_psr.STL;
            pl_stats[p.TOS] = pl_psr.TOS;
            pl_stats[p.BLK] = pl_psr.BLK;
            pl_stats[p.AST] = pl_psr.AST;
            pl_stats[p.FOUL] = pl_psr.FOUL;

            CalcAvg(true);
        }

        /// <summary>
        ///     Calculates the averages of a player's stats.
        /// </summary>
        /// <param name="playoffsOnly">
        ///     if set to <c>true</c>, only the playoff averages will be calculated.
        /// </param>
        public void CalcAvg(bool playoffsOnly = false)
        {
            if (!playoffsOnly)
            {
                uint games = stats[p.GP];
                averages[p.MPG] = (float) stats[p.MINS]/games;
                averages[p.PPG] = (float) stats[p.PTS]/games;
                averages[p.FGp] = (float) stats[p.FGM]/stats[p.FGA];
                averages[p.FGeff] = averages[p.FGp]*((float) stats[p.FGM]/games);
                averages[p.TPp] = (float) stats[p.TPM]/stats[p.TPA];
                averages[p.TPeff] = averages[p.TPp]*((float) stats[p.TPM]/games);
                averages[p.FTp] = (float) stats[p.FTM]/stats[p.FTA];
                averages[p.FTeff] = averages[p.FTp]*((float) stats[p.FTM]/games);
                averages[p.RPG] = (float) (stats[p.OREB] + stats[p.DREB])/games;
                averages[p.ORPG] = (float) stats[p.OREB]/games;
                averages[p.DRPG] = (float) stats[p.DREB]/games;
                averages[p.SPG] = (float) stats[p.STL]/games;
                averages[p.BPG] = (float) stats[p.BLK]/games;
                averages[p.TPG] = (float) stats[p.TOS]/games;
                averages[p.APG] = (float) stats[p.AST]/games;
                averages[p.FPG] = (float) stats[p.FOUL]/games;
            }

            uint pl_games = pl_stats[p.GP];
            pl_averages[p.MPG] = (float) pl_stats[p.MINS]/pl_games;
            pl_averages[p.PPG] = (float) pl_stats[p.PTS]/pl_games;
            pl_averages[p.FGp] = (float) pl_stats[p.FGM]/pl_stats[p.FGA];
            pl_averages[p.FGeff] = pl_averages[p.FGp]*((float) pl_stats[p.FGM]/pl_games);
            pl_averages[p.TPp] = (float) pl_stats[p.TPM]/pl_stats[p.TPA];
            pl_averages[p.TPeff] = pl_averages[p.TPp]*((float) pl_stats[p.TPM]/pl_games);
            pl_averages[p.FTp] = (float) pl_stats[p.FTM]/pl_stats[p.FTA];
            pl_averages[p.FTeff] = pl_averages[p.FTp]*((float) pl_stats[p.FTM]/pl_games);
            pl_averages[p.RPG] = (float) (pl_stats[p.OREB] + pl_stats[p.DREB])/pl_games;
            pl_averages[p.ORPG] = (float) pl_stats[p.OREB]/pl_games;
            pl_averages[p.DRPG] = (float) pl_stats[p.DREB]/pl_games;
            pl_averages[p.SPG] = (float) pl_stats[p.STL]/pl_games;
            pl_averages[p.BPG] = (float) pl_stats[p.BLK]/pl_games;
            pl_averages[p.TPG] = (float) pl_stats[p.TOS]/pl_games;
            pl_averages[p.APG] = (float) pl_stats[p.AST]/pl_games;
            pl_averages[p.FPG] = (float) pl_stats[p.FOUL]/pl_games;
        }

        /// <summary>
        ///     Calculates the Metric Stats for this Player
        /// </summary>
        /// <param name="ts">The player's team's stats</param>
        /// <param name="tsopp">The player's team's opponents' stats</param>
        /// <param name="ls">The total league stats</param>
        /// <param name="leagueOv">Whether CalcMetrics is being called from the League Overview screen</param>
        public void CalcMetrics(TeamStats ts, TeamStats tsopp, TeamStats ls, bool leagueOv = false, bool GmScOnly = false,
                                bool playoffs = false)
        {
            var pstats = new double[stats.Length];
            for (int i = 0; i < stats.Length; i++)
            {
                if (!playoffs)
                    pstats[i] = stats[i];
                else
                    pstats[i] = pl_stats[i];
            }

            var tstats = new double[ts.stats.Length];
            for (int i = 0; i < ts.stats.Length; i++)
            {
                if (!playoffs)
                    tstats[i] = ts.stats[i];
                else
                    tstats[i] = ts.pl_stats[i];
            }

            var toppstats = new double[tsopp.stats.Length];
            for (int i = 0; i < tsopp.stats.Length; i++)
            {
                if (!playoffs)
                    toppstats[i] = tsopp.stats[i];
                else
                    toppstats[i] = tsopp.pl_stats[i];
            }

            var lstats = new double[ls.stats.Length];
            for (int i = 0; i < ls.stats.Length; i++)
            {
                if (!playoffs)
                    lstats[i] = ls.stats[i];
                else
                    lstats[i] = ls.pl_stats[i];
            }


            double pREB = pstats[p.OREB] + pstats[p.DREB];
            double tREB = tstats[t.OREB] + tstats[t.DREB];

            var temp_metrics = new Dictionary<string, double>();

            double GmSc = pstats[p.PTS] + 0.4*pstats[p.FGM] - 0.7*pstats[p.FGA] - 0.4*(pstats[p.FTA] - pstats[p.FTM]) + 0.7*pstats[p.OREB] +
                          0.3*pstats[p.DREB] + pstats[p.STL] + 0.7*pstats[p.AST] + 0.7*pstats[p.BLK] - 0.4*pstats[p.FOUL] - pstats[p.TOS];
            temp_metrics.Add("GmSc", GmSc/pstats[p.GP]);

            double GmScE = 36*(1/pstats[p.MINS])*GmSc;
            temp_metrics.Add("GmScE", GmScE);

            if (!GmScOnly)
            {
                #region temp_metrics that do not require Opponent Stats

                double ASTp = pstats[p.AST]/(((pstats[p.MINS]/(tstats[t.MINS]))*tstats[t.FGM]) - pstats[p.FGM]);
                temp_metrics.Add("AST%", ASTp);

                double EFGp = (pstats[p.FGM] + 0.5*pstats[p.TPM])/pstats[p.FGA];
                temp_metrics.Add("EFG%", EFGp);

                Dictionary<string, double> toppmetrics;
                if (!playoffs)
                    toppmetrics = tsopp.metrics;
                else
                    toppmetrics = tsopp.pl_metrics;

                double STLp = (pstats[p.STL]*(tstats[t.MINS]))/(pstats[p.MINS]*toppmetrics["Poss"]);
                temp_metrics.Add("STL%", STLp);

                double TOp = pstats[p.TOS]/(pstats[p.FGA] + 0.44*pstats[p.FTA] + pstats[p.TOS]);
                temp_metrics.Add("TO%", TOp);

                double TSp = pstats[p.PTS]/(2*(pstats[p.FGA] + 0.44*pstats[p.FTA]));
                temp_metrics.Add("TS%", TSp);

                double USGp = ((pstats[p.FGA] + 0.44*pstats[p.FTA] + pstats[p.TOS])*(tstats[t.MINS]))/
                              (pstats[p.MINS]*(tstats[t.FGA] + 0.44*tstats[t.FTA] + tstats[t.TOS]));
                temp_metrics.Add("USG%", USGp);

                CalculateRates(pstats, ref temp_metrics);
                // PER preparations
                double lREB = lstats[t.OREB] + lstats[t.DREB];
                double factor = (2/3) - (0.5*(lstats[t.AST]/lstats[t.FGM]))/(2*(lstats[t.FGM]/lstats[t.FTM]));
                double VOP = lstats[t.PF]/(lstats[t.FGA] - lstats[t.OREB] + lstats[t.TOS] + 0.44*lstats[t.FTA]);
                double lDRBp = lstats[t.DREB]/lREB;

                double uPER = (1/pstats[p.MINS])*
                              (pstats[p.TPM] + (2/3)*pstats[p.AST] + (2 - factor*(tstats[t.AST]/tstats[t.FGM]))*pstats[p.FGM] +
                               (pstats[p.FTM]*0.5*(1 + (1 - (tstats[t.AST]/tstats[t.FGM])) + (2/3)*(tstats[t.AST]/tstats[t.FGM]))) -
                               VOP*pstats[p.TOS] - VOP*lDRBp*(pstats[p.FGA] - pstats[p.FGM]) -
                               VOP*0.44*(0.44 + (0.56*lDRBp))*(pstats[p.FTA] - pstats[p.FTM]) + VOP*(1 - lDRBp)*(pREB - pstats[p.OREB]) +
                               VOP*lDRBp*pstats[p.OREB] + VOP*pstats[p.STL] + VOP*lDRBp*pstats[p.BLK] -
                               pstats[p.FOUL]*((lstats[t.FTM]/lstats[t.FOUL]) - 0.44*(lstats[t.FTA]/lstats[t.FOUL])*VOP));
                temp_metrics.Add("EFF", uPER*100);

                #endregion

                #region temp_metrics that require Opponents stats

                if (ts.getGames() == tsopp.getGames())
                {
                    double BLKp = (pstats[p.BLK]*(tstats[t.MINS]))/(pstats[p.MINS]*(toppstats[t.FGA] - toppstats[t.TPA]));

                    double DRBp = (pstats[p.DREB]*(tstats[t.MINS]))/(pstats[p.MINS]*(tstats[t.DREB] + toppstats[t.OREB]));

                    double ORBp = (pstats[p.OREB]*(tstats[t.MINS]))/(pstats[p.MINS]*(tstats[t.OREB] + toppstats[t.DREB]));

                    double toppREB = toppstats[t.OREB] + toppstats[t.DREB];

                    double REBp = (pREB*(tstats[t.MINS]))/(pstats[p.MINS]*(tREB + toppREB));

                    #region temp_metrics that require league stats

                    double aPER;
                    double PPR;

                    if (ls.name != "$$Empty")
                    {
                        //double paceAdj = ls.temp_metrics["Pace"]/ts.temp_metrics["Pace"];
                        double estPaceAdj;
                        if (!playoffs)
                            estPaceAdj = 2*ls.averages[t.PPG]/(ts.averages[t.PPG] + tsopp.averages[t.PPG]);
                        else
                            estPaceAdj = 2*ls.pl_averages[t.PPG]/(ts.pl_averages[t.PPG] + tsopp.pl_averages[t.PPG]);

                        aPER = estPaceAdj*uPER;

                        PPR = 100*estPaceAdj*(((pstats[p.AST]*2/3) - pstats[p.TOS])/pstats[p.MINS]);
                    }
                    else
                    {
                        aPER = Double.NaN;
                        PPR = Double.NaN;
                    }

                    #endregion

                    temp_metrics.Add("aPER", aPER);
                    temp_metrics.Add("BLK%", BLKp);
                    temp_metrics.Add("DREB%", DRBp);
                    temp_metrics.Add("OREB%", ORBp);
                    temp_metrics.Add("REB%", REBp);
                    temp_metrics.Add("PPR", PPR);
                }
                else
                {
                    temp_metrics.Add("aPER", Double.NaN);
                    temp_metrics.Add("BLK%", Double.NaN);
                    temp_metrics.Add("DREB%", Double.NaN);
                    temp_metrics.Add("OREB%", Double.NaN);
                    temp_metrics.Add("REB%", Double.NaN);
                    temp_metrics.Add("PPR", Double.NaN);
                }

                #endregion
            }

            uint games = (!playoffs) ? ts.getGames() : ts.getPlayoffGames();

            var gamesRequired = (int) Math.Ceiling(0.8522*games);
            if (leagueOv)
            {
                if (pstats[p.GP] < gamesRequired)
                {
                    foreach (string name in temp_metrics.Keys.ToList())
                        temp_metrics[name] = Double.NaN;
                }
            }

            if (!playoffs)
                metrics = new Dictionary<string, double>(temp_metrics);
            else
                pl_metrics = new Dictionary<string, double>(temp_metrics);
        }

        public static void CalculateRates(double[] pstats, ref Dictionary<string, double> temp_metrics)
        {
            double pREB = pstats[p.OREB] + pstats[p.DREB];

            // Rates, stat per 36 minutes played
            double PTSR = (pstats[p.PTS]/pstats[p.MINS])*36;
            temp_metrics.Remove("PTSR");
            temp_metrics.Add("PTSR", PTSR);

            double REBR = (pREB/pstats[p.MINS])*36;
            temp_metrics.Remove("REBR");
            temp_metrics.Add("REBR", REBR);

            double OREBR = (pstats[p.OREB]/pstats[p.MINS])*36;
            temp_metrics.Remove("OREBR");
            temp_metrics.Add("OREBR", OREBR);

            double ASTR = (pstats[p.AST]/pstats[p.MINS])*36;
            temp_metrics.Remove("ASTR");
            temp_metrics.Add("ASTR", ASTR);

            double BLKR = (pstats[p.BLK]/pstats[p.MINS])*36;
            temp_metrics.Remove("BLKR");
            temp_metrics.Add("BLKR", BLKR);

            double STLR = (pstats[p.STL]/pstats[p.MINS])*36;
            temp_metrics.Remove("STLR");
            temp_metrics.Add("STLR", STLR);

            double TOR = (pstats[p.TOS]/pstats[p.MINS])*36;
            temp_metrics.Remove("TOR");
            temp_metrics.Add("TOR", TOR);

            double FTR = (pstats[p.FTM]/pstats[p.FGA]);
            temp_metrics.Remove("FTR");
            temp_metrics.Add("FTR", FTR);

            double FTAR = (pstats[p.FTA]/pstats[p.MINS])*36;
            temp_metrics.Remove("FTAR");
            temp_metrics.Add("FTAR", FTAR);
            //
        }

        /// <summary>
        ///     Calculates the PER.
        /// </summary>
        /// <param name="lg_aPER">The league average PER.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the PER is calculated for the player's playoff stats.
        /// </param>
        public void CalcPER(double lg_aPER, bool playoffs = false)
        {
            try
            {
                if (!playoffs)
                    metrics.Add("PER", metrics["aPER"]*(15/lg_aPER));
                else
                    pl_metrics.Add("PER", pl_metrics["aPER"]*(15/lg_aPER));
            }
            catch (Exception)
            {
                if (!playoffs)
                    metrics.Add("PER", Double.NaN);
                else
                    pl_metrics.Add("PER", Double.NaN);
            }
        }

        /// <summary>
        ///     Adds a player's box score to their stats.
        /// </summary>
        /// <param name="pbs">The Player Box Score.</param>
        /// <param name="isPlayoff">
        ///     if set to <c>true</c>, the stats are added to the playoff stats.
        /// </param>
        /// <exception cref="System.Exception">Occurs when the player IDs from the stats and box score do not match.</exception>
        public void AddBoxScore(PlayerBoxScore pbs, bool isPlayoff = false)
        {
            if (ID != pbs.PlayerID)
                throw new Exception("Tried to update PlayerStats " + ID + " with PlayerBoxScore " + pbs.PlayerID);

            if (!isPlayoff)
            {
                if (pbs.isStarter)
                    stats[p.GS]++;
                if (pbs.MINS > 0)
                {
                    stats[p.GP]++;
                    stats[p.MINS] += pbs.MINS;
                }
                stats[p.PTS] += pbs.PTS;
                stats[p.FGM] += pbs.FGM;
                stats[p.FGA] += pbs.FGA;
                stats[p.TPM] += pbs.TPM;
                stats[p.TPA] += pbs.TPA;
                stats[p.FTM] += pbs.FTM;
                stats[p.FTA] += pbs.FTA;
                stats[p.OREB] += pbs.OREB;
                stats[p.DREB] += pbs.DREB;
                stats[p.STL] += pbs.STL;
                stats[p.TOS] += pbs.TOS;
                stats[p.BLK] += pbs.BLK;
                stats[p.AST] += pbs.AST;
                stats[p.FOUL] += pbs.FOUL;
            }
            else
            {
                if (pbs.isStarter)
                    pl_stats[p.GS]++;
                if (pbs.MINS > 0)
                {
                    pl_stats[p.GP]++;
                    pl_stats[p.MINS] += pbs.MINS;
                }
                pl_stats[p.PTS] += pbs.PTS;
                pl_stats[p.FGM] += pbs.FGM;
                pl_stats[p.FGA] += pbs.FGA;
                pl_stats[p.TPM] += pbs.TPM;
                pl_stats[p.TPA] += pbs.TPA;
                pl_stats[p.FTM] += pbs.FTM;
                pl_stats[p.FTA] += pbs.FTA;
                pl_stats[p.OREB] += pbs.OREB;
                pl_stats[p.DREB] += pbs.DREB;
                pl_stats[p.STL] += pbs.STL;
                pl_stats[p.TOS] += pbs.TOS;
                pl_stats[p.BLK] += pbs.BLK;
                pl_stats[p.AST] += pbs.AST;
                pl_stats[p.FOUL] += pbs.FOUL;
            }

            CalcAvg();
        }

        /// <summary>
        ///     Adds the player stats from a PlayerStats instance to the current stats.
        /// </summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="addBothToSeasonStats">
        ///     if set to <c>true</c>, both season and playoff stats will be added to the season stats.
        /// </param>
        public void AddPlayerStats(PlayerStats ps, bool addBothToSeasonStats = false)
        {
            if (!addBothToSeasonStats)
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    stats[i] += ps.stats[i];
                }

                for (int i = 0; i < pl_stats.Length; i++)
                {
                    pl_stats[i] += ps.pl_stats[i];
                }
            }
            else
            {
                for (int i = 0; i < stats.Length; i++)
                {
                    stats[i] += ps.stats[i];
                }

                for (int i = 0; i < pl_stats.Length; i++)
                {
                    stats[i] += ps.pl_stats[i];
                }
            }

            CalcAvg();
        }

        /// <summary>
        ///     Resets the stats.
        /// </summary>
        public void ResetStats()
        {
            for (int i = 0; i < stats.Length; i++)
            {
                stats[i] = 0;
            }

            for (int i = 0; i < pl_stats.Length; i++)
            {
                pl_stats[i] = 0;
            }

            metrics.Clear();

            CalcAvg();
        }

        /// <summary>
        ///     Calculates the league averages.
        /// </summary>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="teamStats">The team stats.</param>
        /// <returns></returns>
        public static PlayerStats CalculateLeagueAverages(Dictionary<int, PlayerStats> playerStats, Dictionary<int, TeamStats> teamStats)
        {
            var lps = new PlayerStats(new Player(-1, "", "League", "Averages", Position.None, Position.None));
            foreach (int key in playerStats.Keys)
            {
                lps.AddPlayerStats(playerStats[key]);
            }

            var ls = new TeamStats("League");
            for (int i = 0; i < teamStats.Count; i++)
            {
                ls.AddTeamStats(teamStats[i], Span.Season);
                ls.AddTeamStats(teamStats[i], Span.Playoffs);
            }
            ls.CalcMetrics(ls);
            ls.CalcMetrics(ls, true);
            lps.CalcMetrics(ls, ls, ls, true);
            lps.CalcMetrics(ls, ls, ls, true, playoffs: true);

            var playerCount = (uint) playerStats.Count;
            for (int i = 0; i < lps.stats.Length; i++)
            {
                lps.stats[i] /= playerCount;
                lps.pl_stats[i] /= playerCount;
            }
            //ps.CalcAvg();
            return lps;
        }

        /// <summary>
        ///     Calculates all metrics.
        /// </summary>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="teamStats">The team stats.</param>
        /// <param name="oppStats">The opposing team stats.</param>
        /// <param name="TeamOrder">The team order.</param>
        /// <param name="leagueOv">
        ///     set to <c>true</c> if calling from the LeagueOverview window.
        /// </param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the metrics will be calculated for the playoff stats.
        /// </param>
        public static void CalculateAllMetrics(ref Dictionary<int, PlayerStats> playerStats, Dictionary<int, TeamStats> teamStats,
                                               Dictionary<int, TeamStats> oppStats, SortedDictionary<string, int> TeamOrder,
                                               bool leagueOv = false, bool playoffs = false)
        {
            int tCount = teamStats.Count;

            var ls = new TeamStats();
            for (int i = 0; i < tCount; i++)
            {
                if (!playoffs)
                {
                    ls.AddTeamStats(teamStats[i], Span.Season);
                    teamStats[i].CalcMetrics(oppStats[i]);
                    oppStats[i].CalcMetrics(teamStats[i]);
                }
                else
                {
                    ls.AddTeamStats(teamStats[i], Span.Playoffs);
                    teamStats[i].CalcMetrics(oppStats[i], true);
                    oppStats[i].CalcMetrics(teamStats[i], true);
                }
            }
            ls.CalcMetrics(ls, playoffs);

            double lg_aPER = 0;
            double pl_lg_aPER = 0;
            double totalMins = 0;
            double pl_totalMins = 0;

            foreach (int playerid in playerStats.Keys.ToList())
            {
                if (String.IsNullOrEmpty(playerStats[playerid].TeamF))
                    continue;

                int teamid = TeamOrder[playerStats[playerid].TeamF];
                TeamStats ts = teamStats[teamid];
                TeamStats tsopp = oppStats[teamid];

                playerStats[playerid].CalcMetrics(ts, tsopp, ls, leagueOv, playoffs: playoffs);
                if (!playoffs)
                {
                    if (!(Double.IsNaN(playerStats[playerid].metrics["aPER"])))
                    {
                        lg_aPER += playerStats[playerid].metrics["aPER"]*playerStats[playerid].stats[p.MINS];
                        totalMins += playerStats[playerid].stats[p.MINS];
                    }
                }
                else
                {
                    if (!(Double.IsNaN(playerStats[playerid].pl_metrics["aPER"])))
                    {
                        pl_lg_aPER += playerStats[playerid].pl_metrics["aPER"]*playerStats[playerid].pl_stats[p.MINS];
                        pl_totalMins += playerStats[playerid].pl_stats[p.MINS];
                    }
                }
            }
            if (!playoffs)
                lg_aPER /= totalMins;
            else
                pl_lg_aPER /= pl_totalMins;

            foreach (int playerid in playerStats.Keys.ToList())
            {
                if (String.IsNullOrEmpty(playerStats[playerid].TeamF))
                    continue;

                if (!playoffs)
                    playerStats[playerid].CalcPER(lg_aPER);
                else
                    playerStats[playerid].CalcPER(pl_lg_aPER, true);
            }
        }

        public static void CalculateRates(uint[] pstats, ref Dictionary<string, double> tempMetrics)
        {
            var pstats_d = new double[pstats.Length];
            for (int i = 0; i < pstats.Length; i++)
            {
                pstats_d[i] = Convert.ToDouble(pstats[i]);
            }

            CalculateRates(pstats_d, ref tempMetrics);
        }

        public void UpdateContract(PlayerStatsRow psr)
        {
            Contract.Option = psr.ContractOption;
            Contract.ContractSalaryPerYear.Clear();
            for (int i = 1; i <= 7; i++)
            {
                int salary = Convert.ToInt32(typeof (PlayerStatsRow).GetProperty("ContractY" + i).GetValue(psr, null));
                if (salary == 0)
                {
                    break;
                }

                Contract.ContractSalaryPerYear.Add(salary);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public PlayerStats ConvertToMyLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            PlayerStatsRow newpsr = new PlayerStatsRow(this, playoffs, calcRatings: false).ConvertToMyLeagueLeader(teamStats, playoffs);
            return new PlayerStats(newpsr, playoffs);
        }

        public PlayerStats ConvertToLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            PlayerStatsRow newpsr = new PlayerStatsRow(this, playoffs, calcRatings: false).ConvertToLeagueLeader(teamStats, playoffs);
            return new PlayerStats(newpsr, playoffs);

            /*
            string team = ps.TeamF;
            TeamStats ts = teamStats[MainWindow.TeamOrder[team]];
            uint gamesTeam = (!playoffs) ? ts.getGames() : ts.getPlayoffGames();
            uint gamesPlayed = (!playoffs) ? ps.stats[p.GP] : ps.pl_stats[p.GP];
            PlayerStats newps = ps.Clone();
            
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

            var tempstats = (!playoffs) ? ps.stats : ps.pl_stats;
            var newavg = (!playoffs) ? newps.averages : newps.pl_averages;

            if (tempstats[p.FGM] < fgmRequired)
                newavg[p.FGp] = float.NaN;
            if (tempstats[p.TPM] < tpmRequired)
                newavg[p.TPp] = float.NaN;
            if (tempstats[p.FTM] < ftmRequired)
                newavg[p.FTp] = float.NaN;

            if (gamesPlayed >= gamesRequired)
            {
                return newps;
            }

            if (tempstats[p.PTS] < ptsRequired)
                newavg[p.PPG] = float.NaN;
            if ((tempstats[p.DREB] + tempstats[p.OREB]) < rebRequired)
                newavg[p.RPG] = float.NaN;
            if (tempstats[p.AST] < astRequired)
                newavg[p.APG] = float.NaN;
            if (tempstats[p.STL] < stlRequired)
                newavg[p.SPG] = float.NaN;
            if (tempstats[p.BLK] < blkRequired)
                newavg[p.BPG] = float.NaN;
            if (tempstats[p.MINS] < minRequired)
                newavg[p.MPG] = float.NaN;
            return newps;
            */
        }
    }
}