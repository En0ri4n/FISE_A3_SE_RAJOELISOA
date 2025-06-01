using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Models;
using EasySaveShared.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasySaveCore.Server
{
    public class ServerNetworkHandler
    {
        private readonly NetworkServer _networkServer;
        private readonly BackupJobManager _backupJobManager;
        private readonly object _lockObject = new object();

        public ServerNetworkHandler(NetworkServer networkServer, BackupJobManager backupJobManager)
        {
            _networkServer = networkServer;
            _backupJobManager = backupJobManager;
            
            _backupJobManager.DataUpdatedHandler += (job) =>
            {
                // When a job is updated, send the updated job to all clients
                SharedBackupJob sharedJob = TransformToSharedBackupJob((BackupJob)job);
                _networkServer.BroadcastMessage(NetworkMessage.Create(MessageType.JobDataUpdate2Client, CreateJsonObject("backupJob", JToken.FromObject(sharedJob))));
            };
        }

        public void HandleNetworkMessage(Socket sender, NetworkMessage message)
        {
            // Lock to ensure thread safety when handling messages
            lock (_lockObject)
            {
                switch (message.Type)
                {
                    case MessageType.FetchJobs:
                        HandleFetchBackupJobList();
                        break;
                    case MessageType.UpdateJob2Server:
                        HandleBackupJobUpdate(message);
                        break;
                    case MessageType.AddJob2Server:
                        HandleBackupJobAdd(message);
                        break;
                    case MessageType.RemoveJob2Server:
                        HandleBackupJobRemove(message);
                        break;
                    case MessageType.PauseMultipleJobs2Server:
                        HandleBackupJobPause(message);
                        break;
                    case MessageType.ResumeMultipleJobs2Server:
                        HandleBackupJobResume(message);
                        break;
                    case MessageType.DeleteMultipleJobs2Server:
                        HandleBackupJobDelete(message);
                        break;
                    case MessageType.StartMultipleJobs2Server:
                        HandleBackupJobRun(message);
                        break;
                    case MessageType.StopMultipleJobs2Server:
                        HandleBackupJobStop(message);
                        break;
                    default:
                        throw new ArgumentException("Unknown message type");
                }
            }
        }

        private void HandleBackupJobPause(NetworkMessage message)
        {
            List<string>? jobNames = message.Data.GetValue("jobNames")?.ToObject<List<string>>();
            if (jobNames == null || jobNames.Count == 0)
            {
                Logger.Log(LogLevel.Error, "Received null or empty job names for pause operation.");
                return;
            }

            SendFunctionToMainThread(() => _backupJobManager.PauseJobs(jobNames.Select(name => _backupJobManager.GetJob(name)).ToList()));
            SendJobs();
        }
        
        private void HandleBackupJobResume(NetworkMessage message)
        {
            List<string>? jobNames = message.Data.GetValue("jobNames")?.ToObject<List<string>>();
            if (jobNames == null || jobNames.Count == 0)
            {
                Logger.Log(LogLevel.Error, "Received null or empty job names for resume operation.");
                return;
            }

            SendFunctionToMainThread(() => _backupJobManager.PauseJobs(jobNames.Select(name => _backupJobManager.GetJob(name)).ToList()));
            SendJobs();
        }
        
        private void HandleBackupJobDelete(NetworkMessage message)
        {
            List<string>? jobNames = message.Data.GetValue("jobNames")?.ToObject<List<string>>();
            if (jobNames == null || jobNames.Count == 0)
            {
                Logger.Log(LogLevel.Error, "Received null or empty job names for delete operation.");
                return;
            }

            SendFunctionToMainThread(() =>
            {
                foreach (string jobName in jobNames)
                    _backupJobManager.RemoveJob(jobName);
            });
            SendJobs();
        }
        
        private void HandleBackupJobRun(NetworkMessage message)
        {
            List<string>? jobNames = message.Data.GetValue("jobNames")?.ToObject<List<string>>();
            if (jobNames == null || jobNames.Count == 0)
            {
                Logger.Log(LogLevel.Error, "Received null or empty job names for start operation.");
                return;
            }

            SendFunctionToMainThread(() => _backupJobManager.DoMultipleJob(jobNames));
            SendJobs();
        }
        
        private void HandleBackupJobStop(NetworkMessage message)
        {
            List<string>? jobNames = message.Data.GetValue("jobNames")?.ToObject<List<string>>();
            if (jobNames == null || jobNames.Count == 0)
            {
                Logger.Log(LogLevel.Error, "Received null or empty job names for stop operation.");
                return;
            }

            SendFunctionToMainThread(() => _backupJobManager.StopJobs(jobNames.Select(name => _backupJobManager.GetJob(name)).ToList()));
            SendJobs();
        }

        /// <summary>
        /// Called when a new backup job is added by a client.<br></br>
        /// It transforms the client backup job into a server-side backup job and adds it to the job manager.<br></br>
        /// </summary>
        /// <param name="message"></param>
        private void HandleBackupJobAdd(NetworkMessage message)
        {
            SharedBackupJob? clientBackupJob = JsonConvert.DeserializeObject<SharedBackupJob>(message.Data.GetValue("backupJob")?.ToString()!);
            if (clientBackupJob == null)
            {
                Logger.Log(LogLevel.Error, "Received null backup job from client.");
                return;
            }
            
            
            SendFunctionToMainThread(() => _backupJobManager.AddJob(TransformToBackupJob(clientBackupJob), true));
            SendJobs();
        }
        private void HandleBackupJobRemove(NetworkMessage message)
        {
            SharedBackupJob? clientBackupJob = JsonConvert.DeserializeObject<SharedBackupJob>(message.Data.GetValue("backupJob")?.ToString()!);
            if (clientBackupJob == null)
            {
                Logger.Log(LogLevel.Error, "Received null backup job from client.");
                return;
            }
            
            SendFunctionToMainThread(() => _backupJobManager.RemoveJob(_backupJobManager.GetJob(clientBackupJob.InitialName!)));
            SendJobs();
        }
        private void HandleBackupJobUpdate(NetworkMessage message)
        {
            SharedBackupJob? clientBackupJob = JsonConvert.DeserializeObject<SharedBackupJob>(message.Data.GetValue("backupJob")?.ToString()!);
            SendFunctionToMainThread(() => _backupJobManager.UpdateJob(clientBackupJob?.InitialName!, TransformToBackupJob(clientBackupJob!)));
            SendJobs();
        }
        private void HandleFetchBackupJobList()
        {
            SendJobs();
        }
        
        private void SendFunctionToMainThread(Action action)
        {
            if (Application.Current.Dispatcher.CheckAccess())
                action();
            else
                Application.Current.Dispatcher.Invoke(action);
        }
        
        /// <summary>
        /// Send the list of backup jobs to all connected clients.<br/>
        /// </summary>
        private void SendJobs()
        {
            ObservableCollection<IJob> backupJobs = _backupJobManager.GetJobs();
            List<SharedBackupJob> sharedBackupJobs = backupJobs
                .Select(job => TransformToSharedBackupJob((BackupJob)job))
                .ToList();

            _networkServer.BroadcastMessage(NetworkMessage.Create(MessageType.FetchJobs, CreateJsonObject("backupJobs", sharedBackupJobs)));
        }

        private BackupJob TransformToBackupJob(SharedBackupJob clientBackupJob)
        {
            return new BackupJob((BackupJobManager)CLEA.EasySaveCore.Core.EasySaveCore.Get().JobManager,
                clientBackupJob.Name, clientBackupJob.Source, clientBackupJob.Target,
                Enum.Parse<JobExecutionStrategy.StrategyType>(clientBackupJob.StrategyType.ToString()),
                clientBackupJob.IsEncrypted);
        }

        private SharedBackupJob TransformToSharedBackupJob(BackupJob backupJob)
        {
            return new SharedBackupJob(backupJob.Name, backupJob.Name, backupJob.Source, backupJob.Target,
                Enum.Parse<SharedExecutionStrategyType>(backupJob.StrategyType.ToString()),
                backupJob.IsEncrypted,
                backupJob.Progress,
                Enum.Parse<SharedExecutionStatus>(backupJob.Status.ToString())
            );
        }

        private JObject CreateJsonObject(string name, object value)
        {
            JObject jObject = new JObject { [name] = JToken.FromObject(value) };
            return jObject;
        }
    }
}