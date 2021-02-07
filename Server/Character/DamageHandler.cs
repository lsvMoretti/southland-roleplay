using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Extensions.TextLabel;

namespace Server.Character
{
    public class DamageHandler
    {
        private const string _weaponDamage = "WeaponDamageData";

        public static Dictionary<int, TextLabel> BulletCasingLabels = new Dictionary<int, TextLabel>();
        public static Dictionary<int, BulletTextDraw> BulletCasings = new Dictionary<int, BulletTextDraw>();

        public static int BulletId = 1;

        public static Dictionary<int, List<BodyDamage>> DamageDictionary = new Dictionary<int, List<BodyDamage>>();

        public static List<WeaponModel> MeeleWeapons = new List<WeaponModel>
        {
            WeaponModel.BaseballBat,
            WeaponModel.Crowbar,
            WeaponModel.Fist,
            WeaponModel.Flashlight,
            WeaponModel.GolfClub,
            WeaponModel.BrassKnuckles,
            WeaponModel.Nightstick,
            WeaponModel.PipeWrench,
            WeaponModel.BattleAxe,
            WeaponModel.PoolCue,
        };

        public static void AltOnOnPlayerDamage(IPlayer player, IEntity attacker, uint weapon, ushort damage)
        {
        }

        public static bool AltOnOnWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotoffset, BodyPart bodypart)
        {
            if (target.Type == BaseObjectType.Player)
            {
                IPlayer? targetPlayer = (IPlayer)target;

                if (targetPlayer.GetClass().AdminDuty)
                {
                    return false;
                }

                Models.Character? targetCharacter = targetPlayer.FetchCharacter();

                if (targetCharacter != null)
                {
                    Logging.AddToCharacterLog(targetPlayer, $"has been hit by {targetPlayer.GetClass().Name} (({targetPlayer.GetClass().UcpName})) with a {(WeaponModel)weapon}.");
                }

                bool isFist = (WeaponModel)weapon == WeaponModel.Fist;

                if (isFist)
                {
                    Random rnd = new Random();

                    int rndDamage = rnd.Next(5, 11);

                    damage = (ushort)rndDamage;
                }

                int characterId = targetPlayer.GetClass().CharacterId;

                BodyDamage newDamage = new BodyDamage(bodypart, 1, damage, weapon);

                bool hasWeaponDamages =
                    DamageDictionary.TryGetValue(characterId, out List<BodyDamage>? weaponDamage);

                if (!hasWeaponDamages || weaponDamage == null)
                {
                    List<BodyDamage> newDamageList = new List<BodyDamage> { newDamage };

                    DamageDictionary.Add(characterId, newDamageList);

                    if (isFist)
                    {
                        targetPlayer.Health -= damage;
                        return false;
                    }
                    if (targetPlayer.Health <= 0) DeathHandler.OnPlayerDeath(targetPlayer, player, weapon);

                    return true;
                }

                BodyDamage? currentDamage = weaponDamage.FirstOrDefault(x => x.BodyPart == bodypart && x.Weapon == weapon);

                if (currentDamage == null)
                {
                    DamageDictionary.Remove(characterId);
                    weaponDamage.Add(newDamage);
                    DamageDictionary.Add(characterId, weaponDamage);

                    if (isFist)
                    {
                        targetPlayer.Health -= damage;
                        return false;
                    }
                    if (targetPlayer.Health <= 0) DeathHandler.OnPlayerDeath(targetPlayer, player, weapon);

                    return true;
                }

                BodyDamage bodyDamage = currentDamage;

                weaponDamage.Remove(currentDamage);

                bodyDamage.Count += 1;
                bodyDamage.DamageAmount += damage;

                DamageDictionary.Remove(characterId);
                weaponDamage.Add(bodyDamage);
                DamageDictionary.Add(characterId, weaponDamage);

                if (isFist)
                {
                    targetPlayer.Health -= damage;
                    return false;
                }
                if (targetPlayer.Health <= 0) DeathHandler.OnPlayerDeath(targetPlayer, player, weapon);

                return true;
            }

            return true;
        }

