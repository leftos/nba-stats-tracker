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
        public static T DeepClone<T>(this T original, params Object[] args)
        {
            return original.DeepClone(new Dictionary<Object, Object>(), args);
        }

        private static T DeepClone<T>(this T original, Dictionary<Object, Object> copies, params Object[] args)
        {
            T result;
            Type t = original.GetType();

            Object tmpResult;
            // Check if the object already has been copied
            if (copies.TryGetValue(original, out tmpResult))
            {
                return (T) tmpResult;
            }
            else
            {
                if (!t.IsArray)
                {
                    /* Create new instance, at this point you pass parameters to
                        * the constructor if the constructor if there is no default constructor
                        * or you change it to Activator.CreateInstance<T>() if there is always
                        * a default constructor */
                    result = (T) Activator.CreateInstance(t, args);
                    copies.Add(original, result);

                    // Maybe you need here some more BindingFlags
                    foreach (
                        FieldInfo field in
                            t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy |
                                        BindingFlags.Instance))
                    {
                        /* You can filter the fields here ( look for attributes and avoid
                            * unwanted fields ) */

                        Object fieldValue = field.GetValue(original);

                        // Check here if the instance should be cloned
                        Type ft = field.FieldType;

                        /* You can check here for ft.GetCustomAttributes(typeof(SerializableAttribute), false).Length != 0 to 
                            * avoid types which do not support serialization ( e.g. NetworkStreams ) */
                        if (fieldValue != null && !ft.IsValueType && ft != typeof (String))
                        {
                            fieldValue = fieldValue.DeepClone(copies);
                            /* Does not support parameters for subobjects nativly, but you can provide them when using
                                * a delegate to create the objects instead of the Activator. Delegates should not work here
                                * they need some more love */
                        }

                        field.SetValue(result, fieldValue);
                    }
                }
                else
                {
                    // Handle arrays here
                    var originalArray = (Array) (Object) original;
                    var resultArray = (Array) originalArray.Clone();
                    copies.Add(original, resultArray);

                    // If the type is not a value type we need to copy each of the elements
                    if (!t.GetElementType().IsValueType)
                    {
                        var lengths = new Int32[t.GetArrayRank()];
                        var indicies = new Int32[lengths.Length];
                        // Get lengths from original array
                        for (int i = 0; i < lengths.Length; i++)
                        {
                            lengths[i] = resultArray.GetLength(i);
                        }

                        Int32 p = lengths.Length - 1;

                        /* Now we need to iterate though each of the ranks
                            * we need to keep it generic to support all array ranks */
                        while (Increment(indicies, lengths, p))
                        {
                            Object value = resultArray.GetValue(indicies);
                            if (value != null)
                                resultArray.SetValue(value.DeepClone(copies), indicies);
                        }
                    }
                    result = (T) (Object) resultArray;
                }
                return result;
            }
        }

        private static Boolean Increment(Int32[] indicies, Int32[] lengths, Int32 p)
        {
            if (p > -1)
            {
                indicies[p]++;
                if (indicies[p] < lengths[p])
                {
                    return true;
                }
                else
                {
                    if (Increment(indicies, lengths, p - 1))
                    {
                        indicies[p] = 0;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

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
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
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
                    if (rk == null) throw new Exception();
                }
                catch (Exception)
                {
                    rk = Registry.CurrentUser;
                    rk.CreateSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                    rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker", true);
                    if (rk == null) throw new Exception();
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
                if (rk == null) throw new Exception();

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                if (rk != null) settingValue = Convert.ToInt32(rk.GetValue(setting, defaultValue));
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
                if (rk == null) throw new Exception();

                rk = rk.OpenSubKey(@"SOFTWARE\Lefteris Aslanoglou\NBA Stats Tracker");
                if (rk != null) settingValue = rk.GetValue(setting, defaultValue).ToString();
            }
            catch
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }
    }
}