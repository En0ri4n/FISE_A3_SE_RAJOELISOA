using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;

namespace CLEA.EasySaveCore.View;

public abstract class EasySaveView<TJob, TViewModelObjectBuilder> where TJob : IJob where TViewModelObjectBuilder : ViewModelJobBuilder<TJob>
{
    protected readonly L10N<TJob> L10N = L10N<TJob>.Get();
    public readonly EasySaveCore<TJob> Core;
    protected EasySaveViewModel<TJob> ViewModel => EasySaveViewModel<TJob>.Get();
    
    protected EasySaveView(EasySaveCore<TJob> core, TViewModelObjectBuilder viewModelObjectBuilder)
    {
        Core = core;
        ViewModel.SetJobBuilder(viewModelObjectBuilder);
    }
    
    public TViewModelObjectBuilder GetJobBuilder()
    {
        return (TViewModelObjectBuilder) ViewModel.JobBuilder;
    }

    protected abstract void DisplayMainMenu();
    protected abstract void DisplayJobMenu();
    protected abstract void DisplayLanguageMenu();
    protected abstract void DisplayLogTypeMenu();
    protected abstract void DisplayJobResultMenu();
    protected abstract void DisplaySettingsMenu();
    protected abstract void DisplayDailyLogDirectoryMenu();
    protected abstract void DisplayStatusLogDirectoryMenu();

}