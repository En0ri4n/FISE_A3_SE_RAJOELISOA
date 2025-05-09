namespace CLEA.EasySaveCore.View;

public abstract class EasySaveView
{
    protected readonly L10N.L10N L10N = CLEA.EasySaveCore.L10N.L10N.Get();
    protected readonly EasySaveCore Core = EasySaveCore.Get();
    
    protected abstract void DisplayMainMenu();
    protected abstract void DisplayJobMenu();
    protected abstract void DisplayLanguageMenu();
    
    protected abstract void DisplayJobResultMenu();
    protected abstract void DisplayJobSettingsMenu();
}