using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Server.Character;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Server.Models;
using Blip = Server.Objects.Blip;

namespace Server.Bank
{
    public class BankHandler
    {
        public static readonly Dictionary<int, Dictionary<string, Position>> bankList = new Dictionary<int, Dictionary<string, Position>>();

        /// <summary>
        /// Loads all bank locations
        /// </summary>
        public static void InitializeBanks()
        {
            bankList.Add(1, new Dictionary<string, Position> { { "Fleeca Bank", new Position(149.9203f, -1040.654f, 28.07409f) } }); // Legion Square
            bankList.Add(2, new Dictionary<string, Position> { { "Fleeca Bank", new Position(314.2738f, -279.1122f, 54.17078f) } }); // Alta
            bankList.Add(3, new Dictionary<string, Position> { { "Fleeca Bank", new Position(-1212.964f, -330.2031f, 37.78702f) } }); // Rockford Hills
            bankList.Add(4, new Dictionary<string, Position> { { "Fleeca Bank", new Position(-2963.299f, 483.0803f, 15.70309f) } }); // Banham Canyon Bank
            bankList.Add(5, new Dictionary<string, Position> { { "Fleeca Bank", new Position(1175.218f, 2706.805f, 38.09403f) } }); // Grand Senora Desert Bank
            //bankList.Add(6, new Dictionary<string, Position> { { "Bank", new Position(-112.0607, 6469.162, 31.6267) } }); // Blaine County Savings Bank
            bankList.Add(6, new Dictionary<string, Position> { { "Fleeca Bank", new Position(-111.1049f, 6462.638f, 31.64077f) } }); // Outside BCSB
            bankList.Add(7, new Dictionary<string, Position> { { "Fleeca Bank", new Position(251.6458f, 221.6907f, 106.2866f) } }); // Pacific Standard Bank
            bankList.Add(8, new Dictionary<string, Position> { { "Fleeca Bank", new Position(-350.9675f, -49.89236f, 49.04258f) } }); // Burton bank

            foreach (KeyValuePair<int, Dictionary<string, Position>> keyValuePair in bankList)
            {
                foreach (KeyValuePair<string, Position> bankInfo in keyValuePair.Value)
                {
                    Blip bankBlip = new Blip(bankInfo.Key, bankInfo.Value, 108, 2, 1f);
                    bankBlip.Add();

                    TextLabel bankLabel = new TextLabel($"{bankInfo.Key}\nUsage: /bank", bankInfo.Value + new Position(0, 0, 0.5f), TextFont.FontChaletComprimeCologne, new LsvColor(Color.Green));
                    bankLabel.Add();
                }
            }
        }

        [Command("paymortgage", onlyOne:true, commandType: CommandType.Bank, description: "Used to pay off an existing mortgage")]
        public static void BankCommandPayMortgage(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            bool nearBank = false;

            foreach (KeyValuePair<int, Dictionary<string, Position>> bank in bankList)
            {
                if (bank.Value.FirstOrDefault().Value.Distance(player.Position) > 8) continue;

                nearBank = true;
            }

            if (!nearBank)
            {
                player.SendErrorNotification("You are not near a bank!");
                return;
            }

            bool tryParse = double.TryParse(args, out double paymentAmount);

            if (!tryParse)
            {
                player.SendSyntaxMessage("/paymortgage [Amount]");
                return;
            }

            if (paymentAmount < 1)
            {
                player.SendErrorNotification("Amount must be more than 0!");
                return;
            }

            if (player.GetClass().Cash < paymentAmount)
            {
                player.SendErrorNotification("You must have this much on you!");
                return;
            }

            Models.Property? mortgageProperty = Models.Property.FetchCharacterProperties(player.FetchCharacter()).FirstOrDefault(x => x.MortgageValue > 0);

            if (mortgageProperty == null)
            {
                player.SendErrorNotification("You don't have an outstanding mortgage!");
                return;
            }

            // Minimal Amount = 1% of left to pay
            double minAmount = mortgageProperty.MortgageValue * 0.01;

            if (paymentAmount < minAmount)
            {
                player.SendErrorNotification($"You must put in a minimum amount of {minAmount:C}.");
                return;
            }

            if (mortgageProperty.MortgageValue - minAmount < 0)
            {
                player.SendErrorNotification("The mortgage value and minimum amount are less than $0");
                return;
            }


            using Context context = new Context();

            Models.Property property = context.Property.Find(mortgageProperty.Id);

            if (property == null)
            {
                player.SendErrorNotification("Unable to find the property info.");
                return;
            }

            property.MortgageValue -= paymentAmount;
            property.LastMortgagePayment = DateTime.Now;
            player.RemoveCash(paymentAmount);

            context.SaveChanges();

            player.SendInfoNotification($"You've paid {paymentAmount:C} from your mortgage. Remaining left to pay is {property.MortgageValue:C}. Next payment due by {property.LastMortgagePayment.AddMonths(2)}");
        }

