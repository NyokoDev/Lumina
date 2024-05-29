namespace Lumina
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using Lumina.CompatibilityPolice;
    using Lumina;
    using System.Drawing;
    using UnityEngine;
    using Color = UnityEngine.Color;

    [XmlRoot("VisualismTabSettings")]
    public class VisualismTabSettings
    {
        public string SelectedDayCubemap { get; set; }
        public string SelectedNightCubemap { get; set; }
    }

    internal sealed class EffectsTab : PanelTabBase
    {
        private UISlider _intensitySlider;
        private UISlider _biasSlider;
        private UICheckBox _shadowSmoothCheck;
        private UICheckBox _minShadOffsetCheck;
        private UICheckBox _fogCheckBox;
        private UICheckBox HazeCheckbox;
        private UICheckBox _edgefogCheckbox;
        private UISlider _fogIntensitySlider;
        private UILabel _modlabel;
        private UILabel _modlabel2;
        private UILabel _cubemaplabel;
        private UILabel _cubemaplabel2;
        private UILabel _foglabel3;
        private UICheckBox _nightfog;
        private UISlider _colordecaySlider;
        private UILabel _Effects;
        private UIDropDown _cubemapDropDown;
        private UIDropDown _cubemapDropDownNight;
        public UISlider AOSlider;
        public UISlider AOSliderRadius;

        private int offsetY;
     
        private LuminaLogic LuminaLogic;
        Loading Loading;
        private UISlider EdgeDistanceSlider;
        private UISlider HorizonHeight;
        private UISlider FogHeight;

        private UIDropDown _colorcorrectiondropdown;
     
        private UIButton SSAAApplyButton;
        private UIButton SSAAResetButton;
        private UILabel ssaaLabel;
        private int defaultScreenWidth;
        private int defaultScreenHeight;
        private UISlider FogDistanceSlider;

        private float CurrentSlider = 8f;

        /// <summary>
        /// Creates a new <see cref="EffectsTab"/> instance.
        /// </summary>
        /// <param name="tabStrip">Parent TabStrip.</param>
        /// <param name="tabIndex">Tab index.</param>
        internal EffectsTab(UITabstrip tabStrip, int tabIndex)
        {
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.VISUALISM_MOD_NAME), tabIndex, out UIButton _);

            float currentY = Margin;
            UIScrollbars.AddScrollbar(panel);
            ModSettings instance = new ModSettings();

            {

                _cubemaplabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.CUBEMAP_TEXT_CONTROLLER), panel.width - (Margin * 2f), 0.5f, alignment: UIHorizontalAlignment.Center);
                currentY += 20f;
                // Dropdown Cubemap and Daylight reflections
                _cubemapDropDown = UIDropDowns.AddLabelledDropDown(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.CUBEMAP_TEXT), itemTextScale: 0.7f, width: panel.width - (Margin * 2f));
                _cubemapDropDown.items = CubemapManager.Instance.DayCubemapDescriptions;
                _cubemapDropDown.selectedIndex = CubemapManager.Instance.DayCubmapIndex;
                _cubemapDropDown.eventSelectedIndexChanged += (c, index) => CubemapManager.Instance.SetDayReplacment(index);

                currentY += 30f;


                // Slider 1: Intensity Slider
                _intensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWINT_TEXT), 0f, 1f, -1, ref currentY);
                _intensitySlider.value = LuminaLogic.ShadowIntensity;
                _intensitySlider.eventValueChanged += (c, value) => LuminaLogic.ShadowIntensity = value;
                currentY += CurrentSlider + Margin;

                // Slider 2: Bias Slider
                _biasSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWBIAS_TEXT), 0f, 2f, -1, ref currentY);
                _biasSlider.value = Patches.UpdateLighting.BiasMultiplier;
                _biasSlider.eventValueChanged += (c, value) => Patches.UpdateLighting.BiasMultiplier = value;
                currentY += CurrentSlider + Margin;

                // Checkbox 1: Shadow Smooth Check
                _shadowSmoothCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.DISABLE_SHADOWSMOOTH_TEXT));
                _shadowSmoothCheck.isChecked = LuminaLogic.DisableSmoothing;
                _shadowSmoothCheck.eventCheckChanged += (c, isChecked) => { LuminaLogic.DisableSmoothing = isChecked; };
                currentY += CheckHeight + Margin;

                // Checkbox 2: Min Shadow Offset Check
                _minShadOffsetCheck = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.FORCELOWBIAS_TEXT));
                _minShadOffsetCheck.isChecked = Patches.UpdateLighting.ForceLowBias;
                _minShadOffsetCheck.eventCheckChanged += (c, isChecked) => { Patches.UpdateLighting.ForceLowBias = isChecked; };
                currentY += CheckHeight + Margin;

                // Label: Fog Label
                _foglabel3 = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.FOGSETTINGS_TEXT), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += CheckHeight + Margin;

                // Checkbox 3: Classic Fog Checkbox
                _fogCheckBox = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.CLASSICFOG_TEXT));
                _fogCheckBox.isChecked = LuminaLogic.ClassicFogEnabled;
                _fogCheckBox.eventCheckChanged += (c, isChecked) => LuminaLogic.ClassicFogEnabled = isChecked;
                currentY += CheckHeight + Margin;

                // Checkbox 4: Edge Fog Checkbox
                _edgefogCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.EDGEFOG_TEXT));
                _edgefogCheckbox.isChecked = LuminaLogic.EdgeFogEnabled;
                _edgefogCheckbox.eventCheckChanged += (c, isChecked) => LuminaLogic.EdgeFogEnabled = isChecked;
                currentY += CheckHeight + Margin;

                // Checkbox: Night Fog
                _nightfog = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.NIGHTFOG_TEXT));
                _nightfog.isChecked = LuminaLogic.FogEffectEnabled;
                _nightfog.eventCheckChanged += (c, isChecked) => LuminaLogic.FogEffectEnabled = isChecked;
                currentY += CheckHeight + Margin;

                // Slider 3: Fog Intensity Slider
                _fogIntensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGINTENSITY_TEXT), 0f, 0.01f, -1, ref currentY);
                _fogIntensitySlider.value = LuminaLogic.FogIntensity;
                _fogIntensitySlider.eventValueChanged += (c, value) => LuminaLogic.FogIntensity = value;
                _fogIntensitySlider.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGINTENSITY_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 4 - Color Decay
                _colordecaySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGVISIBILITY_TEXT), 0.06f, 0.4f, -1, ref currentY);
                _colordecaySlider.value = LuminaLogic.ColorDecay;
                _colordecaySlider.eventValueChanged += (c, value) => LuminaLogic.ColorDecay = value;
                _colordecaySlider.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGVISIBILITY_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 5 - Edge Distance
                EdgeDistanceSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.EDGEDISTANCE_TEXT), 0f, 2800f, -1, ref currentY);
                EdgeDistanceSlider.value = LuminaLogic.EdgeFogDistance;
                EdgeDistanceSlider.eventValueChanged += (c, value) => LuminaLogic.EdgeFogDistance = value;
                EdgeDistanceSlider.tooltip = Translations.Translate(LuminaTR.TranslationID.EDGEDISTANCE_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 6 - Horizon Height
                HorizonHeight = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.HORIZONHEIGHT_TEXT), 0f, 5000f, -1, ref currentY);
                HorizonHeight.value = LuminaLogic.HorizonHeight;
                HorizonHeight.eventValueChanged += (c, value) => LuminaLogic.HorizonHeight = value;
                HorizonHeight.tooltip = Translations.Translate(LuminaTR.TranslationID.HORIZONHEIGHT_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                FogHeight = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGHEIGHT_TEXT), 0f, 5000f, -1, ref currentY);
                FogHeight.value = LuminaLogic.FogHeight;
                FogHeight.eventValueChanged += (c, value) => LuminaLogic.FogHeight = value;
                FogHeight.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGHEIGHT_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 7 - Fog Distance

                FogDistanceSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGDISTANCE_TEXT), 0f, 20000f, -1, ref currentY);
                FogDistanceSlider.value = LuminaLogic.FogDistance;
                FogDistanceSlider.eventValueChanged += (c, value) =>
                {
                    LuminaLogic.FogDistance = value;
                    LuminaLogic.ThreeDFogDistance = value;
                    LuminaLogic.VolumeFogDistance = value;
                    LuminaLogic.InscatteringExponent = 0f;
                    LuminaLogic.InscatteringStartDistance = 0f;
                    LuminaLogic.InscatteringIntensity = 0f;
                };

               
                // Assuming you have an event handler for when the checkbox state changes
       
                HazeCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.BLUEHAZE));
                HazeCheckbox.isChecked = instance.HazeEnabled;
                HazeCheckbox.eventCheckChanged += (c, isChecked) =>
                {
                    if (isChecked)
                    {
                        BlueHaze();
                    }
                };
                currentY += 40f;


                // Reset Button
                UIButton resetButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.RESET_TEXT), 120f);
                resetButton.eventClicked += (c, p) =>
                {
                    // Show a confirmation popup
                    ConfirmNotification notification = NotificationBase.ShowNotification<ConfirmNotification>();
                    notification.AddParas("Reset all Lumina settings? Action can't be undone.");

                    notification._yesButton.eventClicked += (sender, args) =>
                    {
                        // Reset all settings
                        _intensitySlider.value = 1f;
                        _biasSlider.value = 0f;
                        _fogIntensitySlider.value = 0f;
                        _colordecaySlider.value = 1f;
                        _nightfog.isChecked = false;
                        _shadowSmoothCheck.isChecked = false;
                        _minShadOffsetCheck.isChecked = false;
                        HorizonHeight.value = 0f;
                        _fogCheckBox.isChecked = false;
                        _edgefogCheckbox.isChecked = false;
                       

                        notification.Close();
                    };
                };










                // Calculate the X-coordinate for reset2Button based on resetButton's position and width
                float advbuttonX = resetButton.relativePosition.x - 120f;
            }
        }


        public bool BlueHaze()
        {
            try
            {
                LuminaLogic.InscatteringExponent = 0f;
                LuminaLogic.InscatteringStartDistance = 0f;
                LuminaLogic.InscatteringIntensity = 0f;
                LuminaLogic._InScatteringColor = new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1f); // This sets the color to a neutral gray
                ModSettings instance = new ModSettings();
                instance.HazeEnabled = true;
                return true;

            }
            catch (Exception ex)
            {
                ModSettings instance = new ModSettings();
                instance.HazeEnabled = false;
                return false;
            }
        }


        public class ConfirmNotification : ListNotification
        {
            // Don't Show Again button.
            internal UIButton _noButton;
            internal UIButton _yesButton;

            /// <summary>
            /// Gets the 'No' button (button 1) instance.
            /// </summary>
            public UIButton NoButton => _noButton;

            /// <summary>
            /// Gets the 'Yes' button (button 2) instance.
            /// </summary>
            public UIButton YesButton => _yesButton;

            /// <summary>
            /// Gets the number of buttons for this panel (for layout).
            /// </summary>
            protected override int NumButtons => 2;



            /// <summary>
            /// Adds buttons to the message box.
            /// </summary>
            public override void AddButtons()
            {

                // Add yes button.
                _yesButton = AddButton(1, NumButtons, Translations.Translate("YES"), Close);


                _noButton = AddButton(2, NumButtons, Translations.Translate("NO"), Close);

            }
        }
    }
}




