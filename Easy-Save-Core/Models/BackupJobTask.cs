using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using CLEA.EasySaveCore.Models;

namespace EasySaveCore.Models
{
    internal class BackupJobTask : JobTask
    {
        public Property<dynamic> Timestamp;
        public Property<dynamic> Source;
        public Property<dynamic> Target;
        public Property<dynamic> Size;
        public Property<dynamic> TransferTime;
        public BackupJob BackupJob;

        public BackupJobTask(BackupJob backupJob, string source, string target) : base((string)backupJob.Name.Value)
        {
            Timestamp = new Property<dynamic>("timestamp", new DateTime());
            Source = new Property<dynamic>("source", source);
            Target = new Property<dynamic>("target", target);
            Size = new Property<dynamic>("size", 0);
            TransferTime = new Property<dynamic>("transferTime", -1);
            _properties.AddRange([Timestamp, Source, Target, Size, TransferTime]);
        }

        public override void Deserialize(JsonObject data)
        {
            throw new NotImplementedException();
        }

        public override void ExecuteTask()
        {
            Timestamp.Value = DateTime.Now;
            Size.Value = new FileInfo(Source.Value).Length;

            if (File.Exists(Target.Value)) { 
                if(FilesAreEqual_Hash(new FileInfo (Source.Value), new FileInfo(Target.Value))){
                    return;//TODO check for strategy
                }
            }

            Stopwatch watch = Stopwatch.StartNew();

            File.Copy(Source.Value, Target.Value);

            watch.Stop();
            long transferTime = watch.ElapsedMilliseconds;
            this.TransferTime.Value = transferTime;


        }

        static bool FilesAreEqual_Hash(FileInfo first, FileInfo second)
        {
            byte[] firstHash = MD5.Create().ComputeHash(first.OpenRead());
            byte[] secondHash = MD5.Create().ComputeHash(second.OpenRead());

            for (int i = 0; i < firstHash.Length; i++)
            {
                if (firstHash[i] != secondHash[i])
                    return false;
            }
            return true;
        }

        public override JsonObject Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
