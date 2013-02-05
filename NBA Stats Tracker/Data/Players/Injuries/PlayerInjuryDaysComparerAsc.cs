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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    public class PlayerInjuryDaysComparerAsc : IComparer
    {
        protected static readonly Dictionary<string, int> DurationOrder = new Dictionary<string, int>
                                                                          {
                                                                              {"Active", 0},
                                                                              {"Day-To-Day", 1},
                                                                              {"1-2 weeks", 2},
                                                                              {"2-4 weeks", 3},
                                                                              {"4-6 weeks", 4},
                                                                              {"6-8 weeks", 5},
                                                                              {"2-4 months", 6},
                                                                              {"4-6 months", 7},
                                                                              {"6-8 months", 8},
                                                                              {"8-10 months", 9},
                                                                              {"10-12 months", 10},
                                                                              {"More than a year", 11},
                                                                              {"Unknown", 12},
                                                                              {"Career-Ending", 13}
                                                                          };

        #region IComparer Members

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

        #endregion
    }
}