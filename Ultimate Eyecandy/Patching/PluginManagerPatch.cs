using System;
using System.IO;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using HarmonyLib;

namespace Lumina.Patching
{
    // Harmony patch for the PluginManager.LoadPlugins method
    [HarmonyPatch(typeof(MainMenu), "Awake")]
    public static class PluginManagerLoadPluginsPatch
    {
        // Method to run before LoadPlugins
        public static void Prefix()
        {
            // Log that the patch is running
            Logger.Log("(typeof(MainMenu), \"Awake\")] method is being patched. Running DeleteSourceFolders.");

            // Call the method to delete source folders
            DeleteSourceFolders();
        }

        // Your method to delete source folders
        private static void DeleteSourceFolders()
        {
            string addonsPath = Path.Combine(DataLocation.localApplicationData, "Addons");
            string localModPath = Path.Combine(addonsPath, "Mods");

            try
            {
                // Ensure local mod directory exists.
                if (!Directory.Exists(localModPath))
                {
                    Directory.CreateDirectory(localModPath);
                }

                // Recursively find directories containing `.light` files and delete adjacent `Source` folders.
                foreach (string directory in Directory.GetDirectories(localModPath, "*", SearchOption.AllDirectories))
                {
                    string[] lightFiles = Directory.GetFiles(directory, "*.light", SearchOption.TopDirectoryOnly);

                    if (lightFiles.Length > 0)
                    {
                        string sourceDirectory = Path.Combine(directory, "Source");

                        // Set any DLL files in the current directory (same level as `.light` files) to read-only.
                        foreach (string dllFile in Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
                        {
                            try
                            {
                                // Set the DLL attributes to read-only
                                File.SetAttributes(dllFile, FileAttributes.ReadOnly);
                                Logger.Log($"Set DLL file to ReadOnly: {dllFile}");
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // Suppress logging for UnauthorizedAccessException
                            }
                        }

                        // If the Source directory exists, delete it
                        if (Directory.Exists(sourceDirectory))
                        {
                            try
                            {
                                Directory.Delete(sourceDirectory, true);
                                Logger.Log($"Deleted directory: {sourceDirectory}");
                            }
                            catch (UnauthorizedAccessException)
                            {
                                // Suppress logging for UnauthorizedAccessException
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle and log other exceptions
                Logger.Log($"An error occurred while deleting source folders: {ex.Message}");
            }
        }
    }
}
