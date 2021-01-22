using System;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Inventory;

namespace Server.Weapons
{
    public class WeaponSwitch
    {
        public static void OnWeaponKeyBindReleased(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (player.GetClass().Cuffed) return;

            Models.Character? playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            if (string.IsNullOrEmpty(playerCharacter.ActiveWeapon)) return;

            bool hasCurrentWeaponData = player.GetData("CURRENTWEAPON", out string currentWeapon);

            if (hasCurrentWeaponData && !string.IsNullOrEmpty(currentWeapon))
            {
                // Has Weapon Equipped
                WeaponCommands.CommandUnEquip(player);
                return;
            }

            // Weapon not equipped

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            InventoryItem activeWeaponInventoryItem =
                JsonConvert.DeserializeObject<InventoryItem>(playerCharacter.ActiveWeapon);

            bool hasItem = false;

            WeaponInfo activeWeaponInfo =
                JsonConvert.DeserializeObject<WeaponInfo>(activeWeaponInventoryItem.ItemValue);

            foreach (InventoryItem inventoryItem in playerInventory.GetInventoryItems(activeWeaponInventoryItem.Id))
            {
                WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(inventoryItem.ItemValue);

                if (weaponInfo.AmmoCount != activeWeaponInfo.AmmoCount) continue;

                hasItem = true;
            }

            if (!hasItem)
            {
                player.SendErrorNotification("An error occurred getting your weapon. Do you have it?");
                return;
            }

            EquipItem.EquipItemAttribute(player, activeWeaponInventoryItem);
        }

