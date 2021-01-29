using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using EntityStreamer;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Groups.Police.MDT;
using Server.Inventory;
using Server.Models;

namespace Server.Groups.Police
{
    public class PoliceCommands
    {
        private static List<SpikeStrip> _spikeStrips = new List<SpikeStrip>();

        [Command("ss", commandType: CommandType.Law, description: "Used to deploy a spike strip.")]
        public static void CommandDeploySpikeStrip(IPlayer player)
        {
            try
            {
                if (!player.IsLeo(true))
                {
                    player.SendPermissionError();
                    return;
                }

                if (player.IsInVehicle)
                {
                    player.SendErrorNotification("Your in a vehicle!");
                    return;
                }

                if (_spikeStrips.Count(x => x.PlayerId == player.GetPlayerId()) >= 2)
                {
                    player.SendErrorNotification("You may only have two spikes deployed at a time!");
                    return;
                }

                Position forwardPos = player.PositionInFront(1);

                if (forwardPos == Position.Zero)
                {
                    player.SendErrorNotification("Unable to fetch forward position.");
                    return;
                }

                DegreeRotation rotation = player.Rotation;

                Vector3 playerRot = new Vector3(rotation.Pitch, rotation.Roll, rotation.Yaw);

                forwardPos.Z -= 1.0f;

                Prop prop = PropStreamer.Create("p_stinger_04", forwardPos, playerRot, player.Dimension, frozen: true);

                prop.SetRotation(playerRot);

                IColShape colShape = Alt.CreateColShapeCylinder(forwardPos, 3f, 3f);

                colShape.Dimension = player.Dimension;
                colShape.SetData("SpikeStrip", true);

                SpikeStrip newStrip = new SpikeStrip(player, forwardPos, prop, colShape);

                _spikeStrips.Add(newStrip);

                player.SendInfoNotification($"You've placed down a spike strip.");
                Logging.AddToCharacterLog(player, $"has placed down a spike strip.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("pickupstrip", commandType: CommandType.Law, description: "Used to clear a spike strip.")]
        public static void PickupSpikeStrip(IPlayer player)
        {
            try
            {
                if (!player.IsLeo(true))
                {
                    player.SendPermissionError();
                    return;
                }

                if (player.IsInVehicle)
                {
                    player.SendErrorNotification("Your in a vehicle!");
                    return;
                }

                SpikeStrip nearestStrip = null;
                float distance = 5f;

                foreach (SpikeStrip spikeStrip in _spikeStrips)
                {
                    float spikeDistance = spikeStrip.Position.Distance(player.Position);

                    if (spikeDistance < distance)
                    {
                        nearestStrip = spikeStrip;
                        distance = spikeDistance;
                    }
                }

                if (nearestStrip == null)
                {
                    player.SendErrorNotification("Your not near a spike strip!");
                    return;
                }

                nearestStrip.ColShape.Remove();
                nearestStrip.Object.Delete();

                _spikeStrips.Remove(nearestStrip);

                player.SendInfoNotification($"You've removed the nearest spike strip!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("clearspikes", commandType: CommandType.Law, description: "Used to clear all spike strips")]
        public static void CommandClearSpikeStrips(IPlayer player)
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (!_spikeStrips.Any())
            {
                player.SendErrorNotification("There are no spike strips.");
                return;
            }

            int totalCount = _spikeStrips.Count;
            int count = 0;

            foreach (SpikeStrip spikeStrip in _spikeStrips)
            {
                count++;
                spikeStrip.ColShape.Remove();
                spikeStrip.Object.Delete();
            }

            _spikeStrips = new List<SpikeStrip>();

            player.SendInfoNotification($"You've removed {count}/{totalCount} spike strips!");
            Logging.AddToCharacterLog(player, $"has removed {count}/{totalCount} spike strips!");
        }

        [Command("mdt", commandType: CommandType.Law, description: "MDT: Shows the Mobile Data Terminal")]
        public static void CommandMobileDataTerminal(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            };

            if (!playerCharacter.FactionDuty)
            {
                player.SendErrorNotification("You must be on duty!");
                return;
            }

            bool hasPermission = false;

            if (!player.IsInVehicle)
            {
                hasPermission = player.Position.Distance(PoliceHandler.ArrestPosition) < 10f;
            }
            if (!hasPermission && player.IsInVehicle && player.Seat == 1)
            {
                Models.Vehicle vehicleData = player.Vehicle.FetchVehicleData();

                if (vehicleData == null)
                {
                    player.SendErrorNotification("You must be in a faction vehicle.");
                    return;
                }

                Faction vehicleFaction = Faction.FetchFaction(vehicleData.FactionId);

                if (vehicleFaction == null || vehicleFaction.SubFactionType != SubFactionTypes.Law)
                {
                    player.SendErrorNotification("You must be in a Law Enforcement Vehicle.");
                    return;
                }

                hasPermission = true;
            }
            if (!hasPermission && player.IsInVehicle && player.Seat == 2)
            {
                Models.Vehicle vehicleData = player.Vehicle.FetchVehicleData();

                if (vehicleData == null)
                {
                    player.SendErrorNotification("You must be in a faction vehicle.");
                    return;
                }

                Faction vehicleFaction = Faction.FetchFaction(vehicleData.FactionId);

                if (vehicleFaction == null || vehicleFaction.SubFactionType != SubFactionTypes.Law)
                {
                    player.SendErrorNotification("You must be in a Law Enforcement Vehicle.");
                    return;
                }

                hasPermission = true;
            }

            if (!hasPermission)
            {
                player.SendPermissionError();
                return;
            }

            MdtHandler.ShowMdt(player);
        }

        // /arrest [IdOrName] [Time Mins]
        [Command("arrest", onlyOne: true, commandType: CommandType.Law,
            description: "Arrest: Arrests / Jails a play for specified time.")]
        public static void LawCommandArrest(IPlayer player, string args = "")
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/arrest [IdOrName] [Time Mins]");
                return;
            }

            if (player.Position.Distance(PoliceHandler.ArrestPosition) > 5)
            {
                player.SendErrorNotification("You must be near the arrest location.");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot!");
                return;
            }

            string[] paramStrings = args.Split(' ');

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(paramStrings[0]);

            if (targetPlayer == null)
            {
                player.SendSyntaxMessage("/arrest [IdOrName] [Time Mins]");
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            if (targetPlayer == player)
            {
                player.SendErrorNotification("You can't jail yourself!");
                return;
            }

            bool tryTimeParse = int.TryParse(paramStrings[1], out int arrestTime);

            if (!tryTimeParse)
            {
                player.SendSyntaxMessage("/arrest [IdOrName] [Time Mins]");
                player.SendErrorNotification("Unable to fetch time input.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 5)
            {
                player.SendErrorNotification("You are not near this player.");
                return;
            }

            if (targetPlayer.IsInVehicle)
            {
                player.SendErrorNotification("The target must be on foot!");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacter = context.Character.Find(targetPlayer.GetClass().CharacterId);

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Unable to fetch target data.");
                return;
            }

            KeyValuePair<Position, int> cell = PoliceHandler.JailCells.OrderBy(x => x.Value).FirstOrDefault();

            PoliceHandler.JailCells.Remove(cell.Key);

            int newCount = cell.Value + 1;

            PoliceHandler.JailCells.Add(cell.Key, newCount);

            targetPlayer.Position = cell.Key;
            targetPlayer.Dimension = 0;

            targetPlayer.SetData("InJailCell", cell.Key);

            targetCharacter.JailMinutes = arrestTime;
            targetCharacter.InJail = true;

            context.SaveChanges();

            player.SendInfoNotification($"You have jailed {targetCharacter.Name} for {arrestTime} minutes.");
            targetPlayer.SendInfoNotification($"You have been arrested by {player.GetClass().Name} for {arrestTime} minutes.");

            Logging.AddToCharacterLog(player, $"has arrested {targetCharacter.Name} for {arrestTime} minutes.");
            Logging.AddToCharacterLog(targetPlayer, $"has been arrested by {player.GetClass().Name} for {arrestTime} minutes.");
        }

        [Command("unarrest", onlyOne: true, commandType: CommandType.Law, description: "Arrest: Un-jails a player")]
        public static void LawCommandUnarrest(IPlayer player, string args)
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/unarrest [IdOrName]");
                return;
            }

            if (player.Position.Distance(PoliceHandler.ArrestPosition) > 5)
            {
                player.SendErrorNotification("You must be near the arrest location.");
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot!");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendSyntaxMessage("/unarrest [IdOrName]");
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            if (targetPlayer == player)
            {
                player.SendErrorNotification("You can't unjail yourself!");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacter = context.Character.Find(targetPlayer.GetClass().CharacterId);

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Unable to fetch target data.");
                return;
            }

            if (!targetCharacter.InJail)
            {
                player.SendErrorNotification("This player is not in jail.");
                return;
            }

            if (targetPlayer.HasData("InJailCell"))
            {
                targetPlayer.GetData("InJailCell", out Position cellPosition);

                targetPlayer.DeleteData("InJailCell");

                KeyValuePair<Position, int> cell = PoliceHandler.JailCells.FirstOrDefault(x => x.Key == cellPosition);

                PoliceHandler.JailCells.Remove(cell.Key);

                int newCount = cell.Value - 1;

                PoliceHandler.JailCells.Add(cell.Key, newCount);
            }

            targetPlayer.Position = targetPlayer.Position;
            targetPlayer.Dimension = 0;

            targetCharacter.JailMinutes = 0;
            targetCharacter.InJail = false;

            context.SaveChanges();

            player.SendInfoNotification($"You have released {targetCharacter.Name} from jail!");
            targetPlayer.SendInfoNotification($"You have been released from jail.");

            Logging.AddToCharacterLog(player, $"has released {targetCharacter.Name} from jail.");
            Logging.AddToCharacterLog(targetPlayer, $"has been released from jail by {player.GetClass().Name}.");

            DiscordHandler.SendMessageToLogChannel(
                $"{player.GetClass().Name} has released {targetCharacter.Name} from jail.");
        }

        [Command("ticket", onlyOne: true, commandType: CommandType.Law,
            description: "Ticket: Issues a ticket to a player.")]
        public static void LawCommandTicket(IPlayer player, string args = "")
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/ticket [IdOrName] [Amount] [Reason]");
                return;
            }

            string[] splitParams = args.Split(' ');

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(splitParams[0]);

            if (targetPlayer == null)
            {
                player.SendSyntaxMessage("/ticket [IdOrName] [Amount] [Reason]");
                player.SendErrorNotification("Unable to find this player.");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can't do this fool!");
                return;
            }

            bool tryAmountParse = double.TryParse(splitParams[1], out double amount);

            if (!tryAmountParse)
            {
                player.SendSyntaxMessage("/ticket [IdOrName] [Amount] [Reason]");
                return;
            }

            string reason = string.Join(' ', splitParams.Skip(2));

            if (player.Position.Distance(targetPlayer.Position) > 5)
            {
                player.SendErrorNotification("You aren't in range.");
                return;
            }

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            if (targetInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching their inventory.");
                return;
            }

            string officerName = player.GetClass().Name;
            string targetName = targetPlayer.GetClass().Name;

            Ticket newTicket = new Ticket(targetPlayer.GetClass().CharacterId, player.GetClass().CharacterId, officerName, reason, amount);

            using Context context = new Context();

            context.Tickets.Add(newTicket);
            context.SaveChanges();

            InventoryItem ticketItem = new InventoryItem("ITEM_POLICE_TICKET", "Ticket", JsonConvert.SerializeObject(newTicket));

            bool added = targetInventory.AddItem(ticketItem);

            if (!added)
            {
                player.SendErrorNotification("An error occurred giving them a ticket.");
                return;
            }
            Logging.AddToCharacterLog(player, $"has given a ticket to {targetName}, Reason: {reason}, Amount: {amount}. Ticket Id: {newTicket.Id}");
            Logging.AddToCharacterLog(targetPlayer, $"has received a ticket from {officerName}. Reason: {reason}, Amount: {amount}. Ticket Id: {newTicket.Id}");

            player.SendInfoNotification($"You have given a ticket to {targetName}. Amount: {amount:C}, Reason: {reason}.");

            targetPlayer.SendInfoNotification($"You have received a ticket from {officerName}. Amount: {amount:C}, Reason: {reason}.");

            player.SendEmoteMessage($"reaches out and hands a ticket to {targetName}.");
        }

        [Command("impound", commandType: CommandType.Law, description: "Impound: Impounds a vehicle")]
        public static void LawCommandImpoundVehicle(IPlayer player)
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (player.Position.Distance(PoliceHandler.ImpoundPosition) > 15)
            {
                player.SendErrorNotification("You must be in the impound lot!");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicleData = context.Vehicle.Find(player.Vehicle.GetClass().Id);

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a ownable vehicle.");
                return;
            }

            Position playerPosition = player.Position;

            vehicleData.Impounded = true;
            vehicleData.PosX = playerPosition.X;
            vehicleData.PosY = playerPosition.Y;
            vehicleData.PosZ = playerPosition.Z;
            vehicleData.Engine = false;

            player.Vehicle.EngineOn = false;

            vehicleData.RotZ = player.Rotation.Yaw;

            player.Emit("Vehicle:SetEngineStatus", player.Vehicle, player.Vehicle.EngineOn, false);

            context.SaveChanges();

            Logging.AddToCharacterLog(player, $"has impounded vehicle id {vehicleData.Id}.");

            player.SendInfoNotification($"You have impounded {vehicleData.Name}, plate: {vehicleData.Plate}.");

            player.SendEmoteMessage(vehicleData.Engine ? $"turns the {vehicleData.Name} engine on." : $"turns the {vehicleData.Name} engine off.");
        }

        [Command("unimpound", commandType: CommandType.Law, description: "Impound: Un-impounds a vehicle")]
        public static void lawCommandUnimpound(IPlayer player)
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (player.Position.Distance(PoliceHandler.ImpoundPosition) > 15)
            {
                player.SendErrorNotification("You must be in the impound lot!");
                return;
            }

            using Context context = new Context();

            Models.Vehicle vehicleData = context.Vehicle.Find(player.Vehicle.GetClass().Id);

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a ownable vehicle.");
                return;
            }

