using System.Windows;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    public partial class Options_PopUp : Window
    {
        public Options_PopUp()
        {
            BackupJobViewModel.Get().LoadEncryptionKeyCommand.Execute(null);
            InitializeComponent();
            DataContext = BackupJobViewModel.Get();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
