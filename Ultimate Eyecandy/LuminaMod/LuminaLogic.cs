namespace Lumina
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using UnifiedUI.Helpers;
    using UnityEngine;

    /// <summary>
    /// Lumina logic class.
    /// </summary>
    internal sealed class LuminaLogic
    {
        // Shadow values.
        private static bool s_disableSmoothing;
        private static float s_shadowIntensity = 1f;

        // UUI button.
        internal UUICustomButton _uuiButton;

        // Instance reference.
        private static LuminaLogic s_instance;

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static LuminaLogic Instance => s_instance;

        /// <summary>
        /// Gets the UUI button reference.
        /// </summary>
        internal UUICustomButton UUIButton => _uuiButton;

        /// <summary>
        /// Gets or sets the a value indicating whether shadow smoothing is disabled (<c>true</c>) or enabled (<c>false</c>).
        /// </summary>
        internal static bool DisableSmoothing
        {
            get => s_disableSmoothing;

            set
            {
                s_disableSmoothing = value;

                // Update value if live.
                s_instance?.ApplyShadowSmoothing();
            }
        }

        /// <summary>
        /// Gets or sets shadow intensity.
        /// </summary>
        internal static float ShadowIntensity
        {
            get => s_shadowIntensity;

            set
            {
                s_shadowIntensity = Mathf.Clamp(value, 0f, 1f);

                // Update value if live.
                s_instance?.UpdateShadowSettings();
            }
        }

        /// <summary>
        /// Sets loaded fog data (from configuration file).
        /// </summary>
        internal static FogData LoadedFogData { private get; set; }

        /// <summary>
        /// Gets or sets the name of the selected daytime CubeMap.
        /// </summary>
        internal static string DayCubeMap { get; set; } = null;

        /// <summary>
        /// Gets or sets the name of the selected nighttime CubeMap.
        /// </summary>
        internal static string NightCubeMap { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether fog effects are enabled.
        /// </summary>
        internal static bool FogEffectEnabled
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.FogEffectEnabled;
                }

                // Otherwise use active settings.
                FogEffect fogEffect = UnityEngine.Object.FindObjectOfType<FogEffect>();
                return fogEffect != null && fogEffect.enabled;
            }

            set
            {
                FogEffect fogEffect = UnityEngine.Object.FindObjectOfType<FogEffect>();
                if (fogEffect != null)
                {
                    fogEffect.enabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether classic fog effects are enabled.
        /// </summary>
        internal static bool ClassicFogEnabled
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.FogEffectEnabled;
                }

                // Otherwise use active settings.
                FogEffect fog = UnityEngine.Object.FindObjectOfType<FogEffect>();
                return fog != null && fog.enabled;
            }

            set
            {
                FogEffect fog = UnityEngine.Object.FindObjectOfType<FogEffect>();
                if (fog != null)
                {
                    fog.enabled = value;
                    // Optionally, synchronize EdgeFogEnabled with ClassicFogEnabled when setting ClassicFogEnabled.
                    EdgeFogEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets fog intensity.
        /// </summary>
        internal static float FogIntensity
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.FogIntensity;
                }

                // Otherwise use active settings.
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                return fogProperties != null ? fogProperties.m_FogDensity : 0f;
            }

            set
            {
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                if (fogProperties != null)
                {
                    fogProperties.m_FogDensity = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets fog height.
        /// </summary>
        internal static float FogHeight
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.FogHeight;
                }

                // Otherwise use active settings.
                RenderProperties fogProperties = UnityEngine.Object.FindObjectOfType<RenderProperties>();
                return (fogProperties != null && fogProperties.enabled) ? fogProperties.m_fogHeight : 5000f;
            }

            set
            {
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                if (fogProperties != null)
                {
                    fogProperties.m_FogHeight = Mathf.Clamp(value, 0f, 5000f);
                }
            }
        }

        public static float FogDistance
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.FogDistance;
                }

                // Otherwise use active settings.
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                return (fogProperties != null && fogProperties.enabled) ? fogProperties.m_FogDistance : 10f;
            }

            set
            {
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                if (fogProperties != null)
                {
                    fogProperties.m_FogDistance = Mathf.Clamp(value, -10f, 20000f);
                }
            }
        }

        /// <summary>
        /// Gets or sets fog color decay.
        /// </summary>
        internal static float ColorDecay
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.ColorDecay;
                }

                // Otherwise use active settings.
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                return (fogProperties != null && fogProperties.enabled) ? fogProperties.m_ColorDecay : 0.0f;
            }

            set
            {
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                if (fogProperties != null)
                {
                    fogProperties.m_ColorDecay = Mathf.Clamp(value, 0f, 1f); ;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether edge fog effects are enabled.
        /// </summary>
        internal static bool EdgeFogEnabled
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.EdgeFogEnabled;
                }

                // Otherwise use active settings.
                FogEffect fog = UnityEngine.Object.FindObjectOfType<FogEffect>();
                return fog != null && fog.m_edgeFog;
            }

            set
            {
                FogEffect fog = UnityEngine.Object.FindObjectOfType<FogEffect>();
                if (fog != null)
                {
                    fog.m_edgeFog = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets edge fog distance.
        /// </summary>
        public static float EdgeFogDistance
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.EdgeFogDistance;
                }

                // Otherwise use active settings.
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                return (fogProperties != null && fogProperties.enabled) ? fogProperties.m_EdgeFogDistance : 0.0f;
            }

            set
            {
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                if (fogProperties != null)
                {
                    fogProperties.m_EdgeFogDistance = Mathf.Clamp(value, 0f, 2800f);
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizon height.
        /// </summary>
        internal static float HorizonHeight
        {
            get
            {
                // Use loaded data if available.
                if (LoadedFogData != null && LoadedFogData.IsValid)
                {
                    return LoadedFogData.HorizonHeight;
                }

                // Otherwise use active settings.
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                return (fogProperties != null && fogProperties.enabled) ? fogProperties.m_HorizonHeight : 800f;
            }

            set
            {
                FogProperties fogProperties = UnityEngine.Object.FindObjectOfType<FogProperties>();
                if (fogProperties != null)
                {
                    fogProperties.m_HorizonHeight = Mathf.Clamp(value, 0f, 5000f); ;
                }
            }
        }

        /// <summary>
        /// Sets the volume fog distance.
        /// </summary>
        internal static float VolumeFogDistance
        {
            private get
            {
                RenderProperties renderProperties = UnityEngine.Object.FindObjectOfType<RenderProperties>();
                return (renderProperties != null && renderProperties.enabled) ? renderProperties.m_volumeFogDistance : 10f;
            }
            set
            {
                RenderProperties renderProperties = UnityEngine.Object.FindObjectOfType<RenderProperties>();
                if (renderProperties != null)
                {
                    renderProperties.m_volumeFogDistance = Mathf.Clamp(value, -10f, 4800f);
                }
            }
        }

        /// <summary>
        /// Sets the 3D fog distance.
        /// </summary>
        internal static float ThreeDFogDistance
        {
            private get
            {
                FogEffect fogEffect = UnityEngine.Object.FindObjectOfType<FogEffect>();
                return (fogEffect != null && fogEffect.enabled) ? fogEffect.m_3DFogDistance : 8f;
            }

            set
            {
                FogEffect fogEffect = UnityEngine.Object.FindObjectOfType<FogEffect>();
                if (fogEffect != null)
                {
                    fogEffect.m_3DFogDistance = Mathf.Clamp(value, -10f, 10f);
                }
            }
        }

        /// <summary>
        /// Loads current settings and ensure an active instance.
        /// Must be called prior to use.
        /// </summary>
        internal static void OnLoad()
        {
            // Set instance reference.
            if (s_instance == null)
            {
                s_instance = new LuminaLogic();
            }

            // Add UUI button.
            s_instance._uuiButton = UUIHelpers.RegisterCustomButton(
                name: LuminaMod.Instance.Name,
                groupName: null, // default group
                tooltip: Translations.Translate("MOD_NAME"),
                icon: UUIHelpers.LoadTexture(UUIHelpers.GetFullPath<LuminaMod>("Resources", "UUI.png")),
                onToggle: (value) =>
                {
                    if (value)
                    {
                        StandalonePanelManager<LuminaPanel>.Create();
                    }
                    else
                    {
                        StandalonePanelManager<LuminaPanel>.Panel?.Close();
                    }
                },
                hotkeys: new UUIHotKeys { ActivationKey = ModSettings.ToggleKey });

            // Apply settings.
            StyleManager.ApplySettings();
            s_instance.ApplyShadowSmoothing();
            s_instance.UpdateShadowSettings();

            // Apply any loaded fog values and then clear reference.
            ApplyFogValues();
            LoadedFogData = null;
        }

        /// <summary>
        /// Destroys the active instance.
        /// </summary>
        internal static void Destroy() => s_instance = null;

        /// <summary>
        /// Resets the UUI button to the non-pressed state.
        /// </summary>
        internal void ResetButton() => _uuiButton.IsPressed = false;

        /// <summary>
        /// Updates shadow settings.
        /// </summary>
        private void UpdateShadowSettings()
        {
            if (GameObject.Find("Directional Light")?.GetComponent<Light>() is Light directionalLight)
            {
                directionalLight.shadowStrength = s_shadowIntensity;
            }
            else
            {
                Logging.Error("couldn't find directional light");
            }
        }

        public static void CalculateAll(float[] values, bool skyTonemapping)
        {
            CalculateLighting(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9]);
            CalculateTonemapping(values[10], values[11], values[12]);
            SkyTonemapping(skyTonemapping);
        }

        public void ApplyShadowSmoothing()
        {
            if (s_disableSmoothing)
                QualitySettings.shadows = ShadowQuality.HardOnly;
            else
                QualitySettings.shadows = ShadowQuality.All;
        }


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
            catch (Exception e)
            {
                Logging.LogException(e, "exception calculating tonemapping");
            }
        }

        public static void SkyTonemapping(bool enable)
        {
            var daynight = UnityEngine.Object.FindObjectOfType<DayNightProperties>();
            daynight.m_Tonemapping = enable;
        }


        /// <summary>
        /// Applies previously-loaded fog values.
        /// </summary>
        private static void ApplyFogValues()
        {
            // Only apply if data is loaded and valid.
            if (LoadedFogData != null && LoadedFogData.IsValid)
            {
                FogEffectEnabled = LoadedFogData.FogEffectEnabled;
                ClassicFogEnabled = LoadedFogData.ClassicFogEnabled;
                FogIntensity = LoadedFogData.FogIntensity;
                FogHeight = LoadedFogData.FogHeight;
                FogDistance = LoadedFogData.FogDistance;
                ColorDecay = LoadedFogData.ColorDecay;
                EdgeFogEnabled = LoadedFogData.EdgeFogEnabled;
                EdgeFogDistance = LoadedFogData.EdgeFogDistance;
                HorizonHeight = LoadedFogData.HorizonHeight;
            }
        }
    }
}
