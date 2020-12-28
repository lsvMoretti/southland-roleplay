using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.DMV
{
    public class DmvCommands
    {
        private static readonly double _renewDrivingLicense = 10;
        private static readonly double _newPlateCost = 100;

        [Command("dmv", commandType: CommandType.Character, description: "Shows the DMV menu")]
        public static void CommandDmv(IPlayer player)
        {
            if (!player.GetClass().Spawned)
            {
                player.SendLoginError();
                return;
            }

            Position playerPosition = player.Position;

            if (playerPosition.Distance(DmvHandler.DmvPosition) >= 2f)
            {
                player.SendErrorNotification("You're not near the DMV!");
                return;
            }

            List<LicenseTypes> licenses = JsonConvert.DeserializeObject<List<LicenseTypes>>(player.FetchCharacter().LicensesHeld);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (!licenses.Contains(LicenseTypes.Driving))
            {
                menuItems.Add(new NativeMenuItem("Take Driving Test", "Take the test to obtain the drivers license."));
            }
            else if (licenses.Contains(LicenseTypes.Driving))
            {
                menuItems.Add(new NativeMenuItem("Request New License", $"Requests a new drivers license from the DMV. Costs {_renewDrivingLicense:C}."));
            }

            menuItems.Add(new NativeMenuItem("Request New Vehicle Plate", $"Requests a new vehicle plate. Costs {_newPlateCost:C}"));

            NativeMenu menu = new NativeMenu("DMVMainMenuSelect", "DMV", "Select an option.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static async void OnDmvMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Take Driving Test")
            {
                if (player.FetchCharacter().Money < 50)
                {
                    player.SendErrorNotification("You need $50 to take the test.");
                    return;
                }

                DmvHandler.StartDrivingTest(player);
                return;
            }

            if (option == "Request New License")
            {
                if (player.GetClass().Cash < _renewDrivingLicense)
                {
                    player.SendErrorNotification($"You don't have enough funds. You require {_renewDrivingLicense:C}.");
                    return;
                }

                Inventory.Inventory playerInventory = player.FetchInventory();

                bool added = playerInventory.AddItem(new InventoryItem("ITEM_DRIVING_LICENSE", "Drivers License", player.GetClass().Name));

                if (!added)
                {
                    player.SendErrorNotification($"You don't have space to receive your license.");
                    return;
                }

                player.GetClass().Cash -= (float)_renewDrivingLicense;

                player.SendInfoNotification($"You've been given a new copy of your drivers license. This has cost you {_renewDrivingLicense:C}.");
            }

            if (option == "Request New Vehicle Plate")
            {
                await using Context context = new Context();

                Models.Character playerCharacter = player.FetchCharacter();

                if (playerCharacter == null)
                {
                    player.SendErrorNotification("Unable to find your account data!");
                    return;
                }

                IQueryable<Models.Vehicle> ownedVehicles = context.Vehicle.Where(x => x.OwnerId == playerCharacter.Id);

                if (!ownedVehicles.Any())
                {
                    player.SendErrorNotification("You don't own any vehicles!");
                    return;
                }

                List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

                foreach (Models.Vehicle ownedVehicle in ownedVehicles)
                {
                    menuItems.Add(new NativeMenuItem(ownedVehicle.Model, $"Current Plate: {ownedVehicle.Plate}"));
                }

                NativeMenu menu = new NativeMenu("DMV:Menu:OnNewPlateSelect", "Select a vehicle to get a new plate", $"This costs ~g~{_newPlateCost:C}", menuItems);

                NativeUi.ShowNativeMenu(player, menu, true);
            }
        }

        public static async void OnNewPlateSelect(IPlayer player, string option, int selectedIndex)
        {
            if (option == "Close") return;

            await using Context context = new Context();

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendErrorNotification("Unable to find your account data!");
                return;
            }

            List<Models.Vehicle> ownedVehicles = context.Vehicle.Where(x => x.OwnerId == playerCharacter.Id).ToList();

            if (!ownedVehicles.Any())
            {
                player.SendErrorNotification("You don't own any vehicles!");
                return;
            }

            Models.Vehicle selectedVehicle = ownedVehicles[selectedIndex];

            if (selectedVehicle == null)
            {
                player.SendErrorNotification("Unable to find this vehicle!");
                return;
            }

            string newPlate = null;

            for (int i = 0; i < 100; i++)
            {
                newPlate = Utility.GenerateRandomString(8);

                if (await context.Vehicle.AnyAsync(x => x.Plate == newPlate) == false) break;
            }

            if (newPlate is null)
            {
                player.SendErrorNotification("There was an error generating a new plate.");
                return;
            }

            if (player.GetClass().Cash < _newPlateCost)
            {
                player.SendErrorNotification($"You require {_newPlateCost:C} to purchase a plate.");
                return;
            }

            Models.Vehicle vehicle = await context.Vehicle.FirstOrDefaultAsync(x => x.Id == selectedVehicle.Id);

            if (vehicle is null)
            {
                player.SendErrorNotification("Unable to find this vehicle!");
                return;
            }

            player.RemoveCash(_newPlateCost);

            vehicle.HasPlateBeenStolen = false;
            vehicle.Plate = newPlate;

            await context.SaveChangesAsync();

            player.SendInfoMessage($"You've purchased a new plate for your {vehicle.Name}. New Plate: {newPlate}");

            IVehicle vehicleEntity = Alt.Server.GetVehicles().FirstOrDefault(x => x.GetClass().Id == vehicle.Id);

            if (vehicleEntity is null) return;

            vehicleEntity.NumberplateText = newPlate;
        }
    }
}