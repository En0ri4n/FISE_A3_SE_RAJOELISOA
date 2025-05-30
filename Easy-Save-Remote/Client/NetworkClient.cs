using System.Net;
using System.Net.Sockets;
using System.Text;
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
        //TODO classe clients config avec attributs de config.json

        private readonly ClientNetworkHandler _clientNetworkHandler;
        private Socket _clientSocket = null!;
        private bool IsRunning { get; set; }

        public NetworkClient()
        {
            _clientNetworkHandler = new ClientNetworkHandler(this);
        }

        /// <summary>
        /// Connects to a remote server using the specified URL and port.<br/>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Socket Connect(string url, int port)
        {
            IPAddress address = IPAddress.Parse(url);
            IPEndPoint serverEndPoint = new IPEndPoint(address, port);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverEndPoint);
            IsRunning = true;
            return _clientSocket = clientSocket;
        }

        public void ListenToServer()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int received = _clientSocket.Receive(buffer);
                    if (received == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    NetworkMessage? networkMessage = JsonConvert.DeserializeObject<NetworkMessage>(message);
                    if (networkMessage == null)
                    {
                        //Console.WriteLine("Received invalid message from server.");
                        continue;
                    }
                    // We handle the message using the client network handler
                    _clientNetworkHandler.HandleNetworkMessage(networkMessage);
                }
                catch
                {
                    //Console.WriteLine("Disconnected from server.");
                    break;
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
            _clientSocket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
        }

        public void Disconnect()
        {
            _clientSocket.Close();
        }
    }
}