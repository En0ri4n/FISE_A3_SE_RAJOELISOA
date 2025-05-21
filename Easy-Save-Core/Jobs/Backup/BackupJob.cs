using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;

namespace EasySaveCore.Models
{
    public class BackupJob : IJob
    {
        public Property<dynamic> Timestamp;
        public Property<dynamic> Source;
        public Property<dynamic> Target;
        public Property<dynamic> StrategyType;
        
        public Property<dynamic> Size;
        public Property<dynamic> TransferTime;

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
            Timestamp = new Property<dynamic>("timestamp", new DateTime());
            Source = new Property<dynamic>("source", source);
            Target = new Property<dynamic>("target", target);
            StrategyType = new Property<dynamic>("strategyType", strategy);
            Size = new Property<dynamic>("size", (long) 0);
            TransferTime = new Property<dynamic>("transferTime", (long) 0);
            Properties.AddRange(new List<Property<dynamic>> 
            { 
                new Property<dynamic>("name", name), 
                Timestamp, 
                Source, 
                Target, 
                StrategyType,
                Size, 
                TransferTime
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
            return !IsRunning;
        }

        public void RunJob()
        {
            if (!CanRunJob())
            {
                Status = JobExecutionStrategy.ExecutionStatus.CanNotStart;
                return;
            }

            if (string.IsNullOrEmpty(Source.Value.ToString()) || string.IsNullOrEmpty(Target.Value.ToString()))
                throw new Exception("Source or Target path is not set.");

            if (!Directory.Exists(Source.Value.ToString()))
                throw new DirectoryNotFoundException($"Source directory '{Source.Value.ToString()}' does not exist.");

            if (Source.Value == Target.Value)
                throw new Exception("Source and Target paths cannot be the same.");
        
            BackupJobTasks.Clear();
        
            IsRunning = true;
            Timestamp.Value = DateTime.Now;
        
            if (!Directory.Exists(Target.Value.ToString()))
                Directory.CreateDirectory(Target.Value.ToString());

            string[] sourceDirectoriesArray = Directory.GetDirectories((string) Source.Value.ToString(), "*", SearchOption.AllDirectories);

            foreach (string directory in sourceDirectoriesArray)
            {
                string dirToCreate = directory.Replace(Source.Value.ToString(), Target.Value.ToString());
                Directory.CreateDirectory(dirToCreate);
            }

            string[] sourceFilesArray = Directory.GetFiles((string) Source.Value.ToString(), "*.*", SearchOption.AllDirectories);

            foreach (string path in sourceFilesArray)
            {
                BackupJobTask jobTask = new BackupJobTask(this, path, path.Replace((string) Source.Value.ToString(), (string) Target.Value.ToString()));
                BackupJobTasks.Add(jobTask);
            }

            foreach(BackupJobTask jobTask in BackupJobTasks)
                jobTask.ExecuteTask((JobExecutionStrategy.StrategyType) StrategyType.Value);

            TransferTime.Value = BackupJobTasks.Select(x => (long)x.TransferTime.Value).Sum();
            Size.Value = BackupJobTasks.FindAll(x=>x.TransferTime.Value != -1).Select(x => (long)x.Size.Value).Sum();

            IsRunning = false;

            Status = JobExecutionStrategy.ExecutionStatus.Completed;
        }

        public JsonObject JsonSerialize()
        {
            JsonObject jsonObject = new JsonObject();

            jsonObject.Add("Name", Name);
            jsonObject.Add("Source", Source.Value);
            jsonObject.Add("Target", Target.Value);
            return jsonObject;
        }

        public void JsonDeserialize(JsonObject data)
        {
            if (data.ContainsKey("Name"))
                Name = data["Name"]!.ToString();
            else
                throw new Exception("Invalid JSON data: Missing 'Name' property.");

            if (data.ContainsKey("Source"))
                Source.Value = data["Source"]!.ToString();
            else
                throw new Exception("Invalid JSON data: Missing 'Source' property.");

            if (data.ContainsKey("Target"))
                Target.Value = data["Target"]!.ToString();
            else
                throw new Exception("Invalid JSON data: Missing 'Target' property.");
        }

        public XmlElement XmlSerialize(XmlDocument parent)
        {
            XmlElement jobElement = parent.CreateElement("BackupJob");

            jobElement.SetAttribute("Name", Name);
            jobElement.SetAttribute("Source", Source.Value.ToString());
            jobElement.SetAttribute("Target", Target.Value.ToString());

            return jobElement;
        }

        public void XmlDeserialize(XmlElement data)
        {
            if (data.HasAttribute("Name"))
                Name = data.GetAttribute("Name");
            else
                throw new Exception("Invalid XML data: Missing 'Name' attribute.");

            if (data.HasAttribute("Source"))
                Source.Value = data.GetAttribute("Source");
            else
                throw new Exception("Invalid XML data: Missing 'Source' attribute.");

            if (data.HasAttribute("Target"))
                Target.Value = data.GetAttribute("Target");
            else
                throw new Exception("Invalid XML data: Missing 'Target' attribute.");
        }
    }
}
