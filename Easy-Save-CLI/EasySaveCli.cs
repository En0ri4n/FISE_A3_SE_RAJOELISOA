using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.View;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CLEA.EasySaveCLI;

public sealed class EasySaveCli : EasySaveView<BackupJob, ViewModelBackupJobBuilder>
{
    enum Menu
    {
        Main,
        Job,
        JobResult,
        JobSetting,
        Language,
        LogType
    }
    private readonly List<Menu> menuHistory = new List<Menu>();

    private void AddToMenuHistory(Menu menuName)
    {
        menuHistory.Add(menuName);
    }

    public EasySaveCli() : base(EasySaveCore<BackupJob>.Init(new BackupJobManager()), new ViewModelBackupJobBuilder())
    {
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
        /*else if(choice == L10N.GetTranslation("main_menu.change_language"))
        {
            AddToMenuHistory(Menu.Language); 
            DisplayLanguageMenu();
        }
        else if (choice == L10N.GetTranslation("main_menu.change_log_type").Replace("{LOGTYPE}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat.ToString()))
        {
            AddToMenuHistory(Menu.LogType);
            DisplayLogTypeMenu();
        }*/
        else if (choice == L10N.GetTranslation("main_menu.settings"))
        {
            AddToMenuHistory(Menu.Option);
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
    //TODO Check if directory exist and if name is duplicated
    //Check if more than 5 jobs ?
    //TODO Add specific function or rework go_back ? All these "go back" send you to the main menu
    {
        AnsiConsole.Clear();
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("job_menu.title"))
                .AddChoices(
                    L10N.GetTranslation("job_menu.list_job"),
                    L10N.GetTranslation("job_menu.create_job"),
                    L10N.GetTranslation("job_menu.modify_job"),
                    L10N.GetTranslation("job_menu.delete_job"),
                    L10N.GetTranslation("go_back")
                ));
        if (choice == L10N.GetTranslation("job_menu.list_job"))
        {
            AnsiConsole.Write(new Text(L10N.GetTranslation("job_menu.list_job")).Centered());
            Table table = new Table();
            
            table.AddColumns(["Job Name", "Source", "Target"]);
            
            foreach (BackupJob job in ViewModel.JobManager.GetJobs())
                table.AddRow([job.Name, job.Source.Value, job.Target.Value]);
            AnsiConsole.Write(table);
            AnsiConsole.Write(L10N.GetTranslation("click_any"));
            Console.ReadKey();
            GoBack();
        }
        /*job format in config.json
        {
            "jobName" = "myJobName",
            "jobSource" = "path\\to\\base\\directory",
            "jobDestination" = "path\\to\\target\\directory"
        }*/
        else if (choice == L10N.GetTranslation("job_menu.create_job"))
        {
            GetJobBuilder().Name = AnsiConsole.Ask<string>("What is the name of the job?");
            GetJobBuilder().Source = AnsiConsole.Ask<string>("What is the source directory?");
            GetJobBuilder().Target = AnsiConsole.Ask<string>("What is the target directory?");
            
            ViewModel.BuildJobCommand.Execute(null);
            GoBack();
        }
        else if (choice == L10N.GetTranslation("job_menu.modify_job"))
        {
            //TODO JobName should be unique, add a check for that
            throw new NotImplementedException();
        }
        else if (choice == L10N.GetTranslation("job_menu.delete_job"))
        {
            string jobName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Which job do you want to delete?")
                    .AddChoices(
                        ViewModel.JobManager.GetJobs().Select(job => job.Name).ToArray()
                    ));
            ViewModel.DeleteJobCommand.Execute(jobName);
            GoBack();
        }
        else if (choice == L10N.GetTranslation("go_back"))
        {
            GoBack();
        }
        //Add for selecting Directory
        //TODO : Add exit option to file explorer (maybe in another branch)
        
        /*FileBrowser.Browser browser = new FileBrowser.Browser();
        browser.GetFolderPath().RunSynchronously();
        throw new NotImplementedException();*/
    }

    protected override void DisplayLanguageMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
        
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("language_menu.title"))
                .AddChoices(
                    Languages.SupportedLangs.Select(li => li.Name).ToArray().Append(L10N.GetTranslation("go_back"))
                ));
        if (choice != L10N.GetTranslation("go_back"))
        {
            LangIdentifier selectedLang = Languages.SupportedLangs.First(li => li.Name == choice);

            if (selectedLang != L10N.GetLanguage())
            {
                L10N.SetLanguage(selectedLang);
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
                    Format.Xml.ToString(),
                    Format.Json.ToString(),
                    L10N.GetTranslation("go_back")
                ));

        if (choice != L10N.GetTranslation("go_back"))
        {
            //TODO Move to a view model (Inside a view it is so-so)
            EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat = (Format)Enum.Parse(typeof(Format), choice);
            EasySaveConfiguration<BackupJob>.SaveConfiguration();
        }
        GoBack();
    }



    protected override void DisplayJobResultMenu()
    {
        AddToMenuHistory(Menu.JobResult);
        throw new NotImplementedException();
    }

    protected override void DisplaySettingsMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
        string choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title(L10N.GetTranslation("settings_menu.title"))
            .AddChoices(
                L10N.GetTranslation("main_menu.change_language"), //TODO Rename main_menu -> option_menu
                L10N.GetTranslation("main_menu.change_log_type").Replace("{LOGTYPE}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat.ToString()),
                L10N.GetTranslation("main.go_back")
            ));

        if (choice == L10N.GetTranslation("main_menu.change_language"))
        {
            AddToMenuHistory(Menu.Language);
            DisplayLanguageMenu();
        }
        else if (choice == L10N.GetTranslation("main_menu.change_log_type").Replace("{LOGTYPE}", EasySaveCore.Utilities.Logger<BackupJob>.Get().DailyLogFormat.ToString()))
        {
            AddToMenuHistory(Menu.LogType);
            DisplayLogTypeMenu();
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

    /// <summary>
    /// Remove current menu from the menu History and go to the one before
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
            case Menu.JobResult:
                DisplayJobResultMenu();
                break;
            case Menu.Option:
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