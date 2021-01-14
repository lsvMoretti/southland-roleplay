using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Character;
using Server.Chat;
using Server.Commands;
using Server.DMV;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Inventory;
using Server.Models;
using Blip = Server.Objects.Blip;

namespace Server.Vehicle
{
    public class VehicleRental
    {
        public static Dictionary<int, IVehicle> RentalVehicles = new Dictionary<int, IVehicle>();
        private static Dictionary<int, string> _rentalKeys = new Dictionary<int, string>();

        private static readonly Position DmvCommandPosition = new Position(-938.75604f, -282.34286f, 39.288696f);
        private static readonly Position DmvRentalSpawnPosition = new Position(-919.74066f, -313.80658f, 38.850586f);
        private static readonly DegreeRotation DmvRentalSpawnRotation = new DegreeRotation(0, 0, 66.248245f);

        private static readonly Position PillboxRentalSpawnPosition = new Position(107.63077f, -1080.6461f, 28.673218f);
        private static readonly DegreeRotation PillboxRentalSpawnRotation = new DegreeRotation(0, 0, -19.695423f);
        private static readonly Position PillboxCommandPosition = new Position(100.37802f, -1073.3011f, 29.364136f);

        private static readonly Position MarinaSpawnPosition = new Position(-722.7033f, -1326.8704f, -0.08935547f);
        private static readonly DegreeRotation MarinaSpawnRotation = new DegreeRotation(0, 0, -127.125f);
        private static readonly Position MarinaCommandPosition = new Position(-712.6418f, -1298.7429f, 5.100342f);

        public static double DilettanteInitCost = 50;
        private static double _seaSharkInitCost = 70;

        public static void InitVehicleRentalSystem()
        {
            TextLabel dmvLabel = new TextLabel($"Vehicle Rental\n/rentvehicle\n{DilettanteInitCost:C}", DmvCommandPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.OrangeRed));
            dmvLabel.Add();

            TextLabel pillboxLabel = new TextLabel($"Vehicle Rental\n/rentvehicle\n{DilettanteInitCost:C}", PillboxCommandPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.OrangeRed));
            pillboxLabel.Add();

