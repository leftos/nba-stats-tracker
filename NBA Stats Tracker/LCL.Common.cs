using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LeftosCommonLibrary
{
    class Tools
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
            Crc32 crc32 = new Crc32();
            String hash = String.Empty;

            using (FileStream fs = File.Open(filename, FileMode.Open))
                foreach (byte b in crc32.ComputeHash(fs))
                    hash += b.ToString("x2").ToLower();
            return hash;
        }

        public static byte[] ReverseByteOrder(byte[] original, int length)
        {
            byte[] newArr = new byte[length];
            for (int i = 0; i < length; i++)
            {
                newArr[length - i - 1] = original[i];
            }
            return newArr;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
