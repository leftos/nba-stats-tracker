using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using LeftosCommonLibrary;
using NBA_Stats_Tracker.Data.Teams;

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    /// Contains all the information of a player's performance in a game.
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
        //public ObservableCollection<KeyValuePair<int, string>> PlayersList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        public PlayerBoxScore()
        {
            PlayerID = -1;
            Team = "";
            isStarter = false;
            playedInjured = false;
            isOut = false;
            ResetStats();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="r">The DataRow containing the player's box score.</param>
        public PlayerBoxScore(DataRow r)
        {
            PlayerID = Tools.getInt(r, "PlayerID");
            GameID = Tools.getInt(r, "GameID");
            Team = r["Team"].ToString();
            isStarter = Tools.getBoolean(r, "isStarter");
            playedInjured = Tools.getBoolean(r, "playedInjured");
            isOut = Tools.getBoolean(r, "isOut");
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
            FGp = (float) FGM/FGA;
            TPp = (float) TPM/TPA;
            FTp = (float) FTM/FTA;

            // Let's try to get the result and date of the game
            // Only works for INNER JOIN'ed rows
            try
            {
                int T1PTS = Tools.getInt(r, "T1PTS");
                int T2PTS = Tools.getInt(r, "T2PTS");

                string Team1 = Tools.getString(r, "T1Name");
                string Team2 = Tools.getString(r, "T2Name");

                if (Team == Team1)
                {
                    if (T1PTS > T2PTS)
                        Result = "W " + T1PTS.ToString() + "-" + T2PTS.ToString();
                    else
                        Result = "L " + T1PTS.ToString() + "-" + T2PTS.ToString();

                    TeamPTS = T1PTS;
                    OppTeam = Team2;
                    OppTeamPTS = T2PTS;
                }
                else
                {
                    if (T2PTS > T1PTS)
                        Result = "W " + T2PTS.ToString() + "-" + T1PTS.ToString();
                    else
                        Result = "L " + T2PTS.ToString() + "-" + T1PTS.ToString();

                    TeamPTS = T2PTS;
                    OppTeam = Team1;
                    OppTeamPTS = T1PTS;
                }

                Date = Tools.getString(r, "Date").Split(' ')[0];
                RealDate = Convert.ToDateTime(Date);

                CalcMetrics(r);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="brRow">The Basketball-Reference.com row containing the player's box score.</param>
        /// <param name="team">The team.</param>
        /// <param name="gameID">The game ID.</param>
        /// <param name="starter">if set to <c>true</c>, the player is a starter.</param>
        /// <param name="playerStats">The player stats.</param>
        public PlayerBoxScore(DataRow brRow, string team, int gameID, bool starter, Dictionary<int, PlayerStats> playerStats)
        {
            string[] nameParts = brRow[0].ToString().Split(new[] {' '}, 2);
            try
            {
                PlayerID = playerStats.Single(delegate(KeyValuePair<int, PlayerStats> kvp)
                                              {
                                                  if (kvp.Value.LastName == nameParts[1] && kvp.Value.FirstName == nameParts[0] &&
                                                      kvp.Value.TeamF == team)
                                                      return true;
                                                  return false;
                                              }).Value.ID;
            }
            catch (Exception)
            {
                try
                {
                    PlayerID = playerStats.Single(delegate(KeyValuePair<int, PlayerStats> kvp)
                                                  {
                                                      if (kvp.Value.LastName == nameParts[1] && kvp.Value.FirstName == nameParts[0])
                                                          return true;
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
            Team = team;
            isStarter = starter;
            playedInjured = false;
            isOut = false;
            PTS = Tools.getUInt16(brRow, "PTS");
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
                MINS++;
            DREB = (UInt16) (REB - OREB);
            FGp = (float) FGM/FGA;
            TPp = (float) TPM/TPA;
            FTp = (float) FTM/FTA;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class.
        /// </summary>
        /// <param name="dict">The dictionary containing the player box score.</param>
        /// <param name="playerID">The player ID.</param>
        /// <param name="team">The team.</param>
        public PlayerBoxScore(Dictionary<string, string> dict, int playerID, string team)
        {
            PlayerID = playerID;
            Team = team;
            isStarter = isStarter.TrySetValue(dict, "Starter", typeof (bool));
            playedInjured = playedInjured.TrySetValue(dict, "Injured", typeof (bool));
            isOut = isOut.TrySetValue(dict, "Out", typeof (bool));
            MINS = MINS.TrySetValue(dict, "MINS", typeof (UInt16));
            PTS = PTS.TrySetValue(dict, "PTS", typeof (UInt16));
            REB = REB.TrySetValue(dict, "REB", typeof (UInt16));
            AST = AST.TrySetValue(dict, "AST", typeof (UInt16));
            STL = STL.TrySetValue(dict, "STL", typeof (UInt16));
            BLK = BLK.TrySetValue(dict, "BLK", typeof (UInt16));
            TOS = TOS.TrySetValue(dict, "TO", typeof (UInt16));
            FGM = FGM.TrySetValue(dict, "FGM", typeof (UInt16));
            FGA = FGA.TrySetValue(dict, "FGA", typeof (UInt16));
            TPM = TPM.TrySetValue(dict, "3PM", typeof (UInt16));
            TPA = TPA.TrySetValue(dict, "3PA", typeof (UInt16));
            FTM = FTM.TrySetValue(dict, "FTM", typeof (UInt16));
            FTA = FTA.TrySetValue(dict, "FTA", typeof (UInt16));
            OREB = OREB.TrySetValue(dict, "OREB", typeof (UInt16));
            FOUL = FOUL.TrySetValue(dict, "FOUL", typeof (UInt16));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerBoxScore" /> class. 
        /// Used to cast a LivePlayerBoxScore to a PlayerBoxScore which can be saved to the database.
        /// </summary>
        /// <param name="lpbs">The LivePlayerBoxScore instance containing the player's box score.</param>
        public PlayerBoxScore(LivePlayerBoxScore lpbs)
        {
            PlayerID = lpbs.PlayerID;
            Name = lpbs.Name;
            Team = lpbs.Team;
            TeamPTS = lpbs.TeamPTS;
            OppTeam = lpbs.OppTeam;
            OppTeamPTS = lpbs.OppTeamPTS;
            isStarter = lpbs.isStarter;
            playedInjured = lpbs.playedInjured;
            isOut = lpbs.isOut;
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

        public void AddInfoFromTeamBoxScore(TeamBoxScore bs)
        {
            bs.PrepareForDisplay(Team);
            Result = bs.DisplayResult;
            TeamPTS = Team == bs.Team1 ? bs.PTS1 : bs.PTS2;
            OppTeam = Team == bs.Team1 ? bs.Team2 : bs.Team1;
            OppTeamPTS = Team == bs.Team1 ? bs.PTS2 : bs.PTS1;
            Date = bs.gamedate.ToString().Split(' ')[0];
            RealDate = bs.gamedate;
        }

        public DateTime RealDate { get; set; }

        public int PlayerID { get; set; }
        public string Name { get; set; }
        public string Team { get; set; }
        public int TeamPTS { get; set; }
        public string OppTeam { get; set; }
        public int OppTeamPTS { get; set; }
        public bool isStarter { get; set; }
        public bool playedInjured { get; set; }
        public bool isOut { get; set; }
        public double GmSc { get; set; }
        public double GmScE { get; set; }
        public UInt16 MINS { get; set; }
        public UInt16 PTS { get; set; }

        public UInt16 FGM
        {
            get { return _FGM; }
            set
            {
                _FGM = value;
                FGp = (float) _FGM/_FGA;
                CalculatePoints();
                NotifyPropertyChanged("FGp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 FGA
        {
            get { return _FGA; }
            set
            {
                _FGA = value;
                FGp = (float) _FGM/_FGA;
                CalculatePoints();
                NotifyPropertyChanged("FGp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float FGp { get; set; }

        public UInt16 TPM
        {
            get { return _TPM; }
            set
            {
                _TPM = value;
                TPp = (float) _TPM/_TPA;
                CalculatePoints();
                NotifyPropertyChanged("TPp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 TPA
        {
            get { return _TPA; }
            set
            {
                _TPA = value;
                TPp = (float) _TPM/_TPA;
                CalculatePoints();
                NotifyPropertyChanged("TPp");
                NotifyPropertyChanged("PTS");
            }
        }

        public float TPp { get; set; }

        public UInt16 FTM
        {
            get { return _FTM; }
            set
            {
                _FTM = value;
                FTp = (float) FTM/FTA;
                CalculatePoints();
                NotifyPropertyChanged("FTp");
                NotifyPropertyChanged("PTS");
            }
        }

        public UInt16 FTA
        {
            get { return _FTA; }
            set
            {
                _FTA = value;
                FTp = (float) FTM/FTA;
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

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /// <summary>
        /// Calculates the metrics of a player's performance.
        /// </summary>
        /// <param name="r">The SQLite DataRow containing the player's box score. Should be the result of an INNER JOIN'ed query between PlayerResults and GameResults.</param>
        public void CalcMetrics(DataRow r)
        {
            var bs = new TeamBoxScore(r);

            var ts = new TeamStats(Team);
            var tsopp = new TeamStats(OppTeam);

            string Team1 = Tools.getString(r, "T1Name");
            string Team2 = Tools.getString(r, "T2Name");

            if (Team == Team1)
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            else
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);

            var ps = new PlayerStats();
            ps.ID = PlayerID;
            ps.AddBoxScore(this, bs.isPlayoff);
            ps.CalcMetrics(ts, tsopp, new TeamStats("$$Empty"), GmScOnly: true);

            GmSc = ps.metrics["GmSc"];
            GmScE = ps.metrics["GmScE"];
        }

        /// <summary>
        /// Calculates the metrics of a player's performance.
        /// </summary>
        /// <param name="gameID">The game ID.</param>
        /// <param name="r">The SQLite DataRow containing the player's box score. Should be the result of an INNER JOIN'ed query between PlayerResults and GameResults.</param>
        public void CalcMetrics(TeamBoxScore bs)
        {
            var ts = new TeamStats(Team);
            var tsopp = new TeamStats(OppTeam);

            string Team1 = bs.Team1;
            string Team2 = bs.Team2;

            if (Team == Team1)
                TeamStats.AddTeamStatsFromBoxScore(bs, ref ts, ref tsopp);
            else
                TeamStats.AddTeamStatsFromBoxScore(bs, ref tsopp, ref ts);

            var ps = new PlayerStats();
            ps.ID = PlayerID;
            ps.AddBoxScore(this, bs.isPlayoff);
            ps.CalcMetrics(ts, tsopp, new TeamStats("$$Empty"), GmScOnly: true);

            GmSc = ps.metrics["GmSc"];
            GmScE = ps.metrics["GmScE"];
        }

        /// <summary>
        /// Calculates the points scored.
        /// </summary>
        protected void CalculatePoints()
        {
            PTS = (ushort) ((_FGM - _TPM)*2 + _TPM*3 + _FTM);
        }

        /// <summary>
        /// Gets the best stats of a player's performance.
        /// </summary>
        /// <param name="count">The count of stats to return.</param>
        /// <param name="position">The player's primary position.</param>
        /// <returns></returns>
        public string GetBestStats(int count, Position position)
        {
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

            if (FGM > 4)
            {
                fgn = FGp/fgfactor;
            }
            statsn.Add("fgn", fgn);

            if (TPM > 2)
            {
                tpn = TPp/tpfactor;
            }
            statsn.Add("tpn", tpn);

            if (FTM > 4)
            {
                ftn = FTp/ftfactor;
            }
            statsn.Add("ftn", ftn);

            double orebn = OREB/orebfactor;
            statsn.Add("orebn", orebn);

            /*
            drebn = (REB-OREB)/6.348;
            statsn.Add("drebn", drebn);
            */

            double rebn = REB/rebfactor;
            statsn.Add("rebn", rebn);

            double astn = AST/astfactor;
            statsn.Add("astn", astn);

            double stln = STL/stlfactor;
            statsn.Add("stln", stln);

            double blkn = BLK/blkfactor;
            statsn.Add("blkn", blkn);

            double ptsn = PTS/ptsfactor;
            statsn.Add("ptsn", ptsn);

            if (FTM > 3)
            {
                ftrn = ((double) FTM/FGA)/ftrfactor;
            }
            statsn.Add("ftrn", ftrn);

            IOrderedEnumerable<string> items = from k in statsn.Keys orderby statsn[k] descending select k;

            string s = "";
            s += String.Format("PTS: {0}\n", PTS);
            int i = 1;
            foreach (string item in items)
            {
                if (i == count)
                    break;

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
                        s += String.Format("FTM/FGA: {0}-{1} ({2:F3})\n", FTM, FGA, (double) FTM/FGA);
                        break;
                }

                i++;
            }
            return s;
        }

        /// <summary>
        /// Resets the stats.
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