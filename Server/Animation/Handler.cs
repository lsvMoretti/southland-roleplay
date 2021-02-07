using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Extensions;

namespace Server.Animation
{
    public class Handler
    {
        public static void PlayScenario(IPlayer player, string scenario)
        {
            player.SetInAnimation(true);
            player.Emit("animation:StartScenario", scenario);
        }

        /// <summary>
        /// Plays an Animation for a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="flag"></param>
        /// <param name="dict"></param>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        public static void PlayPlayerAnimationEx(IPlayer player, int flag, string dict, string name, int duration = -1)
        {
            player.SetInAnimation(true);
            player.Emit("animation:StartAnimation", dict, name, duration, flag);
        }

        /// <summary>
        /// Stops an animation for a player
        /// </summary>
        /// <param name="player"></param>
        public static void StopPlayerAnimation(IPlayer player)
        {
            player.SetInAnimation(false);
            player.Emit("animation:StopAnimation");
        }

        /// <summary>
        /// Checks if they are able to Anim
        /// </summary>
        /// <param name="player"></param>
        /// <param name="includeVehicle">If true, checks if in a vehicle too</param>
        /// <returns></returns>
        public static bool CanAnim(IPlayer player, bool includeVehicle = false)
        {
            if (!player.IsSpawned())
            {
                player.SendErrorNotification("You're not spawned.");
                return false;
            }

            if (player.GetClass().Cuffed) return false;

            if (player.IsInAnimation())
            {
                //player.SendErrorMessage("You're in an animation already. Use /stopanim");
                //return false;
                StopPlayerAnimation(player);
            }

            if (includeVehicle && player.IsInVehicle)
            {
                player.SendErrorNotification("You can't do this whilst inside a vehicle.");
                return false;
            }

            return true;
        }
    }
}