using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Inventory;
using Server.Models;

namespace Server.Radio
{
    public class RadioCommands
    {
        [Command("tune", onlyOne: true, commandType: CommandType.Character, description: "Used to tune to a channel")]
        public static void RadioCommandTuneChannel(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (string.IsNullOrEmpty(args))
            {
                player.SendSyntaxMessage("/tune [1-4] [Channel]");
                return;
            }

            string[] splitString = args.Split(' ');

            if (splitString.Length != 2)
            {
                player.SendSyntaxMessage("/tune [1-4] [Channel]");
                return;
            }

            bool slotParse = int.TryParse(splitString[0], out int slot);
            bool channelParse = int.TryParse(splitString[1], out int channel);

            if (!slotParse || !channelParse)
            {
                player.SendSyntaxMessage("/tune [1-4] [Channel]");
                return;
            }

            if (slot < 1 || slot > 4)
            {
                player.SendSyntaxMessage("/tune [1-4] [Channel]");
                return;
            }

            RadioChannel? radioChannel = RadioHandler.RadioChannels.FirstOrDefault(x => x.Channel == channel);

            if (radioChannel != null)
            {
                if (radioChannel.Factions.Any())
                {
                    if (!radioChannel.Factions.Contains(player.FetchCharacter().ActiveFaction))
                    {
                        player.SendPermissionError();
                        return;
                    }
                }
            }

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            List<InventoryItem> radioItems = playerInventory.GetInventoryItems("ITEM_RADIO");

            if (radioItems.Count > 1)
            {
                player.SendErrorNotification("You must only have one radio on you!");
                return;
            }

            InventoryItem? radio = radioItems[0];

            if (radio == null)
            {
                player.SendErrorNotification("Unable to fetch your radio.");
                return;
            }

            if (string.IsNullOrEmpty(radio.ItemValue))
            {
                List<RadioChannelItem> radioChannelItems = new List<RadioChannelItem>
                {
                    new RadioChannelItem(channel, slot)
                };

                RadioItem radioItem = new RadioItem(radioChannelItems);

                InventoryItem newRadioGameItem = new InventoryItem(radio.Id, radio.CustomName, JsonConvert.SerializeObject(radioItem));

                if (!playerInventory.AddItem(newRadioGameItem))
                {
                    player.SendErrorNotification("Unable to update the radio.");
                    return;
                }

                if (!playerInventory.RemoveItem(radio))
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                player.SendInfoNotification($"You've set channel {channel} to slot {slot} on your radio.");
            }
            else
            {
                RadioItem radioItem = JsonConvert.DeserializeObject<RadioItem>(radio.ItemValue);

                RadioChannelItem? channelItem = radioItem.RadioChannels.FirstOrDefault(x => x.Slot == slot);

                if (channelItem == null)
                {
                    radioItem.RadioChannels.Add(new RadioChannelItem(channel, slot));
                }
                else
                {
                    radioItem.RadioChannels.Remove(channelItem);
                    player.SendInfoNotification("Radio Slot already used. Overwriting it.");
                    radioItem.RadioChannels.Add(new RadioChannelItem(channel, slot));
                }

                InventoryItem newRadioGameItem = new InventoryItem(radio.Id, radio.CustomName, JsonConvert.SerializeObject(radioItem));

                if (!playerInventory.AddItem(newRadioGameItem))
                {
                    player.SendErrorNotification("Unable to update the radio.");
                    return;
                }

                if (!playerInventory.RemoveItem(radio))
                {
                    player.SendErrorNotification("An error occurred.");
                    return;
                }

                player.SendInfoNotification($"You've set channel {channel} to slot {slot} on your radio.");
            }
        }

        [Command("leave", onlyOne: true, commandType: CommandType.Character, description: "Used to leave a radio slot")]
        public static void RadioCommandLeave(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (string.IsNullOrEmpty(args))
            {
                player.SendSyntaxMessage("/leave [1-4]");
                return;
            }

            string[] splitString = args.Split(' ');

            if (splitString.Length != 1)
            {
                player.SendSyntaxMessage("/leave [1-4]");
                return;
            }

            bool slotParse = int.TryParse(splitString[0], out int slot);

            if (!slotParse)
            {
                player.SendSyntaxMessage("/leave [1-4]");
                return;
            }

            if (slot < 1 || slot > 4)
            {
                player.SendSyntaxMessage("/tune [1-4]");
                return;
            }

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            List<InventoryItem> radioItems = playerInventory.GetInventoryItems("ITEM_RADIO");

            if (radioItems.Count > 1)
            {
                player.SendErrorNotification("You must only have one radio on you!");
                return;
            }

            InventoryItem? radio = radioItems[0];

            if (radio == null)
            {
                player.SendErrorNotification("Unable to fetch your radio.");
                return;
            }

            RadioItem? radioItem = JsonConvert.DeserializeObject<RadioItem?>(radio.ItemValue);

            if (radioItem == null)
            {
                player.SendErrorNotification("You've not set any channels.");
                return;
            }

            RadioChannelItem? radioChannelItem = radioItem.RadioChannels.FirstOrDefault(x => x.Slot == slot);

            if (radioChannelItem == null)
            {
                player.SendErrorNotification("You've not set this slot");
                return;
            }

            radioItem.RadioChannels.Remove(radioChannelItem);

            InventoryItem newRadioGameItem = new InventoryItem(radio.Id, radio.CustomName, JsonConvert.SerializeObject(radioItem));

            if (!playerInventory.AddItem(newRadioGameItem))
            {
                player.SendErrorNotification("Unable to update the radio.");
                return;
            }

            if (!playerInventory.RemoveItem(radio))
            {
                player.SendErrorNotification("An error occurred.");
                return;
            }

            player.SendInfoMessage($"You've left radio slot {slot}.");
        }

