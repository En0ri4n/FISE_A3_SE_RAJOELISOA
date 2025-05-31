using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;

namespace EasySaveCore.Models
{
    public sealed class BackupJob : IJob, INotifyPropertyChanged
    {
        public BackupJobManager Manager { get; }
        public List<JobTask> JobTasks { get; set; } = new List<JobTask>();

        public bool IsPaused { get; set; }
        public bool WasPaused { get; set; }
        public bool IsStopped { get; set; }

        private static int _hasShownPopup = 0;

        public event IJob.TaskCompletedDelegate? TaskCompletedHandler;
        public event IJob.JobCompletedDelegate? JobCompletedHandler;
        public event IJob.JobPausedDelegate? JobPausedHandler;
        public event IJob.JobStoppedDelegate? JobStoppedHandler;


        public BackupJob(BackupJobManager manager) : this( manager, string.Empty, string.Empty, string.Empty, JobExecutionStrategy.StrategyType.Full)
        {
        }

        public BackupJob(BackupJobManager manager, string name, string source, string target, JobExecutionStrategy.StrategyType strategy, bool isEncrypted = false)
        {
            Manager = manager;
            Name = name;
            Timestamp = DateTime.Now;
            Source = source;
            Target = target;
            StrategyType = strategy;
            IsEncrypted = isEncrypted;
            Size = -1L;
            TransferTime = -1L;
            EncryptionTime = -1L;
            TaskCompletedHandler += task => UpdateProgress();
            ClearAndSetupJob();
        }

        private static int _isPopupOpenAtomic = 0;
        private static bool _alreadyWarnedForBlacklistedProcess = false;

        public DateTime Timestamp { get; set; }

        public string Source { get; set; }

        public string Target { get; set; }

        public JobExecutionStrategy.StrategyType StrategyType { get; set; }

        public bool IsEncrypted { get; set; }

        public long Size { get; set; }

        public long TransferTime { get; set; }

        public long EncryptionTime { get; set; }

        public double Progress
        {
            get
            {
                if (JobTasks.Count == 0)
                    return 0.0D;

                long totalTasksSize = JobTasks.Sum(jt => jt.Size);
                long completedTasks = JobTasks
                    .FindAll(task => task.Status != JobExecutionStrategy.ExecutionStatus.NotStarted)
                    .Sum(task => task.Size);
                return (double)completedTasks / totalTasksSize * 100D;
            }
        }

        public bool IsRunning { get; set; }

        public string Name { get; private set; }

        private readonly static Semaphore _semaphoreSizeThreshold = new Semaphore(1, 1);

        public JobExecutionStrategy.ExecutionStatus Status { get; set; } = JobExecutionStrategy.ExecutionStatus.NotStarted;

        public void ClearTaskCompletedHandler()
        {
            TaskCompletedHandler = null;
        }

        public void ClearJobCompletedHandler()
        {
            JobCompletedHandler = null;
        }

        public bool CanRunJob()
        {
            return !IsRunning;
        }

        public void RunJob(CountdownEvent countdown)
        {
            try
            {
                if (!CanRunJob() && !Manager.IsPaused)
                {
                    CompleteJob(JobExecutionStrategy.ExecutionStatus.JobAlreadyRunning);
                    return;
                }

                if (!Directory.Exists(Source))
                {
                    CompleteJob(JobExecutionStrategy.ExecutionStatus.SourceNotFound);
                    return;
                }

                if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Target))
                {
                    CompleteJob(JobExecutionStrategy.ExecutionStatus.DirectoriesNotSpecified);
                    return;
                }

                if (Source.Equals(Target))
                {
                    CompleteJob(JobExecutionStrategy.ExecutionStatus.SameSourceAndTarget);
                    return;
                }

                Status = JobExecutionStrategy.ExecutionStatus.InProgress;

                if (!WasPaused)
                {
                    IsRunning = true;
                    Timestamp = DateTime.Now;
                }

                UpdateProgress();

                if (!Directory.Exists(Target))
                    Directory.CreateDirectory(Target);

                string[] sourceDirectoriesArray = Directory.GetDirectories(Source, "*", SearchOption.AllDirectories);

                foreach (string directory in sourceDirectoriesArray)
                {
                    string dirToCreate = directory.Replace(Source, Target);
                    Directory.CreateDirectory(dirToCreate);
                }

                foreach (JobTask jobTask in JobTasks)
                {
                    while (IsPaused)
                    {
                        Thread.Sleep(500);
                    }

                    if (WasPaused && jobTask.Status != JobExecutionStrategy.ExecutionStatus.NotStarted)
                        continue;

                    while (ProcessHelper.IsAnyProcessRunning(
                        ((BackupJobConfiguration)CLEA.EasySaveCore.Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.ToArray()))
                    {
                        if (!_alreadyWarnedForBlacklistedProcess)
                        {
                            if (Interlocked.CompareExchange(ref _isPopupOpenAtomic, 1, 0) == 0)
                            {
                                _alreadyWarnedForBlacklistedProcess = true;
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(
                                        "Blacklisted process detected, all backup jobs have been paused.",
                                        "Jobs Paused",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information
                                    );
                                    Interlocked.Exchange(ref _isPopupOpenAtomic, 0);
                                });
                            }
                        }

                        Manager.PauseJobs(Manager.GetJobs().ToList(), true);
                        Thread.Sleep(500);
                    }

                    _alreadyWarnedForBlacklistedProcess = false;


