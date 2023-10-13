using UnityEngine;
using ColossalFramework.UI;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace Lumina
{
    [XmlRoot("Settings")]
    public class Settings
    {
        public float SliderValue;
        public float SunIntensity;
        public float Exposure; // Add Exposure property
        public float RayleighScattering; // Add Rayleigh Scattering property
        public float MieScattering; // Add Mie Scattering property
        public float SimulationSpeed; // Game Speed
    }


    internal sealed class MainAdvancedTab : AdvancedPanelTabBase
    {
        private UISlider SSAASlider;
        private UISlider sunIntensitySlider;
        private UISlider ExposureSlider;
        private UISlider SkyRayleighScattering;
        private UISlider SkyMieScattering;
        private UISlider SimSpeed;
        private UIButton SSAAApplyButton;
        private UIButton SSAAResetButton;
        private UILabel ssaaLabel;
        private int defaultScreenWidth;
        private int defaultScreenHeight;

        // Define the XML file path using Path.Combine
        private string settingsFilePath = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "LuminaAdvancedLogic.xml");

        public MainAdvancedTab(UITabstrip tabStrip, int tabIndex)
        {
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.ADVANCED), tabIndex, out UIButton _);
            float currentY = Margin;
            string pluginname = "Screen Resolution Level";
            Debug.Log("[LUMINA] Screen Resolution Level started.");

            // Label for Screen Resolution
            ssaaLabel = UILabels.AddLabel(panel, Margin, currentY, "Advanced Adjustments", panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
            currentY += ssaaLabel.height + Margin;

            // SSAA Slider
            SSAASlider = AddSlider(panel, "Screen Resolution Level", 1f, 4f, 0, ref currentY);
            currentY += 0.5f; // Add space

            // Apply Button
            SSAAApplyButton = UIButtons.AddSmallerButton(panel, Margin, currentY, "Apply");
            SSAAApplyButton.eventClicked += OnApplyButtonClicked;
            currentY += 0.5f; // Add space

            // Calculate the X-coordinate for the Reset Button
            float resetButtonX = SSAAApplyButton.relativePosition.x + SSAAApplyButton.width + Margin;

            // Reset Button
            SSAAResetButton = UIButtons.AddSmallerButton(panel, resetButtonX, currentY, "Reset");
            SSAAResetButton.eventClicked += OnResetButtonClicked;
            currentY += 0.5f; // Add space

            // Increment the Y position if needed
            currentY += SSAAApplyButton.height + Margin; // You can adjust this based on your layout
            currentY += 0.5f; // Add space

            // Increment the Y position again
            currentY += SSAAResetButton.height + Margin;
            currentY += 0.5f; // Add space

            // Sun Intensity Slider
            sunIntensitySlider = AddSlider(panel, "Sun Intensity", 0f, 8f, 0, ref currentY);
            sunIntensitySlider.eventValueChanged += (c, value) => { AdvancedLogic.DayNightSunIntensity = value; }; // Set Sun Intensity value
            currentY += 2f; // Add space

            // Exposure Slider
            ExposureSlider = AddSlider(panel, "Exposure", 0f, 5f, 0, ref currentY);
            ExposureSlider.eventValueChanged += (c, value) => { AdvancedLogic.m_Exposure = value; }; // Set Exposure value
            currentY += 0.5f; // Add space

            // Sky Rayleigh Scattering
            SkyRayleighScattering = AddSlider(panel, "Rayleigh Scattering", 0f, 5f, 0, ref currentY);
            SkyRayleighScattering.eventValueChanged += (c, value) => { AdvancedLogic.SkyRayleighScattering = value; }; // Set Sky Rayleigh value
            currentY += 0.5f; // Add space

            // Sky Mie Scattering
            SkyMieScattering = AddSlider(panel, "Mie Scattering", 0f, 5f, 0, ref currentY);
            SkyMieScattering.eventValueChanged += (c, value) => { AdvancedLogic.SkyMieScattering = value; }; // Set Sky Mie value

            SimSpeed = AddSlider(panel, "Simulation Speed", 0f, 2f, 0, ref currentY);
            SimSpeed.eventValueChanged += (c, value) => { AdvancedLogic.CustomTimeScale = value; }; // Set Sim Speed value




            // Load saved settings from the XML file
            LoadSettings();
        }


        private void OnApplyButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            float ssaaValue = SSAASlider.value;
            ApplyScreenResolutionSettings(ssaaValue);

            // Save settings to the XML file
            SaveSettings();
        }

        private void OnResetButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            // Reset the screen resolution to its default values
            ResetScreenResolutionSettings();

            // Reset the slider value to its default
            SSAASlider.value = 1.0f;

            // Save settings to the XML file
            SaveSettings();
        }

        public void SaveSettings()
        {
            Settings settings = new Settings
            {
                SliderValue = SSAASlider.value,
                SunIntensity = sunIntensitySlider.value, // Save Sun Intensity
                Exposure = ExposureSlider.value,
                RayleighScattering = SkyRayleighScattering.value,
                MieScattering = SkyMieScattering.value,
                SimulationSpeed = SimSpeed.value,

            };

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (TextWriter writer = new StreamWriter(settingsFilePath))
            {
                serializer.Serialize(writer, settings);
            }
        }

        public void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (TextReader reader = new StreamReader(settingsFilePath))
                {
                    Settings settings = (Settings)serializer.Deserialize(reader);
                    SSAASlider.value = settings.SliderValue;
                    sunIntensitySlider.value = settings.SunIntensity;
                    ExposureSlider.value = settings.Exposure;
                    SkyRayleighScattering.value = settings.RayleighScattering;
                    SkyMieScattering.value = settings.MieScattering;
                    SimSpeed.value = settings.SimulationSpeed;
                }
            }
        }


        private void SetScreenResolution(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Screen.SetResolution(width, height, true);
                Camera.main.allowMSAA = false;
            }
            else
            {
                ShowResolutionError("Invalid Resolution", "Invalid screen resolution: width or height is zero or negative.");
            }
        }

        private void ShowResolutionError(string title, string message)
        {
            ExceptionPanel myExceptionPanel = new ExceptionPanel();
            myExceptionPanel.SetMessage(title, message, true);
        }

        private void ApplyScreenResolutionSettings(float ssaaValue)
        {
            int ssaaLevel = Mathf.RoundToInt(ssaaValue);
            int newWidth = defaultScreenWidth * ssaaLevel;
            int newHeight = defaultScreenHeight * ssaaLevel;

            SetScreenResolution(newWidth, newHeight);
        }

        private void ResetScreenResolutionSettings()
        {
            SetScreenResolution(defaultScreenWidth, defaultScreenHeight);
        }

    }
}