        public static bool AltOnOnPlayerWeaponChange(IPlayer player, uint oldweapon, uint newweapon)
        {
            WeaponModel oldWeaponModel = (WeaponModel)oldweapon;
            WeaponModel newWeaponModel = (WeaponModel)newweapon;

            if (newWeaponModel == WeaponModel.Fist)
            {
                // Holstered

                bool pistolSwitchAnim = false;

                switch (oldWeaponModel)
                {
                    case WeaponModel.AntiqueCavalryDagger:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.BaseballBat:
                        break;

                    case WeaponModel.BrokenBottle:
                        break;

                    case WeaponModel.Crowbar:
                        break;

                    case WeaponModel.Fist:
                        break;

                    case WeaponModel.Flashlight:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.GolfClub:
                        break;

                    case WeaponModel.Hammer:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Hatchet:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.BrassKnuckles:
                        break;

                    case WeaponModel.Knife:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Machete:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Switchblade:
                        break;

                    case WeaponModel.Nightstick:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.PipeWrench:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.BattleAxe:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.PoolCue:
                        break;

                    case WeaponModel.StoneHatchet:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Pistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.PistolMkII:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.CombatPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.APPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.StunGun:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Pistol50:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.SNSPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.SNSPistolMkII:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.HeavyPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.VintagePistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.FlareGun:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MarksmanPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.HeavyRevolver:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.HeavyRevolverMkII:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.DoubleActionRevolver:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.UpnAtomizer:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MicroSMG:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.SMG:
                        break;

                    case WeaponModel.SMGMkII:
                        break;

                    case WeaponModel.AssaultSMG:
                        break;

                    case WeaponModel.CombatPDW:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MachinePistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MiniSMG:
                        break;

                    case WeaponModel.UnholyHellbringer:
                        break;

                    case WeaponModel.PumpShotgun:
                        break;

                    case WeaponModel.PumpShotgunMkII:
                        break;

                    case WeaponModel.SawedOffShotgun:
                        break;

                    case WeaponModel.AssaultShotgun:
                        break;

                    case WeaponModel.BullpupShotgun:
                        break;

                    case WeaponModel.Musket:
                        break;

                    case WeaponModel.HeavyShotgun:
                        break;

                    case WeaponModel.DoubleBarrelShotgun:
                        break;

                    case WeaponModel.SweeperShotgun:
                        break;

                    case WeaponModel.AssaultRifle:
                        break;

                    case WeaponModel.AssaultRifleMkII:
                        break;

                    case WeaponModel.CarbineRifle:
                        break;

                    case WeaponModel.CarbineRifleMkII:
                        break;

                    case WeaponModel.AdvancedRifle:
                        break;

                    case WeaponModel.SpecialCarbine:
                        break;

                    case WeaponModel.SpecialCarbineMkII:
                        break;

                    case WeaponModel.BullpupRifle:
                        break;

                    case WeaponModel.BullpupRifleMkII:
                        break;

                    case WeaponModel.CompactRifle:
                        break;

                    case WeaponModel.MG:
                        break;

                    case WeaponModel.CombatMG:
                        break;

                    case WeaponModel.CombatMGMkII:
                        break;

                    case WeaponModel.GusenbergSweeper:
                        break;

                    case WeaponModel.SniperRifle:
                        break;

                    case WeaponModel.HeavySniper:
                        break;

                    case WeaponModel.HeavySniperMkII:
                        break;

                    case WeaponModel.MarksmanRifle:
                        break;

                    case WeaponModel.MarksmanRifleMkII:
                        break;

                    case WeaponModel.RPG:
                        break;

                    case WeaponModel.GrenadeLauncher:
                        break;

                    case WeaponModel.GrenadeLauncherSmoke:
                        break;

                    case WeaponModel.Minigun:
                        break;

                    case WeaponModel.FireworkLauncher:
                        break;

                    case WeaponModel.Railgun:
                        break;

                    case WeaponModel.HomingLauncher:
                        break;

                    case WeaponModel.CompactGrenadeLauncher:
                        break;

                    case WeaponModel.Widowmaker:
                        break;

                    case WeaponModel.Grenade:
                        break;

                    case WeaponModel.BZGas:
                        break;

                    case WeaponModel.MolotovCocktail:
                        break;

                    case WeaponModel.StickyBomb:
                        break;

                    case WeaponModel.ProximityMines:
                        break;

                    case WeaponModel.Snowballs:
                        break;

                    case WeaponModel.PipeBombs:
                        break;

                    case WeaponModel.Baseball:
                        break;

                    case WeaponModel.TearGas:
                        break;

                    case WeaponModel.Flare:
                        break;

                    case WeaponModel.JerryCan:
                        break;

                    case WeaponModel.Parachute:
                        break;

                    case WeaponModel.FireExtinguisher:
                        break;

                    default:
                        pistolSwitchAnim = false;
                        break;
                }

                //if (pistolSwitchAnim) player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 1 : 3);

                bool hasWeaponHashData = player.GetData("CurrentWeaponHash", out uint currentWeaponHash);

                if (hasWeaponHashData)
                {
                    if (oldweapon == currentWeaponHash)
                    {
                        player.GiveWeapon(currentWeaponHash, 0, false);
                    }
                }
            }

            if (oldWeaponModel == WeaponModel.Fist)
            {
                // Un holstered

                bool pistolSwitchAnim = false;

                switch (newWeaponModel)
                {
                    case WeaponModel.AntiqueCavalryDagger:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.BaseballBat:
                        break;

                    case WeaponModel.BrokenBottle:
                        break;

                    case WeaponModel.Crowbar:
                        break;

                    case WeaponModel.Fist:
                        break;

                    case WeaponModel.Flashlight:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.GolfClub:
                        break;

                    case WeaponModel.Hammer:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Hatchet:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.BrassKnuckles:
                        break;

                    case WeaponModel.Knife:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Machete:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Switchblade:
                        break;

                    case WeaponModel.Nightstick:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.PipeWrench:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.BattleAxe:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.PoolCue:
                        break;

                    case WeaponModel.StoneHatchet:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Pistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.PistolMkII:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.CombatPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.APPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.StunGun:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.Pistol50:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.SNSPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.SNSPistolMkII:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.HeavyPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.VintagePistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.FlareGun:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MarksmanPistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.HeavyRevolver:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.HeavyRevolverMkII:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.DoubleActionRevolver:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.UpnAtomizer:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MicroSMG:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.SMG:
                        break;

                    case WeaponModel.SMGMkII:
                        break;

                    case WeaponModel.AssaultSMG:
                        break;

                    case WeaponModel.CombatPDW:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MachinePistol:
                        pistolSwitchAnim = true;
                        break;

                    case WeaponModel.MiniSMG:
                        break;

                    case WeaponModel.UnholyHellbringer:
                        break;

                    case WeaponModel.PumpShotgun:
                        break;

                    case WeaponModel.PumpShotgunMkII:
                        break;

                    case WeaponModel.SawedOffShotgun:
                        break;

                    case WeaponModel.AssaultShotgun:
                        break;

                    case WeaponModel.BullpupShotgun:
                        break;

                    case WeaponModel.Musket:
                        break;

                    case WeaponModel.HeavyShotgun:
                        break;

                    case WeaponModel.DoubleBarrelShotgun:
                        break;

                    case WeaponModel.SweeperShotgun:
                        break;

                    case WeaponModel.AssaultRifle:
                        break;

                    case WeaponModel.AssaultRifleMkII:
                        break;

                    case WeaponModel.CarbineRifle:
                        break;

                    case WeaponModel.CarbineRifleMkII:
                        break;

                    case WeaponModel.AdvancedRifle:
                        break;

                    case WeaponModel.SpecialCarbine:
                        break;

                    case WeaponModel.SpecialCarbineMkII:
                        break;

                    case WeaponModel.BullpupRifle:
                        break;

                    case WeaponModel.BullpupRifleMkII:
                        break;

                    case WeaponModel.CompactRifle:
                        break;

                    case WeaponModel.MG:
                        break;

                    case WeaponModel.CombatMG:
                        break;

                    case WeaponModel.CombatMGMkII:
                        break;

                    case WeaponModel.GusenbergSweeper:
                        break;

                    case WeaponModel.SniperRifle:
                        break;

                    case WeaponModel.HeavySniper:
                        break;

                    case WeaponModel.HeavySniperMkII:
                        break;

                    case WeaponModel.MarksmanRifle:
                        break;

                    case WeaponModel.MarksmanRifleMkII:
                        break;

                    case WeaponModel.RPG:
                        break;

                    case WeaponModel.GrenadeLauncher:
                        break;

                    case WeaponModel.GrenadeLauncherSmoke:
                        break;

                    case WeaponModel.Minigun:
                        break;

                    case WeaponModel.FireworkLauncher:
                        break;

                    case WeaponModel.Railgun:
                        break;

                    case WeaponModel.HomingLauncher:
                        break;

                    case WeaponModel.CompactGrenadeLauncher:
                        break;

                    case WeaponModel.Widowmaker:
                        break;

                    case WeaponModel.Grenade:
                        break;

                    case WeaponModel.BZGas:
                        break;

                    case WeaponModel.MolotovCocktail:
                        break;

                    case WeaponModel.StickyBomb:
                        break;

                    case WeaponModel.ProximityMines:
                        break;

                    case WeaponModel.Snowballs:
                        break;

                    case WeaponModel.PipeBombs:
                        break;

                    case WeaponModel.Baseball:
                        break;

                    case WeaponModel.TearGas:
                        break;

                    case WeaponModel.Flare:
                        break;

                    case WeaponModel.JerryCan:
                        break;

                    case WeaponModel.Parachute:
                        break;

                    case WeaponModel.FireExtinguisher:
                        break;

                    default:
                        pistolSwitchAnim = false;
                        break;
                }

                // if (pistolSwitchAnim) player.Emit("WeaponSwitchAnim", player.IsLeo(true) ? 0 : 2);
            }
            return true;
        }

