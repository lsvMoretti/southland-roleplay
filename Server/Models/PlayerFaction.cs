namespace Server.Models
{
    public class PlayerFaction
    {
        public int Id { get; set; }
        public int RankId { get; set; }
        public int DivisionId { get; set; }
        public bool Leader { get; set; }
    }
}