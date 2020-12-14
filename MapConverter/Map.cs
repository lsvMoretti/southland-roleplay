using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Elements.Entities;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MapConverter
{
    public class DynamicObject
    {
        public uint Model { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public int Dimension { get; set; }
        public bool? Dynamic  { get; set; }
        public bool? Frozen { get; set; }
        public uint?  LodDistance { get; set; }
        public Rgb LightColor { get; set; }
        public bool? OnFire { get; set; }
        public TextureVariation? TextureVariation { get; set; }
        public bool? Visible  { get; set; }
        public uint StreamRange { get; set; }
    }

    public class Rgb
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public Rgb(int red, int green, int blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
    
    public enum TextureVariation
    {
        Pacific = 0,
        Azure = 1,
        Nautical = 2,
        Continental = 3,
        Battleship = 4,
        Intrepid = 5,
        Uniform = 6,
        Classico = 7,
        Mediterranean = 8,
        Command = 9,
        Mariner = 10,
        Ruby = 11,
        Vintage = 12,
        Pristine = 13,
        Merchant = 14,
        Voyager = 15
    }

    public class ObjectStreamer
    {
        public static DynamicObject CreateDynamicObject(uint model, Vector3 position, Vector3 rotation,
            int dimension = 0, bool? isDynamic = null, bool? frozen = null, uint? lodDistance = null,
            Rgb lightColor = null, bool? onFire = null, TextureVariation? textureVariation = null, bool? visible = null,
            uint streamRange = 400)
        {
            return new DynamicObject
            {
                Model = model,
                Position = position,
                Rotation = rotation,
                Dimension = dimension,
                Dynamic = isDynamic,
                Frozen = frozen,
                LodDistance = lodDistance,
                LightColor = lightColor,
                OnFire = onFire,
                TextureVariation = textureVariation,
                Visible = visible,
                StreamRange = streamRange
            };
        }
    }
    
    public class Map
    {
        public string MapName { get; set; }
        public bool IsInterior { get; set; }
        public string Interior { get;set; }
        public List<DynamicObject> MapObjects { get; set; }

        public Map(string mapName, bool isInterior, string interiorName = "")
        {
            List<DynamicObject> newObjects = new List<DynamicObject>
            {
                ObjectStreamer.CreateDynamicObject(3188625299, new Vector3(-1172.6964f, -1181.2162f, -150f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3188625299, new Vector3(-1172.6996f, -1173.279f, -150f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3188625299, new Vector3(-1181.4777f, -1173.283f, -150f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3188625299, new Vector3(-1181.476f, -1181.2295f, -150f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3188625299, new Vector3(-1172.6958f, -1165.3359f, -150f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3188625299, new Vector3(-1181.478f, -1165.3447f, -150f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1185.7526f, -1181.1624f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1185.7478f, -1173.0725f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1185.7478f, -1164.9789f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1173.581f, -1185.1797f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3584148813, new Vector3(-1177.2495f, -1185.1049f, -148.74077f), new Vector3(0f, 0f, 0f), 0, true, true, 30, null, false, (TextureVariation) 0, true, 30),
                ObjectStreamer.CreateDynamicObject(1593135630, new Vector3(-1175.8964f, -1172.1573f, -149.91095f), new Vector3(0f, -0f, 90f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1593135630, new Vector3(-1173.2393f, -1174.4862f, -149.91095f), new Vector3(0f, -0f, 179.9999f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(3388854276, new Vector3(-1183.9974f, -1173.8788f, -149.91095f), new Vector3(0f, -0f, 90f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1865404709, new Vector3(-1181.6237f, -1166.1074f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1019644700, new Vector3(-1173.6501f, -1181.7726f, -149.30098f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2528215952, new Vector3(-1172.0865f, -1178.3992f, -148.6759f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2528215952, new Vector3(-1176.4088f, -1181.8085f, -148.6759f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(3192690208, new Vector3(-1175.1571f, -1184.8127f, -149.91095f), new Vector3(0f, -0f, -179.9999f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(1793667637, new Vector3(-1175.5768f, -1179.1046f, -148.66911f), new Vector3(-1.4033414E-14f, 7.016706E-15f, 5.099994f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(3836784261, new Vector3(-1174.3473f, -1184.8069f, -149.91095f), new Vector3(0f, -0f, 179.9999f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(3192690208, new Vector3(-1173.5343f, -1184.8147f, -149.91095f), new Vector3(0f, -0f, -179.9999f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(2598687019, new Vector3(-1171.2776f, -1179.103f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(1753238891, new Vector3(-1173.6432f, -1178.6006f, -148.66977f), new Vector3(-1.4033416E-14f, 7.016708E-15f, 7.1999907f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(993353915, new Vector3(-1175.1182f, -1179.2854f, -149.07591f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2010966735, new Vector3(-1174.584f, -1179.0321f, -149.08102f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(67883626, new Vector3(-1175.8666f, -1181.4222f, -149.06104f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(3773666191, new Vector3(-1175.2815f, -1178.9652f, -148.67598f), new Vector3(-1.4033418E-14f, 7.01671E-15f, -144.60016f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2456611903, new Vector3(-1176.234f, -1180.0594f, -148.67094f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2675939568, new Vector3(-1172.4199f, -1184.626f, -149.91095f), new Vector3(0f, -0f, 179.9999f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(846652480, new Vector3(-1176.312f, -1180.8433f, -148.67099f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2583440873, new Vector3(-1171.5303f, -1178.5736f, -148.66962f), new Vector3(0f, 0f, 0f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(3990339795, new Vector3(-1172.3175f, -1179.0547f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(18704222, new Vector3(-1173.2576f, -1179.0415f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(3874153059, new Vector3(-1175.8689f, -1179.5756f, -148.66977f), new Vector3(-1.4033416E-14f, 7.016708E-15f, 54.499825f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(3925937179, new Vector3(-1176.1547f, -1183.7018f, -149.07089f), new Vector3(-1.40334166E-14f, 7.0167083E-15f, 113.099525f), 0, false, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(1850701663, new Vector3(-1176.3988f, -1175.4391f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1850701663, new Vector3(-1174.4207f, -1177.0353f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1850701663, new Vector3(-1176.6226f, -1174.8894f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1850701663, new Vector3(-1173.4792f, -1177.0564f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1850701663, new Vector3(-1172.6809f, -1177.0499f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1850701663, new Vector3(-1171.9526f, -1177.0708f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(986810493, new Vector3(-1173.2108f, -1171.9778f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(2380792685, new Vector3(-1171.9564f, -1179.0135f, -148.98924f), new Vector3(0f, 0f, 0f), 0, false, true, 35, null, false, (TextureVariation) 0, true, 35),
                ObjectStreamer.CreateDynamicObject(2644815621, new Vector3(-1184.0015f, -1182.782f, -149.91095f), new Vector3(0f, -0f, 90.99989f), 0, false, true, 60, null, false, (TextureVariation) 13, true, 60),
                ObjectStreamer.CreateDynamicObject(2644815621, new Vector3(-1183.9991f, -1178.2719f, -149.91095f), new Vector3(0f, -0f, 90.99989f), 0, false, true, 60, null, false, (TextureVariation) 13, true, 60),
                ObjectStreamer.CreateDynamicObject(3388854276, new Vector3(-1184.0034f, -1169.6702f, -149.91095f), new Vector3(0f, -0f, 90f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1169.5721f, -1173.1296f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1169.5658f, -1165.0496f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(1593135630, new Vector3(-1175.8597f, -1168.5868f, -149.91095f), new Vector3(0f, -0f, 90f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1593135630, new Vector3(-1173.3136f, -1166.1125f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(986810493, new Vector3(-1173.2052f, -1168.6339f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(1194029334, new Vector3(-1169.6473f, -1170.3009f, -147.71451f), new Vector3(0f, -0f, -90f), 0, false, true, 40, null, false, (TextureVariation) 2, true, 40),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1173.6185f, -1161.3003f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1181.6873f, -1161.3003f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(583850254, new Vector3(-1181.3832f, -1181.1598f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(583850254, new Vector3(-1172.5962f, -1181.1328f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(583850254, new Vector3(-1172.6008f, -1173.1938f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(583850254, new Vector3(-1181.3756f, -1173.2324f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(583850254, new Vector3(-1172.5991f, -1165.2911f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(583850254, new Vector3(-1181.3671f, -1165.2983f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1169.5742f, -1181.209f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1169.5742f, -1181.209f, -145.51093f), new Vector3(0.00010427405f, 179.99997f, 179.99997f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1169.5721f, -1173.1296f, -145.51093f), new Vector3(0.00010427405f, 179.99997f, 179.99997f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1169.5658f, -1165.0496f, -145.51093f), new Vector3(0.00010427403f, 179.99994f, 179.99994f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1173.6196f, -1161.3003f, -145.51093f), new Vector3(0.000104273924f, 179.9999f, -89.99936f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1181.6853f, -1161.3003f, -145.51093f), new Vector3(0.000104273924f, 179.9999f, -89.99936f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1185.7478f, -1164.9789f, -145.51093f), new Vector3(0.00010427399f, 179.9999f, 179.9999f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1185.7478f, -1173.0725f, -145.51093f), new Vector3(0.00010427399f, 179.9999f, 179.9999f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1185.7526f, -1181.1624f, -145.51093f), new Vector3(0.00010427396f, 179.9999f, 179.9999f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1181.6819f, -1185.18f, -145.51093f), new Vector3(0.00010427396f, 179.9999f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1173.5845f, -1185.1797f, -145.51093f), new Vector3(0.00010427396f, 179.9999f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3917661608, new Vector3(-1169.6206f, -1175.0554f, -147.71451f), new Vector3(0f, -0f, -90f), 0, false, true, 50, null, false, (TextureVariation) 0, true, 50),
                ObjectStreamer.CreateDynamicObject(513682938, new Vector3(-1169.6494f, -1181.9565f, -147.71451f), new Vector3(0f, -0f, -90f), 0, false, true, 50, null, false, (TextureVariation) 0, true, 50),
                ObjectStreamer.CreateDynamicObject(2124237493, new Vector3(-1185.0443f, -1164.5844f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(2520158452, new Vector3(-1170.0275f, -1184.6449f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(84687827, new Vector3(-1185.6826f, -1176.3715f, -147.71451f), new Vector3(0f, -4.098112E-05f, 90f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(276954077, new Vector3(-1181.0706f, -1184.5048f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 60, null, false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1430014549, new Vector3(-1177.9393f, -1164.7362f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 30, null, false, (TextureVariation) 0, true, 30),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1181.6819f, -1185.18f, -148.441f), new Vector3(0.000104273924f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2989156284, new Vector3(-1179.2335f, -1183.8755f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3860852799, new Vector3(-1177.9563f, -1182.5863f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3671685777, new Vector3(-1176.6605f, -1183.8574f, -148.441f), new Vector3(0f, 0f, 0f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3086975581, new Vector3(-1177.3003f, -1182.5834f, -148.77081f), new Vector3(0f, 0f, 0f), 0, true, true, 40, null, false, (TextureVariation) 0, true, 40),
                ObjectStreamer.CreateDynamicObject(2989156284, new Vector3(-1179.2335f, -1183.8755f, -145.51093f), new Vector3(0.00010427406f, -180f, -180f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2989156284, new Vector3(-1176.6646f, -1183.8602f, -145.51093f), new Vector3(0.00010427403f, 179.99994f, 179.99994f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2989156284, new Vector3(-1177.9602f, -1182.5851f, -145.51093f), new Vector3(0.00010427396f, 179.9999f, -90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2247548087, new Vector3(-1176.6094f, -1186.6355f, -148.07393f), new Vector3(0f, -0f, -90f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(3860852799, new Vector3(-1170.9839f, -1164.0658f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(992647982, new Vector3(-1185.1016f, -1162.6648f, -149.24524f), new Vector3(0f, -0f, 90f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1176.3368f, -1164.0658f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3671685777, new Vector3(-1181.7227f, -1164.0658f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2989156284, new Vector3(-1184.398f, -1164.0658f, -148.441f), new Vector3(0f, -0f, 90f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2247548087, new Vector3(-1182.6826f, -1164.1292f, -148.18387f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1181.6853f, -1164.0658f, -145.51093f), new Vector3(0.000104273924f, 179.9999f, -89.99936f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(2230324134, new Vector3(-1173.6196f, -1164.0658f, -145.51093f), new Vector3(0.000104273924f, 179.9999f, -89.99936f), 0, false, true, 106, null, false, (TextureVariation) 0, true, 106),
                ObjectStreamer.CreateDynamicObject(3775898501, new Vector3(-1171.6685f, -1164.0413f, -148.77185f), new Vector3(0f, 0f, 0f), 0, true, true, 30, null, false, (TextureVariation) 0, true, 30),
                ObjectStreamer.CreateDynamicObject(810899590, new Vector3(-1183.4946f, -1163.1016f, -149.44098f), new Vector3(0f, -0f, -90f), 0, false, true, 20, null, false, (TextureVariation) 0, true, 20),
                ObjectStreamer.CreateDynamicObject(2353589337, new Vector3(-1181.6373f, -1161.7449f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(2353589337, new Vector3(-1180.8375f, -1161.7434f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(1282927707, new Vector3(-1184.7435f, -1162.3689f, -149.10385f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1282927707, new Vector3(-1184.8148f, -1163.7717f, -149.10385f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1282927707, new Vector3(-1184.8148f, -1163.841f, -149.10385f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1282927707, new Vector3(-1184.8148f, -1163.7717f, -149.06577f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1282927707, new Vector3(-1184.8148f, -1163.7717f, -149.02766f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(1282927707, new Vector3(-1184.8148f, -1163.7717f, -148.98958f), new Vector3(0f, 0f, 0f), 0, false, true, 100, null, false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(3584148813, new Vector3(-1170.4253f, -1161.3354f, -148.76463f), new Vector3(0f, 0f, 0f), 0, true, true, 30, null, false, (TextureVariation) 0, true, 30),
                ObjectStreamer.CreateDynamicObject(3531165681, new Vector3(-1178.0233f, -1161.8278f, -149.91095f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(4063788866, new Vector3(-1173.1554f, -1163.7292f, -149.90096f), new Vector3(0f, -0f, 179.9999f), 0, false, true, 30, null, false, (TextureVariation) 0, true, 30),
                ObjectStreamer.CreateDynamicObject(168901740, new Vector3(-1185.3229f, -1161.7732f, -144.71092f), new Vector3(-1.4033411E-14f, 7.016708E-15f, 65.39955f), 0, false, true, 70, null, false, (TextureVariation) 0, true, 70),
                ObjectStreamer.CreateDynamicObject(2954561821, new Vector3(-1184.9349f, -1164.8333f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(2954561821, new Vector3(-1170.2357f, -1184.2723f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 1000, null, false, (TextureVariation) 0, true, 1000),
                ObjectStreamer.CreateDynamicObject(3956240264, new Vector3(-1176.7325f, -1182.7738f, -145.14314f), new Vector3(0f, -0f, -90f), 0, false, true, 65, null, false, (TextureVariation) 0, true, 65),
                ObjectStreamer.CreateDynamicObject(2369898685, new Vector3(-1174.4008f, -1185.1045f, -146.89742f), new Vector3(0f, -0f, 179.9999f), 0, false, true, 35, null, false, (TextureVariation) 0, true, 35),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1184.3612f, -1182.7622f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(67, 44, 27), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1184.3612f, -1178.0615f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(67, 44, 27), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1184.3612f, -1173.7147f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(67, 44, 27), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1184.3612f, -1169.6512f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(67, 44, 27), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1181.2815f, -1166.339f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(67, 44, 27), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1171.9766f, -1166.339f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(32, 26, 64), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1172.0305f, -1173.7147f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(32, 26, 64), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1172.9451f, -1180.9421f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(48, 27, 15), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1177.9918f, -1183.8208f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(16, 9, 5), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(2756229782, new Vector3(-1178.0282f, -1180.9421f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 100, new Rgb(48, 27, 15), false, (TextureVariation) 0, true, 100),
                ObjectStreamer.CreateDynamicObject(3449848423, new Vector3(-1184.0428f, -1162.7147f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 60, new Rgb(46, 46, 46), false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(3449848423, new Vector3(-1176.8176f, -1162.5802f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 60, new Rgb(46, 46, 46), false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(3449848423, new Vector3(-1171.5741f, -1162.4591f, -144.69093f), new Vector3(0f, 0f, 0f), 0, false, true, 60, new Rgb(46, 46, 46), false, (TextureVariation) 0, true, 60),
                ObjectStreamer.CreateDynamicObject(1517151235, new Vector3(-1185.6826f, -1162.6587f, -147.81105f), new Vector3(0.000104274026f, 179.99992f, 90f), 0, false, true, 50, null, false, (TextureVariation) 0, true, 50),
            };

            MapName = mapName;
            foreach (DynamicObject newObject in newObjects)
            {
                if (isInterior)
                {
                    newObject.Dimension = Int32.MinValue;
                }
            }

            IsInterior = isInterior;
            Interior = interiorName;
            
            MapObjects = newObjects;

        }
        
    }
}