using System.Text.Json.Nodes;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models;

public interface IJob : IJsonSerializable
{
    string Name { get; }
    
    List<Property<dynamic>> Properties { get; }
    
    bool IsRunning { get; }
    
    bool CanRunJob();
    bool RunJob(bool async);
}