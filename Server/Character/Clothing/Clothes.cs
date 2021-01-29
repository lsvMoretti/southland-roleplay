using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Extensions;
using Server.Inventory;

namespace Server.Character.Clothing
{
    public enum ClothesType { Mask = 1, Hair, Torso, Legs, Backpack, Feet, Accessories, Undershirt, Bodyarmour, Decal, Top }

    public enum AccessoriesType { Hat = 0, Glasses = 1, Watch = 6, Ears = 2, Bracelets = 7 }

    public class ClothesData
    {
        public int slot;
        public int drawable;
        public int texture;

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool male;

        public ClothesData(int s, int draw, int text, bool isMale = true)
        {
            slot = s;
            drawable = draw;
            texture = text;
            male = isMale;
        }
    }

    public class AccessoryData
    {
        public int slot;
        public int drawable;
        public int texture;

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool male;

        public AccessoryData(int s, int draw, int text, bool isMale = true)
        {
            slot = s;
            drawable = draw;
            texture = text;
            male = isMale;
        }
    }

    public class ClothesInfo
    {
        public string DisplayNameMale;
        public string DisplayNameFemale;
        public int TorsoData;
        public int Price;

        public ClothesInfo(string displayNameMale, string displayNameFemale, int price, int torso = 0)
        {
            DisplayNameMale = displayNameMale;
            DisplayNameFemale = displayNameFemale;
            TorsoData = torso;
            Price = price;
        }
    }

    public class ClothesJson
    {
        public Dictionary<ClothesData, ClothesInfo> clothingItem;

        public override int GetHashCode()
        {
            if (clothingItem == null) return 0;
            return clothingItem.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is ClothesJson other && other.clothingItem == this.clothingItem;
        }
    }

    #region Roots Clothing Stuff

    public partial class RootJson
    {
        [JsonProperty("GXT")]
        public string Gxt { get; set; }

        [JsonProperty("Localized")]
        public string Localized { get; set; }
    }

