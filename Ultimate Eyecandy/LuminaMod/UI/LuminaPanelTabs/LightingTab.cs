namespace Lumina
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using Lumina.CompChecker;
    using Lumina.CompatibilityPolice;
    using UnityEngine;
    using System.ComponentModel;

    /// <summary>
    /// Lumina panel tab for setting lighting options.
    /// </summary>
    internal sealed class LightingTab : PanelTabBase
    {
        // Panel components.
        private UISlider _luminositySlider;
        private UISlider _gammaSlider;
        private UISlider _contrastSlider;
        private UISlider _hueSlider;
        private UISlider _tintSlider;
        private UISlider _sunTempSlider;
        private UISlider _sunTintSlider;
        private UISlider _skyTempSlider;
        private UISlider _skyTintSlider;
        private UISlider _moonTempSlider;
        private UISlider _moonTintSlider;
        private UISlider _moonLightSlider;
        private UISlider _twilightTintSlider;
        private UICheckBox _skyTonemappingCheck;
        private UILabel _disabledLabel;
        private UILabel _causeLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightingTab"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal LightingTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.LIGHTING_TEXT), tabIndex, out UIButton _);
            float currentY = Margin * 2f;

            if (CompatibilityHelper.IsAnyLightColorsManipulatingModsEnabled())
            {
                _disabledLabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LIGHTCOMP_TEXT));
                _causeLabel = UILabels.AddLabel(panel, Margin, currentY + _disabledLabel.height + Margin, Translations.Translate(LuminaTR.TranslationID.MOD_CAUSE_TEXT));
                _causeLabel.textScale = 0.7f;
                _disabledLabel.autoSize = true;
                _disabledLabel.width = panel.width - (2 * Margin);
                _disabledLabel.textAlignment = UIHorizontalAlignment.Center;
                currentY += HeaderHeight + _disabledLabel.height + Margin;
            }
            else
            {
                // Exposure control.
                UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.EXPOSURECONTROL_TEXT), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += HeaderHeight;
                _luminositySlider = AddExposureSlider(panel, Translations.Translate(LuminaTR.TranslationID.LUMINOSITY_TEXT), LuminaStyle.ValueIndex.Brightness, ref currentY);
                _gammaSlider = AddExposureSlider(panel, Translations.Translate(LuminaTR.TranslationID.GAMMA_TEXT), LuminaStyle.ValueIndex.Gamma, ref currentY);
                _contrastSlider = AddExposureSlider(panel, Translations.Translate(LuminaTR.TranslationID.RADIANCE_TEXT), LuminaStyle.ValueIndex.Contrast, ref currentY);

                // Lighting control.
                currentY += HeaderHeight;
                UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LIGHTING_TEXT), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += HeaderHeight;
                _hueSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.HUE_TEXT), LuminaStyle.ValueIndex.Temperature, ref currentY);
                _tintSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.TINT_TEXT), LuminaStyle.ValueIndex.Tint, ref currentY);
                _sunTempSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.SUNTEMP_TEXT), LuminaStyle.ValueIndex.SunTemp, ref currentY);
                _sunTintSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.SUNTINT_TEXT), LuminaStyle.ValueIndex.SunTint, ref currentY);
                _skyTempSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.SKYTEMP_TEXT), LuminaStyle.ValueIndex.SkyTemp, ref currentY);
                _skyTintSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.SKYTINT_TEXT), LuminaStyle.ValueIndex.SkyTint, ref currentY);
                _moonTempSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.MOONTEMP_TEXT), LuminaStyle.ValueIndex.MoonTemp, ref currentY);
                _moonTintSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.MOONTINT_TEXT), LuminaStyle.ValueIndex.MoonTint, ref currentY);
                _moonLightSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.MOONLIGHT_TEXT), LuminaStyle.ValueIndex.MoonLight, ref currentY);
                _twilightTintSlider = AddLightingSlider(panel, Translations.Translate(LuminaTR.TranslationID.TWILIGHTTINT_TEXT), LuminaStyle.ValueIndex.TwilightTint, ref currentY);

                // Reset button.
                UIButton resetButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.RESET_TEXT), 120f);
                resetButton.eventClicked += (c, p) =>
                {
                    _luminositySlider.value = 0f;
                    _gammaSlider.value = 0f;
                    _contrastSlider.value = 0f;
                    _hueSlider.value = 0f;
                    _tintSlider.value = 0f;
                    _sunTempSlider.value = 0f;
                    _sunTintSlider.value = 0f;
                    _skyTempSlider.value = 0f;
                    _skyTintSlider.value = 0f;
                    _moonTempSlider.value = 0f;
                    _moonTintSlider.value = 0f;
                    _moonLightSlider.value = 0f;
                    _twilightTintSlider.value = 0f;
                };

                // Checkboxes.
                _skyTonemappingCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.ENABLE_SKYTONE_TEXT));
                _skyTonemappingCheck.isChecked = StyleManager.EnableSkyTonemapping;
                _skyTonemappingCheck.eventCheckChanged += (c, isChecked) =>
                {
                    StyleManager.EnableSkyTonemapping = isChecked;
                    LuminaLogic.SkyTonemapping(isChecked);
                };
            }
        }



        /// <summary>
        /// Adds a Lumina exposure slider to the given UIComponent.
        /// </summary>
        /// <param name="panel">Parent component.</param>
        /// <param name="label">Slider text label.</param>
        /// <param name="index">Slider setting index.</param>
        /// <param name="currentY">Relative Y position reference.</param>
        /// <returns>New UISlider.</returns>
        private UISlider AddExposureSlider(UIComponent panel, string label, LuminaStyle.ValueIndex index, ref float currentY)
        {
            UISlider newSlider = AddSlider(panel, label, -1f, 1f, (int)index, ref currentY); ;
            newSlider.value = StyleManager.ActiveSettings[(int)index];
            newSlider.eventValueChanged += (c, value) =>
            {
                if (c.objectUserData is LuminaSliderData data)
                {
                    StyleManager.ActiveSettings[data.Index] = value;

                    LuminaLogic.CalculateTonemapping(_luminositySlider.value, _gammaSlider.value, _contrastSlider.value);
                }
            };

            return newSlider;
        }

        /// <summary>
        /// Adds a Lumina lighting slider to the given UIComponent.
        /// </summary>
        /// <param name="panel">Parent component.</param>
        /// <param name="label">Slider text label.</param>
        /// <param name="index">Slider setting index.</param>
        /// <param name="currentY">Relative Y position reference.</param>
        /// <returns>New UISlider.</returns>
        private UISlider AddLightingSlider(UIComponent panel, string label, LuminaStyle.ValueIndex index, ref float currentY)
        {
            UISlider newSlider = AddSlider(panel, label, -1f, 1f, (int)index, ref currentY);
            newSlider.value = StyleManager.ActiveSettings[(int)index];
            newSlider.eventValueChanged += (c, value) =>
            {
                if (c.objectUserData is LuminaSliderData data)
                {
                    StyleManager.ActiveSettings[data.Index] = value;

                    LuminaLogic.CalculateLighting(
                        _hueSlider.value,
                        _tintSlider.value,
                        _sunTempSlider.value,
                        _sunTintSlider.value,
                        _skyTempSlider.value,
                        _skyTintSlider.value,
                        _moonTempSlider.value,
                        _moonTintSlider.value,
                        _moonLightSlider.value,
                        _twilightTintSlider.value);
                }
            };
            return newSlider;
        }
    }
}