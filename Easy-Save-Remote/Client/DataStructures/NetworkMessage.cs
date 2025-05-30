using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EasySaveRemote.Client.DataStructures
{
    /// <summary>
    /// Represents a message sent over the network.<br/>
    /// This class encapsulates the type of message and the associated data.<br/>
    /// Example:
    /// <code>
    ///   NetworkMessage message = NetworkMessage.Create(MessageType.BackupJobAdd, new ClientBackupJob("job1", "sourcePath", "targetPath", ClientJobExecutionStrategyType.Full, false));
    /// </code>
    /// </summary>
    public class NetworkMessage
    {
        [JsonPropertyName("type")]
        public MessageType Type { get; set; }
        
        [JsonPropertyName("data")]
        public JsonNode Data { get; set; }
        
        private NetworkMessage(MessageType type, JsonNode data)
        {
            Type = type;
            Data = data;
        }

        public NetworkMessage(MessageType type) : this(type, new JsonObject()) { }
        
        public static NetworkMessage Create(MessageType type, JsonNode data)
        {
            return new NetworkMessage(type, data);
        }
    }
    
    /// <summary>
    /// Represents the type of message being sent over the network.
    /// </summary>
    public enum MessageType
    {
        BackupJobList,
        BackupJobUpdate,
        BackupJobAdd,
        BackupJobRemove,
    }
}