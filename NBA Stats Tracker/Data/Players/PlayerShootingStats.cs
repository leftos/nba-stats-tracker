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

namespace NBA_Stats_Tracker.Data.Players
{
    #region Using Directives

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    using NBA_Stats_Tracker.Annotations;
    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;

    #endregion

    public class PlayerShootingStats : INotifyPropertyChanged
    {
        private uint _assisted;
        private uint _defassisted;
        private uint _deffga;
        private uint _deffgm;
        private string _description;
        private uint _fga;
        private uint _fgm;

        private uint _gp;

        public PlayerShootingStats()
        {
            ResetStats();
        }

        public PlayerShootingStats(uint gp)
            : this()
        {
            GP = gp;
        }

        public PlayerShootingStats(uint gp, IEnumerable<PlayByPlayEntry> pbpeList, int distance = -1, int origin = -1, int type = -1)
            : this(gp)
        {
            Add(pbpeList, distance, origin, type, false);
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public uint GP
        {
            get { return _gp; }
            set
            {
                _gp = value;
                OnPropertyChanged("GP");
            }
        }

        public uint FGM
        {
            get { return _fgm; }
            set
            {
                _fgm = value;
                OnPropertyChanged("FGM");
                updateStats();
            }
        }

        public uint FGA
        {
            get { return _fga; }
            set
            {
                _fga = value;
                OnPropertyChanged("FGA");
                updateStats();
            }
        }

        public double FGp
        {
            get { return (double) _fgm / _fga; }
        }

        public double FGeff
        {
            get { return FGp * ((double) _fgm / _gp); }
        }

        public uint Assisted
        {
            get { return _assisted; }
            set
            {
                _assisted = value;
                OnPropertyChanged("Assisted");
            }
        }

        public uint DefFGM
        {
            get { return _deffgm; }
            set
            {
                _deffgm = value;
                OnPropertyChanged("DefFGM");
                updateDefStats();
            }
        }

        public uint DefFGA
        {
            get { return _deffga; }
            set
            {
                _deffga = value;
                OnPropertyChanged("DefFGA");
                updateStats();
            }
        }

        public double DefFGp
        {
            get { return (double) _deffgm / _deffga; }
        }

        public double DefFGeff
        {
            get { return DefFGp * ((double) _deffgm / _gp); }
        }

        public uint DefAssisted
        {
            get { return _defassisted; }
            set
            {
                _defassisted = value;
                OnPropertyChanged("DefAssisted");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void updateStats()
        {
            OnPropertyChanged("FGp");
            OnPropertyChanged("FGeff");
        }

        private void updateDefStats()
        {
            OnPropertyChanged("DefFGp");
            OnPropertyChanged("DefFGeff");
        }

        public void ResetStats()
        {
            FGM = 0;
            FGA = 0;
            Assisted = 0;
            DefFGM = 0;
            DefFGA = 0;
            DefAssisted = 0;
        }

        public void Add(
            IEnumerable<PlayByPlayEntry> pbpeList, int distance = -1, int origin = -1, int type = -1, bool addGamePlayed = true)
        {
            if (addGamePlayed)
            {
                GP++;
            }

            foreach (var e in pbpeList.Where(e => e.EventType == PlayByPlayEntry.ShotAttemptEventType).ToList())
            {
                if (distance != -1 && e.ShotEntry.Distance != distance)
                {
                    continue;
                }
                if (origin != -1 && e.ShotEntry.Origin != origin)
                {
                    continue;
                }
                if (type != -1 && e.ShotEntry.Type != type)
                {
                    continue;
                }

                FGA++;
                if (e.ShotEntry.IsMade)
                {
                    FGM++;
                }
                if (e.ShotEntry.IsAssisted)
                {
                    Assisted++;
                }
            }
        }

        public void AddDef(
            IEnumerable<PlayByPlayEntry> pbpeList, int distance = -1, int origin = -1, int type = -1, bool addGamePlayed = true)
        {
            if (addGamePlayed)
            {
                GP++;
            }

            foreach (var e in pbpeList.Where(e => e.EventType == PlayByPlayEntry.ShotAttemptEventType).ToList())
            {
                if (distance != -1 && e.ShotEntry.Distance != distance)
                {
                    continue;
                }
                if (origin != -1 && e.ShotEntry.Origin != origin)
                {
                    continue;
                }
                if (type != -1 && e.ShotEntry.Type != type)
                {
                    continue;
                }

                DefFGA++;
                if (e.ShotEntry.IsMade)
                {
                    DefFGM++;
                }
                if (e.ShotEntry.IsAssisted)
                {
                    DefAssisted++;
                }
            }
        }

        public void AddBoth(
            int playerID,
            IEnumerable<PlayByPlayEntry> pbpeList,
            int distance = -1,
            int origin = -1,
            int type = -1,
            bool addGamePlayed = true)
        {
            if (addGamePlayed)
            {
                GP++;
            }

            foreach (var e in pbpeList.Where(e => e.EventType == PlayByPlayEntry.ShotAttemptEventType).ToList())
            {
                if (distance != -1 && e.ShotEntry.Distance != distance)
                {
                    continue;
                }
                if (origin != -1 && e.ShotEntry.Origin != origin)
                {
                    continue;
                }
                if (type != -1 && e.ShotEntry.Type != type)
                {
                    continue;
                }

                if (e.Player1ID == playerID)
                {
                    FGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        FGM++;
                    }
                    if (e.ShotEntry.IsAssisted)
                    {
                        Assisted++;
                    }
                }
                else if (e.Player2ID == playerID)
                {
                    DefFGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        DefFGM++;
                    }
                    if (e.ShotEntry.IsAssisted)
                    {
                        DefAssisted++;
                    }
                }
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
    }
}