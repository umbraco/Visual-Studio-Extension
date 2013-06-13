using System.Windows.Input;

namespace Umbraco.UmbracoStudio.Commands
{
    public sealed class MenuCommands
    {
        private MenuCommands(){ }
        static MenuCommands()
        {
            MenuCommand = new RoutedUICommand("Command from common context menu", "MenuCommand", typeof (MenuCommands));
        }

        public static RoutedUICommand MenuCommand { get; private set; }
    }
}