        [Command("mortgagevalue", commandType: CommandType.Bank, description:"Used to see remainder of mortgage to pay")]
        public static void BankCommandMortgageValue(IPlayer player)
        {

            if (!player.IsSpawned()) return;

            Models.Property? mortgageProperty = Models.Property.FetchCharacterProperties(player.FetchCharacter()).FirstOrDefault(x => x.MortgageValue > 0);

            if (mortgageProperty == null)
            {
                player.SendErrorNotification("You don't have an outstanding mortgage!");
                return;
            }

            // Minimal Amount = 1% of left to pay
            double minAmount = mortgageProperty.MortgageValue * 0.01;

            player.SendInfoNotification($"You have {mortgageProperty.MortgageValue:C} left to pay. Your minimum payment of {minAmount:C} is due by {mortgageProperty.LastMortgagePayment.AddMonths(2)}.");

        }

        [Command("bank", commandType: CommandType.Bank, description: "Shows the bank menu when at a bank")]
        public static void BankCommand(IPlayer player)
        {
            if (player?.FetchCharacter() == null) return;

            bool nearBank = false;

            foreach (KeyValuePair<int, Dictionary<string, Position>> bank in bankList)
            {
                if (bank.Value.FirstOrDefault().Value.Distance(player.Position) > 8) continue;

                nearBank = true;
            }

            if (!nearBank)
            {
                player.SendErrorNotification("You are not near a bank!");
                return;
            }

            ShowBankMenu(player);

            bool hasWelcomeData = player.GetData(WelcomePlayer.WelcomeData, out int welcomeStage);

            if (hasWelcomeData && welcomeStage == 2)
            {
                WelcomePlayer.OnBankCommand(player);
                return;
                ;
            }
        }

        /// <summary>
        /// Shows the Bank Menu to the player
        /// </summary>
        /// <param name="player"></param>
        public static void ShowBankMenu(IPlayer player)
        {
            player.FreezeCam(true);
            player.FreezeInput(true);
            player.Emit("ShowBankMenu", JsonConvert.SerializeObject(BankAccount.FindCharacterBankAccounts(player.FetchCharacter()).Where(x => !x.Disabled)));
        }

