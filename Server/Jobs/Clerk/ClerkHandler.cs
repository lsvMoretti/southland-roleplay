using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Discord;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Models;

namespace Server.Jobs.Clerk
{
    public class ClerkHandler
    {
        public static List<int> OccupiedJobs = new List<int>();

        private static Dictionary<int, TextLabel> _textLabels = new Dictionary<int, TextLabel>();

        public static void InitClerkJob()
        {
            Console.WriteLine($"Loading Clerk Jobs.");

            List<Models.Clerk> clerks = Models.Clerk.FetchClerks();

            foreach (Models.Clerk clerk in clerks)
            {
                LoadClerkJob(clerk);
            }

            Console.WriteLine($"Loaded {clerks.Count} Clerk Jobs");
        }

        public static void LoadClerkJob(Models.Clerk clerk)
        {
            Position clerkPosition = Models.Clerk.FetchPosition(clerk);

            TextLabel label = new TextLabel("Clerk Job\n/startclerk to start!", clerkPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkGreen));

            label.Add();

            _textLabels.Add(clerk.Id, label);

            if (clerk.PropertyId > 0)
            {
                using Context context = new Context();

                Models.Property property = context.Property.Find(clerk.PropertyId);

                if (property != null && property.ClerkActive)
                {
                    property.ClerkActive = false;
                    context.SaveChanges();
                }

                
            }
        }

        public static void OnFiveMinuteInterval(IPlayer player)
        {
            Random rnd = new Random();

            int counter = player.GetClass().ClerkCount;

            int clerkStoreId = player.GetClass().ClerkJob;

            Models.Clerk clerk = Models.Clerk.FetchClerk(clerkStoreId);

            bool counterEven = counter % 2 == 0;

            if (counter == 0)
            {
                // First Five minutes
                PublishClerkAdvert(clerk);
                player.GetClass().ClerkCount = counter + 1;
                return;
            }

            List<Position> clerkPositions = Models.Clerk.FetchPositions(clerk);

            int positionId = rnd.Next(0, clerkPositions.Count - 1);

            Position gotoPosition = clerkPositions[positionId];

            if (!counterEven)
            {
                if (counter == 1)
                {
                    // Counter 1
                    using Context context = new Context();

                    Models.Property property = context.Property.Find(clerk.PropertyId);

                    if (property != null)
                    {
                        property.ClerkActive = true;
                        context.SaveChanges();
                        player.SendInfoNotification($"A 10% discount has been given to players.");
                    }

                    
                }
                player.Emit("Clerk:SetGotoPosition", gotoPosition);
                player.GetClass().HasClerkTask = true;
                player.GetClass().ClerkCount = counter + 1;
                player.SendInfoNotification($"You've been given a new task. Head there and RP.");
                return;
            }

            if (player.GetClass().HasClerkTask)
            {
                // Been five minutes and still not done
                player.SendInfoNotification($"You've failed to complete the task.");
                StopClerkJob(player);
                return;
            }

            player.GetClass().ClerkCount = counter + 1;
            // Not second time and is every other time
            PublishClerkAdvert(clerk);
            return;
        }

        public static void StopClerkJob(IPlayer player)
        {
            player.Emit("Clerk:StopJob");
            player.SendInfoNotification($"You've stopped the clerk job.");

            Models.Clerk clerk = Models.Clerk.FetchClerk(player.GetClass().ClerkJob);

            if (clerk.PropertyId != 0)
            {
                using Context context = new Context();

                Models.Property property = context.Property.Find(clerk.PropertyId);

                if (property != null && property.ClerkActive)
                {
                    property.ClerkActive = false;
                    context.SaveChanges();
                }

                
            }

            player.GetClass().HasClerkTask = false;
            player.GetClass().ClerkCount = 0;
            player.GetClass().ClerkJob = 0;

            if (OccupiedJobs.Contains(clerk.Id))
            {
                OccupiedJobs.Remove(clerk.Id);
            }
        }

        public static void AtJobPosition(IPlayer player)
        {
            player.GetClass().HasClerkTask = false;

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            playerCharacter.PaydayAmount += 7;

            context.SaveChanges();
            
        }

        public static void PublishClerkAdvert(Models.Clerk clerk)
        {
            List<string> gasAds = new List<string>
            {
                "Snacks, drinks, gasoline and more - fill up your gut and your tank!",
                "Cigarettes, cigars, and other smoking accessories - it’s good for you!",
                "Forget the groceries, we’ve got snacks - your wife will love you!",
                "Fill up that gas-guzzling SUV with premium gasoline!",
                "Our tills are always full of cash to better serve our customers!"
            };

            List<string> convenienceAds = new List<string>
            {
                "Cigarettes, cigars, and other smoking accessories - it’s good for you!",
                "Forget the groceries, we’ve got snacks - your wife will love you!",
                "Our tills are always full of cash to better serve our customers!",
                "Twice the convenience at twice the price!",
            };

            string adText = "";

            Random rnd = new Random();

            if (clerk.StoreType == ClerkStoreType.FuelStation)
            {
                int index = rnd.Next(0, gasAds.Count - 1);
                adText = gasAds[index];
            }

            if (clerk.StoreType == ClerkStoreType.Convenience)
            {
                int index = rnd.Next(0, convenienceAds.Count - 1);
                adText = convenienceAds[index];
            }

            string ad = $"[{clerk.StoreName}] {adText}";

            foreach (IPlayer target in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null).ToList())
            {
                target.SendAdvertMessage(ad);
            }

            SignalR.SendDiscordMessage(704070210357035018, ad);
        }
    }
}