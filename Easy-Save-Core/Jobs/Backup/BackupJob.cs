using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;

namespace EasySaveCore.Models
{
    public class BackupJob : IJob
    {
        private DateTime _timestamp;
        private string _source;
        private string _target;
        private JobExecutionStrategy.StrategyType _strategyType;
        private bool _isEncrypted;
        
        private long _size;
        private long _transferTime;
        private long _encryptionTime;
        
        public DateTime Timestamp
        {
            get => _timestamp;
            set => _timestamp = value;
        }
        
        public string Source
        {
            get => _source;
            set => _source = value;
        }
        
        public string Target
        {
            get => _target;
            set => _target = value;
        }
        
        public JobExecutionStrategy.StrategyType StrategyType
        {
            get => _strategyType;
            set => _strategyType = value;
        }
        
        public bool IsEncrypted
        {
            get => _isEncrypted;
            set => _isEncrypted = value;
        }
        
        public long Size
        {
            get => _size;
            set => _size = value;
        }
        
        public long TransferTime
        {
            get => _transferTime;
            set => _transferTime = value;
        }
        
        public long EncryptionTime
        {
            get => _encryptionTime;
            set => _encryptionTime = value;
        }

        public bool IsRunning;

        public string Name { get; set; }
        
        public JobExecutionStrategy.ExecutionStatus Status { get; set; } = JobExecutionStrategy.ExecutionStatus.NotStarted;

        public event IJob.TaskCompletedDelegate? TaskCompletedHandler;

        bool IJob.IsRunning { get => IsRunning; set => IsRunning = value; }

        public readonly List<BackupJobTask> BackupJobTasks = new List<BackupJobTask>();

        public BackupJob() : this(string.Empty, string.Empty, string.Empty, JobExecutionStrategy.StrategyType.Full)
        {
        }

        public BackupJob(string name, string source, string target, JobExecutionStrategy.StrategyType strategy)
        {
            Name = name;
            _timestamp = DateTime.Now;
            _source = source;
            _target = target;
            _strategyType = strategy;
            _isEncrypted = false;
            _size = -1L;
            _transferTime = -1L;
            _encryptionTime = -1L;
        }
        
        public void OnTaskCompleted(dynamic task)
        {
            TaskCompletedHandler?.Invoke(task);
        }
        
        public void ClearTaskCompletedHandler()
        {
            TaskCompletedHandler = null;
        }
        
        public bool CanRunJob()
        {
            return !IsRunning && !ProcessHelper.IsAnyProcessRunning(BackupJobConfiguration.Get().ProcessesToBlacklist.ToArray());
        }

        public void RunJob()
        {
            if (!CanRunJob())
            {
                Status = JobExecutionStrategy.ExecutionStatus.CanNotStart;
                return;
            }

            if (!Directory.Exists(Source))
            {
                Status = JobExecutionStrategy.ExecutionStatus.SourceNotFound;
                return;
            }

            if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(Target))
            {
                Status = JobExecutionStrategy.ExecutionStatus.DirectoriesNotSpecified;
                return;
            }

            if (Source.Equals(Target))
            {
                Status = JobExecutionStrategy.ExecutionStatus.SameSourceAndTarget;
                return;
            }

            BackupJobTasks.Clear();
        
            IsRunning = true;
            Timestamp = DateTime.Now;
        
            if (!Directory.Exists(Target))
                Directory.CreateDirectory(Target);

            string[] sourceDirectoriesArray = Directory.GetDirectories(Source, "*", SearchOption.AllDirectories);

            foreach (string directory in sourceDirectoriesArray)
            {
                string dirToCreate = directory.Replace(Source, Target);
                Directory.CreateDirectory(dirToCreate);
            }

            string[] sourceFilesArray = Directory.GetFiles(Source, "*.*", SearchOption.AllDirectories);

            foreach (string path in sourceFilesArray)
            {
                BackupJobTask jobTask = new BackupJobTask(this, path, path.Replace(Source, Target));
                BackupJobTasks.Add(jobTask);
            }

            foreach(BackupJobTask jobTask in BackupJobTasks)
                jobTask.ExecuteTask(_strategyType);

            TransferTime = BackupJobTasks.Select(x => x.TransferTime).Sum();
            Size = BackupJobTasks.FindAll(x=>x.TransferTime != -1).Select(x => x.Size).Sum();
            EncryptionTime = BackupJobTasks.Select(x => x.EncryptionTime).Sum();

            IsRunning = false;

            Status = JobExecutionStrategy.ExecutionStatus.Completed;
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
                StrategyType = (JobExecutionStrategy.StrategyType)Enum.Parse(typeof(JobExecutionStrategy.StrategyType), data["StrategyType"]!.ToString());
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'StrategyType' property.");
            
            if (data.ContainsKey("IsEncrypted"))
                IsEncrypted = bool.Parse(data["IsEncrypted"]!.ToString());
            else
                throw new KeyNotFoundException("Invalid JSON data: Missing 'IsEncrypted' property.");
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
                StrategyType = (JobExecutionStrategy.StrategyType)Enum.Parse(typeof(JobExecutionStrategy.StrategyType), data.GetAttribute("StrategyType"));
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'StrategyType' attribute.");
            
            if (data.HasAttribute("IsEncrypted"))
                IsEncrypted = bool.Parse(data.GetAttribute("IsEncrypted"));
            else
                throw new KeyNotFoundException("Invalid XML data: Missing 'IsEncrypted' attribute.");
        }
    }
}
