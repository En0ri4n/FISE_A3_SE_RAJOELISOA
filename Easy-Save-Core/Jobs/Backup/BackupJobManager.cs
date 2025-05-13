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

    protected override void DoAllJobs()
    {
        foreach (var job in Jobs)
        {
            if (job.CanRunJob())
            {
                job.RunJob(Strategy);
            }
        }
    }

    public override void DoJob(BackupJob job)
    {
        if (!job.CanRunJob())
            throw new Exception($"Job {job.Name} cannot be run");
        
        job.RunJob(Strategy);
        Logger<BackupJob>.Get().SaveDailyLog(job, new List<JobTask>(job.BackupJobTasks));
    }
}