        [Command("damages", onlyOne: true, commandType: CommandType.Character,
            description: "Shows yours or another players damages")]
        public static void CharacterDamagesCommand(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                int characterId = player.GetClass().CharacterId;

                bool hasOwnDamageData = DamageDictionary.TryGetValue(characterId, out List<BodyDamage>? ownDamages);

                if (!hasOwnDamageData || ownDamages == null)
                {
                    player.SendErrorNotification("You don't have any damages recorded!");
                    return;
                }

                player.SendInfoMessage($"-- Your Damages -- ");

                foreach (BodyDamage ownDamage in ownDamages)
                {
                    string bodyPart = ownDamage.BodyPart switch
                    {
                        BodyPart.Pelvis => "Pelvis",
                        BodyPart.LeftHip => "Left Hip",
                        BodyPart.LeftLeg => "Left Leg",
                        BodyPart.LeftFoot => "Left Foot",
                        BodyPart.RightHip => "Right Hip",
                        BodyPart.RightLeg => "Right Leg",
                        BodyPart.RightFoot => "Right Foot",
                        BodyPart.LowerTorso => "Lower Torso",
                        BodyPart.UpperTorso => "Upper Torso",
                        BodyPart.Chest => "Chest",
                        BodyPart.UnderNeck => "Under Neck",
                        BodyPart.LeftShoulder => "Left Shoulder",
                        BodyPart.LeftUpperArm => "Left Upper Arm",
                        BodyPart.LeftElbow => "Left Elbow",
                        BodyPart.LeftWrist => "Left Wrist",
                        BodyPart.RightShoulder => "Right Shoulder",
                        BodyPart.RightUpperArm => "Right Upper Arm",
                        BodyPart.RightElbow => "Right Elbow",
                        BodyPart.RightWrist => "Right Wrist",
                        BodyPart.Neck => "Neck",
                        BodyPart.Head => "Head",
                        BodyPart.Unknown => "Unknown",
                        _ => "Unknown"
                    };

                    WeaponModel weaponModel = (WeaponModel)ownDamage.Weapon;

                    string weaponName = FetchWeaponNameForModel(weaponModel);

                    player.SendInfoMessage($"Weapon: {weaponName}, Location: {bodyPart}, Total Damage: {ownDamage.DamageAmount}, Damage Count: {ownDamage.Count}.");
                }

                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null || !targetPlayer.IsSpawned())
            {
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            if (targetPlayer.Position.Distance(player.Position) > 3)
            {
                player.SendErrorNotification("You must be closer!");
                return;
            }

            bool targetHasDamageData = DamageDictionary.TryGetValue(targetPlayer.GetClass().CharacterId, out List<BodyDamage>? targetBodyDamages);

            if (!targetHasDamageData || targetBodyDamages == null)
            {
                player.SendErrorNotification("They have no damages recorded!");
                return;
            }

            player.SendInfoMessage($"-- Damages for {targetPlayer.GetClass().Name} --");

            foreach (BodyDamage targetBodyDamage in targetBodyDamages)
            {
                string bodyPart = targetBodyDamage.BodyPart switch
                {
                    BodyPart.Pelvis => "Pelvis",
                    BodyPart.LeftHip => "Left Hip",
                    BodyPart.LeftLeg => "Left Leg",
                    BodyPart.LeftFoot => "Left Foot",
                    BodyPart.RightHip => "Right Hip",
                    BodyPart.RightLeg => "Right Leg",
                    BodyPart.RightFoot => "Right Foot",
                    BodyPart.LowerTorso => "Lower Torso",
                    BodyPart.UpperTorso => "Upper Torso",
                    BodyPart.Chest => "Chest",
                    BodyPart.UnderNeck => "Under Neck",
                    BodyPart.LeftShoulder => "Left Shoulder",
                    BodyPart.LeftUpperArm => "Left Upper Arm",
                    BodyPart.LeftElbow => "Left Elbow",
                    BodyPart.LeftWrist => "Left Wrist",
                    BodyPart.RightShoulder => "Right Shoulder",
                    BodyPart.RightUpperArm => "Right Upper Arm",
                    BodyPart.RightElbow => "Right Elbow",
                    BodyPart.RightWrist => "Right Wrist",
                    BodyPart.Neck => "Neck",
                    BodyPart.Head => "Head",
                    BodyPart.Unknown => "Unknown",
                    _ => "Unknown"
                };

                WeaponModel weaponModel = (WeaponModel)targetBodyDamage.Weapon;

                string weaponName = FetchWeaponNameForModel(weaponModel);

                player.SendInfoMessage($"Weapon: {weaponName}, Location: {bodyPart}, Total Damage: {targetBodyDamage.DamageAmount}, Damage Count: {targetBodyDamage.Count}.");
            }
        }

