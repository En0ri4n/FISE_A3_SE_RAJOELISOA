using System;
using EasySaveCore.Server.DataStructures;
using Newtonsoft.Json;

namespace Easy_Save_Remote
{
    public class ClientNetworkHandler
    {
        
        private readonly object _lockObject = new object();
        private readonly NetworkClient _networkClient;

        public ClientNetworkHandler(NetworkClient networkClient)
        {
            _networkClient = networkClient;
        }
        
        public void HandleNetworkMessage(NetworkMessage message)
        {
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
                        // Deserialize the data into a ClientBackupJob object
                        ClientBackupJob? backupJob = JsonConvert.DeserializeObject<ClientBackupJob>(message.Data.ToJsonString());
                        if (backupJob == null)
                        {
                            Console.WriteLine("Failed to deserialize backup job from message data.");
                            return;
                        }
                        // Call the method to handle the addition of the backup job
                        HandleBackupJobAdd(backupJob);
                        break;
                    case MessageType.BackupJobRemove:
                        // HandleBackupJobRemove(message.Data);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, null);
                }
            }
        }

        private void HandleBackupJobAdd(ClientBackupJob backupJob)
        {
            
        }
    }
}