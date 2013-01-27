using System;

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    class PlayerInjuryDaysComparerDesc : PlayerInjuryDaysComparer
    {
        public override int Compare(object x, object y)
        {
            string s1;
            string s2;
            try
            {
                s1 = ((PlayerStatsRow)x).InjuryApproxDaysLeft;
                s2 = ((PlayerStatsRow)y).InjuryApproxDaysLeft;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("PlayerInjuryComparer can only compare PlayerStatsRow instances.");
            }
            return DurationOrder[s2].CompareTo(DurationOrder[s1]);
        }
    }
}