        [Command("r", onlyOne: true, commandType: CommandType.Character,
            description: "Used to send a message to others")]
        public static void RadioCommandRadio(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (string.IsNullOrEmpty(args))
            {
                player.SendSyntaxMessage("/r [Slot] [Message]");
                return;
            }

            string[] stringSplit = args.Split(' ');

            if (stringSplit.Length < 2)
            {
                player.SendSyntaxMessage("/r [Slot] [Message]");
                return;
            }

            bool slotParse = int.TryParse(stringSplit[0], out int slot);

            if (!slotParse)
            {
                player.SendSyntaxMessage("/r [Slot] [Message]");
                return;
            }

            if (slot < 1 || slot > 4)
            {
                player.SendSyntaxMessage("/r [1-4] [Message]");
                return;
            }

            string message = string.Join(" ", stringSplit.Skip(1));

            Inventory.Inventory? playerInventory = player.FetchInventory();

            if (playerInventory == null)
            {
                player.SendErrorNotification("Unable to fetch your inventory.");
                return;
            }

            List<InventoryItem> radioItems = playerInventory.GetInventoryItems("ITEM_RADIO");

            if (radioItems.Count > 1)
            {
                player.SendErrorNotification("You must only have one radio on you!");
                return;
            }

            InventoryItem? radio = radioItems[0];

            if (radio == null)
            {
                player.SendErrorNotification("Unable to fetch your radio.");
                return;
            }

            if (string.IsNullOrEmpty(radio.ItemValue))
            {
                player.SendErrorNotification("You've not tuned any channels!");
                return;
            }

            RadioItem radioItem = JsonConvert.DeserializeObject<RadioItem>(radio.ItemValue);

            RadioChannelItem? channelItem = radioItem.RadioChannels.FirstOrDefault(x => x.Slot == slot);

            if (channelItem == null)
            {
                player.SendErrorNotification("Unable to find this slot!");
                return;
            }

            RadioChannel? radioChannel =
                RadioHandler.RadioChannels.FirstOrDefault(x => x.Channel == channelItem.Channel);

            if (radioChannel != null)
            {
                if (radioChannel.Factions.Any())
                {
                    if (!radioChannel.Factions.Contains(player.FetchCharacter().ActiveFaction))
                    {
                        player.SendPermissionError();
                        return;
                    }
                    if (radioChannel.DutyCheck && !player.FetchCharacter().FactionDuty)
                    {
                        player.SendPermissionError();
                        return;
                    }
                }
            }

            SendRadioMessageToChannelFromPlayer(player, channelItem.Channel, message);

            ChatHandler.SendMessageToNearbyPlayers(player, message, MessageType.Talk, excludePlayer: true);
        }

        public static void SendRadioMessageToChannelFromPlayer(IPlayer player, int channel, string message)
        {
            string playerName = player.GetClass().Name;

            foreach (IPlayer target in Alt.GetAllPlayers())
            {
                lock (target)
                {
                    if (!target.IsSpawned()) continue;

                    Models.Character? targetCharacter = target.FetchCharacter();

                    if (targetCharacter == null) continue;

                    Inventory.Inventory? targetInventory = target.FetchInventory();

                    if (targetInventory == null) continue;

                    bool messageSent = false;

                    List<InventoryItem> radioItems = targetInventory.GetInventoryItems("ITEM_RADIO");

                    if (!radioItems.Any()) continue;

                    RadioChannel? radioChannel = RadioHandler.RadioChannels.FirstOrDefault(x => x.Channel == channel);

                    if (radioChannel != null)
                    {
                        if (radioChannel.DutyCheck && !targetCharacter.FactionDuty) continue;

                        if (radioChannel.Factions.Any())
                        {
                            if (!radioChannel.Factions.Contains(targetCharacter.ActiveFaction)) continue;
                        }
                    }

                    foreach (InventoryItem item in radioItems)
                    {
                        lock (item)
                        {
                            if (messageSent) break;

                            if (string.IsNullOrEmpty(item.ItemValue)) continue;

                            RadioItem? radioItem = JsonConvert.DeserializeObject<RadioItem?>(item.ItemValue);

                            if (radioItem == null) continue;

                            if (!radioItem.RadioChannels.Any(x => x.Channel == channel)) continue;

                            target.SendRadioMessage($"{playerName} says: {message}");

                            messageSent = true;
                        }
                    }
                }
            }
        }
    }
}