namespace Lumina
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using ColossalFramework.Plugins;
    using Epic.OnlineServices.TitleStorage;
    using UnityEngine;

    /// <summary>
    /// Handling of styles.
    /// </summary>
    public sealed class LuminaStyle
    {
        private string DLLPath;
        private string kDLL = ".dll";

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

                    DirectoryPath = Path.Combine(localModPath, "Lumina_" + sanitizedName);

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
        /// Creates dummy mod source code for the style.
        /// </summary>
        /// <param name="sanitizedName">Sanitized style name.</param>
        public void CreateSourceCode(string sanitizedName)
        {
            StringBuilder sourceText = new StringBuilder();

            // Check if the main DirectoryPath exists
            if (!Directory.Exists(DirectoryPath))
            {
                Logger.Log($"Attempting to create Lumina style mod with an invalid directory. DirectoryPath: {DirectoryPath} does not exist.");
                return;
            }

            // Create the source directory path
            string sourceDirectory = Path.Combine(DirectoryPath, "Source");
            Logger.Log($"Creating source directory at: {sourceDirectory}");

            // Ensure the source directory exists or attempt to create it
            if (!Directory.Exists(sourceDirectory))
            {
                try
                {
                    Directory.CreateDirectory(sourceDirectory);
                    Logger.Log($"Successfully created source directory: {sourceDirectory}");
                }
                catch (Exception e)
                {
                    Logging.LogException(e, $"Exception occurred while creating the source directory {sourceDirectory} for Lumina style {StyleName}.");
                    return;
                }
            }
            else
            {
                Logger.Log($"Source directory already exists: {sourceDirectory}");
            }

            // Build the source code text
            Logger.Log($"Building source code for: {sanitizedName}");
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

            // Try writing the source code to a file
            try
            {
                string sourceFilePath = Path.Combine(sourceDirectory, sanitizedName + ".cs");
                File.WriteAllText(sourceFilePath, code);
                Logger.Log($"Source code successfully written to {sourceFilePath}");
            }
            catch (Exception e)
            {
                Logging.LogException(e, $"Exception occurred while writing the source code for Lumina style {StyleName}");
                return;
            }

            // Force manual compilation of source
            if (Application.platform != RuntimePlatform.OSXPlayer)
            {
                Logger.Log("Initiating manual compilation of the source.");
                try
                {
                    try
                    {
                        // Assuming CompileSourceInFolder is synchronous; if not, you need to handle asynchronous completion
                        PluginManager.CompileSourceInFolder(sourceDirectory, DirectoryPath, new string[] { typeof(ICities.IUserMod).Assembly.Location });
                        DLLPath = Path.Combine(sourceDirectory, DirectoryPath + kDLL);
                        Logger.Log(DLLPath);
                        // Check if the DLL file exists before attempting to set attributes
                        if (File.Exists(DLLPath))
                        {
                            // Set the DLL attributes to read-only
                            File.SetAttributes(DLLPath, FileAttributes.ReadOnly);
                            Logger.Log("File attributes set to ReadOnly.");
                        }
                        Logger.Log("Source code compilation successfully completed.");
                    }
                    catch (Exception compileEx)
                    {
                        Logging.LogException(compileEx, "Exception occurred during source code compilation for Lumina style");
                        return;  // Prevent further execution if compilation fails
                    }
                }
                catch (Exception innerEx)
                {
                    Logging.LogException(innerEx, "Unexpected exception during manual compilation initiation.");
                    return;
                }
            }

        
        }
    }
}