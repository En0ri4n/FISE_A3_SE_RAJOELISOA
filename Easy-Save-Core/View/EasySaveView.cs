using CLEA.EasySaveCore.Translations;
using CLEA.EasySaveCore.ViewModel;

namespace CLEA.EasySaveCore.View
{
    public abstract class EasySaveView
    {
        public readonly Core.EasySaveCore Core;
        protected L10N L10N => L10N.Get();

        protected EasySaveView(Core.EasySaveCore core, EasySaveViewModelBase viewModel,
            ViewModelJobBuilderBase viewModelObjectBuilder)
        {
            Core = core;
            viewModel.SetJobBuilder(viewModelObjectBuilder);
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