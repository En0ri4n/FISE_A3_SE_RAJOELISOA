using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour ManageJobs_Page.xaml
    /// </summary>

    //TODO select all rows : https://www.youtube.com/watch?app=desktop&v=bxTkTOZV0eQ
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            // Initialize the EasySaveCore with the necessary components
            // MANDATORY: This line is required to ensure that the core is initialized properly.
            EasySaveCore<BackupJob, BackupJobManager, BackupJobConfiguration>.Init(BackupJobViewModel.Get(), new BackupJobManager(), BackupJobConfiguration.Get());
            BackupJobViewModel.Get().SetJobBuilder(new ViewModelBackupJobBuilder());
            
            InitializeComponent();
            DataContext = BackupJobViewModel.Get();
            Buttons.DataContext = BackupJobViewModel.Get();
            jobsDatagrid.ItemsSource = BackupJobViewModel.Get().AvailableJobs;
            BackupJobViewModel.Get().JobManager.JobInterruptedHandler += (reason, job, processName) =>
            {
                switch (reason)
                {
                    case JobInterruptionReasons.NotEnoughDiskSpace:
                        MessageBox.Show(L10N.Get().GetTranslation($"message_box.interrupt_space.text").Replace("{JOB}", job.Name), L10N.Get().GetTranslation($"message_box.interrupt_space.title"), MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case JobInterruptionReasons.ProcessRunning:
                        MessageBox.Show(L10N.Get().GetTranslation($"message_box.interrupt_process.text").Replace("{JOB}", job.Name).Replace("{PROCESS}", processName), L10N.Get().GetTranslation($"message_box.interrupt_process.title"), MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show(L10N.Get().GetTranslation($"message_box.interrupt_unknown.text").Replace("{JOB}", job.Name), L10N.Get().GetTranslation($"message_box.interrupt_unknown.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
            };
        }
        
        private void OnCustomClosing(object sender, CancelEventArgs e)
        {
            if (BackupJobViewModel.Get().CanJobBeRun)
                return;
            
            MessageBoxResult result = MessageBox.Show(L10N.Get().GetTranslation("message_box.close_confirm.text"), 
                                         L10N.Get().GetTranslation("message_box.close_confirm.title"),
                                         MessageBoxButton.YesNo, 
                                         MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
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
            BackupJobViewModel.Get().JobBuilder.Clear();

            JobFormWindow jobFormWindow = new JobFormWindow("create_job", true);
            jobFormWindow.Owner = GetWindow(App.Current.MainWindow);
            jobFormWindow.ShowDialog();
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            if (!BackupJobViewModel.Get().CanJobBeRun)
                return;
            
            string selectedJobName = ((BackupJob)this.jobsDatagrid.SelectedItem)?.Name;

            if (selectedJobName == null)
                return;

            BackupJobViewModel.Get().LoadJobInBuilderCommand.Execute(selectedJobName);

            JobFormWindow modifyJobFormWindow = new JobFormWindow("edit_job", false);
            modifyJobFormWindow.Owner = GetWindow(App.Current.MainWindow);
            modifyJobFormWindow.ShowDialog();
        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItem == null)
            {
                MessageBox.Show(L10N.Get().GetTranslation($"message_box.delete_no_selected.text"), L10N.Get().GetTranslation($"message_box.delete_no_selected.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                
                return;
            }
            
            MessageBoxResult result = MessageBox.Show(L10N.Get().GetTranslation($"message_box.delete_confirm.text"), L10N.Get().GetTranslation($"message_box.delete_confirm.title"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            BackupJob[] selectedJobs = jobsDatagrid.SelectedItems.Cast<BackupJob>().ToArray();

            foreach (var selectedJob in selectedJobs) {
                BackupJobViewModel.Get().DeleteJobCommand.Execute(selectedJob.Name); 
            }
        }
        public void StopBTN_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement the logic to stop the selected job(s)
        }
        public void PauseBTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Implement the logic to pause the selected job(s)
        }
        public void RunJob_Click(object sender, RoutedEventArgs e)
        {
            BackupJob[] selectedJobs = jobsDatagrid.SelectedItems.Cast<BackupJob>().ToArray();
            BackupJobViewModel.Get().RunMultipleJobsCommand.Execute(selectedJobs.Select(bj => bj.Name).ToList());
        }
        public void dailyLogBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = BackupJobViewModel.Get().DailyLogFilePath;

            if (!File.Exists(path))
            {
                MessageBox.Show(L10N.Get().GetTranslation($"message_box.no_daily_log.text"), L10N.Get().GetTranslation($"message_box.no_daily_log.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using Process myProcess = new Process();
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
        public void statusLogBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = BackupJobViewModel.Get().StatusLogFilePath;

            if (!File.Exists(path)) {
                MessageBox.Show(L10N.Get().GetTranslation($"message_box.no_status_log.text"), L10N.Get().GetTranslation($"message_box.no_status_log.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }

        //public void DeactivateButtons()
        //{
        //    CreateJobBTN.IsEnabled = false;
        //    ModifyJobBTN.IsEnabled = false;
        //    DeleteJobBTN.IsEnabled = false;
        //    RunJobBTN.IsEnabled = false;
        //    StopJobBTN.IsEnabled = false;
        //    PauseJobBTN.IsEnabled = false;
        //}

        //public void ReactivateButtons()
        //{
        //    CreateJobBTN.IsEnabled = false;
        //    ModifyJobBTN.IsEnabled = false;
        //    DeleteJobBTN.IsEnabled = false;
        //    RunJobBTN.IsEnabled = false;
        //    StopJobBTN.IsEnabled = false;
        //    PauseJobBTN.IsEnabled = false;
        //}
    }
}
