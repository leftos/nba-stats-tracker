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

    #endregion

    /// <summary>Used to keep live track of a player's performance. Extends PlayerBoxScore.</summary>
    [Serializable]
    public class LivePlayerBoxScore : PlayerBoxScore
    {
        private ushort _oreb;
        private ushort _twoPM;

        public UInt16 TwoPM
        {
            get
            {
                return _twoPM;
            }
            set
            {
                _twoPM = value;
                FGM = (ushort) (TPM + _twoPM);
                CalculatePoints();
                NotifyPropertyChanged("FGM");
                NotifyPropertyChanged("PTS");
            }
        }

        public new UInt16 TPM
        {
            get
            {
                return _TPM;
            }
            set
            {
                _TPM = value;
                FGM = (ushort) (TPM + _twoPM);
                CalculatePoints();
                NotifyPropertyChanged("FGM");
                NotifyPropertyChanged("PTS");
            }
        }

        public new UInt16 OREB
        {
            get
            {
                return _oreb;
            }
            set
            {
                if (_oreb < value)
                {
                    REB++;
                }
                else if (_oreb > value)
                {
                    REB--;
                }
                _oreb = value;
                NotifyPropertyChanged("REB");
            }
        }
    }
}