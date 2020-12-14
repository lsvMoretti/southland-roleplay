using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Admin;
using Server.Character;
using Server.Chat;
using Server.Discord;
using Server.Extensions;
using Server.Jobs.Clerk;
using Server.Models;
using Server.Property;

namespace Server.Connection
{
    public class ConnectionHandler
    {
        public static void OnLoginViewLoaded(IPlayer player)
        {
            using Context context = new Context();

            foreach (Models.Account account in context.Account.ToList())
            {
                if (!account.AutoLogin) continue;

                if (player.Ip != account.LastIp) continue;
                if (player.SocialClubId.ToString() != account.LastSocial) continue;
                if (player.HardwareIdHash != Convert.ToUInt64(account.HardwareIdHash)) continue;
                if (player.HardwareIdExHash != Convert.ToUInt64(account.HardwareIdExHash)) continue;

                AutoLoginPlayer(player, account);
                return;
            }
        }

        public static void AltOnOnPlayerConnect(IPlayer player, string reason)
        {
            try
            {
                Console.WriteLine($"{player.Name} has connected.");
                player.Emit("showLogin");
                player.SendInfoMessage($"Welcome to Southland Roleplay. Upon logging in, you accept the server rules.");
                player.SendInfoMessage($"Version: v{Utility.Build} - Build: {Utility.LastUpdate}");
                player.SendInfoMessage("Please visit https://sol-rp.com for more information!");

                player.Spawn(new Position(0, 0, 72));

                player.FreezePlayer(true);

                Position startCamPos = new Position(400.465f, 1052.056f, 323.8427f);

                CameraExtension.InterpolateCamera(player, startCamPos, new Rotation(0, 0, -95), 90, startCamPos + new Position(0, 50, -50), new Rotation(0, 0, 180), 90, 20000);

                player.ChatInput(false);

                player.ShowHud(false);

                player.ShowCursor(true);

                player.Emit("LoadDLC");

                player.Emit("loadAllIpls");

                player.AllowMouseContextMenu(false);

                player.SetWeather((uint)TimeWeather.CurrentWeatherType);

                DateTime dateNow = DateTime.Now;

                player.SetDateTime(dateNow.Day, dateNow.Month, dateNow.Year, Settings.ServerSettings.Hour, Settings.ServerSettings.Minute, 0);

                player.GetClass().CharacterId = 0;
                player.GetClass().CompletedTutorial = true;

                player.GetClass().Spawned = false;
                if (!string.IsNullOrEmpty(Settings.ServerSettings.MOTD))
                {
                    player.SendMotdMessage();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                player.Emit("showLogin");
            }
        }

        private static void AutoLoginPlayer(IPlayer player, Models.Account playerAccount)
        {
            try
            {
                player.ChatInput(false);
                player.FreezePlayer(true);

                player.Emit("closeLogin");
                bool isBanned = Bans.IsAccountBanned(player);

                if (!isBanned && playerAccount.Banned)
                {
                    if (DateTime.Compare(DateTime.Now, playerAccount.UnBanTime) < 0)
                    {
                        isBanned = true;
                    }
                }

                if (isBanned)
                {
                    player.SendErrorNotification($"You are banned. Unban time: {playerAccount.UnBanTime}.");

                    DiscordHandler.SendMessageToLogChannel($"{playerAccount.Username} has tried logging in when banned.");

                    player.Kick("Banned.");

                    return;
                }

                if (playerAccount.IsOnline)
                {
                    player.Kick("Already Logged In!");
                    return;
                }

#if DEBUG
                if (playerAccount.AdminLevel < AdminLevel.Support)
                {
                    if (!playerAccount.Tester)
                    {
                        player.Kick("Server Not Ready");
                        return;
                    }
                }
#endif

                bool acceptedCharacters = Models.Character.FetchCharacters(playerAccount).Any(x => x.BioStatus == 2);

                if (!acceptedCharacters)
                {
                    player.SendErrorNotification("You don't have any accepted characters on your account.");
                    player.SendInfoNotification($"Visit the UCP: https://ucp.paradigmroleplay.com to create some characters!");
                    return;
                }

                player.GetClass().AccountId = playerAccount.Id;

                if (playerAccount.Enable2FA)
                {
                    if (string.IsNullOrEmpty(playerAccount.HardwareIdHash) ||
                        string.IsNullOrEmpty(playerAccount.HardwareIdExHash))
                    {
                        player.SendErrorNotification("Two Factor is setup. Please input the code below.");
                        player.Emit("2FA:GetInput");
                        player.FreezeCam(true);
                        player.FreezeInput(true);
                        return;
                    }

                    ulong hardwareId = ulong.Parse(playerAccount.HardwareIdHash);
                    ulong hardwareIdEx = ulong.Parse(playerAccount.HardwareIdExHash);
                    if (player.Ip != playerAccount.LastIp ||
                        playerAccount.LastSocial != player.SocialClubId.ToString() ||
                        player.HardwareIdHash != hardwareId ||
                        player.HardwareIdExHash != hardwareIdEx)
                    {
                        player.SendErrorNotification("Two Factor is setup. Please input the code below.");
                        player.Emit("2FA:GetInput");
                        player.FreezeCam(true);
                        player.FreezeInput(true);
                        return;
                    }
                }

                player.SendInfoNotification($"You've been auto logged in.");
                CompleteLogin(player);
            }
            catch (Exception e)
            {
                player.SendErrorNotification("An error occurred auto logging you in!");
                player.Emit("showLogin");
                Console.WriteLine(e);
                return;
            }
        }

        public static void ReceiveLoginRequest(IPlayer player, string user, string password)
        {
            try
            {
                player.ChatInput(false);

                player.Emit("closeLogin");

                using Context context = new Context();
                Models.Account playerAccount = context.Account.FirstOrDefault(x => x.Username.ToLower() == user.ToLower());

                if (playerAccount == null)
                {
                    playerAccount = context.Account.FirstOrDefault(x => x.Email.ToLower() == user.ToLower());

                    if (playerAccount == null)
                    {
                        player.Emit("showLogin");
                        player.SendErrorNotification("No account found by this user.");
                        return;
                    }
                }

                bool isBanned = Bans.IsAccountBanned(player);

                if (!isBanned && playerAccount.Banned)
                {
                    if (DateTime.Compare(DateTime.Now, playerAccount.UnBanTime) < 0)
                    {
                        isBanned = true;
                    }
                }

                if (isBanned)
                {
                    player.SendErrorNotification($"You are banned. Unban time: {playerAccount.UnBanTime}.");

                    DiscordHandler.SendMessageToLogChannel($"{playerAccount.Username} has tried logging in when banned.");

                    player.Kick("Banned.");

                    return;
                }

                if (playerAccount.IsOnline)
                {
                    player.Kick("Already Logged In!");
                    return;
                }

                if (!BCrypt.Net.BCrypt.Verify(password, playerAccount.Password))
                {
                    player.Emit("showLogin");
                    player.SendErrorNotification("Incorrect Password!");
                    return;
                }

#if DEBUG
                if (playerAccount.AdminLevel < AdminLevel.Support)
                {
                    if (!playerAccount.Tester)
                    {
                        player.Kick("Server Not Ready");
                        return;
                    }
                }
#endif

                bool acceptedCharacters = Models.Character.FetchCharacters(playerAccount).Any(x => x.BioStatus == 2);

                if (!acceptedCharacters)
                {
                    player.SendErrorNotification("You don't have any accepted characters on your account.");
                    player.SendInfoNotification($"Visit the UCP: https://ucp.paradigmroleplay.com to create some characters!");
                    return;
                }

                player.SetAccountId(playerAccount.Id);
                player.GetClass().AccountId = playerAccount.Id;

                if (playerAccount.Enable2FA)
                {
                    if (string.IsNullOrEmpty(playerAccount.HardwareIdHash) ||
                        string.IsNullOrEmpty(playerAccount.HardwareIdExHash))
                    {
                        player.SendErrorNotification("Two Factor is setup. Please input the code below.");
                        player.Emit("2FA:GetInput");
                        player.FreezeCam(true);
                        player.FreezeInput(true);
                        return;
                    }

                    ulong hardwareId = ulong.Parse(playerAccount.HardwareIdHash);
                    ulong hardwareIdEx = ulong.Parse(playerAccount.HardwareIdExHash);
                    if (player.Ip != playerAccount.LastIp ||
                        playerAccount.LastSocial != player.SocialClubId.ToString() ||
                        player.HardwareIdHash != hardwareId ||
                        player.HardwareIdExHash != hardwareIdEx)
                    {
                        player.SendErrorNotification("Two Factor is setup. Please input the code below.");
                        player.Emit("2FA:GetInput");
                        player.FreezeCam(true);
                        player.FreezeInput(true);
                        return;
                    }
                }

                CompleteLogin(player);
            }
            catch (Exception e)
            {
                player.SendErrorNotification("An error occurred logging you in!");
                Console.WriteLine(e);
                return;
            }
        }

        public static void CompleteLogin(IPlayer player)
        {
            try
            {
                List<string> characterNames = new List<string>();
                using Context context = new Context();

                Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

                if (playerAccount == null)
                {
                    NotificationExtension.SendErrorNotification(player, "An error occurred fetching your account.");
                    player.Kick("Failed to fetch account.");
                    return;
                }

                player.SendInfoNotification($"Welcome back {playerAccount.Username}.");

                characterNames = Models.Character.FetchCharacterNames(playerAccount);

                if (!characterNames.Any())
                {
                    player.SendErrorNotification("An error occurred fetching your character names.");
                    player.Kick("Error Fetching Character Names");
                    return;
                }

                playerAccount.LastIp = player.Ip;
                playerAccount.LastSocial = player.SocialClubId.ToString();
                playerAccount.HardwareIdHash = player.HardwareIdHash.ToString();
                playerAccount.HardwareIdExHash = player.HardwareIdExHash.ToString();
                playerAccount.IsOnline = true;

                context.SaveChanges();

                player.GetClass().UcpName = playerAccount.Username;

                SignalR.SendUserLogin(playerAccount);

                player.ChatInput(true);
                player.ShowCursor(false);

                if (string.IsNullOrEmpty(playerAccount.DiscordId))
                {
                    player.SendNotification("~y~You have not yet linked your Discord and Game Account together!", true);
                }

                foreach (KeyValuePair<int, DeadBody> keyValuePair in DeathHandler.DeadBodies)
                {
                    DeadBodyHandler.LoadBodyForPlayer(player, keyValuePair.Value);
                }

                CreatorRoom.SendToCreatorRoom(player);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void OnPlayerDisconnect(IPlayer player, string reason)
        {
            try
            {
                if (player.GetClass().AccountId != 0)
                {
                    using Context context = new Context();

                    Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

                    playerAccount.IsOnline = false;

                    SignalR.SendUserLogout(playerAccount);

                    context.SaveChanges();
                }

                Console.WriteLine($"{player.Name} ({player.FetchCharacter()?.Name}) has disconnected. Reason: {reason}");

                int playerId = player.GetPlayerId();

                if (playerId == 0) return;

                CreatorRoom.PlayerIds.Remove(playerId);

                if (player.GetClass().ClerkJob > 0)
                {
                    ClerkHandler.StopClerkJob(player);
                }

                AdminReport adminReport = AdminHandler.AdminReports.FirstOrDefault(x => x.Player == player);

                if (adminReport != null)
                {
                    AdminHandler.AdminReports.Remove(adminReport);

                    AdminReportObject reportObject =
                        AdminHandler.AdminReportObjects.FirstOrDefault(x =>
                            x.CharacterId == player.GetClass().CharacterId);

                    if (reportObject != null)
                    {
                        SignalR.RemoveReport(reportObject);
                        AdminHandler.AdminReportObjects.Remove(reportObject);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}