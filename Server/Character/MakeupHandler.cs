using System;
using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;

namespace Server.Character
{
    public class MakeupHandler
    {
        private static List<string> _overlayList = new List<string>
        {
            "Blemishes", "Facial Hair", "Eyebrows", "Ageing", "Makeup", "Blush", "Complexion", "Sun Damage", "Lipstick", "Moles/Freckles", "Chest Hair", "Body Blemishes"
        };

        private static List<string> _blemishNameList = new List<string>
        {
            "None", "Measles", "Pimples", "Spots", "Break Out", "Blackheads", "Build Up", "Pustules", "Zits",
            "Full Acne", "Acne", "Cheek Rash", "Face Rash", "Picker", "Puberty", "Eyesore", "Chin Rash", "Two Face",
            "T Zone", "Greasy", "Marked", "Acne Scarring", "Full Acne Scarring", "Cold Sores", "Impetigo"
        }; 

        private static List<string> _eyebrowList = new List<string>
        {
            "None", "Balanced", "Fashion", "Cleopatra", "Quizzical", "Femme", "Seductive", "Pinched", "Chola",
            "Triomphe", "Carefree", "Curvaceous", "Rodent", "Double Tram", "Thin", "Penciled", "Mother Plucker",
            "Straight and Narrow", "Natural", "Fuzzy", "Unkempt", "Caterpillar", "Regular", "Mediterranean", "Groomed",
            "Bushels", "Feathered", "Prickly", "Monobrow", "Winged", "Triple Tram", "Arched Tram", "Cutouts",
            "Fade Away", "Solo Tram"
        };

        private static List<string> _ageingList = new List<string>
        {
            "None", "Crow's Feet", "First Signs", "Middle Aged", "Worry Lines", "Depression", "Distinguished", "Aged",
            "Weathered", "Wrinkled", "Sagging", "Tough Life", "Vintage", "Retired", "Junkie", "Geriatric"
        };

        private static List<string> _makeupList = new List<string>
        {
            "None", "Smoky Black", "Bronze", "Soft Gray", "Retro Glam", "Natural Look", "Cat Eyes", "Chola", "Vamp",
            "Vinewood Glamour", "Bubblegum", "Aqua Dream", "Pin Up", "Purple Passion", "Smoky Cat Eye",
            "Smoldering Ruby", "Pop Princess"
        };

        private static List<string> _blushList = new List<string>
        {
            "None", "Full", "Angled", "Round", "Horizontal", "High", "Sweetheart", "Eighties"
        };

        private static List<string> _complextionList = new List<string>
        {
            "None", "Rosy Cheeks", "Stubble Rash", "Hot Flush", "Sunburn", "Bruised", "Alchoholic", "Patchy", "Totem", "Blood Vessels", "Damaged", "Pale", "Ghostly"
        };

        private static List<string> _sunDamageList = new List<string>
        {
            "None", "Uneven", "Sandpaper", "Patchy", "Rough", "Leathery", "Textured", "Coarse", "Rugged", "Creased", "Cracked", "Gritty"
        };

        private static List<string> _lipstickList = new List<string>
        {
            "None", "Color Matte", "Color Gloss", "Lined Matte", "Lined Gloss", "Heavy Lined Matte",
            "Heavy Lined Gloss", "Lined Nude Matte", "Liner Nude Gloss", "Smudged", "Geisha"
        };

        private static List<string> _moleNames = new List<string>
        {
            "None", "Cherub", "All Over", "Irregular", "Dot Dash", "Over the Bridge", "Baby Doll", "Pixie",
            "Sun Kissed", "Beauty Marks", "Line Up", "Modelesque", "Occasional", "Speckled", "Rain Drops", "Double Dip",
            "One Sided", "Pairs", "Growth"
        };

        public static void ShowMakeupMenu(IPlayer player)
        {
            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Blemishes"),
                new NativeMenuItem("Eyebrows"),
                new NativeMenuItem("Ageing"),
                new NativeMenuItem("Makeup"),
                new NativeMenuItem("Blush"),
                new NativeMenuItem("Complexion"),
                new NativeMenuItem("Sun Damage"),
                new NativeMenuItem("Lipstick"),
                new NativeMenuItem("Moles/Freckles")
            };

