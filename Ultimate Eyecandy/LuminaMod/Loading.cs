namespace Lumina
{
    using System.Collections.Generic;
    using AlgernonCommons.Patching;
    using ICities;
    using UnityEngine;
    using Lumina;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using Lumina.Shaders.AO;
    using UnityEngine.PostProcessing;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {
        /// <summary>
        /// Gets a list of permitted loading modes.
        /// </summary>
        /// 
        public ColorCorrectionManager colorCorrectionManager;

        /// <summary>
        /// Initialize Ambient Occlusion instance.
        /// </summary>
       
      
        protected override List<AppMode> PermittedModes => new List<AppMode> { AppMode.Game, AppMode.MapEditor, AppMode.AssetEditor, AppMode.ThemeEditor, AppMode.ScenarioEditor };

        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            // Destroy any existing Lumina logic.
            LuminaLogic.Destroy();
        }

        public static CameraHook hook = null;

        private UnityEngine.GameObject _gameObject;
        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Create logic instance.
            LuminaLogic.OnLoad();
        

            if (LuminaLogic.DynResEnabled)
            {
                var cameraController = GameObject.FindObjectOfType<CameraController>();
                hook = cameraController.gameObject.AddComponent<CameraHook>();
            }
            else
            {
                Debug.Log("[LUMINA] Dynamic Resolution disabled.");
            }
        

            if (ModUtils.IsModEnabled("dynamicresolution")) {

                CompatibilityDR notification = NotificationBase.ShowNotification<CompatibilityDR>();
                notification.AddParas("Dynamic Resolution has been detected. For optimal use of Lumina, turn off Dynamic Resolution since both have identical functions. Failure to deactivate Dynamic Resolution might cause unexpected behavior.");
            }
            if (ModUtils.IsModEnabled("skyboxreplacer"))
            {
                CompatibilityDR notification = NotificationBase.ShowNotification<CompatibilityDR>();
                notification.AddParas("Cubemap Replacer has been detected. For optimal use of Lumina, turn off Cubemap Replacer since both have identical functions. Failure to deactivate Cubemap Replacer might cause unexpected behavior.");
            }

            // Initialize cubemaps.
            CubemapManager.Initialize();

            _gameObject = new UnityEngine.GameObject("CubemapReplacerRedux");
            _gameObject.AddComponent<CubemapUpdater>();
            AO.Instance.Start();
        }
    }
    public class CompatibilityDR : ListNotification
    {
        internal UIButton _yesButton;
        
        /// <summary>
        /// Gets the 'No' button (button 1) instance.
        /// </summary>

        /// <summary>
        /// Gets the 'Yes' button (button 2) instance.
        /// </summary>
        public UIButton YesButton => _yesButton;

        /// <summary>
        /// Gets the number of buttons for this panel (for layout).
        /// </summary>
        protected override int NumButtons => 1;



        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {

            // Add yes button.
            _yesButton = AddButton(1, NumButtons, Translations.Translate("AGREEMENT_TEXT"), Close);

        }
    }


}
