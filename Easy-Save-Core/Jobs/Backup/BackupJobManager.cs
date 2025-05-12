using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Models;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup;

public class BackupJobManager : JobManager<BackupJob>
{
    public BackupJobManager() : base(5)
    {
    }

    public override bool AddJob(BackupJob? job)
    {
        if (job == null || Jobs.Count >= Size || Jobs.Any(j => j.Name == job.Name))
            return false;
        
        Jobs.Add(job);
        return true;
    }

    public override bool AddJob(JsonObject? jobJson)
    {
        if (jobJson == null)
            return false;

        var job = new BackupJob();
        
        if (AddJob(job))
        {
            job.JsonDeserialize(jobJson);
            return true;
        }

        return false;
    }

    public override bool RemoveJob(BackupJob? job)
    {
        if (job == null || !Jobs.Contains(job))
            return false;
        
        Jobs.Remove(job);
        return true;
    }

    protected override void DoAllJobs(ExecutionFlowType flowType, JobExecutionStrategy.StrategyType strategy)
    {
        foreach (var job in Jobs)
        {
            if (job.CanRunJob())
            {
                job.RunJob(strategy);
            }
        }
    }

    protected override void DoJob(string name, JobExecutionStrategy.StrategyType strategy)
    {
        BackupJob job = GetJob(name);
        
        if (job.CanRunJob())
            job.RunJob(strategy);
        else
            throw new Exception($"Job {name} cannot be run");
    }
}