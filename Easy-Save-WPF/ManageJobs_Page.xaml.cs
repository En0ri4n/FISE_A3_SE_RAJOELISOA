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
    /// Logique d'interaction pour ManageJobs_Page.xaml
    /// </summary>
    public partial class ManageJobs_Page : Page
    {
        //MainWindow window = new MainWindow();
        Options_Page options = new Options_Page();

        public ManageJobs_Page()
        {
            InitializeComponent();
        }
        public void QuitBTN_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            //window.MainFrame.Content = options;
            //NavigationCommands.BrowseBack;
        }

        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            CreateJob_Window create = new CreateJob_Window();
            create.Show();
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            CreateJob_Window create = new CreateJob_Window();
            create.Show();
        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteJob_Window create = new DeleteJob_Window();
            create.Show();
        }


    }
}
