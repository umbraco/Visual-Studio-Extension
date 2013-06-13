using System.Windows;
using System.Windows.Controls;

namespace UmbracoStudioApp.Helpers
{
    public class UmbracoTreeViewItem : TreeViewItem
    {
        public string MetaData { get; set; }

        public int NodeId { get; set; }

        public override string ToString()
        {
            return MetaData;
        }

    }

    public class TreeViewHelper
    {
        public static UmbracoTreeViewItem CreateTreeViewItemWithImage(string name, string imageName, bool showExpander)
        {
            var stackpanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new System.Windows.Thickness(2) };
            stackpanel.Children.Add(ImageHelper.GetImageFromResource(imageName));
            stackpanel.Children.Add(new TextBlock { Text = " " + name });

            var databaseTreeViewItem = new UmbracoTreeViewItem { Header = stackpanel, MetaData = name };
            databaseTreeViewItem.MouseRightButtonDown += DatabaseTreeViewItemMouseRightButtonDown;
            databaseTreeViewItem.ContextMenu = new ContextMenu { Visibility = Visibility.Hidden };
            if (showExpander) databaseTreeViewItem.Items.Add("Loading...");
            return databaseTreeViewItem;
        }

        public static UmbracoTreeViewItem CreateTreeViewItemWithImageAndTooltip(string name, string imageName, bool showExpander, string toolTip)
        {
            var stackpanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new System.Windows.Thickness(2) };
            stackpanel.Children.Add(ImageHelper.GetImageFromResource(imageName));
            stackpanel.Children.Add(new TextBlock { Text = " " + name });

            var databaseTreeViewItem = new UmbracoTreeViewItem { Header = stackpanel, MetaData = name };
            databaseTreeViewItem.MouseRightButtonDown += DatabaseTreeViewItemMouseRightButtonDown;
            databaseTreeViewItem.ContextMenu = new ContextMenu { Visibility = Visibility.Hidden };
            if (!string.IsNullOrWhiteSpace(toolTip))
            {
                databaseTreeViewItem.ToolTip = toolTip;
            }
            if (showExpander) databaseTreeViewItem.Items.Add("Loading...");
            return databaseTreeViewItem;
        }

        static void DatabaseTreeViewItemMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }
    }
}