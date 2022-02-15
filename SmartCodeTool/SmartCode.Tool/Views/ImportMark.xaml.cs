using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Framework.SqliteModel;
using System.IO;
using System.Xml;
using SmartCode.DocUtils;
using SmartCode.DocUtils.Dtos;
using SmartCode.Framework;

namespace SmartCode.Tool.Views
{
    /// <summary>
    /// ImportNote.xaml 的交互逻辑
    /// </summary>
    public partial class ImportMark
    {
        #region DependencyProperty
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "Connection", typeof(ConnectConfigs), typeof(ImportMark), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(ImportMark), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        #endregion
        public ImportMark()
        {
            InitializeComponent();
        }

        /// <summary>
        /// XML更新表批注
        /// </summary>
        /// <param name="path"></param>
        private void UpdateCommentByXML(string path)
        {
            var dbMaintenance = SugarFactory.GetDbMaintenance(SelectedConnection.DbType, SelectedConnection.DbDefaultConnectString);
            var xmlContent = File.ReadAllText(path, Encoding.UTF8);
            if (xmlContent.Contains("ArrayOfTableDto"))
            {
                //通过 dbchm 导出的 XML文件 来更新 表列批注
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                if (doc.DocumentElement != null)
                {
                    var dbName = doc.DocumentElement.GetAttribute("databaseName");

                    if (!SelectedDataBase.DbName.Equals(dbName, StringComparison.OrdinalIgnoreCase))
                    {
                        //if (MessageBox.Show("检测到数据库名称不一致，确定要继续吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                        //{
                        //    return;
                        //}
                    }
                }

                var listDTO = typeof(List<TableDto>).DeserializeXml(xmlContent) as List<TableDto>;
                foreach (var tabInfo in listDTO)
                {
                    //更新表描述
                    if (dbMaintenance.IsAnyTable(tabInfo.TableName) && !string.IsNullOrWhiteSpace(tabInfo.Comment))
                    {
                        if (dbMaintenance.IsAnyTableRemark(tabInfo.TableName))
                        {
                            dbMaintenance.DeleteTableRemark(tabInfo.TableName);
                        }
                        dbMaintenance.AddTableRemark(tabInfo.TableName, tabInfo.Comment);
                    }
                    //更新表列的描述
                    foreach (var colInfo in tabInfo.Columns)
                    {
                        if (dbMaintenance.IsAnyColumn(tabInfo.TableName, colInfo.ColumnName) && !string.IsNullOrWhiteSpace(colInfo.Comment))
                        {
                            if (dbMaintenance.IsAnyColumnRemark(colInfo.ColumnName, tabInfo.TableName))
                            {
                                dbMaintenance.DeleteColumnRemark(colInfo.ColumnName, tabInfo.TableName);
                            }
                            dbMaintenance.AddColumnRemark(colInfo.ColumnName, tabInfo.TableName, colInfo.Comment);
                        }
                    }
                }
            }
            else
            {
                //通过 有 VS 生成的 实体类库 XML文档文件 来更新 表列批注
                XmlAnalyze analyze = new XmlAnalyze(path);
                var data = analyze.Data;
                foreach (var item in data)
                {
                    var tableName = item.Key.Key;
                    var tableComment = item.Key.Value;
                    //更新表描述
                    if (dbMaintenance.IsAnyTable(tableName) && !string.IsNullOrWhiteSpace(tableComment))
                    {
                        if (dbMaintenance.IsAnyTableRemark(tableName))
                        {
                            dbMaintenance.DeleteTableRemark(tableName);
                        }
                        dbMaintenance.AddTableRemark(tableName, tableComment);
                    }
                    //更新表的列描述
                    foreach (var colKV in item.Value)
                    {
                        var colName = colKV.Key;
                        var colComment = colKV.Value;
                        if (dbMaintenance.IsAnyColumn(tableName, colName) && !string.IsNullOrWhiteSpace(colComment))
                        {
                            if (dbMaintenance.IsAnyColumnRemark(colName, tableName))
                            {
                                dbMaintenance.DeleteColumnRemark(colName, tableName);
                            }
                            dbMaintenance.AddColumnRemark(colName, tableName, colComment);
                        }
                    }
                }
            }
        }
    }
}
