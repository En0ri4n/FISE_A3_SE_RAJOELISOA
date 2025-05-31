using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySaveRemote.Client.Commands;
using EasySaveRemote.Client.DataStructures;
using Newtonsoft.Json;

namespace EasySaveRemote.Client.ViewModel
{
    public class ClientViewModel
    {
        public ClientBackupJobBuilder BackupJobBuilder { get; } = new ClientBackupJobBuilder();
        public ObservableCollection<ClientBackupJob> AvailableBackupJobs => RemoteClient.Get().BackupJobs;
        public ICommand BuildJobCommand { get; set; }
        
        public void InitializeCommands()
        {
            BuildJobCommand = new RelayCommand(isCreation =>
            {
                if(!bool.TryParse(isCreation?.ToString(), out bool isJobCreation))
                    return;
                
                ClientBackupJob clientBackupJob = RemoteClient.Get().ViewModel.BackupJobBuilder.Build();
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(isJobCreation ? MessageType.BackupJobAdd : MessageType.BackupJobUpdate, 
                    JsonConvert.SerializeObject(clientBackupJob)));
            }, _ => true);
        }
    }
}