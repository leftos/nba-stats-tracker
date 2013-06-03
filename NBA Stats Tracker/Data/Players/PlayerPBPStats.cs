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
    using NBA_Stats_Tracker.Data.BoxScores;
    using NBA_Stats_Tracker.Data.BoxScores.PlayByPlay;

    #endregion

    public class PlayerPBPStats : INotifyPropertyChanged
    {
        private uint _assisted;
        private uint _astRec;
        private uint _blkRec;
        private uint _chrgRec;
        private uint _defassisted;
        private uint _deffga;
        private uint _deffgm;
        private string _description;
        private uint _fga;
        private uint _fgm;
        private uint _foulRec;

        private uint _gp;
        private uint _ofoul;
        private uint _saeoreb;
        private uint _stlRec;
        private uint _tech;
        private uint _tosRec;

        public PlayerPBPStats()
        {
            ResetStats();
        }

        public PlayerPBPStats(uint gp)
            : this()
        {
            GP = gp;
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

        public double Assistedp
        {
            get { return (double) _assisted / _fgm; }
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

        public double DefAssistedp
        {
            get { return (double) _defassisted / _deffgm; }
        }

        public uint ASTRec
        {
            get { return _astRec; }
            set
            {
                _astRec = value;
                OnPropertyChanged("ASTRec");
            }
        }

        public double ASTRecPG
        {
            get { return (double) _astRec / _gp; }
        }

        public uint STLRec
        {
            get { return _stlRec; }
            set
            {
                _stlRec = value;
                OnPropertyChanged("STLRec");
            }
        }

        public double STLRecPG
        {
            get { return (double) _stlRec / _gp; }
        }

        public uint BLKRec
        {
            get { return _blkRec; }
            set
            {
                _blkRec = value;
                OnPropertyChanged("BLKRec");
            }
        }

        public double BLKRecPG
        {
            get { return (double) _blkRec / _gp; }
        }

        public uint TOSRec
        {
            get { return _tosRec; }
            set
            {
                _tosRec = value;
                OnPropertyChanged("TOSRec");
            }
        }

        public double TOSRecPG
        {
            get { return (double) _tosRec / _gp; }
        }

        public uint FOULRec
        {
            get { return _foulRec; }
            set
            {
                _foulRec = value;
                OnPropertyChanged("FOULRec");
            }
        }

        public double FOULRecPG
        {
            get { return (double) _foulRec / _gp; }
        }

        public uint CHRGRec
        {
            get { return _chrgRec; }
            set
            {
                _chrgRec = value;
                OnPropertyChanged("CHRGRec");
            }
        }

        public double CHRGRecPG
        {
            get { return (double) _chrgRec / _gp; }
        }

        public uint TECH
        {
            get { return _tech; }
            set
            {
                _tech = value;
                OnPropertyChanged("TECH");
            }
        }

        public double TECHPG
        {
            get { return (double) _tech / _gp; }
        }

        public uint OFOUL
        {
            get { return _ofoul; }
            set
            {
                _ofoul = value;
                OnPropertyChanged("OFOUL");
            }
        }

        public double OFOULPG
        {
            get { return (double) _ofoul / _gp; }
        }

        /// <summary>Shot Attempts Ending in Offensive Rebound</summary>
        public uint SAEOREB
        {
            get { return _saeoreb; }
            set
            {
                _saeoreb = value;
                OnPropertyChanged("SAEOREB");
            }
        }

        public double SAEOREBPG
        {
            get { return (double) _saeoreb / _gp; }
        }

        public double SAEOREBp
        {
            get { return ((double) _saeoreb / (_fga - _fgm)); }
        }

        public double SAEDREBp
        {
            get { return 1.0 - SAEOREBp; }
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

        public void AddShots(
            int playerID,
            IEnumerable<PlayByPlayEntry> pbpeList,
            int distance = -1,
            int origin = -1,
            int type = -1,
            bool addGamePlayed = true)
        {
            AddShots(new List<int> { playerID }, pbpeList, distance, origin, type, addGamePlayed);
        }

        public void AddShots(
            List<int> teamPlayerIDs,
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

                if (teamPlayerIDs.Contains(e.Player1ID))
                {
                    FGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        FGM++;
                        if (e.ShotEntry.IsAssisted)
                        {
                            Assisted++;
                        }
                    }
                }
                else if (teamPlayerIDs.Contains(e.Player2ID))
                {
                    DefFGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        DefFGM++;
                        if (e.ShotEntry.IsAssisted)
                        {
                            DefAssisted++;
                        }
                    }
                }
            }
        }

        public static void AddShotsToList(
            ref List<PlayerPBPStats> shstList,
            List<int> teamPlayerIDs,
            IEnumerable<PlayByPlayEntry> pbpeList,
            int origin = -1,
            int type = -1,
            bool addGamePlayed = true)
        {
            if (addGamePlayed)
            {
                foreach (var o in shstList)
                {
                    o.GP++;
                }
            }

            var lastIndex = shstList.Count - 1;
            var lastEntry = shstList[lastIndex];

            foreach (var e in pbpeList.Where(e => e.EventType == PlayByPlayEntry.ShotAttemptEventType).ToList())
            {
                if (origin != -1 && e.ShotEntry.Origin != origin)
                {
                    continue;
                }
                if (type != -1 && e.ShotEntry.Type != type)
                {
                    continue;
                }

                if (teamPlayerIDs.Contains(e.Player1ID))
                {
                    var listEntry = shstList[e.ShotEntry.Distance];
                    listEntry.FGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        listEntry.FGM++;
                        if (e.ShotEntry.IsAssisted)
                        {
                            listEntry.Assisted++;
                        }
                    }
                }
                else if (teamPlayerIDs.Contains(e.Player2ID))
                {
                    var listEntry = shstList[e.ShotEntry.Distance];
                    listEntry.DefFGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        listEntry.DefFGM++;
                        if (e.ShotEntry.IsAssisted)
                        {
                            listEntry.DefAssisted++;
                        }
                    }
                }
            }
            lastEntry.ResetStats();
            for (int i = 0; i < shstList.Count - 1; i++)
            {
                var listEntry = shstList[i];
                lastEntry.AddShots(listEntry);
            }
        }

        public static void AddShotsToList(
            ref List<PlayerPBPStats> shstList,
            List<int> teamPlayerIDs,
            IEnumerable<BoxScoreEntry> bseList,
            int origin = -1,
            int type = -1,
            bool addGamePlayed = true)
        {
            foreach (var bse in bseList.Where(bse => bse.PBSList.Any(pbs => teamPlayerIDs.Contains(pbs.PlayerID))))
            {
                AddShotsToList(ref shstList, teamPlayerIDs, bse.PBPEList, origin, type, addGamePlayed);
            }
        }

        private static int getDictKey(int distance, int origin)
        {
            if (distance == 0 || origin == 0)
            {
                return 1;
            }
            else if (distance == 1)
            {
                return 2;
            }

            var value = 1;
            switch (distance)
            {
                case 2:
                    value = 3;
                    break;
                case 3:
                    value = 6;
                    break;
                case 4:
                    value = 11;
                    break;
                case 5:
                    value = 16;
                    break;
            }
            if (origin > 1)
            {
                if (distance != 2)
                {
                    value += origin - 2;
                }
                else
                {
                    value += (origin - 2) / 2;
                }
            }
            return value;
        }

        public static void AddShotsToDictionary(
            ref Dictionary<int, PlayerPBPStats> dict, List<int> playerIDs, IEnumerable<PlayByPlayEntry> pbpeList)
        {
            foreach (var e in pbpeList.Where(e => e.EventType == PlayByPlayEntry.ShotAttemptEventType).ToList())
            {
                if (playerIDs.Contains(e.Player1ID))
                {
                    var dictEntry = dict[getDictKey(e.ShotEntry.Distance, e.ShotEntry.Origin)];
                    dictEntry.FGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        dictEntry.FGM++;
                    }
                    if (e.ShotEntry.IsAssisted)
                    {
                        dictEntry.Assisted++;
                    }
                }
                else if (playerIDs.Contains(e.Player2ID))
                {
                    var dictEntry = dict[getDictKey(e.ShotEntry.Distance, e.ShotEntry.Origin)];
                    dictEntry.DefFGA++;
                    if (e.ShotEntry.IsMade)
                    {
                        dictEntry.DefFGM++;
                    }
                    if (e.ShotEntry.IsAssisted)
                    {
                        dictEntry.DefAssisted++;
                    }
                }
            }
        }

        public void AddOtherStats(List<int> teamPlayerIDs, IEnumerable<PlayByPlayEntry> pbpeList, bool addGamePlayed = true)
        {
            if (addGamePlayed)
            {
                GP++;
            }

            foreach (var e in pbpeList)
            {
                switch (e.EventType)
                {
                    case 5:
                        if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            ASTRec++;
                        }
                        break;
                    case 6:
                        if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            STLRec++;
                        }
                        break;
                    case 7:
                        if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            BLKRec++;
                        }
                        break;
                    case 8:
                        if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            TOSRec++;
                        }
                        break;
                    case 9:
                        if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            FOULRec++;
                        }
                        break;
                    case 10:
                        if (teamPlayerIDs.Contains(e.Player1ID))
                        {
                            CHRGRec++;
                        }
                        else if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            OFOUL++;
                        }
                        break;
                    case 11:
                        if (teamPlayerIDs.Contains(e.Player1ID))
                        {
                            TECH++;
                        }
                        break;
                    case 13:
                        if (teamPlayerIDs.Contains(e.Player2ID))
                        {
                            SAEOREB++;
                        }
                        break;
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

        public void AddOtherStats(int playerID, IEnumerable<PlayByPlayEntry> playerPBPEList, bool addGamePlayed)
        {
            AddOtherStats(new List<int> { playerID }, playerPBPEList, addGamePlayed);
        }

        public void AddShots(PlayerPBPStats s)
        {
            FGM += s.FGM;
            FGA += s.FGA;
            Assisted += s.Assisted;
            DefFGM += s.DefFGM;
            DefFGA += s.DefFGA;
            DefAssisted += s.DefAssisted;
        }
    }
}