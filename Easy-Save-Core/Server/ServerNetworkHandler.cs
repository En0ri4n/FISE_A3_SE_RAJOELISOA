using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Models;
using EasySaveCore.Server.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        }

        public void HandleNetworkMessage(Socket sender, NetworkMessage message)
        {
            // Lock to ensure thread safety when handling messages
            lock (_lockObject)
            {
                switch (message.Type)
                {
                    case MessageType.FetchBackupJobList:
                        HandleFetchBackupJobList(sender);
                        break;
                    case MessageType.BackupJobUpdate:
                        ClientBackupJob? updatedClientBackupJob = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (updatedClientBackupJob == null)
                        {
                            Logger.Log(LogLevel.Error, "Failed to deserialize backup job from message data.");
                            return;
                        }
                        HandleBackupJobUpdate(sender, updatedClientBackupJob);
                        break;
                    case MessageType.BackupJobAdd:
                        ClientBackupJob? addedClientBackupJob = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (addedClientBackupJob == null)
                        {
                            Logger.Log(LogLevel.Error, "Failed to deserialize backup job from message data.");
                            return;
                        }

                        HandleBackupJobAdd(sender, addedClientBackupJob);
                        break;
                    case MessageType.BackupJobRemove:
                        ClientBackupJob? removeClientBackupJob = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (removeClientBackupJob == null)
                        {
                            Logger.Log(LogLevel.Error, "Failed to deserialize backup job from message data.");
                            return;
                        }
                        HandleBackupJobRemove(sender, removeClientBackupJob);
                        break;
                    default:
                        throw new ArgumentException("Unknown message type");
                }
            }
        }

        /// <summary>
        /// Called when a new backup job is added by a client.<br></br>
        /// It transforms the client backup job into a server-side backup job and adds it to the job manager.<br></br>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="backupJob"></param>
        private void HandleBackupJobAdd(Socket sender, ClientBackupJob backupJob)
        {
            _backupJobManager.AddJob(TransformToBackupJob(backupJob), true);
            _networkServer.BroadcastMessage(sender, NetworkMessage.Create(MessageType.BackupJobAdd, CreateJsonObject("backupJob", JsonConvert.SerializeObject(backupJob))));
        }
        private void HandleBackupJobRemove(Socket sender, ClientBackupJob backupJob)
        {
            _backupJobManager.RemoveJob(TransformToBackupJob(backupJob));
            _networkServer.BroadcastMessage(sender, NetworkMessage.Create(MessageType.BackupJobRemove, CreateJsonObject("backupJob", JsonConvert.SerializeObject(backupJob))));
        }
        private void HandleBackupJobUpdate(Socket sender, ClientBackupJob backupJob)
        {
            _backupJobManager.UpdateJob(backupJob.InitialName, TransformToBackupJob(backupJob));
            _networkServer.BroadcastMessage(sender, NetworkMessage.Create(MessageType.BackupJobUpdate, CreateJsonObject("backupJob", JsonConvert.SerializeObject(backupJob))));
        }
        private void HandleFetchBackupJobList(Socket sender)
        {
            // Send the list of backup jobs to the client
            ObservableCollection<IJob> backupJobs = _backupJobManager.GetJobs();
            JsonArray clientBackupJobs = new JsonArray();
            foreach (IJob job in backupJobs)
                clientBackupJobs.Add(TransformToClientBackupJob((BackupJob)job));

            NetworkMessage responseMessage = NetworkMessage.Create(MessageType.FetchBackupJobList, CreateJsonObject("backupJobs", clientBackupJobs));
            _networkServer.SendMessage(sender, responseMessage);
        }

        private BackupJob TransformToBackupJob(ClientBackupJob clientBackupJob)
        {
            return new BackupJob((BackupJobManager)CLEA.EasySaveCore.Core.EasySaveCore.Get().JobManager,
                clientBackupJob.Name, clientBackupJob.Source, clientBackupJob.Target,
                clientBackupJob.StrategyType, clientBackupJob.IsEncrypted);
        }

        private ClientBackupJob TransformToClientBackupJob(BackupJob backupJob)
        {
            return new ClientBackupJob(backupJob.Name, backupJob.Source, backupJob.Target,
                backupJob.StrategyType, backupJob.IsEncrypted);
        }

        private JsonObject CreateJsonObject(string name, JsonNode value)
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.Add(name, value);
            return jsonObject;
        }
    }
}