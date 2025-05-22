using System.ComponentModel;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel
{
    public abstract class EasySaveViewModelBase<TJob, TJobManager> : INotifyPropertyChanged where TJob : IJob where TJobManager : JobManager<TJob>
    {
        public TJobManager JobManager;

        public ViewModelJobBuilder<TJob> JobBuilder;
        
        protected EasySaveViewModelBase() { }

        public void InitializeViewModel(TJobManager jobManager)
        {
            JobManager = jobManager;
            InitializeCommand();
        }

        public void SetJobBuilder(ViewModelJobBuilder<TJob> jobBuilder)
        {
            JobBuilder = jobBuilder;
        }

        protected abstract void InitializeCommand();

        public abstract event PropertyChangedEventHandler? PropertyChanged;
    }

    // Options PopUp Methods
}