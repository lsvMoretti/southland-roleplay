using System;
using System.Collections.Generic;
using System.Drawing;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Extensions
{
    public class NativeMenu
    {
        /// <summary>
        /// Creates a new NativeMenu object
        /// </summary>
        /// <param name="serverTrigger">The trigger to be called back on the Action</param>
        /// <param name="title">Title of the menu</param>
        /// <param name="description">Description of the Menu</param>
        /// <param name="menuItems">List of menu items</param>
        public NativeMenu(string serverTrigger, string title, string description = "", List<NativeMenuItem> menuItems = null)
        {
            ServerTrigger = serverTrigger;
            Title = title;
            SubTitle = description;
            MenuItems = menuItems;
        }

        /// <summary>
        /// Menu Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Menu Sub Title
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// List of "standard" menu items
        /// </summary>
        public List<NativeMenuItem> MenuItems { get; set; }

        /// <summary>
        /// List of "list" menu items
        /// </summary>
        public List<NativeListItem> ListMenuItems { get; set; }

        /// <summary>
        /// List of "checkbox" menu items
        /// </summary>
        public List<NativeCheckItem> CheckedMenuItems { get; set; }

        /// <summary>
        /// List of "colorbox" menu items
        /// </summary>
        public List<ColorBoxItem> NativeColorItems { get; set; }

        /// <summary>
        /// Event name to return back Server Side
        /// </summary>
        public string ServerTrigger { get; set; }

        /// <summary>
        /// Event Name to return back on List Change
        /// </summary>
        public string ListTrigger { get; set; }

        /// <summary>
        /// Item Change Trigger
        /// </summary>
        public string ItemChangeTrigger { get; set; }

        /// <summary>
        /// Pass Index with RemoteEvent & Index Change return
        /// </summary>
        public bool PassIndex { get; set; }

        [Obsolete("To be implemented - Moretti", true)]
        /// <summary>
        /// Disables going into FPP if in a long menu
        /// </summary>
        public bool DisableFPP { get; set; }
    }

    public class NativeSubMenu
    {
        public string Title { get; set; }

        /// <summary>
        /// Sub Menus, Title & Description
        /// </summary>
        public Dictionary<string, string> SubMenus { get; set; }
    }

    public class NativeMenuItem
    {
        /// <summary>
        /// Creates a new Native Menu Item object
        /// </summary>
        /// <param name="Title">Title (Left side)</param>
        /// <param name="Description">Description (Bottom of Menu)</param>
        public NativeMenuItem(string title, string description = "")
        {
            Title = title;
            Description = description;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public NativeMenuBadge LeftBadge { get; set; }
        public NativeMenuBadge RightBadge { get; set; }
    }

    public class NativeListItem
    {
        public string Title { get; set; }
        public List<string> StringList { get; set; }

        public NativeListItem(string title, List<string> stringList)
        {
            Title = title;
            StringList = stringList;
        }

        public NativeListItem()
        {
        }
    }

    public class NativeCheckItem
    {
        /// <summary>
        /// Creates a new Check Box Menu Item
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="check"></param>
        public NativeCheckItem(string title, string description = "", bool check = false)
        {
            Title = title;
            Description = description;
            Checked = check;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool Checked { get; set; }
    }

    public class ColorBoxItem
    {
        /// <summary>
        /// Creates a new Color Box Menu Item
        /// </summary>
        /// <param name="title"></param>
        /// <param name="color"></param>
        /// <param name="highlightColor"></param>
        /// <param name="description"></param>
        public ColorBoxItem(string title, LsvColor color, LsvColor highlightColor, string description = "")
        {
            Title = title;
            Description = description;
            ColorR = color.R;
            ColorG = color.G;
            ColorB = color.B;
            ColorA = color.A;
            HighlightColorR = highlightColor.R;
            HighlightColorG = highlightColor.G;
            HighlightColorB = highlightColor.B;
            HighlightColorA = highlightColor.A;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public byte ColorR { get; set; }
        public byte ColorG { get; set; }
        public byte ColorB { get; set; }
        public byte ColorA { get; set; }
        public byte HighlightColorR { get; set; }
        public byte HighlightColorG { get; set; }
        public byte HighlightColorB { get; set; }
        public byte HighlightColorA { get; set; }
    }

    public enum NativeMenuBadge
    {
        None, Alert, Ammo, Armour, Bike, BronzeMedal, Car, Clothes, Crown, Franklin, GoldMedal, Gun, Heart, Lock, Makeup, Mask, Michael, SilverMedal, Star, Tatoo, Tick, Trevor
    }
}