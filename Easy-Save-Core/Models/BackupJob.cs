using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;
using System.Text.Json;
using CLEA.EasySaveCore.utilities;

namespace EasySaveCore.Models
{
    internal class BackupJob : IJob
    {

        public Property<dynamic> Name;
        public Property<dynamic> Timestamp;
        public Property<dynamic> Source;
        public Property<dynamic> Target;
        public Property<dynamic> Size;
        public Property<dynamic> TransferTime;

        public bool IsRunning = false;

        public List<Property<dynamic>> Properties => new List<Property<dynamic>>();

        bool IJob.IsRunning { get => IsRunning; set => IsRunning = value; }

        public List<BackupJobTask> backupJobTasks = new List<BackupJobTask>();

        protected BackupJob(string name, string source, string target)
        {

            Name = new Property<dynamic>("name", name);
            Timestamp = new Property<dynamic>("timestamp", new DateTime());
            Source = new Property<dynamic>("source", source);
            Target = new Property<dynamic>("target", target);
            Size = new Property<dynamic>("size", 0);
            TransferTime = new Property<dynamic>("transferTime", 0);
            Properties.AddRange([Name, Timestamp, Source, Target, Size, TransferTime]);

        }

        public JsonObject Serialize(BackupJobTask backupJobTask)
        {
            string jSonString = JsonSerializer.Serialize(backupJobTask);
            return (JsonObject)jSonString;
        }

        public void Deserialize(JsonObject data)
        {
            JsonSerializer.Deserialize<BackupJobTask>(data);
        }

        public bool CanRunJob()
        {
            return !IsRunning;
        }

        public bool RunJob(JobExecutionStrategy async)//TODO
        {
            Timestamp.Value = DateTime.Now;

            IsRunning = true;

            string[] SourceDirectoriesArray = Directory.GetDirectories(this.Source.Value, "*", SearchOption.AllDirectories);

            foreach (string directory in SourceDirectoriesArray)
            {

                string dirToCreate = directory.Replace(this.Source.Value, this.Target.Value);
                Directory.CreateDirectory(dirToCreate);
            }

            string[] SourceFilesArray = Directory.GetFiles(this.Source.Value, "*.*", SearchOption.AllDirectories);

            foreach (string path in SourceFilesArray)
            {
                BackupJobTask jobTask = new BackupJobTask(this, path, path.Replace(this.Source.Value, this.Target.Value));
                backupJobTasks.Add(jobTask);
            }

            foreach(JobTask jobTask in backupJobTasks)
            {
                jobTask.ExecuteTask();
            }

            this.TransferTime.Value = backupJobTasks.Select(x => (long)x.TransferTime.Value).Sum();
            this.Size.Value = backupJobTasks.FindAll(x=>x.TransferTime.Value != -1).Select(x => (long)x.Size.Value).Sum();

            IsRunning = false;

            return true;
        }

        JsonObject IJsonSerializable.Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
