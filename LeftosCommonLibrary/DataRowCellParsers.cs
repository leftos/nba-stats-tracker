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
using System.Data;

#endregion

namespace LeftosCommonLibrary
{
    public class DataRowCellParsers
    {
        /// <summary>
        ///     Gets an unsigned 16-bit integer from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static UInt16 GetUInt16(DataRow r, string columnName)
        {
            return Convert.ToUInt16(r[columnName].ToString());
        }

        /// <summary>
        ///     Gets an unsigned 32-bit integer from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static UInt32 GetUInt32(DataRow r, string columnName)
        {
            return Convert.ToUInt32(r[columnName].ToString());
        }

        /// <summary>
        ///     Gets a signed 32-bit integer from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static int GetInt32(DataRow r, string columnName)
        {
            return Convert.ToInt32(r[columnName].ToString());
        }

        /// <summary>
        ///     Gets a boolean from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static Boolean GetBoolean(DataRow r, string columnName)
        {
            string s = r[columnName].ToString();
            s = s.ToLower();
            return Convert.ToBoolean(s);
        }

        /// <summary>
        ///     Gets a string from the specified column of the given DataRow.
        /// </summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static string GetString(DataRow r, string columnName)
        {
            return r[columnName].ToString();
        }

        public static double GetFloat(DataRow r, string columnName)
        {
            return Convert.ToSingle(r[columnName].ToString());
        }
    }
}