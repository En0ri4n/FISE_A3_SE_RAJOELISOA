using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.Models
{
    public abstract class JobManager : INotifyPropertyChanged
    {
        public abstract event PropertyChangedEventHandler? PropertyChanged;
        public abstract event OnJobInterrupted? JobInterruptedHandler;
        public abstract event OnMultipleJobCompleted? MultipleJobCompletedHandler;
        
        public delegate void OnJobInterrupted(JobInterruptionReasons reason, IJob job, string processName = "");
        public delegate void OnMultipleJobCompleted(ObservableCollection<IJob> jobs);
        
        protected JobManager(int size)
        {
            Size = size;
            Jobs = new ObservableCollection<IJob>();
        }

        protected ObservableCollection<IJob> Jobs { get; }
        protected int Size { get; }
        
        public abstract bool IsRunning { get; set; }

        public int JobCount => Jobs.Count;

        public ExecutionFlowType ExecutionFlowType { get; set; } = ExecutionFlowType.Sequential;
        public JobExecutionStrategy.StrategyType Strategy { get; set; } = JobExecutionStrategy.StrategyType.Full;

        public abstract bool AddJob(IJob job, bool save);

        public abstract bool AddJob(JsonObject? jobJson);

        public abstract bool RemoveJob(IJob job);

        public abstract void UpdateJob(string name, JsonObject? jobJson);

        public abstract bool UpdateJob(string name, IJob job);

        public bool RemoveJob(string name)
        {
            IJob? job = Jobs.FirstOrDefault(j => j.Name == name);
            return job != null && RemoveJob(job);
        }

        public bool RemoveAllJobs()
        {
            if (Jobs.Count <= 0)
                return false;

            foreach (IJob job in Jobs)
                RemoveJob(job);

            return true;
        }

        public IJob GetJob(string name)
        {
            return Jobs.FirstOrDefault(j => j.Name == name);
        }

        public ObservableCollection<IJob> GetJobs()
        {
            return Jobs;
        }

        public abstract void DoAllJobs();

        public void DoJob(string name)
        {
            IJob? job = Jobs.FirstOrDefault(j => j.Name == name);

            if (job == null)
                throw new Exception($"IJob[{typeof(IJob)}] with name {name} not found");

            DoJob(job);
        }

        protected abstract void DoJob(IJob job);

        public void DoMultipleJob(List<string> jobs)
        {
            DoMultipleJob(new ObservableCollection<IJob>(jobs.Select(GetJob)));
        }

        protected abstract void DoMultipleJob(ObservableCollection<IJob> jobs);

        protected bool HasEnoughDiskSpace(string path, long minimumBytesRequired)
        {
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(path)!);
                return drive.AvailableFreeSpace >= minimumBytesRequired;
            }
            catch
            {
                return false;
            }
        }

        public abstract void PauseMultipleJobs(List<string> jobNames);
    }

    public enum JobInterruptionReasons
    {
        NotEnoughDiskSpace,
        ProcessRunning
    }
}