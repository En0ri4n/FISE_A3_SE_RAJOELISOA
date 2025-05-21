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
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Models;


namespace Easy_Save_WPF
{
    /// <summary>
    /// Logique d'interaction pour DeleteJob_Window.xaml
    /// </summary>
    public partial class DeleteJob_Window : Window
    {
        static EasySaveViewModel<BackupJob> ViewModel => EasySaveViewModel<BackupJob>.Get();
        public string jobName { get; set; } //TODO : get job name from selected datagrid row
        public DeleteJob_Window()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void CancelBTN_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void DeleteBTN_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeleteJobCommand.Execute(jobName);
        }
    }
}
