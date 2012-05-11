#region Copyright Notice

// Created by Lefteris Aslanoglou, (c) 2011-2012
// 
// Implementation of thesis
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
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

#endregion

namespace LeftosCommonLibrary
{
    public static class Tools
    {
        public static string getExtension(string fn)
        {
            string[] parts = fn.Split('.');
            return parts[parts.Length - 1];
        }

        public static string getSafeFilename(string f)
        {
            string[] parts = f.Split('\\');
            string curName = parts[parts.Length - 1];
            return curName;
        }

        public static String getCRC(string filename)
        {
            var crc32 = new Crc32();
            String hash = String.Empty;

            using (FileStream fs = File.Open(filename, FileMode.Open))
                foreach (byte b in crc32.ComputeHash(fs))
                    hash += b.ToString("x2").ToLower();
            return hash;
        }

        public static byte[] ReverseByteOrder(byte[] original, int length)
        {
            var newArr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                newArr[length - i - 1] = original[i];
            }
            return newArr;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            var bytes = new byte[NumberChars/2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string GetMD5(string s)
        {
            //Declarations
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = Encoding.Default.GetBytes(s);
            encodedBytes = md5.ComputeHash(originalBytes);

            //Convert encoded bytes back to a 'readable' string
            return BitConverter.ToString(encodedBytes);
        }

        public static DataGridCell GetCell(DataGrid dataGrid, int row, int col)
        {
            return (dataGrid.Items[row] as DataRowView).Row.ItemArray[col] as DataGridCell;
        }
    }
}