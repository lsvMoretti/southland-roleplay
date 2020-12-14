using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;

namespace Server.Objects
{
    public class Blip
    {
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public int Sprite { get; set; }

        public int Color { get; set; }

        public float Scale { get; set; }

        public bool ShortRange { get; set; }

        public string Name { get; set; }

        public int? UniqueId { get; set; }

        /// <summary>
        /// Creates a new blip
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        /// <param name="sprite"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        /// <param name="shortRange"></param>
        /// <param name="uniqueId"></param>
        public Blip(string name, Position position, int sprite, int color, float scale, bool shortRange = true,
            int uniqueId = -1)
        {
            Name = name;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            Sprite = sprite;
            Color = color;
            Scale = scale;
            ShortRange = shortRange;
            UniqueId = uniqueId;
        }
    }
}