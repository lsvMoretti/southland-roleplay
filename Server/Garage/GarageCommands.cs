using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Garage
{
    public class GarageCommands
    {
        [Command("genter", commandType: CommandType.Property, description: "Garages: Used to enter specific garages")]
        public static void GarageCommandEnter(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Garage nearestGarage = null;

            if (player.IsInVehicle)
            {
                if (player.Seat != 1)
                {
                    player.SendErrorNotification("You must be in the drivers seat.");
                    return;
                }

                Position vehiclePosition = player.Vehicle.Position;

                foreach (Models.Garage garage in Models.Garage.FetchGarages())
                {
                    Position garagePosition = Models.Garage.FetchGarageExteriorPosition(garage);

                    if (vehiclePosition.Distance(garagePosition) <= 5)
                    {
                        nearestGarage = garage;
                        break;
                    }
                }
            }
            else
            {
                Position playerPosition = player.Position;

                foreach (Models.Garage garage in Models.Garage.FetchGarages())
                {
                    Position garagePosition = Models.Garage.FetchGarageExteriorPosition(garage);

                    if (playerPosition.Distance(garagePosition) <= 5)
                    {
                        nearestGarage = garage;
                        break;
                    }
                }
            }

            if (nearestGarage == null)
            {
                player.SendErrorNotification("You aren't near a garage!");
                return;
            }

            Position interiorPosition = Models.Garage.FetchGarageInternalPosition(nearestGarage);

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(nearestGarage.PropJson);

            using Context context = new Context();

            if (player.IsInVehicle)
            {
                IVehicle vehicle = player.Vehicle;
                Dictionary<byte, int> occupants = vehicle.Occupants();

                foreach (KeyValuePair<byte, int> keyValuePair in occupants)
                {
                    IPlayer occupant = Alt.Server.GetPlayers()
                        .FirstOrDefault(x => x.GetPlayerId() == keyValuePair.Value);

                    if (occupant == null) continue;

                    occupant.SetData("VehicleSeat", keyValuePair.Key);

                    occupant.Emit("EjectFromVehicle", vehicle);
                }

                Timer teleportTimer = new Timer(1) { AutoReset = false };

                teleportTimer.Start();
                teleportTimer.Elapsed += (sender, args) =>
                {
                    teleportTimer.Stop();
                    vehicle.Position = interiorPosition + new Position(0, 0, 0.5f);
                    vehicle.Rotation = new Rotation(0, 0, nearestGarage.IntRotZ);
                    vehicle.Dimension = nearestGarage.Id;
                };

                foreach (KeyValuePair<byte, int> keyValuePair in occupants)
                {
                    IPlayer occupant = Alt.Server.GetPlayers()
                        .FirstOrDefault(x => x.GetPlayerId() == keyValuePair.Value);

                    if (occupant == null) continue;

                    occupant.GetData("VehicleSeat", out byte vehicleSeat);

                    occupant.Dimension = nearestGarage.Id;

                    if (!string.IsNullOrEmpty(nearestGarage.Ipl))
                    {
                        occupant.RequestIpl(nearestGarage.Ipl);
                    }

                    if (propList.Any())
                    {
                        foreach (string prop in propList)
                        {
                            occupant.LoadInteriorProp(prop);

                            if (nearestGarage.ColorId > 0)
                            {
                                occupant.Emit("SetInteriorColor", prop, nearestGarage.ColorId);
                            }
                        }
                    }

                    occupant.Position = interiorPosition + new Position(0, 0, 1f);

                    occupant.SetPosition(interiorPosition + new Position(0, 0, 1f),
                        new Rotation(0, 0, nearestGarage.IntRotZ), 2000, unfreezeTime: 3000);

                    occupant.Dimension = nearestGarage.Id;

                    Timer intoTimer = new Timer(5000) { AutoReset = false };
                    intoTimer.Start();

                    intoTimer.Elapsed += (sender, args) =>
                    {
                        intoTimer.Stop();

                        occupant.Emit("SetIntoVehicle", vehicle, vehicleSeat - 2);
                    };

                    Models.Character occupantDb = context.Character.Find(occupant.GetClass().CharacterId);

                    if (occupantDb == null) continue;

                    occupantDb.InsideGarage = nearestGarage.Id;


                    context.SaveChanges();
                }


                player.Dimension = nearestGarage.Id;
                
                Models.Vehicle vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

                vehicleDb.Dimension = (uint)nearestGarage.Id;

                context.SaveChanges();
            }
            else
            {
                player.Position = interiorPosition;
                player.Rotation = new Rotation(0, 0, nearestGarage.IntRotZ);
                player.Dimension = nearestGarage.Id;

                player.RequestIpl(nearestGarage.Ipl);

                context.Character.Find(player.GetClass().CharacterId).InsideGarage = nearestGarage.Id;

                context.SaveChanges();

                if (propList.Any())
                {
                    foreach (string prop in propList)
                    {
                        player.LoadInteriorProp(prop);

                        if (nearestGarage.ColorId > 0)
                        {
                            if (prop.Contains("tint"))
                            {
                                player.Emit("SetInteriorColor", prop, nearestGarage.ColorId);
                            }
                        }
                    }
                }
            }

            
        }

        [Command("gexit", commandType: CommandType.Property, description: "Garages: Used to exit garages")]
        public static void GarageCommandExit(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            Models.Garage nearestGarage = null;

            if (player.IsInVehicle)
            {
                if (player.Seat != 1)
                {
                    player.SendErrorNotification("You must be in the drivers seat.");
                    return;
                }

                nearestGarage = Models.Garage.FetchGarage(player.Dimension);
            }
            else
            {
                nearestGarage = Models.Garage.FetchGarage(player.Dimension);
            }

            if (nearestGarage == null)
            {
                nearestGarage = Models.Garage.FetchGarage(player.FetchCharacter().InsideGarage);
            }

            if (nearestGarage == null)
            {
                player.SendErrorNotification("You aren't near a garage.");
                return;
            }

            Position externalPosition = Models.Garage.FetchGarageExteriorPosition(nearestGarage);

            List<string> propList = JsonConvert.DeserializeObject<List<string>>(nearestGarage.PropJson);

            using Context context = new Context();
            if (player.IsInVehicle)
            {
                IVehicle vehicle = player.Vehicle;
                Dictionary<byte, int> occupants = vehicle.Occupants();

                foreach (KeyValuePair<byte, int> keyValuePair in occupants)
                {
                    IPlayer occupant = Alt.Server.GetPlayers()
                        .FirstOrDefault(x => x.GetPlayerId() == keyValuePair.Value);

                    if (occupant == null) continue;

                    occupant.SetData("VehicleSeat", keyValuePair.Key);

                    occupant.Emit("EjectFromVehicle", vehicle);
                }

                Timer teleportTimer = new Timer(1) { AutoReset = false };

                teleportTimer.Start();
                teleportTimer.Elapsed += (sender, args) =>
                {
                    teleportTimer.Stop();
                    vehicle.Position = externalPosition + new Position(0, 0, 0.5f);
                    vehicle.Rotation = new Rotation(0, 0, nearestGarage.ExtRotZ);
                    vehicle.Dimension = (int)nearestGarage.ExtDimension;
                };

                foreach (KeyValuePair<byte, int> keyValuePair in occupants)
                {
                    IPlayer occupant = Alt.Server.GetPlayers()
                        .FirstOrDefault(x => x.GetPlayerId() == keyValuePair.Value);

                    if (occupant == null) continue;

                    occupant.GetData("VehicleSeat", out byte vehicleSeat);

                    occupant.Dimension = nearestGarage.Id;

                    if (!string.IsNullOrEmpty(nearestGarage.Ipl))
                    {
                        occupant.RequestIpl(nearestGarage.Ipl);
                    }

                    if (propList.Any())
                    {
                        foreach (string prop in propList)
                        {
                            occupant.UnloadInteriorProp(prop);
                        }
                    }

                    if (nearestGarage.LinkedGarage > 0)
                    {
                        if (propList.Any())
                        {
                            foreach (string prop in propList)
                            {
                                occupant.UnloadInteriorProp(prop);

                                if (nearestGarage.ColorId > 0)
                                {
                                    if (prop.Contains("tint"))
                                    {
                                        occupant.Emit("SetInteriorColor", prop, nearestGarage.ColorId);
                                    }
                                }
                            }
                        }

                        context.Character.Find(occupant.GetClass().CharacterId).InsideGarage =
                            nearestGarage.LinkedGarage;
                        context.SaveChanges();
                        continue;
                    }

                    occupant.Position = externalPosition + new Position(0, 0, 1f);

                    occupant.SetPosition(externalPosition + new Position(0, 0, 1f),
                        new Rotation(0, 0, nearestGarage.ExtRotZ), 2000, unfreezeTime: 3000);

                    occupant.Dimension = (int)nearestGarage.ExtDimension;
                    ;

                    Timer intoTimer = new Timer(5000) { AutoReset = false };
                    intoTimer.Start();

                    intoTimer.Elapsed += (sender, args) =>
                    {
                        intoTimer.Stop();

                        occupant.Emit("SetIntoVehicle", vehicle, vehicleSeat - 2);
                    };

                    Models.Character occupantDb = context.Character.Find(occupant.GetClass().CharacterId);

                    if (occupantDb == null) continue;

                    occupantDb.InsideGarage = 0;

                    context.SaveChanges();
                }

                
                Models.Vehicle vehicleDb = context.Vehicle.Find(player.Vehicle.GetClass().Id);

                vehicleDb.Dimension = (uint)nearestGarage.ExtDimension;

                context.SaveChanges();
            }
            else
            {
                player.Position = externalPosition;
                player.Dimension = (int)nearestGarage.ExtDimension;
                player.Rotation = new Rotation(0, 0, nearestGarage.ExtRotZ);

                if (!string.IsNullOrEmpty(nearestGarage.Ipl))
                {
                    player.UnloadIpl(nearestGarage.Ipl);
                }

                if (propList.Any())
                {
                    foreach (string prop in propList)
                    {
                        player.UnloadInteriorProp(prop);
                    }
                }

                if (nearestGarage.LinkedGarage > 0)
                {
                    if (propList.Any())
                    {
                        foreach (string prop in propList)
                        {
                            player.UnloadInteriorProp(prop);

                            if (nearestGarage.ColorId > 0)
                            {
                                if (prop.Contains("tint"))
                                {
                                    player.Emit("SetInteriorColor", prop, nearestGarage.ColorId);
                                }
                            }
                        }
                    }

                    context.Character.Find(player.GetClass().CharacterId).InsideGarage = nearestGarage.LinkedGarage;
                    context.SaveChanges();
                    return;
                }

                context.Character.Find(player.GetClass().CharacterId).InsideGarage = 0;
                context.SaveChanges();
            }
        }
    }
}