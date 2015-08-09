namespace Zepheus.Zone
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
                    WorldServerIP = Zepheus.InterLib.Settings.GetString("Zone.WorldServerIP"),
                    WorldServerPort = (ushort)Zepheus.InterLib.Settings.GetInt32("Zone.WorldServerPort"),
                    IP = Zepheus.InterLib.Settings.GetString("Zone.IP"),
                    Debug = Zepheus.InterLib.Settings.GetBool("Zone.Debug"),

                    WorkInterval = Zepheus.InterLib.Settings.GetInt32("Zone.WorkInterval"),
                    TransferTimeout = Zepheus.InterLib.Settings.GetInt32("Zone.TransferTimeout"),

                    WorldServiceUri = Zepheus.InterLib.Settings.GetString("Zone.WorldServiceURI"),
                    InterPassword = Zepheus.InterLib.Settings.GetString("Zone.Password"),
                    zoneMysqlServer = Zepheus.InterLib.Settings.GetString("Data.Mysql.Server"),
                    zoneMysqlPort = Zepheus.InterLib.Settings.GetInt32("Data.Mysql.Port"),
                    zoneMysqlUser = Zepheus.InterLib.Settings.GetString("Data.Mysql.User"),
                    zoneMysqlPassword = Zepheus.InterLib.Settings.GetString("Data.Mysql.Password"),
                    zoneMysqlDatabase = Zepheus.InterLib.Settings.GetString("Data.Mysql.Database"),
                    WorldMysqlServer = Zepheus.InterLib.Settings.GetString("World.Mysql.Server"),
                    ZoneDBMinPoolSize = (uint)Zepheus.InterLib.Settings.GetInt32("Data.Mysql.MinPool"),
                    ZoneDBMaxPoolSize = (uint)Zepheus.InterLib.Settings.GetInt32("Data.Mysql.MaxPool"),
                    WorldMysqlPort = Zepheus.InterLib.Settings.GetInt32("World.Mysql.Port"),
                    WorldMysqlUser = Zepheus.InterLib.Settings.GetString("World.Mysql.User"),
                    WorldMysqlPassword = Zepheus.InterLib.Settings.GetString("World.Mysql.Password"),
                    WorldMysqlDatabase = Zepheus.InterLib.Settings.GetString("World.Mysql.Database"),
                    QuerCachePerClientZoneWorld = Zepheus.InterLib.Settings.GetInt32("ZoneWorld.Mysql.QuerCachePerClient"),
                    OverloadFlagsZoneWorld = Zepheus.InterLib.Settings.GetInt32("ZoneWorld.Mysql.OverloadFlags"),
                    QuerCachePerClient = Zepheus.InterLib.Settings.GetInt32("Data.Mysql.QuerCachePerClient"),
                    OverloadFlags = Zepheus.InterLib.Settings.GetInt32("Data.Mysql.OverloadFlags"),
                    WorldDBMinPoolSizeZoneWorld = (uint)Zepheus.InterLib.Settings.GetInt32("ZoneWorld.Mysql.MinPool"),
                    WorldDBMaxPoolSizeZoneWorld = (uint)Zepheus.InterLib.Settings.GetInt32("ZoneWorld.Mysql.MaxPool"),
                    TicksToSleep = Zepheus.InterLib.Settings.GetUInt32("Zone.TicksToSleep"),
                    SleepTime = Zepheus.InterLib.Settings.GetInt32("Zone.SleepTime"),
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
