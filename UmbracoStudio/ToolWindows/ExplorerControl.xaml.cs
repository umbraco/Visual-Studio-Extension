using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Umbraco.UmbracoStudio.Commands;
using Umbraco.UmbracoStudio.ContextMenues;
using Umbraco.UmbracoStudio.Dialogs;
using Umbraco.UmbracoStudio.ExternalApplication;
using Umbraco.UmbracoStudio.Helpers;
using Umbraco.UmbracoStudio.Models;
using Umbraco.UmbracoStudio.VisualStudio;

namespace Umbraco.UmbracoStudio.ToolWindows
{
    /// <summary>
    /// Interaction logic for ExplorerControl.xaml
    /// </summary>
    public partial class ExplorerControl : UserControl
    {
        private bool _loaded;
        private static ExplorerToolWindow _parentWindow;
        private Storyboard _myStoryboard;
        private SolutionManager _solutionManager;
        private SelectionContainer _mySelContainer;
        private ArrayList _mySelItems;
        private IVsWindowFrame _frame = null;

        public static UmbracoStudioPackage Package { get; private set; }

        public ExplorerControl(ExplorerToolWindow parentWindow)
        {
            _parentWindow = parentWindow;
            InitializeComponent();
        }

        public void RefreshItems(string key, string value, string newNodeName)
        {
            var item = ((UmbracoTreeViewItem)DataTreeView.SelectedItem) as UmbracoTreeViewItem;
            item.MetaData = newNodeName;

            var parent = item.Parent;
            if (parent != null)
            {
                var isParentExpanded = ((UmbracoTreeViewItem)parent).IsExpanded;
                GetNodeItems(parent, null, new KeyValuePair<string, string>(key, value));
            }
            else
            {
                var isItemExpanded = item.IsExpanded;
                GetNodeItems(item, null, new KeyValuePair<string, string>(key, value));
            }
        }

        private delegate void FillRootTreeListHandler(SortedDictionary<string, string> treeList);
        private delegate void GetNodeItemsHandler(object sender, RoutedEventArgs args, KeyValuePair<string, string> tree);
        private delegate void FillNodeItemsHandler(KeyValuePair<string, string> tree, UmbracoTreeViewItem parentItem);

        private void ToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded == false)
            {
                Package = (UmbracoStudioPackage)_parentWindow.Package;
                var overflowGrid = ExplorerToolbar.Template.FindName("OverflowGrid", ExplorerToolbar) as FrameworkElement;
                if (overflowGrid != null)
                {
                    overflowGrid.Visibility = Visibility.Collapsed;
                }
                ExplorerToolbar.Foreground = Helpers.VsTheming.GetWindowText();
                ExplorerToolbar.Background = Helpers.VsTheming.GetCommandBackground();
                ToolTray.Background = ExplorerToolbar.Background;
                sep1.Background = Helpers.VsTheming.GetToolbarSeparatorBackground();
                DataTreeView.Background = grid1.Background = Helpers.VsTheming.GetToolWindowBackground();
                Updated.Visibility = System.Windows.Visibility.Hidden;

                // Animate updated button
                DoubleAnimation myDoubleAnimation = new DoubleAnimation();
                myDoubleAnimation.From = 0.1;
                myDoubleAnimation.To = 1.0;
                myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(5));
                myDoubleAnimation.AutoReverse = true;
                myDoubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

