using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Bank
{
    public class AtmHandler
    {
        /// <summary>
        /// ATM Page loaded
        /// </summary>
        /// <param name="player"></param>
        public static void AtmPageLoaded(IPlayer player)
        {
            if (player?.FetchCharacter() == null) return;

            List<InventoryItem> bankCards = player.FetchInventory().GetInventoryItems("ITEM_BANK_CARD");

            if (!bankCards.Any())
            {
                player.Emit("atm:NoBankCards");
                return;
            }

            List<BankAccount> cardAccounts = bankCards.Select(bankCard => BankAccount.FetchBankAccount(long.Parse(bankCard.ItemValue))).ToList();

            List<BankAccount> allowedAccounts = new List<BankAccount>();

            foreach (BankAccount cardAccount in cardAccounts)
            {
                if (cardAccount.WithdrawalBlocked) continue;
                if (!cardAccount.Disabled)
                {
                    allowedAccounts.Add(cardAccount);
                    continue;
                }

                InventoryItem bankCard = bankCards.FirstOrDefault(i => i.ItemValue == cardAccount.AccountNumber.ToString());

                bankCards.Remove(bankCard);
            }

            player.FreezeInput(true);
            player.ChatInput(false);
            player.FreezeCam(true);

            player.Emit("atm:loadCardData", JsonConvert.SerializeObject(bankCards), JsonConvert.SerializeObject(allowedAccounts));
        }

        public static void AtmPageClosed(IPlayer player)
        {
            if (player == null) return;

            player.FreezeInput(false);
            player.ChatInput(true);
            player.FreezeCam(false);
        }

        public static void AtmWithdrawFunds(IPlayer player, string accountNumberString, string balanceString)
        {
            if (player == null) return;

            bool accountNumberParse = long.TryParse(accountNumberString, out long accountNumber);

            if (!accountNumberParse)
            {
                player.SendErrorNotification("An error occurred fetching the account information.");
                return;
            }

            bool balanceParse = float.TryParse(balanceString, out float requestedAmount);

            if (!balanceParse)
            {
                player.SendErrorNotification("An error occurred parsing the balance amount.");
                return;
            }

            requestedAmount = (float)Math.Truncate(requestedAmount);

            if (requestedAmount < 1 || requestedAmount > 1000)
            {
                player.SendErrorNotification("You can only withdraw $1 - $1,000");
                return;
            }

            Context context = new Context();

            BankAccount? bankAccount = context.BankAccount.FirstOrDefault(i => i.AccountNumber == accountNumber);

            if (bankAccount == null)
            {
                player.SendErrorNotification("An error occurred fetching the bank account.");

                return;
            }

            List<BankTransaction> previousTransactions =
                JsonConvert.DeserializeObject<List<BankTransaction>>(bankAccount.TransactionHistoryJson);

            int recentCount = 0;

            DateTime now = DateTime.Now;

            DateTime lastDay = now.AddDays(-1);

            foreach (BankTransaction previousTransaction in previousTransactions)
            {
                if (previousTransaction.TransactionType != BankTransactionType.Atm) continue;

                int result = DateTime.Compare(previousTransaction.TransactionTime, lastDay);

                if (result > 0)
                {
                    // Withdrawal has been within
                    recentCount += 1;
                }
            }

            Console.WriteLine($"Now: {now} - lastDay: {lastDay} - recentCount: {recentCount}");

            if (recentCount >= 3)
            {
                // 3 transactions in last 24 hours
                player.SendErrorNotification("This transaction couldn't be completed.");
                return;
            }

            if (bankAccount.Balance < requestedAmount)
            {
                player.SendErrorNotification("You don't have that much in your account!");

                return;
            }

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendPermissionError();

                return;
            }

            BankTransaction bankTransaction = new BankTransaction
            {
                TransactionType = BankTransactionType.Atm,
                SenderId = 0,
                ReceiverId = playerCharacter.Id,
                Amount = requestedAmount,
                TransactionTime = DateTime.Now,
                SenderAccount = 0,
                ReceiverAccount = 0
            };

            List<BankTransaction> bankTransactions =
                JsonConvert.DeserializeObject<List<BankTransaction>>(bankAccount.TransactionHistoryJson);

            bankTransactions.Add(bankTransaction);

            bankAccount.TransactionHistoryJson = JsonConvert.SerializeObject(bankTransactions);
            bankAccount.Balance -= requestedAmount;
            playerCharacter.Money += requestedAmount;

            Logging.AddToCharacterLog(player, $"Has withdrawn {requestedAmount:C} from the ATM. Bank Account: {bankAccount.AccountNumber}, ID: {bankAccount.Id}");

            Logging.AddToBankLog(bankAccount, $"{player.GetClass().Name} has withdrawn {requestedAmount:C} from an ATM.");

            context.SaveChanges();

            player.SendInfoNotification($"You have have withdrawn {requestedAmount:C0} from the bank.");
        }

        public static void OnAtmPinIncorrect(IPlayer player, string accountNumber)
        {
            bool tryParse = long.TryParse(accountNumber, out long accNumber);

            if (!tryParse)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            using Context context = new Context();

            BankAccount? bankAccount = context.BankAccount.FirstOrDefault(x => x.AccountNumber == accNumber);

            if (bankAccount is null)
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            bankAccount.PinAttempts += 1;
            if (bankAccount.PinAttempts >= 3)
            {
                bankAccount.WithdrawalBlocked = true;
            }

            return;
        }
    }
}