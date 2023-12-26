using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Helper;
using SmartSQL.Models;
using SmartSQL.Views;
using SqlSugar;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ZXing.PDF417.Internal;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using TSQL.Statements;
using TSQL;
using System.Text;
using TSQL.Tokens;
using SqlSugar.Extensions;
using SmartSQL.Framework.Util;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Stfu.Linq;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class UcRedisInfo : BaseUserControl
    {
        #region Filds
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcRedisInfo), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty RedisServerInfoProperty = DependencyProperty.Register(
            "RedisServerInfo", typeof(RedisServerInfo), typeof(UcRedisInfo), new PropertyMetadata(default(RedisServerInfo)));

        public static readonly DependencyProperty RedisServerInfoDicProperty = DependencyProperty.Register(
            "RedisServerInfoDic", typeof(Dictionary<string, string>), typeof(UcRedisInfo), new PropertyMetadata(default(Dictionary<string, string>)));

        /// <summary>
        /// 当前数据连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        /// <summary>
        /// Redis服务器信息
        /// </summary>
        public RedisServerInfo RedisServerInfo
        {
            get => (RedisServerInfo)GetValue(RedisServerInfoProperty);
            set => SetValue(RedisServerInfoProperty, value);
        }

        /// <summary>
        /// Redis服务器信息
        /// </summary>
        public Dictionary<string, string> RedisServerInfoDic
        {
            get => (Dictionary<string, string>)GetValue(RedisServerInfoDicProperty);
            set => SetValue(RedisServerInfoDicProperty, value);
        }
        #endregion

        public UcRedisInfo()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BaseUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedConnection = SelectedConnection;
            if (selectedConnection == null)
            {
                return;
            }
            try
            {
                var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnection.DbDefaultConnectString, "DB0");
                RedisServerInfo = dbInstance.GetInfo();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Oops.God($"连接失败 {selectedConnection.ConnectName}，原因：" + ex.ToMsg());
                }));
            }
            #endregion
        }
    }
}
