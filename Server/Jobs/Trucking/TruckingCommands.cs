using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Jobs.Delivery;
using Server.Models;

namespace Server.Jobs.Trucking
{
    public class TruckingCommands
    {
        private static Dictionary<int, IVehicle> _truckingVehicles = new Dictionary<int, IVehicle>();
        private static readonly Position TruckSpawnPosition = new Position(-409.43738f, -2789.8022f, 6.060791f);
        private static readonly Rotation TruckSpawnRotation = new Rotation(0, 0, -44.762325f);
        private static readonly int Payment = 15;

        [Command("starttrucking", commandType: CommandType.Job, description: "Used to start trucking")]
        public static async void TruckingCommandStart(IPlayer player)
        {
            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must be on foot.");
                return;
            }

            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            if (player.Position.Distance(DeliveryHandler.JobPosition) > 5)
            {
                player.SendErrorNotification("You must be near the delivery job!");
                return;
            }

            if (_truckingVehicles.ContainsKey(player.GetClass().CharacterId))
            {
                player.SendErrorNotification("You already have a truck out!");
                return;
            }

            List<IVehicle> vehicles = Alt.Server.GetVehicles().ToList();

            if (vehicles.Any(x => x.Position.Distance(TruckSpawnPosition) <= 5))
            {
                player.SendErrorNotification("The truck spawn point is occupied!");
                return;
            }

            IVehicle? truck = Alt.Server.CreateVehicle((uint)VehicleModel.Pounder, TruckSpawnPosition, TruckSpawnRotation);

            if (truck == null)
            {
                player.SendErrorNotification("Unable to spawn the vehicle!");
                return;
            }

            truck.ManualEngineControl = true;
            truck.EngineOn = false;
            truck.LockState = VehicleLockState.Unlocked;

            truck.GetClass().FuelLevel = 100;
            truck.GetClass().Distance = Convert.ToSingle(Utility.GenerateRandomNumber(6));

            truck.SetData("Trucking:Owner", player.GetClass().CharacterId);
            truck.SetData("Trucking:Products", 0);

            _truckingVehicles.Add(player.GetClass().CharacterId, truck);

            player.SendInfoNotification("The Pounder has been spawned! You can now head to a /warehouses and /loadtruck.");
        }

        [Command("stoptrucking", commandType: CommandType.Job, description: "Used to stop trucking")]
        public static void TruckingCommandStop(IPlayer player)
        {
            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            bool hasVehicle = _truckingVehicles.TryGetValue(player.GetClass().CharacterId, out IVehicle truck);

            if (!hasVehicle)
            {
                player.SendErrorNotification("You don't have a truck!");
                return;
            }

            truck.Delete();

            _truckingVehicles.Remove(player.GetClass().CharacterId);

            player.SendInfoNotification("You've stopped trucking.");
        }

        [Command("loadtruck", commandType: CommandType.Job, description: "Used to load the truck at Post Op.")]
        public static void TruckingCommandLoad(IPlayer player)
        {
            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in your truck!");
                return;
            }

            bool hasVehicle = _truckingVehicles.TryGetValue(player.GetClass().CharacterId, out IVehicle truck);

            if (!hasVehicle)
            {
                player.SendErrorNotification("You don't have a truck!");
                return;
            }

            if (player.Vehicle != truck)
            {
                player.SendErrorNotification("You must be in your truck!");
                return;
            }

            if (player.Vehicle.EngineOn)
            {
                player.SendInfoNotification($"The engine must be off!");
                return;
            }

            Warehouse nearestWarehouse = WarehouseHandler.FetchNearestPoint(player.Position, 20);

            if (nearestWarehouse == null)
            {
                player.SendErrorNotification("You must be near a warehouse!");
                return;
            }

            player.Vehicle.GetData("Trucking:Products", out int productCount);

            if (productCount >= 50)
            {
                player.SendErrorNotification("You have the max products (50)");
                return;
            }

            Timer waitTimer = new Timer(5000)
            {
                AutoReset = false
            };

            waitTimer.Start();

            waitTimer.Elapsed += (sender, args) =>
            {
                waitTimer.Stop();
                truck.DeleteData("Trucking:Loading");

                int remainingProducts = 50 - productCount;

                if (remainingProducts <= 10)
                {
                    truck.SetData("Trucking:Products", 50);
                    player.SendInfoNotification($"You've loaded a total of 50 products onto the vehicle.");
                    return;
                }

                int newCount = productCount += 10;

                truck.SetData("Trucking:Products", newCount);
                player.SendInfoNotification($"You've loaded {newCount}/50 products. You can now /deliver to head to a location!");
            };

            truck.SetData("Trucking:Loading", true);

