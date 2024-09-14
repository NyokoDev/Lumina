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
        private static string logFilePath;
        public static string logsPath;

        static Logger()
        {
            // Set the log file path to the Mods folder
            CheckForDirectory();
        }

        private static void CheckForDirectory()
        {
            string modPath = Singleton<PluginManager>.instance.FindPluginInfo(Assembly.GetAssembly(typeof(LuminaMod))).modPath;
            logsPath = Path.Combine(modPath, "Logs");

            // Check if the Logs directory exists, and create it if it doesn't
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }

            logFilePath = Path.Combine(logsPath, "Lumina.LogFile");

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

                // Also log the main Unity log.
                Debug.Log(logMessage);
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
                throw new IOException("[LUMINA] Failed to write to log file. Restart the game or reach Support Center");
            }
        }
    }
}