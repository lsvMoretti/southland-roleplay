using System;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;

namespace Server.Jobs.FoodStand
{
    public class FoodStandCommands
    {
        /// <summary>
        /// Do they have the job data
        /// </summary>
        private static readonly string _jobDataName = "FoodStandJob";
        /// <summary>
        /// The FoodStandPosition data
        /// </summary>
        private static readonly string _jobDataPos = "FoodStandPosition";
        /// <summary>
        /// 1 - Burger, 2 - Hotdog
        /// </summary>
        private static readonly string _jobDataType = "FoodStandType";

        /// <summary>
        /// Location of the Food Stand
        /// </summary>
        private static readonly string _jobDataLoc = "FoodStandLoc";

        private static readonly string _jobTimerCount = "FoodStandTimerCount";
        
        /// <summary>
        /// Amount of pay to receive every five minutes
        /// </summary>
        private static readonly int _fiveMinutePay = 4;
        
        [Command("foodjob", commandType: CommandType.Job, description: "Food Stand: Starts the Food Stand Job")]
        public static void StartFoodStandJob(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            bool hasJobData = player.GetData(_jobDataName, out bool hasJob);

            if (hasJobData && hasJob)
            {
                StopFoodStandJob(player);
                return;
            }

            FoodStandPosition foodStand = FoodStandHandler.FetchNearestPosition(player.Position);

            if (foodStand == null)
            {
                NotificationExtension.SendErrorNotification(player, "Your not near a burger or hotdog stand.");
                return;
            }
            
            player.SetData(_jobDataName, true);
            player.SetData(_jobDataPos, foodStand);
            if (foodStand.IsBurgerStand)
            {
                player.SetData(_jobDataType, 1);
            }

            if (foodStand.IsHotDogStand)
            {
                player.SetData(_jobDataType, 2);
            }
            
            player.SendInfoNotification($"You've started the Food Job!");
            player.FetchLocation("FoodStand:FetchLocationData");
            
            player.SetData(_jobTimerCount, 1);
        }

        private static void StopFoodStandJob(IPlayer player)
        {
            player.DeleteData(_jobDataName);
            player.DeleteData(_jobDataPos);
            player.DeleteData(_jobDataType);
            player.DeleteData(_jobDataLoc);
            player.DeleteData(_jobTimerCount);
            NotificationExtension.SendInfoNotification(player, "You've stopped the food stand job.");
            return;
        }
        
        public static void OnLocationDataReturn(IPlayer player, string streetName, string areaName)
        {
            player.SetData(_jobDataLoc, $"{streetName}, {areaName}");
        }

        public static void StartFoodStandTimer()
        {
            //300000
            // Start five minute timer
            Timer fiveMinuteTimer = new Timer(300000)
            {
                AutoReset = true
            };
            
            fiveMinuteTimer.Start();
            fiveMinuteTimer.Elapsed += (sender, args) =>
            {
                fiveMinuteTimer.Stop();
                
                using Context context = new Context();
                
                foreach (IPlayer player in Alt.GetAllPlayers())
                {
                    bool hasJobData = player.GetData(_jobDataName, out bool hasJob);
                    if(!hasJobData || !hasJob) continue;

                    player.GetData(_jobDataPos, out FoodStandPosition standPosition);

                    if (standPosition == null)
                    {
                        player.SendErrorNotification("An error occurred fetching your food stand!");
                        StopFoodStandJob(player);
                        continue;
                    }

                    if (player.Position.Distance(standPosition.FetchPosition()) > 10)
                    {
                        player.SendErrorNotification("You've gone to far away from the food stand!");
                        return;
                    }

                    Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);
                    
                    if(playerCharacter == null) continue;

                    playerCharacter.PaydayAmount += _fiveMinutePay;

                    bool hasTimerCount = player.GetData(_jobTimerCount, out int count);

                    if (hasTimerCount)
                    {
                        if (count > 1)
                        {
                            player.GetData(_jobDataLoc, out string location);
                            player.GetData(_jobDataType, out int type);
                            
                            player.SetData(_jobTimerCount, 1);
                            foreach (IPlayer adPlayer in Alt.GetAllPlayers().Where(x => x.IsSpawned()))
                            {
                                if (type == 1)
                                {
                                    // Burger Stand
                                    adPlayer.SendAdvertMessage($"Come get your Beefy Bills Burgers over at {location}!");
                                }

                                if (type == 2)
                                {
                                    // Hotdog Stand
                                    adPlayer.SendAdvertMessage($"Got some filling dogs that are hot over at {location}!");
                                }
                            }
                            
                            continue;
                        }
                        if(count == 1)
                        {
                            player.SetData(_jobTimerCount, 2);
                        }
                    }
                    else
                    {
                        player.SetData(_jobTimerCount, 1);
                    }
                }

                context.SaveChanges();
                fiveMinuteTimer.Start();
                
            };
        }

        [Command("foodstand", commandType: CommandType.Job,
            description: "Food Stand: Allows you to buy from a food stand")]
        public static void FoodStandCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            bool hasJobData = player.GetData(_jobDataName, out bool hasJob);

            if (!hasJobData || !hasJob)
            {
                player.SendPermissionError();
                return;
            }

            int itemPrice = 1;

            if (player.GetClass().Cash < itemPrice)
            {
                player.SendErrorNotification($"You require {itemPrice:C}.");
                return;
            }
            
            player.GetData(_jobDataType, out int standType);

            Inventory.Inventory playerInventory = player.FetchInventory();
            
            if (standType == 1)
            {
                // Burger Stand
                
                InventoryItem burgerItem = new InventoryItem("ITEM_BEEFY_BURGER", "Beefy Burger");

                bool itemAdded = playerInventory.AddItem(burgerItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding the burger to your inventory.");
                    return;
                }
                
                player.RemoveCash(itemPrice);
                player.SendInfoNotification($"You've got a burger from the Food Stand!");
                return;
            }

            if (standType == 2)
            {
                // Hotdogs
                
                InventoryItem hotdogItem = new InventoryItem("ITEM_HOTDOG", "Hot Dog");

                bool itemAdded = playerInventory.AddItem(hotdogItem);

                if (!itemAdded)
                {
                    player.SendErrorNotification("There was an error adding the Hotdog to your inventory.");
                    return;
                }
                
                player.RemoveCash(itemPrice);
                player.SendInfoNotification($"You've got a Hotdog from the Food Stand!");
                return;
            }
        }
    }
}