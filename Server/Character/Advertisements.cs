using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Models;

namespace Server.Character
{
    public class Advertisements
    {
        /// <summary>
        /// List of Character Id's and Advert text
        /// </summary>
        public static Dictionary<int, string> AdvertList = new Dictionary<int, string>();

        public static Position AdvertPosition = new Position(-317.2569f, -609.8212f, 33.5582f);

        private static double _adPrice = 10;

        public static void LoadAdvertisementBuilding()
        {
            TextLabel textLabel = new TextLabel($"Advertisement Center\nUse /ad\n{_adPrice:C}", AdvertPosition, TextFont.FontChaletComprimeCologne, new LsvColor(42, 140, 15));
            textLabel.Add();
        }

        public static bool AddAdvert(IPlayer player, string message)
        {
            try
            {
                if (AdvertList.ContainsKey(player.GetClass().CharacterId)) return false;

                AdvertList.Add(player.GetClass().CharacterId, message);

                List<IPlayer> onlineAdmins =
                    Alt.GetAllPlayers().Where(x => x.FetchAccount()?.AdminLevel >= AdminLevel.Tester).ToList();

                if (!onlineAdmins.Any())
                {
                    player.SendInfoNotification($"Your advert has been approved!");
                    PublishAdvert(player.GetClass().CharacterId);
                    DiscordHandler.SendMessageToLogChannel($"Character {player.GetClass().Name} (Character Id: {player.GetClass().CharacterId}) (Player Id: {player.GetPlayerId()}) has auto posted the following advert due to no admins in-game.\n{message}");
                    return true;
                }

                foreach (IPlayer onlineAdmin in onlineAdmins)
                {
                    onlineAdmin.SendAdminMessage($"Advert: [{message}] from {player.GetClass().Name}, /acceptad {player.GetClass().CharacterId}.");
                }

                player.SendInfoNotification($"Your advert has been sent for approval.");

                player.SetData("Advert:LastAd", DateTime.Now);

                return true;
            }
            catch (Exception e)
            {
                player.SendErrorNotification("An error occurred.");
                Console.WriteLine(e);
                return false;
            }
        }

        public static void PublishAdvert(int characterId)
        {
            if (!AdvertList.ContainsKey(characterId)) return;

            string message = AdvertList.FirstOrDefault(x => x.Key == characterId).Value;

            AdvertList.Remove(characterId);

            foreach (IPlayer target in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null).ToList())
            {
                target.SendAdvertMessage(message);
            }

            SignalR.SendDiscordMessage(795084275090587748, message);
        }

        public static void DenyAdvert(int characterId)
        {
            if (!AdvertList.ContainsKey(characterId)) return;

            AdvertList.Remove(characterId);
        }

        [Command("ad", onlyOne: true, commandType: CommandType.Character,
            description: "Advertisement: Used to advertise")]
        public static void CommandAdvert(IPlayer player, string message = "")
        {
            if (message == "")
            {
                player.SendSyntaxMessage("/advert [Advert Message]");
                return;
            }

            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (player.GetClass().Cash < _adPrice)
            {
                player.SendErrorNotification($"You don't have enough money for this. {_adPrice:C}.");
                return;
            }

            bool hasLastAdData = player.GetData("Advert:LastAd", out DateTime lastAd);

            if (hasLastAdData)
            {
                DateTime now = DateTime.Now;
                double waitTimeSeconds = 120;

                using Context context = new Context();

                List<Donations> donations = context.Donations.Where(x =>
                    x.AccountId == player.GetClass().AccountId && x.Activated == true).ToList();

                bool isSilverDonator = false;
                bool isGoldDonator = false;

                foreach (Donations donation in donations)
                {
                    if (donation.Type == DonationType.Silver)
                    {
                        isSilverDonator = true;
                    }

                    if (donation.Type == DonationType.Gold)
                    {
                        isGoldDonator = true;
                    }
                }

                if (isSilverDonator)
                {
                    waitTimeSeconds = 60;
                }

                if (isGoldDonator)
                {
                    waitTimeSeconds = 30;
                }

                if (DateTime.Compare(lastAd.AddSeconds(waitTimeSeconds), now) < 0)
                {
                    player.SendErrorNotification($"You must wait at least {waitTimeSeconds} seconds between each advert!");
                    return;
                }
            }

            if (message.Length < 3)
            {
                player.SendErrorNotification("You need to input a longer advert.");
                return;
            }
            /*
                        if (player.Position.Distance(AdvertPosition) > 5)
                        {
                            player.SendErrorNotification("You're not in range of the advertisement building.");
                            return;
                        }
            */
            player.RemoveCash(_adPrice);

            AddAdvert(player, message);
        }
    }
}