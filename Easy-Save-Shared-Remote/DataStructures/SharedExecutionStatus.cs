namespace EasySaveShared.DataStructures
{
    public enum SharedExecutionStatus
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
}