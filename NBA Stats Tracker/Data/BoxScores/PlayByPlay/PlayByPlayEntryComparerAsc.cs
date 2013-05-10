#region Copyright Notice
//     Copyright 2011-2013 Eleftherios Aslanoglou
//  
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
//  
//         http:www.apache.org/licenses/LICENSE-2.0
//  
//     Unless required by applicable law or agreed to in writing, software
//     distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and
//     limitations under the License.
#endregion
namespace NBA_Stats_Tracker.Data.BoxScores
{
    using System.Collections;
    using System.Collections.Generic;

    public class PlayByPlayEntryComparerAsc : IComparer<PlayByPlayEntry>
    {
        public int Compare(PlayByPlayEntry x, PlayByPlayEntry y)
        {
            if (x.Quarter < y.Quarter)
            {
                return -1;
            }
            else if (x.Quarter > y.Quarter)
            {
                return 1;
            }
            else
            {
                if (x.TimeLeft < y.TimeLeft)
                {
                    return 1;
                }
                else if (x.TimeLeft > y.TimeLeft)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}