#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

using System;
using System.Collections.Generic;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Windows;

namespace NBA_Stats_Tracker.Helper
{
    public static class Misc
    {
        public static string GetCurTeamFromDisplayName(Dictionary<int, TeamStats> teamStats, string p)
        {
            for (int i = 0; i < MainWindow.tst.Count; i++)
            {
                if (teamStats[i].displayName == p)
                {
                    if (teamStats[i].isHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.tst[i].name);

                    return teamStats[i].name;
                }
            }
            throw new Exception("Team not found: " + p);
        }

        public static string GetDisplayNameFromTeam(Dictionary<int, TeamStats> teamStatses, string p)
        {
            for (int i = 0; i < teamStatses.Count; i++)
            {
                if (teamStatses[i].name == p)
                {
                    if (teamStatses[i].isHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.tst[i].name);

                    return teamStatses[i].displayName;
                }
            }
            throw new Exception("Team not found: " + p);
        }
    }
}