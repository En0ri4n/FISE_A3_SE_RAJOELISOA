using System;
using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models
{
    public abstract class JobTask : IJsonSerializable, IXmlSerializable
    {
        public string Name { get; set; }

        public DateTime Timestamp { get; set; }

        public string Source { get; set; }

        public string Target { get; set; }

        public long Size { get; set; }

        public long TransferTime { get; set; }

        public long EncryptionTime { get; set; }
        
        protected JobTask(string name)
        {
            Name = name;
        }

        public JobExecutionStrategy.ExecutionStatus Status { get; protected set; } =
            JobExecutionStrategy.ExecutionStatus.NotStarted;


        public abstract JsonObject JsonSerialize();

        public abstract void JsonDeserialize(JsonObject data);

        public abstract XmlElement XmlSerialize(XmlDocument parent);
        public abstract void XmlDeserialize(XmlElement data);

        public abstract void ExecuteTask(JobExecutionStrategy.StrategyType strategyType);
    }
}