using System;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Extensions;

namespace Server.Chat
{
    public static class PlayerChatExtension
    {
        public static void SendEmoteMessage(this IPlayer player, string message)
        {
            ChatHandler.SendMessageToNearbyPlayers(player, message, MessageType.Me, ChatHandler.EmoteRange);
        }

        /// <summary>
        /// Sends a [INFO] message to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendInfoNotification(this IPlayer player, string message)
        {
            NotificationExtension.SendInfoNotification(player, message);
        }

        public static void SendInfoMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{FDFE8B}" + "[INFO]{ffffff} " + message);
        }

        public static void SendPayMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{FDFE8B}" + "[PAY]{ffffff} " + message);
        }

        public static void SendSyntaxMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorInfo}[Syntax] " + "{ffffff}" + message);
        }

        /// <summary>
        /// Sends a Chat Message (Unformatted to a player)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendChatMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, message);
        }

        public static void SendAdvertMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{33cc33}" + "[ADVERT]{ffffff} " + message);
        }

        public static void SendMegaphoneMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{ccff66}" + "[MEGAPHONE]{ffffff} " + message);
        }

        /// <summary>
        /// Sends a Weapon Message (Unformatted to a player)
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendWeaponMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{3399ff}" + "[WEAPON] {ffffff}" + message);
        }

        public static void SendMotdMessage(this IPlayer player)
        {
            player.Emit("chatmessage", null, "{3399ff}" + "[Message of the day] {ffffff}" + Settings.ServerSettings.MOTD);
        }

        public static void SendCharityMessage(this IPlayer player, double amount)
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById("UTC");

            DateTimeOffset localServerTime = DateTimeOffset.Now;

            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(localServerTime, info);

            player.Emit("chatmessage", null, "{FDFE8B}" + $"You have sent {amount:C} to charity at {localTime}.");
        }

        /// <summary>
        /// Sends a [ERROR] message to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendErrorNotification(this IPlayer player, string message)
        {
            NotificationExtension.SendErrorNotification(player, message);
        }

        public static void SendErrorMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{A80707}" + "[ERROR] {ffffff}" + message);
        }

        /// <summary>
        /// Sends a [ERROR] message that they're not logged in
        /// </summary>
        /// <param name="player"></param>
        public static void SendLoginError(this IPlayer player)
        {
            //player.Emit("chatmessage", null, "{A80707}" + "[ERROR] {ffffff}You are not logged in!");
            NotificationExtension.SendErrorNotification(player, "You are not logged in!");
        }

        /// <summary>
        /// Sends a [ERROR] You don't have permission message
        /// </summary>
        /// <param name="player"></param>
        public static void SendPermissionError(this IPlayer player)
        {
            //player.Emit("chatmessage", null, "{A80707}" + "[ERROR] {ffffff}You don't have permission.");
            NotificationExtension.SendInfoNotification(player, "You don't have permission.");
        }

        /// <summary>
        /// Sends a [PM] Message to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public static void SendPrivateMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, "{FFCC00}" + $"(( [PM] {message} ))");
        }

        public static void SendPhoneMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorChatNear}[Phone] {ChatHandler.ColorWhite}{message}");
        }

        public static void SendSmsMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorInfo}[SMS] {ChatHandler.ColorWhite}{message}");
            //player.SendNotification($"~y~[SMS]~w~ {message}");
        }

        public static void SendFactionMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorAdminNews}(( [Faction] {message} ))");
        }

        public static void SendFactionChatMessage(this IPlayer player, string factionName, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorFactionMessage}(( [FCHAT] [{factionName}] {message} ))");
        }

        public static void SendAdminMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorAdminInfo} [ADMIN] {ChatHandler.ColorWhite}{message} ");
        }

        public static void SendHelperMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorAdminInfo} [HELPER] {ChatHandler.ColorWhite}{message} ");
        }

        public static void SendAdminBroadcastMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorAdminNews}(( [BROADCAST] {message} ))");
        }

        public static void SendAdminChatMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorAdminChat}(( [ACHAT] {message} ))");
        }

        public static void SendDiscordAdminChatMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorAdminChat}(( [DISCORD ACHAT] {message} ))");
        }

        public static void SendStatsMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorStatMessage}{message}");
        }

        public static void SendRadioMessage(this IPlayer player, int slot, int channel, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorRadioMessage}[S:{slot} #{channel}] {message}");
        }

        public static void SendDepartmentRadioMessage(this IPlayer player, string message)
        {
            player.Emit("chatmessage", null, $"{ChatHandler.ColorRadioMessage}[DEP] {message}");
        }
    }
}