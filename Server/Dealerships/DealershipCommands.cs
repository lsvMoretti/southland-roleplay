using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Dealerships
{
    public class DealershipCommands
    {
        [Command("vbuy", commandType: CommandType.Vehicle, description: "Shows the selection of vehicles at the dealership")]
        public static void DealershipCommandVBuy(IPlayer player)
        {
            if (player?.FetchCharacter() == null) return;

            Dealership nearestDealership = Dealership.FetchDealerships()
                .FirstOrDefault(x => new Position(x.PosX, x.PosY, x.PosZ).Distance(player.Position) <= 8f);

            if (nearestDealership == null)
            {
                player.SendErrorNotification("You are not near a dealership!");
                return;
            }

            player.FreezeCam(true);
            player.FreezeInput(true);
            player.ShowCursor(true);

            List<DealershipVehicle> dealershipVehicles =
                JsonConvert.DeserializeObject<List<DealershipVehicle>>(nearestDealership.VehicleList);

            IOrderedEnumerable<DealershipVehicle> orderedList = dealershipVehicles.OrderByDescending(x => x.VehName);

            string jsonString = JsonConvert.SerializeObject(orderedList);

            player.SetData("ATDEALERSHIP", nearestDealership.Id);
            player.SetData("DEALERSHIPVEHICLELIST", jsonString);

            bool hasVoucher = player.FetchCharacter().VehicleVoucher;

            player.Emit("showDealershipCars", jsonString, hasVoucher);
        }
    }
}