        public static string FetchWeaponNameForModel(WeaponModel model)
        {
            string weaponName = model switch
            {
                WeaponModel.AntiqueCavalryDagger => "Antique Cavalry Dagger",
                WeaponModel.BaseballBat => "Baseball Bat",
                WeaponModel.BrokenBottle => "Broken Bottle",
                WeaponModel.Crowbar => "Crowbar",
                WeaponModel.Fist => "Fist",
                WeaponModel.Flashlight => "Flashlight",
                WeaponModel.GolfClub => "Golf Club",
                WeaponModel.Hammer => "Hammer",
                WeaponModel.Hatchet => "Hatchet",
                WeaponModel.BrassKnuckles => "Brass Knuckles",
                WeaponModel.Knife => "Knife",
                WeaponModel.Machete => "Machete",
                WeaponModel.Switchblade => "Switchblade",
                WeaponModel.Nightstick => "Nightstick",
                WeaponModel.PipeWrench => "Pipe Wrench",
                WeaponModel.BattleAxe => "Battle Axe",
                WeaponModel.PoolCue => "Pool Cue",
                WeaponModel.StoneHatchet => "Stone Hatchet",
                WeaponModel.Pistol => "Pistol",
                WeaponModel.PistolMkII => "Pistol MKII",
                WeaponModel.CombatPistol => "Combat Pistol",
                WeaponModel.APPistol => "AP Pistol",
                WeaponModel.StunGun => "Stun Gun",
                WeaponModel.Pistol50 => "Pistol 50.",
                WeaponModel.SNSPistol => "SNS Pistol",
                WeaponModel.SNSPistolMkII => "SNS Pistol MKII",
                WeaponModel.HeavyPistol => "Heavy Pistol",
                WeaponModel.VintagePistol => "Vintage Pistol",
                WeaponModel.FlareGun => "Flare Gun",
                WeaponModel.MarksmanPistol => "Marksman Pistol",
                WeaponModel.HeavyRevolver => "Heavy Revolver",
                WeaponModel.HeavyRevolverMkII => "Heavy Revolver MKII",
                WeaponModel.DoubleActionRevolver => "Double Action Revolver",
                WeaponModel.UpnAtomizer => "Up' n' Atomizer",
                WeaponModel.MicroSMG => "Micro SMG",
                WeaponModel.SMG => "SMG",
                WeaponModel.SMGMkII => "SMG MKII",
                WeaponModel.AssaultSMG => "Assualt SMG",
                WeaponModel.CombatPDW => "Combat PDW",
                WeaponModel.MachinePistol => "Machine Pistol",
                WeaponModel.MiniSMG => "Mini SMG",
                WeaponModel.UnholyHellbringer => "Unholy Hellbringer",
                WeaponModel.PumpShotgun => "Pump Shotgun",
                WeaponModel.PumpShotgunMkII => "Pump Shotgun MKII",
                WeaponModel.SawedOffShotgun => "Sawed Off Shotgun",
                WeaponModel.AssaultShotgun => "Assualt Shotgun",
                WeaponModel.BullpupShotgun => "Bullpup Shotgun",
                WeaponModel.Musket => "Musket",
                WeaponModel.HeavyShotgun => "Heavy Shotgun",
                WeaponModel.DoubleBarrelShotgun => "Double Barrel Shotgun",
                WeaponModel.SweeperShotgun => "Sweeper Shotgun",
                WeaponModel.AssaultRifle => "Assault Rifle",
                WeaponModel.AssaultRifleMkII => "Assault Rifle MKII",
                WeaponModel.CarbineRifle => "Carbine Rifle",
                WeaponModel.CarbineRifleMkII => "Carbine Rifle MKII",
                WeaponModel.AdvancedRifle => "Advanced Rifle",
                WeaponModel.SpecialCarbine => "Special Carbine Rifle",
                WeaponModel.SpecialCarbineMkII => "Special Carbine Rifle MKII",
                WeaponModel.BullpupRifle => "Bullpup Rifle",
                WeaponModel.BullpupRifleMkII => "Bullpup Rifle MKII",
                WeaponModel.CompactRifle => "Compact Rifle",
                WeaponModel.MG => "MG",
                WeaponModel.CombatMG => "Combat MG",
                WeaponModel.CombatMGMkII => "Combat MG MKII",
                WeaponModel.GusenbergSweeper => "Gusenberg Sweeper",
                WeaponModel.SniperRifle => "Sniper Rifle",
                WeaponModel.HeavySniper => "Heavy Sniper",
                WeaponModel.HeavySniperMkII => "Heavy Sniper MKII",
                WeaponModel.MarksmanRifle => "Marksman Rifle",
                WeaponModel.MarksmanRifleMkII => "Marksman Rifle MKII",
                WeaponModel.RPG => "RPG",
                WeaponModel.GrenadeLauncher => "Grenade Launcher",
                WeaponModel.GrenadeLauncherSmoke => "Grenade Launcher Smoke",
                WeaponModel.Minigun => "Mini-gun",
                WeaponModel.FireworkLauncher => "Firework Launcher",
                WeaponModel.Railgun => "Rail Gun",
                WeaponModel.HomingLauncher => "Homing Launcher",
                WeaponModel.CompactGrenadeLauncher => "Compact Grenade Launcher",
                WeaponModel.Widowmaker => "Widow Maker",
                WeaponModel.Grenade => "Grenade",
                WeaponModel.BZGas => "BZ Gas",
                WeaponModel.MolotovCocktail => "Molotov Cocktail",
                WeaponModel.StickyBomb => "Sticky Bomb",
                WeaponModel.ProximityMines => "Proximity Mine",
                WeaponModel.Snowballs => "Snowball",
                WeaponModel.PipeBombs => "Pipe Bomb",
                WeaponModel.Baseball => "Baseball",
                WeaponModel.TearGas => "Tear Gas",
                WeaponModel.Flare => "Flare",
                WeaponModel.JerryCan => "Jerry Can",
                WeaponModel.Parachute => "Parachute",
                WeaponModel.FireExtinguisher => "Fire Extinguisher",
                _ => "Unknown"
            };

            return weaponName;
        }
    }
}