    public partial class RootJson
    {
        public static Dictionary<string, Dictionary<string, RootJson>> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, RootJson>>>(json, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    public partial class BestTorso
    {
        [JsonProperty("BestTorsoDrawable")]
        public long BestTorsoDrawable { get; set; }

        [JsonProperty("BestTorsoTexture")]
        public long BestTorsoTexture { get; set; }
    }

    public partial class BestTorso
    {
        public static Dictionary<string, Dictionary<string, BestTorso>> FromJson(string json) => JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, BestTorso>>>(json, Converter.Settings);
    }

    #endregion Roots Clothing Stuff

    public class Clothes
    {
        private const string CLOTHES_ITEM_ID = "ITEM_CLOTHES";
        private const string ACCESSORIES_ITEM_ID = "ITEM_CLOTHES_ACCESSORY";

        public static void LoadTorsoData()
        {
            #region Copies Clothing From Our Code to JSON

            /* KEEP THIS IN - Makes .JSON files from code
            Dictionary<clothesData, ClothesInfo> clothesData = new Dictionary<clothesData, ClothesInfo>
             {
             };

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.ContractResolver = new DictionaryAsArrayResolver();

            ClothesJson newJson = new ClothesJson();

            newJson.clothingItem = clothesData;

            var jsonOutput = JsonConvert.SerializeObject(newJson, settings);

            File.WriteAllText("glasses.json", jsonOutput);*/

            #endregion Copies Clothing From Our Code to JSON

            string torsoDirectory = "data/clothing/torso";

            if (Directory.Exists(torsoDirectory))
            {
                string[] torsoFiles = Directory.GetFiles(torsoDirectory);

                foreach (string file in torsoFiles)
                {
                    if (file == $"{torsoDirectory}\\besttorso_male.json")
                    {
                        MaleTopToTorso = new Dictionary<int, int>();

                        string content = File.ReadAllText(file);

                        Dictionary<string, Dictionary<string, BestTorso>> bestTorso = BestTorso.FromJson(content);

                        foreach (KeyValuePair<string, Dictionary<string, BestTorso>> torsoInfo in bestTorso)
                        {
                            foreach (KeyValuePair<string, BestTorso> torsoData in torsoInfo.Value)
                            {
                                // Torso Id
                                int key = Convert.ToInt32(torsoInfo.Key);

                                if (MaleTopToTorso.ContainsKey(key)) continue;

                                // Best Torso Drawable
                                int value = Convert.ToInt32(torsoData.Value.BestTorsoDrawable);

                                MaleTopToTorso.Add(key, value);
                            }
                        }
                    }

                    if (file == $"{torsoDirectory}\\besttorso_female.json")
                    {
                        FemaleTopToTorso = new Dictionary<int, int>();

                        string content = File.ReadAllText(file);

                        Dictionary<string, Dictionary<string, BestTorso>> bestTorso = BestTorso.FromJson(content);

                        foreach (KeyValuePair<string, Dictionary<string, BestTorso>> torsoInfo in bestTorso)
                        {
                            foreach (KeyValuePair<string, BestTorso> torsoData in torsoInfo.Value)
                            {
                                // Torso Id
                                int key = Convert.ToInt32(torsoInfo.Key);

                                if (FemaleTopToTorso.ContainsKey(key)) continue;

                                // Best Torso Drawable
                                int value = Convert.ToInt32(torsoData.Value.BestTorsoDrawable);

                                FemaleTopToTorso.Add(key, value);
                            }
                        }
                    }
                }
            }
        }

        public static void LoadClothingItems()
        {
            Console.WriteLine($"Fetching Clothing Items");

            DictClothesInfo = new Dictionary<ClothesData, ClothesInfo>();

            DictClothesInfo = LoadClothes();

            Console.WriteLine($"Loaded {DictClothesInfo.Count} clothing items.");

            Console.WriteLine("Fetching Clothing Accessories");

            DictAccessoriesInfo = new Dictionary<ClothesData, ClothesInfo>();

            DictAccessoriesInfo = LoadAccessories();

            Console.WriteLine($"Loaded {DictAccessoriesInfo.Count} accessory items.");

            LoadTorsoData();

            Console.WriteLine($"Loaded {FemaleTopToTorso.Count + MaleTopToTorso.Count} Torso items");

            LoadBannedClothingNames();
        }

        public static void LoadBannedClothingNames()
        {
            try
            {
                if (File.Exists($"data/clothing/BannedClothingNames.json"))
                {
                    using StreamReader sr = new StreamReader("data/clothing/BannedClothingNames.json");

                    string contents = sr.ReadToEnd();

                    sr.Dispose();

                    List<string> bannedClothingNames = JsonConvert.DeserializeObject<List<string>>(contents);

                    ClothingCommand.BannedClothingNames = bannedClothingNames;

                    Console.WriteLine($"Loaded {bannedClothingNames.Count} Banned Clothing Names (Won't be added on switching)");
                    return;
                }

                File.WriteAllText("data/clothing/BannedClothingNames.json", JsonConvert.SerializeObject(ClothingCommand.BannedClothingNames, Formatting.Indented));

                Console.WriteLine($"Loaded {ClothingCommand.BannedClothingNames.Count} Banned Clothing Names (Won't be added on switching)");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        private static Dictionary<ClothesData, ClothesInfo> LoadClothes()
        {
            string ClothesDirectory = "data/clothing/clothing";

            int topCount = 0;

            try
            {
                if (Directory.Exists(ClothesDirectory))
                {
                    Dictionary<ClothesData, ClothesInfo> clothesData = new Dictionary<ClothesData, ClothesInfo>();

                    string[] clothingFiles = Directory.GetFiles(ClothesDirectory);

                    Console.WriteLine($"Found {clothingFiles.Length} Clothing Files");

                    foreach (string clothingItem in clothingFiles)
                    {
                        string content = File.ReadAllText($"{clothingItem}");

                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            ContractResolver = new DictionaryAsArrayResolver()
                        };

                        ClothesJson result = JsonConvert.DeserializeObject<ClothesJson>(content, settings);

                        foreach (KeyValuePair<ClothesData, ClothesInfo> info in result.clothingItem)
                        {
                            if (info.Key.slot == 11)
                            {
                                topCount++;
                            }
                            clothesData.Add(info.Key, info.Value);
                        }
                    }

                    return clothesData;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching Clothing Data. Ensure Clothing Files are placed correctly!");
                Console.WriteLine(e);
                return null;
            }
        }

        private static Dictionary<ClothesData, ClothesInfo> LoadAccessories()
        {
            string ClothesDirectory = "data/clothing/props";

            try
            {
                if (Directory.Exists(ClothesDirectory))
                {
                    Dictionary<ClothesData, ClothesInfo> accessoryDictionary = new Dictionary<ClothesData, ClothesInfo>();

                    string[] clothingFiles = Directory.GetFiles(ClothesDirectory);

                    Console.WriteLine($"Found {clothingFiles.Length} Accessory Files");

                    foreach (string clothingItem in clothingFiles)
                    {
                        Console.WriteLine($"Loading {clothingItem}");

                        string content = File.ReadAllText($"{clothingItem}");

                        JsonSerializerSettings settings = new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            ContractResolver = new DictionaryAsArrayResolver()
                        };

                        ClothesJson result = JsonConvert.DeserializeObject<ClothesJson>(content, settings);

                        foreach (KeyValuePair<ClothesData, ClothesInfo> info in result.clothingItem)
                        {
                            accessoryDictionary.Add(info.Key, info.Value);
                        }
                    }

                    return accessoryDictionary;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching Clothing Data. Ensure Clothing Files are placed correctly!");
                Console.WriteLine(e);
                return null;
            }
        }

        #region data lists

        #region clothes info

        // { new clothesData((int)ClothesType.Undershirt, 17, 0), new ClothesInfo("Basic Bodyarmour","Basic Bodyarmour", 1000) },

        public static Dictionary<ClothesData, ClothesInfo> DictClothesInfo = new Dictionary<ClothesData, ClothesInfo>();

        public static Dictionary<ClothesData, ClothesInfo> DictAccessoriesInfo = new Dictionary<ClothesData, ClothesInfo>();

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

        #endregion clothes info

        private static Dictionary<int, ClothesData> dictMaleDefaultClothing = new Dictionary<int, ClothesData>()
        {
            {1, new ClothesData(1, 0,0) },
            //{4, new clothesData(4, 61, 0) },
            {4, new ClothesData(4, 0, 0) },
            {5,  new ClothesData(5, 0,0)},
            {6,  new ClothesData(6,1,0)},
            {7,  new ClothesData(7, 0,0) },
            {8,  new ClothesData(8, 122, 1)},
            {9, new ClothesData(9, 0,0) },
            {10, new ClothesData(10, 0,0) },
            {11, new ClothesData(11, 16,0) }
        };

        private static Dictionary<int, ClothesData> dictFemaleDefaultClothing = new Dictionary<int, ClothesData>()
        {
            {1, new ClothesData(1, 0,0) },
            {4, new ClothesData(4, 0, 0) },
            {5,  new ClothesData(5, 0,0)},
            {6,  new ClothesData(6,0,0)},
            {7,  new ClothesData(7, 0,0) },
            {8,  new ClothesData(8, 152, 1)},
            {9, new ClothesData(9, 0,0) },
            {10, new ClothesData(10, 0,0) },
            {11, new ClothesData(11, 0 ,0) }
        };

        #endregion data lists

        public static void LoadClothes(IPlayer player, List<ClothesData> clothes, List<AccessoryData> accessories)
        {
            // 1st key - Clothing Slot (Tops, Feet)
            // 1st value - Dictionary<int, int>
            // 2nd Key - Clothing Drawable
            // 2nd Value - Clothing Texture
            Dictionary<int, Dictionary<int, int>> clothingData = new Dictionary<int, Dictionary<int, int>>();

            if (!clothes.Any())
            {
                List<ClothesData> defaultClothesDatas = new List<ClothesData>();

                //Load Default Clothes
                for (int i = 1; i <= 11; i++)
                {
                    ClothesData def = GetDefaultClothing(i, player.FetchCharacter().Sex == 0);
                    if (def != null)
                    {
                        defaultClothesDatas.Add(def);
                        player.SetClothes(def.slot, def.drawable, def.texture);

                        clothingData.Add(def.slot, new Dictionary<int, int> { { def.drawable, def.texture } });

                        if (def.slot == (int)ClothesType.Top)
                        {
                            int torso = GetTorsoDataForTop(def.drawable, player.FetchCharacter().Sex == 0);
                            clothingData.Add((int)ClothesType.Torso, new Dictionary<int, int> { { torso, 0 } });
                            player.SetClothes((int)ClothesType.Torso, torso, 0);
                        }
                    }
                }

                using Context context = new Context();
                Models.Character dbCharacter = context.Character.Find(player.FetchCharacterId());

                dbCharacter.ClothesJson = JsonConvert.SerializeObject(defaultClothesDatas);

                context.SaveChanges();

                LoadClothes(player, defaultClothesDatas, accessories);

                return;
            }

            foreach (ClothesData data in clothes)
            {
                // Player is already created - Clothes exist

                clothingData.Add(data.slot, new Dictionary<int, int> { { data.drawable, data.texture } });

                player.SetClothes(data.slot, data.drawable, data.texture);

                if (data.slot == (int)ClothesType.Top)
                {
                    int torso = GetTorsoDataForTop(data.drawable, player.FetchCharacter().Sex == 0);

                    clothingData.Add((int)ClothesType.Torso, new Dictionary<int, int> { { torso, 0 } });

                    player.SetClothes((int)ClothesType.Torso, torso, 0);
                }
            }

            player.SetAccessory(0, -1, 0);
            player.SetAccessory(1, -1, 0);
            player.SetAccessory(2, -1, 0);
            player.SetAccessory(6, -1, 0);
            player.SetAccessory(7, -1, 0);

            foreach (AccessoryData data in accessories)
            {
                player.SetAccessory(data.slot, data.drawable, data.texture);
            }

            string clothingJson = JsonConvert.SerializeObject(clothingData);

            player.DeleteData("CLOTHINGDATA");

            player.SetData("CLOTHINGDATA", clothingJson);

            string accessoryJson = JsonConvert.SerializeObject(accessories);

            player.DeleteData("ACCESSORYDATA");
            player.SetData("ACCESSORYDATA", accessoryJson);

            //player.Position = player.Position;

            //player.TriggerEvent("LoadClothing", clothingJson, accessoryJson);
        }

        public static ClothesData GetDefaultClothing(int slot, bool male)
        {
            if (male)
            {
                if (dictMaleDefaultClothing.ContainsKey(slot))
                {
                    ClothesData clothing = dictMaleDefaultClothing[slot];
                    clothing.male = true;
                    return clothing;
                }
            }
            else
            {
                if (dictFemaleDefaultClothing.ContainsKey(slot))
                {
                    ClothesData foundData = dictFemaleDefaultClothing[slot];
                    ClothesData d = new ClothesData(foundData.slot, foundData.drawable, foundData.texture, false);
                    return d;
                }
            }
            return null;
        }

        /// <summary>
        /// returns the correct torso texture, if not defined returns default one
        /// </summary>
        /// <returns></returns>
        public static int GetTorsoDataForTop(int top, bool isMale)
        {
            // Returns previous 15 if not found or was -1. Changed to 0 (27/12/19 - Moretti)
            if (isMale)
            {
                bool contains = MaleTopToTorso.ContainsKey(top);
                if (!contains) return 0;

                int value = MaleTopToTorso.FirstOrDefault(x => x.Key == top).Value;

                if (value == -1)
                {
                    value = 0;
                }

                return value;
            }

            bool femaleContains = FemaleTopToTorso.ContainsKey(top);
            if (!femaleContains) return 0;

            int fValue = FemaleTopToTorso.FirstOrDefault(x => x.Key == top).Value;

            if (fValue == -1)
            {
                fValue = 0;
            }

            return fValue;
        }

        public static int GetPricesForClothing(ClothesType type, int drawable)
        {
            ClothesData data = DictClothesInfo.Keys.FirstOrDefault(i => i.slot == (int)type && i.drawable == drawable);
            if (data == null) return 50;
            return DictClothesInfo[data].Price;
        }

        public static string GetClothesName(ClothesType type, int drawable, int texture, bool male)
        {
            ClothesData data = DictClothesInfo.Keys.FirstOrDefault(i => i.slot == (int)type && i.drawable == drawable && i.texture == texture);
            if (data == null) return "undefined";

            ClothesInfo clothesInfo = DictClothesInfo[data];

            if (male)
            {
                return clothesInfo.DisplayNameMale;
            }

            return clothesInfo.DisplayNameFemale;
        }

        public static InventoryItem ConvertClothesToInventoryItem(ClothesData data, bool male)
        {
            return new InventoryItem(CLOTHES_ITEM_ID, GetClothesName((ClothesType)data.slot, data.drawable, data.texture, male), JsonConvert.SerializeObject(data), 1);
        }

        public static InventoryItem ConvertAccessoryToInventoryItem(AccessoryData data, bool male)
        {
            return new InventoryItem(ACCESSORIES_ITEM_ID, GetAccessoryName((AccessoriesType)data.slot, data.drawable, data.texture, male), JsonConvert.SerializeObject(data), 1);
        }

        public static InventoryItem ConvertAccessoryToInventoryItem(ClothesData data, bool male)
        {
            return new InventoryItem(ACCESSORIES_ITEM_ID, GetAccessoryName((AccessoriesType)data.slot, data.drawable, data.texture, male), JsonConvert.SerializeObject(data), 1);
        }

        public AccessoryData GetAccessoriesData(IPlayer player, int slot)
        {
            List<AccessoryData> accessory =
                JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson);
            return accessory.FirstOrDefault(i => i.slot == slot);
        }

        public ClothesData GetClothesData(IPlayer player, int slot)
        {
            List<ClothesData> clothes =
                JsonConvert.DeserializeObject<List<ClothesData>>(player.FetchCharacter().ClothesJson);
            return clothes.FirstOrDefault(i => i.slot == slot);
        }

        public static string GetAccessoryName(AccessoriesType type, int drawable, int texture, bool male)
        {
            ClothesData data = DictAccessoriesInfo.Keys.FirstOrDefault(i => i.slot == (int)type && i.drawable == drawable && i.texture == texture);
            if (data == null) return "undefined";
            return male ? DictAccessoriesInfo[data].DisplayNameMale : DictAccessoriesInfo[data].DisplayNameFemale;
        }

        public InventoryItem ConvertToClothesItem(IPlayer player, ClothesType type)
        {
            ClothesData data = GetClothesData(player, (int)type);
            if (data == null) return null;
            InventoryItem item = new InventoryItem(CLOTHES_ITEM_ID, GetClothesName((ClothesType)data.slot, data.drawable, data.texture, player.FetchCharacter().Sex == 0), JsonConvert.SerializeObject(data), 1);
            return item;
        }

        public static ClothesData ConvertItemToClothesData(InventoryItem item)
        {
            try
            {
                ClothesData data = JsonConvert.DeserializeObject<ClothesData>(item.ItemValue);
                return data;
            }
            catch
            {
                return null;
            }
        }

        public static void SetClothes(IPlayer player, ClothesData data)
        {
            player.GetData("CLOTHINGDATA", out string clothesJson);

            if (!string.IsNullOrEmpty(clothesJson)) return;

            Dictionary<int, Dictionary<int, int>> clothesData =
                JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, int>>>(clothesJson);

            player.SetClothes(data.slot, data.drawable, data.texture);
            if (data.slot == (int)ClothesType.Top)
            {
                int torso = GetTorsoDataForTop(data.drawable, player.FetchCharacter().Sex == 0);
                player.SetClothes((int)ClothesType.Torso, torso, 0);

                var oldTorsoData = clothesData.FirstOrDefault(i => i.Key == (int)ClothesType.Torso);

                clothesData.Remove(oldTorsoData.Key);

                clothesData.Add((int)ClothesType.Torso, new Dictionary<int, int> { { torso, 0 } });
            }

            var pair = clothesData.FirstOrDefault(i => i.Key == data.slot);

            var newValue = new Dictionary<int, int> { { data.drawable, data.texture } };

            clothesData.Remove(pair.Key);

            clothesData.Add(data.slot, newValue);

            string dataJson = JsonConvert.SerializeObject(clothesData);

            player.SetData("CLOTHINGDATA", dataJson);

            if (!player.IsInVehicle)
            {
                player.Position = player.Position;
            }
        }

        public static void SetAccessories(IPlayer player, AccessoryData data)
        {
            try
            {
                player.GetData("ACCESSORYDATA", out string accessoryJson);

                if (string.IsNullOrEmpty(accessoryJson)) return;

                List<AccessoryData> accessoryDatas = JsonConvert.DeserializeObject<List<AccessoryData>>(accessoryJson);

                player.SetAccessory(data.slot, data.drawable, data.texture);

                var oldDataList = JsonConvert.DeserializeObject<List<AccessoryData>>(player.FetchCharacter().AccessoryJson);

                var pair = oldDataList.FirstOrDefault(i => i.slot == data.slot);

                oldDataList.Remove(pair);

                oldDataList.Add(data);

                using (Context context = new Context())
                {
                    context.Character.Find(player.FetchCharacterId()).AccessoryJson =
                        JsonConvert.SerializeObject(oldDataList);

                    context.SaveChanges();
                }

                accessoryDatas.Remove(pair);

                accessoryDatas.Add(data);

                player.SetData("ACCESSORYDATA", JsonConvert.SerializeObject(accessoryDatas));

                if (!player.IsInVehicle)
                {
                    player.Position = player.Position;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Adds an individual accessory to the characters data
        /// </summary>
        /// <param name="player"></param>
        /// <param name="accessoryData"></param>
        public static void SaveAccessories(IPlayer player, AccessoryData accessoryData)
        {
            try
            {
                using Context context = new Context();
                Models.Character dbCharacter = context.Character.Find(player.FetchCharacterId());

                if (dbCharacter.Sex == 0)
                {
                    accessoryData.male = true;
                }

                List<AccessoryData> accessory =
                    JsonConvert.DeserializeObject<List<AccessoryData>>(dbCharacter.AccessoryJson);

                var selectedAccessory = accessory.FirstOrDefault(i => i.slot == accessoryData.slot);

                accessory.Remove(selectedAccessory);
                accessory.Add(accessoryData);

                dbCharacter.AccessoryJson = JsonConvert.SerializeObject(accessory);

                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Adds an individual piece of clothing to the characters data.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="clothesData"></param>
        public static void SaveClothes(IPlayer player, ClothesData clothesData)
        {
            using Context context = new Context();
            Models.Character dbCharacter = context.Character.Find(player.FetchCharacterId());

            if (dbCharacter.Sex == 0)
            {
                clothesData.male = true;
            }

            List<ClothesData> clothes =
                JsonConvert.DeserializeObject<List<ClothesData>>(dbCharacter.ClothesJson);

            var selectedClothes = clothes.FirstOrDefault(i => i.slot == clothesData.slot);

            clothes.Remove(selectedClothes);

            clothes.Add(clothesData);

            dbCharacter.ClothesJson = JsonConvert.SerializeObject(clothes);

            context.SaveChanges();

            LoadClothes(player, clothes, JsonConvert.DeserializeObject<List<AccessoryData>>(dbCharacter.AccessoryJson));
        }
    }
}