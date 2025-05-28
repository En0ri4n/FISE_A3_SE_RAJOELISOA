using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Forms;
using CLEA.EasySaveCore.L10N;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;

namespace Easy_Save_Remote
{
    /// <summary>
    /// Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>

    public partial class JobFormWindow : Window
    {
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        public JobFormWindow(string menuName, bool isJobCreation)
        {
            InitializeComponent();
            DataContext = BackupJobViewModel.Get().GetJobBuilder();
            FooterCreateJob.DataContext = BackupJobViewModel.Get();

            BackupJobViewModel.Get().CloseAction = Close;

            Title = L10N.Get().GetTranslation($"{menuName}.title");
            MainTitle.Text = Title;
            MainSubtitle.Text = L10N.Get().GetTranslation($"{menuName}.subtitle");

            DoneButton.Content = L10N.Get().GetTranslation($"{menuName}.button.done");
            DoneButton.CommandParameter = isJobCreation.ToString();
        }

        public void CreateJobBTN_Click() {
            Client client = new Client();
            MainWindow window = new MainWindow();

            BackupJobViewModel.Get().BuildJobCommand.Execute(this.DataContext);

            client.SendData(BackupJobViewModel.Get().AvailableJobs.ToString());
        }

    }
}
