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
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;


namespace Easy_Save_Remote
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

            Thread receiveThread = new Thread(() => networkClient.LoadData());
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
