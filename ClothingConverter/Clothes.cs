using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;

namespace ClothingConverter
{
    public class Clothes
    {
        public enum ClothesType { Mask = 1, Hair, Torso, Legs, Backpack, Feet, Accessories, Undershirt, Bodyarmour, Decal, Top }

        public enum AccessoriesType { Hat = 0, Glasses = 1, Watch = 6, Ears = 2, Bracelets = 7 }

        public class clothesData
        {
            public int slot;
            public int drawable;
            public int texture;

            [DefaultValue(true)]
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public bool male;

            public clothesData(int s, int draw, int text, bool male = true)
            {
                slot = s;
                drawable = draw;
                texture = text;
                male = male;
            }
        }

        public class accessoryData
        {
            public int slot;
            public int drawable;
            public int texture;

            [DefaultValue(true)]
            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
            public bool male;

            public accessoryData(int s, int draw, int text, bool male = true)
            {
                slot = s;
                drawable = draw;
                texture = text;
                male = male;
            }
        }

        public class ClothesInfo
        {
            public string DisplayNameMale;
            public string DisplayNameFemale;
            public int TorsoData;
            public int Price;

            public ClothesInfo(string displayNameMale, string displayNameFemale, int price, int torso = 0)
            {
                DisplayNameMale = displayNameMale;
                DisplayNameFemale = displayNameFemale;
                TorsoData = torso;
                Price = price;
            }
        }

        public class ClothesJson
        {
            public Dictionary<clothesData, ClothesInfo> clothingItem;

            public override int GetHashCode()
            {
                if (clothingItem == null) return 0;
                return clothingItem.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                ClothesJson other = obj as ClothesJson;
                return other != null && other.clothingItem == this.clothingItem;
            }
        }
    }
}