using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup
{
    public sealed class BackupJobManager : JobManager
    {
        private bool _isRunning;

        public BackupJobManager() : base(-1)
        {
            Jobs.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Jobs));
            JobInterruptedHandler += (reason, job, name) => IsRunning = false;
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
        private readonly object _lockObject = new object();
        private static readonly Semaphore _semaphoreObject = new Semaphore(3, 3);

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


        // Unused
        protected override void DoJob(IJob job)
        {
            if (!job.CanRunJob())
                throw new Exception($"Job {job.Name} cannot be run");

            job.Status = JobExecutionStrategy.ExecutionStatus.InProgress;
            job.RunJob();
            job.Status = job.JobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed)
                ? JobExecutionStrategy.ExecutionStatus.Completed
                : JobExecutionStrategy.ExecutionStatus.Failed;

            Logger.Get().SaveDailyLog(job.JobTasks.Select(task => task).ToList());
        }

        protected override void DoMultipleJob(ObservableCollection<IJob> jobs)
        {
            //TODO CHECK ERROR HANDLING (PROBABLY WRONG HERE)
            IsRunning = true;
            int jobsUnfinished = jobs.Count();
            foreach (BackupJob job in jobs)
            {
                if (ProcessHelper.IsAnyProcessRunning(((BackupJobConfiguration)Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.ToArray()))
                {
                    job.CompleteJob(JobExecutionStrategy.ExecutionStatus.InterruptedByProcess);
                    JobInterruptedHandler?.Invoke(JobInterruptionReasons.ProcessRunning, job, ((BackupJobConfiguration)Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.FirstOrDefault(ProcessHelper.IsProcessRunning) ?? string.Empty);
                    IsRunning = false;
                    return;
                }
                job.ClearAndSetupJob();
                Task.Run(() =>
                {
                    _semaphoreObject.WaitOne();

                    string targetPath = job.Target;
                    if (!HasEnoughDiskSpace(targetPath, job.Size))
                    {
                        job.CompleteJob(JobExecutionStrategy.ExecutionStatus.NotEnoughDiskSpace);
                        JobInterruptedHandler?.Invoke(JobInterruptionReasons.NotEnoughDiskSpace, job, "Not enough disk space on target drive.");
                        _semaphoreObject.Release();
                        IsRunning = false;
                        return;
                    }
                    job.RunJob();
                    _semaphoreObject.Release();
                    jobsUnfinished--;
                    lock (_lockObject)
                    {
                        Logger.Get().SaveDailyLog(jobs.SelectMany(job => job.JobTasks).Cast<JobTask>().ToList());
                    }
                });

            }
            //this wait until all jobs are finished without blocking the main program. A bit ugly
            Task.Run(() => //TODO prettier way for this ???
            {
                while (jobsUnfinished != 0) { }
                IsRunning = false;
                MultipleJobCompletedHandler?.Invoke(jobs);
            });
        }

        // TODO: Pause all instead of choosing which ones to pause, so it's easier to manage
        public override void PauseMultipleJobs(List<string> jobNames)
        {
            lock (_lockObject)
            {
                foreach (var jobName in jobNames)
                {
                    IJob? job = Jobs.FirstOrDefault(j => j.Name == jobName);
                    if (!(job is { IsRunning: true })) continue;

                    if (!job.IsPaused)
                        job.PauseJob();
                    else
                        job.ResumeJob();
                    OnPropertyChanged(nameof(Jobs));
                }
            }
        }

        public override void DoAllJobs()
        {
            DoMultipleJob(Jobs);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}