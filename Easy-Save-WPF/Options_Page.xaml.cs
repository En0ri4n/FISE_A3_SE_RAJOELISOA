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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour Options_Page.xaml
    /// </summary>
    public partial class Options_Page : Page
    {
        public Options_Page()
        {
            InitializeComponent();
        }

        public void LanguageBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.LanguageBTN_Click(sender, e);
        }
        public void LogTypeBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.LogTypeBTN_Click(sender, e);
        }
        public void DailyLogBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.DailyLogBTN_Click(sender, e);
        }
        public void StatusLogBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.StatusLogBTN_Click(sender, e);
        }
        public void ReturnBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.ReturnBTN_Click(sender, e);
        }
    }
}
