using System.Collections.Generic;
using System.Drawing;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Extensions
{
    public class ContextMenu
    {
        /// <summary>
        /// The event trigger that is called back to the server
        /// </summary>
        public string EventTrigger { get; set; }

        /// <summary>
        /// World Position of the Menu
        /// </summary>
        public Position Pos { get; set; }

        /// <summary>
        /// List of Menu Items
        /// </summary>
        public List<string> MenuItems { get; set; }

        /// <summary>
        /// Height of each MenuItem
        /// </summary>
        public float ItemHeight { get; set; }

        /// <summary>
        /// Width of each MenuItem
        /// </summary>
        public float ItemWidth { get; set; }

        /// <summary>
        /// Background Color of Menu
        /// </summary>
        public LsvColor BackgroundColor { get; set; }

        /// <summary>
        /// Text Color on Menu
        /// </summary>
        public LsvColor TextColor { get; set; }

        /// <summary>
        /// Freezes cam & player position
        /// </summary>
        public bool FreezePosition { get; set; }

        /// <summary>
        /// Returns a new Context Menu
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <param name="position"></param>
        /// <param name="menuItems"></param>
        /// <param name="textColor"></param>
        /// <param name="itemHeight"></param>
        /// <param name="itemWidth"></param>
        /// <param name="freezePosition"></param>
        /// <param name="backgroundColor"></param>
        public ContextMenu(string eventTrigger, Position position, List<string> menuItems, bool freezePosition = true, Color backgroundColor = new Color(), Color textColor = new Color(), float itemHeight = 0.06f, float itemWidth = 0.06f)
        {
            EventTrigger = eventTrigger;
            Pos = position;
            MenuItems = menuItems;
            ItemHeight = itemHeight;
            ItemWidth = itemWidth;
            if (backgroundColor == new Color())
            {
                BackgroundColor = new LsvColor(0, 129, 253);
            }
            else if (backgroundColor != new Color())
            {
                BackgroundColor = new LsvColor(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A);
            }
            if (textColor == new Color())
            {
                TextColor = new LsvColor(255, 255, 255);
            }
            else if (textColor != new Color())
            {
                TextColor = new LsvColor(textColor.R, textColor.G, textColor.B, textColor.A);
            }

            FreezePosition = freezePosition;
        }

        public static void ShowContextMenu(IPlayer player, ContextMenu contextMenu)
        {
            if (contextMenu.FreezePosition)
            {
                player.SetData("ContextMenuFreezePos", true);
                player.FreezePlayer(true);
            }
            player.Emit("createContextMenu", JsonConvert.SerializeObject(contextMenu, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }
    }
}