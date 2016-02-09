using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.HashAlgorithms;

namespace Core
{
    public static class Hashes
    {
        public static byte[] GetHash(string input, string algorithm = null, bool managed = true)
        {
            HashAlgorithm ha = GetHashAlgorithm(algorithm ?? "Default", managed);
            return GetHash(input, ha);
        }

        public static byte[] GetHash(string input, HashAlgorithm algorithm = null, Encoding encoding = null)
        {
            algorithm = algorithm ?? GetHashAlgorithm();

            return algorithm.ComputeHash(input.ToBytes(encoding));
        }

        public static byte[] GetHash(byte[] input, HashAlgorithm algorithm = null)
        {
            algorithm = algorithm ?? GetHashAlgorithm();

            return algorithm.ComputeHash(input);
        }

        public static HashAlgorithm GetHashAlgorithm(string algorithm = "Default", bool managed = true)
        {
            switch (algorithm.ToAlphanumeric().ToUpper())
            {
                case "SHA512":
                case "512":
                case "DEFAULT":
                    return managed ? (HashAlgorithm)new SHA512Managed() : new SHA512CryptoServiceProvider();

                case "SHA256":
                case "256":
                    return managed ? (HashAlgorithm)new SHA256Managed() : new SHA256CryptoServiceProvider();

                case "SHA384":
                case "384":
                    return managed ? (HashAlgorithm)new SHA384Managed() : new SHA384CryptoServiceProvider();

                case "SHA1":
                case "1":
                    return managed ? (HashAlgorithm)new SHA1Managed() : new SHA1CryptoServiceProvider();

                case "MD5":
                case "MD":
                case "5":
                    //Unmanaged Only
                    return new MD5CryptoServiceProvider();

                case "CRC32":
                case "CRC":
                    return new CRC32();

                case "CRC32C":
                case "Castagnoli":
                    return new CRC32C();

                case "CRC32K":
                case "Koopman":
                    return new CRC32K();

                case "CRC16":
                    return new CRC16();

                case "CRC16C":
                case "CRC16CCITT":
                    return new CRC16CCITT();

                case "RIPEMD":
                case "RIP":
                case "EMD":
                case "160":
                    //Managed Only
                    return new RIPEMD160Managed();

                default:
                    throw new NotSupportedException(String.Format("Unsupported Hash Algorithm ({0})", algorithm));
            }
        }
    }
}
