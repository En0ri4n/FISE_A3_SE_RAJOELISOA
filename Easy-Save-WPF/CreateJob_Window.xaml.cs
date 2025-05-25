using System;
using System.Windows;
using System.Windows.Forms;
using CLEA.EasySaveCore.L10N;
using EasySaveCore.Jobs.Backup.ViewModels;
using EasySaveCore.Models;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>

    public partial class CreateJob_Window : Window
    {
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        public CreateJob_Window(string menuName)
        {

            InitializeComponent();
            DataContext = BackupJobViewModel.Get().GetJobBuilder();
            FooterCreateJob.DataContext = BackupJobViewModel.Get();

            BackupJobViewModel.Get().CloseAction = new Action(() => this.Close());

            Title = L10N<BackupJob>.Get().GetTranslation($"{menuName}.title");
            MainTitle.Text = Title;
            MainSubtitle.Text = L10N<BackupJob>.Get().GetTranslation($"{menuName}.subtitle");

            DoneButton.Content = L10N<BackupJob>.Get().GetTranslation($"{menuName}.button.done");
        }
    }
}
