using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Data;
using Newtonsoft.Json;

namespace Server.Property
{
    public class Interiors
    {
        /// <summary>
        /// The list of all loaded Interiors
        /// </summary>
        public static List<Interiors> InteriorList = new List<Interiors>();

#if RELEASE
        private static string altVDirectory = "D:/servers/Paradigm-Dev/data";
#endif

#if DEBUG
        private static string altVDirectory = "D:/servers/Paradigm-Dev/data";
#endif

        /// <summary>
        /// Loads all interiors (On start)
        /// </summary>
        public static void LoadInteriors()
        {
            try
            {
                InteriorList = new List<Interiors>();

                if (!File.Exists($"{altVDirectory}/interiors.json"))
                {
                    Console.WriteLine($"An error occurred. interiors.json file is empty!");
                    Console.WriteLine($"Creating Default Interior List");

                    #region Default Interiors

                    AddInterior("Modern 1 Apartment", new Position(-786.8663f, 315.7642f, 217.6385f), "apa_v_mp_h_01_a");
                    AddInterior("Modern 2 Apartment", new Position(-786.9563f, 315.6229f, 187.9136f), "apa_v_mp_h_01_c");
                    AddInterior("Modern 3 Apartment", new Position(-774.0126f, 342.0428f, 196.6864f), "apa_v_mp_h_01_b");
                    AddInterior("Mody 1 Apartment", new Position(-787.0749f, 315.8198f, 217.6386f), "apa_v_mp_h_02_a");
                    AddInterior("Mody 2 Apartment", new Position(-786.8195f, 315.5634f, 187.9137f), "apa_v_mp_h_02_c");
                    AddInterior("Mody 3 Apartment", new Position(-774.1382f, 342.0316f, 196.6864f), "apa_v_mp_h_02_b");
                    AddInterior("Vibrant 1 Apartment", new Position(-786.6245f, 315.6175f, 217.6385f), "apa_v_mp_h_03_a");
                    AddInterior("Vibrant 2 Apartment", new Position(-786.9584f, 315.7974f, 187.9135f), "apa_v_mp_h_03_c");
                    AddInterior("Vibrant 3 Apartment", new Position(-774.0223f, 342.1718f, 196.6863f), "apa_v_mp_h_03_b");
                    AddInterior("Sharp 1 Apartment", new Position(-787.0902f, 315.7039f, 217.6384f), "apa_v_mp_h_04_a");
                    AddInterior("Sharp 2 Apartment", new Position(-787.0902f, 315.7039f, 217.6384f), "apa_v_mp_h_04_c");
                    AddInterior("Sharp 3 Apartment", new Position(-787.0902f, 315.7039f, 217.6384f), "apa_v_mp_h_04_b");
                    AddInterior("Monochrome 1 Apartment", new Position(-786.9887f, 315.7393f, 217.6386f), "apa_v_mp_h_05_a");
                    AddInterior("Monochrome 2 Apartment", new Position(-786.8809f, 315.6634f, 187.9136f), "apa_v_mp_h_05_c");
                    AddInterior("Monochrome 3 Apartment", new Position(-774.0675f, 342.0773f, 196.6864f), "apa_v_mp_h_05_b");
                    AddInterior("Seductive 1 Apartment", new Position(-787.1423f, 315.6943f, 217.6384f), "apa_v_mp_h_06_a");
                    AddInterior("Seductive 2 Apartment", new Position(-787.0961f, 315.815f, 187.9135f), "apa_v_mp_h_06_c");
                    AddInterior("Seductive 3 Apartment", new Position(-773.9552f, 341.9892f, 196.6862f), "apa_v_mp_h_06_b");
                    AddInterior("Regal 1 Apartment", new Position(-787.029f, 315.7113f, 217.6385f), "apa_v_mp_h_07_a");
                    AddInterior("Regal 2 Apartment", new Position(-787.0574f, 315.6567f, 187.9135f), "apa_v_mp_h_07_c");
                    AddInterior("Regal 3 Apartment", new Position(-774.0109f, 342.0965f, 196.6863f), "apa_v_mp_h_07_b");
                    AddInterior("Aqua 1 Apartment", new Position(-786.9469f, 315.5655f, 217.6383f), "apa_v_mp_h_08_a");
                    AddInterior("Aqua 2 Apartment", new Position(-786.9756f, 315.723f, 187.9134f), "apa_v_mp_h_08_c");
                    AddInterior("Aqua 3 Apartment", new Position(-774.0349f, 342.0296f, 196.6862f), "apa_v_mp_h_08_b");

                    //Arcadius Business Centre

                    AddInterior("Executive Rich", new Position(-141.1987f, -620.913f, 168.8205f), "ex_dt1_02_office_02b");
                    AddInterior("Executive Cool", new Position(-141.5429f, -620.9524f, 168.8204f), "ex_dt1_02_office_02c");
                    AddInterior("Executive Contrast", new Position(-141.2896f, -620.9618f, 168.8204f), "ex_dt1_02_office_02a");
                    AddInterior("Old Spice Warm", new Position(-141.4966f, -620.8292f, 168.8204f), "ex_dt1_02_office_01a");
                    AddInterior("Old Spice Classical", new Position(-141.3997f, -620.9006f, 168.8204f), "ex_dt1_02_office_01b");
                    AddInterior("Old Spice Vintage", new Position(-141.5361f, -620.9186f, 168.8204f), "ex_dt1_02_office_01c");
                    AddInterior("Power Broker Ice", new Position(-141.392f, -621.0451f, 168.8204f), "ex_dt1_02_office_03a");
                    AddInterior("Power Broker Conservative", new Position(-141.1945f, -620.8729f, 168.8204f), "ex_dt1_02_office_03b");
                    AddInterior("Power Broker Polished", new Position(-141.4924f, -621.0035f, 168.8205f), "ex_dt1_02_office_03c");
                    AddInterior("Garage 1", new Position(-191.0133f, -579.1428f, 135.0000f), "imp_dt1_02_cargarage_a");
                    AddInterior("Garage 2", new Position(-117.4989f, -568.1132f, 135.0000f), "imp_dt1_02_cargarage_b");
                    AddInterior("Garage 3", new Position(-136.0780f, -630.1852f, 135.0000f), "imp_dt1_02_cargarage_c");
                    AddInterior("Mod Shop", new Position(-146.6166f, -596.6301f, 166.0000f), "imp_dt1_02_modgarage");

                    // Maze Bank Building

                    AddInterior("Executive Rich", new Position(-75.8466f, -826.9893f, 243.3859f), "ex_dt1_11_office_02b");
                    AddInterior("Executive Cool", new Position(-75.49945f, -827.05f, 243.386f), "ex_dt1_11_office_02c");
                    AddInterior("Executive Contrast", new Position(-75.49827f, -827.1889f, 243.386f), "ex_dt1_11_office_02a");
                    AddInterior("Old Spice Warm", new Position(-75.44054f, -827.1487f, 243.3859f), "ex_dt1_11_office_01a");
                    AddInterior("Old Spice Classical", new Position(-75.63942f, -827.1022f, 243.3859f), "ex_dt1_11_office_01b");
                    AddInterior("Old Spice Vintage", new Position(-75.47446f, -827.2621f, 243.386f), "ex_dt1_11_office_01c");
                    AddInterior("Power Broker Ice", new Position(-75.56978f, -827.1152f, 243.3859f), "ex_dt1_11_office_03a");
                    AddInterior("Power Broker Conservative", new Position(-75.51953f, -827.0786f, 243.3859f), "ex_dt1_11_office_03b");
                    AddInterior("Power Broker Polished", new Position(-75.41915f, -827.1118f, 243.3858f), "ex_dt1_11_office_03c");
                    AddInterior("Garage 1", new Position(-84.2193f, -823.0851f, 221.0000f), "imp_dt1_11_cargarage_a");
                    AddInterior("Garage 2", new Position(-69.8627f, -824.7498f, 221.0000f), "imp_dt1_11_cargarage_b");
                    AddInterior("Garage 3", new Position(-80.4318f, -813.2536f, 221.0000f), "imp_dt1_11_cargarage_c");
                    AddInterior("Mod Shop", new Position(-73.9039f, -821.6204f, 284.0000f), "imp_dt1_11_modgarage");

                    //Lom Bank

                    AddInterior("Executive Rich", new Position(-1579.756f, -565.0661f, 108.523f), "ex_sm_13_office_02b");
                    AddInterior("Executive Cool", new Position(-1579.678f, -565.0034f, 108.5229f), "ex_sm_13_office_02c");
                    AddInterior("Executive Contrast", new Position(-1579.583f, -565.0399f, 108.5229f), "ex_sm_13_office_02a");
                    AddInterior("Old Spice Warm", new Position(-1579.702f, -565.0366f, 108.5229f), "ex_sm_13_office_01a");
                    AddInterior("Old Spice Classical", new Position(-1579.643f, -564.9685f, 108.5229f), "ex_sm_13_office_01b");
                    AddInterior("Old Spice Vintage", new Position(-1579.681f, -565.0003f, 108.523f), "ex_sm_13_office_01c");
                    AddInterior("Power Broker Ice", new Position(-1579.677f, -565.0689f, 108.5229f), "ex_sm_13_office_03a");
                    AddInterior("Power Broker Conservative", new Position(-1579.708f, -564.9634f, 108.5229f), "ex_sm_13_office_03b");
                    AddInterior("Power Broker Polished", new Position(-1579.693f, -564.8981f, 108.5229f), "ex_sm_13_office_03c");
                    AddInterior("Garage 1", new Position(-1581.1120f, -567.2450f, 85.5000f), "imp_sm_13_cargarage_a");
                    AddInterior("Garage 2", new Position(-1568.7390f, -562.0455f, 85.5000f), "imp_sm_13_cargarage_b");
                    AddInterior("Garage 3", new Position(-1563.5570f, -574.4314f, 85.5000f), "imp_sm_13_cargarage_c");
                    AddInterior("Mod Shop", new Position(-1578.0230f, -576.4251f, 104.2000f), "imp_sm_13_modgarage");

                    //Maze Bank West

                    AddInterior("Executive Rich", new Position(-1392.667f, -480.4736f, 72.04217f), "ex_sm_15_office_02b");
                    AddInterior("Executive Cool", new Position(-1392.542f, -480.4011f, 72.04211f), "ex_sm_15_office_02c");
                    AddInterior("Executive Contrast", new Position(-1392.626f, -480.4856f, 72.04212f), "ex_sm_15_office_02a");
                    AddInterior("Old Spice Warm", new Position(-1392.617f, -480.6363f, 72.04208f), "ex_sm_15_office_01a");
                    AddInterior("Old Spice Classical", new Position(-1392.532f, -480.7649f, 72.04207f), "ex_sm_15_office_01b");
                    AddInterior("Old Spice Vintage", new Position(-1392.611f, -480.5562f, 72.04214f), "ex_sm_15_office_01c");
                    AddInterior("Power Broker Ice", new Position(-1392.563f, -480.549f, 72.0421f), "ex_sm_15_office_03a");
                    AddInterior("Power Broker Conservative", new Position(-1392.528f, -480.475f, 72.04206f), "ex_sm_15_office_03b");
                    AddInterior("Power Broker Polished", new Position(-1392.416f, -480.7485f, 72.04207f), "ex_sm_15_office_03c");
                    AddInterior("Garage 1", new Position(-1388.8400f, -478.7402f, 56.1000f), "imp_sm_15_cargarage_a");
                    AddInterior("Garage 2", new Position(-1388.8600f, -478.7574f, 48.1000f), "imp_sm_15_cargarage_b");
                    AddInterior("Garage 3", new Position(-1374.6820f, -474.3586f, 56.1000f), "imp_sm_15_cargarage_c");
                    AddInterior("Mod Shop", new Position(-1391.2450f, -473.9638f, 77.2000f), "imp_sm_15_modgarage");

                    //Clubhouse & Warehouse

                    AddInterior("Clubhouse 1", new Position(1107.04f, -3157.399f, -37.51859f), "bkr_biker_interior_placement_interior_0_biker_dlc_int_01_milo");
                    AddInterior("Clubhouse 2", new Position(998.4809f, -3164.711f, -38.90733f), "bkr_biker_interior_placement_interior_1_biker_dlc_int_02_milo");
                    AddInterior("Meth Lab", new Position(997.5482f, -3200.537f, -36.3937f), "bkr_biker_interior_placement_interior_2_biker_dlc_int_ware01_milo");
                    AddInterior("Weed Farm", new Position(1065.857f, -3183.387f, -39.16349f), "bkr_biker_interior_placement_interior_3_biker_dlc_int_ware02_milo");
                    AddInterior("Cocaine Lockup", new Position(1088.763f, -3188.536f, -38.99347f), "bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo");
                    AddInterior("Counterfeit Cash Factory", new Position(1138.415f, -3198.326f, -39.66572f), "bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo");
                    AddInterior("Document Forgery Office", new Position(1173.283f, -3196.641f, -39.00797f), "bkr_biker_interior_placement_interior_6_biker_dlc_int_ware05_milo");
                    AddInterior("Warehouse Small", new Position(1087.849f, -3099.215f, -38.99995f), "ex_exec_warehouse_placement_interior_1_int_warehouse_s_dlc_milo");
                    AddInterior("Warehouse Medium", new Position(1048.302f, -3097.103f, -38.99993f), "ex_exec_warehouse_placement_interior_0_int_warehouse_m_dlc_milo");
                    AddInterior("Warehouse Large", new Position(992.762f, -3097.747f, -38.99586f), "ex_exec_warehouse_placement_interior_2_int_warehouse_l_dlc_milo");
                    AddInterior("Vehicle Warehouse", new Position(994.5925f, -3002.594f, -39.64699f), "imp_impexp_interior_placement_interior_1_impexp_intwaremed_milo_");
                    AddInterior("Lost MC Clubhouse", new Position(982.0083f, -100.8747f, 74.84512f), "bkr_bi_hw1_13_int");

                    // Special Locations

                    AddInterior("Union Depository", new Position(2.6968f, -667.0166f, 16.13061f), "FINBANK");
                    AddInterior("Trevors Trailer Dirty", new Position(1975.552f, 3820.538f, 33.44833f), "TrevorsMP");
                    AddInterior("Trevors Trailer Clean", new Position(1975.552f, 3820.538f, 33.44833f), "TrevorsTrailerTidy");
                    AddInterior("Stadium", new Position(-248.6731f, -2010.603f, 30.14562f), "SP1_10_real_interior");
                    AddInterior("Jewel Store", new Position(-630.07f, -236.332f, 38.05704f), "post_hiest_unload");
                    AddInterior("FIB Lobby", new Position(110.4f, -744.2f, 45.7496f), "FIBlobby");

                    // Car Garages

                    AddInterior("2 Car Garage", new Position(173.2903f, -1003.6f, -99.65707f), "");
                    AddInterior("4 Car Garage", new Position(197.8153f, -1002.293f, -99.65749f), "");
                    AddInterior("10 Car Garage", new Position(229.9559f, -981.7928f, -99.66071f), "");

                    //Apartments

                    AddInterior("Low End Apartment", new Position(266.1273f, -1007.511f, -101.0086f), "");
                    AddInterior("Medium End Apartment", new Position(346.6075f, -1012.648f, -99.19622f), "");
                    AddInterior("4 Integrity Way, Apt 28", new Position(-23.49449f, 597.8414f, 80.03088f), "");
                    AddInterior("4 Integrity Way, Apt 30", new Position(-17.54274f, -588.6515f, 90.11487f), "");
                    AddInterior("Dell Perro Heights, Apt 4", new Position(-1456.943f, -534.215f, 74.04458f), "");
                    AddInterior("Dell Perro Heights, Apt 7", new Position(-1451.86f, -523.5939f, 56.92902f), "");
                    AddInterior("Richard Majestic, Apt 2", new Position(-919.4082f, -368.6953f, 114.275f), "");
                    AddInterior("Tinsel Towers, Apt 42", new Position(-610.5037f, 59.09584f, 98.20042f), "");
                    AddInterior("Eclipse Towers, Apt 3", new Position(-776.8801f, 323.6757f, 211.9972f), "");
                    AddInterior("3655 Wild Oats Drive", new Position(-174.1733f, 497.699f, 137.6653f), "");
                    AddInterior("2044 North Conker Avenue", new Position(342.05f, 437.8131f, 149.3821f), "");
                    AddInterior("2045 North Conker Avenue", new Position(373.8355f, 423.5041f, 145.9079f), "");
                    AddInterior("2862 Hillcrest Avenue", new Position(-682.0886f, 592.3275f, 145.3931f), "");
                    AddInterior("2868 Hillcrest Avenue", new Position(-758.6028f, 619.0494f, 144.1539f), "");
                    AddInterior("2874 Hillcrest Avenue", new Position(-859.8859f, 691.3547f, 152.8608f), "");
                    AddInterior("2677 Whispymound Drive", new Position(117.2296f, 559.8178f, 184.3049f), "");
                    AddInterior("2133 Mad Wayne Thunder", new Position(-1289.805f, 449.5435f, 97.90253f), "");

                    // Misc

                    AddInterior("Bunker Interior", new Position(899.5518f, -3246.038f, -98.04907f), "");
                    AddInterior("CharCreator", new Position(402.5164f, -1002.847f, -99.2587f), "");
                    AddInterior("Mission Carpark", new Position(405.9228f, -954.1149f, -99.6627f), "");
                    AddInterior("Torture Room", new Position(136.5146f, -2203.149f, 7.30914f), "");
                    AddInterior("Solomon's Office", new Position(-1005.84f, -478.92f, 50.02733f), "");
                    AddInterior("Psychiatrist's Office", new Position(-1902.348f, -572.7907f, 19.09722f), "");
                    AddInterior("Omega's Garage", new Position(2331.344f, 2574.073f, 46.68137f), "");
                    AddInterior("Movie Theatre", new Position(-1427.299f, -245.1012f, 16.8039f), "");
                    AddInterior("Motel", new Position(152.2605f, -1004.471f, -98.99999f), "");
                    AddInterior("Mandrazos Ranch", new Position(1396.355f, 1141.521f, 114.3336f), "");
                    AddInterior("Life Invader Office", new Position(-1044.193f, -236.9535f, 37.96496f), "");
                    AddInterior("Lester's House", new Position(1273.9f, -1719.305f, 54.77141f), "");
                    AddInterior("FBI Top Floor", new Position(134.5835f, -749.339f, 258.152f), "");
                    AddInterior("FBI Floor 47", new Position(134.5835f, -766.486f, 234.152f), "");
                    AddInterior("FBI Floor 49", new Position(134.635f, -765.831f, 242.152f), "");
                    AddInterior("IAA Office", new Position(117.22f, -620.938f, 206.1398f), "");
                    AddInterior("Smuggler's Run Hangar", new Position(-1266.802f, -3014.837f, -49.000f), "");
                    AddInterior("Avenger Interior", new Position(520.0f, 4750.0f, -70.0f), "");
                    AddInterior("Facility", new Position(345.0041f, 4842.001f, -59.9997f), "");
                    AddInterior("Server Farm", new Position(2168.0f, 2920.0f, -84.0f), "");
                    AddInterior("Submarine", new Position(514.33f, 4886.18f, -62.59f), "");
                    AddInterior("IAA Facility", new Position(2147.91f, 2921.0f, -61.9f), "");
                    AddInterior("Nightclub", new Position(-1569.379f, -3017.259f, -74.40615f), "");
                    AddInterior("Nightclub Warehouse", new Position(-1505.783f, -3012.587f, -80.000f), "");
                    AddInterior("Terrorbyte Interior", new Position(-1421.015f, -3012.587f, -80.000f), "");

                    AddInterior("Bahama Mamas West", new Position(-1387.384f, -588.557f, 30.31952f), "hei_sm_16_interior_v_bahama_milo_");
                    AddInterior("Split Sides West Comedy Club", new Position(382.6698f, -1001.339f, -99.00004f), "hei_hw1_blimp_interior_v_comedy_milo_");
                    AddInterior("Avenger Interior", new Position(520.0f, 4750.0f, -70.0f), "");
                    AddInterior("Tequi-la-la", new Position(-564.6385f, 277.7336f, 83.13632f), "apa_ss1_11_interior_v_rockclub_milo_");
                    AddInterior("Floyd's Apartment", new Position(-1150.703f, -1520.713f, 10.633f), "");

                    AddInterior("Michael_premier", new Position(-813.3f, 177.5f, 75.76f), "Michael_premier");
                    AddInterior("6 CAR GARAGE", new Position(199.971f, -999.667f, -100.000f), "V_GARAGEM");

                    AddInterior("Franklin's Vinewood Home", new Position(7.994648f, 538.5573f, 176.0281f), "");
                    AddInterior("Franklin's Aunts House", new Position(-14.08369f, -1440.43f, 31.10155f), "");
                    AddInterior("Vanilla Unicorn", new Position(127.6424f, -1296.912f, 29.26953f), "");
                    AddInterior("Lifeinvader", new Position(-1047.921f, -233.1176f, 39.0144f), "");
                    AddInterior("Michael's House", new Position(-815.7581f, 178.5198f, 72.15312f), "");
                    AddInterior("Michael's Garage", new Position(-806.9432f, 186.1182f, 72.47498f), "");

                    AddInterior("Stadium Vip Bar", new Position(2797.927f, -3943.155f, 185.2357f), "xs_arena_interior_vip");
                    AddInterior("ModShop Hayes", new Position(482.9214f, -1313.043f, 29.20045f), "");
                    AddInterior("Arena", new Position(2800.00f, -3800.00f, 100.00f), "xs_arena_interior");
                    AddInterior("Yacht Interior", new Position(-3191.682f, -217.7284f, 5.885169f), "");

                    #endregion Default Interiors

                    CompileInteriors();
                    return;
                }

                using StreamReader streamReader = new StreamReader($"{altVDirectory}/interiors.json");
                string json = streamReader.ReadToEnd();

                InteriorList = JsonConvert.DeserializeObject<List<Interiors>>(json);

                Console.WriteLine($"Successfully {InteriorList.Count} loaded interiors!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        /// <summary>
        /// Used to compile all Interiors to a JSON file from the current InteriorsList
        /// </summary>
        public static void CompileInteriors()
        {
            try
            {
                Console.WriteLine($"Compiling Interiors");

                if (!Directory.Exists($"{altVDirectory}"))
                {
                    Console.WriteLine($"Directory not found!");
                    Console.WriteLine($"{Directory.GetCurrentDirectory()}");
                    return;
                }

                Console.WriteLine($"Directory Exists");

                if (File.Exists($"{altVDirectory}/interiors.json"))
                {
                    Console.WriteLine($"File exists, deleting");
                    File.Delete($"{altVDirectory}/interiors.json");
                }

                Console.WriteLine($"Writing to file!");
                File.WriteAllText($"{altVDirectory}/interiors.json", JsonConvert.SerializeObject(InteriorList, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }), Encoding.Default);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Adds an Interior to the list
        /// </summary>
        /// <param name="interiorName"></param>
        /// <param name="position"></param>
        /// <param name="ipl"></param>
        /// <param name="description"></param>
        public static void AddInterior(string interiorName, Position position, string ipl, string description = null)
        {
            Interiors newInterior = new Interiors { InteriorName = interiorName, Position = position, Ipl = ipl, Description = description };
            InteriorList.Add(newInterior);
        }

        public string InteriorName { get; set; }
        public Position Position { get; set; }
        public string Ipl { get; set; }
        public string Description { get; set; }

        public bool? IsMapped { get; set; }
    }
}