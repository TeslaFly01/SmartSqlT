using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TSqlFormatter;
using TSqlFormatter.Formatters;

namespace SmartSQL.Framework
{
    public static class TsqlFormatHelper
    {
        /// <summary>
        /// Sql格式化
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="caseWrite"></param>
        /// <returns></returns>
        public static string SqlFormat(this string strSql, bool caseWrite = true)
        {
            string result;
            if (string.IsNullOrEmpty(strSql))
                return strSql;
            try
            {
                var errorsEncountered = false;
                var formaterManager = new SqlFormattingManager();
                var formater = (TSqlStandardFormatter)formaterManager.Formatter;
                formater.Options.UppercaseKeywords = caseWrite;
                result = formaterManager.Format(strSql, ref errorsEncountered);
            }
            catch (Exception)
            {
                result = strSql;
            }
            return result;
        }

        public static string SqlFormatToString(string strSql, bool caseWrite)
        {
            return SqlFormat(strSql, caseWrite);
        }

        /// <summary>
        /// sql压缩
        /// </summary>
        /// <param name="strsql"></param>
        /// <returns></returns>
        public static string CompressToString(string strSql)
        {
            string result = string.Empty;
            strSql = strSql.Replace("//", "");
            strSql = strSql.Replace('\r', ' ');
            strSql = strSql.Replace('\n', ' ');

            char[] char_array = strSql.ToCharArray();
            var char_list = new List<char>();
            int space_count = 0;
            for (int i = 0, len = char_array.Length; i < len; i++)
            {
                char chr = char_array[i];
                if (chr == ' ')
                {
                    space_count++;
                    if (space_count == 1)
                    {
                        char_list.Add(chr);
                    }
                }
                if (chr != ' ')
                {
                    space_count = 0;
                    char_list.Add(chr);
                }
            }
            result = string.Concat(char_list.ToArray());
            return result;
        }

    }
}
