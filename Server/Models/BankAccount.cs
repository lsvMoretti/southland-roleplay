using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Serilog;
using Server.Chat;
using Server.Extensions;
using Server.Inventory;

namespace Server.Models
{
    public class BankAccount
    {
        /// <summary>
        /// Unique Account ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Character ID of Bank Account Creator
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Account Type
        /// </summary>
        public BankAccountType AccountType { get; set; }

        /// <summary>
        /// Account Number for the Account
        /// </summary>
        public long AccountNumber { get; set; }

        /// <summary>
        /// The active Card for the account
        /// </summary>
        public long CardNumber { get; set; }

        /// <summary>
        /// The active PIN for the account
        /// </summary>
        public int Pin { get; set; }

        /// <summary>
        /// Cash balance for the Account
        /// </summary>
        public float Balance { get; set; }

        /// <summary>
        /// A history of Transactions, saved as JSON string? from *BankTransaction*
        /// </summary>
        public string? TransactionHistoryJson { get; set; }

        /// <summary>
        /// The amount of Credit the account can have
        /// </summary>
        public int CreditLimit { get; set; }

        /// <summary>
        /// Account is Disabled?
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Adds a BankAccount to the Database
        /// </summary>
        /// <param name="account"></param>
        public static void AddBankAccount(BankAccount account)
        {
            using Context context = new Context();
            context.BankAccount.Add(account);
            context.SaveChanges();
            context.SaveChanges();
        }

        /// <summary>
        /// Fetches a Bank Account by Account Number
        /// </summary>
        /// <param name="AccountNumber"></param>
        /// <returns></returns>
        public static BankAccount FetchBankAccount(long AccountNumber)
        {
            using Context context = new Context();
            return context.BankAccount.FirstOrDefault(i => i.AccountNumber == AccountNumber);
        }

        /// <summary>
        /// Fetches Bank Account By Card Number
        /// </summary>
        /// <param name="CardNumber"></param>
        /// <returns></returns>
        public static BankAccount FetchAccountByCard(long CardNumber)
        {
            using Context context = new Context();
            return context.BankAccount.FirstOrDefault(i => i.CardNumber == CardNumber);
        }

        /// <summary>
        /// Fetches a List of BankAccount's by Character ID
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static List<BankAccount> FindCharacterBankAccounts(Character character)
        {
            using Context context = new Context();

            List<BankAccount> bankAccounts = context.BankAccount.Where(i => i.OwnerId == character.Id).ToList();

            foreach (BankAccount bankAccount in bankAccounts)
            {
                List<BankTransaction> transactions =
                    JsonConvert.DeserializeObject<List<BankTransaction>>(bankAccount.TransactionHistoryJson);

                if (transactions.Count > 50)
                {
                    transactions = transactions.Skip(Math.Max(0, transactions.Count() - 50)).ToList();
                    bankAccount.TransactionHistoryJson = JsonConvert.SerializeObject(transactions);
                    Logging.AddToBankLog(bankAccount, $"Taken last 50 bank account transactions");
                }
            }

            context.SaveChanges();

            return bankAccounts;
        }

        /// <summary>
        /// Updates the BankAccount with the total being newBalance
        /// </summary>
        /// <param name="account"></param>
        /// <param name="newBalance"></param>
        public static void UpdateBalance(BankAccount account, float newBalance)
        {
            using Context context = new Context();
            BankAccount bankAccount = context.BankAccount.Find(account.Id);

            bankAccount.Balance = newBalance;
            context.SaveChanges();
        }

        /// <summary>
        /// Transfer a specified amount across two bank accounts
        /// </summary>
        /// <param name="senderBankAccount">Sender Bank Account</param>
        /// <param name="receiverBankAccount">Receiver Bank Account</param>
        /// <param name="amount">Balance to transfer</param>
        /// <returns>True if successful</returns>
        public static bool TransferAmount(BankAccount senderBankAccount, BankAccount receiverBankAccount, int amount)
        {
            if (senderBankAccount == null || receiverBankAccount == null) return false;

            using Context context = new Context();

            BankAccount senderBank = context.BankAccount.Find(senderBankAccount.Id);

            BankAccount receiverBank = context.BankAccount.Find(receiverBankAccount.Id);

            if (senderBank == null || receiverBank == null) return false;

            senderBank.Balance -= amount;

            receiverBank.Balance += amount;

            context.SaveChanges();

            return true;
        }

        /// <summary>
        /// Requests a new Card Number and Pin Number.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bankAccount"></param>
        /// <returns>True = Added to Inventory, False = Not added</returns>
        public static bool RequestNewCard(IPlayer player, BankAccount bankAccount)
        {
            if (player == null || bankAccount == null) return false;

            Inventory.Inventory playerInventory = player.FetchInventory();

            if (playerInventory == null) return false;

            long cardNumber = long.Parse(Utility.GenerateRandomNumber(8));

            int newPin = int.Parse(Utility.GenerateRandomNumber(4));

            InventoryItem newBankCard = new InventoryItem("ITEM_BANK_CARD", $"Bank Card: {cardNumber}", bankAccount.AccountNumber.ToString());

            Context context = new Context();

            BankAccount databaseAccount = context.BankAccount.Find(bankAccount.Id);

            if (databaseAccount == null)
            {
                return false;
            }

            databaseAccount.CardNumber = cardNumber;
            databaseAccount.Pin = newPin;

            context.SaveChanges();

            playerInventory.AddItem(newBankCard);

            player.SendInfoNotification($"You have a new Bank Card. Number: {cardNumber}. PIN: {newPin}.");

            Logging.AddToCharacterLog(player, $"Has requested a new Bank Card: {cardNumber} and PIN {newPin} for Bank Account {bankAccount.AccountNumber}, ID: {bankAccount.Id}");

            Logging.AddToBankLog(bankAccount, $"Has been set a new Bank Card: {cardNumber} and PIN {newPin} by {player.GetClass().Name}");

            return true;
        }
    }

    public enum BankAccountType
    {
        Debit,
        Credit,
        Savings
    }

    public enum BankTransactionType { Atm, Withdraw, Deposit, Transfer, Purchase }

    public class BankTransaction
    {
        /// <summary>
        /// Type of Transaction
        /// </summary>
        public BankTransactionType TransactionType { get; set; }

        /// <summary>
        /// The character ID that sent the money
        /// Used for Deposit and Transfers
        /// </summary>
        public int SenderId { get; set; }

        /// <summary>
        /// The character ID that received the money.
        /// Used with ATM, Withdraw
        /// </summary>
        public int ReceiverId { get; set; }

        /// <summary>
        /// The Amount of Money
        /// </summary>
        public float Amount { get; set; }

        /// <summary>
        /// The Time of Transaction
        /// </summary>
        public DateTime TransactionTime { get; set; }

        /// <summary>
        /// The Account that sent the transaction
        /// Used for Transfers
        /// </summary>
        public long SenderAccount { get; set; }

        /// <summary>
        /// The Account that received the transaction
        /// Used for Transfers
        /// </summary>
        public long ReceiverAccount { get; set; }
    }
}