using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartSQL.Framework.Util
{
    public static class StringExtension
    {
        public static string CamelCaseName(this string str)
        {
            if (String.IsNullOrEmpty(str)) return str;
            string[] words = Regex.Split(str, "[_\\-\\. ]");
            return string.Join("", words.Select(FirstCharToUpper));
        }

        public static string LowerCamelCaseName(this string str)
        {
            if (String.IsNullOrEmpty(str)) return str;
            string[] words = Regex.Split(str, "[_\\-\\. ]");
            return string.Join("", words.Select(FirstCharToLower));
        }

        public static string FirstCharToLower(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.Length == 0)
                return str;
            if (str.Length == 1) return str.ToLower();
            return str.Substring(0, 1).ToLower() + str.Substring(1);
        }

        public static string FirstCharToUpper(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.Length == 0)
                return str;
            if (str.Length == 1) return str.ToUpper();
            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// 处理非法字符路径
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FilterIllegalDir(this string str)
        {
            if (str.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                str = string.Join(" ", str.Split(Path.GetInvalidFileNameChars()));
            }
            return str;
        }

        public static string ToEmpty(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.Length == 0)
                return string.Empty;
            if (str.Trim().Equals("''")) return string.Empty;
            return str;
        }

        public static string SingleQuoteToDoubleQuote(this string str)
        {
            if (String.IsNullOrEmpty(str) || str.Length == 0)
                return str;
            return Regex.Replace(str, "[']", "\"");
        }
    }
}
