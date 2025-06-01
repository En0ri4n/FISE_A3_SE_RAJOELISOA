using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using EasySaveShared.Client.Commands;
using EasySaveShared.DataStructures;
using Newtonsoft.Json.Linq;

namespace EasySaveShared.Client.ViewModel
{
    public class ClientViewModel
    {
        public ClientBackupJobBuilder BackupJobBuilder { get; } = new ClientBackupJobBuilder();
        public ObservableCollection<SharedBackupJob> AvailableBackupJobs => RemoteClient.Get().BackupJobManager.BackupJobs;
        public ICommand BuildJobCommand { get; set; }
        public ICommand LoadJobInBuilderCommand { get; set; }
        
        public ICommand PauseMultipleJobsCommand { get; set; }
        public ICommand ResumeMultipleJobsCommand { get; set; }
        public ICommand DeleteMultipleJobsCommand { get; set; }
        public ICommand RunMultipleJobsCommand { get; set; }
        public ICommand StopMultipleJobsCommand { get; set; }
        
        public Action CloseAction { get; set; } = () => { };

        public void InitializeCommands()
        {
            BuildJobCommand = new RelayCommand(isCreation =>
            {
                if(!bool.TryParse(isCreation?.ToString(), out bool isJobCreation))
                    return;
                
                SharedBackupJob clientBackupJob = RemoteClient.Get().ViewModel.BackupJobBuilder.Build();
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(isJobCreation ? MessageType.AddJob2Server : MessageType.UpdateJob2Server, 
                    CreateJsonObject("backupJob", JToken.FromObject(clientBackupJob))));
                CloseAction();
            }, _ => true);
            
            LoadJobInBuilderCommand = new RelayCommand(jobName =>
            {
                SharedBackupJob? sharedJob = RemoteClient.Get().BackupJobManager.GetJob((string) jobName!);
                if (sharedJob != null)
                    RemoteClient.Get().ViewModel.BackupJobBuilder.GetFrom(sharedJob);
            }, _ => true);
            
            PauseMultipleJobsCommand = new RelayCommand(jobNames =>
            {
                if (!(jobNames is List<string> jobNamesList))
                    return;
                
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(MessageType.PauseMultipleJobs2Server, 
                    CreateJsonObject("jobNames", jobNamesList)));
            }, _ => true);
            
            ResumeMultipleJobsCommand = new RelayCommand(jobNames =>
            {
                if (!(jobNames is List<string> jobNamesList))
                    return;
                
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(MessageType.ResumeMultipleJobs2Server, 
                    CreateJsonObject("jobNames", jobNamesList)));
            }, _ => true);
            
            DeleteMultipleJobsCommand = new RelayCommand(jobNames =>
            {
                if (!(jobNames is List<string> jobNamesList))
                    return;
                        
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(MessageType.DeleteMultipleJobs2Server, 
                    CreateJsonObject("jobNames", jobNamesList)));
            }, _ => true);
            
            RunMultipleJobsCommand = new RelayCommand(jobNames =>
            {
                if (!(jobNames is List<string> jobNamesList))
                    return;
                
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(MessageType.StartMultipleJobs2Server, 
                    CreateJsonObject("jobNames", jobNamesList)));
            }, _ => true);
            
            StopMultipleJobsCommand = new RelayCommand(jobNames =>
            {
                if (jobNames == null)
                    return;
                
                if (!(jobNames is List<string> jobNamesList))
                    return;
                
                RemoteClient.Get().NetworkClient.SendMessage(NetworkMessage.Create(MessageType.StopMultipleJobs2Server, 
                    CreateJsonObject("jobNames", jobNamesList)));
            }, _ => true);
        }

        private JObject CreateJsonObject(string name, object value)
        {
            return new JObject { [name] = JToken.FromObject(value) };
        }
    }
}