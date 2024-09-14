using System;
using System.IO;
using AlgernonCommons;
using System.Linq;
using ColossalFramework.PlatformServices;
using HarmonyLib;

namespace Lumina.Patching
{
    [HarmonyPatch(typeof(Workshop), "UpdateItem", typeof(PublishedFileId), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string[]))]
    public static class WorkshopUpdateItemPatch
    {
        private static void Prefix(string contentPath, ref string[] tags)
        {
            string[] files = Directory.GetFiles(contentPath, "*.light");

            if (files.Length > 0)
            {
                tags = new[] { SteamHelper.kSteamTagMod, "Lumina Style" };
            }
        }
    }

    [HarmonyPatch(typeof(WorkshopModUploadPanel), "PrepareStagingArea")]
    public static class StagingPatch
    {
        public static void Postfix()
        {
            string folders = string.Join("\\", new[] { "Colossal Order", "Cities_Skylines", "WorkshopStagingArea" });

            string workshopStagingAreaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folders);
            Logger.Log(workshopStagingAreaPath);

            // Get the latest directory based on creation time
            DirectoryInfo latestDirectory = new DirectoryInfo(workshopStagingAreaPath)
                .GetDirectories()
                .OrderByDescending(d => d.CreationTime)
                .FirstOrDefault();

            if (latestDirectory != null)
            {
                string inPath = Path.Combine(latestDirectory.FullName, "Content");
                if (File.Exists(Path.Combine(inPath, ".style")))
                {
                    // Check if workshopStagingAreaPath is null or empty
                    if (string.IsNullOrEmpty(workshopStagingAreaPath))
                    {
                        Logger.Log("Invalid Workshop Staging Area path.");
                        return;
                    }

                    // Check if the directory exists
                    if (!Directory.Exists(workshopStagingAreaPath))
                    {
                        Logger.Log("Workshop Staging Area path not found.");

                        // Workaround: Create the directory
                        try
                        {
                            Directory.CreateDirectory(workshopStagingAreaPath);
                            Logger.Log("Workshop Staging Area path created: " + workshopStagingAreaPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log("Error creating Workshop Staging Area path: " + ex.Message);
                            return;
                        }
                    }

                    Logger.Log(workshopStagingAreaPath);


                    // Get the latest directory based on creation time
                    DirectoryInfo latestDirectoryOm = new DirectoryInfo(workshopStagingAreaPath)
                        .GetDirectories()
                        .OrderByDescending(d => d.CreationTime)
                        .FirstOrDefault();

                    if (latestDirectory == null)
                    {
                        Logger.Log("No directories found in Workshop Staging Area.");
                        return;
                    }

                    // Path to the initial image (ThemeMix.png)
                    string folders2 = string.Join("\\", new[] { "Resources", "PreviewImage.png" });
                    Logger.Log(folders2);
                    string initialImagePath = Path.Combine(AssemblyUtils.AssemblyPath, folders2);
                    Logger.Log(initialImagePath);
                    Logger.Log("Initial path = " + initialImagePath);

                    // Copy the initial image into the latest directory
                    try
                    {
                        string destinationImagePath = Path.Combine(latestDirectory.FullName, "PreviewImage.png");
                        File.Copy(initialImagePath, destinationImagePath, true);
                        Logger.Log("Initial image (PreviewImage.png) added successfully.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to copy initial image (PreviewImage.png): {ex.Message}");
                    }
                }
                else
                {
                    Logger.Log("Not a style?");
                }
            }
            else
            {
                // Handle case when no directories are found
            }


        }
    }
}