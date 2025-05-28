using System.ComponentModel;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.ViewModel
{
    public abstract class EasySaveViewModelBase : INotifyPropertyChanged
    {
        public ViewModelJobBuilderBase JobBuilderBase;
        public JobManager JobManager;
        
        public abstract bool IsRunning { get; set; }

        public EasySaveConfigurationBase JobConfiguration { get; set; }

        public abstract event PropertyChangedEventHandler? PropertyChanged;
        
        protected EasySaveViewModelBase(EasySaveConfigurationBase configuration)
        {
            JobConfiguration = configuration;
        }

        public void InitializeViewModel(JobManager jobManager)
        {
            JobManager = jobManager;
            InitializeCommand();
        }

        public void SetJobBuilder(ViewModelJobBuilderBase jobBuilderBase)
        {
            JobBuilderBase = jobBuilderBase;
        }

        protected abstract void InitializeCommand();
    }
}