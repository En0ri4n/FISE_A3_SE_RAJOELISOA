using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;

namespace CLEA.EasySaveCore.View
{
    public abstract class EasySaveView<TJob, TJobManager, TConfiguration, TViewModel, TViewModelObjectBuilder> where TJob : IJob where TJobManager : JobManager<TJob> where TConfiguration : EasySaveConfigurationBase where TViewModel : EasySaveViewModelBase<TJob, TJobManager>
        where TViewModelObjectBuilder : ViewModelJobBuilder<TJob>
    {
        protected readonly L10N<TJob> L10N = L10N<TJob>.Get();
        public readonly EasySaveCore<TJob, TJobManager, TConfiguration> Core;
        protected TViewModel ViewModel;

        protected EasySaveView(EasySaveCore<TJob, TJobManager, TConfiguration> core, TViewModel viewModel, TViewModelObjectBuilder viewModelObjectBuilder)
        {
            Core = core;
            ViewModel = viewModel;
            viewModel.SetJobBuilder(viewModelObjectBuilder);
        }

        public TViewModelObjectBuilder GetJobBuilder()
        {
            return (TViewModelObjectBuilder)ViewModel.JobBuilder;
        }

        protected abstract void DisplayMainMenu();
        protected abstract void DisplayJobMenu();
        protected abstract void DisplayLanguageMenu();
        protected abstract void DisplayLogTypeMenu();
        protected abstract void DisplayJobResultMenu(int jobNumber);
        protected abstract void DisplaySettingsMenu();
        protected abstract void DisplayJobListMenu();
        protected abstract void DisplayRunMenu();
        protected abstract void DisplayRunMultipleMenu();
        protected abstract void DisplayRunAllMenu();
        protected abstract void DisplayCreateJobMenu();
        protected abstract void DisplayModifyJobMenu();
        protected abstract void DisplayDeleteJobMenu();
        protected abstract void DisplayDailyLogDirectoryMenu();
        protected abstract void DisplayStatusLogDirectoryMenu();
    }
}