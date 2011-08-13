﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace XRouter.Remoting.RemoteCommunication
{
    public class TcpServer : IServer
    {
        internal static readonly string LineFeedReplacement = "[[t*e$_/n]]";
        internal static readonly string CarriageReturnReplacement = "[[e$7@_/r]]";

        public ServerAddress Address { get; private set; }

        public event RequestHandler RequestArrived;

        private bool isStarted = false;

        public TcpServer()
        {
            Address = ServerAddress.GetLocalServerAddress();
        }

        public void Start()
        {
            if (isStarted) {
                return;
            }
            isStarted = true;            
            Task.Factory.StartNew(Run);
        }

        private void Run()
        {
            var listener = new TcpListener(IPAddress.Any, Address.Port);
            listener.Start();
            while (true) {
                var client = listener.AcceptTcpClient();
                Task.Factory.StartNew(HandleClient, client);
            }
        }

        private void HandleClient(object clientObject)
        {
            var client = (System.Net.Sockets.TcpClient)clientObject;
            using (var clientStream = client.GetStream()) {
                var sr = new StreamReader(clientStream);
                string command = sr.ReadLine();
                if (command == null) {
                    return;
                }
                string dataCountStr = sr.ReadLine();
                if (dataCountStr == null) {
                    return;
                }
                int dataCount = int.Parse(dataCountStr);
                string[] data = new string[dataCount];
                for (int i = 0; i < dataCount; i++) {
                    data[i] = sr.ReadLine();
                    if (data[i] == null) {
                        return;
                    }
                    data[i] = data[i]
                        .Replace(CarriageReturnReplacement, "\r")
                        .Replace(LineFeedReplacement, "\n");
                }

                string result = RequestArrived(command, data);
                result = result
                    .Replace("\r", CarriageReturnReplacement)
                    .Replace("\n", LineFeedReplacement);

                var sw = new StreamWriter(clientStream);
                sw.WriteLine(result);
                sw.Flush();
            }            
        }
    }
}