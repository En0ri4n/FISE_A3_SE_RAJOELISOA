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
            NetworkClient networkClient = new NetworkClient();
            ManageJobsWindow window = new ManageJobsWindow();   
            Socket clientSocket = networkClient.Connect(URL, port);

            Thread receiveThread = new Thread(() => networkClient.ListenToServer());
            receiveThread.Start();

            Close();
            networkClient.Disconnect();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
