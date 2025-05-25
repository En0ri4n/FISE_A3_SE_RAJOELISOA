using CLEA.EasySaveCore.Models;
using EasySaveCore.Models;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;

namespace CLEA.EasySaveCore.ViewModel
{
    public sealed class ViewModelBackupJobBuilder : ViewModelJobBuilder<BackupJob>
    {
        private string _name = string.Empty;
        private string _source = string.Empty;
        private string _target = string.Empty;
        private JobExecutionStrategy.StrategyType _strategyType = JobExecutionStrategy.StrategyType.Full;
        private bool _isEncrypted = false;

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
        
        public JobExecutionStrategy.StrategyType StrategyType
        {
            get => _strategyType;
            set { _strategyType = value; OnPropertyChanged(); }
        }
        
        public bool IsEncrypted
        {
            get => _isEncrypted;
            set { _isEncrypted = value; OnPropertyChanged(); }
        }

        public JobExecutionStrategy.StrategyType[] AvailableStrategies
        {
            get
            {
                return Enum.GetValues(typeof(JobExecutionStrategy.StrategyType)) as JobExecutionStrategy.StrategyType[]
                       ?? Array.Empty<JobExecutionStrategy.StrategyType>();
            }
        }

        public ICommand ShowFolderDialogCommand { get; set; }

        public ViewModelBackupJobBuilder()
        {
            ShowFolderDialogCommand = new RelayCommand(input =>
            {
                bool isSource = bool.Parse((string)input!);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                string title = "Select Target Path";

                folderBrowserDialog.Title = title;
                string path = Target;

                if (isSource)
                {
                    folderBrowserDialog.Title = "Select Source Path";
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
        
        public override void Clear()
        {
            Name = string.Empty;
            Source = string.Empty;
            Target = string.Empty;
            StrategyType = JobExecutionStrategy.StrategyType.Full;
            IsEncrypted = false;
        }

        public override void GetFrom(BackupJob job)
        {
            InitialName = job.Name;
            Name = job.Name;
            Source = job.Source;
            Target = job.Target;
            StrategyType = job.StrategyType;
            IsEncrypted = job.IsEncrypted;
        }

        public override BackupJob Build(bool clear = true)
        {
            BackupJob job = new BackupJob(Name, Source, Target, StrategyType, IsEncrypted);
            if (clear)
                Clear();
            return job;
        }
    }
}