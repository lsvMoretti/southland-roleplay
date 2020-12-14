using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AltV.Net;
using AltV.Net.Elements.Entities;

namespace Server.Extensions
{
    public class Utility
    {
        private static readonly Random _random = new Random();

        public static IPlayer FindPlayerByNameOrId(string nameorid)
        {
            try
            {
                bool tryParse = int.TryParse(nameorid, out int id);

                if (tryParse)
                {
                    if (id == 0) return null;

                    return Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == id);
                }

                return Alt.Server.GetPlayers().FirstOrDefault(x =>
                    x.FetchCharacter() != null && x.GetClass().Name.ToLower().Contains(nameorid.ToLower()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// The last time the DLL / Script was built
        /// </summary>
        public static string LastUpdate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString();

        /// <summary>
        /// The Version number of the build
        /// </summary>
        public static string Build = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Generates a random number (1,2,3) by length
        /// </summary>
        /// <param name="length"></param>
        /// <returns>string (number only)</returns>
        public static string GenerateRandomNumber(int length)
        {
            //const string chars = "0123456789";

            string number = "";

            for (int i = 0; i < length; i++)
            {
                if (i == 0)
                {
                    number = _random.Next(1, 9).ToString();
                }
                else
                {
                    string joinNumber = _random.Next(0, 9).ToString();

                    number += joinNumber;
                }
            }

            return number;

            /* return new string(Enumerable.Repeat(chars, length)
                 .Select(s => s[_random.Next(s.Length)]).ToArray());*/
        }

        /// <summary>
        /// Generates a random string (A,B,C,1,2,3 etc)
        /// </summary>
        /// <param name="length"></param>
        /// <returns>string</returns>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTOVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Rescales a set of variables.
        /// Ex: Input = amount of products
        /// InputMin = Minimum amount of Products
        /// InputMax = Maximum amount of Products
        /// OutMin = Minimum amount returned
        /// OutMax = Maximum amount returned
        /// </summary>
        /// <param name="input">Input Value</param>
        /// <param name="inputMin">Minimum expected input</param>
        /// <param name="inputMax">Maximum expected input</param>
        /// <param name="outMin">Value when input = inputMin</param>
        /// <param name="outMax">Value when input = inputMax</param>
        /// <returns>Rescaled value</returns>
        public static double Rescale(double input, double inputMin, double inputMax, double outMin, double outMax)
        {
            double outInRatio = 0.0;

            if (inputMax == inputMin)
            {
                outInRatio = 0.0;
            }
            else
            {
                outInRatio = (outMax - outMin) / (inputMax - inputMin);
            }

            if (input <= inputMin)
            {
                return outMin;
            }

            if (input >= inputMax)
            {
                return outMax;
            }

            return outInRatio * (input - inputMin) + outMin;
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}