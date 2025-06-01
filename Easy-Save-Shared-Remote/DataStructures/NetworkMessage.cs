using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasySaveShared.DataStructures
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
        [JsonProperty("type")]
        public MessageType Type { get; set; }
        
        [JsonProperty("data")]
        public JObject Data { get; set; }
        
        [Newtonsoft.Json.JsonConstructor]
        public NetworkMessage(MessageType type, JObject data)
        {
            Type = type;
            Data = data;
        }

        public NetworkMessage(MessageType type) : this(type, new JObject()) { }
        
        public static NetworkMessage Create(MessageType type, JObject data)
        {
            return new NetworkMessage(type, data);
        }
        
        public static NetworkMessage Create(MessageType type)
        {
            return new NetworkMessage(type);
        } }
    
    public enum MessageType
    {
        FetchJobs,
        UpdateJob2Server,
        AddJob2Server,
        RemoveJob2Server,
        JobDataUpdate2Client,
        PauseMultipleJobs2Server,
        ResumeMultipleJobs2Server,
        DeleteMultipleJobs2Server,
        StartMultipleJobs2Server,
        StopMultipleJobs2Server
    }
}