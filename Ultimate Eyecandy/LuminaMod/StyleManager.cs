﻿namespace Lumina
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using ColossalFramework.PlatformServices;
    using static LuminaStyle;

    /// <summary>
    /// Handling of styles.
    /// </summary>
    internal static class StyleManager
    {
        private static float[] _activeSettings;
        private static List<LuminaStyle> _styleList = new List<LuminaStyle>();

        /// <summary>
        /// Gets the active settings arrray.
        /// </summary>
        internal static float[] ActiveSettings
        {
            get
            {
                if (_activeSettings == null)
                {
                    _activeSettings = new float[(int)ValueIndex.NumSettings];
                }

                return _activeSettings;
            }
        }

        /// <summary>
        /// Gets or sets the current Lumina loaded data reference.
        /// </summary>
        internal static LuminaData LoadedData { get; set; } = new LuminaData();

        /// <summary>
        /// Gets or sets a value indicating whether sky tonemapping is enabled.
        /// </summary>
        internal static bool EnableSkyTonemapping { get; set; }

        /// <summary>
        /// Gets the list of currently active styles.
        /// </summary>
        internal static List<LuminaStyle> StyleList => _styleList;

        /// <summary>
        /// Deletes the given style.
        /// </summary>
        /// <param name="style"></param>
        internal static void DeleteStyle(LuminaStyle style)
        {
            if (style != null && style.IsLocal && _styleList.Contains(style))
            {
                _styleList.Remove(style);

                // Delete directory if it exists.
                if (!style.DirectoryPath.IsNullOrWhiteSpace() && Directory.Exists(style.DirectoryPath))
                {
                    Directory.Delete(style.DirectoryPath, true);
                }
            }
        }

        /// <summary>
        /// Applies current settings to the game.
        /// </summary>
        internal static void ApplySettings()
        {
            // Ensure active settings are initialized.
            float[] activeSettings = ActiveSettings;

            LuminaLogic.CalculateLighting(
                activeSettings[(int)ValueIndex.Temperature],
                activeSettings[(int)ValueIndex.Tint],
                activeSettings[(int)ValueIndex.SunTemp],
                activeSettings[(int)ValueIndex.SunTint],
                activeSettings[(int)ValueIndex.SkyTemp],
                activeSettings[(int)ValueIndex.SkyTint],
                activeSettings[(int)ValueIndex.MoonTemp],
                activeSettings[(int)ValueIndex.MoonTint],
                activeSettings[(int)ValueIndex.MoonLight],
                activeSettings[(int)ValueIndex.TwilightTint]);

            LuminaLogic.CalculateTonemapping(activeSettings[(int)ValueIndex.Brightness], activeSettings[(int)ValueIndex.Gamma], activeSettings[(int)ValueIndex.Contrast]);
            LuminaLogic.SkyTonemapping(EnableSkyTonemapping);
        }

        /// <summary>
        /// Finds and loads available styles.
        /// </summary>
        internal static void LoadStyles()
        {
            // Clear existing list.
            _styleList.Clear();

            // Determine filepath.
            string addonsPath = Path.Combine(DataLocation.localApplicationData, "Addons");
            string localModPath = Path.Combine(addonsPath, "Mods");

            // Ensure local mod directory exists.
            if (!Directory.Exists(localModPath))
            {
                Directory.CreateDirectory(localModPath);
            }

            // Dictionary for parsed values.
            Dictionary<int, float> fileDict = new Dictionary<int, float>();

            // Local styles.
            ReadStyles(localModPath, true, fileDict);

            // Workshop styles.
            foreach (PublishedFileId fileId in PlatformService.workshop.GetSubscribedItems())
            {
                string itemPath = PlatformService.workshop.GetSubscribedItemPath(fileId);
                if (!itemPath.IsNullOrWhiteSpace() && Directory.Exists(itemPath))
                {
                    ReadStyles(itemPath, false, fileDict);
                }
            }
        }

        /// <summary>
        /// Reads Lumina styles from the given location.
        /// </summary>
        /// <param name="filePath">Location to search.</param>
        /// <param name="isLocal">A value indicating whether the location is local (permitting style updates or deletions).</param>
        /// <param name="fileDict">Dictionary to use for file contents.</param>
        private static void ReadStyles(string filePath, bool isLocal, Dictionary<int, float> fileDict)
        {
            // Iterate through each directory.
            foreach (string filename in Directory.GetFiles(filePath, "*.light", SearchOption.AllDirectories))
            {
                Logging.Message("parsing lighting file ", filename);

                fileDict.Clear();
                bool skyTonemapping = true;
                string styleName = null;

                try
                {
                    // Parse each line in file with an equals delimiter.
                    foreach (string line in File.ReadAllLines(filename).Where(s => s.Contains(" = ")))
                    {
                        string[] lineSplit = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);

                        if (lineSplit[0] == "skyTmpg")
                        {
                            skyTonemapping = bool.Parse(lineSplit[1]);
                        }
                        else if (lineSplit[0] == "name")
                        {
                            Logging.Message("reading style name ", lineSplit[1]);
                            styleName = lineSplit[1];
                        }
                        else
                        {
                            if (int.TryParse(lineSplit[0], out int index))
                            {
                                if (float.TryParse(lineSplit[1], out float value))
                                {
                                    fileDict[index] = value;
                                }
                            }
                        }
                    }

                    // If no valid style name was parsed, use the filename.
                    if (!styleName.IsNullOrWhiteSpace())
                    {
                        styleName = Path.GetFileName(filename);
                    }

                    // Add style to list if it has a valid name.
                    if (!styleName.IsNullOrWhiteSpace())
                    {
                        Logging.Message("adding style ", styleName);
                        _styleList.Add(new LuminaStyle(
                            styleName,
                            fileDict[(int)ValueIndex.Temperature],
                            fileDict[(int)ValueIndex.Tint],
                            fileDict[(int)ValueIndex.SunTemp],
                            fileDict[(int)ValueIndex.SunTint],
                            fileDict[(int)ValueIndex.SkyTemp],
                            fileDict[(int)ValueIndex.SkyTint],
                            fileDict[(int)ValueIndex.MoonTemp],
                            fileDict[(int)ValueIndex.MoonTint],
                            fileDict[(int)ValueIndex.MoonLight],
                            fileDict[(int)ValueIndex.TwilightTint],
                            fileDict[(int)ValueIndex.Brightness],
                            fileDict[(int)ValueIndex.Gamma],
                            fileDict[(int)ValueIndex.Contrast],
                            skyTonemapping,
                            true)
                        {
                            DirectoryPath = Path.GetDirectoryName(filename),
                        });
                    }
                    else
                    {
                        Logging.Error("invalid style name for ", filename);
                    }
                }
                catch (Exception e)
                {
                    // Don't let a single failure stop us.
                    Logging.LogException(e, "exception parsing style file ", filename);
                }
            }
        }
    }
}
