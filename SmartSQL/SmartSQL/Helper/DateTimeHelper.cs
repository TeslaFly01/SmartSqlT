using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Helper
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// 将Unix时间戳转换为时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixToDateTime(long timeStamp)
        {
            DateTime dtStart = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1), TimeZoneInfo.Local); //表示本地时区
            long lTime = long.Parse(timeStamp.ToString() + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 将时间转换为Unix时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long DateTimeToUnix(DateTime time)
        {
            DateTime startDt = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);
            return (long)(time - startDt).TotalSeconds;
        }
    }
}
