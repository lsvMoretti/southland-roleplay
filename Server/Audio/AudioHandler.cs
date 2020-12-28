using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Objects;

namespace Server.Audio
{
    public class AudioHandler
    {
        /// <summary>
        /// List of Audio Stations
        /// </summary>
        public static List<RadioStation> StationList = new List<RadioStation>();

        private static readonly string altVDirectory = "C:/Game Server/data";

        /// <summary>
        /// Load Radio Stations
        /// </summary>
        public static void LoadRadioStations()
        {
            try
            {
                if (!File.Exists($"{altVDirectory}/stations.json"))
                {
                    Console.WriteLine($"An error occurred. stations.json file is empty!");
                    Console.WriteLine($"Creating Default Station List");

                    StationList = new List<RadioStation>
                    {
                        new RadioStation("181fm Classic Hits", "http://listen.livestreamingservice.com:8132"),
                        new RadioStation("181fm Good Time Oldies", "http://listen.livestreamingservice.com:8046"),
                        new RadioStation("181fm The Office",    "http://listen.livestreamingservice.com:8002"),
                        new RadioStation("181fm The Mix", "http://listen.livestreamingservice.com:8032")
                    };

                    SaveRadioStations();
                }
                else
                {
                    using StreamReader streamReader = new StreamReader($"{altVDirectory}/stations.json");
                    string json = streamReader.ReadToEnd();

                    StationList = new List<RadioStation>();

                    StationList = JsonConvert.DeserializeObject<List<RadioStation>>(json);

                    Console.WriteLine($"Successfully loaded interiors!");
                }

                Console.WriteLine($"Loaded {StationList.Count} Radio Stations!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Save Radio Stations
        /// </summary>
        public static void SaveRadioStations()
        {
            try
            {
                Console.WriteLine($"Compiling Interiors");

                if (!Directory.Exists($"{altVDirectory}"))
                {
                    Console.WriteLine($"Directory not found!");
                    Console.WriteLine($"{Directory.GetCurrentDirectory()}");
                    return;
                }

                Console.WriteLine($"Directory Exists");

                if (File.Exists($"{altVDirectory}/stations.json"))
                {
                    Console.WriteLine($"File exists, deleting");
                    File.Delete($"{altVDirectory}/stations.json");
                }

                Console.WriteLine($"Writing to file!");
                File.WriteAllText($"{altVDirectory}/stations.json", JsonConvert.SerializeObject(StationList, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Load Audio Control HTML page
        /// </summary>
        /// <param name="player"></param>
        public static void LoadStreamPage(IPlayer player)
        {
            player.FreezeInput(true);
            player.ChatInput(false);
            player.Emit("showAudioControl", JsonConvert.SerializeObject(StationList));
        }

        /// <summary>
        /// Stream selected from HTML page
        /// </summary>
        /// <param name="player"></param>
        /// <param name="stationName"></param>
        public static void StreamPageStreamSelected(IPlayer player, string stationName)
        {
            player.FreezeInput(false);
            player.ChatInput(true);

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You're not in a vehicle.");
                return;
            }

            if (player.Seat != 1)
            {
                if (player.Seat != 2)
                {
                    // Not driver or front passenger
                    player.SendErrorNotification("You're not in the front!");
                    return;
                }
            }

            RadioStation selectedStation = StationList.FirstOrDefault(x => x.StationName == stationName);

            if (selectedStation == null)
            {
                player.SendErrorNotification("An error occurred fetching this station!");
                return;
            }

            player.Vehicle.SetData("CURRENTMUSICSTREAM", selectedStation.StationUrl);

            foreach (KeyValuePair<byte, int> occupantId in player.Vehicle.Occupants())
            {
                IPlayer? targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == occupantId.Value);

                targetPlayer?.SendInfoNotification($"Now playing: {selectedStation.StationName}");
                targetPlayer?.PlayMusicFromUrl(selectedStation.StationUrl);
            }
        }

        /// <summary>
        /// Stop stream selected from HTML page
        /// </summary>
        /// <param name="player"></param>
        public static void StreamPageStopStream(IPlayer player)
        {
            player.FreezeInput(false);
            player.ChatInput(true);

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You're not in a vehicle.");
                return;
            }

            if (player.Seat != 1)
            {
                if (player.Seat != 2)
                {
                    // Not driver or front passenger
                    player.SendErrorNotification("You're not in the front!");
                    return;
                }
            }

            player.Vehicle.SetData("CURRENTMUSICSTREAM", string.Empty);

            foreach (KeyValuePair<byte, int> occupantId in player.Vehicle.Occupants())
            {
                IPlayer? targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == occupantId.Value);

                targetPlayer?.StopMusic();
            }
        }
    }
}