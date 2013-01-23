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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using LumenWorks.Framework.IO.Csv;

#endregion

namespace LeftosCommonLibrary
{
    /// <summary>
    ///     Provides methods to convert from and to CSV data.
    /// </summary>
    public static class CSV
    {
        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = {',', '"', '\n', ' '};

        private static readonly char listSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];

        public static char DetectSeparator(string path)
        {
            string s = File.ReadAllText(path);
            return DetectSeparator(new StringReader(s), Tools.SplitLinesToArray(s).Length, new char[] {',', ';'});
        }

        /// <summary>
        ///     Converts CSV data from a file into a list of dictionaries.
        /// </summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromCSVFile(string path, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(new StreamReader(path), true, useCultureSeparator ? listSeparator : DetectSeparator(path));
            var dictList = DictionaryListFromCSV(cr);

            return dictList;
        }

        public static List<Dictionary<string, string>> DictionaryListFromCSVString(string text, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(new StringReader(text), true,
                                   useCultureSeparator ? listSeparator : DetectSeparator(new StringReader(text), 1, new char[] { ',', ';' }));
            var dictList = DictionaryListFromCSV(cr);

            return dictList;
        }

        public static List<string[]> ArrayListFromCSVFile(string path, bool hasHeaders = true, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(new StreamReader(path), hasHeaders, useCultureSeparator ? listSeparator : DetectSeparator(path));
            var arrayList = ArrayListFromCSV(cr);

            return arrayList;
        }

        public static List<string[]> ArrayListFromCSVString(string text, bool hasHeaders = true, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(new StringReader(text), true,
                                   useCultureSeparator ? listSeparator : DetectSeparator(new StringReader(text), 1, new char[] { ',', ';' }));
            var arrayList = ArrayListFromCSV(cr);

            return arrayList;
        }

        private static List<Dictionary<string, string>> DictionaryListFromCSV(CsvReader cr)
        {
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

        private static List<string[]> ArrayListFromCSV(CsvReader cr)
        {
            List<string[]> arrayList;
            using (cr)
            {
                arrayList = new List<string[]>();

                int fieldCount = cr.FieldCount;
                if (cr.HasHeaders)
                {
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
                    arrayList.Add(headers);
                }

                int j = arrayList.Count;
                while (cr.ReadNextRecord())
                {
                    arrayList.Add(new string[fieldCount]);
                    for (int i = 0; i < fieldCount; i++)
                    {
                        arrayList[j][i] = cr[i];
                    }
                    j++;
                }
            }
            return arrayList;
        }

        /// <summary>
        ///     Converts a dictionary list into CSV data and writes it to a file.
        /// </summary>
        /// <param name="dList">The dictionary list. All dictionaries should have the same format. Each dictionary should be a record, and the key-value pairs should be the column header and corresponding value.</param>
        /// <param name="path">The path of the file where the data should be written to.</param>
        public static void CSVFromDictionaryList(List<Dictionary<string, string>> dList, string path, string separator = null)
        {
            var sw = new StreamWriter(path);
            string str = "";

            var columns = new Dictionary<string, string>();

            var actualSeparator = (separator == null ? listSeparator : separator.ToCharArray(0, 1)[0]);

            foreach (var kvp in dList[0])
            {
                string oldColumn = kvp.Key;
                string newColumn;
                if (!kvp.Key.StartsWith("Column"))
                {
                    newColumn = kvp.Key + actualSeparator;
                }
                else
                    newColumn = "\" \"" + actualSeparator;

                columns.Add(oldColumn, newColumn);

                str += newColumn;
            }
            str = str.TrimEnd(new[] { actualSeparator });

            sw.WriteLine(str);

            foreach (var dict in dList)
            {
                string s3 = "";
                foreach (var col in columns)
                {
                    s3 += Escape(dict[col.Key]) + actualSeparator;
                }
                s3 = s3.TrimEnd(new[] { actualSeparator });
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
            str = str.TrimEnd(new[] {'\t'});

            sw.WriteLine(str);

            foreach (var dict in dList)
            {
                string s3 = "";
                foreach (var col in columns)
                {
                    s3 += dict[col.Key] + "\t";
                }
                s3 = s3.TrimEnd(new[] {'\t'});
                sw.WriteLine(s3);
            }

            sw.Close();
        }

        /// <summary>
        ///     Converts TSV data from a file into a list of dictionaries.
        /// </summary>
        /// <param name="path">The path of the TSV file.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromTSVFile(string path)
        {
            string[] TSV = File.ReadAllLines(path);
            return DictionaryListFromTSV(TSV);
        }

        public static List<Dictionary<string, string>> DictionaryListFromTSVString(string text)
        {
            string[] TSV = Tools.SplitLinesToArray(text);
            return DictionaryListFromTSV(TSV);
        }

        public static List<string[]> ArrayListFromTSVFile(string path, bool hasHeaders = true)
        {
            string[] TSV = File.ReadAllLines(path);
            return ArrayListFromTSV(TSV, hasHeaders);
        }

        public static List<string[]> ArrayListFromTSVString(string text, bool hasHeaders = true)
        {
            string[] TSV = Tools.SplitLinesToArray(text);
            return ArrayListFromTSV(TSV, hasHeaders);
        }

        /// <summary>
        ///     Converts TSV data from an array of strings into a list of dictionaries.
        /// </summary>
        /// <param name="lines">
        ///     The array of strings to be converted. First string should be the tab-separated column headers.
        ///     Each following string should be a tab-separated record.
        /// </param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        private static List<Dictionary<string, string>> DictionaryListFromTSV(string[] lines)
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

        private static List<string[]> ArrayListFromTSV(string[] lines, bool hasHeaders = true)
        {
            var arrayList = new List<string[]>();
            string[] headers = lines[0].Split('\t');
            if (hasHeaders)
            {
                arrayList.Add(headers);
            }
            for (int i = arrayList.Count; i < lines.Length; i++)
            {
                string[] values = lines[i].Split('\t');
                if (values.Length < headers.Length)
                    continue;

                arrayList.Add(new string[values.Length]);
                for (int index = 0; index < headers.Length; index++)
                {
                    arrayList[i][index] = values[index];
                }
            }

            return arrayList;
        }

        /// <summary>
        ///     Adds quotes to a string if it needs to be escaped.
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
        ///     Unescapes (removes the quotes from) the specified string.
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

        public static char DetectSeparator(TextReader reader, int rowCount, IList<char> separators)
        {
            IList<int> separatorsCount = new int[separators.Count];

            int character;

            int row = 0;

            bool quoted = false;
            bool firstChar = true;

            while (row < rowCount)
            {
                character = reader.Read();

                switch (character)
                {
                    case '"':
                        if (quoted)
                        {
                            if (reader.Peek() != '"') // Value is quoted and 
                                // current character is " and next character is not ".
                                quoted = false;
                            else
                                reader.Read(); // Value is quoted and current and 
                            // next characters are "" - read (skip) peeked qoute.
                        }
                        else
                        {
                            if (firstChar) 	// Set value as quoted only if this quote is the 
                                // first char in the value.
                                quoted = true;
                        }
                        break;
                    case '\n':
                        if (!quoted)
                        {
                            ++row;
                            firstChar = true;
                            continue;
                        }
                        break;
                    case -1:
                        row = rowCount;
                        break;
                    default:
                        if (!quoted)
                        {
                            int index = separators.IndexOf((char)character);
                            if (index != -1)
                            {
                                ++separatorsCount[index];
                                firstChar = true;
                                continue;
                            }
                        }
                        break;
                }

                if (firstChar)
                    firstChar = false;
            }

            int maxCount = separatorsCount.Max();

            return maxCount == 0 ? '\0' : separators[separatorsCount.IndexOf(maxCount)];
        }

        public static List<string[]> ParseClipboardData()
        {
            List<string[]> clipboardData = null;
            object clipboardRawData = null;
            bool? isCSV = null;

            // get the data and set the parsing method based on the format
            // currently works with CSV and Text DataFormats            
            IDataObject dataObj = Clipboard.GetDataObject();
            if ((clipboardRawData = dataObj.GetData(DataFormats.CommaSeparatedValue)) != null)
            {
                isCSV = true;
            }
            else if ((clipboardRawData = dataObj.GetData(DataFormats.Text)) != null)
            {
                isCSV = false;
            }

            if (isCSV != null)
            {
                string rawDataStr = clipboardRawData as string;

                if (rawDataStr == null && clipboardRawData is MemoryStream)
                {
                    // cannot convert to a string so try a MemoryStream
                    MemoryStream ms = clipboardRawData as MemoryStream;
                    StreamReader sr = new StreamReader(ms);
                    rawDataStr = sr.ReadToEnd();
                }
                Debug.Assert(rawDataStr != null, String.Format("clipboardRawData: {0}, could not be converted to a string or memorystream.", clipboardRawData));

                if (isCSV == true)
                {
                    clipboardData = ArrayListFromCSVString(rawDataStr, false);
                }
                else
                {
                    clipboardData = ArrayListFromTSVString(rawDataStr, false);
                }
            }

            return clipboardData;
        }
    }
}