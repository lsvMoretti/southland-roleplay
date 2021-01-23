using AltV.Net;
using AltV.Net.Elements.Entities;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Discord
{
    public class DiscordHandler
    {
        private static readonly ulong DiscordLogChannelId = 795062350398881832;
        private static readonly ulong DiscordIgAdminChatChannelId = 787791455950471218;
        private static readonly ulong DiscordIgReportsId = 795086774149447721;
        private static readonly ulong DiscordDepartmentId = 685633206246178860;

        /// <summary>
        /// Sends a message to the Discord Log Channel on the Main Guild
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessageToLogChannel(string message)
        {
            SignalR.SendDiscordMessage(DiscordLogChannelId, message);
        }

        public static void SendMessageToIgAdminChannel(string message)
        {
            SignalR.SendDiscordMessage(DiscordIgAdminChatChannelId, message);
        }

        public static void SendEmbedToIgAdminChannel(DiscordEmbed embed)
        {
            SignalR.SendDiscordEmbed(DiscordIgAdminChatChannelId, embed);
        }

        public static void SendMessageToReportsChannel(string message)
        {
            SignalR.SendDiscordMessage(DiscordIgReportsId, message);
        }

        public static void SendEmbedToReportsChannel(DiscordEmbed embed)
        {
            SignalR.SendDiscordEmbed(DiscordIgReportsId, embed);
        }

        public static void SendMessageToDepartmentalChannel(string message)
        {
            //SignalR.SendDiscordMessage(DiscordDepartmentId, message);
        }

        public static void SendMessageToUser(string userId, string message)
        {
            SignalR.SendDiscordUserMessage(userId, message);
        }

        public static void OnReceiveAdminMessage(string username, string message)
        {
            foreach (IPlayer admin in Alt.Server.GetPlayers())
            {
                if (!admin.IsSpawned()) continue;

                Models.Account adminAccount = admin.FetchAccount();

                if (adminAccount == null) continue;

                if (adminAccount.AdminLevel < AdminLevel.Tester && !adminAccount.Developer) continue;

                admin.SendDiscordAdminChatMessage($"{username} says: {message}");
            }
        }
    }
}