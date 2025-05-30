using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Easy_Save_Remote.ViewModel;
using EasySaveCore.Server.DataStructures;
using Newtonsoft.Json;

namespace Easy_Save_Remote
{
    public class RemoteClient
    {
        private static RemoteClient? _instance;
        public NetworkClient NetworkClient { get; private set; }
        public ClientViewModel ViewModel { get; }
        
        private readonly ObservableCollection<ClientBackupJob> _backupJobs;
        public ObservableCollection<ClientBackupJob> BackupJobs => _backupJobs;
        
        private RemoteClient()
        {
            NetworkClient = new NetworkClient();
            ViewModel = new ClientViewModel();
            _backupJobs = new ObservableCollection<ClientBackupJob>();
        }
        
        public void AddBackupJob(ClientBackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job), "Backup job cannot be null.");
            
            _backupJobs.Add(job);
        }

        public static void Initialize()
        {
            if (_instance != null)
                return;
            
            _instance = new RemoteClient();
        }

        public static RemoteClient Get()
        {
            if (_instance == null)
                throw new InvalidOperationException("RemoteClient has not been initialized. Call Initialize() first.");
            
            return _instance;
        }
    }
}