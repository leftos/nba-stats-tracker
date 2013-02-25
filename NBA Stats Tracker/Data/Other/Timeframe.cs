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

namespace NBA_Stats_Tracker.Data.Other
{
    /// <summary>
    ///     Used to pass on all the required information for a specific timeframe.
    /// </summary>
    public class Timeframe
    {
        public Timeframe(int seasonNum)
        {
            IsBetween = false;
            SeasonNum = seasonNum;
        }

        public Timeframe(DateTime startDate, DateTime endDate)
        {
            IsBetween = true;
            StartDate = startDate;
            EndDate = endDate;
            SeasonNum = 1;
        }

        public bool IsBetween { get; set; }
        public int SeasonNum { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}