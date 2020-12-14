namespace Server.Language
{
    public class Language
    {
        public string LanguageName { get; set; }
        public string Code { get; set; }

        public Language(string name, string code)
        {
            LanguageName = name;
            Code = code;
        }
    }
}