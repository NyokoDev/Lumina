namespace Lumina
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using UnityEngine;


    /// <summary>
    /// Global mod settings.
    /// </summary>
    [XmlRoot("Lumina")]
    public class ModSettings : SettingsXMLBase
    {
        // Settings file name.


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
        public string DayCubeMap { get => CubemapManager.DayCubeMap; set => CubemapManager.DayCubeMap = value; }

        /// <summary>
        /// Gets or sets the name of the selected nighttime CubeMap.
        /// </summary>
        [XmlElement("NightCubeMap")]
        public string NightCubeMap { get => CubemapManager.NightCubeMap; set => CubemapManager.NightCubeMap = value; }


        [XmlElement("SelectedLut")]
        public int SelectedLut
        {
            get => ColorCorrectionManager.instance.lastSelection;
            set => ColorCorrectionManager.instance.currentSelection = value;
        }


        [XmlElement("SunIntensityLevel")]
        public float SunIntensity
        {
            get => LuminaLogic.DayNightSunIntensity;
            set => LuminaLogic.DayNightSunIntensity = value;
        }

        [XmlElement("ExposureLevel")]
        public float Exposure
        {
            get => LuminaLogic.m_Exposure;
            set => LuminaLogic.m_Exposure = value;
        }

        [XmlElement("SkyRayleighScattering")]
        public float SkyRayleighScattering
        {
            get => LuminaLogic.SkyRayleighScattering;
            set => LuminaLogic.SkyRayleighScattering = value;
        }

        [XmlElement("SkyMieScattering")]
        public float SkyMieScattering
        {
            get => LuminaLogic.SkyMieScattering;
            set => LuminaLogic.SkyMieScattering = value;
        }

        [XmlElement("Simulation Speed")]
        public float SimulationSpeed
        {
            get => LuminaLogic.CustomTimeScale;
            set => LuminaLogic.CustomTimeScale = value;
        }

        [XmlElement("DisableInscatteringEffects")]
        public bool HazeEnabled = false;




        LuminaLogic LuminaLogicInstance = new LuminaLogic(); // Create an instance of LuminaLogic


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
    }
}