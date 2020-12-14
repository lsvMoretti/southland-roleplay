using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Bank
{
    public class AtmCommands
    {
        [Command("atm", commandType: CommandType.Bank, description: "Shows the ATM system when near an ATM")]
        public static void CommandAtm(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (player.IsInVehicle)
            {
                player.SendErrorNotification("You must not be in a vehicle.");
                return;
            }

            player.Emit("atAtm");
        }
    }
}