using System;
using System.Net.Sockets;

using Zepheus.InterLib.Networking;

namespace Zepheus.InterLib.NetworkObjects
{
    public class AbstractConnector
    {
        public string IpAddress { get; private set; }
        public int Port { get; private set; }

        protected InterClient client;
        public bool Pong { get; private set; }
        public bool ForcedClose { get; private set; }

        public void Connect(string ip, int port)
        {
            IpAddress = ip;
            Port = port;
            ForcedClose = false;
            Connect();
        }

        public void Connect()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IpAddress, Port);
            client = new InterClient(tcpClient.Client);
            client.OnPacket += new EventHandler<InterPacketReceivedEventArgs>(ClientOnPacket);
        }

        void ClientOnPacket(object sender, InterPacketReceivedEventArgs e)
        {
            if (e.Packet.OpCode == InterHeader.Ping)
            {
                SendPong();
            }
            else if (e.Packet.OpCode == InterHeader.Pong)
            {
                Pong = true;
            }

        }

        public void SendPing()
        {
            Pong = false;
            using (var packet = new InterPacket(InterHeader.Ping))
            {
                client.SendPacket(packet);
            }
        }

        public void SendPong()
        {
            using (var packet = new InterPacket(InterHeader.Pong))
            {
                client.SendPacket(packet);
            }
        }
    }
}
