#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.
//
// Additional code:
//  - FolderBrowseDialog
//     Source: http://stackoverflow.com/questions/315164/how-to-use-a-folderbrowserdialog-from-a-wpf-application

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Windows;
using NBA_Stats_Tracker.Interop;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker
{
    internal static class Helper
    {
        public static string AppDocsPath = MainWindow.AppDocsPath;
        public static string SavesPath = MainWindow.SavesPath;
        public static string AppTempPath = MainWindow.AppTempPath;
        public static string mode = "";

        public static PlayoffTree tempPT;

        public static System.Windows.Forms.IWin32Window GetIWin32Window(this System.Windows.Media.Visual visual)
        {
            var source = System.Windows.PresentationSource.FromVisual(visual) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : System.Windows.Forms.IWin32Window
        {
            private readonly System.IntPtr _handle;
            public OldWindow(System.IntPtr handle)
            {
                _handle = handle;
            }

            #region IWin32Window Members
            System.IntPtr System.Windows.Forms.IWin32Window.Handle
            {
                get { return _handle; }
            }
            #endregion
        }

        public static SortedDictionary<string, int> setTeamOrder(string modeToSet)
        {
            SortedDictionary<string, int> TeamOrder;

            switch (modeToSet)
            {
                default:
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 9},
                                        {"Bulls", 28},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 7},
                                        {"Grizzlies", 6},
                                        {"Hawks", 16},
                                        {"Heat", 4},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 13},
                                        {"Knicks", 5},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 2},
                                        {"Pistons", 3},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 10},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 8},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 1":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 2},
                                        {"Bulls", 28},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 7},
                                        {"Grizzlies", 6},
                                        {"Hawks", 16},
                                        {"Heat", 4},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 13},
                                        {"Knicks", 5},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 9},
                                        {"Pistons", 10},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 3},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 8},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 6":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 8},
                                        {"Bulls", 28},
                                        {"Cavaliers", 12},
                                        {"Celtics", 13},
                                        {"Clippers", 6},
                                        {"Grizzlies", 5},
                                        {"Hawks", 16},
                                        {"Heat", 3},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 14},
                                        {"Knicks", 4},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 10},
                                        {"Pistons", 11},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 9},
                                        {"Suns", 2},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 7},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 2":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 8},
                                        {"Bulls", 28},
                                        {"Cavaliers", 12},
                                        {"Celtics", 13},
                                        {"Clippers", 6},
                                        {"Grizzlies", 5},
                                        {"Hawks", 16},
                                        {"Heat", 3},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 2},
                                        {"Knicks", 4},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 10},
                                        {"Pistons", 11},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 9},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 7},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 3":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 7},
                                        {"Bulls", 28},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 5},
                                        {"Grizzlies", 4},
                                        {"Hawks", 16},
                                        {"Heat", 2},
                                        {"Hornets", 15},
                                        {"Jazz", 27},
                                        {"Kings", 13},
                                        {"Knicks", 3},
                                        {"Lakers", 25},
                                        {"Magic", 23},
                                        {"Mavericks", 29},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 9},
                                        {"Pistons", 10},
                                        {"Raptors", 21},
                                        {"Rockets", 26},
                                        {"Spurs", 8},
                                        {"Suns", 14},
                                        {"Thunder", 24},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 6},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 4":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 20},
                                        {"Bobcats", 22},
                                        {"Bucks", 7},
                                        {"Bulls", 24},
                                        {"Cavaliers", 11},
                                        {"Celtics", 12},
                                        {"Clippers", 5},
                                        {"Grizzlies", 4},
                                        {"Hawks", 16},
                                        {"Heat", 2},
                                        {"Hornets", 15},
                                        {"Jazz", 29},
                                        {"Kings", 13},
                                        {"Knicks", 3},
                                        {"Lakers", 27},
                                        {"Magic", 23},
                                        {"Mavericks", 25},
                                        {"Nets", 18},
                                        {"Nuggets", 0},
                                        {"Pacers", 9},
                                        {"Pistons", 10},
                                        {"Raptors", 21},
                                        {"Rockets", 28},
                                        {"Spurs", 8},
                                        {"Suns", 14},
                                        {"Thunder", 26},
                                        {"Timberwolves", 17},
                                        {"Trail Blazers", 1},
                                        {"Warriors", 6},
                                        {"Wizards", 19}
                                    };
                    break;

                case "Mode 5":
                    TeamOrder = new SortedDictionary<string, int>
                                    {
                                        {"76ers", 13},
                                        {"Bobcats", 10},
                                        {"Bucks", 0},
                                        {"Bulls", 4},
                                        {"Cavaliers", 20},
                                        {"Celtics", 14},
                                        {"Clippers", 5},
                                        {"Grizzlies", 16},
                                        {"Hawks", 22},
                                        {"Heat", 1},
                                        {"Hornets", 9},
                                        {"Jazz", 11},
                                        {"Kings", 29},
                                        {"Knicks", 17},
                                        {"Lakers", 28},
                                        {"Magic", 8},
                                        {"Mavericks", 26},
                                        {"Nets", 3},
                                        {"Nuggets", 27},
                                        {"Pacers", 19},
                                        {"Pistons", 25},
                                        {"Raptors", 21},
                                        {"Rockets", 24},
                                        {"Spurs", 12},
                                        {"Suns", 23},
                                        {"Thunder", 7},
                                        {"Timberwolves", 18},
                                        {"Trail Blazers", 2},
                                        {"Warriors", 6},
                                        {"Wizards", 15}
                                    };
                    break;
            }

            var checklist = new List<int>();
            foreach (KeyValuePair<string, int> kvp in TeamOrder)
            {
                if (checklist.Contains(kvp.Value) == false)
                {
                    checklist.Add(kvp.Value);
                }
                else
                {
                    MessageBox.Show("Conflict for " + modeToSet + " TeamOrder on ID " + kvp.Value);
                    Environment.Exit(-1);
                }
            }

            return TeamOrder;
        }

        public static int askGamesInSeason(int gamesInSeason)
        {
            MessageBoxResult r =
                MessageBox.Show(
                    "How many games does each season have in this save?\n\n82 Games: Yes\n58 Games: No\n29 Games: Cancel",
                    "", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes) gamesInSeason = 82;
            else if (r == MessageBoxResult.No) gamesInSeason = 58;
            else if (r == MessageBoxResult.Cancel) gamesInSeason = 28;
            return gamesInSeason;
        }
    }
}