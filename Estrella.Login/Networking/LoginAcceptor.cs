using System;
using System.Net.Sockets;

using Estrella.FiestaLib.Networking;
using Estrella.Util;

namespace Estrella.Login.Networking
{
    [ServerModule(Util.InitializationStage.Networking)]
    public sealed class LoginAcceptor : Listener
    {
        public static LoginAcceptor Instance { get; private set; }
        public LoginAcceptor(int port)
            : base(port)
        {
            Start();
            Log.WriteLine(LogLevel.Info, "Accepting clients on port {0}", port);
        }

        public override void OnClientConnect(Socket socket)
        {
            LoginClient client = new LoginClient(socket);
            ClientManager.Instance.AddClient(client);
            Log.WriteLine(LogLevel.Debug, "Client connected from {0}", client.Host);
        }

        [InitializerMethod]
        public static bool Load()
        {
            try
            {
                Instance = new LoginAcceptor(Settings.Instance.Port);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "LoginAcceptor exception: {0}", ex.ToString());
                return false;
            }
        }
    }
}
