using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Inventory;
using Server.Models;
using Server.Weapons;

namespace Server.Character
{
    public class DeathHandler
    {
        public static Dictionary<int, DeadBody> DeadBodies = new Dictionary<int, DeadBody>();
        private static int _nextDeadBodyId = 1;

        public static void OnPlayerDeath(IPlayer player, IEntity killer, uint weapon)
        {
            if (!player.IsSpawned())
            {
                CreatorRoom.SendToCreatorRoom(player);
                return;
            }
            bool hasCurrentWeaponData = player.GetData("CurrentWeaponHash", out uint weaponHash);

            if (hasCurrentWeaponData && weaponHash != 0)
            {
                //player.Emit("fetchCurrentAmmo", "weapon:death:fetchAmmo", player.CurrentWeapon);
                WeaponCommands.CommandUnEquip(player);
            }

            //CharacterHandler.LoadCustomCharacter(player, true);

            player.FreezeInput(true);

            player.SetData("REVIVED", false);
            player.SetData("CANRESPAWN", false);

            player.GetClass().Downed = true;

            player.SendInfoNotification($"You've been downed.");

            bool hasDamageData = DamageHandler.DamageDictionary.TryGetValue(player.GetClass().CharacterId,
                out List<BodyDamage> bodyDamages);

            if (!hasDamageData || bodyDamages == null)
            {
                player.SendErrorNotification("No damage data for this player.");
                return;
            }

            int meeleCount = 0;

            foreach (BodyDamage bodyDamage in bodyDamages)
            {
                if (!DamageHandler.MeeleWeapons.Contains((WeaponModel)bodyDamage.Weapon)) continue;
                meeleCount++;
            }

            if (meeleCount == bodyDamages.Count)
            {
                player.SetSyncedMetaData("KnockedDown", true);
            }

            if (killer != null && killer != player)
            {
                if (killer.Type == BaseObjectType.Player)
                {
                    IPlayer killerPlayer = (IPlayer)killer;

                    WeaponModel weaponModel = (WeaponModel)weapon;

                    DiscordHandler.SendMessageToLogChannel(
                            $"Player {player.GetClass().Name} ({player.FetchAccount().Username}) has been killed by {killerPlayer.GetClass().Name}({killerPlayer.FetchAccount().Username}). Weapon: {weaponModel.ToString()}");

                    player.SetData("LastKiller", killerPlayer.GetClass().Name);
                    player.SetData("LastKillerWeapon", weapon);
                }

                if (killer.Type == BaseObjectType.Vehicle)
                {
                    IVehicle vehicle = (IVehicle)killer;

                    DiscordHandler.SendMessageToLogChannel(
                        $"Player {player.GetClass().Name} ({player.FetchAccount().Username}) has been killed by a vehicle. Driver: {vehicle.Driver?.GetClass().Name} ({vehicle.Driver?.FetchAccount().Username}).");
                }
            }
            else
            {
                DiscordHandler.SendMessageToLogChannel(
                    $"Player {player.GetClass().Name} ({player.FetchAccount().Username}) has died.");
            }

            Timer deathTimer = new Timer(30000) { AutoReset = false };

            deathTimer.Start();

            deathTimer.Elapsed += (sender, args) =>
            {
                deathTimer.Stop();

                bool hasRevivedData = player.GetData("REVIVED", out bool revived);

                if (!revived)
                {
                    player.SetData("CANRESPAWN", true);
                    player.SendInfoNotification($"You can now respawn if your not in a roleplay situation. /respawn");
                    return;
                }
            };
        }

        [Command("respawn", commandType: CommandType.Character, description: "Used once you have been downed to give up.")]
        public static void CommandRespawn(IPlayer player)
        {
            if (player == null) return;

            player.GetData("REVIVED", out bool revived);

            player.GetData("CANRESPAWN", out bool canRespawn);

            if (revived || !player.GetClass().Downed)
            {
                player.SendErrorNotification("You're not dead.");
                return;
            }

            if (!canRespawn)
            {
                player.SendErrorNotification("You can't respawn yet!");
                return;
            }

            bool hasCurrentWeaponData = player.GetData("CurrentWeaponHash", out uint weaponHash);

            if (hasCurrentWeaponData && weaponHash != 0)
            {
                using Context context = new Context();

                Models.Character playerCharacter = player.FetchCharacter();

                playerCharacter.CurrentWeapon = string.Empty;

                context.SaveChanges();
            }

            DeadBody newBody = new DeadBody(player);
            int nextId = _nextDeadBodyId;
            _nextDeadBodyId++;

            DeadBodies.Add(nextId, newBody);

            DeadBodyHandler.LoadDeadBodyForAll(newBody);

            player.GetClass().Downed = false;
            player.DeleteSyncedMetaData("KnockedDown");
            player.SetData("CANRESPAWN", false);
            player.SetData("REVIVED", true);
            player.DeleteData("WeaponDamageData");

            player.FreezeInput(false);

            player.SetPosition(new Position(1154.004f, -1525.728f, 34.84343f), Rotation.Zero, 1, true, true, switchOut: true);

            player.Dimension = 0;
            Alt.EmitAllClients("clearBlood", player);

            CharacterHandler.LoadCustomCharacter(player);
        }

