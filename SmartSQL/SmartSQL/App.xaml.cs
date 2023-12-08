using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;
using HandyControl.Controls;
using NLog.Config;
using NLog.Targets;
using NLog;
using SmartSQL.Helper;
using System.Collections.Generic;
using SmartSQL.Models.Api;
using RestSharp;
using System.Net;
using System.Text.Json;
using System.Linq;

namespace SmartSQL
{
    public partial class App
    {
        #region MyRegion
        private readonly static string CategoryApiUrl = "https://apiv.gitee.io/smartapi/categoryApi.json";
        private readonly static string SiteApiUrl = "https://apiv.gitee.io/smartapi/siteApi.json"; 
        #endregion
        /// <summary>
        /// 导航站点信息
        /// </summary>
        public static List<CategoryApi> SiteInfo;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public App()
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //初始化日志配置
            ConfigureNLog();
            //初始化站点信息
            InitSiteInfo();
            //var splashScreen = new SplashScreen("/Resources/Img/Readme/Cover.png");
            //splashScreen.Show(true);
            //splashScreen.Close(new TimeSpan(0, 0, 23));
            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Logger.Info(e.Exception);
                //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出
                e.Handled = true;
                if (e.Exception is NotImplementedException)
                {
                    Oops.Oh("该功能暂未实现，\r\n请关注公众号【IT搬砖人plus】,\r\n获取后期版本更新通知");
                    return;
                }
                Growl.Warning("程序异常，请稍后再试");
            }
            catch (Exception)
            {
                Growl.Warning("程序异常，请稍后再试");
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //StringBuilder sbEx = new StringBuilder();
            //if (e.IsTerminating)
            //{
            //    sbEx.Append("非UI线程发生致命错误：");
            //}
            //sbEx.Append("非UI线程异常：");
            //if (e.ExceptionObject is Exception)
            //{
            //    sbEx.Append(((Exception)e.ExceptionObject).Message);
            //}
            //else
            //{
            //    sbEx.Append(e.ExceptionObject);
            //}
            Growl.WarningGlobal("程序异常，请稍后再试");
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            //Growl.WarningGlobal("Task线程异常：" + e.Exception.Message);
            Growl.WarningGlobal("程序异常，请稍后再试");
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }

        /// <summary>
        /// 日志配置
        /// </summary>
        private void ConfigureNLog()
        {
            #region MyRegion
            // 创建新的日志配置对象
            var config = new LoggingConfiguration();
            // 创建新的文件日志目标并配置你的需要
            var logfile = new FileTarget("logfile")
            {
                FileName = "logfile.txt",
                Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} [${level:uppercase=true}] ${message} ${exception:format=ToString}"
            };
            // 定义日志等级（例如，Info以上的日志等级）
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            // 应用配置设置
            LogManager.Configuration = config;
            #endregion
        }

        /// <summary>
        /// 初始化站点信息
        /// </summary>
        /// <returns></returns>
        private void InitSiteInfo()
        {
            #region MyRegion
            Task.Run(() =>
               {
                   var client = new RestClient(CategoryApiUrl);
                   var result = client.Execute(new RestRequest());
                   if (result.StatusCode == HttpStatusCode.OK)
                   {
                       var categoryList = JsonSerializer.Deserialize<List<CategoryApi>>(result.Content);
                       client = new RestClient(SiteApiUrl);
                       result = client.Execute(new RestRequest());
                       if (result.StatusCode == HttpStatusCode.OK)
                       {
                           var siteList = JsonSerializer.Deserialize<List<SiteApi>>(result.Content);
                           categoryList.ForEach(x =>
                           {
                               int initType = 0;
                               x.count = siteList.Count(t => t.category == x.categoryName);
                               if (x.type.Any())
                               {
                                   x.type.ForEach(t =>
                                   {
                                       if (initType == 0)
                                       {
                                           x.SelectedType = t;
                                       }
                                       t.sites = siteList.Where(s => s.category == x.categoryName && s.type == t.typeName).ToList();
                                       initType++;
                                   });
                                   if (x.type.Count(t => t.sites.Count > 0) == 1)
                                   {
                                       x.sites = siteList.Where(s => s.category == x.categoryName).ToList();
                                       x.type = new List<CategoryApiType>();
                                       return;
                                   }
                                   x.type = x.type.Where(t => t.sites.Count > 0).ToList();
                                   return;
                               }
                               x.sites = siteList.Where(s => s.category == x.categoryName).ToList();
                           });
                           SiteInfo = categoryList.Where(x => x.isEnable).ToList();
                       }
                   }
               }); 
            #endregion
        }
    }
}
