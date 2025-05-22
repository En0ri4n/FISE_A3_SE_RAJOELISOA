using System.Windows;
using EasySaveCore.Jobs.Backup.ViewModels;
using Microsoft.Win32;

namespace Easy_Save_WPF
{
    public partial class Options_PopUp : Window
    {
        public Options_PopUp()
        {

            InitializeComponent();
            DataContext = BackupJobViewModel.Get();
        }

        private void ChangeDailyLogPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Daily Log Destination Folder";
            dialog.ShowDialog();
        }

        private void ChangeStatusLogPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select Status Log Destination Folder";
            dialog.ShowDialog();
        }

        private void AddExtension_Click(object sender, RoutedEventArgs e)
        {
            string ext = NewExtensionBox.Text.Trim();
            if (!string.IsNullOrEmpty(ext))
            {
                EncryptionExtensionsList.Items.Add(ext);
                NewExtensionBox.Clear();
            }
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            string proc = NewProcessBox.Text.Trim();
            if (!string.IsNullOrEmpty(proc))
            {
                BlacklistedProcessesList.Items.Add(proc);
                NewProcessBox.Clear();
            }
        }

        private void SaveOptions_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Saved options !");
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
