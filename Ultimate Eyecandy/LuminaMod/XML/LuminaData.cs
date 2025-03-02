namespace Lumina
{
    using System.Xml.Serialization;
    using static LuminaStyle;

    /// <summary>
    /// Lumina lighting data in XML format.
    /// </summary>
    public class LuminaData
    {
        /// <summary>
        /// Gets or sets the last-used brightness value.
        /// </summary>
        [XmlElement("Brightness")]
        public float Brightness { get => StyleManager.ActiveSettings[(int)ValueIndex.Brightness]; set => StyleManager.ActiveSettings[(int)ValueIndex.Brightness] = value; }

        /// <summary>
        /// Gets or sets the current gamma value.
        /// </summary>
        [XmlElement("Gamma")]
        public float Gamma { get => StyleManager.ActiveSettings[(int)ValueIndex.Gamma]; set => StyleManager.ActiveSettings[(int)ValueIndex.Gamma] = value; }

        /// <summary>
        /// Gets or sets the current contrast value.
        /// </summary>
        [XmlElement("Contrast")]
        public float Contrast { get => StyleManager.ActiveSettings[(int)ValueIndex.Contrast]; set => StyleManager.ActiveSettings[(int)ValueIndex.Contrast] = value; }

        /// <summary>
        /// Gets or sets the last-used temperature value.
        /// </summary>
        [XmlElement("LightTemperature")]
        public float Temperature { get => StyleManager.ActiveSettings[(int)ValueIndex.Temperature]; set => StyleManager.ActiveSettings[(int)ValueIndex.Temperature] = value; }

        /// <summary>
        /// Gets or sets the current tint value.
        /// </summary>
        [XmlElement("LightTint")]
        public float Tint { get => StyleManager.ActiveSettings[(int)ValueIndex.Tint]; set => StyleManager.ActiveSettings[(int)ValueIndex.Tint] = value; }

        /// <summary>
        /// Gets or sets the current sun temperature value.
        /// </summary>
        [XmlElement("SunTemperature")]
        public float SunTemp { get => StyleManager.ActiveSettings[(int)ValueIndex.SunTemp]; set => StyleManager.ActiveSettings[(int)ValueIndex.SunTemp] = value; }

        /// <summary>
        /// Gets or sets the current sun tint value.
        /// </summary>
        [XmlElement("SunTint")]
        public float SunTint { get => StyleManager.ActiveSettings[(int)ValueIndex.SunTint]; set => StyleManager.ActiveSettings[(int)ValueIndex.SunTint] = value; }

        /// <summary>
        /// Gets or sets the current sky temperature value.
        /// </summary>
        [XmlElement("SkyTemperature")]
        public float SkyTemp { get => StyleManager.ActiveSettings[(int)ValueIndex.SkyTemp]; set => StyleManager.ActiveSettings[(int)ValueIndex.SkyTemp] = value; }

        /// <summary>
        /// Gets or sets the current sky tint value.
        /// </summary>
        [XmlElement("SkyTint")]
        public float SkyTint { get => StyleManager.ActiveSettings[(int)ValueIndex.SkyTint]; set => StyleManager.ActiveSettings[(int)ValueIndex.SkyTint] = value; }

        /// <summary>
        /// Gets or sets the current moon temperature value.
        /// </summary>
        [XmlElement("MoonTemperature")]
        public float MoonTemp { get => StyleManager.ActiveSettings[(int)ValueIndex.MoonTemp]; set => StyleManager.ActiveSettings[(int)ValueIndex.MoonTemp] = value; }

        /// <summary>
        /// Gets or sets the current moon tint value.
        /// </summary>
        [XmlElement("MoonTint")]
        public float MoonTint { get => StyleManager.ActiveSettings[(int)ValueIndex.MoonTint]; set => StyleManager.ActiveSettings[(int)ValueIndex.MoonTint] = value; }

        /// <summary>
        /// Gets or sets the current moon light value.
        /// </summary>
        [XmlElement("MoonLight")]
        public float MoonLight { get => StyleManager.ActiveSettings[(int)ValueIndex.MoonLight]; set => StyleManager.ActiveSettings[(int)ValueIndex.MoonLight] = value; }

        /// <summary>
        /// Gets or sets the current twilight tint value.
        /// </summary>
        [XmlElement("TwilightTint")]
        public float TwilightTint { get => StyleManager.ActiveSettings[(int)ValueIndex.TwilightTint]; set => StyleManager.ActiveSettings[(int)ValueIndex.TwilightTint] = value; }

        /// <summary>
        /// Gets or sets a value indicating whether sky tonemapping is enabled.
        /// </summary>
        [XmlElement("EnableSkyTonemapping")]
        public bool EnableSkyTonemapping { get => StyleManager.EnableSkyTonemapping; set => StyleManager.EnableSkyTonemapping = value; }

        /// <summary>
        /// Gets or sets the a value indicating whether shadow smoothing is disabled (<c>true</c>) or enabled (<c>false</c>).
        /// </summary>
        [XmlElement("DisableSmoothing")]
        public bool DisableSmoothing { get => LuminaLogic.DisableSmoothing; set => LuminaLogic.DisableSmoothing = value; }

        /// <summary>
        /// Gets or sets a value indicating whether shadow bias should be forced to a low value.
        /// </summary>
        [XmlElement("ForceLowShadowBias")]
        public bool ForceLowBias { get => Patches.UpdateLighting.ForceLowBias; set => Patches.UpdateLighting.ForceLowBias = value; }

        /// <summary>
        /// Gets or sets shadow intensity.
        /// </summary>
        [XmlElement("ShadowIntensity")]
        public float ShadowIntensity { get => LuminaLogic.ShadowIntensity; set => LuminaLogic.ShadowIntensity = value; }

        /// <summary>
        /// Gets or sets shadow bias.
        /// </summary>
        [XmlElement("ShadowBias")]
        public float ShadowBias { get => Patches.UpdateLighting.BiasMultiplier; set => Patches.UpdateLighting.BiasMultiplier = value; }
    }
}
