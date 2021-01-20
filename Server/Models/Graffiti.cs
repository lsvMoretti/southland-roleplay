using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Extensions;

namespace Server.Models
{
    public class Graffiti
    {
        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public string? Text { get; set; }
        public GraffitiColor Color { get; set; }

        public static Graffiti CreateGraffiti(IPlayer player, string text, GraffitiColor color, Position position)
        {
            using Context context = new Context();

            Graffiti newGraffiti = new Graffiti
            {
                CharacterId = player.GetClass().CharacterId,
                PosX = position.X,
                PosY = position.Y,
                PosZ = position.Z + 0.5f,
                Text = text,
                Color = color
            };

            Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                return null;
            }

            playerCharacter.NextGraffitiTime = DateTime.Now.AddMinutes(1);

            context.Graffiti.Add(newGraffiti);

            context.SaveChanges();

            return newGraffiti;
        }

        public static void DeleteGraffiti(Graffiti graffiti)
        {
            using Context context = new Context();

            context.Graffiti.Remove(graffiti);

            context.SaveChanges();
        }

        public static List<Graffiti> FetchGraffitis()
        {
            using Context context = new Context();

            return context.Graffiti.ToList();
        }
    }

    public enum GraffitiColor
    {
        Green,
        Red,
        Yellow,
        Purple,
        LightBlue
    }
}