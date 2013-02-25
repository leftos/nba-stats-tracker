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

#endregion

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    public class PlayerInjuryNameComparerDesc : PlayerInjuryNameComparerAsc
    {
        public override int Compare(object x, object y)
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
                return 1;
            else if (s2 == "Healthy")
                return -1;
            else
                return String.Compare(s2, s1, StringComparison.Ordinal);
        }
    }
}