using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasySaveRemote;
using EasySaveRemote.Client.DataStructures;

namespace Easy_Save_Remote.ViewModel
{
    public class ClientViewModel
    {
        public ObservableCollection<ClientBackupJob> AvailableBackupJobs => RemoteClient.Get().BackupJobs;
    }
}