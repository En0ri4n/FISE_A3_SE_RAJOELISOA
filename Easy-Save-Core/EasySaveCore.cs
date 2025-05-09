using System.Text;
using CLEA.EasySaveCore.Utilities;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore;

public class EasySaveCore
{
    public static readonly Version Version = new(1, 0, 0);
    public static readonly string Name = "EasySave-CLEA";

    public static ILogger Logger;
    private static readonly EasySaveCore Instance = new EasySaveCore();


    private EasySaveCore()
    {
        Console.OutputEncoding = Encoding.Unicode;
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "[HH:mm:ss] ";
        }));
        Logger = factory.CreateLogger(Name);
        Logger.LogInformation("EasySave-CLEA initialized");
        
        EasySaveConfiguration.LoadConfiguration();
        
        Logger.LogInformation("EasySave-CLEA started");
    }

    public static EasySaveCore Get()
    {
        return Instance;
    }

    public static void Main(string[] args) { throw new NotImplementedException("Main method not implemented, EasySave-Core can't be started in standalone !"); }
}