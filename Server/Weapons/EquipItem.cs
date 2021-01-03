using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Inventory;

namespace Server.Weapons
{
    public class EquipItem
    {
        public static void EquipItemAttribute(IPlayer player, InventoryItem invItem)
        {
            bool hasCurrentWeaponData = player.GetData("CURRENTWEAPON", out string currentWeapon);

            if (hasCurrentWeaponData && !string.IsNullOrEmpty(currentWeapon))
            {
                player.SendErrorNotification("You can only have one weapon at a time.");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            InventoryItem selectedInventoryItem = playerInventory.GetInventory().FirstOrDefault(x => x.Id == invItem.Id && x.CustomName == invItem.CustomName && x.Quantity == invItem.Quantity && x.ItemValue == invItem.ItemValue);

            if (selectedInventoryItem == null)
            {
                player.SendErrorNotification("Couldn't find this item in your Inventory!");
                return;
            }

            using (Context context = new Context())
            {
                var playerCharacter = context.Character.Find(player.FetchCharacterId());

                if (playerCharacter == null)
                {
                    player.SendErrorNotification($"An error occurred.");
                    return;
                }

                WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(selectedInventoryItem.ItemValue);

                List<string> lastPersons = weaponInfo.LastPerson;

                if (lastPersons.Count > 5)
                {
                    if (lastPersons.LastOrDefault() != playerCharacter.Name)
                    {
                        lastPersons.RemoveAt(0);
                    }
                }
                lastPersons.Add(playerCharacter.Name);

                selectedInventoryItem.ItemValue = weaponInfo.ToString();

                playerCharacter.CurrentWeapon = JsonConvert.SerializeObject(selectedInventoryItem);

                context.SaveChanges();
            }

            bool success = playerInventory.RemoveItem(selectedInventoryItem);

            if (success)
            {
                WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(selectedInventoryItem.ItemValue);

                EquipWeapon(player, playerInventory, selectedInventoryItem, weaponInfo.AmmoCount);
                return;
            }
            player.SendErrorNotification("An error occurred.");
        }

        public static void EquipWeapon(IPlayer player, Inventory.Inventory playerInventory,
            InventoryItem selectedInventoryItem, int currentBulletCount)
        {
            player.RemoveAllWeapons();

            #region Melee

            if (selectedInventoryItem.Id == "ITEM_WEAPON_MELEE_BOTTLE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.BrokenBottle, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Broken Bottle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.BrokenBottle);
                player.Emit("WeaponEquipped", (uint)WeaponModel.BrokenBottle);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_MELEE_BAT")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.BaseballBat, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Baseball Bat");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.BaseballBat);
                player.Emit("WeaponEquipped", (uint)WeaponModel.BaseballBat);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_MELEE_CROWBAR")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Crowbar, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Crowbar");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Crowbar);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Crowbar);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_MELEE_HAMMER")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Hammer, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Hammer");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Hammer);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Hammer);
                return;
            }

            #endregion Melee

            if (selectedInventoryItem.Id == "ITEM_WEAPON_PISTOL")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Pistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Pistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Pistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_PISTOL_MK2")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.PistolMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Pistol MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.PistolMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.PistolMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_COMBATPISTOL")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.CombatPistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Combat Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.CombatPistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.CombatPistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_APPISTOL")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.APPistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Auto Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.APPistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.APPistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_PISTOL50")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Pistol50, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Pistol 50");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Pistol50);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Pistol50);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SNS")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SNSPistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a SNS Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SNSPistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SNSPistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SNS_MK2")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SNSPistolMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a SNS Pistol MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SNSPistolMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SNSPistolMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_HEAVYPISTOL")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.HeavyPistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Heavy Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.HeavyPistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.HeavyPistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_VINTAGEPISTOL")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.VintagePistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Vintage Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.VintagePistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.VintagePistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_REVOLVER")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.HeavyRevolver, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Revolver");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.HeavyRevolver);
                player.Emit("WeaponEquipped", (uint)WeaponModel.HeavyRevolver);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_REVOLVER_MK2")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.HeavyRevolverMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Revolver MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.HeavyRevolverMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.HeavyRevolverMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_MICROSMG")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.MicroSMG, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Micro SMG");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.MicroSMG);
                player.Emit("WeaponEquipped", (uint)WeaponModel.MicroSMG);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SMG")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SMG, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a SMG");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SMG);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SMG);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SMG_MK2")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SMGMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a SMG MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SMGMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SMGMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_ASSAULTSMG")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.AssaultSMG, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Assault SMG");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.AssaultSMG);
                player.Emit("WeaponEquipped", (uint)WeaponModel.AssaultSMG);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_COMBATPDW")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.CombatPDW, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Combat PDW");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.CombatPDW);
                player.Emit("WeaponEquipped", (uint)WeaponModel.CombatPDW);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_MACHINEPISTOL")
            {
                player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.MachinePistol, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Machine Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.MachinePistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.MachinePistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_MINISMG")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.MiniSMG, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Mini SMG");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.MiniSMG);
                player.Emit("WeaponEquipped", (uint)WeaponModel.MiniSMG);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_PUMPSHOTGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.PumpShotgun, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Pump Shotgun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.PumpShotgun);
                player.Emit("WeaponEquipped", (uint)WeaponModel.PumpShotgun);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_PUMPSHOTGUN_MK2")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.PumpShotgunMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Pump Shotgun MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.PumpShotgunMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.PumpShotgunMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SAWNOFFSHOTGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SawedOffShotgun, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Sawnoff Shotgun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SawedOffShotgun);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SawedOffShotgun);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_ASSAULTSHOTGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.AssaultShotgun, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Assault Shotgun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.AssaultShotgun);
                player.Emit("WeaponEquipped", (uint)WeaponModel.AssaultShotgun);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_BULLPUPSHOTGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.BullpupShotgun, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Bullpup Shotgun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.BullpupShotgun);
                player.Emit("WeaponEquipped", (uint)WeaponModel.BullpupShotgun);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_HEAVYSHOTGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.HeavyShotgun, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Heavy Shotgun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.HeavyShotgun);
                player.Emit("WeaponEquipped", (uint)WeaponModel.HeavyShotgun);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_ASSAULTRIFLE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.AssaultRifle, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Assault Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.AssaultRifle);
                player.Emit("WeaponEquipped", (uint)WeaponModel.AssaultRifle);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_ASSAULTRIFLE_MK2")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.AssaultRifleMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Assault Rifle MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.AssaultRifleMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.AssaultRifleMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_CARBINE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.CarbineRifle, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Carbine Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.CarbineRifle);
                player.Emit("WeaponEquipped", (uint)WeaponModel.CarbineRifle);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_CARBINE_MK2")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.CarbineRifleMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Carbine Rifle MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.CarbineRifleMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.CarbineRifleMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_ADVANCEDRIFLE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.AdvancedRifle, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Advanced Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.AdvancedRifle);
                player.Emit("WeaponEquipped", (uint)WeaponModel.AdvancedRifle);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SPECIALCARBINE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SpecialCarbine, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Special Carbine Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SpecialCarbine);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SpecialCarbine);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SPECIALCARBINE_MK2")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SpecialCarbineMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Special Carbine Rifle MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SpecialCarbineMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SpecialCarbineMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_BULLPUPRIFLE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.BullpupRifle, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Bullpup Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.BullpupRifle);
                player.Emit("WeaponEquipped", (uint)WeaponModel.BullpupRifle);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_BULLPUPRIFLE_MK2")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.BullpupRifleMkII, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Bullpup Rifle MK2");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.BullpupRifleMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.BullpupRifleMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_GUSENBERG")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.GusenbergSweeper, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Tommygun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.GusenbergSweeper);
                player.Emit("WeaponEquipped", (uint)WeaponModel.GusenbergSweeper);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_SNIPERRIFLE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.SniperRifle, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Sniper Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.SniperRifle);
                player.Emit("WeaponEquipped", (uint)WeaponModel.SniperRifle);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_WEAPON_HEAVYSNIPER")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.HeavySniper, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Heavy Sniper Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.HeavySniper);
                player.Emit("WeaponEquipped", (uint)WeaponModel.HeavySniper);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_GRENADE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Grenade, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Grenade.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Grenade);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Grenade);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_BZGAS")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.BZGas, currentBulletCount, true);
                player.SendEmoteMessage("has equipped a Gas Grenade.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.BZGas);
                player.Emit("WeaponEquipped", (uint)WeaponModel.BZGas);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_MOLOTOV")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.MolotovCocktail, currentBulletCount, true);
                player.SendEmoteMessage($"has equipped a {selectedInventoryItem.GetName(false)}.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.MolotovCocktail);
                player.Emit("WeaponEquipped", (uint)WeaponModel.MolotovCocktail);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_PROXMINE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.ProximityMines, currentBulletCount, true);
                player.SendEmoteMessage($"has equipped a {selectedInventoryItem.GetName(false)}.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.ProximityMines);
                player.Emit("WeaponEquipped", (uint)WeaponModel.ProximityMines);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_BASEBALL")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Baseball, currentBulletCount, true);
                player.SendEmoteMessage($"has equipped a {selectedInventoryItem.GetName(false)}.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Baseball);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Baseball);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_KNIFE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Knife, currentBulletCount, true);
                player.SendEmoteMessage($"has equipped a {selectedInventoryItem.GetName(false)}.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Knife);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Knife);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_MACHETE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Machete, currentBulletCount, true);
                player.SendEmoteMessage($"has equipped a {selectedInventoryItem.GetName(false)}.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Machete);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Machete);
                return;
            }
            if (selectedInventoryItem.Id == "ITEM_WEAPON_SWITCHBLADE")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Switchblade, currentBulletCount, true);
                player.SendEmoteMessage($"has equipped a {selectedInventoryItem.GetName(false)}.");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Switchblade);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Switchblade);
                return;
            }

            #region Police Weapons

            if (selectedInventoryItem.Id == "ITEM_POLICE_WEAPON_STUNGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.StunGun, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.StunGun, 5);
                player.SendEmoteMessage("has equipped a Taser");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.StunGun);
                player.Emit("WeaponEquipped", (uint)WeaponModel.StunGun);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_POLICE_WEAPON_PISTOL")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.CombatPistol, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.CombatPistol, 5);
                player.AddWeaponComponent(WeaponModel.CombatPistol, Alt.Hash("COMPONENT_AT_PI_FLSH"));
                player.SendEmoteMessage("has equipped a Pistol");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.CombatPistol);
                player.Emit("WeaponEquipped", (uint)WeaponModel.CombatPistol);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_POLICE_WEAPON_NIGHTSTICK")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Nightstick, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.Nightstick, 5);
                player.SendEmoteMessage("has equipped a Nightstick");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Nightstick);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Nightstick);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_POLICE_WEAPON_FLASHLIGHT")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Flashlight, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.Flashlight, 5);
                player.SendEmoteMessage("has equipped a Flashlight");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Flashlight);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Flashlight);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_POLICE_WEAPON_SHOTGUN")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.PumpShotgunMkII, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.PumpShotgunMkII, 5);
                player.SendEmoteMessage("has equipped a Shotgun");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.PumpShotgunMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.PumpShotgunMkII);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_POLICE_WEAPON_AR")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.CarbineRifleMkII, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.CarbineRifleMkII, 5);
                player.SendEmoteMessage("has equipped a Assault Rifle");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.CarbineRifleMkII);
                player.Emit("WeaponEquipped", (uint)WeaponModel.CarbineRifleMkII);
                return;
            }

            #endregion Police Weapons

            #region Fire Department Weapons

            if (selectedInventoryItem.Id == "ITEM_FIRE_WEAPON_FLASHLIGHT")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Flashlight, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.Flashlight, 5);
                player.SendEmoteMessage("has equipped a Flashlight");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Flashlight);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Flashlight);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_FIRE_WEAPON_HATCHET")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Hatchet, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.Hatchet, 5);
                player.SendEmoteMessage("has equipped a Hatchet");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Hatchet);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Hatchet);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_FIRE_WEAPON_CROWBAR")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.Crowbar, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.Crowbar, 5);
                player.SendEmoteMessage("has equipped a Crowbar");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.Crowbar);
                player.Emit("WeaponEquipped", (uint)WeaponModel.Crowbar);
                return;
            }

            if (selectedInventoryItem.Id == "ITEM_FIRE_WEAPON_FIRE_EXTINGUISHER")
            {
                player.SetData("CURRENTWEAPON", selectedInventoryItem.ItemInfo.ID);
                player.GiveWeapon(WeaponModel.FireExtinguisher, currentBulletCount, true);
                player.SetWeaponTintIndex(WeaponModel.FireExtinguisher, 5);
                player.SendEmoteMessage("has equipped a Fire Extinguisher");
                player.SetData("CurrentWeaponHash", (uint)WeaponModel.FireExtinguisher);
                player.Emit("WeaponEquipped", (uint)WeaponModel.FireExtinguisher);
                return;
            }

            #endregion Fire Department Weapons

            player.SendErrorNotification("Item can't be equipped!");
            return;
        }
    }
}