using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Groups;
using Server.Inventory;
using Server.Jobs.Taxi;
using Server.Models;

namespace Server.Phone
{
    public class PhoneCommands
    {
        [Command("phone", commandType: CommandType.Phone, description: "Used to access your various phone options")]
        public static void CommandPhone(IPlayer player)
        {
            Models.Character playerCharacter = player.FetchCharacter();

            if (!player.GetClass().Spawned || playerCharacter == null)
            {
                player.SendLoginError();
                return;
            }

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> phoneItems = playerInventory.GetInventory()
                .Where(x => x.Id == "ITEM_PHONE" || x.Id == "ITEM_EXPENSIVEPHONE").ToList();

            if (!phoneItems.Any())
            {
                player.SendErrorNotification("You don't have any phones.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            foreach (InventoryItem phoneItem in phoneItems)
            {
                menuItems.Add(new NativeMenuItem($"Number: {phoneItem.ItemValue}", phoneItem.CustomName));
            }

            NativeMenu phoneMenu = new NativeMenu("PhoneSystemSelectPhone", "Phone", "Select a phone", menuItems);

            NativeUi.ShowNativeMenu(player, phoneMenu, true);
        }

        public static void EventPhoneSelected(IPlayer player, string option)
        {
            if (option == "Close") return;

            Inventory.Inventory playerInventory = player.FetchInventory();

            List<InventoryItem> phoneItems = playerInventory.GetInventory()
                .Where(x => x.Id == "ITEM_PHONE" || x.Id == "ITEM_EXPENSIVEPHONE").ToList();

            string phoneNumber = option.Split("Number: ")[1];

            InventoryItem phoneItem = phoneItems.FirstOrDefault(x => x.ItemValue == phoneNumber);

            Phones phone = Phones.FetchPhone(phoneNumber);

            if (phoneItem == null || phone == null)
            {
                player.SendErrorNotification("An error occurred fetching the phone information.");
                return;
            }

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>();

            if (!phone.TurnedOn)
            {
                menuItems.Add(new NativeMenuItem("Turn On"));
            }
            else
            {
                menuItems.Add(new NativeMenuItem("Turn Off"));
                menuItems.Add(new NativeMenuItem("Contacts"));
                menuItems.Add(new NativeMenuItem("Call"));
                menuItems.Add(new NativeMenuItem("SMS"));
                menuItems.Add(new NativeMenuItem("Set Active"));
            }

            NativeMenu phoneMenu = new NativeMenu("PhoneMenuSelectMainMenuItem", "Phone", "Select an option", menuItems);

            player.SetData("CURRENTPHONEINTERACTION", phone.PhoneNumber);

            NativeUi.ShowNativeMenu(player, phoneMenu, true);
        }

        public static void EventPhoneMainMenuSelected(IPlayer player, string option)
        {
            if (option == "Close")
            {
                player.DeleteData("CURRENTPHONEINTERACTION");
                return;
            }

            bool hasPhoneData = player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            if (!hasPhoneData)
            {
                player.SendErrorNotification("An error occurred fetching the phone information.");
                return;
            }

            if (option == "Turn On" || option == "Turn Off")
            {
                using Context context = new Context();

                Phones dbPhone = context.Phones.FirstOrDefault(x => x.PhoneNumber == phoneNumber);

                if (dbPhone == null)
                {
                    player.SendErrorNotification("An error occurred fetching the phone information.");
                    return;
                }

                dbPhone.TurnedOn = !dbPhone.TurnedOn;
                bool newStatus = dbPhone.TurnedOn;
                context.SaveChanges();
                

                string status = newStatus ? "turned on" : "turned off";

                player.SendInfoNotification($"You have {status} your phone.");
                return;
            }

            if (option == "Contacts")
            {
                List<NativeMenuItem> phoneContactsItems = new List<NativeMenuItem>
                {
                    new NativeMenuItem("Add Contact")
                };

                List<PhoneContact> phoneContacts = Phones.FetchContacts(Phones.FetchPhone(phoneNumber));

                if (phoneContacts.Any())
                {
                    foreach (PhoneContact phoneContact in phoneContacts)
                    {
                        phoneContactsItems.Add(new NativeMenuItem(phoneContact.Name, $"Number: {phoneContact.PhoneNumber}"));
                    }
                }

                NativeMenu phoneContactsMenu = new NativeMenu("PhoneSystemShowContactsMainMenu", "Phone", "Phone Contacts", phoneContactsItems);

                NativeUi.ShowNativeMenu(player, phoneContactsMenu, true);
                return;
            }

            if (option == "Call")
            {
                player.Emit("phone:showCallNumber");
                player.FreezeInput(true);
                player.FreezeCam(true);
                player.ChatInput(false);
                return;
            }

            if (option == "SMS")
            {
                player.Emit("phone:showSMSPage");
                player.FreezeInput(true);
                player.FreezeCam(true);
                player.ChatInput(false);
                return;
            }

            if (option == "Set Active")
            {
                using Context context = new Context();

                Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

                playerCharacter.ActivePhoneNumber = phoneNumber;

                context.SaveChanges();
                

                player.SendInfoNotification($"You have set phone number {phoneNumber} as your active number!");
                return;
            }
        }

        public static void EventPhoneSystemContactSelected(IPlayer player, string option)
        {
            if (option == "Close")
            {
                player.DeleteData("CURRENTPHONEINTERACTION");
                return;
            }

            bool hasPhoneData = player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            if (!hasPhoneData)
            {
                player.SendErrorNotification("An error occurred fetching the phone information.");
                return;
            }

            Phones phone = Phones.FetchPhone(phoneNumber);

            if (option == "Add Contact")
            {
                player.Emit("phone:showAddContactPage");
                player.FreezeInput(true);
                player.FreezeCam(true);
                player.ChatInput(false);
                return;
            }

            List<PhoneContact> phoneContacts = Phones.FetchContacts(phone);

            PhoneContact selectedContact = phoneContacts.FirstOrDefault(x => x.Name == option);

            if (selectedContact == null)
            {
                player.SendErrorNotification("Unable to find this contact in your phone.");
                return;
            }

            player.SetData("CURRENTPHONECONTACTSELECTED", selectedContact.Name);

            List<NativeMenuItem> menuItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Call"),
                new NativeMenuItem("SMS"),
                new NativeMenuItem("Remove Contact")
            };

            NativeMenu phoneContactMenu = new NativeMenu("PhoneSystemContactsSubMenu", "Phone", "Select an option", menuItems);

            NativeUi.ShowNativeMenu(player, phoneContactMenu, true);
        }

        public static void EventPhoneSystemContactSubMenu(IPlayer player, string option)
        {
            if (option == "Close")
            {
                player.DeleteData("CURRENTPHONEINTERACTION");
                return;
            }

            bool hasPhoneData = player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            if (!hasPhoneData)
            {
                player.SendErrorNotification("An error occurred fetching the phone information.");
                return;
            }

            Phones phone = Phones.FetchPhone(phoneNumber);

            bool hasContactData = player.GetData("CURRENTPHONECONTACTSELECTED", out string contactName);

            if (!hasContactData)
            {
                player.SendErrorNotification("An error occurred fetching the contact information.");
                return;
            }

            PhoneContact phoneContact = Phones.FetchContacts(phone).FirstOrDefault(x => x.Name == contactName);

            if (phoneContact == null)
            {
                player.SendErrorNotification("An error occurred fetching the contact information.");
                return;
            }

            
            if (option == "Remove Contact")
            {
                using Context context = new Context();

                Phones dbPhone = context.Phones.Find(phone.Id);

                List<PhoneContact> phoneContacts =
                    JsonConvert.DeserializeObject<List<PhoneContact>>(dbPhone.ContactList);

                PhoneContact removeContact = phoneContacts.FirstOrDefault(x =>
                    x.Name == phoneContact.Name && x.PhoneNumber == phoneContact.PhoneNumber);

                phoneContacts.Remove(removeContact);

                dbPhone.ContactList = JsonConvert.SerializeObject(phoneContacts);

                context.SaveChanges();
                

                player.SendInfoNotification($"You have successfully removed {phoneContact.Name} from your contact list.");
                return;
            }

            Phones contactPhone = Phones.FetchPhone(phoneContact.PhoneNumber);

            if (contactPhone == null)
            {
                player.SendErrorNotification("An error occurred fetching the contact information.");
                return;
            }

            if (option == "Call")
            {
                PhoneCallHandler.StartPhoneCall(player, phone, contactPhone);
                return;
            }

            if (option == "SMS")
            {
                player.Emit("phone:showSMSContact");
                player.FreezeInput(true);
                player.FreezeCam(true);
                player.ChatInput(false);
                return;
            }

        }

        public static void HandleCallNumber(IPlayer player, string number)
        {
            player.FreezeInput(false);
            player.FreezeCam(false);
            player.ChatInput(true);

            bool hasPhoneData = player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            if (!hasPhoneData)
            {
                player.SendErrorNotification("An error occurred fetching the phone information.");
                return;
            }

            Phones phone = Phones.FetchPhone(phoneNumber);

            switch (number)
            {
                case "911":
                    Handler911.Start911Call(player, phone);
                    return;
                case "311":
                    Handler911.Start311Call(player, phone);
                    return;
                case "5555":
                    CallHandler.StartTaxiCall(player, phone);
                    return;
            }

            Phones targetPhone = Phones.FetchPhone(number);

            if (targetPhone == null)
            {
                player.SendErrorNotification("Number not found!");
                return;
            }

            PhoneCallHandler.StartPhoneCall(player, phone, targetPhone);
        }

        [Command("pickup")]
        public static void CommandPickupPhone(IPlayer player)
        {
            bool hasData = player.GetData("PHONERINGING", out bool phoneRinging);

            if (!hasData || !phoneRinging)
            {
                player.SendErrorNotification("Your phone isn't ringing!");
                return;
            }

            player.SetData("PHONEANSWERED", 1);
        }

        [Command("hangup")]
        public static void CommandHangupPhone(IPlayer player)
        {
            bool hasRingData = player.GetData("PHONERINGING", out bool phoneRinging);

            if (hasRingData && phoneRinging)
            {
                // Phone Ringing - Not yet answered
                player.SetData("PHONEANSWERED", 2);
                return;
            }

            bool isCallingData = player.GetData("ISCALLINGSOMEONE", out bool isCalling);



            bool hasData = player.GetData("ONPHONEWITH", out int onPhoneWith);

            if (!hasData || onPhoneWith == 0)
            {
                if (!isCallingData || !isCalling)
                {
                    // Not on a phone call
                    player.SendErrorNotification("You're not on a phone call.");
                    return;
                }

            }

            if (isCalling)
            {
                player.GetData("ISCALLINGCHARACTER", out int targetCharacterId);

                IPlayer targetCaller = Alt.Server.GetPlayers()
                    .FirstOrDefault(x => x.GetClass().CharacterId == targetCharacterId);

                if (targetCaller != null)
                {
                    
                    targetCaller.SendPhoneMessage("They've hung up.");
                    targetCaller.SendEmoteMessage("puts their phone away.");
                    //targetCaller.SetData("ONPHONEWITH", 0);
                    targetCaller.SetData("PHONEANSWERED", 2);
                    targetCaller.Emit("phone:stopPhoneRinging");
                }

                player.SetData("ISCALLINGSOMEONE", false);

                player.Emit("phone:stopPhoneRinging");
                player.SendPhoneMessage("You've hung up.");
                player.SendEmoteMessage("puts their phone away.");
                player.SetData("PHONEANSWERED", 0);
                player.SetData("ONPHONEWITH", 0);
                player.SetData("PHONERINGING", false);

                return;
            }

            // On an active call

            IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == onPhoneWith);

            if (targetPlayer != null)
            {
                targetPlayer.SendPhoneMessage("They've hung up.");
                targetPlayer.SendEmoteMessage("puts their phone away.");
                targetPlayer.SetData("ONPHONEWITH", 0);
                targetPlayer.Emit("phone:stopPhoneRinging");
            }
            
            player.SetData("ISCALLINGSOMEONE", false);

            player.Emit("phone:stopPhoneRinging");
            player.SendPhoneMessage("You've hung up.");
            player.SendEmoteMessage("puts their phone away.");
            player.SetData("PHONEANSWERED", 0);
            player.SetData("ONPHONEWITH", 0);
            player.SetData("PHONERINGING", false);
        }

