using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.View;
using Spectre.Console;

namespace CLEA.EasySaveCLI;

public sealed class EasySaveCli : EasySaveView
{
    private EasySaveCli()
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
            FileBrowser.Browser browser = new FileBrowser.Browser();
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
        
        if (selectedLang != L10N.Get().GetLanguage())
        {
            L10N.Get().SetLanguage(selectedLang);
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
        AnsiConsole.Clear();
        Environment.Exit(0);
    }
    
    public static void Main(string[] args) { new EasySaveCli(); }
}