using System.Diagnostics;
using System.Windows;

namespace EasySaveRemote
{
    public partial class OptionsPopup : Window
    {
        public OptionsPopup()
        {
            // RemoteClient.Get().ViewModel.LoadEncryptionKeyCommand.Execute(null);
            InitializeComponent();
            DataContext = RemoteClient.Get().ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        public void configFileBTN_Click(object sender, RoutedEventArgs e)
        {
            var path = "config.json";

            using Process myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.StartInfo.Verb = "open";
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.Start();
        }
    }
}
