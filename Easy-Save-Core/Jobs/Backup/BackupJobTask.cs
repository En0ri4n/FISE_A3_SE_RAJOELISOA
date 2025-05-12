using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;

namespace EasySaveCore.Models
{
    public class BackupJobTask : JobTask
    {
        public Property<dynamic> Timestamp;
        public Property<dynamic> Source;
        public Property<dynamic> Target;
        public Property<dynamic> Size;
        public Property<dynamic> TransferTime;

        public BackupJobTask(BackupJob backupJob, string source, string target) : base(backupJob.Name)
        {
            Timestamp = new Property<dynamic>("timestamp", new DateTime());
            Source = new Property<dynamic>("source", source);
            Target = new Property<dynamic>("target", target);
            Size = new Property<dynamic>("size", 0);
            TransferTime = new Property<dynamic>("transferTime", -1);
            GetProperties().AddRange([Timestamp, Source, Target, Size, TransferTime]);
        }

        public override JobExecutionStrategy.ExecutionStatus ExecuteTask(JobExecutionStrategy.StrategyType strategyType)
        {
            Timestamp.Value = DateTime.Now;
            Size.Value = new FileInfo(Source.Value).Length;

            if (File.Exists(Target.Value)
                && FilesAreEqual_Hash(new FileInfo (Source.Value), new FileInfo(Target.Value))
                && strategyType == JobExecutionStrategy.StrategyType.Differential)
            {
                    return JobExecutionStrategy.ExecutionStatus.Skipped;
            }

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                File.Copy(Source.Value, Target.Value);
            }
            catch (Exception e)
            {
                return JobExecutionStrategy.ExecutionStatus.Failed;
            }

            watch.Stop();
            TransferTime.Value = watch.ElapsedMilliseconds;
            return JobExecutionStrategy.ExecutionStatus.Completed;
        }

        public override JsonObject JsonSerialize()
        {
            JsonObject json = new JsonObject
            {
                ["Name"] = Name,
                ["Timestamp"] = Timestamp.Value,
                ["Source"] = Source.Value,
                ["Target"] = Target.Value,
                ["Size"] = Size.Value,
                ["TransferTime"] = TransferTime.Value
            };
            return json;
        }

        public override void JsonDeserialize(JsonObject data)
        {
            throw new NotImplementedException("This method should not be called.");
        }

        public override XmlElement XmlSerialize()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement jobElement = doc.CreateElement("BackupJobTask");

            jobElement.SetAttribute("Name", Name);
            jobElement.SetAttribute("Source", Source.Value);
            jobElement.SetAttribute("Target", Target.Value);
            jobElement.SetAttribute("Size", Size.Value.ToString());
            jobElement.SetAttribute("TransferTime", TransferTime.Value.ToString());
            jobElement.SetAttribute("Timestamp", Timestamp.Value.ToString());

            return jobElement;
        }

        public override void XmlDeserialize(XmlElement data)
        {
            throw new NotImplementedException("This method should not be called.");
        }

        private static bool FilesAreEqual_Hash(FileInfo first, FileInfo second)
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
    }
}
