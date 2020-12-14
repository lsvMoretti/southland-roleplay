using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ClothingConverter
{
    internal class Program
    {
        //File name after the male/female_
        //private static string fileValue = "torsos";

        /// <summary>
        /// Male Torso Data
        /// Key - Top Drawable
        /// Value - Torso
        /// </summary>
        private static Dictionary<int, int> MaleTopToTorso = new Dictionary<int, int>();

        /// <summary>
        /// female Torso Data
        /// Key - Top Drawable
        /// Value - Torso
        /// </summary>
        private static Dictionary<int, int> FemaleTopToTorso = new Dictionary<int, int>();

        private static void Main(string[] args)
        {
            bool readyToExit = false;

            Console.WriteLine($"Starting Clothing Converter - Created by Moretti");

            while (!readyToExit)
            {
                string userInput = Console.ReadLine();

                if (userInput.Contains("start"))
                {
                    string[] fileArgs = userInput.Split(' ');
                    StartConversion(fileArgs[1]);
                }

                //if (userInput == "start")
                //{
                //    StartConversion();
                //}
            }
        }

        private static void StartConversion(string fileName)
        {
            Console.WriteLine($"Starting Clothing Conversion");
            StartClothingConversion(fileName);
        }

        private static void StartClothingConversion(string fileValue)
        {
            if (!Directory.Exists("C:/ConvertClothing/Clothes"))
            {
                Console.WriteLine($"Error: No directory found at: C:/ConvertClothing/Clothes");
                return;
            }

            #region Copy Roots Clothing

            Dictionary<Clothes.clothesData, Clothes.ClothesInfo> clothesData = new Dictionary<Clothes.clothesData, Clothes.ClothesInfo>();

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;

            settings.ContractResolver = new DictionaryAsArrayResolver();

            Clothes.ClothesJson newJson = new Clothes.ClothesJson();

            string clothesCopy = "C:/ConvertClothing/Clothes";

            try
            {
                if (Directory.Exists(clothesCopy))
                {
                    var clothingFiles = Directory.GetFiles(clothesCopy);
                    Console.WriteLine("Found Directory and Files");

                    Dictionary<Clothes.clothesData, Clothes.ClothesInfo> femaleClothesData = new Dictionary<Clothes.clothesData, Clothes.ClothesInfo>();
                    Dictionary<Clothes.clothesData, Clothes.ClothesInfo> maleClothesData = new Dictionary<Clothes.clothesData, Clothes.ClothesInfo>();

                    foreach (var clothingFile in clothingFiles)
                    {
                        if (clothingFile == $"{clothesCopy}\\clothingOutput.json") continue;

                        Console.WriteLine($"Opening {clothingFile}");
                        string content = File.ReadAllText(clothingFile);

                        var rootJson = GtaClothing.FromJson(content);

                        if (clothingFile == $"{clothesCopy}\\props_female_{fileValue}.json")
                        {
                            foreach (var data in rootJson)
                            {
                                foreach (var data2 in data.Value)
                                {
                                    Console.WriteLine($"Female {fileValue}");
                                    Clothes.clothesData key = new Clothes.clothesData(11, Convert.ToInt32(data.Key), Convert.ToInt32(data2.Key), false);

                                    string localName = data2.Value.Localized;

                                    if (localName == "NULL" || localName == "NO_LABEL")
                                    {
                                        localName = "undefined";
                                    }

                                    Clothes.ClothesInfo cInfo = new Clothes.ClothesInfo(localName, localName, 5, 0);

                                    femaleClothesData.Add(key, cInfo);
                                }
                            }
                        }
                        //if (clothingFile == $"{clothesCopy}\\male_{fileValue}.json")
                        if (clothingFile == $"{clothesCopy}\\props_male_{fileValue}.json")
                        {
                            foreach (var data in rootJson)
                            {
                                foreach (var data2 in data.Value)
                                {
                                    Console.WriteLine($"Male {fileValue}");

                                    string localName = data2.Value.Localized;

                                    if (localName == "NULL" || localName == "NO_LABEL")
                                    {
                                        localName = "undefined";
                                    }

                                    Clothes.clothesData key = new Clothes.clothesData(11, Convert.ToInt32(data.Key), Convert.ToInt32(data2.Key), false);

                                    Clothes.ClothesInfo cInfo = new Clothes.ClothesInfo(localName, "", 5, 0);

                                    maleClothesData.Add(key, cInfo);
                                }
                            }
                        }
                    }
                    foreach (var maleClothesInfo in maleClothesData)
                    {
                        clothesData.Add(maleClothesInfo.Key, maleClothesInfo.Value);
                    }

                    foreach (var femaleClothesInfo in femaleClothesData)
                    {
                        var maleFound = clothesData.FirstOrDefault(x =>
                            x.Key.drawable == femaleClothesInfo.Key.drawable &&
                            x.Key.texture == femaleClothesInfo.Key.texture);

                        if (maleFound.Equals(default(KeyValuePair<Clothes.clothesData, Clothes.ClothesInfo>)))
                        {
                            // Not found male
                            Console.WriteLine("Male Not Found");
                            clothesData.Add(femaleClothesInfo.Key, femaleClothesInfo.Value);
                            continue;
                        }

                        // Male found
                        Console.WriteLine("Male Found");
                        maleFound.Value.DisplayNameFemale = femaleClothesInfo.Value.DisplayNameFemale;
                    }
                    if (clothesData.Any())
                    {
                        newJson.clothingItem = clothesData;

                        var jsonOutput = JsonConvert.SerializeObject(newJson, settings);

                        File.WriteAllText($"{clothesCopy}/output/props/{fileValue}.json", jsonOutput);
                        Console.WriteLine($"Finished");
                    }
                }
            }
            catch
            {
            }

            #endregion Copy Roots Clothing
        }
    }
}