using System.ComponentModel;
using System.Linq;
using System.Windows;
using EasySaveShared.DataStructures;
using Newtonsoft.Json.Linq;

namespace EasySaveShared
{
    /// <summary>
    /// Logique d'interaction pour ManageJobs_Page.xaml
    /// </summary>

    //TODO select all rows : https://www.youtube.com/watch?app=desktop&v=bxTkTOZV0eQ
    public partial class ManageJobsWindow : Window
    {
        private readonly object _lockObject = new object();
        public ManageJobsWindow()
        {
            InitializeComponent();
            DataContext = RemoteClient.Get().ViewModel;
            
            RemoteClient.Get().NetworkClient.OnDisconnected += (client) =>
            {
                MessageBox.Show("Connection to the server has been lost. Please check your connection and try again.",
                                "Connection Lost",
                                MessageBoxButton.OK, 
                                MessageBoxImage.Warning);

                Dispatcher.Invoke(() =>
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                });
            };
            // Buttons.DataContext = RemoteClient.Get().ViewModel;
            // jobsDatagrid.ItemsSource = RemoteClient.Get().ViewModel.AvailableBackupJobs;
            // BackupJobViewModel.Get().JobManager.JobInterruptedHandler += (reason, job, processName) =>
            // {
            //     switch (reason)
            //     {
            //         case JobInterruptionReasons.NotEnoughDiskSpace:
            //             MessageBox.Show(L10N.Get().GetTranslation($"message_box.interrupt_space.text").Replace("{JOB}", job.Name), L10N.Get().GetTranslation($"message_box.interrupt_space.title"), MessageBoxButton.OK, MessageBoxImage.Error);
            //             break;
            //         case JobInterruptionReasons.ProcessRunning:
            //             MessageBox.Show(L10N.Get().GetTranslation($"message_box.interrupt_process.text").Replace("{JOB}", job.Name).Replace("{PROCESS}", processName), L10N.Get().GetTranslation($"message_box.interrupt_process.title"), MessageBoxButton.OK, MessageBoxImage.Error);
            //             break;
            //         default:
            //             MessageBox.Show(L10N.Get().GetTranslation($"message_box.interrupt_unknown.text").Replace("{JOB}", job.Name), L10N.Get().GetTranslation($"message_box.interrupt_unknown.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            //             break;
            //     }
            // };
        }
        
        private void OnCustomClosing(object sender, CancelEventArgs e)
        {
            // if (BackupJobViewModel.Get().CanJobBeRun)
            //     return;
            //
            // MessageBoxResult result = MessageBox.Show(L10N.Get().GetTranslation("message_box.close_confirm.text"), 
            //                              L10N.Get().GetTranslation("message_box.close_confirm.title"),
            //                              MessageBoxButton.YesNo, 
            //                              MessageBoxImage.Warning);
            // if (result != MessageBoxResult.Yes)
            // {
            //     e.Cancel = true;
            // }
        }
        
        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            OptionsPopup options = new OptionsPopup();
            options.Owner = GetWindow(App.Current.MainWindow);
            options.ShowDialog();
        }
        public void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItems.Count < jobsDatagrid.Items.Count)
            {
                jobsDatagrid.SelectAll();
                SelectAll_Checkbox.IsChecked = true;
            }
            else
            {
                jobsDatagrid.UnselectAll();
                SelectAll_Checkbox.IsChecked = false;
            }
        }
        
        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            RemoteClient.Get().ViewModel.BackupJobBuilder.Clear();
            
            JobFormWindow jobFormWindow = new JobFormWindow(JobFormWindowType.Create, true);
            // jobFormWindow.Owner = GetWindow(App.Current.MainWindow);
            jobFormWindow.ShowDialog();
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            string selectedJobName = ((SharedBackupJob)jobsDatagrid.SelectedItem)?.Name;
            
            if (selectedJobName == null)
                return;
            
            RemoteClient.Get().ViewModel.LoadJobInBuilderCommand.Execute(selectedJobName);
            
            JobFormWindow modifyJobFormWindow = new JobFormWindow(JobFormWindowType.Edit, false);
            // modifyJobFormWindow.Owner = GetWindow(App.Current.MainWindow);
            modifyJobFormWindow.ShowDialog();
        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a job to delete.",
                "No Job Selected", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            
                return;
            }
            
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the selected job?",
                "Confirm Deletion", MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            
            RemoteClient.Get().ViewModel.DeleteMultipleJobsCommand.Execute(GetSelectedJobs().Select(j => j.Name).ToList());
        }
        public void StopBTN_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a job to stop.",
                    "No Job Selected", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            
                return;
            }
            
            MessageBoxResult result = MessageBox.Show("Are you sure you want to stop the selected job(s)?",
                "Confirm Stop", MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes) return;
            
            RemoteClient.Get().ViewModel.StopMultipleJobsCommand.Execute(GetSelectedJobs().Select(j => j.Name).ToList());
        }
        public void PauseBTN_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a job to pause.",
                    "No Job Selected", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            
                return;
            }
            
            RemoteClient.Get().ViewModel.PauseMultipleJobsCommand.Execute(GetSelectedJobs().Select(j => j.Name).ToList());
        }
        public void RunJob_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a job to run.",
                    "No Job Selected", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            
                return;
            }
            
            RemoteClient.Get().ViewModel.RunMultipleJobsCommand.Execute(GetSelectedJobs().Select(j => j.Name).ToList());
        }
        public void dailyLogBTN_Click(object sender, RoutedEventArgs e)
        {
            // var path = BackupJobViewModel.Get().DailyLogFilePath;
            //
            // if (!File.Exists(path))
            // {
            //     MessageBox.Show(L10N.Get().GetTranslation($"message_box.no_daily_log.text"), L10N.Get().GetTranslation($"message_box.no_daily_log.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            //     return;
            // }
            //
            // using Process myProcess = new Process();
            // myProcess.StartInfo.Verb = "open";
            // myProcess.StartInfo.FileName = path;
            // myProcess.StartInfo.UseShellExecute = true;
            // myProcess.Start();
        }
        public void statusLogBTN_Click(object sender, RoutedEventArgs e)
        {
            // var path = BackupJobViewModel.Get().StatusLogFilePath;
            //
            // if (!File.Exists(path)) {
            //     MessageBox.Show(L10N.Get().GetTranslation($"message_box.no_status_log.text"), L10N.Get().GetTranslation($"message_box.no_status_log.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
            //     return;
            // }
            //
            // using Process myProcess = new Process();
            // myProcess.StartInfo.FileName = path;
            // myProcess.StartInfo.Verb = "open";
            // myProcess.StartInfo.UseShellExecute = true;
            // myProcess.Start();
        }

        private void Reload(object sender, RoutedEventArgs e)
        {
            // This method is used to reload the job list from the server.
            // It sends a request to the server to fetch the latest backup job list.
            // NetworkClient networkClient = new NetworkClient();
            // networkClient.SendData(NetworkMessage.Create(MessageType.FetchBackupJobList, new JsonObject()));
            
            // Optionally, you can also refresh the DataGrid to reflect any changes.
            // jobsDatagrid.Items.Refresh();
            RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(MessageType.FetchJobs, new JObject()));
        }
        
        private SharedBackupJob[] GetSelectedJobs()
        {
            return jobsDatagrid.SelectedItems.Cast<SharedBackupJob>().ToArray();
        }
    }
}
