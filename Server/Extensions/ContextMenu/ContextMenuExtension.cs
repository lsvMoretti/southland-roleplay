using System;
using System.Collections.Generic;
using System.Drawing;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Property;

namespace Server.Extensions
{
    public class ContextMenuExtension
    {
        /// <summary>
        /// The Event Handler for Context Menus
        /// </summary>
        /// <param name="player"></param>
        /// <param name="eventTrigger"></param>
        /// <param name="selectedItem"></param>
        public static void OnContextMenuClickEvent(IPlayer player, string eventTrigger, string selectedItem)
        {
            player.Emit("closeContextMenu");

            player.GetData("ContextMenuFreezePos", out bool positionFrozen);

            if (positionFrozen)
            {
                player.SetData("ContextMenuFreezePos", false);
                player.FreezePlayer(false);
            }

            if (eventTrigger == "ShowPlayerPropertyEnterMenu")
            {
                PropertyEntrances.EnterPropertyContextMenu(player, selectedItem);
                return;
            }

            if (eventTrigger == "ShowPlayerPropertyLeaveMenu")
            {
                PropertyEntrances.ExitPropertyContextMenu(player, selectedItem);
            }

            if (eventTrigger == "OnVehicleClickMenu")
            {
                MouseMenuExtension.OnVehicleClickMenu(player, selectedItem);
            }
        }
    }
}