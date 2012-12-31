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
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;

#endregion

namespace LeftosCommonLibrary
{
    /// <summary>
    /// Provides methods to convert from and to CSV data. 
    /// </summary>
    public static class CSV
    {
        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = {',', '"', '\n', ' '};

        private static readonly char listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];

        /// <summary>
        /// Converts CSV data from a file into a list of dictionaries.
        /// </summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromCSV(string path)
        {
            var cr = new CsvReader(new StreamReader(path), true, listSeparator);
            List<Dictionary<string, string>> dictList;
            using (cr)
            {
                dictList = new List<Dictionary<string, string>>();

                int fieldCount = cr.FieldCount;
                string[] headers = cr.GetFieldHeaders();

                for (int i = 0; i < headers.Length; i++)
                {
                    var regex = new Regex("[^a-zA-Z0-9]");
                    if (regex.IsMatch(headers[i].Substring(0, 1)))
                    {
                        headers[i] = headers[i].Split(new[] {" ", "\r\n", "\n"}, 2, StringSplitOptions.None)[1];
                        break;
                    }
                }

                int j = 0;
                while (cr.ReadNextRecord())
                {
                    dictList.Add(new Dictionary<string, string>());
                    for (int i = 0; i < fieldCount; i++)
                    {
                        dictList[j][headers[i]] = cr[i];
                    }
                    j++;
                }
            }

            return dictList;
        }

        /// <summary>
        /// Converts a dictionary list into CSV data and writes it to a file.
        /// </summary>
        /// <param name="dList">The dictionary list. All dictionaries should have the same format. Each dictionary should be a record, and the key-value pairs should be the column header and corresponding value.</param>
        /// <param name="path">The path of the file where the data should be written to.</param>
        public static void CSVFromDictionaryList(List<Dictionary<string, string>> dList, string path)
        {
            var sw = new StreamWriter(path);
            string str = "";

            var columns = new Dictionary<string, string>();

            foreach (var kvp in dList[0])
            {
                string oldColumn = kvp.Key;
                string newColumn;
                if (!kvp.Key.StartsWith("Column"))
                    newColumn = kvp.Key + listSeparator;
                else
                    newColumn = "\" \"" + listSeparator;

                columns.Add(oldColumn, newColumn);

                str += newColumn;
            }
            str = str.TrimEnd(new[] {listSeparator});

            sw.WriteLine(str);

            foreach (var dict in dList)
            {
                string s3 = "";
                foreach (var col in columns)
                {
                    s3 += Escape(dict[col.Key]) + listSeparator;
                }
                s3 = s3.TrimEnd(new[] {listSeparator});
                sw.WriteLine(s3);
            }

            sw.Close();
        }

        public static void TSVFromDictionaryList(List<Dictionary<string, string>> dList, string path)
        {
            var sw = new StreamWriter(path);
            string str = "";

            var columns = new Dictionary<string, string>();

            foreach (var kvp in dList[0])
            {
                string oldColumn = kvp.Key;
                string newColumn;
                if (!kvp.Key.StartsWith("Column"))
                    newColumn = kvp.Key + "\t";
                else
                    newColumn = "\" \"" + "\t";

                columns.Add(oldColumn, newColumn);

                str += newColumn;
            }
            str = str.TrimEnd(new[] { '\t' });

            sw.WriteLine(str);

            foreach (var dict in dList)
            {
                string s3 = "";
                foreach (var col in columns)
                {
                    s3 += dict[col.Key] + "\t";
                }
                s3 = s3.TrimEnd(new[] { '\t' });
                sw.WriteLine(s3);
            }

            sw.Close();
        }

        /// <summary>
        /// Converts TSV data from a file into a list of dictionaries.
        /// </summary>
        /// <param name="path">The path of the TSV file.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromTSV(string path)
        {
            string[] TSV = File.ReadAllLines(path);
            return DictionaryListFromTSV(TSV);
        }

        /// <summary>
        /// Converts TSV data from an array of strings into a list of dictionaries.
        /// </summary>
        /// <param name="lines">The array of strings to be converted. First string should be the tab-separated column headers. 
        /// Each following string should be a tab-separated record.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromTSV(string[] lines)
        {
            var dictList = new List<Dictionary<string, string>>();
            string[] headers = lines[0].Split('\t');
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split('\t');
                if (values.Length < headers.Length)
                    continue;

                dictList.Add(new Dictionary<string, string>());
                for (int index = 0; index < headers.Length; index++)
                {
                    dictList[i - 1][headers[index]] = values[index];
                }
            }

            return dictList;
        }

        /// <summary>
        /// Adds quotes to a string if it needs to be escaped.
        /// </summary>
        /// <param name="s">The string to be escaped.</param>
        /// <returns>The escaped string.</returns>
        private static string Escape(string s)
        {
            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPED_QUOTE);

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = QUOTE + s + QUOTE;

            return s;
        }

        /// <summary>
        /// Unescapes (removes the quotes from) the specified string.
        /// </summary>
        /// <param name="s">The string to be unescaped.</param>
        /// <returns>The unescaped string.</returns>
        public static string Unescape(string s)
        {
            if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(ESCAPED_QUOTE))
                    s = s.Replace(ESCAPED_QUOTE, QUOTE);
            }

            return s;
        }
    }
}