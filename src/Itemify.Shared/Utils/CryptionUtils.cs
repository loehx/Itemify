using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Itemify.Shared.Utils
{
    public static partial class CryptionUtils
    {

        /// <summary>
        /// Generates 4 byte keys in order.
        /// Max keys: 16,777,216
        /// </summary>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static IEnumerable<string> Generate4BytesProductKeys(string salt)
        {
            if (salt == null) throw new ArgumentNullException(nameof(salt));

            const int length = 3;
            var saltBytes = salt.GetBytes();
            var max = Math.Pow(2, 8 * length);

            unchecked
            {
                for (var i = 0; i < max; i++)
                {
                    var bytes = BitConverter.GetBytes(i);

                    for (var j = 0; j < saltBytes.Length; j++)
                    {
                        bytes[j % length] ^= saltBytes[j];
                    }

                    bytes[3] = (byte)(bytes[0] ^ bytes[1] ^ bytes[2]);

                    yield return bytes.MapHexBytes(0, bytes.Length, "ABCDEHKMOSTUWXYZ");
                }
            }
        }

        public static bool Is4BytesProductKey(string key)
        {
            if (key == null || key.Length != 8)
                return false;

            try
            {
                var bytes = UnmapHexedBytes(key, "ABCDEHKMOSTUWXYZ");
                unchecked
                {
                    return bytes[3] == (byte)(bytes[0] ^ bytes[1] ^ bytes[2]);
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err);
                return false;
            }
        }

        public static byte[] ContractToLength(this byte[] source, int targetSize)
        {
            var result = new byte[targetSize];

            for (var i = 0; i < source.Length; i++)
            {
                if (targetSize < i)
                    result[i] = source[i];
                else
                    result[i % targetSize] ^= source[i];
            }

            return result;
        }

        /// <summary>
        /// Gibt ein Byte-Array als String wieder.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetString(this byte[] source)
        {
            return Encoding.Default.GetString(source);
        }

        /// <summary>
        /// Gibt die Bytes eines String zurück.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this string source)
        {
            return Encoding.Default.GetBytes(source);
        }


#if NET_CORE

        private abstract class Encoding : System.Text.Encoding
        {
            public static System.Text.Encoding Default { get { return System.Text.Encoding.GetEncoding(0); } }

        }
#endif


        /// <summary>
        /// Generiert ein Base 64 String aus einem String.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GenerateBase64String(this string str)
        {
            return GenerateBase64String(str.GetBytes());
        }

        /// <summary>
        /// Generiert ein Base 64 String aus einem Byte-Array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string GenerateBase64String(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }


        /// <summary>
        /// Liest einen Base64-String ein und gibt das Ergebnis als String zurück.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ParseBase64String(this string data)
        {
            return Convert.FromBase64String(data).GetString();
        }

        /// <summary>
        /// Generiert ein Base 64 Byte-Array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] ParseBase64(this string data)
        {
            return Convert.FromBase64String(data);
        }



        /// <summary>
        /// Generiert einen MD5-Hashcode aus einem String.
        /// </summary>
        /// <param name="string"></param>
        /// <returns></returns>
        public static byte[] GenerateMD5(this string @string)
        {
            return GenerateMD5(@string.GetBytes());
        }

        /// <summary>
        /// Generiert einen MD5-Hashcode aus einem Byte-Array.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GenerateMD5(this byte[] data)
        {
            return MD5.Create().ComputeHash(data);
        }





        public static byte[] GenerateSH1(this byte[] data)
        {
            return SHA1.Create().ComputeHash(data);
        }





        private static byte[] HEX_UPPER_VALUES = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                                 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D,
                                 0x0E, 0x0F };

        /// <summary>
        /// Gibt den verschlüsselten Wert eines Hex-Strings zurück.
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        public static byte[] ParseUpperHex(this string Hex)
        {
            byte[] Bytes = new byte[Hex.Length / 2];

            for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
            {
                Bytes[x] = (byte)(HEX_UPPER_VALUES[(char)(Hex[i + 0]) - '0'] << 4 |
                                  HEX_UPPER_VALUES[(char)(Hex[i + 1]) - '0']);
            }

            return Bytes;
        }




        private static byte[] HEX_LOWER_VALUES = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
                                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0,
                                0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F};

        /// <summary>
        /// Gibt den verschlüsselten Wert eines Hex-Strings zurück.
        /// </summary>
        /// <param name="Hex"></param>
        /// <returns></returns>
        public static byte[] ParseLowerHex(this string Hex)
        {
            byte[] Bytes = new byte[Hex.Length / 2];

            for (int x = 0, i = 0; i < Hex.Length; i += 2, x += 1)
            {
                Bytes[x] = (byte)(HEX_LOWER_VALUES[(char)(Hex[i + 0]) - '0'] << 4 |
                                  HEX_LOWER_VALUES[(char)(Hex[i + 1]) - '0']);
            }

            return Bytes;
        }




        /// <summary>
        /// "Hallo" >> "456A2BCFE3FA12"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToLowerHex(this string text)
        {
            return text.GetBytes().ToLowerHex();
        }

        /// <summary>
        /// "Hallo" >> "456a2bcfe3fa12"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToUpperHex(this string text)
        {
            return text.GetBytes().ToUpperHex();
        }


        private const string HEX_ALPHABET_LOWER = "0123456789abcdef";

        /// <summary>
        /// { 0x65, 0xf1, 0xab, ... } >> "456A2BCFE3FA12"
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToLowerHex(this byte[] bytes)
        {
            return MapHexBytes(bytes, 0, bytes.Length, HEX_ALPHABET_LOWER);
        }


        private const string HEX_ALPHABET_UPPER = "0123456789ABCDEF";

        /// <summary>
        /// { 0x65, 0xf1, 0xab, ... } >> "456a2bcfe3fa12"
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="alphabet">0123456789ABCDEF</param>
        /// <returns></returns>
        public static string ToUpperHex(this byte[] bytes, string alphabet = HEX_ALPHABET_UPPER)
        {
            return MapHexBytes(bytes, 0, bytes.Length, HEX_ALPHABET_UPPER);
        }


        /// <summary>
        /// { 0x65, 0xf1, 0xab, ... } >> "456a2bcfe3fa12"
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="alphabet">0123456789ABCDEF</param>
        /// <returns></returns>
        public static string MapHexBytes(this byte[] bytes, int offset, int length, string alphabet)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            if (alphabet == null) throw new ArgumentNullException(nameof(alphabet));
            if (alphabet.Length < 16)
                throw new ArgumentException($"Parameter {nameof(alphabet)} must have a length of 16 characters.");

            var res = new char[length * 2];

            unchecked
            {
                for (int i = 0; i < length; i++)
                {
                    res[i * 2] = (alphabet[(int)(bytes[i + offset] >> 4)]);
                    res[i * 2 + 1] = (alphabet[(int)(bytes[i + offset] & 0xF)]);
                }
            }

            return new string(res);
        }


        public static byte[] UnmapHexedBytes(this string source, string alphabet)
        {
            byte[] res = new byte[source.Length / 2];
            var characters = alphabet.ToCharArray();

            unchecked
            {
                for (int x = 0, i = 0; i < source.Length; i += 2, x += 1)
                {
                    var a = Array.IndexOf(characters, source[i]);
                    if (a == -1)
                        throw new Exception($"Character '{source[i]}' could not be found in alphabet: '{alphabet}'.");

                    var b = Array.IndexOf(characters, source[i + 1]);
                    if (b == -1)
                        throw new Exception($"Character '{source[i + 1]}' could not be found in alphabet: '{alphabet}'.");

                    res[x] = (byte)(a << 4 | b);
                }
            }

            return res;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ConvertToHex(this byte[] bytes)
        {
            byte[] res = new byte[bytes.Length * 2];

            unchecked
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    res[i * 2] = (byte)(bytes[i] >> 4);
                    res[i * 2 + 1] = (byte)(bytes[i] & 0xF);
                }
            }

            return res;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static byte[] ParseHex(byte[] bytes)
        {
            byte[] res = new byte[bytes.Length / 2];

            unchecked
            {
                for (int x = 0, i = 0; i < bytes.Length; i += 2, x += 1)
                {
                    res[x] = (byte)(bytes[i + 0] << 4 | bytes[i + 1]);
                }
            }

            return res;
        }



