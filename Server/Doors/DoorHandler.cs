using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Doors
{
    public class DoorHandler
    {
        public static void OnReturnClosestDoor(IPlayer player, string entityModel, float posX, float posY, float posZ)
        {
            Position entityPosition = new Position(posX, posY, posZ);

            if (entityModel == "0")
            {
                player.SendNotification("~r~Door model incorrect");
                return;
            }

            Door door = Door.FetchDoor(entityModel, entityPosition, player.Dimension);

            if (door != null)
            {
                player.SendNotification("~r~This door already exists.");
                return;
            }

            door = new Door(entityModel, entityPosition, dimension: player.Dimension);

            using Context context = new Context();

            context.Doors.Add(door);

            context.SaveChanges();

            player.SendInfoNotification($"You've added a new door. Door Id: {door.Id}.");
        }

        public static void UpdateDoorsForAllPlayers()
        {
            foreach (IPlayer player in Alt.Server.GetPlayers())
            {
                UpdateDoorsForPlayer(player);
            }
        }

        public static void UpdateDoorForPlayer(IPlayer player, Door door)
        {
            player.Emit("SetDoorStatus", door.Model, door.PosX, door.PosY, door.PosZ, door.Locked);
        }

        public static void UpdateDoorsForPlayer(IPlayer player)
        {
            using Context context = new Context();

            List<Door> doors = context.Doors.ToList().Where(x => x.Dimension == player.Dimension || x.Dimension == -1).ToList();

            //player.Emit("receiveDoorList", JsonConvert.SerializeObject(doors));

            player.Emit("receiveDoorList", doors);
        }
    }
}