using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore;

public class EasySaveCore
{
    public static readonly Version Version = new(1, 0, 0);
    private static readonly EasySaveCore Instance = new();

    private ILogger Logger { get; }

    private EasySaveCore()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "[HH:mm:ss] ";
        }));
        Logger = factory.CreateLogger("EasySave-CLEA");
        Logger.LogInformation("EasySave-CLEA started");
    }

    public static EasySaveCore Get()
    {
        return Instance;
    }

    public static void Main(string[] args) { throw new NotImplementedException("Main method not implemented, EasySave-Core can't be started in standalone !"); }
}