#if !NET_CORE
        //Apparantly PasswordDeriveBytes are comming to net core so there is no point in replacing them
        public static CryptoStream Encrypt(Stream data, string key)
        {
            return Encrypt(data, key.GetBytes());
        }


        public static CryptoStream Encrypt(Stream data, byte[] key)
        {
            Rijndael rijndeal = Rijndael.Create();
            PasswordDeriveBytes password = new PasswordDeriveBytes(key, key);

            var encryptor = rijndeal.CreateEncryptor(password.GetBytes(32), password.GetBytes(16));

            return new CryptoStream(data, encryptor, CryptoStreamMode.Read);
        }
        


        public static CryptoStream Decrypt(Stream data, string key)
        {
            return Decrypt(data, key.GetBytes());
        }


        public static CryptoStream Decrypt(Stream data, byte[] key)
        {
            Rijndael rijndeal = Rijndael.Create();
            PasswordDeriveBytes password = new PasswordDeriveBytes(key, key);

            var encryptor = rijndeal.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));

            return new CryptoStream(data, encryptor, CryptoStreamMode.Read);
        }







        public static byte[] Encrypt(this byte[] data, string key, CipherMode mode = CipherMode.CBC)
        {
            return Encrypt(data, key.GetBytes(), mode);
        }

        public static byte[] Encrypt(this byte[] data, byte[] key, CipherMode mode = CipherMode.CBC)
        {
            return Encrypt(data, 0, data.Length, key, mode);
        }



        public static byte[] Encrypt(this byte[] data, int index, int count, byte[] key, CipherMode mode = CipherMode.CBC)
        {
            try
            {
                Rijndael rijndeal = Rijndael.Create();
                rijndeal.Mode = mode;
                PasswordDeriveBytes password = new PasswordDeriveBytes(key, key);
                var encryptor = rijndeal.CreateEncryptor(password.GetBytes(32), password.GetBytes(16));
                return encryptor.TransformFinalBlock(data, index, count);
            }
            catch (Exception err)
            {
                throw new Exception("Could not encrypt data!", err);
            }
        }





        public static byte[] Decrypt(this byte[] data, string key, CipherMode mode = CipherMode.CBC)
        {
            return Decrypt(data, key.GetBytes(), mode);
        }

        public static byte[] Decrypt(this byte[] data, byte[] key, CipherMode mode = CipherMode.CBC)
        {
            return Decrypt(data, 0, data.Length, key, mode);
        }



        public static byte[] Decrypt(this byte[] data, int index, int count, byte[] key, CipherMode mode = CipherMode.CBC)
        {
            try
            {
                Rijndael rijndeal = Rijndael.Create();
                rijndeal.Mode = mode;
                PasswordDeriveBytes password = new PasswordDeriveBytes(key, key);
                var encryptor = rijndeal.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));
                return encryptor.TransformFinalBlock(data, index, count);
            }
            catch (Exception err)
            {
                throw new Exception("Could not decrypt data!", err);
            }
        }
