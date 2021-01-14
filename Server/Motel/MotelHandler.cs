using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Models;
using Server.Property;
using Blip = Server.Objects.Blip;

namespace Server.Motel
{
    public class MotelHandler
    {
        public static List<MotelObject> MotelObjects = new List<MotelObject>();

        public static void LoadMotelsForPlayer(IPlayer player)
        {
            using Context context = new Context();

            List<Models.Motel> motels = context.Motels.ToList();

            foreach (Models.Motel motel in motels)
            {
                player.Emit("Motel:OnMotelAdded", JsonConvert.SerializeObject(motel));
            }
        }

        /// <summary>
        /// Load all motels
        /// </summary>
        public static void InitMotels()
        {
            Console.WriteLine("Loading Motels");
            using Context context = new Context();

            MotelObjects = new List<MotelObject>();

            List<Models.Motel> motels = context.Motels.ToList();

            Alt.EmitAllClients("Motel:ClearRoomLabels");

            foreach (var motel in motels)
            {
                LoadMotel(motel);
            }

            Console.WriteLine($"Loaded {context.Motels.Count()} Motels.");
        }

        /// <summary>
        /// Loads an individual motel
        /// </summary>
        /// <param name="motel"></param>
        public static void LoadMotel(Models.Motel motel)
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();

            Position motelPosition = new Position(motel.PosX, motel.PosY, motel.PosZ);

            TextLabel motelLabel = new TextLabel($"{motel.Name}", motelPosition, TextFont.FontChaletComprimeCologne, new LsvColor(Color.DarkGoldenrod));

            motelLabel.Add();

            Blip motelBlip = new Blip(motel.Name, motelPosition, 78, 5, 0.5f);

            motelBlip.Add();

            MotelObject newMotelObject = new MotelObject()
            {
                TextLabel = motelLabel,
                Blip = motelBlip,
                Id = motel.Id,
                Motel = motel,
                Type = 0
            };

            MotelObjects.Add(newMotelObject);

            Alt.EmitAllClients("Motel:OnMotelAdded", JsonConvert.SerializeObject(motel));

            sw.Stop();

            Console.WriteLine($"Loaded {motel.Name}. This took {sw.Elapsed}.");
        }

        /// <summary>
        /// Reloads an individual motel
        /// </summary>
        /// <param name="motel"></param>
        public static void ReloadMotel(Models.Motel motel)
        {
            InitMotels();
        }

        /// <summary>
        /// Reloads all motels
        /// </summary>
        public static void ReloadMotels()
        {
            foreach (MotelObject motelObject in MotelObjects)
            {
                motelObject.Blip?.Remove();

                motelObject.TextLabel?.Remove();
            }

            MotelObjects = new List<MotelObject>();

            InitMotels();
        }

        public static MotelRoom? FetchNearestMotelRoom(Position position, float distance = 5f)
        {
            float lastDistance = distance;
            MotelRoom? lastRoom = null;

            using Context context = new Context();

            List<Models.Motel> motels = context.Motels.ToList();

            foreach (Models.Motel motel in motels)
            {
                List<MotelRoom> motelRooms = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);
                foreach (MotelRoom motelRoom in motelRooms)
                {
                    Position roomPosition = new Position(motelRoom.PosX, motelRoom.PosY, motelRoom.PosZ);

                    float roomDistance = roomPosition.Distance(position);

                    if (roomDistance < distance && roomDistance < lastDistance)
                    {
                        lastDistance = roomDistance;
                        lastRoom = motelRoom;
                    }
                }
            }

            return lastRoom;
        }

        public static bool ToggleNearestRoomLock(IPlayer player, bool isAdmin = false)
        {
            MotelRoom nearRoom = FetchNearestMotelRoom(player.Position);

            if (nearRoom == null) return false;

            Models.Character playerCharacter = player.FetchCharacter();

            if (nearRoom.OwnerId != playerCharacter.Id)
            {
                player.SendErrorNotification("You don't rent this room!");
                return true;
            }

            using Context context = new Context();

            var motel = context.Motels.Find(nearRoom.MotelId);

            if (motel == null)
            {
                motel = context.Motels.Find(playerCharacter.InMotel);

                if (motel == null)
                {
                    player.SendErrorNotification("You're not in near a motel room.");
                    return false;
                }

                List<MotelRoom> insideMotelRooms = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

                MotelRoom insideRoom = insideMotelRooms.FirstOrDefault(x => x.Id == playerCharacter.InMotelRoom);

                if (insideRoom == null)
                {
                    player.SendErrorNotification("You're not in near a motel room.");
                    return false;
                }

                insideRoom.Locked = !insideRoom.Locked;

                motel.RoomList = JsonConvert.SerializeObject(insideMotelRooms);

                context.SaveChanges();

                string roomLockStatus = "unlocked";

                if (insideRoom.Locked)
                {
                    roomLockStatus = "locked";
                }

                player.SendInfoNotification($"You have {roomLockStatus} the door.");

                return true;
            }

            List<MotelRoom> motelRooms = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

            MotelRoom room = motelRooms.FirstOrDefault(x => x.Id == nearRoom.Id);

            room.Locked = !room.Locked;

            motel.RoomList = JsonConvert.SerializeObject(motelRooms);

            context.SaveChanges();

            string lockStatus = "unlocked";

            if (room.Locked)
            {
                lockStatus = "locked";
            }

            player.SendInfoNotification($"You have {lockStatus} the door.");

            return true;
        }

        public static void SetPlayerIntoMotelRoom(IPlayer player, MotelRoom room)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            playerCharacter.InMotel = room.MotelId;
            playerCharacter.InMotelRoom = room.Id;

            Interiors? interior = Interiors.InteriorList.FirstOrDefault(x => x.InteriorName == "Motel");

            player.Position = interior.Position;

            int newDimension = room.Id;

            newDimension *= 10000;

            player.Dimension = newDimension;

            player.SetSyncedMetaData("PlayerDimension", player.Dimension);
            context.SaveChanges();
        }

        public static void SetPlayerOutOfMotelRoom(IPlayer player)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            Models.Motel inMotel = context.Motels.Find(playerCharacter.InMotel);

            if (inMotel == null)
            {
                Console.WriteLine($" {playerCharacter.Name} Has tried leaving a motel room that doesn't exist.");
                return;
            }

            MotelRoom? inMotelRoom = JsonConvert.DeserializeObject<List<MotelRoom>>(inMotel.RoomList)
                .FirstOrDefault(x => x.Id == playerCharacter.InMotelRoom);

            if (inMotelRoom == null)
            {
                Console.WriteLine($" {playerCharacter.Name} Has tried leaving a motel room that doesn't exist.");
                return;
            }

            Position exteriorPosition = new Position(inMotelRoom.PosX, inMotelRoom.PosY, inMotelRoom.PosZ);

            player.Position = exteriorPosition;
            player.Dimension = 0;

            player.SetSyncedMetaData("PlayerDimension", player.Dimension);
            playerCharacter.InMotel = 0;
            playerCharacter.InMotelRoom = 0;

            context.SaveChanges();
        }
    }

    public class MotelObject
    {
        public int Id { get; set; }
        public Models.Motel Motel { get; set; }

        public TextLabel TextLabel { get; set; }

        public Blip Blip { get; set; }

        /// <summary>
        /// 0 = Motel, 1 = Motel Room
        /// </summary>
        public int Type { get; set; }
    }
}