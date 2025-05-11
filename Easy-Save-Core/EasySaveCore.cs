using System.Text;
using CLEA.EasySaveCore.Utilities;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore;

public class EasySaveCore
{
    public static readonly Version Version = new(1, 0, 0);
    public const string Name = "EasySave-CLEA";

    private static readonly EasySaveCore Instance = new EasySaveCore();

    private EasySaveCore()
    {
        // Load the configuration first, so everything is set up correctly
        // before we start logging.
        EasySaveConfiguration.LoadConfiguration();
        
        // Set the console output encoding to Unicode
        // This is important for displaying Unicode characters correctly
        // in the console, especially for languages with special characters
        // like emojis or non-Latin scripts.
        Console.OutputEncoding = Encoding.Unicode;

        Logger.Log(LogLevel.Information, "EasySave-CLEA started");
    }

    public static EasySaveCore Get()
    {
        return Instance;
    }

    public static void Main(string[] args) { throw new NotImplementedException("Main method not implemented, EasySave-Core can't be started in standalone !"); }
}