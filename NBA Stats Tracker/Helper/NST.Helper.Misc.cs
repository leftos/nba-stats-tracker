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
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data;
using NBA_Stats_Tracker.Windows;

namespace NBA_Stats_Tracker.Helper
{
    /// <summary>
    /// Implements miscellaneous helper methods used all over NBA Stats Tracker.
    /// </summary>
    public static class Misc
    {
        /// <summary>
        /// Finds a team's name based on its displayName.
        /// </summary>
        /// <param name="teamStats">The team stats dictionary..</param>
        /// <param name="displayName">The display name.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Requested team that is hidden.</exception>
        public static string GetCurTeamFromDisplayName(Dictionary<int, TeamStats> teamStats, string displayName)
        {
            for (int i = 0; i < MainWindow.tst.Count; i++)
            {
                if (teamStats[i].displayName == displayName)
                {
                    if (teamStats[i].isHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.tst[i].name);

                    return teamStats[i].name;
                }
            }
            throw new Exception("Team not found: " + displayName);
        }

        /// <summary>
        /// Finds a team's display name from its name.
        /// </summary>
        /// <param name="teamStats">The team stats dictionary.</param>
        /// <param name="name">The display name.</param>
        /// <exception cref="System.Exception">Requested team that is hidden.</exception>
        public static string GetDisplayNameFromTeam(Dictionary<int, TeamStats> teamStats, string name)
        {
            for (int i = 0; i < teamStats.Count; i++)
            {
                if (teamStats[i].name == name)
                {
                    if (teamStats[i].isHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.tst[i].name);

                    return teamStats[i].displayName;
                }
            }
            throw new Exception("Team not found: " + name);
        }

        /// <summary>
        /// Loads an image into a BitmapImage object.
        /// </summary>
        /// <param name="path">The path to the image file.</param>
        public static BitmapImage LoadImage(string path)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            return bi;
        }

        /// <summary>
        /// Saves a setting into the Windows registry.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setting">The setting.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception"></exception>
        public static void SetRegistrySetting<T>(string setting, T value)
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

        /// <summary>
        /// Gets a setting from the Windows registry.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
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

        /// <summary>
        /// Gets a setting from the Windows registry.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
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

    /// <summary>
    /// Implements a list of five players. Used in determining the best starting five in a specific scope.
    /// </summary>
    public class StartingFivePermutation
    {
        public List<int> idList = new List<int>(5);
        public int PlayersInPrimaryPosition = 0;
        public double Sum = 0;
    }

    /// <summary>
    /// Implements a generic combo-box item with an IsEnabled property. 
    /// Used to create items in combo-boxes that can't be selected (e.g. group headers).
    /// </summary>
    public class ComboBoxItemWithIsEnabled
    {
        public ComboBoxItemWithIsEnabled(string item, bool isEnabled = true)
        {
            Item = item;
            IsEnabled = isEnabled;
        }

        public string Item { get; set; }
        public bool IsEnabled { get; set; }
    }
}