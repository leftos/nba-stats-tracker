using System;
using System.Collections;
using System.Collections.Generic;

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    public class PlayerInjuryDaysComparer : IComparer
    {
        public virtual int Compare(object x, object y)
        {
            string s1;
            string s2;
            try
            {
                s1 = ((PlayerStatsRow) x).InjuryApproxDaysLeft;
                s2 = ((PlayerStatsRow) y).InjuryApproxDaysLeft;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("PlayerInjuryComparer can only compare PlayerStatsRow instances.");
            }
            return DurationOrder[s1].CompareTo(DurationOrder[s2]);
        }

        protected static readonly Dictionary<string, int> DurationOrder = new Dictionary<string, int>
                                                                          {
                                                                              {"Career-Ending", 12},
                                                                              {"Unknown", 11},
                                                                              {"Active", 0},
                                                                              {"Day-To-Day", 1},
                                                                              {"1-2 weeks", 2},
                                                                              {"3-4 weeks", 3},
                                                                              {"1-2 months", 4},
                                                                              {"3-4 months", 5},
                                                                              {"4-6 months", 6},
                                                                              {"6-8 months", 7},
                                                                              {"8-10 months", 8},
                                                                              {"10-12 months", 9},
                                                                              {"More than a year", 10}
                                                                          };
    }
}