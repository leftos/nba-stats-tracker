using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using HtmlAgilityPack;

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

    public static class EventHandlers
    {
        private static bool _isTabPressed;

        public static void WPFDataGrid_RowEditEnding_GoToNewRowOnTab(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (_isTabPressed && e.EditAction == DataGridEditAction.Commit)
            {
                DataGrid dataGrid = sender as DataGrid;

                if (e.Row.Item == dataGrid.Items[dataGrid.Items.Count - 2])
                {
                    Window parentWindow = Window.GetWindow(dataGrid);
                    parentWindow.Dispatcher.BeginInvoke(new DispatcherOperationCallback((param) =>
                    {
                        dataGrid.Focus();
                        dataGrid.SelectedIndex = dataGrid.Items.IndexOf(CollectionView.NewItemPlaceholder);
                        dataGrid.CurrentCell = new DataGridCellInfo(CollectionView.NewItemPlaceholder,
                                                                    dataGrid.Columns[0]);

                        //dataGrid.BeginEdit();
                        return null;
                    }), DispatcherPriority.Background, new object[] { null });
                }
            }
        }

        public static void Any_PreviewKeyDown_CheckTab(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                _isTabPressed = true;
            }
        }

        public static void Any_PreviewKeyUp_CheckTab(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                _isTabPressed = false;
            }
        }
        
    }

    public class Crc32 : HashAlgorithm
    {
        public const UInt32 DefaultPolynomial = 0xedb88320;
        public const UInt32 DefaultSeed = 0xffffffff;
        private static UInt32[] defaultTable;

        private readonly UInt32 seed;
        private readonly UInt32[] table;
        private UInt32 hash;

        public Crc32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public override sealed void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public static UInt32 Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            var createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                var entry = (UInt32) i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new[]
                       {
                           (byte) ((x >> 24) & 0xff),
                           (byte) ((x >> 16) & 0xff),
                           (byte) ((x >> 8) & 0xff),
                           (byte) (x & 0xff)
                       };
        }
    }
}