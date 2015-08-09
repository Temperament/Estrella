using System;
using System.Collections.Concurrent;
using System.Timers;
using Zepheus.FiestaLib;
using Zepheus.Login.InterServer;
using Zepheus.Util;

namespace Zepheus.Login
{
	[ServerModule(Util.InitializationStage.DataStore)]
	public sealed class WorldManager
	{
		public static WorldManager Instance { get; private set; }
		public ConcurrentDictionary<byte, WorldConnection> Worlds { get; private set; }
		public ConcurrentBag<int> InterServerConnections { get; private set; }
		public int WorldCount { get { return Worlds.Count; } }

		private readonly Timer updater; //updates the world loads

		public WorldManager()
		{
			Worlds = new ConcurrentDictionary<byte, WorldConnection>();

			updater = new Timer(10000);
			updater.Elapsed += new ElapsedEventHandler(UpdaterElapsed);
			updater.Start();
		}

		void UpdaterElapsed(object sender, ElapsedEventArgs e)
		{
			if (WorldCount == 0) return;
			foreach (var world in Worlds.Values)
			{
				int load = world.Load;
				if (world.Status == WorldStatus.Maintenance && load >= 0)
				{
					Log.WriteLine(LogLevel.Info, "{0} is out of maintenance.", world.Name);
				}
				if (load == -1)
				{
					if (world.Status != WorldStatus.Maintenance)
					{
						Log.WriteLine(LogLevel.Info, "{0} went into maintenance.", world.Name);
					}
					world.Status = WorldStatus.Maintenance;
				}
				else if (load == -2) //zones are offline
				{
					world.Status = WorldStatus.Offline;
				}
				else if (load < 2)
				{
					world.Status = WorldStatus.Low;
				}
				else if (load < 5)
				{
					world.Status = WorldStatus.Medium;
				}
				else
				{
					world.Status = WorldStatus.High;
				}
			}
		}

		[InitializerMethod]
		public static bool Load()
		{
			Instance = new WorldManager();
			Log.WriteLine(LogLevel.Info, "WorldManager initialized.");
			return true;
		}

		public void StartWorldService(Uri uri)
		{
			/*
			WorldServiceClient service = new WorldServiceClient(uri);
			WorldServiceInfo info = service.GetWorldInfo();
			Log.WriteLine(LogLevel.Info, "{0} worldserver linked at {1}", info.Name, info.IP);
			if (Worlds.ContainsKey(info.ID))
			{
				Log.WriteLine(LogLevel.Info, "World ID {0} reconnecting.", info.ID);
				Worlds[info.ID].Service.Close();
			}

			if (!Worlds.TryAdd(info.ID, new WorldInfo(info, service)))
			{
				Log.WriteLine(LogLevel.Error, "Error loading world {0}", info.Name);
			}
			*/
		}
	}
}
