using System;
using System.Collections.Generic;
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


    public partial class ManageJobs_Page : Page
    {
        //static EasySaveViewModel<BackupJob> ViewModel => EasySaveViewModel<BackupJob>.Get();
        public string TitreJobs;

        public ManageJobs_Page()
        {
            InitializeComponent();
            this.DataContext = EasySaveViewModel<BackupJob>.Get();
            this.jobsDatagrid.ItemsSource = EasySaveViewModel<BackupJob>.Get().AvailableJobs;
            //this.jobsDatagrid.DataContext = this;
        }
        public void QuitBTN_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.OptionsBTN_Click(sender, e);
        }

        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            CreateJob_Window create = new CreateJob_Window()
            {
                Titre = "Create Job"
            };
            while (create.ShowDialog() == true)
            {

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

            create.Show();

            var selectedJob = ((BackupJob)this.jobsDatagrid.SelectedItem).Name;
            EasySaveViewModel<BackupJob>.Get().LoadJobInBuilderCommand.Execute(selectedJob);

            create.jobTargetInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            create.jobSourceInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            create.jobNameInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;

        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteJob_Window create = new DeleteJob_Window();
            create.Show();
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
            //ViewModel.RunMultipleJobsCommand.Execute(jobListName);
        }

        public void Selected(object sender, RoutedEventArgs e)
        {
            //TODO select all job
            //https://stackoverflow.com/questions/14441273/how-to-handle-checkbox-checked-unchecked-event-with-command-in-viewmodel-in-silv/14442515
        }

    }
}
