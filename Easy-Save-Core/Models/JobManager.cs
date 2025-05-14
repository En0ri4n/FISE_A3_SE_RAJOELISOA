using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.Models;

public abstract class JobManager<TJob>(int size) where TJob : IJob
{
    protected List<TJob> Jobs { get; } = new List<TJob>(size);
    protected int Size { get; } = size;

    public int JobCount => Jobs.Count;

    public ExecutionFlowType ExecutionFlowType { get; set; } = ExecutionFlowType.Sequential;
    public JobExecutionStrategy.StrategyType Strategy { get; set; } = JobExecutionStrategy.StrategyType.Full;

    public abstract bool AddJob(TJob job, bool save);

    public abstract bool AddJob(JsonObject? jobJson);

    public abstract bool RemoveJob(TJob job);
    
    public void UpdateJob(string name, JsonObject? jobJson)
    {
        var job = Jobs.FirstOrDefault(j => j.Name == name);
        if (job == null)
            throw new Exception($"IJob[{typeof(TJob)}] with name {name} not found");

        if (jobJson != null)
            job.JsonDeserialize(jobJson);
    }
    
    public void UpdateJob(string name, TJob? job)
    {
        var existingJob = Jobs.FirstOrDefault(j => j.Name == name);
        if (existingJob == null)
            throw new Exception($"IJob[{typeof(TJob)}] with name {name} not found");

        if (job != null)
            existingJob.JsonDeserialize(job.JsonSerialize());
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
        var job = Jobs.FirstOrDefault(j => j.Name == name);
        
        if (job == null)
            throw new Exception($"IJob[{typeof(TJob)}] with name {name} not found");
        
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
        DoMultipleJob(jobs.Select(jobName => GetJob(jobName)).ToList());
    }

    public abstract void DoMultipleJob(List<TJob> jobs);
}