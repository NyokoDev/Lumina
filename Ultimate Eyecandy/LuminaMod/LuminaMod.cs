namespace Lumina
{
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ICities;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class LuminaMod : PatcherMod<OptionsPanel, PatcherBase>, IUserMod
    {
        /// <summary>
        /// Gets the mod's base display name (name only).
        /// </summary>
        public override string BaseName => "Lumina";

        /// <summary>
        /// Gets the mod's unique Harmony identfier.
        /// </summary>
        public override string HarmonyID => "com.nyoko.lumina.patch";

        /// <summary>
        /// Gets the mod's description for display in the content manager.
        /// </summary>
        public string Description => LuminaTR.TranslationFramework.Translation.Instance.GetTranslation(LuminaTR.Locale.TranslationID.MOD_DESCRIPTION);

        /// <summary>
        /// Saves settings file.
        /// </summary>
        public override void SaveSettings()
        {
        }

        /// <summary>
        /// Loads settings file.
        /// </summary>
        public override void LoadSettings()
        {
            // Enable detailed logging.
            Logging.DetailLogging = true;
        }
    }
}