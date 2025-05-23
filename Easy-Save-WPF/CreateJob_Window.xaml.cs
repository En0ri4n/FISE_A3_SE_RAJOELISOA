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

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>
    
    //TODO : File explorer
    public partial class CreateJob_Window : Window
    {
        public string Titre { get; set; }

        public CreateJob_Window()
        {
            InitializeComponent();
            this.gridInputs.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            title.DataContext = this;
            createBTN.DataContext = this;
            //this.jobTargetInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            //this.jobSourceInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;
            //this.jobNameInput.DataContext = EasySaveViewModel<BackupJob>.Get().JobBuilder;

               
        }

        public void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public void CreateBTN_Click(object sender, RoutedEventArgs e)
        {
            if(this.Titre == "Create Job")
            {
                if (!EasySaveViewModel<BackupJob>.Get().IsNameValid(jobNameInput.Text, true))
                {
                    nameErrorLabel.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (!EasySaveViewModel<BackupJob>.Get().DoesDirectoryPathExist(jobSourceInput.Text))
            {
                sourceErrorLabel.Visibility = Visibility.Visible;
                return;
            }
            if (!EasySaveViewModel<BackupJob>.Get().IsDirectoryPathValid(jobTargetInput.Text))
            {
                targetErrorLabel.Visibility = Visibility.Visible;
                return;
            }

            //ViewModelBackupJobBuilder a = (ViewModelBackupJobBuilder) EasySaveViewModel<BackupJob>.Get().JobBuilder;
            //EasySaveViewModel<BackupJob>.Get().JobBuilder.ToString();

            EasySaveViewModel<BackupJob>.Get().BuildJobCommand.Execute(null);

            Close();
        }
        public void sourceFolderBTN_CLick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            var response = dialog.ShowDialog();

            if (response.ToString() != string.Empty)
            {
                this.jobSourceInput.Text = dialog.SelectedPath;
                return;
            }
        }

        public void targetFolderBTN_CLick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            var response = dialog.ShowDialog();

            if (response.ToString() != string.Empty)
            {
                this.jobTargetInput.Text = dialog.SelectedPath;
                return;
            }
        }
    }
}
