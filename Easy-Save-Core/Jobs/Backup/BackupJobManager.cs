using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup
{
    public class BackupJobManager : JobManager<BackupJob>
    {
        public delegate void OnJobInterrupted(BackupJob job);
        public event OnJobInterrupted? JobInterruptedHandler;

        public BackupJob? CurrentRunningJob { get; private set; }

        public BackupJobManager() : base(-1)
        {
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
            var job = Jobs.FirstOrDefault(j => j.Name == name);
            if (job == null)
                throw new Exception($"BackupJob with name {name} not found");

            if (jobJson != null)
                job.JsonDeserialize(jobJson);

            BackupJobConfiguration.Get().SaveConfiguration();
        }

        public override void UpdateJob(string name, BackupJob? job)
        {
            var existingJob = Jobs.FirstOrDefault(j => j.Name == name);
            if (existingJob == null)
                throw new Exception($"BackupJob with name {name} not found");

            if (job != null)
                existingJob.JsonDeserialize(job.JsonSerialize());

            BackupJobConfiguration.Get().SaveConfiguration();
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

        protected override void DoMultipleJob(List<BackupJob> jobs)
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
                        JobInterruptedHandler?.Invoke(job);
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
    }
}
