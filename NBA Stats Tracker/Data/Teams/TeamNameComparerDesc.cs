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
    public class TeamNameComparerDesc : TeamNameComparerAsc
    {
        #region IComparer Members

        public override int Compare(object x, object y)
        {
            string s1;
            string s2;
            try
            {
                s1 = ((PlayerStatsRow) x).TeamFDisplay;
                s2 = ((PlayerStatsRow) y).TeamFDisplay;
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("TeamNameComparer can only compare PlayerStatsRow instances.");
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

        #endregion
    }
}