            player.SendInfoNotification("Loading the products..", 5000);
        }

        [Command("deliver", commandType: CommandType.Job, description: "Used to deliver to a business")]
        public static void TruckingCommandDeliver(IPlayer player)
        {
            if (!player.HasJob(Models.Jobs.DeliveryJob))
            {
                player.SendPermissionError();
                return;
            }

            bool hasVehicle = _truckingVehicles.TryGetValue(player.GetClass().CharacterId, out IVehicle? truck);

            if (!hasVehicle)
            {
                player.SendErrorNotification("You don't have a truck!");
                return;
            }

            if (player.GetClass().LastVehicle == null || player.GetClass().LastVehicle != truck)
            {
                player.SendErrorNotification("Your last vehicle isn't a pounder!");
                return;
            }

            bool hasDeliveryPoint = player.GetData("Trucking:DeliveryBusiness", out int businessId);

            if (hasDeliveryPoint)
            {
                Models.Property? deliverBusiness = Models.Property.FetchProperty(businessId);

                if (deliverBusiness == null)
                {
                    player.SendErrorMessage("Unable to find this business.");
                }

                float doorDistance = player.Position.Distance(deliverBusiness.FetchExteriorPosition());

                Console.WriteLine($"Distance to {deliverBusiness.BusinessName} is {doorDistance}");

                if (doorDistance > 5)
                {
                    player.SendErrorNotification("You must be stood by the business door!");
                    return;
                }

                truck.GetData("Trucking:Products", out int truckProducts);

                int businessProductSpace = 100 - deliverBusiness.Products;

                if (businessProductSpace <= 0)
                {
                    player.SendErrorNotification("This business is full of products.");
                    player.DeleteData("Trucking:DeliveryBusiness");
                    return;
                }

                if (businessProductSpace <= 10)
                {
                    if (truckProducts > 10)
                    {
                        int truckCount = truckProducts -= businessProductSpace;

                        truck.SetData("Trucking:Products", truckCount);
                        player.DeleteData("Trucking:DeliveryBusiness");
                        player.SendInfoNotification($"You've dropped off {businessProductSpace} products at the business. This business is now full! You have {truckCount} products left in your truck.");

                        using Context context = new Context();

                        Models.Property fullBusiness = context.Property.Find(deliverBusiness.Id);

                        Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                        playerCharacter.PaydayAmount += Payment;

                        player.SendInfoNotification($"You've gained {Payment:C} into your next payday!");

                        fullBusiness.Products = 100;
                        context.SaveChanges();
                        return;
                    }
                    else
                    {
                        truck.SetData("Trucking:Products", 0);
                        player.DeleteData("Trucking:DeliveryBusiness");
                        player.SendInfoNotification($"You've dropped off {truckProducts} products at the business. You have 0 products left in your truck.");

                        using Context context = new Context();

                        Models.Property fullBusiness = context.Property.Find(deliverBusiness.Id);

                        Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                        playerCharacter.PaydayAmount += Payment;

                        player.SendInfoNotification($"You've gained {Payment:C} into your next payday!");

                        fullBusiness.Products += truckProducts;
                        context.SaveChanges();
                        return;
                    }
                }
                else
                {
                    if (truckProducts < 10)
                    {
                        truck.SetData("Trucking:Products", 0);
                        player.DeleteData("Trucking:DeliveryBusiness");
                        player.SendInfoNotification($"You've dropped off {truckProducts} products at the business. You have 0 products left in your truck.");

                        using Context context = new Context();

                        Models.Property business = context.Property.Find(deliverBusiness.Id);

                        Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                        playerCharacter.PaydayAmount += Payment;

                        player.SendInfoNotification($"You've gained {Payment:C} into your next payday!");

                        business.Products += truckProducts;
                        context.SaveChanges();
                        return;
                    }
                    else
                    {
                        int truckCount = truckProducts -= 10;
                        truck.SetData("Trucking:Products", truckCount);
                        player.DeleteData("Trucking:DeliveryBusiness");
                        player.SendInfoNotification($"You've dropped off 10 products at the business. You have {truckCount} products left in your truck.");

                        using Context context = new Context();

                        Models.Property business = context.Property.Find(deliverBusiness.Id);

                        Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                        playerCharacter.PaydayAmount += Payment;

                        player.SendInfoNotification($"You've gained {Payment:C} into your next payday!");

                        business.Products = business.Products += 10;
                        context.SaveChanges();
                        return;
                    }
                }

                return;
            }

            // Hasn't been assigned a property yet

            Random random = new Random();

            List<Models.Property> properties = Models.Property.FetchProperties()
                .Where(x => x.PropertyType != PropertyType.House && x.Products < 100 && x.BlipId > 0 && !x.Hidden).ToList();

            if (!properties.Any())
            {
                player.SendErrorNotification("There are no businesses that need stocking!");
                return;
            }

            int index = random.Next(properties.Count);

            Models.Property selectedProperty = properties[index];

            player.SetData("Trucking:DeliveryBusiness", selectedProperty.Id);

            player.SetWaypoint(selectedProperty.FetchExteriorPosition());

            player.SendInfoNotification($"Head to {selectedProperty.BusinessName} and type /deliver again.");
        }
    }
}