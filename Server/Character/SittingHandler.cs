using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Extensions;

namespace Server.Character
{
    public class SittingHandler
    {
        private static List<Position> seatsTaken = new List<Position>();

        public static void ToggleSitting(IPlayer player)
        {
            player.Emit("sitting:ToggleSitting");
        }

        public static void IsPlayerPositionFree(IPlayer player, float x, float y, float z, string objectName)
        {
            Position position = player.Position;

            Position pos = new Position(x, y, z);

            Console.WriteLine($"Pos: {pos}");
            if (seatsTaken.Contains(position))
            {
                Console.WriteLine($"Not Free");
                player.SendNotification("~r~This position is taken.");
                return;
            }

            Alt.EmitAllClients("Sitting:RemoveEntityCollision", objectName, pos);

            Console.WriteLine("Free");
            seatsTaken.Add(position);
            player.Emit("sitting:PositionFree");
        }

        public static void SetPlayerPosition(IPlayer player, Vector3 pos, float rot)
        {
            Console.WriteLine($"Pos: {pos}, rot: {rot}");
            player.Position = pos;

            Rotation playerRotation = player.Rotation;

            playerRotation.Yaw = (float)Utility.DegreeToRadian(rot);

            player.Rotation = playerRotation;

            Console.WriteLine($"Rotation: {playerRotation.Yaw}");
        }

        public static void ClearSeat(IPlayer player, float x, float y, float z, string objectName)
        {
            Position position = new Position(x, y, z);

            if (seatsTaken.Contains(position))
            {
                seatsTaken.Remove(position);
            }

            Alt.EmitAllClients("Sitting:SetEntityCollision", objectName, position);

            player.Position = player.Position;
        }
    }
}