        [Command("p", onlyOne: true, commandType: CommandType.Phone, description: "Used to talk on the phone")]
        public static void CommandPhoneChat(IPlayer player, string args = "")
        {
            if (args == "" || args.Trim().Length == 0)
            {
                player.SendInfoNotification("Usage: /p [Message]");
                return;
            }

            bool has911CallData = player.GetData("911:onCall", out int on911Call);

            if (has911CallData)
            {
                if (on911Call == 1)
                {
                    Handler911.On911CallMessage(player, on911Call, args.Trim());
                    return;
                }
            }
            
            bool has311CallData = player.GetData("311:onCall", out int on311Call);
            {
                if (has311CallData)
                {
                    if (on311Call == 1)
                    {
                        Handler911.On311CallMessage(player, on311Call, args.Trim());
                        return;
                    }
                }
            }

            bool hasTaxiCallData = player.GetData("taxi:onCall", out int onTaxiCall);

            if (hasTaxiCallData)
            {
                if (onTaxiCall > 0)
                {
                    CallHandler.OnDestinationReturn(player, args.Trim());
                    return;
                }
            }

            bool hasData = player.GetData("ONPHONEWITH", out int onPhoneWith);

            if (!hasData || onPhoneWith == 0)
            {
                // Not on a phone call
                player.SendErrorNotification("You're not on a phone call.");
                return;
            }

            IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x.GetPlayerId() == onPhoneWith);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Target player not found!");
                player.SendEmoteMessage("puts their phone away.");
                player.SetData("ONPHONEWITH", 0);
                return;
            }

