using Zepheus.Database;
using Zepheus.FiestaLib;
using Zepheus.FiestaLib.Networking;
using Zepheus.Login.Networking;
using Zepheus.Login.InterServer;
using Zepheus.Util;
using System.Data;

namespace Zepheus.Login.Handlers
{
    public sealed class LoginHandler
    {
        [PacketHandler(CH3Type.Version)]
        public static void VersionInfo(LoginClient pClient, Packet pPacket)
        {
            ushort year;
            ushort version;
            if (!pPacket.TryReadUShort(out year) ||
                !pPacket.TryReadUShort(out version))
            {
                Log.WriteLine(LogLevel.Warn, "Invalid client version.");
                pClient.Disconnect();
                return;
            }
            Log.WriteLine(LogLevel.Debug, "Client version {0}:{1}.", year, version);
            using (Packet response = new Packet(SH3Type.VersionAllowed))
            {
                response.WriteShort(1);
                pClient.SendPacket(response);
            }
        }

        [PacketHandler(CH3Type.Login)]
        public static void Login(LoginClient pClient, Packet pPacket)
        {
            string md5 = pPacket.ReadStringForLogin(34);
            char[] tmpUserAndPass = md5.ToCharArray();
            string clientPassword = "";

            string username = "";

            for (int i = 0; i < 18; i++)
            {
                username += tmpUserAndPass[i].ToString().Replace("\0", "");
            }
            for (int i = 18; i < 34; i++)
            {
                //is not hash is password from client

                clientPassword += tmpUserAndPass[i].ToString().Replace("\0", "");
            }
            Log.WriteLine(LogLevel.Debug, "{0} tries to login.", username);

            bool banned = false;
            DataTable loginData = null;
            using (DatabaseClient dbClient = Program.DatabaseManager.GetClient())
            {
                loginData = dbClient.ReadDataTable("SELECT `ID`, `Username`, `Password`, `Admin`, `Blocked` FROM accounts WHERE Username= '" + username + "'");
            }
            if (loginData != null)
            {
                if (loginData.Rows.Count > 0)
                {
                    foreach (DataRow row in loginData.Rows)
                    {
                        string uIsername = (string)row["Username"];
                        string password = (string)row["Password"];
                        if (password == clientPassword)
                        {

                            banned = Database.DataStore.ReadMethods.EnumToBool(row["Blocked"].ToString());
                            if (banned == true)
                            {
                                SendFailedLogin(pClient, ServerError.Blocked);

                            }
                            else if (ClientManager.Instance.IsLoggedIn(uIsername))
                            {
                                Log.WriteLine(LogLevel.Warn, "{0} is trying dual login. Disconnecting.", uIsername);
                                pClient.Disconnect();

                                break;
                            }
                            else
                            {
                                pClient.Username = uIsername;
                                pClient.IsAuthenticated = true;
                                pClient.Admin = (byte)row["Admin"];
                                pClient.AccountID = int.Parse(row["ID"].ToString());
                                AllowFiles(pClient, true);
                                WorldList(pClient, false);
                            }
                        }
                        else
                        {
                            SendFailedLogin(pClient, ServerError.InvalidCredentials);
                        }
                    }
                }
                else
                {
                    SendFailedLogin(pClient, ServerError.DatabaseError);
                }
            }
        }
        [PacketHandler(CH3Type.WorldReRequest)]
        public static void WorldReRequestHandler(LoginClient pClient, Packet pPacket)
        {
            if (!pClient.IsAuthenticated)
            {
                Log.WriteLine(LogLevel.Warn, "Invalid world list request.");
                return;
            }
            WorldList(pClient, true);
        }

        [PacketHandler(CH3Type.FileHash)]
        public static void FileHash(LoginClient pClient, Packet pPacket)
        {
            string hash;
            if (!pPacket.TryReadString(out hash))
            {
                Log.WriteLine(LogLevel.Warn, "Empty filehash received.");
                SendFailedLogin(pClient, ServerError.Exception);
            }
            else
            {
                //allowfiles here fucks shit up?
            }
        }

