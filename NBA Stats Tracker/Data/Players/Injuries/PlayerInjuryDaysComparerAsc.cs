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

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    #region Using Directives

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    public class PlayerInjuryDaysComparerAsc : IComparer
    {
        protected static readonly Dictionary<string, int> DurationOrder = new Dictionary<string, int>
            {
                { "Active", 0 },
                { "Day-To-Day", 1 },
                { "1-2 weeks", 2 },
                { "2-4 weeks", 3 },
                { "4-6 weeks", 4 },
                { "6-8 weeks", 5 },
                { "2-4 months", 6 },
                { "4-6 months", 7 },
                { "6-8 months", 8 },
                { "8-10 months", 9 },
                { "10-12 months", 10 },
                { "More than a year", 11 },
                { "Unknown", 12 },
                { "Career-Ending", 13 }
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