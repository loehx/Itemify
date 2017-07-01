using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Humanizer;

// ReSharper disable once CheckNamespace
namespace Itemify.Shared.Utils
{
    public static class StringExtensions
    {
        public static string[] SplitIntoPiecesWithSize(this string source, int chars)
        {
            var result = new string[(int)Math.Ceiling((double)source.Length / chars)];

            for (var i = 0; (i * chars) < source.Length; i++)
            {
                result[i] = source.Substring(i * chars, chars);
            }

            return result;
        }

        public static string Indent(this string source, int count, char character = ' ')
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Length == 0 || count <= 0) return source;

            var indent = new string(character, count);
            return indent + InsertAfterLineBreaks(source, indent);
        }

        public static string InsertAfterLineBreaks(this string source, string suffix)
        {
            return InsertAfter(source, GetLineBreak(source), suffix);
        }

        public static string InsertAfter(this string source, string search, string suffix)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Replace(search, search + suffix);
        }

        public static string InsertBefore(this string source, string search, string prefix)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return source.Replace(search, prefix + search);
        }

        public static string GetLineBreak(this string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            if (source.IndexOf("\r\n", StringComparison.Ordinal) != -1)
                return "\r\n";

            if (source.IndexOf("\n", StringComparison.Ordinal) != -1)
                return "\n";

            return Environment.NewLine;
        }

        public static string ReplaceLineBreaks(this string source, string replacement)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Length == 0) return source;

            var linebreak = GetLineBreak(source);
            return source.Replace(linebreak, replacement);
        }

        public static string Shorten(this string source, int maxLength, string prefix = "...")
        {
            if (source == null)
                return null;

            if (maxLength < prefix.Length)
                return source.Length >= maxLength ? source.Substring(0, maxLength) : source;

            if (source.Length <= maxLength)
                return source;

            return source.Substring(0, maxLength - prefix.Length) + prefix;
        }

        public static string ShortenLeft(this string source, int maxLength, string prefix = "...")
        {
            if (maxLength < prefix.Length)
                throw new ArgumentOutOfRangeException("maxLength", maxLength, "Must be greater or equal prefix length: " + prefix.Length);

            if (source == null || source.Length <= maxLength)
                return source;

            return prefix + source.Substring(source.Length - maxLength + prefix.Length);
        }

        public static string Join(this IEnumerable<string> source, string separator)
        {
            var sb = new StringBuilder();
            foreach (var item in source)
                sb.Append(item).Append(separator);

            if (sb.Length < separator.Length)
                return sb.ToString();

            sb.Length -= separator.Length;
            return sb.ToString();
        }

        public static T? TryParseEnum<T>(this string value)
            where T : struct
        {
            T result;

            if (Enum.TryParse(value, true, out result))
                return result;

            return null;
        }

        public static bool IsNotEmpty(this string source)
        {
            return !string.IsNullOrEmpty(source);
        }

        public static bool IsEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static string Left(this string str, int length)
        {
            str = (str ?? string.Empty);
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            str = (str ?? string.Empty);
            return (str.Length >= length)
                ? str.Substring(str.Length - length, length)
                : str;
        }


        /// <summary>
        /// Allows case insensitive checks
        /// </summary>
        public static bool Contains(this string source, string toCheck, bool ingoreCase)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Allows case insensitive checks
        /// </summary>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string ToCamelCase(this string text)
        {
            return text.Camelize().RemoveCharacters(c => !char.IsLetterOrDigit(c));
        }

        public static string RemoveCharacters(this string source, Func<char, bool> removeFunc)
        {
            if (source == null) return source;

            var sb = new StringBuilder(source.Length);

            for (int i = 0; i < source.Length; i++)
            {
                var c = source[i];
                if (!removeFunc(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Tries to parse a string to GUID.
        /// If this fails, a MD5 hash creates a new GUID.
        /// Empty or null strings return an empty GUID.
        /// </summary>
        /// <param name="source">GUID or any string</param>
        /// <returns></returns>
        public static Guid ToGuid(this string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return Guid.Empty;

            Guid guid;
            if (Guid.TryParse(source, out guid))
                return guid;

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.Default.GetBytes(source));
                return new Guid(hash);
            }
        }

        public static String RemoveDiacritics(this String s)
        {
            String normalizedString = s.ReplaceGermanUmlauts().Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++)
            {
                Char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }

        public static String ReplaceGermanUmlauts(this String s)
        {
            String t = s;
            t = t.Replace("ä", "ae");
            t = t.Replace("ö", "oe");
            t = t.Replace("ü", "ue");
            t = t.Replace("Ä", "Ae");
            t = t.Replace("Ö", "Oe");
            t = t.Replace("Ü", "Ue");
            t = t.Replace("ß", "ss");
            return t;
        }


        public static String getASCIIString(this string source)
        {
            // Convert the string into a byte array.
            byte[] unicodeBytes = source.GetBytes();

            // Perform the conversion from one encoding to the other.
            byte[] asciiBytes = Encoding.Convert(Encoding.Default, Encoding.ASCII
                , unicodeBytes);

            // Convert the new byte[] into a char[] and then into a string.
            char[] asciiChars = new char[Encoding.ASCII.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            Encoding.ASCII.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            string asciiString = new string(asciiChars);

            return asciiString;
        }
    
    #if NET_CORE

    private abstract class Encoding : System.Text.Encoding {
        public static System.Text.Encoding Default { get { return System.Text.Encoding.GetEncoding(0); }}

    }
    #endif
    }
}