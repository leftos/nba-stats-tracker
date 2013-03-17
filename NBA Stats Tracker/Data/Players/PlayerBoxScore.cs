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

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

using LeftosCommonLibrary;

using NBA_Stats_Tracker.Data.Teams;

#endregion

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     Contains all the information of a player's performance in a game.
    /// </summary>
    [Serializable]
    public class PlayerBoxScore : INotifyPropertyChanged
    {
        private UInt16 _FGA;
        private UInt16 _FGM;
        private UInt16 _FTA;
        private UInt16 _FTM;
        private UInt16 _TPA;
        protected UInt16 _TPM;
        private UInt16 _mins;
        //public ObservableCollection<KeyValuePair<int, string>> PlayersList { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        public PlayerBoxScore()
        {
            PlayerID = -1;
            TeamID = -1;
            IsStarter = false;
            PlayedInjured = false;
            IsOut = false;
            ResetStats();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="r">The DataRow containing the player's box score.</param>
        public PlayerBoxScore(DataRow r, Dictionary<int, TeamStats> tst)
        {
            PlayerID = ParseCell.GetInt32(r, "PlayerID");
            GameID = ParseCell.GetInt32(r, "GameID");
            try
            {
                TeamID = Convert.ToInt32(r["TeamID"].ToString());
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is KeyNotFoundException)
                {
                    TeamID = tst.Single(ts => ts.Value.Name == ParseCell.GetString(r, "Team")).Value.ID;
                }
                else
                {
                    throw;
                }
            }
            IsStarter = ParseCell.GetBoolean(r, "isStarter");
            PlayedInjured = ParseCell.GetBoolean(r, "playedInjured");
            IsOut = ParseCell.GetBoolean(r, "isOut");
            MINS = Convert.ToUInt16(r["MINS"].ToString());
            PTS = Convert.ToUInt16(r["PTS"].ToString());
            REB = Convert.ToUInt16(r["REB"].ToString());
            AST = Convert.ToUInt16(r["AST"].ToString());
            STL = Convert.ToUInt16(r["STL"].ToString());
            BLK = Convert.ToUInt16(r["BLK"].ToString());
            TOS = Convert.ToUInt16(r["TOS"].ToString());
            FGM = Convert.ToUInt16(r["FGM"].ToString());
            FGA = Convert.ToUInt16(r["FGA"].ToString());
            TPM = Convert.ToUInt16(r["TPM"].ToString());
            TPA = Convert.ToUInt16(r["TPA"].ToString());
            FTM = Convert.ToUInt16(r["FTM"].ToString());
            FTA = Convert.ToUInt16(r["FTA"].ToString());
            OREB = Convert.ToUInt16(r["OREB"].ToString());
            FOUL = Convert.ToUInt16(r["FOUL"].ToString());
            DREB = (UInt16) (REB - OREB);
            FGp = (float) FGM / FGA;
            TPp = (float) TPM / TPA;
            FTp = (float) FTM / FTA;

            // Let's try to get the result and date of the game
            // Only works for INNER JOIN'ed rows
            try
            {
                int t1PTS = ParseCell.GetInt32(r, "T1PTS");
                int t2PTS = ParseCell.GetInt32(r, "T2PTS");

                int team1 = ParseCell.GetInt32(r, "Team1ID");
                int team2 = ParseCell.GetInt32(r, "Team2ID");

                if (TeamID == team1)
                {
                    if (t1PTS > t2PTS)
                    {
                        Result = "W " + t1PTS.ToString() + "-" + t2PTS.ToString();
                    }
                    else
                    {
                        Result = "L " + t1PTS.ToString() + "-" + t2PTS.ToString();
                    }

                    TeamPTS = t1PTS;
                    OppTeamID = team2;
                    OppTeamPTS = t2PTS;
                }
                else
                {
                    if (t2PTS > t1PTS)
                    {
                        Result = "W " + t2PTS.ToString() + "-" + t1PTS.ToString();
                    }
                    else
                    {
                        Result = "L " + t2PTS.ToString() + "-" + t1PTS.ToString();
                    }

                    TeamPTS = t2PTS;
                    OppTeamID = team1;
                    OppTeamPTS = t1PTS;
                }

                Date = ParseCell.GetString(r, "Date").Split(' ')[0];
                RealDate = Convert.ToDateTime(Date);
                SeasonNum = ParseCell.GetInt32(r, "SeasonNum");

                CalcMetrics(r);
            }
            catch
            {
                Console.WriteLine("Call to PlayerBoxScore constructor without inner-joined TeamBoxScore information.");
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="brRow">The Basketball-Reference.com row containing the player's box score.</param>
        /// <param name="teamID">The team.</param>
        /// <param name="gameID">The game ID.</param>
        /// <param name="starter">
        ///     if set to <c>true</c>, the player is a starter.
        /// </param>
        /// <param name="playerStats">The player stats.</param>
        public PlayerBoxScore(DataRow brRow, int teamID, int gameID, bool starter, Dictionary<int, PlayerStats> playerStats)
        {
            string[] nameParts = brRow[0].ToString().Split(new[] { ' ' }, 2);
            try
            {
                PlayerID = playerStats.Single(
                    delegate(KeyValuePair<int, PlayerStats> kvp)
                        {
                            if (kvp.Value.LastName == nameParts[1] && kvp.Value.FirstName == nameParts[0] && kvp.Value.TeamF == teamID)
                            {
                                return true;
                            }
                            return false;
                        }).Value.ID;
            }
            catch (Exception)
            {
                try
                {
                    PlayerID = playerStats.Single(
                        delegate(KeyValuePair<int, PlayerStats> kvp)
                            {
                                if (kvp.Value.LastName == nameParts[1] && kvp.Value.FirstName == nameParts[0])
                                {
                                    return true;
                                }
                                return false;
                            }).Value.ID;
                }
                catch (Exception)
                {
                    //MessageBox.Show("No player with the name " + nameParts[0] + " " + nameParts[1] + " was found in the database.");
                    PlayerID = -1;
                    return;
                }
            }

            GameID = gameID;
            TeamID = teamID;
            IsStarter = starter;
            PlayedInjured = false;
            IsOut = false;
            PTS = ParseCell.GetUInt16(brRow, "PTS");
            REB = Convert.ToUInt16(brRow["TRB"].ToString());
            AST = Convert.ToUInt16(brRow["AST"].ToString());
            STL = Convert.ToUInt16(brRow["STL"].ToString());
            BLK = Convert.ToUInt16(brRow["BLK"].ToString());
            TOS = Convert.ToUInt16(brRow["TOV"].ToString());
            FGM = Convert.ToUInt16(brRow["FG"].ToString());
            FGA = Convert.ToUInt16(brRow["FGA"].ToString());
            TPM = Convert.ToUInt16(brRow["3P"].ToString());
            TPA = Convert.ToUInt16(brRow["3PA"].ToString());
            FTM = Convert.ToUInt16(brRow["FT"].ToString());
            FTA = Convert.ToUInt16(brRow["FTA"].ToString());
            OREB = Convert.ToUInt16(brRow["ORB"].ToString());
            FOUL = Convert.ToUInt16(brRow["PF"].ToString());
            MINS = Convert.ToUInt16(brRow["MP"].ToString().Split(':')[0]);
            if (Convert.ToUInt16(brRow["MP"].ToString().Split(':')[1]) >= 30)
            {
                MINS++;
            }
            DREB = (UInt16) (REB - OREB);
            FGp = (float) FGM / FGA;
            TPp = (float) TPM / TPA;
            FTp = (float) FTM / FTA;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="dict">The dictionary containing the player box score.</param>
        /// <param name="playerID">The player ID.</param>
        /// <param name="teamID">The team.</param>
        public PlayerBoxScore(Dictionary<string, string> dict, int playerID, int teamID)
        {
            PlayerID = playerID;
            TeamID = teamID;
            IsStarter = IsStarter.TrySetValue(dict, "Starter", typeof(bool));
            PlayedInjured = PlayedInjured.TrySetValue(dict, "Injured", typeof(bool));
            IsOut = IsOut.TrySetValue(dict, "DNP", typeof(bool));
            MINS = MINS.TrySetValue(dict, "MINS", typeof(UInt16));
            PTS = PTS.TrySetValue(dict, "PTS", typeof(UInt16));
            REB = REB.TrySetValue(dict, "REB", typeof(UInt16));
            AST = AST.TrySetValue(dict, "AST", typeof(UInt16));
            STL = STL.TrySetValue(dict, "STL", typeof(UInt16));
            BLK = BLK.TrySetValue(dict, "BLK", typeof(UInt16));
            TOS = TOS.TrySetValue(dict, "TO", typeof(UInt16));
            FGM = FGM.TrySetValue(dict, "FGM", typeof(UInt16));
            FGA = FGA.TrySetValue(dict, "FGA", typeof(UInt16));
            TPM = TPM.TrySetValue(dict, "3PM", typeof(UInt16));
            TPA = TPA.TrySetValue(dict, "3PA", typeof(UInt16));
            FTM = FTM.TrySetValue(dict, "FTM", typeof(UInt16));
            FTA = FTA.TrySetValue(dict, "FTA", typeof(UInt16));
            OREB = OREB.TrySetValue(dict, "OREB", typeof(UInt16));
            FOUL = FOUL.TrySetValue(dict, "FOUL", typeof(UInt16));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        ///     Used to cast a LivePlayerBoxScore to a PlayerBoxScore which can be saved to the database.
        /// </summary>
        /// <param name="lpbs">The LivePlayerBoxScore instance containing the player's box score.</param>
        public PlayerBoxScore(LivePlayerBoxScore lpbs)
        {
            PlayerID = lpbs.PlayerID;
            Name = lpbs.Name;
            TeamID = lpbs.TeamID;
            TeamPTS = lpbs.TeamPTS;
            OppTeamID = lpbs.OppTeamID;
            OppTeamPTS = lpbs.OppTeamPTS;
            IsStarter = lpbs.IsStarter;
            PlayedInjured = lpbs.PlayedInjured;
            IsOut = lpbs.IsOut;
            GmSc = lpbs.GmSc;
            GmScE = lpbs.GmScE;
            MINS = lpbs.MINS;
            PTS = lpbs.PTS;
            FGM = lpbs.FGM;
            FGA = lpbs.FGA;
            FGp = lpbs.FGp;
            TPM = lpbs.TPM;
            TPA = lpbs.TPA;
            TPp = lpbs.TPp;
            FTM = lpbs.FTM;
            FTA = lpbs.FTA;
            FTp = lpbs.FTp;
            REB = lpbs.REB;
            OREB = lpbs.OREB;
            DREB = lpbs.DREB;
            STL = lpbs.STL;
            TOS = lpbs.TOS;
            BLK = lpbs.BLK;
            AST = lpbs.AST;
            FOUL = lpbs.FOUL;

            Result = lpbs.Result;
            Date = lpbs.Date;
            GameID = lpbs.GameID;
        }

        public int SeasonNum { get; set; }

        public DateTime RealDate { get; set; }

        public int PlayerID { get; set; }
        public string Name { get; set; }
        public int TeamID { get; set; }
        public int TeamPTS { get; set; }
        public int OppTeamID { get; set; }
        public int OppTeamPTS { get; set; }
        public bool IsStarter { get; set; }
        public bool PlayedInjured { get; set; }
        public bool IsOut { get; set; }
        public double GmSc { get; set; }
        public double GmScE { get; set; }

        public UInt16 MINS
        {
            get
            {
                return _mins;
            }
            set
            {
                _mins = value;
                IsOut = value == 0;
                NotifyPropertyChanged("MINS");
                NotifyPropertyChanged("isOut");
            }
        }

        public UInt16 PTS { get; set; }

        public UInt16 FGM
        {
            get
            {
                return _FGM;
            }
            set
            {
                _FGM = value;
                FGp = (float) _FGM / _FGA;
                CalculatePoints();
                NotifyPropertyChanged("FGp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 FGA
        {
            get
            {
                return _FGA;
            }
            set
            {
                _FGA = value;
                FGp = (float) _FGM / _FGA;
                CalculatePoints();
                NotifyPropertyChanged("FGp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float FGp { get; set; }

        public UInt16 TPM
        {
            get
            {
                return _TPM;
            }
            set
            {
                _TPM = value;
                TPp = (float) _TPM / _TPA;
                CalculatePoints();
                NotifyPropertyChanged("TPp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 TPA
        {
            get
            {
                return _TPA;
            }
            set
            {
                _TPA = value;
                TPp = (float) _TPM / _TPA;
                CalculatePoints();
                NotifyPropertyChanged("TPp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float TPp { get; set; }

        public UInt16 FTM
        {
            get
            {
                return _FTM;
            }
            set
            {
                _FTM = value;
                FTp = (float) FTM / FTA;
                CalculatePoints();
                NotifyPropertyChanged("FTp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 FTA
        {
            get
            {
                return _FTA;
            }
            set
            {
                _FTA = value;
                FTp = (float) FTM / FTA;
                CalculatePoints();
                NotifyPropertyChanged("FTp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float FTp { get; set; }
        public UInt16 REB { get; set; }
        public UInt16 OREB { get; set; }
        public UInt16 DREB { get; set; }
        public UInt16 STL { get; set; }
        public UInt16 TOS { get; set; }
        public UInt16 BLK { get; set; }
        public UInt16 AST { get; set; }
        public UInt16 FOUL { get; set; }

        public string Result { get; set; }
        public string Date { get; set; }
        public int GameID { get; set; }

        public string DisplayTeam { get; set; }
        public string DisplayOppTeam { get; set; }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void AddInfoFromTeamBoxScore(TeamBoxScore bs, Dictionary<int, TeamStats> tst)
        {
            bs.PrepareForDisplay(tst, TeamID);
            Result = bs.DisplayResult;
            TeamPTS = TeamID == bs.Team1ID ? bs.PTS1 : bs.PTS2;
            OppTeamID = TeamID == bs.Team1ID ? bs.Team2ID : bs.Team1ID;
            OppTeamPTS = TeamID == bs.Team1ID ? bs.PTS2 : bs.PTS1;
            Date = bs.GameDate.ToString().Split(' ')[0];
            RealDate = bs.GameDate;
            DisplayTeam = tst[TeamID].DisplayName;
            DisplayOppTeam = tst[OppTeamID].DisplayName;
        }

        /// <summary>
        ///     Calculates the metrics of a player's performance.
        /// </summary>
        /// <param name="r">The SQLite DataRow containing the player's box score. Should be the result of an INNER JOIN'ed query between PlayerResults and GameResults.</param>
        public void CalcMetrics(DataRow r)
        {
            var bs = new TeamBoxScore(r, null);

            var ts = new TeamStats(TeamID);
            var tsopp = new TeamStats(OppTeamID);

            int team1ID = ParseCell.GetInt32(r, "Team1ID");
            int team2ID = ParseCell.GetInt32(r, "Team2ID");

            if (TeamID == team1ID)
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            }
            else
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);
            }

            var ps = new PlayerStats { ID = PlayerID };
            ps.AddBoxScore(this, bs.IsPlayoff);
            ps.CalcMetrics(ts, tsopp, new TeamStats(-1), GmScOnly: true);

            GmSc = ps.Metrics["GmSc"];
            GmScE = ps.Metrics["GmScE"];
        }

        /// <summary>
        ///     Calculates the metrics of a player's performance.
        /// </summary>
        /// <param name="gameID">The game ID.</param>
        /// <param name="r">The SQLite DataRow containing the player's box score. Should be the result of an INNER JOIN'ed query between PlayerResults and GameResults.</param>
        public void CalcMetrics(TeamBoxScore bs)
        {
            var ts = new TeamStats(TeamID);
            var tsopp = new TeamStats(OppTeamID);

            int team1ID = bs.Team1ID;

            if (TeamID == team1ID)
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            }
            else
            {
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);
            }

            var ps = new PlayerStats { ID = PlayerID };
            ps.AddBoxScore(this, bs.IsPlayoff);
            ps.CalcMetrics(ts, tsopp, new TeamStats(-1), GmScOnly: true);

            GmSc = ps.Metrics["GmSc"];
            GmScE = ps.Metrics["GmScE"];
        }

        /// <summary>
        ///     Calculates the points scored.
        /// </summary>
        protected void CalculatePoints()
        {
            PTS = (ushort) ((_FGM - _TPM) * 2 + _TPM * 3 + _FTM);
        }

        /// <summary>
        ///     Gets the best stats of a player's performance.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <param name="position">The player's primary position.</param>
        /// <returns></returns>
        public string GetBestStats(int count, Position position)
        {
            double fgn = 0, tpn = 0, ftn = 0, ftrn = 0;
            var statsn = new Dictionary<string, double>();

            double fgfactor, tpfactor, ftfactor, orebfactor, rebfactor, astfactor, stlfactor, blkfactor, ptsfactor, ftrfactor;

            GetFactors(
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

            if (FGM > 4)
            {
                fgn = FGp / fgfactor;
            }
            statsn.Add("fgn", fgn);

            if (TPM > 2)
            {
                tpn = TPp / tpfactor;
            }
            statsn.Add("tpn", tpn);

            if (FTM > 3)
            {
                ftn = FTp / ftfactor;
            }
            statsn.Add("ftn", ftn);

            double orebn = OREB / orebfactor;
            statsn.Add("orebn", orebn);

            /*
            drebn = (REB-OREB)/6.348;
            statsn.Add("drebn", drebn);
            */

            double rebn = REB / rebfactor;
            statsn.Add("rebn", rebn);

            double astn = AST / astfactor;
            statsn.Add("astn", astn);

            double stln = STL / stlfactor;
            statsn.Add("stln", stln);

            double blkn = BLK / blkfactor;
            statsn.Add("blkn", blkn);

            double ptsn = PTS / ptsfactor;
            statsn.Add("ptsn", ptsn);

            if (FTM > 3)
            {
                ftrn = ((double) FTM / FGA) / ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            IOrderedEnumerable<string> items = from k in statsn.Keys orderby statsn[k] descending select k;

            string s = "";
            s += String.Format("PTS: {0}\n", PTS);
            int i = 1;
            foreach (string item in items)
            {
                if (i == count)
                {
                    break;
                }

                switch (item)
                {
                    case "fgn":
                        s += String.Format("FG: {0}-{1} ({2:F3})\n", FGM, FGA, FGp);
                        break;

                    case "tpn":
                        s += String.Format("3P: {0}-{1} ({2:F3})\n", TPM, TPA, TPp);
                        break;

                    case "ftn":
                        s += String.Format("FT: {0}-{1} ({2:F3})\n", FTM, FTA, FTp);
                        break;

                    case "orebn":
                        s += String.Format("OREB: {0}\n", OREB);
                        break;

                        /*
                case "drebn":
                    s += String.Format("DREB: {0}\n", REB - OREB);
                    break;
                */

                    case "rebn":
                        s += String.Format("REB: {0}\n", REB);
                        break;

                    case "astn":
                        s += String.Format("AST: {0}\n", AST);
                        break;

                    case "stln":
                        s += String.Format("STL: {0}\n", STL);
                        break;

                    case "blkn":
                        s += String.Format("BLK: {0}\n", BLK);
                        break;

                    case "ptsn":
                        continue;

                    case "ftrn":
                        s += String.Format("FTM/FGA: {0}-{1} ({2:F3})\n", FTM, FGA, (double) FTM / FGA);
                        break;
                }

                i++;
            }
            return s;
        }

        public static void GetFactors(
            Position position,
            out double fgfactor,
            out double tpfactor,
            out double ftfactor,
            out double orebfactor,
            out double rebfactor,
            out double astfactor,
            out double stlfactor,
            out double blkfactor,
            out double ptsfactor,
            out double ftrfactor)
        {
            if (position.ToString().EndsWith("G"))
            {
                fgfactor = 0.443707;
                tpfactor = 0.361878;
                ftfactor = 0.813468;
                orebfactor = 0.800345;
                rebfactor = 3.539908;
                astfactor = 4.999772;
                stlfactor = 1.251853;
                blkfactor = 0.245448;
                ptsfactor = 15.35178;
                ftrfactor = 0.253303;
            }
            else if (position.ToString().EndsWith("F"))
            {
                fgfactor = 0.476727;
                tpfactor = 0.346698;
                ftfactor = 0.757107;
                orebfactor = 1.982639;
                rebfactor = 6.986424;
                astfactor = 2.329346;
                stlfactor = 0.964269;
                blkfactor = 0.856456;
                ptsfactor = 15.5138;
                ftrfactor = 0.2671;
            }
            else if (position.ToString().EndsWith("C"))
            {
                fgfactor = 0.505723;
                tpfactor = 0.261248;
                ftfactor = 0.670934;
                orebfactor = 2.115109;
                rebfactor = 6.527221;
                astfactor = 1.093232;
                stlfactor = 0.531171;
                blkfactor = 1.304965;
                ptsfactor = 9.309844;
                ftrfactor = 0.276999;
            }
            else
            {
                fgfactor = 0.474997;
                tpfactor = 0.352848;
                ftfactor = 0.769459;
                orebfactor = 1.762842;
                rebfactor = 6.640311;
                astfactor = 3.901761;
                stlfactor = 1.147817;
                blkfactor = 0.899758;
                ptsfactor = 17.78004;
                ftrfactor = 0.290733;
            }
        }

        /// <summary>
        ///     Resets the stats.
        /// </summary>
        public void ResetStats()
        {
            MINS = 0;
            PTS = 0;
            FGM = 0;
            FGA = 0;
            TPM = 0;
            TPA = 0;
            FTM = 0;
            FTA = 0;
            REB = 0;
            OREB = 0;
            DREB = 0;
            STL = 0;
            TOS = 0;
            BLK = 0;
            AST = 0;
            FOUL = 0;
            FGp = 0;
            FTp = 0;
            TPp = 0;
        }

        protected void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}