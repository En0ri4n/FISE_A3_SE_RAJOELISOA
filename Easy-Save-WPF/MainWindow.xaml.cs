using System;
using System.Diagnostics;
using System.Linq;
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
            DataContext = BackupJobViewModel.Get().JobBuilder;
            jobsDatagrid.ItemsSource = BackupJobViewModel.Get().AvailableJobs;
            BackupJobViewModel.Get().JobManager.JobInterruptedHandler += (job, processName) =>
            {
                MessageBox.Show($"Job {job.Name} and every following jobs have been interrupted by process {processName}.", "Job(s) Interruption(s)", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            
            OptionsPopup options = new OptionsPopup();
            options.Owner = Window.GetWindow(App.Current.MainWindow);
            options.ShowDialog();
        }
        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            BackupJobViewModel.Get().JobBuilder.Clear();

            JobFormWindow jobFormWindow = new JobFormWindow("create_job");
            jobFormWindow.Owner = Window.GetWindow(App.Current.MainWindow);
            jobFormWindow.ShowDialog();
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            string selectedJobName = ((BackupJob)this.jobsDatagrid.SelectedItem).Name;
            BackupJobViewModel.Get().LoadJobInBuilderCommand.Execute(selectedJobName);

            JobFormWindow modifyJobFormWindow = new JobFormWindow("edit_job");
            modifyJobFormWindow.Owner = Window.GetWindow(App.Current.MainWindow);
            modifyJobFormWindow.ShowDialog();
        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteJobWindow delete = new DeleteJobWindow();

            var selectedJob = ((BackupJob)this.jobsDatagrid.SelectedItem).Name;
            BackupJobViewModel.Get().LoadJobInBuilderCommand.Execute(selectedJob);

            while (delete.ShowDialog() == true)
            {
                delete.Owner = Window.GetWindow(App.Current.MainWindow);

                delete.deleteJobInput.DataContext = BackupJobViewModel.Get().JobBuilder;
            }
            this.jobsDatagrid.ItemsSource = null;
            this.jobsDatagrid.ItemsSource = BackupJobViewModel.Get().AvailableJobs;

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

            using Process myProcess = new Process();
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
        public void statusLogBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = BackupJobViewModel.Get().StatusLogFilePath;

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
    }
}
