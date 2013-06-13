using System.Windows;
using Microsoft.VisualStudio.PlatformUI;

namespace Umbraco.UmbracoStudio.Dialogs
{
    /// <summary>
    /// Interaction logic for RenameDialog.xaml
    /// </summary>
    public partial class RenameDialog : DialogWindow
    {
        public string NewName { get; set; }

        public RenameDialog(string itemName)
        {
            InitializeComponent();
            this.Background = Helpers.VsTheming.GetWindowBackground();
            this.Title = "Rename '" + itemName + "'";
            this.ItemName.Text = itemName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveSettings();
            Close();
        }

        private void SaveSettings()
        {
            NewName = this.ItemName.Text;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ItemName.Focus();
        }
    }
}