            NativeMenu menu = new NativeMenu("Makeup:MainMenuSelect", "Makeup", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnMainMenuSelect(IPlayer player, string option)
        {
            if(option == "Close") return;

            List<NativeListItem> listItems = new List<NativeListItem>();

            List<string> opacityValues = new List<string>();

            for (int i = 0; i <= 100; i++)
            {
                opacityValues.Add(i.ToString());
            }
            
            List<string> hairColorList = new List<string>();

            for (int i = 0; i <= 64; i++)
            {
                hairColorList.Add(i.ToString());
            }

            List<string> lipstickColorList = new List<string>();

            for (int i = 0; i <= 27; i++)
            {
                lipstickColorList.Add(i.ToString());
            }

            listItems.Add(new NativeListItem("Opacity", opacityValues));

            if (option == "Blemishes")
            {
                listItems.Add(new NativeListItem(option, _blemishNameList));
            }

            if (option == "Eyebrows")
            {
                listItems.Add(new NativeListItem(option, _eyebrowList));
            }

            if (option == "Ageing")
            {
                listItems.Add(new NativeListItem(option, _ageingList));
            }

            if (option == "Makeup")
            {
                listItems.Add(new NativeListItem(option, _makeupList));
            }

            if (option == "Blush")
            {
                listItems.Add(new NativeListItem(option, _blushList));
                listItems.Add(new NativeListItem("Blush Color", lipstickColorList));
            }

            if (option == "Complexion")
            {
                listItems.Add(new NativeListItem(option, _complextionList));
            }

            if (option == "Sun Damage")
            {
                listItems.Add(new NativeListItem(option, _sunDamageList));
            }

            if (option == "Lipstick")
            {
                listItems.Add(new NativeListItem(option, _lipstickList));
                listItems.Add(new NativeListItem("Lipstick Color", lipstickColorList));
            }

            if (option == "Moles/Freckles")
            {
                listItems.Add(new NativeListItem(option, _moleNames));
            }

            player.SetData("Makeup:CurrentlyEditing", option);
            player.SetData("Makeup:CurrentOpacity", 0.0f);

            NativeMenu menu = new NativeMenu("Makeup:OnSubMenuSelect", "Makeup", "Select an option")
            {
                ListMenuItems = listItems,
                ListTrigger = "Makeup:OnSubMenuListChange",
                PassIndex = true,
                ItemChangeTrigger = "Makeup:OnSubMenuItemChange"
            };

            NativeUi.ShowNativeMenu(player, menu, true);
        }

        public static void OnSubMenuSelect(IPlayer player, string option, int index)
        {
            if (option == "Close")
            {
                player.DeleteData("Makeup:CurrentlyEditing");
                player.DeleteData("Makeup:CurrentOpacity");
                player.DeleteData("Makeup:CurrentColorEditIndex");
                player.DeleteData("Makeup:CurrentEditIndex");
                player.LoadCharacterCustomization();
                return;
            }

            player.GetData("Makeup:CurrentlyEditing", out string editing);
            player.GetData("Makeup:CurrentOpacity", out float currentOpacity);
            player.GetData("Makeup:CurrentEditIndex", out int editIndex);
            bool hasColorData = player.GetData("Makeup:CurrentColorEditIndex", out int colorId);

            player.DeleteData("Makeup:CurrentlyEditing");
            player.DeleteData("Makeup:CurrentOpacity");
            player.DeleteData("Makeup:CurrentColorEditIndex");
            player.DeleteData("Makeup:CurrentEditIndex");

            int overlayIndex = _overlayList.IndexOf(option);

            if (overlayIndex == -1)
            {
                string[] splitString = option.Split(' ');
                overlayIndex = _overlayList.IndexOf(splitString[0]);

                if (overlayIndex == -1)
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }
            }

            Console.WriteLine($"Selected Overlay Index: {overlayIndex}");
            Console.WriteLine($"Currently Editing: {editing}");
            Console.WriteLine($"EditIndex: {editIndex}");


            using Context context = new Context();

            Models.Character character = context.Character.Find(player.GetClass().CharacterId);

            CustomCharacter customCharacter = JsonConvert.DeserializeObject<CustomCharacter>(character.CustomCharacter);

            List<ApperanceInfo> appearanceInfo =
                JsonConvert.DeserializeObject<List<ApperanceInfo>>(customCharacter.Appearance);

            ApperanceInfo selectedInfo = appearanceInfo[overlayIndex];

            selectedInfo.Value = editIndex;
            selectedInfo.Opacity = currentOpacity;


            if (editing == "Blush"  || editing == "Blush Color")
            {
                if (hasColorData)
                {
                    customCharacter.BlushColor = colorId;
                }
            }

            if (editing == "Lipstick" || editing == "Lipstick Color")
            {
                if (hasColorData)
                {
                    Console.WriteLine($"Lipstick Color: {colorId}");
                    customCharacter.LipstickColor = colorId;
                }
            }

            customCharacter.Appearance = JsonConvert.SerializeObject(appearanceInfo);

            character.CustomCharacter = JsonConvert.SerializeObject(customCharacter);

            context.SaveChanges();

            
            
            player.SendNotification($"~y~You've updated your appearance!", true);

            player.LoadCharacterCustomization();
        }

