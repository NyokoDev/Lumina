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
        private UISlider ContrastSlider; // New slider for contrast
        private UISlider BrightnessSlider; // New slider for brightness
        private UILabel lutLabel;

        internal LookUpTableTab(UITabstrip tabStrip, int tabIndex)
        {
            // Create and configure the panel
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.LUT_MOD_NAME), tabIndex, out UIButton _);
            float currentY = Margin;
            string pluginname = "LUT CREATOR";
            Debug.Log("[LUMINA] Plugins:" + pluginname);

            lutLabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.LUT_MOD_NAME), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);

            OpacitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.LUT_SLIDER_OPACITY_TEXT), 0f, 100f, 30, ref currentY); // Set max value to 100, default to 30
            HueSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.LUT_SLIDER_HUE_TEXT), -180, 180, 0, ref currentY);

            // Add contrast and brightness sliders
            ContrastSlider = AddSlider(panel, "Contrast", -100f, 100f, 0, ref currentY);
            BrightnessSlider = AddSlider(panel, "Brightness", -100f, 100f, 0, ref currentY);

            LutButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.SAVE_LUT_TEXT), 120f);
            LutButton.eventClicked += OnLutButtonClicked;
        }

        private void OnLutButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            // Get the slider values
            float hue = HueSlider.value;
            float contrast = ContrastSlider.value;
            float brightness = BrightnessSlider.value;
            float opacity = OpacitySlider.value / 100f;

            // Load the original LUT.png texture from the Resources folder
            Texture2D originalLutTexture = Resources.Load<Texture2D>("LUT");

            int width = originalLutTexture != null ? originalLutTexture.width : 4096;
            int height = originalLutTexture != null ? originalLutTexture.height : 64;

            // Create a new Texture2D to store the modified LUT
            Texture2D modifiedLutTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Define the threshold for identifying neutral colors
            float neutralThreshold = 0.1f;

            // Loop through each pixel of the LUT and apply the adjustments
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the color for each pixel based on x and y positions
                    float r, g, b;
                    if (originalLutTexture != null)
                    {
                        Color originalColor = originalLutTexture.GetPixel(x, y);
                        r = originalColor.r;
                        g = originalColor.g;
                        b = originalColor.b;
                    }
                    else
                    {
                        // If the original LUT is not found, create a new LUT with default values
                        r = (float)x / (float)width;
                        g = (float)y / (float)height;
                        b = (float)x / (float)width + (float)y / (float)height;
                    }

                    // Apply contrast and brightness adjustments to the pixel color
                    r = ApplyContrastBrightness(r, contrast, brightness);
                    g = ApplyContrastBrightness(g, contrast, brightness);
                    b = ApplyContrastBrightness(b, contrast, brightness);

                    // Convert RGB to HSV
                    Color.RGBToHSV(new Color(r, g, b), out float h, out float s, out float v);

                    // Set a low saturation value (e.g., 0.1f) to produce neutral (dark) colors
                    s = 0.1f;

                    // Adjust the hue value
                    h += hue / 360f; // Convert hue value to range 0-1 and add it to the existing hue
                    h = Mathf.Repeat(h, 1f); // Ensure hue value is between 0 and 1

                    // Convert back from HSV to RGB with the modified hue and low saturation
                    Color adjustedColor = Color.HSVToRGB(h, s, v);

                    // Check if the color is neutral (close to black) and adjust to a darker shade if necessary
                    if (adjustedColor.grayscale < neutralThreshold)
                    {
                        // Adjust to a darker shade while preserving some vibrancy
                        adjustedColor *= 0.7f;
                    }

                    // Set the alpha (transparency) to the desired opacity
                    adjustedColor.a = opacity;

                    // Set the adjusted pixel color in the new LUT texture
                    modifiedLutTexture.SetPixel(x, y, adjustedColor);
                }
            }

            // Apply the changes to the texture
            modifiedLutTexture.Apply();

            // Apply soft look effect with 5 blur iterations (you can adjust the number of iterations as needed)
            modifiedLutTexture = ApplySoftLook(modifiedLutTexture, 5);

            // Save the modified LUT texture to the "ColorCorrections" folder
            string colorCorrectionPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order/Cities_Skylines/Addons/ColorCorrections/");
            if (!Directory.Exists(colorCorrectionPath))
            {
                Directory.CreateDirectory(colorCorrectionPath);
            }

            string lutSavePath = Path.Combine(colorCorrectionPath, "LUT.png");
            File.WriteAllBytes(lutSavePath, modifiedLutTexture.EncodeToPNG());

            // Release resources for the Texture2D
            UnityEngine.Object.Destroy(modifiedLutTexture);
        }





        // Helper method to apply a soft look effect to the LUT texture
        private Texture2D ApplySoftLook(Texture2D lutTexture, int blurIterations)
        {
            RenderTexture renderTexture = new RenderTexture(lutTexture.width, lutTexture.height, 0);
            Graphics.Blit(lutTexture, renderTexture);

            for (int i = 0; i < blurIterations; i++)
            {
                RenderTexture tempTexture = RenderTexture.GetTemporary(lutTexture.width, lutTexture.height, 0);
                Graphics.Blit(renderTexture, tempTexture);
                Graphics.Blit(tempTexture, renderTexture);
                RenderTexture.ReleaseTemporary(tempTexture);
            }

            Texture2D softLutTexture = new Texture2D(lutTexture.width, lutTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            softLutTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            softLutTexture.Apply();
            RenderTexture.active = null;
            UnityEngine.Object.Destroy(renderTexture);

            return softLutTexture;
        }

        // Helper method to apply contrast and brightness adjustments to a color component (R, G, or B)
        private float ApplyContrastBrightness(float component, float contrast, float brightness)
        {
            // Apply contrast adjustment
            component = (component - 0.5f) * (contrast + 1f) + 0.5f;

            // Apply brightness adjustment
            component += brightness / 100f;

            // Clamp the value between 0 and 1
            component = Mathf.Clamp01(component);

            return component;
        }
    }
}
