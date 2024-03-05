/*
 * Written by algernon.
 * Based on code used in the Dynamic Resolution project, available at:
 * https://github.com/d235j/Skylines-DynamicResolution/tree/cc7d04df204b74c1ba781dc3a5f492ba30ce6b61
  */

namespace Lumina
{
    using System;
    using System.IO;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.UI;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Custom camera renderer for dynamic resolution.
    /// </summary>
    public class DynamicResolutionCamera : MonoBehaviour
    {
        // Shader names.
        private const string downsampleAssetName = "downsampleShader.shader";
        private const string downsampleX2AssetName = "downsampleX2Shader.shader";

        // Cached references.
        private static Camera s_undergroundCamera;
        private static CameraController s_cameraController;
        private static UndergroundView s_undergroundView;

        // Reflection data for private fields.
        private static FieldInfo s_undergroundRGBDField;
        private static FieldInfo s_freeCameraField;

        // Rendering settings.
        private static float s_aliasingFactor = 1f;
        private static int s_renderWidth;
        private static int s_renderHeight;

        // Harmony patching.
        MethodInfo _undergroundViewTarget;
        MethodInfo _undergroundViewPatch;
        MethodInfo _freeCameraTarget;
        MethodInfo _freeCameraPatch;

        // Reference items.
        private static Camera _mainCamera;
        private readonly Rect _unitRect = new Rect(0f, 0f, 1f, 1f);

        // Dynamic resolution rendering.
        private Camera _dynamicResolutionCamera;
        private Material _downsampleMaterial;
        private Material _downsampleX2Material;
        private RenderTexture _fullResolutionRenderTexture;
        private RenderTexture _halfResolutionRenderTexture;

        /// <summary>
        /// Gets or sets the active main camera reference.
        /// </summary>
        public static Camera MainCamera
        {
            get => _mainCamera;
            set => _mainCamera = value;
        }

        /// <summary>
        /// Gets or sets the active anti-aliasing factor.
        /// </summary>
        public static float AliasingFactor
        {
            get => s_aliasingFactor;

            set
            {
                s_aliasingFactor = value;

                // Update render width and height according to new scale.
                s_renderWidth = (int)(Screen.width * value);
                s_renderHeight = (int)(Screen.height * value);
            }
        }

        /// <summary>
        /// Gets or sets the full-resolution rendering texture.
        /// </summary>
        public RenderTexture FullResolutionRenderTexture
        {
            get => _fullResolutionRenderTexture;
            set => _fullResolutionRenderTexture = value;
        }

        /// <summary>
        /// Gets or sets the half-resolution rendering texture.
        /// </summary>
        public RenderTexture HalfResolutionRenderTexture
        {
            get => _halfResolutionRenderTexture;
            set => _halfResolutionRenderTexture = value;
        }

        /// <summary>
        /// Called by Unity every update.
        /// Used to update the dynamic resolution camera values to mirror the main game camera.
        /// </summary>
        public void Update()
        {
            _dynamicResolutionCamera.fieldOfView = MainCamera.fieldOfView;
            _dynamicResolutionCamera.nearClipPlane = MainCamera.nearClipPlane;
            _dynamicResolutionCamera.farClipPlane = MainCamera.farClipPlane;
            _dynamicResolutionCamera.transform.position = MainCamera.transform.position;
            _dynamicResolutionCamera.transform.rotation = MainCamera.transform.rotation;
            _dynamicResolutionCamera.rect = MainCamera.rect;
        }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Loads shaders and applies Harmony patches for Dynamic Resolution.
        /// </summary>
        public void Awake()
        {
            // Grab camera and view instances.
            _dynamicResolutionCamera = GetComponent<Camera>();
            s_cameraController = FindObjectOfType<CameraController>();
            s_undergroundView = FindObjectOfType<UndergroundView>();

            // Load shaders.
            LoadShaders();

            // Reflect fields.
            s_undergroundRGBDField = AccessTools.Field(typeof(UndergroundView), "m_undergroundRGBD"); ;
            s_freeCameraField = AccessTools.Field(typeof(CameraController), "m_cachedFreeCamera");
            s_undergroundCamera = AccessTools.Field(typeof(UndergroundView), "m_undergroundCamera").GetValue(s_undergroundView) as Camera;

            // Apply Harmony patches.
            Harmony harmony = new Harmony(LuminaMod.Instance.HarmonyID);
            _undergroundViewTarget = AccessTools.Method(typeof(UndergroundView), "LateUpdate") ?? AccessTools.Method(typeof(UndergroundView), "FpsBoosterLateUpdate");
            _undergroundViewPatch = AccessTools.Method(typeof(DynamicResolutionCamera), nameof(UndergroundViewDynamicResolution));

            if (_undergroundViewTarget != null && _undergroundViewPatch != null)
            {
                harmony.Patch(_undergroundViewTarget, prefix: new HarmonyMethod(_undergroundViewPatch));
            }
            else
            {
                Logging.Error("Underground view dynamic resolution patch method(s) not found: dyanamic resolution won't be available for underground mode.");
            }

            _freeCameraTarget = AccessTools.Method(typeof(CameraController), "UpdateFreeCamera");
            _freeCameraPatch = AccessTools.Method(typeof(DynamicResolutionCamera), nameof(FreeCameraDynamicResolution));

            if (_freeCameraTarget != null && _freeCameraPatch != null)
            {
                harmony.Patch(_freeCameraTarget, prefix: new HarmonyMethod(_freeCameraPatch));
            }
            else
            {
                Logging.Error("Free camera dynamic resolution patch method(s) not found: dyanamic resolution won't be available for free camera.");
            }
        }

