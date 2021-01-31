using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Bank.Payment;
using Server.Chat;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Inventory;
using Server.Models;
using Server.Vehicle;
using Blip = Server.Objects.Blip;

namespace Server.Dealerships
{
    public class DealershipHandler
    {
        private static Dictionary<int, Blip> dealershipBlips = new Dictionary<int, Blip>();
        private static Dictionary<int, TextLabel> dealershipLabels = new Dictionary<int, TextLabel>();
        private static Dictionary<int, ushort> previewVehicles = new Dictionary<int, ushort>();

        private static int _nextDimension = 1;

        public static void LoadDealerships()
        {
            foreach (KeyValuePair<int, Blip> dealershipBlip in dealershipBlips)
            {
                dealershipBlip.Value.Remove();
            }

            foreach (KeyValuePair<int, TextLabel> dealershipLabel in dealershipLabels)
            {
                dealershipLabel.Value.Remove();
            }

            dealershipBlips = new Dictionary<int, Blip>();
            dealershipLabels = new Dictionary<int, TextLabel>();

            List<Dealership> dealershipList = Dealership.FetchDealerships();

            foreach (Dealership dealership in dealershipList)
            {
                Blip dealershipBlip = new Blip(dealership.Name, new Position(dealership.PosX, dealership.PosY, dealership.PosZ), 225, 24, 1f, true, dealership.Id);
                dealershipBlips.Add(dealership.Id, dealershipBlip);
                dealershipBlip.Add();

                TextLabel dealershipLabel = new TextLabel($"{dealership.Name}\nUsage: /vbuy to purchase a vehicle", new Position(dealership.PosX, dealership.PosY, dealership.PosZ), TextFont.FontChaletComprimeCologne, new LsvColor(Color.Chocolate), 5f);
                dealershipLabel.Add();

                dealershipLabels.Add(dealership.Id, dealershipLabel);
            }
        }

        public static void UnloadDealership(Dealership dealership)
        {
            try
            {
                bool tryGetBlip = dealershipBlips.TryGetValue(dealership.Id, out Blip? dealershipBlip);

                bool tryGetLabel = dealershipLabels.TryGetValue(dealership.Id, out TextLabel? dealershipTextLabel);

                if (tryGetBlip)
                {
                    dealershipBlip.Remove();
                }

                if (tryGetLabel)
                {
                    dealershipTextLabel.Remove();
                }

                foreach (KeyValuePair<int, ushort> previewVehicle in previewVehicles.Where(x => x.Key == dealership.Id))
                {
                    IVehicle? vehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.Id == previewVehicle.Value);

                    vehicle?.Remove();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return;
            }
        }

