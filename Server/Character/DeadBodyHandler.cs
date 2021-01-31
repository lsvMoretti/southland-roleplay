using System;
using System.Collections.Generic;
using System.Timers;
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

        public static void InitDeadBodyClearing()
        {
            Timer timer = new Timer(60000)
            {
                AutoReset = true
            };

            timer.Start();

            timer.Elapsed += (sender, args) =>
            {
                timer.Stop();

                Dictionary<int, DeadBody> deadBodies = DeathHandler.DeadBodies;

                foreach (KeyValuePair<int, DeadBody> keyValuePair in deadBodies)
                {
                    DateTime deathTime = keyValuePair.Value.TimeOfDeath;

                    if (DateTime.Compare(deathTime.AddMinutes(15), DateTime.Now) > 0)
                    {
                        DeathHandler.DeadBodies.Remove(keyValuePair.Key);
                        RemoveDeadBodyForAll(keyValuePair.Value);
                    }
                }

                timer.Start();
            };
        }
    }
}