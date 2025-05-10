using CLEA.EasySaveCore.utilities;

namespace CLEA.EasySaveCore.Models;

public abstract class JobManager<T> where T : IJob
{
    protected List<T> Jobs { get; }
    protected int Size { get; }

    protected JobManager(int size)
    {
        Jobs = new List<T>(size);
        Size = size;
    }

    protected abstract bool AddJob(T job);

    protected abstract bool RemoveJob(T job);
    
    protected bool RemoveJob(string name)
    {
        var job = Jobs.FirstOrDefault(j => j.Name == name);
        if (job == null)
            return false;
        
        return RemoveJob(job);
    }

    protected bool RemoveAllJobs()
    {
        if (Jobs.Count <= 0)
            return false;

        foreach (var job in Jobs)
            RemoveJob(job);
        
        return true;
    }
    
    protected T GetJob(string name)
    {
        var job = Jobs.FirstOrDefault(j => j.Name == name);
        
        if (job == null)
            throw new Exception($"IJob[{typeof(T)}] with name {name} not found");
        
        return job;
    }
    
    protected abstract void DoAllJobs(ExecutionFlowType flowType);
    
    protected abstract void DoJob(string name);
}