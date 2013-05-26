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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;

    using LumenWorks.Framework.IO.Csv;

    #endregion

    /// <summary>Provides methods to convert from and to CSV data.</summary>
    public static class CSV
    {
        private const string Quote = "\"";

        private const string EscapedQuote = "\"\"";

        private static readonly char[] CharactersThatMustBeQuoted = { ',', '"', '\n', ' ' };

        private static readonly char ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator.ToCharArray()[0];

        /// <summary>
        ///     If set to <c>true</c>, sorting characters at the start of any column header in REDitor-exported CSV files will be removed.
        /// </summary>
        public static bool ReplaceREDitorSortingChars;

        private static readonly char[] REDSortingChars = new[] { '^', Convert.ToChar(65533), '?' };

        private static char detectSeparator(string path)
        {
            var s = File.ReadAllText(path);
            return DetectSeparator(new StringReader(s), Tools.SplitLinesToArray(s).Length, new[] { ',', ';' });
        }

        /// <summary>Converts CSV data from a file into a list of dictionaries.</summary>
        /// <param name="path">The path of the CSV file.</param>
        /// <param name="useCultureSeparator">
        ///     If <c>true</c>, the method uses the current culture's list separator. If <c>false</c>, it tries to automatically detect it.
        /// </param>
        /// <returns>
        ///     A list of dictionaries, where each dictionary is a record, and the key-value pairs are the column header and corresponding
        ///     value.
        /// </returns>
        public static List<Dictionary<string, string>> DictionaryListFromCSVFile(string path, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(
                new StreamReader(path),
                true,
                useCultureSeparator ? ListSeparator : detectSeparator(path),
                '\"',
                '\"',
                '\0',
                ValueTrimmingOptions.UnquotedOnly);
            var dictList = dictionaryListFromCSV(cr);

            return dictList;
        }

        /// <summary>Converts CSV data from a string into a list of dictionaries.</summary>
        /// <param name="text">The CSV data.</param>
        /// <param name="useCultureSeparator">
        ///     If <c>true</c>, the method uses the current culture's list separator. If <c>false</c>, it tries to automatically detect it.
        /// </param>
        /// <returns>
        ///     A list of dictionaries, where each dictionary is a record, and the key-value pairs are the column header and corresponding
        ///     value.
        /// </returns>
        public static List<Dictionary<string, string>> DictionaryListFromCSVString(string text, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(
                new StringReader(text),
                true,
                useCultureSeparator ? ListSeparator : DetectSeparator(new StringReader(text), 1, new[] { ',', ';' }),
                '\"',
                '\"',
                '\0',
                ValueTrimmingOptions.UnquotedOnly);
            var dictList = dictionaryListFromCSV(cr);

            return dictList;
        }

        /// <summary>Converts CSV data from a file into a list of string arrays.</summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="hasHeaders">
        ///     if set to <c>true</c>, the file is assumed to have headers in the first row.
        /// </param>
        /// <param name="useCultureSeparator">
        ///     If <c>true</c>, the method uses the current culture's list separator. If <c>false</c>, it tries to automatically detect it.
        /// </param>
        /// <returns>A list of string arrays containing the CSV data.</returns>
        public static List<string[]> ArrayListFromCSVFile(string path, bool hasHeaders = true, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(
                new StreamReader(path),
                hasHeaders,
                useCultureSeparator ? ListSeparator : detectSeparator(path),
                '\"',
                '\"',
                '\0',
                ValueTrimmingOptions.UnquotedOnly);
            var arrayList = arrayListFromCSV(cr);

            return arrayList;
        }

        /// <summary>Converts CSV data from a string into a list of string arrays.</summary>
        /// <param name="text">The CSV data.</param>
        /// <param name="hasHeaders">
        ///     if set to <c>true</c>, the file is assumed to have headers in the first row.
        /// </param>
        /// <param name="useCultureSeparator">
        ///     If <c>true</c>, the method uses the current culture's list separator. If <c>false</c>, it tries to automatically detect it.
        /// </param>
        /// <returns>A list of string arrays containing the CSV data.</returns>
        public static List<string[]> ArrayListFromCSVString(string text, bool hasHeaders = true, bool useCultureSeparator = false)
        {
            var cr = new CsvReader(
                new StringReader(text),
                hasHeaders,
                useCultureSeparator ? ListSeparator : DetectSeparator(new StringReader(text), 1, new[] { ',', ';' }),
                '\"',
                '\"',
                '\0',
                ValueTrimmingOptions.UnquotedOnly);
            var arrayList = arrayListFromCSV(cr);

            return arrayList;
        }

        private static List<Dictionary<string, string>> dictionaryListFromCSV(CsvReader cr)
        {
            List<Dictionary<string, string>> dictList;
            using (cr)
            {
                dictList = new List<Dictionary<string, string>>();

                var fieldCount = cr.FieldCount;
                var headers = cr.GetFieldHeaders();

                if (ReplaceREDitorSortingChars)
                {
                    for (var i = 0; i < headers.Length; i++)
                    {
                        var firstChar = Convert.ToChar(headers[i].Substring(0, 1));
                        if (REDSortingChars.Contains(firstChar))
                        {
                            headers[i] = headers[i].Split(new[] { " ", "\r\n", "\n" }, 2, StringSplitOptions.None)[1];
                            break;
                        }
                    }
                }

                var j = 0;
                while (cr.ReadNextRecord())
                {
                    dictList.Add(new Dictionary<string, string>());
                    for (var i = 0; i < fieldCount; i++)
                    {
                        dictList[j][headers[i]] = cr[i];
                    }
                    j++;
                }
            }
            return dictList;
        }

        private static List<string[]> arrayListFromCSV(CsvReader cr)
        {
            List<string[]> arrayList;
            using (cr)
            {
                arrayList = new List<string[]>();

                var fieldCount = cr.FieldCount;
                if (ReplaceREDitorSortingChars)
                {
                    if (cr.HasHeaders)
                    {
                        var headers = cr.GetFieldHeaders();

                        for (var i = 0; i < headers.Length; i++)
                        {
                            var firstChar = Convert.ToChar(headers[i].Substring(0, 1));
                            if (REDSortingChars.Contains(firstChar))
                            {
                                headers[i] = headers[i].Split(new[] { " ", "\r\n", "\n" }, 2, StringSplitOptions.None)[1];
                                break;
                            }
                        }
                        arrayList.Add(headers);
                    }
                }

                var j = arrayList.Count;
                while (cr.ReadNextRecord())
                {
                    arrayList.Add(new string[fieldCount]);
                    for (var i = 0; i < fieldCount; i++)
                    {
                        arrayList[j][i] = cr[i];
                    }
                    j++;
                }
            }
            return arrayList;
        }

        /// <summary>Converts a dictionary list into CSV data and writes it to a file.</summary>
        /// <param name="dList">
        ///     The dictionary list. All dictionaries should have the same format. Each dictionary should be a record, and the
        ///     key-value pairs should be the column header and corresponding value.
        /// </param>
        /// <param name="path">The path of the file where the data should be written to.</param>
        /// <param name="separator">
        ///     The separator to use; should be a single-character string. If <c>null</c>, the current culture's separator will be used.
        /// </param>
        public static void CSVFromDictionaryList(List<Dictionary<string, string>> dList, string path, string separator = null)
        {
            var sw = new StreamWriter(path);
            var str = "";

            var columns = new Dictionary<string, string>();

            var actualSeparator = (separator == null ? ListSeparator : separator.ToCharArray(0, 1)[0]);

            foreach (var kvp in dList[0])
            {
                var oldColumn = kvp.Key;
                string newColumn;
                if (!kvp.Key.StartsWith("Column"))
                {
                    newColumn = kvp.Key + actualSeparator;
                }
                else
                {
                    newColumn = "\" \"" + actualSeparator;
                }

                columns.Add(oldColumn, newColumn);

                str += newColumn;
            }
            str = str.TrimEnd(new[] { actualSeparator });

            sw.WriteLine(str);

            foreach (var dict in dList)
            {
                var s3 = columns.Aggregate("", (current, col) => current + (escape(dict[col.Key]) + actualSeparator));
                s3 = s3.TrimEnd(new[] { actualSeparator });
                sw.WriteLine(s3);
            }

            sw.Close();
        }

        /// <summary>Converts a dictionary list into TSV data and writes it to a file.</summary>
        /// <param name="dList">
        ///     The dictionary list. All dictionaries should have the same format. Each dictionary should be a record, and the
        ///     key-value pairs should be the column header and corresponding value.
        /// </param>
        /// <param name="path">The path of the file where the data should be written to.</param>
        public static void TSVFromDictionaryList(List<Dictionary<string, string>> dList, string path)
        {
            var sw = new StreamWriter(path);
            var str = "";

            var columns = new Dictionary<string, string>();

            foreach (var kvp in dList[0])
            {
                var oldColumn = kvp.Key;
                string newColumn;
                if (!kvp.Key.StartsWith("Column"))
                {
                    newColumn = kvp.Key + "\t";
                }
                else
                {
                    newColumn = "\" \"" + "\t";
                }

                columns.Add(oldColumn, newColumn);

                str += newColumn;
            }
            str = str.TrimEnd(new[] { '\t' });

            sw.WriteLine(str);

            foreach (var dict in dList)
            {
                var s3 = columns.Aggregate("", (current, col) => current + (dict[col.Key] + "\t"));
                s3 = s3.TrimEnd(new[] { '\t' });
                sw.WriteLine(s3);
            }

            sw.Close();
        }

        /// <summary>Converts TSV data from a file into a list of dictionaries.</summary>
        /// <param name="path">The path of the TSV file.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromTSVFile(string path)
        {
            var tsv = File.ReadAllLines(path);
            return dictionaryListFromTSV(tsv);
        }

        /// <summary>Converts TSV data from a string into a list of dictionaries.</summary>
        /// <param name="text">The TSV data.</param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        public static List<Dictionary<string, string>> DictionaryListFromTSVString(string text)
        {
            var tsv = Tools.SplitLinesToArray(text);
            return dictionaryListFromTSV(tsv);
        }

        /// <summary>Converts TSV data from a file into a list of dictionaries.</summary>
        /// <param name="path">The path of the TSV file.</param>
        /// <param name="hasHeaders">
        ///     If <c>true</c>, the file is assumed to include headers in the first row.
        /// </param>
        /// <returns>A list of string arrays containing the TSV data.</returns>
        public static List<string[]> ArrayListFromTSVFile(string path, bool hasHeaders = true)
        {
            var tsv = File.ReadAllLines(path);
            return arrayListFromTSV(tsv, hasHeaders);
        }

        /// <summary>Converts TSV data from a file into a list of dictionaries.</summary>
        /// <param name="text">The TSV data.</param>
        /// <param name="hasHeaders">
        ///     If <c>true</c>, the file is assumed to include headers in the first row.
        /// </param>
        /// <returns>A list of string arrays containing the TSV data.</returns>
        public static List<string[]> ArrayListFromTSVString(string text, bool hasHeaders = true)
        {
            var tsv = Tools.SplitLinesToArray(text);
            return arrayListFromTSV(tsv, hasHeaders);
        }

        /// <summary>Converts TSV data from an array of strings into a list of dictionaries.</summary>
        /// <param name="lines">
        ///     The array of strings to be converted. First string should be the tab-separated column headers. Each following
        ///     string should be a tab-separated record.
        /// </param>
        /// <returns>A list of dictionaries. Each dictionary is a record, and the key-value pairs are the column header and corresponding value.</returns>
        private static List<Dictionary<string, string>> dictionaryListFromTSV(string[] lines)
        {
            var dictList = new List<Dictionary<string, string>>();
            var headers = lines[0].Split('\t');
            for (var i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length < headers.Length)
                {
                    continue;
                }

                dictList.Add(new Dictionary<string, string>());
                for (var index = 0; index < headers.Length; index++)
                {
                    dictList[i - 1][headers[index]] = values[index];
                }
            }

            return dictList;
        }

        private static List<string[]> arrayListFromTSV(string[] lines, bool hasHeaders = true)
        {
            var arrayList = new List<string[]>();
            var headers = lines[0].Split('\t');
            if (hasHeaders)
            {
                arrayList.Add(headers);
            }
            for (var i = arrayList.Count; i < lines.Length; i++)
            {
                var values = lines[i].Split('\t');
                if (values.Length < headers.Length)
                {
                    continue;
                }

                arrayList.Add(new string[values.Length]);
                for (var index = 0; index < headers.Length; index++)
                {
                    arrayList[i][index] = values[index];
                }
            }

            return arrayList;
        }

        /// <summary>Adds quotes to a string if it needs to be escaped.</summary>
        /// <param name="s">The string to be escaped.</param>
        /// <returns>The escaped string.</returns>
        private static string escape(string s)
        {
            if (s.Contains(Quote))
            {
                s = s.Replace(Quote, EscapedQuote);
            }

            if (s.IndexOfAny(CharactersThatMustBeQuoted) > -1)
            {
                s = Quote + s + Quote;
            }

            return s;
        }

        /// <summary>Unescapes (removes the quotes from) the specified string.</summary>
        /// <param name="s">The string to be unescaped.</param>
        /// <returns>The unescaped string.</returns>
        public static string Unescape(string s)
        {
            if (s.StartsWith(Quote) && s.EndsWith(Quote))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(EscapedQuote))
                {
                    s = s.Replace(EscapedQuote, Quote);
                }
            }

            return s;
        }

        /// <summary>Detects the list separator.</summary>
        /// <param name="reader">The TextReader instance.</param>
        /// <param name="rowCount">The row count.</param>
        /// <param name="separators">The list of separator candidates.</param>
        /// <returns></returns>
        public static char DetectSeparator(TextReader reader, int rowCount, IList<char> separators)
        {
            IList<int> separatorsCount = new int[separators.Count];

            var row = 0;

            var quoted = false;
            var firstChar = true;

            while (row < rowCount)
            {
                var character = reader.Read();

                switch (character)
                {
                    case '"':
                        if (quoted)
                        {
                            if (reader.Peek() != '"') // Value is quoted and 
                            {
                                // current character is " and next character is not ".
                                quoted = false;
                            }
                            else
                            {
                                reader.Read(); // Value is quoted and current and 
                            }
                            // next characters are "" - read (skip) peeked qoute.
                        }
                        else
                        {
                            if (firstChar) // Set value as quoted only if this quote is the 
                            {
                                // first char in the value.
                                quoted = true;
                            }
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
                            var index = separators.IndexOf((char) character);
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
                {
                    firstChar = false;
                }
            }

            var maxCount = separatorsCount.Max();

            return maxCount == 0 ? '\0' : separators[separatorsCount.IndexOf(maxCount)];
        }

        /// <summary>Parses the clipboard data.</summary>
        /// <returns>A list of string arrays containing the parsed CSV/TSV data.</returns>
        public static List<string[]> ParseClipboardData()
        {
            List<string[]> clipboardData = null;
            object clipboardRawData;
            bool? isCSV = null;

            // get the data and set the parsing method based on the format
            // currently works with CSV and Text DataFormats            
            var dataObj = Clipboard.GetDataObject();
            if (dataObj == null)
            {
                return new List<string[]>();
            }
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
                var rawDataStr = clipboardRawData as string;

                if (rawDataStr == null && clipboardRawData is MemoryStream)
                {
                    // cannot convert to a string so try a MemoryStream
                    var ms = clipboardRawData as MemoryStream;
                    var sr = new StreamReader(ms);
                    rawDataStr = sr.ReadToEnd();
                }
                Debug.Assert(
                    rawDataStr != null,
                    String.Format("clipboardRawData: {0}, could not be converted to a string or memorystream.", clipboardRawData));

                clipboardData = isCSV == true ? ArrayListFromCSVString(rawDataStr, false) : ArrayListFromTSVString(rawDataStr, false);
            }

            return clipboardData;
        }
    }
}