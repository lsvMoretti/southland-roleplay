using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace MapConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            bool readyToExit = false;
            int convertStage = 0;
            string mapName = "";

            Console.WriteLine($"Starting Mapping Converter - Created by Moretti");

            while (!readyToExit)
            {
                string userInput = Console.ReadLine();

                if(string.IsNullOrEmpty(userInput)) continue;
                
                if (userInput == "convert")
                {
                    if (convertStage != 0)
                    {
                        Console.WriteLine($"You already have a conversion in progress!");
                        continue;
                    }
                    Console.WriteLine($"Starting Map Conversion");
                    convertStage = 1;
                    Console.WriteLine($"Enter the name of the map.");
                    continue;
                }

                if (convertStage == 1)
                {
                    mapName = userInput;
                    Console.WriteLine($"Map Name set to {mapName}");
                    
                    Map newMap = new Map(mapName, true);
                    
                    Console.WriteLine($"Loaded {newMap.MapObjects.Count} objects for {newMap.MapName}.");
                    
                    File.WriteAllText($"{Environment.CurrentDirectory}/{mapName}.json", JsonConvert.SerializeObject(newMap, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        Formatting = Formatting.Indented
                    }));
                    
                    Console.WriteLine($"File Created! Conversion Finished!");
                    Console.WriteLine($"{Environment.CurrentDirectory}/{mapName}.json");
                    convertStage = 0;
                    mapName = "";
                    continue;
                    
                }
                if (userInput == "close")
                {
                    Console.WriteLine($"Closing.. Good bye!");
                    
                    Timer closeTimer = new Timer(2000)
                    {
                        AutoReset = false
                    };
                    
                    closeTimer.Start();

                    closeTimer.Elapsed += (sender, eventArgs) =>
                    {
                        closeTimer.Stop();
                        readyToExit = true;
                    };
                }
            }
        }
    }
}