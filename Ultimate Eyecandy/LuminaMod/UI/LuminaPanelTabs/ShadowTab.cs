namespace Lumina
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Lumina panel tab for setting shadow options.
    /// </summary>
    internal sealed class ShadowTab : PanelTabBase
    {
        // Panel components.
        private UISlider _intensitySlider;
        private UISlider _biasSlider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowTab"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ShadowTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.VISUALISM_MOD_NAME), tabIndex, out UIButton _);

            float currentY = Margin;

            // Sliders.
            _intensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWINT_TEXT), ref currentY);
            _intensitySlider.value = LuminaLogic.ShadowIntensity;
            _intensitySlider.eventValueChanged += (c, value) => LuminaLogic.ShadowIntensity = value;

            _biasSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWBIAS_TEXT), ref currentY);
            _biasSlider.value = LuminaLogic.ShadowBias;
            _biasSlider.eventValueChanged += (c, value) => LuminaLogic.ShadowBias = value;
        }

        /// <summary>
        /// Adds a Lumina slider to the given UIComponent.
        /// </summary>
        /// <param name="panel">Parent component.</param>
        /// <param name="label">Slider text label.</param>
        /// <param name="currentY">Relative Y position reference.</param>
        /// <returns>New UISlider.</returns>
        private UISlider AddSlider(UIComponent panel, string label, ref float currentY) =>  AddSlider(panel, label, 3f, -1, ref currentY);
    }
}