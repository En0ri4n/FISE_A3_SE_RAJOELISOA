using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EasySaveRemote.Client.DataStructures;
using Newtonsoft.Json;

namespace EasySaveRemote.Client
{
    /// <summary>
    /// Represents a client that connects to a remote server to send and receive network messages.
    /// This class handles the connection, message sending, and receiving operations.<br/>
    /// </summary>
    public class NetworkClient
    {
        public event OnConnectedHandler? OnConnected;
        public delegate void OnConnectedHandler(NetworkClient client);
        
        public event OnDisconnectedHandler? OnDisconnected;
        public delegate void OnDisconnectedHandler(NetworkClient client);

        private readonly ClientNetworkHandler _clientNetworkHandler;
        private Socket _clientSocket = null!;
        private bool IsRunning { get; set; }
        private Task _clientThread = null!;

        public NetworkClient()
        {
            _clientNetworkHandler = new ClientNetworkHandler(this, RemoteClient.Get().BackupJobManager);
        }

        /// <summary>
        /// Connects to a remote server using the specified URL and port.<br/>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string url, int port)
        {
            try
            {
                IPAddress address = IPAddress.Parse(url);
                IPEndPoint serverEndPoint = new IPEndPoint(address, port);
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.Connect(serverEndPoint);
                IsRunning = true;
                _clientThread = Task.Run(ListenToServer);
                OnConnected?.Invoke(this);
            }
            catch (Exception e)
            {
                return false;
            }
            
            return true;
        }

        private void ListenToServer()
        {
            byte[] buffer = new byte[4096]; // 4 KB buffer for receiving messages
            while (true)
            {
                try
                {
                    int received = _clientSocket.Receive(buffer);
                    if (received == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    NetworkMessage? networkMessage = NetworkMessage.Deserialize(message);
                    if (networkMessage == null)
                    {
                        //Console.WriteLine("Received invalid message from server.");
                        continue;
                    }
                    // We handle the message using the client network handler
                    _clientNetworkHandler.HandleNetworkMessage(networkMessage);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error receiving message: {e.Message}");
                    OnDisconnected?.Invoke(this);
                    throw;
                }
            }
        }

        /// <summary>
        /// Sends a network message to the connected server.<br/>
        /// This method serializes the message to JSON format before sending it over the socket connection.<br/>
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(NetworkMessage message)
        {
            _clientSocket.Send(Encoding.UTF8.GetBytes(message.Serialize()));
        }

        public void Disconnect()
        {
            if (!IsRunning)
                return;
            
            _clientSocket.Close();
            OnDisconnected?.Invoke(this);
        }
    }
}