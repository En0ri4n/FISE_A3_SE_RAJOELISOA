using System;
using System.Windows;
using EasySaveShared;

namespace EasySaveShared
{
    /// <summary>
    /// Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>

    public partial class JobFormWindow : Window
    {
        private readonly JobFormWindowType _menuType;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        public JobFormWindow(JobFormWindowType menuType, bool isJobCreation)
        {
            _menuType = menuType;
            InitializeComponent();
            DataContext = RemoteClient.Get().ViewModel.BackupJobBuilder;
            FooterCreateJob.DataContext = RemoteClient.Get().ViewModel;

            RemoteClient.Get().ViewModel.CloseAction = Close;

            Title = menuType switch
            {
                JobFormWindowType.Create => "Create Job",
                JobFormWindowType.Edit => "Edit Job",
                _ => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null)
            };
            MainTitle.Text = Title;
            MainSubtitle.Text = menuType switch
            {
                JobFormWindowType.Create => "Create a new job",
                JobFormWindowType.Edit => "Edit an existing job",
                _ => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null)
            };

            DoneButton.Content = menuType switch
            {
                JobFormWindowType.Create => "Create",
                JobFormWindowType.Edit => "Save",
                _ => throw new ArgumentOutOfRangeException(nameof(menuType), menuType, null)
            };
            DoneButton.CommandParameter = isJobCreation.ToString();
        }

        public void CreateJobBTN_Click(object sender, RoutedEventArgs routedEventArgs) {
            RemoteClient.Get().ViewModel.BuildJobCommand.Execute(_menuType == JobFormWindowType.Create);
        }
    }

    public enum JobFormWindowType
    {
        Create,
        Edit
    }
}
