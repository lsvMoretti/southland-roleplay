using System.Collections.Generic;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Character.Clothing;
using Server.Extensions;

namespace Server.Character
{
    public class CustomCharacter
    {
        /// <summary>
        /// Male 0 - Female 1
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// JSON of ParentInfo
        /// </summary>
        public string Parents { get; set; }

        /// <summary>
        /// JSON of List of floats
        /// </summary>
        public string Features { get; set; }

        /// <summary>
        ///  JSON of List<ApperanceInfo>
        /// </summary>
        public string Appearance { get; set; }

        /// <summary>
        /// JSON of HairInfo
        /// </summary>
        public string Hair { get; set; }

        /// <summary>
        /// Eyebrow Color
        /// </summary>
        public int EyebrowColor { get; set; }

        /// <summary>
        /// Beard Color
        /// </summary>
        public int BeardColor { get; set; }

        /// <summary>
        /// Eye Color
        /// </summary>
        public int EyeColor { get; set; }

        /// <summary>
        /// Blue Color
        /// </summary>
        public int BlushColor { get; set; }

        /// <summary>
        /// Lipstick Color
        /// </summary>
        public int LipstickColor { get; set; }

        /// <summary>
        /// Chest Hair Color
        /// </summary>
        public int ChestHairColor { get; set; }

        public static CustomCharacter DefaultCharacter()
        {
            ParentInfo parentInfo = new ParentInfo
            {
                Father = 0,
                Mother = 0,
                Similarity = 0.5f,
                SkinSimilarity = 0.5f
            };

            HairInfo hairInfo = new HairInfo
            {
                Hair = 0,
                Color = 0,
                HighlightColor = 0
            };

            List<float> featureInfo = new List<float>();

            for (int i = 0; i < 20; i++)
            {
                featureInfo.Add(0.0f);
            }

            List<ApperanceInfo> apperanceInfo = new List<ApperanceInfo>();

            for (int i = 0; i <= 11; i++)
            {
                apperanceInfo.Add(new ApperanceInfo
                {
                    Value = -1,
                    Opacity = 1.0f
                });
            }

            CustomCharacter newCharacter = new CustomCharacter
            {
                Gender = 0,
                Parents = JsonConvert.SerializeObject(parentInfo),
                Features = JsonConvert.SerializeObject(featureInfo),
                Appearance = JsonConvert.SerializeObject(apperanceInfo),
                Hair = JsonConvert.SerializeObject(hairInfo),
                EyebrowColor = 0,
                BeardColor = 0,
                EyeColor = 0,
                BlushColor = 0,
                LipstickColor = 0,
                ChestHairColor = 0,
            };
            return newCharacter;
        }

        public static void LoadCustomCharacter(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            if (playerCharacter.Sex == 0)
            {
                player.Model = (uint)PedModel.FreemodeMale01;
            }
            else
            {
                player.Model = (uint)PedModel.FreemodeFemale01;
            }

            player.Emit("loadCustomPlayer", playerCharacter.CustomCharacter, playerCharacter.ClothesJson, playerCharacter.AccessoryJson);

            List<ClothesData> clothingData =
                JsonConvert.DeserializeObject<List<ClothesData>>(playerCharacter.ClothesJson);

            List<AccessoryData> accessoryData =
                JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson);

            Clothes.LoadClothes(player, clothingData, accessoryData);
        }
    }

    public class ParentInfo
    {
        public int Father { get; set; }

        public int Mother { get; set; }

        public float Similarity { get; set; }

        public float SkinSimilarity { get; set; }
    }

    public class HairInfo
    {
        public int Hair { get; set; }
        public int Color { get; set; }
        public int HighlightColor { get; set; }
    }

    public class ApperanceInfo
    {
        public int Value { get; set; }
        public float Opacity { get; set; }
    }
}