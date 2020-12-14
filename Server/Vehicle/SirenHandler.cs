using System.Net.Http.Headers;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Vehicle
{
    public class SirenHandler
    {
        /// <summary>
        /// Called when a player presses the Horn Key
        /// </summary>
        /// <param name="player"></param>
        /// <param name="vehicle"></param>
        public static void OnHornPress(IPlayer player, IVehicle vehicle)
        {
            Alt.EmitAllClients("newSirenHandler:HornActive", vehicle);
        }

        public static void OnHornRelease(IPlayer player, IVehicle vehicle)
        {
            Alt.EmitAllClients("newSirenHandler:HornRelease", vehicle);
        }
    }
}