using System.Collections.Generic;
using System.Net.Http;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
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

            NativeMenu menu = new NativeMenu("DMVMainMenuSelect", "DMV", "Select an option.", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnDmvMainMenuSelect(IPlayer player, string option)
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

                var playerInventory = player.FetchInventory();

                bool added = playerInventory.AddItem(new InventoryItem("ITEM_DRIVING_LICENSE", "Drivers License", player.GetClass().Name));

                if (!added)
                {
                    player.SendErrorNotification($"You don't have space to receive your license.");
                    return;
                }

                player.GetClass().Cash -= (float)_renewDrivingLicense;

                player.SendInfoNotification($"You've been given a new copy of your drivers license. This has cost you {_renewDrivingLicense:C}.");

            }
        }
    }
}