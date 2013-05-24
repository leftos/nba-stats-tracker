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

    #endregion

    [Serializable]
    public class ShotEntry : INotifyPropertyChanged
    {
        public static Dictionary<int, string> ShotDistances = new Dictionary<int, string>
            {
                { 0, "Unknown" },
                { 1, "Under the basket" },
                { 2, "3-10ft (0.91-3.04m)" },
                { 3, "10-16ft (3.04-4.88m)" },
                { 4, "16ft-3pt (4.88m-3pt)" },
                { 5, "Beyond 3pt" }
            };

        public static Dictionary<int, string> ShotOrigins = new Dictionary<int, string>
            {
                { 0, "Unknown" },
                { 1, "Under the basket" },
                { 2, "Left" },
                { 3, "Mid-Left" },
                { 4, "Middle" },
                { 5, "Mid-Right" },
                { 6, "Right" }
            };

        public static Dictionary<int, string> ShotTypes = new Dictionary<int, string>
            {
                { 0, "Unknown" },
                { 1, "Dunk" },
                { 2, "Hook Shot" },
                { 3, "Jump Shot" },
                { 4, "Layup" },
                { 5, "Tip Shot" }
            };

        private int _distance;
        private bool _isAssisted;
        private bool _isMade;
        private int _origin;
        private int _type;

        public ShotEntry()
        {
        }

        public ShotEntry(int distance, int origin, int type, bool isMade, bool isAssisted)
        {
            Distance = distance;
            Origin = origin;
            Type = type;
            IsMade = isMade;
            IsAssisted = isAssisted;
        }

        public ShotEntry(DataRow row)
        {
            Distance = ParseCell.GetInt32(row, "ShotDistance");
            Origin = ParseCell.GetInt32(row, "ShotOrigin");
            Type = ParseCell.GetInt32(row, "ShotType");
            IsMade = ParseCell.GetBoolean(row, "ShotIsMade");
            IsAssisted = ParseCell.GetBoolean(row, "ShotIsAssisted");
        }

        public int Distance
        {
            get { return _distance; }
            set
            {
                _distance = value;
                OnPropertyChanged("Distance");
            }
        }

        public int Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                OnPropertyChanged("Origin");
            }
        }

        public int Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        public bool IsMade
        {
            get { return _isMade; }
            set
            {
                _isMade = value;
                OnPropertyChanged("IsMade");
            }
        }

        public bool IsAssisted
        {
            get { return _isAssisted; }
            set
            {
                _isAssisted = value;
                OnPropertyChanged("IsAssisted");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

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