using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Groups.Police.MDT;
using Server.Inventory;
using Server.Models;
using Server.Voice;

namespace Server.Groups.Police
{
    public class PoliceHandler
    {
        public static Position ArrestPosition = new Position(459.7846f, -989.11646f, 24.898926f);
        public static Position JailLocation = new Position(1586.135f, 2557.978f, 45.5649f);
        public static Position UnJailPosition = new Position(428.822f, -981.9429f, 30.69519f);
        public static Position ImpoundPosition = new Position(408.3956f, -1638.3561f, 29.279907f);

        public static Dictionary<Position, int> JailCells = new Dictionary<Position, int>
        {
            {new Position(458.76923f, -1001.45935f, 24.898926f), 0 },
            {new Position(459.12527f, -997.75385f, 24.898926f), 0 },
            {new Position(459.77142f, -994.2857f, 24.898926f), 0 },
            {new Position(467.81537f, -994.04834f, 24.898926f), 0 },
            {new Position(472.1011f, -994.6022f, 24.898926f), 0 },
            {new Position(476.96704f, -994.5626f, 24.898926f), 0  },
            {new Position(480.65933f, -994.7209f, 24.898926f), 0  }
        };

        /// <summary>
        /// Removes the players police items (Weapons etc)
        /// </summary>
        /// <param name="player"></param>
        /// <returns>True if success</returns>
        public static bool RemovePoliceItems(IPlayer player)
        {
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> fireWeapons = playerInventory.GetInventory().Where(x => x.Id.Contains("ITEM_POLICE_WEAPON")).ToList();

            lock (fireWeapons)
            {
                foreach (InventoryItem inventoryItem in fireWeapons)
                {
                    bool success = playerInventory.RemoveItem(inventoryItem);

                    if (!success)
                    {
                        player.SendErrorNotification("An error occurred removing your police issued weapons.");
                        return false;
                    }
                }
            }

            using Context context = new Context();

            var playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (!string.IsNullOrEmpty(playerCharacter.CurrentWeapon))
            {
                var currentWeapon = JsonConvert.DeserializeObject<InventoryItem>(playerCharacter.CurrentWeapon);

                if (currentWeapon.Id.Contains("ITEM_POLICE_WEAPON"))
                {
                    playerCharacter.CurrentWeapon = string.Empty;
                    player.RemoveAllWeapons();
                    player.SetData("CurrentWeaponHash", 0);
                    player.DeleteData("CurrentWeaponHash");
                    context.SaveChanges();
                }
            }

            

            return true;
        }

        /// <summary>
        /// Adds a set of equipment to a player on duty
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type">0 = Normal Duty, 1 = SWAT, 2 = Detective</param>
        /// <returns>True if Success</returns>
        public static bool AddPoliceItems(IPlayer player, int type)
        {
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> inventoryItems = new List<InventoryItem>();

            if (type == 0)
            {
                WeaponInfo generalInfo = new WeaponInfo(1, true, "LSPD");
                WeaponInfo pistolInfo = new WeaponInfo(100, true, "LSPD");

                // Normal Duty
                inventoryItems.Add(new InventoryItem("ITEM_POLICE_WEAPON_STUNGUN", "Stungun", generalInfo.ToString()));
                inventoryItems.Add(new InventoryItem("ITEM_POLICE_WEAPON_PISTOL", "Pistol", pistolInfo.ToString()));
                inventoryItems.Add(new InventoryItem("ITEM_POLICE_WEAPON_NIGHTSTICK", "Nightstick", generalInfo.ToString()));
                inventoryItems.Add(new InventoryItem("ITEM_POLICE_WEAPON_FLASHLIGHT", "Flashlight", generalInfo.ToString()));

                bool success = playerInventory.AddItem(inventoryItems);

                if (!success) return false;
            }

            if (type == 1)
            {
                // Shotgun
                WeaponInfo shotgunInfo = new WeaponInfo(50, true, "LSPD");
                inventoryItems.Add(new InventoryItem("ITEM_POLICE_WEAPON_SHOTGUN", "Shotgun", shotgunInfo.ToString()));
                
                bool success = playerInventory.AddItem(inventoryItems);

                if (!success) return false;

            }

            if (type == 2)
            {
                // AR
                WeaponInfo arInfo = new WeaponInfo(200, true, "LSPD");
                inventoryItems.Add(new InventoryItem("ITEM_POLICE_WEAPON_AR", "Assault Rife", arInfo.ToString()));
                
                bool success = playerInventory.AddItem(inventoryItems);

                if (!success) return false;

            }



            return true;
        }
    }
}