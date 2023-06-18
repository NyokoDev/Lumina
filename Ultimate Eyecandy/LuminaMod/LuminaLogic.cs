using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AlgernonCommons.Translation;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnifiedUI.Helpers;
using UnityEngine;

namespace Lumina
{
    public class LuminaLogic : UIPanel
    {
        // SIMON PART (UI + SAVING)

        private Rect windowRect = new Rect(200, 200, 450, 581);
        private int instanceID, windowMode = 0;
        private Vector2 scrollstyles = Vector2.zero;


        // lighting values
        public float[] lightingValues, oldLightingValues;
        public bool skyTonemappingUi, oldSkyTonemappingUi;

        // shadows values
        public bool disableSmoothing, oldDisableSmoothing;
        public static bool forceLowBias, oldForceLowBias;

        public string CachePath = DataLocation.localApplicationData + "\\LuminaCache.light";


        private bool _showUI = false;
        private UUICustomButton _uuiButton;

        /// <summary>
        /// Gets or sets a value indicating whether the UI should be shown.  Invoked only from main thread.
        /// </summary>
        private bool ShowUI

        {
            get => _showUI;

            set
            {
                // Don't do anything if no change.
                if (value != _showUI)
                {
                    _showUI = value;

                    /// Update values if hiding.
                    if (!value)
                    {
                        if (IsAnyDifference())
                        {
                            CalculateAll(lightingValues, skyTonemappingUi);
                            ApplyShadowValues();
                            SaveCache();
                        }
                        oldLightingValues = new float[]
                        {
                    lightingValues[0], lightingValues[1], lightingValues[2], lightingValues[3], lightingValues[4], lightingValues[5], lightingValues[6], lightingValues[7], lightingValues[8], lightingValues[9], lightingValues[10], lightingValues[11], lightingValues[12]
                        };
                        oldSkyTonemappingUi = skyTonemappingUi;
                        oldDisableSmoothing = disableSmoothing;
                        oldForceLowBias = forceLowBias;
                    }
                }
            }
        }

        public override void Start()
        {
            base.Start();
            backgroundSprite = "MenuPanelInfo";
            canFocus = true;
            isInteractive = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = new RectOffset(10, 10, 10, 10);


            // Add UUI button.
            _uuiButton = UUIHelpers.RegisterCustomButton(
                name: LuminaMod.Instance.Name,
                groupName: null, // default group
                tooltip: Translations.Translate("MOD_NAME"),
                icon: UUIHelpers.LoadTexture(UUIHelpers.GetFullPath<LuminaMod>("Resources", "UUI.png")),
                onToggle: (value) => ShowUI = value,
                hotkeys: new UUIHotKeys { ActivationKey = ModSettings.ToggleKey }) ;


            instanceID = this.GetInstanceID();
            Lightstyle.LoadLightstyles();

            if (File.Exists(CachePath))
            {
                LoadCache();
                CalculateAll(lightingValues, skyTonemappingUi);
                ApplyShadowValues();
            }
            else
                AllToZero();
        }

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            base.OnMouseDown(p);
            BringToFront();
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            base.OnMouseMove(p);
            if (p.buttons.IsFlagSet(UIMouseButton.Left))
            {
                windowRect.x += p.moveDelta.x;
                windowRect.y += p.moveDelta.y;
            }
        }

