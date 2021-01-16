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
                if (drugItem.Id == "ITEM_DRUG_ZIPLOCK_BAG_SMALL" || drugItem.Id == "ITEM_DRUG_ZIPLOCK_BAG_LARGE")
                {
                    DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(drugItem.ItemValue);
                    DrugBagType drugBagType = drugItem.Id switch
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

                    NativeMenuItem menuItem = new NativeMenuItem(drugItem.ItemInfo.Name, $"{drugBag.DrugQuantity:0.0}/{maxWeight:0.0} of {drugBag.DrugType.AsString(EnumFormat.Description)}");
                    menuItems.Add(menuItem);
                }
                else
                {
                    NativeMenuItem menuItem = new NativeMenuItem(drugItem.ItemInfo.Name, $"Quantity: {drugItem.Quantity:0.0}");
                    menuItems.Add(menuItem);
                }
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
                OnSelectItemBagMainMenu(player, selectedDrugItem);
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

        public static void OnSelectItemBagMainMenu(IPlayer player, InventoryItem item)
        {
            DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(item.ItemValue);

            if (drugBag.DrugType == DrugType.Empty)
            {
                player.SendInfoNotification("This bag is empty.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (drugBag.DrugQuantity > 0)
            {
                menuItems.Add(new NativeMenuItem("Use Drug", "Uses 0.1 of this drug"));
                menuItems.Add(new NativeMenuItem("Take Out of Bag", "Takes a quantity from the bag"));
            }
            else
            {
                player.SendInfoNotification("This bag is empty.");
                return;
            }

            NativeMenu menu = new NativeMenu("DrugSystem:InteractWithBag", "Bag", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnInteractWithDrugBag(IPlayer player, string selectedOption)
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

            DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(selectedDrugItem.ItemValue);

            if (selectedOption == "Take Out of Bag")
            {
                List<string> values = new List<string>();

                // Quantity
                for (double i = 0.1; i <= Math.Round(drugBag.DrugQuantity, 2); i += 0.1)
                {
                    values.Add($"{Math.Round(i, 2)}");
                }

                List<NativeListItem> listItems = new List<NativeListItem>
                {
                    new NativeListItem("Quantity: ", values)
                };

                NativeMenu menu = new NativeMenu("DrugSystem:RemoveDrugQuantityFromBag", "Bag", "Select a quantity to remove")
                {
                    ListTrigger = "DrugSystem:OnRemoveDrugFromBagQuantityChange",
                    ListMenuItems = listItems
                };

                NativeUi.ShowNativeMenu(player, menu, true);

                player.SetData("DrugSystem:RemoveQuantityFromBag", 0.1);
                return;
            }

            drugBag.DrugQuantity -= 1;
            if (drugBag.DrugQuantity == 0)
            {
                drugBag.DrugType = DrugType.Empty;
            }

            playerInventory.RemoveItem(selectedDrugItem);

            selectedDrugItem.ItemValue = JsonConvert.SerializeObject(drugBag);

            playerInventory.AddItem(selectedDrugItem);

            switch (drugBag.DrugType)
            {
                case DrugType.Mushroom:
                    UseDrug.UseMushroomItem(player);
                    break;

                case DrugType.Weed:
                    UseDrug.UseWeedItem(player);
                    break;

                case DrugType.Heroin:
                    UseDrug.UseHeroinItem(player);
                    break;

                case DrugType.Meth:
                    UseDrug.UseMethItem(player);
                    break;

                case DrugType.Cocaine:
                    UseDrug.UseCocaineItem(player);
                    break;

                case DrugType.Ecstasy:
                    UseDrug.UseEcstasyItem(player);
                    break;
            }

            player.SendEmoteMessage($"reaches into a Zip-Lock bag and uses some {drugBag.DrugType.AsString(EnumFormat.Description)}");
        }

        public static void OnRemoveDrugFromBagQuantitySelect(IPlayer player, string selectedOption)
        {
            if (selectedOption == "Close") return;

            player.GetData("DrugSystem:RemoveQuantityFromBag", out double quantity);

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

            DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(selectedDrugItem.ItemValue);

            quantity = Math.Round(quantity, 2);

            Console.WriteLine($"Quantity: {quantity}");

            if (quantity > drugBag.DrugQuantity)
            {
                player.SendErrorMessage("You don't have this many drugs in your bag!");
                return;
            }

            string newDrugItemId = "";

            Console.WriteLine($"Drug Type: {drugBag.DrugType}");

            switch (drugBag.DrugType)
            {
                case DrugType.Empty:
                    break;

                case DrugType.Mushroom:
                    newDrugItemId = "ITEM_DRUG_MUSHROOM";
                    break;

                case DrugType.Weed:
                    newDrugItemId = "ITEM_DRUG_WEED";
                    break;

                case DrugType.Heroin:
                    newDrugItemId = "ITEM_DRUG_HEROIN";
                    break;

                case DrugType.Meth:
                    newDrugItemId = "ITEM_DRUG_METH";
                    break;

                case DrugType.Cocaine:
                    newDrugItemId = "ITEM_DRUG_COCAINE";
                    break;

                case DrugType.Ecstasy:
                    newDrugItemId = "ITEM_DRUG_ECSTASY";
                    break;
            }

            drugBag.DrugQuantity = Math.Round(drugBag.DrugQuantity -= quantity, 2);

            if (drugBag.DrugQuantity <= 0)
            {
                drugBag.DrugType = DrugType.Empty;
            }

            Console.WriteLine(newDrugItemId);

            InventoryItem newDrugItem = new InventoryItem(newDrugItemId, GameWorld.GetGameItem(newDrugItemId).Name, "", quantity);

            if (!playerInventory.AddItem(newDrugItem))
            {
                player.SendErrorNotification("Unable to put this drug across. You got space?");
                return;
            }

            playerInventory.RemoveItem(selectedDrugItem);

            InventoryItem newDrugBag = new InventoryItem(selectedDrugItem.Id, selectedDrugItem.CustomName, JsonConvert.SerializeObject(drugBag));

            playerInventory.AddItem(newDrugBag);

            player.SendEmoteMessage($"reaches into a ziploc bag and takes out some {newDrugItem.GetName(false)}.");
        }

        public static void OnRemoveDrugFromBagQuantityChange(IPlayer player, string menuText, string listText)
        {
            if (menuText == "Close") return;

            bool tryParse = Double.TryParse(listText, out double newQuantity);

            if (!tryParse) return;

            player.SetData("DrugSystem:RemoveQuantityFromBag", newQuantity);
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

                    DrugBagType drugBagType = bagItem.Id switch
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

                    bagMenuItems.Add(new NativeMenuItem(bagItem.GetName(false), $"{drugBag.DrugQuantity:0.0} / {maxWeight:0.0} of {drugName}"));
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

            List<string> values = new List<string>();

            // Quantity
            for (double i = 0.1; i <= Math.Round(selectedDrugItem.Quantity, 2); i += 0.1)
            {
                values.Add($"{Math.Round(i, 2)}");
            }

            List<NativeListItem> listItems = new List<NativeListItem>
            {
                new NativeListItem("Quantity: ", values)
            };

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

            selectedQuantity = Math.Round(selectedQuantity, 2);
            selectedDrugItem.Quantity = Math.Round(selectedDrugItem.Quantity, 2);

            Console.WriteLine($"SDI Q: {selectedDrugItem.Quantity}, Quantity: {selectedQuantity}");

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

            InventoryItem newBagItem = new InventoryItem(selectedBagItem.Id, selectedBagItem.CustomName, JsonConvert.SerializeObject(drugBag));

            playerInventory.AddItem(newBagItem);

            player.SendInfoMessage($"You've added {selectedQuantity} of {drugType.AsString(EnumFormat.Description)} to the bag!");

            player.SendEmoteMessage($"reaches into the Ziploc bag and places some {selectedDrugItem.GetName(false)} into it.");
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