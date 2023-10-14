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

            // Sun Intensity Slider
            sunIntensitySlider = AddSlider(panel, "Sun Intensity", 0f, 8f, 0, ref currentY);
            sunIntensitySlider.eventValueChanged += (c, value) => { AdvancedLogic.DayNightSunIntensity = value; SaveSettings(); };  // Set Sun Intensity value
            currentY += 2f; // Add space

            // Exposure Slider
            ExposureSlider = AddSlider(panel, "Exposure", 0f, 5f, 0, ref currentY);
            ExposureSlider.eventValueChanged += (c, value) => { AdvancedLogic.m_Exposure = value; SaveSettings(); };  // Set Exposure value
            currentY += 0.5f; // Add space

            // Sky Rayleigh Scattering
            SkyRayleighScattering = AddSlider(panel, "Rayleigh Scattering", 0f, 5f, 0, ref currentY);
            SkyRayleighScattering.eventValueChanged += (c, value) => { AdvancedLogic.SkyRayleighScattering = value; SaveSettings(); };  // Set Sky Rayleigh value
            currentY += 0.5f; // Add space

            // Sky Mie Scattering
            SkyMieScattering = AddSlider(panel, "Mie Scattering", 0f, 5f, 0, ref currentY);
            SkyMieScattering.eventValueChanged += (c, value) => { AdvancedLogic.SkyMieScattering = value; SaveSettings(); };// Set Sky Mie value

            SimSpeed = AddSlider(panel, "Simulation Speed", 0f, 2f, 0, ref currentY);
            SimSpeed.eventValueChanged += (c, value) => { AdvancedLogic.CustomTimeScale = value; SaveSettings(); };  // Set Sim Speed value

            // Set the mainAdvancedTabInstance in the ModSettings class
            ModSettings.mainAdvancedTabInstance = this;

            // Pass the instance of MainAdvancedTab to ExternalSettingsHandler
            ExternalSettingsHandler externalSettingsHandler = new ExternalSettingsHandler(this);

            // Load saved settings
            externalSettingsHandler.LoadSettings();
        
    }




   

        public void LoadSettings()
        {
            if (!File.Exists(settingsFilePath))
            {
                if (CreateDefaultSettings())
                {
                    Debug.Log("[LUMINA] Created default settings file at path: " + settingsFilePath);
                }
                else
                {
                    Debug.LogError("[LUMINA] Failed to create default settings file at path: " + settingsFilePath);
                }
                return;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            using (TextReader reader = new StreamReader(settingsFilePath))
            {
                Settings settings = (Settings)serializer.Deserialize(reader);

                if (settings != null)
                {  
                 
                    if (sunIntensitySlider != null)
                    {
                        sunIntensitySlider.value = settings.SunIntensity;
                    }
                    else
                    {
                        Debug.LogError("[LUMINA] sunIntensitySlider is null.");
                    }

                    if (ExposureSlider != null)
                    {
                        ExposureSlider.value = settings.Exposure;
                    }
                    else
                    {
                        Debug.LogError("[LUMINA] ExposureSlider is null.");
                    }

                    if (SkyRayleighScattering != null)
                    {
                        SkyRayleighScattering.value = settings.RayleighScattering;
                    }
                    else
                    {
                        Debug.LogError("[LUMINA] SkyRayleighScattering is null.");
                    }

                    if (SkyMieScattering != null)
                    {
                        SkyMieScattering.value = settings.MieScattering;
                    }
                    else
                    {
                        Debug.LogError("[LUMINA] SkyMieScattering is null.");
                    }

                    if (SimSpeed != null)
                    {
                        SimSpeed.value = settings.SimulationSpeed;
                    }
                    else
                    {
                        Debug.LogError("[LUMINA] SimSpeed is null.");
                    }
                }
                else
                {
                    Debug.LogError("[LUMINA] Loaded settings object is null.");
                }
            }
        }

        private bool CreateDefaultSettings()
        {
            try
            {
                Settings defaultSettings = new Settings();

                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                using (TextWriter writer = new StreamWriter(settingsFilePath))
                {
                    serializer.Serialize(writer, defaultSettings);
                }

                return true; // File creation successful
            }
            catch (Exception e)
            {
                Debug.LogError("[LUMINA] Failed to create default settings file: " + e.Message);
                return false; // File creation failed
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




        public void SaveSettings()
        {
            Settings settings = new Settings
            {
                SunIntensity = sunIntensitySlider.value,
                Exposure = ExposureSlider.value,
                RayleighScattering = SkyRayleighScattering.value,
                MieScattering = SkyMieScattering.value,
                SimulationSpeed = SimSpeed.value
            };

            try
            {
                using (TextWriter writer = new StreamWriter(settingsFilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    serializer.Serialize(writer, settings);
                }
            }
            catch (IOException e)
            {
                // Handle the exception, e.g., by logging an error message or informing the user.
                Debug.LogError("[LUMINA] Error while saving settings: " + e.Message);
            }
        }


        public class ExternalSettingsHandler
        {
            private MainAdvancedTab mainTab;

            public ExternalSettingsHandler(MainAdvancedTab mainTab)
            {
                this.mainTab = mainTab;
            }



            public void LoadSettings()
            {
                mainTab.LoadSettings();
            }

            public void SaveSettings()
            {
                if (mainTab != null)
                {
                    mainTab.SaveSettings();
                    Debug.Log("[LUMINA] SaveSettings method ran successfully.");
                }
                else
                {
                    Debug.LogError("[LUMINA] mainTab is null.");
                }
            }


        }
    }
}
