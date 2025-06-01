using System;
using System.Text.Json;
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
        public JsonObject Data { get; set; }
        
        private NetworkMessage(MessageType type, JsonObject data)
        {
            Type = type;
            Data = data;
        }

        public NetworkMessage(MessageType type) : this(type, new JsonObject()) { }
        
        public static NetworkMessage Create(MessageType type, JsonObject data)
        {
            return new NetworkMessage(type, data);
        }
        
        public string Serialize()
        {
            JsonObject jsonObject = new JsonObject
            {
                ["type"] = Type.ToString(),
                ["data"] = Data
            };

            return jsonObject.ToJsonString();
        }

        public static NetworkMessage? Deserialize(string message)
        {
            // Deserialize the JSON string into a NetworkMessage object manually
            try
            {
                JsonObject? jsonNode = JsonNode.Parse(message)?.AsObject();
                if (jsonNode == null || !jsonNode.ContainsKey("type") || !jsonNode.ContainsKey("data"))
                {
                    return null;
                }
                
                MessageType type = Enum.Parse<MessageType>(jsonNode["type"]?.ToString()!);
                JsonObject data = jsonNode["data"]?.AsObject() ?? new JsonObject();
                return new NetworkMessage(type, data);
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
    
    public enum MessageType
    {
        FetchBackupJobList,
        BackupJobUpdate,
        BackupJobAdd,
        BackupJobRemove,
    }
}