﻿namespace Lumina
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
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.LIGHTING_TEXT), tabIndex, out UIButton _);
            float currentY = Margin * 2f;


            if (LuminaLogic.DynResEnabled)
            {

                SSAALabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.DYNAMICRESOLUTION_TEXT), panel.width - (Margin * 2f), 0.8f, alignment: UIHorizontalAlignment.Center);
                currentY += 20f;
                SSAAConfig = AddDynamicSlider(panel, Translations.Translate(LuminaTR.TranslationID.DRSLIDERLABEL), 0.25f, DynamicResolutionManager.MaximumDRValue, 1, ref currentY);
                SSAAConfig.value = DynamicResolutionCamera.AliasingFactor;

                SSAAButton = UIButtons.AddButton(panel, ControlWidth - 200f, currentY, Translations.Translate(LuminaTR.TranslationID.SSAA_SLIDER_TEXT));
                SSAAButton.horizontalAlignment = UIHorizontalAlignment.Center;
                currentY += 32f;
                SSAAButton.eventClicked += (c, p) => HandleButtonClick(SSAAConfig.value);


                LowerVRAMUSAGE = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LOWERVRAMUSAGE));
                currentY += 30f;
                LowerVRAMUSAGE.isChecked = DynamicResolutionManager.LowerVRAMUsage;
                LowerVRAMUSAGE.eventCheckChanged += (c, isChecked) =>
                {
                    if (isChecked != DynamicResolutionManager.LowerVRAMUsage)
                    {
                        DynamicResolutionManager.LowerVRAMUsage = isChecked;
                    }
                };

                UnlockSliderCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.UnlockSliderLabel));
                currentY += 25f;
                UnlockSliderCheckbox.isChecked = DynamicResolutionManager.UnlockSlider;
                UnlockSliderCheckbox.eventCheckChanged += (c, isChecked) =>
                {

                    UnlockSliderNotif notification = NotificationBase.ShowNotification<UnlockSliderNotif>();
                    notification.AddParas("Unlocking the Dynamic Resolution slider comes with a cautionary note, as it may lead to potential instability within the game. Before proceeding, we would like to bring to your attention the possibility of encountering issues related to game stability, including potential implications for your GPU performance. Could you confirm your decision to unlock the Dynamic Resolution slider?");
                    notification._yesButton.eventClicked += (sender, args) =>
                    {
                        DynamicResolutionManager.MaximumDRValue = 10f;
                        UnlockSliderCheckbox.isChecked = true;



                    };
                    notification._noButton.eventClicked += (sender, args) =>
                    {
                        UnlockSliderCheckbox.isChecked = false;
                        DynamicResolutionManager.MaximumDRValue = 4f;

                    };

                };
            }


        
            


                UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LUT_TEXT), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += 30f;


                _lutdropdown = UIDropDowns.AddLabelledDropDown(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LUT_TEXT), itemTextScale: 0.7f, width: panel.width - (Margin * 2f));
                currentY += 30f;

                // Define a dictionary to hold the mapping of lowercased names
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


                // Create a List<string> to hold the modified items
                List<string> modifiedItems = new List<string>();

                foreach (var item in ColorCorrectionManager.instance.items)
                {
                    // Check if the item name matches any lowercased name in the mapping
                    string lowercasedName = item.ToLower();
                    if (nameMapping.ContainsKey(lowercasedName))
                    {
                        // If so, add the mapped value to the modified list
                        modifiedItems.Add(nameMapping[lowercasedName]);
                    }
                    else
                    {
                        // If not, process the item name according to the dot-separated rule
                        int dotIndex = item.LastIndexOf('.');
                        if (dotIndex >= 0 && dotIndex < item.Length - 1)
                        {
                            modifiedItems.Add(item.Substring(dotIndex + 1));
                        }
                    }
                }

                // Set the modified items to the dropdown
                _lutdropdown.items = modifiedItems.ToArray(); // Convert back to array if necessary

                _lutdropdown.selectedIndex = ColorCorrectionManager.instance.lastSelection;
                _lutdropdown.eventSelectedIndexChanged += LuminaLogic.Instance.OnSelectedIndexChanged;
                _lutdropdown.localeID = LocaleID.BUILTIN_COLORCORRECTION;
            





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
                currentY += 30f;
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
                    StylesTab.SimSpeed.value = 1f;
                    StylesTab.sunIntensitySlider.value = 1f;
                    StylesTab.ExposureSlider.value = 1f;
                    StylesTab.SkyRayleighScattering.value = 1f;
                    StylesTab.SkyMieScattering.value = 1f;
                };
                currentY += 20f;

                UILabel versionlabel = UILabels.AddLabel(panel, Margin, currentY, Assembly.GetExecutingAssembly().GetName().Version.ToString(), panel.width - (Margin * 2f), 0.6f, alignment: UIHorizontalAlignment.Center);

                // Checkboxes.
                _skyTonemappingCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.ENABLE_SKYTONE_TEXT));
                currentY += 30f;
                _skyTonemappingCheck.isChecked = StyleManager.EnableSkyTonemapping;
                _skyTonemappingCheck.eventCheckChanged += (c, isChecked) =>
                {
                    StyleManager.EnableSkyTonemapping = isChecked;
                    LuminaLogic.SkyTonemapping(isChecked);
                };
            
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