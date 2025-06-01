using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using EasySaveRemote.Client.DataStructures;

namespace EasySaveRemote.Client
{
    public class ClientBackupJobManager
    {
        private readonly ObservableCollection<ClientBackupJob> _backupJobs;
        public ObservableCollection<ClientBackupJob> BackupJobs => _backupJobs;
        
        public ClientBackupJobManager()
        {
            _backupJobs = new ObservableCollection<ClientBackupJob>();
        }
        
        public void AddBackupJob(ClientBackupJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job), "Backup job cannot be null.");
            
            _backupJobs.Add(job);
        }
        public void RemoveBackupJob(ClientBackupJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job), $"Backup job cannot be null");

            _backupJobs.Remove(job);
        }

        public void SetBackupJobs(List<ClientBackupJob> jobs)
        {
            if (jobs == null) throw new ArgumentNullException(nameof(jobs), $"Backup job cannot be null");
            Application.Current.Dispatcher.Invoke(() =>
            {
                _backupJobs.Clear();
                foreach (ClientBackupJob job in jobs)
                    _backupJobs.Add(job);
            });
        }

        public void UpdateBackupJob(ClientBackupJob job)
        {
            //TODO
            if (job == null) throw new ArgumentNullException(nameof(job), $"Backup job cannot be null");
            _backupJobs[_backupJobs.IndexOf(job)] = job;
        }
    }
}