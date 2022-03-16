using System;
using System.Collections.Generic;
//using System.Windows.Forms;

namespace CodeBuilder.WinForm.UI
{
    //using Properties;
    //using PhysicalDataModel;
    //using DataSource.Exporter;
    //using Configuration;

    public class ExportModelHelper
    {
        //public static TreeNode ExportPDM(string connectionString, TreeView treeView)
        //{
        //    TreeNode rootNode = new TreeNode(connectionString, 1, 1);
        //    treeView.Nodes.Add(rootNode);

        //    Export(new PowerDesigner12Exporter(), connectionString, rootNode);

        //    return rootNode;
        //}

        //public static TreeNode Export(string dataSourceName, TreeView treeView)
        //{
        //    TreeNode rootNode = new TreeNode(dataSourceName, 1, 1);
        //    treeView.Nodes.Add(rootNode);

        //    string connectionString = ConfigManager.DataSourceSection.DataSources[dataSourceName].ConnectionString;
        //    string exporterName = ConfigManager.DataSourceSection.DataSources[dataSourceName].Exporter;
        //    string typeName = ConfigManager.SettingsSection.Exporters[exporterName].Type;
        //    IExporter exporter = (IExporter)Activator.CreateInstance(Type.GetType(typeName));
        //    Export(exporter, connectionString, rootNode);

        //    return rootNode;
        //}

        //private static void Export(IExporter exporter,string connectionString, TreeNode rootNode)
        //{
        //    Model model = exporter.Export(connectionString);

        //    rootNode.Tag = model.Database;
        //    AddBranchToTree(model, rootNode);

        //    ModelManager.Add(rootNode.Index.ToString(), model);
        //}

        //private static void AddBranchToTree(Model model, TreeNode parentNode)
        //{
        //    AddBranchToTree(model.Tables, parentNode);
        //    AddBranchToTree(model.Views, parentNode);
        //}

        //public static void CheckedTreeNode(TreeNode tn)
        //{
        //    foreach (TreeNode childNode in tn.Nodes)
        //    {
        //        if (childNode.Checked != tn.Checked)
        //            childNode.Checked = tn.Checked;
        //        CheckedTreeNode(childNode);
        //    }
        //}

        //private static void AddBranchToTree<T>(Dictionary<string, T> objects, TreeNode parentNode) where T : BaseTable
        //{
        //    if (objects == null ||
        //        objects.Count == 0) return;

        //    string text = typeof(T).Name.Equals("Table") ? Resources.Tables : Resources.Views;
        //    TreeNode childNode = new TreeNode(text, 1, 1);
        //    foreach (BaseTable baseTable in objects.Values)
        //    {
        //        TreeNode newNode = new TreeNode();
        //        newNode.Tag = baseTable.Id;
        //        newNode.Text = baseTable.DisplayName;
        //        newNode.ToolTipText = baseTable.DisplayName;
        //        newNode.ImageIndex = 2;
        //        newNode.SelectedImageIndex = 2;
        //        childNode.Nodes.Add(newNode);
        //    }
        //    parentNode.Nodes.Add(childNode);
        //}
    }
}
