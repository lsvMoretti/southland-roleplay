using System.Drawing;
using System.Numerics;
using AltV.Net.Data;

namespace Server.Extensions.Marker
{
    public class Marker
    {
        public int Type { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float DirX { get; set; }
        public float DirY { get; set; }
        public float DirZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float Scale { get; set; }
        public Color Color { get; set; }
        public float Range { get; set; }
        public bool Bob { get; set; }
        public bool FaceCamera { get; set; }
        public bool Rotate { get; set; }

        /// <summary>
        /// Returns a New Marker Object
        /// </summary>
        /// <param name="type"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <param name="color"></param>
        /// <param name="range"></param>
        /// <param name="rotate"></param>
        /// <param name="bob"></param>
        /// <param name="faceCamera"></param>
        public Marker(MarkerType type, Position position, Vector3 direction, Rotation rotation, float scale, Color color, float range = 5f,
            bool rotate = false, bool bob = false, bool faceCamera = false)
        {
            Type = (int)type;
            PosX = position.X;
            PosY = position.Y;
            PosZ = position.Z;
            DirX = direction.X;
            DirY = direction.Y;
            DirZ = direction.Z;
            RotX = rotation.Pitch;
            RotY = rotation.Roll;
            RotZ = rotation.Yaw;
            Scale = scale;
            Color = color;
            Range = range;
            Rotate = rotate;
            Bob = bob;
            FaceCamera = faceCamera;
        }
    }
}