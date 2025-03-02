namespace Lumina
{
    using System.Xml.Serialization;

    /// <summary>
    /// Fog settings data in XML format.
    /// </summary>
    public class FogData
    {
        /// <summary>
        /// Gets or sets a value indicating whether this custom fog data is valid.
        /// </summary>
        [XmlElement("Valid")]
        public bool IsValid { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether fog effects are enabled.
        /// </summary>
        [XmlElement("Enabled")]
        public bool FogEffectEnabled { get; set; } = LuminaLogic.FogEffectEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether classic fog effects are enabled.
        /// </summary>
        [XmlElement("ClassicFog")]
        public bool ClassicFogEnabled { get; set; } = LuminaLogic.ClassicFogEnabled;

        /// <summary>
        /// Gets or sets fog intensity.
        /// </summary>
        [XmlElement("Intensity")]
        public float FogIntensity { get; set; } = LuminaLogic.FogIntensity;

        /// <summary>
        /// Gets or sets fog height
        /// </summary>
        [XmlElement("Height")]
        public float FogHeight { get; set; } = LuminaLogic.FogHeight;

        /// <summary>
        /// Gets or sets fog distance.
        /// </summary>
        [XmlElement("Distance")]
        public float FogDistance { get; set; } = LuminaLogic.FogDistance;

        /// <summary>
        /// Gets or sets fog color decay.
        /// </summary>
        [XmlElement("ColorDecay")]
        public float ColorDecay { get; set; } = LuminaLogic.ColorDecay;

        /// <summary>
        /// Gets or sets a value indicating whether edge fog effects are enabled.
        /// </summary>
        [XmlElement("EdgeFog")]
        public bool EdgeFogEnabled { get; set; } = LuminaLogic.EdgeFogEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether fog effects are enabled at night.
        /// </summary>
        [XmlElement("DisableFogAtNight")]
        public bool DisableAtNightFog { get; set; } = LuminaLogic.DisableAtNightFog;

        /// <summary>
        /// Gets or sets edge fog distance.
        /// </summary>
        [XmlElement("EdgeFogDistance")]
        public float EdgeFogDistance { get; set; } = LuminaLogic.EdgeFogDistance;

        /// <summary>
        /// Gets or sets the horizon height.
        /// </summary>
        [XmlElement("HorizonHeight")]
        public float HorizonHeight { get; set; } = LuminaLogic.HorizonHeight;

        [XmlElement("DisableInscatteringEffects")]
        public bool HazeEnabled
        {

            get => LuminaLogic.HazeEnabled;
            set => LuminaLogic.HazeEnabled = value;
        }
    }
}
