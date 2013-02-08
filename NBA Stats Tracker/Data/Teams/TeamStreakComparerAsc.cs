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
using NBA_Stats_Tracker.Data.Players;

#endregion

namespace NBA_Stats_Tracker.Data.Teams
{
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
                    s1 = ((TeamStatsRow)x).CurStreak;
                    s2 = ((TeamStatsRow)y).CurStreak;
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

            string t1Type = s1.Substring(0, 1);
            int t1Count = Convert.ToInt32(s1.Substring(1));

            string t2Type = s2.Substring(0, 1);
            int t2Count = Convert.ToInt32(s2.Substring(1));

            if (s1 == s2)
                return 0;
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