            if (!vehicleData.Impounded)
            {
                player.SendErrorNotification("This vehicle isn't impounded!");
                return;
            }

            vehicleData.Impounded = false;

            context.SaveChanges();

            player.SendInfoNotification($"You have un-impounded {vehicleData.Name}, plate: {vehicleData.Plate} from the impound lot.");

            Logging.AddToCharacterLog(player, $"has unimpounded vehicle id {vehicleData.Id} from the impound.");
        }

        [Command("license", onlyOne: true, commandType: CommandType.Law, description: "Licensing: Give / take license to / from a player")]
        public static void LawCommandGiveWeaponLicense(IPlayer player, string args = "")
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            Models.Character? playerCharacter = player.FetchCharacter();

            PlayerFaction? activePlayerFaction = JsonConvert
                .DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList)
                .FirstOrDefault(x => x.Id == playerCharacter.ActiveFaction);

            if (activePlayerFaction == null)
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/license [Type] [IdOrName]");
                player.SendInfoNotification($"Types: 1 - Driving, 2 - Pistol");
                return;
            }

            string[] splitString = args.Split(' ');

            if (splitString.Length < 2)
            {
                player.SendSyntaxMessage("/license [Type] [IdOrName]");
                player.SendInfoNotification($"Types: 1 - Driving, 2 - Pistol");
                return;
            }

            bool tryTypeParse = int.TryParse(splitString[0], out int type);

            if (!tryTypeParse)
            {
                player.SendSyntaxMessage("/giveweplicense [Type] [IdOrName]");
                player.SendInfoNotification($"Types: 1 - Driving, 2 - Pistol");
                return;
            }

            string nameorid = string.Join(' ', splitString.Skip(1));

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(nameorid);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Target not found!");
                return;
            }

            Models.Character? targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Target not found!");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can't do this!");
                return;
            }

            if (targetPlayer.Position.Distance(player.Position) > 5)
            {
                player.SendErrorNotification("You are not in range of the player.");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacterDb = context.Character.Find(targetCharacter.Id);

            List<LicenseTypes> targetLicenses =
                JsonConvert.DeserializeObject<List<LicenseTypes>>(targetCharacterDb.LicensesHeld);

            LicenseTypes license;
            string licenseName;

            switch (type)
            {
                case 1:
                    license = LicenseTypes.Driving;
                    licenseName = "Driving";
                    break;

                case 2:
                    license = LicenseTypes.Pistol;
                    licenseName = "Pistol";
                    break;

                default:
                    player.SendErrorNotification("License type is not valid");
                    return;
            }

            bool containsLicense = targetLicenses.Contains(license);

            if (containsLicense)
            {
                targetLicenses.Remove(license);
                player.SendInfoNotification($"You have removed a {licenseName} License from {targetCharacter.Name}.");
                Logging.AddToCharacterLog(player, $"has removed {targetCharacter.Name} - Id: {targetCharacter.Id} license type: {licenseName}");
            }
            else
            {
                targetLicenses.Add(license);
                player.SendInfoNotification($"You have added a {licenseName} License to {targetCharacter.Name}.");
                Logging.AddToCharacterLog(player, $"has given {targetCharacter.Name} - Id: {targetCharacter.Id} license type: {licenseName}");
            }

            targetCharacterDb.LicensesHeld = JsonConvert.SerializeObject(targetLicenses);

            context.SaveChanges();
        }

        [Command("m", onlyOne: true, alternatives: "megaphone", commandType: CommandType.Law,
            description: "Chat: Used to broadcast your voice at range")]
        public static void LawCommandMegaphone(IPlayer player, string message = "")
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (message == "")
            {
                player.SendSyntaxMessage("/m [Message]");
                return;
            }

            if (message.Length < 3)
            {
                player.SendErrorNotification("Invalid message length.");
                return;
            }

            List<IPlayer> nearbyPlayers =
                Alt.Server.GetPlayers().Where(x => x.Position.Distance(player.Position) < 30).ToList();

            string playerName = player.GetClass().Name;

            foreach (IPlayer nearbyPlayer in nearbyPlayers)
            {
                nearbyPlayer.SendMegaphoneMessage($"{playerName} says: {message}");
            }
        }

        [Command("inspectweapon", commandType: CommandType.Law,
            description: "Investigate: Used to show weapon information")]
        public static void LawCommandInspectWeapon(IPlayer player)
        {
            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            if (string.IsNullOrEmpty(player.FetchCharacter().CurrentWeapon))
            {
                player.SendNotification("~r~You must be holding a weapon.");
                return;
            }

            InventoryItem currentWeaponItem = JsonConvert.DeserializeObject<InventoryItem>(player.FetchCharacter().CurrentWeapon);

            WeaponInfo weaponInfo = JsonConvert.DeserializeObject<WeaponInfo>(currentWeaponItem.ItemValue);

            if (weaponInfo == null)
            {
                player.SendErrorNotification("There was an error fetching the weapon info.");
                return;
            }

            player.SendEmoteMessage($"looks over the weapon they're holding.");

            string registeredSerial = !string.IsNullOrEmpty(weaponInfo.SerialNumber)
                ? weaponInfo.SerialNumber
                : "Unregistered";

            string registeredOwner = weaponInfo.Legal ? weaponInfo.Purchaser : "Unregistered";

            player.SendWeaponMessage($"Serial Number: {registeredSerial} - Registered Owner: {registeredOwner}");
            if (!weaponInfo.LastPerson.Any())
            {
                player.SendWeaponMessage($"No DNA found on this weapon.");
                return;
            }

            foreach (string dnaName in weaponInfo.LastPerson)
            {
                player.SendWeaponMessage($"DNA Match found for {dnaName}");
            }
        }

        [Command("cuff", onlyOne: true, commandType: CommandType.Law, description: "Used to cuff/uncuff someone")]
        public static void LawCommandCuff(IPlayer player, string args = "")
        {
            if (string.IsNullOrEmpty(args))
            {
                player.SendSyntaxMessage("/cuff [IdOrName]");
                return;
            }

            if (!player.IsLeo(true))
            {
                player.SendPermissionError();
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (player == targetPlayer) return;

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Unable to find this player.");
                return;
            }

            if (targetPlayer.Position.Distance(player.Position) > 3)
            {
                player.SendErrorNotification("You must be closer.");
                return;
            }

            if (targetPlayer.GetClass().Cuffed)
            {
                targetPlayer.GetClass().Cuffed = false;

                player.SendInfoNotification($"You've un-cuffed {targetPlayer.GetClass().Name}.");
                player.SendEmoteMessage($"reaches for the cuffs on {targetPlayer.GetClass().Name} and takes them off.");
                return;
            }

            targetPlayer.GetClass().Cuffed = true;

            player.SendInfoNotification($"You've cuffed {targetPlayer.GetClass().Name}.");
            player.SendEmoteMessage($"reaches for their cuffs, putting them on {targetPlayer.GetClass().Name}.");
            return;
        }
    }
}