using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AltV.Net.Elements.Entities;

namespace Server.Extensions
{
    public class Screenshots
    {
        public static void OnPlayerSendScreenshot(IPlayer player, string screenshot)
        {
            try
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

                string filePath = $"{Directory.GetCurrentDirectory()}/screens/{playerAccount.Username}-{DateTime.Now.ToString("dd-MM-yy-hh-mm-ss")}.jpg";

                File.WriteAllBytes(filePath, Convert.FromBase64String(screenshot));

                SignalR.SendScreenShotToDiscord(playerAccount.DiscordId, filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}