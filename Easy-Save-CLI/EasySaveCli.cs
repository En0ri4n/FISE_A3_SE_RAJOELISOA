using CLEA.EasySaveCore.L10N;
using Spectre.Console;

namespace CLEA.EasySaveCLI;

public class EasySaveCli
{
    public EasySaveCli()
    {
        DisplayMainMenu();
    }
    
    private void DisplayMainMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Text(L10N.Get().GetTranslation("main.title")).Centered());

        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(L10N.Get().GetTranslation("main_menu.title"))
                .AddChoices(
                    L10N.Get().GetTranslation("main_menu.jobs"),
                    L10N.Get().GetTranslation("main_menu.change_language"),
                    L10N.Get().GetTranslation("main_menu.exit")
                ));

        if (choice == L10N.Get().GetTranslation("main_menu.jobs"))
        {
            AnsiConsole.MarkupLine("[bold yellow]Jobs menu not implemented yet[/]");
        }
        else if(choice == L10N.Get().GetTranslation("main_menu.change_language"))
        {
            AnsiConsole.MarkupLine("[bold yellow]Change language menu not implemented yet[/]");
        }
        else if(choice == L10N.Get().GetTranslation("main_menu.exit"))
        {
            Exit();
        }
    }
    
    private void Exit()
    {
        AnsiConsole.Write(L10N.Get().GetTranslation("main.exiting"));
        Thread.Sleep(1000);
        Environment.Exit(0);
    }
    
    public static void Main(string[] args) { new EasySaveCli(); }
}