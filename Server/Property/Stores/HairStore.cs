using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Character;
using Server.Character.Tattoo;
using Server.Chat;
using Server.Extensions;

namespace Server.Property.Stores
{
    public class HairStore
    {
        #region Definitions

        /// <summary>
        /// List of Male hair Names
        /// </summary>
        private static string[] _maleHairNameList = new string[]
        {
            "Close Shave", "Buzzcut", "Faux Hawk", "Hipster", "Side Parting", "Shorter Cut", "Biker", "Ponytail",
            "Cornrows", "Slicked", "Short Brushed", "Spikey", "Caesar", "Chopped",
            "Dreads", "Long Hair", "Shaggy Curls", "Surfer Dude", "Short Side Part", "High Slicked Sides",
            "Long Slicked", "Hipster Youth", "Mullet", "Classic Cornrows", "Palm Cornrows", "Lightning Cornrows",
            "Whipped Cornrows", "Zig Zag Cornrows", "Snail Cornrows", "Hightop", "Loose Swept Back",
            "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk", "Mod", "Layered Mod", "Flattop",
            "Military Buzzcut"
        };

        /// <summary>
        /// List of Male Hair ID's
        /// </summary>
        private static int[] _maleHairIntList = new int[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 24, 25, 26, 27, 28, 29,
            30, 31, 32, 33, 34, 35, 36, 72, 73
        };

        /// <summary>
        /// List of Female Hair Names
        /// </summary>
        private static string[] _femaleHairNameList = new string[]
        {
            "Close Shave", "Short", "Layered Bob", "Pigtails", "Ponytail", "Braided Mohawk", "Braids", "Bob", "Faux Hawk", "French Twist",
            "Long Bob", "Loose Tied", "Pixie", "Shaved Bangs", "Top Knot", "Wavy Bob", "Messy Bun", "Pin Up Girl", "Tight Bun", "Twisted Bob",
            "Flapper Bob", "Big Bangs", "Braided Top Knot", "Mullet", "Pinched Cornrows", "Leaf Cornrows", "Zig Zag Cornrows", "Pigtail Bangs",
            "Wave Braids", "Coil Braids", "Rolled Quiff", "Loose Swept Back", "Undercut Swept Back", "Undercut Swept Side", "Spiked Mohawk",
            "Bandana and Braid", "Layered Mod", "Skinbyrd", "Neat Bun", "Right Side Braid", "Short Right Part", "Long back Middle Part", "Long Middle Part",
            "Short Left Part", "Medium Straight", "Bob Bandana", "Medium Left Beach Wave", "Left High Pony", "Long Dreads", "Medium Smooth", "Long Braid Crown",
            "Loose Bun", "Looped Pigtails Bangs", "Medium one shoulder", "Long Mermaid Bangs", "Dreaded High Pony", "Medium Right Waves", "High Bun bangs",
            "Short Afro", "Short Curly Bands", "Thick Right Braid", "Long Left Deep Part", "Short Volume Bob", "Top Bun Wispy Bangs", "Long Straight Middle Part",
            "Long Straight Middle Part Bangs", "Medium Waves Middle Part", "High Pony Bangs", "High Pony Highlights", "Short Wide Right Part", "Long Super Straight",
            "Up do Sweep Across", "Long back layers", "Dreaded Bob", "Long back Highlights", "Left Braid", "Tight Bun Side Swept", "Cute Pixie"
        };

        /// <summary>
        /// List of Female Hair ID's
        /// </summary>
        private static int[] _femaleHairIntList = new int[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 25, 26, 27, 28, 29, 30, 31,
            32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60,
            61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77
        };

        private static string[] _facialHair = new string[] {"None", "Light Stubble", "Balbo", "Circle Beard", "Goatee", "Chin", "Chin Fuzz",
            "Pencil Chin Strap", "Scruffy", "Musketeer", "Mustache", "Trimmed Beard", "Stubble", "Thin Circle Beard",
            "Horseshoe", "Pencil and 'Chops", "Chin Strap Beard", "Balbo and Sideburns", "Mutton Chops", "Scruffy Beard",
            "Curly", "Curly & Deep Stranger", "Handlebar", "Faustic", "Otto & Patch", "Otto & Full Stranger", "Light Franz",
            "The Hampstead", "The Ambrose", "Lincoln Curtain"};

