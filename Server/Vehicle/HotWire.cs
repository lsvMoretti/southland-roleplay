using System;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;

namespace Server.Vehicle
{
    public class HotWire
    {
        [Command("hotwire", commandType: CommandType.Vehicle, description: "Used to hotwire a vehicle.")]
        public static void HotWireCommand(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            if (!player.IsInVehicle)
            {
                NotificationExtension.SendErrorNotification(player, "You must be in a vehicle!");
                return;
            }

            if (player.Vehicle.Driver != player)
            {
                NotificationExtension.SendErrorNotification(player, "You must be in the drivers seat");
                return;
            }

            Models.Vehicle? vehicleData = player.Vehicle.FetchVehicleData();

            if (vehicleData == null || vehicleData.FactionId > 0)
            {
                NotificationExtension.SendErrorNotification(player, "Unable to do this in this vehicle!");
                return;
            }

            if (player.Vehicle.EngineOn)
            {
                NotificationExtension.SendErrorNotification(player, "This engine is already on!");
                return;
            }

            string? decryptWord = null;

            while (true)
            {
                string? word = WordListHandler.FetchWord();
                if (string.IsNullOrEmpty(word)) continue;
                decryptWord = word;
                break;
            }

            string shuffledWord = decryptWord.Shuffle();

            player.SetSyncedMetaData("Hotwire:Decrypted", decryptWord);
            player.SetSyncedMetaData("Hotwire:Shuffled", shuffledWord);
            player.SetData("Hotwire:Vehicle", player.Vehicle);
            // Emit event to show word page here
            Console.WriteLine($"Word for: {player.GetClass().Name} is: {decryptWord}. Shuffled to: {shuffledWord}");
            // Correct Word, Incorrect Word, Time, Attempts
            player.Emit("VehicleScramble:LoadPage", decryptWord, shuffledWord, 30, 5);
            player.FreezeCam(true);
            player.ChatInput(false);
        }

        public static void OnMaxAttemptsReached(IPlayer player)
        {
            player.DeleteSyncedMetaData("Hotwire:Decrypted");
            player.DeleteSyncedMetaData("Hotwire:Shuffled");
            player.DeleteData("Hotwire:Vehicle");

            player.FreezeCam(false);
            player.ChatInput(true);
            player.SendErrorNotification("You've reached the maximum amount of attempts!");

            player.SendEmoteMessage("attempts to hot wire a vehicle and fails.");
        }

        public static void OnTimeExpired(IPlayer player)
        {
            player.DeleteSyncedMetaData("Hotwire:Decrypted");
            player.DeleteSyncedMetaData("Hotwire:Shuffled");
            player.DeleteData("Hotwire:Vehicle");
            player.FreezeCam(false);
            player.ChatInput(true);
            player.SendErrorNotification("You've ran out of time!");
            player.SendEmoteMessage("attempts to hot wire a vehicle and fails.");
        }

        public static void OnPageClosed(IPlayer player)
        {
            player.DeleteSyncedMetaData("Hotwire:Decrypted");
            player.DeleteSyncedMetaData("Hotwire:Shuffled");
            player.DeleteData("Hotwire:Vehicle");
            player.FreezeCam(false);
            player.ChatInput(true);
            player.SendErrorNotification("You've stopped Hot Wiring!");
        }

        public static void OnCorrectWord(IPlayer player)
        {
            player.GetData("Hotwire:Vehicle", out IVehicle vehicle);
            if (player.Vehicle != vehicle)
            {
                player.SendErrorNotification("You must be in the vehicle!");
                return;
            }
            player.DeleteSyncedMetaData("Hotwire:Decrypted");
            player.DeleteSyncedMetaData("Hotwire:Shuffled");
            player.DeleteData("Hotwire:Vehicle");
            player.FreezeCam(false);
            player.ChatInput(true);

            using Context context = new Context();
            var vehicleDb = context.Vehicle.Find(player.Vehicle.GetVehicleId());

            vehicleDb.Engine = !vehicleDb.Engine;

            context.SaveChanges();

            player.Vehicle.EngineOn = vehicleDb.Engine;

            player.Emit("Vehicle:SetEngineStatus", player.Vehicle, player.Vehicle.EngineOn, false);

            player.SendEmoteMessage(vehicleDb.Engine
                ? $"hot wires the {vehicleDb.Name}."
                : $"turns the {vehicleDb.Name} engine off.");

            Logging.AddToCharacterLog(player, $"Has hot wired vehicle ID {vehicleDb.Id}.");
        }
    }
}