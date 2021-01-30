using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Elements.Refs;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Inventory;

namespace Server.Backpack
{
    public class BackpackCommands
    {
        [Command("pickupbackpack", commandType: CommandType.Character,
            description: "Backpack: Used to pickup a dropped backpack.")]
        public static void PickupBackPack(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Position playerPosition = player.Position;

            Models.Backpack nearestBackPack = null;
            float distance = 3f;
            TextLabel backpackLabel = null;

            foreach (KeyValuePair<int, TextLabel> droppedLabel in BackpackHandler.DroppedLabels)
            {
                Models.Backpack backpack = Models.Backpack.FetchBackpack(droppedLabel.Key);

                if (backpack == null)
                {
                    droppedLabel.Value.Remove();
                    BackpackHandler.DroppedLabels.Remove(droppedLabel.Key);
                    continue;
                }

                Position backPackPosition = new Position(backpack.DropPosX, backpack.DropPosY, backpack.DropPosZ);

                if (backPackPosition == Position.Zero) continue;

                float dis = backPackPosition.Distance(playerPosition);

                if (dis < distance)
                {
                    nearestBackPack = backpack;
                    distance = dis;
                    backpackLabel = droppedLabel.Value;
                }
            }

            if (nearestBackPack == null)
            {
                player.SendErrorNotification("Your not near any backpacks.");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter.BackpackId > 0)
            {
                player.SendErrorNotification("You already have a backpack.");
                return;
            }

            Models.Backpack backpackDb = context.Backpacks.Find(nearestBackPack.Id);

            backpackDb.DropPosX = 0;
            backpackDb.DropPosY = 0;
            backpackDb.DropPosZ = 0;

            playerCharacter.BackpackId = backpackDb.Id;

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (backpackDb.Drawable == 31)
            {
                // Backpack

                InventoryItem newItem = new InventoryItem("ITEM_BACKPACK", "Backpack", backpackDb.Id.ToString());
                playerInventory.AddItem(newItem);
            }

            if (backpackDb.Drawable == 44)
            {
                // Duffle

                InventoryItem newItem = new InventoryItem("ITEM_DUFFELBAG", "Duffelbag", backpackDb.Id.ToString());
                playerInventory.AddItem(newItem);
            }

            context.SaveChanges();
            
            backpackLabel?.Remove();
            BackpackHandler.DroppedLabels.Remove(backpackDb.Id);

            player.SendInfoNotification($"You have picked up a backpack.");

            player.LoadCharacterCustomization();
        }

        [Command("backpack", commandType: CommandType.Character,
            description: "Backpack: Shows your backpack Inventory")]
        public static void BackpackCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter.BackpackId == 0)
            {
                player.SendErrorNotification("You don't have a backpack on you!");
                return;
            }

            Models.Backpack backpack = Models.Backpack.FetchBackpack(playerCharacter.BackpackId);

            if (backpack == null)
            {
                player.SendErrorNotification("Unable to fetch backpack data!");
                return;
            }

            Inventory.Inventory backpackInventory = Models.Backpack.FetchBackpackInventory(backpack);

