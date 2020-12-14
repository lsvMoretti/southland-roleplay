using System.Collections.Generic;

namespace Server.Groups.EUP
{
    public class EupOutfit
    {
        public string Name { get; set; }
        public int[] Torso { get; set; }
        public int[]  Legs { get; set; }
        public int[]  Shoes { get; set; }
        public int[]  Top { get; set; }

        /// <summary>
        /// Creates a new EUP Outfit
        /// </summary>
        /// <param name="name">Name of Outfit</param>
        /// <param name="torso">Drawable & Texture of Torso</param>
        /// <param name="legs">Drawable & Texture of Legs</param>
        /// <param name="shoes">Drawable & Texture of Shoes</param>
        /// <param name="top">Drawable & Texture of Top</param>
        public EupOutfit(string name, int[] torso, int[] legs, int[] shoes, int[] top)
        {
            Name = name;
            Torso = torso;
            Legs = legs;
            Shoes = shoes;
            Top = top;
        }
    }
}