using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Helper
{
    public static class TextEncryptHelper
    {
        /// <summary>
        /// MD5_16位加密
        /// </summary>
        public static string GetMD5_16(string value)
        {
            string md5Str = string.Empty;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] byData = Encoding.Default.GetBytes(value);
            byte[] result = md5.ComputeHash(byData);

            md5Str = BitConverter.ToString(result, 4, 8);
            md5Str = md5Str.Replace("-", "");

            return md5Str;
        }

        /// <summary>
        /// MD5_32位加密
        /// </summary>
        public static string GetMD5_32(string value)
        {
            string md5Str = string.Empty;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] byData = Encoding.Default.GetBytes(value);
            byte[] result = md5.ComputeHash(byData);

            md5Str = BitConverter.ToString(result);
            md5Str = md5Str.Replace("-", "");

            return md5Str;
        }
    }
}
