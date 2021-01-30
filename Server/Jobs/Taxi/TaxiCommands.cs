using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Jobs.Taxi
{
    public class TaxiCommands
    {
        [Command("taxiduty", commandType: CommandType.Job, description: "Taxi: Sets yourself on Taxi Duty")]
        public static void TaxiCommandDuty(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (string.IsNullOrEmpty(player.FetchCharacter().JobList))
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasTaxiJob = JsonConvert.DeserializeObject<List<Models.Jobs>>(player.FetchCharacter().JobList)
                .Contains(Models.Jobs.TaxiDriver);

            if (!hasTaxiJob)
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasDutyData = player.GetData("taxi:onDuty", out bool taxiDuty);

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null || playerVehicle.Model != Alt.Server.Hash("taxi"))
            {
                player.SendErrorNotification("You must be in a Taxi!");
                return;
            }

            if (taxiDuty)
            {
                player.SetData("taxi:onDuty", false);
                player.Emit("setTaxiLight", false);
                playerVehicle.SetData("taxi:lightStatus", false);

                player.SendInfoNotification($"You have gone off Taxi Duty.");

                return;
            }

            player.SetData("taxi:onDuty", true);
            player.Emit("setTaxiLight", true);
            playerVehicle.SetData("taxi:lightStatus", true);

            player.SendInfoNotification("You have gone on Taxi Duty.");

            foreach (IPlayer target in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null).ToList())
            {
                target.SendAdvertMessage($"A taxi is now available. Please call 5555 to arrange your transport!");
            }
        }

        [Command("taxilight", alternatives: "tlight", commandType: CommandType.Job, description: "Taxi: Enables the taxi job")]
        public static void TaxiCommandTaxiLight(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (string.IsNullOrEmpty(player.FetchCharacter().JobList))
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasTaxiJob = JsonConvert.DeserializeObject<List<Models.Jobs>>(player.FetchCharacter().JobList)
                .Contains(Models.Jobs.TaxiDriver);

            if (!hasTaxiJob)
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasDutyData = player.GetData("taxi:onDuty", out bool taxiDuty);

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null || playerVehicle.Model != Alt.Server.Hash("taxi"))
            {
                player.SendErrorNotification("You must be in a Taxi!");
                return;
            }

            if (!hasDutyData || !taxiDuty)
            {
                player.SendErrorNotification("You are not on Taxi Duty!");
                return;
            }

            bool hasLightData = playerVehicle.GetData("taxi:lightStatus", out bool lightStatus);

            if (!hasLightData || !lightStatus)
            {
                player.Emit("setTaxiLight", true);
                playerVehicle.SetData("taxi:lightStatus", true);
                player.SendInfoNotification("You've turned on your Taxi Light.");
                return;
            }

            player.Emit("setTaxiLight", false);
            playerVehicle.SetData("taxi:lightStatus", false);
            player.SendInfoNotification("You've turned off your Taxi Light.");
        }

        [Command("taxicalls", commandType: CommandType.Job, description: "Taxi: Shows available Taxi Calls")]
        public static void TaxiCommandTaxiCalls(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (string.IsNullOrEmpty(player.FetchCharacter().JobList))
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasTaxiJob = JsonConvert.DeserializeObject<List<Models.Jobs>>(player.FetchCharacter().JobList)
                .Contains(Models.Jobs.TaxiDriver);

            if (!hasTaxiJob)
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasDutyData = player.GetData("taxi:onDuty", out bool taxiDuty);

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null || playerVehicle.Model != Alt.Server.Hash("taxi"))
            {
                player.SendErrorNotification("You must be in a Taxi!");
                return;
            }

            if (!hasDutyData || !taxiDuty)
            {
                player.SendErrorNotification("You are not on Taxi Duty!");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (TaxiCall taxiCall in CallHandler.TaxiCalls)
            {
                menuItems.Add(new NativeMenuItem($"Call Id: {taxiCall.Id}", $"{taxiCall.Street}, {taxiCall.Area}"));
            }

            NativeMenu nativeMenu = new NativeMenu("taxi:showTaxiCallList", "Taxi Calls", "Select a Call to Respond", menuItems);

            NativeUi.ShowNativeMenu(player, nativeMenu, true);
        }

        public static void OnTaxiCallListSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            var subString = option.Substring(9);

            bool tryParse = int.TryParse(subString, out int callId);

            if (!tryParse)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            TaxiCall selectedCall = CallHandler.TaxiCalls.FirstOrDefault(x => x.Id == callId);

            if (selectedCall == null)
            {
                player.SendErrorNotification("This call no longer exists!");
                return;
            }

            CallHandler.TaxiCalls.Remove(selectedCall);

            IPlayer? targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x.GetClass().CharacterId == selectedCall.CallerId);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Target caller not found.");
                return;
            }

            player.SendInfoNotification($"You have accepted the Call Id {selectedCall.Id}, Contact Number: {selectedCall.Number}. A marker has been set to their location on the GPS.");
            player.SendInfoNotification($"Their destination is {selectedCall.Destination}.");

            targetPlayer.SendInfoNotification($"Your Taxi Call has been accepted!");

            player.SetWaypoint(selectedCall.Position);

            Logging.AddToCharacterLog(player, $"has accepted Taxi Call Id: {selectedCall.Id} for player {targetPlayer.GetClass().Name}.");
        }

        [Command("startfare", onlyOne: true, commandType: CommandType.Job, description: "Taxi: Starts a fare for a player")]
        public static void CommandStartFare(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                player.SendSyntaxMessage("/startfare [IdOrName]");
                return;
            }

            if (!string.IsNullOrEmpty(player.FetchCharacter().JobList))
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasTaxiJob = JsonConvert.DeserializeObject<List<Models.Jobs>>(player.FetchCharacter().JobList)
                .Contains(Models.Jobs.TaxiDriver);

            if (!hasTaxiJob)
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasDutyData = player.GetData("taxi:onDuty", out bool taxiDuty);

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null || playerVehicle.Model != Alt.Server.Hash("taxi"))
            {
                player.SendErrorNotification("You must be in a Taxi!");
                return;
            }

            if (!hasDutyData || !taxiDuty)
            {
                player.SendErrorNotification("You are not on Taxi Duty!");
                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            if (targetPlayer.Vehicle != player.Vehicle)
            {
                player.SendErrorNotification("They are not in your vehicle!");
                return;
            }

            if (playerVehicle.FetchVehicleData() == null)
            {
                player.SendErrorNotification("You must be in an owned vehicle!");
                return;
            }

            player.SetData("taxi:payingCustomer", targetPlayer.GetPlayerId());

            float vehicleStartDistance = playerVehicle.FetchVehicleData().Odometer / 1609;

            double distance = 0;
            double lastDistance = 0;

            Timer distanceTimer = new Timer(1000) { AutoReset = true };

            distanceTimer.Start();

            targetPlayer.SendInfoNotification($"The fare has started.");
            player.SendInfoNotification($"The fare has started.");

            targetPlayer.SetData("taxi:driverId", player.GetPlayerId());

            double currentCost = 2.5;

            targetPlayer.SetData("taxi:CurrentCost", currentCost);

            player.SetData("taxi:CurrentCost", currentCost);

            player.SendInfoNotification($"Current Fare: {currentCost:C}");
            targetPlayer.SendInfoNotification($"Current Fare: {currentCost:C}");

            distanceTimer.Elapsed += (sender, eventArgs) =>
            {
                distanceTimer.Stop();
                bool hasData = player.GetData("taxi:payingCustomer", out int targetId);

                if (targetId == 0)
                {
                    distanceTimer.Stop();
                    player.SendInfoNotification("The Fare has stopped.");
                    return;
                }

                targetPlayer = Alt.GetAllPlayers().FirstOrDefault(x => x.GetPlayerId() == targetId);

                if (targetPlayer == null)
                {
                    distanceTimer.Stop();
                    player.SendInfoNotification("The Fare has stopped.");
                    return;
                }

                distance = Math.Round((playerVehicle.FetchVehicleData().Odometer / 1609) - vehicleStartDistance, 1);

                if (distance >= lastDistance + 0.5)
                {
                    // This works every 0.5 miles sends message
                    // distance = distance traveled since start of fare

                    lastDistance = distance;
                    currentCost += 0.5;
                    player.SendInfoNotification($"Current Fare: {currentCost:C}");
                    targetPlayer.SendInfoNotification($"Current Fare: {currentCost:C}");
                    targetPlayer.SetData("taxi:CurrentCost", currentCost);
                    player.SetData("taxi:CurrentCost", currentCost);
                }

                distanceTimer.Start();
            };
        }

        [Command("stopfare", commandType: CommandType.Job, description: "Taxi: Stops the fare for the player")]
        public static void TaxiCommandStopFare(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (!string.IsNullOrEmpty(player.FetchCharacter().JobList))
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }
            bool hasTaxiJob = JsonConvert.DeserializeObject<List<Models.Jobs>>(player.FetchCharacter().JobList)
                .Contains(Models.Jobs.TaxiDriver);

            if (!hasTaxiJob)
            {
                player.SendErrorNotification("You don't have the Taxi Job!");
                return;
            }

            bool hasDutyData = player.GetData("taxi:onDuty", out bool taxiDuty);

            IVehicle playerVehicle = player.Vehicle;

            if (playerVehicle == null || playerVehicle.Model != Alt.Server.Hash("taxi"))
            {
                player.SendErrorNotification("You must be in a Taxi!");
                return;
            }

            if (!hasDutyData || !taxiDuty)
            {
                player.SendErrorNotification("You are not on Taxi Duty!");
                return;
            }

            bool hasPassengerData = player.GetData("taxi:payingCustomer", out int targetCharacterId);

            if (!hasPassengerData || targetCharacterId == 0)
            {
                player.SendErrorNotification("You don't have a fare.");
                return;
            }

            player.SetData("taxi:payingCustomer", 0);

            player.SendInfoNotification($"You've stopped the fare.");
        }
    }
}