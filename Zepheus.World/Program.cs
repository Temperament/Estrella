using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Zepheus.Database;
using Zepheus.Util;
using System.IO;
using System.Security.Permissions;
using Zepheus.World.InterServer;

namespace Zepheus.World
{
	class Program
	{
		public static bool Maintenance { get; set; }
		private static bool HandleCommands = true;
		public static Database.DatabaseManager DatabaseManager { get; set; }
        public static DateTime CurrentTime { get; set; }
		public static ConcurrentDictionary<byte, ZoneConnection> Zones { get; private set; }
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
		static void Main(string[] args)
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
			Console.Title = "Zepheus.World";
#if DEBUG
			Thread.Sleep(980);//give loginserver some time to start.
#endif
			if (Load())
			{
				Log.IsDebug = Settings.Instance.Debug;
				Zones = new ConcurrentDictionary<byte, ZoneConnection>();


				while (HandleCommands)
				{
					string line = Console.ReadLine();
					try
					{
						HandleCommand(line);
					}
					catch (Exception ex)
					{
						Log.WriteLine(LogLevel.Exception, "Could not parse: {0}; Error: {1}", line, ex.ToString());
					}
				}
				Log.WriteLine(LogLevel.Warn, "Shutting down the server..");
				CleanUp();
				Log.WriteLine(LogLevel.Info, "Server has been cleaned up. Program will now exit.");
			}
			else
			{
				Log.WriteLine(LogLevel.Error, "Errors occured starting server. Press RETURN to exit.");
				Console.ReadLine();
			}
		}

		private static void CleanUp()
		{
			foreach (var method in Reflector.GetCleanupMethods())
			{
				method();
			}
		}

		static void MyHandler(object sender, UnhandledExceptionEventArgs args)
		{
			Exception e = (Exception)args.ExceptionObject;

			#region Logging
			#region Write Errors to a log file
			// Create a writer and open the file:
			StreamWriter log;

			if (!File.Exists("errorlog.txt"))
			{
				log = new StreamWriter("errorlog.txt");
			}
			else
			{
				log = File.AppendText("errorlog.txt");
			}

			// Write to the file:
			log.WriteLine(DateTime.Now);
			log.WriteLine(e.ToString());
			log.WriteLine();

			// Close the stream:
			log.Close();
			#endregion
			#endregion

			Log.WriteLine(LogLevel.Exception, "Unhandled Exception : " + e);
			Console.ReadKey(true);
		}
		public static ZoneConnection GetZoneByMap(int id)
		{
			try
			{
				return Zones.Values.First(z => z.Maps.Count(m => m.ID == id) > 0);
			}
			catch
			{
				Log.WriteLine(LogLevel.Exception, "No zones are active at the moment.");
				return null;
			}
		}
        public static ZoneConnection GetZoneByMapShortName(string Name)
        {
            try
            {
                return Zones.Values.First(z => z.Maps.Count(m => m.ShortName == Name) > 0);
            }
            catch
            {
                Log.WriteLine(LogLevel.Exception, "No zones are active at the moment.");
                return null;
            }
        }
		public static void HandleCommand(string line)
		{
			string[] command = line.Split(' ');
			switch (command[0].ToLower())
			{
				case "maintenance":
					if (command.Length >= 2)
					{
						Maintenance = bool.Parse(command[1]);
					}
					break;
				case "shutdown":
					HandleCommands = false;
					break;
				case "exit":
					HandleCommands = false;
					break;
				case "quit":
					HandleCommands = false;
					break;
				default:
					Console.WriteLine("Command not recognized.");
					break;
			}
		}

		public static bool Load()
		{
			InterLib.Settings.Initialize();
			Settings.Load();
            DatabaseManager = new DatabaseManager(Settings.Instance.WorldMysqlServer, (uint)Settings.Instance.WorldMysqlPort, Settings.Instance.WorldMysqlUser, Settings.Instance.WorldMysqlPassword, Settings.Instance.WorldMysqlDatabase, Settings.Instance.WorldDBMinPoolSize, Settings.Instance.WorldDBMaxPoolSize, Settings.Instance.QuerCachePerClient,Settings.Instance.OverloadFlags);
			//DatabaseManager.GetClient(); //testclient
			Log.SetLogToFile(string.Format(@"Logs\World\{0}.log", DateTime.Now.ToString("d_M_yyyy HH_mm_ss")));

			try
			{
				if (Reflector.GetInitializerMethods().Any(method => !method.Invoke()))
				{
					Log.WriteLine(LogLevel.Error, "Server could not be started. Errors occured.");
					return false;
				}
				else return true;
			}
			catch (Exception ex)
			{
				Log.WriteLine(LogLevel.Exception, "Fatal exception while load: {0}:{1}", ex.ToString(), ex.StackTrace);
				return false;
			}
		}

		public static byte GetFreeZoneID()
		{
			for (byte i = 0; i < 3; i++)
			{
				if (Zones.ContainsKey(i)) continue;
				return i;
			}
			return 255;

		}
	}
}
