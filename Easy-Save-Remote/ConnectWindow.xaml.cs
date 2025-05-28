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
    public partial class ConnectWindow : Window
    {
        public string URL;
        public int port;


        public ConnectWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {

            Client client = new Client();
            MainWindow window = new MainWindow();   
            Socket clientSocket = client.Connect(URL, port);

            Thread receiveThread = new Thread(() => client.LoadData());
            receiveThread.Start();

            Close();
            client.Disconnect();
        }

    }
}
