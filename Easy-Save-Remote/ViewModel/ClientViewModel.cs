using System.Collections.Generic;
using System.Collections.ObjectModel;
using EasySaveCore.Server.DataStructures;

namespace Easy_Save_Remote.ViewModel
{
    public class ClientViewModel
    {
        public ObservableCollection<ClientBackupJob> AvailableBackupJobs => RemoteClient.Get().BackupJobs;
    }
}