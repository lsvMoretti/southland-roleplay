using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Character
{
    public class Payday
    {
        private static readonly float MaxTax = 1000f;
        private static readonly float MaxInterest = 5000f;
        private static float _paydayAmount = 150;

        public static int PaydayBoost = 1;

        /// <summary>
        /// Process the players payday
        /// </summary>
        /// <param name="player"></param>
        public static void ProcessPlayerPayday(IPlayer player)
        {
            try
            {
                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                if (playerCharacter == null)
                {
                    player.SendErrorNotification("An error occurred fetching your character information at Payday.");
                    return;
                }

                if (playerCharacter.PaydayAccount == 0)
                {
                    player.SendInfoNotification("You need to set a Bank Account up to accept paychecks!");
                    return;
                }

                if (playerCharacter.InJail) return;

                BankAccount mainBankAccount = BankAccount.FetchBankAccount(playerCharacter.PaydayAccount);

                if (mainBankAccount == null)
                {
                    player.SendErrorNotification($"Your paycheck bAccount isn't valid");
                    return;
                }

                if (mainBankAccount.AccountType != BankAccountType.Debit)
                {
                    player.SendErrorNotification("Unable to receive your payday. You bank account is a savings type. It must be debit!");
                    return;
                }

                if (mainBankAccount.OwnerId != playerCharacter.Id)
                {
                    player.SendErrorNotification($"You need to set a Bank Account to accept Paychecks");
                    return;
                }

                _paydayAmount = playerCharacter.TotalHours <= 150 ? 550 : 150;

                if (PaydayBoost > 1)
                {
                    _paydayAmount = _paydayAmount * PaydayBoost;
                }

                List<BankAccount> playerBankAccounts = BankAccount.FindCharacterBankAccounts(playerCharacter);

                Faction activeFaction = Faction.FetchFaction(playerCharacter.ActiveFaction);

                float totalInterest = 0;

                foreach (BankAccount playerBankAccount in playerBankAccounts)
                {
                    lock (playerBankAccount)
                    {
                        BankAccount bAccount = context.BankAccount.Find(playerBankAccount.Id);

                        if (bAccount.Disabled) continue;

                        //player.SendChatMessage($"Account Number: {bAccount.AccountNumber}");

                        if (bAccount.AccountType == BankAccountType.Debit)
                        {
                            float interestRate = 0.002f;
                            float interestAmount = (float)Math.Round(bAccount.Balance * interestRate);

                            float interestRemaining = MaxInterest - totalInterest;
                            if (Math.Round(interestRemaining) >= 1)
                            {
                                if (interestRemaining < interestAmount)
                                {
                                    interestAmount = interestRemaining;
                                    totalInterest += interestRemaining;
                                }
                                else
                                {
                                    totalInterest += interestAmount;
                                }
                            }
                            else
                            {
                                interestAmount = 0;
                            }

                            if (playerCharacter.PaydayAccount == bAccount.AccountNumber)
                            {
                                player.SendChatMessage($"Payday Earnings: {playerCharacter.PaydayAmount:C}");
                            }
                            player.SendChatMessage($"Previous Bank Balance: {bAccount.Balance:C}");
                            player.SendChatMessage($"Interest (Rate 0.02%): {interestAmount:C}");
                            bAccount.Balance += (int)interestAmount;

                            if (playerCharacter.PaydayAccount == bAccount.AccountNumber)
                            {
                                bAccount.Balance += playerCharacter.PaydayAmount;

                                playerCharacter.PaydayAmount = 0;

                                player.SendChatMessage($"Government Income: {_paydayAmount:C}");

                                bAccount.Balance += _paydayAmount;

                                if (activeFaction != null)
                                {
                                    if (activeFaction.SubFactionType == SubFactionTypes.Law)
                                    {
                                        bAccount.Balance += 170;
                                        player.SendChatMessage($"Law Salary: {170:C}.");
                                    }

                                    if (activeFaction.SubFactionType == SubFactionTypes.Medical)
                                    {
                                        bAccount.Balance += 170;
                                        player.SendChatMessage($"FD Salary: {170:C}.");
                                    }
                                    if (activeFaction.SubFactionType == SubFactionTypes.Government)
                                    {
                                        bAccount.Balance += 170;
                                        player.SendChatMessage($"Government Salary: {170:C}.");
                                    }
                                }

                                if (playerCharacter.RentingMotelRoom)
                                {
                                    int rentPrice = 25;

                                    List<Models.Motel> motels = context.Motels.ToList();

                                    foreach (Models.Motel motel in motels)
                                    {
                                        List<MotelRoom> motelRooms =
                                            JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

                                        if (motelRooms.Any(x => x.OwnerId == playerCharacter.Id))
                                        {
                                            MotelRoom room =
                                                motelRooms.FirstOrDefault(x => x.OwnerId == playerCharacter.Id);

                                            rentPrice = room.Value / 2;

                                            player.SendChatMessage($"Motel Rental: {rentPrice:C}.");
                                            bAccount.Balance -= rentPrice;
                                        }
                                    }
                                }
                            }

                            player.SendChatMessage($"New Bank Balance: {bAccount.Balance:C}");
                        }

                        if (bAccount.AccountType == BankAccountType.Credit)
                        {
                            float taxRate = 0.002f;
                            float taxAmount = Math.Min(bAccount.Balance * taxRate, MaxTax);

                            int paybackAmount = 0;

                            if (bAccount.Balance < bAccount.CreditLimit)
                            {
                                float paybackRate = 0.05f;
                                paybackAmount = (int)Math.Round(bAccount.Balance * paybackRate);
                            }

                            int totalAmount = (int)taxAmount + paybackAmount;

                            bAccount.Balance = -totalAmount;
                        }

                        if (bAccount.AccountType == BankAccountType.Savings)
                        {
                            float taxRate = 0.002f;
                            float taxAmount = Math.Min(bAccount.Balance * taxRate, MaxTax);

                            float interestRate = 0.007f;
                            float interestAmount = (float)Math.Round(bAccount.Balance * interestRate);

                            float interestRemaining = MaxInterest - totalInterest;
                            if (Math.Round(interestRemaining) >= 1)
                            {
                                if (interestRemaining < interestAmount)
                                {
                                    interestAmount = interestRemaining;
                                    totalInterest += interestRemaining;
                                }
                                else
                                {
                                    totalInterest += interestAmount;
                                }
                            }
                            else
                            {
                                interestAmount = 0;
                            }

                            player.SendNotification($"Previous Savings: ~g~{bAccount.Balance:C}.");
                            player.SendNotification($"Interest Amount: ~g~{interestAmount:C} ~w~@ {interestRate:P}. Taxed: ~r~{taxAmount:C} ~w~ @ {taxRate:P}.");

                            bAccount.Balance += (int)interestAmount;

                            bAccount.Balance -= (int)taxAmount;

                            player.SendNotification($"New Savings: ~g~{bAccount.Balance:C}");
                        }
                    }
                }

                playerCharacter.GraffitiCleanCount = 0;

                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}