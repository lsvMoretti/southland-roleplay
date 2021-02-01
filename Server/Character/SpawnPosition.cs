namespace Server.Character
{
    public class SpawnPosition
    {
        public SpawnType SpawnType { get; set; }
        public int PropertyId { get; set; }
        public int FactionId { get; set; }

        public int MotelId { get; set; }
        public int MotelRoomId { get; set; }

        public SpawnPosition()
        {
        }

        public SpawnPosition(SpawnType spawnType, int propertyId = 0, int factionId = 0, int motelId = 0, int motelRoomId = 0)
        {
            SpawnType = spawnType;
            PropertyId = propertyId;
            FactionId = factionId;
            MotelId = motelId;
            MotelRoomId = motelRoomId;
        }
    }

    public enum SpawnType
    {
        LastLocation, Property, Faction, Motel
    }
}