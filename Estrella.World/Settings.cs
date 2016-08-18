namespace Estrella.World
{
    public sealed class Settings
    {
        public const int SettingsVersion = 2;

        public int? Version { get; set; }
        public string WorldName { get; set; }
        public byte ID { get; set; }
        public string IP { get; set; }
        public ushort Port { get; set; }
        public ushort ZoneBasePort { get; set; }
        public ushort ZoneCount { get; set; }

        public int TransferTimeout { get; set; }
        public bool Debug { get; set; }
        public int WorkInterval { get; set; }

        public string LoginServiceUri { get; set; }
        public string WorldServiceUri { get; set; }
        public string GameServiceUri { get; set; }
        public string InterPassword { get; set; }
        public string LoginServerIP { get; set; }
        public ushort LoginServerPort { get; set; }
        public ushort InterServerPort { get; set; }
        public string WorldMysqlServer { get; set; }
        public int WorldMysqlPort { get; set; }
        public string WorldMysqlUser { get; set; }
        public string WorldMysqlPassword { get; set; }
        public string WorldMysqlDatabase { get; set; }
        public string zoneMysqlServer { get; set; }
        public int zoneMysqlPort { get; set; }
        public string zoneMysqlUser { get; set; }
        public string zoneMysqlPassword { get; set; }
        public string zoneMysqlDatabase { get; set; }
        public bool ShowEquips { get; set; }
        public string ConnString { get; set; }
        public string DataConnString { get; set; }
        public static Settings Instance { get; set; }
        public uint WorldDBMinPoolSize { get; set; }
        public uint WorldDBMaxPoolSize { get; set; }
        public int OverloadFlags { get; set; }
        public int QuerCachePerClient { get; set; }
        public ulong TicksToSleep { get; set; }
        public int SleepTime { get; set; }
        public static bool Load()
        {
            try{
                Settings obj = new Settings()
                {
                    Port = (ushort)Estrella.InterLib.Settings.GetInt32("World.Port"),
                    ZoneBasePort = (ushort)Estrella.InterLib.Settings.GetInt32("World.ZoneBase.Port"),
                    ZoneCount = (ushort)Estrella.InterLib.Settings.GetInt32("World.ZoneCount"),
                    IP = Estrella.InterLib.Settings.GetString("World.IP"),
                    Debug = Estrella.InterLib.Settings.GetBool("World.Debug"),
                    InterServerPort = (ushort)Estrella.InterLib.Settings.GetInt32("World.InterServerPort"),//zone lisner port
                    WorkInterval = Estrella.InterLib.Settings.GetInt32("World.WorkInterval"),
                    TransferTimeout = Estrella.InterLib.Settings.GetInt32("World.TranferTimeout"),
                    LoginServerIP = Estrella.InterLib.Settings.GetString("World.LoginServer.IP"),
                    LoginServerPort = (ushort)Estrella.InterLib.Settings.GetInt32("World.LoginServer.Port"),
                   
                    WorldName = Estrella.InterLib.Settings.GetString("World.Name"),
                    ID = Estrella.InterLib.Settings.GetByte("World.ID"),
                    ShowEquips = true,
                    LoginServiceUri = Estrella.InterLib.Settings.GetString("World.LoginServiceURI"),
                    WorldServiceUri = Estrella.InterLib.Settings.GetString("World.WorldServiceURI"),
                    GameServiceUri = Estrella.InterLib.Settings.GetString("World.GameServiceURI"),
                    InterPassword = Estrella.InterLib.Settings.GetString("World.InterPassword"),
                    WorldMysqlServer = Estrella.InterLib.Settings.GetString("World.Mysql.Server"),
                    WorldMysqlPort = Estrella.InterLib.Settings.GetInt32("World.Mysql.Port"),
                    WorldMysqlUser = Estrella.InterLib.Settings.GetString("World.Mysql.User"),
                    WorldMysqlPassword = Estrella.InterLib.Settings.GetString("World.Mysql.Password"),
                    WorldMysqlDatabase = Estrella.InterLib.Settings.GetString("World.Mysql.Database"),
                    zoneMysqlServer = Estrella.InterLib.Settings.GetString("Data.Mysql.Server"),
                    zoneMysqlPort = Estrella.InterLib.Settings.GetInt32("Data.Mysql.Port"),
                    zoneMysqlUser = Estrella.InterLib.Settings.GetString("Data.Mysql.User"),
                    zoneMysqlPassword = Estrella.InterLib.Settings.GetString("Data.Mysql.Password"),
                    zoneMysqlDatabase = Estrella.InterLib.Settings.GetString("Data.Mysql.Database"),
                    WorldDBMinPoolSize = Estrella.InterLib.Settings.GetUInt32("World.Mysql.MinPool"),
                    WorldDBMaxPoolSize = Estrella.InterLib.Settings.GetUInt32("World.Mysql.MaxPool"),
                    QuerCachePerClient = Estrella.InterLib.Settings.GetInt32("World.Mysql.QuerCachePerClient"),
                    OverloadFlags = Estrella.InterLib.Settings.GetInt32("World.Mysql.OverloadFlags"),
                    TicksToSleep = Estrella.InterLib.Settings.GetUInt32("World.TicksToSleep"),
                    SleepTime = Estrella.InterLib.Settings.GetInt32("World.SleepTime"),
                };
                obj.ConnString = " User ID=" + obj.WorldMysqlUser + ";Password=" + obj.WorldMysqlPassword + ";Host=" + obj.WorldMysqlServer + ";Port=" + obj.WorldMysqlPort + ";Database=" + obj.WorldMysqlDatabase + ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
                obj.DataConnString = " User ID=" + obj.zoneMysqlUser + ";Password=" + obj.zoneMysqlPassword + ";Host=" + obj.zoneMysqlServer + ";Port=" + obj.zoneMysqlPort + ";Database=" + obj.zoneMysqlDatabase + ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
                Settings.Instance = obj;
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
