using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using EasySaveRemote.Client.DataStructures;
using Newtonsoft.Json;

namespace EasySaveRemote.Client
{
    /// <summary>
    /// Handles network messages received by the client from the server.<br/>
    /// This class is responsible for processing different types of messages such as backup job updates, additions, and removals.<br/>
    /// </summary>
    public class ClientNetworkHandler
    {
        
        private readonly object _lockObject = new object();
        private readonly NetworkClient _networkClient;
        private readonly ClientBackupJobManager _jobManager;

        public ClientNetworkHandler(NetworkClient networkClient, ClientBackupJobManager jobManager)
        {
            _networkClient = networkClient;
            _jobManager = jobManager;
        }
        
        /// <summary>
        /// Handles incoming network messages from the server.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void HandleNetworkMessage(NetworkMessage message)
        {
            lock (_lockObject)
            {
                switch (message.Type)
                {
                    case MessageType.FetchBackupJobList:
                        HandleBackupJobList(message);
                        break;
                    case MessageType.BackupJobUpdate:
                        ClientBackupJob? backupJobUpdate = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (backupJobUpdate == null)
                        {
                            Console.WriteLine("Failed to deserialize backup job from message data.");
                            return;
                        }
                        HandleBackupJobUpdate(backupJobUpdate);
                        break;
                    case MessageType.BackupJobAdd:
                        // Deserialize the data into a ClientBackupJob object
                        ClientBackupJob? backupJobAdd = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (backupJobAdd == null)
                        {
                            Console.WriteLine("Failed to deserialize backup job from message data.");
                            return;
                        }
                        // Call the method to handle the addition of the backup job
                        HandleBackupJobAdd(backupJobAdd);
                        break;
                    case MessageType.BackupJobRemove:
                        ClientBackupJob? backupJobRemove = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (backupJobRemove == null)
                        {
                            Console.WriteLine("Failed to deserialize backup job from message data.");
                            return;
                        }
                        HandleBackupJobRemove(backupJobRemove);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
                }
            }
        }

        private void HandleBackupJobAdd(ClientBackupJob backupJob)
        {
            _jobManager.AddBackupJob(backupJob);
        }

        private void HandleBackupJobRemove(ClientBackupJob backupJob)
        {
            _jobManager.RemoveBackupJob(backupJob);
        }

        private void HandleBackupJobList(NetworkMessage networkMessage)
        {
            if (!networkMessage.Data.TryGetPropertyValue("backupJobs", out JsonNode? backupJobsJson) || backupJobsJson == null)
                return;
            
            List<ClientBackupJob> backupJobs = new List<ClientBackupJob>();
            JsonArray backupJobsArray = backupJobsJson.AsArray();
            foreach (JsonNode? jsonNode in backupJobsArray)
            {
                backupJobs.Add(JsonConvert.DeserializeObject<ClientBackupJob>(jsonNode.ToJsonString())!);
            }

            _jobManager.SetBackupJobs(backupJobs);
        }

        private void HandleBackupJobUpdate(ClientBackupJob backupJob)
        {
            _jobManager.UpdateBackupJob(backupJob);
        }
    }
}