using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Elasticsearch.Net.Specification.LicenseApi;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Models;
using Blip = Server.Objects.Blip;

namespace Server.Property
{
    public class RealEstateOffice
    {
        public static Position ExteriorPosition = new Position(100.93187f, -1115.7495f, 29.313599f);

        public static void LoadRealEstateOffice()
        {
            Blip realEstateBlip = new Blip("Wolfs International Realty", ExteriorPosition, 40, 17, 1f);
            TextLabel realEstateLabel = new TextLabel("Real Estate Office\nUsage: /realestate", ExteriorPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.OrangeRed));

            realEstateLabel.Add();
            realEstateBlip.Add();
        }

        [Command("realestate")]
        public static void CommandRealEstate(IPlayer player)
        {
            if (player?.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            }

            if (player.Position.Distance(ExteriorPosition) > 3)
            {
                player.SendErrorNotification("You aren't near the Real Estate Office!");
                return;
            }

            List<Models.Property> buyableProperties =
                Models.Property.FetchProperties();

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (Models.Property property in buyableProperties)
            {
                if (property.PropertyType != PropertyType.House) continue;
                if (property.OwnerId != 0) continue;
                if (property.Value <= 1) continue;

                menuItems.Add(new NativeMenuItem(property.Address, property.Value.ToString("C0")));
            }

            NativeMenu menu = new NativeMenu("RealEstateSelectProperty", "Wolfs", "Viewing Ownable Property", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void EventRealEstateSelectProperty(IPlayer player, string option)
        {
            if (option == "Close") return;

            Models.Property selectedProperty =
                Models.Property.FetchProperties().FirstOrDefault(x => x.Address == option);

            if (selectedProperty == null)
            {
                player.SendErrorNotification("An error occurred fetching the address.");
                return;
            }

            player.SetWaypoint(new Position(selectedProperty.PosX, selectedProperty.PosY, selectedProperty.PosZ));

            player.SendInfoNotification($"Waypoint has been set to {selectedProperty.Address}.");
        }
    }
}