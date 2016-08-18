namespace Estrella.Zone
{
    public sealed class Settings
    {
        public const int SettingsVersion = 2;

        public int? Version { get; set; }
        public string IP { get; set; }
        public bool Debug { get; set; }
        public int WorkInterval { get; set; }
        public int TransferTimeout { get; set; }

        public string WorldServiceUri { get; set; }
        public string InterPassword { get; set; }
        public string WorldServerIP { get; set; }
        public ushort WorldServerPort { get; set; }
        public ushort InterServerPort { get; set; }
        public string zoneMysqlServer { get; set; }
        public int zoneMysqlPort { get; set; }
        public string zoneMysqlUser { get; set; }
        public string zoneMysqlPassword { get; set; }
        public string zoneMysqlDatabase { get; set; }
        public string WorldMysqlServer { get; set; }
        public int WorldMysqlPort { get; set; }
        public string WorldMysqlUser { get; set; }
        public string WorldMysqlPassword { get; set; }
        public string WorldMysqlDatabase { get; set; }
        public uint WorldDBMinPoolSizeZoneWorld { get; set; }
        public uint WorldDBMaxPoolSizeZoneWorld { get; set; }
        public static Settings Instance { get; set; }
        public string ConnString { get; set; }
        public string WorldConnString { get; set; }
        public uint ZoneDBMinPoolSize { get; set; }
        public uint ZoneDBMaxPoolSize { get; set; }
        public int OverloadFlags { get; set; }
        public int QuerCachePerClient { get; set; }
        public int OverloadFlagsZoneWorld { get; set; }
        public int QuerCachePerClientZoneWorld { get; set; }
        public ulong TicksToSleep { get; set; }
        public int SleepTime { get; set; }

        public static bool Load()
        {
            try
            {
                Settings obj = new Settings()
                {
                    // V.1
                    WorldServerIP = Estrella.InterLib.Settings.GetString("Zone.WorldServerIP"),
                    WorldServerPort = (ushort)Estrella.InterLib.Settings.GetInt32("Zone.WorldServerPort"),
                    IP = Estrella.InterLib.Settings.GetString("Zone.IP"),
                    Debug = Estrella.InterLib.Settings.GetBool("Zone.Debug"),

                    WorkInterval = Estrella.InterLib.Settings.GetInt32("Zone.WorkInterval"),
                    TransferTimeout = Estrella.InterLib.Settings.GetInt32("Zone.TransferTimeout"),

                    WorldServiceUri = Estrella.InterLib.Settings.GetString("Zone.WorldServiceURI"),
                    InterPassword = Estrella.InterLib.Settings.GetString("Zone.Password"),
                    zoneMysqlServer = Estrella.InterLib.Settings.GetString("Data.Mysql.Server"),
                    zoneMysqlPort = Estrella.InterLib.Settings.GetInt32("Data.Mysql.Port"),
                    zoneMysqlUser = Estrella.InterLib.Settings.GetString("Data.Mysql.User"),
                    zoneMysqlPassword = Estrella.InterLib.Settings.GetString("Data.Mysql.Password"),
                    zoneMysqlDatabase = Estrella.InterLib.Settings.GetString("Data.Mysql.Database"),
                    WorldMysqlServer = Estrella.InterLib.Settings.GetString("World.Mysql.Server"),
                    ZoneDBMinPoolSize = (uint)Estrella.InterLib.Settings.GetInt32("Data.Mysql.MinPool"),
                    ZoneDBMaxPoolSize = (uint)Estrella.InterLib.Settings.GetInt32("Data.Mysql.MaxPool"),
                    WorldMysqlPort = Estrella.InterLib.Settings.GetInt32("World.Mysql.Port"),
                    WorldMysqlUser = Estrella.InterLib.Settings.GetString("World.Mysql.User"),
                    WorldMysqlPassword = Estrella.InterLib.Settings.GetString("World.Mysql.Password"),
                    WorldMysqlDatabase = Estrella.InterLib.Settings.GetString("World.Mysql.Database"),
                    QuerCachePerClientZoneWorld = Estrella.InterLib.Settings.GetInt32("ZoneWorld.Mysql.QuerCachePerClient"),
                    OverloadFlagsZoneWorld = Estrella.InterLib.Settings.GetInt32("ZoneWorld.Mysql.OverloadFlags"),
                    QuerCachePerClient = Estrella.InterLib.Settings.GetInt32("Data.Mysql.QuerCachePerClient"),
                    OverloadFlags = Estrella.InterLib.Settings.GetInt32("Data.Mysql.OverloadFlags"),
                    WorldDBMinPoolSizeZoneWorld = (uint)Estrella.InterLib.Settings.GetInt32("ZoneWorld.Mysql.MinPool"),
                    WorldDBMaxPoolSizeZoneWorld = (uint)Estrella.InterLib.Settings.GetInt32("ZoneWorld.Mysql.MaxPool"),
                    TicksToSleep = Estrella.InterLib.Settings.GetUInt32("Zone.TicksToSleep"),
                    SleepTime = Estrella.InterLib.Settings.GetInt32("Zone.SleepTime"),
                };
                obj.WorldConnString = " User ID=" + obj.WorldMysqlUser + ";Password=" + obj.WorldMysqlPassword + ";Host=" + obj.WorldMysqlServer + ";Port=" + obj.WorldMysqlPort + ";Database=" + obj.WorldMysqlDatabase + ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
                obj.ConnString = " User ID=" + obj.zoneMysqlUser + ";Password=" + obj.zoneMysqlPassword + ";Host=" + obj.zoneMysqlServer + ";Port=" + obj.zoneMysqlPort + ";Database=" + obj.zoneMysqlDatabase + ";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size=0;Max Pool Size=2000;Connection Lifetime=0;";
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
