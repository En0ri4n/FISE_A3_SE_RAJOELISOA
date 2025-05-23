using System;
using System.Text;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore
{
    public class EasySaveCore<TJob, TJobManager, TConfiguration> where TJob : IJob where TJobManager : JobManager<TJob> where TConfiguration : EasySaveConfigurationBase
    {
        public static readonly Version Version = new Version(2, 0, 0);
        public const string Name = "EasySave-CLEA";

        private static EasySaveCore<TJob, TJobManager, TConfiguration> _instance;

        private EasySaveCore(EasySaveViewModelBase<TJob, TJobManager> easySaveViewModelBase, TJobManager jobManager, TConfiguration configuration)
        {
            // Initialize the view model with the job manager
            easySaveViewModelBase.InitializeViewModel(jobManager);

            // Load the configuration first, so everything is set up correctly
            // before we start logging.
            configuration.LoadConfiguration();

            // Set the console output encoding to Unicode
            // This is important for displaying Unicode characters correctly
            // in the console, especially for languages with special characters
            // like emojis or non-Latin scripts.
            Console.OutputEncoding = Encoding.Unicode;

            Logger.Log(LogLevel.Information, "EasySave-CLEA started");
        }

        public static EasySaveCore<TJob, TJobManager, TConfiguration> Init(EasySaveViewModelBase<TJob, TJobManager> easySaveViewModelBase, TJobManager jobManager, TConfiguration configuration)
        {
            if (_instance != null)
                throw new InvalidOperationException("EasySaveCore is already initialized.");

            return _instance = new EasySaveCore<TJob, TJobManager, TConfiguration>(easySaveViewModelBase, jobManager, configuration);
        }

        public static EasySaveCore<TJob, TJobManager, TConfiguration> Get()
        {
            return _instance;
        }
    }
}