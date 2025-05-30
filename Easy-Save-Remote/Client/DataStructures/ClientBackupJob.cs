
using System.Text.Json.Serialization;

namespace EasySaveRemote.Client.DataStructures
{
    /// <summary>
    /// Represents a backup job for the client.
    /// </summary>
    public class ClientBackupJob
    {
        [JsonPropertyName("initialName")] public string InitialName { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("source")] public string Source { get; set; }

        [JsonPropertyName("target")] public string Target { get; set; }

        [JsonPropertyName("strategyType")] public ClientJobExecutionStrategyType StrategyType { get; set; }

        [JsonPropertyName("isEncrypted")] public bool IsEncrypted { get; set; }

        public ClientBackupJob(string initialName, string name, string source, string target,
            ClientJobExecutionStrategyType strategyType, bool isEncrypted)
        {
            InitialName = initialName;
            Name = name;
            Source = source;
            Target = target;
            StrategyType = strategyType;
            IsEncrypted = isEncrypted;
        }

        public ClientBackupJob(string name, string source, string target,
            ClientJobExecutionStrategyType strategyType, bool isEncrypted)
            : this(name, name, source, target, strategyType, isEncrypted)
        {
            InitialName = name;
        }
    }
}