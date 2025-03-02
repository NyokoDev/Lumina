namespace Lumina
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using UnityEngine;


    /// <summary>
    /// The Lumina UI panel.
    /// </summary>
    public sealed class LuminaPanel : StandalonePanel
    {
        /// <summary>
        /// Width of panel content.
        /// </summary>
        internal const float ContentWidth = 500f;

        // Layout constants - private.
        private const float TitleHeight = 40f;
        private const float ContainerHeight = 930f;

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public override float PanelWidth => ContentWidth + (Margin * 2f);

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public override float PanelHeight => TitleHeight + ContainerHeight;

        /// <summary>
        /// Gets the panel's title.
        /// </summary>
        protected override string PanelTitle => Translations.Translate(LuminaTR.TranslationID.MOD_NAME);

  

        Loading Loading;
        /// <summary>
        /// Gets the panel opacity.
        /// </summary>
        protected override float PanelOpacity => 1f;

        /// <summary>
        /// Called by Unity before first display.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Add tabstrip.
            AutoTabstrip tabStrip = AutoTabstrip.AddTabstrip(this, Margin, TitleHeight, ContentWidth, ContainerHeight, out _);

           
                // Add tabs and panels.
                new LightingTab(tabStrip, 0);
                new StylesTab(tabStrip, 1);
                new EffectsTab(tabStrip, 2);

                if (ModUtils.IsModEnabled("lutcreator"))
                {
                    Logger.Log("[LUMINA] LUT Creator plugin enabled.");
                    new LookUpTableTab(tabStrip, 3);
                }

                SetIcon(UITextures.LoadSprite("ADV"), "normal");
                //handler.LoadSettings();

                // Force initial tab selection.
                tabStrip.selectedIndex = -1;
                tabStrip.selectedIndex = 0;
            }

        

        /// <summary>
        /// Performs any actions required before closing the panel and checks that it's safe to do so.
        /// </summary>
        /// <returns>Always true.</returns>
        protected override bool PreClose()
        {
            // Deselect UUI button.
            LuminaLogic.Instance?.ResetButton();
            LuminaLogic.Instance.UUIButton.IsPressed = false;

            // Save settings.
            ModSettings.Save();

            // Always okay to close.
            return true;
        }
    }
}