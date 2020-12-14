using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using AltV.Net.Elements.Entities;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Server.Chat;
using Server.Commands;
using Server.DMV;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Focuses;
using Server.Jobs.Bus;
using Server.Jobs.Delivery;
using Server.Jobs.FoodStand;
using Server.Models;
using Server.Vehicle;
using Position = AltV.Net.Data.Position;
using Rotation = AltV.Net.Data.Rotation;

namespace Server.Character
{
    public class WelcomePlayer
    {
        public static readonly Position PedPosition = new Position(267.75824f, -1200.3824f, 28.279907f);
        public static readonly Rotation PedRotation = new Rotation(0f, 0f, 90.70866f);
        public static readonly string WelcomeData = "WelcomePlayerData";

        /*
         * Welcome Stages
         * 1 - Vehicle Rental
         * 2 - Vehicle has been rented
         * 3 - Player has used /bank
         * 4 - Has typed /buy at an electronics store
         */
        
        public static void InitPed()
        {
            TextLabel label = new TextLabel("Welcome!\nType /welcome to get started", PedPosition + new Position(0, 0, 1f), TextFont.FontChaletComprimeCologne, new LsvColor(Color.Brown));
            label.Add();
        }
        
        public static void InitPedForPlayer(IPlayer player)
        {
            // Send event to load ped for player
            player.Emit("WelcomePed:SpawnPed", PedPosition, PedRotation);
            if (player.FetchCharacter().StartStage < 3)
            {
                player.Emit("WelcomePed:ShowInitMessage");
            }
        }

        [Command("welcome", commandType: CommandType.Character, description: "Used when at the Welcome Ped")]
        public static void WelcomePlayerCommand(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null || !player.IsSpawned())
            {
                player.SendPermissionError();
                return;
            }

            if (player.Position.Distance(PedPosition) > 3)
            {
                player.SendErrorNotification("You're not near Oscar!");
                return;
            }

            int startStage = playerCharacter.StartStage;

            bool hasWelcomeData = player.GetData(WelcomeData, out int welcomeStage);
            
            if (startStage <= 2)
            {
                player.Emit("WelcomePed:ShowWelcomeMessage");
                player.SetData(WelcomeData, 1);
                return;
            }

            if (startStage == 3)
            {
                // Needs to complete DMV
                player.Emit("WelcomePed:SendToDmv", DmvHandler.DmvPosition);
                player.SetData(WelcomeData, 5);
                return;
            }

            if (startStage == 4)
            {
                OnDmvFinish(player, true);
                return;
            }

        }

        public static void OnRentVehicle(IPlayer player)
        {
            player.Emit("WelcomePed:OnRentVehicle");
            player.SetData(WelcomeData, 2);
            player.AddCash(VehicleRental.DilettanteInitCost);
        }

        public static void OnBankCommand(IPlayer player)
        {
            player.SetData(WelcomeData, 3);
            player.Emit("WelcomePed:OnBankCommand");
        }

        public static void OnBuyCommand(IPlayer player)
        {
            bool hasWelcomeData = player.GetData(WelcomeData, out int welcomeStage);

            if (!hasWelcomeData) return;
            
            if (welcomeStage == 3)
            {
                // Phone
                player.SetData(WelcomeData, 4);
                player.Emit("WelcomePed:OnBuyCommand", "phone");
                return;
            }

            if (welcomeStage == 4)
            {
                // Clothes
                player.SetData(WelcomeData, 5);
                player.Emit("WelcomePed:OnBuyCommand", "clothing", DmvHandler.DmvPosition);
                
                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                if (playerCharacter == null) return;

                playerCharacter.StartStage = 3;
                context.SaveChanges();
                return;
            }
        }

        public static void OnDmvFinish(IPlayer player, bool usedCommand = false)
        {
            player.Emit("WelcomePed:OnDmvFinish", usedCommand);
            if (usedCommand)
            {
                List<NativeMenuItem> menuItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Mechanic", "Fixes cars!"),
                    new NativeMenuItem("Fisherman", "Finds Fish!"),
                    new NativeMenuItem("Delivery Driver", "Delivers things!"),
                    new NativeMenuItem("Store Clerk", "Stacks shelves and serves the city!"),
                    new NativeMenuItem("Bus Driver", "Drives around the city looking cool."),
                    new NativeMenuItem("Food Stand", "Serves some of the questionable food."),
                };

                NativeMenu menu = new NativeMenu("WelcomePlayer:JobMenu", "Jobs", "Select from a job below!", menuItems);
                NativeUi.ShowNativeMenu(player, menu, true);
                return;
            }
        }

        public static void OnJobMenuSelectItem(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Mechanic")
            {
                player.SetWaypoint(FocusHandler.MechanicPosition);
                SendWelcomePersonMessage(player, "Mechanic Job", "Head to the waypoint", "I've set a waypoint on your map for the Mechanic Job!");
                return;
            }

            if (option == "Fisherman")
            {
                player.SetWaypoint(new Position(-1851.11f, -1248.26f, 8.6f));
                SendWelcomePersonMessage(player, "Fisherman", "So you want to smell of fish?", "I've set a waypoint to a fishing spot!");
                return;
            }
            
            if (option == "Delivery Driver")
            {
                player.SetWaypoint(DeliveryHandler.JobPosition);
                SendWelcomePersonMessage(player, "Delivery Driver", "So you want to drop off package?", "I've set a waypoint to the job!");
                return;
            }
            
            if (option == "Store Clerk")
            {
                player.SetWaypoint(new Position(-14.1341f,-1748.56f,29.4147f));
                SendWelcomePersonMessage(player, "Store Clerk", "So you want to get robbed?", "I've set a waypoint to the job in Davis LTD!");
                return;
            }
            
            if (option == "Bus Driver")
            {
                player.SetWaypoint(BusHandler.CommandPosition);
                SendWelcomePersonMessage(player, "Bus Driver", "So you want to get lost?", "I've set a waypoint to the job for you!");
                return;
            }
            
            if (option == "Food Stand")
            {
                Random rnd = new Random();
                int r = rnd.Next(FoodStandHandler.FoodStands.Count);

                player.SetWaypoint(FoodStandHandler.FoodStands[r].FetchPosition());
                SendWelcomePersonMessage(player, "Food Stand", "So you want to serve some hotdogs?", "I've set a waypoint to a cart for you. You can do this at any cart for hotdogs and burgers!");
                return;
            }
        }

        private static void SendWelcomePersonMessage(IPlayer player, string title, string subTitle, string message)
        {
            player.Emit("WelcomePed:SendMessage", title, subTitle, message);
        }
    }
}