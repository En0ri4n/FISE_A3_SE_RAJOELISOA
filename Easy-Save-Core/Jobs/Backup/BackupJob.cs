using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Xml;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Jobs.Backup.ViewModels;

namespace EasySaveCore.Models
{
    public sealed class BackupJob : IJob, INotifyPropertyChanged
    {
        public BackupJobManager Manager { get; }
        public List<JobTask> JobTasks { get; set; } = new List<JobTask>();

        public bool IsPaused { get; set; }
        public bool WasPaused { get; set; }

        public event IJob.TaskCompletedDelegate? TaskCompletedHandler;
        public event IJob.JobCompletedDelegate? JobCompletedHandler;
        public event IJob.JobPausedDelegate? JobPausedHandler;

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

        public void RunJob(int threadsHandlingPriority, ManualResetEventSlim canStartNonPriority, List<String> priority_extensions)
        {
            if (!CanRunJob() && !IsPaused)
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

            List<JobTask> PriorityJobTasks = new List<JobTask>();
            List<JobTask> NonPriorityJobTasks = new List<JobTask>();
            foreach (JobTask jobTask in JobTasks) //check for priority queue.
            {
                //ExtensionsToPrioritize;
                //TODO HOW TO ACCESS THAT

                //List<String> priority_extensions = new List<String>(); //TODO REMOVE when seraching in the config
                //TODO : what if a file has no extension ???
                int index_extension = jobTask.Name.LastIndexOf(".");
                if (index_extension == -1)
                {
                    PriorityJobTasks.Add(jobTask);
                }
                else if (priority_extensions.Contains(jobTask.Name.Substring(index_extension))) // cut to the extension only TODO CHECK. 
                {
                    PriorityJobTasks.Add(jobTask);
                }
                else
                {
                    NonPriorityJobTasks.Add(jobTask);
                }
            }
            Interlocked.Increment(ref threadsHandlingPriority);
            runTasks(PriorityJobTasks);
            int remainingPriority = Interlocked.Decrement(ref threadsHandlingPriority);
            if (remainingPriority == 0)
            {
                canStartNonPriority.Set(); // Allow B to start
            }
            canStartNonPriority.Wait();
            runTasks(NonPriorityJobTasks);
            TransferTime = JobTasks.Select(x => x.TransferTime).Sum();
            EncryptionTime = JobTasks.Select(x => x.EncryptionTime).Sum();

            CompleteJob(JobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed)
                ? JobExecutionStrategy.ExecutionStatus.Completed
                : JobExecutionStrategy.ExecutionStatus.Failed);
        }

        public void runTasks(List<JobTask> JobTasks)
        {
            foreach (JobTask jobTask in JobTasks)
            {
                // Wait until the job is resumed
                while (IsPaused)
                {
                    Thread.Sleep(100); // Sleep to avoid busy waiting
                }

                if (WasPaused && jobTask.Status != JobExecutionStrategy.ExecutionStatus.NotStarted)
                    continue;

                // Compare if jobTask size in kB is greater than or equal to the threshold defined in the configuration
                if (jobTask.Size * 1024 >= ((BackupJobConfiguration)CLEA.EasySaveCore.Core.EasySaveCore.Get().Configuration).SimultaneousFileSizeThreshold) //TODO : Is it possible to only call the lock in the if statement to avoid duplicating code | Test to be sure
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
        }

        public void PauseJob()
        {
            if (!IsRunning || IsPaused)
                return;
            
            IsPaused = true;
            Status = JobExecutionStrategy.ExecutionStatus.Paused;
            UpdateProgress();
            JobPausedHandler?.Invoke(this);
        }
        //URGENT TODO
        /*public Action ResumeJob()
        {
            if (!IsRunning || !IsPaused)
                return () => {};

            IsPaused = false;
            WasPaused = true;
            Status = JobExecutionStrategy.ExecutionStatus.InProgress;
            UpdateProgress();
            return RunJob;
        }*/

        //THIS IS TEMPORRARY BUT AWFUL/ Changes to the runJob function
        public Action ResumeJob()
        {
            return PauseJob;
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
            IsPaused = false;
            WasPaused = false;
            UpdateProgress();
            JobCompletedHandler?.Invoke(this);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}