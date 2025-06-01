using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using EasySaveRemote.Client;
using EasySaveRemote.Client.DataStructures;
using EasySaveRemote.Client.ViewModel;

namespace EasySaveRemote
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
                NetworkMessage message = NetworkMessage.Create(MessageType.FetchBackupJobList, new JsonObject());
                client.SendMessage(message);
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