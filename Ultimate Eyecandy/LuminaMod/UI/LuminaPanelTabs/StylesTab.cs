namespace Lumina
{
    using System;
    using System.Globalization;
    using System.Linq;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using Lumina.Helpers;
    using UnityEngine;
    using static RenderManager;

    /// <summary>
    /// Lumina panel tab for setting shadow options.
    /// </summary>
    internal sealed class StylesTab : PanelTabBase
    {
        // Components.
        UIList _styleList;
        UITextField _nameField;
        UIButton _saveButton;
        UIButton _deleteButton;
        UIButton _loadButton;
        public static UISlider ExposureSlider;
        public static UISlider SkyRayleighScattering;
        public static UISlider SkyMieScattering;
        public static UISlider SimSpeed;
        public static UISlider sunIntensitySlider;
        public static UISlider DaylightTimeHourSlider;
        public static UISlider RainIntensity;
        public static UILabel DaylightTimeHourSliderLabel;
        WeatherManager _weatherManager = Singleton<WeatherManager>.instance;

        // Selection.
        private LuminaStyle _selectedItem;

        /// <summary>
        /// Gets or sets the currently selected style.
        /// </summary>
        private LuminaStyle SelectedItem
        {
            get => _selectedItem;

            set
            {
                // Don't do anything if no change.
                if (_selectedItem != value)
                {
                    _selectedItem = value;

                    // Toggle button state.
                    _deleteButton.isEnabled = value != null && value.IsLocal;
                    _loadButton.isEnabled = value != null;

                    // Reset name textfield.
                    _nameField.text = value.StyleName;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StylesTab"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal StylesTab(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.STYLES_MOD_NAME), tabIndex, out UIButton _);

            float currentY = Margin;

            // Vehicle selection list.
            _styleList = UIList.AddUIList<StyleSelectionRow>(
                panel,
                0f,
                0f,
                ControlWidth,
                200f,
                20f);
            _styleList.EventSelectionChanged += (c, selectedItem) => SelectedItem = selectedItem as LuminaStyle;
            currentY += _styleList.height + Margin;

            // Load theme button.
            _loadButton = UIButtons.AddButton(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LOAD_TEXT));
            _loadButton.eventClicked += (c, p) =>
            {
                if (SelectedItem != null)
                {
                    SelectedItem.Apply();
                    RefreshDisplayList();
                }
            };
            _loadButton.Disable();

            // Delete theme button.
            _deleteButton = UIButtons.AddButton(panel, ControlWidth - 200f, currentY, Translations.Translate(LuminaTR.TranslationID.DELETE_TEXT));
            _deleteButton.eventClicked += (c, p) =>
            {
                if (SelectedItem != null)
                {
                    StyleManager.DeleteStyle(SelectedItem);
                    RefreshDisplayList();
                }
            };
            _deleteButton.Disable();
            currentY += _deleteButton.height + Margin;

            // Theme name textfield and label.
            UILabel nameLabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.STYLENAME_TEXT));
            currentY += nameLabel.height;
            _nameField = UITextFields.AddTextField(panel, Margin, currentY, ControlWidth);

            // Save theme button.
            currentY += _nameField.height + Margin;
            _saveButton = UIButtons.AddButton(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.SAVE_TEXT));
            _saveButton.eventClicked += (c, p) =>
            {
                if (!_nameField.text.IsNullOrWhiteSpace())
                {
                    // Determine if this style exists.
                    if (StyleManager.StyleList.Find(x => x.IsLocal && x.StyleName.Equals(_nameField.text)) is LuminaStyle existingStyle)
                    {
                        // Existing style - update it and save.
                        existingStyle.UpdateStyle();
                        existingStyle.Save();
                    }
                    else
                    {
                        // No existing style - add new style.
                        try
                        {
                            LuminaStyle newStyle = new LuminaStyle(_nameField.text, true);
                            newStyle.Save();
                            StyleManager.StyleList.Add(newStyle);

                            // Refresh list and current selection.
                            RefreshDisplayList();
                            _styleList.FindItem(newStyle);
                        }
                        catch (Exception e)
                        {
                            Logging.LogException(e, "exception creating new style");
                        }
                    }




                }


            };

            // Load styles and populate initial list.
            StyleManager.LoadStyles();
            RefreshDisplayList();
            currentY += 40f;

            // Modernized Miscellaneous Settings Panel — single label + slider combined
            const float margin = 15f;
            const float sliderHeight = 30f;
            const float spaceBetweenElements = 2f;

            // Load styles and populate initial list.
            StyleManager.LoadStyles();
            RefreshDisplayList();



            // Label
            UILabel WarningLabel = UILabels.AddLabel(
                panel, margin, currentY,
                "Note: The settings below affect visual quality in real-time but are not saved as part of your Style presets.",
                panel.width - margin * 2f - 16f, 0.85f,
                UIHorizontalAlignment.Center,
                new Color32(255, 230, 120, 255)
            );
            WarningLabel.textColor = Color.white; // Set text color to white for better visibility
                                                  // Create dark overlay behind modal for focus
            UIPanel overlay = panel.AddUIComponent<UIPanel>();
            overlay.width = panel.width;
            overlay.height = panel.height;
            overlay.backgroundSprite = "GenericPanel";  // basic plain panel sprite
            overlay.color = new Color32(0, 0, 0, 150);  // semi-transparent black
            overlay.relativePosition = Vector2.zero;
            overlay.zOrder = -998;

            // Small "?" button with clean modern circular background
            UIButton infoBtn = panel.AddUIComponent<UIButton>();
            infoBtn.text = "?";
            infoBtn.width = 24;
            infoBtn.height = 24;
            infoBtn.textScale = 0.7f;
            infoBtn.relativePosition = new Vector3(panel.width - margin - 18f, currentY + 2f);
            infoBtn.tooltip = "Click for more info.";
            infoBtn.normalBgSprite = "ButtonMenu"; // modern rounded button sprite
            infoBtn.hoveredBgSprite = "ButtonMenuHovered";
            infoBtn.focusedBgSprite = "ButtonMenuFocused";
            infoBtn.pressedBgSprite = "ButtonMenuPressed";
            infoBtn.textColor = new Color32(255, 255, 255, 200);
            infoBtn.hoveredTextColor = new Color32(255, 255, 255, 255);
            infoBtn.pressedTextColor = new Color32(200, 200, 200, 255);

            // Hover effect: subtle glow
            infoBtn.eventMouseEnter += (c, e) => infoBtn.textColor = new Color32(255, 255, 255, 255);
            infoBtn.eventMouseLeave += (c, e) => infoBtn.textColor = new Color32(255, 255, 255, 200);

            infoBtn.eventClicked += (c, e) =>
            {
                // Modal panel
                UIPanel modal = panel.AddUIComponent<UIPanel>();
                modal.width = 360f;
                modal.height = 140f;
                modal.backgroundSprite = "ButtonMenu";  // clean rounded rectangle
                modal.color = new Color32(30, 30, 30, 240); // dark grey, translucent
                modal.relativePosition = new Vector2((panel.width - modal.width) / 2f, (panel.height - modal.height) / 2f);
                modal.zOrder = 999;
                modal.autoLayout = true;
                modal.autoLayoutDirection = LayoutDirection.Vertical;
                modal.autoLayoutPadding = new RectOffset(12, 12, 12, 12);
                modal.autoLayoutStart = LayoutStart.TopLeft;

                // Label inside modal
                UILabel label = modal.AddUIComponent<UILabel>();
                label.text = "Changes here affect visuals in real-time but won't be saved in Style presets. Only settings from the Lighting Tab are saved in presets.";
                label.wordWrap = true;
                label.autoSize = false;
                label.width = modal.width - 24;
                label.height = 80;
                label.textScale = 0.9f;
                label.textAlignment = UIHorizontalAlignment.Center;
                label.textColor = new Color32(255, 255, 255, 255);

                // Close button
                UIButton closeBtn = modal.AddUIComponent<UIButton>();
                closeBtn.text = "Close";
                closeBtn.width = 80f;
                closeBtn.height = 28f;
                closeBtn.textScale = 0.9f;
                closeBtn.relativePosition = new Vector2((modal.width - closeBtn.width) / 2f, modal.height - 40f);
                closeBtn.normalBgSprite = "ButtonMenu";
                closeBtn.hoveredBgSprite = "ButtonMenuHovered";
                closeBtn.pressedBgSprite = "ButtonMenuPressed";
                closeBtn.textColor = new Color32(255, 255, 255, 220);
                closeBtn.hoveredTextColor = new Color32(255, 255, 255, 255);
                closeBtn.pressedTextColor = new Color32(200, 200, 200, 255);

                closeBtn.eventClicked += (btn, evt) =>
                {
                    UnityEngine.Object.Destroy(modal);
                    UnityEngine.Object.Destroy(overlay);
                };
            };

            currentY += WarningLabel.height + 4f;


            currentY += 20f;


            float spaceBetweenSliders = 2f; 
            float GlobalWidth = 450f;

            // Sun Intensity Slider
            sunIntensitySlider = AddGlobalSlider(panel, Translations.Translate(LuminaTR.TranslationID.SUNINTENSITY_TEXT), 0.01f, 8f, 0, ref currentY, GlobalWidth);
            sunIntensitySlider.value = LuminaLogic.DayNightSunIntensity;
            sunIntensitySlider.width = GlobalWidth;
            sunIntensitySlider.eventValueChanged += (_, value) => { LuminaLogic.DayNightSunIntensity = value; };
            sunIntensitySlider.thumbObject.color = new Color32(128, 128, 128, 255); // Grey color for thumb
            currentY += spaceBetweenSliders; // Move to the next row

            // Exposure Slider
            ExposureSlider = AddGlobalSlider(panel, Translations.Translate(LuminaTR.TranslationID.EXPOSURESLIDER_TEXT), 0.01f, 5f, 0, ref currentY, GlobalWidth);
            ExposureSlider.width = GlobalWidth;
            ExposureSlider.value = LuminaLogic.m_Exposure;
            ExposureSlider.eventValueChanged += (_, value) => { LuminaLogic.m_Exposure = value; };
            ExposureSlider.thumbObject.color = new Color32(128, 128, 128, 255); // Grey color for thumb
            currentY += spaceBetweenSliders; // Move to the next row

            // Sky Rayleigh Scattering Slider
            SkyRayleighScattering = AddGlobalSlider(panel, Translations.Translate(LuminaTR.TranslationID.RAYSCATTERING_TEXT), 0.01f, 5f, 0, ref currentY, GlobalWidth);
            SkyRayleighScattering.width = GlobalWidth;
            SkyRayleighScattering.value = LuminaLogic.SkyRayleighScattering;
            SkyRayleighScattering.thumbObject.color = new Color32(128, 128, 128, 255); // Grey color for thumb
            SkyRayleighScattering.eventValueChanged += (_, value) => { LuminaLogic.SkyRayleighScattering = value; };
            currentY += spaceBetweenSliders; // Move to the next row

            // Sky Mie Scattering Slider
            SkyMieScattering = AddGlobalSlider(panel, Translations.Translate(LuminaTR.TranslationID.MIESCATTERING_TEXT), 0.01f, 5f, 0, ref currentY, GlobalWidth);
            SkyMieScattering.width = GlobalWidth;
            SkyMieScattering.value = LuminaLogic.SkyMieScattering;
            SkyMieScattering.thumbObject.color = new Color32(128, 128, 128, 255); // Grey color for thumb
            SkyMieScattering.eventValueChanged += (_, value) => { LuminaLogic.SkyMieScattering = value; };
            currentY += spaceBetweenSliders;

            // Sim Speed Slider
            SimSpeed = AddGlobalSlider(panel, Translations.Translate(LuminaTR.TranslationID.SIMULATIONSPEED_TEXT), 0.01f, 2f, 0, ref currentY, GlobalWidth);
            SimSpeed.thumbObject.color = new Color32(128, 128, 128, 255); // Grey color for thumb
            SimSpeed.value = LuminaLogic.CustomTimeScale;
            SimSpeed.eventValueChanged += (_, value) =>
            {
                LuminaLogic.CustomTimeScale = value;
            };  // Set Sim Speed value
            currentY += spaceBetweenSliders;

            // Rain Intensity Slider
            RainIntensity = AddGlobalSlider(panel, Translations.Translate(LuminaTR.TranslationID.RAININTENSITY_TEXT), 0.01f, 5f, 0, ref currentY, GlobalWidth);
            RainIntensity.thumbObject.color = new Color32(128, 128, 128, 255); // Grey color for thumb
            RainIntensity.value = _weatherManager.m_currentRain;
            RainIntensity.eventValueChanged += (_, value) =>
            {
                _weatherManager.m_currentFog = value;
                _weatherManager.m_targetFog = value; // Fog as well
                _weatherManager.m_targetRain = value;
                _weatherManager.m_currentRain = value;
            };
        }




        /// <summary>
        /// Refreshes the style list display.
        /// </summary>
        private void RefreshDisplayList()
        {
            _styleList.Data = new FastList<object>
            {
                m_buffer = StyleManager.StyleList.OrderBy(x => x.StyleName).ToArray(),
                m_size = StyleManager.StyleList.Count,
            };

            // Reselect previously selected item.
            _styleList.FindItem(SelectedItem);
        }
    }
}