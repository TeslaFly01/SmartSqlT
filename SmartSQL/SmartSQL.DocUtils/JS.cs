using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.DocUtils;
using Jint;

namespace SmartSQL.DocUtils
{
    public class JS
    {
        private static Engine jt = null;
        static JS()
        {
            jt = new Engine(cfg =>
            {
                //cfg.LimitRecursion();
                cfg.Strict();
            });
            jt.SetValue("log", new Action<object>(Console.WriteLine)); 
            
            var text = string.Empty;
            var names = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var name in names)
            {
                if (Path.GetExtension(name) == ".js")
                {
                    text = System.Reflection.Assembly.GetExecutingAssembly().GetResourceContent(name);
                    try
                    {
                        jt.Execute(text);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        /// summary>
        /// https://zeroturnaround.github.io/sql-formatter/
        /// 格式化SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="dbtype">数据库类型</param>
        /// <returns>格式化后的SQL语句</returns>
        public static string RunFmtSql(string sql, string dbtype)
        {
            var supportLang = new List<string>()
            {
                "sql",
                "redshift",
                "db2",
                "mariadb",
                "mysql",
                "n1ql",
                "plsql",
                "postgresql",
                "spark",
                "tsql"
            };

            var lang = dbtype.ToLower();

            if (!supportLang.Contains(lang))
            {
                if (lang.StartsWith("oracle"))
                {
                    lang = "plsql";
                }
                else if (lang == "sqlserver")
                {
                    lang = "tsql";
                }
                else
                {
                    lang = "sql";
                }
            }

            sql = sql.Replace("`", "");
            var code = $@"var fmtSql = sqlFormatter.format(`{ sql }`, {{language: ""{ lang }"",uppercase: true }});";
            var res = sql;
            try
            {
                jt.Execute(code);
                res = jt.GetValue("fmtSql").ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return res;
        }

        /// <summary>
        /// https://highlightjs.org/static/demo/
        /// 9.9版本
        /// 返回带有css样式的高亮SQL代码
        /// </summary>
        /// <param name="sql">格式化后的SQL代码</param>
        /// <param name="dbtype">数据库类型</param>
        /// <returns></returns>
        public static string RunHighlightHtml(string sql, string dbtype)
        {
            dbtype = dbtype.ToLower();
            var lang = "sql";

            //if (dbtype.StartsWith("oracle"))
            //{
            //    lang = "ruleslanguage";
            //}
            //else if (dbtype == "postgresql")
            //{
            //    lang = "pgsql";
            //}

            sql = sql.Replace("`", "");
            var code = $@"var hlhtml = hljs.highlight(""{ lang }"",`{ sql }`,true).value;";
            var res = sql;
            try
            {
                jt.Execute(code);
                res = jt.GetValue("hlhtml").ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return res;
        }


        public static string RunStyleSql(string sql, string dbtype)
        {
            var fmtSql = RunFmtSql(sql, dbtype);

            var reSql = RunHighlightHtml(fmtSql, dbtype);

            return reSql;
        }
    }
}

