using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Server.Admin;
using Server.Apartments;
using Server.Character.Clothing;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Doors;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Extensions.Weather;
using Server.Groups.Police;
using Server.Inventory;
using Server.Models;
using Server.Motel;
using Server.Property;
using MessageType = Server.Chat.MessageType;

namespace Server.Character
{
    public class CharacterCommands
    {
        [Command("blindfold", onlyOne: true, commandType: CommandType.Character,
            description: "Used to blindfold others")]
        public static void CharacterCommandBlindfold(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/blindfold [NameOrId]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null || !targetPlayer.IsSpawned())
            {
                player.SendErrorNotification("Unable to find this player.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 3f || player.Dimension != targetPlayer.Dimension)
            {
                player.SendErrorNotification("You're not near this player!");
                return;
            }

            if (player == targetPlayer)
            {
                // Blindfolding self
                bool blindfoldedByOther = player.HasData("BlindfoldedByOthers");

                if (blindfoldedByOther)
                {
                    player.SendErrorNotification("You can't do this!");
                    return;
                }

                bool blindfolded = player.HasData("Blindfolded");

                if (!blindfolded)
                {
                    targetPlayer.Emit("Blindfolded", true);
                    targetPlayer.SetData("Blindfolded", true);
                }
                else
                {
                    targetPlayer.Emit("Blindfolded", false);
                    targetPlayer.DeleteData("Blindfolded");
                }

                return;
            }

            bool targetBlindfolded = targetPlayer.HasData("Blindfolded");

            if (!targetBlindfolded)
            {
                // Not blindfolded
                targetPlayer.Emit("Blindfolded", true);
                player.SendEmoteMessage($"ties a blindfold around {targetPlayer.GetClass().Name}'s eyes.");
                targetPlayer.SetData("Blindfolded", true);
                targetPlayer.SetData("BlindfoldedByOthers", true);
                return;
            }
            targetPlayer.Emit("Blindfolded", false);
            player.SendEmoteMessage($"unties the blindfold from around {targetPlayer.GetClass().Name}'s eyes.");
            targetPlayer.DeleteData("Blindfolded");
            targetPlayer.DeleteData("BlindfoldedByOthers");
            return;
        }

        [Command("setage", onlyOne: true, commandType: CommandType.Character, description: "Used to set your age")]
        public static void CharacterCommandSetAge(IPlayer player, string args)
        {
            if (!player.IsSpawned()) return;

            bool tryParse = int.TryParse(args, out int age);

            if (!tryParse)
            {
                player.SendErrorNotification("You must input a number.");
                return;
            }

            if (age < 16)
            {
                player.SendInfoNotification($"Minimum age is 16. As per Rule IV.4");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("An error occurred fetching your data.");
                return;
            }

            playerCharacter.Age = age;
            context.SaveChanges();

            Logging.AddToCharacterLog(player, $"has set their character age to {age}.");

            player.SendInfoNotification($"You've changed your age to {age}.");
        }

        [Command("stime", commandType: CommandType.Character, description: "Used to view the OOC server time")]
        public static void CharacterCommandsServerTime(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            var info = TimeZoneInfo.FindSystemTimeZoneById("UTC");

            DateTimeOffset localServerTime = DateTimeOffset.Now;

            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            player.SendInfoMessage($"(Real World Time: {localTime.Hour:D2}:{localTime.Minute:D2} - {localTime.Day:D2}/{localTime.Month:D2}/{localTime.Year}))");
        }

        [Command("time", commandType: CommandType.Character, description: "Used to view the time")]
        public static void CharacterCommandTime(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            ServerSettings serverSettings = ServerSettings.FetchServerSettings();

            var info = TimeZoneInfo.FindSystemTimeZoneById("UTC");

            DateTimeOffset localServerTime = DateTimeOffset.Now;

            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            player.SendInfoMessage($"Current Time: {serverSettings.Hour:D2}:{serverSettings.Minute:D2}\n((Real World Time: {localTime.Hour:D2}:{localTime.Minute:D2} - {localTime.Day:D2}/{localTime.Month:D2}/{localTime.Year}))");
        }

        [Command("charity", onlyOne: true, commandType: CommandType.Character, description: "Used to get rid of those dirty notes")]
        public static void CharacterCommandCharity(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            bool tryParse = double.TryParse(args, out double amount);

            if (!tryParse || args == "")
            {
                player.SendSyntaxMessage("/charity [Amount]");
                return;
            }

            if (amount < 0)
            {
                player.SendErrorNotification("You can't charity nothing, fool!");
                return;
            }

            if (player.GetClass().Cash < amount)
            {
                player.SendErrorNotification($"You must have ${amount} on you!");
                return;
            }

            player.RemoveCash(amount);

            player.SendCharityMessage(amount);

            Logging.AddToCharacterLog(player, $"has charitied {amount}.");
        }

        [Command("timestamp", commandType: CommandType.Character,
            description: "Other: Used to show timestamps on your chat.")]
        public static void CharacterCommandTimeStamp(IPlayer player)
        {
            bool hasData = player.GetData("timestamp", out bool timeStamp);

            if (!hasData || !timeStamp)
            {
                player.SendNotification("~g~Timestamps activated.");
                player.SetData("timestamp", true);
                player.Emit("chat:EnableTimestamp", true);
                return;
            }

            player.SendNotification("~g~Timestamps deactivated.");
            player.SetData("timestamp", false);
            player.Emit("chat:EnableTimestamp", false);
            return;
        }

        [Command("tickets", commandType: CommandType.Character,
            description: "Other: Used at LSPD to see your tickets.")]
        public static void ViewTicketsCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            if (player.Position.Distance(PoliceHandler.UnJailPosition) > 5)
            {
                player.SendErrorNotification("You must be in front of Vespucci PD.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> ticketItems = playerInventory.GetInventoryItems("ITEM_POLICE_TICKET");

            if (!ticketItems.Any())
            {
                player.SendErrorNotification("You don't have any tickets on you.");
                return;
            }

            foreach (InventoryItem inventoryItem in ticketItems)
            {
                Ticket ticket = JsonConvert.DeserializeObject<Ticket>(inventoryItem.ItemValue);

                menuItems.Add(new NativeMenuItem(ticket.Reason, $"{ticket.Amount:C}"));
            }

            NativeMenu menu = new NativeMenu("character:tickets:MainMenu", "Tickets", "Select a ticket to pay!", menuItems)
            {
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnTicketMainMenuSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> ticketItems = playerInventory.GetInventoryItems("ITEM_POLICE_TICKET");

            InventoryItem selectedItem = ticketItems[index];

            Ticket ticket = JsonConvert.DeserializeObject<Ticket>(selectedItem.ItemValue);

            if (player.GetClass().Cash < ticket.Amount)
            {
                player.SendErrorNotification($"You don't have enough. Required: {ticket.Amount:C}.");
                return;
            }

            using Context context = new Context();

            Ticket ticketDb = context.Tickets.Find(ticket.Id);

            if (ticketDb == null)
            {
                player.SendErrorNotification("An error occurred fetching ticket information.");
                return;
            }

            player.RemoveCash(ticket.Amount);

            ticketDb.Paid = true;

            context.SaveChanges();

            player.SendInfoNotification($"You have paid your ticket {ticket.Reason}. Amount: {ticket.Amount:C}.");

            bool remove = playerInventory.RemoveItem(selectedItem);

            if (!remove)
            {
                player.SendErrorNotification("An error occurred removing this from your inventory.");
                return;
            }
        }

        [Command("walkstyle", onlyOne: true, commandType: CommandType.Character,
            description: "Other: Set your characters walk style")]
        public static void WalkStyleCommand(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/walkstyle [0-42]");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            bool tryParse = int.TryParse(args, out int walkId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/walkstyle [0-42]");

                return;
            }

            if (walkId < 0 || walkId > 42)
            {
                player.SendSyntaxMessage("/walkstyle [0-42]");

                return;
            }

            if (playerCharacter.WalkStyle == walkId)
            {
                player.SendInfoNotification($"You have already set this walk style.");

                return;
            }

            playerCharacter.WalkStyle = walkId;

            player.GetClass().WalkStyle = walkId;

            context.SaveChanges();

            var walkName = walkId switch
            {
                0 => "Default",
                1 => "Ballistic",
                2 => "Lemar",
                3 => "Trash",
                4 => "Female fast runner",
                5 => "Garbage Man",
                6 => "Franklin",
                7 => "Jimmy",
                8 => "Michael",
                9 => "Flee",
                10 => "Scared",
                11 => "Sexy",
                12 => "Heist Lester",
                13 => "Injured Generic",
                14 => "Lester Cane",
                15 => "Hold Bag",
                16 => "Bail Bond",
                17 => "Bail Bond Tazered",
                18 => "Brave",
                19 => "Casual",
                20 => "Moderate Drunk",
                21 => "Moderate Drunk head up",
                22 => "Slightly Drunk",
                23 => "Very drunk",
                24 => "Fire",
                25 => "Gangster 1",
                26 => "Gangster 2",
                27 => "Gangster 3",
                28 => "Jog",
                29 => "Prison Guard",
                30 => "P M One",
                31 => "P M One Briefcase",
                32 => "Janitor",
                33 => "Slow",
                34 => "Bucket",
                35 => "Mop",
                36 => "Femme",
                37 => "Male Gangster",
                38 => "Female Gangster",
                39 => "Male Posh",
                40 => "Female Posh",
                41 => "Male Tough guy",
                42 => "Female Tough guy",
                _ => ""
            };

            player.SendInfoNotification($"You've set your walk style to {walkName}.");

            Logging.AddToCharacterLog(player, $"Has set their walk style to {walkName}.");

            player.Emit("SetWalkStyle", walkId);
        }

        [Command("dimension", commandType: CommandType.Character, description: "Other: Used to view current dimension")]
        public static void DimensionCommand(IPlayer player)
        {
            player.SendInfoNotification($"Current Dimension: {player.Dimension}.");
        }

        [Command("help", commandType: CommandType.Character, description: "Shows this menu")]
        public static void HelpCommand(IPlayer player)
        {
            player.FreezeInput(true);
            player.ChatInput(false);

            bool isAdmin = player.FetchAccount().AdminLevel > AdminLevel.None;

            bool isLaw = player.IsLeo(true);

            bool isHelper = player.FetchAccount().Helper;

            player.Emit("helpMenu:ShowHelpMenu", isAdmin, isLaw, isHelper);
        }

        [Command("onduty", commandType: CommandType.Character, description: "Shows a list of on duty law and medical staff")]
        public static void CommandOnDuty(IPlayer player)
        {
            if (player.FetchCharacter() == null) return;

            List<IPlayer> playerList = Alt.Server.GetPlayers().Where(x => x.FetchCharacter().FactionDuty).ToList();

            if (!playerList.Any())
            {
                player.SendErrorNotification("No-one is on duty!");
                return;
            }

            int medical = 0;
            int police = 0;

            foreach (IPlayer target in playerList)
            {
                Faction targetFaction = Faction.FetchFaction(target.FetchCharacter().ActiveFaction);

                if (targetFaction == null || !target.FetchCharacter().FactionDuty) continue;

                if (targetFaction.SubFactionType == SubFactionTypes.Law) police++;

                if (targetFaction.SubFactionType == SubFactionTypes.Medical) medical++;
            }

            player.SendInfoNotification($"Current Law Enforcement on duty: {police}. Current Medical on duty: {medical}.");
        }

        [Command("torso", onlyOne: true, commandType: CommandType.Character, description: "Sets your torso to match your top")]
        public static void CommandTorso(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/torso [TorsoId]");
                return;
            }

            bool tryParse = int.TryParse(args, out int torsoId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/torso [TorsoId]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            List<ClothesData> clothingData =
                JsonConvert.DeserializeObject<List<ClothesData>>(playerCharacter.ClothesJson);

            ClothesData data = clothingData.FirstOrDefault(x => x.slot == 11);

            player.SetClothes(3, torsoId, 0);

            player.SendInfoNotification($"Torso {torsoId} set for top id: {data.drawable} - {data.texture}. Please report this on the bug tracker. This can prevent you typing it all the time!");
        }

        [Command("lock", commandType: CommandType.Character, description: "Locks properties and vehicles")]
        public static void CommandLock(IPlayer player)
        {
            try
            {
                if (!player.IsSpawned()) return;

                if (Vehicle.Commands.ToggleNearestVehicleLock(player)) return;

                if (PropertyCommands.CommandPropertyLock(player)) return;

                if (MotelHandler.ToggleNearestRoomLock(player)) return;

                if (DoorCommands.DoorCommandLock(player)) return;

                if (!string.IsNullOrEmpty(player.FetchCharacter().InsideApartment))
                {
                    ApartmentComplexes apartmentComplex = ApartmentComplexes.FetchApartmentComplex(player.FetchCharacter().InsideApartmentComplex);

                    if (apartmentComplex != null)
                    {
                        ApartmentHandler.ToggleApartmentLock(player);
                        return;
                    }
                }

                Position playerPosition = player.Position;

                foreach (ApartmentComplexes fetchApartmentComplex in ApartmentComplexes.FetchApartmentComplexes())
                {
                    Position position = new Position(fetchApartmentComplex.PosX, fetchApartmentComplex.PosY, fetchApartmentComplex.PosZ);

                    float distance = playerPosition.Distance(position);

                    if (distance <= 3f)
                    {
                        ApartmentHandler.LoadApartmentLockMenu(player, fetchApartmentComplex);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        #region Report System

        [Command("admins", commandType: CommandType.Character, description: "See on duty admins!")]
        public static void CommandViewAdmins(IPlayer player)
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            var onlinePlayers = Alt.GetAllPlayers();

            var onlineAdmins = new List<IPlayer>();

            foreach (IPlayer onlinePlayer in onlinePlayers)
            {
                Models.Account? account = onlinePlayer.FetchAccount();

                if (account is null) continue;

                if (account.AdminLevel < AdminLevel.Moderator) continue;

                onlineAdmins.Add(onlinePlayer);
            }

            if (!onlineAdmins.Any())
            {
                player.SendErrorMessage("No admins online");
                return;
            }

            var sortedList = onlineAdmins.OrderByDescending(x => x.FetchAccount().AdminLevel)
                .ThenByDescending(n => n.GetClass().UcpName);

            player.SendChatMessage("[Online Admins]");

            foreach (var onlineAdmin in sortedList)
            {
                player.SendAdminMessage(onlineAdmin.GetClass().AdminDuty ? $"(On Duty) {onlineAdmin.GetClass().UcpName}" : onlineAdmin.GetClass().UcpName);
            }
        }

        [Command("report", onlyOne: true, commandType: CommandType.Character, description: "Report a situation to the admin team")]
        public static void CommandReport(IPlayer player, string message = "")
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (message == "")
            {
                player.SendSyntaxMessage($"/report [Message]");
                return;
            }

            if (message.Length < 5)
            {
                player.SendErrorNotification("Message length must be greater than five!");
                return;
            }

            int reportCount =
                AdminHandler.AdminReports.Count(x => x.Player.GetClass().AccountId == player.GetClass().AccountId);

            if (reportCount >= 1)
            {
                player.SendErrorNotification("You can only have one report open at a time.");
                return;
            }
            AdminReport newReport = AdminHandler.AddAdminReport(player, message);

            player.SendInfoNotification($"You've created a new report id {newReport.Id}. Please await for further assistance.");

            List<IPlayer> onlineAdmins = Alt.Server.GetPlayers().ToList();

            if (onlineAdmins.Any())
            {
                foreach (IPlayer onlineAdmin in onlineAdmins)
                {
                    if (onlineAdmin == null) continue;

                    Models.Account adminAccount = onlineAdmin.FetchAccount();

                    if (adminAccount == null) continue;

                    if (adminAccount.AdminLevel < AdminLevel.Moderator && !adminAccount.Developer) continue;

                    onlineAdmin.SendAdminMessage($"New Report by {player.GetClass().Name} (PID: {player.GetPlayerId()}). Reason: {message}. Id: {newReport.Id}.");
                }
            }

            player.SendAdminMessage($"You've submitted a report. Your report Id is {newReport.Id}. You can use /cr to cancel the report");
        }

        [Command("cancelreport", commandType: CommandType.Character,
            description: "Used to cancel your active", alternatives: "cr")]
        public static void CommandCancelReport(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            AdminReport adminReport = AdminHandler.AdminReports.FirstOrDefault(x => x.Player == player);

            if (adminReport == null)
            {
                player.SendErrorNotification("You don't have an admin report outstanding.");
                return;
            }

            AdminReportObject? adminReportObject = AdminHandler.AdminReportObjects.FirstOrDefault
                (x => x.Id == adminReport.Id);

            if (adminReportObject != null)
            {
                SignalR.RemoveReport(adminReportObject);
            }

            AdminHandler.CloseReport(adminReport.Id);

            player.SendInfoNotification($"You've closed your admin report.");
        }

        [Command("reportr", onlyOne: true, commandType: CommandType.Character,
            description: "Used to reply to a report")]
        public static void CommandReportReply(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/reportr [ReportId] [Message]");
                return;
            }

            string[] argSplit = args.Split(" ");

            if (argSplit.Length < 2)
            {
                player.SendSyntaxMessage("/reportr [ReportId] [Message]");
                return;
            }

            bool idParse = int.TryParse(argSplit[0], out int reportId);

            if (!idParse)
            {
                player.SendSyntaxMessage("/reportr [ReportId] [Message]");
                return;
            }

            AdminReport adminReport = AdminHandler.AdminReports.FirstOrDefault(x => x.Id == reportId);

            if (adminReport == null)
            {
                player.SendErrorNotification("Report not found!");
                return;
            }

            if (adminReport.Player != player)
            {
                player.SendErrorNotification("You didn't create this report!");
                return;
            }

            string message = string.Join(' ', argSplit.Skip(1));

            player.SendAdminMessage($"Report Reply: {message}.");

            SignalR.SendReportMessage(reportId, message);
        }

        #endregion Report System

        #region Help Me System

        [Command("helpers", commandType: CommandType.Character, description: "See onduty helpers!")]
        public static void CommandViewHelpers(IPlayer player)
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            var onlineHelpers = Alt.GetAllPlayers().Where(x => x.HasSyncedMetaData(HelperCommands.HelperDutyData)).OrderByDescending(x => x.GetClass().UcpName);

            if (!onlineHelpers.Any())
            {
                player.SendErrorMessage("No on-duty helpers");
                return;
            }

            player.SendHelperMessage("____[On Duty Helpers]____");

            foreach (var onlineHelper in onlineHelpers)
            {
                player.SendHelperMessage(onlineHelper.GetClass().UcpName);
            }
        }

        [Command("helpme", onlyOne: true, commandType: CommandType.Character, description: "Get help from a Helper!")]
        public static void CommandHelpMe(IPlayer player, string message = "")
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (message == "")
            {
                player.SendSyntaxMessage($"/helpme [Message]");
                return;
            }

            if (message.Length < 5)
            {
                player.SendErrorNotification("Message length must be greater than five!");
                return;
            }

            int helpCount =
                AdminHandler.HelpReports.Count(x => x.Player.GetClass().AccountId == player.GetClass().AccountId);

            if (helpCount >= 1)
            {
                player.SendErrorNotification("You can only have one helpme open at a time.");
                return;
            }

            HelpReport newReport = AdminHandler.AddHelpReport(player, message);

            player.SendInfoNotification($"You've created a new helpme id {newReport.Id}. Please await for further assistance.");

            List<IPlayer> onlinePlayers = Alt.Server.GetPlayers().ToList();

            if (onlinePlayers.Any())
            {
                foreach (IPlayer onlinePlayer in onlinePlayers)
                {
                    Models.Account playerAccount = onlinePlayer?.FetchAccount();

                    if (playerAccount == null) continue;

                    if (!playerAccount.Helper) continue;

                    onlinePlayer.SendHelperMessage($"New Help Me by {player.GetClass().Name} (PID: {player.GetPlayerId()}). Request: {message}. Id: {newReport.Id}.");
                }
            }

            player.SendHelperMessage($"You've submitted a help me. Your help me Id is {newReport.Id}.");
        }

