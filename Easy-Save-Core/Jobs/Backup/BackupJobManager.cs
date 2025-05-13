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
        if (job == null || Jobs.Count >= Size || Jobs.Any(j => j.Name == job.Name))
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


    public override void DoJob(BackupJob job)
    {
        if (!job.CanRunJob())
            throw new Exception($"Job {job.Name} cannot be run");
        
        job.RunJob(Strategy);
        Logger<BackupJob>.Get().SaveDailyLog(new List<JobTask>(job.BackupJobTasks));
    }

    public override void DoMultipleJob(List<BackupJob> jobs)
    {
        foreach (var job in jobs)
        {
            if (job.CanRunJob())
            {
                job.RunJob(Strategy);

            }
        }

        Logger<BackupJob>.Get().SaveDailyLog([.. jobs.SelectMany(job => job.BackupJobTasks)]);
    }

    public override void DoAllJobs()
    {
        DoMultipleJob(Jobs);
    }
}