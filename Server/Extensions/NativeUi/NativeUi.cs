using System;
using System.Collections.Generic;
using System.Drawing;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Extensions
{
    public class NativeUi
    {
        /// <summary>
        /// Shows a NativeMenu to a player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="nativeMenu"></param>
        /// <param name="addClose">"Close" option added</param>
        public static void ShowNativeMenu(IPlayer player, NativeMenu nativeMenu, bool addClose = false)
        {
            if (player.IsDead || player.GetClass().Downed)
            {
                player.SendNotification("~r~Error: You are downed.", true);
                return;
            }

            if (addClose)
            {
                ColorBoxItem closeItem = new ColorBoxItem("Close", new LsvColor(Color.DarkRed), new LsvColor(Color.Red));

                if (nativeMenu.NativeColorItems == null)
                {
                    nativeMenu.NativeColorItems = new List<ColorBoxItem>
                    {
                        closeItem
                    };
                }
                else
                {
                    nativeMenu.NativeColorItems.Add(closeItem);
                }
            }

            player.ChatInput(false);
            player.HideChat(true);

            string json = JsonConvert.SerializeObject(nativeMenu, Formatting.None);

            player.Emit("CreateNativeMenu", json);
        }

        /// <summary>
        /// Creates a User Input (NativeUi type) and returns on serverTrigger
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serverTrigger"></param>
        /// <param name="defaultText"></param>
        /// <param name="defaultLength"></param>
        public static void GetUserInput(IPlayer player, string serverTrigger, string defaultText = "",
            int defaultLength = 120)
        {
            player.Emit("getUserInput", serverTrigger, defaultText, defaultLength);
        }

        /// <summary>
        /// Shows a Yes/No option with no Close Option
        /// </summary>
        /// <param name="player"></param>
        /// <param name="serverTrigger"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        public static void ShowYesNoMenu(IPlayer player, string serverTrigger, string title, string description = "")
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Yes"),
                new NativeMenuItem("No")
            };

            NativeMenu menu = new NativeMenu(serverTrigger, title, description, menuItems);

            NativeUi.ShowNativeMenu(player, menu);
        }
    }
}