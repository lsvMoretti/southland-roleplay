using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Extensions.Blip
{
    public class BlipHandler
    {
        public static List<Objects.Blip> BlipList = new List<Objects.Blip>();

        public static void LoadBlipsOnSpawn(IPlayer player)
        {
            foreach (Objects.Blip blip in BlipList)
            {
                LoadBlipForPlayer(player, blip);
            }

            List<Models.Property> characterProperties =
                Models.Property.FetchCharacterProperties(player.FetchCharacter());

            if (!characterProperties.Any()) return;

            foreach (Models.Property characterProperty in characterProperties)
            {
                Objects.Blip newBlip = new Objects.Blip(characterProperty.Address, characterProperty.FetchExteriorPosition(), 40, 2, 0.75f);
                LoadBlipForPlayer(player, newBlip);
            }
        }

        public static void LoadBlipForPlayer(IPlayer player, Objects.Blip blip)
        {
            player.Emit("createLocalBlip", JsonConvert.SerializeObject(blip));
        }

        public static void RemoveBlipForPlayer(IPlayer player, Objects.Blip blip)
        {
            player.Emit("deleteLocalBlip", blip.UniqueId);
        }

        public static void OnBlipAdded(Objects.Blip blip)
        {
            foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null))
            {
                LoadBlipForPlayer(player, blip);
            }
        }

        public static void OnBlipRemoved(Objects.Blip blip)
        {
            foreach (IPlayer player in Alt.Server.GetPlayers().Where(x => x.FetchCharacter() != null))
            {
                RemoveAllBlipsForPlayer(player);
                LoadBlipsOnSpawn(player);
            }
        }

        public static void RemoveAllBlipsForPlayer(IPlayer player)
        {
            player.Emit("deleteAllBlips");
        }
    }
}