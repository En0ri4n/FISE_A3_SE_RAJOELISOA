using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;

namespace EasySaveCore.Models
{
    internal class BackupJobTask : JobTask
    {
        public Property<string> Name;
        public Property<DateTime> Timestamp;
        public Property<string> Source;
        public Property<string> Target;
        public Property<long> Size;
        public Property<long> TrasferTime;
        public BackupJob BackupJob;

        public BackupJobTask(BackupJob backupJob) : base(backupJob.Name.Value)
        {
            this.Name = backupJob.Name;
            this.Timestamp = backupJob.Timestamp;
            this.Source = backupJob.Source;
            this.Target = backupJob.Target;
            this.Size = backupJob.Size;
            this.TrasferTime = backupJob.TrasferTime;
            this.BackupJob = backupJob;
        }

        public override void Deserialize(JsonObject data)
        {
            throw new NotImplementedException();
        }

        public override void ExecuteTask()
        {

            string[] SourceDirectoriesArray = Directory.GetDirectories(this.Source.Value, "*", SearchOption.AllDirectories);

            foreach (string directory in SourceDirectoriesArray) {

                string dirToCreate = directory.Replace(this.Source.Value, this.Target.Value);
                Directory.CreateDirectory(dirToCreate);
            }

            string[] SourceFilesArray = Directory.GetFiles(this.Source.Value, "*.*", SearchOption.AllDirectories);

            foreach (string path in SourceFilesArray)
            {
                File.Copy(path, path.Replace(this.Source.Value, this.Target.Value));
            }

        }

        public override JsonObject Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
