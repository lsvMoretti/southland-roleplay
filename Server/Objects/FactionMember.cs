namespace Server.Objects
{
    public class FactionMember
    {
        public string Name { get; set; }
        public int RankId { get; set; }
        public int FactionId { get; set; }

        public FactionMember(string name, int rankId, int factionId)
        {
            Name = name;
            RankId = rankId;
            FactionId = factionId;
        }
    }
}