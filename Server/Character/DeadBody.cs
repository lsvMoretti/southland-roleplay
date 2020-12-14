using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Character.Clothing;
using Server.Extensions;

namespace Server.Character
{
    public class DeadBody
    {
        public string CharacterName { get; set; }
        public string CustomCharacter { get; set; }
        public string Clothes { get; set; }
        public string Accessories { get; set; }
        public string Tattoos { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Dimension { get; set; }

        public int Torso { get; set; }

        public string KillerName { get; set; }
        public uint KillerWeapon { get; set; }

        public DeadBody(IPlayer player)
        {
            CharacterName = player.FetchCharacter().Name;
            CustomCharacter = player.FetchCharacter().CustomCharacter;

            Clothes = player.FetchCharacter().ClothesJson;
            Accessories = player.FetchCharacter().AccessoryJson;

            Tattoos = player.FetchCharacter().TattooJson;  

            PosX = player.Position.X;
            PosY = player.Position.Y;
            PosZ = player.Position.Z;

            Dimension = player.Dimension;

            Torso = 0;

            List<ClothesData> clothing = JsonConvert.DeserializeObject<List<ClothesData>>(Clothes);

            ClothesData top = clothing.FirstOrDefault(x => x.slot == (int) ClothesType.Top);

            if (top != null)
            {
                int torso = Clothing.Clothes.GetTorsoDataForTop(top.drawable, player.FetchCharacter().Sex == 0);
                Torso = torso;
            }

            player.GetData("LastKiller", out string lastKiller);
            player.GetData("LastKillerWeapon", out uint lastWeapon);

            KillerName = lastKiller;
            KillerWeapon = lastWeapon;
        }
    }
}