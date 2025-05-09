using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models;

public interface IJob : IJsonSerializable
{
    List<Property<dynamic>> Properties { get; }
    
    bool IsRunning { get; }
    
    bool CanRunJob();
    bool RunJob(bool async);
}