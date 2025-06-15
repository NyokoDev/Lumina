namespace Lumina
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using Lumina.CompatibilityPolice;
    using Lumina.Helpers;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Lumina panel tab for setting lighting options.
    /// </summary>
    internal sealed class LightingTab : PanelTabBase
    {
        // Panel components.
        public UISlider _luminositySlider;
        public UISlider _gammaSlider;
        public UISlider _contrastSlider;
        public UISlider _hueSlider;
        public UISlider _tintSlider;
        public UISlider _sunTempSlider;
        public UISlider _sunTintSlider;
        public UISlider _skyTempSlider;
        public UISlider _skyTintSlider;
        public UISlider _moonTempSlider;
        public UISlider _moonTintSlider;
        public UISlider _moonLightSlider;
        public UISlider _twilightTintSlider;
        private UICheckBox _skyTonemappingCheck;
        private UILabel _disabledLabel;
        private UILabel _causeLabel;
        private UILabel LUTLabel;
        private UIDropDown _lutdropdown;
        private UISlider SSAAConfig;
        private UILabel SSAALabel;
        private UICheckBox LowerVRAMUSAGE;
        private UIButton SSAAButton;
        private UICheckBox UnlockSliderCheckbox;
        /// <summary>
        /// Initializes a new instance of the <see cref="LightingTab"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal LightingTab(UITabstrip tabStrip, int tabIndex)
        {
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.LIGHTING_TEXT), tabIndex, out UIButton _);
            float currentY = Margin * 2f;

            if (LuminaLogic.DynResEnabled)
            {
                SSAALabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_TEXT), panel.width - (Margin * 2f), 0.8f, alignment: UIHorizontalAlignment.Center);
                currentY += 20f;

                SSAAConfig = AddDynamicSlider(panel, Translations.Translate(LuminaTR.TranslationID.DRSLIDERLABEL), 0.25f, DynamicResolutionManager.MaximumDRValue, 1, ref currentY);
                if (SSAAConfig != null)
                {
                    SSAAConfig.value = DynamicResolutionCamera.AliasingFactor;
                }
                else
                {
                    Logger.Log("SSAAConfig slider is null");
                }

                SSAAButton = UIButtons.AddButton(panel, ControlWidth - 200f, currentY, Translations.Translate(LuminaTR.TranslationID.SSAA_SLIDER_TEXT));
                if (SSAAButton != null)
                {
                    SSAAButton.horizontalAlignment = UIHorizontalAlignment.Center;
                    SSAAButton.eventClicked += (c, p) =>
                    {
                        if (SSAAConfig != null)
                        {
                            HandleButtonClick(SSAAConfig.value);
                        }
                        else
                        {
                            Logger.Log("SSAAConfig was null on button click.");
                        }
                    };
                }
                else
                {
                    Logger.Log("SSAAButton is null");
                }
                currentY += 32f;

                LowerVRAMUSAGE = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LOWERVRAMUSAGE));
                if (LowerVRAMUSAGE != null)
                {
                    LowerVRAMUSAGE.isChecked = DynamicResolutionManager.LowerVRAMUsage;
                    LowerVRAMUSAGE.eventCheckChanged += (c, isChecked) =>
                    {
                        if (isChecked != DynamicResolutionManager.LowerVRAMUsage)
                        {
                            DynamicResolutionManager.LowerVRAMUsage = isChecked;
                        }
                    };
                }
                else
                {
                    Logger.Log("LowerVRAMUSAGE checkbox is null");
                }
                currentY += 30f;

                UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LUT_TEXT), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += 30f;

                _lutdropdown = UIDropDowns.AddLabelledDropDown(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LUT_TEXT), itemTextScale: 0.7f, width: panel.width - (Margin * 2f));
                if (_lutdropdown != null)
                {
                    _lutdropdown.items = GetLUTDropdownItems();
                    _lutdropdown.selectedIndex = ColorCorrectionManager.instance.lastSelection;
                    _lutdropdown.eventSelectedIndexChanged += LuminaLogic.Instance.OnSelectedIndexChanged;
                    _lutdropdown.localeID = LocaleID.BUILTIN_COLORCORRECTION;
                }
                else
                {
                    Logger.Log("_lutdropdown is null");
                }
                currentY += 30f;

                UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.EXPOSURECONTROL_TEXT), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += HeaderHeight;

                _luminositySlider = AddExposureSlider(panel, Translations.Translate(LuminaTR.TranslationID.LUMINOSITY_TEXT), LuminaStyle.ValueIndex.Brightness, ref currentY);
                _gammaSlider = AddExposureSlider(panel, Translations.Translate(LuminaTR.TranslationID.GAMMA_TEXT), LuminaStyle.ValueIndex.Gamma, ref currentY);
                _contrastSlider = AddExposureSlider(panel, Translations.Translate(LuminaTR.TranslationID.RADIANCE_TEXT), LuminaStyle.ValueIndex.Contrast, ref currentY);

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

                UIButton resetButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.RESET_TEXT), 120f);
                currentY += 30f;

                resetButton.eventClicked += (c, p) =>
                {
                    if (_luminositySlider != null) _luminositySlider.value = 0f; else Logger.Log("_luminositySlider is null");
                    if (_gammaSlider != null) _gammaSlider.value = 0f; else Logger.Log("_gammaSlider is null");
                    if (_contrastSlider != null) _contrastSlider.value = 0f; else Logger.Log("_contrastSlider is null");
                    if (_hueSlider != null) _hueSlider.value = 0f; else Logger.Log("_hueSlider is null");
                    if (_tintSlider != null) _tintSlider.value = 0f; else Logger.Log("_tintSlider is null");
                    if (_sunTempSlider != null) _sunTempSlider.value = 0f; else Logger.Log("_sunTempSlider is null");
                    if (_sunTintSlider != null) _sunTintSlider.value = 0f; else Logger.Log("_sunTintSlider is null");
                    if (_skyTempSlider != null) _skyTempSlider.value = 0f; else Logger.Log("_skyTempSlider is null");
                    if (_skyTintSlider != null) _skyTintSlider.value = 0f; else Logger.Log("_skyTintSlider is null");
                    if (_moonTempSlider != null) _moonTempSlider.value = 0f; else Logger.Log("_moonTempSlider is null");
                    if (_moonTintSlider != null) _moonTintSlider.value = 0f; else Logger.Log("_moonTintSlider is null");
                    if (_moonLightSlider != null) _moonLightSlider.value = 0f; else Logger.Log("_moonLightSlider is null");
                    if (_twilightTintSlider != null) _twilightTintSlider.value = 0f; else Logger.Log("_twilightTintSlider is null");

                    if (StylesTab.SimSpeed != null) StylesTab.SimSpeed.value = 1f; else Logger.Log("StylesTab.SimSpeed is null");
                    if (StylesTab.sunIntensitySlider != null) StylesTab.sunIntensitySlider.value = 1f; else Logger.Log("StylesTab.sunIntensitySlider is null");
                    if (StylesTab.ExposureSlider != null) StylesTab.ExposureSlider.value = 1f; else Logger.Log("StylesTab.ExposureSlider is null");
                    if (StylesTab.SkyRayleighScattering != null) StylesTab.SkyRayleighScattering.value = 1f; else Logger.Log("StylesTab.SkyRayleighScattering is null");
                    if (StylesTab.SkyMieScattering != null) StylesTab.SkyMieScattering.value = 1f; else Logger.Log("StylesTab.SkyMieScattering is null");
                };
                currentY += 20f;

                UILabel versionlabel = UILabels.AddLabel(panel, Margin, currentY, Assembly.GetExecutingAssembly().GetName().Version.ToString(), panel.width - (Margin * 2f), 0.6f, alignment: UIHorizontalAlignment.Center);

                _skyTonemappingCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.ENABLE_SKYTONE_TEXT));
                if (_skyTonemappingCheck != null)
                {
                    _skyTonemappingCheck.isChecked = StyleManager.EnableSkyTonemapping;
                    _skyTonemappingCheck.eventCheckChanged += (c, isChecked) =>
                    {
                        StyleManager.EnableSkyTonemapping = isChecked;
                        LuminaLogic.SkyTonemapping(isChecked);
                    };
                }
                else
                {
                    Logger.Log("_skyTonemappingCheck is null");
                }
            }
        }

        private string[] GetLUTDropdownItems()
        {
            Dictionary<string, string> nameMapping = new Dictionary<string, string>
    {
        { "none", "None" },
        { "lutsunny", "Temperate" },
        { "lutnorth", "Boreal" },
        { "luttropical", "Tropical" },
        { "luteurope", "European" },
        { "lutcold", "Cold" },
        { "lutdark", "Dark" },
        { "lutfaded", "Faded" },
        { "lutneutral", "Neutral" },
        { "lutvibrant", "Vibrant" },
        { "lutwarm", "Warm" },
    };

            List<string> modifiedItems = new List<string>();
            foreach (var item in ColorCorrectionManager.instance.items)
            {
                string lowercasedName = item.ToLower();
                if (nameMapping.TryGetValue(lowercasedName, out string mapped))
                {
                    modifiedItems.Add(mapped);
                }
                else
                {
                    int dotIndex = item.LastIndexOf('.');
                    if (dotIndex >= 0 && dotIndex < item.Length - 1)
                    {
                        modifiedItems.Add(item.Substring(dotIndex + 1));
                    }
                }
            }
            return modifiedItems.ToArray();
        }

        private void HandleButtonClick(float value)
        {
            Loading.ActiveDRManager?.SetSSAAFactor(value);
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