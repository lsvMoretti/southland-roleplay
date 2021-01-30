using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Models;
using Server.Vehicle;

namespace Server.Groups
{
    public class LeaderCommands
    {
        [Command("fpark", commandType: CommandType.Faction, description: "[Leader] Parks a faction vehicle")]
        public static void LeaderCommandFPark(IPlayer player)
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle");
                return;
            }

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            bool canFPark = playerFaction.Leader;

            bool isAdmin = false;

            if (!canFPark)
            {
                canFPark = player.FetchAccount().AdminLevel >= AdminLevel.HeadAdmin;

                isAdmin = player.FetchAccount().AdminLevel >= AdminLevel.HeadAdmin;

                if (!canFPark)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            Faction vehicleFaction = Faction.FetchFaction(player.Vehicle.FetchVehicleData().FactionId);

            if (vehicleFaction == null)
            {
                player.SendErrorNotification("You must be in a faction vehicle.");
                return;
            }

            bool isCorrectFaction = vehicleFaction.Id == activeFaction.Id;

            if (!isCorrectFaction)
            {
                isCorrectFaction = vehicleFaction.Id == playerFaction.Id;

                if (!isCorrectFaction && !isAdmin)
                {
                    player.SendErrorNotification("You must be in the faction.");
                    return;
                }
            }

            using Context context = new Context();

            Models.Vehicle vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

            if (vehicleDb == null)
            {
                player.SendErrorNotification("An error occurred fetching the vehicle data.");
                return;
            }

            IVehicle playerVehicle = player.Vehicle;

            DegreeRotation rotation = playerVehicle.Rotation;

            vehicleDb.PosX = playerVehicle.Position.X;
            vehicleDb.PosY = playerVehicle.Position.Y;
            vehicleDb.PosZ = playerVehicle.Position.Z;

            vehicleDb.RotZ = rotation.Yaw;

            context.SaveChanges();

            player.SendInfoNotification($"Vehicle position updated.");
        }

        [Command("ftow", commandType: CommandType.Faction, description: "[Leader] Tows the faction vehicles")]
        public static async void LeaderCommandFTow(IPlayer player)
        {
            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction? playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            Rank? playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                .FirstOrDefault(x => x.Id == playerFaction.RankId);

            if (playerRank == null)
            {
                player.SendPermissionError();
                return;
            }

            bool canFTow = playerFaction.Leader;

            if (!canFTow)
            {
                if (!playerRank.Tow)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            using Context context = new Context();

            List<Models.Vehicle> factionVehicles = context.Vehicle.Where(x => x.FactionId == activeFaction.Id).ToList();

            int removeCount = 0;
            int loadCount = 0;

            foreach (Models.Vehicle factionVehicle in factionVehicles)
            {
                IVehicle targetVehicle =
                    Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == factionVehicle.Id);

                if (targetVehicle != null)
                {
                    if (targetVehicle.Occupants().Any()) continue;

                    await targetVehicle.RemoveAsync();
                    removeCount++;
                }

                if (!string.IsNullOrEmpty(factionVehicle.GarageId)) continue;

                IVehicle vehicle = await LoadVehicle.LoadDatabaseVehicleAsync(factionVehicle,
                    new Position(factionVehicle.PosX, factionVehicle.PosY, factionVehicle.PosZ), true);

                loadCount++;
            }

            player.SendInfoNotification($"Removed {removeCount} vehicles, Loaded {loadCount} vehicles.");

            Logging.AddToCharacterLog(player, $"has respawned {activeFaction.Name}'s vehicles.");
        }

        [Command("invite", onlyOne: true, commandType: CommandType.Faction,
            description: "[Leader] Invite a player to a faction")]
        public static void LeaderCommandInvite(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/invite [IdOrName]");
                return;
            }

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            bool canInvite = playerFaction.Leader;

            if (!canInvite)
            {
                Rank playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                    .FirstOrDefault(x => x.Id == playerFaction.RankId);

                if (playerRank == null)
                {
                    player.SendPermissionError();
                    return;
                }

                canInvite = playerRank.Invite;

                if (!canInvite)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find the player.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            player.SetData("ainvite:targetCharacter", targetPlayer.GetClass().CharacterId);
            player.SetData("ainvite:targetFactionId", activeFaction.Id);

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);

            foreach (Rank factionRank in factionRanks.OrderBy(x => x.Id))
            {
                menuItems.Add(new NativeMenuItem(factionRank.Name));
            }

