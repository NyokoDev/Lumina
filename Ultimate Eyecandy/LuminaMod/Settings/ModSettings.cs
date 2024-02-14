namespace Lumina
{
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using Lumina;
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


        [XmlIgnore]
        public bool ShouldSerializeSelectedLut => !ModUtils.IsAnyModsEnabled(PotentialConflicts);

        [XmlElement("SelectedLut")]
        public int SelectedLut
        {
            get => ColorCorrectionManager.instance.lastSelection;
            set
            {
                if (ShouldSerializeSelectedLut)
                {
                    ColorCorrectionManager.instance.currentSelection = value;
                }
            }
        }



        [XmlElement("DynamicResolutionState")]
        public bool DynamicResolutionState
        {
            get => LuminaLogic.DynResEnabled; // Get the value from LuminaLogic.DynRes.Enabled
            set => LuminaLogic.DynResEnabled = value; // Set the value to LuminaLogic.DynRes.Enabled
        }
    

        [XmlIgnore]
        public string[] PotentialConflicts = { "renderit", "thememixer" };

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

        [XmlElement("InscatteringColor")]
        public Color InscatteringColor
        {
            get => LuminaLogic._InScatteringColor;
            set => LuminaLogic._InScatteringColor = value;
        }

        [XmlElement("Simulation Speed")]
        public float SimulationSpeed
        {
            get => LuminaLogic.CustomTimeScale;
            set => LuminaLogic.CustomTimeScale = value;
        }

        [XmlElement("AOIntensity")]
        public float AOIntensity
        {
            get => LuminaLogic.AOIntensity;
            set
            {
                LuminaLogic.AOIntensity = value;
            }
        }




        [XmlElement("DisableInscatteringEffects")]
        public bool HazeEnabled = false;

        [XmlElement("CompatibilityHelper")]
        public bool Compatibility {
            
            get => LuminaLogic.CompatibilityDisabled; 
            set => LuminaLogic.CompatibilityDisabled = value; }


        [XmlElement("LockedSliderValue")]
        public float LockedSliderValue
        {
            get { return ShaderStructure.LockedSliderValue; }
            set { ShaderStructure.LockedSliderValue = value; }
        }

        [XmlElement("SSAAFactor")]
        public float SsaaFactor
        {
            get { return ShaderStructure.ssaaFactor; }
            set { ShaderStructure.ssaaFactor = value; }
        }

        [XmlElement("UnlockSlider")]
        public bool UnlockSlider
        {
            get { return ShaderStructure.unlockSlider; }
            set { ShaderStructure.unlockSlider = value; }
        }


        [XmlElement("SsaoState")]
        public bool SsaoState
        {
            get { return ShaderStructure.ssaoState; }
            set { ShaderStructure.ssaoState = value; }
        }

        [XmlElement("LowerVRAMUsage")]
        public bool LowerVRAMUsage
        {
            get { return ShaderStructure.lowerVRAMUsage; }
            set { ShaderStructure.lowerVRAMUsage = value; }
        }

        [XmlElement("SliderMaximumIndex")]
        public int SliderMaximumIndex
        {
            get { return ShaderStructure.sliderMaximumIndex; }
            set { ShaderStructure.sliderMaximumIndex = value; }
        }


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