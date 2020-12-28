using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Phone
{
    public class PhoneCallHandler
    {
        public static void StartPhoneCall(IPlayer caller, Phones callerPhone, Phones targetPhone)
        {
            if (caller == null || callerPhone == null || targetPhone == null) return;

            if (callerPhone == targetPhone)
            {
                caller.SendErrorNotification("You can't call yourself!");
                return;
            }

            if(caller.FetchCharacter().InJail || caller.FetchAccount().InJail)
            {
                caller.SendPermissionError();
                return;
            }

            IPlayer targetPlayer = Alt.Server.GetPlayers()
                .FirstOrDefault(x => x.GetClass().CharacterId == targetPhone.CharacterId);

            if (targetPlayer == null || !targetPhone.TurnedOn)
            {
                caller.SendPhoneMessage("Phone is off.");
                return;
            }

            if (targetPlayer.FetchCharacter().InJail || targetPlayer.FetchAccount().InJail)
            {
                caller.SendPhoneMessage("Phone is off.");
                return;
            }

            Inventory.Inventory targetInventory = targetPlayer.FetchInventory();

            if (targetInventory == null)
            {
                caller.SendPhoneMessage("Phone is off.");
                return;
            }

            List<InventoryItem> phoneItems = targetInventory.GetInventory().Where(x => x.Id.Contains("PHONE")).ToList();

            bool hasPhone = false;

            foreach (InventoryItem phoneItem in phoneItems)
            {
                if (phoneItem.ItemValue == targetPhone.PhoneNumber)
                {
                    hasPhone = true;
                }
            }

            if (!hasPhone)
            {
                caller.SendPhoneMessage("Phone is off.");
                return;
            }
            
            targetPlayer.GetData("PHONERINGING", out bool phoneRinging);
            targetPlayer.GetData("ONPHONEWITH", out int onPhoneWith);

            if (phoneRinging || onPhoneWith != 0)
            {
                caller.SendPhoneMessage($"The phone is busy!");
                return;
            }

            caller.SetData("ISCALLINGSOMEONE", true);
            caller.SetData("ISCALLINGCHARACTER", targetPlayer.GetClass().CharacterId);

            targetPlayer.SetData("PHONERINGING", true);

            caller.Emit("phone:startAudioPhoneCall", 0);

            targetPlayer.Emit("phone:startAudioPhoneCall", 1);

            List<PhoneContact> targetContacts =
                JsonConvert.DeserializeObject<List<PhoneContact>>(targetPhone.ContactList);

            var contact = targetContacts.FirstOrDefault(x => x.PhoneNumber == callerPhone.PhoneNumber);
            
            if (contact != null)
            {
                targetPlayer.SendPhoneMessage($"Incoming call from {contact.Name}. On Number: {targetPhone.PhoneNumber}.");
            }
            else
            {
                targetPlayer.SendPhoneMessage($"Incoming Call from {callerPhone.PhoneNumber}. On Number: {targetPhone.PhoneNumber}.");
            }


            Timer messageTimer = new Timer(5);
            messageTimer.Start();

            int count = 999;

            messageTimer.AutoReset = true;
            messageTimer.Elapsed += (sender, args) =>
            {

                messageTimer.Stop();

                targetPlayer.GetData("PHONEANSWERED", out int answered);

                if (answered == 1)
                {
                    // Pickup
                    messageTimer.Stop();
                    targetPlayer.SendEmoteMessage("answers their phone.");
                    PickupCall(caller, callerPhone, targetPlayer, targetPhone);
                    caller.Emit("phone:stopPhoneRinging");
                    targetPlayer.Emit("phone:stopPhoneRinging");
                    return;
                }

                if (answered == 2)
                {
                    // Hangup whilst ringing
                    messageTimer.Stop();
                    caller.SendPhoneMessage("They've hung up.");
                    targetPlayer.SendEmoteMessage("pockets their phone.");
                    targetPlayer.SetData("PHONERINGING", false);
                    targetPlayer.SetData("ONPHONEWITH", 0);
                    targetPlayer.SetData("PHONEANSWERED", 0);
                    caller.Emit("phone:stopPhoneRinging");
                    targetPlayer.Emit("phone:stopPhoneRinging");
                    return;
                }

                if (answered == 3)
                {
                    // Inital Caller has hung up

                }

                if (count == 0)
                {
                    caller.SendPhoneMessage("Ring Ring..");
                    targetPlayer.SendEmoteMessage("phone rings.");
                }

                if (count == 1000)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }

                messageTimer.Start();
            };
        }

        public static void PickupCall(IPlayer caller, Phones callerPhone, IPlayer targetPlayer, Phones targetPhone)
        {
            caller.SendPhoneMessage($"They have picked up.");
            caller.SetData("ONPHONEWITH", targetPlayer.GetPlayerId());
            targetPlayer.SetData("ONPHONEWITH", caller.GetPlayerId());

            caller.SetData("PHONERINGING", false);
            targetPlayer.SetData("PHONERINGING", false);

            using Context context = new Context();

            Phones callerPhoneDb = context.Phones.Find(callerPhone.Id);

            PhoneCall callerNewCall = new PhoneCall
            {
                PhoneNumber = targetPhone.PhoneNumber,
                CallType = PhoneCallState.Dialed,
                Time = DateTime.Now
            };

            List<PhoneCall> callerPhoneCalls =
                JsonConvert.DeserializeObject<List<PhoneCall>>(callerPhoneDb.CallHistory);

            callerPhoneCalls.Add(callerNewCall);

            callerPhoneDb.CallHistory = JsonConvert.SerializeObject(callerPhoneCalls);

            Phones targetPhoneDb = context.Phones.Find(targetPhone.Id);

            List<PhoneCall> targetPhoneCalls =
                JsonConvert.DeserializeObject<List<PhoneCall>>(targetPhoneDb.CallHistory);

            PhoneCall targetCall = new PhoneCall
            {
                PhoneNumber = callerPhone.PhoneNumber,
                CallType = PhoneCallState.Received,
                Time = DateTime.Now
            };

            targetPhoneCalls.Add(targetCall);

            targetPhoneDb.CallHistory = JsonConvert.SerializeObject(targetPhoneCalls);

            context.SaveChanges();

            
        }
    }
}