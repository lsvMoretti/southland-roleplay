using System;
using System.Collections.Generic;
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

        public static string Shuffle(this String str)
        {
            Random rand = new Random();

            var list = new SortedList<int, char>();
            foreach (var c in str)
                list.Add(rand.Next(), c);
            return new string(list.Values.ToArray());
        }
    }
}