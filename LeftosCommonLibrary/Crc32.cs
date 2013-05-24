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
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    #endregion

    /// <summary>Provides methods to calculate CRC32 (32-bit cyclic redundancy check) hashes of byte arrays.</summary>
    public sealed class Crc32 : HashAlgorithm
    {
        private const UInt32 DefaultPolynomial = 0xedb88320;
        private const UInt32 DefaultSeed = 0xffffffff;
        private static UInt32[] _defaultTable;

        private readonly UInt32 _seed;
        private readonly UInt32[] _table;
        private UInt32 _hash;

        /// <summary>Default constructor for the Crc32 class, using the default polynomial and seed.</summary>
        public Crc32()
        {
            _table = initializeTable(DefaultPolynomial);
            _seed = DefaultSeed;
            Initialize();
        }

        /// <summary>Constructor for the Crc32 class, with user-set polynomial and seed.</summary>
        /// <param name="polynomial">The polynomial used to initialize the table.</param>
        /// <param name="seed">The seed used to calculate the hash.</param>
        public Crc32(UInt32 polynomial, UInt32 seed)
        {
            _table = initializeTable(polynomial);
            _seed = seed;
            Initialize();
        }

        /// <summary>Overriden property; returns 32 because of the 32-bit implementation.</summary>
        public override int HashSize
        {
            get { return 32; }
        }

        /// <summary>
        ///     Initializes an implementation of the <see cref="T:System.Security.Cryptography.HashAlgorithm" /> class.
        /// </summary>
        public override void Initialize()
        {
            _hash = _seed;
        }

        /// <summary>When overridden in a derived class, routes data written to the object into the hash algorithm for computing the hash.</summary>
        /// <param name="array">The input to compute the hash code for. </param>
        /// <param name="ibStart">The offset into the byte array from which to begin using data. </param>
        /// <param name="cbSize">The number of bytes in the byte array to use as data. </param>
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _hash = calculateHash(_table, _hash, array, ibStart, cbSize);
        }

        /// <summary>
        ///     When overridden in a derived class, finalizes the hash computation after the last data is processed by the cryptographic
        ///     stream object.
        /// </summary>
        /// <returns>The computed hash code.</returns>
        protected override byte[] HashFinal()
        {
            var hashBuffer = uInt32ToBigEndianBytes(~_hash);
            HashValue = hashBuffer;
            return hashBuffer;
        }

        /// <summary>Computes the hash of the specified buffer.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash of the buffer.</returns>
        public static UInt32 Compute(byte[] buffer)
        {
            return ~calculateHash(initializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        /// <summary>Computes the hash of the specified buffer.</summary>
        /// <param name="seed">The seed to use.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash of the buffer.</returns>
        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return ~calculateHash(initializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>Computes the hash of the specified buffer.</summary>
        /// <param name="polynomial">The polynomial to initialize the table with.</param>
        /// <param name="seed">The seed to use.</param>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash of the buffer.</returns>
        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~calculateHash(initializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static UInt32[] initializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && _defaultTable != null)
            {
                return _defaultTable;
            }

            var createTable = new UInt32[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (UInt32) i;
                for (var j = 0; j < 8; j++)
                {
                    if ((entry & 1) == 1)
                    {
                        entry = (entry >> 1) ^ polynomial;
                    }
                    else
                    {
                        entry = entry >> 1;
                    }
                }
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
            {
                _defaultTable = createTable;
            }

            return createTable;
        }

        private static UInt32 calculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            var crc = seed;
            for (var i = start; i < size; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            }
            return crc;
        }

        private static byte[] uInt32ToBigEndianBytes(UInt32 x)
        {
            return new[] { (byte) ((x >> 24) & 0xff), (byte) ((x >> 16) & 0xff), (byte) ((x >> 8) & 0xff), (byte) (x & 0xff) };
        }

        /// <summary>Calculates the CRC32 of a specified file.</summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="ignoreFirst4Bytes">
        ///     Whether to ignore the first 4 bytes of the file; should be used when the those bytes are the CRC
        ///     itself.
        /// </param>
        /// <returns>A hex string representation of the CRC32 hash.</returns>
        public static String CalculateCRC(string path, bool ignoreFirst4Bytes = false)
        {
            var file = !ignoreFirst4Bytes ? File.ReadAllBytes(path) : File.ReadAllBytes(path).Skip(4).ToArray();
            return CalculateCRC(file);
        }

        /// <summary>Calculates the CRC32 of a specified byte array.</summary>
        /// <param name="ba">The byte array of which to calculate the CRC32 hash.</param>
        /// <returns>A hex string representation of the CRC32 hash.</returns>
        public static String CalculateCRC(byte[] ba)
        {
            var crc32 = new Crc32();

            return crc32.ComputeHash(ba).Aggregate(String.Empty, (current, b) => current + b.ToString("x2").ToLower());
        }
    }
}