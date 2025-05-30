using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace EasySaveCore.Server.DataStructures
{
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
    
    public enum MessageType
    {
        BackupJobList,
        BackupJobUpdate,
        BackupJobAdd,
        BackupJobRemove,
    }
}