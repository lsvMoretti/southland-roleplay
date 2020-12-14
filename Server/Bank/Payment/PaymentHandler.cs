using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Bank.Payment
{
    public class PaymentHandler
    {
        /// <summary>
        /// Returns Account Number if selected Bank Account after successful PIN inout. or 'cash' if cash was selected. Returns 'close' if screen closed. 'IncorrectPin' if incorrect PIN
        /// </summary>
        /// <param name="player"></param>
        /// <param name="returnEvent"></param>
        public static void ShowPaymentSelection(IPlayer player, string returnEvent)
        {
            if (player?.FetchCharacter() == null) return;

            List<InventoryItem> bankCards = player.FetchInventory().GetInventoryItems("ITEM_BANK_CARD");

            List<BankAccount> cardAccounts = bankCards.Select(bankCard => BankAccount.FetchBankAccount(long.Parse(bankCard.ItemValue))).ToList();

            List<BankAccount> allowedAccounts = new List<BankAccount>();

            foreach (BankAccount cardAccount in cardAccounts)
            {
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

            player.Emit("showPaymentScreen", JsonConvert.SerializeObject(bankCards), JsonConvert.SerializeObject(allowedAccounts), returnEvent);
        }
    }
}