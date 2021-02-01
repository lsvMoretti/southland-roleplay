using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using EnumsNET;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Server.Backpack;
using Server.Chat;
using Server.Commands;
using Server.Drug;
using Server.Extensions;
using Server.Models;
using Server.Weapons;

namespace Server.Inventory
{
    public class InventoryCommands
    {
        /// <summary>
        /// Inventory Command
        /// </summary>
        /// <param name="player"></param>
        [Command("inv", commandType: CommandType.Character, description: "Shows you the Inventory of your character")]
        public static void InventoryCommand(IPlayer player)
        {
            try
            {
                ShowInventoryToPlayer(player, player.FetchInventory(), true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void ShowInventoryToPlayer(IPlayer player, Inventory inventory, bool isOwner = false, int inventoryId = 0)
        {
            try
            {
                Models.Character playerCharacter = player.FetchCharacter();

                if (playerCharacter == null)
                {
                    player.SendLoginError();
                    return;
                }

                if (isOwner)
                {
                    player.SetData("IsInventoryOwner", true);
                }
                else
                {
                    if (inventoryId != 0)
                    {
                        player.SetData("ShowingInventoryId", inventoryId);
                    }
                    player.SetData("IsInventoryOwner", false);
                }

                List<InventoryItem> inventoryItems = inventory.GetInventory().OrderBy(x => x.CustomName).ToList();

                bool containsClothing = inventory
                    .GetInventory().Any(x => x.Id == "ITEM_CLOTHES" || x.Id == "ITEM_CLOTHES_ACCESSORY");

                bool containsVehicleKeys = inventory
                    .GetInventory().Any(x => x.Id == "ITEM_VEHICLE_KEY");
                bool containsPropertyKeys = inventory
                    .GetInventory().Any(x => x.Id == "ITEM_PROPERTY_KEY");
                bool containsApartmentKeys = inventory
                    .GetInventory().Any(x => x.Id == "ITEM_APARTMENT_KEY");

                foreach (InventoryItem inventoryItem in inventoryItems)
                {
                    if (inventoryItem.CustomName == "undefined" || inventoryItem.CustomName == "")
                    {
                        inventory.RemoveItem(inventoryItem);
                    }
                }

                List<NativeMenuItem> itemList = new List<NativeMenuItem>();

                NativeMenu inventoryMenu = new NativeMenu("InventoryMenuSelect", "Inventory", $"Your Inventory {Math.Round(inventory.CurrentWeight, 2)}/{inventory.MaximumWeight}");

                if (containsClothing)
                {
                    itemList.Add(new NativeMenuItem("Clothing"));
                }

                if (containsVehicleKeys)
                {
                    itemList.Add(new NativeMenuItem("Vehicle Keys"));
                }

                if (containsPropertyKeys)
                {
                    itemList.Add(new NativeMenuItem("Property Keys"));
                }

                if (containsApartmentKeys)
                {
                    itemList.Add(new NativeMenuItem("Apartment Keys"));
                }

                foreach (var item in inventoryItems)
                {
                    if (item.Id == "ITEM_CLOTHES" || item.Id == "ITEM_CLOTHES_ACCESSORY" || item.Id == "ITEM_VEHICLE_KEY" || item.Id == "ITEM_PROPERTY_KEY" || item.Id == "ITEM_APARTMENT_KEY") continue;

                    if (item.Id.Contains("WEAPON") && !item.Id.Contains("AMMO"))
                    {
                        WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(item.ItemValue);

                        if (weaponInfo != null)
                        {
                            if (weaponInfo.AmmoCount > 0)
                            {
                                NativeMenuItem menuItem = new NativeMenuItem(item.CustomName, $"{item.ItemInfo.Description} - Bullets: {weaponInfo.AmmoCount}");
                                itemList.Add(menuItem);
                                continue;
                            }
                        }
                    }

                    if (item.Id.Contains("AMMO"))
                    {
                        int.TryParse(item.ItemValue, out int ammoCount);
                        if (ammoCount > 1)
                        {
                            NativeMenuItem menuItem = new NativeMenuItem(item.CustomName, $"{item.ItemInfo.Description} - x{ammoCount}");
                            itemList.Add(menuItem);
                            continue;
                        }
                        else
                        {
                            NativeMenuItem menuItem = new NativeMenuItem(item.CustomName, $"{item.ItemInfo.Description}");
                            itemList.Add(menuItem);
                            continue;
                        }
                    }

                    if (item.Id == "ITEM_DRUG_ZIPLOCK_BAG_SMALL" || item.Id == "ITEM_DRUG_ZIPLOCK_BAG_LARGE")
                    {
                        DrugBag drugBag = JsonConvert.DeserializeObject<DrugBag>(item.ItemValue);
                        DrugBagType drugBagType = item.Id switch
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

                        NativeMenuItem menuItem = new NativeMenuItem(item.GetName(false), $"{drugBag.DrugQuantity:0.0}/{maxWeight:0.0} of {drugBag.DrugType.AsString(EnumFormat.Description)}");
                        itemList.Add(menuItem);
                        continue;
                    }

                    if (item.Id.Contains("DRUG"))
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(item.CustomName, $"{item.ItemInfo.Description} - x{item.Quantity:0.0}");
                        itemList.Add(menuItem);
                        continue;
                    }

                    if (item.Quantity > 1)
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(item.CustomName, $"{item.ItemInfo.Description} - x{item.Quantity}");
                        itemList.Add(menuItem);
                    }
                    else
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(item.CustomName, $"{item.ItemInfo.Description}");
                        itemList.Add(menuItem);
                    }
                }

                inventoryMenu.MenuItems = itemList;

                NativeUi.ShowNativeMenu(player, inventoryMenu, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Return from /inventory menu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="option"></param>
        public static void OnInventoryMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("IsInventoryOwner", out bool isOwner);

            Inventory inventory = null;

            if (isOwner)
            {
                inventory = player.FetchInventory();
            }
            else
            {
                player.FreezeInput(false);
                player.FreezePlayer(false);

                bool hasInventoryId = player.GetData("ShowingInventoryId", out int inventoryId);

                if (hasInventoryId)
                {
                    inventory = new Inventory(InventoryData.GetInventoryData(inventoryId));
                }

                if (inventory == null)
                {
                    player.SendErrorNotification("An error occurred fetching the inventory.");
                    return;
                }
            }

            if (option == "Clothing")
            {
                List<InventoryItem> inventoryItems = inventory.GetInventory().Where(x => x.Id == "ITEM_CLOTHES" || x.Id == "ITEM_CLOTHES_ACCESSORY").OrderBy(x => x.CustomName).ToList();

                List<NativeMenuItem> clothingItemList = new List<NativeMenuItem>();

                NativeMenu inventoryMenu = new NativeMenu("InventoryMenuSelect", "Inventory", "Your Inventory");

                foreach (var clothingItem in inventoryItems)
                {
                    if (clothingItem.Quantity > 1)
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description} - x{clothingItem.Quantity}");
                        clothingItemList.Add(menuItem);
                    }
                    else
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description}");
                        clothingItemList.Add(menuItem);
                    }
                }

