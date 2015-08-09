using System;
using System.Collections.Generic;
using Zepheus.Login.Networking;
using Zepheus.Util;

namespace Zepheus.Login
{
	[ServerModule(Util.InitializationStage.Clients)]
	public sealed class ClientManager
	{
		public static ClientManager Instance { get; private set; }

		private readonly List<LoginClient> clients = new List<LoginClient>();

		public bool IsConnected(string ip)
		{
			lock (clients)
			{
				LoginClient client = clients.Find(c => c.Host == ip);
				return (client != null);
			}
		}

		public bool IsLoggedIn(string username)
		{
			lock (clients)
			{
				LoginClient client = clients.Find(c => c.Username == username);
				return (client != null);
			}
		}

		public bool RemoveClient(LoginClient client)
		{
			lock (clients)
			{
				return clients.Remove(client);
			}
		}

	   

		public void AddClient(LoginClient client)
		{
			lock (clients)
			{
				clients.Add(client);
			}
		}

		[InitializerMethod]
		public static bool Load()
		{
			try
			{
				Instance = new ClientManager();
				Log.WriteLine(LogLevel.Info, "ClientManager Initialized.");
				return true;
			}
			catch (Exception exception) {
				Log.WriteLine(LogLevel.Exception, "ClientManager failed to initialize: {0}", exception.ToString());
				return false;
			}
		}
	}
}
