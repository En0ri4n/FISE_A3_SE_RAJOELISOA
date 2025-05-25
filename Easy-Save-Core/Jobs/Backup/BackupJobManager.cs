using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup
{
    public class BackupJobManager : JobManager<BackupJob>, INotifyPropertyChanged
    {
        public delegate void OnJobInterrupted(BackupJob job, string processName = "");
        public event OnJobInterrupted? JobInterruptedHandler;

        public BackupJob? CurrentRunningJob { get; private set; }

        public BackupJobManager() : base(-1)
        {
            Jobs.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Jobs));
        }

        public override bool AddJob(BackupJob job, bool save)
        {
            if (job == null || (Jobs.Count >= Size && Size!= -1) || Jobs.Any(j => j.Name == job.Name))

                return false;
            
            Jobs.Add(job);
            if (save)
                BackupJobConfiguration.Get().SaveConfiguration();
            return true;
        }

        public override bool AddJob(JsonObject? jobJson)
        {
            if (jobJson == null)
                return false;

            var job = new BackupJob();
            job.JsonDeserialize(jobJson);
            
            return AddJob(job, false);
        }

        public override bool RemoveJob(BackupJob? job)
        {
            if (job == null || !Jobs.Contains(job))
                return false;
            
            Jobs.Remove(job);
            BackupJobConfiguration.Get().SaveConfiguration();
            return true;
        }
        public override void UpdateJob(string name, JsonObject? jobJson)
        {
            BackupJob job = Jobs.FirstOrDefault(j => j.Name == name);
            if (job == null)
                throw new Exception($"BackupJob with name {name} not found");

            if (jobJson != null)
                job.JsonDeserialize(jobJson);

            BackupJobConfiguration.Get().SaveConfiguration();
        }

        public override bool UpdateJob(string name, BackupJob? job)
        {
            BackupJob existingJob = Jobs.FirstOrDefault(j => j.Name == name);
            if (existingJob == null)
                return false;

            if (job == null)
                return false;

            if (Jobs.Any(j => j.Name == job.Name) && (name != job.Name))
                return false;

            // Sorry for this, but to trigger the CollectionChanged event we need to replace the job in the collection
            Jobs[Jobs.IndexOf(existingJob)] = job;
            
            BackupJobConfiguration.Get().SaveConfiguration();

            return true;
        }


        protected override void DoJob(BackupJob job)
        {
            if (!job.CanRunJob())
                throw new Exception($"Job {job.Name} cannot be run");

            job.Status = JobExecutionStrategy.ExecutionStatus.InProgress;
            job.RunJob();
            job.Status = job.BackupJobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed) ? JobExecutionStrategy.ExecutionStatus.Completed : JobExecutionStrategy.ExecutionStatus.Failed;
            
            Logger.Get().SaveDailyLog(job.BackupJobTasks.Select(task => task).Cast<JobTask>().ToList());
        }

        protected override void DoMultipleJob(ObservableCollection<BackupJob> jobs)
        {
            foreach (BackupJob job in jobs)
                job.ClearTasksAndProgress();
            
            Task.Run(() =>
            {
                foreach (BackupJob job in jobs)
                {
                    CurrentRunningJob = job;
                    if (!ProcessHelper.IsAnyProcessRunning(BackupJobConfiguration.Get().ProcessesToBlacklist.ToArray()))
                    {
                        job.RunJob();
                    }
                    else
                    {
                        job.CompleteJob(JobExecutionStrategy.ExecutionStatus.InterruptedByProcess);
                        JobInterruptedHandler?.Invoke(job, BackupJobConfiguration.Get().ProcessesToBlacklist.FirstOrDefault(ProcessHelper.IsProcessRunning) ?? string.Empty);
                        break;
                    }
                }
                CurrentRunningJob = null;

                Logger.Get().SaveDailyLog(jobs.SelectMany(job => job.BackupJobTasks).Cast<JobTask>().ToList());
            });
        }

        public override void DoAllJobs()
        {
            DoMultipleJob(Jobs);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
