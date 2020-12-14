using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AltV.Net.Data;
using Newtonsoft.Json;

namespace Server.Models
{
    public class Motel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public string RoomList { get; set; }

        public int Value { get; set; }

        public int OwnerId { get; set; }

        public Motel()
        {
        }

        public Motel(string name, Position position)
        {
            Name = name;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            RoomList = JsonConvert.SerializeObject(new List<MotelRoom>());
            Value = 0;
            OwnerId = 0;
        }
    }

    public class MotelRoom
    {
        public int Id { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public int Value { get; set; }
        public int OwnerId { get; set; }

        public bool Locked { get; set; }

        public int MotelId { get; set; }
    }
}