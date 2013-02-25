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

#region Using Directives

using System;
using NBA_Stats_Tracker.Data.Players;

#endregion

namespace NBA_Stats_Tracker.Data.Teams
{
    public class TeamNameComparerDesc : TeamNameComparerAsc
    {
        public override int Compare(object x, object y)
        {
            string s1;
            string s2;
            try
            {
                if (x is PlayerStatsRow)
                {
                    s1 = ((PlayerStatsRow) x).TeamFDisplay;
                    s2 = ((PlayerStatsRow) y).TeamFDisplay;
                }
                else if (x is TeamStatsRow)
                {
                    s1 = ((TeamStatsRow) x).DisplayName;
                    s2 = ((TeamStatsRow) y).DisplayName;
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

            if (s1 == s2)
                return 0;
            else if (s1 == "- Inactive -")
                return -1;
            else if (s2 == "- Inactive -")
                return 1;
            else
                return String.Compare(s2, s1, StringComparison.Ordinal);
        }
    }
}