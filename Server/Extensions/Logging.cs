using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using AltV.Net.Elements.Entities;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.Elasticsearch;
using Server.Models;

namespace Server.Extensions
{
    public class Logging
    {
        private static readonly string _characterDirectory = $"{Directory.GetCurrentDirectory()}/Logs/Characters/";
        private static readonly string _adminDirectory = $"{Directory.GetCurrentDirectory()}/Logs/Admins/";
        private static readonly string _bankDirectory = $"{Directory.GetCurrentDirectory()}/Logs/Banks/";
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
                    await Task.Run(() =>
                    {
                        Models.Character? playerCharacter = player.FetchCharacter();

                        if (playerCharacter == null) return;

                        Log.Logger = new LoggerConfiguration().WriteTo
                            .File($"{_characterDirectory}{playerCharacter.Name}.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();

                        Log.Information(logMessage);
                    });
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
                    await Task.Run(() =>
                    {
                        Models.Character? playerCharacter = Models.Character.GetCharacter(characterId);

                        if (playerCharacter == null) return;

                        Log.Logger = new LoggerConfiguration().WriteTo
                            .File($"{_characterDirectory}{playerCharacter.Name}.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();

                        Log.Information(logMessage);
                    });
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
                    await Task.Run(() =>
                    {
                        Models.Account? playerAccount = player.FetchAccount();

                        if (playerAccount == null) return;

                        Log.Logger = new LoggerConfiguration().WriteTo
                            .File($"{_adminDirectory}{playerAccount.Username}.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();

                        Log.Information(logMessage);
                    });
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

                    await Task.Run(() =>
                    {
                        Log.Logger = new LoggerConfiguration().WriteTo
                            .File($"{_bankDirectory}{bankAccount.AccountNumber}.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();

                        Log.Information(logMessage);
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static bool IsFileReady(string filename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}