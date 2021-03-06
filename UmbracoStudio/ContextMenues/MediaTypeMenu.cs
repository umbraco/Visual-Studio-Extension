﻿using System.Windows.Controls;
using System.Windows.Input;
using Umbraco.UmbracoStudio.Commands;
using Umbraco.UmbracoStudio.Helpers;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.ContextMenues
{
    public class MediaTypeMenu : ContextMenu
    {
        public MediaTypeMenu(NodeMenuCommandParameters parameters, ExplorerToolWindow parent)
        {
            var cmd = new CommonMenuCommandHandler(parent);
            //Export to Xml
            CreateXmlExportMenuItem(cmd, parameters);
        }

        private void CreateXmlExportMenuItem(CommonMenuCommandHandler cmd, NodeMenuCommandParameters parameters)
        {
            var serializeNodeCommandBinding = new CommandBinding(MenuCommands.MenuCommand, cmd.SerializeNode);
            var serializeNodeMenuItem = new MenuItem
                                            {
                                                Header = "Serialize item",
                                                Icon = ImageHelper.GetImageFromResource("../resources/file.png"),
                                                Command = MenuCommands.MenuCommand,
                                                CommandParameter = parameters
                                            };
            serializeNodeMenuItem.CommandBindings.Add(serializeNodeCommandBinding);
            Items.Add(serializeNodeMenuItem);
        }
    }
}