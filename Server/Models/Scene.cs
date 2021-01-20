using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net.Data;

namespace Server.Models
{
    [Table("scenes")]
    public class Scene
    {
        [Key]
        public int Id { get; set; }

        public string? Text { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        public int CharacterId { get; set; }

        public int Dimension { get; set; }

        public Scene(int characterId, Position position, string text, int dimension)
        {
            Text = text;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;

            CharacterId = characterId;
            Dimension = dimension;
        }

        public Scene()
        {
        }
    }
}