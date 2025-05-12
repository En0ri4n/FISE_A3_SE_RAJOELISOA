using System.ComponentModel;
using System.Runtime.CompilerServices;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel;

public class EasySaveViewModel<TJob> : INotifyPropertyChanged where TJob : IJob
{
    public readonly JobManager<TJob> JobManager;
    public IViewModelObjectBuilder<TJob>? JobBuilder;
    
    private static EasySaveViewModel<TJob> _instance;
    
    private EasySaveViewModel(JobManager<TJob> jobManager)
    {
        JobManager = jobManager;
    }
    
    public void SetJobBuilder(IViewModelObjectBuilder<TJob> jobBuilder)
    {
        JobBuilder = jobBuilder;
    }
    
    public bool AddJob(TJob job)
    {
        return JobManager.AddJob(job);
    }
    
    public bool RemoveJob(TJob job)
    {
        return JobManager.RemoveJob(job);
    }
    
    public void UpdateJob(string name, TJob? job)
    {
        JobManager.UpdateJob(name, job);
    }
    
    public List<TJob> GetJobs()
    {
        return JobManager.GetJobs();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public static EasySaveViewModel<TJob> Get()
    {
        if (_instance == null)
            throw new Exception("EasySaveViewModel not initialized");
        
        return _instance;
    }
    
    public static void Init(JobManager<TJob> jobManager)
    {
        if (_instance != null)
            throw new Exception("EasySaveViewModel already initialized");
        
        _instance = new EasySaveViewModel<TJob>(jobManager);
    }
}