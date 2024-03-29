﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Serilog;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;

namespace Server.Weapons
{
    public class WeaponCommands
    {
        [Command("weapons", commandType: CommandType.Character, description: "Shows your weapons that are in your inventory")]
        public static void Command_Weapons(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendLoginError();
                return;
            }

            List<InventoryItem> weaponItems = playerInventory.GetInventory()
                .Where(x => x.Id.Contains("WEAPON")).ToList();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (!string.IsNullOrEmpty(player.FetchCharacter()?.ActiveWeapon))
            {
                menuItems.Add(new NativeMenuItem("Clear Active Weapon"));
            }

            foreach (var weaponItem in weaponItems)
            {
                if (weaponItem.Quantity > 1)
                {
                    menuItems.Add(new NativeMenuItem(weaponItem.ItemInfo.Name, $"{weaponItem.ItemInfo.Description} x{weaponItem.Quantity}"));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(weaponItem.ItemInfo.Name, weaponItem.ItemInfo.Description));
                }
            }

            NativeMenu weaponMenu =
                new NativeMenu("WeaponManagementMainMenu", "Weapons", "Weapon & Ammo Management", menuItems)
                {
                    PassIndex = true
                };

            NativeUi.ShowNativeMenu(player, weaponMenu, true);

            player.SetData("WeaponItemMenu", JsonConvert.SerializeObject(weaponItems));
        }

        public static void EventWeaponManagementMainMenu(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            if (option == "Clear Active Weapon")
            {
                using Context context = new Context();

                Models.Character? playerCharacter =
                    context.Character.FirstOrDefault(x => x.Id == player.GetClass().CharacterId);

                if (playerCharacter == null) return;

                playerCharacter.ActiveWeapon = string.Empty;

                context.SaveChanges();

                player.SendInfoNotification("Your active weapon has been removed.");
                return;
            }

            bool hasWeaponItemMenuData = player.GetData("WeaponItemMenu", out string itemString);

            if (!hasWeaponItemMenuData)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            List<InventoryItem> weaponItems =
                JsonConvert.DeserializeObject<List<InventoryItem>>(itemString);

            if (!string.IsNullOrEmpty(player.FetchCharacter()?.ActiveWeapon))
            {
                index--;
            }

            InventoryItem selectedWeaponItem = weaponItems[index];

            player.DeleteData("WeaponItemMenu");

            player.SetData("WeaponManagementSelectedWeapon", JsonConvert.SerializeObject(selectedWeaponItem));

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (selectedWeaponItem.Id.Contains("AMMO"))
            {
                bool hasCurrentWeaponData = player.GetData("CURRENTWEAPON", out string weaponItemId);

                if (hasCurrentWeaponData && !string.IsNullOrEmpty(weaponItemId))
                {
                    bool correctAmmo = false;

                    switch (weaponItemId)
                    {
                        case "ITEM_WEAPON_PISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_PISTOL_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_APPISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_COMBATPISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_PISTOL50":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_50AE")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SNS":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_45ACP")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SNS_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_45ACP")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_HEAVYPISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_762")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_VINTAGEPISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_45ACP")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_REVOLVER":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_320")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_REVOLVER_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_320")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_MICROSMG":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SMG":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SMG_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_ASSAULTSMG":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_COMBATPDW":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_MACHINEPISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_MINISMG":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_PUMPSHOTGUN":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_12G")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_PUMPSHOTGUN_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_12G")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SAWNOFFSHOTGUN":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_12G")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_ASSAULTSHOTGUN":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_12G")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_BULLPUPSHOTGUN":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_12G")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_HEAVYSHOTGUN":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_12G")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_ASSAULTRIFLE":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_762")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_ASSAULTRIFLE_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_762")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_CARBINE":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_CARBINE_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_ADVANCEDRIFLE":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SPECIALCARBINE":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SPECIALCARBINE_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_BULLPUPRIFLE":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_BULLPUPRIFLE_MK2":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_556")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_GUSENBERG":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_45ACP")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_SNIPERRIFLE":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_338")
                            {
                                correctAmmo = true;
                            }

                            break;

                        case "ITEM_WEAPON_HEAVYSNIPER":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_50")
                            {
                                correctAmmo = true;
                            }
                            break;

                        case "ITEM_POLICE_WEAPON_PISTOL":
                            if (selectedWeaponItem.Id == "ITEM_WEAPON_AMMO_9MM")
                            {
                                correctAmmo = true;
                            }

                            break;

                        default:
                            correctAmmo = false;
                            break;
                    }

                    if (correctAmmo)
                    {
                        menuItems.Add(new NativeMenuItem("Combine Ammo"));
                    }
                }
            }
            else
            {
                menuItems.Add(new NativeMenuItem("Equip Weapon"));
                menuItems.Add(new NativeMenuItem("Set Active Weapon"));
            }

            NativeMenu menu = new NativeMenu("WeaponMenuWeaponManagement", "Weapons", "Weapon & Ammo Management", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void EventWeaponMenuWeaponManagement(IPlayer player, string option)
        {
            if (option == "Close") return;

            bool hasSelectedWeaponData =
                player.GetData("WeaponManagementSelectedWeapon", out string selectedWeaponData);

            if (!hasSelectedWeaponData)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            InventoryItem selectedWeaponItem =
                JsonConvert.DeserializeObject<InventoryItem>(selectedWeaponData);

            if (option == "Combine Ammo")
            {
                List<string> bulletCount = new List<string>();

                bool tryAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int ammoCount);

                if (!tryAmmoParse)
                {
                    player.SendErrorNotification("An error occurred parsing ammo data.");
                    return;
                }

                #region Max Ammo

                bool hasCurrentWeaponData = player.GetData("CURRENTWEAPON", out string weaponItemId);

                if (hasCurrentWeaponData)
                {
                    if (weaponItemId == "ITEM_WEAPON_PISTOL")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }

                    if (weaponItemId == "ITEM_WEAPON_PISTOL_MK2")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }

                    if (weaponItemId == "ITEM_WEAPON_APPISTOL")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }

                    if (weaponItemId == "ITEM_WEAPON_COMBATPISTOL")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }

                    if (weaponItemId == "ITEM_WEAPON_PISTOL50")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }

                    if (weaponItemId == "ITEM_WEAPON_SNS")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                    if (weaponItemId == "ITEM_WEAPON_SNS_MK2")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                    if (weaponItemId == "ITEM_WEAPON_HEAVYPISTOL")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                    if (weaponItemId == "ITEM_WEAPON_SNS")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                    if (weaponItemId == "ITEM_WEAPON_VINTAGEPISTOL")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                    if (weaponItemId == "ITEM_WEAPON_REVOLVER")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                    if (weaponItemId == "ITEM_WEAPON_REVOLVER_MK2")
                    {
                        if (ammoCount > 30)
                        {
                            ammoCount = 30;
                        }
                    }
                }

                #endregion
                
                for (int i = 1; i <= ammoCount; i++)
                {
                    bulletCount.Add(i.ToString());
                }

                NativeListItem bulletListItem = new NativeListItem
                {
                    Title = "Combine Ammo",
                    StringList = bulletCount
                };

                player.SetData("UpdatedBulletCount", 1);

                List<NativeListItem> listItems = new List<NativeListItem>
                {
                    bulletListItem
                };

                NativeMenu menu = new NativeMenu("WeaponManagementCombineAmmo", "Ammo", "Weapon & Ammo Management") { ListMenuItems = listItems, ListTrigger = "WeaponManagementCombineAmmoListTrigger" };

                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }

            if (option == "Equip Weapon")
            {
                EquipItem.EquipItemAttribute(player, selectedWeaponItem);
                return;
            }

            if (option == "Set Active Weapon")
            {
                bool hasCurrentWeaponData = player.GetData("CURRENTWEAPON", out string currentWeapon);

                if (hasCurrentWeaponData && !string.IsNullOrEmpty(currentWeapon))
                {
                    player.SendErrorNotification("Please /unequip your current weapon.");
                    return;
                }

                using Context context = new Context();

                Models.Character? playerCharacter =
                    context.Character.FirstOrDefault(x => x.Id == player.GetClass().CharacterId);

                if (playerCharacter == null)
                {
                    player.SendErrorNotification("Unable to find your character.");
                    return;
                }

                playerCharacter.ActiveWeapon = selectedWeaponData;

                player.SendInfoNotification($"You've set your {selectedWeaponItem.CustomName} as your active weapon!");

                context.SaveChanges();
                return;
            }
        }

        public static void EventWeaponManagementCombineAmmoListTrigger(IPlayer player, string bulletCountString)
        {
            int bulletCount = Convert.ToInt32(bulletCountString);

            player.SetData("UpdatedBulletCount", bulletCount);
        }

        public static void EventWeaponManagementCombineAmmo(IPlayer player, string option)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            player.GetData("WeaponManagementSelectedWeapon", out string weaponData);

            InventoryItem weaponItem =
                JsonConvert.DeserializeObject<InventoryItem>(weaponData);

            InventoryItem selectedWeaponItem = playerInventory.GetItem(weaponItem.Id, weaponItem.ItemValue);

            InventoryItem selectedInventoryItem = playerInventory.GetInventory().FirstOrDefault(x => x.Id == selectedWeaponItem.Id && x.CustomName == selectedWeaponItem.CustomName && x.Quantity == selectedWeaponItem.Quantity);

            bool hasBulletCountData = player.GetData("UpdatedBulletCount", out int bulletCount);

            if (!hasBulletCountData)
            {
                bulletCount = 1;
            }

            bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);

            if (!weaponAmmoParse)
            {
                player.SendErrorNotification("An error occurred parsing the ammo when combining.");
                return;
            }

            Console.WriteLine($"ItemAmmoCount: {itemAmmoCount}, BulletCount: {bulletCount}");

            if (itemAmmoCount < bulletCount)
            {
                player.SendErrorNotification("You don't have these many bullets");
                return;
            }

            int bulletsLeft = itemAmmoCount - bulletCount;

            bool tryRemove = playerInventory.RemoveItem(selectedWeaponItem);

            if (bulletsLeft > 0)
            {
                selectedWeaponItem.ItemValue = bulletsLeft.ToString();
                selectedWeaponItem.Quantity = 1;
                bool addItem = playerInventory.AddItem(selectedWeaponItem);
            }

            bool hasUnEquipWeaponData = player.GetData("UnEquipWeapon", out string unequipWeapon);

            if (hasUnEquipWeaponData && !string.IsNullOrEmpty(unequipWeapon))
            {
                UnEquipWeapon(player, bulletCount);
                return;
            }

            player.Emit("fetchCurrentAmmo", "weapon:combineAmmo:ReturnCurrentAmmo", player.CurrentWeapon);
        }

        public static void ReturnCurrentAmmo(IPlayer player, int currentAmmo)
        {
            bool hasBulletCountData = player.GetData("UpdatedBulletCount", out int bulletCount);

            if (!hasBulletCountData)
            {
                bulletCount = 1;
            }

            int totalBullets = currentAmmo + bulletCount;
            
            bool hasCurrentWeaponData = player.GetData("CURRENTWEAPON", out string weaponItemId);

            if (hasCurrentWeaponData && totalBullets > 30)
            {
                Inventory.Inventory? playerInventory = player.FetchInventory();
                    
                player.GetData("WeaponManagementSelectedWeapon", out string weaponData);

                InventoryItem weaponItem = 
                    JsonConvert.DeserializeObject<InventoryItem>(weaponData);

                InventoryItem? selectedWeaponItem = playerInventory?.GetItem(weaponItem.Id, weaponItem.ItemValue);

                    
                if (weaponItemId == "ITEM_WEAPON_PISTOL")
                {
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }

                if (weaponItemId == "ITEM_WEAPON_PISTOL_MK2")
                {
                        
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }

                if (weaponItemId == "ITEM_WEAPON_APPISTOL")
                {
                        
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }

                if (weaponItemId == "ITEM_WEAPON_COMBATPISTOL")
                {
                        
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }

                if (weaponItemId == "ITEM_WEAPON_PISTOL50")
                {
                        
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }

                if (weaponItemId == "ITEM_WEAPON_SNS")
                {
                        
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                if (weaponItemId == "ITEM_WEAPON_SNS_MK2")
                {
                        
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                if (weaponItemId == "ITEM_WEAPON_HEAVYPISTOL")
                {
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                if (weaponItemId == "ITEM_WEAPON_SNS")
                {
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                if (weaponItemId == "ITEM_WEAPON_VINTAGEPISTOL")
                {
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                if (weaponItemId == "ITEM_WEAPON_REVOLVER")
                {
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                if (weaponItemId == "ITEM_WEAPON_REVOLVER_MK2")
                {
                    if (selectedWeaponItem == null)
                    {
                        selectedWeaponItem = new InventoryItem(weaponItem.Id, weaponItem.CustomName);
                        selectedWeaponItem.ItemValue = "0";
                    }
                    else
                    {
                        playerInventory.RemoveItem(selectedWeaponItem);
                    }
                    bool weaponAmmoParse = int.TryParse(selectedWeaponItem.ItemValue, out int itemAmmoCount);
                    
                    int bullets = itemAmmoCount + bulletCount;

                    selectedWeaponItem.ItemValue = bullets.ToString();

                    playerInventory.AddItem(selectedWeaponItem);
                    player.SendErrorNotification("You can only put 30 bullets in max.");
                    return;
                }
                }
            

            player.GetData("CurrentWeaponHash", out uint weaponHash);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            InventoryItem currentWeapon = JsonConvert.DeserializeObject<InventoryItem>(playerCharacter.CurrentWeapon);

            WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(currentWeapon.ItemValue);

            
            weaponInfo.AmmoCount = totalBullets;

            currentWeapon.ItemValue = JsonConvert.SerializeObject(weaponInfo);

            playerCharacter.CurrentWeapon = JsonConvert.SerializeObject(currentWeapon);

            context.SaveChanges();

            player.RemoveAllWeapons();

            //player.TriggerEvent("RemovedWeapon");

            player.GiveWeapon(weaponHash, totalBullets, true);
        }

        [Command("unequip", commandType: CommandType.Character, description: "Unequips your current weapon")]
        public static void CommandUnEquip(IPlayer player)
        {
            bool hasCurrentWeaponData = player.GetData("CurrentWeaponHash", out uint weaponHash);

            if (!hasCurrentWeaponData || weaponHash == 0)
            {
                player.SendErrorNotification("You don't have a weapon equipped.");
                return;
            }

            if (weaponHash != 0 && weaponHash != player.CurrentWeapon)
            {
                player.CurrentWeapon = weaponHash;
            }

            InventoryItem weaponItem =
                JsonConvert.DeserializeObject<InventoryItem>(player.FetchCharacter().CurrentWeapon);

            WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(weaponItem.ItemValue);

            player.GetData("DeathWeapon", out bool isDeathWeapon);

            if (isDeathWeapon)
            {
                using Context unequipContext = new Context();

                var pC = unequipContext.Character.Find(player.GetClass().CharacterId);

                if (pC == null)
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                player.SetData("UnEquipWeapon", pC.CurrentWeapon);
                pC.CurrentWeapon = string.Empty;

                unequipContext.SaveChanges();

                UnEquipWeapon(player, weaponInfo.AmmoCount);

                player.DeleteData("DeathWeapon");

                return;
            }

            if (player.CurrentWeapon == (uint)WeaponModel.Fist)
            {
                player.SendErrorNotification("You must have a weapon in hand!");

                Logging.AddToCharacterLog(player, $"has tried to unequip a weapon. Last Weapon Ammo Count: {weaponInfo.AmmoCount}. Weapon: {weaponItem.CustomName}.");
                return;
            }

            using Context context = new Context();

            var playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            player.SetData("UnEquipWeapon", playerCharacter.CurrentWeapon);
            playerCharacter.CurrentWeapon = string.Empty;

            player.SetData("Weapon:CurrentBulletCount", 0);
            context.SaveChanges();

            //UnEquipWeapon(player, ammo);

            player.Emit("fetchCurrentAmmo", "weapon:unequip:ReturnAmmo", player.CurrentWeapon);
        }

        public static void UnEquipWeapon(IPlayer player, int bulletCount)
        {
            try
            {
                player.GetData("UnEquipWeapon", out string itemData);

                InventoryItem weaponItem = JsonConvert.DeserializeObject<InventoryItem>(itemData);

                if (weaponItem == null)
                {
                    player.SendErrorNotification("An error occurred fetching the item data.");
                    return;
                }

                WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(weaponItem.ItemValue);

                /*
                if (hasAmmoData && lastAmmoCount > 0)
                {
                    bulletCount = lastAmmoCount;
                }
                */
                weaponInfo.AmmoCount = bulletCount;

                weaponItem.ItemValue = JsonConvert.SerializeObject(weaponInfo);

                Models.Character? playerCharacter = player.FetchCharacter();
                Inventory.Inventory playerInventory = player.FetchInventory();

                if (!playerInventory.AddItem(weaponItem))
                {
                    player.SendErrorMessage("Not enough space!");
                    return;
                }

                if (!string.IsNullOrEmpty(playerCharacter?.ActiveWeapon))
                {
                    using Context context = new Context();
                    Models.Character? character = context.Character.FirstOrDefault(x => x.Id == playerCharacter.Id);
                    if (character == null) return;

                    character.ActiveWeapon = JsonConvert.SerializeObject(weaponItem);
                    context.SaveChanges();
                }

                player.DeleteData("UnEquipWeapon");

                player.GetData("CURRENTWEAPON", out string weaponItemId);

                player.DeleteData("CURRENTWEAPON");
                player.DeleteData("CurrentWeaponHash");

                #region Melee

                if (weaponItemId == "ITEM_WEAPON_MELEE_BOTTLE")
                {
                    player.RemoveWeapon(WeaponModel.BrokenBottle);
                    player.SendWeaponMessage("Broken Bottle has been holstered.");
                }
                if (weaponItemId == "ITEM_WEAPON_MELEE_BAT")
                {
                    player.RemoveWeapon(WeaponModel.BaseballBat);
                    player.SendWeaponMessage("Baseball Bat has been holstered.");
                }
                if (weaponItemId == "ITEM_WEAPON_MELEE_CROWBAR")
                {
                    player.RemoveWeapon(WeaponModel.Crowbar);
                    player.SendWeaponMessage("Crowbar has been holstered.");
                }

                if (weaponItemId == "ITEM_WEAPON_MELEE_HAMMER")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Hammer has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                #endregion Melee

                if (weaponItemId == "ITEM_WEAPON_PISTOL")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_PISTOL_MK2")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Pistol MK2 has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_COMBATPISTOL")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Combat Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_APPISTOL")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("AP Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_PISTOL50")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Pistol 50 has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_SNS")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("SNS Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_SNS_MK2")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("SNS Pistol MK2 has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_HEAVYPISTOL")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Heavy Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_VINTAGEPISTOL")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Vintage Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_REVOLVER")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Revolver has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_REVOLVER_MK2")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Revolver MK2 has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_MICROSMG")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Micro SMG has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_SMG")
                {
                    player.RemoveWeapon(WeaponModel.SMG);
                    player.SendWeaponMessage("SMG has been holstered.");
                }

                if (weaponItemId == "ITEM_WEAPON_SMG_MK2")
                {
                    player.RemoveWeapon(WeaponModel.SMGMkII);
                    player.SendWeaponMessage("SMG MK2 has been holstered.");
                }

                if (weaponItemId == "ITEM_WEAPON_ASSAULTSMG")
                {
                    player.RemoveWeapon(WeaponModel.AssaultSMG);
                    player.SendWeaponMessage("Assault SMG has been holstered.");
                }

                if (weaponItemId == "ITEM_WEAPON_COMBATPDW")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Combat PDW has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_MACHINEPISTOL")
                {
                    player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);
                    player.SendWeaponMessage("Machine Pistol has been holstered.");
                    Timer timer = new Timer(1700)
                    {
                        Enabled = true,
                    };
                    timer.Elapsed += (s, e) =>
                    {
                        timer.Stop();
                        player.RemoveAllWeapons();
                        timer.Dispose();
                    };
                }

                if (weaponItemId == "ITEM_WEAPON_MINISMG")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.MiniSMG);
                }

                if (weaponItemId == "ITEM_WEAPON_PUMPSHOTGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.PumpShotgun);
                }

                if (weaponItemId == "ITEM_WEAPON_PUMPSHOTGUN_MK2")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.PumpShotgunMkII);
                }

                if (weaponItemId == "ITEM_WEAPON_SAWNOFFSHOTGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.SawedOffShotgun);
                }

                if (weaponItemId == "ITEM_WEAPON_ASSAULTSHOTGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.AssaultShotgun);
                }

                if (weaponItemId == "ITEM_WEAPON_BULLPUPSHOTGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.BullpupShotgun);
                }

                if (weaponItemId == "ITEM_WEAPON_HEAVYSHOTGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.HeavyShotgun);
                }

                if (weaponItemId == "ITEM_WEAPON_ASSAULTRIFLE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.AssaultRifle);
                }

                if (weaponItemId == "ITEM_WEAPON_ASSAULTRIFLE_MK2")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.AssaultRifleMkII);
                }

                if (weaponItemId == "ITEM_WEAPON_CARBINE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.CarbineRifle);
                }

                if (weaponItemId == "ITEM_WEAPON_CARBINE_MK2")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.CarbineRifleMkII);
                }

                if (weaponItemId == "ITEM_WEAPON_ADVANCEDRIFLE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.AdvancedRifle);
                }

                if (weaponItemId == "ITEM_WEAPON_SPECIALCARBINE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.SpecialCarbine);
                }

                if (weaponItemId == "ITEM_WEAPON_SPECIALCARBINE_MK2")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.SpecialCarbineMkII);
                }

                if (weaponItemId == "ITEM_WEAPON_BULLPUPRIFLE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.BullpupRifle);
                }

                if (weaponItemId == "ITEM_WEAPON_BULLPUPRIFLE_MK2")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.BullpupRifleMkII);
                }

                if (weaponItemId == "ITEM_WEAPON_GUSENBERG")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.GusenbergSweeper);
                }

                if (weaponItemId == "ITEM_WEAPON_SNIPERRIFLE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.SniperRifle);
                }

                if (weaponItemId == "ITEM_WEAPON_HEAVYSNIPER")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.HeavySniper);
                }
                if (weaponItemId == "ITEM_WEAPON_GRENADE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Grenade);
                }
                if (weaponItemId == "ITEM_WEAPON_BZGAS")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.BZGas);
                }
                if (weaponItemId == "ITEM_WEAPON_MOLOTOV")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.MolotovCocktail);
                }
                if (weaponItemId == "ITEM_WEAPON_PROXMINE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.ProximityMines);
                }

                if (weaponItemId == "ITEM_WEAPON_BASEBALL")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Baseball);
                }

                if (weaponItemId == "ITEM_WEAPON_SMOKEGRENADE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.TearGas);
                }
                if (weaponItemId == "ITEM_WEAPON_KNIFE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Knife);
                }
                if (weaponItemId == "ITEM_WEAPON_MACHETE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Machete);
                }
                if (weaponItemId == "ITEM_WEAPON_SWITCHBLADE")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Switchblade);
                }

                #region Police Weapons

                if (weaponItemId == "ITEM_POLICE_WEAPON_STUNGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.StunGun);
                }

                if (weaponItemId == "ITEM_POLICE_WEAPON_PISTOL")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.CombatPistol);
                }

                if (weaponItemId == "ITEM_POLICE_WEAPON_NIGHTSTICK")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Nightstick);
                }

                if (weaponItemId == "ITEM_POLICE_WEAPON_FLASHLIGHT")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Flashlight);
                }
                if (weaponItemId == "ITEM_POLICE_WEAPON_SHOTGUN")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.PumpShotgunMkII);
                }
                if (weaponItemId == "ITEM_POLICE_WEAPON_AR")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.CarbineRifleMkII);
                }

                #endregion Police Weapons

                #region Fire Department Weapons

                if (weaponItemId == "ITEM_FIRE_WEAPON_FLASHLIGHT")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Flashlight);
                }

                if (weaponItemId == "ITEM_FIRE_WEAPON_HATCHET")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Hatchet);
                }

                if (weaponItemId == "ITEM_FIRE_WEAPON_CROWBAR")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.Crowbar);
                }

                if (weaponItemId == "ITEM_FIRE_WEAPON_FIRE_EXTINGUISHER")
                {
                    player.SendWeaponMessage($"{weaponItem.GetName(false)} has been holstered.");
                    player.RemoveWeapon(WeaponModel.FireExtinguisher);
                }

                #endregion Fire Department Weapons
            }
            catch (Exception e)
            {
                player.SendErrorNotification("An error occurred.");
                Console.WriteLine(e);
                return;
            }
        }
    }
}