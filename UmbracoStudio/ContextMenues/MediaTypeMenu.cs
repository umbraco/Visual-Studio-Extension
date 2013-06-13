using System.Windows.Controls;
using Umbraco.UmbracoStudio.Commands;
using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.ContextMenues
{
    public class MediaTypeMenu : ContextMenu
    {
        public MediaTypeMenu(NodeMenuCommandParameters parameters, ExplorerToolWindow parent)
        {
            Items.Add(new Separator());
        }
    }
}