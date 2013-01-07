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

#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

#endregion

namespace LeftosCommonLibrary
{
    public static class Tools
    {
        /// <summary>
        ///     Gets the extension of a specified file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The extension of the file.</returns>
        public static string getExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        ///     Gets the filename part of a path to a file.
        /// </summary>
        /// <param name="f">The path to the file.</param>
        /// <returns>The safe filename of the file.</returns>
        public static string getSafeFilename(string f)
        {
            return Path.GetFileName(f);
        }

        /// <summary>
        ///     Gets the CRC32 of a specified file.
        /// </summary>
        /// <param name="filename">The path to the file.</param>
        /// <returns>The hex representation of the CRC32 of the file.</returns>
        public static String getCRC(string filename)
        {
            return Crc32.CalculateCRC(filename);
        }

        /// <summary>
        ///     Reverses the byte order of (part of) an array of bytes.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="length">The amount of bytes that should be reversed and returned, counting from the start of the array.</param>
        /// <returns>The reversed byte array.</returns>
        public static byte[] ReverseByteOrder(byte[] original, int length)
        {
            var newArr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                newArr[length - i - 1] = original[i];
            }
            return newArr;
        }

        /// <summary>
        ///     Converts a hex representation string to a byte array of corresponding values.
        /// </summary>
        /// <param name="hex">The hex representation.</param>
        /// <returns>The corresponding byte array.</returns>
        public static byte[] HexStringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            var bytes = new byte[NumberChars/2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        /// <summary>
        ///     Gets the MD5 hash of a string.
        /// </summary>
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
                Byte[] originalBytes = Encoding.Default.GetBytes(s);
                encodedBytes = md5.ComputeHash(originalBytes);
            }

            //Convert encoded bytes back to a 'readable' string
            return BitConverter.ToString(encodedBytes);
        }

        /// <summary>
        ///     Gets a cell of a WPF DataGrid at the specified row and column.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The column.</param>
        /// <returns></returns>
        public static DataGridCell GetCell(DataGrid dataGrid, int row, int col)
        {
            var dataRowView = dataGrid.Items[row] as DataRowView;
            if (dataRowView != null)
                return dataRowView.Row.ItemArray[col] as DataGridCell;

            return null;
        }

        /// <summary>
        ///     Gets an unsigned 16-bit integer from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="ColumnName">Name of the column.</param>
        /// <returns></returns>
        public static UInt16 getUInt16(DataRow r, string ColumnName)
        {
            return Convert.ToUInt16(r[ColumnName].ToString());
        }

        /// <summary>
        ///     Gets an unsigned 32-bit integer from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="ColumnName">Name of the column.</param>
        /// <returns></returns>
        public static UInt32 getUInt32(DataRow r, string ColumnName)
        {
            return Convert.ToUInt32(r[ColumnName].ToString());
        }

        /// <summary>
        ///     Gets a signed 32-bit integer from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="ColumnName">Name of the column.</param>
        /// <returns></returns>
        public static int getInt(DataRow r, string ColumnName)
        {
            return Convert.ToInt32(r[ColumnName].ToString());
        }

        /// <summary>
        ///     Gets a boolean from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="ColumnName">Name of the column.</param>
        /// <returns></returns>
        public static Boolean getBoolean(DataRow r, string ColumnName)
        {
            string s = r[ColumnName].ToString();
            s = s.ToLower();
            return Convert.ToBoolean(s);
        }

        /// <summary>
        ///     Gets a string from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="ColumnName">Name of the column.</param>
        /// <returns></returns>
        public static string getString(DataRow r, string ColumnName)
        {
            return r[ColumnName].ToString();
        }

        /// <summary>
        ///     Splits a multi-line string to an array of its lines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string[] SplitLinesToArray(string text)
        {
            return text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
        }

        /// <summary>
        ///     Splits a multi-line string to a list of its lines.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="keepDuplicates">
        ///     if set to <c>true</c> [keep duplicates].
        /// </param>
        /// <returns></returns>
        public static List<string> SplitLinesToList(string text, bool keepDuplicates = true)
        {
            string[] arr = text.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);
            if (keepDuplicates)
                return arr.ToList();
            else
            {
                var list = new List<string>();
                foreach (string item in arr)
                {
                    if (!list.Contains(item))
                        list.Add(item);
                }
                return list;
            }
        }
    }
}