using System;
using System.Collections.Generic;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models
{
    public interface IJob : IJsonSerializable, IXmlSerializable
    {
        public delegate void JobCompletedDelegate(IJob job);
        public event JobCompletedDelegate JobCompletedHandler;
        public delegate void TaskCompletedDelegate(dynamic task);
        public event TaskCompletedDelegate TaskCompletedHandler;
        public delegate void JobPausedDelegate(IJob job);
        public event JobPausedDelegate JobPausedHandler;

        string Name { get; }
        DateTime Timestamp { get; set; }
        string Source { get; set; }
        string Target { get; set; }
        JobExecutionStrategy.StrategyType StrategyType { get; set; }
        bool IsEncrypted { get; set; }
        long Size { get; set; }
        long TransferTime { get; set; }
        long EncryptionTime { get; set; }
        double Progress { get; }
        
        bool IsPaused { get; set; }
        bool WasPaused { get; set; }
        
        List<JobTask> JobTasks { get; set; }

        JobExecutionStrategy.ExecutionStatus Status { get; set; }

        bool IsRunning { get; set; }
        public void ClearTaskCompletedHandler();
        public void ClearJobCompletedHandler();
        
        void PauseJob();
        Action ResumeJob();

        bool CanRunJob();
        void RunJob();
        void ClearAndSetupJob();
        void CompleteJob(JobExecutionStrategy.ExecutionStatus notEnoughDiskSpace);
    }
}