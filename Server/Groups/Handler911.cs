using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Groups.Police.MDT;
using Server.Models;
using MessageType = Server.Chat.MessageType;

namespace Server.Groups
{
    public class Handler911
    {
        #region 911

        public static void Start911Call(IPlayer player, Phones callerPhone)
        {
            player.SetData("911:phoneUsed", callerPhone.PhoneNumber);
            player.FetchLocation("911:startCall");
        }

        public static void On911LocationReturn(IPlayer player, string streetName, string areaName)
        {
            player.Emit("phone:stopPhoneRinging");
            player.SendPhoneMessage("911, what is your emergency?");
            player.SetData("911:onCall", 1);
            player.SetData("911:streetName", streetName);
            player.SetData("911:areaName", areaName);
        }

        public static void On911CallMessage(IPlayer player, int stage, string message)
        {
            player.GetData("911:streetName", out string streetName);
            player.GetData("911:areaName", out string areaName);
            player.GetData("911:phoneUsed", out string phoneNumber);

            if (stage == 1)
            {
                Call911 newCall911 = new Call911(player.GetClass().Name, phoneNumber, message, $"{streetName} - {areaName}");
                MdtHandler.CallList.Add(newCall911);

                player.SetData("911:onCall", 0);

                ChatHandler.SendMessageToNearbyPlayers(player, message, MessageType.LocalPhone);
                player.SendPhoneMessage($"Operator Says: Thanks for calling. The emergency services have been alerted!");

                player.Emit("phone:stopPhoneRinging");
                player.SendPhoneMessage("You've hung up.");
                player.SendEmoteMessage("puts their phone away.");
                player.SetData("PHONEANSWERED", 0);
                player.SetData("ONPHONEWITH", 0);
                player.SetData("PHONERINGING", false);

                IEnumerable<IPlayer> players = Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null);

                foreach (IPlayer targetPlayer in players)
                {
                    if (!targetPlayer.FetchCharacter().FactionDuty) continue;

                    List<PlayerFaction> playerFactions =
                        JsonConvert.DeserializeObject<List<PlayerFaction>>(targetPlayer.FetchCharacter().FactionList);

                    bool emergencyFaction = false;

                    foreach (PlayerFaction playerFaction in playerFactions)
                    {
                        Faction faction = Faction.FetchFaction(playerFaction.Id);

                        if (faction == null) continue;

                        if (faction.SubFactionType == SubFactionTypes.Law)
                        {
                            emergencyFaction = true;
                        }
                        else if (faction.SubFactionType == SubFactionTypes.Medical)
                        {
                            emergencyFaction = true;
                        }
                    }

                    if (!emergencyFaction) continue;

                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}________[911]________{ChatHandler.ColorWhite}");
                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}Caller Number: {ChatHandler.ColorWhite}{newCall911.Number}.");
                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}Caller Location: {ChatHandler.ColorWhite}{newCall911.Location}.");
                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}Situation: {ChatHandler.ColorWhite}{newCall911.CallInformation}.");
                }

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Description = newCall911.CallInformation,
                    Timestamp = DateTime.Now,
                    Title = "Incoming 911",
                };

                embedBuilder.AddField("Number", newCall911.Number);
                embedBuilder.AddField("Location", newCall911.Location);

                SignalR.SendDiscordEmbed(798258593672462436, embedBuilder);
            }
        }

        #endregion 911

        #region 311

        public static void Start311Call(IPlayer player, Phones callerPhone)
        {
            player.SetData("311:phoneUsed", callerPhone.PhoneNumber);
            player.FetchLocation("311:startCall");
        }

        public static void On311LocationReturn(IPlayer player, string streetName, string areaName)
        {
            player.Emit("phone:stopPhoneRinging");
            player.SendPhoneMessage("311, How can I help you today?");
            player.SetData("311:onCall", 1);
            player.SetData("311:streetName", streetName);
            player.SetData("311:areaName", areaName);
        }

        public static void On311CallMessage(IPlayer player, int stage, string message)
        {
            player.GetData("311:streetName", out string streetName);
            player.GetData("311:areaName", out string areaName);
            player.GetData("311:phoneUsed", out string phoneNumber);

            if (stage == 1)
            {
                Call911 newCall911 = new Call911(player.GetClass().Name, phoneNumber, message, $"{streetName} - {areaName}");

                player.SetData("311:onCall", 0);

                ChatHandler.SendMessageToNearbyPlayers(player, message, MessageType.LocalPhone);
                player.SendPhoneMessage($"Operator Says: Thanks for calling. Your message has been sent to the relevant authority.");

                player.Emit("phone:stopPhoneRinging");
                player.SendPhoneMessage("You've hung up.");
                player.SendEmoteMessage("puts their phone away.");
                player.SetData("PHONEANSWERED", 0);
                player.SetData("ONPHONEWITH", 0);
                player.SetData("PHONERINGING", false);

                IEnumerable<IPlayer> players = Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null);

                foreach (IPlayer targetPlayer in players)
                {
                    List<PlayerFaction> playerFactions =
                        JsonConvert.DeserializeObject<List<PlayerFaction>>(targetPlayer.FetchCharacter().FactionList);

                    bool callAccess = false;

                    foreach (PlayerFaction playerFaction in playerFactions)
                    {
                        Faction faction = Faction.FetchFaction(playerFaction.Id);

                        if (faction == null) continue;

                        if (faction.SubFactionType == SubFactionTypes.Law)
                        {
                            if (!targetPlayer.FetchCharacter().FactionDuty) continue;
                            callAccess = true;
                        }
                        else if (faction.SubFactionType == SubFactionTypes.Medical)
                        {
                            if (!targetPlayer.FetchCharacter().FactionDuty) continue;
                            callAccess = true;
                        }
                        else if (faction.SubFactionType == SubFactionTypes.Government)
                        {
                            callAccess = true;
                        }
                    }

                    if (!callAccess) continue;

                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}________[311]________{ChatHandler.ColorWhite}");
                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}Caller Number: {ChatHandler.ColorWhite}{newCall911.Number}.");
                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}Caller Location: {ChatHandler.ColorWhite}{newCall911.Location}.");
                    targetPlayer.SendChatMessage($"{ChatHandler.ColorRadioMessage}Situation: {ChatHandler.ColorWhite}{newCall911.CallInformation}.");
                }

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Description = newCall911.CallInformation,
                    Timestamp = DateTime.Now,
                    Title = "Incoming 311",
                };

                embedBuilder.AddField("Number", newCall911.Number);
                embedBuilder.AddField("Location", newCall911.Location);
                SignalR.SendDiscordEmbed(798258593672462436, embedBuilder);
            }
        }

        #endregion 311
    }
}