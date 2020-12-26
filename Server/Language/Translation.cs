using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.Language
{
    public class Translation    {
        public string text { get; set; } 
        public string to { get; set; } 
    }

    public class Translations    {
        public List<Translation> translations { get; set; } 
    }
}