            ChatHandler.SendMessageToNearbyPlayers(player, args.Trim(), MessageType.LocalPhone);

            targetPlayer.SendPhoneMessage($"Caller: {args.Trim()}");
        }

        [Command("call", onlyOne: true, commandType: CommandType.Phone, description: "Used to call someone with your active phone")]
        public static void CommandCallNumber(IPlayer player, string args = "")
        {
            if (args == "" || args.Trim().Length == 0)
            {
                player.SendSyntaxMessage("/call [Name/Number]");
                return;
            }

            Phones phone = Phones.FetchPhone(player.FetchCharacter().ActivePhoneNumber);

            switch (args)
            {
                case "911":
                    Handler911.Start911Call(player, phone);
                    return;
                case "311":
                    Handler911.Start311Call(player, phone);
                    return;
                case "5555":
                    CallHandler.StartTaxiCall(player, phone);
                    return;
            }

            Phones targetPhone = Phones.FetchPhone(args);

            if (targetPhone == null)
            {
                List<PhoneContact> contactList = JsonConvert.DeserializeObject<List<PhoneContact>>(phone.ContactList);

                PhoneContact contact = contactList.FirstOrDefault(x => string.Equals(x.Name, args, StringComparison.CurrentCultureIgnoreCase));

                if (contact == null)
                {
                    player.SendErrorNotification("Number not found!");
                    return;
                }

                targetPhone = Phones.FetchPhone(contact.PhoneNumber);

                if (targetPhone == null)
                {
                    player.SendErrorNotification("Number not found!");
                    return;
                }
            }

            PhoneCallHandler.StartPhoneCall(player, phone, targetPhone);
        }

        [Command("sms", onlyOne: true, commandType: CommandType.Phone, description: "Used to sms someone with your active phone")]
        public static void SmsNumber(IPlayer player, string args = "")
        {
            player.FreezeInput(false);
            player.FreezeCam(false);
            player.ChatInput(true);
            string[] split = args.Split(' ');

            string number = split[0];

            string[] messageArray = split.Skip(1).ToArray();

            string message = string.Join(' ', messageArray);

            if (player == null) return;

            if (number.Length == 0 || message.Length == 0)
            {
                player.SendSyntaxMessage("Usage: /sms [Number] [Message]");
                return;
            }

            player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            Phones phone = Phones.FetchPhone(phoneNumber) ?? Phones.FetchPhone(player.FetchCharacter().ActivePhoneNumber);

            if (phone == null)
            {
                player.SendErrorNotification("An error occurred fetching your phone.");
                return;
            }

            Phones targetPhone = Phones.FetchPhone(number) ?? Phones.FetchPhone(JsonConvert.DeserializeObject<List<PhoneContact>>(phone.ContactList).FirstOrDefault(x => x.Name == number)?.PhoneNumber);

            if (targetPhone == null)
            {
                player.SendErrorNotification("This number doesn't exist!");
                return;
            }

            IPlayer targetPlayer =
                Alt.Server.GetPlayers().FirstOrDefault(x => x.FetchCharacter()?.Id == targetPhone.CharacterId);

            if (targetPlayer == null || !targetPhone.TurnedOn)
            {
                player.SendPhoneMessage("Phone is turned off.");
                return;
            }

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            if (targetInventory == null)
            {
                player.SendPhoneMessage("Phone is turned off.");
                return;
                
            }

            List<InventoryItem> phoneItems = targetInventory.GetInventory().Where(x => x.Id.Contains("PHONE")).ToList();

            bool hasPhone = false;

            foreach (InventoryItem inventoryItem in phoneItems)
            {
                if (inventoryItem.ItemValue == targetPhone.PhoneNumber)
                {
                    hasPhone = true;
                }
            }

            if (!hasPhone)
            {
                player.SendPhoneMessage("Phone is turned off.");
                return;
            }

            List<PhoneContact> playerContacts = JsonConvert.DeserializeObject<List<PhoneContact>>(phone.ContactList);

            PhoneContact playerContact = playerContacts.FirstOrDefault(x => x.PhoneNumber == targetPhone.PhoneNumber);

            List<PhoneContact> targetContacts =
                JsonConvert.DeserializeObject<List<PhoneContact>>(targetPhone.ContactList);

            PhoneContact targetContact = targetContacts.FirstOrDefault(x => x.PhoneNumber == phone.PhoneNumber);

            player.SendSmsMessage(playerContact != null
                ? $"To [{playerContact.PhoneNumber} ({playerContact.Name})]: {message}"
                : $"To {targetPhone.PhoneNumber}: {message}");

            player.Emit("phone:playSmsTone");

            targetPlayer.SendSmsMessage(targetContact != null ? $"From [{targetContact.PhoneNumber} ({targetContact.Name})]: {message} [Received on: {targetPhone.PhoneNumber}]" : $"From {phone.PhoneNumber}: {message} [Received on: {targetPhone.PhoneNumber}]");

            targetPlayer.Emit("phone:playSmsTone");

            PhoneMessage playerPhoneMessage = new PhoneMessage
            {
                PhoneNumber = targetPhone.PhoneNumber,
                MessageText = message,
                Time = DateTime.Now,
                SentRecieved = PhoneMessageState.Sent
            };

            PhoneMessage targetPhoneMessage = new PhoneMessage
            {
                PhoneNumber = phone.PhoneNumber,
                MessageText = message,
                Time = DateTime.Now,
                SentRecieved = PhoneMessageState.Recieved
            };

            using Context context = new Context();

            var playerPhone = context.Phones.Find(phone.Id);

            var playerPhoneMessages = JsonConvert.DeserializeObject<List<PhoneMessage>>(playerPhone.MessageHistory);

            playerPhoneMessages.Add(playerPhoneMessage);

            playerPhone.MessageHistory = JsonConvert.SerializeObject(playerPhoneMessages);

            var targetPhoneDb = context.Phones.Find(targetPhone.Id);

            var targetPhoneMessages = JsonConvert.DeserializeObject<List<PhoneMessage>>(targetPhoneDb.MessageHistory);

            targetPhoneMessages.Add(targetPhoneMessage);

            targetPhoneDb.MessageHistory = JsonConvert.SerializeObject(targetPhoneMessages);

            context.SaveChanges();
            

        }

        public static void HandleSmsContact(IPlayer player, string message)
        {
            player.FreezeInput(false);
            player.FreezeCam(false);
            player.ChatInput(true);

            player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            Phones phone = Phones.FetchPhone(phoneNumber) ?? Phones.FetchPhone(player.FetchCharacter().ActivePhoneNumber);

            if (phone == null)
            {
                player.SendErrorNotification("An error occurred fetching your phone.");
                return;
            }

            bool hasContactData = player.GetData("CURRENTPHONECONTACTSELECTED", out string contactName);

            if (!hasContactData)
            {
                player.SendErrorNotification("An error occurred fetching the contact information.");
                return;
            }

            PhoneContact phoneContact = Phones.FetchContacts(phone).FirstOrDefault(x => x.Name == contactName);

            if (phoneContact == null)
            {
                player.SendErrorNotification("An error occurred fetching the contact information.");
                return;
            }

            Phones contactPhone = Phones.FetchPhone(phoneContact.PhoneNumber);

            if (contactPhone == null)
            {
                player.SendErrorNotification("An error occurred fetching the contact information.");
                return;
            }

            SmsNumber(player, $"{phoneContact.PhoneNumber} {message}");
        }

        public static void AddContact(IPlayer player, string name, string number)
        {
            player.FreezeInput(false);
            player.FreezeCam(false);
            player.ChatInput(true);

            player.GetData("CURRENTPHONEINTERACTION", out string phoneNumber);

            Phones phone = Phones.FetchPhone(phoneNumber) ?? Phones.FetchPhone(player.FetchCharacter().ActivePhoneNumber);

            if (phone == null)
            {
                player.SendErrorNotification("An error occurred fetching your phone.");
                return;
            }

            PhoneContact newContact = new PhoneContact
            {
                Name = name,
                PhoneNumber = number
            };


            Context context = new Context();

            Phones phoneDb = context.Phones.Find(phone.Id);

            List<PhoneContact> phoneContacts = JsonConvert.DeserializeObject<List<PhoneContact>>(phoneDb.ContactList);

            if (phoneContacts.FirstOrDefault(x => x.Name.ToLower() == name) != null)
            {
                player.SendPhoneMessage("Error: A contact already exists with this name.");
                return;
            }

            phoneContacts.Add(newContact);

            phoneDb.ContactList = JsonConvert.SerializeObject(phoneContacts);

            context.SaveChanges();

            

            player.SendPhoneMessage($"You've added {newContact.Name} under the number {newContact.PhoneNumber}.");
        }

        [Command("contacts", commandType: CommandType.Phone, description: "View your phone contacts")]
        public static void PhoneCommandContacts(IPlayer player)
        {
            if(player?.FetchCharacter() == null)
            {
                player.SendLoginError();
                return;
            } 

            if (string.IsNullOrEmpty(player.FetchCharacter().ActivePhoneNumber))
            {
                player.SendErrorNotification("You need to set a phone number as active.");
                return;
            }

            List<NativeMenuItem> phoneContactsItems = new List<NativeMenuItem>
            {
                new NativeMenuItem("Add Contact")
            };

            List<PhoneContact> phoneContacts = Phones.FetchContacts(Phones.FetchPhone(player.FetchCharacter().ActivePhoneNumber));

            if (phoneContacts.Any())
            {
                foreach (PhoneContact phoneContact in phoneContacts)
                {
                    phoneContactsItems.Add(new NativeMenuItem(phoneContact.Name, $"Number: {phoneContact.PhoneNumber}"));
                }
            }

            NativeMenu phoneContactsMenu = new NativeMenu("PhoneSystemShowContactsMainMenu", "Phone", "Phone Contacts", phoneContactsItems);

            NativeUi.ShowNativeMenu(player, phoneContactsMenu, true);
            return;
        }
    }
}