using System;
using AltV.Net.Elements.Entities;

namespace Server.Extensions
{
    public class Screenshots
    {
        public static void OnPlayerSendScreenshot(IPlayer player, string screenshot)
        {
            if (!player.IsSpawned()) return;

            Models.Account? playerAccount = player.FetchAccount();

            if (playerAccount == null)
            {
                player.SendErrorNotification("Unable to find your account.");
                return;
            }

            if (string.IsNullOrEmpty(playerAccount.DiscordId))
            {
                player.SendErrorNotification("Please link your Discord via the UCP.");
                return;
            }

            Console.WriteLine($"Screenshot received from {player.GetClass().Name}");

            SignalR.SendScreenShotToDiscord(playerAccount.DiscordId, screenshot);
        }
    }
}