using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using Lumina.CompatibilityPolice;
using Lumina.CompChecker;
using SkyboxReplacer.OptionsFramework.Attibutes;
using SkyboxReplacer;
using UnityEngine;

namespace Lumina
{
    [Serializable]
    public class ShadowTabSettings
    {
        public bool ClassicFogEnabled { get; set; }
        public bool EdgeFogEnabled { get; set; }
        public bool FogEffectEnabled { get; set; }
        public float FogIntensity { get; set; }
        public float ColorDecay { get; set; }
        public float EdgeFogDistance { get; set; }
        public float HorizonHeight { get; set; }
        public float FogHeight { get; set; }
        public float FogDistance { get; set; }

        public string SelectedDayCubemap { get; set; }
        public string SelectedNightCubemap { get; set; }
    }

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
        private UILabel _cubemaplabel;
        private UILabel _cubemaplabel2;
        private UILabel _foglabel3;
        private UICheckBox _nightfog;
        private UISlider _colordecaySlider;
        private UILabel _Effects;
        private UIDropDown cubemapdropdown;
        private SunShaftsCompositeShaderProperties sunShaftsScript;
        private string _currentDayCubemap; // Store the current day cubemap value separately
        private Options Options;
        private string _currentNightCubemap; // Store the current night cubemap value separately
        private ShadowTabSettings _settings;
     
        private UISlider EdgeDistanceSlider;
        private UISlider HorizonHeight;
        private UISlider FogHeight;
       
        private UISlider FogDistanceSlider;

     
        private string _vanillaDayCubemap;
        private string _vanillaNightCubemap;
        private string _vanillaOuterSpaceCubemap;
        private float CurrentSlider = 8f;

     
        private List<string> GetCubemapItems()
        {
            List<string> items = new List<string>
            {
                "Vanilla", // Add the vanilla option to the dropdown
            };

            // Get the day cubemap options from CubemapManager
            DropDownEntry<string>[] dayCubemaps = CubemapManager.GetDayCubemaps();
            items.AddRange(dayCubemaps.Select(entry => entry.Code)); // Use entry.Description instead of entry.Value

            return items;
        }

        // Function to handle changes in the cubemap dropdown selection
        private void OnCubemapDropdownValueChanged(UIComponent component, int value)
        {
            string selectedCubemap = cubemapdropdown.items[value];

            // Get the day and night cubemap dictionaries from CubemapManager (No need for ImportCubemapDictionaries)
            CubemapManager.ImportFromMods();

            List<string> cubemaps = GetCubemapItems();

            // Handle day and night cubemap selection
            if (cubemaps.Contains(selectedCubemap))
            {
                if (CubemapManager.GetDayReplacement(selectedCubemap) != null)
                {
                    _settings.SelectedDayCubemap = selectedCubemap;
                    SetCubemapValue(selectedCubemap, isDayCubemap: true);
                    Debug.Log($"Setting day cubemap to: {selectedCubemap}");

                }
                else if (CubemapManager.GetNightReplacement(selectedCubemap) != null)
                {
                    _settings.SelectedNightCubemap = selectedCubemap;
                    SetCubemapValue(selectedCubemap, isDayCubemap: false);
                    Debug.Log($"Setting night cubemap to: {selectedCubemap}");
                }
            }
            else
            {
                // Handle the case where the selected cubemap is not found in the dictionary
                Debug.LogError($"Cubemap with code '{selectedCubemap}' not found in the dictionary.");
            }
        }


        // Function to set the selected cubemap value in the SkyboxReplacer.Options class
        private void SetCubemapValue(string cubemap, bool isDayCubemap)
        {
            if (isDayCubemap)
            {
                // Set the day cubemap in SkyboxReplacer
                SkyboxReplacer.SkyboxReplacer.SetDayCubemap(cubemap);
                _currentDayCubemap = cubemap;
                Debug.Log($"Setting day cubemap to: {cubemap}");
                SaveSettings();
            }
            else
            {
                // Set the night cubemap in SkyboxReplacer
                SkyboxReplacer.SkyboxReplacer.SetNightCubemap(cubemap);
                _currentNightCubemap = cubemap;
                Debug.Log($"Setting night cubemap to: {cubemap}");
                SaveSettings();
            }
        }
        internal ShadowTab(UITabstrip tabStrip, int tabIndex)
        {
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(LuminaTR.TranslationID.VISUALISM_MOD_NAME), tabIndex, out UIButton _);
            float currentY = Margin;

            if (ModUtils.IsModEnabled("skyboxreplacer"))
            {
                _cubemaplabel = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.CUBEMAP_TEXT_DISABLED), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                currentY += CheckHeight + Margin;

