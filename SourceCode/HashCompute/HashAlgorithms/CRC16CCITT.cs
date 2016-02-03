using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace HashCompute.HashAlgorithms
{
    /// <summary>
    /// Implements a 16-bit CRC (CCITT) hash algorithm compatible with Zip etc.
    /// </summary>
    /// <remarks>
    /// Crc32 should only be used for backward compatibility with older file formats
    /// and algorithms. It is not secure enough for new applications.
    /// If you need to call multiple times for the same data either use the HashAlgorithm
    /// interface or remember that the result of one Compute call needs to be ~ (XOR) before
    /// being passed in as the seed for the next Compute call.
    /// </remarks>
    public sealed class CRC16CCITT : HashAlgorithm
    {
        public const UInt16 DefaultPolynomial = 0x8408;
        public const UInt16 DefaultSeed = 0xffff;

        static UInt16[] defaultTable;

        readonly UInt16 seed;
        readonly UInt16[] table;
        UInt16 hash;

        public CRC16CCITT() : this(DefaultPolynomial, DefaultSeed)
        {
        }

        public CRC16CCITT(UInt16 polynomial, UInt16 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = hash = seed;
        }

        public override void Initialize()
        {
            hash = seed;
        }

        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            hash = CalculateHash(table, hash, array, ibStart, cbSize);
        }

        protected override byte[] HashFinal()
        {
            var hashBuffer = UInt16ToBigEndianBytes((UInt16)(~hash));
            HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize { get { return 16; } }

        public static UInt16 Compute(byte[] buffer)
        {
            return Compute(DefaultSeed, buffer);
        }

        public static UInt16 Compute(UInt16 seed, byte[] buffer)
        {
            return Compute(DefaultPolynomial, seed, buffer);
        }

        public static UInt16 Compute(UInt16 polynomial, UInt16 seed, byte[] buffer)
        {
            return (UInt16)(~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length));
        }

        static UInt16[] InitializeTable(UInt16 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            var createTable = new UInt16[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (UInt16)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (UInt16)((entry >> 1) ^ polynomial);
                    else
                        entry = (UInt16)(entry >> 1);
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        static UInt16 CalculateHash(UInt16[] table, UInt16 seed, IList<byte> buffer, int start, int size)
        {
            var crc = seed;
            for (var i = start; i < size - start; i++)
                crc = (UInt16)((crc >> 8) ^ table[buffer[i] ^ crc & 0xff]);
            return crc;
        }

        static byte[] UInt16ToBigEndianBytes(UInt16 UInt16)
        {
            var result = BitConverter.GetBytes(UInt16);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
    }
}