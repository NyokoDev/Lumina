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
    [XmlRoot("ToggleEdgeScrolling")]
    public sealed class ModSettings : SettingsXMLBase
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
    }
}