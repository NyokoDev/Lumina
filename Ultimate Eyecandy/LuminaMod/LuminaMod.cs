namespace Lumina
{
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using ICities;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class LuminaMod : PatcherMod<OptionsPanel, PatcherBase>, IUserMod
    {
        /// <summary>
        /// Mod name.
        /// </summary>
        public override string BaseName => "Lumina";

        /// <summary>
        /// Gets the mod's unique Harmony identfier.
        /// </summary>
        public override string HarmonyID => "com.nyoko.lumina.patch";

        /// <summary>
        /// Gets the mod's description for display in the content manager.
        /// </summary>
        public string Description => Translations.Translate(LuminaTR.TranslationID.MOD_DESCRIPTION);

        /// <summary>
        /// Saves settings file.
        /// </summary>
        public override void SaveSettings() => ModSettings.Save();

        /// <summary>
        /// Loads settings file.
        /// </summary>
        public override void LoadSettings()
        {
            ModSettings.Load();

            // Enable detailed logging.
            Logging.DetailLogging = true;
        }
    }
}