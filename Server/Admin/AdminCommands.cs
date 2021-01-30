using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionTranslators.Internal;
using Serilog;
using Server.Apartments;
using Server.Audio;
using Server.Character;
using Server.Character.Clothing;
using Server.Chat;
using Server.Commands;
using Server.Dealerships;
using Server.Discord;
using Server.Doors;
using Server.Extensions;
using Server.Focuses;
using Server.Graffiti;
using Server.Groups;
using Server.Groups.Fire;
using Server.Groups.Police;
using Server.Inventory;
using Server.Jobs.Clerk;
using Server.Jobs.Delivery;
using Server.Jobs.Fishing;
using Server.Map;
using Server.Models;
using Server.Motel;
using Server.Objects;
using Server.Property;
using Server.Vehicle;
using Server.Weapons;
using Console = System.Console;

namespace Server.Admin
{
    public class AdminCommands
    {
        [Command("setdim", AdminLevel.Tester, commandType: CommandType.Admin, description: "Used to set a players dim")]
        public static void AdminCommandSetDim(IPlayer player, string args = "")
        {
            try
            {
                if (string.IsNullOrEmpty(args))
                {
                    player.SendSyntaxMessage("/setdim [Dimension] [IDOrName]");
                    return;
                }

                string[] splitString = args.Split(' ');

                if (splitString.Length < 2)
                {
                    player.SendSyntaxMessage("/setdim [Dimension] [IDOrName]");
                    return;
                }

                bool dimParse = int.TryParse(splitString[0], out int dimension);

                if (!dimParse)
                {
                    player.SendErrorNotification("Dimension must be a number.");
                    return;
                }

                string idOrName = string.Join(' ', splitString.Skip(1));

                IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(idOrName);

                if (targetPlayer == null)
                {
                    player.SendErrorNotification("Unable to find a player");
                    return;
                }

                targetPlayer.Dimension = dimension;

                player.SendInfoMessage($"You've set {targetPlayer.GetClass().Name}'s dimension to {dimension}.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("twp", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Used to teleport to your map waypoint")]
        public static void AdminCommandTPWaypoint(IPlayer player)
        {
            player.Emit("teleportToWaypoint");
        }

        [Command("editor", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Loads the rockstar editor")]
        public static void AdminCommandLoadEditor(IPlayer player)
        {
            if (player.HasData("RockstarEditor:Enabled"))
            {
                // If have data = in rockstar editor

                player.DeleteData("RockstarEditor:Enabled");
                player.Emit("RockstarEditor:Toggle", false);
                return;
            }

            player.SetData("RockstarEditor:Enabled", true);
            player.Emit("RockstarEditor:Toggle", true);
        }

        [Command("tp", AdminLevel.Tester, commandType: CommandType.Admin, description: "Other: Shows a list of places to TP too")]
        public static void CommandTP(IPlayer player)
        {
            Models.Account playerAccount = player.FetchAccount();

            if (playerAccount == null)
            {
                player.SendLoginError();

                return;
            }

            List<Teleport> teleportList = Teleport.FetchTeleports();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Teleport teleport in teleportList.OrderBy(x => x.Name))
            {
                menuItems.Add(new NativeMenuItem(teleport.Name));
            }

            NativeMenu menu = new NativeMenu("AdminTeleportSelect", "Teleports", "Select a Teleport", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void TeleportSelected(IPlayer player, string menuItem)
        {
            if (menuItem == "Close") return;

            Teleport selectedTeleport = Teleport.FetchTeleport(menuItem);

            player.SendInfoNotification($"Teleporting you to {selectedTeleport.Name}");

            Position newPosition = new Position(selectedTeleport.PosX, selectedTeleport.PosY, selectedTeleport.PosZ);

            player.Position = newPosition;

            player.Dimension = 0;
        }

        [Command("addtp", AdminLevel.HeadAdmin, true, "", CommandType.Admin, "Other: Used to add a teleport")]
        public static void AdminCommandAddTp(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/addtp [Name of TP]");
                return;
            }

            if (args.Trim().Length < 2)
            {
                player.SendErrorNotification("Name too short!");
                return;
            }

            Context context = new Context();

            Teleport newTeleport = new Teleport
            {
                Name = args,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z
            };

            context.Teleport.Add(newTeleport);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new teleport at this location with the following name: {args}");

            Logging.AddToAdminLog(player, $"has created a new teleport at their position with the name: {args}.");
        }

        [Command("sendto", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "TP: TP a player to a location")]
        public static void AdminCommandSendTo(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/sendto [NameOrId]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find this player.");
                return;
            }

            if (!targetPlayer.IsSpawned())
            {
                player.SendErrorNotification("Player not spawned.");
                return;
            }

            player.SetData("Admin:SendTo:Player", targetPlayer.GetPlayerId());

            List<Teleport> teleportList = Teleport.FetchTeleports();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Teleport teleport in teleportList.OrderBy(x => x.Name))
            {
                menuItems.Add(new NativeMenuItem(teleport.Name));
            }

            NativeMenu menu = new NativeMenu("AdminSendPlayerTeleportSelect", "Teleports", "Select a Teleport", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSendToTeleportSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("Admin:SendTo:Player", out int playerId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == playerId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find this player.");
                return;
            }

            Teleport selectedTeleport = Teleport.FetchTeleport(option);

            if (selectedTeleport == null)
            {
                player.SendErrorNotification("Unable to find this TP.");
                return;
            }

            targetPlayer.Position = new Position(selectedTeleport.PosX, selectedTeleport.PosY, selectedTeleport.PosZ);

            targetPlayer.SendInfoNotification($"You've been sent to {selectedTeleport.Name}.");

            Logging.AddToCharacterLog(targetPlayer, $"has been sent to {selectedTeleport.Name} by {player.GetClass().Name}");

            Logging.AddToAdminLog(player, $"has tp'd {targetPlayer.GetClass().Name} to {selectedTeleport.Name}");

            player.SendInfoNotification($"You've sent {targetPlayer.GetClass().Name} to {selectedTeleport.Name}.");
        }

        [Command("ajail", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "AJail: Used to Admin Jail someone")]
        public static void AdminCommandJail(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/ajail [NameOrId] [Time (Minutes)]");
                return;
            }

            string[] argSplit = args.Split(' ');

            if (argSplit.Length != 2)
            {
                player.SendSyntaxMessage("/ajail [NameOrId] [Time (Minutes)]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(argSplit[0]);

            if (targetPlayer == null)
            {
                player.SendNotification("~r~Unable to find target player.");
                return;
            }

            if (targetPlayer.FetchAccount() == null)
            {
                player.SendNotification("~r~Target isn't logged in!");
                return;
            }

            bool tryParse = int.TryParse(argSplit[1], out int time);

            if (!tryParse)
            {
                player.SendNotification("~r~Time must be a number!");
                return;
            }

            string adminUsername = player.FetchAccount().Username;

            using Context context = new Context();

            Models.Account targetAccount = context.Account.Find(targetPlayer.GetClass().AccountId);

            targetAccount.JailMinutes = time;
            targetAccount.InJail = true;

            AdminRecord newAdminRecord = new AdminRecord
            {
                AccountId = targetAccount.Id,
                CharacterId = targetAccount.LastCharacter,
                RecordType = AdminRecordType.Jail,
                DateTime = DateTime.Now,
                Reason = null,
                Time = time,
                Admin = adminUsername
            };

            context.AdminRecords.Add(newAdminRecord);

            context.SaveChanges();

            targetPlayer.Position = PoliceHandler.JailLocation;
            targetPlayer.Dimension = targetPlayer.GetPlayerId();

            targetPlayer.SendAdminMessage($"You have been admin jailed by {adminUsername} for {time:##' Minutes'}.");

            player.SendAdminMessage($"You have admin jailed {targetAccount.Username} for {time:##' Minutes'}.");

            Logging.AddToAdminLog(player, $"has admin jailed {targetAccount.Username} for {time:##' Minutes'}.");
        }

        [Command("pveh", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Character: Shows a list of player vehicles")]
        public static void CommandPlayerVehicles(IPlayer player, string nameorid = "")
        {
            if (nameorid == "")
            {
                player.SendSyntaxMessage("/pveh [Name or Id]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(nameorid);

            if (targetPlayer == null)
            {
                player.SendErrorNotification($"Player not found.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification($"Player not logged in.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<Models.Vehicle> targetDbVehicles = Models.Vehicle.FetchCharacterVehicles(targetCharacter.Id);

            if (!targetDbVehicles.Any())
            {
                player.SendErrorNotification($"This player has no vehicles.");
                return;
            }

            foreach (Models.Vehicle targetDbVehicle in targetDbVehicles)
            {
                menuItems.Add(new NativeMenuItem(targetDbVehicle.Name));
            }

            NativeMenu menu = new NativeMenu("AdminShowPlayerVehicles", "Vehicles",
                $"Vehicles for {targetCharacter.Name}", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("ADMINPLAYERVEHICLELIST", JsonConvert.SerializeObject(targetDbVehicles));
        }

        public static void AdminPlayerVehicleSelected(IPlayer player, string menuItem, int index)
        {
            if (menuItem == "Close") return;

            player.GetData("ADMINPLAYERVEHICLELIST", out string vehicleJson);
            List<Models.Vehicle> targetDbVehicles = JsonConvert.DeserializeObject<List<Models.Vehicle>>(vehicleJson);

            Models.Vehicle selectedDbVehicle = targetDbVehicles[index];

            if (selectedDbVehicle == null)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle information");
                return;
            }

            IVehicle targetVehicle =
                Alt.Server.GetVehicles().FirstOrDefault(x =>
                    x.FetchVehicleData() != null && x.FetchVehicleData().Id == selectedDbVehicle.Id);

            if (targetVehicle != null)
            {
                targetVehicle.Remove();
                /*
                targetVehicle.Position = player.Position;
                targetVehicle.Dimension = player.Dimension;*/

                //player.SendInfoMessage($"Teleported {selectedDbVehicle.Name} to you!");
                //return;
            }

            targetVehicle = Vehicle.Commands.SpawnVehicleById(selectedDbVehicle.Id, player.Position.Around(2f));

            targetVehicle.Dimension = player.Dimension;

            player.SendInfoNotification($"Spawned and Teleported {selectedDbVehicle.Name} to you.");
        }

        [Command("agetcar", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Vehicle: TP a vehicle by Id")]
        public static void AdminCommandGetCar(IPlayer player, string vehicleId = "")
        {
            if (vehicleId == "")
            {
                player.SendSyntaxMessage("/agetcar [Vehicle Id]");
                return;
            }

            bool tryParse = int.TryParse(vehicleId, out int vid);

            if (!tryParse)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle Id.");
                return;
            }

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(i => i.FetchVehicleData()?.Id == vid);

            if (targetVehicle == null)
            {
                Models.Vehicle vehicleData = Models.Vehicle.FetchVehicle(vid);

                if (vehicleData == null)
                {
                    player.SendErrorNotification("This vehicle doesn't exist!");
                    return;
                }

                targetVehicle = LoadVehicle.LoadDatabaseVehicleAsync(vehicleData, player.Position.Around(2f)).Result;

                targetVehicle.Dimension = player.Dimension;

                player.SendInfoNotification($"You have spawned vehicle Id: {vid} and teleported it to you.");

                return;
            }

            targetVehicle.Position = player.Position;

            targetVehicle.Dimension = player.Dimension;

            player.SendInfoNotification($"You have spawned vehicle Id: {vid} and teleported it to you.");
        }

        [Command("goto", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Player: TP to another player")]
        public static void AdminCommandGoto(IPlayer player, string idorname = "")
        {
            if (idorname == "")
            {
                player.SendSyntaxMessage("/goto [Id Or Name]");
                return;
            }

            bool tryParse = int.TryParse(idorname, out int id);

            IPlayer? targetPlayer = null;

            if (!tryParse)
            {
                targetPlayer = Alt.Server.GetPlayers()
                    .FirstOrDefault(x => x.GetClass().Name.ToLower().Contains(idorname.ToLower()));

                if (targetPlayer == null)
                {
                    player.SendErrorNotification("Player not found.");
                    return;
                }
            }
            else
            {
                targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == id);

                if (targetPlayer == null)
                {
                    player.SendErrorNotification("Player not found.");
                    return;
                }
            }

            player.Position = targetPlayer.Position;
            player.Dimension = targetPlayer.Dimension;

            player.SendInfoNotification(
                $"You have teleported to {targetPlayer.GetClass().Name} (Id: {targetPlayer.GetPlayerId()})");
        }

        [Command("bring", AdminLevel.Tester, true, "gethere", commandType: CommandType.Admin, description: "Player: Brings another player to you")]
        public static void AdminCommandBring(IPlayer player, string nameorid = "")
        {
            if (nameorid == "")
            {
                player.SendSyntaxMessage("/bring [idorname]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(nameorid);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            if (targetPlayer.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not spawned.");
                return;
            }

            targetPlayer.Position = player.Position.Around(1);
            targetPlayer.Dimension = player.Dimension;

            player.SendInfoNotification($"You have teleported {targetPlayer.GetClass().Name} to your location.");
            targetPlayer.SendInfoNotification(
                $"Admin {player.FetchAccount().Username} has teleported you to their location.");

            Logging.AddToAdminLog(player, $"has teleported {targetPlayer.GetClass().Name} to their location.");
        }

        [Command("asay", AdminLevel.Administrator, true, "broadcast", commandType: CommandType.Admin, description: "Chat: Announce server wide")]
        public static void AdminCommandBroadcast(IPlayer player, string message = "")
        {
            if (message == "")
            {
                player.SendSyntaxMessage($"/broadcast [Message]");
                return;
            }

            foreach (IPlayer? targetPlayer in Alt.Server.GetPlayers())
            {
                targetPlayer?.SendAdminBroadcastMessage(message);
            }

            Logging.AddToAdminLog(player, $"has broadcasted: {message}");
        }

        [Command("freeze", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Player: Freezes a player")]
        public static void AdminCommandFreezePlayer(IPlayer player, string idorname = "")
        {
            if (idorname == "")
            {
                player.SendSyntaxMessage($"/freeze [IdOrName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(idorname);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            string adminName = player.FetchAccount().Username;

            if (targetPlayer.IsFrozen())
            {
                targetPlayer.FreezePlayer(false);
                targetPlayer.FreezeCam(false);
                targetPlayer.FreezeInput(false);

                targetPlayer.SendInfoNotification($"You have been unfrozen by {adminName}.");
                player.SendInfoNotification($"You have unfrozen {targetPlayer.GetClass().Name}.");

                Logging.AddToAdminLog(player, $"has unfrozen {targetPlayer.GetClass().Name}.");
                Logging.AddToCharacterLog(targetPlayer, $"has been unfrozen by {adminName}.");
                return;
            }

            targetPlayer.FreezePlayer(true);
            targetPlayer.FreezeCam(true);

            targetPlayer.SendInfoNotification($"You have been frozen by {adminName}.");
            player.SendInfoNotification($"You have frozen {targetPlayer.GetClass().Name}.");

            Logging.AddToAdminLog(player, $"has frozen {targetPlayer.GetClass().Name}.");
            Logging.AddToCharacterLog(targetPlayer, $"has been frozen by {adminName}.");
        }

        [Command("mute", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Chat: Mutes a player")]
        public static void AdminCommandMute(IPlayer player, string idorname = "")
        {
            if (idorname == "")
            {
                player.SendSyntaxMessage($"/mute [IdOrName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(idorname);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            string adminName = player.FetchAccount().Username;

            bool hasData = targetPlayer.GetData("admin:isMuted", out bool isMuted);

            if (!hasData || !isMuted)
            {
                targetPlayer.SetData("admin:isMuted", true);
                targetPlayer.SendInfoNotification($"You have been muted by {adminName}.");
                player.SendInfoNotification($"You have muted {targetPlayer.GetClass().Name}.");

                Logging.AddToAdminLog(player, $"has muted {targetPlayer.GetClass().Name}.");
                Logging.AddToCharacterLog(targetPlayer, $"has been muted by {adminName}.");
                return;
            }

            targetPlayer.SetData("admin:isMuted", false);
            targetPlayer.SendInfoNotification($"You have been un-muted by {adminName}.");
            player.SendInfoNotification($"You have un-muted {targetPlayer.GetClass().Name}.");

            Logging.AddToAdminLog(player, $"has un-muted {targetPlayer.GetClass().Name}.");
            Logging.AddToCharacterLog(targetPlayer, $"has been un-muted by {adminName}.");
        }

        [Command("arefuel", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Vehicle: Admin refuels a vehicle")]
        public static void AdminCommandRefuel(IPlayer player, string fuelLevel = "")
        {
            if (fuelLevel == "")
            {
                player.SendSyntaxMessage("/arefuel [Amount]");
                return;
            }

            if (player.Vehicle == null)
            {
                player.SendErrorNotification("You are not in a vehicle!");
                return;
            }

            if (player.Vehicle.FetchVehicleData() == null)
            {
                player.SendErrorNotification("You can't do that in this vehicle!");
                return;
            }

            bool tryParse = int.TryParse(fuelLevel, out int fuelResult);

            if (!tryParse)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            if (fuelResult < 0 || fuelResult > 100)
            {
                player.SendErrorNotification("Values must be between 0 and 100.");
                return;
            }

            player.Vehicle.GetClass().FuelLevel = fuelResult;

            player.SendInfoNotification($"You've refueled this vehicle to {fuelResult}%");
        }

        [Command("mark", AdminLevel.Tester, commandType: CommandType.Admin, description: "Other: Marks a point to return to with /gotomark")]
        public static void AdminCommandMarkPosition(IPlayer player)
        {
            player.SendInfoNotification($"You have marked this position. You can use /gotomark.");
            player.SetData("admin:markedPosition", player.Position);
            player.SetData("admin:markedDimension", player.Dimension);
        }

        [Command("gotomark", AdminLevel.Tester, commandType: CommandType.Admin, description: "Other: Used with /mark to return to a position")]
        public static void AdminCommandGotoMark(IPlayer player)
        {
            bool hasMarkData = player.GetData("admin:markedPosition", out Position position);

            if (!hasMarkData)
            {
                player.SendErrorNotification("You need to use /mark first.");
                return;
            }

            player.GetData("admin:markedDimension", out int dimension);

            player.Position = position;
            player.Dimension = dimension;

            player.SendInfoNotification($"You have returned to your marked position.");
        }

        #region Dealership Commands

        [Command("setdealercam", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Dealership: Sets a cam position")]
        public static void AdminCommandSetDealershipCam(IPlayer player, string dealershipId = "")
        {
            if (dealershipId == "")
            {
                player.SendSyntaxMessage("/setdealercam [DealershipId]");
                return;
            }

            bool tryParse = int.TryParse(dealershipId, out int dealership);

            if (!tryParse)
            {
                player.SendErrorNotification("Dealership Id must be a number!");
                return;
            }

            player.SetData("DealershipCamId", dealership);

            player.Emit("FetchDealershipCamRot", dealershipId);
        }

        public static void FetchDealershipCamRotation(IPlayer player, int dealershipId, float camRot)
        {
            Console.WriteLine($"Dealer Id: {dealershipId}");

            player.GetData("DealershipCamId", out int dealerId);

            using Context context = new Context();

            Dealership? selectedDealership = context.Dealership.FirstOrDefault(x => x.Id == dealerId);

            if (selectedDealership == null)
            {
                player.SendErrorNotification("This dealership doesn't exist!");
                return;
            }

            selectedDealership.CamPosX = player.Position.X;
            selectedDealership.CamPosY = player.Position.Y;
            selectedDealership.CamPosZ = player.Position.Z;
            selectedDealership.CamRotZ = camRot;

            context.SaveChanges();

            player.SendInfoNotification($"You've updated the camera position for dealership: {selectedDealership.Name}.");
        }

        [Command("dealerid", AdminLevel.Administrator, commandType: CommandType.Admin, description: "Dealership: Returns the nearby dealership id")]
        public static void AdminCommandDealershipId(IPlayer player)
        {
            Dealership? nearestDealership = Dealership.FetchDealerships()
                .FirstOrDefault(x => new Position(x.PosX, x.PosY, x.PosZ).Distance(player.Position) <= 8f);

            if (nearestDealership == null)
            {
                player.SendErrorNotification("You're not near a dealership!");
                return;
            }

            player.SendInfoNotification($"Dealership: {nearestDealership.Name} - Id: {nearestDealership.Id}");
        }

        [Command("removedealership", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Dealership: Removes a dealership")]
        public static void AdminCommandRemoveDealership(IPlayer player, string dealershipId = "")
        {
            if (dealershipId == "")
            {
                player.SendSyntaxMessage("/removedealership [DealershipId]");
                return;
            }

            bool tryParse = int.TryParse(dealershipId, out int dealerId);

            if (!tryParse)
            {
                player.SendErrorNotification("An error occurred fetching the dealership id.");
                return;
            }

            using Context context = new Context();

            Dealership dealership = context.Dealership.Find(dealerId);

            if (dealership == null)
            {
                player.SendErrorNotification("Unable to find a dealership by this Id.");
                return;
            }

            player.SendInfoNotification($"You have removed dealership Id {dealerId} from the system!");

            /*DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Description = $"{player.FetchAccount().Username} has removed a dealership from the database.",
                ThumbnailUrl = "http://ls-v.com/img/logo.jpg",
                Timestamp = DateTimeOffset.Now,
                Title = "Dealership System",
            };

            embedBuilder.AddField("Dealership Id", dealershipId);
            embedBuilder.AddField("Dealership Name", dealership.Name);*/

            //await DiscordBot.SendEmbedToLogChannel(embedBuilder);

            DiscordHandler.SendMessageToLogChannel($"{player.FetchAccount().Username} has removed dealership id {dealershipId} from the database.");

            Dealership oldDealership = dealership;

            context.Dealership.Remove(dealership);
            context.SaveChanges();

            DealershipHandler.UnloadDealership(oldDealership);
        }

        [Command("createdealership", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Dealership: Creates a dealership")]
        public static void AdminCommandCreateDealership(IPlayer player, string dealershipName = "")
        {
            if (dealershipName == "")
            {
                player.SendSyntaxMessage("/createdealership [DealershipName]");
                return;
            }

            if (string.IsNullOrWhiteSpace(dealershipName) || dealershipName.Length < 3 ||
                dealershipName.StartsWith(" "))
            {
                player.SendErrorNotification("You need to input a longer dealership name!");
                return;
            }

            Context context = new Context();

            bool dealershipExists = context.Dealership.Any(x => x.Name == dealershipName);

            if (dealershipExists)
            {
                player.SendErrorNotification($"A dealership already exists with name: {dealershipName}.");
                return;
            }

            Dealership newDealership = new Dealership
            {
                Name = dealershipName,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                VehicleList = JsonConvert.SerializeObject(new List<DealershipVehicle>())
            };

            context.Add(newDealership);

            context.SaveChanges();

            player.SendInfoNotification(
                $"You have added a new dealership with the name {dealershipName}. Id: {newDealership.Id}");

            player.SendInfoNotification($"Don't forget to use /setdealercam and /setdealerveh");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Blue,
                Description = $"{player.FetchAccount().Username} has created a dealership.",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = "http://ls-v.com/img/logo.jpg" },
                Timestamp = DateTimeOffset.Now,
                Title = "Dealership System",
            };

            embedBuilder.AddField("Dealership Id", newDealership.Id.ToString());
            embedBuilder.AddField("Dealership Name", newDealership.Name);

            DealershipHandler.LoadDealerships();
        }

        [Command("setdealerveh", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Dealership: Sets dealership vehicle spawn position")]
        public static void AdminCommandSetDealerVeh(IPlayer player, string dealershipId = "")
        {
            if (dealershipId == "")
            {
                player.SendSyntaxMessage("/setdealerveh [DealershipId]");
                return;
            }

            bool tryParse = int.TryParse(dealershipId, out int dealerId);

            if (!tryParse)
            {
                player.SendErrorNotification("You need to input numbers only.");
                return;
            }

            using Context context = new Context();

            Dealership selectedDealership = context.Dealership.Find(dealerId);

            if (selectedDealership == null)
            {
                player.SendErrorNotification($"Unable to find a dealership by the id {dealerId}");

                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle");
                return;
            }

            Position playerPos = player.Vehicle.Position;

            selectedDealership.VehPosX = playerPos.X;
            selectedDealership.VehPosY = playerPos.Y;
            selectedDealership.VehPosZ = playerPos.Z;

            selectedDealership.VehRotZ = player.Rotation.Yaw;

            context.SaveChanges();

            player.SendInfoNotification($"You have updated the vehicle location for dealership id {dealerId}");
        }

        [Command("editdealer")]
        public static void AdminCommandEditDealer(IPlayer player)
        {
            if (!player.FetchAccount().Developer) return;

            AdminCommandEditDealership(player);
        }

        [Command("editdealership", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Dealership: Edits a dealership")]
        public static void AdminCommandEditDealership(IPlayer player)
        {
            Context context = new Context();

            Dealership nearDealership = null;

            foreach (Dealership dealership in context.Dealership.ToList())
            {
                Position dealershipPosition = new Position(dealership.PosX, dealership.PosY, dealership.PosZ);

                if (player.Position.Distance(dealershipPosition) < 5)
                {
                    nearDealership = dealership;
                    break;
                }
            }

            if (nearDealership == null)
            {
                player.SendErrorNotification("You are not near a dealership.");
                return;
            }

            var vehicleList = JsonConvert.DeserializeObject<List<DealershipVehicle>>(nearDealership.VehicleList)
                .OrderByDescending(x => x.VehName);

            player.ChatInput(false);
            player.FreezeCam(true);
            player.FreezeInput(true);
            player.Emit("admin:showEditDealershipVehicles", nearDealership.Name,
                JsonConvert.SerializeObject(vehicleList));
            player.SetData("EditingDealership", nearDealership.Id);
        }

        public static void CreateNewDealershipVehicle(IPlayer player, string vehicleName, string vehicleModel,
            string vehiclePrice)
        {
            if (player == null) return;

            bool tryPriceParse = int.TryParse(vehiclePrice, out int vPrice);

            if (!tryPriceParse)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle price.");
                return;
            }

            DealershipVehicle newDealershipVehicle = new DealershipVehicle
            {
                VehName = vehicleName,
                VehModel = 0,
                VehPrice = vPrice,
                NewVehModel = vehicleModel,
            };

            bool hasData = player.GetData("EditingDealership", out int dealershipId);

            if (!hasData)
            {
                player.SendErrorNotification("An error occurred!");
                return;
            }

            using Context context = new Context();

            Dealership nearestDealership = context.Dealership.Find(dealershipId);

            if (nearestDealership == null)
            {
                player.SendErrorNotification("An error occurred!");
                return;
            }

            List<DealershipVehicle> vehicleList =
                JsonConvert.DeserializeObject<List<DealershipVehicle>>(nearestDealership.VehicleList);

            vehicleList.Add(newDealershipVehicle);

            nearestDealership.VehicleList = JsonConvert.SerializeObject(vehicleList);

            context.SaveChanges();

            player.SendInfoNotification(
                $"You have added the {newDealershipVehicle.VehName} to {nearestDealership.Name} for {newDealershipVehicle.VehPrice.ToString("C0")}");
        }

        public static void EventCloseDealershipEditPage(IPlayer player)
        {
            player.ChatInput(true);
            player.FreezeInput(false);
            player.FreezeCam(false);
        }

        public static void EditDealershipVehicle(IPlayer player, string vehicleIndex, string vehicleName,
            string vehicleModel, string vehiclePrice)
        {
            if (player == null) return;

            DealershipVehicle newDealershipVehicle = new DealershipVehicle
            {
                VehName = vehicleName,
                VehModel = 0,
                VehPrice = Convert.ToInt32(vehiclePrice),
                NewVehModel = vehicleName
            };

            bool hasData = player.GetData("EditingDealership", out int dealershipId);

            if (!hasData)
            {
                player.SendErrorNotification("An error occurred!");
                return;
            }

            using Context context = new Context();

            Dealership nearestDealership = context.Dealership.Find(dealershipId);

            if (nearestDealership == null)
            {
                player.SendErrorNotification("An error occurred!");
                return;
            }

            List<DealershipVehicle> vehicleList =
                JsonConvert.DeserializeObject<List<DealershipVehicle>>(nearestDealership.VehicleList);

            int vIndex = int.Parse(vehicleIndex);

            vehicleList.Remove(vehicleList[vIndex]);

            vehicleList.Add(newDealershipVehicle);

            nearestDealership.VehicleList = JsonConvert.SerializeObject(vehicleList);

            context.SaveChanges();

            player.SendInfoNotification(
                $"You have updated the {newDealershipVehicle.VehName} to {nearestDealership.Name} for {newDealershipVehicle.VehPrice.ToString("C0")}");
        }

        public static void RemoveDealershipVehicle(IPlayer player, string vehicleIndex)
        {
            if (player == null) return;

            bool hasData = player.GetData("EditingDealership", out int dealershipId);

            if (!hasData)
            {
                player.SendErrorNotification("An error occurred!");
                return;
            }

            using Context context = new Context();

            Dealership nearestDealership = context.Dealership.Find(dealershipId);

            if (nearestDealership == null)
            {
                player.SendErrorNotification("An error occurred!");
                return;
            }

            List<DealershipVehicle> vehicleList =
                JsonConvert.DeserializeObject<List<DealershipVehicle>>(nearestDealership.VehicleList)
                    .OrderByDescending(x => x.VehName).ToList();

            int vIndex = int.Parse(vehicleIndex);

            DealershipVehicle dealershipVehicle = vehicleList[vIndex];

            if (dealershipVehicle == null)
            {
                player.SendErrorNotification("This vehicle doesn't exist!");
                return;
            }

            vehicleList.Remove(dealershipVehicle);

            nearestDealership.VehicleList = JsonConvert.SerializeObject(vehicleList);

            context.SaveChanges();

            player.SendInfoNotification($"You have removed {dealershipVehicle.VehName} from the {nearestDealership.Name}.");
        }

        [Command("editdealershipname", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Dealership: Edits a dealership name")]
        public static void AdminCommandDealershipName(IPlayer player, string dealershipName = "")
        {
            if (dealershipName == "")
            {
                player.SendSyntaxMessage("/editdealershipname [dealershipName]");
                return;
            }

            string dealerName = dealershipName.Trim();

            if (dealerName.Length < 3)
            {
                player.SendErrorNotification("You need to input a length greater than 3!");
                return;
            }

            using Context context = new Context();

            List<Dealership> dealerships = context.Dealership.ToList();

            Position playerPosition = player.Position;

            Dealership nearDealership = null;

            foreach (Dealership dealership in dealerships)
            {
                Position dealerPosition = new Position(dealership.PosX, dealership.PosY, dealership.PosZ);

                if (playerPosition.Distance(dealerPosition) < 5)
                {
                    nearDealership = dealership;
                    break;
                }
            }

            if (nearDealership == null)
            {
                player.SendErrorNotification("You are not near a dealership.");
                return;
            }

            Dealership currentDealership = context.Dealership.Find(nearDealership.Id);

            currentDealership.Name = dealerName;

            context.SaveChanges();

            player.SendInfoNotification($"You have updated the dealership name to {dealerName}");
            DealershipHandler.LoadDealerships();
        }

        #endregion Dealership Commands

        #region Faction Management Commands

        [Command("afactions", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Faction: Shows a list of factions")]
        public static void AdminCommandFactions(IPlayer player)
        {
            player.ChatInput(false);
            player.FreezeInput(true);
            player.FreezeCam(true);
            player.Emit("loadFactionList", JsonConvert.SerializeObject(Faction.FetchFactions()));
        }

        public static void OnFactionViewClose(IPlayer player)
        {
            player.ChatInput(true);
            player.FreezeInput(false);
            player.FreezeCam(false);
        }

        public static void AdjustFactionRankPerm(IPlayer player, string perm, string factionId, string rankIndex)
        {
            if (player == null) return;

            player.Emit("closeFactionPage");

            bool factionIsInt = int.TryParse(factionId, out int factionResult);

            if (!factionIsInt)
            {
                player.SendErrorNotification("An error occurred parsing some information!");
                return;
            }

            Faction selectedFaction = Faction.FetchFactions()[factionResult];

            if (selectedFaction == null)
            {
                player.SendErrorNotification("An error occured fetching the faction.");
                return;
            }

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(selectedFaction.RanksJson);

            bool rankIndexIsIndex = int.TryParse(rankIndex, out int rankResult);

            if (!rankIndexIsIndex)
            {
                player.SendErrorNotification("An error occured parsing some rank information.");
                return;
            }

            Rank selectedRank = factionRanks[rankResult];

            if (selectedRank == null)
            {
                player.SendErrorNotification("An error occured fetching the rank information.");
                return;
            }

            using Context context = new Context();

            Faction contextFaction = context.Faction.Find(selectedFaction.Id);

            if (contextFaction == null)
            {
                player.SendErrorNotification("An error occured fetching the faction from the Database.");
                return;
            }

            switch (perm)
            {
                case "invite":
                    selectedRank.Invite = !selectedRank.Invite;
                    player.SendInfoNotification(
                        $"You've set rank {selectedRank.Name} invite permission to {selectedRank.Invite} for faction {selectedFaction.Name}.");
                    break;

                case "promote":
                    selectedRank.Promote = !selectedRank.Promote;
                    player.SendInfoNotification(
                        $"You've set rank {selectedRank.Name} promote permission to {selectedRank.Promote} for faction {selectedFaction.Name}.");
                    break;

                case "addRank":
                    selectedRank.AddRanks = !selectedRank.AddRanks;
                    player.SendInfoNotification(
                        $"You've set rank {selectedRank.Name} add rank permission to {selectedRank.AddRanks} for faction {selectedFaction.Name}.");
                    break;

                case "deleteRank":
                    factionRanks.Remove(selectedRank);
                    player.SendInfoNotification(
                        $"You've removed rank {selectedRank.Name} from faction {selectedFaction.Name}.");
                    break;

                case "towVehicle":
                    selectedRank.Tow = !selectedRank.Tow;
                    player.SendInfoNotification(
                        $"You've set rank {selectedRank.Name} add rank permission to {selectedRank.Tow} for faction {selectedFaction.Name}.");
                    break;
            }

            contextFaction.RanksJson = JsonConvert.SerializeObject(factionRanks);

            context.SaveChanges();
        }

        public static void FetchFactionMembers(IPlayer player, string factionIdString)
        {
            if (player == null) return;

            bool factionIdIsInt = int.TryParse(factionIdString, out int factionId);

            if (!factionIdIsInt)
            {
                player.Emit("closeFactionPage");
                player.SendErrorNotification("An error occurred fetching the faction Id.");
                return;
            }

            using Context context = new Context();

            List<FactionMember> factionMembers = new List<FactionMember>();

            List<Models.Character> factionCharacters = context.Character.ToList();

            lock (factionCharacters)
            {
                foreach (Models.Character factionCharacter in factionCharacters)
                {
                    List<PlayerFaction> playerFactions =
                        JsonConvert.DeserializeObject<List<PlayerFaction>>(factionCharacter.FactionList);

                    foreach (PlayerFaction playerFaction in playerFactions)
                    {
                        if (playerFaction.Id == factionId)
                        {
                            factionMembers.Add(
                                new FactionMember(factionCharacter.Name, playerFaction.RankId, factionId));
                        }
                    }
                }

                player.Emit("admin:faction:factionMembers", JsonConvert.SerializeObject(factionMembers));
                player.SetData("admin:faction:memberList", JsonConvert.SerializeObject(factionMembers));
            }
        }

        public static void RemoveFactionMember(IPlayer player, string factionMemberIndex)
        {
            if (player == null) return;

            player.Emit("closeFactionPage");

            bool isIndexInt = int.TryParse(factionMemberIndex, out int index);

            if (!isIndexInt)
            {
                player.SendErrorNotification("An error occured fetching the index.");
                return;
            }

            bool hasListData = player.GetData("admin:faction:memberList", out string jsonList);

            if (!hasListData)
            {
                player.SendErrorNotification("An error occurred fetching the member list.");
                return;
            }

            List<FactionMember> factionMembers = JsonConvert.DeserializeObject<List<FactionMember>>(jsonList);

            FactionMember selectedFactionMember = factionMembers[index];

            using Context context = new Context();

            Models.Character playerCharacter =
                context.Character.FirstOrDefault(x => x.Name == selectedFactionMember.Name);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find this player in the database.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList);

            PlayerFaction selectedPlayerFaction =
                playerFactions.FirstOrDefault(x => x.Id == selectedFactionMember.FactionId);

            if (selectedPlayerFaction == null)
            {
                player.SendErrorNotification("This player isn't part of this faction.");
                return;
            }

            playerFactions.Remove(selectedPlayerFaction);

            playerCharacter.FactionList = JsonConvert.SerializeObject(playerFactions);

            context.SaveChanges();

            player.SendInfoNotification(
                $"You've removed {playerCharacter.Name} from {Faction.FetchFaction(selectedPlayerFaction.Id).Name}.");

            IPlayer? targetPlayer =
                Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().CharacterId == playerCharacter.Id);

            targetPlayer?.SendInfoNotification(
                $"You've been removed from {Faction.FetchFaction(selectedPlayerFaction.Id).Name}.");
        }

        public static void AdjustFactionMemberRank(IPlayer player, string memberIndexString, string factionIdString,
            string rankIndexString)
        {
            if (player == null) return;

            player.Emit("closeFactionPage");

            bool memberIndexIsInt = int.TryParse(memberIndexString, out int memberIndex);

            bool factionIdIsInt = int.TryParse(factionIdString, out int factionId);

            bool factionRankIsInt = int.TryParse(rankIndexString, out int rankIndex);

            if (!memberIndexIsInt || !factionIdIsInt || !factionRankIsInt)
            {
                player.SendErrorNotification("An error occurred fetching the member information.");
                return;
            }

            bool hasListData = player.GetData("admin:faction:memberList", out string jsonList);

            if (!hasListData)
            {
                player.SendErrorNotification("An error occurred fetching the member list.");
                return;
            }

            List<FactionMember> factionMembers = JsonConvert.DeserializeObject<List<FactionMember>>(jsonList);

            FactionMember selectedFactionMember = factionMembers[memberIndex];

            using Context context = new Context();

            Models.Character playerCharacter =
                context.Character.FirstOrDefault(x => x.Name == selectedFactionMember.Name);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find this player in the database.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList);

            PlayerFaction selectedPlayerFaction =
                playerFactions.FirstOrDefault(x => x.Id == selectedFactionMember.FactionId);

            if (selectedPlayerFaction == null)
            {
                player.SendErrorNotification("This player isn't part of this faction.");
                return;
            }

            Faction selectedFaction = Faction.FetchFaction(selectedFactionMember.FactionId);

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(selectedFaction.RanksJson);

            Rank selectedRank = factionRanks[rankIndex];

            selectedPlayerFaction.RankId = selectedRank.Id;

            playerCharacter.FactionList = JsonConvert.SerializeObject(playerFactions);

            context.SaveChanges();

            player.SendInfoNotification(
                $"You have set {playerCharacter.Name}'s rank to {selectedRank.Name} in {selectedFaction.Name}.");

            IPlayer? targetPlayer =
                Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().CharacterId == playerCharacter.Id);

            targetPlayer?.SendInfoNotification(
                $"Your rank in {selectedFaction.Name} has been updated to {selectedRank.Name}.");
        }

        public static void OnFactionRemove(IPlayer player, int factionId)
        {
            player.Emit("closeFactionPage");

            bool removed = FactionHandler.RemoveFaction(factionId);

            if (!removed)
            {
                player.SendErrorNotification("An error occured removing the faction.");
                return;
            }

            player.SendInfoNotification($"You have removed the faction successfully.");
        }

        [Command("ainvite", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Faction: Invites a player to a faction")]
        public static void AdminCommandInvite(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage($"Usage: /ainvite [IdorName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            if (targetPlayer.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not logged in.");
                return;
            }

            player.SetData("ainvite:targetCharacter", targetPlayer.GetClass().CharacterId);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<Faction> factionList = Faction.FetchFactions();

            foreach (Faction faction in factionList)
            {
                menuItems.Add(new NativeMenuItem(faction.Name));
            }

            NativeMenu menu = new NativeMenu("admin:faction:ainvite:showFactions", "Factions", "Select a Faction",
                menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void AdminInviteFactionSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            Faction selectedFaction = Faction.FetchFaction(option);

            if (selectedFaction == null)
            {
                player.SendErrorNotification("Faction not found.");
                return;
            }

            player.SetData("ainvite:targetFactionId", selectedFaction.Id);

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(selectedFaction.RanksJson);

            foreach (Rank factionRank in factionRanks)
            {
                menuItems.Add(new NativeMenuItem(factionRank.Name));
            }

            NativeMenu menu = new NativeMenu("admin:faction:ainvite:showRanks", "Ranks", "Select a Rank", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void AdminInviteRankSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("ainvite:targetCharacter", out int targetCharacterId);

            player.GetData("ainvite:targetFactionId", out int factionId);

            Faction selectedFaction = Faction.FetchFaction(factionId);

            if (selectedFaction == null)
            {
                player.SendErrorNotification("An error occurred fetching the faction.");
                return;
            }

            List<Rank> ranks = JsonConvert.DeserializeObject<List<Rank>>(selectedFaction.RanksJson);

            Rank selectedRank = ranks.FirstOrDefault(x => x.Name == option);

            if (selectedRank == null)
            {
                player.SendErrorNotification("An error occurred fetching the rank information.");
                return;
            }

            Context context = new Context();

            Models.Character targetCharacter = context.Character.Find(targetCharacterId);

            if (targetCharacter == null)
            {
                player.SendErrorNotification("An error occurred fetching the character information.");

                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(targetCharacter.FactionList);

            bool hasData = playerFactions.FirstOrDefault(x => x.Id == factionId) != null;

            if (hasData)
            {
                player.SendErrorNotification("This player is already in this faction.");

                return;
            }

            PlayerFaction newFaction = new PlayerFaction
            {
                Id = selectedFaction.Id,
                RankId = selectedRank.Id,
                Leader = false,
                DivisionId = 0,
            };

            playerFactions.Add(newFaction);

            targetCharacter.FactionList = JsonConvert.SerializeObject(playerFactions);

            player.SendInfoNotification(
                $"You've added {targetCharacter.Name} to the {selectedFaction.Name} faction with the rank of {selectedRank.Name}.");

            IPlayer? targetPlayer =
                Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().CharacterId == targetCharacterId);

            targetPlayer?.SendInfoNotification(
                $"You've been added to the {selectedFaction.Name} faction with the rank of {selectedRank.Name}.");

            context.SaveChanges();
        }

        [Command("createfaction", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Faction: Creates a faction")]
        public static void AdminCommandCreateFaction(IPlayer player)
        {
            player.FreezeInput(true);
            player.ChatInput(false);
            player.ShowCursor(true);
            player.Emit("admin:faction:showCreatePage");
        }

        public static void OnFactionCreateSubmit(IPlayer player, string factionName, string factionType,
            string factionSubType)
        {
            if (player == null) return;

            player.FreezeInput(false);
            player.ChatInput(true);
            player.ShowCursor(false);

            player.Emit("closeFactionPage");

            bool nameExists = Faction.FetchFaction(factionName) != null;

            if (nameExists)
            {
                player.SendErrorNotification("A faction already exists with this name.");
                return;
            }

            var factionTypes = factionType switch
            {
                "business" => FactionTypes.Business,
                "faction" => FactionTypes.Faction,
                _ => FactionTypes.Business
            };

            SubFactionTypes subFactionTypes = SubFactionTypes.None;

            if (factionTypes != FactionTypes.Business)
            {
                subFactionTypes = factionSubType switch
                {
                    "law" => SubFactionTypes.Law,
                    "medical" => SubFactionTypes.Medical,
                    "gov" => SubFactionTypes.Government,
                    "news" => SubFactionTypes.News,
                    _ => SubFactionTypes.None
                };
            }

            List<Rank> ranks = new List<Rank>
            {
                new Rank()
                {
                    Id = 1,
                    Name = "Rank1",
                    AddRanks = false,
                    Invite = false,
                    Promote = false,
                    Tow = false
                }
            };

            Faction newFaction = new Faction
            {
                Name = factionName,
                FactionType = factionTypes,
                SubFactionType = subFactionTypes,
                RanksJson = JsonConvert.SerializeObject(ranks),
                DivisionJson = JsonConvert.SerializeObject(new List<Division>())
            };

            Faction.AddFaction(newFaction);

            player.SendInfoNotification($"You've created a new faction with the name of {newFaction.Name}.");
        }

        [Command("setleader", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Faction: Sets a faction leader")]
        public static void AdminCommandSetLeader(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage($"/setleader [IdOrName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacter = context.Character.Find(targetPlayer.GetClass().CharacterId);

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned.");

                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(targetCharacter.FactionList);

            PlayerFaction activePlayerFaction =
                playerFactions.FirstOrDefault(x => x.Id == targetCharacter.ActiveFaction);

            if (activePlayerFaction == null)
            {
                player.SendErrorNotification("This player isn't in an active faction.");
                return;
            }

            playerFactions.Remove(activePlayerFaction);

            activePlayerFaction.Leader = !activePlayerFaction.Leader;

            playerFactions.Add(activePlayerFaction);

            targetCharacter.FactionList = JsonConvert.SerializeObject(playerFactions);

            context.SaveChanges();

            player.SendInfoNotification($"You've set {targetPlayer.Name} leader status to {activePlayerFaction.Leader} for faction id {activePlayerFaction.Id}.");

            Logging.AddToAdminLog(player, $"has set {targetPlayer.Name} leader status to {activePlayerFaction.Leader} for faction id {activePlayerFaction.Id}.");

            Logging.AddToCharacterLog(targetPlayer, $"has had their leader status set to {activePlayerFaction.Leader} for faction id {activePlayerFaction.Id} by {player.FetchAccount().Username}.");
        }

        #endregion Faction Management Commands

        #region Admin Reports

        [Command("ar", AdminLevel.Tester, true, "acceptreport", commandType: CommandType.Admin, description: "Report: Accepts a report")]
        public static void AdminCommandAcceptReport(IPlayer player, string idString = "")
        {
            if (idString == "")
            {
                player.SendSyntaxMessage("/ar [reportId]");
                return;
            }

            bool tryParse = int.TryParse(idString, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("You must enter a number.");
                return;
            }

            AdminReport adminReport = AdminHandler.AdminReports.FirstOrDefault(x => x.Id == id);

            if (adminReport == null)
            {
                player.SendErrorNotification("Report not found!");
                return;
            }

            IPlayer? targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x == adminReport.Player);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can not accept your own reports!");
                return;
            }

            AdminReportObject adminReportObject =
                AdminHandler.AdminReportObjects[AdminHandler.AdminReports.IndexOf(adminReport)];
            AdminHandler.AdminReportObjects.Remove(adminReportObject);

            AdminHandler.AdminReports.Remove(adminReport);
            targetPlayer.SendInfoNotification(
                $"Your report Id {adminReport.Id} has been accepted. Please await for them to contact you.");

            player.SendInfoNotification($"You have accepted report Id {adminReport.Id}. Message: {adminReport.Message}.");
            player.SendInfoNotification($"Player TP Id: {targetPlayer.GetPlayerId()}");

            var onlineAdmins = Alt.Server.GetPlayers()
                .Where(x => x.FetchAccount()?.AdminLevel >= AdminLevel.Tester).ToList();

            if (onlineAdmins.Any())
            {
                foreach (IPlayer onlineAdmin in onlineAdmins)
                {
                    onlineAdmin.SendAdminMessage(
                        $"Admin {player.FetchAccount()?.Username} has accepted report Id: {adminReport.Id}.");
                }
            }

            SignalR.RemoveReport(adminReportObject);

            DiscordHandler.SendMessageToReportsChannel(
                $"Admin {player.FetchAccount().Username} has accepted report Id {adminReport.Id}");

            Logging.AddToAdminLog(player,
                $"has accepted report Id {adminReport.Id} for character {targetPlayer.GetClass().Name}.");
        }

        [Command("dr", AdminLevel.Tester, true, "denyreport", commandType: CommandType.Admin, description: "Report: Denies a report")]
        public static void AdminCommandDenyReport(IPlayer player, string idString = "")
        {
            if (idString == "")
            {
                player.SendSyntaxMessage("/dr [reportId]");
                return;
            }

            bool tryParse = int.TryParse(idString, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("You must enter a number.");
                return;
            }

            AdminReport? adminReport = AdminHandler.AdminReports.FirstOrDefault(x => x.Id == id);

            if (adminReport == null)
            {
                player.SendErrorNotification("Report not found!");
                return;
            }

            IPlayer? targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x == adminReport.Player);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can not accept your own reports!");
                return;
            }
            AdminReportObject adminReportObject =
                AdminHandler.AdminReportObjects[AdminHandler.AdminReports.IndexOf(adminReport)];
            AdminHandler.AdminReportObjects.Remove(adminReportObject);

            AdminHandler.AdminReports.Remove(adminReport);
            targetPlayer.SendInfoNotification($"Your report Id: {adminReport.Id} has been denied.");

            player.SendInfoNotification($"You have declined report Id: {adminReport.Id}.");

            foreach (IPlayer onlineAdmin in Alt.Server.GetPlayers()
                .Where(x => x.FetchAccount()?.AdminLevel >= AdminLevel.Tester))
            {
                onlineAdmin.SendAdminMessage(
                    $"Admin {player.FetchAccount().Username} has denied report Id: {adminReport.Id}.");
            }

            SignalR.RemoveReport(adminReportObject);

            DiscordHandler.SendMessageToReportsChannel(
                $"Admin {player.FetchAccount().Username} has denied report Id {adminReport.Id}");

            Logging.AddToAdminLog(player,
                $"has denied report Id {adminReport.Id} for character {targetPlayer.GetClass().Name}.");
        }

        #endregion Admin Reports

        #region Kick Command

        [Command("kick", AdminLevel.Tester, onlyOne: true, commandType: CommandType.Admin, description: "Player: Kicks a player")]
        public static void AdminCommandKick(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/kick [IdOrName] [Reason]");
                return;
            }

            string[] argsSplit = args.Split(' ');

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(argsSplit[0]);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            string reason = string.Join(' ', argsSplit.Skip(1));

            if (reason.Length < 3)
            {
                player.SendErrorNotification("Your reason must be longer than 3 characters!");
                return;
            }

            AdminRecord newAdminRecord = new AdminRecord
            {
                AccountId = targetPlayer.GetClass().AccountId,
                CharacterId = targetPlayer.GetClass().CharacterId,
                RecordType = AdminRecordType.Kick,
                DateTime = DateTime.Now,
                Reason = reason,
                Time = 0,
                Admin = player.FetchAccount().Username
            };

            using Context context = new Context();

            context.AdminRecords.Add(newAdminRecord);

            context.SaveChanges();

            Logging.AddToCharacterLog(targetPlayer,
                $"has been kicked by {player.FetchAccount().Username}. Reason: {reason}");

            Logging.AddToAdminLog(player,
                $"has kicked {targetPlayer.GetClass().Name} (Character Id: {targetPlayer.GetClass().CharacterId}). Reason: {reason}");

            DiscordHandler.SendMessageToLogChannel(
                $"Admin {player.FetchAccount().Username} has kicked {targetPlayer.GetClass().Name}. Reason: {reason}");

            targetPlayer.Kick(reason);
        }

        #endregion Kick Command

        #region Ban Command

        [Command("ban", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Player: Bans a player for specified days")]
        public static void AdminCommandBan(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/ban [IdOrName] [Days] [Reason]");
                return;
            }

            string[] argsSplit = args.Split(' ');

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(argsSplit[0]);

            if (targetPlayer?.FetchAccount() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            string dayString = argsSplit[1].ToString();

            bool tryParseDay = int.TryParse(dayString, out int days);

            if (!tryParseDay)
            {
                player.SendErrorNotification("An error occurred fetching the days.");
                return;
            }

            string reason = string.Join(' ', argsSplit.Skip(2));

            if (reason.Length < 3)
            {
                player.SendErrorNotification("Your reason must be longer than 3 characters!");
                return;
            }

            Bans newBan = new Bans(targetPlayer.Ip, targetPlayer.SocialClubId.ToString(), targetPlayer.HardwareIdHash.ToString(), targetPlayer.HardwareIdExHash.ToString());

            AdminRecord newAdminRecord = new AdminRecord
            {
                AccountId = targetPlayer.GetClass().AccountId,
                CharacterId = targetPlayer.GetClass().CharacterId,
                RecordType = AdminRecordType.Ban,
                DateTime = DateTime.Now,
                Reason = reason,
                Time = days,
                Admin = player.FetchAccount().Username
            };

            using Context context = new Context();

            context.Bans.Add(newBan);

            context.AdminRecords.Add(newAdminRecord);

            Models.Account targetAccount = context.Account.Find(targetPlayer.GetClass().AccountId);

            targetAccount.Banned = true;

            targetAccount.UnBanTime = days == -1 ? DateTime.MaxValue : DateTime.Now.AddDays(days);

            context.SaveChanges();

            Logging.AddToCharacterLog(targetPlayer,
                $"has been banned by {player.FetchAccount().Username}. Reason: {reason}. Unban time: {targetAccount.UnBanTime}");

            Logging.AddToAdminLog(player,
                $"has banned {targetPlayer.GetClass().Name} (Character Id: {targetPlayer.GetClass().CharacterId}). Reason: {reason}. Unban time: {targetAccount.UnBanTime}");

            DiscordHandler.SendMessageToLogChannel(
                $"Admin {player.FetchAccount().Username} has banned {targetPlayer.GetClass().Name}. Reason: {reason}. Unban time: {targetAccount.UnBanTime}");

            targetPlayer.Kick(reason);
        }

        #endregion Ban Command

        #region AFix Vehicle

        [Command("afix", AdminLevel.Tester, commandType: CommandType.Admin, description: "Vehicle: Fixes a vehicle")]
        public static void AdminCommandAFix(IPlayer player)
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            player.Emit("FixVehicle", playerVehicle);

            byte count = playerVehicle.WheelsCount;

            for (byte i = 0; i <= count; i++)
            {
                playerVehicle.SetWheelBurst(i, false);
            }

            player.SendInfoNotification($"You've repaired the vehicle.");

            Logging.AddToAdminLog(player, $"has fixed vehicle id {playerVehicle.GetClass().Id}");
        }

        #endregion AFix Vehicle

        #region Health and Armor

        [Command("sethealth", AdminLevel.Administrator, true, commandType: CommandType.Admin, description: "Player: Sets a players health")]
        public static void AdminCommandSetHealth(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/sethealth [IdOrName] [Health]");
                return;
            }

            string[] argsSplit = args.Split(" ");

            if (argsSplit.Length != 2)
            {
                player.SendSyntaxMessage("/sethealth [IdOrName] [Health]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(argsSplit[0]);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            bool tryHealthParse = ushort.TryParse(argsSplit[1], out ushort health);

            if (!tryHealthParse)
            {
                player.SendSyntaxMessage("/sethealth [IdOrName] [Health]");
                return;
            }

            targetPlayer.Health = health;

            player.SendInfoNotification($"You've set {targetPlayer.GetClass().Name}'s health to {health}.");
        }

        [Command("setarmor", AdminLevel.Administrator, true, commandType: CommandType.Admin, description: "Player: Sets a players armor")]
        public static void AdminCommandSetArmor(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setarmor [IdOrName] [Armor]");
                return;
            }

            string[] argsSplit = args.Split(" ");

            if (argsSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setarmor [IdOrName] [Armor]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(argsSplit[0]);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            bool tryArmorParse = ushort.TryParse(argsSplit[1], out ushort armor);

            if (!tryArmorParse)
            {
                player.SendSyntaxMessage("/setarmor [IdOrName] [Armor]");
                return;
            }

            targetPlayer.Armor = armor;
            player.SendInfoNotification($"You've set {targetPlayer.GetClass().Name}'s armor to {armor}.");
        }

        #endregion Health and Armor

        [Command("deletecharacter", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Character: Deletes a character")]
        public static void AdminCommandDeleteCharacter(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/deletecharacter [CharacterName]");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacter =
                context.Character.FirstOrDefault(x => x.Name.ToLower().Contains(args.ToLower()));

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Unable to find the character.");
                return;
            }

            var playerVehicles = context.Vehicle.Where(x => x.OwnerId == targetCharacter.Id);

            foreach (Models.Vehicle playerVehicle in playerVehicles)
            {
                IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetVehicleId() == playerVehicle.Id);

                targetVehicle?.Remove();

                context.Vehicle.Remove(playerVehicle);
            }

            var playerProperties = context.Property.Where(x => x.OwnerId == targetCharacter.Id);

            foreach (Models.Property playerProperty in playerProperties)
            {
                playerProperty.OwnerId = 0;
                playerProperty.BuyinPaid = 0;
                playerProperty.PurchaseDateTime = DateTime.MinValue;
                playerProperty.VoucherUsed = false;
            }

            var inventoryTable = context.Inventory.Find(targetCharacter.InventoryID);

            context.Inventory.Remove(inventoryTable);

            var bankAccounts = context.BankAccount.Where(x => x.OwnerId == targetCharacter.Id);

            foreach (var bankAccount in bankAccounts)
            {
                context.BankAccount.Remove(bankAccount);
            }

            List<ApartmentComplexes> apartmentComplexes = context.ApartmentComplexes.ToList();

            foreach (ApartmentComplexes fetchApartmentComplex in apartmentComplexes)
            {
                List<Apartment> apartments =
                    JsonConvert.DeserializeObject<List<Apartment>>(fetchApartmentComplex.ApartmentList);

                List<Apartment> ownedApartments = apartments
                    .Where(x => x.Owner == targetCharacter.Id).ToList();

                if (!ownedApartments.Any()) continue;

                foreach (Apartment apartment in apartments)
                {
                    if (apartment.Owner != targetCharacter.Id) continue;

                    apartment.Owner = 0;
                    apartment.Locked = false;
                    apartment.KeyCode = Utility.GenerateRandomString(6);
                }

                fetchApartmentComplex.ApartmentList = JsonConvert.SerializeObject(apartments);
            }

            List<Models.Motel> motels = context.Motels.ToList();

            foreach (Models.Motel motel in motels)
            {
                List<MotelRoom> motelRooms =
                    JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

                foreach (var motelRoom in motelRooms)
                {
                    if (motelRoom.OwnerId != targetCharacter.Id) continue;

                    motelRoom.OwnerId = 0;
                    motelRoom.Locked = false;
                }

                motel.RoomList = JsonConvert.SerializeObject(motelRooms);
            }

            context.Character.Remove(targetCharacter);

            context.SaveChanges();

            player.SendInfoNotification($"You have deleted the character: {targetCharacter.Name}.");

            DiscordHandler.SendMessageToLogChannel(
                $"{player.FetchAccount().Username} has deleted the character {targetCharacter.Name} and all assets.");
        }

        [Command("deletevehicle", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Vehicle: Permanently deletes a vehicle")]
        public static void AdminCommandDeleteVehicle(IPlayer player)
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle!");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendInfoNotification($"Vehicle removed.");

                playerVehicle.Remove();
                return;
            }

            using Context context = new Context();

            Models.Vehicle dbVehicle = context.Vehicle.Find(vehicleData.Id);

            InventoryData dbInventory = context.Inventory.Find(dbVehicle.InventoryId);

            player.Vehicle.Remove();

            if (dbVehicle != null)
            {
                context.Vehicle.Remove(dbVehicle);
            }

            if (dbInventory != null)
            {
                context.Inventory.Remove(dbInventory);
            }

            context.SaveChanges();

            string username = player.FetchAccount().Username;

            player.SendInfoNotification($"You have deleted vehicle Id {vehicleData.Id} from the server.");

            Logging.AddToAdminLog(player, $"has removed vehicle Id {vehicleData.Id} from the database.");

            DiscordHandler.SendMessageToLogChannel(
                $"Admin {username} has removed vehicle Id {vehicleData.Id} from the server.");
        }

        [Command("createproperty", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Property: Creates a Property")]
        public static void AdminCommandCreateProperty(IPlayer player, string args = "")
        {
            if (args == "" || args.Length < 1)
            {
                player.SendSyntaxMessage("/createproperty [Property Number]");
                return;
            }

            player.SetData("admin:property:PropertyNumber", args);

            if (player.FetchAccount().AdminLevel >= AdminLevel.HeadAdmin)
            {
                List<NativeMenuItem> menuItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("House"),
                    new NativeMenuItem("General Store"),
                    new NativeMenuItem("Mod Shop"),
                    new NativeMenuItem("Clothing Store"),
                    new NativeMenuItem("Hair Salon"),
                    new NativeMenuItem("Tattoo"),
                    new NativeMenuItem("Surgeon"),
                    new NativeMenuItem("Key Smith"),
                    new NativeMenuItem("Gun Store"),
                };

                NativeMenu menu = new NativeMenu("admin:property:CreatePropertyTypeSelect", "Property", "Select a Property Type", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
            }
            else
            {
                List<NativeMenuItem> menuItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("House"),
                };

                NativeMenu menu = new NativeMenu("admin:property:CreatePropertyTypeSelect", "Property", "Select a Property Type", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
            }
        }

        public static void OnCreatePropertyTypeSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.SetData("admin:property:SelectedPropertyType", option);

            player.FetchLocation("admin:property:PropertyAddress");
        }

        public static void OnPropertyFetchLocation(IPlayer player, string streetName, string areaName)
        {
            player.SetData("admin:property:StreetName", streetName);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Interiors interior in Interiors.InteriorList.OrderBy(x => x.InteriorName))
            {
                menuItems.Add(new NativeMenuItem(interior.InteriorName));
            }

            NativeMenu menu = new NativeMenu("admin:property:SelectPropertyInterior", "Property", "Select an Interior", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectPropertyInterior(IPlayer player, string option)
        {
            if (option == "Close") return;

            Interiors selectedInterior = Interiors.InteriorList.FirstOrDefault(x => x.InteriorName == option);

            player.GetData("admin:property:PropertyNumber", out string propertyNumber);
            player.GetData("admin:property:SelectedPropertyType", out string typeString);
            player.GetData("admin:property:StreetName", out string streetName);

            PropertyType selectedPropertyType = PropertyType.House;

            switch (typeString)
            {
                case "House":
                    selectedPropertyType = PropertyType.House;
                    break;

                case "General Store":
                    selectedPropertyType = PropertyType.GeneralBiz;
                    break;

                case "Mod Shop":
                    selectedPropertyType = PropertyType.VehicleModShop;
                    break;

                case "Clothing Store":
                    selectedPropertyType = PropertyType.LowEndClothes;
                    break;

                case "Hair Salon":
                    selectedPropertyType = PropertyType.Hair;
                    break;

                case "Tattoo":
                    selectedPropertyType = PropertyType.Tattoo;
                    break;

                case "Surgeon":
                    selectedPropertyType = PropertyType.Surgeon;
                    break;

                case "Key Smith":
                    selectedPropertyType = PropertyType.KeySmith;
                    break;

                case "Gun Store":
                    selectedPropertyType = PropertyType.GunStore;
                    break;
            }

            Models.Property newProperty = new Models.Property
            {
                PropertyType = selectedPropertyType,
                BusinessName = "",
                Address = $"{propertyNumber} {streetName}",
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                InteriorName = selectedInterior.InteriorName,
                InteractionPoints = JsonConvert.SerializeObject(new List<PropertyInteractionPoint>()),
                ItemList = JsonConvert.SerializeObject(new List<GameItem>()),
                InventoryId = 0,
                Ipl = selectedInterior.Ipl,
                BlipId = 0,
                BlipColor = 0,
                Locked = false,
                Value = 0,
                OwnerId = 0,
                Key = null,
                GarageList = JsonConvert.SerializeObject(new List<PropertyGarage>()),
                Enterable = true,
                VoucherUsed = false,
                ExtDimension = player.Dimension,
                PurchaseDateTime = default,
                BuyinValue = 0,
                BuyinPaid = 0,
                PropList = JsonConvert.SerializeObject(new List<string>()),
                DoorPositions = JsonConvert.SerializeObject(new List<PropertyDoor>())
            };

            using Context context = new Context();

            context.Property.Add(newProperty);

            context.SaveChanges();

            player.SendInfoNotification($"You've created property Id {newProperty.Id} at {newProperty.Address}.");

            Logging.AddToAdminLog(player, $"has created a new property Id {newProperty.Id} at {newProperty.Address}");
        }

        [Command("deleteproperty", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Property: Deletes a property")]
        public static void AdminCommandDeleteProperty(IPlayer player)
        {
            try
            {
                Models.Property? nearbyProperty = Models.Property.FetchNearbyProperty(player, 5);

                if (nearbyProperty == null)
                {
                    player.SendErrorNotification("You're not near a property.");
                    return;
                }

                using Context context = new Context();

                Models.Property? propertyDb = context.Property.FirstOrDefault(x => x.Id == nearbyProperty.Id);

                if (propertyDb == null)
                {
                    player.SendErrorNotification("Property doesn't exist!");
                    return;
                }

                context.Property.Remove(propertyDb);

                context.SaveChanges();

                player.SendInfoNotification($"You have removed {propertyDb.Address} from the system.");
                Logging.AddToAdminLog(player, $"has removed property {propertyDb.Address} from the system.");

                LoadProperties.UnloadProperty(propertyDb);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("setblip", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Property: Sets a properties blip")]
        public static void AdminCommandSetBlip(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setblip [Blip Id] [Blip Color]");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length != 2)
            {
                player.SendSyntaxMessage("/setblip [Blip Id] [Blip Color]");
                return;
            }

            bool idTryParse = int.TryParse(split[0], out int blipId);
            bool colorTryParse = int.TryParse(split[1], out int blipColor);

            if (!idTryParse || !colorTryParse)
            {
                player.SendSyntaxMessage("/setblip [Blip Id] [Blip Color]");
                return;
            }

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearbyProperty == null)
            {
                player.SendErrorNotification("You must be near a property.");
                return;
            }

            if (nearbyProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You can't set a blip for this.");
                return;
            }

            using Context context = new Context();

            Models.Property nearbyPropertyDb = context.Property.Find(nearbyProperty.Id);

            nearbyPropertyDb.BlipId = blipId;
            nearbyPropertyDb.BlipColor = blipColor;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {nearbyPropertyDb.Address} blip to {blipId}, color to {blipColor}.");

            Logging.AddToAdminLog(player, $"has set {nearbyPropertyDb.Address} blip to {blipId}, color to {blipColor}.");

            LoadProperties.ReloadProperty(nearbyPropertyDb);
        }

        [Command("setenterable", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Property: Sets property as enterable")]
        public static void AdminCommandSetEnterable(IPlayer player)
        {
            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 5);

            if (nearbyProperty == null)
            {
                player.SendErrorNotification("You're not near a property.");
                return;
            }

            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(nearbyProperty.Id);

            propertyDb.Enterable = !propertyDb.Enterable;

            player.SendInfoNotification($"You've set {propertyDb.Address} enterable state to: {propertyDb.Enterable}.");

            context.SaveChanges();

            Logging.AddToAdminLog(player, $"has set {propertyDb.Address} enterable state to: {propertyDb.Enterable}.");
            LoadProperties.ReloadProperty(propertyDb);
        }

        [Command("setbusinessname", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Property: Adjusts the business name")]
        public static void AdminCommandSetBusinessName(IPlayer player, string args = "")
        {
            if (args == "" || args.Length < 5)
            {
                player.SendSyntaxMessage("/setbusinessname [Name (5 Min Chars)]");
                return;
            }

            Models.Property nearbyProperty = Models.Property.FetchNearbyProperty(player, 5);

            if (nearbyProperty == null)
            {
                player.SendErrorNotification("You're not near a property.");
                return;
            }

            using Context context = new Context();

            Models.Property propertyDb = context.Property.Find(nearbyProperty.Id);

            propertyDb.BusinessName = args;

            context.SaveChanges();

            player.SendInfoNotification($"You've set the business name for {propertyDb.Address} to {args}.");

            Logging.AddToAdminLog(player, $"has set the business name for {propertyDb.Address} to {args}.");

            LoadProperties.ReloadProperty(propertyDb);
        }

        [Command("addinteraction", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Property: Used to set additional interaction points")]
        public static void AdminCommandAddInteraction(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/addinteraction [Property Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int propertyId);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be numeric.");
                return;
            }

            using Context context = new Context();
            Models.Property property = context.Property.Find(propertyId);

            if (property == null)
            {
                player.SendErrorNotification("Unable to find a property by this id.");
                return;
            }

            List<PropertyInteractionPoint> interactionPoints =
                JsonConvert.DeserializeObject<List<PropertyInteractionPoint>>(property.InteractionPoints);

            interactionPoints.Add(new PropertyInteractionPoint
            {
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z
            });

            property.InteractionPoints = JsonConvert.SerializeObject(interactionPoints);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new point to property id {propertyId}.");
        }

        [Command("addintprop", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Property: Adds a interior prop")]
        public static void AdminCommandAddProp(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/addintprop [PropName]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter.InsideApartmentComplex > 0)
            {
                using Context context = new Context();

                ApartmentComplexes complex =
                    context.ApartmentComplexes.Find(playerCharacter.InsideApartmentComplex);

                if (complex == null)
                {
                    player.SendErrorNotification("An error occurred fetching the complex.");
                    return;
                }

                List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

                Apartment oldApartment = apartments.FirstOrDefault(x => x.Name == playerCharacter.InsideApartment);

                Apartment apartment = oldApartment;

                if (apartment == null)
                {
                    player.SendErrorNotification("An error occurred fetching an apartment.");
                    return;
                }

                List<string> propList = JsonConvert.DeserializeObject<List<string>>(apartment.PropList);

                if (propList.Contains(args))
                {
                    player.SendErrorNotification("This interior already has this prop.");
                    return;
                }

                propList.Add(args);

                apartment.PropList = JsonConvert.SerializeObject(propList);

                apartments.Remove(oldApartment);
                apartments.Add(apartment);

                complex.ApartmentList = JsonConvert.SerializeObject(apartments);

                context.SaveChanges();

                foreach (IPlayer? targetPlayer in Alt.Server.GetPlayers().Where(x => x.FetchCharacter()?.InsideApartment == apartment.Name && x.FetchCharacter()?.InsideApartmentComplex == complex.Id))
                {
                    targetPlayer.LoadInteriorProp(args);
                }

                player.SendInfoNotification($"You've added {args} to Apartment {apartment.Name}");
                return;
            }

            if (playerCharacter.InsideProperty > 0)
            {
                using Context context = new Context();

                Models.Property property = context.Property.Find(playerCharacter.InsideProperty);

                if (property == null)
                {
                    player.SendErrorNotification("You are not in a property.");
                    return;
                }

                List<string> propList = JsonConvert.DeserializeObject<List<string>>(property.PropList);

                if (propList.Contains(args))
                {
                    player.SendErrorNotification("This prop is already added.");
                    return;
                }

                propList.Add(args);

                property.PropList = JsonConvert.SerializeObject(propList);
                context.SaveChanges();

                foreach (IPlayer? targetPlayer in Alt.Server.GetPlayers().Where(x => x.FetchCharacter()?.InsideProperty == property.Id))
                {
                    targetPlayer.LoadInteriorProp(args);
                }

                player.SendInfoNotification($"You've added {args} to property {property.Address}.");
                return;
            }
        }

        [Command("removeintprop", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Property: Removes a interior prop")]
        public static void AdminCommandRemoveProp(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/removeintprop [PropName]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter.InsideApartmentComplex > 0)
            {
                using Context context = new Context();

                ApartmentComplexes complex =
                    context.ApartmentComplexes.Find(playerCharacter.InsideApartmentComplex);

                if (complex == null)
                {
                    player.SendErrorNotification("An error occurred fetching the complex.");
                    return;
                }

                List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

                Apartment apartment = apartments.FirstOrDefault(x => x.Name == playerCharacter.InsideApartment);

                Apartment oldApartment = apartment;

                if (apartment == null)
                {
                    player.SendErrorNotification("An error occurred fetching an apartment.");
                    return;
                }

                List<string> propList = JsonConvert.DeserializeObject<List<string>>(apartment.PropList);

                if (!propList.Contains(args))
                {
                    player.SendErrorNotification("This interior doesn't have this prop.");
                    return;
                }

                propList.Remove(args);

                apartment.PropList = JsonConvert.SerializeObject(propList);

                apartments.Remove(oldApartment);
                apartments.Add(apartment);

                complex.ApartmentList = JsonConvert.SerializeObject(apartments);

                context.SaveChanges();

                player.SendInfoNotification($"You've removed {args} from Apartment {apartment.Name}");

                foreach (IPlayer? targetPlayer in Alt.Server.GetPlayers().Where(x => x.FetchCharacter()?.InsideApartment == apartment.Name && x.FetchCharacter()?.InsideApartmentComplex == complex.Id))
                {
                    targetPlayer.UnloadInteriorProp(args);
                }

                return;
            }

            if (playerCharacter.InsideProperty > 0)
            {
                using Context context = new Context();

                Models.Property property = context.Property.Find(playerCharacter.InsideProperty);

                if (property == null)
                {
                    player.SendErrorNotification("You are not in a property.");
                    return;
                }

                List<string> propList = JsonConvert.DeserializeObject<List<string>>(property.PropList);

                if (!propList.Contains(args))
                {
                    player.SendErrorNotification("This prop isn't here.");
                    return;
                }

                propList.Remove(args);

                property.PropList = JsonConvert.SerializeObject(propList);

                context.SaveChanges();

                player.SendInfoNotification($"You've removed {args} from property {property.Address}.");

                foreach (IPlayer? targetPlayer in Alt.Server.GetPlayers().Where(x => x.FetchCharacter()?.InsideProperty == property.Id))
                {
                    targetPlayer.UnloadInteriorProp(args);
                }

                return;
            }
        }

        [Command("setprice", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "Property: Used to set property prices")]
        public static void AdminCommandSetPrice(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setprice [Price]");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You are not near a property.");
                return;
            }

            bool tryParse = int.TryParse(args, out int value);

            if (!tryParse)
            {
                player.SendErrorNotification("Value must be numeric");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.Value = value;

            context.SaveChanges();

            LoadProperties.ReloadProperty(property);

            player.SendInfoNotification($"You've set {property.Address} value to {value:C}");

            Logging.AddToAdminLog(player, $"has set property id {property.Id} value to {value}");

            DiscordHandler.SendMessageToLogChannel($"Staff {player.FetchAccount().Username} has set property id {property.Id} to {value:C}.");
        }

        [Command("additem", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Property: Used to add an item to the property.")]
        public static void AdminCommandAddItem(IPlayer player, string args = "")
        {
            // /additem ITEM_DIGITAL_CAMERA

            if (args == "")
            {
                player.SendSyntaxMessage($"/additem [ItemId] - Available on the forums");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You aren't nearby a property.");
                return;
            }

            if (nearestProperty.PropertyType != PropertyType.GeneralBiz)
            {
                player.SendErrorNotification("You must be at a general store to add in items.");
                return;
            }

            List<GameItem> gameItems = JsonConvert.DeserializeObject<List<GameItem>>(nearestProperty.ItemList);

            bool containsId = gameItems.Any(x => x.ID == args);

            if (containsId)
            {
                player.SendErrorNotification("This store already has this product.");
                return;
            }

            GameItem newGameItem = GameWorld.GetGameItem(args);

            if (newGameItem == null)
            {
                player.SendErrorNotification("This item doesn't exist.");
                return;
            }

            using Context context = new Context();

            gameItems.Add(newGameItem);

            Models.Property editProperty = context.Property.Find(nearestProperty.Id);

            editProperty.ItemList = JsonConvert.SerializeObject(gameItems);

            context.SaveChanges();

            player.SendInfoNotification($"You have added in {newGameItem.Name} into {nearestProperty.BusinessName}");

            Logging.AddToAdminLog(player, $"has added item {newGameItem.ID} into property id {nearestProperty.Id}");
        }

        [Command("removeitem", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Property: Removes an item from the property.")]
        public static void AdminCommandRemoveItem(IPlayer player)
        {
            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You're not nearby a property.");
                return;
            }

            if (nearestProperty.PropertyType != PropertyType.GeneralBiz)
            {
                player.SendErrorNotification("You must be by a general store.");
                return;
            }

            List<GameItem> gameItems = JsonConvert.DeserializeObject<List<GameItem>>(nearestProperty.ItemList);

            if (!gameItems.Any())
            {
                player.SendErrorNotification("There are no items here.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (GameItem gameItem in gameItems)
            {
                menuItems.Add(new NativeMenuItem(gameItem.Name, gameItem.Description));
            }

            NativeMenu menu = new NativeMenu("admin:property:removeItem", "Items", "Select an item to remove", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnPropertyRemoveItemSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You're not nearby a property.");
                return;
            }

            if (nearestProperty.PropertyType != PropertyType.GeneralBiz)
            {
                player.SendErrorNotification("You must be by a general store.");
                return;
            }

            List<GameItem> gameItems = JsonConvert.DeserializeObject<List<GameItem>>(nearestProperty.ItemList);

            if (!gameItems.Any())
            {
                player.SendErrorNotification("There are no items here.");
                return;
            }

            gameItems.RemoveAt(index);

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.ItemList = JsonConvert.SerializeObject(gameItems);

            context.SaveChanges();

            player.SendInfoNotification($"You've removed {option} from {nearestProperty.BusinessName}.");

            Logging.AddToAdminLog(player, $"has removed item {option} from property id {nearestProperty.Id}.");
        }

        [Command("reloadradio", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Other: Reloads radio stations")]
        public static void AdminCommandReloadRadio(IPlayer player)
        {
            AudioHandler.LoadRadioStations();

            player.SendInfoNotification($"Reloaded {AudioHandler.StationList.Count} stations");
        }

        [Command("aweapon", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Character: Gives them weapons")]
        public static void AdminCommandAGiveWeapon(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/agiveweapon [IdOrName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            player.SetData("admin:weapon:giveWeaponTo", targetPlayer.GetClass().Name);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<GameItem> weaponItems = GameWorld.GameItems.Where(x => x.ID.Contains("ITEM_WEAPON")).ToList();

            foreach (GameItem weaponItem in weaponItems)
            {
                menuItems.Add(new NativeMenuItem(weaponItem.Name, weaponItem.Description));
            }

            NativeMenu menu = new NativeMenu("admin:weapon:selectWeapon", "Weapons", "Select a Weapon", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnAdminSelectWeapon(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.SetData("admin:weapon:selectedWeaponName", option);

            List<string> stringList = new List<string>();

            for (int i = 1; i <= 100; i++)
            {
                stringList.Add(i.ToString());
            }

            List<NativeListItem> listItems = new List<NativeListItem>{
                new NativeListItem{Title = "Quantity", StringList = stringList}};

            NativeMenu menu = new NativeMenu("admin:weapon:selectQuantity", "Weapon", "Select a Quantity")
            {
                ListMenuItems = listItems,
                ListTrigger = "admin:weapon:onWeaponQuantityChange",
            };

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("admin:weapon:quantity", 1);
        }

        public static void OnAdminWeaponQuantityChange(IPlayer player, string newQuantity)
        {
            int quantity = Convert.ToInt32(newQuantity);

            player.SetData("admin:weapon:quantity", quantity);
        }

        public static void OnAdminWeaponQuantitySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("admin:weapon:quantity", out int quantity);

            player.GetData("admin:weapon:selectedWeaponName", out string selectedWeaponName);

            player.GetData("admin:weapon:giveWeaponTo", out string targetName);

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(targetName);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            if (targetInventory == null)
            {
                player.SendErrorNotification("Target Inventory is not found.");
                return;
            }

            GameItem selectedWeaponItem = GameWorld.GameItems.Where(x => x.ID.Contains("ITEM_WEAPON")).FirstOrDefault(x => x.Name == selectedWeaponName);

            if (selectedWeaponItem == null)
            {
                player.SendErrorNotification("Unable to find the selected item.");
                return;
            }

            WeaponInfo newWeaponInfo = new WeaponInfo(0, false, targetPlayer.GetClass().Name);

            bool itemAdded = targetInventory.AddItem(selectedWeaponItem.ID.Contains("AMMO")
                ? new InventoryItem(selectedWeaponItem.ID, selectedWeaponItem.Name, $"{quantity}", 1)
                : new InventoryItem(selectedWeaponItem.ID, selectedWeaponItem.Name, JsonConvert.SerializeObject(newWeaponInfo), quantity));

            if (!itemAdded)
            {
                player.SendErrorNotification("Target's inventory is full!");
                return;
            }

            string adminName = player.FetchAccount().Username;

            player.SendInfoNotification($"You've given {targetName} {selectedWeaponItem.Name} x {quantity}");

            targetPlayer.SendWeaponMessage($"You've been given {selectedWeaponItem.Name} x {quantity} by {adminName}.");

            Logging.AddToAdminLog(player, $"has given {targetName} {selectedWeaponItem.Name} x {quantity}");

            Logging.AddToCharacterLog(targetPlayer, $"has been given {selectedWeaponItem.Name} x {quantity} by {adminName}.");
        }

        [Command("adrugs", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Character: Gives a player drugs")]
        public static void AdminCommandGiveDrugs(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/adrugs [IdorName] [Quantity]");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length != 2)
            {
                player.SendSyntaxMessage("/adrugs [IdorName] [Quantity]");
                return;
            }

            bool tryQuantityParse = double.TryParse(split[1], out double quantity);

            if (!tryQuantityParse || quantity == 0)
            {
                player.SendErrorNotification("Invalid Quantity.");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(split[0]);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            player.SetData("admin:drugs:GiveTo", targetPlayer.GetPlayerId());
            player.SetData("admin:drugs:Quantity", quantity);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<GameItem> drugItems = GameWorld.GameItems.Where(x => x.ID.Contains("ITEM_DRUG")).ToList();

            foreach (GameItem drugItem in drugItems)
            {
                menuItems.Add(new NativeMenuItem(drugItem.Name, drugItem.Description));
            }

            NativeMenu menu = new NativeMenu("admin:drugs:SelectedDrug", "Drugs", "Select a Marijuana", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectedDrug(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("admin:drugs:GiveTo", out int targetPlayerId);

            player.GetData("admin:drugs:Quantity", out double quantity);

            GameItem selectedGameItem = GameWorld.GameItems.FirstOrDefault(x => x.Name == option);

            if (selectedGameItem == null)
            {
                player.SendErrorNotification("Unable to find the selected item.");
                return;
            }

            IPlayer? targetPlayer = Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null)
                .FirstOrDefault(x => x.GetPlayerId() == targetPlayerId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find the target player.");
                return;
            }

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            targetInventory.AddItem(new InventoryItem(selectedGameItem.ID, selectedGameItem.Name, "", quantity));

            string adminName = player.FetchAccount().Username;

            player.SendInfoNotification($"You've given {targetPlayer.GetClass().Name} {selectedGameItem.Name} x {quantity}.");

            targetPlayer.SendInfoNotification($"You've been given {selectedGameItem.Name} x {quantity} by {adminName}.");

            Logging.AddToAdminLog(player, $"has given {targetPlayer.GetClass().Name} {selectedGameItem.Name} x {quantity}.");

            Logging.AddToCharacterLog(targetPlayer, $"has been given {selectedGameItem.Name} x {quantity} by {adminName}.");
        }

        [Command("astats", AdminLevel.Tester, onlyOne: true, commandType: CommandType.Admin, description: "Player: View a players stats")]
        public static void AdminCommandAStats(IPlayer player, string nameorid = "")
        {
            if (nameorid == "")
            {
                player.SendSyntaxMessage($"/astats [NameOrId]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(nameorid);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            if (targetPlayer.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not logged in.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            int bankAccountCount = BankAccount.FindCharacterBankAccounts(targetCharacter).Count;

            Faction activeFaction = Faction.FetchFaction(targetCharacter.ActiveFaction);

            List<Models.Motel> motelList = new List<Models.Motel>();

            foreach (MotelObject motelObject in MotelHandler.MotelObjects)
            {
                motelList.Add(motelObject.Motel);
            }

            List<MotelRoom> motelRooms = new List<MotelRoom>();

            foreach (Models.Motel motel in motelList)
            {
                List<MotelRoom> motelRoomList = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

                if (motelRoomList.Any())
                {
                    foreach (MotelRoom motelRoom in motelRoomList)
                    {
                        if (motelRoom.OwnerId == targetCharacter.Id)
                        {
                            motelRooms.Add(motelRoom);
                        }
                    }
                }
            }

            List<AdminRecord> playerAdminRecords = AdminRecord.FetchAdminRecords(targetCharacter.OwnerId);
            int banCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Ban);
            int kickCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Kick);
            int jailCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Jail);
            int warnCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Warning);

            player.SendStatsMessage($"Showing stats for {targetCharacter.Name}");
            player.SendStatsMessage($"Username: {targetPlayer.FetchAccount().Username}, Playtime: {targetCharacter.TotalHours}:{targetCharacter.TotalMinutes}");
            player.SendStatsMessage($"Account Id: {targetCharacter.OwnerId}, Character Id: {targetCharacter.Id}, Inventory Id: {targetCharacter.InventoryID}");
            player.SendStatsMessage($"Cash: {targetCharacter.Money:C}, Dimension: {targetPlayer.Dimension}, Next Payday Earning: {targetCharacter.PaydayAmount:C}");
            player.SendStatsMessage($"Active Number: {targetCharacter.ActivePhoneNumber}, Payday Account: {targetCharacter.PaydayAccount}, Bank Accounts: {bankAccountCount}");
            if (activeFaction != null)
            {
                PlayerFaction activePlayerFaction = JsonConvert
                    .DeserializeObject<List<PlayerFaction>>(targetCharacter.FactionList)
                    .FirstOrDefault(x => x.Id == targetCharacter.ActiveFaction);

                Rank playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                    .FirstOrDefault(x => x.Id == activePlayerFaction?.RankId);

                player.SendStatsMessage($"Active Faction: {activeFaction.Name}, Rank: {playerRank?.Name}");
            }

            if (motelRooms.Any())
            {
                player.SendStatsMessage($"Motel: {motelList.FirstOrDefault(x => x.Id == motelRooms.FirstOrDefault()?.MotelId)?.Name} - Room {motelRooms.FirstOrDefault()?.Id}.");
            }

            player.SendStatsMessage(
                $"Bans: {banCount}, Kicks: {kickCount}, Jails: {jailCount}, Warnings: {warnCount}, AFK Kicks: {targetPlayer.FetchAccount().AfkKicks}.");

            if (player.FetchAccount().AdminLevel > AdminLevel.HeadAdmin)
            {
                List<string> characterNamesList = Models.Character.FetchCharacterNames(targetPlayer.FetchAccount());

                string characterNames = string.Join(", ", characterNamesList);

                player.SendStatsMessage($"Characters: {characterNames}");
            }
        }

        #region Delivery Job

        [Command("adddeliverypoint", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "/adddeliverypoint [Type] [WarehouseId] [Name]")]
        public static void AdminCommandAddDeliveryPoint(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/adddeliverypoint [Type] [WarehouseId] [Name]");
                return;
            }

            string[] splitArgs = args.Split(' ');

            if (splitArgs.Length < 3)
            {
                player.SendSyntaxMessage("/adddeliverypoint [Type] [WarehouseId] [Name]");
                return;
            }

            bool typeParse = int.TryParse(splitArgs[0], out int pointType);
            bool idParse = int.TryParse(splitArgs[1], out int warehouseId);

            if (!typeParse || !idParse)
            {
                player.SendSyntaxMessage("/adddeliverypoint [Type] [WarehouseId] [Name]");
                return;
            }

            if (pointType < 0 || pointType > 1)
            {
                player.SendErrorNotification("Point types can only be 0 (Pickup) or 1 (DropOff)");
                return;
            }

            if (warehouseId < 0)
            {
                player.SendErrorNotification("Warehouse Id can't be less than 0!");
                return;
            }

            DeliveryPointType deliveryPointType = (DeliveryPointType)pointType;

            if (deliveryPointType == DeliveryPointType.DropOff)
            {
                if (warehouseId == 0)
                {
                    player.SendErrorNotification("You must set a warehouse id for this type.");
                    return;
                }
            }

            string name = string.Join(" ", splitArgs.Skip(2));

            DeliveryPoint newDeliveryPoint = new DeliveryPoint
            {
                Id = 0,
                Name = name,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                PointType = deliveryPointType,
                WarehouseId = warehouseId,
                CostPerItem = 0
            };

            using Context context = new Context();

            if (context.DeliveryPoint.Any(x => x.Name == name))
            {
                player.SendErrorNotification("A delivery point already contains this name!");
                return;
            }

            context.DeliveryPoint.Add(newDeliveryPoint);

            context.SaveChanges();

            player.SendInfoNotification($"New delivery point added by the name: {newDeliveryPoint.Name}.");

            DeliveryHandler.LoadDeliveryPoint(newDeliveryPoint);

            Logging.AddToAdminLog(player, $"has created a new delivery point. Name: {newDeliveryPoint.Name}, Id: {newDeliveryPoint.Id}, Type: {newDeliveryPoint.PointType.ToString()}");
        }

        [Command("setdeliveryprice", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "/setdeliveryprice [Id] [Price]")]
        public static void AdminCommandSetDeliveryPrice(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setdeliveryprice [Id] [Price]");
                return;
            }

            string[] stringSplit = args.Split(" ");

            if (stringSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setdeliveryprice [Id] [Price]");
                return;
            }

            bool idParse = int.TryParse(stringSplit[0], out int id);

            bool priceParse = double.TryParse(stringSplit[1], out double price);

            if (!idParse || !priceParse)
            {
                player.SendErrorNotification("Parameters must be number only.");
                return;
            }

            using Context context = new Context();

            DeliveryPoint deliveryPoint = context.DeliveryPoint.Find(id);

            if (deliveryPoint == null)
            {
                player.SendErrorNotification("Invalid Delivery Point Id.");
                return;
            }

            deliveryPoint.CostPerItem = (float)price;
            context.SaveChanges();

            player.SendInfoNotification($"You've set the delivery point {deliveryPoint.Name}'s price per item to {price:C}");

            Logging.AddToAdminLog(player, $"has adjusted delivery point id: {deliveryPoint.Id} cost per item to {price}");
        }

        [Command("addwarehouse", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "/addwarehouse [Type] [Name]")]
        public static void AdminCommandAddWarehouse(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage($"/addwarehouse [Type] [Name]");
                return;
            }

            string[] argsSplit = args.Split(' ');

            if (argsSplit.Length < 2)
            {
                player.SendSyntaxMessage($"/addwarehouse [Type] [Name]");
                return;
            }

            bool typeParse = int.TryParse(argsSplit[0], out int type);

            if (!typeParse)
            {
                player.SendErrorNotification("Type must be a number. 0 - None");
                return;
            }

            string name = string.Join(' ', argsSplit.Skip(1));

            Warehouse newWarehouse = new Warehouse
            {
                Id = 0,
                Name = name,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                Type = (WarehouseType)type,
                Products = 0,
                MaxProducts = 0,
                MinPrice = 0,
                MaxPrice = 0
            };

            using Context context = new Context();

            context.Warehouse.Add(newWarehouse);

            context.SaveChanges();

            WarehouseHandler.LoadWarehouse(newWarehouse);

            player.SendInfoNotification($"You've created a new warehouse called: {newWarehouse}.");
            player.SendInfoNotification($"You can now use /setmaxproducts, /setminprice & /setmaxprice");

            Logging.AddToAdminLog(player, $"has created a new warehouse called {newWarehouse.Name}. Id: {newWarehouse.Id}");
        }

        [Command("setmaxproducts", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "/setmaxproducts [Id] [MaxProducts]")]
        public static void AdminCommandSetMaxProducts(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setmaxproducts [Id] [MaxProducts]");
                return;
            }

            string[] stringSplit = args.Split(' ');

            if (stringSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setmaxproducts [Id] [MaxProducts]");
                return;
            }

            bool idParse = int.TryParse(stringSplit[0], out int id);
            bool maxParse = double.TryParse(stringSplit[1], out double maxProducts);

            if (!idParse || !maxParse)
            {
                player.SendErrorNotification("Parameters must be number only.");
                return;
            }

            using Context context = new Context();

            Warehouse warehouse = context.Warehouse.Find(id);

            if (warehouse == null)
            {
                player.SendErrorNotification("Unable to find a warehouse by that type.");
                return;
            }

            warehouse.MaxProducts = maxProducts;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {warehouse.Name}'s max products to: {maxProducts}.");

            Logging.AddToAdminLog(player, $"has adjusted warehouse id {warehouse.Id} max products to {maxProducts}");
        }

        [Command("setminprice", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "/setminprice [Id] [MinPrice]")]
        public static void AdminCommandSetMinPrice(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setminprice [Id] [MinPrice]");
                return;
            }

            string[] stringSplit = args.Split(' ');

            if (stringSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setminprice [Id] [MinPrice]");
                return;
            }

            bool idParse = int.TryParse(stringSplit[0], out int id);
            bool priceParse = double.TryParse(stringSplit[1], out double minPrice);

            if (!idParse || !priceParse)
            {
                player.SendErrorNotification("Parameters must be number only.");
                return;
            }

            using Context context = new Context();

            Warehouse warehouse = context.Warehouse.Find(id);

            if (warehouse == null)
            {
                player.SendErrorNotification("Unable to find a warehouse by that type.");
                return;
            }

            warehouse.MinPrice = minPrice;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {warehouse.Name}'s min price to: {minPrice:C}.");

            Logging.AddToAdminLog(player, $"has adjusted warehouse id {warehouse.Id} min price to {minPrice}");
        }

        [Command("setmaxprice", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "/setmaxprice [Id] [MaxPrice]")]
        public static void AdminCommandSetMaxPrice(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setmaxprice [Id] [MaxPrice]");
                return;
            }

            string[] stringSplit = args.Split(' ');

            if (stringSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setmaxprice [Id] [MaxPrice]");
                return;
            }

            bool idParse = int.TryParse(stringSplit[0], out int id);
            bool priceParse = double.TryParse(stringSplit[1], out double maxPrice);

            if (!idParse || !priceParse)
            {
                player.SendErrorNotification("Parameters must be number only.");
                return;
            }

            using Context context = new Context();

            Warehouse warehouse = context.Warehouse.Find(id);

            if (warehouse == null)
            {
                player.SendErrorNotification("Unable to find a warehouse by that type.");
                return;
            }

            warehouse.MaxPrice = maxPrice;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {warehouse.Name}'s max price to: {maxPrice:C}.");

            Logging.AddToAdminLog(player, $"has adjusted warehouse id {warehouse.Id} max price to {maxPrice}");
        }

        #endregion Delivery Job

        [Command("gotograffiti", AdminLevel.Tester, true, "", CommandType.Admin,
            "Graffiti: Teleport to a Graffiti Position /gotograffiti [Id]")]
        public static void AdminCommandGotoGraffiti(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/gotograffiti [Id]");
                return;
            }

            bool idParse = int.TryParse(args, out int id);

            if (!idParse)
            {
                player.SendErrorNotification("Parameters are number only.");
                return;
            }

            Models.Graffiti graffiti = Models.Graffiti.FetchGraffitis().FirstOrDefault(x => x.Id == id);

            if (graffiti == null)
            {
                player.SendErrorNotification("Unable to find a graffiti by this Id.");
                return;
            }

            Position graffitiPosition = GraffitiHandler.FetchGraffitiPosition(graffiti);

            player.Position = graffitiPosition;
            player.Dimension = 0;

            player.SendInfoNotification($"Teleported you to Graffiti Id: {graffiti.Id}.");
        }

        [Command("graffitilist", AdminLevel.Tester, commandType: CommandType.Admin,
            description: "Graffiti: Display a list of Graffiti's")]
        public static void AdminCommandShowGraffitiList(IPlayer player)
        {
            List<Models.Graffiti> graffitis = Models.Graffiti.FetchGraffitis();

            if (!graffitis.Any())
            {
                player.SendErrorNotification("There are no graffiti's");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Models.Graffiti graffiti in graffitis)
            {
                menuItems.Add(new NativeMenuItem(graffiti.Text, $"Id: {graffiti.Id}"));
            }

            NativeMenu menu = new NativeMenu("admin:graffiti:OnSelectGraffiti", "Graffiti's", "Select a Graffiti", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectGraffiti(IPlayer player, string option)
        {
            if (option == "Close") return;

            Models.Graffiti selectedGraffiti = Models.Graffiti.FetchGraffitis().FirstOrDefault(x => x.Text == option);

            if (selectedGraffiti == null)
            {
                player.SendErrorNotification("An error occurred fetching the graffiti");
                return;
            }

            Position graffitiPosition = GraffitiHandler.FetchGraffitiPosition(selectedGraffiti);
            player.Position = graffitiPosition;
            player.Dimension = 0;

            player.SendInfoNotification($"Teleported you to Graffiti Id: {selectedGraffiti.Id}.");
        }

        [Command("changeinterior", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Property: Used to change interiors")]
        public static void AdminCommandChangeInterior(IPlayer player)
        {
            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 3f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You're not near any properties.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            List<Interiors> interiorList = Interiors.InteriorList;

            List<Interiors> orderedList = interiorList.OrderBy(x => x.InteriorName).ToList();

            foreach (Interiors interior in orderedList)
            {
                menuItems.Add(!string.IsNullOrEmpty(interior.Description)
                    ? new NativeMenuItem(interior.InteriorName, interior.Description)
                    : new NativeMenuItem(interior.InteriorName));
            }

            NativeMenu menu = new NativeMenu("admin:property:SelectInterior", "Interiors", "Select an Interior", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnChangeInteriorSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            List<Interiors> interiorList = Interiors.InteriorList;

            List<Interiors> orderedList = interiorList.OrderBy(x => x.InteriorName).ToList();

            Interiors selectedInterior = orderedList[index];

            if (selectedInterior == null)
            {
                player.SendErrorNotification("An error occurred fetching the interior.");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 3f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You're not near any properties.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.Ipl = selectedInterior.Ipl;
            property.InteriorName = selectedInterior.InteriorName;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {property.Address}'s interior to {selectedInterior.InteriorName}.");

            Logging.AddToAdminLog(player, $"has set property id {property.Id}'s interior to {selectedInterior.InteriorName}.");
        }

        [Command("pstats", AdminLevel.Administrator, commandType: CommandType.Admin,
            description: "Property: Used to view property stats.")]
        public static void AdminCommandViewPStats(IPlayer player)
        {
            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("Property not founded.");
                return;
            }

            player.SendInfoMessage($"Property Id: {nearestProperty.Id}, Interior: {nearestProperty.InteriorName}");
            player.SendInfoMessage($"Address: {nearestProperty.Address}, Price: {nearestProperty.Value:C}");
            if (Models.Character.GetCharacter(nearestProperty.OwnerId) != null)
            {
                player.SendInfoMessage($"Owner: {Models.Character.GetCharacter(nearestProperty.OwnerId).Name}");
                player.SendInfoMessage($"Purchase Date: {nearestProperty.PurchaseDateTime}");
            }

            if (nearestProperty.PropertyType != PropertyType.House)
            {
                player.SendInfoMessage($"Last Set Active: {nearestProperty.LastSetActive}");
            }
        }

        [Command("setproducts", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Property: Used to set the products")]
        public static void AdminCommandSetProducts(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setproducts [Amount]");
                return;
            }

            bool tryParse = int.TryParse(args, out int products);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be number.");
                return;
            }

            if (products < 0 || products > 500)
            {
                player.SendErrorNotification("You can only set between 0-500.");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null || nearestProperty.PropertyType == PropertyType.House)
            {
                player.SendErrorNotification("You must be by a business.");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            property.Products = products;

            context.SaveChanges();

            player.SendInfoNotification($"You've set {property.BusinessName} products to {products}.");

            Logging.AddToAdminLog(player, $"Has set property id {property.Id} products to {products}");
        }

        [Command("aengine", AdminLevel.Administrator, commandType: CommandType.Admin,
            description: "Vehicle: Used to start an engine")]
        public static void AdminCommandEngine(IPlayer player)
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification($"You must be in a vehicle.");
                return;
            }

            if (player.Vehicle.GetClass().FuelLevel == 0)
            {
                player.SendErrorNotification("This vehicle has no fuel!");
                return;
            }

            player.Vehicle.EngineOn = !player.Vehicle.EngineOn;

            using Context context = new Context();
            var vehicleDb = context.Vehicle.Find(player.Vehicle.GetVehicleId());

            if (vehicleDb == null) return;

            vehicleDb.Engine = player.Vehicle.EngineOn;

            vehicleDb.Engine = player.Vehicle.EngineOn;

            Logging.AddToAdminLog(player, $"Has set vehicle Id {vehicleDb.Id} engine status to {vehicleDb.Engine}");

            context.SaveChanges();
        }

        [Command("vehicle", AdminLevel.Administrator, true, commandType: CommandType.Admin, description: "Vehicle: Used to spawn a temporary vehicle.")]
        public static async void VehicleTest(IPlayer player, string model = "")
        {
            if (model == "")
            {
                player.SendSyntaxMessage("/vehicle [Model]");
                return;
            }

            uint hash = Alt.Hash(model);

            IVehicle? temporaryVehicle = Alt.Server.CreateVehicle(hash, player.Position.Around(2f), player.Rotation);

            if (temporaryVehicle == null)
            {
                player.SendErrorNotification("Unable to spawn!");
                return;
            }

            temporaryVehicle.Dimension = player.Dimension;

            temporaryVehicle.GetClass().Id = AdminHandler.NextAdminSpawnedVehicleId;

            AdminHandler.NextAdminSpawnedVehicleId -= 1;

            player.SendInfoNotification($"Spawned a vehicle with the id of {temporaryVehicle.GetClass().Id}.");
        }

        [Command("gotovehicle", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "Vehicle: Used to teleport to vehicles.")]
        public static void AdminCommandGotoVehicle(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/gotovehicle [Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be number.");
                return;
            }

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == id);

            if (targetVehicle == null)
            {
                player.SendErrorNotification("Unable to find a vehicle by this id.");
                return;
            }

            player.Position = targetVehicle.Position.Around(1f);
            player.Dimension = targetVehicle.Dimension;

            player.SendInfoNotification($"Teleporting you to vehicle id {id}.");

            Logging.AddToAdminLog(player, $"has teleported to vehicle id {id}.");
        }

        [Command("bringvehicle", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "Vehicle: Used to bring vehicles to you.")]
        public static void AdminCommandBringVehicle(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/bringvehicle [Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be number.");
                return;
            }

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == id);

            if (targetVehicle == null)
            {
                player.SendErrorNotification("Unable to find a vehicle by this id.");
                return;
            }

            targetVehicle.Position = player.Position.Around(2f);
            targetVehicle.Dimension = player.Dimension;

            player.SendInfoNotification($"Bringing vehicle {id} to you.");

            Logging.AddToAdminLog(player, $"has teleported vehicle id {id} to them.");
        }

        [Command("acceptad", AdminLevel.Tester, onlyOne: true, commandType: CommandType.Admin,
            description: "Advertisement: Used to accept adverts")]
        public static void AdminCommandAcceptAdvert(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/acceptad [Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be numeric.");
                return;
            }

            if (!Advertisements.AdvertList.ContainsKey(id))
            {
                player.SendErrorNotification("Advert doesn't exist.");
                return;
            }

            Advertisements.PublishAdvert(id);

            string username = player.FetchAccount().Username;

            List<IPlayer> onlineAdmins =
                Alt.Server.GetPlayers().Where(x => x.FetchAccount().AdminLevel >= AdminLevel.Tester).ToList();

            foreach (IPlayer onlineAdmin in onlineAdmins)
            {
                onlineAdmin.SendAdminMessage($"{username} has accepted advert id: {id}.");
            }
        }

        [Command("denyad", AdminLevel.Tester, onlyOne: true, commandType: CommandType.Admin,
            description: "Advertisement: Used to deny adverts")]
        public static void AdminCommandDenyAdvert(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/denyad [Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be numeric.");
                return;
            }

            if (!Advertisements.AdvertList.ContainsKey(id))
            {
                player.SendErrorNotification("Advert doesn't exist.");
                return;
            }

            Advertisements.DenyAdvert(id);

            string username = player.FetchAccount().Username;

            List<IPlayer> onlineAdmins =
                Alt.Server.GetPlayers().Where(x => x.FetchAccount().AdminLevel >= AdminLevel.Tester).ToList();

            foreach (IPlayer onlineAdmin in onlineAdmins)
            {
                onlineAdmin.SendAdminMessage($"{username} has denied advert id: {id}.");
            }
        }

        [Command("playerproperties", AdminLevel.Tester, onlyOne: true, commandType: CommandType.Admin,
            description: "Properties: Used to teleport to a players property")]
        public static void AdminCommandPlayerProperties(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage($"/playerproperties [Name Or Id]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Target not logged in.");
                return;
            }

            List<Models.Property> targetList = Models.Property.FetchCharacterProperties(targetCharacter);

            if (!targetList.Any())
            {
                player.SendErrorNotification("Player has no properties.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Models.Property property in targetList)
            {
                menuItems.Add(new NativeMenuItem(property.Address, $"Id: {property.Id}"));
            }

            player.SetData("admin:properties:fetchingProperty", targetCharacter.Id);

            NativeMenu menu = new NativeMenu("admin:Properties:PlayerProperties", "Properties", "Select a property", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnPlayerPropertySelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("admin:properties:fetchingProperty", out int targetId);

            Models.Property targetProperty = Models.Property.FetchCharacterProperties(Models.Character.GetCharacter(targetId))
                .FirstOrDefault(x => x.Address == option);

            if (targetProperty == null)
            {
                player.SendErrorNotification("Unable to fetch the property.");
                return;
            }

            Position position = targetProperty.FetchExteriorPosition();

            player.Position = position;
            player.Dimension = (int)targetProperty.ExtDimension;
        }

        [Command("setlivery", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Vehicle: Used to set vehicle livery")]
        public static void AdminCommandSetLivery(IPlayer player, string liveryString = "")
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            if (liveryString == "")
            {
                player.SendSyntaxMessage("/livery [LiveryId]");
                return;
            }

            bool tryParse = byte.TryParse(liveryString, out byte livery);

            if (!tryParse)
            {
                player.SendErrorNotification("Parameter must be numeric");
                return;
            }

            player.Vehicle.Livery = livery;

            if (player.Vehicle.FetchVehicleData() != null)
            {
                using Context context = new Context();

                var vehicle = context.Vehicle.Find(player.Vehicle.GetVehicleId());

                vehicle.Livery = livery;

                context.SaveChanges();
            }

            player.SendInfoNotification($"You've set the vehicle livery to {livery}.");
        }

        [Command("acleantag", AdminLevel.Administrator, commandType: CommandType.Admin,
            description: "Graffiti: Cleans graffiti")]
        public static void AdminCommandCleanGraffiti(IPlayer player)
        {
            Models.Graffiti nearestGraffiti = GraffitiHandler.FetchNearestGraffiti(player.Position, 5f);

            if (nearestGraffiti == null)
            {
                player.SendErrorNotification("You're not near a graffiti!");
                return;
            }

            GraffitiHandler.UnloadGraffitiLabel(nearestGraffiti);
            Models.Graffiti.DeleteGraffiti(nearestGraffiti);
        }

        [Command("pid", AdminLevel.Tester, commandType: CommandType.Admin,
            description: "Property: Used to fetch property id")]
        public static void AdminCommandFetchPropertyId(IPlayer player)
        {
            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("You are not near a property.");
                return;
            }

            player.SendInfoNotification($"Property Id: {nearestProperty.Id}.");
        }

        //[Command("spectate", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Player: Used to spectate other players")]
        public static void SpectateTest(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/spectate [nameOrId / off]");
                return;
            }

            if (args.ToLower() == "off")
            {
                bool hasSpecData = player.GetData("admin:CurrentlySpectating", out int playerId);

                IPlayer currentlySpectating = null;

                if (hasSpecData)
                {
                    currentlySpectating = Utility.FindPlayerByNameOrId(playerId.ToString());
                }

                player.Emit("ToggleSpectate", false, currentlySpectating);
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            Models.Account targetAccount = targetPlayer.FetchAccount();

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned in.");
                return;
            }

            if (targetAccount.AdminLevel >= player.FetchAccount().AdminLevel && player.FetchAccount().AdminLevel < AdminLevel.Management)
            {
                player.SendPermissionError();
                return;
            }

            player.Emit("ToggleTransparency", true);

            player.SetSyncedMetaData("IsSpectating", true);

            player.SendInfoNotification($"You've started spectating {targetCharacter.Name} (Id: {targetPlayer.GetPlayerId()}).");

            Logging.AddToAdminLog(player, $"has started spectating {targetCharacter.Name} (Id: {targetPlayer.GetPlayerId()}).");

            AdminCommandAStats(player, targetPlayer.GetPlayerId().ToString());

            player.SetData("admin:CurrentlySpectating", targetPlayer.GetPlayerId());

            player.SetData("admin:LastSpectatePosition", player.Position);

            player.SetData("admin:LastSpectateDimension", player.Dimension);

            player.Position = targetPlayer.Position - new Position(0, 0, 2);

            player.Emit("ToggleSpectate", true, targetPlayer);

            player.Dimension = targetPlayer.Dimension;

            player.FreezePlayer(true);
        }

        public static void OnSpectateFinish(IPlayer player)
        {
            player.SetSyncedMetaData("IsSpectating", false);
            player.SetData("admin:CurrentlySpectating", 0);

            player.GetData("admin:LastSpectatePosition", out Position playerPosition);

            player.GetData("admin:LastSpectateDimension", out int dimension);

            player.Position = playerPosition;

            player.Dimension = dimension;

            player.FreezePlayer(false);

            player.SendInfoNotification($"You've stopped spectating.");
        }

        public static void OnSpectateSetPosition(IPlayer player)
        {
            player.GetData("admin:CurrentlySpectating", out int targetId);

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(targetId.ToString());

            if (targetPlayer == null) return;
            player.Position = targetPlayer.Position - new Position(0, 0, 2);
        }

        [Command("addfishpoint", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Fishing: Adds a fishing point")]
        public static void AdminCommandAddFishPoint(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendInfoNotification($"Point Types: 0 - Fish Point, 1 - Sell Point");
                player.SendSyntaxMessage("/addfishpoint [PointType]");
                return;
            }

            bool tryParse = int.TryParse(args, out int pointType);
            if (!tryParse)
            {
                player.SendInfoNotification($"Point Types: 0 - Fish Point, 1 - Sell Point");
                player.SendSyntaxMessage("/addfishpoint [PointType]");
                return;
            }

            if (pointType < 0 || pointType > 1)
            {
                player.SendInfoNotification($"Point Types: 0 - Fish Point, 1 - Sell Point");
                player.SendSyntaxMessage("/addfishpoint [PointType]");
                return;
            }

            FishingPoint newPoint = new FishingPoint((FishingPointType)pointType, player.Position, 0, 300, 1, 5);

            if (newPoint.PointType == FishingPointType.FishSpot)
            {
                newPoint.FishCount = 100;
            }

            using Context context = new Context();

            context.FishingPoints.Add(newPoint);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new fishing point.");

            Logging.AddToAdminLog(player, $"Has added a new fishing point, type: {newPoint.PointType}, Id: {newPoint.Id}.");

            FishingHandler.LoadFishingPoint(newPoint);
        }

        [Command("gotoproperty", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Property: Used to teleport to a property")]
        public static void AdminCommandGotoProperty(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/gotoproperty [Property Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int propertyId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/gotoproperty [Property Id]");
                return;
            }

            Models.Property nearProperty = Models.Property.FetchProperty(propertyId);

            if (nearProperty == null)
            {
                player.SendErrorNotification("Property not found.");
                return;
            }

            Position position = nearProperty.FetchExteriorPosition();

            player.Position = position;

            player.SendInfoNotification($"You have been teleported to {nearProperty.Address}.");
        }

        [Command("createmotel", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Motel: Used to create a motel")]
        public static void AdminCommandCreateMotel(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/createmotel [MotelName]");
                return;
            }

            using Context context = new Context();

            bool hotelNameUsed = context.Motels.Any(x => x.Name == args);

            if (hotelNameUsed)
            {
                player.SendErrorNotification("This name is already taken.");
                return;
            }

            Models.Motel newMotel = new Models.Motel(args, player.Position);

            context.Motels.Add(newMotel);

            context.SaveChanges();

            player.SendInfoNotification($"You've created {newMotel.Name} with the Id {newMotel.Id}.");

            Logging.AddToAdminLog(player, $"has created a new motel by the Id of {newMotel.Id}");

            MotelHandler.LoadMotel(newMotel);
        }

        [Command("motelid", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Motel: Used to find a motel Id")]
        public static void AdminCommandMotelId(IPlayer player)
        {
            using Context context = new Context();

            List<Models.Motel> motels = context.Motels.ToList();

            Models.Motel lastMotel = null;
            float lastDistance = 5f;

            foreach (Models.Motel motel in motels)
            {
                Position motelPosition = new Position(motel.PosX, motel.PosY, motel.PosZ);

                float distance = motelPosition.Distance(player.Position);

                if (distance < lastDistance)
                {
                    lastMotel = motel;
                    lastDistance = distance;
                }
            }

            if (lastMotel == null)
            {
                player.SendErrorNotification("No nearby motels found.");
                return;
            }

            player.SendInfoNotification($"Motel {lastMotel.Name}'s Id is {lastMotel.Id}.");
        }

        [Command("addmotelroom", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Motel: Used to add a room to a motel")]
        public static void AdminCommandAddMotelRoom(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/addmotelroom [Motel Id] [Room Number] [Value]");
                return;
            }

            string[] splitArgs = args.Split(' ');

            if (splitArgs.Length != 3)
            {
                player.SendSyntaxMessage("/addmotelroom [Motel Id] [Room Number] [Value]");
                return;
            }

            bool motelParse = int.TryParse(splitArgs[0], out int motelId);

            bool roomParse = int.TryParse(splitArgs[1], out int roomId);

            bool valueParse = int.TryParse(splitArgs[2], out int value);

            if (!motelParse || !roomParse || !valueParse)
            {
                player.SendSyntaxMessage("/addmotelroom [Motel Id] [Room Number] [Value]");
                return;
            }

            MotelRoom newMotelRoom = new MotelRoom
            {
                Id = roomId,
                OwnerId = 0,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                Value = value,
                Locked = false,
                MotelId = motelId
            };

            using Context context = new Context();

            Models.Motel motel = context.Motels.Find(motelId);

            if (motel == null)
            {
                player.SendErrorNotification("Unable to find this Motel.");
                return;
            }

            List<MotelRoom> motelRooms = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

            motelRooms.Add(newMotelRoom);

            motel.RoomList = JsonConvert.SerializeObject(motelRooms);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a Room {newMotelRoom.Id} to motel {motel.Name}.");
            player.SendInfoNotification($"Use /reloadmotels when fished");
        }

        [Command("reloadmotels", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Motel: Used to reload motels")]
        public static void AdminCommandReloadMotel(IPlayer player)
        {
            MotelHandler.ReloadMotels();
        }

        [Command("creategarage", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Garage: Creates a garage")]
        public static void AdminCommandCreateGarage(IPlayer player)
        {
            using Context context = new Context();

            Models.Garage newGarage = new Models.Garage
            {
                ExtPosX = player.Position.X,
                ExtPosY = player.Position.Y,
                ExtPosZ = player.Position.Z,
                ExtRotZ = player.Rotation.Yaw,
                ExtDimension = (uint)player.Dimension,
            };

            context.Garages.Add(newGarage);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new garage. The exterior has been made, now do /setgarageint {newGarage.Id} inside an interior.");
        }

        [Command("setgarageint", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Garage: Sets a garage interior")]
        public static void AdminCommandSetGarageInt(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setgarageint (GarageId)");
                return;
            }

            bool tryParse = int.TryParse(args, out int garageId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/setgarageint (GarageId)");
                return;
            }

            using Context context = new Context();

            Models.Garage garage = context.Garages.Find(garageId);

            if (garage == null)
            {
                player.SendErrorNotification("An error occurred fetching the garage.");
                return;
            }

            Position playerPosition = player.Position;

            garage.IntPosX = playerPosition.X;
            garage.IntPosY = playerPosition.Y;
            garage.IntPosZ = playerPosition.Z;
            garage.IntRotZ = player.Rotation.Yaw;
            context.SaveChanges();

            player.SendInfoNotification($"Garage Id {garage.Id} interior position has been updated!");
        }

        [Command("gipl", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Garage: Used to set the garage IPL")]
        public static void CommandGarageIpl(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/gipl [GarageId] [IPL]");
                return;
            }

            string[] splitStrings = args.Split(' ');

            bool idParse = int.TryParse(splitStrings[0], out int garageId);

            if (!idParse || splitStrings.Length < 2)
            {
                player.SendSyntaxMessage("/gipl [GarageId] [IPL]");
                return;
            }

            string ipl = string.Join(' ', splitStrings.Skip(1));

            using Context context = new Context();
            Models.Garage garage = context.Garages.Find(garageId);

            if (garage == null)
            {
                player.SendErrorNotification("An error occurred fetching the garage.");
                return;
            }

            garage.Ipl = ipl;

            context.SaveChanges();

            player.SendInfoNotification($"Garage Id {garage.Id} IPL updated to {ipl}!");
            return;
        }

        [Command("gcolor", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Garage: Used to set the garage IPL")]
        public static void CommandGarageColor(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/gcolor [GarageId] [ColorId]");
                return;
            }

            string[] splitStrings = args.Split(' ');

            bool idParse = int.TryParse(splitStrings[0], out int garageId);

            if (!idParse || splitStrings.Length < 2)
            {
                player.SendSyntaxMessage("/gcolor [GarageId] [ColorId]");
                return;
            }

            bool colorParse = int.TryParse(splitStrings[1], out int colorId);

            if (!colorParse)
            {
                player.SendSyntaxMessage("/gcolor [GarageId] [ColorId]");
                return;
            }

            using Context context = new Context();

            Models.Garage garage = context.Garages.Find(garageId);

            if (garage == null)
            {
                player.SendErrorNotification("An error occurred fetching the garage.");
                return;
            }

            garage.ColorId = colorId;

            context.SaveChanges();

            player.SendInfoNotification($"Garage Id {garage.Id} color updated to {colorId}!");

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(garage.PropJson);

            if (propList.Any())
            {
                foreach (var client in Alt.Server.GetPlayers().Where(x =>
                    x.FetchCharacter() != null && x.FetchCharacter().InsideGarage == garageId))
                {
                    foreach (var prop in propList)
                    {
                        client.UnloadInteriorProp(prop);
                        client.LoadInteriorProp(prop);

                        if (garage.ColorId > 0)
                        {
                            if (prop.Contains("tint"))
                            {
                                client.Emit("SetInteriorColor", prop, garage.ColorId);
                            }
                        }
                    }
                }
            }

            return;
        }

        [Command("reloadclothes", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Clothing: Reloads all clothing and torso data.")]
        public static void AdminCommandReloadClothes(IPlayer player)
        {
            Clothes.LoadClothingItems();

            player.SendInfoNotification($"Reloaded all clothing items.");
        }

        [Command("addclerk", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Clerk Job: Used to create a new clerk job")]
        public static void AdminCommandAddClerk(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendInfoNotification($"Clerk Types: 0 - Gas Station, 1 - Convenience");
                player.SendSyntaxMessage("/addclerk [Type] [StoreName]");
                return;
            }

            string[] param = args.Split(' ');

            bool typeParse = int.TryParse(param[0], out int storeType);

            if (!typeParse)
            {
                player.SendInfoNotification($"Clerk Types: 0 - Gas Station, 1 - Convenience");
                player.SendSyntaxMessage("/addclerk [Type] [StoreName]");
                return;
            }

            string storeName = string.Join(' ', param.Skip(1));

            if (storeName.Length < 3)
            {
                player.SendErrorNotification("Store name too short!");
                return;
            }

            using Context context = new Context();

            bool storeExists = context.Clerks.Any(x => x.StoreName.ToLower() == storeName.ToLower());

            if (storeExists)
            {
                player.SendErrorNotification("This store name has already been used!");
                return;
            }

            ClerkStoreType storeEnum;

            switch (storeType)
            {
                case 0:
                    storeEnum = ClerkStoreType.FuelStation;
                    break;

                case 1:
                    storeEnum = ClerkStoreType.Convenience;
                    break;

                default:
                    player.SendErrorNotification("Invalid store type.");
                    return;
            }

            Clerk newClerk = new Clerk
            {
                Id = 0,
                StoreName = storeName,
                StoreType = storeEnum,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                Positions = JsonConvert.SerializeObject(new List<Position>())
            };

            context.Clerks.Add(newClerk);
            context.SaveChanges();

            player.SendInfoNotification($"New clerk job added. Name: {newClerk.StoreName}, Id: {newClerk.Id}.");

            ClerkHandler.LoadClerkJob(newClerk);
        }

        [Command("clerkid", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Clerk Job: Used to fetch the Id.")]
        public static void AdminCommandClerkId(IPlayer player)
        {
            Clerk closestClerk = Clerk.FetchClerks()
                .FirstOrDefault(x => Clerk.FetchPosition(x).Distance(player.Position) < 5);

            if (closestClerk == null)
            {
                player.SendErrorNotification("Your not near a clerk position.");
                return;
            }

            player.SendInfoNotification($"Clerk Job Id: {closestClerk.Id}, Name: {closestClerk.StoreName}.");
        }

        [Command("setclerkpos", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Clerk Job: Used to set a position whilst doing the job")]
        public static void AdminCommandSetClerkPos(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setclerkpos (Clerk Id)");
                return;
            }

            bool idParse = int.TryParse(args, out int storeId);

            if (!idParse)
            {
                player.SendSyntaxMessage("/setclerkpos (Clerk Id)");
                return;
            }

            using Context context = new Context();

            Clerk clerk = context.Clerks.Find(storeId);

            if (clerk == null)
            {
                player.SendErrorNotification("Unable to find a clerk by this name.");
                return;
            }

            List<Position> clerkPositions = Clerk.FetchPositions(clerk);

            Position playerPosition = player.Position;

            clerkPositions.Add(playerPosition);

            clerk.Positions = JsonConvert.SerializeObject(clerkPositions, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new position to clerk id {clerk.Id}");
        }

        [Command("clerkproperty", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Clerk Job: Links a Clerk Job to Property")]
        public static void AdminCommandClerkProperty(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/clerkproperty [ClerkId] [PropertyId]");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length < 2)
            {
                player.SendSyntaxMessage("/clerkproperty [ClerkId] [PropertyId]");
                return;
            }

            bool clerkParse = int.TryParse(split[0], out int clerkId);
            bool propertyParse = int.TryParse(split[1], out int propertyId);

            if (!clerkParse || !propertyParse)
            {
                player.SendSyntaxMessage("/clerkproperty [ClerkId] [PropertyId]");
                return;
            }

            using Context context = new Context();

            Clerk clerk = context.Clerks.Find(clerkId);
            if (clerk == null)
            {
                player.SendErrorNotification("Clerk Job not found by that Id.");
                return;
            }

            clerk.PropertyId = propertyId;

            context.SaveChanges();

            player.SendInfoNotification($"You've set clerk id {clerkId}'s linked property id to {propertyId}");
        }

        [Command("xp", AdminLevel.Administrator)]
        public static void AdminCommandXp(IPlayer player)
        {
            player.Position = player.Position + new Position(1, 0, 0);
        }

        [Command("mp", AdminLevel.Administrator)]
        public static void AdminCommandMp(IPlayer player)
        {
            player.Position = player.Position - new Position(1, 0, 0);
        }

        [Command("xy", AdminLevel.Administrator)]
        public static void AdminCommandXy(IPlayer player)
        {
            player.Position = player.Position + new Position(0, 1, 0);
        }

        [Command("minusy", AdminLevel.Administrator)]
        public static void AdminCommandMy(IPlayer player)
        {
            player.Position = player.Position - new Position(0, 1, 0);
        }

        [Command("xz", AdminLevel.Administrator)]
        public static void AdminCommandXz(IPlayer player)
        {
            player.Position = player.Position + new Position(0, 0, 1);
        }

        [Command("mz", AdminLevel.Administrator)]
        public static void AdminCommandMz(IPlayer player)
        {
            player.Position = player.Position - new Position(0, 0, 1);
        }

        [Command("whois", AdminLevel.Tester, true, description: "Used to see who is behind a mask")]
        public static void AdminCommandWhoIs(IPlayer player, string maskId = "")
        {
            if (maskId == "")
            {
                player.SendSyntaxMessage("/whois [MaskId]");
                return;
            }

            IPlayer? targetPlayer = null;

            foreach (IPlayer target in Alt.Server.GetPlayers().Where(x => x.GetClass().Name.ToLower().Contains("mask")))
            {
                string[] nameSplit = target.GetClass().Name.Split(' ');

                if (nameSplit.Length < 2) continue;

                if (nameSplit[1] == maskId)
                {
                    targetPlayer = target;
                }
            }

            if (targetPlayer == null)
            {
                player.SendErrorNotification($"Unable to find mask id {maskId}.");
                return;
            }

            player.SendInfoNotification($"Mask Id {maskId} is {targetPlayer.FetchCharacter().Name}.");

            Logging.AddToAdminLog(player, $"has searched for mask Id {maskId}. Result: {targetPlayer.FetchCharacter().Name}");
        }

        [Command("fetchrental", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Rental: Used to fetch a players rental car")]
        public static void AdminCommandFetchRental(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/fetchrental [NameOrId]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Target player not logged in.");
                return;
            }

            bool tryGetRental =
                VehicleRental.RentalVehicles.TryGetValue(targetPlayer.GetClass().CharacterId, out IVehicle rental);

            if (!tryGetRental || rental == null)
            {
                player.SendErrorNotification("Unable to find a rental car for this player.");
                return;
            }

            rental.Position = player.Position.Around(3f);
            rental.Dimension = player.Dimension;

            player.SendInfoNotification($"You've teleported {targetPlayer.GetClass().Name}'s rental vehicle to you.");

            Logging.AddToAdminLog(player, $"has teleported {targetPlayer.GetClass().Name}'s rental vehicle to to them");
        }

        [Command("reloadinteriors", AdminLevel.Management, description: "Used to reload interiors")]
        public static void AdminCommandReloadInteriors(IPlayer player)
        {
            Interiors.LoadInteriors();

            player.SendInfoNotification($"Reloaded {Interiors.InteriorList.Count} Interiors.");
        }

        [Command("givemoney", AdminLevel.HeadAdmin, true, description: "Used to give money to players")]
        public static void AdminCommandGiveMoney(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/givemoney [Amount] [IdOrName]");
                return;
            }

            string[] splitString = args.Split(' ');

            if (splitString.Length < 2)
            {
                player.SendSyntaxMessage("/givemoney [Amount] [IdOrName]");
                return;
            }

            bool moneyParse = float.TryParse(splitString[0], out float money);

            if (!moneyParse)
            {
                player.SendErrorNotification("Money must be a number!");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(string.Join(' ', splitString.Skip(1)));

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            if (targetPlayer.FetchCharacter() == null)
            {
                player.SendErrorNotification("Target not spawned!");
                return;
            }

            targetPlayer.AddCash(money);

            Logging.AddToAdminLog(player, $"has admin given {targetPlayer.GetClass().Name} {money:C}.");

            Logging.AddToCharacterLog(targetPlayer, $"has been admin given {money:C} by {player.FetchAccount().Username}.");

            player.SendInfoNotification($"You've given {money:C} to {targetPlayer.GetClass().Name}.");
        }

        [Command("ping", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "Used to view another players ping")]
        public static void AdminCommandViewPing(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/ping [IdOrName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (!targetPlayer.IsSpawned())
            {
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            player.SendInfoNotification($"Ping for {targetPlayer.GetClass().Name} ({targetPlayer.FetchAccount().Username}) is {targetPlayer.Ping}.");
        }

        [Command("setmileage", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Sets a vehicle's odometer reading")]
        public static void AdminCommandSetMileage(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setmileage [Miles]");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            Models.Vehicle vehicleData = playerVehicle.FetchVehicleData();

            if (vehicleData == null)
            {
                player.SendErrorNotification("You must be in a server vehicle.");
                return;
            }

            bool tryParse = float.TryParse(args, out float distance);

            if (!tryParse)
            {
                player.SendErrorNotification("Mileage must be numeric!");
                return;
            }

            using Context context = new Context();

            var vehicleDb = context.Vehicle.Find(playerVehicle.GetClass().Id);

            double mileage = Math.Round(distance * 1609, 2);

            vehicleDb.Odometer = (float)mileage;
            playerVehicle.GetClass().Distance = (float)mileage;
            context.SaveChanges();
            player.SendInfoNotification($"You've set the mileage to {args} miles.");
        }

        [Command("manage", AdminLevel.Tester, true, commandType: CommandType.Admin,
            description: "Other: Used to do shit to a player")]
        public static void AdminCommandManagePlayer(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/manage [IdOrName]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Bring"),
                new NativeMenuItem("Goto"),
                new NativeMenuItem("Revive")
            };

            List<Models.Vehicle> playerVehicles = Models.Vehicle.FetchCharacterVehicles(targetCharacter.Id);

            if (playerVehicles.Any())
            {
                menuItems.Add(new NativeMenuItem("Vehicles"));
            }

            List<Models.Property> playerProperties = Models.Property.FetchCharacterProperties(targetCharacter);

            if (playerProperties.Any())
            {
                menuItems.Add(new NativeMenuItem("Properties"));
            }

            player.SetData("AdminCommand:ManagingPlayer", targetCharacter.Id);

            NativeMenu menu = new NativeMenu("AdminCommand:OnManagePlayer", "Manage", $"Managing {targetCharacter.Name}", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnManagePlayerSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("AdminCommand:ManagingPlayer", out int targetCharacterId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers()
                .FirstOrDefault(x => x.GetClass()?.CharacterId == targetCharacterId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned!");
                return;
            }

            if (option == "Bring")
            {
                // Brings Player To Admin

                targetPlayer.Position = player.Position;
                targetPlayer.Dimension = player.Dimension;

                player.SendNotification($"~y~You have brought {targetCharacter.Name} to you!");
                targetPlayer.SendNotification($"~y~You have been brought to {player.GetClass().Name}.");

                Logging.AddToAdminLog(player, $"has teleported {targetCharacter.Name} to them.");
                Logging.AddToCharacterLog(targetPlayer, $"has been teleported to {player.GetClass().Name}.");

                return;
            }

            if (option == "Goto")
            {
                // Sends player to admin

                player.Position = targetPlayer.Position;
                player.Dimension = targetPlayer.Dimension;

                player.SendNotification($"~y~You have been sent to {targetCharacter.Name}!");

                Logging.AddToAdminLog(player, $"has teleported to {targetCharacter.Name}.");

                return;
            }

            if (option == "Revive")
            {
                // Revives player
                DeathHandler.CommandRevive(player, targetCharacter.Name);
                return;
            }

            if (option == "Vehicles")
            {
                // Views Vehicles

                List<Models.Vehicle> playerVehicles = Models.Vehicle.FetchCharacterVehicles(targetCharacter.Id);

                List<NativeMenuItem> vehicleItems = new List<NativeMenuItem>();

                foreach (Models.Vehicle playerVehicle in playerVehicles)
                {
                    vehicleItems.Add(new NativeMenuItem(playerVehicle.Name, $"Id: {playerVehicle.Id}"));
                }

                NativeMenu menu = new NativeMenu("AdminCommand:OnManagePlayer:Vehicles", "Manage", "Select the vehicle", vehicleItems);

                NativeUi.ShowNativeMenu(player, menu, true);

                return;
            }

            if (option == "Properties")
            {
                // Views Properties

                List<Models.Property> playerProperties = Models.Property.FetchCharacterProperties(targetCharacter);

                List<NativeMenuItem> propertyItems = new List<NativeMenuItem>();

                foreach (Models.Property playerProperty in playerProperties)
                {
                    string businessName = "";

                    if (!string.IsNullOrEmpty(playerProperty.BusinessName))
                    {
                        businessName = playerProperty.BusinessName;
                    }

                    propertyItems.Add(new NativeMenuItem(playerProperty.Address, businessName));
                }

                NativeMenu menu = new NativeMenu("AdminCommand:OnManagePlayer:Properties", "Manage", "Select a property", propertyItems);

                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }
        }

        public static void OnManagingPlayerSelectVehicle(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("AdminCommand:ManagingPlayer", out int targetCharacterId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers()
                .FirstOrDefault(x => x.GetClass()?.CharacterId == targetCharacterId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned!");
                return;
            }

            List<Models.Vehicle> playerVehicles = Models.Vehicle.FetchCharacterVehicles(targetCharacter.Id);

            Models.Vehicle selectedVehicle = playerVehicles.FirstOrDefault(x => x.Name == option);

            if (selectedVehicle == null)
            {
                player.SendErrorNotification("Unable to find this vehicle.");
                return;
            }

            player.SetData("AdminCommand:ManagingPlayer:VehicleId", selectedVehicle.Id);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Bring"),
                new NativeMenuItem("Send To Player"),
                new NativeMenuItem("Goto")
            };

            NativeMenu menu = new NativeMenu("AdminCommand:OnManagePlayer:Vehicle:Selected", "Manage", $"Select an option for {selectedVehicle.Name}.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnManagingPlayerSelectVehicleSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("AdminCommand:ManagingPlayer", out int targetCharacterId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers()
                .FirstOrDefault(x => x.GetClass()?.CharacterId == targetCharacterId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned!");
                return;
            }

            player.GetData("AdminCommand:ManagingPlayer:VehicleId", out int targetVehicleId);

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass()?.Id == targetVehicleId);

            if (targetVehicle == null)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle");
                return;
            }

            Models.Vehicle targetVehicleData = Models.Vehicle.FetchVehicle(targetVehicleId);

            if (targetVehicleData == null)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle");
                return;
            }

            if (option == "Bring")
            {
                targetVehicle.Position = player.Position.Around(3f);
                targetVehicle.Dimension = player.Dimension;

                player.SendInfoNotification($"You've teleported {targetVehicleData.Name} to you.");
                return;
            }

            if (option == "Send To Player")
            {
                targetVehicle.Position = targetPlayer.Position.Around(3f);
                targetVehicle.Dimension = targetPlayer.Dimension;

                player.SendInfoNotification($"You've teleported {targetVehicleData.Name} to {targetCharacter.Name}.");
                return;
            }

            if (option == "Goto")
            {
                player.Position = targetVehicle.Position.Around(2f);
                player.Dimension = targetVehicle.Dimension;

                player.SendInfoNotification($"You've been teleported to {targetVehicleData.Name}.");
            }
        }

        public static void OnManagingPlayerSelectProperty(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("AdminCommand:ManagingPlayer", out int targetCharacterId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers()
                .FirstOrDefault(x => x.GetClass()?.CharacterId == targetCharacterId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned!");
                return;
            }

            List<Models.Property> playerProperties = Models.Property.FetchCharacterProperties(targetCharacter);

            Models.Property selectedProperty = playerProperties.FirstOrDefault(x => x.Address == option);

            if (selectedProperty == null)
            {
                player.SendErrorNotification("Unable to find the selected property");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Goto"),
                new NativeMenuItem("Send Player")
            };

            NativeMenu menu = new NativeMenu("AdminCommand:OnManagePlayer:Properties:Selected", "Manage", $"Select an option for {selectedProperty.Address}.", menuItems);

            player.SetData("AdminCommand:ManagingPlayer:PropertyId", selectedProperty.Id);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnManagePlayerPropertiesSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("AdminCommand:ManagingPlayer", out int targetCharacterId);

            IPlayer? targetPlayer = Alt.Server.GetPlayers()
                .FirstOrDefault(x => x.GetClass()?.CharacterId == targetCharacterId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Player not spawned!");
                return;
            }

            player.GetData("AdminCommand:ManagingPlayer:PropertyId", out int propertyId);

            Models.Property selectedProperty = Models.Property.FetchProperty(propertyId);

            if (selectedProperty == null)
            {
                player.SendErrorNotification("Unable to find property!");
                return;
            }

            if (option == "Goto")
            {
                player.Position = selectedProperty.FetchExteriorPosition();
                player.Dimension = (int)selectedProperty.ExtDimension;

                player.SendInfoNotification($"You've been teleported to {selectedProperty.Address}.");
                return;
            }

            if (option == "Send Player")
            {
                targetPlayer.Position = selectedProperty.FetchExteriorPosition();

                targetPlayer.Dimension = (int)selectedProperty.ExtDimension;

                player.SendInfoNotification($"You've sent {targetCharacter.Name} to {selectedProperty.Address}.");

                targetPlayer.SendInfoNotification($"You've been sent to {selectedProperty.Address} by {player.GetClass().Name}.");

                return;
            }
        }

        [Command("ainv", AdminLevel.Administrator, true, commandType: CommandType.Admin,
            description: "Inventory: Used to see a players inventory")]
        public static void AdminCommandViewInventory(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/ainv [NameOrId]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("This player couldn't be found!");
                return;
            }

            InventoryCommands.ShowInventoryToPlayer(player, targetPlayer.FetchInventory(), false, targetPlayer.FetchCharacter().InventoryID);
        }

        [Command("createapartmentcomplex", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Used to create a new apartment complex")]
        public static void AdminCommandCreateApartmentComplex(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/createapartmentcomplex [Name]");
                return;
            }

            ApartmentComplexes newComplex = new ApartmentComplexes
            {
                ComplexName = args,
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                GaragePosX = 0,
                GaragePosY = 0,
                GaragePosZ = 0,
                ApartmentList = JsonConvert.SerializeObject(new List<Apartment>())
            };

            using Context context = new Context();

            context.ApartmentComplexes.Add(newComplex);

            context.SaveChanges();

            player.SendInfoNotification($"You've created a new apartment complex by the name of {args}. Id: {newComplex.Id}.");

            Logging.AddToAdminLog(player, $"has created a new apartment complex by the name of {args}. Id: {newComplex.Id}.");

            ApartmentHandler.LoadApartmentComplex(newComplex);
        }

        [Command("deleteapartmentcomplex", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Used to delete an apartment complex")]
        public static void AdminCommandDeleteApartmentComplex(IPlayer player)
        {
            List<ApartmentComplexes> apartmentComplexes = ApartmentComplexes.FetchApartmentComplexes();

            ApartmentComplexes nearestComplex = null;

            foreach (var apartmentComplex in apartmentComplexes)
            {
                Position complexPosition =
                    new Position(apartmentComplex.PosX, apartmentComplex.PosY, apartmentComplex.PosZ);

                if (player.Position.Distance(complexPosition) <= 3f)
                {
                    nearestComplex = apartmentComplex;
                    break;
                }
            }

            if (nearestComplex == null)
            {
                player.SendErrorNotification("You're not near any apartment complexes!");
                return;
            }

            ApartmentHandler.UnloadApartmentComplex(nearestComplex);

            using Context context = new Context();

            ApartmentComplexes complex = context.ApartmentComplexes.Find(nearestComplex.Id);

            if (complex == null)
            {
                player.SendErrorNotification("An error occurred fetching the complex data.");
                return;
            }

            context.ApartmentComplexes.Remove(complex);

            context.SaveChanges();

            player.SendInfoNotification($"You've deleted apartment complex: {complex.ComplexName}");

            Logging.AddToAdminLog(player, $"has deleted apartment complex: {complex.ComplexName}.");
        }

        [Command("deleteapartment", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Used to delete an apartment")]
        public static void AdminCommandDeleteApartment(IPlayer player)
        {
            List<ApartmentComplexes> apartmentComplexes = ApartmentComplexes.FetchApartmentComplexes();

            ApartmentComplexes nearestComplex = null;

            foreach (var apartmentComplex in apartmentComplexes)
            {
                Position complexPosition =
                    new Position(apartmentComplex.PosX, apartmentComplex.PosY, apartmentComplex.PosZ);

                if (player.Position.Distance(complexPosition) <= 3f)
                {
                    nearestComplex = apartmentComplex;
                    break;
                }
            }

            if (nearestComplex == null)
            {
                player.SendErrorNotification("You're not near any apartment complexes!");
                return;
            }

            List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(nearestComplex.ApartmentList);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Apartment apartment in apartments)
            {
                menuItems.Add(new NativeMenuItem(apartment.Name, $"Floor: {apartment.Floor}"));
            }

            NativeMenu menu = new NativeMenu("AdminCommand:DeleteApartment", nearestComplex.ComplexName, "Select an apartment to delete", menuItems)
            {
                PassIndex = true
            };

            player.SetData("EditingApartmentComplex", nearestComplex.Id);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnDeleteApartmentSelect(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("EditingApartmentComplex");
                return;
            }

            player.GetData("EditingApartmentComplex", out int complexId);

            player.DeleteData("EditingApartmentComplex");

            using Context context = new Context();

            ApartmentComplexes complex = context.ApartmentComplexes.FirstOrDefault(x => x.Id == complexId);

            if (complex == null)
            {
                player.SendErrorNotification("An error occurred fetching the complex.");
                return;
            }

            List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

            Apartment selectedApartment = apartments[index];

            if (selectedApartment == null)
            {
                player.SendErrorNotification("An error occurred fetching the apartment");
                return;
            }

            apartments.Remove(selectedApartment);

            complex.ApartmentList = JsonConvert.SerializeObject(apartments);

            context.SaveChanges();

            player.SendInfoNotification($"You've removed apartment {selectedApartment.Name} from {complex.ComplexName}.");

            Logging.AddToAdminLog(player, $"has removed apartment {selectedApartment.Name} from {complex.ComplexName}.");
        }

        [Command("createapartment", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Used to create an apartment at a complex")]
        public static void AdminCommandCreateApartment(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/createapartment [Floor] [Price] [Name]");
                return;
            }

            string[] argsSplit = args.Split(' ');

            if (argsSplit.Length < 3)
            {
                player.SendSyntaxMessage("/createapartment [Floor] [Price] [Name]");
                return;
            }

            bool floorParse = int.TryParse(argsSplit[0], out int floor);

            bool priceParse = int.TryParse(argsSplit[1], out int price);

            if (!floorParse || !priceParse)
            {
                player.SendErrorNotification("Values must be numbers for Floor & Price.");
                player.SendSyntaxMessage("/createapartment [Floor] [Price] [Name]");
                return;
            }

            List<ApartmentComplexes> apartmentComplexes = ApartmentComplexes.FetchApartmentComplexes();

            ApartmentComplexes nearestComplex = null;

            foreach (var apartmentComplex in apartmentComplexes)
            {
                Position complexPosition =
                    new Position(apartmentComplex.PosX, apartmentComplex.PosY, apartmentComplex.PosZ);

                if (player.Position.Distance(complexPosition) <= 3f)
                {
                    nearestComplex = apartmentComplex;
                    break;
                }
            }

            if (nearestComplex == null)
            {
                player.SendErrorNotification("You're not near any apartment complexes!");
                return;
            }

            player.SetData("ATAPARTMENTCOMPLEX", nearestComplex.Id);
            player.SetData("CreateAparmtent:Floor", floor);
            player.SetData("CreateAparmtent:Price", price);
            player.SetData("CreateAparmtent:Name", string.Join(' ', argsSplit.Skip(2)));

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Interiors interior in Interiors.InteriorList)
            {
                if (string.IsNullOrEmpty(interior.Description))
                {
                    menuItems.Add(new NativeMenuItem(interior.InteriorName));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(interior.InteriorName, interior.Description));
                }
            }

            NativeMenu interiorMenu = new NativeMenu("AdminCommand:CreateApartment:InteriorSelect", "Interiors", "Select an Interior for the Apartment", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, interiorMenu, true);
        }

        public static void OnCreateApartmentInteriorSelect(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("ATAPARTMENTCOMPLEX");
                player.DeleteData("CreateAparmtent:Floor");
                player.DeleteData("CreateAparmtent:Price");
                player.DeleteData("CreateAparmtent:Name");
                return;
            }

            Interiors interior = Interiors.InteriorList[index];

            if (interior == null)
            {
                player.SendErrorNotification("An error occurred fetching the interior.");
                return;
            }

            player.GetData("ATAPARTMENTCOMPLEX", out int complexId);
            player.GetData("CreateAparmtent:Floor", out int floor);
            player.GetData("CreateAparmtent:Price", out int price);
            player.GetData("CreateAparmtent:Name", out string apartmentName);

            player.DeleteData("ATAPARTMENTCOMPLEX");
            player.DeleteData("CreateAparmtent:Floor");
            player.DeleteData("CreateAparmtent:Price");
            player.DeleteData("CreateAparmtent:Name");

            Apartment newApartment = new Apartment
            {
                Name = apartmentName,
                Owner = 0,
                Price = price,
                KeyCode = Utility.GenerateRandomString(8),
                InteriorName = interior.InteriorName,
                Floor = floor,
                Locked = false,
                PropList = JsonConvert.SerializeObject(new List<string>())
            };

            using Context context = new Context();

            var complex = context.ApartmentComplexes.Find(complexId);
            if (complex == null)
            {
                player.SendErrorNotification("Unable to find this complex.");
                return;
            }

            List<Apartment> apartments = JsonConvert.DeserializeObject<List<Apartment>>(complex.ApartmentList);

            if (apartments.Any(x => x.Name == apartmentName))
            {
                player.SendErrorNotification("An apartment already exists with this name.");
                return;
            }

            apartments.Add(newApartment);

            complex.ApartmentList = JsonConvert.SerializeObject(apartments);

            context.SaveChanges();

            player.SendInfoNotification($"You've created a new apartment named {apartmentName} at the {complex.ComplexName}.");

            Logging.AddToAdminLog(player, $"has created a new apartment named {apartmentName} at the {complex.ComplexName}.");
        }

        [Command("createexit", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Property: Used to create additional entrance / exit points")]
        public static void AdminCommandCreateExit(IPlayer player)
        {
            if (player.Dimension == 0)
            {
                player.SendErrorNotification("You're not in a property.");
                return;
            }

            Models.Property insideProperty = Models.Property.FetchProperty(player.Dimension);

            if (insideProperty == null)
            {
                player.SendErrorNotification("You're not in a property.");
                return;
            }

            PropertyDoor propertyDoor = new PropertyDoor
            {
                ExitPosX = player.Position.X,
                ExitPosY = player.Position.Y,
                ExitPosZ = player.Position.Z,
                ExitDimension = (uint)player.Dimension,
                EnterPosX = 0,
                EnterPosY = 0,
                EnterPosZ = 0,
                EnterDimension = 0
            };

            player.SetData("makingPropertyDoor", JsonConvert.SerializeObject(propertyDoor));
            player.SetData("makingPropertyDoorAt", insideProperty.Id);
        }

        [Command("createenter", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Property: Used to create additional entrance / exit points")]
        public static void AdminCommandCreateEntrance(IPlayer player)
        {
            if (!player.HasData("makingPropertyDoor"))
            {
                player.SendErrorNotification("You need to do /createexit first.");
                player.DeleteData("makingPropertyDoor");
                player.DeleteData("makingPropertyDoorAt");
                return;
            }

            player.GetData("makingPropertyDoor", out string jsonInfo);

            PropertyDoor propertyDoor =
                JsonConvert.DeserializeObject<PropertyDoor>(jsonInfo);

            propertyDoor.EnterPosX = player.Position.X;
            propertyDoor.EnterPosY = player.Position.Y;
            propertyDoor.EnterPosZ = player.Position.Z;
            propertyDoor.EnterDimension = (uint)player.Dimension;

            using Context context = new Context();

            player.GetData("makingPropertyDoorAt", out int propertyId);

            var propertyDb = context.Property.Find(propertyId);

            if (propertyDb == null)
            {
                player.SendErrorNotification("There was an error fetching the DB.");

                player.DeleteData("makingPropertyDoor");
                player.DeleteData("makingPropertyDoorAt");
                return;
            }

            List<PropertyDoor> propertyDoors =
                JsonConvert.DeserializeObject<List<PropertyDoor>>(propertyDb.DoorPositions);

            propertyDoors.Add(propertyDoor);

            propertyDb.DoorPositions = JsonConvert.SerializeObject(propertyDoors);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new entrance to this property.");
        }

        [Command("createbusroute", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Used to create a new bus route")]
        public static void AdminCommandCreateBusRoute(IPlayer player, string args = "")
        {
            if (args == "" || args.Trim().Length < 2)
            {
                player.SendSyntaxMessage("/createbusroute [Route Name]");
                return;
            }

            BusRoute busRoute = BusRoute.FetchBusRoute(args);

            if (busRoute != null)
            {
                player.SendErrorNotification("A route already exists with this name.");
                return;
            }

            BusRoute newRoute = new BusRoute
            {
                RouteName = args,
                BusStops = JsonConvert.SerializeObject(new List<BusStop>())
            };

            using Context context = new Context();

            context.BusRoutes.Add(newRoute);

            context.SaveChanges();

            player.SendInfoNotification($"You've created a bus new route with the name: {newRoute.RouteName}, Id: {newRoute.Id}.");

            Logging.AddToAdminLog(player, $"has created a bus new route with the name: {newRoute.RouteName}, Id: {newRoute.Id}.");
        }

        [Command("addbusstop", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Used to add a bus stop to a route")]
        public static void AdminCommandAddBusStop(IPlayer player, string args = "")
        {
            if (args == "" || args.Trim().Length < 1)
            {
                player.SendSyntaxMessage("/addbusstop [RouteId] [StopName]");
                return;
            }

            string[] argSplit = args.Split(' ');

            if (argSplit.Length < 2)
            {
                player.SendSyntaxMessage("/addbusstop [RouteId] [StopName]");
                return;
            }

            bool tryParse = int.TryParse(argSplit[0], out int routeId);

            if (!tryParse || routeId <= 0)
            {
                player.SendErrorNotification("The route Id must be a number.");
                return;
            }

            BusStop newStop = new BusStop
            {
                Name = string.Join(' ', argSplit.Skip(1)),
                PosX = player.Vehicle.Position.X,
                PosY = player.Vehicle.Position.Y,
                PosZ = player.Vehicle.Position.Z,
                RotZ = player.Vehicle.Rotation.Yaw
            };

            using Context context = new Context();

            BusRoute busRoute = context.BusRoutes.Find(routeId);

            if (busRoute == null)
            {
                player.SendErrorNotification("Unable to find a route by this Id.");
                return;
            }

            List<BusStop> busStops = JsonConvert.DeserializeObject<List<BusStop>>(busRoute.BusStops);

            busStops.Add(newStop);

            busRoute.BusStops = JsonConvert.SerializeObject(busStops);

            context.SaveChanges();

            player.SendInfoNotification($"You've added the {newStop.Name} to the bus route: {busRoute.RouteName}.");

            Logging.AddToAdminLog(player, $"has added the {newStop.Name} to the bus route: {busRoute.RouteName}.");
        }

        [Command("addpgarage", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin, description: "Property: Used to add a garage to a property")]
        public static void AdminCommandAddPropertyGarage(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/addpgarage [PropertyId] [Garage Size]");
                return;
            }

            string[] argsSplit = args.Split(' ');

            if (argsSplit.Length < 2)
            {
                player.SendSyntaxMessage("/addpgarage [PropertyId] [Garage Size]");
                return;
            }

            bool idParse = int.TryParse(argsSplit[0], out int propertyId);
            bool sizeParse = int.TryParse(argsSplit[1], out int garageSize);

            if (!idParse || !sizeParse || propertyId <= 0 || garageSize <= 0)
            {
                player.SendSyntaxMessage("/addpgarage [PropertyId] [Garage Size]");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(propertyId);

            if (property == null)
            {
                player.SendErrorNotification("Unable to find the property by this Id.");
                return;
            }

            PropertyGarage newGarage = new PropertyGarage
            {
                GarageType = GarageTypes.None,
                Id = Utility.GenerateRandomString(6),
                PosX = player.Position.X,
                PosY = player.Position.Y,
                PosZ = player.Position.Z,
                PropertyId = propertyId,
                VehicleCount = garageSize
            };

            List<PropertyGarage> propertyGarages =
                JsonConvert.DeserializeObject<List<PropertyGarage>>(property.GarageList);

            propertyGarages.Add(newGarage);

            property.GarageList = JsonConvert.SerializeObject(propertyGarages);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new garage to {property.Address}.");

            Logging.AddToAdminLog(player, $"has added a new garage to {property.Address}.");
        }

        [Command("gotoint", AdminLevel.Tester, commandType: CommandType.Admin,
            description: "TP: Used to go to an interior")]
        public static void AdminCommandGotoInterior(IPlayer player)
        {
            List<Interiors> interiorList = Interiors.InteriorList;

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Interiors interior in interiorList)
            {
                menuItems.Add(string.IsNullOrEmpty(interior.Description)
                    ? new NativeMenuItem(interior.InteriorName)
                    : new NativeMenuItem(interior.InteriorName, interior.Description));
            }

            NativeMenu menu = new NativeMenu("AdminCommand:GotoInterior", "Interiors", "Select an Interior", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnGotoInteriorSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            List<Interiors> interiorList = Interiors.InteriorList;

            Interiors selectedInterior = interiorList[index];

            if (selectedInterior == null)
            {
                player.SendErrorNotification("Unable to find the interior.");
                return;
            }

            if (!string.IsNullOrEmpty(selectedInterior.Ipl))
            {
                player.RequestIpl(selectedInterior.Ipl);
            }

            player.Position = selectedInterior.Position;

            player.SendInfoNotification($"You have gone to interior: {selectedInterior.InteriorName}.");
        }

        [Command("registerdoor", AdminLevel.Administrator, commandType: CommandType.Admin, description: "Doors: Used to register a door")]
        public static void CommandDoorTest(IPlayer player)
        {
            player.Emit("getClosestDoor");
        }

        [Command("setdoorowner", AdminLevel.Administrator, true, commandType: CommandType.Admin,
            description: "Door: Used to set a door to a character")]
        public static void AdminCommandSetDoorOwner(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setdoorowner [DoorId] [Character Id]");
                return;
            }

            string[] argSplit = args.Split(' ');

            if (argSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setdoorowner [DoorId] [Character Id]");
                return;
            }

            bool doorIdParse = int.TryParse(argSplit[0], out int doorId);
            bool charIdParse = int.TryParse(argSplit[1], out int characterId);

            if (!charIdParse || !doorIdParse)
            {
                player.SendSyntaxMessage("/setdoorowner [DoorId] [Character Id]");
                return;
            }

            using Context context = new Context();

            var door = context.Doors.FirstOrDefault(x => x.Id == doorId);

            if (door == null)
            {
                player.SendNotification("~r~Unable to find this door.");
                return;
            }

            door.OwnerId = characterId;

            context.SaveChanges();

            player.SendNotification($"~r~You've set door id {doorId} owner to {characterId}.");
        }

        [Command("setdoorproperty", AdminLevel.Administrator, true, commandType: CommandType.Admin,
            description: "Door: Used to set a door to a property")]
        public static void AdminCommandSetDoorProperty(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setdoorowner [DoorId] [Property Id]");
                return;
            }

            string[] argSplit = args.Split(' ');

            if (argSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setdoorowner [DoorId] [Property Id]");
                return;
            }

            bool doorIdParse = int.TryParse(argSplit[0], out int doorId);
            bool propertyIdParse = int.TryParse(argSplit[1], out int propertyId);

            if (!propertyIdParse || !doorIdParse)
            {
                player.SendSyntaxMessage("/setdoorowner [DoorId] [Property Id]");
                return;
            }

            Models.Property property = Models.Property.FetchProperty(propertyId);

            if (property == null)
            {
                player.SendErrorNotification("Unable to find property.");
                return;
            }

            using Context context = new Context();

            var door = context.Doors.FirstOrDefault(x => x.Id == doorId);

            if (door == null)
            {
                player.SendNotification("~r~Unable to find this door.");
                return;
            }

            door.PropertyId = propertyId;

            context.SaveChanges();

            player.SendNotification($"~r~You've set door id {doorId} property to {propertyId}.");
        }

        [Command("setdoorfaction", AdminLevel.Administrator, true, commandType: CommandType.Admin,
            description: "Door: Used to set a door to a faction")]
        public static void AdminCommandSetDoorFaction(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setdoorfaction [DoorId] [Faction Id]");
                return;
            }

            string[] argSplit = args.Split(' ');

            if (argSplit.Length != 2)
            {
                player.SendSyntaxMessage("/setdoorfaction [DoorId] [Faction Id]");
                return;
            }

            bool doorIdParse = int.TryParse(argSplit[0], out int doorId);
            bool factionParse = int.TryParse(argSplit[1], out int factionId);

            if (!factionParse || !doorIdParse)
            {
                player.SendSyntaxMessage("/setdoorfaction [DoorId] [Faction Id]");
                return;
            }

            Faction faction = Faction.FetchFaction(factionId);

            if (faction == null)
            {
                player.SendErrorNotification("Unable to find property.");
                return;
            }

            using Context context = new Context();

            var door = context.Doors.FirstOrDefault(x => x.Id == doorId);

            if (door == null)
            {
                player.SendNotification("~r~Unable to find this door.");
                return;
            }

            door.FactionId = factionId;

            context.SaveChanges();

            player.SendNotification($"~r~You've set door id {doorId} faction to {factionId}.");
        }

        [Command("alockdoor", AdminLevel.Administrator, true, commandType: CommandType.Admin, description: "Doors: Used to lock/unlock a door")]
        public static void AdminCommandALockDoor(IPlayer player, string args = "")
        {
            Door nearestDoor = null;

            if (args == "")
            {
                nearestDoor = Door.FetchNearestDoor(player, 3f);
            }
            else
            {
                bool tryParse = int.TryParse(args, out int doorId);

                if (!tryParse)
                {
                    player.SendErrorNotification("Door Id must be a number.");
                    return;
                }

                nearestDoor = Door.FetchDoor(doorId);
            }

            if (nearestDoor == null)
            {
                player.SendNotification("~r~Your not by a door.");
                return;
            };

            using Context context = new Context();

            Door doorDb = context.Doors.Find(nearestDoor.Id);

            doorDb.Locked = !doorDb.Locked;

            context.SaveChanges();

            string state = doorDb.Locked ? "locked" : "unlocked";

            player.SendNotification($"~y~You've {state} the door.");

            DoorHandler.UpdateDoorsForAllPlayers();
        }

        [Command("globaldoor", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Doors: Used to set a door to globally")]
        public static void AdminCommandGlobalDoor(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/globaldoor [DoorId]");
                return;
            }

            bool tryParse = int.TryParse(args, out int doorId);

            if (!tryParse)
            {
                player.SendErrorNotification("Door Id must be a number.");
                return;
            }

            Door nearestDoor = Door.FetchDoor(doorId);

            if (nearestDoor == null)
            {
                player.SendNotification("~r~Your not by a door.");
                return;
            }

            using Context context = new Context();

            Door door = context.Doors.Find(nearestDoor.Id);

            if (door == null)
            {
                player.SendNotification("~r~Unable to find the door.");
                return;
            }

            if (door.Dimension != -1)
            {
                door.Dimension = -1;
                context.SaveChanges();

                player.SendInfoNotification($"Door set globally.");
                return;
            }

            door.Dimension = player.Dimension;
            context.SaveChanges();
            player.SendInfoNotification($"Door set to your dimension of {player.Dimension}.");
        }

        [Command("aduty", AdminLevel.Tester, commandType: CommandType.Admin,
            description: "Used to go on admin duty")]
        public static void AdminCommandAdminDuty(IPlayer player)
        {
            if (player.GetClass().AdminDuty)
            {
                // On Duty

                player.GetClass().AdminDuty = false;
                player.GetClass().Name = player.FetchCharacter().Name;
                player.Emit("EnabledAdminDuty", false);
                player.LoadCharacterCustomization();
            }
            else
            {
                bool hasCurrentWeaponData = player.GetData("CurrentWeaponHash", out uint weaponHash);

                if (hasCurrentWeaponData && weaponHash != 0)
                {
                    player.Emit("fetchCurrentAmmo", "admin:duty:fetchAmmo", player.CurrentWeapon);
                }

                player.GetClass().AdminDuty = true;
                player.GetClass().Name = player.GetClass().UcpName;
                player.Emit("EnabledAdminDuty", true);

                Random rnd = new Random();

                int index = rnd.Next(0, AdminHandler.AdminModels.Count - 1);

                player.Model = (uint)AdminHandler.AdminModels[index];

                player.SetData("OldAdminId", player.GetPlayerId());
            }
        }

        public static void OnDutyReturnAmmo(IPlayer player, int ammoCount)
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

        [Command("bodyid", commandType: CommandType.Character,
            description: "Used to fetch a dead body Id")]
        public static void AdminCommandDeadId(IPlayer player)
        {
            int nearestId = 0;

            Position playerPosition = player.Position;

            float lastDistance = 5f;

            foreach (KeyValuePair<int, DeadBody> keyValuePair in DeathHandler.DeadBodies)
            {
                Position bodyPosition = new Position(keyValuePair.Value.PosX, keyValuePair.Value.PosY, keyValuePair.Value.PosZ);

                float distance = bodyPosition.Distance(playerPosition);

                if (distance < lastDistance)
                {
                    lastDistance = distance;
                    nearestId = keyValuePair.Key;
                }
            }

            if (nearestId == 0)
            {
                player.SendErrorNotification("Your not by a body.");
                return;
            }

            player.SendInfoNotification($"Nearest body is {nearestId}.");
        }

        [Command("removebody", onlyOne: true, commandType: CommandType.Character,
            description: "Used to remove a dead body")]
        public static void AdminCommandRemoveBody(IPlayer player, string args = "")
        {
            bool tryParse = int.TryParse(args, out int bodyId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/removebody [BodyId]");
                return;
            }

            bool exists = DeathHandler.DeadBodies.TryGetValue(bodyId, out DeadBody body);

            if (!exists || body == null)
            {
                player.SendErrorNotification("Couldn't find the body.");
                return;
            }

            DeathHandler.DeadBodies.Remove(bodyId);

            DeadBodyHandler.RemoveDeadBodyForAll(body);

            player.SendInfoNotification($"You've removed the dead body Id {bodyId}.");
        }

        [Command("motd", AdminLevel.Management, true, commandType: CommandType.Admin,
            description: "Used to set a welcome message to the server")]
        public static void AdminCommandMotd(IPlayer player, string args = "")
        {
            player.SendInfoNotification(args.Trim().Length == 0
                ? $"You've removed the welcome message."
                : $"You've set the welcome message to {args}.");

            Settings.ServerSettings.MOTD = args;

            Logging.AddToAdminLog(player, $"has set the MOTD to {args}.");
        }

        [Command("setweather", AdminLevel.Management, true, commandType: CommandType.Admin,
            description: "Used to set the server weather")]
        public static void AdminCommandSetWeather(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/setweather [CityId]\nPlease use OpenWeatherMap for the weather Id");
                return;
            }

            bool tryParse = int.TryParse(args, out int weatherId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/setweather [CityId]\nPlease use OpenWeatherMap for the weather Id");
                return;
            }

            Settings.ServerSettings.WeatherLocation = weatherId;

            player.SendInfoNotification($"You've set the weather to: {weatherId}.");

            Logging.AddToAdminLog(player, $"has set the weather to {args}.");
        }

        [Command("changeaddress", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Used to change a properties address")]
        public static void AdminCommandChangeAddress(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/changeaddress [New Full Address]");
                return;
            }

            Models.Property nearestProperty = Models.Property.FetchNearbyProperty(player, 5f);

            if (nearestProperty == null)
            {
                player.SendErrorNotification("Your not near a property!");
                return;
            }

            using Context context = new Context();

            Models.Property property = context.Property.Find(nearestProperty.Id);

            if (property == null)
            {
                player.SendErrorNotification("Unable to fetch the property from the database.");
                return;
            }

            property.Address = args;

            context.SaveChanges();

            LoadProperties.ReloadProperty(property);

            player.SendInfoMessage($"Updated {nearestProperty.Address} has been changed to {args}.");

            Logging.AddToAdminLog(player, $"has updated property {property.Id} from {nearestProperty.Address} has been changed to {args}.");
        }

        [Command("reloadmaps", AdminLevel.HeadAdmin, commandType: CommandType.Admin,
            description: "Used to reload maps")]
        public static void AdminCommandReloadMaps(IPlayer player)
        {
            MapHandler.ReloadMaps();

            player.SendInfoNotification($"Maps reloaded");

            Logging.AddToAdminLog(player, $"has reloaded maps");
        }

        [Command("reloadbuyobjects", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Used to reload buyable objects for mapping")]
        public static void ReloadBuyObjects(IPlayer player)
        {
            PurchaseObjectHandler.LoadBuyableObjects();
            player.SendInfoNotification($"Reloaded buyable objects.");
        }

        [Command("vstats", AdminLevel.Tester, commandType: CommandType.Admin, description: "Used to view vehicle stats")]
        public static void AdminVehicleStatsCommand(IPlayer player)
        {
            IVehicle vehicle = player.Vehicle ?? VehicleHandler.FetchNearestVehicle(player);

            if (vehicle?.FetchVehicleData() == null)
            {
                player.SendErrorNotification("Your not in or near a vehicle!");
                return;
            }

            Models.Vehicle vehicleData = vehicle.FetchVehicleData();

            Models.Character ownerData = Models.Character.GetCharacter(vehicleData.OwnerId);

            player.SendInfoMessage($"Vehicle Id: {vehicleData.Id}, Inventory Id: {vehicleData.InventoryId}");
            if (ownerData == null)
            {
                player.SendInfoMessage("Owner: None");
            }
            else
            {
                player.SendInfoMessage($"Owner: {ownerData.Name}, Purchase Price: {vehicleData.VehiclePrice:C}");
            }
            List<VehicleMods> vehicleMods = JsonConvert.DeserializeObject<List<VehicleMods>>(vehicleData.VehicleMods);

            player.SendInfoMessage($"Color 1: {vehicleData.Color1}, Color 2: {vehicleData.Color2}, Vehicle Color: {vehicleData.WheelColor}, Mod count: {vehicleMods.Count(x => x.modType > 0)}");
            player.SendInfoMessage($"Mileage: {vehicleData.Odometer / 1609} Miles");
            if (vehicleData.FactionId > 0)
            {
                Faction vehicleFaction = Faction.FetchFaction(vehicleData.FactionId);

                if (vehicleFaction != null)
                {
                    player.SendInfoMessage($"Faction: {vehicleFaction.Name}");
                }
            }
        }

        [Command("avmod", AdminLevel.HeadAdmin, commandType: CommandType.Admin, description: "Used to admin mod a vehicle")]
        public static void AdminCommandModVehicle(IPlayer player)
        {
            player.SetData("AdminModVehicle", true);

            Vehicle.ModShop.ModHandler.ShowVehicleModMenu(player);
        }

        [Command("focuses", AdminLevel.HeadAdmin, true, commandType: CommandType.Admin,
            description: "Used to set a players focuses")]
        public static void AdminCommandFocuses(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/focuses [NameOrId]");
                return;
            }

            IPlayer target = Utility.FindPlayerByNameOrId(args);

            if (target == null || !target.IsSpawned())
            {
                player.SendErrorNotification("Unable to find player.");
                return;
            }

            List<FocusTypes> targetFocuses =
                JsonConvert.DeserializeObject<List<FocusTypes>>(target.FetchCharacter().FocusJson);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (!targetFocuses.Contains(FocusTypes.Mechanic))
            {
                menuItems.Add(new NativeMenuItem("~g~Grant Mechanic Focus"));
            }

            if (targetFocuses.Contains(FocusTypes.Mechanic))
            {
                menuItems.Add(new NativeMenuItem("~r~Revoke Mechanic Focus"));
            }

            if (!targetFocuses.Contains(FocusTypes.Stealth))
            {
                menuItems.Add(new NativeMenuItem("~g~Grant Stealth Focus"));
            }
            if (targetFocuses.Contains(FocusTypes.Stealth))
            {
                menuItems.Add(new NativeMenuItem("~r~Revoke Stealth Focus"));
            }

            NativeMenu menu = new NativeMenu("Admin:SetPlayerFocuses", "Focuses", $"Set {target.GetClass().Name} Focus", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);

            player.SetData("AdjustingFocus", target.GetClass().CharacterId);
        }

        public static void OnSelectFocus(IPlayer player, string option)
        {
            player.GetData("AdjustingFocus", out int targetCharacterId);

            player.DeleteData("AdjustingFocus");

            if (option.Contains("Close")) return;

            using Context context = new Context();

            Models.Character targetCharacter = context.Character.Find(targetCharacterId);

            if (targetCharacter == null)
            {
                player.SendErrorNotification("Unable to find target character.");
                return;
            }

            List<FocusTypes> focuses = JsonConvert.DeserializeObject<List<FocusTypes>>(targetCharacter.FocusJson);

            if (option.Contains("Grant Mechanic Focus"))
            {
                focuses.Add(FocusTypes.Mechanic);
                player.SendInfoNotification($"You've granted the mechanic focus");
            }

            if (option.Contains("Revoke Mechanic Focus"))
            {
                focuses.Remove(FocusTypes.Mechanic);
                player.SendInfoNotification($"You've revoked the mechanic focus");
            }

            if (option.Contains("Grant Stealth Focus"))
            {
                focuses.Add(FocusTypes.Stealth);
                player.SendInfoNotification($"You've granted the stealth focus");
            }

            if (option.Contains("Revoke Stealth Focus"))
            {
                focuses.Remove(FocusTypes.Stealth);
                player.SendInfoNotification($"You've revoked the stealth focus");
            }

            targetCharacter.FocusJson = JsonConvert.SerializeObject(focuses);
            context.SaveChanges();
        }

        [Command("maketester", AdminLevel.Management, true, commandType: CommandType.Admin,
            description: "Used to promote or demote a tester")]
        public static async void AdminCommandMakeHelper(IPlayer player, string args = "")
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                player.SendSyntaxMessage("/makehelper [NameOrId]");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find this player!");
                return;
            }

            if (targetPlayer.FetchAccount() == null)
            {
                player.SendErrorNotification("This player isn't logged in!");
                return;
            }

            await using Context context = new Context();

            Models.Account? target = context.Account.FirstOrDefault(x => x.Id == targetPlayer.GetClass().AccountId);

            if (target == null)
            {
                player.SendErrorNotification("This player isn't logged in!");
                return;
            }

            target.Tester = !target.Tester;
            await context.SaveChangesAsync();

            string message =
                $"You have {(target.Tester ? "promoted" : "demoted")} {target.Username} {(target.Tester ? "to" : "from")} Tester!";
            player.SendInfoNotification(message);

            string playerMessage =
                $"You have been {(target.Tester ? "promoted" : "demoted")} {(target.Tester ? "to" : "from")} Tester by {player.GetClass().UcpName}";
            targetPlayer.SendInfoMessage(playerMessage);
        }
    }
}