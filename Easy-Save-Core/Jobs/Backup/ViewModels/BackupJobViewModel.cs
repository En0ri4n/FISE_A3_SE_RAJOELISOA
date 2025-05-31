using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using CLEA.EasySaveCore.External;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Translations;
using CLEA.EasySaveCore.Utilities;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using Microsoft.Extensions.Logging;
using static CLEA.EasySaveCore.Models.JobExecutionStrategy;
using Application = System.Windows.Application;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;
using MessageBox = System.Windows.MessageBox;

namespace EasySaveCore.Jobs.Backup.ViewModels
{
    public class BackupJobViewModel : EasySaveViewModelBase
    {
        private string _newExtensionToEncrypt;

        private string _newProcessToBlacklist;

        private string _newExtensionToPrioritize;

        private string _tempEncryptionKey;

        private string _tempSimultaneousFileSizeThreshold;


        public override bool IsRunning { get; set; }
        public override event PropertyChangedEventHandler? PropertyChanged;


        // Languages
        public List<LangIdentifier> AvailableLanguages => Languages.SupportedLangs;

        public LangIdentifier CurrentApplicationLang
        {
            get => L10N.Get().GetLanguage();
            set
            {
                L10N.Get().SetLanguage(value);
                Configuration.SaveConfiguration();
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
                Configuration.SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string DailyLogPath
        {
            get => Logger.Get().DailyLogPath;
            set
            {
                Logger.Get().DailyLogPath = value;
                Configuration.SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string StatusLogPath
        {
            get => Logger.Get().StatusLogPath;
            set
            {
                Logger.Get().StatusLogPath = value;
                Configuration.SaveConfiguration();
                OnPropertyChanged();
            }
        }

        public string StatusLogFilePath => Logger.Get().GetStatusLogFilePath();
        public string DailyLogFilePath => Logger.Get().GetDailyLogFilePath();


        public ObservableCollection<IJob> AvailableJobs => JobManager.GetJobs();

        public string TempEncryptionKey
        {
            get => _tempEncryptionKey;
            set
            {
                _tempEncryptionKey = value;
                OnPropertyChanged();
            }
        }

        public string TempSimultaneousFileSizeThreshold
        {
            get => _tempSimultaneousFileSizeThreshold;
            set
            {
                _tempSimultaneousFileSizeThreshold = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ExtensionsToEncrypt => Configuration.ExtensionsToEncrypt;
        public ObservableCollection<string> ProcessesToBlacklist => Configuration.ProcessesToBlacklist;
        public ObservableCollection<string> ExtensionsToPrioritize => Configuration.ExtensionsToPrioritize;


        public string NewExtensionToEncrypt
        {
            get => _newExtensionToEncrypt;
            set
            {
                _newExtensionToEncrypt = value;
                OnPropertyChanged();
            }
        }

        public string NewProcessToBlacklist
        {
            get => _newProcessToBlacklist;
            set
            {
                _newProcessToBlacklist = value;
                OnPropertyChanged();
            }
        }

        public string NewExtensionToPrioritize
        {
            get => _newExtensionToPrioritize;
            set
            {
                _newExtensionToPrioritize = value;
                OnPropertyChanged();
            }
        }


        public bool CanJobBeRun => !JobManager.IsRunning;
        public bool CanJobsBePaused => JobManager.GetJobs().Any(job => job.IsRunning && !JobManager.IsPaused);
        public bool AreJobsPaused => JobManager.IsPaused;

        public ICommand BuildJobCommand { get; private set; }
        public ICommand LoadJobInBuilderCommand { get; private set; }
        public ICommand DeleteJobCommand { get; private set; }
        public ICommand RunJobCommand { get; private set; }
        public ICommand RunMultipleJobsCommand { get; private set; }
        public ICommand RunAllJobsCommand { get; private set; }
        public ICommand ChangeRunStrategyCommand { get; private set; }
        public ICommand ShowFolderDialogCommand { get; set; }
        public ICommand ResetFolderLogPathCommand { get; set; }
        public ICommand AddProcessToBlacklistCommand { get; set; }
        public ICommand RemoveProcessToBlacklistCommand { get; set; }
        public ICommand AddExtensionToEncryptCommand { get; set; }
        public ICommand RemoveExtensionToEncryptCommand { get; set; }
        public ICommand AddExtensionToPrioritizeCommand { get; set; }
        public ICommand RemoveExtensionToPrioritizeCommand { get; set; }

        public ICommand LoadEncryptionKeyCommand { get; set; }
        public ICommand SaveEncryptionKeyCommand { get; set; }
        public ICommand PauseJobsCommand { get; set; }
        public ICommand StopJobsCommand { get; set; }


        public ICommand LoadSimultaneousFileSizeThresholdCommand { get; set; }
        public ICommand SaveSimultaneousFileSizeThresholdCommand { get; set; }


        public Action CloseAction { get; set; }
        
        private BackupJobConfiguration Configuration => (BackupJobConfiguration)JobConfiguration;

        public BackupJobViewModel(EasySaveConfigurationBase configuration) : base(configuration)
        {
            
        }

        protected override void InitializeCommand()
        {
            _tempEncryptionKey = Configuration.EncryptionKey;
            _tempSimultaneousFileSizeThreshold = Configuration.SimultaneousFileSizeThreshold.ToString();

            JobManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(JobManager.IsRunning))
                    OnPropertyChanged(nameof(CanJobBeRun));
            };
            JobManager.MultipleJobCompletedHandler += jobs =>
            {
                if (jobs == null || jobs.Count == 0)
                    return;

                Dispatcher dispatcher = Application.Current.Dispatcher;
                dispatcher.Invoke(() =>
                {
                    Window mainWindow = Application.Current.MainWindow;
                    MessageBox.Show(
                        mainWindow,
                        L10N.Get().GetTranslation("message_box.jobs_completed.text")
                            .Replace("{COUNT}", jobs.Count.ToString()),
                        L10N.Get().GetTranslation("message_box.jobs_completed.title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            };
            
            JobManager.JobsStoppedHandler += () =>
            {
                Dispatcher dispatcher = Application.Current.Dispatcher;
                dispatcher.Invoke(() =>
                {
                    Window mainWindow = Application.Current.MainWindow;
                    MessageBox.Show(
                        mainWindow,
                        L10N.Get().GetTranslation("message_box.jobs_stopped.text"),
                        L10N.Get().GetTranslation("message_box.jobs_stopped.title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            };

            JobManager.JobsPausedHandler += () =>
            {
                Dispatcher dispatcher = Application.Current.Dispatcher;
                dispatcher.Invoke(() =>
                {
                    Window mainWindow = Application.Current.MainWindow;
                    MessageBox.Show(
                        mainWindow,
                        L10N.Get().GetTranslation("message_box.jobs_paused.text"),
                        L10N.Get().GetTranslation("message_box.jobs_paused.title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            };

            PauseJobsCommand = new RelayCommand(jobNameList =>
            {
                if (!(jobNameList is List<string> jobNames))
                    return;

                if (jobNames.Count == 0)
                {
                    MessageBox.Show(L10N.Get().GetTranslation("message_box.run_no_selected.text"),
                        L10N.Get().GetTranslation("message_box.run_no_selected.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                List<IJob> jobs = jobNames.Select(name => JobManager.GetJob(name)).ToList();

                JobManager.PauseJobs(jobs);
            }, _ => true);

            StopJobsCommand = new RelayCommand(jobNameList =>
            {
                if (!(jobNameList is List<string> jobNames))
                    return;

                if (jobNames.Count == 0)
                {
                    MessageBox.Show(L10N.Get().GetTranslation("message_box.run_no_selected.text"),
                        L10N.Get().GetTranslation("message_box.run_no_selected.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                List<IJob> jobs = jobNames.Select(name => JobManager.GetJob(name)).ToList();

                MessageBoxResult messageBoxResult = MessageBox.Show("Selected Job(s) is/are running, are you sure you want to stop them?",
                    "Stop Jobs", MessageBoxButton.YesNo, MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (messageBoxResult != MessageBoxResult.Yes)
                    return;

                JobManager.StopJobs(jobs);
            }, _ => true);

            BuildJobCommand = new RelayCommand(isCreation =>
            {
                bool isJobCreation = bool.Parse((string)isCreation!);

                if (JobBuilderBase == null)
                    throw new NullReferenceException("BackupJob builder is not defined !");

                if (string.IsNullOrWhiteSpace(GetJobBuilder().Name) ||
                    string.IsNullOrWhiteSpace(GetJobBuilder().Source) ||
                    string.IsNullOrWhiteSpace(GetJobBuilder().Target))
                {
                    MessageBox.Show(L10N.Get().GetTranslation("message_box.missing_data.text"),
                        L10N.Get().GetTranslation("message_box.missing_data.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (isJobCreation)
                {
                    if (!JobManager.AddJob(JobBuilderBase.Build(false), true))
                    {
                        MessageBox.Show(L10N.Get().GetTranslation("message_box.existing_job.text"),
                            L10N.Get().GetTranslation("message_box.existing_job.title"), MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                }
                else if (!JobManager.UpdateJob(JobBuilderBase.InitialName, JobBuilderBase.Build(false)))
                {
                    MessageBox.Show(L10N.Get().GetTranslation("message_box.existing_job.text"),
                        L10N.Get().GetTranslation("message_box.existing_job.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                CloseAction();
            }, _ => true);

            LoadJobInBuilderCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name)
                    JobBuilderBase?.GetFrom(JobManager.GetJob(name));
            }, _ => true);

            DeleteJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name) JobManager.RemoveJob(name);
            }, _ => true);

            //RunJobCommand = new RelayCommand(jobName =>
            //{
            //    if (jobName is string name) JobManager.DoJob(name);
            //}, _ => true);

            RunMultipleJobsCommand = new RelayCommand(jobNameList =>
            {
                if (!(jobNameList is List<string> jobNames))
                    return;

                if (jobNames.Count == 0)
                {
                    MessageBox.Show(L10N.Get().GetTranslation("message_box.run_no_selected.text"),
                        L10N.Get().GetTranslation("message_box.run_no_selected.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                List<IJob> jobs = jobNames.Select(name => JobManager.GetJob(name)).ToList();

                if (!ExternalEncryptor.IsEncryptorPresent() || (ExternalEncryptor.IsEncryptorPresent() && jobs.Any(job =>
                        job.IsEncrypted && !Configuration.ExtensionsToEncrypt.Any())))
                {
                    if (!Configuration.ExtensionsToEncrypt.Any() && ExternalEncryptor.IsEncryptorPresent())
                    {
                        Logger.Log(LogLevel.Warning,
                            "No extensions to encrypt specified in the config file. Encryption will not be performed.");
                        MessageBox.Show(L10N.Get().GetTranslation("message_box.no_extension_encrypt.text"),
                            L10N.Get().GetTranslation("message_box.no_extension_encrypt.title"), MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                    else
                    {
                        Logger.Log(LogLevel.Warning,
                            "'CLEA-Encryptor.exe' not found. Encryption will not be performed.");
                        MessageBox.Show(L10N.Get().GetTranslation("message_box.cant_find_encryptor.text"),
                            L10N.Get().GetTranslation("message_box.cant_find_encryptor.title"), MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                JobManager.DoMultipleJob(jobNames);
            }, _ => !JobManager.IsRunning);

            ChangeRunStrategyCommand = new RelayCommand(strategy =>
            {
                if (strategy is string strategyName)
                    JobManager.Strategy = strategyName switch
                    {
                        "Full" => StrategyType.Full,
                        "Differential" => StrategyType.Differential,
                        _ => throw new NotImplementedException()
                    };
            }, _ => true);

            ShowFolderDialogCommand = new RelayCommand(input =>
            {
                bool isDailyLog = bool.Parse((string)input!);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                string title = L10N.Get().GetTranslation("browse_folder.status_log");

                folderBrowserDialog.Title = title;
                string path = StatusLogPath;

                if (isDailyLog)
                {
                    folderBrowserDialog.Title = L10N.Get().GetTranslation("browse_folder.daily_log");
                    path = DailyLogPath;
                }

                string fullPath = Path.IsPathRooted(path)
                    ? path
                    : Path.GetFullPath(Path.Combine(".", path));

                folderBrowserDialog.InitialFolder = fullPath;
                folderBrowserDialog.AllowMultiSelect = false;

                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;

                if (isDailyLog)
                    DailyLogPath = folderBrowserDialog.SelectedFolder;
                else
                    StatusLogPath = folderBrowserDialog.SelectedFolder;
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
                    MessageBox.Show(L10N.Get().GetTranslation("message_box.encryption_key_invalid.text"),
                        L10N.Get().GetTranslation("message_box.encryption_key_invalid.title"), MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Configuration.EncryptionKey = ExternalEncryptor.ProcessEncryptionKey(encryptionKey);
                MessageBox.Show(L10N.Get().GetTranslation("message_box.encryption_key_valid.text"),
                    L10N.Get().GetTranslation("message_box.encryption_key_valid.title"), MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });

            LoadSimultaneousFileSizeThresholdCommand = new RelayCommand(input =>
            {
                TempSimultaneousFileSizeThreshold = Configuration.SimultaneousFileSizeThreshold.ToString();
            });

            SaveSimultaneousFileSizeThresholdCommand = new RelayCommand(input =>
            {
                string inputStr = (input as string)?.Trim() ?? string.Empty;

                if (!Regex.IsMatch(inputStr, @"^\d+$") || !long.TryParse(inputStr, out long fileSizeThreshold))
                {
                    MessageBox.Show(
                        L10N.Get().GetTranslation("message_box.simultaneous_file_size_threshold_invalid.text.regex"),
                        L10N.Get().GetTranslation("message_box.simultaneous_file_size_threshold_invalid.title.regex"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (fileSizeThreshold < 1)
                {
                    MessageBox.Show(
                        L10N.Get().GetTranslation("message_box.simultaneous_file_size_threshold_invalid.text"),
                        L10N.Get().GetTranslation("message_box.simultaneous_file_size_threshold_invalid.title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                Configuration.SimultaneousFileSizeThreshold = fileSizeThreshold;

                MessageBox.Show(
                    L10N.Get().GetTranslation("message_box.simultaneous_file_size_threshold_valid.text"),
                    L10N.Get().GetTranslation("message_box.simultaneous_file_size_threshold_valid.title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
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
                    ExtensionsToEncrypt.Remove(extensionToRemove);
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
                    ProcessesToBlacklist.Remove(processToRemove);
            }, _ => true);

            AddExtensionToPrioritizeCommand = new RelayCommand(input =>
            {
                string extension = (input as string)?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(extension))
                    return;

                if (!Regex.IsMatch(extension, @"^\.[\w]+$"))
                    return;

                if (!ExtensionsToPrioritize.Contains(extension))
                {
                    ExtensionsToPrioritize.Add(extension);
                    NewExtensionToPrioritize = string.Empty;
                }
            }, _ => true);

            RemoveExtensionToPrioritizeCommand = new RelayCommand(input =>
            {
                string extensionToPrioritize = (input as string)?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(extensionToPrioritize) && ExtensionsToPrioritize.Contains(extensionToPrioritize))
                    ExtensionsToPrioritize.Remove(extensionToPrioritize);
            }, _ => true);

            RunAllJobsCommand = new RelayCommand(_ => { JobManager.DoAllJobs(); }, _ => true);
        }

        public ViewModelBackupJobBuilder GetJobBuilder()
        {
            return (ViewModelBackupJobBuilder)JobBuilderBase;
        }

        public void OnTaskCompletedFor(string[] jobNames, IJob.TaskCompletedDelegate callback)
        {
            foreach (string jobName in jobNames)
            {
                IJob job = JobManager.GetJob(jobName);
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
            IJob existingJob = JobManager.GetJob(name);
            bool exists = existingJob != null;

            return isCreation ? !exists : exists;
        }

        public void UpdateFromJobBuilder()
        {
            JobManager?.UpdateJob(JobBuilderBase?.InitialName, JobBuilderBase.Build());
        }
        
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}