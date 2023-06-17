namespace Lumina
{
    using AlgernonCommons.Patching;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {
        private static GameObject s_gameObject;

        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            // Destroy any existing GameObject.
            if (GameObject.Find("Lumina") is GameObject gameObject)
            {
                GameObject.Destroy(gameObject);
                s_gameObject = null;
            }
        }

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Destroy any existing GameObject.
            if (GameObject.Find("Lumina") is GameObject gameObject)
            {
                GameObject.DestroyImmediate(gameObject);
            }
            
            // Create new GameObject.
            s_gameObject = new GameObject("Lumina");
            s_gameObject.AddComponent<LuminaLogic>();
        }
    }
}
