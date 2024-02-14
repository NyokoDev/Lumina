using System;
using UnityEngine;
using UnityEngine.PostProcessing;
using static UnityEngine.PostProcessing.AmbientOcclusionModel;

namespace Lumina.Shaders.AO
{
    internal class AO : MonoBehaviour
    {
        private Camera _camera = null;
        private PostProcessingBehaviour _postProcessingBehaviour = null;
        public AmbientOcclusionModel _ambientOcclusionModel = null;

        public bool Initialized = false;

        /// <summary>
        /// Initialize PostProcessing profile.
        /// </summary>
        public void Start()
        {
            try
            {
                if (_camera == null)
                {
                    _camera = Camera.main;
                    if (_camera == null)
                    {
                        throw new Exception("No camera found in the scene.");
                    }
                }

                AssetManager.Instance.AssetBundle = AssetBundleUtils.LoadAssetBundle("renderit");

                if (_postProcessingBehaviour == null)
                {
                    _postProcessingBehaviour = _camera.gameObject.GetComponent<PostProcessingBehaviour>();
                    if (_postProcessingBehaviour == null)
                    {
                        _postProcessingBehaviour = _camera.gameObject.AddComponent<PostProcessingBehaviour>();
                        _postProcessingBehaviour.profile = ScriptableObject.CreateInstance<PostProcessingProfile>();
                    }
                }

                if (_postProcessingBehaviour == null || _postProcessingBehaviour.profile == null)
                {
                    throw new Exception("PostProcessingBehaviour or its profile is null.");
                }

                _postProcessingBehaviour.enabled = true;

                if (_ambientOcclusionModel == null)
                {
                    _ambientOcclusionModel = new AmbientOcclusionModel();
                }

                // Setting Ambient Occlusion settings
                _postProcessingBehaviour.profile.ambientOcclusion.enabled = true;
                _ambientOcclusionModel.enabled = true;
                AmbientOcclusionModel.Settings settings = _ambientOcclusionModel.settings;

                // Adjusting Ambient Occlusion intensity and radius
                settings.intensity = 4f;
                settings.radius = 0.3f;
                settings.sampleCount = SampleCount.Medium;
                settings.downsampling = true;
                settings.forceForwardCompatibility = false;
                settings.ambientOnly = false;
                settings.highPrecision = false;

                // Applying modified settings
                _postProcessingBehaviour.profile.ambientOcclusion.settings = settings;

                Initialized = true;
                Debug.Log("[LUMINA] Successfully loaded AO Shader.");
                Debug.Log("[LUMINA] Ambient Occlusion enabled.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred in Start(): {ex.Message}");
            }
        }

        public void Update()
        {
            try
            {
                if (Initialized)
                UpdateAmbientOcclusion();
            }
            catch (Exception e)
            {
                Debug.Log("[LUMINA] ModManager:Update -> Exception: " + e.Message);
            }
        }

        private void UpdateAmbientOcclusion()
        {
            try
            {
                AmbientOcclusionModel.Settings settings = _ambientOcclusionModel.settings;

                settings.intensity = LuminaLogic.AOIntensity;
                settings.radius = LuminaLogic.AORadius;
                _ambientOcclusionModel.settings = settings;
                _ambientOcclusionModel.enabled = true;

                _postProcessingBehaviour.profile.ambientOcclusion.settings = _ambientOcclusionModel.settings;
                _postProcessingBehaviour.profile.ambientOcclusion.enabled = _ambientOcclusionModel.enabled;
            }
            catch (Exception e)
            {
                Debug.Log("[LUMINA] ModManager:UpdateAmbientOcclusion -> Exception: " + e.Message);
            }
        }
    }
}
