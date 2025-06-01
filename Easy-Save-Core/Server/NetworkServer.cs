using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CLEA.EasySaveCore.Jobs.Backup;
using CLEA.EasySaveCore.Models;
using CLEA.EasySaveCore.Utilities;
using EasySaveShared.DataStructures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EasySaveCore.Server
{
    /// <summary>
    /// Represents a network server that listens for incoming client connections and handles communication with them.
    /// This server can accept multiple clients and broadcast messages to all connected clients.
    /// </summary>
    public class NetworkServer
    {
        private readonly List<Socket> _clients = new List<Socket>();
        private readonly ServerNetworkHandler _serverNetworkHandler;
        private readonly BackupJobManager _backupJobManager;
        private readonly object _lockObj = new object();
        private Socket _serverSocket;
        private Task _serverThread = null!;
        
        public bool IsRunning { get; private set; }

        public NetworkServer(JobManager backupJobManager)
        {
            _backupJobManager = (BackupJobManager) backupJobManager;
            _serverNetworkHandler = new ServerNetworkHandler(this, _backupJobManager);
        }
        
        // private static void Main()
        // {
        //     Start();
        // }

        /// <summary>
        /// Starts the network server and begins listening for incoming client connections.<br/>
        /// This method will block the current thread and run indefinitely until the server is stopped.<br/>
        /// </summary>
        public void Start()
        {
            Logger.Log(LogLevel.Information, "Network server starting...");
            
            _serverSocket = Connect();
            IsRunning = true;

            _serverThread = Task.Run(() =>
            {
                while (true)
                {
                    Socket clientSocket = _serverSocket.Accept();
                    lock (_lockObj)
                    {
                        _clients.Add(clientSocket);
                    }

                    // Create a new thread to handle the client
                    Thread clientThread = new Thread(() => ListenToClient(clientSocket));
                    clientThread.Start();
                }
            });
        }

        private Socket Connect()
        {
            if (IsRunning)
                throw new InvalidOperationException("Server is already running.");
            
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);
            Logger.Log(LogLevel.Information, "Server started. Waiting for clients...");
            return serverSocket;
        }

        /// <summary>
        /// Listens for incoming messages from a connected client.<br/>
        /// This method runs in a separate thread for each client and processes messages until the client disconnects.<br/>
        /// </summary>
        /// <param name="client"></param>
        private void ListenToClient(Socket client)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (true)
                {
                    int received = client.Receive(buffer);
                    if (received == 0) continue;

                    // We assume the message is a JSON string
                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    // We receive the message from client and deserialize it
                    NetworkMessage? deserializedMessage = JsonConvert.DeserializeObject<NetworkMessage>(message);
                    if (deserializedMessage == null)
                    {
                        Logger.Log(LogLevel.Information, "Received invalid message from client.");
                        continue;
                    }
                    
                    // We handle the message using the server network handler
                    _serverNetworkHandler.HandleNetworkMessage(client, deserializedMessage);
                }
            }
            catch (SocketException)
            {
                Logger.Log(LogLevel.Information, $"Client {client.RemoteEndPoint} disconnected.");
            }
            finally
            {
                lock (_lockObj)
                {
                    _clients.Remove(client);
                }

                client.Close();
                // Console.WriteLine($"Client {client.RemoteEndPoint} disconnected.");
            }
        }
        
        /// <summary>
        /// Sends a message to a specific client.<br/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void SendMessage(Socket client, NetworkMessage message)
        {
            lock (_lockObj)
            {
                try
                {
                    // Serialize the message to JSON
                    string serializedMessage = JToken.FromObject(message).ToString(Formatting.None);
                    byte[] data = Encoding.UTF8.GetBytes(serializedMessage);
                    client.Send(data);
                }
                catch (SocketException)
                {
                    Console.WriteLine($"Failed to send message to {client.RemoteEndPoint}. Removing client.");
                    _clients.Remove(client);
                    client.Close();
                }
            }
        }

        /// <summary>
        /// Sends a message to all connected clients except the sender.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void BroadcastMessage(Socket sender, NetworkMessage message)
        {
            lock (_lockObj)
            {
                foreach (Socket client in _clients.Where(client => client != sender).ToList())
                {
                    try
                    {
                        // Serialize the message to JSON
                        byte[] data = Encoding.UTF8.GetBytes(JToken.FromObject(message).ToString(Formatting.None));
                        client.Send(data);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to send message to {client.RemoteEndPoint}. Removing client.");
                        lock (_lockObj)
                        {
                            _clients.Remove(client);
                        }
                        client.Close();
                    }
                }
            }
        }
        
        public void BroadcastMessage(NetworkMessage message)
        {
            lock (_lockObj)
            {
                foreach (Socket client in _clients.ToList())
                {
                    try
                    {
                        // Serialize the message to JSON
                        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                        client.Send(data);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to send message to {client.RemoteEndPoint}. Removing client.");
                        _clients.Remove(client);
                        client.Close();
                    }
                }
            }
        }
    }
}
