using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Jobs.Backup.Configurations;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup
{
    public sealed class BackupJobManager : JobManager
    {
        public BackupJobManager() : base(-1)
        {
            Jobs.CollectionChanged += (sender, args) => OnPropertyChanged(nameof(Jobs));
        }

        public override event PropertyChangedEventHandler? PropertyChanged;
        public override event OnJobInterrupted? JobInterruptedHandler;
        public override event OnMultipleJobCompleted? MultipleJobCompletedHandler;
        public override event OnJobsStopped? JobsStoppedHandler;
        public override event OnJobsPaused? JobsPausedHandler;
        private readonly object _lockObject = new object();
        private static readonly Semaphore SemaphoreObject = new Semaphore(Environment.ProcessorCount, Environment.ProcessorCount);

        public override bool AddJob(IJob job, bool save)
        {
            if ((Jobs.Count >= Size && Size != -1) || Jobs.Any(j => j.Name == job.Name))

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
            IJob? job = Jobs.FirstOrDefault(j => j.Name == name);
            if (job == null)
                throw new Exception($"BackupJob with name {name} not found");

            if (jobJson != null)
                job.JsonDeserialize(jobJson);

            Core.EasySaveCore.Get().Configuration.SaveConfiguration();
        }

        public override bool UpdateJob(string name, IJob? job)
        {
            IJob? existingJob = Jobs.FirstOrDefault(j => j.Name == name);
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
        //protected override void DoJob(IJob job)
        //{
        //    if (!job.CanRunJob())
        //        throw new Exception($"Job {job.Name} cannot be run");

        //    job.Status = JobExecutionStrategy.ExecutionStatus.InProgress;
        //    job.RunJob();
        //    job.Status = job.JobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed)
        //        ? JobExecutionStrategy.ExecutionStatus.Completed
        //        : JobExecutionStrategy.ExecutionStatus.Failed;

        //    Logger.Get().SaveDailyLog(job.JobTasks.Select(task => task).ToList());
        //}

        protected override void DoMultipleJob(ObservableCollection<IJob> jobs)
        {
            CountdownEvent countdown = new CountdownEvent(jobs.Count);

            foreach (IJob job in jobs)
            {
                if (ProcessHelper.IsAnyProcessRunning(((BackupJobConfiguration)Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.ToArray()))
                {
                    job.CompleteJob(JobExecutionStrategy.ExecutionStatus.InterruptedByProcess);
                    JobInterruptedHandler?.Invoke(JobInterruptionReasons.ProcessRunning, job, ((BackupJobConfiguration)Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.FirstOrDefault(ProcessHelper.IsProcessRunning) ?? string.Empty);
                    countdown.Signal();
                    UpdateProperties();
                    return;
                }

                if (!HasEnoughDiskSpace(job.Target, job.Size))
                {
                    job.CompleteJob(JobExecutionStrategy.ExecutionStatus.NotEnoughDiskSpace);
                    JobInterruptedHandler?.Invoke(JobInterruptionReasons.NotEnoughDiskSpace, job, "Not enough disk space on target drive.");
                    countdown.Signal();
                    return;
                }

                job.ClearAndSetupJob();

                Task.Run(() =>
                {
                    SemaphoreObject.WaitOne();

                    job.RunJob(countdown);
                    SemaphoreObject.Release();

                    lock (_lockObject)
                    {
                        Logger.Get().SaveDailyLog(jobs.SelectMany(j => j.JobTasks).ToList());
                    }
                });
            }

            Task.Run(() =>
            {
                countdown.Wait();
                countdown.Dispose();
                UpdateProperties();
                MultipleJobCompletedHandler?.Invoke(jobs);
            });
        }


        public override void PauseJobs(List<IJob> selectedJobs, bool forcePause = false)
        {
            lock (_lockObject)
            {
                foreach (IJob job in selectedJobs)
                {
                    if (!(job is { IsRunning: true })) continue;

                    if (!job.IsPaused || forcePause)
                        job.PauseJob();
                    else
                    {
                        if (ProcessHelper.IsAnyProcessRunning(((BackupJobConfiguration)Core.EasySaveCore.Get().Configuration).ProcessesToBlacklist.ToArray()))
                        {
                            MessageBox.Show(
                                "Blacklisted process detected, all backup jobs have been paused.",
                                "Jobs Paused",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );

                            return;
                        }
                        job.ResumeJob();
                    }
                }
                
                UpdateProperties();
            }
        }
        public override void StopJobs(List<IJob> selectedJobs)
        {
            lock (_lockObject)
            {
                foreach (IJob job in selectedJobs)
                {
                    if (!(job is { IsRunning: true })) continue;

                    job.StopJob();
                }

                UpdateProperties();
            }
        }

        public override void StopJob(string jobName)
        {
            lock (_lockObject)
            {
                IJob? job = Jobs.FirstOrDefault(j => j.Name == jobName);

                if (!(job is { IsRunning: true })) return; 

                job.StopJob();

                UpdateProperties();
            }
        }

        public override void UpdateProperties()
        {
            OnPropertyChanged(nameof(Jobs));
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