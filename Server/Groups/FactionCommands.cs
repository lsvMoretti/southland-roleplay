using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Models;

namespace Server.Groups
{
    public class FactionCommands
    {
        [Command("faction", commandType: CommandType.Faction, description: "Allows you to manage your personal factions.")]
        public static void FactionCommandFaction(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            List<PlayerFaction> playerFactions =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList);

            if (!playerFactions.Any())
            {
                player.SendErrorNotification("You are not in any factions.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (PlayerFaction playerFaction in playerFactions)
            {
                Faction faction = Faction.FetchFaction(playerFaction.Id);

                if (faction == null) continue;

                menuItems.Add(new NativeMenuItem(faction.Name));
            }

            NativeMenu menu = new NativeMenu("faction:player:faction", "Factions", "Select a Faction", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void FactionCommandOnFactionSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            Faction selectedFaction = Faction.FetchFaction(option);

            if (selectedFaction == null)
            {
                player.SendErrorNotification("Unable to fetch this factions data.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Set Active", "Set the faction as your active faction.")
            };

            player.SetData("faction:player:factionSelected", selectedFaction.Id);

            NativeMenu menu = new NativeMenu("faction:player:subFactionMenu", selectedFaction.Name, "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void FactionCommandOnFactionSubSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            player.GetData("faction:player:factionSelected", out int factionId);

            Faction selectedFaction = Faction.FetchFaction(factionId);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (option == "Set Active")
            {
                playerCharacter.ActiveFaction = selectedFaction.Id;

                context.SaveChanges();

                player.SendInfoNotification($"You've set the {selectedFaction.Name} faction as your active faction.");
                return;
            }
        }

        [Command("r", onlyOne: true, commandType: CommandType.Faction, description: "Law/Fire: Used for internal communications")]
        public static void FactionCommandRadio(IPlayer player, string args = "")
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You don't have an active faction.");
                return;
            }

            if (!playerCharacter.FactionDuty)
            {
                player.SendErrorNotification("You aren't on duty!");
                return;
            }

            if (activeFaction.SubFactionType != SubFactionTypes.Law)
            {
                if (activeFaction.SubFactionType != SubFactionTypes.Medical)
                {
                    if (activeFaction.SubFactionType != SubFactionTypes.Government)
                    {
                        player.SendPermissionError();
                        return;
                    }
                }
            }

            if (args == "" || args.Length < 2)
            {
                player.SendSyntaxMessage("/(r)adio [Message]");
                return;
            }

            string playerName = player.GetClass().Name;

            PlayerFaction playerFaction = JsonConvert
                .DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList)
                .FirstOrDefault(x => x.Id == activeFaction.Id);

            if (playerFaction == null)
            {
                player.SendErrorNotification("Unable to fetch your faction data.");
                return;
            }

            Rank playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                .FirstOrDefault(x => x.Id == playerFaction.RankId);

            string rank = "";

            if (playerRank != null)
            {
                rank = playerRank.Name;
            }

            foreach (IPlayer target in Alt.Server.GetPlayers())
            {
                Models.Character targetCharacter = target.FetchCharacter();

                // Is spawned
                if (targetCharacter == null) continue;

                // Not in same faction
                if (targetCharacter.ActiveFaction != playerCharacter.ActiveFaction) continue;

                // Not on duty
                if (!targetCharacter.FactionDuty) continue;

                target.SendRadioMessage($"{rank} {playerName} says: {args}");
            }

            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Talk);

            sw.Stop();
        }

        [Command("dep", onlyOne: true, commandType: CommandType.Faction,
            description: "Used to communicate between departments")]
        public static void FactionCommandDepartmental(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/dep (Message)");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            bool allowed = player.IsLeo(true);

            if (!allowed)
            {
                allowed =
                    Faction.FetchFaction(playerCharacter.ActiveFaction).SubFactionType == SubFactionTypes.Medical ||
                    !playerCharacter.FactionDuty;
            }

            if (!allowed)
            {
                allowed =
                    Faction.FetchFaction(playerCharacter.ActiveFaction).SubFactionType == SubFactionTypes.Government ||
                    !playerCharacter.FactionDuty;
            }

            if (!allowed)
            {
                player.SendPermissionError();
                return;
            }

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("You don't have an active faction.");
                return;
            }

            PlayerFaction playerFaction = JsonConvert
                .DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList)
                .FirstOrDefault(x => x.Id == activeFaction.Id);

            Rank playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                .FirstOrDefault(x => x.Id == playerFaction.RankId);

            string rank = "";

            if (playerRank != null)
            {
                rank = playerRank.Name;
            }

            string[] factionNameSplit = activeFaction.Name.Split(" ");

            string factionName = "";

            foreach (var s in factionNameSplit)
            {
                factionName += s.First();
            }

            foreach (IPlayer target in Alt.Server.GetPlayers())
            {
                Models.Character targetCharacter = target.FetchCharacter();

                if (targetCharacter == null) continue;

                if (!targetCharacter.FactionDuty) continue;

                bool canReceive = target.IsLeo(true);

                if (!canReceive)
                {
                    canReceive = Faction.FetchFaction(targetCharacter.ActiveFaction).SubFactionType ==
                                 SubFactionTypes.Medical;
                }

                if (!canReceive)
                {
                    canReceive = Faction.FetchFaction(targetCharacter.ActiveFaction).SubFactionType ==
                                 SubFactionTypes.Government;
                }

                if (!canReceive) continue;

                target.SendRadioMessage($"[Departmental - {factionName}] {rank} {playerCharacter.Name} says: {args}");
            }

            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Talk);
            DiscordHandler.SendMessageToDepartmentalChannel($"[{factionName}] {rank} {playerCharacter.Name} says: {args}");
        }
    }
}