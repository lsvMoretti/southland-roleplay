using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Extensions;

namespace Server.Character.Tattoo
{
    public class TattooHandler
    {
        public static Dictionary<TattooData, string> TattooList { get; set; }

        public static void InitTattoos()
        {
            try
            {
                if (!Directory.Exists(@"data\tattoo")) return;

                TattooList = new Dictionary<TattooData, string>();

                string[] tattooFiles = Directory.GetFiles(@"data\tattoo");

                Console.WriteLine($"Found {tattooFiles.Length} Tattoo Files.");

                foreach (string tattooFile in tattooFiles)
                {
                    try
                    {
                        string content = File.ReadAllText(tattooFile);

                        List<TattooData> tattooDataList = JsonConvert.DeserializeObject<List<TattooData>>(content);

                        string filename = tattooFile.Split(new char[] { '\\', '.' })[2];

                        foreach (TattooData tattooData in tattooDataList)
                        {
                            TattooList.Add(tattooData, filename);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }
                }

                Console.WriteLine($"Loaded {TattooList.Count} Tattoo's");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        #region Hair Tattoo Data

        public static List<HairTattooData> MaleHairTattoos = new List<HairTattooData>
        {
            {new HairTattooData{HairId = 0, Collection = "mpbeach_overlays", Overlay = "FM_Hair_Fuzz"} },
            {new HairTattooData{HairId = 1, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_001"} },
            {new HairTattooData{HairId = 2, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_002"} },
            {new HairTattooData{HairId = 3, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_003"} },
            {new HairTattooData{HairId = 4, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_004"} },
            {new HairTattooData{HairId = 5, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_005"} },
            {new HairTattooData{HairId = 6, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_006"} },
            {new HairTattooData{HairId = 7, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_007"} },
            {new HairTattooData{HairId = 8, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_008"} },
            {new HairTattooData{HairId = 9, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_009"} },
            {new HairTattooData{HairId = 10, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_013"} },
            {new HairTattooData{HairId = 11, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_002"} },
            {new HairTattooData{HairId = 12, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_011"} },
            {new HairTattooData{HairId = 13, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_012"} },
            {new HairTattooData{HairId = 14, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_014"} },
            {new HairTattooData{HairId = 15, Collection = "multiplayer_overlays", Overlay = "NG_M_Hair_015"} },
            {new HairTattooData{HairId = 16, Collection = "multiplayer_overlays", Overlay = "NGBea_M_Hair_000"} },
            {new HairTattooData{HairId = 17, Collection = "multiplayer_overlays", Overlay = "NGBea_M_Hair_001"} },
            {new HairTattooData{HairId = 18, Collection = "multiplayer_overlays", Overlay = "NGBus_M_Hair_000"} },
            {new HairTattooData{HairId = 19, Collection = "multiplayer_overlays", Overlay = "NGBus_M_Hair_002"} },
            {new HairTattooData{HairId = 20, Collection = "multiplayer_overlays", Overlay = "NGHip_M_Hair_000"} },
            {new HairTattooData{HairId = 21, Collection = "multiplayer_overlays", Overlay = "NGHip_M_Hair_001"} },
            {new HairTattooData{HairId = 22, Collection = "multiplayer_overlays", Overlay = "NGInd_M_Hair_000"} },
            {new HairTattooData{HairId = 24, Collection = "mplowrider_overlays", Overlay = "LR_M_Hair_000"} },
            {new HairTattooData{HairId = 25, Collection = "mplowrider_overlays", Overlay = "LR_M_Hair_001"} },
            {new HairTattooData{HairId = 26, Collection = "mplowrider_overlays", Overlay = "LR_M_Hair_002"} },
            {new HairTattooData{HairId = 27, Collection = "mplowrider_overlays", Overlay = "LR_M_Hair_003"} },
            {new HairTattooData{HairId = 28, Collection = "mplowrider2_overlays", Overlay = "LR_M_Hair_004"} },
            {new HairTattooData{HairId = 29, Collection = "mplowrider2_overlays", Overlay = "LR_M_Hair_005"} },
            {new HairTattooData{HairId = 30, Collection = "mplowrider2_overlays", Overlay = "LR_M_Hair_006"} },
            {new HairTattooData{HairId = 31, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_000_M"} },
            {new HairTattooData{HairId = 32, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_001_M"} },
            {new HairTattooData{HairId = 33, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_002_M"} },
            {new HairTattooData{HairId = 34, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_003_M"} },
            {new HairTattooData{HairId = 35, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_004_M"} },
            {new HairTattooData{HairId = 36, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_005_M"} },
            {new HairTattooData{HairId = 72, Collection = "mpgunrunning_overlays", Overlay = "MP_Gunrunning_Hair_M_000_M"} },
            {new HairTattooData{HairId = 73, Collection = "mpgunrunning_overlays", Overlay = "MP_Gunrunning_Hair_M_001_M"} },
        };

        public static List<HairTattooData> FemaleHairTattoos = new List<HairTattooData>
        {
            {new HairTattooData{HairId = 0, Collection = "mpbeach_overlays", Overlay = "FM_Hair_Fuzz"} },
            {new HairTattooData{HairId = 1, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_001"} },
            {new HairTattooData{HairId = 2, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_002"} },
            {new HairTattooData{HairId = 3, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_003"} },
            {new HairTattooData{HairId = 4, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_004"} },
            {new HairTattooData{HairId = 5, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_005"} },
            {new HairTattooData{HairId = 6, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_006"} },
            {new HairTattooData{HairId = 7, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_007"} },
            {new HairTattooData{HairId = 8, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_008"} },
            {new HairTattooData{HairId = 9, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_009"} },
            {new HairTattooData{HairId = 10, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_010"} },
            {new HairTattooData{HairId = 11, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_011"} },
            {new HairTattooData{HairId = 12, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_012"} },
            {new HairTattooData{HairId = 13, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_013"} },
            {new HairTattooData{HairId = 14, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_014"} },
            {new HairTattooData{HairId = 15, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_015"} },
            {new HairTattooData{HairId = 16, Collection = "multiplayer_overlays", Overlay = "NGBea_F_Hair_000"} },
            {new HairTattooData{HairId = 17, Collection = "multiplayer_overlays", Overlay = "NGBea_F_Hair_001"} },
            {new HairTattooData{HairId = 18, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_007"} },
            {new HairTattooData{HairId = 19, Collection = "multiplayer_overlays", Overlay = "NGBus_F_Hair_000"} },
            {new HairTattooData{HairId = 20, Collection = "multiplayer_overlays", Overlay = "NGBus_F_Hair_001"} },
            {new HairTattooData{HairId = 21, Collection = "multiplayer_overlays", Overlay = "NGBea_F_Hair_001"} },
            {new HairTattooData{HairId = 22, Collection = "multiplayer_overlays", Overlay = "NGHip_F_Hair_000"} },
            {new HairTattooData{HairId = 23, Collection = "multiplayer_overlays", Overlay = "NGInd_F_Hair_000"} },
            {new HairTattooData{HairId = 25, Collection = "mplowrider_overlays", Overlay = "LR_F_Hair_000"} },
            {new HairTattooData{HairId = 26, Collection = "mplowrider_overlays", Overlay = "LR_F_Hair_001"} },
            {new HairTattooData{HairId = 27, Collection = "mplowrider_overlays", Overlay = "LR_F_Hair_002"} },
            {new HairTattooData{HairId = 28, Collection = "mplowrider2_overlays", Overlay = "LR_F_Hair_003"} },
            {new HairTattooData{HairId = 29, Collection = "mplowrider2_overlays", Overlay = "LR_F_Hair_003"} },
            {new HairTattooData{HairId = 30, Collection = "mplowrider2_overlays", Overlay = "LR_F_Hair_004"} },
            {new HairTattooData{HairId = 31, Collection = "mplowrider2_overlays", Overlay = "LR_F_Hair_006"} },
            {new HairTattooData{HairId = 32, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_000_F"} },
            {new HairTattooData{HairId = 33, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_001_F"} },
            {new HairTattooData{HairId = 34, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_002_F"} },
            {new HairTattooData{HairId = 35, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_003_F"} },
            {new HairTattooData{HairId = 36, Collection = "multiplayer_overlays", Overlay = "NG_F_Hair_003"} },
            {new HairTattooData{HairId = 37, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_006_F"} },
            {new HairTattooData{HairId = 38, Collection = "mpbiker_overlays", Overlay = "MP_Biker_Hair_004_F"} },
            {new HairTattooData{HairId = 72, Collection = "mpgunrunning_overlays", Overlay = "MP_Gunrunning_Hair_F_000_F"} },
            {new HairTattooData{HairId = 73, Collection = "mpgunrunning_overlays", Overlay = "MP_Gunrunning_Hair_F_001_F"} },
        };

        public static HairTattooData FetchHairTattooData(int hair, bool isMale)
        {
            if (isMale)
            {
                return MaleHairTattoos.FirstOrDefault(i => i.HairId == hair);
            }

            return FemaleHairTattoos.FirstOrDefault(i => i.HairId == hair);
        }

        #endregion Hair Tattoo Data

        public static void LoadTattoos(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null) return;

            player.Emit("ClearTattoos");

            CustomCharacter customCharacter =
                JsonConvert.DeserializeObject<CustomCharacter>(playerCharacter.CustomCharacter);

            HairInfo hairInfo = JsonConvert.DeserializeObject<HairInfo>(customCharacter.Hair);

            HairTattooData hairTattooData = FetchHairTattooData(hairInfo.Hair, player.GetClass().IsMale);

            if (hairTattooData != null)
            {
                player.Emit("loadTattooData", hairTattooData.Collection, hairTattooData.Overlay);
            }

            if (string.IsNullOrEmpty(playerCharacter.TattooJson)) return;

            List<TattooData> tattooList = JsonConvert.DeserializeObject<List<TattooData>>(playerCharacter.TattooJson);

            if (!tattooList.Any()) return;

            foreach (TattooData tattooData in tattooList)
            {
                KeyValuePair<TattooData, string>? selectedTattooData = TattooList.FirstOrDefault(s => s.Key.Name == tattooData.Name);

                string collection = selectedTattooData.Value;

                string overlay = player.GetClass().IsMale ? selectedTattooData.Key.HashNameMale : selectedTattooData.Key.HashNameFemale;

                player.Emit("loadTattooData", collection, overlay);
            }
        }
    }
}