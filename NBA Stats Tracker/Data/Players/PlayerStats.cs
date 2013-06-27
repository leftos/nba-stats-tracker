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
    using System.Data;
    using System.Linq;
    using System.Windows;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.Other;
    using NBA_Stats_Tracker.Data.Players.Contracts;
    using NBA_Stats_Tracker.Data.Players.Injuries;
    using NBA_Stats_Tracker.Data.Teams;
    using NBA_Stats_Tracker.Windows.MainInterface;

    #endregion

    /// <summary>A container for all of a player's information, stats, PerGame and metrics handled by the program.</summary>
    [Serializable]
    public class PlayerStats : INotifyPropertyChanged
    {
        public ushort[] CareerHighs = new ushort[18];
        public PlayerContract Contract;
        public string FirstName;
        public double Height;
        public int ID;
        public PlayerInjury Injury;

        public bool IsAllStar;

        public bool IsNBAChampion;
        public string LastName;
        public Dictionary<string, double> Metrics = new Dictionary<string, double>(PlayerStatsHelper.MetricsNames.Count);
        public float[] PerGame = new float[16];
        public Dictionary<string, double> PlMetrics = new Dictionary<string, double>(PlayerStatsHelper.MetricsNames.Count);
        public float[] PlPerGame = new float[16];
        public uint[] PlTotals = new uint[17];
        public Position Position1;
        public Position Position2;
        public int TeamF;
        public int TeamS = -1;
        public uint[] Totals = new uint[17];
        public double Weight;
        public int YearOfBirth;
        public int YearsPro;
        private bool _isHidden;
        private bool _isSigned;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        public PlayerStats()
        {
            for (var i = 0; i < Totals.Length; i++)
            {
                Totals[i] = 0;
            }

            for (var i = 0; i < PerGame.Length; i++)
            {
                PerGame[i] = 0;
            }

            for (var i = 0; i < PlTotals.Length; i++)
            {
                PlTotals[i] = 0;
            }

            for (var i = 0; i < PlPerGame.Length; i++)
            {
                PlPerGame[i] = 0;
            }

            for (var i = 0; i < CareerHighs.Length; i++)
            {
                CareerHighs[i] = 0;
            }

            Contract = new PlayerContract();
            IsSigned = false;
            IsHidden = false;
            Injury = new PlayerInjury();
            IsAllStar = false;
            IsNBAChampion = false;
            YearOfBirth = 0;
            YearsPro = 0;
            Height = 0;
            Weight = 0;
            TeamF = -1;
            TeamS = -1;

            Metrics = new Dictionary<string, double>(PlayerStatsHelper.MetricsDict);
            PlMetrics = new Dictionary<string, double>(Metrics);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="player">A Player instance containing the information to initialize with.</param>
        /// <param name="fromAddScreen">Determines whether the instance is being created from the AddWindow.</param>
        public PlayerStats(Player player, bool fromAddScreen = false)
            : this()
        {
            ID = player.ID;
            LastName = player.LastName;
            FirstName = player.FirstName;
            Position1 = player.Position1;
            Position2 = player.Position2;
            TeamF = player.Team;
            IsSigned = TeamF != -1;
            if (!fromAddScreen)
            {
                Height = Convert.ToDouble(player.Height);
                Weight = player.Weight;
            }
            else
            {
                Height = MainWindow.IsImperial
                             ? PlayerStatsRow.ConvertImperialHeightToMetric(player.Height)
                             : Convert.ToDouble(player.Height);
                Weight = MainWindow.IsImperial ? Weight : PlayerStatsRow.ConvertMetricWeightToImperial(player.Weight);
            }
            YearOfBirth = player.YearOfBirth;
            YearsPro = player.YearsPro;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="dataRow">A row of an SQLite query result containing player information.</param>
        /// <param name="tst">The Team Stats dictionary from which to add additional information.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the row is assumed to contain playoff stats.
        /// </param>
        public PlayerStats(DataRow dataRow, Dictionary<int, TeamStats> tst, bool playoffs = false)
            : this()
        {
            ID = ParseCell.GetInt32(dataRow, "ID");

            if (!playoffs)
            {
                LastName = ParseCell.GetString(dataRow, "LastName");
                FirstName = ParseCell.GetString(dataRow, "FirstName");
                var p1 = ParseCell.GetString(dataRow, "Position1");
                if (String.IsNullOrWhiteSpace(p1))
                {
                    Position1 = Position.None;
                }
                else
                {
                    Position1 = (Position) Enum.Parse(typeof(Position), p1);
                }
                var p2 = ParseCell.GetString(dataRow, "Position2");
                if (String.IsNullOrWhiteSpace(p2))
                {
                    Position2 = Position.None;
                }
                else
                {
                    Position2 = (Position) Enum.Parse(typeof(Position), p2);
                }
                try
                {
                    TeamF = ParseCell.GetInt32(dataRow, "TeamFin");
                    TeamS = ParseCell.GetInt32(dataRow, "TeamSta");
                }
                catch (FormatException)
                {
                    try
                    {
                        TeamF = tst.Single(ts => ts.Value.Name == ParseCell.GetString(dataRow, "TeamFin")).Value.ID;
                    }
                    catch (InvalidOperationException)
                    {
                        TeamF = -1;
                    }
                    try
                    {
                        TeamS = tst.Single(ts => ts.Value.Name == ParseCell.GetString(dataRow, "TeamSta")).Value.ID;
                    }
                    catch (InvalidOperationException)
                    {
                        TeamS = -1;
                    }
                }
                IsSigned = ParseCell.GetBoolean(dataRow, "isActive");

                // Backwards compatibility with databases that didn't have the field
                try
                {
                    IsHidden = ParseCell.GetBoolean(dataRow, "isHidden");
                }
                catch
                {
                    IsHidden = false;
                }

                try
                {
                    YearOfBirth = ParseCell.GetInt32(dataRow, "YearOfBirth");
                }
                catch
                {
                    try
                    {
                        YearOfBirth = Convert.ToInt32(MainWindow.BaseYear) - ParseCell.GetInt32(dataRow, "Age");
                    }
                    catch
                    {
                        YearOfBirth = 0;
                    }
                }

                try
                {
                    YearsPro = ParseCell.GetInt32(dataRow, "YearsPro");
                }
                catch (Exception)
                {
                    YearsPro = 0;
                }
                //

                try
                {
                    var injType = ParseCell.GetInt32(dataRow, "InjuryType");
                    var days = ParseCell.GetInt32(dataRow, "InjuryDaysLeft");
                    Injury = injType != -1
                                 ? new PlayerInjury(injType, days)
                                 : new PlayerInjury(ParseCell.GetString(dataRow, "CustomInjuryName"), days);
                }
                catch
                {
                    Injury = ParseCell.GetBoolean(dataRow, "isInjured") ? new PlayerInjury("Unknown", -1) : new PlayerInjury();
                }

                IsAllStar = ParseCell.GetBoolean(dataRow, "isAllStar");
                IsNBAChampion = ParseCell.GetBoolean(dataRow, "isNBAChampion");
                Contract = new PlayerContract();
                for (var i = 1; i <= 7; i++)
                {
                    int salary;
                    try
                    {
                        salary = ParseCell.GetInt32(dataRow, "ContractY" + i);
                    }
                    catch (ArgumentException)
                    {
                        break;
                    }
                    if (salary == 0)
                    {
                        break;
                    }

                    Contract.ContractSalaryPerYear.Add(salary);
                }
                try
                {
                    Contract.Option =
                        (PlayerContractOption) Enum.Parse(typeof(PlayerContractOption), ParseCell.GetString(dataRow, "ContractOption"));
                }
                catch (ArgumentException)
                {
                    Contract.Option = PlayerContractOption.None;
                }
                try
                {
                    Height = ParseCell.GetFloat(dataRow, "Height");
                    Weight = ParseCell.GetFloat(dataRow, "Weight");
                }
                catch (ArgumentException)
                {
                    Height = 0;
                    Weight = 0;
                }
            }

            GetStatsFromDataRow(dataRow, playoffs);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerStats" /> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="position1">The primary position.</param>
        /// <param name="position2">The secondary position.</param>
        /// <param name="yearsPro">The amount of years this player has been in the league.</param>
        /// <param name="teamF">The team the player is currently with.</param>
        /// <param name="teamS">The team the player started the season with.</param>
        /// <param name="isActive">
        ///     if set to <c>true</c> the player is currently active (i.e. signed with a team).
        /// </param>
        /// <param name="isHidden">
        ///     if set to <c>true</c> the player is hidden for this season.
        /// </param>
        /// <param name="injury">The PlayerInjury instance containing information about the player's injury, if any.</param>
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
        /// <param name="yearOfBirth">The year the player was born.</param>
        public PlayerStats(
            int id,
            string lastName,
            string firstName,
            Position position1,
            Position position2,
            int yearOfBirth,
            int yearsPro,
            int teamF,
            int teamS,
            bool isActive,
            bool isHidden,
            PlayerInjury injury,
            bool isAllStar,
            bool isNBAChampion,
            DataRow dataRow,
            bool playoffs = false)
            : this()
        {
            ID = id;
            LastName = lastName;
            FirstName = firstName;
            Position1 = position1;
            Position2 = position2;
            TeamF = teamF;
            TeamS = teamS;
            YearOfBirth = yearOfBirth;
            YearsPro = yearsPro;
            IsSigned = isActive;
            IsHidden = isHidden;
            IsAllStar = isAllStar;
            Injury = injury;
            IsNBAChampion = isNBAChampion;

            try
            {
                if (!playoffs)
                {
                    Totals[PAbbrT.GP] = ParseCell.GetUInt16(dataRow, "GP");
                    Totals[PAbbrT.GS] = ParseCell.GetUInt16(dataRow, "GS");
                    Totals[PAbbrT.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                    Totals[PAbbrT.PTS] = ParseCell.GetUInt16(dataRow, "PTS");

                    var parts = ParseCell.GetString(dataRow, "FG").Split('-');

                    Totals[PAbbrT.FGM] = Convert.ToUInt16(parts[0]);
                    Totals[PAbbrT.FGA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "3PT").Split('-');

                    Totals[PAbbrT.TPM] = Convert.ToUInt16(parts[0]);
                    Totals[PAbbrT.TPA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "FT").Split('-');

                    Totals[PAbbrT.FTM] = Convert.ToUInt16(parts[0]);
                    Totals[PAbbrT.FTA] = Convert.ToUInt16(parts[1]);

                    Totals[PAbbrT.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                    Totals[PAbbrT.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                    Totals[PAbbrT.STL] = ParseCell.GetUInt16(dataRow, "STL");
                    Totals[PAbbrT.TOS] = ParseCell.GetUInt16(dataRow, "TO");
                    Totals[PAbbrT.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                    Totals[PAbbrT.AST] = ParseCell.GetUInt16(dataRow, "AST");
                    Totals[PAbbrT.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
                }
                else
                {
                    PlTotals[PAbbrT.GP] = ParseCell.GetUInt16(dataRow, "GP");
                    PlTotals[PAbbrT.GS] = ParseCell.GetUInt16(dataRow, "GS");
                    PlTotals[PAbbrT.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                    PlTotals[PAbbrT.PTS] = ParseCell.GetUInt16(dataRow, "PTS");

                    var parts = ParseCell.GetString(dataRow, "FG").Split('-');

                    PlTotals[PAbbrT.FGM] = Convert.ToUInt16(parts[0]);
                    PlTotals[PAbbrT.FGA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "3PT").Split('-');

                    PlTotals[PAbbrT.TPM] = Convert.ToUInt16(parts[0]);
                    PlTotals[PAbbrT.TPA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "FT").Split('-');

                    PlTotals[PAbbrT.FTM] = Convert.ToUInt16(parts[0]);
                    PlTotals[PAbbrT.FTA] = Convert.ToUInt16(parts[1]);

                    PlTotals[PAbbrT.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                    PlTotals[PAbbrT.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                    PlTotals[PAbbrT.STL] = ParseCell.GetUInt16(dataRow, "STL");
                    PlTotals[PAbbrT.TOS] = ParseCell.GetUInt16(dataRow, "TO");
                    PlTotals[PAbbrT.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                    PlTotals[PAbbrT.AST] = ParseCell.GetUInt16(dataRow, "AST");
                    PlTotals[PAbbrT.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("{0} {1} ({2}) has some invalid data.\n\nError: {3}", firstName, lastName, teamF, ex.Message));
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
        public PlayerStats(PlayerStatsRow playerStatsRow, bool playoffs = false)
            : this()
        {
            LastName = playerStatsRow.LastName;
            FirstName = playerStatsRow.FirstName;

            if (!playoffs)
            {
                Totals[PAbbrT.GP] = playerStatsRow.GP;
                Totals[PAbbrT.GS] = playerStatsRow.GS;
                Totals[PAbbrT.MINS] = playerStatsRow.MINS;
                Totals[PAbbrT.PTS] = playerStatsRow.PTS;
                Totals[PAbbrT.FGM] = playerStatsRow.FGM;
                Totals[PAbbrT.FGA] = playerStatsRow.FGA;
                Totals[PAbbrT.TPM] = playerStatsRow.TPM;
                Totals[PAbbrT.TPA] = playerStatsRow.TPA;
                Totals[PAbbrT.FTM] = playerStatsRow.FTM;
                Totals[PAbbrT.FTA] = playerStatsRow.FTA;
                Totals[PAbbrT.OREB] = playerStatsRow.OREB;
                Totals[PAbbrT.DREB] = playerStatsRow.DREB;
                Totals[PAbbrT.STL] = playerStatsRow.STL;
                Totals[PAbbrT.TOS] = playerStatsRow.TOS;
                Totals[PAbbrT.BLK] = playerStatsRow.BLK;
                Totals[PAbbrT.AST] = playerStatsRow.AST;
                Totals[PAbbrT.FOUL] = playerStatsRow.FOUL;

                Metrics["GmSc"] = playerStatsRow.GmSc;
                Metrics["GmScE"] = playerStatsRow.GmScE;
                Metrics["EFF"] = playerStatsRow.EFF;
                Metrics["EFG%"] = playerStatsRow.EFGp;
                Metrics["TS%"] = playerStatsRow.TSp;
                Metrics["AST%"] = playerStatsRow.ASTp;
                Metrics["STL%"] = playerStatsRow.STLp;
                Metrics["TO%"] = playerStatsRow.TOp;
                Metrics["USG%"] = playerStatsRow.USGp;
                Metrics["PTSR"] = playerStatsRow.PTSR;
                Metrics["REBR"] = playerStatsRow.REBR;
                Metrics["OREBR"] = playerStatsRow.OREBR;
                Metrics["ASTR"] = playerStatsRow.ASTR;
                Metrics["BLKR"] = playerStatsRow.BLKR;
                Metrics["STLR"] = playerStatsRow.STLR;
                Metrics["TOR"] = playerStatsRow.TOR;
                Metrics["FTR"] = playerStatsRow.FTR;
                Metrics["PER"] = playerStatsRow.PER;
                Metrics["BLK%"] = playerStatsRow.BLKp;
                Metrics["DREB%"] = playerStatsRow.DREBp;
                Metrics["OREB%"] = playerStatsRow.OREBp;
                Metrics["REB%"] = playerStatsRow.REBp;
                Metrics["PPR"] = playerStatsRow.PPR;
            }
            else
            {
                PlTotals[PAbbrT.GP] = playerStatsRow.GP;
                PlTotals[PAbbrT.GS] = playerStatsRow.GS;
                PlTotals[PAbbrT.MINS] = playerStatsRow.MINS;
                PlTotals[PAbbrT.PTS] = playerStatsRow.PTS;
                PlTotals[PAbbrT.FGM] = playerStatsRow.FGM;
                PlTotals[PAbbrT.FGA] = playerStatsRow.FGA;
                PlTotals[PAbbrT.TPM] = playerStatsRow.TPM;
                PlTotals[PAbbrT.TPA] = playerStatsRow.TPA;
                PlTotals[PAbbrT.FTM] = playerStatsRow.FTM;
                PlTotals[PAbbrT.FTA] = playerStatsRow.FTA;
                PlTotals[PAbbrT.OREB] = playerStatsRow.OREB;
                PlTotals[PAbbrT.DREB] = playerStatsRow.DREB;
                PlTotals[PAbbrT.STL] = playerStatsRow.STL;
                PlTotals[PAbbrT.TOS] = playerStatsRow.TOS;
                PlTotals[PAbbrT.BLK] = playerStatsRow.BLK;
                PlTotals[PAbbrT.AST] = playerStatsRow.AST;
                PlTotals[PAbbrT.FOUL] = playerStatsRow.FOUL;

                PlMetrics["GmSc"] = playerStatsRow.GmSc;
                PlMetrics["GmScE"] = playerStatsRow.GmScE;
                PlMetrics["EFF"] = playerStatsRow.EFF;
                PlMetrics["EFG%"] = playerStatsRow.EFGp;
                PlMetrics["TS%"] = playerStatsRow.TSp;
                PlMetrics["AST%"] = playerStatsRow.ASTp;
                PlMetrics["STL%"] = playerStatsRow.STLp;
                PlMetrics["TO%"] = playerStatsRow.TOp;
                PlMetrics["USG%"] = playerStatsRow.USGp;
                PlMetrics["PTSR"] = playerStatsRow.PTSR;
                PlMetrics["REBR"] = playerStatsRow.REBR;
                PlMetrics["OREBR"] = playerStatsRow.OREBR;
                PlMetrics["ASTR"] = playerStatsRow.ASTR;
                PlMetrics["BLKR"] = playerStatsRow.BLKR;
                PlMetrics["STLR"] = playerStatsRow.STLR;
                PlMetrics["TOR"] = playerStatsRow.TOR;
                PlMetrics["FTR"] = playerStatsRow.FTR;
                PlMetrics["PER"] = playerStatsRow.PER;
                PlMetrics["BLK%"] = playerStatsRow.BLKp;
                PlMetrics["DREB%"] = playerStatsRow.DREBp;
                PlMetrics["OREB%"] = playerStatsRow.OREBp;
                PlMetrics["REB%"] = playerStatsRow.REBp;
                PlMetrics["PPR"] = playerStatsRow.PPR;
            }

            ID = playerStatsRow.ID;
            Position1 = playerStatsRow.Position1;
            Position2 = playerStatsRow.Position2;
            TeamF = playerStatsRow.TeamF;
            TeamS = playerStatsRow.TeamS;
            YearOfBirth = playerStatsRow.YearOfBirth;
            YearsPro = playerStatsRow.YearsPro;
            IsSigned = playerStatsRow.IsSigned;
            IsHidden = playerStatsRow.IsHidden;
            IsAllStar = playerStatsRow.IsAllStar;
            Injury = PlayerInjury.InjuryTypes.ContainsValue(playerStatsRow.InjuryName)
                         ? new PlayerInjury(
                               PlayerInjury.InjuryTypes.Single(pi => pi.Value == playerStatsRow.InjuryName).Key,
                               playerStatsRow.InjuryDaysLeft)
                         : new PlayerInjury(playerStatsRow.InjuryName, playerStatsRow.InjuryDaysLeft);
            IsNBAChampion = playerStatsRow.IsNBAChampion;

            Contract.Option = playerStatsRow.ContractOption;
            Contract.ContractSalaryPerYear.Clear();
            for (var i = 1; i <= 7; i++)
            {
                var salary = Convert.ToInt32(typeof(PlayerStatsRow).GetProperty("ContractY" + i).GetValue(playerStatsRow, null));
                if (salary == 0)
                {
                    break;
                }

                Contract.ContractSalaryPerYear.Add(salary);
            }

            Height = playerStatsRow.Height;
            Weight = playerStatsRow.Weight;

            CalcAvg();
        }

        public bool IsSigned
        {
            get { return _isSigned; }
            set
            {
                _isSigned = value;
                if (!_isSigned)
                {
                    TeamF = -1;
                }
                OnPropertyChanged("IsSigned");
            }
        }

        public bool IsHidden
        {
            get { return _isHidden; }
            set
            {
                _isHidden = value;
                if (_isHidden)
                {
                    IsSigned = false;
                }
                OnPropertyChanged("IsHidden");
            }
        }

        public string Position1S
        {
            get { return PositionToString(Position1); }
        }

        public string Position2S
        {
            get { return PositionToString(Position2); }
        }

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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public string FullInfo(IDictionary<int, TeamStats> tst, bool givenFirst = false, bool showPosition = true)
        {
            return String.Format(
                "{0} ({1}{2})",
                givenFirst ? FullNameGivenFirst : FullName,
                showPosition ? Position1S + " - " : "",
                IsSigned ? tst[TeamF].DisplayName : "Free Agent");
        }

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
            var importedID = ParseCell.GetInt32(r, "PlayerID");
            if (importedID != ID)
            {
                throw new Exception(
                    "Tried to update Career Highs of Player with ID " + ID + " with career highs of player with ID " + importedID);
            }
            CareerHighs[PAbbrT.MINS] = Convert.ToUInt16(r["MINS"].ToString());
            CareerHighs[PAbbrT.PTS] = Convert.ToUInt16(r["PTS"].ToString());
            CareerHighs[PAbbrT.REB] = Convert.ToUInt16(r["REB"].ToString());
            CareerHighs[PAbbrT.AST] = Convert.ToUInt16(r["AST"].ToString());
            CareerHighs[PAbbrT.STL] = Convert.ToUInt16(r["STL"].ToString());
            CareerHighs[PAbbrT.BLK] = Convert.ToUInt16(r["BLK"].ToString());
            CareerHighs[PAbbrT.TOS] = Convert.ToUInt16(r["TOS"].ToString());
            CareerHighs[PAbbrT.FGM] = Convert.ToUInt16(r["FGM"].ToString());
            CareerHighs[PAbbrT.FGA] = Convert.ToUInt16(r["FGA"].ToString());
            CareerHighs[PAbbrT.TPM] = Convert.ToUInt16(r["TPM"].ToString());
            CareerHighs[PAbbrT.TPA] = Convert.ToUInt16(r["TPA"].ToString());
            CareerHighs[PAbbrT.FTM] = Convert.ToUInt16(r["FTM"].ToString());
            CareerHighs[PAbbrT.FTA] = Convert.ToUInt16(r["FTA"].ToString());
            CareerHighs[PAbbrT.OREB] = Convert.ToUInt16(r["OREB"].ToString());
            CareerHighs[PAbbrT.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            CareerHighs[PAbbrT.DREB] = Convert.ToUInt16(r["DREB"].ToString());
        }

        public void UpdateCareerHighs(PlayerHighsRow phr)
        {
            if (phr.PlayerID != ID)
            {
                throw new Exception(
                    "Tried to update Career Highs of Player with ID " + ID + " with career highs of player with ID " + phr.PlayerID);
            }
            CareerHighs[PAbbrT.MINS] = phr.MINS;
            CareerHighs[PAbbrT.PTS] = phr.PTS;
            CareerHighs[PAbbrT.FGM] = phr.FGM;
            CareerHighs[PAbbrT.FGA] = phr.FGA;
            CareerHighs[PAbbrT.TPM] = phr.TPM;
            CareerHighs[PAbbrT.TPA] = phr.TPA;
            CareerHighs[PAbbrT.FTM] = phr.FTM;
            CareerHighs[PAbbrT.FTA] = phr.FTA;
            CareerHighs[PAbbrT.REB] = phr.REB;
            CareerHighs[PAbbrT.OREB] = phr.OREB;
            CareerHighs[PAbbrT.DREB] = phr.DREB;
            CareerHighs[PAbbrT.STL] = phr.STL;
            CareerHighs[PAbbrT.TOS] = phr.TOS;
            CareerHighs[PAbbrT.BLK] = phr.BLK;
            CareerHighs[PAbbrT.AST] = phr.AST;
            CareerHighs[PAbbrT.FOUL] = phr.FOUL;
        }

        public void CalculateSeasonHighs(IEnumerable<BoxScoreEntry> bsList)
        {
            var bsListWithPlayer = bsList.Where(bse => bse.PBSList.Any(pbs => pbs.PlayerID == ID)).ToList();
            var seasonsList = bsListWithPlayer.GroupBy(bse => bse.BS.SeasonNum).Select(pair => pair.Key).ToList();
            var allTimePBSList = bsListWithPlayer.Select(bse => bse.PBSList.Single(pbs => pbs.PlayerID == ID)).ToList();
            allTimePBSList.ForEach(pbs => pbs.SeasonNum = bsListWithPlayer.Single(bse => bse.BS.ID == pbs.GameID).BS.SeasonNum);

            if (MainWindow.SeasonHighs.ContainsKey(ID))
            {
                MainWindow.SeasonHighs.Remove(ID);
            }
            MainWindow.SeasonHighs.Add(ID, new Dictionary<int, ushort[]>());
            foreach (var season in seasonsList)
            {
                var seasonPBSList = allTimePBSList.Where(pbs => pbs.SeasonNum == season).ToList();
                MainWindow.SeasonHighs[ID].Add(season, new ushort[18]);
                var sh = MainWindow.SeasonHighs[ID][season];
                for (var i = 0; i < sh.Length; i++)
                {
                    sh[i] = 0;
                }
                sh[PAbbrT.AST] = seasonPBSList.Select(pbs => pbs.AST).Max();
                sh[PAbbrT.BLK] = seasonPBSList.Select(pbs => pbs.BLK).Max();
                sh[PAbbrT.DREB] = seasonPBSList.Select(pbs => pbs.DREB).Max();
                sh[PAbbrT.OREB] = seasonPBSList.Select(pbs => pbs.OREB).Max();
                sh[PAbbrT.REB] = seasonPBSList.Select(pbs => pbs.REB).Max();
                sh[PAbbrT.STL] = seasonPBSList.Select(pbs => pbs.STL).Max();
                sh[PAbbrT.TOS] = seasonPBSList.Select(pbs => pbs.TOS).Max();
                sh[PAbbrT.FOUL] = seasonPBSList.Select(pbs => pbs.FOUL).Max();
                sh[PAbbrT.FGM] = seasonPBSList.Select(pbs => pbs.FGM).Max();
                sh[PAbbrT.FGA] = seasonPBSList.Select(pbs => pbs.FGA).Max();
                sh[PAbbrT.TPM] = seasonPBSList.Select(pbs => pbs.TPM).Max();
                sh[PAbbrT.TPA] = seasonPBSList.Select(pbs => pbs.TPA).Max();
                sh[PAbbrT.FTM] = seasonPBSList.Select(pbs => pbs.FTM).Max();
                sh[PAbbrT.FTA] = seasonPBSList.Select(pbs => pbs.FTA).Max();
                sh[PAbbrT.PTS] = seasonPBSList.Select(pbs => pbs.PTS).Max();
                sh[PAbbrT.MINS] = seasonPBSList.Select(pbs => pbs.MINS).Max();

                for (var i = 0; i < sh.Length; i++)
                {
                    if (CareerHighs[i] < sh[i])
                    {
                        CareerHighs[i] = sh[i];
                    }
                }
            }
        }

        public void GetStatsFromDataRow(DataRow dataRow, bool isPlayoff)
        {
            if (!isPlayoff)
            {
                Totals[PAbbrT.GP] = ParseCell.GetUInt16(dataRow, "GP");
                Totals[PAbbrT.GS] = ParseCell.GetUInt16(dataRow, "GS");
                Totals[PAbbrT.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                Totals[PAbbrT.PTS] = ParseCell.GetUInt16(dataRow, "PTS");
                Totals[PAbbrT.FGM] = ParseCell.GetUInt16(dataRow, "FGM");
                Totals[PAbbrT.FGA] = ParseCell.GetUInt16(dataRow, "FGA");
                Totals[PAbbrT.TPM] = ParseCell.GetUInt16(dataRow, "TPM");
                Totals[PAbbrT.TPA] = ParseCell.GetUInt16(dataRow, "TPA");
                Totals[PAbbrT.FTM] = ParseCell.GetUInt16(dataRow, "FTM");
                Totals[PAbbrT.FTA] = ParseCell.GetUInt16(dataRow, "FTA");
                Totals[PAbbrT.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                Totals[PAbbrT.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                Totals[PAbbrT.STL] = ParseCell.GetUInt16(dataRow, "STL");
                Totals[PAbbrT.TOS] = ParseCell.GetUInt16(dataRow, "TOS");
                Totals[PAbbrT.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                Totals[PAbbrT.AST] = ParseCell.GetUInt16(dataRow, "AST");
                Totals[PAbbrT.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
            }
            else
            {
                PlTotals[PAbbrT.GP] = ParseCell.GetUInt16(dataRow, "GP");
                PlTotals[PAbbrT.GS] = ParseCell.GetUInt16(dataRow, "GS");
                PlTotals[PAbbrT.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                PlTotals[PAbbrT.PTS] = ParseCell.GetUInt16(dataRow, "PTS");
                PlTotals[PAbbrT.FGM] = ParseCell.GetUInt16(dataRow, "FGM");
                PlTotals[PAbbrT.FGA] = ParseCell.GetUInt16(dataRow, "FGA");
                PlTotals[PAbbrT.TPM] = ParseCell.GetUInt16(dataRow, "TPM");
                PlTotals[PAbbrT.TPA] = ParseCell.GetUInt16(dataRow, "TPA");
                PlTotals[PAbbrT.FTM] = ParseCell.GetUInt16(dataRow, "FTM");
                PlTotals[PAbbrT.FTA] = ParseCell.GetUInt16(dataRow, "FTA");
                PlTotals[PAbbrT.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                PlTotals[PAbbrT.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                PlTotals[PAbbrT.STL] = ParseCell.GetUInt16(dataRow, "STL");
                PlTotals[PAbbrT.TOS] = ParseCell.GetUInt16(dataRow, "TOS");
                PlTotals[PAbbrT.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                PlTotals[PAbbrT.AST] = ParseCell.GetUInt16(dataRow, "AST");
                PlTotals[PAbbrT.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
            }

            CalcAvg();
        }

        /// <summary>Updates the playoff stats.</summary>
        /// <param name="dataRow">The data row containing the playoff stats.</param>
        public void UpdatePlayoffStats(DataRow dataRow)
        {
            PlTotals[PAbbrT.GP] = ParseCell.GetUInt16(dataRow, "GP");
            PlTotals[PAbbrT.GS] = ParseCell.GetUInt16(dataRow, "GS");
            PlTotals[PAbbrT.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
            PlTotals[PAbbrT.PTS] = ParseCell.GetUInt16(dataRow, "PTS");
            PlTotals[PAbbrT.FGM] = ParseCell.GetUInt16(dataRow, "FGM");
            PlTotals[PAbbrT.FGA] = ParseCell.GetUInt16(dataRow, "FGA");
            PlTotals[PAbbrT.TPM] = ParseCell.GetUInt16(dataRow, "TPM");
            PlTotals[PAbbrT.TPA] = ParseCell.GetUInt16(dataRow, "TPA");
            PlTotals[PAbbrT.FTM] = ParseCell.GetUInt16(dataRow, "FTM");
            PlTotals[PAbbrT.FTA] = ParseCell.GetUInt16(dataRow, "FTA");
            PlTotals[PAbbrT.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
            PlTotals[PAbbrT.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
            PlTotals[PAbbrT.STL] = ParseCell.GetUInt16(dataRow, "STL");
            PlTotals[PAbbrT.TOS] = ParseCell.GetUInt16(dataRow, "TOS");
            PlTotals[PAbbrT.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
            PlTotals[PAbbrT.AST] = ParseCell.GetUInt16(dataRow, "AST");
            PlTotals[PAbbrT.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");

            CalcAvg(true);
        }

        /// <summary>Updates the playoff stats.</summary>
        /// <param name="pl_psr">The Playoffs PlayerStatsRow instance to get the stats from.</param>
        public void UpdatePlayoffStats(PlayerStatsRow pl_psr)
        {
            PlTotals[PAbbrT.GP] = pl_psr.GP;
            PlTotals[PAbbrT.GS] = pl_psr.GS;
            PlTotals[PAbbrT.MINS] = pl_psr.MINS;
            PlTotals[PAbbrT.PTS] = pl_psr.PTS;
            PlTotals[PAbbrT.FGM] = pl_psr.FGM;
            PlTotals[PAbbrT.FGA] = pl_psr.FGA;
            PlTotals[PAbbrT.TPM] = pl_psr.TPM;
            PlTotals[PAbbrT.TPA] = pl_psr.TPA;
            PlTotals[PAbbrT.FTM] = pl_psr.FTM;
            PlTotals[PAbbrT.FTA] = pl_psr.FTA;
            PlTotals[PAbbrT.OREB] = pl_psr.OREB;
            PlTotals[PAbbrT.DREB] = pl_psr.DREB;
            PlTotals[PAbbrT.STL] = pl_psr.STL;
            PlTotals[PAbbrT.TOS] = pl_psr.TOS;
            PlTotals[PAbbrT.BLK] = pl_psr.BLK;
            PlTotals[PAbbrT.AST] = pl_psr.AST;
            PlTotals[PAbbrT.FOUL] = pl_psr.FOUL;

            CalcAvg(true);
        }

        /// <summary>Calculates the PerGame of a player's stats.</summary>
        /// <param name="playoffsOnly">
        ///     if set to <c>true</c>, only the playoff PerGame will be calculated.
        /// </param>
        public void CalcAvg(bool playoffsOnly = false)
        {
            if (!playoffsOnly)
            {
                var games = Totals[PAbbrT.GP];
                PerGame[PAbbrPG.MPG] = (float) Totals[PAbbrT.MINS] / games;
                PerGame[PAbbrPG.PPG] = (float) Totals[PAbbrT.PTS] / games;
                PerGame[PAbbrPG.FGp] = (float) Totals[PAbbrT.FGM] / Totals[PAbbrT.FGA];
                PerGame[PAbbrPG.FGeff] = PerGame[PAbbrPG.FGp] * ((float) Totals[PAbbrT.FGM] / games);
                PerGame[PAbbrPG.TPp] = (float) Totals[PAbbrT.TPM] / Totals[PAbbrT.TPA];
                PerGame[PAbbrPG.TPeff] = PerGame[PAbbrPG.TPp] * ((float) Totals[PAbbrT.TPM] / games);
                PerGame[PAbbrPG.FTp] = (float) Totals[PAbbrT.FTM] / Totals[PAbbrT.FTA];
                PerGame[PAbbrPG.FTeff] = PerGame[PAbbrPG.FTp] * ((float) Totals[PAbbrT.FTM] / games);
                PerGame[PAbbrPG.RPG] = (float) (Totals[PAbbrT.OREB] + Totals[PAbbrT.DREB]) / games;
                PerGame[PAbbrPG.ORPG] = (float) Totals[PAbbrT.OREB] / games;
                PerGame[PAbbrPG.DRPG] = (float) Totals[PAbbrT.DREB] / games;
                PerGame[PAbbrPG.SPG] = (float) Totals[PAbbrT.STL] / games;
                PerGame[PAbbrPG.BPG] = (float) Totals[PAbbrT.BLK] / games;
                PerGame[PAbbrPG.TPG] = (float) Totals[PAbbrT.TOS] / games;
                PerGame[PAbbrPG.APG] = (float) Totals[PAbbrT.AST] / games;
                PerGame[PAbbrPG.FPG] = (float) Totals[PAbbrT.FOUL] / games;
            }

            var plGames = PlTotals[PAbbrT.GP];
            PlPerGame[PAbbrPG.MPG] = (float) PlTotals[PAbbrT.MINS] / plGames;
            PlPerGame[PAbbrPG.PPG] = (float) PlTotals[PAbbrT.PTS] / plGames;
            PlPerGame[PAbbrPG.FGp] = (float) PlTotals[PAbbrT.FGM] / PlTotals[PAbbrT.FGA];
            PlPerGame[PAbbrPG.FGeff] = PlPerGame[PAbbrPG.FGp] * ((float) PlTotals[PAbbrT.FGM] / plGames);
            PlPerGame[PAbbrPG.TPp] = (float) PlTotals[PAbbrT.TPM] / PlTotals[PAbbrT.TPA];
            PlPerGame[PAbbrPG.TPeff] = PlPerGame[PAbbrPG.TPp] * ((float) PlTotals[PAbbrT.TPM] / plGames);
            PlPerGame[PAbbrPG.FTp] = (float) PlTotals[PAbbrT.FTM] / PlTotals[PAbbrT.FTA];
            PlPerGame[PAbbrPG.FTeff] = PlPerGame[PAbbrPG.FTp] * ((float) PlTotals[PAbbrT.FTM] / plGames);
            PlPerGame[PAbbrPG.RPG] = (float) (PlTotals[PAbbrT.OREB] + PlTotals[PAbbrT.DREB]) / plGames;
            PlPerGame[PAbbrPG.ORPG] = (float) PlTotals[PAbbrT.OREB] / plGames;
            PlPerGame[PAbbrPG.DRPG] = (float) PlTotals[PAbbrT.DREB] / plGames;
            PlPerGame[PAbbrPG.SPG] = (float) PlTotals[PAbbrT.STL] / plGames;
            PlPerGame[PAbbrPG.BPG] = (float) PlTotals[PAbbrT.BLK] / plGames;
            PlPerGame[PAbbrPG.TPG] = (float) PlTotals[PAbbrT.TOS] / plGames;
            PlPerGame[PAbbrPG.APG] = (float) PlTotals[PAbbrT.AST] / plGames;
            PlPerGame[PAbbrPG.FPG] = (float) PlTotals[PAbbrT.FOUL] / plGames;
        }

        /// <summary>Calculates the Metric Stats for this Player</summary>
        /// <param name="ts">The player's team's stats</param>
        /// <param name="tsopp">The player's team's opponents' stats</param>
        /// <param name="ls">The total league stats</param>
        /// <param name="leagueOv">Whether CalcMetrics is being called from the League Overview screen</param>
        /// <param name="GmScOnly">Whether to only calculate the GmSc metric.</param>
        /// <param name="playoffs">Whether to calculate the metrics based on the playoffs stats.</param>
        public void CalcMetrics(
            TeamStats ts, TeamStats tsopp, TeamStats ls, bool leagueOv = false, bool GmScOnly = false, bool playoffs = false)
        {
            var pstats = new double[Totals.Length];
            for (var i = 0; i < Totals.Length; i++)
            {
                if (!playoffs)
                {
                    pstats[i] = Totals[i];
                }
                else
                {
                    pstats[i] = PlTotals[i];
                }
            }

            var tstats = new double[ts.Totals.Length];
            for (var i = 0; i < ts.Totals.Length; i++)
            {
                if (!playoffs)
                {
                    tstats[i] = ts.Totals[i];
                }
                else
                {
                    tstats[i] = ts.PlTotals[i];
                }
            }

            var toppstats = new double[tsopp.Totals.Length];
            for (var i = 0; i < tsopp.Totals.Length; i++)
            {
                if (!playoffs)
                {
                    toppstats[i] = tsopp.Totals[i];
                }
                else
                {
                    toppstats[i] = tsopp.PlTotals[i];
                }
            }

            var lstats = new double[ls.Totals.Length];
            for (var i = 0; i < ls.Totals.Length; i++)
            {
                if (!playoffs)
                {
                    lstats[i] = ls.Totals[i];
                }
                else
                {
                    lstats[i] = ls.PlTotals[i];
                }
            }

            var pREB = pstats[PAbbrT.OREB] + pstats[PAbbrT.DREB];
            var tREB = tstats[TAbbrT.OREB] + tstats[TAbbrT.DREB];

            var tempTeamMetricsOwn = !playoffs ? ts.Metrics : ts.PlMetrics;

            var tempMetrics = new Dictionary<string, double>(PlayerStatsHelper.MetricsDict);

            var gmSc = pstats[PAbbrT.PTS] + 0.4 * pstats[PAbbrT.FGM] - 0.7 * pstats[PAbbrT.FGA]
                       - 0.4 * (pstats[PAbbrT.FTA] - pstats[PAbbrT.FTM]) + 0.7 * pstats[PAbbrT.OREB] + 0.3 * pstats[PAbbrT.DREB]
                       + pstats[PAbbrT.STL] + 0.7 * pstats[PAbbrT.AST] + 0.7 * pstats[PAbbrT.BLK] - 0.4 * pstats[PAbbrT.FOUL]
                       - pstats[PAbbrT.TOS];
            tempMetrics["GmSc"] = gmSc / pstats[PAbbrT.GP];

            var gmScE = 36 * (1 / pstats[PAbbrT.MINS]) * gmSc;
            tempMetrics["GmScE"] = gmScE;

            if (!GmScOnly)
            {
                #region Metrics that do not require Opponent Stats

                var ASTp = pstats[PAbbrT.AST]
                           / (((pstats[PAbbrT.MINS] / (tstats[TAbbrT.MINS])) * tstats[TAbbrT.FGM]) - pstats[PAbbrT.FGM]);
                tempMetrics["AST%"] = ASTp;

                var EFGp = (pstats[PAbbrT.FGM] + 0.5 * pstats[PAbbrT.TPM]) / pstats[PAbbrT.FGA];
                tempMetrics["EFG%"] = EFGp;

                var tempTeamMetricsOpp = !playoffs ? tsopp.Metrics : tsopp.PlMetrics;

                var STLp = (pstats[PAbbrT.STL] * (tstats[TAbbrT.MINS])) / (pstats[PAbbrT.MINS] * tempTeamMetricsOpp["Poss"]);
                tempMetrics["STL%"] = STLp;

                var TOp = pstats[PAbbrT.TOS] / (pstats[PAbbrT.FGA] + 0.44 * pstats[PAbbrT.FTA] + pstats[PAbbrT.TOS]);
                tempMetrics["TO%"] = TOp;

                var TSp = pstats[PAbbrT.PTS] / (2 * (pstats[PAbbrT.FGA] + 0.44 * pstats[PAbbrT.FTA]));
                tempMetrics["TS%"] = TSp;

                var USGp = ((pstats[PAbbrT.FGA] + 0.44 * pstats[PAbbrT.FTA] + pstats[PAbbrT.TOS]) * (tstats[TAbbrT.MINS]))
                           / (pstats[PAbbrT.MINS] * (tstats[TAbbrT.FGA] + 0.44 * tstats[TAbbrT.FTA] + tstats[TAbbrT.TOS]));
                tempMetrics["USG%"] = USGp;

                calculateRates(pstats, ref tempMetrics);
                // PER preparations
                var lREB = lstats[TAbbrT.OREB] + lstats[TAbbrT.DREB];
                var factor = (2 / 3)
                             - (0.5 * (lstats[TAbbrT.AST] / lstats[TAbbrT.FGM])) / (2 * (lstats[TAbbrT.FGM] / lstats[TAbbrT.FTM]));
                var VOP = lstats[TAbbrT.PF]
                          / (lstats[TAbbrT.FGA] - lstats[TAbbrT.OREB] + lstats[TAbbrT.TOS] + 0.44 * lstats[TAbbrT.FTA]);
                var lDRBp = lstats[TAbbrT.DREB] / lREB;

                var uPER = (1 / pstats[PAbbrT.MINS])
                           * (pstats[PAbbrT.TPM] + (2 / 3) * pstats[PAbbrT.AST]
                              + (2 - factor * (tstats[TAbbrT.AST] / tstats[TAbbrT.FGM])) * pstats[PAbbrT.FGM]
                              + (pstats[PAbbrT.FTM] * 0.5
                                 * (1 + (1 - (tstats[TAbbrT.AST] / tstats[TAbbrT.FGM]))
                                    + (2 / 3) * (tstats[TAbbrT.AST] / tstats[TAbbrT.FGM]))) - VOP * pstats[PAbbrT.TOS]
                              - VOP * lDRBp * (pstats[PAbbrT.FGA] - pstats[PAbbrT.FGM])
                              - VOP * 0.44 * (0.44 + (0.56 * lDRBp)) * (pstats[PAbbrT.FTA] - pstats[PAbbrT.FTM])
                              + VOP * (1 - lDRBp) * (pREB - pstats[PAbbrT.OREB]) + VOP * lDRBp * pstats[PAbbrT.OREB]
                              + VOP * pstats[PAbbrT.STL] + VOP * lDRBp * pstats[PAbbrT.BLK]
                              - pstats[PAbbrT.FOUL]
                              * ((lstats[TAbbrT.FTM] / lstats[TAbbrT.FOUL]) - 0.44 * (lstats[TAbbrT.FTA] / lstats[TAbbrT.FOUL]) * VOP));
                tempMetrics["EFF"] = uPER * 100;

                #endregion

                #region Metrics that require Opponents stats

                if (ts.GetGames() == tsopp.GetGames())
                {
                    var BLKp = (pstats[PAbbrT.BLK] * (tstats[TAbbrT.MINS]))
                               / (pstats[PAbbrT.MINS] * (toppstats[TAbbrT.FGA] - toppstats[TAbbrT.TPA]));

                    var DRBp = (pstats[PAbbrT.DREB] * (tstats[TAbbrT.MINS]))
                               / (pstats[PAbbrT.MINS] * (tstats[TAbbrT.DREB] + toppstats[TAbbrT.OREB]));

                    var ORBp = (pstats[PAbbrT.OREB] * (tstats[TAbbrT.MINS]))
                               / (pstats[PAbbrT.MINS] * (tstats[TAbbrT.OREB] + toppstats[TAbbrT.DREB]));

                    var toppREB = toppstats[TAbbrT.OREB] + toppstats[TAbbrT.DREB];

                    var REBp = (pREB * (tstats[TAbbrT.MINS])) / (pstats[PAbbrT.MINS] * (tREB + toppREB));

                    #region Metrics that require league stats

                    double aPER;
                    double PPR;

                    if (ls.Name != "$$Empty")
                    {
                        //double paceAdj = ls.temp_metrics["Pace"]/ts.temp_metrics["Pace"];
                        double estPaceAdj;
                        if (!playoffs)
                        {
                            estPaceAdj = 2 * ls.PerGame[TAbbrPG.PPG] / (ts.PerGame[TAbbrPG.PPG] + tsopp.PerGame[TAbbrPG.PPG]);
                        }
                        else
                        {
                            estPaceAdj = 2 * ls.PlPerGame[TAbbrPG.PPG] / (ts.PlPerGame[TAbbrPG.PPG] + tsopp.PlPerGame[TAbbrPG.PPG]);
                        }

                        aPER = estPaceAdj * uPER;

                        PPR = 100 * estPaceAdj * (((pstats[PAbbrT.AST] * 2 / 3) - pstats[PAbbrT.TOS]) / pstats[PAbbrT.MINS]);
                    }
                    else
                    {
                        aPER = Double.NaN;
                        PPR = Double.NaN;
                    }

                    #endregion

                    tempMetrics["aPER"] = aPER;
                    tempMetrics["BLK%"] = BLKp;
                    tempMetrics["DREB%"] = DRBp;
                    tempMetrics["OREB%"] = ORBp;
                    tempMetrics["REB%"] = REBp;
                    tempMetrics["PPR"] = PPR;

                    #region Offensive Rating

                    var qAST = ((pstats[PAbbrT.MINS] / (tstats[TAbbrT.MINS] / 5))
                                * (1.14 * ((tstats[TAbbrT.AST] - pstats[PAbbrT.AST]) / tstats[TAbbrT.FGM])))
                               + ((((tstats[TAbbrT.AST] / tstats[TAbbrT.MINS]) * pstats[PAbbrT.MINS] * 5 - pstats[PAbbrT.AST])
                                   / ((tstats[TAbbrT.FGM] / tstats[TAbbrT.MINS]) * pstats[PAbbrT.MINS] * 5 - pstats[PAbbrT.FGM]))
                                  * (1 - (pstats[PAbbrT.MINS] / (tstats[TAbbrT.MINS] / 5))));

                    var fgPart = pstats[PAbbrT.FGM]
                                 * (1 - 0.5 * ((pstats[PAbbrT.PTS] - pstats[PAbbrT.FTM]) / (2 * pstats[PAbbrT.FGA])) * qAST);

                    var astPart = 0.5
                                  * (((tstats[TAbbrT.PF] - tstats[TAbbrT.FTM]) - (pstats[PAbbrT.PTS] - pstats[PAbbrT.FTM]))
                                     / (2 * (tstats[TAbbrT.FGA] - pstats[PAbbrT.FGA]))) * pstats[PAbbrT.AST];

                    var pFTp = pstats[PAbbrT.FTM] / pstats[PAbbrT.FTA];

                    if (double.IsNaN(pFTp))
                    {
                        pFTp = 0;
                    }

                    var ftPart = (1 - Math.Pow(1 - pFTp, 2)) * 0.4 * pstats[PAbbrT.FTA];

                    var tFTp = tstats[TAbbrT.FTM] / tstats[TAbbrT.FTA];

                    if (double.IsNaN(tFTp))
                    {
                        tFTp = 0;
                    }

                    var teamScPoss = tstats[TAbbrT.FGM] + (1 - Math.Pow(1 - tFTp, 2)) * tstats[TAbbrT.FTA] * 0.4;

                    var teamOREBPct = tstats[TAbbrT.OREB] / (tstats[TAbbrT.OREB] + toppstats[TAbbrT.DREB]);

                    var teamPlayPct = teamScPoss / (tstats[TAbbrT.FGA] + tstats[TAbbrT.FTA] * 0.4 + tstats[TAbbrT.TOS]);

                    var teamOREBWeight = ((1 - teamOREBPct) * teamPlayPct)
                                         / ((1 - teamOREBPct) * teamPlayPct + teamOREBPct * (1 - teamPlayPct));

                    var orebPart = pstats[PAbbrT.OREB] * teamOREBWeight * teamPlayPct;

                    var scPoss = (fgPart + astPart + ftPart) * (1 - (tstats[TAbbrT.OREB] / teamScPoss) * teamOREBWeight * teamPlayPct)
                                 + orebPart;

                    var fgxPoss = (pstats[PAbbrT.FGA] - pstats[PAbbrT.FGM]) * (1 - 1.07 * teamOREBPct);

                    var ftxPoss = Math.Pow(1 - pFTp, 2) * 0.4 * pstats[PAbbrT.FTA];

                    var totPoss = scPoss + fgxPoss + ftxPoss + pstats[PAbbrT.TOS];

                    var pprodFGPart = 2 * (pstats[PAbbrT.FGM] + 0.5 * pstats[PAbbrT.TPM])
                                      * (1 - 0.5 * ((pstats[PAbbrT.PTS] - pstats[PAbbrT.FTM]) / (2 * pstats[PAbbrT.FGA])) * qAST);

                    var pprodASTPart = 2
                                       * ((tstats[TAbbrT.FGM] - pstats[PAbbrT.FGM] + 0.5 * (tstats[TAbbrT.TPM] - pstats[PAbbrT.TPM]))
                                          / (tstats[TAbbrT.FGM] - pstats[PAbbrT.FGM])) * 0.5
                                       * (((tstats[TAbbrT.PF] - tstats[TAbbrT.FTM]) - (pstats[PAbbrT.PTS] - pstats[PAbbrT.FTM]))
                                          / (2 * (tstats[TAbbrT.FGA] - pstats[PAbbrT.FGA]))) * pstats[PAbbrT.AST];

                    var pprodOREBPart = pstats[PAbbrT.OREB] * teamOREBWeight * teamPlayPct
                                        * (tstats[TAbbrT.PF]
                                           / (tstats[TAbbrT.FGM] + (1 - Math.Pow(1 - tFTp, 2)) * 0.4 * tstats[TAbbrT.FTA]));

                    var pProd = (pprodFGPart + pprodASTPart + pstats[PAbbrT.FTM])
                                * (1 - (tstats[TAbbrT.OREB] / teamScPoss) * teamOREBWeight * teamPlayPct) + pprodOREBPart;

                    var ortg = 100 * (pProd / totPoss);

                    var floorPct = scPoss / totPoss;

                    tempMetrics["ORTG"] = ortg;
                    tempMetrics["Floor%"] = floorPct;

                    #endregion

                    #region Defensive Rating

                    var dorPct = toppstats[TAbbrT.OREB] / (toppstats[TAbbrT.OREB] + tstats[TAbbrT.DREB]);

                    var dfgPct = toppstats[TAbbrT.FGM] / toppstats[TAbbrT.FGA];

                    var fmWt = (dfgPct * (1 - dorPct)) / (dfgPct * (1 - dorPct) + (1 - dfgPct) * dorPct);

                    var stops1 = pstats[PAbbrT.STL] + pstats[PAbbrT.BLK] * fmWt * (1 - 1.07 * dorPct)
                                 + pstats[PAbbrT.DREB] * (1 - fmWt);

                    var stops2 = (((toppstats[TAbbrT.FGA] - toppstats[TAbbrT.FGM] - tstats[TAbbrT.BLK]) / tstats[TAbbrT.MINS]) * fmWt
                                  * (1 - 1.07 * dorPct) + ((toppstats[TAbbrT.TOS] - tstats[TAbbrT.STL]) / tstats[TAbbrT.MINS]))
                                 * pstats[PAbbrT.MINS]
                                 + (pstats[PAbbrT.PTS] / tstats[TAbbrT.PF]) * 0.4 * toppstats[TAbbrT.FTA]
                                 * Math.Pow(1 - (toppstats[TAbbrT.FTM] / toppstats[TAbbrT.FTA]), 2);

                    var stops = stops1 + stops2;

                    var stopPct = (stops * toppstats[TAbbrT.MINS]) / (tempTeamMetricsOwn["Poss"] * pstats[PAbbrT.MINS]);

                    var dPtsPerScPoss = toppstats[TAbbrT.PF]
                                        / (toppstats[TAbbrT.FGM]
                                           + (1 - Math.Pow(1 - (toppstats[TAbbrT.FTM] / toppstats[TAbbrT.FTA]), 2))
                                           * toppstats[TAbbrT.FTA] * 0.4);

                    var drtg = tempTeamMetricsOwn["DRTG"] + 0.2 * (100 * dPtsPerScPoss * (1 - stopPct) - tempTeamMetricsOwn["DRTG"]);

                    var rtgd = ortg - drtg;

                    tempMetrics["DRTG"] = drtg;
                    tempMetrics["RTGd"] = rtgd;

                    #endregion
                }
                else
                {
                    tempMetrics["aPER"] = Double.NaN;
                    tempMetrics["BLK%"] = Double.NaN;
                    tempMetrics["DREB%"] = Double.NaN;
                    tempMetrics["OREB%"] = Double.NaN;
                    tempMetrics["REB%"] = Double.NaN;
                    tempMetrics["PPR"] = Double.NaN;

                    tempMetrics["ORTG"] = Double.NaN;
                    tempMetrics["Floor%"] = Double.NaN;
                    tempMetrics["DRTG"] = Double.NaN;
                    tempMetrics["RTGd"] = Double.NaN;
                }

                #endregion
            }

            var games = (!playoffs) ? ts.GetGames() : ts.GetPlayoffGames();

            var gamesRequired = (int) Math.Ceiling(0.8522 * games);
            if (leagueOv)
            {
                if (pstats[PAbbrT.GP] < gamesRequired)
                {
                    foreach (var name in tempMetrics.Keys.ToList())
                    {
                        tempMetrics[name] = Double.NaN;
                    }
                }
            }

            if (!playoffs)
            {
                Metrics = new Dictionary<string, double>(tempMetrics);
            }
            else
            {
                PlMetrics = new Dictionary<string, double>(tempMetrics);
            }
        }

        private static void calculateRates(double[] pstats, ref Dictionary<string, double> tempMetrics)
        {
            var pREB = pstats[PAbbrT.OREB] + pstats[PAbbrT.DREB];

            // Rates, stat per 36 minutes played
            var PTSR = (pstats[PAbbrT.PTS] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("PTSR");
            tempMetrics.Add("PTSR", PTSR);

            var REBR = (pREB / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("REBR");
            tempMetrics.Add("REBR", REBR);

            var OREBR = (pstats[PAbbrT.OREB] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("OREBR");
            tempMetrics.Add("OREBR", OREBR);

            var ASTR = (pstats[PAbbrT.AST] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("ASTR");
            tempMetrics.Add("ASTR", ASTR);

            var BLKR = (pstats[PAbbrT.BLK] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("BLKR");
            tempMetrics.Add("BLKR", BLKR);

            var STLR = (pstats[PAbbrT.STL] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("STLR");
            tempMetrics.Add("STLR", STLR);

            var TOR = (pstats[PAbbrT.TOS] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("TOR");
            tempMetrics.Add("TOR", TOR);

            var FTR = (pstats[PAbbrT.FTM] / pstats[PAbbrT.FGA]);
            tempMetrics.Remove("FTR");
            tempMetrics.Add("FTR", FTR);

            var FTAR = (pstats[PAbbrT.FTA] / pstats[PAbbrT.MINS]) * 36;
            tempMetrics.Remove("FTAR");
            tempMetrics.Add("FTAR", FTAR);
            //
        }

        /// <summary>Calculates the PER.</summary>
        /// <param name="lgAvgPER">The league average PER.</param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the PER is calculated for the player's playoff stats.
        /// </param>
        private void calcPER(double lgAvgPER, bool playoffs = false)
        {
            try
            {
                if (!playoffs)
                {
                    Metrics["PER"] = Metrics["aPER"] * (15 / lgAvgPER);
                }
                else
                {
                    PlMetrics["PER"] = PlMetrics["aPER"] * (15 / lgAvgPER);
                }
            }
            catch (Exception)
            {
                if (!playoffs)
                {
                    Metrics["PER"] = Double.NaN;
                }
                else
                {
                    PlMetrics["PER"] = Double.NaN;
                }
            }
        }

        /// <summary>Adds a player's box score to their stats.</summary>
        /// <param name="pbs">The Player Box Score.</param>
        /// <param name="isPlayoff">
        ///     if set to <c>true</c>, the stats are added to the playoff stats.
        /// </param>
        /// <exception cref="System.Exception">Occurs when the player IDs from the stats and box score do not match.</exception>
        public void AddBoxScore(PlayerBoxScore pbs, bool isPlayoff = false)
        {
            if (ID != pbs.PlayerID)
            {
                throw new Exception("Tried to update PlayerStats " + ID + " with PlayerBoxScore " + pbs.PlayerID);
            }

            if (!isPlayoff)
            {
                if (pbs.IsStarter)
                {
                    Totals[PAbbrT.GS]++;
                }
                if (pbs.MINS > 0)
                {
                    Totals[PAbbrT.GP]++;
                    Totals[PAbbrT.MINS] += pbs.MINS;
                }
                Totals[PAbbrT.PTS] += pbs.PTS;
                Totals[PAbbrT.FGM] += pbs.FGM;
                Totals[PAbbrT.FGA] += pbs.FGA;
                Totals[PAbbrT.TPM] += pbs.TPM;
                Totals[PAbbrT.TPA] += pbs.TPA;
                Totals[PAbbrT.FTM] += pbs.FTM;
                Totals[PAbbrT.FTA] += pbs.FTA;
                Totals[PAbbrT.OREB] += pbs.OREB;
                Totals[PAbbrT.DREB] += pbs.DREB;
                Totals[PAbbrT.STL] += pbs.STL;
                Totals[PAbbrT.TOS] += pbs.TOS;
                Totals[PAbbrT.BLK] += pbs.BLK;
                Totals[PAbbrT.AST] += pbs.AST;
                Totals[PAbbrT.FOUL] += pbs.FOUL;
            }
            else
            {
                if (pbs.IsStarter)
                {
                    PlTotals[PAbbrT.GS]++;
                }
                if (pbs.MINS > 0)
                {
                    PlTotals[PAbbrT.GP]++;
                    PlTotals[PAbbrT.MINS] += pbs.MINS;
                }
                PlTotals[PAbbrT.PTS] += pbs.PTS;
                PlTotals[PAbbrT.FGM] += pbs.FGM;
                PlTotals[PAbbrT.FGA] += pbs.FGA;
                PlTotals[PAbbrT.TPM] += pbs.TPM;
                PlTotals[PAbbrT.TPA] += pbs.TPA;
                PlTotals[PAbbrT.FTM] += pbs.FTM;
                PlTotals[PAbbrT.FTA] += pbs.FTA;
                PlTotals[PAbbrT.OREB] += pbs.OREB;
                PlTotals[PAbbrT.DREB] += pbs.DREB;
                PlTotals[PAbbrT.STL] += pbs.STL;
                PlTotals[PAbbrT.TOS] += pbs.TOS;
                PlTotals[PAbbrT.BLK] += pbs.BLK;
                PlTotals[PAbbrT.AST] += pbs.AST;
                PlTotals[PAbbrT.FOUL] += pbs.FOUL;
            }

            CalcAvg();
        }

        /// <summary>Adds the player stats from a PlayerStats instance to the current stats.</summary>
        /// <param name="ps">The PlayerStats instance.</param>
        /// <param name="addBothToSeasonStats">
        ///     if set to <c>true</c>, both season and playoff stats will be added to the season stats.
        /// </param>
        public void AddPlayerStats(PlayerStats ps, bool addBothToSeasonStats = false)
        {
            if (!addBothToSeasonStats)
            {
                for (var i = 0; i < Totals.Length; i++)
                {
                    Totals[i] += ps.Totals[i];
                }

                for (var i = 0; i < PlTotals.Length; i++)
                {
                    PlTotals[i] += ps.PlTotals[i];
                }
            }
            else
            {
                for (var i = 0; i < Totals.Length; i++)
                {
                    Totals[i] += ps.Totals[i];
                }

                for (var i = 0; i < PlTotals.Length; i++)
                {
                    Totals[i] += ps.PlTotals[i];
                }
            }

            CalcAvg();
        }

        /// <summary>Resets the stats.</summary>
        public void ResetStats()
        {
            for (var i = 0; i < Totals.Length; i++)
            {
                Totals[i] = 0;
            }

            for (var i = 0; i < PlTotals.Length; i++)
            {
                PlTotals[i] = 0;
            }

            Metrics.Clear();

            Injury = new PlayerInjury();

            CalcAvg();
        }

        /// <summary>Calculates the league PerGame.</summary>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="teamStats">The team stats.</param>
        /// <returns></returns>
        public static PlayerStats CalculateLeagueAverages(
            Dictionary<int, PlayerStats> playerStats, Dictionary<int, TeamStats> teamStats)
        {
            var lps = new PlayerStats(new Player(-1, -1, "League", "Averages", Position.None, Position.None));
            foreach (var key in playerStats.Keys)
            {
                lps.AddPlayerStats(playerStats[key]);
            }

            var ls = new TeamStats(-2, "League");
            for (var i = 0; i < teamStats.Count; i++)
            {
                ls.AddTeamStats(teamStats[i], Span.Season);
                ls.AddTeamStats(teamStats[i], Span.Playoffs);
            }
            ls.CalcMetrics(ls);
            ls.CalcMetrics(ls, true);
            foreach (var name in PlayerStatsHelper.MetricsNames)
            {
                try
                {
                    lps.Metrics[name] =
                        playerStats.Where(ps => !Double.IsNaN(ps.Value.Metrics[name]) && !Double.IsInfinity(ps.Value.Metrics[name]))
                                   .Average(ps => ps.Value.Metrics[name]);
                }
                catch (InvalidOperationException)
                {
                    lps.Metrics[name] = Double.NaN;
                }
                try
                {
                    lps.PlMetrics[name] =
                        playerStats.Where(ps => !Double.IsNaN(ps.Value.PlMetrics[name]) && !Double.IsInfinity(ps.Value.Metrics[name]))
                                   .Average(ps => ps.Value.PlMetrics[name]);
                }
                catch (InvalidOperationException)
                {
                    lps.Metrics[name] = Double.NaN;
                }
            }

            var playerCount = (uint) playerStats.Count;
            for (var i = 0; i < lps.Totals.Length; i++)
            {
                lps.Totals[i] /= playerCount;
                lps.PlTotals[i] /= playerCount;
            }
            //ps.CalcAvg();
            return lps;
        }

        /// <summary>Calculates all metrics.</summary>
        /// <param name="playerStats">The player stats.</param>
        /// <param name="teamStats">The team stats.</param>
        /// <param name="oppStats">The opposing team stats.</param>
        /// <param name="leagueOv">
        ///     set to <c>true</c> if calling from the LeagueOverview window.
        /// </param>
        /// <param name="playoffs">
        ///     if set to <c>true</c>, the metrics will be calculated for the playoff stats.
        /// </param>
        /// <param name="teamsPerPlayer">
        ///     if set to <c>true</c>, the team stats dictionary is assumed to be per player.
        /// </param>
        public static void CalculateAllMetrics(
            ref Dictionary<int, PlayerStats> playerStats,
            Dictionary<int, TeamStats> teamStats,
            Dictionary<int, TeamStats> oppStats,
            bool leagueOv = false,
            bool playoffs = false,
            bool teamsPerPlayer = false)
        {
            var tCount = teamStats.Count;

            var ls = new TeamStats();
            var tKeys = teamStats.Keys.ToList();
            for (var i = 0; i < tCount; i++)
            {
                var key = tKeys[i];
                if (!playoffs)
                {
                    ls.AddTeamStats(teamStats[key], Span.Season);
                    teamStats[key].CalcMetrics(oppStats[key]);
                    oppStats[key].CalcMetrics(teamStats[key]);
                }
                else
                {
                    ls.AddTeamStats(teamStats[key], Span.Playoffs);
                    teamStats[key].CalcMetrics(oppStats[key], true);
                    oppStats[key].CalcMetrics(teamStats[key], true);
                }
            }
            ls.CalcMetrics(ls, playoffs);

            double lgAvgPER = 0;
            double plLgAvgPER = 0;
            double totalMins = 0;
            double plTotalMins = 0;

            foreach (var playerid in playerStats.Keys.ToList())
            {
                if (playerStats[playerid].TeamF == -1)
                {
                    continue;
                }

                var teamid = playerStats[playerid].TeamF;
                TeamStats ts;
                TeamStats tsopp;
                if (!teamsPerPlayer)
                {
                    ts = teamStats[teamid];
                    tsopp = oppStats[teamid];
                }
                else
                {
                    ts = teamStats[playerid];
                    tsopp = oppStats[playerid];
                }

                playerStats[playerid].CalcMetrics(ts, tsopp, ls, leagueOv, playoffs: playoffs);
                if (!playoffs)
                {
                    if (!(Double.IsNaN(playerStats[playerid].Metrics["aPER"]))
                        && !(Double.IsInfinity(playerStats[playerid].Metrics["aPER"])))
                    {
                        lgAvgPER += playerStats[playerid].Metrics["aPER"] * playerStats[playerid].Totals[PAbbrT.MINS];
                        totalMins += playerStats[playerid].Totals[PAbbrT.MINS];
                    }
                }
                else
                {
                    if (!(Double.IsNaN(playerStats[playerid].PlMetrics["aPER"]))
                        && !(Double.IsInfinity(playerStats[playerid].PlMetrics["aPER"])))
                    {
                        plLgAvgPER += playerStats[playerid].PlMetrics["aPER"] * playerStats[playerid].PlTotals[PAbbrT.MINS];
                        plTotalMins += playerStats[playerid].PlTotals[PAbbrT.MINS];
                    }
                }
            }
            if (!playoffs)
            {
                lgAvgPER /= totalMins;
            }
            else
            {
                plLgAvgPER /= plTotalMins;
            }

            foreach (var playerid in playerStats.Keys.ToList())
            {
                if (playerStats[playerid].TeamF == -1)
                {
                    continue;
                }

                if (!playoffs)
                {
                    playerStats[playerid].calcPER(lgAvgPER);
                }
                else
                {
                    playerStats[playerid].calcPER(plLgAvgPER, true);
                }
            }
        }

        public static void CalculateRates(uint[] pstats, ref Dictionary<string, double> tempMetrics)
        {
            var pstatsD = new double[pstats.Length];
            for (var i = 0; i < pstats.Length; i++)
            {
                pstatsD[i] = Convert.ToDouble(pstats[i]);
            }

            calculateRates(pstatsD, ref tempMetrics);
        }

        public void UpdateContract(PlayerStatsRow psr)
        {
            Contract.Option = psr.ContractOption;
            Contract.ContractSalaryPerYear.Clear();
            for (var i = 1; i <= 7; i++)
            {
                var salary = Convert.ToInt32(typeof(PlayerStatsRow).GetProperty("ContractY" + i).GetValue(psr, null));
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
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public PlayerStats ConvertToMyLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            var newpsr = new PlayerStatsRow(this, playoffs, calcRatings: false).ConvertToMyLeagueLeader(teamStats, playoffs);
            return new PlayerStats(newpsr, playoffs);
        }

        public PlayerStats ConvertToLeagueLeader(Dictionary<int, TeamStats> teamStats, bool playoffs = false)
        {
            var newpsr = new PlayerStatsRow(this, playoffs, calcRatings: false).ConvertToLeagueLeader(teamStats, playoffs);
            return new PlayerStats(newpsr, playoffs);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}