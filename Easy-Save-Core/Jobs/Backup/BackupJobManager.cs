using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.Jobs.Backup;

public class BackupJobManager : JobManager<BackupJob>
{
    public BackupJobManager() : base(5)
    {
    }

    public override bool AddJob(BackupJob job, bool save)
    {
        if (job == null || (Size != -1 && Jobs.Count >= Size) || Jobs.Any(j => j.Name == job.Name))
            return false;
        
        Jobs.Add(job);
        if (save)
            EasySaveConfiguration<BackupJob>.SaveConfiguration();
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
        EasySaveConfiguration<BackupJob>.SaveConfiguration();
        return true;
    }
    public void UpdateJob(string name, JsonObject? jobJson)
    {
        var job = Jobs.FirstOrDefault(j => j.Name == name);
        if (job == null)
            throw new Exception($"BackupJob with name {name} not found");

        if (jobJson != null)
            job.JsonDeserialize(jobJson);

        EasySaveConfiguration<BackupJob>.SaveConfiguration();
    }

    public void UpdateJob(string name, BackupJob? job)
    {
        var existingJob = Jobs.FirstOrDefault(j => j.Name == name);
        if (existingJob == null)
            throw new Exception($"BackupJob with name {name} not found");

        if (job != null)
            existingJob.JsonDeserialize(job.JsonSerialize());

        EasySaveConfiguration<BackupJob>.SaveConfiguration();
    }


    public override void DoJob(BackupJob job)
    {
        if (!job.CanRunJob())
            throw new Exception($"Job {job.Name} cannot be run");

        job.Status = JobExecutionStrategy.ExecutionStatus.InProgress;
        job.RunJob(Strategy);
        job.Status = job.BackupJobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed) ? JobExecutionStrategy.ExecutionStatus.Completed : JobExecutionStrategy.ExecutionStatus.Failed;
        
        Logger<BackupJob>.Get().SaveDailyLog(new List<JobTask>(job.BackupJobTasks));
    }

    public override void DoMultipleJob(List<BackupJob> jobs)
    {
        foreach (var job in jobs)
        {
            if (job.CanRunJob())
            {
                job.Status = JobExecutionStrategy.ExecutionStatus.InProgress;
                job.RunJob(Strategy);
                job.Status = job.BackupJobTasks.All(x => x.Status != JobExecutionStrategy.ExecutionStatus.Failed) ? JobExecutionStrategy.ExecutionStatus.Completed : JobExecutionStrategy.ExecutionStatus.Failed;
            }
        }

        Logger<BackupJob>.Get().SaveDailyLog([.. jobs.SelectMany(job => job.BackupJobTasks)]);
    }

    public override void DoAllJobs()
    {
        DoMultipleJob(Jobs);
    }
}