using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using EasySaveCore.Jobs.Backup.ViewModels;
using CLEA.EasySaveCore.Models;
using EasySaveCore.Jobs.Backup.Configurations;
using CLEA.EasySaveCore.Translations;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Nodes;
using EasySaveCore.Server.DataStructures;

namespace EasySaveCore.Server
{
    public class NetworkServer
    {
        private readonly List<Socket> _clients = new List<Socket>();
        private readonly ServerNetworkHandler _serverNetworkHandler;
        private readonly object _lockObj = new object();
        private Socket _serverSocket;
        
        public bool IsRunning { get; private set; }

        public NetworkServer()
        {
            _serverNetworkHandler = new ServerNetworkHandler(this);
        }
        
        // private static void Main()
        // {
        //     Start();
        // }

        public void Start()
        {
            _serverSocket = Connect();
            IsRunning = true;
            
            while (true)
            {
                Socket clientSocket = _serverSocket.Accept();
                lock (_lockObj)
                {
                    _clients.Add(clientSocket);
                }

                // No, we don't need to broadcast the message immediately after accepting a client.
                // BroadcastMessage(clientSocket); //message = backup job object list

                // Create a new thread to handle the client
                Thread clientThread = new Thread(() => ListenToClient(clientSocket));
                clientThread.Start();
            }
        }

        private Socket Connect()
        {
            if (IsRunning)
                throw new InvalidOperationException("Server is already running.");
            
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);
            //Console.WriteLine("Server started. Waiting for clients...");
            return serverSocket;
        }

        private void ListenToClient(Socket client)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int received = client.Receive(buffer);
                    if (received == 0) break;

                    // We assume the message is a JSON string
                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    // We receive the message from client and deserialize it
                    NetworkMessage? deserializedMessage = JsonConvert.DeserializeObject<NetworkMessage>(message);
                    if (deserializedMessage == null)
                    {
                        //Console.WriteLine("Received invalid message from client.");
                        continue;
                    }
                    
                    // We handle the message using the server network handler
                    _serverNetworkHandler.HandleNetworkMessage(client, deserializedMessage);
                }
            }
            catch (SocketException)
            {
                //Console.WriteLine("Client disconnected unexpectedly.");
            }
            finally
            {
                lock (_lockObj)
                {
                    _clients.Remove(client);
                }

                client.Close();
                //Console.WriteLine($"Client {client.RemoteEndPoint} disconnected.");
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
                        byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
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

        // TODO: Supprimer ça, c'est pas comme ça qu'on va faire la communication
        
        // public void ServerJsonDeserialize(JsonObject data)
        // {
        //     // Version
        //     data.TryGetPropertyValue("name", out var name);
        //     data.TryGetPropertyValue("source", out var source);
        //     data.TryGetPropertyValue("target", out var target);
        //     data.TryGetPropertyValue("action", out var action);
        //
        //     if (action == null)
        //     {
        //         throw new JsonException("No actions were given");
        //     }else if (action.ToString() == "create")
        //     { //TODO
        //         BackupJobViewModel.Get().BuildJobCommand.Execute();
        //     }
        //     else if (action.ToString() == "modify")
        //     { //TODO : no data context how do i call it ???
        //         ViewModel.DeleteJobCommand.Execute();
        //     }
        //     else if (action.ToString() == "delete")
        //     {
        //         ViewModel.DeleteJobCommand.Execute(name);
        //     }
        //     else if (action.ToString() == "delete")
        //     {
        //         ViewModel.RunJobCommand.Execute(name);
        //     }
        // }
    }
}
