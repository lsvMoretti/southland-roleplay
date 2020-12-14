using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Drug
{
    public class Commands
    {
        [Command("plantseed", commandType: CommandType.Character, description: "Used to plant a Marijuana Seed")]
        public static void DrugCommandPlantSeed(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                NotificationExtension.SendErrorNotification(player, "An error occurred fetching your inventory.");
                return;
            }

            bool hasMarijuanaSeed = playerInventory.HasItem("ITEM_MARIJUANASEED");

            if (!hasMarijuanaSeed)
            {
                player.SendErrorNotification("You don't have any seeds!");
                return;
            }
            
            Marijuana nearestMarijuana = Marijuana.FetchNearest(player.Position, player.Dimension, 1);

            if (nearestMarijuana != null)
            {
                player.SendErrorNotification("Your too near a plant.");
                return;
            }

            bool seedRemoved = playerInventory.RemoveItem("ITEM_MARIJUANASEED");

            if (!seedRemoved)
            {
                player.SendErrorNotification("An error occurred removing the seed from your inventory.");
                return;
            }

            Marijuana.PlantMarijuana(player.Position, player.Dimension);
        }

        [Command("harvestweed", commandType: CommandType.Character,
            description: "Used to harvest your beautiful seeds")]
        public static void DrugCommandHarvestWeed(IPlayer player)
        {
            
            if (!player.IsSpawned()) return;

            Marijuana nearestMarijuana = Marijuana.FetchNearest(player.Position, player.Dimension, 1);

            if (nearestMarijuana == null)
            {
                player.SendErrorNotification("Your too near a plant.");
                return;
            }

            if (nearestMarijuana.Status == MarijuanaStatus.Harvest)
            {

                Inventory.Inventory playerInventory = player.FetchInventory();

                if (playerInventory == null)
                {
                    player.SendErrorNotification("Couldn't fetch your inventory.");
                    return;
                }
                
                InventoryItem weedItem = new InventoryItem("ITEM_DRUG_WEED", "Marijuana", "GROWN", 112);

                bool itemAdded = playerInventory.AddItem(weedItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("Couldn't add the weed to your inventory!");
                    return;
                }
                
                Marijuana.RemoveMarijuana(nearestMarijuana);
                
                player.SendInfoNotification($"You've harvested the Marijuana.");

                return;
            }

            if (nearestMarijuana.Status == MarijuanaStatus.Withered)
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                if (playerInventory == null)
                {
                    player.SendErrorNotification("Couldn't fetch your inventory.");
                    return;
                }
                
                InventoryItem weedItem = new InventoryItem("ITEM_DRUG_WEED", "Marijuana", "GROWN", 1);

                bool itemAdded = playerInventory.AddItem(weedItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("Couldn't add the weed to your inventory!");
                    return;
                }
                
                Marijuana.RemoveMarijuana(nearestMarijuana);
                
                player.SendInfoNotification($"You've harvested the withered Marijuana.");

                return;
            }

            player.SendInfoNotification($"This plant isn't ready to be harvested.");
        }
    }
}