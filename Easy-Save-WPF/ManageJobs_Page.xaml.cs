using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour ManageJobs_Page.xaml
    /// </summary>

    //TODO select all rows : https://www.youtube.com/watch?app=desktop&v=bxTkTOZV0eQ
    public partial class ManageJobs_Page : Page
    {
        
        public ManageJobs_Page()
        {
            InitializeComponent();
            this.DataContext = BackupJobViewModel.Get().JobBuilder;
            this.jobsDatagrid.ItemsSource = BackupJobViewModel.Get().AvailableJobs;
            BackupJobViewModel.Get().JobManager.JobInterruptedHandler += (job, processName) =>
            {
                MessageBox.Show($"Job {job.Name} and every following jobs have been interrupted by process {processName}.", "Job(s) Interruption(s)", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
        public void QuitBTN_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            
            Options_PopUp options = new Options_PopUp();
            options.Owner = Window.GetWindow(App.Current.MainWindow);
            options.ShowDialog();
        }
        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            BackupJobViewModel.Get().JobBuilder.Clear();

            CreateJob_Window createJobWindow = new CreateJob_Window("create_job");
            createJobWindow.Owner = Window.GetWindow(App.Current.MainWindow);
            createJobWindow.ShowDialog();
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            string selectedJobName = ((BackupJob)this.jobsDatagrid.SelectedItem).Name;
            BackupJobViewModel.Get().LoadJobInBuilderCommand.Execute(selectedJobName);

            CreateJob_Window modifyJobWindow = new CreateJob_Window("edit_job");
            modifyJobWindow.Owner = Window.GetWindow(App.Current.MainWindow);
            modifyJobWindow.ShowDialog();
        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteJob_Window delete = new DeleteJob_Window();

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
            //TODO
        }
        public void PauseBTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO
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
        private void DataGridSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            // this.runBTN.IsEnabled = true;
            // this.runBTN.Opacity = 1;
            // this.pauseBTN.IsEnabled = true;
            // this.pauseBTN.Opacity = 1;
            // this.stopBTN.IsEnabled = true;
            // this.stopBTN.Opacity = 1;
            // this.deleteBTN.IsEnabled = true;
            // this.deleteBTN.Opacity = 1;
        }
    }
}
