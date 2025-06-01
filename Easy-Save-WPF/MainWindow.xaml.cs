using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Translations;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;

namespace Easy_Save_WPF
{
    /// <summary>
    ///     Logique d'interaction pour ManageJobs_Page.xaml
    /// </summary>

    //TODO select all rows : https://www.youtube.com/watch?app=desktop&v=bxTkTOZV0eQ
    public partial class MainWindow : Window
    {
        private readonly BackupJobViewModel ViewModel;

        public MainWindow()
        {
            // Initialize the EasySaveCore with the necessary components
            // MANDATORY: This line is required to ensure that the core is initialized properly.
            BackupJobManager jobManager = new BackupJobManager();
            BackupJobConfiguration jobConfiguration = new BackupJobConfiguration();
            ViewModel = new BackupJobViewModel(jobConfiguration);
            CLEA.EasySaveCore.Core.EasySaveCore.Init(ViewModel, jobManager, jobConfiguration);
            ViewModel.SetJobBuilder(new ViewModelBackupJobBuilder(jobManager));

            InitializeComponent();
            DataContext = ViewModel;
            Buttons.DataContext = ViewModel;
            jobsDatagrid.ItemsSource = ViewModel.AvailableJobs;
            jobManager.JobInterruptedHandler += (reason, job, processName) =>
            {
                switch (reason)
                {
                    case JobInterruptionReasons.NotEnoughDiskSpace:
                        MessageBox.Show(
                            L10N.Get().GetTranslation("message_box.interrupt_space.text").Replace("{JOB}", job.Name),
                            L10N.Get().GetTranslation("message_box.interrupt_space.title"), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                    case JobInterruptionReasons.ProcessRunning:
                        MessageBox.Show(
                            L10N.Get().GetTranslation("message_box.interrupt_process.text").Replace("{JOB}", job.Name)
                                .Replace("{PROCESS}", processName),
                            L10N.Get().GetTranslation("message_box.interrupt_process.title"), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                    case JobInterruptionReasons.ManualStop:
                        MessageBox.Show(
                            L10N.Get().GetTranslation("message_box.interrupt_process.text").Replace("{JOB}", job.Name),
                            L10N.Get().GetTranslation("message_box.interrupt_process.title"), MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show(
                            L10N.Get().GetTranslation("message_box.interrupt_unknown.text").Replace("{JOB}", job.Name),
                            L10N.Get().GetTranslation("message_box.interrupt_unknown.title"), MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        break;
                }
            };
        }

        private void OnCustomClosing(object sender, CancelEventArgs e)
        {
            if (!ViewModel.CanJobBeRun)
                return;

            MessageBoxResult result = MessageBox.Show(L10N.Get().GetTranslation("message_box.close_confirm.text"),
                L10N.Get().GetTranslation("message_box.close_confirm.title"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) e.Cancel = true;
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            OptionsPopup options = new OptionsPopup(ViewModel);
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
            OnCellsSelected(sender, null);
        }

        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.JobBuilderBase.Clear();

            JobFormWindow jobFormWindow = new JobFormWindow(ViewModel, "create_job", true);
            jobFormWindow.Owner = GetWindow(this);
            jobFormWindow.ShowDialog();
        }

        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            string selectedJobName = ((IJob)jobsDatagrid.SelectedItem)?.Name;
            
            ViewModel.UpdateCanJobsRunCommand.Execute(GetSelectedJobs().Select(j => j.Name).ToList());
            
            if (!ViewModel.CanJobBeRun)
                return;

            if (selectedJobName == null)
                return;

            ViewModel.LoadJobInBuilderCommand.Execute(selectedJobName);

            JobFormWindow modifyJobFormWindow = new JobFormWindow(ViewModel, "edit_job", false);
            modifyJobFormWindow.Owner = GetWindow(this);
            modifyJobFormWindow.ShowDialog();
        }

        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            if (jobsDatagrid.SelectedItem == null)
            {
                MessageBox.Show(L10N.Get().GetTranslation("message_box.delete_no_selected.text"),
                    L10N.Get().GetTranslation("message_box.delete_no_selected.title"), MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result = MessageBox.Show(L10N.Get().GetTranslation("message_box.delete_confirm.text"),
                L10N.Get().GetTranslation("message_box.delete_confirm.title"), MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            foreach (IJob selectedJob in GetSelectedJobs()) ViewModel.DeleteJobCommand.Execute(selectedJob.Name);
        }

        public void OnPauseJobsButtonClicked(object sender, RoutedEventArgs e)
        {
            ExecuteWithReselection(jobNames =>
            {
                ViewModel.PauseJobsCommand.Execute(null);
            });
        }

        public void OnStopJobsButtonClicked(object sender, RoutedEventArgs e)
        {
            ExecuteWithReselection(jobNames =>
            {
                ViewModel.StopJobsCommand.Execute(jobNames);
            });
        }

        public void RunJob_Click(object sender, RoutedEventArgs e)
        {
            IJob[] selectedJobs = GetSelectedJobs();

            foreach (IJob jobIteration in selectedJobs)
            {
                if (selectedJobs.Any(selectedJob => selectedJob.Target == jobIteration.Target && selectedJob != jobIteration))
                {
                    MessageBox.Show("Same target is detected, this will result in errors. Please select jobs with different targets.",
                        L10N.Get().GetTranslation("message_box.same_target.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            ExecuteWithReselection(jobNames =>
            {
                ViewModel.UpdateProperties(null);
                ViewModel.RunMultipleJobsCommand.Execute(jobNames);
                ViewModel.UpdateProperties(null);
            });
        }


        private void ExecuteWithReselection(Action<List<string>> command, bool useJobManager = false)
        {
            IJob[] selectedJobs = GetSelectedJobs();
            List<string> selectedNames = selectedJobs.Select(j => j.Name).ToList();

            jobsDatagrid.SelectedItems.Clear();

            command(selectedNames);

            List<IJob> allJobs = jobsDatagrid.ItemsSource.Cast<IJob>().ToList();
            foreach (IJob job in allJobs.Where(j => selectedNames.Contains(j.Name)))
            {
                jobsDatagrid.SelectedItems.Add(job);
            }

            ViewModel.UpdateCanJobsRunCommand.Execute(selectedNames);
        }


        private IJob[] GetSelectedJobs()
        {
            return jobsDatagrid.SelectedItems.Cast<IJob>().ToArray();
        }

        public void dailyLogBTN_Click(object sender, RoutedEventArgs e)
        {
            string path = ViewModel.DailyLogFilePath;

            if (!File.Exists(path))
            {
                MessageBox.Show(L10N.Get().GetTranslation("message_box.no_daily_log.text"),
                    L10N.Get().GetTranslation("message_box.no_daily_log.title"), MessageBoxButton.OK,
                    MessageBoxImage.Warning);
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
            string path = ViewModel.StatusLogFilePath;

            if (!File.Exists(path))
            {
                MessageBox.Show(L10N.Get().GetTranslation("message_box.no_status_log.text"),
                    L10N.Get().GetTranslation("message_box.no_status_log.title"), MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
        
        private void OnCellsSelected(object sender, SelectedCellsChangedEventArgs e)
        {
            ViewModel.UpdateCanJobsRunCommand.Execute(GetSelectedJobs().Select(j => j.Name).ToList());
        }
    }
}