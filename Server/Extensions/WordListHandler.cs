using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Extensions
{
    public class WordListHandler
    {
        public static string[]? WordList;

        public static void LoadWords()
        {
            Console.WriteLine("Loading Word List");

            if (!File.Exists(@"C:\Servers\Southland\data\words_alpha.txt"))
            {
                Console.WriteLine("No word list found!");
                return;
            }

            WordList = File.ReadAllLines(@"C:\Servers\Southland\data\words_alpha.txt");

            Console.WriteLine($"Loaded {WordList.Length} words!");
        }

        public static string? FetchWord(int minChar = 5, int maxChar = 5)
        {
            if (WordList == null) return null;
            Random rnd = new Random();
            while (true)
            {
                int randomNumber = rnd.Next(WordList.Length);
                string match = WordList[randomNumber];
                if (string.IsNullOrEmpty(match)) continue;
                if (match.Length < minChar) continue;
                if (match.Length > maxChar) continue;
                return match;
            }
        }
    }
}