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
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
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

        public static BitmapImage LoadImage(string path)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }

        public static void SetRegistrySetting(string setting, int value)
        {
            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null)
                        throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null)
                        throw new Exception();
                }

                rk.SetValue(setting, value);
            }
            catch
            {
                MessageBox.Show("Couldn't save changed setting.");
            }
        }

        public static void SetRegistrySetting(string setting, string value)
        {
            RegistryKey rk = Registry.CurrentUser;
            try
            {
                try
                {
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null)
                        throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null)
                        throw new Exception();
                }

                rk.SetValue(setting, value);
            }
            catch
            {
                MessageBox.Show("Couldn't save changed setting.");
            }
        }

        public static int GetRegistrySetting(string setting, int defaultValue)
        {
            RegistryKey rk = Registry.CurrentUser;
            int settingValue = defaultValue;
            try
            {
                if (rk == null)
                    throw new Exception();

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                if (rk != null)
                    settingValue = Convert.ToInt32(rk.GetValue(setting, defaultValue));
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }

        public static string GetRegistrySetting(string setting, string defaultValue)
        {
            RegistryKey rk = Registry.CurrentUser;
            string settingValue = defaultValue;
            try
            {
                if (rk == null)
                    throw new Exception();

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                if (rk != null)
                    settingValue = rk.GetValue(setting, defaultValue).ToString();
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }
    }

    public class StartingFivePermutation
    {
        public List<int> idList = new List<int>();
        public double sum = 0;
        public int pInP = 0;
    }
}