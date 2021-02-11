using System;
using System.Collections.Generic;
using System.IO;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Radio
{
    public class RadioHandler
    {
        private static readonly string radioFile = $"{Directory.GetCurrentDirectory()}/data/radios.json";
        
        public static void LoadRadioChannels()
        {
            try
            {
                Console.WriteLine("Loading Radio Channels");
                
                if (!File.Exists(radioFile))
                {
                    RadioChannels = new List<RadioChannel>
                    {
                        new RadioChannel(911, new List<int>
                        {
                            1
                        }, true)
                    };
                
                    File.WriteAllText(radioFile, JsonConvert.SerializeObject(RadioChannels, Formatting.Indented));
                    
                    Console.WriteLine($"Radio Channels Loaded. {RadioChannels.Count}");
                }
                else
                {
                    string contents = File.ReadAllText(radioFile);

                    RadioChannels = JsonConvert.DeserializeObject<List<RadioChannel>>(contents);
                    
                    Console.WriteLine($"Radio Channels Loaded. {RadioChannels.Count}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static List<RadioChannel> RadioChannels = new List<RadioChannel>();
    }
}