/*
 * Written by algernon.
 * Based on code used in the Dynamic Resolution project, available at:
 * https://github.com/d235j/Skylines-DynamicResolution/tree/cc7d04df204b74c1ba781dc3a5f492ba30ce6b61
  */

namespace Lumina
{
    using AlgernonCommons;
    using UnityEngine;

    /// <summary>
    /// Manager class for Dynamic Resolution.
    /// </summary>
    internal class DynamicResolutionManager
    {
        // Dynamic resolution components.
        private GameObject _gameObject;
        private DynamicResolutionCamera _dynamicResolutionCamera;
        private RenderTexture _renderTexture;

        // Cached references.
        private CameraController _cameraController;
        private Camera _mainCamera;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicResolutionManager"/> class.
        /// </summary>
        internal DynamicResolutionManager()
        {
            // Set instance.
            Instance = this;

            // Disable main camera.
            Camera mainCamera = MainCamera;
            mainCamera.enabled = false;

            // Generate rendering texture.
            float ssaaFactor = DynamicResolutionCamera.AliasingFactor;
            float width = Screen.width * ssaaFactor;
            float height = Screen.height * ssaaFactor;
            _renderTexture = new RenderTexture((int)width, (int)height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

            // Create camera game object and attach dummy main camera.
            _gameObject = new GameObject("Lumina dynamic resolution");
            Camera dummyCamera = _gameObject.AddComponent<Camera>();
            dummyCamera.cullingMask = 0;
            dummyCamera.depth = -3;
            dummyCamera.tag = "MainCamera";
            dummyCamera.pixelRect = mainCamera.pixelRect;

            // Disable in-game AA.
            SetInGameAA(false);

            // Set up dynamic resolution camera.
            _dynamicResolutionCamera = _gameObject.AddComponent<DynamicResolutionCamera>();
            _dynamicResolutionCamera.FullResolutionRenderTexture = _renderTexture;
            _dynamicResolutionCamera.HalfResolutionRenderTexture = new RenderTexture(Screen.width, (int)height, 0);
            DynamicResolutionCamera.MainCamera = mainCamera;
            DynamicResolutionCamera.MainCamera.targetTexture = null;
            DynamicResolutionCamera.MainCamera.pixelRect = mainCamera.pixelRect;
        }

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        internal static DynamicResolutionManager Instance { get; private set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether the resolution slider should be unlocked.
        /// </summary>
        internal static bool UnlockSlider { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether VRAM usage should be reduced by eliminating the half-height render texture.
        /// </summary>
        internal static bool LowerVRAMUsage { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum limit for the dynamic resolution slider.
        /// </summary>
        internal static float MaximumDRValue { get; set; } = 4f;

        /// <summary>
        /// Gets the CameraController reference.
        /// </summary>
        private CameraController Controller
        {
            get
            {
                // Get camera controller if we haven't already, and main camera has been instantiated.
                if (_cameraController == null)
                {
                    GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                    if (mainCamera)
                    {
                        _cameraController = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<CameraController>();
                    }
                }

                return _cameraController;
            }
        }

        /// <summary>
        /// Gets the main camera reference.
        /// </summary>
        private Camera MainCamera
        {
            get
            {
                // Get main camera if we haven't already.
                if (_mainCamera == null)
                {
                    _mainCamera = Controller.GetComponent<Camera>();
                }

                return _mainCamera;
            }
        }

        /// <summary>
        /// Cleanly disposes of the manager.
        /// </summary>
        internal static void Destroy()
        {
            if (Instance != null)
            {
                // Re-enable main camera.
                if (Instance.MainCamera is Camera mainCamera)
                {
                    mainCamera.enabled = true;
                }

                // Destroy dynamic resolution GameObject.
                GameObject.Destroy(Instance._gameObject);

                // Nullify instance reference.
                Instance = null;
            }
        }

        /// <summary>
        /// Sets the in-game anti-aliasing state.
        /// </summary>
        /// <param name="state"><c>true</c> to enable in-game anti-aliasing, <c>false</c> to disable.</param>
        private void SetInGameAA(bool state)
        {
            // Toggling is via creating or destroying the SMAA component on the camera.
            Camera camera = _gameObject.GetComponent<Camera>();
            if (!state)
            {
                if (camera.GetComponent<SMAA>())
                {
                    GameObject.Destroy(camera.gameObject.GetComponent<SMAA>());
                }
            }
            else
            {
                if (!camera.GetComponent<SMAA>())
                {
                    camera.gameObject.AddComponent<SMAA>();
                }
            }
        }

        /// <summary>
        /// Sets the dynamic resolution anti-aliasing factor.
        /// </summary>
        /// <param name="factor">New factor to set.</param>
        internal void SetSSAAFactor(float factor)
        {
            // Dsetroy existing texture and create a new one to the desired dimensions.
            int newWidth = (int)(Screen.width * factor);
            int newHeight = (int)(Screen.height * factor);
            GameObject.Destroy(_renderTexture);
            _renderTexture = new RenderTexture(newWidth, newHeight, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

            // Update stored scaling value.
            DynamicResolutionCamera.AliasingFactor = factor;

            // Update camera.
            if (_gameObject.GetComponent<DynamicResolutionCamera>() is DynamicResolutionCamera dynamicResolutionCamera)
            {
                // Set full resolution texture.
                dynamicResolutionCamera.FullResolutionRenderTexture = _renderTexture;

                // Destroy existing half-resolution texture.
                if (dynamicResolutionCamera.HalfResolutionRenderTexture is RenderTexture halfResTexture)
                {
                    GameObject.Destroy(halfResTexture);
                }

                // Update half-resolution texture.
                if (!LowerVRAMUsage)
                {
                    dynamicResolutionCamera.HalfResolutionRenderTexture = new RenderTexture(Screen.width, (int)newHeight, 0);
                }
                else
                {
                    // Nullify texture (and don't create new one) if 'lower VRAM usage' option is enabled.
                    dynamicResolutionCamera.HalfResolutionRenderTexture = null;
                }

                // Apply new texture.
                GameObject.Destroy(DynamicResolutionCamera.MainCamera.targetTexture);
                DynamicResolutionCamera.MainCamera.targetTexture = _renderTexture;
            }
        }
    }
}


