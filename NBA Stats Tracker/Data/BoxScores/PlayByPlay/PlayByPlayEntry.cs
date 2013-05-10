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

namespace NBA_Stats_Tracker.Data.BoxScores
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;

    using LeftosCommonLibrary;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.Players;

    public class PlayByPlayEntry : INotifyPropertyChanged
    {
        public uint ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        private uint _id;

        public int GameID
        {
            get { return _gameID; }
            set
            {
                _gameID = value;
                OnPropertyChanged("GameID");
            }
        }

        private int _gameID;

        public int Quarter
        {
            get { return _quarter; }
            set
            {
                _quarter = value;
                OnPropertyChanged("Quarter");
            }
        }

        private int _quarter;

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

        private double _shotClockLeft;

        private double _timeLeft;

        public int Player1ID
        {
            get { return _player1ID; }
            set
            {
                _player1ID = value;
                OnPropertyChanged("Player1ID");
            }
        }

        private int _player1ID;

        public int Player2ID
        {
            get { return _player2ID; }
            set
            {
                _player2ID = value;
                OnPropertyChanged("Player2ID");
            }
        }

        private int _player2ID;

        public int T1PTS
        {
            get { return _t1PTS; }
            set
            {
                _t1PTS = value;
                OnPropertyChanged("T1PTS");
            }
        }

        private int _t1PTS;

        public int T2PTS
        {
            get { return _t2PTS; }
            set
            {
                _t2PTS = value;
                OnPropertyChanged("T2PTS");
            }
        }

        private int _t2PTS;

        public List<int> Team1PlayerIDs
        {
            get { return _team1PlayerIDs; }
            set
            {
                _team1PlayerIDs = value;
                OnPropertyChanged("Team1PlayerIDs");
            }
        }

        private List<int> _team1PlayerIDs;

        public List<int> Team2PlayerIDs
        {
            get { return _team2PlayerIDs; }
            set
            {
                _team2PlayerIDs = value;
                OnPropertyChanged("Team2PlayerIDs");
            }
        }

        private List<int> _team2PlayerIDs;

        public int EventType
        {
            get { return _eventType; }
            set
            {
                _eventType = value;
                OnPropertyChanged("EventType");
            }
        }

        private int _eventType;

        public string EventDesc
        {
            get { return _eventDesc; }
            set
            {
                _eventDesc = value;
                OnPropertyChanged("EventDesc");
            }
        }

        private string _eventDesc;

        public int Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged("Location");
            }
        }

        private int _location;

        public string LocationDesc
        {
            get { return _locationDesc; }
            set
            {
                _locationDesc = value;
                OnPropertyChanged("LocationDesc");
            }
        }

        private string _locationDesc;

        public ShotEntry ShotEntry
        {
            get { return _shotEntry; }
            set
            {
                _shotEntry = value;
                OnPropertyChanged("ShotEntry");
            }
        }

        private ShotEntry _shotEntry;

        public string DisplayTeam
        {
            get { return _displayTeam; }
            set
            {
                _displayTeam = value;
                OnPropertyChanged("DisplayTeam");
            }
        }

        private string _displayTeam;

        public string DisplayPlayer1
        {
            get { return _displayPlayer1; }
            set
            {
                _displayPlayer1 = value;
                OnPropertyChanged("DisplayPlayer1");
            }
        }

        private string _displayPlayer1;

        public string DisplayPlayer2
        {
            get { return _displayPlayer2; }
            set
            {
                _displayPlayer2 = value;
                OnPropertyChanged("DisplayPlayer2");
            }
        }

        private string _displayPlayer2;

        public static Dictionary<int, string> EventTypes = new Dictionary<int, string>
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

        public static Dictionary<int, string> Player2Definition = new Dictionary<int, string>
            {
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

        public static List<int> UseOpposingTeamAsPlayer2  = new List<int>
            {
                1, 6, 7, 8, 9, 10, 12
            };

        public static Dictionary<int, string> EventLocations = new Dictionary<int, string>
            {
                { -1, "Other" },
                { 0, "Unknown" },
                { 100, "Under Basket" },
                { 110, "Offensive Half: Close, Left" },
                { 111, "Offensive Half: Close, Mid" },
                { 112, "Offensive Half: Close, Right" },
                { 120, "Offensive Half: Medium, Left" },
                { 121, "Offensive Half: Medium, Mid-Left" },
                { 122, "Offensive Half: Medium, Middle" },
                { 123, "Offensive Half: Medium, Mid-Right" },
                { 124, "Offensive Half: Medium, Right" },
                { 130, "Offensive Half: Beyond 3-pt, Left" },
                { 131, "Offensive Half: Beyond 3-pt, Mid-Left" },
                { 132, "Offensive Half: Beyond 3-pt, Middle" },
                { 133, "Offensive Half: Beyond 3-pt, Mid-Right" },
                { 134, "Offensive Half: Beyond 3-pt, Right" },
                { 200, "Defensive Half: Under Basket" },
                { 210, "Defensive Half: Close, Left" },
                { 211, "Defensive Half: Close, Mid" },
                { 212, "Defensive Half: Close, Right" },
                { 220, "Defensive Half: Medium, Left" },
                { 221, "Defensive Half: Medium, Mid-Left" },
                { 222, "Defensive Half: Medium, Middle" },
                { 223, "Defensive Half: Medium, Mid-Right" },
                { 224, "Defensive Half: Medium, Right" },
                { 230, "Defensive Half: Beyond 3-pt, Left" },
                { 231, "Defensive Half: Beyond 3-pt, Mid-Left" },
                { 232, "Defensive Half: Beyond 3-pt, Middle" },
                { 233, "Defensive Half: Beyond 3-pt, Mid-Right" },
                { 234, "Defensive Half: Beyond 3-pt, Right" }
            };

        public PlayByPlayEntry()
        {
            Team1PlayerIDs = new List<int>(5);
            Team2PlayerIDs = new List<int>(5);
            ShotEntry = new ShotEntry();
        }

        public PlayByPlayEntry(DataRow row)
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

            if (EventType == 1)
            {
                ShotEntry = new ShotEntry(row);
            }
        }

        public override string ToString()
        {
            string eventTypeDescription = "";
            if (EventType == -1)
            {
                eventTypeDescription = EventDesc;
            }
            else if (EventType != 1)
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
            return string.Format(
                "P{0} - {1:00}:{2:00} ({3:F1}) - {7} - {4}: {5} {6}",
                Quarter,
                Convert.ToInt32(Math.Floor(TimeLeft)) / 60,
                TimeLeft % 60,
                ShotClockLeft,
                eventTypeDescription,
                DisplayPlayer1,
                DisplayPlayer2 != "" ? "(" + DisplayPlayer2 + ")" : "",
                DisplayTeam);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}