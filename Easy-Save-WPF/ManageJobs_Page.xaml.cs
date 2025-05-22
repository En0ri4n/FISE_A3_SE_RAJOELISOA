using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;
using Easy_Save_WPF;
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
            this.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            this.jobsDatagrid.ItemsSource = EasySaveViewModel<BackupJob>.Get().AvailableJobs;

                this.runBTN.IsEnabled = false;
                this.runBTN.Opacity = 0.5;
                this.pauseBTN.IsEnabled = false;
                this.pauseBTN.Opacity = 0.5;
                this.stopBTN.IsEnabled = false;
                this.stopBTN.Opacity = 0.5;
                this.deleteBTN.IsEnabled = false;
                this.deleteBTN.Opacity = 0.5;
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
            EasySaveViewModel<BackupJob>.Get().JobBuilder.Clear();

            CreateJob_Window create = new CreateJob_Window()
            {
                Titre = "Create Job"
            };
            while (create.ShowDialog() == true)
            {
                create.Owner = Window.GetWindow(App.Current.MainWindow);
            }
            this.jobsDatagrid.ItemsSource = null;
            this.jobsDatagrid.ItemsSource = EasySaveViewModel<BackupJob>.Get().AvailableJobs;
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            CreateJob_Window create = new CreateJob_Window()
            {
                Titre = "Modify Job"
            };

            var selectedJob = ((BackupJob)this.jobsDatagrid.SelectedItem).Name;
            EasySaveViewModel<BackupJob>.Get().LoadJobInBuilderCommand.Execute(selectedJob);

            while (create.ShowDialog() == true)
            {
                create.Owner = Window.GetWindow(App.Current.MainWindow);

                create.jobTargetInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
                create.jobSourceInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
                create.jobNameInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            }
            this.jobsDatagrid.ItemsSource = null;
            this.jobsDatagrid.ItemsSource = EasySaveViewModel<BackupJob>.Get().AvailableJobs;

        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteJob_Window delete = new DeleteJob_Window();

            var selectedJob = ((BackupJob)this.jobsDatagrid.SelectedItem).Name;
            EasySaveViewModel<BackupJob>.Get().LoadJobInBuilderCommand.Execute(selectedJob);

            while (delete.ShowDialog() == true)
            {
                delete.Owner = Window.GetWindow(App.Current.MainWindow);

                delete.deleteJobInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            }
            this.jobsDatagrid.ItemsSource = null;
            this.jobsDatagrid.ItemsSource = EasySaveViewModel<BackupJob>.Get().AvailableJobs;

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
            var selectedJobs = ((BackupJob)this.jobsDatagrid.SelectedItems).Name;
            EasySaveViewModel<BackupJob>.Get().RunMultipleJobsCommand.Execute(selectedJobs);
        }
        public void dailyLogBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = EasySaveViewModel<BackupJob>.Get().DailyLogPath.ToString();

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.Start();
        }
        public void statusLogBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = EasySaveViewModel<BackupJob>.Get().StatusLogPath.ToString();

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.Start();
        }
        private void DataGridSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            this.runBTN.IsEnabled = true;
            this.runBTN.Opacity = 1;
            this.pauseBTN.IsEnabled = true;
            this.pauseBTN.Opacity = 1;
            this.stopBTN.IsEnabled = true;
            this.stopBTN.Opacity = 1;
            this.deleteBTN.IsEnabled = true;
            this.deleteBTN.Opacity = 1;
        }
    }
}