        private static readonly double hairCost = 10.5;
        private static readonly double facialHairCost = 6.5;

        #endregion Definitions

        public static void LoadHairStoreMenu(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Hair"),
                new NativeMenuItem("Hair Color"),
                new NativeMenuItem("Hair Highlight Color"),
                new NativeMenuItem("Eyebrow Color"),
                new NativeMenuItem("Chest Hair Color")
            };

            if (player.GetClass().IsMale)
            {
                menuItems.Add(new NativeMenuItem("Facial Hair"));
                menuItems.Add(new NativeMenuItem("Facial Hair Color"));
            }

            NativeMenu menu = new NativeMenu("store:HairStore:MainMenuSelect", "Hair Store", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnMainMenuSelect(IPlayer player, string option)
        {
            if (option == "Close") return;

            if (option == "Hair")
            {
                ShowHairMenu(player);
                return;
            }

            if (option == "Facial Hair")
            {
                ShowFacialHairMenu(player);
                return;
            }

            // Hair Colors

            if (option == "Hair Color")
            {
                ShowHairColorMenu(player, 0);
                return;
            }

            if (option == "Hair Highlight Color")
            {
                ShowHairColorMenu(player, 1);
                return;
            }

            if (option == "Facial Hair Color")
            {
                ShowHairColorMenu(player, 2);
            }
            if (option == "Eyebrow Color")
            {
                ShowHairColorMenu(player, 3);
            }
            if (option == "Chest Hair Color")
            {
                ShowHairColorMenu(player, 4);
            }
        }

        /// <summary>
        /// Shows the Hair Color menu
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type">0 Hair, 1 Hair Highlight, 2 Facial Hair, 3 Eyebrow Color, 4 Chest Hair Color</param>
        public static void ShowHairColorMenu(IPlayer player, int type)
        {
            // 0 -64 hair colors

            List<string> colorList = new List<string>();

            for (int i = 0; i <= 64; i++)
            {
                colorList.Add(i.ToString());
            }

            string hairName;

            switch (type)
            {
                case 0:
                    hairName = "Hair Color";
                    break;

                case 1:
                    hairName = "Hair Highlight";
                    break;

                case 2:
                    hairName = "Facial Hair Color";
                    break;

                case 3:
                    hairName = "Eyebrow Color";
                    break;

                case 4:
                    hairName = "Chest Hair Color";
                    break;

                default:
                    player.SendErrorNotification("Incorrect selection.");
                    return;
            }

            List<NativeListItem> listItems = new List<NativeListItem>
            {
                new NativeListItem(hairName, colorList)
            };

            NativeMenu menu = new NativeMenu("HairStore:ColorSelection", "Hair Color - $5", "Select a Hair Color")
            {
                ListMenuItems = listItems,
                ListTrigger = "HairStore:OnHairColorListChange"
            };

            CustomCharacter customCharacter =
                JsonConvert.DeserializeObject<CustomCharacter>(player.FetchCharacter().CustomCharacter);

            HairInfo hairInfo = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

            player.Emit("HairStore:HighlightColor", hairInfo.HighlightColor, hairInfo.Color);

            player.SetData("HairStore:ColorIndex", 0);
            player.SetData("HairStore:AdjustHairIndex", type);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnHairColorListChange(IPlayer player, string listText)
        {
            player.GetData("HairStore:AdjustHairIndex", out int type);

            int newIndex = int.Parse(listText);

            player.SetData("HairStore:ColorIndex", newIndex);

            player.Emit("HairStore:HairColorChange", type, newIndex);
        }

        public static void OnHairColorSelect(IPlayer player, string option)
        {
            if (option == "Close")
            {
                player.LoadCharacterCustomization();
                return;
            }

            player.GetData("HairStore:AdjustHairIndex", out int type);
            player.GetData("HairStore:ColorIndex", out int index);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            CustomCharacter customCharacter =
                JsonConvert.DeserializeObject<CustomCharacter>(playerCharacter.CustomCharacter);

            if (type == 0)
            {
                //  "Hair Color"

                HairInfo hairInfo = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

                hairInfo.Color = index;

                customCharacter.Hair = JsonConvert.SerializeObject(hairInfo);

                playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                context.SaveChanges();

                //player.RemoveCash(5);

                player.SendInfoNotification($"You've changed your hair color. This has cost you {5:C}. (Currently Free)");

                player.LoadCharacterCustomization();

                return;
            }

            if (type == 1)
            {
                // Hair Highlights
                HairInfo hairInfo = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

                hairInfo.HighlightColor = index;

                customCharacter.Hair = JsonConvert.SerializeObject(hairInfo);

                playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                context.SaveChanges();

                //player.RemoveCash(5);

                player.SendInfoNotification($"You've changed your hair highlight color. This has cost you {5:C}. (Currently Free)");

                player.LoadCharacterCustomization();

                return;
            }

            if (type == 2)
            {
                //Facial Hair Color

                customCharacter.BeardColor = index;

                playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                context.SaveChanges();

                //player.RemoveCash(5);

                player.SendInfoNotification($"You've changed your facial hair color. This has cost you {5:C}. (Currently Free)");

                player.LoadCharacterCustomization();

                return;
            }

            if (type == 3)
            {
                // Eyebrow Color
                customCharacter.EyebrowColor = index;

                playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                context.SaveChanges();

                //player.RemoveCash(5);

                player.SendInfoNotification($"You've changed your eyebrow color. This has cost you {5:C}. (Currently Free)");

                player.LoadCharacterCustomization();

                return;
            }

            if (type == 4)
            {
                // Chest Hair Color

                customCharacter.ChestHairColor = index;

                playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

                context.SaveChanges();

                //player.RemoveCash(5);

                player.SendInfoNotification($"You've changed your eyebrow color. This has cost you {5:C}. (Currently Free)");

                player.LoadCharacterCustomization();

                return;
            }
        }

        public static void ShowHairMenu(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            string[] hairNames;

            hairNames = player.GetClass().IsMale ? _maleHairNameList : _femaleHairNameList;

            foreach (string hairName in hairNames)
            {
                menuItems.Add(new NativeMenuItem(hairName, $"Cost: ~g~{hairCost:C}"));
            }

            NativeMenu menu = new NativeMenu("store:Hair:OnHairSelect", "Hair Store", "Select an option", menuItems)
            {
                ItemChangeTrigger = "store:Hair:OnHairChange",
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnHairChange(IPlayer player, int index, string itemText)
        {
            try
            {
                if (itemText == "Close") return;
                if (player.GetClass().IsMale)
                {
                    if (index >= _maleHairIntList.Length) return;

                    player.SetClothes(2, _maleHairIntList[index], 0);

                    HairTattooData hairTattooData = TattooHandler.FetchHairTattooData(_maleHairIntList[index], player.GetClass().IsMale);

                    if (hairTattooData != null)
                    {
                        player.Emit("loadTattooData", hairTattooData.Collection, hairTattooData.Overlay);
                    }
                }
                else
                {
                    if (index >= _femaleHairIntList.Length) return;

                    player.SetClothes(2, _femaleHairIntList[index], 0);

                    HairTattooData hairTattooData = TattooHandler.FetchHairTattooData(_maleHairIntList[index], player.GetClass().IsMale);

                    if (hairTattooData != null)
                    {
                        player.Emit("loadTattooData", hairTattooData.Collection, hairTattooData.Overlay);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void OnHairSelect(IPlayer player, string option, int index)
        {
            CharacterHandler.LoadCustomCharacter(player, true, true);
            if (option == "Close") return;

            string[] hairNames = player.GetClass().IsMale ? _maleHairNameList : _femaleHairNameList;

            string selectedHair = player.GetClass().IsMale ? _maleHairNameList[index] : _femaleHairNameList[index];

            if (player.FetchCharacter().Money < hairCost)
            {
                player.SendErrorNotification("You don't have enough.");
                return;
            }

            int hairIndex = Array.IndexOf(hairNames, selectedHair, 0);

            int hair = player.GetClass().IsMale ? _maleHairIntList[hairIndex] : _femaleHairIntList[hairIndex];

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            CustomCharacter customCharacter =
                JsonConvert.DeserializeObject<CustomCharacter>(playerCharacter.CustomCharacter);

            HairInfo hairInfo = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

            hairInfo.Hair = hair;

            customCharacter.Hair = JsonConvert.SerializeObject(hairInfo);

            playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

            context.SaveChanges();

            CharacterHandler.LoadCustomCharacter(player, true, true);

            player.RemoveCash((float)hairCost);

            player.SendInfoNotification($"You've bought {selectedHair} hair style for {hairCost:C}");

            player.GetData("INSTOREID", out int storeId);

            Models.Property property = Models.Property.FetchProperty(storeId);

            property?.AddToBalance(hairCost);
            player.Position = player.Position;
        }

        public static void ShowFacialHairMenu(IPlayer player)
        {
            string[] facialHairList = new string[] {"None", "Light Stubble", "Balbo", "Circle Beard", "Goatee", "Chin", "Chin Fuzz",
                "Pencil Chin Strap", "Scruffy", "Musketeer", "Mustache", "Trimmed Beard", "Stubble", "Thin Circle Beard",
                "Horseshoe", "Pencil and 'Chops", "Chin Strap Beard", "Balbo and Sideburns", "Mutton Chops", "Scruffy Beard",
                "Curly", "Curly & Deep Stranger", "Handlebar", "Faustic", "Otto & Patch", "Otto & Full Stranger", "Light Franz",
                "The Hampstead", "The Ambrose", "Lincoln Curtain"};

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (string facialHair in facialHairList)
            {
                menuItems.Add(new NativeMenuItem(facialHair, $"Cost ~g~{facialHairCost:C}"));
            }

            NativeMenu menu = new NativeMenu("store:hair:OnFacialHairSelect", "Tattoo Store", "Select a facial hair", menuItems)
            {
                ItemChangeTrigger = "store:hair:OnFacialHairChange",
                PassIndex = true
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnFacialHairChange(IPlayer player, int index, string itemText)
        {
            if (itemText == "Close") return;
            if (index >= _facialHair.Length) return;

            player.Emit("previewFacialHair", index - 1);
        }

        public static void OnFacialHairSelect(IPlayer player, string option, int selectedIndex)
        {
            CharacterHandler.LoadCustomCharacter(player, true, true);

            if (option == "Close") return;

            string selectedHair = _facialHair[selectedIndex];

            int index = Array.IndexOf(_facialHair, selectedHair, 0);

            if (player.FetchCharacter().Money < facialHairCost)
            {
                player.SendErrorNotification("You don't have enough money.");
                return;
            }

            if (selectedHair == "None")
            {
                index = 255;
            }
            else
            {
                index -= 1;
            }

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            CustomCharacter customCharacter =
                JsonConvert.DeserializeObject<CustomCharacter>(playerCharacter.CustomCharacter);

            List<ApperanceInfo> appearanceItems = JsonConvert.DeserializeObject<List<ApperanceInfo>>(customCharacter.Appearance);

            appearanceItems[1].Value = index;

            customCharacter.Appearance = JsonConvert.SerializeObject(appearanceItems);

            playerCharacter.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

            context.SaveChanges();

            CharacterHandler.LoadCustomCharacter(player, true, true);

            player.RemoveCash((float)facialHairCost);

            player.SendInfoNotification($"You've changed your facial hair to {selectedHair}. This has cost you {facialHairCost:C}.");

            player.GetData("INSTOREID", out int storeId);

            Models.Property property = Models.Property.FetchProperty(storeId);

            property?.AddToBalance(hairCost);
        }
    }
}