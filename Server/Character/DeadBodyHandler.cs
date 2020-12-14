using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Character
{
    public class DeadBodyHandler
    {
        public static void LoadDeadBodyForAll(DeadBody deadBody)
        {
            Alt.EmitAllClients("SendDeadBody", JsonConvert.SerializeObject(deadBody));
        }

        public static void LoadBodyForPlayer(IPlayer player, DeadBody body)
        {
            player.Emit("SendDeadBody", JsonConvert.SerializeObject(body));
        }

        public static void RemoveDeadBodyForAll(DeadBody body)
        {
            Alt.EmitAllClients("RemoveDeadBody", JsonConvert.SerializeObject(body));
        }
    }
}