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
using System.Text.RegularExpressions;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class UcRedisFrom : BaseUserControl
    {
        #region Filds
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcRedisFrom), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcRedisFrom), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcRedisFrom), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedTypeProperty = DependencyProperty.Register(
            "SelectedType", typeof(string), typeof(UcRedisFrom), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedTtlProperty = DependencyProperty.Register(
            "SelectedTtl", typeof(long), typeof(UcRedisFrom), new PropertyMetadata(default(long)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public TreeNodeItem SelectedObject
        {
            get => (TreeNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        /// <summary>
        /// 当前Key类型
        /// </summary>
        public string SelectedType
        {
            get => (string)GetValue(SelectedTypeProperty);
            set => SetValue(SelectedTypeProperty, value);
        }
        /// <summary>
        /// 当前Key剩余过期时间（-1：永不过期，-2：已过期）
        /// </summary>
        public long SelectedTtl
        {
            get => (long)GetValue(SelectedTtlProperty);
            set => SetValue(SelectedTtlProperty, value);
        }
        #endregion

        public UcRedisFrom()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void BaseUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            var selectedObejct = SelectedObject;
            if (selectedConnection == null || selectedDatabase == null || selectedObejct == null)
            {
                return;
            }
            try
            {
                var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnection.DbDefaultConnectString, selectedDatabase.DbName);
                var redisDb = dbInstance.GetDB();
                var type = redisDb.Type(selectedObejct.Name);
                var ttl = redisDb.Ttl(selectedObejct.Name);
                SelectedTtl = ttl;
                if (type == FreeRedis.KeyType.@string)
                {
                    SelectedType = "String";
                    var keyValue = redisDb.Get(selectedObejct.Name);
                    TextEditor.Text = keyValue;
                }
                if (type == FreeRedis.KeyType.zset)
                {
                    SelectedType = "Zset";
                }
                if (type == FreeRedis.KeyType.hash)
                {
                    SelectedType = "Hash";
                }
                if (type == FreeRedis.KeyType.none)
                {
                    SelectedType = "None";
                }
                if (type == FreeRedis.KeyType.list)
                {
                    SelectedType = "List";
                }
                if (type == FreeRedis.KeyType.set)
                {
                    SelectedType = "Set";
                }
                if (type == FreeRedis.KeyType.stream)
                {
                    SelectedType = "Stream";
                }
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
