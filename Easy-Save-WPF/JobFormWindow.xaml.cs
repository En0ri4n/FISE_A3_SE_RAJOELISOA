using System.Windows;
using CLEA.EasySaveCore.Translations;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    /// <summary>
    ///     Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>
    public partial class JobFormWindow : Window
    {
        public JobFormWindow(BackupJobViewModel viewModel, string menuName, bool isJobCreation)
        {
            InitializeComponent();
            DataContext = viewModel.GetJobBuilder();
            FooterCreateJob.DataContext = viewModel;

            viewModel.CloseAction = Close;

            Title = L10N.Get().GetTranslation($"{menuName}.title");
            MainTitle.Text = Title;
            MainSubtitle.Text = L10N.Get().GetTranslation($"{menuName}.subtitle");

            DoneButton.Content = L10N.Get().GetTranslation($"{menuName}.button.done");
            DoneButton.CommandParameter = isJobCreation.ToString();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}