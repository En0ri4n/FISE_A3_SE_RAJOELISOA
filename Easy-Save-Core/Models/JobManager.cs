using System;
using System.Collections.Generic;
using System.Linq;
using CLEA.EasySaveCore.Utilities;
using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.Models
{
    public abstract class JobManager<TJob> where TJob : IJob
    {
        protected List<TJob> Jobs { get; }
        protected int Size { get; }

        public int JobCount => Jobs.Count;

        public ExecutionFlowType ExecutionFlowType { get; set; } = ExecutionFlowType.Sequential;
        public JobExecutionStrategy.StrategyType Strategy { get; set; } = JobExecutionStrategy.StrategyType.Full;
        
        protected JobManager(int size)
        {
            Size = size;
            Jobs = new List<TJob>(size);
        }

        public abstract bool AddJob(TJob job, bool save);

        public abstract bool AddJob(JsonObject? jobJson);

        public abstract bool RemoveJob(TJob job);
        
        public void UpdateJob(string name, JsonObject? jobJson)
        {
            var job = Jobs.FirstOrDefault(j => j.Name == name);
            if (job == null)
                throw new Exception($"IJob[{typeof(TJob)}] with name {name} not found");

            if (jobJson != null)
            {
                job.JsonDeserialize(jobJson);
                EasySaveConfiguration<TJob>.SaveConfiguration();
            }

        }
        
        public void UpdateJob(string name, TJob job)
        {
            var existingJob = Jobs.FirstOrDefault(j => j.Name == name);
            if (existingJob == null)
                throw new Exception($"IJob[{typeof(TJob)}] with name {name} not found");

            if (job != null)
            {
                existingJob.JsonDeserialize(job.JsonSerialize());
                EasySaveConfiguration<TJob>.SaveConfiguration();
            }
        }

        public bool RemoveJob(string name)
        {
            var job = Jobs.FirstOrDefault(j => j.Name == name);
            if (job == null)
                return false;
            
            return RemoveJob(job);
        }

        public bool RemoveAllJobs()
        {
            if (Jobs.Count <= 0)
                return false;

            foreach (var job in Jobs)
                RemoveJob(job);
            
            return true;
        }

        public TJob GetJob(string name)
        {
            TJob job = Jobs.FirstOrDefault(j => j.Name == name);
            
            return job;
        }
        
        public List<TJob> GetJobs()
        {
            return Jobs;
        }
        
        public abstract void DoAllJobs();

        public void DoJob(string name)
        {
            var job = Jobs.FirstOrDefault(j => j.Name == name);
            
            if (job == null)
                throw new Exception($"IJob[{typeof(TJob)}] with name {name} not found");
            
            DoJob(job);
        }

        public abstract void DoJob(TJob job);

        public void DoMultipleJob(List<string> jobs)
        {
            DoMultipleJob(jobs.Select(GetJob).ToList());
        }

        public abstract void DoMultipleJob(List<TJob> jobs);
    }
}