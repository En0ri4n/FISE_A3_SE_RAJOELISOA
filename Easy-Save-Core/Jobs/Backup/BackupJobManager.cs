using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup
{
    public class BackupJobManager : JobManager
    {
        private readonly object _lockObject = new object();

        private bool _isRunning;

        public BackupJobManager() : base(-1)
        {
            Jobs.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Jobs));
        }

        public override bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }
        
        public override event PropertyChangedEventHandler? PropertyChanged;
        public override event OnJobInterrupted? JobInterruptedHandler;
        public override event OnMultipleJobCompleted? MultipleJobCompletedHandler;

        public override bool AddJob(IJob job, bool save)
        {
            if (job == null || (Jobs.Count >= Size && Size != -1) || Jobs.Any(j => j.Name == job.Name))

                return false;

            Jobs.Add(job);
            if (save)
                Core.EasySaveCore.Get().Configuration.SaveConfiguration();
            return true;
        }

        public override bool AddJob(JsonObject? jobJson)
        {
            if (jobJson == null)
                return false;

            BackupJob job = new BackupJob(this);
            job.JsonDeserialize(jobJson);

            return AddJob(job, false);
        }

        public override bool RemoveJob(IJob? job)
        {
            if (job == null || !Jobs.Contains(job))
                return false;

            Jobs.Remove(job);
            Core.EasySaveCore.Get().Configuration.SaveConfiguration();
            return true;
        }

        public override void UpdateJob(string name, JsonObject? jobJson)
        {
            var job = Jobs.FirstOrDefault(j => j.Name == name);
            if (job == null)
                throw new Exception($"BackupJob with name {name} not found");

            if (jobJson != null)
                job.JsonDeserialize(jobJson);

            Core.EasySaveCore.Get().Configuration.SaveConfiguration();
        }

        public override bool UpdateJob(string name, IJob? job)
        {
            var existingJob = Jobs.FirstOrDefault(j => j.Name == name);
            if (existingJob == null)
                return false;

            if (job == null)
                return false;

            if (Jobs.Any(j => j.Name == job.Name) && name != job.Name)
                return false;

            // Sorry for this, but to trigger the CollectionChanged event we need to replace the job in the collection
            Jobs[Jobs.IndexOf(existingJob)] = job;

            Core.EasySaveCore.Get().Configuration.SaveConfiguration();

            return true;
        }


        protected override void DoJob(IJob job)
            //unused
        {
            if (!job.CanRunJob())
                throw new Exception($"Job {job.Name} cannot be run");

            job.Status = JobExecutionStrategy.ExecutionStatus.InProgress;
            job.RunJob();
            job.Status = job.JobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed)
                ? JobExecutionStrategy.ExecutionStatus.Completed
                : JobExecutionStrategy.ExecutionStatus.Failed;

            Logger.Get().SaveDailyLog(job.JobTasks.Select(task => task).Cast<JobTask>().ToList());
        }

        protected override void DoMultipleJob(ObservableCollection<IJob> jobs)
        {
            IsRunning = true;

            foreach (var job in jobs)
                job.ClearAndSetupJob();

            Task.Run(() =>
            {
                foreach (var job in jobs)
                {
                    string targetPath = job.Target;
                    if (!HasEnoughDiskSpace(targetPath, job.Size))
                    {
                        IsRunning = false;
                        job.CompleteJob(JobExecutionStrategy.ExecutionStatus.NotEnoughDiskSpace);
                        JobInterruptedHandler?.Invoke(JobInterruptionReasons.NotEnoughDiskSpace, job,
                            "Not enough disk space on target drive.");
                        return;
                    }

                    if (!ProcessHelper.IsAnyProcessRunning(((BackupJobConfiguration) Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.ToArray()))
                    {
                        job.RunJob();
                    }
                    else
                    {
                        IsRunning = false;
                        job.CompleteJob(JobExecutionStrategy.ExecutionStatus.InterruptedByProcess);
                        JobInterruptedHandler?.Invoke(JobInterruptionReasons.ProcessRunning, job,
                            ((BackupJobConfiguration) Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist
                                .FirstOrDefault(ProcessHelper.IsProcessRunning) ?? string.Empty);
                        return;
                    }
                }

                Logger.Get().SaveDailyLog(jobs.SelectMany(job => job.JobTasks).ToList());

                lock (_lockObject)
                {
                    IsRunning = false;
                    MultipleJobCompletedHandler?.Invoke(jobs);
                }
            });
        }

        public override void DoAllJobs()
        {
            DoMultipleJob(Jobs);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}