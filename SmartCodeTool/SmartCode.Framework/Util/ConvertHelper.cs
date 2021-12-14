using System;

namespace SmartCode.Framework.Util
{
    public static class ConvertHelper
    {
        public static bool GetBoolean(int value)
        {
            return value > 0;
        }

        public static Int64 GetInt64(String str)
        {
            Int64 data = 0;
            Int64.TryParse(str,out data);
            return data;
        }

        public static int GetInt32(String str)
        {
            Int32 data = 0;
            Int32.TryParse(str, out data);
            return data;
        }

        public static short GetInt16(String str)
        {
            Int16 data = 0;
            Int16.TryParse(str, out data);
            return data;
        }

        public static Byte GetByte(String str)
        {
            Byte data = 0;
            Byte.TryParse(str, out data);
            return data;
        }

        public static float GetFloat(String str)
        {
            Single data = 0;
            Single.TryParse(str, out data);
            return data;
        }

        public static double GetDouble(String str)
        {
            Double data = 0;
            Double.TryParse(str, out data);
            return data;
        }

        public static decimal GetDecimal(String str)
        {
            Decimal data = 0;
            Decimal.TryParse(str, out data);
            return data;
        }

        public static Single GetSingle(String str)
        {
            Single data = 0;
            Single.TryParse(str, out data);
            return data;
        }

        public static bool GetBoolean(String str)
        {
            Boolean data = false;
            Boolean.TryParse(str, out data);
            return data;
        }

        public static byte[] GetBytes(String str)
        {
            if (String.IsNullOrEmpty(str) ||
               str.Trim().Length == 0) return null;

            return System.Text.Encoding.Unicode.GetBytes(str);
        }

        public static Guid GetGuid(String str)
        {
            if (String.IsNullOrEmpty(str) || 
                str.Trim().Length == 0) return Guid.Empty;

            Guid data = new Guid(str);
            return data;
        }

        public static DateTime GetDateTime(String str)
        {
            if (String.IsNullOrEmpty(str) || 
                str.Trim().Length == 0) return DateTime.Now;

            DateTime data = DateTime.Now;
            DateTime.TryParse(str, out data);
            return data;
        }
    }
}
