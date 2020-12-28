using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;

namespace Server.Vehicle
{
    public class VehicleInventory
    {
        [Command("vinv", commandType: CommandType.Vehicle, description: "Inventory: Used to interact with a vehicle inventory")]
        public static void VehicleCommandVehicleInventory(IPlayer player)
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            IVehicle nearbyVehicle = VehicleHandler.FetchNearestVehicle(player);

            if (nearbyVehicle?.FetchVehicleData() == null)
            {
                player.SendErrorNotification("You are not nearby a vehicle.");
                return;
            }

            if (nearbyVehicle.FetchVehicleData().Impounded && !player.IsLeo(true))
            {
                player.SendErrorNotification("Vehicle is impounded.");
                return;
            }

            //Position trunkPosition = nearbyVehicle.GetTrunkPosition();

            player.GetTrunkPosition(nearbyVehicle, "trunkPos:VInventory");
        }

        public static void OnReturnTrunkPosition(IPlayer player, float position)
        {
            try
            {
                IVehicle nearbyVehicle = VehicleHandler.FetchNearestVehicle(player);
                
                if (nearbyVehicle?.FetchVehicleData() == null)
                {
                    player.SendErrorNotification("You are not nearby a vehicle.");
                    return;
                }

                Position trunkPosition = new Position(nearbyVehicle.Position.X, position, nearbyVehicle.Position.Z);

                float distance = trunkPosition.Distance(player.Position);

                if (distance > 4f)
                {
                    player.SendErrorNotification("You must be near the trunk");
                    return;
                }

                //if (nearbyVehicle.GetDoorState(VehicleDoor.Trunk) == VehicleDoorState.Closed)
                //{
                //    player.SendErrorMessage("The trunk is closed.");
                //    return;
                //}

                if (player.IsInVehicle)
                {
                    player.SendErrorNotification("You must be on foot!");
                    return;
                }

                if (nearbyVehicle.FetchVehicleData().Locked)
                {
                    player.SendErrorNotification("Vehicle is locked");
                    return;
                }

                Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();

                if (vehicleInventory == null)
                {
                    player.SendErrorNotification("An error occured.");
                    return;
                }

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Take Item"),
                    new NativeMenuItem("Place Item")
                };

                player.SetData("vehicle:inventory:interaction", nearbyVehicle.GetClass()?.Id);

                NativeMenu menu = new NativeMenu("vehicle:inventory:mainMenu", "Vehicle", $"Weight: {Math.Round(vehicleInventory.CurrentWeight, 2)}/{vehicleInventory.MaximumWeight}", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
            }
            catch (Exception e)
            {
                player.SendErrorNotification("An error occurred.");
                Console.WriteLine(e);
                return;
            }
        }

        public static void InventoryMenuMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("vehicle:inventory:interaction", out int vehicleId);

