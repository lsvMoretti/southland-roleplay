using System.Collections.Generic;
using System.ComponentModel;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Property.Stores
{
    public class GunStore
    {
        public static void ShowGunStoreMainMenu(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (string.IsNullOrEmpty(playerCharacter.LicensesHeld))
            {
                player.SendErrorNotification("You don't have any licenses.");
                return;
            }

            List<LicenseTypes> playerLicenseTypes =
                JsonConvert.DeserializeObject<List<LicenseTypes>>(playerCharacter.LicensesHeld);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (playerLicenseTypes.Contains(LicenseTypes.Pistol))
            {
                //                menuItems.Add(new NativeMenuItem("SNS Pistol"));
                menuItems.Add(new NativeMenuItem("Pistol"));
                menuItems.Add(new NativeMenuItem("Pistol MKII"));
                menuItems.Add(new NativeMenuItem("9mm Rounds", "One pack of 5 9mm Rounds"));
            }

            NativeMenu menu = new NativeMenu("gunstore:MainMenuSelect", "Gun Store", "Select an item you wish to purchase", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnGunStoreMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (option == "SNS Pistol")
            {
                double cost = 250;

                if (player.GetClass().Cash < cost)
                {
                    player.SendErrorNotification($"You don't have enough. You require {cost:C}.");
                    return;
                }

                InventoryItem newItem = new InventoryItem("ITEM_WEAPON_SNS", $"{option}");

                bool itemAdded = playerInventory.AddItem(newItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding this. Check you have space!");
                    return;
                }

                player.RemoveCash(cost);

                player.SendInfoNotification($"You have purchased a {option}. This has cost you {cost:C}.");

                Logging.AddToCharacterLog(player, $"has purchased 1x {option}.");

                return;
            }

            if (option == "Pistol")
            {
                double cost = 300;

                if (player.GetClass().Cash < cost)
                {
                    player.SendErrorNotification($"You don't have enough. You require {cost:C}.");
                    return;
                }

                WeaponInfo newWeaponInfo = new WeaponInfo(0, true, player.GetClass().Name);

                InventoryItem newItem = new InventoryItem("ITEM_WEAPON_PISTOL", $"{option}", JsonConvert.SerializeObject(newWeaponInfo));

                bool itemAdded = playerInventory.AddItem(newItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding this. Check you have space!");
                    return;
                }

                player.RemoveCash(cost);

                player.SendInfoNotification($"You have purchased a {option}. This has cost you {cost:C}.");

                Logging.AddToCharacterLog(player, $"has purchased 1x {option}.");

                return;
            }

            if (option == "Pistol MKII")
            {
                double cost = 300;

                if (player.GetClass().Cash < cost)
                {
                    player.SendErrorNotification($"You don't have enough. You require {cost:C}.");
                    return;
                }

                WeaponInfo newWeaponInfo = new WeaponInfo(0, true, player.GetClass().Name);

                InventoryItem newItem = new InventoryItem("ITEM_WEAPON_PISTOL_MK2", $"{option}", JsonConvert.SerializeObject(newWeaponInfo));

                bool itemAdded = playerInventory.AddItem(newItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding this. Check you have space!");
                    return;
                }

                player.RemoveCash(cost);

                player.SendInfoNotification($"You have purchased a {option}. This has cost you {cost:C}.");

                Logging.AddToCharacterLog(player, $"has purchased 1x {option}.");

                return;
            }

            if (option == "9mm Rounds")
            {
                double cost = 1;

                if (player.GetClass().Cash < cost)
                {
                    player.SendErrorNotification($"You don't have enough. You require {cost:C}.");
                    return;
                }

                InventoryItem newItem = new InventoryItem("ITEM_WEAPON_AMMO_9MM", $"{option}", 5.ToString());

                bool itemAdded = playerInventory.AddItem(newItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding this. Check you have space!");
                    return;
                }

                player.RemoveCash(cost);

                player.SendInfoNotification($"You have purchased a {option}. This has cost you {cost:C}.");

                Logging.AddToCharacterLog(player, $"has purchased 1x {option}.");

                return;
            }
        }
    }
}