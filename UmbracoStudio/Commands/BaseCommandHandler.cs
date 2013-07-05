using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Umbraco.UmbracoStudio.Dialogs;
using Umbraco.UmbracoStudio.ExternalApplication;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.Commands
{
    public abstract class BaseCommandHandler
    {
        private readonly ExplorerToolWindow _parentWindow;

        protected BaseCommandHandler(ExplorerToolWindow parent)
        {
            _parentWindow = parent;
        }

        public void DeleteNode(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if(menuInfo == null)
                return;

            try
            {
                UmbracoApplicationContext.Current.Delete(menuInfo.NodeType, menuInfo.NodeId);

                menuInfo.ExplorerControl.RefreshItems(menuInfo.NodeType, menuInfo.NodeTypeName, menuInfo.Name);
            }
            catch (Exception ex)
            {
                var message = string.Format("An error occured while trying to delete '{0}': {1}", menuInfo.Name, ex.Message);
                MessageBox.Show(message);
            }
        }

        public void TrashNode(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null)
                return;

            try
            {
                UmbracoApplicationContext.Current.Trash(menuInfo.NodeType, menuInfo.NodeId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }
        }

        public void MoveNode(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null)
                return;

            try
            {
                //TODO Call the ServiceBridge through the UmbracoApplicationContext to move a Content or Media item to a specified parent
                //Show dialog to select new location
                //Validate move before committing - Is allowed child doc type?
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }
        }

        public void RenameNode(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null)
                return;

            try
            {
                var ro = new RenameDialog(menuInfo.Name);
                ro.ShowModal();
                if (ro.DialogResult.HasValue && ro.DialogResult.Value == true && !string.IsNullOrWhiteSpace(ro.NewName) && !menuInfo.Name.Equals(ro.NewName))
                {
                    UmbracoApplicationContext.Current.Rename(menuInfo.NodeType, ro.NewName, menuInfo.NodeId);
                    
                    if (_parentWindow != null && _parentWindow.Content != null)
                    {
                        var control = _parentWindow.Content as ExplorerControl;
                        control.RefreshItems(menuInfo.NodeType, menuInfo.NodeTypeName, ro.NewName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }
        }

        public void SerializeNode(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null)
                return;

            try
            {
                UmbracoApplicationContext.Current.SerializeNode(menuInfo.NodeType, menuInfo.NodeId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }
        }

        public void DeserializeNode(object sender, ExecutedRoutedEventArgs e)
        {
            var menuInfo = ValidateMenuInfo(sender);
            if (menuInfo == null)
                return;

            try
            {
                var imo = new ImportDialog();

                if (imo.ShowModal() == true)
                {
                    if (!string.IsNullOrWhiteSpace(imo.File) && System.IO.File.Exists(imo.File))
                    {
                        UmbracoApplicationContext.Current.DeserializeNode(menuInfo.NodeType, menuInfo.NodeId, imo.File);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occured: " + ex.Message);
            }
        }

        private static NodeMenuCommandParameters ValidateMenuInfo(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                return menuItem.CommandParameter as NodeMenuCommandParameters;
            }
            else
            {
                return null;
            }
        } 
    }
}