        public static void OnSubMenuListChange(IPlayer player, string itemText, string listText)
        {
            player.GetData("Makeup:CurrentlyEditing", out string editing);
            player.GetData("Makeup:CurrentOpacity", out float currentOpacity);

            float opacityValue = currentOpacity / 100;

            if (itemText == "Opacity")
            {
                bool tryParse = float.TryParse(listText, out currentOpacity);

                if (!tryParse)
                {
                    player.SendErrorNotification("An error occurred fetching the opacity.");
                    return;
                }

                player.SetData("Makeup:CurrentOpacity", currentOpacity);

                opacityValue = currentOpacity / 100;

                if (editing == "Blemishes")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 0, index, opacityValue);

                    return;
                }
                if (editing == "Eyebrows")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 2, index, opacityValue);

                    return;
                }
                if (editing == "Ageing")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 3, index, opacityValue);

                    return;
                }
                if (editing == "Makeup")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 4, index, opacityValue);

                    return;
                }
                if (editing == "Blush")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 5, index, opacityValue);

                    return;
                }

                if (itemText == "Blush Color")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentColorEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 0;
                    }

                    player.Emit("Makeup:OnColorChange", 5, 2, index);

                    return;
                }
                
                if (editing == "Complexion")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 6, index, opacityValue);

                    return;
                }
                
                if (editing == "Sun Damage")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 7, index, opacityValue);

                    return;
                }
                
                if (editing == "Lipstick")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 8, index, opacityValue);

                    return;
                }

                if (itemText == "Lipstick Color")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentColorEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 0;
                    }

                    player.Emit("Makeup:OnColorChange", 8, 2, index);

                    return;
                }


                if (editing == "Moles/Freckles")
                {
                    bool hasEditIndex = player.GetData("Makeup:CurrentEditIndex", out int index);

                    if (!hasEditIndex)
                    {
                        index = 255;
                    }

                    player.Emit("Makeup:OnListChange", 9, index, opacityValue);

                    return;
                }

            }
            if (itemText == "Blemishes")
            {
                int index = _blemishNameList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);

                player.Emit("Makeup:OnListChange", 0, index, opacityValue);

                return;
            }

            if (itemText == "Eyebrows")
            {
                int index = _eyebrowList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 2, index, opacityValue);

                return;
            }

            if (itemText == "Ageing")
            {
                int index = _ageingList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 3, index, opacityValue);

                return;
            }

            if (itemText == "Makeup")
            {
                int index = _makeupList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 4, index, opacityValue);

                return;
            }

            if (itemText == "Blush")
            {
                int index = _blushList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 5, index, opacityValue);

                return;
            }

            if (itemText == "Blush Color")
            {
                bool tryParse = int.TryParse(listText, out int index);

                if (!tryParse)
                {
                    index = 0;
                }

                player.SetData("Makeup:CurrentColorEditIndex", index);

                player.Emit("Makeup:OnColorChange", 5, 2, index);
            }

            if (itemText == "Complexion")
            {
                int index = _complextionList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 6, index, opacityValue);

                return;
            }
            

            if (itemText == "Sun Damage")
            {
                int index = _sunDamageList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 7, index, opacityValue);

                return;
            }
            
            if (itemText == "Lipstick")
            {
                int index = _lipstickList.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 8, index, opacityValue);

                return;
            }

            if (itemText == "Lipstick Color")
            {
                bool tryParse = int.TryParse(listText, out int index);

                if (!tryParse)
                {
                    index = 0;
                }

                player.SetData("Makeup:CurrentColorEditIndex", index);

                player.Emit("Makeup:OnColorChange", 8, 2, index);
            }

            if (itemText == "Moles/Freckles")
            {
                int index = _moleNames.IndexOf(listText);

                if (index == 0)
                {
                    index = 255;
                }
                else
                {
                    index -= 1;
                }

                player.SetData("Makeup:CurrentEditIndex", index);
                player.Emit("Makeup:OnListChange", 9, index, opacityValue);

                return;
            }



            
        }

        public static void OnSubMenuItemChange(IPlayer player, string itemText)
        {

        }
    }
}