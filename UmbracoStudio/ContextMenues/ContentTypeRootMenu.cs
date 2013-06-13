using System.Windows.Controls;
using Umbraco.UmbracoStudio.Commands;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.ContextMenues
{
    public class ContentTypeRootMenu : ContextMenu
    {
        public ContentTypeRootMenu(NodeMenuCommandParameters parameters, ExplorerToolWindow parent)
        {
            var cmd = new CommonMenuCommandHandler(parent);
            //Import xml
            Items.Add(new Separator());
        }
    }
}