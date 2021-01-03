using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;

namespace Server.Character.Clothing
{
    public class ClothingCommand
    {
        public static List<string> BannedClothingNames = new List<string>
        {
            "undefined",
            "null",
            "none",
            "love heart boxer shorts",
            "no tie",
            "no shoes",
        };

        [Command("clothes", commandType: CommandType.Character, description: "Allows control of your clothing.")]
        public static void CommandClothes(IPlayer player)
        {
            if (player.Model != Alt.Hash("mp_f_freemode_01"))
            {
                if (player.Model != Alt.Hash("mp_m_freemode_01"))
                {
                    player.SendErrorNotification("You must be in a Freemode skin to use this.");
                    return;
                }
            }

            NativeMenu clothesMenu = new NativeMenu("ClothesMainMenuSelect", "Clothing", "Select an item");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Mask"),
                new NativeMenuItem("Tops"),
                new NativeMenuItem("Undershirt"),
                new NativeMenuItem("Legs"),
                new NativeMenuItem("Bags"),
                new NativeMenuItem("Feet"),
                new NativeMenuItem("Accessories"),
                new NativeMenuItem("Props"),
            };

            clothesMenu.MenuItems = menuItems;

            NativeUi.ShowNativeMenu(player, clothesMenu, true);
        }

        public static void OnClothingMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Props")
            {
                AccessoriesMenu(player);
                return;
            }

            player.SetData("ClothesMainMenuSelect", option);

