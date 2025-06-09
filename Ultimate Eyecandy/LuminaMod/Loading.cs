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

            // Dispose of created GameObjects to prevent memory leaks.
            if (_gameObject != null)
            {
                UnityEngine.Object.Destroy(_gameObject);
                _gameObject = null;
            }

            if (_TimeManagerGameObject != null)
            {
                UnityEngine.Object.Destroy(_TimeManagerGameObject);
                _TimeManagerGameObject = null;
            }

            if (rainInstance != null)
            {
                UnityEngine.Object.Destroy(rainInstance);
                rainInstance = null;
            }

            if (rainBundle != null)
            {
                rainBundle.Unload(true);
                rainBundle = null;
            }
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
            try
            {
                if (LuminaLogic.DynResEnabled)
                {
                    if (s_dynamicResolutionManager != null)
                    {
                        Logger.Log("Dynamic Resolution already enabled, destroying previous instance.");
                        DynamicResolutionManager.Destroy();
                        s_dynamicResolutionManager = null;
                    }

                    s_dynamicResolutionManager = new DynamicResolutionManager();
                    Logger.Log("Dynamic Resolution enabled.");
                }
                else
                {
                    if (s_dynamicResolutionManager != null)
                    {
                        Logger.Log("Dynamic Resolution disabled, destroying existing instance.");
                        DynamicResolutionManager.Destroy();
                        s_dynamicResolutionManager = null;
                    }
                    else
                    {
                        Logger.Log("Dynamic Resolution disabled.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to enable Dynamic Resolution: " + ex);
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