        [Command("revive", onlyOne: true, commandType: CommandType.Fire, description: "Used to revive another player")]
        public static void CommandRevive(IPlayer player, string idorName = "")
        {
            Faction playerFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (playerFaction == null || playerFaction.SubFactionType != SubFactionTypes.Medical)
            {
                if (player.FetchAccount().AdminLevel < AdminLevel.Tester)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            if (player.FetchAccount().AdminLevel < AdminLevel.Tester && !player.FetchCharacter().FactionDuty)
            {
                player.SendPermissionError();
                return;
            }

            if (idorName == "")
            {
                player.SendSyntaxMessage("Usage: /revive [idorName]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(idorName);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            bool hasRevivedData = targetPlayer.GetData("REVIVED", out bool revived);

            if (hasRevivedData && revived || !targetPlayer.GetClass().Downed)
            {
                player.SendErrorNotification("Target not downed.");
                return;
            }

            targetPlayer.FreezePlayer(false);
            targetPlayer.FreezeCam(false);
            targetPlayer.FreezeInput(false);

            targetPlayer.GetClass().Downed = false;
            targetPlayer.DeleteSyncedMetaData("KnockedDown");

            targetPlayer.SetData("REVIVED", true);

            if (DamageHandler.DamageDictionary.ContainsKey(targetPlayer.GetClass().CharacterId))
            {
                DamageHandler.DamageDictionary.Remove(targetPlayer.GetClass().CharacterId);
            }

            targetPlayer.SetPosition(targetPlayer.Position, targetPlayer.Rotation, 1, true, true, loadWeapon: true);

            Alt.EmitAllClients("clearBlood", targetPlayer);
        }

        [Command("helpup", onlyOne: true, commandType: CommandType.Character,
            description: "Used to pickup someone during a brawl")]
        public static void CommandHelpUp(IPlayer player, string args = "")
        {
            if (!player.GetClass().Spawned) return;

            if (string.IsNullOrEmpty(args))
            {
                player.SendSyntaxMessage("/helpup [NameOrId]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null || !targetPlayer.GetClass().Spawned)
            {
                player.SendErrorNotification("Unable to find this player.");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can't do this.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 5 || targetPlayer.Dimension != player.Dimension)
            {
                player.SendErrorNotification("Your not near the player.");
                return;
            }

            if (player.GetClass().Downed)
            {
                player.SendErrorNotification("Your downed.");
                return;
            }

            bool hasRevivedData = targetPlayer.GetData("REVIVED", out bool revived);

            if (hasRevivedData && revived || !targetPlayer.GetClass().Downed)
            {
                player.SendErrorNotification("Target not downed.");
                return;
            }

            bool hasDamageData = DamageHandler.DamageDictionary.TryGetValue(targetPlayer.GetClass().CharacterId,
                out List<BodyDamage> bodyDamages);

            if (!hasDamageData || bodyDamages == null)
            {
                player.SendErrorNotification("No damage data for this player.");
                return;
            }

            int meeleCount = 0;

            foreach (BodyDamage bodyDamage in bodyDamages)
            {
                if (!DamageHandler.MeeleWeapons.Contains((WeaponModel)bodyDamage.Weapon)) continue;
                meeleCount++;
            }

            if (meeleCount != bodyDamages.Count)
            {
                player.SendErrorNotification("Looks like it's more than Meele's here!");
                return;
            }

            targetPlayer.FreezePlayer(false);
            targetPlayer.FreezeCam(false);
            targetPlayer.FreezeInput(false);

            targetPlayer.GetClass().Downed = false;
            targetPlayer.DeleteSyncedMetaData("KnockedDown");

            targetPlayer.SetData("REVIVED", true);

            if (DamageHandler.DamageDictionary.ContainsKey(targetPlayer.GetClass().CharacterId))
            {
                DamageHandler.DamageDictionary.Remove(targetPlayer.GetClass().CharacterId);
            }

            targetPlayer.SetPosition(targetPlayer.Position, targetPlayer.Rotation, 1, true, true, loadWeapon: true);

            Alt.EmitAllClients("clearBlood", targetPlayer);

            player.SendEmoteMessage($"reaches down and grabs {targetPlayer.GetClass().Name} and helps them up.");
        }

        public static void OnDeathReturnAmmo(IPlayer player, int ammoCount)
        {
            try
            {
                using Context context = new Context();

                var playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                if (!string.IsNullOrEmpty(playerCharacter.CurrentWeapon))
                {
                    InventoryItem currentWeaponItem =
                        JsonConvert.DeserializeObject<InventoryItem>(playerCharacter
                            .CurrentWeapon);

                    if (currentWeaponItem != null)
                    {
                        WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(currentWeaponItem.ItemValue);

                        weaponInfo.AmmoCount = ammoCount;

                        currentWeaponItem.ItemValue = JsonConvert.SerializeObject(weaponInfo);

                        playerCharacter.CurrentWeapon = JsonConvert.SerializeObject(currentWeaponItem);

                        context.SaveChanges();

                        player.SetData("UnEquipWeapon", playerCharacter.CurrentWeapon);

                        player.SetData("DeathWeapon", true);

                        WeaponCommands.CommandUnEquip(player);
                    }
                }

                player.SetData("DeathAmmo", ammoCount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}