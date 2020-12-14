using AltV.Net.Data;
using Server.Models;

namespace Server.Extensions
{
    public static class DoorExtension
    {
        public static Position Position(this Door door)
        {
            return new Position(door.PosX, door.PosY, door.PosZ);
        }
    }
}