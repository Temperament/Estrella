/*    This file is part of Zepheus Emulator

    Zepheus Emulator is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Zepheus Emulator is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Zepheus Emulator.  If not, see <http://www.gnu.org/licenses/>.  */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Zepheus.Util;

namespace Zepheus.InterLib
{
    [ServerModule(InitializationStage.Settings)]
    public class Settings
    {
        private const string ConfigName = "\\Config.cfg";
        private static readonly string configPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + ConfigName;

        private static readonly Settings instance = new Settings();
        public static string Comments { get { return comments; } }
        private static string comments = string.Empty;
        // TS: This is otherwise known as a dictionary:
        //private static List<KeyValuePair<object, object>> Properties;
        private readonly Dictionary<string, object> properties;
        private bool isInitialized;
        public static Random Random { get; private set; }

        private Settings()
        {
            this.properties = new Dictionary<string, object>();
            this.isInitialized = false;
        }

        /// <summary>
        /// Automatically loads settings from config file
        /// </summary>
        [InitializerMethod]
        public static bool Initialize()
        {
            return instance.InitializeInternal();
        }

        private bool InitializeInternal()
        {
            if (this.isInitialized) return true;
            try
            {
                this.ParseFile(configPath);
            }
            catch (Exception ex)
            {
                Log.WriteLine(LogLevel.Exception, "Error reading settings: {0}", ex.Message);
                return false;
            }
            Random = new Random(DateTime.Now.Second);
            Log.WriteLine(LogLevel.Info, "Settings loaded successfully.");
            this.isInitialized = true;
            return true;
        }

        #region Get methods

        /// <summary>
        /// Gets a Boolean from the file
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>true if value is true, else returns false</returns>
        public static bool GetBool(string key)
        {
            return GetString(key).ToLower() == "true";
        }

        /// <summary>
        /// Gets an Int32 type variable from the file
        /// </summary>
        /// <param name="key">The key to get the value from</param>
        /// <returns>'key's Int32 value</returns>
        public static int GetInt32(string key)
        {
            return Convert.ToInt32(instance.properties[key]);
        }
        public static uint GetUInt32(string key)
        {
            return Convert.ToUInt32(instance.properties[key]);
        }
        /// <summary>
        /// Gets an Int16 type variable from the file
        /// </summary>
        /// <param name="key">The key to get the value from</param>
        /// <returns>'key's Int16 value</returns>
        public static short GetInt16(string key)
        {
            return Convert.ToInt16(instance.properties[key]);
        }

        /// <summary>
        /// Gets an Byte type variable from the file
        /// </summary>
        /// <param name="key">The key to get the value from</param>
        /// <returns>'key's Byte value</returns>
        public static byte GetByte(string key)
        {
            return Convert.ToByte(instance.properties[key]);
        }

        /// <summary>
        /// Gets a String type variable from the file
        /// </summary>
        /// <param name="key">The key to get the value from</param>
        /// <returns>'key's String vaule</returns>
        public static string GetString(string key)
        {
            return instance.properties[key].ToString();
        }

        #endregion

        /// <summary>
        /// Reads the file and parse it into a List of Key Vaule Pairs.
        /// </summary>
        /// <param name="fileName">filepath</param>
        /// <returns>List of Key Value Pairs</returns>
        private void ParseFile(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);

            foreach (string entry in lines.Select(line => line.Trim()).Where(line => line.Length > 0))
            {
                if (!entry.Contains("#"))
                {
                    string[] parts = entry.Split('=');
                    if (parts.Length != 2) continue;

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    if (!properties.ContainsKey(key))
                    {
                        properties.Add(key, value);
                    }
                }
                else
                {
                    comments += Environment.NewLine + entry.Remove(0, 1);
                }
            }
        }
    }
}
