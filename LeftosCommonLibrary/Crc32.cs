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
using System.IO;
using System.Linq;
using System.Security.Cryptography;

#endregion

namespace LeftosCommonLibrary
{
    /// <summary>
    ///     Provides methods to calculate CRC32 (32-bit cyclic redundancy check) hashes of byte arrays.
    /// </summary>
    public sealed class Crc32 : HashAlgorithm
    {
        private const UInt32 DefaultPolynomial = 0xedb88320;
        private const UInt32 DefaultSeed = 0xffffffff;
        private static UInt32[] _defaultTable;

        private readonly UInt32 _seed;
        private readonly UInt32[] _table;
        private UInt32 _hash;

        /// <summary>
        ///     Default constructor for the Crc32 class, using the default polynomial and seed.
        /// </summary>
        public Crc32()
        {
            _table = initializeTable(DefaultPolynomial);
            _seed = DefaultSeed;
            Initialize();
        }

        /// <summary>
        ///     Constructor for the Crc32 class, with user-set polynomial and seed.
        /// </summary>
        /// <param name="polynomial">The polynomial used to initialize the table.</param>
        /// <param name="seed">The seed used to calculate the hash.</param>
        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            _table = initializeTable(polynomial);
            _seed = seed;
            Initialize();
        }

        /// <summary>
        ///     Overriden property; returns 32 because of the 32-bit implementation.
        /// </summary>
        public override int HashSize
        {
            get { return 32; }
        }

        public override void Initialize()
        {
            _hash = _seed;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            _hash = calculateHash(_table, _hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public static UInt32 Compute(byte[] buffer)
        {
            return ~calculateHash(initializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return ~calculateHash(initializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~calculateHash(initializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static UInt32[] initializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && _defaultTable != null)
                return _defaultTable;

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
                _defaultTable = createTable;

            return createTable;
        }

        private static UInt32 calculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private static byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new[] {(byte) ((x >> 24) & 0xff), (byte) ((x >> 16) & 0xff), (byte) ((x >> 8) & 0xff), (byte) (x & 0xff)};
        }

        /// <summary>
        ///     Calculates the CRC32 of a specified file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="ignoreFirst4Bytes">Whether to ignore the first 4 bytes of the file; should be used when the those bytes are the CRC itself.</param>
        /// <returns>A hex string representation of the CRC32 hash.</returns>
        public static String CalculateCRC(string path, bool ignoreFirst4Bytes = false)
        {
            var crc32 = new Crc32();
            String hash = String.Empty;

            byte[] file;
            file = !ignoreFirst4Bytes ? File.ReadAllBytes(path) : File.ReadAllBytes(path).Skip(4).ToArray();
            return CalculateCRC(file);
        }

        /// <summary>
        ///     Calculates the CRC32 of a specified byte array.
        /// </summary>
        /// <param name="file">The byte array of which to calculate the CRC32 hash.</param>
        /// <returns>A hex string representation of the CRC32 hash.</returns>
        public static String CalculateCRC(byte[] file)
        {
            var crc32 = new Crc32();

            return crc32.ComputeHash(file).Aggregate(String.Empty, (current, b) => current + b.ToString("x2").ToLower());
        }
    }
}