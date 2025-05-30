using System.Text.Json.Serialization;
using CLEA.EasySaveCore.Models;

namespace EasySaveCore.Server.DataStructures
{
    public class ClientBackupJob
    {
        [JsonPropertyName("initialName")] public string InitialName { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("source")] public string Source { get; set; }

        [JsonPropertyName("target")] public string Target { get; set; }

        [JsonPropertyName("strategyType")] public JobExecutionStrategy.StrategyType StrategyType { get; set; }

        [JsonPropertyName("isEncrypted")] public bool IsEncrypted { get; set; }

        public ClientBackupJob(string initialName, string name, string source, string target,
            JobExecutionStrategy.StrategyType strategyType, bool isEncrypted)
        {
            InitialName = initialName;
            Name = name;
            Source = source;
            Target = target;
            StrategyType = strategyType;
            IsEncrypted = isEncrypted;
        }

        public ClientBackupJob(string name, string source, string target,
            JobExecutionStrategy.StrategyType strategyType, bool isEncrypted)
            : this(name, name, source, target, strategyType, isEncrypted)
        {
            InitialName = name;
        }
    }
}