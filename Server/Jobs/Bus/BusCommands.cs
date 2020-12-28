using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Jobs.Bus
{
    public class BusCommands
    {
        private static double _routeEarning = 30;

        [Command("bus", commandType: CommandType.Job, description: "Bus: Starts the bus route")]
        public static void CommandStartBusJob(IPlayer player)
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            Models.Character playerCharacter = player.FetchCharacter();

            List<Models.Jobs> playerJobs = JsonConvert.DeserializeObject<List<Models.Jobs>>(playerCharacter.JobList);

            if (!playerJobs.Contains(Models.Jobs.BusDriver))
            {
                player.SendErrorNotification("You must have the bus driver job!");
                return;
            }

            bool hasOnBusRouteData = player.GetData("bus:onRoute", out bool onRoute);

            if (hasOnBusRouteData && onRoute)
            {
                player.SendErrorNotification("You're already on a bus route!");
                return;
            }

            if (BusHandler.BusVehicles.ContainsKey(player.GetClass().CharacterId))
            {
                player.SendErrorNotification($"A bus has already been spawned!");
                return;
            }

            List<BusRoute> busRoutes = BusRoute.FetchBusRoutes();

            if (!busRoutes.Any())
            {
                player.SendErrorNotification("There are no bus routes available.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (BusRoute busRoute in busRoutes)
            {
                menuItems.Add(new NativeMenuItem(busRoute.RouteName));
            }

            NativeMenu menu = new NativeMenu("bus:RouteSelected", "LS Transit", "Select a Bus Route", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnBusRouteSelect(IPlayer player, string option)
        {
            try
            {
                if (option == "Close") return;

                BusRoute busRoute = BusRoute.FetchBusRoute(option);

                if (busRoute == null)
                {
                    player.SendErrorNotification("Unable to find this Bus Route.");
                    return;
                }

                List<BusStop> busStopList = BusRoute.FetchBusStops(busRoute.Id);

                if (!busStopList.Any())
                {
                    player.SendErrorNotification("There aren't any bus stops.");
                    return;
                }

                BusStop firstPoint = busStopList.FirstOrDefault();

                Position vehicleSpawnPosition = new Position(firstPoint.PosX, firstPoint.PosY, firstPoint.PosZ);

                bool spaceTaken = Alt.Server.GetVehicles().Any(x => x.Position.Distance(vehicleSpawnPosition) < 5f);

                if (spaceTaken)
                {
                    player.SendErrorNotification("Please wait till the vehicle has moved.");
                    return;
                }

                player.SetData("BusJob:StopCount", busStopList.Count);

                IVehicle busVehicle = Alt.CreateVehicle(VehicleModel.Bus, vehicleSpawnPosition,
                    new DegreeRotation(0, 0, firstPoint.RotZ));

                busVehicle.PrimaryColor = 6;
                busVehicle.SecondaryColor = 6;
                busVehicle.NumberplateText = "TRANSIT";
                busVehicle.SetNumberPlateStyleExt(NumberPlateStyle.BlueWhite3);
                busVehicle.SetData("FUELLEVEL", 100);
                busVehicle.SetData("bus:emptyTime", DateTime.Now.AddMinutes(10));
                busVehicle.SetData("ODOREADING", 193921124f);
                busVehicle.ManualEngineControl = false;
                busVehicle.LockState = VehicleLockState.Unlocked;

                BusHandler.BusVehicles.Add(player.GetClass().CharacterId, busVehicle);

                player.SetData("bus:onRoute", true);

                player.Emit("bus:startJob", JsonConvert.SerializeObject(busStopList));

                player.SendInfoNotification("You've started the bus job. Head to the bus to begin your route!");
            }
            catch (Exception e)
            {
                player.SendErrorNotification("An error occurred.");
                Console.WriteLine(e);
                return;
            }
        }

        [Command("stopbus", commandType: CommandType.Job, description: "Bus: Stops the bus route")]
        public static void CommandStopBus(IPlayer player)
        {
            if (player.FetchCharacter() == null)
            {
                player.SendLoginError();

                return;
            }

            if (!BusHandler.BusVehicles.ContainsKey(player.GetClass().CharacterId))
            {
                player.SendErrorNotification("Your not on a bus route!");
                return;
            }

            KeyValuePair<int, IVehicle>? keyValuePair = BusHandler.BusVehicles.FirstOrDefault(x => x.Key == player.GetClass().CharacterId);

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x == keyValuePair.Value);

            targetVehicle?.Remove();

            BusHandler.BusVehicles.Remove(keyValuePair.Key);

            player.SendInfoNotification($"You've stopped the bus route.");

            player.SetData("bus:onRoute", false);

            player.Emit("bus:endRoute");
        }

        public static void OnBusRouteFinish(IPlayer player)
        {
            KeyValuePair<int, IVehicle>? keyValuePair = BusHandler.BusVehicles.FirstOrDefault(x => x.Key == player.GetClass().CharacterId);

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x == keyValuePair.Value);

            targetVehicle?.Remove();

            BusHandler.BusVehicles.Remove(keyValuePair.Key);

            player.GetData("BusJob:StopCount", out int stopCount);

            player.SetData("bus:onRoute", false);

            _routeEarning = stopCount < 15 ? 80 : 150;

            player.SendInfoNotification($"You've completed the bus route. You've gained {_routeEarning:C} from this.");

            player.AddCash(_routeEarning);
        }
    }
}