        [Command("cancelhelp", commandType: CommandType.Character,
            description: "Used to cancel your active helpme", alternatives: "ch")]
        public static void CommandCancelHelp(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            HelpReport helpReport = AdminHandler.HelpReports.FirstOrDefault(x => x.Player == player);

            if (helpReport == null)
            {
                player.SendErrorNotification("You don't have an admin report outstanding.");
                return;
            }

            AdminHandler.CloseHelpReport(helpReport.Id);

            player.SendInfoNotification($"You've closed your help request.");
        }

        [Command("helpr", onlyOne: true, commandType: CommandType.Character,
            description: "Used to reply to a helpme")]
        public static void CommandHelpReply(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/helpr [HelpId] [Message]");
                return;
            }

            string[] argSplit = args.Split(" ");

            if (argSplit.Length < 2)
            {
                player.SendSyntaxMessage("/helpr [HelpId] [Message]");
                return;
            }

            bool idParse = int.TryParse(argSplit[0], out int reportId);

            if (!idParse)
            {
                player.SendSyntaxMessage("/helpr [HelpId] [Message]");
                return;
            }

            HelpReport helpReport = AdminHandler.HelpReports.FirstOrDefault(x => x.Id == reportId);

            if (helpReport == null)
            {
                player.SendErrorNotification("Request not found!");
                return;
            }