            NativeMenu menu = new NativeMenu("admin:faction:ainvite:showRanks", "Ranks", "Select a Rank", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        [Command("uninvite", onlyOne: true, commandType: CommandType.Faction,
            description: "[Leader] Removes a player from a faction")]
        public static void LeaderCommandUnInvite(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/uninvite [IdOrName]");
                return;
            }

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            bool canInvite = playerFaction.Leader;

            if (!canInvite)
            {
                Rank playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                    .FirstOrDefault(x => x.Id == playerFaction.RankId);

                if (playerRank == null)
                {
                    player.SendPermissionError();
                    return;
                }

                canInvite = playerRank.Invite;

                if (!canInvite)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find the player.");
                return;
            }

            if (targetPlayer == player)
            {
                player.SendErrorNotification("You are unable to un-invite yourself!");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("The player is not logged in.");
                return;
            }

            List<PlayerFaction> targetFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(targetCharacter.FactionList);

            PlayerFaction targetFaction = targetFactions.FirstOrDefault(x => x.Id == playerFaction.Id);

            if (targetFaction == null)
            {
                player.SendErrorNotification("This player isn't in your faction!");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacterDb = context.Character.Find(targetCharacter.Id);

            if (targetCharacterDb == null)
            {
                player.SendErrorNotification("An error occured fetching the target database information.");
                return;
            }

            targetFactions.Remove(targetFaction);

            if (targetCharacterDb.ActiveFaction == targetFaction.Id)
            {
                targetCharacterDb.ActiveFaction = 0;
            }

            targetCharacterDb.FactionList = JsonConvert.SerializeObject(targetFactions);

            context.SaveChanges();

            player.SendInfoNotification($"You have removed {targetCharacter.Name} from the faction!");

            targetPlayer.SendInfoNotification(
                $"You have been removed from the {activeFaction.Name} by {player.GetClass().Name}.");

            Logging.AddToCharacterLog(player,
                $"has removed {targetCharacter.Name} from the faction {activeFaction.Name}.");
            Logging.AddToCharacterLog(targetPlayer,
                $"has been removed from the faction {activeFaction.Name} by {player.GetClass().Name}.");
        }

        [Command("giverank", onlyOne: true, commandType: CommandType.Faction,
            description: "[Leader] Adjusts a players rank")]
        public static void LeaderCommandGiveRank(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/giverank [IdOrName]");
                return;
            }

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            bool canInvite = playerFaction.Leader;

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);
            Rank playerRank = factionRanks.FirstOrDefault(x => x.Id == playerFaction.RankId);

            if (playerRank == null)
            {
                player.SendPermissionError();
                return;
            }

            if (!canInvite)
            {
                canInvite = playerRank.Promote;

                if (!canInvite)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Unable to find the player.");
                return;
            }

            if (targetPlayer == player)
            {
                player.SendErrorNotification("You are unable to adjust your rank yourself!");
                return;
            }

            bool inFaction = Faction.FetchFaction(targetPlayer.FetchCharacter().ActiveFaction) != null;

            if (!inFaction)
            {
                List<PlayerFaction> targetFactions =
                    JsonConvert.DeserializeObject<List<PlayerFaction>>(targetPlayer.FetchCharacter().FactionList);

                inFaction = targetFactions.FirstOrDefault(x => x.Id == playerFaction.Id) != null;

                if (!inFaction)
                {
                    player.SendErrorNotification("This player is not in your faction.");
                    return;
                }
            }

