using System.Diagnostics;
using System.Windows;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    public partial class OptionsPopup : Window
    {
        public OptionsPopup(BackupJobViewModel viewModel)
        {
            viewModel.LoadEncryptionKeyCommand.Execute(null);
            viewModel.LoadSimultaneousFileSizeThresholdCommand.Execute(null);
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void configFileBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = "config.json";

            using var myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
    }
}