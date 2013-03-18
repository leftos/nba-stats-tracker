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
    using System.Data;

    #endregion

    /// <summary>Methods to parse a DataRowCell into common data-types.</summary>
    public class ParseCell
    {
        /// <summary>Gets an unsigned 16-bit integer from the specified column of the given DataRow.</summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static UInt16 GetUInt16(DataRow r, string columnName)
        {
            return Convert.ToUInt16(r[columnName].ToString());
        }

        /// <summary>Gets an unsigned 32-bit integer from the specified column of the given DataRow.</summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static UInt32 GetUInt32(DataRow r, string columnName)
        {
            return Convert.ToUInt32(r[columnName].ToString());
        }

        /// <summary>Gets a signed 32-bit integer from the specified column of the given DataRow.</summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static int GetInt32(DataRow r, string columnName)
        {
            return Convert.ToInt32(r[columnName].ToString());
        }

        /// <summary>Gets a boolean from the specified column of the given DataRow.</summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static Boolean GetBoolean(DataRow r, string columnName)
        {
            var s = r[columnName].ToString();
            s = s.ToLower();
            return Convert.ToBoolean(s);
        }

        /// <summary>Gets a string from the specified column of the given DataRow.</summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static string GetString(DataRow r, string columnName)
        {
            return r[columnName].ToString();
        }

        /// <summary>Gets a float from the specified column of the given DataRow.</summary>
        /// <param name="r">The row.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static double GetFloat(DataRow r, string columnName)
        {
            return Convert.ToSingle(r[columnName].ToString());
        }
    }
}