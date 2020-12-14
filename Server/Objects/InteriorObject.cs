using AltV.Net.Data;

namespace Server.Objects
{
    public class InteriorObject
    {
        public uint ObjectHash { get; set; }
        public Position Position { get; set; }
        public Rotation Rotation { get; set; }
    }
}