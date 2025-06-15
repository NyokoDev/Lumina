namespace Lumina
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using System;
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

            this.backgroundSprite = LuminaLogic.BackgroundStyle;
            Logger.Log("[LuminaPanel] Start: Initializing tabstrip...");
            AutoTabstrip tabStrip = null;
            try
            {
                tabStrip = AutoTabstrip.AddTabstrip(this, Margin, TitleHeight, ContentWidth, ContainerHeight, out _);
                if (tabStrip == null)
                {
                    Logger.Log("[LuminaPanel] ERROR: tabStrip is null after AddTabstrip.");
                    return;
                }
                Logger.Log("[LuminaPanel] Tabstrip created successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"[LuminaPanel] ERROR: Exception while creating tabstrip: {ex}");
                return;
            }

            try
            {
                Logger.Log("[LuminaPanel] Creating LightingTab...");
                new LightingTab(tabStrip, 0);
                Logger.Log("[LuminaPanel] Creating StylesTab...");
                new StylesTab(tabStrip, 1);
                Logger.Log("[LuminaPanel] Creating EffectsTab...");
                new EffectsTab(tabStrip, 2);
            }
            catch (Exception ex)
            {
                Logger.Log($"[LuminaPanel] ERROR: Exception while creating tabs: {ex}");
                return;
            }

            try
            {
                if (ModUtils.IsModEnabled("lutcreator"))
                {
                    Logger.Log("[LuminaPanel] LUT Creator plugin enabled.");
                    new LookUpTableTab(tabStrip, 3);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[LuminaPanel] ERROR: Exception in LUT Creator check: {ex}");
            }

            try
            {
                Logger.Log("[LuminaPanel] Loading icon sprite...");
                var sprite = UITextures.LoadSprite("ADV");
                if (sprite == null)
                {
                    Logger.Log("[LuminaPanel] WARNING: Sprite 'ADV' not found.");
                }
                else
                {
                    SetIcon(sprite, "normal");
                    Logger.Log("[LuminaPanel] Icon set successfully.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[LuminaPanel] ERROR: Exception while setting icon: {ex}");
            }

            try
            {
                Logger.Log("[LuminaPanel] Setting tabStrip.selectedIndex = 0");
                tabStrip.selectedIndex = -1;
                Logger.Log("[LuminaPanel] Tabstrip selectedIndex set to 0.");
            }
            catch (Exception ex)
            {
                Logger.Log($"[LuminaPanel] ERROR: Exception while setting selectedIndex: {ex}");
            }
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