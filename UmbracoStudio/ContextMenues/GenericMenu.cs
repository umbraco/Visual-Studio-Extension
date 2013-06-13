using System.Windows.Controls;
using System.Windows.Input;
using Umbraco.UmbracoStudio.Commands;
using Umbraco.UmbracoStudio.Helpers;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.ContextMenues
{
    public class GenericMenu : ContextMenu
    {
        public GenericMenu(NodeMenuCommandParameters parameters, ExplorerToolWindow parent)
        {
            var cmd = new CommonMenuCommandHandler(parent);
            //Delete
            CreateDeleteMenuItem(cmd, parameters);
            //Move
            CreateMoveMenuItem(cmd, parameters);
            //Trash
            CreateTrashMenuItem(cmd, parameters);
            //Rename
            CreateRenameMenuItem(cmd, parameters);
            Items.Add(new Separator());
        }

        private void CreateDeleteMenuItem(CommonMenuCommandHandler cmd, NodeMenuCommandParameters parameters)
        {
            var deleteNodeCommandBinding = new CommandBinding(MenuCommands.MenuCommand, cmd.DeleteNode);
            var deleteNodeMenuItem = new MenuItem
                                         {
                                             Header = "Delete item",
                                             Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                                             Command = MenuCommands.MenuCommand,
                                             CommandParameter = parameters
                                         };
            deleteNodeMenuItem.CommandBindings.Add(deleteNodeCommandBinding);
            Items.Add(deleteNodeMenuItem);
        }

        private void CreateTrashMenuItem(CommonMenuCommandHandler cmd, NodeMenuCommandParameters parameters)
        {
            var trashNodeCommandBinding = new CommandBinding(MenuCommands.MenuCommand, cmd.TrashNode);
            var trashNodeMenuItem = new MenuItem
                                        {
                                            Header = "Move item to Recycle Bin",
                                            Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                                            Command = MenuCommands.MenuCommand,
                                            CommandParameter = parameters
                                        };
            trashNodeMenuItem.CommandBindings.Add(trashNodeCommandBinding);
            Items.Add(trashNodeMenuItem);
        }

        private void CreateMoveMenuItem(CommonMenuCommandHandler cmd, NodeMenuCommandParameters parameters)
        {
            var moveNodeCommandBinding = new CommandBinding(MenuCommands.MenuCommand, cmd.MoveNode);
            var moveNodeMenuItem = new MenuItem
                                       {
                                           Header = "Move item",
                                           Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                                           Command = MenuCommands.MenuCommand,
                                           CommandParameter = parameters
                                       };
            moveNodeMenuItem.CommandBindings.Add(moveNodeCommandBinding);
            Items.Add(moveNodeMenuItem);
        }

        private void CreateRenameMenuItem(CommonMenuCommandHandler cmd, NodeMenuCommandParameters parameters)
        {
            var renameNodeCommandBinding = new CommandBinding(MenuCommands.MenuCommand, cmd.RenameNode);
            var renameNodeMenuItem = new MenuItem
                                         {
                                             Header = "Rename item",
                                             Icon = ImageHelper.GetImageFromResource("../resources/sqlEditor.png"),
                                             Command = MenuCommands.MenuCommand,
                                             CommandParameter = parameters
                                         };
            renameNodeMenuItem.CommandBindings.Add(renameNodeCommandBinding);
            Items.Add(renameNodeMenuItem);
        }
    }
}