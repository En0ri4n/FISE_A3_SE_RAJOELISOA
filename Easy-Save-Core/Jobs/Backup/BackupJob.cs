using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;

namespace EasySaveCore.Models
{
    public class BackupJob : IJob
    {
        private Property<dynamic> _timestamp;
        private Property<dynamic> _source;
        private Property<dynamic> _target;
        private Property<dynamic> _strategyType;
        
        private Property<dynamic> _size;
        private Property<dynamic> _transferTime;
        
        public DateTime Timestamp
        {
            get => (DateTime) _timestamp.Value;
            set => _timestamp.Value = value;
        }
        
        public string Source
        {
            get => (string) _source.Value;
            set => _source.Value = value;
        }
        
        public string Target
        {
            get => (string) _target.Value;
            set => _target.Value = value;
        }
        
        public JobExecutionStrategy.StrategyType StrategyType
        {
            get => (JobExecutionStrategy.StrategyType) _strategyType.Value;
            set => _strategyType.Value = value;
        }
        
        public long Size
        {
            get => (long) _size.Value;
            set => _size.Value = value;
        }
        
        public long TransferTime
        {
            get => (long) _transferTime.Value;
            set => _transferTime.Value = value;
        }

        public bool IsRunning;

        public string Name { get; set; }
        
        public JobExecutionStrategy.ExecutionStatus Status { get; set; } = JobExecutionStrategy.ExecutionStatus.NotStarted;

        public event IJob.TaskCompletedDelegate? TaskCompletedHandler;
        public List<Property<dynamic>> Properties => new List<Property<dynamic>>();

        bool IJob.IsRunning { get => IsRunning; set => IsRunning = value; }

        public readonly List<BackupJobTask> BackupJobTasks = new List<BackupJobTask>();

        public BackupJob() : this(string.Empty, string.Empty, string.Empty, JobExecutionStrategy.StrategyType.Full)
        {
        }

        public BackupJob(string name, string source, string target, JobExecutionStrategy.StrategyType strategy)
        {
            Name = name;
            _timestamp = new Property<dynamic>("timestamp", new DateTime());
            _source = new Property<dynamic>("source", source);
            _target = new Property<dynamic>("target", target);
            _strategyType = new Property<dynamic>("strategyType", strategy);
            _size = new Property<dynamic>("size", (long) 0);
            _transferTime = new Property<dynamic>("transferTime", (long) 0);
            Properties.AddRange(new List<Property<dynamic>> 
            { 
                new Property<dynamic>("name", name), 
                _timestamp, 
                _source, 
                _target, 
                _strategyType,
                _size, 
                _transferTime
            });
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
            return !IsRunning && !ProcessHelper.IsAnyProcessRunning(EasySaveConfiguration<BackupJob>.Get().ProcessesToBlacklist.ToArray());
        }

        public void RunJob()
        {
            if (!CanRunJob())
            {
                Status = JobExecutionStrategy.ExecutionStatus.CanNotStart;
                return;
            }

            if (string.IsNullOrEmpty(_source.Value.ToString()) || string.IsNullOrEmpty(_target.Value.ToString()))
                throw new Exception("Source or Target path is not set.");

            if (!Directory.Exists(_source.Value.ToString()))
                throw new DirectoryNotFoundException($"Source directory '{_source.Value.ToString()}' does not exist.");

            if (_source.Value == _target.Value)
                throw new Exception("Source and Target paths cannot be the same.");
        
            BackupJobTasks.Clear();
        
            IsRunning = true;
            _timestamp.Value = DateTime.Now;
        
            if (!Directory.Exists(_target.Value.ToString()))
                Directory.CreateDirectory(_target.Value.ToString());

            string[] sourceDirectoriesArray = Directory.GetDirectories((string) _source.Value.ToString(), "*", SearchOption.AllDirectories);

            foreach (string directory in sourceDirectoriesArray)
            {
                string dirToCreate = directory.Replace(_source.Value.ToString(), _target.Value.ToString());
                Directory.CreateDirectory(dirToCreate);
            }

            string[] sourceFilesArray = Directory.GetFiles((string) _source.Value.ToString(), "*.*", SearchOption.AllDirectories);

            foreach (string path in sourceFilesArray)
            {
                BackupJobTask jobTask = new BackupJobTask(this, path, path.Replace((string) _source.Value.ToString(), (string) _target.Value.ToString()));
                BackupJobTasks.Add(jobTask);
            }

            foreach(BackupJobTask jobTask in BackupJobTasks)
                jobTask.ExecuteTask((JobExecutionStrategy.StrategyType) _strategyType.Value);

            _transferTime.Value = BackupJobTasks.Select(x => (long)x.TransferTime.Value).Sum();
            _size.Value = BackupJobTasks.FindAll(x=>x.TransferTime.Value != -1).Select(x => (long)x.Size.Value).Sum();

            IsRunning = false;

            Status = JobExecutionStrategy.ExecutionStatus.Completed;
        }

        public JsonObject JsonSerialize()
        {
            JsonObject jsonObject = new JsonObject();

            jsonObject.Add("Name", Name);
            jsonObject.Add("Source", _source.Value);
            jsonObject.Add("Target", _target.Value);
            return jsonObject;
        }

        public void JsonDeserialize(JsonObject data)
        {
            if (data.ContainsKey("Name"))
                Name = data["Name"]!.ToString();
            else
                throw new Exception("Invalid JSON data: Missing 'Name' property.");

            if (data.ContainsKey("Source"))
                _source.Value = data["Source"]!.ToString();
            else
                throw new Exception("Invalid JSON data: Missing 'Source' property.");

            if (data.ContainsKey("Target"))
                _target.Value = data["Target"]!.ToString();
            else
                throw new Exception("Invalid JSON data: Missing 'Target' property.");
        }

        public XmlElement XmlSerialize(XmlDocument parent)
        {
            XmlElement jobElement = parent.CreateElement("BackupJob");

            jobElement.SetAttribute("Name", Name);
            jobElement.SetAttribute("Source", _source.Value.ToString());
            jobElement.SetAttribute("Target", _target.Value.ToString());

            return jobElement;
        }

        public void XmlDeserialize(XmlElement data)
        {
            if (data.HasAttribute("Name"))
                Name = data.GetAttribute("Name");
            else
                throw new Exception("Invalid XML data: Missing 'Name' attribute.");

            if (data.HasAttribute("Source"))
                _source.Value = data.GetAttribute("Source");
            else
                throw new Exception("Invalid XML data: Missing 'Source' attribute.");

            if (data.HasAttribute("Target"))
                _target.Value = data.GetAttribute("Target");
            else
                throw new Exception("Invalid XML data: Missing 'Target' attribute.");
        }
    }
}