            TextLabel marinaLabel = new TextLabel($"Vehicle Rental\n/rentvehicle\n{_seaSharkInitCost:C}", MarinaCommandPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.OrangeRed));
            marinaLabel.Add();

            Blip dmvBlip = new Blip("Vehicle Rental", DmvCommandPosition, 225, 7, 0.5f);
            dmvBlip.Add();

            Blip pillboxBlip = new Blip("Vehicle Rental", PillboxCommandPosition, 225, 7, 0.5f);
            pillboxBlip.Add();

            Blip marinaBlip = new Blip("Vehicle Rental", MarinaCommandPosition, 455, 7, 0.5f);
            marinaBlip.Add();
        }

        [Command("rentvehicle", commandType: CommandType.Vehicle, description: "Rental: Used to rent a vehicle.")]
        public static void VehicleRentalCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            // 0 - None, 1 - DMV, 2 - Pillbox, 3 - Marina
            int atPosition = 0;

            if (player.Position.Distance(DmvCommandPosition) < 5f)
            {
                atPosition = 1;
            }

            if (player.Position.Distance(PillboxCommandPosition) < 5f)
            {
                atPosition = 2;
            }

            if (player.Position.Distance(MarinaCommandPosition) < 5f)
            {
                atPosition = 3;
            }

            if (atPosition == 0)
            {
                player.SendErrorNotification("You must be at the DMV or Pillbox to rent a vehicle!");
                return;
            }

            Position rentalSpawn = new Position();
            Rotation rentalRotation = new Rotation();
            VehicleModel vehicleModel = VehicleModel.Dilettante;
            double cost = DilettanteInitCost;

            string rentalKeyCode = Utility.GenerateRandomString(8);
            InventoryItem rentalKey = null;

            // 1 - DMV
            // 2 - Pillbox (Caesars Parking)
            // 3 - Marina

            switch (atPosition)
            {
                case 1:
                    rentalSpawn = DmvRentalSpawnPosition;
                    rentalRotation = DmvRentalSpawnRotation;
                    vehicleModel = VehicleModel.Dilettante;
                    cost = DilettanteInitCost;
                    rentalKey = new InventoryItem("ITEM_VEHICLE_KEY", "Dilettante Rental Key", rentalKeyCode);
                    break;

                case 2:
                    rentalSpawn = PillboxRentalSpawnPosition;
                    rentalRotation = PillboxRentalSpawnRotation;
                    vehicleModel = VehicleModel.Dilettante;
                    cost = DilettanteInitCost;
                    rentalKey = new InventoryItem("ITEM_VEHICLE_KEY", "Dilettante Rental Key", rentalKeyCode);
                    break;

                case 3:
                    rentalSpawn = MarinaSpawnPosition;
                    rentalRotation = MarinaSpawnRotation;
                    vehicleModel = VehicleModel.Seashark3;
                    cost = _seaSharkInitCost;
                    rentalKey = new InventoryItem("ITEM_VEHICLE_KEY", "Sea Shark Rental Key", rentalKeyCode);
                    break;
            }

            if (playerCharacter.Money < cost)
            {
                player.SendNotification($"~r~You don't have enough. ~g~{cost:C}.");
                return;
            }

            bool vehicleSpotTaken = Alt.Server.GetVehicles().Any(x => x.Position.Distance(rentalSpawn) < 4);

            if (vehicleSpotTaken)
            {
                player.SendNotification($"~r~The vehicle spot is taken. Please wait!");
                return;
            }

            if (_rentalKeys.ContainsKey(playerCharacter.Id))
            {
                player.SendNotification("~r~You already have a rental vehicle.");
                return;
            }

            if (rentalKey == null)
            {
                player.SendErrorNotification("Unable to generate a rental key!");
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            bool keyAdded = playerInventory.AddItem(rentalKey);

            if (!keyAdded)
            {
                player.SendNotification($"~r~There is not enough space to take the key!");
                return;
            }

            player.RemoveCash(cost);

            IVehicle rentalVehicle =
                Alt.CreateVehicle(vehicleModel, rentalSpawn, rentalRotation);

            rentalVehicle.ManualEngineControl = true;
            rentalVehicle.EngineOn = false;
            rentalVehicle.LockState = VehicleLockState.Locked;

            rentalVehicle.GetClass().FuelLevel = 100;
            rentalVehicle.GetClass().Distance = Convert.ToSingle(Utility.GenerateRandomNumber(6));
            rentalVehicle.SetData("RentalCar:KeyCode", rentalKeyCode);

            rentalVehicle.NumberplateText = "RENT";

            RentalVehicles.Add(playerCharacter.Id, rentalVehicle);
            _rentalKeys.Add(playerCharacter.Id, rentalKeyCode);

            player.SendNotification($"~g~Your rental car is waiting for you. Cost: {DilettanteInitCost:C}.");
            player.SetWaypoint(rentalSpawn);

            using Context context = new Context();

            Models.Character character = context.Character.Find(playerCharacter.Id);

            character.RentVehicleKey = rentalKeyCode;

            context.SaveChanges();

            bool hasWelcomeData = player.GetData(WelcomePlayer.WelcomeData, out int status);

            if (status == 1)
            {
                WelcomePlayer.OnRentVehicle(player);
                return;
            }
        }

        [Command("unrentvehicle", commandType: CommandType.Vehicle, description: "Rental: Used to unrent a vehicle.")]
        public static void VehicleRentalUnRent(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            /*if (!player.IsInVehicle)
            {
                player.SendErrorMessage("You must be in a vehicle.");
                return;
            }*/

            /*if (!RentalVehicles.ContainsValue(player.Vehicle))
            {
                player.SendPermissionError();
                return;
            }*/

            /*KeyValuePair<int, IVehicle> rentalInformation =
                RentalVehicles.FirstOrDefault(x => x.Value == player.Vehicle);

            if (rentalInformation.Key != playerCharacter.Id)
            {
                player.SendErrorMessage("You can't un-rent this vehicle!");
                return;
            }*/

            /*bool hasRentalCar = RentalVehicles.ContainsKey(playerCharacter.Id);
            if (!hasRentalCar)
            {
                player.SendErrorMessage("You don't have a rental vehicle!");
                return;
            }*/

            bool tryGetValue = RentalVehicles.TryGetValue(playerCharacter.Id, out IVehicle rentalVehicle);

            if (!tryGetValue)
            {
                player.SendErrorNotification("You don't have a rental vehicle.");
                return;
            }

            rentalVehicle.Remove();

            RentalVehicles.Remove(playerCharacter.Id);

            _rentalKeys.TryGetValue(playerCharacter.Id, out string keyCode);

            _rentalKeys.Remove(playerCharacter.Id);

            Inventory.Inventory playerInventory = player.FetchInventory();

            InventoryItem keyItem = playerInventory.GetItem("ITEM_VEHICLE_KEY", keyCode);

            if (keyItem != null)
            {
                playerInventory.RemoveItem(keyItem);
            }

            player.SendInfoNotification($"You have returned the rental vehicle.");

            using Context context = new Context();

            Models.Character character = context.Character.Find(playerCharacter.Id);

            character.RentVehicleKey = null;

            context.SaveChanges();
        }

        public static void RefundVehicleRental()
        {
            using Context context = new Context();

            Console.WriteLine($"Refunding Rental Key Prices");

            List<Models.Character> players =
                context.Character.Where(x => !string.IsNullOrEmpty(x.RentVehicleKey)).ToList();

            foreach (Models.Character character in players)
            {
                string keyValue = character.RentVehicleKey;

                character.Money += 50;
                character.RentVehicleKey = null;

                InventoryData playerInventoryData = context.Inventory.Find(character.InventoryID);

                if (playerInventoryData == null) continue;

                List<InventoryItem> items =
                    JsonConvert.DeserializeObject<List<InventoryItem>>(playerInventoryData.Items);

                InventoryItem oldKey = items.FirstOrDefault(x => x.ItemValue == keyValue);

                if (oldKey == null) continue;

                items.Remove(oldKey);

                playerInventoryData.Items = JsonConvert.SerializeObject(items);
            }

            context.SaveChanges();

            Console.WriteLine($"Refunded {players.Count} characters");
        }

        [Command("findrental", commandType: CommandType.Vehicle,
            description: "Rental: Used to find your current rental vehicle")]
        public static void RentalCommandFindRental(IPlayer player)
        {
            bool tryGetValue = RentalVehicles.TryGetValue(player.GetClass().CharacterId, out IVehicle rentalVehicle);

            if (!tryGetValue || rentalVehicle == null)
            {
                player.SendErrorNotification("You don't have a rental vehicle.");
                return;
            }

            player.SendNotification($"~y~A waypoint has been set to the cars position.");

            player.SetWaypoint(rentalVehicle.Position);
        }
    }
}