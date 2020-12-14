using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Bank.Payment;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Apartments
{
    public class ApartmentCommand
    {
        [Command("buyapartment", commandType: CommandType.Property,
            description: "Apartments: Used to purchase an Apartment")]
        public static void ApartmentBuyCommand(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (player.FetchCharacter().InsideApartmentComplex == 0)
            {
                player.SendErrorNotification("Your not in an apartment.");
                return;
            }

            using Context context = new Context();
            var apartmentComplex = context.ApartmentComplexes.Find(player.FetchCharacter().InsideApartmentComplex);

            if (apartmentComplex == null)
            {
                player.SendErrorNotification("An error occurred fetching the Apartment Complex data.");
                return;
            }

            List<Apartment> complexApartments =
                JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList);

            if (!complexApartments.Any())
            {
                player.SendErrorNotification("An error occurred fetching the Apartments.");
                return;
            }

            Apartment apartment =
                complexApartments.FirstOrDefault(x => x.Name == player.FetchCharacter().InsideApartment);

            if (apartment == null)
            {
                player.SendErrorNotification("An error occurred fetching the Apartment.");
                return;
            }

            if (apartment.Owner != 0)
            {
                player.SendErrorNotification("This apartment is already owned.");
                return;
            }

            PaymentHandler.ShowPaymentSelection(player, "apartment:OnPaymentMethodReturn");
        }

        public static void OnPaymentSelection(IPlayer player, string option)
        {
            if (option == "IncorrectPin")
            {
                player.SendErrorNotification("Incorrect PIN.");
                return;
            }
            if (option == "close") return;

            var apartmentComplex =
                ApartmentComplexes.FetchApartmentComplex(player.FetchCharacter().InsideApartmentComplex);

            var apartment = JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList)
                .FirstOrDefault(x => x.Name == player.FetchCharacter().InsideApartment);

            if (option == "cash")
            {
                if (player.GetClass().Cash < apartment.Price)
                {
                    player.SendErrorNotification($"You don't have enough. You need {apartment.Price:C}.");
                    return;
                }

                player.RemoveCash(apartment.Price);

                ApartmentHandler.PurchaseApartment(player, apartmentComplex, apartment);
                return;
            }

            bool accountParse = long.TryParse(option, out long accountNumber);

            if (!accountParse)
            {
                player.SendErrorNotification("An error occurred fetching the account number.");
                return;
            }

            BankAccount bankAccount = BankAccount.FetchBankAccount(accountNumber);

            if (bankAccount == null)
            {
                player.SendErrorNotification("Bank account not found!");
                return;
            }

            if (bankAccount.OwnerId != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You must be the account holder.");
                return;
            }

            if (bankAccount.Balance < apartment.Price)
            {
                player.SendErrorNotification($"You don't have enough in your account. You need {apartment.Price:C}.");
                return;
            }

            BankAccount.UpdateBalance(bankAccount, bankAccount.Balance -= apartment.Price);

            ApartmentHandler.PurchaseApartment(player, apartmentComplex, apartment);
        }

        [Command("sellapartment", commandType: CommandType.Property,
            description: "Apartments: Used to sell your apartment.")]
        public static void ApartmentCommandSellApartment(IPlayer player)
        {
            if (!player.IsSpawned())
            {
                player.SendLoginError();
                return;
            }

            if (player.FetchCharacter().InsideApartmentComplex == 0)
            {
                player.SendErrorNotification("Your not in an apartment.");
                return;
            }

            using Context context = new Context();
            var apartmentComplex = context.ApartmentComplexes.Find(player.FetchCharacter().InsideApartmentComplex);

            if (apartmentComplex == null)
            {
                player.SendErrorNotification("An error occurred fetching the Apartment Complex data.");
                return;
            }

            List<Apartment> complexApartments =
                JsonConvert.DeserializeObject<List<Apartment>>(apartmentComplex.ApartmentList);

            if (!complexApartments.Any())
            {
                player.SendErrorNotification("An error occurred fetching the Apartments.");
                return;
            }

            Apartment apartment =
                complexApartments.FirstOrDefault(x => x.Name == player.FetchCharacter().InsideApartment);

            if (apartment == null)
            {
                player.SendErrorNotification("An error occurred fetching the Apartment.");
                return;
            }

            if (apartment.Owner != player.GetClass().CharacterId)
            {
                player.SendErrorNotification("You don't own this apartment.");
                return;
            }

            int halfPrice = apartment.Price / 2;

            player.AddCash(halfPrice);

            apartment.Owner = 0;
            apartment.KeyCode = null;
            apartment.Locked = false;

            complexApartments.Remove(apartment);
            complexApartments.Add(apartment);

            apartmentComplex.ApartmentList = JsonConvert.SerializeObject(complexApartments);

            context.SaveChanges();
            

            player.SendInfoNotification($"You've sold {apartment.Name} for {halfPrice:C}.");
        }
    }
}