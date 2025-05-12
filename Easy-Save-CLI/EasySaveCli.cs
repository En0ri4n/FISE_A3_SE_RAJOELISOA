using CLEA.EasySaveCore;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.View;
using EasySaveCore.Models;
using FileBrowser;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace CLEA.EasySaveCLI;

public sealed class EasySaveCli : EasySaveView<BackupJob>
{
    public EasySaveCli() : base(EasySaveCore<BackupJob>.Init(new BackupJobManager()))
    {
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
            Browser browser = new Browser();
            browser.GetFolderPath().RunSynchronously();
            
            
            DisplayJobMenu();
        }
        else if(choice == L10N.GetTranslation("main_menu.change_language"))
        {
            DisplayLanguageMenu();
        }
        else if(choice == L10N.GetTranslation("main_menu.exit"))
        {
            Exit();
        }
    }
    
    protected override void DisplayJobMenu()
    {
        throw new NotImplementedException();
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
        
        if (selectedLang != L10N.GetLanguage())
        {
            L10N.SetLanguage(selectedLang);
        }
        
        DisplayMainMenu();
    }
    
    protected override void DisplayJobResultMenu()
    {
        throw new NotImplementedException();
    }
    
    protected override void DisplayJobSettingsMenu()
    {
        throw new NotImplementedException();
    }
    
    private void Exit()
    {
        AnsiConsole.Write(L10N.GetTranslation("main.exiting"));
        Thread.Sleep(1000);
        EasySaveCore.Utilities.Logger<BackupJob>.Log(LogLevel.Information, "Quitting EasySave-CLEA..." + Environment.NewLine);
        AnsiConsole.Clear();
        Environment.Exit(0);
    }
}

public class Program
{
    public static void Main(string[] args) { new EasySaveCli(); }
}