using System;
using System.Linq;
using System.Net;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Elasticsearch.Net.Specification.IndicesApi;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Extensions.Weather;

namespace Server
{
    public class TimeWeather
    {
        private static Timer _minuteTimer = null;
        public static OpenWeather CurrentWeather = null;
        public static WeatherType CurrentWeatherType = WeatherType.ExtraSunny;

        public static void InitTimeWeather()
        {
            Console.WriteLine($"Fetching current weather.");
            CurrentWeather = FetchCurrentWeather();
            Console.WriteLine($"Fetched latest weather for {CurrentWeather.name}.");
            _minuteTimer = new Timer(30000) { AutoReset = true };
            _minuteTimer.Elapsed += MinuteTimerOnElapsed;
            _minuteTimer.Start();
#if RELEASE

            Console.Title = $"alt:V Server - V{Utility.Build} - {Utility.LastUpdate}";

#endif

#if DEBUG
            Console.Title = $"alt:V Server - V{Utility.Build} - {Utility.LastUpdate} [DEBUG MODE]";
#endif
        }

        private static void MinuteTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _minuteTimer.Stop();

                ServerSettings serverSettings = Settings.ServerSettings;

                int oldHour = serverSettings.Hour;

                if (serverSettings.Hour >= 23 && serverSettings.Minute >= 59)
                {
                    serverSettings.Minute = 0;
                    serverSettings.Hour = 0;
                }
                else if (serverSettings.Minute >= 59)
                {
                    serverSettings.Minute = 0;
                    serverSettings.Hour += 1;
                }
                else
                {
                    serverSettings.Minute += 1;
                }

#if RELEASE

                SignalR.SendGameTime();
                Console.Title =
                    $"alt:V Server - Game Time: {serverSettings.Hour:D2}:{serverSettings.Minute:D2} - V{Utility.Build} - {Utility.LastUpdate}";

#endif

#if DEBUG
                Console.Title =
     $"alt:V Server - Game Time: {serverSettings.Hour}:{serverSettings.Minute} - V{Utility.Build} - {Utility.LastUpdate} [DEBUG MODE]";
#endif

                foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null).ToList())
                {
                    DateTime dateNow = DateTime.Now;

                    if (player.GetClass().CreatorRoom)
                    {
                        player.SetDateTime(dateNow.Day, dateNow.Month, dateNow.Year, 12, 0, 0);
                        player.SetWeather(WeatherType.Clear);
                        player.SetData("Weather:LastHour", -1);
                    }
                    else
                    {
                        bool hasLastWeather = player.GetData("Weather:LastHour", out int lastHour);

                        if (!hasLastWeather || oldHour != lastHour)
                        {
                            // Hasn't had last hour update or old hour is different to last update

                            player.SetDateTime(dateNow.Day, dateNow.Month, dateNow.Year, serverSettings.Hour, serverSettings.Minute, 0);
                            player.Emit("OnTimeUpdate");
                        }
                    }
                }

                DateTime timeNow = DateTime.Now;

                if (timeNow.Minute != 0 && timeNow.Minute != 15 && timeNow.Minute != 30 &&
                    timeNow.Minute != 45)
                {
                    _minuteTimer.Start();
                    return;
                }

                CurrentWeather = FetchCurrentWeather();

                foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null).ToList())
                {
                    if (player.GetClass().CreatorRoom) continue;
                    player.SetWeather((uint)CurrentWeatherType);
                }

                _minuteTimer.Start();
            }
            catch (Exception exception)
            {
                _minuteTimer.Start();
                Console.WriteLine(exception);
                return;
            }
        }

        /// <summary>
        /// Fetches the latest information from OpenWeather
        /// </summary>
        /// <returns></returns>
        private static OpenWeather FetchCurrentWeather()
        {
            try
            {
                ServerSettings serverSettings = Settings.ServerSettings;

                if (serverSettings == null)
                {
                    Settings.InitServerSettings();
                    serverSettings = ServerSettings.FetchServerSettings();
                }

                using WebClient wc = new WebClient();
                string updatedJson = wc.DownloadString(
                    $"https://api.openweathermap.org/data/2.5/weather?id={serverSettings.WeatherLocation}&mode=json&units=metric&APPID=37c1a999011411a01b4d200ea16e9b9a");
                wc.Dispose();

                OpenWeather currentWeather = JsonConvert.DeserializeObject<OpenWeather>(updatedJson);

                int currentWeatherType = currentWeather.weather.FirstOrDefault().id;

                int firstDigit = (int)(currentWeatherType.ToString()[0]);

                if (currentWeatherType >= 200 && currentWeatherType < 300)
                {
                    //Thunderstorm
                    CurrentWeatherType = WeatherType.Thunder;
                }

                if (currentWeatherType >= 300 && currentWeatherType < 400)
                {
                    //Drizzle
                    CurrentWeatherType = WeatherType.Overcast;
                }

                if (currentWeatherType >= 500 && currentWeatherType < 600)
                {
                    //Rain
                    CurrentWeatherType = WeatherType.Rain;
                }

                if (currentWeatherType >= 600 && currentWeatherType < 700)
                {
                    //Snow
                    if (currentWeatherType == 600 || currentWeatherType == 601)
                    {
                        //Light Snow
                        CurrentWeatherType = WeatherType.Snowlight;
                    }
                    else
                    {
                        CurrentWeatherType = WeatherType.Snow;
                    }
                }

                if (currentWeatherType == 701)
                {
                    // Mist
                    CurrentWeatherType = WeatherType.Smog;
                }

                if (currentWeatherType == 711)
                {
                    //Smoke
                    CurrentWeatherType = WeatherType.Smog;
                }

                if (currentWeatherType == 721)
                {
                    //Haze
                    CurrentWeatherType = WeatherType.Clouds;
                }

                if (currentWeatherType == 741)
                {
                    //Fog
                    CurrentWeatherType = WeatherType.Foggy;
                }

                if (currentWeatherType == 800)
                {
                    //Clear
                    CurrentWeatherType = WeatherType.ExtraSunny;
                }

                if (currentWeatherType >= 801 || currentWeatherType <= 804)
                {
                    //Clouds
                    if (currentWeatherType >= 801 && currentWeatherType <= 802)
                    {
                        CurrentWeatherType = WeatherType.Clouds;
                    }
                    else
                    {
                        CurrentWeatherType = WeatherType.Overcast;
                    }
                }

                DateTime currentTime = DateTime.Now;

#if RELEASE

                if (currentTime.Month == 12)
                {
                    if (currentTime.Day >= 18 && currentTime.Day <= 31)
                    {
                        CurrentWeatherType = WeatherType.Xmas;
                    }
                }

                if (currentTime.Day == 31 && currentTime.Month == 10)
                {
                    CurrentWeatherType = WeatherType.Halloween;
                }

#endif

                return currentWeather;
            }
            catch (Exception e)
            {
                CurrentWeatherType = WeatherType.ExtraSunny;
                Console.WriteLine($"An error occurred fetching the latest weather.");
                Console.WriteLine(e);
                return null;
            }
        }
    }
}