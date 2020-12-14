using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Audio
{
    public class AudioCommands
    {
        [Command("station", alternatives: "radio,xmr")]
        public static void ShowAudioStations(IPlayer player)
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You're not in a vehicle.");
                return;
            }

            if (!player.Vehicle.FetchVehicleData().DigitalRadio)
            {
                player.SendErrorNotification("This vehicle doesn't have a digital radio fitted.");
                return;
            }

            if (player.Seat != 1)
            {
                if (player.Seat != 2)
                {
                    // Not driver or front passenger
                    player.SendErrorNotification("You're not in the front!");
                    return;
                }
            }

            AudioHandler.LoadStreamPage(player);
        }
    }
}