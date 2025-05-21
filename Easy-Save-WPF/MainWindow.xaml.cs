using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using EasySaveCore.Models;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    //TODO : lock app window size + make bigger
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            EasySaveCore<BackupJob>.Init(new BackupJobManager());
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