        public static async void OnWebViewClose(IPlayer player)
        {
            try
            {
                player.FreezeCam(false);
                player.FreezeInput(false);
                player.ShowCursor(false);
                player.HideChat(false);
                player.ChatInput(true);

                if (previewVehicles.ContainsKey(player.GetPlayerId()))
                {
                    var vehId = previewVehicles[player.GetPlayerId()];

                    IVehicle? vehicle = Alt.Server.GetVehicles().FirstOrDefault(i => i.Id == vehId);

                    if (vehicle == null) return;

                    vehicle.Delete();

                    previewVehicles.Remove(player.GetPlayerId());
                }

                CameraExtension.DeleteCamera(player);

                player.Dimension = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static async void OnVehiclePreviewSelect(IPlayer player, int index)
        {
            try
            {
                player.Emit("closeCurrentPage");

                bool dataSuccess = player.GetData("DEALERSHIPVEHICLELIST", out string dealershipVehiclesJson);

                if (!dataSuccess)
                {
                    player.SendErrorNotification("An error occurred.");
                    OnWebViewClose(player);
                    return;
                }

                List<DealershipVehicle> dealershipVehicles =
                    JsonConvert.DeserializeObject<List<DealershipVehicle>>(dealershipVehiclesJson);

                DealershipVehicle selectedDealershipVehicle = dealershipVehicles[index];

                player.GetData("ATDEALERSHIP", out int dealershipId);

                Dealership currentDealership = Dealership.FetchDealership(dealershipId);

                if (selectedDealershipVehicle == null || currentDealership == null)
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                player.Dimension = _nextDimension;
                _nextDimension++;

                Position vehiclePosition = new Position(currentDealership.VehPosX, currentDealership.VehPosY, currentDealership.VehPosZ + 2f);

                if (previewVehicles.ContainsKey(player.GetPlayerId()))
                {
                    var oldVehId = previewVehicles[player.GetPlayerId()];

                    IVehicle? oldPreviewVehicle = Alt.Server.GetVehicles().FirstOrDefault(i => i.Id == oldVehId);

                    if (oldPreviewVehicle != null)
                    {
                        oldPreviewVehicle.Delete();
                    }

                    previewVehicles.Remove(player.GetPlayerId());
                }

                uint vehicleModel;

                if (selectedDealershipVehicle.VehModel != 0)
                {
                    vehicleModel = (uint)selectedDealershipVehicle.VehModel;
                }
                else
                {
                    vehicleModel = Alt.Server.Hash(selectedDealershipVehicle.NewVehModel);
                }

                if (vehicleModel == 0) return;

                IVehicle? previewVehicle = Alt.Server.CreateVehicle(vehicleModel, vehiclePosition, new DegreeRotation(0, 0, 270));

                if (previewVehicle == null)
                {
                    player.SendErrorNotification("Unable to create the preview vehicle");
                }

                previewVehicle.Dimension = player.Dimension;
                previewVehicle.EngineOn = false;
                previewVehicle.LockState = VehicleLockState.Locked;
                previewVehicle.NumberplateText = "PREVIEW";

                previewVehicles.Add(player.GetPlayerId(), previewVehicle.Id);

                player.SetData("dealership:selectedPreviewVehicle", JsonConvert.SerializeObject(selectedDealershipVehicle));

                player.HideChat(true);
                player.ShowCursor(true);

                var delayTimer = new Timer(2000);

                delayTimer.Start();
                delayTimer.AutoReset = false;

                delayTimer.Elapsed += (sender, args) =>
                {
                    delayTimer.Stop();
                    if (!previewVehicle.Exists)
                    {
                        player.EmitLocked("dealership:CloseView");
                        return;
                    }
                    CameraExtension.CreateCameraAtEntity(player,
                        new Position(currentDealership.CamPosX, currentDealership.CamPosY,
                            currentDealership.CamPosZ), currentDealership.CamRotZ, 80, previewVehicle);
                    player.EmitLocked("ShowPreviewScreen", previewVehicle);
                };
            }
            catch (Exception e)
            {
                player.EmitLocked("dealership:CloseView");

                Console.WriteLine(e);
                return;
            }
        }

        public static void OnPreviewVehicleRotationChange(IPlayer player, string newRotation)
        {
            if (player == null) return;

            ushort previewVehicleId = previewVehicles[player.GetPlayerId()];

            IVehicle previewVehicle = Alt.Server.GetVehicles().First(i => i.Id == previewVehicleId);

            if (previewVehicle == null)
            {
                player.EmitLocked("dealership:CloseView");
                return;
            }

            DegreeRotation currentVehicleRotation = previewVehicle.Rotation;

            previewVehicle.Rotation = currentVehicleRotation;
        }

        public static void OnPreviewVehicleColorSelect(IPlayer player, string newColor)
        {
            if (player == null) return;
            ushort previewVehicleId = previewVehicles[player.GetPlayerId()];

            IVehicle previewVehicle = Alt.Server.GetVehicles().First(i => i.Id == previewVehicleId);

            if (previewVehicle == null)
            {
                player.EmitLocked("dealership:CloseView");
                return;
            }

            LsvColor previewColor = new LsvColor();

            switch (newColor)
            {
                case "red":
                    previewColor = new LsvColor(Color.DarkRed);
                    break;

                case "blue":
                    previewColor = new LsvColor(Color.DarkBlue);
                    break;

                case "white":
                    previewColor = new LsvColor(Color.White);
                    break;

                case "green":
                    previewColor = new LsvColor(Color.DarkGreen);
                    break;

                case "orange":
                    previewColor = new LsvColor(Color.DarkOrange);
                    break;

                case "black":
                    previewColor = new LsvColor(Color.Black);
                    break;

                default:
                    previewColor = new LsvColor(Color.Black);
                    break;
            }

            previewVehicle.PrimaryColorRgb = new Rgba(previewColor.R, previewColor.G, previewColor.B, previewColor.A);

            previewVehicle.SecondaryColorRgb = new Rgba(previewColor.R, previewColor.G, previewColor.B, previewColor.A);

            player.SetData("dealership:PreviewColor", previewColor);
        }

        public static void OnPreviewSelectPurchase(IPlayer player)
        {
            if (player == null) return;

            PaymentHandler.ShowPaymentSelection(player, "dealership:purchaseVehiclePaymentSelection");
        }

        public static void OnPaymentSelection(IPlayer player, string paymentOption)
        {
            try
            {
                player.FreezeInput(false);
                player.FreezePlayer(false);
                player.ChatInput(true);
                player.HideChat(false);
                player.ShowHud(true);
                OnWebViewClose(player);
                if (paymentOption == "IncorrectPin")
                {
                    player.SendInfoNotification($"You've inputted an incorrect pin.");
                    return;
                }

                if (paymentOption == "close")
                {
                    Console.WriteLine($"{player.GetClass().Name} has closed Dealership Payment Screen");
                    return;
                }

                Models.Character? playerCharacter = player?.FetchCharacter();

                if (playerCharacter == null)
                {
                    Console.WriteLine($"{player.GetClass().Name} character null (299 @ dealershipHandler.cs)");
                    return;
                }

                bool hasDealershipVehicleData =
                    player.GetData("dealership:selectedPreviewVehicle", out string previewVehicleJson);

                if (!hasDealershipVehicleData)
                {
                    Console.WriteLine($"DealershipHandler : Line 308");
                    return;
                }

                DealershipVehicle selectedDealershipVehicle =
                    JsonConvert.DeserializeObject<DealershipVehicle>(previewVehicleJson);

                if (selectedDealershipVehicle == null)
                {
                    Console.WriteLine($"DealershipHandler : Line 317");
                    return;
                }

                if (paymentOption == "cash")
                {
                    if (selectedDealershipVehicle.VehPrice > playerCharacter.Money)
                    {
                        player.SendErrorNotification("You don't have this much cash on you!");
                        return;
                    }

                    player.RemoveCash(selectedDealershipVehicle.VehPrice);

                    PurchaseVehicle(player, selectedDealershipVehicle);
                    return;
                }

                if (paymentOption == "voucher")
                {
                    if (!playerCharacter.VehicleVoucher)
                    {
                        player.SendErrorNotification("You don't have a vehicle voucher to use!");
                        return;
                    }

                    using Context voucherContext = new Context();

                    Models.Character dbPlayerCharacter = voucherContext.Character.Find(playerCharacter.Id);

                    if (dbPlayerCharacter == null) return;

                    dbPlayerCharacter.VehicleVoucher = false;

                    voucherContext.SaveChanges();

                    PurchaseVehicle(player, selectedDealershipVehicle);
                }

                bool accountNumberParse = long.TryParse(paymentOption, out long accountNumber);

                Console.WriteLine($"DealershipHandler : Line 360 : {accountNumber} - {accountNumberParse}");

                if (!accountNumberParse)
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                BankAccount selectedBankAccount = BankAccount.FetchBankAccount(accountNumber);

                if (selectedBankAccount == null)
                {
                    player.SendErrorNotification("An error occurred getting the bank data.");
                    return;
                }

                if (selectedBankAccount.OwnerId != playerCharacter.Id)
                {
                    player.SendErrorNotification("You can't purchase a vehicle with that card!");
                    return;
                }

                if (selectedDealershipVehicle.VehPrice > selectedBankAccount.Balance)
                {
                    player.SendErrorNotification("You don't have this amount in the account!");
                    return;
                }

                using Context dbcontext = new Context();

                BankAccount dbBankAccount = dbcontext.BankAccount.Find(selectedBankAccount.Id);

                dbBankAccount.Balance -= selectedDealershipVehicle.VehPrice;

                List<BankTransaction> bankTransactions =
                    JsonConvert.DeserializeObject<List<BankTransaction>>(dbBankAccount.TransactionHistoryJson);

                BankTransaction newBankTransaction = new BankTransaction();

                newBankTransaction.TransactionTime = DateTime.Now;
                newBankTransaction.Amount = selectedDealershipVehicle.VehPrice;
                newBankTransaction.TransactionType = BankTransactionType.Purchase;

                bankTransactions.Add(newBankTransaction);

                dbBankAccount.TransactionHistoryJson = JsonConvert.SerializeObject(bankTransactions);

                dbcontext.SaveChanges();

                PurchaseVehicle(player, selectedDealershipVehicle);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }

        public static void PurchaseVehicle(IPlayer player, DealershipVehicle dealershipVehicle)
        {
            try
            {
                Models.Character playerCharacter = player.FetchCharacter();

                player.GetData("ATDEALERSHIP", out int dealershipId);

                Dealership currentDealership = Dealership.FetchDealership(dealershipId);

                bool previewColorData = player.GetData("dealership:PreviewColor", out LsvColor previewColor);

                if (!previewColorData)
                {
                    previewColor = new LsvColor(Color.Black);
                }

                Models.Vehicle newVehicle = new Models.Vehicle
                {
                    Id = 0,
                    Name = dealershipVehicle.VehName,
                    Plate = Utility.GenerateRandomString(8),
                    PosX = currentDealership.VehPosX,
                    PosY = currentDealership.VehPosY,
                    PosZ = currentDealership.VehPosZ,
                    RotZ = currentDealership.VehRotZ,
                    Dimension = 0,
                    Health = 100,
                    OwnerId = playerCharacter.Id,
                    Locked = false,
                    Engine = false,
                    Keycode = Utility.GenerateRandomString(8),
                    Spawned = false,
                    GarageId = null,
                    VehicleMods = JsonConvert.SerializeObject(new Dictionary<int, int>()),
                    Color1 = $"{previewColor.R},{previewColor.G},{previewColor.B}",
                    Color2 = $"{previewColor.R},{previewColor.G},{previewColor.B}",
                    FuelLevel = 100,
                    Odometer = 0,
                    VehiclePrice = dealershipVehicle.VehPrice,
                    StoredVehicles = JsonConvert.SerializeObject(new List<int>()),
                    IsStored = false
                };

                newVehicle.Model = dealershipVehicle.VehModel != 0 ? dealershipVehicle.VehModel.ToString() : dealershipVehicle.NewVehModel;

                Context context = new Context();

                context.Vehicle.Add(newVehicle);
                context.SaveChanges();

                int newVehicleId = newVehicle.Id;
                Inventory.Inventory inventory = player.FetchInventory();

                inventory.AddItem(new InventoryItem("ITEM_VEHICLE_KEY", newVehicle.Name, newVehicle.Keycode));

                float inventorySpace = 20;
                float inventoryCapacity = 5;

                int inventoryId = InventoryData.CreateDefaultInventory(inventorySpace, inventoryCapacity).Id;

                var vehicleDb = context.Vehicle.Find(newVehicleId);

                vehicleDb.InventoryId = inventoryId;

                context.SaveChanges();

                IVehicle newIVehicle = Vehicle.Commands.SpawnVehicleById(newVehicleId,
                    new Position(newVehicle.PosX, newVehicle.PosY, newVehicle.PosZ));

                newIVehicle.LockState = VehicleLockState.Unlocked;

                player.SendInfoNotification($"Your new {dealershipVehicle.VehName} is ready for you!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }
        }
    }
}