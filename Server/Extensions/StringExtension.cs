using System.Linq;

namespace Server.Extensions
{
    public static class StringExtension
    {
        public static string CapitalizeFirst(this string str)
        {
            string firstLetter = str.First().ToString();

            if (firstLetter == firstLetter.ToUpper()) return str;

            return firstLetter.ToUpper() + str.Substring(1);
        }

        public static string FullStopEnd(this string str)
        {
            string lastChar = str.Last().ToString();

            if (lastChar == ".") return str;

            return str + ".";
        }
    }
}