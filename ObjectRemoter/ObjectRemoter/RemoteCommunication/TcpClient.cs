﻿using System.IO;

namespace ObjectRemoter.RemoteCommunication
{
    /// <summary>
    /// A remote communication client using TCP.
    /// </summary>
    internal class TcpClient : IClient
    {
        /// <summary>
        /// Address of server to connect to.
        /// </summary>
        public ServerAddress ServerAddress { get; private set; }

        public TcpClient(ServerAddress serverAddress)
        {
            ServerAddress = serverAddress;
        }

        public string Request(string command, string[] data)
        {
            // TODO: System.Net.Sockets.SocketException must be handled when
            // it is not possible to connect to the server.
            // This TODO should be removed since it is resolved on higher level in RemoteObjectProxyProvider.ProxyInterceptor.Intercept()
            var client = new System.Net.Sockets.TcpClient(ServerAddress.IPAddress.ToString(), ServerAddress.Port);
            using (var clientStream = client.GetStream())
            {
                var sw = new StreamWriter(clientStream);

                command = command
                    .Replace("\r", TcpServer.CarriageReturnReplacement)
                    .Replace("\n", TcpServer.LineFeedReplacement);
                sw.WriteLine(command);

                sw.WriteLine(data.Length.ToString());
                foreach (string item in data)
                {
                    string encodedItem = item
                        .Replace("\r", TcpServer.CarriageReturnReplacement)
                        .Replace("\n", TcpServer.LineFeedReplacement);
                    sw.WriteLine(encodedItem);
                }

                sw.Flush();

                var sr = new StreamReader(clientStream);
                string result = sr.ReadLine();
                result = result
                    .Replace(TcpServer.CarriageReturnReplacement, "\r")
                    .Replace(TcpServer.LineFeedReplacement, "\n");
                return result;
            }
        }
    }
}