        /// <summary>
        /// Called by Unity when image rendering occurs.
        /// </summary>
        /// <param name="source">Source texture (unused).</param>
        /// <param name="destination">Destination texture (adjusted for dynamic resolution).</param>
        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_fullResolutionRenderTexture == null)
            {
                return;
            }

            // Apply rectangle offset to main camera render.
            Rect activeRect = MainCamera.rect;
            MainCamera.rect = _unitRect;
            MainCamera.targetTexture = _fullResolutionRenderTexture;
            MainCamera.Render();
            MainCamera.targetTexture = null;
            MainCamera.rect = activeRect;

            // Apply aliasing downsampling.
            if (s_aliasingFactor != 1.0f && _halfResolutionRenderTexture)
            {
                // Select appropriate shader.
                Material shader = s_aliasingFactor <= 2.0f ? _downsampleX2Material : _downsampleMaterial;

                // Downsample X resolution.
                _downsampleMaterial.SetVector("_ResampleOffset", new Vector4(_fullResolutionRenderTexture.texelSize.x, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(_fullResolutionRenderTexture, _halfResolutionRenderTexture, shader);

                // Downsample Y resolution.
                _downsampleMaterial.SetVector("_ResampleOffset", new Vector4(0.0f, _fullResolutionRenderTexture.texelSize.y, 0.0f, 0.0f));
                Graphics.Blit(_halfResolutionRenderTexture, destination, shader);
            }
            else
            {
                // No downsampling required (aliasing is 1) - just do direct copy.
                Graphics.Blit(_fullResolutionRenderTexture, destination);
            }
        }

        /// <summary>
        /// Called by Unity when the object is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            // Remove Harmony patches.
            Harmony harmony = new Harmony(LuminaMod.Instance.HarmonyID);
            if (_undergroundViewTarget != null && _undergroundViewPatch != null)
            {
                harmony.Unpatch(_undergroundViewTarget, _undergroundViewPatch);
            }

            if (_freeCameraTarget != null && _freeCameraPatch != null)
            {
                harmony.Unpatch(_freeCameraTarget, _freeCameraPatch);
            }
        }

        /// <summary>
        /// Loads shaders from files.
        /// </summary>
        /// <exception cref="PlatformNotSupportedException">Current runtime platform is not supported.</exception>
        /// <exception cref="FileNotFoundException">Shader asset bundle file wasn't found or was unreadable.</exception>
        /// <exception cref="NullReferenceException">A required shader was missing or invalid, or a material was unable to be created using it.</exception>
        private void LoadShaders()
        {
            // Determine shader file for this platform.
            string shaderFileName;
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                    shaderFileName = "/dynamicresolutionshaders_windows";
                    break;
                case RuntimePlatform.OSXPlayer:
                    shaderFileName = "/dynamicresolutionshaders_mac";
                    break;
                case RuntimePlatform.LinuxPlayer:
                    shaderFileName = "/dynamicresolutionshaders_linux";
                    break;
                default:
                    throw new PlatformNotSupportedException($"[LUMINA] Unsupported runtime platform: Dynamic Resolution shaders are not supported for platform {Application.platform}");
            }

            // Load shader asset bundle.
            // Replace backslashes in path with forward slashes for the asset URI.
            string shaderAssetURI = $"file:///{AssemblyUtils.AssemblyPath.Replace("\\", "/")}{shaderFileName}";
            AssetBundle shaderAssetBundle = new WWW(shaderAssetURI)?.assetBundle;
            if (!shaderAssetBundle)
            {
                throw new FileNotFoundException($"Shader asset bundle ${shaderAssetURI} does not exist or was not readable.");
            }

