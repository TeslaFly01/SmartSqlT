using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace SmartCode.DocUtils
{
    public static class RazorTpl
    {
        static RazorTpl()
        {
            var config = new TemplateServiceConfiguration();
            config.Language = Language.CSharp;
            config.EncodedStringFactory = new RawStringFactory();
            config.DisableTempFileLocking = true;
            config.Namespaces.Add("RazorEngine");
            //config.EncodedStringFactory = new HtmlEncodedStringFactory();
            var service = RazorEngineService.Create(config);
            Engine.Razor = service;
        }

        public static string RazorRender(this FileInfo tpl_file, object model, string encoding = "utf-8")
        {
            try
            {
                var tpl_text = File.ReadAllText(tpl_file.FullName, System.Text.Encoding.GetEncoding(encoding));

                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), null, model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string RazorRender(this string tpl_text, object model)
        {
            try
            {
                return Engine.Razor.RunCompile(tpl_text, Md5(tpl_text), null, model);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 字符串的Md5值
        /// </summary>
        private static string Md5(string value)
        {
            if (value == null)
                return null;
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
