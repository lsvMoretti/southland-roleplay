namespace Server.Character.Clothing
{
    public class Outfit
    {
        public string? Name { get; set; }
        public string? Clothes { get; set; }
        public string? Accessories { get; set; }

        public Outfit()
        {
        }

        public Outfit(string? name, string? clothes, string? accessories)
        {
            Name = name;
            Clothes = clothes;
            Accessories = accessories;
        }
    }
}