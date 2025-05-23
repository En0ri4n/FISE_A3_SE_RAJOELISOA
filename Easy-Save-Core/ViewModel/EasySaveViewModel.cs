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
using FolderBrowserEx;
using System.Windows.Forms;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        public ICommand ShowFolderDialogCommand { get; }
        public ICommand ResetFolderLogPathCommand { get; }
        public ICommand AddProcessToBlacklistCommand { get; }
        public ICommand RemoveProcessToBlacklistCommand { get; }
        public ICommand AddExtensionToEncryptCommand { get; }
        public ICommand RemoveExtensionToEncryptCommand { get; }


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

            ShowFolderDialogCommand = new RelayCommand((input) =>
            {
                bool isDailyLog = bool.Parse((string)input);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                string title = "Select Status Log Folder";

                folderBrowserDialog.Title = title;
                string path = StatusLogPath;

                if (isDailyLog) {
                    folderBrowserDialog.Title = "Select Daily Log Folder";
                    path = DailyLogPath;
                }

                string fullPath = Path.IsPathRooted(path) ? path
                : Path.GetFullPath(Path.Combine(".", path));

                folderBrowserDialog.InitialFolder = fullPath;
                folderBrowserDialog.AllowMultiSelect = false;

                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;

                if (isDailyLog)
                {
                    DailyLogPath = folderBrowserDialog.SelectedFolder;
                }
                else
                {
                    StatusLogPath = folderBrowserDialog.SelectedFolder;
                }
            }, _ => true);

            ResetFolderLogPathCommand = new RelayCommand((input) =>
            {
                bool isDailyLogPath = bool.Parse((string)input);

                string path = @"logs\";

                if (isDailyLogPath)
                {
                    path += @"daily\";
                    DailyLogPath = path;
                }
                else
                {
                    path += @"status\"; 
                    StatusLogPath = path;
                }
            }, _ => true);

            AddExtensionToEncryptCommand = new RelayCommand((input) =>
            {
                string extension = (input as string)?.Trim();

                if (string.IsNullOrEmpty(extension))
                    return;

                if (!System.Text.RegularExpressions.Regex.IsMatch(extension, @"^\.[\w]+$"))
                    return;

                if (!ExtensionsToEncrypt.Contains(extension))
                {
                    ExtensionsToEncrypt.Add(extension);
                }
            }, _ => true);

            RemoveExtensionToEncryptCommand = new RelayCommand((input) =>
            {
                string extensionToRemove = (input as string);
                if (extensionToRemove != null && ExtensionsToEncrypt.Contains(extensionToRemove))
                {
                    ExtensionsToEncrypt.Remove(extensionToRemove);
                }
            }, _ => true);

            AddProcessToBlacklistCommand = new RelayCommand((input) =>
            {
                string process = (input as string)?.Trim();

                if (string.IsNullOrEmpty(process))
                    return;

                if (!System.Text.RegularExpressions.Regex.IsMatch(process, @"^[\w\-]+\.[\w\-]+$"))
                    return;

                if (!ProcessesToBlacklist.Contains(process))
                {
                    ProcessesToBlacklist.Add(process);
                }
            }, _ => true);

            RemoveProcessToBlacklistCommand = new RelayCommand((input) =>
            {
                string processToRemove = (input as string);
                if (processToRemove != null && ProcessesToBlacklist.Contains(processToRemove))
                {
                    ProcessesToBlacklist.Remove(processToRemove);
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

        // Extensions to Encrypt
        public ObservableCollection<string> ExtensionsToEncrypt
        {
            get => EasySaveConfiguration<TJob>.Get().ExtensionsToEncrypt;
            set
            {
                EasySaveConfiguration<BackupJob>.Get().ExtensionsToEncrypt = value;
                OnPropertyChanged();
            }
        }

        private string _newExtension;
        public string NewExtension
        {
            get => _newExtension;
            set
            {
                _newExtension = value;
                OnPropertyChanged();
            }
        }

        // Processes to Blacklist
        public ObservableCollection<string> ProcessesToBlacklist
        {
            get => EasySaveConfiguration<TJob>.Get().ProcessesToBlacklist;
            set
            {
                EasySaveConfiguration<BackupJob>.Get().ProcessesToBlacklist = value;
                OnPropertyChanged();
            }
        }

        private string _newProcess;
        public string NewProcess
        {
            get => _newProcess;
            set
            {
                _newProcess = value;
                OnPropertyChanged();
            }
        }
    }
}