namespace CLEA.EasySaveCore.Models
{
    public static class JobExecutionStrategy
    {
        public enum ExecutionStatus
        {
            NotStarted,
            InQueue,
            CanNotStart,
            InProgress,
            Completed,
            Skipped,
            JobAlreadyRunning,
            InterruptedByProcess,
            Failed,
            SourceNotFound,
            DirectoriesNotSpecified,
            SameSourceAndTarget,
            NotEnoughDiskSpace,
            Paused,
            Stopped
        }

        public enum StrategyType
        {
            Full,
            Differential
        }
    }
}