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

namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour CreateJob_Window.xaml
    /// </summary>
    
    //TODO : File explorer
    public partial class CreateJob_Window : Window
    {
        public string Titre { get; set; }
        public string jobName { get; set; }
        public string jobTarget { get; set; }
        public string jobSource { get; set; }


        public CreateJob_Window()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        public void CreateBTN_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }
        public void folderBTN_CLick(object sender, RoutedEventArgs e)
        {
            //TODO
        }
    }
}
