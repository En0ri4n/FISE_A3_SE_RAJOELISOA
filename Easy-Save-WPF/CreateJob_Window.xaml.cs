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
using System.Windows.Shapes;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Models;
using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.View;
using CLEA.EasySaveCore.Models;
using System.Reflection.Emit;
using System.Windows.Forms;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>

    //TODO : File explorer
    public partial class CreateJob_Window : Window
    {
        public string Titre { get; set; }

        /*TODO FIX THIS

        public CreateJob_Window()
        {

            InitializeComponent();
            gridInputs.DataContext = BackupJobViewModel.Get().JobBuilder;
            title.DataContext = this;
            createBTN.DataContext = this;
            //this.jobTargetInput.DataContext = BackupJobViewModel.Get().JobBuilder;
            //this.jobSourceInput.DataContext = BackupJobViewModel.Get().JobBuilder;
            //this.jobNameInput.DataContext = BackupJobViewModel.Get().JobBuilder;
        }

        public void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        public void CreateBTN_Click(object sender, RoutedEventArgs e)
        {
            if(Titre == "Create Job")
            {
                if (!BackupJobViewModel.Get().IsNameValid(jobNameInput.Text, true))
                {
                    nameErrorLabel.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (!BackupJobViewModel.Get().DoesDirectoryPathExist(jobSourceInput.Text))
            {
                sourceErrorLabel.Visibility = Visibility.Visible;
                return;
            }
            if (!BackupJobViewModel.Get().IsDirectoryPathValid(jobTargetInput.Text))
            {
                targetErrorLabel.Visibility = Visibility.Visible;
                return;
            }

            //ViewModelBackupJobBuilder a = (ViewModelBackupJobBuilder) BackupJobViewModel.Get().JobBuilder;
            //BackupJobViewModel.Get().JobBuilder.ToString();

            BackupJobViewModel.Get().BuildJobCommand.Execute(null);

            Close();
        }
        public void sourceFolderBTN_CLick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            var response = dialog.ShowDialog();

            if (response.ToString() != string.Empty)
            {
                BackupJobViewModel.Get().GetJobBuilder().Source = dialog.SelectedPath;
                return;
            }
        }

        public void targetFolderBTN_CLick(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            var response = dialog.ShowDialog();

            if (response.ToString() != string.Empty)
            {
                BackupJobViewModel.Get().GetJobBuilder().Target = dialog.SelectedPath;
                return;
            }
        }
        */
        public void CreateButton_Click(object sender, RoutedEventArgs e) { }
        public void CloseButton_Click(object sender, RoutedEventArgs e) { }
        public CreateJob_Window()
        {

            InitializeComponent();
            //this.jobTargetInput.DataContext = BackupJobViewModel.Get().JobBuilder;
            //this.jobSourceInput.DataContext = BackupJobViewModel.Get().JobBuilder;
            //this.jobNameInput.DataContext = BackupJobViewModel.Get().JobBuilder;
        }
    }
}
