using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using Server.Account;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Discord
{
    public class DiscordCommands
    {
        [Command("account", alternatives: "discord")]
        public static void DiscordCommandDiscord(IPlayer player)
        {
            Models.Account playerAccount = player.FetchAccount();

            if (playerAccount == null)
            {
                player.SendLoginError();
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (string.IsNullOrEmpty(playerAccount.DiscordId))
            {
                menuItems.Add(new NativeMenuItem("~g~Link Discord"));
            }

            if (!playerAccount.Enable2FA)
            {
                menuItems.Add(new NativeMenuItem("~g~Enable 2FA"));
            }

            if (playerAccount.Enable2FA)
            {
                menuItems.Add(new NativeMenuItem("~r~Disable 2FA"));
            }

            if (!playerAccount.AutoLogin && playerAccount.Enable2FA && !string.IsNullOrEmpty(playerAccount.DiscordId))
            {
                menuItems.Add(new NativeMenuItem("~g~Enable Auto Login"));
            }

            if (playerAccount.AutoLogin)
            {
                menuItems.Add(new NativeMenuItem("~r~Disable Auto Login"));
            }

            NativeMenu menu = new NativeMenu("DiscordMenu:MainMenu", "Discord", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnDiscordMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option.Contains("Link Discord"))
            {
                player.Emit("DiscordLink:FetchUserId");
                return;
            }

            if (option.Contains("Enable 2FA"))
            {
                TwoFactorHandler.SetupTwoFactorForPlayer(player);
                return;
            }

            using Context context = new Context();

            Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

            if (playerAccount == null)
            {
                player.SendErrorNotification("There was an error fetching your account information.");
                return;
            }

            if (option.Contains("Disable 2FA"))
            {
                playerAccount.Enable2FA = false;

                NotificationExtension.SendInfoNotification(player, "Two Factor Authentication has been disabled.");
            }

            if (option.Contains("Enable Auto Login"))
            {
                playerAccount.AutoLogin = true;
                player.SendInfoNotification($"You've enabled Auto Login.");
            }

            if (option.Contains("Disable Auto Login"))
            {
                playerAccount.AutoLogin = false;
                player.SendInfoNotification($"You've disabled Auto Login.");
            }
            
            context.SaveChanges();

        }

        public static void OnReturnDiscordLinkUserId(IPlayer player, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                player.SendErrorNotification("There was an error fetching your discord Id.");
                return;
            }
            
            using Context context = new Context();

            Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

            if (playerAccount == null)
            {
                player.SendErrorNotification("There was an error fetching your account information.");
                return;
            }

            playerAccount.DiscordId = userId;
            context.SaveChanges();

            string authToken = Utility.GenerateRandomString(4);

            player.SetData("DiscordUserId", userId);
            player.SetData("DiscordAuthCode", authToken);

            DiscordHandler.SendMessageToUser(userId, $"A request has been made to link your game account to the UCP User {playerAccount.Username} at {DateTime.Now}. If this was not you, please contact an admin. Otherwise use /linkdiscord {authToken} in-game!");
        }

        [Command("linkdiscord", onlyOne: true)]
        public static void DiscordCommandLinkDiscord(IPlayer player, string args = "")
        {
            bool hasAuthCode = player.GetData("DiscordAuthCode", out string authToken);
            bool hasDiscordUserId = player.GetData("DiscordUserId", out string userId);
            bool hasFailCount = player.GetData("DiscordAuthFails", out int fails);

            if (!hasFailCount)
            {
                fails = 0;
            }

            if (!hasAuthCode && !hasDiscordUserId)
            {
                player.SendErrorNotification("You need to use /discord first!");
                return;
            }
            
            using Context context = new Context();
            
            Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

            if (playerAccount == null)
            {
                player.SendErrorNotification("There was an error fetching your account information.");
                return;
            }

            if (args != authToken)
            {
                if (fails >= 3)
                {
                    player.SendErrorNotification("Your auth tokens don't match. You have had three attempts.");
                    player.DeleteData("DiscordAuthCode");
                    player.DeleteData("DiscordUserId");
                    player.DeleteData("DiscordAuthFails");
                    return;
                }

                int newFailCount = fails++;

                player.SendErrorNotification($"Your auth tokens don't match. Attempts remaining {newFailCount}/3");
                player.SetData("DiscordAuthFails", fails);
                return;
            }

            playerAccount.DiscordId = userId;
            context.SaveChanges();

            player.SendInfoNotification($"You have linked your discord to your game account. Thanks!");
            DiscordHandler.SendMessageToUser(userId, $"Thank you! Your game account has been linked to your discord account!");
        }
    }
}