using System.Text.Json.Nodes;
using System.Xml;
using CLEA.EasySaveCore.Utilities;

namespace CLEA.EasySaveCore.Models
{
    public abstract class JobTask : IJsonSerializable, IXmlSerializable
    {
        public JobExecutionStrategy.ExecutionStatus Status { get; protected set; } = JobExecutionStrategy.ExecutionStatus.NotStarted;

        private string _name;
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        protected JobTask(string name)
        {
            _name = name;
        }
    
        public abstract void ExecuteTask(JobExecutionStrategy.StrategyType strategyType);

        public abstract JsonObject JsonSerialize();

        public abstract void JsonDeserialize(JsonObject data);

        public abstract XmlElement XmlSerialize(XmlDocument parent);
        public abstract void XmlDeserialize(XmlElement data);
    }
}