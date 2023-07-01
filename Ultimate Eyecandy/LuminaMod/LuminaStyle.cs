namespace Lumina
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using ColossalFramework.Plugins;
    using UnityEngine;

    /// <summary>
    /// Handling of styles.
    /// </summary>
    public sealed class LuminaStyle
    {
        /// <summary>
        /// Gets or sets the style's name.
        /// </summary>
        public string StyleName { get; set; }

        /// <summary>
        /// Gets the lighting value array for this style.
        /// </summary>
        public float[] LightValues { get; } = new float[(int)ValueIndex.NumSettings];

        /// <summary>
        /// Gets a value indicating whether this is a local style file.
        /// </summary>
        public bool IsLocal { get; private set; }

        /// <summary>
        /// Gets or sets the style's directory filepath.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sky tonemapping is enabled.
        /// </summary>
        public bool EnableSkyTonemapping { get; set; }

        /// <summary>
        /// Gets a filesystem-safe representation of the style name.
        /// </summary>
        public string SanitizedFileName => Regex.Replace(StyleName, @"(@|&|'|\(|\)|<|>|#|""|\*|\s|:|\?|\||\$|%)", "_");

        /// <summary>
        /// Index of values within settings array.
        /// </summary>
        internal enum ValueIndex : int
        {
            Temperature = 0,
            Tint,
            SunTemp,
            SunTint,
            SkyTemp,
            SkyTint,
            MoonTemp,
            MoonTint,
            MoonLight,
            TwilightTint,
            Brightness,
            Gamma,
            Contrast,
            NumSettings,
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LuminaStyle"/> class using the currently active settings.
        /// </summary>
        /// <param name="name">Style name.</param>
        /// <param name="isLocal"><c>true</c> if this is a local file (and can be changed/deleted), <c>false</c> if readonly.</param>
        public LuminaStyle(string name, bool isLocal)
        {
            StyleName = name;
            IsLocal = isLocal;
            UpdateStyle();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="LuminaStyle"/> class using the provided settings.
        /// </summary>
        /// <param name="name">Style name.</param>
        /// <param name="temperature">Base light hue.</param>
        /// <param name="tint">Base light temperature.</param>
        /// <param name="sunTemp">Sun light temperature.</param>
        /// <param name="sunTint">Sun light tint.</param>
        /// <param name="skyTemp">Sky light temperatur.</param>
        /// <param name="skyTint">Sky light tint.</param>
        /// <param name="moonTemp">Moon light temperatur.</param>
        /// <param name="moonTint">Moon light tint.</param>
        /// <param name="moonLight">Moonlight.</param>
        /// <param name="twilightTint">Twilight tint.</param>
        /// <param name="brightness">Brightness.</param>
        /// <param name="gamma">Gamma.</param>
        /// <param name="contrast">Contrast.</param
        /// <param name="bool">Sky tonemapping enbled.</param>
        /// <param name="isLocal"><c>true</c> if this is a local file (and can be changed/deleted), <c>false</c> if readonly.</param>
        public LuminaStyle(
            string name,
            float temperature,
            float tint,
            float sunTemp,
            float sunTint,
            float skyTemp,
            float skyTint,
            float moonTemp,
            float moonTint,
            float moonLight,
            float twilightTint,
            float brightness,
            float gamma,
            float contrast,
            bool enableSkyTonemapping,
            bool isLocal)
        {
            // Populate values.
            StyleName = name;
            LightValues[(int)ValueIndex.Temperature] = temperature;
            LightValues[(int)ValueIndex.Tint] = tint;
            LightValues[(int)ValueIndex.SunTemp] = sunTemp;
            LightValues[(int)ValueIndex.SunTint] = sunTint;
            LightValues[(int)ValueIndex.SkyTemp] = skyTemp;
            LightValues[(int)ValueIndex.SkyTint] = skyTint;
            LightValues[(int)ValueIndex.MoonTemp] = moonTemp;
            LightValues[(int)ValueIndex.MoonTint] = moonTint;
            LightValues[(int)ValueIndex.MoonLight] = moonLight;
            LightValues[(int)ValueIndex.TwilightTint] = twilightTint;
            LightValues[(int)ValueIndex.Brightness] = brightness;
            LightValues[(int)ValueIndex.Gamma] = gamma;
            LightValues[(int)ValueIndex.Contrast] = contrast;
            EnableSkyTonemapping = enableSkyTonemapping;
            IsLocal = isLocal;
        }

        /// <summary>
        /// Applies this style's settings to the game.
        /// </summary>
        public void Apply()
        {
            LightValues.CopyTo(StyleManager.ActiveSettings, 0);
            StyleManager.EnableSkyTonemapping = EnableSkyTonemapping;
            StyleManager.ApplySettings();
        }

        /// <summary>
        /// Updates this <see cref="LuminaStyle"/> instance with the currently active settings.
        /// </summary>
        public void UpdateStyle()
        {
            StyleManager.ActiveSettings.CopyTo(LightValues, 0);
            EnableSkyTonemapping = StyleManager.EnableSkyTonemapping;
        }

        /// <summary>
        /// Saves the current settings as a mod.
        /// </summary>
        public void Save()
        {
            if (DirectoryPath == null)
            {
                // Determine filepath.
                string addonsPath = Path.Combine(DataLocation.localApplicationData, "Addons");
                string localModPath = Path.Combine(addonsPath, "Mods");

                // Sanitize name.
                string sanitizedName = SanitizedFileName;

                try
                {
                    // Ensure local mod directory exists.
                    if (!Directory.Exists(localModPath))
                    {
                        Directory.CreateDirectory(localModPath);
                    }

                    DirectoryPath = Path.Combine(localModPath, "Lumina_ " + sanitizedName);

                    if (!Directory.Exists(DirectoryPath))
                    {
                        Directory.CreateDirectory(DirectoryPath);
                    }
                    else
                    {
                        // Delete existing light files.
                        foreach (string file in Directory.GetFiles(DirectoryPath, "*.light"))
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception e)
                { 
                    Logging.LogException(e, "exception with directory handling when saving Lumina style ", StyleName);
                    return;
                }

                // Enforce local setting.
                IsLocal = true;

                // Write settings file.
                try
                {
                    using (TextWriter writer = new StreamWriter(Path.Combine(DirectoryPath, SanitizedFileName + ".light")))
                    {
                        writer.Write("name = ");
                        writer.WriteLine(StyleName);

                        // Serialize array values.
                        for (int i = 0; i < LightValues.Length; ++i)
                        {
                            writer.Write(i);
                            writer.Write(" = ");
                            writer.WriteLine(LightValues[i]);
                        }

                        writer.Write("skyTmpg = ");
                        writer.WriteLine(EnableSkyTonemapping);
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception saving lumina style", StyleName);
                    return;
                }

                // Create dummy mod if not on mac

                CreateSourceCode(sanitizedName);
            }
        }

        /// <summary>
        /// Creates dummy mod sourcecode for the style.
        /// </summary>
        /// <param name="sanitizedName">Sanitized style name.</param>
        public void CreateSourceCode(string sanitizedName)
        {
            StringBuilder sourceText = new StringBuilder();
            if (!Directory.Exists(DirectoryPath))
            {
                Logging.Error("attempting to create Lumina style mod whith invalid directory");
                return;
            }

            // Create source directory.
            string sourceDirectory = Path.Combine(DirectoryPath, "Source");
            if (!Directory.Exists(sourceDirectory))
            {
                try
                {
                    Directory.CreateDirectory(sourceDirectory);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception creating source directory ", sourceDirectory, " for Lumina style ", StyleName);
                    return;
                }
            }

            // Build source.
            sourceText.AppendLine("using ICities;");
            sourceText.AppendLine($"namespace {SanitizedFileName}");
            sourceText.AppendLine("{");
            sourceText.AppendLine($"    public class {SanitizedFileName}Mod : IUserMod");
            sourceText.AppendLine("    {");
            sourceText.AppendLine("        public string Name {");
            sourceText.AppendLine("            get {");
            sourceText.AppendLine($"                return \"{StyleName}\";");
            sourceText.AppendLine("            }");
            sourceText.AppendLine("        }");
            sourceText.AppendLine("        public string Description {");
            sourceText.AppendLine("            get {");
            sourceText.AppendLine("                return \"Lumina Style for use with Lumina\";");
            sourceText.AppendLine("            }");
            sourceText.AppendLine("        }");
            sourceText.AppendLine("    }");
            sourceText.AppendLine("}");
            string code = sourceText.ToString();

            try
            {
                File.WriteAllText(Path.Combine(sourceDirectory, sanitizedName + ".cs"), code);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception writing Lumina style source code for Lumina style ", StyleName);
            }

            // Force manual compilation of source at controlled time.
            // Only specify ICities.dll as additional assembly to avoid 'file not found' errors with m_additionalAssembly empty strings.
            if (Application.platform != RuntimePlatform.OSXPlayer)
            {
                PluginManager.CompileSourceInFolder(sourceDirectory, DirectoryPath, new string[] { typeof(ICities.IUserMod).Assembly.Location });
            }

            // Delete source once we've compiled; this prevents redundant (and error-prone) re-compiliation attempts on future game loads.
            try
            {
                Directory.Delete(sourceDirectory, true);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deleting source code for Lumina style ", StyleName);
            }

        }
    }
}
