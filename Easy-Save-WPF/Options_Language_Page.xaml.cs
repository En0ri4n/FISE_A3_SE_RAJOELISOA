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
    /// Logique d'interaction pour Options_Language_Page.xaml
    /// </summary>
    public partial class Options_Language_Page : Page
    {
        public Options_Language_Page()
        {
            InitializeComponent();
            List<string> listLanguages = new List<string>()
            {
                "Test", "TODO", "hello world"
            };
            this.languagesListBox.ItemsSource = listLanguages;
            //TODO
        }
    }
}
