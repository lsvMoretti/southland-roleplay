using AltV.Net.Data;

namespace Server.Extensions.TextLabel
{
    public class TextLabel
    {
        /// <summary>
        /// Returns a Text Label Object
        /// </summary>
        /// <param name="text"></param>
        /// <param name="position"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="range"></param>
        /// <param name="dimension"></param>
        public TextLabel(string text, Position position, TextFont font, LsvColor color, float range = 5f, int dimension = 0)
        {
            Text = text;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            Font = (byte)font;
            Color = color;
            Range = range;
            Dimension = dimension;
        }

        public string Text { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Font { get; set; }
        public float Range { get; set; }
        public LsvColor Color { get; set; }

        public int Dimension { get; set; }
    }
}