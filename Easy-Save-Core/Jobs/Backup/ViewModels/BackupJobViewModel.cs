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

        private string _tempEncryptionKey;

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

        public ObservableCollection<string> ExtensionsToEncrypt => Configuration.ExtensionsToEncrypt;
        public ObservableCollection<string> ProcessesToBlacklist => Configuration.ProcessesToBlacklist;

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

        public bool CanJobBeRun => !JobManager.IsRunning;

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
        public ICommand LoadEncryptionKeyCommand { get; set; }
        public ICommand SaveEncryptionKeyCommand { get; set; }

        public Action CloseAction { get; set; }
        
        private BackupJobConfiguration Configuration => (BackupJobConfiguration)JobConfiguration;

        public BackupJobViewModel(EasySaveConfigurationBase configuration) : base(configuration)
        {
            
        }

        protected override void InitializeCommand()
        {
            _tempEncryptionKey = Configuration.EncryptionKey;
            JobManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(JobManager.IsRunning))
                    OnPropertyChanged(nameof(CanJobBeRun));
            };
            JobManager.MultipleJobCompletedHandler += jobs =>
            {
                if (jobs == null || jobs.Count == 0)
                    return;

                var dispatcher = Application.Current.Dispatcher;
                dispatcher.Invoke(() =>
                {
                    var mainWindow = Application.Current.MainWindow;
                    MessageBox.Show(
                        mainWindow,
                        L10N.Get().GetTranslation("message_box.jobs_completed.text")
                            .Replace("{COUNT}", jobs.Count.ToString()),
                        L10N.Get().GetTranslation("message_box.jobs_completed.title"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            };

            BuildJobCommand = new RelayCommand(isCreation =>
            {
                var isJobCreation = bool.Parse((string)isCreation!);

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
            }, _ => true); //Todo utiliser commandes pour désactiver boutons

            RunJobCommand = new RelayCommand(jobName =>
            {
                if (jobName is string name) JobManager.DoJob(name);
            }, _ => true);

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

                var jobs = jobNames.Select(name => JobManager.GetJob(name)).ToList();

                if (!ExternalEncryptor.IsEncryptorPresent() && jobs.Any(job =>
                        job.IsEncrypted && Configuration.ExtensionsToEncrypt.Any()))
                {
                    if (!Configuration.ExtensionsToEncrypt.Any())
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

                // deactivateButtons()
                JobManager.DoMultipleJob(jobNames);
                // reactivateButtons()
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
                var isDailyLog = bool.Parse((string)input!);

                var folderBrowserDialog = new FolderBrowserDialog();
                var title = L10N.Get().GetTranslation("browse_folder.status_log");

                folderBrowserDialog.Title = title;
                var path = StatusLogPath;

                if (isDailyLog)
                {
                    folderBrowserDialog.Title = L10N.Get().GetTranslation("browse_folder.daily_log");
                    ;
                    path = DailyLogPath;
                }

                var fullPath = Path.IsPathRooted(path)
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
                var isDailyLogPath = bool.Parse((string)input!);

                var path = @"logs\";

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
                var encryptionKey = (input as string)?.Trim() ?? string.Empty;

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

            AddExtensionToEncryptCommand = new RelayCommand(input =>
            {
                var extension = (input as string)?.Trim() ?? string.Empty;

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
                var extensionToRemove = (input as string)?.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(extensionToRemove) && ExtensionsToEncrypt.Contains(extensionToRemove))
                    ExtensionsToEncrypt.Remove(extensionToRemove);
            }, _ => true);

            AddProcessToBlacklistCommand = new RelayCommand(input =>
            {
                var process = (input as string)?.Trim() ?? string.Empty;

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
                var processToRemove = input as string ?? string.Empty;
                if (!string.IsNullOrEmpty(processToRemove) && ProcessesToBlacklist.Contains(processToRemove))
                    ProcessesToBlacklist.Remove(processToRemove);
            }, _ => true);

            RunAllJobsCommand = new RelayCommand(_ => { JobManager.DoAllJobs(); }, _ => true);
        }

        public ViewModelBackupJobBuilder GetJobBuilder()
        {
            return (ViewModelBackupJobBuilder)JobBuilderBase;
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
                var fullPath = Path.GetFullPath(path);

                return !Path.HasExtension(fullPath);
            }
            catch
            {
                return false;
            }
        }


        public bool IsNameValid(string name, bool isCreation)
        {
            var existingJob = JobManager.GetJob(name);
            var exists = existingJob != null;

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