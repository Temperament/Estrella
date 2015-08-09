using System;
using System.Linq;
using System.IO;
using System.Security.Permissions;
using Zepheus.Database;
using Zepheus.Util;

namespace Zepheus.Login
{
    class Program
    {
        internal static  DatabaseManager DatabaseManager { get; set; }
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            //if debug we always start with default settings :)
#if DEBUG
            //File.Delete("Login.xml");
#endif
            
            Console.Title = "Zepheus.Login";
            if (Load())
            {
                Log.IsDebug = Settings.Instance.Debug;
                while (true)
                    Console.ReadLine();
            }
            else
            {
                Log.WriteLine(LogLevel.Error, "Could not start server. Press RETURN to exit.");
                Console.ReadLine();
            }
        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

            #region Logging
            #region Write Errors to a log file
            // Create a writer and open the file:
            StreamWriter log;

            if (!File.Exists("errorlog.txt"))
            {
                log = new StreamWriter("errorlog.txt");
            }
            else
            {
                log = File.AppendText("errorlog.txt");
            }

            // Write to the file:
            log.WriteLine(DateTime.Now);
            log.WriteLine(e.ToString());
            log.WriteLine();

            // Close the stream:
            log.Close();
            #endregion
            #endregion

            Log.WriteLine(LogLevel.Exception, "Unhandled Exception : " + e.ToString());
            Console.ReadKey(true);
        }
        public static bool Load()
        {
            Zepheus.InterLib.Settings.Initialize();
            Settings.Load();
            DatabaseManager = new DatabaseManager(Settings.Instance.LoginMysqlServer, (uint)Settings.Instance.LoginMysqlPort, Settings.Instance.LoginMysqlUser, Settings.Instance.LoginMysqlPassword, Settings.Instance.LoginMysqlDatabase, Settings.Instance.LoginDBMinPoolSize, Settings.Instance.LoginDBMaxPoolSize,Settings.Instance.QuerCachePerClient,Settings.Instance.OverloadFlags);
            DatabaseManager.GetClient(); //testclient
    
            Log.SetLogToFile(string.Format(@"Logs\Login\{0}.log", DateTime.Now.ToString("d_M_yyyy HH_mm_ss")));

            if (Reflector.GetInitializerMethods().Any(method => !method.Invoke()))
            {
                Log.WriteLine(LogLevel.Error, "Server could not be started. Errors occured.");
                return false;
            }
            else return true;
        }
    }
}