            NativeMenu clothingInteractionMenu =
                new NativeMenu("ClothesInteraction", option, "Choose what you wish to do");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Take Off"), new NativeMenuItem("Switch")
            };

            clothingInteractionMenu.MenuItems = menuItems;

            NativeUi.ShowNativeMenu(player, clothingInteractionMenu, true);
        }

        public static void EventOnClothesInteractionSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            int slot = 0;

            player.GetData("ClothesMainMenuSelect", out string selectedMenuItem);

            switch (selectedMenuItem)
            {
                case "Mask":
                    slot = 1;
                    break;

                case "Tops":
                    slot = 11;
                    break;

                case "Undershirt":
                    slot = 8;
                    break;

                case "Legs":
                    slot = 4;
                    break;

                case "Bags":
                    slot = 5;
                    break;

                case "Feet":
                    slot = 6;
                    break;

                case "Accessories":
                    slot = 7;
                    break;
            }

            player.SetData("SwitchClothingSlot", slot);

            if (option == "Take Off")
            {
                TakeClothingItemOff(player);
                return;
            }

            if (option == "Switch")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                List<InventoryItem> items = playerInventory.GetInventory().Where(i => i.Id == "ITEM_CLOTHES")
                    .OrderBy(x => x.CustomName)
                    .ToList();

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                List<InventoryItem> newList = new List<InventoryItem>();

                foreach (var inventoryItem in items)
                {
                    ClothesData data = Clothes.ConvertItemToClothesData(inventoryItem);

                    if (data == null) continue;

                    if (data.slot == slot)
                    {
                        newList.Add(inventoryItem);
                        continue;
                    }
                }

                foreach (var inventoryItem in newList)
                {
                    if (inventoryItem.CustomName.ToLower() == "undefined" ||
                        inventoryItem.CustomName.ToLower() == "topless")
                    {
                        playerInventory.RemoveItem(inventoryItem);
                        continue;
                    }

                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
                }

                player.SetData("SwitchClothingItems", JsonConvert.SerializeObject(items));

                NativeMenu newMenu = new NativeMenu("SwitchClothingItemSelected", "Clothes",
                    "Select the item you wish to put on", menuItems);

                NativeUi.ShowNativeMenu(player, newMenu, true);
            }
        }

        public static void SelectedSwitchClothingItem(IPlayer player, string option)
        {
            try
            {
                if (option == "Close") return;

                player.GetData("SwitchClothingSlot", out int slot);

                player.GetData("SwitchClothingItems", out string jsonClothingItems);

                List<InventoryItem> items =
                    JsonConvert.DeserializeObject<List<InventoryItem>>(jsonClothingItems);

                InventoryItem selectedItem = items.FirstOrDefault(x => x.CustomName == option);

                if (selectedItem == null) return;

                ClothesData newClothesData = Clothes.ConvertItemToClothesData(selectedItem);

                if (newClothesData == null) return;

                if (newClothesData.male && !player.GetClass().IsMale)
                {
                    player.SendErrorNotification($"Clothing item isn't for your gender.");
                    return;
                }

                Inventory.Inventory playerInventory = player.FetchInventory();

                InventoryItem newItem = null;

                List<ClothesData> playerClothesDatas =
                    JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson);

                ClothesData currentClothesData = playerClothesDatas.FirstOrDefault(i => i.slot == slot);

                if (player.IsMale())
                {
                    currentClothesData.male = true;
                }

                newItem = Clothes.ConvertClothesToInventoryItem(currentClothesData, player.IsMale());

                if (!BannedClothingNames.Contains(newItem.CustomName.ToLower()))
                {
                    if (!playerInventory.HasItem(newItem))
                    {
                        bool added = playerInventory.AddItem(newItem);

                        if (!added)
                        {
                            player.SendErrorNotification("No space to take this off.");
                            return;
                        }
                    }
                }
#if DEBUG
                else
                {
                    Console.WriteLine($"Banned Clothing Name {newItem.CustomName.ToLower()} hasn't been added to {player.GetClass().Name}.");
                }
#endif

                playerInventory.RemoveItem(playerInventory.GetInventory()
                    .FirstOrDefault(s => s.CustomName == selectedItem.CustomName));

                Clothes.SetClothes(player, newClothesData);
                Clothes.SaveClothes(player, newClothesData);

                Logging.AddToCharacterLog(player, $"Has switched clothing slot {slot} to {selectedItem.CustomName}");

                player.SendInfoNotification($"Clothing changed.");

                if (slot == 1)
                {
                    using Context context = new Context();
                    Models.Character dbCharacter = context.Character.Find(player.FetchCharacterId());

                    dbCharacter.Anon = true;

                    player.SendNotification("Your anonymity has been turned ~g~on");

                    player.GetClass().Name = $"Mask {CharacterHandler.NextMaskId}";

                    context.SaveChanges();

                    CharacterHandler.NextMaskId++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static void TakeClothingItemOff(IPlayer player)
        {
            player.GetData("SwitchClothingSlot", out int selectedItem);

            List<ClothesData> playerClothesDatas =
                JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson);

            ClothesData currentClothesData = playerClothesDatas.FirstOrDefault(i => i.slot == selectedItem);

            Inventory.Inventory playerInventory = player.FetchInventory();

            int nakedItem = new int();

            if (player.IsMale())
            {
                switch (selectedItem)
                {
                    case 1:
                        nakedItem = 0;
                        break;

                    case 3:
                        nakedItem = 15;
                        break;

                    case 4:
                        nakedItem = 21;
                        break;

                    case 5:
                        nakedItem = 0;
                        break;

                    case 6:
                        nakedItem = 34;
                        break;

                    case 7:
                        nakedItem = 0;
                        break;

                    case 8:
                        nakedItem = 15;
                        break;

                    case 9:
                        nakedItem = 0;
                        break;

                    case 11:
                        nakedItem = 15;
                        break;

                    case 15:
                        nakedItem = 15;
                        break;
                }
            }
            else
            {
                switch (selectedItem)
                {
                    case 1:
                        nakedItem = 0;
                        break;

                    case 3:
                        nakedItem = 15;
                        break;

                    case 4:
                        nakedItem = 17;
                        break;

                    case 5:
                        nakedItem = 0;
                        break;

                    case 6:
                        nakedItem = 35;
                        break;

                    case 7:
                        nakedItem = 0;
                        break;

                    case 8:
                        nakedItem = 2;
                        break;

                    case 9:
                        nakedItem = 0;
                        break;

                    case 11:
                        nakedItem = 82;
                        break;
                }
            }

            InventoryItem item = Clothes.ConvertClothesToInventoryItem(currentClothesData, player.IsMale());

            if (currentClothesData.drawable != nakedItem)
            {
                if (!playerInventory.HasItem(item.Id, item.ItemValue))
                {
                    bool added = playerInventory.AddItem(item);

                    if (!added)
                    {
                        player.SendErrorNotification("No space to take this off.");
                        return;
                    }
                }
            }

            if (selectedItem == 1)
            {
                // Mask

                using Context context = new Context();
                Models.Character dbCharacter = context.Character.Find(player.FetchCharacterId());

                if (dbCharacter.Anon)
                {
                    dbCharacter.Anon = false;

                    player.SendNotification($"Your anonymity has been turned ~r~off~w~.");

                    context.SaveChanges();
                }

                player.GetClass().Name = dbCharacter.Name;
            }

            playerClothesDatas.Remove(currentClothesData);

            var newClothesData = new ClothesData(selectedItem, nakedItem, 0, player.IsMale());

            if (player.IsMale())
            {
                newClothesData.male = true;
            }

            Clothes.SetClothes(player, newClothesData);
            Clothes.SaveClothes(player, newClothesData);

            Logging.AddToCharacterLog(player, $"Has taken off clothing slot {selectedItem}");
        }

        public static void AccessoriesMenu(IPlayer player)
        {
            NativeMenu accessoriesMenu = new NativeMenu("AccessoriesMainMenuSelect", "Props", "Select an item");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Hats"),
                new NativeMenuItem("Glasses"),
                new NativeMenuItem("Ears"),
                new NativeMenuItem("Watches"),
                new NativeMenuItem("Bracelets")
            };

            accessoriesMenu.MenuItems = menuItems;

            NativeUi.ShowNativeMenu(player, accessoriesMenu, true);
        }

        public static void EventAccessoriesMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.SetData("AccessoriesMainMenuSelect", option);

            NativeMenu accessoryInteractionMenu =
                new NativeMenu("AccessoryInteraction", option, "Choose what you would like to do");

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Take off"), new NativeMenuItem("Switch")
            };

            accessoryInteractionMenu.MenuItems = menuItems;

            NativeUi.ShowNativeMenu(player, accessoryInteractionMenu, true);
        }

        public static void EventAccessoryInteraction(IPlayer player, string option)
        {
            if (option == "Close") return;

            int slot = 0;
            player.GetData("AccessoriesMainMenuSelect", out string accessoryMenuSelected);

            slot = accessoryMenuSelected switch
            {
                "Hats" => 0,
                "Glasses" => 1,
                "Ears" => 2,
                "Watches" => 6,
                "Bracelets" => 7,
                _ => -1
            };

            player.SetData("SwitchClothingSlot", slot);

            if (option == "Take off")
            {
                TakeAccessoryItemOff(player);
                return;
            }

            if (option == "Switch")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                List<InventoryItem> items = playerInventory.GetInventory().Where(i => i.Id == "ITEM_CLOTHES_ACCESSORY")
                    .OrderBy(x => x.CustomName)
                    .ToList();

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                List<InventoryItem> newList = new List<InventoryItem>();

                foreach (var inventoryItem in items)
                {
                    ClothesData data = Clothes.ConvertItemToClothesData(inventoryItem);

                    if (data == null) continue;

                    if (data.slot == slot)
                    {
                        newList.Add(inventoryItem);
                        continue;
                    }
                }

                foreach (var inventoryItem in newList)
                {
                    if (inventoryItem.CustomName.ToLower() == "undefined" ||
                        inventoryItem.CustomName.ToLower() == "topless")
                    {
                        playerInventory.RemoveItem(inventoryItem);
                        continue;
                    }

                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
                }

                player.SetData("SwitchAccessoryItems", JsonConvert.SerializeObject(items));

                NativeMenu newMenu = new NativeMenu("SwitchAccessoryItemSelected", "Accessories",
                    "Select the accessory you wish to put on", menuItems);

                NativeUi.ShowNativeMenu(player, newMenu, true);
            }
        }

        public static void EventSwitchAccessoryItemSelected(IPlayer player, string option)
        {
            try
            {
                if (option == "Close") return;

                player.GetData("SwitchClothingSlot", out int slot);
                player.GetData("SwitchAccessoryItems", out string accessoryJson);

                List<InventoryItem> items =
                    JsonConvert.DeserializeObject<List<InventoryItem>>(accessoryJson);

                InventoryItem selectedItem = items.FirstOrDefault(s => s.CustomName == option);

                if (selectedItem == null)
                {
                    Console.WriteLine("Selected Item is null #536-ClothingCommand.cs");
                    return;
                }

                ClothesData newClothesData = Clothes.ConvertItemToClothesData(selectedItem);

                if (newClothesData == null)
                {
                    Console.WriteLine("newClothesData is null #545-ClothingCommand.cs");
                    return;
                }

                if (newClothesData.male && !player.IsMale())
                {
                    player.SendErrorNotification($"Clothing item isn't for your gender.");
                    return;
                }

                Inventory.Inventory playerInventory = player.FetchInventory();

                InventoryItem newItem = null;

                List<AccessoryData> playerClothesDatas =
                    JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson);

                AccessoryData currentClothesData = playerClothesDatas.FirstOrDefault(i => i.slot == slot);

                if (currentClothesData != null)
                {
                    if (player.IsMale())
                    {
                        currentClothesData.male = true;
                    }

                    var clothesItem = new AccessoryData(currentClothesData.slot, currentClothesData.drawable,
                        currentClothesData.texture, player.IsMale());

                    newItem = Clothes.ConvertAccessoryToInventoryItem(clothesItem, player.IsMale());

                    bool itemAdded = playerInventory.AddItem(newItem);

                    if (!itemAdded)
                    {
                        player.SendErrorNotification("An error occurred adding the item to your inventory.");
                        return;
                    }

                    playerInventory.RemoveItem(playerInventory.GetInventory()
                        .FirstOrDefault(s => s.CustomName == selectedItem.CustomName));

                    var newAccessory = new AccessoryData(newClothesData.slot, newClothesData.drawable,
                        newClothesData.texture, player.IsMale());

                    Clothes.SetAccessories(player, newAccessory);
                    Clothes.SaveAccessories(player, newAccessory);

                    player.SendInfoNotification($"Clothing changed.");

                    Logging.AddToCharacterLog(player,
                        $"Has switched accessory slot {slot} to {selectedItem.CustomName}");
                }
                else
                {
                    var newAccessory = new AccessoryData(newClothesData.slot, newClothesData.drawable,
                        newClothesData.texture, player.IsMale());

                    Clothes.SetAccessories(player, newAccessory);
                    Clothes.SaveAccessories(player, newAccessory);

                    player.SendInfoNotification($"Clothing changed.");

                    Logging.AddToCharacterLog(player,
                        $"Has switched accessory slot {slot} to {selectedItem.CustomName}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static void TakeAccessoryItemOff(IPlayer player)
        {
            try
            {
                player.GetData("SwitchClothingSlot", out int selectedItem);

                List<AccessoryData> playerClothesDatas =
                    JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson);

                AccessoryData currentClothesData = playerClothesDatas.FirstOrDefault(i => i.slot == selectedItem);

                Inventory.Inventory playerInventory = player.FetchInventory();

                var accessoryItem = new AccessoryData(currentClothesData.slot, currentClothesData.drawable,
                    currentClothesData.texture, player.IsMale());

                bool added =
                    playerInventory.AddItem(Clothes.ConvertAccessoryToInventoryItem(accessoryItem, player.IsMale()));

                if (!added)
                {
                    player.SendErrorNotification("No space to take this off.");
                    return;
                }

                playerClothesDatas.Remove(currentClothesData);

                var newClothesData = new ClothesData(selectedItem, -1, 0, player.IsMale());

                if (player.IsMale())
                {
                    newClothesData.male = true;
                }

                AccessoryData newAccessoryData =
                    new AccessoryData(newClothesData.slot, -1, newClothesData.texture, player.IsMale());

                Clothes.SetAccessories(player, newAccessoryData);
                Clothes.SaveAccessories(player, newAccessoryData);

                Logging.AddToCharacterLog(player, $"Has taken accessory slot {selectedItem} off.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}