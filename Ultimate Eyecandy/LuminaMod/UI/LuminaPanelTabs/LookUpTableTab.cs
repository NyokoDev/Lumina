using System;
using System.IO;
using UnityEngine;
using ColossalFramework.UI;
using Color = UnityEngine.Color;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;

namespace Lumina
{
    internal sealed class LookUpTableTab : PanelTabBase
    {
        // Define UI elements here
        private UIButton LutButton;
        private UISlider OpacitySlider;
        private UISlider HueSlider;
        private UILabel lutLabel;

        internal LookUpTableTab(UITabstrip tabStrip, int tabIndex)
        {
            // Create and configure the panel
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.LUT_MOD_NAME), tabIndex, out UIButton _);
            float currentY = Margin;

            // Load the sprite (assuming you have the "UUI" sprite atlas and "normal" sprite defined)
            lutLabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LUT_MOD_NAME), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);

            // Placeholder method for AddSlider. Replace it with your actual implementation.
            OpacitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.LUT_SLIDER_OPACITY_TEXT), 0f, 100f, 30, ref currentY); // Set max value to 100, default to 30
            HueSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.LUT_SLIDER_HUE_TEXT), -180, 180, 0, ref currentY);

            LutButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.SAVE_LUT_TEXT), 120f);
            LutButton.eventClicked += OnLutButtonClicked;
        }

        private string GetSavePath()
        {
            // Retrieve the path to the local AppData directory
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Combine the local AppData path with the desired subdirectories
            string savePath = $"{localAppDataPath}/Colossal Order/Cities_Skylines/Addons/ColorCorrections";

            return savePath;
        }

        private void OnLutButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            // Get the slider values
            float opacityPercentage = OpacitySlider.value;
            float hue = HueSlider.value;

            // Convert the slider value to a decimal opacity value (0 to 0.3)
            float opacity = Mathf.Min(opacityPercentage, 30f) / 100f; // Limit the opacity to be less than 30% (0.3)

            Texture2D lutTexture = new Texture2D(4096, 64, TextureFormat.RGBA32, false);

            for (int x = 0; x < 4096; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    // Set the pixel color with the desired opacity and hue
                    Color color = new Color(x % 256, (x / 256) % 256, y % 256, 1f); // Set the alpha to 1 (fully opaque)
                    color = Color.HSVToRGB(hue / 360f, 1f, color.b); // Apply hue adjustment

                    // Multiply the alpha value by the desired opacity
                    color.a *= opacity;

                    lutTexture.SetPixel(x, y, color);
                }
            }

            // Encode the texture to a PNG byte array
            byte[] pngData = lutTexture.EncodeToPNG();

            // Release resources for the Texture2D
            UnityEngine.Object.Destroy(lutTexture);

            // Save the PNG byte array to a file
            string savePath = GetSavePath();
            string lutFileName = $"{savePath}/LUT.png";

            Directory.CreateDirectory(savePath); // Make sure the directory exists
            File.WriteAllBytes(lutFileName, pngData);
        }
    }
}
