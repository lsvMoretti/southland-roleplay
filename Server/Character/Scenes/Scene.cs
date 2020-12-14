using AltV.Net.Data;
using Server.Extensions.TextLabel;

namespace Server.Character.Scenes
{
    public class Scene
    {
        public string Text { get; set; }
        public Position Position { get; set; }
        public TextLabel TextLabel { get; set; }
        public int CharacterId { get; set; }

        public int DatabaseId { get; set; }

        public Scene(string text, Position position, TextLabel textLabel, int characterId, int databaseId = 0)
        {
            Text = text;
            Position = position;
            TextLabel = textLabel;
            CharacterId = characterId;
            DatabaseId = databaseId;
        }
    }
}