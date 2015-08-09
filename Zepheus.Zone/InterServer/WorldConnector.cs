using System;
using System.Reflection;
using Zepheus.InterLib.Networking;
using Zepheus.InterLib.NetworkObjects;
using Zepheus.Util;

namespace Zepheus.Zone.InterServer
{
    [ServerModule(Util.InitializationStage.Services)]
    public sealed class WorldConnector : AbstractConnector
    {
        public static WorldConnector Instance { get; private set; }

        public WorldConnector(string ip, int port)
        {
            try
            {
                ConnectAndAssign(ip, port);
            }
            catch
            {
                Log.WriteLine(LogLevel.Error, "Couldn't connect to server @ {0}:{1}", ip, port);
                Console.ReadLine();
                Environment.Exit(7);
            }
        }
        private void ConnectAndAssign(string ip, int port)
        {
            Connect(ip, port);
            Log.WriteLine(LogLevel.Info, "Connected to server @ {0}:{1}", ip, port);
            this.client.OnPacket += new EventHandler<InterPacketReceivedEventArgs>(ClientOnPacket);
            this.client.OnDisconnect += new EventHandler<SessionCloseEventArgs>(ClientOnDisconnect);
            this.client.SendInterPass(Settings.Instance.InterPassword);
            InterHandler.TryAssiging(this);
        }

        void ClientOnDisconnect(object sender, SessionCloseEventArgs e)
        {
            Log.WriteLine(LogLevel.Error, "Disconnected from server.");
            this.client.OnPacket -= new EventHandler<InterPacketReceivedEventArgs>(ClientOnPacket);
            this.client.OnDisconnect -= new EventHandler<SessionCloseEventArgs>(ClientOnDisconnect);
            if (!Program.Shutdown)
            {
                // Try reconnect
                while (true)
                {
                    try
                    {
                        ConnectAndAssign(Settings.Instance.WorldServerIP, Settings.Instance.WorldServerPort);
                        break;
                    }
                    catch
                    {
                        Log.WriteLine(LogLevel.Warn, "Trying to reconnect in 5 seconds.");
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                Log.WriteLine(LogLevel.Warn, "We should be up again :)");
            }
        }

        void ClientOnPacket(object sender, InterPacketReceivedEventArgs e)
        {
            try
            {
                MethodInfo method = InterHandlerStore.GetHandler(e.Packet.OpCode);
                if (method != null)
                {
                    Action action = InterHandlerStore.GetCallback(method, this, e.Packet);
                    if (Worker.Instance == null)
                    {
                        action();
                    }
                    else
                    {
                        Worker.Instance.AddCallback(action);
                    }
                }
                else
                {
                    Log.WriteLine(LogLevel.Debug, "Unhandled packet: {0}", e.Packet);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, ex.ToString());
            }
        }

        [InitializerMethod]
        public static bool Load()
        {
            return Load(Settings.Instance.WorldServerIP, Settings.Instance.WorldServerPort);
        }

        public static bool Load(string ip, int port)
        {
            try
            {
                Instance = new WorldConnector(ip, port);
                return true;
            }
            catch { return false; }
        }

        public void SendPacket(InterPacket packet)
        {
            if (this.client == null) return;
            this.client.SendPacket(packet);
        }

        public void Disconnect()
        {
            if (this.client == null) return;
            this.client.Disconnect();
        }
    }
}
