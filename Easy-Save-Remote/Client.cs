using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace Easy_Save_WPF
{
    internal class Client
    {
        //private static void Main()
        //{
           // Socket clientSocket = Connect();

            //Thread receiveThread = new Thread(() => LoadData(clientSocket));
            //receiveThread.Start();

            //while (true)
            //{
              //  string? message = Console.ReadLine();
                //if (message == null) continue;
                //clientSocket.Send(Encoding.UTF8.GetBytes(message));

                //if (message.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
                  //  break;
            //}

            //Disconnect(clientSocket);
        //}

        public Socket Connect(string url, int port)
        {
            IPAddress address = IPAddress.Parse(url);
            IPEndPoint serverEndPoint = new IPEndPoint(address, port);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Connected to server.");
            return clientSocket;
        }

        public void LoadData(Socket client)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int received = client.Receive(buffer);
                    if (received == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    string json = JsonConvert.SerializeObject(message);
                }
                catch
                {
                    //Console.WriteLine("Disconnected from server.");
                    break;
                }
            }
        }

        public void Disconnect(Socket socket)
        {
            socket.Close();
        }
    }
}