                inventoryMenu.MenuItems = clothingItemList;

                NativeUi.ShowNativeMenu(player, inventoryMenu, true);

                return;
            }

            if (option == "Vehicle Keys")
            {
                List<InventoryItem> inventoryItems = inventory.GetInventory().Where(x => x.Id == "ITEM_VEHICLE_KEY").OrderBy(x => x.CustomName).ToList();

                List<NativeMenuItem> clothingItemList = new List<NativeMenuItem>();

                NativeMenu inventoryMenu = new NativeMenu("InventoryMenuSelect", "Inventory", "Your Inventory");

                foreach (var clothingItem in inventoryItems)
                {
                    if (clothingItem.Quantity > 1)
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description} - x{clothingItem.Quantity}");
                        clothingItemList.Add(menuItem);
                    }
                    else
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description}");
                        clothingItemList.Add(menuItem);
                    }
                }

                inventoryMenu.MenuItems = clothingItemList;

                NativeUi.ShowNativeMenu(player, inventoryMenu, true);

                return;
            }

            if (option == "Property Keys")
            {
                List<InventoryItem> inventoryItems = inventory.GetInventory().Where(x => x.Id == "ITEM_PROPERTY_KEY").OrderBy(x => x.CustomName).ToList();

                List<NativeMenuItem> clothingItemList = new List<NativeMenuItem>();

                NativeMenu inventoryMenu = new NativeMenu("InventoryMenuSelect", "Inventory", "Your Inventory");

                foreach (var clothingItem in inventoryItems)
                {
                    if (clothingItem.Quantity > 1)
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description} - x{clothingItem.Quantity}");
                        clothingItemList.Add(menuItem);
                    }
                    else
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description}");
                        clothingItemList.Add(menuItem);
                    }
                }

                inventoryMenu.MenuItems = clothingItemList;

                NativeUi.ShowNativeMenu(player, inventoryMenu, true);

                return;
            }

            if (option == "Apartment Keys")
            {
                List<InventoryItem> inventoryItems = inventory.GetInventory().Where(x => x.Id == "ITEM_APARTMENT_KEY").OrderBy(x => x.CustomName).ToList();

                List<NativeMenuItem> clothingItemList = new List<NativeMenuItem>();

                NativeMenu inventoryMenu = new NativeMenu("InventoryMenuSelect", "Inventory", "Your Inventory");

                foreach (var clothingItem in inventoryItems)
                {
                    if (clothingItem.Quantity > 1)
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description} - x{clothingItem.Quantity}");
                        clothingItemList.Add(menuItem);
                    }
                    else
                    {
                        NativeMenuItem menuItem = new NativeMenuItem(clothingItem.CustomName, $"{clothingItem.ItemInfo.Description}");
                        clothingItemList.Add(menuItem);
                    }
                }

                inventoryMenu.MenuItems = clothingItemList;

                NativeUi.ShowNativeMenu(player, inventoryMenu, true);

                return;
            }

            if (!isOwner) return;

            InventoryItem item = inventory.GetInventory().FirstOrDefault(i => i.CustomName == option);

            if (item == null)
            {
                player.SendErrorNotification("An error has occured. #ER02");
                return;
            }

            if (item.Id.Contains("ITEM_POLICE_WEAPON"))
            {
                NativeMenu policeWeaponMenu = new NativeMenu("InventoryMenuSubSelect", "Inventory", "Choose your option");

                List<NativeMenuItem> policeItemList = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Delete Item"),
                };

                if (item.Id.Contains("WEAPON"))
                {
                    policeItemList.Add(new NativeMenuItem("Equip Item"));
                }

                policeWeaponMenu.MenuItems = policeItemList;

                player.SetData("SELECTEDINVITEM", inventory.GetInventory().IndexOf(item));

                NativeUi.ShowNativeMenu(player, policeWeaponMenu, true);
                return;
            }

            NativeMenu itemMenu = new NativeMenu("InventoryMenuSubSelect", "Inventory", "Choose your option");

            List<NativeMenuItem> itemList = new List<NativeMenuItem>
            {
                new NativeMenuItem("Use Item"),
                new NativeMenuItem("Drop Item"),
                new NativeMenuItem("Delete Item"),
                new NativeMenuItem("Give to Player")
            };

            if (player.FetchBackpackInventory() != null)
            {
                itemList.Add(new NativeMenuItem("Transfer to backpack"));
            }

            if (item.Id == "ITEM_BACKPACK" || item.Id == "ITEM_DUFFELBAG")
            {
                itemList = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Drop Item")
                };
            }

            if (item.Id.Contains("WEAPON"))
            {
                itemList.Add(new NativeMenuItem("Equip Item"));
            }

            if (item.Id.Contains("BEER") && !item.Id.Contains("CASE"))
            {
                // Breakable Bottle
                //itemList.Add(new NativeMenuItem("Break Bottle"));
            }

            itemMenu.MenuItems = itemList;

            int index = inventory.GetInventory().IndexOf(item);

            player.SetData("SELECTEDINVITEM", index);

            NativeUi.ShowNativeMenu(player, itemMenu, true);
        }

        /// <summary>
        /// Return from the selected item from the inventory menu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="option"></param>
        public static void OnInventoryMenuSubSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("SELECTEDINVITEM", out int itemIndex);

            if (itemIndex == -1)
            {
                player.SendErrorNotification("An error has occurred. #ER03");
                return;
            }

            Inventory inventory = player.FetchInventory();

            InventoryItem item = inventory.GetInventory()[itemIndex];

            if (item == null)
            {
                player.SendErrorNotification("An error has occured. #ER04");
                player.SetData("SELECTEDINVITEM", -1);
                return;
            }

            if (option == "Drop Item")
            {
                if (item.Id == "ITEM_BACKPACK" || item.Id == "ITEM_DUFFELBAG")
                {
                    BackpackHandler.DropBackpack(player, item);
                    return;
                }

                if (item.Quantity > 1 || item.Id.Contains("AMMO"))
                {
                    List<NativeListItem> menuItems = new List<NativeListItem>();

                    List<string> stringList = new List<string>();

                    if (item.Id.Contains("AMMO"))
                    {
                        bool ammoParse = int.TryParse(item.ItemValue, out int ammoCount);

                        if (!ammoParse)
                        {
                            player.SendErrorNotification("An error occurred parsing your ammo.");
                            return;
                        }

                        for (int i = 1; i <= ammoCount; i++)
                        {
                            stringList.Add(i.ToString());
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= item.Quantity; i++)
                        {
                            stringList.Add(i.ToString());
                        }
                    }

                    menuItems.Add(new NativeListItem("Quantity", stringList));

                    NativeMenu menu = new NativeMenu("inventory:DropItem:SelectQuantity", "Inventory", "Select a Quantity to Drop")
                    {
                        ListMenuItems = menuItems,
                        ListTrigger = "inventory:DropItemQuantityChange"
                    };

                    player.SetData("DROPITEMQUANTITY", 1);

                    NativeUi.ShowNativeMenu(player, menu, true);
                    return;
                }

                bool success = inventory.RemoveItem(item, item.Quantity);
                if (success)
                {
                    Console.WriteLine($"Item Quantity: {item.Quantity}");
                    item.Quantity += 1;
                    player.SetData("SELECTEDINVITEM", -1);
                    DroppedItems.CreateDroppedItem(item, player.Position.Around(0.5f));
                    player.SendInfoNotification($"You've dropped {item.CustomName} from your inventory.");
                    return;
                }

                player.SendErrorNotification("An error occurred dropping this item. #ER05");
                player.SetData("SELECTEDINVITEM", -1);
                return;
            }

            if (option == "Delete Item")
            {
                if (item.Quantity > 1 || item.Id.Contains("AMMO"))
                {
                    List<NativeListItem> menuItems = new List<NativeListItem>();

                    List<string> stringList = new List<string>();

                    if (item.Id.Contains("AMMO"))
                    {
                        bool ammoParse = int.TryParse(item.ItemValue, out int ammoCount);

                        if (!ammoParse)
                        {
                            player.SendErrorNotification("An error occurred parsing your ammo.");
                            return;
                        }

                        for (int i = 1; i <= ammoCount; i++)
                        {
                            stringList.Add(i.ToString());
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= item.Quantity; i++)
                        {
                            stringList.Add(i.ToString());
                        }
                    }

                    player.SetData("DELETEITEMQUANTITY", 1);

                    menuItems.Add(new NativeListItem("Quantity", stringList));

                    NativeMenu menu = new NativeMenu("inventory:DeleteItem:SelectQuantity", "Inventory", "Select a Quantity to Drop")
                    {
                        ListMenuItems = menuItems,
                        ListTrigger = "inventory:DeleteItemQuantityChange"
                    };

                    NativeUi.ShowNativeMenu(player, menu, true);
                    return;
                }

                bool success = inventory.RemoveItem(item);
                if (success)
                {
                    player.SendInfoNotification($"You've deleted {item.CustomName} from your inventory.");
                    player.DeleteData("SELECTEDINVITEM");
                    return;
                }

                player.SendErrorNotification("An error occurred dropping this item. #ER05");
                player.DeleteData("SELECTEDINVITEM");
                return;
            }

            if (option == "Use Item")
            {
                UseItem.UseItemAttribute(player, item);
                return;
            }

            if (option == "Equip Item")
            {
                EquipItem.EquipItemAttribute(player, item);
                return;
            }

            if (option == "Give to Player")
            {
                List<IPlayer> targetList = new List<IPlayer>();

                List<IPlayer> playerList = Alt.GetAllPlayers().ToList();

                Position playerPosition = player.Position;

                foreach (IPlayer target in playerList)
                {
                    lock (target)
                    {
                        if (!target.IsSpawned()) continue;

                        if (target.Dimension != player.Dimension) continue;

                        Position targetPosition = target.Position;

                        if (playerPosition.Distance(targetPosition) > 4) continue;

                        targetList.Add(target);
                    }
                }

                if (!targetList.Any())
                {
                    player.SendErrorNotification("No players found.");
                    return;
                }

                NativeMenu targetPlayerMenu = new NativeMenu("InventoryGiveItemToPlayer", "Inventory", "Select a Player");

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                foreach (IPlayer client in targetList.Where(s => s.Name != player.Name).ToList())
                {
                    menuItems.Add(new NativeMenuItem(client.GetClass().Name));
                }

                targetPlayerMenu.MenuItems = menuItems;

                NativeUi.ShowNativeMenu(player, targetPlayerMenu, true);
            }

            if (option == "Transfer to backpack")
            {
                Inventory backpackInventory = player.FetchBackpackInventory();

                if (backpackInventory == null)
                {
                    player.SendErrorNotification("An error occurred fetching your backpack data.");
                    return;
                }

                if (item.Quantity > 1)
                {
                    List<NativeListItem> backpackQuantityItems = new List<NativeListItem>
                    {
                        new NativeListItem("Inventory", item.QuantityListString)
                    };

                    NativeMenu backPackMenu = new NativeMenu("Inventory:Backpack:QuantitySelect", "Inventory", "Select a quantity")
                    {
                        ListTrigger = "Inventory:Backpack:QuantityChange",
                        ListMenuItems = backpackQuantityItems
                    };

                    player.SetData("Inventory:Backpack:Quantity", 1);

                    NativeUi.ShowNativeMenu(player, backPackMenu, true);
                    return;
                }

                bool transferItem = inventory.TransferItem(backpackInventory, item, item.Quantity);

                if (!transferItem)
                {
                    player.SendErrorNotification("An error occurred moving the item!");
                    return;
                }

                player.SendNotification($"~y~You've stored {item.CustomName} in your backpack.");
            }

            if (option == "Break Bottle")
            {
                bool itemRemoved = inventory.RemoveItem(item);

                if (!itemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the item from your inventory.");
                    return;
                }

                InventoryItem brokenBottleItem = new InventoryItem("ITEM_WEAPON_MELEE_BOTTLE", "Broken Bottle", JsonConvert.SerializeObject(new WeaponInfo(1, false, player.GetClass().Name)));

                bool itemAdded = inventory.AddItem(brokenBottleItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding the item to your inventory.");
                    return;
                }

                player.SendEmoteMessage($"smashes a bottle, breaking it.");
            }
        }

        public static void OnBackpackQuantityChange(IPlayer player, string listText)
        {
            bool tryParse = int.TryParse(listText, out int quantity);
            if (!tryParse) return;

            player.SetData("Inventory:Backpack:Quantity", quantity);
        }

        public static void OnBackPackQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("SELECTEDINVITEM", out int itemIndex);

            player.GetData("Inventory:Backpack:Quantity", out int quantity);

            if (itemIndex == -1)
            {
                player.SendErrorNotification("An error has occurred. #ER03");
                return;
            }

            Inventory inventory = player.FetchInventory();

            InventoryItem item = inventory.GetInventory()[itemIndex];

            if (item == null)
            {
                player.SendErrorNotification("An error has occured. #ER04");
                player.SetData("SELECTEDINVITEM", -1);
                return;
            }

            Inventory backpackInventory = player.FetchBackpackInventory();

            if (backpackInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your backpack data.");
                return;
            }

            bool transferItem = inventory.TransferItem(backpackInventory, item, quantity);

            if (!transferItem)
            {
                player.SendErrorNotification("An error occurred moving the item!");
                return;
            }

            player.SendNotification($"~y~You've stored {item.CustomName} in your backpack.");

            player.DeleteData("Inventory:Backpack:Quantity");
        }

        public static void OnInventoryDropItemQuantitySelect(IPlayer player, string args)
        {
            if (args == "Close") return;
            player.GetData("SELECTEDINVITEM", out int itemIndex);
            Inventory inventory = player.FetchInventory();

            InventoryItem item = inventory.GetInventory()[itemIndex];

            bool hasQuantityData = player.GetData("DROPITEMQUANTITY", out int quantity);

            if (!hasQuantityData || quantity == 0)
            {
                quantity = 1;
            }

            if (item.Id.Contains("AMMO"))
            {
                bool tryParse = int.TryParse(item.ItemValue, out int ammoCount);

                if (!tryParse)
                {
                    player.SendErrorNotification("An error occurred parsing the ammo data when dropping!");
                    return;
                }

                int newAmmo = ammoCount - quantity;

                if (newAmmo == 0)
                {
                    bool itemRemoved = inventory.RemoveItem(item);
                    if (!itemRemoved)
                    {
                        player.SendErrorNotification("An error occurred removing the item!");
                        return;
                    }
                    player.SendInfoNotification($"You have dropped {item.CustomName} from your inventory.");
                    return;
                }
                else
                {
                    InventoryItem newItem = new InventoryItem(item.Id, item.CustomName, newAmmo.ToString(), item.Quantity);
                    InventoryItem ammoDroppedItem = new InventoryItem(item.Id, item.CustomName, quantity.ToString());

                    bool itemRemoved = inventory.RemoveItem(item);
                    if (!itemRemoved)
                    {
                        player.SendErrorNotification("An error occurred removing the item!");
                        return;
                    }

                    inventory.AddItem(newItem);
                    player.SendInfoNotification($"You have dropped x{quantity} of {item.CustomName} from your inventory.");
                    DroppedItems.CreateDroppedItem(ammoDroppedItem, player.Position.Around(0.5f));
                    return;
                }
            }

            bool success = inventory.RemoveItem(item, quantity);
            if (!success)
            {
                player.SendErrorNotification("An error occurred removing the item from your inventory.");
                return;
            }

            if (item.Id.Contains("PHONE"))
            {
                using Context context = new Context();

                var phone = context.Phones.FirstOrDefault(x => x.PhoneNumber == item.ItemValue);

                if (phone != null)
                {
                    phone.CharacterId = 0;

                    context.SaveChanges();
                }
            }

            player.SendInfoNotification($"You've dropped x{quantity} of {item.CustomName} from your inventory.");

            Logging.AddToCharacterLog(player, $"has dropped x{quantity} of {item.CustomName} from their inventory.");

            player.SetData("SELECTEDINVITEM", -1);

            InventoryItem droppedItem = item;

            droppedItem.Quantity = quantity;

            DroppedItems.CreateDroppedItem(droppedItem, player.Position.Around(0.5f));
        }

        public static void OnInventoryDropItemListChange(IPlayer player, string newQuantity)
        {
            int quantity = Convert.ToInt32(newQuantity);

            player.SetData("DROPITEMQUANTITY", quantity);
        }

        public static void OnInventoryDeleteItemListChange(IPlayer player, string newQuantity)
        {
            int quantity = Convert.ToInt32(newQuantity);

            player.SetData("DELETEITEMQUANTITY", quantity);

            Console.WriteLine("New Quantity: " + quantity);
        }

        public static void OnInventoryDeleteItemQuantitySelect(IPlayer player, string args)
        {
            if (args == "Close") return;
            Stopwatch sw = Stopwatch.StartNew();
            player.GetData("SELECTEDINVITEM", out int itemIndex);
            Inventory inventory = player.FetchInventory();

            InventoryItem item = inventory.GetInventory()[itemIndex];

            bool hasQuantityData = player.GetData("DELETEITEMQUANTITY", out int quantity);

            if (!hasQuantityData || quantity == 0)
            {
                quantity = 1;
            }

            if (item.Id.Contains("AMMO"))
            {
                bool tryParse = int.TryParse(item.ItemValue, out int ammoCount);

                if (!tryParse)
                {
                    player.SendErrorNotification("An error occurred parsing the ammo data when dropping!");
                    return;
                }

                int newAmmo = ammoCount - quantity;

                if (newAmmo == 0)
                {
                    bool itemRemoved = inventory.RemoveItem(item);
                    if (!itemRemoved)
                    {
                        player.SendErrorNotification("An error occurred removing the item!");
                        return;
                    }
                    player.SendInfoNotification($"You have removed {item.CustomName} from your inventory.");
                    return;
                }
                else
                {
                    InventoryItem newItem = new InventoryItem(item.Id, item.CustomName, newAmmo.ToString(), item.Quantity);

                    bool itemRemoved = inventory.RemoveItem(item);
                    if (!itemRemoved)
                    {
                        player.SendErrorNotification("An error occurred removing the item!");
                        return;
                    }

                    inventory.AddItem(newItem);
                    player.SendInfoNotification($"You have removed x{quantity} of {item.CustomName} from your inventory.");
                }

                return;
            }

            bool success = inventory.RemoveItem(item, quantity);
            if (!success)
            {
                player.SendErrorNotification("An error occurred removing the item from your inventory.");
                return;
            }
            if (item.Id.Contains("PHONE"))
            {
                using Context context = new Context();

                var phone = context.Phones.FirstOrDefault(x => x.PhoneNumber == item.ItemValue);

                if (phone != null)
                {
                    phone.CharacterId = 0;

                    context.SaveChanges();
                }
            }

            player.SendInfoNotification($"You've deleted x{quantity} of {item.CustomName} from your inventory.");

            Logging.AddToCharacterLog(player, $"has deleted x{quantity} of {item.CustomName} from their inventory.");

            player.SetData("DELETEITEMQUANTITY", 0);
            player.SetData("SELECTEDINVITEM", -1);
            sw.Stop();
            Console.WriteLine($"It took {sw.Elapsed} to process deleting an item.");
        }

        public static void OnInventoryItemGiveItemToPlayer(IPlayer player, string option)
        {
            if (option == "Close") return;

            IPlayer target = Alt.GetAllPlayers().FirstOrDefault(x => x.GetClass().Name == option);

            if (target?.FetchCharacter() == null)
            {
                player.SendErrorNotification("An error occurred. #ER06");
                return;
            }

            player.GetData("SELECTEDINVITEM", out int itemIndex);

            Inventory playerInventory = player.FetchInventory();

            InventoryItem item = playerInventory.GetInventory()[itemIndex];

            bool hasQuantityData = player.GetData("GIVEITEMQUANTITY", out int quantity);

            if (!hasQuantityData || quantity == 0)
            {
                player.SetData("GIVEITEMQUANTITY", 1);
            }

            player.SetData("GIVEITEMTARGETPLAYER", target.GetPlayerId());

            if (item.Id.Contains("AMMO"))
            {
                List<string> stringList = new List<string>();

                bool ammoParse = int.TryParse(item.ItemValue, out int ammoCount);

                if (!ammoParse)
                {
                    player.SendErrorNotification("An error occurred parsing your ammo.");
                    return;
                }

                for (int i = 1; i <= ammoCount; i++)
                {
                    stringList.Add(i.ToString());
                }

                List<NativeListItem> listItems = new List<NativeListItem>
                {
                    new NativeListItem {Title = "Quantity", StringList = stringList}
                };

                NativeMenu menu =
                    new NativeMenu("InventoryMenuGiveItemToPlayerQuantity", "Quantity", "Select a Quantity")
                    {
                        ListMenuItems = listItems,
                        ListTrigger = "InventoryMenuGiveItemToPlayerQuantityTrigger"
                    };

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }

            if (item.Quantity > 1)
            {
                List<string> stringList = new List<string>();

                for (int i = 1; i <= item.Quantity; i++)
                {
                    stringList.Add(i.ToString());
                }

                List<NativeListItem> listItems = new List<NativeListItem>
                {
                    new NativeListItem {Title = "Quantity", StringList = stringList}
                };

                NativeMenu menu =
                    new NativeMenu("InventoryMenuGiveItemToPlayerQuantity", "Quantity", "Select a Quantity")
                    {
                        ListMenuItems = listItems,
                        ListTrigger = "InventoryMenuGiveItemToPlayerQuantityTrigger"
                    };

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }

            OnGiveItemToPlayerQuantitySelect(player, "1");
        }

        public static void OnGiveItemToPlayerQuantityChange(IPlayer player, string newQuantity)
        {
            int quantity = Convert.ToInt32(newQuantity);

            player.SetData("GIVEITEMQUANTITY", quantity);
        }

        public static void OnGiveItemToPlayerQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("GIVEITEMQUANTITY", out int quantitySelected);

            player.GetData("GIVEITEMTARGETPLAYER", out int targetValue);

            player.GetData("SELECTEDINVITEM", out int itemIndex);

            IPlayer? target = Alt.GetAllPlayers().FirstOrDefault(x => x.GetPlayerId() == targetValue);

            if (target == null)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            Inventory playerInventory = player.FetchInventory();

            Inventory targetInventory = target.FetchInventory();

            InventoryItem item = playerInventory.GetInventory()[itemIndex];

            if (item.Id.Contains("AMMO"))
            {
                bool tryParse = int.TryParse(item.ItemValue, out int ammoCount);

                if (!tryParse)
                {
                    player.SendErrorNotification("An error occurred parsing the ammo data when dropping!");
                    return;
                }

                int newAmmo = ammoCount - quantitySelected;

                InventoryItem targetItem = new InventoryItem(item.Id, item.CustomName, quantitySelected.ToString());

                if (newAmmo == 0)
                {
                    InventoryItem targetAmmoItem = targetInventory.GetInventory().FirstOrDefault(x => x.Id == item.Id);

                    if (targetAmmoItem != null)
                    {
                        targetInventory.RemoveItem(targetAmmoItem);

                        int.TryParse(targetAmmoItem.ItemValue, out int targetAmmo);

                        InventoryItem newTargetAmmo = new InventoryItem(targetAmmoItem.Id, targetAmmoItem.CustomName, (targetAmmo + quantitySelected).ToString());

                        targetInventory.AddItem(newTargetAmmo);
                    }
                    else
                    {
                        bool itemAdded = targetInventory.AddItem(targetItem);
                        if (!itemAdded)
                        {
                            player.SendErrorNotification("An error occurred adding the item to their inventory!");
                            return;
                        }
                    }

                    bool itemRemoved = playerInventory.RemoveItem(item);
                    if (!itemRemoved)
                    {
                        player.SendErrorNotification("An error occurred removing the item!");
                        return;
                    }
                    player.SendInfoNotification($"You've given {target.GetClass().Name} {item.CustomName}");
                    target.SendInfoNotification($"{player.GetClass().Name} has given you {item.CustomName}");
                    return;
                }
                else
                {
                    InventoryItem newItem = new InventoryItem(item.Id, item.CustomName, newAmmo.ToString(), item.Quantity);

                    bool itemRemoved = playerInventory.RemoveItem(item);
                    if (!itemRemoved)
                    {
                        player.SendErrorNotification("An error occurred removing the item!");
                        return;
                    }

                    playerInventory.AddItem(newItem);
                    targetInventory.AddItem(targetItem);

                    player.SendInfoNotification($"You've given {target.GetClass().Name} {item.CustomName} x{quantitySelected}");
                    target.SendInfoNotification($"{player.GetClass().Name} has given you {item.CustomName} x{quantitySelected}");
                }

                return;
            }

            bool giveItem = playerInventory.TransferItem(targetInventory, item, quantitySelected);

            if (giveItem)
            {
                if (quantitySelected > 1)
                {
                    player.SendInfoNotification($"You've given {target.GetClass().Name} {item.CustomName} x{quantitySelected}");
                    target.SendInfoNotification($"{player.GetClass().Name} has given you {item.CustomName} x{quantitySelected}");
                }
                else
                {
                    player.SendInfoNotification($"You've given {target.GetClass().Name} {item.CustomName}");
                    target.SendInfoNotification($"{player.GetClass().Name} has given you {item.CustomName}");
                }

                if (item.Id.Contains("PHONE"))
                {
                    using Context context = new Context();

                    var phone = context.Phones.FirstOrDefault(x => x.PhoneNumber == item.ItemValue);

                    if (phone != null)
                    {
                        phone.CharacterId = target.GetClass().CharacterId;

                        context.SaveChanges();
                    }
                }

                player.DeleteData("GIVEITEMQUANTITY");
                player.DeleteData("GIVEITEMTARGETPLAYER");
                player.DeleteData("SELECTEDINVITEM");
                return;
            }

            player.DeleteData("GIVEITEMQUANTITY");
            player.SendErrorNotification("An error has occurred. #ER07");
            player.DeleteData("GIVEITEMTARGETPLAYER");
            player.DeleteData("SELECTEDINVITEM");
        }

        [Command("pickupitem", commandType: CommandType.Character, description: "Picks an item off the ground")]
        public static void CommandPickupItem(IPlayer player)
        {
            if (!player.GetClass().Spawned)
            {
                player.SendLoginError();
                return;
            }

            DroppedItem? droppedItem = DroppedItems.FetchNearestDroppedItem(player.Position, 2f);

            if (droppedItem == null)
            {
                player.SendErrorNotification("You're not near an item!");
                return;
            }

            Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            if (droppedItem.Item.Id.Contains("AMMO"))
            {
                InventoryItem? currentAmmoItem = playerInventory.GetInventory().FirstOrDefault(x => x.Id == droppedItem.Item.Id);
                if (currentAmmoItem != null)
                {
                    int.TryParse(droppedItem.Item.ItemValue, out int ammoCount);
                    int.TryParse(currentAmmoItem.ItemValue, out int currentAmmo);
                    InventoryItem newAmmoItem = new InventoryItem(currentAmmoItem.Id, currentAmmoItem.CustomName,
                        (currentAmmo + ammoCount).ToString());
                    if (!playerInventory.RemoveItem(currentAmmoItem))
                    {
                        player.SendErrorNotification("Unable to do this!");
                        return;
                    }

                    if (!playerInventory.AddItem(newAmmoItem))
                    {
                        player.SendErrorNotification("Unable to do this!");
                        return;
                    }
                    DroppedItems.RemoveDroppedItem(droppedItem);

                    player.SendInfoNotification($"You've picked up {droppedItem.Item.CustomName} from the ground.");
                    return;
                }
            }

            InventoryItem item = new InventoryItem(droppedItem.Item.Id, droppedItem.Item.CustomName, droppedItem.Item.ItemValue, droppedItem.Item.Quantity);

            if (playerInventory.AddItem(item))
            {
                DroppedItems.RemoveDroppedItem(droppedItem);

                player.SendInfoNotification($"You've picked up {item.CustomName} from the ground.");

                if (droppedItem.Item.Id.Contains("PHONE"))
                {
                    using Context context = new Context();

                    var phone = context.Phones.FirstOrDefault(x => x.PhoneNumber == droppedItem.Item.ItemValue);

                    if (phone != null)
                    {
                        phone.CharacterId = player.GetClass().CharacterId;

                        context.SaveChanges();
                    }
                }
                return;
            }

            player.SendErrorNotification("There was an error adding this item to your inventory.");
        }

        [Command("giveitem", onlyOne: true, commandType: CommandType.Inventory,
            description: "Used to give an item to a player")]
        public static void InventoryCommandGiveItem(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/giveitem [NameOrId]");
                return;
            }

            Inventory inventory = player.FetchInventory();

            if (inventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Target player not found.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 3)
            {
                player.SendErrorNotification("Your not in range.");
                return;
            }

            if (!inventory.GetInventory().Any())
            {
                player.SendErrorNotification("You don't have any items.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (var item in inventory.GetInventory())
            {
                menuItems.Add(new NativeMenuItem(item.CustomName, $"Quantity: {item.Quantity}."));
            }

            NativeMenu menu = new NativeMenu("Inventory:OnGiveItemToPlayerSelect", "Inventory",
                "Select an item to give", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("Inventory:SelectItemGiveToPlayer", targetPlayer.GetPlayerId());
        }

        public static void OnSelectItemGiveToPlayer(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory inventory = player.FetchInventory();

            List<InventoryItem> inventoryItems = inventory.GetInventory();

            InventoryItem selectedItem = inventoryItems[index];

            if (selectedItem == null)
            {
                player.SendErrorNotification("Unable to find the selected item.");
                return;
            }

            if (selectedItem.Quantity > 1)
            {
                List<string> stringList = new List<string>();

                for (int i = 1; i <= selectedItem.Quantity; i++)
                {
                    stringList.Add(i.ToString());
                }

                List<NativeListItem> listItems = new List<NativeListItem>
                {
                    new NativeListItem("Quantity", stringList)
                };

                NativeMenu menu = new NativeMenu("Inventory:SelectedItemGiveToPlayerQuantity", "Inventory",
                    "Select a quantity")
                {
                    ListTrigger = "Inventory:SelectedItemGiveToPlayerQuantityList",
                    ListMenuItems = listItems
                };

                player.SetData("Inventory:GiveSelectedInventoryItemIndex", inventoryItems.IndexOf(selectedItem));
                player.SetData("Inventory:GiveSelectedInventoryQuantity", 1);

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }

            bool hasData = player.GetData("Inventory:SelectItemGiveToPlayer", out int targetId);

            if (!hasData)
            {
                player.SendErrorNotification("An error has occurred.");
                return;
            }

            IPlayer? targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x.GetPlayerId() == targetId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            Inventory targetInventory = targetPlayer.FetchInventory();

            if (targetInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching the inventory of the target.");
                return;
            }

            bool transferred = inventory.TransferItem(targetInventory, selectedItem, selectedItem.Quantity);

            if (!transferred)
            {
                player.SendErrorNotification("An error occurred sending the item.");
                return;
            }

            player.SendInfoNotification($"You've given {targetPlayer.GetClass().Name} {selectedItem.CustomName}.");

            targetPlayer.SendInfoNotification($"You've received {selectedItem.CustomName} from {player.GetClass().Name}.");
        }

        public static void SelectedItemGiveToPlayerQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("Inventory:GiveSelectedInventoryItemIndex", out int itemIndex);
            player.GetData("Inventory:GiveSelectedInventoryQuantity", out int quantity);

            Inventory inventory = player.FetchInventory();

            List<InventoryItem> inventoryItems = inventory.GetInventory();

            InventoryItem selectedItem = inventoryItems[itemIndex];

            if (selectedItem == null)
            {
                player.SendErrorNotification("Unable to find the selected item.");
                return;
            }

            bool hasData = player.GetData("Inventory:SelectItemGiveToPlayer", out int targetId);

            if (!hasData)
            {
                player.SendErrorNotification("An error has occurred.");
                return;
            }

            IPlayer? targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x.GetPlayerId() == targetId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            Inventory targetInventory = targetPlayer.FetchInventory();

            if (targetInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching the inventory of the target.");
                return;
            }

            bool transferred = inventory.TransferItem(targetInventory, selectedItem, quantity);

            if (!transferred)
            {
                player.SendErrorNotification("An error occurred sending the item.");
                return;
            }

            player.SendInfoNotification($"You've given {targetPlayer.GetClass().Name} x{quantity} of {selectedItem.CustomName}.");

            targetPlayer.SendInfoNotification($"You've received x{quantity} of {selectedItem.CustomName} from {player.GetClass().Name}.");
        }

        public static void SelectedItemGiveToPlayerQuantityChange(IPlayer player, string itemText, string listText)
        {
            int quantity = int.Parse(listText);

            player.SetData("Inventory:GiveSelectedInventoryQuantity", quantity);
        }
    }
}