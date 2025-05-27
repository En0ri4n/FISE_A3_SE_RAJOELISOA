using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;
using CLEA.EasySaveCore.External;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;
using System.Windows;
using Microsoft.Extensions.Logging;
using MessageBox = System.Windows.MessageBox;

namespace EasySaveCore.Jobs.Backup.ViewModels
{
    public class BackupJobViewModel : EasySaveViewModelBase<BackupJob, BackupJobManager>
    {
        private static readonly BackupJobViewModel Instance = new BackupJobViewModel();
        public static BackupJobViewModel Get() => Instance;


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
        
        public string StatusLogFilePath => Logger.Get().GetStatusLogFilePath();
        public string DailyLogFilePath => Logger.Get().GetDailyLogFilePath();


        public ObservableCollection<BackupJob> AvailableJobs => JobManager.GetJobs();

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

        private string _tempEncryptionKey;
        public string TempEncryptionKey
        {
            get => _tempEncryptionKey;
            set
            {
                _tempEncryptionKey = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ExtensionsToEncrypt => BackupJobConfiguration.Get().ExtensionsToEncrypt;
        public ObservableCollection<string> ProcessesToBlacklist => BackupJobConfiguration.Get().ProcessesToBlacklist;

        private string _newExtensionToEncrypt;
        public string NewExtensionToEncrypt
        {
            get => _newExtensionToEncrypt;
            set
            {
                _newExtensionToEncrypt = value;
                OnPropertyChanged();
            }
        }

        private string _newProcessToBlacklist;
        public string NewProcessToBlacklist
        {
            get => _newProcessToBlacklist;
            set
            {
                _newProcessToBlacklist = value;
                OnPropertyChanged();
            }
        }

        public bool CanJobBeRun => !JobManager.IsRunning;

        public ICommand BuildJobCommand { get; set; }
        public ICommand SelectedJobCommand { get; set; }
        public ICommand LoadJobInBuilderCommand { get; set; }
        public ICommand DeleteJobCommand { get; set; }
        public ICommand RunJobCommand { get; set; }
        public ICommand RunMultipleJobsCommand { get; set; }
        public ICommand RunAllJobsCommand { get; set; }
        public ICommand ChangeRunStrategyCommand { get; set; }
        public ICommand ShowFolderDialogCommand { get; set; }
        public ICommand ResetFolderLogPathCommand { get; set; }
        public ICommand AddProcessToBlacklistCommand { get; set; }
        public ICommand RemoveProcessToBlacklistCommand { get; set; }
        public ICommand AddExtensionToEncryptCommand { get; set; }
        public ICommand RemoveExtensionToEncryptCommand { get; set; }
        public ICommand LoadEncryptionKeyCommand { get; set; }
        public ICommand SaveEncryptionKeyCommand { get; set; }

        public Action CloseAction { get; set; }
        //public Action DeactivateButtons { get; set; }
        //public Action ReactivateButtons { get; set; }

        protected override void InitializeCommand()
        {
            _tempEncryptionKey = BackupJobConfiguration.Get().EncryptionKey;
            JobManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(JobManager.IsRunning))
                    OnPropertyChanged(nameof(CanJobBeRun));
            };
            JobManager.MultipleJobCompletedHandler += jobs =>
            {
                if (jobs == null || jobs.Count == 0)
                    return;

                MessageBox.Show(L10N<BackupJob>.Get().GetTranslation($"message_box.jobs_completed.text").Replace("{COUNT}", jobs.Count.ToString()), L10N<BackupJob>.Get().GetTranslation($"message_box.jobs_completed.title"), MessageBoxButton.OK, MessageBoxImage.Information);
            };

            BuildJobCommand = new RelayCommand(isCreation =>
            {
                bool isJobCreation = bool.Parse((string)isCreation!);
                
                if (JobBuilder == null)
                    throw new NullReferenceException("BackupJob builder is not defined !");

                if (string.IsNullOrWhiteSpace(GetJobBuilder().Name) ||
                     string.IsNullOrWhiteSpace(GetJobBuilder().Source) ||
                     string.IsNullOrWhiteSpace(GetJobBuilder().Target))
                {
                    MessageBox.Show(L10N<BackupJob>.Get().GetTranslation($"message_box.missing_data.text"), L10N<BackupJob>.Get().GetTranslation($"message_box.missing_data.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (isJobCreation)
                {
                    if (!JobManager.AddJob(SelectedJob = JobBuilder.Build(false), true))
                    {
                        MessageBox.Show(L10N<BackupJob>.Get().GetTranslation($"message_box.existing_job.text"), L10N<BackupJob>.Get().GetTranslation($"message_box.existing_job.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                    if (!JobManager.UpdateJob(JobBuilder.InitialName, JobBuilder.Build(false)))
                    {
                        MessageBox.Show(L10N<BackupJob>.Get().GetTranslation($"message_box.existing_job.text"), L10N<BackupJob>.Get().GetTranslation($"message_box.existing_job.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                CloseAction();
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
                }
            }, _ => true); //Todo utiliser commandes pour désactiver boutons

            RunJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                {
                    JobManager.DoJob(name);
                }
            }, _ => true);

            RunMultipleJobsCommand = new RelayCommand(jobNameList =>
            {
                if (!(jobNameList is List<string> jobNames))
                    return;
                
                if (jobNames.Count == 0)
                {
                    MessageBox.Show(L10N<BackupJob>.Get().GetTranslation($"message_box.run_no_selected.text"), L10N<BackupJob>.Get().GetTranslation($"message_box.run_no_selected.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                List<BackupJob> jobs = jobNames.Select(name => JobManager.GetJob(name)).ToList();
                
                if (!ExternalEncryptor.IsEncryptorPresent() && jobs.Any(job => job.IsEncrypted && BackupJobConfiguration.Get().ExtensionsToEncrypt.Any()))
                {
                    Logger.Log(LogLevel.Warning, "'CLEA-Encryptor.exe' not found. Encryption will not be performed.");
                    MessageBox.Show(L10N<BackupJob>.Get().GetTranslation($"message_box.cant_find_encryptor.text"), L10N<BackupJob>.Get().GetTranslation($"message_box.cant_find_encryptor.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                //TODO Deactivate all buttons that can impact job running (.e.g: Delete Button, Create Job Button, Settings Button, Run Job Button (to see)
                // deactivateButtons()
                JobManager.DoMultipleJob(jobNames);
                // reactivateButtons()
            }, _ => !JobManager.IsRunning);

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

            ShowFolderDialogCommand = new RelayCommand(input =>
            {
                bool isDailyLog = bool.Parse((string)input!);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                string title = L10N<BackupJob>.Get().GetTranslation("browse_folder.status_log");

                folderBrowserDialog.Title = title;
                string path = StatusLogPath;

                if (isDailyLog) {
                    folderBrowserDialog.Title = L10N<BackupJob>.Get().GetTranslation("browse_folder.daily_log"); ;
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

            ResetFolderLogPathCommand = new RelayCommand(input =>
            {
                bool isDailyLogPath = bool.Parse((string)input!);

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

            LoadEncryptionKeyCommand = new RelayCommand(input =>
            {
                TempEncryptionKey = ExternalEncryptor.GetEncryptionKey();
            });

            SaveEncryptionKeyCommand = new RelayCommand(input =>
            {
                string encryptionKey = (input as string)?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(encryptionKey) || encryptionKey.Length < 8 || encryptionKey.Length > 30)
                {
                    MessageBox.Show(L10N<BackupJob>.Get().GetTranslation("message_box.encryption_key_invalid.text"), L10N<BackupJob>.Get().GetTranslation("message_box.encryption_key_invalid.title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                BackupJobConfiguration.Get().EncryptionKey = ExternalEncryptor.ProcessEncryptionKey(encryptionKey);
                MessageBox.Show(L10N<BackupJob>.Get().GetTranslation("message_box.encryption_key_valid.text"), L10N<BackupJob>.Get().GetTranslation("message_box.encryption_key_valid.title"), MessageBoxButton.OK, MessageBoxImage.Information);
            });

            AddExtensionToEncryptCommand = new RelayCommand(input =>
            {
                string extension = (input as string)?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(extension))
                    return;

                if (!Regex.IsMatch(extension, @"^\.[\w]+$"))
                    return;

                if (!ExtensionsToEncrypt.Contains(extension))
                {
                    ExtensionsToEncrypt.Add(extension);
                    NewExtensionToEncrypt = string.Empty;
                }
            }, _ => true);

            RemoveExtensionToEncryptCommand = new RelayCommand(input =>
            {
                string extensionToRemove = (input as string)?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(extensionToRemove) && ExtensionsToEncrypt.Contains(extensionToRemove))
                {
                    ExtensionsToEncrypt.Remove(extensionToRemove);
                }
            }, _ => true);

            AddProcessToBlacklistCommand = new RelayCommand(input =>
            {
                string process = (input as string)?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(process))
                    return;

                if (!Regex.IsMatch(process, @"^[\w\-]+$"))
                    return;

                if (!ProcessesToBlacklist.Contains(process))
                {
                    ProcessesToBlacklist.Add(process);
                    NewProcessToBlacklist = string.Empty;
                }
            }, _ => true);

            RemoveProcessToBlacklistCommand = new RelayCommand(input =>
            {
                string processToRemove = input as string ?? string.Empty;
                if (!string.IsNullOrEmpty(processToRemove) && ProcessesToBlacklist.Contains(processToRemove))
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

        public ViewModelBackupJobBuilder GetJobBuilder()
        {
            return (ViewModelBackupJobBuilder) JobBuilder;
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