using System.Windows;
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
            gridInputs.DataContext = BackupJobViewModel.Get().GetJobBuilder();
            title.DataContext = this;
            createBTN.DataContext = this;
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
