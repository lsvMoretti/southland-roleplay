using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.Marker;
using Server.Extensions.TextLabel;
using Blip = Server.Objects.Blip;

namespace Server.Jobs.Bus
{
    public class BusHandler
    {
        public static Position CommandPosition = new Position(435.1624f, -645.9141f, 28.73564f);
        public static Dictionary<int, IVehicle> BusVehicles = new Dictionary<int, IVehicle>();

        private static int _busEmptyTime = 10;

        public static void InitBusJob()
        {
            Blip busBlip = new Blip("Bus Driver", CommandPosition, 513, 4, 0.75f);
            busBlip.Add();

            Marker busMarker = new Marker(MarkerType.MarkerTypeVerticleCircle, CommandPosition - new Position(0, 0, 1.5f), Vector3.Zero, Vector3.Zero, 0.5f, Color.FromArgb(43, 147, 227));

            busMarker.Add();

            TextLabel busLabel = new TextLabel("Bus Job\nType /bus\nType /join to join!", CommandPosition, TextFont.FontChaletComprimeCologne, new LsvColor(43, 147, 227));

            busLabel.Add();

            Timer thirtySecondTimer = new Timer(30000) { AutoReset = true };

            thirtySecondTimer.Elapsed += (sender, args) =>
            {
                thirtySecondTimer.Stop();
                if (!BusVehicles.Any())
                {
                    thirtySecondTimer.Start();
                    return;
                }

                foreach (KeyValuePair<int, IVehicle> keyValuePair in BusVehicles)
                {
                    IVehicle vehicle = keyValuePair.Value;

                    IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x == vehicle);

                    if (targetVehicle == null)
                    {
                        BusVehicles.Remove(keyValuePair.Key);
                        continue;
                    }

                    bool hasData = vehicle.GetData("bus:emptyTime", out DateTime nextEmptyCheck);

                    if (hasData)
                    {
                        if (DateTime.Compare(DateTime.Now, nextEmptyCheck) > 0)
                        {
                            if (vehicle.Driver != null)
                            {
                                vehicle.SetData("bus:emptyTime", DateTime.Now.AddMinutes(_busEmptyTime));
                                continue;
                            }

                            vehicle.Remove();

                            IPlayer targetPlayer = Alt.Server.GetPlayers()
                                .FirstOrDefault(x => x.GetClass().CharacterId == keyValuePair.Key);

                            if (targetPlayer == null) continue;

                            bool busRouteData = targetPlayer.GetData("bus:onRoute", out bool onRoute);

                            if (busRouteData && onRoute)
                            {
                                targetPlayer.SetData("bus:onRoute", false);
                                targetPlayer.SendErrorNotification("Your bus has been despawned. You were out of the bus for more than 10 minutes!");
                                BusVehicles.Remove(keyValuePair.Key);
                                targetPlayer.Emit("bus:endRoute");
                            }
                        }
                        continue;
                    }

                    if (Alt.Server.GetPlayers().FirstOrDefault(x => x.GetClass().CharacterId == keyValuePair.Key) == null)
                    {
                        vehicle.Remove();
                        BusVehicles.Remove(keyValuePair.Key);
                    }
                }

                thirtySecondTimer.Start();
            };
        }

        public static void OnEnterMarker(IPlayer player, int stopId, int stopCount)
        {
            player.SendNotification($"You've reached {stopId}/{stopCount}");
        }
    }
}