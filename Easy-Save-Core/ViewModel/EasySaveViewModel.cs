using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Linq;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Models;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;

namespace CLEA.EasySaveCore.ViewModel
{
    public class EasySaveViewModel<TJob> : INotifyPropertyChanged where TJob : IJob
    {
        public readonly JobManager<TJob> JobManager;

        public ViewModelJobBuilder<TJob> JobBuilder;
        public readonly ICommand BuildJobCommand;

        // Languages
        public List<LangIdentifier> AvailableLanguages => Languages.SupportedLangs;

        public LangIdentifier CurrentApplicationLang
        {
            get => L10N<TJob>.Get().GetLanguage();
            set
            {
                L10N<TJob>.Get().SetLanguage(value);
                EasySaveConfiguration<BackupJob>.SaveConfiguration();
                OnPropertyChanged();
            }
        }

        // Daily Logs Formats
        public List<Format> AvailableDailyLogFormats => Enum.GetValues(typeof(Format)).Cast<Format>().ToList();

        public Format CurrentDailyLogFormat
        {
            get => Logger.Get().DailyLogFormat;
            set
            {
                Logger.Get().DailyLogFormat = value;
                EasySaveConfiguration<BackupJob>.SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string DailyLogPath
        {
            get => Logger.Get().DailyLogPath;
            set
            {
                Logger.Get().DailyLogPath = value;
                EasySaveConfiguration<BackupJob>.SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string StatusLogPath
        {
            get => Logger.Get().StatusLogPath;
            set
            {
                Logger.Get().StatusLogPath = value;
                EasySaveConfiguration<BackupJob>.SaveConfiguration();
                OnPropertyChanged();
            }
        }


        public List<TJob> AvailableJobs => JobManager.GetJobs();

        public ICommand SelectedJobCommand;
        private TJob _selectedJob;

        public TJob SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand LoadJobInBuilderCommand;

        private string _userInput;

        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeleteJobCommand;
        public ICommand RunJobCommand;
        public ICommand RunMultipleJobsCommand;
        public ICommand RunAllJobsCommand;
        public ICommand ChangeRunStrategyCommand;

        private static EasySaveViewModel<TJob> _instance;

        private EasySaveViewModel(JobManager<TJob> jobManager)
        {
            JobManager = jobManager;

            BuildJobCommand = new RelayCommand(_ =>
            {
                if (JobBuilder == null)
                    throw new NullReferenceException($"Job Builder for <{typeof(TJob)}> is not defined !");

                JobManager.AddJob(SelectedJob = JobBuilder.Build(), true);
                EasySaveConfiguration<TJob>.SaveConfiguration();
            }, _ => true);

            SelectedJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                    SelectedJob = JobManager.GetJob(name);
            }, _ => true);

            LoadJobInBuilderCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                    JobBuilder?.GetFrom(JobManager.GetJob(name));
            }, _ => true);

            DeleteJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                {
                    JobManager.RemoveJob(name);
                    EasySaveConfiguration<TJob>.SaveConfiguration();
                }
            }, _ => true);

            RunJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                {
                    JobManager.DoJob(name);
                }
            }, _ => true);

            RunMultipleJobsCommand = new RelayCommand(jobNameList =>
            {
                if (jobNameList is List<string> jobNames)
                {
                    JobManager.DoMultipleJob(jobNames);
                }
            }, _ => true);

            ChangeRunStrategyCommand = new RelayCommand(strategy =>
            {
                if (strategy is string strategyName)
                {
                    JobManager.Strategy = strategyName switch
                    {
                        "Full" => StrategyType.Full,
                        "Differential" => StrategyType.Differential,
                        _ => throw new NotImplementedException()
                    };
                }
            }, _ => true);

            RunAllJobsCommand = new RelayCommand(_ => { JobManager.DoAllJobs(); }, _ => true);
        }

        public void OnTaskCompletedFor(string[] jobNames, IJob.TaskCompletedDelegate callback)
        {
            foreach (var jobName in jobNames)
            {
                var job = JobManager.GetJob(jobName);
                if (job == null)
                    continue;

                job.ClearTaskCompletedHandler();
                job.TaskCompletedHandler += callback;
            }
        }

        public void SetJobBuilder(ViewModelJobBuilder<TJob> jobBuilder)
        {
            JobBuilder = jobBuilder;
        }

        public bool DoesDirectoryPathExist(string path)
        {
            return Directory.Exists(path);
        }

        public bool IsDirectoryPathValid(string path)
        {
            try
            {
                string fullPath = Path.GetFullPath(path);

                return !Path.HasExtension(fullPath);
            }
            catch
            {
                return false;
            }
        }


        public bool IsNameValid(string name, bool isCreation)
        {
            TJob existingJob = JobManager.GetJob(name);
            bool exists = existingJob != null;

            return isCreation ? !exists : exists;
        }

        public void UpdateFromJobBuilder()
        {
            JobManager.UpdateJob(JobBuilder.InitialName, JobBuilder.Build());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static EasySaveViewModel<TJob> Get()
        {
            if (_instance == null)
                throw new Exception("EasySaveViewModel not initialized");

            return _instance;
        }

        public static void Init(JobManager<TJob> jobManager)
        {
            if (_instance != null)
                throw new Exception("EasySaveViewModel already initialized");

            _instance = new EasySaveViewModel<TJob>(jobManager);
        }
    }
}