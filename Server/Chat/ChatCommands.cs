using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Models;

namespace Server.Chat
{
    public class ChatCommands
    {
        [Command("fontsize", onlyOne: true, commandType: CommandType.Chat, description: "Used to set your font style")]
        public static void CommandFontSize(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/fontsize [-3 to 3]");
                return;
            }

            if (args.Length < 1)
            {
                player.SendSyntaxMessage("/fontsize [-3 to 3]");
                return;
            }

            bool tryParse = int.TryParse(args, out int fontSize);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/fontsize [-3 to 3]");
                return;
            }

            if (fontSize < -3 || fontSize > 3)
            {
                player.SendSyntaxMessage("/fontsize [-3 to 3]");
                return;
            }
            player.Emit("Chat:ChangeFontSize", fontSize);

            player.SendInfoNotification($"You've changed your font size to {fontSize}");
        }

        [Command("ame", onlyOne: true, commandType: CommandType.Chat, description: "An above head emote command")]
        public static void CommandAMe(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/ame [Emote]");
                return;
            }

            if (args.Length < 1)
            {
                player.SendSyntaxMessage("/ame [Emote]");
                return;
            }

            player.SetSyncedMetaData("ChatCommand:Ame", $"{player.GetClass().Name} {args}");
            player.SetSyncedMetaData("ChatCommand:AmeActive", true);

            Timer newTimer = new Timer(5000) { AutoReset = false, Enabled = true };

            newTimer.Start();

            newTimer.Elapsed += (sender, eventArgs) =>
            {
                player.SetSyncedMetaData("ChatCommand:Ame", null);

                player.SetSyncedMetaData("ChatCommand:AmeActive", false);
                newTimer.Stop();
            };

            player.SendNotification($"~p~AME: {args}.");
        }

        [Command("me", onlyOne: true, commandType: CommandType.Chat, description: "An emote command")]
        public static void CommandMe(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/me [Emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Me);
        }

        [Command("melow", onlyOne: true, commandType: CommandType.Chat, description: "An emote command")]
        public static void CommandMeLow(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/melow [Emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.MeLow);
        }

        [Command("melong", onlyOne: true, commandType: CommandType.Chat, description: "An emote command")]
        public static void CommandMeHigh(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/melong [Emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.MeLong);
        }

        [Command("my", onlyOne: true, commandType: CommandType.Chat, description: "An emote command")]
        public static void CommandMy(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/my [Emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.My);
        }

        [Command("do", onlyOne: true, commandType: CommandType.Chat, description: "An emote command")]
        public static void CommandDo(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;
            if (args == "")
            {
                player.SendSyntaxMessage("/do [emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Do);
        }

        [Command("dolow", onlyOne: true, commandType: CommandType.Chat, description: "A Low emote command")]
        public static void CommandDoLow(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;
            if (args == "")
            {
                player.SendSyntaxMessage("/dolow [emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.DoLow);
        }

        [Command("dolong", onlyOne: true, commandType: CommandType.Chat, description: "A Low emote command")]
        public static void CommandDoLong(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;
            if (args == "")
            {
                player.SendSyntaxMessage("/dolong [emote]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.DoLong);
        }

        [Command("b", onlyOne: true, commandType: CommandType.Chat, description: "Local OOC Chat")]
        public static void CommandLocalOoc(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendInfoNotification("Usage: /b [Message]");
                return;
            }

            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Ooc);
        }

        [Command("s", onlyOne: true, commandType: CommandType.Chat, description: "Shout command")]
        public static void CommandShout(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/s [Message]");
                return;
            }

            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Shout, 20);
        }

        [Command("w", onlyOne: true, commandType: CommandType.Chat, description: "Whisper command")]
        public static void CommandWhisper(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;
            if (args == "")
            {
                player.SendSyntaxMessage("/w [Message]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Whisper, 5f);
        }

        [Command("low", onlyOne: true, commandType: CommandType.Chat, description: "Low Talking command")]
        public static void CommandLow(IPlayer player, string args = "")
        {
            if (player?.FetchCharacter() == null) return;
            if (args == "")
            {
                player.SendSyntaxMessage("/w [Message]");
                return;
            }
            ChatHandler.SendMessageToNearbyPlayers(player, args, MessageType.Low, 5f);
        }

        [Command("pm", onlyOne: true, commandType: CommandType.Chat, description: "Private message other players")]
        public static void CommandPrivateMessage(IPlayer player, string args = "")
        {
            try
            {
                if (player?.FetchCharacter() == null) return;
                if (args == "")
                {
                    player.SendSyntaxMessage("/pm [IdOrName] [Message]");
                    return;
                }

                string[] split = args.Split(' ');

                if (split.Length < 2)
                {
                    player.SendSyntaxMessage("/pm [IdOrName] [Message]");
                    return;
                }

                IPlayer targetPlayer = Utility.FindPlayerByNameOrId(split[0]);

                if (targetPlayer == null)
                {
                    player.SendErrorNotification("Player not found!");
                    return;
                }

                string message = string.Join(' ', split.Skip(1));

                player.SendPrivateMessage($"Sent to {targetPlayer.GetClass().Name} (Id: {targetPlayer.GetPlayerId()}): {message}");
                targetPlayer.SendPrivateMessage($" From {player.GetClass().Name} (Id: {player.GetPlayerId()}): {message}");
                targetPlayer.SetData("LastPmFrom", player.GetPlayerId());

                Logging.AddToCharacterLog(player, $"Has sent a PM to {targetPlayer.GetClass().Name}: {message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("re", onlyOne: true, commandType: CommandType.Chat, description: "Used to quickly reply to last PM")]
        public static void CommandReply(IPlayer player, string args = "")
        {
            try
            {
                if (player?.FetchCharacter() == null) return;

                if (args == "" || args.Length < 1)
                {
                    player.SendSyntaxMessage("/re [Message]");
                    return;
                }

                bool hasLastData = player.GetData("LastPmFrom", out int targetId);

                if (!hasLastData)
                {
                    player.SendErrorNotification("You haven't received a PM to use this!");
                    return;
                }

                IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == targetId);

                if (targetPlayer?.FetchCharacter() == null)
                {
                    player.SendErrorNotification("Unable to find the target player.");
                    return;
                }

                player.SendPrivateMessage($"Sent to {targetPlayer.GetClass().Name} (Id: {targetPlayer.GetPlayerId()}): {args}");
                targetPlayer.SendPrivateMessage($" From {player.GetClass().Name} (Id: {player.GetPlayerId()}): {args}");

                Logging.AddToCharacterLog(player, $"Has sent a PM to {targetPlayer.GetClass().Name}: {args}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        [Command("f", onlyOne: true, commandType: CommandType.Chat, description: "Faction Chat for active faction")]
        public static void CommandFactionChat(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/f [Message]");
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            List<IPlayer> targetPlayers = Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null).ToList();

            Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

            if (activeFaction == null)
            {
                player.SendErrorNotification("Unable to find your active faction.");
                return;
            }

            string[] factionNameSplit = activeFaction.Name.Split(" ");

            string factionName = "";

            foreach (var s in factionNameSplit)
            {
                factionName += s.First();
            }

            PlayerFaction playerFaction =
                JsonConvert.DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList).FirstOrDefault(i => i.Id == activeFaction.Id);

            string playerRank = JsonConvert.DeserializeObject<List<Rank>>(activeFaction.RanksJson)
                .FirstOrDefault(x => x.Id == playerFaction.RankId).Name;

            foreach (IPlayer targetPlayer in targetPlayers)
            {
                List<PlayerFaction> targetFactions =
                    JsonConvert.DeserializeObject<List<PlayerFaction>>(targetPlayer.FetchCharacter().FactionList);

                bool inFaction = targetFactions.FirstOrDefault(x => x.Id == playerFaction.Id) != null;

                if (!inFaction) continue;

                targetPlayer.SendFactionChatMessage(factionName, $"{playerRank} {player.GetClass().Name}: {args}");
            }
        }

        [Command("a", AdminLevel.Tester, true, commandType: CommandType.Admin, description: "Admin Chat")]
        public static void CommandAdminChat(IPlayer player, string message = "")
        {
            if (message == "" || message.Length < 2)
            {
                player.SendSyntaxMessage("/a [Message]");
                return;
            }

            string username = player.FetchAccount().Username;

            foreach (IPlayer admin in Alt.Server.GetPlayers())
            {
                if (!admin.IsSpawned()) continue;

                Models.Account adminAccount = admin.FetchAccount();

                if (adminAccount == null) continue;

                if (adminAccount.AdminLevel < AdminLevel.Tester && !adminAccount.Developer) continue;

                admin.SendAdminChatMessage($"{username} says: {message}");
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "In Game Message",
                Color = DiscordColor.IndianRed
            };

            embedBuilder.AddField("Message", message);
            embedBuilder.AddField("Admin", username);

            DiscordHandler.SendEmbedToIgAdminChannel(embedBuilder);
        }
    }
}