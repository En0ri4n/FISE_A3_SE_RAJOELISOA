using System.Diagnostics;
using System.Windows;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    public partial class OptionsPopup : Window
    {
        public OptionsPopup()
        {
            BackupJobViewModel.Get().LoadEncryptionKeyCommand.Execute(null);
            InitializeComponent();
            DataContext = BackupJobViewModel.Get();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        public void configFileBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = "config.json";

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
    }
}
