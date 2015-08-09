using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Zepheus.Util
{
    public static class Log
    {
        private static readonly Mutex locker = new Mutex();
        public static bool IsDebug { get; set; }
        public static TextWriter Writer { get; set; }

        public static void WriteLine(LogLevel pLogLevel, string pFormat, params object[] pArgs)
        {
            if (pLogLevel == LogLevel.Debug && !IsDebug) return;
            string header = "[" + DateTime.Now + "] (" + pLogLevel + ") ";
            string buffer = string.Format(pFormat, pArgs);

            if (pLogLevel == LogLevel.Debug)
            {
                Debug.WriteLine(header + buffer);
            }

            locker.WaitOne();
            try
            {
                Console.ForegroundColor = GetColor(pLogLevel);
                Console.Write(header);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(buffer);
                if (Writer != null)
                {
                    Writer.WriteLine(header + buffer);
                }
            }
            finally
            {
                locker.ReleaseMutex();
            }
        }

        public static void SetLogToFile(string filename)
        {
            Directory.CreateDirectory(filename.Replace(Path.GetFileName(filename), ""));
            StreamWriter sw = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read));
            sw.AutoFlush = true;
            Writer = sw;
        }

        private static ConsoleColor GetColor(LogLevel pLevel)
        {
            switch (pLevel)
            {
                case LogLevel.Info:
                    return ConsoleColor.Green;
                case LogLevel.Warn:
                    return ConsoleColor.Yellow;
                case LogLevel.Debug:
                    return ConsoleColor.Magenta;
                case LogLevel.Error:
                    return ConsoleColor.DarkRed;
                case LogLevel.Exception:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }
    }

}
