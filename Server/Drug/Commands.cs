using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AltV.Net.Elements.Entities;
using EnumsNET;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Drug
{
    public class Commands
    {
        [Command("drugs", commandType: CommandType.Character, description: "Used to interact with the drugs system")]
        public static void DrugsCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            List<InventoryItem> drugItems = playerInventory.GetInventory().Where(x => x.Id.Contains("DRUG")).ToList();

            if (!drugItems.Any())
            {
                player.SendErrorNotification("You have no drugs on you!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem drugItem in drugItems)
            {
                NativeMenuItem menuItem = new NativeMenuItem(drugItem.ItemInfo.Name, $"Quantity: {drugItem.Quantity}");
                menuItems.Add(menuItem);
            }

            NativeMenu menu = new NativeMenu("DrugSystem:DrugsMainMenuSelect", "Drugs", "Select an item", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnDrugMainMenuSelect(IPlayer player, string selectedItem, int selectedIndex)
        {
            if (selectedItem == "Close") return;

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            List<InventoryItem> drugItems = playerInventory.GetInventory().Where(x => x.Id.Contains("DRUG")).ToList();

            InventoryItem selectedDrugItem = drugItems[selectedIndex];

            if (selectedDrugItem == null)
            {
                player.SendErrorNotification("Unable to find this item!");
                return;
            }

            player.SetData("DrugSystem:SelectedDrug", selectedIndex);

            if (selectedDrugItem.Id.Contains("BAG"))
            {
                // TODO: Process this
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Use Drug")
            };

            // Have a bag item?
            bool hasSmallBag = playerInventory.HasItem("ITEM_DRUG_ZIPLOCK_BAG_SMALL");
            bool hasLargeBag = playerInventory.HasItem("ITEM_DRUG_ZIPLOCK_BAG_LARGE");

            if (hasSmallBag || hasLargeBag)
            {
                NativeMenuItem addToBagItem = new NativeMenuItem("Add to Bag");
                menuItems.Add(addToBagItem);
            }

            NativeMenu menu = new NativeMenu("DrugSystem:SubMenuDrugSelected", "Drugs", "Select an item", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void SubMenuDrugSelected(IPlayer player, string selectedOption)
        {
            if (selectedOption == "Close") return;

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            List<InventoryItem> drugItems = playerInventory.GetInventory().Where(x => x.Id.Contains("DRUG")).ToList();

            bool hasData = player.GetData("DrugSystem:SelectedDrug", out int selectedIndex);

            if (!hasData)
            {
                player.SendErrorMessage("An error occurred");
                return;
            }

            InventoryItem selectedDrugItem = drugItems[selectedIndex];

            if (selectedOption == "Add to Bag")
            {
                // Add to a Ziplock Bag
                List<InventoryItem> bagItems = playerInventory.GetInventory().Where(x =>
                    x.Id == "ITEM_DRUG_ZIPLOCK_BAG_SMALL" || x.Id == "ITEM_DRUG_ZIPLOCK_BAG_LARGE").ToList();

                List<NativeMenuItem> bagMenuItems = new List<NativeMenuItem>();

                DrugType drugType = DrugType.Empty;

                switch (selectedDrugItem.Id)
                {
                    case "ITEM_DRUG_MUSHROOM":
                        drugType = DrugType.Mushroom;
                        break;

                    case "ITEM_DRUG_WEED":
                        drugType = DrugType.Weed;
                        break;

                    case "ITEM_DRUG_HEROIN":
                        drugType = DrugType.Heroin;
                        break;

                    case "ITEM_DRUG_METH":
                        drugType = DrugType.Meth;
                        break;

                    case "ITEM_DRUG_COCAINE":
                        drugType = DrugType.Cocaine;
                        break;

                    case "ITEM_DRUG_ECSTASY":
                        drugType = DrugType.Ecstasy;
                        break;
                }

                foreach (InventoryItem bagItem in bagItems)
                {
                    DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(bagItem.ItemValue);

                    if (drugBag.DrugType != DrugType.Empty && drugType != drugBag.DrugType) continue;

                    string? drugName = drugBag.DrugType.AsString(EnumFormat.Description);

                    bagMenuItems.Add(new NativeMenuItem(bagItem.GetName(false), $"{drugBag.DrugQuantity} of {drugName}"));
                }

                if (!bagMenuItems.Any())
                {
                    player.SendErrorNotification("You don't have any bags on you!");
                    return;
                }

                NativeMenu menu = new NativeMenu("DrugSystem:CombineDrugWithBag", "Combine", "Select a bag to combine the drug with", bagMenuItems)
                {
                    PassIndex = true
                };

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }

            if (selectedOption == "Use Drug")
            {
                bool removeItem = playerInventory.RemoveItem(selectedDrugItem, 0.1);
                if (!removeItem)
                {
                    player.SendErrorMessage("Unable to use this drug from your Inventory!");
                    return;
                }

                switch (selectedDrugItem.Id)
                {
                    case "ITEM_DRUG_MUSHROOM":
                        UseDrug.UseMushroomItem(player);
                        return;

                    case "ITEM_DRUG_WEED":
                        UseDrug.UseWeedItem(player);
                        return;

                    case "ITEM_DRUG_HEROIN":
                        UseDrug.UseHeroinItem(player);
                        return;

                    case "ITEM_DRUG_METH":
                        UseDrug.UseMethItem(player);
                        return;

                    case "ITEM_DRUG_COCAINE":
                        UseDrug.UseCocaineItem(player);
                        return;

                    case "ITEM_DRUG_ECSTASY":
                        UseDrug.UseEcstasyItem(player);
                        return;
                }
            }
        }

        public static void OnCombineDrugWithDrugBag(IPlayer player, string selectedOption, int selectedIndex)
        {
            if (selectedOption == "Close") return;

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            List<InventoryItem> drugItems = playerInventory.GetInventory().Where(x => x.Id.Contains("DRUG")).ToList();

            bool hasData = player.GetData("DrugSystem:SelectedDrug", out int itemIndex);

            if (!hasData)
            {
                player.SendErrorMessage("An error occurred");
                return;
            }

            InventoryItem selectedDrugItem = drugItems[itemIndex];

            List<InventoryItem> bagItems = playerInventory.GetInventory().Where(x =>
                x.Id == "ITEM_DRUG_ZIPLOCK_BAG_SMALL" || x.Id == "ITEM_DRUG_ZIPLOCK_BAG_LARGE").ToList();

            DrugType drugType = selectedDrugItem.Id switch
            {
                "ITEM_DRUG_MUSHROOM" => DrugType.Mushroom,
                "ITEM_DRUG_WEED" => DrugType.Weed,
                "ITEM_DRUG_HEROIN" => DrugType.Heroin,
                "ITEM_DRUG_METH" => DrugType.Meth,
                "ITEM_DRUG_COCAINE" => DrugType.Cocaine,
                "ITEM_DRUG_ECSTASY" => DrugType.Ecstasy,
                _ => DrugType.Empty
            };

            List<InventoryItem> updatedBagItems = new List<InventoryItem>();

            foreach (InventoryItem bagItem in bagItems)
            {
                DrugBag drugBagItem = JsonConvert.DeserializeObject<DrugBag>(bagItem.ItemValue);

                if (drugBagItem.DrugType != DrugType.Empty && drugType != drugBagItem.DrugType) continue;

                updatedBagItems.Add(bagItem);
            }

            InventoryItem selectedBagItem = updatedBagItems[selectedIndex];

            if (selectedBagItem == null)
            {
                player.SendErrorNotification("Unable to find this item.");
                return;
            }

            player.SetData("DrugSystem:SelectedBag", selectedIndex);

            DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(selectedBagItem.ItemValue);

            DrugBagType drugBagType = selectedBagItem.Id switch
            {
                "ITEM_DRUG_ZIPLOCK_BAG_SMALL" => DrugBagType.ZipLockSmall,
                "ITEM_DRUG_ZIPLOCK_BAG_LARGE" => DrugBagType.ZipLockLarge,
                _ => DrugBagType.ZipLockSmall,
            };

            double maxWeight = drugBagType switch
            {
                DrugBagType.ZipLockSmall => DrugBag.SmallBagLimit,
                DrugBagType.ZipLockLarge => DrugBag.LargeBagLimit
            };

            if (drugBag.DrugQuantity >= maxWeight)
            {
                player.SendErrorMessage("This bag is full!");
                return;
            }

            List<NativeListItem> listItems = new List<NativeListItem>();

            List<string> values = new List<string>();

            // Quantity
            for (double i = 0.0; i < drugBag.DrugQuantity; i += 0.1)
            {
                values.Add($"{i:#}");
            }

            listItems.Add(new NativeListItem("Quantity", values));

            NativeMenu menu = new NativeMenu("DrugSystem:SelectedCombineDrugToBagQuantity", "Combine", "Select a Quantity to combine")
            {
                ListMenuItems = listItems,
                ListTrigger = "DrugSystem:OnCombineDrugToBagListChange"
            };

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("DrugSystem:CombineDrugToBagQuantity", 0.01);
        }

        public static void OnSelectedCombineDrugToBag(IPlayer player, string selectedOption)
        {
            if (selectedOption == "Close") return;

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            List<InventoryItem> drugItems = playerInventory.GetInventory().Where(x => x.Id.Contains("DRUG")).ToList();

            bool hasData = player.GetData("DrugSystem:SelectedDrug", out int itemIndex);

            if (!hasData)
            {
                player.SendErrorMessage("An error occurred");
                return;
            }

            InventoryItem selectedDrugItem = drugItems[itemIndex];

            List<InventoryItem> bagItems = playerInventory.GetInventory().Where(x =>
                x.Id == "ITEM_DRUG_ZIPLOCK_BAG_SMALL" || x.Id == "ITEM_DRUG_ZIPLOCK_BAG_LARGE").ToList();

            DrugType drugType = selectedDrugItem.Id switch
            {
                "ITEM_DRUG_MUSHROOM" => DrugType.Mushroom,
                "ITEM_DRUG_WEED" => DrugType.Weed,
                "ITEM_DRUG_HEROIN" => DrugType.Heroin,
                "ITEM_DRUG_METH" => DrugType.Meth,
                "ITEM_DRUG_COCAINE" => DrugType.Cocaine,
                "ITEM_DRUG_ECSTASY" => DrugType.Ecstasy,
                _ => DrugType.Empty
            };

            List<InventoryItem> updatedBagItems = new List<InventoryItem>();

            foreach (InventoryItem bagItem in bagItems)
            {
                DrugBag drugBagItem = JsonConvert.DeserializeObject<DrugBag>(bagItem.ItemValue);

                if (drugBagItem.DrugType != DrugType.Empty && drugType != drugBagItem.DrugType) continue;

                updatedBagItems.Add(bagItem);
            }

            player.GetData("DrugSystem:SelectedBag", out int bagIndex);

            InventoryItem selectedBagItem = updatedBagItems[bagIndex];

            DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(selectedBagItem.ItemValue);

            player.GetData("DrugSystem:CombineDrugToBagQuantity", out double selectedQuantity);

            if (selectedDrugItem.Quantity < selectedQuantity)
            {
                player.SendErrorNotification("Looks like you tried adding more than you have!");
                return;
            }

            bool tryRemove = playerInventory.RemoveItem(selectedDrugItem, selectedQuantity);

            if (!tryRemove)
            {
                player.SendErrorMessage("An error occurred removing this from your Inventory.");
                return;
            }

            playerInventory.RemoveItem(selectedBagItem);

            drugBag.DrugType = drugType;
            drugBag.DrugQuantity += selectedQuantity;

            selectedBagItem.ItemValue = JsonConvert.SerializeObject(drugBag);

            playerInventory.AddItem(selectedBagItem);

            player.SendInfoMessage($"You've added {selectedQuantity} of {drugType.AsString(EnumFormat.Description)} to the bag!");
        }

        public static void OnCombineDrugToBagQuantityChange(IPlayer player, string menuText, string listText)
        {
            if (menuText == "Close") return;

            bool tryParse = Double.TryParse(listText, out double newQuantity);

            if (!tryParse) return;

            player.SetData("DrugSystem:CombineDrugToBagQuantity", newQuantity);
        }

        [Command("plantseed", commandType: CommandType.Character, description: "Used to plant a Marijuana Seed")]
        public static void DrugCommandPlantSeed(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Inventory.Inventory? playerInventory = player.FetchInventory();

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