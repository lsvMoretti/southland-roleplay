using System;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace Server.Extensions
{
    public class CameraExtension
    {
        /// <summary>
        /// Creates a clientside camera
        /// </summary>
        /// <param name="player">Player who needs the camera</param>
        /// <param name="position">Position of the Camera</param>
        /// <param name="rotation">Rotation of the Camera</param>
        /// <param name="fov">Field of View</param>
        public static void CreateCamera(IPlayer player, Position position, Rotation rotation, int fov)
        {
            player.Emit("createCamera", position.X, position.Y, position.Z, rotation.Yaw, fov);
        }

        /// <summary>
        /// Creates a clientside camera and moves it towards the second position
        /// </summary>
        /// <param name="player">Player who needs the camera</param>
        /// <param name="positionOne">Start Camera Position</param>
        /// <param name="rotationOne">Start Camera Rotation</param>
        /// <param name="fovOne">Start Camera FOV</param>
        /// <param name="positionTwo">End Camera Position</param>
        /// <param name="rotationTwo">End Camera Rotation</param>
        /// <param name="fovTwo">End Camera Field of View</param>
        /// <param name="duration">Duration between start and end camera change</param>
        public static void InterpolateCamera(IPlayer player, Position positionOne, Rotation rotationOne, int fovOne,
            Position positionTwo, Rotation rotationTwo, int fovTwo, int duration)
        {
            player.Emit("interpolateCamera", positionOne.X, positionOne.Y, positionTwo.Z, rotationOne.Yaw, fovOne, positionTwo.X, positionTwo.Y, positionTwo.Z, rotationTwo.Yaw, fovTwo, duration);
        }

        /// <summary>
        /// Deletes a clientside camera
        /// </summary>
        /// <param name="player"></param>
        public static void DeleteCamera(IPlayer player)
        {
            try
            {
                player.Emit("destroyCamera");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void CreateCameraAtEntity(IPlayer player, Position position, float rotZ, int fov, IEntity entity)
        {
            player.Emit("CreateCameraAtEntity", position.X, position.Y, position.Z, rotZ, fov, entity);
        }
    }
}