using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.VisualStudio.PlatformUI;
using Umbraco.UmbracoStudio.VisualStudio;

namespace Umbraco.UmbracoStudio.Dialogs
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : DialogWindow
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Background = Helpers.VsTheming.GetWindowBackground();

            Version.Text = "Version " + Assembly.GetExecutingAssembly().GetName().Version;

            var solutionManager = new SolutionManager();

            txtStatus.Text = "Valid Umbraco Solution running -> \n";
            txtStatus.Text += "Is solution open? " + ReturnYesNo(solutionManager.IsSolutionOpen) + "\n";
            txtStatus.Text += "Is an Umbraco website solution? " + ReturnYesNo(solutionManager.DefaultProject.IsUmbracoWebsite()) + "\n";
            txtStatus.Text += "Is database configured in config? " + ReturnYesNo(solutionManager.DefaultProject.IsDatabaseConfigured()) + "\n";
            txtStatus.Text += "Default Project Name: " + solutionManager.DefaultProjectName + "\n";
            txtStatus.Text += "Default Project (FullPath): " + solutionManager.DefaultProject.GetFullPath() + "\n";
            txtStatus.Text += "Default Project (OutputPath): " + solutionManager.DefaultProject.GetOutputPath() + "\n";
            
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private string ReturnYesNo(bool isAyes)
        {
            return isAyes ? "Yes" : "No";
        }
    }
}
