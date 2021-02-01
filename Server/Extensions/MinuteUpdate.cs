using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Character;
using Server.Chat;
using Server.Discord;
using Server.Groups.Police;
using Server.Models;

namespace Server.Extensions
{
    public class MinuteUpdate
    {
        private static Timer _minuteTimer = null;
        private static Timer _tenSecondTimer = null;

        private static Timer _hourTimer = null;

        public static Dictionary<IPlayer, int> HighPingCount = new Dictionary<IPlayer, int>();

        private static long _memUsage = 3000000000;

        public static void StartMinuteTimer()
        {
            _minuteTimer = new Timer { Interval = 60000 };
            _minuteTimer.Elapsed += MinuteTimerOnElapsed;
            _minuteTimer.AutoReset = true;
            _minuteTimer.Start();

            _tenSecondTimer = new Timer(1000) { AutoReset = true };
            _tenSecondTimer.Start();
            _tenSecondTimer.Elapsed += tenSecondTimer_Elapsed;

            _hourTimer = new Timer(3600000) { AutoReset = true };
            _hourTimer.Elapsed += _hourTimer_Elapsed;
            _hourTimer.Start();
        }

        private static void _hourTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _hourTimer.Stop();
            List<Models.Property> mortgageProperties = Models.Property.FetchProperties().Where(x => x.MortgageValue > 0).ToList();

            if (!mortgageProperties.Any())
            {
                _hourTimer.Start();
                return;
            }

            DateTime timeNow = DateTime.Now;

            using Context context = new Context();

            foreach (Models.Property mortgageProperty in mortgageProperties)
            {
                if (DateTime.Compare(timeNow, mortgageProperty.LastMortgagePayment.AddMonths(2)) > 0)
                {
                    // Gone past 2 month payment window
                    Models.Property property = context.Property.Find(mortgageProperty.Id);

                    if (property == null) continue;

                    property.Key = Utility.GenerateRandomString(8);

                    double depositAmount = property.Value * 0.2;

                    Models.Character ownerCharacter = context.Character.Find(property.OwnerId);

                    property.OwnerId = 0;

                    if (ownerCharacter == null)
                    {
                        context.SaveChanges();
                        continue;
                    }

                    ownerCharacter.Money += (float)depositAmount;

                    property.MortgageValue = 0;

                    Logging.AddToCharacterLog(ownerCharacter.Id, $"{property.Address} has been taken away from them. Reason: Failure to keep up with mortgage payments! Refunded: {depositAmount:C}.");
                }
            }

            context.SaveChanges();
            _hourTimer.Start();
        }

        private static void tenSecondTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _tenSecondTimer.Stop();

            SignalR.SendPlayerCount();

            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

            if (currentProcess.WorkingSet64 > _memUsage)
            {
                DiscordHandler.SendMessageToLogChannel($"Current Ram Usage: {(currentProcess.WorkingSet64 / 1000000000):D2} Gigabytes");
                _memUsage = currentProcess.WorkingSet64;
            }

            /*foreach (IPlayer player in Alt.GetAllPlayers())
            {
                if(!player.IsSpawned()) continue;

                if(player.Ping > )
            }*/

            foreach (var player in Alt.GetAllPlayers())
            {
                Models.Character playerCharacter = player.FetchCharacter();

                if (playerCharacter == null) continue;

                player.SetData("MoneyValue", playerCharacter.Money);
            }

            _tenSecondTimer.Start();
        }

        private static void MinuteTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _minuteTimer.Stop();

#if RELEASE
            CharacterHandler.CheckPlayerAfk();

#endif

            using Context context = new Context();

            foreach (IPlayer player in Alt.GetAllPlayers())
            {
                lock (player)
                {
                    if (player.FetchCharacter() == null) continue;

                    if (player.GetClass().CreatorRoom) continue;

                    // Payday

                    Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                    if (playerCharacter == null) continue;

                    if (playerCharacter.InJail && playerCharacter.JailMinutes <= 0)
                    {
                        // In jail and finished time;
                        player.Position = PoliceHandler.UnJailPosition;
                        player.Dimension = 0;
                        playerCharacter.InJail = false;
                        playerCharacter.JailMinutes = 0;

                        PlayerChatExtension.SendInfoNotification(player, $"You have been released from jail.");
                    }
                    else if (playerCharacter.InJail && playerCharacter.JailMinutes > 0)
                    {
                        playerCharacter.JailMinutes -= 1;
                        PlayerChatExtension.SendInfoNotification(player, $"You have {playerCharacter.JailMinutes} minutes remaining in jail.");
                    }

                    Models.Account playerAccount = context.Account.Find(player.GetClass().AccountId);

                    if (playerAccount.InJail && playerAccount.JailMinutes <= 0)
                    {
                        // In jail and finished time;
                        player.Position = PoliceHandler.UnJailPosition;
                        player.Dimension = 0;
                        playerAccount.InJail = false;
                        playerAccount.JailMinutes = 0;
                        player.SendAdminMessage($"You have been released from jail.");
                    }
                    else if (playerAccount.InJail && playerAccount.JailMinutes > 0)
                    {
                        playerAccount.JailMinutes -= 1;

                        PlayerChatExtension.SendInfoNotification(player, $"You have {playerAccount.JailMinutes} minutes remaining in admin jail.");

                        continue;
                    }

                    playerCharacter.LastTimeCheck = DateTime.Now;
                    playerCharacter.PosX = player.Position.X;
                    playerCharacter.PosY = player.Position.Y;
                    playerCharacter.PosZ = player.Position.Z;
                    playerCharacter.RotZ = player.Rotation.Yaw;

                    bool hasLastTime = player.GetData("AFK:LastMove", out DateTime lastMove);

                    if (hasLastTime)
                    {
                        if (DateTime.Compare(DateTime.Now, lastMove.AddMinutes(5)) > 0 && playerAccount.AdminLevel < AdminLevel.Director)
                        {
                            Console.WriteLine($"[{DateTime.Now}] - {player.GetClass().Name} hasn't done anything since {lastMove}.");
                            Alt.Server.LogInfo($"[{DateTime.Now}] - {player.GetClass().Name} hasn't done anything since {lastMove}.");
                            continue;
                        }
                    }

                    if (playerCharacter.TotalMinutes >= 59)
                    {
                        playerCharacter.TotalMinutes = 0;
                        playerCharacter.TotalHours += 1;
                        /*if (playerCharacter.TotalHours % 50 == 0)
                    {
                        playerCharacter.Money += 20000;
                        player.SendInfoMessage($"You've hit another fifty hours! Here is {20000:C}!");
                    }*/

                        Payday.ProcessPlayerPayday(player);
                        continue;
                    }

                    playerCharacter.TotalMinutes += 1;
                }
            }
            //DiscordBot.UpdateDiscordPlayerCount(Alt.GetAllPlayers().Count(x => x.FetchCharacter() != null));
            context.SaveChanges();
            _minuteTimer.Start();

            /*
            foreach (Models.Character character in context.Character.Where(x => x.InJail))
            {
                if (!Models.Account.FindAccountById(character.OwnerId).IsOnline) continue;

                if (character.JailMinutes > 120)
                {
                    character.JailMinutes -= 1;
                }
            }
            */
            context.SaveChanges();
        }
    }
}