            if (helpReport.Player != player)
            {
                player.SendErrorNotification("You didn't create this report!");
                return;
            }

            string message = string.Join(' ', argSplit.Skip(1));

            player.SendAdminMessage($"Help Reply: {message}.");
        }

        #endregion Help Me System

        [Command("describe", onlyOne: true, alternatives: "description", commandType: CommandType.Character, description: "Sets your description for players to look at")]
        public static void CommandSetDescription(IPlayer player, string args = "")
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/describe [Description]");
                return;
            }

            if (args.Length < 5)
            {
                player.SendErrorNotification("Description not long enough!");
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            playerCharacter.Description = args;

            context.SaveChanges();

            player.SendInfoNotification($"You've set your description to: {args}");
        }

        [Command("lookat", onlyOne: true, commandType: CommandType.Character, description: "Looks at other players description")]
        public static void CommandLookAt(IPlayer player, string args = "")
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/lookat [IdOrName]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 8)
            {
                player.SendErrorNotification("Your not near the player.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetPlayer.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not found.");
                return;
            }

            if (string.IsNullOrEmpty(targetCharacter.Description) || targetCharacter.Description.Length < 3)
            {
                player.SendErrorNotification("No description found.");
                return;
            }

            player.SendEmoteMessage($"looks at {targetCharacter.Name}.");

            player.SendInfoNotification($"Description: {targetCharacter.Description}");
        }

        [Command("pay", onlyOne: true, commandType: CommandType.Bank, description: "Sends money to other players")]
        public static void CommandPay(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/pay [IdOrName] [Amount]");
                return;
            }

            string[] split = args.Split(" ");

            if (split.Length != 2)
            {
                player.SendSyntaxMessage("/pay [IdOrName] [Amount]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(split[0]);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendErrorNotification("Player not logged in.");
                return;
            }

            if (player.Position.Distance(targetPlayer.Position) > 5)
            {
                player.SendErrorNotification("You must be closer.");
                return;
            }

            bool tryParse = double.TryParse(split[1], out double result);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/pay [IdOrName] [Amount]");
                return;
            }

            if (result < 0)
            {
                player.SendErrorNotification("Don't be stupid!");
                return;
            }

            if (player.FetchCharacter().Money < result)
            {
                player.SendErrorNotification("You don't have this much on you.");
                return;
            }

            player.RemoveCash(result);
            targetPlayer.AddCash(result);

            Logging.AddToCharacterLog(player, $"has sent {result:C} to {targetPlayer.GetClass().Name}.");
            Logging.AddToCharacterLog(targetPlayer, $"has received {result:C} from {player.GetClass().Name}.");

            player.SendPayMessage($"You've given {result:C} to {targetPlayer.GetClass().Name}.");

            targetPlayer.SendPayMessage($"You've received {result:C} from {player.GetClass().Name}.");
        }

        [Command("characters", commandType: CommandType.Character, description: "Takes you back to the character room")]
        public static void CommandCharacters(IPlayer player)
        {
            if (player.FetchAccount() == null)
            {
                player.SendErrorNotification("Your not logged in!");
                return;
            }

            if (player.FetchAccount().InJail)
            {
                player.SendPermissionError();
                return;
            }

            if (player.GetClass().Downed)
            {
                player.SendErrorNotification("You can't do that when downed.");
                return;
            }

            player.StopMusic();

            player.GetClass().CreatorRoom = true;

            player.SendInfoNotification($"Sending you to the character creator room.");

            Logging.AddToCharacterLog(player, $"has used /characters to TP to the character selection room.");

            DiscordHandler.SendMessageToLogChannel(
                $"{player.FetchAccount().Username} ({player.GetClass().Name}) has used /characters to TP to the character selection room.");

            using Context context = new Context();

            var playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            playerCharacter.PosX = player.Position.X;
            playerCharacter.PosY = player.Position.Y;
            playerCharacter.PosZ = player.Position.Z;

            playerCharacter.Dimension = player.Dimension;

            context.SaveChanges();

            CreatorRoom.SendToCreatorRoom(player);
        }

        [Command("info", commandType: CommandType.Character, description: "Shows some info from your character")]
        public static void CommandInfo(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Bank Details"),
                new NativeMenuItem("Property Addresses"),
                new NativeMenuItem("Phone Numbers")
            };

            NativeMenu menu = new NativeMenu("character:showInfoMenu", "Info", "Information about character information", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnShowInfoMenu(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Bank Details")
            {
                List<BankAccount> characterAccounts = BankAccount.FindCharacterBankAccounts(player.FetchCharacter());

                if (!characterAccounts.Any())
                {
                    player.SendErrorNotification("You don't have any bank accounts.");
                    return;
                }

                foreach (BankAccount characterAccount in characterAccounts)
                {
                    player.SendInfoNotification($"Account Type: {characterAccount.AccountType.ToString()} - Account Number: {characterAccount.AccountNumber} - Card Number: {characterAccount.CardNumber} - Card PIN: {characterAccount.Pin}.");
                }

                return;
            }

            if (option == "Property Addresses")
            {
                List<Models.Property> properties = Models.Property.FetchCharacterProperties(player.FetchCharacter());

                if (!properties.Any())
                {
                    player.SendErrorNotification("You don't own any properties.");
                    return;
                }

                foreach (Models.Property property in properties)
                {
                    player.SendInfoNotification($"Address: {property.Address} - Value: {property.Value:C}");
                }

                return;
            }

            if (option == "Phone Numbers")
            {
                using Context context = new Context();

                List<Phones> phones = context.Phones.Where(x => x.CharacterId == player.GetClass().CharacterId)
                    .ToList();

                if (!phones.Any())
                {
                    player.SendErrorNotification("You don't own any phones.");
                    return;
                }

                foreach (Phones phone in phones)
                {
                    string status = phone.TurnedOn ? "On" : "Off";

                    player.SendInfoNotification($"Phone Number: {phone.PhoneNumber} - Status: {status}.");
                }

                return;
            }
        }

        [Command("stats", commandType: CommandType.Character, description: "Show your stats")]
        public static void CommandStats(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

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
                        if (motelRoom.OwnerId == playerCharacter.Id)
                        {
                            motelRooms.Add(motelRoom);
                        }
                    }
                }
            }

            int bankAccountCount = BankAccount.FindCharacterBankAccounts(playerCharacter).Count;

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            List<AdminRecord> playerAdminRecords = AdminRecord.FetchAdminRecords(playerCharacter.OwnerId);
            int banCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Ban);
            int kickCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Kick);
            int jailCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Jail);
            int warnCount = playerAdminRecords.Count(x => x.RecordType == AdminRecordType.Warning);

            player.SendStatsMessage($"Showing stats for {playerCharacter.Name}");
            player.SendStatsMessage($"Username: {player.FetchAccount().Username}, Playtime: {playerCharacter.TotalHours}:{playerCharacter.TotalMinutes}");
            player.SendStatsMessage($"Account Id: {playerCharacter.OwnerId}, Character Id: {playerCharacter.Id}, Inventory Id: {playerCharacter.InventoryID}");
            player.SendStatsMessage($"Cash: {playerCharacter.Money:C}, Dimension: {player.Dimension}, Next Payday Earning: {playerCharacter.PaydayAmount:C}");
            player.SendStatsMessage($"Active Number: {playerCharacter.ActivePhoneNumber}, Payday Account: {playerCharacter.PaydayAccount}, Bank Accounts: {bankAccountCount}");
            if (activeFaction != null)
            {
                PlayerFaction activePlayerFaction = JsonConvert
                    .DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList)
                    .FirstOrDefault(x => x.Id == playerCharacter.ActiveFaction);

                Rank playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                    .FirstOrDefault(x => x.Id == activePlayerFaction?.RankId);

                player.SendStatsMessage($"Active Faction: {activeFaction.Name}, Rank: {playerRank?.Name}");
            }

            if (motelRooms.Any())
            {
                player.SendStatsMessage($"Motel: {motelList.FirstOrDefault(x => x.Id == motelRooms.FirstOrDefault()?.MotelId)?.Name} - Room {motelRooms.FirstOrDefault()?.Id}.");
            }

            player.SendStatsMessage($"Bans: {banCount}, Kicks: {kickCount}, Jails: {jailCount}, Warnings: {warnCount}, AFK Kicks: {player.FetchAccount().AfkKicks}.");
        }

        [Command("reloadtext", commandType: CommandType.Character, description: "Other: Used to reload text draws")]
        public static void CommandReloadText(IPlayer player)
        {
            TextLabelHandler.RemoveAllTextLabelsForPlayer(player);
            TextLabelHandler.LoadTextLabelsOnSpawn(player);

            player.SendNotification("Text Labels reloaded!");
        }

        [Command("weather", commandType: CommandType.Character, description: "Other: Shows the weather forecast")]
        public static void CommandShowWeather(IPlayer player)
        {
            OpenWeather currentWeather = TimeWeather.CurrentWeather;

            if (currentWeather == null)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            string weatherDesc = textInfo.ToTitleCase(currentWeather.weather.First().description);
            double temperatureDegC = Math.Round(currentWeather.main.temp);
            double temperatureHighDegC = Math.Round(currentWeather.main.temp_max);
            double temperatureLowDegC = Math.Round(currentWeather.main.temp_min);

            player.SendChatMessage("{3399ff}Weather for Los Santos, US is{ffffff}" + $" {weatherDesc.CapitalizeFirst()}");
            player.SendChatMessage("{3399ff}Temperature: {ffffff}" + $"{temperatureDegC} DegC / {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(temperatureDegC))} DegF.");
            player.SendChatMessage("{3399ff}Today's High: {ffffff}" + $"{temperatureHighDegC} DegC / {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(temperatureHighDegC))} DegF, Today's Low: {temperatureLowDegC} DegC / {Math.Round(ConvertTemp.ConvertCelsiusToFahrenheit(temperatureLowDegC))} DegF.");
            player.SendChatMessage("{3399ff}((Real World Weather Location: {ffffff}" + $"{currentWeather.name}." + "{3399ff}))");
        }

        [Command("dice", onlyOne: true, commandType: CommandType.Character, description: "Other: Used to roll a set number.")]
        public static void CharacterCommandDice(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/dice [Sides of Dice]");
                return;
            }

            bool tryParse = int.TryParse(args, out int sideCount);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/dice [Sides of Dice]");
                return;
            }

            Random rnd = new Random();

            player.SendEmoteMessage($"rolls a {sideCount} sided dice. It lands on {rnd.Next(1, sideCount + 1)}.");
        }

        [Command("id", onlyOne: true, commandType: CommandType.Character,
            description: "Used to find the name or Id of a player")]
        public static void CharacterCommandId(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/id [IdOrName]");
                return;
            }

            bool tryParse = int.TryParse(args, out int playerId);

            if (tryParse)
            {
                IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == playerId);

                if (targetPlayer == null)
                {
                    player.SendNotification($"~r~Unable to find player by Id of {playerId}.");
                    return;
                }

                player.SendInfoNotification($"Player Name: {targetPlayer.GetClass().Name} - Id: {playerId}.");
                return;
            }

            List<IPlayer> targetList = Alt.Server.GetPlayers()
                .Where(x => x.GetClass().Name.ToLower().Contains(args.ToLower())).ToList();

            if (!targetList.Any())
            {
                player.SendNotification($"~r~Unable to find player by name of {args}.");
                return;
            }

            foreach (IPlayer target in targetList)
            {
                player.SendInfoNotification($"Player Name: {target.GetClass().Name} - Id: {target.GetPlayerId()}.");
            }
        }

        [Command("showid", onlyOne: true, commandType: CommandType.Character,
            description: "Other: Used to show Id to a player")]
        public static void CharacterCommandShowId(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/showid [NameOrId]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendNotification("~r~Unable to find player.");
                return;
            }

            if (targetPlayer.FetchCharacter() == null)
            {
                player.SendNotification("~r~Player not spawned in!");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> idItems = playerInventory.GetInventoryItems("ITEM_DRIVING_LICENSE");

            if (!idItems.Any())
            {
                player.SendNotification("~r~You don't have any Id items on you!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in idItems)
            {
                menuItems.Add(new NativeMenuItem(inventoryItem.CustomName, inventoryItem.ItemValue));
            }

            NativeMenu menu = new NativeMenu("CharacterCommands:ShowIdSelect", "Id's", "Select the Id you want to show", menuItems)
            {
                PassIndex = true
            };

            player.SetData("ShowingIdTo", targetPlayer.GetPlayerId());

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnShowIdSelect(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            player.GetData("ShowingIdTo", out int targetPlayerId);

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> idItems = playerInventory.GetInventoryItems("ITEM_DRIVING_LICENSE");

            InventoryItem selectedItem = idItems[index];

            if (selectedItem == null)
            {
                player.SendNotification("~r~An error occurred fetching the item.");
                return;
            }

            IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == targetPlayerId);

            if (targetPlayer?.FetchCharacter() == null)
            {
                player.SendNotification("~r~Unable to find target.");
                return;
            }

            if (selectedItem.ItemValue == player.GetClass().Name)
            {
                // Is their Id card
                player.SendEmoteMessage($"shows their {selectedItem.CustomName} to {targetPlayer.GetClass().Name}.");
                targetPlayer.SendNotification($"~y~Name: ~w~{selectedItem.ItemValue}\n~y~Age: ~w~{player.FetchCharacter().Age}.");
            }
            else
            {
                Models.Character selectedCharacter = Models.Character.GetCharacter(selectedItem.ItemValue);
                player.SendEmoteMessage($"shows their {selectedItem.CustomName} to {targetPlayer.GetClass().Name}.");
                if (selectedCharacter == null)
                {
                    targetPlayer.SendNotification($"~y~Name: ~w~{selectedItem.ItemValue}.");
                }
                else
                {
                    targetPlayer.SendNotification($"~y~Name: ~w~{selectedItem.ItemValue}\n~y~Age: ~w~{selectedCharacter.Age}.");
                }
            }
        }

        [Command("makeup", commandType: CommandType.Character,
            description: "Used to adjust your characters appearance")]
        public static void CharacterCommandMakeup(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendPermissionError();
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool hasItem = playerInventory.HasItem("ITEM_MAKEUP");

            if (!hasItem)
            {
                player.SendErrorNotification("You need to have a makeup bag!");
                return;
            }

            MakeupHandler.ShowMakeupMenu(player);
        }

        [Command("logout", commandType: CommandType.Character, description: "Other: Used to logout safely",
            alternatives: "q")]
        public static void CharacterCommandLogout(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("You must be spawned.");
                return;
            }

            if (player.GetClass().CreatorRoom)
            {
                player.SendErrorNotification("You must be out of the character areas.");
                return;
            }

            playerCharacter.PosX = player.Position.X;
            playerCharacter.PosY = player.Position.Y;
            playerCharacter.PosZ = player.Position.Z;

            playerCharacter.Dimension = player.Dimension;

            context.SaveChanges();

            Logging.AddToCharacterLog(player, $"has logged out.");

            player.Emit("SendPlayerLogout");
        }

        [Command("addgps", onlyOne: true, commandType: CommandType.Character,
            description: "Used to add a GPS waypoint to your GPS.")]
        public static void CharacterCommandAddGps(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/addgps [Waypoint Name]");
                return;
            }

            if (args.Trim().Length < 2)
            {
                player.SendErrorNotification("You must input a longer word to save!");
                player.SendSyntaxMessage("/addgps [Waypoint Name]");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            if (!gpsItems.Any())
            {
                player.SendErrorNotification("You don't have any GPS items on you!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in gpsItems)
            {
                if (!string.IsNullOrEmpty(inventoryItem.ItemValue))
                {
                    Gps gps = JsonConvert.DeserializeObject<Gps>(inventoryItem.ItemValue);
                    menuItems.Add(new NativeMenuItem(gps.Name));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
                }
            }

            player.SetData("AddWayPointName", args);

            NativeMenu menu = new NativeMenu("CharacterCommands:AddGpsWaypoint", "GPS", "Select a GPS to add to", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectGpsAddWayPoint(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("AddWayPointName");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            InventoryItem selectedGpsItem = gpsItems[index];

            if (selectedGpsItem == null)
            {
                player.SendErrorNotification("An error occurred fetching the GPS!");
                return;
            }

            playerInventory.RemoveItem(selectedGpsItem);

            player.GetData("AddWayPointName", out string wayPointName);

            Gps gps;

            if (!string.IsNullOrEmpty(selectedGpsItem.ItemValue))
            {
                gps = JsonConvert.DeserializeObject<Gps>(selectedGpsItem.ItemValue);

                gps.WayPoints.Add(new GpsWayPoint(wayPointName, player.Position));
            }
            else
            {
                gps = new Gps { WayPoints = new List<GpsWayPoint> { new GpsWayPoint(wayPointName, player.Position) } };
            }

            InventoryItem newGps = new InventoryItem("ITEM_GPS", "GPS", JsonConvert.SerializeObject(gps));

            playerInventory.AddItem(newGps);

            if (string.IsNullOrEmpty(gps.Name))
            {
                player.SendInfoNotification($"You've added the waypoint: {wayPointName} to your GPS.");
                return;
            }

            player.SendInfoNotification($"You've added the waypoint: {wayPointName} to your GPS: {gps.Name}.");
        }

        [Command("renamegps", onlyOne: true, commandType: CommandType.Character,
            description: "Used to rename a GPS")]
        public static void CharacterCommandRenameGps(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/renamegps [Name]");
                return;
            }

            if (args.Trim().Length < 2)
            {
                player.SendErrorNotification("You must input a longer word to save!");
                player.SendSyntaxMessage("/renamegps [Name]");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            if (!gpsItems.Any())
            {
                player.SendErrorNotification("You don't have any GPS items on you!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in gpsItems)
            {
                if (!string.IsNullOrEmpty(inventoryItem.ItemValue))
                {
                    Gps gps = JsonConvert.DeserializeObject<Gps>(inventoryItem.ItemValue);
                    menuItems.Add(new NativeMenuItem(gps.Name));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
                }
            }

            player.SetData("AddGpsName", args);

            NativeMenu menu = new NativeMenu("CharacterCommands:AddGpsName", "GPS", "Select a GPS to name", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectGpsRename(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("AddGpsName");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            InventoryItem selectedGpsItem = gpsItems[index];

            if (selectedGpsItem == null)
            {
                player.SendErrorNotification("An error occurred fetching the GPS!");
                return;
            }

            playerInventory.RemoveItem(selectedGpsItem);

            player.GetData("AddGpsName", out string gpsName);

            Gps gps;

            if (!string.IsNullOrEmpty(selectedGpsItem.ItemValue))
            {
                gps = JsonConvert.DeserializeObject<Gps>(selectedGpsItem.ItemValue);

                gps.Name = gpsName;
            }
            else
            {
                gps = new Gps
                {
                    Name = gpsName,
                    WayPoints = new List<GpsWayPoint>()
                };
            }

            selectedGpsItem.ItemValue = JsonConvert.SerializeObject(gps);

            InventoryItem newGps = new InventoryItem("ITEM_GPS", "GPS", JsonConvert.SerializeObject(gps));

            playerInventory.AddItem(newGps);

            if (string.IsNullOrEmpty(gps.Name))
            {
                player.SendInfoNotification($"You've removed the name from your GPS.");
                return;
            }

            player.SendInfoNotification($"You've renamed your GPS to {gpsName}.");
        }

        [Command("gps", commandType: CommandType.Character,
            description: "Uses a GPS")]
        public static void CharacterCommandGps(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            if (!gpsItems.Any())
            {
                player.SendErrorNotification("You don't have any GPS items on you!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in gpsItems)
            {
                if (!string.IsNullOrEmpty(inventoryItem.ItemValue))
                {
                    Gps gps = JsonConvert.DeserializeObject<Gps>(inventoryItem.ItemValue);
                    menuItems.Add(new NativeMenuItem(gps.Name));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
                }
            }

            NativeMenu menu = new NativeMenu("CharacterCommands:gps:gpsSelected", "GPS", "Select a GPS", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectGps(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            InventoryItem selectedGpsItem = gpsItems[index];

            if (selectedGpsItem == null)
            {
                player.SendErrorNotification("An error occurred fetching the GPS!");
                return;
            }

            if (string.IsNullOrEmpty(selectedGpsItem.ItemValue))
            {
                player.SendErrorNotification("You don't have any way points set!");
                return;
            }

            Gps gps = JsonConvert.DeserializeObject<Gps>(selectedGpsItem.ItemValue);

            if (!gps.WayPoints.Any())
            {
                player.SendErrorNotification("There are no GPS way points on this device.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (GpsWayPoint gpsWayPoint in gps.WayPoints)
            {
                menuItems.Add(new NativeMenuItem(gpsWayPoint.Name));
            }

            player.SetData("SelectedGPS", index);

            NativeMenu menu = new NativeMenu("CharacterCommands:gps:wayPointSelected", gps.Name, "Select a way point", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void GpsWayPointSelect(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("SelectedGPS");
                return;
            }

            player.GetData("SelectedGPS", out int gpsIndex);

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            InventoryItem selectedGpsItem = gpsItems[gpsIndex];

            Gps gps = JsonConvert.DeserializeObject<Gps>(selectedGpsItem.ItemValue);

            GpsWayPoint selectedWayPoint = gps.WayPoints[index];

            if (selectedWayPoint == null)
            {
                player.SendErrorNotification("An error occurred fetching the way point.");
                return;
            }

            player.RemoveWaypoint();
            player.SetWaypoint(new Position(selectedWayPoint.PosX, selectedWayPoint.PosY, selectedWayPoint.PosZ));

            player.SendInfoNotification($"Way point set to: {selectedWayPoint.Name}.");
            player.DeleteData("SelectedGPS");
        }

        [Command("removewaypoint", commandType: CommandType.Character,
            description: "Removes a GPS waypoint")]
        public static void CharacterCommandRemoveGpsWaypoint(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            if (!gpsItems.Any())
            {
                player.SendErrorNotification("You don't have any GPS items on you!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem inventoryItem in gpsItems)
            {
                if (!string.IsNullOrEmpty(inventoryItem.ItemValue))
                {
                    Gps gps = JsonConvert.DeserializeObject<Gps>(inventoryItem.ItemValue);
                    menuItems.Add(new NativeMenuItem(gps.Name));
                }
                else
                {
                    menuItems.Add(new NativeMenuItem(inventoryItem.CustomName));
                }
            }

            NativeMenu menu = new NativeMenu("CharacterCommands:gps:gpsSelectedRemoveWaypoint", "GPS", "Select a GPS", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSelectGpsSelectedRemoveWaypoint(IPlayer player, string option, int index)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            InventoryItem selectedGpsItem = gpsItems[index];

            if (selectedGpsItem == null)
            {
                player.SendErrorNotification("An error occurred fetching the GPS!");
                return;
            }

            if (string.IsNullOrEmpty(selectedGpsItem.ItemValue))
            {
                player.SendErrorNotification("You don't have any way points set!");
                return;
            }

            Gps gps = JsonConvert.DeserializeObject<Gps>(selectedGpsItem.ItemValue);

            if (!gps.WayPoints.Any())
            {
                player.SendErrorNotification("There are no GPS way points on this device.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (GpsWayPoint gpsWayPoint in gps.WayPoints)
            {
                menuItems.Add(new NativeMenuItem(gpsWayPoint.Name));
            }

            player.SetData("SelectedGPS", index);

            NativeMenu menu = new NativeMenu("CharacterCommands:gps:removeWayPointSelected", gps.Name, "Select a way point", menuItems) { PassIndex = true };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void GpsRemoveWayPointSelected(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("SelectedGPS");
                return;
            }

            player.GetData("SelectedGPS", out int gpsIndex);

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> gpsItems = playerInventory.GetInventoryItems("ITEM_GPS");

            InventoryItem selectedGpsItem = gpsItems[gpsIndex];

            Gps gps = JsonConvert.DeserializeObject<Gps>(selectedGpsItem.ItemValue);

            GpsWayPoint selectedWayPoint = gps.WayPoints[index];

            if (selectedWayPoint == null)
            {
                player.SendErrorNotification("An error occurred fetching the way point.");
                return;
            }

            playerInventory.RemoveItem(selectedGpsItem);

            gps.WayPoints.Remove(selectedWayPoint);

            selectedGpsItem.ItemValue = JsonConvert.SerializeObject(gps);

            InventoryItem newGps = new InventoryItem("ITEM_GPS", "GPS", JsonConvert.SerializeObject(gps));

            playerInventory.AddItem(newGps);

            player.SendInfoNotification($"The way point: {selectedWayPoint.Name} has been removed from your GPS.");
        }

        [Command("frisk", onlyOne: true, commandType: CommandType.Character, description: "Frisks another player")]
        public static void CharacterCommandFrisk(IPlayer player, string args = "")
        {
            if (!player.IsSpawned())
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/frisk [NameOrId]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find a target player.");
                return;
            }

            if (!targetPlayer.IsSpawned())
            {
                player.SendErrorNotification("Target isn't spawned.");
                return;
            }

            float distance = player.Position.Distance(targetPlayer.Position);

            if (distance > 3)
            {
                player.SendErrorNotification("Your not in range of the player.");
                return;
            }

            targetPlayer.SetData("FriskedBy", player.GetPlayerId());

            player.SendInfoNotification($"You've tried to frisk {targetPlayer.GetClass().Name}.");
            targetPlayer.SendInfoNotification($"{player.GetClass().Name} has tried to frisk you. Use /acceptfrisk {player.GetPlayerId()} to accept");
        }

        [Command("acceptfrisk", onlyOne: true, commandType: CommandType.Character,
            description: "Used to accept a frisk")]
        public static void CharacterCommandAcceptFrisk(IPlayer player, string args)
        {
            if (!player.IsSpawned())
            {
                player.SendPermissionError();
                return;
            }

            if (args == "")
            {
                player.SendSyntaxMessage("/acceptfrisk [Id]");
                return;
            }

            bool tryParse = int.TryParse(args, out int friskId);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/acceptfrisk [Id]");
                return;
            }

            IPlayer friskingPlayer = Alt.Server.GetPlayers().FirstOrDefault(i => i.GetPlayerId() == friskId);

            if (friskingPlayer == null || !friskingPlayer.IsSpawned())
            {
                player.SendErrorNotification("Unable to find a target player.");
                return;
            }

            float distance = player.Position.Distance(friskingPlayer.Position);

            if (distance > 3)
            {
                player.SendErrorNotification("Your not in range of the player.");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("An error occurred fetching your inventory.");
                return;
            }

            InventoryCommands.ShowInventoryToPlayer(friskingPlayer, playerInventory, false, player.FetchCharacter().InventoryID);

            player.SendInfoNotification($"You've been frisked by {friskingPlayer.GetClass().Name}.");
        }
    }
}