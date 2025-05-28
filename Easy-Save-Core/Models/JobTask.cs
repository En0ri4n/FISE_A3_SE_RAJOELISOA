using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models
{
    public abstract class JobTask : IJsonSerializable, IXmlSerializable
    {
        protected JobTask(string name)
        {
            Name = name;
        }

        public JobExecutionStrategy.ExecutionStatus Status { get; protected set; } =
            JobExecutionStrategy.ExecutionStatus.NotStarted;

        public string Name { get; set; }

        public abstract JsonObject JsonSerialize();

        public abstract void JsonDeserialize(JsonObject data);

        public abstract XmlElement XmlSerialize(XmlDocument parent);
        public abstract void XmlDeserialize(XmlElement data);

        public abstract void ExecuteTask(JobExecutionStrategy.StrategyType strategyType);
    }
}