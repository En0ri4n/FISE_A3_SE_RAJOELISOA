using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models;

public interface IJob : IJsonSerializable, IXmlSerializable
{
    string Name { get; }
    
    JobExecutionStrategy.ExecutionStatus Status { get; set; }
    
    public delegate void TaskCompletedDelegate(dynamic task);
    public event TaskCompletedDelegate TaskCompletedHandler;
    public void ClearTaskCompletedHandler();
    
    List<Property<dynamic>> Properties { get; }
    
    bool IsRunning { get; set; }
    
    bool CanRunJob();
    void RunJob(JobExecutionStrategy.StrategyType strategyType);
}