        private static Timer tickTimer = null;

        public static void InitTick()
        {
            /*
            tickTimer = new Timer(1) { Enabled = true, AutoReset = true };

            tickTimer.Elapsed += (sender, args) =>
            {
                tickTimer.Stop();
                foreach (IPlayer player in Alt.Server.GetPlayers())
                {
                    if (player == null) continue;
                    bool hasLastData = player.GetData("tick:lastWeapon", out uint lastWeapon);

                    if (hasLastData && lastWeapon != player.CurrentWeapon)
                    {
                        player.SetData("tick:lastWeapon", player.CurrentWeapon);

                        OnWeaponSwitch(player, lastWeapon, player.CurrentWeapon);
                    }
                }
                tickTimer.Start();
            };*/
        }

        private static void OnWeaponSwitch(IPlayer player, uint oldWeapon, uint newWeapon)
        {
            if (newWeapon == (uint)WeaponModel.Fist)
            {
                bool hasWeaponHashData = player.GetData("CurrentWeaponHash", out uint currentWeaponHash);

                if (hasWeaponHashData)
                {
                    if (oldWeapon == currentWeaponHash)
                    {
                        player.GiveWeapon(currentWeaponHash, 0, false);
                    }
                }
            }
        }

        public static void OnLeftMouseButton(IPlayer player)
        {
        }
    }
}