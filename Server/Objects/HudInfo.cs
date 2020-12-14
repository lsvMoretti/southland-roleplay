namespace Server.Objects
{
    public class HudInfo
    {
        public ushort Health { get; set; }
        public float Money { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        public HudInfo(ushort health, float money, int hour, int minute)
        {
            Health = health;
            Money = money;
            Hour = hour;
            Minute = minute;
        }
    }
}