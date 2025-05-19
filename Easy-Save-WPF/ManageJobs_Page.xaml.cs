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
    
    //TODO : dynamic job list binding
    public partial class ManageJobs_Page : Page
    {
        //MainWindow window = new MainWindow();
        Options_Page options = new Options_Page();

        public ManageJobs_Page()
        {
            InitializeComponent();
            ManageJobs_Grid.DataContext = options;
        }
        public void QuitBTN_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void OptionsBTN_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.OptionsBTN_Click(sender, e);
        }

        public void CreateWindow_Click(object sender, RoutedEventArgs e)
        {
            CreateJob_Window create = new CreateJob_Window()
            {
                Titre = "Create Job"
            };
            create.Show();
        }
        public void ModifyWindow_Click(object sender, RoutedEventArgs e)
        {
            CreateJob_Window create = new CreateJob_Window()
            {
                Titre = "Modify Job"
            };
            create.Show();
        }
        public void DeleteWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteJob_Window create = new DeleteJob_Window();
            create.Show();
        }

        public void RunOne_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        public void RunAll_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

    }
}
