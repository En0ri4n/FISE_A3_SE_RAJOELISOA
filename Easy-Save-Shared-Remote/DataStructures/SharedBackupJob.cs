using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace EasySaveShared.DataStructures
{
    /// <summary>
    /// Represents a backup job for the shared remote service.<br/>
    /// </summary>
    public class SharedBackupJob : INotifyPropertyChanged
    {
        [JsonProperty("initialName")] public string InitialName { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("source")] public string Source { get; set; }

        [JsonProperty("target")] public string Target { get; set; }

        [JsonProperty("strategyType")] public SharedExecutionStrategyType StrategyType { get; set; }

        [JsonProperty("isEncrypted")] public bool IsEncrypted { get; set; }
        
        [JsonProperty("progress")] public double Progress { get; set; } = 0.0;
        
        [JsonProperty("status")] public SharedExecutionStatus Status { get; set; } = SharedExecutionStatus.NotStarted;

        [Newtonsoft.Json.JsonConstructor]
        public SharedBackupJob(string initialName, string name, string source, string target,
            SharedExecutionStrategyType strategyType, bool isEncrypted, double progress,
            SharedExecutionStatus status)
        {
            InitialName = initialName;
            Name = name;
            Source = source;
            Target = target;
            StrategyType = strategyType;
            IsEncrypted = isEncrypted;
            Progress = progress;
            Status = status;
        }

        public SharedBackupJob(string name, string source, string target,
            SharedExecutionStrategyType strategyType, bool isEncrypted)
            : this(name, name, source, target, strategyType, isEncrypted, 0.0D, SharedExecutionStatus.NotStarted)
        {
        }
        
        public void UpdateData(SharedBackupJob job)
        {
            if (job == null) return;
            
            Progress = job.Progress;
            Status = job.Status;
            
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Status));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}