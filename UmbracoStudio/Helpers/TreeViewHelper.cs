using System.Windows;
using System.Windows.Controls;

namespace Umbraco.UmbracoStudio.Helpers
{
    public class UmbracoTreeViewItem : TreeViewItem
    {
        public string MetaData { get; set; }

        public int NodeId { get; set; }

        public string NodeType { get; set; }

        public string NodeTypeName { get; set; }

        public override string ToString()
        {
            return MetaData;
        }

    }

    public class TreeViewHelper
    {
        public static UmbracoTreeViewItem CreateTreeViewItemWithImage(string name, string imageName, bool showExpander)
        {
            var stackpanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
            stackpanel.Children.Add(ImageHelper.GetImageFromResource(imageName));
            stackpanel.Children.Add(new TextBlock { Text = " " + name });

            var treeViewItem = new UmbracoTreeViewItem { Header = stackpanel, MetaData = name };
            treeViewItem.MouseRightButtonDown += DatabaseTreeViewItemMouseRightButtonDown;
            treeViewItem.ContextMenu = new ContextMenu { Visibility = Visibility.Hidden };
            if (showExpander) treeViewItem.Items.Add("Loading...");
            return treeViewItem;
        }

        public static UmbracoTreeViewItem CreateTreeViewItemWithImageAndTooltip(string name, string imageName, bool showExpander, string toolTip)
        {
            var stackpanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
            stackpanel.Children.Add(ImageHelper.GetImageFromResource(imageName));
            stackpanel.Children.Add(new TextBlock { Text = " " + name });

            var treeViewItem = new UmbracoTreeViewItem { Header = stackpanel, MetaData = name };
            treeViewItem.MouseRightButtonDown += DatabaseTreeViewItemMouseRightButtonDown;
            treeViewItem.ContextMenu = new ContextMenu { Visibility = Visibility.Hidden };
            if (!string.IsNullOrWhiteSpace(toolTip))
            {
                treeViewItem.ToolTip = toolTip;
            }
            if (showExpander) treeViewItem.Items.Add("Loading...");
            return treeViewItem;
        }

        static void DatabaseTreeViewItemMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }
    }
}