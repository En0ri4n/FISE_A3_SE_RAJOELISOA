using System.ComponentModel;
using System.Runtime.CompilerServices;
using CLEA.EasySaveCore.Models;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel;

public sealed class ViewModelBackupJobBuilder : ViewModelJobBuilder<BackupJob>
{
    private string _name = string.Empty;
    private string _source = string.Empty;
    private string _target = string.Empty;
    
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }
    
    public string Source
    {
        get => _source;
        set { _source = value; OnPropertyChanged(); }
    }
    
    public string Target
    {
        get => _target;
        set { _target = value; OnPropertyChanged(); }
    }

    public override void Clear()
    {
        Name = string.Empty;
        Source = string.Empty;
        Target = string.Empty;
    }

    public override void GetFrom(BackupJob job)
    {
        Name = job.Name;
        Source = job.Source.Value;
        Target = job.Target.Value;
    }

    public override BackupJob Build()
    {
        BackupJob job = new BackupJob(Name, Source, Target);
        Clear();
        return job;
    }
}