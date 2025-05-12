using System.Text.Json.Nodes;
using CLEA.EasySaveCore.utilities;

namespace CLEA.EasySaveCore.Models;

public interface IJob : IJsonSerializable
{
    List<Property<dynamic>> Properties { get; }
    
    bool IsRunning { get; set; }
    
    bool CanRunJob();
    bool RunJob(bool async);
}