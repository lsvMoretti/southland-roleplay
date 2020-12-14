using System;
using System.IO;
using System.Timers;
using Newtonsoft.Json;

namespace Server
{
    public class Settings
    {
        /// <summary>
        /// The current settings of the server
        /// </summary>
        public static ServerSettings ServerSettings = null;

        private static Timer _minuteTimer = null;

        /// <summary>
        /// Initialise the server settings object
        /// </summary>
        public static void InitServerSettings()
        {
            ServerSettings = ServerSettings.FetchServerSettings();

            _minuteTimer = new Timer { Interval = 60000, AutoReset = true };

            _minuteTimer.Elapsed += MinuteTimer_Elapsed;
            _minuteTimer.Start();
        }

        private static void MinuteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _minuteTimer.Stop();
            if (ServerSettings == null)
            {
                ServerSettings = ServerSettings.FetchServerSettings();
                _minuteTimer.Start();
                return;
            }

            ServerSettings.SaveServerSettings(ServerSettings);
            _minuteTimer.Start();
        }
    }

    public class ServerSettings
    {
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int WeatherLocation { get; set; }
        public DateTime LastDiscordUpdate { get; set; }
        public string MOTD { get; set; }

        private const string FileLocation = "data/settings.json";

        /// <summary>
        /// Fetches the current server settings
        /// </summary>
        /// <returns></returns>
        public static ServerSettings FetchServerSettings()
        {
            try
            {
                Console.WriteLine("Fetching current settings");

                if (!File.Exists(FileLocation))
                {
                    Console.WriteLine($"Creating new settings file at {FileLocation}");

                    ServerSettings newSettings = new ServerSettings { Hour = 0, Minute = 0, WeatherLocation = 5368361, LastDiscordUpdate = DateTime.MinValue, MOTD = "" };

                    File.WriteAllText(FileLocation, JsonConvert.SerializeObject(newSettings, Formatting.Indented));

                    return newSettings;
                }

                using StreamReader sr = new StreamReader(FileLocation);
                Console.WriteLine($"Reading server settings");

                string jsonString = sr.ReadToEnd();

                sr.Dispose();

                return JsonConvert.DeserializeObject<ServerSettings>(jsonString);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the current server settings to a file
        /// </summary>
        /// <param name="settings"></param>
        public static void SaveServerSettings(ServerSettings settings)
        {
            try
            {
                File.WriteAllText(FileLocation, JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            catch
            {
                return;
            }
        }
    }
}