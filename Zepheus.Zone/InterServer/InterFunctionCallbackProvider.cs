using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Zepheus.Util;
using Zepheus.InterLib.Networking;

namespace Zepheus.Zone.InterServer
{
	[ServerModule(Util.InitializationStage.Networking)]
	public class InterFunctionCallbackProvider
	{
		#region .ctor

		public InterFunctionCallbackProvider()
		{
			waithandlers = new Dictionary<long,Mutex>();
			results = new Dictionary<long,object>();
			readFuncs = new Dictionary<long,Func<InterPacket,object>>();
			nextId = 0;
		}

		#endregion
		#region Properties

		public static InterFunctionCallbackProvider Instance { get; private set; }

		private Dictionary<long, Mutex> waithandlers;
		private Dictionary<long, object> results;
		private Dictionary<long, Func<InterPacket, object>> readFuncs;
		private long nextId;

		#endregion
		#region Methods

		[InitializerMethod]
		public static bool Initialize()
		{
			Instance = new InterFunctionCallbackProvider();
			return true;
		}
		public object QueuePacket(Func<long, InterPacket> getPacket, Func<InterPacket, object> readFromPacket)
		{
			long id = GetNextId();
			readFuncs.Add(id, readFromPacket);
			Mutex m;
			using(var packet = getPacket(id))
				m = QueueAndReturnMutex(packet, id);
			m.WaitOne();
			object returnValue = results[id];
			results.Remove(id);
			waithandlers.Remove(id);
			readFuncs.Remove(id);
			return returnValue;
		}

		internal Func<InterPacket, object> GetReadFunc(long id)
		{
			return readFuncs[id];
		}
		internal void OnResult(long id, object result)
		{
            if (id == 0)
            {
                return;
            }
			results.Add(id, result);
			waithandlers[id].ReleaseMutex();
		}

		private Mutex QueueAndReturnMutex(InterPacket packet, long id)
		{
			Mutex m = new Mutex();
			m.WaitOne();
			waithandlers.Add(id, m);
			WorldConnector.Instance.SendPacket(packet);
			return m;
		}
		private long GetNextId()
		{
			long id = nextId;
			nextId++;
			return id;
		}
		#endregion
	}
}