        /// <summary>
        /// Comes from JS event when Withdrawing / Depositing
        /// </summary>
        /// <param name="player"></param>
        /// <param name="accountNumber"></param>
        /// <param name="state">0 = Deposit, 1 = Withdraw</param>
        /// <param name="amount"></param>
        public static void HandleBankTransaction(IPlayer player, string accountNumberString, string stateString, string amountString)
        {
            if (player == null) return;

            player.FreezeCam(false);

            player.FreezeInput(false);

            bool accountNumberTry = long.TryParse(accountNumberString, out long accountNumber);

            if (!accountNumberTry)
            {
                player.SendErrorNotification("An error occurred fetching your account number!");
                return;
            }

            bool stateTry = int.TryParse(stateString, out int state);

            if (!stateTry)
            {
                player.SendErrorNotification("An error occurred handling the transaction.");
                return;
            }

            bool amountTry = double.TryParse(amountString, out double amount);

            if (!amountTry)
            {
                player.SendErrorNotification("An error occurred converting the amount!");
                return;
            }

            BankAccount bankAccount = BankAccount.FetchBankAccount(accountNumber);

            if (bankAccount == null)
            {
                player.SendErrorNotification("An error occurred fetching your account data.");
                return;
            }

            if (amount <= 0)
            {
                player.SendErrorNotification("You're unable to do this.");
                Logging.AddToCharacterLog(player, $"has tried to input a minus number in bank account {bankAccount.AccountNumber} - Value: {amount}");
                return;
            }

            float newBalance = bankAccount.Balance;
            string stateText = "deposited into";
            BankTransactionType transactionType = BankTransactionType.Deposit;

            switch (state)
            {
                case 0:
                    // Deposit
                    newBalance += (float)amount;
                    stateText = "deposited into";
                    transactionType = BankTransactionType.Deposit;
                    break;

                case 1:
                    // Withdraw
                    newBalance -= (float)amount;
                    stateText = "withdrawn from";
                    transactionType = BankTransactionType.Withdraw;
                    break;
            }

            using Context context = new Context();

            BankAccount playerBankAccount = context.BankAccount.Find(bankAccount.Id);

            if (playerBankAccount == null)
            {
                player.SendErrorNotification("An error occurred fetching your account data.");
                
                return;
            }

            List<BankTransaction> bankTransactions =
                JsonConvert.DeserializeObject<List<BankTransaction>>(playerBankAccount.TransactionHistoryJson);

            if (bankTransactions.Count > 50)
            {
                bankTransactions.Remove(bankTransactions.FirstOrDefault());
            }

            BankTransaction newTransaction = new BankTransaction
            {
                TransactionType = transactionType,
                Amount = (float)amount,
                TransactionTime = DateTime.Now,
                SenderAccount = 0,
                ReceiverAccount = 0
            };

            if (transactionType == BankTransactionType.Deposit)
            {
                newTransaction.SenderId = player.GetClass().CharacterId;

                if (amount > player.FetchCharacter().Money)
                {
                    player.SendErrorNotification("You don't have this much cash on you!");
                    
                    return;
                }
            }

            if (transactionType == BankTransactionType.Withdraw)
            {
                newTransaction.ReceiverId = player.GetClass().CharacterId;

                if (bankAccount.Balance < amount)
                {
                    player.SendErrorNotification("You don't have this much in your account!");
                    
                    return;
                }
            }

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (transactionType == BankTransactionType.Deposit)
            {
                playerCharacter.Money -= (float)amount;
                Logging.AddToCharacterLog(player, $"Has deposited {amount:C} to Bank Account Number: {bankAccount.AccountNumber}, ID: {bankAccount.Id}");
                Logging.AddToBankLog(bankAccount, $"{amount:C} has been deposited by {playerCharacter.Name}.");
            }
            if (transactionType == BankTransactionType.Withdraw)
            {
                playerCharacter.Money += (float)amount;
                Logging.AddToCharacterLog(player, $"Has withdrawn {amount:C} from Bank Account Number: {bankAccount.AccountNumber}, ID: {bankAccount.Id}");
                Logging.AddToBankLog(bankAccount, $"{amount:C} has been withdrawn by {playerCharacter.Name}.");
            }

            playerBankAccount.Balance = newBalance;
            bankTransactions.Add(newTransaction);

            playerBankAccount.TransactionHistoryJson = JsonConvert.SerializeObject(bankTransactions);

            context.SaveChanges();
            

            player.SendInfoNotification($"You have {stateText.Split(' ')[0]} {amount:C} {stateText.Split(' ')[1]} your Bank Account.");
        }

