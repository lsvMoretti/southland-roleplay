using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Extensions;

namespace Server.Objects
{
    public class OnlinePlayer
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public uint Ping { get; set; }

        public OnlinePlayer(IPlayer player, int id = -1)
        {
            Name = player.GetClass().Name;
            Id = id == -1 ? player.GetPlayerId() : id;
            Ping = player.Ping;
        }

        /// <summary>
        /// Fetches a list of Online Players for the UI
        /// </summary>
        /// <returns></returns>
        public static List<OnlinePlayer> FetchOnlinePlayers()
        {
            List<OnlinePlayer> onlinePlayers = new List<OnlinePlayer>();

            foreach (IPlayer player in Alt.GetAllPlayers().Where(x => x.FetchCharacter() != null).ToList())
            {
                if (player.GetClass().AdminDuty)
                {
                    onlinePlayers.Add(new OnlinePlayer(player, 0));
                }
                else
                {
                    onlinePlayers.Add(new OnlinePlayer(player));
                }
            }

            return onlinePlayers.OrderByDescending(x => x.Id).ToList();
        }
    }
}