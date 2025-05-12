using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;

namespace CLEA.EasySaveCore.View;

public abstract class EasySaveView<TJob>(EasySaveCore<TJob> core) where TJob : IJob
{
    protected readonly L10N<TJob> L10N = L10N<TJob>.Get();
    public readonly EasySaveCore<TJob> Core = core;
    protected EasySaveViewModel<TJob> ViewModel => EasySaveViewModel<TJob>.Get();

    protected abstract void DisplayMainMenu();
    protected abstract void DisplayJobMenu();
    protected abstract void DisplayLanguageMenu();
    protected abstract void DisplayLogTypeMenu();
    protected abstract void DisplayJobResultMenu();
    protected abstract void DisplayJobSettingsMenu();
}