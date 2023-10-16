namespace Lumina
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using UnityEngine;
    using static Lumina.MainAdvancedTab;

    /// <summary>
    /// Global mod settings.
    /// </summary>
    [XmlRoot("Lumina")]
    public sealed class ModSettings : SettingsXMLBase
    {
        // Settings file name.
        [XmlIgnore]
        internal static MainAdvancedTab mainAdvancedTabInstance;

        [XmlIgnore]
        private static readonly string SettingsFileName = "Lumina.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        // Full userdir settings file name.
        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

        // UUI hotkey.
        [XmlIgnore]
        private static readonly UnsavedInputKey UUIKey = new UnsavedInputKey(name: "Lumina hotkey", keyCode: KeyCode.L, control: false, shift: true, alt: true);

        /// <summary>
        /// Gets or sets the toggle key.
        /// </summary>
        [XmlElement("ToggleKey")]
        public Keybinding XMLToggleKey
        {
            get => UUIKey.Keybinding;

            set => UUIKey.Keybinding = value;
        }

        /// <summary>
        /// Gets or sets the current Lumina lighting settings.
        /// </summary>
        [XmlElement("LightingSettings")]
        public LuminaData LightingSettings { get => StyleManager.LoadedData; set => StyleManager.LoadedData = value; }

        /// <summary>
        /// Gets or sets the current Lumina fog settings.
        /// </summary>
        [XmlElement("FogSettings")]
        public FogData FogSettings { get => new FogData { IsValid = Loading.IsLoaded }; set => LuminaLogic.LoadedFogData = value; }

        /// <summary>
        /// Gets or sets the name of the selected daytime CubeMap.
        /// </summary>
        [XmlElement("DayCubeMap")]
        public string DayCubeMap { get => LuminaLogic.DayCubeMap; set => LuminaLogic.DayCubeMap = value; }

        /// <summary>
        /// Gets or sets the name of the selected nighttime CubeMap.
        /// </summary>
        [XmlElement("NightCubeMap")]
        public string NightCubeMap { get => LuminaLogic.NightCubeMap; set => LuminaLogic.NightCubeMap = value; }

        /// <summary>
        /// Gets the current hotkey as a UUI UnsavedInputKey.
        /// </summary>
        [XmlIgnore]
        internal static UnsavedInputKey ToggleKey => UUIKey;

        /// <summary>
        /// Loads settings from file.
        /// </summary>
        internal static void Load() => XMLFileUtils.Load<ModSettings>(SettingsFile);

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        internal static void Save() => XMLFileUtils.Save<ModSettings>(SettingsFile);

        /// <summary>
        /// Saves settings to file.
        /// </summary>
        internal static void saveXML(MainAdvancedTab mainAdvancedTabInstance)
        {
            // Create an instance of ExternalSettingsHandler and pass the MainAdvancedTab instance
            ExternalSettingsHandler handler = new ExternalSettingsHandler(mainAdvancedTabInstance);

            // Call the SaveSettings method on the instance
            handler.SaveSettings();
        }
    }
}