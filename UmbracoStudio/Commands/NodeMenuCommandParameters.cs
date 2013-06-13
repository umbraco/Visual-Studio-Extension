using Umbraco.UmbracoStudio.ToolWindows;

namespace Umbraco.UmbracoStudio.Commands
{
    public class NodeMenuCommandParameters
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int NodeId { get; set; }
        public string NodeType { get; set; }
        public string NodeTypeName { get; set; }
        public ExplorerControl ExplorerControl { get; set; }
    }
}