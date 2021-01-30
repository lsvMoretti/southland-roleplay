using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Extensions.Marker
{
    public class MarkerHandler
    {
        public static List<Marker> MarkerLabels = new List<Marker>();

        public static void LoadMarkersOnSpawn(IPlayer player)
        {
            foreach (Marker marker in MarkerLabels)
            {
                LoadMarkerForPlayer(player, marker);
            }
        }

        public static void LoadMarkerForPlayer(IPlayer player, Marker marker)
        {
            player.Emit("createMarker", JsonConvert.SerializeObject(marker));
        }

        public static void RemoveMarkerForPlayer(IPlayer player, Marker marker)
        {
            player.Emit("deleteMarker", JsonConvert.SerializeObject(marker));
        }

        public static void RemoveAllMarkersForPlayer(IPlayer player)
        {
            player.Emit("deleteAllMarkers");
        }

        public static void OnMarkerAdded(Marker marker)
        {
            foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null))
            {
                LoadMarkerForPlayer(player, marker);
            }
        }

        public static void OnMarkerRemoved(Marker marker)
        {
            foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null))
            {
                RemoveMarkerForPlayer(player, marker);
            }
        }
    }
}