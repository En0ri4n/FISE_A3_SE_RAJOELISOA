using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.View;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;

namespace CLEA.EasySaveCLI;

public sealed class EasySaveCli : EasySaveView<BackupJob, ViewModelBackupJobBuilder>
{
    enum Menu
    {
        Main,
        Job,
        JobList,
        RunJob,
        RunMultipleJobs,
        RunAllJobs,
        CreateJob,
        ModifyJob,
        DeleteJob,
        Language,
        LogType,
        DailyLogDirectory,
        StatusLogDirectory,
        Settings
    }

    private readonly List<Menu> menuHistory = new List<Menu>();

    private void AddToMenuHistory(Menu menuName)
    {
        menuHistory.Add(menuName);
    }

    public EasySaveCli() : base(EasySaveCore<BackupJob>.Init(new BackupJobManager()), new ViewModelBackupJobBuilder())
    {
        Console.Title = L10N.GetTranslation("main.title");
        AddToMenuHistory(Menu.Main);
        DisplayMainMenu();
    }
    
    protected override void DisplayMainMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(L10N.GetTranslation("main.title")).Color(Color.Green3).Centered());
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("main_menu.title"))
                .AddChoices(
                    L10N.GetTranslation("main_menu.jobs"),
                    L10N.GetTranslation("main_menu.settings"),
                    L10N.GetTranslation("main_menu.exit")
                ));

        if (choice == L10N.GetTranslation("main_menu.jobs"))
        {
            AddToMenuHistory(Menu.Job);
            DisplayJobMenu();
        }
        else if (choice == L10N.GetTranslation("main_menu.settings"))
        {
            AddToMenuHistory(Menu.Settings);
            DisplaySettingsMenu(); 
        }
        else if(choice == L10N.GetTranslation("main_menu.exit"))
        {
            Exit();
        }
        /*else if (choice == "test_file_explorer")
        {
            FileBrowser.Browser browser = new FileBrowser.Browser();
            browser.GetFolderPath().RunSynchronously();
        }*/
    }
    
    protected override void DisplayJobMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("job_menu.title")).Centered());

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices(
                    L10N.GetTranslation("job_menu.list_job"),
                    L10N.GetTranslation("job_menu.run_job"),
                    L10N.GetTranslation("job_menu.run_multiple_jobs"),
                    L10N.GetTranslation("job_menu.run_all_jobs"),
                    L10N.GetTranslation("job_menu.create_job"),
                    L10N.GetTranslation("job_menu.modify_job"),
                    L10N.GetTranslation("job_menu.delete_job"),
                    L10N.GetTranslation("main.go_back")
                ));
        if (choice == L10N.GetTranslation("job_menu.list_job"))
        {
            AddToMenuHistory(Menu.JobList);
            DisplayJobListMenu();
        }
        else if (choice == L10N.GetTranslation("job_menu.run_job"))
        {
            AddToMenuHistory(Menu.RunJob);
            DisplayRunMenu();
        }
        else if (choice == L10N.GetTranslation("job_menu.run_multiple_jobs"))
        {
            AddToMenuHistory(Menu.RunMultipleJobs);
            DisplayRunMultipleMenu();
        }
        else if (choice == L10N.GetTranslation("job_menu.run_all_jobs"))
        {
            AddToMenuHistory(Menu.RunAllJobs);
            DisplayRunAllMenu();
        }
        else if (choice == L10N.GetTranslation("job_menu.create_job"))
        {
            AddToMenuHistory(Menu.CreateJob);
            DisplayCreateJobMenu();
        }
        else if (choice == L10N.GetTranslation("job_menu.modify_job"))
        {
            AddToMenuHistory(Menu.ModifyJob);
            DisplayModifyJobMenu();
        }
        else if (choice == L10N.GetTranslation("job_menu.delete_job"))
        {
            AddToMenuHistory(Menu.DeleteJob);
            DisplayDeleteJobMenu();
        }
        else if (choice == L10N.GetTranslation("main.go_back"))
        {
            GoBack();
        }
    }

    protected override void DisplayLanguageMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
        
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("language_menu.title"))
                .AddChoices(
                    ViewModel.AvailableLanguages.Select(l => l.Name).ToArray().Append(L10N.GetTranslation("main.go_back"))
                ));
        if (choice != L10N.GetTranslation("main.go_back"))
        {
            LangIdentifier selectedLang = Languages.SupportedLangs.First(li => li.Name == choice);

            if (selectedLang != L10N.GetLanguage())
            {
                ViewModel.CurrentApplicationLang = selectedLang;
            }
        }
        GoBack();
    }
    
    protected override void DisplayLogTypeMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("logtype_menu.title").Replace("{LOGTYPE}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat.ToString()))
                .AddChoices(
                    ViewModel.AvailableDailyLogFormats.Select(f=> f.ToString()).Append(L10N.GetTranslation("main.go_back"))
                ));

        if (choice != L10N.GetTranslation("main.go_back"))
        {
            ViewModel.CurrentDailyLogFormat = Enum.Parse<Format>(choice);
        }
        GoBack();
    }

    protected override void DisplayJobListMenu()
    {
        AnsiConsole.Write(new Text(L10N.GetTranslation("job_menu.list_job")).Centered());
        Table table = new Table();

        table.AddColumns([L10N.GetTranslation("job_menu.column.name"), L10N.GetTranslation("job_menu.column.source"), L10N.GetTranslation("job_menu.column.target")]);

        foreach (BackupJob job in ViewModel.JobManager.GetJobs())
            table.AddRow([job.Name, job.Source.Value, job.Target.Value]);
        AnsiConsole.Write(table);
        AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
        Console.ReadKey();
        GoBack();
    }

    protected bool DisplayPromptRunStrategy()
    {
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title(L10N.GetTranslation("run_strategy_menu.title"))
            .AddChoices(
                L10N.GetTranslation("run_strategy_menu.option_full"),
                L10N.GetTranslation("run_strategy_menu.option_differential"),
                L10N.GetTranslation("main.go_back")
            ));
        if (choice != L10N.GetTranslation("main.go_back"))
        {
            ViewModel.ChangeRunStrategyCommand.Execute(choice == L10N.GetTranslation("run_strategy_menu.option_full") ? StrategyType.Full.ToString() : StrategyType.Differential.ToString());
            return true;
        }
        return false;
    }

    protected override void DisplayRunMenu()
    {
        string jobName = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(L10N.GetTranslation("job_menu.title_one"))
            .AddChoices(
                ViewModel.JobManager.GetJobs().Select(job => job.Name).ToArray().Append(L10N.GetTranslation("main.go_back"))
            ));
        if (jobName != L10N.GetTranslation("main.go_back") && DisplayPromptRunStrategy())
        {
            if (!ViewModel.DoesDirectoryPathExist(ViewModel.JobManager.GetJob(jobName).Source.Value))
            {
                ShowErrorScreen(L10N.GetTranslation("error.path_with").Replace("{JOBNAME}", jobName).Replace("{PATH}", ViewModel.JobManager.GetJob(jobName).Source.Value));
                return;
            }
            SetRunHandler([jobName]);
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Aesthetic)
                .SpinnerStyle(Style.Parse("blue"))
                .Start(L10N.GetTranslation("loader.running.text"),
                    ctx => ViewModel.RunJobCommand.Execute(jobName));
            DisplayJobResultMenu(1);
        }
        GoBack();
    }

    protected override void DisplayRunMultipleMenu()
    {
        if (ViewModel.JobManager.JobCount == 0)
        {
            ShowErrorScreen(L10N.GetTranslation("job_menu.error_no_jobs"));
            return;
        }
        List<string> jobListName = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
            .Title(L10N.GetTranslation("job_menu.title_multiple"))
            .NotRequired()
            .AddChoices(
                ViewModel.JobManager.GetJobs().Select(job => job.Name).ToArray()
            ));
        if (jobListName.Count() != 0 && DisplayPromptRunStrategy())
        {
            foreach (string jobName in jobListName)
            {
                if (!ViewModel.DoesDirectoryPathExist(ViewModel.JobManager.GetJob(jobName).Source.Value))
                {
                    ShowErrorScreen(L10N.GetTranslation("error.path_with").Replace("{JOBNAME}", jobName).Replace("{PATH}", ViewModel.JobManager.GetJob(jobName).Source.Value));
                    return;
                }
            }
            SetRunHandler(jobListName.ToArray());
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Grenade)
                .SpinnerStyle(Style.Parse("yellow"))
                .Start(L10N.GetTranslation("loader.running.text"),
                    ctx => ViewModel.RunMultipleJobsCommand.Execute(jobListName));
            DisplayJobResultMenu(jobListName.Count());
        }
        GoBack();
    }
    
    private void SetRunHandler(string[] jobNames)
    {
        ViewModel.OnTaskCompletedFor(jobNames, task =>
        {
            BackupJobTask backupTask = (BackupJobTask) task;
            AnsiConsole.WriteLine(L10N.GetTranslation("job_menu.task_run_information")
                .Replace("{JOB_NAME}", backupTask.Name)
                .Replace("{SOURCE}", backupTask.Source.Value.ToString())
                .Replace("{TARGET}", backupTask.Target.Value.ToString())
                .Replace("{TIME}", backupTask.TransferTime.Value.ToString())
                .Replace("{STATUS}", backupTask.Status.ToString()));
        });
    }

    protected override void DisplayRunAllMenu()
    {
        string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(L10N.GetTranslation("job_menu.title_all"))
            .AddChoices(
                L10N.GetTranslation("job_menu.option_all"),
                L10N.GetTranslation("main.go_back")
        ));
        if (choice != L10N.GetTranslation("main.go_back") && DisplayPromptRunStrategy())
        {
            foreach (BackupJob job in ViewModel.JobManager.GetJobs())
            {
                if (!ViewModel.DoesDirectoryPathExist(job.Source.Value))
                {
                    ShowErrorScreen(L10N.GetTranslation("error.path_with").Replace("{JOBNAME}", job.Name).Replace("{PATH}", job.Source.Value));
                    return;
                }
            }
            SetRunHandler(ViewModel.AvailableJobs.Select(x => x.Name).ToArray());
            AnsiConsole.Status()
            .Spinner(Spinner.Known.Pong)
            .SpinnerStyle(Style.Parse("green"))
            .Start(L10N.GetTranslation("loader.running.text"), ctx =>
            {
                ViewModel.RunAllJobsCommand.Execute(null);
            });
            DisplayJobResultMenu(ViewModel.JobManager.GetJobs().Count());
        }
        GoBack();
    }

    protected override void DisplayCreateJobMenu()
    {
        if (ViewModel.JobManager.JobCount == 5)
        {
            ShowErrorScreen(L10N.GetTranslation("job_menu.error_excessive_jobs"));
            return;
        }
        GetJobBuilder().Name = AnsiConsole.Ask<string>(L10N.GetTranslation("job_menu.name_question"));
        if (!ViewModel.IsNameValid(GetJobBuilder().Name, true))
        {
            ShowErrorScreen(L10N.GetTranslation("job_menu.error_job_exist"));
            return;
        }
        GetJobBuilder().Source = AnsiConsole.Ask<string>(L10N.GetTranslation("information.source_directory"));
        if (!ViewModel.DoesDirectoryPathExist(GetJobBuilder().Source))
        {
            ShowErrorScreen(L10N.GetTranslation("error.path"));
            return;
        }
        GetJobBuilder().Target = AnsiConsole.Ask<string>(L10N.GetTranslation("information.target_directory"));
        if (!ViewModel.IsDirectoryPathValid(GetJobBuilder().Target))
        {
            ShowErrorScreen(L10N.GetTranslation("error.path"));
            return;
        }
        ViewModel.BuildJobCommand.Execute(null);
        GoBack();
    }

    protected override void DisplayModifyJobMenu()
    {
        string jobName = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(L10N.GetTranslation("job_modify_menu.title"))
            .AddChoices(
                ViewModel.JobManager.GetJobs().Select(job => job.Name).ToArray().Append(L10N.GetTranslation("main.go_back"))
            ));
        AnsiConsole.Clear();
        if (jobName != L10N.GetTranslation("main.go_back"))
        {
            AnsiConsole.Write(new Text(L10N.GetTranslation("job_modify_menu.title_modify_page")).Centered());
            AnsiConsole.WriteLine("‎‎‎ "); // some space to breathe ...
            ViewModel.LoadJobInBuilderCommand.Execute(jobName);
            GetJobBuilder().Name = AnsiConsole.Ask<string>(L10N.GetTranslation("job_menu.name_question"), GetJobBuilder().Name);
            if (!ViewModel.IsNameValid(GetJobBuilder().InitialName, false))
            {
                ShowErrorScreen(L10N.GetTranslation("job_menu.error_job_exist"));
                return;
            }
            GetJobBuilder().Source = AnsiConsole.Ask<string>(L10N.GetTranslation("information.source_directory"), GetJobBuilder().Source);
            if (!ViewModel.DoesDirectoryPathExist(GetJobBuilder().Source))
            {
                ShowErrorScreen(L10N.GetTranslation("error.path"));
                return;
            }
            GetJobBuilder().Target = AnsiConsole.Ask<string>(L10N.GetTranslation("information.target_directory"), GetJobBuilder().Target);
            if (!ViewModel.IsDirectoryPathValid(GetJobBuilder().Target))
            {
                ShowErrorScreen(L10N.GetTranslation("error.path"));
                return;
            }
            ViewModel.UpdateFromJobBuilder();
            GoBack();
        }
        GoBack();
    }
    protected override void DisplayDeleteJobMenu()
    {
        string jobName = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Which job do you want to delete?")
            .AddChoices(
                ViewModel.JobManager.GetJobs().Select(job => job.Name).ToArray().Append(L10N.GetTranslation("main.go_back"))
            ));
        if (jobName != L10N.GetTranslation("main.go_back"))
        {
            ViewModel.DeleteJobCommand.Execute(jobName);
        }
        GoBack();
    }

    protected override void DisplayJobResultMenu(int jobNumber)
    {
        AnsiConsole.Clear();
        if (jobNumber == 1)
        {
            AnsiConsole.WriteLine(L10N.GetTranslation("job_menu.one_job_success"));
        }
        else
        {
            AnsiConsole.WriteLine(L10N.GetTranslation("job_menu.many_jobs_success").Replace("{JOBNUMBER}",jobNumber.ToString()));
        }
        AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
        Console.ReadKey();
    }

    protected override void DisplayDailyLogDirectoryMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

        string path = AnsiConsole.Ask<string>(L10N.GetTranslation("settings_menu.message_daily_log_path"), EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogPath.ToString());

        if (!ViewModel.IsDirectoryPathValid(path))
        {
            AnsiConsole.WriteLine(L10N.GetTranslation("error.path"));
            AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
            Console.ReadKey();
            GoBack();
            return;
        }

        ViewModel.DailyLogPath = path;

        GoBack();
    }

    protected override void DisplayStatusLogDirectoryMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());

        string path = AnsiConsole.Ask<string>(L10N.GetTranslation("settings_menu.message_status_log_path"), EasySaveCore.Utilities.Logger<BackupJob>.Get().StatusLogPath.ToString());

        if (!ViewModel.IsDirectoryPathValid(path))
        {
            AnsiConsole.WriteLine(L10N.GetTranslation("error.path"));
            AnsiConsole.Write(L10N.GetTranslation("main.click_any"));
            Console.ReadKey();
            GoBack();
            return;
        }

        ViewModel.StatusLogPath = path;

        GoBack();
    }
    protected override void DisplaySettingsMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
        string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(L10N.GetTranslation("settings_menu.title"))
            .AddChoices(
                L10N.GetTranslation("settings_menu.change_language"),
                L10N.GetTranslation("settings_menu.change_log_type").Replace("{LOGTYPE}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat.ToString()),
                L10N.GetTranslation("settings_menu.change_daily_log_path").Replace("{PATH}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogPath.ToString()),
                L10N.GetTranslation("settings_menu.change_status_log_path").Replace("{PATH}", EasySaveCore.Utilities.Logger<BackupJob>.Get().StatusLogPath.ToString()),
                L10N.GetTranslation("main.go_back")
            ));

        if (choice == L10N.GetTranslation("settings_menu.change_language"))
        {
            AddToMenuHistory(Menu.Language);
            DisplayLanguageMenu();
        }
        else if (choice == L10N.GetTranslation("settings_menu.change_log_type").Replace("{LOGTYPE}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat.ToString()))
        {
            AddToMenuHistory(Menu.LogType);
            DisplayLogTypeMenu();
        }
        else if (choice == L10N.GetTranslation("settings_menu.change_daily_log_path").Replace("{PATH}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogPath.ToString()))
        {
            AddToMenuHistory(Menu.DailyLogDirectory);
            DisplayDailyLogDirectoryMenu();
        }
        else if (choice == L10N.GetTranslation("settings_menu.change_status_log_path").Replace("{PATH}", EasySaveCore.Utilities.Logger<BackupJob>.Get().StatusLogPath.ToString()))
        {
            AddToMenuHistory(Menu.StatusLogDirectory);
            DisplayStatusLogDirectoryMenu();
        }
        GoBack();
    }

    private void Exit()
    {
        AnsiConsole.Write(L10N.GetTranslation("main.exiting"));
        Thread.Sleep(1000);
        EasySaveCore.Utilities.Logger<BackupJob>.Log(LogLevel.Information, "Quitting EasySave-CLEA..." + Environment.NewLine);
        AnsiConsole.Clear();
        Environment.Exit(0);
    }

    public void ShowErrorScreen(string error) 
    {
        AnsiConsole.Write(new Markup($"[red]{error}[/]"));
        AnsiConsole.Write(Environment.NewLine + L10N.GetTranslation("main.click_any"));
        Console.ReadKey();
        GoBack();
    }

    /// <summary>
    /// Remove current menu from the menu History and go to the one before
    /// Note that only menus with submenus are possible values for the switch case
    /// </summary>
    private void GoBack()
    {
        menuHistory.RemoveAt(menuHistory.Count - 1);
        Menu target = menuHistory.Last();
        switch (target)
        {
            case Menu.Main:
                DisplayMainMenu();
                break;
            case Menu.Job:
                DisplayJobMenu();
                break;
            case Menu.Settings:
                DisplaySettingsMenu();
                break;
            default:
                throw new NotImplementedException();
        }
    }

}

public class Program
{
    public static void Main(string[] args) { new EasySaveCli(); }
}