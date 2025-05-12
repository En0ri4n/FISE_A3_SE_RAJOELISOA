using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel;

public sealed class ViewModelJobBuilder : IViewModelObjectBuilder<BackupJob>
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _name = string.Empty;
    private string _source = string.Empty;
    private string _target = string.Empty;
    
    public string Name 
    {
        get => _name;
        set => SetField(ref _name, value);
    }
    
    public string Source 
    {
        get => _source;
        set => SetField(ref _source, value);
    }
    
    public string Target 
    {
        get => _target;
        set => SetField(ref _target, value);
    }

    public ViewModelJobBuilder()
    {
    }

    public void Clear()
    {
        Name = string.Empty;
        Source = string.Empty;
        Target = string.Empty;
    }

    public void GetFrom(BackupJob job)
    {
        Name = job.Name;
        Source = job.Source.Value;
        Target = job.Target.Value;
    }

    public BackupJob Build()
    {
        BackupJob job = new BackupJob(Name, Source, Target);
        Clear();
        return job;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}