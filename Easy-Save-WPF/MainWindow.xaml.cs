using System;
using System.Windows;
using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            EasySaveCore<BackupJob, BackupJobManager, BackupJobConfiguration>.Init(BackupJobViewModel.Get(), new BackupJobManager(), BackupJobConfiguration.Get());
            BackupJobViewModel.Get().SetJobBuilder(new ViewModelBackupJobBuilder());
            InitializeComponent();
        }

        public void QuitBTN_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            Options_PopUp optionsWindow = new Options_PopUp();
            optionsWindow.Owner = this;
            optionsWindow.ShowDialog();
        }

        private void ManageJobsBTN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ManageJobs_Page());
        }

        public void LanguageBTN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Options_Language_Page());

        }
        public void LogTypeBTN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Options_LogType_Page());

        }
        public void DailyLogBTN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Options_DailyLog_Page());

        }
        public void StatusLogBTN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Options_StatusLog_Page());

        }
        public void ReturnBTN_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Options_Page());

        }

    }
}