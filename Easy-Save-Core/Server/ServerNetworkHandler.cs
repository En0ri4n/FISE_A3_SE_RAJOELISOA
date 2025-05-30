using System;
using System.Net.Sockets;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Models;
using EasySaveCore.Server.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EasySaveCore.Server
{
    public class ServerNetworkHandler
    {
        private readonly object _lockObject = new object();
        private readonly NetworkServer _networkServer;

        public ServerNetworkHandler(NetworkServer networkServer)
        {
            _networkServer = networkServer;
        }

        public void HandleNetworkMessage(Socket sender, NetworkMessage message)
        {
            // Lock to ensure thread safety when handling messages
            lock (_lockObject)
            {
                switch (message.Type)
                {
                    case MessageType.BackupJobList:
                        // HandleBackupJobList(message.Data);
                        break;
                    case MessageType.BackupJobUpdate:
                        // HandleBackupJobUpdate(message.Data);
                        break;
                    case MessageType.BackupJobAdd:
                        ClientBackupJob? backupJob =
                            JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (backupJob == null)
                        {
                            Logger.Log(LogLevel.Error, "Failed to deserialize backup job from message data.");
                            return;
                        }

                        HandleBackupJobAdd(sender, backupJob);
                        break;
                    case MessageType.BackupJobRemove:
                        // HandleBackupJobRemove(message.Data);
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
            // Lock to ensure thread safety when adding a job
            CLEA.EasySaveCore.Core.EasySaveCore.Get().JobManager.AddJob(TransformToBackupJob(backupJob), true);
            _networkServer.BroadcastMessage(sender, NetworkMessage.Create(MessageType.BackupJobAdd, JsonConvert.SerializeObject(backupJob)));
        }

        private BackupJob TransformToBackupJob(ClientBackupJob clientBackupJob)
        {
            BackupJob backupJob = new BackupJob((BackupJobManager)CLEA.EasySaveCore.Core.EasySaveCore.Get().JobManager,
                clientBackupJob.Name, clientBackupJob.Source, clientBackupJob.Target,
                clientBackupJob.StrategyType, clientBackupJob.IsEncrypted);

            return backupJob;
        }
    }
}