            //List of ranks below the players rank
            List<Rank> availableRanks = factionRanks.Where(x => x.Id < playerRank.Id).ToList();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Rank availableRank in availableRanks.OrderBy(x => x.Id))
            {
                menuItems.Add(new NativeMenuItem(availableRank.Name));
            }

            player.SetData("faction:giverank:targetPlayer", targetPlayer.GetClass().CharacterId);

            NativeMenu menu = new NativeMenu("faction:leadership:giverank", activeFaction.Name, "Select a Rank",
                menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnGiveRankMenuCalled(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("faction:giverank:targetPlayer", out int targetCharacterId);

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            List<Rank> rankList = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);

            Rank selectedRank = rankList.FirstOrDefault(x => x.Name == option);

            if (selectedRank == null)
            {
                player.SendErrorNotification("Unable to find this rank in the faction.");
                return;
            }

            IPlayer? targetPlayer =
                Alt.GetAllPlayers().FirstOrDefault(x => x.GetClass().CharacterId == targetCharacterId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("This player has logged out.");
                return;
            }

            Models.Character targetCharacter = targetPlayer.FetchCharacter();

            if (targetCharacter == null)
            {
                player.SendErrorNotification("This player isn't logged in.");
                return;
            }

            List<PlayerFaction> targetFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(targetCharacter.FactionList);

            PlayerFaction targetFaction = targetFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (targetFaction == null)
            {
                player.SendErrorNotification("This player isn't in your faction.");
                return;
            }

            if (targetFaction.RankId == selectedRank.Id)
            {
                player.SendErrorNotification($"The player already has the rank of {selectedRank.Name}.");
                return;
            }

            using Context context = new Context();

            Models.Character targetCharacterDb = context.Character.Find(targetCharacter.Id);

            if (targetCharacterDb == null)
            {
                player.SendErrorNotification("This player isn't logged in.");
                return;
            }

            targetFaction.RankId = selectedRank.Id;

            targetCharacterDb.FactionList = JsonConvert.SerializeObject(targetFactions);

            context.SaveChanges();

            player.SendInfoNotification($"You have changed {targetCharacter.Name}'s rank to {selectedRank.Name}.");

            targetPlayer.SendInfoNotification(
                $"You rank has been changed to {selectedRank.Name} by {player.GetClass().Name}.");

            Logging.AddToCharacterLog(player,
                $"has adjusted {targetCharacter.Name}'s rank to {selectedRank.Name} in the {activeFaction.Name} faction.");
            Logging.AddToCharacterLog(targetPlayer,
                $"rank has been adjusted by {player.GetClass().Name} to {selectedRank.Name} in the {activeFaction.Name} faction.");
        }

        [Command("ranks", commandType: CommandType.Faction, description: "[Leader] Shows the ranks for the faction")]
        public static void LeaderCommandViewRanks(IPlayer player)
        {
            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            bool addRank = playerFaction.Leader;

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);
            Rank playerRank = factionRanks.FirstOrDefault(x => x.Id == playerFaction.RankId);

            if (playerRank == null)
            {
                player.SendPermissionError();
                return;
            }

            if (!addRank)
            {
                addRank = playerRank.AddRanks;

                if (!addRank)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Rank factionRank in factionRanks.OrderBy(x => x.Id))
            {
                menuItems.Add(new NativeMenuItem(factionRank.Name));
            }

            menuItems.Add(new NativeMenuItem("Add Rank", "Adds a new faction rank."));

            NativeMenu menu = new NativeMenu("faction:leader:showFactionRanks", "Ranks", activeFaction.Name, menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnRanksRankSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Add Rank")
            {
                NativeUi.GetUserInput(player, "faction:leader:rank:rankName", "RankName");
                return;
            }

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);
            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);

            Rank selectedRank = factionRanks.FirstOrDefault(x => x.Name == option);

            if (selectedRank == null)
            {
                player.SendErrorNotification("Unable to find the selected rank.");
                return;
            }

            player.SetData("faction:leader:ranks:adjustRankPerm", selectedRank.Name);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Toggle Invite"),
                new NativeMenuItem("Toggle Promote"),
                new NativeMenuItem("Toggle Rank Edits"),
                new NativeMenuItem("Toggle FTow"),
                new NativeMenuItem("Delete Rank")
            };

            NativeMenu menu = new NativeMenu("faction:leader:editFactionRank", "Ranks", selectedRank.Name, menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnRanksEditRank(IPlayer player, string option)
        {
            if (option == "Close") return;

            using Context context = new Context();

            Faction factionDb = context.Faction.Find(player.FetchCharacter().ActiveFaction);

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(factionDb.RanksJson);

            player.GetData("faction:leader:ranks:adjustRankPerm", out string rankName);

            Rank selectedRank = factionRanks.FirstOrDefault(x => x.Name == rankName);

            if (selectedRank == null)
            {
                player.SendErrorNotification("Unable to find the selected rank.");
                return;
            }

            if (option == "Toggle Invite")
            {
                selectedRank.Invite = !selectedRank.Invite;

                player.SendInfoNotification(
                    $"You've set the invite permission for rank {selectedRank.Name} to {selectedRank.Invite}.");
                Logging.AddToCharacterLog(player,
                    $"has set rank {selectedRank.Name} invite permission to: {selectedRank.Invite} for faction {factionDb.Name}.");
            }

            if (option == "Toggle Promote")
            {
                selectedRank.Promote = !selectedRank.Promote;

                player.SendInfoNotification(
                    $"You've set the promote permission for rank {selectedRank.Name} to {selectedRank.Promote}.");
                Logging.AddToCharacterLog(player,
                    $"has set rank {selectedRank.Name} Promote permission to: {selectedRank.Promote} for faction {factionDb.Name}.");
            }

            if (option == "Toggle Rank Edits")
            {
                selectedRank.AddRanks = !selectedRank.AddRanks;

                player.SendInfoNotification(
                    $"You've set the add/remove rank permission for rank {selectedRank.Name} to {selectedRank.AddRanks}.");
                Logging.AddToCharacterLog(player,
                    $"has set rank {selectedRank.Name} AddRanks permission to: {selectedRank.AddRanks} for faction {factionDb.Name}.");
            }

            if (option == "Toggle FTow")
            {
                selectedRank.Tow = !selectedRank.Tow;

                player.SendInfoNotification(
                    $"You've set the add/remove ftow permission for rank {selectedRank.Name} to {selectedRank.Tow}.");
                Logging.AddToCharacterLog(player,
                    $"has set rank {selectedRank.Name} ftow permission to: {selectedRank.Tow} for faction {factionDb.Name}.");
            }

            if (option == "Delete Rank")
            {
                factionRanks.Remove(selectedRank);

                player.SendInfoNotification($"You've removed the rank {selectedRank.Name} from the faction.");
                Logging.AddToCharacterLog(player,
                    $"has removed rank {selectedRank.Name} from faction {factionDb.Name}.");
            }

            factionDb.RanksJson = JsonConvert.SerializeObject(factionRanks);

            context.SaveChanges();
        }

        public static void OnRanksAddRank(IPlayer player, string rankName)
        {
            if (string.IsNullOrEmpty(rankName)) return;

            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            using Context context = new Context();

            Faction dbFaction = context.Faction.Find(activeFaction.Id);

            if (dbFaction == null) return;

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(dbFaction.RanksJson);

            bool rankExists = factionRanks.FirstOrDefault(x => x.Name == rankName) != null;

            if (rankExists)
            {
                player.SendErrorNotification("A rank already exists with this name.");
                return;
            }

            int nextId = 1;

            for (int i = 1; i < 500; i++)
            {
                Rank idRank = factionRanks.FirstOrDefault(x => x.Id == i);

                if (idRank == null)
                {
                    nextId = i;
                    break;
                }
            }

            Rank newRank = new Rank
            {
                Name = rankName,
                AddRanks = false,
                Invite = false,
                Promote = false,
                Id = nextId
            };

            factionRanks.Add(newRank);

            dbFaction.RanksJson = JsonConvert.SerializeObject(factionRanks);

            context.SaveChanges();

            player.SendInfoNotification($"You've added {newRank.Name} to {activeFaction.Name}.");
        }

        [Command("members", commandType: CommandType.Faction, description: "[Leader] Shows the players of a faction")]
        public static void LeaderCommandViewMembers(IPlayer player)
        {
            Faction activeFaction = Faction.FetchFaction(player.FetchCharacter().ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You are not in an active faction.");
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(player.FetchCharacter().FactionList);

            PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("You are not in your active faction.");
                return;
            }

            bool addRank = playerFaction.Leader;

            List<Rank> factionRanks = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson);
            Rank playerRank = factionRanks.FirstOrDefault(x => x.Id == playerFaction.RankId);

            if (playerRank == null)
            {
                player.SendPermissionError();
                return;
            }

            if (!addRank)
            {
                addRank = playerRank.Invite;

                if (!addRank)
                {
                    player.SendPermissionError();
                    return;
                }
            }

            using Context context = new Context();

            List<Models.Character> characters = context.Character.ToList();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Models.Character character in characters.OrderBy(x => x.Name).ToList())
            {
                if (string.IsNullOrEmpty(character.FactionList)) continue;

                List<PlayerFaction> targetFactions =
                    JsonConvert.DeserializeObject<List<PlayerFaction>>(character.FactionList);

                if (targetFactions.Any(x => x.Id == activeFaction.Id))
                {
                    menuItems.Add(new NativeMenuItem(character.Name));
                }
            }

            NativeMenu menu = new NativeMenu("faction:leader:memberList", "Members", activeFaction.Name, menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnLeaderSelectMember(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.SetData("faction:leader:memberList:name", option);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Remove")
            };

            NativeMenu menu = new NativeMenu("faction:leader:memberList:selected", "Members", option, menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnLeaderSelectMemberOption(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("faction:leader:memberList:name", out string characterName);

            if (option == "Remove")
            {
                Context context = new Context();

                Models.Character characterDb = context.Character.FirstOrDefault(x => x.Name == characterName);

                if (characterDb == null)
                {
                    player.SendErrorNotification("Unable to fetch this characters database data.");
                    return;
                }

                List<PlayerFaction> playerFactions =
                    JsonConvert.DeserializeObject<List<PlayerFaction>>(characterDb.FactionList);

                PlayerFaction faction =
                    playerFactions.FirstOrDefault(x => x.Id == player.FetchCharacter().ActiveFaction);

                if (faction == null) return;

                playerFactions.Remove(faction);

                characterDb.FactionList = JsonConvert.SerializeObject(playerFactions);

                context.SaveChanges();

                player.SendInfoNotification($"You've removed {characterName} from your faction.");

                Logging.AddToCharacterLog(player,
                    $"has removed {characterName} from the faction Id: {player.FetchCharacter().ActiveFaction}.");
            }
        }
    }
}