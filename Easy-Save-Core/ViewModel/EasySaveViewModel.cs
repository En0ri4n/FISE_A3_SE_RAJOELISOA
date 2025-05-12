using System.ComponentModel;
using System.Runtime.CompilerServices;
using CLEA.EasySaveCore.Models;

namespace CLEA.EasySaveCore.ViewModel;

public class EasySaveViewModel<T> : INotifyPropertyChanged where T : IJob
{
    public readonly JobManager<T> JobManager;
    
    private EasySaveViewModel(JobManager<T> jobManager)
    {
        JobManager = jobManager;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private static EasySaveViewModel<T> _instance;
    
    public static EasySaveViewModel<T> Get()
    {
        if (_instance == null)
            throw new Exception("EasySaveViewModel not initialized");
        
        return _instance;
    }
    
    public static void Init(JobManager<T> jobManager)
    {
        if (_instance != null)
            throw new Exception("EasySaveViewModel already initialized");
        
        _instance = new EasySaveViewModel<T>(jobManager);
    }
}