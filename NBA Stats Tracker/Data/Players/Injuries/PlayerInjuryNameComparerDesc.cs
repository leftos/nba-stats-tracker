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

#endregion

namespace NBA_Stats_Tracker.Data.Players.Injuries
{
    public class PlayerInjuryNameComparerDesc : PlayerInjuryNameComparer
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
                return s2.CompareTo(s1);
        }
    }
}