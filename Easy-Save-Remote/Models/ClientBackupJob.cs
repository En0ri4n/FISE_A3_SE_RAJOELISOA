using System;
using System.Collections.Generic;
using System.Text;
using CLEA.EasySaveCore.Models;

namespace Easy_Save_Remote.Models
{
    public class ClientBackupJob
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public JobExecutionStrategy.StrategyType StrategyType { get; set; }
        public bool IsEncrypted { get; set; }

        public ClientBackupJob(string name, string source, string target, JobExecutionStrategy.StrategyType strategyType, bool isEncrypted)
        {
            Name = name;
            Source = source;
            Target = target;
            StrategyType = strategyType;
            IsEncrypted = isEncrypted;
        }
    }
}
