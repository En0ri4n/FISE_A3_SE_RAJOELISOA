using System.Windows;
using FolderBrowserEx;
using System.Windows.Controls;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Models;
using Microsoft.Win32;
using System;

namespace Easy_Save_WPF
{
    public partial class Options_PopUp : Window
    {
        public Options_PopUp()
        {
            InitializeComponent();
            DataContext = EasySaveViewModel<BackupJob>.Get();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
