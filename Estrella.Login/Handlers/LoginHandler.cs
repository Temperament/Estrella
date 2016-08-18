using Estrella.Database;
using Estrella.FiestaLib;
using Estrella.FiestaLib.Networking;
using Estrella.Login.Networking;
using Estrella.Login.InterServer;
using Estrella.Util;
using System.Data;

namespace Estrella.Login.Handlers {
public sealed class LoginHandler {
    [PacketHandler(CH3Type.Version)]
    public static void VersionInfo(LoginClient pClient, Packet pPacket) {
        ushort year;
        ushort version;
        if (!pPacket.TryReadUShort(out year) ||
                !pPacket.TryReadUShort(out version)) {
            Log.WriteLine(LogLevel.Warn, "Invalid client version.");
            pClient.Disconnect();
            return;
        }
        Log.WriteLine(LogLevel.Debug, "Client version authenticated - Year: {0} Version: {1}.", year, version);
        using (Packet response = new Packet(SH3Type.VersionAllowed)) {
            response.WriteShort(1);
            pClient.SendPacket(response);
        }
    }

    [PacketHandler(CH3Type.Login)]
    public static void Login(LoginClient pClient, Packet pPacket) {
        // Initialize DB
        DatabaseClient dbClient = Program.DatabaseManager.GetClient();

        // XX XX XX XX XX XX XX XX XX XX XX - login length
        // 00 00 00 00 00 00 00 - space
        // XX XX XX XX XX XX XX - password
        // 00 00 00 00 00 00 00 00 00 4F 72 69 67 69 6E 61 6C 00 00 00 00 00 00 00 00 00 00 00

        // Define packet lengths, as these may change with client updates
        int packetLength = 54;

        int loginBlock = 11;
        int spaceLength = 7;
        int passwordBlock = 7;

        string md5 = pPacket.ReadStringForLogin(packetLength);
        char[] md5Char = md5.ToCharArray();
        string username = "";
        string clientPassword = "";

        // TODO - Escape these before query processing

        // Read from 0 --> 11
        for (int i = 0; i <= loginBlock; i++)
            username += md5Char[i].ToString().Replace("\0", "");

        Log.WriteLine(LogLevel.Debug, "{0} tries to login.", username);

        // Read from 18 --> onwards
        for (int i = loginBlock + spaceLength; i <= loginBlock + spaceLength + passwordBlock; i++)
            clientPassword += md5Char[i].ToString().Replace("\0", "");

        Log.WriteLine(LogLevel.Debug, "{0} tries to login.", clientPassword);

        DataTable loginData = null;

        using (dbClient)
        loginData = dbClient.ReadDataTable("SELECT * FROM accounts WHERE Username= '" + username + "'");

        // Auto account creation if no username found
        if (loginData.Rows.Count == 0) {
            dbClient.ExecuteQuery("INSERT INTO accounts (username, password) VALUES ('" + username + "','" + clientPassword + "')");

            using (dbClient)
            loginData = dbClient.ReadDataTable("SELECT * FROM accounts WHERE Username= '" + username + "'");
        }

        if (loginData != null) {
            if (loginData.Rows.Count > 0) {
                foreach (DataRow row in loginData.Rows) {
                    string uIsername = (string)row["username"];
                    string password = (string)row["password"];
                    bool banned = Database.DataStore.ReadMethods.EnumToBool(row["banned"].ToString());

                    if (clientPassword == password) {
                        if (banned) {
                            SendFailedLogin(pClient, ServerError.Blocked);
                            Log.WriteLine(LogLevel.Debug, "Banned user - {0} tries to login.", username);
                        }

                        else if (ClientManager.Instance.IsLoggedIn(uIsername)) {
                            Log.WriteLine(LogLevel.Warn, "{0} is trying dual login. Disconnecting.", uIsername);
                            pClient.Disconnect();

                            break;
                        } else {
                            pClient.Username = uIsername;
                            pClient.IsAuthenticated = true;
                            pClient.Admin = 0; /*(byte)row["Admin"];*/
                            pClient.AccountID = int.Parse(row["id"].ToString());
                            WorldList(pClient, false);
                        }
                    } else
                        SendFailedLogin(pClient, ServerError.InvalidCredentials);
                }
            }
        }
    }
    [PacketHandler(CH3Type.WorldReRequest)]
    public static void WorldReRequestHandler(LoginClient pClient, Packet pPacket) {
        if (!pClient.IsAuthenticated) {
            Log.WriteLine(LogLevel.Warn, "Invalid world list request.");
            return;
        }
        WorldList(pClient, true);
    }

