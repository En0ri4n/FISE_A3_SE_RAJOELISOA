using System.Windows;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour DeleteJob_Window.xaml
    /// </summary>
    public partial class DeleteJobWindow : Window
    {
        static BackupJobViewModel ViewModel => BackupJobViewModel.Get();

        public DeleteJobWindow()
        {
            InitializeComponent();
            this.deleteJobInput.DataContext = ViewModel.JobBuilder;
        }

        public void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void DeleteBTN_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteJobCommand.Execute(this.deleteJobInput.Text);
            Close();
        }
    }
}
