using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;

namespace EasySaveCore.Jobs.Backup.ViewModels
{
    public class BackupJobViewModel : EasySaveViewModelBase<BackupJob, BackupJobManager>
    {
        private static readonly BackupJobViewModel Instance = new BackupJobViewModel();
        public static BackupJobViewModel Get() => Instance;

        public ICommand BuildJobCommand;
        public ICommand SelectedJobCommand;
        public RelayCommand LoadJobInBuilderCommand;

        public ICommand DeleteJobCommand;
        public ICommand RunJobCommand;
        public ICommand RunMultipleJobsCommand;
        public ICommand RunAllJobsCommand;
        public ICommand ChangeRunStrategyCommand;

        // Languages
        public List<LangIdentifier> AvailableLanguages => Languages.SupportedLangs;

        public LangIdentifier CurrentApplicationLang
        {
            get => L10N<BackupJob>.Get().GetLanguage();
            set
            {
                L10N<BackupJob>.Get().SetLanguage(value);
                BackupJobConfiguration.Get().SaveConfiguration();
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
                BackupJobConfiguration.Get().SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string DailyLogPath
        {
            get => Logger.Get().DailyLogPath;
            set
            {
                Logger.Get().DailyLogPath = value;
                BackupJobConfiguration.Get().SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string StatusLogPath
        {
            get => Logger.Get().StatusLogPath;
            set
            {
                Logger.Get().StatusLogPath = value;
                BackupJobConfiguration.Get().SaveConfiguration();
                OnPropertyChanged();
            }
        }


        public List<BackupJob> AvailableJobs => JobManager.GetJobs();

        private BackupJob _selectedJob;

        public BackupJob SelectedJob
        {
            get => _selectedJob;
            set
            {
                _selectedJob = value;
                OnPropertyChanged();
            }
        }


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

        protected override void InitializeCommand()
        {
            BuildJobCommand = new RelayCommand(_ =>
            {
                if (JobBuilder == null)
                    throw new NullReferenceException($"BackupJob builder is not defined !");

                JobManager.AddJob(SelectedJob = JobBuilder.Build(), true);
                BackupJobConfiguration.Get().SaveConfiguration();
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
                    BackupJobConfiguration.Get().SaveConfiguration();
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
                        "Full" => JobExecutionStrategy.StrategyType.Full,
                        "Differential" => JobExecutionStrategy.StrategyType.Differential,
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

        public override event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            BackupJob existingJob = JobManager.GetJob(name);
            bool exists = existingJob != null;

            return isCreation ? !exists : exists;
        }

        public void UpdateFromJobBuilder()
        {
            JobManager?.UpdateJob(JobBuilder?.InitialName, JobBuilder.Build());
        }
    }
}