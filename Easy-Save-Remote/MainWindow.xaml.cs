using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using EasySaveRemote.Client;

namespace EasySaveRemote
{
    /// <summary>
    /// Logique d'interaction pour ConnectWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string URL;
        public int port;
        
        public MainWindow()
        {
            RemoteClient.Initialize();
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ManageJobsWindow window = new ManageJobsWindow();   
            RemoteClient.Get().NetworkClient.Connect(URL, port);
            
            window.Show();

            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
