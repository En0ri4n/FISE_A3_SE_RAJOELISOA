using System.ComponentModel;
using System.Runtime.CompilerServices;
using CLEA.EasySaveCore.Models;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel;

public sealed class ViewModelJobBuilder : ViewModelObjectBuilder<BackupJob>
{
    public ViewModelJobBuilder()
    {
        SetProperty("Name", string.Empty);
        SetProperty("Source", string.Empty);
        SetProperty("Target", string.Empty);
    }

    public override void Clear()
    {
        SetProperty("Name", string.Empty);
        SetProperty("Source", string.Empty);
        SetProperty("Target", string.Empty);
    }

    public override void GetFrom(BackupJob job)
    {
        SetProperty("Name", job.Name);
        SetProperty("Source", job.Source.Value);
        SetProperty("Target", job.Target.Value);
    }

    public override BackupJob Build()
    {
        BackupJob job = new BackupJob(GetProperty("Name"), GetProperty("Source"), GetProperty("Target"));
        Clear();
        return job;
    }
}