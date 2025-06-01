using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using EasySaveShared.DataStructures;

namespace EasySaveShared.Client
{
    public class ClientBackupJobManager
    {
        private readonly ObservableCollection<SharedBackupJob> _backupJobs;
        public ObservableCollection<SharedBackupJob> BackupJobs => _backupJobs;
        
        public ClientBackupJobManager()
        {
            _backupJobs = new ObservableCollection<SharedBackupJob>();
        }

        public void SetBackupJobs(List<SharedBackupJob> jobs)
        {
            if (jobs == null) throw new ArgumentNullException(nameof(jobs), $"Backup job cannot be null");
            Application.Current.Dispatcher.Invoke(() =>
            {
                _backupJobs.Clear();
                foreach (SharedBackupJob job in jobs)
                    _backupJobs.Add(job);
            });
        }
        
        public SharedBackupJob? GetJob(string? name)
        {
            foreach(SharedBackupJob job in _backupJobs)
            {
                if (job.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return job;
            }
            return null;
        }
    }
}