        /// <summary>
        /// Handles Bank Transfers from the bank UI
        /// </summary>
        /// <param name="player"></param>
        /// <param name="playerAccountString"></param>
        /// <param name="targetAccountString"></param>
        /// <param name="amountString"></param>
        public static void HandleBankTransfer(IPlayer player, string playerAccountString, string targetAccountString, string amountString)
        {
            if (player == null) return;

            player.FreezeCam(false);

            player.FreezeInput(false);

            bool parsePlayerAccount = long.TryParse(playerAccountString, out long playerAccountNumber);

            if (!parsePlayerAccount)
            {
                player.SendErrorNotification("An error occurred fetching your account information.");
                return;
            }

            bool parseTargetAccount = long.TryParse(targetAccountString, out long targetAccountNumber);

            if (!parseTargetAccount)
            {
                player.SendErrorNotification("An error occurred fetching the target account information.");
                return;
            }

            bool parseAmount = int.TryParse(amountString, out int amount);

            if (!parseAmount)
            {
                player.SendErrorNotification("Unable to convert the amount. Make sure it's number only!");
                return;
            }

            
            if (amount <= 0)
            {
                player.SendErrorNotification("You're unable to do this.");
                Logging.AddToCharacterLog(player, $"has tried to input a minus number in bank account {playerAccountNumber} - Value: {amount}");
                return;
            }

            using Context context = new Context();

            BankAccount playerBankAccount =
                context.BankAccount.FirstOrDefault(i => i.AccountNumber == playerAccountNumber);

            
            if (playerBankAccount == null)
            {
                player.SendErrorNotification("An error occurred fetching your account information.");
                return;
            }

            BankAccount targetBankAccount =
                context.BankAccount.FirstOrDefault(i => i.AccountNumber == targetAccountNumber);

            if (targetBankAccount == null)
            {
                player.SendErrorNotification("An error occurred fetching the target account information.");
                return;
            }

            if (playerBankAccount.Balance < amount)
            {
                player.SendErrorNotification("You don't have enough in your bank!");
                return;
            }

            BankTransaction playerBankTransaction = new BankTransaction
            {
                TransactionType = BankTransactionType.Transfer,
                SenderId = player.GetClass().CharacterId,
                ReceiverId = 0,
                Amount = amount,
                TransactionTime = DateTime.Now,
                SenderAccount = playerAccountNumber,
                ReceiverAccount = targetAccountNumber
            };

            List<BankTransaction> playerBankTransactions =
                JsonConvert.DeserializeObject<List<BankTransaction>>(playerBankAccount.TransactionHistoryJson);

            
            if (playerBankTransactions.Count > 50)
            {
                playerBankTransactions.Remove(playerBankTransactions.FirstOrDefault());
            }


            playerBankTransactions.Add(playerBankTransaction);

            playerBankAccount.TransactionHistoryJson = JsonConvert.SerializeObject(playerBankTransactions);

            List<BankTransaction> targetBankTransactions =
                JsonConvert.DeserializeObject<List<BankTransaction>>(targetBankAccount.TransactionHistoryJson);

            if (targetBankTransactions.Count > 100)
            {
                targetBankTransactions.Remove(targetBankTransactions.FirstOrDefault());
            }

            targetBankTransactions.Add(playerBankTransaction);

            targetBankAccount.TransactionHistoryJson = JsonConvert.SerializeObject(targetBankTransactions);

            playerBankAccount.Balance -= amount;
            targetBankAccount.Balance += amount;

            Logging.AddToCharacterLog(player, $"Has sent {amount:C} from Bank Account {playerBankAccount.AccountNumber}, ID: {playerBankAccount.Id} to Bank Account {targetBankAccount.AccountNumber}, ID: {targetBankAccount.Id}");

            Logging.AddToBankLog(playerBankAccount, $"{player.GetClass().Name} has sent {amount:C} to Bank Account {targetBankAccount.AccountNumber}, ID: {targetBankAccount.Id}");

            Logging.AddToBankLog(targetBankAccount, $"Has received {amount:C} by {player.GetClass().Name}, from Bank Account: {playerBankAccount.AccountNumber}, ID: {playerBankAccount.Id}");

            context.SaveChanges();

            

            player.SendInfoNotification($"You've sent {amount:C0} to Bank Account Number: {targetAccountNumber}.");
        }

        /// <summary>
        /// When the bank view is closed, this is triggered
        /// </summary>
        /// <param name="player"></param>
        public static void OnBankViewClose(IPlayer player)
        {
            if (player == null) return;

            player.FreezeCam(false);
            player.FreezeInput(false);
        }

        /// <summary>
        /// Creates new Bank Card & PIN on request from Bank Interface
        /// </summary>
        /// <param name="player"></param>
        /// <param name="accountNumberString"></param>
        public static void RequestNewBankCard(IPlayer player, string accountNumberString)
        {
            if (player == null) return;

            if (!long.TryParse(accountNumberString, out long accountNumber))
            {
                player.SendErrorNotification("An error occurred fetching your account data.");
                return;
            }

            if (!BankAccount.RequestNewCard(player, BankAccount.FetchBankAccount(accountNumber)))
            {
                player.SendErrorNotification("An error occurred requesting your new bank card!");
            }
        }

