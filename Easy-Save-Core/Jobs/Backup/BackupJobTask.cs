using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using CLEA.Encryptor;
using Microsoft.Extensions.Logging;

namespace EasySaveCore.Models
{
    public class BackupJobTask : JobTask
    {
        private readonly BackupJob _backupJob;
        public Property<dynamic> Timestamp;
        public Property<dynamic> Source;
        public Property<dynamic> Target;
        public Property<dynamic> Size;
        public Property<dynamic> TransferTime;
        public Property<dynamic> EncryptionTime;

        public BackupJobTask(BackupJob backupJob, string source, string target) : base(backupJob.Name)
        {
            _backupJob = backupJob;
            Timestamp = new Property<dynamic>("timestamp", new DateTime());
            Source = new Property<dynamic>("source", source);
            Target = new Property<dynamic>("target", target);
            Size = new Property<dynamic>("size", 0);
            TransferTime = new Property<dynamic>("transferTime", -1);
            EncryptionTime = new Property<dynamic>("encryptionTime", -1);
            GetProperties().AddRange(new List<Property<dynamic>>()
            {
                Timestamp,
                Source,
                Target,
                Size,
                TransferTime
            });
        }

        public override void ExecuteTask(JobExecutionStrategy.StrategyType strategyType)
        {
            Timestamp.Value = DateTime.Now;
            Size.Value = new FileInfo(Source.Value).Length;

            if (File.Exists(Target.Value)
                && FilesAreEqual(new FileInfo ((string) Source.Value.ToString()), new FileInfo((string) Target.Value.ToString()))
                && strategyType == JobExecutionStrategy.StrategyType.Differential)
            {
                    Status = JobExecutionStrategy.ExecutionStatus.Skipped;
                    _backupJob.OnTaskCompleted(this);
                    CLEA.EasySaveCore.Utilities.Logger.Log(level: LogLevel.Information, $"[{Name}] Backup job task from {Source.Value} to {Target.Value} completed in {TransferTime.Value}ms ({Status})");
                    return;
            }

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                if (EasySaveConfiguration<BackupJob>.isEncryptorLoaded() && EasySaveConfiguration<BackupJob>.Get().ExtensionsToEncrypt.Any(ext => Source.Value.EndsWith(ext)))
                {
                    Stopwatch encryptionWatch = Stopwatch.StartNew();
                    Encryptor.Get().ProcessFile(Source.Value.ToString(), Target.Value.ToString());
                    encryptionWatch.Stop();
                    EncryptionTime.Value = encryptionWatch.ElapsedMilliseconds;
                }
                else
                {
                    File.Copy((string) Source.Value.ToString(), (string) Target.Value.ToString(), true);
                }
            }
            catch (Exception e)
            {
                Status = JobExecutionStrategy.ExecutionStatus.Failed;
                return;
            }

            watch.Stop();
            TransferTime.Value = watch.ElapsedMilliseconds;
            Status = JobExecutionStrategy.ExecutionStatus.Completed;
            _backupJob.OnTaskCompleted(this);
            CLEA.EasySaveCore.Utilities.Logger.Log(level: LogLevel.Information, $"[{Name}] Backup job task from {Source.Value} to {Target.Value} completed in {TransferTime.Value}ms ({Status})");
        }

        public override JsonObject JsonSerialize()
        {
            JsonObject json = new JsonObject
            {
                ["Name"] = Name,
                ["Timestamp"] = ((DateTime) Timestamp.Value).ToString("dd/MM/yyyy HH:mm:ss"),
                ["Source"] = Source.Value,
                ["Target"] = Target.Value,
                ["Size"] = (long) Size.Value,
                ["FileTransferTime"] = (double) TransferTime.Value / 1000D,
                ["EncryptionTime"] = (double) EncryptionTime.Value / 1000D
            };
            return json;
        }

        public override void JsonDeserialize(JsonObject data)
        {
            throw new NotImplementedException("This method should not be called.");
        }

        public override XmlElement XmlSerialize(XmlDocument document)
        {
            XmlElement jobElement = document.CreateElement("BackupJobTask");

            XmlElement nameElement = document.CreateElement("Name");
            nameElement.InnerText = Name;
            jobElement.AppendChild(nameElement);

            XmlElement sourceElement = document.CreateElement("Source");
            sourceElement.InnerText = Source.Value;
            jobElement.AppendChild(sourceElement);

            XmlElement targetElement = document.CreateElement("Target");
            targetElement.InnerText = Target.Value;
            jobElement.AppendChild(targetElement);

            XmlElement sizeElement = document.CreateElement("Size");
            sizeElement.InnerText = Size.Value.ToString();
            jobElement.AppendChild(sizeElement);

            XmlElement fileTransferTimeElement = document.CreateElement("FileTransferTime");
            fileTransferTimeElement.InnerText = ((double)TransferTime.Value / 1000D).ToString(CultureInfo.InvariantCulture);
            jobElement.AppendChild(fileTransferTimeElement);

            XmlElement timestampElement = document.CreateElement("Timestamp");
            timestampElement.InnerText = ((DateTime)Timestamp.Value).ToString("dd/MM/yyyy HH:mm:ss");
            jobElement.AppendChild(timestampElement);
            
            XmlElement encryptionTimeElement = document.CreateElement("EncryptionTime");
            encryptionTimeElement.InnerText = ((double)EncryptionTime.Value / 1000D).ToString(CultureInfo.InvariantCulture);
            jobElement.AppendChild(encryptionTimeElement);

            return jobElement;
        }

        public override void XmlDeserialize(XmlElement data)
        {
            throw new NotImplementedException("This method should not be called.");
        }

        private static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            return first.Length == second.Length && first.Name == second.Name;
        }
    }
}
