namespace Lumina
{
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using DigitalRuby.RainMaker;
    using ICities;
    using Lumina.Bundles;
    using Lumina.Helpers;
    using System;
    using System.Collections.Generic;
    using UnityEngine;


    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {
        private static DynamicResolutionManager s_dynamicResolutionManager = null;
        private GameObject _gameObject;
        private GameObject _TimeManagerGameObject;

        private GameObject rainInstance;
        private AssetBundle rainBundle;

        /// <summary>
        /// Gets the active dynamic resolution manager.
        /// </summary>
        internal static DynamicResolutionManager ActiveDRManager => s_dynamicResolutionManager;

        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor, AppMode.AssetEditor, AppMode.ThemeEditor, AppMode.ScenarioEditor };

        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            // Disable dynamic resolution, if it was enabled.
            if (s_dynamicResolutionManager != null)
            {
                DynamicResolutionManager.Destroy();
                s_dynamicResolutionManager = null;
            }

            // Destroy any existing Lumina logic.
            LuminaLogic.Destroy();
        }

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Create logic instance.
            LuminaLogic.OnLoad();
            Logger.Log("On load called.");

            // Enable dynamic resolution.
            EnableDynamicResolution();
            RainPlugin();
            CheckForModConflicts();
            AttachTimeManager();

            // Initialize cubemaps.
            CubemapManager.Initialize();

            _gameObject = new GameObject("CubemapReplacerRedux");
            _gameObject.AddComponent<CubemapUpdater>();
        }

        private void RainPlugin()
        {
            rainInstance = new GameObject("LuminaRain");
            var loader = rainInstance.AddComponent<RainmakerBundleLoader>();

        }

        private void AttachTimeManager()
        {
            // Initialize the GameObject if it hasn't been already
            if (_TimeManagerGameObject == null)
            {
                _TimeManagerGameObject = new GameObject("Lumina's TimeManager");
            }

            // Attach the TimeManager component to the GameObject
            _TimeManagerGameObject.AddComponent<TimeManager>();
        }

        private void EnableDynamicResolution()
        {
            // Enable dynamic resolution.
            if (LuminaLogic.DynResEnabled)
            {
                s_dynamicResolutionManager = new DynamicResolutionManager();
            }
            else
            {
                Logging.Message("Dynamic Resolution disabled");

            }
        }

        /// <summary>
        /// Checks for Mod Conflicts and logs them.
        /// </summary>
        private void CheckForModConflicts()
        {
            CompatibilityAssistant.CheckAll();
        }
    }
}