            if (!backpackInventory.GetInventory().Any())
            {
                player.SendNotification("~y~Backpack is empty.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem item in backpackInventory.GetInventory())
            {
                if (item.Id.Contains("WEAPON") && !item.Id.Contains("AMMO"))
                {
                    WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(item.ItemValue);

                    if (weaponInfo != null)
                    {
                        menuItems.Add(new NativeMenuItem(item.CustomName, $"Bullet Count: {weaponInfo.AmmoCount}"));
                        continue;
                    }
                }

                menuItems.Add(item.Quantity > 1
                    ? new NativeMenuItem(item.CustomName, $"Quantity x{item.Quantity}")
                    : new NativeMenuItem(item.CustomName, item.ItemInfo.Description));
            }

            NativeMenu menu = new NativeMenu("backpack:MainMenuSelect", "Backpack", "Select an item", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnBackpackMainMenuSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory backpackInventory = player.FetchBackpackInventory();

            if (backpackInventory == null)
            {
                player.SendErrorNotification("There was an error fetching the inventory.");
                return;
            }

            InventoryItem selectedItem = backpackInventory.GetInventory()[index];

            if (selectedItem == null)
            {
                player.SendErrorNotification("The selected backpack item doesn't exist!");
                return;
            }

            player.SetData("Backpack:SelectedItemIndex", index);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Transfer to player"),
                new NativeMenuItem("Drop Item"),
                new NativeMenuItem("Delete Item"),
                new NativeMenuItem("Give to player"),
                new NativeMenuItem("Transfer to Vehicle"),
            };

            NativeMenu menu = new NativeMenu("backpack:ShowItemOptions", "Backpack", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnShowItemOptionSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            Inventory.Inventory backpackInventory = player.FetchBackpackInventory();

            Inventory.Inventory playerInventory = player.FetchInventory();

            player.GetData("Backpack:SelectedItemIndex", out int itemIndex);

            InventoryItem selectedItem = backpackInventory.GetInventory()[itemIndex];

            if (selectedItem == null)
            {
                player.SendErrorNotification("The selected backpack item doesn't exist!");
                return;
            }

            if (option == "Transfer to player")
            {
                if (selectedItem.Quantity > 1)
                {
                    List<NativeListItem> listItems = new List<NativeListItem>
                    {
                        new NativeListItem("Quantity", selectedItem.QuantityListString)
                    };
                    
                    NativeMenu menu = new NativeMenu("backpack:OnItemQuantitySelect", "Backpack", "Select an option")
                    {
                        ListMenuItems = listItems,
                        ListTrigger = "backpack:ItemQuantityChange"
                    };
                    
                    player.SetData("backpack:ItemOption", option);
                    player.SetData("backpack:QuantityChange", 1);
                    
                    NativeUi.ShowNativeMenu(player, menu, true);
                    return;
                }
                
                bool playerTransfer = backpackInventory.TransferItem(playerInventory, selectedItem, selectedItem.Quantity);
                if (!playerTransfer)
                {
                    player.SendErrorNotification("There was an error transferring the item.");
                    return;
                }

                player.SendNotification($"~y~You've transferred {selectedItem.CustomName} back to you.");
                return;
            }

            if (option == "Drop Item")
            {
                bool itemRemoved = backpackInventory.RemoveItem(selectedItem, selectedItem.Quantity);

                if (!itemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the item.");
                    return;
                }

                DroppedItems.CreateDroppedItem(selectedItem, player.Position);

                player.SendEmoteMessage($"drops an item on the ground from their backpack.");

                player.SendNotification(selectedItem.Quantity > 1
                    ? $"~y~You've dropped {selectedItem.CustomName} x{selectedItem.Quantity}."
                    : $"~y~You've dropped {selectedItem.CustomName}.");

                return;
            }

            if (option == "Delete Item")
            {
                bool itemRemoved = backpackInventory.RemoveItem(selectedItem, selectedItem.Quantity);

                if (!itemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the item.");
                    return;
                }

                player.SendNotification(selectedItem.Quantity > 1
                    ? $"~y~You've deleted {selectedItem.CustomName} x{selectedItem.Quantity}."
                    : $"~y~You've deleted {selectedItem.CustomName}.");
            }

            if (option == "Give to player")
            {
                IEnumerable<IPlayer> playerList = Alt.Server.GetPlayers();
                float lastDistance = 3;
                IPlayer lastPlayer = null;
                Position playerPosition = player.Position;

                foreach (IPlayer target in playerList)
                {
                    if (target == player) continue;

                    if (target.FetchCharacter() == null) continue;

                    if (target.Dimension != player.Dimension) continue;

                    Position targetPosition = target.Position;

                    float distance = playerPosition.Distance(targetPosition);

                    if (distance < lastDistance)
                    {
                        lastDistance = distance;
                        lastPlayer = target;
                    }
                }

                if (lastPlayer == null)
                {
                    player.SendErrorNotification("There are no nearby players!");
                    return;
                }

                player.SetData("Backpack:GiveItemToPlayer", lastPlayer.GetPlayerId());

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Confirm")
                };

                NativeMenu menu = new NativeMenu("Backpack:ConfirmGiveToPlayer", "Confirm", $"Give to {lastPlayer.GetClass().Name}?", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }

            if (option == "Transfer to Vehicle")
            {
                IEnumerable<IVehicle> vehicles = Alt.Server.GetVehicles();

                float lastDistance = 5f;
                IVehicle lastVehicle = null;

                Position playerPosition = player.Position;

                foreach (IVehicle vehicle in vehicles)
                {
                    if (vehicle.FetchVehicleData() == null) continue;

                    if (vehicle.Dimension != player.Dimension) continue;

                    Position vehiclePosition = vehicle.Position;

                    float distance = playerPosition.Distance(vehiclePosition);

                    if (distance < lastDistance)
                    {
                        lastDistance = distance;
                        lastVehicle = vehicle;
                    }
                }

                if (lastVehicle == null)
                {
                    player.SendErrorNotification("Your not near a vehicle.");
                    return;
                }

                if (lastVehicle.LockState != VehicleLockState.Unlocked)
                {
                    player.SendErrorNotification("This vehicle is locked.");
                    return;
                }

                Inventory.Inventory vehicleInventory = lastVehicle.FetchInventory();

                if (vehicleInventory == null)
                {
                    player.SendErrorNotification("There was an error fetching vehicle inventory.");
                    return;
                }

                bool tryTransferItem = backpackInventory.TransferItem(vehicleInventory, selectedItem);

                if (!tryTransferItem)
                {
                    player.SendErrorNotification("There was an error moving the item to the vehicle.");
                    return;
                }

                player.SendInfoNotification($"You've placed {selectedItem.CustomName} into {lastVehicle.FetchVehicleData().Name}'s trunk.");
                player.SendEmoteMessage($"stores an item in {lastVehicle.FetchVehicleData().Name}'s trunk.");
                return;
            }
            else
            {
                player.SendErrorNotification("Feature not implemented!");
            }
        }

        public static void OnItemQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            
            Inventory.Inventory backpackInventory = player.FetchBackpackInventory();

            Inventory.Inventory playerInventory = player.FetchInventory();

            player.GetData("Backpack:SelectedItemIndex", out int itemIndex);
            player.GetData("backpack:QuantityChange", out int quantity);

            InventoryItem selectedItem = backpackInventory.GetInventory()[itemIndex];

            if (selectedItem == null)
            {
                player.SendErrorNotification("The selected backpack item doesn't exist!");
                return;
            }

            
            player.GetData("backpack:ItemOption", out string selectedOption);

            if (selectedOption == "Transfer to player")
            {
                
                bool playerTransfer = backpackInventory.TransferItem(playerInventory, selectedItem, quantity);
                if (!playerTransfer)
                {
                    player.SendErrorNotification("There was an error transferring the item.");
                    return;
                }
                player.DeleteData("backpack:QuantityChange");
                player.SendNotification($"~y~You've transferred {selectedItem.CustomName} back to you.");
                return;
            }

            if (option == "Drop Item")
            {
                bool itemRemoved = backpackInventory.RemoveItem(selectedItem, quantity);

                if (!itemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the item.");
                    return;
                }

                selectedItem.Quantity = quantity;

                DroppedItems.CreateDroppedItem(selectedItem, player.Position);

                player.SendEmoteMessage($"drops an item on the ground from their backpack.");

                player.SendInfoNotification($"You've dropped {selectedItem.CustomName} x{quantity}.");
                player.DeleteData("backpack:QuantityChange");
                return;
            }

            if (option == "Delete Item")
            {
                bool itemRemoved = backpackInventory.RemoveItem(selectedItem, quantity);

                if (!itemRemoved)
                {
                    player.SendErrorNotification("There was an error removing the item.");
                    return;
                }

                player.SendInfoNotification($"You've deleted {selectedItem.CustomName} x{quantity} from your inventory.");
                player.DeleteData("backpack:QuantityChange");
                return;
            }

            if (option == "Give to player")
            {
                IEnumerable<IPlayer> playerList = Alt.Server.GetPlayers();
                float lastDistance = 3;
                IPlayer lastPlayer = null;
                Position playerPosition = player.Position;

                foreach (IPlayer target in playerList)
                {
                    if (target == player) continue;

                    if (target.FetchCharacter() == null) continue;

                    if (target.Dimension != player.Dimension) continue;

                    Position targetPosition = target.Position;

                    float distance = playerPosition.Distance(targetPosition);

                    if (distance < lastDistance)
                    {
                        lastDistance = distance;
                        lastPlayer = target;
                    }
                }

                if (lastPlayer == null)
                {
                    player.SendErrorNotification("There are no nearby players!");
                    return;
                }

                player.SetData("Backpack:GiveItemToPlayer", lastPlayer.GetPlayerId());

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Confirm")
                };

                NativeMenu menu = new NativeMenu("Backpack:ConfirmGiveToPlayer", "Confirm", $"Give to {lastPlayer.GetClass().Name}?", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }

            if (option == "Transfer to Vehicle")
            {
                IEnumerable<IVehicle> vehicles = Alt.Server.GetVehicles();

                float lastDistance = 5f;
                IVehicle lastVehicle = null;

                Position playerPosition = player.Position;

                foreach (IVehicle vehicle in vehicles)
                {
                    if (vehicle.FetchVehicleData() == null) continue;

                    if (vehicle.Dimension != player.Dimension) continue;

                    Position vehiclePosition = vehicle.Position;

                    float distance = playerPosition.Distance(vehiclePosition);

                    if (distance < lastDistance)
                    {
                        lastDistance = distance;
                        lastVehicle = vehicle;
                    }
                }

                if (lastVehicle == null)
                {
                    player.SendErrorNotification("Your not near a vehicle.");
                    return;
                }

                if (lastVehicle.LockState != VehicleLockState.Unlocked)
                {
                    player.SendErrorNotification("This vehicle is locked.");
                    return;
                }

                Inventory.Inventory vehicleInventory = lastVehicle.FetchInventory();

                if (vehicleInventory == null)
                {
                    player.SendErrorNotification("There was an error fetching vehicle inventory.");
                    return;
                }

                bool tryTransferItem = backpackInventory.TransferItem(vehicleInventory, selectedItem, quantity);

                if (!tryTransferItem)
                {
                    player.SendErrorNotification("There was an error moving the item to the vehicle.");
                    return;
                }

                player.SendInfoNotification($"You've placed {selectedItem.CustomName} into {lastVehicle.FetchVehicleData().Name}'s trunk.");
                player.SendEmoteMessage($"stores an item in {lastVehicle.FetchVehicleData().Name}'s trunk.");
                return;
            }
            
            
        }

        public static void ItemQuantityChange(IPlayer player, string menuItemText, string listText)
        {   
            bool tryParse = int.TryParse(listText, out int quantity);

            if (!tryParse) return;
            
            player.SetData("backpack:QuantityChange", quantity);
        }
        
        public static void OnConfirmGiveToPlayer(IPlayer player, string option)
        {
            if (option == "Close") return;

            Inventory.Inventory backpackInventory = player.FetchBackpackInventory();

            player.GetData("Backpack:SelectedItemIndex", out int itemIndex);
            bool hasQuantityData = player.GetData("backpack:QuantityChange", out int quantity);

            InventoryItem selectedItem = backpackInventory.GetInventory()[itemIndex];

            if (selectedItem == null)
            {
                player.SendErrorNotification("The selected backpack item doesn't exist!");
                return;
            }

            player.GetData("Backpack:GiveItemToPlayer", out int targetId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == targetId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not available.");
                return;
            }

            
            string targetName = targetPlayer.GetClass().Name;

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            if (hasQuantityData)
            {
                bool tryQuantityTransfer = backpackInventory.TransferItem(targetInventory, selectedItem, quantity);

                if (!tryQuantityTransfer)
                {
                    targetInventory = targetPlayer.FetchBackpackInventory();

                    if (targetInventory == null)
                    {
                        player.SendErrorNotification("There was an error giving the item.");
                        return;
                    }

                    tryQuantityTransfer = backpackInventory.TransferItem(targetInventory, selectedItem, quantity);

                    if (!tryQuantityTransfer)
                    {
                        player.SendErrorNotification("There was an error giving the item.");
                        return;
                    }

                    player.SendInfoNotification($"You've given {targetName} {selectedItem.CustomName}");
                    targetPlayer.SendInfoNotification($"You've been given {selectedItem.CustomName} by {player.GetClass().Name}. This has been added to your backpack.");
                }
                else
                {
                    player.SendInfoNotification($"You've given {targetName} {selectedItem.CustomName}");
                    targetPlayer.SendInfoNotification($"You've been given {selectedItem.CustomName} by {player.GetClass().Name}.");
                }

                return;
            }
            
            bool tryTransfer = backpackInventory.TransferItem(targetInventory, selectedItem);

            if (!tryTransfer)
            {
                targetInventory = targetPlayer.FetchBackpackInventory();

                if (targetInventory == null)
                {
                    player.SendErrorNotification("There was an error giving the item.");
                    return;
                }

                tryTransfer = backpackInventory.TransferItem(targetInventory, selectedItem);

                if (!tryTransfer)
                {
                    player.SendErrorNotification("There was an error giving the item.");
                    return;
                }

                player.SendInfoNotification($"You've given {targetName} {selectedItem.CustomName}");
                targetPlayer.SendInfoNotification($"You've been given {selectedItem.CustomName} by {player.GetClass().Name}. This has been added to your backpack.");
            }
            else
            {
                player.SendInfoNotification($"You've given {targetName} {selectedItem.CustomName}");
                targetPlayer.SendInfoNotification($"You've been given {selectedItem.CustomName} by {player.GetClass().Name}.");
            }
        }
    }
}