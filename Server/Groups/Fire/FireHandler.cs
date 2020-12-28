using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Inventory;

namespace Server.Groups.Fire
{
    public class FireHandler
    {
        public static bool RemoveFireItems(IPlayer player)
        {
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> policeWeapons = playerInventory.GetInventory().Where(x => x.Id.Contains("ITEM_FIRE_WEAPON")).ToList();

            lock (policeWeapons)
            {
                foreach (InventoryItem inventoryItem in policeWeapons)
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

                if (currentWeapon.Id.Contains("ITEM_FIRE_WEAPON"))
                {
                    playerCharacter.CurrentWeapon = string.Empty;
                    player.SetData("CurrentWeaponHash", 0);
                    player.DeleteData("CurrentWeaponHash");
                    player.RemoveAllWeapons();
                    context.SaveChanges();
                }
            }

            

            return true;
        }
    }
}