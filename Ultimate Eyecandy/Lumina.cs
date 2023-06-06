using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.IO;
using HarmonyLib;
using System.Diagnostics;


namespace Lumina
{
    public class LuminaMod : LoadingExtensionBase, IUserMod
    {
        private GameObject gameobj;
        private static LuminaLogic logic;

        public string Name { get { return "Lumina"; } }

        public string Description { get { return "Optimized and adaptable illumination and tone adjustment."; } }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            OnEnabled();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (gameobj != null)
            {
                UnityEngine.Object.Destroy(gameobj);
                gameobj = null;
                logic = null;
            }

        }

        private Harmony harmony;

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("Lumina");
            group.AddButton("Launch LUT Editor", OpenLUTEditor);
        }

        private void OpenLUTEditor()
        {
            string lutEditorPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2983036781\LUT Editor\lut.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = lutEditorPath;
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }

        public void OnEnabled()
        {

            while (true)
            {
                var obj = GameObject.Find("Lumina");
                if (!obj) break;
                GameObject.DestroyImmediate(obj);
            }
            gameobj = new GameObject("Lumina");
            logic = gameobj.AddComponent<LuminaLogic>();

            harmony = new Harmony("com.nyoko.lumina.patch");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnDisabled()
        {
            while (true)
            {
                var obj = GameObject.Find("Lumina");
                if (!obj) break;

                GameObject.DestroyImmediate(obj);
            }
            harmony = null;
        }

        public class Raycaster : ToolBase
        {
            public static float RayDistance(float angle, Vector3 transform)
            {
                Vector3 direction = Quaternion.AngleAxis(angle, transform) * Camera.main.transform.forward;
                //Vector3 direction = Camera.main.transform.forward;
                ToolBase.RaycastInput input = new ToolBase.RaycastInput(new Ray(Camera.main.transform.position, direction), Camera.main.farClipPlane);
                input.m_ignoreBuildingFlags = Building.Flags.None;
                input.m_ignoreNodeFlags = NetNode.Flags.None;
                input.m_ignoreSegmentFlags = NetSegment.Flags.None;
                input.m_ignorePropFlags = PropInstance.Flags.None;
                input.m_buildingService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                input.m_netService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                input.m_netService2 = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                input.m_propService = new RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
                ToolBase.RaycastOutput rayOutput;
                if (ToolBase.RayCast(input, out rayOutput)) return Vector3.Distance(Camera.main.transform.position, rayOutput.m_hitPos);
                else return 1000f;
            }
        }

        public static float GetShadowBias()
        {
            var rayDistance = Raycaster.RayDistance(0, Camera.main.transform.forward);
            var rayDistanceMax = rayDistance;

            rayDistance = Raycaster.RayDistance(10, Camera.main.transform.right);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(-10, Camera.main.transform.right);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(10, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(20, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(-10, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistance = Raycaster.RayDistance(-20, Camera.main.transform.up);
            if (rayDistance > rayDistanceMax) rayDistanceMax = rayDistance;
            rayDistanceMax = Mathf.Clamp(rayDistanceMax, 0f, 1000f);
            if (rayDistanceMax == 0) rayDistanceMax = 1000;

            var heightMultiplier = 0.6f; // multiplier - how much height affects bias
            var heightUpper = 10f; // divider - affects height above which shadow bias is no longer relevant
            var heightLower = 20f; // subtracter - affects height below which shadow bias is no longer relevant
            var heightAngleRatio = 0.65f; // multiplier (0-1) - higher values means angle affects shadow bias more at increased height
            var angleBaseAffect = 0.15f; // lower limit for how much angle affects bias
            var rayAffect = 16; // divider (1/value) - how much raycasting is mixed into the height value
            var cameraController = ToolsModifierControl.cameraController;

            // calculates relevant height by subtracting terrain height from camera height
            var height = cameraController.transform.position.y - cameraController.m_currentHeight;
            if (rayDistanceMax > height) height = ((rayAffect - 1) * height + rayDistanceMax) / rayAffect;

            // calculates camera angle: 0 = at horizon, 90 = down
            var angle = cameraController.m_targetAngle.y; if (angle > 90f || angle < 0f) angle = 0;

            // calculates relevant angle, where we're looking: 0 = down, 1 = at horizon
            var calc_angle = 1 - (angle / 90f);

            // calculates relevant height: 0 = close to ground, 1 = very high
            var calc_height = Mathf.Clamp(((height - heightLower) * heightMultiplier / heightUpper), 0f, 100f) / 100;

            // calculates bias, increasing height and angle increases leads to higher values
            var calc_bias = calc_height + (((calc_height * heightAngleRatio) + angleBaseAffect) * calc_angle);

            // makes bias increase more quickly when going above 0.2
            var exp_bias = Math.Pow((calc_bias + 0.8f), 1.5f) - 0.8f;

            // clamps bias to lowest and highest useful values
            var final_bias = Mathf.Clamp(Convert.ToSingle(exp_bias), 0.1f, 1f);

            if (LuminaLogic.forceLowBias) return Mathf.Clamp(final_bias, 0.20f, 1f) - 0.19f;
            return final_bias;
        }
    }

    public class LuminaLogic : MonoBehaviour
    {
        // SIMON PART (UI + SAVING)

        public bool ShowUI = false;
        private Rect windowRect = new Rect(200, 200, 450, 581);
        private int instanceID, windowMode = 0;
        private Vector2 scrollPresets = Vector2.zero;
        private string nameTextfield = "Preset Name";

        // lighting values
        public float[] lightingValues, oldLightingValues;
        public bool skyTonemappingUi, oldSkyTonemappingUi;

        // shadows values
        public bool disableSmoothing, oldDisableSmoothing;
        public static bool forceLowBias, oldForceLowBias;

        public string CachePath = DataLocation.localApplicationData + "\\LuminaCache.light";

        void Start()
        {
            instanceID = this.GetInstanceID();
            LightPreset.LoadLightPresets();

            if (File.Exists(CachePath))
            {
                LoadCache();
                CalculateAll(lightingValues, skyTonemappingUi);
                ApplyShadowValues();
            }
            else
                AllToZero();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        ShowUI = !ShowUI;
                    }
                }
            }

            if (ShowUI)
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

        void OnGUI()
        {
            if (ShowUI)
                windowRect = GUI.Window(instanceID, windowRect, DrawWindow, "Lumina");
        }

        void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 420, 20));
            if (GUI.Button(new Rect(422, 4, 25, 20), "x"))
                ShowUI = false;

            windowMode = GUI.Toolbar(new Rect(5, 26, 440, 25), windowMode, new string[] { "Lightness", "Presets", "Color Correction" });

            if (windowMode == 0)
            {
                // CUSTOMIZATION WINDOW MODE

                GUI.Label(new Rect(180, 60, 150, 26), "<size=14>Exposure Control</size>");
                // Luminosity
                GUI.Label(new Rect(5, 80, 115, 30), "Luminosity");
                lightingValues[10] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 85, 270, 25), lightingValues[10], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 80, 50, 30), lightingValues[10].ToString(), GUI.skin.label))
                    lightingValues[10] = 0;
                // gamma
                GUI.Label(new Rect(5, 110, 115, 30), "Gamma");
                lightingValues[11] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 115, 270, 25), lightingValues[11], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 110, 50, 30), lightingValues[11].ToString(), GUI.skin.label))
                    lightingValues[11] = 0;
                // contrast
                GUI.Label(new Rect(5, 140, 115, 30), "Radiance");
                lightingValues[12] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 145, 270, 25), lightingValues[12], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 140, 50, 30), lightingValues[12].ToString(), GUI.skin.label))
                    lightingValues[12] = 0;

                GUI.Label(new Rect(200, 160, 150, 26), "<size=14>Lighting</size>");
                // temperature
                GUI.Label(new Rect(5, 190, 115, 30), "Hue");
                lightingValues[0] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 195, 270, 25), lightingValues[0], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 190, 50, 30), lightingValues[0].ToString(), GUI.skin.label))
                    lightingValues[0] = 0;
                // tint
                GUI.Label(new Rect(5, 220, 115, 30), "Tint");
                lightingValues[1] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 225, 270, 25), lightingValues[1], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 220, 50, 30), lightingValues[1].ToString(), GUI.skin.label))
                    lightingValues[1] = 0;
                // sun temperature
                GUI.Label(new Rect(5, 250, 115, 30), "Sun Temperature");
                lightingValues[2] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 255, 270, 25), lightingValues[2], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 250, 50, 30), lightingValues[2].ToString(), GUI.skin.label))
                    lightingValues[2] = 0;
                // sun tint
                GUI.Label(new Rect(5, 280, 115, 30), "Sun Tint");
                lightingValues[3] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 285, 270, 25), lightingValues[3], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 280, 50, 30), lightingValues[3].ToString(), GUI.skin.label))
                    lightingValues[3] = 0;
                // sky temperature
                GUI.Label(new Rect(5, 310, 115, 30), "Sky Temperature");
                lightingValues[4] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 315, 270, 25), lightingValues[4], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 310, 50, 30), lightingValues[4].ToString(), GUI.skin.label))
                    lightingValues[4] = 0;
                // sky tint
                GUI.Label(new Rect(5, 340, 115, 30), "Sky Tint");
                lightingValues[5] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 345, 270, 25), lightingValues[5], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 340, 50, 30), lightingValues[5].ToString(), GUI.skin.label))
                    lightingValues[5] = 0;
                // moon  temperature
                GUI.Label(new Rect(5, 370, 115, 30), "Moon Temperature");
                lightingValues[6] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 375, 270, 25), lightingValues[6], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 370, 50, 30), lightingValues[6].ToString(), GUI.skin.label))
                    lightingValues[6] = 0;
                // moon tint
                GUI.Label(new Rect(5, 400, 115, 30), "Moon Tint");
                lightingValues[7] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 405, 270, 25), lightingValues[7], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 400, 50, 30), lightingValues[7].ToString(), GUI.skin.label))
                    lightingValues[7] = 0;
                // moonlight
                GUI.Label(new Rect(5, 430, 115, 30), "Moon Light");
                lightingValues[8] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 435, 270, 25), lightingValues[8], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 430, 50, 30), lightingValues[8].ToString(), GUI.skin.label))
                    lightingValues[8] = 0;
                // Twilight tint
                GUI.Label(new Rect(5, 460, 115, 30), "Twilight Tint");
                lightingValues[9] = (float)(Math.Round(GUI.HorizontalSlider(new Rect(120, 465, 270, 25), lightingValues[9], -1f, 1f) * 20) / 20);
                if (GUI.Button(new Rect(395, 460, 50, 30), lightingValues[9].ToString(), GUI.skin.label))
                    lightingValues[9] = 0;

                skyTonemappingUi = GUI.Toggle(new Rect(5, 495, 348, 28), skyTonemappingUi, " Enable Sky Tonemapping");
                disableSmoothing = GUI.Toggle(new Rect(5, 523, 348, 28), disableSmoothing, " Disable Shadow Smoothing");
                forceLowBias = GUI.Toggle(new Rect(5, 551, 348, 28), forceLowBias, " Enforce Minimal Shadow Offset");

                if (GUI.Button(new Rect(355, 516, 88, 28), "Reset All"))
                    AllToZero();
            }
            else if (windowMode == 1)
            {
                // PRESETS WINDOW MODE 

                if (LightPreset.LightPresets.Count > 0)
                {
                    scrollPresets = GUI.BeginScrollView(new Rect(5, 55, 440, 200), scrollPresets, new Rect(0, 0, 415, LightPreset.LightPresets.Count * 30));
                    for (int i = 0; i < LightPreset.LightPresets.Count; i++)
                    {
                        var preset = LightPreset.LightPresets[i];
                        GUI.Label(new Rect(5, i * 30 + 3, 240, 25), preset.m_presetName);
                        if (GUI.Button(new Rect(245, i * 30, 70, 28), "Load"))
                            LoadPreset(preset);
                        if (preset.m_canBeDeleted)
                        {
                            if (GUI.Button(new Rect(320, i * 30, 70, 28), "Delete"))
                                LightPreset.DeletePreset(preset);
                        }
                        else
                            GUI.Label(new Rect(320, i * 30 + 2, 70, 28), "[ <i>Workshop</i> ]");
                    }
                    GUI.EndScrollView();
                }
                else
                {
                    GUI.Box(new Rect(5, 55, 440, 200), string.Empty);
                    GUI.Label(new Rect(28, 60, 410, 30), "No preset found !");
                }
                if (GUI.Button(new Rect(5, 260, 440, 27), "Reload Presets List"))
                    LightPreset.LoadLightPresets();

                if (File.Exists(DataLocation.localApplicationData + @"\ModConfig\LuminaPresets\" + LightPreset.ToFileName(nameTextfield) + ".light"))
                {
                    GUI.color = Color.red;
                    nameTextfield = GUI.TextField(new Rect(5, 290, 440, 27), nameTextfield);
                    GUI.Label(new Rect(5, 320, 440, 27), "<b>A preset with this name already exists !</b>", GUI.skin.button);
                    GUI.color = Color.white;
                }
                else
                {
                    nameTextfield = GUI.TextField(new Rect(5, 290, 440, 27), nameTextfield);
                    if (GUI.Button(new Rect(5, 320, 440, 27), "<b>+</b> Save Preset Locally"))
                        CreatePreset();
                }
                if (GUI.Button(new Rect(5, 350, 440, 27), "Open Local Presets Folder"))
                {
                    string path = DataLocation.localApplicationData + @"\ModConfig\LuminaPresets\";
                    if (!Directory.Exists(DataLocation.localApplicationData + @"\ModConfig\"))
                        Directory.CreateDirectory(DataLocation.localApplicationData + @"\ModConfig\");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    Application.OpenURL("file://" + path);
                }
                else if (windowMode == 2)
                {
                    // CUSTOMIZATION WINDOW MODE - Color Correction

                    GUI.Label(new Rect(180, 60, 150, 26), "<size=14>Color Correction</size>");

                    // Add the button below the label
                    if (GUI.Button(new Rect(180, 90, 150, 30), "LUT Editor"))
                    {
                        string lutEditorPath = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\255710\2983036781\LUT Editor\lut.exe";
                        Process.Start(lutEditorPath);
                    }
                }


            }
        }

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
        public void LoadPreset(LightPreset preset)
        {
            lightingValues = new float[]
            {
                preset.m_values[0], preset.m_values[1], preset.m_values[2], preset.m_values[3], preset.m_values[4], preset.m_values[5], preset.m_values[6], preset.m_values[7], preset.m_values[8], preset.m_values[9], preset.m_values[10], preset.m_values[11], preset.m_values[12]
            };
            oldLightingValues = new float[]
            {
                preset.m_values[0], preset.m_values[1], preset.m_values[2], preset.m_values[3], preset.m_values[4], preset.m_values[5], preset.m_values[6], preset.m_values[7], preset.m_values[8], preset.m_values[9], preset.m_values[10], preset.m_values[11], preset.m_values[12]
            };
            skyTonemappingUi = preset.m_skyTonemapping;
            oldSkyTonemappingUi = preset.m_skyTonemapping;
            CalculateAll(preset.m_values, preset.m_skyTonemapping);
            SaveCache();
        }
        public void CreatePreset()
        {
            LightPreset preset = new LightPreset();
            preset.m_presetName = nameTextfield;
            preset.m_values = new float[]
            {
                lightingValues[0], lightingValues[1], lightingValues[2], lightingValues[3], lightingValues[4], lightingValues[5], lightingValues[6], lightingValues[7], lightingValues[8], lightingValues[9], lightingValues[10], lightingValues[11], lightingValues[12]
            };
            preset.m_canBeDeleted = true;
            preset.m_skyTonemapping = skyTonemappingUi;
            preset.SavePreset(true);
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

        #region nyoko PART (LOGIC)

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
            UnityEngine.Object.FindObjectOfType<DayNightProperties>().m_LightColor = gradient_Direct;
            var ambient = typeof(DayNightProperties.AmbientColor);
            ambient.GetField("m_SkyColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(DayNightProperties.instance.m_AmbientColor, gradient_Sky);
            ambient.GetField("m_EquatorColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(DayNightProperties.instance.m_AmbientColor, gradient_Equator);
            ambient.GetField("m_GroundColor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(DayNightProperties.instance.m_AmbientColor, gradient_Ground);

        }

        public static void CalculateTonemapping(float Luminosity, float gamma, float contrast)
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

        public static void SkyTonemapping(bool enable)
        {
            var daynight = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            daynight.m_Tonemapping = enable;
        }
        #endregion
    }


    [HarmonyPatch(typeof(DayNightProperties))]
    [HarmonyPatch("UpdateLighting")]
    class UpdateLighting
    {
        static void Postfix()
        {
            RenderManager.instance.MainLight.shadowBias = LuminaMod.GetShadowBias();
        }
    }
}
public class LightPreset
{
    public LightPreset() { }

    public float[] m_values;
    public bool m_skyTonemapping, m_canBeDeleted;
    public string m_presetName, m_filePath;

    public void SavePreset(bool addToPresetsList)
    {
        string path = DataLocation.localApplicationData + @"\ModConfig\LuminaPresets\";
        if (!Directory.Exists(DataLocation.localApplicationData + @"\ModConfig\"))
            Directory.CreateDirectory(DataLocation.localApplicationData + @"\ModConfig\");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        this.m_canBeDeleted = true;
        this.m_filePath = path + ToFileName(this.m_presetName) + ".light";

        TextWriter tw = new StreamWriter(this.m_filePath);
        tw.WriteLine("name = " + ((this.m_presetName == "") ? "[none]" : this.m_presetName));
        tw.WriteLine("0 = " + this.m_values[0]);
        tw.WriteLine("1 = " + this.m_values[1]);
        tw.WriteLine("2 = " + this.m_values[2]);
        tw.WriteLine("3 = " + this.m_values[3]);
        tw.WriteLine("4 = " + this.m_values[4]);
        tw.WriteLine("5 = " + this.m_values[5]);
        tw.WriteLine("6 = " + this.m_values[6]);
        tw.WriteLine("7 = " + this.m_values[7]);
        tw.WriteLine("8 = " + this.m_values[8]);
        tw.WriteLine("9 = " + this.m_values[9]);
        tw.WriteLine("10 = " + this.m_values[10]);
        tw.WriteLine("11 = " + this.m_values[11]);
        tw.WriteLine("12 = " + this.m_values[12]);
        tw.WriteLine("skyTmpg = " + this.m_skyTonemapping);
        tw.Close();
        if (addToPresetsList)
            LightPresets.Add(this);
    }

    public static void LoadLightPresets()
    {
        LightPresets = new List<LightPreset>();
        string path = DataLocation.localApplicationData + @"\ModConfig\LuminaPresets\";
        if (!Directory.Exists(DataLocation.localApplicationData + @"\ModConfig\"))
            Directory.CreateDirectory(DataLocation.localApplicationData + @"\ModConfig\");
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        foreach (string file in Directory.GetFiles(path, "*.light", SearchOption.AllDirectories))
        {
            var preset = new LightPreset();
            preset.m_presetName = "[none]";
            preset.m_canBeDeleted = true;
            preset.m_filePath = file;
            var dict = new Dictionary<int, float>();

            foreach (string line in File.ReadAllLines(file).Where(s => s.Contains(" = ")))
            {
                string[] data = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                if (data[0] == "skyTmpg")
                {
                    preset.m_skyTonemapping = bool.Parse(data[1]);
                }
                else if (data[0] == "name")
                {
                    preset.m_presetName = data[1];
                }
                else
                {
                    var id = int.Parse(data[0]);
                    dict[id] = 0;
                    dict[id] = float.Parse(data[1]);
                }
            }
            preset.m_values = new float[]
            {
                dict[0], dict[1], dict[2], dict[3], dict[4], dict[5], dict[6], dict[7], dict[8], dict[9], dict[10], dict[11], dict[12]
            };
            LightPresets.Add(preset);
        }

        foreach (PublishedFileId fileId in PlatformService.workshop.GetSubscribedItems())
        {
            string fileDir = PlatformService.workshop.GetSubscribedItemPath(fileId);
            if (fileDir == null)
                continue;
            if (fileDir == "")
                continue;
            if (!Directory.Exists(fileDir))
                continue;
            var files = Directory.GetFiles(fileDir, "*.light", SearchOption.AllDirectories);
            if (files.Any())
            {
                foreach (string file in files)
                {
                    var preset = new LightPreset();
                    preset.m_presetName = "[none]";
                    preset.m_canBeDeleted = false;
                    var dict = new Dictionary<int, float>();

                    foreach (string line in File.ReadAllLines(file).Where(s => s.Contains(" = ")))
                    {
                        string[] data = line.Split(new string[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                        if (data[0] == "skyTmpg")
                        {
                            preset.m_skyTonemapping = bool.Parse(data[1]);
                        }
                        else if (data[0] == "name")
                        {
                            preset.m_presetName = data[1];
                        }
                        else
                        {
                            var id = int.Parse(data[0]);
                            dict[id] = 0;
                            dict[id] = float.Parse(data[1]);
                        }
                    }
                    preset.m_values = new float[]
                    {
                        dict[0], dict[1], dict[2], dict[3], dict[4], dict[5], dict[6], dict[7], dict[8], dict[9], dict[10], dict[11], dict[12]
                    };
                    LightPresets.Add(preset);
                }
            }
        }

    }
    public static void DeletePreset(LightPreset preset)
    {
        if (preset.m_canBeDeleted)
        {
            if (File.Exists(preset.m_filePath))
                File.Delete(preset.m_filePath);
            if (LightPresets.Contains(preset))
                LightPresets.Remove(preset);
        }
    }
    public static List<LightPreset> LightPresets;
    public static string ToFileName(string s)
    {
        return s.Replace(" ", "_").Replace(@"\", "").Replace("/", "").Replace("|", "").Replace("<", "").Replace(">", "").Replace("*", "").Replace(":", "").Replace("?", "").Replace("\"", "");
    }
}