using System;
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

        public ClientNetworkHandler(NetworkClient networkClient)
        {
            _networkClient = networkClient;
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
                    case MessageType.BackupJobList:
                        ClientBackupJob? backupJobList = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (backupJobList == null)
                        {
                            Console.WriteLine("Failed to deserialize backup job from message data.");
                            return;
                        }
                        HandleBackupJobList(backupJobList);
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
            RemoteClient.Get().AddBackupJob(backupJob);
        }

        private void HandleBackupJobRemove(ClientBackupJob backupJob)
        {
            RemoteClient.Get().RemoveBackupJob(backupJob);
        }

        private void HandleBackupJobList(ClientBackupJob backupJobList)
        {
            RemoteClient.Get().ListBackupJob(backupJobList);
        }

        private void HandleBackupJobUpdate(ClientBackupJob backupJob)
        {
            RemoteClient.Get().UpdateBackupJob(backupJob);
        }
    }
}