    [PacketHandler(CH3Type.FileHash)]
    public static void FileHash(LoginClient pClient, Packet pPacket) {
        string hash;
        if (!pPacket.TryReadString(out hash)) {
            Log.WriteLine(LogLevel.Warn, "Empty filehash received.");
            SendFailedLogin(pClient, ServerError.Exception);
        } else
            AllowFiles(pClient, true);
    }

    [PacketHandler(CH3Type.WorldSelect)]
    public static void WorldSelectHandler(LoginClient pClient, Packet pPacket) {
        if (!pClient.IsAuthenticated || pClient.IsTransferring) {
            Log.WriteLine(LogLevel.Warn, "Invalid world select request.");
            SendFailedLogin(pClient, ServerError.Exception);
            return;
        }

        byte id;
        if (!pPacket.TryReadByte(out id)) {
            Log.WriteLine(LogLevel.Warn, "Invalid world select.");
            return;
        }
        WorldConnection world;

        if (WorldManager.Instance.Worlds.TryGetValue(id, out world)) {
            switch (world.Status) {
            case WorldStatus.Maintenance:
                Log.WriteLine(LogLevel.Warn, "{0} tried to join world in maintentance.", pClient.Username);
                SendFailedLogin(pClient, ServerError.ServerMaintenance);
                return;
            case WorldStatus.Offline:
                Log.WriteLine(LogLevel.Warn, "{0} tried to join offline world.", pClient.Username);
                SendFailedLogin(pClient, ServerError.ServerMaintenance);
                return;
            default:
                Log.WriteLine(LogLevel.Debug, "{0} joins world {1}", pClient.Username, world.Name);
                break;
            }
            string hash = System.Guid.NewGuid().ToString().Replace("-", "");


            world.SendTransferClientFromWorld(pClient.AccountID, pClient.Username, pClient.Admin, pClient.Host, hash);
            Log.WriteLine(LogLevel.Debug, "Transferring login client {0}.", pClient.Username);
            pClient.IsTransferring = true;
            SendWorldServerIP(pClient, world, hash);
        } else {
            Log.WriteLine(LogLevel.Warn, "{0} selected invalid world.", pClient.Username);
            return;
        }
    }

    private static void InvalidClientVersion(LoginClient pClient) {
        using (Packet pack = new Packet(SH3Type.IncorrectVersion)) {
            pack.Fill(10, 0);
            pClient.SendPacket(pack);
        }
    }

    private static void SendFailedLogin(LoginClient pClient, ServerError pError) {
        using (Packet pack = new Packet(SH3Type.Error)) {
            pack.WriteUShort((ushort)pError);
            pClient.SendPacket(pack);
        }
    }

    private static void AllowFiles(LoginClient pClient, bool pIsOk) {
        using (Packet pack = new Packet(SH3Type.FilecheckAllow)) {
            pack.WriteBool(pIsOk);
            pClient.SendPacket(pack);
        }
    }

    private static void WorldList(LoginClient pClient, bool pPing) {
        using (var pack = new Packet(pPing ? SH3Type.WorldistResend : SH3Type.WorldlistNew)) {
            pack.WriteByte(11);//worldmax count
            //pack.WriteByte((byte)WorldManager.Instance.WorldCount);
            foreach (var world in WorldManager.Instance.Worlds.Values) {
                pack.WriteByte(world.ID);
                pack.WriteString(world.Name, 16);
                pack.WriteByte((byte)world.Status);
            }
            for (int i = 0; i < (11 - WorldManager.Instance.Worlds.Count); i++) {
                pack.WriteByte((byte)i);
                pack.WriteString("DUMMY" + i, 16);
                pack.WriteByte((byte)WorldStatus.Offline);
            }
            pClient.SendPacket(pack);
        }
    }

    private static void SendWorldServerIP(LoginClient pClient, WorldConnection wc, string hash) {
        using (var pack = new Packet(SH3Type.WorldServerIP)) {
            pack.WriteByte((byte)wc.Status);

            pack.WriteString(wc.IP, 16);
            Log.WriteLine(LogLevel.Warn, "{0} - IP", wc.IP);
            pack.WriteUShort(wc.Port);
            pack.WriteString(hash, 32);
            pack.Fill(32, 0);
            pClient.SendPacket(pack);
        }
    }
}
}
