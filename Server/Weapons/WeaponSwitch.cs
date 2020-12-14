using System;
using System.Linq;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Server.Extensions;

namespace Server.Weapons
{
    public class WeaponSwitch
    {
        private static Timer tickTimer = null;

        public static void InitTick()
        {
            tickTimer = new Timer(1) { Enabled = true, AutoReset = true };

            tickTimer.Elapsed += (sender, args) =>
            {
                tickTimer.Stop();
                foreach (IPlayer player in Alt.Server.GetPlayers())
                {
                    if (player == null) continue;
                    bool hasLastData = player.GetData("tick:lastWeapon", out uint lastWeapon);

                    if (hasLastData && lastWeapon != player.CurrentWeapon)
                    {
                        player.SetData("tick:lastWeapon", player.CurrentWeapon);

                        OnWeaponSwitch(player, lastWeapon, player.CurrentWeapon);
                    }
                }
                tickTimer.Start();
            };
        }

        private static void OnWeaponSwitch(IPlayer player, uint oldWeapon, uint newWeapon)
        {
            if (newWeapon == (uint)WeaponModel.Fist)
            {
                bool hasWeaponHashData = player.GetData("CurrentWeaponHash", out uint currentWeaponHash);

                if (hasWeaponHashData)
                {
                    if (oldWeapon == currentWeaponHash)
                    {
                        player.GiveWeapon(currentWeaponHash, 0, false);
                    }
                }
            }
        }

        public static void OnLeftMouseButton(IPlayer player)
        {
        }
    }
}