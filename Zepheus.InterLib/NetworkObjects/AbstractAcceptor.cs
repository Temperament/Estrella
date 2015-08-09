using System;
using System.Net;
using System.Net.Sockets;

namespace Zepheus.InterLib.NetworkObjects
{
	public class AbstractAcceptor
	{
		public event OnIncomingConnectionDelegate OnIncommingConnection;
		private readonly TcpListener listener;
		public ulong AcceptedClients { get; private set; }

		public AbstractAcceptor(int port)
		{
			listener = new TcpListener(IPAddress.Any, port);
			listener.Start(5);
			StartReceive();
		}

		private void StartReceive()
		{
			listener.BeginAcceptSocket(new AsyncCallback(EndReceive), null);
		}

		private void EndReceive(IAsyncResult iar)
		{
			Socket socket = listener.EndAcceptSocket(iar);
			if (socket != null && OnIncommingConnection != null)
			{
				OnIncommingConnection(socket);
			}
			AcceptedClients++;
			StartReceive();
		}
	}
}
