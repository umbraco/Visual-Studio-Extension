using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FabTab;
using UmbracoStudioApp.ExternalApplication;
using UmbracoStudioApp.Helpers;

namespace UmbracoStudioApp.ToolWindows
{
    /// <summary>
    /// Interaction logic for ExplorerControl.xaml
    /// </summary>
    public partial class ExplorerControl : UserControl
    {
        private bool _loaded;
        private FabTab.FabTabControl _fabTab;
        private Storyboard _myStoryboard;

        public ExplorerControl(FabTabControl fabTab)
        {
            _fabTab = fabTab;
            InitializeComponent();
        }

        private delegate void FillRootTreeListHandler(SortedDictionary<string, string> treeList);
        private delegate void GetNodeItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, string> tree);
        private delegate void FillNodeItemsHandler(KeyValuePair<string, string> tree, UmbracoTreeViewItem parentItem);

        private void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded == false)
            {
                if (UmbracoApplicationContext.Current == null || UmbracoApplicationContext.Current.IsReady == false)
                {
                    UmbracoApplicationContext.Current =
                        new UmbracoApplicationContext(@"C:\Temp\Playground\Umb610TestSiteVsPlugin\Umb610TestSiteVsPlugin");
                    UmbracoApplicationContext.Current.StartApplication();
                }
                BuildUmbracoTrees();
            }

            _loaded = true;
        }

        private void BuildUmbracoTrees()
        {
            var treeList = new SortedDictionary<string, string>();

            treeList.Add("content", "Content");
            treeList.Add("media", "Media");
            treeList.Add("contentTypes", "Content Types");
            treeList.Add("mediaTypes", "Media Types");

            var fillList = new FillRootTreeListHandler(FillTreeList);
            Dispatcher.BeginInvoke(fillList, treeList); //fill the tree on the UI thread
        }

        private void FillTreeList(SortedDictionary<string, string> treeList)
        {
            ItemDataSources.Items.Clear();
            foreach (var tree in treeList)
            {
                var databaseTreeViewItem = AddTreeToTreeView(tree);
                ItemDataSources.Items.Add(databaseTreeViewItem);
            }

            ItemDataSources.IsExpanded = true;
        }

        private TreeViewItem AddTreeToTreeView(KeyValuePair<string, string> tree)
        {
            var umbracoTreeViewItem = TreeViewHelper.CreateTreeViewItemWithImage(tree.Value, "../Resources/folder.png", true);
            umbracoTreeViewItem.ToolTip = tree.Value;
            umbracoTreeViewItem.Items.Clear();

            if (UmbracoApplicationContext.Current.IsReady)
            {
                var nodes = UmbracoApplicationContext.Current.GetRootByType(tree.Key);
                foreach (var node in nodes)
                {
                    var children = UmbracoApplicationContext.Current.GetChildrenByType(tree.Key, node.Key);
                    var item = TreeViewHelper.CreateTreeViewItemWithImage(node.Value["Name"], "../Resources/doc2.gif", children.Any());
                    item.NodeId = node.Key;
                    item.Expanded += (sender, args) => new GetNodeItemsHandler(GetNodeItems).BeginInvoke(sender, args, tree, null, null);
                    umbracoTreeViewItem.Items.Add(item);
                }
            }

            return umbracoTreeViewItem;
        }

        private void GetNodeItems(object sender, RoutedEventArgs args, KeyValuePair<string, string> tree)
        {
            var parentItem = sender as UmbracoTreeViewItem;
            if (parentItem != null && (parentItem.Items.Count > 0 && parentItem.Items[0].ToString() == "Loading..."))
            {
                Dispatcher.BeginInvoke(new FillNodeItemsHandler(FillNodeItems), tree, parentItem);
            }
            args.Handled = true;
        }

        private void FillNodeItems(KeyValuePair<string, string> tree, UmbracoTreeViewItem parentItem)
        {
            parentItem.Items.Clear();
            var nodes = UmbracoApplicationContext.Current.GetChildrenByType(tree.Key, parentItem.NodeId);
            foreach (var node in nodes)
            {
                var children = UmbracoApplicationContext.Current.GetChildrenByType(tree.Key, node.Key);
                var item = TreeViewHelper.CreateTreeViewItemWithImage(node.Value["Name"], "../Resources/doc2.gif", children.Any());
                item.NodeId = node.Key;
                item.Expanded += (sender, args) => new GetNodeItemsHandler(GetNodeItems).BeginInvoke(sender, args, tree, null, null);
                parentItem.Items.Add(item);
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }

        private void ToolbarRefresh_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
