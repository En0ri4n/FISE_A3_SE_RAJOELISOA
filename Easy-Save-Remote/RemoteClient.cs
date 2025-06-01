using System;
using EasySaveShared.Client;
using EasySaveShared.Client.ViewModel;
using EasySaveShared.DataStructures;

namespace EasySaveShared
{
    /// <summary>
    /// Singleton class representing the remote client.<br/>
    /// It manages the network client, view model, and backup jobs.<br/>
    /// </summary>
    public class RemoteClient
    {
        private static RemoteClient? _instance;
        public NetworkClient NetworkClient { get; private set; }
        public ClientViewModel ViewModel { get; }
        
        public ClientBackupJobManager BackupJobManager { get; }
        
        private RemoteClient()
        {
            _instance = this;
            BackupJobManager = new ClientBackupJobManager();
            NetworkClient = new NetworkClient();
            ViewModel = new ClientViewModel();
            
            ViewModel.InitializeCommands();
            
            NetworkClient.OnConnected += (client) =>
            {
                // Fetch the backup job list from the server when connected
                client.SendMessage(NetworkMessage.Create(MessageType.FetchJobs));
            };
        }

        public static void Initialize()
        {
            if (_instance != null)
                return;
            
            new RemoteClient();
        }

        public static RemoteClient Get()
        {
            if (_instance == null)
                throw new InvalidOperationException("RemoteClient has not been initialized. Call Initialize() first.");
            
            return _instance;
        }
    }
}