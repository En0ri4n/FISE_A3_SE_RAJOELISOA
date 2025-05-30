using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.ViewModel;
using EasySaveCore.Models;
using System.Windows.Input;
using System.IO;
using System.Windows.Forms;
using FolderBrowserDialog = FolderBrowserEx.FolderBrowserDialog;
using System.ComponentModel;
using CLEA.EasySaveCore.Translations;
using System.Runtime.CompilerServices;
using Easy_Save_Remote.Models;
using EasySaveCore.Jobs.Backup.Configurations;
using CLEA.EasySaveCore.Utilities;
using System.Text.Json.Nodes;
using System.Xml.Linq;


namespace Easy_Save_Remote
{

    internal class Client
    {
        //TODO classe clients config avec attributs de config.json

        private static Socket socket;
        private static BackupJobConfiguration config = new BackupJobConfiguration();


        public Socket Connect(string url, int port)
        {
            IPAddress address = IPAddress.Parse(url);
            IPEndPoint serverEndPoint = new IPEndPoint(address, port);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Connected to server.");
            return socket = clientSocket;
        }

        public void LoadData()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int received = socket.Receive(buffer);
                    if (received == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    dynamic json = JsonConvert.SerializeObject(message);
                    config.JsonDeserialize(json);

                }
                catch
                {
                    //Console.WriteLine("Disconnected from server.");
                    break;
                }
            }
        }

        public void SendData(JsonObject message)
        {
            socket.Send(Encoding.UTF8.GetBytes(config.JsonSerialize().ToString()));
        }

        public void Disconnect()
        {
            socket.Close();
        }

        public JsonObject ClientJsonSerialize(string name, string source, string target, string action)
        {
            var data = new JsonObject
            {
                { "name", name},
                { "source", source},
                { "target", target },
                { "action", action},
            };

            return data;
        }
    }

    public sealed class ClientBackupJobBuilder : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _name = string.Empty;
        private string _source = string.Empty;
        private string _target = string.Empty;
        private JobExecutionStrategy.StrategyType _strategyType = JobExecutionStrategy.StrategyType.Full;
        private bool _isEncrypted = false;

        private string _initialName = string.Empty;

        public string InitialName
        {
            get => _initialName;
            protected set
            {
                _initialName = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Source
        {
            get => _source;
            set { _source = value; OnPropertyChanged(); }
        }

        public string Target
        {
            get => _target;
            set { _target = value; OnPropertyChanged(); }
        }

        public JobExecutionStrategy.StrategyType StrategyType
        {
            get => _strategyType;
            set { _strategyType = value; OnPropertyChanged(); }
        }

        public bool IsEncrypted
        {
            get => _isEncrypted;
            set { _isEncrypted = value; OnPropertyChanged(); }
        }

        public JobExecutionStrategy.StrategyType[] AvailableStrategies
        {
            get
            {
                return Enum.GetValues(typeof(JobExecutionStrategy.StrategyType)) as JobExecutionStrategy.StrategyType[]
                       ?? Array.Empty<JobExecutionStrategy.StrategyType>();
            }
        }

        public ICommand ShowFolderDialogCommand { get; set; }

        public ClientBackupJobBuilder()
        {
            ShowFolderDialogCommand = new RelayCommand(input =>
            {
                bool isSource = bool.Parse((string)input!);

                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                string title = L10N.Get().GetTranslation("browse_folder.target");

                folderBrowserDialog.Title = title;
                string path = Target;

                if (isSource)
                {
                    folderBrowserDialog.Title = L10N.Get().GetTranslation("browse_folder.source");
                    Source = path;
                }

                string fullPath = Path.IsPathRooted(path) ? path
                : Path.GetFullPath(Path.Combine(".", path));

                folderBrowserDialog.InitialFolder = fullPath;
                folderBrowserDialog.AllowMultiSelect = false;

                if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                    return;

                if (isSource)
                {
                    Source = folderBrowserDialog.SelectedFolder;
                }
                else
                {
                    Target = folderBrowserDialog.SelectedFolder;
                }
            }, _ => true);
        }

        public void Clear()
        {
            Name = string.Empty;
            Source = string.Empty;
            Target = string.Empty;
            StrategyType = JobExecutionStrategy.StrategyType.Full;
            IsEncrypted = false;
        }

        public void GetFrom(BackupJob job)
        {
            InitialName = job.Name;
            Name = job.Name;
            Source = job.Source;
            Target = job.Target;
            StrategyType = job.StrategyType;
            IsEncrypted = job.IsEncrypted;
        }

        public ClientBackupJob Build(bool clear = true)
        {
            ClientBackupJob job = new ClientBackupJob(Name, Source, Target, StrategyType, IsEncrypted);
            if (clear)
                Clear();
            return job;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        
    }
}
