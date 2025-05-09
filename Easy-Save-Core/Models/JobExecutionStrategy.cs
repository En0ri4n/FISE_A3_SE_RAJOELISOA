namespace CLEA.EasySaveCore.Models;

public abstract class JobExecutionStrategy
{
    private IJob _job;

    private StrategyType _type;

    private JobStatus _status;
    
    public IJob Job
    {
        get => _job;
        set => _job = value;
    }
    
    public StrategyType Type
    {
        get => _type;
        set => _type = value;
    }
    
    public JobStatus Status
    {
        get => _status;
        set => _status = value;
    }
    
    protected JobExecutionStrategy(IJob job, StrategyType strategyType)
    {
        _job = job;
        _type = strategyType;
        _status = JobStatus.NotStarted;
    }

    public abstract void ExecuteJob();

    public enum StrategyType
    {
        Full,
        Differential
    }
    
    public enum JobStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
}