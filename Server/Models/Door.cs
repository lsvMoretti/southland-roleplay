using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Extensions;

namespace Server.Models
{
    public class Door : IWritable
    {
        [Key] public int Id { get; set; }

        public string? Model { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public bool Locked { get; set; }
        public int OwnerId { get; set; }
        public int PropertyId { get; set; }
        public int FactionId { get; set; }

        public int Dimension { get; set; }

        public void OnWrite(IMValueWriter writer)
        {
            writer.BeginObject();
            writer.Name("Id");
            writer.Value(Id);
            writer.Name("Model");
            writer.Value(Model);
            writer.Name("PosX");
            writer.Value(PosX);
            writer.Name("PosY");
            writer.Value(PosY);
            writer.Name("PosZ");
            writer.Value(PosZ);
            writer.Name("Locked");
            writer.Value(Locked);
            writer.Name("OwnerId");
            writer.Value(OwnerId);
            writer.Name("PropertyId");
            writer.Value(PropertyId);
            writer.Name("FactionId");
            writer.Value(FactionId);
            writer.Name("Dimension");
            writer.Value(Dimension);
            writer.EndObject();
        }

        public Door()
        {
        }

        public Door(string model, Position position, bool locked = false, int ownerId = 0, int propertyId = 0, int factionId = 0, int dimension = 0)
        {
            Model = model;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            Locked = locked;
            OwnerId = ownerId;
            PropertyId = propertyId;
            FactionId = factionId;
            Dimension = dimension;
        }

        public static Door FetchDoor(int id)
        {
            using Context context = new Context();

            return context.Doors.Find(id);
        }

        public static Door FetchDoor(string model, Position position, int dimension)
        {
            using Context context = new Context();

            var door = context.Doors.FirstOrDefault(x => x.Model == model && x.Position() == position && x.Dimension == dimension);

            return door;
        }

        public static Door FetchNearestDoor(IPlayer player, float range = 1.5f)
        {
            using Context context = new Context();

            Position playerPosition = player.Position;

            Door nearestDoor = null;

            float distance = range;

            foreach (Door door in context.Doors.ToList())
            {
                Position doorPosition = door.Position();

                if (doorPosition.Distance(playerPosition) < distance && door.Dimension == player.Dimension)
                {
                    distance = doorPosition.Distance(playerPosition);
                    nearestDoor = door;
                }
            }

            return nearestDoor;
        }
    }
}