namespace CLEA.EasySaveCore.Utilities
{
    public class StatusLogEntry
    {
        public DateTime Timestamp { get; set; }

        public string Message { get; set; }

        public StatusLogEntry(string message)
        {
            Timestamp = DateTime.Now;
            Message = message;
        }
    }
}
