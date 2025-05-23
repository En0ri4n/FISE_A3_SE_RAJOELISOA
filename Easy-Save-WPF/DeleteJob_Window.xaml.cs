using System.Windows;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour DeleteJob_Window.xaml
    /// </summary>
    public partial class DeleteJob_Window : Window
    {
        static BackupJobViewModel ViewModel => BackupJobViewModel.Get();

        public DeleteJob_Window()
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
