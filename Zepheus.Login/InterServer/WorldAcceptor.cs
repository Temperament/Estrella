using System.Net.Sockets;
using Zepheus.InterLib.NetworkObjects;
using Zepheus.Util;

namespace Zepheus.Login.InterServer
{
	[ServerModule(Util.InitializationStage.Services)]
	public sealed class WorldAcceptor : AbstractAcceptor
	{
		public static WorldAcceptor Instance { get; private set; }

		public WorldAcceptor(int port) : base(port)
		{
			this.OnIncommingConnection += new OnIncomingConnectionDelegate(WorldAcceptor_OnIncommingConnection);
			Log.WriteLine(LogLevel.Info, "Listening on port {0}", port);
		}

		private void WorldAcceptor_OnIncommingConnection(Socket session)
		{
			// So something with it X:
			Log.WriteLine(LogLevel.Info, "Incomming connection from {0}", session.RemoteEndPoint);
			WorldConnection wc = new WorldConnection(session);
		}

		[InitializerMethod]
		public static bool Load()
		{
			return Load(Settings.Instance.InterServerPort);
		}

		public static bool Load(int port)
		{
			try
			{
				Instance = new WorldAcceptor(port);
				return true;
			}
			catch { return false; }
		}

	}
}