                _cubemaplabel2 = UILabels.AddLabel(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.CUBEMAP_TEXT_DISABLED_CAUSE), panel.width - (Margin * 2f), alignment: UIHorizontalAlignment.Center);
                _cubemaplabel2.textScale = 0.7f;
            }
            else
            {
                // Dropdown Cubemap
                cubemapdropdown = UIDropDowns.AddLabelledDropDown(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.CUBEMAP_TEXT), itemTextScale: 0.7f, width: panel.width - (Margin * 2f));
                cubemapdropdown.items = GetCubemapItems().ToArray();
                cubemapdropdown.eventSelectedIndexChanged += OnCubemapDropdownValueChanged;
                currentY += 30f;
            }

            // Check if Renderit mod or fog manipulating mods are enabled
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
                _currentDayCubemap = "Vanilla";
                _currentNightCubemap = "Vanilla";
                // Set the vanilla cubemap values
                _vanillaDayCubemap = UnityEngine.Object.FindObjectOfType<RenderProperties>()?.m_cubemap?.name;
                _vanillaNightCubemap = UnityEngine.Object.FindObjectOfType<RenderProperties>()?.m_cubemap?.name;
                _vanillaOuterSpaceCubemap = UnityEngine.Object.FindObjectOfType<DayNightProperties>()?.m_OuterSpaceCubemap?.name;

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
                _fogCheckBox.eventCheckChanged += (c, isChecked) => { LuminaLogic.ClassicFogEnabled = isChecked; SaveSettings(); };
                currentY += CheckHeight + Margin;

                // Checkbox 4: Edge Fog Checkbox
                _edgefogCheckbox = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.EDGEFOG_TEXT));
                _edgefogCheckbox.isChecked = LuminaLogic.EdgeFogEnabled;
                _edgefogCheckbox.eventCheckChanged += (c, isChecked) => { LuminaLogic.EdgeFogEnabled = isChecked; SaveSettings(); };
                currentY += CheckHeight + Margin;

                // Checkbox: Night Fog
                _nightfog = UICheckBoxes.AddLabelledCheckBox(panel, Margin, currentY, Translations.Translate(LuminaTR.TranslationID.NIGHTFOG_TEXT));
                _nightfog.isChecked = LuminaLogic.FogEffectEnabled;
                _nightfog.eventCheckChanged += (c, isChecked) => { LuminaLogic.FogEffectEnabled = isChecked; SaveSettings(); };
                currentY += CheckHeight + Margin;

                // Slider 3: Fog Intensity Slider
                _fogIntensitySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGINTENSITY_TEXT), 0f, 0.01f, -1, ref currentY);
                _fogIntensitySlider.value = LuminaLogic.FogIntensity;
                _fogIntensitySlider.eventValueChanged += (c, value) => { LuminaLogic.FogIntensity = value; SaveSettings(); };
                _fogIntensitySlider.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGINTENSITY_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 4 - Color Decay
                _colordecaySlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGVISIBILITY_TEXT), 0.06f, 0.4f, -1, ref currentY);
                _colordecaySlider.value = LuminaLogic.ColorDecay;
                _colordecaySlider.eventValueChanged += (c, value) => { LuminaLogic.ColorDecay = value; SaveSettings(); };
                _colordecaySlider.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGVISIBILITY_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 5 - Edge Distance
                EdgeDistanceSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.EDGEDISTANCE_TEXT), 0f, 3800f, -1, ref currentY);
                EdgeDistanceSlider.value = LuminaLogic.EdgeFogDistance;
                EdgeDistanceSlider.eventValueChanged += (c, value) => { LuminaLogic.EdgeFogDistance = value; SaveSettings(); };
                EdgeDistanceSlider.tooltip = Translations.Translate(LuminaTR.TranslationID.EDGEDISTANCE_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                // Slider 6 - Horizon Height
                HorizonHeight = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.HORIZONHEIGHT_TEXT), 0f, 5000f, -1, ref currentY);
                HorizonHeight.value = LuminaLogic.HorizonHeight;
                HorizonHeight.eventValueChanged += (c, value) => { LuminaLogic.HorizonHeight = value; SaveSettings(); };
                HorizonHeight.tooltip = Translations.Translate(LuminaTR.TranslationID.HORIZONHEIGHT_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)

                FogHeight = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGHEIGHT_TEXT), 0f, 5000f, -1, ref currentY);
                FogHeight.value = LuminaLogic.FogHeight;
                FogHeight.eventValueChanged += (c, value) => { LuminaLogic.FogHeight = value; SaveSettings(); };
                FogHeight.tooltip = Translations.Translate(LuminaTR.TranslationID.FOGHEIGHT_TEXT);
                currentY += CurrentSlider; // Adjust the spacing as needed (10 in this case)


                // Slider 7 - Fog Distance
                // Assuming you have a reference to the FogEffect component named "fogEffectComponent"
                FogDistanceSlider = AddSlider(panel, Translations.Translate(LuminaTR.TranslationID.FOGDISTANCE_TEXT), -10f, 20000f, -1, ref currentY);
                FogDistanceSlider.value = LuminaLogic.FogDistance;
                FogDistanceSlider.eventValueChanged += (c, value) =>
                {
                    LuminaLogic.VolumeFogDistance = value;
                    LuminaLogic.FogDistance = value;
                    LuminaLogic.ThirdFogDistance = 0f;
                    SaveSettings();

                };

                // Reset Button
                UIButton resetButton = UIButtons.AddSmallerButton(panel, ControlWidth - 120f, currentY, Translations.Translate(LuminaTR.TranslationID.RESET_TEXT), 120f);
                resetButton.eventClicked += (c, p) =>
                {
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
                };

                // Calculate the X-coordinate for reset2Button based on resetButton's position and width
                float advbuttonX = resetButton.relativePosition.x - 120f;

                UIButton advbutton = UIButtons.AddSmallerButton(panel, advbuttonX, currentY, Translations.Translate(LuminaTR.TranslationID.ADVANCED), 120f);
                advbutton.eventClicked += (c, p) =>
                {
                    StandalonePanelManager<AdvancedTab>.Create();
                };

            }
        }

        private void Awake()
        {
            // Create the settings file if it doesn't exist
            EnsureSettingsFileExists();
        }

        public void EnsureSettingsFileExists()
        {
            string filePath = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "ShadowTabSettings.xml");
            if (!File.Exists(filePath))
            {
                // Create a new settings object if the file doesn't exist
                _settings = new ShadowTabSettings();
                Debug.Log("[LUMINA] ShadowTabSettings.xml file created.");

                // Save the default settings to the XML file
                SaveSettings();
            }
            else
            {
                // Load existing settings from the XML file
                LoadSettings();
            }
        }

        // Helper method to load saved settings from an XML file
        public void LoadSettings()
        {
            string filePath = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "ShadowTabSettings.xml");
            if (File.Exists(filePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ShadowTabSettings));
                    using (FileStream stream = new FileStream(filePath, FileMode.Open))
                    {
                        // Deserialize the XML and store the settings in the _settings object
                        _settings = (ShadowTabSettings)serializer.Deserialize(stream);

                        // Apply the loaded settings to UI elements
                        _fogCheckBox.isChecked = _settings.ClassicFogEnabled;
                        _edgefogCheckbox.isChecked = _settings.EdgeFogEnabled;
                        _nightfog.isChecked = _settings.FogEffectEnabled;
                        _fogIntensitySlider.value = _settings.FogIntensity;
                        _colordecaySlider.value = _settings.ColorDecay;
                        EdgeDistanceSlider.value = _settings.EdgeFogDistance;
                        HorizonHeight.value = _settings.HorizonHeight;
                        FogHeight.value = _settings.FogHeight;
                        FogDistanceSlider.value = _settings.FogDistance;

                        SetCubemapValue(_settings.SelectedDayCubemap, isDayCubemap: true);
                        SetCubemapValue(_settings.SelectedNightCubemap, isDayCubemap: false);
                        // Add code to apply other settings as needed
                        Debug.Log("[LUMINA] Visualism tab settings loaded succesfully.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[LUMINA] Error loading settings: " + ex.Message);
                }
            }
            else
            {
                // Create a new settings object if the file doesn't exist
                _settings = new ShadowTabSettings();
            }
        }

        // Helper method to save current settings to an XML file
        public void SaveSettings()
        {
            try
            {
                // Save UI element settings into the _settings object
                _settings.ClassicFogEnabled = _fogCheckBox.isChecked;
                _settings.EdgeFogEnabled = _edgefogCheckbox.isChecked;
                _settings.FogEffectEnabled = _nightfog.isChecked;
                _settings.FogIntensity = _fogIntensitySlider.value;
                _settings.ColorDecay = _colordecaySlider.value;
                _settings.EdgeFogDistance = EdgeDistanceSlider.value;
                _settings.HorizonHeight = HorizonHeight.value;
                _settings.FogHeight = FogHeight.value;
                _settings.FogDistance = FogDistanceSlider.value;

                //SAVE ALL SETTINGS 
                string filePath = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, "ShadowTabSettings.xml");
                XmlSerializer serializer = new XmlSerializer(typeof(ShadowTabSettings));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, _settings);
                }
                Debug.Log("[LUMINA] Visualism tab settings saved succesfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[LUMINA] Error saving settings: " + ex.Message);
            }
        }

        // Override the OnDestroy method to save settings when the tab is destroyed
        
    }
}
