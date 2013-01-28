#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2013
// 
// Initial development until v1.0 done as part of the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;

#endregion

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    ///     Used to keep live track of a player's performance. Extends PlayerBoxScore.
    /// </summary>
    [Serializable]
    public class LivePlayerBoxScore : PlayerBoxScore
    {
        private ushort _OREB;
        private ushort _TwoPM;

        public UInt16 TwoPM
        {
            get { return _TwoPM; }
            set
            {
                _TwoPM = value;
                FGM = (ushort) (TPM + _TwoPM);
                CalculatePoints();
                NotifyPropertyChanged("FGM");
                NotifyPropertyChanged("PTS");
            }
        }

        public new UInt16 TPM
        {
            get { return _TPM; }
            set
            {
                _TPM = value;
                FGM = (ushort) (TPM + _TwoPM);
                CalculatePoints();
                NotifyPropertyChanged("FGM");
                NotifyPropertyChanged("PTS");
            }
        }

        public new UInt16 OREB
        {
            get { return _OREB; }
            set
            {
                if (_OREB < value)
                    REB++;
                else if (_OREB > value)
                    REB--;
                _OREB = value;
                NotifyPropertyChanged("REB");
            }
        }
    }
}