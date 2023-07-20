using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using Lumina.CompatibilityPolice;
using Lumina.CompChecker;

namespace Lumina
{
    internal sealed class ShadowTab : PanelTabBase
    {
        private UISlider _intensitySlider;
        private UISlider _biasSlider;
        private UICheckBox _shadowSmoothCheck;
        private UICheckBox _minShadOffsetCheck;
        private UICheckBox _fogCheckBox;
        private UICheckBox _edgefogCheckbox;
        private UISlider _fogIntensitySlider;
        private UILabel _modlabel;
        private UILabel _modlabel2;
        private UILabel _foglabel3;
        private UICheckBox _nightfog;
        private UISlider _colordecaySlider;
        private UILabel _Effects;
        private UISlider _sunshaftseffect;
        public SunShaftsCompositeShaderProperties sunShaftsScript;


        public FXAAController FXAAController;
        public UISlider focusPlaneSlider;

        internal ShadowTab(UITabstrip tabStrip, int tabIndex)
        {
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.VISUALISM_MOD_NAME), tabIndex, out UIButton _);
            float currentY = Margin;

            // Check if renderit mod or fog manipulating mods are enabled
            if (ModUtils.IsModEnabled("renderit") || CompatibilityHelper.IsAnyFogManipulatingModsEnabled())
            {
                _modlabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.VISUALISMCOMP_TEXT));
                _modlabel2 = UILabels.AddLabel(panel, Margin, currentY + _modlabel.height + Margin, Translations.Translate(LuminaTR.TranslationID.VISUALISM_CAUSE_TEXT));
                _modlabel2.textScale = 0.7f;
                _modlabel.autoSize = true;
                _modlabel.width = panel.width - (2 * Margin);
                _modlabel.textAlignment = UIHorizontalAlignment.Center;
                currentY += HeaderHeight + _modlabel.height + Margin;
            }
            else
            {
                // Slider 1: Intensity Slider
                _intensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWINT_TEXT), 0f, 1f, -1, ref currentY);
                _intensitySlider.value = LuminaLogic.ShadowIntensity;
                _intensitySlider.eventValueChanged += (c, value) => LuminaLogic.ShadowIntensity = value;
                currentY += SliderHeight + Margin;

                // Slider 2: Bias Slider
                _biasSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.SHADOWBIAS_TEXT), 0f, 2f, -1, ref currentY);
                _biasSlider.value = Patches.UpdateLighting.BiasMultiplier;
                _biasSlider.eventValueChanged += (c, value) => Patches.UpdateLighting.BiasMultiplier = value;
                currentY += SliderHeight + Margin;

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
                _fogCheckBox.eventCheckChanged += (c, isChecked) => { LuminaLogic.ClassicFogEnabled = isChecked; };
                currentY += CheckHeight + Margin;

                // Checkbox 4: Edge Fog Checkbox
                _edgefogCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.EDGEFOG_TEXT));
                _edgefogCheckbox.isChecked = LuminaLogic.EdgeFogEnabled;
                _edgefogCheckbox.eventCheckChanged += (c, isChecked) => { LuminaLogic.EdgeFogEnabled = isChecked; };
                currentY += CheckHeight + Margin;

                // Checkbox 5: Disable at Night fog
                _nightfog = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.NIGHTFOG_TEXT));
                _nightfog.isChecked = LuminaLogic.FogEffectEnabled;
                _nightfog.eventCheckChanged += (c, isChecked) => { LuminaLogic.FogEffectEnabled = isChecked; };
                currentY += CheckHeight + Margin;

                // Slider 3: Fog Intensity Slider
                _fogIntensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGINTENSITY_TEXT), 0f, 1f, -1, ref currentY);
                _fogIntensitySlider.value = LuminaLogic.FogIntensity;
                _fogIntensitySlider.eventValueChanged += (c, value) => { LuminaLogic.FogIntensity = value; };
                _fogIntensitySlider.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGINTENSITY_TEXT);
                currentY += SliderHeight + Margin;

             //   Slider 4 - Color Decay
              _colordecaySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGVISIBILITY_TEXT), 0.1f, 5f, -1, ref currentY);
               _colordecaySlider.value = LuminaLogic.ColorDecay;
               _colordecaySlider.eventValueChanged += (c, value) => { LuminaLogic.ColorDecay = value; };
               _colordecaySlider.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGVISIBILITY_TEXT);
               currentY += SliderHeight + Margin;

                //SunShafts Effect
                _sunshaftseffect = AddSlider(panel, "Sun Shafts", 0f, 1f, -1, ref currentY);
                _sunshaftseffect.value = sunShaftsScript.blurRadius;
                _sunshaftseffect.eventValueChanged += (c, value) => { sunShaftsScript.blurRadius = value; };
                currentY += SliderHeight + Margin;

                // Reset Button
                UIButton resetButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.RESET_TEXT), 120f);
                resetButton.eventClicked += (c, p) =>
                {
                    _intensitySlider.value = 1f;
                    _biasSlider.value = 0f;
                    _fogIntensitySlider.value = 0f;
                    _colordecaySlider.value = 1f;
                    _sunshaftseffect.value = 1f;
                    _nightfog.isChecked = false;
                    _shadowSmoothCheck.isChecked = false;
                    _minShadOffsetCheck.isChecked = false;
                    _fogCheckBox.isChecked = false;
                    _edgefogCheckbox.isChecked = false;
                };
            }
        }
    }
}
