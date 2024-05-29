using ColossalFramework.Plugins;
using ColossalFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Lumina
{
    public static class Logger
    {
        private static readonly string logFilePath;

        static Logger()
        {
            // Set the log file path to the Mods folder
            string modPath = Singleton<PluginManager>.instance.FindPluginInfo(Assembly.GetAssembly(typeof(LuminaMod))).modPath;
            logFilePath = Path.Combine(modPath, "Lumina.LogFile");
        }



        public static void Log(object message)
        {
            try
            {

                string logMessage;

                if (message is Exception)
                {
                    // If the message is an exception, format it as an error message
                    Exception ex = (Exception)message;
                    logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {ex}";
                }
                else
                {
                    // If the message is a string, format it as a regular log message
                    logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                }

                // Append the log message to the log file
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle sharing violation error
                Debug.Log($"[LUMINA]: Unauthorized access occurred: {ex.Message}");
                throw; // Re-throw the exception
            }
            catch (Exception ex)
            {
                // If an error occurs while logging, print the exception message to the console
                Debug.Log($"[LUMINA]: Failed to write to log file: {ex.Message}");
                throw new IOException("[LUMINA] Failed to write to log file. Restart the game or verify the existence of the logging file.");
            }
        }

        class Program
        {
            static void Main(string[] args)
            {
                Logger.Log("This is a test log message.");
                Logger.Log("Logging another message.");

                Console.WriteLine("Log messages written to desktop log file.");
            }
        }
    }
}

