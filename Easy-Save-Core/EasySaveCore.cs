using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Translations;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Server;
using Microsoft.Extensions.Logging;

namespace CLEA.EasySaveCore.Core
{
    public class EasySaveCore
    {
        public const string Name = "EasySave-CLEA";
        public static readonly Version Version = new Version(3, 0, 0);

        private static EasySaveCore _instance;
        public NetworkServer NetworkServer { get; }
        
        public EasySaveConfigurationBase Configuration { get; private set; }
        public JobManager JobManager { get; private set; }
        public EasySaveViewModelBase EasySaveViewModelBase { get; private set; }

        private EasySaveCore(EasySaveViewModelBase easySaveViewModelBase, JobManager jobManager,
            EasySaveConfigurationBase configuration)
        {
            _instance = this;
            // Initialize the server
            NetworkServer = new NetworkServer(jobManager);
            NetworkServer.Start();
            Configuration = configuration;
            JobManager = jobManager;
            EasySaveViewModelBase = easySaveViewModelBase;
            
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

        public static EasySaveCore Init(
            EasySaveViewModelBase easySaveViewModelBase, JobManager jobManager,
            EasySaveConfigurationBase configuration)
        {
            if (ProcessHelper.GetProcessCount(Process.GetCurrentProcess().ProcessName) > 1)
            {
                MessageBox.Show(
                    L10N.Get().GetTranslation("message_box.process_already_running.text"),
                    L10N.Get().GetTranslation("message_box.process_already_running.title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Environment.Exit(1);
            }

            if (_instance != null)
                throw new InvalidOperationException("EasySaveCore is already initialized.");

            return new EasySaveCore(easySaveViewModelBase, jobManager, configuration);
        }

        public static EasySaveCore Get()
        {
            return _instance;
        }
    }
}