using System;

namespace NBA_Stats_Tracker.Data.Players
{
    /// <summary>
    /// Used to keep live track of a player's performance. Extends PlayerBoxScore.
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