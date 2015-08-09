using System;
using System.Collections.Generic;
using System.Reflection;
using Zepheus.InterLib.Networking;
using Zepheus.Util;

namespace Zepheus.World.InterServer
{
    [ServerModule(Util.InitializationStage.Metadata)]
    public class InterHandlerStore
    {
        private static Dictionary<InterHeader, MethodInfo> Handlers;

        [InitializerMethod]
        public static bool Load()
        {
            Handlers = new Dictionary<InterHeader,MethodInfo>();
            foreach (var info in Reflector.FindMethodsByAttribute<InterPacketHandlerAttribute>())
            {
                InterPacketHandlerAttribute attribute = info.First;
                MethodInfo method = info.Second;
                if (!Handlers.ContainsKey(attribute.Header)) {
                    Handlers[attribute.Header] = method;
                }
                else {
                    Log.WriteLine(LogLevel.Warn, "Duplicate interhandler found: {0}", attribute.Header.ToString());
                }
            }

            Log.WriteLine(LogLevel.Info, "{0} InterHandlers loaded.", Handlers.Count);
            return true;
        }

        public static MethodInfo GetHandler(InterHeader ih)
        {
            MethodInfo meth;
            if (Handlers.TryGetValue(ih, out meth))
            {
                return meth;
            }
            return null;
        }

        public static Action GetCallback(MethodInfo method, params object[] parameters)
        {
            return () => method.Invoke(null, parameters);
        }
    }
}
