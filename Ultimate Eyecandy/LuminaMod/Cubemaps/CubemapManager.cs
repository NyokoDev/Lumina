namespace Lumina
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Cubemap management.
    /// Based on BloodyPenguin's original cubemap replacer.
    /// </summary>
    internal class CubemapManager
    {
        /// <summary>
        /// Vanilla cubemap code.
        /// </summary>
        internal const string Vanilla = "vanilla";

        // Configuration file name to look for in mod folders.
        private const string ModConfigName = "CubemapReplacements.xml";

        // Cubemap dictionaries.
        private readonly Dictionary<string, CubemapReplacement> DayCubemaps = new Dictionary<string, CubemapReplacement>();
        private readonly Dictionary<string, CubemapReplacement> NightCubemaps = new Dictionary<string, CubemapReplacement>();
        private readonly Dictionary<string, CubemapReplacement> OuterSpaceCubemaps = new Dictionary<string, CubemapReplacement>();

        // Display lists.
        private string[] _dayCubemapDescriptions;
        private string[] _dayCubemapCodes;

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static CubemapManager Instance { get; private set; }

        /// <summary>
        /// Gets or sets the name of the selected daytime CubeMap.
        /// </summary>
        internal static string DayCubeMap { get; set; } = Vanilla;

        /// <summary>
        /// Gets or sets the name of the selected nighttime CubeMap.
        /// </summary>
        internal static string NightCubeMap { get; set; } = Vanilla;

        /// <summary>
        /// Gets or sets the name of the selected nighttime CubeMap.
        /// </summary>
        internal static string OuterSpaceCubeMap { get; set; } = Vanilla;

        /// <summary>
        /// Gets an array of daytime cubemap descriptions, internally indexed.
        /// </summary>
        internal string[] DayCubemapDescriptions => _dayCubemapDescriptions;

        /// <summary>
        /// Gets the index for the current daytime cubemap.
        /// </summary>
        internal int DayCubmapIndex
        {
            get
            {
                for (int i = 0; i < _dayCubemapCodes.Length; ++i)
                {
                    if (_dayCubemapCodes[i] == DayCubeMap)
                    {
                        return i;
                    }
                }

                // If we got here, something went wrong; we didn't find a match.
                return -1;
            }
        }

        /// <summary>
        /// Initializes the manager and assigns the instance reference.
        /// MUST be called before use.
        /// </summary>
        internal static void Initialize()
        {
            Instance = new CubemapManager();

            // Read cubemaps from mods.
            Instance.ReadModCubemaps();
        }

        /// <summary>
        /// Sets the daytime cubemap replacement to the given index.
        /// </summary>
        /// <param name="index">Cubemap replacement index.</param>
        internal void SetDayReplacment(int index)
        {
            if (GetDayReplacement(index) is CubemapReplacement selectedReplacement)
            {
                DayCubeMap = selectedReplacement.Code;
                CubemapUpdater.Instance.SetDayCubemap(DayCubeMap);

                Logging.Message("Setting day cubemap to ", selectedReplacement.Description);
            }
        }

        /// <summary>
        /// Gets the daytime cubemap replacement corresponding to the given index.
        /// </summary>
        /// <param name="index">Cubemap replacement index.</param>
        /// <returns>Cubemap replacement (<c>null</c> if no valid replacement was found).</returns>
        internal CubemapReplacement GetDayReplacement(int index)
        {
            if (index < 0 || index >= _dayCubemapCodes.Length)
            {
                Logging.Error("invalid cubemap index ", index);
                return null;
            }

            if (DayCubemaps.TryGetValue(_dayCubemapCodes[index], out CubemapReplacement replacement))
            {
                return replacement;
            }

            // If we got here, something went wrong.
            Logging.Error("no valid cubemap found for code ", _dayCubemapCodes[index]);
            return null;
        }

        /// <summary>
        /// Gets the daytime cubemap replacement with the given cubemap code.
        /// </summary>
        /// <param name="cubemapCode">Cubemap code.</param>
        /// <returns>Daytime cubemap replacement (<c>null</c> if none).</returns>
        /// 
        internal CubemapReplacement GetDayReplacement(string cubemapCode) => GetReplacement(DayCubemaps, cubemapCode);

        /// <summary>
        /// Gets the nightime cubemap replacement with the given cubemap code.
        /// </summary>
        /// <param name="cubemapCode">Cubemap code.</param>
        /// <returns>Daytime cubemap replacement (<c>null</c> if none).</returns>
        internal CubemapReplacement GetNightReplacement(string cubemapCode) => GetReplacement(NightCubemaps, cubemapCode);

        /// <summary>
        /// Gets the outer space cubemap replacement with the given cubemap code.
        /// </summary>
        /// <param name="cubemapCode">Cubemap code.</param>
        /// <returns>Daytime cubemap replacement (<c>null</c> if none).</returns>
        internal CubemapReplacement GetOuterSpaceReplacement(string cubemapCode) => GetReplacement(NightCubemaps, cubemapCode);

        /// <summary>
        /// Reads cubemaps from installed and enabled mods.
        /// </summary>
        private void ReadModCubemaps()
        {
            // Iterate through each loaded pligin.
            foreach (PluginManager.PluginInfo pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo().Where(pluginInfo => pluginInfo.isEnabled))
            {
                try
                {
                    // Attempt to deserialize a config for this plugin.
                    CubemapReplacementsConfig config = CubemapReplacementsConfig.Deserialize(Path.Combine(pluginInfo.modPath, ModConfigName));
                    if (config == null)
                    {
                        // No valid config; skip this one.
                        continue;
                    }

                    // Iterate through each cubemap in file.
                    foreach (CubemapReplacement replacement in config.Replacements)
                    {
                        // Ensure valid code.
                        if (replacement.Code.IsNullOrWhiteSpace())
                        {
                            Logging.Error("Empty cubemap replacement code for ", pluginInfo.name);
                            continue;
                        }

                        // Ensure valid description.
                        if (replacement.Description.IsNullOrWhiteSpace())
                        {
                            Logging.Error("Empty cubemap description code for ", pluginInfo.name);
                            continue;
                        }

                        // Set directory.
                        replacement.Directory = pluginInfo.modPath;

                        // Set cubemap time.
                        if (replacement.IsOuterSpace)
                        {
                            if (OuterSpaceCubemaps.ContainsKey(replacement.Code))
                            {
                                Logging.Error("Duplicate outer space cubemap replacement code for ", pluginInfo.name);
                                continue;
                            }

                            // Add this cubemap.
                            OuterSpaceCubemaps.Add(replacement.Code, replacement);
                        }
                        else if (replacement.TimePeriod == "day" || !replacement.IsNight)
                        {
                            if (DayCubemaps.ContainsKey(replacement.Code))
                            {
                                Logging.Error("Duplicate day cubemap replacement code for ", pluginInfo.name);
                                continue;
                            }

                            // Add this cubemap.
                            DayCubemaps.Add(replacement.Code, replacement);
                        }
                        else if (replacement.TimePeriod == "night" || replacement.IsNight)
                        {
                            if (NightCubemaps.ContainsKey(replacement.Code))
                            {
                                Logging.Error("Duplicate night cubemap replacement code for ", pluginInfo.name);
                                continue;
                            }

                            // Add this cubemap.
                            NightCubemaps.Add(replacement.Code, replacement);
                        }
                        else
                        {
                            Logging.Error("No valid time period for cubemap replacement for ", pluginInfo.name);
                            continue;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception parsing CubemapReplacements.xml for " + pluginInfo.name);
                }
            }

            // Generate daytime names.
            GenerateNames(DayCubemaps, out _dayCubemapDescriptions, out _dayCubemapCodes);
        }

        /// <summary>
        /// Generates arrays of cubemap descriptions and names, consistently indexed.
        /// </summary>
        /// <param name="cubemaps">Dictionary of cubemaps to index.</param>
        /// <param name="descriptions">Array of cubemap descriptions, consistently indexed against codes.</param>
        /// <param name="codes">Array of cubemap codes, consistently indexed against descriptions.</param>
        private void GenerateNames(Dictionary<string, CubemapReplacement> cubemaps, out string[] descriptions, out string[] codes)
        {
            // Generate list of descriptons and codes.
            List<KeyValuePair<string, string>> names = new List<KeyValuePair<string, string>>(cubemaps.Count + 1);
            foreach (KeyValuePair<string, CubemapReplacement> kvp in cubemaps)
            {
                names.Add(new KeyValuePair<string, string>(kvp.Value.Description, kvp.Key));
            }

            // Sort alphabetically.
            names.Sort((x, y) => x.Key.CompareTo(y.Key));

            // Add vanilla entry at start.
            names.Insert(0, new KeyValuePair<string, string>("Vanilla", Vanilla));

            // Initialize string arrays.
            descriptions = new string[cubemaps.Count + 1];
            codes = new string[cubemaps.Count + 1];

            // Copy list to arrays.
            int index = 0;
            foreach (KeyValuePair<string, string> kvp in names)
            {
                descriptions[index] = kvp.Key;
                codes[index++] = kvp.Value;
            }
        }

        /// <summary>
        /// Gets the cubemap replacement from the specified dictionary with the given cubemap code.
        /// </summary>
        /// <param name="cubemaps">Cubemap dictionary to use.</param>
        /// <param name="cubemapCode">Cubemap code.</param>
        /// <returns>Daytime cubemap replacement (<c>null</c> if none).</returns>
        private CubemapReplacement GetReplacement(Dictionary<string, CubemapReplacement> cubemaps, string cubemapCode)
        {
            // Try to get existing replacement with the specified code.
            if (cubemaps.TryGetValue(cubemapCode, out CubemapReplacement replacement))
            {
                return replacement;
            }

            // If we got here, something went wrong; return null.
            return null;
        }
    }
}