using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.View;
using Spectre.Console;
using System.ComponentModel;
using System.Linq.Expressions;

namespace CLEA.EasySaveCLI;

public sealed class EasySaveCli : EasySaveView
{
    public List<string> menuStack = new List<string>();

    private void AddMenuToStack(string menuName)
    {
        menuStack.Add(menuName);
    }
    private EasySaveCli()
    {
        AddMenuToStack("Main");
        DisplayMainMenu();
    }
    
    protected override void DisplayMainMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("main_menu.title"))
                .AddChoices(
                    L10N.GetTranslation("main_menu.jobs"),
                    L10N.GetTranslation("main_menu.change_language"),
                    L10N.GetTranslation("main_menu.exit")
                ));

        if (choice == L10N.GetTranslation("main_menu.jobs"))
        {
            AddMenuToStack("Job");
            DisplayJobMenu();
        }
        else if(choice == L10N.GetTranslation("main_menu.change_language"))
        {
            AddMenuToStack("Language"); 
            DisplayLanguageMenu();
        }
        else if(choice == L10N.GetTranslation("main_menu.exit"))
        {
            Exit();
        }
    }
    
    protected override void DisplayJobMenu()
    {
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
            throw new NotImplementedException();
        }
        else if (choice == L10N.GetTranslation("job_menu.create_job"))
        {
            throw new NotImplementedException();
        }
        else if (choice == L10N.GetTranslation("job_menu.modify_job"))
        {
            throw new NotImplementedException();
        }
        else if (choice == L10N.GetTranslation("job_menu.delete_job"))
        {
            throw new NotImplementedException();
        }
        else if (choice == L10N.GetTranslation("go_back"))
        {
            GoBack();
        }
        //Add for selecting Directory
        //TODO : Add exit option to file explorer (maybe in another branch)
        /*FileBrowser.Browser browser = new FileBrowser.Browser();
        browser.GetFolderPath().RunSynchronously();*/
        //throw new NotImplementedException();
    }

    protected override void DisplayLanguageMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.GetTranslation("main.title")).Centered());
        
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.GetTranslation("language_menu.title"))
                .AddChoices(
                    Languages.SupportedLangs.Select(li => li.Name).ToArray()
                ));
        
        LangIdentifier selectedLang = Languages.SupportedLangs.First(li => li.Name == choice);
        
        if (selectedLang != L10N.Get().GetLanguage())
        {
            L10N.Get().SetLanguage(selectedLang);
        }
        GoBack();
    }
    
    protected override void DisplayJobResultMenu()
    {
        AddMenuToStack("JobResult");
        throw new NotImplementedException();
    }
    
    protected override void DisplayJobSettingsMenu()
    {
        AddMenuToStack("JobSetting");
        throw new NotImplementedException();
    }
    
    private void Exit()
    {
        AnsiConsole.Write(L10N.GetTranslation("main.exiting"));
        Thread.Sleep(1000);
        AnsiConsole.Clear();
        Environment.Exit(0);
    }

    private void GoBack()
    /// <summary>
    /// Remove current menu from the menu Stack and go to the one before
    /// </summary>
    {
        menuStack.RemoveAt(menuStack.Count - 1);
        string target = menuStack.Last();
        switch (target)
        {
            case "Main":
                DisplayMainMenu();
                break;
            case "Job":
                DisplayJobMenu();
                break;
            case "JobResult":
                DisplayJobResultMenu();
                break;
            case "JobSetting":
                DisplayJobSettingsMenu();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void Main(string[] args) { new EasySaveCli(); }
}