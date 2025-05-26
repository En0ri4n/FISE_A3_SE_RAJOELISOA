using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Xml;
using CLEA.EasySaveCore.External;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
using Microsoft.Extensions.Logging;

namespace EasySaveCore.Models
{
    public class BackupJobTask : JobTask
    {
        private readonly BackupJob _backupJob;
        private DateTime _timestamp;
        private string _source;
        private string _target;
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
        

        public BackupJobTask(BackupJob backupJob, string source, string target) : base(backupJob.Name)
        {
            _backupJob = backupJob;
            _timestamp = DateTime.Now;
            _source = source;
            _target = target;
            _size = new FileInfo(Source).Length;
            _transferTime = -1L;
            _encryptionTime = -1L;
        }

        public override void ExecuteTask(JobExecutionStrategy.StrategyType strategyType)
        {
            Timestamp = DateTime.Now;

            if (File.Exists(Target)
                && FilesAreEqual(new FileInfo (Source), new FileInfo(Target))
                && strategyType == JobExecutionStrategy.StrategyType.Differential)
            {
                    Status = JobExecutionStrategy.ExecutionStatus.Skipped;
                    _backupJob.OnTaskCompleted(this);
                    Logger.Log(level: LogLevel.Information, $"[{Name}] Backup job task from {Source} to {Target} completed in {TransferTime}ms ({Status})");
                    return;
            }

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                if (!_backupJob.IsEncrypted)
                    EncryptionTime = 0L;
                
                if (ExternalEncryptor.IsEncryptorPresent() && _backupJob.IsEncrypted && BackupJobConfiguration.Get().ExtensionsToEncrypt.Any(ext => Source.EndsWith(ext)))
                {
                    Stopwatch encryptionWatch = Stopwatch.StartNew();
                    ExternalEncryptor.ProcessFile(ExternalEncryptor.GetEncryptionKey(), Source, $"{Target}.encrypted"); // TODO: Add key from configuration
                    encryptionWatch.Stop();
                    EncryptionTime = encryptionWatch.ElapsedMilliseconds;
                }
                else
                {
                    CopyWithHardThrottle(Source, Target, 128 * 1024 * 1024);
                }
            }
            catch (Exception e)
            {
                Status = JobExecutionStrategy.ExecutionStatus.Failed;
                return;
            }

            watch.Stop();
            TransferTime = watch.ElapsedMilliseconds;
            Status = JobExecutionStrategy.ExecutionStatus.Completed;
            _backupJob.OnTaskCompleted(this);
            Logger.Log(level: LogLevel.Information, $"[{Name}] Backup job task from {Source} to {Target} completed in {TransferTime}ms ({Status})");
        }
        
        public static void CopyWithHardThrottle(string sourceFilePath, string targetFilePath, long maxBytesPerSecond)
        {
            const int bufferSize = 256 * 1024; // limite l'usage disque
            byte[] buffer = new byte[bufferSize];
        
            using FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using FileStream targetStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using BufferedStream bufferedSource = new BufferedStream(sourceStream, bufferSize);
            using BufferedStream bufferedTarget = new BufferedStream(targetStream, bufferSize);
        
            long bytesRead;
            long bytesTransferredThisSecond = 0;
            Stopwatch secondTimer = Stopwatch.StartNew();
        
            while ((bytesRead = bufferedSource.Read(buffer, 0, buffer.Length)) > 0)
            {
                bufferedTarget.Write(buffer, 0, (int)bytesRead);
                bufferedSource.Flush();
                bufferedTarget.Flush();
                bytesTransferredThisSecond += bytesRead;
        
                Thread.Sleep(2);
        
                if (bytesTransferredThisSecond >= maxBytesPerSecond)
                {
                    long elapsed = secondTimer.ElapsedMilliseconds;
                    if (elapsed < 1000)
                    {
                        Thread.Sleep((int)(1000 - elapsed));
                    }
                    bytesTransferredThisSecond = 0;
                    secondTimer.Restart();
                }
            }
        }


        public override JsonObject JsonSerialize()
        {
            JsonObject json = new JsonObject
            {
                ["Name"] = Name,
                ["Timestamp"] = Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                ["Source"] = Source,
                ["Target"] = Target,
                ["Size"] = Size,
                ["FileTransferTime"] = TransferTime == -1 ? -1D : TransferTime / 1000D,
                ["EncryptionTime"] = EncryptionTime == -1 ? -1D : EncryptionTime / 1000D
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
            sourceElement.InnerText = Source;
            jobElement.AppendChild(sourceElement);

            XmlElement targetElement = document.CreateElement("Target");
            targetElement.InnerText = Target;
            jobElement.AppendChild(targetElement);

            XmlElement sizeElement = document.CreateElement("Size");
            sizeElement.InnerText = Size.ToString();
            jobElement.AppendChild(sizeElement);

            XmlElement fileTransferTimeElement = document.CreateElement("FileTransferTime");
            fileTransferTimeElement.InnerText = TransferTime == -1 ? "-1" : $"{TransferTime / 1000D:F3}";
            jobElement.AppendChild(fileTransferTimeElement);

            XmlElement timestampElement = document.CreateElement("Timestamp");
            timestampElement.InnerText = Timestamp.ToString("dd/MM/yyyy HH:mm:ss");
            jobElement.AppendChild(timestampElement);
            
            XmlElement encryptionTimeElement = document.CreateElement("EncryptionTime");
            encryptionTimeElement.InnerText = (EncryptionTime == -1 ? "-1" : $"{EncryptionTime / 1000D:F3}");
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
