using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using EasySaveShared.Client.Commands;
using EasySaveShared.DataStructures;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;

namespace EasySaveShared.Client.ViewModel
{
    public class ClientBackupJobBuilder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _name = string.Empty;
        private string _source = string.Empty;
        private string _target = string.Empty;
        private SharedExecutionStrategyType _strategyType = SharedExecutionStrategyType.Full;
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

        public SharedExecutionStrategyType StrategyType
        {
            get => _strategyType;
            set { _strategyType = value; OnPropertyChanged(); }
        }

        public bool IsEncrypted
        {
            get => _isEncrypted;
            set { _isEncrypted = value; OnPropertyChanged(); }
        }

        public SharedExecutionStrategyType[] AvailableStrategies =>
            Enum.GetValues(typeof(SharedExecutionStrategyType)) as SharedExecutionStrategyType[]
            ?? Array.Empty<SharedExecutionStrategyType>();

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
            StrategyType = SharedExecutionStrategyType.Full;
            IsEncrypted = false;
        }

        public void GetFrom(SharedBackupJob job)
        {
            InitialName = job.InitialName;
            Name = job.Name;
            Source = job.Source;
            Target = job.Target;
            StrategyType = job.StrategyType;
            IsEncrypted = job.IsEncrypted;
        }

        public SharedBackupJob Build(bool clear = true)
        {
            SharedBackupJob job = new SharedBackupJob(InitialName, Name, Source, Target, StrategyType, IsEncrypted, 0.0D, SharedExecutionStatus.NotStarted);
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