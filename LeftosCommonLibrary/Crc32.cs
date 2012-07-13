#region Copyright Notice

// Created by Damien Guard (c) 2006-2012
// Source: http://damieng.com/blog/2006/08/08/calculating_crc32_in_c_and_net
//
// Included in LeftosCommonLibrary by Lefteris Aslanoglou (c) 2011-2012, as part of
// implementation of thesis
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
using System.Security.Cryptography;

#endregion

namespace LeftosCommonLibrary
{
    public sealed class Crc32 : HashAlgorithm
    {
        private const UInt32 DefaultPolynomial = 0xedb88320;
        private const UInt32 DefaultSeed = 0xffffffff;
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

        public override void Initialize()
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