                    if (!Directory.Exists(Source))
                    {
                        Manager.StopJob(Name);
                        MessageBox.Show("The source Directory has been removed in between the running of the job. Job has been terminated.", "Source Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    if (jobTask.Size * 1024 >= ((BackupJobConfiguration)CLEA.EasySaveCore.Core.EasySaveCore.Get().Configuration).SimultaneousFileSizeThreshold)
                    {
                        _semaphoreSizeThreshold.WaitOne();

                        if (IsStopped)
                        {
                            _semaphoreSizeThreshold.Release();
                            CompleteJob(JobExecutionStrategy.ExecutionStatus.Stopped);
                            return;
                        }
                        jobTask.ExecuteTask(StrategyType);
                        _semaphoreSizeThreshold.Release();
                    }
                    else
                    {
                        if (IsStopped)
                        {
                            CompleteJob(JobExecutionStrategy.ExecutionStatus.Stopped);
                            return;
                        }
                        jobTask.ExecuteTask(StrategyType);
                    }
                }

                TransferTime = JobTasks.Select(x => x.TransferTime).Sum();
                EncryptionTime = JobTasks.Select(x => x.EncryptionTime).Sum();

                CompleteJob(JobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed)
                    ? JobExecutionStrategy.ExecutionStatus.Completed
                    : JobExecutionStrategy.ExecutionStatus.Failed);
            }
            finally
            {
                countdown.Signal();
            }
        }


        public void PauseJob()
        {
            if (!IsRunning)
                return;
            
            WasPaused = false;
            IsPaused = true;
            Status = JobExecutionStrategy.ExecutionStatus.Paused;
            UpdateProgress();
            JobPausedHandler?.Invoke(this);
        }

        public void StopJob()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            IsStopped = true;
            Status = JobExecutionStrategy.ExecutionStatus.Stopped;
            UpdateProgress();
            JobStoppedHandler?.Invoke(this);
        }

        public void ResumeJob()
        {
            if (!IsRunning)
                return;

            WasPaused = true;
            IsPaused = false;
            Status = JobExecutionStrategy.ExecutionStatus.InProgress;
            UpdateProgress();
        }

        public JsonObject JsonSerialize()
        {
            JsonObject jsonObject = new JsonObject();

            jsonObject.Add("Name", Name);
            jsonObject.Add("Source", Source);
            jsonObject.Add("Target", Target);
            jsonObject.Add("StrategyType", StrategyType.ToString());
            jsonObject.Add("IsEncrypted", IsEncrypted.ToString());

            return jsonObject;
        }

        public void JsonDeserialize(JsonObject data)
        {
            if (data.ContainsKey("Name"))
                Name = data["Name"]!.ToString();
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'Name' property.");

            if (data.ContainsKey("Source"))
                Source = data["Source"]!.ToString();
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'Source' property.");

            if (data.ContainsKey("Target"))
                Target = data["Target"]!.ToString();
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'Target' property.");

            if (data.ContainsKey("StrategyType"))
                StrategyType = (JobExecutionStrategy.StrategyType)Enum.Parse(typeof(JobExecutionStrategy.StrategyType),
                    data["StrategyType"]!.ToString());
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'StrategyType' property.");

            if (data.ContainsKey("IsEncrypted"))
                IsEncrypted = bool.Parse(data["IsEncrypted"]!.ToString());
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'IsEncrypted' property.");
            
            ClearAndSetupJob();
        }

        public XmlElement XmlSerialize(XmlDocument parent)
        {
            XmlElement jobElement = parent.CreateElement("BackupJob");

            jobElement.SetAttribute("Name", Name);
            jobElement.SetAttribute("Source", Source);
            jobElement.SetAttribute("Target", Target);
            jobElement.SetAttribute("StrategyType", StrategyType.ToString());
            jobElement.SetAttribute("IsEncrypted", IsEncrypted.ToString());

            return jobElement;
        }

        public void XmlDeserialize(XmlElement data)
        {
            if (data.HasAttribute("Name"))
                Name = data.GetAttribute("Name");
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'Name' attribute.");

            if (data.HasAttribute("Source"))
                Source = data.GetAttribute("Source");
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'Source' attribute.");

            if (data.HasAttribute("Target"))
                Target = data.GetAttribute("Target");
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'Target' attribute.");

            if (data.HasAttribute("StrategyType"))
                StrategyType = (JobExecutionStrategy.StrategyType)Enum.Parse(typeof(JobExecutionStrategy.StrategyType),
                    data.GetAttribute("StrategyType"));
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'StrategyType' attribute.");

            if (data.HasAttribute("IsEncrypted"))
                IsEncrypted = bool.Parse(data.GetAttribute("IsEncrypted"));
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'IsEncrypted' attribute.");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void UpdateProgress()
        {
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(TransferTime));
            OnPropertyChanged(nameof(EncryptionTime));
        }

        public void ClearAndSetupJob()
        {
            JobTasks.Clear();

            if (!string.IsNullOrEmpty(Source))
            {
                string[] sourceFilesArray = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

                foreach (string path in sourceFilesArray)
                {
                    BackupJobTask jobTask = new BackupJobTask(this, path, path.Replace(Source, Target));
                    JobTasks.Add(jobTask);
                }
            }

            TransferTime = -1L;
            EncryptionTime = -1L;
            Size = JobTasks.Select(x => x.Size).Sum();
            Status = JobExecutionStrategy.ExecutionStatus.NotStarted;
            UpdateProgress();
        }

        public void OnTaskCompleted(dynamic task)
        {
            TaskCompletedHandler?.Invoke(task);
        }

        public void CompleteJob(JobExecutionStrategy.ExecutionStatus status)
        {
            Status = status;
            IsRunning = false;
            WasPaused = false;
            UpdateProgress();
            if (status != JobExecutionStrategy.ExecutionStatus.Stopped)
                JobCompletedHandler?.Invoke(this);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}