using UnityEngine;
using ColossalFramework.UI;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;

namespace Lumina
{
    internal sealed class SSAAPlusTab : PanelTabBase
    {
        private UISlider SSAASlider;
        private UIButton SSAAApplyButton;
        private UILabel ssaaLabel;

        internal SSAAPlusTab(UITabstrip tabStrip, int tabIndex)
        {
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.SSAA_MOD_NAME), tabIndex, out UIButton _);
            float currentY = Margin;
            string pluginname = "SSAA+";
            Debug.Log("[LUMINA] Plugins:" + pluginname);

            // Label for SSAA
            ssaaLabel = UILabels.AddLabel(panel, Margin, currentY, "SSAA Level", panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
            currentY += ssaaLabel.height + Margin;

            // SSAA Slider
            SSAASlider = AddSlider(panel, "SSAA Level", 1f, 4f, 0, ref currentY);

            // Apply Button
            SSAAApplyButton = UIButtons.AddButton(panel, Margin, currentY, "Apply"); // Customize the button label
            SSAAApplyButton.eventClicked += OnApplyButtonClicked;
           
        }

        private void OnApplyButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            float ssaaValue = SSAASlider.value;
            ApplySSAASettings(ssaaValue);
        }

        private void ApplySSAASettings(float ssaaValue)
        {
            int ssaaLevel = Mathf.RoundToInt(ssaaValue);

            // Adjust your SSAA settings based on the value.
            int newWidth = Screen.currentResolution.width * ssaaLevel;
            int newHeight = Screen.currentResolution.height * ssaaLevel;
            Screen.SetResolution(newWidth, newHeight, true);

            Camera.main.allowMSAA = false; // Disable MSAA when using SSAA.
        }
    }
}
