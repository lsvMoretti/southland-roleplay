using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Extensions;
using Server.Inventory;

namespace Server.Property.Stores
{
    public class KeySmith
    {
        private static double _newPropertyKeyValue = 0;
        private static double _duplicatePropertyKeyValue = 0;
        private static double _newVehicleKeyValue = 0;
        private static double _duplicateVehicleKeyValue = 0;

        public static void ShowKeySmithMenu(IPlayer player, Models.Property property)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            bool anyProperties = Models.Property.FetchCharacterProperties(playerCharacter).Any();

            bool anyVehicles = Models.Vehicle.FetchCharacterVehicles(playerCharacter.Id).Any();

            if (anyProperties)
            {
                menuItems.Add(new NativeMenuItem("New Property Key", $"{_newPropertyKeyValue:C}"));

                menuItems.Add(new NativeMenuItem("Duplicate Property Key", $"{_duplicatePropertyKeyValue:C}"));
            }

            if (anyVehicles)
            {
                menuItems.Add(new NativeMenuItem("New Vehicle Key", $"{_newVehicleKeyValue:C}"));

                menuItems.Add(new NativeMenuItem("Duplicate Vehicle Key", $"{_duplicateVehicleKeyValue:C}"));
            }

            if (!anyProperties && !anyVehicles)
            {
                player.SendErrorNotification("You have no vehicles or properties.");
                return;
            }

            NativeMenu menu = new NativeMenu("store:keysmith:MainMenuSelect", property.BusinessName, "Select a key option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnKeySmithMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            int keyType = option switch
            {
                "New Property Key" => 1,
                "Duplicate Property Key" => 2,
                "New Vehicle Key" => 3,
                "Duplicate Vehicle Key" => 4,
                _ => 1
            };

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (keyType == 1 || keyType == 2)
            {
                // Properties
                List<Models.Property> playerProperties =
                    Models.Property.FetchCharacterProperties(player.FetchCharacter());

                foreach (Models.Property playerProperty in playerProperties)
                {
                    menuItems.Add(new NativeMenuItem(playerProperty.Address, $"Property Id: {playerProperty.Id}"));
                }
            }

            if (keyType == 3 || keyType == 4)
            {
                // Vehicles
                List<Models.Vehicle> playerVehicles =
                    Models.Vehicle.FetchCharacterVehicles(player.GetClass().CharacterId);

                foreach (Models.Vehicle playerVehicle in playerVehicles)
                {
                    menuItems.Add(new NativeMenuItem(playerVehicle.Name, $"Vehicle Id: {playerVehicle.Id}"));
                }
            }

            player.SetData("keySmith:KeyOption", keyType);

            NativeMenu menu = new NativeMenu("store:keysmith:OnSelectKeyItem", "Keys", "Select a key you wish to adjust", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnKeySmithSelectKeyItem(IPlayer player, string option, int index)
        {
            try
            {
                Console.WriteLine(option);

                if (option == "Close") return;

                player.GetData("keySmith:KeyOption", out int keyType);

                Console.WriteLine($"KeyType {keyType}");

                if (keyType == 1 || keyType == 2)
                {
                    // Properties

                    List<Models.Property> playerProperties =
                        Models.Property.FetchCharacterProperties(player.FetchCharacter());

                    Models.Property playerProperty = playerProperties[index];

                    using Context context = new Context();

                    Models.Property property = context.Property.Find(playerProperty.Id);

                    if (keyType == 1)
                    {
                        if (player.GetClass().Cash < _newPropertyKeyValue)
                        {
                            player.SendErrorNotification($"You don't have funds. You require {_newPropertyKeyValue:C}");
                            return;
                        }

                        // New Property Key
                        string newKey = Utility.GenerateRandomString(8);

                        Inventory.Inventory playerInventory = player.FetchInventory();

                        bool addedNewKey = playerInventory.AddItem(new InventoryItem("ITEM_PROPERTY_KEY", property.Address, newKey));

                        if (!addedNewKey)
                        {
                            player.SendErrorNotification("Your inventory is full!");
                            return;
                        }

                        property.Key = newKey;

                        context.SaveChanges();

                        

                        player.RemoveCash(_newPropertyKeyValue);

                        player.SendInfoNotification($"You have bought a new key for {property.Address}. This has cost you {_newPropertyKeyValue:C}.");

                        return;
                    }

                    if (keyType == 2)
                    {
                        // Duplicate Property Key
                        if (player.GetClass().Cash < _duplicatePropertyKeyValue)
                        {
                            player.SendErrorNotification($"You don't have funds. You require {_duplicatePropertyKeyValue:C}");
                            return;
                        }

                        Inventory.Inventory playerInventory = player.FetchInventory();

                        bool addedNewKey = playerInventory.AddItem(new InventoryItem("ITEM_PROPERTY_KEY", property.Address, property.Key));

                        if (!addedNewKey)
                        {
                            player.SendErrorNotification("Your inventory is full!");
                            return;
                        }

                        player.RemoveCash(_duplicatePropertyKeyValue);

                        player.SendInfoNotification($"You have bought another key for {property.Address}. This has cost you {_duplicatePropertyKeyValue:C}.");
                        return;
                    }
                }

                if (keyType == 3 || keyType == 4)
                {
                    // Vehicles
                    List<Models.Vehicle> playerVehicles =
                        Models.Vehicle.FetchCharacterVehicles(player.GetClass().CharacterId);

                    Models.Vehicle selectedVehicle = playerVehicles[index];

                    if (keyType == 3)
                    {
                        // new vehicle key

                        if (player.GetClass().Cash < _newVehicleKeyValue)
                        {
                            player.SendErrorNotification($"You don't have enough funds. You require {_newVehicleKeyValue:C}.");
                            return;
                        }

                        string newKey = Utility.GenerateRandomString(8);

                        using Context context = new Context();

                        Models.Vehicle vehicle = context.Vehicle.Find(selectedVehicle.Id);

                        Inventory.Inventory playerInventory = player.FetchInventory();

                        bool itemAdded = playerInventory.AddItem(new InventoryItem("ITEM_VEHICLE_KEY", selectedVehicle.Name, newKey));

                        if (!itemAdded)
                        {
                            player.SendErrorNotification("Your inventory is full.");
                            return;
                        }

                        vehicle.Keycode = newKey;
                        context.SaveChanges();
                        

                        player.RemoveCash(_newVehicleKeyValue);

                        player.SendInfoNotification($"You have bought a new vehicle key for {selectedVehicle.Name}. This has cost you {_newVehicleKeyValue:C}.");
                        return;
                    }

                    if (keyType == 4)
                    {
                        // Duplicate vehicle key

                        if (player.GetClass().Cash < _duplicateVehicleKeyValue)
                        {
                            player.SendErrorNotification($"You don't have enough funds. You require {_duplicateVehicleKeyValue:C}.");
                            return;
                        }

                        Inventory.Inventory playerInventory = player.FetchInventory();

                        bool itemAdded = playerInventory.AddItem(new InventoryItem("ITEM_VEHICLE_KEY", selectedVehicle.Name, selectedVehicle.Keycode));

                        if (!itemAdded)
                        {
                            player.SendErrorNotification("Your inventory is full.");
                            return;
                        }

                        player.RemoveCash(_duplicateVehicleKeyValue);

                        player.SendInfoNotification($"You have bought a duplicated key for {selectedVehicle.Name}. This has cost you {_duplicateVehicleKeyValue:C}.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}