using System.Text;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore;

public class EasySaveCore<TJob> where TJob : IJob
{
    public static readonly Version Version = new Version(1, 0, 0);
    public const string Name = "EasySave-CLEA";

    private static EasySaveCore<TJob> _instance;

    private EasySaveCore(JobManager<TJob> jobManager)
    {
        // Initialize the view model with the job manager
        EasySaveViewModel<TJob>.Init(jobManager);
        
        // Load the configuration first, so everything is set up correctly
        // before we start logging.
        EasySaveConfiguration<TJob>.LoadConfiguration();
        
        // Set the console output encoding to Unicode
        // This is important for displaying Unicode characters correctly
        // in the console, especially for languages with special characters
        // like emojis or non-Latin scripts.
        Console.OutputEncoding = Encoding.Unicode;

        Utilities.Logger<TJob>.Log(LogLevel.Information, "EasySave-CLEA started");
    }

    public static EasySaveCore<TJob> Init(JobManager<TJob> jobManager)
    {
        if(_instance != null)
            throw new InvalidOperationException("EasySaveCore is already initialized.");
        
        return _instance = new EasySaveCore<TJob>(jobManager);
    }

    public static EasySaveCore<TJob> Get()
    {
        return _instance;
    }
}