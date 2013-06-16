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
        public Dictionary<string, double> Metrics = new Dictionary<string, double>(PAbbr.MetricsNames.Count);
        public float[] PerGame = new float[16];
        public Dictionary<string, double> PlMetrics = new Dictionary<string, double>(PAbbr.MetricsNames.Count);
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

            Metrics = new Dictionary<string, double>(PAbbr.MetricsDict);
            PlMetrics = new Dictionary<string, double>(Metrics);
            /*
            var metricsNames = PAbbr.MetricsNames;
            for (int i = 0; i < metricsNames.Count; i++)
            {
                var name = metricsNames[i];
                Metrics.Add(name, double.NaN);
                PlMetrics.Add(name, double.NaN);
            }
            */
            /*
            PAbbr.MetricsNames.ForEach(name =>
                                       {
                                           Metrics.Add(name, double.NaN);
                                           PlMetrics.Add(name, double.NaN);
                                       });
            */
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
                    Totals[PAbbr.GP] = ParseCell.GetUInt16(dataRow, "GP");
                    Totals[PAbbr.GS] = ParseCell.GetUInt16(dataRow, "GS");
                    Totals[PAbbr.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                    Totals[PAbbr.PTS] = ParseCell.GetUInt16(dataRow, "PTS");

                    var parts = ParseCell.GetString(dataRow, "FG").Split('-');

                    Totals[PAbbr.FGM] = Convert.ToUInt16(parts[0]);
                    Totals[PAbbr.FGA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "3PT").Split('-');

                    Totals[PAbbr.TPM] = Convert.ToUInt16(parts[0]);
                    Totals[PAbbr.TPA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "FT").Split('-');

                    Totals[PAbbr.FTM] = Convert.ToUInt16(parts[0]);
                    Totals[PAbbr.FTA] = Convert.ToUInt16(parts[1]);

                    Totals[PAbbr.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                    Totals[PAbbr.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                    Totals[PAbbr.STL] = ParseCell.GetUInt16(dataRow, "STL");
                    Totals[PAbbr.TOS] = ParseCell.GetUInt16(dataRow, "TO");
                    Totals[PAbbr.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                    Totals[PAbbr.AST] = ParseCell.GetUInt16(dataRow, "AST");
                    Totals[PAbbr.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
                }
                else
                {
                    PlTotals[PAbbr.GP] = ParseCell.GetUInt16(dataRow, "GP");
                    PlTotals[PAbbr.GS] = ParseCell.GetUInt16(dataRow, "GS");
                    PlTotals[PAbbr.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                    PlTotals[PAbbr.PTS] = ParseCell.GetUInt16(dataRow, "PTS");

                    var parts = ParseCell.GetString(dataRow, "FG").Split('-');

                    PlTotals[PAbbr.FGM] = Convert.ToUInt16(parts[0]);
                    PlTotals[PAbbr.FGA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "3PT").Split('-');

                    PlTotals[PAbbr.TPM] = Convert.ToUInt16(parts[0]);
                    PlTotals[PAbbr.TPA] = Convert.ToUInt16(parts[1]);

                    parts = ParseCell.GetString(dataRow, "FT").Split('-');

                    PlTotals[PAbbr.FTM] = Convert.ToUInt16(parts[0]);
                    PlTotals[PAbbr.FTA] = Convert.ToUInt16(parts[1]);

                    PlTotals[PAbbr.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                    PlTotals[PAbbr.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                    PlTotals[PAbbr.STL] = ParseCell.GetUInt16(dataRow, "STL");
                    PlTotals[PAbbr.TOS] = ParseCell.GetUInt16(dataRow, "TO");
                    PlTotals[PAbbr.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                    PlTotals[PAbbr.AST] = ParseCell.GetUInt16(dataRow, "AST");
                    PlTotals[PAbbr.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
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
                Totals[PAbbr.GP] = playerStatsRow.GP;
                Totals[PAbbr.GS] = playerStatsRow.GS;
                Totals[PAbbr.MINS] = playerStatsRow.MINS;
                Totals[PAbbr.PTS] = playerStatsRow.PTS;
                Totals[PAbbr.FGM] = playerStatsRow.FGM;
                Totals[PAbbr.FGA] = playerStatsRow.FGA;
                Totals[PAbbr.TPM] = playerStatsRow.TPM;
                Totals[PAbbr.TPA] = playerStatsRow.TPA;
                Totals[PAbbr.FTM] = playerStatsRow.FTM;
                Totals[PAbbr.FTA] = playerStatsRow.FTA;
                Totals[PAbbr.OREB] = playerStatsRow.OREB;
                Totals[PAbbr.DREB] = playerStatsRow.DREB;
                Totals[PAbbr.STL] = playerStatsRow.STL;
                Totals[PAbbr.TOS] = playerStatsRow.TOS;
                Totals[PAbbr.BLK] = playerStatsRow.BLK;
                Totals[PAbbr.AST] = playerStatsRow.AST;
                Totals[PAbbr.FOUL] = playerStatsRow.FOUL;

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
                PlTotals[PAbbr.GP] = playerStatsRow.GP;
                PlTotals[PAbbr.GS] = playerStatsRow.GS;
                PlTotals[PAbbr.MINS] = playerStatsRow.MINS;
                PlTotals[PAbbr.PTS] = playerStatsRow.PTS;
                PlTotals[PAbbr.FGM] = playerStatsRow.FGM;
                PlTotals[PAbbr.FGA] = playerStatsRow.FGA;
                PlTotals[PAbbr.TPM] = playerStatsRow.TPM;
                PlTotals[PAbbr.TPA] = playerStatsRow.TPA;
                PlTotals[PAbbr.FTM] = playerStatsRow.FTM;
                PlTotals[PAbbr.FTA] = playerStatsRow.FTA;
                PlTotals[PAbbr.OREB] = playerStatsRow.OREB;
                PlTotals[PAbbr.DREB] = playerStatsRow.DREB;
                PlTotals[PAbbr.STL] = playerStatsRow.STL;
                PlTotals[PAbbr.TOS] = playerStatsRow.TOS;
                PlTotals[PAbbr.BLK] = playerStatsRow.BLK;
                PlTotals[PAbbr.AST] = playerStatsRow.AST;
                PlTotals[PAbbr.FOUL] = playerStatsRow.FOUL;

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
            CareerHighs[PAbbr.MINS] = Convert.ToUInt16(r["MINS"].ToString());
            CareerHighs[PAbbr.PTS] = Convert.ToUInt16(r["PTS"].ToString());
            CareerHighs[PAbbr.REB] = Convert.ToUInt16(r["REB"].ToString());
            CareerHighs[PAbbr.AST] = Convert.ToUInt16(r["AST"].ToString());
            CareerHighs[PAbbr.STL] = Convert.ToUInt16(r["STL"].ToString());
            CareerHighs[PAbbr.BLK] = Convert.ToUInt16(r["BLK"].ToString());
            CareerHighs[PAbbr.TOS] = Convert.ToUInt16(r["TOS"].ToString());
            CareerHighs[PAbbr.FGM] = Convert.ToUInt16(r["FGM"].ToString());
            CareerHighs[PAbbr.FGA] = Convert.ToUInt16(r["FGA"].ToString());
            CareerHighs[PAbbr.TPM] = Convert.ToUInt16(r["TPM"].ToString());
            CareerHighs[PAbbr.TPA] = Convert.ToUInt16(r["TPA"].ToString());
            CareerHighs[PAbbr.FTM] = Convert.ToUInt16(r["FTM"].ToString());
            CareerHighs[PAbbr.FTA] = Convert.ToUInt16(r["FTA"].ToString());
            CareerHighs[PAbbr.OREB] = Convert.ToUInt16(r["OREB"].ToString());
            CareerHighs[PAbbr.FOUL] = Convert.ToUInt16(r["FOUL"].ToString());
            CareerHighs[PAbbr.DREB] = Convert.ToUInt16(r["DREB"].ToString());
        }

        public void UpdateCareerHighs(PlayerHighsRow phr)
        {
            if (phr.PlayerID != ID)
            {
                throw new Exception(
                    "Tried to update Career Highs of Player with ID " + ID + " with career highs of player with ID " + phr.PlayerID);
            }
            CareerHighs[PAbbr.MINS] = phr.MINS;
            CareerHighs[PAbbr.PTS] = phr.PTS;
            CareerHighs[PAbbr.FGM] = phr.FGM;
            CareerHighs[PAbbr.FGA] = phr.FGA;
            CareerHighs[PAbbr.TPM] = phr.TPM;
            CareerHighs[PAbbr.TPA] = phr.TPA;
            CareerHighs[PAbbr.FTM] = phr.FTM;
            CareerHighs[PAbbr.FTA] = phr.FTA;
            CareerHighs[PAbbr.REB] = phr.REB;
            CareerHighs[PAbbr.OREB] = phr.OREB;
            CareerHighs[PAbbr.DREB] = phr.DREB;
            CareerHighs[PAbbr.STL] = phr.STL;
            CareerHighs[PAbbr.TOS] = phr.TOS;
            CareerHighs[PAbbr.BLK] = phr.BLK;
            CareerHighs[PAbbr.AST] = phr.AST;
            CareerHighs[PAbbr.FOUL] = phr.FOUL;
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
                sh[PAbbr.AST] = seasonPBSList.Select(pbs => pbs.AST).Max();
                sh[PAbbr.BLK] = seasonPBSList.Select(pbs => pbs.BLK).Max();
                sh[PAbbr.DREB] = seasonPBSList.Select(pbs => pbs.DREB).Max();
                sh[PAbbr.OREB] = seasonPBSList.Select(pbs => pbs.OREB).Max();
                sh[PAbbr.REB] = seasonPBSList.Select(pbs => pbs.REB).Max();
                sh[PAbbr.STL] = seasonPBSList.Select(pbs => pbs.STL).Max();
                sh[PAbbr.TOS] = seasonPBSList.Select(pbs => pbs.TOS).Max();
                sh[PAbbr.FOUL] = seasonPBSList.Select(pbs => pbs.FOUL).Max();
                sh[PAbbr.FGM] = seasonPBSList.Select(pbs => pbs.FGM).Max();
                sh[PAbbr.FGA] = seasonPBSList.Select(pbs => pbs.FGA).Max();
                sh[PAbbr.TPM] = seasonPBSList.Select(pbs => pbs.TPM).Max();
                sh[PAbbr.TPA] = seasonPBSList.Select(pbs => pbs.TPA).Max();
                sh[PAbbr.FTM] = seasonPBSList.Select(pbs => pbs.FTM).Max();
                sh[PAbbr.FTA] = seasonPBSList.Select(pbs => pbs.FTA).Max();
                sh[PAbbr.PTS] = seasonPBSList.Select(pbs => pbs.PTS).Max();
                sh[PAbbr.MINS] = seasonPBSList.Select(pbs => pbs.MINS).Max();

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
                Totals[PAbbr.GP] = ParseCell.GetUInt16(dataRow, "GP");
                Totals[PAbbr.GS] = ParseCell.GetUInt16(dataRow, "GS");
                Totals[PAbbr.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                Totals[PAbbr.PTS] = ParseCell.GetUInt16(dataRow, "PTS");
                Totals[PAbbr.FGM] = ParseCell.GetUInt16(dataRow, "FGM");
                Totals[PAbbr.FGA] = ParseCell.GetUInt16(dataRow, "FGA");
                Totals[PAbbr.TPM] = ParseCell.GetUInt16(dataRow, "TPM");
                Totals[PAbbr.TPA] = ParseCell.GetUInt16(dataRow, "TPA");
                Totals[PAbbr.FTM] = ParseCell.GetUInt16(dataRow, "FTM");
                Totals[PAbbr.FTA] = ParseCell.GetUInt16(dataRow, "FTA");
                Totals[PAbbr.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                Totals[PAbbr.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                Totals[PAbbr.STL] = ParseCell.GetUInt16(dataRow, "STL");
                Totals[PAbbr.TOS] = ParseCell.GetUInt16(dataRow, "TOS");
                Totals[PAbbr.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                Totals[PAbbr.AST] = ParseCell.GetUInt16(dataRow, "AST");
                Totals[PAbbr.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
            }
            else
            {
                PlTotals[PAbbr.GP] = ParseCell.GetUInt16(dataRow, "GP");
                PlTotals[PAbbr.GS] = ParseCell.GetUInt16(dataRow, "GS");
                PlTotals[PAbbr.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
                PlTotals[PAbbr.PTS] = ParseCell.GetUInt16(dataRow, "PTS");
                PlTotals[PAbbr.FGM] = ParseCell.GetUInt16(dataRow, "FGM");
                PlTotals[PAbbr.FGA] = ParseCell.GetUInt16(dataRow, "FGA");
                PlTotals[PAbbr.TPM] = ParseCell.GetUInt16(dataRow, "TPM");
                PlTotals[PAbbr.TPA] = ParseCell.GetUInt16(dataRow, "TPA");
                PlTotals[PAbbr.FTM] = ParseCell.GetUInt16(dataRow, "FTM");
                PlTotals[PAbbr.FTA] = ParseCell.GetUInt16(dataRow, "FTA");
                PlTotals[PAbbr.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
                PlTotals[PAbbr.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
                PlTotals[PAbbr.STL] = ParseCell.GetUInt16(dataRow, "STL");
                PlTotals[PAbbr.TOS] = ParseCell.GetUInt16(dataRow, "TOS");
                PlTotals[PAbbr.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
                PlTotals[PAbbr.AST] = ParseCell.GetUInt16(dataRow, "AST");
                PlTotals[PAbbr.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");
            }

            CalcAvg();
        }

        /// <summary>Updates the playoff stats.</summary>
        /// <param name="dataRow">The data row containing the playoff stats.</param>
        public void UpdatePlayoffStats(DataRow dataRow)
        {
            PlTotals[PAbbr.GP] = ParseCell.GetUInt16(dataRow, "GP");
            PlTotals[PAbbr.GS] = ParseCell.GetUInt16(dataRow, "GS");
            PlTotals[PAbbr.MINS] = ParseCell.GetUInt16(dataRow, "MINS");
            PlTotals[PAbbr.PTS] = ParseCell.GetUInt16(dataRow, "PTS");
            PlTotals[PAbbr.FGM] = ParseCell.GetUInt16(dataRow, "FGM");
            PlTotals[PAbbr.FGA] = ParseCell.GetUInt16(dataRow, "FGA");
            PlTotals[PAbbr.TPM] = ParseCell.GetUInt16(dataRow, "TPM");
            PlTotals[PAbbr.TPA] = ParseCell.GetUInt16(dataRow, "TPA");
            PlTotals[PAbbr.FTM] = ParseCell.GetUInt16(dataRow, "FTM");
            PlTotals[PAbbr.FTA] = ParseCell.GetUInt16(dataRow, "FTA");
            PlTotals[PAbbr.OREB] = ParseCell.GetUInt16(dataRow, "OREB");
            PlTotals[PAbbr.DREB] = ParseCell.GetUInt16(dataRow, "DREB");
            PlTotals[PAbbr.STL] = ParseCell.GetUInt16(dataRow, "STL");
            PlTotals[PAbbr.TOS] = ParseCell.GetUInt16(dataRow, "TOS");
            PlTotals[PAbbr.BLK] = ParseCell.GetUInt16(dataRow, "BLK");
            PlTotals[PAbbr.AST] = ParseCell.GetUInt16(dataRow, "AST");
            PlTotals[PAbbr.FOUL] = ParseCell.GetUInt16(dataRow, "FOUL");

            CalcAvg(true);
        }

        /// <summary>Updates the playoff stats.</summary>
        /// <param name="pl_psr">The Playoffs PlayerStatsRow instance to get the stats from.</param>
        public void UpdatePlayoffStats(PlayerStatsRow pl_psr)
        {
            PlTotals[PAbbr.GP] = pl_psr.GP;
            PlTotals[PAbbr.GS] = pl_psr.GS;
            PlTotals[PAbbr.MINS] = pl_psr.MINS;
            PlTotals[PAbbr.PTS] = pl_psr.PTS;
            PlTotals[PAbbr.FGM] = pl_psr.FGM;
            PlTotals[PAbbr.FGA] = pl_psr.FGA;
            PlTotals[PAbbr.TPM] = pl_psr.TPM;
            PlTotals[PAbbr.TPA] = pl_psr.TPA;
            PlTotals[PAbbr.FTM] = pl_psr.FTM;
            PlTotals[PAbbr.FTA] = pl_psr.FTA;
            PlTotals[PAbbr.OREB] = pl_psr.OREB;
            PlTotals[PAbbr.DREB] = pl_psr.DREB;
            PlTotals[PAbbr.STL] = pl_psr.STL;
            PlTotals[PAbbr.TOS] = pl_psr.TOS;
            PlTotals[PAbbr.BLK] = pl_psr.BLK;
            PlTotals[PAbbr.AST] = pl_psr.AST;
            PlTotals[PAbbr.FOUL] = pl_psr.FOUL;

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
                var games = Totals[PAbbr.GP];
                PerGame[PAbbr.MPG] = (float) Totals[PAbbr.MINS] / games;
                PerGame[PAbbr.PPG] = (float) Totals[PAbbr.PTS] / games;
                PerGame[PAbbr.FGp] = (float) Totals[PAbbr.FGM] / Totals[PAbbr.FGA];
                PerGame[PAbbr.FGeff] = PerGame[PAbbr.FGp] * ((float) Totals[PAbbr.FGM] / games);
                PerGame[PAbbr.TPp] = (float) Totals[PAbbr.TPM] / Totals[PAbbr.TPA];
                PerGame[PAbbr.TPeff] = PerGame[PAbbr.TPp] * ((float) Totals[PAbbr.TPM] / games);
                PerGame[PAbbr.FTp] = (float) Totals[PAbbr.FTM] / Totals[PAbbr.FTA];
                PerGame[PAbbr.FTeff] = PerGame[PAbbr.FTp] * ((float) Totals[PAbbr.FTM] / games);
                PerGame[PAbbr.RPG] = (float) (Totals[PAbbr.OREB] + Totals[PAbbr.DREB]) / games;
                PerGame[PAbbr.ORPG] = (float) Totals[PAbbr.OREB] / games;
                PerGame[PAbbr.DRPG] = (float) Totals[PAbbr.DREB] / games;
                PerGame[PAbbr.SPG] = (float) Totals[PAbbr.STL] / games;
                PerGame[PAbbr.BPG] = (float) Totals[PAbbr.BLK] / games;
                PerGame[PAbbr.TPG] = (float) Totals[PAbbr.TOS] / games;
                PerGame[PAbbr.APG] = (float) Totals[PAbbr.AST] / games;
                PerGame[PAbbr.FPG] = (float) Totals[PAbbr.FOUL] / games;
            }

            var plGames = PlTotals[PAbbr.GP];
            PlPerGame[PAbbr.MPG] = (float) PlTotals[PAbbr.MINS] / plGames;
            PlPerGame[PAbbr.PPG] = (float) PlTotals[PAbbr.PTS] / plGames;
            PlPerGame[PAbbr.FGp] = (float) PlTotals[PAbbr.FGM] / PlTotals[PAbbr.FGA];
            PlPerGame[PAbbr.FGeff] = PlPerGame[PAbbr.FGp] * ((float) PlTotals[PAbbr.FGM] / plGames);
            PlPerGame[PAbbr.TPp] = (float) PlTotals[PAbbr.TPM] / PlTotals[PAbbr.TPA];
            PlPerGame[PAbbr.TPeff] = PlPerGame[PAbbr.TPp] * ((float) PlTotals[PAbbr.TPM] / plGames);
            PlPerGame[PAbbr.FTp] = (float) PlTotals[PAbbr.FTM] / PlTotals[PAbbr.FTA];
            PlPerGame[PAbbr.FTeff] = PlPerGame[PAbbr.FTp] * ((float) PlTotals[PAbbr.FTM] / plGames);
            PlPerGame[PAbbr.RPG] = (float) (PlTotals[PAbbr.OREB] + PlTotals[PAbbr.DREB]) / plGames;
            PlPerGame[PAbbr.ORPG] = (float) PlTotals[PAbbr.OREB] / plGames;
            PlPerGame[PAbbr.DRPG] = (float) PlTotals[PAbbr.DREB] / plGames;
            PlPerGame[PAbbr.SPG] = (float) PlTotals[PAbbr.STL] / plGames;
            PlPerGame[PAbbr.BPG] = (float) PlTotals[PAbbr.BLK] / plGames;
            PlPerGame[PAbbr.TPG] = (float) PlTotals[PAbbr.TOS] / plGames;
            PlPerGame[PAbbr.APG] = (float) PlTotals[PAbbr.AST] / plGames;
            PlPerGame[PAbbr.FPG] = (float) PlTotals[PAbbr.FOUL] / plGames;
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

            var pREB = pstats[PAbbr.OREB] + pstats[PAbbr.DREB];
            var tREB = tstats[TAbbr.OREB] + tstats[TAbbr.DREB];

            var tempMetrics = new Dictionary<string, double>();

            var gmSc = pstats[PAbbr.PTS] + 0.4 * pstats[PAbbr.FGM] - 0.7 * pstats[PAbbr.FGA]
                       - 0.4 * (pstats[PAbbr.FTA] - pstats[PAbbr.FTM]) + 0.7 * pstats[PAbbr.OREB] + 0.3 * pstats[PAbbr.DREB]
                       + pstats[PAbbr.STL] + 0.7 * pstats[PAbbr.AST] + 0.7 * pstats[PAbbr.BLK] - 0.4 * pstats[PAbbr.FOUL]
                       - pstats[PAbbr.TOS];
            tempMetrics.Add("GmSc", gmSc / pstats[PAbbr.GP]);

            var gmScE = 36 * (1 / pstats[PAbbr.MINS]) * gmSc;
            tempMetrics.Add("GmScE", gmScE);

            if (!GmScOnly)
            {
                #region temp_metrics that do not require Opponent Stats

                var ASTp = pstats[PAbbr.AST] / (((pstats[PAbbr.MINS] / (tstats[TAbbr.MINS])) * tstats[TAbbr.FGM]) - pstats[PAbbr.FGM]);
                tempMetrics.Add("AST%", ASTp);

                var EFGp = (pstats[PAbbr.FGM] + 0.5 * pstats[PAbbr.TPM]) / pstats[PAbbr.FGA];
                tempMetrics.Add("EFG%", EFGp);

                var tempOppMetrics = !playoffs ? tsopp.Metrics : tsopp.PlMetrics;

                var STLp = (pstats[PAbbr.STL] * (tstats[TAbbr.MINS])) / (pstats[PAbbr.MINS] * tempOppMetrics["Poss"]);
                tempMetrics.Add("STL%", STLp);

                var TOp = pstats[PAbbr.TOS] / (pstats[PAbbr.FGA] + 0.44 * pstats[PAbbr.FTA] + pstats[PAbbr.TOS]);
                tempMetrics.Add("TO%", TOp);

                var TSp = pstats[PAbbr.PTS] / (2 * (pstats[PAbbr.FGA] + 0.44 * pstats[PAbbr.FTA]));
                tempMetrics.Add("TS%", TSp);

                var USGp = ((pstats[PAbbr.FGA] + 0.44 * pstats[PAbbr.FTA] + pstats[PAbbr.TOS]) * (tstats[TAbbr.MINS]))
                           / (pstats[PAbbr.MINS] * (tstats[TAbbr.FGA] + 0.44 * tstats[TAbbr.FTA] + tstats[TAbbr.TOS]));
                tempMetrics.Add("USG%", USGp);

                calculateRates(pstats, ref tempMetrics);
                // PER preparations
                var lREB = lstats[TAbbr.OREB] + lstats[TAbbr.DREB];
                var factor = (2 / 3) - (0.5 * (lstats[TAbbr.AST] / lstats[TAbbr.FGM])) / (2 * (lstats[TAbbr.FGM] / lstats[TAbbr.FTM]));
                var VOP = lstats[TAbbr.PF] / (lstats[TAbbr.FGA] - lstats[TAbbr.OREB] + lstats[TAbbr.TOS] + 0.44 * lstats[TAbbr.FTA]);
                var lDRBp = lstats[TAbbr.DREB] / lREB;

                var uPER = (1 / pstats[PAbbr.MINS])
                           * (pstats[PAbbr.TPM] + (2 / 3) * pstats[PAbbr.AST]
                              + (2 - factor * (tstats[TAbbr.AST] / tstats[TAbbr.FGM])) * pstats[PAbbr.FGM]
                              + (pstats[PAbbr.FTM] * 0.5
                                 * (1 + (1 - (tstats[TAbbr.AST] / tstats[TAbbr.FGM]))
                                    + (2 / 3) * (tstats[TAbbr.AST] / tstats[TAbbr.FGM]))) - VOP * pstats[PAbbr.TOS]
                              - VOP * lDRBp * (pstats[PAbbr.FGA] - pstats[PAbbr.FGM])
                              - VOP * 0.44 * (0.44 + (0.56 * lDRBp)) * (pstats[PAbbr.FTA] - pstats[PAbbr.FTM])
                              + VOP * (1 - lDRBp) * (pREB - pstats[PAbbr.OREB]) + VOP * lDRBp * pstats[PAbbr.OREB]
                              + VOP * pstats[PAbbr.STL] + VOP * lDRBp * pstats[PAbbr.BLK]
                              - pstats[PAbbr.FOUL]
                              * ((lstats[TAbbr.FTM] / lstats[TAbbr.FOUL]) - 0.44 * (lstats[TAbbr.FTA] / lstats[TAbbr.FOUL]) * VOP));
                tempMetrics.Add("EFF", uPER * 100);

                #endregion

                #region temp_metrics that require Opponents stats

                if (ts.GetGames() == tsopp.GetGames())
                {
                    var BLKp = (pstats[PAbbr.BLK] * (tstats[TAbbr.MINS]))
                               / (pstats[PAbbr.MINS] * (toppstats[TAbbr.FGA] - toppstats[TAbbr.TPA]));

                    var DRBp = (pstats[PAbbr.DREB] * (tstats[TAbbr.MINS]))
                               / (pstats[PAbbr.MINS] * (tstats[TAbbr.DREB] + toppstats[TAbbr.OREB]));

                    var ORBp = (pstats[PAbbr.OREB] * (tstats[TAbbr.MINS]))
                               / (pstats[PAbbr.MINS] * (tstats[TAbbr.OREB] + toppstats[TAbbr.DREB]));

                    var toppREB = toppstats[TAbbr.OREB] + toppstats[TAbbr.DREB];

                    var REBp = (pREB * (tstats[TAbbr.MINS])) / (pstats[PAbbr.MINS] * (tREB + toppREB));

                    #region temp_metrics that require league stats

                    double aPER;
                    double PPR;

                    if (ls.Name != "$$Empty")
                    {
                        //double paceAdj = ls.temp_metrics["Pace"]/ts.temp_metrics["Pace"];
                        double estPaceAdj;
                        if (!playoffs)
                        {
                            estPaceAdj = 2 * ls.PerGame[TAbbr.PPG] / (ts.PerGame[TAbbr.PPG] + tsopp.PerGame[TAbbr.PPG]);
                        }
                        else
                        {
                            estPaceAdj = 2 * ls.PlPerGame[TAbbr.PPG] / (ts.PlPerGame[TAbbr.PPG] + tsopp.PlPerGame[TAbbr.PPG]);
                        }

                        aPER = estPaceAdj * uPER;

                        PPR = 100 * estPaceAdj * (((pstats[PAbbr.AST] * 2 / 3) - pstats[PAbbr.TOS]) / pstats[PAbbr.MINS]);
                    }
                    else
                    {
                        aPER = Double.NaN;
                        PPR = Double.NaN;
                    }

                    #endregion

                    tempMetrics.Add("aPER", aPER);
                    tempMetrics.Add("BLK%", BLKp);
                    tempMetrics.Add("DREB%", DRBp);
                    tempMetrics.Add("OREB%", ORBp);
                    tempMetrics.Add("REB%", REBp);
                    tempMetrics.Add("PPR", PPR);
                }
                else
                {
                    tempMetrics.Add("aPER", Double.NaN);
                    tempMetrics.Add("BLK%", Double.NaN);
                    tempMetrics.Add("DREB%", Double.NaN);
                    tempMetrics.Add("OREB%", Double.NaN);
                    tempMetrics.Add("REB%", Double.NaN);
                    tempMetrics.Add("PPR", Double.NaN);
                }

                #endregion
            }

            var games = (!playoffs) ? ts.GetGames() : ts.GetPlayoffGames();

            var gamesRequired = (int) Math.Ceiling(0.8522 * games);
            if (leagueOv)
            {
                if (pstats[PAbbr.GP] < gamesRequired)
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
            var pREB = pstats[PAbbr.OREB] + pstats[PAbbr.DREB];

            // Rates, stat per 36 minutes played
            var PTSR = (pstats[PAbbr.PTS] / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("PTSR");
            tempMetrics.Add("PTSR", PTSR);

            var REBR = (pREB / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("REBR");
            tempMetrics.Add("REBR", REBR);

            var OREBR = (pstats[PAbbr.OREB] / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("OREBR");
            tempMetrics.Add("OREBR", OREBR);

            var ASTR = (pstats[PAbbr.AST] / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("ASTR");
            tempMetrics.Add("ASTR", ASTR);

            var BLKR = (pstats[PAbbr.BLK] / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("BLKR");
            tempMetrics.Add("BLKR", BLKR);

            var STLR = (pstats[PAbbr.STL] / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("STLR");
            tempMetrics.Add("STLR", STLR);

            var TOR = (pstats[PAbbr.TOS] / pstats[PAbbr.MINS]) * 36;
            tempMetrics.Remove("TOR");
            tempMetrics.Add("TOR", TOR);

            var FTR = (pstats[PAbbr.FTM] / pstats[PAbbr.FGA]);
            tempMetrics.Remove("FTR");
            tempMetrics.Add("FTR", FTR);

            var FTAR = (pstats[PAbbr.FTA] / pstats[PAbbr.MINS]) * 36;
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
                    Metrics.Add("PER", Metrics["aPER"] * (15 / lgAvgPER));
                }
                else
                {
                    PlMetrics.Add("PER", PlMetrics["aPER"] * (15 / lgAvgPER));
                }
            }
            catch (Exception)
            {
                if (!playoffs)
                {
                    Metrics.Add("PER", Double.NaN);
                }
                else
                {
                    PlMetrics.Add("PER", Double.NaN);
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
                    Totals[PAbbr.GS]++;
                }
                if (pbs.MINS > 0)
                {
                    Totals[PAbbr.GP]++;
                    Totals[PAbbr.MINS] += pbs.MINS;
                }
                Totals[PAbbr.PTS] += pbs.PTS;
                Totals[PAbbr.FGM] += pbs.FGM;
                Totals[PAbbr.FGA] += pbs.FGA;
                Totals[PAbbr.TPM] += pbs.TPM;
                Totals[PAbbr.TPA] += pbs.TPA;
                Totals[PAbbr.FTM] += pbs.FTM;
                Totals[PAbbr.FTA] += pbs.FTA;
                Totals[PAbbr.OREB] += pbs.OREB;
                Totals[PAbbr.DREB] += pbs.DREB;
                Totals[PAbbr.STL] += pbs.STL;
                Totals[PAbbr.TOS] += pbs.TOS;
                Totals[PAbbr.BLK] += pbs.BLK;
                Totals[PAbbr.AST] += pbs.AST;
                Totals[PAbbr.FOUL] += pbs.FOUL;
            }
            else
            {
                if (pbs.IsStarter)
                {
                    PlTotals[PAbbr.GS]++;
                }
                if (pbs.MINS > 0)
                {
                    PlTotals[PAbbr.GP]++;
                    PlTotals[PAbbr.MINS] += pbs.MINS;
                }
                PlTotals[PAbbr.PTS] += pbs.PTS;
                PlTotals[PAbbr.FGM] += pbs.FGM;
                PlTotals[PAbbr.FGA] += pbs.FGA;
                PlTotals[PAbbr.TPM] += pbs.TPM;
                PlTotals[PAbbr.TPA] += pbs.TPA;
                PlTotals[PAbbr.FTM] += pbs.FTM;
                PlTotals[PAbbr.FTA] += pbs.FTA;
                PlTotals[PAbbr.OREB] += pbs.OREB;
                PlTotals[PAbbr.DREB] += pbs.DREB;
                PlTotals[PAbbr.STL] += pbs.STL;
                PlTotals[PAbbr.TOS] += pbs.TOS;
                PlTotals[PAbbr.BLK] += pbs.BLK;
                PlTotals[PAbbr.AST] += pbs.AST;
                PlTotals[PAbbr.FOUL] += pbs.FOUL;
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
            lps.CalcMetrics(ls, ls, ls, true);
            lps.CalcMetrics(ls, ls, ls, true, playoffs: true);

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
                        lgAvgPER += playerStats[playerid].Metrics["aPER"] * playerStats[playerid].Totals[PAbbr.MINS];
                        totalMins += playerStats[playerid].Totals[PAbbr.MINS];
                    }
                }
                else
                {
                    if (!(Double.IsNaN(playerStats[playerid].PlMetrics["aPER"]))
                        && !(Double.IsInfinity(playerStats[playerid].PlMetrics["aPER"])))
                    {
                        plLgAvgPER += playerStats[playerid].PlMetrics["aPER"] * playerStats[playerid].PlTotals[PAbbr.MINS];
                        plTotalMins += playerStats[playerid].PlTotals[PAbbr.MINS];
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