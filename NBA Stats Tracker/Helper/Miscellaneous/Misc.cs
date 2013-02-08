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
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NBA_Stats_Tracker.Data.Teams;
using NBA_Stats_Tracker.Windows;

#endregion

namespace NBA_Stats_Tracker.Helper.Miscellaneous
{
    /// <summary>
    ///     Implements miscellaneous helper methods used all over NBA Stats Tracker.
    /// </summary>
    public static class Misc
    {
        public static int GetTeamIDFromDisplayName(Dictionary<int, TeamStats> teamStats, string displayName)
        {
            if (displayName == "- Inactive -")
                return -1;
            for (int i = 0; i < MainWindow.TST.Count; i++)
            {
                if (teamStats[i].DisplayName == displayName)
                {
                    if (teamStats[i].IsHidden)
                        throw new Exception("Requested team that is hidden: " + MainWindow.TST[i].Name);

                    return teamStats[i].ID;
                }
            }
            throw new Exception("Team not found: " + displayName);
        }

        /// <summary>
        ///     Loads an image into a BitmapImage object.
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
        ///     Saves a setting into the Windows registry.
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
        ///     Gets a setting from the Windows registry.
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
        ///     Gets a setting from the Windows registry.
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

        public static string GetRankingSuffix(int rank)
        {
            if (rank%10 == 1)
            {
                if (rank.ToString(CultureInfo.InvariantCulture).EndsWith("11"))
                {
                    return "th";
                }
                else
                {
                    return "st";
                }
            }
            else if (rank%10 == 2)
            {
                if (rank.ToString(CultureInfo.InvariantCulture).EndsWith("12"))
                {
                    return "th";
                }
                else
                {
                    return "nd";
                }
            }
            else if (rank%10 == 3)
            {
                if (rank.ToString(CultureInfo.InvariantCulture).EndsWith("13"))
                {
                    return "th";
                }
                else
                {
                    return "rd";
                }
            }
            else
            {
                return "th";
            }
        }

        public static string GetDisplayName(Dictionary<int, string> displayNames, int id)
        {
            if (id == -1)
                return "";
            else
            {
                try
                {
                    return displayNames[id];
                }
                catch (KeyNotFoundException)
                {
                    return "Unknown";
                }
            }
        }
    }
}