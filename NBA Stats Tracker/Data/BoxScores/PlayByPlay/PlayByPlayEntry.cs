#region Copyright Notice

//     Copyright 2011-2013 Eleftherios Aslanoglou
//  
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//  
//         http:www.apache.org/licenses/LICENSE-2.0
//  
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.

#endregion

namespace NBA_Stats_Tracker.Data.BoxScores.PlayByPlay
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.Players;
    using NBA_Stats_Tracker.Data.Teams;

    #endregion

    [Serializable]
    public class PlayByPlayEntry : INotifyPropertyChanged
    {
        public const int ShotAttemptEventType = 1;

        public static readonly Dictionary<int, string> EventTypes = new Dictionary<int, string>
            {
                { -1, "Other" },
                { 0, "Unknown" },
                { 1, "Shot Attempt" },
                { 3, "Free Throw Made" },
                { 4, "Free Throw Missed" },
                { 5, "Assist" },
                { 6, "Steal" },
                { 7, "Block" },
                { 8, "Turnover" },
                { 9, "Foul" },
                { 10, "Charge Drawn" },
                { 11, "Technical Foul" },
                { 12, "Defensive Rebound" },
                { 13, "Offensive Rebound" }
            };

        public static readonly Dictionary<int, string> Player2Definition = new Dictionary<int, string>
            {
                { -2, "2nd Player" },
                { -1, "2nd Player" },
                { 0, "2nd Player" },
                { 1, "Shot Defender" },
                { 5, "Assisted To" },
                { 6, "Stolen From" },
                { 7, "Blocked" },
                { 8, "Forced By" },
                { 9, "Fouled" },
                { 10, "Charged By" },
                { 12, "Shot By" },
                { 13, "Shot By" }
            };

        public static readonly List<int> UseOpposingTeamAsPlayer2 = new List<int> { 1, 6, 7, 8, 9, 10, 12 };

        public static readonly Dictionary<int, string> EventLocations = new Dictionary<int, string>
            {
                { -1, "Other" },
                { 0, "Unknown" },
                { 100, "Off: Under Basket" },
                { 110, "Off: Close, Left" },
                { 111, "Off: Close, Mid" },
                { 112, "Off: Close, Right" },
                { 120, "Off: Medium, Left" },
                { 121, "Off: Medium, Mid-Left" },
                { 122, "Off: Medium, Middle" },
                { 123, "Off: Medium, Mid-Right" },
                { 124, "Off: Medium, Right" },
                { 130, "Off: Beyond 3-pt, Left" },
                { 131, "Off: Beyond 3-pt, Mid-Left" },
                { 132, "Off: Beyond 3-pt, Middle" },
                { 133, "Off: Beyond 3-pt, Mid-Right" },
                { 134, "Off: Beyond 3-pt, Right" },
                { 200, "Def: Under Basket" },
                { 210, "Def: Close, Left" },
                { 211, "Def: Close, Mid" },
                { 212, "Def: Close, Right" },
                { 220, "Def: Medium, Left" },
                { 221, "Def: Medium, Mid-Left" },
                { 222, "Def: Medium, Middle" },
                { 223, "Def: Medium, Mid-Right" },
                { 224, "Def: Medium, Right" },
                { 230, "Def: Beyond 3-pt, Left" },
                { 231, "Def: Beyond 3-pt, Mid-Left" },
                { 232, "Def: Beyond 3-pt, Middle" },
                { 233, "Def: Beyond 3-pt, Mid-Right" },
                { 234, "Def: Beyond 3-pt, Right" }
            };

        private string _displayPlayer1;
        private string _displayPlayer2;
        private string _displayTeam;
        private string _eventDesc;
        private int _eventType;
        private int _gameID;

        private uint _id;
        private int _location;
        private string _locationDesc;
        private int _player1ID;
        private int _player2ID;
        private int _quarter;
        private double _shotClockLeft;
        private ShotEntry _shotEntry;
        private int _t1PTS;
        private int _t2PTS;
        private List<int> _team1PlayerIDs;
        private List<int> _team2PlayerIDs;
        private double _timeLeft;

        public PlayByPlayEntry()
        {
            Team1PlayerIDs = new List<int>(5);
            Team2PlayerIDs = new List<int>(5);
            ShotEntry = new ShotEntry();
        }

        public PlayByPlayEntry(DataRow row, BoxScoreEntry bse, Dictionary<int, TeamStats> tst, Dictionary<int, PlayerStats> pst)
            : this()
        {
            ID = ParseCell.GetUInt32(row, "ID");
            GameID = ParseCell.GetInt32(row, "GameID");
            Quarter = ParseCell.GetInt32(row, "Quarter");
            TimeLeft = ParseCell.GetDouble(row, "TimeLeft");
            ShotClockLeft = ParseCell.GetDouble(row, "ShotClockLeft");
            Player1ID = ParseCell.GetInt32(row, "P1ID");
            Player2ID = ParseCell.GetInt32(row, "P2ID");

            T1PTS = ParseCell.GetInt32(row, "T1CurPTS");
            T2PTS = ParseCell.GetInt32(row, "T2CurPTS");

            Team1PlayerIDs.Clear();
            Team1PlayerIDs.Add(ParseCell.GetInt32(row, "T1P1ID"));
            Team1PlayerIDs.Add(ParseCell.GetInt32(row, "T1P2ID"));
            Team1PlayerIDs.Add(ParseCell.GetInt32(row, "T1P3ID"));
            Team1PlayerIDs.Add(ParseCell.GetInt32(row, "T1P4ID"));
            Team1PlayerIDs.Add(ParseCell.GetInt32(row, "T1P5ID"));

            Team2PlayerIDs.Clear();
            Team2PlayerIDs.Add(ParseCell.GetInt32(row, "T2P1ID"));
            Team2PlayerIDs.Add(ParseCell.GetInt32(row, "T2P2ID"));
            Team2PlayerIDs.Add(ParseCell.GetInt32(row, "T2P3ID"));
            Team2PlayerIDs.Add(ParseCell.GetInt32(row, "T2P4ID"));
            Team2PlayerIDs.Add(ParseCell.GetInt32(row, "T2P5ID"));

            EventType = ParseCell.GetInt32(row, "EventType");
            EventDesc = ParseCell.GetString(row, "EventDesc");
            Location = ParseCell.GetInt32(row, "Location");
            LocationDesc = ParseCell.GetString(row, "LocationDesc");

            DisplayTeam = Team1PlayerIDs.Contains(Player1ID) ? tst[bse.BS.Team1ID].DisplayName : tst[bse.BS.Team2ID].DisplayName;
            DisplayPlayer1 = pst[Player1ID].FullName;
            DisplayPlayer2 = pst[Player2ID].FullName;

            if (EventType == ShotAttemptEventType)
            {
                ShotEntry = new ShotEntry(row);
            }
        }

        public uint ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        public int GameID
        {
            get { return _gameID; }
            set
            {
                _gameID = value;
                OnPropertyChanged("GameID");
            }
        }

        public int Quarter
        {
            get { return _quarter; }
            set
            {
                _quarter = value;
                OnPropertyChanged("Quarter");
            }
        }

        public double TimeLeft
        {
            get { return _timeLeft; }
            set
            {
                _timeLeft = value;
                OnPropertyChanged("TimeLeft");
            }
        }

        public double ShotClockLeft
        {
            get { return _shotClockLeft; }
            set
            {
                _shotClockLeft = value;
                OnPropertyChanged("ShotClockLeft");
            }
        }

        public int Player1ID
        {
            get { return _player1ID; }
            set
            {
                _player1ID = value;
                OnPropertyChanged("Player1ID");
            }
        }

        public int Player2ID
        {
            get { return _player2ID; }
            set
            {
                _player2ID = value;
                OnPropertyChanged("Player2ID");
            }
        }

        public int T1PTS
        {
            get { return _t1PTS; }
            set
            {
                _t1PTS = value;
                OnPropertyChanged("T1PTS");
            }
        }

        public int T2PTS
        {
            get { return _t2PTS; }
            set
            {
                _t2PTS = value;
                OnPropertyChanged("T2PTS");
            }
        }

        public List<int> Team1PlayerIDs
        {
            get { return _team1PlayerIDs; }
            set
            {
                _team1PlayerIDs = value;
                OnPropertyChanged("Team1PlayerIDs");
            }
        }

        public List<int> Team2PlayerIDs
        {
            get { return _team2PlayerIDs; }
            set
            {
                _team2PlayerIDs = value;
                OnPropertyChanged("Team2PlayerIDs");
            }
        }

        public int EventType
        {
            get { return _eventType; }
            set
            {
                _eventType = value;
                OnPropertyChanged("EventType");
            }
        }

        public string EventDesc
        {
            get { return _eventDesc; }
            set
            {
                _eventDesc = value;
                OnPropertyChanged("EventDesc");
            }
        }

        public int Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged("Location");
            }
        }

        public string LocationDesc
        {
            get { return _locationDesc; }
            set
            {
                _locationDesc = value;
                OnPropertyChanged("LocationDesc");
            }
        }

        public ShotEntry ShotEntry
        {
            get { return _shotEntry; }
            set
            {
                _shotEntry = value;
                OnPropertyChanged("ShotEntry");
            }
        }

        public string DisplayTeam
        {
            get { return _displayTeam; }
            set
            {
                _displayTeam = value;
                OnPropertyChanged("DisplayTeam");
            }
        }

        public string DisplayPlayer1
        {
            get { return _displayPlayer1; }
            set
            {
                _displayPlayer1 = value;
                OnPropertyChanged("DisplayPlayer1");
            }
        }

        public string DisplayPlayer2
        {
            get { return _displayPlayer2; }
            set
            {
                _displayPlayer2 = value;
                OnPropertyChanged("DisplayPlayer2");
            }
        }

        public string DisplayTimeLeft
        {
            get { return timeLeftToString(TimeLeft); }
        }

        public string DisplayShotClock
        {
            get { return shotClockToString(ShotClockLeft); }
        }

        public string DisplayScore
        {
            get { return String.Format("{0}-{1}", T1PTS, T2PTS); }
        }

        public string DisplayLocation
        {
            get
            {
                if (EventType == ShotAttemptEventType)
                {
                    return ShotEntry.ShotDistances[ShotEntry.Distance];
                }
                else
                {
                    return Location == -1 ? LocationDesc : EventLocations[Location];
                }
            }
        }

        public string DisplayShotOrigin
        {
            get
            {
                if (EventType == ShotAttemptEventType)
                {
                    return ShotEntry.ShotOrigins[ShotEntry.Origin];
                }
                else
                {
                    return "";
                }
            }
        }

        public string DisplayShotType
        {
            get
            {
                if (EventType == ShotAttemptEventType)
                {
                    return ShotEntry.ShotTypes[ShotEntry.Type];
                }
                else
                {
                    return "";
                }
            }
        }

        public string DisplayEvent
        {
            get
            {
                var eventTypeDescription = "";
                if (EventType == -1)
                {
                    eventTypeDescription = EventDesc;
                }
                else if (EventType != ShotAttemptEventType)
                {
                    eventTypeDescription = EventTypes[EventType];
                }
                else
                {
                    if (ShotEntry.Distance == 5)
                    {
                        eventTypeDescription += "3PT ";
                    }
                    else if (ShotEntry.Distance != 0)
                    {
                        eventTypeDescription += "2PT ";
                    }
                    else
                    {
                        eventTypeDescription += "Shot ";
                    }

                    eventTypeDescription += ShotEntry.IsMade ? "Made" : "Missed";
                }
                return eventTypeDescription;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public override string ToString()
        {
            var eventTypeDescription = DisplayEvent;
            return String.Format(
                "P{0} - {1:00}:{2:00} ({3:F1}) - {7} - {4}: {5} {6}",
                Quarter,
                Convert.ToInt32(Math.Floor(TimeLeft)) / 60,
                TimeLeft % 60,
                ShotClockLeft,
                eventTypeDescription,
                DisplayPlayer1,
                !String.IsNullOrWhiteSpace(DisplayPlayer2) ? "(" + DisplayPlayer2 + ")" : "",
                DisplayTeam);
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

        public static KeyValuePair<string, string> TimeLeftToStringPair(double timeLeft)
        {
            var intPart = Convert.ToInt32(Math.Floor(timeLeft));
            var decPart = timeLeft - intPart;

            var minutes = intPart / 60;
            var seconds = intPart % 60;

            var dispDecPart = Convert.ToInt32(decPart * 10);
            if (dispDecPart == 10)
            {
                dispDecPart = 0;
            }

            var timeLeftIntString = String.Format("{0:00}:{1:00}", minutes, seconds);
            var timeLeftDecString = String.Format(".{0:0}", dispDecPart);

            return new KeyValuePair<string, string>(timeLeftIntString, timeLeftDecString);
        }

        private static string timeLeftToString(double timeLeft)
        {
            var intPart = Convert.ToInt32(Math.Floor(timeLeft));
            var decPart = timeLeft - intPart;

            var minutes = intPart / 60;
            var seconds = intPart % 60;

            var dispDecPart = Convert.ToInt32(decPart * 10);
            if (dispDecPart == 10)
            {
                dispDecPart = 0;
            }

            string timeLeftString;
            if (minutes > 0)
            {
                timeLeftString = String.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                timeLeftString = String.Format("{0}.{1:0}", seconds, dispDecPart);
            }

            return timeLeftString;
        }

        public static KeyValuePair<string, string> ShotClockToStringPair(double shotClock)
        {
            var intPart = Convert.ToInt32(Math.Floor(shotClock));
            var decPart = shotClock - intPart;

            var dispDecPart = Convert.ToInt32(decPart * 10);
            if (dispDecPart == 10)
            {
                dispDecPart = 0;
            }

            var intPartString = String.Format("{0:0}", intPart);
            var decPartString = String.Format(".{0:0}", dispDecPart);

            var pair = new KeyValuePair<string, string>(intPartString, decPartString);
            return pair;
        }

        private static string shotClockToString(double shotClock)
        {
            var intPart = Convert.ToInt32(Math.Floor(shotClock));
            var decPart = shotClock - intPart;

            var dispDecPart = Convert.ToInt32(decPart * 10);
            if (dispDecPart == 10)
            {
                dispDecPart = 0;
            }

            var intPartString = String.Format("{0:0}", intPart);
            var decPartString = String.Format(".{0:0}", dispDecPart);

            if (intPart >= 5)
            {
                return intPartString;
            }
            else
            {
                return intPartString + decPartString;
            }
        }
    }
}