﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore;
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
        [Command("factions", commandType: CommandType.Faction, description: "View online numbers in a faction")]
        public static async void FactionCommandViewOnlineNumbers(IPlayer player)
        {
            if (!player.GetClass().Spawned)
            {
                player.SendLoginError();
                return;
            }

            player.SendInfoMessage("____[Online Faction Members]____");

            await using Context context = new Context();

            List<Faction> factions = await context.Faction.ToListAsync();

            foreach (Faction faction in factions)
            {
                int factionCount = 0;

                foreach (IPlayer client in Alt.GetAllPlayers())
                {
                    if (!client.IsSpawned()) continue;

                    Models.Character clientCharacter = client.FetchCharacter();

                    if (clientCharacter == null) continue;

                    List<PlayerFaction> clientFactions =
                        JsonConvert.DeserializeObject<List<PlayerFaction>>(clientCharacter.FactionList);

                    if (clientFactions.All(x => x.Id != faction.Id)) continue;

                    factionCount++;
                }

                if (factionCount == 0) continue;

                player.SendInfoMessage($"{faction.Name} - {factionCount} Members Online.");
            }
        }

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
                    Faction.FetchFaction(playerCharacter.ActiveFaction)?.SubFactionType == SubFactionTypes.Medical ||
                    !playerCharacter.FactionDuty;
            }

            if (!allowed)
            {
                allowed =
                    Faction.FetchFaction(playerCharacter.ActiveFaction)?.SubFactionType == SubFactionTypes.Government ||
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

            PlayerFaction? playerFaction = JsonConvert
                .DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList)
                .FirstOrDefault(x => x.Id == activeFaction.Id);

            Rank? playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
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

            foreach (IPlayer target in Alt.GetAllPlayers())
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

                target.SendDepartmentRadioMessage($"[Departmental - {factionName}] {playerCharacter.Name} says: {args}");
            }

            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Talk, excludePlayer: true);
            DiscordHandler.SendMessageToDepartmentalChannel($"[{factionName}] {rank} {playerCharacter.Name} says: {args}");
        }
    }
}