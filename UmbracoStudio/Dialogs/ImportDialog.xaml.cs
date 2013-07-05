using System.Windows;
using System.Text;
using System.Collections.Generic;
using System;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.VisualStudio.PlatformUI;

namespace Umbraco.UmbracoStudio.Dialogs
{
    /// <summary>
    /// Interaction logic for ImportDialog.xaml
    /// </summary>
    public partial class ImportDialog : DialogWindow
    {
        public ImportDialog()
        {
            InitializeComponent();

            this.Background = Helpers.VsTheming.GetWindowBackground();
            this.ImportButton.IsEnabled = false;
        }

        public string File
        {
            get
            {
                return this.FileName.Text;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.FileName.Focus();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML (eXtensible Markup Language) (*.xml)|*.xml|All Files(*.*)|*.*";
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;
            ofd.ValidateNames = true;
            ofd.Title = "Select Import File";
            if (ofd.ShowDialog() == true)
            {
                this.FileName.Text = ofd.FileName;
            }
        }

        private void FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.FileName.Text))
            {
                this.ImportButton.IsEnabled = false;
            }
            else
            {
                this.ImportButton.IsEnabled = true;
            }
        }
    }
}
