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

namespace LeftosCommonLibrary
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Controls;

    using Microsoft.Win32;

    #endregion

    /// <summary>Various miscellaneous tools.</summary>
    public static class Tools
    {
        /// <summary>The application name; used as default title in various dialogs.</summary>
        public static string AppName = "";

        /// <summary>The application's registry key; used by GetRegistrySetting and SetRegistrySetting for example.</summary>
        public static string AppRegistryKey = "";

        /// <summary>Sets a registry setting using the AppRegistryKey field as the key.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setting">The setting.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">KeyCreateError: Thrown if the key could neither be opened nor created.</exception>
        public static void SetRegistrySetting<T>(string setting, T value)
        {
            SetRegistrySetting(AppRegistryKey, setting, value);
        }

        /// <summary>Sets a registry setting using a user-defined key.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">KeyCreateError: Thrown if the key could neither be opened nor created.</exception>
        public static void SetRegistrySetting<T>(string key, string setting, T value)
        {
            var rk = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            try
            {
                rk = rk.OpenSubKey(key, true);
                if (rk == null)
                {
                    rk = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                    rk = rk.OpenSubKey(key, true);
                    if (rk == null)
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception)
            {
                rk = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                rk.CreateSubKey(key);
                rk = rk.OpenSubKey(key, true);
                if (rk == null)
                {
                    throw new Exception("KeyCreateError");
                }
            }

            rk.SetValue(setting, value);
        }

        /// <summary>Gets a registry setting using the AppRegistryKey field as the key.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setting">The setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The registry setting's value.</returns>
        /// <exception cref="System.Exception">KeyOpenError: Thrown if the key couldn't be opened.</exception>
        public static T GetRegistrySetting<T>(string setting, T defaultValue)
        {
            return GetRegistrySetting(AppRegistryKey, setting, defaultValue);
        }

        /// <summary>Gets a registry setting using the AppRegistryKey field as the key.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="setting">The setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The registry setting's value.</returns>
        /// <exception cref="System.Exception">KeyOpenError: Thrown if the key couldn't be opened.</exception>
        public static T GetRegistrySetting<T>(string key, string setting, T defaultValue)
        {
            var settingValue = defaultValue;
            try
            {
                var rk = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
                rk = rk.OpenSubKey(key);
                if (rk != null)
                {
                    settingValue = (T) (Convert.ChangeType(rk.GetValue(setting, defaultValue), typeof(T)));
                }
                else
                {
                    rk = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);
                    rk = rk.OpenSubKey(key);
                    if (rk != null)
                    {
                        settingValue = (T) (Convert.ChangeType(rk.GetValue(setting, defaultValue), typeof(T)));
                    }
                    else
                    {
                        throw new Exception("KeyOpenError");
                    }
                }
            }
            catch (InvalidCastException)
            {
                settingValue = defaultValue;
            }
            catch (FormatException)
            {
                settingValue = defaultValue;
            }

            return settingValue;
        }

        /// <summary>Gets the extension of a specified file.</summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The extension of the file.</returns>
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>Gets the filename part of a path to a file.</summary>
        /// <param name="f">The path to the file.</param>
        /// <returns>The safe filename of the file.</returns>
        public static string GetSafeFilename(string f)
        {
            return Path.GetFileName(f);
        }

        /// <summary>Gets the full path of the file without its extension.</summary>
        /// <param name="f">The path to the file.</param>
        /// <returns></returns>
        public static string GetFullPathWithoutExtension(string f)
        {
            var fullpath = Path.GetFullPath(f);
            var ext = Path.GetExtension(f);
            if (!String.IsNullOrEmpty(ext))
            {
                fullpath = fullpath.Replace(ext, "");
            }
            return fullpath;
        }

        /// <summary>Gets the CRC32 of a specified file.</summary>
        /// <param name="filename">The path to the file.</param>
        /// <returns>The hex representation of the CRC32 of the file.</returns>
        public static String GetCRC(string filename)
        {
            return Crc32.CalculateCRC(filename);
        }

        /// <summary>Reverses the byte order of (part of) an array of bytes.</summary>
        /// <param name="original">The original.</param>
        /// <param name="length">The amount of bytes that should be reversed and returned, counting from the start of the array.</param>
        /// <returns>The reversed byte array.</returns>
        public static byte[] ReverseByteOrder(byte[] original, int length)
        {
            var newArr = new byte[length];
            for (var i = 0; i < length; i++)
            {
                newArr[length - i - 1] = original[i];
            }
            return newArr;
        }

        /// <summary>Converts a hex representation string to a byte array of corresponding values.</summary>
        /// <param name="hex">The hex representation.</param>
        /// <returns>The corresponding byte array.</returns>
        public static byte[] HexStringToByteArray(String hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>Converts a byte array to the equivalent hex string representation.</summary>
        /// <param name="ba">The byte array.</param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] ba)
        {
            var hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        /// <summary>Gets the MD5 hash of a string.</summary>
        /// <param name="s">The string.</param>
        /// <returns>The MD5 hash.</returns>
        public static string GetMD5(string s)
        {
            //Declarations
            Byte[] encodedBytes;
            MD5 md5;

            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            using (md5 = new MD5CryptoServiceProvider())
            {
                var originalBytes = Encoding.Default.GetBytes(s);
                encodedBytes = md5.ComputeHash(originalBytes);
            }

            //Convert encoded bytes back to a 'readable' string
            return BitConverter.ToString(encodedBytes);
        }

        /// <summary>Gets a cell of a WPF DataGrid at the specified row and column.</summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        public static DataGridCell GetCell(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
            {
                return dataRowView.Row.ItemArray[col] as DataGridCell;
            }

            return null;
        }

        /// <summary>Splits a multi-line string to an array of its lines.</summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string[] SplitLinesToArray(string text)
        {
            return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        /// <summary>Splits a multi-line string to a list of its lines.</summary>
        /// <param name="text">The text.</param>
        /// <param name="keepDuplicates">
        ///     if set to <c>true</c> [keep duplicates].
        /// </param>
        /// <returns></returns>
        public static IEnumerable<string> SplitLinesToList(string text, bool keepDuplicates = true)
        {
            var arr = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (keepDuplicates)
            {
                return arr.ToList();
            }
            else
            {
                var list = new List<string>();
                foreach (var item in arr)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
                return list;
            }
        }

        /// <summary>Writes a message to the trace listeners including a date and time stamp.</summary>
        /// <param name="msg">The message.</param>
        public static void WriteToTrace(string msg)
        {
            Trace.WriteLine(String.Format("{0}: {1}", DateTime.Now, msg));
        }

        /// <summary>Writes a message to the trace listeners including a date and time stamp and information about the exception.</summary>
        /// <param name="msg">The message.</param>
        /// <param name="ex">The exception.</param>
        public static void WriteToTraceWithException(string msg, Exception ex)
        {
            Trace.WriteLine(String.Format("{0}: {1} ({2})", DateTime.Now, msg, ex.Message));
        }

        /// <summary>Determines if a type is numeric.  Nullable numeric types are considered numeric.</summary>
        /// <remarks>Boolean is not considered numeric.</remarks>
        public static bool IsNumericType(Type type)
        {
            if (type == null)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }
                    return false;
            }
            return false;
        }

        /// <summary>Determines whether the specified string is numeric.</summary>
        /// <param name="s">The source string.</param>
        /// <returns>
        ///     <c>true</c> if the specified string is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(string s)
        {
            double temp;

            return Double.TryParse(s, out temp);
        }

        /// <summary>Checks for balanced bracketing.</summary>
        /// <param name="incomingString">The source string.</param>
        /// <returns></returns>
        public static bool CheckForBalancedBracketing(string incomingString)
        {
            const char LeftParenthesis = '(';
            const char RightParenthesis = ')';
            uint bracketCount = 0;

            try
            {
                checked // Turns on overflow checking.
                {
                    foreach (var t in incomingString)
                    {
                        switch (t)
                        {
                            case LeftParenthesis:
                                bracketCount++;
                                continue;
                            case RightParenthesis:
                                bracketCount--;
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }

            catch (OverflowException)
            {
                return false;
            }

            if (bracketCount == 0)
            {
                return true;
            }

            return false;
        }
    }
}