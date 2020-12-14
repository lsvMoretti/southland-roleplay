namespace Server.Property
{
    public class PropertyDoor
    {
        public float ExitPosX { get; set; }
        public float ExitPosY { get; set; }
        public float ExitPosZ { get; set; }

        public uint ExitDimension { get; set; }

        public float EnterPosX { get; set; }
        public float EnterPosY { get; set; }
        public float EnterPosZ { get; set; }

        public uint EnterDimension { get; set; }
    }
}