using System;
using System.Collections.Generic;
using System.Threading;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models
{
    public interface IJob : IJsonSerializable, IXmlSerializable
    {
        public delegate void JobCompletedDelegate(IJob job, JobExecutionStrategy.ExecutionStatus status);
        public event JobCompletedDelegate JobFinishedHandler;
        public delegate void TaskCompletedDelegate(dynamic task);
        public event TaskCompletedDelegate TaskCompletedHandler;
        public delegate void JobPausedDelegate(IJob job);
        public event JobPausedDelegate JobPausedHandler;
        public delegate void JobStoppedDelegate(IJob job);
        public event JobStoppedDelegate JobStoppedHandler;
        public delegate void JobStartedDelegate(IJob job);
        public event JobStartedDelegate JobStartedHandler;

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

        bool IsStopped { get; set; }


        List<JobTask> JobTasks { get; set; }

        JobExecutionStrategy.ExecutionStatus Status { get; set; }

        bool IsRunning { get; set; }
        public void ClearTaskCompletedHandler();
        public void ClearJobCompletedHandler();
        
        void PauseJob();
        void ResumeJob();
        void StopJob();

        bool CanRunJob();
        void RunJob(bool runPriority, CountdownEvent countdown);
        void ClearAndSetupJob();
        void CompleteJob(JobExecutionStrategy.ExecutionStatus notEnoughDiskSpace);
    }
}