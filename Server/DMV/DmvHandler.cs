using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Character;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Inventory;
using Server.Models;
using Blip = Server.Objects.Blip;

namespace Server.DMV
{
    public class DmvHandler
    {
        public static Position DmvPosition = new Position(-940.5114f, -279.0121f, 39.23505f);

        /// <summary>
        /// DMV Checkpoints. Key - Point ID, Value - Point Position
        /// </summary>
        public static Dictionary<int, Position> DrivingCheckpoints = new Dictionary<int, Position>
        {
            //Vehicle Spawn position
            {1,  new Position(-965.1891f, -308.599f, 37.80809f)},
            { 2,  new Position(-991.5255f, -332.2308f, 37.52644f)},
            {3,  new Position(-1046.71f, -272.1252f, 37.54427f)},
            { 4,  new Position(-1142.408f, -275.9025f, 37.46664f)},
            { 5,  new Position(-1165.128f, -268.6004f, 37.41806f)},
            { 6,  new Position(-1281.293f, -328.6458f, 36.45417f)},
            { 7,  new Position(-1365.766f, -373.6917f, 36.43179f)},
            { 8,  new Position(-1445.22f, -428.638f, 35.41704f)},
            { 9,  new Position(-1396.107f, -545.429f, 30.0848f)},
            { 10,  new Position(-1526.639f, -653.9207f, 28.58915f)},
            { 11,  new Position(-1462.02f, -741.668f, 23.83417f)},
            { 12,  new Position(-1400.516f, -792.8173f, 19.5209f)},
            { 13,  new Position(-1405.591f, -824.7039f, 18.44456f)},
            { 14,  new Position(-2022.225f, -416.0034f, 11.0086f)},
            { 15,  new Position(-2140.296f, -333.1033f, 12.74189f)},
            { 16,  new Position(-1746.98f, -359.1166f, 46.07966f)},
            { 17,  new Position(-1612.155f, -317.4477f, 50.43686f)},
            { 18,  new Position(-1576.721f, -337.0265f, 46.84907f)},
            { 19,  new Position(-1485.403f, -418.0595f, 36.52372f)},
            { 20,  new Position(-1399.766f, -409.3449f, 36.26789f)},
            { 21,  new Position(-1315.809f, -364.8718f, 36.3912f)},
            { 22,  new Position(-1086.508f, -276.4744f, 37.4233f)},
            { 23,  new Position(-1006.722f, -237.0581f, 37.45618f)},
            { 24,  new Position(-945.0342f, -251.5495f, 38.52803f)},
            { 25,  new Position(-945.7062f, -266.5934f, 38.72596f)},
            // Last checkpoint for DMV vehicle to despawn
            { 26,  new Position(-960.5753f, -306.5247f, 38.03578f)},
        };

        /// <summary>
        /// List of spawned DMV Vehicles
        /// </summary>
        public static Dictionary<int, IVehicle> DrivingVehicles = new Dictionary<int, IVehicle>();

        public static void StartDrivingTest(IPlayer player)
        {
            Position vehicleSpawnPosition = DrivingCheckpoints.FirstOrDefault().Value;

            bool spaceOccupied =
                Alt.Server.GetVehicles().Any(x => x.Position.Distance(vehicleSpawnPosition) <= 5f);

            if (spaceOccupied)
            {
                player.SendErrorNotification("Please wait till the vehicle has moved from the space.");
                return;
            }

            player.GetClass().Cash -= 25;

            player.SendInfoNotification($"The test has started. A fee of {25:C0} has been taken. Head to the location around the corner.");
            player.SendInfoNotification($"The City Speed Limit is 35 MPH, Highways and Interstates are 55 MPH. You will be judged on your speed!");

            IVehicle dmvVehicle = new AltV.Net.Elements.Entities.Vehicle(Alt.Hash("prairie"), vehicleSpawnPosition, new DegreeRotation(0, 0, 197f));

            dmvVehicle.NumberplateText = "DMV";
            dmvVehicle.NumberplateIndex = 4;

            dmvVehicle.SetData("DMV:OwnerCharacter", player.FetchCharacter().Id);

            dmvVehicle.GetClass().FuelLevel = 100;
            dmvVehicle.SetSyncedMetaData("FUELLEVEL", 100);

            float randomOdo = Convert.ToSingle(Utility.GenerateRandomNumber(5));

            float randomOdoDec = Convert.ToSingle(Utility.GenerateRandomNumber(1));

            float odo = Convert.ToSingle($"{randomOdo}.{randomOdoDec}");

            dmvVehicle.SetSyncedMetaData("ODOREADING", odo);

            List<Position> checkpoints = new List<Position>();

            foreach (KeyValuePair<int, Position> drivingCheckpoint in DrivingCheckpoints)
            {
                checkpoints.Add(drivingCheckpoint.Value);
            }

            player.Emit("startDrivingTest", JsonConvert.SerializeObject(checkpoints, Formatting.None,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

            DrivingVehicles.Add(player.FetchCharacter().Id, dmvVehicle);
        }

        public static void OnDrivingTestFinished(IPlayer player, int value)
        {
            bool success = value > 0;

            bool hasValue = DrivingVehicles.TryGetValue(player.FetchCharacter().Id, out IVehicle playerVehicle);

            if (!hasValue)
            {
                player.SendErrorNotification("Unable to finish the test.");
                return;
            }

            playerVehicle.Remove();

            if (success)
            {
                using Context context = new Context();

                var playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                if (playerCharacter == null)
                {
                    player.SendErrorNotification("An error has occurred.");
                    return;
                }

                if (playerCharacter.Money < 25)
                {
                    player.SendErrorNotification("You don't have enough money to collect your license!");
                    return;
                }

                var licenses = JsonConvert.DeserializeObject<List<LicenseTypes>>(playerCharacter.LicensesHeld);

                licenses.Add(LicenseTypes.Driving);

                playerCharacter.LicensesHeld = JsonConvert.SerializeObject(licenses);
                if (playerCharacter.StartStage == 3)
                {
                    playerCharacter.StartStage = 4;
                    WelcomePlayer.OnDmvFinish(player);
                }

                context.SaveChanges();

                var playerInventory = player.FetchInventory();

                bool added = playerInventory.AddItem(new InventoryItem("ITEM_DRIVING_LICENSE", "Drivers License", playerCharacter.Name));

                if (!added)
                {
                    player.SendErrorNotification($"You don't have space to receive your license.");
                    return;
                }

                player.GetClass().Cash -= 25;

                player.SendInfoNotification($"You've completed the Driving Test at the Los Santos DMV and have completed it successfully. Don't lose it!");
                return;
            }

            player.SendErrorNotification("You have failed the driving test.");
        }

        public static void OnSpeeding(IPlayer player, int speedCount)
        {
            player.SendInfoNotification($"You have been warned for speeding. You have {speedCount} / 5 chances to slow down or you will fail!");
        }

        public static void InitDmv()
        {
            Blip dmvBlip = new Blip("DMV", DmvPosition, 538, 84, 0.75f);

            dmvBlip.Add();

            TextLabel dmvLabel = new TextLabel("Department of Motor Vehicles\nType /dmv", DmvPosition, TextFont.FontChaletComprimeCologne, new LsvColor(43, 147, 227, 255));

            dmvLabel.Add();
        }
    }
}