            // Extract shaders from bundle.
            _downsampleMaterial = LoadShader(shaderAssetBundle, downsampleAssetName);
            _downsampleX2Material = LoadShader(shaderAssetBundle, downsampleX2AssetName);

            // Cleanup.
            shaderAssetBundle.Unload(false);

            // Check shader loading.
            if (!_downsampleMaterial || !_downsampleX2Material)
            {
                throw new NullReferenceException("Failed to create material for one or more Dynamic Resolution shaders");
            }
        }

        /// <summary>
        /// Loads a shader from a Unity asset bundle and creates a new material from it.
        /// </summary>
        /// <param name="shaderAssetBundle">Asset bundle containing shader.</param>
        /// <param name="shaderName">Shader asset name.</param>
        /// <returns>New material created from shader, or <c>null</c> if a material couldn't be created (including if the shader wasn't found or is unsupported).</returns>
        private Material LoadShader(AssetBundle shaderAssetBundle, string shaderName)
        {
            // Load shader from bundle.
            Shader shader = shaderAssetBundle.LoadAsset(shaderName) as Shader;
            if (!shader)
            {

                Logging.Error($"shader ${shaderName} is missing or invalid");
                return null;
            }

            if (!shader.isSupported)
            {
                Logging.Error($"shader ${shaderName} is not supported");
                return null;
            }

            return new Material(shader);
        }

        /// <summary>
        /// Pre-emptive Harmony prefix to <see cref="UndergroundView"/><c>.UndergroundViewLateUpdate</c> to implement dynamic resolution scaling for underground camera mode.
        /// </summary>
        private static bool UndergroundViewDynamicResolution()
        {
            RenderTexture undergroundRenderTexture = s_undergroundRGBDField.GetValue(s_undergroundView) as RenderTexture;

            if (undergroundRenderTexture)
            {
                RenderTexture.ReleaseTemporary(undergroundRenderTexture);
                s_undergroundRGBDField.SetValue(s_undergroundView, null);
            }

            if (s_undergroundCamera != null && MainCamera != null)
            {
                if (s_undergroundCamera.cullingMask != 0)
                {
                    // Replaces screen.width and height with dynamic resolution.
                    undergroundRenderTexture = RenderTexture.GetTemporary(s_renderWidth, s_renderHeight, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

                    s_undergroundCamera.fieldOfView = MainCamera.fieldOfView;
                    s_undergroundCamera.nearClipPlane = MainCamera.nearClipPlane;
                    s_undergroundCamera.farClipPlane = MainCamera.farClipPlane;
                    s_undergroundCamera.rect = MainCamera.rect;
                    s_undergroundCamera.targetTexture = undergroundRenderTexture;
                    s_undergroundCamera.enabled = true;

                    // Write back the render texture.
                    s_undergroundRGBDField.SetValue(s_undergroundView, undergroundRenderTexture);
                }
                else
                {
                    s_undergroundCamera.enabled = false;
                }
            }

            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix to <see cref="CameraController"/><c>.UpdateFreeCamera</c> to implement dynamic resolution scaling for free camera mode.
        /// </summary>
        private static bool FreeCameraDynamicResolution()
        {
            bool m_cachedFreeCamera = (bool)s_freeCameraField.GetValue(s_cameraController);

            if (s_cameraController.m_freeCamera != m_cachedFreeCamera)
            {
                m_cachedFreeCamera = s_cameraController.m_freeCamera;
                s_freeCameraField.SetValue(s_cameraController, m_cachedFreeCamera);

                UIView.Show(UIView.HasModalInput() || !m_cachedFreeCamera);
                Singleton<NotificationManager>.instance.NotificationsVisible = !m_cachedFreeCamera;
                Singleton<GameAreaManager>.instance.BordersVisible = !m_cachedFreeCamera;
                Singleton<DistrictManager>.instance.NamesVisible = !m_cachedFreeCamera;
                Singleton<PropManager>.instance.MarkersVisible = !m_cachedFreeCamera;
                Singleton<GuideManager>.instance.TutorialDisabled = m_cachedFreeCamera;
                Singleton<DisasterManager>.instance.MarkersVisible = !m_cachedFreeCamera;
                Singleton<NetManager>.instance.RoadNamesVisible = !m_cachedFreeCamera;
            }

            FieldInfo cameraField = AccessTools.Field(typeof(CameraController), "m_camera");
            Camera m_camera = cameraField.GetValue(s_cameraController) as Camera;
            m_camera.rect = new Rect(0f, 0f, 1f, 1f);

            return false;
        }
    }
}
