using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup;

public class BackupJobManager : JobManager<BackupJob>
{
    public BackupJobManager() : base(5)
    {
    }

    protected override bool AddJob(BackupJob job)
    {
        if (job == null || Jobs.Count >= Size || Jobs.Any(j => j.Name == job.Name))
            return false;
        
        Jobs.Add(job);
        return true;
    }

    protected override bool RemoveJob(BackupJob job)
    {
        if (job == null || !Jobs.Contains(job))
            return false;
        
        Jobs.Remove(job);
        return true;
    }

    protected override void DoAllJobs(ExecutionFlowType flowType)
    {
        foreach (var job in Jobs)
        {
            if (job.CanRunJob())
            {
                job.RunJob(flowType == ExecutionFlowType.Parallel);
            }
        }
    }

    protected override void DoJob(string name)
    {
        BackupJob job = GetJob(name);
        
        if (job.CanRunJob())
            job.RunJob(false);
        else
            throw new Exception($"Job {name} cannot be run");
    }
}