using System.Drawing;
using Newtonsoft.Json;

namespace Server.Extensions
{
    public class LsvColor
    {
        // Red
        public byte R { get; }

        // Green
        public byte G { get; }

        // Blue
        public byte B { get; }

        // Alpha
        public byte A { get; }

        [JsonConstructor]
        public LsvColor(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public LsvColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public LsvColor()
        { }
    }
}