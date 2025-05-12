using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CLEA.EasySaveCore.L10N;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.ViewModel;

public class EasySaveViewModel<TJob> : INotifyPropertyChanged where TJob : IJob
{
    public readonly JobManager<TJob> JobManager;
    
    public IViewModelObjectBuilder<TJob>? JobBuilder;
    public ICommand BuildJobCommand;

    // Languages
    public List<LangIdentifier> AvailableLanguages => Languages.SupportedLangs;
    public LangIdentifier CurrentApplicationLang
    {
        get => L10N<TJob>.Get().GetLanguage();
        set { L10N<TJob>.Get().SetLanguage(value); OnPropertyChanged(); }
    }

    // Daily Logs Formats
    public List<Format> AvailableDailyLogFormats => new List<Format>(Enum.GetValues<Format>());
    public Format CurrentDailyLogFormat
    {
        get => Logger<TJob>.Get().DailyLogFormat;
        set { Logger<TJob>.Get().DailyLogFormat = value; OnPropertyChanged(); }
    }

    public List<TJob> AvailableJobs => JobManager.GetJobs();

    public ICommand SelectedJobCommand;
    private TJob? _selectedJob;
    public TJob? SelectedJob
    {
        get => _selectedJob;
        set { _selectedJob = value; OnPropertyChanged();}
    }

    private string _userInput;
    public string UserInput
    {
        get => _userInput;
        set { _userInput = value; OnPropertyChanged(); }
    }

    private static EasySaveViewModel<TJob> _instance;
    
    private EasySaveViewModel(JobManager<TJob> jobManager)
    {
        JobManager = jobManager;

        BuildJobCommand = new RelayCommand(_ =>
        {
            if (JobBuilder == null)
                throw new NullReferenceException($"Job Builder for <{typeof(TJob)}> is not defined !");
            
            JobManager.AddJob(SelectedJob = JobBuilder.Build());
        }, _ => true);

        SelectedJobCommand = new RelayCommand(jobName =>
        {
            if (jobName is string name)
                SelectedJob = JobManager.GetJob(name);
        }, _ => true);
    }
    
    public void SetJobBuilder(IViewModelObjectBuilder<TJob> jobBuilder)
    {
        JobBuilder = jobBuilder;
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