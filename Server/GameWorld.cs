using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Server.Inventory;

namespace Server
{
    public class GameWorld
    {
        public static List<GameItem> GameItems;

        private const string itemsFile = "Data/gameItems.json";

        public static void InitGameWorld()
        {
            try
            {
                CreateGameItems();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void CreateGameItems()
        {
            GameItems = new List<GameItem>();

            try
            {
                if (!File.Exists(itemsFile))
                {
                    GameItems.Add(new GameItem("ITEM_EXAMPLE_ID", "Example item name", "Item description", 1, 2, 3, false));
                    GameItems.Add(new GameItem("ITEM_SECOND_EXAMPLE", "Example item name", "Item description", 1, 2, 3, false));
                    File.WriteAllText(itemsFile, JsonConvert.SerializeObject(GameItems));
                }
                else
                {
                    string content = File.ReadAllText(itemsFile);
                    GameItems = new List<GameItem>(JsonConvert.DeserializeObject<List<GameItem>>(content));
                    Console.WriteLine(GameItems.Count + " Game items loaded.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Couldn't load items file.");
                Console.WriteLine(ex.ToString());
                return;
            }
        }

        public static GameItem GetGameItem(string id)
        {
            return GameItems.FirstOrDefault(i => i.ID == id);
        }
    }
}