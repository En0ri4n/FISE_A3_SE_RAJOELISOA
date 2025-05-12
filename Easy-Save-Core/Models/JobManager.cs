using System.Text.Json.Nodes;

namespace CLEA.EasySaveCore.Models;

public abstract class JobManager<TJob>(int size) where TJob : IJob
{
    protected List<TJob> Jobs { get; } = new List<TJob>(size);
    protected int Size { get; } = size;

    public abstract bool AddJob(TJob job);

    public abstract bool AddJob(JsonObject? jobJson);

    public abstract bool RemoveJob(TJob job);

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
    
    protected abstract void DoAllJobs(ExecutionFlowType flowType, JobExecutionStrategy.StrategyType strategy);
    
    protected abstract void DoJob(string name, JobExecutionStrategy.StrategyType strategy);
}