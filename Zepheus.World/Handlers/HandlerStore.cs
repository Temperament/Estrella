using System;
using System.Collections.Generic;
using System.Reflection;
using Zepheus.FiestaLib.Networking;
using Zepheus.Util;

namespace Zepheus.World.Handlers
{
    [ServerModule(Util.InitializationStage.Metadata)]
    class HandlerStore
    {
        private static Dictionary<byte, Dictionary<byte, MethodInfo>> Handlers;

        [InitializerMethod]
        public static bool Load()
        {
            Handlers = new Dictionary<byte, Dictionary<byte, MethodInfo>>();
            foreach (var info in Reflector.FindMethodsByAttribute<PacketHandlerAttribute>())
            {
                PacketHandlerAttribute attribute = info.First;
                MethodInfo method = info.Second;
                if (!Handlers.ContainsKey(attribute.Header))
                    Handlers.Add(attribute.Header, new Dictionary<byte, MethodInfo>());
                if (Handlers[attribute.Header].ContainsKey(attribute.Type))
                {
                    Log.WriteLine(LogLevel.Warn, "Duplicate handler found: {0}:{1}", attribute.Header, attribute.Type);
                    Handlers[attribute.Header].Remove(attribute.Type);
                }
                Handlers[attribute.Header].Add(attribute.Type, method);
            }

            int count = 0;
            foreach (var dict in Handlers.Values)
                count += dict.Count;
            Log.WriteLine(LogLevel.Info, "{0} Handlers loaded.", count);
            return true;
        }

        public static MethodInfo GetHandler(byte header, byte type)
        {
            Dictionary<byte, MethodInfo> dict;
            MethodInfo meth;
            if (Handlers.TryGetValue(header, out dict))
            {
                if (dict.TryGetValue(type, out meth))
                {
                    return meth;
                }
            }
            return null;
        }

        public static Action GetCallback(MethodInfo method, params object[] parameters)
        {
            return () => method.Invoke(null, parameters);
        }
    }
}
