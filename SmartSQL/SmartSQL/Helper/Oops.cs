using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HandyControl.Controls;
using HandyControl.Data;

namespace SmartSQL.Helper
{
    public static class Oops
    {
        /// <summary>
        /// 警示、提醒、提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="showDateTime"></param>
        public static void Oh(string msg, bool showDateTime = false)
        {
            var growInfo = new GrowlInfo
            {
                Message = msg,
                ShowDateTime = showDateTime,
                WaitTime = 1
            };
            Growl.Warning(growInfo);
        }

        /// <summary>
        /// 警示、提醒、提示全局信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="showDateTime"></param>
        public static void OhGlobal(string msg, bool showDateTime = false)
        {
            var growInfo = new GrowlInfo
            {
                Message = msg,
                ShowDateTime = showDateTime,
                WaitTime = 1
            };
            Growl.WarningGlobal(growInfo);
        }

        /// <summary>
        /// 成功提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="showDateTime"></param>
        public static void Success(string msg, bool showDateTime = false)
        {
            var growInfo = new GrowlInfo
            {
                Message = msg,
                ShowDateTime = showDateTime,
                WaitTime = 1
            };
            Growl.Success(growInfo);
        }

        /// <summary>
        /// 成功全局提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="showDateTime"></param>
        public static void SuccessGlobal(string msg, bool showDateTime = false)
        {
            var growInfo = new GrowlInfo
            {
                Message = msg,
                ShowDateTime = showDateTime,
                WaitTime = 1
            };
            Growl.SuccessGlobal(growInfo);
        }

        /// <summary>
        /// 错误、异常提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="showDateTime"></param>
        public static void God(string msg, bool showDateTime = false)
        {
            var growInfo = new GrowlInfo
            {
                Message = msg,
                ShowDateTime = showDateTime,
                WaitTime = 1
            };
            Growl.Error(growInfo);
        }

        /// <summary>
        /// 错误、异常全局提示信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="showDateTime"></param>
        public static void GodGlobal(string msg, bool showDateTime = false)
        {
            var growInfo = new GrowlInfo
            {
                Message = msg,
                ShowDateTime = showDateTime,
                WaitTime = 1
            };
            Growl.ErrorGlobal(growInfo);
        }
    }
}
