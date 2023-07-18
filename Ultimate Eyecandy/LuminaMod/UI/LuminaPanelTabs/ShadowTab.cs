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
        private UICheckBox _shadowSmoothCheck;
        private UICheckBox _minShadOffsetCheck;

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
            _intensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWINT_TEXT), 0f, 1f, -1, ref currentY);
            _intensitySlider.value = LuminaLogic.ShadowIntensity;
            _intensitySlider.eventValueChanged += (c, value) => LuminaLogic.ShadowIntensity = value;

            _biasSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWBIAS_TEXT), 0f, 2f, -1, ref currentY);
            _biasSlider.value = Patches.UpdateLighting.BiasMultiplier;
            _biasSlider.eventValueChanged += (c, value) => Patches.UpdateLighting.BiasMultiplier = value;

            // Shadow checks.
            currentY += CheckHeight;
            _shadowSmoothCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.DISABLE_SHADOWSMOOTH_TEXT));
            _shadowSmoothCheck.isChecked = LuminaLogic.DisableSmoothing;
            _shadowSmoothCheck.eventCheckChanged += (c, isChecked) =>
            {
                LuminaLogic.DisableSmoothing = isChecked;
            };

            currentY += CheckHeight;
            _minShadOffsetCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.FORCELOWBIAS_TEXT));
            _minShadOffsetCheck.isChecked = Patches.UpdateLighting.ForceLowBias;
            _minShadOffsetCheck.eventCheckChanged += (c, isChecked) => Patches.UpdateLighting.ForceLowBias = isChecked;
        }
    }
}