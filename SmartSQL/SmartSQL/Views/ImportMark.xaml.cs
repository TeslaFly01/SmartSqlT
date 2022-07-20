using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;
using System.Windows.Forms;
using System.Xml.Serialization;
using HandyControl.Controls;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace SmartSQL.Views
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

        private void BtnFindFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = @"支持文件类型(*.pdm;*.xml)|*.pdm;*.xml|PowerDesigner文件(*.pdm)|*.pdm|XML文件|*.xml"
            };
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TxtPath.Text = openFileDialog.FileName;
            }
        }

        private void BtnImport_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateCommentXml(TxtPath.Text);
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// XML更新表批注
        /// </summary>
        /// <param name="path"></param>
        private void UpdateCommentXml(string path)
        {
            #region MyRegion
            if (string.IsNullOrWhiteSpace(path))
            {
                Growl.WarningGlobal("导入文件为空.");
                return;
            }
            LoadingG.Visibility = Visibility.Visible;
            var dbMaintenance = SugarFactory.GetDbMaintenance(SelectedConnection.DbType, SelectedConnection.DbDefaultConnectString);
            var xmlContent = File.ReadAllText(path, Encoding.UTF8);
            Task.Run(() =>
            {
                if (xmlContent.Contains("DBDto"))
                {
                    #region MyRegion
                    //通过 smartsql 导出的 XML文件 来更新 表列批注
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlContent);
                    if (doc.DocumentElement != null)
                    {
                        var dbName = doc.DocumentElement.GetAttribute("databaseName");
                        //if (!SelectedDataBase.DbName.Equals(dbName, StringComparison.OrdinalIgnoreCase))
                        //{
                        //    //if (MessageBox.Show("检测到数据库名称不一致，确定要继续吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                        //    //{
                        //    //    return;
                        //    //}
                        //}
                    }
                    var dbDTO = new DBDto().DeserializeXml(xmlContent);
                    foreach (var tabInfo in dbDTO.Tables)
                    {
                        var tableName = tabInfo.TableName;
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
                        tabInfo.Columns.ForEach(colInfo =>
                        {
                            var colName = colInfo.ColumnName;
                            var comment = colInfo.Comment;
                            if (dbMaintenance.IsAnyColumn(tableName, colName) && !string.IsNullOrWhiteSpace(comment))
                            {
                                if (dbMaintenance.IsAnyColumnRemark(colName, tableName))
                                {
                                    dbMaintenance.DeleteColumnRemark(colName, tableName);
                                }
                                dbMaintenance.AddColumnRemark(colName, tableName, comment);
                            }
                        });
                    }
                    //更新视图描述
                    dbDTO.Views.ForEach(view =>
                    {
                        var viewName = view.ObjectName;
                        var comment = view.Comment;
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            if (dbMaintenance.IsAnyViewRemark(viewName))
                            {
                                dbMaintenance.DeleteViewRemark(viewName);
                            }
                            dbMaintenance.AddViewRemark(viewName, comment);
                        }
                    });
                    //更新存储过程描述
                    dbDTO.Procs.ForEach(proc =>
                    {
                        var procName = proc.ObjectName;
                        var comment = proc.Comment;
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            if (dbMaintenance.IsAnyProcRemark(procName))
                            {
                                dbMaintenance.DeleteProcRemark(procName);
                            }
                            dbMaintenance.AddProcRemark(procName, comment);
                        }
                    });
                    #endregion
                }
                else
                {
                    #region MyRegion
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
                        item.Value.ForEach(colKV =>
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
                        });
                    }
                    #endregion
                }
                Dispatcher.Invoke(() =>
                {
                    LoadingG.Visibility = Visibility.Collapsed;
                    Growl.SuccessGlobal("导入成功.");
                });
            });

            #endregion
        }
    }
}
