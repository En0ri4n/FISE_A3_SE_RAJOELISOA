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

namespace EasySaveCore.Server
{
    internal class Server
    {
        private static readonly List<Socket> Clients = new List<Socket>();
        private static readonly object LockObj = new object();
        private static Socket socket;

        private static BackupJobConfiguration config = new BackupJobConfiguration();



        // private static void Main()
        // {
        //     Start();
        // }

        public void Start()
        {
            Socket serverSocket = Connect();
            
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                lock (LockObj)
                {
                    Clients.Add(clientSocket);
                }

                BroadcastMessage(clientSocket); //message = backup job object list 

                // Create a new thread to handle the client
                Thread clientThread = new Thread(() => ListenToClient(clientSocket));
                clientThread.Start();
            }

        }

        private static Socket Connect()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 5000);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);
            //Console.WriteLine("Server started. Waiting for clients...");
            return serverSocket;
        }

        private static void ListenToClient(Socket client)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int received = client.Receive(buffer);
                    if (received == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    dynamic json = JsonConvert.DeserializeObject(message);
                    config.JsonDeserialize(json);
                }
            }
            catch (SocketException)
            {
                //Console.WriteLine("Client disconnected unexpectedly.");
            }
            finally
            {
                lock (LockObj)
                {
                    Clients.Remove(client);
                }

                client.Close();
                //Console.WriteLine($"Client {client.RemoteEndPoint} disconnected.");
            }
        }

        private static void BroadcastMessage(Socket sender)
        {
            byte[] data = Encoding.UTF8.GetBytes(config.JsonSerialize().ToString());

            lock (LockObj)
            {
                foreach (Socket client in Clients.Where(client => client != sender).ToList())
                {
                    try
                    {
                        client.Send(data);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to send message to {client.RemoteEndPoint}. Removing client.");
                        lock (LockObj)
                        {
                            Clients.Remove(client);
                        }
                        client.Close();
                    }
                }
            }
        }

        public void ServerJsonDeserialize(JsonObject data)
        {
            // Version
            data.TryGetPropertyValue("name", out var name);
            data.TryGetPropertyValue("source", out var source);
            data.TryGetPropertyValue("target", out var target);
            data.TryGetPropertyValue("action", out var action);

            if (action == null)
            {
                throw new JsonException("No actions were given");
            }else if (action.ToString() == "create")
            { //TODO
                BackupJobViewModel.Get().BuildJobCommand.Execute();
            }
            else if (action.ToString() == "modify")
            { //TODO : no data context how do i call it ???
                ViewModel.DeleteJobCommand.Execute();
            }
            else if (action.ToString() == "delete")
            {
                ViewModel.DeleteJobCommand.Execute(name);
            }
        }
    }
}
