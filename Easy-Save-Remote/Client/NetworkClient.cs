using System.Net;
using System.Net.Sockets;
using System.Text;
using EasySaveCore.Server.DataStructures;
using Newtonsoft.Json;

namespace Easy_Save_Remote
{
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

        public Socket Connect(string url, int port)
        {
            IPAddress address = IPAddress.Parse(url);
            IPEndPoint serverEndPoint = new IPEndPoint(address, port);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverEndPoint);
            IsRunning = true;
            return _clientSocket = clientSocket;
        }

        public void LoadData()
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