                _myStoryboard = new Storyboard();
                _myStoryboard.Children.Add(myDoubleAnimation);
                Storyboard.SetTargetName(myDoubleAnimation, UpdatedText.Name);
                Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(TextBlock.OpacityProperty));

                var rootText = "No Umbraco solution was found...";
                _solutionManager = new SolutionManager();
                if (UmbracoApplicationContext.Current == null || UmbracoApplicationContext.Current.IsReady == false)
                {
                    var isUmbracoWebsite = _solutionManager.DefaultProject.IsUmbracoWebsite();
                    var isDatabaseConfigured = _solutionManager.DefaultProject.IsDatabaseConfigured();
                    if (isUmbracoWebsite && isDatabaseConfigured)
                    {
                        rootText = _solutionManager.DefaultProjectName;

                        UmbracoApplicationContext.Current =
                            new UmbracoApplicationContext(_solutionManager.DefaultProject.GetFullPath(),
                                                          _solutionManager.DefaultProject.GetConnectionString(),
                                                          _solutionManager.DefaultProject.GetProviderName());
                        UmbracoApplicationContext.Current.StartApplication();
                    }
                }

                TxtDataSources.Text = rootText;

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
            treeList.Add("dataTypes", "Data Types");

            var fillList = new FillRootTreeListHandler(FillTreeList);
            Dispatcher.BeginInvoke(fillList, treeList); //fill the tree on the UI thread
        }

        private void FillTreeList(SortedDictionary<string, string> treeList)
        {
            ItemDataSources.Items.Clear();
            foreach (var tree in treeList)
            {
                var treeViewItem = AddTreeToTreeView(tree);

                if (tree.Key.Equals("contentTypes"))
                    treeViewItem.ContextMenu = new ContentTypeRootMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeType = tree.Key, NodeTypeName = tree.Value}, _parentWindow);

                ItemDataSources.Items.Add(treeViewItem);
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
                    //Track selection to show properties per item
                    //TrackSelection(tree.Key, node.Key);

                    var children = UmbracoApplicationContext.Current.GetChildrenByType(tree.Key, node.Key);
                    var item = TreeViewHelper.CreateTreeViewItemWithImage(node.Value["Name"], "../Resources/doc2.gif", children.Any());
                    
                    if(tree.Key.Equals("content") || tree.Key.Equals("media"))
                        item.ContextMenu = new GenericMenu(new NodeMenuCommandParameters {ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"]}, _parentWindow);
                    if(tree.Key.Equals("contentTypes"))
                        item.ContextMenu = new ContentTypeMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);
                    if (tree.Key.Equals("mediaTypes"))
                        item.ContextMenu = new MediaTypeMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);
                    if (tree.Key.Equals("dataTypes"))
                        item.ContextMenu = new DataTypeMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);

                    item.NodeId = node.Key;
                    item.NodeType = tree.Key;
                    item.NodeTypeName = tree.Value;
                    item.ToolTip = string.Format("{0}, Id: {1}", node.Value["Name"], node.Key);
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

            if(args != null)
                args.Handled = true;
        }

        private void FillNodeItems(KeyValuePair<string, string> tree, UmbracoTreeViewItem parentItem)
        {
            parentItem.Items.Clear();
            var nodes = UmbracoApplicationContext.Current.GetChildrenByType(tree.Key, parentItem.NodeId);
            foreach (var node in nodes)
            {
                //Track selection to show properties per item
                //TrackSelection(tree.Key, node.Key);

                var children = UmbracoApplicationContext.Current.GetChildrenByType(tree.Key, node.Key);
                var item = TreeViewHelper.CreateTreeViewItemWithImage(node.Value["Name"], "../Resources/doc2.gif", children.Any());
                
                if (tree.Key.Equals("content") || tree.Key.Equals("media"))
                    item.ContextMenu = new GenericMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);
                if (tree.Key.Equals("contentTypes"))
                    item.ContextMenu = new ContentTypeMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);
                if (tree.Key.Equals("mediaTypes"))
                    item.ContextMenu = new MediaTypeMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);
                if (tree.Key.Equals("dataTypes"))
                    item.ContextMenu = new DataTypeMenu(new NodeMenuCommandParameters { ExplorerControl = this, NodeId = node.Key, NodeType = tree.Key, NodeTypeName = tree.Value, Name = node.Value["Name"] }, _parentWindow);

                item.NodeId = node.Key;
                item.NodeType = tree.Key;
                item.NodeTypeName = tree.Value;
                item.ToolTip = string.Format("{0}, Id: {1}", node.Value["Name"], node.Key);
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
            BuildUmbracoTrees();
        }

        private void ToolbarAbout_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowModal();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DataTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var treeViewItem = DataTreeView.SelectedItem as UmbracoTreeViewItem;
            if (treeViewItem != null)
            {
                var nodeId = treeViewItem.NodeId;
                var nodeType = treeViewItem.NodeType;
                TrackSelection(nodeType, nodeId);
                treeViewItem.Focus();
            }
        }

        private void TrackSelection(string nodeType, int id)
        {
            ShowPropertiesFrame();

            if (_mySelContainer == null)
            {
                _mySelContainer = new SelectionContainer();
            }

            _mySelItems = new ArrayList();

            if (string.IsNullOrEmpty(nodeType) == false && id != default(int))
            {
                var model = UmbracoApplicationContext.Current.GetPropertiesModel(nodeType, id);
                _mySelItems.Add(model);
            }

            _mySelContainer.SelectedObjects = _mySelItems;

            //Must use the GetService of the Window to get the ITrackSelection reference
            var track = _parentWindow.GetServiceHelper(typeof(STrackSelection)) as ITrackSelection;
            if (track != null)
            {
                track.OnSelectChange(_mySelContainer);
            }
        }

        private void ShowPropertiesFrame()
        {
            var package = _parentWindow.Package as UmbracoStudioPackage;
            if (package == null) return;
            if (_frame == null)
            {
                var shell = package.GetServiceHelper(typeof (SVsUIShell)) as IVsUIShell;
                if (shell != null)
                {
                    var guidPropertyBrowser = new
                        Guid(ToolWindowGuids.PropertyBrowser);
                    shell.FindToolWindow((uint) __VSFINDTOOLWIN.FTW_fForceCreate,
                                         ref guidPropertyBrowser, out _frame);
                    if (_frame != null)
                    {
                        _frame.Show();
                    }
                }
            }
        }
    }
}