#endif


        private static byte[] HexToBytes(string hex)
        {
            return hex.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
        }

        private static string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes);
        }

        public static string EncryptAes(string plainText, string key, string iv)
        {
            byte[] encrypted;

            using (var aes = Aes.Create())
            {
                aes.Key = HexToBytes(key);
                aes.IV = HexToBytes(iv);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptAes(string base64CipherText, string key, string iv)
        {
            var cipherBytes = Convert.FromBase64String(base64CipherText);
            string plainText;

            using (var aes = Aes.Create())
            {
                aes.Key = HexToBytes(key);
                aes.IV = HexToBytes(iv);

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (var msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plainText;
        }

#if !NET_CORE
        public static byte[] NotThrowingDecrypt(this byte[] data, string key, CipherMode mode = CipherMode.CBC)
        {
            return NotThrowingDecrypt(data, key.GetBytes(), mode);
        }

        public static byte[] NotThrowingDecrypt(this byte[] data, byte[] key, CipherMode mode = CipherMode.CBC)
        {
            return NotThrowingDecrypt(data, 0, data.Length, key, mode);
        }



        public static byte[] NotThrowingDecrypt(this byte[] data, int index, int count, byte[] key, CipherMode mode = CipherMode.CBC)
        {
            try
            {
                Rijndael rijndeal = Rijndael.Create();
                rijndeal.Mode = mode;
                PasswordDeriveBytes password = new PasswordDeriveBytes(key, key);
                var encryptor = rijndeal.CreateDecryptor(password.GetBytes(32), password.GetBytes(16));
                return encryptor.TransformFinalBlock(data, index, count);
            }
            catch (Exception err)
            {
                return key.GenerateMD5();
            }
        }




        public static byte[] Widen(this byte[] data, int finalSize)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(data, data);
            return pdb.GetBytes(finalSize);
        }

#endif




        public static byte[] ExclusiveOr(this byte[] dataA, byte[] dataB)
        {
            if (dataA.Length == dataB.Length)
            {
                return ExclusiveOr(dataA, 0, dataB, 0, dataB.Length);
            }
            else
            {
                return ExclusiveOr(dataA, 0, dataA.Length, dataB, 0, dataB.Length);
            }
        }




        public static byte[] ExclusiveOr(byte[] dataA, int indexA, byte[] dataB, int indexB, int count)
        {

            byte[] res = new byte[count];

            for (int i = 0; i < count; i++)
            {
                res[i] = unchecked((byte)(dataA[indexA + i] ^ dataB[indexB + i]));
            }

            return res;
        }



        public static byte[] ExclusiveOr(byte[] dataA, int indexA, int countA, byte[] dataB, int indexB, int countB)
        {
            int min, max;

            if (countA > countB)
            {
                max = countA;
                min = countB;
            }
            else
            {
                max = countB;
                min = countA;
            }


            byte[] res = new byte[max];

            for (int i = 0; i < min; i++)
            {
                res[i] = unchecked((byte)(dataA[indexA + i] ^ dataB[indexB + i]));
            }

            for (int i = min; i < max; i++)
            {
                if (countA > countB)
                {
                    res[i] = dataA[indexA + i];
                }
                else
                {
                    res[i] = dataB[indexB + i];
                }
            }

            return res;
        }


        /* Nur so gemacht...

        public static IEnumerable<int> ToIntegers(this byte[] bytes)
        {
            for (int i = 0; i < bytes.Length - 3; i += sizeof(int))
            {
                yield return BitConverter.ToInt32(bytes, i);
            }

            int res = 0;

            switch (bytes.Length % 4)
            {
                case 3:
                    res |= bytes[bytes.Length - 3] << 24;
                    res |= bytes[bytes.Length - 2] << 16;
                    res |= bytes[bytes.Length - 1] << 8;
                    goto case 2;
                case 2:
                    res |= bytes[bytes.Length - 2] << 24;
                    res |= bytes[bytes.Length - 1] << 16;
                    goto case 1;
                case 1:
                    res |= bytes[bytes.Length - 1] << 24;
                    break;
                default:
                    yield break;
            }

            yield return res;
        }
        */


        public static void ValidatePassword(string password,
            int minLength,
            int maxLength,
            int incNumbersMin = 0,
            int incUpperCharsMin = 0,
            int incSpecialChars = 0,
            bool whitespaces = false,
            params string[] last)
        {
            int numbers = 0,
                uppers = 0,
                specials = 0;

            if (string.IsNullOrEmpty(password))
                throw new Exception("Password can not be empty!");

            if (password.Length < minLength)
                throw new Exception("Password must consist of at least " + minLength + " characters!");

            if (password.Length > maxLength)
                throw new Exception("Password must consist of less than " + maxLength + " characters!");

            if (last != null && last.Contains(password))
                throw new Exception("Password can not be one of the last " + last.Length + "!");

            foreach (var c in password)
            {
                if (Char.IsNumber(c))
                {
                    numbers++;
                }
                else if (Char.IsUpper(c))
                {
                    uppers++;
                }
                else if (!whitespaces && Char.IsWhiteSpace(c))
                {
                    throw new Exception("Password can not contain white spaces!");
                }
                else if (!Char.IsLetterOrDigit(c))
                {
                    specials++;
                }
            }

            if (numbers < incNumbersMin)
                throw new Exception("Password must contain at least " + incNumbersMin + " number(s)!");

            if (uppers < incUpperCharsMin)
                throw new Exception("Password must contain at least " + incUpperCharsMin + " upper letter(s)!");

            if (specials < incSpecialChars)
                throw new Exception("Password must contain at least " + incSpecialChars + " numbers!");
        }
    }

}
