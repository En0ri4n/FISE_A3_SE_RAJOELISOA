using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Xml;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;

namespace EasySaveCore.Models
{
    public class BackupJob : IJob, INotifyPropertyChanged
    {
        public BackupJobManager Manager { get; }
        public readonly List<BackupJobTask> BackupJobTasks = new List<BackupJobTask>();
        
        public List<JobTask> JobTasks
        {
            get => BackupJobTasks.Cast<JobTask>().ToList();
            set
            {
                BackupJobTasks.Clear();
                BackupJobTasks.AddRange(value.Cast<BackupJobTask>());
            }
        }

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
                if (BackupJobTasks.Count == 0)
                    return 0.0D;

                var totalTasksSize = BackupJobTasks.Sum(bjt => bjt.Size);
                var completedTasks = BackupJobTasks
                    .FindAll(task => task.Status != JobExecutionStrategy.ExecutionStatus.NotStarted)
                    .Sum(task => task.Size);
                return (double)completedTasks / totalTasksSize * 100D;
            }
        }

        public bool IsRunning { get; set; }

        public string Name { get; private set; }

        private readonly int fileSizeThreshold = 100000; //TODO fetch real value in the config
        private readonly static Semaphore _semaphoreSizeThreshold = new Semaphore(1, 1);

        public JobExecutionStrategy.ExecutionStatus Status { get; set; } = JobExecutionStrategy.ExecutionStatus.NotStarted;

        public event IJob.TaskCompletedDelegate? TaskCompletedHandler;
        public event IJob.JobCompletedDelegate? JobCompletedHandler;

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

        public void RunJob()
        {
            if (!CanRunJob())
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

            IsRunning = true;
            Timestamp = DateTime.Now;

            UpdateProgress();

            if (!Directory.Exists(Target))
                Directory.CreateDirectory(Target);

            string[] sourceDirectoriesArray = Directory.GetDirectories(Source, "*", SearchOption.AllDirectories);

            foreach (var directory in sourceDirectoriesArray)
            {
                var dirToCreate = directory.Replace(Source, Target);
                Directory.CreateDirectory(dirToCreate);
            }





            foreach (BackupJobTask jobTask in BackupJobTasks)
            {
                if (jobTask.Size >= fileSizeThreshold) //TODO : Is it possible to only call the lock in the if statement to avoid duplicating code
                {
                    _semaphoreSizeThreshold.WaitOne();
                    jobTask.ExecuteTask(StrategyType);
                    _semaphoreSizeThreshold.Release();
                }
                else
                {
                    jobTask.ExecuteTask(StrategyType);
                }
            }
            TransferTime = BackupJobTasks.Select(x => x.TransferTime).Sum();
            EncryptionTime = BackupJobTasks.Select(x => x.EncryptionTime).Sum();

            CompleteJob(BackupJobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed)
                ? JobExecutionStrategy.ExecutionStatus.Completed
                : JobExecutionStrategy.ExecutionStatus.Failed);
        }

        public JsonObject JsonSerialize()
        {
            var jsonObject = new JsonObject();

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
            var jobElement = parent.CreateElement("BackupJob");

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
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(TransferTime));
            OnPropertyChanged(nameof(EncryptionTime));
        }

        public void ClearAndSetupJob()
        {
            BackupJobTasks.Clear();

            if (!string.IsNullOrEmpty(Source))
            {
                string[] sourceFilesArray = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

                foreach (var path in sourceFilesArray)
                {
                    var jobTask = new BackupJobTask(this, path, path.Replace(Source, Target));
                    BackupJobTasks.Add(jobTask);
                }
            }

            TransferTime = -1L;
            EncryptionTime = -1L;
            Size = BackupJobTasks.Select(x => x.Size).Sum();
            Status = JobExecutionStrategy.ExecutionStatus.InQueue;
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
            UpdateProgress();
            JobCompletedHandler?.Invoke(this);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}