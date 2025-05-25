using CLEA.EasySaveCore.Models;
using EasySaveCore.Models;

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

        public override BackupJob Build()
        {
            BackupJob job = new BackupJob(Name, Source, Target, StrategyType, IsEncrypted);
            Clear();
            return job;
        }
    }
}