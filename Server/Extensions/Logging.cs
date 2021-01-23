using System;
using System.IO;
using System.Text;
using AltV.Net.Elements.Entities;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Elasticsearch;
using Server.Models;

namespace Server.Extensions
{
    public class Logging
    {
        private static readonly string _characterDirectory = "C:/Logs/Characters/";
        private static readonly string _adminDirectory = "C:/Logs/Admins/";
        private static readonly string _bankDirectory = "C:/Logs/Banks/";
        private static bool _enableLogging = true;

        /// <summary>
        /// To be called on ResourceStart
        /// </summary>
        public static void InitLogging()
        {
            try
            {/*
#if DEBUG
                _enableLogging = false;
#endif*/
                if (_enableLogging)
                {
                    if (!Directory.Exists(_characterDirectory))
                    {
                        Directory.CreateDirectory(_characterDirectory);
                    }

                    if (!Directory.Exists(_adminDirectory))
                    {
                        Directory.CreateDirectory(_adminDirectory);
                    }

                    if (!Directory.Exists(_bankDirectory))
                    {
                        Directory.CreateDirectory(_bankDirectory);
                    }
                }

                var characterFiles = Directory.GetFiles(_characterDirectory);

                foreach (var characterFile in characterFiles)
                {
                    if (characterFile.EndsWith(".log"))
                    {
                        string[] fileName = characterFile.Split('.');

                        Console.WriteLine($"Moving {characterFile} to {fileName[0]}.txt");

                        File.Move(characterFile, $"{fileName[0]}.txt");
                    }
                }

                var adminFiles = Directory.GetFiles(_adminDirectory);

                foreach (var adminFile in adminFiles)
                {
                    if (adminFile.EndsWith(".log"))
                    {
                        string[] fileName = adminFile.Split('.');

                        Console.WriteLine($"Moving {adminFile} to {fileName[0]}.txt");

                        File.Move(adminFile, $"{fileName[0]}.txt");
                    }
                }

                var bankFiles = Directory.GetFiles(_bankDirectory);

                foreach (var bankFile in bankFiles)
                {
                    if (bankFile.EndsWith(".log"))
                    {
                        string[] fileName = bankFile.Split('.');

                        Console.WriteLine($"Moving {bankFile} to {fileName[0]}.txt");

                        File.Move(bankFile, $"{fileName[0]}.txt");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async void AddToCharacterLog(IPlayer player, string logMessage)
        {
            try
            {
                if (_enableLogging)
                {
                    Models.Character? playerCharacter = player.FetchCharacter();

                    if (playerCharacter == null) return;

                    await using var sourceStream = new FileStream($"{_characterDirectory}{playerCharacter.Name}.txt", FileMode.Append, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true);

                    byte[] encodedText = Encoding.Unicode.GetBytes(logMessage);

                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async void AddToCharacterLog(int characterId, string logMessage)
        {
            try
            {
                if (_enableLogging)
                {
                    Models.Character? playerCharacter = Models.Character.GetCharacter(characterId);

                    if (playerCharacter == null) return;

                    await using var sourceStream = new FileStream($"{_characterDirectory}{playerCharacter.Name}.txt", FileMode.Append, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true);

                    byte[] encodedText = Encoding.Unicode.GetBytes(logMessage);

                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async void AddToAdminLog(IPlayer player, string logMessage)
        {
            try
            {
                if (_enableLogging)
                {
                    Models.Account? playerAccount = player.FetchAccount();

                    if (playerAccount == null) return;

                    await using var sourceStream = new FileStream($"{_adminDirectory}{playerAccount.Username}.txt", FileMode.Append, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true);

                    byte[] encodedText = Encoding.Unicode.GetBytes(logMessage);

                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async void AddToBankLog(BankAccount? bankAccount, string logMessage)
        {
            try
            {
                if (_enableLogging)
                {
                    if (bankAccount == null) return;

                    await using var sourceStream = new FileStream($"{_bankDirectory}{bankAccount.AccountNumber}.txt", FileMode.Append, FileAccess.ReadWrite, FileShare.None, bufferSize: 4096, useAsync: true);

                    byte[] encodedText = Encoding.Unicode.GetBytes(logMessage);

                    await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}