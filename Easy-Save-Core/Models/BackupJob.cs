using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;

//TODO : parcours fichiers dans dossier source -> un job task par fichier

namespace EasySaveCore.Models
{
    internal class BackupJob
    {
        public Property<string> Name;
        public Property<DateTime> Timestamp;
        public Property<string> Source;
        public Property<string> Target;
        public Property<long> Size;
        public Property<long> TrasferTime;

        protected BackupJob(string name, string source, string target)
        {
            Name = new Property<string>("name", name) ;
            Timestamp = new Property<DateTime>("timestamp", 0);
            Source = new Property<string>("source", source);
            Target = new Property<string>("target", target);
            Size = new Property<long>("size", 0);
            TrasferTime = new Property<long>("transferTime", 0);
        }

        static void Main()
        {
            var watch = Stopwatch.StartNew();
            BackupJob backupJob = new BackupJob("name", "source", "target");
            BackupJobTask backupJobTask = new BackupJobTask(backupJob);
            backupJobTask.ExecuteTask();
            watch.Stop();
            BackupJob timestamp = watch.ElapsedMilliseconds;
        }
    }
}