        protected void OnGUI()
        {
            if (ShowUI)
            {
                windowRect = GUI.Window(GetHashCode(), windowRect, DrawWindow, "Lumina");
            }
        }

        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 420, 20));
            string buttonText = Translations.Translate(LuminaTR.TranslationID.CLOSE_TEXT);
            float buttonWidth = GUI.skin.label.CalcSize(new GUIContent(buttonText)).x + 10f;
            if (GUI.Button(new Rect(422, 4, buttonWidth, 20), buttonText))
            {
                ShowUI = false;
                _uuiButton.IsPressed = false;
            }

            windowMode = GUI.Toolbar(new Rect(5, 26, 440, 25), windowMode, new string[] {
        Translations.Translate(LuminaTR.TranslationID.LIGHTNESS_MOD_NAME),
        Translations.Translate(LuminaTR.TranslationID.STYLES_MOD_NAME),
        Translations.Translate(LuminaTR.TranslationID.COLOR_CORRECTION_MOD_NAME),
        Translations.Translate(LuminaTR.TranslationID.VISUALISM_MOD_NAME)
    });

            if (windowMode == 0)
            {
                // CUSTOMIZATION WINDOW MODE

                GUI.skin.label.fontSize = 14;
                GUI.Label(new Rect(180, 60, 150, 26), Translations.Translate(LuminaTR.TranslationID.EXPOSURECONTROL_TEXT));
                // Luminosity
                GUI.Label(new Rect(5, 80, 115, 30), Translations.Translate(LuminaTR.TranslationID.LUMINOSITY_TEXT));
                lightingValues[10] = GUI.HorizontalSlider(new Rect(120, 85, 270, 25), lightingValues[10], -1f, 1f);
                if (GUI.Button(new Rect(395, 80, 50, 30), lightingValues[10].ToString(), GUI.skin.label))
                {
                    lightingValues[10] = 0;
                }
                // gamma
                GUI.Label(new Rect(5, 110, 115, 30), Translations.Translate(LuminaTR.TranslationID.GAMMA_TEXT));
                lightingValues[11] = GUI.HorizontalSlider(new Rect(120, 115, 270, 25), lightingValues[11], -1f, 1f);
                if (GUI.Button(new Rect(395, 110, 50, 30), lightingValues[11].ToString(), GUI.skin.label))
                {
                    lightingValues[11] = 0;
                }
                // contrast
                GUI.Label(new Rect(5, 140, 115, 30), Translations.Translate(LuminaTR.TranslationID.RADIANCE_TEXT));
                lightingValues[12] = GUI.HorizontalSlider(new Rect(120, 145, 270, 25), lightingValues[12], -1f, 1f);
                if (GUI.Button(new Rect(395, 140, 50, 30), lightingValues[12].ToString(), GUI.skin.label))
                {
                    lightingValues[12] = 0;
                }

                GUI.Label(new Rect(200, 160, 150, 26), Translations.Translate(LuminaTR.TranslationID.LIGHTING_TEXT));
                // temperature
                GUI.Label(new Rect(5, 190, 115, 30), Translations.Translate(LuminaTR.TranslationID.HUE_TEXT));
                lightingValues[0] = GUI.HorizontalSlider(new Rect(120, 195, 270, 25), lightingValues[0], -1f, 1f);
                if (GUI.Button(new Rect(395, 190, 50, 30), lightingValues[0].ToString(), GUI.skin.label))
                {
                    lightingValues[0] = 0;
                }
                // tint
                GUI.Label(new Rect(5, 220, 115, 30), Translations.Translate(LuminaTR.TranslationID.TINT_TEXT));
                lightingValues[1] = GUI.HorizontalSlider(new Rect(120, 225, 270, 25), lightingValues[1], -1f, 1f);
                if (GUI.Button(new Rect(395, 220, 50, 30), lightingValues[1].ToString(), GUI.skin.label))
                {
                    lightingValues[1] = 0;
                }
                // sun temperature
                GUI.Label(new Rect(5, 250, 115, 30), Translations.Translate(LuminaTR.TranslationID.SUNTEMP_TEXT));
                lightingValues[2] = GUI.HorizontalSlider(new Rect(120, 255, 270, 25), lightingValues[2], -1f, 1f);
                if (GUI.Button(new Rect(395, 250, 50, 30), lightingValues[2].ToString(), GUI.skin.label))
                {
                    lightingValues[2] = 0;
                }
                // sun tint
                GUI.Label(new Rect(5, 280, 115, 30), Translations.Translate(LuminaTR.TranslationID.SUNTINT_TEXT));
                lightingValues[3] = GUI.HorizontalSlider(new Rect(120, 285, 270, 25), lightingValues[3], -1f, 1f);
                if (GUI.Button(new Rect(395, 280, 50, 30), lightingValues[3].ToString(), GUI.skin.label))
                {
                    lightingValues[3] = 0;
                }
                // sky temperature
                GUI.Label(new Rect(5, 310, 115, 30), Translations.Translate(LuminaTR.TranslationID.SKYTEMP_TEXT));
                lightingValues[4] = GUI.HorizontalSlider(new Rect(120, 315, 270, 25), lightingValues[4], -1f, 1f);
                if (GUI.Button(new Rect(395, 310, 50, 30), lightingValues[4].ToString(), GUI.skin.label))
                {
                    lightingValues[4] = 0;
                }
                // sky tint
                GUI.Label(new Rect(5, 340, 115, 30), Translations.Translate(LuminaTR.TranslationID.SKYTINT_TEXT));
                lightingValues[5] = GUI.HorizontalSlider(new Rect(120, 345, 270, 25), lightingValues[5], -1f, 1f);
                if (GUI.Button(new Rect(395, 340, 50, 30), lightingValues[5].ToString(), GUI.skin.label))
                {
                    lightingValues[5] = 0;
                }
                // moon temperature
                GUI.Label(new Rect(5, 370, 115, 30), Translations.Translate(LuminaTR.TranslationID.MOONTEMP_TEXT));
                lightingValues[6] = GUI.HorizontalSlider(new Rect(120, 375, 270, 25), lightingValues[6], -1f, 1f);
                if (GUI.Button(new Rect(395, 370, 50, 30), lightingValues[6].ToString(), GUI.skin.label))
                {
                    lightingValues[6] = 0;
                }
                // moon tint
                GUI.Label(new Rect(5, 400, 115, 30), Translations.Translate(LuminaTR.TranslationID.MOONTINT_TEXT));
                lightingValues[7] = GUI.HorizontalSlider(new Rect(120, 405, 270, 25), lightingValues[7], -1f, 1f);
                if (GUI.Button(new Rect(395, 400, 50, 30), lightingValues[7].ToString(), GUI.skin.label))
                {
                    lightingValues[7] = 0;
                }
                // moonlight
                GUI.Label(new Rect(5, 430, 115, 30), Translations.Translate(LuminaTR.TranslationID.MOONLIGHT_TEXT));
                lightingValues[8] = GUI.HorizontalSlider(new Rect(120, 435, 270, 25), lightingValues[8], -1f, 1f);
                if (GUI.Button(new Rect(395, 430, 50, 30), lightingValues[8].ToString(), GUI.skin.label))
                {
                    lightingValues[8] = 0;
                }
                // Twilight tint
                GUI.Label(new Rect(5, 460, 115, 30), Translations.Translate(LuminaTR.TranslationID.TWILIGHTTINT_TEXT));
                lightingValues[9] = GUI.HorizontalSlider(new Rect(120, 465, 270, 25), lightingValues[9], -1f, 1f);
                if (GUI.Button(new Rect(395, 460, 50, 30), lightingValues[9].ToString(), GUI.skin.label))
                {
                    lightingValues[9] = 0;
                }

                skyTonemappingUi = GUI.Toggle(new Rect(5, 495, 348, 28), skyTonemappingUi, Translations.Translate(LuminaTR.TranslationID.ENABLE_SKYTONE_TEXT));
                disableSmoothing = GUI.Toggle(new Rect(5, 523, 348, 28), disableSmoothing, Translations.Translate(LuminaTR.TranslationID.DISABLE_SHADOWSMOOTH_TEXT));
                forceLowBias = GUI.Toggle(new Rect(5, 551, 348, 28), forceLowBias, Translations.Translate(LuminaTR.TranslationID.FORCELOWBIAS_TEXT));

                if (GUI.Button(new Rect(355, 516, 88, 28), Translations.Translate(LuminaTR.TranslationID.RESET_TEXT)))
                {
                    AllToZero();
                }
            }
            else if (windowMode == 1)
            {
                // PRESETS WINDOW MODE

                if (Lightstyle.Lightstyles.Count > 0)
                {
                    scrollstyles = GUI.BeginScrollView(new Rect(5, 55, 440, 200), scrollstyles, new Rect(0, 0, 415, Lightstyle.Lightstyles.Count * 30));
                    for (int i = 0; i < Lightstyle.Lightstyles.Count; i++)
                    {
                        var preset = Lightstyle.Lightstyles[i];
                        GUI.Label(new Rect(5, i * 30 + 3, 240, 25), preset.m_styleName);
                        if (GUI.Button(new Rect(245, i * 30, 70, 28), "Load"))
                        {
                            Loadstyle(preset);
                        }
                        if (preset.m_canBeDeleted)
                        {
                            if (GUI.Button(new Rect(320, i * 30, 70, 28), "Delete"))
                            {
                                Lightstyle.Deletestyle(preset);
                            }
                        }
                        else
                        {
                            GUI.Label(new Rect(320, i * 30 + 2, 70, 28), "[ <i>Workshop</i> ]");
                        }
                    }
                    GUI.EndScrollView();
                }
                else
                {
                    GUI.Box(new Rect(5, 55, 440, 200), string.Empty);
                    GUI.Label(new Rect(28, 60, 410, 30), "No style found !");
                }
                if (GUI.Button(new Rect(5, 260, 440, 27), "Reload Styles List"))
                {
                    Lightstyle.LoadLightstyles();
                }

                GlobalVariables input = new GlobalVariables();
                string nameTextfield = input.nameTextfield;

                if (File.Exists(DataLocation.localApplicationData + @"\Lumina\Styles\" + Lightstyle.ToFileName(nameTextfield) + ".light"))
                {
                    GUI.color = Color.red;

                    nameTextfield = GUI.TextField(new Rect(5, 290, 440, 27), nameTextfield);
                    GUI.Label(new Rect(5, 320, 440, 27), "<b>A style with this name already exists !</b>", GUI.skin.button);
                    GUI.color = Color.white;
                }
                else
                {
                    nameTextfield = GUI.TextField(new Rect(5, 290, 440, 27), nameTextfield);
                    if (GUI.Button(new Rect(5, 320, 440, 27), "<b>+</b> Save Style"))
                    {
                        string path = DataLocation.localApplicationData + @"\Addons\Mods\" + nameTextfield + "\\Source";
                        Createstyle();
                        CreateSourceCode(path, "LuminaX1", nameTextfield);


                    }
                }
                if (GUI.Button(new Rect(5, 350, 440, 27), "Open Local Styles Folder"))
                {
                    string path = DataLocation.localApplicationData + @"\Lumina\Styles\";
                    if (!Directory.Exists(DataLocation.localApplicationData + @"\Lumina\"))
                        Directory.CreateDirectory(DataLocation.localApplicationData + @"\Lumina\");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    Application.OpenURL("file://" + path);
                }
            }
            else if (windowMode == 2)
            {
                // CUSTOMIZATION WINDOW MODE - Color Correction

                GUI.Label(new Rect(180, 60, 150, 26), "<size=14>LUT Creator</size>");

                // Add the button below the label
                if (GUI.Button(new Rect(5, 80, 115, 30), Translations.Translate(LuminaTR.TranslationID.LAUNCHLUT_TEXT)))
                {
                    string lutEditorPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2983036781\LUT Editor\";
                    Process.Start(lutEditorPath);
                }
            }
            else if (windowMode == 3)
            {
                // VISUALISM WINDOW MODE
                GUI.skin.label.fontSize = 14;
                GUI.Label(new Rect(180, 60, 150, 26), Translations.Translate(LuminaTR.TranslationID.VISUALISM_MOD_NAME));

                // Adjust game shadows with sliders
                GUI.Label(new Rect(5, 90, 115, 30), Translations.Translate(LuminaTR.TranslationID.SHADOWINT_TEXT));
                shadowIntensity = GUI.HorizontalSlider(new Rect(120, 95, 270, 25), shadowIntensity, -1f, 3f);
                GUI.Label(new Rect(395, 90, 50, 30), shadowIntensity.ToString(), GUI.skin.label);

                GUI.Label(new Rect(5, 150, 115, 30), Translations.Translate(LuminaTR.TranslationID.SHADOWBIAS_TEXT));
                shadowBias = GUI.HorizontalSlider(new Rect(120, 155, 270, 25), shadowBias, -1f, 3f);
                GUI.Label(new Rect(395, 150, 50, 30), shadowBias.ToString(), GUI.skin.label);

                UpdateShadowSettings();
            }
        }

        void UpdateShadowSettings()
        {
            if (directionalLight != null)
            {
                directionalLight.shadowStrength = shadowIntensity;
                directionalLight.shadowBias = shadowBias;

                UnityEngine.Debug.Log("Updated shadow settings: Intensity = " + shadowIntensity + ", Bias = " + shadowBias);
            }
        }

        private void CreateSourceCode(string mixModSourceDir, string mixNameTypeSafe, string mixName)
        {
            var sb = new StringBuilder();
            if (!Directory.Exists(mixModSourceDir))
            {
                try
                {
                    Directory.CreateDirectory(mixModSourceDir);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log(string.Concat("Failed Creating Lumina Style source code directory: ", e.Message));
                    throw;
                }
            }
            sb.AppendLine("using ICities;");
            sb.AppendLine($"namespace {mixNameTypeSafe}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {mixNameTypeSafe}Mod : IUserMod");
            sb.AppendLine("    {");
            sb.AppendLine("        public string Name {");
            sb.AppendLine("            get {");
            sb.AppendLine($"                return \"{mixName}\";");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("        public string Description {");
            sb.AppendLine("            get {");
            sb.AppendLine("                return \"Lumina Style for use with Lumina\";");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            string code = sb.ToString();
            sb = new StringBuilder();
            try
            {
                File.WriteAllText(Path.Combine(mixModSourceDir, mixNameTypeSafe + ".cs"), code);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(string.Concat("Failed Creating Lumina Style source code: ", e.Message));
                throw;
            }
            GlobalVariables input = new GlobalVariables();
            string nameTextfield = input.nameTextfield;

            string mixdir = DataLocation.localApplicationData + @"\Addons\Mods\" + nameTextfield;
            PluginManager.CompileSourceInFolder(mixModSourceDir, mixdir, new string[] { typeof(ICities.IUserMod).Assembly.Location });
        }



        public Light directionalLight;
        [Range(-1f, 3f)]
        public float shadowIntensity = 1f;
        [Range(-1f, 3f)]
        public float shadowBias = 1f;




        public static void CalculateAll(float[] values, bool skyTonemapping)
        {
            CalculateLighting(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]);
            CalculateTonemapping(values[10], values[11], values[12]);
            SkyTonemapping(skyTonemapping);
        }
        private bool IsAnyDifference(/*float[] valuesNew, bool skyTonemappingNew, float[] valuesOld, bool skyTonemappingOld*/)
        {
            for (int i = 0; i <= 12; i++)
            {
                if (lightingValues[i] != oldLightingValues[i])
                    return true;
            }
            if (disableSmoothing != oldDisableSmoothing)
                return true;
            if (forceLowBias != oldForceLowBias)
                return true;
            return (skyTonemappingUi != oldSkyTonemappingUi);
        }

        public void SaveCache()
        {
            if (File.Exists(CachePath))
                File.Delete(CachePath);
            TextWriter tw = new StreamWriter(CachePath);
            tw.WriteLine("0 = " + lightingValues[0]);
            tw.WriteLine("1 = " + lightingValues[1]);
            tw.WriteLine("2 = " + lightingValues[2]);
            tw.WriteLine("3 = " + lightingValues[3]);
            tw.WriteLine("4 = " + lightingValues[4]);
            tw.WriteLine("5 = " + lightingValues[5]);
            tw.WriteLine("6 = " + lightingValues[6]);
            tw.WriteLine("7 = " + lightingValues[7]);
            tw.WriteLine("8 = " + lightingValues[8]);
            tw.WriteLine("9 = " + lightingValues[9]);
            tw.WriteLine("10 = " + lightingValues[10]);
            tw.WriteLine("11 = " + lightingValues[11]);
            tw.WriteLine("12 = " + lightingValues[12]);
            tw.WriteLine("disableSmoothing = " + disableSmoothing);
            tw.WriteLine("forceLowBias = " + forceLowBias);
            tw.WriteLine("skyTmpg = " + skyTonemappingUi);
            tw.Close();
        }
        public void ApplyShadowValues()
        {
            if (disableSmoothing)
                QualitySettings.shadows = ShadowQuality.HardOnly;
            else
                QualitySettings.shadows = ShadowQuality.All;
        }
        public void LoadCache()
        {
            if (!File.Exists(CachePath))
                SaveCache();
            string[] lines = File.ReadAllLines(CachePath);
            var dict = new Dictionary<int, float>();
            foreach (string line in lines.Where(s => s.Contains(" = ")))
            {
                string[] data = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                if (data[0] == "skyTmpg")
                {
                    skyTonemappingUi = bool.Parse(data[1]);
                    oldSkyTonemappingUi = skyTonemappingUi;
                }
                else if (data[0] == "disableSmoothing")
                {
                    disableSmoothing = bool.Parse(data[1]);
                    oldDisableSmoothing = disableSmoothing;
                }
                else if (data[0] == "forceLowBias")
                {
                    forceLowBias = bool.Parse(data[1]);
                    oldForceLowBias = forceLowBias;
                }
                else
                {
                    int id = 0;
                    if (int.TryParse(data[0], out id))
                    {
                        dict[id] = 0;
                        dict[id] = float.Parse(data[1]);
                    }
                }
            }
            lightingValues = new float[]
            {
                dict[0], dict[1], dict[2], dict[3], dict[4], dict[5], dict[6], dict[7], dict[8], dict[9], dict[10], dict[11], dict[12]
            };
            oldLightingValues = new float[]
            {
                dict[0], dict[1], dict[2], dict[3], dict[4], dict[5], dict[6], dict[7], dict[8], dict[9], dict[10], dict[11], dict[12]
            };
        }
        public void Loadstyle(Lightstyle style)
        {
            lightingValues = new float[]
            {
                style.m_values[0], style.m_values[1], style.m_values[2], style.m_values[3], style.m_values[4], style.m_values[5], style.m_values[6], style.m_values[7], style.m_values[8], style.m_values[9], style.m_values[10], style.m_values[11], style.m_values[12]
            };
            oldLightingValues = new float[]
            {
                style.m_values[0], style.m_values[1], style.m_values[2], style.m_values[3], style.m_values[4], style.m_values[5], style.m_values[6], style.m_values[7], style.m_values[8], style.m_values[9], style.m_values[10], style.m_values[11], style.m_values[12]
            };
            skyTonemappingUi = style.m_skyTonemapping;
            oldSkyTonemappingUi = style.m_skyTonemapping;
            CalculateAll(style.m_values, style.m_skyTonemapping);
            SaveCache();
        }
        public void Createstyle()
        {
            Lightstyle style = new Lightstyle();
            GlobalVariables input = new GlobalVariables();
            string nameTextfield = input.nameTextfield;
            style.m_styleName = nameTextfield;
            style.m_values = new float[]
            {
                lightingValues[0], lightingValues[1], lightingValues[2], lightingValues[3], lightingValues[4], lightingValues[5], lightingValues[6], lightingValues[7], lightingValues[8], lightingValues[9], lightingValues[10], lightingValues[11], lightingValues[12]
            };
            style.m_canBeDeleted = true;
            style.m_skyTonemapping = skyTonemappingUi;
            style.Savestyle(true);
        }

        private void AllToZero()
        {
            lightingValues = new float[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            oldLightingValues = new float[]
                {
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                };
            skyTonemappingUi = true;
            oldSkyTonemappingUi = skyTonemappingUi;
            SaveCache();
            CalculateAll(lightingValues, skyTonemappingUi);
        }

        #region ronyx PART (LOGIC)


        // Intensity Tweak
        private static float tweak_Intensity = 1.90f;

        // Time
        private static float time_Sunrise1 = 0.23f;
        private static float time_Sunrise2 = 0.27f;
        private static float time_Early = 0.40f;
        private static float time_Late = 0.60f;
        private static float time_Sunset1 = 0.74f;
        private static float time_Sunset2 = 0.76f;

        // Placeholder Color
        private static Color initcolor = new Color(1f, 1f, 1f, 1f);

        // Lighting gradients, that will be edited and assigned, instead of creating new ones every time.
        private static Gradient gradient_Direct = new Gradient
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(initcolor, time_Sunrise1),
                new GradientColorKey(initcolor, time_Sunrise2),
                new GradientColorKey(initcolor, time_Early),
                new GradientColorKey(initcolor, time_Late),
                new GradientColorKey(initcolor, time_Sunset1),
                new GradientColorKey(initcolor, time_Sunset2),
            },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        };

        private static Gradient gradient_Sky = new Gradient
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(initcolor, time_Sunrise1),
                new GradientColorKey(initcolor, time_Sunrise2),
                new GradientColorKey(initcolor, time_Early),
                new GradientColorKey(initcolor, time_Late),
                new GradientColorKey(initcolor, time_Sunset1),
                new GradientColorKey(initcolor, time_Sunset2),
            },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        };

        private static Gradient gradient_Equator = new Gradient
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(initcolor, time_Sunrise1),
                new GradientColorKey(initcolor, time_Sunrise2),
                new GradientColorKey(initcolor, time_Early),
                new GradientColorKey(initcolor, time_Late),
                new GradientColorKey(initcolor, time_Sunset1),
                new GradientColorKey(initcolor, time_Sunset2),
            },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        };

        private static Gradient gradient_Ground = new Gradient
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(initcolor, time_Sunrise1),
                new GradientColorKey(initcolor, time_Sunrise2),
                new GradientColorKey(initcolor, time_Early),
                new GradientColorKey(initcolor, time_Late),
                new GradientColorKey(initcolor, time_Sunset1),
                new GradientColorKey(initcolor, time_Sunset2),
            },
            alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        };


        public static void CalculateLighting(float temperature, float tint, float sunTemp, float sunTint, float skyTemp, float skyTint, float moonTemp, float moonTint, float moonLight, float twilightTint)
        {

            // Day/Night Value
            var mult_Day = 1.00f * tweak_Intensity;
            var mult_Night = 0.32f * (1 + (0.7f * moonLight)) * tweak_Intensity;
            var mult_Twilight = ((mult_Day * 2) + mult_Night) / 3f;

            // Light Value
            var mult_Ambient = 0.60f;
            var mult_Sun = 0.25f;
            var mult_Moon = 0.20f;
            var mult_Sky = 0.95f;
            var mult_Equator = 0.85f;
            var mult_Ground = 0.65f;


            // Main
            var mult_Temperature = temperature;
            var mult_Tint = tint;
            var tint_Twilight = 0.08f * (twilightTint + 1);
            var temp_Sun = sunTemp;
            var temp_Moon = -moonTemp;
            var temp_Ambient = -skyTemp;
            var tint_Sun = sunTint;
            var tint_Moon = moonTint;
            var tint_Ambient = skyTint;


            // Overall Temperature and Tint
            var calc_tempMultR = 1 + (0.1f * mult_Temperature);
            var calc_tempMultB = 1 - (0.1f * mult_Temperature);
            var calc_tintMult = 1 + (0.1f * mult_Tint);

            // Sun Temperature and Tint
            var calc_tempOffset_Sun = (((temp_Sun + 1) * 0.5f) * 0.05f);
            var calc_tintOffset_Sun = ((tint_Sun * 0.5f) * 0.05f);

            // Moon Temperature and Tint
            var calc_tempOffset_Moon = (((temp_Moon + 1) * 0.5f) * 0.1f);
            var calc_tintOffset_Moon = ((tint_Moon * 0.5f) * 0.06f);

            // Ambient Temperature and Tint
            var calc_tempOffset_Sky = (((temp_Ambient + 1) * 0.5f) * 0.23f);
            var calc_tempOffset_Equator = (((temp_Ambient + 1) * 0.5f) * 0.18f);
            var calc_tempOffset_Ground = (((temp_Ambient + 1) * 0.5f) * 0.13f);
            var calc_tintOffset_Ambient = tint_Ambient * 0.06f;


            // Value Day
            var calc_SunDay = mult_Sun * mult_Day;
            var calc_SkyDay = mult_Sky * mult_Day * mult_Ambient;
            var calc_EquatorDay = mult_Equator * mult_Day * mult_Ambient;
            var calc_GroundDay = mult_Ground * mult_Day * mult_Ambient;

            // Value Night
            var calc_MoonNight = mult_Moon * mult_Night;
            var calc_SkyNight = mult_Sky * mult_Night * mult_Ambient;
            var calc_EquatorNight = mult_Equator * mult_Night * mult_Ambient;
            var calc_GroundNight = mult_Ground * mult_Night * mult_Ambient;

            // Value Twilight
            var calc_SunTwilight = mult_Moon * mult_Twilight;
            var calc_SkyTwilight = mult_Sky * mult_Twilight * mult_Ambient * 1.2f;
            var calc_EquatorTwilight = mult_Equator * mult_Twilight * mult_Ambient * 1.2f;
            var calc_GroundTwilight = mult_Ground * mult_Twilight * mult_Ambient * 1.2f;


            // Day Colors
            Color color_Direct_Day = new Color(
                (calc_SunDay + (0.92f * calc_tempOffset_Sun)) * calc_tempMultR,
                (calc_SunDay + calc_tintOffset_Sun) * calc_tintMult,
                (calc_SunDay - calc_tempOffset_Sun) * calc_tempMultB,
                1f);

            Color color_Sky_Day = new Color(
                (calc_SkyDay - calc_tempOffset_Sky) * calc_tempMultR,
                (calc_SkyDay + calc_tintOffset_Ambient) * calc_tintMult,
                (calc_SkyDay + (0.92f * calc_tempOffset_Sky)) * calc_tempMultB,
                1f);

            Color color_Equator_Day = new Color(
                (calc_EquatorDay - calc_tempOffset_Equator) * calc_tempMultR,
                (calc_EquatorDay + calc_tintOffset_Ambient) * calc_tintMult,
                (calc_EquatorDay + (0.92f * calc_tempOffset_Equator)) * calc_tempMultB,
                1f);

            Color color_Ground_Day = new Color(
                (calc_GroundDay - calc_tempOffset_Ground) * calc_tempMultR,
                (calc_GroundDay + calc_tintOffset_Ambient) * calc_tintMult,
                (calc_GroundDay + (0.92f * calc_tempOffset_Ground)) * calc_tempMultB,
                1f);

            // Night Colors
            Color color_Direct_Night = new Color(
                (calc_MoonNight - calc_tempOffset_Moon) * calc_tempMultR,
                (calc_MoonNight + calc_tintOffset_Moon) * calc_tintMult,
                (calc_MoonNight + calc_tempOffset_Moon) * calc_tempMultB,
                1f);

            Color color_Sky_Night = new Color(
                (calc_SkyNight - calc_tempOffset_Sky) * calc_tempMultR,
                (calc_SkyNight + calc_tintOffset_Ambient) * calc_tintMult,
                (calc_SkyNight + (0.92f * calc_tempOffset_Sky)) * calc_tempMultB,
                1f);

            Color color_Equator_Night = new Color(
                (calc_EquatorNight - calc_tempOffset_Equator) * calc_tempMultR,
                (calc_EquatorNight + calc_tintOffset_Ambient) * calc_tintMult,
                (calc_EquatorNight + (0.92f * calc_tempOffset_Equator)) * calc_tempMultB,
                1f);

            Color color_Ground_Night = new Color(
                (calc_GroundNight - calc_tempOffset_Ground) * calc_tempMultR,
                (calc_GroundNight + calc_tintOffset_Ambient) * calc_tintMult,
                (calc_GroundNight + (0.92f * calc_tempOffset_Ground)) * calc_tempMultB,
                1f);

            // Twilight Colors
            Color color_Direct_Twilight = new Color(
                (calc_SunTwilight + calc_tempOffset_Sun + tint_Twilight) * calc_tempMultR,
                (calc_SunTwilight + calc_tintOffset_Sun) * calc_tintMult,
                (calc_SunTwilight - calc_tempOffset_Sun - tint_Twilight) * calc_tempMultB,
                1f);

            Color color_Sky_Twilight = new Color(
                (calc_SkyTwilight - calc_tempOffset_Sky - tint_Twilight) * calc_tempMultR,
                (calc_SkyTwilight + calc_tintOffset_Ambient - (0.1f * tint_Twilight)) * calc_tintMult,
                (calc_SkyTwilight + calc_tempOffset_Sky + tint_Twilight) * calc_tempMultB,
                1f);

            Color color_Equator_Twilight = new Color(
                (calc_EquatorTwilight - calc_tempOffset_Equator - tint_Twilight) * calc_tempMultR,
                (calc_EquatorTwilight + calc_tintOffset_Ambient - (0.1f * tint_Twilight)) * calc_tintMult,
                (calc_EquatorTwilight + calc_tempOffset_Equator + tint_Twilight) * calc_tempMultB,
                1f);

            Color color_Ground_Twilight = new Color(
                (calc_GroundTwilight - calc_tempOffset_Ground - tint_Twilight) * calc_tempMultR,
                (calc_GroundTwilight + calc_tintOffset_Ambient - (0.1f * tint_Twilight)) * calc_tintMult,
                (calc_GroundTwilight + calc_tempOffset_Ground + tint_Twilight) * calc_tempMultB,
                1f);

            // Updating the colors for the gradients.
            GradientAlphaKey[] gradientAlphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

            GradientColorKey[] gradientColorKeys_Direct = new GradientColorKey[] {
                new GradientColorKey(color_Direct_Night, time_Sunrise1),
                new GradientColorKey(color_Direct_Twilight, time_Sunrise2),
                new GradientColorKey(color_Direct_Day, time_Early),
                new GradientColorKey(color_Direct_Day, time_Late),
                new GradientColorKey(color_Direct_Twilight, time_Sunset1),
                new GradientColorKey(color_Direct_Night, time_Sunset2), };
            gradient_Direct.SetKeys(gradientColorKeys_Direct, gradientAlphaKeys);

            GradientColorKey[] gradientColorKeys_Sky = new GradientColorKey[] {
                new GradientColorKey(color_Sky_Night, time_Sunrise1),
                new GradientColorKey(color_Sky_Twilight, time_Sunrise2),
                new GradientColorKey(color_Sky_Day, time_Early),
                new GradientColorKey(color_Sky_Day, time_Late),
                new GradientColorKey(color_Sky_Twilight, time_Sunset1),
                new GradientColorKey(color_Sky_Night, time_Sunset2), };
            gradient_Sky.SetKeys(gradientColorKeys_Sky, gradientAlphaKeys);

            GradientColorKey[] gradientColorKeys_Equator = new GradientColorKey[] {
                new GradientColorKey(color_Equator_Night, time_Sunrise1),
                new GradientColorKey(color_Equator_Twilight, time_Sunrise2),
                new GradientColorKey(color_Equator_Day, time_Early),
                new GradientColorKey(color_Equator_Day, time_Late),
                new GradientColorKey(color_Equator_Twilight, time_Sunset1),
                new GradientColorKey(color_Equator_Night, time_Sunset2), };
            gradient_Equator.SetKeys(gradientColorKeys_Equator, gradientAlphaKeys);

            GradientColorKey[] gradientColorKeys_Ground = new GradientColorKey[] {
                new GradientColorKey(color_Ground_Night, time_Sunrise1),
                new GradientColorKey(color_Ground_Twilight, time_Sunrise2),
                new GradientColorKey(color_Ground_Day, time_Early),
                new GradientColorKey(color_Ground_Day, time_Late),
                new GradientColorKey(color_Ground_Twilight, time_Sunset1),
                new GradientColorKey(color_Ground_Night, time_Sunset2), };
            gradient_Ground.SetKeys(gradientColorKeys_Ground, gradientAlphaKeys);

            // Assigning the new color gradients.
            try
            {
                UnityEngine.Object.FindObjectOfType<DayNightProperties>().m_LightColor = gradient_Direct;
                var ambient = typeof(DayNightProperties.AmbientColor);
                ambient.GetField("m_SkyColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(DayNightProperties.instance.m_AmbientColor, gradient_Sky);
                ambient.GetField("m_EquatorColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(DayNightProperties.instance.m_AmbientColor, gradient_Equator);
                ambient.GetField("m_GroundColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(DayNightProperties.instance.m_AmbientColor, gradient_Ground);
            }
            catch
            {
                UnityEngine.Debug.Log("Lumina error");
                if (UnityEngine.Object.FindObjectOfType<DayNightProperties>() == null)
                {
                    UnityEngine.Debug.Log("DayNightProperties null");
                }

            }

        }

        public static void CalculateTonemapping(float Luminosity, float gamma, float contrast)
        {
            try
            {
                var calc_Contrast = contrast * 1.4f;
                var toneMap = GameObject.Find("Main Camera").GetComponent<ColossalFramework.ToneMapping>();
                toneMap.m_Luminance = 0.10f + (calc_Contrast * 0.013f);
                toneMap.m_ToneMappingBoostFactor = (1.00f + (0.5f * Luminosity)) / (float)Math.Pow(tweak_Intensity, 2.2f);
                toneMap.m_ToneMappingGamma = 2.60f * (((gamma + 1) / 4) + 0.75f);
                toneMap.m_ToneMappingParamsFilmic.A = 0.50f + (calc_Contrast * 0.13f);
                toneMap.m_ToneMappingParamsFilmic.B = 0.25f - (calc_Contrast * 0.08f);
                toneMap.m_ToneMappingParamsFilmic.C = 0.10f - (calc_Contrast * 0.005f);
                toneMap.m_ToneMappingParamsFilmic.D = 0.70f + (calc_Contrast * 0.13f);
                toneMap.m_ToneMappingParamsFilmic.E = 0.01f;
                toneMap.m_ToneMappingParamsFilmic.F = 0.25f - (calc_Contrast * 0.11f);
                toneMap.m_ToneMappingParamsFilmic.W = 11.20f + calc_Contrast;
            }
            catch
            {
                UnityEngine.Debug.Log("Lumina error");
                if (GameObject.Find("Main Camera")?.GetComponent<ColossalFramework.ToneMapping>() == null)
                {
                    UnityEngine.Debug.Log("ToneMapping null");
                }
            }
        }

        public static void SkyTonemapping(bool enable)
        {
            var daynight = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            daynight.m_Tonemapping = enable;
        }
        #endregion
    }
}
