using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    public class PlayerInjuryNameComparer : IComparer
    {
        public virtual int Compare(object x, object y)
        {
            string s1;
            string s2;
            try
            {
                s1 = ((PlayerStatsRow) x).InjuryName;
                s2 = ((PlayerStatsRow) y).InjuryName;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("PlayerInjuryComparer can only compare PlayerStatsRow instances.");
            }

            if (s1 == s2)
                return 0;
            else if (s1 == "Healthy")
                return -1;
            else if (s2 == "Healthy")
                return 1;
            else
                return String.Compare(s1, s2, StringComparison.Ordinal);
        }
    }
}