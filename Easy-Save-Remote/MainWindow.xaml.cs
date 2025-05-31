using System;
using System.Windows;

namespace EasySaveRemote
{
    /// <summary>
    /// Logique d'interaction pour ConnectWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string url { get; set; }
        public int port { get; set; } = 5000; // Default port, can be changed
        
        public MainWindow()
        {
            RemoteClient.Initialize();
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        { 
            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("Please enter a valid URL.", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (port <= 0 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Invalid Port", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Attempt to connect to the server
            bool connected = RemoteClient.Get().NetworkClient.Connect(url, port);
            if (!connected)
            {
                MessageBox.Show("Failed to connect to the server. Please check the URL and port.", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            ManageJobsWindow window = new ManageJobsWindow();
            window.Show();

            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
