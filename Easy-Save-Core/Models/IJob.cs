using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models;

public interface IJob : IJsonSerializable, IXmlSerializable
{
    string Name { get; }
    
    List<Property<dynamic>> Properties { get; }
    
    bool IsRunning { get; set; }
    
    bool CanRunJob();
    JobExecutionStrategy.ExecutionStatus RunJob(JobExecutionStrategy.StrategyType strategyType);
}