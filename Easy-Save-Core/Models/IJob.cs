using System.Collections.Generic;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models
{
    public interface IJob : IJsonSerializable, IXmlSerializable
    {
        string Name { get; }
    
        JobExecutionStrategy.ExecutionStatus Status { get; set; }
    
        public delegate void TaskCompletedDelegate(dynamic task);
        public event TaskCompletedDelegate TaskCompletedHandler;
        public void ClearTaskCompletedHandler();
        
        public delegate void JobCompletedDelegate(IJob job);
        public event JobCompletedDelegate JobCompletedHandler;
        public void ClearJobCompletedHandler();
    
        bool IsRunning { get; set; }
    
        bool CanRunJob();
        void RunJob();
    }
}