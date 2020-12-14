namespace Server.Groups.Police.MDT
{
    public class PersonResult
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public int CharacterId { get; set; }

        /// <summary>
        /// Result for MDC person search
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="gender"></param>
        /// <param name="characterId"></param>
        public PersonResult(string name, int age, string gender, int characterId)
        {
            Name = name;
            Age = age;
            Gender = gender;
            CharacterId = characterId;
        }
    }
}