        [PacketHandler(CH3Type.WorldSelect)]
        public static void WorldSelectHandler(LoginClient pClient, Packet pPacket)
        {
            if (!pClient.IsAuthenticated || pClient.IsTransferring)
            {
                Log.WriteLine(LogLevel.Warn, "Invalid world select request.");
                SendFailedLogin(pClient, ServerError.Exception);
                return;
            }

            byte id;
            if (!pPacket.TryReadByte(out id))
            {
                Log.WriteLine(LogLevel.Warn, "Invalid world select.");
                return;
            }
            WorldConnection world;
            if (WorldManager.Instance.Worlds.TryGetValue(id, out world))
            {
                switch (world.Status)
                {
                    case WorldStatus.Maintenance:
                        Log.WriteLine(LogLevel.Warn, "{0} tried to join world in maintentance.", pClient.Username);
                        SendFailedLogin(pClient, ServerError.ServerMaintenance);
                        return;
                    case WorldStatus.Offline:
                        Log.WriteLine(LogLevel.Warn, "{0} tried to join offline world.", pClient.Username);
                        SendFailedLogin(pClient, ServerError.ServerMaintenance);
                        return;
                    default: Log.WriteLine(LogLevel.Debug, "{0} joins world {1}", pClient.Username, world.Name); break;
                }
                string hash = System.Guid.NewGuid().ToString().Replace("-", "");


                world.SendTransferClientFromWorld(pClient.AccountID, pClient.Username, pClient.Admin, pClient.Host, hash);
                Log.WriteLine(LogLevel.Debug, "Transferring login client {0}.", pClient.Username);
                pClient.IsTransferring = true;
                SendWorldServerIP(pClient, world, hash);
            }
            else
            {
                Log.WriteLine(LogLevel.Warn, "{0} selected invalid world.", pClient.Username);
                return;
            }
        }

        private static void InvalidClientVersion(LoginClient pClient)
        {
            using (Packet pack = new Packet(SH3Type.IncorrectVersion))
            {
                pack.Fill(10, 0);
                pClient.SendPacket(pack);
            }
        }

        private static void SendFailedLogin(LoginClient pClient, ServerError pError)
        {
            using (Packet pack = new Packet(SH3Type.Error))
            {
                pack.WriteUShort((ushort)pError);
                pClient.SendPacket(pack);
            }
        }

        private static void AllowFiles(LoginClient pClient, bool pIsOk)
        {
            using (Packet pack = new Packet(SH3Type.FilecheckAllow))
            {
                pack.WriteBool(pIsOk);
                pClient.SendPacket(pack);
            }
        }

        private static void WorldList(LoginClient pClient, bool pPing)
        {
            using (var pack = new Packet(pPing ? SH3Type.WorldistResend : SH3Type.WorldlistNew))
            {
                pack.WriteByte(11);//worldmax count
                //pack.WriteByte((byte)WorldManager.Instance.WorldCount);
                foreach (var world in WorldManager.Instance.Worlds.Values)
                {
                    pack.WriteByte(world.ID);
                    pack.WriteString(world.Name, 16);
                    pack.WriteByte((byte)world.Status);
                }
                for (int i = 0; i < (11 - WorldManager.Instance.Worlds.Count); i++)
                {
                    pack.WriteByte((byte)i);
                    pack.WriteString("DUMMY" + i, 16);
                    pack.WriteByte((byte)WorldStatus.Offline);
                }
                pClient.SendPacket(pack);
            }
        }

        private static void SendWorldServerIP(LoginClient pClient, WorldConnection wc, string hash)
        {
            using (var pack = new Packet(SH3Type.WorldServerIP))
            {
                pack.WriteByte((byte)wc.Status);

                pack.WriteString(wc.IP, 16);

                pack.WriteUShort(wc.Port);
                pack.WriteString(hash, 32);
                pack.Fill(32, 0);
                pClient.SendPacket(pack);
            }
        }
    }
}
