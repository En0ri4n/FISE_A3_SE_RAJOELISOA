using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using EasySaveShared.DataStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasySaveShared.Client
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
                    case MessageType.FetchJobs:
                        HandleBackupJobList(message);
                        break;
                    case MessageType.JobDataUpdate2Client:
                        HandleJobDataUpdate(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
                }
            }
        }

        private void HandleJobDataUpdate(NetworkMessage message)
        {
            if (!message.Data.TryGetValue("backupJob", out JToken? jobJson))
                return;

            SharedBackupJob? backupJob = jobJson.ToObject<SharedBackupJob>();
            if (backupJob == null)
                return;

            _jobManager.GetJob(backupJob.Name)?.UpdateData(backupJob);
        }

        private void HandleBackupJobList(NetworkMessage networkMessage)
        {
            if (!networkMessage.Data.TryGetValue("backupJobs", out JToken? backupJobsJson))
                return;
            
            List<SharedBackupJob> backupJobs = new List<SharedBackupJob>();
            JArray backupJobsArray = (JArray) backupJobsJson;
            foreach (JToken jsonNode in backupJobsArray)
            {
                backupJobs.Add(jsonNode.ToObject<SharedBackupJob>()!);
            }

            _jobManager.SetBackupJobs(backupJobs);
        }
    }
}