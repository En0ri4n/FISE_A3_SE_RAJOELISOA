using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using EasySaveRemote.Client.Commands;
using EasySaveRemote.Client.DataStructures;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;

namespace EasySaveRemote.Client.ViewModel
{
    public sealed class ClientBackupJobBuilder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _name = string.Empty;
        private string _source = string.Empty;
        private string _target = string.Empty;
        private ClientJobExecutionStrategyType _strategyType = ClientJobExecutionStrategyType.Full;
        private bool _isEncrypted = false;

        private string _initialName = string.Empty;

        public string InitialName
        {
            get => _initialName;
            protected set
            {
                _initialName = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Source
        {
            get => _source;
            set { _source = value; OnPropertyChanged(); }
        }

        public string Target
        {
            get => _target;
            set { _target = value; OnPropertyChanged(); }
        }

        public ClientJobExecutionStrategyType StrategyType
        {
            get => _strategyType;
            set { _strategyType = value; OnPropertyChanged(); }
        }

        public bool IsEncrypted
        {
            get => _isEncrypted;
            set { _isEncrypted = value; OnPropertyChanged(); }
        }

        public ClientJobExecutionStrategyType[] AvailableStrategies =>
            Enum.GetValues(typeof(ClientJobExecutionStrategyType)) as ClientJobExecutionStrategyType[]
            ?? Array.Empty<ClientJobExecutionStrategyType>();

        public ICommand ShowFolderDialogCommand { get; set; }

        public ClientBackupJobBuilder()
        {
            ShowFolderDialogCommand = new RelayCommand(input =>
            {
                bool isSource = bool.Parse((string)input!);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                const string title = "Choose a target folder";

                folderBrowserDialog.Title = title;
                string path = Target;

                if (isSource)
                {
                    folderBrowserDialog.Title = "Choose a source folder";
                    Source = path;
                }

                string fullPath = Path.IsPathRooted(path) ? path
                : Path.GetFullPath(Path.Combine(".", path));

                folderBrowserDialog.InitialFolder = fullPath;
                folderBrowserDialog.AllowMultiSelect = false;

                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;

                if (isSource)
                {
                    Source = folderBrowserDialog.SelectedFolder;
                }
                else
                {
                    Target = folderBrowserDialog.SelectedFolder;
                }
            }, _ => true);
        }

        public void Clear()
        {
            Name = string.Empty;
            Source = string.Empty;
            Target = string.Empty;
            StrategyType = ClientJobExecutionStrategyType.Full;
            IsEncrypted = false;
        }

        public void GetFrom(ClientBackupJob job)
        {
            InitialName = job.InitialName;
            Name = job.Name;
            Source = job.Source;
            Target = job.Target;
            StrategyType = job.StrategyType;
            IsEncrypted = job.IsEncrypted;
        }

        public ClientBackupJob Build(bool clear = true)
        {
            ClientBackupJob job = new ClientBackupJob(InitialName, Name, Source, Target, StrategyType, IsEncrypted);
            if (clear)
                Clear();
            return job;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}