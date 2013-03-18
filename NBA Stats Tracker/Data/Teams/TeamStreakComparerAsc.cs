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

namespace NBA_Stats_Tracker.Data.Teams
{
    #region Using Directives

    using System;
    using System.Collections;

    #endregion

    public class TeamStreakComparerAsc : IComparer
    {
        #region IComparer Members

        public virtual int Compare(object x, object y)
        {
            string s1;
            string s2;
            try
            {
                if (x is TeamStatsRow)
                {
                    s1 = ((TeamStatsRow) x).CurStreak;
                    s2 = ((TeamStatsRow) y).CurStreak;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("TeamNameComparer can only compare PlayerStatsRow or TeamStatsRow instances.");
            }

            var t1Type = s1.Substring(0, 1);
            var t1Count = Convert.ToInt32(s1.Substring(1));

            var t2Type = s2.Substring(0, 1);
            var t2Count = Convert.ToInt32(s2.Substring(1));

            if (s1 == s2)
            {
                return 0;
            }
            else if (t1Type == "W")
            {
                if (t2Type == "W")
                {
                    if (t1Count > t2Count)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
                else
                {
                    return 1;
                }
            }
            else //t1Type == "L"
            {
                if (t2Type == "W")
                {
                    return -1;
                }
                else
                {
                    if (t1Count > t2Count)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }

        #endregion
    }
}