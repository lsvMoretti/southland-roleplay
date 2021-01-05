using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Backpack;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Inventory.OpenInventory
{
    public class OpenInventoryCommands
    {
        [Command("storage", commandType: CommandType.Inventory,
            description: "Used to interact with open world storage lockers")]
        public static void OICommandStorage(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Storage nearStorage = Storage.FetchNearestStorage(player.Position);

            if (nearStorage == null) return;

            Inventory storageInventory = Storage.FetchInventory(nearStorage);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Store"),
                new NativeMenuItem("Take")
            };

            NativeMenu menu = new NativeMenu("OpenInventory:StorageMainMenu", "Storage", $"Space: {storageInventory.CurrentWeight}/{storageInventory.MaximumWeight}.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("NearOWStorage", nearStorage.Id);
        }

        public static void OnOWStorageMainMenu(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Take")
            {
                ShowStorageTakeMenu(player);
                return;
            }

            if (option == "Store")
            {
                ShowStoragePlaceMenu(player);
                return;
            }
        }

        #region Take items

        private static void ShowStorageTakeMenu(IPlayer player)
        {
            bool hasNearData = player.GetData("NearOWStorage", out int storageId);

            if (!hasNearData)
            {
                player.SendErrorNotification("Your not near a storage location.");
                return;
            }

            Storage nearStorage = Storage.FetchStorage(storageId);

            if (nearStorage == null)
            {
                player.SendErrorNotification("Your not near a storage location.");
                return;
            }

            Inventory storageInventory = Storage.FetchInventory(nearStorage);

            if (!storageInventory.GetInventory().Any())
            {
                player.SendNotification("~r~It's empty!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in storageInventory.GetInventory())
            {
                if (inventoryItem.Id.Contains("WEAPON") && !inventoryItem.Id.Contains("AMMO"))
                {
                    WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(inventoryItem.ItemValue);

                    if (weaponInfo != null)
                    {
                        if (weaponInfo.AmmoCount > 0)
                        {
                            NativeMenuItem menuItem = new NativeMenuItem(inventoryItem.CustomName, $"{inventoryItem.ItemInfo.Description} - Bullets: {weaponInfo.AmmoCount}");
                            menuItems.Add(menuItem);
                            continue;
                        }
                    }
                }

                menuItems.Add(new NativeMenuItem(inventoryItem.CustomName, $"Quantity: {inventoryItem.Quantity}"));
            }

            NativeMenu menu = new NativeMenu("OpenInventory:StorageTakeItem", "Storage", $"Space: {storageInventory.CurrentWeight}/{storageInventory.MaximumWeight}.", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnOWTakeItemSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            bool hasNearData = player.GetData("NearOWStorage", out int storageId);

            if (!hasNearData)
            {
                player.SendErrorNotification("Your not near a storage location.");
                return;
            }

            Storage nearStorage = Storage.FetchStorage(storageId);

            if (nearStorage == null)
            {
                player.SendErrorNotification("Your not near a storage location.");
                return;
            }

            Inventory storageInventory = Storage.FetchInventory(nearStorage);

            List<InventoryItem> storageItems = storageInventory.GetInventory();

            if (!storageItems.Any())
            {
                player.SendNotification("~r~It's empty!");
                return;
            }

            InventoryItem selectedItem = storageItems[index];

            if (selectedItem == null)
            {
                player.SendErrorNotification("Unable to find this item. Try again!");
                return;
            }

            Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory!");
                return;
            }

            bool transferred = storageInventory.TransferItem(playerInventory, selectedItem, selectedItem.Quantity);

            if (!transferred)
            {
                player.SendErrorNotification("This item couldn't be added. You got space bro?");
                return;
            }

            player.SendInfoNotification($"You've taken {selectedItem.CustomName} from the storage.");
            player.SendEmoteMessage($"reaches in and takes an item from the bin.");
        }

        #endregion Take items

        #region Store Items

        private static void ShowStoragePlaceMenu(IPlayer player)
        {
            Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in playerInventory.GetInventory())
            {
                if (inventoryItem.Id.Contains("WEAPON") && !inventoryItem.Id.Contains("AMMO"))
                {
                    WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(inventoryItem.ItemValue);

                    if (weaponInfo != null)
                    {
                        if (weaponInfo.AmmoCount > 0)
                        {
                            NativeMenuItem menuItem = new NativeMenuItem(inventoryItem.CustomName, $"{inventoryItem.ItemInfo.Description} - Bullets: {weaponInfo.AmmoCount}");
                            menuItems.Add(menuItem);
                            continue;
                        }
                    }
                }
                menuItems.Add(new NativeMenuItem(inventoryItem.CustomName, $"Quantity: {inventoryItem.Quantity}."));
            }

            NativeMenu menu = new NativeMenu("OpenInventory:StoreItem", "Storage", "", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnOWStoreItemSelect(IPlayer player, string option, int index)
        {
            Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            List<InventoryItem> inventoryItems = playerInventory.GetInventory();

            InventoryItem selectedItem = inventoryItems[index];

            if (selectedItem == null)
            {
                player.SendErrorNotification("Unable to find the item.");
                return;
            }

            bool hasNearData = player.GetData("NearOWStorage", out int storageId);

            if (!hasNearData)
            {
                player.SendErrorNotification("Your not near a storage location.");
                return;
            }

            Storage nearStorage = Storage.FetchStorage(storageId);

            if (nearStorage == null)
            {
                player.SendErrorNotification("Your not near a storage location.");
                return;
            }

            if (selectedItem.Id == "ITEM_BACKPACK" || selectedItem.Id == "ITEM_DUFFELBAG")
            {
                BackpackHandler.StoreBackpackInStorage(player, selectedItem, nearStorage);
                return;
            }

            Inventory storageInventory = Storage.FetchInventory(nearStorage);

            bool transferred = playerInventory.TransferItem(storageInventory, selectedItem, selectedItem.Quantity);

            if (!transferred)
            {
                player.SendErrorNotification("An error occurred placing the item.");
                return;
            }

            player.SendInfoNotification($"You've stored {selectedItem.CustomName} in the bin. Bin Id: {nearStorage.Id}.");

            player.SendEmoteMessage($"reaches into the bin placing an item.");
        }

        #endregion Store Items

        [Command("sbalance", commandType: CommandType.Inventory,
            description: "Used to interact with the open world storages")]
        public static void OICommandViewBalance(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Storage nearStorage = Storage.FetchNearestStorage(player.Position);

            if (nearStorage == null) return;

            player.SendInfoNotification($"Amount in storage: {nearStorage.Balance:C}.");
            Logging.AddToCharacterLog(player, $"has looked at the balance in storage container ID {nearStorage.Id}.");
        }

        [Command("sdeposit", onlyOne: true, commandType: CommandType.Inventory,
            description: "Used to interact with the open world storage")]
        public static void OICommandDeposit(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/sdeposit [Amount]");
                return;
            }

            Storage nearStorage = Storage.FetchNearestStorage(player.Position);

            if (nearStorage == null) return;

            bool tryParse = double.TryParse(args, out double amount);

            if (!tryParse || amount <= 0)
            {
                player.SendSyntaxMessage("/sdeposit [Amount]");
                return;
            }

            if (player.GetClass().Cash < amount)
            {
                player.SendErrorNotification("You don't have enough on you!");
                return;
            }

            using Context context = new Context();

            Storage storage = context.Storages.Find(nearStorage.Id);

            if (storage == null) return;

            storage.Balance += amount;

            player.RemoveCash(amount);

            context.SaveChanges();

            player.SendInfoNotification($"You've stored {amount:C} into the storage container.");

            Logging.AddToCharacterLog(player, $"has stored {amount:C} into the storage container ID {storage.Id}.");
        }

        [Command("swithdraw", onlyOne: true, commandType: CommandType.Inventory,
            description: "Used to interact with the open world storage")]
        public static void OICommandWithdraw(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/swithdraw [Amount]");
                return;
            }

            Storage nearStorage = Storage.FetchNearestStorage(player.Position);

            if (nearStorage == null) return;

            bool tryParse = double.TryParse(args, out double amount);

            if (!tryParse || amount <= 0)
            {
                player.SendSyntaxMessage("/swithdraw [Amount]");
                return;
            }

            if (nearStorage.Balance < amount)
            {
                player.SendErrorNotification("There isn't this much in the storage!");
                return;
            }

            using Context context = new Context();

            Storage storage = context.Storages.Find(nearStorage.Id);

            if (storage == null) return;

            storage.Balance -= amount;

            player.AddCash(amount);

            context.SaveChanges();

            player.SendInfoNotification($"You've taken {amount:C} from the storage container.");

            Logging.AddToCharacterLog(player, $"has taken {amount:C} from the storage container ID {storage.Id}.");
        }
    }
}