        /// <summary>
        /// Requests a bank account to be closed from the bank interface
        /// </summary>
        /// <param name="player"></param>
        /// <param name="accountNumberString"></param>
        public static void RequestAccountClosure(IPlayer player, string accountNumberString)
        {
            if (player == null) return;

            if (!long.TryParse(accountNumberString, out long accountNumber))
            {
                player.SendErrorNotification("An error occurred fetching your account data.");
                return;
            }

            using Context context = new Context();

            BankAccount playerBankAccount = context.BankAccount.FirstOrDefault(i => i.AccountNumber == accountNumber);

            if (playerBankAccount == null)
            {
                player.SendErrorNotification("Unable to find your bank account details!");
                
                return;
            }

            playerBankAccount.Disabled = true;

            context.SaveChanges();
            

            player.SendInfoNotification($"You have disabled your bank account with Account Number: {accountNumber}.");
        }

        public static void RequestNewBankAccount(IPlayer player, string accountTypeString)
        {
            if (player == null) return;

            BankAccountType accountType = accountTypeString switch
            {
                "debit" => BankAccountType.Debit,
                "savings" => BankAccountType.Savings,
                _ => BankAccountType.Debit
            };

            BankAccount newBankAccount = new BankAccount
            {
                OwnerId = player.GetClass().CharacterId,
                AccountType = accountType,
                AccountNumber = long.Parse(Utility.GenerateRandomNumber(6)),
                CardNumber = 0,
                Pin = 0,
                Balance = 0,
                TransactionHistoryJson = JsonConvert.SerializeObject(new List<BankTransaction>()),
                CreditLimit = 0,
                Disabled = false
            };
            if (BankAccount.FetchBankAccount(newBankAccount.AccountNumber) != null)
            {
                for (int i = 0; i < 1000; i++)
                {
                    long newAccountNumber = long.Parse(Utility.GenerateRandomNumber(6));

                    Console.WriteLine(newAccountNumber);

                    if (i == 999)
                    {
                        if (BankAccount.FetchBankAccount(newAccountNumber) != null)
                        {
                            player.SendInfoNotification("Unable to create a new bank account!");
                            return;
                        }
                    }

                    if (BankAccount.FetchBankAccount(newAccountNumber) == null)
                    {
                        newBankAccount.AccountNumber = newAccountNumber;
                        break;
                    }
                }
            }

            bool firstAccount = false;

            if (!BankAccount.FindCharacterBankAccounts(player.FetchCharacter()).Any())
            {
                newBankAccount.Balance = 1000;
                firstAccount = true;
            }

            Context context = new Context();

            context.BankAccount.Add(newBankAccount);

            if (firstAccount)
            {
                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                playerCharacter.PaydayAccount = newBankAccount.AccountNumber;
            }

            context.SaveChanges();
            

            if (newBankAccount.AccountType != BankAccountType.Savings)
            {
                BankAccount.RequestNewCard(player, newBankAccount);
            }

            player.SendInfoNotification($"You've created a new {new CultureInfo("en-us", false).TextInfo.ToTitleCase(accountTypeString)} account. Account Number: {newBankAccount.AccountNumber}");

            if (firstAccount)
            {
                player.SendInfoNotification($"You've received a welcome bonus into your new bank account!");
            }
        }

        public static void SetBankAccountAsActive(IPlayer player, string accNo)
        {
            long accountNumber = long.Parse(accNo);

            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("An error occurred fetching the account data.");
                return;
            }

            BankAccount paydayAccount = BankAccount.FetchBankAccount(accountNumber);

            if (paydayAccount.AccountType != BankAccountType.Debit)
            {
                player.SendErrorNotification("You must set this to a debit account!");
                return;
            }

            playerCharacter.PaydayAccount = accountNumber;

            context.SaveChanges();
            

            player.SendInfoNotification($"You've set {accountNumber} as your active payday account.");
        }
    }
}