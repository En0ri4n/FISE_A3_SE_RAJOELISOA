using System;
using System.Diagnostics;
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
        
        public BackupJobTask(BackupJob backupJob, string source, string target) : base(backupJob.Name)
        {
            _backupJob = backupJob;
            Timestamp = DateTime.Now;
            Source = source;
            Target = target;
            Size = new FileInfo(Source).Length;
            TransferTime = -1L;
            EncryptionTime = -1L;
        }

        public override void ExecuteTask(JobExecutionStrategy.StrategyType strategyType)
        {
            Timestamp = DateTime.Now;

            if (_backupJob.IsEncrypted)
            {
                if (File.Exists($"{Target}.encrypted") &&
                    strategyType == JobExecutionStrategy.StrategyType.Differential)
                    if (ExternalEncryptor.IsEncryptorPresent() &&
                        ExternalEncryptor.CheckIfFileMatchEncryptedFile(Source, $"{Target}.encrypted"))
                    {
                        TransferTime = 0L;
                        EncryptionTime = 0L;
                        Status = JobExecutionStrategy.ExecutionStatus.Skipped;
                        _backupJob.OnTaskCompleted(this);
                        Logger.Log(LogLevel.Information,
                            $"[{Name}] Backup job task from {Source} to {Target}.encrypted completed in {TransferTime}ms ({Status})");
                        return;
                    }
            }
            else
            {
                if (File.Exists(Target) && strategyType == JobExecutionStrategy.StrategyType.Differential)
                    if (FilesAreEqual(new FileInfo(Source), new FileInfo(Target)))
                    {
                        TransferTime = 0L;
                        EncryptionTime = 0L;
                        Status = JobExecutionStrategy.ExecutionStatus.Skipped;
                        _backupJob.OnTaskCompleted(this);
                        Logger.Log(LogLevel.Information,
                            $"[{Name}] Backup job task from {Source} to {Target} completed in {TransferTime}ms ({Status})");
                        return;
                    }
            }

            Stopwatch watch = Stopwatch.StartNew();

            try
            {
                if (!_backupJob.IsEncrypted)
                    EncryptionTime = 0L;

                if (ExternalEncryptor.IsEncryptorPresent() && _backupJob.IsEncrypted && ((BackupJobConfiguration) CLEA.EasySaveCore.Core.EasySaveCore.Get().Configuration).ExtensionsToEncrypt.Any(ext => Source.EndsWith(ext)))
                {
                    Stopwatch encryptionWatch = Stopwatch.StartNew();
                    ExternalEncryptor.ProcessFile(Source, $"{Target}.encrypted");
                    encryptionWatch.Stop();
                    EncryptionTime = encryptionWatch.ElapsedMilliseconds;
                }
                else
                {
                    //CopyWithHardThrottle(Source, Target, 512 * 1024 * 1024);
                    CopyFileWithThrottle(Source, Target);
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
            Logger.Log(LogLevel.Information,
                $"[{Name}] Backup job task from {Source} to {Target} completed in {TransferTime}ms ({Status})");
        }

        public static void CopyWithHardThrottle(string sourceFilePath, string targetFilePath, long maxBytesPerSecond)
        {
            const int bufferSize = 128 * 1024; // limite l'usage disque
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
                    if (elapsed < 1000) Thread.Sleep((int)(1000 - elapsed));

                    bytesTransferredThisSecond = 0;
                    secondTimer.Restart();
                }
            }
        }

        private void CopyFileWithThrottle(string source, string target)
        {
            const int bufferSize = 512 * 1024;
            using FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read);
            using FileStream targetStream = new FileStream(target, FileMode.Create, FileAccess.Write);
            
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                targetStream.Write(buffer, 0, bytesRead);
                Thread.Sleep(5);
            }
        }

        public override JsonObject JsonSerialize()
        {
            JsonObject json = new JsonObject
            {
                ["Name"] = Name,
                ["Timestamp"] = Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                ["Source"] = Source,
                ["Target"] = ((BackupJobConfiguration) CLEA.EasySaveCore.Core.EasySaveCore.Get().Configuration).ExtensionsToEncrypt.Any(ext => Source.EndsWith(ext))
                    ? $"{Target}.encrypted"
                    : Target,
                ["Size"] = Size,
                ["FileTransferTime"] = TransferTime == -1 ? -1D : TransferTime / 1000D,
                ["EncryptionTime"] = EncryptionTime == -1 ? -1D : EncryptionTime / 1000D,
                ["Status"] = Status.ToString()
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
            targetElement.InnerText = ((BackupJobConfiguration) CLEA.EasySaveCore.Core.EasySaveCore.Get().Configuration).ExtensionsToEncrypt.Any(ext => Source.EndsWith(ext))
                ? $"{Target}.encrypted"
                : Target;
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
            encryptionTimeElement.InnerText = EncryptionTime == -1 ? "-1" : $"{EncryptionTime / 1000D:F3}";
            jobElement.AppendChild(encryptionTimeElement);

            XmlElement statusElement = document.CreateElement("Status");
            statusElement.InnerText = Status.ToString();
            jobElement.AppendChild(statusElement);

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