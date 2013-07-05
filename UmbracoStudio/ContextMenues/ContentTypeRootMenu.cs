using System.Windows.Controls;
using System.Windows.Input;
using Umbraco.UmbracoStudio.Commands;
using Umbraco.UmbracoStudio.Helpers;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.ContextMenues
{
    public class ContentTypeRootMenu : ContextMenu
    {
        public ContentTypeRootMenu(NodeMenuCommandParameters parameters, ExplorerToolWindow parent)
        {
            var cmd = new CommonMenuCommandHandler(parent);
            //Import xml
            CreateXmlImportMenuItem(cmd, parameters);
        }

        private void CreateXmlImportMenuItem(CommonMenuCommandHandler cmd, NodeMenuCommandParameters parameters)
        {
            var nodeCommandBinding = new CommandBinding(MenuCommands.MenuCommand, cmd.DeserializeNode);
            var nodeMenuItem = new MenuItem
                                            {
                                                Header = "Deserialize item",
                                                Icon = ImageHelper.GetImageFromResource("../resources/file.png"),
                                                Command = MenuCommands.MenuCommand,
                                                CommandParameter = parameters
                                            };
            nodeMenuItem.CommandBindings.Add(nodeCommandBinding);
            Items.Add(nodeMenuItem);
        }
    }
}