using System;
using System.IO;
using System.Linq;
using AlgernonCommons;
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
                // Define the base path and subfolders
                string subfolders = CombinePaths("Colossal Order", "Cities_Skylines", "WorkshopStagingArea");
                string workshopStagingAreaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), subfolders);
                Logger.Log("Workshop Staging Area Path: " + workshopStagingAreaPath);

                // Ensure the directory exists
                if (!Directory.Exists(workshopStagingAreaPath))
                {
                    try
                    {
                        Directory.CreateDirectory(workshopStagingAreaPath);
                        Logger.Log("Created Workshop Staging Area path: " + workshopStagingAreaPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error creating Workshop Staging Area path: " + ex.Message);
                        return;
                    }
                }

                // Get the latest directory based on creation time
                DirectoryInfo latestDirectory = new DirectoryInfo(workshopStagingAreaPath)
                    .GetDirectories()
                    .OrderByDescending(d => d.CreationTime)
                    .FirstOrDefault();

                if (latestDirectory != null)
                {
                    string contentDirectory = Path.Combine(latestDirectory.FullName, "Content");
                    Logger.Log("Content Directory: " + contentDirectory);

                    // Check if the Content directory exists and search for .light files
                    if (Directory.Exists(contentDirectory))
                    {
                        string[] lightFiles = Directory.GetFiles(contentDirectory, "*.light");

                        if (lightFiles.Length > 0)
                        {
                            // Path to the initial image
                            string resourcesDirectory = Path.Combine(AssemblyUtils.AssemblyPath, "Resources");
                            string initialImagePath = Path.Combine(resourcesDirectory, "PreviewImage.png");
                            Logger.Log("Initial image path: " + initialImagePath);

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
                            Logger.Log("No .light file found in Content directory.");
                        }
                    }
                    else
                    {
                        Logger.Log("Content directory does not exist.");
                    }
                }
                else
                {
                    Logger.Log("No directories found in Workshop Staging Area.");
                }
            }





            public static string CombinePaths(string first, params string[] others)
            {
                // Put error checking in here :)
                string path = first;
                foreach (string section in others)
                {
                    path = Path.Combine(path, section);
                }
                return path;
            }
        }
    }


