namespace Estrella.Login
{
    public sealed class Settings
    {
        public const int SettingsVersion = 2;

        public int? Version { get; set; }
        public ushort Port { get; set; }
        public bool Debug { get; set; }
        public int WorkInterval { get; set; }
        public string LoginMysqlServer { get; set; }
        public int LoginMysqlPort { get; set; }
        public string LoginMysqlUser { get; set; }
        public string LoginMysqlPassword { get; set; }
        public string LoginMysqlDatabase { get; set; }
        public uint LoginDBMinPoolSize { get; set; }
        public uint LoginDBMaxPoolSize { get; set; }
        public string LoginServiceUri { get; set; }
        public int OverloadFlags { get; set; }
        public int QuerCachePerClient { get; set; }
        public string InterPassword { get; set; }
        public ushort InterServerPort { get; set; }
        public static Settings Instance { get; set; }
        public string ConnString { get; set; }
        public static bool Load()
        {
      try
      {
            Settings obj = new Settings()
            {
                InterServerPort = (ushort)Estrella.InterLib.Settings.GetInt32("Login.InterServerPort"),
                Port = (ushort)Estrella.InterLib.Settings.GetInt32("Login.Port"),
                Debug = Estrella.InterLib.Settings.GetBool("Login.Debug"),
                WorkInterval = Estrella.InterLib.Settings.GetInt32("Login.WorkInterVal"),
                LoginServiceUri = Estrella.InterLib.Settings.GetString("Login.LoginServiceURI"),
                InterPassword =  Estrella.InterLib.Settings.GetString("Login.InterPassword"),
                LoginMysqlServer = Estrella.InterLib.Settings.GetString("Login.Mysql.Server"),
                LoginMysqlPort = Estrella.InterLib.Settings.GetInt32("Login.Mysql.Port"),
                LoginMysqlUser = Estrella.InterLib.Settings.GetString("Login.Mysql.User"),
                LoginMysqlPassword = Estrella.InterLib.Settings.GetString("Login.Mysql.Password"),
                LoginMysqlDatabase = Estrella.InterLib.Settings.GetString("Login.Mysql.Database"),
                LoginDBMinPoolSize = Estrella.InterLib.Settings.GetUInt32("Login.Mysql.MinPool"),
                LoginDBMaxPoolSize = Estrella.InterLib.Settings.GetUInt32("Login.Mysql.MaxPool"),
                QuerCachePerClient = Estrella.InterLib.Settings.GetInt32("Login.Mysql.QuerCachePerClient"),
                OverloadFlags = Estrella.InterLib.Settings.GetInt32("Login.Mysql.OverloadFlags"),
               
                Version = SettingsVersion,
            };
                obj.ConnString =  " User ID="+obj.LoginMysqlUser+";Password="+obj.LoginMysqlPassword+";Host="+obj.LoginMysqlServer+";Port="+obj.LoginMysqlPort+";Database="+obj.LoginMysqlDatabase+";Protocol=TCP;Compress=false;Pooling=true;Min Pool Size="+obj.LoginDBMinPoolSize+";Max Pool Size="+obj.LoginDBMaxPoolSize+";Connection Lifetime=0;";
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