            IVehicle nearbyVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicleId);

            if (nearbyVehicle == null || !nearbyVehicle.Exists)
            {
                player.SendErrorNotification("Unable to find the vehicle");
                return;
            }

            if (option == "Take Item")
            {
                Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();

                List<InventoryItem> vehicleItems = vehicleInventory.GetInventory().OrderBy(x => x.CustomName).ToList();

                if (!vehicleItems.Any())
                {
                    player.SendErrorNotification("There are no items in this vehicle!");
                    return;
                }

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                int backPackId = player.FetchCharacter().BackpackId;

                foreach (var inventoryItem in vehicleItems)
                {
                    if (backPackId > 0)
                    {
                        if (inventoryItem.Id == "ITEM_BACKPACK" || inventoryItem.Id == "ITEM_DUFFELBAG") continue;
                    }
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
                    
                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName, inventoryItem.ItemInfo.Description));
                }

                NativeMenu menu = new NativeMenu("vehicle:inventory:takeItem:list", "Inventory",
                    nearbyVehicle.FetchVehicleData().Name, menuItems)
                { PassIndex = true };

                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }

            if (option == "Place Item")
            {
                Inventory.Inventory playerInventory = player.FetchInventory();

                List<InventoryItem> playerItems = playerInventory.GetInventory().OrderBy(x => x.CustomName).ToList();

                Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();

                int backPackCount = vehicleInventory.GetInventoryItems("ITEM_BACKPACK").Count;

                backPackCount += vehicleInventory.GetInventoryItems("ITEM_DUFFELBAG").Count;

                if (!playerItems.Any())
                {
                    player.SendErrorNotification("You don't have any items on you!");
                    return;
                }

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                foreach (InventoryItem inventoryItem in playerItems)
                {
                    if (inventoryItem.Id == "ITEM_BACKPACK" || inventoryItem.Id == "ITEM_DUFFELBAG")
                    {
                        if (backPackCount >= 2) continue;
                    }
                    
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
                    
                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName, inventoryItem.ItemInfo.Description));
                }

                NativeMenu menu = new NativeMenu("vehicle:inventory:placeItem:list", "Inventory",
                    nearbyVehicle.FetchVehicleData().Name, menuItems)
                { PassIndex = true };

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }
        }

        public static void InventoryMenuOnItemTake(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            player.GetData("vehicle:inventory:interaction", out int vehicleId);

            IVehicle nearbyVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicleId);

            if (nearbyVehicle == null || !nearbyVehicle.Exists)
            {
                player.SendErrorNotification("Unable to find the vehicle");
                return;
            }

            Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> vehicleItems = vehicleInventory.GetInventory().OrderBy(x => x.CustomName).ToList();

            InventoryItem selectedItem = vehicleItems[index];

            if (selectedItem == null)
            {
                player.SendErrorNotification("This item isn't in the trunk.");
                return;
            }

            if (selectedItem.Quantity > 1)
            {
                player.SetData("Vehicle:Inventory:TakeIndex", index);
                List<string> stringOptions = new List<string>();

                for (int i = 1; i <= selectedItem.Quantity; i++)
                {
                    stringOptions.Add(i.ToString());
                }

                List<NativeListItem> quantityMenuItems = new List<NativeListItem>()
                {
                    new NativeListItem("Quantity: ", stringOptions)
                };

                NativeMenu quantityMenu = new NativeMenu("vehicle:inventory:takeQuantity", "Inventory", "Select a Quantity")
                {
                    ListMenuItems = quantityMenuItems,
                    ListTrigger = "vehicle:inventory:takeQuantityChange",
                    PassIndex = true
                };

                player.SetData("vehicle:inventory:QuantityTake", 1);

                NativeUi.ShowNativeMenu(player, quantityMenu, true);

                return;
            }

            if (selectedItem.Id == "ITEM_BACKPACK" || selectedItem.Id == "ITEM_DUFFELBAG")
            {
                bool tryParse = int.TryParse(selectedItem.ItemValue, out int backPackId);

                if (!tryParse)
                {
                    player.SendErrorNotification("An error occurred getting the backpack data.");
                    return;
                }

                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                playerCharacter.BackpackId = backPackId;

                context.SaveChanges();
                
                player.LoadCharacterCustomization();
            }

            bool transferItem = vehicleInventory.TransferItem(playerInventory, selectedItem);

            if (!transferItem)
            {
                player.SendErrorNotification("An error occurred transferring the item.");
                return;
            }

            /*bool placeSuccess = playerInventory.AddItem(selectedItem);

            if (!placeSuccess)
            {
                player.SendErrorMessage("You don't have space on you!");
                return;
            }

            bool takeSuccess = vehicleInventory.RemoveItem(selectedItem, selectedItem.Quantity);

            if (!takeSuccess)
            {
                player.SendErrorMessage("An error occurred taking the item.");
                return;
            }*/

            player.SendInfoNotification($"You've taken {selectedItem.CustomName} from the vehicle trunk.");
            player.SendEmoteMessage("takes an item from the vehicle trunk.");

            Logging.AddToCharacterLog(player, $"has taken {selectedItem.CustomName} (Quantity: {selectedItem.Quantity}) (Item: {selectedItem.ItemInfo.Name}) " +
                                              $"from vehicle ID's {nearbyVehicle.FetchVehicleData().Id} (Inv: {nearbyVehicle.FetchVehicleData().InventoryId})'s trunk");
        }

        public static void InventoryMenuOnTakeQuantityChange(IPlayer player, string quantityString)
        {
            int quantity = Convert.ToInt32(quantityString);

            player.SetData("vehicle:inventory:QuantityTake", quantity);
        }

        public static void InventoryMenuOnTakeQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("vehicle:inventory:QuantityTake", out int quantity);

            player.GetData("Vehicle:Inventory:TakeIndex", out int takeIndex);

            player.GetData("vehicle:inventory:interaction", out int vehicleId);

            IVehicle nearbyVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicleId);

            if (nearbyVehicle == null || !nearbyVehicle.Exists)
            {
                player.SendErrorNotification("Unable to find the vehicle");
                return;
            }

            Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> vehicleItems = vehicleInventory.GetInventory().OrderBy(x => x.CustomName).ToList();

            InventoryItem selectedItem = vehicleItems[takeIndex];

            if (quantity > selectedItem.Quantity)
            {
                player.SendErrorNotification("An error occurred with the quantities.");
                return;
            }

            bool transferItem = vehicleInventory.TransferItem(playerInventory, selectedItem, quantity);

            if (!transferItem)
            {
                player.SendErrorNotification("An error occurred transferring the item.");
                return;
            }

            /*InventoryItem newItem = new InventoryItem(selectedItem.Id, selectedItem.CustomName, selectedItem.ItemValue, quantity);

            bool placeSuccess = playerInventory.AddItem(newItem);

            if (!placeSuccess)
            {
                player.SendErrorMessage("You don't have space on you!");
                return;
            }

            bool takeSuccess = vehicleInventory.RemoveItem(selectedItem, quantity);

            if (!takeSuccess)
            {
                player.SendErrorMessage("An error occurred taking the item.");
                return;
            }*/

            player.SendEmoteMessage("takes an item from the vehicle trunk.");

            player.SendInfoNotification($"You've taken x{quantity} of {selectedItem.CustomName} from the vehicle.");

            Logging.AddToCharacterLog(player, $"has taken x{quantity} of {selectedItem.CustomName} from the vehicle id {nearbyVehicle.GetClass().Id}");
        }

        public static void InventoryMenuOnItemPlace(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            player.GetData("vehicle:inventory:interaction", out int vehicleId);

            IVehicle nearbyVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicleId);

            if (nearbyVehicle == null || !nearbyVehicle.Exists)
            {
                player.SendErrorNotification("Unable to find the vehicle");
                return;
            }

            Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();
            Inventory.Inventory playerInventory = player.FetchInventory();

            InventoryItem selectedItem = playerInventory.GetInventory().OrderBy(x => x.CustomName).ToList()[index];

            if (selectedItem == null)
            {
                player.SendErrorNotification("You don't have this item on you.");
                return;
            }

            if (selectedItem.Quantity > 1)
            {
                player.SetData("Vehicle:Inventory:PlaceIndex", index);
                List<string> stringOptions = new List<string>();

                for (int i = 1; i <= selectedItem.Quantity; i++)
                {
                    stringOptions.Add(i.ToString());
                }

                List<NativeListItem> quantityMenuItems = new List<NativeListItem>()
                {
                    new NativeListItem("Quantity: ", stringOptions)
                };

                NativeMenu quantityMenu = new NativeMenu("vehicle:inventory:placeQuantity", "Inventory", "Select a Quantity")
                {
                    ListMenuItems = quantityMenuItems,
                    ListTrigger = "vehicle:inventory:placeQuantityChange",
                    PassIndex = true
                };

                player.SetData("vehicle:inventory:QuantityPlace", 1);

                NativeUi.ShowNativeMenu(player, quantityMenu, true);

                return;
            }

            bool transferSuccess = playerInventory.TransferItem(vehicleInventory, selectedItem);

            if (!transferSuccess)
            {
                player.SendErrorNotification("An error occurred transferring the item.");
                return;
            }

            if (selectedItem.Id == "ITEM_BACKPACK" || selectedItem.Id == "ITEM_DUFFELBAG")
            {
                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                playerCharacter.BackpackId = 0;

                context.SaveChanges();
                

                player.LoadCharacterCustomization();
            }

            /*bool placeSuccess = vehicleInventory.AddItem(selectedItem);

            if (!placeSuccess)
            {
                player.SendErrorMessage("Your not able to place this item in the trunk.");
                return;
            }

            bool takeSuccess = playerInventory.RemoveItem(selectedItem);

            if (!takeSuccess)
            {
                player.SendErrorMessage("An error occurred taking an item from you.");
                return;
            }*/

            player.SendInfoNotification($"You've placed {selectedItem.CustomName} into the vehicle trunk.");

            Logging.AddToCharacterLog(player, $"has placed {selectedItem.CustomName} (Quantity: {selectedItem.Quantity}) (Item: {selectedItem.ItemInfo.Name}) " +
                                              $"into vehicle ID's {nearbyVehicle.FetchVehicleData().Id} (Inv: {nearbyVehicle.FetchVehicleData().InventoryId})'s trunk");
        }

        public static void InventoryMenuOnPlaceQuantityChange(IPlayer player, string quantityString)
        {
            int quantity = Convert.ToInt32(quantityString);

            player.SetData("vehicle:inventory:QuantityPlace", quantity);
        }

        public static void InventoryMenuOnPlaceQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("vehicle:inventory:QuantityPlace", out int quantity);

            player.GetData("Vehicle:Inventory:PlaceIndex", out int placeIndex);

            player.GetData("vehicle:inventory:interaction", out int vehicleId);

            IVehicle nearbyVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicleId);

            if (nearbyVehicle == null || !nearbyVehicle.Exists)
            {
                player.SendErrorNotification("Unable to find the vehicle");
                return;
            }

            Inventory.Inventory vehicleInventory = nearbyVehicle.FetchInventory();
            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> playerItems = playerInventory.GetInventory().OrderBy(x => x.CustomName).ToList();

            InventoryItem selectedItem = playerItems[placeIndex];

            if (quantity > selectedItem.Quantity)
            {
                player.SendErrorNotification("An error occurred with the quantities.");
                return;
            }

            bool transferItem = playerInventory.TransferItem(vehicleInventory, selectedItem, quantity);

            if (!transferItem)
            {
                player.SendErrorNotification("An error occurred moving the items.");
                return;
            }

            player.SendEmoteMessage("places an item into the vehicle trunk.");
            player.SendInfoNotification($"You've placed x{quantity} of {selectedItem.CustomName} into the vehicle.");
            Logging.AddToCharacterLog(player, $"has placed x{quantity} of {selectedItem.CustomName} into the vehicle id {nearbyVehicle.GetClass().Id}");
        }
    }
}