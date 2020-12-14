using System.Collections.Generic;
using AltV.Net.Data;

namespace Server.Inventory
{
    public class Gps
    {
        public string Name { get; set; }
        public List<GpsWayPoint> WayPoints { get; set; }

    }

    public class GpsWayPoint
    {
        public string Name { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public GpsWayPoint()
        {

        }

        public GpsWayPoint(string name, Position position)
        {
            Name = name;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
        }
    }
}