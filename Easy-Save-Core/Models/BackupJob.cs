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

        public Property<string> Name;
        public Property<DateTime> Timestamp;
        public Property<string> Source;
        public Property<string> Target;
        public Property<long> Size;
        public Property<long> TrasferTime;

        public bool IsRunning => throw new NotImplementedException();

        public List<Property<dynamic>> Properties => throw new NotImplementedException();

        protected BackupJob(string name, string source, string target)
        {

            Name = new Property<string>("name", name) ;
            Timestamp = new Property<DateTime>("timestamp", new DateTime());
            Source = new Property<string>("source", source);
            Target = new Property<string>("target", target);
            Size = new Property<long>("size", 0);
            TrasferTime = new Property<long>("transferTime", 0);

        }

        public static JsonObject Serialize(BackupJobTask backupJobTask)
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
            throw new NotImplementedException();
        }

        public bool RunJob(bool async)
        {
            var watch = Stopwatch.StartNew();
            BackupJob backupJob = new BackupJob("name", "source", "target");
            BackupJobTask backupJobTask = new BackupJobTask(backupJob);
            backupJobTask.ExecuteTask();
            watch.Stop();
            BackupJob transferTime = watch.ElapsedMilliseconds;
            return true;
        }

        JsonObject IJsonSerializable.Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
