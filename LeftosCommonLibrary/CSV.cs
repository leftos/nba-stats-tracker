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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;

#endregion

namespace LeftosCommonLibrary
{
    public static class CSV
    {
        private const string QUOTE = "\"";
        private const string ESCAPED_QUOTE = "\"\"";
        private static readonly char[] CHARACTERS_THAT_MUST_BE_QUOTED = {',', '"', '\n', ' '};

        public static List<Dictionary<string, string>> CreateDictionaryListFromCSV(string path)
        {
            var cr = new CsvReader(new StreamReader(path), true);
            List<Dictionary<string, string>> dictList;
            using (cr)
            {
                dictList = new List<Dictionary<string, string>>();

                int fieldCount = cr.FieldCount;
                string[] headers = cr.GetFieldHeaders();

                for (int i = 0; i < headers.Length; i++)
                {
                    var regex = new Regex("[^a-zA-Z]");
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

        public static List<Dictionary<string, string>> CreateDictionaryListFromTSV(string path)
        {
            string[] TSV = File.ReadAllLines(path);
            var dictList = new List<Dictionary<string, string>>();
            string[] headers = TSV[0].Split('\t');
            for (int i = 1; i < TSV.Length; i++)
            {
                string[] values = TSV[i].Split('\t');
                dictList.Add(new Dictionary<string, string>());
                for (int index = 0; index < headers.Length; index++)
                {
                    dictList[i - 1][headers[index]] = values[index];
                }
            }

            return dictList;
        }

        public static void CreateCSVFromDictionaryList(List<Dictionary<string, string>> dList, string path)
        {
            var sw = new StreamWriter(path);
            string str = "";
            foreach (var kvp in dList[0])
            {
                if (!kvp.Key.StartsWith("Column")) str += kvp.Key + ",";
                else str += "\" \",";
            }
            str = str.TrimEnd(new[] {','});

            sw.WriteLine(str);

            foreach (var team in dList)
            {
                string s2 = team.Aggregate("", (current, kvp) => current + (Escape(kvp.Value) + ","));
                s2 = s2.TrimEnd(new[] {','});

                sw.WriteLine(s2);
            }

            sw.Close();
        }

        private static string Escape(string s)
        {
            if (s.Contains(QUOTE))
                s = s.Replace(QUOTE, ESCAPED_QUOTE);

            if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1)
                s = QUOTE + s + QUOTE;

            return s;
        }

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